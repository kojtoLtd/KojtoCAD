// -----------------------------------------------------------------------
// <copyright file="Complex.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace KojtoCAD.Mathematics.Geometry
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Complex//double complex
    {
        //--- members
        public double x;
        public double y;
        public static double dif = 0.0000000000001;//ползва за сравнение на две реални числа ( double )

        //--constructors
        public Complex()
        {
            x = y = 0;
        }
        public Complex(double X, double Y)
        {
            x = X;
            y = Y;
        }
        public Complex(Complex c)
        {
            x = c.x;
            y = c.y;
        }
        //--operators
        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.x - c2.x, c1.y - c2.y);
        }
        public static Complex operator -(Complex c1)
        {
            return new Complex(-c1.x, -c1.y);
        }
        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.x + c2.x, c1.y + c2.y);
        }
        public static Complex operator +(double d, Complex c2)
        {
            return new Complex(d + c2.x, c2.y);
        }

        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(c1.x * c2.x - c1.y * c2.y, c1.x * c2.y + c2.x * c1.y);
        }
        public static Complex operator *(Complex c1, double d)
        {
            return new Complex(c1.x * d, c1.y * d);
        }
        public static Complex operator /(Complex c1, double d)
        {
            return new Complex(c1.x / d, c1.y / d);
        }
        public static Complex operator /(Complex c1, Complex c2)
        {
            return (c1 * c2.conj()) / (c2.x * c2.x + c2.y * c2.y);
        }
        public static bool operator !=(Complex c1, Complex c2)
        {
            return (Math.Abs(c1.real() - c2.real()) > dif) || (Math.Abs(c1.imag() - c2.imag()) > dif);
        }
        public static bool operator ==(Complex c1, Complex c2)
        {
            return !(c1 != c2);
        }

        public static implicit operator Matrix(Complex c)
        {
            return new Matrix(2, 1, new double[2] { c.real(), c.imag() });
        }


        // --- function --             
        public double imag()
        {
            return y;
        }
        public double real()
        {
            return x;
        }
        public double abs()
        {
            return Math.Sqrt(x * x + y * y);
        }
        public double modul()
        {
            return Math.Sqrt(x * x + y * y);
        }
        public double arg()
        {
            double res = (y >= 0) ? Math.Acos(x / Math.Sqrt(x * x + y * y)) : -Math.Acos(x / Math.Sqrt(x * x + y * y));
            return (res < 0) ? (2 * Math.PI + res) : res;
        }
        public double norm()
        {
            return x * x + y * y;
        }
        public Complex conj()
        {
            return new Complex(x, -y);
        }
        public static Complex polar(double m, double ang)
        {
            return new Complex(m * Math.Cos(ang), m * Math.Sin(ang));
        }
        public override string ToString()
        {
            return x.ToString("F8") + "," + y.ToString("F8");
        }
        public string ToString1()
        {
            return x.ToString("F15") + "," + y.ToString("F15");
        }
        public string ToString2()
        {
            return x.ToString("F12") + "," + y.ToString("F12");
        }
        public string ToInfo()
        {
            return x.ToString("F4") + " , " + y.ToString("F4") + "<br>modul = " + abs().ToString("F4") + "<br>arg = " + (arg() * 180 / Math.PI).ToString("F4") + "<br>";
        }
        public override bool Equals(Object obj)
        {
            return ((obj is Complex) && (this == (Complex)obj));
        }
        public override int GetHashCode()
        {
            return (x.GetHashCode() ^ y.GetHashCode());
        }
    }
}
