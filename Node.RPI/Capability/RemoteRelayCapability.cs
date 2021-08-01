using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;

namespace RPINode.Capability
{
    [Capability("RemoteRelay", "1.0.0")]
    public class RemoteRelayCapability : ICapability
    {
        private readonly Transmitter433 _transmitter433;
        
        public RemoteRelayCapability(Transmitter433 transmitter433)
        {
            _transmitter433 = transmitter433;
        }
        
        [CapabilityAction]
        public Task Pair(int channel)
        {
            return new RemoteRelay(_transmitter433).Pair(channel);
        } 
        
        [CapabilityAction]
        public Task On(int channel)
        {
            return new RemoteRelay(_transmitter433).On(channel);
        }  
        [CapabilityAction]
        public Task Off(int channel)
        {
            return new RemoteRelay(_transmitter433).Off(channel);
        } 
        [CapabilityAction]
        public Task Pwm(int channel, float dutyCycle, float cyclesPerSecond)
        {
            return new RemoteRelay(_transmitter433).Pwm(channel, dutyCycle, cyclesPerSecond);
        } 
    }
}