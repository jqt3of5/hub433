using System;
using System.Threading.Tasks;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public record RadioSymbol(TimeSpan High, TimeSpan Low);
    
    public class Transmitter433
    {
        private readonly IGpioPin _pin;

        public Transmitter433(IGpioPin pin)
        {
            _pin = pin;
            pin.PinMode = GpioPinDriveMode.Output;
        }

        public async Task Transmit(RadioSymbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                if (symbol.High != TimeSpan.Zero)
                {
                    _pin.Value = true;
                    await Task.Delay(symbol.High);
                }
                
                if (symbol.Low != TimeSpan.Zero)
                {
                    _pin.Value = false;
                    await Task.Delay(symbol.Low);
                }
            }  
        }
    }
}