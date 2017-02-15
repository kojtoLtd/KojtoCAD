using System;
#if !bcad
using Autodesk.AutoCAD.Geometry;
#else
using Teigha.Geometry;
#endif

namespace KojtoCAD.KojtoCAD3D
{
    static class Constants
    {
        public const double pi = 3.1415926535897932384626433832795;
        public const double zero_dist = 0.0000001;
        public const double zero_angular_difference = 0.000000001;
        public const double zero_area_difference = 0.000000001;        
    }
    static class Common
    {
        public static double[] GetAutoCAD_Matrix3d(ref UCS ucs)        
        {
            return new double[]{ucs.ang.GetAt(1, 1), ucs.ang.GetAt(1, 2), ucs.ang.GetAt(1, 3), ucs.o.GetX(),
                                ucs.ang.GetAt(2, 1), ucs.ang.GetAt(2, 2), ucs.ang.GetAt(2, 3), ucs.o.GetY(),
                                ucs.ang.GetAt(3, 1), ucs.ang.GetAt(3, 2), ucs.ang.GetAt(3, 3), ucs.o.GetZ(),
                                0.0,                 0.0,                 0.0,          1.0};
        }
        public static double[] GetScaleAutoCAD_Matrix3d(double k, quaternion B /*base*/)
        {
            return new double[]{ k  ,  0.0 ,  0.0 ,  -B.GetX()*(k-1.0),
                                 0.0,     k,   0.0,  -B.GetY()*(k-1.0),
                                 0.0,   0.0,     k,  -B.GetZ()*(k-1.0),
                                 0.0,   0.0,   0.0,  1.0}; ;
        }
        public static matrix Line(complex z1, complex z2)//m(0,0) = A, m(0,1)=B,m(0,2)=C 
        {
            matrix m = new matrix(1, 3);
            m.SetAt(0,0,z2.imag() - z1.imag());
            m.SetAt(0,1,z1.real()-z2.real());
            m.SetAt(0,2, -z1.real() * m.GetAt(0,0) - z1.imag() * m.GetAt(0,1));
            return m;
        }
        public static matrix IntersectCircleAndLine(double r, complex z1, complex z2)
        //m(0,0) = X1, m(0,1)=Y1, m(0,2)=X2, m(0,3) = Y2, m(0,4) < 0 ako nema presi4ane
        {
            matrix m = new matrix(1, 5);
            matrix L = Line(z1,z2);
            double a, b, c, A, B, C;
            A = L.GetAt(0, 0); B = L.GetAt(0, 1); C = L.GetAt(0, 2);
            if (A != 0.0)
            {
                #region
                a = B * B / (A * A) + 1;
                b = 2 * B * C / (A * A);
                c = C * C / (A * A) - r * r;

                double D = b * b - 4 * a * c;
                if (D >= 0)
                {
                    D = Math.Sqrt(D);
                    m.SetAt(0, 1, (-b + D) / (2 * a));
                    m.SetAt(0, 3, (-b - D) / (2 * a));
                    m.SetAt(0, 0,(-C - B * m.GetAt(0, 1)) / A);
                    m.SetAt(0, 2,(-C - B * m.GetAt(0, 3)) / A);
                }
                else
                {
                    m.SetAt(0, 0, 0); m.SetAt(0, 1, 0); m.SetAt(0, 2, 0); m.SetAt(0, 3,0);
                    m.SetAt(0, 4,-1);
                }
                #endregion
            }
            else
            {
                #region
                a = A * A / (B * B) + 1;
                b = 2 * A * C / (B * B);
                c = C * C / (B * B) - r * r;
                double D = b * b - 4 * a * c;

                if (D >= 0)
                {
                    D = Math.Sqrt(D);
                    m.SetAt(0, 0, (-b + D) / (2 * a));
                    m.SetAt(0, 2, (-b - D) / (2 * a));
                    m.SetAt(0, 1, (-C - A * m.GetAt(0, 0)) / B);
                    m.SetAt(0, 3, (-C - A * m.GetAt(0, 2)) / B);
                }
                else
                {
                    m.SetAt(0, 0, 0); m.SetAt(0, 1,0); m.SetAt(0, 2, 0); m.SetAt(0, 3, 0);
                    m.SetAt(0, 4, -1);
                }
                #endregion
            }
            return m;
        }

        //3 points and line
        //p1,p2 define line
        // nout coincident - return null
        public static quaternion IsCoincidentWithLine(quaternion p1, quaternion p2, quaternion p)
        {
            quaternion rez = (p - p1) / (p2 - p1);           
            return (rez.absV() < Constants.zero_dist) ? rez: null;
        }

