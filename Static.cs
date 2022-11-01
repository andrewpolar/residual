using System;
using System.Collections.Generic;
using System.Text;

namespace Residual
{
    class Static
    {
        public static double PearsonCorrelation(double[] x, double[] y)
        {
            int length = x.Length;
            if (length > y.Length)
            {
                length = y.Length;
            }

            double xy = 0.0;
            double x2 = 0.0;
            double y2 = 0.0;
            for (int i = 0; i < length; ++i)
            {
                xy += x[i] * y[i];
                x2 += x[i] * x[i];
                y2 += y[i] * y[i];
            }
            xy /= (double)(length);
            x2 /= (double)(length);
            y2 /= (double)(length);
            double xav = 0.0;
            for (int i = 0; i < length; ++i)
            {
                xav += x[i];
            }
            xav /= length;
            double yav = 0.0;
            for (int i = 0; i < length; ++i)
            {
                yav += y[i];
            }
            yav /= length;
            double ro = xy - xav * yav;
            ro /= Math.Sqrt(x2 - xav * xav);
            ro /= Math.Sqrt(y2 - yav * yav);
            return ro;
        }

        /////// Kolmogorov-Smirnov test
        private static double GetFunctionValue(double[] f, double v)
        {
            if (v > f[f.Length - 1]) return 1.0;
            int n = 0;
            for (int i = 0; i < f.Length; ++i)
            {
                if (f[i] > v)
                {
                    n = i;
                    break;
                }
            }
            double delta = 1.0 / (double)(f.Length);
            return (double)(n) * delta;
        }

        public static bool KSTRejected005(double[] x, double[] y)
        {
            double D = Double.MinValue;
            double deltaX = 1.0 / (double)(x.Length);
            for (int i = 0; i < x.Length; ++i)
            {

                double f1 = (double)(i + 1) * deltaX;
                double f2 = GetFunctionValue(y, x[i]);
                double diff = Math.Abs(f1 - f2);
                if (diff > D)
                {
                    D = diff;
                }
            }
            double deltaY = 1.0 / (double)(y.Length);
            for (int i = 0; i < y.Length; ++i)
            {
                double f1 = GetFunctionValue(x, y[i]);
                double f2 = (double)(i + 1) * deltaY;
                double diff = Math.Abs(f1 - f2);
                if (diff > D)
                {
                    D = diff;
                }
            }
            double D_critical = 1.358 * Math.Sqrt(1.0 / (double)(x.Length) + 1.0 / (double)(y.Length));
            if (D >= D_critical) return true;
            else return false;
        }
    }
}
