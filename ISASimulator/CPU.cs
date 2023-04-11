using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ISASimulator
{
    internal class CPU
    {
        private Dictionary<string, byte> dictionary;
        private Dictionary<string, long> labels;
        private Dictionary<string, long> registers;
        private Dictionary<byte, string> registersDecode;
        private Dictionary<long, byte>? memory;
        private Dictionary<long, byte>? debugModeAddresses = new();
        private long programCounter = 0;
        private byte currentInstruction;
        private bool endOfProgram = false;

        private byte firstOperandType, secondOperandType, registerNum;
        private long firstOperand, secondOperand, result = 0, address;

        public CPU(Dictionary<string, byte> dictionary, Dictionary<string, long> labels, Dictionary<string, long> registers, Dictionary<byte, string> registersDecode, Dictionary<long, byte> memory)
        {
            this.dictionary = dictionary;
            this.labels = labels;
            this.registers = registers;
            this.registersDecode = registersDecode;
            this.memory = memory;
        }

        public void Run()
        {
            while (!endOfProgram)
            {
                InstructionFetch();
                InstructionDecodeAndExecute();
            }
        }

        private void InstructionFetch()
        {
            memory.TryGetValue(programCounter++, out currentInstruction);
        }

        private void InstructionDecodeAndExecute()
        {
            switch (currentInstruction)
            {
                case 1:
                    Calculate("ADD");
                    break;
                case 2:
                    Calculate("SUB");
                    break;
                case 3:
                    Calculate("MUL");
                    break;
                case 4:
                    Calculate("DIV");
                    break;
                case 5:
                    Calculate("AND");
                    break;
                case 6:
                    Calculate("OR");
                    break;
                case 7:
                    Not();
                    break;
                case 8:
                    Calculate("XOR");
                    break;
                case 9:
                    Mov();
                    break;
                case 11:
                    Break();
                    break;
                case 12:
                    Jmp();
                    break;
                case 13:
                    Cmp();
                    break;
                case 14:
                    Je();
                    break;
                case 15:
                    Jne();
                    break;
                case 16:
                    Jge();
                    break;
                case 17:
                    Jl();
                    break;
                case 18:
                    Write();
                    break;
                case 19:
                    Read();
                    break;
                case 20:
                    End();
                    break;
                default:
                    throw new Exception("Instruction cannot be decoded.");
            }
        }

        private void Calculate(string instruction)
        {
            string destinationRegister = "";
            long destinationAddress = 0;
            string registerName;

            // first operand fetch
            firstOperandType = ReadByteFromMemory(); 
            switch (firstOperandType) 
            {
                case 101: // if it's a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    destinationRegister = registerName;
                    registers.TryGetValue(registerName, out firstOperand);
                    break;
                case 102: // if it's an address
                    address = ReadLongFromMemory();
                    destinationAddress = address;
                    firstOperand = ReadLongFromMemoryAt(address);
                    break;
                case 104: // if it's an address stored in a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out destinationAddress);
                    firstOperand = ReadLongFromMemoryAt(destinationAddress);
                    break;
            }

            // second operand fetch
            secondOperandType = ReadByteFromMemory();
            switch (secondOperandType)
            {
                case 201: // if it's a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out secondOperand);
                    break;
                case 202:
                    address = ReadLongFromMemory();
                    secondOperand = ReadLongFromMemoryAt(address);
                    break;
                case 203:
                    secondOperand = ReadLongFromMemory();
                    break;
                case 204:
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out address);
                    secondOperand = ReadLongFromMemoryAt(address);
                    break;
            }
            // perform calculation based on instruction
            switch (instruction)
            {
                case "ADD":
                    result = firstOperand + secondOperand;
                    break;
                case "SUB":
                    result = firstOperand - secondOperand;
                    break;
                case "MUL":
                    result = firstOperand * secondOperand;
                    break;
                case "DIV":
                    result = firstOperand / secondOperand;
                    registers["rdx"] = firstOperand % secondOperand;
                    break;
                case "AND":
                    result = firstOperand & secondOperand;
                    break;
                case "OR":
                    result = firstOperand | secondOperand;
                    break;
                case "XOR":
                    result = firstOperand ^ secondOperand;
                    break;
            }
            // deciding where to save the result based on the first operand type
            switch (firstOperandType)
            {
                case 101:
                    registers[destinationRegister] = result;
                    break;
                case 102:
                    WriteLongToMemory(result, destinationAddress);
                    break;
                case 104:
                    WriteLongToMemory(result, destinationAddress);
                    break;
            }
        }

        private void Not()
        {
            string registerName;
            // first operand fetch
            firstOperandType = ReadByteFromMemory();
            switch (firstOperandType)
            {
                case 101: // first operand is a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out firstOperand);

                    result = ~firstOperand;

                    registers[registerName] = result;
                    break;
                case 102: // first operand is an address
                    address = ReadLongFromMemory();
                    firstOperand = ReadLongFromMemoryAt(address);

                    result = ~firstOperand;

                    WriteLongToMemory(result, address);
                    break;
                case 104:
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out address);

                    result = ~firstOperand;

                    firstOperand = ReadLongFromMemoryAt(address);
                    break;
            }
        }

        private void Mov()
        {
            string destinationRegister = "";
            long destinationAddress = 0;
            string registerName;

            // first operand fetch
            firstOperandType = ReadByteFromMemory();
            switch (firstOperandType)
            {
                case 101: // if it's a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    destinationRegister = registerName;
                    break;
                case 102: // if it's an address
                    destinationAddress = ReadLongFromMemory();
                    break;
                case 104: // if it's an address stored in a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out destinationAddress);

                    break;
            }

            // second operand fetch
            secondOperandType = ReadByteFromMemory();
            switch (secondOperandType)
            {
                case 201: // if it's a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out secondOperand);
                    break;
                case 202:
                    address = ReadLongFromMemory();
                    secondOperand = ReadLongFromMemoryAt(address);
                    break;
                case 203:
                    secondOperand = ReadLongFromMemory();
                    break;
                case 204:
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out address);

                    secondOperand = ReadLongFromMemoryAt(address);
                    break;
            }

            // deciding where to save the result based on the first operand type
            switch (firstOperandType)
            {
                case 101:
                    registers[destinationRegister] = secondOperand;
                    break;
                case 102:
                case 104:
                    WriteLongToMemory(secondOperand, destinationAddress);
                    break;
            }
        }

        private void Break()
        {
            // loading all the addresses to be watched
            byte numOfAddresses = 0;
            HashSet<long> addresses = new();
            memory.TryGetValue(programCounter++, out numOfAddresses);
            for (int i = 0; i < numOfAddresses; i++)
            {
                addresses.Add(ReadLongFromMemory());
            }
            DisplayDebugInfo(addresses);
            string option;
            do
            {
                Console.WriteLine("Options: NEXT, CONTINUE");
                option = Console.ReadLine();
                switch (option)
                {
                    case "NEXT":
                        if (!endOfProgram)
                        {
                            InstructionFetch();
                            InstructionDecodeAndExecute();
                        }
                        else
                        {
                            return;
                        }
                        DisplayDebugInfo(addresses);
                        break;
                    case "CONTINUE":
                        break;
                    default:
                        Console.WriteLine("Unknown option.");
                        break;
                }
            } while (!option.Equals("CONTINUE"));
        }

        private void Cmp()
        {
            string registerName;
            long address;

            // first operand fetch
            firstOperandType = ReadByteFromMemory();
            switch (firstOperandType)
            {
                case 101: // if it's a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out firstOperand);
                    break;
                case 102: // if it's an address
                    address = ReadLongFromMemory();
                    firstOperand = ReadLongFromMemoryAt(address);
                    break;
                case 104: // if it's an address stored in a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out address);
                    firstOperand = ReadLongFromMemoryAt(address);
                    break;
            }

            // second operand fetch
            secondOperandType = ReadByteFromMemory();
            switch (secondOperandType)
            {
                case 201: // if it's a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out secondOperand);
                    break;
                case 202:
                    address = ReadLongFromMemory();
                    secondOperand = ReadLongFromMemoryAt(address);
                    break;
                case 203:
                    secondOperand = ReadLongFromMemory();
                    break;
                case 204:
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out address);
                    secondOperand = ReadLongFromMemoryAt(address);
                    break;
            }

            result = firstOperand - secondOperand;
            if (result == 0)
            {
                registers["cmp"] = 0;
            }
            else if (result > 0)
            {
                registers["cmp"] = 1;
            }
            else
            {
                registers["cmp"] = -1;
            }
        }

        private void Jmp()
        {
            address = ReadLongFromMemory();
            programCounter = address;
        }


        private void Je()
        {
            address = ReadLongFromMemory();
            long cmpValue;
            registers.TryGetValue("cmp", out cmpValue);
            if (cmpValue == 0)
            {
                programCounter = address;
            }
        }

        private void Jne()
        {
            address = ReadLongFromMemory();
            long cmpValue;
            registers.TryGetValue("cmp", out cmpValue);
            if (cmpValue != 0)
            {
                programCounter = address;
            }
        }

        private void Jge()
        {
            address = ReadLongFromMemory();
            long cmpValue;
            registers.TryGetValue("cmp", out cmpValue);
            if (cmpValue >= 0)
            {
                programCounter = address;
            }
        }

        private void Jl()
        {
            address = ReadLongFromMemory();
            long cmpValue;
            registers.TryGetValue("cmp", out cmpValue);
            if (cmpValue < 0)
            {
                programCounter = address;
            }
        }

        private void Write()
        {
            string destinationRegister = "";
            long destinationAddress = 0, number = 0;
            string registerName;

            // first operand fetch
            firstOperandType = ReadByteFromMemory();
            switch (firstOperandType)
            {
                case 101: // if it's a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    destinationRegister = registerName;
                    registers.TryGetValue(registerName, out number);
                    break;
                case 102: // if it's an address
                    destinationAddress = ReadLongFromMemory();
                    break;
                case 103:
                    number = ReadByteFromMemory();
                    break;
                case 104: // if it's an address stored in a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out destinationAddress);
                    break;
            }
            Console.WriteLine("WRITE:");
            // choose what to print
            switch (firstOperandType)
            {
                case 101:
                case 103:
                    Console.WriteLine(number);
                    break;
                case 102:
                case 104:
                    byte b;
                    string output = "";
                    while ((b = ReadByteFromMemoryAt(destinationAddress++)) != 0)
                    {
                        char c = Encoding.ASCII.GetString(new[] { b })[0];
                        output += c;
                    }
                    Console.WriteLine(output);
                    break;
            }
        }

        private void Read()
        {
            string destinationRegister = "";
            long destinationAddress = 0;
            string registerName;

            // first operand fetch
            firstOperandType = ReadByteFromMemory();
            switch (firstOperandType)
            {
                case 101: // if it's a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    destinationRegister = registerName;
                    break;
                case 102: // if it's an address
                    destinationAddress = ReadLongFromMemory();
                    break;
                case 104: // if it's an address stored in a register
                    registersDecode.TryGetValue(ReadByteFromMemory(), out registerName);
                    registers.TryGetValue(registerName, out destinationAddress);

                    break;
            }

            Console.WriteLine("READ:");
            string input = Console.ReadLine();

            switch (firstOperandType)
            {
                case 101:
                    long number;
                    long.TryParse(input, out number);
                    registers[destinationRegister] = number;
                    break;
                case 102:
                case 104:
                    foreach (char c in input)
                    {
                        memory[destinationAddress++] = ((byte)c);
                    }
                    memory[destinationAddress] = 0;
                    break;
            }
        }

        private void End()
        {
            endOfProgram = true;
        }

        private long ReadLongFromMemory() // to read long from program counter address
        {
            byte[] bytes = new byte[8];
            for(int i = 0; i < 8; i++)
            {
                memory.TryGetValue(programCounter++, out bytes[i]);
            }

            return BitConverter.ToInt64(bytes, 0); 
        }

        private void WriteLongToMemory(long value, long address)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes)
            {
                memory[address++] = b;
            }
        }

        private long ReadLongFromMemoryAt(long address) // to read long from any address
        {
            byte[] bytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                memory.TryGetValue(address++, out bytes[i]);
            }

            return BitConverter.ToInt64(bytes, 0);
        }

        private byte ReadByteFromMemory()
        {
            byte b;
            memory.TryGetValue(programCounter++, out b);

            return b;
        }

        private byte ReadByteFromMemoryAt(long address)
        {
            byte b;
            memory.TryGetValue(address, out b);

            return b;
        }

        private void DisplayDebugInfo(HashSet<long> addresses)
        {
            Console.WriteLine("Registers:");
            DisplayRegisters();
            Console.WriteLine("-----------------------");
            Console.WriteLine("Addresses:");
            DisplayAddresses(addresses);
            Console.WriteLine();
        }

        private void DisplayAddresses(HashSet<long> addresses)
        {
            byte memoryLocation;
            if (addresses.Count == 0) { Console.WriteLine("No addresses specified."); }
            foreach (var address in addresses)
            {
                if (memory.TryGetValue(address, out memoryLocation))
                {
                    Console.WriteLine("[" + address + "]: " + memoryLocation);
                }
                else
                {
                    Console.WriteLine("Address [" + address + "] not available.");
                }
            }
        }

        private void DisplayRegisters()
        {
            foreach (var pair in registers)
            {
                Console.WriteLine(pair.Key + ": " + pair.Value);
            }
        }

    }
}
