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

                long waveSample = 0;
                long totalSamples = 0;
                bool? lastValue = null;
                var sw = new Stopwatch();

                while (true)
                {
                    var value = _pin.Value;
                    totalSamples += 1;
                    waveSample += 1; 
                    if (value != lastValue && lastValue.HasValue)
                    {
                        sw.Stop();
                        symbols.Add(new RadioSymbol(sw.ElapsedTicks, lastValue.Value, waveSample -1));        
                        waveSample = 1;
                        sw.Restart();
                    }
                    lastValue = value;
                    
                    if (token.IsCancellationRequested)
                }

                if (lastValue.HasValue)
                {
                    symbols.Add(new RadioSymbol(waveSample, lastValue.Value));        
                }
            
                Console.WriteLine($"total samples: {totalSamples}");
                return symbols.ToArray();
            });
        }
    }
}