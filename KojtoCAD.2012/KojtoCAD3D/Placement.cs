using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
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
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.Placement))]

namespace KojtoCAD.KojtoCAD3D
{
    public class Placement
    {
        public Containers container = ContextVariablesProvider.Container;

        //---  placement of blocks in nodes ---------------------------------
        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/PLACEMENT_OF_NODE_3D_IN_POSITION.htm", "")]
        public void KojtoCAD_3D_Placement_of_Node_3D_in_Position()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    string blockName = "";
                    string layer = "";
                    double L = 0.0; //Distance from Node Position  to the real Point of the Figure (lying on the axis)

                    #region prompt blockname, layer and work
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
                    pStrOpts.AllowSpaces = false;
                    pStrOpts.DefaultValue = ConstantsAndSettings.Node3DBlock;
                    PromptResult pStrRes;
                    pStrRes = ed.GetString(pStrOpts);
                    if (pStrRes.Status == PromptStatus.OK)
                    {
                        blockName = pStrRes.StringResult;

                        PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                        pStrOpts_.AllowSpaces = false;
                        pStrOpts_.DefaultValue = ConstantsAndSettings.Node3DLayer;
                        PromptResult pStrRes_;
                        pStrRes_ = ed.GetString(pStrOpts_);
                        if (pStrRes_.Status == PromptStatus.OK)
                        {
                            layer = pStrRes_.StringResult;

                            #region check
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                #region check block
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                if (!acBlkTbl.Has(blockName))
                                {
                                    MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    blockName = "";
                                    return;
                                }
                                #endregion

                                #region check layer
                                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                                if (!lt.Has(layer))
                                {
                                    MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    layer = "";
                                    return;
                                }
                                #endregion
                            }
                            #endregion

                            #region work
                            if ((container.Nodes.Count > 0) && (blockName != "") && (layer != ""))
                            {
                                using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    SymbolUtilityServices.ValidateSymbolName(blockName, false);
                                    SymbolUtilityServices.ValidateSymbolName(layer, false);

                                    if (acBlkTbl.Has(blockName))
                                    {
                                        foreach (WorkClasses.Node Node in container.Nodes)
                                        {
                                            bool noFictive = false;
                                            foreach (int N in Node.Bends_Numers_Array)
                                            {
                                                WorkClasses.Bend bend = container.Bends[N];
                                                if (!bend.IsFictive())
                                                {
                                                    noFictive = true;
                                                    break;
                                                }
                                            }
                                            if (noFictive)
                                            {
                                                try
                                                {
                                                    BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                                    BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);

                                                    UCS ucs = Node.CreateNodeUCS(L, ref container);
                                                    double[] matrxarr = ucs.GetAutoCAD_Matrix3d();
                                                    Matrix3d trM = new Matrix3d(matrxarr);
                                                    br.Layer = layer;
                                                    br.TransformBy(trM);

                                                    acBlkTblRec.AppendEntity(br);
                                                    tr.AddNewlyCreatedDBObject(br, true);

                                                    Node.SolidHandle = new Pair<int, Handle>(1, br.Handle);
                                                }
                                                catch { }
                                            }

                                            //KojtoCAD_3D_Placement_of_Node_3D_in_Position_By_Numer(Node, blockName, layer, L);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("\nBlock " + blockName + " Missing !", "Block Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }

                                    tr.Commit();
                                    ed.UpdateScreen();
                                }
                            }
                            else
                            {
                                MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_DELETE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_OR_HIDE_3D.htm", "")]
        public void KojtoCAD_3D_Placement_of_Node_3D_in_Position_Delete()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Node Node in container.Nodes)
                        {
                            bool noFictive = false;
                            foreach (int N in Node.Bends_Numers_Array)
                            {
                                WorkClasses.Bend bend = container.Bends[N];
                                if (!bend.IsFictive())
                                {
                                    noFictive = true;
                                    break;
                                }
                            }
                            if (noFictive)
                            {
                                if (Node.SolidHandle.First >= 0)
                                {
                                    try
                                    {
                                        Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(Node.SolidHandle.Second), OpenMode.ForWrite) as Entity;
                                        Node.SolidHandle = new Pair<int, Handle>(-1, new Handle(-1));
                                        ent.Erase();
                                    }
                                    catch
                                    {
                                        Node.SolidHandle = new Pair<int, Handle>(-1, new Handle(-1));
                                    }
                                }
                            }
                        }

                        tr.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
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

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_HIDE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_OR_HIDE_3D.htm", "")]
        public void KojtoCAD_3D_Placement_of_Node_3D_in_Position_Hide()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Node Node in container.Nodes)
                        {
                            if (Node.SolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(Node.SolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    ent.Visible = false;
                                }
                                catch { }
                            }

                        }

