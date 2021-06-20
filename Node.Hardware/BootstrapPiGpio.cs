using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Swan.DependencyInjection;
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
            // DependencyContainer.Current.Register<ITiming>((ITiming) new TimingMock());
            // DependencyContainer.Current.Register<IThreading>((IThreading) new Threading());  
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
                   throw new NotImplementedException();
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
               public int PhysicalPinNumber => 0;
               public GpioHeader Header => GpioHeader.None;

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

           public IGpioPin this[int bcmPinNumber] => new PiGpioPin(Board.Pins[bcmPinNumber]);

           public IGpioPin this[BcmPin bcmPin] => new PiGpioPin(Board.Pins[(int)bcmPin]);

           public IGpioPin this[P1 pinNumber] => new PiGpioPin(Board.Pins[(int)pinNumber]);

           public IGpioPin this[P5 pinNumber] => new PiGpioPin(Board.Pins[(int)pinNumber]);
       }
    }
}