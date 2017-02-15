using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
#endif
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.KojtoGlassDomes))]

namespace KojtoCAD.KojtoCAD3D
{
    public class KojtoGlassDomes
    {
        public Containers container = ContextVariablesProvider.Container;

        private Dictionary<Handle, Pair<Pair<quaternion, quaternion>, Pair<quaternion, quaternion>>> buffDictionary = ContextVariablesProvider.BuffDictionary;

        FormulaEvaluator eval = new FormulaEvaluator();

        #region expolicit Stoil, cherkwa, washington
        const double blockH_1 = 32.3289; //27.3989;
        const double blockH_2 =  27.3989;
        const double additionalFugue = 5.0;
        const double firstCenterDistance = 50.0;
        const double secondCenterDistance = 50.0;//cylinder radius
        const double backToCenterLevel = 4.93;
        const double cylR = 50.8;
        const double len3 = 22.6;
        const double halfLenpl = 73.025;
        const double planka_debelina = 12.7;
        const double otworR = 19.05;
        #endregion

        #region PrototypProfile
        //---- PrototypProfile ---------------------------------
        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_PROFILE_SOLID", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/DRAW_PROFILE_SOLID_BY_CSV_FILE.htm", "")]
        public void KojtoCAD_3D_DrawProfileSolid()
        {
#if !bcad
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
            try
            {
                KojtoCAD_3D_DrawProfile();
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
#else
            MessageBox.Show("Not available for BricsCAD", "Warning", MessageBoxButtons.OK);
#endif
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_DRAW_PROFILE_SOLID_A", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/KCAD_DRAW_PROFILE_SOLID_BY_CSV_A.htm", "")]
        public void KojtoCAD_3D_DrawProfileSolidA()
        {
#if !bcad
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {
                KojtoCAD_3D_DrawProfile(true);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
#else
            MessageBox.Show("Not available for BricsCAD", "Warning", MessageBoxButtons.OK);
#endif
        }


        public void KojtoCAD_3D_DrawProfile(bool erase = false)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
            dlg.Multiselect = false;
            dlg.Title = "Select CSV File ";
            dlg.DefaultExt = "csv";
            if (dlg.ShowDialog() == DialogResult.OK)
            {

                PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                pIntOpts.Message = "\nNumber of Points per Section: ";

                pIntOpts.AllowZero = false;
                pIntOpts.AllowNegative = false;

                PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                if (pIntRes.Status == PromptStatus.OK)
                {
                    int Number_of_Points_per_Section = pIntRes.Value;
                    PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                    pDoubleOpts.Message = "\nEnter Thickness (positive or negative): ";
                    pDoubleOpts.AllowZero = false;
                    pDoubleOpts.AllowNegative = true;
                    PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                    if (pDoubleRes.Status == PromptStatus.OK)
                    {
                        double thickness = pDoubleRes.Value;

                        PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                        pDoubleOpts_.Message = "\nEnter Width of the Joint (positive): ";
                        pDoubleOpts_.AllowZero = false;
                        pDoubleOpts_.AllowNegative = false;
                        PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                        if (pDoubleRes_.Status == PromptStatus.OK)
                        {
                            double jWidth = pDoubleRes_.Value;

                            List<List<Pair<string, quaternion>>> pointsArray = new List<List<Pair<string, quaternion>>>();
                            #region fill array
                            try
                            {
                                char dec = '.';
                                char[] splitChars = new Char[] { ' ', ',', ';', ':', '\t' };
                                #region check decimal symbol and split chars
                                using (StreamReader sr = new StreamReader(dlg.FileNames[0]))
                                {
                                    string line;
                                    line = sr.ReadLine(); line = sr.ReadLine();
                                    if (line.IndexOf('.') < 0)
                                    {
                                        dec = ',';
                                        splitChars = new Char[] { ' ', ';', ':', '\t' };
                                    }
                                    sr.Close();
                                }
                                #endregion

                                List<Pair<string, quaternion>> currentSectionPoints = new List<Pair<string, quaternion>>();
                                using (StreamReader sr = new StreamReader(dlg.FileNames[0]))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        if (line.IndexOf(dec) > 0)
                                        {
                                            string[] split = line.Split(splitChars);

                                            #region chec decimal symbol
                                            if (dec == ',')
                                            {
                                                split[1] = split[1].Replace(',', '.');
                                                split[2] = split[2].Replace(',', '.');
                                                split[3] = split[3].Replace(',', '.');
                                            }
                                            #endregion

                                            double X = double.NaN;
                                            double Y = double.NaN;
                                            double Z = double.NaN;

                                            #region convert
                                            try
                                            {
                                                X = double.Parse(split[1]);
                                                Y = double.Parse(split[2]);
                                                Z = double.Parse(split[3]);
                                            }
                                            catch (FormatException)
                                            {
                                                MessageBox.Show("Unable to convert '{0}' to a Double.", line, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            }
                                            catch (OverflowException)
                                            {
                                                MessageBox.Show("'{0}' is outside the range of a Double.", line, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            }
                                            #endregion

                                            if ((!double.IsNaN(X)) && (!double.IsNaN(Y)) && (!double.IsNaN(Z)))
                                            {
                                                currentSectionPoints.Add(new Pair<string, quaternion>(split[0], new quaternion(0.0, X, Y, Z)));
                                                if (currentSectionPoints.Count == Number_of_Points_per_Section)
                                                {
                                                    pointsArray.Add(currentSectionPoints);
                                                    currentSectionPoints = new List<Pair<string, quaternion>>();
                                                }
                                            }
                                        }
                                    }
                                    sr.Close();
                                }
                            }
                            catch (System.Exception e)
                            {
                                Console.WriteLine("The file could not be read:\n" + e.Message);
                            }
                            #endregion

                            Polyline prePoly = null;
                            Solid3d preCutBox = null;
                            Matrix3d preUCS = new Matrix3d();
                            for (int i = 0; i < pointsArray.Count; i++)
                            {
                                string test = (i + 1).ToString() + "\n\n";
                                List<Pair<string, quaternion>> list = pointsArray[i];
                                Point3dCollection listPoints = new Point3dCollection();
                                for (int j = 0; j < list.Count; j++)
                                    listPoints.Add((Point3d)list[j].Second);

                                using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl;
                                    acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec;
                                    acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    UCS ucs = new UCS();
                                    Matrix3d ucsMatrix = new Matrix3d();

                                    Polyline pl = GlobalFunctions.GetPoly(listPoints, ref ucs, ref  ucsMatrix, false);
                                    acBlkTblRec.AppendEntity(pl);
                                    tr.AddNewlyCreatedDBObject(pl, true);

                                    Matrix3d userUCS = ed.CurrentUserCoordinateSystem;
                                    ed.CurrentUserCoordinateSystem = ucsMatrix;

                                    Polyline pl1 = (pl.GetOffsetCurves(thickness))[0] as Polyline;
                                    acBlkTblRec.AppendEntity(pl1);
                                    tr.AddNewlyCreatedDBObject(pl1, true);

                                    Point3d ps = pl.StartPoint;
                                    Point3d pe = (ps.DistanceTo(pl1.StartPoint) < ps.DistanceTo(pl1.EndPoint)) ? pl1.StartPoint : pl1.EndPoint;

                                    Point3d pS = pl.EndPoint;
                                    Point3d pE = (pS.DistanceTo(pl1.StartPoint) < pS.DistanceTo(pl1.EndPoint)) ? pl1.StartPoint : pl1.EndPoint;

                                    Line L1 = new Line(ps, pe);
                                    acBlkTblRec.AppendEntity(L1);
                                    tr.AddNewlyCreatedDBObject(L1, true);

                                    Line L2 = new Line(pS, pE);
                                    acBlkTblRec.AppendEntity(L2);
                                    tr.AddNewlyCreatedDBObject(L2, true);

                                    pl.JoinEntity((Entity)L1);
                                    pl.JoinEntity((Entity)L2);
                                    pl.JoinEntity((Entity)pl1);

                                    L1.Erase(); L2.Erase(); pl1.Erase();

                                    ed.CurrentUserCoordinateSystem = userUCS;

                                    //--------
                                    if ((object)prePoly != null)
                                    {
                                        Entity[] Curves = new Entity[2];
                                        Curves[0] = prePoly; Curves[1] = pl;

                                        //Number_of_Points_per_Section

                                        Point3dCollection pColl = new Point3dCollection(); Point3dCollection pColl_ = new Point3dCollection();
                                        IntegerCollection iCol1 = new IntegerCollection(); IntegerCollection iCol1_ = new IntegerCollection();
                                        IntegerCollection iCol2 = new IntegerCollection(); IntegerCollection iCol2_ = new IntegerCollection();
                                        pl.GetGripPoints(pColl, iCol1, iCol2);
                                        prePoly.GetGripPoints(pColl_, iCol1_, iCol2_);

                                        Entity[] gCurves = new Entity[pColl.Count];
                                        for (int f = 0; f < pColl.Count; f++)
                                        {
                                            Line li = new Line(pColl[f], pColl_[f]);
                                            acBlkTblRec.AppendEntity(li);
                                            tr.AddNewlyCreatedDBObject(li, true);
                                            gCurves[f] = (Entity)li;
                                        }

                                        LoftOptions lo = new LoftOptions();
                                        Solid3d acSol3D = new Solid3d();
                                        acSol3D.SetDatabaseDefaults();
                                        acSol3D.CreateLoftedSolid(Curves, gCurves, null, lo);
                                        acBlkTblRec.AppendEntity(acSol3D);
                                        tr.AddNewlyCreatedDBObject(acSol3D, true);
                                        for (int f = 0; f < gCurves.Length; f++)
                                        {
                                            gCurves[f].Erase();
                                        }


                                        //jWidth
                                        double xLength = Math.Abs(prePoly.EndPoint.X - prePoly.StartPoint.X) + 100;
                                        double yLength = Math.Abs(prePoly.EndPoint.Y - prePoly.StartPoint.Y) + 100;
                                        double zLength = Math.Abs(prePoly.EndPoint.Z - prePoly.StartPoint.Z) + 100;
                                        double length = Math.Sqrt(xLength * xLength + yLength * yLength + zLength * zLength);
                                        length *= 10.0;

                                        preCutBox = new Solid3d();
                                        preCutBox.CreateBox(length, length, jWidth);

                                        acBlkTblRec.AppendEntity(preCutBox);
                                        tr.AddNewlyCreatedDBObject(preCutBox, true);

                                        Solid3d preCutBox1 = preCutBox.Clone() as Solid3d;
                                        preCutBox.TransformBy(preUCS);
                                        preCutBox1.TransformBy(ucsMatrix);
                                        acSol3D.BooleanOperation(BooleanOperationType.BoolSubtract, preCutBox);
                                        acSol3D.BooleanOperation(BooleanOperationType.BoolSubtract, preCutBox1);
                                    }
                                    //-----------------------------------------------                                

                                    if (erase)
                                        pl.Erase();

                                    prePoly = pl;
                                    preUCS = ucsMatrix;
                                    tr.Commit();
                                    Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
                                }
                            }
                        }
                    }
                }
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_ADD_PROFILE_POINT_TO_FILE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/ADD_PROFILE_POINT_TO_FILE.htm", "")]
        public void KojtoCAD_3D_Add_Profile_Point_To_File()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                FileDialog dlg = new SaveFileDialog();
                dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                dlg.Title = "Select CSV File ";
                dlg.DefaultExt = "csv";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Point Name: ");
                    pStrOpts.AllowSpaces = true;
                    PromptResult pStrRes;
                    do
                    {
                        pStrRes = ed.GetString(pStrOpts);
                        if (pStrRes.Status == PromptStatus.OK)
                        {
                            string pointName = pStrRes.StringResult;

                            PromptPointResult pPtRes;
                            PromptPointOptions pPtOpts = new PromptPointOptions("");
                            pPtOpts.Message = "\nEnter the Point : ";
                            pPtRes = ed.GetPoint(pPtOpts);
                            if (pPtRes.Status == PromptStatus.OK)
                            {
                                string str = string.Format("{0};{1:f5};{2:f5};{3:f5}", pointName, pPtRes.Value.X, pPtRes.Value.Y, pPtRes.Value.Z);
                                FileStream aFile = new FileStream(dlg.FileName, FileMode.OpenOrCreate);
                                using (StreamWriter sw = new StreamWriter(aFile))
                                {
                                    //StreamReader sr = new StreamReader(aFile);
                                    // while (sr.Peek() >= 0)
                                    // {
                                    //    Console.WriteLine(sr.ReadLine());
                                    // }
                                    for (long i = 0; i < aFile.Length; i++)
                                    {
                                        aFile.Seek(-i, SeekOrigin.End);
                                        try
                                        {
                                            char ch = Convert.ToChar(aFile.ReadByte());
                                            if ((Char.IsLetterOrDigit(ch)) || (Char.IsSurrogate(ch)) || (Char.IsPunctuation(ch)))
                                                break;
                                        }
                                        catch { }
                                    }
                                    sw.WriteLine();
                                    sw.WriteLine(str);
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                        }
                    } while (pStrRes.Status == PromptStatus.OK);
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_PREPARE_PROFILE_UNFOLD", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/PREPARE_PROFILE_UNFOLD.htm", "")]
        public void KojtoCAD_3D_Prepare_Profile_Unfold()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                KojtoCAD_3D_DrawProfilePolyLinesAndRips();

            }
            catch { }
            finally
            {
                ed.CurrentUserCoordinateSystem = old;
            }
        }
        public void KojtoCAD_3D_DrawProfilePolyLinesAndRips()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
            dlg.Multiselect = false;
            dlg.Title = "Select CSV File ";
            dlg.DefaultExt = "csv";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                pIntOpts.Message = "\nNumber of Points per Section: ";

                pIntOpts.AllowZero = false;
                pIntOpts.AllowNegative = false;

                PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                if (pIntRes.Status == PromptStatus.OK)
                {
                    int Number_of_Points_per_Section = pIntRes.Value;

                    PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                    pDoubleOpts_.Message = "\nEnter Width of the Joint (positive): ";
                    pDoubleOpts_.AllowZero = false;
                    pDoubleOpts_.AllowNegative = false;
                    PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                    if (pDoubleRes_.Status == PromptStatus.OK)
                    {
                        double jWidth = pDoubleRes_.Value;
                        List<List<Pair<string, quaternion>>> pointsArray = new List<List<Pair<string, quaternion>>>();
                        #region fill array
                        try
                        {
                            char dec = '.';
                            char[] splitChars = new Char[] { ' ', ',', ';', ':', '\t' };
                            #region check decimal symbol and split chars
                            using (StreamReader sr = new StreamReader(dlg.FileNames[0]))
                            {
                                string line;
                                line = sr.ReadLine(); line = sr.ReadLine();
                                if (line.IndexOf('.') < 0)
                                {
                                    dec = ',';
                                    splitChars = new Char[] { ' ', ';', ':', '\t' };
                                }
                                sr.Close();
                            }
                            #endregion

                            List<Pair<string, quaternion>> currentSectionPoints = new List<Pair<string, quaternion>>();
                            using (StreamReader sr = new StreamReader(dlg.FileNames[0]))
                            {
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    if (line.IndexOf(dec) > 0)
                                    {
                                        string[] split = line.Split(splitChars);

                                        #region chec decimal symbol
                                        if (dec == ',')
                                        {
                                            split[1] = split[1].Replace(',', '.');
                                            split[2] = split[2].Replace(',', '.');
                                            split[3] = split[3].Replace(',', '.');
                                        }
                                        #endregion

                                        double X = double.NaN;
                                        double Y = double.NaN;
                                        double Z = double.NaN;

                                        #region convert
                                        try
                                        {
                                            X = double.Parse(split[1]);
                                            Y = double.Parse(split[2]);
                                            Z = double.Parse(split[3]);
                                        }
                                        catch (FormatException)
                                        {
                                            MessageBox.Show("Unable to convert '{0}' to a Double.", line, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        catch (OverflowException)
                                        {
                                            MessageBox.Show("'{0}' is outside the range of a Double.", line, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        #endregion

                                        if ((!double.IsNaN(X)) && (!double.IsNaN(Y)) && (!double.IsNaN(Z)))
                                        {
                                            currentSectionPoints.Add(new Pair<string, quaternion>(split[0], new quaternion(0.0, X, Y, Z)));
                                            if (currentSectionPoints.Count == Number_of_Points_per_Section)
                                            {
                                                pointsArray.Add(currentSectionPoints);
                                                currentSectionPoints = new List<Pair<string, quaternion>>();
                                            }
                                        }
                                    }
                                }
                                sr.Close();
                            }
                        }
                        catch (System.Exception e)
                        {
                            Console.WriteLine("The file could not be read:");
                            Console.WriteLine(e.Message);
                        }
                        #endregion


                        for (int i = 1; i < pointsArray.Count; i++)
                        {
                            List<Pair<string, quaternion>> list_i = pointsArray[i];
                            List<Pair<string, quaternion>> list_i0 = pointsArray[i - 1];
                            Point3dCollection listPoints_i = new Point3dCollection();
                            Point3dCollection listPoints_i0 = new Point3dCollection();
                            for (int j = 0; j < list_i.Count; j++)
                                listPoints_i.Add((Point3d)list_i[j].Second);
                            for (int j = 0; j < list_i0.Count; j++)
                                listPoints_i0.Add((Point3d)list_i0[j].Second);
                            //-------
                            UCS ucs_i = new UCS();
                            Matrix3d ucsMatrix_i = new Matrix3d();
                            Polyline pl_i = GlobalFunctions.GetPoly(listPoints_i, ref ucs_i, ref  ucsMatrix_i, false);

                            UCS ucs_i0 = new UCS();
                            Matrix3d ucsMatrix_i0 = new Matrix3d();
                            Polyline pl_i0 = GlobalFunctions.GetPoly(listPoints_i0, ref ucs_i0, ref  ucsMatrix_i0, false);

                            int k_i = 1;
                            int k_i0 = 1;

                            if (ucs_i.FromACS(ucs_i0.o).GetZ() < 0.0)
                                k_i = -1;

                            if (ucs_i0.FromACS(ucs_i.o).GetZ() < 0.0)
                                k_i0 = -1;

                            plane plane_i = new plane(ucs_i.ToACS(new quaternion(0.0, 0.0, 0.0, k_i * jWidth / 2.0)),
                                ucs_i.ToACS(new quaternion(0.0, 100.0, 0.0, k_i * jWidth / 2.0)),
                                ucs_i.ToACS(new quaternion(0.0, 0.0, 100.0, k_i * jWidth / 2.0)));

                            plane plane_i0 = new plane(ucs_i0.ToACS(new quaternion(0.0, 0.0, 0.0, k_i0 * jWidth / 2.0)),
                                ucs_i0.ToACS(new quaternion(0.0, 100.0, 0.0, k_i0 * jWidth / 2.0)),
                                ucs_i0.ToACS(new quaternion(0.0, 0.0, 100.0, k_i0 * jWidth / 2.0)));

                            Point3dCollection qList_i = new Point3dCollection();
                            Point3dCollection qList_i0 = new Point3dCollection();
                            Point3dCollection qList_mid = new Point3dCollection();
                            //--------------
                            Point3dCollection rez_i_Coll = new Point3dCollection();
                            Point3dCollection rez_i0_Coll = new Point3dCollection();
                            Point3dCollection rez_mid_Coll = new Point3dCollection();

                            for (int j = 0; j < list_i.Count; j++)
                            {
                                Pair<string, quaternion> pa1 = list_i[j];
                                Pair<string, quaternion> pa2 = list_i0[j];
                                quaternion mQ_i = plane_i.IntersectWithVector(pa1.Second, pa2.Second);
                                quaternion mQ_i0 = plane_i0.IntersectWithVector(pa1.Second, pa2.Second);
                                quaternion mid = (mQ_i + mQ_i0) / 2.0;
                                qList_i.Add((Point3d)mQ_i);
                                qList_i0.Add((Point3d)mQ_i0);
                                qList_mid.Add((Point3d)mid);

                                rez_i_Coll.Add((Point3d)mQ_i);
                                rez_i0_Coll.Add((Point3d)mQ_i0);
                                rez_mid_Coll.Add((Point3d)mid);


                                ObjectId id = GlobalFunctions.DrawLine((Point3d)mQ_i, (Point3d)mQ_i0);


                            }

                            GlobalFunctions.GetPoly3d(rez_i_Coll, false);
                            GlobalFunctions.GetPoly3d(rez_i0_Coll, false);
                            GlobalFunctions.GetPoly3d(rez_mid_Coll, false);

                        }
                    }
                }
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_EXPROF", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/Solid_Extrude_from_Block.htm", "")]
        public void Extrude_Profi_Method()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("Left");
                pKeyOpts.Keywords.Add("Right");
                pKeyOpts.Keywords.Default = "Left";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "Left": Extrude_Profi_Method_work(); break;
                        case "Right": Extrude_Profi_Method_work(true); break;
                    }
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }

        }

        [CommandMethod("KojtoCAD_3D", "KCAD_EXTRIM", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/Trim_whith_profile.htm", "")]
        public void Extrude_Profi_Method_trim()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "3DSOLID"), 0);

                List<Entity> solids = GlobalFunctions.GetSelection(ref acTypValAr, "Select Solid3d: ");
                List<ObjectId> Ids = new List<ObjectId>();

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    foreach (Entity ent in solids)
                    {
                        Entity sol1 = tr.GetObject(ent.ObjectId, OpenMode.ForWrite) as Entity;
                        ent.Visible = true;
                    }
                    tr.Commit();
                }

                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("Left");
                pKeyOpts.Keywords.Add("Right");
                pKeyOpts.Keywords.Default = "Left";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "Left": Ids = Extrude_Profi_Method_work(); break;
                        case "Right": Ids = Extrude_Profi_Method_work(true); break;
                    }
                }

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                    // BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (Entity ent in solids)
                    {
                        Solid3d sol1 = tr.GetObject(ent.ObjectId, OpenMode.ForWrite) as Solid3d;
                        foreach (ObjectId id in Ids)
                        {
                            Solid3d sol2 = tr.GetObject(id, OpenMode.ForWrite) as Solid3d;
                            Solid3d sol3 = sol2.Clone() as Solid3d;
                            sol1.BooleanOperation(BooleanOperationType.BoolSubtract, sol3);
                        }
                    }

                    foreach (ObjectId id in Ids)
                    {
                        Solid3d sol = tr.GetObject(id, OpenMode.ForWrite) as Solid3d;
                        sol.Erase();
                    }

                    tr.Commit();
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        public List<ObjectId> Extrude_Profi_Method_work(bool b = false)
        {
            List<ObjectId> rez = new List<ObjectId>();

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            // Prompt for the first point
            pPtOpts.Message = "\nEnter the Start Point of the Line: ";
            pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
            if (pPtRes.Status == PromptStatus.OK)
            {
                Point3d ptFirst = pPtRes.Value;

                pPtOpts.Message = "\nEnter the End Point of the Line: ";
                pPtOpts.UseBasePoint = true;
                pPtOpts.BasePoint = ptFirst;
                pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                if (pPtRes.Status == PromptStatus.OK)
                {
                    Point3d ptSecond = pPtRes.Value;

                    pPtOpts.Message = "\nEnter the Orientation Point: ";
                    pPtOpts.UseBasePoint = true;
                    pPtOpts.BasePoint = ptFirst;
                    pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                    if (pPtRes.Status == PromptStatus.OK)
                    {
                        Point3d ptOr = pPtRes.Value;

                        PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                        pDoubleOpts.Message = "\nEnter the Ends Offset: ";

                        pDoubleOpts.AllowZero = true;
                        pDoubleOpts.AllowNegative = true;

                        PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                        if (pDoubleRes.Status == PromptStatus.OK)
                        {
                            double offset = pDoubleRes.Value;

                            double len = (((quaternion)(ptSecond)) - ((quaternion)(ptFirst))).abs();
                            if ((offset < 0.0) && Math.Abs(offset * 2.0) > len)
                            {
                                MessageBox.Show("Offset error |offset| > distance between points !", "E R R O R");
                                return rez;
                            }
                            len += offset * 2.0;

                            UCS preUCS = new UCS(ptFirst, ptSecond, ptOr);
                            quaternion o = preUCS.ToACS(new quaternion());
                            quaternion x = preUCS.ToACS(new quaternion(0, 0, 100, 0));
                            quaternion y = preUCS.ToACS(new quaternion(0, 0, 0, 100));
                            UCS ucs = new UCS(o, x, y);
                            Matrix3d matUCS = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                            Matrix3d old = ed.CurrentUserCoordinateSystem;

                            BlockSelection form = new BlockSelection();

                            form.ShowDialog();
                            if (form.DialogResult == DialogResult.OK)
                            {
                                string blockName = form.BlockName;

                                try
                                {
                                    using (Transaction tr = db.TransactionManager.StartTransaction())
                                    {
                                        BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                                        //ed.CurrentUserCoordinateSystem = matUCS;

                                        BlockTableRecord btr = tr.GetObject(acBlkTbl[blockName], OpenMode.ForWrite) as BlockTableRecord;
                                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);

                                        if (b)
                                            br.TransformBy(Matrix3d.Mirroring(new Line3d(new Point3d(), new Point3d(0, 100, 0))));

                                        if (offset != 0.0)
                                            br.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -offset)));

                                        DBObjectCollection temp = new DBObjectCollection();
                                        br.Explode(temp);

                                        foreach (DBObject dbo in temp)
                                        {
                                            Entity ent = (Entity)dbo;
                                            acBlkTblRec.AppendEntity(ent);
                                            tr.AddNewlyCreatedDBObject(ent, true);

                                        }

                                        if (temp.Count > 0)
                                        {
                                            foreach (DBObject dbo in temp)
                                            {
                                                string type = dbo.GetType().ToString();
                                                if (type.ToUpper().IndexOf("REGION") >= 0)
                                                {
                                                    Region reg = (Region)dbo;

                                                    try
                                                    {
                                                        //SweepOptionsBuilder sob = new SweepOptionsBuilder();
                                                        // sob.Align = SweepOptionsAlignOption.AlignSweepEntityToPath;

                                                        string layer = reg.Layer;

                                                        Solid3d acSol3D = new Solid3d();
                                                        acSol3D.SetDatabaseDefaults();
                                                        acSol3D.CreateExtrudedSolid(reg, new Vector3d(0, 0, len), new SweepOptions());
                                                        acSol3D.Layer = layer;

                                                        acSol3D.TransformBy(matUCS);

                                                        //acSol3D.TransformBy(trM);
                                                        acBlkTblRec.AppendEntity(acSol3D);
                                                        tr.AddNewlyCreatedDBObject(acSol3D, true);

                                                        rez.Add(acSol3D.ObjectId);
                                                        reg.Erase();
                                                    }
                                                    catch
                                                    {
                                                        //reg.Erase();
                                                    }
                                                }
                                                else
                                                {
                                                    dbo.Erase();
                                                }
                                            }
                                        }
                                        tr.Commit();
                                    }

                                    //-----------------

                                    //----------------


                                }
                                catch (System.Exception ex)
                                {
                                    Debug.WriteLine(ex.ToString());
                                    ed.WriteMessage(ex.ToString());
                                }

                            }
                        }
                    }//
                }
            }
            return rez;
        }

        #endregion

        #region deviation
        [CommandMethod("KojtoCAD_3D", "KCAD_AGT", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/Gaskets_Analize_Distances.htm", "")]//Gaskets Analise Dichtung
        public void KojtoCAD_3D_Analise_Gaskets_Triangles()
        {
            
            Matrix3d old = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
            Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    Gaskets_FORM form = new Gaskets_FORM(ConstantsAndSettings.GasketA,
                    ConstantsAndSettings.GasketB, ConstantsAndSettings.GasketC);

                    form.ShowDialog();
                    if (form.DialogResult == DialogResult.OK)
                    {
                        ConstantsAndSettings.GasketA = form.mA;
                        ConstantsAndSettings.GasketB = form.mB;
                        ConstantsAndSettings.GasketC = form.mC;

                        foreach (Triangle TR in container.Triangles)
                        {
                            Bend bend1 = container.Bends[TR.GetFirstBendNumer()];
                            Bend bend2 = container.Bends[TR.GetSecondBendNumer()];
                            Bend bend3 = container.Bends[TR.GetThirdBendNumer()];

                            if (bend1.IsFictive() || bend2.IsFictive() || bend3.IsFictive()) { continue; }

                            double bendLEN_1 = bend1.Length - 2.0 * form.mA;
                            double bendLEN_2 = bend2.Length - 2.0 * form.mB;
                            double bendLEN_3 = bend3.Length - 2.0 * form.mC;

                            if ((bendLEN_1 <= 0) || (bendLEN_2 <= 0) || (bendLEN_3 <= 0))
                            {
                                MessageBox.Show(string.Format("\n 2.A  > BendLength !\nTriangle {0}", TR.Numer + 1), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                continue;
                            }

                            UCS ucsBend1 = bend1.GetUCS_A();
                            UCS ucsBend2 = bend2.GetUCS_A();
                            UCS ucsBend3 = bend3.GetUCS_A();

                            int sign_ucsBend1 = ((ucsBend1.FromACS(TR.Normal.First)).GetZ() >= 0) ? 1 : (-1);
                            int sign_ucsBend2 = ((ucsBend2.FromACS(TR.Normal.First)).GetZ() >= 0) ? 1 : (-1);
                            int sign_ucsBend3 = ((ucsBend3.FromACS(TR.Normal.First)).GetZ() >= 0) ? 1 : (-1);

                            quaternion p1 = new quaternion(0, bendLEN_1 / 2.0, form.mC, sign_ucsBend1 * form.mB);
                            quaternion p2 = new quaternion(0, -bendLEN_1 / 2.0, form.mC, sign_ucsBend1 * form.mB);
                            Pair<quaternion, quaternion> newBend1 = new Pair<quaternion, quaternion>(ucsBend1.ToACS(p1), ucsBend1.ToACS(p2));

                            p1 = new quaternion(0, bendLEN_2 / 2.0, form.mC, sign_ucsBend2 * form.mB);
                            p2 = new quaternion(0, -bendLEN_2 / 2.0, form.mC, sign_ucsBend2 * form.mB);
                            Pair<quaternion, quaternion> newBend2 = new Pair<quaternion, quaternion>(ucsBend2.ToACS(p1), ucsBend2.ToACS(p2));

                            p1 = new quaternion(0, bendLEN_3 / 2.0, form.mC, sign_ucsBend3 * form.mB);
                            p2 = new quaternion(0, -bendLEN_3 / 2.0, form.mC, sign_ucsBend3 * form.mB);
                            Pair<quaternion, quaternion> newBend3 = new Pair<quaternion, quaternion>(ucsBend3.ToACS(p1), ucsBend3.ToACS(p2));

                            GlobalFunctions.DrawLine((Point3d)newBend1.First, (Point3d)newBend1.Second);
                            GlobalFunctions.DrawLine((Point3d)newBend2.First, (Point3d)newBend2.Second);
                            GlobalFunctions.DrawLine((Point3d)newBend3.First, (Point3d)newBend3.Second);

                            GlobalFunctions.GetCrossingLinesDistance(newBend1.First, newBend1.Second, newBend2.First, newBend2.Second, true);
                            GlobalFunctions.GetCrossingLinesDistance(newBend1.First, newBend1.Second, newBend3.First, newBend3.Second, true);
                            GlobalFunctions.GetCrossingLinesDistance(newBend2.First, newBend2.Second, newBend3.First, newBend3.Second, true);
                        }
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_AGT_DRAW", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/GASKETS_DRAW_DEV.htm", "")]
        public void KojtoCAD_3D_Analise_Gaskets_Triangles_Draw()
        {
            
            Matrix3d old = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
            Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                pDoubleOpts_.Message = "\n Enter Text Offset from Triangle Plane: ";
                pDoubleOpts_.DefaultValue = 0.0;
                pDoubleOpts_.AllowZero = true;
                pDoubleOpts_.AllowNegative = true;

                PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                if (pDoubleRes_.Status == PromptStatus.OK)
                {
                    double offset = pDoubleRes_.Value;

                    if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                    {
                        Gaskets_FORM form = new Gaskets_FORM(ConstantsAndSettings.GasketA,
                            ConstantsAndSettings.GasketB, ConstantsAndSettings.GasketC);

                        form.ShowDialog();
                        if (form.DialogResult == DialogResult.OK)
                        {
                            ConstantsAndSettings.GasketA = form.mA;
                            ConstantsAndSettings.GasketB = form.mB;
                            ConstantsAndSettings.GasketC = form.mC;

                            foreach (Triangle TR in container.Triangles)
                            {
                                Bend bend1 = container.Bends[TR.GetFirstBendNumer()];
                                Bend bend2 = container.Bends[TR.GetSecondBendNumer()];
                                Bend bend3 = container.Bends[TR.GetThirdBendNumer()];

                                if (bend1.IsFictive() || bend2.IsFictive() || bend3.IsFictive()) { continue; }
                                quaternion p = new quaternion(0, TR.Normal.First.GetX(), TR.Normal.First.GetY(), TR.Normal.First.GetZ());

                                double bendLEN_1 = bend1.Length - 2.0 * form.mA;
                                double bendLEN_2 = bend2.Length - 2.0 * form.mB;
                                double bendLEN_3 = bend3.Length - 2.0 * form.mC;

                                if ((bendLEN_1 <= 0) || (bendLEN_2 <= 0) || (bendLEN_3 <= 0))
                                {
                                    MessageBox.Show(string.Format("\n 2.A  > BendLength !\nTriangle {0}", TR.Numer + 1), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    continue;
                                }

                                UCS trUCS = new UCS(TR.Normal.First, bend1.Start, bend1.End);
                                UCS ucsBend1 = new UCS(bend1.MidPoint, bend1.End, bend1.Normal);
                                UCS ucsBend2 = new UCS(bend2.MidPoint, bend2.End, bend2.Normal);
                                UCS ucsBend3 = new UCS(bend3.MidPoint, bend3.End, bend3.Normal);

                                int sign_ucsBend1 = ((ucsBend1.FromACS(TR.Normal.First)).GetZ() >= 0) ? 1 : (-1);
                                int sign_ucsBend2 = ((ucsBend2.FromACS(TR.Normal.First)).GetZ() >= 0) ? 1 : (-1);
                                int sign_ucsBend3 = ((ucsBend3.FromACS(TR.Normal.First)).GetZ() >= 0) ? 1 : (-1);

                                quaternion p1 = new quaternion(0, bendLEN_1 / 2.0, form.mC, sign_ucsBend1 * form.mB);
                                quaternion p2 = new quaternion(0, -bendLEN_1 / 2.0, form.mC, sign_ucsBend1 * form.mB);
                                Pair<quaternion, quaternion> newBend1 = new Pair<quaternion, quaternion>(trUCS.FromACS(ucsBend1.ToACS(p1)), trUCS.FromACS(ucsBend1.ToACS(p2)));

                                p1 = new quaternion(0, bendLEN_2 / 2.0, form.mC, sign_ucsBend2 * form.mB);
                                p2 = new quaternion(0, -bendLEN_2 / 2.0, form.mC, sign_ucsBend2 * form.mB);
                                Pair<quaternion, quaternion> newBend2 = new Pair<quaternion, quaternion>(trUCS.FromACS(ucsBend2.ToACS(p1)), trUCS.FromACS(ucsBend2.ToACS(p2)));

                                p1 = new quaternion(0, bendLEN_3 / 2.0, form.mC, sign_ucsBend3 * form.mB);
                                p2 = new quaternion(0, -bendLEN_3 / 2.0, form.mC, sign_ucsBend3 * form.mB);
                                Pair<quaternion, quaternion> newBend3 = new Pair<quaternion, quaternion>(trUCS.FromACS(ucsBend3.ToACS(p1)), trUCS.FromACS(ucsBend3.ToACS(p2)));


                                //quaternion MASCEN = newBend1.First + newBend1.Second + newBend2.First + newBend2.Second + newBend3.First + newBend3.Second;
                                //MASCEN /= 6.0;

                                List<double> deltaArr = new List<double>();

                                deltaArr.Add(newBend1.First.GetZ());
                                deltaArr.Add(newBend1.Second.GetZ());
                                deltaArr.Add(newBend2.First.GetZ());
                                deltaArr.Add(newBend2.Second.GetZ());
                                deltaArr.Add(newBend3.First.GetZ());
                                deltaArr.Add(newBend3.Second.GetZ());

                                double MIN = deltaArr[0];
                                int Min = 0;
                                double MAX = deltaArr[0];
                                int Max = 0;

                                for (int j = 1; j < deltaArr.Count(); j++)
                                {
                                    if (deltaArr[j] < MIN) { MIN = deltaArr[j]; Min = j; }
                                    if (deltaArr[j] > MAX) { MAX = deltaArr[j]; Max = j; }
                                }

                                UCS UCS1 = TR.GetUcsByBends1(ref container.Bends);
                                Matrix3d mat = new Matrix3d(UCS1.GetAutoCAD_Matrix3d());

                                string Mess = string.Format("{0:f3}", Math.Abs(MAX - MIN));

                                using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                                {
                                    BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                    DBText acText = new DBText();
                                    acText.SetDatabaseDefaults();
                                    acText.Position = new Point3d(-100, 250, offset);
                                    acText.Height = 50;
                                    acText.TextString = Mess;

                                    acText.TransformBy(mat);

                                    acBlkTblRec.AppendEntity(acText);
                                    tr.AddNewlyCreatedDBObject(acText, true);

                                    tr.Commit();
                                }
                            }
                        }
                    }
                    else
                        MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch { }
            finally { Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem = old; }
            
        }//

        //--- Bends Cutting 
       
        [CommandMethod("KojtoCAD_3D", "KCAD_AGP", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/Gaskets_Analize_Distances.htm", "")]//Gaskets Analise Dichtung
        public void KojtoCAD_3D_Analise_Gaskets_Polygons()
        {
            
            
            Matrix3d old = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
            Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    Gaskets_FORM form = new Gaskets_FORM(ConstantsAndSettings.GasketA,
                    ConstantsAndSettings.GasketB, ConstantsAndSettings.GasketC);

                    form.ShowDialog();
                    if (form.DialogResult == DialogResult.OK)
                    {
                        ConstantsAndSettings.GasketA = form.mA;
                        ConstantsAndSettings.GasketB = form.mB;
                        ConstantsAndSettings.GasketC = form.mC;

                        foreach (Polygon POL in container.Polygons)
                        {
                            List<Triangle> TRs = POL.GetPeripherialTrianglesChain(ref container);

                            Pair<quaternion, quaternion> oldBend = null;
                            Pair<quaternion, quaternion> firstBend = null;
                            Bend oBend = null;
                            Triangle oTR = null;
                            foreach (Triangle TR in TRs)
                            {
                                Bend bend = container.Bends[TR.GetFirstBendNumer()];
                                if (bend.IsFictive())
                                {
                                    bend = container.Bends[TR.GetSecondBendNumer()];
                                    if (bend.IsFictive())
                                        bend = container.Bends[TR.GetThirdBendNumer()];
                                }

                                oBend = bend;
                                oTR = TR;
                                double bendLEN = bend.Length - 2.0 * form.mA;

                                if ((bendLEN <= 0))
                                {
                                    MessageBox.Show(string.Format("\n 2.A  > BendLength !\nTriangle {0}", TR.Numer + 1), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    continue;
                                }

                                UCS ucsBend = bend.GetUCS_A();
                                int sign_ucsBend = ((ucsBend.FromACS(TR.Normal.First)).GetZ() >= 0) ? 1 : (-1);

                                quaternion p1 = new quaternion(0, bendLEN / 2.0, form.mC, sign_ucsBend * form.mB);
                                quaternion p2 = new quaternion(0, -bendLEN / 2.0, form.mC, sign_ucsBend * form.mB);
                                Pair<quaternion, quaternion> newBend = new Pair<quaternion, quaternion>(ucsBend.ToACS(p1), ucsBend.ToACS(p2));

                                GlobalFunctions.DrawLine((Point3d)newBend.First, (Point3d)newBend.Second);
                                if ((object)oldBend != null)
                                {
                                    Pair<quaternion, quaternion> m = GlobalFunctions.GetCrossingLinesDistance(oldBend.First, oldBend.Second, newBend.First, newBend.Second, true);
                                    if (((object)m != null) && (m.Second - m.First).abs() > 0.0)
                                    {

                                        quaternion F = ((bend.Start - m.First).abs() < (bend.End - m.First).abs()) ? bend.Start : bend.End;
                                        quaternion bF = F;
                                        F = bend.MidPoint - F; F /= F.abs(); F *= form.mA; F += bF;
                                        UCS ucs = new UCS(F, bend.MidPoint, TR.Normal.First);
                                        if (ucs.FromACS(TR.Normal.Second).GetZ() < 0.0)
                                            ucs = new UCS(F, bend.MidPoint, 2.0 * bend.MidPoint - TR.Normal.First);
                                        Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                        using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                                        {
                                            BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                                            BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                            DBText acText = new DBText();
                                            acText.SetDatabaseDefaults();
                                            acText.Position = new Point3d(25, 25, 0.0);
                                            acText.Height = 50;
                                            acText.TextString = string.Format("{0:f3}", (m.Second - m.First).abs()); ;

                                            acText.TransformBy(mat);

                                            acBlkTblRec.AppendEntity(acText);
                                            tr.AddNewlyCreatedDBObject(acText, true);

                                            tr.Commit();
                                        }
                                    }
                                }
                                oldBend = new Pair<quaternion, quaternion>(newBend.First, newBend.Second);
                                if ((object)firstBend == null)
                                    firstBend = new Pair<quaternion, quaternion>(newBend.First, newBend.Second);
                            }
                            Pair<quaternion, quaternion> M = GlobalFunctions.GetCrossingLinesDistance(oldBend.First, oldBend.Second, firstBend.First, firstBend.Second, true);
                            if (((object)M != null) && ((M.Second - M.First).abs() > 0.0))
                            {

                                quaternion F = ((oBend.Start - M.First).abs() < (oBend.End - M.First).abs()) ? oBend.Start : oBend.End;
                                quaternion bF = F;
                                F = oBend.MidPoint - F; F /= F.abs(); F *= form.mA; F += bF;
                                UCS ucs = new UCS(F, oBend.MidPoint, oTR.Normal.First);
                                if (ucs.FromACS(oTR.Normal.Second).GetZ() < 0.0)
                                    ucs = new UCS(F, oBend.MidPoint, 2.0 * oBend.MidPoint - oTR.Normal.First);
                                Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                GlobalFunctions.DrawText(new Point3d(25, 100, 0.0), 50, string.Format("{0:f3}", (M.Second - M.First).abs()), ref mat);
                            }
                        }
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem = old; }
        
        }
        #endregion

        #region explicit Stoil Iwanow, cherkwa Washington
        [CommandMethod("KojtoCAD_3D", "KCAD_GLOBAL_FITTING", null, CommandFlags.Modal, null, "", "")]
        public void test()
        {
            
           // buffDictionary.Clear();
            if (buffDictionary.Count == 0)
            {
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("KCAD_READMESH S ", true, false, false);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("KCAD_BENDS_TO_DICTIONARY ", true, false, false);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("KCAD_READMESH B ", true, false, false);
            }
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("a3test1 ", true, false, false);
            
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_PARTICULAR_FITTING", null, CommandFlags.Modal, null, "", "")]
        public void test_()
        {
            
            // buffDictionary.Clear();
            if (buffDictionary.Count == 0)
            {
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("KCAD_READMESH S ", true, false, false);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("KCAD_BENDS_TO_DICTIONARY ", true, false, false);
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("KCAD_READMESH B ", true, false, false);
            }
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("a3test2 ", true, false, false);

        }

        [CommandMethod("KojtoCAD_3D", "a3test1", null, CommandFlags.Modal, null, "", "")]
        public void test1()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

             PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
            pDoubleOpts.Message = "\nEnter the Distance from the endPoint of the Bend to anchorPoint : ";
            pDoubleOpts.AllowZero = false;
            pDoubleOpts.AllowNegative = false;
            PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
            if (pDoubleRes.Status == PromptStatus.OK)
            {
                double dLen = pDoubleRes.Value;

                if (buffDictionary.Count > 0)
                {
                    foreach (Bend bend in container.Bends)
                    {
                        if (bend.IsFictive()) continue;

                        UCS UCSbend = bend.GetUCS();

                        quaternion bendORT = bend.End - bend.Start;
                        double bendHalfLen = bendORT.abs() / 2.0;
                        bendHalfLen -= dLen;
                        bendORT /= bendORT.abs();
                        bendORT *= bendHalfLen;

                        quaternion mid_O = bend.MidPoint;
                        quaternion mid_A = UCSbend.ToACS(new quaternion(0, 0, 100, 0));
                        quaternion mid_B = UCSbend.ToACS(new quaternion(0, 0, 0, 100));


                        plane planeMID = new plane(bend.MidPoint, mid_A, mid_B);
                        plane planeEND = new plane(bend.MidPoint + bendORT, mid_A + bendORT, mid_B + bendORT);
                        plane planeSTART = new plane(bend.MidPoint - bendORT, mid_A - bendORT, mid_B - bendORT);

                        #region angle
                        Triangle fTriangle = container.Triangles[bend.FirstTriangleNumer];
                        quaternion bendNormal = bend.Normal - bend.MidPoint;
                        quaternion triangleNormal = fTriangle.Normal.Second - fTriangle.Normal.First;

                        double angle = bendNormal.angTo(triangleNormal); //angle between normals
                        angle *= 180.0;                                  //
                        angle /= Math.PI;                                // rad -> degree  
                        #endregion

                        if (angle != double.NaN)
                        {
                            if ((container.IsBendConvex(bend) == -1) && (angle > 30.0))
                            {
                                Pair<quaternion, quaternion> paSteel = GetSteelBendPoints(bend); // ос на металната тръба / начало край
                                if (paSteel != (object)null)
                                {

                                    #region a>30
                                    Pair<quaternion, quaternion> paSteelNormal = GetSteelBendPointsNormal(bend); // нормала на  металната тръба


                                    #region pre
                                    quaternion paMid = (paSteel.First + paSteel.Second) / 2.0;
                                    UCS UCS = new UCS(paMid, paSteel.First, paMid + paSteelNormal.Second - paSteelNormal.First);
                                    quaternion z = new quaternion(0, 0, 0, 100);
                                    z = UCS.ToACS(z);
                                    plane plane = new plane(paMid, z, paMid + paSteelNormal.Second - paSteelNormal.First);
                                    quaternion Q = (bend.Start+ bend.End)/2.0;
                                    
                                    #endregion

                                    if ((paSteel.Second - paSteel.First).abs() > 2032 * 2)
                                    {
                                        //UtilityClasses.GlobalFunctions.DrawSphere(50.0, (Point3d)Q);
                                        if (Q != (object)null)
                                            DrawPlanka(bend, GetAncorForBendA(bend, paSteel, true, Q - bend.MidPoint), paMid);
                                        //GetAncorForBendA(bend, paSteel, Q - bend.MidPoint);
                                    }

                                    quaternion q = planeEND.IntersectWithVector(paSteel.First, paSteel.Second);
                                    Q = bend.MidPoint + bendORT;
                             

                                    //UtilityClasses.GlobalFunctions.DrawSphere(50.0, (Point3d)Q);                                    
                                     DrawPlanka(bend, GetAncorForBendA(bend, paSteel, true, Q - bend.MidPoint), paMid + q);
                                    //GetAncorForBendA(bend, paSteel,true,Q - bend.MidPoint);

                                    q = planeSTART.IntersectWithVector(paSteel.First, paSteel.Second);
                                    q *= -1.0;
                                    Q = bend.MidPoint - bendORT;                                  
 
                                    //UtilityClasses.GlobalFunctions.DrawSphere(50.0, (Point3d)Q);                                        
                                    DrawPlanka(bend, GetAncorForBendA(bend, paSteel, true, Q - bend.MidPoint), paMid - q);                                    
                                    //GetAncorForBendA(bend, paSteel,true ,Q - bend.MidPoint);
                                    #endregion

                                }
                            }
                            else
                            {
                                try
                                {
                                    if (container.IsBendConvex(bend) == -1)
                                    {

                                        #region angle <= 30
                                        Pair<quaternion, quaternion> paSteel = GetSteelBendPoints(bend); // ос на металната тръба / начало край
                                        if (paSteel != (object)null)
                                        {
                                            Pair<quaternion, quaternion> paSteelNormal = GetSteelBendPointsNormal(bend); // нормала на  металната тръба

                                            #region pre
                                            quaternion paMid = (paSteel.First + paSteel.Second) / 2.0;
                                            UCS UCS = new UCS(paMid, paSteel.First, paMid + paSteelNormal.Second - paSteelNormal.First);
                                            quaternion z = new quaternion(0, 0, 0, 100);
                                            z = UCS.ToACS(z);
                                            plane plane = new plane(paMid, z, paMid + paSteelNormal.Second - paSteelNormal.First);
                                            quaternion Q = plane.IntersectWithVector(bend.Start, bend.End);
                                            #endregion

                                            #region repair

                                            Q = (bend.Start + bend.End)/2.0;
                                            #endregion

                                            if ((paSteel.Second - paSteel.First).abs() > 2032 * 2)
                                            {
                                                //UtilityClasses.GlobalFunctions.DrawSphere(50.0, (Point3d)Q, 3);
                                                if (Q != (object)null)
                                                    DrawPlanka(bend, GetAncorForBendB(bend, paSteel, true, Q - bend.MidPoint), paMid);
                                                // GetAncorForBendB(bend, paSteel,true ,Q - bend.MidPoint);

                                            }


                                            quaternion q = planeEND.IntersectWithVector(paSteel.First, paSteel.Second);
                                            Q = bend.MidPoint + bendORT;


                                            //UtilityClasses.GlobalFunctions.DrawSphere(50.0, (Point3d)Q, 3);
                                            DrawPlanka(bend, GetAncorForBendB(bend, paSteel, true, Q - bend.MidPoint), paMid + q);
                                            //GetAncorForBendB(bend, paSteel,true ,Q - bend.MidPoint);

                                            q = planeSTART.IntersectWithVector(paSteel.First, paSteel.Second);
                                            q *= -1.0;
                                            Q = bend.MidPoint - bendORT;

                                            //UtilityClasses.GlobalFunctions.DrawSphere(50.0, (Point3d)Q, 3);
                                            DrawPlanka(bend, GetAncorForBendB(bend, paSteel, true, Q - bend.MidPoint), paMid - q);
                                            //GetAncorForBendB(bend, paSteel,true ,Q - bend.MidPoint);
                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        if (container.IsBendConvex(bend) == 1)
                                        {

                                            #region concave
                                            Pair<quaternion, quaternion> paSteel = GetSteelBendPoints(bend); // ос на металната тръба / начало край
                                            if (paSteel != (object)null)
                                            {
                                                Pair<quaternion, quaternion> paSteelNormal = GetSteelBendPointsNormal(bend); // нормала на  металната тръба

                                                quaternion paMid = (paSteel.First + paSteel.Second) / 2.0;

                                                UCS UCS = new UCS(paMid, paSteel.First, paMid + paSteelNormal.Second - paSteelNormal.First);
                                                quaternion z = new quaternion(0, 0, 0, 100);
                                                z = UCS.ToACS(z);
                                                plane plane = new plane(paMid, z, paMid + paSteelNormal.Second - paSteelNormal.First);
                                                quaternion Q = (bend.Start+ bend.End)/2.0;
                                               
                                                if ((paSteel.Second - paSteel.First).abs() > 2032 * 2)
                                                {
                                                    //UtilityClasses.GlobalFunctions.DrawSphere(50.0, (Point3d)Q, 5);
                                                    if (Q != (object)null)
                                                        DrawPlanka(bend, GetAncorForBendC(bend, paSteel, true, Q - bend.MidPoint), paMid);
                                                    //GetAncorForBendC(bend, paSteel,true,Q - bend.MidPoint);
                                                }


                                                quaternion q = planeEND.IntersectWithVector(paSteel.First, paSteel.Second);
                                                Q = bend.MidPoint + bendORT;
                                       
          
                                                // UtilityClasses.GlobalFunctions.DrawSphere(50.0, (Point3d)Q, 5);
                                                DrawPlanka(bend, GetAncorForBendC(bend, paSteel, true, Q - bend.MidPoint), paMid + q);
                                                //GetAncorForBendC(bend, paSteel,true, Q - bend.MidPoint);

                                                q = planeSTART.IntersectWithVector(paSteel.First, paSteel.Second);
                                                q *= -1.0;
                                                Q = bend.MidPoint - bendORT;

                                                // UtilityClasses.GlobalFunctions.DrawSphere(50.0, (Point3d)Q, 5);
                                                DrawPlanka(bend, GetAncorForBendC(bend, paSteel, true, Q - bend.MidPoint), paMid - q);
                                                //GetAncorForBendC(bend, paSteel,true ,Q - bend.MidPoint);
                                            }
                                            #endregion

                                        }
                                    }
                                }
                                catch { }
                            }
                        }

                    }
                }
                else
                    MessageBox.Show("for Steel Mesh -> Command: KCAD_BENDS_TO_DICTIONARY", "E R R O R");
            }
        }

        [CommandMethod("KojtoCAD_3D", "a3test2", null, CommandFlags.Modal, null, "", "")]
        public void test2()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                if (buffDictionary.Count > 0)
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
                                        if (!TR.IsFictive())
                                        {
                                            //-----------------------

                                            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                                            pDoubleOpts.Message = "\nEnter the Distance from the midPoint of Bend to anchorPoint : ";
                                            pDoubleOpts.AllowZero = true;
                                            pDoubleOpts.AllowNegative = true;
                                            PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                                            if (pDoubleRes.Status == PromptStatus.OK)
                                            {
                                                Pair<quaternion, quaternion> jPB = GetSteelBendPoints(TR);
                                                quaternion jq = jPB.Second - jPB.First;
                                                jq /= jq.abs();
                                                jq *= pDoubleRes.Value;
                                                //jq += TR.MidPoint;

                                                using (Transaction tr = db.TransactionManager.StartTransaction())
                                                {
                                                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                                                    if (TR.SolidHandle.First >= 0)
                                                    {
                                                        Bend bend = TR;
                                                        double dLen = pDoubleRes.Value;

                                                        UCS UCSbend = bend.GetUCS();

                                                        quaternion bendORT = bend.End - bend.Start;
                                                        double bendHalfLen = bendORT.abs() / 2.0;
                                                        bendHalfLen -= Math.Abs(dLen);
                                                        bendORT /= bendORT.abs();
                                                        bendORT *= bendHalfLen;

                                                        if (dLen < 0.0) bendORT *= -1.0;

                                                        quaternion mid_O = bend.MidPoint;
                                                        quaternion mid_A = UCSbend.ToACS(new quaternion(0, 0, 100, 0));
                                                        quaternion mid_B = UCSbend.ToACS(new quaternion(0, 0, 0, 100));

                                                       
                                                        plane planeEND = new plane(bend.MidPoint + bendORT, mid_A + bendORT, mid_B + bendORT);
                                                       
                                                        

                                                        try
                                                        {
                                                            #region angle
                                                            Triangle fTriangle = container.Triangles[bend.FirstTriangleNumer];
                                                            quaternion bendNormal = bend.Normal - bend.MidPoint;
                                                            quaternion triangleNormal = fTriangle.Normal.Second - fTriangle.Normal.First;

                                                            double angle = bendNormal.angTo(triangleNormal); //angle between normals
                                                            angle *= 180.0;                                  //
                                                            angle /= Math.PI;                                // rad -> degree  
                                                            #endregion

                                                            if (angle != double.NaN)
                                                            {
                                                                if ((container.IsBendConvex(bend) == -1) && (angle > 30.0))
                                                                {
                                                                    Pair<quaternion, quaternion> paSteel = GetSteelBendPoints(bend); // ос на металната тръба / начало край
                                                                    if (paSteel != (object)null)
                                                                    {

                                                                        #region a>30
                                                                        Pair<quaternion, quaternion> paSteelNormal = GetSteelBendPointsNormal(bend); // нормала на  металната тръба


                                                                        #region pre
                                                                        quaternion paMid = (paSteel.First + paSteel.Second) / 2.0;
                                                                       
                                                                        #endregion


                                                                        quaternion q = planeEND.IntersectWithVector(paSteel.First, paSteel.Second);
                                                                        quaternion Q = bend.MidPoint + bendORT;
                                                                        
                                                                        DrawPlanka(bend, GetAncorForBendA(bend, paSteel, true, Q - bend.MidPoint), paMid + q);
                                                                       
                                                                        #endregion

                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    try
                                                                    {
                                                                        if (container.IsBendConvex(bend) == -1)
                                                                        {

                                                                            #region angle <= 30
                                                                            Pair<quaternion, quaternion> paSteel = GetSteelBendPoints(bend); // ос на металната тръба / начало край
                                                                            if (paSteel != (object)null)
                                                                            {
                                                                                Pair<quaternion, quaternion> paSteelNormal = GetSteelBendPointsNormal(bend); // нормала на  металната тръба

                                                                             
                                                                                quaternion paMid = (paSteel.First + paSteel.Second) / 2.0;


                                                                                quaternion q = planeEND.IntersectWithVector(paSteel.First, paSteel.Second);
                                                                                quaternion Q = bend.MidPoint + bendORT;                                                                         

                                                                               
                                                                                DrawPlanka(bend, GetAncorForBendB(bend, paSteel, true, Q - bend.MidPoint), paMid + q);
                                                                                

                                                                            }
                                                                            #endregion

                                                                        }
                                                                        else
                                                                        {
                                                                            if (container.IsBendConvex(bend) == 1)
                                                                            {

                                                                                #region concave
                                                                                Pair<quaternion, quaternion> paSteel = GetSteelBendPoints(bend); // ос на металната тръба / начало край
                                                                                if (paSteel != (object)null)
                                                                                {
                                                                                    Pair<quaternion, quaternion> paSteelNormal = GetSteelBendPointsNormal(bend); // нормала на  металната тръба

                                                                                    quaternion paMid = (paSteel.First + paSteel.Second) / 2.0;

                                                                                    quaternion q = planeEND.IntersectWithVector(paSteel.First, paSteel.Second);
                                                                                    quaternion Q = bend.MidPoint + bendORT;   
                                                                                                                                                                                                                                               
                                                                                    DrawPlanka(bend, GetAncorForBendC(bend, paSteel, true, Q - bend.MidPoint), paMid + q);
                                                                                    

                                                                                }
                                                                                #endregion

                                                                            }
                                                                        }
                                                                    }
                                                                    catch { }
                                                                }
                                                            }
                                                        }
                                                        catch { }
                                                    }
                                                    else { MessageBox.Show("Missing attached pipe \n( or other problem with attached solid )", "E R R O R"); }

                                                    tr.Commit();
                                                    ed.UpdateScreen();
                                                }

                                            }
                                            else
                                            {
                                                return;
                                            }                                           
                                            //-----------------
                                        }
                                        else
                                            MessageBox.Show("Selected Bend is Fictive !", "E R R O R");
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
                    catch{}
                    finally { ed.CurrentUserCoordinateSystem = old; }
                }
                else
                    MessageBox.Show("for Steel Mesh -> Command: KCAD_BENDS_TO_DICTIONARY", "E R R O R");
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //if (container.IsBendConvex(bend) == -1) , angle > 30 degree
        public Pair<Triplet<quaternion, quaternion, quaternion>, quaternion>
            GetAncorForBendA(Bend bend, Pair<quaternion, quaternion> paSteel, bool draw, quaternion disp = null)
        {
            Pair<Triplet<quaternion, quaternion, quaternion>, quaternion> rez
                = new Pair<Triplet<quaternion, quaternion, quaternion>, quaternion>(null,null);

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Triangle fTriangle = container.Triangles[bend.FirstTriangleNumer];
            quaternion bendNormal = bend.Normal - bend.MidPoint;
            quaternion triangleNormal = fTriangle.Normal.Second - fTriangle.Normal.First;

            try
            {
                Triplet<quaternion, quaternion, quaternion> triplet = GetAncorsA(ref bend, disp);
                if (double.IsNaN(triplet.Third.GetX()) || double.IsNaN(triplet.Third.GetY()) || double.IsNaN(triplet.Third.GetZ()) ||
                    double.IsNaN(triplet.Second.GetX()) || double.IsNaN(triplet.Second.GetY()) || double.IsNaN(triplet.Second.GetZ()) ||
                    double.IsNaN(triplet.First.GetX()) || double.IsNaN(triplet.First.GetY()) || double.IsNaN(triplet.First.GetZ()))
                {

                    return GetAncorForBendB(bend, paSteel, draw, disp);
                }


                plane sectPlane = new plane(bend.MidPoint + disp, triplet.Second, triplet.Third);
                quaternion intPoint = sectPlane.IntersectWithVector(paSteel.First, paSteel.Second);

                rez = new Pair<Triplet<quaternion, quaternion, quaternion>, quaternion>
                    (new Triplet<quaternion, quaternion, quaternion>(triplet.First, triplet.Second, triplet.Third), intPoint);

                if (draw)
                {
                    GlobalFunctions.DrawLine((Point3d)triplet.Third, (Point3d)intPoint, 2);

                    #region ucs
                    UCS UCSS = new UCS(triplet.Third, intPoint, bend.GetUCS().ToACS(new quaternion(0, 0, 0, 1000)));
                    // MathLibKCAD.UCS UCSS = new MathLibKCAD.UCS(triplet.Third, intPoint, bend.MidPoint);
                    if (disp != (object)null)
                        UCSS = new UCS(triplet.Third, intPoint, disp + bend.GetUCS().ToACS(new quaternion(0, 0, 0, 1000)));
                    // UCSS = new MathLibKCAD.UCS(triplet.Third, intPoint, bend.MidPoint + disp);
                    Matrix3d mat = new Matrix3d(UCSS.GetAutoCAD_Matrix3d());


                    UCS ucs1 = new UCS(triplet.Third, triplet.Second, bend.MidPoint);
                    if (disp != (object)null)
                        ucs1 = new UCS(triplet.Third, triplet.Second, bend.MidPoint + disp);
                    Matrix3d mat1 = new Matrix3d(ucs1.GetAutoCAD_Matrix3d());

                    triangleNormal /= triangleNormal.abs();
                    triangleNormal *= backToCenterLevel;
                    UCS ucs2 = new UCS(triplet.First, triplet.Second + triangleNormal, bend.MidPoint);
                    if (disp != (object)null)
                        ucs2 = new UCS(triplet.First, triplet.Second + triangleNormal, bend.MidPoint + disp);
                    Matrix3d mat2 = new Matrix3d(ucs2.GetAutoCAD_Matrix3d());

                    triangleNormal /= triangleNormal.abs();
                    triangleNormal *= backToCenterLevel;
                    #endregion

                    #region trans
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl = acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        BlockTableRecord btr = tr.GetObject(acBlkTbl["vilka"], OpenMode.ForWrite) as BlockTableRecord;
                        BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);

                        br.TransformBy(mat);
                        acBlkTblRec.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);

                        BlockTableRecord btrSHp = tr.GetObject(acBlkTbl["Shpilka"], OpenMode.ForWrite) as BlockTableRecord;
                        BlockReference brShpilka = new BlockReference(Point3d.Origin, btrSHp.ObjectId);
                        brShpilka.TransformBy(mat1);
                        acBlkTblRec.AppendEntity(brShpilka);
                        tr.AddNewlyCreatedDBObject(brShpilka, true);

                        BlockTableRecord btr1 = tr.GetObject(acBlkTbl["motov"], OpenMode.ForWrite) as BlockTableRecord;
                        BlockReference br1 = new BlockReference(Point3d.Origin, btr1.ObjectId);
                        br1.TransformBy(mat1);
                        acBlkTblRec.AppendEntity(br1);
                        tr.AddNewlyCreatedDBObject(br1, true);

                        BlockReference br12 = new BlockReference(Point3d.Origin, btr1.ObjectId);
                        br12.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -127 * 2 - 33.8 * 2)));
                        quaternion acQ1 = bend.MidPoint;
                        quaternion acQ2 = bend.Normal;
                        quaternion acQ3 = bend.Start;
                        Plane acPlane = new Plane((Point3d)acQ1, (Point3d)acQ2, (Point3d)acQ3);
                        br12.TransformBy(mat1);
                        br12.TransformBy(Matrix3d.Mirroring(acPlane));
                        acBlkTblRec.AppendEntity(br12);
                        tr.AddNewlyCreatedDBObject(br12, true);

                        BlockTableRecord btr2 = tr.GetObject(acBlkTbl["kub"], OpenMode.ForWrite) as BlockTableRecord;
                        BlockReference br2 = new BlockReference(Point3d.Origin, btr2.ObjectId);
                        //br2.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -160)));
                        br2.TransformBy(mat2);
                        acBlkTblRec.AppendEntity(br2);
                        tr.AddNewlyCreatedDBObject(br2, true);


                        BlockTableRecord btr22 = tr.GetObject(acBlkTbl["kubb"], OpenMode.ForWrite) as BlockTableRecord;
                        BlockReference br22 = new BlockReference(Point3d.Origin, btr22.ObjectId);
                        //br22.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, 160)));
                        br22.TransformBy(mat2);
                        br22.TransformBy(Matrix3d.Mirroring(acPlane));
                        acBlkTblRec.AppendEntity(br22);
                        tr.AddNewlyCreatedDBObject(br22, true);


                        DBObjectCollection coll = new DBObjectCollection();
                        br.Explode(coll);
                        br1.Explode(coll);


                        tr.Commit();
                    }
                    #endregion
                }
            }
            catch
            {
                return GetAncorForBendB(bend, paSteel, draw, disp);
            }
            return rez;
        }
        //if (container.IsBendConvex(bend) == -1), angle < 30 degree
        public Pair<Triplet<quaternion, quaternion, quaternion>, quaternion>
            GetAncorForBendB(Bend bend, Pair<quaternion, quaternion> paSteel, bool draw, quaternion disp = null)
        {
            Pair<Triplet<quaternion, quaternion, quaternion>, quaternion> rez
               = new Pair<Triplet<quaternion, quaternion, quaternion>, quaternion>(null, null);

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Triangle fTriangle = container.Triangles[bend.FirstTriangleNumer];
            quaternion bendNormal = bend.Normal - bend.MidPoint;
            quaternion triangleNormal = fTriangle.Normal.Second - fTriangle.Normal.First;

            Triplet<quaternion, quaternion, quaternion> triplet = GetAncorsB(ref bend, disp);
            plane sectPlane = new plane(bend.MidPoint + disp, triplet.Second, triplet.Third);
            quaternion intPoint = sectPlane.IntersectWithVector(paSteel.First, paSteel.Second);

            rez = new Pair<Triplet<quaternion, quaternion, quaternion>, quaternion>
                (new Triplet<quaternion, quaternion, quaternion>(triplet.First, triplet.Second, triplet.Third), intPoint);

            if (draw)
            {
                GlobalFunctions.DrawLine((Point3d)triplet.Third, (Point3d)intPoint, 2);

                #region ucs

                UCS UCSS = new UCS(triplet.Third, intPoint, bend.GetUCS().ToACS(new quaternion(0, 0, 0, 1000)));                
                if (disp != (object)null)
                    UCSS = new UCS(triplet.Third, intPoint, disp + bend.GetUCS().ToACS(new quaternion(0, 0, 0, 1000)));                
                Matrix3d mat = new Matrix3d(UCSS.GetAutoCAD_Matrix3d());

                /*
                MathLibKCAD.UCS UCSS = new MathLibKCAD.UCS(triplet.Third, intPoint, bend.MidPoint);
                if (disp != (object)null)
                    UCSS = new MathLibKCAD.UCS(triplet.Third, intPoint, bend.MidPoint + disp);
                Matrix3d mat = new Matrix3d(UCSS.GetAutoCAD_Matrix3d());
                */

                UCS ucs1 = new UCS(triplet.Third, triplet.Second, bend.MidPoint);
                if (disp != (object)null)
                    ucs1 = new UCS(triplet.Third, triplet.Second, bend.MidPoint + disp);
                Matrix3d mat1 = new Matrix3d(ucs1.GetAutoCAD_Matrix3d());

                UCS ucs2 = new UCS(triplet.Second, triplet.First, bend.MidPoint);
                if (disp != (object)null)
                    ucs2 = new UCS(triplet.Second, triplet.First, bend.MidPoint + disp);
                Matrix3d mat2 = new Matrix3d(ucs2.GetAutoCAD_Matrix3d());

                triangleNormal /= triangleNormal.abs();
                triangleNormal *= backToCenterLevel;
                #endregion

                #region trans
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    BlockTableRecord btr = tr.GetObject(acBlkTbl["vilka"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);
                                     
                    br.TransformBy(mat);

                    acBlkTblRec.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);

                    BlockTableRecord btrSHp = tr.GetObject(acBlkTbl["Shpilka"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference brShpilka = new BlockReference(Point3d.Origin, btrSHp.ObjectId);
                    brShpilka.TransformBy(mat1);
                    acBlkTblRec.AppendEntity(brShpilka);
                    tr.AddNewlyCreatedDBObject(brShpilka, true);

                    BlockTableRecord btr1 = tr.GetObject(acBlkTbl["motov"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br1 = new BlockReference(Point3d.Origin, btr1.ObjectId);
                    br1.TransformBy(mat1);
                    acBlkTblRec.AppendEntity(br1);
                    tr.AddNewlyCreatedDBObject(br1, true);

                    BlockReference br12 = new BlockReference(Point3d.Origin, btr1.ObjectId);
                    br12.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -127 * 2 - 33.8 * 2)));
                    quaternion acQ1 = bend.MidPoint;
                    quaternion acQ2 = bend.Normal;
                    quaternion acQ3 = bend.Start;
                    Plane acPlane = new Plane((Point3d)acQ1, (Point3d)acQ2, (Point3d)acQ3);
                    br12.TransformBy(mat1);
                    br12.TransformBy(Matrix3d.Mirroring(acPlane));
                    acBlkTblRec.AppendEntity(br12);
                    tr.AddNewlyCreatedDBObject(br12, true);

                    BlockTableRecord btr2 = tr.GetObject(acBlkTbl["kub1"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br2 = new BlockReference(Point3d.Origin, btr2.ObjectId);                    
                    //br2.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -160)));
                    br2.TransformBy(mat2);
                    acBlkTblRec.AppendEntity(br2);
                    tr.AddNewlyCreatedDBObject(br2, true);


                    BlockTableRecord btr22 = tr.GetObject(acBlkTbl["kubb1"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br22 = new BlockReference(Point3d.Origin, btr22.ObjectId);
                    //br22.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, 160)));                    
                    br22.TransformBy(mat2);
                    br22.TransformBy(Matrix3d.Mirroring(acPlane));                   
                    acBlkTblRec.AppendEntity(br22);
                    tr.AddNewlyCreatedDBObject(br22, true);
                    

                    DBObjectCollection coll = new DBObjectCollection();
                    br.Explode(coll);
                    br1.Explode(coll);


                    tr.Commit();
                }
                #endregion
            }
            return rez;
        }
        //if (container.IsBendConvex(bend) == 1)
        public Pair<Triplet<quaternion, quaternion, quaternion>, quaternion> 
            GetAncorForBendC(Bend bend, Pair<quaternion, quaternion> paSteel,bool draw, quaternion disp = null)
        {
            Pair<Triplet<quaternion, quaternion, quaternion>, quaternion> rez
               = new Pair<Triplet<quaternion, quaternion, quaternion>, quaternion>(null, null);

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Triangle fTriangle = container.Triangles[bend.FirstTriangleNumer];
            quaternion bendNormal = bend.Normal - bend.MidPoint;
            quaternion triangleNormal = fTriangle.Normal.Second - fTriangle.Normal.First;

            Triplet<quaternion, quaternion, quaternion> triplet = GetAncorsC(ref bend, disp);
            plane sectPlane = new plane(bend.MidPoint + disp, triplet.Second, triplet.Third);
            quaternion intPoint = sectPlane.IntersectWithVector(paSteel.First, paSteel.Second);

            rez = new Pair<Triplet<quaternion, quaternion, quaternion>, quaternion>
                (new Triplet<quaternion, quaternion, quaternion>(triplet.First, triplet.Second, triplet.Third), intPoint);

            if (draw)
            {
                GlobalFunctions.DrawLine((Point3d)triplet.Third, (Point3d)intPoint, 2);

                #region ucs
                UCS UCSS = new UCS(triplet.Third, intPoint, bend.GetUCS().ToACS(new quaternion(0, 0, 0, 1000)));
                if (disp != (object)null)
                    UCSS = new UCS(triplet.Third, intPoint, disp + bend.GetUCS().ToACS(new quaternion(0, 0, 0, 1000)));
                Matrix3d mat = new Matrix3d(UCSS.GetAutoCAD_Matrix3d());

                /*
                MathLibKCAD.UCS UCSS = new MathLibKCAD.UCS(triplet.Third, intPoint, bend.MidPoint);
                if (disp != (object)null)
                    UCSS = new MathLibKCAD.UCS(triplet.Third, intPoint, bend.MidPoint + disp);
                Matrix3d mat = new Matrix3d(UCSS.GetAutoCAD_Matrix3d());
                */

                UCS ucs1 = new UCS(triplet.Third, triplet.Second, bend.MidPoint);
                if (disp != (object)null)
                    ucs1 = new UCS(triplet.Third, triplet.Second, bend.MidPoint + disp);
                Matrix3d mat1 = new Matrix3d(ucs1.GetAutoCAD_Matrix3d());

                UCS ucs2 = new UCS(triplet.Second, triplet.First, bend.MidPoint);
                if (disp != (object)null)
                    ucs2 = new UCS(triplet.Second, triplet.First, bend.MidPoint + disp);
                Matrix3d mat2 = new Matrix3d(ucs2.GetAutoCAD_Matrix3d());

                triangleNormal /= triangleNormal.abs();
                triangleNormal *= backToCenterLevel;
                #endregion

                #region trans
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    BlockTableRecord btr = tr.GetObject(acBlkTbl["vilka"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br = new BlockReference(Point3d.Origin, btr.ObjectId);

                 
                    br.TransformBy(mat);

                    acBlkTblRec.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);

                    BlockTableRecord btrSHp = tr.GetObject(acBlkTbl["Shpilka"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference brShpilka = new BlockReference(Point3d.Origin, btrSHp.ObjectId);
                    brShpilka.TransformBy(mat1);
                    acBlkTblRec.AppendEntity(brShpilka);
                    tr.AddNewlyCreatedDBObject(brShpilka, true);

                    BlockTableRecord btr1 = tr.GetObject(acBlkTbl["motov"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br1 = new BlockReference(Point3d.Origin, btr1.ObjectId);
                    br1.TransformBy(mat1);
                    acBlkTblRec.AppendEntity(br1);
                    tr.AddNewlyCreatedDBObject(br1, true);

                    BlockReference br12 = new BlockReference(Point3d.Origin, btr1.ObjectId);
                    br12.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -127 * 2 - 33.8 * 2)));
                    quaternion acQ1 = bend.MidPoint;
                    quaternion acQ2 = bend.Normal;
                    quaternion acQ3 = bend.Start;
                    Plane acPlane = new Plane((Point3d)acQ1, (Point3d)acQ2, (Point3d)acQ3);
                    br12.TransformBy(mat1);
                    br12.TransformBy(Matrix3d.Mirroring(acPlane));
                    acBlkTblRec.AppendEntity(br12);
                    tr.AddNewlyCreatedDBObject(br12, true);

                    BlockTableRecord btr2 = tr.GetObject(acBlkTbl["kub1"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br2 = new BlockReference(Point3d.Origin, btr2.ObjectId);
                    //br2.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, 160)));
                    br2.TransformBy(mat2);
                    acBlkTblRec.AppendEntity(br2);
                    tr.AddNewlyCreatedDBObject(br2, true);


                    BlockTableRecord btr22 = tr.GetObject(acBlkTbl["kubb1"], OpenMode.ForWrite) as BlockTableRecord;
                    BlockReference br22 = new BlockReference(Point3d.Origin, btr22.ObjectId);
                    //br22.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0,-160)));
                    br22.TransformBy(mat2);
                    br22.TransformBy(Matrix3d.Mirroring(acPlane));                   
                    acBlkTblRec.AppendEntity(br22);
                    tr.AddNewlyCreatedDBObject(br22, true);
                    

                    DBObjectCollection coll = new DBObjectCollection();
                    br.Explode(coll);
                    br1.Explode(coll);


                    tr.Commit();
                }
                #endregion
            }
            return rez;
        }


        //if (container.IsBendConvex(bend) == -1) , angle > 30 degree
        public Triplet<quaternion, quaternion, quaternion> GetAncorsA(ref Bend bend, quaternion disp = null)
        {

            Triangle fTriangle = container.Triangles[bend.FirstTriangleNumer];
            quaternion bendNormal = bend.Normal - bend.MidPoint;
            quaternion triangleNormal = fTriangle.Normal.Second - fTriangle.Normal.First;
            
            Bend bend1 = container.Bends[fTriangle.GetFirstBendNumer()];
            Bend bend2 = container.Bends[fTriangle.GetSecondBendNumer()];
            Bend bend3 = container.Bends[fTriangle.GetThirdBendNumer()];

            triangleNormal /= triangleNormal.abs();   // length = 1.0

            UCS bend_ucs = new UCS(bend.MidPoint, bend.Start, fTriangle.Normal.First);
            if (bend_ucs.FromACS(fTriangle.Normal.Second).GetZ() < 0)
                bend_ucs = new UCS(bend.MidPoint, bend.End, fTriangle.Normal.First);

            quaternion bend_ucs_Y_ort = bend_ucs.ToACS(new quaternion(0, 0, 100, 0)) - bend.MidPoint;
            bend_ucs_Y_ort /= bend_ucs_Y_ort.abs();

            UCS bendUCS = bend.GetUCS();  // ucs Y = bend Normal, X = bend
            if (bendUCS.FromACS(fTriangle.Normal.First).GetZ() < 0.0)
                bendUCS = bend.GetUCS_A();

            //UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass
            //UtilityClasses.ConstantsAndSettings.DoubleGlass_h2
            // UtilityClasses.ConstantsAndSettings.halfGlassFugue

            triangleNormal *= (ConstantsAndSettings.Thickness_of_the_Glass/* + blockH_1*/);
            plane planeUp = new plane(bend1.MidPoint + triangleNormal, bend2.MidPoint + triangleNormal, bend3.MidPoint + triangleNormal);

            quaternion fugueLineStart = new quaternion(0, 0, 0, ConstantsAndSettings.halfGlassFugue/*UtilityClasses.ConstantsAndSettings.halfGlassFugue + additionalFugue*/);
            quaternion fugueLineEnd = new quaternion(0, 0, 100, ConstantsAndSettings.halfGlassFugue/*UtilityClasses.ConstantsAndSettings.halfGlassFugue + additionalFugue*/);

            fugueLineStart = bendUCS.ToACS(fugueLineStart);
            fugueLineEnd = bendUCS.ToACS(fugueLineEnd);

            quaternion upIntersect = planeUp.IntersectWithVector(fugueLineStart, fugueLineEnd);
            quaternion bend_ucs_Y_ort_1 = bend_ucs_Y_ort *(25.4 - 2.4095778);//&&&
            upIntersect += bend_ucs_Y_ort_1;//&&&
            quaternion triangleNormal_1 = triangleNormal / triangleNormal.abs();//&&&
            triangleNormal_1 *= blockH_1;
            upIntersect += triangleNormal_1;//&&&

            //UtilityClasses.GlobalFunctions.DrawLine((Point3d)bend.MidPoint, (Point3d)upIntersect, 2);

            triangleNormal /= triangleNormal.abs();
            triangleNormal *= -backToCenterLevel;

            quaternion firstCenter = upIntersect + bend_ucs_Y_ort * firstCenterDistance;
            quaternion buffFirstCenter = firstCenter;
            firstCenter += triangleNormal;

            UCS ucs = new UCS(firstCenter, upIntersect, bend.MidPoint);

            //MessageBox.Show(bendUCS.FromACS(firstCenter).GetZ().ToString());

            quaternion a = bend.MidPoint;
            quaternion b = bend.Normal;
            quaternion d = b - a;
            d /= d.abs();
            d *= 1000;
            b = a + d;


            a = ucs.FromACS(a);
            b = ucs.FromACS(b);

            // Matrix3d mat = new  Matrix3d(ucs.GetAutoCAD_Matrix3d());
            // UtilityClasses.GlobalFunctions.DrawLine((Point3d)a, (Point3d)b, 2, ref mat);
            // UtilityClasses.GlobalFunctions.DrawCircle(secondCenterDistance, new Point3d(), 2, ref mat);

            double R = secondCenterDistance;

            d = b - a;

            double dX = d.GetX();
            double dY = d.GetY();

            double A = dX * dX / (R * R) + dY * dY / (R * R);
            double B = 2 * (dX * a.GetX() / (R * R) + dY * a.GetY() / (R * R));
            double C = a.GetX() * a.GetX() / (R * R) + a.GetY() * a.GetY() / (R * R) - 1.0;

            double k1 = (-B + Math.Sqrt(B * B - 4.0 * A * C)) / (2.0 * A);
            double k2 = (-B - Math.Sqrt(B * B - 4.0 * A * C)) / (2.0 * A);

            quaternion s = a + (b - a) * k1;
            quaternion m = a + (b - a) * k2;

            s = ucs.ToACS(s);
            m = ucs.ToACS(m);
           
            quaternion cen2 = ((bend.MidPoint - s).abs() > (bend.MidPoint - m).abs()) ? s : m;
           
            if(disp == (object)null)
                 return new Triplet<quaternion, quaternion, quaternion>(upIntersect, firstCenter, cen2);
            else
                return new Triplet<quaternion, quaternion, quaternion>(upIntersect+disp, firstCenter+disp, cen2+disp);
        }
        //if (container.IsBendConvex(bend) == -1), angle < 30 degree
        public Triplet<quaternion, quaternion, quaternion> GetAncorsB(ref Bend bend, quaternion disp = null)
        {
            Triangle fTriangle = container.Triangles[bend.FirstTriangleNumer];
            quaternion bendNormal = bend.Normal - bend.MidPoint;
            quaternion triangleNormal = fTriangle.Normal.Second - fTriangle.Normal.First;

            Bend bend1 = container.Bends[fTriangle.GetFirstBendNumer()];
            Bend bend2 = container.Bends[fTriangle.GetSecondBendNumer()];
            Bend bend3 = container.Bends[fTriangle.GetThirdBendNumer()];

            triangleNormal /= triangleNormal.abs();   // length = 1.0

            UCS bend_ucs = new UCS(bend.MidPoint, bend.Start, fTriangle.Normal.First);
            if (bend_ucs.FromACS(fTriangle.Normal.Second).GetZ() < 0)
                bend_ucs = new UCS(bend.MidPoint, bend.End, fTriangle.Normal.First);

            quaternion bend_ucs_Y_ort = bend_ucs.ToACS(new quaternion(0, 0, 100, 0)) - bend.MidPoint;
            bend_ucs_Y_ort /= bend_ucs_Y_ort.abs();

            UCS bendUCS = bend.GetUCS();  // ucs Y = bend Normal, X = bend
            if (bendUCS.FromACS(fTriangle.Normal.First).GetZ() < 0.0)
                bendUCS = bend.GetUCS_A();

            triangleNormal *= ConstantsAndSettings.Thickness_of_the_Glass;
            plane planeUp = new plane(bend1.MidPoint + triangleNormal, bend2.MidPoint + triangleNormal, bend3.MidPoint + triangleNormal);

            quaternion fugueLineStart = new quaternion(0, 0, 0, ConstantsAndSettings.halfGlassFugue/*UtilityClasses.ConstantsAndSettings.halfGlassFugue + additionalFugue*/);
            quaternion fugueLineEnd = new quaternion(0, 0, 100, ConstantsAndSettings.halfGlassFugue/*UtilityClasses.ConstantsAndSettings.halfGlassFugue + additionalFugue*/);

            fugueLineStart = bendUCS.ToACS(fugueLineStart);
            fugueLineEnd = bendUCS.ToACS(fugueLineEnd);

            quaternion upIntersect = planeUp.IntersectWithVector(fugueLineStart, fugueLineEnd);
           
            triangleNormal /= triangleNormal.abs();   // length = 1.0
            triangleNormal *= blockH_2;
            quaternion firstCenter = upIntersect + triangleNormal + bend_ucs_Y_ort * 12.7575603;
            upIntersect = firstCenter + bend_ucs_Y_ort * firstCenterDistance;
            

            UCS ucs = new UCS(firstCenter, upIntersect, bend.MidPoint);
           
            quaternion a = bend.MidPoint;
            quaternion b = bend.Normal;
            quaternion d = b - a;
            d /= d.abs();
            d *= 1000;
            b = a + d;


            a = ucs.FromACS(a);
            b = ucs.FromACS(b);


            double R = secondCenterDistance;

            d = b - a;

            double dX = d.GetX();
            double dY = d.GetY();

            double A = dX * dX / (R * R) + dY * dY / (R * R);
            double B = 2 * (dX * a.GetX() / (R * R) + dY * a.GetY() / (R * R));
            double C = a.GetX() * a.GetX() / (R * R) + a.GetY() * a.GetY() / (R * R) - 1.0;

            double k1 = (-B + Math.Sqrt(B * B - 4.0 * A * C)) / (2.0 * A);
            double k2 = (-B - Math.Sqrt(B * B - 4.0 * A * C)) / (2.0 * A);

            quaternion s = a + (b - a) * k1;
            quaternion m = a + (b - a) * k2;

            s = ucs.ToACS(s);
            m = ucs.ToACS(m);

            quaternion cen2 = ((bend.MidPoint - s).abs() > (bend.MidPoint - m).abs()) ? s : m;

            if (disp == (object)null)
                return new Triplet<quaternion, quaternion, quaternion>(upIntersect, firstCenter, cen2);
            else
                return new Triplet<quaternion, quaternion, quaternion>(upIntersect + disp, firstCenter + disp, cen2 + disp);
        }
        //if (container.IsBendConvex(bend) == 1)
        public Triplet<quaternion, quaternion, quaternion> GetAncorsC(ref Bend bend, quaternion disp = null)
        {
            Triangle fTriangle = container.Triangles[bend.FirstTriangleNumer];
            quaternion bendNormal = bend.Normal - bend.MidPoint;
            quaternion triangleNormal = fTriangle.Normal.Second - fTriangle.Normal.First;
            double angle = bendNormal.angTo(triangleNormal);

            Bend bend1 = container.Bends[fTriangle.GetFirstBendNumer()];
            Bend bend2 = container.Bends[fTriangle.GetSecondBendNumer()];
            Bend bend3 = container.Bends[fTriangle.GetThirdBendNumer()];

            triangleNormal /= triangleNormal.abs();   // length = 1.0
           // UtilityClasses.GlobalFunctions.DrawLine(new Point3d(10000,10000,10000), (Point3d)bend.MidPoint, 1);

            UCS bend_ucs = new UCS(bend.MidPoint, bend.Start, fTriangle.Normal.First);
            if (bend_ucs.FromACS(fTriangle.Normal.Second).GetZ() < 0)
                bend_ucs = new UCS(bend.MidPoint, bend.End, fTriangle.Normal.First);

            quaternion bend_ucs_Y_ort = bend_ucs.ToACS(new quaternion(0, 0, 100, 0)) - bend.MidPoint;
            bend_ucs_Y_ort /= bend_ucs_Y_ort.abs();

            UCS bendUCS = bend.GetUCS();  // ucs Y = bend Normal, X = bend
            if (bendUCS.FromACS(fTriangle.Normal.First).GetZ() < 0.0)
                bendUCS = bend.GetUCS_A();

            triangleNormal *= ConstantsAndSettings.Thickness_of_the_Glass - ConstantsAndSettings.DoubleGlass_h2;
            plane planeUp = new plane(bend1.MidPoint + triangleNormal, bend2.MidPoint + triangleNormal, bend3.MidPoint + triangleNormal);

            quaternion fugueLineStart = new quaternion(0, 0, 0, ConstantsAndSettings.halfGlassFugue/*UtilityClasses.ConstantsAndSettings.halfGlassFugue + additionalFugue*/);
            quaternion fugueLineEnd = new quaternion(0, 0, 100, ConstantsAndSettings.halfGlassFugue/*UtilityClasses.ConstantsAndSettings.halfGlassFugue + additionalFugue*/);

            fugueLineStart = bendUCS.ToACS(fugueLineStart);
            fugueLineEnd = bendUCS.ToACS(fugueLineEnd);

            quaternion upIntersect = planeUp.IntersectWithVector(fugueLineStart, fugueLineEnd);
            triangleNormal /= triangleNormal.abs();
            triangleNormal *= ConstantsAndSettings.DoubleGlass_h2;
            upIntersect += triangleNormal;


            triangleNormal /= triangleNormal.abs();   // length = 1.0
            triangleNormal *= blockH_2;

            quaternion firstCenter = firstCenter = upIntersect + triangleNormal + bend_ucs_Y_ort * (5.9 - 0.04 - 0.0024397);
            if (angle <= Math.PI / 4.5)                         
                firstCenter = upIntersect + triangleNormal + bend_ucs_Y_ort * 12.7575603;
            //MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage((angle * 180.0 / Math.PI).ToString());

            upIntersect = firstCenter + bend_ucs_Y_ort * firstCenterDistance;

            UCS ucs = new UCS(firstCenter, upIntersect, bend.MidPoint);

            quaternion a = bend.MidPoint;
            quaternion b = bend.Normal;
            quaternion d = b - a;
            d /= d.abs();
            d *= 1000;
            b = a + d;


            a = ucs.FromACS(a);
            b = ucs.FromACS(b);


            double R = secondCenterDistance;

            d = b - a;

            double dX = d.GetX();
            double dY = d.GetY();

            double A = dX * dX / (R * R) + dY * dY / (R * R);
            double B = 2 * (dX * a.GetX() / (R * R) + dY * a.GetY() / (R * R));
            double C = a.GetX() * a.GetX() / (R * R) + a.GetY() * a.GetY() / (R * R) - 1.0;

            double k1 = (-B + Math.Sqrt(B * B - 4.0 * A * C)) / (2.0 * A);
            double k2 = (-B - Math.Sqrt(B * B - 4.0 * A * C)) / (2.0 * A);

            quaternion s = a + (b - a) * k1;
            quaternion m = a + (b - a) * k2;

            s = ucs.ToACS(s);
            m = ucs.ToACS(m);

            quaternion cen2 = ((bend.MidPoint - s).abs() > (bend.MidPoint - m).abs()) ? s : m;

            if (disp == (object)null)
                return new Triplet<quaternion, quaternion, quaternion>(upIntersect, firstCenter, cen2);
            else
                return new Triplet<quaternion, quaternion, quaternion>(upIntersect + disp, firstCenter + disp, cen2 + disp);            
        }

        public void DrawPlanka(Bend bend,  
            Pair<Triplet<quaternion, quaternion, quaternion>, quaternion> data,
            quaternion baseQ)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Pair<quaternion, quaternion> axe = GetSteelBendPoints(bend);
            Pair<quaternion, quaternion> axeNormal = GetSteelBendPointsNormal(bend);
            UCS axeUCS = new UCS(axeNormal.First, axeNormal.Second, axe.Second);
            axeUCS = new UCS(axeNormal.First, axeNormal.Second, axeUCS.ToACS(new quaternion(0,0,0,100.0)));
            Matrix3d axeMAT = new Matrix3d(axeUCS.GetAutoCAD_Matrix3d());

            #region cylinder intersect points
            quaternion a = data.First.Third;
            quaternion b = data.Second;
            quaternion d = b - a;
            d /= d.abs();
            d *= 1000;
            b = a + d;

            a = axeUCS.FromACS(a);
            b = axeUCS.FromACS(b);

            double R = cylR;

            d = b - a;

            double dX = d.GetX();
            double dY = d.GetY();

            double A = dX * dX / (R * R) + dY * dY / (R * R);
            double B = 2 * (dX * a.GetX() / (R * R) + dY * a.GetY() / (R * R));
            double C = a.GetX() * a.GetX() / (R * R) + a.GetY() * a.GetY() / (R * R) - 1.0;

            double k1 = (-B + Math.Sqrt(B * B - 4.0 * A * C)) / (2.0 * A);
            double k2 = (-B - Math.Sqrt(B * B - 4.0 * A * C)) / (2.0 * A);

            quaternion s = a + (b - a) * k1;
            quaternion m = a + (b - a) * k2;

            s = axeUCS.ToACS(s);
            m = axeUCS.ToACS(m);

            #endregion
            quaternion cylInt = ((data.First.Third - s).abs() < (data.First.Third - m).abs()) ? s : m;

           // UtilityClasses.GlobalFunctions.DrawLine((Point3d)data.Second, (Point3d)baseQ, 1);
            //ed.WriteMessage("\n"+(baseQ - data.Second).abs().ToString());

            double length = (data.First.Third - cylInt).abs()-len3 + 30.0;

            quaternion dQ =  data.Second - data.First.Third;
            dQ /= dQ.abs();
            dQ *= len3;
            dQ += data.First.Third;
            UCS Ucs = new UCS(dQ,dQ+bend.End-bend.Start,data.Second);
            Matrix3d matUcs = new Matrix3d(Ucs.GetAutoCAD_Matrix3d());
           
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Solid3d acSol3DCyl = new Solid3d();
                acSol3DCyl.SetDatabaseDefaults();
                acSol3DCyl.CreateFrustum((axe.Second - axe.First).abs(), cylR, cylR, cylR);
                acSol3DCyl.ColorIndex = 4;

                acSol3DCyl.TransformBy(axeMAT);

                acBlkTblRec.AppendEntity(acSol3DCyl);
                tr.AddNewlyCreatedDBObject(acSol3DCyl, true);

                Polyline acPoly = new Polyline();
                acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(halfLenpl,0), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(halfLenpl,length), 0, 0, 0);
                acPoly.AddVertexAt(3, new Point2d(-halfLenpl, length), 0, 0, 0);
                acPoly.AddVertexAt(4, new Point2d(-halfLenpl,0), 0, 0, 0);
                acPoly.AddVertexAt(5, new Point2d(0, 0), 0, 0, 0);

                //acPoly.TransformBy(matUcs);
                
                Solid3d acSol3D = new Solid3d();
                acSol3D.CreateExtrudedSolid(acPoly, new Vector3d(0, 0, planka_debelina), new SweepOptions());

                acSol3D.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -planka_debelina/2.0)));

                acBlkTblRec.AppendEntity(acSol3D);
                tr.AddNewlyCreatedDBObject(acSol3D, true);

                acSol3D.TransformBy(matUcs);

                acSol3D.BooleanOperation(BooleanOperationType.BoolSubtract,acSol3DCyl);

                //----------
                Solid3d acSol3DCy = new Solid3d();                
                acSol3DCy.CreateFrustum(40.0, otworR, otworR,otworR);
                acSol3DCy.TransformBy(Matrix3d.Displacement(new Vector3d(31.75,31.75,0)));
                acSol3DCy.TransformBy(matUcs);

                acSol3D.BooleanOperation(BooleanOperationType.BoolSubtract, acSol3DCy);

                acSol3DCy.Dispose();

                //acBlkTblRec.AppendEntity(acSol3DCy);
                //tr.AddNewlyCreatedDBObject(acSol3DCy, true);
              
                Solid3d acSol3DC = new Solid3d();
                acSol3DC.CreateFrustum(40.0, otworR, otworR, otworR);
                acSol3DC.TransformBy(Matrix3d.Displacement(new Vector3d(-31.75, 31.75, 0)));
                acSol3DC.TransformBy(matUcs);
                acSol3D.BooleanOperation(BooleanOperationType.BoolSubtract, acSol3DC);

                //acBlkTblRec.AppendEntity(acSol3DC);
                //tr.AddNewlyCreatedDBObject(acSol3DC, true);

                acSol3DC.Dispose();
               

                tr.Commit();
            }
   
        }


        public Pair<quaternion, quaternion> GetSteelBendPoints(Bend bend)
        {
            try
            {
                return buffDictionary[bend.SolidHandle.Second].First;
            }
            catch { return null; }
        }
        public Pair<quaternion, quaternion> GetSteelBendPointsNormal(Bend bend)
        {
            try
            {
                return buffDictionary[bend.SolidHandle.Second].Second;
            }
            catch { return null; }
        }
        public quaternion GetPointProjectionToAxe(quaternion axeStart, quaternion axeEnd, quaternion p)
        {
            UCS ucs = new UCS(axeStart, axeEnd, p);
            quaternion P = ucs.FromACS(p);
            return  ucs.ToACS(new quaternion(0,P.GetX(),0,0));
        }
        #endregion

        [CommandMethod("KojtoCAD_3D", "KCAD_CALCULATE_FORMULA", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CALCULATE_FORMULA.htm", "")]
        public void KojtoCAD_3D_Calculate_Formula()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter formula (+,-,*,/,^,sin,cos,tan,ctn - angles in degree): ");
            pStrOpts.AllowSpaces = true;
            PromptResult pStrRes;

            pStrRes = ed.GetString(pStrOpts);
            if (pStrRes.Status == PromptStatus.OK)
            {
                string formula = pStrRes.StringResult;                
                try
                {
                    double q = eval.Evaluate(formula.Trim());
                    ed.WriteMessage(string.Format("Result: {0}",q));
                    MessageBox.Show(string.Format("Result: {0}", q),"CLALCULATE");
                }
                catch
                {
                }
            }
        }
    }
}
