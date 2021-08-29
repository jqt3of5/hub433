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
                List<RadioSymbol> symbols = new List<RadioSymbol>(1000); 
                var sw = new Stopwatch();
                sw.Start();
                
                const int window = 5; //# samples
                const int sample_time = 20000; //nS
                var history = new Queue<bool>();
                bool? lastValue = null;
                long? lastTicks = null;
    #if DEBUG
                long totalMissed = 0;
                long totalSamples = 0;
    #endif
                while (!token.IsCancellationRequested)
                {
                    for (int i = 0; i < 100; ++i)
                    {
                        var ticks = sw.ElapsedTicks + sample_time;
                        history.Enqueue(_pin.Value);
                        
                        //Don't run the algorithm if our queue isn't full 
                        if (history.Count >= window)
                        {
                            var t = sw.ElapsedTicks;
                            var v = lowPass(history.ToArray());
                            //If our value switches, track the edge. 
                            if (v != lastValue)
                            {
                                if (lastTicks != null)
                                {
                                    symbols.Add(new RadioSymbol((int) (t - lastTicks), v)); 
                                }
                                
                                lastValue = v;
                                lastTicks = t;
                            } 
                            history.Dequeue();
                        }
     #if DEBUG   
                        totalSamples += 1;
                        if (sw.ElapsedTicks > ticks + sample_time)
                        {
                            //Not really sure about this, but if elapsed ticks is greater than our target, then we did too much processing
                            totalMissed += 1;
                        }
     #endif                   
                        // ReSharper disable once EmptyEmbeddedStatement
                        while (sw.ElapsedTicks < ticks);
                    }
                }
    #if DEBUG
                Console.WriteLine($"totalMissed: {totalMissed} totalSamples:{totalSamples} ticksPerSecond: {Stopwatch.Frequency}");
    #endif
                
                return symbols.ToArray(); 
            });
            
            bool lowPass(IEnumerable<bool> samples) => samples.Sum( b => b ? 1.0 : 0.0 ) / samples.Count() > .5;
        }
        
    }
}