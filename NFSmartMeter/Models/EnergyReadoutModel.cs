using System;
using System.Collections;

namespace NFSmartMeter.Models
{
    public class EnergyReadoutModel
    {
        /// <summary>
        /// 1-3:0.2.8.255 - Version information for P1 output
        /// </summary>
        public uint ProtocolVer { get; set; }

        /// <summary>
        /// Header information Manufacturer specific
        /// </summary>
        public string MeterId { get; set; }

        /// <summary>
        /// 0-0:1.0.0.255 - Date-time stamp of the P1 message
        /// </summary>
        public DateTime P1TimeStamp { get; set; }

        /// <summary>
        /// 0-0:96.1.1.255 - Equipment identifier
        /// </summary>
        public string PowerId { get; set; }

        /// <summary>
        /// 1-0:1.8.1.255 - Meter Reading electricity delivered to client(Tariff 1) in 0,001 kWh
        /// </summary>
        public double Tariff1Consumed { get; set; }

        /// <summary>
        /// 1-0:1.8.2.255 - Meter Reading electricity delivered to client(Tariff 2) in 0,001 kWh
        /// </summary>
        public double Tariff2Consumed { get; set; }

        /// <summary>
        /// 1-0:2.8.1.255 - Meter Reading electricity delivered by client(Tariff 1) in 0,001 kWh
        /// </summary>
        public double Tariff1Deliverd { get; set; }

        /// <summary>
        /// 1-0:2.8.2.255 - Meter Reading electricity delivered by client(Tariff 2) in 0,001 kWh
        /// </summary>
        public double Tariff2Delivered { get; set; }

        /// <summary>
        /// 0-0:96.14.0.255 - Tariff indicator electricity.The tariff indicator can also be used to switch tariff dependent loads e.g boilers.This is the responsibility of the P1 user
        /// </summary>
        public uint TariffIndicator { get; set; }

        /// <summary>
        /// 1-0:1.7.0.255 - Actual electricity power delivered (+P) in 1 Watt resolution
        /// </summary>
        public double PowerConsuming { get; set; }

        /// <summary>
        /// 1-0:2.7.0.255 - Actual electricity power delivered (-P) in 1 Watt resolution
        /// </summary>
        public double PowerDelivering { get; set; }

        /// <summary>
        /// 0-0:96.7.21.255 - Number of power failures in any phase
        /// </summary>
        public uint PowerFailuresNumAny { get; set; }

        /// <summary>
        /// 0-0:96.7.9.255 - Number of long power failures in any phase
        /// </summary>
        public uint PowerFailuresNumLongAny { get; set; }

        /// <summary>
        /// 1-0:99.97.0.255 - Power Failure Event Log (long power failures)
        /// </summary>
        public PowerOutageModel[] PowerFailureLog { get; set; }

        /// <summary>
        /// 1-0:32.32.0.255 - Number of voltage sags in phase L1
        /// </summary>
        public uint VoltageSagsNumL1 { get; set; }

        /// <summary>
        /// 1-0:32.52.0.255 - Number of voltage sags in phase L2
        /// </summary>
        public uint VoltageSagsNumL2 { get; set; }

        /// <summary>
        /// 1-0:32.72.0.255 - Number of voltage sags in phase L3
        /// </summary>
        public uint VoltageSagsNumL3 { get; set; }

        /// <summary>
        /// 1-0:32.36.0.255 - Number of voltage swells in phase L1
        /// </summary>
        public uint VoltageSwellsNumL1 { get; set; }

        /// <summary>
        /// 1-0:32.36.0.255 - Number of voltage swells in phase L2
        /// </summary>
        public uint VoltageSwellsNumL2 { get; set; }

        /// <summary>
        /// 1-0:32.36.0.255 - Number of voltage swells in phase L3
        /// </summary>
        public uint VoltageSwellsNumL3 { get; set; }

        /// <summary>
        /// 0-0:96.13.0.255 - Text message max 1024 characters.
        /// </summary>
        public string TextMessage { get; set; }

        /// <summary>
        /// 1-0:32.7.0.255 - Instantaneous voltage L1 in V resolution
        /// </summary>
        public double VoltageL1 { get; set; }

        /// <summary>
        /// 1-0:52.7.0.255 - Instantaneous voltage L2 in V resolution
        /// </summary>
        public double VoltageL2 { get; set; }

        /// <summary>
        /// 1-0:72.7.0.255 - Instantaneous voltage L3 in V resolution
        /// </summary>
        public double VoltageL3 { get; set; }

        /// <summary>
        /// 1-0:31.7.0.255 - Instantaneous current L1 in A resolution
        /// </summary>
        public uint CurrentL1 { get; set; }

        /// <summary>
        /// 1-0:51.7.0.255 - Instantaneous current L2 in A resolution
        /// </summary>
        public uint CurrentL2 { get; set; }

        /// <summary>
        /// 1-0:71.7.0.255 - Instantaneous current L3 in A resolution
        /// </summary>
        public uint CurrentL3 { get; set; }

        /// <summary>
        /// 1-0:21.7.0.255 - Instantaneous active power L1 (+P) in W resolution
        /// </summary>
        public double PowerPositiveL1 { get; set; }

        /// <summary>
        /// 1-0:41.7.0.255 - Instantaneous active power L2 (+P) in W resolution
        /// </summary>
        public double PowerPositiveL2 { get; set; }

        /// <summary>
        /// 1-0:61.7.0.255 - Instantaneous active power L3 (+P) in W resolution
        /// </summary>
        public double PowerPositiveL3 { get; set; }

        /// <summary>
        /// 1-0:22.7.0.255 - Instantaneous active power L1 (-P) in W resolution
        /// </summary>
        public double PowerNegativeL1 { get; set; }

        /// <summary>
        /// 1-0:42.7.0.255 - Instantaneous active power L2 (-P) in W resolution
        /// </summary>
        public double PowerNegativeL2 { get; set; }

        /// <summary>
        /// 1-0:62.7.0.255 - Instantaneous active power L3 (-P) in W resolution
        /// </summary>
        public double PowerNegativeL3 { get; set; }

        /// <summary>
        /// HashTable of extra attached meter devices like Gas, Water, Warmth etc. contains keys deviceId and values ConnecteMeterModels.
        /// 
        /// </summary>
        //public Dictionary<string, ConnectedMeterModel> Devices = new Dictionary<string, ConnectedMeterModel>();
        public Hashtable Devices  { get; set;} = new Hashtable();
        
    }

}

