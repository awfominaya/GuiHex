using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using GUIHex;
using GUIHex.Properties;

namespace GUIHex
{
    public partial class Form1 : Form
    {
        // to do
        //Convert all "var = var + newthing" to +=
        //Add error message management class
        //write single item bytes
        //write multibytes

        #region "Global Variables"

        OpenFileDialog ofd = new OpenFileDialog();
        string filepath = "";

        OpenFileDialog schemaofd = new OpenFileDialog();
        string schemapath = "";

        string savedirectory = Settings.Default["SaveDirectory"].ToString();
        string fileextension = Settings.Default["DefaultExtension"].ToString();
        string backupextension = Settings.Default["BackupExtension"].ToString();

        public List<int> listofstartpositions = new List<int>();

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TempAddDataToListBoxForTesting();
            LoadPrefs();
            
        }

        #region "Messager, Preferences, Schema and Bins"

        void UpdateMessageBox(string message)
        {
            richTextBox4.Text = MessageManager.GetMessages(message, richTextBox4.Text);
        }

        void LoadPrefs()
        {
            textBox1.Text = Settings.Default["Offset"].ToString();
            textBox2.Text = Settings.Default["NumVals"].ToString();
            textBox6.Text = Settings.Default["StartHex"].ToString();
            textBox7.Text = Settings.Default["EndHex"].ToString();
            filepath = Settings.Default["BinPath"].ToString();

            UpdateMessageBox(@"Now writing to Bin File: " + Settings.Default.BinPath);

            schemapath = Settings.Default.SchemaPath.ToString();
            LoadSchema(schemapath);
            schemaofd.FileName = schemapath;

            //UpdateMessageBox("Schema Path Loaded from " + schemapath);
        }

        void LoadSchema(string fullpath)
        {
            listBox1.Items.Clear();

            string[] lines = System.IO.File.ReadAllLines(fullpath);

            foreach (string line in lines)
            {
                string[] s = line.Split(',');
                listBox1.Items.Add(s[0] + "," + s[1] + "," + s[2] + "," + s[3]);
            }

            string directory = Path.GetDirectoryName(fullpath);
            string filename = Path.GetFileNameWithoutExtension(fullpath);

            FileManager.Save(directory, filename, lines.ToList<string>(), true, true);
            UpdateMessageBox("Schema Path Loaded from " + schemapath);
        }

        #endregion

        #region "Object Controls"

        private void button5_Click(object sender, EventArgs e)
        {
            List<Record> recordlist = new List<Record>();
            Record r = new Record();

            int totalrows = dataGridView1.Rows.Count;

            int rowcounter = 1; // set as 1 to align with the count of rows.
            int cellcounter = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (rowcounter < totalrows)
                {
                    foreach (DataGridViewCell c in row.Cells)
                    {
                        r.cellvalues.Add(row.Cells[cellcounter].Value.ToString()); //string needs to be converted later
                        cellcounter++;
                    }
                }
                recordlist.Add(r);
                r = new Record();
                cellcounter = 0;
                rowcounter++;
            }

            List<ByteReader> br = new List<ByteReader>();
            List<string> brstringlist = new List<string>();

            foreach (string s in listBox1.Items)
            {
                string[] vals = s.Split(',');
                // 0 = name, 1 = Offset, 2 = Bytes, 3 = datatype

                brstringlist.Add(vals[0] + "," + vals[1] + "," + vals[2] + "," + vals[3]);
            }

            List<ByteReader> brlist = ByteReader.GetByteReaderListFromConcatStringList(brstringlist);
            ByteWriter bw = new ByteWriter();
            bw.startpositions = listofstartpositions;

            int startval = System.Convert.ToInt32(this.textBox6.Text, 16);
            int endval = System.Convert.ToInt32(this.textBox7.Text, 16);

