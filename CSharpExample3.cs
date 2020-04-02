using SpssLib.DataReader;
using SpssLib.SpssDataset;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SPSSLabelValueCombiner
{
    //A program that takes an SPSS file and produces a tabulated output with labels and values in the data.
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
                string combinedOutputFileName = directory + "\\" + fileNameWithoutExtension + "_COMBINED.TXT";
                string labelOutputFileName = directory + "\\" + fileNameWithoutExtension + "_LABELS.TXT";
                using (SpssReader spssReader = new SpssReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    using (StreamWriter combinedStreamWriter = new StreamWriter(combinedOutputFileName))
                    {
                        StringBuilder header = new StringBuilder();
                        string delimiter = "|";
                        using (StreamWriter labelStreamWriter = new StreamWriter(labelOutputFileName))
                        {
                            labelStreamWriter.WriteLine("Name" + delimiter + "Label");
                            foreach (Variable variable in spssReader.Variables)
                            {
                                labelStreamWriter.WriteLine(variable.Name + delimiter + variable.Label);
                                if (variable.ValueLabels.Any())
                                {
                                    header.Append(variable.Name + "Value" + delimiter);
                                    header.Append(variable.Name + "Label" + delimiter);
                                }
                                else
                                {
                                    header.Append(variable.Name + delimiter);
                                }
                            }
                        }
                        Console.WriteLine(string.Format("{0} created.", labelOutputFileName));
                        combinedStreamWriter.WriteLine(header);
                        foreach (Record record in spssReader.Records)
                        {
                            StringBuilder outputRecord = new StringBuilder();
                            foreach (Variable variable in spssReader.Variables)
                            {
                                double value = Convert.ToDouble(record.GetValue(variable));
                                string label = "";
                                if (variable.ValueLabels.TryGetValue(value, out label))
                                {
                                    outputRecord.Append(value + delimiter);
                                    outputRecord.Append("\"" + label + "\"" + delimiter);
                                }
                                else
                                {
                                    outputRecord.Append(value + delimiter);
                                }
                            }
                            combinedStreamWriter.WriteLine(outputRecord);
                        }
                        Console.WriteLine(string.Format("{0} created.", combinedOutputFileName));
                    }                  
                }
            }
            Console.WriteLine("Done.");
            System.Threading.Thread.Sleep(-1);
        }
    }
}
