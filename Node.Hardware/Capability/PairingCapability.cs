using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;

namespace Node.Hardware.Capability
{
    //Used to pair a new device to this hub
    //Should transmit the hub id, and the address/channel the device should listen for. 
    [Capability("PairDevice", "1.0.0")]
    public class PairingCapability : ICapability
    {
        private readonly Transmitter433 _transmitter433;

        public PairingCapability(Transmitter433 transmitter433)
        {
            _transmitter433 = transmitter433;
        }

        [CapabilityAction]
        public Task WithChannel(int channel)
        {
            return Task.CompletedTask;
        }

        [CapabilityAction]
        public Task WithAddress(string address)
        {
            return Task.CompletedTask;
        }
    }
}