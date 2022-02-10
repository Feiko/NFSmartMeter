using System;
using System.Diagnostics;
using System.Threading;
using NF.ESP32.InvertSerial;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using nanoFramework.Hardware.Esp32;
using System.Text;



namespace NFSmartMeter
{
    public class Program
    {
        static DataReader inputDataReader;

        static RGBLed led = new RGBLed();
        public static void Main()
        {
            
            led.SetLed(new Color() { R = 255 });
            led.Update();
            Configuration.SetPinFunction(25, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(21, DeviceFunction.COM2_RX);
            var _serialDevice = SerialDevice.FromId("COM2");
            _serialDevice.BaudRate = 115200;
            _serialDevice.StopBits = SerialStopBitCount.One;
            _serialDevice.Parity = SerialParity.None;
            _serialDevice.DataBits = 8;
            _serialDevice.ReadTimeout = new TimeSpan(0, 0, 0, 0,500);
            _serialDevice.Handshake = SerialHandshake.None;
            
            inputDataReader = new DataReader(_serialDevice.InputStream);
            inputDataReader.InputStreamOptions = InputStreamOptions.ReadAhead;

            //the interop magic that inverts the serial port on the ESP IDF
            SerialInverter.InvertSerial(1);


            Debug.WriteLine("Hello from nanoFramework!");

            //use string as databuffer because easier to append than a byte[]
            string DataBuffString = string.Empty;
            while (true)
            {
                if (_serialDevice.BytesToRead > 0)
                {
                    var bytesread = inputDataReader.Load(_serialDevice.BytesToRead);

                    if (DataBuffString.Length + bytesread > 3072) DataBuffString = string.Empty; //buffer full

                    DataBuffString += inputDataReader.ReadString(bytesread);
                    if(DataBuffString.Length > 6 && DataBuffString[DataBuffString.Length -7] == '!') //end of package
                    {
                        Debug.WriteLine("received");
                        var buff = Encoding.UTF8.GetBytes(DataBuffString);
                        DataBuffString = string.Empty;
                        if (SerialDataHandler.SerialHandler(buff))
                        {
                            Debug.WriteLine("success");
                            led.Blink(new Color() { B = 255 }, 400);
                        }

                    }
                }
            }
            Thread.Sleep(Timeout.Infinite);
            




            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

    }
}
