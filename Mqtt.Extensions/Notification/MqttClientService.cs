using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;

namespace mqtt.Notification
{
    public interface IMqttClientService
    {
        public string DeviceId { get; }
        public Task Connect(string host);
        public Task<bool> Subscribe(string route, MqttClientService.RouteHandler func);
        public Task<bool> Subscribe(string route, MethodInfo method, object instance);
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
        
        public string DeviceId { get; }
        
        private readonly IMqttClient _client;

        public static MqttClientService Create(string deviceId)
        {
            var client = new MqttFactory().CreateMqttClient();
            return new(deviceId, client);
        }
        public MqttClientService(string deviceId, IMqttClient client)
        {
            DeviceId = deviceId;
            _client = client;
        }

        public async Task Connect(string host)
        {
            var options = new MqttClientOptionsBuilder()
                            .WithClientId(DeviceId)
                            //Must be version 500 so that response topics work
                            // .WithProtocolVersion(MqttProtocolVersion.V500)
                            .WithTcpServer(host)
                            .Build();
            await _client.ConnectAsync(options, new CancellationToken());
            _client.ApplicationMessageReceivedHandler = this;
        }
      
        private async Task<bool> PublishInternal(string topic, object value)
        {
            try
            {
                switch (value)
                {
                    case string str:
                        await _client.PublishAsync(topic, str);
                        break;
                    default:

                        var serialized =  JsonSerializer.Serialize(value);
                        await _client.PublishAsync(topic, serialized);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to publish to topic {topic}. Error: {e}");
                return false;
            }
            return true;
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
        
        public async Task<bool> Subscribe(string route, MethodInfo method, object instance)
        {
            //Convert a "route" (routes can have named path parameters) into a "topic" (topics follow Mqtt.Extensions path formatting. Mainly wildcards are always "+")
            var  topic= Regex
                .Replace(route, "\\{.+\\}", Wildcard)
                .Trim('/'); 

            var result = await _client.SubscribeAsync(topic);

            RouteHandler func = message => InvokeWithMappedParameters(message, method, instance);
            RouteTrie.AddValue(topic, (route, func));

            return result.Items.Any(item => item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS0
                                            || item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS1
                                            || item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS2);

        } 
        /// <summary>
        /// Maps the parametesrs of the method to the list of values in the message, then invokes the method
        /// </summary>
        /// <param name="message"></param>
        /// <param name="method"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        private object? InvokeWithMappedParameters(MqttClientService.NotificationMessage message, MethodInfo method,
            object instance)
        {
            var stringArguments = message.GetPayload<string[]>();
            var parameters = method.GetParameters();
            if (parameters.Length > stringArguments.Length)
            {
                //we cannot invoke the method if we don't have enough arguments
                return null;
            }

            var typeConverter = new TypeConverter();
            List<object> arguments = new List<object>();
            for (int i = 0; i < parameters.Length; ++i)
            {
                var argument = typeConverter.ConvertTo(stringArguments[i], parameters[i].ParameterType);
                arguments.Add(argument);
            }

            try
            {
                return method.Invoke(instance, arguments.ToArray());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> Unsubscribe(string topic)
        {
            //TODO: Remove from trie
            var result = await _client.UnsubscribeAsync(topic);
            return result.Items.Any(item => item.ReasonCode != MqttClientUnsubscribeResultCode.Success);
        } 

        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            
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
                        if (response != null && !string.IsNullOrEmpty(eventArgs.ApplicationMessage.ResponseTopic))
                        {
                            //TODO: return values from handlers for responding to messages instead of requiring a client instance?
                            // await _client.PublishAsync(eventArgs.ApplicationMessage.ResponseTopic, response);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error while handling Mqtt topic: {e}");
                    }
                }
            }
        }
    }
}