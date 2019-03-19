using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class ChaCha20
    {
        public ChaCha20()
        {
        }
        public uint[] GenerateKeystreamLonger(uint[] input, int rounds, int keyStreamLength, char[] operations)
        {
            uint[] output = new uint[input.Length * keyStreamLength];
            for(int i=0; i < input.Length * keyStreamLength; i+=input.Length)
            {
                GenerateKeystream(input, rounds, operations).CopyTo(output, i);
                input[12] += 1;
                if (input[12] == 0) { 
                    input[13] += 1;
                    if(input[13] == 0)
                    {
                        return output;
                    }
                }
            }
            return output;
        }
        public uint[] GenerateKeystream(uint[] input, int rounds, char[] operations)
        {
            uint[] output = new uint[input.Length];
            input.CopyTo(output, 0);
            for (int i = 0; i < rounds; i++)
            {
                if (i % 2 == 0)//Even 
                {
                        output = QuarterRound(output, 0, 4, 8, 12, operations);
                        output = QuarterRound(output, 1, 5, 9, 13, operations);
                        output = QuarterRound(output, 2, 6, 10, 14, operations);
                        output = QuarterRound(output, 3, 7, 11, 15, operations);
                    }
                else//Odd
                {
                        output = QuarterRound(output, 0, 5, 10, 15, operations);
                        output = QuarterRound(output, 1, 6, 11, 12, operations);
                        output = QuarterRound(output, 2, 7, 8, 13, operations);
                        output = QuarterRound(output, 3, 4, 9, 14, operations);
                   }
            }
            for (int i = 0; i < 16; i++) //
                output[i] += input[i];
            return output;
        }
        private uint[] QuarterRound(uint[] input, int a, int b, int c, int d,char[] operations)
        {
            if(operations[0]=='1')
                input[a] += input[b];
            if (operations[1] == '1')
                input[d] ^= input[a];
            if (operations[2] == '1')
                input[d] = (input[d] << 16) | (input[d] >> (32 - 16));
            if (operations[3] == '1')
                input[c] += input[d];
            if (operations[4] == '1')
                input[b] ^= input[c];
            if (operations[5] == '1')
                input[b] = (input[b] << 12) | (input[b] >> (32 - 12));
            if (operations[6] == '1')
                input[a] += input[b];
            if (operations[7] == '1')
                input[d] ^= input[a];
            if (operations[8] == '1')
                input[d] = (input[d] << 8) | (input[d] >> (32 - 8));
            if (operations[9] == '1')
                input[c] += input[d];
            if (operations[10] == '1')
                input[b] ^= input[c];
            if (operations[11] == '1')
                input[b] = (input[b] << 7) | (input[b] >> (32 - 7));
            return input;
        }
        public string ShowHex(uint[] input)
        {
            string output = "";

            byte[] messageTest = input.SelectMany(BitConverter.GetBytes).ToArray();
            output = BitConverter.ToString(messageTest);

            return output;
        }

        public UInt32[] ReverseUintArrayBytes(UInt32[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                value[i] = ReverseBytes(value[i]);
            }
            return value;
        }

        public UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public uint[] ConvertStringToUintASCII(string input)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] copiedBytes;
            if (inputBytes.Length % 4 == 0)
            {
                copiedBytes = new byte[inputBytes.Length];
                inputBytes.CopyTo(copiedBytes, 0);
            }
            else
            {
                copiedBytes = new byte[inputBytes.Length + (4 - (inputBytes.Length % 4))];
                inputBytes.CopyTo(copiedBytes, 0);
                for (int i = 1; i < 4 - (inputBytes.Length % 4); i++)
                {
                    copiedBytes[inputBytes.Length + i] = 0;
                }
            }
            uint[] output;
            if (inputBytes.Length % 4 != 0)
            {
                output = new uint[copiedBytes.Length / 4];
            }
            else
                output = new uint[copiedBytes.Length / 4];
            for (int i = 0; i < output.Length * 4; i += 4)
            {
                output[i / 4] = BitConverter.ToUInt32(copiedBytes, i);
            }
            return output;
        }
    }
}
