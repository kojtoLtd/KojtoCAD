using System;

namespace KojtoCAD.Mathematics.Geometry
{
    public class KLine2D
    {
        //---members--------------
        public double A, B, C;
        //---constructors---------
        public KLine2D()
        {
            A = 0;
            B = 0;
            C = 0;
        }
        public KLine2D(KLine2D l)
        {
            A = l.A;
            B = l.B;
            C = l.C;
        }
        public KLine2D(KLine2D l, double delta)
        // |L| - разстоянието от (0,0) до правата
        //delta > 0 = |L| нараства ; delta < 0 = |L| намалявa
        {
            double k = (l.C == 0) ? 1 : (l.C / Math.Abs(l.C));
            C = Math.Abs(l.C) + delta * Math.Sqrt(l.A * l.A + l.B * l.B);
            C *= k;
            A = l.A;
            B = l.B;
        }
        public KLine2D(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }
        public KLine2D(Complex z1, Complex z2)
        {
            A = z2.imag() - z1.imag();
            B = z1.real() - z2.real();
            C = -z1.real() * A - z1.imag() * B;
        }
        public KLine2D(Quaternion q1, Quaternion q2)
        {
            Complex z1 = new Complex(q1.GetX(), q1.GetY());
            Complex z2 = new Complex(q2.GetX(), q2.GetY());

            A = z2.imag() - z1.imag();
            B = z1.real() - z2.real();
            C = -z1.real() * A - z1.imag() * B;
        }

        //---functions---------------------------
        public int PositionOfТhePointToLineSign(Complex c)
        {
            int rez = 0;
            double d = A * c.real() + B * c.imag() + C;

            if (Math.Abs(d) > 0.00000001)
            {
                if (d < 0) { rez = -1; }
                else { rez = 1; }
            }

            return rez;
        }
        public double PositionOfТhePointToLine(Complex c)
        {
            return A * c.real() + B * c.imag() + C;
        }
        public Complex IntersectWitch(KLine2D l)
        {
            double x = (-C * l.B + B * l.C) / (A * l.B - B * l.A);
            double y = (-A * l.C + C * l.A) / (A * l.B - B * l.A);

            return new Complex(x, y);
        }
        public Complex IntersectWitch(Complex z1, Complex z2)
        {
            return new Complex(IntersectWitch(new KLine2D(z1, z2)));
        }
        public Complex IntersectWitch(Quaternion q1, Quaternion q2)
        {
            return new Complex(IntersectWitch(new KLine2D(new Complex(q1.GetX(), q1.GetY()), new Complex(q2.GetX(), q2.GetY()))));
        }
        public static bool IsParalel(KLine2D l1, KLine2D l2)
        {
            bool rez = false;
            Complex c = (new Complex(l1.A, l1.B)) / (new Complex(l2.A, l2.B));
            c.x = 0.0;
            if (c.norm() < 0.00000001)
            {
                rez = true;
            }
            return rez;
        }

    }
}
