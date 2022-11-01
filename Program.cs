
//This is demo of obtaining expectations and variances for stochastic regression model
//by non Bayesian neural network.  The result is similar to BNN approach published on link
//https://github.com/andrewpolar/Benchmark5/blob/main/Benchmark5.py
//The method is straight forward computing of residuals and build second model for them.
//Code written by Andrew Polar.

using System;
using System.Collections.Generic;

namespace Residual
{
    class Program
    {
        static void GetExpectationAndVariance(double[] y, out double expectation, out double variance, out double std)
        {
            expectation = 0.0;
            foreach (double d in y)
            {
                expectation += d;
            }
            expectation /= (double)(y.Length);

            variance = 0.0;
            foreach (double d in y)
            {
                variance += (d - expectation) * (d - expectation);
            }
            std = Math.Sqrt(variance / (double)(y.Length));
            variance /= (double)(y.Length - 1);
        }

        static void Main(string[] args)
        {
            DataHolder dh = new DataHolder();
            dh.BuildFormulaData(0.8, 10000);

            //This is expectation model
            KolmogorovModel km_expectaion = new KolmogorovModel(dh._inputs, dh._target, new int[] { 3, 3, 3, 3, 3 });
            int NLeaves = 12;
            int[] linearBlocksPerRootInput = new int[NLeaves];
            for (int m = 0; m < NLeaves; ++m)
            {
                linearBlocksPerRootInput[m] = 16;
            }
            km_expectaion.GenerateInitialOperators(NLeaves, linearBlocksPerRootInput);
            km_expectaion.BuildRepresentation(500, 0.01, 0.01);
            Console.WriteLine("Modelled to actual output correlation koeff for expectaion model {0:0.00}", km_expectaion.ComputeCorrelationCoeff());

            //Now we build residual model
            List<double> residuals = new List<double>();
            for (int i = 0; i < dh._inputs.Count; ++i)
            {
                double y = km_expectaion.ComputeOutput(dh._inputs[i]);
                residuals.Add((y - dh._target[i]) * (y - dh._target[i]));
            }

            KolmogorovModel km_residual = new KolmogorovModel(dh._inputs, residuals, new int[] { 3, 3, 3, 3, 3 });
            int NL = 12;
            int[] blocks = new int[NL];
            for (int m = 0; m < NL; ++m)
            {
                blocks[m] = 16;
            }
            km_residual.GenerateInitialOperators(NL, blocks);
            km_residual.BuildRepresentation(500, 0.02, 0.02);
            Console.WriteLine("Modelled to actual output correlation koeff for residuals {0:0.00}", km_residual.ComputeCorrelationCoeff());

            //Evaluation of accuracy
            int NTests = 256;
            List<double> modelE = new List<double>();
            List<double> monteE = new List<double>();
            List<double> monteV = new List<double>();
            List<double> list_residuals = new List<double>();

            for (int n = 0; n < NTests; ++n)
            {
                double[] randomInput = dh.GetRandomInput();
                double modelled_residual = km_residual.ComputeOutput(randomInput);
                double modelled_output = km_expectaion.ComputeOutput(randomInput);
                list_residuals.Add(modelled_residual);

                double[] MonteCarloOutput = dh.GetStatData(randomInput, 1024);
 
                double me;
                double ms;
                double mv;
                GetExpectationAndVariance(MonteCarloOutput, out me, out mv, out ms);
                monteE.Add(me);
                monteV.Add(mv);
                modelE.Add(modelled_output);
            }

            Console.WriteLine("Correlation for variance    and squared residuals {0:0.0000}", Static.PearsonCorrelation(list_residuals.ToArray(), monteV.ToArray()));
            Console.WriteLine("Correlation for expectation and monte carlo mean  {0:0.0000}", Static.PearsonCorrelation(modelE.ToArray(), monteE.ToArray()));
        }
    }
}
