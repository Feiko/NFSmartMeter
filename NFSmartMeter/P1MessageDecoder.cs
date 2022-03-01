using NFSmartMeter.Models;
using System;
using System.Text;

namespace NFSmartMeter
{

    public static class P1MessageDecoder
    {

        public static EnergyReadoutModel DecodeData(byte[] data, int length = -1)
        {
            if (length == -1) length = data.Length;

            EnergyReadoutModel readout = new EnergyReadoutModel();
            int lineBreakIndex = GetNextLineBreak(0, data, length);

            readout.MeterId = Encoding.UTF8.GetString(data, 5, lineBreakIndex - 5);
            int startIndex = lineBreakIndex + 2;
            while (lineBreakIndex != -1)
            {

                lineBreakIndex = GetNextLineBreak(startIndex, data, length);
                if (lineBreakIndex - startIndex > 10)
                {
                    COSEMObjectModel cosem = GetCosemObjectFromLine(data, startIndex, (lineBreakIndex == -1 ? length - startIndex : lineBreakIndex - startIndex));

                    if (cosem.IsValidObject)
                    {
                        switch (cosem.ObisId)
                        {
                            case "0.2.8":
                                readout.ProtocolVer = uint.Parse(cosem.Value);
                                break;
                            case "1.0.0":
                                readout.P1TimeStamp = ParseTime(cosem.Value);
                                break;
                            case "96.1.1":
                                readout.PowerId = OctStringToHexString(cosem.Value);
                                break;
                            case "1.8.1":
                                readout.Tariff1Consumed = ParseKWH(cosem.Value);
                                break;
                            case "1.8.2":
                                readout.Tariff2Consumed = ParseKWH(cosem.Value);
                                break;
                            case "2.8.1":
                                readout.Tariff1Deliverd = ParseKWH(cosem.Value);
                                break;
                            case "2.8.2":
                                readout.Tariff2Delivered = ParseKWH(cosem.Value);
                                break;
                            case "96.14.0":
                                readout.TariffIndicator = uint.Parse(cosem.Value);
                                break;
                            case "1.7.0":
                                readout.PowerConsuming = ParseKWH(cosem.Value);
                                break;
                            case "2.7.0":
                                readout.PowerDelivering = ParseKWH(cosem.Value);
                                break;
                            case "96.7.21":
                                readout.PowerFailuresNumAny = uint.Parse(cosem.Value);
                                break;
                            case "96.7.9":
                                readout.PowerFailuresNumLongAny = uint.Parse(cosem.Value);
                                break;
                            case "99.97.0":
                                readout.PowerFailureLog = ParsePowerOutages(cosem.Values);
                                break;
                            case "32.32.0":
                                readout.VoltageSagsNumL1 = uint.Parse(cosem.Value);
                                break;
                            case "52.32.0":
                                readout.VoltageSagsNumL2 = uint.Parse(cosem.Value);
                                break;
                            case "72.32.0":
                                readout.VoltageSagsNumL3 = uint.Parse(cosem.Value);
                                break;
                            case "32.36.0":
                                readout.VoltageSwellsNumL1 = uint.Parse(cosem.Value);
                                break;
                            case "52.36.0":
                                readout.VoltageSwellsNumL2 = uint.Parse(cosem.Value);
                                break;
                            case "72.36.0":
                                readout.VoltageSwellsNumL3 = uint.Parse(cosem.Value);
                                break;
                            case "96.13.0":
                                readout.TextMessage = OctStringToHexString(cosem.Value);
                                break;
                            case "32.7.0":
                                readout.VoltageL1 = ParseKWH(cosem.Value);
                                break;
                            case "52.7.0":
                                readout.VoltageL2 = ParseKWH(cosem.Value);
                                break;
                            case "72.7.0":
                                readout.VoltageL3 = ParseKWH(cosem.Value);
                                break;
                            case "31.7.0":
                                readout.CurrentL1 = uint.Parse(cosem.Value);
                                break;
                            case "51.7.0":
                                readout.CurrentL2 = uint.Parse(cosem.Value);
                                break;
                            case "71.7.0":
                                readout.CurrentL3 = uint.Parse(cosem.Value);
                                break;
                            case "21.7.0":
                                readout.PowerPositiveL1 = ParseKWH(cosem.Value);
                                break;
                            case "41.7.0":
                                readout.PowerPositiveL2 = ParseKWH(cosem.Value);
                                break;
                            case "61.7.0":
                                readout.PowerPositiveL3 = ParseKWH(cosem.Value);
                                break;
                            case "22.7.0":
                                readout.PowerNegativeL1 = ParseKWH(cosem.Value);
                                break;
                            case "42.7.0":
                                readout.PowerNegativeL2 = ParseKWH(cosem.Value);
                                break;
                            case "62.7.0":
                                readout.PowerNegativeL3 = ParseKWH(cosem.Value);
                                break;
                            case "24.1.0":
                            case "96.1.0":
                            case "24.2.1":
                                string key = cosem.DeviceKey;
                                ConnectedMeterModel device;
                                if (readout.Devices.Contains(key))
                                {
                                    device = (ConnectedMeterModel)readout.Devices[key];
                                }
                                else
                                {
                                    device = new ConnectedMeterModel() { Channel = uint.Parse(key) };
                                }

                                if (cosem.ObisId == "24.1.0")
                                {
                                    device.DeviceType = uint.Parse(cosem.Value);
                                }
                                else if (cosem.ObisId == "96.1.0")
                                {
                                    device.DeviceId = OctStringToHexString(cosem.Value);
                                }
                                else
                                {
                                    device.DeviceTimeStamp = ParseTime(cosem.Values[0]);
                                    device.DeviceValue = ParseKWH(cosem.Values[1]);
                                    device.DeviceMeasurementUnit = cosem.Unit;
                                }
                                readout.Devices[key] = device;
                                break;

                            default:
                                Console.WriteLine($"unsupported Command: {cosem.ObisId}");
                                break;
                        }
                    }
                }
                startIndex = lineBreakIndex + 2;
            }
            return readout;
        }

