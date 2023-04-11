using ISASimulator;
using Microsoft.VisualStudio.CodeCoverage;

namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void MovTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "1"},
                new string[] {"MOV", "rax", "5"},
                new string[] {"MOV", "rax", "9"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 9);
        }

        [TestMethod]
        public void AddTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "1"},
                new string[] {"ADD", "rax", "9"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 10);
        }

        [TestMethod]
        public void SubTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "10"},
                new string[] {"SUB", "rax", "5"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 5);
        }

        [TestMethod]
        public void MulTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string [] {"MOV", "rax", "5"},
                new string [] {"MUL", "rax", "5"},
                new string [] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 25);
        }

        [TestMethod]
        public void DivTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "99"},
                new string[] {"DIV", "9"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 11);
        }

        [TestMethod]
        public void AndTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "10"},
                new string[] {"AND", "rax", "7"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 2);
        }

        [TestMethod]
        public void OrTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "10"},
                new string[] {"OR", "rax", "5"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 15);
        }

        [TestMethod]
        public void NotTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "2"},
                new string[] {"NOT", "rax"},
                new string[] { "END" },
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], -3);
        }

        [TestMethod]
        public void XorTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "7"},
                new string[] {"XOR", "rax", "6"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 1);
        }

        [TestMethod]
        public void JmpTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "1"},
                new string[] {"MOV", "rax", "2"},
                new string[] {"JMP", "label"},
                new string[] {"MOV", "rax", "100"},
                new string[] {"label:"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 2);
        }

        [TestMethod]
        public void JgeTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "1"},
                new string[] {"MOV", "rax", "2"},
                new string[] {"CMP", "rax", "10"},
                new string[] {"JGE", "label"},
                new string[] {"MOV", "rax", "100"},
                new string[] {"label:"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();
            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 100);
        }

        [TestMethod]
        public void SelfModTest()
        {
            Simulator sim = new();

            sim.input = new List<string[]>()
            {
                new string[] {"MOV", "rax", "1"},
                new string[] {"ADD", "rax", "2"},
                new string[] {"WRITE", "rax"},
                new string[] {"END"},
            };

            sim.LoadInputToMemory();

            sim.memory[16] = 3;

            sim.Run();

            Assert.AreEqual(sim.registers["rax"], 4);
        }
    }
}