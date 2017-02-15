using System;
using System.Windows.Forms;
using KojtoCAD.Utilities;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Acad3DSolid = Autodesk.AutoCAD.Interop.Common.Acad3DSolid;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
using Acad3DSolid = BricscadDb.Acad3DSolid;
#endif

namespace KojtoCAD.Mathematics.Geometry
{
    public class GeometryUtilityCommands
    { 
        /// <summary>
        /// Intersect line with plane
        /// </summary>
        [CommandMethod("ILP")]        
        public void IntersectLineAndPlaneStart()
        {
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;

            CommandLineHelper.Command("_UCS", "World");

            PromptDoubleOptions Size = new PromptDoubleOptions("\nIntersection Points MarkSize<5>:");
            PromptDoubleResult size = Ed.GetDouble(Size);
            if (size.Status != PromptStatus.OK)
            {
                return;
            }

            int cl = 0;
            PromptIntegerOptions Color = new PromptIntegerOptions("\nIntersection Points MarkColorIndex< 0 = Esc >: ");
            PromptIntegerResult color = Ed.GetInteger(Color);
            if (color.Status == PromptStatus.OK)
            {
                cl = color.Value;
            }

            // PromptDistanceResult SizeResult = Ed.GetPoint(PointOptions);  

            PromptPointResult fPlanePoint = GetPointFromEditor("\nSelect first PLANE Point: ");
            if (fPlanePoint.Status == PromptStatus.OK)
            {
                Quaternion planePoint1 = new Quaternion(0, fPlanePoint.Value.X, fPlanePoint.Value.Y, fPlanePoint.Value.Z);
                PromptPointResult sPlanePoint = GetPointFromEditor("\nSelect second PLANE Point: ");
                if (sPlanePoint.Status == PromptStatus.OK)
                {
                    Quaternion planePoint2 = new Quaternion(0, sPlanePoint.Value.X, sPlanePoint.Value.Y,
                        sPlanePoint.Value.Z);
                    PromptPointResult tPlanePoint = GetPointFromEditor("\nSelect third PLANE Point: ");
                    {
                        Quaternion planePoint3 = new Quaternion(0, tPlanePoint.Value.X, tPlanePoint.Value.Y,
                            tPlanePoint.Value.Z);
                        if (tPlanePoint.Status == PromptStatus.OK)
                        {
                            UCS ucs = new UCS(planePoint1, planePoint2, planePoint3);
                            var plane = new KPlane(planePoint1, planePoint2, planePoint3);

                            Quaternion X = new Quaternion(0, (double) size.Value, 0, 0);
                            Quaternion Y = new Quaternion(0, 0, (double) size.Value, 0);
                            Quaternion Z = new Quaternion(0, 0, 0, (double) size.Value);

                            X = ucs.ToACS(X);
                            Y = ucs.ToACS(Y);
                            Z = ucs.ToACS(Z);

                            Point3d origin = fPlanePoint.Value;
                            Vector3d xAxis = origin.GetVectorTo(new Point3d(X.GetX(), X.GetY(), X.GetZ()));
                            Vector3d yAxis = origin.GetVectorTo(new Point3d(Y.GetX(), Y.GetY(), Y.GetZ()));
                            Vector3d zAxis = origin.GetVectorTo(new Point3d(Z.GetX(), Z.GetY(), Z.GetZ()));

                            Matrix3d mat = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis,
                                Vector3d.ZAxis, origin, xAxis, yAxis, zAxis);

                            do
                            {
                                PromptPointResult fLinePoint = GetPointFromEditor("\nSelect first Point: ");
                                if (fLinePoint.Status == PromptStatus.OK)
                                {
                                    Quaternion lineePoint1 = new Quaternion(0, fLinePoint.Value.X, fLinePoint.Value.Y,
                                        fLinePoint.Value.Z);
                                    //MessageBox.Show(plane.GetA().ToString() + "\n" + plane.GetB().ToString() + "\n" + plane.GetC().ToString() + "\n" + plane.GetD().ToString());
                                    PromptPointResult sLinePoint = GetPointFromEditor("\nSelect second Point: ");
                                    {
                                        if (sLinePoint.Status == PromptStatus.OK)
                                        {
                                            Quaternion lineePoint2 = new Quaternion(0, sLinePoint.Value.X,
                                                sLinePoint.Value.Y, sLinePoint.Value.Z);

                                            double dist1 = plane.dist(lineePoint1);
                                            double dist2 = plane.dist(lineePoint2);

                                            if (Math.Abs(dist2 - dist1) < 0.00001)
                                            {
                                                MessageBox.Show("Line is parallel to Plane !");
                                                continue;
                                            }

                                            Quaternion v = lineePoint2 - lineePoint1;
                                            double d = (-plane.GetA()*lineePoint1.GetX() -
                                                        plane.GetB()*lineePoint1.GetY() -
                                                        plane.GetC()*lineePoint1.GetZ() - plane.GetD())/
                                                       (plane.GetA()*v.GetX() + plane.GetB()*v.GetY() +
                                                        plane.GetC()*v.GetZ());
                                            Quaternion vv = new Quaternion(0, d*v.GetX() + lineePoint1.GetX(),
                                                d*v.GetY() + lineePoint1.GetY(), d*v.GetZ() + lineePoint1.GetZ());


                                            using (Transaction acTrans = Db.TransactionManager.StartTransaction())
                                            {
                                                // Open the Block table for read
                                                BlockTable acBlkTbl;
                                                acBlkTbl =
                                                    acTrans.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;

                                                // Open the Block table record Model space for write
                                                BlockTableRecord acBlkTblRec;
                                                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                    OpenMode.ForWrite) as BlockTableRecord;

                                                // Create a point at (4, 3, 0) in Model space
                                                DBPoint acPoint =
                                                    new DBPoint(new Point3d(vv.GetX(), vv.GetY(), vv.GetZ()));
                                                acPoint.ColorIndex = cl;
                                                acPoint.SetDatabaseDefaults();

                                                // Add the new object to the block table record and the transaction
                                                acBlkTblRec.AppendEntity(acPoint);
                                                acTrans.AddNewlyCreatedDBObject(acPoint, true);

                                                // Set the style for all point objects in the drawing
                                                Db.Pdmode = 34;
                                                Db.Pdsize = size.Value;

                                                Point3d acP = new Point3d(vv.GetX(), vv.GetY(), vv.GetZ());
                                                Vector3d acVec3d = acP.GetVectorTo(new Point3d(0, 0, 0));
                                                Vector3d acVec = origin.GetVectorTo(acP);
                                                acPoint.TransformBy(Matrix3d.Displacement(acVec3d));

                                                acPoint.TransformBy(mat);
                                                acPoint.TransformBy(Matrix3d.Displacement(acVec));


                                                Line acLine = new Line(new Point3d(vv.GetX(), vv.GetY(), vv.GetZ()),
                                                    new Point3d(X.GetX() + (vv - planePoint1).GetX(),
                                                        X.GetY() + (vv - planePoint1).GetY(),
                                                        X.GetZ() + (vv - planePoint1).GetZ()));
                                                acLine.ColorIndex = cl + 1;
                                                acLine.SetDatabaseDefaults();

                                                // Add the new object to the block table record and the transaction
                                                acBlkTblRec.AppendEntity(acLine);
                                                acTrans.AddNewlyCreatedDBObject(acLine, true);

                                                Line acLine1 = new Line(new Point3d(vv.GetX(), vv.GetY(), vv.GetZ()),
                                                    new Point3d(Y.GetX() + (vv - planePoint1).GetX(),
                                                        Y.GetY() + (vv - planePoint1).GetY(),
                                                        Y.GetZ() + (vv - planePoint1).GetZ()));
                                                acLine1.ColorIndex = cl + 1;
                                                acLine1.SetDatabaseDefaults();

                                                // Add the new object to the block table record and the transaction
                                                acBlkTblRec.AppendEntity(acLine1);
                                                acTrans.AddNewlyCreatedDBObject(acLine1, true);

                                                // Save the new object to the database
                                                acTrans.Commit();
                                            }
                                            Ed.Regen();
                                        }

                                    }
                                }

                            } while (
                                MessageBox.Show("Next Line ? ", "Line Selection", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes);
                        }
                    }
                }
            }
            CommandLineHelper.Command("_UCS", "Pre");

        }

