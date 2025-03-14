﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TMNF1
{
    class Program
    {
        static string process = "TmForever"; //TMNF Titel i process
        Random rnd = new Random(System.DateTime.Today.Millisecond);
        static VAMemory vam = new VAMemory(process);

        static int TmForeverBaseAdress;
        static int D8INPUTBaseAdress;


        static void Main(string[] args)
        {
            TmForeverBaseAdress = GetModuleAddress(process, "TmForever.exe");
            D8INPUTBaseAdress = GetModuleAddress(process, "DINPUT8.dll");


            //Checkpoint MED goal counter
            int goalChecker = 0;
            int lastTime = -11;
            int goalTime = -11;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Stopwatch stopTest = new Stopwatch();
            stopTest.Start();
            int decision = 0;

            int instruction = 0;
            int timePeriod = 500;






            //Ingame time ____________ "TmForever.exe" + __
            int time = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096847C);
            int time1 = vam.ReadInt32((IntPtr)time + 0x100);
            int time2 = vam.ReadInt32((IntPtr)time1 + 0x5B4);
            int time3 = vam.ReadInt32((IntPtr)time2 + 0x24);
            int time4 = vam.ReadInt32((IntPtr)time3 + 0x30C);
            int IngameTime = vam.ReadInt32((IntPtr)time4 + 0x4B0);
            int timeHex = time4 + 0x4B0;


            //Ingame speed
            int speed = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C);
            int speed1 = vam.ReadInt32((IntPtr)speed + 0x0);
            int speed2 = vam.ReadInt32((IntPtr)speed1 + 0x1C);
            int IngameSpeed = vam.ReadInt32((IntPtr)speed2 + 0x340);
            int ingameHex = speed2 + 0x340;



            //Checkpoint goal
            int cpg = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C);
            int cpg1 = vam.ReadInt32((IntPtr)cpg + 0x0);
            int cpg2 = vam.ReadInt32((IntPtr)cpg1 + 0x1C);
            int CheckPointGoal = vam.ReadInt32((IntPtr)cpg2 + 0x334);
            int cpgHex = cpg2 + 0x334;


            //Checkpoint
            int cp = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C);
            int cp1 = vam.ReadInt32((IntPtr)cp + 0x0);
            int cp2 = vam.ReadInt32((IntPtr)cp1 + 0x1C);
            int CheckPoint = vam.ReadInt32((IntPtr)cp2 + 0x330);
            int cgHex = cp2 + 0x330;




            int turning = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096A2A4);
            int turning1 = vam.ReadInt32((IntPtr)turning + 0x35C);
            int turning2 = vam.ReadInt32((IntPtr)turning1 + 0x0);
            int turning3 = vam.ReadInt32((IntPtr)turning2 + 0x0);
            int TurningValue = vam.ReadInt32((IntPtr)turning3 + 0x5E8);
            int turningHex = turning3 + 0x5E8;



            int forward = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x004CE65C);
            int forward1 = vam.ReadInt32((IntPtr)forward + 0x14);
            int forward2 = vam.ReadInt32((IntPtr)forward1 + 0x84);
            int forward3 = vam.ReadInt32((IntPtr)forward2 + 0x20);
            int forward4 = vam.ReadInt32((IntPtr)forward3 + 0x24);
            int Forward = vam.ReadInt32((IntPtr)forward4 + 0x1E4);
            int forwardHex = forward4 + 0x1E4;

            while (true)
            {
                //IngameTime = vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096847C + 0x100 + 0x5B4 + 0x24 + 0x30C + 0x4B0);
                
                if (lastTime > 0 && IngameTime == 0)
                {
                    goalTime = lastTime;
                }
                lastTime = IngameTime;


                if (CheckPointGoal == CheckPoint + 1)
                {
                    goalChecker++;
                    if (goalChecker >= 3)
                    {
                        Console.WriteLine($"Goal reached at: {goalTime} seconds");
                        break;
                    }
                }


                Console.WriteLine($"Speed={IngameSpeed} | CheckGoal={CheckPointGoal} | Check={CheckPoint} | Time={IngameTime} | Forward={Forward} | Turning {TurningValue} | ");


                //vam.WriteInt32((IntPtr)forward4 + 0x1E4, 128);

                //int forwardTest = vam.ReadInt32((IntPtr)D8INPUTBaseAdress + 0x32348);

                //int forwardTest1 = vam.ReadInt32((IntPtr)forwardTest + 3);


                if (stopTest.ElapsedMilliseconds > 3000)
                {
                    stopTest.Restart();
                    if (decision == 1065353216)
                    {
                        decision = -1082130432;
                    } else
                    {
                        decision = 1065353216;
                    }

                }
                //vam.WriteInt32((IntPtr)turning3 + 0x5E8, decision);



                if (stopWatch.IsRunning == true && stopWatch.ElapsedMilliseconds >= timePeriod)
                {
                    if (!(instruction >= 8))
                    {
                        drive(instruction);
                        instruction++;
                    } else
                    {
                        instruction = 0;
                    }

                    stopWatch.Restart();

                }
            }

            Console.WriteLine("Program has ended");
            Console.ReadKey();
        }




        static public void drive(int instruction)
        {
            bool forward = false;
            bool backwards = false;
            bool left = false;
            bool right = false;

            switch (instruction)
            {
                case 0:
                    break;

                case 1:
                    Console.WriteLine("---DRIVING FORWARD--");
                    forward = true;
                    break;

                case 2:
                    Console.WriteLine("---DRIVING LEFT--");
                    left = true;
                    break;

                case 3:
                    Console.WriteLine("---DRIVING RIGHT--");
                    right = true;
                    break;

                case 4:
                    Console.WriteLine("---DRIVING BACKWARDS--");
                    backwards = true;
                    break;

                case 5:
                    Console.WriteLine("---DRIVING FORWARD AND LEFT--");
                    forward = true;
                    left = true;
                    break;

                case 6:
                    Console.WriteLine("---DRIVING FORWARD AND RIGHT--");
                    forward = true;
                    right = true;
                    break;

                case 7:
                    backwards = true;
                    left = true;
                    break;

                case 8:
                    backwards = true;
                    right = true;
                    break;
            }

            if (forward)
            {
                vam.WriteInt32((IntPtr)D8INPUTBaseAdress + 0x32348, 128);
            }
            if (left)
            {
                vam.WriteInt32((IntPtr)D8INPUTBaseAdress + 32348 + 3, 128);
            }
            if (right)
            {
                vam.WriteInt32((IntPtr)D8INPUTBaseAdress + 32348 + 5, 128);
            }
            if (backwards)
            {
                vam.WriteInt32((IntPtr)D8INPUTBaseAdress + 0x32350, 128);
            }

            vam.WriteInt32((IntPtr)D8INPUTBaseAdress + 0x32348, 0);
            vam.WriteInt32((IntPtr)D8INPUTBaseAdress + 32348 + 3, 0);
            vam.WriteInt32((IntPtr)D8INPUTBaseAdress + 32348 + 5, 0);
            vam.WriteInt32((IntPtr)D8INPUTBaseAdress + 0x32350, 0);
        }




        static int GetModuleAddress(string process, string module)
        {
            int BaseAdress = -1;
            try
            {
                Process[] p = Process.GetProcessesByName(process);

                if (p.Length != 0)
                {
                    Console.WriteLine("Process <" + process + "> found - Searching <" + p[0].Modules.Count + "> modules");
                    foreach (ProcessModule m in p[0].Modules)
                    {
                        //Console.WriteLine(m.ModuleName);
                        if (m.ModuleName == module)
                        {
                            Console.WriteLine("Found requested module: " + m.ModuleName);
                            BaseAdress = (int)m.BaseAddress;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Exception found");
            }

            Console.WriteLine($"Baseadress of <{module}> = " + BaseAdress + "\nGetModuleAdress END");
            return BaseAdress;
        }
    }
}
