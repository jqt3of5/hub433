using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Node.Abstractions
{
    public interface ICapability
    {
        Task<object> Invoke(JsonElement request);
        
    }
    
    public struct DeviceCapabilityActionRequest
    {
        public string CapabilityType { get; set; } 
        public string CapabilityAction { get; set; }
        public JsonElement Payload { get; set; }

        public T? GetPayloadAs<T>()
        {
            return JsonSerializer.Deserialize<T>(Payload.GetRawText());
        }
    }
}