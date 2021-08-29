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

        public async Task<RadioSymbol[]> Receive(CancellationToken token)
        {
            const int window = 5; //# samples
            const int sample_time = 100000; //nS
            
            object @lock = new object();
            var samples = new Queue<(bool, long)>(100000);
            
            var producer = Task.Run(() =>
            {
#if DEBUG
                long totalMissed = 0;
                long totalSamples = 0;
#endif
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    while (!token.IsCancellationRequested)
                    {
                        var ticks = sw.ElapsedTicks + sample_time;
                        var sample = _pin.Value;
                        lock (@lock)
                        {
                            samples.Enqueue((sample, sw.ElapsedTicks));
                        }

                        totalSamples += 1;
                        if (sw.ElapsedTicks > ticks)
                        {
                            //Not really sure about this, but if elapsed ticks is greater than our target, then we did too much processing
                            totalMissed += 1;
                        }

                        // ReSharper disable once EmptyEmbeddedStatement
                        while (sw.ElapsedTicks < ticks) ;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine($"totalMissed: {totalMissed} totalSamples: {totalSamples}");
            });


            List<RadioSymbol> symbols = new List<RadioSymbol>(1000);
            try
            {
                var history = new Queue<(bool, long)>();
                bool? lastValue = null;
                long? lastTicks = null;
                while (samples.Any() || !token.IsCancellationRequested)
                {
                    // Thread.Sleep(1);
                    if (!samples.Any())
                    {
                        continue;
                    }

                    (bool, long) sample;
                    lock (@lock)
                    {
                        sample = samples.Dequeue();
                    }

                    history.Enqueue(sample);

                    if (history.Count >= window)
                    {
                        var v = history.Average(s => s.Item1 ? 1 : 0) > .5;
                        var t = history.Peek().Item2;
                        // //If our value switches, track the edge. 
                        // if (v != lastValue)
                        {
                            //     if (lastTicks != null)
                            //     {
                            //         symbols.Add(new RadioSymbol((int) (t - lastTicks), v));
                            //     }
                            //
                            // lastValue = v;
                            // lastTicks = t;
                        }

                        history.Dequeue();
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return symbols.ToArray();
            // return new RadioSymbol[] { };
            bool lowPass(IEnumerable<bool> samples) => samples.Sum( b => b ? 1.0 : 0.0 ) / samples.Count() > .5;
        }
        public Task<RadioSymbol[]> Receive2(CancellationToken token)
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