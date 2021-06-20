using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Node.Abstractions
{
    public record DeviceCapabilityDescriptor (string CapabilityId, string CapabilityTypeId, DeviceCapabilityDescriptor.PropertyDescriptor [] Properties, DeviceCapabilityDescriptor.ActionDescriptor [] Actions)
    {
        public record ActionDescriptor(string Name, ValueDescriptor[] Parameters, string TypeName)
        {
            public string MqttTopic { get; set; }
            
            [JsonIgnore]
            public MethodInfo Method { get; init; }
        }
        
        public record PropertyDescriptor(string Name, string TypeName, bool ReadOnly)
        {
            public string MqttTopic { get; set; }
            
            [JsonIgnore]
            public PropertyInfo Property{ get; init; }
        }

        public record ValueDescriptor(string Name, string TypeName);
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class ActionAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public ActionAttribute()
        {
        }
    }
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class ValueAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public ValueAttribute()
        {
        }
    } 
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class EventAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public EventAttribute()
        {
        }
    }  
    
    public interface ICapability
    {
        string CapabilityId { get; }
        string CapabilityTypeId { get; } 
    }
}