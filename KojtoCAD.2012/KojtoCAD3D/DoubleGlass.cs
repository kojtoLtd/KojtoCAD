using System;
using System.Reflection;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
using KojtoCAD.KojtoCAD3D.WorkClasses;
using KojtoCAD.Utilities;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
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
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.DoubleGlass))]

namespace KojtoCAD.KojtoCAD3D
{
    public class DoubleGlass
    {
        public Containers container = ContextVariablesProvider.Container;

        //double glass
        [CommandMethod("KojtoCAD_3D", "KCAD_SHOW_DOUBLE_GLAS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_DOUBLE_GLASS.htm", "")]
        public void KojtoCAD_3D_Show_DoubleGlass()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    double buff = ConstantsAndSettings.halfGlassFugue;
                    if (ConstantsAndSettings.halfGlassFugue < 0.0001)
                        ConstantsAndSettings.halfGlassFugue = 0.0001;

                    if (ConstantsAndSettings.Single_or_Double_Glass == 0)
                    {
                        try
                        {

                            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                            pDoubleOpts.Message = "\n Enter Glass Height: ";
                            pDoubleOpts.DefaultValue = 13.52;
                            pDoubleOpts.AllowZero = false;
                            pDoubleOpts.AllowNegative = false;
                            pDoubleOpts.DefaultValue = ConstantsAndSettings.Thickness_of_the_Glass;

                            PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                            if (pDoubleRes.Status == PromptStatus.OK)
                            {
                                double HH = pDoubleRes.Value;

                                PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                                pDoubleOpts_.Message = "\n Enter lower Glass Height: ";
                                pDoubleOpts_.DefaultValue = 16.0;
                                pDoubleOpts_.AllowZero = true;
                                pDoubleOpts_.AllowNegative = false;
                                pDoubleOpts_.DefaultValue = ConstantsAndSettings.DoubleGlass_h1;

                                PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                                if (pDoubleRes_.Status == PromptStatus.OK)
                                {
                                    double h1 = pDoubleRes_.Value;

                                    PromptDoubleOptions _pDoubleOpts = new PromptDoubleOptions("");
                                    _pDoubleOpts.Message = "\n Enter upper Glass height: ";
                                    _pDoubleOpts.DefaultValue = 13.52;
                                    _pDoubleOpts.AllowZero = true;
                                    _pDoubleOpts.AllowNegative = false;
                                    _pDoubleOpts.DefaultValue = ConstantsAndSettings.DoubleGlass_h2;

                                    PromptDoubleResult _pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(_pDoubleOpts);
                                    if (_pDoubleRes.Status == PromptStatus.OK)
                                    {
                                        double h2 = _pDoubleRes.Value;

                                        PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                                        pKeyOpts.Message = "\nEnter an option ";
                                        pKeyOpts.Keywords.Add("Both");
                                        pKeyOpts.Keywords.Add("Lower");
                                        pKeyOpts.Keywords.Add("Upper");
                                        pKeyOpts.Keywords.Default = "Both";
                                        pKeyOpts.AllowNone = true;

                                        PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                                        if (pKeyRes.Status == PromptStatus.OK)
                                        {
                                            switch (pKeyRes.StringResult)
                                            {
                                                case "Both":
                                                    foreach (Triangle TR in container.Triangles)
                                                    {
                                                        if ((!container.Bends[TR.GetFirstBendNumer()].IsFictive()) && (!container.Bends[TR.GetSecondBendNumer()].IsFictive()) && (!container.Bends[TR.GetThirdBendNumer()].IsFictive()))
                                                        {
                                                            if (h1 > 0.0)
                                                                Draw_DOUBLE_GLASS_one_of_the_two(TR, 0.0, h1);
                                                            if (h2 > 0.0)
                                                                Draw_DOUBLE_GLASS_one_of_the_two(TR, HH - h2, h2);
                                                        }
                                                    }
                                                    if (h1 > 0.0)
                                                        ShowDoubleGlassPolygonsL(HH, h1);
                                                    if (h2 > 0.0)
                                                        ShowDoubleGlassPolygonsU(HH, h2);
                                                    break;
                                                case "Lower":
                                                    foreach (Triangle TR in container.Triangles)
                                                    {
                                                        if ((!container.Bends[TR.GetFirstBendNumer()].IsFictive()) && (!container.Bends[TR.GetSecondBendNumer()].IsFictive()) && (!container.Bends[TR.GetThirdBendNumer()].IsFictive()))
                                                        {
                                                            if (h1 > 0.0)
                                                                Draw_DOUBLE_GLASS_one_of_the_two(TR, 0.0, h1);
                                                        }
                                                    }
                                                    if (h1 > 0.0)
                                                        ShowDoubleGlassPolygonsL(HH, h1);
                                                    break;
                                                case "Upper":
                                                    if (h2 > 0)
                                                    {
                                                        foreach (Triangle TR in container.Triangles)
                                                        {
                                                            if ((!container.Bends[TR.GetFirstBendNumer()].IsFictive()) && (!container.Bends[TR.GetSecondBendNumer()].IsFictive()) && (!container.Bends[TR.GetThirdBendNumer()].IsFictive()))
                                                            {
                                                                Draw_DOUBLE_GLASS_one_of_the_two(TR, HH - h2, h2);
                                                            }
                                                        }
                                                        ShowDoubleGlassPolygonsU(HH, h2);
                                                    }
                                                    break;
                                            }
                                        }


                                    }
                                }
                            }
                        }
                        catch
                        {
                            string mess = "Try changing 3D machine (Command: KCAD_CHANGE_3D_MASHINE)\nor\nTry reducing Extrude Ratio (Command: KCAD_SET_CUT_SOLID_ER)";
                            mess += "\n\nTo localize the problem first start command (for single glass): KCAD_SHOW_GLASS";
                            MessageBox.Show(mess, "Modeling error !");
                        }
                    }
                    else
                    {
                        MessageBox.Show("\nSettings\\Glass\\Single is ON - E R R O R  !\n\nSelect\nKojtoCAD_3D\\GLASS\\SHOW", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    ConstantsAndSettings.halfGlassFugue = buff;
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        #region auxiliary functions
        public Triplet<quaternion, quaternion, quaternion> Draw_DOUBLE_GLASS_one_of_the_two(Triangle TR, double H, double h)
        {
            Triplet<quaternion, quaternion, quaternion> rez = new Triplet<quaternion, quaternion, quaternion>(null, null, null);

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if ((!container.Bends[TR.GetFirstBendNumer()].IsFictive()) || (!container.Bends[TR.GetSecondBendNumer()].IsFictive()) || (!container.Bends[TR.GetThirdBendNumer()].IsFictive()))
            {
                UCS ucs = TR.GetUcsByCentroid1();
                Matrix3d UCS = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR, H, h);

                Triplet<quaternion, quaternion, quaternion> ptL = new Triplet<quaternion, quaternion, quaternion>(ucs.FromACS(pt.First), ucs.FromACS(pt.Second), ucs.FromACS(pt.Third));

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Polyline acPoly = new Polyline();
                    acPoly.SetDatabaseDefaults();
                    acPoly.AddVertexAt(0, new Point2d(ptL.First.GetX(), ptL.First.GetY()), 0, 0, 0);
                    acPoly.AddVertexAt(1, new Point2d(ptL.Second.GetX(), ptL.Second.GetY()), 0, 0, 0);
                    acPoly.AddVertexAt(2, new Point2d(ptL.Third.GetX(), ptL.Third.GetY()), 0, 0, 0);
                    acPoly.AddVertexAt(3, new Point2d(ptL.First.GetX(), ptL.First.GetY()), 0, 0, 0);

                    acBlkTblRec.AppendEntity(acPoly);
                    tr.AddNewlyCreatedDBObject(acPoly, true);
                    acPoly.Closed = true;

                    rez = new Triplet<quaternion, quaternion, quaternion>(ptL.First, ptL.Second, ptL.Third);

                    try
                    {
                        Solid3d acSol3D = new Solid3d();
                        acSol3D.SetDatabaseDefaults();
                        acSol3D.CreateExtrudedSolid(acPoly, new Vector3d(0, 0, h), new SweepOptions());

                        acBlkTblRec.AppendEntity(acSol3D);
                        tr.AddNewlyCreatedDBObject(acSol3D, true);

                        if (H < h / 2.0)
                            TR.lowSolidHandle = new Pair<int, Handle>(1, acSol3D.Handle);
                        else
                            TR.upSolidHandle = new Pair<int, Handle>(1, acSol3D.Handle);

                        acSol3D.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, H)));
                        acSol3D.TransformBy(UCS);

                        acPoly.Erase();
                    }
                    catch
                    {
                        rez = new Triplet<quaternion, quaternion, quaternion>(null, null, null);
                        acPoly.Erase();
                    }

