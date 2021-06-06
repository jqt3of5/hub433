using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Node.Abstractions;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public record RadioSymbol(int DurationUS, bool Value, int? Samples = null);
    
    public class Transmitter433
    {
        private readonly IGpioPin _pin;
        public BcmPin BcmPin => _pin.BcmPin;

        public Transmitter433(IGpioPin pin)
        {
            _pin = pin;
            pin.PinMode = GpioPinDriveMode.Output;
        }

        public Task Transmit(RadioSymbol[] symbols)
        {
            return Task.Run(() =>
            {
                var sw = new Stopwatch();
                sw.Start();
                foreach (var symbol in symbols)
                {
                    _pin.Write(symbol.Value);
                    var ticks = Stopwatch.Frequency / 100000 * symbol.DurationUS;
                    while (sw.ElapsedTicks < ticks);
                }  
                _pin.Write(false);
            });
        }
    }
}