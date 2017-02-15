using System;
using System.Collections.Generic;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
using KojtoCAD.KojtoCAD3D.WorkClasses;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;

#endif
[assembly: CommandClass(typeof(Kojto3DTrim))]

namespace KojtoCAD.KojtoCAD3D
{
    public class Kojto3DTrim
    {
        public Containers container = ContextVariablesProvider.Container;

        [CommandMethod("KojtoCAD_3D", "KCAD_TrimPick_low", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_TrimPick_low()
        {
            
            if (container.Triangles.Count > 0)
            {
                double x1 = 65.0;
                double x2 = 33.0;
                double x3 = 12.0;
                double x4 = 8.0;

                GetDistances(ref x1, ref x2, ref x3, ref x4);

                foreach (Triangle TR in container.Triangles)
                {
                    int[] nodes = TR.GetNodesNumers();

                    quaternion Q0 = container.Nodes[nodes[0]].Position;
                    quaternion Q1 = container.Nodes[nodes[1]].Position;
                    quaternion Q2 = container.Nodes[nodes[2]].Position;

                    double[] angles = new double[3];
                    double[] anglesDegree = new double[3];

                    angles[0] = (Q1 - Q0).angTo(Q2 - Q0); anglesDegree[0] = angles[0] * 180.0 / Math.PI;
                    angles[1] = (Q0 - Q1).angTo(Q2 - Q1); anglesDegree[1] = angles[1] * 180.0 / Math.PI;
                    angles[2] = (Q0 - Q2).angTo(Q1 - Q2); anglesDegree[2] = angles[2] * 180.0 / Math.PI;

                    List<ObjectId> ids = new List<ObjectId>();

                    if (anglesDegree[0] <= 12.5) ids.Add(TrimPick(nodes[0], TR.GetNumer(), x1, anglesDegree[0]));
                    if ((anglesDegree[0] > 12.5) && (anglesDegree[0] <= 20.0)) ids.Add(TrimPick(nodes[0], TR.GetNumer(), x2, anglesDegree[0]));
                    if ((anglesDegree[0] > 20.0) && (anglesDegree[0] <= 35.0)) ids.Add(TrimPick(nodes[0], TR.GetNumer(), x3, anglesDegree[0]));
                    if ((anglesDegree[0] > 35.0) && (anglesDegree[0] <= 45.0)) ids.Add(TrimPick(nodes[0], TR.GetNumer(), x4, anglesDegree[0]));

                    if (anglesDegree[1] <= 12.5) ids.Add(TrimPick(nodes[1], TR.GetNumer(), x1, anglesDegree[1]));
                    if ((anglesDegree[1] > 12.5) && (anglesDegree[1] <= 20.0)) ids.Add(TrimPick(nodes[1], TR.GetNumer(), x2, anglesDegree[1]));
                    if ((anglesDegree[1] > 20.0) && (anglesDegree[1] <= 35.0)) ids.Add(TrimPick(nodes[1], TR.GetNumer(), x3, anglesDegree[1]));
                    if ((anglesDegree[1] > 35.0) && (anglesDegree[1] <= 45.0)) ids.Add(TrimPick(nodes[1], TR.GetNumer(), x4, anglesDegree[1]));

                    if (anglesDegree[2] <= 12.5) ids.Add(TrimPick(nodes[2], TR.GetNumer(), x1, anglesDegree[2]));
                    if ((anglesDegree[2] > 12.5) && (anglesDegree[2] <= 20.0)) ids.Add(TrimPick(nodes[2], TR.GetNumer(), x2, anglesDegree[2]));
                    if ((anglesDegree[2] > 20.0) && (anglesDegree[2] <= 35.0)) ids.Add(TrimPick(nodes[2], TR.GetNumer(), x3, anglesDegree[2]));
                    if ((anglesDegree[2] > 35.0) && (anglesDegree[2] <= 45.0)) ids.Add(TrimPick(nodes[2], TR.GetNumer(), x4, anglesDegree[2]));

                    if (ids.Count > 0)
                    {
                        using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                        {
                            Solid3d ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                Solid3d sol = tr.GetObject(ids[0], OpenMode.ForWrite) as Solid3d;
                                for (int i = 1; i < ids.Count; i++)
                                {
                                    Solid3d sol_i = tr.GetObject(ids[i], OpenMode.ForWrite) as Solid3d;
                                    sol.BooleanOperation(BooleanOperationType.BoolUnite, sol_i);
                                }

                                ent.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                            }
                            catch { }

                            tr.Commit();
                        }
                    }

                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_TrimPick_low_for_tr", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_TrimPick_low_tr()
        {
            
            if (container.Triangles.Count > 0)
            {
                Pair<int, PromptStatus> pa = GlobalFunctions.GetInt(1, "\ntriangle numer:");
                if ((pa.Second != PromptStatus.OK) || (pa.First < 1)) return;
                int num = pa.First - 1;
                Triangle TR = container.Triangles[num];

                double x1 = 65.0;
                double x2 = 33.0;
                double x3 = 12.0;
                double x4 = 8.0;

                GetDistances(ref x1, ref x2, ref x3, ref x4);

                //foreach (WorkClasses.Triangle TR in container.Triangles)
                {
                    int[] nodes = TR.GetNodesNumers();

                    quaternion Q0 = container.Nodes[nodes[0]].Position;
                    quaternion Q1 = container.Nodes[nodes[1]].Position;
                    quaternion Q2 = container.Nodes[nodes[2]].Position;

                    double[] angles = new double[3];
                    double[] anglesDegree = new double[3];

                    angles[0] = (Q1 - Q0).angTo(Q2 - Q0); anglesDegree[0] = angles[0] * 180.0 / Math.PI;
                    angles[1] = (Q0 - Q1).angTo(Q2 - Q1); anglesDegree[1] = angles[1] * 180.0 / Math.PI;
                    angles[2] = (Q0 - Q2).angTo(Q1 - Q2); anglesDegree[2] = angles[2] * 180.0 / Math.PI;

                    List<ObjectId> ids = new List<ObjectId>();

                    if (anglesDegree[0] <= 12.5) ids.Add(TrimPick(nodes[0], TR.GetNumer(), x1, anglesDegree[0]));
                    if ((anglesDegree[0] > 12.5) && (anglesDegree[0] <= 20.0)) ids.Add(TrimPick(nodes[0], TR.GetNumer(), x2, anglesDegree[0]));
                    if ((anglesDegree[0] > 20.0) && (anglesDegree[0] <= 35.0)) ids.Add(TrimPick(nodes[0], TR.GetNumer(), x3, anglesDegree[0]));
                    if ((anglesDegree[0] > 35.0) && (anglesDegree[0] <= 45.0)) ids.Add(TrimPick(nodes[0], TR.GetNumer(), x4, anglesDegree[0]));

                    if (anglesDegree[1] <= 12.5) ids.Add(TrimPick(nodes[1], TR.GetNumer(), x1, anglesDegree[1]));
                    if ((anglesDegree[1] > 12.5) && (anglesDegree[1] <= 20.0)) ids.Add(TrimPick(nodes[1], TR.GetNumer(), x2, anglesDegree[1]));
                    if ((anglesDegree[1] > 20.0) && (anglesDegree[1] <= 35.0)) ids.Add(TrimPick(nodes[1], TR.GetNumer(), x3, anglesDegree[1]));
                    if ((anglesDegree[1] > 35.0) && (anglesDegree[1] <= 45.0)) ids.Add(TrimPick(nodes[1], TR.GetNumer(), x4, anglesDegree[1]));

                    if (anglesDegree[2] <= 12.5) ids.Add(TrimPick(nodes[2], TR.GetNumer(), x1, anglesDegree[2]));
                    if ((anglesDegree[2] > 12.5) && (anglesDegree[2] <= 20.0)) ids.Add(TrimPick(nodes[2], TR.GetNumer(), x2, anglesDegree[2]));
                    if ((anglesDegree[2] > 20.0) && (anglesDegree[2] <= 35.0)) ids.Add(TrimPick(nodes[2], TR.GetNumer(), x3, anglesDegree[2]));
                    if ((anglesDegree[2] > 35.0) && (anglesDegree[2] <= 45.0)) ids.Add(TrimPick(nodes[2], TR.GetNumer(), x4, anglesDegree[2]));

                    if (ids.Count > 0)
                    {
                        using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                        {
                            Solid3d ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                Solid3d sol = tr.GetObject(ids[0], OpenMode.ForWrite) as Solid3d;
                                for (int i = 1; i < ids.Count; i++)
                                {
                                    Solid3d sol_i = tr.GetObject(ids[i], OpenMode.ForWrite) as Solid3d;
                                    sol.BooleanOperation(BooleanOperationType.BoolUnite, sol_i);
                                }

                                ent.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                            }
                            catch { }

                            tr.Commit();
                        }
                    }

                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_TrimPick_low_tr", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_TrimPick_low_tr_()
        {
            
            if (container.Triangles.Count > 0)
            {
                Pair<int, PromptStatus> pa = GlobalFunctions.GetInt(1, "\ntriangle numer:");
                if ((pa.Second != PromptStatus.OK) || (pa.First < 1)) return;
                int num = pa.First - 1;
                Triangle TR = container.Triangles[num];

                Pair<int, PromptStatus> pa1 = GlobalFunctions.GetInt(1, "\nNode numer:");
                if ((pa1.Second != PromptStatus.OK) || (pa1.First < 1)) return;

                double x1 = 65.0;
                Pair<double, PromptStatus> pa2 = GlobalFunctions.GetDouble(x1, "\ndistance:");
                if (pa2.Second != PromptStatus.OK) return;
                x1 = pa2.First;



                //foreach (WorkClasses.Triangle TR in container.Triangles)
                {
                    int[] nodes = TR.GetNodesNumers();

                    List<int> temp = new List<int>();
                    if (nodes[0] != (pa1.First - 1)) temp.Add(nodes[0]);
                    if (nodes[1] != (pa1.First - 1)) temp.Add(nodes[1]);
                    if (nodes[2] != (pa1.First - 1)) temp.Add(nodes[2]);
                    if (temp.Count != 2)
                    {
                        MessageBox.Show("Node E R R O R !", "E R R O R");
                        return;
                    }
                    //quaternion Q0 = container.Nodes[nodes[0]].Position;
                    quaternion Q0 = container.Nodes[pa1.First - 1].Position;
                    quaternion Q1 = container.Nodes[temp[0]].Position;
                    quaternion Q2 = container.Nodes[temp[1]].Position;

                    double[] angles = new double[3];
                    double[] anglesDegree = new double[3];

                    angles[0] = (Q1 - Q0).angTo(Q2 - Q0); anglesDegree[0] = angles[0] * 180.0 / Math.PI;

                    List<ObjectId> ids = new List<ObjectId>();

                    ids.Add(TrimPick(pa1.First - 1, TR.GetNumer(), x1, anglesDegree[0]));

                    if (ids.Count > 0)
                    {
                        using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                        {
                            Solid3d ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                Solid3d sol = tr.GetObject(ids[0], OpenMode.ForWrite) as Solid3d;
                                for (int i = 1; i < ids.Count; i++)
                                {
                                    Solid3d sol_i = tr.GetObject(ids[i], OpenMode.ForWrite) as Solid3d;
                                    sol.BooleanOperation(BooleanOperationType.BoolUnite, sol_i);
                                }

                                ent.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                            }
                            catch { }

                            tr.Commit();
                        }
                    }

                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private ObjectId TrimPick(int nodeNumer, int TriangleNumer, double X, double angle)
        {
            // MessageBox.Show(X.ToString());

            //UtilityClasses.ConstantsAndSettings.DoubleGlass_h1
            //UtilityClasses.ConstantsAndSettings.DoubleGlass_h2
            //UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass
            #region basedata
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\n");
            ed.WriteMessage(angle.ToString());

            var node = container.Nodes[nodeNumer];
            Triangle TR = container.Triangles[TriangleNumer];
            quaternion trNormal = TR.Normal.Second - TR.Normal.First;
            trNormal /= trNormal.abs();
            plane trPlane = new plane(TR.Nodes.First, TR.Nodes.Second, TR.Nodes.Third);
            plane trPlane_h1 = new plane(TR.Nodes.First + trNormal * ConstantsAndSettings.DoubleGlass_h1,
                                                     TR.Nodes.Second + trNormal * ConstantsAndSettings.DoubleGlass_h1,
                                                     TR.Nodes.Third + trNormal * ConstantsAndSettings.DoubleGlass_h1);
            plane trPlane_H = new plane(TR.Nodes.First + trNormal * ConstantsAndSettings.Thickness_of_the_Glass,
                                                     TR.Nodes.Second + trNormal * ConstantsAndSettings.Thickness_of_the_Glass,
                                                     TR.Nodes.Third + trNormal * ConstantsAndSettings.Thickness_of_the_Glass);
            plane trPlane_h2 = new plane(TR.Nodes.First + trNormal * (ConstantsAndSettings.Thickness_of_the_Glass - ConstantsAndSettings.DoubleGlass_h2),
                                                     TR.Nodes.Second + trNormal * (ConstantsAndSettings.Thickness_of_the_Glass - ConstantsAndSettings.DoubleGlass_h2),
                                                     TR.Nodes.Third + trNormal * (ConstantsAndSettings.Thickness_of_the_Glass - ConstantsAndSettings.DoubleGlass_h2));

            Point3dCollection pColl = new Point3dCollection();
            IntegerCollection iCol1 = new IntegerCollection();
            IntegerCollection iCol2 = new IntegerCollection();

            Point3dCollection pColl_up = new Point3dCollection();
            IntegerCollection iCol1_up = new IntegerCollection();
            IntegerCollection iCol2_up = new IntegerCollection();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                if ((TR.lowSolidHandle.First >= 0) && (TR.lowSolidHandle.Second != null))
                {
                    try
                    {
                        Solid3d lowSolid = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                        lowSolid.GetGripPoints(pColl, iCol1, iCol2);

                    }
                    catch { }
                }
                if ((TR.upSolidHandle.First >= 0) && (TR.upSolidHandle.Second != null))
                {
                    try
                    {
                        Solid3d upSolid = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                        upSolid.GetGripPoints(pColl_up, iCol1_up, iCol2_up);
                    }
                    catch { }
                }
            }


            Bend bend1 = container.Bends[TR.GetFirstBendNumer()];
            Bend bend2 = container.Bends[TR.GetSecondBendNumer()];
            Bend bend3 = container.Bends[TR.GetThirdBendNumer()];
            #endregion

            List<Bend> bends = new List<Bend>();
            if ((bend1.StartNodeNumer == nodeNumer) || (bend1.EndNodeNumer == nodeNumer))
                bends.Add(bend1);
            if ((bend2.StartNodeNumer == nodeNumer) || (bend2.EndNodeNumer == nodeNumer))
                bends.Add(bend2);
            if ((bend3.StartNodeNumer == nodeNumer) || (bend3.EndNodeNumer == nodeNumer))
                bends.Add(bend3);

            if (bends.Count != 2)
            {
                MessageBox.Show("Only two Bends are connected at one point !", "E R R O R");
                return ObjectId.Null;
            }

            if (bends[0].IsFictive() || bends[1].IsFictive())
            {
                return ObjectId.Null;
            }

            quaternion Q0 = node.Position;
            quaternion Q1 = (bends[0].Start + bends[0].End) / 2.0;
            quaternion Q2 = (bends[1].Start + bends[1].End) / 2.0;

            // coorinat systems for plane points calculate           
            Q1 = Q1 - Q0; Q1 /= Q1.abs(); Q1 *= X; Q1 = Q0 + Q1;
            Q2 = Q2 - Q0; Q2 /= Q2.abs(); Q2 *= X; Q2 = Q0 + Q2;

            UCS ucs = new UCS(Q0, (Q1 + Q2) / 2.0, Q2);
            ucs = new UCS(Q0, ucs.ToACS(new quaternion(0, 0, 100, 0)), ucs.ToACS(new quaternion(0, 100, 0, 0)));

            #region UP glass
            /*
            double dist = 100000000.0;
            quaternion pick = null;//origin
            #region search origin
            foreach (Point3d p in pColl)
            {
                
                quaternion q = (quaternion)p;
                q = ucs.FromACS(q);
                double z = Math.Abs(q.GetZ());
                
                if (pick == (object)null)
                {
                    pick = (quaternion)p;
                    dist = z;
                }
                else
                {
                    if (z < dist)
                    {
                        dist = z;
                        pick = (quaternion)p;
                    }
                }
            }
            pColl.Clear(); pColl.Dispose();
            iCol1.Clear();
            iCol1.Clear();
            #endregion
            */
            double dist = (node.Position - pColl[0]).abs();
            quaternion pick = new quaternion();

            foreach (Point3d p in pColl)
            {

                quaternion q = p;
                double z = (node.Position - q).abs();

                if (z < dist)
                {
                    dist = z;
                    pick = new quaternion(0, p.X, p.Y, p.Z);
                }

            }
            pColl.Clear(); pColl.Dispose();
            iCol1.Clear();
            iCol1.Clear();


            ucs.o = pick;
            Q1 = pick + Q1 - Q0;
            Q2 = pick + Q2 - Q0;
            Q0 = pick;

            // UtilityClasses.GlobalFunctions.DrawLine((Point3d)Q0, new Point3d(), 1);

            quaternion t1 = (Q2 - Q1) * 2.0;
            quaternion t2 = (Q1 - Q2) * 2.0;
            Q1 = Q1 + t2;
            Q2 = Q2 + t1;

            Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());
            Q1 = ucs.FromACS(Q1);
            Q2 = ucs.FromACS(Q2);
            quaternion Q3 = new quaternion(0, Q1.GetX(), -Q1.GetY(), Q1.GetZ());
            quaternion Q4 = new quaternion(0, Q2.GetX(), -Q2.GetY(), Q2.GetZ());
            quaternion[] arr = { Q1, Q2, Q4, Q3, Q1 };
            ObjectId obj = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Polyline pol = GlobalFunctions.GetPoly(ref arr);
                //pol.TransformBy(Matrix3d.Displacement(new Point3d().GetVectorTo(new Point3d(0, 0, -100))));
                //pol.TransformBy(mat);

                //acBlkTblRec.AppendEntity(pol);
                //tr.AddNewlyCreatedDBObject(pol, true);

                try
                {

                    Solid3d sol = new Solid3d();
                    sol.CreateExtrudedSolid(pol, new Point3d(0, 0, 0).GetVectorTo(new Point3d(0, 0, 100)), new SweepOptions());
                    sol.TransformBy(Matrix3d.Displacement(new Point3d().GetVectorTo(new Point3d(0, 0, -50))));
                    sol.TransformBy(mat);
                    acBlkTblRec.AppendEntity(sol);
                    tr.AddNewlyCreatedDBObject(sol, true);
                    obj = sol.ObjectId;
                }
                catch { }

                tr.Commit();
            }
            /*
            if (obj != ObjectId.Null)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {

                    Solid3d sol = tr.GetObject(obj, OpenMode.ForWrite) as Solid3d;
                    Solid3d ent = tr.GetObject(UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                    ent.BooleanOperation(BooleanOperationType.BoolSubtract, sol);

                    tr.Commit();
                }
            }*/
            #endregion




            // ed.WriteMessage("\n");
            // ed.WriteMessage(ucs.FromACS(Q0).ToString());
            // ed.WriteMessage("\n-----------------------------\n");

            return obj;

        }

        [CommandMethod("KojtoCAD_3D", "KCAD_TrimPick_up", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_TrimPick_up()
        {
            
            if (container.Triangles.Count > 0)
            {
                double x1 = 65.0;
                double x2 = 33.0;
                double x3 = 12.0;
                double x4 = 8.0;

                GetDistances(ref x1, ref x2, ref x3, ref x4);

                foreach (Triangle TR in container.Triangles)
                {
                    int[] nodes = TR.GetNodesNumers();

                    quaternion Q0 = container.Nodes[nodes[0]].Position;
                    quaternion Q1 = container.Nodes[nodes[1]].Position;
                    quaternion Q2 = container.Nodes[nodes[2]].Position;

                    double[] angles = new double[3];
                    double[] anglesDegree = new double[3];

                    angles[0] = (Q1 - Q0).angTo(Q2 - Q0); anglesDegree[0] = angles[0] * 180.0 / Math.PI;
                    angles[1] = (Q0 - Q1).angTo(Q2 - Q1); anglesDegree[1] = angles[1] * 180.0 / Math.PI;
                    angles[2] = (Q0 - Q2).angTo(Q1 - Q2); anglesDegree[2] = angles[2] * 180.0 / Math.PI;

                    List<ObjectId> ids = new List<ObjectId>();

                    if (anglesDegree[0] <= 12.5) ids.Add(TrimPickUP(nodes[0], TR.GetNumer(), x1, anglesDegree[0]));
                    if ((anglesDegree[0] > 12.5) && (anglesDegree[0] <= 20.0)) ids.Add(TrimPickUP(nodes[0], TR.GetNumer(), x2, anglesDegree[0]));
                    if ((anglesDegree[0] > 20.0) && (anglesDegree[0] <= 35.0)) ids.Add(TrimPickUP(nodes[0], TR.GetNumer(), x3, anglesDegree[0]));
                    if ((anglesDegree[0] > 35.0) && (anglesDegree[0] <= 45.0)) ids.Add(TrimPickUP(nodes[0], TR.GetNumer(), x4, anglesDegree[0]));

                    if (anglesDegree[1] <= 12.5) ids.Add(TrimPickUP(nodes[1], TR.GetNumer(), x1, anglesDegree[1]));
                    if ((anglesDegree[1] > 12.5) && (anglesDegree[1] <= 20.0)) ids.Add(TrimPickUP(nodes[1], TR.GetNumer(), x2, anglesDegree[1]));
                    if ((anglesDegree[1] > 20.0) && (anglesDegree[1] <= 35.0)) ids.Add(TrimPickUP(nodes[1], TR.GetNumer(), x3, anglesDegree[1]));
                    if ((anglesDegree[1] > 35.0) && (anglesDegree[1] <= 45.0)) ids.Add(TrimPickUP(nodes[1], TR.GetNumer(), x4, anglesDegree[1]));

                    if (anglesDegree[2] <= 12.5) ids.Add(TrimPickUP(nodes[2], TR.GetNumer(), x1, anglesDegree[2]));
                    if ((anglesDegree[2] > 12.5) && (anglesDegree[2] <= 20.0)) ids.Add(TrimPickUP(nodes[2], TR.GetNumer(), x2, anglesDegree[2]));
                    if ((anglesDegree[2] > 20.0) && (anglesDegree[2] <= 35.0)) ids.Add(TrimPickUP(nodes[2], TR.GetNumer(), x3, anglesDegree[2]));
                    if ((anglesDegree[2] > 35.0) && (anglesDegree[2] <= 45.0)) ids.Add(TrimPickUP(nodes[2], TR.GetNumer(), x4, anglesDegree[2]));


                    if (ids.Count > 0)
                    {
                        using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                        {
                            Solid3d ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                Solid3d sol = tr.GetObject(ids[0], OpenMode.ForWrite) as Solid3d;
                                for (int i = 1; i < ids.Count; i++)
                                {
                                    Solid3d sol_i = tr.GetObject(ids[i], OpenMode.ForWrite) as Solid3d;
                                    sol.BooleanOperation(BooleanOperationType.BoolUnite, sol_i);
                                }

                                ent.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                            }
                            catch { }

                            tr.Commit();
                        }
                    }

                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_TrimPick_up_for_tr", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_TrimPick_up_tr()
        {
            
            if (container.Triangles.Count > 0)
            {
                Pair<int, PromptStatus> pa = GlobalFunctions.GetInt(1, "\ntriangle numer:");
                if ((pa.Second != PromptStatus.OK) || (pa.First < 1)) return;
                int num = pa.First - 1;
                Triangle TR = container.Triangles[num];

                double x1 = 65.0;
                double x2 = 33.0;
                double x3 = 12.0;
                double x4 = 8.0;

                GetDistances(ref x1, ref x2, ref x3, ref x4);

                //foreach (WorkClasses.Triangle TR in container.Triangles)
                {
                    int[] nodes = TR.GetNodesNumers();

                    quaternion Q0 = container.Nodes[nodes[0]].Position;
                    quaternion Q1 = container.Nodes[nodes[1]].Position;
                    quaternion Q2 = container.Nodes[nodes[2]].Position;

                    double[] angles = new double[3];
                    double[] anglesDegree = new double[3];

                    angles[0] = (Q1 - Q0).angTo(Q2 - Q0); anglesDegree[0] = angles[0] * 180.0 / Math.PI;
                    angles[1] = (Q0 - Q1).angTo(Q2 - Q1); anglesDegree[1] = angles[1] * 180.0 / Math.PI;
                    angles[2] = (Q0 - Q2).angTo(Q1 - Q2); anglesDegree[2] = angles[2] * 180.0 / Math.PI;

                    List<ObjectId> ids = new List<ObjectId>();

                    if (anglesDegree[0] <= 12.5) ids.Add(TrimPickUP(nodes[0], TR.GetNumer(), x1, anglesDegree[0]));
                    if ((anglesDegree[0] > 12.5) && (anglesDegree[0] <= 20.0)) ids.Add(TrimPickUP(nodes[0], TR.GetNumer(), x2, anglesDegree[0]));
                    if ((anglesDegree[0] > 20.0) && (anglesDegree[0] <= 35.0)) ids.Add(TrimPickUP(nodes[0], TR.GetNumer(), x3, anglesDegree[0]));
                    if ((anglesDegree[0] > 35.0) && (anglesDegree[0] <= 45.0)) ids.Add(TrimPickUP(nodes[0], TR.GetNumer(), x4, anglesDegree[0]));

                    if (anglesDegree[1] <= 12.5) ids.Add(TrimPickUP(nodes[1], TR.GetNumer(), x1, anglesDegree[1]));
                    if ((anglesDegree[1] > 12.5) && (anglesDegree[1] <= 20.0)) ids.Add(TrimPickUP(nodes[1], TR.GetNumer(), x2, anglesDegree[1]));
                    if ((anglesDegree[1] > 20.0) && (anglesDegree[1] <= 35.0)) ids.Add(TrimPickUP(nodes[1], TR.GetNumer(), x3, anglesDegree[1]));
                    if ((anglesDegree[1] > 35.0) && (anglesDegree[1] <= 45.0)) ids.Add(TrimPickUP(nodes[1], TR.GetNumer(), x4, anglesDegree[1]));

                    if (anglesDegree[2] <= 12.5) ids.Add(TrimPickUP(nodes[2], TR.GetNumer(), x1, anglesDegree[2]));
                    if ((anglesDegree[2] > 12.5) && (anglesDegree[2] <= 20.0)) ids.Add(TrimPickUP(nodes[2], TR.GetNumer(), x2, anglesDegree[2]));
                    if ((anglesDegree[2] > 20.0) && (anglesDegree[2] <= 35.0)) ids.Add(TrimPickUP(nodes[2], TR.GetNumer(), x3, anglesDegree[2]));
                    if ((anglesDegree[2] > 35.0) && (anglesDegree[2] <= 45.0)) ids.Add(TrimPickUP(nodes[2], TR.GetNumer(), x4, anglesDegree[2]));


                    if (ids.Count > 0)
                    {
                        using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                        {
                            Solid3d ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                Solid3d sol = tr.GetObject(ids[0], OpenMode.ForWrite) as Solid3d;
                                for (int i = 1; i < ids.Count; i++)
                                {
                                    Solid3d sol_i = tr.GetObject(ids[i], OpenMode.ForWrite) as Solid3d;
                                    sol.BooleanOperation(BooleanOperationType.BoolUnite, sol_i);
                                }

                                ent.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                            }
                            catch { }

                            tr.Commit();
                        }
                    }

                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_TrimPick_up_tr", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_TrimPick_up_tr_()
        {
            
            if (container.Triangles.Count > 0)
            {
                Pair<int, PromptStatus> pa = GlobalFunctions.GetInt(1, "\ntriangle numer:");
                if ((pa.Second != PromptStatus.OK) || (pa.First < 1)) return;
                int num = pa.First - 1;
                Triangle TR = container.Triangles[num];

                Pair<int, PromptStatus> pa1 = GlobalFunctions.GetInt(1, "\nNode numer:");
                if ((pa1.Second != PromptStatus.OK) || (pa1.First < 1)) return;

                double x1 = 65.0;
                Pair<double, PromptStatus> pa2 = GlobalFunctions.GetDouble(x1, "\ndistance:");
                if (pa2.Second != PromptStatus.OK) return;
                x1 = pa2.First;


                //foreach (WorkClasses.Triangle TR in container.Triangles)
                {
                    int[] nodes = TR.GetNodesNumers();

                    List<int> temp = new List<int>();
                    if (nodes[0] != (pa1.First - 1)) temp.Add(nodes[0]);
                    if (nodes[1] != (pa1.First - 1)) temp.Add(nodes[1]);
                    if (nodes[2] != (pa1.First - 1)) temp.Add(nodes[2]);
                    if (temp.Count != 2)
                    {
                        MessageBox.Show("Node E R R O R !", "E R R O R");
                        return;
                    }
                    //quaternion Q0 = container.Nodes[nodes[0]].Position;
                    quaternion Q0 = container.Nodes[pa1.First - 1].Position;
                    quaternion Q1 = container.Nodes[temp[0]].Position;
                    quaternion Q2 = container.Nodes[temp[1]].Position;

                    double[] angles = new double[3];
                    double[] anglesDegree = new double[3];

                    angles[0] = (Q1 - Q0).angTo(Q2 - Q0); anglesDegree[0] = angles[0] * 180.0 / Math.PI;

                    List<ObjectId> ids = new List<ObjectId>();

                    ids.Add(TrimPickUP(pa1.First - 1, TR.GetNumer(), x1, anglesDegree[0]));

                    if (ids.Count > 0)
                    {
                        using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                        {
                            Solid3d ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                Solid3d sol = tr.GetObject(ids[0], OpenMode.ForWrite) as Solid3d;
                                for (int i = 1; i < ids.Count; i++)
                                {
                                    Solid3d sol_i = tr.GetObject(ids[i], OpenMode.ForWrite) as Solid3d;
                                    sol.BooleanOperation(BooleanOperationType.BoolUnite, sol_i);
                                }

                                ent.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                            }
                            catch { }

                            tr.Commit();
                        }
                    }

                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private ObjectId TrimPickUP(int nodeNumer, int TriangleNumer, double X, double angle)
        {
            //UtilityClasses.ConstantsAndSettings.DoubleGlass_h1
            //UtilityClasses.ConstantsAndSettings.DoubleGlass_h2
            //UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass
            #region basedata
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\n");
            ed.WriteMessage(angle.ToString());

            var node = container.Nodes[nodeNumer];
            Triangle TR = container.Triangles[TriangleNumer];
            quaternion trNormal = TR.Normal.Second - TR.Normal.First;
            trNormal /= trNormal.abs();
            plane trPlane = new plane(TR.Nodes.First, TR.Nodes.Second, TR.Nodes.Third);


            Point3dCollection pColl = new Point3dCollection();
            IntegerCollection iCol1 = new IntegerCollection();
            IntegerCollection iCol2 = new IntegerCollection();



            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                if ((TR.upSolidHandle.First >= 0) && (TR.upSolidHandle.Second != null))
                {
                    try
                    {
                        Solid3d upSolid = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                        upSolid.GetGripPoints(pColl, iCol1, iCol2);
                    }
                    catch { }
                }
            }


            Bend bend1 = container.Bends[TR.GetFirstBendNumer()];
            Bend bend2 = container.Bends[TR.GetSecondBendNumer()];
            Bend bend3 = container.Bends[TR.GetThirdBendNumer()];
            #endregion

            List<Bend> bends = new List<Bend>();
            if ((bend1.StartNodeNumer == nodeNumer) || (bend1.EndNodeNumer == nodeNumer))
                bends.Add(bend1);
            if ((bend2.StartNodeNumer == nodeNumer) || (bend2.EndNodeNumer == nodeNumer))
                bends.Add(bend2);
            if ((bend3.StartNodeNumer == nodeNumer) || (bend3.EndNodeNumer == nodeNumer))
                bends.Add(bend3);

            if (bends.Count != 2)
            {
                MessageBox.Show("Only two Bends are connected at one point !", "E R R O R");
                return ObjectId.Null;
            }

            if (bends[0].IsFictive() || bends[1].IsFictive())
            {
                return ObjectId.Null;
            }

            quaternion Q0 = node.Position;
            quaternion Q1 = (bends[0].Start + bends[0].End) / 2.0;
            quaternion Q2 = (bends[1].Start + bends[1].End) / 2.0;

            // coorinat systems for plane points calculate           
            Q1 = Q1 - Q0; Q1 /= Q1.abs(); Q1 *= X; Q1 = Q0 + Q1;
            Q2 = Q2 - Q0; Q2 /= Q2.abs(); Q2 *= X; Q2 = Q0 + Q2;

            UCS ucs = new UCS(Q0, (Q1 + Q2) / 2.0, Q2);
            ucs = new UCS(Q0, ucs.ToACS(new quaternion(0, 0, 100, 0)), ucs.ToACS(new quaternion(0, 100, 0, 0)));

            #region UP glass

            double dist = (node.Position - (quaternion)pColl[0]).abs();
            quaternion pick = new quaternion();

            foreach (Point3d p in pColl)
            {

                quaternion q = (quaternion)p;
                double z = (node.Position - q).abs();

                if (z < dist)
                {
                    dist = z;
                    pick = new quaternion(0, p.X, p.Y, p.Z);
                }

            }
            pColl.Clear(); pColl.Dispose();
            iCol1.Clear();
            iCol1.Clear();
            #endregion


            ucs.o = pick;
            Q1 = pick + Q1 - Q0;
            Q2 = pick + Q2 - Q0;
            Q0 = pick;

            // UtilityClasses.GlobalFunctions.DrawLine((Point3d)Q0, new Point3d(), 1);

            quaternion t1 = (Q2 - Q1) * 2.0;
            quaternion t2 = (Q1 - Q2) * 2.0;
            Q1 = Q1 + t2;
            Q2 = Q2 + t1;

            Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());
            Q1 = ucs.FromACS(Q1);
            Q2 = ucs.FromACS(Q2);
            quaternion Q3 = new quaternion(0, Q1.GetX(), -Q1.GetY(), Q1.GetZ());
            quaternion Q4 = new quaternion(0, Q2.GetX(), -Q2.GetY(), Q2.GetZ());
            quaternion[] arr = { Q1, Q2, Q4, Q3, Q1 };
            ObjectId obj = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Polyline pol = GlobalFunctions.GetPoly(ref arr);
                //pol.TransformBy(Matrix3d.Displacement(new Point3d().GetVectorTo(new Point3d(0, 0, -100))));
                //pol.TransformBy(mat);

                //acBlkTblRec.AppendEntity(pol);
                //tr.AddNewlyCreatedDBObject(pol, true);

                try
                {

                    Solid3d sol = new Solid3d();
                    sol.CreateExtrudedSolid(pol, new Point3d(0, 0, 0).GetVectorTo(new Point3d(0, 0, 100)), new SweepOptions());
                    sol.TransformBy(Matrix3d.Displacement(new Point3d().GetVectorTo(new Point3d(0, 0, -50))));
                    sol.TransformBy(mat);
                    acBlkTblRec.AppendEntity(sol);
                    tr.AddNewlyCreatedDBObject(sol, true);
                    obj = sol.ObjectId;
                }
                catch { }

                tr.Commit();
            }
            /*
            if (obj != ObjectId.Null)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {

                    Solid3d sol = tr.GetObject(obj, OpenMode.ForWrite) as Solid3d;
                    Solid3d ent = tr.GetObject(UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                    ent.BooleanOperation(BooleanOperationType.BoolSubtract, sol);

                    tr.Commit();
                }
            }*/





            // ed.WriteMessage("\n");
            // ed.WriteMessage(ucs.FromACS(Q0).ToString());
            // ed.WriteMessage("\n-----------------------------\n");

            return obj;

        }

        private void GetDistances(ref double x1, ref double x2, ref double x3, ref double x4)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Pair<double, PromptStatus> pa = GlobalFunctions.GetDouble(x1, "<= 12.5 degree");
            if (pa.Second == PromptStatus.OK)
            {
                x1 = pa.First;
                Pair<double, PromptStatus> pa1 = GlobalFunctions.GetDouble(x2, "> 12.5 and <= 20 degree");
                if (pa1.Second == PromptStatus.OK)
                {
                    x2 = pa1.First;
                    Pair<double, PromptStatus> pa2 = GlobalFunctions.GetDouble(x3, "> 20 and <= 35 degree");
                    if (pa2.Second == PromptStatus.OK)
                    {
                        x3 = pa2.First;
                        Pair<double, PromptStatus> pa3 = GlobalFunctions.GetDouble(x4, "> 35 and <= 45 degree");
                        if (pa3.Second == PromptStatus.OK)
                        {
                            x4 = pa3.First;
                        }
                    }
                }
            }
        }
    }
}
