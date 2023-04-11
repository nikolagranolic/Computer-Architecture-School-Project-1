using ISASimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ISASimulator
{
    public class Simulator
    {
        private long startingCodeSegAddress;
        private long codeSegAddress;

        private Dictionary<string, byte> dictionary = new();
        private Dictionary<string, long> labels = new();
        public Dictionary<long, byte>? memory;
        public Dictionary<string, long> registers = new();
        private Dictionary<byte, string> registersDecode = new();

        public List<string[]>? input;
        private bool hasCmp = false;

        private Analyzer analyzer = new();
        private Loader? loader;
        private CPU? cpu;
        public Simulator()
        {
            startingCodeSegAddress = 0;
            codeSegAddress = startingCodeSegAddress;

            dictionary.Add("ADD", 1);   
            dictionary.Add("SUB", 2);   
            dictionary.Add("MUL", 3);   
            dictionary.Add("DIV", 4);   
            dictionary.Add("AND", 5);   
            dictionary.Add("OR", 6);    
            dictionary.Add("NOT", 7);   
            dictionary.Add("XOR", 8);   
            dictionary.Add("MOV", 9);   
            dictionary.Add("BREAK", 11);
            dictionary.Add("JMP", 12);  
            dictionary.Add("CMP", 13);  
            dictionary.Add("JE", 14);   
            dictionary.Add("JNE", 15);  
            dictionary.Add("JGE", 16);  
            dictionary.Add("JL", 17);   
            dictionary.Add("WRITE", 18);
            dictionary.Add("READ", 19);
            dictionary.Add("END", 20);

            dictionary.Add("rax", 41);
            dictionary.Add("rbx", 42);
            dictionary.Add("rcx", 43);
            dictionary.Add("rdx", 44);
            dictionary.Add("cmp", 45);

            registers.Add("rax", 0);
            registers.Add("rbx", 0);
            registers.Add("rcx", 0);
            registers.Add("rdx", 0);
            registers.Add("cmp", 0);

            registersDecode.Add(41, "rax");
            registersDecode.Add(42, "rbx");
            registersDecode.Add(43, "rcx");
            registersDecode.Add(44, "rdx");
            registersDecode.Add(45, "cmp");
        }

        public void ParseInput(string inputFilePath)
        {
            input = analyzer.Parse(inputFilePath);
        }

        public void LoadInputToMemory()
        {
            loader = new Loader(dictionary, labels, registers, input, codeSegAddress, hasCmp);
            memory = loader.LoadInputToMemory();
        }

        public void Run()
        {
            cpu = new CPU(dictionary, labels, registers, registersDecode, memory);
            cpu.Run();
        }
    }
}
