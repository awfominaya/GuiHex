using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using GUIHex;

namespace GUIHex
{
    class ByteReader
    {
        string _name;
        int _offsetbytes;
        int _bytes; //How many bytes long
        DataType _datatype;

        public ByteReader(string name, int offset, int bytes, DataType dt )
        {
            _name = name;
            _offsetbytes = offset;
            _bytes = bytes;
            _datatype = dt;
        }

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int offsetbytes
        {
            get { return _offsetbytes; }
            set { _offsetbytes = value; }
        }

        public int bytes
        {
            get { return _bytes; }
            set { _bytes = value; }
        }

        public DataType datatype
        {
            get { return _datatype; }
            set { _datatype = value; }
        }

        #region "static functions"

        public static string GetDataTypeStringFromDataType(DataType dt)
        {
            string s = "";

            switch (dt)
            {
                case DataType.Int16_:
                    s = "Int16";
                    break;
                case DataType.Int32_:
                    s = "Int32";
                    break;
                case DataType.Int64_:
                    s = "Int64";
                    break;
                default:
                    s = "string";
                    break;
            }
            return s;
        }

        public static DataType GetDataTypeFromString(string s)
        {
            switch (s)
            {
                case "Int16":
                    return GUIHex.DataType.Int16_;
                case "Int32":
                    return GUIHex.DataType.Int32_;
                case "Int64":
                    return GUIHex.DataType.Int64_;
                default:
                    return GUIHex.DataType.String_;
            }
        }

        public static List<ByteReader> GetByteReaderListFromConcatStringList(List<string> concatstringlist)
        {
            List<ByteReader> brlist = new List<ByteReader>();

            foreach (string s in concatstringlist)
            {
                brlist.Add(GetByteReaderFromConcatString(s));
            }
            return brlist;
        }

        public static ByteReader GetByteReaderFromConcatString(string s)
        {
            string[] sarray = s.Split(',');
            ByteReader br = new ByteReader(sarray[0], GlobalFuncts.ParseStringToInt(sarray[1]), GlobalFuncts.ParseStringToInt(sarray[2]), ByteReader.GetDataTypeFromString(sarray[3]));
            return br;
        }

        public static string ReadRecord(ByteReader br, string hexstring)
        {
            //Convert the raw hex string to two byte blocks in a List<string>
            List<string> hexstringlist = ConvertHexStringToTwoByteBlocks(hexstring);

            //Get just the segment of string I need to process
            List<string> byteblocks = ByteReader.GetHexStringSegmentFromByteBlocks(br, hexstringlist);

            string record = "";

            switch (br.datatype)
            {
                case DataType.Int16_:
                    record = ProcInt16(byteblocks);
                    break;
                case DataType.Int32_:
                    record = ProcInt32(byteblocks);
                    break;
                case DataType.Int64_:
                    record = ProcInt64(byteblocks);
                    break;
                default:
                    record = ProcString(br, byteblocks);
                    break;
            }

            return record;
        }

        static List<string> GetHexStringSegmentFromByteBlocks(ByteReader br, List<string> byteblocks)
        {
            int startpoint = br.offsetbytes;
            int length = br.bytes;
            //get only the portion of text I need

            List<string> stringsegment = new List<string>();
            for (int i = startpoint; i < startpoint + length; i++)
            {
                stringsegment.Add(byteblocks[i]);
            }

            return stringsegment;
        }

        static string ConcatByteBlocks(List<string> byteblocks)
        {
            string hexstring = "";

            foreach (string s in byteblocks)
            {
                hexstring = hexstring + s;
            }
            return hexstring;
        }

        static string ProcInt16(List<string> byteblocks)
        {
            byte[] bytes = StringToByteArray(ConcatByteBlocks(byteblocks));
            int i = BitConverter.ToInt16(bytes,0);

            return i.ToString();
        }

        static byte[] StringToByteArray(string hexencodedarray)
        {
            int NumberChars = hexencodedarray.Length / 2;
            byte[] bytes = new byte[NumberChars];
            using (var sr = new StringReader(hexencodedarray))
            {
                for (int i = 0; i < NumberChars; i++)
                    bytes[i] =
                      Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return bytes;
        }

        static string ProcInt32(List<string> byteblocks)
        {
            byte[] bytes = StringToByteArray(ConcatByteBlocks(byteblocks));
            int i = BitConverter.ToInt16(bytes, 0);

            return i.ToString();
        }

        static string ProcInt64(List<string> byteblocks)
        {
            byte[] bytes = StringToByteArray(ConcatByteBlocks(byteblocks));
            int i = BitConverter.ToInt16(bytes, 0);

            return i.ToString();
        }

        static string ProcString(ByteReader br, List<string> byteblocks)
        {
            int startpoint = br.offsetbytes;
            int length = br.bytes;
            //get only the portion of text I need

            byte[] barray = new byte[byteblocks.Count];

            string hextext = "";

            int counter = 0;

            foreach (string bb in byteblocks)
            {
                barray[counter] = (byte)Convert.ToInt32(bb, 16);
                counter++;
            }


            string finaltext = hextext = System.Text.Encoding.UTF8.GetString(barray);
            return finaltext;
        }

        public static List<string> ConvertHexStringToTwoByteBlocks(string s)
        {
            string hexbyte = "";

            List<string> hexlist = new List<string>();

            bool second = false;
            foreach (char c in s)
            {
                if (second)
                {
                    hexbyte = hexbyte + c;
                    hexlist.Add(hexbyte);
                    hexbyte = "";
                    second = false;
                }
                else
                {
                    hexbyte = hexbyte + c;
                    second = true;
                }
            }
            return hexlist;


        }

        static string ConvertTwoByteHexStringToText(string s)
        {
            string[] sarray = s.Split('-');
            byte[] barray = new byte[sarray.Length];

            int counter = 0;

            foreach (string sval in sarray)
            {
                if (counter > barray.Length - 1)
                {

                }
                else
                {
                    barray[counter] = (byte)Convert.ToInt32(sval, 16);
                    counter++;
                }
            }

            string newstring = System.Text.Encoding.UTF8.GetString(barray);
            return newstring;
        }

        public byte[] ReadHexFile(OpenFileDialog ofd)
        {

            return null;
        }
    }
    #endregion
}
