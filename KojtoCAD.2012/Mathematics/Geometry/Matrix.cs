// -----------------------------------------------------------------------
// <copyright file="Matrix.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace KojtoCAD.Mathematics.Geometry
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Matrix//double Matrix
    {
        //--- members
        public int r;
        public int c;
        public double[] arr;// a11,a12,a13,a21,a22,a23,.......
        //--constructors
        public Matrix()
        {
            r = 0;
            c = 0;
            arr = new double[0];
        }
        public Matrix(int R, int C)
        {
            r = R;
            c = C;
            arr = new double[R * C];
        }
        public Matrix(int R)/*квадратна*/{
            r = R;
            c = R;
            arr = new double[R * R];
        }
        public Matrix(Matrix m)
        {
            r = m.r;
            c = m.c;
            arr = new double[m.r * m.c];
            for (int i = 0; i < m.arr.Length; i++)
            {
                arr[i] = m.arr[i];
            }
        }//copy constructor
        public Matrix(int R, int C, Matrix m)//подматрица на m чрез премахване на ред R и стълб C
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
        public Matrix(int R, int C, double[] ARR)
        {
            r = R;
            c = C;
            arr = new double[ARR.Length];
            for (int i = 0; i < R * C; i++)
            {
                arr[i] = ARR[i];
            }
        }
        public Matrix(int R, double[] ARR)//квадратна матрица
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
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            Matrix m = new Matrix(m1.r, m2.c);
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
        public static Matrix operator *(Matrix m1, double d)
        {
            Matrix m = new Matrix(m1.r, m1.c);
            for (int i = 0; i < m1.arr.Length; i++)
            {
                m.arr[i] = m1.arr[i] * d;
            }
            return m;
        }
        public static Matrix operator *(double d, Matrix m1)
        {
            Matrix m = new Matrix(m1.r, m1.c);
            for (int i = 0; i < m1.arr.Length; i++)
            {
                m.arr[i] = m1.arr[i] * d;
            }
            return m;
        }
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            Matrix m = new Matrix(m1.r, m1.c);
            for (int i = 0; i < m1.arr.Length; i++)
            {
                m.arr[i] = m1.arr[i] + m2.arr[i];
            }
            return m;
        }
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            Matrix m = new Matrix(m1.r, m1.c);
            for (int i = 0; i < m1.arr.Length; i++)
            {
                m.arr[i] = m1.arr[i] - m2.arr[i];
            }
            return m;
        }
        public static Matrix operator /(Matrix m1, double d)
        {
            Matrix m = new Matrix(m1.r, m1.c);
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
        public double GetXi(int i, Matrix m)
        {
            double res = Det();
            Matrix mm = ChangeColumn(i, 0, m);
            return mm.Det() / res;
        }
        /// <summary>
        /// Re6avane sistema lineini urawneniq
        /// </summary>
        /// <param name="i">matrica stalb na svobodnite koeficienti</param>
        /// <returns></returns>
        public Matrix GetMatXi(Matrix m)
        {
            Matrix mmm = new Matrix(r, 1);
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
        public Matrix Mat_1()
        {
            Matrix m = new Matrix(r);
            double[] arr = new double[r];
            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < r; j++)
                {
                    arr[j] = (i == j) ? 1 : 0;
                }
                m = m.ChangeColumn(i, 0, GetMatXi(new Matrix(r, 1, arr)));
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
        public Matrix ChangeColumn(int cN, int c1, Matrix mm)
        {
            Matrix m = new Matrix(r, c);
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
        public Matrix Tr()
        {
            Matrix m = new Matrix(c, r);
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
                    res += Math.Pow(-1, 2 + i) * arr[i] * (new Matrix(0, i, this).Det());
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
        public Matrix column(int C)
        {
            Matrix m = new Matrix(r, 1);
            for (int i = 0; i < r; i++)
            {
                m.arr[i] = arr[c * i + C];
            }
            return m;
        }
        public Matrix row(int R)
        {
            Matrix m = new Matrix(1, c);
            for (int i = 0; i < c; i++)
            {
                m.arr[i] = arr[c * R + i];
            }
            return m;
        }
    }
}
