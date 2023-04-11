using ISASimulator;
using System;
using System.Text.RegularExpressions;

Simulator sim = new();

//READ/WRITE test
//sim.input = new List<string[]>()
//{
//    new string[] {"READ", "[1000]"},
//    new string[] {"WRITE", "[1000]"},
//    new string[] {"END"},
//};

//DEBUG test
sim.input = new List<string[]>()
{
    new string[] {"MOV", "rax", "5"},
    new string[] {"BREAK"},
    new string[] {"ADD", "rax", "5"},
    new string[] {"MOV", "[300]", "2"},
    new string[] {"BREAK", "300"},
    new string[] {"END"},
};



//sim.ParseInput("..\\..\\..\\..\\input3.txt");


sim.LoadInputToMemory();
sim.Run();

//Console.WriteLine(sim.registers["rax"]);
