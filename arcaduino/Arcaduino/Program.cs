using Arcademenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace Arcaduino
{
    class Program : RunListener
    {
        const int ID_KEY_DOWN = 0;
        const int ID_KEY_UP = 1;
        const int ID_AXIS = 2;

        InputSimulator inSim = new InputSimulator();
        SerialPort arduPort = new SerialPort();
        Dictionary<string, KeyExecutor> keyMap = new Dictionary<string, KeyExecutor>();

        Program()
        {
            readFile("");
            setupPort();
            Menu.listener = this;
            new Thread(Start.Main).Start();
            loopSerial();
        }

        void loopSerial()
        {
            while (true)
            {
                int button = arduPort.ReadByte();
                int action = (button & 0b11000000) >> 6;
                int id = button & 0b00111111;

                if (action == ID_AXIS)
                {
                    readAxis(id);
                }
                else
                {
                    Console.WriteLine(action + " ,id: " + id);
                    KeyExecutor executor;
                    if (action == ID_KEY_DOWN)
                        if (keyMap.TryGetValue("" + id, out executor))
                            executor.keysDown(inSim);
                    if (action == ID_KEY_UP)
                        if (keyMap.TryGetValue("" + id, out executor))
                            executor.keysUp(inSim);
                }
            }
        }

        const int AXIS_COUNT = 2;
        const int AXIS_MAX = 225;

        int[] isAxisActive = new int[AXIS_COUNT];

        void readAxis(int data)
        {
            for (int i = 0; i < AXIS_COUNT; i++)
            {
                int axis_val = arduPort.ReadByte();
                Console.Write("A" + i + ": " + axis_val + ", ");
                KeyExecutor executor;

                if (isAxisActive[i] != 0)
                {
                    if (axis_val < 0.8 * AXIS_MAX && axis_val > 0.2 * AXIS_MAX)
                    {

                        if (keyMap.TryGetValue((isAxisActive[i] < 0 ? "-A" : "A") + i, out executor))
                            executor.keysUp(inSim);
                        isAxisActive[i] = 0;
                    }
                }
                else
                {
                    if (axis_val > 0.8 * AXIS_MAX)
                    {
                        if (keyMap.TryGetValue("A" + i, out executor))
                            executor.keysDown(inSim);
                        isAxisActive[i] = 1;
                    }
                    else if (axis_val < 0.2 * AXIS_MAX)
                    {
                        if (keyMap.TryGetValue("-A" + i, out executor))
                            executor.keysDown(inSim);
                        isAxisActive[i] = -1;
                    }
                }
            }
            Console.WriteLine();
        }

        void readFile(string file)
        {
            if (file == "" || !File.Exists(Menu.gamesPath + file + ".keymap"))
                keyMap = new FileCompiler().readFile(Menu.gamesPath + "default.keymap");
            else
                keyMap = new FileCompiler().readFile(Menu.gamesPath + file + ".keymap");
        }

        void setupPort()
        {
            arduPort.BaudRate = 9600;
            arduPort.PortName = "COM3";
            arduPort.Open();
        }

        static void Main(string[] args)
        {
            new Program();
        }

        public void AppRunning(string name)
        {
            readFile(name);
        }
    }
}
