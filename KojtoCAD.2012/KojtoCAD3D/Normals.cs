using System;
using System.Collections.Generic;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
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
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.Normals))]

namespace KojtoCAD.KojtoCAD3D
{
    public class Normals
    {
        public Containers container = ContextVariablesProvider.Container;

        //Reverse All Normals
        [CommandMethod("KojtoCAD_3D", "KCAD_REVERSE_ALL_NORMALS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Reverse_All_Normals()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                container.ReverseNormals();
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        //Reverse Triangles Normals
        [CommandMethod("KojtoCAD_3D", "KCAD_REVERSE_TRIANGLES_NORMALS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Reverse_Triangles_Normals()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                container.ReverseTrianglesNormals();
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Reverse Nodes Normals
        [CommandMethod("KojtoCAD_3D", "KCAD_REVERSE_NODES_NORMALS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Reverse_Nodes_Normals()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                container.ReverseNodesNormals();
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Reverse Bends Normals
        [CommandMethod("KojtoCAD_3D", "KCAD_REVERSE_BENDS_NORMALS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Reverse_Bends_Normals()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                container.ReverseBendsNormals();
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //----Reverse Normals
        [CommandMethod("KojtoCAD_3D", "KCAD_REVERSE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/REVERSE_NORMALS.htm", "")]
        public void KojtoCAD_3D_Reverse_Normals()
        {
            
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter an option ";
            pKeyOpts.Keywords.Add("All");
            pKeyOpts.Keywords.Add("Triangles");
            pKeyOpts.Keywords.Add("Bends");
            pKeyOpts.Keywords.Add("Nodes");
            pKeyOpts.Keywords.Default = "All";
            pKeyOpts.AllowNone = true;

            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.OK)
            {
                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "All": KojtoCAD_3D_Reverse_All_Normals(); break;
                        case "Triangles": KojtoCAD_3D_Reverse_Triangles_Normals(); break;
                        case "Bends": KojtoCAD_3D_Reverse_Bends_Normals(); break;
                        case "Nodes": KojtoCAD_3D_Reverse_Nodes_Normals(); break;
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        
        //samo za  pryti
        [CommandMethod("KojtoCAD_3D", "KCAD_CHANGE_SELECTED_BENDS_NORMALS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SET_EXPLICIT_BEND_NORMAL.htm", "")]
        public void KojtoCAD_3D_Change_Selected_Bends_Normals()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    if (container.Bends.Count > 0)
                    {
                        Pair<List<Entity>, List<Entity>> pa =
                        GlobalFunctions.GetSelectionOn(ConstantsAndSettings.BendsLayer, ConstantsAndSettings.FictivebendsLayer);

                        PromptPointResult pPtRes;
                        PromptPointOptions pPtOpts = new PromptPointOptions("");
                        pPtOpts.Message = "\nEnter the Start Point of the Direction: ";
                        pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                        if (pPtRes.Status == PromptStatus.OK)
                        {
                            Point3d ptStart = pPtRes.Value;

                            PromptPointResult pPtRes_;
                            PromptPointOptions pPtOpts_ = new PromptPointOptions("");
                            pPtOpts_.Message = "\nEnter the Second Point of the Direction: ";
                            pPtRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts_);
                            if (pPtRes_.Status == PromptStatus.OK)
                            {
                                Point3d ptEnd = pPtRes_.Value;

                                List<Pair<quaternion, quaternion>> pbs_NoFictive = GlobalFunctions.GetPreBends(pa.First);
                                // PRE_BEND_ARRAY pbs_Fictive   = UtilityClasses.GlobalFunctions.GetPreBends(pa.Second);
                                foreach (Pair<quaternion, quaternion> pb in pbs_NoFictive)
                                {
                                    foreach (WorkClasses.Bend bend in container.Bends)
                                    {
                                        if (bend.IsFictive())// || !bend.IsPeripheral())
                                            continue;
                                        else
                                            if (bend == pb)
                                            {
                                                bend.SetNormalByDirection(ptStart, ptEnd);
                                                bend.ExplicitNormal = 1;
                                            }

                                    }
                                }

                                container.SetNodesNormals();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_RESTORE_BENDS_NORMALS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Restore_Bends_Normals()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                    container.RestoreBendsNormals();
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_RESTORE_BEND_NORMAL", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/RESTORE_BEND_NORMAL.htm", "")]
        public void KojtoCAD_3D_Restore_Bend_Normal_By_Selection()
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
                                WorkClasses.Bend TR = null;
                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (bend == pb)
                                    {
                                        TR = bend;
                                        break;
                                    }
                                }
                                //------------
                                if ((object)TR != null)
                                {

                                    quaternion m = TR.MidPoint;
                                    if ((object)TR.MidPoint == null)
                                    {
                                        m = (TR.Start + TR.End) / 2.0;
                                        TR.MidPoint = m;
                                    }

                                    quaternion q1 = container.Triangles[TR.FirstTriangleNumer].Normal.Second - container.Triangles[TR.FirstTriangleNumer].Normal.First;
                                    quaternion q3 = m + q1;
                                    q1 /= q1.abs();
                                    if (TR.SecondTriangleNumer < 0)
                                    {
                                        if (ConstantsAndSettings.PerepherialBendsNormalDirection == 0)
                                        {
                                            TR.Normal = q3;
                                        }
                                        else
                                        {
                                            UCS trUCS = new UCS(container.Triangles[TR.FirstTriangleNumer].Normal.First, TR.Start, TR.End);
                                            bool bSign = (trUCS.FromACS(container.Triangles[TR.FirstTriangleNumer].Normal.Second).GetZ() >= 0) ? true : false;
                                            if (!bSign)
                                                trUCS = new UCS(container.Triangles[TR.FirstTriangleNumer].Normal.First, TR.End, TR.Start);

                                            if (ConstantsAndSettings.PerepherialBendsNormalDirection == 1)
                                            {
                                                quaternion q2 = new quaternion(0, 0, 0, 1) + m;
                                                TR.Normal = q2;
                                                if (Math.Abs(trUCS.FromACS(q2).GetZ()) <= ConstantsAndSettings.MinDistBhetwenNodes)
                                                {
                                                    if ((container.Triangles[TR.FirstTriangleNumer].Normal.First - m).abs() < (container.Triangles[TR.FirstTriangleNumer].Normal.First - q2).abs())
                                                    {
                                                        TR.Normal = m + m - q2;
                                                    }
                                                }
                                                else
                                                    if (trUCS.FromACS(m + (q2 - m) * 10000.0).GetZ() < 0.0)
                                                    {
                                                        TR.Normal = m + m - q2;
                                                    }
                                            }
                                            if (ConstantsAndSettings.PerepherialBendsNormalDirection == 2)
                                            {
                                                quaternion q2 = new quaternion(0, 0, 1, 0) + m;
                                                TR.Normal = q2;
                                                if (Math.Abs(trUCS.FromACS(q2).GetZ()) <= ConstantsAndSettings.MinDistBhetwenNodes)
                                                {
                                                    if ((container.Triangles[TR.FirstTriangleNumer].Normal.First - m).abs() < (container.Triangles[TR.FirstTriangleNumer].Normal.First - q2).abs())
                                                    {
                                                        TR.Normal = m + m - q2;
                                                    }
                                                }
                                                else
                                                    if (trUCS.FromACS(m + (q2 - m) * 10000.0).GetZ() < 0.0)
                                                    {
                                                        TR.Normal = m + m - q2;
                                                    }
                                            }
                                            if (ConstantsAndSettings.PerepherialBendsNormalDirection == 3)
                                            {
                                                quaternion q2 = new quaternion(0, 1, 0, 0) + m;
                                                TR.Normal = q2;
                                                if (Math.Abs(trUCS.FromACS(q2).GetZ()) <= ConstantsAndSettings.MinDistBhetwenNodes)
                                                {
                                                    if ((container.Triangles[TR.FirstTriangleNumer].Normal.First - m).abs() < (container.Triangles[TR.FirstTriangleNumer].Normal.First - q2).abs())
                                                    {
                                                        TR.Normal = m + m - q2;
                                                    }
                                                }
                                                else
                                                    if (trUCS.FromACS(m + (q2 - m) * 10000.0).GetZ() < 0.0)
                                                    {
                                                        TR.Normal = m + m - q2;
                                                    }
                                            }
                                        }

                                        quaternion bak = TR.Normal;
                                        UCS ucs = TR.GetUCS();
                                        TR.Normal = ucs.ToACS(new quaternion(0, 0, 1.0, 0));
                                        if (double.IsNaN(TR.Normal.GetX()) || double.IsNaN(TR.Normal.GetY()) || double.IsNaN(TR.Normal.GetZ()))
                                        {
                                            TR.Normal = bak;
                                            string mess = string.Format("Bend Numer {0} Normal Error !", TR.Numer + 1);
                                            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(mess);
                                            MessageBox.Show(mess, "E R R O R");
                                        }
                                    }
                                    else
                                    {
                                        quaternion q2 = container.Triangles[TR.SecondTriangleNumer].Normal.Second - container.Triangles[TR.SecondTriangleNumer].Normal.First;
                                        q2 /= q2.abs();

                                        TR.Normal = (q1 + q2) / 2.0;
                                        TR.Normal /= TR.Normal.abs();
                                        TR.Normal += m;
                                    }
                                }
                                //------------
                                else
                                {
                                    MessageBox.Show("\nBend not found - E R R O R  !", "E R R O R - Selection Bend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ed.WriteMessage("\nBend not found - E R R O R  !");
                                }
                            }
                            else
                            {
                                MessageBox.Show("\nDistance between selected Points is less - E R R O R  !", "E R R O R - Selection Bend", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        //explicit node normal
        [CommandMethod("KojtoCAD_3D", "KCAD_SET_EXPLICIT_NODE_NORMAL_BY_POSITION", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SET_EXPLICIT_NODE_NORMAL_BY_POSITION.htm", "")]
        public void KojtoCAD_3D_SetExplicitNodeNormalByNodePosition()
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

                    pPtOpts.Message = "\nEnter the Point of the Node: ";
                    pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);

                    if (pPtRes.Status == PromptStatus.OK)
                    {
                        quaternion nq = new quaternion(0, pPtRes.Value.X, pPtRes.Value.Y, pPtRes.Value.Z);
                        WorkClasses.Node Node = null;
                        foreach (WorkClasses.Node node in container.Nodes)
                        {
                            if (node == nq)
                            {
                                Node = node;
                                break;
                            }
                        }
                        if ((object)Node != null)
                        {
                            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                            pKeyOpts.Message = "\nEnter an option ";
                            pKeyOpts.Keywords.Add("Selection");
                            pKeyOpts.Keywords.Add("NodeCentroid");
                            pKeyOpts.Keywords.Default = "Selection";
                            pKeyOpts.AllowNone = true;
                            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                            if (pKeyRes.Status == PromptStatus.OK)
                            {
                                PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                                pDoubleOpts.Message = "\n Enter Explicit Length of the Normal: ";
                                pDoubleOpts.DefaultValue = 1.0;
                                pDoubleOpts.AllowZero = false;
                                pDoubleOpts.AllowNegative = false;

                                PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                                if (pDoubleRes.Status == PromptStatus.OK)
                                {
                                    switch (pKeyRes.StringResult)
                                    {
                                        case "NodeCentroid":
                                            quaternion q = GetNodeCentroidByNoFictiveBendsMidPoints(Node.Numer);
                                            q = ((q - Node.Position).angTo(Node.Normal - Node.Position) <= Math.PI / 2.0) ? q - Node.Position : Node.Position - q;
                                            q /= q.abs();
                                            q += Node.Position;
                                            Node.SetExplicitNormal(q);
                                            Node.ExplicitNormalLength = pDoubleRes.Value;
                                            break;
                                        case "Selection":
                                            PromptPointResult pPtRes_;
                                            PromptPointOptions pPtOpts_ = new PromptPointOptions("");
                                            pPtOpts_.Message = "\nEnter the Direction Point of the Normal: ";
                                            pPtRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts_);
                                            if (pPtRes_.Status == PromptStatus.OK)
                                            {
                                                quaternion Q = new quaternion(0, pPtRes_.Value.X, pPtRes_.Value.Y, pPtRes_.Value.Z);
                                                Q = ((Q - Node.Position).angTo(Node.Normal - Node.Position) <= Math.PI / 2.0) ? Q - Node.Position : Node.Position - Q;
                                                Q /= Q.abs();
                                                Q += Node.Position;
                                                Node.SetExplicitNormal(Q);
                                                Node.ExplicitNormalLength = pDoubleRes.Value;
                                            }
                                            else
                                            {
                                                MessageBox.Show("\nPoint not found - E R R O R  !", "E R R O R - Selection Point", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                ed.WriteMessage("\nPoint not found - E R R O R  !");
                                            }
                                            break;
                                    }
                                }
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
        private quaternion GetNodeCentroidByNoFictiveBendsMidPoints(int nodeNumer)
        {
            quaternion rez = new quaternion();
            WorkClasses.Node node = container.Nodes[nodeNumer];
            foreach (int B in node.Bends_Numers_Array)
            {
                WorkClasses.Bend bend = container.Bends[B];
                if (!bend.IsFictive())
                {
                    quaternion q = bend.MidPoint;
                    //quaternion q = bend.MidPoint - node.Position;
                    //q /= q.abs();
                    //q *= 300;
                    rez += q;
                }
            }
            rez /= node.Bends_Numers_Array.Count;
            // rez += node.Position;
            return rez;
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SET_ALL_NODES_NORMALS_EXPLICIT", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SET_ALL_NODES_NORMALS_EXPLICIT.htm", "")]
        public void KojtoCAD_3D_Set_All_NodeNormal_ExplicitByNodeBendsMidpointsCentroid()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message = "\n Enter Explicit Length of the Normal: ";
                    pDoubleOpts.DefaultValue = 1.0;
                    pDoubleOpts.AllowZero = false;
                    pDoubleOpts.AllowNegative = false;

                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
                        foreach (WorkClasses.Node Node in container.Nodes)
                        {
                            quaternion q = GetNodeCentroidByNoFictiveBendsMidPoints(Node.Numer);
                            q = ((q - Node.Position).angTo(Node.Normal - Node.Position) <= Math.PI / 2.0) ? q - Node.Position : Node.Position - q;
                            q /= q.abs();
                            q += Node.Position;
                            Node.SetExplicitNormal(q);
                            Node.ExplicitNormalLength = pDoubleRes.Value;
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_RESTORE_NODES_NORMALS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Restore_Nodes_Normals()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {
                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                    container.RestoreNodesNormals();
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_RESTORE_NODE_NORMAL_BY_SELECTION", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/RESTORE_NODE_NORMAL_BY_SELECTION.htm", "")]
        public void KojtoCAD_3D_Restore_Node_Normal_By_Selection()
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

                    pPtOpts.Message = "\nEnter the Point of the Node: ";
                    pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);

                    if (pPtRes.Status == PromptStatus.OK)
                    {
                        quaternion nq = new quaternion(0, pPtRes.Value.X, pPtRes.Value.Y, pPtRes.Value.Z);
                        WorkClasses.Node Node = null;

                        foreach (WorkClasses.Node node in container.Nodes)
                        {
                            if (node == nq)
                            {
                                Node = node;
                                break;
                            }
                        }
                        if ((object)Node != null)
                        {
                            container.RestoreNodesNormals(Node.Numer);
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_CHANGE_EXPLICIT_NORMAL_LENGTH_SELECTION", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CHANGE_EXPLICIT_NORMAL_LENGTH_SELECTION.htm", "")]
        public void KojtoCAD_3D_Change_Explicit_Normal_Length_Selection()
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

                    pPtOpts.Message = "\nEnter the Point of the Node: ";
                    pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);

                    if (pPtRes.Status == PromptStatus.OK)
                    {
                        quaternion nq = new quaternion(0, pPtRes.Value.X, pPtRes.Value.Y, pPtRes.Value.Z);
                        WorkClasses.Node Node = null;

                        foreach (WorkClasses.Node node in container.Nodes)
                        {
                            if (node == nq)
                            {
                                Node = node;
                                break;
                            }
                        }
                        if ((object)Node != null)
                        {
                            PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                            pDoubleOpts_.Message = "\n Enter new Explicit Length : ";
                            pDoubleOpts_.DefaultValue = 0.0;
                            pDoubleOpts_.AllowZero = true;
                            pDoubleOpts_.AllowNegative = true;

                            PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                            if (pDoubleRes_.Status == PromptStatus.OK)
                            {
                                Node.ExplicitNormalLength = pDoubleRes_.Value;
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
    }
}
