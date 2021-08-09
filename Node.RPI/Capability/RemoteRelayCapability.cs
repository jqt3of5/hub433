using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Node.Abstractions;
using Node.Hardware.Peripherals;

namespace RPINode.Capability
{
    [Capability("RemoteRelay")]
    public class RemoteRelayCapability : ICapability
    {
        private readonly Transmitter433 _transmitter433;
        
        public RemoteRelayCapability(Transmitter433 transmitter433)
        {
            _transmitter433 = transmitter433;
        }

        public class PairPayload
        {
           public int Channel { get; set; } 
        }
        public class OnOffPayload
        {
            public int Channel { get; set; }
            public int [] Ports { get; set; }
        }
        public class PwmPayload
        {
            public int Channel { get; set; }
            public (int port, float dutyCycle, float cyclesPerSecond) [] Values { get; set; }
        }
        
        [CapabilityAction(typeof(PairPayload))]
        public Task Pair(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<PairPayload>();
            return new RemoteRelay(_transmitter433).Pair(payload.Channel);
        }
        
        [CapabilityAction(typeof(OnOffPayload))]
        public Task On(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<OnOffPayload>();
            return new RemoteRelay(_transmitter433).On(payload.Channel, payload.Ports);
        }  
        
        [CapabilityAction(typeof(OnOffPayload))]
        public Task Off(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<OnOffPayload>();
            return new RemoteRelay(_transmitter433).Off(payload.Channel, payload.Ports);
        }
        
        [CapabilityAction(typeof(PwmPayload))]
        public Task Pwm(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<PwmPayload>();
            return new RemoteRelay(_transmitter433).Pwm(payload.Channel, payload.Values);
        }

        public struct RemoteRelayState
        {
            public ConnectedRelayCapability.RelayPwmState? Channel1 { get; set; }
            public ConnectedRelayCapability.RelayPwmState? Channel2 { get; set; }
            public ConnectedRelayCapability.RelayPwmState? Channel3 { get; set; }
            public ConnectedRelayCapability.RelayPwmState? Channel4 { get; set; }
            public ConnectedRelayCapability.RelayPwmState? Channel5 { get; set; }
            public ConnectedRelayCapability.RelayPwmState? Channel6 { get; set; }

            public ConnectedRelayCapability.RelayPwmState? [] Channels()
            {
                return new[] {Channel1, Channel2, Channel3, Channel4, Channel5, Channel6};
            }
        }
        public async Task<object> UpdateState(JsonElement request)
        {
            //TODO: I want the object structure in the shadow to follow this pattern - but it does make for some awkward code in this method
            var state = JsonSerializer.Deserialize<RemoteRelayState>(request.GetRawText());

            var relay = new RemoteRelay(_transmitter433);

            for (int i = 0; i < state.Channels().Length; ++i)
            {
                if (state.Channels()[i] is { } channel)
                {
                    //Remote relays are sent the whole list states for the ports at once.  
                    var portValues = channel.Ports().Where(port => port != null).Select((port, j) =>
                        (j, port.Value.DutyCycle, port.Value.CyclesPerSecond)).ToArray();
                    await relay.Pwm(i, portValues);
                }
            }

            return null;
        }
    }
}