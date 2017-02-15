// -----------------------------------------------------------------------
// <copyright file="UCS2d.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace KojtoCAD.Mathematics.Geometry
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class UCS2d//клас за равнинна координатна система
    {
        //--- members
        public Complex o;//радиус вектора от началото на абсолютната
        double ang;// ъгъл на завъртане спрямо оста X
        //--constructors
        public UCS2d()
        {
            o = new Complex(0, 0);
            ang = 0;
        }
        public UCS2d(UCS2d u)
        {
            o = u.o;
            ang = u.ang;
        }//копиращ конструктор
        public UCS2d(Complex rv, double ANG)
        {
            o = rv;
            ang = ANG;
        }
        public UCS2d(double A, double B, double ANG)
        {
            o = new Complex(A, B);
            ang = ANG;
        }

        // --- function --
        public Complex GetComplex()
        {
            return o;
        }
        public Complex GetRV()
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
        public void SetRV(Complex RV)
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
        public Complex ToACS(Complex p)
        {
            return p * Complex.polar(1.0, ang) + o;
        }//Преобразува координатите на p в абсолютни координати
        public Complex FromACS(Complex p)
        {
            return (p - o) / (Complex.polar(1.0, ang));
        }//Преобразува координатите на p от ACS в относителни координати
    }
}
