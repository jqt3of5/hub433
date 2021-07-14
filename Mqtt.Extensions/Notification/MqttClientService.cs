using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace mqtt.Notification
{
    public interface IMqttClientService
    {
        public string DeviceId { get; }

        public Task Connect(string clientId, string host, X509Certificate2 certificate, RSA privateKey);
        public Task Connect(string clientId, string host, X509Certificate2 pfxCertificate);
        public Task<bool> Subscribe(string route, MqttClientService.RouteHandler func);
        public Task<bool> Subscribe(string route, MethodInfo method, object instance);
        public Task<bool> Subscribe(string topic); 
        public Task<bool> Subscribe(IMqttApplicationMessageReceivedHandler messageHandler);
        public Task<bool> Unsubscribe(string topic);
        public Task<bool> RegisterHandler(IMqttApplicationMessageReceivedHandler handler);
        
        public Task Publish(string topic, string body);
        public Task<string> PublishWithResult(string topic, string body, CancellationToken token);
    }
    
    public class MqttClientService : IMqttClientService, IMqttApplicationMessageReceivedHandler
    {
        public class NotificationMessage
        {
            public MqttApplicationMessage InternalMessage { get; }
            public string [] PathParams { get; }

            public O GetPayload<O>()
            {
                return JsonSerializer.Deserialize<O>(InternalMessage.ConvertPayloadToString());
            }
        
            public NotificationMessage(MqttApplicationMessage message, string[] pathParams)
            {
                InternalMessage = message;
                PathParams = pathParams;
            }
        }
        
        public delegate object? RouteHandler(NotificationMessage message);
        
        public const string Wildcard = "+";
        
        public readonly TrieNode<(string, RouteHandler)> RouteTrie = new(); 
        
        public string DeviceId { get; private set; }
        
        private readonly IMqttClient _client;

        private List<IMqttApplicationMessageReceivedHandler> _otherHandlers = new();
        
        public static MqttClientService Create()
        {
            var client = new MqttFactory().CreateMqttClient();
            return new(client);
        }
        public MqttClientService(IMqttClient client)
        {
            _client = client;
            _client.ApplicationMessageReceivedHandler = this;
        }

        public async Task Connect(string clientId, string host, X509Certificate2 certificate, RSA privateKey)
        {
            var certWithPrivateKey = certificate.CopyWithPrivateKey(privateKey);
            
            // work around for Windows (WinApi) problems with PEMS, still in .NET 5
            //Found here: https://github.com/dotnet/runtime/issues/23749
            var pfx =  new X509Certificate2(certWithPrivateKey.Export(X509ContentType.Pfx));

            await Connect(clientId, host, pfx);
        }
        
        public async Task Connect(string clientId, string host, X509Certificate2 pfxCertificate)
        {
            if (!pfxCertificate.HasPrivateKey)
            {
                throw new ArgumentException("Certificate should contain private key");
            }
            
            var options = new MqttClientOptionsBuilder()
                            .WithClientId(clientId)
                            .WithTls(new MqttClientOptionsBuilderTlsParameters(){Certificates = new[] {pfxCertificate}, 
                                UseTls = true, 
                                SslProtocol = SslProtocols.Tls12, 
                                AllowUntrustedCertificates = true,
                                IgnoreCertificateChainErrors = true, 
                                IgnoreCertificateRevocationErrors = true})
                            .WithCleanSession() 
                            .WithProtocolVersion(MqttProtocolVersion.V311)
                            //Must be version 500 so that response topics work
                            // .WithProtocolVersion(MqttProtocolVersion.V500)
                            .WithTcpServer(host, 8883)
                            .Build();

            DeviceId = clientId;
            var result = await _client.ConnectAsync(options, new CancellationToken());
        }
        
        public async Task<bool> Subscribe(string route, RouteHandler func)
        {
            //Convert a "route" (routes can have named path parameters) into a "topic" (topics follow Mqtt.Extensions path formatting. Mainly wildcards are always "+")
            var  topic= Regex
                .Replace(route, "\\{.+\\}", Wildcard)
                .Trim('/'); 

            var result = await _client.SubscribeAsync(topic);
         
            RouteTrie.AddValue(topic, (route, func));

            return result.Items.Any(item => item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS0
                                            || item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS1
                                            || item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS2);
        }
        public Task<bool> Subscribe(string route, Func<string[], object> method)
        {
            RouteHandler func = message => method.Invoke(message.GetPayload<string[]>());
            return Subscribe(route, func); 
        }
        
        public Task<bool> Subscribe(string route, MethodInfo method, object instance)
        {
            if (method == null)
            {
                return Task.FromResult(false);
            }

            RouteHandler func = message => InvokeWithMappedParameters(message, method, instance);
            return Subscribe(route, func);
        }

        public async Task<bool> Subscribe(string topic)
        {
            await _client.SubscribeAsync(topic);
            return true;
        }

        public Task<bool> Subscribe(IMqttApplicationMessageReceivedHandler messageHandler)
        {
            _otherHandlers.Add(messageHandler);
            return Task.FromResult(true);
        }

        public Task Publish(string topic, string body)
        {
            return _client.PublishAsync(topic, payload: body);
        }

        public async Task<string?> PublishWithResult(string topic, string body, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<string>();
            var responseTopic = GenerateResponseTopic();
            await Subscribe(responseTopic, (NotificationMessage message) =>
            {
                tcs.SetResult(message.GetPayload<string>());    
                return null;
            });
            
            await _client.PublishAsync(new MqttApplicationMessage()
                {Topic = topic, Payload = Encoding.UTF8.GetBytes(body), ResponseTopic = responseTopic});

            tcs.Task.Wait(token);

            await Unsubscribe(responseTopic);
            
            if (token.IsCancellationRequested)
            {
                return null;
            }

            return tcs.Task.Result;
        }

        private string GenerateResponseTopic()
        {
            return $"{DeviceId}/{Guid.NewGuid()}";
        }

        /// <summary>
        /// Maps the parametesrs of the method to the list of values in the message, then invokes the method
        /// </summary>
        /// <param name="message"></param>
        /// <param name="method"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        private object? InvokeWithMappedParameters(NotificationMessage message, MethodInfo method,
            object instance)
        {
            //TODO: This can be improved in a couple of ways....
            //since this is deserializing as a string array, each element must be a string in the JSON. Then we type convert later
            //If we deserialized as an object array, we would get JsonElements, which we can then convert/type check into the method parameters. 
            //allowing a bit more flexibility in the JSON format
            //OR we can convert to a dictionary and mapped named parameters.  
            var stringArguments = message.GetPayload<string[]>();
            var parameters = method.GetParameters();
            if (parameters.Length > stringArguments.Length)
            {
                //we cannot invoke the method if we don't have enough arguments
                return null;
            }

            List<object> arguments = new();
            for (int i = 0; i < parameters.Length; ++i)
            {
                var argument = TypeDescriptor.GetConverter(parameters[i].ParameterType).ConvertFromString(stringArguments[i]);
                arguments.Add(argument);
            }

            return method.Invoke(instance, arguments.ToArray());
        }

        /// <summary>
        /// WARNING: This method will unsubscribe all local listeners from this topic. 
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task<bool> Unsubscribe(string topic)
        {
            RouteTrie.DeletePathValues(topic);
            var result = await _client.UnsubscribeAsync(topic);
            return result.Items.Any(item => item.ReasonCode != MqttClientUnsubscribeResultCode.Success);
        }

        public Task<bool> RegisterHandler(IMqttApplicationMessageReceivedHandler handler)
        {
            return Task.FromResult(_otherHandlers.Remove(handler));
        }
        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            foreach (var handler in _otherHandlers)
            {
                await handler.HandleApplicationMessageReceivedAsync(eventArgs);
            }
            
            if (RouteTrie.TryGetValue(eventArgs.ApplicationMessage.Topic, out var handlers))
            {
                foreach (var (path, method) in handlers)
                {
                    //Map actual topic to topic descriptor
                    var pathParts = path.Split('/');
                    
                    //Extract the parameters from the topic
                    var subtopic = eventArgs.ApplicationMessage.Topic.Split('/').ToArray();
                    var handlerParams = new List<string>();
                    for (var i = 0; i < pathParts.Length; ++i)
                    {
                        //It's possible for the path to have {} as a part - but because there is no name, we will consider it a throw away wildcard and not match it
                        if (Regex.IsMatch(pathParts[i], "\\{.+\\}"))//Don't pass if the wild card is specified || pathParts[i] == Wildcard)
                        {
                           handlerParams.Add(subtopic[i]); 
                        }
                    }
    
                    try
                    {
                        var message = new NotificationMessage(eventArgs.ApplicationMessage, handlerParams.ToArray());
                        var response = method.Invoke(message);
                        if (!string.IsNullOrEmpty(eventArgs.ApplicationMessage.ResponseTopic))
                        {
                            await _client.PublishAsync(eventArgs.ApplicationMessage.ResponseTopic, JsonSerializer.Serialize(response));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error while handling Mqtt topic: {e}");
                        if (!string.IsNullOrEmpty(eventArgs.ApplicationMessage.ResponseTopic))
                        {
                            await _client.PublishAsync(eventArgs.ApplicationMessage.ResponseTopic, JsonSerializer.Serialize(e));
                        }
                    }
                }
            }
        }
    }
}