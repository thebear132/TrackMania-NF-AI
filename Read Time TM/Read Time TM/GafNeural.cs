using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAF;

namespace Read_Time_TM
{
    class GafNeural
    {
        const double crossoverProbability = 0.85;
        const double mutationProbability = 0.1;
        const int elitismPercentage = 5;
        const int timeSegment = 100;


        Population MyPopulation = new Population(10, (1000 / timeSegment) * 30, false, false);



        private bool TerminateFunction(Population population, int currentGeneration, long currentEvaluation)
        {

            return currentGeneration > 400;
        }






        private double CalculateFitness(Chromosome chromosome) // One trackmania run
        {
            //GET FROM TRACKMANIA CODE
            int ingameTime = 15;
            int maxTime = 30;

            int totalCheckpoints = 10;
            int checkpoints = 5;
            //GET FROM TRACKMANIA CODE

            
            return 0.5;
        }


    }
}