        //p1 - start na otse4kata, p2 - end of segment
        // nout internal - return null
        public static quaternion IsInternalForSegment(quaternion p1, quaternion p2, quaternion p)
        {
            quaternion rez = (p - p1) / (p2 - p1);
            return (rez.absV() < Constants.zero_dist && rez.real() > Constants.zero_dist && rez.real() < (1.0 - Constants.zero_dist)) ? rez : null;
        }

        public static double doubleTruncate(double d, int di)
        {
            int k = (d < 0.0) ? -1 : 1;
            double D = Math.Abs(d);
            double t0 = Math.Truncate(D);
            double t1 = D - t0;                //drobna 4ast
            double t2 = Math.Pow(10.0, di);
            t1 *= t2;
            t1 = Math.Truncate(t1) / t2;

            return (t0 + t1) * k;            
        }
    }
    public class matrix//double Matrix
    {
        //--- members
        public int r;
        public int c;
        public double[] arr;// a11,a12,a13,a21,a22,a23,.......
        //--constructors
        public matrix()
        {
            r = 0;
            c = 0;
            arr = new double[0];
        }
        public matrix(int R, int C)
        {
            r = R;
            c = C;
            arr = new double[R * C];
        }
        public matrix(int R)/*квадратна*/{
            r = R;
            c = R;
            arr = new double[R * R];
        }
        public matrix(matrix m)
        {
            r = m.r;
            c = m.c;
            arr = new double[m.r * m.c];
            for (int i = 0; i < m.arr.Length; i++)
            {
                arr[i] = m.arr[i];
            }
        }//copy constructor
        public matrix(int R, int C, matrix m)//подматрица на m чрез премахване на ред R и стълб C
        {
            r = m.r - 1;
            c = m.c - 1;
            arr = new double[r * c];
            int k = 0;
            int kk = 0;
            for (int i = 0; i < m.arr.Length; i++)
            {
                if (i < C)
                {
                    kk = 1;
                }
                else if (i == C)
                {
                    kk = 0;
                }
                else
                {
                    kk++;
                    if (kk == m.c)
                    {
                        kk = 0;
                    }
                }
                if (!((i >= m.c * R) && (i < m.c * (R + 1))) && (kk != 0))
                {
                    arr[k] = m.arr[i];
                    k++;
                }
            }
        }
        public matrix(int R, int C, double[] ARR)
        {
            r = R;
            c = C;
            arr = new double[ARR.Length];
            for (int i = 0; i < R * C; i++)
            {
                arr[i] = ARR[i];
            }
        }
        public matrix(int R, double[] ARR)//квадратна матрица
        {
            r = R;
            c = R;
            arr = new double[ARR.Length];
            for (int i = 0; i < R * R; i++)
            {
                arr[i] = ARR[i];
            }
        }

