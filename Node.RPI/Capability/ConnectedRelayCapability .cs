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

        public async Task<object> Invoke(JsonElement request)
        {
            //TODO: I want the object structure in the shadow to follow this pattern - but it does make for some awkward code in this method
            var state = JsonSerializer.Deserialize<RelayPwmState>(request.GetRawText());

            for (int i = 0; i < state.Ports().Length; ++i)
            {
                
            }
            if (state.Port1 is {} port1)
            {
                if (_relays.Length > 0)
                {
                    await _relays[0].SetDutyCycle(port1.DutyCycle);
                }
            }
            if (state.Port2 is {} port2)
            {
                if (_relays.Length > 1)
                {
                    await _relays[1].SetDutyCycle(port2.DutyCycle);
                }    
            }
            if (state.Port3 is {} port3)
            {
                if (_relays.Length > 2)
                {
                    await _relays[2].SetDutyCycle(port3.DutyCycle);
                } 
            }
            if (state.Port4 is {} port4)
            {
                if (_relays.Length > 3)
                {
                    await _relays[3].SetDutyCycle(port4.DutyCycle);
                } 
            } 
            if (state.Port5 is {} port5)
            {
                if (_relays.Length > 4)
                {
                    await _relays[4].SetDutyCycle(port5.DutyCycle);
                } 
            }
            if (state.Port6 is {} port6)
            {
                if (_relays.Length > 5)
                {
                    await _relays[5].SetDutyCycle(port6.DutyCycle);
                } 
            }
            if (state.Port7 is {} port7)
            {
                if (_relays.Length > 6)
                {
                    await _relays[6].SetDutyCycle(port7.DutyCycle);
                } 
            }
            if (state.Port8 is {} port8)
            {
                if (_relays.Length > 7)
                {
                    await _relays[7].SetDutyCycle(port8.DutyCycle);
                } 
            } 
            
            return null;
        }
    }
}