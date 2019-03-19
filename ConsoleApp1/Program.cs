using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            string fileDestination = Directory.GetCurrentDirectory() + "/[Dane/";
            if (!Directory.Exists(fileDestination))
            {
                DirectoryInfo tempDirectoryInfo = Directory.CreateDirectory(fileDestination);
            }
            int numberOfRounds = 20; // Number of rounds
            int keyStreamLength = 39063; // x times 512-bit 16384 is megabyte /
            int compareTreshold = 2; //How many words can be left unchanged; 0 means all words need to be changed; 16 means no words need to be changed
            int allVariantsDecimal = 4095; // 12 operations in binary 4095 makes 111 111 111 111 set to 4095
            uint a = 0;
            a -= 1; // used to get the maximum possible value to insert into a 32-bit word
                    // 2863311530 = Every odd set,  1431655765 = every even set

            uint[,] keys = new uint[7, 8] { { a, a, a, a, a, a, a, a},
                { 0, 0, 0, 0, 0, 0, 0, 0},
                { a, a, a, a, 0, 0, 0, 0},
                { 2863311530, 2863311530, 2863311530, 2863311530, 2863311530, 2863311530, 2863311530, 2863311530},
                { 1431655765, 1431655765, 1431655765, 1431655765, 1431655765, 1431655765, 1431655765, 1431655765},
                { 1578910043, 1057447980, 165290405, 571398096, 1594292374, 1546133824, 815907939, 1063986345 },
                { 409712059, 203392419, 1095906744, 3972444612, 3409694117, 2971128328, 211196319, 2436685777 } };
            uint[,] nonces = new uint[7, 2] { { a, a },
                { 0, 0 },
                { 0, 0 },
                { 1431655765, 1431655765 },
                { 2863311530, 2863311530 } ,
                { 1427755126, 614521627 },
                { 1496614300, 1082907386 } };
            string[] names = new string[7] {"allones","allzeros","everyeven","everyodd","halfones","random1","random2" };
            foreach (string name in names)
            {
                if (!Directory.Exists(fileDestination + name +"/"))
                {
                    DirectoryInfo tempDirectoryInfo = Directory.CreateDirectory(fileDestination+name+"/");
                }
            }
            for (int nameIndex = 0; nameIndex < 1; nameIndex++)
            {
                Console.WriteLine("First used set of data : {0}",names[nameIndex]);
                string[,] resultsOfComparison = new string[4096, 2];

                List<string>[] wordsChanged = new List<string>[16];
                for (int i = 0; i < 16; i++)
                    wordsChanged[i] = new List<string>();

                List<string>[] variantQuality = new List<string>[17];
                for (int i = 0; i < 17; i++)
                    variantQuality[i] = new List<string>();
                int[,] operationImportance = new int[12, 17];

                fileDestination = Directory.GetCurrentDirectory() + "/[Dane/";
                fileDestination += names[nameIndex] + "/";

                ChaCha20 chacha = new ChaCha20();
                uint[] constant = chacha.ConvertStringToUintASCII("expand 32-byte k");
                uint[] counter = new uint[2] { 0, 0 };

                uint[] key = new uint[8] { keys[nameIndex,0], keys[nameIndex, 1], keys[nameIndex, 2], keys[nameIndex, 3], keys[nameIndex, 4], keys[nameIndex, 5], keys[nameIndex, 6], keys[nameIndex, 7] };
                uint[] nonce = new uint[2] { nonces[nameIndex,0], nonces[nameIndex,1] };

                char[] operations = Convert.ToString(allVariantsDecimal, 2).PadLeft(12, '0').ToCharArray();

                uint[] StartingState = new uint[16] {
            constant[0], constant[1], constant[2], constant[3],
            key[0], key[1], key[2], key[3],
            key[4], key[5], key[6], key[7],
            counter[0], counter[1], nonce[0], nonce[1] };

                uint[] startingStateCopy = new uint[16];
                StartingState.CopyTo(startingStateCopy, 0);

                byte[] IVBytes = StartingState.SelectMany(BitConverter.GetBytes).ToArray();

                string startingStateBinary = "";
                for (int i = 0; i < IVBytes.Length; i++)
                    startingStateBinary += Convert.ToString(IVBytes[i], 2).PadLeft(8, '0');
                //for (int i = allVariantsDecimal; i >= 0; i -= 1)
                for (int i = allVariantsDecimal; i >= 4085; i -= 1)
                {
                    operations = Convert.ToString(i, 2).PadLeft(12, '0').ToCharArray();

                    startingStateCopy.CopyTo(StartingState, 0);
                    uint[] output = chacha.GenerateKeystreamLonger(StartingState, numberOfRounds, keyStreamLength, operations);

                    int[] compareResultArray = CompareArrays(output, startingStateCopy);
                    int compareResult = 0; // 0 means no words are the same on output. 16 means all words are the same 
                    for (int k = 0; k < 16; k++) // Lower value of compare result means better mixing properties [?]
                    {
                        //Count compareResult for a number of total words changed
                        compareResult += compareResultArray[k];
                        //Save the operation variant into wordsChanged list depending on the words changed or not
                        if (compareResultArray[k] == 1)
                            wordsChanged[k].Add(string.Join("", operations));
                    }
                    for (int k = 0; k < 12; k++)
                    {
                        if (operations[k] == '1')
                            operationImportance[k, compareResult] += 1;
                    }
                    variantQuality[compareResult].Add(string.Join("", operations));

                    resultsOfComparison[i, 0] = string.Join("", compareResultArray);
                    resultsOfComparison[i, 1] = compareResult.ToString();

                    if (compareResult < compareTreshold)
                    {
                        string filename = names[nameIndex] + "_" + new string(operations) + "_output.dat";
                        byte[] outputBytes = output.SelectMany(BitConverter.GetBytes).ToArray();
                        File.WriteAllBytes(fileDestination + filename, outputBytes);
                    }
                    using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + names[nameIndex] + "_finalReport.txt"))
                    {
                        sw.WriteLine("===============================================");
                        sw.WriteLine("Words Changed:");
                        sw.WriteLine("Each word not changed. Operation Variants used that resulted in that word not being changed." +
                            "Two operations seem to be responsible for four words in the matrix." +
                            "Total number of variants: " + allVariantsDecimal.ToString());
                        for (int c = 0; c < 16; c++)
                        {
                            sw.Write(c.ToString().PadLeft(4, '>') + "  ");
                            sw.WriteLine("Count : " + wordsChanged[c].Count().ToString());
                        }
                        sw.WriteLine("===============================================");
                        sw.WriteLine("Variant Quality:");
                        sw.WriteLine("Number of words not changed. Operation Variants listed below." +
                            "Quality of the variants stated in the number of words diffrent to the initial values." +
                            "Total number of variants: " + allVariantsDecimal.ToString());
                        for (int c = 0; c < 17; c++)
                        {
                            sw.Write(c.ToString().PadLeft(4, '>') + "  ");
                            sw.WriteLine("Count : " + variantQuality[c].Count().ToString());
                        }
                        sw.WriteLine("===============================================");
                        sw.WriteLine("Operation Importance:");
                        sw.WriteLine("Number of the operation in the algorithm. Number of words not changed and all occurences in Operation Variants for that particular number of words not changed.");
                        for (int c = 0; c < 12; c++)
                        {
                            sw.WriteLine("Operation Number " + (c + 1).ToString());
                            for (int j = 0; j < 17; j++)
                            {
                                sw.Write((j).ToString().PadLeft(4, '>') + "  ");
                                sw.WriteLine(operationImportance[c, j]);
                            }
                        }
                        sw.WriteLine("===============================================");
                        sw.WriteLine("Words Changed:");
                        sw.WriteLine("Each word not changed. Operation Variants used that resulted in that word not being changed." +
                            "Two operations seem to be responsible for four words in the matrix." +
                            "Total number of variants: " + allVariantsDecimal.ToString());
                        for (int c = 0; c < 16; c++)
                        {
                            sw.Write(">>>" + c.ToString() + "<<<");
                            sw.WriteLine("Count : " + wordsChanged[c].Count().ToString());
                            foreach (string str in wordsChanged[c])
                            {
                                sw.Write(str + " ");
                            }
                            sw.WriteLine("");
                        }
                        sw.WriteLine("===============================================");
                        sw.WriteLine("===============================================");
                        sw.WriteLine("Variant Quality:");
                        sw.WriteLine("Number of words not changed. Operation Variants listed below." +
                            "Quality of the variants stated in the number of words diffrent to the initial values." +
                            "Total number of variants: " + allVariantsDecimal.ToString());

                        for (int c = 0; c < 17; c++)
                        {
                            sw.WriteLine(">>>" + c.ToString() + "<<<");
                            sw.WriteLine("Count : " + variantQuality[c].Count().ToString());
                            foreach (string str in variantQuality[c])
                            {
                                sw.Write(str + " ");
                            }
                            sw.WriteLine("");
                        }
                    }

                }
            }
            Console.WriteLine("Finished");
            Console.ReadKey();
        }
        static int[] CompareArrays(uint[] arr1, uint[] arr2)
        {
            int[] result = new int[16];
            uint[] test = new uint[16];
            arr2.CopyTo(test, 0);
            for (int i = 0; i < 16; i++)
                test[i] += test[i];
                for(int i = 0; i < 16; i++)
                {
                    if (arr1[i] == test[i])
                        result[i] = 1;
                }
            return result;
        }
    }
}
