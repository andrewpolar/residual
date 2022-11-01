using System;
using System.Collections.Generic;
using System.Text;

namespace Residual
{
    interface IPLL
    {
        public void SetRandom(double ymin, double ymax);
        public double GetDerrivative(double x);
        public void Update(double x, double delta, double mu);
        public double GetFunctionValue(double x);
        public double GetXmin();
        public double GetXmax();
    }
}