            ByteWriter.WriteDGVToHexFile(brlist, recordlist, bw, startval, endval);
            UpdateMessageBox(@"Attempting to write bytes to " + Settings.Default.BinPath);

        }


        private void button4_Click(object sender, EventArgs e)
        {
            int offset = 0;
            Int32.TryParse(textBox4.Text, out offset);

            int bytes = 0;
            Int32.TryParse(textBox5.Text, out bytes);

            string dt = "";

            if (radioButton1.Checked) { dt = "Int32"; }
            if (radioButton2.Checked) { dt = "Int64"; }
            if (radioButton3.Checked) { dt = "String"; }
            if (radioButton4.Checked) { dt = "Int16"; }

            //            ByteReader newbr = new ByteReader(textBox3.Text, offset, bytes, dt);
            //           ByteReaderList.Add(newbr);
            listBox1.Items.Add(textBox3.Text + "," + offset + "," + bytes + "," + dt);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textBox5.Text = "4";
            textBox5.Enabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox5.Text = "8";
            textBox5.Enabled = false;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            textBox5.Enabled = true;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            textBox5.Text = "2";
            textBox5.Enabled = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {

            List<string> concatstringlist = new List<string>();
            foreach (string s in listBox1.Items)
            {
                concatstringlist.Add(s);
            }

            List<ByteReader> tempbrlist = ByteReader.GetByteReaderListFromConcatStringList(concatstringlist);

            int selectedindex = 0;
            selectedindex = listBox1.SelectedIndex;

            List<ByteReader> newlist = new List<ByteReader>();
            bool pendingreader = false; // signify if there's a pending byte to store
            ByteReader heldreader = new ByteReader("default", 0, 0, DataType.String_);
            int counter = 0;

            //Reordering logic (MoveDown)
            if (selectedindex == -1 || selectedindex + 1 == listBox1.Items.Count)
            {
                //fail
            }
            else
            {
                foreach (ByteReader b in tempbrlist)
                {
                    if (counter == selectedindex) //the selected index is the one that is currently being evaluated
                    {
                        pendingreader = true;
                        heldreader = b;
                        //then go to the next one
                    }
                    else
                    {
                        if (pendingreader)
                        {
                            newlist.Add(b);
                            newlist.Add(heldreader);
                            pendingreader = false;
                        }
                        else
                        {
                            newlist.Add(b);
                        }
                    }
                    counter++;
                }

                listBox1.Items.Clear();

                foreach (ByteReader b in newlist)
                {
                    string s = b.name + "," + b.offsetbytes.ToString() + "," + b.bytes.ToString() + "," + ByteReader.GetDataTypeStringFromDataType(b.datatype);
                    listBox1.Items.Add(s);
                }
                try { listBox1.SelectedIndex = selectedindex + 1; } catch { }
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            List<string> concatstringlist = new List<string>();
            foreach (string s in listBox1.Items)
            {
                concatstringlist.Add(s);
            }

            List<ByteReader> tempbrlist = ByteReader.GetByteReaderListFromConcatStringList(concatstringlist);

            int selectedindex = 0;
            selectedindex = listBox1.SelectedIndex;

            List<ByteReader> newlist = new List<ByteReader>();
            bool pendingreader = false; // signify if there's a pending byte to store
            ByteReader heldreader = new ByteReader("default", 0, 0, DataType.String_);
            int counter = 0;

            //Reordering logic (Moveup)
            if (selectedindex <= 0)
            {
                //fail
            }
            else
            {
                foreach (ByteReader b in tempbrlist)
                {
                    if (counter == selectedindex - 1)
                    {
                        pendingreader = true;
                        heldreader = b;
                    }
                    else
                    {
                        if (counter == selectedindex)
                        {
                            newlist.Add(b);
                            newlist.Add(heldreader);
                            pendingreader = false;
                        }
                        else
                        {
                            newlist.Add(b);
                        }
                    }
                    counter++;
                }

                listBox1.Items.Clear();

                foreach (ByteReader b in newlist)
                {
                    string s = b.name + "," + b.offsetbytes.ToString() + "," + b.bytes.ToString() + "," + ByteReader.GetDataTypeStringFromDataType(b.datatype);
                    listBox1.Items.Add(s);
                }

                try { listBox1.SelectedIndex = selectedindex - 1; } catch { }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            List<string> concatstringlist = new List<string>();
            foreach (string s in listBox1.Items)
            {
                concatstringlist.Add(s);
            }

            List<ByteReader> tempbrlist = ByteReader.GetByteReaderListFromConcatStringList(concatstringlist);

            int selectedindex = 0;
            selectedindex = listBox1.SelectedIndex;

            List<ByteReader> newlist = new List<ByteReader>();
            int counter = 0;

            if (listBox1.SelectedIndex == -1)
            {
                //fail
            }
            else
            {
                foreach (ByteReader b in tempbrlist)
                {
                    if (listBox1.SelectedIndex != counter)
                    {
                        newlist.Add(b);
                    } // no neeed for an else condition because if they do equal, we just skip that value.
                    counter++;
                }

                listBox1.Items.Clear();

                foreach (ByteReader b in newlist)
                {
                    string s = b.name + "," + b.offsetbytes.ToString() + "," + b.bytes.ToString() + "," + ByteReader.GetDataTypeStringFromDataType(b.datatype);
                    listBox1.Items.Add(s);
                }

                try { listBox1.SelectedIndex = selectedindex - 1; } catch { }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {

            dataGridView1.AutoGenerateColumns = false;
            int initcount = dataGridView1.Columns.Count;

            if (initcount > 0)
            {
                for (int i = 0; i < initcount; i++)
                {
                    string s = dataGridView1.Columns[0].Name;
                    dataGridView1.Columns.Remove(s);
                }
            }

            dataGridView1.Update();
            dataGridView1.Refresh();

            List<string> bytereaderstrings = new List<string>();
            foreach(string s in listBox1.Items)
            {

            }

            foreach (ByteReader br in ByteReader.GetByteReaderListFromConcatStringList(bytereaderstrings))
            {
                AddDGVColumn(br);
            }

//            AddRows();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Settings.Default["Offset"] = textBox1.Text;
            Settings.Default.Save();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Settings.Default["NumVals"] = textBox2.Text;
            Settings.Default.Save();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            Settings.Default["StartHex"] = textBox6.Text;
            Settings.Default.Save();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            Settings.Default["EndHex"] = textBox7.Text;
            Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {

            }
            else
            {


                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
                int startpoint = Convert.ToInt32(textBox6.Text, 16);
                int endpoint = Convert.ToInt32(textBox7.Text, 16);
                int totallength = endpoint - startpoint;

                List<ByteReader> brlist = new List<ByteReader>();
                foreach (string s in listBox1.Items)
                {
                    brlist.Add(ByteReader.GetByteReaderFromConcatString(s));
                }

                int recordlength = 0;
                foreach (ByteReader b in brlist)
                {
                    recordlength += b.bytes;
                }

                List<Record> recordlist = Record.GetRecords(brlist, GetHexBlockListFromFile(startpoint, endpoint, recordlength));
                AddColumns(brlist);
                AddRows(recordlist);

                dataGridView1.Update();
                dataGridView1.Refresh();

                ByteWriter bw = new ByteWriter();
                bw.startpositions = listofstartpositions;

                if (ByteWriter.CheckEntrySpacing(bw))
                {
                    UpdateMessageBox(@"Spacing between entries is even. If you enter new rows into the form, " +
                                                    "the program will use this spacing to determine the start position for new records. " +
                                                    "The program will not allow you to write past the end point. Monitor this carefully because you could end up " +
                                                    "with a partial record written at the end of this section");
                }
                else
                {
                    UpdateMessageBox(@"Spacing between entries is uneven. If you enter new rows into the form, " +
                                                    "the program will use the spacing between the first two records as its guide in determining the start position for new records. " +
                                                    "this may require you to manually move the bytes in a traditional hex editor later");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int brindex;

            //try
            //{
            try {
                brindex = listBox1.SelectedIndex;
                ByteReader br = ByteReader.GetByteReaderFromConcatString(listBox1.Items[brindex].ToString());
                try
                {
                    ByteReader brnext = ByteReader.GetByteReaderFromConcatString(listBox1.Items[brindex + 1].ToString());
                    int stopval = brnext.offsetbytes;
                }
                catch
                {
                    int stopval = br.offsetbytes + br.bytes + 1;
                }

                int startpoint = GlobalFuncts.ParseStringToInt(textBox1.Text) + br.offsetbytes;
                int length = br.bytes;

                byte[] bytearray = Encoding.UTF8.GetBytes(richTextBox3.Text);
                byte[] finarray = new byte[length];

                for (int i = 0; i < length; i++)
                {
                    finarray[i] = bytearray[i];
                }

                BinaryWriter bw = new BinaryWriter(File.OpenWrite(Settings.Default.BinPath));
                bw.BaseStream.Position = startpoint;
                bw.Write(finarray);
                bw.Close();

            }
            catch {
                brindex = 0; UpdateMessageBox("No ListBox Item Selected. You must select an item from the schema list to ensure that you do not accidentally overwrite the wrong bytes");
            };

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string offset = textBox1.Text;
            string bytes = textBox2.Text;

            Int32 offseti = (System.Convert.ToInt32(this.textBox1.Text, 16));
            int bytesi = GlobalFuncts.ParseStringToInt(bytes);

            BinaryReader br = new BinaryReader(File.OpenRead(Settings.Default.BinPath.ToString()));
            
            string result = ReadSection((Int32)offseti, bytesi);
            richTextBox1.Text = result;

            br.Close();

            List<string> byteblocks = ByteReader.ConvertHexStringToTwoByteBlocks(result);
            byte[] bytearray = new byte[byteblocks.Count];

            try
            {
                int i = int.Parse(byteblocks[0], System.Globalization.NumberStyles.HexNumber);
                ParseBytesToIntsToRTB2(byteblocks);
                
            }
            catch
            {
                UpdateMessageBox(@"Error loading bytes. The most likely reason for this is that the number you've added to \" + "NumVals\" " +
                    "exceeds the length of the .bin file. Try a smaller number of a different offset point.");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string dt = "";

            if (radioButton1.Checked) { dt = "Int32"; }
            if (radioButton2.Checked) { dt = "Int64"; }
            if (radioButton3.Checked) { dt = "String"; }
            if (radioButton4.Checked) { dt = "Int16"; }

            listBox1.Items[listBox1.SelectedIndex] = textBox3.Text + "," + textBox4.Text + "," + textBox5.Text + "," + dt;
        }

        #endregion

        #region "MenuFunctions"

        private void openBinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofd.ShowDialog();
            Settings.Default.BinPath = ofd.FileName;
            filepath = ofd.FileName;
            string filename = Path.GetFileNameWithoutExtension(filepath);

            FileManager.BackupSystem(AppDomain.CurrentDomain.BaseDirectory, filename, "binbak");

            UpdateMessageBox("Currently Reading and saving to " + filepath);
            Settings.Default.Save();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            schemaofd.ShowDialog();
            Settings.Default.SchemaPath = schemaofd.FileName;
            schemapath = schemaofd.FileName;
            LoadSchema(schemapath);
            //UpdateMessageBox("Schema Path Saved: " + filepath);
            Settings.Default.Save();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> lines = new List<string>();
            foreach (string s in listBox1.Items)
            {
                lines.Add(s);
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();
            string fullpath = sfd.FileName;
            string directory = Path.GetDirectoryName(fullpath);
            string filename = Path.GetFileNameWithoutExtension(fullpath);

            FileManager.Save(directory, filename, lines, true, true);
        }

        private void opencsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            
        }

        #endregion

        #region ListBoxControls

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                textBox3.Text = ByteReader.GetByteReaderFromConcatString(listBox1.SelectedItem.ToString()).name;
                textBox4.Text = ByteReader.GetByteReaderFromConcatString(listBox1.SelectedItem.ToString()).offsetbytes.ToString();
                textBox5.Text = ByteReader.GetByteReaderFromConcatString(listBox1.SelectedItem.ToString()).bytes.ToString();

                switch (ByteReader.GetByteReaderFromConcatString(listBox1.SelectedItem.ToString()).datatype)
                {
                    case DataType.Int16_:
                        radioButton4.Checked = true;
                        break;
                    case DataType.Int32_:
                        radioButton1.Checked = true;
                        break;
                    case DataType.Int64_:
                        radioButton2.Checked = true;
                        break;
                    case DataType.String_:
                        radioButton3.Checked = true;
                        break;
                }
            }
            catch
            {
                UpdateMessageBox("Cannot Add Schema Options. Did you click whitespace?");
            }
            
            string record = "";
            try
            {
                record = ByteReader.ReadRecord(ByteReader.GetByteReaderFromConcatString(listBox1.SelectedItem.ToString()), richTextBox1.Text);
                richTextBox3.Text = record;
            }
            catch { UpdateMessageBox("Fatal Error, did you load the hex string?"); }
            
        }

        void TempAddDataToListBoxForTesting()
        {
            
        }

        #endregion

        #region "DataGridViewControls

        void AddDGVColumn(ByteReader br)
        {
            DataGridViewTextBoxColumn dgvc = new DataGridViewTextBoxColumn();
            switch (br.datatype)
            {
                case DataType.Int16_:
                    dgvc.Name = br.name;
                    dgvc.ValueType = typeof(Int16);
                    dataGridView1.Columns.Add(dgvc);
                    break;
                case DataType.Int32_:
                    dgvc.Name = br.name;
                    dgvc.ValueType = typeof(Int32);
                    dataGridView1.Columns.Add(dgvc);
                    break;
                case DataType.Int64_:
                    dgvc.Name = br.name;
                    dgvc.ValueType = typeof(Int64);
                    dataGridView1.Columns.Add(dgvc);
                    break;
                case DataType.String_:
                    dgvc.Name = br.name;
                    dgvc.ValueType = typeof(String);
                    dataGridView1.Columns.Add(dgvc);
                    break;
                default:
                    break;
            }

        }

        void AddColumns(List<ByteReader> brlist)
        {
            foreach (ByteReader br in brlist)
            {
                AddDGVColumn(br);
            }
        }

        void AddRows(List<Record> recordlist)
        {
            int cvcounter = 0;
            DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[0].Clone();

            foreach (Record r in recordlist)
            {
                foreach(string s in r.cellvalues)
                {
                    row.Cells[cvcounter].Value = s;
                    cvcounter++;
                }
                dataGridView1.Rows.Add(row);
                row = (DataGridViewRow)dataGridView1.Rows[0].Clone();
                cvcounter = 0;
            }
        }

        #endregion

        # region "Read Data"

        int ConvertHexStringToInt(string s)
        {
            int i = Convert.ToInt32(s);
            return i;
        }

        void ParseBytesToIntsToRTB2 (List<string> twobytehexvals)
        {
            string result = "";
            foreach (string s in twobytehexvals)
            {
                result = result + int.Parse(s, System.Globalization.NumberStyles.HexNumber) + " ";
            }

            richTextBox2.Text = result;
        }

        byte[] StringToByteArray(string hexencodedarray)
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

        //Can I delete this??
        List<string> ProcessString(string s)
        {
            int counter = 0;
            string byt = "";

            List<string> stringlist = new List<string>();

            foreach (char c in richTextBox1.Text)
            {
                switch (counter)
                {
                    case 0:
                        byt = byt + c;
                        counter = 1;
                        break;
                    case 1:
                        byt = byt + c;
                        stringlist.Add(byt);
                        byt = "";
                        counter = 0;
                        break;
                }
            }
            return stringlist;
        }
        
        List<string> GetHexBlockListFromFile(int startpoint, int endpoint, int recordlength)
        {
            listofstartpositions.Clear();

            int currentposition = startpoint;

            List<string> hexblocklist = new List<string>();

            string result = "";
            bool read = true;

            while (currentposition < endpoint)
            {
                if (read)
                {
                    listofstartpositions.Add(currentposition);
                    result = ReadSection(currentposition, recordlength);
                    currentposition = currentposition + recordlength;
                    hexblocklist.Add(result);
                    read = false;
                }
                else //don't read
                {
                    result = ReadSection(currentposition, recordlength);
                    List<string> byteblocks = ByteReader.ConvertHexStringToTwoByteBlocks(result);

                    if (byteblocks.Exists(x => x != "00"))
                    {
                        currentposition = currentposition + byteblocks.FindIndex(x => x != "00");
                        read = true;
                    }
                    else
                    {
                        currentposition = currentposition + recordlength;
                        read = false;
                    }

                    //if the first value is a readable character, then roll read it, else find the new start position.
                    //if there is no new start position in this thread, keep reading until you find one. 
                    //watch for going above the endpoint
                }
            }
            return hexblocklist;
            //}
            

        }

        //returns a block of hexstring
        string ReadSection(int startposition, int numbytes) 
        {
            //try
            //{
                BinaryReader binaryreader = new BinaryReader(File.OpenRead(Settings.Default.BinPath.ToString()));
                string s = "";

                for (int i = startposition; i <= (startposition + numbytes)-1; i++)
                {
                    binaryreader.BaseStream.Position = i;
                    s += binaryreader.ReadByte().ToString("X2"); //"X2 forces Hexidecimal Format"
                }

                binaryreader.Close();
                return s;
            ////}
            ////catch
            ////{
            // //   UpdateMessageBox("@Cannot load string. There are many reasons this could be happening. " +
            //   //     "Check the following: Make sure you have selected a valid binary hex file. " +
            //        "Make sure that you have entered a hex value in the Offset box that begins with \"0x\". " +
            //        "Make sure that the value in the \"Num Vals\" box wouldn't exceed the length of the entire .bin file. " +
            //        "The program cannot handle being read past the end of the file\"");
            //    return "";
            ////}
            
        }

        #endregion

            }
}
        
