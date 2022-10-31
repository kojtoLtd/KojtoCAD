#if !bcad
using Autodesk.AutoCAD.Geometry;
#else
using Teigha.Geometry;
#endif

namespace KojtoCAD.LayoutCommands
{
    public class ViewportInModelSpace
    {
        public ViewportInModelSpace(Point3d bottomLeft, Point3d topRight)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        public Point3d BottomLeft { get; }
        public Point3d TopRight { get; }
    }
}
