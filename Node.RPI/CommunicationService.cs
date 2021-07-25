using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using mqtt.Notification;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Receiving;
using Node.Abstractions;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public class CommunicationService : BackgroundService, IHostedService, IMqttApplicationMessageReceivedHandler 
    {
        private readonly IMqttClientService _mqttClientService;
        private readonly IConfiguration _configuration;
        private readonly CapabilityService _capabilityService;

        public CommunicationService(IConfiguration configuration, IMqttClientService mqttClientService, CapabilityService capabilityService)
        {
            _configuration = configuration;
            _capabilityService = capabilityService;
            _mqttClientService = mqttClientService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var version = _configuration["version"];
                await _mqttClientService.Publish($"device/{_mqttClientService.ThingName}/online", version);
                await Task.Delay(10000, stoppingToken);
            }
            //TODO: Publish shutdown reason
            await _mqttClientService.Publish($"device/{_mqttClientService.ThingName}/offline", "Cancellation Requested");
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _mqttClientService.Subscribe(this);
            await _mqttClientService.Subscribe($"capability/{_mqttClientService.ThingName}");

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _mqttClientService.Unsubscribe($"capability/{_mqttClientService.ThingName}");
            await _mqttClientService.RemoveHandler(this);

            await base.StopAsync(cancellationToken);
        }
        
        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            try
            {
                var capabilityMatch = Regex.Match(eventArgs.ApplicationMessage.Topic, $"capability/{_mqttClientService.ThingName}");
                if (capabilityMatch.Success)
                {
                    var payload = eventArgs.ApplicationMessage.Payload;
                    var capabilityRequest =
                        JsonSerializer.Deserialize<DeviceCapabilityRequest>(Encoding.UTF8.GetString(payload));
                    var response = await _capabilityService.InvokeCapability(capabilityRequest);
                    if (!string.IsNullOrEmpty(eventArgs.ApplicationMessage.ResponseTopic))
                    {
                        await _mqttClientService.Publish(eventArgs.ApplicationMessage.ResponseTopic,
                            JsonSerializer.Serialize(response));
                    }
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(eventArgs.ApplicationMessage.ResponseTopic))
                {
                    await _mqttClientService.Publish(eventArgs.ApplicationMessage.ResponseTopic, e.ToString());
                }
                
                await _mqttClientService.Publish($"error/{_mqttClientService.ThingName}", e.ToString());
            }
        }
    }
}