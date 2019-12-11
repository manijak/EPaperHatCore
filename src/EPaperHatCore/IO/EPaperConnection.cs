using System;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
//using System.Device.Gpio;

namespace BetaSoft.EPaperHatCore.IO
{
    internal class EPaperConnection : IEpaperConnection
    {
        private readonly Connections _connection;
        private static readonly object _syncLock = new object();

        public EPaperConnection(Connections connection)
        {
            _connection = connection;
        }

        public void SendCommand(int command)
        {
            lock(_syncLock)
            {
                _connection.DcPin.Write(GpioPinValue.Low);
                _connection.CsPin.Write(GpioPinValue.Low);
                _connection.Channel.SendReceive(BitConverter.GetBytes(command));
                _connection.CsPin.Write(GpioPinValue.High);


                /******* System.Device.Gpio Alternative *********/
                //_connection.Gpio.Write(_connection.DcPin, PinValue.Low);
                //_connection.Gpio.Write(_connection.CsPin, PinValue.Low);
                //_connection.SpiDevice.Write(BitConverter.GetBytes(command));
                //_connection.Gpio.Write(_connection.CsPin, PinValue.High);
            }
        }

        public void SendData(int data)
        {
            lock(_syncLock)
            {
                _connection.DcPin.Write(GpioPinValue.High);
                _connection.CsPin.Write(GpioPinValue.Low);
                _connection.Channel.SendReceive(BitConverter.GetBytes(data));
                _connection.CsPin.Write(GpioPinValue.High);


                /******* System.Device.Gpio Alternative *********/
                //_connection.Gpio.Write(_connection.DcPin, PinValue.High);
                //_connection.Gpio.Write(_connection.CsPin, PinValue.Low);
                //_connection.SpiDevice.Write(BitConverter.GetBytes(data));
                //_connection.Gpio.Write(_connection.CsPin, PinValue.High);
            }
        }
    }
}