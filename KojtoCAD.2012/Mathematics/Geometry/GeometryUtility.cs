using System;
using System.Collections.Generic;
using System.Linq;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Surface = Autodesk.AutoCAD.DatabaseServices.Surface;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.EditorInput;
using Surface = Teigha.DatabaseServices.Surface;
using NurbSurface = Teigha.DatabaseServices.NurbSurface;
#endif

#if acad2013
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
#if acad2012
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#endif
#if bcad
using Application = Bricscad.ApplicationServices.Application;
#endif

using Exception = System.Exception;

namespace KojtoCAD.Mathematics.Geometry
{
    public enum ArcSide
    {
        ArcSideInside,

        ArcSideOutside,

        ArcSideMiddle
    };

    public static class GeometryUtility
    {
        public static Point3d GetPlanePolarPoint(Point3d aBasepoint, double aAngle, double aDistance)
        {
            return new Point3d(
                aBasepoint.X + (aDistance*Math.Cos(aAngle)),
                aBasepoint.Y + (aDistance*Math.Sin(aAngle)),
                aBasepoint.Z);
        }

        public static Point2d GetPlanePolarPoint(Point2d aBasepoint, double aAngle, double aDistance)
        {
            return new Point2d(
                aBasepoint.X + (aDistance*Math.Cos(aAngle)), aBasepoint.Y + (aDistance*Math.Sin(aAngle)));
        }

        public static Point3d GetSphericalPolarPoint(Point3d aBasepoint, double aAngle, double aDistance)
        {
            return new Point3d(
                aBasepoint.X + (aDistance*Math.Cos(aAngle)),
                aBasepoint.Y + (aDistance*Math.Sin(aAngle)),
                aBasepoint.Z);
        }

        /// <summary>
        /// Gets the angle between the pt1-pt2 vector and X-Axis
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns>angle</returns>
        public static double GetAngleFromXAxis(Point3d pt1, Point3d pt2)
        {
            var ucsMatrix = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
            var ucs = ucsMatrix.CoordinateSystem3d;
            var vec = pt2.TransformBy(GetTransforMatrixToWcs()) - pt1.TransformBy(GetTransforMatrixToWcs());
            var vectorComplex = new Complex(vec.X, vec.Y);
            var angle = (vectorComplex/new Complex(ucs.Xaxis.X, ucs.Xaxis.Y)).arg();

            return angle;
        }

        /// <summary>
        /// Converts double (degrees) to radians for geometric calculations
        /// </summary>
        /// <param name="angleInDegrees"></param>
        /// <returns></returns>
        public static double DoubleToRadians(double angleInDegrees)
        {
            return angleInDegrees*(Math.PI/180.0);
        }

        /// <summary>
        /// Converts radians to double (degrees) for geometric calculations
        /// </summary>
        /// <param name="angleInRadians"></param>
        /// <returns></returns>
        public static double RadiansToDouble(double angleInRadians)
        {
            return angleInRadians*(180.0/Math.PI);
        }

        /// <summary>
        /// Get the bulge of an arc
        /// </summary>
        /// <param name="aArc"></param>
        /// <returns>The bulge height</returns>
        public static double GetBulge(Arc aArc)
        {
            var lenght = aArc.StartPoint.DistanceTo(aArc.EndPoint);
            var radius = aArc.Radius;
            var temp = lenght/2/radius;
            temp = Math.Atan(temp/Math.Sqrt(-temp*temp + 1)); //temp = 1/2 ot centralniq agal v radiani

            var bulge = Math.Tan(temp/2);
            return bulge;
        }

        /// <summary>
        /// This func gets arc center point, arc radius and side point,
        /// measures if the side point is on the inside or outside of the arc
        /// and returns a value indicating where the SidePoint lies - outside or inside the arc perimeter.
        /// Circle is an Arc with start angle of 0 and end angle of 360, so you can use the same func for both.
        /// </summary>
        /// <param name="aArcCenterPt"></param>
        /// <param name="aArcRadius"></param>
        /// <param name="aSidePoint"></param>
        /// <returns>enum ArcSide</returns>
        public static ArcSide SideOfPointToArc(Point3d aArcCenterPt, double aArcRadius, Point3d aSidePoint)
        {
            if (!aSidePoint.IsEqualTo(new Point3d(0, 0, 0)))
            {
                var proportion = aArcCenterPt.DistanceTo(aSidePoint)/aArcRadius;
                if (proportion > 1)
                {
                    return ArcSide.ArcSideOutside;
                }
                else
                {
                    return ArcSide.ArcSideInside;
                }
            }
            else
            {
                return ArcSide.ArcSideMiddle;
            }
        }

