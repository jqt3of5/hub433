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
        private readonly MqttClientService _mqttClientService;

        public CapabilityService(MqttClientService mqttClientService)
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
                            param.ParameterType.Name)
                    ).ToArray();
                
                var descriptor = new DeviceCapabilityDescriptor.ActionDescriptor(
                    actionMethod.Name,
                    parameters,
                    actionMethod.ReturnType.Name
                );

                await _mqttClientService.Subscribe(
                    $"action/*/{_mqttClientService.DeviceId}/{capability.CapabilityId}/{descriptor.Name}",
                    (message) => InvokeWithMappedParameters(message, actionMethod, capability));
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
                    valueProperty.PropertyType.Name);
                
                await _mqttClientService.Subscribe(
                    $"value/*/{_mqttClientService.DeviceId}/{capability.CapabilityId}/{descriptor.Name}",
                    (message) => InvokeWithMappedParameters(message, valueProperty.GetMethod, capability)); 
                
                values.Add(descriptor); 
            }
            
            return new DeviceCapabilityDescriptor(capability.CapabilityId, capability.CapabilityTypeId, values.ToArray(), actions.ToArray());
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
        
    }
}