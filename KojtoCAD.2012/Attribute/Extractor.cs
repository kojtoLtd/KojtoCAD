using ExcelWriter;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Bricscad.EditorInput;
#endif
#if acad2013
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
#if acad2012
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#endif
#if bcad
using Application = Bricscad.ApplicationServices.Application;
#endif
[assembly: CommandClass(typeof(KojtoCAD.Attribute.Extractor))]

namespace KojtoCAD.Attribute
{
    public class Extractor
    {

        /// <summary>
        /// Extract attributes
        /// </summary>
        [CommandMethod("ea")]
        public void ExtractAttributesStart()
        {
            #region Init local vars
            List<Block> OriginalBlockDefs = new List<Block>();
            List<Block> OriginalBlockRefs = new List<Block>();
            SortedList<string, double> AttsTotals = new SortedList<string, double>();

            ObjectIdCollection OrigBlockRefsIds = new ObjectIdCollection();
            ObjectIdCollection OrigBlockDefsIds = new ObjectIdCollection();

            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Database Db = Doc.Database;
            Editor Ed = Doc.Editor;

            char[] badChars = { '\'', ' ', '-', '`', '~', '"', '_' };

            string dateFormat = "yyyy.MM.dd_HH.mm.ss";

            string[] stringSeparators = new string[] { "\\" };
            string[] fileNameExploded = Db.Filename.Split(stringSeparators, StringSplitOptions.None);

            string outputFileName = fileNameExploded[fileNameExploded.Length - 2] + "." + DateTime.Now.ToString(dateFormat) + ".xls";
            string outputDir = Db.Filename.Remove(Db.Filename.LastIndexOf("\\") + 1);

            ExcelDocument document = new ExcelDocument();
            document.UserName = Environment.UserName;
            document.CodePage = CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
            document.ColumnWidth(0, 120);
            document.ColumnWidth(1, 80);
            int XLSrows = 0;
            int XLScols = 0;

            document.WriteCell(XLSrows, 4, fileNameExploded[fileNameExploded.Length - 2]);
            XLSrows += 3;
            #endregion

            #region Prompt user for BlockRefs and dwg file/dir opts

            PromptEntityOptions PromptForBlocksOpts = new PromptEntityOptions("\nSelect single block");
            PromptForBlocksOpts.SetRejectMessage("\nYou can select blocks only.");
            PromptForBlocksOpts.AddAllowedClass(typeof(BlockReference), true);
            PromptForBlocksOpts.AllowNone = true;
            PromptEntityResult PromptForBlocksRes = Ed.GetEntity(PromptForBlocksOpts);
            if (PromptForBlocksRes.Status == PromptStatus.Cancel)
            {
                return;
            }

            // Prompt the user for blocks
            while (PromptForBlocksRes.Status == PromptStatus.OK)
            {
                using (Transaction Tr = Db.TransactionManager.StartTransaction())
                {
                    BlockReference Bref = (BlockReference)Tr.GetObject(PromptForBlocksRes.ObjectId, OpenMode.ForRead);
                    Bref.Highlight();         // highlight the selected BlockReference
                    OrigBlockRefsIds.Add(PromptForBlocksRes.ObjectId);

                    // extract the BlockTableRecord from the BlockReference
                    BlockTableRecord Btr = (BlockTableRecord)Tr.GetObject(Bref.BlockTableRecord, OpenMode.ForRead);

                    bool BlockIsAlreadyIn = false;
                    foreach (Block BlockDef in OriginalBlockDefs)
                    {
                        if (BlockDef.Name == Btr.Name)
                        {
                            BlockIsAlreadyIn = true;
                        }
                    }

                    if (!BlockIsAlreadyIn)
                    {
                        StringCollection AttributeTags = new StringCollection();
                        StringBuilder Atts = new StringBuilder();

                        foreach (ObjectId ObjId in Btr)
                        {
                            AttributeDefinition attDef = ObjId.GetObject(OpenMode.ForRead) as AttributeDefinition;
                            if (attDef != null)
                            {
                                Atts.Append(attDef.Tag + ";");
                                AttributeTags.Add(attDef.Tag);
                            }
                        }

                        if (AttributeTags.Count > 0)
                        {
                            OriginalBlockDefs.Add(new Block(Btr.Name));
                            OriginalBlockDefs[OriginalBlockDefs.Count - 1].AttributeTags = AttributeTags;

                            XLScols = 1;
                            foreach (string AttTag in AttributeTags)
                            {
                                document.WriteCell(XLSrows, XLScols, AttTag);
                                XLScols++;
                            }
                            XLSrows++;
                        }
                        else   // If a Block Def does not contain AttributeDefs - it is excluded
                        {
                            //ProcLogger.WriteToLog( "Block Definition ;" + Btr.Name + " was excluded becase it has no attributes" );
                        }
                    }
                    else
                    {
                        //ProcLogger.WriteToLog( "Block Definition not imported : ; already in." );
                    }
                    Tr.Commit();
                }

                #region If the user presses cancel in the middle
                //unhighlight all Block References and flush the collections

                PromptForBlocksRes = Ed.GetEntity(PromptForBlocksOpts);
                if (PromptForBlocksRes.Status == PromptStatus.Cancel)
                {
                    using (Transaction Tr = Db.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId BlockRefId in OrigBlockRefsIds)
                        {
                            BlockReference Bref = (BlockReference)Tr.GetObject(BlockRefId, OpenMode.ForWrite);
                            Bref.Unhighlight();
                            Bref.DowngradeOpen();
                        }

                        OriginalBlockDefs.Clear();
                        OriginalBlockRefs.Clear();

                        Tr.Commit();
                    }
                    return;
                }
                #endregion
            }

