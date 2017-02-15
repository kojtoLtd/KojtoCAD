using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using AcadEntity = Autodesk.AutoCAD.Interop.Common.AcadEntity;
using AcadBlockReference = Autodesk.AutoCAD.Interop.Common.AcadBlockReference;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
using AcadEntity = BricscadDb.AcadEntity;
using AcadBlockReference = BricscadDb.AcadBlockReference;
#endif

[assembly:CommandClass(typeof(KojtoCAD.ObjectCopier.DrawingObjectCopier))]

namespace KojtoCAD.ObjectCopier
{
    public struct DynamicBlocksData
    {
        public string FileName;
        public string[] arr;
        public string[] ARRR;
        public Point3d Pos;
        public double xD1;
        public double yD1;
    }
    public class DynamicBlocksDataComparer : IComparer
    {
        public static IComparer Default = new DynamicBlocksDataComparer();
        public int Compare(object A, object B)
        {
            int rez;
            if (A is DynamicBlocksData && B is DynamicBlocksData)
            {
                int temp = 0;
                for (int i = 0; i < ((DynamicBlocksData)A).arr.Length; i++)
                {
                    temp = Comparer.Default.Compare(((DynamicBlocksData)A).arr[i], ((DynamicBlocksData)B).arr[i]);
                    if (temp != 0) { break; }
                }
                rez = temp;
            }
            else
            {
                throw new ArgumentException("Objet to compare to is not a  dynamicBlocksDataSTRUCT object");
            }
            return rez;
        }
    }
    public class DrawingObjectCopier
    {
        private const int Len = 20;

        private static readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        /// <summary>
        /// Copy Objects Between DWGs
        /// </summary>
        [CommandMethod("COBD", CommandFlags.Session)]
        public static void CopyObjectsStart()
        {
            
            string slectMODE = PromptSTR();                         //полочаваме начина на селектиране
            string[] MASK = PromptStr();                            //получаваме от промпта маската за сортиране
            string separate = PromptSTRR();
            if (MASK.Length < 1) { return; }
            ArrayList LIST = new ArrayList();                       // съхранява данните за динамичните блокове и чертежите
            StringCollection flesNames = GetFilesNamesCollection(); // създава списък от файлове
            string outfName = GetSaveFileName();                    // получаваме името на файла за електронната таблица
            GetPreData(ref flesNames, ref LIST);                    // прочита фаиловете от списъка и вади данните на динамичните блокове и името на чертежа     
            #region  подготовка за сортиране
            foreach (DynamicBlocksData dstr in LIST)
            {
                for (int i = 0; i < dstr.arr.Length; i++)
                {
                    if (i < MASK.Length)
                    {
                        if (!MASK[i].Contains("*")) { dstr.arr[i] = ""; }
                    }
                    else
                    {
                        dstr.arr[i] = "";
                    }
                }
            }
            if (separate == "Full")
            {
                ArrayList LIST1 = new ArrayList();
                foreach (DynamicBlocksData dstr in LIST)
                {
                    bool erase = false;
                    for (int i = 0; i < MASK.Length; i++)
                    {
                        if (MASK[i].Contains("*"))
                        {
                            if (dstr.arr[i] == "")
                            {
                                erase = true;
                                break;
                            }
                        }
                    }
                    if (erase != true) { LIST1.Add(dstr); }
                }
                LIST = new ArrayList();
                foreach (DynamicBlocksData dstr in LIST1)
                {
                    LIST.Add(dstr);
                }
            }
            #endregion
            LIST.Sort(DynamicBlocksDataComparer.Default);           //сортираме по материал, тип, дебелина .....                     
            CopyItems(ref LIST, slectMODE, separate);               //цопиране в отворения чертожен файл            
            SaveFile(ref LIST, outfName);                           //запис във  csv file           
        }

