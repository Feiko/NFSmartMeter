using nanoFramework.Hardware.Esp32;
using NFSmartMeter.Models;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Collections;
using Windows.Storage.Streams;

namespace NFSmartMeter
{
    public  class SerialSmartMeterListner
    {
        private object _lock = new object();
        private Queue Messages = new Queue();
        private SerialPort _serialDevice;
        public event P1MessageEventHandler P1MessageReceived;
        public delegate void P1MessageEventHandler (object sender, P1MessageEventArgs e);
        private static P1MessageDecoder _decoder = new P1MessageDecoder();

        public SerialSmartMeterListner(int rxPin, int txPin)
        {
            Configuration.SetPinFunction(txPin, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(rxPin, DeviceFunction.COM2_RX);

            _serialDevice = new SerialPort("com2", 115200, Parity.None, 8, StopBits.One);
            _serialDevice.ReadTimeout = 2000;
            _serialDevice.InvertSignalLevels = true;

            Thread listenTread = new Thread(ReadSerialPort);
            listenTread.Start();

            Thread DecodePayloadThread = new Thread(SerialHandler);
            DecodePayloadThread.Start();
        }

        private  void ReadSerialPort()
        {
            //var message = _decoder.DecodeData(TestDataRaw);

            //while (true)
            //{
            //    P1MessageReceived?.Invoke(this, new P1MessageEventArgs() { EnergyReadout = message });
            //    Thread.Sleep(1000);
            //}
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
                    Thread.Sleep(10);
                }

                if (!bufferOverload)
                {
                    byte[] messageBuffer = new byte[index];
                    Array.Copy(buffer, messageBuffer, index);
                    lock (_lock)
                    {
                        Messages.Enqueue(messageBuffer);
                    }
                    /*EnergyReadoutModel readOut = */ //SerialHandler(messageBuffer);
                    //if (readOut != null)
                    //{
                    //    P1MessageReceived?.Invoke(this, new P1MessageEventArgs() { EnergyReadout = readOut });
                    //}

                }

                index = 0;
                bufferOverload = false;
            }

            _serialDevice.Close();
        }

        public void /*EnergyReadoutModel*/ SerialHandler(/*byte[] serialdata*/)
        {
            while (true)
            {
                while (Messages.Count == 0)
                {
                    Thread.Sleep(150);
                }

                byte[] serialdata;
                lock (_lock)
                {
                    serialdata = (byte[])Messages.Dequeue();
                }
                string crcString = Encoding.UTF8.GetString(serialdata, serialdata.Length - 6, 4);
                uint crc = hexString2uint(crcString);
                if (CheckCrc(serialdata, crc))
                {
                    var readOut = _decoder.DecodeData(Encoding.UTF8.GetString(serialdata, 0, serialdata.Length - 6));
                    P1MessageReceived?.Invoke(this, new P1MessageEventArgs() { EnergyReadout = readOut });

                }
                
            }
            //else
            //{
            //    return null;
            //}
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

        public const string TestDataRaw = @"/XMX5LGF0000453094270

1-3:0.2.8(50)
0-0:1.0.0(210304120347W)
0-0:96.1.1(4530303531303035333039343237303139)
1-0:1.8.1(001819.387*kWh)
1-0:1.8.2(002093.302*kWh)
1-0:2.8.1(000088.650*kWh)
1-0:2.8.2(000157.206*kWh)
0-0:96.14.0(0002)
1-0:1.7.0(00.288*kW)
1-0:2.7.0(00.000*kW)
0-0:96.7.21(00015)
0-0:96.7.9(00002)
1-0:99.97.0(1)(0-0:96.7.19)(190226161118W)(0000000541*s)
1-0:32.32.0(00019)
1-0:32.36.0(00002)
0-0:96.13.0()
1-0:32.7.0(231.0*V)
1-0:31.7.0(001*A)
1-0:21.7.0(00.288*kW)
1-0:22.7.0(00.000*kW)
0-1:24.1.0(003)
0-1:96.1.0(4730303339303031393231393034393139)
0-1:24.2.1(210304120005W)(01980.598*m3)
!894F  ";

    }

    public class P1MessageEventArgs : EventArgs
    {
        public EnergyReadoutModel EnergyReadout { get; set; }
    }
}
