using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;

namespace RPINode.Capability
{
    [Capability("Blinds", "1.0.0")]
    public class BlindsCapability : ICapability
    {
        private readonly Transmitter433 _transmitter433;

        public BlindsCapability(Transmitter433 transmitter433)
        {
            _transmitter433 = transmitter433;
        }

        [CapabilityAction]
        public Task Pair(Blinds.BlindsChannel channel)
        {
            return new Blinds(_transmitter433).Pair(channel);
        }

        [CapabilityAction]
        public Task Stop(Blinds.BlindsChannel channel)
        {
            return new Blinds(_transmitter433).Broadcast(channel, Blinds.BlindsCommand.Stop);
        }
        
        [CapabilityAction]
        public Task Open(Blinds.BlindsChannel channel)
        {
            return new Blinds(_transmitter433).Broadcast(channel, Blinds.BlindsCommand.Open);
        }
       
        [CapabilityAction]
        public Task Close(Blinds.BlindsChannel channel)
        {
            return new Blinds(_transmitter433).Broadcast(channel, Blinds.BlindsCommand.Close);
        }
       
        [CapabilityAction]
        public Task Set(Blinds.BlindsChannel channel, int increment)
        {
            if (increment > 0)
            {
                return new Blinds(_transmitter433).Broadcast(channel, Blinds.BlindsCommand.Down);
            }
            else if (increment < 0)
            {
                return new Blinds(_transmitter433).Broadcast(channel, Blinds.BlindsCommand.Up);
            }

            return Task.CompletedTask;
        }
    }
}