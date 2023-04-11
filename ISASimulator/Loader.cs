using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ISASimulator
{
    internal class Loader
    {
        private Dictionary<string, byte> dictionary;
        private Dictionary<string, long> labels;
        private Dictionary<string, long> registers;
        List<string[]> input;
        private long codeSegAddress;
        private bool hasCmp;
        private Dictionary<long, byte> memory = new();

        public Loader(Dictionary<string, byte> dictionary, Dictionary<string, long> labels, Dictionary<string, long> registers, List<string[]> input, long codeSegAddress, bool hasCmp)
        {
            this.dictionary = dictionary;
            this.labels = labels;
            this.registers = registers;
            this.input = input;
            this.codeSegAddress = codeSegAddress;
            this.hasCmp = hasCmp;
        }

        public Dictionary<long, byte> LoadInputToMemory()
        {
            long tempAddress = codeSegAddress;
            // first detect all labels
            foreach (string[] line in input)
            {
                if (!dictionary.ContainsKey(line[0]) && line[0].EndsWith(':'))
                {
                    labels.Add(line[0].Substring(0, line[0].Length - 1), tempAddress);
                }
                else if (dictionary.ContainsKey(line[0]))
                {
                    tempAddress++;
                    if (line[0].Equals("BREAK"))
                    {
                        tempAddress++;
                    }
                    else if (line.Length >= 2)
                    {
                        if (CheckIfArgReg(line[1]))
                        {
                            tempAddress += 2; // register is 2 bytes in memory; one to say it is a register; one to say which one it is
                        }
                        else if (CheckIfArgAddr(line[1]) || CheckIfArgRegAddr(line[1]) || CheckIfArgNum(line[1]))
                        {
                            tempAddress += 9; // address is 9 bytes in memory; one byte to say it is an address; eight to specify address itself
                        }
                        else // then it is a label
                        {
                            tempAddress += 8; // label arguments are 8 bytes in memory because after jump instructions we know that there will be an address
                        }

                        if (line[0].Equals("DIV")) // because for DIV even though we don't write the first operand there is still a first operand in memory
                        {
                            tempAddress += 2;
                        }

                        if (line.Length == 3)
                        {
                            if (CheckIfArgReg(line[2]) || CheckIfArgRegAddr(line[2]))
                            {
                                tempAddress += 2;
                            }
                            else if (CheckIfArgAddr(line[2]) || CheckIfArgNum(line[2]))
                            {
                                tempAddress += 9;
                            }
                        }
                        if (line.Length > 3) // only if we have BREAK instr. with more than 2 addresses to watch
                        {
                            for(int i = 3; i < line.Length; i++)
                            {
                                tempAddress += 8;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Syntax error.");
                }
            }
            foreach (string[] line in input)
            {
                byte byteCode;
                // loading instruction bytecode to memory (condition true unless the keyword is label which is not in dictionary
                if (dictionary.ContainsKey(line[0]))
                {
                    dictionary.TryGetValue(line[0], out byteCode); // retrieving bytecode for the instruction
                    memory.Add(codeSegAddress++, byteCode); // adding instruction bytecode to memory
                }
                else if (line[0].EndsWith(':')) // if it's a label
                {

                }
                else
                {
                    throw new Exception("Syntax error.");
                }
                // depending on the type of instruction (0/1/2 operands)...:
                switch (line[0])
                {
                    case "ADD": // they all take two operands
                    case "SUB":
                    case "MUL":
                    case "AND":
                    case "OR":
                    case "XOR":
                    case "MOV":
                    case "CMP":
                        if (line[0].Equals("CMP")) // setting that we have had a CMP instruction (required for conditional jumps)
                        {
                            hasCmp = true;
                        }
                        // loading first operand (register or address) bytecode to memory
                        if (CheckIfArgReg(line[1]))
                        {
                            LoadRegToMemory(line[1], 101); // 101 signalizes interpreter that the first operand is a register
                        }
                        else if (CheckIfArgAddr(line[1]))
                        {
                            LoadAddrToMemory(line[1], 102);// 102 signalizes interpreter that the second operand is an address
                        }
                        else if (CheckIfArgRegAddr(line[1]))
                        {
                            LoadRegToMemory(line[1].Substring(1, line[1].Length - 2), 104); // 104 signalizes the interpreter to use the address stored in such register
                        }
                        else
                        {
                            throw new Exception("Syntax error.");
                        }

                        // loading second operand (register or address or number) bytecode to memory
                        if (CheckIfArgReg(line[2]))
                        {
                            LoadRegToMemory(line[2], 201); // 201 signalizes the interpreter that the second operand is a register
                        }
                        else if (CheckIfArgAddr(line[2]))
                        {
                            LoadAddrToMemory(line[2], 202);// 202 signalizes the interpreter that the second operand is an address
                        }
                        else if (CheckIfArgNum(line[2]))
                        {
                            LoadNumToMemory(line[2], 203); // 203 signalizes the interpreter that the second operand is a number
                        }
                        else if (CheckIfArgRegAddr(line[2]))
                        {
                            LoadRegToMemory(line[2].Substring(1, line[2].Length - 1), 204); // 204 signalizes the interpreter to use the address stored in such register
                        }
                        else
                        {
                            throw new Exception("Syntax error.");
                        }
                        break;
                    case "DIV": // takes one operand, the divisor, dividend is in rax; quotient is stored in rax, remainder in rdx
                        LoadRegToMemory("rax", 101); // dummy operand, since we want to use the same function that we used for other operations
                        // loading operand into memory
                        if (CheckIfArgReg(line[1]))
                        {
                            LoadRegToMemory(line[1], 201); // 201 signalizes the interpreter that the second operand is a register
                        }
                        else if (CheckIfArgAddr(line[1]))
                        {
                            LoadAddrToMemory(line[1], 202);// 202 signalizes the interpreter that the second operand is an address
                        }
                        else if (CheckIfArgNum(line[1]))
                        {
                            LoadNumToMemory(line[1], 203); // 203 signalizes the interpreter that the second operand is a number
                        }
                        else if (CheckIfArgRegAddr(line[1]))
                        {
                            LoadRegToMemory(line[1].Substring(1, line[1].Length - 1), 204); // 104 signalizes the interpreter to use the address stored in such register
                        }
                        else
                        {
                            throw new Exception("Syntax error.");
                        }
                        break;
                    case "BREAK": // takes as many operands as you want
                        // to specify the number of addresses to be watched in debug mode
                        memory.Add(codeSegAddress++, (byte)(line.Length - 1));
                        for (int i = 1; i < line.Length; i++)
                        {
                            LoadAddrToMemory("[" + line[i] + "]");
                        }
                        break;
                    case "END": // takes no operands
                        break;
                    case "NOT": // takes one operand that can be either register or an address
                        // loading first operand (register or address) bytecode to memory
                        if (CheckIfArgReg(line[1]))
                        {
                            LoadRegToMemory(line[1], 101); // 101 signalizes interpreter that the first operand is a register
                        }
                        else if (CheckIfArgAddr(line[1]))
                        {
                            LoadAddrToMemory(line[1], 102);// 102 signalizes interpreter that the second operand is an address
                        }
                        else if (CheckIfArgRegAddr(line[1]))
                        {
                            LoadRegToMemory(line[1].Substring(1, line[1].Length - 1), 104); // 104 signalizes the interpreter to use the address stored in such register
                        }
                        else
                        {
                            throw new Exception("Syntax error.");
                        }
                        break;
                    case "JMP": // they all take one operand - the label
                    case "JE":
                    case "JNE":
                    case "JGE":
                    case "JL":  
                        if (!line[0].Equals("JMP") && !hasCmp)
                        {
                            throw new Exception("Syntax error.");
                        }
                        if (CheckIfArgLabel(line[1]))
                        {
                            LoadLabelAddrToMemory(line[1]);
                        }
                        else if (CheckIfArgAddr(line[1]))
                        {
                            LoadAddrToMemory(line[1]);
                        }
                        else if (CheckIfArgRegAddr(line[1]))
                        {
                            LoadAddrToMemory(line[1].Substring(1, line[1].Length - 1)); // 104 signalizes the interpreter to use the address stored in such register
                        }
                        else
                        {
                            throw new Exception("Syntax error.");
                        }
                        break;
                    case "WRITE": // takes one operand (register or address or number)
                        if (CheckIfArgReg(line[1]))
                        {
                            LoadRegToMemory(line[1], 101);
                        }
                        else if (CheckIfArgAddr(line[1]))
                        {
                            LoadAddrToMemory(line[1], 102);
                        }
                        else if (CheckIfArgNum(line[1]))
                        {
                            LoadNumToMemory(line[1], 103); // 103 signalizes the interpreter that the first argument is a number 
                        }
                        else if (CheckIfArgRegAddr(line[1]))
                        {
                            LoadRegToMemory(line[1].Substring(1, line[1].Length - 1), 104); // 104 signalizes the interpreter to use the address stored in such register
                        }
                        else
                        {
                            throw new Exception("Syntax error.");
                        }
                        break;
                    case "READ":
                        if (CheckIfArgReg(line[1]))
                        {
                            LoadRegToMemory(line[1], 101);
                        }
                        else if (CheckIfArgAddr(line[1]))
                        {
                            LoadAddrToMemory(line[1], 102);
                        }
                        else if (CheckIfArgRegAddr(line[1]))
                        {
                            LoadRegToMemory(line[1].Substring(1, line[1].Length - 1), 104); // 104 signalizes the interpreter to use the address stored in such register
                        }
                        else
                        {
                            throw new Exception("Syntax error.");
                        }
                        break;
                }
            }

            return memory;
        }
        private bool CheckIfArgReg(string arg)
        {
            if (dictionary.ContainsKey(arg))
            {
                return true;
            }
            return false;
        }

        private bool CheckIfArgNum(string arg)
        {
            long temp;
            return long.TryParse(arg, out temp);
        }

        private bool CheckIfArgAddr(string arg)
        {
            return Regex.IsMatch(arg, "^\\[[1-9][0-9]*\\]$");
        }

        private bool CheckIfArgRegAddr(string arg)
        {
            if (arg.Length >= 4)
            {
                arg = arg.Substring(1, arg.Length - 2);
                return registers.ContainsKey(arg);
            }
            return false;
        }

        private bool CheckIfArgLabel(string arg)
        {
            return labels.ContainsKey(arg);
        }

        private void LoadRegToMemory(string register, byte firstOrSecondOp)
        {
            byte byteCode;
            memory.Add(codeSegAddress++, firstOrSecondOp);
            dictionary.TryGetValue(register, out byteCode);
            memory.Add(codeSegAddress++, byteCode);
        }

        private void LoadNumToMemory(string number, byte firstOrSecondOp = 0)
        {
            if (firstOrSecondOp != 0)
            {
                memory.Add(codeSegAddress++, firstOrSecondOp);
            }
            long temp;
            long.TryParse(number, out temp);
            byte[] bytes = BitConverter.GetBytes(temp);
            foreach (byte b in bytes)
            {
                memory.Add(codeSegAddress++, b); // adding 8 bytes to memory
            }
        }

        private void LoadAddrToMemory(string address, byte firstOrSecondOp = 0)
        {
            if (CheckIfArgRegAddr(address))
            {
                string reg = address.Substring(1, address.Length - 2);
                long regValue;
                registers.TryGetValue(reg, out regValue);
                address = address.Replace(reg, regValue.ToString());
            }
            if (firstOrSecondOp != 0) // in some cases (jump instructions) we do not need this since an operand is always an address
            {
                memory.Add(codeSegAddress++, firstOrSecondOp);
            }
            long temp;
            long.TryParse(address.Substring(1, address.Length - 2), out temp);
            byte[] bytes = BitConverter.GetBytes(temp);
            foreach (byte b in bytes)
            {
                memory.Add(codeSegAddress++, b); // adding 8 bytes to memory
            }
        }

        private void LoadLabelAddrToMemory(string label)
        {
            long temp;
            labels.TryGetValue(label, out temp);
            byte[] bytes = BitConverter.GetBytes(temp);
            foreach (byte b in bytes)
            {
                memory.Add(codeSegAddress++, b); // adding 8 bytes to memory
            }
        }
    }
}
