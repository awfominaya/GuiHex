using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GUIHex.Properties;

namespace GUIHex
{
    class FileManager
    {
        static string defaultbasepath = AppDomain.CurrentDomain.BaseDirectory;
        static string defaultextension = "Schema";
        static string backupextension = "Bak";

        #region "SaveFile"

        public static void Save(string directory, string filename, List<string> lines, bool overwrite, bool usebackupsystem)
        {
            string backupfilename = "";

            if (overwrite) { CreateDirectory(directory); }

            WriteFile(directory, filename + "." + defaultextension, lines);

            if (usebackupsystem)
            {
                backupfilename= BackupSystem(directory, filename, defaultextension + backupextension);
                WriteFile(directory + "//"+"Backup"+"//", backupfilename, lines);
            }
        }

        public static string BackupSystem(string directory, string filename, string extension) //returns the new filepath
        {
            CreateDirectory(directory + "//" + "Backup");
            string pathtest = directory + "//backup//" + filename + "0." + extension;
            bool test = Directory.Exists(pathtest);

            if (System.IO.File.Exists(directory + "//backup//" + filename + "0." + extension))
            {
                if (System.IO.File.Exists(directory + "//backup//" + filename + "1." + extension))
                {
                    if(System.IO.File.Exists(directory + "//backup//" + filename + "2." + extension))
                    {
                        File.Delete(directory + "//backup//" + filename + "0." + extension);
                        File.Copy(directory + "//backup//" + filename + "1." + extension, directory + "//backup//" + filename + "0." + extension);
                        File.Delete(directory + "//backup//" + filename + "1." + extension);
                        File.Copy(directory + "//backup//" + filename + "2." + extension, directory + "//backup//" + filename + "1." + extension);
                        File.Delete(directory + "//backup//" + filename + "2." + extension);
                        return filename + "2." + extension;
                    }
                    else
                    {
                        return filename + "2." + extension;
                    }
                }
                else
                {
                    return filename + "1." + extension;
                }
            }
            return filename + "0." + extension;
        }

        static void WriteFile(string directory, string filenameandextension, List<string> lines)
        {
            using (var v = new StreamWriter(directory + "//" + filenameandextension, false))
            {
                foreach (string line in lines)
                {
                    v.WriteLine(line.ToString());
                }
                v.Close();
            }
        }

        public static void CreateDirectory(string path)
        {
            bool exists = System.IO.Directory.Exists(path);
            if (!exists) { System.IO.Directory.CreateDirectory(path); }
        }

    }
    #endregion

}
