using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        }

        public RadioSymbol[] Receive(TimeSpan timeout)
        {
            List<bool> samples = new List<bool>(10000);
            var sw = new Stopwatch();
            var ticks = 0L;
            while (sw.Elapsed < timeout)
            {
                for (int i = 0; i < 100; ++i)
                {
                    samples.Add(_pin.Value); 
                    // ReSharper disable once EmptyEmbeddedStatement
                    ticks = sw.ElapsedTicks + 9;
                    while (sw.ElapsedTicks < ticks);
                }
            }

            const int window = 5;
            var history = new Queue<bool>();
            bool? value = null;
            for(var i = 0; i < samples.Count; ++i)
            {
                 history.Enqueue(samples[i]);
                 if (history.Count < window)
                     //Don't run the algorithm if our queue isn't full 
                     continue;

                 var v = lowPass(history.ToArray());
                 if (v != value)
                 {
                     value = v;
                 }
                 
                 history.Dequeue();
            }

            bool lowPass(IEnumerable<bool> samples) => samples.Sum( b => b ? 1.0 : 0.0 ) / samples.Count() > .5;
            
            //TODO: Process the results and convert into radio symbols
            return new RadioSymbol[] { };
        }
        
    }
}