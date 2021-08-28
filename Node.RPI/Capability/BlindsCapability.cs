﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;

namespace RPINode.Capability
{
    [Capability("Blinds", typeof(BlindsStatePayload))]
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
        public Task Pair(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ChannelCommandPayload>();
            Logger.Log($"Pairing blinds on channel: {payload.Channel}");
            return new Blinds(_transmitter433).Pair(payload.Channel);
        }

        public class ChannelCommandPayload
        {
            public Blinds.BlindsChannel Channel { get; set; }
            
        }
        
        public Task Stop(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ChannelCommandPayload>();
            Logger.Log($"Stopping blinds on channel: {payload.Channel}");
            return new Blinds(_transmitter433).Broadcast(payload.Channel, Blinds.BlindsCommand.Stop);
        }
        public Task Up(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ChannelCommandPayload>();
            Logger.Log($"Upping blinds on channel: {payload.Channel}");
            return new Blinds(_transmitter433).Broadcast(payload.Channel, Blinds.BlindsCommand.Up);
        }
        public Task Down(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<ChannelCommandPayload>();
            Logger.Log($"Downing blinds on channel: {payload.Channel}");
            return new Blinds(_transmitter433).Broadcast(payload.Channel, Blinds.BlindsCommand.Down);
        }

        public struct BlindsStatePayload
        {
            public ChannelState? channel1 { get; set; }
            public ChannelState? channel2 { get; set; }
            public ChannelState? channel3 { get; set; }
            public ChannelState? channel4 { get; set; }
            public ChannelState? channel5 { get; set; }
            public ChannelState? channel6 { get; set; }

            public (Blinds.BlindsChannel, ChannelState?)[] Channels() => new[] {
                (Blinds.BlindsChannel.Channel1, channel1),
                (Blinds.BlindsChannel.Channel2, channel2), 
                (Blinds.BlindsChannel.Channel3, channel3), 
                (Blinds.BlindsChannel.Channel4, channel4), 
                (Blinds.BlindsChannel.Channel5, channel5), 
                (Blinds.BlindsChannel.Channel6, channel6)
            };
            
            
            public struct ChannelState
            {
                public float percentage { get; set; }
            }
        }
        
        public async Task<object> UpdateState(JsonElement request)
        {
            var state = JsonSerializer.Deserialize<BlindsStatePayload>(request.GetRawText());
            var blinds = new Blinds(_transmitter433);

            for (int i = 0; i < state.Channels().Length; ++i)
            {
                var (channel, channelState) = state.Channels()[i];
                
                if (channelState is {} cs)
                {
                    if (cs.percentage > .50)
                    {
                        Logger.Log($"Closing blinds on channel: {channel}");
                        await blinds.Broadcast(channel, Blinds.BlindsCommand.Close);
                    }
                    else
                    {
                        Logger.Log($"Opening blinds on channel: {channel}");
                       await blinds.Broadcast(channel, Blinds.BlindsCommand.Open);
                    }
                }
            }
            return null;
        }
    }
}