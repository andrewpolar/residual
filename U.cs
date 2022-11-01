using System;
using System.Collections.Generic;
using System.Text;

namespace Residual
{
    class U
    {
        private List<IPLL> _plist = new List<IPLL>();
        private int _layers;
        private int[] _positions = null;

        public U(int[] group)
        {
            _positions = new int[group.Length];
            for (int i = 0; i < group.Length; ++i)
            {
                _positions[i] = group[i];
            }
        }

        public void Clear()
        {
            _plist.Clear();
        }

        public double GetDerrivative(int layer, double x)
        {
            return _plist[layer].GetDerrivative(x);
        }

        public void SetRandom(double ymin, double ymax)
        {
            for (int i = 0; i < _layers; ++i)
            {
                _plist[i].SetRandom(ymin / _layers, ymax / _layers);
            }
        }

        public void Initialize(int layers, int[] points, double[] xmin, double[] xmax)
        {
            _layers = layers;
            for (int i = 0; i < _layers; ++i)
            {
                IPLL pll = new PLL(points[i], xmin[i], xmax[i]);
                _plist.Add(pll);
            }
        }

        public void Update(double delta, double[] inputs, double mu)
        {
            delta /= _layers;
            for (int i = 0; i < _layers; ++i)
            {
                _plist[i].Update(inputs[_positions[i]], delta, mu);
            }
        }

        public double GetU(double[] inputs)
        {
            double f = 0.0;
            for (int i = 0; i < _layers; ++i)
            {
                f += _plist[i].GetFunctionValue(inputs[_positions[i]]);
            }
            return f;
        }

        public void ShowOperatorLimits()
        {
            Console.WriteLine("----- Top operator limits -----");
            for (int i = 0; i < _layers; ++i)
            {
                String S = String.Format("(min, max) = ({0:0.00}, {1:0.00})", _plist[i].GetXmin(), _plist[i].GetXmax());
                Console.WriteLine(S);

            }
            Console.WriteLine("-------------------------------");
        }
    }
}