        /// <summary>
        /// Calculates the area (called Double Meridian) Distance. 
        /// If the sum is positive the polyline is clockwise 
        /// and the return value is set to true.
        /// </summary>
        /// <param name="aPoly"></param>
        /// <returns></returns>
        public static bool PolyIsClockwise(Polyline aPoly)
        {
            var count = aPoly.NumberOfVertices - 1;
            var sum = 0.0;
            Point2d p1, p2 = new Point2d(0, 0);
            for (var i = 0; i < count; i++)
            {
                p1 = aPoly.GetPoint2dAt(i);
                p2 = aPoly.GetPoint2dAt(i + 1);
                sum = sum + ((p1.X*p2.Y) - (p1.Y*p2.X));
            }
            p1 = p2;
            p2 = aPoly.GetPoint2dAt(0);
            sum = sum + ((p1.X*p2.Y) - (p1.Y*p2.X));
            return sum > 1;
        }

        /// <summary>
        /// Point3d is returned as "x.xxx,y.yyy,z.zzz"
        /// </summary>
        /// <param name="aPoint"></param>
        /// <returns></returns>
        public static string Point3DToPoint2DToString(Point3d aPoint)
        {
            var mX = Convert.ToString(aPoint.X);
            mX = mX.Replace(',', '.');
            var mY = Convert.ToString(aPoint.Y);
            mY = mY.Replace(',', '.');
            var mResult = mX + "," + mY;
            return mResult;
        }

        /// <summary>
        /// Extract the points of a Polyline3d
        /// </summary>
        /// <param name="aPolyline"></param>
        /// <returns>Point3dCollection</returns>
        public static Point3dCollection GetPointsFromPolyline(Polyline3d aPolyline)
        {
            var polyPts = new Point3dCollection();
            var polySegments = new DBObjectCollection();
            aPolyline.Explode(polySegments);

            // Cycle through the segments
            for (var i = 0; i <= polySegments.Count - 1; i++)
            {
                // if segment is Line
                var line = polySegments[i] as Line;
                if (line != null)
                {
                    var tempLine = line;
                    polyPts.Add(new Point3d(tempLine.StartPoint.X, tempLine.StartPoint.Y, tempLine.StartPoint.Z));
                    polyPts.Add(new Point3d(tempLine.EndPoint.X, tempLine.EndPoint.Y, tempLine.EndPoint.Z));
                }
                // else if segment is Arc
                else
                {
                    var arc = polySegments[i] as Arc;
                    if (arc != null)
                    {
                        var tempArc = arc;
                        polyPts.Add(new Point3d(tempArc.StartPoint.X, tempArc.StartPoint.Y, tempArc.StartPoint.Z));
                        polyPts.Add(new Point3d(tempArc.EndPoint.X, tempArc.EndPoint.Y, tempArc.EndPoint.Z));
                    }
                    // else is something else - this must never happen
                    else
                    {
                        throw new Exception(
                            "KojtoCAD Error - Can not find the type of this polyline segment. Not Arc nor Line.");
                    }
                }
            }
            RemoveDuplicate(polyPts);
            return polyPts;
        }

