using System;
using System.Collections.Generic;
using System.IO;

namespace ReportManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderName = "random2";
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\Mateusz\Desktop\"+folderName);// is your Folder
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            int j = 0;
            List<string>[] proportions = new List<string>[Files.Length];
            for (int i = 0; i < Files.Length; i++)
                proportions[i] = new List<string>();
            int value = 0, total = 0, number = 0, cvalue = 0, ctotal = 0, svalue = 0, stotal = 0;
            string tempString = "";
            double[,] operationQuality = new double[12, 16];
            int q = 0, save=0, secondi = 0;
            while (q < Files.Length)
            {
                if (q!=0 && q % 200 == 0 || q==Files.Length-1)
                {
                    for (int i = save+1; i < q; i++)
                    {
                        for (int k = 0; k < 12; k++)
                        {
                            if (proportions[i][0][k] == '1')
                            {
                                operationQuality[k, 15] += 1;
                                for (int l = 1; l < 14; l++)
                                {
                                    if (operationQuality[k, l] == 0)
                                    {
                                        operationQuality[k, l] = Convert.ToDouble(proportions[i][l].Substring(0, proportions[i][l].IndexOf(' ')));                                    }
                                    else
                                    {
                                        operationQuality[k, l] = ((operationQuality[k, l]* (operationQuality[k, 15]-1)) + Convert.ToDouble(proportions[i][l].Substring(0, proportions[i][l].IndexOf(' ')))) / operationQuality[k, 15];
                                    }
                                }
                                operationQuality[k, 14] += 1;
                            }
                        }
                        secondi = i;
                    }
                    save += (secondi-save)+1;
                    using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + folderName +q.ToString()+ "_operationQuality.txt"))
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            int padInt = 30;
                            sw.WriteLine("Operation number: ".PadRight(padInt, ' ') + (i + 1));
                            sw.WriteLine("Average :".PadRight(padInt, ' ') + operationQuality[i, 1]);
                            sw.WriteLine("Frequency :".PadRight(padInt, ' ') + operationQuality[i, 2]);
                            sw.WriteLine("BlockFrequency :".PadRight(padInt, ' ') + operationQuality[i, 3]);
                            sw.WriteLine("Runs :".PadRight(padInt, ' ') + operationQuality[i, 4]);
                            sw.WriteLine("Longest Run :".PadRight(padInt, ' ') + operationQuality[i, 5]);
                            sw.WriteLine("Rank :".PadRight(padInt, ' ') + operationQuality[i, 6]);
                            sw.WriteLine("FFT :".PadRight(padInt, ' ') + operationQuality[i, 7]);
                            sw.WriteLine("OverlappingTemplate :".PadRight(padInt, ' ') + operationQuality[i, 8]);
                            sw.WriteLine("ApproximateEntropy :".PadRight(padInt, ' ') + operationQuality[i, 9]);
                            sw.WriteLine("LinearComplexity :".PadRight(padInt, ' ') + operationQuality[i, 10]);
                            sw.WriteLine("NonOverlappingTemplate :".PadRight(padInt, ' ') + operationQuality[i, 11]);
                            sw.WriteLine("CumulativeSums :".PadRight(padInt, ' ') + operationQuality[i, 12]);
                            sw.WriteLine("Serial :".PadRight(padInt, ' ') + operationQuality[i, 13]);
                            sw.WriteLine("Count :".PadRight(padInt, ' ') + operationQuality[i, 14]);
                            sw.WriteLine("=================================================================================");
                        }
                    }
                    proportions = new List<string>[Files.Length];
                    for (int i = 0; i < Files.Length; i++)
                        proportions[i] = new List<string>();
                    value = 0;
                    total = 0;
                    number = 0;
                    cvalue = 0;
                    ctotal = 0;
                    svalue = 0;
                    stotal = 0;
                    operationQuality = new double[12, 16];
                }
                using (StreamReader sr = new StreamReader(Files[q].FullName))
                {
                    string[] tempArray = Files[q].Name.Split('_');
                    proportions[j].Add(tempArray[1]);
                    proportions[j].Add("");
                    while (sr.Peek() >= 0)
                    {
                        tempString = sr.ReadLine();
                        if (tempString.Contains('/') && !tempString.Contains("output") && !tempString.Contains("RandomExcursion") && !tempString.Contains("Universal"))
                        {
                            if (tempString.Contains("NonOverlappingTemplate"))
                            {
                                int tempIndex = tempString.IndexOf('/');
                                value += Convert.ToInt16(tempString.Substring(tempIndex - 4, 4));
                                total += Convert.ToInt16(tempString.Substring(tempIndex + 1, 4));
                                number++;
                            }
                            else
                                if (tempString.Contains("CumulativeSums"))
                            {
                                int tempIndex = tempString.IndexOf('/');
                                cvalue += Convert.ToInt16(tempString.Substring(tempIndex - 4, 4));
                                ctotal += Convert.ToInt16(tempString.Substring(tempIndex + 1, 4));
                            }
                            else
                                if (tempString.Contains("Serial"))
                            {
                                int tempIndex = tempString.IndexOf('/');
                                svalue += Convert.ToInt16(tempString.Substring(tempIndex - 4, 4));
                                stotal += Convert.ToInt16(tempString.Substring(tempIndex + 1, 4));
                            }
                            else
                            {
                                int tempIndex = tempString.IndexOf('/');
                                proportions[j].Add((Convert.ToDouble(tempString.Substring(tempIndex - 4, 4)) / Convert.ToDouble(tempString.Substring(tempIndex + 1, 4))).ToString() + tempString.Substring(tempIndex + 4));
                            }
                        }
                    }
                    proportions[j].Add(((double)value / (double)total).ToString() +
                        "     NonOverlappingTemplate");
                    value = 0;
                    total = 0;
                    number = 0;
                    proportions[j].Add(((double)cvalue / (double)ctotal).ToString() +
                        "     CumulativeSums");
                    cvalue = 0;
                    ctotal = 0;
                    proportions[j].Add(((double)svalue / (double)stotal).ToString() +
                        "     Serial");
                    svalue = 0;
                    stotal = 0;
                }
                double newValue = 0;
                for (int k = 2; k < proportions[j].Count; k++)
                {
                    newValue += Convert.ToDouble(proportions[j][k].Substring(0, proportions[j][k].IndexOf(' ')));
                    number++;
                }
                proportions[j][1] = (newValue / number).ToString() + "     Average";
                newValue = 0;
                number = 0;
                j++;
                q++;
            }
        }
    }
}
