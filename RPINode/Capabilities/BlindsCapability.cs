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
        public void Open()
        {
            _blindsDevice.Open();
        }
        
        [Action]
        public void Close()
        {
            _blindsDevice.Close();     
        }
        [Action]
        public void Stop()
        {
            _blindsDevice.Stop(); 
        }
    }
}