using System.Drawing.Printing;
using KojtoCAD.GraphicItems.Interfaces;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
#endif

namespace KojtoCAD.GraphicItems
{
    class DefaultDrawingFrameContourFactory : IDrawingFrameContourFactory
    {
        public Polyline CreateOuterContour(PaperSize orientedPaperSize, Point3d insertionPoint)
        {
            Point3d ofLowerLeftPt = new Point3d(insertionPoint.X, insertionPoint.Y, 0);
            Point3d ofLowerRightPt = new Point3d(insertionPoint.X + orientedPaperSize.Width, insertionPoint.Y, 0);
            Point3d ofUpperRightPt = new Point3d(insertionPoint.X + orientedPaperSize.Width, insertionPoint.Y + orientedPaperSize.Height, 0);
            Point3d ofUpperLeftPt = new Point3d(insertionPoint.X, insertionPoint.Y + orientedPaperSize.Height, 0);

            Point3dCollection outerContourPoints = new Point3dCollection();
            outerContourPoints.Add(ofLowerLeftPt);
            outerContourPoints.Add(ofLowerRightPt);
            outerContourPoints.Add(ofUpperRightPt);
            outerContourPoints.Add(ofUpperLeftPt);

            var outerContour = new Polyline(outerContourPoints.Count);
            for (short i = 0; i < outerContourPoints.Count; i++)
            {
                outerContour.AddVertexAt(i, new Point2d(outerContourPoints[i].X, outerContourPoints[i].Y), 0.0, 0.0, 0.0);
            }
            outerContour.Closed = true;
            return outerContour;
        }

        public Polyline CreateInnerContour(PaperSize orientedPaperSize, Point3d insertionPoint)
        {
            Point3d ifLowerLeftPt = new Point3d(insertionPoint.X + 20, insertionPoint.Y + 5, 0);
            Point3d ifLowerRightPt = new Point3d(insertionPoint.X + orientedPaperSize.Width - 5, insertionPoint.Y + 5, 0);
            Point3d ifUpperRightPt = new Point3d(insertionPoint.X + orientedPaperSize.Width - 5, insertionPoint.Y + orientedPaperSize.Height - 5, 0);
            Point3d ifUpperLeftPt = new Point3d(insertionPoint.X + 20, insertionPoint.Y + orientedPaperSize.Height - 5, 0);

            Point3dCollection innerContourPoints = new Point3dCollection();
            innerContourPoints.Add(ifLowerLeftPt);
            innerContourPoints.Add(ifLowerRightPt);
            innerContourPoints.Add(ifUpperRightPt);
            innerContourPoints.Add(ifUpperLeftPt);

            var innerContour = new Polyline(innerContourPoints.Count);
            for (short i = 0; i < innerContourPoints.Count; i++)
            {
                innerContour.AddVertexAt(i, new Point2d(innerContourPoints[i].X, innerContourPoints[i].Y), 0.0, 0.0, 0.0);
            }
            innerContour.Closed = true;

            return innerContour;
        }
    }
}