        //--operators
        public static matrix operator *(matrix m1, matrix m2)
        {
            matrix m = new matrix(m1.r, m2.c);
            double S = 0;
            for (int k = 0; k < m2.c; k++)//брой колони на дясната матрица
            {
                for (int i = 0; i < m1.r; i++)//брой редове на лявата матрица
                {
                    for (int j = 0; j < m1.c; j++)//брой колони на лявата матрица
                    {
                        S += (m1.arr[i * m1.c + j] * m2.arr[k + j * m2.c]);
                    }
                    m.arr[i * m.c + k] = S;
                    S = 0;
                }
            }
            return m;
        }
        public static matrix operator *(matrix m1, double d)
        {
            matrix m = new matrix(m1.r, m1.c);
            for (int i = 0; i < m1.arr.Length; i++)
            {
                m.arr[i] = m1.arr[i] * d;
            }
            return m;
        }
        public static matrix operator *(double d, matrix m1)
        {
            matrix m = new matrix(m1.r, m1.c);
            for (int i = 0; i < m1.arr.Length; i++)
            {
                m.arr[i] = m1.arr[i] * d;
            }
            return m;
        }
        public static matrix operator +(matrix m1, matrix m2)
        {
            matrix m = new matrix(m1.r, m1.c);
            for (int i = 0; i < m1.arr.Length; i++)
            {
                m.arr[i] = m1.arr[i] + m2.arr[i];
            }
            return m;
        }
        public static matrix operator -(matrix m1, matrix m2)
        {
            matrix m = new matrix(m1.r, m1.c);
            for (int i = 0; i < m1.arr.Length; i++)
            {
                m.arr[i] = m1.arr[i] - m2.arr[i];
            }
            return m;
        }
        public static matrix operator /(matrix m1, double d)
        {
            matrix m = new matrix(m1.r, m1.c);
            for (int i = 0; i < m1.arr.Length; i++)
            {
                m.arr[i] = m1.arr[i] / d;
            }
            return m;
        }
        // --- function --
        /// <summary>
        /// Re6avane sistema lineini urawneniq 
        /// </summary>
        /// <param name="i">matrica stalb na svobodnite koeficienti</param>
        /// <param name="m">this.r = m.r</param>
        /// <returns></returns>
        public double GetXi(int i, matrix m)
        {
            double res = Det();
            matrix mm = ChangeColumn(i, 0, m);
            return mm.Det() / res;
        }
        /// <summary>
        /// Re6avane sistema lineini urawneniq
        /// </summary>
        /// <param name="i">matrica stalb na svobodnite koeficienti</param>
        /// <returns></returns>
        public matrix GetMatXi(matrix m)
        {
            matrix mmm = new matrix(r, 1);
            double res = Det();
            for (int j = 0; j < r; j++)
            {
                mmm.arr[j] = ChangeColumn(j, 0, m).Det() / res;
            }
            return mmm;
        }
        /// <summary>
        /// //връща обратната матрица ( само за квадратни )
        /// </summary>
        /// <returns></returns>
        public matrix Mat_1()
        {
            matrix m = new matrix(r);
            double[] arr = new double[r];
            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < r; j++)
                {
                    arr[j] = (i == j) ? 1 : 0;
                }
                m = m.ChangeColumn(i, 0, GetMatXi(new matrix(r, 1, arr)));
            }
            return m;
        }
        /// <summary>
        /// // c - nomera na kolonata ot this koqato shtee se zameni s kolona c1 ot mm. mm i this imat ednakyw broj redove
        /// </summary>
        /// <param name="cN"></param>
        /// <param name="c1"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        public matrix ChangeColumn(int cN, int c1, matrix mm)
        {
            matrix m = new matrix(r, c);
            for (int i = 0; i < arr.Length; i++)
            {
                m.arr[i] = arr[i];
            }
            for (int i = 0; i < r; i++)
            {
                m.arr[cN + i * c] = mm.arr[c1 + i * mm.c];
            }
            return m;
        }
        public matrix Tr()
        {
            matrix m = new matrix(c, r);
            int k = 0;
            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < r; j++)
                {
                    m.arr[k] = arr[i + j * c];
                    k++;
                }
            }
            return m;
        }
        public double Det()
        {
            double res = 0;
            if (r > 2)
            {
                for (int i = 0; i < r; i++)
                {
                    res += Math.Pow(-1, 2 + i) * arr[i] * (new matrix(0, i, this).Det());
                }
            }
            else if (r == 2)
            {
                res = arr[0] * arr[3] - arr[1] * arr[2];
            }
            else
            {
                res = arr[0];
            }

            return res;
        }
        public double GetAt(int R, int C)
        {
            return arr[c * R + C];
        }
        public void SetAt(int R, int C, double d)
        {
            arr[c * R + C] = d;
        }
        public string GetStr()
        {
            string str = "";
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    str += GetAt(i, j).ToString("f");
                    str += " , ";
                }
                str += "<br>\n";
            }
            return str;
        }
        public matrix column(int C)
        {
            matrix m = new matrix(r, 1);
            for (int i = 0; i < r; i++)
            {
                m.arr[i] = arr[c * i + C];
            }
            return m;
        }
        public matrix row(int R)
        {
            matrix m = new matrix(1, c);
            for (int i = 0; i < c; i++)
            {
                m.arr[i] = arr[c * R + i];
            }
            return m;
        }
    }
    public class complex//double complex
    {
        //--- members
        public double x;
        public double y;
        public static double dif = 0.0000000000001;//ползва за сравнение на две реални числа ( double )
        public double First { get { return (x); } }
        public double Second { get { return (y); } }

        //--constructors
        public complex()
        {
            x = y = 0;
        }
        public complex(double X, double Y)
        {
            x = X;
            y = Y;
        }
        public complex(complex c)
        {
            x = c.x;
            y = c.y;
        }
        //--operators
        public static complex operator -(complex c1, complex c2)
        {
            return new complex(c1.x - c2.x, c1.y - c2.y);
        }
        public static complex operator -(complex c1)
        {
            return new complex(-c1.x, -c1.y);
        }
        public static complex operator +(complex c1, complex c2)
        {
            return new complex(c1.x + c2.x, c1.y + c2.y);
        }
        public static complex operator +(double d, complex c2)
        {
            return new complex(d + c2.x, c2.y);
        }

        public static complex operator *(complex c1, complex c2)
        {
            return new complex(c1.x * c2.x - c1.y * c2.y, c1.x * c2.y + c2.x * c1.y);
        }
        public static complex operator *(complex c1, double d)
        {
            return new complex(c1.x * d, c1.y * d);
        }
        public static complex operator /(complex c1, double d)
        {
            return new complex(c1.x / d, c1.y / d);
        }
        public static complex operator /(complex c1, complex c2)
        {
            return (c1 * c2.conj()) / (c2.x * c2.x + c2.y * c2.y);
        }
        public static bool operator !=(complex c1, complex c2)
        {
            return (Math.Abs(c1.real() - c2.real()) > dif) || (Math.Abs(c1.imag() - c2.imag()) > dif);
        }
        public static bool operator ==(complex c1, complex c2)
        {
            return !(c1 != c2);
        }

        public static implicit operator matrix(complex c)
        {
            return new matrix(2, 1, new double[2] { c.real(), c.imag() });
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
        public complex conj()
        {
            return new complex(x, -y);
        }
        public static complex polar(double m, double ang)
        {
            return new complex(m * Math.Cos(ang), m * Math.Sin(ang));
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
            return ((obj is complex) && (this == (complex)obj));
        }
        public override int GetHashCode()
        {
            return (x.GetHashCode() ^ y.GetHashCode());
        }
    }
    public class quaternion// double  quaternion
    {
        //--- members
        public double[] q = new double[4];

        public double First { get { return (q[1]); } }
        public double Second { get { return (q[2]); } }
        public double Third { get { return (q[3]); } }

        //--constructors
        public quaternion()
        {
            q[0] = q[1] = q[2] = q[3] = 0.0;
        }
        public quaternion(double Q, double X, double Y, double Z)
        {
            q[0] = Q;
            q[1] = X;
            q[2] = Y;
            q[3] = Z;
        }
        public quaternion(double[] arr)
        {
            q[0] = arr[0];
            q[1] = arr[1];
            q[2] = arr[2];
            q[3] = arr[3];
        }
        public quaternion(double Q, double[] arr)
        {
            q[0] = Q;
            q[1] = arr[0];
            q[2] = arr[1];
            q[3] = arr[2];
        }
        public quaternion(double Q, matrix m)
        {
            q[0] = Q;
            q[1] = m.arr[0];
            q[2] = m.arr[1];
            q[3] = m.arr[2];
        }
        public quaternion(matrix m)
        {
            q[0] = m.arr[0];
            q[1] = m.arr[1];
            q[2] = m.arr[2];
            q[3] = m.arr[3];
        }
        public quaternion(complex c)
        {
            q[0] = c.real();
            q[1] = c.imag();
            q[2] = q[3] = 0.0;
        }
        public quaternion(complex c, complex c1)
        {
            q[0] = c.real();
            q[1] = c.imag();
            q[2] = c1.real();
            q[3] = c1.imag();
        }
        public quaternion(quaternion Q)
        {
            q[0] = Q.q[0];
            q[1] = Q.q[1];
            q[2] = Q.q[2];
            q[3] = Q.q[3];
        }
        public quaternion(matrix ort, double fi)
        {
            q[0] = Math.Cos(fi);
            q[1] = ort.arr[0] * Math.Sin(fi);
            q[2] = ort.arr[1] * Math.Sin(fi);
            q[3] = ort.arr[2] * Math.Sin(fi);
        }
        public quaternion(Point3d p)//***
        {
            q[0] = 0.0;
            q[1] = p.X;
            q[2] = p.Y;
            q[3] = p.Z;
        }

        //--operators        
        public static quaternion operator -(quaternion Q1, quaternion Q2)
        {
            return new quaternion(Q1.q[0] - Q2.q[0], Q1.q[1] - Q2.q[1], Q1.q[2] - Q2.q[2], Q1.q[3] - Q2.q[3]);
        }
        public static quaternion operator +(quaternion Q1, quaternion Q2)
        {
            return new quaternion(Q1.q[0] + Q2.q[0], Q1.q[1] + Q2.q[1], Q1.q[2] + Q2.q[2], Q1.q[3] + Q2.q[3]);
        }
        public static quaternion operator *(quaternion Q, double d)
        {
            return new quaternion(Q.q[0] * d, Q.q[1] * d, Q.q[2] * d, Q.q[3] * d);
        }
        public static quaternion operator *(double d, quaternion Q)
        {
            return new quaternion(Q.q[0] * d, Q.q[1] * d, Q.q[2] * d, Q.q[3] * d);
        }
        public static quaternion operator /(quaternion Q, double d)
        {
            return new quaternion(Q.q[0] / d, Q.q[1] / d, Q.q[2] / d, Q.q[3] / d);
        }
        public static quaternion operator /(double d, quaternion Q)
        {
            return new quaternion(Q.q[0] / d, Q.q[1] / d, Q.q[2] / d, Q.q[3] / d);
        }
        public static quaternion operator *(quaternion A, quaternion Q)
        {
            return new quaternion
                (
                    A.q[0] * Q.q[0] - A.q[1] * Q.q[1] - A.q[2] * Q.q[2] - A.q[3] * Q.q[3],
                    A.q[0] * Q.q[1] + A.q[1] * Q.q[0] + A.q[2] * Q.q[3] - A.q[3] * Q.q[2],
                    A.q[0] * Q.q[2] + A.q[2] * Q.q[0] + A.q[3] * Q.q[1] - A.q[1] * Q.q[3],
                    A.q[0] * Q.q[3] + A.q[3] * Q.q[0] + A.q[1] * Q.q[2] - A.q[2] * Q.q[1]
                );
        }
        public static quaternion operator /(quaternion Q1, quaternion Q2)
        {
            return new quaternion(new quaternion((Q1 * Q2.conj()) / Q2.norm()));
        }       
        public static explicit operator Point3d(quaternion qq) { return new Point3d(qq.GetX(), qq.GetY(), qq.GetZ()); }//***
        public static implicit operator quaternion(Point3d p) { return new quaternion(p); }//***
        
        
        public static bool operator ==(quaternion q1, quaternion q2)//***
        {
            return (q1-q2).abs()<Constants.zero_dist;
        }
        public static bool operator !=(quaternion q1, quaternion q2)//***
        {
            return (!(q1 == q2));
        }
        public override bool Equals(Object obj)//***
        {
            return ((obj is quaternion) && (this == (quaternion)obj));
        }
        

        public static bool operator ==(quaternion qq, Point3d pp)//***
        {
            return qq==new quaternion(pp);
        }
        public static bool operator !=(quaternion qq, Point3d pp)//***
        {
            return (!(qq == pp));
        }

        public static bool operator ==(Point3d pp, quaternion qq)//***
        {
            return qq == new quaternion(pp);
        }
        public static bool operator !=(Point3d pp, quaternion qq)//***
        {
            return (!(qq == pp));
        }

        public string ToString(bool b= false)//***
        {
            if(!b)
                return string.Format("{0},{1},{2},{3}",q[0],q[1],q[2],q[3]);
            else
                return string.Format("{0},{1},{2}", q[1], q[2], q[3]);
        }
        public override int GetHashCode()//***
        {           
            return q[0].GetHashCode() ^ q[1].GetHashCode() ^ q[2].GetHashCode() ^ q[3].GetHashCode();
        }
        public static bool IsEqual(quaternion q1, quaternion q2)//***
        {
            return q1 == q2;
        }
        public static bool IsEqual(quaternion qq, Point3d pp)//***
        {
            return qq == pp;
        }
        public static bool IsEqual(Point3d pp, quaternion qq)//***
        {
            return qq == pp;
        }    

        // --- function --
        public quaternion conj()
        {
            return new quaternion(q[0], -q[1], -q[2], -q[3]);
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
        public matrix imag()
        {
            return new matrix(3, 1, new double[] { q[1], q[2], q[3] });
        }
        public matrix ort()
        {
            return new matrix(imag() / abs());
        }

        public Point3d ToPoint3d() { return new Point3d(q[1], q[2], q[3]); }//***
        public matrix ToMatrixR()
        {
            return new matrix(1, 4, new double[] { q[0], q[1], q[2], q[3] });
        }
        public matrix ToMatrixC()
        {
            return new matrix(4, 1, new double[] { q[0], q[1], q[2], q[3] });
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
        public double cosTo(quaternion Q)
        {
            return (q[1] * Q.q[1] + q[2] * Q.q[2] + q[3] * Q.q[3]) / (abs() * Q.abs());
        }
        public double angTo(quaternion Q)
        {
            double cos = cosTo(Q);
            if (cos > 1.0) { cos = 1.0; }
            return Math.Acos(cos);
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
        public quaternion set_rotateAroundAxe(quaternion Q, double f)
        {
            quaternion t = new quaternion(Q.ort(), f / 2);
            quaternion QQ = this;
            QQ = t * QQ * t.conj();

            this.q[0] = QQ.q[0];
            this.q[1] = QQ.q[1];
            this.q[2] = QQ.q[2];
            this.q[3] = QQ.q[3];
            return this;
        }
        public quaternion set_rotateAroundAxe(quaternion s, quaternion e, double f)
        //posoka na zawyrtane: gledame ot E kam S i zavartame obratno na chasovnika
        {
            quaternion t = new quaternion((e - s).ort(), f / 2);
            quaternion QQ = this;
            QQ = t * (QQ - s) * t.conj();
            QQ += s;

            this.q[0] = QQ.q[0];
            this.q[1] = QQ.q[1];
            this.q[2] = QQ.q[2];
            this.q[3] = QQ.q[3];
            return this;
        }
        public quaternion get_rotateAroundAxe(quaternion Q, double f)
        {
            quaternion t = new quaternion(Q.ort(), f / 2);
            quaternion QQ = this;
            QQ = t * QQ * t.conj();

            return QQ;
        }
        public quaternion get_rotateAroundAxe(quaternion s, quaternion e, double f)
        //posoka na zawyrtane: gledame ot E kam S i zavartame obratno na chasovnika
        {
            quaternion t = new quaternion((e - s).ort(), f / 2);
            quaternion QQ = this;
            QQ = t * (QQ - s) * t.conj();
            QQ += s;

            return QQ;
        }

        //3 points and line
        //p1,p2 define line
        // nout coincident - return null
        public quaternion IsCoincidentWithLine(quaternion lP1, quaternion lP2)
        {
            quaternion rez = (new quaternion(q[0], q[1], q[2], q[3]) - lP1) / (lP2 - lP1);
            return (rez.absV() < Constants.zero_dist) ? rez : null;
        }

        //p1 - start na otse4kata, p2 - end of segment
        // nout internal - return null
        public quaternion IsInternalForSegment(quaternion lP1, quaternion lP2)
        {
            quaternion rez = (new quaternion(q[0], q[1], q[2], q[3]) - lP1) / (lP2 - lP1);
            return (rez.absV() < Constants.zero_dist && rez.real() > Constants.zero_dist && rez.real() < (1.0 - Constants.zero_dist)) ? rez : null;
        }

        public string GetString(bool quater = false )
        {
            if(quater)
                return string.Format("{0},{1},{2},{3}",q[0],q[1],q[2],q[3]);
            else
                return string.Format("{0},{1},{2}", q[1], q[2], q[3]);
        }
    }
    public class plane
    {
        //-- members ------
        quaternion n;
        double D;

        quaternion[] bak=null;
        //-- constructors ---
        public plane()
        {
            n = new quaternion();
            D = 0.0;
            bak = null;
        }
        public plane(plane pl)
        {
            D = pl.D;
            n = new quaternion(0.0, pl.n.imag());
            bak = pl.bak;
        }
        public plane(quaternion q1, quaternion q2, quaternion q3)//равнина през 3 точки 
        {
            double A = (new matrix(3, new double[] { q1.GetY(), q1.GetZ(), 1, q2.GetY(), q2.GetZ(), 1, q3.GetY(), q3.GetZ(), 1 })).Det();
            double B = -(new matrix(3, new double[] { q1.GetX(), q1.GetZ(), 1, q2.GetX(), q2.GetZ(), 1, q3.GetX(), q3.GetZ(), 1 })).Det();
            double C = (new matrix(3, new double[] { q1.GetX(), q1.GetY(), 1, q2.GetX(), q2.GetY(), 1, q3.GetX(), q3.GetY(), 1 })).Det();
            D = -(new matrix(3, new double[] { q1.GetX(), q1.GetY(), q1.GetZ(), q2.GetX(), q2.GetY(), q2.GetZ(), q3.GetX(), q3.GetY(), q3.GetZ() })).Det();
            n = new quaternion(0.0, A, B, C);
            bak = new quaternion[3];
            bak[0] = q1; bak[1] = q2; bak[2] = q3;
        }
        public plane(quaternion normal, quaternion point)
        {
            n = new quaternion(0, normal.GetX(), normal.GetY(), normal.GetZ());
            D = -normal.GetX() * point.GetX() - normal.GetY() * point.GetY() - normal.GetZ() * point.GetZ();
            bak = new quaternion[3];
            bak = this.GetThreePoints();
            bak[0] = point;
        }
        public plane(double aA, double bB, double cC)
        {
            n = new quaternion(0.0, aA, bB, cC);
            D = 0.0;
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
        public quaternion GetNormal() { return n; }
        public quaternion[] GetThreePoints(double k = 100.0)
        {
            quaternion[] rez = new quaternion[3];

            quaternion xOrt = new quaternion(0, 1, 0, 0);
            quaternion yOrt = new quaternion(0, 0, 1, 0);

            quaternion r1 = n / xOrt;
            quaternion r2 = n / yOrt;

            quaternion r3 = (r1.absV() > r2.absV()) ? r1 : r2;
            r3 = new quaternion(0, r3.GetX(), r3.GetY(), r3.GetZ());

            UCS ucs = new UCS(new quaternion(), n, r3);

            rez[0] = new quaternion();
            rez[1] = ucs.ToACS(new quaternion(0, 0, 0, k));
            rez[2] = ucs.ToACS(new quaternion(0, 0, k, 0));

            return rez;
        }      
        public quaternion[] GetThreeBackPoints()
        {
            return bak;
        }
        public quaternion IntersectWithVector(quaternion e, quaternion s)
        {
            quaternion v = new quaternion(e - s);
            double d = (-n.GetX() * s.GetX() - n.GetY() * s.GetY() - n.GetZ() * s.GetZ() - D) / (n.GetX() * v.GetX() + n.GetY() * v.GetY() + n.GetZ() * v.GetZ());
            quaternion rez = new quaternion(0, d * v.GetX() + s.GetX(), d * v.GetY() + s.GetY(), d * v.GetZ() + s.GetZ());
            if (((double.IsInfinity(Math.Abs(rez.GetX())))) || ((double.IsInfinity(Math.Abs(rez.GetY())))) || ((double.IsInfinity(Math.Abs(rez.GetZ())))) ||
                double.IsNaN(rez.GetX()) || double.IsNaN(rez.GetY()) || double.IsNaN(rez.GetZ()))
            {              
                rez = new quaternion(-1, 0, 0, 0);
            }
            return rez;
        }
        
        public quaternion[] IntersectWithPlane(quaternion q1, quaternion q2, quaternion q3)
        {
            return new quaternion[2] { this.IntersectWithVector(q1, q2), this.IntersectWithVector(q1, q3) };
        }
        public quaternion[] IntersectWithPlane(plane pl)
        {            
            quaternion[] z = pl.GetThreeBackPoints();
            UCS ucs = new UCS(z[0],z[1], z[2]);
            quaternion q1 = ucs.ToACS(new quaternion());
            quaternion q2 = ucs.ToACS(new quaternion(0,1000,0,0));
            quaternion q3 = ucs.ToACS(new quaternion(0, 0, 1000, 0));
            quaternion q4 = ucs.ToACS(new quaternion(0, 1000, 1000, 0));
            return new quaternion[2] { this.IntersectWithVector(q1, q2),
                this.IntersectWithVector(q3,q4) };
        }
        
        public double dist()
        {
            return D / Math.Sqrt(n.GetX() * n.GetX() + n.GetY() * n.GetY() + n.GetZ() * n.GetZ());
        }
        public double dist(double X, double Y, double Z)//връща разстоянието до точка M(X,Y,Z)
        {
            return (n.GetX() * X + n.GetY() * Y + n.GetZ() * Z + D) / Math.Sqrt(n.GetX() * n.GetX() + n.GetY() * n.GetY() + n.GetZ() * n.GetZ());
        }
        public double dist(quaternion Q)//връща разстоянието до точка Q
        {
            return (n.GetX() * Q.GetX() + n.GetY() * Q.GetY() + n.GetZ() * Q.GetZ() + D) /
                Math.Sqrt(n.GetX() * n.GetX() + n.GetY() * n.GetY() + n.GetZ() * n.GetZ());
        }

    }
    public class UCS
    {
        //--- members
        public matrix ang = new matrix(4);
        public quaternion o;// origin

        //--constructors
        public UCS()
        {
        }
        public UCS(UCS ucs)
        {
            ang = ucs.ang;
            o = ucs.o;
        }
        public UCS(quaternion O, quaternion Xort, quaternion Yort)
        {
            o = O;

            quaternion I = new quaternion(0.0, (Xort - O).imag());
            quaternion K = new quaternion(0.0, (I * (Yort - O)).imag());
            quaternion J = new quaternion(0.0, (K * I).imag());


            ang.SetAt(1, 1, I.cosToX());
            ang.SetAt(1, 2, J.cosToX());
            ang.SetAt(1, 3, K.cosToX());
            ang.SetAt(2, 1, I.cosToY());
            ang.SetAt(2, 2, J.cosToY());
            ang.SetAt(2, 3, K.cosToY());
            ang.SetAt(3, 1, I.cosToZ());
            ang.SetAt(3, 2, J.cosToZ());
            ang.SetAt(3, 3, K.cosToZ());
        }
        public static explicit operator Matrix3d(UCS ucs)
        // Matrix3d mat = (Matrix3d)ucs;
        {
            return new Matrix3d(ucs.GetAutoCAD_Matrix3d());
        }

        // --- function --
        public static double AngleBetweenZaxes(UCS ucs1, UCS ucs2)
        {
            double ang = 0;
            quaternion q1 = new quaternion(ucs1.ToACS(new quaternion(0, 0, 0, 100)) - ucs1.ToACS(new quaternion(0, 0, 0, 0)));
            quaternion q2 = new quaternion(ucs2.ToACS(new quaternion(0, 0, 0, 100)) - ucs2.ToACS(new quaternion(0, 0, 0, 0)));
            ang = q1.angTo(q2);

            return ang;
        }
        public quaternion ToACS(quaternion Q)
        {
            return new quaternion((ang * Q.ToMatrixC() + o.ToMatrixC()).column(0));
        }
        public quaternion FromACS(quaternion Q)
        {
            matrix ang1 = ang;
            return new quaternion((ang1.Tr() * (Q - o).ToMatrixC()).column(0));
        }
        public double[] GetAutoCAD_Matrix3d()//***
       // Matrix3d acMat3d = new Matrix3d(ucs.GetAutoCAD_Matrix3d());
        {
            return new double[]{ang.GetAt(1, 1), ang.GetAt(1, 2), ang.GetAt(1, 3), o.GetX(),
                                ang.GetAt(2, 1), ang.GetAt(2, 2), ang.GetAt(2, 3), o.GetY(),
                                ang.GetAt(3, 1), ang.GetAt(3, 2), ang.GetAt(3, 3), o.GetZ(),
                                0.0,                 0.0,                 0.0,          1.0};
        }
        public static double[] GetScaleAutoCAD_Matrix3d(double k, quaternion B /*base*/)
        {
            return new double[]{ k  ,  0.0 ,  0.0 ,  -B.GetX()*(k-1.0),
                                 0.0,     k,   0.0,  -B.GetY()*(k-1.0),
                                 0.0,   0.0,     k,  -B.GetZ()*(k-1.0),
                                 0.0,   0.0,   0.0,  1.0};;
        }
    }
    public class UCS_2D//клас за равнинна координатна система
    {
        //--- members
        public complex o;//радиус вектора от началото на абсолютната
        double ang;// ъгъл на завъртане спрямо оста X
        //--constructors
        public UCS_2D()
        {
            o = new complex(0, 0);
            ang = 0;
        }
        public UCS_2D(UCS_2D u)
        {
            o = u.o;
            ang = u.ang;
        }//копиращ конструктор
        public UCS_2D(complex rv, double ANG)
        {
            o = rv;
            ang = ANG;
        }
        public UCS_2D(double A, double B, double ANG)
        {
            o = new complex(A, B);
            ang = ANG;
        }

        // --- function --
        public complex GetComplex()
        {
            return o;
        }
        public complex GetRV()
        {
            return o;
        }//Връща радиус вектора от абсолютното начоало(0,0)
        //до начлота на потрбителската координатна система
        public double GetAng()
        {
            return ang;
        }//връща ъгаъла на завъртане на потрбителската спрямо абсолютната
        public void SetAng(double ANG)
        {
            ang = ANG;
        }
        public void SetRV(complex RV)
        {
            o = RV;
        }
        public double GetA()
        {
            return o.x;
        }//Разстояние до Origin на UCS по Х в ACS
        public double GetB()
        {
            return o.y;
        }//Разстояние до Origin на UCS по Y в ACS
        public complex ToACS(complex p)
        {
            return p * complex.polar(1.0, ang) + o;
        }//Преобразува координатите на p в абсолютни координати
        public complex FromACS(complex p)
        {
            return (p - o) / (complex.polar(1.0, ang));
        }//Преобразува координатите на p от ACS в относителни координати
    }
    public class line2d
    {
        //---members--------------
        public double A, B, C;
        //---constructors---------
        public line2d()
        {
            A = 0;
            B = 0;
            C = 0;
        }
        public line2d(line2d l)
        {
            A = l.A;
            B = l.B;
            C = l.C;
        }
        public line2d(line2d l, double delta)
        // |L| - разстоянието от (0,0) до правата
        //delta > 0 = |L| нараства ; delta < 0 = |L| намалявa
        {
            double k = (l.C == 0) ? 1 : (l.C / Math.Abs(l.C));
            C = Math.Abs(l.C) + delta * Math.Sqrt(l.A * l.A + l.B * l.B);
            C *= k;
            A = l.A;
            B = l.B;
        }
        public line2d(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }
        public line2d(complex z1, complex z2)
        {
            A = z2.imag() - z1.imag();
            B = z1.real() - z2.real();
            C = -z1.real() * A - z1.imag() * B;
        }
        public line2d(quaternion q1, quaternion q2)
        {
            complex z1 = new complex(q1.GetX(), q1.GetY());
            complex z2 = new complex(q2.GetX(), q2.GetY());

            A = z2.imag() - z1.imag();
            B = z1.real() - z2.real();
            C = -z1.real() * A - z1.imag() * B;
        }

        //---functions---------------------------
        public int PositionOfТhePointToLineSign(complex c)
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
        public double PositionOfТhePointToLine(complex c)
        {
            return A * c.real() + B * c.imag() + C;
        }
        public complex IntersectWitch(line2d l)
        {
            double x = (-C * l.B + B * l.C) / (A * l.B - B * l.A);
            double y = (-A * l.C + C * l.A) / (A * l.B - B * l.A);

            return new complex(x, y);
        }
        public complex IntersectWitch(complex z1, complex z2)
        {
            return new complex(IntersectWitch(new line2d(z1, z2)));
        }
        public complex IntersectWitch(quaternion q1, quaternion q2)
        {
            return new complex(IntersectWitch(new line2d(new complex(q1.GetX(), q1.GetY()), new complex(q2.GetX(), q2.GetY()))));
        }
        public static bool IsParalel(line2d l1, line2d l2)
        {
            bool rez = false;
            complex c = (new complex(l1.A, l1.B)) / (new complex(l2.A, l2.B));
            c.x = 0.0;
            if (c.norm() < 0.00000001)
            {
                rez = true;
            }
            return rez;
        }

    }

}
