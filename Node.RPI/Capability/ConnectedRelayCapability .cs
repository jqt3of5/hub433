using System;
using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;

namespace RPINode.Capability
{
    [Capability("Relay", "1.0.0")]
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
        
        [CapabilityAction]
        public Task On(int port)
        {
            return Get(port)?.SetDutyCycle(1);
        }  
        [CapabilityAction]
        public Task Off(int port)
        {
            return Get(port)?.SetDutyCycle(0);
        } 
        [CapabilityAction]
        public Task Pwm(int port, float dutyCycle, float cyclesPerSecond)
        {
            var relay = Get(port);
            relay.PwmPeriod = TimeSpan.FromSeconds(1 / cyclesPerSecond);
            return Get(port)?.SetDutyCycle(dutyCycle);
        } 
    }
}