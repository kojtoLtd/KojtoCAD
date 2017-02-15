using System.Collections.Generic;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;

#else
using Teigha.DatabaseServices;

#endif

namespace KojtoCAD.KojtoCAD3D
{
    public static class ContextVariablesProvider
    {
        public static Containers Container = new Containers();
        public static Dictionary<Handle, Pair<Pair<quaternion, quaternion>, Pair<quaternion, quaternion>>> BuffDictionary =
           new Dictionary<Handle, Pair<Pair<quaternion, quaternion>, Pair<quaternion, quaternion>>>();
    }
}
