using System;
using System.Collections;
using System.Collections.Generic;
using Swan.DependencyInjection;
using Unosquare.RaspberryIO.Abstractions;

namespace HardwareAbstractionServiceRPI.Peripherals
{

    public class BootstrapMock : IBootstrap
    {
        public void Bootstrap()
        {
            DependencyContainer.Current.Register<IGpioController>((IGpioController) new MockGpioController());
            // DependencyContainer.Current.Register<ISpiBus>((ISpiBus) new SpiBus());
            // DependencyContainer.Current.Register<II2CBus>((II2CBus) new I2CBus());
            // DependencyContainer.Current.Register<ISystemInfo>((ISystemInfo) new SystemInfo());
            DependencyContainer.Current.Register<ITiming>((ITiming) new TimingMock());
            // DependencyContainer.Current.Register<IThreading>((IThreading) new Threading()); 
        }
    }

    class TimingMock : ITiming
    {
        public void SleepMilliseconds(uint millis)
        {
        }

        public void SleepMicroseconds(uint micros)
        {
        }

        public uint Milliseconds { get; }
        public uint Microseconds { get; }
    }
   class MockGpio : IGpioPin
        {
            public bool Read()
            {
                return true;
            }

            public void Write(bool value)
            {
                Console.Write(value ? "1": "0");
            }

            public void Write(GpioPinValue value)
            {
                Console.Write(value == GpioPinValue.High ? "1": "0");
            }

            public bool WaitForValue(GpioPinValue status, int timeOutMillisecond)
            {
                return true;
            }

            public void RegisterInterruptCallback(EdgeDetection edgeDetection, Action callback)
            {
            }

            public void RemoveInterruptCallback(EdgeDetection edgeDetection, Action callback)
            {
            }

            public void RegisterInterruptCallback(EdgeDetection edgeDetection, Action<int, int, uint> callback)
            {
            }

            public void RemoveInterruptCallback(EdgeDetection edgeDetection, Action<int, int, uint> callback)
            {
            }
           
            public Dictionary<BcmPin, int> _pinMapping = new Dictionary<BcmPin, int>()
            {
                {BcmPin.Gpio17, 11},
                {BcmPin.Gpio18, 12},
                {BcmPin.Gpio22, 15},
                {BcmPin.Gpio23, 16},
                {BcmPin.Gpio24, 18},
                {BcmPin.Gpio27, 13},
                {BcmPin.Gpio01, 0},
            };
            public BcmPin BcmPin { get; set; }

            public int BcmPinNumber
            {
                get
                {
                    if (_pinMapping.ContainsKey(BcmPin))
                    {
                        return _pinMapping[BcmPin];
                    }

                    return -1;
                }
            }
            public int PhysicalPinNumber => BcmPinNumber;
            public GpioHeader Header { get; } = GpioHeader.P1;
            public GpioPinDriveMode PinMode { get; set; }
            public GpioPinResistorPullMode InputPullMode { get; set; }
            public bool Value { get; set; }
        }

        public class MockGpioController : IGpioController
        {
            public IEnumerator<IGpioPin> GetEnumerator()
            {
                for (int i = 0; i < 50; ++i)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count { get; } = 20;

            private Dictionary<int, IGpioPin> _pins = new Dictionary<int, IGpioPin>();

            public IGpioPin this[int bcmPinNumber]
            {
                get
                {
                    if (_pins.ContainsKey(bcmPinNumber))
                    {
                        return _pins[bcmPinNumber];
                    }
                    
                    _pins[bcmPinNumber] = new MockGpio() {BcmPin = (BcmPin)bcmPinNumber};
                    return _pins[bcmPinNumber];
                }
            }

            public IGpioPin this[BcmPin bcmPin] => this[(int)bcmPin];

            public IGpioPin this[P1 pinNumber] => this[(int)pinNumber];

            public IGpioPin this[P5 pinNumber] => this[(int)pinNumber];
        }
}