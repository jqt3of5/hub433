using System;
using System.Text.Json;
using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;

namespace RPINode.Capability
{
    [Capability("Relay")]
    public class ConnectedRelayCapability : ICapability
    {
        private readonly Relay[] _relays;

        public ConnectedRelayCapability(params Relay [] relays)
        {
            _relays = relays;
        }
        
        private Relay Get(int port)
        {
            if (_relays.Length > port)
            {
                return _relays[port];
            }

            return null;
        }

        public class ConnectedRelayPortPayload
        {
            public int Port { get; set; }
        }
        public Task On(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ConnectedRelayPortPayload>();
            return Get(payload.Port)?.SetDutyCycle(1);
        }  
        
        public Task Off(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ConnectedRelayPortPayload>();
            return Get(payload.Port)?.SetDutyCycle(0);
        }

        public class ConnectedRelayPwmPayload
        {
            public int Port { get; set; } 
            public float DutyCycle { get; set; }
            public float CyclesPerSecond { get; set; }
        }
        
        public Task Pwm(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ConnectedRelayPwmPayload>();
            var relay = Get(payload.Port);
            if (relay == null)
                return Task.CompletedTask;
            
            relay.PwmPeriod = TimeSpan.FromSeconds(1 / payload.CyclesPerSecond);
            
            return relay.SetDutyCycle(payload.DutyCycle);
        }

        public Task<object> Invoke(JsonElement request)
        {
        }
    }
}