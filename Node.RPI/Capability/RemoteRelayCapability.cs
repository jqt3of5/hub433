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
        
        public Task On(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<OnOffPayload>();
            return new RemoteRelay(_transmitter433).On(payload.Channel, payload.Ports);
        }  
        public Task Off(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<OnOffPayload>();
            return new RemoteRelay(_transmitter433).Off(payload.Channel, payload.Ports);
        }
        public Task Pwm(DeviceCapabilityActionRequest actionRequest)
        {
            var payload = actionRequest.GetPayloadAs<PwmPayload>();
            return new RemoteRelay(_transmitter433).Pwm(payload.Channel, payload.Values);
        }

        public Task<object> Invoke(JsonElement request)
        {
        }
    }
}