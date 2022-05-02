using BetaSoft.EPaperHatCore.IO;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
//using System.Device.Gpio;
//using System.Device.Spi;

namespace BetaSoft.EPaperHatCore
{
    internal class Connections
    {
        private readonly IHardwareSpecification _specification;
        public IGpioPin ResetPin { get; private set; }
        public IGpioPin DcPin { get; private set; }
        public IGpioPin CsPin { get; private set; }
        public IGpioPin BusyPin { get; private set; }
        public ISpiChannel Channel { get; private set; }
        private static readonly object _syncLock = new object();


        /******* System.Device.Gpio Alternative *********/
        /*
        public readonly GpioController Gpio;
        private readonly SpiConnectionSettings _spiSettings;
        public int ResetPin { get; private set; }
        public int DcPin { get; private set; }
        public int CsPin { get; private set; }
        public int BusyPin { get; private set; }
        public SpiDevice SpiDevice { get; private set; }
        */

        public Connections(IHardwareSpecification specification)
        {
            _specification = specification;

            /******* System.Device.Gpio Alternative *********/
            /*
            Gpio = new GpioController(PinNumberingScheme.Logical);
            _spiSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = _specification.Channel0Frequency,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };
            */
        }

        public void Initialize()
        {
            lock(_syncLock)
            {
                ResetPin = Pi.Gpio[_specification.RST_PIN];
                ResetPin.PinMode = GpioPinDriveMode.Output;

                DcPin = Pi.Gpio[_specification.DC_PIN];
                DcPin.PinMode = GpioPinDriveMode.Output;

                CsPin = Pi.Gpio[_specification.CS_PIN];
                CsPin.PinMode = GpioPinDriveMode.Output;

                BusyPin = Pi.Gpio[_specification.BUSY_PIN];
                BusyPin.PinMode = GpioPinDriveMode.Input;

                Pi.Spi.Channel0Frequency = _specification.Channel0Frequency;
                Channel = Pi.Spi.Channel0;
                

                /******* System.Device.Gpio Alternative *********/
                //ResetPin = _specification.RST_PIN;
                //Gpio.OpenPin(ResetPin, PinMode.Output);

                //DcPin = _specification.DC_PIN;
                //Gpio.OpenPin(DcPin, PinMode.Output);

                //CsPin = _specification.CS_PIN;
                //Gpio.OpenPin(CsPin, PinMode.Output);

                //BusyPin = _specification.BUSY_PIN;
                //Gpio.OpenPin(BusyPin, PinMode.Input);

                //Gpio.Write(CsPin, PinValue.High);

                //SpiDevice = SpiDevice.Create(_spiSettings);
            }
        }

        public void Close()
        {
            ResetPin = Pi.Gpio[_specification.RST_PIN];
            ResetPin.Write(false);

            DcPin = Pi.Gpio[_specification.DC_PIN];
            DcPin.Write(false);

            CsPin = Pi.Gpio[_specification.CS_PIN];
            CsPin.Write(false);



            /******* System.Device.Gpio Alternative *********/
            //Gpio.Write(_specification.RST_PIN, PinValue.Low);
            //Gpio.Write(_specification.DC_PIN, PinValue.Low);
            //Gpio.Write(_specification.CS_PIN, PinValue.Low);
            //Gpio.Dispose();
        }
    }
}