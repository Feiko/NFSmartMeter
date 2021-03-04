using System;


namespace NFSmartMeter.Models
{
    /// <summary>
    /// 0-0:96.7.19.255- Power Failure Log
    /// </summary>
    public struct PowerOutageModel
    {
        /// <summary>
        /// Timestamp (end of failure)
        /// </summary>
        public DateTime Timestamp;
        /// <summary>
        /// duration in seconds
        /// </summary>
        public TimeSpan Duration;
    }
}
