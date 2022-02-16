using System;


namespace NFSmartMeter.Models
{
    /// <summary>
    /// 0-0:96.7.19.255- Power Failure Log
    /// </summary>
    public class PowerOutageModel
    {
        /// <summary>
        /// Timestamp (end of failure)
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// duration in seconds
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}
