using nanoFramework.AtomLite;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using nanoFramework.SignalR.Client;
using nanoFramework.Networking;
using System.Net;

namespace NFSmartMeter
{
    public class Program
    {


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
                Thread.Sleep(Timeout.Infinite);
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