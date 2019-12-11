using System;
using BetaSoft.EPaperHatCore.GUI;
using BetaSoft.EPaperHatCore.IO;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
//using System.Device.Gpio;
//using System.Threading;

namespace BetaSoft.EPaperHatCore
{
    /// <summary>
    /// Driver class for the Waveshare 7.5" 3-color ePaper display (B and C versions)
    /// TODO: Each driver class should probably inherit from a base class to implement the common parts
    /// </summary>
    public class Epaper_7in5bc
    {
        private readonly IEpaperConnection _ePaperConnection;
        private readonly Connections _connections;
        public Epaper_7in5bc(int screenWidth, int screenHeight, IHardwareSpecification specification = null)
        {
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                throw new ArgumentException("Width and/or height cannot be less or equal zero");
            }
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            _connections = new Connections(specification ?? new DefaultSpecification());
            _ePaperConnection = new EPaperConnection(_connections);
        }

        public int ScreenWidth { get; }
        public int ScreenHeight { get; }

        public void Initialize()
        {
            Console.WriteLine("Initialize 7.5inch Display");
            _connections.Initialize();

            Reset();


            Console.WriteLine("Set POWER_SETTING");
            _ePaperConnection.SendCommand(HardwareCodes.POWER_SETTING);
            _ePaperConnection.SendData(0x37);                 
            _ePaperConnection.SendData(0x00);


            Console.WriteLine("Set PANEL_SETTING");
            _ePaperConnection.SendCommand(HardwareCodes.PANEL_SETTING);
            _ePaperConnection.SendData(0xCF);
            _ePaperConnection.SendData(0x08);


            Console.WriteLine("Set PLL_CONTROL");
            _ePaperConnection.SendCommand(HardwareCodes.PLL_CONTROL);
            _ePaperConnection.SendData(0x3A);       // PLL:  0-15:0x3C, 15+:0x3A


            Console.WriteLine("Set VCM_DC_SETTING");
            _ePaperConnection.SendCommand(HardwareCodes.VCM_DC_SETTING_REGISTER);
            _ePaperConnection.SendData(0x28);       //all temperature  range


            Console.WriteLine("Set BOOSTER_SOFT_START");
            _ePaperConnection.SendCommand(HardwareCodes.BOOSTER_SOFT_START);
            _ePaperConnection.SendData(0xc7);
            _ePaperConnection.SendData(0xcc);
            _ePaperConnection.SendData(0x15);


            Console.WriteLine("Set VCOM_AND_DATA_INTERVAL_SETTING");
            _ePaperConnection.SendCommand(HardwareCodes.VCOM_AND_DATA_INTERVAL_SETTING);
            _ePaperConnection.SendData(0x77);


            Console.WriteLine("Set TCON_SETTING");
            _ePaperConnection.SendCommand(HardwareCodes.TCON_SETTING);
            _ePaperConnection.SendData(0x22);


            Console.WriteLine("Set FLASH_CONTROL");
            _ePaperConnection.SendCommand(HardwareCodes.FLASH_CONTROL);
            _ePaperConnection.SendData(0x00);


            Console.WriteLine("Set TCON_RESOLUTION");
            _ePaperConnection.SendCommand(HardwareCodes.TCON_RESOLUTION);
            _ePaperConnection.SendData(ScreenWidth >> 8);      //source 640
            _ePaperConnection.SendData(ScreenWidth & 0xff);
            _ePaperConnection.SendData(ScreenHeight >> 8);     //gate 384
            _ePaperConnection.SendData(ScreenHeight & 0xff);


            Console.WriteLine("Set FLASH_MODE");
            _ePaperConnection.SendCommand(HardwareCodes.FLASH_MODE);
            _ePaperConnection.SendData(0x03);
        }

        public void Disconnect()
        {
            Console.WriteLine("Disconnecting...");
            _connections.Close();
        }

        private void TurnOnDisplay()
        {
            Console.WriteLine("TurnOnDisplay");

            Console.WriteLine("Send POWER_ON");
            _ePaperConnection.SendCommand(HardwareCodes.POWER_ON);
            WaitUntilIdle();

            Console.WriteLine("Send DISPLAY_REFRESH");
            _ePaperConnection.SendCommand(HardwareCodes.DISPLAY_REFRESH);

            //Thread.Sleep(100);
            Pi.Timing.SleepMilliseconds(100);

            WaitUntilIdle();
        }

