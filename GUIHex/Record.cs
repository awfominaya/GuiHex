using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GUIHex
{
    class Record
    {
        public enum Scenario { Read, EndRecord, Skip, StopSkip };
        List<string> _cellvalues = new List<string>();

        public Record()
        {

        }

        public List<string> cellvalues
        {
            get { return _cellvalues; }
            set { _cellvalues = value; }
        }

        public static List<Record> GetRecords(List<ByteReader> brlist, List<string> hexblocklist)
        {
            //parse hexblock into byteblocks

            //foreach br, readrecord
            List<Record> recordlist = new List<Record>();

            Record r = new Record();
            foreach (string hexblock in hexblocklist)
            {
                foreach (ByteReader br in brlist)
                {
                    string cv = ByteReader.ReadRecord(br, hexblock);
                    r.cellvalues.Add(cv);
                }
                recordlist.Add(r);
                r = new Record();
            }

            return recordlist;
        }

        public static List<string> GetByteBlocks(List<ByteReader> brlist, string fullhexstring)
        {
            int recordlength = 0;
            foreach (ByteReader br in brlist)
            {
                recordlength = recordlength+br.bytes;
            }

            List<string> listobyteblocks = new List<string>();

            int startposition = brlist[0].offsetbytes;
            List<string> allbyteblocks = ByteReader.ConvertHexStringToTwoByteBlocks(fullhexstring);

            int bytecounter = 0;
            Scenario curscenario = Scenario.Read;
            string curblock = "";

            foreach (string s in allbyteblocks)
            {
                curscenario = GetScenario(bytecounter, startposition, recordlength, s, allbyteblocks[GlobalFuncts.ParseStringToInt(s) + 1]);

                switch (curscenario)
                {
                    case Scenario.Read:
                        curblock = curblock + s;
                        break;
                    case Scenario.EndRecord:
                        curblock = curblock + s;
                        listobyteblocks.Add(curblock);
                        curblock = "";
                        //Do I need to change any of my signaling variables or is it truly the same as just reading?
                        break;
                    case Scenario.Skip:
                        break;
                    case Scenario.StopSkip:
                        startposition = bytecounter + 1;
                        break;

                }

                bytecounter++;
            }

            return listobyteblocks;
        }

        static Scenario GetScenario(int bytecounter, int startposition, int recordlength, string curbyte, string nextbyte)
        {
            Scenario curscenario = Scenario.Read;

            bool counterabovestart = false;
            bool counterbelowlength = false;
            bool record1notzerorecord2iszero = false;
            bool bothrecordsarezeros = false;
            bool record1iszerorecord2isnot = false;

            //is the counter at or above the startpositionvalue
            if (bytecounter >= startposition) { counterabovestart= true; } else { counterabovestart= false; }

            //is the counter at or above the length value
            if (bytecounter > startposition+recordlength) { counterbelowlength= false; } else { counterbelowlength= true; }

            //am I looking at zeros
            if(curbyte=="00" && nextbyte == "00") { bothrecordsarezeros = true; } else { bothrecordsarezeros = false; }
            if (curbyte != "00" && nextbyte == "00") { record1notzerorecord2iszero = true; } else { record1notzerorecord2iszero= false; }
            if (curbyte == "00" && nextbyte != "00") { record1iszerorecord2isnot = true; } else { record1iszerorecord2isnot = false; }

            //getscenario
            if (counterabovestart && counterbelowlength)
            {
                curscenario = Scenario.Read; // read if you can read.
            }
            else // If you can't read, let's figure out what to do.
            {
                if (record1notzerorecord2iszero)
                {
                    curscenario = Scenario.EndRecord;
                }

                if (bothrecordsarezeros)
                {
                    curscenario = Scenario.Skip;
                }

                if (record1iszerorecord2isnot)
                {
                    curscenario = Scenario.StopSkip;
                }
            }

            return curscenario;
        }

        string DelineateBytesInHexString(string hexstring)
        {
            string newstring = "";

            foreach (char c in hexstring)
            {
                bool first = true; //signifies whether we are on the first or second character. 
                
                if (first) 
                {
                    newstring = newstring + c;
                    first = false;
                }
                else
                {
                    newstring = c + "-";
                    first = true;
                }
            }

            return newstring;

        } 

    }

}
