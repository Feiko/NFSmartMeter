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
        private static Timer _readTimer;
        private static SerialPort _serialDevice;
        private static HubConnection s_hubConnection;

        private static int _bytesRead;
        private static byte[] _workBuffer = new byte[4000];
        private static AutoResetEvent _dataAvailable = new AutoResetEvent(false);

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
            //_serialDevice.WatchChar = '!';
            //_serialDevice.DataReceived += _serialDevice_DataReceived;
            int freeMemory = (int)nanoFramework.Runtime.Native.GC.Run(true);
            _serialDevice.Open();
            SetTimer();

            //new Thread(ProcessIncommingBuffer).Start();


            // TODO
            // this will go away, possibly reuse some code in the ProcessIncommingBuffer

            //byte[] tempBuffer = new byte[4096];
            //int tempIndex = 0;
            //bool tempSecondLoop = false;
            //while (true)
            //{
            //    int tempBytesToRead = _serialDevice.BytesToRead;
            //    if(tempBytesToRead > 0)
            //    {
            //        _serialDevice.Read(tempBuffer, tempIndex, tempBytesToRead);
            //        tempIndex += tempBytesToRead;

            //        if (tempIndex > 8 && tempBuffer[tempIndex - 7] == '!')
            //        {
            //            if (tempSecondLoop)
            //            {
            //                break;
            //            }
            //            else
            //            {
            //                tempSecondLoop = true;
            //            }
            //        }
            //    }
            //}
            //Thread.Sleep(50);
            //_readTimer = new Timer(ReadSerial, null, 1000, 1000);

            //while (true)
            //{
            //    int bytestoRead = _serialDevice.BytesToRead;
            //    if (bytestoRead != 0)
            //    {
            //        //get all bytes
            //        for (; ; )
            //        {
            //            Thread.Sleep(120);
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

            //        if (buffer.Length > 8 && buffer[buffer.Length - 7] == '!')
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

            //while (true)
            //{
            //    Debug.WriteLine(System.Environment.TickCount64.ToString());
            //    P1Listener_P1MessageReceived(null, new P1MessageEventArgs() { EnergyReadout = new EnergyReadoutModel() {PowerConsuming = freeMemory, P1TimeStamp = DateTime.UtcNow } });
            //    Debug.WriteLine(System.Environment.TickCount64.ToString());
            //    Debug.WriteLine(nanoFramework.Runtime.Native.GC.Run(true).ToString());
            //    Debug.WriteLine(System.Environment.TickCount64.ToString());
            //    Thread.Sleep(50);
            //}

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        private static void SetTimer()
        {
            if(_readTimer != null)
            {
                _readTimer.Dispose();
                //make sure all reading threads have finished
                Thread.Sleep(2000);
            }

            byte[] tempBuffer = new byte[4096];
            int tempIndex = 0;
            bool tempSecondLoop = false;
            while (true)
            {
                int tempBytesToRead = _serialDevice.BytesToRead;
                if (tempBytesToRead > 0)
                {
                    if(tempBytesToRead + tempIndex > tempBuffer.Length)
                    {
                        tempIndex = 0;
                    }
                    _serialDevice.Read(tempBuffer, tempIndex, tempBytesToRead);
                    tempIndex += tempBytesToRead;

                    if (tempIndex > 8 && tempBuffer[tempIndex - 7] == '!')
                    {
                        if (tempSecondLoop)
                        {
                            break;
                        }
                        else
                        {
                            tempSecondLoop = true;
                            tempIndex = 0;

                        }
                    }
                }
            }
            Thread.Sleep(50);
            _readTimer = new Timer(ReadSerial, null, 1000, 1000);
            Debug.WriteLine("timer set");
        }

        static int _freeMemory = 0;
        private static void ReadSerial(object state)
        {
            
            int bytestoRead = _serialDevice.BytesToRead;
            byte[] buffer = new byte[bytestoRead];

            _serialDevice.Read(buffer, 0, buffer.Length);

            if (buffer.Length > 8 && buffer[buffer.Length - 7] == '!')
            {
                var message = SerialHandler(buffer);

                if (message != null)
                {
                    message.PowerConsuming = _freeMemory;
                    P1Listener_P1MessageReceived(null, new P1MessageEventArgs() { EnergyReadout = message });
                }
                else
                {
                    Debug.WriteLine("error");
                }

                _freeMemory = (int)nanoFramework.Runtime.Native.GC.Run(true);
            }
            else
            {
                Debug.WriteLine("setting timer");
                SetTimer();
                return;
            }
        }


        //static bool t_isReceiving = false;
        //private static void _serialDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    if (e.EventType != SerialData.WatchChar)
        //    {
        //        Debug.WriteLine(e.EventType.ToString());
        //        // nothing to do here, need to wait for the !
        //        return;
        //    }

        //    // now we have something to work on
        //    Debug.WriteLine("new data");
        //    _bytesRead = _serialDevice.Read(_workBuffer, 0, _serialDevice.BytesToRead);

        //    // signal the processing thread

        //    _dataAvailable.Set();
        //}

        //private static void ProcessIncommingBuffer()
        //{
        //    while (true)
        //    {
        //        // wait until there is something to do
        //        _dataAvailable.WaitOne();

        //        // read from the work buffer
        //        // ....

        //        if (_bytesRead > 10 && _workBuffer[_bytesRead - 7] == '!')
        //        {
        //            var message = SerialHandler(_workBuffer, _bytesRead);

        //            if (message != null)
        //            {
        //                P1Listener_P1MessageReceived(null, new P1MessageEventArgs() { EnergyReadout = message });
        //            }
        //            else
        //            {
        //                Debug.WriteLine(Encoding.UTF8.GetString(_workBuffer, 0, _bytesRead));
        //            }

        //        }

        //    }
        //}

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

        public static EnergyReadoutModel SerialHandler(byte[] serialdata, int length = -1)
        {
            if(length == -1)
            {
                length = serialdata.Length;
            }
            string crcString = Encoding.UTF8.GetString(serialdata, length - 6, 4);
            uint crc = hexString2uint(crcString);
            if (CheckCrc(serialdata, crc, length))
            {
                return P1MessageDecoder.DecodeData(serialdata, length);
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

        private static bool CheckCrc(byte[] buf, uint givenCrc, int length = -1)
        {
            if(length == -1)
            {
                length = buf.Length;
            }
            uint crc = 0;
            // -6 because we are reusing the original byte[] because of memory conservation
            for (int pos = 0; pos < length - 6; pos++)
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