            #region Unhighlight all entities and get their BlockTableRecords ObjectIds
            using (Transaction Tr = Db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId BlockRefId in OrigBlockRefsIds)
                {
                    BlockReference Bref = (BlockReference)Tr.GetObject(BlockRefId, OpenMode.ForRead);
                    Bref.Unhighlight();
                }
                Tr.Commit();
            }
            #endregion

            #region prompt for file or dir option
            PromptKeywordOptions KeywordsOpts = new PromptKeywordOptions("Scan current DWG or all files in DWGs dir?");
            KeywordsOpts.AllowArbitraryInput = false;
            KeywordsOpts.AllowNone = false;
            KeywordsOpts.Keywords.Add("File");
            KeywordsOpts.Keywords.Add("Dir");
            PromptResult FileOrDirRes = Ed.GetKeywords(KeywordsOpts);
            // If the user pressed cancel - return with no error
            if (FileOrDirRes.Status != PromptStatus.OK)
            {
                return;
            }

            List<string> Files = new List<string>();
            string[] tempFiles;
            if (FileOrDirRes.StringResult == "Dir")
            {
                string currFile = Db.Filename;
                string currDir = currFile.Remove(currFile.LastIndexOf("\\") + 1);
                tempFiles = Directory.GetFiles(currDir, "*.dwg", SearchOption.AllDirectories);
                foreach (string tempFile in tempFiles)
                {
                    Files.Add(tempFile);
                }
            }
            else
            {
                Files.Add(Db.Filename);
            }

            // return;
            #endregion

            #endregion

            #region Traverse Dwgs and extract raw data
            Database UnopenedDb;

