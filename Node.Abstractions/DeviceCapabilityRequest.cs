using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Node.Abstractions
{
    public interface ICapability
    {
    }
    
    public struct DeviceCapabilityRequest
    {
        public string CapabilityType { get; set; } 
        public string CapabilityVersion{ get; set; } 
        public string CapabilityAction { get; set; }
        public string [] arguments { get; set; } 
    }
}