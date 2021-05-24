using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public class Receiver433
    {
        private readonly GpioPin _pin;

        public Receiver433(GpioPin pin)
        {
            _pin = pin;
            _pin.Direction = PinDirection.Input;
        }
        
    }
}