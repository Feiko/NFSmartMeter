//using nanoFramework.AtomLite;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Networking;
using System.Net;
using System.IO.Ports;
using System.Text;
using NFSmartMeter.Models;

using System.Device.Wifi;
using System.Net.Http;

namespace NFSmartMeter
{
    public class Program
    {
        private static Timer _readTimer;
        private static SerialPort _serialDevice;
        static readonly HttpClient _httpClient = new HttpClient();

        public static void Main()
        {
            const string Ssid = "testnetwork";
            const string Password = "securepassword1!";


            WifiAdapter sender = WifiAdapter.FindAllAdapters()[0];
            sender.Disconnect();
            sender.ScanAsync();
            Thread.Sleep(10000);
            WifiNetworkReport report = sender.NetworkReport;

            // Enumerate though networks looking for our network
            foreach (WifiAvailableNetwork net in report.AvailableNetworks)
            {
                // Show all networks found
                Debug.WriteLine($"Net SSID :{net.Ssid},  BSSID : {net.Bsid},  rssi : {net.NetworkRssiInDecibelMilliwatts.ToString()},  signal : {net.SignalBars.ToString()}");

                // If its our Network then try to connect
                if (net.Ssid == "testnetwork")
                {
                    // Disconnect in case we are already connected
                    sender.Disconnect();

                    // Connect to network
                    WifiConnectionResult result = sender.Connect(net, WifiReconnectionKind.Automatic, "securepassword1!");

                    // Display status
                    if (result.ConnectionStatus == WifiConnectionStatus.Success)
                    {
                        Debug.WriteLine("Connected to Wifi network");
                    }
                    else
                    {
                        Debug.WriteLine($"Error {result.ConnectionStatus.ToString()} connecting o Wifi network");
                    }
                }
            }

            Thread.Sleep(5000);


            Debug.WriteLine(IPAddress.GetDefaultLocalAddress().ToString());
            Thread.Sleep(5000);

            var content = new StringContent("{\"temperature\": 20.3}");
            HttpResponseMessage response = _httpClient.Post("http://20.56.99.54:8080/api/v1/kDIAQqIEU4l1A1jpyUQN/telemetry", content);
            Debug.WriteLine(response.StatusCode.ToString());

            //Setup SmartMeter Connection
            //SerialSmartMeterListner p1Listener = new SerialSmartMeterListner(32, 26);
            //p1Listener.P1MessageReceived += P1Listener_P1MessageReceived;




            //setup serial port
            Configuration.SetPinFunction(26, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(32, DeviceFunction.COM2_RX);

            _serialDevice = new SerialPort("com2", 115200, Parity.None, 8, StopBits.One);
            _serialDevice.ReadTimeout = 2000;
            _serialDevice.ReadBufferSize = 3072;
            _serialDevice.InvertSignalLevels = true;
            _serialDevice.Open();
            SetTimer();

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
                    P1Listener_P1MessageReceived(null, new P1MessageEventArgs() { EnergyReadout = message });
                }
                else
                {
                    Debug.WriteLine("error");
                }
            }
            else
            {
                Debug.WriteLine("setting timer");
                SetTimer();
                return;
            }
        }

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

            try
            {
                var readout = e.EnergyReadout;
                var content = new StringContent("{\"temperature\": 20.3}");
                HttpResponseMessage response = _httpClient.Post("http://20.56.99.54:8080/api/v1/kDIAQqIEU4l1A1jpyUQN/telemetry", content);

                //s_Mqtt.Publish("v1/devices/me/telemetry", Encoding.UTF8.GetBytes("{\"temperature\": 20.3}"), MqttQoSLevel.ExactlyOnce, false);
                //AtomLite.NeoPixel.SetColor(Color.Green);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //AtomLite.NeoPixel.SetColor(Color.Red);
            }
        }

 

        
    }
}