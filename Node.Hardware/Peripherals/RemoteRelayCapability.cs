using System;
using System.Threading.Tasks;
using Node.Abstractions;

namespace Node.Hardware.Peripherals
{
    [Capability("Relay", "1.0.0")]
    public class RemoteRelayCapability : ICapability 
    {
        
        private readonly Transmitter433 _transmitter433;
        
        public RemoteRelayCapability(Transmitter433 transmitter433)
        {
            _transmitter433 = transmitter433;
        }

        [CapabilityAction]
        public Task TrySetValue(string address, float dutyCycle, int ticksPerSecond = 256)
        {
            byte[] addr = Convert.FromBase64String(address);
            //TODO: Define the remote relay protocol
            Console.WriteLine($"{address} {dutyCycle} {ticksPerSecond}");
            return Task.CompletedTask; 
        }
    }
}