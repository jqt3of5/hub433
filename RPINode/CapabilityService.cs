using System.Collections.Generic;
using System.Linq;
using MQTTnet.Client;
using Node.Abstractions;

namespace RPINode
{
    public class CapabilityService
    {
        private readonly MqttClient _mqttClient;

        public CapabilityService(MqttClient mqttClient)
        {
            _mqttClient = mqttClient;
        }

        public DeviceCapabilityDescriptor RegisterCapability(ICapability capability)
        {
            var actionMethods = capability.GetType().GetMethods().Where(info =>
                info.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ActionAttribute)));
            
            var actions = new List<DeviceCapabilityDescriptor.ActionDescriptor>();
            foreach (var actionMethod in actionMethods)
            {
                var parameters = actionMethod
                    .GetParameters()
                    .Select(param =>
                        new DeviceCapabilityDescriptor.ValueDescriptor(param.Name,
                            //TODO: Get parameter type
                            DeviceCapabilityDescriptor.ValueDescriptor.TypeEnum.String)
                    ).ToArray();
                
                var descriptor = new DeviceCapabilityDescriptor.ActionDescriptor(
                    actionMethod.Name,
                    parameters,
                    //TODO: get return type
                    DeviceCapabilityDescriptor.ValueDescriptor.TypeEnum.Void
                );
                
                //TODO: Register with mqtt 
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
                
                //TODO: Register with mqtt 
               values.Add(descriptor); 
            }
            
            return new DeviceCapabilityDescriptor(capability.CapabilityId, capability.CapabilityTypeId, values.ToArray(), actions.ToArray());
        }
    }
}