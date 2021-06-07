using Node.Abstractions;
using RPINode.Peripherals;

namespace RPINode.Capabilities
{
    public class BlindsCapability : ICapability
    {
        private readonly BlindsDevice _blindsDevice;
        public string CapabilityId => CapabilityTypeId + _blindsDevice.BcmPin;
        public string CapabilityTypeId => nameof(BlindsDevice);

        public BlindsCapability(BlindsDevice blindsDevice)
        {
            _blindsDevice = blindsDevice;
        }
        
        [Action]
        public void Open(BlindsDevice.BlindsChannel channel)
        {
            _blindsDevice.SendCommand(channel, BlindsDevice.BlindsCommand.Open);
        }
        [Action]
        public void Close(BlindsDevice.BlindsChannel channel)
        {
            _blindsDevice.SendCommand(channel, BlindsDevice.BlindsCommand.Close);
        }
        [Action]
        public void Stop(BlindsDevice.BlindsChannel channel)
        {
            _blindsDevice.SendCommand(channel, BlindsDevice.BlindsCommand.Stop);
        }
    }
}