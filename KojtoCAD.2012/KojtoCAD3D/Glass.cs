using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
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
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.Glass))]

namespace KojtoCAD.KojtoCAD3D
{
    public class Glass
    {
        private Containers container = ContextVariablesProvider.Container;

        [CommandMethod("KojtoCAD_3D", "KCAD_GLASS_EDGES_BY_FOLD_BASE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/GLASS_EDGES_BY_FOLD_BASE.htm", "")]
        public void KojtoCAD_3D_Glass_EdgesA()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
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
                        switch (pKeyRes.StringResult)
                        {
                            case "All": KojtoCAD_3D_Glass_Edges(0, -1, true); break;
                            case "Triangles":

                                #region triangles
                                PromptKeywordOptions pKeyOpts_ = new PromptKeywordOptions("");
                                pKeyOpts_.Message = "\nEnter an option ";
                                pKeyOpts_.Keywords.Add("All");
                                pKeyOpts_.Keywords.Add("ByNumer");
                                pKeyOpts_.Keywords.Default = "All";
                                pKeyOpts_.AllowNone = true;
                                PromptResult pKeyRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts_);
                                if (pKeyRes_.Status == PromptStatus.OK)
                                {
                                    switch (pKeyRes_.StringResult)
                                    {
                                        case "All": KojtoCAD_3D_Glass_Edges(1, -1, true); break;
                                        case "ByNumer":
                                            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                                            pIntOpts.Message = "\nEnter the triangle  Numer ";

                                            pIntOpts.AllowZero = false;
                                            pIntOpts.AllowNegative = false;

                                            PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                                            if (pIntRes.Status == PromptStatus.OK)
                                            {
                                                int N = pIntRes.Value - 1;
                                                if (N < container.Triangles.Count)
                                                {
                                                    if (container.Bends[container.Triangles[N].GetFirstBendNumer()].IsFictive() ||
                                                       container.Bends[container.Triangles[N].GetSecondBendNumer()].IsFictive() ||
                                                        container.Bends[container.Triangles[N].GetThirdBendNumer()].IsFictive())
                                                    {
                                                        MessageBox.Show("Triangle is Fictive !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                    }
                                                    else
                                                    {
                                                        KojtoCAD_3D_Glass_Edges(1, N, true);
                                                    }
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Triangle Numer - Out of Range !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }

                                            break;
                                    }
                                }
                                #endregion

                                break;
                            case "Polygons":

                                PromptKeywordOptions pKeyOpts__ = new PromptKeywordOptions("");
                                pKeyOpts__.Message = "\nEnter an option ";
                                pKeyOpts__.Keywords.Add("All");
                                pKeyOpts__.Keywords.Add("ByNumer");
                                pKeyOpts__.Keywords.Default = "All";
                                pKeyOpts__.AllowNone = true;
                                PromptResult pKeyRes__ = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts__);
                                if (pKeyRes__.Status == PromptStatus.OK)
                                {
                                    switch (pKeyRes__.StringResult)
                                    {
                                        case "All": KojtoCAD_3D_Glass_Edges(2, -1, true); break;
                                        case "ByNumer":
                                            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                                            pIntOpts.Message = "\nEnter the triangle  Numer ";

                                            pIntOpts.AllowZero = false;
                                            pIntOpts.AllowNegative = false;

                                            PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                                            if (pIntRes.Status == PromptStatus.OK)
                                            {
                                                int N = pIntRes.Value - 1;
                                                if (N < container.Polygons.Count)
                                                {
                                                    KojtoCAD_3D_Glass_Edges(2, N, true);
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Polygon Numer - Out of Range !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_GLASS_EDGES_BY_UNFOLD_BASE", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Glass_EdgesB()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    //KojtoCAD_3D_Glass_Edges(0,-1,false);
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
                        switch (pKeyRes.StringResult)
                        {
                            case "All": KojtoCAD_3D_Glass_Edges(0, -1, false); break;
                            case "Triangles":

                                #region triangles
                                PromptKeywordOptions pKeyOpts_ = new PromptKeywordOptions("");
                                pKeyOpts_.Message = "\nEnter an option ";
                                pKeyOpts_.Keywords.Add("All");
                                pKeyOpts_.Keywords.Add("ByNumer");
                                pKeyOpts_.Keywords.Default = "All";
                                pKeyOpts_.AllowNone = true;
                                PromptResult pKeyRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts_);
                                if (pKeyRes_.Status == PromptStatus.OK)
                                {
                                    switch (pKeyRes_.StringResult)
                                    {
                                        case "All": KojtoCAD_3D_Glass_Edges(1, -1, false); break;
                                        case "ByNumer":
                                            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                                            pIntOpts.Message = "\nEnter the triangle  Numer ";

                                            pIntOpts.AllowZero = false;
                                            pIntOpts.AllowNegative = false;

                                            PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                                            if (pIntRes.Status == PromptStatus.OK)
                                            {
                                                int N = pIntRes.Value - 1;
                                                if (N < container.Triangles.Count)
                                                {
                                                    if (container.Bends[container.Triangles[N].GetFirstBendNumer()].IsFictive() ||
                                                       container.Bends[container.Triangles[N].GetSecondBendNumer()].IsFictive() ||
                                                        container.Bends[container.Triangles[N].GetThirdBendNumer()].IsFictive())
                                                    {
                                                        MessageBox.Show("Triangle is Fictive !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                    }
                                                    else
                                                    {
                                                        KojtoCAD_3D_Glass_Edges(1, N, false);
                                                    }
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Triangle Numer - Out of Range !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }

                                            break;
                                    }
                                }
                                #endregion

                                break;
                            case "Polygons":
                                #region Polygon
                                PromptKeywordOptions pKeyOpts__ = new PromptKeywordOptions("");
                                pKeyOpts__.Message = "\nEnter an option ";
                                pKeyOpts__.Keywords.Add("All");
                                pKeyOpts__.Keywords.Add("ByNumer");
                                pKeyOpts__.Keywords.Default = "All";
                                pKeyOpts__.AllowNone = true;
                                PromptResult pKeyRes__ = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts__);
                                if (pKeyRes__.Status == PromptStatus.OK)
                                {
                                    switch (pKeyRes__.StringResult)
                                    {
                                        case "All": KojtoCAD_3D_Glass_Edges(2, -1, false); break;
                                        case "ByNumer":
                                            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                                            pIntOpts.Message = "\nEnter the triangle  Numer ";

                                            pIntOpts.AllowZero = false;
                                            pIntOpts.AllowNegative = false;

                                            PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                                            if (pIntRes.Status == PromptStatus.OK)
                                            {
                                                int N = pIntRes.Value - 1;
                                                if (N < container.Polygons.Count)
                                                {
                                                    KojtoCAD_3D_Glass_Edges(2, N, false);
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Polygon Numer - Out of Range !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                            break;
                                    }
                                }
                                #endregion
                                break;
                        }
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        public void KojtoCAD_3D_Glass_Edges(int obhvat, int ByNumer, bool variant = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if ((obhvat == 0) || (obhvat == 1))
            {
                #region triangles extrude

                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                    {
                        if (ByNumer >= 0)
                            if (TR.Numer != ByNumer)
                                continue;

                        Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR);
                        if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                        {
                            Triplet<quaternion, quaternion, quaternion> ptt = container.GetInnererTriangle(TR);
                            Polyline3d acPoly3d = new Polyline3d();
                            acPoly3d.SetDatabaseDefaults();
                            acBlkTblRec.AppendEntity(acPoly3d);
                            trans.AddNewlyCreatedDBObject(acPoly3d, true);

                            PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)ptt.First);
                            acPoly3d.AppendVertex(acPolVer3d);
                            trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                            PolylineVertex3d acPolVer3d1 = new PolylineVertex3d((Point3d)ptt.Second);
                            acPoly3d.AppendVertex(acPolVer3d1);
                            trans.AddNewlyCreatedDBObject(acPolVer3d1, true);

                            PolylineVertex3d acPolVer3d2 = new PolylineVertex3d((Point3d)ptt.Third);
                            acPoly3d.AppendVertex(acPolVer3d2);
                            trans.AddNewlyCreatedDBObject(acPolVer3d2, true);


                            PolylineVertex3d acPolVer3d3 = new PolylineVertex3d((Point3d)ptt.First);
                            acPoly3d.AppendVertex(acPolVer3d3);
                            trans.AddNewlyCreatedDBObject(acPolVer3d3, true);

                        }
                    }
                    trans.Commit();
                    ed.UpdateScreen();
                }
                #endregion

            }
            if ((obhvat == 0) || (obhvat == 2))
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl;
                    acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL in container.Polygons)
                    {
                        if (ByNumer >= 0)
                            if (POL.GetNumer() != ByNumer)
                                continue;

                        // List<List<MathLibKCAD.quaternion>> qlist1 = POL.Get_Glass_Edge(ref container.Bends, ref container.Nodes, ref container.Triangles);
                        List<List<quaternion>> qlist1 = container.Get_Glass_Edge(POL, variant);

                        #region draw polyline
                        Polyline3d acPoly3d = new Polyline3d();
                        acPoly3d.SetDatabaseDefaults();
                        //acPoly3d.ColorIndex = 3;
                        acBlkTblRec.AppendEntity(acPoly3d);
                        trans.AddNewlyCreatedDBObject(acPoly3d, true);
                        foreach (List<quaternion> llist in qlist1)
                        {
                            foreach (quaternion q in llist)
                            {
                                PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)q);
                                acPoly3d.AppendVertex(acPolVer3d);
                                trans.AddNewlyCreatedDBObject(acPolVer3d, true);
                            }
                        }
                        PolylineVertex3d acPolVer3d_ = new PolylineVertex3d((Point3d)qlist1[0][0]);
                        acPoly3d.AppendVertex(acPolVer3d_);
                        trans.AddNewlyCreatedDBObject(acPolVer3d_, true);
                        //acPoly3d.Close();
                        #endregion

                    }
                    trans.Commit();
                    ed.UpdateScreen();
                }
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SET_GLASS_EDGE", null, CommandFlags.Modal, null, "KojtoCAD_3D", "KojtoCAD_3D_Glass_Edges")]
        public void KojtoCAD_3D_Set_Glass_Edge()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Matrix3d old = ed.CurrentUserCoordinateSystem;
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                try
                {

                    Line l = new Line();

                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                    pIntOpts.Message = "\nEnter the Bend  Numer ";

                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                    if (pIntRes.Status == PromptStatus.OK)
                    {
                        int N = pIntRes.Value - 1;
                        global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle tr = container.Triangles[container.Bends[N].FirstTriangleNumer];

                        #region hatch
                        using (Transaction acTrans = db.TransactionManager.StartTransaction())
                        {
                            BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                            BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                            #region hatch
                            quaternion lq = (tr.Normal.First - container.Bends[N].MidPoint) / 5.0;
                            lq += container.Bends[N].MidPoint;

                            l = new Line((Point3d)container.Bends[N].MidPoint, (Point3d)lq);
                            acBlkTblRec.AppendEntity(l);
                            acTrans.AddNewlyCreatedDBObject(l, true);

                            #endregion

                            acTrans.Commit();
                            ed.UpdateScreen();
                        }
                        #endregion

                        PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                        pDoubleOpts.Message = "\nEnter the Bend Distance for first Triangle";

                        pDoubleOpts.AllowZero = false;
                        pDoubleOpts.AllowNegative = false;

                        PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                        if (pDoubleRes.Status == PromptStatus.OK)
                        {
                            container.Bends[N].SetFirstTriangleOffset(pDoubleRes.Value);
                            if (container.Bends[N].SecondTriangleNumer >= 0)
                            {
                                pDoubleOpts = new PromptDoubleOptions("");
                                pDoubleOpts.Message = "\nEnter the Bend Distance for second Triangle";
                                pDoubleOpts.AllowZero = false;
                                pDoubleOpts.AllowNegative = false;
                                pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                                if (pDoubleRes.Status == PromptStatus.OK)
                                {
                                    container.Bends[N].SetSecondTriangleOffset(pDoubleRes.Value);
                                }
                            }
                        }
                        using (Transaction acTrans = db.TransactionManager.StartTransaction())
                        {
                            Entity ent = acTrans.GetObject(l.ObjectId, OpenMode.ForWrite) as Entity;
                            ent.Erase();
                            acTrans.Commit();
                            ed.UpdateScreen();
                        }
                    }
                }
                catch { }
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SHOW_GLASS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SHOW_GLASS.htm", "")]
        public void KojtoCAD_3D_KCAD_SHOW_GLASS()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    if (global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Single_or_Double_Glass == 1)
                    {
                        int polygonNumer = -1;
                        try
                        {
                            double height = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass;

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
                                switch (pKeyRes.StringResult)
                                {
                                    case "All":
                                        #region All

                                        #region triangles extrude
                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                                        {
                                            using (Transaction trans = db.TransactionManager.StartTransaction())
                                            {
                                                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR);
                                                if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                                                {
                                                    Triplet<quaternion, quaternion, quaternion> ptt = pt;// container.GetInnererTriangle(TR);
                                                    Polyline3d acPoly3d = new Polyline3d();
                                                    acPoly3d.SetDatabaseDefaults();
                                                    acBlkTblRec.AppendEntity(acPoly3d);
                                                    trans.AddNewlyCreatedDBObject(acPoly3d, true);

                                                    PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)ptt.First);
                                                    acPoly3d.AppendVertex(acPolVer3d);
                                                    trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                                    PolylineVertex3d acPolVer3d1 = new PolylineVertex3d((Point3d)ptt.Second);
                                                    acPoly3d.AppendVertex(acPolVer3d1);
                                                    trans.AddNewlyCreatedDBObject(acPolVer3d1, true);

                                                    PolylineVertex3d acPolVer3d2 = new PolylineVertex3d((Point3d)ptt.Third);
                                                    acPoly3d.AppendVertex(acPolVer3d2);
                                                    trans.AddNewlyCreatedDBObject(acPolVer3d2, true);


                                                    PolylineVertex3d acPolVer3d3 = new PolylineVertex3d((Point3d)ptt.First);
                                                    acPoly3d.AppendVertex(acPolVer3d3);
                                                    trans.AddNewlyCreatedDBObject(acPolVer3d3, true);

                                                    quaternion vec = TR.Normal.Second - TR.Normal.First;
                                                    vec /= vec.abs(); vec *= height;
                                                    Solid3d acSol3D = new Solid3d();
                                                    acSol3D.SetDatabaseDefaults();
                                                    acSol3D.CreateExtrudedSolid(acPoly3d, new Vector3d(vec.GetX(), vec.GetY(), vec.GetZ()), new SweepOptions());
                                                    acBlkTblRec.AppendEntity(acSol3D);
                                                    trans.AddNewlyCreatedDBObject(acSol3D, true);

                                                    TR.lowSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(1, acSol3D.Handle);

                                                    acPoly3d.Erase();

                                                    trans.Commit();
                                                    ed.UpdateScreen();
                                                }
                                            }
                                        }
                                        #endregion

                                        #region Polygons
                                        using (Transaction trans = db.TransactionManager.StartTransaction())
                                        {
                                            BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                            foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL in container.Polygons)
                                            {
                                                polygonNumer = POL.GetNumer() + 1;
                                                Solid3d solid = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight(ref container, POL, height, trans, acBlkTblRec);

                                                try
                                                {
                                                    acBlkTblRec.AppendEntity(solid);
                                                    trans.AddNewlyCreatedDBObject(solid, true);
                                                }
                                                catch { }

                                                foreach (int bN in POL.Triangles_Numers_Array)
                                                {
                                                    if (solid != (object)null)
                                                        container.Triangles[bN].lowSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(1, solid.Handle);
                                                }
                                            }
                                            trans.Commit();
                                            ed.UpdateScreen();
                                        }
                                        #endregion

                                        #endregion
                                        break;
                                    case "Triangles":

                                        #region Triangles
                                        PromptKeywordOptions pKeyOpts_ = new PromptKeywordOptions("");
                                        pKeyOpts_.Message = "\nEnter an option ";
                                        pKeyOpts_.Keywords.Add("All");
                                        pKeyOpts_.Keywords.Add("ByNumer");
                                        pKeyOpts_.Keywords.Default = "All";
                                        pKeyOpts_.AllowNone = true;
                                        PromptResult pKeyRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts_);
                                        if (pKeyRes_.Status == PromptStatus.OK)
                                        {
                                            switch (pKeyRes_.StringResult)
                                            {
                                                case "All":
                                                    #region All Triangles

                                                    foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                                                    {
                                                        using (Transaction trans = db.TransactionManager.StartTransaction())
                                                        {
                                                            BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                            Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR);
                                                            if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                                                            {
                                                                Triplet<quaternion, quaternion, quaternion> ptt = pt;// container.GetInnererTriangle(TR);
                                                                Polyline3d acPoly3d = new Polyline3d();
                                                                acPoly3d.SetDatabaseDefaults();
                                                                acBlkTblRec.AppendEntity(acPoly3d);
                                                                trans.AddNewlyCreatedDBObject(acPoly3d, true);

                                                                PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)ptt.First);
                                                                acPoly3d.AppendVertex(acPolVer3d);
                                                                trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                                                PolylineVertex3d acPolVer3d1 = new PolylineVertex3d((Point3d)ptt.Second);
                                                                acPoly3d.AppendVertex(acPolVer3d1);
                                                                trans.AddNewlyCreatedDBObject(acPolVer3d1, true);

                                                                PolylineVertex3d acPolVer3d2 = new PolylineVertex3d((Point3d)ptt.Third);
                                                                acPoly3d.AppendVertex(acPolVer3d2);
                                                                trans.AddNewlyCreatedDBObject(acPolVer3d2, true);


                                                                PolylineVertex3d acPolVer3d3 = new PolylineVertex3d((Point3d)ptt.First);
                                                                acPoly3d.AppendVertex(acPolVer3d3);
                                                                trans.AddNewlyCreatedDBObject(acPolVer3d3, true);

                                                                quaternion vec = TR.Normal.Second - TR.Normal.First;
                                                                vec /= vec.abs(); vec *= height;
                                                                Solid3d acSol3D = new Solid3d();
                                                                acSol3D.SetDatabaseDefaults();
                                                                acSol3D.CreateExtrudedSolid(acPoly3d, new Vector3d(vec.GetX(), vec.GetY(), vec.GetZ()), new SweepOptions());
                                                                acBlkTblRec.AppendEntity(acSol3D);
                                                                trans.AddNewlyCreatedDBObject(acSol3D, true);

                                                                TR.lowSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(1, acSol3D.Handle);

                                                                acPoly3d.Erase();

                                                                trans.Commit();
                                                                ed.UpdateScreen();
                                                            }
                                                        }
                                                    }

