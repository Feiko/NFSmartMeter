using nanoFramework.AtomLite;
using nanoFramework.Hardware.Esp32;
using NFSmartMeter.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Threading;
using nanoFramework.SignalR.Client;
using nanoFramework.Networking;
using System.Net;
using System.Net.WebSockets;

namespace NFSmartMeter
{
    public class Program
    {
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

        private static HubConnection s_hubConnection;
        public static void Main()
        {
            const string Ssid = "testnetwork";
            const string Password = "securepassword1!";

            // Give 60 seconds to the wifi join to happen
            CancellationTokenSource cs = new(60000);
            var success = WiFiNetworkHelper.ScanAndConnectDhcp(Ssid, Password, token: cs.Token);
            if (!success)
            {
                AtomLite.NeoPixel.SetColor(Color.Red);
            }


            Debug.WriteLine(IPAddress.GetDefaultLocalAddress().ToString());


            
            
            //Setup SmartMeter Connection
            SerialSmartMeterListner p1Listener = new SerialSmartMeterListner(32, 26);
            p1Listener.P1MessageReceived += P1Listener_P1MessageReceived;

            //Create headers
            var headers = new System.Net.WebSockets.ClientWebSocketHeaders();
            headers["secret"] = "mySecretKey";
            headers["smartMeterId"] = "Meter 1";

            //Reconnect is set to true
            s_hubConnection = new HubConnection("http://192.168.179.2:5001/SmartMeterHub", headers);
            s_hubConnection.Closed += SigClient_Closed;

            ConnectHub();


            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        private static void P1Listener_P1MessageReceived(object sender, P1MessageEventArgs e)
        {
            if(s_hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    s_hubConnection.SendCore("SendData", new object[] { e.EnergyReadout });
                    AtomLite.NeoPixel.SetColor(Color.Green);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    AtomLite.NeoPixel.SetColor(Color.Red);
                }

            }
            else
            {
                AtomLite.NeoPixel.SetColor(Color.Orange);
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
            AtomLite.NeoPixel.SetColor(Color.Yellow);
            while (s_hubConnection.State != HubConnectionState.Connected)
            {
                try
                {
                    s_hubConnection.Start();
                    AtomLite.NeoPixel.SetColor(Color.Blue);
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