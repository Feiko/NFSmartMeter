using nanoFramework.Hardware.Esp32;
using NFSmartMeter.Models;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace NFSmartMeter
{
    public  class SerialSmartMeterListner
    {
        private SerialPort _serialDevice;
        public event P1MessageEventHandler P1MessageReceived;
        public delegate void P1MessageEventHandler (object sender, P1MessageEventArgs e);

        public SerialSmartMeterListner(int rxPin, int txPin)
        {
            Configuration.SetPinFunction(txPin, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(rxPin, DeviceFunction.COM2_RX);

            _serialDevice = new SerialPort("com2", 115200, Parity.None, 8, StopBits.One);
            _serialDevice.ReadTimeout = 2000;
            _serialDevice.InvertSignalLevels = true;

            Thread listenTread = new Thread(ReadSerialPort);
            listenTread.Start();
        }

        private  void ReadSerialPort()
        {
            _serialDevice.Open();

            byte[] buffer = new byte[3072];
            int index = 0;
            int bytesToRead = 0;
            bool bufferOverload = false;

            while (true)
            {
                while (!bufferOverload || (index > 7 && buffer[index - 7] != '!'))
                {
                    while (_serialDevice.BytesToRead != 0)
                    {
                        bytesToRead = _serialDevice.BytesToRead;
                        if (bytesToRead + index <= buffer.Length)
                        {
                            index += _serialDevice.Read(buffer, index, bytesToRead);
                        }
                        else
                        {
                            bufferOverload = true;
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    if (index > 7 && buffer[index - 7] == '!')
                    {
                        break;
                    }
                }

                if (!bufferOverload)
                {
                    byte[] messageBuffer = new byte[index];
                    Array.Copy(buffer, messageBuffer, index);
                    EnergyReadoutModel readOut = SerialHandler(messageBuffer);
                    if (readOut != null)
                    {
                        P1MessageReceived?.Invoke(this, new P1MessageEventArgs() { EnergyReadout = readOut });
                    }

                }

                index = 0;
                bufferOverload = false;
            }

            _serialDevice.Close();
        }

        public static EnergyReadoutModel SerialHandler(byte[] serialdata)
        {
            string crcString = Encoding.UTF8.GetString(serialdata, serialdata.Length - 6, 4);
            uint crc = hexString2uint(crcString);
            if (CheckCrc(serialdata, crc))
            {
                return P1MessageDecoder.DecodeData(Encoding.UTF8.GetString(serialdata, 0, serialdata.Length - 6));

            }
            else
            {
                return null;
            }
        }

        private static uint hexString2uint(string crcString)
        {
            if (crcString.Length != 4)
            {
                return 0; //uneven Number can not be a hex representation
            }
            uint[] partNums = new uint[crcString.Length];

            for (int i = 0; i < crcString.Length; i++)
            {
                switch (crcString[i])
                {
                    case '0':
                        partNums[i] = 0;
                        break;
                    case '1':
                        partNums[i] = 1;
                        break;
                    case '2':
                        partNums[i] = 2;
                        break;
                    case '3':
                        partNums[i] = 3;
                        break;
                    case '4':
                        partNums[i] = 4;
                        break;
                    case '5':
                        partNums[i] = 5;
                        break;
                    case '6':
                        partNums[i] = 6;
                        break;
                    case '7':
                        partNums[i] = 7;
                        break;
                    case '8':
                        partNums[i] = 8;
                        break;
                    case '9':
                        partNums[i] = 9;
                        break;
                    case 'A':
                        partNums[i] = 10;
                        break;
                    case 'B':
                        partNums[i] = 11;
                        break;
                    case 'C':
                        partNums[i] = 12;
                        break;
                    case 'D':
                        partNums[i] = 13;
                        break;
                    case 'E':
                        partNums[i] = 14;
                        break;
                    case 'F':
                        partNums[i] = 15;
                        break;
                    default:
                        return 0;
                }
            }

            return BitConverter.ToUInt16(new byte[] { (byte)(partNums[2] * 16 + partNums[3]), (byte)(partNums[0] * 16 + partNums[1]) }, 0);

        }

        private static bool CheckCrc(byte[] buf, uint givenCrc)
        {

            uint crc = 0;
            // -6 because we are reusing the original byte[] because of memory conservation
            for (int pos = 0; pos < buf.Length - 6; pos++)
            {
                crc ^= (uint)buf[pos];    // XOR byte into least sig. byte of crc

                for (int i = 8; i != 0; i--)
                {    // Loop over each bit
                    if ((crc & 0x0001) != 0)
                    {      // If the LSB is set
                        crc >>= 1;                    // Shift right and XOR 0xA001
                        crc ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        crc >>= 1;                    // Just shift right
                }
            }

            return crc == givenCrc;
        }



    }

    public class P1MessageEventArgs : EventArgs
    {
        public EnergyReadoutModel EnergyReadout { get; set; }
    }
}
