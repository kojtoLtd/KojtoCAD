// -----------------------------------------------------------------------
// <copyright file="Quaternion.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace KojtoCAD.Mathematics.Geometry
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Quaternion// double  quaternion
    {
        //--- members
        public double[] q = new double[4];

        //--constructors
        public Quaternion()
        {
            q[0] = q[1] = q[2] = q[3] = 0.0;
        }
        public Quaternion(double Q, double X, double Y, double Z)
        {
            q[0] = Q;
            q[1] = X;
            q[2] = Y;
            q[3] = Z;
        }
        public Quaternion(double[] arr)
        {
            q[0] = arr[0];
            q[1] = arr[1];
            q[2] = arr[2];
            q[3] = arr[3];
        }
        public Quaternion(double Q, double[] arr)
        {
            q[0] = Q;
            q[1] = arr[0];
            q[2] = arr[1];
            q[3] = arr[2];
        }
        public Quaternion(double Q, Matrix m)
        {
            q[0] = Q;
            q[1] = m.arr[0];
            q[2] = m.arr[1];
            q[3] = m.arr[2];
        }
        public Quaternion(Matrix m)
        {
            q[0] = m.arr[0];
            q[1] = m.arr[1];
            q[2] = m.arr[2];
            q[3] = m.arr[3];
        }
        public Quaternion(Complex c)
        {
            q[0] = c.real();
            q[1] = c.imag();
            q[2] = q[3] = 0.0;
        }
        public Quaternion(Complex c, Complex c1)
        {
            q[0] = c.real();
            q[1] = c.imag();
            q[2] = c1.real();
            q[3] = c1.imag();
        }
        public Quaternion(Quaternion Q)
        {
            q[0] = Q.q[0];
            q[1] = Q.q[1];
            q[2] = Q.q[2];
            q[3] = Q.q[3];
        }
        public Quaternion(Matrix ort, double fi)
        {
            q[0] = Math.Cos(fi);
            q[1] = ort.arr[0] * Math.Sin(fi);
            q[2] = ort.arr[1] * Math.Sin(fi);
            q[3] = ort.arr[2] * Math.Sin(fi);
        }
        public Quaternion(double Q)
        {
            q[0] = Q;
            q[1] = 0;
            q[2] = 0;
            q[3] = 0;
        }

        //--operators        
        public static Quaternion operator -(Quaternion Q1, Quaternion Q2)
        {
            return new Quaternion(Q1.q[0] - Q2.q[0], Q1.q[1] - Q2.q[1], Q1.q[2] - Q2.q[2], Q1.q[3] - Q2.q[3]);
        }
        public static Quaternion operator +(Quaternion Q1, Quaternion Q2)
        {
            return new Quaternion(Q1.q[0] + Q2.q[0], Q1.q[1] + Q2.q[1], Q1.q[2] + Q2.q[2], Q1.q[3] + Q2.q[3]);
        }
        public static Quaternion operator *(Quaternion Q, double d)
        {
            return new Quaternion(Q.q[0] * d, Q.q[1] * d, Q.q[2] * d, Q.q[3] * d);
        }
        public static Quaternion operator *(double d, Quaternion Q)
        {
            return new Quaternion(Q.q[0] * d, Q.q[1] * d, Q.q[2] * d, Q.q[3] * d);
        }
        public static Quaternion operator /(Quaternion Q, double d)
        {
            return new Quaternion(Q.q[0] / d, Q.q[1] / d, Q.q[2] / d, Q.q[3] / d);
        }
        public static Quaternion operator /(double d, Quaternion Q)
        {
            return new Quaternion(Q.q[0] / d, Q.q[1] / d, Q.q[2] / d, Q.q[3] / d);
        }
        public static Quaternion operator *(Quaternion A, Quaternion Q)
        {
            return new Quaternion
                (
                    A.q[0] * Q.q[0] - A.q[1] * Q.q[1] - A.q[2] * Q.q[2] - A.q[3] * Q.q[3],
                    A.q[0] * Q.q[1] + A.q[1] * Q.q[0] + A.q[2] * Q.q[3] - A.q[3] * Q.q[2],
                    A.q[0] * Q.q[2] + A.q[2] * Q.q[0] + A.q[3] * Q.q[1] - A.q[1] * Q.q[3],
                    A.q[0] * Q.q[3] + A.q[3] * Q.q[0] + A.q[1] * Q.q[2] - A.q[2] * Q.q[1]
                );
        }
        public static Quaternion operator /(Quaternion Q1, Quaternion Q2)
        {
            return new Quaternion(new Quaternion((Q1 * Q2.conj()) / Q2.norm()));
        }
        public static Quaternion operator +(Quaternion Q1, double Q2)
        {
            return new Quaternion(Q1.q[0] + Q2, Q1.q[1], Q1.q[2], Q1.q[3]);
        }
        public static Quaternion operator +(double Q2, Quaternion Q1)
        {
            return new Quaternion(Q1.q[0] + Q2, Q1.q[1], Q1.q[2], Q1.q[3]);
        }
        public static Quaternion operator -(Quaternion Q1, double Q2)
        {
            return new Quaternion(Q1.q[0] - Q2, Q1.q[1], Q1.q[2], Q1.q[3]);
        }
        public static Quaternion operator -(double Q2,Quaternion Q1)
        {
            return new Quaternion(Q2 - Q1.q[0], -Q1.q[1], -Q1.q[2], -Q1.q[3]);
        }

        // --- function --
        public Quaternion conj()
        {
            return new Quaternion(q[0], -q[1], -q[2], -q[3]);
        }
        public double norm()
        {
            return q[0] * q[0] + q[1] * q[1] + q[2] * q[2] + q[3] * q[3];
        }
        public double abs()
        {
            return Math.Sqrt(q[0] * q[0] + q[1] * q[1] + q[2] * q[2] + q[3] * q[3]);
        }
        public double absV()
        {
            return Math.Sqrt(q[1] * q[1] + q[2] * q[2] + q[3] * q[3]);
        }
        public double arg()
        {
            double d = absV();
            return (d > Math.Abs(q[0])) ? Math.Asin(d / abs()) : Math.Acos(q[0] / abs());
        }
        public double real()
        {
            return q[0];
        }
        public Matrix imag()
        {
            return new Matrix(3, 1, new double[] { q[1], q[2], q[3] });
        }
        public Matrix ort()
        {
            return new Matrix(imag() / abs());
        }

        public Matrix ToMatrixR()
        {
            return new Matrix(1, 4, new double[] { q[0], q[1], q[2], q[3] });
        }
        public Matrix ToMatrixC()
        {
            return new Matrix(4, 1, new double[] { q[0], q[1], q[2], q[3] });
        }
        public double[] ToArray()
        {
            return new double[4] { q[0], q[1], q[2], q[3] };
        }
        public double GetX()
        {
            return q[1];
        }
        public double GetY()
        {
            return q[2];
        }
        public double GetZ()
        {
            return q[3];
        }
        public double cosTo(Quaternion Q)
        {
            return (q[1] * Q.q[1] + q[2] * Q.q[2] + q[3] * Q.q[3]) / (abs() * Q.abs());
        }
        public double angTo(Quaternion Q)
        {
            return Math.Acos(cosTo(Q));
        }
        public double cosToX()
        {
            return q[1] / abs();
        }
        public double angToX()
        {
            return Math.Acos(cosToX());
        }
        public double cosToY()
        {
            return q[2] / abs();
        }
        public double angToY()
        {
            return Math.Acos(cosToY());
        }
        public double cosToZ()
        {
            return q[3] / abs();
        }
        public double angToZ()
        {
            return Math.Acos(cosToZ());
        }
        public Quaternion set_rotateAroundAxe(Quaternion Q, double f)
        {
            Quaternion t = new Quaternion(Q.ort(), f / 2);
            Quaternion QQ = this;
            QQ = t * QQ * t.conj();

            this.q[0] = QQ.q[0];
            this.q[1] = QQ.q[1];
            this.q[2] = QQ.q[2];
            this.q[3] = QQ.q[3];
            return this;
        }
        public Quaternion set_rotateAroundAxe(Quaternion s, Quaternion e, double f)
        //posoka na zawyrtane: gledame ot E kam S i zavartame obratno na chasovnika
        {
            Quaternion t = new Quaternion((e - s).ort(), f / 2);
            Quaternion QQ = this;
            QQ = t * (QQ - s) * t.conj();
            QQ += s;

            this.q[0] = QQ.q[0];
            this.q[1] = QQ.q[1];
            this.q[2] = QQ.q[2];
            this.q[3] = QQ.q[3];
            return this;
        }
        public Quaternion get_rotateAroundAxe(Quaternion Q, double f)
        {
            Quaternion t = new Quaternion(Q.ort(), f / 2);
            Quaternion QQ = this;
            QQ = t * QQ * t.conj();

            return QQ;
        }
        public Quaternion get_rotateAroundAxe(Quaternion s, Quaternion e, double f)
        //posoka na zawyrtane: gledame ot E kam S i zavartame obratno na chasovnika
        {
            Quaternion t = new Quaternion((e - s).ort(), f / 2);
            Quaternion QQ = this;
            QQ = t * (QQ - s) * t.conj();
            QQ += s;

            return QQ;
        }

    }
}