        private void Reset()
        {
            Console.WriteLine("Reseting...");

            _connections.ResetPin.Write(GpioPinValue.High);
            Pi.Timing.SleepMilliseconds(200);
            _connections.ResetPin.Write(GpioPinValue.Low);
            Pi.Timing.SleepMilliseconds(10); 
            _connections.ResetPin.Write(GpioPinValue.High);
            Pi.Timing.SleepMilliseconds(200);


            /******* System.Device.Gpio Alternative *********/
            /*
            _connections.Gpio.Write(_connections.ResetPin, PinValue.High);
            Thread.Sleep(200);
            _connections.Gpio.Write(_connections.ResetPin, PinValue.Low);
            Thread.Sleep(10);
            _connections.Gpio.Write(_connections.ResetPin, PinValue.High);
            Thread.Sleep(200);
            */
        }

        private void WaitUntilIdle()
        {
            Console.WriteLine("e-Paper is busy");

            while (_connections.BusyPin.Read() == false) // Very confusing, is busy=true LOW or HIGH?
            {
                _ePaperConnection.SendCommand(HardwareCodes.GET_STATUS);
                Pi.Timing.SleepMilliseconds(100);
                //Thread.Sleep(100);
                Console.Write(".");
            }

            //Console.WriteLine("busy release");
            //Thread.Sleep(200);
            Pi.Timing.SleepMilliseconds(200);
        }

        public void ClearScreen()
        {
            Console.WriteLine("Clear display");

            int Width, Height;
            Width = (ScreenWidth % 8 == 0) ? (ScreenWidth / 8) : (ScreenWidth / 8 + 1);
            Height = ScreenHeight;

            _ePaperConnection.SendCommand(HardwareCodes.DATA_START_TRANSMISSION_1);
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        _ePaperConnection.SendData(0x33);
                    }
                }
            }
            //_ePaperConnection.SendCommand(HardwareCodes.DATA_STOP);

            TurnOnDisplay();
        }

        public void DisplayScreens(Screen blackScreen, Screen redYellowScreen)
        {
            Console.WriteLine("Display Screens");

            if (blackScreen?.Image == null)
                throw new ArgumentNullException(nameof(blackScreen));
            if (redYellowScreen?.Image == null)
                throw new ArgumentNullException(nameof(redYellowScreen));

            Console.WriteLine("> BlackImage size: " + blackScreen.Image.Length);
            Console.WriteLine("> YellowImage size: " + redYellowScreen.Image.Length);

            Display(blackScreen, redYellowScreen);

            TurnOnDisplay();
        }

        private void Display(Screen blackScreen, Screen redYellowScreen)
        {
            int data_black, data_ry, data;
            int width, height;
            width = (ScreenWidth % 8 == 0) ? (ScreenWidth / 8) : (ScreenWidth / 8 + 1);
            height = ScreenHeight;

            _ePaperConnection.SendCommand(HardwareCodes.DATA_START_TRANSMISSION_1);

            Console.WriteLine("--- Sending image data... ---");
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    data_black = blackScreen.Image[i + j * width];
                    data_ry = redYellowScreen.Image[i + j * width]; // Red or Yellow

                    for (int k = 0; k < 8; k++)
                    {
                        if((data_ry & 0x80) == 0x00)
                        {
                            data = 0x04;    // RED
                        }
                        else if ((data_black & 0x80) == 0x00)
                        {
                            data = 0x00;    // BLACK
                        }
                        else
                        {
                            data = 0x03;    // WHITE
                        }

                        data = (data << 4) & 0xFF;
                        data_black = (data_black << 1) & 0xFF;
                        data_ry = (data_ry << 1) & 0xFF;
                        k += 1;

                        if ((data_ry & 0x80) == 0x00)
                        {
                            data |= 0x04;   // RED
                        }
                        else if ((data_black & 0x80) == 0x00)
                        {
                            data |= 0x00;   // BLACK
                        }
                        else
                        {
                            data |= 0x03;   // WHITE
                        }

                        data_black = (data_black << 1) & 0xFF;
                        data_ry = (data_ry << 1) & 0xFF;

                        _ePaperConnection.SendData(data);
                    }
                }
            }
            Console.WriteLine("--- DONE sending image data ---");
        }

        public void Sleep()
        {
            Console.WriteLine("e-Paper display going to sleep...");

            _ePaperConnection.SendCommand(HardwareCodes.POWER_OFF);
            WaitUntilIdle();

            _ePaperConnection.SendCommand(HardwareCodes.DEEP_SLEEP);
            _ePaperConnection.SendData(0xA5);
        }
    }
}
