using System;
using System.Collections.Generic;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Node = Autodesk.AutoCAD.BoundaryRepresentation.Node;
using Face = Autodesk.AutoCAD.DatabaseServices.Face;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Teigha.BoundaryRepresentation;
using Face = Teigha.DatabaseServices.Face;
using Application = Bricscad.ApplicationServices.Application;
#endif
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.Calculations3D))]

namespace KojtoCAD.KojtoCAD3D
{
    public class Calculations3D
    {
        public Containers container = ContextVariablesProvider.Container;
        // krystosani prawi
        [CommandMethod("KojtoCAD_3D", "KCAD_DBP", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CROSS_LINES.htm", "")]
        public void KojtoCAD_3D_Method_dbp()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d OLD = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {
                PromptKeywordOptions pop = new PromptKeywordOptions("");
                pop.AppendKeywordsToMessage = true;
                pop.AllowNone = false;
                pop.Keywords.Add("Run");
                pop.Keywords.Add("Help");
                pop.Keywords.Default = "Run";
                PromptResult res = ed.GetKeywords(pop);
                //_AcAp.Application.ShowAlertDialog(res.ToString());
                if (res.Status == PromptStatus.OK)
                {
                    switch (res.StringResult)
                    {
                        case "Run":
                            //Matrix3d old = ed.CurrentUserCoordinateSystem;
                            //ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
                            GetCrossingLinesDistance();
                            // ed.CurrentUserCoordinateSystem = old;
                            break;
                        case "Help":
                            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/CROSS_LINES.htm");
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(
                    string.Format("\nError: {0}\nStackTrace: {1}", ex.Message, ex.StackTrace));
            }
            finally { ed.CurrentUserCoordinateSystem = OLD; }

        }
        private static void GetCrossingLinesDistance()
        {
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {
                PromptPointOptions pntOpt = new PromptPointOptions("First Line - Get First Point");
                PromptPointResult pntRes = editor.GetPoint(pntOpt);

                if (pntRes.Status == PromptStatus.OK)
                {

                    quaternion fLfP = new quaternion(0, pntRes.Value.X, pntRes.Value.Y, pntRes.Value.Z);

                    PromptPointOptions distOpt = new PromptPointOptions("First Line - Get Second Point");
                    PromptPointResult distRes = editor.GetPoint(distOpt);

                    if (distRes.Status == PromptStatus.OK)
                    {
                        quaternion fLsP = new quaternion(0, distRes.Value.X, distRes.Value.Y, distRes.Value.Z);

                        PromptPointOptions pntOpt1 = new PromptPointOptions("Second Line - Get First Point");
                        PromptPointResult pntRes1 = editor.GetPoint(pntOpt1);
                        if (pntRes1.Status == PromptStatus.OK)
                        {
                            quaternion sLfP = new quaternion(0, pntRes1.Value.X, pntRes1.Value.Y, pntRes1.Value.Z);

                            PromptPointOptions distOpt1 = new PromptPointOptions("Second Line - Get Second Point");
                            PromptPointResult distRes1 = editor.GetPoint(distOpt1);
                            if (distRes1.Status == PromptStatus.OK)
                            {
                                quaternion sLsP = new quaternion(0, distRes1.Value.X, distRes1.Value.Y, distRes1.Value.Z);

                                quaternion FQ = fLsP - fLfP;
                                quaternion SQ = sLsP - sLfP;

                                UCS UCS = new UCS(fLfP, fLsP, fLfP + SQ);

                                fLfP = UCS.FromACS(fLfP); fLsP = UCS.FromACS(fLsP);
                                sLfP = UCS.FromACS(sLfP); sLsP = UCS.FromACS(sLsP);

                                double dist = Math.Abs(sLsP.GetZ());

                                #region calc and message
                                if (!double.IsNaN(dist))
                                {
                                    if (dist > 0.0)
                                    {
                                        MessageBox.Show(string.Format("Min Distance = {0:f4}", dist), "Distance");
                                    }
                                    else
                                        MessageBox.Show(string.Format("Min Distance = {0:f4}\n\nThe lines lie in one Plane !", dist), "Distance");
                                }
                                else
                                {
                                    //System.Windows.Forms.MessageBox.Show(string.Format("the lines are parallel"),"Distance");
                                    UCS = new UCS(new quaternion(0, pntRes.Value.X, pntRes.Value.Y, pntRes.Value.Z),
                                        new quaternion(0, distRes.Value.X, distRes.Value.Y, distRes.Value.Z),
                                        new quaternion(0, pntRes1.Value.X, pntRes1.Value.Y, pntRes1.Value.Z));

                                    dist = Math.Abs(UCS.FromACS(new quaternion(0, pntRes1.Value.X, pntRes1.Value.Y, pntRes1.Value.Z)).GetY());

                                    if (!double.IsNaN(dist))
                                    {
                                        MessageBox.Show(string.Format("Min Distance = {0:f4}\n\nThe lines are parallel !\n\nThe lines lie in one Plane !", dist, "Distance"));
                                    }
                                    else
                                        MessageBox.Show(string.Format("Min Distance = {0:f4}\n\nThe directions of the lines coincide !", 0.0, "Distance"));
                                }
                                #endregion

                                #region draw
                                if ((dist > 0) && !double.IsNaN(dist))
                                {
                                    line2d fLine = new line2d(new complex(fLfP.GetX(), fLfP.GetY()),
                                        new complex(fLsP.GetX(), fLsP.GetY()));

                                    line2d sLine = new line2d(new complex(sLfP.GetX(), sLfP.GetY()),
                                        new complex(sLsP.GetX(), sLsP.GetY()));

                                    complex lC = fLine.IntersectWitch(sLine);

                                    quaternion F = UCS.ToACS(new quaternion(0.0, lC.real(), lC.imag(), 0.0));
                                    quaternion S = UCS.ToACS(new quaternion(0.0, lC.real(), lC.imag(), sLsP.GetZ()));

                                    GlobalFunctions.DrawLine(new Point3d(F.GetX(), F.GetY(), F.GetZ()), new Point3d(S.GetX(), S.GetY(), S.GetZ()), 1);


                                }
                                #endregion
                            }
                            else
                                throw new System.Exception("_AcEd.Editor.Get Second Line - Second Point failed");
                        }
                        else
                            throw new System.Exception("_AcEd.Editor.Get Second Line - First Point failed");
                    }
                    else
                        throw new System.Exception("_AcEd.Editor.Get First Line - Second Point failed");
                }
                else
                    throw new System.Exception("_AcEd.Editor.Get First Line - First Point failed");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private static void GetCrossingLinesDistance(quaternion sL1, quaternion eL1, quaternion sL2, quaternion eL2)
        {
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {
                {
                    quaternion fLfP = sL1;
                    {
                        quaternion fLsP = eL1;
                        {
                            quaternion sLfP = sL2;
                            {
                                quaternion sLsP = eL2;

                                quaternion FQ = fLsP - fLfP;
                                quaternion SQ = sLsP - sLfP;

                                UCS UCS = new UCS(fLfP, fLsP, fLfP + SQ);

                                fLfP = UCS.FromACS(fLfP); fLsP = UCS.FromACS(fLsP);
                                sLfP = UCS.FromACS(sLfP); sLsP = UCS.FromACS(sLsP);

                                double dist = Math.Abs(sLsP.GetZ());

                                #region calc and message
                                if (!double.IsNaN(dist))
                                {
                                    if (dist > 0.0)
                                    {
                                        MessageBox.Show(string.Format("Min Distance = {0:f4}", dist), "Distance");
                                    }
                                    else
                                        MessageBox.Show(string.Format("Min Distance = {0:f4}\n\nThe lines lie in one Plane !", dist), "Distance");
                                }
                                else
                                {
                                    //System.Windows.Forms.MessageBox.Show(string.Format("the lines are parallel"),"Distance");
                                    UCS = new UCS(sL1, eL1, sL2);
                                    dist = Math.Abs(UCS.FromACS(sL2).GetY());

                                    if (!double.IsNaN(dist))
                                    {
                                        MessageBox.Show(string.Format("Min Distance = {0:f4}\n\nThe lines are parallel !\n\nThe lines lie in one Plane !", dist, "Distance"));
                                    }
                                    else
                                        MessageBox.Show(string.Format("Min Distance = {0:f4}\n\nThe directions of the lines coincide !", 0.0, "Distance"));
                                }
                                #endregion

                                #region draw
                                if ((dist > 0) && !double.IsNaN(dist))
                                {
                                    line2d fLine = new line2d(new complex(fLfP.GetX(), fLfP.GetY()),
                                        new complex(fLsP.GetX(), fLsP.GetY()));

                                    line2d sLine = new line2d(new complex(sLfP.GetX(), sLfP.GetY()),
                                        new complex(sLsP.GetX(), sLsP.GetY()));

                                    complex lC = fLine.IntersectWitch(sLine);

                                    quaternion F = UCS.ToACS(new quaternion(0.0, lC.real(), lC.imag(), 0.0));
                                    quaternion S = UCS.ToACS(new quaternion(0.0, lC.real(), lC.imag(), sLsP.GetZ()));

                                    GlobalFunctions.DrawLine(new Point3d(F.GetX(), F.GetY(), F.GetZ()), new Point3d(S.GetX(), S.GetY(), S.GetZ()), 1);


                                }
                                #endregion
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_ILP", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CROSS_LINES.htm", "")]
        public void KojtoCAD_3D_Method_ilp()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                Pair<Point3d, PromptStatus> planeFirstSelection =
                    GlobalFunctions.GetPoint(new Point3d(), "\nSelect the first point of the plane:");
                if (planeFirstSelection.Second == PromptStatus.OK)
                {
                    Point3d planeFirstPoint = planeFirstSelection.First;
                    quaternion planeFirst = new quaternion(planeFirstPoint);

                    Pair<Point3d, PromptStatus> planeSecondSelection =
                        GlobalFunctions.GetPoint(new Point3d(), "\nSelect the second point of the plane:");
                    if (planeSecondSelection.Second == PromptStatus.OK)
                    {
                        Point3d planeSecondPoint = planeSecondSelection.First;
                        quaternion planeSecond = new quaternion(planeSecondPoint);
                        if (planeSecondPoint != planeFirstPoint)
                        {
                            Pair<Point3d, PromptStatus> planeThirdSelection =
                                 GlobalFunctions.GetPoint(new Point3d(), "\nSelect the third point of the plane:");
                            if (planeThirdSelection.Second == PromptStatus.OK)
                            {
                                Point3d planeThirdPoint = planeThirdSelection.First;
                                quaternion planeThird = new quaternion(planeThirdPoint);

                                if ((planeThirdPoint != planeFirstPoint) && (planeSecondPoint != planeThirdPoint))
                                {
                                    UCS UCS = new UCS(planeFirst, planeSecond, planeThird);


                                    double check1 = ((planeThird - planeSecond) / (planeThird - planeFirst)).absV();
                                    if (check1 <= Constants.zero_dist)
                                    {
                                        MessageBox.Show("The three points lie on a Line !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }

                                    Pair<string, PromptStatus> ds = new Pair<string, PromptStatus>("No", PromptStatus.Error);
                                    do
                                    {

                                        Pair<Point3d, PromptStatus> lineFirstSelection =
                                             GlobalFunctions.GetPoint(new Point3d(), "\nSelect the first point of the LINE:");
                                        if (lineFirstSelection.Second == PromptStatus.OK)
                                        {
                                            Point3d lineFirstPoint = lineFirstSelection.First;
                                            quaternion lineFirst = new quaternion(lineFirstPoint);

                                            double firstDist = UCS.FromACS(lineFirst).GetZ();

                                            if ((Math.Abs(firstDist) > Constants.zero_dist) && (Math.Sign(firstDist) != 0))
                                            {
                                                Pair<Point3d, PromptStatus> lineSecondSelection =
                                                     GlobalFunctions.GetPoint(new Point3d(), "\nSelect the second point of the LINE:");
                                                if (lineSecondSelection.Second == PromptStatus.OK)
                                                {
                                                    Point3d lineSecondPoint = lineSecondSelection.First;
                                                    quaternion lineSecond = new quaternion(lineSecondPoint);

                                                    if (lineSecond != lineFirst)
                                                    {

                                                        double secondDist = UCS.FromACS(lineSecond).GetZ();
                                                        if ((Math.Abs(secondDist) > Constants.zero_dist) && (Math.Sign(secondDist) != 0))
                                                        {
                                                            quaternion f = new quaternion(0, 0, 0, firstDist);
                                                            quaternion s = new quaternion(0, 0, 0, secondDist);
                                                            if (f != s)
                                                            {
                                                                plane plane = new plane(planeFirst, planeSecond, planeThird);
                                                                quaternion intQ = plane.IntersectWithVector(lineFirst, lineSecond);

                                                                Matrix3d mat = new Matrix3d(UCS.GetAutoCAD_Matrix3d());

                                                                GlobalFunctions.DrawCircle((planeFirst - planeSecond).abs() / 20.0, (Point3d)UCS.FromACS(intQ), ref mat);
                                                                GlobalFunctions.DrawSphere((planeFirst - planeSecond).abs() / 20.0, (Point3d)intQ);

                                                                Pair<string, PromptStatus> strOpt =
                                                                GlobalFunctions.GetKey(new string[] { "Yes", "No" }, 1, "\nDrawing line to intersection point ?");
                                                                if ((strOpt.Second == PromptStatus.OK) && (strOpt.First == "Yes"))
                                                                {
                                                                    quaternion p = ((lineFirstPoint - intQ).abs() < (lineSecondPoint - intQ).abs()) ? lineFirstPoint : lineSecondPoint;
                                                                    ObjectId lTd = GlobalFunctions.DrawLine((Point3d)intQ, (Point3d)p, 6);

                                                                }
                                                                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("REDRAW ", true, false, false);
                                                            }
                                                            else
                                                                MessageBox.Show("The Line is parallel to plane !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        }
                                                        else
                                                            MessageBox.Show("The second point lying  in plane (intersect plane) !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                    }
                                                    else
                                                        MessageBox.Show("The first Point from Line is equal to second !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                            else
                                                MessageBox.Show("The first point lying  in plane (intersect plane) !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }//

                                        ds = GlobalFunctions.GetKey(new string[] { "Yes", "No" }, 1, "\nYou will choose other Line ?");

                                    } while (ds.Second == PromptStatus.OK && ds.First == "Yes");
                                }
                                else
                                    MessageBox.Show("Two of the points defining the plane are equal !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                            MessageBox.Show("Two of the points defining the plane are equal !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        //projection of the point on the plane
        [CommandMethod("KojtoCAD_3D", "KCAD_PROJECT_POINT_TO_PLANE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CROSS_LINES.htm", "")]
        public void KojtoCAD_3D_Projection_Point_To_Plane()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                Pair<Point3d, PromptStatus> planeFirstSelection =
                         GlobalFunctions.GetPoint(new Point3d(), "\nSelect the first point of the plane:");
                if (planeFirstSelection.Second == PromptStatus.OK)
                {
                    Point3d planeFirstPoint = planeFirstSelection.First;
                    quaternion planeFirst = new quaternion(planeFirstPoint);

                    Pair<Point3d, PromptStatus> planeSecondSelection =
                        GlobalFunctions.GetPoint(new Point3d(), "\nSelect the second point of the plane:");
                    if (planeSecondSelection.Second == PromptStatus.OK)
                    {
                        Point3d planeSecondPoint = planeSecondSelection.First;
                        quaternion planeSecond = new quaternion(planeSecondPoint);

                        if (planeSecondPoint != planeFirstPoint)
                        {
                            Pair<Point3d, PromptStatus> planeThirdSelection =
                                 GlobalFunctions.GetPoint(new Point3d(), "\nSelect the third point of the plane:");
                            if (planeThirdSelection.Second == PromptStatus.OK)
                            {
                                Point3d planeThirdPoint = planeThirdSelection.First;
                                quaternion planeThird = new quaternion(planeThirdPoint);

                                if ((planeThirdPoint != planeFirstPoint) && (planeSecondPoint != planeThirdPoint))
                                {
                                    UCS UCS = new UCS(planeFirst, planeSecond, planeThird);

                                    double check1 = ((planeThird - planeSecond) / (planeThird - planeFirst)).absV();
                                    if (check1 > Constants.zero_dist)
                                    {
                                        Pair<string, PromptStatus> ds = new Pair<string, PromptStatus>("No", PromptStatus.Error);
                                        do
                                        {

                                            Pair<Point3d, PromptStatus> lineFirstSelection =
                                                 GlobalFunctions.GetPoint(new Point3d(), "\nSelect the Point :");
                                            if (lineFirstSelection.Second == PromptStatus.OK)
                                            {
                                                Point3d lineFirstPoint = lineFirstSelection.First;
                                                quaternion lineFirst = new quaternion(lineFirstPoint);


                                                quaternion lineFirst1 = UCS.FromACS(lineFirst);

                                                quaternion intQ = UCS.ToACS(new quaternion(0, lineFirst1.GetX(), lineFirst1.GetY(), 0));

                                                double dist = (lineFirst - intQ).abs();//= lineFirst1.GetZ()

                                                MessageBox.Show("Distance = " + dist.ToString(), "Message");

                                                Pair<Double, PromptStatus> mark =
                                                    GlobalFunctions.GetDouble((dist / 20.0 > 2.0) ? dist / 20.0 : 3.0, "\nLength of the radius of the Mark ?");

                                                if (mark.Second == PromptStatus.OK)
                                                {

                                                    Matrix3d mat = new Matrix3d(UCS.GetAutoCAD_Matrix3d());


                                                    GlobalFunctions.DrawCircle(mark.First, (Point3d)UCS.FromACS(intQ), ref mat);
                                                    GlobalFunctions.DrawSphere(mark.First, (Point3d)intQ);

                                                    if (dist > mark.First)
                                                    {
                                                        GlobalFunctions.DrawLine((Point3d)intQ, (Point3d)lineFirst);
                                                    }
                                                }
                                            }

                                            ds = GlobalFunctions.GetKey(new string[] { "Yes", "No" }, 1, "\nYou will choose other Point ?");
                                        }
                                        while (ds.Second == PromptStatus.OK && ds.First == "Yes");
                                    }//
                                    else
                                        MessageBox.Show("The three points lie on a Line !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                }//
                                else
                                    MessageBox.Show("Two of the points defining the plane are equal !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                            MessageBox.Show("Two of the points defining the plane are equal !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_ANGLE_BETWEEN_TRIANGLES", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CROSS_LINES.htm", "")]
        public void KojtoCAD_3D_Angle_between_adjacent_triangles()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
                {
                    Pair<Point3d, PromptStatus> bendFirstSelection =
                         GlobalFunctions.GetPoint(new Point3d(), "\nSelect the FIRST point of the common edge:");
                    if (bendFirstSelection.Second == PromptStatus.OK)
                    {
                        Point3d bendFirstPoint = bendFirstSelection.First;
                        quaternion bendFirst = new quaternion(bendFirstPoint);

                        Pair<Point3d, PromptStatus> bendSecondSelection =
                                 GlobalFunctions.GetPoint(new Point3d(), "\nSelect the SECOND point of the common edge:");
                        if (bendSecondSelection.Second == PromptStatus.OK)
                        {
                            Point3d bendSecondPoint = bendSecondSelection.First;
                            quaternion bendSecond = new quaternion(bendSecondPoint);

                            Pair<quaternion, quaternion> pb = new Pair<quaternion, quaternion>(bendFirst, bendSecond);
                            WorkClasses.Bend BEND = null;
                            foreach (WorkClasses.Bend bend in container.Bends)
                            {
                                if (bend == pb)
                                {
                                    BEND = bend;
                                    break;
                                }
                            }
                            if ((object)BEND != null)
                            {
                                string convex = "Planar";

                                int convexI = container.IsBendConvex(BEND);
                                if (convexI != 0)
                                    convex = (convexI == 1) ? "Convex" : "Concave";

                                {
                                    WorkClasses.Triangle TR1 = container.Triangles[BEND.FirstTriangleNumer];
                                    //WorkClasses.Triangle TR2 = container.Triangles[BEND.SecondTriangleNumer];
                                    quaternion nTR1 = (TR1.Normal.Second - TR1.Normal.First);
                                    nTR1 /= nTR1.abs();
                                    nTR1 *= 100.0;

                                    quaternion nBEND = BEND.Normal - BEND.MidPoint;
                                    nBEND /= nBEND.abs();
                                    nBEND *= 100.0;

                                    double ang = nBEND.angTo(nTR1);
                                    if ((nBEND - nTR1).abs() <= Constants.zero_angular_difference)
                                    {
                                        ang = 0.0;
                                    }

                                    ang = ang * 180.0 / Math.PI;
                                    if (BEND.SecondTriangleNumer >= 0)// not pereferial
                                        MessageBox.Show(string.Format("Type: {0}\nAngle: {1:f4} degree", convex, ang * 2), "Message");
                                    else
                                        MessageBox.Show(string.Format("Type: {0}\nAngle: {1:f4} degree", convex, ang), "Message");
                                }
                            }
                        }

                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_MIN_DISTANCE_BETWEEN_SOLID3D", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/MINIMAL_DISTANCE_BETWEEN_SOLIDS3D.htm", "")]
        public void KojtoCAD_Mdist3D_Method()
        {
#if !bcad
            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                PromptEntityOptions peo = new PromptEntityOptions("Select a First 3D solid");
                peo.SetRejectMessage("\nA 3D solid must be selected.");
                peo.AddAllowedClass(typeof(Solid3d), true);
                PromptEntityResult per = ed.GetEntity(peo);
                if (per.Status != PromptStatus.OK) return;

                PromptEntityOptions PEO = new PromptEntityOptions("Select a Second 3D solid");
                PEO.SetRejectMessage("\nA 3D solid must be selected.");
                PEO.AddAllowedClass(typeof(Solid3d), true);
                PromptEntityResult PER = ed.GetEntity(PEO);
                if (PER.Status != PromptStatus.OK) return;

                PromptIntegerOptions pIntOpts = new PromptIntegerOptions("\nnumber of Iterations: ");
                pIntOpts.AllowZero = false;
                pIntOpts.AllowNegative = false;
                pIntOpts.DefaultValue = 15;

                PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                if (pIntRes.Status != PromptStatus.OK) return;


                if (per.ObjectId != PER.ObjectId)
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead, false);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false);

                        Solid3d sol1 = tr.GetObject(per.ObjectId, OpenMode.ForWrite) as Solid3d;
                        Solid3d sol2 = tr.GetObject(PER.ObjectId, OpenMode.ForWrite) as Solid3d;

                        if (!sol1.CheckInterference(sol2))
                        {
                            List<quaternion> listS1 = GetPointsOfTheOuterSurfaceAsCollection(tr, ref bt, ref btr, ref sol1, 100, UInt32.MaxValue);
                            List<quaternion> listS2 = GetPointsOfTheOuterSurfaceAsCollection(tr, ref bt, ref btr, ref sol2, 100, UInt32.MaxValue);

                            double distMIN = (listS1[0] - listS2[0]).abs();
                            quaternion mQ1_list1 = listS1[0];
                            quaternion mQ2_list2 = listS2[0];


                            double dist = 0.0;
                            foreach (quaternion Q in listS1)
                            {
                                foreach (quaternion q in listS2)
                                {
                                    dist = (Q - q).abs();
                                    if (dist < distMIN)
                                    {
                                        distMIN = dist;
                                        mQ1_list1 = Q;
                                        mQ2_list2 = q;
                                    }
                                }
                            }

                            quaternion bak1 = mQ1_list1;
                            quaternion bak2 = mQ2_list2;

                            try
                            {
                                GeRezult(pIntRes.Value, tr, ref bt, ref btr, ref mQ1_list1, ref mQ2_list2, ref sol1, ref sol2);
                            }
                            catch
                            {
                                try
                                {
                                    mQ1_list1 = bak1;
                                    mQ2_list2 = bak2;
                                    GeRezult(pIntRes.Value, tr, ref bt, ref btr, ref mQ1_list1, ref mQ2_list2, ref sol1, ref sol2);
                                }
                                catch
                                {
                                    mQ1_list1 = bak1;
                                    mQ2_list2 = bak2;
                                    GeRezult(pIntRes.Value, tr, ref bt, ref btr, ref mQ1_list1, ref mQ2_list2, ref sol1, ref sol2);
                                }
                            }

                            MessageBox.Show(string.Format("Distance = {0}", (mQ1_list1 - mQ2_list2).abs()), "Distance");

                            Line l1_ = new Line((Point3d)mQ1_list1, (Point3d)mQ2_list2);
                            l1_.ColorIndex = 3;
                            btr.AppendEntity(l1_);
                            tr.AddNewlyCreatedDBObject(l1_, true);

                            Solid3d sph = new Solid3d();
                            sph.CreateSphere((mQ1_list1 - mQ2_list2).abs() / 2.0);
                            sph.ColorIndex = 1;
                            sph.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo((Point3d)((mQ1_list1 + mQ2_list2) / 2.0))));
                            btr.AppendEntity(sph);
                            tr.AddNewlyCreatedDBObject(sph, true);


                        }

                        tr.Commit();
                    }
                }

            }
            catch { }
            finally
            {
                ed.CurrentUserCoordinateSystem = old;
            }
#else
            MessageBox.Show("Not available for BricsCAD", "Warning", MessageBoxButtons.OK);
#endif
        }
        #region Mdist3D_Method: functions
#if !bcad
        public List<quaternion> GetPointsOfTheOuterSurfaceAsCollection(Transaction tr, ref  BlockTable bt, ref BlockTableRecord btr, ref Solid3d sol, double MaxNodeSpacing, uint MaxSubdivisions)
        {
            List<quaternion> listREZ = new List<quaternion>();

            double length = sol.GeometricExtents.MinPoint.GetVectorTo(sol.GeometricExtents.MaxPoint).Length;

            try
            {
                using (Brep brp = new Brep(sol))
                {
                    using (Mesh2dControl mc = new Mesh2dControl())
                    {
                        //mc.MaxNodeSpacing = length / 10;
                        mc.MaxNodeSpacing = length / MaxNodeSpacing;
                        //mc.MaxSubdivisions = 100000000;
                        mc.MaxSubdivisions = MaxSubdivisions;

                        //MessageBox.Show((mc.AngleTolerance*180.0/Math.PI).ToString());

                        using (Mesh2dFilter mf = new Mesh2dFilter())
                        {
                            mf.Insert(brp, mc);
                            using (Mesh2d m = new Mesh2d(mf))
                            {
                                foreach (Element2d e in m.Element2ds)
                                {
                                    quaternion cen = new quaternion();
                                    int counter = 0;

                                    Point3dCollection pts = new Point3dCollection();
                                    foreach (Node n in e.Nodes)
                                    {
                                        //listREZ.Add((quaternion)n.Point);
                                        pts.Add(n.Point);
                                        cen += new quaternion(0, n.Point.X, n.Point.Y, n.Point.Z);
                                        counter++;
                                        n.Dispose();
                                    }
                                    cen /= counter;
                                    listREZ.Add(cen);

                                    e.Dispose();

                                    /*
                                    AcDb.Face face = null;

                                    if (pts.Count == 3)
                                        face =  new AcDb.Face(pts[0], pts[1], pts[2],true, true, true, true);

                                    else if (pts.Count == 4)
                                        face = new AcDb.Face(pts[0], pts[1], pts[2], pts[3],true, true, true, true);
                                    // If we have a valid face, add it to the
                                    // database and the transaction

                                    if (face != null)
                                    {
                                        // Make each face yellow for visibility
                                        face.ColorIndex = 2;
                                        btr.AppendEntity(face);
                                        tr.AddNewlyCreatedDBObject(face, true);
                                    }*/
                                }
                            }
                        }
                    }
                }
                //tr.Commit();
            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Exception: {0}", ex.Message);
            }

            return listREZ;
        }
#endif
        public double OffsetSphere(Transaction tr, ref  BlockTable bt, ref BlockTableRecord btr, ref  Solid3d sphere, ref Solid3d solid, quaternion cen, double startR, double step)
        {
            double rez = -1.0;

            if ((sphere == null) || (sphere.IsDisposed))
            {
                sphere = new Solid3d();
                try
                {
                    sphere.CreateSphere(startR);
                    sphere.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo((Point3d)cen)));
                }
                catch
                {
                    rez = startR;
                    sphere.Dispose();
                }
            }
            // btr.AppendEntity(sphere);
            // tr.AddNewlyCreatedDBObject(sphere, true);

            if (rez < 0)
            {
                double len = startR;
                do
                {
                    sphere.OffsetBody(step);
                    len += step;
                }
                while (!sphere.CheckInterference(solid));


                rez = len;
            }
            else
                rez = -1.0;
            return rez;
        }

        #if !bcad
        public Point3d GetTangentPoint(Transaction tr, ref  BlockTable bt, ref BlockTableRecord btr, ref  Solid3d sphere, ref Solid3d solid, quaternion cen, double startR, double step)
        {

            double R = OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 10);
            if (R < 0.0)
                OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 10);
            sphere.OffsetBody(-10);
            startR = R - 10;


            R = OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 1);
            if (R < 0.0)
                OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 1);
            sphere.OffsetBody(-1);
            startR = R - 1;


            R = OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 0.1);
            if (R < 0.0)
                OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 0.1);
            sphere.OffsetBody(-0.1);
            startR = R - 0.1;


            R = OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 0.01);
            if (R < 0.0)
                OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 0.01);
            sphere.OffsetBody(-0.01);
            startR = R - 0.01;


            R = OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 0.004);
            if (R < 0.0)
                OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 0.004);
            sphere.OffsetBody(-0.004);
            startR = R - 0.004;

            R = OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 0.002);
            if (R < 0.0)
                OffsetSphere(tr, ref bt, ref btr, ref sphere, ref solid, cen, startR, 0.002);
            Solid3d solid_ = solid.Clone() as Solid3d;
            try
            {
                sphere.BooleanOperation(BooleanOperationType.BoolIntersect, solid_);
            }
            catch
            {//       
            }


            List<quaternion> listS1 = GetPointsOfTheOuterSurfaceAsCollection(tr, ref bt, ref btr, ref sphere, 10, UInt32.MaxValue);


            quaternion w = new quaternion();
            foreach (quaternion q in listS1) w += q;
            w /= listS1.Count;

            sphere.Dispose();

            return (Point3d)w;
        }
   
        public void GeRezult(int iter, Transaction tr, ref  BlockTable bt, ref BlockTableRecord btr, ref quaternion mQ1_list1, ref quaternion mQ2_list2, ref Solid3d sol1, ref Solid3d sol2)
        {
            Solid3d sphere = null;
            for (int i = 0; i < iter; i++)
            {
                mQ2_list2 = GetTangentPoint(tr, ref bt, ref btr, ref sphere, ref sol2, mQ1_list1, (mQ2_list2 - mQ1_list1).abs() / 2.0, (mQ2_list2 - mQ1_list1).abs() / 20.0);
                sphere.Dispose();
                mQ1_list1 = GetTangentPoint(tr, ref bt, ref btr, ref sphere, ref sol1, mQ2_list2, (mQ2_list2 - mQ1_list1).abs() / 2.0, (mQ2_list2 - mQ1_list1).abs() / 20.0);
                sphere.Dispose();
            }
            /*
            mQ2_list2 = GetTangentPoint(tr, ref bt, ref btr, ref sphere, ref sol2, mQ1_list1, (mQ2_list2 - mQ1_list1).abs() / 2.0, (mQ2_list2 - mQ1_list1).abs() / 20.0);
            mQ1_list1 = GetTangentPoint(tr, ref bt, ref btr, ref sphere, ref sol1, mQ2_list2, (mQ2_list2 - mQ1_list1).abs() / 2.0, (mQ2_list2 - mQ1_list1).abs() / 20.0);

            mQ2_list2 = GetTangentPoint(tr, ref bt, ref btr, ref sphere, ref sol2, mQ1_list1, (mQ2_list2 - mQ1_list1).abs() / 2.0, (mQ2_list2 - mQ1_list1).abs() / 20.0);
            mQ1_list1 = GetTangentPoint(tr, ref bt, ref btr, ref sphere, ref sol1, mQ2_list2, (mQ2_list2 - mQ1_list1).abs() / 2.0, (mQ2_list2 - mQ1_list1).abs() / 20.0);

            mQ2_list2 = GetTangentPoint(tr, ref bt, ref btr, ref sphere, ref sol2, mQ1_list1, (mQ2_list2 - mQ1_list1).abs() / 2.0, (mQ2_list2 - mQ1_list1).abs() / 20.0);
            mQ1_list1 = GetTangentPoint(tr, ref bt, ref btr, ref sphere, ref sol1, mQ2_list2, (mQ2_list2 - mQ1_list1).abs() / 2.0, (mQ2_list2 - mQ1_list1).abs() / 20.0);
        */
        }
#endif
        #endregion

        //intersection line between two planes
        [CommandMethod("KojtoCAD_3D", "KCAD_INTERSECTION_LINE_BETWEEN_TWO_PLANES", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CROSS_LINES.htm", "")]
        public void KojtoCAD_Intersection_Line_between_two_Planes()
        {
            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {
                #region First Plane Define
                Pair<Point3d, PromptStatus> firstPlane_firstPoint_Selection =
                          GlobalFunctions.GetPoint(new Point3d(), "\nSelect first Point from first Plane: ");
                if (firstPlane_firstPoint_Selection.Second != PromptStatus.OK) return;

                Pair<Point3d, PromptStatus> firstPlane_secondPoint_Selection =
                          GlobalFunctions.GetPoint(new Point3d(), "\nSelect second Point from first Plane: ");
                if (firstPlane_secondPoint_Selection.Second != PromptStatus.OK) return;

                Pair<Point3d, PromptStatus> firstPlane_thirdPoint_Selection =
                     GlobalFunctions.GetPoint(new Point3d(), "\nSelect third Point from first Plane: ");
                if (firstPlane_thirdPoint_Selection.Second != PromptStatus.OK) return;
                #endregion


                quaternion fPfP = (quaternion)firstPlane_firstPoint_Selection.First;
                quaternion fPsP = (quaternion)firstPlane_secondPoint_Selection.First;
                quaternion fPtP = (quaternion)firstPlane_thirdPoint_Selection.First;

                #region chek points
                if ((fPfP - fPsP).abs() <= Constants.zero_dist)
                {
                    MessageBox.Show("First Plane\n\ncoincident: first and second point !", "E R R O R");
                    return;
                }
                if ((fPfP - fPtP).abs() <= Constants.zero_dist)
                {
                    MessageBox.Show("First Plane\n\ncoincident: first and third point !", "E R R O R");
                    return;
                }
                if ((fPsP - fPtP).abs() <= Constants.zero_dist)
                {
                    MessageBox.Show("First Plane\n\ncoincident: second and third point !", "E R R O R");
                    return;
                }

                quaternion Q = (fPfP - fPsP) / (fPfP - fPtP);

                if (Q.absV() < Constants.zero_dist)
                {
                    MessageBox.Show("First Plane\n\nThe three points are collinear (not define plane) or are coincident", "E R R O R");
                    return;
                }



                #endregion

                plane fPlane = new plane(fPfP, fPsP, fPtP);
                quaternion fPlaneNormal = fPlane.GetNormal();

                #region Secon Plane Define
                Pair<Point3d, PromptStatus> secondPlane_firstPoint_Selection =
                          GlobalFunctions.GetPoint(new Point3d(), "\nSelect first Point from second Plane: ");
                if (secondPlane_firstPoint_Selection.Second != PromptStatus.OK) return;

                Pair<Point3d, PromptStatus> secondPlane_secondPoint_Selection =
                          GlobalFunctions.GetPoint(new Point3d(), "\nSelect second Point from second Plane: ");
                if (secondPlane_secondPoint_Selection.Second != PromptStatus.OK) return;

                Pair<Point3d, PromptStatus> secondPlane_thirdPoint_Selection =
                     GlobalFunctions.GetPoint(new Point3d(), "\nSelect third Point from second Plane: ");
                if (secondPlane_thirdPoint_Selection.Second != PromptStatus.OK) return;
                #endregion

                quaternion sPfP = (quaternion)secondPlane_firstPoint_Selection.First;
                quaternion sPsP = (quaternion)secondPlane_secondPoint_Selection.First;
                quaternion sPtP = (quaternion)secondPlane_thirdPoint_Selection.First;

                if (Math.Abs(fPlane.dist(sPfP)) < Math.Abs(fPlane.dist(sPsP))) { quaternion buff = sPfP; sPfP = sPsP; sPsP = buff; }
                if (Math.Abs(fPlane.dist(sPfP)) < Math.Abs(fPlane.dist(sPtP))) { quaternion buff = sPfP; sPfP = sPtP; sPtP = buff; }

                #region chek points
                if ((sPfP - sPsP).abs() <= Constants.zero_dist)
                {
                    MessageBox.Show("Second Plane !\n\ncoincident: first and second point !", "E R R O R");
                    return;
                }
                if ((sPfP - sPtP).abs() <= Constants.zero_dist)
                {
                    MessageBox.Show("Second Plane !\n\ncoincident: first and third point !", "E R R O R");
                    return;
                }
                if ((sPsP - sPtP).abs() <= Constants.zero_dist)
                {
                    MessageBox.Show("Second Plane !\n\ncoincident: second and third point !", "E R R O R");
                    return;
                }

                quaternion QQ = (sPfP - sPsP) / (sPfP - sPtP);

                if (QQ.absV() < Constants.zero_dist)
                {
                    MessageBox.Show("Second Plane !\n\nThe three points are collinear (not define plane) or are coincident", "E R R O R");
                    return;
                }



                #endregion

                #region chek planes
                UCS ucs = new UCS(fPfP, fPsP, fPtP);

                double h1 = ucs.FromACS(sPfP).GetZ() * 10000;
                double h2 = ucs.FromACS(sPsP).GetZ() * 10000;
                double h3 = ucs.FromACS(sPtP).GetZ() * 10000;

                bool b1 = Math.Abs(h1 - h2) <= Constants.zero_dist;
                bool b2 = Math.Abs(h1 - h3) <= Constants.zero_dist;
                bool b3 = Math.Abs(h3 - h2) <= Constants.zero_dist;

                if (b1 && b2 && b3)
                {
                    MessageBox.Show("Planes are parallel !", "E R R O R");
                    return;
                }

                #endregion

                plane sPlane = new plane(sPfP, sPsP, sPtP);
                quaternion sPlaneNormal = sPlane.GetNormal();

                Pair<quaternion, quaternion> L1 = new Pair<quaternion, quaternion>(sPfP, sPsP);
                Pair<quaternion, quaternion> L2 = new Pair<quaternion, quaternion>(sPfP, sPtP);

                if (b1)// line sPfP,sPsP is parallel to first plane
                {
                    L1 = new Pair<quaternion, quaternion>((sPsP + sPfP) / 2.0, (sPtP + sPfP) / 2.0);
                    L2 = new Pair<quaternion, quaternion>((sPsP + sPfP) / 2.0, (sPtP + sPsP) / 2.0);
                }

                if (b2)// line sPfP, sPtP parallel to first plane
                {
                    L1 = new Pair<quaternion, quaternion>((sPtP + sPfP) / 2.0, (sPtP + sPsP) / 2.0);
                    L2 = new Pair<quaternion, quaternion>((sPtP + sPfP) / 2.0, (sPfP + sPsP) / 2.0);
                }

                /*
                if (b3)
                {
                    //L1 = new PRE_BEND((sPtP + sPsP)/2.0, (sPfP+sPsP)/2.0);
                    //L2 = new PRE_BEND((sPtP + sPsP) / 2.0, (sPfP + sPtP) / 2.0);
                }*/

                quaternion M1 = fPlane.IntersectWithVector(L1.First, L1.Second);
                quaternion M2 = fPlane.IntersectWithVector(L2.First, L2.Second);

                int colrIndex = 1;
                Pair<int, PromptStatus> colorIndexprompt = GlobalFunctions.GetInt(colrIndex, "\nColor to draw the intersection ?");
                if (colorIndexprompt.Second == PromptStatus.OK)
                    colrIndex = colorIndexprompt.First;

                GlobalFunctions.DrawLine((Point3d)M1, (Point3d)M2, colrIndex);

                Pair<string, PromptStatus> facesPrompt =
                GlobalFunctions.GetKey(new string[] { "Yes", "No" }, 1, "\nShow Planes as Faces ?");

                if ((facesPrompt.Second == PromptStatus.OK) && (facesPrompt.First == "Yes"))
                {
                    Transaction tr = db.TransactionManager.StartTransaction();
                    using (tr)
                    {

                        Face face1 = new Face((Point3d)fPfP, (Point3d)fPsP, (Point3d)fPtP, true, true, true, true);
                        face1.ColorIndex = colrIndex + 1;
                        Face face2 = new Face((Point3d)sPfP, (Point3d)sPsP, (Point3d)sPtP, true, true, true, true);
                        face2.ColorIndex = colrIndex + 2;

                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead, false);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false);

                        btr.AppendEntity(face1);
                        tr.AddNewlyCreatedDBObject(face1, true);

                        btr.AppendEntity(face2);
                        tr.AddNewlyCreatedDBObject(face2, true);

                        tr.Commit();
                    }
                }

                double ang1 = Math.Abs(sPlaneNormal.angTo(fPlaneNormal));
                double ang2 = Math.PI - ang1;

                ang1 *= (180.0 / Math.PI); ang2 *= (180.0 / Math.PI);

                if (ang1 > ang2)
                {
                    double buff = ang1; ang1 = ang2; ang2 = buff;
                }

                string mess = string.Format("\n\nSmall angle: {0} degree\nLarge angle: {1} degree\n", ang1, ang2);
                ed.WriteMessage(mess);
                MessageBox.Show(mess, "Angle between two planes: ");

                // MathLibKCAD.plane sPlane = new plane(sPfP, sPsP, sPtP);
                // quaternion[] M = fPlane.IntersectWithPlane(sPlane);                           

                //UtilityClasses.GlobalFunctions.DrawLine((Point3d)M[0], (Point3d)M[1], 1);     
            }
            catch { }
            finally
            {
                ed.CurrentUserCoordinateSystem = old;
            }
        }
    }
}
