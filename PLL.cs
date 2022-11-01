using System;
using System.Collections.Generic;
using System.Text;

namespace Residual
{
    class PLL : IPLL
    {
        private int _points;
        private double[] _y;
        private double _deltax;
        private double _xmin = double.MaxValue;
        private double _xmax = double.MinValue;
        Random _rnd = new Random();

        public PLL(int points, double xmin, double xmax)
        {
            Initialize(points, xmin, xmax);
        }

        public void SetRandom(double ymin, double ymax)
        {
            for (int i = 0; i < _points; ++i)
            {
                _y[i] = _rnd.Next() % 1000;
            }
            for (int cnt = 0; cnt < 2; ++cnt)
            {
                for (int i = 0; i < _points - 1; ++i)
                {
                    _y[i] = (_y[i] + _y[i + 1]) / 2.0;
                }
                for (int i = _points - 2; i > 0; --i)
                {
                    _y[i] = (_y[i] + _y[i - 1]) / 2.0;
                }
            }

            double min = _y[0];
            double max = min;
            for (int i = 0; i < _points; ++i)
            {
                if (_y[i] > max) max = _y[i];
                if (_y[i] < min) min = _y[i];
            }

            for (int i = 0; i < _points; ++i)
            {
                _y[i] = (_y[i] - min) / (max - min);
                _y[i] = _y[i] * (ymax - ymin) + ymin;
            }
        }

        public double GetDerrivative(double x)
        {
            int low = (int)((x - _xmin) / _deltax);
            if (low < 0) low = 0;
            if (low > _y.Length - 2) low = _y.Length - 2;
            return (_y[low + 1] - _y[low]) / _deltax;
        }

        private void Initialize(int points, double xmin, double xmax)
        {
            if (points < 2)
            {
                Console.WriteLine("Number of blocks is too low");
                Environment.Exit(0);
            }
            if (xmin >= xmax)
            {
                //Console.WriteLine("The limits are invalid {0:0.000}, {1:0.0000}", xmin, xmax);
                //Environment.Exit(0);
                xmax = xmin + 0.5;
                xmin -= 0.5;
            }

            _xmin = xmin;
            _xmax = xmax;
            _points = points;

            _y = new double[_points];
            _deltax = (_xmax - _xmin) / (_points - 1);
            _y[0] = 0.0;
            for (int i = 0; i < _y.Length; ++i)
            {
                _y[i] = 0.0;
            }
        }

        public void Update(double x, double delta, double mu)
        {
            if (x < _xmin)
            {
                _deltax = (_xmax - x) / (_points - 1);
                _xmin = x;
            }

            if (x > _xmax)
            {
                _deltax = (x - _xmin) / (_points - 1);
                _xmax = _xmin + (_points - 1) * _deltax;
            }

            int left = (int)((x - _xmin) / _deltax);
            if (left < 0) left = 0;
            if (left >= _y.Length - 1)
            {
                _y[_y.Length - 1] += delta * mu;
                return;
            }

            double leftx = x - (_xmin + left * _deltax);
            double rightx = _xmin + (left + 1) * _deltax - x;
            _y[left + 1] += delta * leftx / _deltax * mu;
            _y[left] += delta * rightx / _deltax * mu;
        }

        public double GetFunctionValue(double x)
        {
            if (x < _xmin)
            {
                double derrivative = (_y[1] - _y[0]) / _deltax;
                return _y[1] - derrivative * (_xmin + _deltax - x);
            }
            if (x > _xmax)
            {
                double derrivative = (_y[_y.Length - 1] - _y[_y.Length - 2]) / _deltax;
                return _y[_y.Length - 2] + derrivative * (x - (_xmax - _deltax));
            }
            int left = (int)((x - _xmin) / _deltax);
            if (left < 0) left = 0;
            if (left >= _y.Length - 1) return _y[_y.Length - 1];
            double leftx = x - (_xmin + left * _deltax);
            return (_y[left + 1] - _y[left]) / _deltax * leftx + _y[left];
        }

        public double GetXmin()
        {
            return _xmin;
        }

        public double GetXmax()
        {
            return _xmax;
        }
    }
}