        /// <summary>
        /// Extract the points of a Polyline2d
        /// </summary>
        /// <param name="aPolyline"></param>
        /// <returns>Point2dCollection</returns>
        public static Point2dCollection GetPointsFromPolyline(Polyline2d aPolyline)
        {
            var polyPts = new Point2dCollection();
            var polySegments = new DBObjectCollection();
            aPolyline.Explode(polySegments);

            // Cycle through the segments
            for (var i = 0; i <= polySegments.Count - 1; i++)
            {
                // if segment is Line
                var line = polySegments[i] as Line;
                if (line != null)
                {
                    var tempLine = line;
                    polyPts.Add(new Point2d(tempLine.StartPoint.X, tempLine.StartPoint.Y));
                    polyPts.Add(new Point2d(tempLine.EndPoint.X, tempLine.EndPoint.Y));
                }
                // else if segment is Arc
                else
                {
                    var arc = polySegments[i] as Arc;
                    if (arc != null)
                    {
                        var tempArc = arc;
                        polyPts.Add(new Point2d(tempArc.StartPoint.X, tempArc.StartPoint.Y));
                        polyPts.Add(new Point2d(tempArc.EndPoint.X, tempArc.EndPoint.Y));
                    }
                    // else is something else - this must never happen
                    else
                    {
                        throw new Exception(
                            "KojtoCAD Error - Can not find the type of this polyline segment. Not Arc nor Line.");
                    }
                }
            }
            RemoveDuplicate(polyPts);
            return polyPts;
        }

        /// <summary>
        /// Extract the points of a Polyline
        /// </summary>
        /// <param name="aPolyline"></param>
        /// <returns>Point3dCollection</returns>
        public static Point3dCollection GetPointsFromPolyline(Polyline aPolyline)
        {
            var polyPts = new Point3dCollection();
            var polySegments = new DBObjectCollection();
            aPolyline.Explode(polySegments);

            // Cycle through the segments
            for (var i = 0; i <= polySegments.Count - 1; i++)
            {
                // if segment is Line
                var line = polySegments[i] as Line;
                if (line != null)
                {
                    var tempLine = line;
                    polyPts.Add(new Point3d(tempLine.StartPoint.X, tempLine.StartPoint.Y, tempLine.StartPoint.Z));
                    polyPts.Add(new Point3d(tempLine.EndPoint.X, tempLine.EndPoint.Y, tempLine.EndPoint.Z));
                }
                // else if segment is Arc
                else
                {
                    var arc = polySegments[i] as Arc;
                    if (arc != null)
                    {
                        var tempArc = arc;
                        polyPts.Add(new Point3d(tempArc.StartPoint.X, tempArc.StartPoint.Y, tempArc.StartPoint.Z));
                        polyPts.Add(new Point3d(tempArc.EndPoint.X, tempArc.EndPoint.Y, tempArc.EndPoint.Z));
                    }
                    // else is something else - this must never happen
                    else
                    {
                        throw new Exception(
                            "KojtoCAD Error - Can not find the type of this polyline segment. Not Arc nor Line.");
                    }
                }
            }
            RemoveDuplicate(polyPts);
            return polyPts;

        }

        public static UcsTableRecord CreateUcs(Point3d aOrigin, Point3d aXAxis, Point3d aYAxis, string aName)
        {
            //bool UpdateViewport = false;

            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var dblRotation = GetAngleFromXAxis(aOrigin, aXAxis); //*Math.PI / 180;
            var xAxis = new Vector3d(1*Math.Cos(dblRotation), 1*Math.Sin(dblRotation), 0);
            var yAxis = new Vector3d(1*Math.Sin(-1*dblRotation), 1*Math.Cos(dblRotation), 0);

            using (Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                try
                {
                    UcsTableRecord ucstr;
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        using (var ucst = (UcsTable) tr.GetObject(db.UcsTableId, OpenMode.ForWrite))
                        {
                            if (!ucst.Has(aName))
                            {
                                ucstr = new UcsTableRecord {Name = aName};

                                ucst.UpgradeOpen();
                                ucst.Add(ucstr);
                                tr.AddNewlyCreatedDBObject(ucstr, true);
                            }
                            else
                            {
                                ucstr = (UcsTableRecord) tr.GetObject(ucst[aName], OpenMode.ForWrite);
                            }

                            ucstr.Origin = aOrigin;
                            ucstr.XAxis = xAxis;
                            ucstr.YAxis = yAxis;
                            /*
              if (UpdateViewport)
              {
                //Debug.Print(Doc.Editor.CurrentViewportObjectId.ToString);
                ViewportTableRecord vport = (ViewportTableRecord)Tr.GetObject(Doc.Editor.CurrentViewportObjectId, OpenMode.ForWrite);
                vport.SetUcs(Ucstr.ObjectId);
                Doc.Editor.UpdateTiledViewportsFromDatabase();
              }
              */
                            tr.Commit();
                        }
                    }
                    return ucstr;
                }
                catch (Exception ex)
                {
                    ed.WriteMessage(ex.Message + "\n" + ex.StackTrace);
                    return new UcsTableRecord();
                }
            }
        }

