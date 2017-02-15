// -----------------------------------------------------------------------
// <copyright file="UCS.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace KojtoCAD.Mathematics.Geometry
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class UCS
    {
        //--- members
        public Matrix ang = new Matrix(4);
        public Quaternion o;// origin

        //--constructors
        public UCS()
        {
        }
        public UCS(UCS ucs)
        {
            ang = ucs.ang;
            o = ucs.o;
        }
        public UCS(Quaternion O, Quaternion Xort, Quaternion Yort)
        {
            o = O;

            Quaternion I = new Quaternion(0.0, (Xort - O).imag());
            Quaternion K = new Quaternion(0.0, (I * (Yort - O)).imag());
            Quaternion J = new Quaternion(0.0, (K * I).imag());


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

        // --- function --
        public static double AngleBetweenZaxes(UCS ucs1, UCS ucs2)
        {
            double ang = 0;
            Quaternion q1 = new Quaternion(ucs1.ToACS(new Quaternion(0, 0, 0, 100)) - ucs1.ToACS(new Quaternion(0, 0, 0, 0)));
            Quaternion q2 = new Quaternion(ucs2.ToACS(new Quaternion(0, 0, 0, 100)) - ucs2.ToACS(new Quaternion(0, 0, 0, 0)));
            ang = q1.angTo(q2);

            return ang;
        }
        public Quaternion ToACS(Quaternion Q)
        {
            return new Quaternion((ang * Q.ToMatrixC() + o.ToMatrixC()).column(0));
        }
        public Quaternion FromACS(Quaternion Q)
        {
            Matrix ang1 = ang;
            return new Quaternion((ang1.Tr() * (Q - o).ToMatrixC()).column(0));
        }
    }
}
