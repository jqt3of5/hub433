using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly CapabilityService _capabilityService;

        public CommunicationService(IMqttClientService mqttClientService, CapabilityService capabilityService)
        {
            _mqttClientService = mqttClientService;
            _capabilityService = capabilityService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //TODO: Publish true version
                await _mqttClientService.Publish($"device/{_mqttClientService.DeviceId}/online", "0.0.1");
                await Task.Delay(10000, stoppingToken);
            }
            //TODO: Publish shutdown reason
            await _mqttClientService.Publish($"device/{_mqttClientService.DeviceId}/offline", "Cancellation Requested");
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _mqttClientService.Subscribe(this);
            await _mqttClientService.Subscribe($"capability/{_mqttClientService.DeviceId}");

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _mqttClientService.Unsubscribe($"capability/{_mqttClientService.DeviceId}");
            await _mqttClientService.RegisterHandler(this);

            await base.StopAsync(cancellationToken);
        }
        
        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            var capabilityMatch = Regex.Match(eventArgs.ApplicationMessage.Topic, $"capability/{_mqttClientService.DeviceId}");
            if (capabilityMatch.Success)
            {
                try
                {
                    var payload = eventArgs.ApplicationMessage.Payload;
                    var capabilityRequest = JsonSerializer.Deserialize<DeviceCapabilityRequest>(Encoding.UTF8.GetString(payload));
                    var response = await _capabilityService.InvokeCapability(capabilityRequest);
                    if (!string.IsNullOrEmpty(eventArgs.ApplicationMessage.ResponseTopic))
                    {
                        await _mqttClientService.Publish(eventArgs.ApplicationMessage.ResponseTopic,
                            JsonSerializer.Serialize(response));
                    }
                }
                catch (Exception e)
                {
                    if (!string.IsNullOrEmpty(eventArgs.ApplicationMessage.ResponseTopic))
                    {
                        await _mqttClientService.Publish(eventArgs.ApplicationMessage.ResponseTopic, e.ToString());
                    }
                }
            }
        }
    }
}