using Arcademenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
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
        private class PC_Config
        {
            public PC_Config(string _portName, string _basePath)
            {
                portName = _portName;
                basePath = _basePath;
            }

            public readonly string portName;
            public readonly string basePath;
        }
        private enum ConfigName
        {
            Daheim, Arcade, HeggaLappes
        }

        private PC_Config[] configs = new PC_Config[] {
            new PC_Config("COM4",@"C:\dev\cs\Arcaduino"),
            new PC_Config("COM3",@"C:\Users\Arcade\Desktop\Arcaduino"),
            new PC_Config("COM5",@"C:\Users\Anwender\Desktop\Arcaduino")
        };

        private const ConfigName configName = ConfigName.HeggaLappes;
        private readonly PC_Config config;
        private const bool PRINT_AXIS = true;
        private const bool PRINT_BUTTONS = false;

        const int ID_KEY_DOWN = 0;
        const int ID_KEY_UP = 1;
        const int ID_AXIS = 2;

        private bool isConnected = false;

        InputSimulator inSim = new InputSimulator();
        SerialPort arduPort = new SerialPort();
        Dictionary<string, KeyExecutor> keyMap = new Dictionary<string, KeyExecutor>();

        Program()
        {
            config = configs[(int)configName];
            Menu.listener = this;
            Menu.basePath = config.basePath;
            Menu.gamesPath = config.basePath + @"\Games\";

            readFile("");
            setupPort();
            new Thread(Start.Main).Start();
            loopSerial();
        }

        void loopSerial()
        {
            while (isConnected)
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
                    if (PRINT_BUTTONS)
                        Console.WriteLine((action == 0 ? "down" : "up  ") + " ,id: " + id);
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

        //const int AXIS_MAX = 225;
        const int AXIS_COUNT = 4;
        //int[] isAxisActive = new int[AXIS_COUNT];

        void readAxis(int data)
        {
            int axisCount = (data & 0b11) + 1;
            int axisData = (data & 0b111100) >> 2;

            if (axisCount > AXIS_COUNT)
                throw new Exception("More than " + AXIS_COUNT + " are not supported.");

            for (int i = 0; i < axisCount; i++)
            {
                //if (((data >> (i + 2)) & 1) == 0)
                //continue;
                int axis_val = arduPort.ReadByte();
                if (PRINT_AXIS)
                    Console.Write("A" + i + ": " + axis_val + ", ");

                int axis_positive = axis_val > 127 ? 1 : -1;
                axis_val = Math.Max(0, Math.Abs(axis_val - 127) - 5) / 4;

                if (i == 0)
                {
                    Move(-axis_positive * axis_val, 0);
                }
                else if (i == 1)
                {
                    Move(0, axis_positive * axis_val);
                }

                /* Old code
                KeyExecutor executor;
                if (isAxisActive[i] != 0)
                {
                    if (axis_val < 0.75 * AXIS_MAX && axis_val > 0.25 * AXIS_MAX)
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
                */
            }
            if (PRINT_AXIS)
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
            arduPort.PortName = config.portName;
            try
            {
                arduPort.Open();
                isConnected = true;
            }
            catch (Exception)
            {
                Console.Error.Write("FEHLER BEIM VERBINDEN MIT DEM ARDUINO: KEIN GERÄT GEFUNDEN AUF PORT " + config.portName);
            }
        }

        static void Main(string[] args)
        {
            new Program();
        }

        public void AppRunning(string name)
        {
            readFile(name);
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_MOVE = 0x0001;

        public static void Move(int xDelta, int yDelta)
        {
            mouse_event(MOUSEEVENTF_MOVE, xDelta, yDelta, 0, 0);
        }
    }
}