                        tr.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
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

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_SHOW", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_OR_HIDE_3D.htm", "")]
        public void KojtoCAD_3D_Placement_of_Node_3D_in_Position_SHOW()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Node Node in container.Nodes)
                        {
                            if (Node.SolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(Node.SolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    ent.Visible = true;
                                }
                                catch { }
                            }

                        }

                        tr.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
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

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_BENDS_NOZZLE_BLOCKS_IN_NODES", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/PLACEMENT_OF_BENDS_NOZZLE_BLOCKS_IN_NODES.htm", "")]
        public void KojtoCAD_3D_Placement_of_Bends_Nozzle_Blocks_in_Nodes()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                string blockName = "";
                string layer = "";
                double mR = -1.0;

                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    #region prompt blockname, layer, R and work
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
                    pStrOpts.AllowSpaces = false;
                    pStrOpts.DefaultValue = ConstantsAndSettings.EndsOfBends3DBlock;
                    PromptResult pStrRes;
                    pStrRes = ed.GetString(pStrOpts);
                    if (pStrRes.Status == PromptStatus.OK)
                    {
                        blockName = pStrRes.StringResult;

                        PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                        pStrOpts_.AllowSpaces = false;
                        pStrOpts_.DefaultValue = ConstantsAndSettings.EndsOfBends3DLayer;
                        PromptResult pStrRes_;
                        pStrRes_ = ed.GetString(pStrOpts_);
                        if (pStrRes_.Status == PromptStatus.OK)
                        {
                            #region check
                            layer = pStrRes_.StringResult;
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                if (!acBlkTbl.Has(blockName))
                                {
                                    MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    blockName = "";
                                    return;
                                }

                                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                                if (!lt.Has(layer))
                                {
                                    MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    layer = "";
                                    return;
                                }
                            }
                            #endregion

                            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                            pDoubleOpts.Message =
                                "\n\nEnter the distance from the node point to the origin point of the block,\nmeasured along a line " +
                                "(the origin Point of the Block must lie on the Line of the Bend):";
                            pDoubleOpts.AllowNegative = false;
                            pDoubleOpts.AllowZero = false;
                            pDoubleOpts.AllowNone = false;
                            pDoubleOpts.DefaultValue = ConstantsAndSettings.DistanceNodeToNozzle;
                            PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                            if (pDoubleRes.Status == PromptStatus.OK)
                            {
                                mR = pDoubleRes.Value;

                                #region work
                                if ((container.Nodes.Count > 0) && (blockName != "") && (layer != "") && (mR > 0.0))
                                {
                                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                                    {
                                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                        SymbolUtilityServices.ValidateSymbolName(blockName, false);
                                        SymbolUtilityServices.ValidateSymbolName(layer, false);

                                        foreach (WorkClasses.Node Node in container.Nodes)
                                        {
                                            foreach (int N in Node.Bends_Numers_Array)
                                            {
                                                WorkClasses.Bend bend = container.Bends[N];
                                                if (!bend.IsFictive())
                                                {
                                                    double bendLen = bend.Length;
                                                    double bendHalfLen = bend.Length / 2.0 - mR;

                                                    quaternion bQ = bend.MidPoint - Node.Position;
                                                    bQ /= bQ.abs(); bQ *= mR;
                                                    UCS UCS = new UCS(Node.Position + bQ, bend.MidPoint, bend.Normal);
                                                    UCS ucs = new UCS(Node.Position + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                                                    if (ucs.FromACS(Node.Position).GetZ() > 0)
                                                        ucs = new UCS(Node.Position + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                                                    Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                                    BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                                    BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);
                                                    br.Layer = layer;
                                                    br.TransformBy(trM);

                                                    acBlkTblRec.AppendEntity(br);
                                                    tr.AddNewlyCreatedDBObject(br, true);


                                                    if ((bend.Start - Node.Position).abs() < (bend.End - Node.Position).abs())
                                                    {
                                                        bend.startSolidHandle = new Pair<int, Handle>(1, br.Handle);
                                                    }
                                                    else
                                                    {
                                                        bend.endSolidHandle = new Pair<int, Handle>(1, br.Handle);
                                                    }
                                                }
                                            }
                                        }
                                        tr.Commit();
                                        ed.UpdateScreen();
                                    }

                                }
                                else
                                {
                                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                #endregion

                            }
                        }
                    }
                    #endregion

                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_BENDS_NOZZLE_3D_IN_POSITION_HIDE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_OR_HIDE_3D.htm", "")]
        public void KojtoCAD_3D_Placement_of_Bends_Nozzle_in_Position_Hide()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Bend bend in container.Bends)
                        {
                            if (bend.startSolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(bend.startSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    ent.Visible = false;
                                }
                                catch { }
                            }

                            if (bend.endSolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(bend.endSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    ent.Visible = false;
                                }
                                catch { }
                            }

                        }

                        tr.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
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

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_BENDS_NOZZLE_3D_IN_POSITION_SHOW", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_OR_HIDE_3D.htm", "")]
        public void KojtoCAD_3D_Placement_of_Bends_Nozzle_in_Position_Show()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Bend bend in container.Bends)
                        {
                            if (bend.startSolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(bend.startSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    ent.Visible = true;
                                }
                                catch { }
                            }

                            if (bend.endSolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(bend.endSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    ent.Visible = true;
                                }
                                catch { }
                            }

                        }

                        tr.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
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

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_BEND_NOZZLE_3D_DELETE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_OR_HIDE_3D.htm", "")]
        public void KojtoCAD_3D_Placement_of_Bend_Nozzle_3D_Delete()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Bend bend in container.Bends)
                        {
                            if (bend.startSolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(bend.startSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    bend.startSolidHandle = new Pair<int, Handle>(-1, new Handle(-1));
                                    ent.Erase();
                                }
                                catch
                                {
                                    bend.startSolidHandle = new Pair<int, Handle>(-1, new Handle(-1));
                                }
                            }

                            if (bend.endSolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(bend.endSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    bend.endSolidHandle = new Pair<int, Handle>(-1, new Handle(-1));
                                    ent.Erase();
                                }
                                catch
                                {
                                    bend.endSolidHandle = new Pair<int, Handle>(-1, new Handle(-1));
                                }
                            }
                        }

                        tr.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
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
        //------
        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_BENDS_3D", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/PLACEMENT_BENDS_3D.htm", "")]
        public void KojtoCAD_3D_Placement_Bends_3D()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("Pereferial");
                pKeyOpts.Keywords.Add("NoPereferial");
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.Keywords.Default = "All";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "NoPereferial": KojtoCAD_3D_Placement_Bends_3D_NoPereferial(); break;
                        case "Pereferial": KojtoCAD_3D_Placement_Bends_3D_Pereferial(); break;
                        case "All": KojtoCAD_3D_Placement_Bends_3D_All(); break;
                    }
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        public void KojtoCAD_3D_Placement_Bends_3D_NoPereferial()
        {
            string blockName = "";
            string layer = "";
            double mR = -1.0;

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            #region prompt blockname, layer, R
            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
            pStrOpts.AllowSpaces = false;
            pStrOpts.DefaultValue = ConstantsAndSettings.Bends3DBlock;
            PromptResult pStrRes;
            pStrRes = ed.GetString(pStrOpts);
            if (pStrRes.Status == PromptStatus.OK)
            {
                blockName = pStrRes.StringResult;

                PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                pStrOpts_.AllowSpaces = false;
                pStrOpts_.DefaultValue = ConstantsAndSettings.Bends3DLayer;
                PromptResult pStrRes_;
                pStrRes_ = ed.GetString(pStrOpts_);
                if (pStrRes_.Status == PromptStatus.OK)
                {
                    layer = pStrRes_.StringResult;
                    #region check
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {

                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        if (!acBlkTbl.Has(blockName))
                        {
                            MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            blockName = "";
                            return;
                        }

                        LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                        if (!lt.Has(layer))
                        {
                            MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            layer = "";
                            return;
                        }
                    }
                    #endregion

                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message =
                        "\n\nEnter the distance from the node point to the origin point of the block,\nmeasured along a line " +
                        "(the origin Point of the Block must lie on the Line of the Bend):";
                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    pDoubleOpts.AllowNegative = false;
                    pDoubleOpts.AllowZero = false;
                    pDoubleOpts.AllowNone = false;
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
                        mR = pDoubleRes.Value;

                        #region work
                        if ((container.Bends.Count > 0) && (blockName != "") && (layer != ""))
                        {
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                SymbolUtilityServices.ValidateSymbolName(blockName, false);
                                SymbolUtilityServices.ValidateSymbolName(layer, false);

                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (!bend.IsFictive() && !bend.IsPeripheral())
                                    {
                                        double bendLen = bend.Length;
                                        double bendHalfLen = bend.Length / 2.0 - mR;

                                        quaternion bQ = bend.MidPoint - bend.Start;
                                        bQ /= bQ.abs(); bQ *= mR;
                                        UCS UCS = new UCS(bend.Start + bQ, bend.MidPoint, bend.Normal);
                                        UCS ucs = new UCS(bend.Start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                                        if (ucs.FromACS(bend.Start).GetZ() > 0)
                                            ucs = new UCS(bend.Start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                                        Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                        BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);

                                        double minX = br.Bounds.Value.MinPoint.Z;
                                        double maxX = br.Bounds.Value.MaxPoint.Z;
                                        double lX = Math.Abs(minX) + Math.Abs(maxX);
                                        double kFaktorX = bendHalfLen * 2.0 / lX;

                                        DBObjectCollection temp = new DBObjectCollection();
                                        br.Explode(temp);
                                        if (temp.Count == 1)
                                        {
                                            string type = temp[0].GetType().ToString();
                                            if (type.ToUpper().IndexOf("REGION") >= 0)
                                            {
                                                Region reg = (Region)temp[0];
                                                acBlkTblRec.AppendEntity(reg);
                                                tr.AddNewlyCreatedDBObject(reg, true);

                                                try
                                                {
                                                    Solid3d acSol3D = new Solid3d();
                                                    acSol3D.SetDatabaseDefaults();
                                                    acSol3D.CreateExtrudedSolid(reg, new Vector3d(0, 0, (mR / Math.Abs(mR)) * bendHalfLen * 2.0), new SweepOptions());
                                                    acSol3D.Layer = layer;
                                                    acSol3D.TransformBy(trM);
                                                    acBlkTblRec.AppendEntity(acSol3D);
                                                    tr.AddNewlyCreatedDBObject(acSol3D, true);
                                                    bend.SolidHandle = new Pair<int, Handle>(1, acSol3D.Handle);
                                                    reg.Erase();
                                                }
                                                catch
                                                {
                                                    reg.Erase();
                                                }
                                            }
                                        }
                                    }
                                }
                                tr.Commit();
                                ed.UpdateScreen();
                            }

                        }
                        else
                        {
                            MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        #endregion
                    }

                }
            }
            #endregion
        }
        public void KojtoCAD_3D_Placement_Bends_3D_Pereferial()
        {
            string blockName = "";
            string layer = "";
            double mR = -1.0;

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            #region prompt blockname, layer, R
            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
            pStrOpts.AllowSpaces = false;
            pStrOpts.DefaultValue = ConstantsAndSettings.Bends3DBlock;
            PromptResult pStrRes;
            pStrRes = ed.GetString(pStrOpts);
            if (pStrRes.Status == PromptStatus.OK)
            {
                blockName = pStrRes.StringResult;

                PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                pStrOpts_.AllowSpaces = false;
                pStrOpts_.DefaultValue = ConstantsAndSettings.Bends3DLayer;
                PromptResult pStrRes_;
                pStrRes_ = ed.GetString(pStrOpts_);
                if (pStrRes_.Status == PromptStatus.OK)
                {
                    layer = pStrRes_.StringResult;
                    #region check
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {

                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        if (!acBlkTbl.Has(blockName))
                        {
                            MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            blockName = "";
                            return;
                        }

                        LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                        if (!lt.Has(layer))
                        {
                            MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            layer = "";
                            return;
                        }
                    }
                    #endregion

                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message =
                        "\n\nEnter the distance from the node point to the origin point of the block,\nmeasured along a line " +
                        "(the origin Point of the Block must lie on the Line of the Bend):";
                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    pDoubleOpts.AllowNegative = false;
                    pDoubleOpts.AllowZero = false;
                    pDoubleOpts.AllowNone = false;
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
                        mR = pDoubleRes.Value;

                        #region work
                        if ((container.Bends.Count > 0) && (blockName != "") && (layer != ""))
                        {
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                SymbolUtilityServices.ValidateSymbolName(blockName, false);
                                SymbolUtilityServices.ValidateSymbolName(layer, false);

                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (!bend.IsFictive() && bend.IsPeripheral())
                                    {
                                        double bendLen = bend.Length;
                                        double bendHalfLen = bend.Length / 2.0 - mR;

                                        quaternion bQ = bend.MidPoint - bend.Start;
                                        bQ /= bQ.abs(); bQ *= mR;
                                        UCS UCS = new UCS(bend.Start + bQ, bend.MidPoint, bend.Normal);
                                        UCS ucs = new UCS(bend.Start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                                        if (ucs.FromACS(bend.Start).GetZ() > 0)
                                            ucs = new UCS(bend.Start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                                        WorkClasses.Triangle TR = container.Triangles[bend.FirstTriangleNumer];//(
                                        quaternion trCentoid = ucs.FromACS(TR.GetCentroid());//(

                                        Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                        BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);
                                        if (trCentoid.GetY() < 0)
                                            br.TransformBy(Matrix3d.Mirroring(new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0))));

                                        double minX = br.Bounds.Value.MinPoint.Z;
                                        double maxX = br.Bounds.Value.MaxPoint.Z;
                                        double lX = Math.Abs(minX) + Math.Abs(maxX);
                                        double kFaktorX = bendHalfLen * 2.0 / lX;

                                        DBObjectCollection temp = new DBObjectCollection();
                                        br.Explode(temp);
                                        if (temp.Count == 1)
                                        {
                                            string type = temp[0].GetType().ToString();
                                            if (type.ToUpper().IndexOf("REGION") >= 0)
                                            {
                                                Region reg = (Region)temp[0];
                                                acBlkTblRec.AppendEntity(reg);
                                                tr.AddNewlyCreatedDBObject(reg, true);

                                                try
                                                {
                                                    Solid3d acSol3D = new Solid3d();
                                                    acSol3D.SetDatabaseDefaults();
                                                    acSol3D.CreateExtrudedSolid(reg, new Vector3d(0, 0, (mR / Math.Abs(mR)) * bendHalfLen * 2.0), new SweepOptions());
                                                    acSol3D.Layer = layer;
                                                    acSol3D.TransformBy(trM);
                                                    acBlkTblRec.AppendEntity(acSol3D);
                                                    tr.AddNewlyCreatedDBObject(acSol3D, true);
                                                    bend.SolidHandle = new Pair<int, Handle>(1, acSol3D.Handle);
                                                    reg.Erase();
                                                }
                                                catch
                                                {
                                                    reg.Erase();
                                                }
                                            }
                                        }
                                    }
                                }
                                tr.Commit();
                                ed.UpdateScreen();
                            }

                        }
                        else
                        {
                            MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        #endregion
                    }

                }
            }
            #endregion
        }
        public void KojtoCAD_3D_Placement_Bends_3D_All()
        {
            string blockName = "";
            string layer = "";
            double mR = -1.0;

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            #region prompt blockname, layer, R
            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
            pStrOpts.AllowSpaces = false;
            pStrOpts.DefaultValue = ConstantsAndSettings.Bends3DBlock;
            PromptResult pStrRes;
            pStrRes = ed.GetString(pStrOpts);
            if (pStrRes.Status == PromptStatus.OK)
            {
                blockName = pStrRes.StringResult;

                PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                pStrOpts_.AllowSpaces = false;
                pStrOpts_.DefaultValue = ConstantsAndSettings.Bends3DLayer;
                PromptResult pStrRes_;
                pStrRes_ = ed.GetString(pStrOpts_);
                if (pStrRes_.Status == PromptStatus.OK)
                {
                    layer = pStrRes_.StringResult;
                    #region check
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {

                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        if (!acBlkTbl.Has(blockName))
                        {
                            MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            blockName = "";
                            return;
                        }

                        LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                        if (!lt.Has(layer))
                        {
                            MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            layer = "";
                            return;
                        }
                    }
                    #endregion

                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message =
                        "\n\nEnter the distance from the node point to the origin point of the block,\nmeasured along a line " +
                        "(the origin Point of the Block must lie on the Line of the Bend):";
                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    pDoubleOpts.AllowNegative = true;
                    pDoubleOpts.AllowZero = true;
                    pDoubleOpts.AllowNone = false;
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
                        mR = pDoubleRes.Value;

                        #region work
                        if ((container.Bends.Count > 0) && (blockName != "") && (layer != ""))
                        {
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                SymbolUtilityServices.ValidateSymbolName(blockName, false);
                                SymbolUtilityServices.ValidateSymbolName(layer, false);

                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (!bend.IsFictive())
                                    {
                                        double bendLen = bend.Length;
                                        double bendHalfLen = bend.Length / 2.0 - mR;

                                        quaternion bQ = bend.MidPoint - bend.Start;
                                        bQ /= bQ.abs(); bQ *= mR;
                                        UCS UCS = new UCS(bend.Start + bQ, bend.MidPoint, bend.Normal);
                                        UCS ucs = new UCS(bend.Start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                                        if (ucs.FromACS(bend.Start).GetZ() > 0)
                                            ucs = new UCS(bend.Start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                                        WorkClasses.Triangle TR = container.Triangles[bend.FirstTriangleNumer];//(
                                        quaternion trCentoid = ucs.FromACS(TR.GetCentroid());//(

                                        Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                        BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);
                                        if (bend.IsPeripheral())
                                            if (trCentoid.GetY() < 0)
                                                br.TransformBy(Matrix3d.Mirroring(new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0))));

                                        double minX = br.Bounds.Value.MinPoint.Z;
                                        double maxX = br.Bounds.Value.MaxPoint.Z;
                                        double lX = Math.Abs(minX) + Math.Abs(maxX);
                                        double kFaktorX = bendHalfLen * 2.0 / lX;

                                        DBObjectCollection temp = new DBObjectCollection();
                                        br.Explode(temp);
                                        if (temp.Count == 1)
                                        {
                                            string type = temp[0].GetType().ToString();
                                            if (type.ToUpper().IndexOf("REGION") >= 0)
                                            {
                                                Region reg = (Region)temp[0];
                                                acBlkTblRec.AppendEntity(reg);
                                                tr.AddNewlyCreatedDBObject(reg, true);

                                                try
                                                {
                                                    Solid3d acSol3D = new Solid3d();
                                                    acSol3D.SetDatabaseDefaults();
                                                    acSol3D.CreateExtrudedSolid(reg, new Vector3d(0, 0, (mR / Math.Abs(mR)) * bendHalfLen * 2.0), new SweepOptions());
                                                    acSol3D.Layer = layer;
                                                    acSol3D.TransformBy(trM);
                                                    acBlkTblRec.AppendEntity(acSol3D);
                                                    tr.AddNewlyCreatedDBObject(acSol3D, true);
                                                    bend.SolidHandle = new Pair<int, Handle>(1, acSol3D.Handle);
                                                    reg.Erase();
                                                }
                                                catch
                                                {
                                                    reg.Erase();
                                                }
                                            }
                                        }
                                    }
                                }
                                tr.Commit();
                                ed.UpdateScreen();
                            }

                        }
                        else
                        {
                            MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        #endregion
                    }

                }
            }
            #endregion
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_BENDS_3D_BY_NORMALS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/PLACEMENT_BENDS_3D_BY_NORMALS.htm", "")]
        public void KojtoCAD_3D_Placement_Bends_3D_By_Normals()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("Pereferial");
                pKeyOpts.Keywords.Add("NoPereferial");
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.Keywords.Default = "All";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "NoPereferial": KojtoCAD_3D_Placement_Bends_3D_NoPereferial_By_Normals(); break;
                        case "Pereferial": KojtoCAD_3D_Placement_Bends_3D_Pereferial_By_Normals(); break;
                        case "All": KojtoCAD_3D_Placement_Bends_3D_All_By_Normals(); break;
                    }
                }
            }
            catch { }
            finally
            {
                ed.CurrentUserCoordinateSystem = old;
            }
        }
        public void KojtoCAD_3D_Placement_Bends_3D_All_By_Normals()
        {
            string blockName = "";
            string layer = "";
            double mR = -1.0;

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            #region prompt blockname, layer, R
            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
            pStrOpts.AllowSpaces = false;
            pStrOpts.DefaultValue = ConstantsAndSettings.Bends3DBlock;
            PromptResult pStrRes;
            pStrRes = ed.GetString(pStrOpts);
            if (pStrRes.Status == PromptStatus.OK)
            {
                blockName = pStrRes.StringResult;

                PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                pStrOpts_.AllowSpaces = false;
                pStrOpts_.DefaultValue = ConstantsAndSettings.Bends3DLayer;
                PromptResult pStrRes_;
                pStrRes_ = ed.GetString(pStrOpts_);
                if (pStrRes_.Status == PromptStatus.OK)
                {
                    layer = pStrRes_.StringResult;
                    #region check
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {

                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        if (!acBlkTbl.Has(blockName))
                        {
                            MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            blockName = "";
                            return;
                        }

                        LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                        if (!lt.Has(layer))
                        {
                            MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            layer = "";
                            return;
                        }
                    }
                    #endregion

                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message =
                        "\n\nEnter the distance from the node point to the origin point of the block,\nmeasured along a line " +
                        "(the origin Point of the Block must lie on the Line of the Bend):";
                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    pDoubleOpts.AllowNegative = true;
                    pDoubleOpts.AllowZero = true;
                    pDoubleOpts.AllowNone = false;
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
                        mR = pDoubleRes.Value;
                        if (Math.Abs(mR) == 0.0) { mR = 0.000001; }

                        #region work
                        if ((container.Bends.Count > 0) && (blockName != "") && (layer != ""))
                        {
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                SymbolUtilityServices.ValidateSymbolName(blockName, false);
                                SymbolUtilityServices.ValidateSymbolName(layer, false);

                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (!bend.IsFictive())
                                    {
                                        quaternion start = container.Nodes[bend.StartNodeNumer].Normal - container.Nodes[bend.StartNodeNumer].Position;
                                        quaternion end = container.Nodes[bend.EndNodeNumer].Normal - container.Nodes[bend.EndNodeNumer].Position;
                                        start /= start.abs();
                                        end /= end.abs();

                                        if ((object)container.Nodes[bend.StartNodeNumer].ExplicitNormal == null)
                                            start *= ConstantsAndSettings.NormlLengthToShow;
                                        else
                                            start *= container.Nodes[bend.StartNodeNumer].ExplicitNormalLength;

                                        if ((object)container.Nodes[bend.EndNodeNumer].ExplicitNormal == null)
                                            end *= ConstantsAndSettings.NormlLengthToShow;
                                        else
                                            end *= container.Nodes[bend.EndNodeNumer].ExplicitNormalLength;

                                        start += container.Nodes[bend.StartNodeNumer].Position;
                                        end += container.Nodes[bend.EndNodeNumer].Position;

                                        quaternion mid = (end + start) / 2.0;

                                        double bendLen = (end - start).abs();// bend.Length;
                                        double bendHalfLen = bendLen / 2.0 - mR;

                                        quaternion bQ = mid - start;//bend.MidPoint - bend.Start;
                                        bQ /= bQ.abs(); bQ *= mR;
                                        UCS UCS = new UCS(start + bQ, mid, mid + (bend.Normal - bend.MidPoint));
                                        UCS ucs = new UCS(start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                                        if (ucs.FromACS(start).GetZ() > 0)
                                            ucs = new UCS(start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                                        WorkClasses.Triangle TR = container.Triangles[bend.FirstTriangleNumer];//(
                                        quaternion trCentoid = ucs.FromACS(TR.GetCentroid());//(

                                        Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                        BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);
                                        if (bend.IsPeripheral())
                                            if (trCentoid.GetY() < 0)
                                                br.TransformBy(Matrix3d.Mirroring(new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0))));

                                        double minX = br.Bounds.Value.MinPoint.Z;
                                        double maxX = br.Bounds.Value.MaxPoint.Z;
                                        double lX = Math.Abs(minX) + Math.Abs(maxX);
                                        double kFaktorX = bendHalfLen * 2.0 / lX;

                                        DBObjectCollection temp = new DBObjectCollection();
                                        br.Explode(temp);
                                        if (temp.Count == 1)
                                        {
                                            string type = temp[0].GetType().ToString();
                                            if (type.ToUpper().IndexOf("REGION") >= 0)
                                            {
                                                Region reg = (Region)temp[0];
                                                acBlkTblRec.AppendEntity(reg);
                                                tr.AddNewlyCreatedDBObject(reg, true);

                                                try
                                                {
                                                    Solid3d acSol3D = new Solid3d();
                                                    acSol3D.SetDatabaseDefaults();
                                                    acSol3D.CreateExtrudedSolid(reg, new Vector3d(0, 0, (mR / Math.Abs(mR)) * bendHalfLen * 2.0), new SweepOptions());
                                                    acSol3D.Layer = layer;
                                                    acSol3D.TransformBy(trM);
                                                    acBlkTblRec.AppendEntity(acSol3D);
                                                    tr.AddNewlyCreatedDBObject(acSol3D, true);
                                                    bend.SolidHandle = new Pair<int, Handle>(1, acSol3D.Handle);
                                                    reg.Erase();
                                                }
                                                catch
                                                {
                                                    reg.Erase();
                                                }
                                            }
                                        }
                                    }
                                }
                                tr.Commit();
                                ed.UpdateScreen();
                            }

                        }
                        else
                        {
                            MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        #endregion
                    }

                }
            }
            #endregion
        }
        public void KojtoCAD_3D_Placement_Bends_3D_Pereferial_By_Normals()
        {
            string blockName = "";
            string layer = "";
            double mR = -1.0;

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            #region prompt blockname, layer, R
            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
            pStrOpts.AllowSpaces = false;
            pStrOpts.DefaultValue = ConstantsAndSettings.Bends3DBlock;
            PromptResult pStrRes;
            pStrRes = ed.GetString(pStrOpts);
            if (pStrRes.Status == PromptStatus.OK)
            {
                blockName = pStrRes.StringResult;

                PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                pStrOpts_.AllowSpaces = false;
                pStrOpts_.DefaultValue = ConstantsAndSettings.Bends3DLayer;
                PromptResult pStrRes_;
                pStrRes_ = ed.GetString(pStrOpts_);
                if (pStrRes_.Status == PromptStatus.OK)
                {
                    layer = pStrRes_.StringResult;
                    #region check
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {

                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        if (!acBlkTbl.Has(blockName))
                        {
                            MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            blockName = "";
                            return;
                        }

                        LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                        if (!lt.Has(layer))
                        {
                            MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            layer = "";
                            return;
                        }
                    }
                    #endregion

                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message =
                        "\n\nEnter the distance from the node point to the origin point of the block,\nmeasured along a line " +
                        "(the origin Point of the Block must lie on the Line of the Bend):";
                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    pDoubleOpts.AllowNegative = true;
                    pDoubleOpts.AllowZero = true;
                    pDoubleOpts.AllowNone = false;
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
                        mR = pDoubleRes.Value;
                        if (Math.Abs(mR) == 0.0) { mR = 0.000001; }

                        #region work
                        if ((container.Bends.Count > 0) && (blockName != "") && (layer != ""))
                        {
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                SymbolUtilityServices.ValidateSymbolName(blockName, false);
                                SymbolUtilityServices.ValidateSymbolName(layer, false);

                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (!bend.IsFictive() && bend.IsPeripheral())
                                    {
                                        quaternion start = container.Nodes[bend.StartNodeNumer].Normal - container.Nodes[bend.StartNodeNumer].Position;
                                        quaternion end = container.Nodes[bend.EndNodeNumer].Normal - container.Nodes[bend.EndNodeNumer].Position;
                                        start /= start.abs();
                                        end /= end.abs();

                                        if ((object)container.Nodes[bend.StartNodeNumer].ExplicitNormal == null)
                                            start *= ConstantsAndSettings.NormlLengthToShow;
                                        else
                                            start *= container.Nodes[bend.StartNodeNumer].ExplicitNormalLength;

                                        if ((object)container.Nodes[bend.EndNodeNumer].ExplicitNormal == null)
                                            end *= ConstantsAndSettings.NormlLengthToShow;
                                        else
                                            end *= container.Nodes[bend.EndNodeNumer].ExplicitNormalLength;

                                        start += container.Nodes[bend.StartNodeNumer].Position;
                                        end += container.Nodes[bend.EndNodeNumer].Position;

                                        quaternion mid = (end + start) / 2.0;

                                        double bendLen = (end - start).abs();// bend.Length;
                                        double bendHalfLen = bendLen / 2.0 - mR;

                                        quaternion bQ = mid - start;//bend.MidPoint - bend.Start;
                                        bQ /= bQ.abs(); bQ *= mR;
                                        UCS UCS = new UCS(start + bQ, mid, mid + (bend.Normal - bend.MidPoint));
                                        UCS ucs = new UCS(start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                                        if (ucs.FromACS(start).GetZ() > 0)
                                            ucs = new UCS(start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                                        WorkClasses.Triangle TR = container.Triangles[bend.FirstTriangleNumer];//(
                                        quaternion trCentoid = ucs.FromACS(TR.GetCentroid());//(

                                        Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                        BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);
                                        if (bend.IsPeripheral())
                                            if (trCentoid.GetY() < 0)
                                                br.TransformBy(Matrix3d.Mirroring(new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0))));

                                        double minX = br.Bounds.Value.MinPoint.Z;
                                        double maxX = br.Bounds.Value.MaxPoint.Z;
                                        double lX = Math.Abs(minX) + Math.Abs(maxX);
                                        double kFaktorX = bendHalfLen * 2.0 / lX;

                                        DBObjectCollection temp = new DBObjectCollection();
                                        br.Explode(temp);
                                        if (temp.Count == 1)
                                        {
                                            string type = temp[0].GetType().ToString();
                                            if (type.ToUpper().IndexOf("REGION") >= 0)
                                            {
                                                Region reg = (Region)temp[0];
                                                acBlkTblRec.AppendEntity(reg);
                                                tr.AddNewlyCreatedDBObject(reg, true);

                                                try
                                                {
                                                    Solid3d acSol3D = new Solid3d();
                                                    acSol3D.SetDatabaseDefaults();
                                                    acSol3D.CreateExtrudedSolid(reg, new Vector3d(0, 0, (mR / Math.Abs(mR)) * bendHalfLen * 2.0), new SweepOptions());
                                                    acSol3D.Layer = layer;
                                                    acSol3D.TransformBy(trM);
                                                    acBlkTblRec.AppendEntity(acSol3D);
                                                    tr.AddNewlyCreatedDBObject(acSol3D, true);
                                                    bend.SolidHandle = new Pair<int, Handle>(1, acSol3D.Handle);
                                                    reg.Erase();
                                                }
                                                catch
                                                {
                                                    reg.Erase();
                                                }
                                            }
                                        }
                                    }
                                }
                                tr.Commit();
                                ed.UpdateScreen();
                            }

                        }
                        else
                        {
                            MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        #endregion
                    }

                }
            }
            #endregion
        }
        public void KojtoCAD_3D_Placement_Bends_3D_NoPereferial_By_Normals()
        {
            string blockName = "";
            string layer = "";
            double mR = -1.0;

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            #region prompt blockname, layer, R
            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
            pStrOpts.AllowSpaces = false;
            pStrOpts.DefaultValue = ConstantsAndSettings.Bends3DBlock;
            PromptResult pStrRes;
            pStrRes = ed.GetString(pStrOpts);
            if (pStrRes.Status == PromptStatus.OK)
            {
                blockName = pStrRes.StringResult;

                PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                pStrOpts_.AllowSpaces = false;
                pStrOpts_.DefaultValue = ConstantsAndSettings.Bends3DLayer;
                PromptResult pStrRes_;
                pStrRes_ = ed.GetString(pStrOpts_);
                if (pStrRes_.Status == PromptStatus.OK)
                {
                    layer = pStrRes_.StringResult;
                    #region check
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {

                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        if (!acBlkTbl.Has(blockName))
                        {
                            MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            blockName = "";
                            return;
                        }

                        LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                        if (!lt.Has(layer))
                        {
                            MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            layer = "";
                            return;
                        }
                    }
                    #endregion

                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message =
                        "\n\nEnter the distance from the node point to the origin point of the block,\nmeasured along a line " +
                        "(the origin Point of the Block must lie on the Line of the Bend):";
                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    pDoubleOpts.AllowNegative = true;
                    pDoubleOpts.AllowZero = true;
                    pDoubleOpts.AllowNone = false;
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
                        mR = pDoubleRes.Value;
                        if (Math.Abs(mR) == 0.0) { mR = 0.000001; }

                        #region work
                        if ((container.Bends.Count > 0) && (blockName != "") && (layer != ""))
                        {
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                SymbolUtilityServices.ValidateSymbolName(blockName, false);
                                SymbolUtilityServices.ValidateSymbolName(layer, false);

                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (!bend.IsFictive() && !bend.IsPeripheral())
                                    {
                                        quaternion start = container.Nodes[bend.StartNodeNumer].Normal - container.Nodes[bend.StartNodeNumer].Position;
                                        quaternion end = container.Nodes[bend.EndNodeNumer].Normal - container.Nodes[bend.EndNodeNumer].Position;
                                        start /= start.abs();
                                        end /= end.abs();

                                        if ((object)container.Nodes[bend.StartNodeNumer].ExplicitNormal == null)
                                            start *= ConstantsAndSettings.NormlLengthToShow;
                                        else
                                            start *= container.Nodes[bend.StartNodeNumer].ExplicitNormalLength;

                                        if ((object)container.Nodes[bend.EndNodeNumer].ExplicitNormal == null)
                                            end *= ConstantsAndSettings.NormlLengthToShow;
                                        else
                                            end *= container.Nodes[bend.EndNodeNumer].ExplicitNormalLength;

                                        start += container.Nodes[bend.StartNodeNumer].Position;
                                        end += container.Nodes[bend.EndNodeNumer].Position;

                                        quaternion mid = (end + start) / 2.0;

                                        double bendLen = (end - start).abs();// bend.Length;
                                        double bendHalfLen = bendLen / 2.0 - mR;

                                        quaternion bQ = mid - start;//bend.MidPoint - bend.Start;
                                        bQ /= bQ.abs(); bQ *= mR;
                                        UCS UCS = new UCS(start + bQ, mid, mid + (bend.Normal - bend.MidPoint));
                                        UCS ucs = new UCS(start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                                        if (ucs.FromACS(start).GetZ() > 0)
                                            ucs = new UCS(start + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                                        WorkClasses.Triangle TR = container.Triangles[bend.FirstTriangleNumer];//(
                                        quaternion trCentoid = ucs.FromACS(TR.GetCentroid());//(

                                        Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                        BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);
                                        if (bend.IsPeripheral())
                                            if (trCentoid.GetY() < 0)
                                                br.TransformBy(Matrix3d.Mirroring(new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0))));

                                        double minX = br.Bounds.Value.MinPoint.Z;
                                        double maxX = br.Bounds.Value.MaxPoint.Z;
                                        double lX = Math.Abs(minX) + Math.Abs(maxX);
                                        double kFaktorX = bendHalfLen * 2.0 / lX;

                                        DBObjectCollection temp = new DBObjectCollection();
                                        br.Explode(temp);
                                        if (temp.Count == 1)
                                        {
                                            string type = temp[0].GetType().ToString();
                                            if (type.ToUpper().IndexOf("REGION") >= 0)
                                            {
                                                Region reg = (Region)temp[0];
                                                acBlkTblRec.AppendEntity(reg);
                                                tr.AddNewlyCreatedDBObject(reg, true);

                                                try
                                                {
                                                    Solid3d acSol3D = new Solid3d();
                                                    acSol3D.SetDatabaseDefaults();
                                                    acSol3D.CreateExtrudedSolid(reg, new Vector3d(0, 0, (mR / Math.Abs(mR)) * bendHalfLen * 2.0), new SweepOptions());
                                                    acSol3D.Layer = layer;
                                                    acSol3D.TransformBy(trM);
                                                    acBlkTblRec.AppendEntity(acSol3D);
                                                    tr.AddNewlyCreatedDBObject(acSol3D, true);
                                                    bend.SolidHandle = new Pair<int, Handle>(1, acSol3D.Handle);
                                                    reg.Erase();
                                                }
                                                catch
                                                {
                                                    reg.Erase();
                                                }
                                            }
                                        }
                                    }
                                }
                                tr.Commit();
                                ed.UpdateScreen();
                            }

                        }
                        else
                        {
                            MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        #endregion
                    }

                }
            }
            #endregion
        }


        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_BEND_3D_DELETE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_OR_HIDE_3D.htm", "")]
        public void KojtoCAD_3D_Placement_Bends_3D_Delete()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if (container.Bends.Count > 0)
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Bend bend in container.Bends)
                        {
                            if (bend.SolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(bend.SolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    bend.SolidHandle = new Pair<int, Handle>(-1, new Handle(-1));
                                    ent.Erase();
                                }
                                catch
                                {
                                    bend.SolidHandle = new Pair<int, Handle>(-1, new Handle(-1));
                                }
                            }
                        }

                        tr.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
                    }
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_BENDS_3D_IN_POSITION_HIDE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_OR_HIDE_3D.htm", "")]
        public void KojtoCAD_3D_Placement_of_Bends_in_Position_Hide()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if (container.Bends.Count > 0)
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Bend bend in container.Bends)
                        {
                            if (bend.SolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(bend.SolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    ent.Visible = false;
                                }
                                catch { }
                            }
                        }

                        tr.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
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

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_OF_BENDS_3D_IN_POSITION_SHOW", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_OR_HIDE_3D.htm", "")]
        public void KojtoCAD_3D_Placement_of_Bends_in_Position_Show()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if (container.Bends.Count > 0)
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (WorkClasses.Bend bend in container.Bends)
                        {
                            if (bend.SolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(bend.SolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    ent.Visible = true;
                                }
                                catch { }
                            }
                        }

                        tr.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
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

        //---------
        [CommandMethod("KojtoCAD_3D", "KCAD_CALC_MIN_CAM_RADIUS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CALC_MIN_CAM_RADIUS.htm", "")]
        public void KojtoCAD_Calc_Miin_Cam_Radius()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if (container.Nodes.Count > 0)
                {
                    double minR = 0;
                    int nodeNumer = -1;

                    System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                    dlg.Filter = "Text Files|*.txt|All Files|*.*";
                    dlg.Title = "Enter File Name ";
                    dlg.DefaultExt = "txt";
                    dlg.FileName = "*.txt";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = dlg.FileName;

                        PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter test Block Name: ");
                        pStrOpts.AllowSpaces = false;
                        PromptResult pStrRes;
                        pStrRes = ed.GetString(pStrOpts);
                        if (pStrRes.Status == PromptStatus.OK)
                        {
                            string blockName = pStrRes.StringResult;

                            //PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                            //pDoubleOpts.Message = "\n\nEnter the start Radius:  ";
                            //PromptDoubleResult pDoubleRes = MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                            // pDoubleOpts.AllowNegative = false;
                            // pDoubleOpts.AllowZero = false;
                            //pDoubleOpts.AllowNone = false;
                            //if (pDoubleRes.Status == PromptStatus.OK)
                            {
                                PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                                pDoubleOpts_.Message = "\n\nEnter minimal Thickness of the Material between the Ends of the Bends:  ";
                                PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                                pDoubleOpts_.AllowNegative = false;
                                pDoubleOpts_.AllowZero = false;
                                pDoubleOpts_.AllowNone = false;
                                if (pDoubleRes_.Status == PromptStatus.OK)
                                {
                                    new Utilities.UtilityClass().MinimizeWindow();
                                    try
                                    {
                                        using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                                        {
                                            BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                            BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                            using (StreamWriter sw = new StreamWriter(fileName))
                                            {
                                                foreach (WorkClasses.Node Node in container.Nodes)
                                                {
                                                    bool isfictve = true;
                                                    #region check for fictive
                                                    foreach (int N in Node.Bends_Numers_Array)
                                                    {
                                                        if (!container.Bends[N].IsFictive())
                                                        {
                                                            isfictve = false;
                                                            break;
                                                        }
                                                    }
                                                    #endregion
                                                    if (!isfictve)
                                                    {
                                                        List<Pair<Entity, quaternion>> coll = NodeFillWithNozzles(tr, ref acBlkTbl, ref acBlkTblRec, Node, 0.0, blockName);

                                                        double dist = 0;
                                                        double mdist = 10.0;
                                                        double minANG = 0.0;
                                                        while (CheckInterference(ref coll, ref minANG))
                                                        {
                                                            MoveNozzleByBendLine(mdist, ref coll);
                                                            dist += mdist;
                                                        }

                                                        dist -= mdist;
                                                        MoveNozzleByBendLine(mdist, ref coll, -1);

                                                        mdist = 1.0;
                                                        while (CheckInterference(ref coll, ref minANG))
                                                        {
                                                            MoveNozzleByBendLine(mdist, ref coll);
                                                            dist += mdist;
                                                        }

                                                        double L = pDoubleRes_.Value / Math.Sqrt(2.0 * (1 - Math.Cos(minANG)));

                                                        dist += L;
                                                        sw.WriteLine(string.Format("Node: {0}  Minimal Radius = {1:f4}     minAngular = {2:f4}", Node.Numer + 1, dist + 1.0 /*pDoubleRes.Value*/ , minANG * 180.0 / Math.PI));
                                                        if ((dist + 0.0/*pDoubleRes.Value*/) > minR)
                                                        {
                                                            minR = dist + 0.0;// pDoubleRes.Value;
                                                            nodeNumer = Node.Numer;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sw.WriteLine(string.Format("Node: {0} - All Bends are Fictive", Node.Numer + 1));
                                                    }
                                                }
                                                sw.WriteLine("-----------------------------------");
                                                sw.WriteLine(string.Format("Node: {0}  Minimal Radius = {1:f4}", nodeNumer + 1, minR));
                                                sw.Flush();
                                                sw.Close();
                                            }//
                                            //tr.Commit();
                                            //ed.UpdateScreen();
                                        }
                                    }
                                    catch
                                    {
                                        new Utilities.UtilityClass().MaximizeWindow();
                                    }
                                }
                            }//
                            new Utilities.UtilityClass().MaximizeWindow();
                            MessageBox.Show(string.Format("Node: {0}  Minimal Radius = {1:f4}", nodeNumer + 1, minR), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        public List<Pair<Entity, quaternion>> NodeFillWithNozzles(Transaction tr, ref BlockTable acBlkTbl, ref BlockTableRecord acBlkTblRec, WorkClasses.Node node, double mR, string blockName)
        {
            List<Pair<Entity, quaternion>> coll = new List<Pair<Entity, quaternion>>();
            foreach (int N in node.Bends_Numers_Array)
            {
                WorkClasses.Bend bend = container.Bends[N];
                if (!bend.IsFictive())
                {
                    double bendLen = bend.Length;
                    double bendHalfLen = bend.Length / 2.0 - mR;

                    quaternion bQ = bend.MidPoint - node.Position;
                    bQ /= bQ.abs(); bQ *= mR;
                    UCS UCS = new UCS(node.Position + bQ, bend.MidPoint, bend.Normal);
                    UCS ucs = new UCS(node.Position + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                    if (ucs.FromACS(node.Position).GetZ() > 0)
                        ucs = new UCS(node.Position + bQ, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                    Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                    BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);

                    DBObjectCollection tColl = new DBObjectCollection();
                    br.Explode(tColl);

                    // ((Solid3d)tColl[0]).OffsetBody(offset);
                    ((Entity)tColl[0]).TransformBy(trM);
                    coll.Add(new Pair<Entity, quaternion>((Entity)tColl[0], (bend.MidPoint - node.Position) / (bend.MidPoint - node.Position).abs()));

                    acBlkTblRec.AppendEntity(((Entity)tColl[0]));
                    tr.AddNewlyCreatedDBObject(((Entity)tColl[0]), true);
                }
            }
            return coll;
        }
        public void MoveNozzleByBendLine(double dist, ref List<Pair<Entity, quaternion>> coll, int k = 1)
        {
            foreach (Pair<Entity, quaternion> pa in coll)
                pa.First.TransformBy(Matrix3d.Displacement(new Vector3d(pa.Second.GetX() * dist * k, pa.Second.GetY() * dist * k, pa.Second.GetZ() * dist * k)));
        }
        public bool CheckInterference(ref List<Pair<Entity, quaternion>> coll, ref double ang)
        {
            bool rez = false;

            for (int i = 0; i < coll.Count - 1; i++)
            {
                for (int j = i + 1; j < coll.Count; j++)
                {
                    bool b = ((Solid3d)coll[i].First).CheckInterference((Solid3d)coll[j].First);
                    if (b)
                    {
                        rez = true;
                        ang = coll[i].Second.angTo(coll[j].Second);
                        break;
                    }
                }
                if (rez)
                    break;
            }

            return rez;
        }

        //----- Fixing elements
        [CommandMethod("KojtoCAD_3D", "KCAD_ADD_FIXING_ELEMENTS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/ADD_FIXING_ELEMENTS.htm", "")]
        public void KojtoCAD_Add_Fixing_Elements()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("Pereferial");
                pKeyOpts.Keywords.Add("NoPereferial");
                pKeyOpts.Keywords.Default = "NoPereferial";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "NoPereferial": KojtoCAD_Add_Fixing_Elements_NoPereferial(); break;
                        case "Pereferial": KojtoCAD_Add_Fixing_Elements_Pereferial(); break;
                    }
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        public void KojtoCAD_Add_Fixing_Elements_NoPereferial()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if ((container != null) && (container.Bends.Count > 0))
            {
                Fixing_Elements_Setings form = new Fixing_Elements_Setings();
                form.ShowDialog();
                if (form.DialogResult == DialogResult.OK)
                {
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
                    pStrOpts.AllowSpaces = false;
                    pStrOpts.DefaultValue = ConstantsAndSettings.NoPereferialFixngBlockName;
                    PromptResult pStrRes;
                    pStrRes = ed.GetString(pStrOpts);
                    if (pStrRes.Status == PromptStatus.OK)
                    {
                        string blockName = pStrRes.StringResult;

                        PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                        pStrOpts_.AllowSpaces = false;
                        pStrOpts_.DefaultValue = ConstantsAndSettings.NoPereferialFixngLayerName;
                        PromptResult pStrRes_;
                        pStrRes_ = ed.GetString(pStrOpts_);
                        if (pStrRes_.Status == PromptStatus.OK)
                        {
                            string layer = pStrRes_.StringResult;

                            #region check
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {

                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                if (!acBlkTbl.Has(blockName))
                                {
                                    MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    blockName = "";
                                    return;
                                }

                                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                                if (!lt.Has(layer))
                                {
                                    MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    layer = "";
                                    return;
                                }
                            }
                            #endregion

                            ConstantsAndSettings.SetFixing_A(form.A);
                            ConstantsAndSettings.SetFixing_B(form.B);

                            List<Pair<Entity, quaternion>> coll = new List<Pair<Entity, quaternion>>();
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (bend.IsFictive() || bend.IsPeripheral()) continue;

                                    UCS UCS = new UCS(bend.Start, bend.MidPoint, bend.Normal);
                                    UCS ucs = new UCS(bend.Start, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                                    if (ucs.FromACS(bend.Start).GetZ() > 0)
                                        ucs = new UCS(bend.Start, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                                    Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                    double bLength = bend.Length;
                                    double mA = form.A;
                                    double mB = form.B;
                                    int dN = (int)((bLength - 2.0 * mA) / mB);
                                    mA = (bLength - dN * mB) / 2.0;

                                    quaternion ort = bend.End - bend.Start;
                                    ort /= ort.abs();

                                    for (int i = 0; i <= dN; i++)
                                    {
                                        double vLen = mB * i + mA;
                                        quaternion Pos = ort * vLen;

                                        BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);

                                        br.TransformBy(trM);
                                        br.TransformBy(Matrix3d.Displacement(new Vector3d(Pos.GetX(), Pos.GetY(), Pos.GetZ())));

                                        DBObjectCollection tColl = new DBObjectCollection();
                                        br.Explode(tColl);
                                        foreach (DBObject obj in tColl)
                                        {
                                            ((Entity)obj).Layer = layer;
                                            acBlkTblRec.AppendEntity((Entity)obj);
                                            tr.AddNewlyCreatedDBObject((Entity)obj, true);
                                        }
                                    }
                                }

                                tr.Commit();
                                ed.UpdateScreen();
                            }
                        }
                    }
                }
            }
            else
                MessageBox.Show("Data Base Missing !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void KojtoCAD_Add_Fixing_Elements_Pereferial()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if ((container != null) && (container.Bends.Count > 0))
            {
                Fixing_Elements_Setings form = new Fixing_Elements_Setings(true);
                form.ShowDialog();
                if (form.DialogResult == DialogResult.OK)
                {
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Block Name: ");
                    pStrOpts.AllowSpaces = false;
                    pStrOpts.DefaultValue = ConstantsAndSettings.PereferialFixngBlockName;
                    PromptResult pStrRes;
                    pStrRes = ed.GetString(pStrOpts);
                    if (pStrRes.Status == PromptStatus.OK)
                    {
                        string blockName = pStrRes.StringResult;

                        PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Solids Layer Name: ");
                        pStrOpts_.AllowSpaces = false;
                        pStrOpts_.DefaultValue = ConstantsAndSettings.PereferialFixngLayerName;
                        PromptResult pStrRes_;
                        pStrRes_ = ed.GetString(pStrOpts_);
                        if (pStrRes_.Status == PromptStatus.OK)
                        {
                            string layer = pStrRes_.StringResult;

                            #region check
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {

                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                if (!acBlkTbl.Has(blockName))
                                {
                                    MessageBox.Show("\nMissing Block " + blockName + " !", "Block E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    blockName = "";
                                    return;
                                }

                                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                                if (!lt.Has(layer))
                                {
                                    MessageBox.Show("\nMissing Layer " + layer + " !", "Layer E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    layer = "";
                                    return;
                                }
                            }
                            #endregion

                            ConstantsAndSettings.SetFixing_pereferial_A(form.A);
                            ConstantsAndSettings.SetFixing_pereferial_B(form.B);

                            List<Pair<Entity, quaternion>> coll = new List<Pair<Entity, quaternion>>();
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (bend.IsFictive() || !bend.IsPeripheral()) continue;

                                    UCS UCS = new UCS(bend.Start, bend.MidPoint, bend.Normal);
                                    UCS ucs = new UCS(bend.Start, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, 100)));
                                    if (ucs.FromACS(bend.Start).GetZ() > 0)
                                        ucs = new UCS(bend.Start, UCS.ToACS(new quaternion(0, 0, 100, 0)), UCS.ToACS(new quaternion(0, 0, 0, -100)));

                                    WorkClasses.Triangle TR = container.Triangles[bend.FirstTriangleNumer];
                                    quaternion trCentoid = ucs.FromACS(TR.GetCentroid());

                                    Matrix3d trM = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                    double bLength = bend.Length;
                                    double mA = form.A;
                                    double mB = form.B;
                                    int dN = (int)((bLength - 2.0 * mA) / mB);
                                    mA = (bLength - dN * mB) / 2.0;

                                    quaternion ort = bend.End - bend.Start;
                                    ort /= ort.abs();

                                    for (int i = 0; i <= dN; i++)
                                    {
                                        double vLen = mB * i + mA;
                                        quaternion Pos = ort * vLen;

                                        BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);
                                        if (trCentoid.GetY() < 0)
                                            br.TransformBy(Matrix3d.Mirroring(new Line3d(new Point3d(0, 0, 0), new Point3d(100, 0, 0))));

                                        br.TransformBy(trM);
                                        br.TransformBy(Matrix3d.Displacement(new Vector3d(Pos.GetX(), Pos.GetY(), Pos.GetZ())));

                                        DBObjectCollection tColl = new DBObjectCollection();
                                        br.Explode(tColl);
                                        foreach (DBObject obj in tColl)
                                        {
                                            ((Entity)obj).Layer = layer;
                                            acBlkTblRec.AppendEntity((Entity)obj);
                                            tr.AddNewlyCreatedDBObject((Entity)obj, true);
                                        }
                                    }
                                }

                                tr.Commit();
                                ed.UpdateScreen();
                            }
                        }
                    }
                }
            }
            else
                MessageBox.Show("Data Base Missing !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_ATTACHING_A_SOLID3D_TO_BEND", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/ATTACHING_A_SOLID3D_TO_BEND.htm", "")]
        public void KojtoCAD_3D_Attaching_Solid3d_to_Bend()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0))
                {

                    ObjectIdCollection coll = new ObjectIdCollection();

                    TypedValue[] acTypValAr = new TypedValue[1];
                    acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "3DSOLID"), 0);

                    do
                    {

                        PromptPointResult pPtRes;
                        PromptPointOptions pPtOpts = new PromptPointOptions("");

                        pPtOpts.Message = "\nEnter the first Mid (or an internal) Point of the Bend: ";
                        pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);

                        if (pPtRes.Status == PromptStatus.OK)
                        {
                            Point3d ptFirst = pPtRes.Value;

                            foreach (WorkClasses.Bend bend in container.Bends)
                            {
                                if (!bend.IsFictive())
                                {
                                    if (bend == ptFirst)
                                    {
                                        PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                                        pKeyOpts.Message = "\nEnter an option ";
                                        pKeyOpts.Keywords.Add("Null");
                                        pKeyOpts.Keywords.Add("Set");
                                        pKeyOpts.Keywords.Default = "Set";
                                        pKeyOpts.AllowNone = false;

                                        PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                                        if (pKeyRes.Status == PromptStatus.OK)
                                        {
                                            switch (pKeyRes.StringResult)
                                            {
                                                case "Null":
                                                    //MessageBox.Show(string.Format("{0}", bend.SolidHandle.First));
                                                    container.Bends[bend.Numer].SolidHandle = new Pair<int, Handle>(-1, new Handle(-1));
                                                    //MessageBox.Show(string.Format("{0}", bend.SolidHandle.First));
                                                    break;
                                                case "Set":
                                                    try
                                                    {
                                                        List<Entity> solids = GlobalFunctions.GetSelection(ref acTypValAr, "Select Solid3d: ");
                                                        if (solids.Count == 1)
                                                        {
                                                            bend.SolidHandle = new Pair<int, Handle>(1, solids[0].Handle);
                                                            coll.Add(solids[0].ObjectId);
                                                        }
                                                        else
                                                        {
                                                            if (solids.Count < 1)
                                                            {
                                                                MessageBox.Show("Solid3d not selected !", "Empty Selection ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                            }

                                                            if (solids.Count > 1)
                                                                MessageBox.Show("You have selected more than one Solids !", "Selection ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        MessageBox.Show("Selection Error", "Error !");
                                                    }
                                                    break;
                                            }
                                        }

                                        break;
                                    }
                                }
                            }
                        }//
                    } while (MessageBox.Show("Repeat Selection ? ", "Selection", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
                    if (coll.Count > 0)
                    {
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            foreach (ObjectId ID in coll)
                            {
                                Entity ent = tr.GetObject(ID, OpenMode.ForWrite) as Entity;
                                ent.Visible = true;
                            }
                            tr.Commit();
                        }
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_CUTTING_BENDS_IN_NODES", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CUTTING_BENDS_IN_NODES.htm", "")]
        public void KCAD_CUTTING_BENDS_IN_NODES()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
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
                            //----------------
                            PromptKeywordOptions pop_ = new PromptKeywordOptions("");
                            pop_.AppendKeywordsToMessage = true;
                            pop_.AllowNone = false;
                            pop_.Keywords.Add("Base");
                            pop_.Keywords.Add("Additional");
                            pop_.Keywords.Default = "Base";
                            PromptResult res_ = ed.GetKeywords(pop_);
                            if (res_.Status == PromptStatus.OK)
                            {
                                switch (res_.StringResult)
                                {
                                    case "Base": KCAD_CUTTING_BENDS_IN_NODES_PRE(); break;
                                    case "Additional": KCAD_CUTTING_BENDS_IN_NODES_PRE(false); break;
                                }
                            }
                            //----------------
                            break;
                        case "Help":
                            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/CUTTING_BENDS_IN_NODES.htm");
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(
                    string.Format("\nError: {0}\nStackTrace: {1}", ex.Message, ex.StackTrace));
            }
            finally { ed.CurrentUserCoordinateSystem = old; }

        }
        public void KCAD_CUTTING_BENDS_IN_NODES_PRE(bool variant = true)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                foreach (WorkClasses.Node node in container.Nodes)
                {
                    foreach (int N in node.Bends_Numers_Array)
                    {
                        WorkClasses.Bend bend0 = container.Bends[N];
                        if (!bend0.IsFictive() && (bend0.SolidHandle.First >= 0))
                        {
                            quaternion MID = bend0.GetMid();
                            using (Transaction tr = db.TransactionManager.StartTransaction())
                            {
                                Solid3d bendSolid = tr.GetObject(GlobalFunctions.GetObjectId(bend0.SolidHandle.Second), OpenMode.ForWrite) as Solid3d;

                                foreach (int NN in node.Bends_Numers_Array)
                                {
                                    if (NN == N) { continue; }
                                    if (container.Bends[NN].IsFictive()) { continue; }
                                    ObjectId ID = ObjectId.Null;

                                    if (node.ExplicitCuttingMethodForEndsOf3D_Bends < 0)
                                    {
                                        if (variant)
                                            ID = GlobalFunctions.GetCutterWall(N, NN, node.Position, node.GetNodesNormalsByNoFictiveBends(ref container), MID, ref container, 100000, 0, 10000);
                                        else
                                            ID = GlobalFunctions.GetCutterWall(N, NN, node.Position, MID, ref container, 100000, 0, 10000);
                                    }
                                    else
                                    {
                                        if (node.ExplicitCuttingMethodForEndsOf3D_Bends == 0)
                                            ID = GlobalFunctions.GetCutterWall(N, NN, node.Position, node.GetNodesNormalsByNoFictiveBends(ref container), MID, ref container, 100000, 0, 10000);
                                        else
                                            if (node.ExplicitCuttingMethodForEndsOf3D_Bends == 1)
                                                ID = GlobalFunctions.GetCutterWall(N, NN, node.Position, MID, ref container, 100000, 0, 10000);
                                    }

                                    if (ID != ObjectId.Null)
                                    {
                                        Solid3d cSolid = tr.GetObject(ID, OpenMode.ForWrite) as Solid3d;
                                        bendSolid.BooleanOperation(BooleanOperationType.BoolSubtract, cSolid);
                                    }
                                }

                                tr.Commit();
                            }
                        }
                    }
                }
            }//
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
