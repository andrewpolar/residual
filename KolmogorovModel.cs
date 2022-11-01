using System;
using System.Collections.Generic;
using System.Text;

namespace Residual
{
    class KolmogorovModel
    {
        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        public double[] _xmin = null;
        public double[] _xmax = null;
        public double _targetMin;
        public double _targetMax;
        public int[] _LINEAR_BLOCKS_PER_INPUT = null;

        private List<U> _ulist = new List<U>();
        private U _bigU = null;
        private Random _rnd = new Random();

        public KolmogorovModel(List<double[]> inputs, List<double> target, int[] LINEAR_BLOCKS_PER_INPUT)
        {
            _inputs = inputs;
            _target = target;
            _LINEAR_BLOCKS_PER_INPUT = LINEAR_BLOCKS_PER_INPUT;
            FindMinMax();
        }

        private void FindMinMax()
        {
            int size = _inputs[0].Length;
            _xmin = new double[size];
            _xmax = new double[size];

            for (int i = 0; i < size; ++i)
            {
                _xmin[i] = double.MaxValue;
                _xmax[i] = double.MinValue;
            }

            for (int i = 0; i < _inputs.Count; ++i)
            {
                for (int j = 0; j < _inputs[i].Length; ++j)
                {
                    if (_inputs[i][j] < _xmin[j]) _xmin[j] = _inputs[i][j];
                    if (_inputs[i][j] > _xmax[j]) _xmax[j] = _inputs[i][j];
                }

            }

            _targetMin = double.MaxValue;
            _targetMax = double.MinValue;
            for (int j = 0; j < _target.Count; ++j)
            {
                if (_target[j] < _targetMin) _targetMin = _target[j];
                if (_target[j] > _targetMax) _targetMax = _target[j];
            }
        }

        public void GenerateInitialOperators(int Nleaves, int[] linearBlocksPerRootInput)
        {
            _ulist.Clear();
            int points = _inputs[0].Length;
            int[] inputIndexesLeaves = new int[points];
            for (int i = 0; i < points; ++i)
            {
                inputIndexesLeaves[i] = i;
            }

            for (int counter = 0; counter < Nleaves; ++counter)
            {
                U uc = new U(inputIndexesLeaves);
                uc.Initialize(points, _LINEAR_BLOCKS_PER_INPUT, _xmin, _xmax);
                uc.SetRandom(_targetMin, _targetMax);
                _ulist.Add(uc);
            }

            if (null != _bigU)
            {
                _bigU.Clear();
                _bigU = null;
            }

            int[] inputIndexesRoot = new int[Nleaves];
            for (int k = 0; k < Nleaves; ++k)
            {
                inputIndexesRoot[k] = k;
            }

            double[] min = new double[Nleaves];
            double[] max = new double[Nleaves];
            for (int i = 0; i < Nleaves; ++i)
            {
                min[i] = _targetMin;
                max[i] = _targetMax;
            }

            _bigU = new U(inputIndexesRoot);
            _bigU.Initialize(Nleaves, linearBlocksPerRootInput, min, max);
            _bigU.SetRandom(_targetMin, _targetMax);
        }

        private double[] GetVector(double[] data)
        {
            int size = _ulist.Count;
            double[] vector = new double[size];
            for (int i = 0; i < size; ++i)
            {
                vector[i] = _ulist[i].GetU(data);
            }
            return vector;
        }

        public void BuildRepresentation(int steps, double muRoot, double muLeaves)
        {
            for (int step = 0; step < steps; ++step)
            {
                double RMSE = 0.0;
                int cnt = 0;
                for (int i = 0; i < _inputs.Count; ++i)
                {
                    double[] data = _inputs[i];
                    double[] v = GetVector(data);
                    double diff = _target[i] - _bigU.GetU(v);

                    for (int k = 0; k < _ulist.Count; ++k)
                    {
                        if (v[k] > _targetMin && v[k] < _targetMax)
                        {
                            double derrivative = _bigU.GetDerrivative(k, v[k]);
                            _ulist[k].Update(diff / v.Length, data, muLeaves * derrivative);
                        }
                    }

                    _bigU.Update(diff, v, muRoot);

                    RMSE += diff * diff;
                    ++cnt;
                }

                RMSE /= cnt;
                RMSE = Math.Sqrt(RMSE);
            }

            //_bigU.ShowOperatorLimits();
        }

        public double DoTest()
        {
            double RMSE = 0.0;
            int cnt = 0;
            int N = _inputs.Count;
            for (int i = 0; i < N; ++i)
            {
                double[] data = _inputs[i];
                double[] v = GetVector(data);
                double prediction = _bigU.GetU(v);
                double diff = _target[i] - prediction;
                RMSE += diff * diff;
                ++cnt;
            }
            RMSE /= cnt;
            RMSE = Math.Sqrt(RMSE);
            //RMSE /= (_targetMax - _targetMin);

            return RMSE;
        }

        public double ComputeOutput(double[] inputs)
        {
            double[] v = GetVector(inputs);
            double output = _bigU.GetU(v);
            return output;
        }

        public void SortData()
        {
            List<double> error = new List<double>();
            for (int i = 0; i < _inputs.Count; ++i)
            {
                error.Add(_target[i] - _bigU.GetU(GetVector(_inputs[i])));
            }
            int[] indexes = new int[error.Count];
            for (int i = 0; i < indexes.Length; ++i)
            {
                indexes[i] = i;
            }
            Array.Sort(error.ToArray(), indexes);
            ResortData(indexes);
        }

        public void ResortData(int[] indexes)
        {
            int len = _inputs[0].Length;
            List<double[]> tmpInput = new List<double[]>();
            List<double> tmpTarget = new List<double>();
            foreach (int n in indexes)
            {
                double[] x = new double[len];
                for (int k = 0; k < len; ++k)
                {
                    x[k] = _inputs[n][k];
                }
                tmpInput.Add(x);
                tmpTarget.Add(_target[n]);
            }
            _inputs.Clear();
            _target.Clear();
            for (int i = 0; i < tmpInput.Count; ++i)
            {
                _inputs.Add(tmpInput[i]);
                _target.Add(tmpTarget[i]);
            }
        }

        public void ErrorTest()
        {
            List<double> error = new List<double>();
            for (int i = 0; i < _inputs.Count; ++i)
            {
                Console.WriteLine(_target[i] - _bigU.GetU(GetVector(_inputs[i])));
            }
        }

        public double ComputeCorrelationCoeff()
        {
            int N = _inputs.Count;
            double[] targetEstimate = new double[N];
            int count = 0;
            for (int i = 0; i < N; ++i)
            {
                double[] data = _inputs[i];
                double[] v = GetVector(data);
                targetEstimate[i] = _bigU.GetU(v);
                ++count;
            }
            double[] x = new double[count];
            double[] y = new double[count];
            count = 0;
            for (int i = 0; i < N; ++i)
            {
                x[count] = targetEstimate[i];
                y[count] = _target[i];
                ++count;
            }
            return Static.PearsonCorrelation(x, y);
        }
    }
}
