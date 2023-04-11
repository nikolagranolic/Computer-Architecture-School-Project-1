using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISASimulator
{
    internal class Analyzer
    {
        public List<string[]> Parse(string inputFilePath)
        {
            List<string[]> result = new();
            string[] lines = File.ReadAllLines(inputFilePath);
            foreach (string line in lines)
            {
                result.Add(line.Split(' '));
            }

            return result;
        }
    }
}
