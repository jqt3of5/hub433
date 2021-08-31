using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Abstractions;

namespace Node.Hardware.Peripherals
{
    public class Receiver433
    {
        private readonly IGpioPin _pin;

        public Receiver433(IGpioPin pin)
        {
            _pin = pin;
            _pin.PinMode = GpioPinDriveMode.Input;
            _pin.InputPullMode = GpioPinResistorPullMode.PullDown;
        }

        public Task<RadioSymbol[]> Receive(CancellationToken token)
        {
            return Task.Run(() =>
            {
                List<RadioSymbol> symbols = new List<RadioSymbol>();

                long highSamples = 0;
                long bitSamples = 0;
                long totalSamples = 0;
                bool? lastValue = null;
                var sw = new Stopwatch();

                while (!token.IsCancellationRequested)
                {
                    bool value;
                    do
                    {
                        value = _pin.Value;
                        bitSamples += 1;
                        if (value)
                        {
                            highSamples += 1;
                        }

                        //If we read a high state after a low state, bit transition
                        //TODO: this is pretty sensitive to any noise - if we get an errant high reading then the bit ends!. probably not likely though...
                        if (value == true && lastValue == false)
                        {
                            break;
                        }

                        //If the number of samples for this bit exceeds the average samples per bit, timeout. Poor mans PLL
                        //TODO: This might be a little sketchy, depends on a few starting bits that are longer than normal. 
                    } while (symbols.Count == 0 || bitSamples < totalSamples/symbols.Count);
                    
                    sw.Stop();
                    symbols.Add(new RadioSymbol(sw.ElapsedTicks, ((float)highSamples/bitSamples) > .5, bitSamples-1));        
                    sw.Restart();

                    totalSamples += bitSamples;
                    bitSamples = 1;
                    if (value == true)
                    {
                        highSamples = 1;
                    }
                    lastValue = value;
                }
            
                Console.WriteLine($"total samples: {totalSamples}");
                return symbols.ToArray();
            });
        }
    }
}