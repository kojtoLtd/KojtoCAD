using System.Drawing.Printing;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
#endif

namespace KojtoCAD.GraphicItems.Interfaces
{
    interface IDrawingFrameContourFactory
    {
        Polyline CreateOuterContour(PaperSize orientedPaperSize, Point3d insertionPoint);
        Polyline CreateInnerContour(PaperSize orientedPaperSize, Point3d insertionPoint);
    }
}
