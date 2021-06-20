using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using mqtt.Notification;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Receiving;
using Node.Abstractions;

namespace RPINode
{
    public class CapabilityService
    {
        private readonly IMqttClientService _mqttClientService;

        public CapabilityService(IMqttClientService mqttClientService)
        {
            _mqttClientService = mqttClientService;
        }

        public async IAsyncEnumerable<DeviceCapabilityDescriptor> RegisterCapabilities(params ICapability[] capabilities)
        {
            foreach (var capability in capabilities)
            {
               yield return await RegisterCapability(capability);
            }
        }
        
        public async Task<DeviceCapabilityDescriptor> RegisterCapability(ICapability capability)
        {
            var descriptor = CapabilityDescriber.Describe(capability);

            foreach (var action in descriptor.Actions)
            {
                action.MqttTopic = $"action/+/{_mqttClientService.DeviceId}/{capability.CapabilityId}/{action.Name}"; 
                if (!await _mqttClientService.Subscribe(
                    action.MqttTopic,
                    action.Method,
                    capability))
                {
                    //TODO: Failure to subscribe??
                }
            }

            foreach (var property in descriptor.Properties)
            {
                property.MqttTopic = $"value/+/{_mqttClientService.DeviceId}/{capability.CapabilityId}/{property.Name}";
            
                if (!await _mqttClientService.Subscribe(
                    property.MqttTopic,
                    property.Property.GetMethod,
                    capability))
                    //TODO: What about set method?
                {
                    //TODO: fialure
                } 
            }
            //TODO: Register events

            return descriptor;
        }
    }
}