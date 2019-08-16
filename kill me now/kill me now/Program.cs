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
        readonly static int maxTime = 10;
        readonly static string process = "TmForever";
        readonly static int TmForeverBaseAdress = GetModuleAddress(process, "TmForever.exe");
        static VAMemory vam = new VAMemory(process);
        public class BestOfGeneration
        {
            public string BestString;
            public int TimeTaken;
            public int CheckpointsComp;
        }
        private static void Main(string[] args)
        {
            //Sleep 2000 ms for at få tid til at klikke ind på Trackmania.
            Thread.Sleep(2000);

            //Angiv probabilities af crossover/mutation og procentdelen af population der kan være elite(eligible for crossover)
            const double crossoverProbability = 0.85;
            const double mutationProbability = 0.08;
            const int elitismPercentage = 15;

            //create a Population with 4 random chromosomes of length 64
            //First generation/population will have double the amount for better results
            var population = new Population(20, 96, false, false);

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

        public static double EvaluateFitness(Chromosome chromosome)
        {
            double fitnessValue = -1;
            if (chromosome != null)
            {
                //Make the chromosome into a binary string ( string of 0's and 1's )
                string binaryInstructions = chromosome.ToBinaryString(0, chromosome.Count);
                string driveInstruction;

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
                            Drive(driveInstruction, 500);
                        }
                        break;
                    }
                }

                //Læs hvor mange checkpoints der er blevet completed og hvor lang tid der er gået fra memory
                double completedCheckpoints = (double)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0095772C) + 0x0) + 0x1C) + 0x334);
                double completedTime = (double)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)vam.ReadInt32((IntPtr)TmForeverBaseAdress + 0x0096847C) + 0x100) + 0x5B4) + 0x24) + 0x30C) + 0x4B0);

                Console.WriteLine("\nCheckpoints-" + completedCheckpoints + "    |      Time-" + completedTime);
                Console.WriteLine("Fitness: " + (completedCheckpoints )/( 6) * (maxTime / maxTime));

                //Return en fitness value
                fitnessValue = (completedCheckpoints)/( 6) * (maxTime / maxTime);
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
            bestGeneration.Add(currentGen);
            int i = 1;
            foreach(BestOfGeneration instruction in bestGeneration)
            {
                Console.WriteLine("Gen" + i + ": " + instruction.BestString + "\nFitness: " + e.Population.MaximumFitness);
            }

            //Console.WriteLine(chromosome.ToBinaryString(0, chromosome.Count) + "\n FITNESS:" +  e.Population.MaximumFitness);
        }
        static public void Drive(string instruction, int timePeriod)
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
            Thread.Sleep(timePeriod);
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