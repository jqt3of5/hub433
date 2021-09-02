using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Swan.DependencyInjection;
using Swan.Diagnostics;
using Unosquare.PiGpio;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;
using Unosquare.RaspberryIO.Abstractions;
using EdgeDetection = Unosquare.RaspberryIO.Abstractions.EdgeDetection;

namespace Node.Hardware
{
    public class BootstrapPiGpio : IBootstrap
    {
        public void Bootstrap()
        {
            if (Setup.GpioInitialise() < 0)
            {
                System.Console.WriteLine("Failed to initialize. was your libpigpio.so built from a raspberrypi 4?");
                Setup.GpioTerminate();
                return;
            }
            
            DependencyContainer.Current.Register<IGpioController>((IGpioController) new PiGpioController());
            // DependencyContainer.Current.Register<ISpiBus>((ISpiBus) new SpiBus());
            // DependencyContainer.Current.Register<II2CBus>((II2CBus) new I2CBus());
            // DependencyContainer.Current.Register<ISystemInfo>((ISystemInfo) new SystemInfo());
            DependencyContainer.Current.Register<ITiming>((ITiming) new PiTiming());
            // DependencyContainer.Current.Register<IThreading>((IThreading) new Threading());  
        }

        public class PiTiming : ITiming
        {
            public void SleepMilliseconds(uint millis)
            {
                var timer = new HighResolutionTimer();
                timer.Start();

                while (timer.ElapsedMilliseconds < millis) ;
            }

            public void SleepMicroseconds(uint micros)
            {
                var timer = new HighResolutionTimer();
                timer.Start();

                while (timer.ElapsedMicroseconds < micros) ;
            }

            public uint Milliseconds { get; }
            public uint Microseconds { get; }
        }
       public class PiGpioController : IGpioController
       {
           public class PiGpioPin : IGpioPin
           {
               private readonly GpioPin _pin;

               public PiGpioPin(GpioPin pin)
               {
                   _pin = pin;
               }

               public override bool Equals(object? obj)
               {
                   if (obj is IGpioPin pin)
                   {
                       return BcmPin == pin.BcmPin;
                   }
                   return false;
               }

               public bool Read()
               {
                   return _pin.Read() > 0;
               }

               public void Write(bool value)
               {
                   _pin.Write(value ? 1 : 0);
               }

               public void Write(GpioPinValue value)
               {
                   switch (value)
                   {
                       case GpioPinValue.High:
                           _pin.Write(1);
                           break;
                       case GpioPinValue.Low:
                           _pin.Write(0);
                           break;
                       default:
                           throw new ArgumentOutOfRangeException(nameof(value), value, null);
                   }
               }

               public bool WaitForValue(GpioPinValue status, int timeOutMillisecond)
               {
                   var sw = new Stopwatch();
                   sw.Start();
                   while (sw.ElapsedMilliseconds < timeOutMillisecond)
                   {
                       var read = Read();
                       switch (status)
                       {
                           case GpioPinValue.High:
                               if (read)
                               {
                                   return true;
                               }
                               
                               break;
                           case GpioPinValue.Low:
                               if (!read)
                               {
                                   return true;
                               }

                               break;
                       }
                   }

                   return false;
               }

               public void RegisterInterruptCallback(EdgeDetection edgeDetection, Action callback)
               {
                   throw new NotImplementedException();
               }

               public void RemoveInterruptCallback(EdgeDetection edgeDetection, Action callback)
               {
                   throw new NotImplementedException();
               }

               public void RegisterInterruptCallback(EdgeDetection edgeDetection, Action<int, int, uint> callback)
               {
                   throw new NotImplementedException();
               }

               public void RemoveInterruptCallback(EdgeDetection edgeDetection, Action<int, int, uint> callback)
               {
                   throw new NotImplementedException();
               }

               public BcmPin BcmPin => (BcmPin) _pin.PinNumber;
               public int BcmPinNumber => _pin.PinNumber;

               private int[] BcmNumberToPhysicalPinMapping = new[]
               {
                   27, 28, 3, 5, 7, 29, 31, 26, 24, 21, 19, 23, 32, 33, 8, 10, 36, 11, 12, 35, 38, 40, 15, 16, 18, 22,
                   37, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
               };
               public int PhysicalPinNumber
               {
                   get
                   {
                       var pin = BcmNumberToPhysicalPinMapping[BcmPinNumber];
                       return pin;
                   }
               }

               public GpioHeader Header => GpioHeader.P1;