        /// <summary>
        /// Project line on plane
        /// </summary>
        [CommandMethod("PLP")]
        public void ProjectionLineToPlaneStart()
        {
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;

            CommandLineHelper.Command("_UCS", "World");

            PromptPointResult fPlanePoint = GetPointFromEditor("\nSelect first PLANE Point: ");
            if (fPlanePoint.Status == PromptStatus.OK)
            {
                Quaternion planePoint1 = new Quaternion(0, fPlanePoint.Value.X, fPlanePoint.Value.Y, fPlanePoint.Value.Z);
                PromptPointResult sPlanePoint = GetPointFromEditor("\nSelect second PLANE Point: ");
                if (sPlanePoint.Status == PromptStatus.OK)
                {
                    Quaternion planePoint2 = new Quaternion(0, sPlanePoint.Value.X, sPlanePoint.Value.Y,
                        sPlanePoint.Value.Z);
                    PromptPointResult tPlanePoint = GetPointFromEditor("\nSelect third PLANE Point: ");
                    {
                        Quaternion planePoint3 = new Quaternion(0, tPlanePoint.Value.X, tPlanePoint.Value.Y,
                            tPlanePoint.Value.Z);
                        if (tPlanePoint.Status == PromptStatus.OK)
                        {
                            UCS ucs = new UCS(planePoint1, planePoint2, planePoint3);
                            do
                            {
                                PromptPointResult fLinePoint = GetPointFromEditor("\nSelect first Point: ");
                                if (fLinePoint.Status == PromptStatus.OK)
                                {
                                    Quaternion lineePoint1 = new Quaternion(0, fLinePoint.Value.X, fLinePoint.Value.Y,
                                        fLinePoint.Value.Z);
                                    PromptPointResult sLinePoint = GetPointFromEditor("\nSelect second Point: ");
                                    {
                                        if (sLinePoint.Status == PromptStatus.OK)
                                        {
                                            Quaternion lineePoint2 = new Quaternion(0, sLinePoint.Value.X,
                                                sLinePoint.Value.Y, sLinePoint.Value.Z);

                                            Quaternion sL1 = ucs.FromACS(lineePoint1);
                                            Quaternion eL1 = ucs.FromACS(lineePoint2);

                                            Quaternion sR = new Quaternion(0, sL1.GetX(), sL1.GetY(), 0);
                                            Quaternion eR = new Quaternion(0, eL1.GetX(), eL1.GetY(), 0);

                                            Complex cl1 = new Complex(sL1.GetX(), sL1.GetY());
                                            Complex cl2 = new Complex(eL1.GetX(), eL1.GetY());

                                            using (Transaction acTrans = Db.TransactionManager.StartTransaction())
                                            {
                                                // Open the Block table for read
                                                BlockTable acBlkTbl;
                                                acBlkTbl =
                                                    acTrans.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;

                                                // Open the Block table record Model space for write
                                                BlockTableRecord acBlkTblRec;
                                                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                    OpenMode.ForWrite) as BlockTableRecord;

                                                if ((cl1 - cl2).abs() < 0.0001)
                                                {
                                                    MessageBox.Show("The line is perpendicular to the plane !");
                                                    Quaternion u = ucs.ToACS(sR);
                                                    UCS ucss = new UCS(u, (planePoint1 + planePoint2)/2.0,
                                                        (planePoint3 + planePoint2)/2.0);

                                                    Quaternion P1 = ucss.ToACS(new Quaternion(0, -25, 0, 0));
                                                    Quaternion P2 = ucss.ToACS(new Quaternion(0, 25, 0, 0));

                                                    Quaternion P3 = ucss.ToACS(new Quaternion(0, 0, -25, 0));
                                                    Quaternion P4 = ucss.ToACS(new Quaternion(0, 0, 25, 0));


                                                    Line acLine = new Line(
                                                        new Point3d(P1.GetX(), P1.GetY(), P1.GetZ()),
                                                        new Point3d(P2.GetX(), P2.GetY(), P2.GetZ()));
                                                    acLine.SetDatabaseDefaults();
                                                    acBlkTblRec.AppendEntity(acLine);
                                                    acTrans.AddNewlyCreatedDBObject(acLine, true);


                                                    Line acLine1 = new Line(
                                                        new Point3d(P3.GetX(), P3.GetY(), P3.GetZ()),
                                                        new Point3d(P4.GetX(), P4.GetY(), P4.GetZ()));
                                                    acLine1.SetDatabaseDefaults();
                                                    acBlkTblRec.AppendEntity(acLine1);
                                                    acTrans.AddNewlyCreatedDBObject(acLine1, true);

                                                }
                                                else
                                                {
                                                    Quaternion P1 = ucs.ToACS(sR);
                                                    Quaternion P2 = ucs.ToACS(eR);


                                                    Line acLine = new Line(
                                                        new Point3d(P1.GetX(), P1.GetY(), P1.GetZ()),
                                                        new Point3d(P2.GetX(), P2.GetY(), P2.GetZ()));
                                                    acLine.SetDatabaseDefaults();
                                                    acBlkTblRec.AppendEntity(acLine);
                                                    acTrans.AddNewlyCreatedDBObject(acLine, true);

                                                }

                                                acTrans.Commit();
                                            }


                                            Ed.Regen();
                                        }

                                    }
                                }

                            } while (
                                MessageBox.Show("Next Line ? ", "Line Selection", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes);
                        }
                    }
                }
            }
            CommandLineHelper.Command("_UCS", "Pre");

        }

        /// <summary>
        /// Angle between planes
        /// </summary>
        [CommandMethod("ABP")]
        public void AngleBetweenThePlanesStart()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;

            CommandLineHelper.Command("_UCS", "World");

            do
            {
                PromptPointResult fPlanePoint = GetPointFromEditor("\nSelect first PLANE first Point: ");
                if (fPlanePoint.Status == PromptStatus.OK)
                {
                    Quaternion planePoint1 = new Quaternion(0, fPlanePoint.Value.X, fPlanePoint.Value.Y,
                        fPlanePoint.Value.Z);
                    PromptPointResult sPlanePoint = GetPointFromEditor("\nSelect first PLANE second Point: ");
                    if (sPlanePoint.Status == PromptStatus.OK)
                    {
                        Quaternion planePoint2 = new Quaternion(0, sPlanePoint.Value.X, sPlanePoint.Value.Y,
                            sPlanePoint.Value.Z);
                        PromptPointResult tPlanePoint = GetPointFromEditor("\nSelect first PLANE third Point: ");
                        {
                            if (tPlanePoint.Status == PromptStatus.OK)
                            {
                                Quaternion planePoint3 = new Quaternion(0, tPlanePoint.Value.X, tPlanePoint.Value.Y,
                                    tPlanePoint.Value.Z);
                                UCS ucs = new UCS(planePoint1, planePoint2, planePoint3);
                                do
                                {
                                    PromptPointResult fLinePoint =
                                        GetPointFromEditor("\nSelect second PLANE first Point: ");
                                    if (fLinePoint.Status == PromptStatus.OK)
                                    {
                                        Quaternion planePoint4 = new Quaternion(0, fLinePoint.Value.X,
                                            fLinePoint.Value.Y, fLinePoint.Value.Z);
                                        PromptPointResult sLinePoint =
                                            GetPointFromEditor("\nSelect second PLANE second Point: ");
                                        {
                                            if (sLinePoint.Status == PromptStatus.OK)
                                            {
                                                Quaternion planePoint5 = new Quaternion(0, sLinePoint.Value.X,
                                                    sLinePoint.Value.Y, sLinePoint.Value.Z);
                                                PromptPointResult tLinePoint =
                                                    GetPointFromEditor("\nSelect second PLANE third Point: ");
                                                if (tLinePoint.Status == PromptStatus.OK)
                                                {
                                                    Quaternion planePoint6 = new Quaternion(0, tLinePoint.Value.X,
                                                        tLinePoint.Value.Y, tLinePoint.Value.Z);
                                                    UCS ucs1 = new UCS(planePoint4, planePoint5, planePoint6);
                                                    double ang1 = UCS.AngleBetweenZaxes(ucs, ucs1);
                                                    ang1 = ang1*180.0/Math.PI;

                                                    double ang2 = 180.0 - ang1;

                                                    double ang = (ang1 < ang2) ? ang1 : ang2;

                                                    String str1 = "\nmin Angle = " + ang.ToString("f11") + " degree";
                                                    String str2 = "\nmax Angle = " + (180.0 - ang).ToString("f11") +
                                                                  " degree\n";

                                                    Doc.Editor.WriteMessage(str1 + str2);
                                                    MessageBox.Show(str1 + str2);

                                                }
                                            }

                                        }
                                    }

                                } while (
                                    MessageBox.Show("Next second Plane ? ", "Plane Selection", MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question) == DialogResult.Yes);
                            }
                        }
                    }
                }
            } while (
                MessageBox.Show("Next first Plane ? ", "Plane Selection", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes);
            CommandLineHelper.Command("_UCS", "Pre");
        }

        /// <summary>
        /// Distance between lines
        /// </summary>
        [CommandMethod("DBP")]
        public void DistanceBetweenTheLinesStart()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;

            CommandLineHelper.Command("_UCS", "World");