        public static void ChangeUcs(Point3d aOrigin, Point3d aPointForXAxis)
        {
            //Point3d pt1 = new Point3d(10, 10, 0);
            //Point3d pt2 = new Point3d(50, 50, 0);
            var pt1 = aOrigin;
            var pt2 = aPointForXAxis;

            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            var zAxis = ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis;
            var xAxis = pt1.GetVectorTo(pt2).GetNormal();
            var yAxis = zAxis.CrossProduct(xAxis).GetNormal();

            var mat = Matrix3d.AlignCoordinateSystem(
                Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis, pt1, xAxis, yAxis, zAxis);
            ed.CurrentUserCoordinateSystem = mat;
        }

        public static Polyline ReversePolyVertices(Polyline aPoly)
        {
            var resPoly = new Polyline();
            for (var i = 0; i < aPoly.NumberOfVertices; i++)
            {
                resPoly.AddVertexAt(
                    aPoly.NumberOfVertices - i, aPoly.GetPoint2dAt(aPoly.NumberOfVertices - i), 0.0, 0.0, 0.0);
                i++;
            }
            return resPoly;
        }

        public static void RemoveDuplicate(Point2dCollection pts)
        {
            RemoveDuplicate(pts, Tolerance.Global);
        }

        public static void RemoveDuplicate(Point2dCollection pts, Tolerance tol)
        {
            var ptlst = pts.Cast<Point2d>().ToList();
            //ptlst.Sort((p1, p2) => p1.X.CompareTo(p2.X));
            for (var i = 0; i < ptlst.Count - 1; i++)
            {
                for (var j = i + 1; j < ptlst.Count;)
                {
                    if ((ptlst[j].X - ptlst[i].X) > tol.EqualPoint) break;
                    if (ptlst[i].IsEqualTo(ptlst[j], tol))
                    {
                        pts.Remove(ptlst[j]);
                        ptlst.RemoveAt(j);
                    }
                    else j++;
                }
            }
        }

        public static void RemoveDuplicate(Point3dCollection pts)
        {
            RemoveDuplicate(pts, Tolerance.Global);
        }

        public static void RemoveDuplicate(Point3dCollection pts, Tolerance tol)
        {
            var ptlst = new List<Point3d>();
            for (var i = 0; i < pts.Count; i++)
            {
                ptlst.Add(pts[i]);
            }
            //ptlst.Sort((p1, p2) => p1.X.CompareTo(p2.X));
            for (var i = 0; i < ptlst.Count - 1; i++)
            {
                for (var j = i + 1; j < ptlst.Count;)
                {
                    if ((ptlst[j].X - ptlst[i].X) > tol.EqualPoint) break;
                    if (ptlst[i].IsEqualTo(ptlst[j], tol))
                    {
                        pts.Remove(ptlst[j]);
                        ptlst.RemoveAt(j);
                    }
                    else j++;
                }
            }
        }

        public static DBObjectCollection ObjOffset(ObjectId aPolyId, double aOffset, Transaction tr)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            Application.DocumentManager.MdiActiveDocument.LockDocument(DocumentLockMode.AutoWrite, null, null, true);
            var mCurve = (Curve) tr.GetObject(aPolyId, OpenMode.ForWrite, false);
            var objCol = mCurve.GetOffsetCurves(aOffset);

