using System;

namespace KojtoCAD.Mathematics.Geometry
{
    public class KPlane
    {
        //-- members ------
        Quaternion n;
        double D;
        //-- constructors ---
        public KPlane()
        {
            n = new Quaternion();
            D = 0.0;
        }
        public KPlane(KPlane pl)
        {
            D = pl.D;
            n = new Quaternion(0.0, pl.n.imag());
        }
        public KPlane(ref Quaternion q)
        {
            n = new Quaternion(0.0, n.imag());
            D = n.real();
        }
        public KPlane(Quaternion q1, Quaternion q2, Quaternion q3)//равнина през 3 точки 
        {
            double A = (new Matrix(3, new double[] { q1.GetY(), q1.GetZ(), 1, q2.GetY(), q2.GetZ(), 1, q3.GetY(), q3.GetZ(), 1 })).Det();
            double B = -(new Matrix(3, new double[] { q1.GetX(), q1.GetZ(), 1, q2.GetX(), q2.GetZ(), 1, q3.GetX(), q3.GetZ(), 1 })).Det();
            double C = (new Matrix(3, new double[] { q1.GetX(), q1.GetY(), 1, q2.GetX(), q2.GetY(), 1, q3.GetX(), q3.GetY(), 1 })).Det();
            D = -(new Matrix(3, new double[] { q1.GetX(), q1.GetY(), q1.GetZ(), q2.GetX(), q2.GetY(), q2.GetZ(), q3.GetX(), q3.GetY(), q3.GetZ() })).Det();
            n = new Quaternion(0.0, A, B, C);
        }

        //--- funcions ---
        public double GetA()
        {
            return n.GetX();
        }
        public double GetB()
        {
            return n.GetY();
        }
        public double GetC()
        {
            return n.GetZ();
        }
        public double GetD()
        {
            return D;
        }
        public Quaternion IntersectWithVector(Quaternion e, Quaternion s)
        {
            Quaternion v = new Quaternion(e - s);
            double d = (-n.GetX() * s.GetX() - n.GetY() * s.GetY() - n.GetZ() * s.GetZ() - D) / (n.GetX() * v.GetX() + n.GetY() * v.GetY() + n.GetZ() * v.GetZ());
            Quaternion rez = new Quaternion(0, d * v.GetX() + s.GetX(), d * v.GetY() + s.GetY(), d * v.GetZ() + s.GetZ());
            if (((double.IsInfinity(Math.Abs(rez.GetX())))) || ((double.IsInfinity(Math.Abs(rez.GetY())))) || ((double.IsInfinity(Math.Abs(rez.GetZ())))) ||
                double.IsNaN(rez.GetX()) || double.IsNaN(rez.GetY()) || double.IsNaN(rez.GetZ()))
            {
                rez = new Quaternion(-1, 0, 0, 0);
            }
            return rez;
        }


        public double dist()
        {
            return D / Math.Sqrt(n.GetX() * n.GetX() + n.GetY() * n.GetY() + n.GetZ() * n.GetZ());
        }
        public double dist(double X, double Y, double Z)//връща разстоянието до точка M(X,Y,Z)
        {
            return (n.GetX() * X + n.GetY() * Y + n.GetZ() * Z + D) / Math.Sqrt(n.GetX() * n.GetX() + n.GetY() * n.GetY() + n.GetZ() * n.GetZ());
        }
        public double dist(Quaternion Q)//връща разстоянието до точка Q
        {
            return (n.GetX() * Q.GetX() + n.GetY() * Q.GetY() + n.GetZ() * Q.GetZ() + D) /
                Math.Sqrt(n.GetX() * n.GetX() + n.GetY() * n.GetY() + n.GetZ() * n.GetZ());
        }

    }
}