        static PowerOutageModel[] ParsePowerOutages(string[] powerOutageValues)
        {
            int numPowerOutages = powerOutageValues.Length / 2;
            PowerOutageModel[] outages = new PowerOutageModel[numPowerOutages];
            for (int i = 0; i < numPowerOutages; i++)
            {
                outages[i] = new PowerOutageModel { Timestamp = ParseTime(powerOutageValues[i * 2]), Duration = new TimeSpan(0, 0, int.Parse(powerOutageValues[i * 2 + 1])) };
            }

            return outages;
        }

        static private DateTime ParseTime(string timeString)
        {
            bool isWintertime = (timeString[12].Equals('W'));
            int[] timeNum = new int[6];
            for (int i = 0; i < 6; i++)
            {
                timeNum[i] = int.Parse(timeString.Substring(i * 2, 2));
            }
            var time = new DateTime(2000 + timeNum[0], timeNum[1], timeNum[2], timeNum[3], timeNum[4], timeNum[5]).AddHours(-(isWintertime ? 1 : 2));
            return time;
        }

        static string OctStringToHexString(string octString)
        {
            string hexString = string.Empty;
            for (int i = 0; i < octString.Length; i += 2)
            {
                string hs = octString.Substring(i, 2);
                hexString += (((char)Convert.ToUInt32(hs, 16)).ToString());

            }
            return hexString;
        }

        static double ParseKWH(string kwhString)
        {
            return double.Parse(kwhString);
        }

        static COSEMObjectModel GetCosemObjectFromLine(byte[] data, int startIndex, int length)
        {
            COSEMObjectModel cosem = new COSEMObjectModel();
            cosem.IsValidObject = true;
            cosem.DeviceKey = ((char)data[2 + startIndex]).ToString();

            int indexFirstValue = 0;
            for (int i = startIndex + 9; i < startIndex + 12; i++)
            {
                if (data[i] == '(')
                {
                    indexFirstValue = i;
                    break;
                }
            }
            if (indexFirstValue == 0)
            {
                cosem.IsValidObject = false;
            }
            else
            {
                cosem.ObisId = Encoding.UTF8.GetString(data, startIndex + 4, indexFirstValue - (startIndex + 4));
                //multipull values
                if (cosem.ObisId == "24.2.1")
                {
                    var values = Encoding.UTF8.GetString(data, startIndex + 26, length - 27).Split('*');
                    cosem.Values = new string[]
                    {
                        Encoding.UTF8.GetString(data, startIndex + 11, 13),
                        values[0]
                    };
                    cosem.Unit = values[1];
                }
                else if (cosem.ObisId == "99.97.0") //power outages
                {
                    int lengthOfPowerOutagesField = 1;
                    //get number of power outages;
                    for (int i = startIndex + 13; data[i] != ')'; i++)
                    {
                        lengthOfPowerOutagesField++;
                    }
                    string numOutagesField = Encoding.UTF8.GetString(data, startIndex + 12, lengthOfPowerOutagesField);
                    int numOutages = int.Parse(numOutagesField);
                    cosem.Values = new string[numOutages * 2];
                    int firstPowerOutageIndex = startIndex + 26 + lengthOfPowerOutagesField;
                    for (int i = 0; i < numOutages; i++)
                    {
                        cosem.Values[i * 2] = Encoding.UTF8.GetString(data, i * 29 + 1 + firstPowerOutageIndex, 13);
                        cosem.Values[i * 2 + 1] = Encoding.UTF8.GetString(data, i * 29 + 16 + firstPowerOutageIndex, 10);
                    }

                }
                //single value
                else
                {
                    int endIndex = startIndex + length - 2;
                    if (data[endIndex] > 0x2f) //letters are used
                    {
                        //find end index special if *kwh
                        for (int i = endIndex; i > endIndex - 4; i--)
                        {
                            if (data[i] == '*')
                            {
                                endIndex = i - 1;
                            }
                        }
                    }
                    cosem.Value = Encoding.UTF8.GetString(data, indexFirstValue + 1, endIndex - indexFirstValue);
                }
            }
            return cosem;
        }

        private static int GetNextLineBreak(int startindex, byte[] buffer, int len)
        {

            for (int pos = startindex + 1; pos < len - 1; pos += 2)
            {
                if (buffer[pos] < 14)
                {
                    if (buffer[pos] == 13 && buffer[pos + 1] == 10)
                    {
                        return pos;
                    }
                    else if (buffer[pos] == 10 && buffer[pos - 1] == 13)
                    {
                        return pos - 1;
                    }
                }
            }
            return -1;
        }

    }
}

