using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;

namespace RPINode.Capability
{
    [Capability("Blinds")]
    public class BlindsCapability : ICapability
    {
        private readonly Transmitter433 _transmitter433;

        public BlindsCapability(Transmitter433 transmitter433)
        {
            _transmitter433 = transmitter433;
        }
        
        public class PairPayload
        {
           public int Channel { get; set; } 
        }
        
        [CapabilityAction(typeof(PairPayload))]
        public Task Pair(Blinds.BlindsChannel channel)
        {
            return new Blinds(_transmitter433).Pair(channel);
        }

        public class ChannelCommandPayload
        {
            public Blinds.BlindsChannel Channel { get; set; }
            
        }
        
        public Task Stop(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ChannelCommandPayload>();
            return new Blinds(_transmitter433).Broadcast(payload.Channel, Blinds.BlindsCommand.Stop);
        }
        public Task Up(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ChannelCommandPayload>();
            return new Blinds(_transmitter433).Broadcast(payload.Channel, Blinds.BlindsCommand.Up);
        }
        public Task Down(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ChannelCommandPayload>();
            return new Blinds(_transmitter433).Broadcast(payload.Channel, Blinds.BlindsCommand.Down);
        }

        public class BlindsStatePayload
        {
            public Dictionary<Blinds.BlindsChannel, ChannelState> channels { get; set; }

            public class ChannelState
            {
                public enum BlindsState
                {
                    Open, 
                    Closed
                }
                public BlindsState state { get; set; } 
            }
        }
        public async Task<object> Invoke(JsonElement request)
        {
            var payload = JsonSerializer.Deserialize<BlindsStatePayload>(request.GetRawText());

            var blinds = new Blinds(_transmitter433);
            foreach (var channelState in payload.channels)
            {
                switch (channelState.Value.state)
                {
                   case BlindsStatePayload.ChannelState.BlindsState.Closed:
                       await blinds.Broadcast(channelState.Key, Blinds.BlindsCommand.Close);
                       break;
                   case BlindsStatePayload.ChannelState.BlindsState.Open:
                       await blinds.Broadcast(channelState.Key, Blinds.BlindsCommand.Close);
                       break;
                }
            }

            return null;
        }
    }
}