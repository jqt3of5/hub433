using System;
using System.Collections.Generic;
using System.Linq;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.RaspberryIO.Abstractions;

namespace RPINode
{
    public class Receiver433
    {
        private readonly GpioPin _pin;

        public Receiver433(GpioPin pin)
        {
            _pin = pin;
            _pin.Direction = PinDirection.Input;
        }

        public RadioSymbol[] Receive(TimeSpan timeout)
        {
            List<TimeSpan> sampleIntervals = new List<TimeSpan>() {TimeSpan.Zero};
            List<bool> samples = new List<bool>();
            var startTime = DateTime.Now;
            while (sampleIntervals.Last() < timeout)
            {
                for (int i = 0; i < 100; ++i)
                {
                    samples.Add(_pin.Value); 
                }
                //Is this accurate enough to measure these reads? Difficult to say.  
                sampleIntervals.Add(DateTime.Now - startTime);
            }
            //TODO: Process the results and convert into radio symbols
            return new RadioSymbol[] { };
        }
        
    }
}