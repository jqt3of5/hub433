using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Abstractions;

namespace Node.Hardware.Peripherals
{
    public record RadioSymbol(long DurationUS, bool Value, long? Samples = null);
    
    public static class RadioSymbolExtensions {
    
        public static IEnumerable<RadioSymbol> ToSymbols(this byte b)
        {
            for (int i = 7; i >= 0; --i)
            {
                if ((b & (1 << i)) != 0)
                {
                    yield return new RadioSymbol(725, true);
                    yield return new RadioSymbol(350, false);
                }
                else
                {
                    yield return new RadioSymbol(350, true);
                    yield return new RadioSymbol(725, false);
                }
            }
        }
        public static IEnumerable<RadioSymbol> ToSymbols(this IEnumerable<byte> bytes)
        {
            foreach (var b in bytes)
            {
                foreach (var symbol in b.ToSymbols())
                {
                    yield return symbol;
                }
            }
        }

        public static byte crc8(this byte b) => crc8(new[] {b});
        public static byte crc8(this IEnumerable<byte> data)
        {
            //Missing leading bit
            const int divisor = 0b00011101;

            byte crc = 0x00;
            foreach (var b in data.Append((byte)0x00))
            {
                for (int i = 7; i >= 0; --i)
                {
                    var msb = (crc & 0x08) != 0;
                    //Shift in next bit 
                    crc = (byte) (crc << 1);
                    if ((b & (1 << i)) != 0)
                    {
                        //Next bit is '1', set lsb to '1'
                        crc = (byte) (crc | 0x01);
                    }
                       
                    //if msb is '1', perform the division
                    if (msb)
                    {
                        crc = (byte) (crc ^ divisor);
                    }
                }
            }

            return crc;
        }
    }
    
    public class Transmitter433
    {
        private readonly IGpioPin _pin;
        public int BcmPin => _pin.BcmPinNumber;

        private object _lock = new object();
        
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
                lock (_lock)
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
                }
            });
        }
    }
}