using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace Arcaduino
{
    class Program
    {
        const int KEY_DOWN = 0;
        const int KEY_UP = 1;
        const int AXIS = 2;
        const int AXIS_COUNT = 4;
        const int AXIS_ACTIVE = 16000;
        const int AXIS_DEADZONE = 1000;

        InputSimulator inSim = new InputSimulator();
        SerialPort arduPort = new SerialPort();

        Program()
        {
            readFile();
            setupPort();
            loopSerial();
        }

        void loopSerial()
        {
            while (true)
            {
                int button = arduPort.ReadByte();
                int action = (button & 0b11000000) >> 6;
                int id = button & 0b00111111;
                Console.WriteLine(action + " ,id: " + id);

                if (action == AXIS)
                {
                    readAxis(id);
                }
                else
                {
                    //if (keyMap.TryGetValue(id, out keyComb))
                    //{
                    //    if (!keyComb.isCombination)
                    //    {
                    //        if (action == KEY_DOWN)
                    //        {
                    //            foreach (VirtualKeyCode vk in keyComb.keys)
                    //                inSim.Keyboard.KeyDown(vk);
                    //        }
                    //        if (action == KEY_UP)
                    //        {
                    //            foreach (VirtualKeyCode vk in keyComb.keys)
                    //                inSim.Keyboard.KeyUp(vk);
                    //        }
                    //    }
                    //    else if (action == KEY_DOWN)
                    //    {
                    //        inSim.Keyboard.ModifiedKeyStroke(keyComb.combination, keyComb.keys);
                    //    }
                    //}
                }
            }
        }

        void readAxis(int data)
        {
            for (int i = 0; i < AXIS_COUNT; i++)
            {
                int axis_val = arduPort.ReadByte();
                Console.Write("A" + i + ": " + axis_val);
            }
        }

        void readFile()
        {
            new FileCompiler().readFile(@".\mapping.txt");
            //string keys = @"(?:\w+\s*\+\s*)*\w+";
            //string combination = keys + @"\s+,\s+" + keys;
            //string axis = combination + @"\s+\|\s+" + combination;
            //string pattern = @"^(A?\d+)\s+-\s+(.*)$";
            //string keyTerm = "^(?:(" + keys + ")|(" + combination + ")|(" + axis + "))$";
            //string[] lines = System.IO.File.ReadAllLines(@".\mapping.txt");
            //foreach (string line in lines)
            //{
            //    Console.WriteLine("Line: " + line);
            //    Match match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
            //    if (match.Success)
            //    {
            //        int id;
            //        if (match.Groups[1].Value.StartsWith("A"))
            //        {
            //            id = 1000 + Convert.ToInt32(match.Groups[1].Value.Substring(1));
            //        }
            //        else
            //        {
            //            id = Convert.ToInt32(match.Groups[1].Value);
            //        }
            //        Match matchTerm = Regex.Match(match.Groups[2].Value, keyTerm, RegexOptions.IgnoreCase);
            //        if (match.Success)
            //        {
            //            foreach (Group group in matchTerm.Groups)
            //            {
            //                Console.WriteLine(group.Value);
            //            }
            //        }

                    //List<VirtualKeyCode> keyList = new List<VirtualKeyCode>();
                    //List<VirtualKeyCode> combinationList = null;
                    //bool isCombination = false;
                    //int combinationStart = 0;

                    //for (int i = 2; i < match.Groups.Count; i++)
                    //{
                    //    if (match.Groups[i].Value == ",")
                    //    {
                    //        isCombination = true;
                    //        combinationStart = i;
                    //        break;
                    //    }
                    //    if (match.Groups[i].Value != "")
                    //        keyList.Add((VirtualKeyCode)System.Enum.Parse(typeof(VirtualKeyCode), match.Groups[i].Value, true));
                    //}

                    //if (isCombination)
                    //{
                    //    combinationList = new List<VirtualKeyCode>(keyList);
                    //    keyList.Clear();
                    //    for (int i = combinationStart + 1; i < match.Groups.Count; i++)
                    //    {
                    //        if (match.Groups[i].Value != "")
                    //            keyList.Add((VirtualKeyCode)System.Enum.Parse(typeof(VirtualKeyCode), match.Groups[i].Value, true));
                    //    }
                    //}

                    //keyMap.Add(id, new KeyCombination(keyList, combinationList, isCombination));
                    //Console.Write("New Key: " + id + " - ");
                    //foreach (VirtualKeyCode vk in keyList)
                    //    Console.Write(vk.ToString() + " ");
                    //Console.WriteLine();
            //    }
            //}
        }



        void setupPort()
        {
            arduPort.BaudRate = 9600;
            arduPort.PortName = "COM7";
            arduPort.Open();
        }

        static void Main(string[] args)
        {
            new Program();
        }
    }
}
