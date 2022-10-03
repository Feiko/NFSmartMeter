using nanoFramework.Hardware.Esp32;
using NFSmartMeter.Models;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Collections;


namespace NFSmartMeter
{
    

    public class P1MessageEventArgs : EventArgs
    {
        public EnergyReadoutModel EnergyReadout { get; set; }
    }
}