               public GpioPinDriveMode PinMode
               {
                   get
                   {
                       switch (_pin.Mode)
                       {
                           case Unosquare.PiGpio.NativeEnums.PinMode.Input:
                               return GpioPinDriveMode.Input;
                           case Unosquare.PiGpio.NativeEnums.PinMode.Output:
                               return GpioPinDriveMode.Output;
                           case Unosquare.PiGpio.NativeEnums.PinMode.Alt5:
                               return GpioPinDriveMode.GpioClock;
                           case Unosquare.PiGpio.NativeEnums.PinMode.Alt4:
                               return GpioPinDriveMode.PwmOutput;
                           case Unosquare.PiGpio.NativeEnums.PinMode.Alt0:
                               return GpioPinDriveMode.Alt0;
                           case Unosquare.PiGpio.NativeEnums.PinMode.Alt1:
                               return GpioPinDriveMode.Alt1;
                           case Unosquare.PiGpio.NativeEnums.PinMode.Alt2:
                               return GpioPinDriveMode.Alt2;
                           case Unosquare.PiGpio.NativeEnums.PinMode.Alt3:
                               return GpioPinDriveMode.Alt3;
                           default:
                               throw new ArgumentOutOfRangeException();
                       }
                   }
                   set
                   {
                       switch (value)
                       {
                           case GpioPinDriveMode.Input:
                               IO.GpioSetMode(_pin.PinGpio, Unosquare.PiGpio.NativeEnums.PinMode.Input);
                               break;
                           case GpioPinDriveMode.Output:
                               IO.GpioSetMode(_pin.PinGpio, Unosquare.PiGpio.NativeEnums.PinMode.Output);
                               break;
                           case GpioPinDriveMode.PwmOutput:
                               // IO.GpioSetMode(_pin.PinGpio, Unosquare.PiGpio.NativeEnums.PinMode.Alt4);
                               break;
                           case GpioPinDriveMode.GpioClock:
                               // IO.GpioSetMode(_pin.PinGpio, Unosquare.PiGpio.NativeEnums.PinMode.Input);
                               break;
                           case GpioPinDriveMode.Alt0:
                               IO.GpioSetMode(_pin.PinGpio, Unosquare.PiGpio.NativeEnums.PinMode.Alt0);
                               break;
                           case GpioPinDriveMode.Alt1:
                               IO.GpioSetMode(_pin.PinGpio, Unosquare.PiGpio.NativeEnums.PinMode.Alt1);
                               break;
                           case GpioPinDriveMode.Alt2:
                               IO.GpioSetMode(_pin.PinGpio, Unosquare.PiGpio.NativeEnums.PinMode.Alt2);
                               break;
                           case GpioPinDriveMode.Alt3:
                               IO.GpioSetMode(_pin.PinGpio, Unosquare.PiGpio.NativeEnums.PinMode.Alt3);
                               break;
                           default:
                               throw new ArgumentOutOfRangeException(nameof(value), value, null);
                       }
                   }
               }

               public GpioPinResistorPullMode InputPullMode
               {
                   get
                   {
                       return _pin.PullMode switch
                       {
                           GpioPullMode.Off => GpioPinResistorPullMode.Off,
                           GpioPullMode.Down => GpioPinResistorPullMode.PullDown,
                           GpioPullMode.Up => GpioPinResistorPullMode.PullUp,
                           _ => throw new ArgumentOutOfRangeException()
                       };
                   }
                   set
                   {
                       _pin.PullMode = value switch
                       {
                           GpioPinResistorPullMode.Off => GpioPullMode.Off,
                           GpioPinResistorPullMode.PullDown => GpioPullMode.Down,
                           GpioPinResistorPullMode.PullUp => GpioPullMode.Up,
                           _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                       };
                   }
               }

               public bool Value
               {
                   get => _pin.Value;
                   set => _pin.Value = value;
               }
           }
           public IEnumerator<IGpioPin> GetEnumerator()
           {
               foreach (var pin in Board.Pins)
               {
                   yield return new PiGpioPin(pin.Value);
               }
           }

           IEnumerator IEnumerable.GetEnumerator()
           {
               return GetEnumerator();
           }

           public int Count => Board.Pins.Count;

           private Dictionary<int, IGpioPin> _pins = new Dictionary<int, IGpioPin>();

           public IGpioPin this[int bcmPinNumber]
           {
               get
               {
                   if (!_pins.ContainsKey(bcmPinNumber))
                   {
                     _pins[bcmPinNumber] = new PiGpioPin(Board.Pins[bcmPinNumber]);  
                   }
                   return _pins[bcmPinNumber];
               }
           } 

           public IGpioPin this[BcmPin bcmPin] => this[(int) bcmPin];

           public IGpioPin this[P1 pinNumber] => this[(int) pinNumber];

           public IGpioPin this[P5 pinNumber] => this[(int) pinNumber];
       }
    }
}