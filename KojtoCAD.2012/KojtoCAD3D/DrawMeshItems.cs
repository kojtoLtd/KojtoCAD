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
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.DrawMeshItems))]

namespace KojtoCAD.KojtoCAD3D
{
    public class DrawMeshItems
    {
        public Containers container = ContextVariablesProvider.Container;
        //Draw all Bends
        //Draw in current Layer
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_ALL_BENDS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_All_Bends()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl;
                    acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (WorkClasses.Bend b1 in container.Bends)
                    {

                        Line l1 = new Line(new Point3d(b1.Start.GetX(), b1.Start.GetY(), b1.Start.GetZ()), new Point3d(b1.End.GetX(), b1.End.GetY(), b1.End.GetZ()));

                        acBlkTblRec.AppendEntity(l1);
                        tr.AddNewlyCreatedDBObject(l1, true);
                    }

                    tr.Commit();
                    ed.UpdateScreen();
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw NoFictive Bends
        //Draw in Bends Layer
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NON_FICTIVE_BENDS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_NonFictive_Bends()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        bool question = false;

                        foreach (WorkClasses.Bend b1 in container.Bends)
                        {
                            if (!b1.Fictive)
                            {
                                Line l1 = new Line(new Point3d(b1.Start.GetX(), b1.Start.GetY(), b1.Start.GetZ()), new Point3d(b1.End.GetX(), b1.End.GetY(), b1.End.GetZ()));

                                acBlkTblRec.AppendEntity(l1);
                                tr.AddNewlyCreatedDBObject(l1, true);
                                try
                                {
                                    l1.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                }
                                catch
                                {
                                    if (!question)
                                    {
                                        if (MessageBox.Show(" Layer is missing: " + UtilityClasses.ConstantsAndSettings.BendsLayer + " !\n\nDo you want to create it?",
                                            "ERROR while drawing bends! Required layer is missing! !", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            LayerTable acLyrTbl = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                                            LayerTableRecord acLyrTblRec = new LayerTableRecord();
                                            acLyrTblRec.Name = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                            acLyrTbl.UpgradeOpen();

                                            acLyrTbl.Add(acLyrTblRec);
                                            tr.AddNewlyCreatedDBObject(acLyrTblRec, true);
                                            l1.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                        }
                                        else
                                        {
                                            question = true;
                                        }
                                    }
                                }
                            }
                        }

                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Fictive Bends
        //Draw in Fictive Bends Layer
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_FICTIVE_BENDS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_Fictive_Bends()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        bool question = false;

                        foreach (WorkClasses.Bend b1 in container.Bends)
                        {
                            if (b1.Fictive)
                            {
                                Line l1 = new Line(new Point3d(b1.Start.GetX(), b1.Start.GetY(), b1.Start.GetZ()), new Point3d(b1.End.GetX(), b1.End.GetY(), b1.End.GetZ()));

                                acBlkTblRec.AppendEntity(l1);
                                tr.AddNewlyCreatedDBObject(l1, true);
                                try
                                {
                                    l1.Layer = UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                }
                                catch
                                {
                                    if (!question)
                                    {
                                        if (MessageBox.Show(" Layer is missing: " + UtilityClasses.ConstantsAndSettings.FictivebendsLayer + " !\n\nDo you want to create it?",
                                            "ERROR while drawing fictive bends! Required layer is missing !", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            LayerTable acLyrTbl = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                                            LayerTableRecord acLyrTblRec = new LayerTableRecord();
                                            acLyrTblRec.Name = UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                            acLyrTbl.UpgradeOpen();

                                            acLyrTbl.Add(acLyrTblRec);
                                            tr.AddNewlyCreatedDBObject(acLyrTblRec, true);
                                            l1.Layer = UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                        }
                                        else
                                        {
                                            question = true;
                                        }
                                    }
                                }
                            }
                        }

                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Perepherial Bends
        //Draw in Bends Layer
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_PEREFERIAL_BENDS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_Perepherial_Bends()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        bool question = false;

                        foreach (WorkClasses.Bend b1 in container.Bends)
                        {
                            if (b1.SecondTriangleNumer < 0)
                            {
                                Line l1 = new Line(new Point3d(b1.Start.GetX(), b1.Start.GetY(), b1.Start.GetZ()), new Point3d(b1.End.GetX(), b1.End.GetY(), b1.End.GetZ()));

                                acBlkTblRec.AppendEntity(l1);
                                tr.AddNewlyCreatedDBObject(l1, true);
                                try
                                {
                                    l1.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                }
                                catch
                                {
                                    if (!question)
                                    {
                                        if (MessageBox.Show("Layer is missing: " + UtilityClasses.ConstantsAndSettings.BendsLayer + " !\n\nDo you want to create it?",
                                            "ERROR while drawing bends! Required layer is missing! !", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            LayerTable acLyrTbl = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                                            LayerTableRecord acLyrTblRec = new LayerTableRecord();
                                            acLyrTblRec.Name = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                            acLyrTbl.UpgradeOpen();

                                            acLyrTbl.Add(acLyrTblRec);
                                            tr.AddNewlyCreatedDBObject(acLyrTblRec, true);
                                            l1.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                        }
                                        else
                                        {
                                            question = true;
                                        }
                                    }
                                }
                            }
                        }

                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw NoPerepherial Bends and NoFictive
        //Draw in Bends Layer
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NON_PEREFERIAL_NOT_FICTIVE_BENDS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_NoPerepherial_Bends()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        bool question = false;

                        foreach (WorkClasses.Bend b1 in container.Bends)
                        {
                            if (b1.SecondTriangleNumer >= 0)
                            {
                                if (!b1.Fictive)
                                {
                                    Line l1 = new Line(new Point3d(b1.Start.GetX(), b1.Start.GetY(), b1.Start.GetZ()), new Point3d(b1.End.GetX(), b1.End.GetY(), b1.End.GetZ()));

                                    acBlkTblRec.AppendEntity(l1);
                                    tr.AddNewlyCreatedDBObject(l1, true);
                                    try
                                    {
                                        l1.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                    }
                                    catch
                                    {
                                        if (!question)
                                        {
                                            if (MessageBox.Show("Layer is missing: " + UtilityClasses.ConstantsAndSettings.BendsLayer + " !\n\nDo you want to create it?",
                                                "ERROR while drawing bends! Required layer is missing! !", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                            {
                                                LayerTable acLyrTbl = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                                                LayerTableRecord acLyrTblRec = new LayerTableRecord();
                                                acLyrTblRec.Name = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                                acLyrTbl.UpgradeOpen();

                                                acLyrTbl.Add(acLyrTblRec);
                                                tr.AddNewlyCreatedDBObject(acLyrTblRec, true);
                                                l1.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                            }
                                            else
                                            {
                                                question = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Poligons Perepherial Bends
        //Draw in Bends Layer
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_POLIGONS_PEREFERIAL_BENDS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_Poligon_Pereferial_Bends()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        bool question = false;

                        foreach (WorkClasses.Polygon p in container.Polygons)
                        {
                            foreach (int N in p.Bends_Numers_Array)
                            {

                                WorkClasses.Bend b1 = container.Bends[N];

                                Line l1 = new Line(new Point3d(b1.Start.GetX(), b1.Start.GetY(), b1.Start.GetZ()), new Point3d(b1.End.GetX(), b1.End.GetY(), b1.End.GetZ()));

                                acBlkTblRec.AppendEntity(l1);
                                tr.AddNewlyCreatedDBObject(l1, true);
                                try
                                {
                                    l1.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                }
                                catch
                                {
                                    if (!question)
                                    {
                                        if (MessageBox.Show("Layer is missing: " + UtilityClasses.ConstantsAndSettings.BendsLayer + " !\n\nDo you want to create it?",
                                            "ERROR while drawing bends! Required layer is missing! !", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            LayerTable acLyrTbl = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                                            LayerTableRecord acLyrTblRec = new LayerTableRecord();
                                            acLyrTblRec.Name = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                            acLyrTbl.UpgradeOpen();

                                            acLyrTbl.Add(acLyrTblRec);
                                            tr.AddNewlyCreatedDBObject(acLyrTblRec, true);
                                            l1.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                        }
                                        else
                                        {
                                            question = true;
                                        }
                                    }
                                }
                            }

                        }
                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Triangles Normals
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_TRIANGLES_NORMALS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_Triangles_Normals()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    double k = UtilityClasses.ConstantsAndSettings.NormlLengthToShow;

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Triangle t in container.Triangles)
                        {
                            if (((object)t.Normal != null) && ((object)t.Normal.Second != null))
                            {
                                quaternion q = t.Normal.Second - t.Normal.First;
                                q *= k;
                                q += t.Normal.First;
                                Line l1 = new Line(new Point3d(t.Normal.First.GetX(), t.Normal.First.GetY(), t.Normal.First.GetZ()),
                                    new Point3d(q.GetX(), q.GetY(), q.GetZ()));

                                acBlkTblRec.AppendEntity(l1);
                                tr.AddNewlyCreatedDBObject(l1, true);
                            }
                            else
                            {
                                string mess = string.Format("Triangle: {0}\n----------\n\nNormal is not Defined", t.Numer);
                                MessageBox.Show(mess, "Triangles were NOT properly calculated !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }
                        }

                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Nodes Normals
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NODES_NORMALS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_Nodes_Normals()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    double k = UtilityClasses.ConstantsAndSettings.NormlLengthToShow;

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Node n in container.Nodes)
                        {
                            if ((object)n.Normal != null)
                            {
                                quaternion q = n.Normal - n.Position;

                                q *= (((object)n.ExplicitNormal != null) ? n.ExplicitNormalLength : k);
                                q += n.Position;
                                Line l1 = new Line(new Point3d(n.Position.GetX(), n.Position.GetY(), n.Position.GetZ()),
                                    new Point3d(q.GetX(), q.GetY(), q.GetZ()));

                                acBlkTblRec.AppendEntity(l1);
                                tr.AddNewlyCreatedDBObject(l1, true);
                            }
                            else
                            {
                                string mess = string.Format("Node: {0}\n----------\n\nNormal is not Defined", n.Numer);
                                MessageBox.Show(mess, "Nodes Normals - E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }
                        }

                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Nodes Normals By NoFictive Bends
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NODES_NORMALS_BY_NOFICTIVE", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_Nodes_Normals_By_NoFictiweBends()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    double k = UtilityClasses.ConstantsAndSettings.NormlLengthToShow;

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Node n in container.Nodes)
                        {
                            if ((object)n.Normal != null)
                            {
                                bool NoFictive = false;
                                foreach (int N in n.Bends_Numers_Array)
                                {
                                    if (!container.Bends[N].IsFictive())
                                    {
                                        NoFictive = true;
                                        break;
                                    }
                                }
                                if (NoFictive)
                                {
                                    quaternion Q = n.GetNodesNormalsByNoFictiveBends(ref container);
                                    quaternion q = Q - n.Position;

                                    q *= (((object)n.ExplicitNormal != null) ? n.ExplicitNormalLength : k);
                                    q += n.Position;

                                    Line l1 = new Line(new Point3d(n.Position.GetX(), n.Position.GetY(), n.Position.GetZ()),
                                        new Point3d(q.GetX(), q.GetY(), q.GetZ()));

                                    acBlkTblRec.AppendEntity(l1);
                                    tr.AddNewlyCreatedDBObject(l1, true);
                                }
                            }
                            else
                            {
                                string mess = string.Format("Node: {0}\n----------\n\nNormal is not Defined", n.Numer);
                                MessageBox.Show(mess, "Nodes Normals By NoFictive Bends- E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }
                        }

                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NODES_NORMALS_BY_NOFICTIVE_HELP", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_Nodes_Normals_By_NoFictiweBends_help()
        {
            
            UtilityClasses.GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/DRAW_NODES_NORMALS_BY_NOFICTIVE.htm");
        }

        //Draw Bends Normals
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_BENDS_NORMALS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Draw_Bends_Normals()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    double k = UtilityClasses.ConstantsAndSettings.NormlLengthToShow;

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Bend b in container.Bends)
                        {
                            if ((object)b.Normal != null)
                            {
                                quaternion q = b.Normal - b.MidPoint;
                                q *= k;
                                q += b.MidPoint;
                                Line l1 = new Line(new Point3d(b.MidPoint.GetX(), b.MidPoint.GetY(), b.MidPoint.GetZ()),
                                    new Point3d(q.GetX(), q.GetY(), q.GetZ()));

                                acBlkTblRec.AppendEntity(l1);
                                tr.AddNewlyCreatedDBObject(l1, true);
                            }
                            else
                            {
                                string mess = string.Format("Bend: {0}\n----------\n\nNormal is not Defined", b.Numer);
                                MessageBox.Show(mess, "Bends Normals - E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }
                        }

                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Triangles Numers
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_TRIANGLES_NUMERS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Draw_Triangles_Numers()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                    pDoubleOpts_.Message = "\n Enter offset from triangle Plane: ";
                    pDoubleOpts_.DefaultValue = 0.0;
                    pDoubleOpts_.AllowZero = true;
                    pDoubleOpts_.AllowNegative = true;

                    PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                    if (pDoubleRes_.Status == PromptStatus.OK)
                    {
                        double offset = pDoubleRes_.Value;

                        UtilityClasses.Pair<double, PromptStatus> textHeight =
                                                       UtilityClasses.GlobalFunctions.GetDouble(7.0, "Enter Text Height", false, false);
                        if (textHeight.Second == PromptStatus.OK)
                        {
                            foreach (WorkClasses.Triangle tr in container.Triangles)
                            {
                                UCS ucs = tr.GetUcsByCentroid1();
                                Matrix3d acMat3d = UtilityClasses.GlobalFunctions.MathLibKCADUcsToMatrix3d(ref ucs);

                                using (Transaction acTrans = db.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    double len1 = container.Bends[tr.GetFirstBendNumer()].Length;
                                    double len2 = container.Bends[tr.GetSecondBendNumer()].Length;
                                    double len3 = container.Bends[tr.GetThirdBendNumer()].Length;

                                    double len = (len1 < len2) ? len1 : len2;
                                    len = (len < len3) ? len : len3;

                                    DBText acText = new DBText();
                                    acText.SetDatabaseDefaults();
                                    acText.Position = new Point3d(len / 30.0, len / 30.0, offset);
                                    acText.Height = textHeight.First;
                                    acText.TextString = (tr.Numer + 1).ToString();

                                    acText.TransformBy(acMat3d);

                                    acBlkTblRec.AppendEntity(acText);
                                    acTrans.AddNewlyCreatedDBObject(acText, true);

                                    Line l1 = new Line(new Point3d(0, 0, offset), new Point3d(len / 30.0, 0, offset));
                                    Line l2 = new Line(new Point3d(0, 0, offset), new Point3d(0, len / 30.0, offset));

                                    l1.TransformBy(acMat3d);
                                    l2.TransformBy(acMat3d);

                                    acBlkTblRec.AppendEntity(l1);
                                    acTrans.AddNewlyCreatedDBObject(l1, true);

                                    acBlkTblRec.AppendEntity(l2);
                                    acTrans.AddNewlyCreatedDBObject(l2, true);

                                    acTrans.Commit();
                                    ed.UpdateScreen();
                                }
                            }
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Triangles !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Bends Numers
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_BENDSS_NUMERS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Draw_Bends_Numers()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                    pDoubleOpts_.Message = "\n Enter offset srom triangle Plane: ";
                    pDoubleOpts_.DefaultValue = 0.0;
                    pDoubleOpts_.AllowZero = true;
                    pDoubleOpts_.AllowNegative = true;

                    PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                    if (pDoubleRes_.Status == PromptStatus.OK)
                    {
                        double offset = pDoubleRes_.Value;

                        UtilityClasses.Pair<double, PromptStatus> textHeight =
                                                       UtilityClasses.GlobalFunctions.GetDouble(7.0, "Enter Text Height", false, false);
                        if (textHeight.Second == PromptStatus.OK)
                        {
                            foreach (WorkClasses.Bend bend in container.Bends)
                            {

                                UCS ucs = new UCS();
                                if (container.Triangles[bend.FirstTriangleNumer].GetFirstBendNumer() == bend.Numer)
                                    ucs = container.Triangles[bend.FirstTriangleNumer].GetUcsByBends1(ref container.Bends);
                                else
                                    if (container.Triangles[bend.FirstTriangleNumer].GetSecondBendNumer() == bend.Numer)
                                        ucs = container.Triangles[bend.FirstTriangleNumer].GetUcsByBends2(ref container.Bends);
                                    else
                                        ucs = container.Triangles[bend.FirstTriangleNumer].GetUcsByBends3(ref container.Bends);

                                Matrix3d acMat3d = UtilityClasses.GlobalFunctions.MathLibKCADUcsToMatrix3d(ref ucs);

                                using (Transaction acTrans = db.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    double len = bend.Length;

                                    DBText acText = new DBText();
                                    acText.SetDatabaseDefaults();
                                    acText.Position = new Point3d(len / 30.0, len / 30.0, offset);
                                    acText.Height = textHeight.First;
                                    acText.TextString = (bend.Numer + 1).ToString();

                                    acText.TransformBy(acMat3d);

                                    acBlkTblRec.AppendEntity(acText);
                                    acTrans.AddNewlyCreatedDBObject(acText, true);

                                    Line l1 = new Line(new Point3d(0, 0, offset), new Point3d(len / 30.0, 0, offset));
                                    Line l2 = new Line(new Point3d(0, 0, offset), new Point3d(0, len / 30.0, offset));

                                    l1.TransformBy(acMat3d);
                                    l2.TransformBy(acMat3d);

                                    acBlkTblRec.AppendEntity(l1);
                                    acTrans.AddNewlyCreatedDBObject(l1, true);

                                    acBlkTblRec.AppendEntity(l2);
                                    acTrans.AddNewlyCreatedDBObject(l2, true);

                                    acTrans.Commit();
                                    ed.UpdateScreen();
                                }
                            }
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        //Draw Nodes Numers
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NODES_NUMERS", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Draw_Nodes_Numers()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                    pDoubleOpts_.Message = "\n Enter offset from Node Position: ";
                    pDoubleOpts_.DefaultValue = 0.0;
                    pDoubleOpts_.AllowZero = true;
                    pDoubleOpts_.AllowNegative = true;

                    PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                    if (pDoubleRes_.Status == PromptStatus.OK)
                    {
                        double offset = pDoubleRes_.Value;

                        UtilityClasses.Pair<double, PromptStatus> textHeight =
                                                      UtilityClasses.GlobalFunctions.GetDouble(7.0, "Enter Text Height", false, false);
                        if (textHeight.Second == PromptStatus.OK)
                        {
                            foreach (WorkClasses.Node node in container.Nodes)
                            {
                                UCS ucs0 = new UCS(node.Position, node.Normal, container.Triangles[node.Triangles_Numers_Array[0]].Normal.First);
                                UCS ucs = new UCS(node.Position, ucs0.ToACS(new quaternion(0, 0, 0, 1)), ucs0.ToACS(new quaternion(0, 0, 1, 0)));
                                if (ucs.FromACS(node.Normal).GetZ() < 0)
                                    ucs = new UCS(node.Position, ucs0.ToACS(new quaternion(0, 0, 0, -1)), ucs0.ToACS(new quaternion(0, 0, 1, 0)));

                                Matrix3d acMat3d = UtilityClasses.GlobalFunctions.MathLibKCADUcsToMatrix3d(ref ucs);

                                using (Transaction acTrans = db.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    double len = container.Bends[node.Bends_Numers_Array[0]].Length;

                                    DBText acText = new DBText();
                                    acText.SetDatabaseDefaults();
                                    acText.Position = new Point3d(len / 50.0, len / 50.0, offset);
                                    acText.Height = textHeight.First;
                                    acText.TextString = (node.Numer + 1).ToString();

                                    acText.TransformBy(acMat3d);

                                    acBlkTblRec.AppendEntity(acText);
                                    acTrans.AddNewlyCreatedDBObject(acText, true);

                                    Line l1 = new Line(new Point3d(0, 0, offset), new Point3d(len / 30.0, 0, offset));
                                    Line l2 = new Line(new Point3d(0, 0, offset), new Point3d(0, len / 30.0, offset));

                                    l1.TransformBy(acMat3d);
                                    l2.TransformBy(acMat3d);


                                    acBlkTblRec.AppendEntity(l1);
                                    acTrans.AddNewlyCreatedDBObject(l1, true);

                                    acBlkTblRec.AppendEntity(l2);
                                    acTrans.AddNewlyCreatedDBObject(l2, true);



                                    acTrans.Commit();
                                    ed.UpdateScreen();
                                }
                            }
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/DRAW_MESH.htm", "")]
        public void KojtoCAD_3D_Draw()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.Keywords.Add("NoFictive");
                pKeyOpts.Keywords.Add("Fictive");
                pKeyOpts.Keywords.Add("Perepherial");
                pKeyOpts.Keywords.Add("bNoPerepherialBendsAndNoFictive");
                pKeyOpts.Keywords.Add("Polygons");
                pKeyOpts.Keywords.Default = "NoFictive";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "All": KojtoCAD_3D_Method_Draw_All_Bends(); break;
                        case "NoFictive": KojtoCAD_3D_Method_Draw_NonFictive_Bends(); break;
                        case "Fictive": KojtoCAD_3D_Method_Draw_Fictive_Bends(); break;
                        case "Perepherial": KojtoCAD_3D_Method_Draw_Perepherial_Bends(); break;
                        case "bNoPerepherialBendsAndNoFictive": KojtoCAD_3D_Method_Draw_NoPerepherial_Bends(); break;
                        case "Polygons": KojtoCAD_3D_Method_Draw_Poligon_Pereferial_Bends(); break;
                        // case "TrianglesNormals": KojtoCAD_3D_Method_Draw_Triangles_Normals(); break;
                        // case "BendsNormals": KojtoCAD_3D_Method_Draw_Bends_Normals(); break;
                        //case "NodesNormals": KojtoCAD_3D_Method_Draw_Nodes_Normals(); break;
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Normals
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NORMALS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/DRAW_NORMALS.htm", "")]
        public void KojtoCAD_3D_Draw_Normals()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.Keywords.Add("Triangles");
                pKeyOpts.Keywords.Add("Bends");
                pKeyOpts.Keywords.Add("Nodes");
                pKeyOpts.Keywords.Default = "Triangles";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "All": KojtoCAD_3D_Method_Draw_Triangles_Normals();
                            KojtoCAD_3D_Method_Draw_Bends_Normals();
                            KojtoCAD_3D_Method_Draw_Nodes_Normals();
                            KojtoCAD_3D_Method_Draw_Nodes_Normals_By_NoFictiweBends();
                            break;
                        case "Triangles": KojtoCAD_3D_Method_Draw_Triangles_Normals(); break;
                        case "Bends": KojtoCAD_3D_Method_Draw_Bends_Normals(); break;
                        case "Nodes": KojtoCAD_3D_Method_Draw_Nodes_Normals(); break;
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Draw Numers
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NUMERS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/DRAW_NUMERS.htm", "")]
        public void KojtoCAD_3D_Draw_Numers()
        {
            
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter an option ";
            pKeyOpts.Keywords.Add("Triangles");
            pKeyOpts.Keywords.Add("Bends");
            pKeyOpts.Keywords.Add("Nodes");
            pKeyOpts.Keywords.Default = "Triangles";
            pKeyOpts.AllowNone = true;

            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.OK)
            {
                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "Triangles": KojtoCAD_3D_Draw_Triangles_Numers(); break;
                        case "Bends": KojtoCAD_3D_Draw_Bends_Numers(); break;
                        case "Nodes": KojtoCAD_3D_Draw_Nodes_Numers(); break;
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Draw Node UCS
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NODE_UCS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/DRAW_NODE_UCS.htm", "")]
        public void KojtoCAD_3D_Draw_NodeUCS()
        {
            
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter an option (Node by Numer or by Selection ?)";
            pKeyOpts.Keywords.Add("Numer");
            pKeyOpts.Keywords.Add("Selection");
            pKeyOpts.Keywords.Default = "Selection";
            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.OK)
            {
                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "Numer": KojtoCAD_3D_Draw_NodeUCS_By_Numer(); break;
                        case "Selection": KojtoCAD_3D_Draw_NodeUCS_By_Selection(); break;
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NODE_UCS_BY_NUMER", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Draw_NodeUCS_By_Numer()
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
                            WorkClasses.Node Node = container.Nodes[N];
                            if ((object)Node != null)
                            {
                                bool isFictive = true;
                                foreach (int nN in Node.Bends_Numers_Array)
                                {
                                    if (!container.Bends[nN].IsFictive())
                                    {
                                        isFictive = false;
                                        break;
                                    }
                                }

                                if (!isFictive)
                                {
                                    UCS ucs = Node.CreateNodeUCS(0, ref container);
                                    Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());
                                    UtilityClasses.GlobalFunctions.DrawLine(new Point3d(), new Point3d(UtilityClasses.ConstantsAndSettings.NormlLengthToShow, 0, 0), 3, ref mat);
                                    UtilityClasses.GlobalFunctions.DrawLine(new Point3d(), new Point3d(0, UtilityClasses.ConstantsAndSettings.NormlLengthToShow * 1.5, 0), 2, ref mat);
                                    UtilityClasses.GlobalFunctions.DrawLine(new Point3d(), new Point3d(0, 0, UtilityClasses.ConstantsAndSettings.NormlLengthToShow * 2.0), 1, ref mat);
                                }
                                else
                                    MessageBox.Show("Fictive Node !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_NODE_UCS_BY_SELECTION", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Draw_NodeUCS_By_Selection()
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
                    pPtOpts.Message = "\nSelect Point: ";
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
                            bool isFictive = true;
                            foreach (int nN in Node.Bends_Numers_Array)
                            {
                                if (!container.Bends[nN].IsFictive())
                                {
                                    isFictive = false;
                                    break;
                                }
                            }
                            if (!isFictive)
                            {
                                UCS ucs = Node.CreateNodeUCS(0, ref container);
                                Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());
                                UtilityClasses.GlobalFunctions.DrawLine(new Point3d(), new Point3d(UtilityClasses.ConstantsAndSettings.NormlLengthToShow, 0, 0), 3, ref mat);
                                UtilityClasses.GlobalFunctions.DrawLine(new Point3d(), new Point3d(0, UtilityClasses.ConstantsAndSettings.NormlLengthToShow * 1.5, 0), 2, ref mat);
                                UtilityClasses.GlobalFunctions.DrawLine(new Point3d(), new Point3d(0, 0, UtilityClasses.ConstantsAndSettings.NormlLengthToShow * 2.0), 1, ref mat);
                            }
                            else
                                MessageBox.Show("Fictive Node !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_SECOND_MESH", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/DRAW_SECOND_MESH.htm", "")]
        public void KojtoCAD_3D_Draw_Second_Mesh()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Layer: ");
                pStrOpts.AllowSpaces = false;
                pStrOpts.DefaultValue = UtilityClasses.ConstantsAndSettings.BendsLayer;
                PromptResult pStrRes;

                pStrRes = ed.GetString(pStrOpts);
                if (pStrRes.Status == PromptStatus.OK)
                {
                    string layout = pStrRes.StringResult;
                    if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                    {
                        foreach (WorkClasses.Bend bend in container.Bends)
                        {
                            if (!bend.IsFictive())
                            {
                                quaternion curr1 = container.Nodes[bend.StartNodeNumer].Normal;
                                quaternion curr2 = container.Nodes[bend.EndNodeNumer].Normal;

                                curr1 -= container.Nodes[bend.StartNodeNumer].Position;
                                curr2 -= container.Nodes[bend.EndNodeNumer].Position;

                                curr1 /= curr1.abs();
                                if ((object)container.Nodes[bend.StartNodeNumer].ExplicitNormal != null)
                                    curr1 *= container.Nodes[bend.StartNodeNumer].ExplicitNormalLength;
                                else
                                    curr1 *= UtilityClasses.ConstantsAndSettings.NormlLengthToShow;

                                if ((object)container.Nodes[bend.EndNodeNumer].ExplicitNormal != null)
                                    curr2 *= container.Nodes[bend.EndNodeNumer].ExplicitNormalLength;
                                else
                                    curr2 *= UtilityClasses.ConstantsAndSettings.NormlLengthToShow;

                                curr1 += container.Nodes[bend.StartNodeNumer].Position;
                                curr2 += container.Nodes[bend.EndNodeNumer].Position;

                                UtilityClasses.GlobalFunctions.DrawLine((Point3d)curr1, (Point3d)curr2, layout);
                            }
                        }
                    }
                    else
                        MessageBox.Show("\nData Base Empty !\n\nThere is nothing to Record !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }//
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }

        }
    }
}
