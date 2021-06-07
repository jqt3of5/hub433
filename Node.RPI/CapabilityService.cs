using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Receiving;
using Node.Abstractions;

namespace RPINode
{
    public class CapabilityService
    {
        private readonly MqttClient _mqttClient;

        public CapabilityService(MqttClient mqttClient)
        {
            _mqttClient = mqttClient;
            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(MqttMessageHandler);
        }

        private Task MqttMessageHandler(MqttApplicationMessageReceivedEventArgs message)
        {
            //TODO: Route to the proper handler 
            //TODO: Parse parameters
            return Task.CompletedTask;
        }

        private async Task Subscribe(string topic, MethodInfo methodInfo, object instance) 
        {
            await _mqttClient.SubscribeAsync(topic);
        } 
       

        public async Task<DeviceCapabilityDescriptor> RegisterCapability(ICapability capability)
        {
            var actionMethods = capability.GetType().GetMethods().Where(info =>
                info.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ActionAttribute)));
            
            var actions = new List<DeviceCapabilityDescriptor.ActionDescriptor>();
            foreach (var actionMethod in actionMethods)
            {
                var parameters = actionMethod
                    .GetParameters()
                    .Select(param =>
                        new DeviceCapabilityDescriptor.ValueDescriptor(
                            param.Name,
                            //TODO: Get parameter type
                            DeviceCapabilityDescriptor.ValueDescriptor.TypeEnum.String)
                    ).ToArray();
                
                var descriptor = new DeviceCapabilityDescriptor.ActionDescriptor(
                    actionMethod.Name,
                    parameters,
                    //TODO: get return type
                    DeviceCapabilityDescriptor.ValueDescriptor.TypeEnum.Void
                );

                await Subscribe($"action/*/{_mqttClient.Options.ClientId}/{capability.CapabilityId}/{descriptor.Name}", actionMethod, capability); 
                actions.Add(descriptor);
            }
            
            var valueProperties = capability
                .GetType()
                .GetProperties()
                .Where(info =>
                    info
                        .CustomAttributes
                        .Any(attribute => 
                                attribute.AttributeType == typeof(ValueAttribute)
                            )
                    ); 
            
            var values = new List<DeviceCapabilityDescriptor.ValueDescriptor>();
            foreach (var valueProperty in valueProperties)
            {
                var descriptor = new DeviceCapabilityDescriptor.ValueDescriptor(
                    valueProperty.Name,
                    //TODO: get the property type 
                    DeviceCapabilityDescriptor.ValueDescriptor.TypeEnum.String);
                
                await Subscribe($"value/*/{_mqttClient.Options.ClientId}/{capability.CapabilityId}/{descriptor.Name}", valueProperty.GetMethod, capability); 
                values.Add(descriptor); 
            }
            
            return new DeviceCapabilityDescriptor(capability.CapabilityId, capability.CapabilityTypeId, values.ToArray(), actions.ToArray());
        }
    }
}