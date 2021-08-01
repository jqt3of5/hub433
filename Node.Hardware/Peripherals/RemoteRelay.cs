﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace Node.Hardware.Peripherals
{
    public class RemoteRelay
    { 
        private readonly Transmitter433 _transmitter433;
       
       public RemoteRelay(Transmitter433 transmitter433)
       {
           _transmitter433 = transmitter433;
       }
 
       public async Task Pair(int channel)
       {
           
       }

       public async Task Pwm(int channel, (int port, float dutyCycle, float cyclesPerSecond) [] values)
       {
           
       }
       
       public async Task On(int channel, int [] ports)
       {
           
       }

       public async Task Off(int channel, int [] ports)
       {
           
       }
    }
}