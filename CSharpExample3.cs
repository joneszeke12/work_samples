using SpssLib.DataReader;
using SpssLib.SpssDataset;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SPSSExtractor
{
    //A program that takes a SPSS file and produces a KV-like output of labels and data.
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {           
            OpenFileDialog openFileDialog = new OpenFileDialog() {Multiselect = false, Filter = "SPSS Files(*.sav)|*.sav"};
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("Running.");
                string fileName = openFileDialog.FileName;
                string directory = Path.GetDirectoryName(fileName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string outputFileName = directory + "\\" + fileNameWithoutExtension + "_OUTPUT.TXT";
                string labelOutputFileName = directory + "\\" + fileNameWithoutExtension + "_LABELS.TXT";
                using (SpssReader spssReader = new SpssReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    using (StreamWriter streamWriter = new StreamWriter(outputFileName))
                    {
                        string delimiter = "|";
                        using (StreamWriter labelStreamWriter = new StreamWriter(labelOutputFileName))
                        {
                            labelStreamWriter.WriteLine("Name%1Label%1Value%1ValueLabel".Replace("%1",delimiter));
                            foreach (Variable variable in spssReader.Variables)
                            {
                                if (variable.ValueLabels.Any())
                                {
                                    foreach (KeyValuePair<double, string> pair in variable.ValueLabels)
                                    {
                                        labelStreamWriter.WriteLine("\"" + variable.Name + "\"" + delimiter + "\"" + variable.Label + "\"" + delimiter + pair.Key + delimiter + pair.Value);
                                    }
                                }
                                else
                                {
                                    labelStreamWriter.WriteLine("\"" + variable.Name + "\"" + delimiter + "\"" + variable.Label + "\"" + delimiter + delimiter);
                                }
                            }
                        }
                        Console.WriteLine(string.Format("{0} created.", labelOutputFileName));
                        streamWriter.WriteLine("GeneratedUserID" + delimiter + "Key" + delimiter + "Value");
                        int i = 1;
                        foreach (Record record in spssReader.Records)
                        {
                            foreach (Variable variable in spssReader.Variables)
                            {
                                if (!(record.GetValue(variable) == null))
                                {
                                    streamWriter.WriteLine(i + delimiter + variable.Name + delimiter + record.GetValue(variable));
                                }
                            }
                            i++;
                        }
                        Console.WriteLine(string.Format("{0} created.", outputFileName));
                    }                  
                }
                Console.WriteLine("Done.");
            }
            System.Threading.Thread.Sleep(-1);
        }
    }
}
