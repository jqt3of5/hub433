using System;
using System.Threading.Tasks;
using Node.Abstractions;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public record RadioSymbol(TimeSpan Duration, bool Value, int? Samples = null);
    
    public class Transmitter433
    {
        private readonly IGpioPin _pin;
        public BcmPin BcmPin => _pin.BcmPin;

        public Transmitter433(IGpioPin pin)
        {
            _pin = pin;
            pin.PinMode = GpioPinDriveMode.Output;
        }

        public async Task Transmit(RadioSymbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                _pin.Write(symbol.Value);
                await Task.Delay(symbol.Duration);
            }  
        }
    }
}