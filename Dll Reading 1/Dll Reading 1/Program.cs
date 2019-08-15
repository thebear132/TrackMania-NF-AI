using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dll_Reading_1
{
    class Program
    {
        static string process = "TmForever";
        static int tmBase;

        static void Main(string[] args)
        {
            Console.WriteLine("Program start");
            VAMemory vam = new VAMemory(process);



            if (GetModuleAddress(process))
            {
                Console.WriteLine("Confirmed");
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
                    Console.WriteLine("Process found. Getting modules");
                    Console.WriteLine("Amount of modules = " + p.Length);
                    Console.Write("MODULE NAMES: \n");
                    int cycle = 1;
                    foreach (ProcessModule m in p[0].Modules)
                    {
                        if (cycle % 3 == 0)
                        {
                            Console.WriteLine(m.ModuleName + ", ");
                        } else
                        {
                            Console.Write(m.ModuleName + ", ");
                        }
                        cycle++;

                        if (m.ModuleName == "TmForever.exe")
                        {
                            Console.WriteLine("Found client = " + m.ModuleName + " ");
                            tmBase = (int)m.BaseAddress;
                            processFound = true;
                        }
                    }
                    Console.WriteLine("\n\n");
                }
            }
            catch
            {
                Console.WriteLine("Exception found");
            }

            Console.WriteLine("ProcessFound Status = " + processFound);
            return processFound;
        }
    }
}
