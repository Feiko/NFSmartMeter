//using nanoFramework.AtomLite;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using nanoFramework.SignalR.Client;
using nanoFramework.Networking;
using System.Net;
using System.IO.Ports;
using System.Text;
using NFSmartMeter.Models;
using nanoFramework.Runtime.Native;



namespace NFSmartMeter
{
    


    public class Program
    {
        
        const string TestDataRaw = @"/XMX5LGF0000453094270

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

        static int freeMemory = 0;
        private static SerialPort _serialDevice;
        private static HubConnection s_hubConnection;
        public static void Main()
        {
            const string Ssid = "testnetwork";
            const string Password = "securepassword1!";



            // Give 60 seconds to the wifi join to happen
            CancellationTokenSource cs = new(60000);
            var success = WiFiNetworkHelper.ScanAndConnectDhcp(Ssid, Password, token: cs.Token, requiresDateTime: true);
            if (!success)
            {
                //AtomLite.NeoPixel.SetColor(Color.Red);
                Thread.Sleep(Timeout.Infinite);
            }

            


            Debug.WriteLine(IPAddress.GetDefaultLocalAddress().ToString());




            //Setup SmartMeter Connection
            //SerialSmartMeterListner p1Listener = new SerialSmartMeterListner(32, 26);
            //p1Listener.P1MessageReceived += P1Listener_P1MessageReceived;

            //Create headers
            var headers = new System.Net.WebSockets.ClientWebSocketHeaders();
            headers["secret"] = "mySecretKey";
            headers["smartMeterId"] = "Meter 1";

            //Reconnect is set to true
            s_hubConnection = new HubConnection("http://192.168.179.2:5001/SmartMeterHub", headers);
            s_hubConnection.Closed += SigClient_Closed;

            ConnectHub();

            //setup serial port
            Configuration.SetPinFunction(26, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(32, DeviceFunction.COM2_RX);

            _serialDevice = new SerialPort("com2", 115200, Parity.None, 8, StopBits.One);
            _serialDevice.ReadTimeout = 2000;
            _serialDevice.ReadBufferSize = 3072;
            _serialDevice.InvertSignalLevels = true;
            //_serialDevice.DataReceived += _serialDevice_DataReceived;
            freeMemory = (int)nanoFramework.Runtime.Native.GC.Run(true);
            _serialDevice.Open();

            //while (true)
            //{
            //    int bytestoRead = _serialDevice.BytesToRead;
            //    if (bytestoRead != 0)
            //    {
            //        //get all bytes
            //        for (; ; )
            //        {
            //            Thread.Sleep(80);
            //            if (bytestoRead < _serialDevice.BytesToRead)
            //            {
            //                bytestoRead = _serialDevice.BytesToRead;
            //            }
            //            else
            //            {
            //                break;
            //            }

            //        }
            //        byte[] buffer = new byte[bytestoRead];
            //        _serialDevice.Read(buffer, 0, buffer.Length);
            //        if (buffer[buffer.Length - 7] == '!')
            //        {
            //            var message = SerialHandler(buffer);
            //            if (message != null)
            //            {
            //                message.PowerConsuming = freeMemory;
            //                P1Listener_P1MessageReceived(null, new P1MessageEventArgs() { EnergyReadout = message });
            //            }
            //            else
            //            {
            //                Debug.WriteLine(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
            //            }
            //            freeMemory = (int)nanoFramework.Runtime.Native.GC.Run(true);
            //        }
            //    }
            //    else
            //    {
            //        Thread.Sleep(100);
            //    }
            //}


            ///// this test runs without loosing memory! So Singalr Client and weboscket library do not seem to be the problem 

            while (true)
            {
                byte[] serialdata = Encoding.UTF8.GetBytes(TestDataRaw);
                string crcString = Encoding.UTF8.GetString(serialdata, serialdata.Length - 6, 4);
                uint crc = hexString2uint(crcString);
                CheckCrc(serialdata, crc);
                var message = P1MessageDecoder.DecodeData(serialdata);
                message.PowerConsuming = freeMemory + 0.1;
                message.P1TimeStamp = DateTime.UtcNow;
                Debug.WriteLine(System.Environment.TickCount64.ToString());
                P1Listener_P1MessageReceived(null, new P1MessageEventArgs() { EnergyReadout = message});
                Debug.WriteLine(System.Environment.TickCount64.ToString());
                Debug.WriteLine(nanoFramework.Runtime.Native.GC.Run(true).ToString());
                Debug.WriteLine(System.Environment.TickCount64.ToString());
                Thread.Sleep(50);
            }

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }


        static bool t_isReceiving = false;
        //private static void _serialDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    if (t_isReceiving) return;
        //    t_isReceiving = true;
        //    var port = (SerialPort)sender;
        //    int bytestoRead = port.BytesToRead;
        //    if (bytestoRead != 0)
        //    {
        //        //get all bytes
        //        for(; ; )
        //        {
        //            Thread.Sleep(30);
        //            if (bytestoRead < port.BytesToRead)
        //            {
        //                bytestoRead = port.BytesToRead;
        //            }
        //            else
        //            {
        //                break;
        //            }

        //        }
        //        byte[] buffer = new byte[bytestoRead];
        //        port.Read(buffer, 0, buffer.Length);
        //        if (buffer[buffer.Length - 7] == '!')
        //        {
        //            var message = SerialHandler(buffer);
        //            if (message != null)
        //            {
        //                message.PowerConsuming = freeMemory;
        //                P1Listener_P1MessageReceived(null, new P1MessageEventArgs() { EnergyReadout = message });
        //            }
        //            freeMemory = (int)nanoFramework.Runtime.Native.GC.Run(true);
        //        }
        //    }

        //    t_isReceiving = false;

        //}


        //public static void ReadSerialPort()
        //{
        //    _serialDevice.Open();

        //    byte[] buffer = new byte[1500/*3072*/];
        //    int index = 0;
        //    int bytesToRead = 0;
        //    bool bufferOverload = false;

        //    while (true)
        //    {
        //        while (!bufferOverload || (index > 7 && buffer[index - 7] != '!'))
        //        {
        //            while (_serialDevice.BytesToRead != 0)
        //            {
        //                bytesToRead = _serialDevice.BytesToRead;
        //                if (bytesToRead + index <= buffer.Length)
        //                {
        //                    index += _serialDevice.Read(buffer, index, bytesToRead);
        //                }
        //                else
        //                {
        //                    bufferOverload = true;
        //                    break;
        //                }
        //                Thread.Sleep(10);
        //            }
        //            if (index > 7 && buffer[index - 7] == '!')
        //            {
        //                break;
        //            }
        //            Thread.Sleep(50);
        //        }

        //        if (!bufferOverload)
        //        {
        //            byte[] messageBuffer = new byte[index];
        //            Array.Copy(buffer, messageBuffer, index);
        //            var message = SerialHandler(messageBuffer);
        //            if(message != null)
        //            {
        //                P1Listener_P1MessageReceived(null, new P1MessageEventArgs() { EnergyReadout = message });
        //            }
        //            //send data

        //            /*EnergyReadoutModel readOut = */ //SerialHandler(messageBuffer);
        //            //if (readOut != null)
        //            //{
        //            //    P1MessageReceived?.Invoke(this, new P1MessageEventArgs() { EnergyReadout = readOut });
        //            //}

        //        }

        //        index = 0;
        //        bufferOverload = false;
        //    }

        //    _serialDevice.Close();
        //}

        public static EnergyReadoutModel SerialHandler(byte[] serialdata)
        {
            string crcString = Encoding.UTF8.GetString(serialdata, serialdata.Length - 6, 4);
            uint crc = hexString2uint(crcString);
            if (CheckCrc(serialdata, crc))
            {
                return P1MessageDecoder.DecodeData(serialdata);
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
                crc ^= buf[pos];    // XOR byte into least sig. byte of crc

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










        private static void P1Listener_P1MessageReceived(object sender, P1MessageEventArgs e)
        {
            if (s_hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    s_hubConnection.SendCore("SendData", new object[] { e.EnergyReadout });
                    //AtomLite.NeoPixel.SetColor(Color.Green);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    //AtomLite.NeoPixel.SetColor(Color.Red);
                }

            }
            else
            {
                //AtomLite.NeoPixel.SetColor(Color.Orange);
            }
        }

        private static void SigClient_Closed(object sender, SignalrEventMessageArgs message)
        {
            Debug.WriteLine($"closed with message {message.Message} \r\n Reconnecting!");
            ConnectHub();
            while (true)
            {
                Thread.Sleep(2000);
                Debug.WriteLine(s_hubConnection.State.ToString());
            }
        }

        private static void ConnectHub()
        {
            //AtomLite.NeoPixel.SetColor(Color.Yellow);
            while (s_hubConnection.State != HubConnectionState.Connected)
            {
                try
                {
                    s_hubConnection.Start();
                    //AtomLite.NeoPixel.SetColor(Color.Blue);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to connect with message: " + ex.Message);
                }

                //sleep 1 minute
                Thread.Sleep(10000);
            }

            Debug.WriteLine("reconnected");
        }
    }
}