            var bt = (BlockTable) tr.GetObject(db.BlockTableId, OpenMode.ForRead, false);
            var btr =
                (BlockTableRecord) tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false);

            foreach (Entity dbObj in objCol)
            {
                btr.AppendEntity(dbObj);
                tr.AddNewlyCreatedDBObject(dbObj, true);
            }


            return objCol;
        }

        public static int OffsetDirection(Entity originalEntity, Point3d directionPoint)
        {
            using (Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                var curve = (Curve) originalEntity;
                var closestPointTo = curve.GetClosestPointTo(directionPoint, Vector3d.ZAxis, false);
                var pln = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
                Point3d pointAlongObj;
                try
                {
                    pointAlongObj = curve.GetPointAtDist(curve.GetDistAtPoint(closestPointTo) + 1);
                }
                catch (Exception ex)
                {
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
                    pointAlongObj = curve.EndPoint;
                }
                var vecOnObj = pointAlongObj - closestPointTo;
                var vecToPoint = directionPoint - closestPointTo;

                var angAlongObj = vecOnObj.AngleOnPlane(pln);

                var ang = vecToPoint.AngleOnPlane(pln);
                if (ang < angAlongObj) ang += Math.PI*2;

                return angAlongObj + Math.PI < ang ? 1 : -1;
            }
        }

        public static int OffsetInDirection(ObjectId graphicObjectId, Point3d directionPoint)
        {
            using (var tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                var entity = (Entity) tr.GetObject(graphicObjectId, OpenMode.ForRead, false);
                return OffsetDirection(entity, directionPoint);
            }
        }

        public static Line DrawLine(Point3d st, Point3d end, ref Document doc, int colorIndex)
        {
            Line rez;
            using (var acTrans = doc.Database.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

                var acBlkTblRec =
                    (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var line = new Line(st, end) {ColorIndex = colorIndex};
                line.SetDatabaseDefaults();

                acBlkTblRec.AppendEntity(line);
                acTrans.AddNewlyCreatedDBObject(line, true);

                rez = line;

                acTrans.Commit();
            }
            return rez;
        }

        public static Line DrawLine(Point3d st, Point3d end, ref Document doc, int colorIndex, string layer)
        {
            Line rez;
            using (var acTrans = doc.Database.TransactionManager.StartTransaction())
            {
                var acBlkTbl = (BlockTable)acTrans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

                var acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var line = new Line(st, end);
                try
                {
                    if (colorIndex >= 0) line.ColorIndex = colorIndex;
                    if (layer != "") line.Layer = layer;

                }
                catch
                {
                }

                line.SetDatabaseDefaults();

                acBlkTblRec.AppendEntity(line);
                acTrans.AddNewlyCreatedDBObject(line, true);

                rez = line;

                acTrans.Commit();
            }
            return rez;
        }

        public static Matrix3d GetUcs(Point3d f, Point3d s, Point3d T, bool toWcSorToUCs)
        {

            Matrix3d UCS;

            var qF = new Quaternion(0, f.X, f.Y, f.Z);
            var qS = new Quaternion(0, s.X, s.Y, s.Z);
            var qT = new Quaternion(0, T.X, T.Y, T.Z);
            var ucs = new UCS(qF, qS, qT);

            var qX = ucs.ToACS(new Quaternion(0, 1, 0, 0)) - ucs.ToACS(new Quaternion(0, 0, 0, 0));
            var qY = ucs.ToACS(new Quaternion(0, 0, 1, 0)) - ucs.ToACS(new Quaternion(0, 0, 0, 0));
            var qZ = ucs.ToACS(new Quaternion(0, 0, 0, 1)) - ucs.ToACS(new Quaternion(0, 0, 0, 0));
            var o = ucs.ToACS(new Quaternion(0, 0, 0, 0));

            var x = new Vector3d(qX.GetX(), qX.GetY(), qX.GetZ());
            var y = new Vector3d(qY.GetX(), qY.GetY(), qY.GetZ());
            var z = new Vector3d(qZ.GetX(), qZ.GetY(), qZ.GetZ());
            var O = new Point3d(o.GetX(), o.GetY(), o.GetZ());


            if (toWcSorToUCs)
                UCS = Matrix3d.AlignCoordinateSystem(
                    Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis, O, x, y, z);
            else
                UCS = Matrix3d.AlignCoordinateSystem(
                    O, x, y, z, Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis);


            return UCS;
        }

        public static Matrix3d GetTransforMatrixToWcs()
        {
            var world = Matrix3d.AlignCoordinateSystem(
                Point3d.Origin,
                Vector3d.XAxis,
                Vector3d.YAxis,
                Vector3d.ZAxis,
                Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d
                    .Origin,
                Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d
                    .Xaxis,
                Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d
                    .Yaxis,
                Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d
                    .Zaxis);
            return world;
        }

        public static Matrix3d GetTransforMatrixToUcs()
        {
            var world = Matrix3d.AlignCoordinateSystem(
                Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d
                    .Origin,
                Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d
                    .Xaxis,
                Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d
                    .Yaxis,
                Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d
                    .Zaxis,
                Point3d.Origin,
                Vector3d.XAxis,
                Vector3d.YAxis,
                Vector3d.ZAxis);
            return world;
        }

        public static bool IsInternalPoint(ObjectId polyId, double pX, double pY, double pZ, ref Point3dCollection pts)
        {
            //ако е на границата (точката лежи върху полилинията) ще я покаже като вътрешна
            // В момента на посочване активна е UCS

            var rez = false;
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                var pl = (Polyline) acTrans.GetObject(polyId, OpenMode.ForWrite);

                var basePoint = new Point3d(pX, pY, pZ).TransformBy(acDoc.Editor.CurrentUserCoordinateSystem);
                var testVector = pl.GetPoint3dAt(0).GetVectorTo(pl.GetPoint3dAt(1));
                testVector.TransformBy(acDoc.Editor.CurrentUserCoordinateSystem);

                var r = new Ray
                {
                    BasePoint = basePoint,
                    SecondPoint =
                        (new Point3d(basePoint.X + testVector.X, basePoint.Y + testVector.Y, basePoint.Z + testVector.Z))
                };

                pts = new Point3dCollection();
                pl.IntersectWith(r, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);


                if ((pts.Count > 0) && (pts.Count%2 != 0))
                {
                    rez = true;
                }
            }
            return rez;
        }

        public static Point3dCollection DividePoly(ref Polyline poly, double delta)
        {
            var points = new Point3dCollection();
            var polyLength = poly.Length;
            var n = (int) (polyLength/delta);

            points.Add(poly.StartPoint);
            for (var i = 0; i < n; i++)
            {
                var pt = poly.GetPointAtDist(poly.Length*i/n);
                points.Add(pt);
            }
            points.Add(poly.EndPoint);

            return points;
        }

        public static Point3dCollection DivideCircle(ref Circle poly, double delta)
        {
            var points = new Point3dCollection();
            var polyLength = 2*poly.Radius*Math.PI;
            var n = (int) (polyLength/delta);

            points.Add(poly.StartPoint);
            for (var i = 0; i < n; i++)
            {
                var pt = poly.GetPointAtDist(polyLength*i/n);
                points.Add(pt);
            }
            points.Add(poly.EndPoint);

            return points;
        }

        public static Point3dCollection DivideArc(ref Arc poly, double delta)
        {
            var points = new Point3dCollection();
            var polyLength = poly.Length;
            var n = (int) (polyLength/delta);

            points.Add(poly.StartPoint);
            for (var i = 0; i < n; i++)
            {
                var pt = poly.GetPointAtDist(poly.Length*i/n);
                points.Add(pt);
            }
            points.Add(poly.EndPoint);

            return points;
        }

        public static Point3dCollection DivideLine(ref Line poly, double delta)
        {
            var points = new Point3dCollection();
            var polyLength = poly.Length;
            var n = (int) (polyLength/delta);

            points.Add(poly.StartPoint);
            for (var i = 0; i < n; i++)
            {
                var pt = poly.GetPointAtDist(poly.Length*i/n);
                points.Add(pt);
            }
            points.Add(poly.EndPoint);

            return points;
        }

        public static Point3d GetNearestPoint(Point3d p, ref Point3dCollection arr)
        {
            var rez = arr[0];
            var dist = p.DistanceTo(arr[0]);
            foreach (Point3d P in arr)
            {
                if (P.DistanceTo(p) < dist)
                {
                    dist = P.DistanceTo(p);
                    rez = P;
                }
            }
            return rez;
        }

        public static Point3d DistFromPointToSolid(
            ref Document doc, ref Solid3d solid, Point3d point, double start, double step, ref double off, bool line)
        {
            var rez = new Point3d();

            using (var acTrans = doc.Database.TransactionManager.StartTransaction())
            {
                var acBlkTbl = (BlockTable)acTrans.GetObject(doc.Database.BlockTableId, OpenMode.ForWrite);

                var acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var SOLID = (Solid3d) solid.Clone();
                SOLID.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(SOLID);
                acTrans.AddNewlyCreatedDBObject(SOLID, true);


                var startR = start <= 0 ? step : start;

                var sphere = new Solid3d();
                sphere.SetDatabaseDefaults();
                sphere.CreateSphere(startR);
                sphere.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo(point)));
                sphere.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(sphere);
                acTrans.AddNewlyCreatedDBObject(sphere, true);


                while (sphere.CheckInterference(solid) == false)
                {

                    try
                    {
                        sphere.OffsetBody(step);
                        off += step;
                    }
                    catch
                    {
                    }
                }

                if (line)
                {
                    var counter = 0;

                    sphere.BooleanOperation(BooleanOperationType.BoolIntersect, SOLID);
                    var dbo = new DBObjectCollection();
                    sphere.Explode(dbo);

                    var coll = new Point3dCollection();
                    var curvesColl = new DBObjectCollection();

                    var ent = dbo[0] as Entity;
                    if (ent.GetType().ToString().IndexOf("Surface") > 0)
                    {
                        var surf =
                            ent as Surface;
                        var ns = surf.ConvertToNurbSurface();
                        foreach (var nurb in ns)
                        {
                            double ustart = nurb.UKnots.StartParameter,
                                uend = nurb.UKnots.EndParameter,
                                uinc = (uend - ustart)/nurb.UKnots.Count,
                                vstart = nurb.VKnots.StartParameter,
                                vend = nurb.VKnots.EndParameter,
                                vinc = (vend - vstart)/nurb.VKnots.Count;

                            for (var u = ustart; u <= uend; u += uinc)
                            {

                                for (var v = vstart; v <= vend; v += vinc)
                                {

                                    coll.Add(nurb.Evaluate(u, v));
                                    counter++;
                                }

                            }
                        }
                        if (counter < 1)
                        {
                            var sub = new DBObjectCollection();
                            surf.Explode(sub);
                            foreach (Entity entt in sub)
                            {
                                acBlkTblRec.AppendEntity(entt);
                                acTrans.AddNewlyCreatedDBObject(entt, true);
                                curvesColl.Add(entt);
                            }
                        }
                    }
                    else
                    {
                        var surf = (Region) ent;
                        var p1 = surf.GeometricExtents.MaxPoint;
                        var p2 = surf.GeometricExtents.MinPoint;
                        var p = new Point3d((p1.X + p2.X)/2.0, (p1.Y + p2.Y)/2.0, (p1.Z + p2.Z)/2.0);
                        coll.Add(p);
                        coll.Add(p);
                        var sub = new DBObjectCollection();
                        surf.Explode(sub);
                        foreach (Entity entt in sub)
                        {
                            acBlkTblRec.AppendEntity(entt);
                            acTrans.AddNewlyCreatedDBObject(entt, true);
                            curvesColl.Add(entt);
                        }
                    }

                    foreach (DBObject ob in curvesColl)
                    {
                        var curve = (Curve) acTrans.GetObject(ob.Id, OpenMode.ForRead);
                        var p1 = curve.StartPoint;
                        var p2 = curve.EndPoint;
                        var p = new Point3d((p1.X + p2.X)/2.0, (p1.Y + p2.Y)/2.0, (p1.Z + p2.Z)/2.0);

                        coll.Add(p);
                        coll.Add(p1);
                        coll.Add(p2);
                    }

                    if (coll.Count > 0)
                    {
                        rez = coll[0];
                        foreach (Point3d p in coll)
                        {
                            if (p.DistanceTo(point) < rez.DistanceTo(point))
                            {
                                rez = p;
                            }
                        }
                    }
                    else
                    {
                        rez = point;
                    }
                }
                // acTrans.Commit();
            }
            return rez;
        }
    }
}