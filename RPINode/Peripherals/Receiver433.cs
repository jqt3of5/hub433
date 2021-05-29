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
        private readonly IGpioPin _pin;

        public Receiver433(IGpioPin pin)
        {
            _pin = pin;
            _pin.PinMode = GpioPinDriveMode.Input;
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
                    samples.Add(_pin.Read()); 
                }
                //Is this accurate enough to measure these reads? Difficult to say.  
                sampleIntervals.Add(DateTime.Now - startTime);
            }
            //TODO: Process the results and convert into radio symbols
            return new RadioSymbol[] { };
        }
        
    }
}