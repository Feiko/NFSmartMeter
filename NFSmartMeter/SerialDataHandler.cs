using System;
using System.Diagnostics;
using System.Text;
using NFSmartMeter.Models;

namespace NFSmartMeter
{
    static class SerialDataHandler
    {

        public static bool SerialHandler(byte[] serialdata)
        {

            byte[] buffer = new byte[serialdata.Length - 6];
            Array.Copy(serialdata, 0, buffer, 0, serialdata.Length - 6);
            string crcString = Encoding.UTF8.GetString(serialdata, serialdata.Length - 6, 4);
            Debug.WriteLine(crcString);
            Debug.WriteLine(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
            uint crc = hexString2uint(crcString);
            EnergyReadoutModel model;
            if (CheckCrc(buffer, crc)) 
            {
                model = P1MessageDecoder.DecodeData(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                return true;
            }
            else
            {              
                return false;
            }

        }
        
        private static uint hexString2uint(string crcString)
        {
            if (crcString.Length != 4)
            {
                return 0; //uneven Number can not be a hex representation
            }
            uint[] partNums = new uint[crcString.Length];
            for (int i = 0; i < crcString.Length; i++)
            {
                switch (crcString[i])
                {
                    case '0':
                        partNums[i] = 0;
                        break;
                    case '1':
                        partNums[i] = 1;
                        break;
                    case '2':
                        partNums[i] = 2;
                        break;
                    case '3':
                        partNums[i] = 3;
                        break;
                    case '4':
                        partNums[i] = 4;
                        break;
                    case '5':
                        partNums[i] = 5;
                        break;
                    case '6':
                        partNums[i] = 6;
                        break;
                    case '7':
                        partNums[i] = 7;
                        break;
                    case '8':
                        partNums[i] = 8;
                        break;
                    case '9':
                        partNums[i] = 9;
                        break;
                    case 'A':
                        partNums[i] = 10;
                        break;
                    case 'B':
                        partNums[i] = 11;
                        break;
                    case 'C':
                        partNums[i] = 12;
                        break;
                    case 'D':
                        partNums[i] = 13;
                        break;
                    case 'E':
                        partNums[i] = 14;
                        break;
                    case 'F':
                        partNums[i] = 15;
                        break;
                    default:
                        return 0;
                }
            }

            return BitConverter.ToUInt16(new byte[] { (byte)(partNums[2] * 16 + partNums[3]), (byte)(partNums[0] * 16 + partNums[1]) }, 0);

        }



        static bool CheckCrc(byte[] buf, uint givenCrc)
        {

            uint crc = 0;
            for (int pos = 0; pos < buf.Length; pos++)
            {
                crc ^= (uint)buf[pos];    // XOR byte into least sig. byte of crc

                for (int i = 8; i != 0; i--)
                {    // Loop over each bit
                    if ((crc & 0x0001) != 0)
                    {      // If the LSB is set
                        crc >>= 1;                    // Shift right and XOR 0xA001
                        crc ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        crc >>= 1;                    // Just shift right
                }
            }
            Debug.WriteLine(crc.ToString());
            Debug.WriteLine(givenCrc.ToString());
            Debug.WriteLine("");
            return crc == givenCrc;


        }
    }
}
