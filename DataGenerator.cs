using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Residual
{
    class DataGenerator
    {
        Random _rnd = new Random();

        public double Formula(double[] x)
        {
            //y = (1/pi)*(2+2*x3)*(1/3)*(atan(20*exp(x5)*(x1-0.5+x2/6))+pi/2) + (1/pi)*(2+2*x4)*(1/3)*(atan(20*exp(x5)*(x1-0.5-x2/6))+pi/2);
            double pi = 3.14159265359;
            if (5 != x.Length)
            {
                Console.WriteLine("Formala error");
                Environment.Exit(0);
            }
            double y = (1.0 / pi);
            y *= (2.0 + 2.0 * x[2]);
            y *= (1.0 / 3.0);
            y *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 + x[1] / 6.0)) + pi / 2.0;

            double z = (1.0 / pi);
            z *= (2.0 + 2.0 * x[3]);
            z *= (1.0 / 3.0);
            z *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 - x[1] / 6.0)) + pi / 2.0;

            return y + z;
        }

        public double[] GetRandomInput()
        {
            double[] x = new double[5];
            x[0] = (_rnd.Next() % 100) / 100.0;
            x[1] = (_rnd.Next() % 100) / 100.0;
            x[2] = (_rnd.Next() % 100) / 100.0;
            x[3] = (_rnd.Next() % 100) / 100.0;
            x[4] = (_rnd.Next() % 100) / 100.0;
            return x;
        }

        public double[] AddNoise(double[] x, double magnitude, out double datanorm, out double errornorm)
        {
            double[] z = new double[5];
            z[0] = x[0] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[1] = x[1] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[2] = x[2] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[3] = x[3] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);
            z[4] = x[4] + magnitude * ((_rnd.Next() % 100) / 100.0 - 0.5);

            datanorm = 0.0;
            foreach (double d in x)
            {
                datanorm += d * d;
            }
            datanorm /= x.Length;
            datanorm = Math.Sqrt(datanorm);

            errornorm = 0.0;
            for (int i = 0; i < x.Length; ++i)
            {
                errornorm += (x[i] - z[i]) * (x[i] - z[i]);
            }
            errornorm /= x.Length;
            errornorm = Math.Sqrt(errornorm);

            return z;
        }
    }

    class DataHolder
    {
        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        public double[] _xmin = null;
        public double[] _xmax = null;
        public double _noise = 0.0;
        private DataGenerator dg = new DataGenerator();

        public double[] GetRandomInput()
        {
            return dg.GetRandomInput();
        }

        public double GetExactOutput(double[] input)
        {
            return dg.Formula(input);
        }

        public double[] GetStatData(double[] x, int N)
        {
            double[] y = new double[N];

            for (int i = 0; i < N; ++i)
            {
                double datanorm;
                double errornorm;
                y[i] = dg.Formula(dg.AddNoise(x, _noise, out datanorm, out errornorm));
            }

            return y;
        }

        public void SaveData(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                for (int i = 0; i < _inputs.Count; ++i)
                {
                    string line = "";
                    foreach (double d in _inputs[i])
                    {
                        line += String.Format("{0:0.0000}, ", d);
                    }
                    line += String.Format("{0:0.0000}", _target[i]);
                    sw.WriteLine(line);
                }
                sw.Flush();
                sw.Close();
            }
        }

        public void BuildFormulaData(double noise, int N)
        {
            _noise = noise;
            _inputs.Clear();
            _target.Clear();

            double alldatanorm = 0.0;
            double allerrornorm = 0.0;

            double mean = 0.0;
            for (int i = 0; i < N; ++i)
            {
                double datanorm;
                double errornorm;
                double[] x = GetRandomInput();
                double y = dg.Formula(dg.AddNoise(x, noise, out datanorm, out errornorm));

                alldatanorm += datanorm;
                allerrornorm += errornorm;

                _inputs.Add(x);
                _target.Add(y);
                mean += y;
            }
            mean /= (double)N;
            Console.WriteLine("Data is generated, average relative error {0:0.0000}, mean {1:0.00}", allerrornorm / alldatanorm, mean);
        }
    }
}