        private static string PromptSTRR()
        {
            string rez = "";
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter Save mode option ";
            pKeyOpts.Keywords.Add("Together");
            pKeyOpts.Keywords.Add("Separate");
            pKeyOpts.Keywords.Add("Full match");
            pKeyOpts.AllowNone = false;

            PromptResult pKeyRes = Ed.GetKeywords(pKeyOpts);
            rez = pKeyRes.StringResult;

            return rez;
        }
        private static string PromptSTR()
        {
            string rez = "";
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter selection mode option ";
            pKeyOpts.Keywords.Add("Window");
            pKeyOpts.Keywords.Add("CrossingWindow");
            pKeyOpts.AllowNone = false;

            PromptResult pKeyRes = Ed.GetKeywords(pKeyOpts);
            rez = pKeyRes.StringResult;

            return rez;
        }
        private static string[] PromptStr()
        {
            string rez = "";

            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter your mask <*,*,*,-,-,*,-.....>: ");
            pStrOpts.AllowSpaces = true;
            PromptResult pStrRes = Application.DocumentManager.MdiActiveDocument.Editor.GetString(pStrOpts);
            if (pStrRes.Status != PromptStatus.OK)
            {
                return new string[0];
            }

            rez = pStrRes.StringResult;
            return rez.Split(',');
        }
        private static StringCollection GetFilesNamesCollection()
        {
            StringCollection filesNames = new StringCollection();
            string SelectedFolder = "";
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SelectedFolder = folderBrowserDialog.SelectedPath;
            }

