using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Collections;
using System.Text;


namespace NFSmartMeter
{
    public class ThingsMqtt
    {
        public bool Connected = false;
        private MqttClient client;
        private string TELEMETRY_TOPIC = "v1/devices/me/telemetry";



        //public delegate void RpcEventHandler(object sender, RpcEventArgs e);

        //public event RpcEventHandler OnRpcRequestTopic;
        //public event RpcEventHandler OnRpcResponseTopic;
        //public event RpcEventHandler OnRpcError;
        //public event RpcEventHandler OnAttributesResponseTopic;

        private MqttQoSLevel QoS;
        private int tbRequestId = 0;

        public bool Connect(string Host, string AccessToken, int Port = 1883)
        {
            if (Host == null || AccessToken == null) return false;

            string clientId = Guid.NewGuid().ToString(); ;
            client = new MqttClient(Host, Port, false, null, null, MqttSslProtocols.None);

            var result = client.Connect(clientId, AccessToken, null);

            if (result != 0) return false;

            Connected = true;

            this.QoS = MqttQoSLevel.AtLeastOnce;
            return true;
        }

        public void SendTelemetry(string telemetry)
        {
            client.Publish(TELEMETRY_TOPIC, Encoding.UTF8.GetBytes(telemetry), QoS, false);
        }
    }
}


