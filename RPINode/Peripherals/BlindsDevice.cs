using Unosquare.RaspberryIO.Abstractions;

namespace RPINode.Peripherals
{
    //TODO: This is an abstraction on a device, not a device itself. Should it be combined with BlindsCapability? 
    public class BlindsDevice
    {
        private readonly Transmitter433 _transmitter433;

        public BcmPin BcmPin => _transmitter433.BcmPin;
        
        public BlindsDevice(Transmitter433 transmitter433)
        {
            _transmitter433 = transmitter433;
        }

        public void Open()
        {
            
        }

        public void Close()
        {
            
        }

        public void Stop()
        {
            
        }
    }
}