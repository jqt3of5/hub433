using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Swan;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public record RadioSymbol(int DurationUS, bool Value, int? Samples = null);
    
    public class Transmitter433
    {
        private readonly GpioPin _pin;
        public int BcmPin => _pin.PinNumber;

        public Transmitter433(GpioPin pin)
        {
            _pin = pin;
            pin.PullMode = GpioPullMode.Up;
            pin.Direction = PinDirection.Output;
            pin.Value = false;
        }

        public Task Transmit(RadioSymbol[] symbols)
        {
            return Task.Run(() =>
            {
                // Console.WriteLine("Starting transmit");
                var sw = new Stopwatch();
                foreach (var symbol in symbols)
                {
                    // _pin.Write(symbol.Value ? 1 : 0);
                    _pin.Value = symbol.Value;
                    sw.Restart();
                    var ticks = Stopwatch.Frequency / 1000000 * symbol.DurationUS;
                    // ReSharper disable once EmptyEmbeddedStatement
                    while (sw.ElapsedTicks < ticks);
                }  
                // _pin.Write(0);
                _pin.Value = false;
                // Console.WriteLine("Transmit Done");                
            });
        }
    }
}