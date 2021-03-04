using NFSmartMeter.Models;
using System;
using System.Diagnostics;
using System.Text;

namespace NFSmartMeter
{

        public static class P1MessageDecoder
        {
            //static CultureInfo US = new CultureInfo("en-US");

            public static EnergyReadoutModel DecodeData(string data)
            {
                var lines = data.Split(new char[] { '\r', '\n' });

                EnergyReadoutModel readout = new EnergyReadoutModel();
                readout.MeterId = lines[0].Substring(5, lines[0].Length - 5);

                for (int i = 1; i < lines.Length; i++)
                {
                    COSEMObjectModel cosem = GetCosemObjectFromLine(lines[i]);
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
                                string key = cosem.ObisIdTrail.Split('-')[1];
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
                                Debug.WriteLine($"unsupported Command: {cosem.ObisId}");
                                break;
                        }
                    }

                }

                
            return readout;
            }

            static PowerOutageModel[] ParsePowerOutages(string[] powerOutageValues)
            {
                int numPowerOutages = int.Parse(powerOutageValues[0] ?? "0");
                PowerOutageModel[] outages = new PowerOutageModel[numPowerOutages];
                for (int i = 0; i < numPowerOutages; i++)
                {
                    outages[i] = new PowerOutageModel { Timestamp = ParseTime(powerOutageValues[i * 2 + 2]), Duration = new TimeSpan(0, 0, int.Parse(powerOutageValues[i * 2 + 3])) };
                }

                return outages;
            }

            static DateTime ParseTime(string timeString)
            {
                bool isWintertime = (timeString[12].Equals('W'));
                int[] timeNum = new int[6];
                for(int i = 0; i < 6; i++)
                {
                    timeNum[i] = int.Parse(timeString.Substring(i * 2, 2));
                }
                var time = new DateTime(2000 + timeNum[0], timeNum[1], timeNum[2], timeNum[3] + (isWintertime ? 1 : 2), timeNum[4], timeNum[5]);
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
            static COSEMObjectModel GetCosemObjectFromLine(string line)
            {
                COSEMObjectModel cosem = new COSEMObjectModel();
                var splits = line.Split('(');
                if (splits.Length == 1)
                {
                    cosem.IsValidObject = false;
                }
                else
                {
                    string[] cosemIds = splits[0].Split(':');
                    if (cosemIds.Length == 2)
                    {
                        cosem.IsValidObject = true;
                        cosem.ObisIdTrail = cosemIds[0];
                        cosem.ObisId = cosemIds[1];
                        if (splits.Length == 2)
                        {
                            string[] values = removeTrailing(splits[1]);
                            cosem.Value = values[0];
                            if (values.Length == 2)
                            {
                                cosem.Unit = values[1];
                            }
                        }
                        else
                        {
                            cosem.Values = new string[splits.Length - 1];
                            for (int i = 1; i < splits.Length; i++)
                            {
                                string[] values = removeTrailing(splits[i]);
                                cosem.Values[i - 1] = values[0];
                                if (values.Length == 2)
                                {
                                    cosem.Unit = values[1];
                                }
                            }

                        }
                    }
                    else
                    {
                        cosem.IsValidObject = false;
                    }
                }

                return cosem;
            }

            static string[] removeTrailing(string part)
            {
                return part.Trim().TrimEnd(')').Split('*');

            }
           
        }
}