                                                    #endregion
                                                    break;
                                                case "ByNumer":

                                                    #region Triangle By Numer
                                                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                                                    pIntOpts.Message = "\nEnter the triangle  Numer ";

                                                    pIntOpts.AllowZero = false;
                                                    pIntOpts.AllowNegative = false;

                                                    PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                                                    if (pIntRes.Status == PromptStatus.OK)
                                                    {
                                                        int N = pIntRes.Value - 1;

                                                        if (N < container.Triangles.Count)
                                                        {
                                                            global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = container.Triangles[N];

                                                            if (container.Bends[TR.GetFirstBendNumer()].IsFictive() ||
                                                                container.Bends[TR.GetSecondBendNumer()].IsFictive() ||
                                                                 container.Bends[TR.GetThirdBendNumer()].IsFictive())
                                                            {
                                                                MessageBox.Show("Triangle is Fictive !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                            }
                                                            else
                                                            {

                                                                using (Transaction trans = db.TransactionManager.StartTransaction())
                                                                {
                                                                    BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                                    BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                                    Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR);
                                                                    if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                                                                    {
                                                                        Triplet<quaternion, quaternion, quaternion> ptt = pt;// container.GetInnererTriangle(TR);
                                                                        Polyline3d acPoly3d = new Polyline3d();
                                                                        acPoly3d.SetDatabaseDefaults();
                                                                        acBlkTblRec.AppendEntity(acPoly3d);
                                                                        trans.AddNewlyCreatedDBObject(acPoly3d, true);

                                                                        PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)ptt.First);
                                                                        acPoly3d.AppendVertex(acPolVer3d);
                                                                        trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                                                        PolylineVertex3d acPolVer3d1 = new PolylineVertex3d((Point3d)ptt.Second);
                                                                        acPoly3d.AppendVertex(acPolVer3d1);
                                                                        trans.AddNewlyCreatedDBObject(acPolVer3d1, true);

                                                                        PolylineVertex3d acPolVer3d2 = new PolylineVertex3d((Point3d)ptt.Third);
                                                                        acPoly3d.AppendVertex(acPolVer3d2);
                                                                        trans.AddNewlyCreatedDBObject(acPolVer3d2, true);


                                                                        PolylineVertex3d acPolVer3d3 = new PolylineVertex3d((Point3d)ptt.First);
                                                                        acPoly3d.AppendVertex(acPolVer3d3);
                                                                        trans.AddNewlyCreatedDBObject(acPolVer3d3, true);

                                                                        quaternion vec = TR.Normal.Second - TR.Normal.First;
                                                                        vec /= vec.abs(); vec *= height;
                                                                        Solid3d acSol3D = new Solid3d();
                                                                        acSol3D.SetDatabaseDefaults();
                                                                        acSol3D.CreateExtrudedSolid(acPoly3d, new Vector3d(vec.GetX(), vec.GetY(), vec.GetZ()), new SweepOptions());
                                                                        acBlkTblRec.AppendEntity(acSol3D);
                                                                        trans.AddNewlyCreatedDBObject(acSol3D, true);

                                                                        TR.lowSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(1, acSol3D.Handle);

                                                                        acPoly3d.Erase();

                                                                        trans.Commit();
                                                                        ed.UpdateScreen();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            MessageBox.Show("Triangle Numer - Out of Range !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        }
                                                    }
                                                    # endregion

                                                    break;
                                            }
                                        }
                                        #endregion

                                        break;
                                    case "Polygons":

                                        #region polygons

                                        PromptKeywordOptions pKeyOpts__ = new PromptKeywordOptions("");
                                        pKeyOpts__.Message = "\nEnter an option ";
                                        pKeyOpts__.Keywords.Add("All");
                                        pKeyOpts__.Keywords.Add("ByNumer");
                                        pKeyOpts__.Keywords.Default = "All";
                                        pKeyOpts__.AllowNone = true;
                                        PromptResult pKeyRes__ = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts__);
                                        if (pKeyRes__.Status == PromptStatus.OK)
                                        {
                                            switch (pKeyRes__.StringResult)
                                            {
                                                case "All":
                                                    #region All
                                                    using (Transaction trans = db.TransactionManager.StartTransaction())
                                                    {
                                                        BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                        BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL in container.Polygons)
                                                        {
                                                            polygonNumer = POL.GetNumer() + 1;
                                                            Solid3d solid = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight(ref container, POL, height, trans, acBlkTblRec);

                                                            try
                                                            {
                                                                acBlkTblRec.AppendEntity(solid);
                                                                trans.AddNewlyCreatedDBObject(solid, true);

                                                            }
                                                            catch { }


                                                            foreach (int bN in POL.Triangles_Numers_Array)
                                                            {
                                                                if (solid != (object)null)
                                                                    container.Triangles[bN].lowSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(1, solid.Handle);
                                                            }
                                                        }
                                                        trans.Commit();
                                                        ed.UpdateScreen();
                                                        polygonNumer = -1;
                                                    }
                                                    #endregion
                                                    break;
                                                case "ByNumer":

                                                    #region by numer
                                                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                                                    pIntOpts.Message = "\nEnter the Polygon  Numer ";

                                                    pIntOpts.AllowZero = false;
                                                    pIntOpts.AllowNegative = false;

                                                    PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                                                    if (pIntRes.Status == PromptStatus.OK)
                                                    {
                                                        int N = pIntRes.Value - 1;
                                                        if (N < container.Polygons.Count)
                                                        {
                                                            global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL = container.Polygons[N];
                                                            polygonNumer = POL.GetNumer() + 1;

                                                            using (Transaction trans = db.TransactionManager.StartTransaction())
                                                            {
                                                                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                                Solid3d solid = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight(ref container, POL, height, trans, acBlkTblRec);
                                                                try
                                                                {
                                                                    acBlkTblRec.AppendEntity(solid);
                                                                    trans.AddNewlyCreatedDBObject(solid, true);
                                                                }
                                                                catch { }

                                                                foreach (int bN in POL.Triangles_Numers_Array)
                                                                {
                                                                    container.Triangles[bN].lowSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(1, solid.Handle);
                                                                }

                                                                trans.Commit();
                                                                ed.UpdateScreen();
                                                            }
                                                            polygonNumer = -1;
                                                        }
                                                        else
                                                        {
                                                            MessageBox.Show("Polygon Numer - Out of Range !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        }
                                                    }
                                                    #endregion

                                                    break;
                                            }
                                        }
                                        #endregion

                                        break;
                                }
                            }
                        }
                        catch
                        {
                            string mess = "Try changing 3D machine (Command: KCAD_CHANGE_3D_MASHINE)\nor\nTry reducing Extrude Ratio (Command: KCAD_SET_CUT_SOLID_ER)";
                            if (polygonNumer >= 0)
                            {
                                mess += "\n\nPolygon:" + polygonNumer.ToString();
                                global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POl = container.Polygons[polygonNumer - 1];
                                mess += "\n";

                                foreach (int N in POl.Bends_Numers_Array)
                                {
                                    global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend be = container.Bends[N];
                                    bool b = POl.Triangles_Numers_Array.IndexOf(be.FirstTriangleNumer) >= 0;
                                    double bOffet = b ? be.FirstTriangleOffset : be.SecondTriangleOffset;
                                    double bExtruderatio = b ? be.explicit_first_cutExtrudeRatio : be.explicit_second_cutExtrudeRatio;
                                    if (bExtruderatio < 0)
                                        bExtruderatio = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.cutSolidsExtrudeRatio;

                                    string mess1 = "Bend " + (be.GetNumer() + 1).ToString() + ": ";
                                    mess1 += string.Format("glass Offset = {0}      Extrude Ratio = {1}", bOffet, bExtruderatio);
                                    mess1 += "\n";
                                    mess += mess1;
                                }
                            }
                            MessageBox.Show(mess, "Modeling error !");
                        }
                    }
                    else
                        MessageBox.Show("\nSettings\\Glass\\Single is off - E R R O R  !\n\nSelect\nKojtoCAD_3D\\DOUBLE GLASS\\SHOW", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_HIDE_GLASS", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_KCAD_HIDE_GLASS()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    if (global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Single_or_Double_Glass == 1)
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
                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                                        {
                                            if (!(TR.IsFirstBendFictive() || TR.IsSecondBendFictive() || TR.IsThirdBendFictive()))
                                            {
                                                if (TR.lowSolidHandle.First >= 0)
                                                {
                                                    try
                                                    {
                                                        Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        ent.Visible = false;
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL in container.Polygons)
                                        {
                                            global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = container.Triangles[POL.Triangles_Numers_Array[0]];
                                            if (TR.lowSolidHandle.First >= 0)
                                            {
                                                try
                                                {
                                                    Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    ent.Visible = false;
                                                }
                                                catch { }
                                            }
                                        }
                                        break;
                                    case "Triangles":
                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                                        {
                                            if (!(TR.IsFirstBendFictive() || TR.IsSecondBendFictive() || TR.IsThirdBendFictive()))
                                            {
                                                if (TR.lowSolidHandle.First >= 0)
                                                {
                                                    try
                                                    {
                                                        Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        ent.Visible = false;
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                        break;
                                    case "Polygons":
                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL in container.Polygons)
                                        {
                                            global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = container.Triangles[POL.Triangles_Numers_Array[0]];
                                            if (TR.lowSolidHandle.First >= 0)
                                            {
                                                try
                                                {
                                                    Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    ent.Visible = false;
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

        [CommandMethod("KojtoCAD_3D", "KCAD_SHOW_HIDEN_GLASS", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_KCAD_HIDEN_GLASS()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    if (global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Single_or_Double_Glass == 1)
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
                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                                        {
                                            if (!(TR.IsFirstBendFictive() || TR.IsSecondBendFictive() || TR.IsThirdBendFictive()))
                                            {
                                                if (TR.lowSolidHandle.First >= 0)
                                                {
                                                    try
                                                    {
                                                        Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        ent.Visible = true;
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL in container.Polygons)
                                        {
                                            global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = container.Triangles[POL.Triangles_Numers_Array[0]];
                                            if (TR.lowSolidHandle.First >= 0)
                                            {
                                                try
                                                {
                                                    Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    ent.Visible = true;
                                                }
                                                catch { }
                                            }
                                        }
                                        break;
                                    case "Triangles":
                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                                        {
                                            if (!(TR.IsFirstBendFictive() || TR.IsSecondBendFictive() || TR.IsThirdBendFictive()))
                                            {
                                                if (TR.lowSolidHandle.First >= 0)
                                                {
                                                    try
                                                    {
                                                        Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                        ent.Visible = true;
                                                    }
                                                    catch { }
                                                }
                                            }
                                        }
                                        break;
                                    case "Polygons":
                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL in container.Polygons)
                                        {
                                            global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = container.Triangles[POL.Triangles_Numers_Array[0]];
                                            if (TR.lowSolidHandle.First >= 0)
                                            {
                                                try
                                                {
                                                    Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                                    ent.Visible = true;
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

        [CommandMethod("KojtoCAD_3D", "KCAD_PLACEMENT_3D_GLASS_DELETE", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_Placement3d_Glass_Delete()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container.Triangles.Count > 0) && (container.Bends.Count > 0))
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                        {
                            if (TR.lowSolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    TR.lowSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                                    ent.Erase();
                                }
                                catch
                                {
                                    TR.lowSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                                }
                            }

                            if (TR.upSolidHandle.First >= 0)
                            {
                                try
                                {
                                    Entity ent = tr.GetObject(global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Entity;
                                    TR.upSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                                    ent.Erase();
                                }
                                catch
                                {
                                    TR.upSolidHandle = new global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
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

        [CommandMethod("KojtoCAD_3D", "KCAD_SET_GLASS_DISTANCE_FOR_BEND", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SET_GLASS_DISTANCE_FOR_BEND.htm", "")]
        public void KojtoCAD_3D_SET_GLASS_DISTANCE_FOR_BEND()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if (container != null)
                {
                    if (container.Bends.Count > 0)
                    {
                        if (container.Triangles.Count > 0)
                        {
                            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                            pDoubleOpts.Message = "\nEnter the Distance : ";

                            pDoubleOpts.AllowZero = false;
                            pDoubleOpts.AllowNegative = false;
                            PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                            if (pDoubleRes.Status == PromptStatus.OK)
                            {
                                double dist = pDoubleRes.Value;

                                do
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
                                            if (ptSecond.DistanceTo(ptFirst) >= global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.MinBendLength)
                                            {
                                                Pair<quaternion, quaternion> pb = new Pair<quaternion, quaternion>(new quaternion(0, ptFirst.X, ptFirst.Y, ptFirst.Z), new quaternion(0, ptSecond.X, ptSecond.Y, ptSecond.Z));
                                                global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend Bend = null;
                                                foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend in container.Bends)
                                                {
                                                    if (bend == pb)
                                                    {
                                                        Bend = bend;
                                                        break;
                                                    }
                                                }
                                                if ((object)Bend != null)
                                                {
                                                    if (!Bend.IsFictive())
                                                    {
                                                        //---------------------------------
                                                        PromptPointResult tPtRes;
                                                        PromptPointOptions tPtOpts = new PromptPointOptions("");
                                                        tPtOpts.Message = "\nEnter the third Point of the Triangle: ";
                                                        tPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(tPtOpts);
                                                        if (tPtRes.Status == PromptStatus.OK)
                                                        {
                                                            Triplet<quaternion, quaternion, quaternion> pTR = new Triplet<quaternion, quaternion, quaternion>(new quaternion(0, ptFirst.X, ptFirst.Y, ptFirst.Z),
                                                                                new quaternion(0, ptSecond.X, ptSecond.Y, ptSecond.Z),
                                                                                new quaternion(0, tPtRes.Value.X, tPtRes.Value.Y, tPtRes.Value.Z));
                                                            global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = null;
                                                            foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle tr in container.Triangles)
                                                            {
                                                                if (tr == pTR)
                                                                {
                                                                    TR = tr;
                                                                    break;
                                                                }
                                                            }
                                                            if ((object)TR != null)
                                                            {
                                                                if (Bend.FirstTriangleNumer == TR.Numer)
                                                                    Bend.SetFirstTriangleOffset(dist);
                                                                else
                                                                    Bend.SetSecondTriangleOffset(dist);
                                                            }
                                                            else
                                                            {
                                                                MessageBox.Show("\nTriangle not found - E R R O R  !", "E R R O R - Selection Triangle", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                                ed.WriteMessage("\nTriangle not found - E R R O R  !");
                                                            }
                                                        }
                                                        //---------------------------------
                                                    }
                                                    else
                                                        MessageBox.Show("\nBend is Fictive - E R R O R  !", "E R R O R - Selection Bend", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                } while (MessageBox.Show("Select next ?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
                            }
                            else
                                MessageBox.Show("\nData Base Empty !\n\nMissing Triangles !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                        MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SET_GLASS_DISTANCE_FOR_ALL_BENDS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SET_GLASS_DISTANCE_FOR_ALL_BENDS.htm", "")]
        public void KojtoCAD_3D_SET_GLASS_DISTANCE_FOR_ALL_BENDS()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message = "\nEnter the Distance: ";

                    pDoubleOpts.AllowZero = false;
                    pDoubleOpts.AllowNegative = false;
                    pDoubleOpts.DefaultValue = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.halfGlassFugue;

                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
                        // UtilityClasses.ConstantsAndSettings.halfGlassFugue = pDoubleRes.Value;
                        for (int i = 0; i < container.Bends.Count; i++)
                        {
                            container.Bends[i].SetFirstTriangleOffset(pDoubleRes.Value);
                            container.Bends[i].SetSecondTriangleOffset(pDoubleRes.Value);
                        }
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        //------
        [CommandMethod("KojtoCAD_3D", "KCAD_SET_GLASS_DISTANCE_TO_BEND_BY_NUMER", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SET_GLASS_DISTANCE_TO_BEND_BY_NUMER.htm", "")]
        public void KojtoCAD_3D_SET_GLASS_DISTANCE_TO__BEND_BY_NUMER()
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
                            global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend BEND = container.Bends[N];
                            if ((object)BEND != null)
                            {
                                //-----
                                PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                                pDoubleOpts.Message = "\nEnter the Distance in Triangle Numer " + (BEND.GetFirstTriangleNumer() + 1).ToString() + " : ";

                                pDoubleOpts.AllowZero = false;
                                pDoubleOpts.AllowNegative = false;

                                PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                                if (pDoubleRes.Status == PromptStatus.OK)
                                {
                                    BEND.SetFirstTriangleOffset(pDoubleRes.Value);
                                }
                                //-----
                                PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                                pDoubleOpts_.Message = "\nEnter the Distance in Triangle Numer " + (BEND.GetSecondTriangleNumer() + 1).ToString() + " : ";

                                pDoubleOpts_.AllowZero = false;
                                pDoubleOpts_.AllowNegative = false;

                                PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                                if (pDoubleRes_.Status == PromptStatus.OK)
                                {
                                    BEND.SetSecondTriangleOffset(pDoubleRes_.Value);
                                }
                                //-----------
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
        //------

        [CommandMethod("KojtoCAD_3D", "GET_GlASS_CONTURS_BY_LEVEL", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/GlASS_CONTURS_BY_LEVEL.htm", "")]
        public void KojtoCAD_3D_GlASS_CONTURS_BY_LEVEL()
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
                    pDoubleOpts.Message = "\nEnter Height Offset from Triangle Plane:";
                    pDoubleOpts.AllowZero = false;
                    pDoubleOpts.AllowNegative = false;
                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    if (pDoubleRes.Status == PromptStatus.OK)
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
                            switch (pKeyRes.StringResult)
                            {
                                case "All":
                                    #region All
                                    KojtoCAD_3D_Glass_3d_Contur(pDoubleRes.Value);
                                    #region triangles extrude
                                    using (Transaction trans = db.TransactionManager.StartTransaction())
                                    {
                                        BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                        BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                                        {
                                            Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR);
                                            quaternion tr_Normal = TR.Normal.Second - TR.Normal.First;
                                            tr_Normal /= tr_Normal.abs(); tr_Normal *= pDoubleRes.Value;
                                            if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                                            {
                                                Triplet<quaternion, quaternion, quaternion> ptt = container.GetInnererTriangle(TR);
                                                ptt.First += tr_Normal; ptt.Second += tr_Normal; ptt.Third += tr_Normal;

                                                Polyline3d acPoly3d = new Polyline3d();
                                                acPoly3d.SetDatabaseDefaults();
                                                acBlkTblRec.AppendEntity(acPoly3d);
                                                trans.AddNewlyCreatedDBObject(acPoly3d, true);

                                                PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)ptt.First);
                                                acPoly3d.AppendVertex(acPolVer3d);
                                                trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                                PolylineVertex3d acPolVer3d1 = new PolylineVertex3d((Point3d)ptt.Second);
                                                acPoly3d.AppendVertex(acPolVer3d1);
                                                trans.AddNewlyCreatedDBObject(acPolVer3d1, true);

                                                PolylineVertex3d acPolVer3d2 = new PolylineVertex3d((Point3d)ptt.Third);
                                                acPoly3d.AppendVertex(acPolVer3d2);
                                                trans.AddNewlyCreatedDBObject(acPolVer3d2, true);


                                                PolylineVertex3d acPolVer3d3 = new PolylineVertex3d((Point3d)ptt.First);
                                                acPoly3d.AppendVertex(acPolVer3d3);
                                                trans.AddNewlyCreatedDBObject(acPolVer3d3, true);

                                            }
                                        }
                                        trans.Commit();
                                        ed.UpdateScreen();
                                    }
                                    #endregion
                                    #endregion
                                    break;
                                case "Triangles":

                                    #region Triangles
                                    PromptKeywordOptions pKeyOpts_ = new PromptKeywordOptions("");
                                    pKeyOpts_.Message = "\nEnter an option ";
                                    pKeyOpts_.Keywords.Add("All");
                                    pKeyOpts_.Keywords.Add("ByNumer");
                                    pKeyOpts_.Keywords.Default = "All";
                                    pKeyOpts_.AllowNone = true;
                                    PromptResult pKeyRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts_);
                                    if (pKeyRes_.Status == PromptStatus.OK)
                                    {
                                        switch (pKeyRes_.StringResult)
                                        {
                                            case "All":
                                                #region All Triangles

                                                #region triangles extrude
                                                using (Transaction trans = db.TransactionManager.StartTransaction())
                                                {
                                                    BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                    BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                    foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR in container.Triangles)
                                                    {
                                                        Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR);
                                                        quaternion tr_Normal = TR.Normal.Second - TR.Normal.First;
                                                        tr_Normal /= tr_Normal.abs(); tr_Normal *= pDoubleRes.Value;
                                                        if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                                                        {
                                                            Triplet<quaternion, quaternion, quaternion> ptt = container.GetInnererTriangle(TR);
                                                            ptt.First += tr_Normal; ptt.Second += tr_Normal; ptt.Third += tr_Normal;

                                                            Polyline3d acPoly3d = new Polyline3d();
                                                            acPoly3d.SetDatabaseDefaults();
                                                            acBlkTblRec.AppendEntity(acPoly3d);
                                                            trans.AddNewlyCreatedDBObject(acPoly3d, true);

                                                            PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)ptt.First);
                                                            acPoly3d.AppendVertex(acPolVer3d);
                                                            trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                                            PolylineVertex3d acPolVer3d1 = new PolylineVertex3d((Point3d)ptt.Second);
                                                            acPoly3d.AppendVertex(acPolVer3d1);
                                                            trans.AddNewlyCreatedDBObject(acPolVer3d1, true);

                                                            PolylineVertex3d acPolVer3d2 = new PolylineVertex3d((Point3d)ptt.Third);
                                                            acPoly3d.AppendVertex(acPolVer3d2);
                                                            trans.AddNewlyCreatedDBObject(acPolVer3d2, true);


                                                            PolylineVertex3d acPolVer3d3 = new PolylineVertex3d((Point3d)ptt.First);
                                                            acPoly3d.AppendVertex(acPolVer3d3);
                                                            trans.AddNewlyCreatedDBObject(acPolVer3d3, true);

                                                        }
                                                    }
                                                    trans.Commit();
                                                    ed.UpdateScreen();
                                                }
                                                #endregion

                                                #endregion
                                                break;
                                            case "ByNumer":

                                                #region Triangle By Numer
                                                PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                                                pIntOpts.Message = "\nEnter the triangle  Numer ";

                                                pIntOpts.AllowZero = false;
                                                pIntOpts.AllowNegative = false;

                                                PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                                                if (pIntRes.Status == PromptStatus.OK)
                                                {
                                                    int N = pIntRes.Value - 1;

                                                    if (N < container.Triangles.Count)
                                                    {
                                                        global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = container.Triangles[N];

                                                        if (container.Bends[TR.GetFirstBendNumer()].IsFictive() ||
                                                            container.Bends[TR.GetSecondBendNumer()].IsFictive() ||
                                                             container.Bends[TR.GetThirdBendNumer()].IsFictive())
                                                        {
                                                            MessageBox.Show("Triangle is Fictive !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        }
                                                        else
                                                        {
                                                            #region triangles extrude

                                                            using (Transaction trans = db.TransactionManager.StartTransaction())
                                                            {
                                                                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                                {
                                                                    Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR);
                                                                    quaternion tr_Normal = TR.Normal.Second - TR.Normal.First;
                                                                    tr_Normal /= tr_Normal.abs(); tr_Normal *= pDoubleRes.Value;
                                                                    if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                                                                    {
                                                                        Triplet<quaternion, quaternion, quaternion> ptt = container.GetInnererTriangle(TR);
                                                                        ptt.First += tr_Normal; ptt.Second += tr_Normal; ptt.Third += tr_Normal;

                                                                        Polyline3d acPoly3d = new Polyline3d();
                                                                        acPoly3d.SetDatabaseDefaults();
                                                                        acBlkTblRec.AppendEntity(acPoly3d);
                                                                        trans.AddNewlyCreatedDBObject(acPoly3d, true);

                                                                        PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)ptt.First);
                                                                        acPoly3d.AppendVertex(acPolVer3d);
                                                                        trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                                                        PolylineVertex3d acPolVer3d1 = new PolylineVertex3d((Point3d)ptt.Second);
                                                                        acPoly3d.AppendVertex(acPolVer3d1);
                                                                        trans.AddNewlyCreatedDBObject(acPolVer3d1, true);

                                                                        PolylineVertex3d acPolVer3d2 = new PolylineVertex3d((Point3d)ptt.Third);
                                                                        acPoly3d.AppendVertex(acPolVer3d2);
                                                                        trans.AddNewlyCreatedDBObject(acPolVer3d2, true);


                                                                        PolylineVertex3d acPolVer3d3 = new PolylineVertex3d((Point3d)ptt.First);
                                                                        acPoly3d.AppendVertex(acPolVer3d3);
                                                                        trans.AddNewlyCreatedDBObject(acPolVer3d3, true);

                                                                    }
                                                                }
                                                                trans.Commit();
                                                                ed.UpdateScreen();
                                                            }
                                                            #endregion

                                                        }

                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show("Triangle Numer - Out of Range !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                    }
                                                }
                                                # endregion

                                                break;
                                        }
                                    }
                                    #endregion

                                    break;

                                case "Polygons":
                                    #region polygons
                                    PromptKeywordOptions pKeyOpts__ = new PromptKeywordOptions("");
                                    pKeyOpts__.Message = "\nEnter an option ";
                                    pKeyOpts__.Keywords.Add("All");
                                    pKeyOpts__.Keywords.Add("ByNumer");
                                    pKeyOpts__.Keywords.Default = "All";
                                    pKeyOpts__.AllowNone = true;
                                    PromptResult pKeyRes__ = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts__);
                                    if (pKeyRes__.Status == PromptStatus.OK)
                                    {
                                        switch (pKeyRes__.StringResult)
                                        {
                                            case "All":
                                                #region All
                                                KojtoCAD_3D_Glass_3d_Contur(pDoubleRes.Value);
                                                #endregion
                                                break;
                                            case "ByNumer":

                                                #region by numer
                                                PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                                                pIntOpts.Message = "\nEnter the Polygon  Numer ";

                                                pIntOpts.AllowZero = false;
                                                pIntOpts.AllowNegative = false;

                                                PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                                                if (pIntRes.Status == PromptStatus.OK)
                                                {
                                                    int N = pIntRes.Value - 1;
                                                    if (N < container.Polygons.Count)
                                                    {
                                                        KojtoCAD_3D_Glass_3d_Contur(pDoubleRes.Value, N);
                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show("Polygon Numer - Out of Range !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                    }
                                                }
                                                #endregion

                                                break;
                                        }
                                    }
                                    #endregion
                                    break;
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
                finally { ed.CurrentUserCoordinateSystem = old; }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public List<Pair<int, Point3dCollection>> KojtoCAD_3D_Glass_3d_Contur(double height, bool show3d = true)
        {
            //double height = UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass;
            List<Pair<int, Point3dCollection>> rez = new List<Pair<int, Point3dCollection>>();

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL in container.Polygons)
                {
                    rez = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_Ex(ref container, POL, height, trans, acBlkTblRec);
                }
                trans.Commit();
                ed.UpdateScreen();
            }

            return rez;
        }
        public List<Pair<int, Point3dCollection>> KojtoCAD_3D_Glass_3d_Contur(double height, int polNumer, bool show3d = true)
        {
            //double height = UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass;
            List<Pair<int, Point3dCollection>> rez = new List<Pair<int, Point3dCollection>>();

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL = container.Polygons[polNumer];
                rez = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_Ex(ref container, POL, height, trans, acBlkTblRec);

                trans.Commit();
                ed.UpdateScreen();
            }

            return rez;
        }

        [CommandMethod("KojtoCAD_3D", "GET_GlASS_CONTURS_UNFOLD_BY_LEVEL", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/GET_GlASS_CONTURS_UNFOLD_BY_LEVEL.htm", "")]
        public void KojtoCAD_3D_GlASS_CONTURS_UNFOLD_BY_LEVEL()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                if (global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Single_or_Double_Glass == 1)
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

                            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                            pDoubleOpts.Message = "\nEnter Height Offset from Triangle Plane:";
                            pDoubleOpts.AllowZero = false;
                            pDoubleOpts.AllowNegative = false;
                            PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                            if (pDoubleRes.Status == PromptStatus.OK)
                            {

                                int N = pIntRes.Value - 1;
                                if (N < container.Polygons.Count)
                                {
                                    global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL = container.Polygons[N];
                                    if (POL.IsPlanar(ref container.Triangles))
                                    {
                                        using (Transaction trans = db.TransactionManager.StartTransaction())
                                        {
                                            BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                            #region planar

                                            List<Pair<int, Point3dCollection>> points = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_Ex(ref container,
                                                POL, pDoubleRes.Value, trans, acBlkTblRec, false);

                                            #region mass center
                                            quaternion cen = new quaternion();
                                            foreach (Point3d P in points[0].Second)
                                            {
                                                cen += P;
                                            }
                                            cen /= points[0].Second.Count;
                                            #endregion

                                            #region select point != [0]
                                            Point3d second = points[0].Second[1];
                                            if (points[0].Second[0].DistanceTo(second) < global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes())
                                            {
                                                for (int k = 2; k < points[0].Second.Count; k++)
                                                {
                                                    if (points[0].Second[0].DistanceTo(points[0].Second[k]) > global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes())
                                                    {
                                                        second = points[0].Second[k];
                                                        break;
                                                    }
                                                }
                                            }
                                            #endregion

                                            UCS ucs = new UCS(points[0].Second[0], second, cen);
                                            double[] mat = ucs.GetAutoCAD_Matrix3d();
                                            // Matrix3d acUCS = new Matrix3d(mat);

                                            #region create polyline
                                            Polyline pl = new Polyline();
                                            acBlkTblRec.AppendEntity(pl);
                                            trans.AddNewlyCreatedDBObject(pl, true);

                                            points[0].Second.Add(points[0].Second[0]);
                                            for (int i = 0; i < points[0].Second.Count; i++)
                                            {
                                                quaternion q = ucs.FromACS(points[0].Second[i]);
                                                pl.AddVertexAt(i, new Point2d(q.GetX(), q.GetY()), 0, 0, 0);
                                            }
                                            #endregion

                                            pl.TransformBy(Matrix3d.Displacement(pl.GeometricExtents.MinPoint.GetVectorTo(Point3d.Origin)));

                                            #endregion

                                            trans.Commit();
                                            ed.UpdateScreen();
                                        }
                                    }
                                    else
                                    {
                                        using (Transaction trans = db.TransactionManager.StartTransaction())
                                        {
                                            BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                            List<Pair<int, Point3dCollection>> rez = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_ExA(ref container, POL, pDoubleRes.Value, trans, acBlkTblRec);
                                            global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.ArrangeTrianglesByAdjoining(ref container, POL, ref rez, trans, acBlkTblRec);

                                            trans.Commit();
                                            ed.UpdateScreen();
                                        }

                                    }//
                                }
                                else
                                {
                                    MessageBox.Show("Polygon Numer Error !", "Range ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    finally { ed.CurrentUserCoordinateSystem = old; }
                }
                else
                    MessageBox.Show("\nSettings\\Glass\\Single is off - E R R O R  !\n\nSelect\nKojtoCAD_3D\\DOUBLE GLASS\\.....", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_GlASS_UNFOLDS_BY_LEVEL", null, CommandFlags.Session, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SEPARATE_GlASS_UNFOLDS_BY_LEVEL.htm", "")]
        public void KojtoCAD_3D_GlASS_UNFOLDS_BY_LEVEL()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    if (global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Single_or_Double_Glass == 1)
                    {
                        try
                        {

                            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                            pKeyOpts.Message = "\nEnter an option ";
                            pKeyOpts.Keywords.Add("Triangles");
                            pKeyOpts.Keywords.Add("Polygons");
                            pKeyOpts.Keywords.Default = "Polygons";
                            pKeyOpts.AllowNone = true;

                            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                            if (pKeyRes.Status == PromptStatus.OK)
                            {
                                switch (pKeyRes.StringResult)
                                {
                                    case "Triangles": KojtoCAD_3D_GlASS_UNFOLDS_BY_LEVEL_Triangles(); break;
                                    case "Polygons": KojtoCAD_3D_GlASS_UNFOLDS_BY_LEVEL_Polygons(); break;
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
                        MessageBox.Show("\nSettings\\Glass\\Single is off - E R R O R  !\n\nSelect\nKojtoCAD_3D\\DOUBLE GLASS\\.....", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        #region old
        /*
        public void KojtoCAD_3D_GlASS_UNFOLDS_BY_LEVEL_Polygons()
        {
            Document doc = MgdAcApplication.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = MgdAcApplication.DocumentManager.MdiActiveDocument.Editor;

            string sLocalRoot = MgdAcApplication.GetSystemVariable("LOCALROOTPREFIX") as string;
            string sTemplatePath = sLocalRoot + "Template\\acad.dwt";

            System.Windows.Forms.OpenFileDialog dlgT = new System.Windows.Forms.OpenFileDialog();
            dlgT.Filter = "DWT or DWG or DXF|*.dwt;*.dwg;dxf|All Files|*.*";
            dlgT.Multiselect = false;
            dlgT.Title = "Select DWT File ";
            dlgT.DefaultExt = "dwt";
            dlgT.InitialDirectory = sLocalRoot + "Template\\";
            dlgT.FileName = "*.dwt";
            if (dlgT.ShowDialog() == DialogResult.OK)
            {
                string templateName = dlgT.FileName;

                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.Description = "Select Destination Folder. \nDirectory that you want to use as the default for Results.";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string Path = dlg.SelectedPath;

                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message = "\nEnter Height Offset from Triangle Plane:";
                    pDoubleOpts.AllowZero = false;
                    pDoubleOpts.AllowNegative = false;
                    PromptDoubleResult pDoubleRes = MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
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

                                DocumentCollection acDocMgr = MgdAcApplication.DocumentManager;

                                for (int kk = 0; kk < container.Polygons.Count; kk++)
                                {
                                    WorkClasses.Polygon POL = container.Polygons[kk];
                                    Document acNewDoc = acDocMgr.Add(templateName);
                                    Database acDbNewDoc = acNewDoc.Database;
                                    acDocMgr.MdiActiveDocument = acNewDoc;
                                    using (DocumentLock acLckDoc = acNewDoc.LockDocument())
                                    {

                                        #region unfolding
                                        if (POL.IsPlanar(ref container.Triangles))
                                        {
                                            using (Transaction trans = acDbNewDoc.TransactionManager.StartTransaction())
                                            {
                                                BlockTable acBlkTbl = trans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                #region planar

                                                List<UtilityClasses.Pair<int, Point3dCollection>> points = UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_Ex(ref container,
                                                    POL, pDoubleRes.Value, trans, acBlkTblRec, false);

                                                #region mass center
                                                MathLibKCAD.quaternion cen = new MathLibKCAD.quaternion();
                                                foreach (Point3d P in points[0].Second)
                                                {
                                                    cen += P;
                                                }
                                                cen /= points[0].Second.Count;
                                                #endregion

                                                #region select point != [0]
                                                Point3d second = points[0].Second[1];
                                                if (points[0].Second[0].DistanceTo(second) < UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes())
                                                {
                                                    for (int k = 2; k < points[0].Second.Count; k++)
                                                    {
                                                        if (points[0].Second[0].DistanceTo(points[0].Second[k]) > UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes())
                                                        {
                                                            second = points[0].Second[k];
                                                            break;
                                                        }
                                                    }
                                                }
                                                #endregion

                                                MathLibKCAD.UCS ucs = new MathLibKCAD.UCS(points[0].Second[0], second, cen);
                                                double[] mat = ucs.GetAutoCAD_Matrix3d();
                                                // Matrix3d acUCS = new Matrix3d(mat);

                                                #region create polyline
                                                Polyline pl = new Polyline();
                                                acBlkTblRec.AppendEntity(pl);
                                                trans.AddNewlyCreatedDBObject(pl, true);

                                                points[0].Second.Add(points[0].Second[0]);
                                                for (int i = 0; i < points[0].Second.Count; i++)
                                                {
                                                    MathLibKCAD.quaternion q = ucs.FromACS(points[0].Second[i]);
                                                    pl.AddVertexAt(i, new Point2d(q.GetX(), q.GetY()), 0, 0, 0);
                                                }
                                                #endregion

                                                pl.TransformBy(Matrix3d.Displacement(pl.GeometricExtents.MinPoint.GetVectorTo(Point3d.Origin)));

                                                #endregion

                                                trans.Commit();
                                                //ed.UpdateScreen();
                                            }
                                        }
                                        else
                                        {
                                            using (Transaction trans = acDbNewDoc.TransactionManager.StartTransaction())
                                            {
                                                BlockTable acBlkTbl = trans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                List<UtilityClasses.Pair<int, Point3dCollection>> rez = UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_ExA(ref container, POL, pDoubleRes.Value, trans, acBlkTblRec);
                                                UtilityClasses.GlobalFunctions.ArrangeTrianglesByAdjoining(ref container, POL, ref rez, trans, acBlkTblRec);

                                                trans.Commit();
                                                //ed.UpdateScreen();
                                            }
                                        }//
                                        #endregion

                                        Object acadObject = MgdAcApplication.AcadApplication;
                                        acadObject.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject, null);

                                        using (Transaction tr = acDbNewDoc.TransactionManager.StartTransaction())
                                        {
                                            LayerTable lt = (LayerTable)tr.GetObject(acDbNewDoc.LayerTableId, OpenMode.ForRead);
                                            if (!lt.Has(UtilityClasses.ConstantsAndSettings.BendsLayer))
                                            {
                                                LayerTableRecord ltr = new LayerTableRecord();
                                                ltr.Name = UtilityClasses.ConstantsAndSettings.BendsLayer;
                                                ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 0);
                                                lt.UpgradeOpen();
                                                ObjectId ltId = lt.Add(ltr);
                                                tr.AddNewlyCreatedDBObject(ltr, true);
                                                //acDbNewDoc.Clayer = ltId;
                                            }
                                            if (!lt.Has(UtilityClasses.ConstantsAndSettings.FictivebendsLayer))
                                            {
                                                LayerTableRecord ltr = new LayerTableRecord();
                                                ltr.Name = UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                                                lt.UpgradeOpen();
                                                ObjectId ltId = lt.Add(ltr);
                                                tr.AddNewlyCreatedDBObject(ltr, true);
                                                //acDbNewDoc.Clayer = ltId;
                                            }

                                            tr.Commit();
                                            //ed.UpdateScreen();
                                        }

                                        #region in pletce
                                        using (Transaction trans = acDbNewDoc.TransactionManager.StartTransaction())
                                        {
                                            BlockTable acBlkTbl = trans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                            #region solid
                                            Solid3d solid = UtilityClasses.GlobalFunctions.GetPolygonGlassByheight(ref container, POL, UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass
                                                , trans, acBlkTblRec);
                                            acBlkTblRec.AppendEntity(solid);
                                            trans.AddNewlyCreatedDBObject(solid, true);
                                            #endregion

                                            #region bends
                                            foreach (int N in POL.Triangles_Numers_Array)
                                            {
                                                WorkClasses.Triangle tr = container.Triangles[N];
                                                WorkClasses.Bend bend1 = container.Bends[tr.GetFirstBendNumer()];
                                                WorkClasses.Bend bend2 = container.Bends[tr.GetSecondBendNumer()];
                                                WorkClasses.Bend bend3 = container.Bends[tr.GetThirdBendNumer()];

                                                Line bL1 = new Line((Point3d)bend1.Start, (Point3d)bend1.End);
                                                if (bend1.IsFictive())
                                                    bL1.Layer = UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                else
                                                    bL1.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;

                                                Line bL2 = new Line((Point3d)bend2.Start, (Point3d)bend2.End);
                                                if (bend2.IsFictive())
                                                    bL2.Layer = UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                else
                                                    bL2.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;

                                                Line bL3 = new Line((Point3d)bend3.Start, (Point3d)bend3.End);
                                                if (bend3.IsFictive())
                                                    bL3.Layer = UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                else
                                                    bL3.Layer = UtilityClasses.ConstantsAndSettings.BendsLayer;

                                                acBlkTblRec.AppendEntity(bL1);
                                                trans.AddNewlyCreatedDBObject(bL1, true);

                                                acBlkTblRec.AppendEntity(bL2);
                                                trans.AddNewlyCreatedDBObject(bL2, true);

                                                acBlkTblRec.AppendEntity(bL3);
                                                trans.AddNewlyCreatedDBObject(bL3, true);

                                            }
                                            #endregion

                                            trans.Commit();
                                            //ed.UpdateScreen();
                                        }
                                        #endregion
                                    }
                                    //----------------------------------------------------------------------------
                                    acNewDoc.CloseAndSave(Path + "\\" + Prefix + "_" + (kk+1).ToString() + "_" + Sufix);
                                }

                            }
                        }
                    }
                }
            }
        }
        */
        #endregion
        public void KojtoCAD_3D_GlASS_UNFOLDS_BY_LEVEL_Triangles(int tNumer = -1)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

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
                                    global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = container.Triangles[kk];
                                    if ((tNumer >= 0) && (tNumer != TR.Numer))
                                        continue;

                                    bool b1 = container.Bends[TR.GetFirstBendNumer()].IsFictive();
                                    bool b2 = container.Bends[TR.GetSecondBendNumer()].IsFictive();
                                    bool b3 = container.Bends[TR.GetThirdBendNumer()].IsFictive();
                                    if (!b1 && !b2 && !b3)
                                    {
                                        //Document acNewDoc = acDocMgr.Add(templateName);//**#7
                                        var acNewDoc = new Utilities.UtilityClass().ReadDocument(templateName);
                                        Database acDbNewDoc = acNewDoc.Database;
                                        acDocMgr.MdiActiveDocument = acNewDoc;
                                        using (DocumentLock acLckDoc = acNewDoc.LockDocument())
                                        {
                                            Triplet<quaternion, quaternion, quaternion> pt = container.GetInnererTriangle(TR);
                                            using (Transaction trans = acDbNewDoc.TransactionManager.StartTransaction())
                                            {
                                                BlockTable acBlkTbl = trans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                #region layers
                                                LayerTable lt = (LayerTable)trans.GetObject(acDbNewDoc.LayerTableId, OpenMode.ForRead);
                                                if (!lt.Has(global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer))
                                                {
                                                    LayerTableRecord ltr = new LayerTableRecord();
                                                    ltr.Name = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer;
                                                    ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 0);
                                                    lt.UpgradeOpen();
                                                    ObjectId ltId = lt.Add(ltr);
                                                    trans.AddNewlyCreatedDBObject(ltr, true);
                                                    //acDbNewDoc.Clayer = ltId;
                                                }
                                                if (!lt.Has(global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer))
                                                {
                                                    LayerTableRecord ltr = new LayerTableRecord();
                                                    ltr.Name = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                    ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                                                    lt.UpgradeOpen();
                                                    ObjectId ltId = lt.Add(ltr);
                                                    trans.AddNewlyCreatedDBObject(ltr, true);
                                                    //acDbNewDoc.Clayer = ltId;
                                                }
                                                #endregion

                                                if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                                                {
                                                    #region polylines
                                                    Point3dCollection listPoints = new Point3dCollection();
                                                    listPoints.Add((Point3d)pt.First);
                                                    listPoints.Add((Point3d)pt.Second);
                                                    listPoints.Add((Point3d)pt.Third);

                                                    //-----------------------------------
                                                    Point3dCollection listPoints1 = new Point3dCollection();
                                                    listPoints1.Add((Point3d)TR.Nodes.First);
                                                    listPoints1.Add((Point3d)TR.Nodes.Second);
                                                    listPoints1.Add((Point3d)TR.Nodes.Third);
                                                    if (listPoints1[0].DistanceTo(listPoints[0]) > listPoints1[1].DistanceTo(listPoints[0]))
                                                    {
                                                        Point3d buff = listPoints1[0];
                                                        listPoints1[0] = listPoints1[1];
                                                        listPoints1[1] = buff;
                                                    }
                                                    if (listPoints1[0].DistanceTo(listPoints[0]) > listPoints1[2].DistanceTo(listPoints[0]))
                                                    {
                                                        Point3d buff = listPoints1[0];
                                                        listPoints1[0] = listPoints1[2];
                                                        listPoints1[2] = buff;
                                                    }
                                                    if (listPoints1[1].DistanceTo(listPoints[1]) > listPoints1[2].DistanceTo(listPoints[1]))
                                                    {
                                                        Point3d buff = listPoints1[1];
                                                        listPoints1[1] = listPoints1[2];
                                                        listPoints1[2] = buff;
                                                    }
                                                    //-----------------------------------

                                                    UCS ucs = new UCS();
                                                    Matrix3d ucsMatrix = new Matrix3d();

                                                    Polyline pl = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPoly(listPoints, ref ucs, ref  ucsMatrix, true);
                                                    acBlkTblRec.AppendEntity(pl);
                                                    trans.AddNewlyCreatedDBObject(pl, true);

                                                    pl.TransformBy(ucsMatrix.Inverse());

                                                    //--------------------------------------------------
                                                    Line l1 = new Line(listPoints1[0], listPoints1[1]);
                                                    acBlkTblRec.AppendEntity(l1);
                                                    trans.AddNewlyCreatedDBObject(l1, true);

                                                    Line l2 = new Line(listPoints1[0], listPoints1[2]);
                                                    acBlkTblRec.AppendEntity(l2);
                                                    trans.AddNewlyCreatedDBObject(l2, true);

                                                    Line l3 = new Line(listPoints1[1], listPoints1[2]);
                                                    acBlkTblRec.AppendEntity(l3);
                                                    trans.AddNewlyCreatedDBObject(l3, true);

                                                    l1.TransformBy(ucsMatrix.Inverse());
                                                    l2.TransformBy(ucsMatrix.Inverse());
                                                    l3.TransformBy(ucsMatrix.Inverse());
                                                    //------------------------------------------

                                                    Object acadObject = Application.AcadApplication;
                                                    acadObject.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject, null);

                                                    Triplet<quaternion, quaternion, quaternion> pt0 = TR.Nodes;
                                                    Point3dCollection listPoints0 = new Point3dCollection();
                                                    listPoints0.Add((Point3d)pt0.First);
                                                    listPoints0.Add((Point3d)pt0.Second);
                                                    listPoints0.Add((Point3d)pt0.Third);
                                                    UCS ucs0 = new UCS();
                                                    Matrix3d ucsMatrix0 = new Matrix3d();
                                                    Polyline pl0 = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPoly(listPoints0, ref ucs0, ref  ucsMatrix0, true);
                                                    acBlkTblRec.AppendEntity(pl0);
                                                    trans.AddNewlyCreatedDBObject(pl0, true);
                                                    #endregion


                                                    #region 3d poly
                                                    Triplet<quaternion, quaternion, quaternion> ptt = pt;// container.GetInnererTriangle(TR);
                                                    Polyline3d acPoly3d = new Polyline3d();
                                                    acPoly3d.SetDatabaseDefaults();
                                                    acBlkTblRec.AppendEntity(acPoly3d);
                                                    trans.AddNewlyCreatedDBObject(acPoly3d, true);

                                                    PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)ptt.First);
                                                    acPoly3d.AppendVertex(acPolVer3d);
                                                    trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                                    PolylineVertex3d acPolVer3d1 = new PolylineVertex3d((Point3d)ptt.Second);
                                                    acPoly3d.AppendVertex(acPolVer3d1);
                                                    trans.AddNewlyCreatedDBObject(acPolVer3d1, true);

                                                    PolylineVertex3d acPolVer3d2 = new PolylineVertex3d((Point3d)ptt.Third);
                                                    acPoly3d.AppendVertex(acPolVer3d2);
                                                    trans.AddNewlyCreatedDBObject(acPolVer3d2, true);


                                                    PolylineVertex3d acPolVer3d3 = new PolylineVertex3d((Point3d)ptt.First);
                                                    acPoly3d.AppendVertex(acPolVer3d3);
                                                    trans.AddNewlyCreatedDBObject(acPolVer3d3, true);
                                                    #endregion

                                                    quaternion vec = TR.Normal.Second - TR.Normal.First;
                                                    vec /= vec.abs(); vec *= global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass;

                                                    #region solid
                                                    try
                                                    {
                                                        Solid3d acSol3D = new Solid3d();
                                                        acSol3D.SetDatabaseDefaults();
                                                        acSol3D.CreateExtrudedSolid(acPoly3d, new Vector3d(vec.GetX(), vec.GetY(), vec.GetZ()), new SweepOptions());
                                                        acBlkTblRec.AppendEntity(acSol3D);
                                                        trans.AddNewlyCreatedDBObject(acSol3D, true);

                                                        acPoly3d.Erase();
                                                    }
                                                    catch
                                                    {
                                                        acPoly3d.Erase();
                                                    }
                                                    #endregion
                                                }
                                                trans.Commit();
                                                // ed.UpdateScreen();
                                            }
                                        }
                                        //----------------------------------------------------------------------------
                                        var savePath = Path + "\\" + Prefix + "_" + (kk + 1).ToString() + "_" + Sufix;
                                        //acNewDoc.CloseAndSave(savePath);
                                        new Utilities.UtilityClass().CloseAndSaveDocument(acNewDoc, savePath);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    MessageBox.Show("\nTemplate File Name (extension) error !", "Template E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }//
        }
        public void KojtoCAD_3D_GlASS_UNFOLDS_BY_LEVEL_Polygons()
        {
            if ((container != null) && (container.Polygons.Count > 0) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {

                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                string sLocalRoot = Application.GetSystemVariable("LOCALROOTPREFIX") as string;
                string sTemplatePath = sLocalRoot + "Template\\acad.dwt";

                System.Windows.Forms.OpenFileDialog dlgT = new System.Windows.Forms.OpenFileDialog();
                dlgT.Filter = "DWT or DWG or DXF|*.dwt;*.dwg;dxf|All Files|*.*";
                dlgT.Multiselect = false;
                dlgT.Title = "Select DWT File ";
                dlgT.DefaultExt = "dwt";
                dlgT.InitialDirectory = sLocalRoot + "Template\\";
                dlgT.FileName = "*.dwt";
                if (dlgT.ShowDialog() == DialogResult.OK)
                {
                    string templateName = dlgT.FileName;

                    string dwt = ".dwt";
                    if (templateName.ToUpper().IndexOf(dwt.ToUpper()) >= 0)
                    {

                        FolderBrowserDialog dlg = new FolderBrowserDialog();
                        dlg.Description = "Select Destination Folder. \nDirectory that you want to use as the default for Results.";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            string Path = dlg.SelectedPath;

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

                                    PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                                    pDoubleOpts_.Message = "\n Enter offset(height) from triangle Plane: ";
                                    pDoubleOpts_.DefaultValue = 0.0;
                                    pDoubleOpts_.AllowZero = true;
                                    pDoubleOpts_.AllowNegative = false;
                                    PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                                    if (pDoubleRes_.Status == PromptStatus.OK)
                                    {
                                        double height = pDoubleRes_.Value;
                                        if (Math.Abs(height) < 0.005) height = 0.005;

                                        bool bDraw = false;

                                        PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                                        pKeyOpts.Message = "\nDraw trimed Glass ? ";
                                        pKeyOpts.Keywords.Add("Yes");
                                        pKeyOpts.Keywords.Add("No");
                                        pKeyOpts.Keywords.Default = "No";
                                        pKeyOpts.AllowNone = true;

                                        PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                                        if (pKeyRes.Status == PromptStatus.OK)
                                        {
                                            switch (pKeyRes.StringResult)
                                            {
                                                case "Yes": bDraw = true; break;
                                                case "No": bDraw = false; break;
                                            }
                                        }

                                        foreach (global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon PO in container.Polygons)
                                        {
                                            DocumentCollection acDocMgr = Application.DocumentManager;
                                            //Document acNewDoc = acDocMgr.Add(templateName);//**#6
                                            var acNewDoc = new Utilities.UtilityClass().ReadDocument(templateName);
                                            Database acDbNewDoc = acNewDoc.Database;
                                            acDocMgr.MdiActiveDocument = acNewDoc;
                                            using (DocumentLock acLckDoc = acNewDoc.LockDocument())
                                            {
                                                //----
                                                List<int> buffTR = new List<int>();
                                                List<int> buffBends = new List<int>();
                                                separate_poligon_glass_work_base(PO.GetNumer(), ref buffBends, ref buffTR, templateName, Path, Prefix, Sufix, height, bDraw);
                                                //-----    

                                                Object acadObject = Application.AcadApplication;
                                                acadObject.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject, null);

                                                #region layers
                                                using (Transaction tr = acDbNewDoc.TransactionManager.StartTransaction())
                                                {
                                                    LayerTable lt = (LayerTable)tr.GetObject(acDbNewDoc.LayerTableId, OpenMode.ForRead);
                                                    if (!lt.Has(global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer))
                                                    {
                                                        LayerTableRecord ltr = new LayerTableRecord();
                                                        ltr.Name = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer;
                                                        ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 0);
                                                        lt.UpgradeOpen();
                                                        ObjectId ltId = lt.Add(ltr);
                                                        tr.AddNewlyCreatedDBObject(ltr, true);
                                                        //acDbNewDoc.Clayer = ltId;
                                                    }
                                                    if (!lt.Has(global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer))
                                                    {
                                                        LayerTableRecord ltr = new LayerTableRecord();
                                                        ltr.Name = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                        ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                                                        lt.UpgradeOpen();
                                                        ObjectId ltId = lt.Add(ltr);
                                                        tr.AddNewlyCreatedDBObject(ltr, true);
                                                        //acDbNewDoc.Clayer = ltId;
                                                    }

                                                    tr.Commit();
                                                    //ed.UpdateScreen();
                                                }
                                                #endregion

                                                #region in pletce
                                                global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL = PO;
                                                using (Transaction trans = acDbNewDoc.TransactionManager.StartTransaction())
                                                {
                                                    BlockTable acBlkTbl = trans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                                    BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                    #region solid
                                                    Solid3d solid = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight(ref container, POL, global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass
                                                        , trans, acBlkTblRec);
                                                    try
                                                    {
                                                        acBlkTblRec.AppendEntity(solid);
                                                        trans.AddNewlyCreatedDBObject(solid, true);
                                                    }
                                                    catch { }
                                                    #endregion

                                                    #region bends
                                                    foreach (int N in POL.Triangles_Numers_Array)
                                                    {
                                                        global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle tr = container.Triangles[N];
                                                        global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend1 = container.Bends[tr.GetFirstBendNumer()];
                                                        global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend2 = container.Bends[tr.GetSecondBendNumer()];
                                                        global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend3 = container.Bends[tr.GetThirdBendNumer()];

                                                        Line bL1 = new Line((Point3d)bend1.Start, (Point3d)bend1.End);
                                                        if (bend1.IsFictive())
                                                            bL1.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                        else
                                                            bL1.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer;

                                                        Line bL2 = new Line((Point3d)bend2.Start, (Point3d)bend2.End);
                                                        if (bend2.IsFictive())
                                                            bL2.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                        else
                                                            bL2.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer;

                                                        Line bL3 = new Line((Point3d)bend3.Start, (Point3d)bend3.End);
                                                        if (bend3.IsFictive())
                                                            bL3.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                        else
                                                            bL3.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer;


                                                        acBlkTblRec.AppendEntity(bL1);
                                                        trans.AddNewlyCreatedDBObject(bL1, true);

                                                        acBlkTblRec.AppendEntity(bL2);
                                                        trans.AddNewlyCreatedDBObject(bL2, true);

                                                        acBlkTblRec.AppendEntity(bL3);
                                                        trans.AddNewlyCreatedDBObject(bL3, true);

                                                    }
                                                    #endregion

                                                    trans.Commit();
                                                    //ed.UpdateScreen();
                                                }
                                                #endregion
                                            }
                                            var savePath = Path + "\\" + Prefix + "_" + (PO.GetNumer() + 1).ToString() +
                                                           "_T_" + Sufix;
                                            //acNewDoc.CloseAndSave();
                                            new Utilities.UtilityClass().CloseAndSaveDocument(acNewDoc, savePath);
                                        }
                                    }
                                }//
                            }
                        }
                    }
                    else
                        MessageBox.Show("\nTemplate File Name (extension) error !", "Template E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }//

            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_GlASS_TRIANGLE_UNFOLDS_BY_LEVEL", null, CommandFlags.Session, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SEPARATE_TRIANGLE_UNFOLDS_BY_LEVEL.htm", "")]
        public void KojtoCAD_3D_GlASS_UNFOLDS_BY_LEVEL_Triangle_ByNumer()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Polygons.Count > 0) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                    pIntOpts.Message = "\nEnter the Triangle Numer ";

                    pIntOpts.AllowZero = false;
                    pIntOpts.AllowNegative = false;

                    PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                    if (pIntRes.Status == PromptStatus.OK)
                    {
                        if (pIntRes.Value > 0)
                        {
                            KojtoCAD_3D_GlASS_UNFOLDS_BY_LEVEL_Triangles(pIntRes.Value - 1);
                        }
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_GlASS_POLYGON_UNFOLDS_BY_LEVEL", null, CommandFlags.Session, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SEPARATE_POLYGON_UNFOLDS_BY_LEVEL.htm", "")]
        public void KojtoCAD_3D_GlASS_UNFOLDS_BY_LEVEL_Polygons_ByNumer()
        {
            
            if ((container != null) && (container.Polygons.Count > 0) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                if (global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Single_or_Double_Glass == 1)
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
                            if (pIntRes.Value > 0)
                            {
                                string sLocalRoot = Application.GetSystemVariable("LOCALROOTPREFIX") as string;
                                string sTemplatePath = sLocalRoot + "Template\\acad.dwt";

                                System.Windows.Forms.OpenFileDialog dlgT = new System.Windows.Forms.OpenFileDialog();
                                dlgT.Filter = "DWT or DWG or DXF|*.dwt;*.dwg;dxf|All Files|*.*";
                                dlgT.Multiselect = false;
                                dlgT.Title = "Select DWT File ";
                                dlgT.DefaultExt = "dwt";
                                dlgT.InitialDirectory = sLocalRoot + "Template\\";
                                dlgT.FileName = "*.dwt";
                                if (dlgT.ShowDialog() == DialogResult.OK)
                                {
                                    string templateName = dlgT.FileName;

                                    string dwt = ".dwt";
                                    if (templateName.ToUpper().IndexOf(dwt.ToUpper()) >= 0)
                                    {

                                        FolderBrowserDialog dlg = new FolderBrowserDialog();
                                        dlg.Description = "Select Destination Folder. \nDirectory that you want to use as the default for Results.";
                                        if (dlg.ShowDialog() == DialogResult.OK)
                                        {
                                            string Path = dlg.SelectedPath;

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

                                                    PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                                                    pDoubleOpts_.Message = "\n Enter offset(height) from triangle Plane: ";
                                                    pDoubleOpts_.DefaultValue = 0.0;
                                                    pDoubleOpts_.AllowZero = true;
                                                    pDoubleOpts_.AllowNegative = false;
                                                    PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                                                    if (pDoubleRes_.Status == PromptStatus.OK)
                                                    {
                                                        double height = pDoubleRes_.Value;
                                                        if (Math.Abs(height) < 0.005) height = 0.005;

                                                        bool bDraw = false;

                                                        DocumentCollection acDocMgr = Application.DocumentManager;
                                                        //MessageBox.Show(templateName);
                                                        //Document acNewDoc = acDocMgr.Add(templateName);//**#5
                                                        var acNewDoc = new Utilities.UtilityClass().ReadDocument(templateName);
                                                        Database acDbNewDoc = acNewDoc.Database;
                                                        acDocMgr.MdiActiveDocument = acNewDoc;
                                                        using (DocumentLock acLckDoc = acNewDoc.LockDocument())
                                                        {
                                                            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                                                            pKeyOpts.Message = "\nDraw trimed Glass ? ";
                                                            pKeyOpts.Keywords.Add("Yes");
                                                            pKeyOpts.Keywords.Add("No");
                                                            pKeyOpts.Keywords.Default = "No";
                                                            pKeyOpts.AllowNone = true;

                                                            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                                                            if (pKeyRes.Status == PromptStatus.OK)
                                                            {
                                                                switch (pKeyRes.StringResult)
                                                                {
                                                                    case "Yes": bDraw = true; break;
                                                                    case "No": bDraw = false; break;
                                                                }
                                                            }

                                                            //----
                                                            List<int> buffTR = new List<int>();
                                                            List<int> buffBends = new List<int>();
                                                            separate_poligon_glass_work_base(pIntRes.Value - 1, ref buffBends, ref buffTR, templateName, Path, Prefix, Sufix, height, bDraw);
                                                            //-----    

                                                            Object acadObject = Application.AcadApplication;
                                                            acadObject.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject, null);

                                                            #region layers
                                                            using (Transaction tr = acDbNewDoc.TransactionManager.StartTransaction())
                                                            {
                                                                LayerTable lt = (LayerTable)tr.GetObject(acDbNewDoc.LayerTableId, OpenMode.ForRead);
                                                                if (!lt.Has(global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer))
                                                                {
                                                                    LayerTableRecord ltr = new LayerTableRecord();
                                                                    ltr.Name = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer;
                                                                    ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 0);
                                                                    lt.UpgradeOpen();
                                                                    ObjectId ltId = lt.Add(ltr);
                                                                    tr.AddNewlyCreatedDBObject(ltr, true);
                                                                    //acDbNewDoc.Clayer = ltId;
                                                                }
                                                                if (!lt.Has(global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer))
                                                                {
                                                                    LayerTableRecord ltr = new LayerTableRecord();
                                                                    ltr.Name = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                                    ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                                                                    lt.UpgradeOpen();
                                                                    ObjectId ltId = lt.Add(ltr);
                                                                    tr.AddNewlyCreatedDBObject(ltr, true);
                                                                    //acDbNewDoc.Clayer = ltId;
                                                                }

                                                                tr.Commit();
                                                                //ed.UpdateScreen();
                                                            }
                                                            #endregion

                                                            #region in pletce
                                                            global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL = container.Polygons[pIntRes.Value - 1];
                                                            using (Transaction trans = acDbNewDoc.TransactionManager.StartTransaction())
                                                            {
                                                                BlockTable acBlkTbl = trans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                                                                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                                #region solid
                                                                Solid3d solid = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight(ref container, POL, global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass
                                                                    , trans, acBlkTblRec);
                                                                try
                                                                {
                                                                    acBlkTblRec.AppendEntity(solid);
                                                                    trans.AddNewlyCreatedDBObject(solid, true);
                                                                }
                                                                catch { }
                                                                #endregion

                                                                #region bends
                                                                foreach (int N in POL.Triangles_Numers_Array)
                                                                {
                                                                    global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle tr = container.Triangles[N];
                                                                    global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend1 = container.Bends[tr.GetFirstBendNumer()];
                                                                    global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend2 = container.Bends[tr.GetSecondBendNumer()];
                                                                    global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend3 = container.Bends[tr.GetThirdBendNumer()];

                                                                    Line bL1 = new Line((Point3d)bend1.Start, (Point3d)bend1.End);
                                                                    if (bend1.IsFictive())
                                                                        bL1.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                                    else
                                                                        bL1.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer;

                                                                    Line bL2 = new Line((Point3d)bend2.Start, (Point3d)bend2.End);
                                                                    if (bend2.IsFictive())
                                                                        bL2.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                                    else
                                                                        bL2.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer;

                                                                    Line bL3 = new Line((Point3d)bend3.Start, (Point3d)bend3.End);
                                                                    if (bend3.IsFictive())
                                                                        bL3.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.FictivebendsLayer;
                                                                    else
                                                                        bL3.Layer = global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.BendsLayer;

                                                                    acBlkTblRec.AppendEntity(bL1);
                                                                    trans.AddNewlyCreatedDBObject(bL1, true);

                                                                    acBlkTblRec.AppendEntity(bL2);
                                                                    trans.AddNewlyCreatedDBObject(bL2, true);

                                                                    acBlkTblRec.AppendEntity(bL3);
                                                                    trans.AddNewlyCreatedDBObject(bL3, true);

                                                                }
                                                                #endregion

                                                                trans.Commit();
                                                                //ed.UpdateScreen();
                                                            }
                                                            #endregion


                                                        }
                                                        var savePath = Path + "\\" + Prefix + "_" +
                                                                       (pIntRes.Value).ToString() + "_T_" + Sufix;
                                                        //acNewDoc.CloseAndSave();
                                                        new Utilities.UtilityClass().CloseAndSaveDocument(acNewDoc, savePath);
                                                    }
                                                }//
                                            }
                                        }
                                    }
                                    else
                                        MessageBox.Show("\nTemplate File Name (extension) error !", "Template E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }//
                            }
                        }
                    }
                    catch
                    {
                        string mess = "Try changing 3D machine (Command: KCAD_CHANGE_3D_MASHINE)\nor\nTry reducing Extrude Ratio (Command: KCAD_SET_CUT_SOLID_ER)";
                        mess += "\n\nTo localize the problem first start command (for single glass): KCAD_SHOW_GLASS";
                        MessageBox.Show(mess, "Modeling error !");
                    }
                    finally { ed.CurrentUserCoordinateSystem = old; }
                }
                else
                    MessageBox.Show("\nSettings\\Glass\\Single is off - E R R O R  !\n\nSelect\nKojtoCAD_3D\\DOUBLE GLASS\\.....", "Settings E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void separate_poligon_glass_work_base(int pN /*polygon numer*/, ref List<int> buff, ref List<int> buffTR,
            string templateName, string Path, string Prefix, string Sufix, double height, bool bDraw = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            bool planar = true;
            List<Pair<int, Point3dCollection>> rez = new List<Pair<int, Point3dCollection>>();
            if (!container.Polygons[pN].IsPlanar(ref container.Triangles))
            {
                planar = false;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    bool bHeight = false;
                    do
                    {
                        rez = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_ExA(ref container, container.Polygons[pN], height, trans, acBlkTblRec);
                        if (rez.Count == 0)
                        {
                            height += 0.005;
                            bHeight = true;
                        }
                    } while (rez.Count == 0);
                    if (bHeight)
                        MessageBox.Show(string.Format("Polygon {0}\n\nComputing Height = {1}", pN + 1, height), "A L E R T", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    trans.Commit();
                    //ed.UpdateScreen();
                }
            }

            global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon PO = container.Polygons[pN];
            int index = PO.Triangles_Numers_Array[0];
            global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = container.Triangles[index];
            buffTR.Add(TR.Numer);

            quaternion centroid = TR.Normal.First;

            global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend1 = container.Bends[TR.GetFirstBendNumer()];
            global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend2 = container.Bends[TR.GetSecondBendNumer()];
            global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend3 = container.Bends[TR.GetThirdBendNumer()];

            UCS ucs = TR.GetUcsByBends1(ref container.Bends);
            centroid = ucs.FromACS(centroid);

            global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.DrawLine((Point3d)ucs.FromACS(bend1.Start), (Point3d)ucs.FromACS(bend1.End));
            buff.Add(bend1.Numer);
            global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.DrawLine((Point3d)ucs.FromACS(bend2.Start), (Point3d)ucs.FromACS(bend2.End));
            buff.Add(bend2.Numer);
            global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.DrawLine((Point3d)ucs.FromACS(bend3.Start), (Point3d)ucs.FromACS(bend3.End));
            buff.Add(bend3.Numer);

            UCS ucs1 = new UCS(ucs.FromACS(bend1.Start), ucs.FromACS(bend1.End), centroid);
            UCS ucs2 = new UCS(ucs.FromACS(bend2.Start), ucs.FromACS(bend2.End), centroid);
            UCS ucs3 = new UCS(ucs.FromACS(bend3.Start), ucs.FromACS(bend3.End), centroid);

            if (bDraw)
                foreach (global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Point3dCollection> item in rez)
                {
                    if (item.First == index)
                    {
                        for (int i = 1; i < item.Second.Count; i++)
                        {
                            global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.DrawLine((Point3d)ucs.FromACS(item.Second[i]), (Point3d)ucs.FromACS(item.Second[i - 1]), 2);
                        }
                        global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.DrawLine((Point3d)ucs.FromACS(item.Second[0]), (Point3d)ucs.FromACS(item.Second[item.Second.Count - 1]), 2);


                        break;
                    }
                }

            separate_poligon_glass_work_next(bend1.Numer, TR.Numer, ucs1, ref buff, ref buffTR, PO.Triangles_Numers_Array, ref rez, bDraw);
            separate_poligon_glass_work_next(bend2.Numer, TR.Numer, ucs2, ref buff, ref buffTR, PO.Triangles_Numers_Array, ref rez, bDraw);
            separate_poligon_glass_work_next(bend3.Numer, TR.Numer, ucs3, ref buff, ref buffTR, PO.Triangles_Numers_Array, ref rez, bDraw);

            DocumentCollection acDocMgr = Application.DocumentManager;
            //Document acNewDoc = acDocMgr.Add(templateName);//**#4
            var acNewDoc = new Utilities.UtilityClass().ReadDocument(templateName);
            Database acDbNewDoc = acNewDoc.Database;
            acDocMgr.MdiActiveDocument = acNewDoc;
            using (DocumentLock acLckDoc = acNewDoc.LockDocument())
            {
                #region not Planar
                if (!planar)
                {

                    List<Pair<int, Point3dCollection>> rez1 = new List<Pair<int, Point3dCollection>>();
                    foreach (int N in buffTR)
                    {
                        foreach (global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Point3dCollection> item in rez)
                        {
                            if (item.First == N)
                            {
                                rez1.Add(item);
                                break;
                            }
                        }
                    }

                    Editor edd = Application.DocumentManager.MdiActiveDocument.Editor;
                    using (Transaction trans = acDbNewDoc.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl = trans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.ArrangeTrianglesByAdjoiningU(ref ucs, ref container, container.Polygons[pN], ref rez1, trans, acBlkTblRec);

                        trans.Commit();
                        edd.UpdateScreen();
                    }
                }
                #endregion
                else
                {
                    #region Planar
                    global::KojtoCAD.KojtoCAD3D.WorkClasses.Polygon POL = container.Polygons[pN];
                    if (POL.IsPlanar(ref container.Triangles))
                    {
                        using (Transaction trans = acDbNewDoc.TransactionManager.StartTransaction())
                        {
                            BlockTable acBlkTbl = trans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                            #region planar

                            List<Pair<int, Point3dCollection>> points = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_Ex(ref container,
                                POL, height, trans, acBlkTblRec, false);

                            #region mass center
                            quaternion cen = new quaternion();
                            foreach (Point3d P in points[0].Second)
                            {
                                cen += P;
                            }
                            cen /= points[0].Second.Count;
                            #endregion

                            #region select point != [0]
                            Point3d second = points[0].Second[1];
                            if (points[0].Second[0].DistanceTo(second) < global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes())
                            {
                                for (int k = 2; k < points[0].Second.Count; k++)
                                {
                                    if (points[0].Second[0].DistanceTo(points[0].Second[k]) > global::KojtoCAD.KojtoCAD3D.UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes())
                                    {
                                        second = points[0].Second[k];
                                        break;
                                    }
                                }
                            }
                            #endregion

                            UCS ucss = new UCS(points[0].Second[0], second, cen);
                            double[] mat = ucss.GetAutoCAD_Matrix3d();
                            // Matrix3d acUCS = new Matrix3d(mat);

                            #region create polyline
                            Polyline pl = new Polyline();
                            acBlkTblRec.AppendEntity(pl);
                            trans.AddNewlyCreatedDBObject(pl, true);

                            points[0].Second.Add(points[0].Second[0]);
                            for (int i = 0; i < points[0].Second.Count; i++)
                            {
                                quaternion q = ucss.FromACS(points[0].Second[i]);
                                pl.AddVertexAt(i, new Point2d(q.GetX(), q.GetY()), 0, 0, 0);
                            }
                            #endregion

                            pl.TransformBy(Matrix3d.Displacement(pl.GeometricExtents.MinPoint.GetVectorTo(Point3d.Origin)));

                            #endregion

                            trans.Commit();
                            //ed.UpdateScreen();
                        }
                    }
                    else
                    {
                        using (Transaction trans = acDbNewDoc.TransactionManager.StartTransaction())
                        {
                            BlockTable acBlkTbl = trans.GetObject(acDbNewDoc.BlockTableId, OpenMode.ForWrite) as BlockTable;
                            BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                            List<Pair<int, Point3dCollection>> rezz = global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.GetPolygonGlassByheight_ExA(ref container, POL, height, trans, acBlkTblRec);
                            List<Pair<int, Point3dCollection>> rez1 = new List<Pair<int, Point3dCollection>>();
                            foreach (int N in buffTR)
                            {
                                foreach (global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Point3dCollection> item in rezz)
                                {
                                    if (item.First == N)
                                    {
                                        rez1.Add(item);
                                        break;
                                    }
                                }
                            }
                            global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.ArrangeTrianglesByAdjoining(ref container, POL, ref rez1, trans, acBlkTblRec);

                            trans.Commit();
                            ed.UpdateScreen();
                        }
                    }//
                    #endregion
                }

                Object acadObject = Application.AcadApplication;
                acadObject.GetType().InvokeMember("ZoomExtents", BindingFlags.InvokeMethod, null, acadObject, null);
            }//
            var savePath = Path + "\\" + Prefix + "_" + (pN + 1).ToString() + "_G_" + Sufix;
            //acNewDoc.CloseAndSave();
            new Utilities.UtilityClass().CloseAndSaveDocument(acNewDoc, savePath);
        }
        public void separate_poligon_glass_work_next(int bN, int trN, UCS ucs, ref List<int> buff, ref List<int> buffTR, List<int> trARR,
            ref List<Pair<int, Point3dCollection>> rez, bool bDraw = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ucs = new UCS(ucs.ToACS(new quaternion()), ucs.ToACS(new quaternion(0, 1, 0, 0)), ucs.ToACS(new quaternion(0, 0, -1, 0)));
            Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

            global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend bend = container.Bends[bN];
            int tN = (bend.FirstTriangleNumer == trN) ? bend.SecondTriangleNumer : bend.FirstTriangleNumer;
            if (tN >= 0)
            {
                if ((buffTR.IndexOf(tN) < 0) && (trARR.IndexOf(tN) >= 0))
                {
                    global::KojtoCAD.KojtoCAD3D.WorkClasses.Triangle TR = container.Triangles[tN];
                    buffTR.Add(TR.Numer);

                    List<int> bends = new List<int>();
                    if (TR.GetFirstBendNumer() != bN) { bends.Add(TR.GetFirstBendNumer()); }
                    if (TR.GetSecondBendNumer() != bN) { bends.Add(TR.GetSecondBendNumer()); }
                    if (TR.GetThirdBendNumer() != bN) { bends.Add(TR.GetThirdBendNumer()); }

                    UCS UCS = new UCS(bend.Start, bend.End, TR.Normal.First);
                    foreach (int n in bends)
                    {
                        buff.Add(n);
                        global::KojtoCAD.KojtoCAD3D.WorkClasses.Bend b = container.Bends[n];
                        //UtilityClasses.GlobalFunctions.DrawLine((Point3d)UCS.FromACS(b.Start), (Point3d)UCS.FromACS(b.End), ref mat);
                        global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.DrawLine((Point3d)ucs.ToACS(UCS.FromACS(b.Start)), (Point3d)ucs.ToACS(UCS.FromACS(b.End)));

                        UCS UCSs = new UCS(ucs.ToACS(UCS.FromACS(b.Start)), ucs.ToACS(UCS.FromACS(b.End)), ucs.ToACS(UCS.FromACS(TR.Normal.First)));
                        separate_poligon_glass_work_next(n, TR.Numer, UCSs, ref buff, ref buffTR, trARR, ref rez, bDraw);
                    }

                    if (bDraw)
                        foreach (global::KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<int, Point3dCollection> item in rez)
                        {
                            if (item.First == TR.Numer)
                            {
                                for (int i = 1; i < item.Second.Count; i++)
                                {
                                    global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.DrawLine((Point3d)ucs.ToACS(UCS.FromACS(item.Second[i])), (Point3d)ucs.ToACS(UCS.FromACS(item.Second[i - 1])), 2);
                                }
                                global::KojtoCAD.KojtoCAD3D.UtilityClasses.GlobalFunctions.DrawLine((Point3d)ucs.ToACS(UCS.FromACS(item.Second[0])), (Point3d)ucs.ToACS(UCS.FromACS(item.Second[item.Second.Count - 1])), 2);

                                break;
                            }
                        }
                }
            }
        }
    }
}
