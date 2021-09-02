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

                Queue<int> lowPassQueue = new Queue<int>();
                int sampleIntegration = 0;
                const int sample_window = 100;
                
                var sw = new Stopwatch();
                while (!token.IsCancellationRequested)
                {
                    bool readValue;
                    do
                    {
                        readValue = _pin.Value;
                        
                        //Low pass the read value
                        var v = readValue ? 1 : 0;
                        sampleIntegration += v;
                        lowPassQueue.Enqueue(v);
                        //Full queue
                        if (lowPassQueue.Count > sample_window)
                        {
                            sampleIntegration -= lowPassQueue.Dequeue();
                        }
                        //less than half empty. half full is fine
                        else if (lowPassQueue.Count < sample_window/2 + 1)
                        {
                            //fill the queue
                            continue;
                        }

                        readValue = ((float)sampleIntegration / lowPassQueue.Count > .5);
                        
                        bitSamples += 1;
                        if (readValue)
                        {
                            highSamples += 1;
                            if (lastValue == false)
                            {
                                lastValue = readValue;
                                break;
                            }
                        }
                        
                        lastValue = readValue;
                        //Bit change, lowpassed values
                        //If the number of samples for this bit exceeds the average samples per bit, timeout. Poor mans PLL
                    } while (symbols.Count == 0 || bitSamples < totalSamples/symbols.Count);
                    
                    
                    sw.Stop();
                    symbols.Add(new RadioSymbol(sw.ElapsedTicks, ((float)(highSamples-(readValue?1:0))/(bitSamples-1)) > .5, highSamples-(readValue?1:0), bitSamples-1));        
                    sw.Restart();

                    totalSamples += bitSamples-1;
                    bitSamples = 1;
                    if (readValue)
                    {
                        highSamples = 1;
                    }
                    else
                    {
                        highSamples = 0;
                    }
                }
            
                Console.WriteLine($"total samples: {totalSamples}");
                return symbols.ToArray();
            });
        }
    }
}