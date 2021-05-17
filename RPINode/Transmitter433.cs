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
        private readonly GpioPin _pin;

        public Transmitter433(GpioPin pin)
        {
            _pin = pin;
            pin.Direction = PinDirection.Output;
        }

        public async Task Transmit(RadioSymbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                _pin.Value = true;
                await Task.Delay(symbol.High);
                _pin.Value = false;
                await Task.Delay(symbol.Low);
            }  
        }
    }
}