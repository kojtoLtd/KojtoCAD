using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
using KojtoCAD.KojtoCAD3D.WorkClasses;

#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Teigha.Colors;
using Teigha.BoundaryRepresentation;
using Face = Teigha.DatabaseServices.Face;
using Application = Bricscad.ApplicationServices.Application;
using Window = Bricscad.Windows.Window;
#endif
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.GetMeshItems))]

namespace KojtoCAD.KojtoCAD3D
{
    public class GetMeshItems
    {
        public Containers container = ContextVariablesProvider.Container;

        //Get Triangle By Selection
        [CommandMethod("KojtoCAD_3D", "KCAD_GET_TRIANGLE_BY_SELECTION", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Get_Triangle_By_Selection()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");

                    // Prompt for the start point
                    pPtOpts.Message = "\nEnter the first Point of the Triangle: ";
                    pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                    if (pPtRes.Status == PromptStatus.OK)
                    {
                        Point3d ptFirst = pPtRes.Value;

                        // Prompt for the second point
                        pPtOpts.Message = "\nEnter the second Point of the Triangle: ";
                        pPtOpts.UseBasePoint = true;
                        pPtOpts.BasePoint = ptFirst;
                        pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                        if (pPtRes.Status == PromptStatus.OK)
                        {
                            Point3d ptSecond = pPtRes.Value;
                            if (ptSecond.DistanceTo(ptFirst) >= ConstantsAndSettings.MinBendLength)
                            {
                                // Prompt for the third point
                                pPtOpts.Message = "\nEnter the third Point of the Triangle: ";
                                pPtOpts.UseBasePoint = true;
                                pPtOpts.BasePoint = ptFirst;
                                pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                                if (pPtRes.Status == PromptStatus.OK)
                                {
                                    Point3d ptThird = pPtRes.Value;
                                    if ((ptSecond.DistanceTo(ptThird) >= ConstantsAndSettings.MinBendLength) &&
                                        (ptFirst.DistanceTo(ptThird) >= ConstantsAndSettings.MinBendLength))
                                    {
                                        Triplet<quaternion, quaternion, quaternion> pTR = new Triplet<quaternion, quaternion, quaternion>(new quaternion(0, ptFirst.X, ptFirst.Y, ptFirst.Z),
                                            new quaternion(0, ptSecond.X, ptSecond.Y, ptSecond.Z),
                                            new quaternion(0, ptThird.X, ptThird.Y, ptThird.Z));
                                        Triangle TR = null;
                                        foreach (Triangle tr in container.Triangles)
                                        {
                                            if (tr == pTR)
                                            {
                                                TR = tr;
                                                break;
                                            }
                                        }
                                        if ((object)TR != null)
                                        {
                                            using (Transaction tr = db.TransactionManager.StartTransaction())
                                            {
                                                BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                if (MessageBox.Show("Numer: " + (TR.Numer + 1).ToString() +
                                                    "\n\nYES - Line through the Triangle ?\nNO - Sphere in Triangle ?",
                                                    " S E L E C T I O N  Triangle", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                                {
                                                    quaternion lq = (TR.Normal.Second - TR.Normal.First) * 100.0 * container.Bends[TR.GetFirstBendNumer()].Length + TR.Normal.First;
                                                    Line l = new Line(new Point3d(TR.Normal.First.GetX(), TR.Normal.First.GetY(), TR.Normal.First.GetZ()), new Point3d(lq.GetX(), lq.GetY(), lq.GetZ()));

                                                    acBlkTblRec.AppendEntity(l);
                                                    tr.AddNewlyCreatedDBObject(l, true);
                                                }
                                                else
                                                {
                                                    double R = (TR.Normal.First - container.Bends[TR.GetFirstBendNumer()].MidPoint).abs();
                                                    R /= 3.0;

                                                    Solid3d acSol3D = new Solid3d();
                                                    acSol3D.SetDatabaseDefaults();
                                                    acSol3D.CreateSphere(R);
                                                    acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(TR.Normal.First.GetX(), TR.Normal.First.GetY(), TR.Normal.First.GetZ()) - Point3d.Origin));

                                                    acBlkTblRec.AppendEntity(acSol3D);
                                                    tr.AddNewlyCreatedDBObject(acSol3D, true);
                                                }

                                                tr.Commit();
                                                ed.UpdateScreen();
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("\nTriangle not found - E R R O R  !", "E R R O R - Selection Triangle", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            ed.WriteMessage("\nTriangle not found - E R R O R  !");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("\nDistance between selected Points is less - E R R O R  !", "E R R O R - Selection Triangle", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ed.WriteMessage("\nDistance between selected Points is less - E R R O R  !");
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("\nDistance between selected Points is less - E R R O R  !", "E R R O R - Selection Triangle", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ed.WriteMessage("\nDistance between selected Points is less - E R R O R  !");
                            }
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Get Triangle By Numer
        [CommandMethod("KojtoCAD_3D", "KCAD_GET_TRIANGLE_BY_NUMER", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Get_Triangle_By_Numer()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                    pIntOpts.Message = "\nEnter the Triangle Numer ";

                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                    if (pIntRes.Status == PromptStatus.OK)
                    {
                        int N = pIntRes.Value - 1;
                        if ((N >= 0) && (N < container.Triangles.Count))
                        {
                            Triangle TR = container.Triangles[N];
                            if ((object)TR != null)
                            {
                                using (Transaction tr = db.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    if (MessageBox.Show("Numer: " + (TR.Numer + 1).ToString() +
                                        "\n\nYES - Line through the Triangle ?\nNO - Sphere in Triangle ?",
                                        " S E L E C T I O N  Triangle", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                    {
                                        quaternion lq = (TR.Normal.Second - TR.Normal.First) * 100.0 * container.Bends[TR.GetFirstBendNumer()].Length + TR.Normal.First;
                                        Line l = new Line(new Point3d(TR.Normal.First.GetX(), TR.Normal.First.GetY(), TR.Normal.First.GetZ()), new Point3d(lq.GetX(), lq.GetY(), lq.GetZ()));

                                        acBlkTblRec.AppendEntity(l);
                                        tr.AddNewlyCreatedDBObject(l, true);
                                    }
                                    else
                                    {
                                        double R = (TR.Normal.First - container.Bends[TR.GetFirstBendNumer()].MidPoint).abs();
                                        R /= 3.0;

                                        Solid3d acSol3D = new Solid3d();
                                        acSol3D.SetDatabaseDefaults();
                                        acSol3D.CreateSphere(R);
                                        acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(TR.Normal.First.GetX(), TR.Normal.First.GetY(), TR.Normal.First.GetZ()) - Point3d.Origin));

                                        acBlkTblRec.AppendEntity(acSol3D);
                                        tr.AddNewlyCreatedDBObject(acSol3D, true);
                                    }

                                    tr.Commit();
                                    ed.UpdateScreen();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("\nTriangle Numer Range - E R R O R  !", "E R R O R - Selection Triangle", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ed.WriteMessage("\nTriangle Numer Range - E R R O R  !");
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Get Bend By Numer
        [CommandMethod("KojtoCAD_3D", "KCAD_GET_BEND_BY_NUMER", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Get_Bend_By_Numer()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                    pIntOpts.Message = "\nEnter the Bend  Numer ";

                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                    if (pIntRes.Status == PromptStatus.OK)
                    {
                        int N = pIntRes.Value - 1;
                        if ((N >= 0) && (N < container.Bends.Count))
                        {
                            Bend TR = container.Bends[N];
                            if ((object)TR != null)
                            {
                                using (Transaction tr = db.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    if (MessageBox.Show("Numer: " + (TR.Numer + 1).ToString() + "\nFictive: " + ((TR.Fictive) ? "YES" : "NO") +
                                        "\n\nYES - Line through the Bend ?\nNO - Sphere in Bend ?",
                                        " S E L E C T I O N  Bend", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                    {
                                        quaternion lq = (TR.Normal - TR.MidPoint) * 100.0 * TR.Length + TR.MidPoint;
                                        Line l = new Line(new Point3d(TR.MidPoint.GetX(), TR.MidPoint.GetY(), TR.MidPoint.GetZ()), new Point3d(lq.GetX(), lq.GetY(), lq.GetZ()));

                                        acBlkTblRec.AppendEntity(l);
                                        tr.AddNewlyCreatedDBObject(l, true);
                                    }
                                    else
                                    {
                                        double R = TR.Length / 10.0;

                                        Solid3d acSol3D = new Solid3d();
                                        acSol3D.SetDatabaseDefaults();
                                        acSol3D.CreateSphere(R);
                                        acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(TR.MidPoint.GetX(), TR.MidPoint.GetY(), TR.MidPoint.GetZ()) - Point3d.Origin));

                                        acBlkTblRec.AppendEntity(acSol3D);
                                        tr.AddNewlyCreatedDBObject(acSol3D, true);
                                    }

                                    tr.Commit();
                                    ed.UpdateScreen();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("\nBend Numer Range - E R R O R  !", "E R R O R - Selection Bend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ed.WriteMessage("\nBend Numer Range - E R R O R  !");
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void KojtoCAD_3D_Get_BendStart_By_Numer()
        {
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                    pIntOpts.Message = "\nEnter the Bend  Numer ";

                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                    if (pIntRes.Status == PromptStatus.OK)
                    {
                        int N = pIntRes.Value - 1;
                        if ((N >= 0) && (N < container.Bends.Count))
                        {
                            Bend TR = container.Bends[N];
                            if ((object)TR != null)
                            {
                                using (Transaction tr = db.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    if (MessageBox.Show("Numer: " + (TR.Numer + 1).ToString() + "\nFictive: " + ((TR.Fictive) ? "YES" : "NO") +
                                        "\n\nYES - Line through the Bend ?\nNO - Sphere in Bend ?",
                                        " S E L E C T I O N  Bend", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                    {
                                        quaternion lq = (TR.Normal - TR.MidPoint) * 100.0 * TR.Length + TR.MidPoint;
                                        Line l = new Line(new Point3d(TR.MidPoint.GetX(), TR.MidPoint.GetY(), TR.MidPoint.GetZ()), new Point3d(lq.GetX(), lq.GetY(), lq.GetZ()));

                                        acBlkTblRec.AppendEntity(l);
                                        tr.AddNewlyCreatedDBObject(l, true);

                                    }
                                    else
                                    {
                                        double R = TR.Length / 10.0;

                                        Solid3d acSol3D = new Solid3d();
                                        acSol3D.SetDatabaseDefaults();
                                        acSol3D.CreateSphere(R);
                                        acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(TR.MidPoint.GetX(), TR.MidPoint.GetY(), TR.MidPoint.GetZ()) - Point3d.Origin));

                                        acBlkTblRec.AppendEntity(acSol3D);
                                        tr.AddNewlyCreatedDBObject(acSol3D, true);
                                    }

                                    double RR = TR.Length / 10.0;

                                    Solid3d acSolid3D = new Solid3d();
                                    acSolid3D.SetDatabaseDefaults();
                                    acSolid3D.CreateSphere(RR);
                                    acSolid3D.TransformBy(Matrix3d.Displacement(new Point3d(TR.Start.GetX(), TR.Start.GetY(), TR.Start.GetZ()) - Point3d.Origin));

                                    acBlkTblRec.AppendEntity(acSolid3D);
                                    tr.AddNewlyCreatedDBObject(acSolid3D, true);

                                    tr.Commit();
                                    ed.UpdateScreen();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("\nBend Numer Range - E R R O R  !", "E R R O R - Selection Bend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ed.WriteMessage("\nBend Numer Range - E R R O R  !");
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Get Bend By two Points
        [CommandMethod("KojtoCAD_3D", "KCAD_GET_BEND_BY_SELECTION", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Get_Bend_By_Selection()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");


                    // Prompt for the first point
                    pPtOpts.Message = "\nEnter the first Point of the Bend: ";
                    pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                    if (pPtRes.Status == PromptStatus.OK)
                    {
                        Point3d ptFirst = pPtRes.Value;
                        // Prompt for the second point
                        pPtOpts.Message = "\nEnter the second Point of the Bend: ";
                        pPtOpts.UseBasePoint = true;
                        pPtOpts.BasePoint = ptFirst;
                        pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                        if (pPtRes.Status == PromptStatus.OK)
                        {
                            Point3d ptSecond = pPtRes.Value;
                            if (ptSecond.DistanceTo(ptFirst) >= ConstantsAndSettings.MinBendLength)
                            {
                                Pair<quaternion, quaternion> pb = new Pair<quaternion, quaternion>(new quaternion(0, ptFirst.X, ptFirst.Y, ptFirst.Z), new quaternion(0, ptSecond.X, ptSecond.Y, ptSecond.Z));
                                Bend TR = null;
                                foreach (Bend bend in container.Bends)
                                {
                                    if (bend == pb)
                                    {
                                        TR = bend;
                                        break;
                                    }
                                }
                                if ((object)TR != null)
                                {
                                    using (Transaction tr = db.TransactionManager.StartTransaction())
                                    {
                                        BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                        if (MessageBox.Show("Numer: " + (TR.Numer + 1).ToString() + "\nFictive: " + ((TR.Fictive) ? "YES" : "NO") +
                                            "\n\nYES - Line through the Bend ?\nNO - Sphere in Bend ?",
                                            " S E L E C T I O N  Triangle", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            quaternion lq = (TR.Normal - TR.MidPoint) * 100.0 * TR.Length + TR.MidPoint;
                                            Line l = new Line(new Point3d(TR.MidPoint.GetX(), TR.MidPoint.GetY(), TR.MidPoint.GetZ()), new Point3d(lq.GetX(), lq.GetY(), lq.GetZ()));

                                            acBlkTblRec.AppendEntity(l);
                                            tr.AddNewlyCreatedDBObject(l, true);
                                        }
                                        else
                                        {
                                            double R = TR.Length / 10.0;

                                            Solid3d acSol3D = new Solid3d();
                                            acSol3D.SetDatabaseDefaults();
                                            acSol3D.CreateSphere(R);
                                            acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(TR.MidPoint.GetX(), TR.MidPoint.GetY(), TR.MidPoint.GetZ()) - Point3d.Origin));

                                            acBlkTblRec.AppendEntity(acSol3D);
                                            tr.AddNewlyCreatedDBObject(acSol3D, true);
                                        }

                                        tr.Commit();
                                        ed.UpdateScreen();
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("\nBend not found - E R R O R  !", "E R R O R - Selection Bend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ed.WriteMessage("\nBend not found - E R R O R  !");
                                }
                            }

                        }
                        else
                        {
                            MessageBox.Show("\nDistance between selected Points is less - E R R O R  !", "E R R O R - Selection Bend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ed.WriteMessage("\nDistance between selected Points is less - E R R O R  !");
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Get Node By Numer
        [CommandMethod("KojtoCAD_3D", "KCAD_GET_NODE_BY_NUMER", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Get_Node_By_Numer()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                    pIntOpts.Message = "\nEnter the Node Numer ";

                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                    if (pIntRes.Status == PromptStatus.OK)
                    {
                        int N = pIntRes.Value - 1;
                        if ((N >= 0) && (N < container.Nodes.Count))
                        {
                            WorkClasses.Node TR = container.Nodes[N];
                            if ((object)TR != null)
                            {
                                using (Transaction tr = db.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    if (MessageBox.Show("Numer: " + (TR.Numer + 1).ToString() +
                                        "\n\nYES - Line through the Node ?\nNO - Sphere in Node ?",
                                        " S E L E C T I O N  Node", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                    {
                                        quaternion lq = (TR.Normal - TR.Position) * 100.0 * container.Bends[TR.Bends_Numers_Array[0]].Length + TR.Position;
                                        Line l = new Line(new Point3d(TR.Position.GetX(), TR.Position.GetY(), TR.Position.GetZ()), new Point3d(lq.GetX(), lq.GetY(), lq.GetZ()));

                                        acBlkTblRec.AppendEntity(l);
                                        tr.AddNewlyCreatedDBObject(l, true);
                                    }
                                    else
                                    {
                                        double R = container.Bends[TR.Bends_Numers_Array[0]].Length / 10.0;

                                        Solid3d acSol3D = new Solid3d();
                                        acSol3D.SetDatabaseDefaults();
                                        acSol3D.CreateSphere(R);
                                        acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(TR.Position.GetX(), TR.Position.GetY(), TR.Position.GetZ()) - Point3d.Origin));

                                        acBlkTblRec.AppendEntity(acSol3D);
                                        tr.AddNewlyCreatedDBObject(acSol3D, true);
                                    }

                                    tr.Commit();
                                    ed.UpdateScreen();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("\nNode Numer Range - E R R O R  !", "E R R O R - Selection Node", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ed.WriteMessage("\nNode Numer Range - E R R O R  !");
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Get Node By Point
        [CommandMethod("KojtoCAD_3D", "KCAD_GET_NODE_BY_POINT", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Get_Node_By_Selection()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");


                    // Prompt for the first point
                    pPtOpts.Message = "\nEnter the Point of the Node: ";
                    pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                    if (pPtRes.Status == PromptStatus.OK)
                    {
                        quaternion nq = new quaternion(0, pPtRes.Value.X, pPtRes.Value.Y, pPtRes.Value.Z);
                        WorkClasses.Node TR = null;
                        foreach (WorkClasses.Node node in container.Nodes)
                        {
                            if (node == nq)
                            {
                                TR = node;
                                break;
                            }
                        }
                        if ((object)TR != null)
                        {
                            using (Transaction tr = db.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                if (MessageBox.Show("Numer: " + (TR.Numer + 1).ToString() +
                                    "\n\nYES - Line through the Node ?\nNO - Sphere in Node ?",
                                    " S E L E C T I O N  Node", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    quaternion lq = (TR.Normal - TR.Position) * 100.0 * container.Bends[TR.Bends_Numers_Array[0]].Length + TR.Position;
                                    Line l = new Line(new Point3d(TR.Position.GetX(), TR.Position.GetY(), TR.Position.GetZ()), new Point3d(lq.GetX(), lq.GetY(), lq.GetZ()));

                                    acBlkTblRec.AppendEntity(l);
                                    tr.AddNewlyCreatedDBObject(l, true);
                                }
                                else
                                {
                                    double R = container.Bends[TR.Bends_Numers_Array[0]].Length / 10.0;

                                    Solid3d acSol3D = new Solid3d();
                                    acSol3D.SetDatabaseDefaults();
                                    acSol3D.CreateSphere(R);
                                    acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(TR.Position.GetX(), TR.Position.GetY(), TR.Position.GetZ()) - Point3d.Origin));

                                    acBlkTblRec.AppendEntity(acSol3D);
                                    tr.AddNewlyCreatedDBObject(acSol3D, true);
                                }

                                tr.Commit();
                                ed.UpdateScreen();
                            }
                        }
                        else
                        {
                            MessageBox.Show("\nNode not found - E R R O R  !", "E R R O R - Selection Node", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ed.WriteMessage("\nNode not found - E R R O R  !");
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_GET_POLYGON_BY_NUMER", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Get_Polygon_By_Numer()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                    pIntOpts.Message = "\nEnter the Polygon Numer ";

                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                    if (pIntRes.Status == PromptStatus.OK)
                    {
                        if (pIntRes.Value < container.Polygons.Count + 1)
                        {
                            Polygon POL = container.Polygons[pIntRes.Value - 1];
                            using (Transaction trans = db.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                #region draw
                                if (MessageBox.Show("Numer: " + (POL.GetNumer() + 1).ToString() +
                                                                                    "\n\nYES - Glass Regions ?\nNO - Sphere  ?",
                                                                                    " S E L E C T I O N  Polygon", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    //UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_Ex(ref container, POL, 1.0, trans, acBlkTblRec);
                                    Solid3d solid = GlobalFunctions.GetPolygonGlassByheight(ref container, POL, ConstantsAndSettings.Thickness_of_the_Glass, trans, acBlkTblRec);
                                    try
                                    {
                                        acBlkTblRec.AppendEntity(solid);
                                        trans.AddNewlyCreatedDBObject(solid, true);
                                    }
                                    catch { }
                                }
                                else
                                {
                                    quaternion QQ = new quaternion();
                                    foreach (int bN in POL.Bends_Numers_Array)
                                    {
                                        QQ += container.Bends[bN].MidPoint;
                                    }
                                    QQ /= POL.Bends_Numers_Array.Count;

                                    double R = container.Bends[POL.Bends_Numers_Array[0]].Length / 4.0;

                                    Solid3d acSol3D = new Solid3d();
                                    acSol3D.SetDatabaseDefaults();
                                    acSol3D.CreateSphere(R);
                                    acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(QQ.GetX(), QQ.GetY(), QQ.GetZ()) - Point3d.Origin));

                                    acBlkTblRec.AppendEntity(acSol3D);
                                    trans.AddNewlyCreatedDBObject(acSol3D, true);
                                }

                                #endregion

                                trans.Commit();
                                ed.UpdateScreen();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Numer out of Range", "Numer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_GET_POLYGON_BY_SELECTION", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Get_Polygon_By_Selection()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");


                    // Prompt for the first point
                    pPtOpts.Message = "\nEnter the first Point of the Bend: ";
                    pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                    if (pPtRes.Status == PromptStatus.OK)
                    {
                        Point3d ptFirst = pPtRes.Value;
                        // Prompt for the second point
                        pPtOpts.Message = "\nEnter the second Point of the Bend: ";
                        pPtOpts.UseBasePoint = true;
                        pPtOpts.BasePoint = ptFirst;
                        pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                        if (pPtRes.Status == PromptStatus.OK)
                        {
                            Point3d ptSecond = pPtRes.Value;
                            if (ptSecond.DistanceTo(ptFirst) >= ConstantsAndSettings.MinBendLength)
                            {
                                Pair<quaternion, quaternion> pb = new Pair<quaternion, quaternion>(new quaternion(0, ptFirst.X, ptFirst.Y, ptFirst.Z), new quaternion(0, ptSecond.X, ptSecond.Y, ptSecond.Z));
                                Bend BEND = null;
                                foreach (Bend bend in container.Bends)
                                {
                                    if (bend == pb)
                                    {
                                        BEND = bend;
                                        break;
                                    }
                                }
                                if ((object)BEND != null)
                                {
                                    bool exist = false;
                                    foreach (Polygon POL in container.Polygons)
                                    {
                                        bool show = false;
                                        foreach (int numerTR in POL.Triangles_Numers_Array)
                                        {
                                            Triangle TR = container.Triangles[numerTR];
                                            if (!show && ((TR.Bends[0].First == BEND.Numer) || (TR.Bends[1].First == BEND.Numer) || (TR.Bends[2].First == BEND.Numer)))
                                            {
                                                exist = true;
                                                using (Transaction trans = db.TransactionManager.StartTransaction())
                                                {
                                                    BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                    BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                                                    #region draw
                                                    if (MessageBox.Show("Numer: " + (POL.GetNumer() + 1).ToString() +
                                                                                                        "\n\nYES - Glass Regions ?\nNO - Sphere  ?",
                                                                                                        " S E L E C T I O N  Polygon", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                                    {
                                                        //UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_Ex(ref container, POL, 1.0, trans, acBlkTblRec);
                                                        Solid3d solid = GlobalFunctions.GetPolygonGlassByheight(ref container, POL, ConstantsAndSettings.Thickness_of_the_Glass, trans, acBlkTblRec);
                                                        acBlkTblRec.AppendEntity(solid);
                                                        trans.AddNewlyCreatedDBObject(solid, true);
                                                        show = true;
                                                    }
                                                    else
                                                    {
                                                        quaternion QQ = new quaternion();
                                                        foreach (int bN in POL.Bends_Numers_Array)
                                                        {
                                                            QQ += container.Bends[bN].MidPoint;
                                                        }
                                                        QQ /= POL.Bends_Numers_Array.Count;

                                                        double R = BEND.Length / 10.0;

                                                        Solid3d acSol3D = new Solid3d();
                                                        acSol3D.SetDatabaseDefaults();
                                                        acSol3D.CreateSphere(R);
                                                        acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(QQ.GetX(), QQ.GetY(), QQ.GetZ()) - Point3d.Origin));

                                                        acBlkTblRec.AppendEntity(acSol3D);
                                                        trans.AddNewlyCreatedDBObject(acSol3D, true);
                                                        show = true;
                                                    }

                                                    #endregion

                                                    trans.Commit();
                                                    ed.UpdateScreen();
                                                }
                                            }
                                        }
                                    }

                                    if (!exist)
                                        MessageBox.Show("Polygon not Found !", "Polygon selection E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                }//if ((object)BEND != null)
                                else
                                {
                                    MessageBox.Show("Bend not Exist in DataBase !", "Polygon selection E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }//if (ptSecond.DistanceTo(ptFirst) >= UtilityClasses.ConstantsAndSettings.MinBendLength)                 
                        }//status ok
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Get By Numer
        [CommandMethod("KojtoCAD_3D", "KCAD_GET_BY_NUMER", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/GET_BY_NUMER.htm", "")]
        public void KojtoCAD_3D_Get_By_Numer()
        {
            
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter an option ";
            pKeyOpts.Keywords.Add("Triangles");
            pKeyOpts.Keywords.Add("Bends");
            pKeyOpts.Keywords.Add("StartPointOfBend");
            pKeyOpts.Keywords.Add("Nodes");
            pKeyOpts.Keywords.Add("Polygons");
            pKeyOpts.Keywords.Default = "Triangles";
            pKeyOpts.AllowNone = true;

            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.OK)
            {
                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "Triangles": KojtoCAD_3D_Get_Triangle_By_Numer(); break;
                        case "Bends": KojtoCAD_3D_Get_Bend_By_Numer(); break;
                        case "StartPointOfBend": KojtoCAD_3D_Get_BendStart_By_Numer(); break;
                        case "Nodes": KojtoCAD_3D_Get_Node_By_Numer(); break;
                        case "Polygons": KojtoCAD_3D_Get_Polygon_By_Numer(); break;
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Get By selection
        [CommandMethod("KojtoCAD_3D", "KCAD_GET_BY_SELECTION", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/GET_BY_SELECTION.htm", "")]
        public void KojtoCAD_3D_Get_By_Selection()
        {
            
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter an option ";
            pKeyOpts.Keywords.Add("Triangles");
            pKeyOpts.Keywords.Add("Bends");
            pKeyOpts.Keywords.Add("Nodes");
            pKeyOpts.Keywords.Add("Polygons");
            pKeyOpts.Keywords.Default = "Triangles";
            pKeyOpts.AllowNone = true;


            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.OK)
            {
                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "Triangles": KojtoCAD_3D_Get_Triangle_By_Selection(); break;
                        case "Bends": KojtoCAD_3D_Get_Bend_By_Selection(); break;
                        case "Nodes": KojtoCAD_3D_Get_Node_By_Selection(); break;
                        case "Polygons": KojtoCAD_3D_Get_Polygon_By_Selection(); break;
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_NEAREST_FROM_NODES", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/MIN_DISTANCE.htm", "")]
        public void KojtoCAD_3D_Get_Nearest_from_Nodes()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
                {
                    Pair<string, PromptStatus> ds = new Pair<string, PromptStatus>("No", PromptStatus.Error);
                    do
                    {
                        Pair<Point3d, PromptStatus> pointSelection =
                                GlobalFunctions.GetPoint(new Point3d(), "\nSelect Point :");
                        if (pointSelection.Second == PromptStatus.OK)
                        {
                            Point3d Point = pointSelection.First;
                            quaternion point = new quaternion(Point);

                            double distance = (point - container.Nodes[0].Position).abs();
                            quaternion nearest = container.Nodes[0].Position;
                            int numer = container.Nodes[0].GetNumer() + 1;

                            foreach (WorkClasses.Node node in container.Nodes)
                            {
                                double dist = (node.Position - point).abs();
                                if (dist < distance)
                                {
                                    distance = dist;
                                    nearest = node.Position;
                                    numer = node.GetNumer() + 1;
                                }
                            }

                            if (distance < Constants.zero_dist) { distance = 0.0; }

                            if (distance > 0.0)
                                GlobalFunctions.DrawLine((Point3d)nearest, (Point3d)point, 6);

                            string mess = string.Format("\nNearest Node Number: {0}\n\nPosition: {1:f5},{2:f5},{3:f5}\n\nDistance: {4}", numer, nearest.GetX(), nearest.GetY(), nearest.GetZ(), distance);
                            ed.WriteMessage(mess);
                            MessageBox.Show(mess, "Message");

                            ds = GlobalFunctions.GetKey(new string[] { "Yes", "No" }, 1, "\nYou will choose other Point ?");
                        }
                    }
                    while (ds.Second == PromptStatus.OK && ds.First == "Yes");
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_NEAREST_BEND_MIDPOINT", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/MIN_DISTANCE.htm", "")]
        public void KojtoCAD_3D_Get_Nearest_Bend_Midpoint()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
                {
                    Pair<string, PromptStatus> ds = new Pair<string, PromptStatus>("No", PromptStatus.Error);
                    do
                    {
                        Pair<Point3d, PromptStatus> pointSelection =
                                GlobalFunctions.GetPoint(new Point3d(), "\nSelect Point :");
                        if (pointSelection.Second == PromptStatus.OK)
                        {
                            Point3d Point = pointSelection.First;
                            quaternion point = new quaternion(Point);

                            double distance = (point - container.Bends[0].MidPoint).abs();
                            quaternion nearest = container.Bends[0].MidPoint;
                            int numer = container.Bends[0].GetNumer() + 1;

                            foreach (Bend bend in container.Bends)
                            {
                                quaternion mid = bend.MidPoint;
                                double dist = (mid - point).abs();
                                if (dist < distance)
                                {
                                    distance = dist;
                                    nearest = mid;
                                    numer = bend.GetNumer() + 1;
                                }
                            }

                            if (distance < Constants.zero_dist) { distance = 0.0; }

                            if (distance > 0.0)
                                GlobalFunctions.DrawLine((Point3d)nearest, (Point3d)point, 6);

                            string mess = string.Format("\nNearest Mid Point is from Bend Number: {0}\n\nPosition: {1:f5},{2:f5},{3:f5}\n\nDistance: {4}", numer, nearest.GetX(), nearest.GetY(), nearest.GetZ(), distance);
                            ed.WriteMessage(mess);
                            MessageBox.Show(mess, "Message");

                            ds = GlobalFunctions.GetKey(new string[] { "Yes", "No" }, 1, "\nYou will choose other Point ?");
                        }
                    }
                    while (ds.Second == PromptStatus.OK && ds.First == "Yes");
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_NEAREST_TRIANGLE_MIDPOINT", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/MIN_DISTANCE.htm", "")]
        public void KojtoCAD_3D_Get_Nearest_Triangle_Midpoint()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
                {
                    Pair<string, PromptStatus> ds = new Pair<string, PromptStatus>("No", PromptStatus.Error);
                    do
                    {
                        Pair<Point3d, PromptStatus> pointSelection =
                                GlobalFunctions.GetPoint(new Point3d(), "\nSelect Point :");
                        if (pointSelection.Second == PromptStatus.OK)
                        {
                            Point3d Point = pointSelection.First;
                            quaternion point = new quaternion(Point);

                            double distance = (point - container.Triangles[0].GetCentroid()).abs();
                            quaternion nearest = container.Triangles[0].GetCentroid();
                            int numer = container.Triangles[0].GetNumer() + 1;

                            foreach (Triangle TR in container.Triangles)
                            {
                                quaternion mid = TR.GetCentroid();
                                double dist = (mid - point).abs();
                                if (dist < distance)
                                {
                                    distance = dist;
                                    nearest = mid;
                                    numer = TR.GetNumer() + 1;
                                }
                            }

                            if (distance < Constants.zero_dist) { distance = 0.0; }

                            if (distance > 0.0)
                                GlobalFunctions.DrawLine((Point3d)nearest, (Point3d)point, 6);

                            string mess = string.Format("\nNearest Centroid Point is from Triangle Number: {0}\n\nPosition: {1:f5},{2:f5},{3:f5}\n\nDistance: {4}", numer, nearest.GetX(), nearest.GetY(), nearest.GetZ(), distance);
                            ed.WriteMessage(mess);
                            MessageBox.Show(mess, "Message");

                            ds = GlobalFunctions.GetKey(new string[] { "Yes", "No" }, 1, "\nYou will choose other Point ?");
                        }
                    }
                    while (ds.Second == PromptStatus.OK && ds.First == "Yes");
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
    }
}