            do
            {
                PromptPointResult start_first_LinePoint = GetPointFromEditor("\nSelect first LINE start Point: ");
                if (start_first_LinePoint.Status == PromptStatus.OK)
                {
                    Quaternion first_line_StartPoint = new Quaternion(0, start_first_LinePoint.Value.X,
                        start_first_LinePoint.Value.Y, start_first_LinePoint.Value.Z);
                    PromptPointResult sfLinePoint = GetPointFromEditor("\nSelect first LINE end Point: ");
                    if (sfLinePoint.Status == PromptStatus.OK)
                    {
                        Quaternion first_line_EndPoint = new Quaternion(0, sfLinePoint.Value.X, sfLinePoint.Value.Y,
                            sfLinePoint.Value.Z);
                        do
                        {
                            PromptPointResult fsLinePoint = GetPointFromEditor("\nSelect second LINE first Point: ");
                            if (fsLinePoint.Status == PromptStatus.OK)
                            {
                                Quaternion planePoint4 = new Quaternion(0, fsLinePoint.Value.X, fsLinePoint.Value.Y,
                                    fsLinePoint.Value.Z);
                                PromptPointResult sLinePoint = GetPointFromEditor("\nSelect second LINE second Point: ");
                                {
                                    if (sLinePoint.Status == PromptStatus.OK)
                                    {
                                        Doc.Editor.WriteMessage("\n");
                                        Quaternion planePoint5 = new Quaternion(0, sLinePoint.Value.X,
                                            sLinePoint.Value.Y, sLinePoint.Value.Z);
                                        Quaternion q = planePoint5 - planePoint4;
                                        UCS ucs = new UCS(first_line_StartPoint, first_line_EndPoint,
                                            first_line_EndPoint + q);
                                        Quaternion Q = ucs.FromACS(planePoint5);

                                        double ang = q.angTo(first_line_EndPoint - first_line_StartPoint);
                                        if (double.IsNaN(ang))
                                        {
                                            double err = Math.Abs(q.cosTo(first_line_EndPoint - first_line_StartPoint));
                                            if (Math.Abs(err - 1.0) < 0.1)
                                            {
                                                ang = 0.0;
                                            }
                                        }
                                        ang = Math.Abs(ang*180.0/Math.PI);
                                        double ang1 = 180.0 - ang;

                                        double minang = (ang < ang1) ? ang : ang1;

                                        if (double.IsNaN(Q.GetZ()))
                                        {
                                            //MessageBox.Show(ang.ToString());                                        
                                            if ((Math.Abs(ang) < 0.000000001) ||
                                                (Math.Abs(Math.Abs(ang) - 180.0) < 0.000000001))
                                            {
                                                ucs = new UCS(first_line_StartPoint, first_line_EndPoint, planePoint5);
                                                Q = ucs.FromACS(planePoint5);
                                                if (double.IsNaN(Q.GetY()))
                                                {
                                                    MessageBox.Show("Invalid selection or dist = 0.0 !");
                                                }
                                                else
                                                {
                                                    Doc.Editor.WriteMessage("\nmin Dist = " +
                                                                            (Math.Abs(Q.GetY())).ToString("f11") +
                                                                            "\n\n" + "minAngle = " +
                                                                            minang.ToString("f11") + " degree\n" +
                                                                            "maxAngle = " +
                                                                            (180.0 - minang).ToString("f11") +
                                                                            " degree\n");
                                                    MessageBox.Show("\nmin Dist = " +
                                                                    (Math.Abs(Q.GetY())).ToString("f11") + "\n\n" +
                                                                    "minAngle = " + minang.ToString("f11") + " degree\n" +
                                                                    "maxAngle = " + (180.0 - minang).ToString("f11") +
                                                                    " degree\n");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Doc.Editor.WriteMessage("\nmin Dist = " +
                                                                    (Math.Abs(Q.GetZ())).ToString("f11") + "\n\n" +
                                                                    "minAngle = " + minang.ToString("f11") + " degree\n" +
                                                                    "maxAngle = " + (180.0 - minang).ToString("f11") +
                                                                    " degree\n");
                                            MessageBox.Show("\nmin Dist = " + (Math.Abs(Q.GetZ())).ToString("f11") +
                                                            "\n\n" + "minAngle = " + minang.ToString("f11") +
                                                            " degree\n" + "maxAngle = " +
                                                            (180.0 - minang).ToString("f11") + " degree\n");
                                        }

                                    }

                                }
                            }

                        } while (
                            MessageBox.Show("Next second LINE ? ", "Line Selection", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes);
                    }
                }

            } while (
                MessageBox.Show("Next first LINE ? ", "Line Selection", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes);
            CommandLineHelper.Command("_UCS", "Pre");
        }

        /// <summary>
        /// SDistance between lines
        /// </summary>
        [CommandMethod("SDBP")]
        public void SDistanceBetweenTheLinesStart()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;

            CommandLineHelper.Command("_UCS", "World");

            do
            {
                PromptPointResult start_first_LinePoint = GetPointFromEditor("\nSelect first LINE start Point: ");
                if (start_first_LinePoint.Status == PromptStatus.OK)
                {
                    Quaternion first_line_StartPoint = new Quaternion(0, start_first_LinePoint.Value.X,
                        start_first_LinePoint.Value.Y, start_first_LinePoint.Value.Z);
                    PromptPointResult sfLinePoint = GetPointFromEditor("\nSelect first LINE end Point: ");
                    if (sfLinePoint.Status == PromptStatus.OK)
                    {
                        Quaternion first_line_EndPoint = new Quaternion(0, sfLinePoint.Value.X, sfLinePoint.Value.Y,
                            sfLinePoint.Value.Z);
                        do
                        {
                            PromptPointResult fsLinePoint = GetPointFromEditor("\nSelect second LINE first Point: ");
                            if (fsLinePoint.Status == PromptStatus.OK)
                            {
                                Quaternion planePoint4 = new Quaternion(0, fsLinePoint.Value.X, fsLinePoint.Value.Y,
                                    fsLinePoint.Value.Z);
                                PromptPointResult sLinePoint = GetPointFromEditor("\nSelect second LINE second Point: ");
                                {
                                    if (sLinePoint.Status == PromptStatus.OK)
                                    {
                                        Doc.Editor.WriteMessage("\n");
                                        Quaternion planePoint5 = new Quaternion(0, sLinePoint.Value.X,
                                            sLinePoint.Value.Y, sLinePoint.Value.Z);
                                        Quaternion q = planePoint5 - planePoint4;
                                        UCS ucs = new UCS(first_line_StartPoint, first_line_EndPoint,
                                            first_line_EndPoint + q);
                                        Quaternion Q1 = ucs.FromACS(planePoint5);
                                        if (!double.IsNaN(Q1.GetZ()))
                                        {
                                            Quaternion Q2 = ucs.FromACS(planePoint4);
                                            if (!double.IsNaN(Q2.GetZ()))
                                            {
                                                Quaternion Q3 = ucs.FromACS(first_line_StartPoint);
                                                Quaternion Q4 = ucs.FromACS(first_line_EndPoint);

                                                Complex c1 = new Complex(Q1.GetX(), Q1.GetY());
                                                Complex c2 = new Complex(Q2.GetX(), Q2.GetY());
                                                Complex c3 = new Complex(Q3.GetX(), Q3.GetY());
                                                Complex c4 = new Complex(Q4.GetX(), Q4.GetY());

                                                var line1 = new KLine2D(c1, c2);
                                                var line2 = new KLine2D(c3, c4);

                                                Complex intP = line1.IntersectWitch(line2);

                                                Quaternion rezS = new Quaternion(0.0, intP.real(), intP.imag(), 0.0);
                                                Quaternion rezE = new Quaternion(0.0, intP.real(), intP.imag(),
                                                    Q1.GetZ());

                                                Quaternion P1 = ucs.ToACS(rezS);
                                                Quaternion P2 = ucs.ToACS(rezE);

                                                using (Transaction acTrans = Db.TransactionManager.StartTransaction())
                                                {
                                                    BlockTable acBlkTbl;
                                                    acBlkTbl =
                                                        acTrans.GetObject(Db.BlockTableId, OpenMode.ForRead) as
                                                            BlockTable;

                                                    BlockTableRecord acBlkTblRec;
                                                    acBlkTblRec =
                                                        acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                            OpenMode.ForWrite) as BlockTableRecord;

                                                    Line acLine = new Line(
                                                        new Point3d(P1.GetX(), P1.GetY(), P1.GetZ()),
                                                        new Point3d(P2.GetX(), P2.GetY(), P2.GetZ()));
                                                    acLine.SetDatabaseDefaults();
                                                    acBlkTblRec.AppendEntity(acLine);
                                                    acTrans.AddNewlyCreatedDBObject(acLine, true);


                                                    acTrans.Commit();
                                                }
                                                Ed.Regen();
                                            }
                                            else
                                            {
                                                Doc.Editor.WriteMessage("ERROR: Second LINE first Point !");
                                                MessageBox.Show("Second LINE first Point !", "E R R O R");
                                            }
                                        }
                                        else
                                        {
                                            Doc.Editor.WriteMessage("ERROR: Second LINE second Point !");
                                            MessageBox.Show("Second LINE second Point !", "E R R O R");
                                        }

                                    }

                                }
                            }

                        } while (
                            MessageBox.Show("Next second LINE ? ", "Line Selection", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes);
                    }
                }

            } while (
                MessageBox.Show("Next first LINE ? ", "Line Selection", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes);
            CommandLineHelper.Command("_UCS", "Pre");
        }

        /// <summary>
        /// Intersection of two planes"
        /// </summary>
        [CommandMethod("IPP")]
        public void IntersectionOfTwoPlanesStart()
        {
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;

            CommandLineHelper.Command("_UCS", "World");

            double nullDist = 0.00000001;

            do
            {
                PromptPointResult fPlanePoint = GetPointFromEditor("\nSelect first PLANE first Point: ");
                if (fPlanePoint.Status == PromptStatus.OK)
                {
                    Quaternion p1 = new Quaternion(0, fPlanePoint.Value.X, fPlanePoint.Value.Y, fPlanePoint.Value.Z);
                    PromptPointResult sPlanePoint = GetPointFromEditor("\nSelect first PLANE second Point: ");
                    if (sPlanePoint.Status == PromptStatus.OK)
                    {
                        Quaternion p2 = new Quaternion(0, sPlanePoint.Value.X, sPlanePoint.Value.Y, sPlanePoint.Value.Z);
                        PromptPointResult tPlanePoint = GetPointFromEditor("\nSelect first PLANE third Point: ");
                        {
                            if (tPlanePoint.Status == PromptStatus.OK)
                            {
                                Quaternion p3 = new Quaternion(0, tPlanePoint.Value.X, tPlanePoint.Value.Y,
                                    tPlanePoint.Value.Z);
                                UCS ucs = new UCS(p1, p2, p3);
                                Quaternion QQ = (p1 - p2)/(p3 - p2);
                                if ((QQ.absV() == 0.0) || (double.IsNaN(ucs.FromACS(p1).GetZ())))
                                {
                                    MessageBox.Show("ERROR: All three points are on a straight !");
                                }
                                else
                                {
                                    do
                                    {
                                        PromptPointResult fPlanePoint1 =
                                            GetPointFromEditor("\nSelect second PLANE first Point: ");
                                        if (fPlanePoint1.Status == PromptStatus.OK)
                                        {
                                            Quaternion p4 = new Quaternion(0, fPlanePoint1.Value.X, fPlanePoint1.Value.Y,
                                                fPlanePoint1.Value.Z);
                                            PromptPointResult sPlanePoint1 =
                                                GetPointFromEditor("\nSelect second PLANE second Point: ");
                                            if (sPlanePoint1.Status == PromptStatus.OK)
                                            {
                                                Quaternion p5 = new Quaternion(0, sPlanePoint1.Value.X,
                                                    sPlanePoint1.Value.Y, sPlanePoint1.Value.Z);
                                                PromptPointResult tPlanePoint1 =
                                                    GetPointFromEditor("\nSelect second PLANE third Point: ");
                                                if (tPlanePoint1.Status == PromptStatus.OK)
                                                {
                                                    Quaternion p6 = new Quaternion(0, tPlanePoint1.Value.X,
                                                        tPlanePoint1.Value.Y, tPlanePoint1.Value.Z);
                                                    UCS ucs1 = new UCS(p4, p5, p6);
                                                    Quaternion QQQ = (p4 - p5)/(p6 - p5);
                                                    if ((QQQ.absV() == 0.0) || (double.IsNaN(ucs1.FromACS(p4).GetZ())))
                                                    {
                                                        MessageBox.Show("ERROR: All three points are on a straight !");
                                                    }
                                                    else
                                                    {
                                                        double d1 =
                                                            Math.Abs(ucs.FromACS(p4).GetZ() - ucs.FromACS(p5).GetZ());
                                                        double d2 =
                                                            Math.Abs(ucs.FromACS(p4).GetZ() - ucs.FromACS(p6).GetZ());
                                                        double d3 =
                                                            Math.Abs(ucs.FromACS(p5).GetZ() - ucs.FromACS(p6).GetZ());
                                                        if ((d1 < nullDist) && (d2 < nullDist) && (d3 < nullDist))
                                                        {
                                                            MessageBox.Show("ERROR: Parallel Planes !");
                                                            continue;
                                                        }
                                                        else
                                                        {

                                                            var plane = new KPlane(p1, p2, p3);
                                                            Quaternion vv = plane.IntersectWithVector(p5, p4);
                                                            Quaternion vvv = plane.IntersectWithVector(p6, p5);
                                                            Quaternion vvvv = plane.IntersectWithVector(p6, p4);

                                                            if (vv.real() < 0)
                                                            {
                                                                vv = plane.IntersectWithVector(p5, (p4 + p6)/2.0);
                                                            }
                                                            if (vvv.real() < 0)
                                                            {
                                                                vvv = plane.IntersectWithVector(p6, (p4 + p5)/2.0);
                                                            }
                                                            if (vvvv.real() < 0)
                                                            {
                                                                vvvv = plane.IntersectWithVector(p6, (p4 + p5)/2.0);
                                                            }


                                                            // MessageBox.Show(vv.GetX().ToString() + "\n" + vv.GetY().ToString() + "\n" + vv.GetZ().ToString());
                                                            // MessageBox.Show(vvv.GetX().ToString() + "\n" + vvv.GetY().ToString() + "\n" + vvv.GetZ().ToString());
                                                            //  MessageBox.Show(vvvv.GetX().ToString() + "\n" + vvvv.GetY().ToString() + "\n" + vvvv.GetZ().ToString());


                                                            using (
                                                                Transaction acTrans =
                                                                    Db.TransactionManager.StartTransaction())
                                                            {
                                                                // Open the Block table for read
                                                                BlockTable acBlkTbl;
                                                                acBlkTbl =
                                                                    acTrans.GetObject(Db.BlockTableId, OpenMode.ForRead)
                                                                        as BlockTable;

                                                                // Open the Block table record Model space for write
                                                                BlockTableRecord acBlkTblRec;
                                                                acBlkTblRec =
                                                                    acTrans.GetObject(
                                                                        acBlkTbl[BlockTableRecord.ModelSpace],
                                                                        OpenMode.ForWrite) as BlockTableRecord;

                                                                if (((vvv - vvvv).abs() > (vv - vvvv).abs()) &&
                                                                    ((vvv - vvvv).abs() > (vv - vvv).abs()))
                                                                {
                                                                    Line acLine =
                                                                        new Line(
                                                                            new Point3d(vvv.GetX(), vvv.GetY(),
                                                                                vvv.GetZ()),
                                                                            new Point3d(vvvv.GetX(), vvvv.GetY(),
                                                                                vvvv.GetZ()));
                                                                    acLine.SetDatabaseDefaults();
                                                                    acBlkTblRec.AppendEntity(acLine);
                                                                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                                                                }
                                                                else
                                                                {

                                                                    if ((vvvv - vv).abs() > (vvv - vv).abs())
                                                                    {
                                                                        Line acLine1 =
                                                                            new Line(
                                                                                new Point3d(vvvv.GetX(), vvvv.GetY(),
                                                                                    vvvv.GetZ()),
                                                                                new Point3d(vv.GetX(), vv.GetY(),
                                                                                    vv.GetZ()));
                                                                        acLine1.SetDatabaseDefaults();
                                                                        acBlkTblRec.AppendEntity(acLine1);
                                                                        acTrans.AddNewlyCreatedDBObject(acLine1, true);
                                                                    }
                                                                    else
                                                                    {
                                                                        Line acLine2 =
                                                                            new Line(
                                                                                new Point3d(vv.GetX(), vv.GetY(),
                                                                                    vv.GetZ()),
                                                                                new Point3d(vvv.GetX(), vvv.GetY(),
                                                                                    vvv.GetZ()));
                                                                        acLine2.SetDatabaseDefaults();
                                                                        acBlkTblRec.AppendEntity(acLine2);
                                                                        acTrans.AddNewlyCreatedDBObject(acLine2, true);
                                                                    }
                                                                }


                                                                acTrans.Commit();
                                                            }
                                                            Ed.Regen();
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    } while (
                                        MessageBox.Show("Next second Plane ? ", "Plane Selection",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
                                }
                            }
                        }
                    }
                }
            } while (
                MessageBox.Show("Next first Plane ? ", "Plane Selection", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes);
            CommandLineHelper.Command("_UCS", "Pre");
        }

        /// <summary>
        /// "Angle between line and plane"
        /// </summary>
        [CommandMethod("ABLP")]
        public void AngleBetweenTheLineAndPlaneStart()
        {
            CommandLineHelper.Command("_UCS", "World");

            PromptPointResult fPlanePoint = GetPointFromEditor("\nSelect first PLANE Point: ");
            if (fPlanePoint.Status == PromptStatus.OK)
            {
                Quaternion planePoint1 = new Quaternion(0, fPlanePoint.Value.X, fPlanePoint.Value.Y, fPlanePoint.Value.Z);
                PromptPointResult sPlanePoint = GetPointFromEditor("\nSelect second PLANE Point: ");
                if (sPlanePoint.Status == PromptStatus.OK)
                {
                    Quaternion planePoint2 = new Quaternion(0, sPlanePoint.Value.X, sPlanePoint.Value.Y,
                        sPlanePoint.Value.Z);
                    PromptPointResult tPlanePoint = GetPointFromEditor("\nSelect third PLANE Point: ");
                    {
                        Quaternion planePoint3 = new Quaternion(0, tPlanePoint.Value.X, tPlanePoint.Value.Y,
                            tPlanePoint.Value.Z);
                        if (tPlanePoint.Status == PromptStatus.OK)
                        {
                            UCS ucs = new UCS(planePoint1, planePoint2, planePoint3);
                            do
                            {
                                PromptPointResult fLinePoint = GetPointFromEditor("\nSelect first Point: ");
                                if (fLinePoint.Status == PromptStatus.OK)
                                {
                                    Quaternion lineePoint1 = new Quaternion(0, fLinePoint.Value.X, fLinePoint.Value.Y,
                                        fLinePoint.Value.Z);
                                    PromptPointResult sLinePoint = GetPointFromEditor("\nSelect second Point: ");
                                    {
                                        if (sLinePoint.Status == PromptStatus.OK)
                                        {
                                            Quaternion lineePoint2 = new Quaternion(0, sLinePoint.Value.X,
                                                sLinePoint.Value.Y, sLinePoint.Value.Z);

                                            Quaternion sL1 = ucs.FromACS(lineePoint1);
                                            Quaternion eL1 = ucs.FromACS(lineePoint2);

                                            Quaternion sR = new Quaternion(0, sL1.GetX(), sL1.GetY(), 0);
                                            Quaternion eR = new Quaternion(0, eL1.GetX(), eL1.GetY(), 0);

                                            Complex cl1 = new Complex(sL1.GetX(), sL1.GetY());
                                            Complex cl2 = new Complex(eL1.GetX(), eL1.GetY());

                                            if ((cl1 - cl2).abs() < 0.0001)
                                            {
                                                MessageBox.Show("The line is perpendicular to the plane !");
                                            }
                                            else
                                            {
                                                Quaternion P1 = ucs.ToACS(sR);
                                                Quaternion P2 = ucs.ToACS(eR);
                                                Quaternion P3 = P2 - P1;

                                                double min = Math.Abs((P3/(lineePoint2 - lineePoint1)).arg());
                                                min = min*180.0/Math.PI;
                                                double max = 180.0 - min;
                                                min = (min < max) ? min : max;
                                                max = 180 - min;
                                                MessageBox.Show("minAngle = " + min.ToString("f11") + " degree" +
                                                                "\nmaxAngle = " + max.ToString("f11") + " degree");
                                            }

                                        }

                                    }
                                }

                            } while (
                                MessageBox.Show("Next Line ? ", "Line Selection", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes);
                        }
                    }
                }
            }
            CommandLineHelper.Command("_UCS", "Pre");

        }

        [CommandMethod("MDIST")]
        public void MdistStart()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;

            TypedValue[] acTypValAr = new TypedValue[9];
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Operator, "<or"), 0);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "CIRCLE"), 1);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "POLYLINE"), 2);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "LWPOLYLINE"), 3);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "LINE"), 4);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "SPLINE"), 5);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "ELLIPSE"), 6);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "ARC"), 7);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Operator, "or>"), 8);


            SelectionFilter SelFilter = new SelectionFilter(acTypValAr);

            PromptSelectionOptions sOptions = new PromptSelectionOptions();
            sOptions.MessageForAdding = "\r\nSelect first 2d line";
            sOptions.AllowDuplicates = true;
            sOptions.SingleOnly = true;
            PromptSelectionResult acSSPrompt = Ed.GetSelection(sOptions, SelFilter);
            Point3dCollection firstARR = new Point3dCollection();
            Point3dCollection secondARR = new Point3dCollection();
            Curve c1 = null;
            double step1 = 0;
            Curve c2 = null;
            double step2 = 0;
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                firstARR = GetPoints(ref Doc, ref acSSPrompt, 1.0/2.0, ref c1, ref step1);
                //-------------------------------------------------------------------------------------------------------
                PromptSelectionOptions sOptions1 = new PromptSelectionOptions();
                sOptions1.MessageForAdding = "\r\nSelect second 2d Line";
                sOptions1.AllowDuplicates = true;
                sOptions1.SingleOnly = true;
                PromptSelectionResult acSSPrompt1 = Ed.GetSelection(sOptions1, SelFilter);
                if (acSSPrompt1.Status == PromptStatus.OK)
                {
                    secondARR = GetPoints(ref Doc, ref acSSPrompt1, 1.0/2.0, ref c2, ref step2);
                }
                //-------------------------------------------------------------------------------------------------------       

                Point3d p1 = firstARR[0];
                Point3d p2 = secondARR[0];
                double mdist = p1.DistanceTo(p2);

                foreach (Point3d p in firstARR)
                {
                    foreach (Point3d pp in secondARR)
                    {
                        double md = pp.DistanceTo(p);
                        if ((md < mdist) && (md > 0))
                        {
                            p1 = p;
                            p2 = pp;
                            mdist = md;
                        }
                    }
                }

                // GeometryUtility.DrawLine(p1,p2,ref Doc,1);

                double d1 = c1.GetDistAtPoint(p1);
                double d2 = c2.GetDistAtPoint(p2);

                double start1 = ((d1 - 3*step1) < 0)
                    ? (((d1 - 2*step1) < 0) ? (((d1 - step1) < 0) ? 0 : (d1 - step1)) : (d1 - 2*step1))
                    : (d1 - 3*step1);
                double start2 = ((d2 - 3*step2) < 0)
                    ? (((d2 - 2*step2) < 0) ? (((d2 - step2) < 0) ? 0 : (d2 - step2)) : (d2 - 2*step2))
                    : (d2 - 3*step2);
                double end1 = start1 + 6*step1;
                double end2 = start2 + 6*step2;

                firstARR = GetPoints(ref Doc, ref c1, start1, end1, 1.0*50.0);
                secondARR = GetPoints(ref Doc, ref c2, start2, end2, 1.0*50.0);

                p1 = firstARR[0];
                p2 = secondARR[0];
                mdist = p1.DistanceTo(p2);

                foreach (Point3d p in firstARR)
                {
                    foreach (Point3d pp in secondARR)
                    {
                        double md = pp.DistanceTo(p);
                        if ((md < mdist) && (md > 0))
                        {
                            p1 = p;
                            p2 = pp;
                            mdist = md;
                        }
                    }
                }

                GeometryUtility.DrawLine(p1, p2, ref Doc, 2);
                MessageBox.Show("Approximate DISTANCE = " + mdist.ToString("f2"));

            }

        }

        /// <summary>
        /// MDIST3D
        /// </summary>
        [CommandMethod("MDIST3D")]
        public void Mdist2Start()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;


            TypedValue[] acTypValAr = new TypedValue[1];
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "3DSOLID"), 0);


            SelectionFilter SelFilter = new SelectionFilter(acTypValAr);

            PromptSelectionOptions sOptions = new PromptSelectionOptions();
            sOptions.MessageForAdding = "\r\nSelect first 3d Solid";
            sOptions.AllowDuplicates = true;
            sOptions.SingleOnly = true;
            PromptSelectionResult acSSPrompt = Ed.GetSelection(sOptions, SelFilter);
            Point3dCollection firstARR = new Point3dCollection();
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                PromptSelectionOptions sOptions1 = new PromptSelectionOptions();
                sOptions1.MessageForAdding = "\r\nSelect second 3d Solid";
                sOptions1.AllowDuplicates = true;
                sOptions1.SingleOnly = true;
                PromptSelectionResult acSSPrompt1 = Ed.GetSelection(sOptions1, SelFilter);
                if (acSSPrompt1.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    SelectionSet acSSet1 = acSSPrompt1.Value;

                    using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
                    {
                        Solid3d acEnt = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForWrite) as Solid3d;
                        Solid3d acEnt1 = acTrans.GetObject(acSSet1[0].ObjectId, OpenMode.ForWrite) as Solid3d;

                        if (acEnt.CheckInterference(acEnt1) == true)
                        {
                            MessageBox.Show("Interference\n\n DISTANCE = 0", "R E S U L T");
                            return;
                        }

                    }

                    #region pre

                    Vector3d MoveV = new Vector3d();
                    using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = acTrans.GetObject(Db.BlockTableId, OpenMode.ForWrite) as BlockTable;

                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec =
                            acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                                BlockTableRecord;

                        Solid3d acEnt = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForWrite) as Solid3d;
                        Solid3d acEnt1 = acTrans.GetObject(acSSet1[0].ObjectId, OpenMode.ForWrite) as Solid3d;

                        Solid3d acEntNEW = acEnt.Clone() as Solid3d;
                        acEntNEW.SetDatabaseDefaults();
                        acBlkTblRec.AppendEntity(acEntNEW);
                        acTrans.AddNewlyCreatedDBObject(acEntNEW, true);

                        Solid3d acEnt1NEW = acEnt1.Clone() as Solid3d;
                        acEnt1NEW.SetDatabaseDefaults();
                        acBlkTblRec.AppendEntity(acEnt1NEW);
                        acTrans.AddNewlyCreatedDBObject(acEnt1NEW, true);

                        Point3d c1 = acEnt.MassProperties.Centroid;
                        Point3d c2 = acEnt1.MassProperties.Centroid;
                        Point3d c = new Point3d((c1.X + c2.X)/2.0, (c1.Y + c2.Y)/2.0, (c1.Z + c2.Z)/2.0);
                        Vector3d vM = c.GetVectorTo(Point3d.Origin);
                        MoveV = Point3d.Origin.GetVectorTo(c);

                        acEnt.TransformBy(Matrix3d.Displacement(vM));
                        acEnt1.TransformBy(Matrix3d.Displacement(vM));

                        acEnt.Visible = false;
                        acEnt1.Visible = false;

                        acTrans.Commit();
                    }

                    #endregion

                    try
                    {
                        double Offset = 0;
                        Point3d p = new Point3d();
                        Point3d pp = new Point3d();

                        #region calc1

                        using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
                        {
                            Solid3d acEnt = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForWrite) as Solid3d;
                            Solid3d acEnt1 = acTrans.GetObject(acSSet1[0].ObjectId, OpenMode.ForWrite) as Solid3d;

                            double start_dist = 0;
                            double step = 100;
                            Point3d p1 = OffsetSearchColision(ref Doc, ref acEnt, ref acEnt1, step, ref start_dist);

                            try
                            {
                                start_dist = (start_dist - step > 0) ? (start_dist - step) : 0;
                                step = 10;
                                p1 = OffsetSearchColision(ref Doc, ref acEnt, ref acEnt1, step, ref start_dist);
                            }
                            catch
                            {

                            }

                            try
                            {
                                start_dist = (start_dist - step > 0) ? (start_dist - step) : 0;
                                step = 1;
                                p1 = OffsetSearchColision(ref Doc, ref acEnt, ref acEnt1, step, ref start_dist);
                            }
                            catch
                            {
                            }

                            try
                            {
                                start_dist = (start_dist - step > 0) ? (start_dist - step) : 0;
                                step = 0.1;
                                p1 = OffsetSearchColision(ref Doc, ref acEnt, ref acEnt1, step, ref start_dist);
                            }
                            catch
                            {
                            }

                            //---------------------------------------------------------------------------------------

                            double off = 0;
                            Point3d p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, 0, 100, ref off);

                            try
                            {
                                off = (off - 100 > 0) ? (off - 100) : 0;
                                p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, off, 10, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 10 > 0) ? (off - 10) : 0;
                                p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, off, 1, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 1 > 0) ? (off - 1) : 0;
                                p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, off, 0.1, ref off);
                            }
                            catch
                            {
                            }


                            try
                            {
                                off = (off - 0.2 > 0) ? (off - 0.2) : 0;
                                p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, off, 0.01, ref off);
                            }
                            catch
                            {
                            }
                            //------------------------------------------------------------------------------------------------

                            off = 0;
                            Point3d p3 = DistFromPointToSolid(ref Doc, ref acEnt1, p2, 0, 100, ref off);

                            try
                            {
                                off = (off - 100 > 0) ? (off - 100) : 0;
                                DistFromPointToSolid(ref Doc, ref acEnt1, p2, off, 10, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 10 > 0) ? (off - 10) : 0;
                                DistFromPointToSolid(ref Doc, ref acEnt1, p2, off, 1, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 1 > 0) ? (off - 1) : 0;
                                DistFromPointToSolid(ref Doc, ref acEnt1, p2, off, 0.1, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 0.2 > 0) ? (off - 0.2) : 0;
                                p3 = DistFromPointToSolid(ref Doc, ref acEnt1, p2, off, 0.01, ref off);
                            }
                            catch
                            {
                            }

                            p = p2;
                            pp = p3;
                            Offset = off;
                            // acTrans.Commit();
                        }

                        #endregion


                        #region calc2 ( back)

                        using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
                        {
                            Solid3d acEnt = acTrans.GetObject(acSSet1[0].ObjectId, OpenMode.ForWrite) as Solid3d;
                            Solid3d acEnt1 = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForWrite) as Solid3d;

                            double start_dist = 0;
                            double step = 100;
                            Point3d p1 = OffsetSearchColision(ref Doc, ref acEnt, ref acEnt1, step, ref start_dist);

                            try
                            {
                                start_dist = (start_dist - step > 0) ? (start_dist - step) : 0;
                                step = 10;
                                p1 = OffsetSearchColision(ref Doc, ref acEnt, ref acEnt1, step, ref start_dist);
                            }
                            catch
                            {
                            }

                            try
                            {
                                start_dist = (start_dist - step > 0) ? (start_dist - step) : 0;
                                step = 1;
                                p1 = OffsetSearchColision(ref Doc, ref acEnt, ref acEnt1, step, ref start_dist);
                            }
                            catch
                            {
                            }

                            try
                            {
                                start_dist = (start_dist - step > 0) ? (start_dist - step) : 0;
                                step = 0.1;
                                p1 = OffsetSearchColision(ref Doc, ref acEnt, ref acEnt1, step, ref start_dist);
                            }
                            catch
                            {
                            }
                            //---------------------------------------------------------------------------------------

                            double off = 0;
                            Point3d p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, 0, 100, ref off);

                            try
                            {
                                off = (off - 100 > 0) ? (off - 100) : 0;
                                p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, off, 10, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 10 > 0) ? (off - 10) : 0;
                                p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, off, 1, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 1 > 0) ? (off - 1) : 0;
                                p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, off, 0.1, ref off);
                            }
                            catch
                            {
                            }


                            try
                            {
                                off = (off - 0.2 > 0) ? (off - 0.2) : 0;
                                p2 = DistFromPointToSolid(ref Doc, ref acEnt, p1, off, 0.01, ref off);
                            }
                            catch
                            {
                            }
                            //------------------------------------------------------------------------------------------------

                            off = 0;
                            Point3d p3 = DistFromPointToSolid(ref Doc, ref acEnt1, p2, 0, 100, ref off);

                            try
                            {
                                off = (off - 100 > 0) ? (off - 100) : 0;
                                DistFromPointToSolid(ref Doc, ref acEnt1, p2, off, 10, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 10 > 0) ? (off - 10) : 0;
                                DistFromPointToSolid(ref Doc, ref acEnt1, p2, off, 1, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 1 > 0) ? (off - 1) : 0;
                                DistFromPointToSolid(ref Doc, ref acEnt1, p2, off, 0.1, ref off);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 0.2 > 0) ? (off - 0.2) : 0;
                                p3 = DistFromPointToSolid(ref Doc, ref acEnt1, p2, off, 0.01, ref off);
                            }
                            catch
                            {
                            }

                            if (off < Offset)
                            {
                                p = p2;
                                pp = p3;
                                Offset = off;
                            }
                            // acTrans.Commit();
                        }

                        #endregion


                        //GeometryUtility.DrawLine(p, pp, ref Doc, 3);

                        Offset -= 0.02;
                        MessageBox.Show("Approximate DISTANCE = " + Offset.ToString("f2"), "R E S U L T");

                        #region draw sphere

                        try
                        {
                            Point3d PP = new Point3d((p.X + pp.X)/2, (p.Y + pp.Y)/2, (p.Z + pp.Z)/2);
                            using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl;
                                acBlkTbl = acTrans.GetObject(Db.BlockTableId, OpenMode.ForWrite) as BlockTable;

                                BlockTableRecord acBlkTblRec;
                                acBlkTblRec =
                                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                                        BlockTableRecord;

                                if (pp.DistanceTo(Point3d.Origin) < 0.1)
                                {
                                    Solid3d sphere = new Solid3d();
                                    sphere.SetDatabaseDefaults();
                                    sphere.CreateSphere(Offset);
                                    sphere.ColorIndex = 1;
                                    sphere.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo(p)));
                                    sphere.SetDatabaseDefaults();
                                    acBlkTblRec.AppendEntity(sphere);
                                    acTrans.AddNewlyCreatedDBObject(sphere, true);
                                    sphere.TransformBy(Matrix3d.Displacement(MoveV));
                                }
                                else
                                {
                                    Solid3d sphere = new Solid3d();
                                    sphere.SetDatabaseDefaults();
                                    sphere.CreateSphere(Offset/2);
                                    sphere.ColorIndex = 1;
                                    sphere.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo(PP)));
                                    sphere.SetDatabaseDefaults();
                                    acBlkTblRec.AppendEntity(sphere);
                                    acTrans.AddNewlyCreatedDBObject(sphere, true);
                                    sphere.TransformBy(Matrix3d.Displacement(MoveV));

                                    Line li = GeometryUtility.DrawLine(p, pp, ref Doc, 3);
                                    li.TransformBy(Matrix3d.Displacement(MoveV));
                                }

                                acTrans.Commit();
                            }
                        }
                        catch
                        {
                        }

                        #endregion
                    }
                    catch
                    {
                    }

                    using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
                    {
                        Solid3d acEnt = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForWrite) as Solid3d;
                        Solid3d acEnt1 = acTrans.GetObject(acSSet1[0].ObjectId, OpenMode.ForWrite) as Solid3d;

                        acEnt.Erase();
                        acEnt1.Erase();

                        acTrans.Commit();
                    }
                }
            }

        }

        /// <summary>
        /// PDIST3D
        /// </summary>
        [CommandMethod("PDIST3D")]
        public void PdistStart()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "3DSOLID"), 0);


                SelectionFilter SelFilter = new SelectionFilter(acTypValAr);

                PromptSelectionOptions sOptions = new PromptSelectionOptions();
                sOptions.MessageForAdding = "\r\nSelect 3d Solid";
                sOptions.AllowDuplicates = true;
                sOptions.SingleOnly = true;
                PromptSelectionResult acSSPrompt = Ed.GetSelection(sOptions, SelFilter);
                Point3dCollection firstARR = new Point3dCollection();
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    PromptPointOptions sOptions1 = new PromptPointOptions("");
                    sOptions1.Message = "\r\nSelect Point: ";
                    PromptPointResult acSSPrompt1 = Doc.Editor.GetPoint(sOptions1);
                    if (acSSPrompt1.Status == PromptStatus.OK)
                    {
                        SelectionSet acSSet = acSSPrompt.Value;
                        Point3d POINT = acSSPrompt1.Value;
                        POINT = POINT.TransformBy(Ed.CurrentUserCoordinateSystem);
                        double off = 0;
                        Point3d p3 = new Point3d();

                        using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
                        {
                            Solid3d solid = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                p3 = GeometryUtility.DistFromPointToSolid(ref Doc, ref solid, POINT, 0, 1000, ref off,
                                    false);
                            }
                            catch
                            {
                            }


                            try
                            {
                                off = (off - 1000 > 0) ? (off - 1000) : 0;
                                GeometryUtility.DistFromPointToSolid(ref Doc, ref solid, POINT, off, 100, ref off, false);
                            }
                            catch
                            {
                            }


                            try
                            {
                                off = (off - 100 > 0) ? (off - 100) : 0;
                                GeometryUtility.DistFromPointToSolid(ref Doc, ref solid, POINT, off, 10, ref off, false);
                            }
                            catch
                            {
                            }


                            try
                            {
                                off = (off - 10 > 0) ? (off - 10) : 0;
                                GeometryUtility.DistFromPointToSolid(ref Doc, ref solid, POINT, off, 1, ref off, false);
                            }
                            catch
                            {
                            }


                            try
                            {
                                off = (off - 1 > 0) ? (off - 1) : 0;
                                GeometryUtility.DistFromPointToSolid(ref Doc, ref solid, POINT, off, 0.1, ref off, false);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 0.2 > 0) ? (off - 0.2) : 0;
                                GeometryUtility.DistFromPointToSolid(ref Doc, ref solid, POINT, off, 0.01, ref off,
                                    false);
                            }
                            catch
                            {
                            }

                            try
                            {
                                off = (off - 0.03 > 0) ? (off - 0.03) : 0;
                                p3 = GeometryUtility.DistFromPointToSolid(ref Doc, ref solid, POINT, off, 0.002, ref off,
                                    true);
                            }
                            catch
                            {
                            }

                        }

                        MessageBox.Show("Approximate DISTANCE = " + off.ToString("f2"), "R E S U L T");
                        if (p3.DistanceTo(POINT) > 0.0001)
                            GeometryUtility.DrawLine(p3, POINT, ref Doc, 3, "");



                    }

                }
                //Ed.CurrentUserCoordinateSystem = old;
            }
            catch
            {

                // Ed.CurrentUserCoordinateSystem = old;

            }

        }

        /// <summary>
        /// 3D OFFSET
        /// </summary>
        [CommandMethod("S3DOFFSET")]
        public void Mdist3Start()
        {

            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;

            TypedValue[] acTypValAr = new TypedValue[1];
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "3DSOLID"), 0);


            SelectionFilter SelFilter = new SelectionFilter(acTypValAr);

            PromptSelectionOptions sOptions = new PromptSelectionOptions();
            sOptions.MessageForAdding = "\r\nSelect first 3d Solid";
            sOptions.AllowDuplicates = true;
            sOptions.SingleOnly = true;
            PromptSelectionResult acSSPrompt = Ed.GetSelection(sOptions, SelFilter);
            Point3dCollection firstARR = new Point3dCollection();
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                PromptDoubleOptions pOpts = new PromptDoubleOptions("\nEnter offset Distance: ");
                PromptDoubleResult dpr = Doc.Editor.GetDouble(pOpts);
                if (dpr.Status == PromptStatus.OK)
                {
                    try
                    {
                        SelectionSet acSSet = acSSPrompt.Value;
                        using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
                        {
                            Solid3d acEnt = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                acEnt.OffsetBody(dpr.Value);
                            }
                            catch
                            {
                                MessageBox.Show("Offset Distance !", "E R R O R");
                            }
                            acTrans.Commit();
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Centroid 3D
        /// </summary>
        [CommandMethod("CEN3D")]
        public void Centroid3dStart()
        {

            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;


            TypedValue[] acTypValAr = new TypedValue[1];
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "3DSOLID"), 0);


            SelectionFilter SelFilter = new SelectionFilter(acTypValAr);

            PromptSelectionOptions sOptions = new PromptSelectionOptions();
            sOptions.MessageForAdding = "\r\nSelect 3d Solid";
            sOptions.AllowDuplicates = true;
            sOptions.SingleOnly = true;
            PromptSelectionResult acSSPrompt = Ed.GetSelection(sOptions, SelFilter);
            Point3dCollection firstARR = new Point3dCollection();
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                PromptDoubleOptions pOpts =
                    new PromptDoubleOptions("\nLocated in the Center of the Sphere with Radius: ");
                PromptDoubleResult dpr = Doc.Editor.GetDouble(pOpts);
                if (dpr.Status == PromptStatus.OK)
                {
                    try
                    {
                        SelectionSet acSSet = acSSPrompt.Value;
                        using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
                        {
                            Solid3d acEnt = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                BlockTable acBlkTbl;
                                acBlkTbl = acTrans.GetObject(Db.BlockTableId, OpenMode.ForWrite) as BlockTable;

                                BlockTableRecord acBlkTblRec;
                                acBlkTblRec =
                                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                                        BlockTableRecord;

                                Solid3d sphere = new Solid3d();
                                sphere.SetDatabaseDefaults();
                                sphere.CreateSphere(dpr.Value);
                                sphere.TransformBy(
                                    Matrix3d.Displacement(Point3d.Origin.GetVectorTo(acEnt.MassProperties.Centroid)));
                                sphere.ColorIndex = 4;

                                acBlkTblRec.AppendEntity(sphere);
                                acTrans.AddNewlyCreatedDBObject(sphere, true);



                            }
                            catch
                            {
                                MessageBox.Show("Offset Distance !", "E R R O R");
                            }
                            acTrans.Commit();
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private Point3d OffsetSearchColision(ref Document Doc, ref Solid3d solid1, ref Solid3d solid2, double step,
            ref double start_dist)
        {
            Point3d rez = new Point3d(1, 1, 1);
            Solid3d acEnt = solid1.Clone() as Solid3d;
            Solid3d acEnt1 = solid2.Clone() as Solid3d;
            using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(Doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRec;
                acBlkTblRec =
                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double dist = 0;
                int counter = 0;

                Solid3d solid = new Solid3d();
                do
                {
                    if (solid != null)
                        solid.Dispose();
                    solid = acEnt.Clone() as Solid3d;

                    dist = step*counter + start_dist;
                    try
                    {
                        solid.OffsetBody(dist);
                    }
                    catch
                    {
                        if (counter > 0)
                            acEnt.TransformBy(Matrix3d.Scaling(1.03, acEnt.MassProperties.Centroid));
                    }
                    counter++;


                } while ((solid.CheckInterference(solid2) == false) &&
                         (acEnt.MassProperties.Centroid.DistanceTo(acEnt1.MassProperties.Centroid) > dist));

                //rez = test.MassProperties.Centroid;                           
                start_dist = dist;

                solid = acEnt.Clone() as Solid3d;
                solid.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(solid);
                acTrans.AddNewlyCreatedDBObject(solid, true);

                Solid3d SOLID = acEnt1.Clone() as Solid3d;
                SOLID.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(SOLID);
                acTrans.AddNewlyCreatedDBObject(SOLID, true);
                try
                {
                    solid.OffsetBody(dist + 0.01);

                    Acad3DSolid Reg = (Acad3DSolid) solid.AcadObject;
                    Acad3DSolid Reg1 = (Acad3DSolid) SOLID.AcadObject;
#if acad2013   
                    bool solidsInterfere;
                    Reg = Reg.CheckInterference(Reg1, true, out solidsInterfere);
#endif
#if acad2012
                    Reg = Reg.CheckInterference(Reg1, true);
#endif
#if bcad
                    Reg = Reg.CheckInterference(Reg1, true);
#endif
                    double[] centroid = (double[]) Reg.Centroid;
                    rez = new Point3d(centroid[0], centroid[1], centroid[2]);
                }
                catch
                {
                    try
                    {
                        solid.OffsetBody(dist + 0.1);
                        Acad3DSolid Reg = (Acad3DSolid) solid.AcadObject;
                        Acad3DSolid Reg1 = (Acad3DSolid) SOLID.AcadObject;
#if acad2013
                        bool solidsInterfere;
                        Reg = Reg.CheckInterference(Reg1, true, out solidsInterfere);
#endif
#if acad2012
                        Reg = Reg.CheckInterference(Reg1, true);
#endif
#if bcad
                        Reg = Reg.CheckInterference(Reg1, true);
#endif
                        double[] centroid = (double[]) Reg.Centroid;
                        rez = new Point3d(centroid[0], centroid[1], centroid[2]);
                    }
                    catch
                    {
                        try
                        {
                            solid.OffsetBody(dist + 0.5);
                            Acad3DSolid Reg = (Acad3DSolid) solid.AcadObject;
                            Acad3DSolid Reg1 = (Acad3DSolid) SOLID.AcadObject;
#if acad2013
                        bool solidsInterfere;
                        Reg = Reg.CheckInterference(Reg1, true, out solidsInterfere);
#endif
#if acad2012
                            Reg = Reg.CheckInterference(Reg1, true);
#endif
#if bcad
                            Reg = Reg.CheckInterference(Reg1, true);
#endif

                            double[] centroid = (double[]) Reg.Centroid;
                            rez = new Point3d(centroid[0], centroid[1], centroid[2]);
                        }
                        catch
                        {
                            try
                            {
                                solid.OffsetBody(dist + 1);
                                Acad3DSolid Reg = (Acad3DSolid) solid.AcadObject;
                                Acad3DSolid Reg1 = (Acad3DSolid) SOLID.AcadObject;
#if acad2013
                                bool solidsInterfere;
                                Reg = Reg.CheckInterference(Reg1, true, out solidsInterfere);
#endif
#if acad2012
                                Reg = Reg.CheckInterference(Reg1, true);
#endif
#if bcad
                                Reg = Reg.CheckInterference(Reg1, true);
#endif

                                double[] centroid = (double[]) Reg.Centroid;
                                rez = new Point3d(centroid[0], centroid[1], centroid[2]);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                SOLID.Dispose();
                solid.Dispose();
            }

            return rez;
        }

        private Point3d DistFromPointToSolid(ref Document Doc, ref Solid3d solid, Point3d point, double start,
            double step, ref double off)
        {
            Point3d rez = new Point3d();
            using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(Doc.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;

                BlockTableRecord acBlkTblRec;
                acBlkTblRec =
                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double startR = (start <= 0) ? 0.00001 : start;
                Solid3d sphere = new Solid3d();
                sphere.SetDatabaseDefaults();
                sphere.CreateSphere(startR);
                sphere.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo(point)));


                double dist = 0;
                int counter = 0;
                //double volume = 0;

                Solid3d SPHERE = null;
                do
                {
                    if (SPHERE != null)
                        SPHERE.Dispose();
                    SPHERE = sphere.Clone() as Solid3d;
                    dist = step*counter;
                    try
                    {
                        SPHERE.OffsetBody(dist);
                    }
                    catch
                    {
                    }
                    counter++;

                    //acBlkTblRec.AppendEntity(SPHERE);
                    //acTrans.AddNewlyCreatedDBObject(SPHERE, true);                            

                } while (SPHERE.CheckInterference(solid) == false);
                off = start + dist;

                SPHERE = sphere.Clone() as Solid3d;
                SPHERE.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(SPHERE);
                acTrans.AddNewlyCreatedDBObject(SPHERE, true);

                Solid3d SOLID = solid.Clone() as Solid3d;
                SOLID.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(SOLID);
                acTrans.AddNewlyCreatedDBObject(SOLID, true);
                try
                {
                    SPHERE.OffsetBody(dist + 0.01);
                    Acad3DSolid Reg = (Acad3DSolid) solid.AcadObject;
                    Acad3DSolid Reg1 = (Acad3DSolid) SOLID.AcadObject;
#if acad2013
                    bool solidsInterfere;
                    Reg = Reg.CheckInterference(Reg1, true, out solidsInterfere);
#endif
#if acad2012
                    Reg = Reg.CheckInterference(Reg1, true);
#endif
#if bcad
                    Reg = Reg.CheckInterference(Reg1, true);
#endif
                    double[] centroid = (double[]) Reg.Centroid;
                    rez = new Point3d(centroid[0], centroid[1], centroid[2]);
                }
                catch
                {
                    try
                    {
                        SPHERE.OffsetBody(dist + 0.1);
                        Acad3DSolid Reg = (Acad3DSolid) solid.AcadObject;
                        Acad3DSolid Reg1 = (Acad3DSolid) SOLID.AcadObject;
#if acad2013
                        bool solidsInterfere;
                        Reg = Reg.CheckInterference(Reg1, true, out solidsInterfere);
#endif
#if acad2012
                        Reg = Reg.CheckInterference(Reg1, true);
#endif
#if bcad
                        Reg = Reg.CheckInterference(Reg1, true);
#endif

                        double[] centroid = (double[]) Reg.Centroid;
                        rez = new Point3d(centroid[0], centroid[1], centroid[2]);
                    }
                    catch
                    {
                        try
                        {
                            SPHERE.OffsetBody(dist + 0.5);
                            Acad3DSolid Reg = (Acad3DSolid) solid.AcadObject;
                            Acad3DSolid Reg1 = (Acad3DSolid) SOLID.AcadObject;
#if acad2013
                            bool solidsInterfere;
                            Reg = Reg.CheckInterference(Reg1, true, out solidsInterfere);
#endif
#if acad2012
                            Reg = Reg.CheckInterference(Reg1, true);
#endif
#if bcad
                            Reg = Reg.CheckInterference(Reg1, true);
#endif

                            double[] centroid = (double[]) Reg.Centroid;
                            rez = new Point3d(centroid[0], centroid[1], centroid[2]);
                        }
                        catch
                        {
                            SPHERE.OffsetBody(dist + 1);
                            Acad3DSolid Reg = (Acad3DSolid) solid.AcadObject;
                            Acad3DSolid Reg1 = (Acad3DSolid) SOLID.AcadObject;
#if acad2013
                            bool solidsInterfere;
                            Reg = Reg.CheckInterference(Reg1, true, out solidsInterfere);
#endif
#if acad2012
                            Reg = Reg.CheckInterference(Reg1, true);
#endif
#if bcad
                            Reg = Reg.CheckInterference(Reg1, true);
#endif

                            double[] centroid = (double[]) Reg.Centroid;
                            rez = new Point3d(centroid[0], centroid[1], centroid[2]);
                        }
                    }
                }
                SOLID.Dispose();
                SPHERE.Dispose();
            }

            return rez;
        }

        private Point3dCollection GetPoints(ref Document Doc, ref PromptSelectionResult acSSPrompt, double k,
            ref Curve c, ref double step)
        {
            Point3dCollection arr = new Point3dCollection();

            SelectionSet acSSet = acSSPrompt.Value;
            using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
            {
                #region curve and curvelength

                Curve acEnt = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForRead) as Curve;
                c = acEnt;
                double dist = acEnt.GetDistAtPoint(acEnt.EndPoint);
                if (dist == 0)
                {
                    try
                    {
                        Polyline acEnt1 = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForRead) as Polyline;
                        dist = acEnt1.Length;
                    }
                    catch
                    {
                        try
                        {
                            Circle acEnt1 = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForRead) as Circle;
                            dist = acEnt1.Radius*Math.PI*2.0;
                        }
                        catch
                        {
                            Ellipse acEnt1 = acTrans.GetObject(acSSet[0].ObjectId, OpenMode.ForRead) as Ellipse;
                            double a = acEnt1.MajorRadius;
                            double b = acEnt1.MinorRadius;

                            dist = Math.PI*(3*a + 3*b - Math.Sqrt(10*a*b + 3*a*a + 3*b*b));
                        }
                    }
                }

                #endregion

                double dist1 = dist*k;
                double delta = dist/dist1;
                step = delta;
                for (int i = 0; i < dist1; i++)
                {
                    Point3d p = acEnt.GetPointAtDist(delta*i);
                    arr.Add(p);
                }
            }

            return arr;
        }

        private Point3dCollection GetPoints(ref Document Doc, ref Curve acEnt, double start, double end, double k)
        {
            Point3dCollection arr = new Point3dCollection();

            using (Transaction acTrans = Doc.Database.TransactionManager.StartTransaction())
            {


                double dist = Math.Abs(end - start);
                double dist1 = dist*k;
                double delta = dist/dist1;
                for (int i = 0; i < dist1; i++)
                {
                    Point3d p = acEnt.GetPointAtDist(start + delta*i);
                    arr.Add(p);
                }
            }

            return arr;
        }

        private PromptPointResult GetPointFromEditor(string mess)
        {
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptPointOptions PointOptions = new PromptPointOptions(mess);
            PromptPointResult PointResult = Ed.GetPoint(PointOptions);

            return PointResult;

        }
    }
}