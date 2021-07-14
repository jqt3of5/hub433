using System.Diagnostics;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Abstractions;

namespace Node.Hardware.Peripherals
{
    public record RadioSymbol(int DurationUS, bool Value, int? Samples = null);
    
    public class Transmitter433
    {
        private readonly IGpioPin _pin;
        public int BcmPin => _pin.BcmPinNumber;

        public Transmitter433(IGpioPin pin)
        {
            _pin = pin;
            pin.InputPullMode = GpioPinResistorPullMode.PullUp;
            pin.PinMode = GpioPinDriveMode.Output;
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