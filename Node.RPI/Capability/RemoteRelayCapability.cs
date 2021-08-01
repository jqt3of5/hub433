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
        public Task On(int channel, int [] ports)
        {
            return new RemoteRelay(_transmitter433).On(channel, ports);
        }  
        [CapabilityAction]
        public Task Off(int channel, int [] ports)
        {
            return new RemoteRelay(_transmitter433).Off(channel, ports);
        } 
        [CapabilityAction]
        public Task Pwm(int channel, (int port, float dutyCycle, float cyclesPerSecond) [] values)
        {
            return new RemoteRelay(_transmitter433).Pwm(channel, values);
        } 
    }
}