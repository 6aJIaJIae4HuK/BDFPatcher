using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDFPatcher
{
    class MainProgram
    {
        public const string iniFileName = "ini.ini";

        private string sourcePath;
        private string targetPath;
        
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
                
                Dictionary<string, List<string>> patients = new Dictionary<string, List<string>>();

                foreach (string file in files)
                {
                    string fileName = file.Substring(file.LastIndexOf("\\") + 1);
                    string tmp = fileName.Substring(0, fileName.LastIndexOf("_cmpltd.bdf"));
                    string patientName = tmp.Substring(tmp.LastIndexOf('_') + 1);
                    if (!patients.ContainsKey(patientName))
                    {
                        patients.Add(patientName, new List<string>());
                    }
                    patients[patientName].Add(file);
                }

                foreach (string patient in patients.Keys)
                {
                    if (patient.Equals("Asustek")) ;
                        //continue;

                    List<BDFFile> BDFFiles = new List<BDFFile>();
                    foreach (string fileName in patients[patient])
                    {
                        BDFFiles.Add(new BDFFile());
                        Console.WriteLine("Reading file {0}", fileName.Substring(fileName.LastIndexOf('\\') + 1));
                        BDFFiles.Last().readFromFile(fileName);
                    }

                    BDFFiles.Sort((x, y) => DateTime.Compare(x.Header.StartDateTime, y.Header.StartDateTime));

                    DateTime curBegin = BDFFiles.First().Header.StartDateTime;

                    List<BDFFile> curSubList = new List<BDFFile>();

                    foreach (BDFFile file in BDFFiles)
                    {
                        if ((file.Header.StartDateTime - curBegin).TotalHours >= 24)
                        {
                            saveSubList(patient, curSubList);
                            curSubList.Clear();
                            curBegin = file.Header.StartDateTime;
                        }
                        else
                        {
                            curSubList.Add(file);
                        }
                    }
                    if (curSubList.Any())
                        saveSubList(patient, curSubList);
                    BDFFiles.Clear();
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

        private string generateFileName(DateTime dateTime, string patientName)
        {
            string res = "";

            res += dateTime.Year.ToString("D2");
            res += dateTime.Month.ToString("D2");
            res += dateTime.Day.ToString("D2");

            res += '_';

            res += dateTime.Hour.ToString("D2");
            res += dateTime.Minute.ToString("D2");
            res += dateTime.Second.ToString("D2");

            res += '_';

            res += patientName;

            res += "_cmpltd.bdf";

            return res;
        }

        private void saveSubList(string patientName, List<BDFFile> files)
        {
            BDFFile newFile = new BDFFile();
            newFile.generateFromFiles(files);
            string newFileFullPath = sourcePath + patientName + '\\' + generateFileName(newFile.Header.StartDateTime, patientName);

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(newFileFullPath);
            fileInfo.Directory.Create();
            newFile.saveToFile(fileInfo.FullName);
        }
    }
}