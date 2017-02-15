using System;

namespace KojtoCAD.Mathematics.Algebra
{
    public class FloatsComparer
    {
        /// <summary>
        /// Compares two floating point numbers with a given tolerance.
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="exp2"></param>
        /// <param name="difference"></param>
        /// <returns></returns>
        public static bool IsEqual(double exp1, double exp2, double difference)
        {
            bool result = false;
            double res = Math.Abs((exp1 - exp2));
            if ((res <= difference))
            {
                result = true;
            }
            return result;
        }
    }
}