            Files.Sort();
            // Open every file
            foreach (string file in Files)
            {
                document.WriteCell(XLSrows, 0, file.Remove(0, file.LastIndexOf("\\") + 1));

                if (!file.EndsWith(".dwg", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                // Open the Db from the file
                using (UnopenedDb = new Database(false, false))
                {
                    if (Db.Filename != file)
                    {
                        UnopenedDb.ReadDwgFile(file, FileShare.Read, true, "");
                    }
                    else
                    {
                        UnopenedDb = Db;
                    }

                    List<string> AppendedTags = new List<string>();
                    List<StringCollection> AttributeValues = new List<StringCollection>();
                    // open transaction to the db
                    using (Transaction Tr = UnopenedDb.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            // Get the BlockTable
                            BlockTable Bt = (BlockTable)Tr.GetObject(UnopenedDb.BlockTableId, OpenMode.ForRead);

                            // Traverse all the layouts for Block References
                            foreach (ObjectId ObjId in Bt)
                            {
                                BlockTableRecord LayoutBtr = (BlockTableRecord)Tr.GetObject(ObjId, OpenMode.ForRead);
                                if (LayoutBtr.IsLayout)
                                {
                                    Layout currLayout = (Layout)Tr.GetObject(LayoutBtr.LayoutId, OpenMode.ForRead);
                                    if (!currLayout.LayoutName.Contains("Model"))
                                    {
                                        document.WriteCell(XLSrows++, 0, currLayout.LayoutName);
                                    }

                                    foreach (ObjectId LayoutObjId in LayoutBtr)
                                    {
                                        BlockReference Bref = Tr.GetObject(LayoutObjId, OpenMode.ForRead) as BlockReference;   // Dont tuch this!                        
                                        if (Bref != null)
                                        {
                                            StringCollection AttRefValuesForXLS = new StringCollection();
                                            foreach (Block BDef in OriginalBlockDefs)
                                            {
                                                if (Bref.Name == BDef.Name)
                                                {
                                                    for (int i = 0; i < Bref.AttributeCollection.Count; i++)
                                                    {
                                                        AttributeReference AttRef = (AttributeReference)Tr.GetObject(Bref.AttributeCollection[i], OpenMode.ForRead);

                                                        AttRefValuesForXLS.Add(AttRef.TextString.Trim(badChars).Trim());
                                                        continue;
                                                    }
                                                    AttributeValues.Add(AttRefValuesForXLS);
                                                }
                                            }
                                        }
                                    }

                                    #region // bubble sort the attributes by PartNr

                                    string tempAA = "";
                                    string tempBB = "";
                                    int tempA = 0;
                                    int tempB = 0;
                                    bool parseAIsOk = false;
                                    bool parseBIsOk = false;
                                    Match MatchA;
                                    Match MatchB;
                                    //string AlphaPattern = @"[a-z]|[A-Z]";
                                    string NumericPattern = @"[0-9]+";


                                    try
                                    {
                                        if (AttributeValues.Count > 1)
                                        {
                                            for (int j = 0; j < AttributeValues.Count; j++)
                                            {
                                                for (int i = 1; i < AttributeValues.Count; i++)
                                                {
                                                    tempBB = AttributeValues[i][0] = AttributeValues[i][0];
                                                    tempAA = AttributeValues[i - 1][0] = AttributeValues[i - 1][0];

                                                    MatchA = Regex.Match(tempAA, NumericPattern);
                                                    MatchB = Regex.Match(tempBB, NumericPattern);

                                                    parseAIsOk = Int32.TryParse(MatchA.ToString(), out tempA);
                                                    parseBIsOk = Int32.TryParse(MatchB.ToString(), out tempB);

                                                    if (parseAIsOk && parseBIsOk)
                                                    {
                                                        if (tempA > tempB)
                                                        {
                                                            StringCollection temp = AttributeValues[i];
                                                            AttributeValues[i] = AttributeValues[i - 1];
                                                            AttributeValues[i - 1] = temp;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (System.Exception Ex)
                                    {
                                        Ed.WriteMessage(Ex.Message + Ex.Source + Ex.StackTrace);
                                    }

                                    #endregion

                                    #region Parse and write the attribute collection to XLS file


                                    try
                                    {
                                        foreach (StringCollection BlockRefAttValues in AttributeValues)
                                        {

                                            if (!AttsTotals.Keys.Contains(BlockRefAttValues[4]))
                                            {
                                                double value;
                                                if (Double.TryParse(BlockRefAttValues[2], out value))
                                                {
                                                    AttsTotals.Add(BlockRefAttValues[4], value);
                                                }
                                                else
                                                {
                                                    AttsTotals.Add(BlockRefAttValues[4], 0);
                                                }

                                            }
                                            else
                                            {
                                                double value;
                                                double z;
                                                if (Double.TryParse(BlockRefAttValues[2], out value))
                                                {
                                                    z = value;
                                                }
                                                else
                                                {
                                                    z = 0;
                                                }
                                                double x;

                                                if (Double.TryParse(BlockRefAttValues[4], out value))
                                                {
                                                    x = value;
                                                }
                                                else
                                                {
                                                    x = 0;
                                                }

                                                //double total = AttsTotals[BlockRefAttValues[4]];
                                                //AttsTotals[BlockRefAttValues[4]] = total + z + x;
                                                AttsTotals[BlockRefAttValues[4]] += z + x;
                                            }

                                            XLScols = 1;
                                            foreach (string AttVal in BlockRefAttValues)
                                            {
                                                string tmp = AttVal;
                                                double tempInt = 0;
                                                if (Double.TryParse(tmp, out tempInt))
                                                {
                                                    document.WriteCell(XLSrows, XLScols, tempInt);
                                                }
                                                else
                                                {
                                                    document.WriteCell(XLSrows, XLScols, tmp);
                                                }
                                                XLScols++;
                                            }
                                            XLSrows++;
                                        }
                                    }
                                    catch (System.Exception Ex)
                                    {
                                        MessageBox.Show(Ex.Message + "\n" + Ex.Source + "\n" + Ex.StackTrace);
                                    }
                                    #endregion
                                    AttributeValues.Clear();
                                }
                            }
                        }
                        catch (System.Exception Ex)
                        {
                            Ed.WriteMessage(Ex.Message + Ex.Source + Ex.StackTrace);
                        }
                    }
                }
                XLSrows += 3;
            }// foreach ( file in files )

            try
            {
                document.WriteCell(XLSrows, 5, "Parts");
                document.WriteCell(XLSrows, 3, "Totals");
                XLSrows++;

                foreach (KeyValuePair<string, double> Total in AttsTotals)
                {
                    //document.WriteCell(XLSrows, 5, Total.Key.ToString());
                    document.WriteCell(XLSrows, 5, Total.Key);
                    double value;
                    if (Double.TryParse(Total.Value.ToString(), out value))
                    {
                        document.WriteCell(XLSrows, 3, value);
                    }
                    else
                    {
                        document.WriteCell(XLSrows, 3, Total.Value.ToString());
                    }

                    document.WriteCell(XLSrows, 4, "Stk");
                    XLSrows++;
                }
            }
            catch (System.Exception Ex)
            {
                MessageBox.Show(Ex.Message + "\n" + Ex.Source + "\n" + Ex.StackTrace);
            }

            FileStream stream = new FileStream(outputDir + outputFileName, FileMode.Create);
            document.Save(stream);
            stream.Close();
            #endregion
        }

        public class Block
        {
            private readonly string mBlockName;

            public StringCollection AttributeTags;

            public Block(string aBlockName)
            {
                mBlockName = aBlockName;
                AttributeTags = new StringCollection();
            }

            public string Name => mBlockName;
        }
    }
}
