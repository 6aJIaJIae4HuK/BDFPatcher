using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDFPatcher
{
    class MainProgram
    {
        public const string iniFileName = "..\\..\\ini.ini";

        private string sourcePath;
        private string targetPath;
        private Dictionary<string, List<BDFFile>> table;

        public static void Main(string[] argv)
        {
            MainProgram app = new MainProgram();
            app.run();
        }

        public void run()
        {
            try
            {
                initPaths();
                string[] files = System.IO.Directory.GetFiles(sourcePath, "*_*_*_cmpltd.bdf");
                table = new Dictionary<string, List<BDFFile>>();

                foreach (string file in files)
                {
                    string fileName = file.Substring(file.LastIndexOf("\\") + 1);
                    string tmp = fileName.Substring(0, fileName.LastIndexOf("_cmpltd.bdf"));
                    string patientName = tmp.Substring(tmp.LastIndexOf('_') + 1);
                    if (!table.ContainsKey(patientName))
                    {
                        table.Add(patientName, new List<BDFFile>());
                    }
                    table[patientName].Add(new BDFFile());
                    table[patientName].Last().readFromFile(file);
                    Console.WriteLine("Read file {0}", fileName);
                }

                foreach (string key in table.Keys)
                {
                    //table[key].Sort((x, y) => DateTime.Compare(x.Header.StartDateTime, y.Header.StartDateTime));
                    BDFFile newFile = new BDFFile();
                    newFile.generateFromFiles(table[key]);
                    newFile.saveToFile(sourcePath + key + "\\" + "result.bdf");
                }
            }
            catch (Exception e)
            { 
                //???
            }
        }

        private void initPaths()
        {

            string[] lines = null;
            try
            {
                lines = System.IO.File.ReadAllLines(iniFileName);
            }
            catch (Exception e)
            {
                throw e;
            }

            if (lines.Length != 2)
            {
                throw new Exception("Incorrect iniFile");
            }

            try
            {
                sourcePath = getPathArgument("SOURCE_PATH", lines[0]);
                targetPath = getPathArgument("TARGET_PATH", lines[1]);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string getPathArgument(string key, string str)
        {
            if (str.IndexOf(key + '=') != 0)
            {
                throw new Exception("Incorrect iniFile");
            }
            else
            {
                str = str.Substring((key + '=').Length);
                if (!System.IO.Directory.Exists(str))
                {
                    throw new Exception(String.Format("Unknown path: {0}", str));
                }
                else
                {
                    return str;
                }
            }
        }
    }
}
