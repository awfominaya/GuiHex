using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GUIHex.Properties;

namespace GUIHex
{
    class ByteWriter
    {
        List<int> _startpositions = new List<int>();
        List<byte[]> _bytearrays = new List<byte[]>();

        public ByteWriter()
        {

        }

        public List<int> startpositions 
        {
            get { return _startpositions; }
            set { _startpositions = value; }
        }

        public List<byte[]> bytearrays
        {
            get { return _bytearrays; }
            set { _bytearrays = value; }
        }

        //function to add new entry to the start list
        //make sure it checks whether the spacing is even. It should block the user if the spacing is uneven
        //do not let the program write past the end point. 
        // use first spacing if spacing is uneven.

        public static void WriteDGVToHexFile(List<ByteReader> brlist, List<Record> recordlist, ByteWriter bw, int startpoint, int endpoint)
        {
            List<byte[]> bytelist = new List<byte[]>();
            foreach (Record r in recordlist)
            {
                int cellcounter = 0;
                foreach (string s in r.cellvalues)
                {
                    if (brlist[cellcounter].datatype == DataType.String_)
                    {
                        byte[] bytearray = Encoding.UTF8.GetBytes(s);
                        bytelist.Add(bytearray);
                    }
                    else
                    {
                        Int16 i16 = (Int16)GlobalFuncts.ParseStringToInt(s);
                        byte[] bytearray = BitConverter.GetBytes(i16);
                        bytelist.Add(bytearray);
                    }
                    cellcounter++;
                }
                cellcounter = 0;
            }
            bw.bytearrays = bytelist;

            List<byte[]> finalbytelist = new List<byte[]>(); // this is the basis for the final byte array to be written to file
            int bytecount = 0;

            if (bw.startpositions.Count == bw.bytearrays.Count)
            {
                for (int i = 0; i<=bw.bytearrays.Count; i++)
                {
                    finalbytelist.Add(bw.bytearrays[i]);
                    finalbytelist.Add(GetNEmptyBytes(bw.startpositions[i + 1] - bw.startpositions[i]));
                    bytecount += bw.bytearrays[i].Count();
                    bytecount += bw.startpositions[i + 1] - bw.startpositions[i];
                }
            }
            else
            {

                int recordlength = 0;
                foreach(ByteReader br in brlist)
                {
                    recordlength += br.bytes;
                }

                int spacing = bw.startpositions[1] - (bw.startpositions[0] + recordlength) ;

                foreach(byte[] barray in bw.bytearrays)
                {
                    finalbytelist.Add(barray);//writetherealbytes
                    finalbytelist.Add(GetNEmptyBytes(spacing));// writethespacingbytes
                    bytecount += barray.Count();
                    bytecount += spacing;
                }
            }

            byte[] finalbytearray = new byte[bytecount];

            int counter = 0;
            foreach (byte[] bar in finalbytelist)
            {
                foreach (byte b in bar)
                {
                    finalbytearray[counter] = b;
                    counter++;
                }
            }

            int numbytes = endpoint - startpoint;
            WriteByteRecords(finalbytearray,bw.startpositions[0], numbytes);
        }

        static byte[] GetNEmptyBytes(int bytes)
        {



            byte[] barray = new byte[bytes];
            for (int i = 0; i < barray.Length; i++)
            {
                barray[i] = 0; 
            }
            return barray;

        }

        static void WriteByteRecords(byte[] finalfullbytearray, int startpoint, int numbytes)
        {

            byte[] bytestoprint;

            //truncated byte array ensures that we aren't writing past the end point specified in textbox7.
            if (finalfullbytearray.Length > numbytes)
            {
                bytestoprint = new byte[numbytes];
            }
            else
            {
                bytestoprint = finalfullbytearray;
            }
            //the full byte array is already constructed so we just use the first start point listed in the bw and paste everything. 

            BinaryWriter bw = new BinaryWriter(File.OpenWrite(Settings.Default.BinPath));
            bw.BaseStream.Position = startpoint;
            bw.Write(bytestoprint);
            bw.Close();

        }

        public static bool CheckEntrySpacing(ByteWriter bw)
        {
            int lastspacing = bw.startpositions[1] - bw.startpositions[0];
            int spacingtocheck = bw.startpositions[2] - bw.startpositions[1];
            bool even = true;

            int counter = 1;
            foreach(int i in bw.startpositions)
            {
                if(bw.startpositions[counter] - bw.startpositions[counter-1] == bw.startpositions[counter +1] - bw.startpositions[counter])
                {

                }
                else
                {
                    even = false;
                }
            }

            return even;

        }
    }
}