                    tr.Commit();
                }

            }
            return rez;
        }
        public void ShowDoubleGlassPolygonsL(double H, double h1)
        {

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if ((container != null) && (container.Polygons.Count > 0))
            {
                foreach (Polygon P in container.Polygons)
                {
                    //if (P.IsPlanar(ref container.Triangles))
                    {
                        quaternion normal = container.Triangles[P.Triangles_Numers_Array[0]].Normal.Second -
                                                                  container.Triangles[P.Triangles_Numers_Array[0]].Normal.First;
                        normal /= normal.abs();
                        saveOffsetsToBends(P, 0.0, h1);

                        using (Transaction trans = db.TransactionManager.StartTransaction())
                        {
                            BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                            Solid3d solid = GlobalFunctions.GetPolygonGlassByheight(ref container, P, h1, trans, acBlkTblRec);

                            try
                            {
                                if (!P.IsPlanar(ref container.Triangles))
                                {
                                    acBlkTblRec.AppendEntity(solid);
                                    trans.AddNewlyCreatedDBObject(solid, true);
                                }
                            }
                            catch { }

                            foreach (int bN in P.Triangles_Numers_Array)
                            {
                                if (solid != (object)null)
                                    container.Triangles[bN].lowSolidHandle = new Pair<int, Handle>(1, solid.Handle);
                            }

                            trans.Commit();
                            ed.UpdateScreen();
                        }

                    }
                }
            }
        }
        public void ShowDoubleGlassPolygonsU(double H, double h2)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if ((container != null) && (container.Polygons.Count > 0))
            {
                foreach (Polygon P in container.Polygons)
                {
                    //if (P.IsPlanar(ref container.Triangles))
                    {
                        saveOffsetsToBends(P, H - h2, h2);

                        using (Transaction trans = db.TransactionManager.StartTransaction())
                        {
                            BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                            Solid3d solid1 = GlobalFunctions.GetPolygonGlassByheight(ref container, P, H, trans, acBlkTblRec);
                            Solid3d solid2 = GlobalFunctions.GetPolygonGlassByheight(ref container, P, H - h2, trans, acBlkTblRec);

                            if ((solid1 != (object)null) && (solid2 != (object)null))
                                solid1.BooleanOperation(BooleanOperationType.BoolSubtract, solid2);
                            try
                            {
                                if (!P.IsPlanar(ref container.Triangles))
                                {
                                    acBlkTblRec.AppendEntity(solid1);
                                    trans.AddNewlyCreatedDBObject(solid1, true);
                                }
                            }
                            catch { }

                            foreach (int bN in P.Triangles_Numers_Array)
                            {
                                if (solid1 != (object)null)
                                    container.Triangles[bN].upSolidHandle = new Pair<int, Handle>(1, solid1.Handle);
                            }

                            trans.Commit();
                            ed.UpdateScreen();
                        }
                    }
                }
            }
        }
        public void saveOffsetsToBends(Polygon Po, double H, double h)
        {
            foreach (int trN in Po.Triangles_Numers_Array)
            {
                Triangle TR = container.Triangles[trN];
                if (!container.Bends[TR.GetFirstBendNumer()].IsFictive() ||
                    !container.Bends[TR.GetSecondBendNumer()].IsFictive() ||
                     !container.Bends[TR.GetThirdBendNumer()].IsFictive())
                {
                    saveOffsetsToBends(TR, H, h);
                }
            }
        }
        public void saveOffsetsToBends(Triangle TR, double H, double h)
        //da se plzwa samo za planar regioni
        {
            if (ConstantsAndSettings.solidFuncVariant == 1)
                return;

            if (container.Bends[TR.GetFirstBendNumer()].FirstTriangleNumer == TR.Numer)
                container.Bends[TR.GetFirstBendNumer()].SetFirstTriangleOffset(ConstantsAndSettings.halfGlassFugue);
            else
                container.Bends[TR.GetFirstBendNumer()].SetSecondTriangleOffset(ConstantsAndSettings.halfGlassFugue);

            if (container.Bends[TR.GetSecondBendNumer()].FirstTriangleNumer == TR.Numer)
                container.Bends[TR.GetSecondBendNumer()].SetFirstTriangleOffset(ConstantsAndSettings.halfGlassFugue);
            else
                container.Bends[TR.GetSecondBendNumer()].SetSecondTriangleOffset(ConstantsAndSettings.halfGlassFugue);

            if (container.Bends[TR.GetThirdBendNumer()].FirstTriangleNumer == TR.Numer)
                container.Bends[TR.GetThirdBendNumer()].SetFirstTriangleOffset(ConstantsAndSettings.halfGlassFugue);
            else
                container.Bends[TR.GetThirdBendNumer()].SetSecondTriangleOffset(ConstantsAndSettings.halfGlassFugue);

            Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR, H, h);

            #region bend1
            Bend bend1 = container.Bends[TR.GetFirstBendNumer()];
            if (!bend1.IsFictive())
            {
                UCS ucs1 = TR.GetUcsByBends1(ref container.Bends);

                double offset1 = Math.Abs(ucs1.FromACS(pt.First).GetY());
                double offset2 = Math.Abs(ucs1.FromACS(pt.Second).GetY());
                if (offset2 < offset1) { offset1 = offset2; }

                if (bend1.FirstTriangleNumer == TR.Numer)
                    bend1.SetFirstTriangleOffset(offset1);
                else
                    bend1.SetSecondTriangleOffset(offset1);
            }
            #endregion

            #region bend2
            Bend bend2 = container.Bends[TR.GetSecondBendNumer()];
            if (!bend2.IsFictive())
            {
                UCS ucs2 = TR.GetUcsByBends2(ref container.Bends);
                double offset2 = Math.Abs(ucs2.FromACS(pt.Second).GetY());
                double offset3 = Math.Abs(ucs2.FromACS(pt.Third).GetY());
                if (offset3 < offset2) { offset2 = offset3; }

                if (bend2.FirstTriangleNumer == TR.Numer)
                    bend2.SetFirstTriangleOffset(offset2);
                else
                    bend2.SetSecondTriangleOffset(offset2);
            }
            #endregion

            #region bend3
            Bend bend3 = container.Bends[TR.GetThirdBendNumer()];
            if (!bend3.IsFictive())
            {
                UCS ucs3 = TR.GetUcsByBends3(ref container.Bends);
                double offset3 = Math.Abs(ucs3.FromACS(pt.Third).GetY());
                double offset4 = Math.Abs(ucs3.FromACS(pt.First).GetY());
                if (offset4 < offset3) { offset3 = offset4; }

                if (bend3.FirstTriangleNumer == TR.Numer)
                    bend3.SetFirstTriangleOffset(offset3);
                else
                    bend3.SetSecondTriangleOffset(offset3);
            }
            #endregion
        }
        #endregion

        [CommandMethod("KojtoCAD_3D", "KCAD_HIDE_DOUBLE_GLASS", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_KCAD_HIDE_DOUBLE_GLASS()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    if (ConstantsAndSettings.Single_or_Double_Glass == 0)
                    {

                        PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                        pKeyOpts.Message = "\nEnter an option ";
                        pKeyOpts.Keywords.Add("All");
                        pKeyOpts.Keywords.Add("Triangles");
                        pKeyOpts.Keywords.Add("Polygons");
                        pKeyOpts.Keywords.Default = "All";
                        pKeyOpts.AllowNone = true;

                        PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                        if (pKeyRes.Status == PromptStatus.OK)
                        {
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                switch (pKeyRes.StringResult)
                                {
                                    case "All":
                                        foreach (Triangle TR in container.Triangles)
                                        {
                                            if (!(TR.IsFirstBendFictive() || TR.IsSecondBendFictive() || TR.IsThirdBendFictive()))
                                            {
                                                if (TR.lowSolidHandle.First >= 0)
                                                {
                                                    try
                                                    {
                                                        Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        Entity ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        ent.Visible = false;
                                                        ent1.Visible = false;
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                        foreach (Polygon POL in container.Polygons)
                                        {
                                            Triangle TR = container.Triangles[POL.Triangles_Numers_Array[0]];
                                            if (TR.lowSolidHandle.First >= 0)
                                            {
                                                try
                                                {
                                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    Entity ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    ent.Visible = false;
                                                    ent1.Visible = false;
                                                }
                                                catch { }
                                            }
                                        }
                                        break;
                                    case "Triangles":
                                        foreach (Triangle TR in container.Triangles)
                                        {
                                            //MessageBox.Show(string.Format("{0}\n{1}", TR.upSolidHandle.First, TR.lowSolidHandle.Second));
                                            if (!(TR.IsFirstBendFictive() || TR.IsSecondBendFictive() || TR.IsThirdBendFictive()))
                                            {
                                                if (TR.lowSolidHandle.First >= 0)
                                                {
                                                    try
                                                    {
                                                        Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        Entity ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        ent.Visible = false;
                                                        ent1.Visible = false;
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                        break;
                                    case "Polygons":
                                        foreach (Polygon POL in container.Polygons)
                                        {
                                            Triangle TR = container.Triangles[POL.Triangles_Numers_Array[0]];
                                            if (TR.lowSolidHandle.First >= 0)
                                            {
                                                try
                                                {
                                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    Entity ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    ent.Visible = false;
                                                    ent1.Visible = false;
                                                }
                                                catch { }
                                            }
                                        }
                                        break;
                                }

                                tr.Commit();
                            }
                        }
                    }
                    else
                        MessageBox.Show("\nSettings\\Glass\\Single is off - E R R O R  !\n\nSelect\nKojtoCAD_3D\\DOUBLE GLASS\\SHOW", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("\nSettings\\Glass\\Single is off - E R R O R  !\n\nSelect\nKojtoCAD_3D\\DOUBLE GLASS\\SHOW", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SHOW_HIDEN_DOUBLE_GLASS", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_KCAD_SHOW_HIDEN_DOUBLE_GLASS()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    if (ConstantsAndSettings.Single_or_Double_Glass == 0)
                    {
                        PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                        pKeyOpts.Message = "\nEnter an option ";
                        pKeyOpts.Keywords.Add("All");
                        pKeyOpts.Keywords.Add("Triangles");
                        pKeyOpts.Keywords.Add("Polygons");
                        pKeyOpts.Keywords.Default = "All";
                        pKeyOpts.AllowNone = true;

                        PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                        if (pKeyRes.Status == PromptStatus.OK)
                        {
                            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                            {
                                switch (pKeyRes.StringResult)
                                {
                                    case "All":
                                        foreach (Triangle TR in container.Triangles)
                                        {
                                            if (!(TR.IsFirstBendFictive() || TR.IsSecondBendFictive() || TR.IsThirdBendFictive()))
                                            {
                                                if (TR.lowSolidHandle.First >= 0)
                                                {
                                                    try
                                                    {
                                                        Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        Entity ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        ent.Visible = true;
                                                        ent1.Visible = true;
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                        foreach (Polygon POL in container.Polygons)
                                        {
                                            Triangle TR = container.Triangles[POL.Triangles_Numers_Array[0]];
                                            if (TR.lowSolidHandle.First >= 0)
                                            {
                                                try
                                                {
                                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    Entity ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    ent.Visible = true;
                                                    ent1.Visible = true;
                                                }
                                                catch { }
                                            }
                                        }
                                        break;
                                    case "Triangles":
                                        foreach (Triangle TR in container.Triangles)
                                        {
                                            //MessageBox.Show(string.Format("{0}\n{1}", TR.upSolidHandle.First, TR.lowSolidHandle.Second));
                                            if (!(TR.IsFirstBendFictive() || TR.IsSecondBendFictive() || TR.IsThirdBendFictive()))
                                            {
                                                if (TR.lowSolidHandle.First >= 0)
                                                {
                                                    try
                                                    {
                                                        Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        Entity ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        ent.Visible = true;
                                                        ent1.Visible = true;
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                        break;
                                    case "Polygons":
                                        foreach (Polygon POL in container.Polygons)
                                        {
                                            Triangle TR = container.Triangles[POL.Triangles_Numers_Array[0]];
                                            if (TR.lowSolidHandle.First >= 0)
                                            {
                                                try
                                                {
                                                    Entity ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    Entity ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    ent.Visible = true;
                                                    ent1.Visible = true;
                                                }
                                                catch { }
                                            }
                                        }
                                        break;
                                }

                                tr.Commit();
                            }
                        }
                    }
                    else
                        MessageBox.Show("\nSettings\\Glass\\Single is off - E R R O R  !\n\nSelect\nKojtoCAD_3D\\DOUBLE GLASS\\SHOW", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("\nSettings\\Glass\\Single is off - E R R O R  !\n\nSelect\nKojtoCAD_3D\\DOUBLE GLASS\\SHOW", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }


        [CommandMethod("KojtoCAD_3D", "KCAD_DOUBLE_GlASS_UNFOLDS_TRIANGLE", null, CommandFlags.Session, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SEPARATE_DOUBLE_GlASS_UNFOLDS_TRIANGLE.htm", "")]
        public void KojtoCAD_3D_DoubleGlass_Unfold_By_Level()//samo triygylnici
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    if (ConstantsAndSettings.Single_or_Double_Glass == 0)
                    {
                        string sLocalRoot = Application.GetSystemVariable("LOCALROOTPREFIX") as string;
                        string sTemplatePath = sLocalRoot + "Template\\acad.dwt";

                        System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                        dlg.Filter = "DWT or DWG or DXF|*.dwt;*.dwg;dxf|All Files|*.*";
                        dlg.Multiselect = false;
                        dlg.Title = "Select DWT File ";
                        dlg.DefaultExt = "dwt";
                        dlg.InitialDirectory = sLocalRoot + "Template\\";
                        dlg.FileName = "*.dwt";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            string templateName = dlg.FileName;

                            string dwt = ".dwt";
                            if (templateName.ToUpper().IndexOf(dwt.ToUpper()) >= 0)
                            {
                                FolderBrowserDialog dlgF = new FolderBrowserDialog();
                                dlgF.Description = "Select Destination Folder. \nDirectory that you want to use as the default for Results.";
                                if (dlgF.ShowDialog() == DialogResult.OK)
                                {
                                    string Path = dlgF.SelectedPath;

                                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                                    pDoubleOpts.Message = "\n Enter Glass Height: ";
                                    pDoubleOpts.DefaultValue = 13.52;
                                    pDoubleOpts.AllowZero = false;
                                    pDoubleOpts.AllowNegative = false;
                                    pDoubleOpts.DefaultValue = ConstantsAndSettings.Thickness_of_the_Glass;

                                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                                    if (pDoubleRes.Status == PromptStatus.OK)
                                    {
                                        double HH = pDoubleRes.Value;

                                        PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                                        pDoubleOpts_.Message = "\n Enter lower Glass Height: ";
                                        pDoubleOpts_.DefaultValue = 16.0;
                                        pDoubleOpts_.AllowZero = false;
                                        pDoubleOpts_.AllowNegative = false;
                                        pDoubleOpts_.DefaultValue = ConstantsAndSettings.DoubleGlass_h1;

                                        PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                                        if (pDoubleRes_.Status == PromptStatus.OK)
                                        {
                                            double h1 = pDoubleRes_.Value;

                                            PromptDoubleOptions _pDoubleOpts = new PromptDoubleOptions("");
                                            _pDoubleOpts.Message = "\n Enter upper Glass height: ";
                                            _pDoubleOpts.DefaultValue = 13.52;
                                            _pDoubleOpts.AllowZero = false;
                                            _pDoubleOpts.AllowNegative = false;
                                            _pDoubleOpts.DefaultValue = ConstantsAndSettings.DoubleGlass_h2;

                                            PromptDoubleResult _pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(_pDoubleOpts);
                                            if (_pDoubleRes.Status == PromptStatus.OK)
                                            {
                                                double h2 = _pDoubleRes.Value;

                                                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                                                pKeyOpts.Message = "\nEnter an option ";
                                                pKeyOpts.Keywords.Add("Both");
                                                pKeyOpts.Keywords.Add("Lower");
                                                pKeyOpts.Keywords.Add("Upper");
                                                pKeyOpts.Keywords.Default = "Both";
                                                pKeyOpts.AllowNone = true;

                                                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                                                if (pKeyRes.Status == PromptStatus.OK)
                                                {
                                                    switch (pKeyRes.StringResult)
                                                    {
                                                        case "Both": KojtoCAD_3D_Double_GlASS_UNFOLDS_Triangles(templateName, Path, HH, h1, h2, 0); break;
                                                        case "Lower": KojtoCAD_3D_Double_GlASS_UNFOLDS_Triangles(templateName, Path, HH, h1, h2, 1); break;
                                                        case "Upper": KojtoCAD_3D_Double_GlASS_UNFOLDS_Triangles(templateName, Path, HH, h1, h2, 2); break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                MessageBox.Show("\nTemplate File Name (extension) error !", "Template E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                        MessageBox.Show("\nSettings\\Glass\\Single is ON - E R R O R  !\n\nSelect\nKojtoCAD_3D\\GLASS\\SHOW", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        #region auxiliary functions
        public void KojtoCAD_3D_Double_GlASS_UNFOLDS_Triangles(string templateName, string Path, double H, double h1, double h2, int how)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Prefix File Name: ");
            pStrOpts.AllowSpaces = false;
            PromptResult pStrRes;
            pStrRes = ed.GetString(pStrOpts);
            if (pStrRes.Status == PromptStatus.OK)
            {
                string Prefix = pStrRes.StringResult;

                PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Sufix File Name: ");
                pStrOpts_.AllowSpaces = false;
                PromptResult pStrRes_;
                pStrRes_ = ed.GetString(pStrOpts_);
                if (pStrRes_.Status == PromptStatus.OK)
                {
                    string Sufix = pStrRes_.StringResult;

                    DocumentCollection acDocMgr = Application.DocumentManager;
                    for (int kk = 0; kk < container.Triangles.Count; kk++)
                    {
                        Triangle TR = container.Triangles[kk];
                        bool b1 = container.Bends[TR.GetFirstBendNumer()].IsFictive();
                        bool b2 = container.Bends[TR.GetSecondBendNumer()].IsFictive();
                        bool b3 = container.Bends[TR.GetThirdBendNumer()].IsFictive();
                        if (!b1 && !b2 && !b3)
                        {
                            if ((how == 0) || (how == 1))
                            {
                                #region LOWER GLASS
                                //Document acNewDocLow = acDocMgr.Add(templateName);//**#3
                                var acNewDocLow = new UtilityClass().ReadDocument(templateName);
                                Database acDbNewDocLow = acNewDocLow.Database;
                                acDocMgr.MdiActiveDocument = acNewDocLow;
                                using (DocumentLock acLckDoc = acNewDocLow.LockDocument())
                                {
                                    Triplet<quaternion, quaternion, quaternion> PT = Draw_DOUBLE_GLASS_one_of_the_two(TR, H - h2, h2);
                                    Triplet<quaternion, quaternion, quaternion> pt = Draw_DOUBLE_GLASS_one_of_the_two(TR, 0.0, h1);
                                    using (Transaction trans = acDbNewDocLow.TransactionManager.StartTransaction())
                                    {
                                        BlockTable acBlkTbl = trans.GetObject(acDbNewDocLow.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                        BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                        #region layers
                                        LayerTable lt = (LayerTable)trans.GetObject(acDbNewDocLow.LayerTableId, OpenMode.ForRead);
                                        if (!lt.Has(ConstantsAndSettings.BendsLayer))
                                        {
                                            LayerTableRecord ltr = new LayerTableRecord();
                                            ltr.Name = ConstantsAndSettings.BendsLayer;
                                            ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 0);
                                            lt.UpgradeOpen();
                                            ObjectId ltId = lt.Add(ltr);
                                            trans.AddNewlyCreatedDBObject(ltr, true);
                                            //acDbNewDoc.Clayer = ltId;
                                        }
                                        if (!lt.Has(ConstantsAndSettings.FictivebendsLayer))
                                        {
                                            LayerTableRecord ltr = new LayerTableRecord();
                                            ltr.Name = ConstantsAndSettings.FictivebendsLayer;
                                            ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                                            lt.UpgradeOpen();
                                            ObjectId ltId = lt.Add(ltr);
                                            trans.AddNewlyCreatedDBObject(ltr, true);
                                            //acDbNewDoc.Clayer = ltId;
                                        }
                                        #endregion

                                        if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                                        {
                                            #region polyline
                                            Point3dCollection listPoints = new Point3dCollection();
                                            listPoints.Add((Point3d)pt.First);
                                            listPoints.Add((Point3d)pt.Second);
                                            listPoints.Add((Point3d)pt.Third);

                                            Polyline pl = GlobalFunctions.GetPoly(listPoints);
                                            acBlkTblRec.AppendEntity(pl);
                                            trans.AddNewlyCreatedDBObject(pl, true);

                                            #endregion
                                        }

                                        trans.Commit();
                                    }

                                    Object acadObject = Application.AcadApplication;
                                    acadObject.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject, null);
                                }//lock              
                                GlobalFunctions.DrawLine((Point3d)TR.Nodes.First, (Point3d)TR.Nodes.Second, ConstantsAndSettings.BendsLayer);
                                GlobalFunctions.DrawLine((Point3d)TR.Nodes.First, (Point3d)TR.Nodes.Third, ConstantsAndSettings.BendsLayer);
                                GlobalFunctions.DrawLine((Point3d)TR.Nodes.Third, (Point3d)TR.Nodes.Second, ConstantsAndSettings.BendsLayer);
                                //----------------------------------------------------------------------------
                                if (how == 0)
                                {
                                    var savePath = Path + "\\" + Prefix + "_" + (kk + 1).ToString() + "_" + Sufix +
                                                   "_LOWER";
                                    new UtilityClass().CloseAndSaveDocument(acNewDocLow, savePath);
                                }
                                else
                                {
                                    var savePath = Path + "\\" + Prefix + "_" + (kk + 1).ToString() + "_" +
                                                             Sufix;
                                    new UtilityClass().CloseAndSaveDocument(acNewDocLow, savePath);
                                }

                                #endregion
                            }

                            if ((how == 0) || (how == 2))
                            {
                                #region UPPER GLASS
                                //Document acNewDocUP = acDocMgr.Add(templateName);//**#2
                                var acNewDocUP = new UtilityClass().ReadDocument(templateName);
                                Database acDbNewDocUP = acNewDocUP.Database;
                                acDocMgr.MdiActiveDocument = acNewDocUP;
                                using (DocumentLock acLckDoc = acNewDocUP.LockDocument())
                                {
                                    Triplet<quaternion, quaternion, quaternion> pt = Draw_DOUBLE_GLASS_one_of_the_two(TR, H - h2, h2);
                                    Triplet<quaternion, quaternion, quaternion> PT = Draw_DOUBLE_GLASS_one_of_the_two(TR, 0.0, h1);
                                    using (Transaction trans = acDbNewDocUP.TransactionManager.StartTransaction())
                                    {
                                        BlockTable acBlkTbl = trans.GetObject(acDbNewDocUP.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                        BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                        #region layers
                                        LayerTable lt = (LayerTable)trans.GetObject(acDbNewDocUP.LayerTableId, OpenMode.ForRead);
                                        if (!lt.Has(ConstantsAndSettings.BendsLayer))
                                        {
                                            LayerTableRecord ltr = new LayerTableRecord();
                                            ltr.Name = ConstantsAndSettings.BendsLayer;
                                            ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 0);
                                            lt.UpgradeOpen();
                                            ObjectId ltId = lt.Add(ltr);
                                            trans.AddNewlyCreatedDBObject(ltr, true);
                                            //acDbNewDoc.Clayer = ltId;
                                        }
                                        if (!lt.Has(ConstantsAndSettings.FictivebendsLayer))
                                        {
                                            LayerTableRecord ltr = new LayerTableRecord();
                                            ltr.Name = ConstantsAndSettings.FictivebendsLayer;
                                            ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                                            lt.UpgradeOpen();
                                            ObjectId ltId = lt.Add(ltr);
                                            trans.AddNewlyCreatedDBObject(ltr, true);
                                            //acDbNewDoc.Clayer = ltId;
                                        }
                                        #endregion

                                        if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                                        {
                                            #region polyline
                                            Point3dCollection listPoints = new Point3dCollection();
                                            listPoints.Add((Point3d)pt.First);
                                            listPoints.Add((Point3d)pt.Second);
                                            listPoints.Add((Point3d)pt.Third);

                                            Polyline pl = GlobalFunctions.GetPoly(listPoints);
                                            acBlkTblRec.AppendEntity(pl);
                                            trans.AddNewlyCreatedDBObject(pl, true);

                                            #endregion
                                        }

                                        trans.Commit();
                                    }

                                    Object acadObject = Application.AcadApplication;
                                    acadObject.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject, null);
                                }//lock                           
                                //----------------------------------------------------------------------------
                                GlobalFunctions.DrawLine((Point3d)TR.Nodes.First, (Point3d)TR.Nodes.Second, ConstantsAndSettings.BendsLayer);
                                GlobalFunctions.DrawLine((Point3d)TR.Nodes.First, (Point3d)TR.Nodes.Third, ConstantsAndSettings.BendsLayer);
                                GlobalFunctions.DrawLine((Point3d)TR.Nodes.Third, (Point3d)TR.Nodes.Second, ConstantsAndSettings.BendsLayer);
                                if (how == 0)
                                {
                                    var savePath = Path + "\\" + Prefix + "_" + (kk + 1).ToString() + "_" + Sufix + "_UPPER";
                                    new UtilityClass().CloseAndSaveDocument(acNewDocUP, savePath);
                                }
                                else
                                {
                                    var savePath = Path + "\\" + Prefix + "_" + (kk + 1).ToString() + "_" + Sufix;
                                    new UtilityClass().CloseAndSaveDocument(acNewDocUP, savePath);
                                }

                                #endregion
                            }

                        }
                    }

                }
            }
        }
        #endregion

        [CommandMethod("KojtoCAD_3D", "KCAD_BENDS_TO_SEPARATE_DRAWINGS", null, CommandFlags.Session, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/BENDS_TO_SEPARATE_DRAWINGS.htm", "")]
        public void KojtoCAD_3D_Bends_To_separate_drawings()
        {
            
            if ((container != null) && (container.Bends.Count > 0))
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    string sLocalRoot = Application.GetSystemVariable("LOCALROOTPREFIX") as string;
                    string sTemplatePath = sLocalRoot + "Template\\acad.dwt";

                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.Filter = "DWT or DWG or DXF|*.dwt;*.dwg;dxf|All Files|*.*";
                    dlg.Multiselect = false;
                    dlg.Title = "Select DWT File ";
                    dlg.DefaultExt = "dwt";
                    dlg.InitialDirectory = sLocalRoot + "Template\\";
                    dlg.FileName = "*.dwt";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string templateName = dlg.FileName;

                        string dwt = ".dwt";
                        if (templateName.ToUpper().IndexOf(dwt.ToUpper()) >= 0)
                        {
                            sTemplatePath = templateName;

                            FolderBrowserDialog dlgF = new FolderBrowserDialog();
                            dlgF.Description = "Select Destination Folder. \nDirectory that you want to use as the default for Results.";
                            if (dlgF.ShowDialog() == DialogResult.OK)
                            {
                                string Path = dlgF.SelectedPath;

                                PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Prefix File Name: ");
                                pStrOpts.AllowSpaces = false;
                                PromptResult pStrRes;
                                pStrRes = ed.GetString(pStrOpts);
                                if (pStrRes.Status == PromptStatus.OK)
                                {

                                    string Prefix = pStrRes.StringResult;

                                    PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Sufix File Name: ");
                                    pStrOpts_.AllowSpaces = false;
                                    PromptResult pStrRes_;
                                    pStrRes_ = ed.GetString(pStrOpts_);
                                    if (pStrRes_.Status == PromptStatus.OK)
                                    {
                                        string Sufix = pStrRes_.StringResult;

                                        //Application.MainWindow.WindowState = Window.State.Minimized;
                                        new UtilityClass().MinimizeWindow();
                                        foreach (Bend bend in container.Bends)
                                        {
                                            //foreach (int bendN in node.Bends_Numers_Array)
                                            {
                                                ObjectId ID = ObjectId.Null;
                                                using (DocumentLock acLckDoc = doc.LockDocument())
                                                {
                                                    ID = KojtoCAD_3D_Bends_To_separate_drawings_by_Numer(bend.Numer);
                                                }
                                                if (ID != ObjectId.Null)
                                                {
                                                    ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                                                    acObjIdColl.Add(ID);
                                                    DocumentCollection acDocMgr = Application.DocumentManager;
                                                    //Document acNewDoc = acDocMgr.Add(sTemplatePath);//**#1
                                                    var acNewDoc = new UtilityClass().ReadDocument(sTemplatePath);
                                                    Database acDbNewDoc = acNewDoc.Database;
                                                    using (DocumentLock acLckDoc = acNewDoc.LockDocument())
                                                    {
                                                        // Start a transaction in the new database
                                                        using (Transaction acTrans = acDbNewDoc.TransactionManager.StartTransaction())
                                                        {
                                                            BlockTable acBlkTblNewDoc = acTrans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                                            BlockTableRecord acBlkTblRecNewDoc = acTrans.GetObject(acBlkTblNewDoc[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                            IdMapping acIdMap = new IdMapping();
                                                            db.WblockCloneObjects(acObjIdColl, acBlkTblRecNewDoc.ObjectId, acIdMap, DuplicateRecordCloning.Ignore, false);

                                                            try
                                                            {
                                                                quaternion start = bend.Start;
                                                                quaternion nStart = container.Nodes[bend.StartNodeNumer].Position;
                                                                bool b = (start - nStart).abs() > (bend.End - bend.Start).abs() / 2.0;


                                                                DBText acText = new DBText();
                                                                acText.SetDatabaseDefaults();
                                                                acText.Position = new Point3d(150, 100, 0);
                                                                acText.Height = 30;
                                                                acText.TextString = "node: " + (b ?
                                                                    (container.Bends[bend.Numer].GetStartNodeNumer() + 1).ToString() : (container.Bends[bend.Numer].GetEndNodeNumer() + 1).ToString());

                                                                acBlkTblRecNewDoc.AppendEntity(acText);
                                                                acTrans.AddNewlyCreatedDBObject(acText, true);


                                                                DBText acText_ = new DBText();
                                                                acText_.SetDatabaseDefaults();
                                                                acText_.Position = new Point3d(-250, 100, 0);
                                                                acText_.Height = 30;
                                                                acText_.TextString = "node: " + (!b ?
                                                                    (container.Bends[bend.Numer].GetStartNodeNumer() + 1).ToString() : (container.Bends[bend.Numer].GetEndNodeNumer() + 1).ToString());

                                                                acBlkTblRecNewDoc.AppendEntity(acText_);
                                                                acTrans.AddNewlyCreatedDBObject(acText_, true);

                                                                DBText _acText = new DBText();
                                                                _acText.SetDatabaseDefaults();
                                                                _acText.Position = new Point3d(-100, 250, 0);
                                                                _acText.Height = 30;
                                                                _acText.TextString = "bend: " + (bend.Numer + 1).ToString();

                                                                acBlkTblRecNewDoc.AppendEntity(_acText);
                                                                acTrans.AddNewlyCreatedDBObject(_acText, true);
                                                            }
                                                            catch { }

                                                            acTrans.Commit();
                                                        }//acTrans
                                                        Object acadObject = Application.AcadApplication;
                                                        acadObject.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject, null);
                                                    }//lock
                                                    acDocMgr.MdiActiveDocument = acNewDoc;
                                                    var savePath = Path + "\\" + Prefix + "_Bend_N_" + (bend.Numer + 1).ToString() + "_" + Sufix;
                                                    new UtilityClass().CloseAndSaveDocument(acNewDoc, savePath);
                                                    using (DocumentLock acLckDoc = doc.LockDocument())
                                                    {
                                                        using (Transaction trans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                                                        {
                                                            BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                            try
                                                            {
                                                                Entity ent = trans.GetObject(ID, OpenMode.ForWrite) as Entity;
                                                                ent.Erase();
                                                            }
                                                            catch { }

                                                            trans.Commit();
                                                        }
                                                        Object acadObject_ = Application.AcadApplication;
                                                        acadObject_.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject_, null);
                                                    }
                                                }
                                            }//foreach bends in node
                                        }//foreach nodes
                                    }
                                }
                            }//path
                        }//dwt
                        else
                            MessageBox.Show("\nTemplate File Name (extension) error !", "Template E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    //Application.MainWindow.WindowState = Window.State.Maximized;
                    new UtilityClass().MaximizeWindow();
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #region auxiliary functions
        public ObjectId KojtoCAD_3D_Bends_To_separate_drawings_by_Numer(int bendNumer)
        {
            ObjectId rez = ObjectId.Null;

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            int nodeNumer = container.Bends[bendNumer].StartNodeNumer;
            int nodeNumer1 = container.Bends[bendNumer].EndNodeNumer;

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Pair<int, Handle> pa = container.Bends[bendNumer].SolidHandle;
                if (pa.First >= 0)
                {
                    Solid3d bendSolid = trans.GetObject(GlobalFunctions.GetObjectId(pa.Second), OpenMode.ForRead) as Solid3d;
                    Solid3d bendSolid_clone = bendSolid.Clone() as Solid3d;

                    acBlkTblRec.AppendEntity(bendSolid_clone);
                    trans.AddNewlyCreatedDBObject(bendSolid_clone, true);

                    bendSolid_clone = trans.GetObject(bendSolid_clone.ObjectId, OpenMode.ForWrite) as Solid3d;

                    ObjectId ID = ObjectId.Null;
                    foreach (int N in container.Nodes[nodeNumer].Bends_Numers_Array)
                    {
                        if (N != bendNumer)
                        {
                            pa = container.Bends[N].SolidHandle;
                            if (pa.First >= 0)
                            {
                                Solid3d Solid = trans.GetObject(GlobalFunctions.GetObjectId(pa.Second), OpenMode.ForRead) as Solid3d;
                                Solid3d Solid_clone = Solid.Clone() as Solid3d;

                                acBlkTblRec.AppendEntity(Solid_clone);
                                trans.AddNewlyCreatedDBObject(Solid_clone, true);

                                Solid_clone = trans.GetObject(Solid_clone.ObjectId, OpenMode.ForWrite) as Solid3d;

                                if (ID != ObjectId.Null)
                                {
                                    Solid3d S = trans.GetObject(ID, OpenMode.ForWrite) as Solid3d;
                                    try
                                    {
                                        S.BooleanOperation(BooleanOperationType.BoolUnite, Solid_clone);
                                    }
                                    catch
                                    {
                                        MessageBox.Show("\nBoolUnite Error Node:" + (nodeNumer + 1).ToString() + "  Bend:" + (N + 1).ToString()
                                            , "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                else
                                    ID = Solid_clone.ObjectId;

                            }
                        }
                    }
                    if (ID != ObjectId.Null)
                    {
                        Solid3d SS = trans.GetObject(ID, OpenMode.ForWrite) as Solid3d;
                        try
                        {
                            bendSolid_clone.BooleanOperation(BooleanOperationType.BoolSubtract, SS);
                        }
                        catch
                        {
                            MessageBox.Show("\nBoolSubtract Error Node:" + (nodeNumer + 1).ToString() + "  Bend:" + (bendNumer + 1).ToString()
                                       , "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        ID = ObjectId.Null;
                        foreach (int N in container.Nodes[nodeNumer1].Bends_Numers_Array)
                        {
                            if (N != bendNumer)
                            {
                                pa = container.Bends[N].SolidHandle;
                                if (pa.First >= 0)
                                {
                                    Solid3d Solid = trans.GetObject(GlobalFunctions.GetObjectId(pa.Second), OpenMode.ForRead) as Solid3d;
                                    Solid3d Solid_clone = Solid.Clone() as Solid3d;

                                    acBlkTblRec.AppendEntity(Solid_clone);
                                    trans.AddNewlyCreatedDBObject(Solid_clone, true);

                                    Solid_clone = trans.GetObject(Solid_clone.ObjectId, OpenMode.ForWrite) as Solid3d;

                                    if (ID != ObjectId.Null)
                                    {
                                        Solid3d S = trans.GetObject(ID, OpenMode.ForWrite) as Solid3d;
                                        try
                                        {
                                            S.BooleanOperation(BooleanOperationType.BoolUnite, Solid_clone);
                                        }
                                        catch
                                        {
                                            MessageBox.Show("\nBoolUnite Error Node:" + (nodeNumer + 1).ToString() + "  Bend:" + (N + 1).ToString()
                                                , "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                    else
                                        ID = Solid_clone.ObjectId;

                                }
                            }
                        }

                        if (ID != ObjectId.Null)
                        {
                            Solid3d SSS = trans.GetObject(ID, OpenMode.ForWrite) as Solid3d;
                            try
                            {
                                bendSolid_clone.BooleanOperation(BooleanOperationType.BoolSubtract, SSS);
                            }
                            catch
                            {
                                MessageBox.Show("\nBoolUnite Error Node:" + (nodeNumer + 1).ToString() + "  Bend:" + (bendNumer + 1).ToString()
                                   , "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            UCS ucs = container.Bends[bendNumer].GetUCS();
                            Matrix3d matUCS = new Matrix3d(ucs.GetAutoCAD_Matrix3d());
                            bendSolid_clone.TransformBy(matUCS.Inverse());
                            rez = bendSolid_clone.ObjectId;
                        }
                    }

                }
                trans.Commit();
            }


            return rez;
        }
        #endregion

        [CommandMethod("KojtoCAD_3D", "KCAD_BEND_SECTION_TO_SEPARATE_DRAWINGS", null, CommandFlags.Session, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/BEND_SECTION_TO_SEPARATE_DRAWINGS.htm", "")]
        public void KojtoCAD_3D_BendSectionToSeparateDWG()
        {
            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0))
                {
                    if (ConstantsAndSettings.Single_or_Double_Glass == 0)
                    {
                        string sLocalRoot = Application.GetSystemVariable("LOCALROOTPREFIX") as string;
                        string sTemplatePath = sLocalRoot + "Template\\acad.dwt";

                        System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                        dlg.Filter = "DWT or DWG or DXF|*.dwt;*.dwg;dxf|All Files|*.*";
                        dlg.Multiselect = false;
                        dlg.Title = "Select DWT File ";
                        dlg.DefaultExt = "dwt";
                        dlg.InitialDirectory = sLocalRoot + "Template\\";
                        dlg.FileName = "*.dwt";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            string templateName = dlg.FileName;

                            string dwt = ".dwt";
                            if (templateName.ToUpper().IndexOf(dwt.ToUpper()) >= 0)
                            {
                                sTemplatePath = templateName;

                                FolderBrowserDialog dlgF = new FolderBrowserDialog();
                                dlgF.Description = "Select Destination Folder. \nDirectory that you want to use as the default for Results.";
                                if (dlgF.ShowDialog() == DialogResult.OK)
                                {
                                    string Path = dlgF.SelectedPath;

                                    PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Prefix File Name: ");
                                    pStrOpts.AllowSpaces = false;
                                    PromptResult pStrRes;
                                    pStrRes = ed.GetString(pStrOpts);
                                    if (pStrRes.Status == PromptStatus.OK)
                                    {

                                        string Prefix = pStrRes.StringResult;

                                        PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Sufix File Name: ");
                                        pStrOpts_.AllowSpaces = false;
                                        PromptResult pStrRes_;
                                        pStrRes_ = ed.GetString(pStrOpts_);
                                        if (pStrRes_.Status == PromptStatus.OK)
                                        {
                                            string Sufix = pStrRes_.StringResult;
                                            //----------------------
                                            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                                            pDoubleOpts.Message = "\n Enter Glass Height: ";
                                            pDoubleOpts.DefaultValue = 13.52;
                                            pDoubleOpts.AllowZero = false;
                                            pDoubleOpts.AllowNegative = false;
                                            pDoubleOpts.DefaultValue = ConstantsAndSettings.Thickness_of_the_Glass;

                                            PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                                            if (pDoubleRes.Status == PromptStatus.OK)
                                            {
                                                double HH = pDoubleRes.Value;

                                                PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                                                pDoubleOpts_.Message = "\n Enter lower Glass Height: ";
                                                pDoubleOpts_.DefaultValue = 16.0;
                                                pDoubleOpts_.AllowZero = true;
                                                pDoubleOpts_.AllowNegative = false;
                                                pDoubleOpts_.DefaultValue = ConstantsAndSettings.DoubleGlass_h1;

                                                PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                                                if (pDoubleRes_.Status == PromptStatus.OK)
                                                {
                                                    double h1 = pDoubleRes_.Value;

                                                    PromptDoubleOptions _pDoubleOpts = new PromptDoubleOptions("");
                                                    _pDoubleOpts.Message = "\n Enter upper Glass height: ";
                                                    _pDoubleOpts.DefaultValue = 13.52;
                                                    _pDoubleOpts.AllowZero = true;
                                                    _pDoubleOpts.AllowNegative = false;
                                                    _pDoubleOpts.DefaultValue = ConstantsAndSettings.DoubleGlass_h2;

                                                    PromptDoubleResult _pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(_pDoubleOpts);
                                                    if (_pDoubleRes.Status == PromptStatus.OK)
                                                    {
                                                        double h2 = _pDoubleRes.Value;

                                                        PromptDoubleOptions pDoubleOpt = new PromptDoubleOptions("");
                                                        pDoubleOpt.Message = "\n Enter percent of the length to the cross-section: ";
                                                        pDoubleOpt.DefaultValue = 50.0;
                                                        pDoubleOpt.AllowZero = true;
                                                        pDoubleOpt.AllowNegative = false;
                                                        pDoubleOpt.AllowNone = false;

                                                        PromptDoubleResult pDoubleResult = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpt);
                                                        if (pDoubleResult.Status == PromptStatus.OK)
                                                        {
                                                            double percent = pDoubleResult.Value;

                                                            if (percent >= 0 && percent <= 100.0)
                                                            {
                                                                foreach (Bend bend in container.Bends)
                                                                {
                                                                    if (bend.SolidHandle.First >= 0)
                                                                    {
                                                                        ObjectIdCollection coll = new ObjectIdCollection();
                                                                        coll.Clear();
                                                                        Matrix3d mUCS = new Matrix3d();
                                                                        using (DocumentLock acLckDocCur = doc.LockDocument())
                                                                        {
                                                                            Pair<ObjectId, ObjectId> ID = Draw_sect(bend, HH, h1, h2, percent, ref mUCS);
                                                                            if (ID.First != ObjectId.Null)
                                                                                coll.Add(ID.First);
                                                                            if (ID.Second != ObjectId.Null)
                                                                            {
                                                                                coll.Add(ID.Second);
                                                                            }
                                                                        }//unlock base
                                                                        if (coll.Count > 0)
                                                                        {
                                                                            DocumentCollection acDocMgr = Application.DocumentManager;
                                                                            //Document acNewDoc = acDocMgr.Add(sTemplatePath);//**#
                                                                            var acNewDoc = new UtilityClass().ReadDocument(sTemplatePath);
                                                                            Database acDbNewDoc = acNewDoc.Database;
                                                                            using (DocumentLock acLckDoc = acNewDoc.LockDocument())
                                                                            {
                                                                                using (Transaction acTrans = acDbNewDoc.TransactionManager.StartTransaction())
                                                                                {
                                                                                    BlockTable acBlkTblNewDoc = acTrans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                                                                    BlockTableRecord acBlkTblRecNewDoc = acTrans.GetObject(acBlkTblNewDoc[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                                                    IdMapping acIdMap = new IdMapping();
                                                                                    db.WblockCloneObjects(coll, acBlkTblRecNewDoc.ObjectId, acIdMap, DuplicateRecordCloning.Ignore, false);
                                                                                    acTrans.Commit();
                                                                                }
                                                                            }//unlock base
                                                                            using (DocumentLock acLckDoc = doc.LockDocument())
                                                                            {
                                                                                using (Transaction acTrans = doc.TransactionManager.StartTransaction())
                                                                                {
                                                                                    foreach (ObjectId id in coll)
                                                                                    {
                                                                                        Entity ent = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;
                                                                                        ent.Erase();
                                                                                    }
                                                                                    acTrans.Commit();
                                                                                }
                                                                            }
                                                                            using (DocumentLock acLckDoc = acNewDoc.LockDocument())
                                                                            {
                                                                                acDocMgr.MdiActiveDocument = acNewDoc;
                                                                                Triangle TR = container.Triangles[bend.FirstTriangleNumer];
                                                                                Plane plane = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
                                                                                using (Transaction acTrans = acDbNewDoc.TransactionManager.StartTransaction())
                                                                                {
                                                                                    BlockTable acBlkTblNewDoc = acTrans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                                                                    BlockTableRecord acBlkTblRecNewDoc = acTrans.GetObject(acBlkTblNewDoc[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                                                                                    try
                                                                                    {
                                                                                        if (h1 > 0.0)
                                                                                        {
                                                                                            ObjectId ID_L = Draw_DOUBLE_GLASS_one_of_the_two_A(TR, 0.0, h1);
                                                                                            Solid3d solid1 = acTrans.GetObject(ID_L, OpenMode.ForWrite) as Solid3d;
                                                                                            solid1.TransformBy(mUCS.Inverse());
                                                                                            try
                                                                                            {
                                                                                                Region reg1 = solid1.GetSection(plane);
                                                                                                reg1.ColorIndex = 2;
                                                                                                acBlkTblRecNewDoc.AppendEntity(reg1);
                                                                                                acTrans.AddNewlyCreatedDBObject(reg1, true);
                                                                                                //solid1.Erase();
                                                                                            }
                                                                                            catch { }
                                                                                        }
                                                                                    }
                                                                                    catch { }
                                                                                    try
                                                                                    {
                                                                                        if (h2 > 0.0)
                                                                                        {
                                                                                            ObjectId ID_U = Draw_DOUBLE_GLASS_one_of_the_two_A(TR, HH - h2, h2);
                                                                                            Solid3d solid2 = acTrans.GetObject(ID_U, OpenMode.ForWrite) as Solid3d;
                                                                                            solid2.TransformBy(mUCS.Inverse());
                                                                                            try
                                                                                            {
                                                                                                Region reg2 = solid2.GetSection(plane);
                                                                                                reg2.ColorIndex = 2;
                                                                                                acBlkTblRecNewDoc.AppendEntity(reg2);
                                                                                                acTrans.AddNewlyCreatedDBObject(reg2, true);
                                                                                                //solid2.Erase();
                                                                                            }
                                                                                            catch { }
                                                                                        }
                                                                                    }
                                                                                    catch { }
                                                                                    try
                                                                                    {
                                                                                        if (bend.SecondTriangleNumer >= 0)
                                                                                        {
                                                                                            Triangle TR1 = container.Triangles[bend.SecondTriangleNumer];
                                                                                            if (h1 > 0.0)
                                                                                            {
                                                                                                ObjectId ID_LL = Draw_DOUBLE_GLASS_one_of_the_two_A(TR1, 0.0, h1);
                                                                                                Solid3d solid11 = acTrans.GetObject(ID_LL, OpenMode.ForWrite) as Solid3d;
                                                                                                solid11.TransformBy(mUCS.Inverse());
                                                                                                try
                                                                                                {
                                                                                                    Region reg11 = solid11.GetSection(plane);
                                                                                                    reg11.ColorIndex = 3;
                                                                                                    acBlkTblRecNewDoc.AppendEntity(reg11);
                                                                                                    acTrans.AddNewlyCreatedDBObject(reg11, true);
                                                                                                    //solid11.Erase();
                                                                                                }
                                                                                                catch { }
                                                                                            }

                                                                                            if (h2 > 0.0)
                                                                                            {
                                                                                                ObjectId ID_UU = Draw_DOUBLE_GLASS_one_of_the_two_A(TR1, HH - h2, h2);
                                                                                                Solid3d solid22 = acTrans.GetObject(ID_UU, OpenMode.ForWrite) as Solid3d;
                                                                                                solid22.TransformBy(mUCS.Inverse());
                                                                                                try
                                                                                                {
                                                                                                    Region reg22 = solid22.GetSection(plane);
                                                                                                    reg22.ColorIndex = 3;
                                                                                                    acBlkTblRecNewDoc.AppendEntity(reg22);
                                                                                                    acTrans.AddNewlyCreatedDBObject(reg22, true);
                                                                                                    //solid22.Erase();
                                                                                                }
                                                                                                catch { }
                                                                                            }

                                                                                            DBText acText = new DBText();
                                                                                            acText.SetDatabaseDefaults();
                                                                                            acText.Position = new Point3d(100, 100, 0);
                                                                                            acText.Height = 30;
                                                                                            acText.TextString = "triangle: " + (TR1.Numer + 1).ToString();
                                                                                            acText.ColorIndex = 3;
                                                                                            acBlkTblRecNewDoc.AppendEntity(acText);
                                                                                            acTrans.AddNewlyCreatedDBObject(acText, true);
                                                                                        }
                                                                                    }
                                                                                    catch { }

                                                                                    quaternion bendN = bend.Normal - bend.MidPoint;
                                                                                    bendN /= bendN.abs();
                                                                                    bendN *= ConstantsAndSettings.NormlLengthToShow;
                                                                                    bendN += bend.MidPoint;
                                                                                    Line line = new Line((Point3d)bend.MidPoint, (Point3d)bendN);
                                                                                    line.ColorIndex = 1;
                                                                                    acBlkTblRecNewDoc.AppendEntity(line);
                                                                                    acTrans.AddNewlyCreatedDBObject(line, true);
                                                                                    line.TransformBy(mUCS.Inverse());

                                                                                    DBText acText_ = new DBText();
                                                                                    acText_.SetDatabaseDefaults();
                                                                                    acText_.Position = new Point3d(100, 200, 0);
                                                                                    acText_.Height = 30;
                                                                                    acText_.TextString = "triangle: " + (TR.Numer + 1).ToString();
                                                                                    acText_.ColorIndex = 2;
                                                                                    acBlkTblRecNewDoc.AppendEntity(acText_);
                                                                                    acTrans.AddNewlyCreatedDBObject(acText_, true);

                                                                                    Pair<quaternion, quaternion> secondbend = new Pair<quaternion, quaternion>(container.Nodes[bend.StartNodeNumer].Normal, container.Nodes[bend.EndNodeNumer].Normal);
                                                                                    if ((object)container.Nodes[bend.StartNodeNumer].ExplicitNormal == null)
                                                                                    {
                                                                                        secondbend.First -= container.Nodes[bend.StartNodeNumer].Position;
                                                                                        secondbend.First /= secondbend.First.abs();
                                                                                        secondbend.First *= ConstantsAndSettings.NormlLengthToShow;
                                                                                        secondbend.First += container.Nodes[bend.StartNodeNumer].Position;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        secondbend.First -= container.Nodes[bend.StartNodeNumer].Position;
                                                                                        secondbend.First /= secondbend.First.abs();
                                                                                        secondbend.First *= container.Nodes[bend.StartNodeNumer].ExplicitNormalLength;
                                                                                        secondbend.First += container.Nodes[bend.StartNodeNumer].Position;
                                                                                    }
                                                                                    if ((object)container.Nodes[bend.EndNodeNumer].ExplicitNormal == null)
                                                                                    {
                                                                                        secondbend.Second -= container.Nodes[bend.EndNodeNumer].Position;
                                                                                        secondbend.Second /= secondbend.Second.abs();
                                                                                        secondbend.Second *= ConstantsAndSettings.NormlLengthToShow;
                                                                                        secondbend.Second += container.Nodes[bend.EndNodeNumer].Position;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        secondbend.Second -= container.Nodes[bend.EndNodeNumer].Position;
                                                                                        secondbend.Second /= secondbend.Second.abs();
                                                                                        secondbend.Second *= container.Nodes[bend.EndNodeNumer].ExplicitNormalLength;
                                                                                        secondbend.Second += container.Nodes[bend.EndNodeNumer].Position;
                                                                                    }

                                                                                    Line lN = new Line((Point3d)secondbend.First, (Point3d)secondbend.Second);
                                                                                    lN.ColorIndex = 4;
                                                                                    lN.TransformBy(mUCS.Inverse());
                                                                                    acBlkTblRecNewDoc.AppendEntity(lN);
                                                                                    acTrans.AddNewlyCreatedDBObject(lN, true);

                                                                                    Line lB = new Line((Point3d)bend.Start, (Point3d)bend.End);
                                                                                    lB.ColorIndex = 3;
                                                                                    lB.TransformBy(mUCS.Inverse());
                                                                                    acBlkTblRecNewDoc.AppendEntity(lB);
                                                                                    acTrans.AddNewlyCreatedDBObject(lB, true);


                                                                                    acTrans.Commit();
                                                                                }
                                                                                Object acadObject = Application.AcadApplication;
                                                                                acadObject.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject, null);
                                                                            }
                                                                            var savePath = Path + "\\" + Prefix +"_Bend_N_" +(bend.Numer + 1).ToString() +"_" + Sufix;
                                                                            new UtilityClass().CloseAndSaveDocument(acNewDoc, savePath);
                                                                            doc = Application.DocumentManager.MdiActiveDocument;

                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                                MessageBox.Show("\nPercent of the Length >= 0.0 !\n\nPercent of the Length <= 100.0 !", "Percent of the Length  Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        }//percents
                                                    }//Enter upper Glass height:
                                                }//Enter lower Glass Height: 
                                            }// Enter Glass Height: 
                                            //----------------------
                                        }//sufix
                                    }//prefix
                                }//directory
                            }//template
                            else
                                MessageBox.Show("\nTemplate File Name (extension) error !", "Template E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                        MessageBox.Show("\nSettings\\Glass\\Single is ON - E R R O R  !\n\nSelect\nKojtoCAD_3D\\GLASS\\SHOW", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        #region auxiliary functions
        public Pair<ObjectId, ObjectId> Draw_sect(Bend bend, double H, double h1, double h2, double percent, ref Matrix3d mUCS)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ObjectId ID = ObjectId.Null;
            ObjectId id = ObjectId.Null;

            if (bend.SolidHandle.First < 0)
                return new Pair<ObjectId, ObjectId>(ID, id);

            Pair<quaternion, quaternion> secondbend = new Pair<quaternion, quaternion>(container.Nodes[bend.StartNodeNumer].Normal, container.Nodes[bend.EndNodeNumer].Normal);
            if ((object)container.Nodes[bend.StartNodeNumer].ExplicitNormal == null)
            {
                secondbend.First -= container.Nodes[bend.StartNodeNumer].Position;
                secondbend.First /= secondbend.First.abs();
                secondbend.First *= ConstantsAndSettings.NormlLengthToShow;
                secondbend.First += container.Nodes[bend.StartNodeNumer].Position;
            }
            else
            {
                secondbend.First -= container.Nodes[bend.StartNodeNumer].Position;
                secondbend.First /= secondbend.First.abs();
                secondbend.First *= container.Nodes[bend.StartNodeNumer].ExplicitNormalLength;
                secondbend.First += container.Nodes[bend.StartNodeNumer].Position;
            }
            if ((object)container.Nodes[bend.EndNodeNumer].ExplicitNormal == null)
            {
                secondbend.Second -= container.Nodes[bend.EndNodeNumer].Position;
                secondbend.Second /= secondbend.Second.abs();
                secondbend.Second *= ConstantsAndSettings.NormlLengthToShow;
                secondbend.Second += container.Nodes[bend.EndNodeNumer].Position;
            }
            else
            {
                secondbend.Second -= container.Nodes[bend.EndNodeNumer].Position;
                secondbend.Second /= secondbend.Second.abs();
                secondbend.Second *= container.Nodes[bend.EndNodeNumer].ExplicitNormalLength;
                secondbend.Second += container.Nodes[bend.EndNodeNumer].Position;
            }

            double secondbendLength_s = (secondbend.Second - secondbend.First).abs() / 100.0;//dylvina za 1 procent
            double z = (percent - 50.0) * secondbendLength_s;
            Triangle tr = container.Triangles[bend.FirstTriangleNumer];
            quaternion centroid = tr.GetCentroid();

            UCS preUCS = bend.GetUCS();
            quaternion o = preUCS.ToACS(new quaternion());
            quaternion x = preUCS.ToACS(new quaternion(0, 0, 100, 0));
            quaternion y = preUCS.ToACS(new quaternion(0, 0, 0, 100));
            UCS UCS = new UCS(o, x, y);
            if (UCS.FromACS(centroid).GetZ() < 0)
                UCS = new UCS(o, x, preUCS.ToACS(new quaternion(0, 0, 0, -100)));

            mUCS = new Matrix3d(UCS.GetAutoCAD_Matrix3d());

            plane oXY = new plane(o, x, y);
            quaternion intersectPoint = oXY.IntersectWithVector(secondbend.Second, secondbend.First);

            Pair<int, Handle> pa = container.Bends[bend.Numer].SolidHandle;
            if (pa.First >= 0)
            {
                using (Transaction trans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Solid3d bendSolid = trans.GetObject(GlobalFunctions.GetObjectId(pa.Second), OpenMode.ForRead) as Solid3d;
                    Solid3d bendSolid_clone = bendSolid.Clone() as Solid3d;
                    acBlkTblRec.AppendEntity(bendSolid_clone);
                    trans.AddNewlyCreatedDBObject(bendSolid_clone, true);
                    bendSolid_clone.TransformBy(mUCS.Inverse());
                    Plane plane = new Plane(new Point3d(0, 0, z), new Vector3d(0, 0, 1));
                    try
                    {
                        bendSolid_clone = trans.GetObject(bendSolid_clone.ObjectId, OpenMode.ForWrite) as Solid3d;
                        try
                        {
                            Region reg = bendSolid_clone.GetSection(plane);
                            acBlkTblRec.AppendEntity(reg);
                            trans.AddNewlyCreatedDBObject(reg, true);
                            ID = reg.ObjectId;

                        }
                        catch { };
                        id = bendSolid_clone.ObjectId;
                        //bendSolid_clone.Erase();
                    }
                    catch { bendSolid_clone.Erase(); }

                    trans.Commit();
                }
            }
            return new Pair<ObjectId, ObjectId>(ID, id);
        }
        public ObjectId Draw_DOUBLE_GLASS_one_of_the_two_A(Triangle TR, double H, double h)
        {
            ObjectId rez = ObjectId.Null;

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if ((!container.Bends[TR.GetFirstBendNumer()].IsFictive()) || (!container.Bends[TR.GetSecondBendNumer()].IsFictive()) || (!container.Bends[TR.GetThirdBendNumer()].IsFictive()))
            {
                UCS ucs = TR.GetUcsByCentroid1();
                Matrix3d UCS = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR, H, h);

                Triplet<quaternion, quaternion, quaternion> ptL = new Triplet<quaternion, quaternion, quaternion>(ucs.FromACS(pt.First), ucs.FromACS(pt.Second), ucs.FromACS(pt.Third));

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Polyline acPoly = new Polyline();
                    acPoly.SetDatabaseDefaults();
                    acPoly.AddVertexAt(0, new Point2d(ptL.First.GetX(), ptL.First.GetY()), 0, 0, 0);
                    acPoly.AddVertexAt(1, new Point2d(ptL.Second.GetX(), ptL.Second.GetY()), 0, 0, 0);
                    acPoly.AddVertexAt(2, new Point2d(ptL.Third.GetX(), ptL.Third.GetY()), 0, 0, 0);
                    acPoly.AddVertexAt(3, new Point2d(ptL.First.GetX(), ptL.First.GetY()), 0, 0, 0);

                    acBlkTblRec.AppendEntity(acPoly);
                    tr.AddNewlyCreatedDBObject(acPoly, true);
                    acPoly.Closed = true;

                    try
                    {
                        Solid3d acSol3D = new Solid3d();
                        acSol3D.SetDatabaseDefaults();
                        acSol3D.CreateExtrudedSolid(acPoly, new Vector3d(0, 0, h), new SweepOptions());

                        acBlkTblRec.AppendEntity(acSol3D);
                        tr.AddNewlyCreatedDBObject(acSol3D, true);

                        acSol3D.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, H)));
                        acSol3D.TransformBy(UCS);
                        rez = acSol3D.ObjectId;

                        acPoly.Erase();
                    }
                    catch
                    {
                        rez = ObjectId.Null;
                        acPoly.Erase();
                    }

                    tr.Commit();
                }

            }
            return rez;
        }
        #endregion
    }
}
