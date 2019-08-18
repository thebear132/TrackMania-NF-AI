using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using GAF;
using GAF.Operators;
using System.Diagnostics;
using static TrackmaniaGAF.KeyboardClass;

namespace TrackmaniaGAF
{
    internal class Program
    {
        static List<BestOfGeneration> bestGeneration = new List<BestOfGeneration>();
        static int maxTime = 12;
        static int cheatEngineSpeed = 1;
        static int populationCount = 20;
        static int intervalTime = 500;

        readonly static string process = "TmForever";
        readonly static int TmForeverBaseAdress = GetModuleAddress(process, "TmForever.exe");
        static VAMemory vam = new VAMemory(process);
        
        private static void Main(string[] args)
        {
            Console.Title = "TMNF Ga AI";
            Console.WriteLine("__________________TMNF AI GA__________________");
            Console.Write($"MaxTime for each chromosome (enter for default: {maxTime}): ");
            string maxTimeInput = Console.ReadLine();
            if (!(maxTimeInput.ToString() == ""))
            {
                maxTime = Convert.ToInt32(maxTimeInput);
                Console.WriteLine("Maxtime set to " + maxTime);
            }

            Console.Write($"Cheat engine speed cheat (enter for no speed hack): ");
            string cheatEngineSpeedInput = Console.ReadLine();
            if (!(cheatEngineSpeedInput.ToString() == ""))
            {
                cheatEngineSpeed = Convert.ToInt32(cheatEngineSpeedInput);
            }

            Console.Write($"Population number |must be even| (enter for default {populationCount}): ");
            string populationInput = Console.ReadLine();
            if (!(populationInput.ToString() == ""))
            {
                populationCount = Convert.ToInt32(populationInput);
                int mod = populationCount % 2;
                populationCount += mod;
                Console.WriteLine("Population set to: " + populationCount);
            }

            Console.Write($"Interval - ms between actions (enter for default {intervalTime}): ");
            string intervalTimeInput = Console.ReadLine();
            if (!(intervalTimeInput.ToString() == ""))
            {
                intervalTime = Convert.ToInt32(intervalTimeInput);
            }

            Console.WriteLine("Focus trackmania window now");
            //Sleep 4000 ms for at få tid til at klikke ind på Trackmania.
            Thread.Sleep(4000);
            Console.WriteLine("Starting program");

            //Angiv probabilities af crossover/mutation og procentdelen af population der kan være elite(eligible for crossover)
            const double crossoverProbability = 0.85;
            const double mutationProbability = 0.08;
            const int elitismPercentage = 15;

            Console.WriteLine($"{maxTime}, {intervalTime}");
            int chroLengths = ((maxTime * 1000) * 4) / intervalTime;

            Console.WriteLine("Calculated chromosome length: " + chroLengths);
            int test = chroLengths % 4;
            chroLengths += test;
            Console.WriteLine("Adapted Length: " + chroLengths);

            //create a Population with ? random chromosomes of length ?
            //First generation/population will have double the amount automatically for better results
            var population = new Population(populationCount, chroLengths, false, false);

            //create the genetic operators 
            var elite = new Elite(elitismPercentage);

            var crossover = new Crossover(crossoverProbability, true)
            {
                CrossoverType = CrossoverType.SinglePoint
            };

            var mutation = new BinaryMutate(mutationProbability, true);

            //create the GA itself 
            var ga = new GeneticAlgorithm(population, EvaluateFitness);

            //subscribe to the GAs Generation Complete event 
            ga.OnGenerationComplete += ga_OnGenerationComplete;

            //add the operators to the ga process pipeline
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutation);

