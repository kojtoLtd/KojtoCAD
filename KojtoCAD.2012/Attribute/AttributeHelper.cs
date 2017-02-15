using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Document = Bricscad.ApplicationServices.Document;
using Application = Bricscad.ApplicationServices.Application;
#endif
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using KojtoCAD.LayoutCommands;
using KojtoCAD.Utilities;
using S = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using E = DocumentFormat.OpenXml.OpenXmlElement;
using A = DocumentFormat.OpenXml.OpenXmlAttribute;

using R = DocumentFormat.OpenXml.Spreadsheet.Row;
using C = DocumentFormat.OpenXml.Spreadsheet.Cell;

[assembly : CommandClass(typeof(KojtoCAD.Attribute.AttributeHelper))]

namespace KojtoCAD.Attribute
{
    public static class AttributeHelper
    {
        public static void ScanForBlocks(Database db, Transaction acTrans, ref ArrayList blocks,
            ref ArrayList blocksWithLessThanThreeAttribute, ref ArrayList blocksWithRepeatingTags)
        {
            blocks = new ArrayList();
            BlockTable bt = (BlockTable) acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId blockId in bt)
            {
                BlockTableRecord btr = (BlockTableRecord) acTrans.GetObject(blockId, OpenMode.ForRead);
                if (btr.IsLayout || btr.IsAnonymous)
                {
                    continue;
                }
                // if (btr.IsDynamicBlock == false)
                {
                    StringCollection tags = new StringCollection();
                    foreach (ObjectId attId in btr)
                    {
                        bool exist = false;
                        DBObject obj = (DBObject) acTrans.GetObject(attId, OpenMode.ForRead);
                        AttributeDefinition ad2 = obj as AttributeDefinition;
                        if (ad2 != null)
                        {
                            string testStr = (ad2.Tag).ToString();
                            foreach (string t in tags)
                            {
                                if (t.ToUpper() == testStr.ToUpper())
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (exist == false)
                            {
                                tags.Add(testStr);
                            }
                            else
                            {
                                tags = new StringCollection();
                                blocksWithRepeatingTags.Add(btr);
                                break;
                            }
                        }
                    }
                    if (tags.Count > 2)
                    {
                        MyBlock bk = new MyBlock(btr.Name);
                        foreach (string str in tags)
                        {
                            bk.AddAtt(str);
                        }
                        blocks.Add(bk);
                    }
                    else
                    {
                        if (tags.Count > 0)
                        {
                            bool saved_in_repeated = false;
                            foreach (string str in blocksWithRepeatingTags)
                            {
                                if (str == btr.Name)
                                {
                                    saved_in_repeated = true;
                                    break;
                                }
                            }
                            if (!saved_in_repeated)
                            {
                                //BlocksWithLessThanThreeAttribute.Add(btr);
                                MyBlock bk = new MyBlock(btr.Name);
                                foreach (string str in tags)
                                {
                                    bk.AddAtt(str);
                                }
                                blocksWithLessThanThreeAttribute.Add(bk);
                            }
                        }
                    }
                } //check for dynamic block
                //else
                //{
                //    DynamicBlocksNames.Add(btr);
                //}
            }
        }

