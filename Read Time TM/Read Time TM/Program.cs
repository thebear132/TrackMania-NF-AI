using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using GAF;

using WindowsInput;
using WindowsInput.Native;

namespace Read_Time_TM
{
    class Program
    {
        static string process = "TmForever";

        static int TmForeverBaseAdress; //Getting adress at runtime
        //get TmForeverBaseAdress()



        static void Main(string[] args)
        {
            InputSimulator inputSim = new InputSimulator();


            VAMemory vam = new VAMemory(process);

            if (GetModuleAddress(process))
            {
                Console.WriteLine("TmForever.exe baseadress = " + TmForeverBaseAdress);
                
                //Checkpoint MED goal counter
                int goalChecker = 0;
                int lastTime = -11;
                int goalTime = -11;
                while (true)
                {
                    //Ingame time ____________ "TmForever.exe" + __
                    int time = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096847C);
                    int time1 = vam.ReadInt32((IntPtr)time + 0x100);
                    int time2 = vam.ReadInt32((IntPtr)time1 + 0x5B4);
                    int time3 = vam.ReadInt32((IntPtr)time2 + 0x24);
                    int time4 = vam.ReadInt32((IntPtr)time3 + 0x30C);
                    int IngameTime = vam.ReadInt32((IntPtr)time4 + 0x4B0);

                    
                    if (lastTime > 2 && IngameTime == 0)
                    {
                        goalTime = lastTime;
                    }
                    lastTime = IngameTime;


                    int speed = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C);
                    int speed1 = vam.ReadInt32((IntPtr)speed + 0x0);
                    int speed2 = vam.ReadInt32((IntPtr)speed1 + 0x1C);
                    int IngameSpeed = vam.ReadInt32((IntPtr)speed2 + 0x340);



                    //Checkpoint goal
                    int cpg = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C);
                    int cpg1 = vam.ReadInt32((IntPtr)cpg + 0x0);
                    int cpg2 = vam.ReadInt32((IntPtr)cpg1 + 0x1C);
                    int CheckPointGoal = vam.ReadInt32((IntPtr)cpg2 + 0x334);

                    //Checkpoint
                    int cp = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C);
                    int cp1 = vam.ReadInt32((IntPtr)cp + 0x0);
                    int cp2 = vam.ReadInt32((IntPtr)cp1 + 0x1C);
                    int CheckPoint = vam.ReadInt32((IntPtr)cp2 + 0x330);

                    
                    if (CheckPointGoal == CheckPoint + 1)
                    {
                        goalChecker++;
                        if (goalChecker >= 3)
                        {
                            Console.WriteLine($"Goal reached at:{goalTime}");
                            break;
                        }
                    }



                    int turning = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096A2A4);
                    int turning1 = vam.ReadInt32((IntPtr)turning + 0x35C);
                    int turning2 = vam.ReadInt32((IntPtr)turning1 + 0x0);
                    int turning3 = vam.ReadInt32((IntPtr)turning2 + 0x0);
                    int TurningValue = vam.ReadInt32((IntPtr)turning3 + 0x5E8);

                    

                    Console.WriteLine($"Turning {TurningValue} | Speed={IngameSpeed} | CheckGoal={CheckPointGoal} | Check={CheckPoint} | Time={IngameTime}");

                    }
            }
            
            Console.WriteLine("Program has ended");
            Console.ReadKey();
        }




        static bool GetModuleAddress(string getProcess)
        {
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
                        //Console.WriteLine(m.ModuleName);
                        if (m.ModuleName == "TmForever.exe")
                        {
                            Console.WriteLine("Found client = " + m.ModuleName + " ");
                            TmForeverBaseAdress = (int)m.BaseAddress;
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
            return processFound;
        }
    }
}
