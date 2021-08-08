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

        public struct RelayPwmState
        {
            public struct RelayPortState
            {
                public float DutyCycle { get; set; }
                public float CyclesPerSecond { get; set; }
            }
            
            public RelayPortState? Port1 { get; set; }
            public RelayPortState? Port2 { get; set; }
            public RelayPortState? Port3 { get; set; }
            public RelayPortState? Port4 { get; set; }
            public RelayPortState? Port5 { get; set; }
            public RelayPortState? Port6 { get; set; }
            public RelayPortState? Port7 { get; set; }
            public RelayPortState? Port8 { get; set; }

            public RelayPortState?[] Ports()
            {
                return new[] {Port1, Port2, Port3, Port4, Port5, Port6, Port7, Port8};
            }
        }
        
        public struct ConnectedRelayPortPayload
        {
            public int Port { get; set; }
        }
        
        
        public struct ConnectedRelayPwmPayload
        {
            public int Port { get; set; } 
            public float DutyCycle { get; set; }
            public float CyclesPerSecond { get; set; }
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
 
        public Task Pwm(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ConnectedRelayPwmPayload>();
            var relay = Get(payload.Port);
            if (relay == null)
                return Task.CompletedTask;
            
            relay.PwmPeriod = TimeSpan.FromSeconds(1 / payload.CyclesPerSecond);
            
            return relay.SetDutyCycle(payload.DutyCycle);
        }

        public async Task<object> UpdateState(JsonElement request)
        {
            //TODO: I want the object structure in the shadow to follow this pattern - but it does make for some awkward code in this method
            var state = JsonSerializer.Deserialize<RelayPwmState>(request.GetRawText());

            for (int i = 0; i < state.Ports().Length; ++i)
            {
                if (state.Ports()[i] is { } port)
                {
                    if (_relays.Length > i)
                    {
                        await _relays[i].SetDutyCycle(port.DutyCycle);
                    }
                }
            }
            return null;
        }
    }
}