        public static List<StringCollection> GetExelData(string fileName, string sheetName)
        {
            List<StringCollection> rez = new List<System.Collections.Specialized.StringCollection>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, false))
            {

                #region sheet number N

                S sheets = document.WorkbookPart.Workbook.Sheets;
                int N = -1;
                int i = 0;
                foreach (E st in sheets)
                {
                    foreach (A attr in st.GetAttributes())
                    {
                        if (attr.LocalName == "name")
                        {
                            if (attr.Value == sheetName)
                            {
                                N = i;
                            }
                            break;
                        }

                    }
                    if (N >= 0)
                    {
                        break;
                    }
                    i++;
                }

                #endregion

                Sheet sheet = document.WorkbookPart.Workbook.Descendants<Sheet>().ElementAt<Sheet>(N);
                Worksheet worksheet = ((WorksheetPart) document.WorkbookPart.GetPartById(sheet.Id)).Worksheet;
                IEnumerable<R> allRows = worksheet.GetFirstChild<SheetData>().Descendants<R>();


                WorkbookPart workbookPart = document.WorkbookPart;
                WorksheetPart worksheetPart = (WorksheetPart) workbookPart.GetPartById(sheet.Id);
                SharedStringTablePart sharedStringPart = workbookPart.SharedStringTablePart;
                IEnumerable<SharedStringItem> table = sharedStringPart.SharedStringTable.Elements<SharedStringItem>();

                List<string> PF = new List<string>();
                foreach (SharedStringItem fd in sharedStringPart.SharedStringTable)
                {
                    PF.Add(fd.InnerText);
                }

                // int NN = 0;
                foreach (R currentRow in allRows)
                {
                    // if (NN > 1000) { break; }
                    StringCollection strcoll = new System.Collections.Specialized.StringCollection();

                    IEnumerable<C> allCells = currentRow.Descendants<C>();
                    foreach (C currentCell in allCells)
                    {
                        try
                        {
                            string data = null;
                            if (currentCell.DataType != null && currentCell.DataType.Value == CellValues.SharedString)
                            {
                                int index = int.Parse(currentCell.CellValue.Text);
                                data = PF[index]; //table.ElementAt(index).InnerText;
                            }
                            else
                            {
                                data = currentCell.CellValue.Text;
                            }
                            if ((data != null) && (data.Length > 0))
                            {
                                strcoll.Add(data);
                            }

                        }
                        catch
                        {

                        }
                    }
                    if (strcoll.Count > 0)
                    {
                        rez.Add(strcoll);
                    }
                    //NN++;
                }
            }
            return rez;
        }

        public static void CheckAttribute(string drawingFullName, string blockName, string tag,
            ref List<StringCollection> excelData, ref StringCollection logFileContents, int existcolor,
            int notesistcolor, bool checdigut, bool delimiterISpoint)
        {

            Document doc = null;
            bool document_is_open = false;

            #region проверяваме дали документа е отворен

            foreach (Document dd in Application.DocumentManager)
            {
                if (dd.Name == drawingFullName)
                {
                    document_is_open = true;
                    doc = dd;
                    Application.DocumentManager.MdiActiveDocument = doc;
                    break;
                }
            }
            if (!document_is_open)
            {
                //doc = Application.DocumentManager.Open(drawingFullName, false);
                doc = new UtilityClass().ReadDocument(drawingFullName);
            }
           
            #endregion

            bool existBLOCKREF = false;
            String LOGSTRING = "";
            using (DocumentLock DocLock = doc.LockDocument())
            {
                Database db = doc.Database;
                using (Transaction acTrans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable) acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
                    foreach (ObjectId blockId in bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord) acTrans.GetObject(blockId, OpenMode.ForRead);
                        if (btr.IsLayout || btr.IsAnonymous)
                        {
                            continue;
                        }
                        if (btr.Name == blockName)
                        {
                            ObjectIdCollection coll = GetBlockReferenceIds(blockId);
                            //---------------------------------------------------
                            List<List<Pair<string, ObjectId>>> pre = GetRefLayout(ref coll, ref db);
                            int NN = 0;
                            foreach (List<Pair<string, ObjectId>> jjo in pre)
                            {
                                foreach (Pair<string, ObjectId> rts in jjo)
                                {
                                    // MessageBox.Show(NN.ToString()+"\n"+rts.First);
                                }
                                NN++;
                            }
                            //-----------------------------------------------------
                            if (coll.Count > 0)
                            {
                                existBLOCKREF = true;
                            }
                            //foreach (ObjectId obid in coll)
                            string temp = "";
                            String LOGS = "";
                            String LOGS1 = "";
                            String LOGS2 = "";
                            foreach (List<Pair<string, ObjectId>> list in pre)
                            {
                                if (list[0].First != "-1")
                                {
                                    temp = "Layer: " + list[0].First + "- Attribute content is\r\n";
                                }
                                else
                                {
                                    //temp = "Layer:  - Attribute content is\r\n";
                                    continue;
                                }

                                foreach (Pair<string, ObjectId> PA in list)
                                {
                                    BlockReference brf =
                                        acTrans.GetObject(PA.Second, OpenMode.ForWrite) as BlockReference;
                                    bool existATTR = false;
                                    for (int i = 0; i < brf.AttributeCollection.Count; i++)
                                    {
                                        AttributeReference AttRef =
                                            (AttributeReference)
                                                acTrans.GetObject(brf.AttributeCollection[i], OpenMode.ForWrite);
                                        if (AttRef.Tag.ToString() == tag)
                                        {
                                            existATTR = true;
                                            if (((AttributeReference) AttRef).TextString.Length > 0)
                                            {
                                                if (checdigut)
                                                {
                                                    bool digit = true;
                                                    string sr = ((AttributeReference) AttRef).TextString;
                                                    char ch = delimiterISpoint ? '.' : ',';
                                                    if (sr.IndexOf(ch) == sr.LastIndexOf(ch))
                                                    {
                                                        foreach (char c in sr)
                                                        {
                                                            if (!Char.IsDigit(c) && (c != ch))
                                                            {
                                                                digit = false;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        digit = false;
                                                    }

                                                    if (!digit)
                                                    {
                                                        LOGS2 += "\"" + ((AttributeReference) AttRef).TextString +
                                                                 "\"  in BlockReference Position" +
                                                                 brf.Position.ToString() + " not a Number.\r\n";
                                                    }
                                                }

                                                if (excelData != null)
                                                {
                                                    Pair<int, int> pa =
                                                        SearchStrigInEXcelData(
                                                            ((AttributeReference) AttRef).TextString, ref excelData);
                                                    if ((pa.First < 0) || (pa.Second < 0))
                                                    {
                                                        ((AttributeReference) AttRef).ColorIndex = notesistcolor;
                                                        LOGS1 += "\"" + ((AttributeReference) AttRef).TextString +
                                                                 "\"  in BlockReference Position" +
                                                                 brf.Position.ToString() +
                                                                 "is not found in the Excel file\r\n";
                                                    }
                                                    else
                                                    {
                                                        ((AttributeReference) AttRef).ColorIndex = existcolor;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                LOGS += "TAG is Empty in BlockReference Position" +
                                                        brf.Position.ToString() + "\r\n";
                                            }

                                            break;
                                        }
                                    }

                                    if (!existATTR)
                                    {
                                        LOGS += "TAG Missin in BlockReference Position " + brf.Position.ToString() +
                                                "\r\n";
                                    }
                                }

                                if (LOGS.Length > 0)
                                {
                                    LOGSTRING += temp + LOGS;
                                }

                                if (!(LOGS.Length > 0))
                                {
                                    if (LOGS1.Length > 0)
                                    {
                                        LOGSTRING += temp + LOGS1;
                                    }
                                }
                                else
                                {
                                    if (LOGS1.Length > 0)
                                    {
                                        LOGSTRING += LOGS1;
                                    }
                                }

                                if (!(LOGS.Length > 0) && !(LOGS1.Length > 0))
                                {
                                    if (LOGS2.Length > 0)
                                    {
                                        LOGSTRING += temp + LOGS2;
                                    }
                                }
                                else
                                {
                                    if (LOGS2.Length > 0)
                                    {
                                        LOGSTRING += LOGS2;
                                    }
                                }

                                LOGS = string.Empty;
                                LOGS1 = string.Empty;
                                LOGS2 = string.Empty;
                            }
                        }
                    }

                    acTrans.Commit();
                }
            }

            if (!existBLOCKREF)
            {
                LOGSTRING = "In File " /* + DrawingFullName + */+ " missing BlockReference !\r\n";
            }

            if (LOGSTRING.Length > 0)
            {
                LOGSTRING = drawingFullName + ":\r\n" + LOGSTRING;
            }

            if (LOGSTRING.Length > 0)
            {
                logFileContents.Add(LOGSTRING);
            }

            try
            {
                if (!document_is_open)
                {
                    //doc.CloseAndSave(drawingFullName);
                    new UtilityClass().CloseActiveDocument(true);
                }
                else
                {
                    doc.SendStringToExecute("_qsave ", false, false, false);
                }
            }
            catch
            {
                // Why is that?
            }

        }

        public static ObjectIdCollection GetBlockReferenceIds(ObjectId BtrId)
        {
            if (BtrId.IsNull)
            {
                throw new ArgumentException("null object id");
            }

            ObjectIdCollection result = new ObjectIdCollection();
            using (Transaction transaction = BtrId.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTableRecord blockTableRecord =
                        transaction.GetObject(BtrId, OpenMode.ForRead) as BlockTableRecord;
                    if (blockTableRecord != null)
                    {
                        // Add the ids of all references to the BTR. Some dynamic blocks may
                        // reference the dynamic block directly rather than an anonymous block,
                        // so this will get those as well:
                        ObjectIdCollection BrefIds = blockTableRecord.GetBlockReferenceIds(true, false);
                        if (BrefIds != null)
                        {
                            foreach (ObjectId Id in BrefIds)
                                result.Add(Id);
                        }

                        // if this is not a dynamic block, we're done:

                        if (!blockTableRecord.IsDynamicBlock)
                            return result;

                        // Get the ids of all anonymous block table records for the dynamic block

                        ObjectIdCollection anonBtrIds = blockTableRecord.GetAnonymousBlockIds();
                        if (anonBtrIds != null)
                        {
                            foreach (ObjectId anonBtrId in anonBtrIds)
                            {
                                // get all references to each anonymous block:

                                BlockTableRecord rec =
                                    transaction.GetObject(anonBtrId, OpenMode.ForRead) as BlockTableRecord;
                                if (rec != null)
                                {
                                    BrefIds = rec.GetBlockReferenceIds(false, true);
                                    if (BrefIds != null)
                                    {
                                        foreach (ObjectId id in BrefIds)
                                            result.Add(id);
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    transaction.Commit();
                }
            }
            return result;
        }

        private static List<List<Pair<string, ObjectId>>> GetRefLayout(ref ObjectIdCollection coll, ref Database db)
        {
            // Database db = HostApplicationServices.WorkingDatabase;
            List<Pair<string, ObjectId>> PRE = new List<Pair<string, ObjectId>>();
            foreach (ObjectId ID in coll)
            {
                PRE.Add(new Pair<string, ObjectId>("-1", ID));
            }

            List<List<Pair<string, ObjectId>>> REZ = new List<List<Pair<string, ObjectId>>>();

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                foreach (ObjectId btrId in acBlkTbl)
                {
                    BlockTableRecord btr = (BlockTableRecord) acTrans.GetObject(btrId, OpenMode.ForWrite);
                    if (btr.IsLayout)
                    {
                        ObjectIdCollection anIds = btr.GetAnonymousBlockIds();

                        List<Pair<string, ObjectId>> mas = new List<Pair<string, ObjectId>>();
                        Layout lo = (Layout) acTrans.GetObject(btr.LayoutId, OpenMode.ForWrite);

                        foreach (Pair<string, ObjectId> ID in PRE)
                        {
                            if (ID.First == "-1")
                            {
                                BlockReference ghbref = acTrans.GetObject(ID.Second, OpenMode.ForRead) as BlockReference;
                                if (ExistObjectIDInLayout(ref btr, ID.Second))
                                {
                                    mas.Add(new Pair<string, ObjectId>(lo.LayoutName, ID.Second));
                                }
                            }
                        }

                        if (mas.Count > 0) REZ.Add(mas);
                    }
                }
            }
            List<Pair<string, ObjectId>> Mas = new List<Pair<string, ObjectId>>();
            foreach (Pair<string, ObjectId> ID in PRE)
            {
                if (ID.First == "-1")
                {
                    Mas.Add(ID);
                }
            }
            if (Mas.Count > 0)
            {
                REZ.Add(Mas);
            }

            return REZ;
        }

        private static bool ExistObjectIDInLayout(ref BlockTableRecord btr, ObjectId ID)
        {
            bool rez = false;

            foreach (ObjectId ObjId in btr)
            {
                if ((ID == ObjId))
                {
                    rez = true;
                    break;
                }
            }

            return rez;
        }

        public static StringCollection GetSheetInfo(string fileName)
        {
            var stringCollection = new StringCollection();
            // Open file as read-only.
            using (SpreadsheetDocument mySpreadsheet = SpreadsheetDocument.Open(fileName, false))
            {
                S sheets = mySpreadsheet.WorkbookPart.Workbook.Sheets;
                // For each sheet, display the sheet information.
                foreach (E sheet in sheets)
                {
                    foreach (A attr in sheet.GetAttributes())
                    {
                        //Console.WriteLine("{0}: {1}", attr.LocalName, attr.Value);
                        if (attr.LocalName == "name")
                        {
                            stringCollection.Add(attr.Value);
                            break;
                        }

                    }
                }
            }

            return stringCollection;
        }

        public static string GetFolder()
        {
            string SelectedFolder = "";
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SelectedFolder = folderBrowserDialog.SelectedPath;
            }
            return SelectedFolder + "\\";
        }

        private static string[] ConvertToStringArray(Array values)
        {

            // create a new string array
            string[] theArray = new string[values.Length];

            // loop through the 2-D System.Array and populate the 1-D String Array
            for (int i = 1; i <= values.Length; i++)
            {
                if (values.GetValue(1, i) == null)
                    theArray[i - 1] = "";
                else
                    theArray[i - 1] = (string) values.GetValue(1, i).ToString();
            }

            return theArray;
        }

        private static Pair<int, int> SearchStrigInEXcelData(string str, ref List<StringCollection> excelData)
        {
            Pair<int, int> rez = new Pair<int, int>(-1, -1);

            for (int i = 0; i < excelData.Count; i++)
            {

                for (int j = 0; j < excelData[i].Count; j++)
                {
                    if ((excelData[i])[j] == str)
                    {
                        rez = new Pair<int, int>(i, j);
                        return rez;
                    }

                }
            }

            return rez;
        }
    }
}   