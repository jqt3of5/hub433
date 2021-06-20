using System.Collections.Generic;
using System.Linq;

namespace Node.Abstractions
{
  public class CapabilityDescriber
    {
        public static DeviceCapabilityDescriptor Describe(ICapability capability)
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
                ){Method = actionMethod};
                
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
            
            var values = new List<DeviceCapabilityDescriptor.PropertyDescriptor>();
            foreach (var valueProperty in valueProperties)
            {
                var descriptor = new DeviceCapabilityDescriptor.PropertyDescriptor(
                    valueProperty.Name,
                    valueProperty.PropertyType.Name, 
                    true)
                    {Property = valueProperty};

                values.Add(descriptor); 
            }
            //TODO: Register events
            
            return new DeviceCapabilityDescriptor(capability.CapabilityId, capability.CapabilityTypeId, values.ToArray(), actions.ToArray()); 
        }
    }
}