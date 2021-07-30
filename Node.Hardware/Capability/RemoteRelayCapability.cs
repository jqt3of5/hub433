using System;
using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;

namespace Node.Hardware.Capability
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
        public async Task PairAddress(string address)
        {
            
        } 
        [CapabilityAction]
        public async Task PairChannel(int channel)
        {
            //TODO: Define the remote relay protocol
        } 
        
        [CapabilityAction]
        public Task Transmit(string address, int port, float dutyCycle)
        {
            byte[] addr = Convert.FromBase64String(address);
            //TODO: Define the remote relay protocol
            Console.WriteLine($"{address} {dutyCycle}"); 
            return Task.CompletedTask; 
        }
        
        [CapabilityAction]
        public Task Broadcast(int channel, int port, float dutyCycle)
        {
            //TODO: Define the remote relay protocol
            Console.WriteLine($"{channel} {dutyCycle}"); 
            return Task.CompletedTask; 
        }
        
        
        
    }
}