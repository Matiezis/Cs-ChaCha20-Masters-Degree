using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberOfOps
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderName = "sts";
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\Mateusz\Desktop\" + folderName);// is your Folder
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            int[] numbers = new int[13];
            int value = 0;
            foreach (FileInfo file in Files)
            {
                value = 0;
                string[] tempArray = file.Name.Split('_');
                for(int i = 0; i < 12; i++)
                {
                    if(tempArray[1][i] == '1')
                    {
                        value += 1;
                    }
                }
                numbers[value] += 1;
            }
            using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + folderName + "_operationNumber.txt"))
            {
                for (int i = 0; i < 13; i++)
                    sw.WriteLine(i.ToString() + " : " + numbers[i].ToString());
            }
        }
    }
}