            //run the GA 
            ga.Run(TerminateAlgorithm);
        }
        public class BestOfGeneration
        {
            public string BestString;
            public double Fitness;
            public int TimeTaken;
            public int CheckpointsComp;
        }
        public static double EvaluateFitness(Chromosome chromosome)
        {
            double fitnessValue = -1;
            if (chromosome != null)
            {
                //Make the chromosome into a binary string ( string of 0's and 1's )
                string binaryInstructions = chromosome.ToBinaryString(0, chromosome.Count);
                string driveInstruction;
                double[] timePerCheckPoint = new double[6];
                //Klik på delete, (Restart banen)
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_DELETE, false, Keyboard.InputType.Keyboard);
                Thread.Sleep(10);
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_DELETE, true, Keyboard.InputType.Keyboard);
                //Begynd at sende driving instructions (binary string) i segmenter af 4 characters når banen er blevet restarted ( tid=0 )
                while (true)
                {
                    if (vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096847C) + 0x100) + 0x5B4) + 0x24) + 0x30C) + 0x4B0) == 0)
                    {
                        for (int i = 0; i < binaryInstructions.Length; i += 4)
                        {
                            driveInstruction = binaryInstructions.Substring(i, 4);
                            Console.Write(driveInstruction);
                            timePerCheckPoint = Drive(driveInstruction, intervalTime / cheatEngineSpeed, timePerCheckPoint);
                        }
                        break;
                    }
                }
                Console.Write("\nTIME PER CHECKPOINT: ");
                foreach (double i in timePerCheckPoint)
                    Console.Write(i + "-");

                //Læs hvor mange checkpoints der er blevet completed og hvor lang tid der er gået fra memory
                double completedCheckpoints = (double)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C) + 0x0) + 0x1C) + 0x334);
                double completedTime = (double)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096847C) + 0x100) + 0x5B4) + 0x24) + 0x30C) + 0x4B0);

                Console.WriteLine("\nCheckpoints-" + completedCheckpoints + "    |      Time-" + completedTime);
                //Console.WriteLine("Fitness: " + (completedCheckpoints )/( 6) * (maxTime / maxTime));

                double[] fitnessOperations = new double[6];
                int index = 0;
                foreach (double i in timePerCheckPoint)
                {
                    if (i != 0)
                    {
                        double midCalc = (double)(-1) / (double)30;
                        fitnessOperations[index] = (double)midCalc * i + (double)2;
                    }
                    index++;
                }
                double result = 1;
                foreach (double i in fitnessOperations)
                {
                    if (i != 0)
                        result = (double)result * (double)i;
                }
                //foreach (double i in fitnessOperations)
                //   Console.Write(i + "=");

                //double testa = (double)(-1) / 30;
                //testa = (double)testa * 5 + 2;
                //testa = (double)testa * 3 / 32 / 6;

                //Return en fitness value
                //fitnessValue = (completedCheckpoints)/( 6) * (maxTime / maxTime);
                //double midcalc = (double)(3 / 32);
                fitnessValue = (double)((((double)3 / (double)32) * (double)result) / (double)6);
                Console.WriteLine("\nFitness: " + fitnessValue);

            }
            else
            {
                //chromosome is null
                throw new ArgumentNullException("chromosome", "The specified Chromosome is null.");
            }

            return fitnessValue;
        }

        public static bool TerminateAlgorithm(Population population, int currentGeneration, long currentEvaluation)
        {
            //Stop on 100th generation
            return currentGeneration > 100;
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {

            //get the best solution
            var chromosome = e.Population.GetTop(1)[0];
            BestOfGeneration currentGen = new BestOfGeneration();
            currentGen.BestString = chromosome.ToBinaryString(0, chromosome.Count);
            currentGen.Fitness = e.Population.MaximumFitness;
            bestGeneration.Add(currentGen);
            int i = 1;
            Console.WriteLine("\n\n\n ----------- GENERATION " + e.Generation + " COMPLETE ------------ ");

            string defaultPath = @".\Trackmania Generation Datalog.txt";
            foreach (BestOfGeneration instruction in bestGeneration)
            {
                Console.WriteLine("Gen" + (i - 1) + ": " + instruction.BestString + "\nFitness: " + instruction.Fitness);
                i++;
                //WRITE TO FILE
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(defaultPath, true))
            {
                file.WriteLine("Gen" + (i - 1) + ": " + currentGen.BestString + "\nFitness: " + currentGen.Fitness);
            }
            Console.WriteLine('\n');
            Console.WriteLine("----------- GENERATION LOG COMPLETE ------------\n\n\n");    
        }
        static public double[] Drive(string instruction, int timePeriod, double[] checkPointArray)
        {
            if (instruction[0] == '1')
            {
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_UPARROW, false, Keyboard.InputType.Keyboard);
            }
            if (instruction[1] == '1')
            {
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_DOWNARROW, false, Keyboard.InputType.Keyboard);
            }
            if (instruction[2] == '1')
            {
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_LEFTARROW, false, Keyboard.InputType.Keyboard);
            }
            if (instruction[3] == '1')
            {
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_RIGHTARROW, false, Keyboard.InputType.Keyboard);
            }
            //Thread.Sleep(timePeriod);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int firstCheck;
            int secondCheck;
            while(stopWatch.IsRunning && stopWatch.ElapsedMilliseconds < timePeriod)
            {
                firstCheck = vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C) + 0x0) + 0x1C) + 0x334);
                Thread.Sleep(10);
                secondCheck = vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C) + 0x0) + 0x1C) + 0x334);
                if ( firstCheck != secondCheck)
                {
                    for (int i = 0; i < checkPointArray.Length; i ++)
                    {
                        if(checkPointArray[i] == 0)
                        {
                            if (i!= 0)
                            {
                                checkPointArray[i] = (double)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096847C) + 0x100) + 0x5B4) + 0x24) + 0x30C) + 0x4B0) - checkPointArray[i - 1];
                                break;
                            }
                            else
                            {
                                checkPointArray[i] = (double)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096847C) + 0x100) + 0x5B4) + 0x24) + 0x30C) + 0x4B0);
                            }



                        }

                    }

                }
            }


            if (instruction[0] == '1')
            {
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_UPARROW, true, Keyboard.InputType.Keyboard);
            }
            if (instruction[1] == '1')
            {
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_DOWNARROW, true, Keyboard.InputType.Keyboard);
            }
            if (instruction[2] == '1')
            {
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_LEFTARROW, true, Keyboard.InputType.Keyboard);
            }
            if (instruction[3] == '1')
            {
                Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_RIGHTARROW, true, Keyboard.InputType.Keyboard);
            }
            return checkPointArray;
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