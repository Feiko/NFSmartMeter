
namespace NFSmartMeter.Models
{
    public struct COSEMObjectModel
    {
        public bool IsValidObject;
        public string ObisId;
        public string ObisIdTrail;
        public string[] Values;
        public string Value;
        public string DeviceKey;
        public string Unit { get; internal set; }
    }
}
