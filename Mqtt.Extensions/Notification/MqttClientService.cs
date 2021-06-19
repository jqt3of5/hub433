using System;
using System.Collections.Generic;
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

namespace mqtt.Notification
{
    public class MqttClientService : IMqttApplicationMessageReceivedHandler
    {
        public delegate object? RouteHandler(MqttApplicationMessage message);
        public const string Wildcard = "+";
        
        public readonly TrieNode<(string, RouteHandler)> RouteTrie = new(); 
        
        private readonly string _deviceId;
        
        private readonly IMqttClient _client;

        public static MqttClientService Create(string deviceId)
        {
            var client = new MqttFactory().CreateMqttClient();
            return new(deviceId, client);
        }
        public MqttClientService(string deviceId, IMqttClient client)
        {
            _deviceId = deviceId;
            _client = client;
        }

        public async Task Connect(string host)
        {
            var options = new MqttClientOptionsBuilder()
                            .WithClientId(_deviceId)
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

        public async Task<IDisposable> Subscribe(string route, RouteHandler func)
        {
            //Convert a "route" (routes can have named path parameters) into a "topic" (topics follow Mqtt.Extensions path formatting. Mainly wildcards are always "+")
            var  topic= Regex
                .Replace(route, "\\{.+\\}", Wildcard)
                .Trim('/'); 

            var result = await _client.SubscribeAsync(topic);
         
            RouteTrie.AddValue(topic, (route, func));
            
            //TODO: Remove from trie, store the unsubscriber in the tree?
            return new Unsubscriber(() => _client.UnsubscribeAsync(topic));  
        }
        public bool Unsubscribe(string topic)
        {
            //TODO: Need to build this out
            return false;
        } 
        public bool Unsubscribe(RouteHandler func)
        {
            //TODO: Need to build this out
            return false;
        }

        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            var subtopic = eventArgs.ApplicationMessage.Topic.Split('/').ToArray();
            
            if (RouteTrie.TryGetValue(subtopic, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    var (path, method) = handler;
                    //Map actual topic to topic descriptor
                    var pathParts = path.Split('/');
                    
                    //Extract the parameters from the topic
                    var handlerParams = new List<string>();
                    for (var i = 0; i < pathParts.Length; ++i)
                    {
                        if (Regex.IsMatch(pathParts[i], "\\{.+\\}") || pathParts[i] == Wildcard)
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