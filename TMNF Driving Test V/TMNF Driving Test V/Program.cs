using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Text;
//using GAF;


namespace Read_Time_TM
{
    class Program
    {
        static string process = "TmForever";
        readonly static int TmForeverBaseAdress = GetModuleAddress(process, "TmForever.exe");
        readonly static int DINPUT8 = GetModuleAddress("TmForever", "DINPUT8.dll");
        static public void Drive(char instruction, int timePeriod)
        {
            VAMemory vam = new VAMemory(process);

            int turning = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096A2A4);
            int turning1 = vam.ReadInt32((IntPtr)turning + 0x35C);
            int turning2 = vam.ReadInt32((IntPtr)turning1 + 0x0);
            int turning3 = vam.ReadInt32((IntPtr)turning2 + 0x0);


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while (stopWatch.IsRunning == true && stopWatch.ElapsedMilliseconds < timePeriod)
            {
                switch (instruction)
                {
                    case '0':
                        vam.WriteInt32((IntPtr)DINPUT8 + 0x32348, 128);
                        vam.WriteInt32((IntPtr)turning3 + 0x5E8, 0); //NOTHING
                        break;

                    case '1':

                        vam.WriteInt32((IntPtr)DINPUT8 + 0x32348, 128);
                        vam.WriteInt32((IntPtr)turning3 + 0x5E8, -1082130432); //LEFT
                        break;

                    case '2':
                        vam.WriteInt32((IntPtr)DINPUT8 + 0x32348, 128);
                        vam.WriteInt32((IntPtr)turning3 + 0x5E8, 1065353216); //RIGHT
                        break;
                }
            }
            //vam.WriteInt32((IntPtr)DINPUT8 + 0x32348, 0);
            vam.WriteInt32((IntPtr)turning3 + 0x5E8, 0); //NOTHING
        }
        static void Main(string[] args)
        {

            Random rnd = new Random();
            VAMemory vam = new VAMemory(process);
            Thread.Sleep(2000);
            int time = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096847C);
            int time1 = vam.ReadInt32((IntPtr)time + 0x100);
            int time2 = vam.ReadInt32((IntPtr)time1 + 0x5B4);
            int time3 = vam.ReadInt32((IntPtr)time2 + 0x24);
            int time4 = vam.ReadInt32((IntPtr)time3 + 0x30C);

            string driveInstructions;
            StringBuilder sb = new StringBuilder("", 60);
            for (int i = 0; i < 60; i++)
            {
                sb.Append(rnd.Next(3).ToString());
            }
            driveInstructions = sb.ToString();

            //Comment this out : )
            driveInstructions = "000020001011022000000000000001110000000000000111";
            driveInstructions = "00002000101102200000000001110000000001000111";
            Console.WriteLine("Currently using custom string");
            //


            Console.WriteLine("CURRENT INSTRUCTION: " + driveInstructions + "\nRestart the round, then restart it again when timer is past 0.5 (1.0 to be safe)");
            while (true)
            {
                if (vam.ReadInt32((IntPtr)time4 + 0x4B0) == 0)
                {
                    while (true)
                    {
                        Thread.Sleep(500);
                        vam.WriteInt32((IntPtr)DINPUT8 + 0x32348, 128);
                        if (vam.ReadInt32((IntPtr)time4 + 0x4B0) == 0)
                        {
                            foreach (char c in driveInstructions)
                            {
                                Console.Write(c);
                                Drive(c, 500);
                            }
                        }
                    }
                }
            }

        }

        static int GetModuleAddress(string getProcess, string moduleName)
        {
            int baseAddress = 0;
            bool processFound = false;
            try
            {
                Process[] p = Process.GetProcessesByName(getProcess);

                if (p.Length != 0)
                {
                    Console.WriteLine("Process(" + process + ")found - Getting modules");
                    Console.WriteLine("Amount of modules = " + p[0].Modules.Count);
                    foreach (ProcessModule m in p[0].Modules)
                    {
                        if (m.ModuleName == moduleName)
                        {
                            Console.WriteLine("Found client = " + m.ModuleName + " ");
                            baseAddress = (int)m.BaseAddress;
                            processFound = true;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Exception found");
            }

            Console.WriteLine("Process found = " + processFound);
            return baseAddress;
        }
    }
}