            string[] files = null;
            try
            {
                files = Directory.GetFiles(SelectedFolder);
                Array.Sort(files);
                foreach (string file in files)
                {
                    if (file.Contains(".dwg"))
                    {
                        filesNames.Add(file);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Files List Error");
            }

            return filesNames;
        }
        private static bool OpenNewDocument(string Name)
        {
            try
            {
                //Application.DocumentManager.Open(Name);
                new UtilityClass().ReadDocument(Name);
                return true;
            }
            catch
            {
                try
                {
                    PromptOpenFileOptions opts = new PromptOpenFileOptions("Select a drawing file (for a custom command)");
                    opts.Filter = "Drawing (*.dwg)";
                    PromptFileNameResult pr = Application.DocumentManager.MdiActiveDocument.Editor.GetFileNameForOpen(opts);
                    if (pr.Status == PromptStatus.OK)
                    {
                        //Application.DocumentManager.Open(pr.StringResult);
                        new UtilityClass().ReadDocument(pr.StringResult);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
        private static void PreData(ref ArrayList list, string Name)
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = Doc.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = tr.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId asObjId in acBlkTblRec)
                {
                    Entity ent = asObjId.GetObject(OpenMode.ForRead) as Entity;
                    AcadEntity En = (AcadEntity)ent.AcadObject;
                    if (En.ObjectName == "AcDbBlockReference")
                    {
                        BlockReference bref = tr.GetObject(ent.ObjectId, OpenMode.ForRead) as BlockReference;
                        AcadBlockReference BR = (AcadBlockReference)bref.AcadObject;

                        if ((BR.EffectiveName == "PSYMB_EXTRACT_DATA_2007") || (BR.EffectiveName == "PSYMB_EXTRACT_DATA"))
                        {
                            DynamicBlocksData dstr = new DynamicBlocksData();
                            dstr.Pos = bref.Position;
                            dstr.arr = new string[Len];
                            dstr.ARRR = new string[Len];
                            for (int i = 0; i < Len; i++) { dstr.arr[i] = ""; dstr.ARRR[i] = ""; }
                            foreach (DynamicBlockReferenceProperty attId in bref.DynamicBlockReferencePropertyCollection)
                            {
                                if (attId.PropertyName == "X Distance1")
                                {
                                    dstr.xD1 = (double)attId.Value;
                                }
                                if (attId.PropertyName == "Y Distance1")
                                {
                                    dstr.yD1 = (double)attId.Value;
                                }
                            }

                            foreach (ObjectId attId in bref.AttributeCollection)
                            {
                                AttributeReference attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                                if (attRef.Tag == "DESCRIPTION")
                                {
                                    dstr.FileName = Name;
                                    string[] desarr = attRef.TextString.Split(',');
                                    for (int j = 0; j < desarr.Length; j++)
                                    {
                                        if (j < Len)
                                        {
                                            dstr.arr[j] = desarr[j];
                                            dstr.arr[j] = dstr.arr[j].TrimEnd(' ');
                                            dstr.arr[j] = dstr.arr[j].TrimStart(' ');
                                            dstr.ARRR[j] = dstr.arr[j];
                                        }
                                        else { break; }
                                    }
                                    list.Add(dstr);

                                }
                            }
                        }
                    }
                }
            }
        }
        private static void GetPreData(ref StringCollection flesNames, ref ArrayList LIST)
        {
            foreach (string str in flesNames)
            {
                OpenNewDocument(str);
                DocumentLock loc = Application.DocumentManager.MdiActiveDocument.LockDocument();
                using (loc)
                {
                    PreData(ref LIST, str);
                }
                //Application.DocumentManager.MdiActiveDocument.CloseAndDiscard();
                new UtilityClass().CloseActiveDocument(false);
            }
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("ERASE ALL  ", false, false, false);
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("UNDO 1 ", false, false, false);
        }

        private static void CopyItems(ref ArrayList LIST, string sm, string spm)
        {
            double DY = 0; //определя позицията за вмъкване
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;
            string oldSTR = "";//ползва се при сепарирането пази предишнч резултат от маската с което се сравнява 
            string oldSTRR = "";
            int counter = 0;   //брой за да се отчете последния елемент и да има запис без резултат от сравняване
            foreach (DynamicBlocksData dstr in LIST)
            {
                #region SEPARATE
                try
                {
                    string str1 = "";
                    string str11 = "";
                    string pathh = dstr.FileName.Remove(dstr.FileName.LastIndexOf('\\') + 1);
                    for (int i = 0; i < dstr.arr.Length; i++)
                    {
                        str1 += dstr.arr[i];
                        str11 += dstr.ARRR[i]; str11 += "_";
                        if (str1.Length > 0)
                        {
                            if (str1[str1.Length - 1] != '_') { str1 += "_"; }
                        }
                    }
                    if (counter == 0)
                    {
                        oldSTR = str1;
                        oldSTRR = str11;
                    }
                    else
                    {
                        if ((oldSTR != str1) || (counter == (LIST.Count - 1)))
                        {
                            if (spm == "Together")
                            {
                                if (counter != (LIST.Count - 1))
                                {
                                    DY += 200;
                                }
                            }
                            else
                            {
                                string str2 = oldSTR.TrimEnd('_');
                                if ((str2 == "") || (str2 == " ") || (str2 == "  "))
                                {
                                    str2 = "OTHER_" + counter.ToString();
                                }
                                str2 = pathh + str2 + ".dwg";
                                DocumentLock locch = Doc.LockDocument();
                                using (locch)
                                {
                                    Db.SaveAs(str2, DwgVersion.Current);
                                }
                                if (counter != (LIST.Count - 1))
                                {
                                    EraseAll();
                                    DY = 0;
                                }
                            }
                            oldSTR = str1;
                            oldSTRR = str11;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Separate ERROR !");
                }
                #endregion

                ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                OpenNewDocument(dstr.FileName);
                Document DocO = Application.DocumentManager.MdiActiveDocument;
                Database DbO = Application.DocumentManager.MdiActiveDocument.Database;
                Editor EdO = Application.DocumentManager.MdiActiveDocument.Editor;
                Point3d minP = new Point3d(dstr.Pos.X - 1, dstr.Pos.Y - 1, 0);
                Point3d maxP = new Point3d(dstr.Pos.X + dstr.xD1 + 1, dstr.Pos.Y + dstr.yD1 + 1, 0);

                PromptSelectionResult prrr = null;
                if (sm == "Window")
                {
                    prrr = EdO.SelectWindow(minP, maxP);
                }
                else
                {
                    Point3d minP1 = new Point3d(dstr.Pos.X + dstr.xD1 + 0.2, dstr.Pos.Y - 0.2, 0);
                    Point3d maxP1 = new Point3d(dstr.Pos.X - 0.2, dstr.Pos.Y + dstr.yD1 + 0.2, 0);
                    Point3dCollection pcd = new Point3dCollection();
                    pcd.Add(minP); pcd.Add(minP1); pcd.Add(maxP); pcd.Add(maxP1);
                    prrr = EdO.SelectCrossingPolygon(pcd);
                }
                if (prrr.Status == PromptStatus.OK)
                {
                    SelectionSet acSSett = prrr.Value;
                    acObjIdColl = new ObjectIdCollection(acSSett.GetObjectIds());
                }
                else
                {

                    continue;
                }
                DocumentLock locc = DocO.LockDocument();
                using (locc)
                {
                    using (Transaction trr = DocO.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId ID in acObjIdColl)
                        {
                            Entity ent = trr.GetObject(ID, OpenMode.ForWrite) as Entity;
                            Point3d f = new Point3d(0, 0, 0);
                            Point3d t = new Point3d(dstr.Pos.X, dstr.Pos.Y - DY, dstr.Pos.Z);
                            Vector3d acVec3d = t.GetVectorTo(f);
                            try
                            {
                                ent.TransformBy(Matrix3d.Displacement(acVec3d));
                            }
                            catch { MessageBox.Show("Transform Coordinates ERROR !"); }
                        }

                        trr.Commit();
                    }
                    using (Transaction trr = DocO.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId asObjId in acObjIdColl)
                        {
                            Entity ent = trr.GetObject(asObjId, OpenMode.ForWrite) as Entity;
                            AcadEntity En = (AcadEntity)ent.AcadObject;
                            if (En.ObjectName == "AcDbBlockReference")
                            {
                                BlockReference bref = trr.GetObject(ent.ObjectId, OpenMode.ForRead) as BlockReference;
                                AcadBlockReference BR = (AcadBlockReference)bref.AcadObject;
                                if ((BR.EffectiveName == "PSYMB_EXTRACT_DATA_2007") || (BR.EffectiveName == "PSYMB_EXTRACT_DATA"))
                                {
                                    foreach (ObjectId attId in bref.AttributeCollection)
                                    {
                                        AttributeReference attRef = (AttributeReference)trr.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                                        if (attRef.Tag == "DESCRIPTION")
                                        {

                                            {
                                                string[] path = dstr.FileName.Split('\\');
                                                try
                                                {
                                                    attRef.TextString = path[path.Length - 1] + "-" + attRef.TextString;
                                                }
                                                catch { }

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        trr.Commit();
                    }
                }
                Application.DocumentManager.MdiActiveDocument = Doc;
                DocumentLock loc = Doc.LockDocument();
                using (loc)
                {
                    using (Transaction tr = Doc.TransactionManager.StartTransaction())
                    {
                        // Open the Block table for read
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;

                        // Open the Block table record Model space for write
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        IdMapping acIdMap = new IdMapping();
                        Db.WblockCloneObjects(acObjIdColl, acBlkTblRec.ObjectId, acIdMap, DuplicateRecordCloning.Ignore, false);

                        tr.Commit();
                    }
                }
                //DocO.CloseAndDiscard();
                new UtilityClass().CloseActiveDocument(false);
                DY += (dstr.yD1 + 5);
                counter++;
            }
        }
        private static string GetSaveFileName()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "csv files (*.csv)|*.csv|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 3;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Save dynamic bloks data in File - Name, pos(X,Y,Z), width, haight, data...";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog1.FileName;
            }
            else
            {
                return "";
            }

        }
        private static void SaveFile(ref ArrayList List, string fName)
        {
            if (fName != "")
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(fName))
                    {
                        foreach (DynamicBlocksData dstr in List)
                        {
                            string rez = "";
                            rez += dstr.FileName; rez += ";";
                            rez += dstr.Pos.X.ToString(); rez += ";";
                            rez += dstr.Pos.Y.ToString(); rez += ";";
                            rez += dstr.Pos.Z.ToString(); rez += ";";
                            rez += dstr.xD1.ToString(); rez += ";";
                            rez += dstr.yD1.ToString(); rez += ";";
                            rez += ";";
                            for (int i = 0; i < dstr.ARRR.Length; i++)
                            {
                                rez += dstr.ARRR[i]; rez += ";";
                            }
                            sw.WriteLine(rez);
                        }
                    }
                }
                catch
                {
                    return;
                }
            }
        }
        private static void EraseAll()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ObjectIdCollection acObjIdColl = new ObjectIdCollection();
            PromptSelectionResult prrr = null;
            prrr = Ed.SelectAll();

            if (prrr.Status == PromptStatus.OK)
            {
                SelectionSet acSSett = prrr.Value;
                acObjIdColl = new ObjectIdCollection(acSSett.GetObjectIds());
            }
            else
            {

            }
            DocumentLock locc = Doc.LockDocument();
            using (locc)
            {
                using (Transaction trr = Doc.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId ID in acObjIdColl)
                    {
                        Entity ent = trr.GetObject(ID, OpenMode.ForWrite) as Entity;
                        ent.Erase();
                    }
                    trr.Commit();
                }
            }
        }
    }
}
