using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Castle.Core.Logging;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
#else
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
#endif
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using KojtoCAD.Utilities.Interfaces;
using KojtoCAD.Attribute;
#if !bcad
using OpenFileDialog = Autodesk.AutoCAD.Windows.OpenFileDialog;
#endif
#if acad2013
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
#if acad2012
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#endif
#if bcad
using Application = Bricscad.ApplicationServices.Application;
using OpenFileDialog = Bricscad.Windows.OpenFileDialog;

#endif

[assembly: CommandClass(typeof(KojtoCAD.Macros))]

namespace KojtoCAD
{
    public class Macros
    {
        private readonly Document _doc = Application.DocumentManager.MdiActiveDocument;
        private readonly Editor _ed = Application.DocumentManager.MdiActiveDocument.Editor;
        private readonly Database _db = Application.DocumentManager.MdiActiveDocument.Database;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IFileService _fileService;
        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        public Macros()
        {
            _fileService = IoC.ContainerRegistrar.Container.Resolve<IFileService>();
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
        }

        /// <summary>
        /// Save a drawing properly.
        /// Sets layer to 0, zooms to extends, audits, purges all, quick-saves and closes the drawing
        /// </summary>
        [CommandMethod("closedwg", CommandFlags.Session)]
        public void CloseDwgProperlyStart()
        {

#if acad2012
            object activeDocument = Application.DocumentManager.MdiActiveDocument.AcadDocument;
#endif
#if acad2013
            object activeDocument = Application.DocumentManager.MdiActiveDocument.GetAcadDocument();
#endif
#if bcad
            object activeDocument = Application.DocumentManager.MdiActiveDocument.AcadDocument;
#endif


            object[] data;

            data = new object[] { "-layer\nSet\n0\n\n" };
            activeDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, data);
            data = new object[] { "zoom\ne\n" };
            activeDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, data);
            data = new object[] { "audit\nYes\n" };
            activeDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, data);
            data = new object[] { "-purge\nall\n*\nNo\n" };
            activeDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, data);
            data = new object[] { "tilemode\n0\n" };
            activeDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, data);
            data = new object[] { "zoom\ne\n" };
            activeDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, data);
            data = new object[] { "qsave\n" };
            activeDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, data);
            data = new object[] { "close\n" };
            activeDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, data);

            //_ed.Command("-layer", "Set", 0, " " );
            //_ed.Command( "zoom", "e" );
            //_ed.Command( "audit", "Yes" );
            //_ed.Command( "-purge", "all", "*", "No" );
            //_ed.Command( "tilemode", 0 );
            //_ed.Command( "zoom", "e" );
            //_doc.CloseAndSave(_doc.Name);

            //CommandLineHelper.Command("_layer", "Set", "0", "");
            //CommandLineHelper.Command("zoom", "e");
            //CommandLineHelper.Command("audit", "Yes");
            //CommandLineHelper.Command("-purge", "all", "*", "No");
            //CommandLineHelper.Command("qsave");
            //CommandLineHelper.Command("close");
        }

        /// <summary>
        /// Kojto Commands List
        /// </summary>
        [CommandMethod("kojtocommands")]
        public void ShowCommandsListStart()
        {
            // search through the current assembly for CommandMethods
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            object[] customAttributes = currentAssembly.GetCustomAttributes(typeof(CommandClassAttribute), true);
            Type[] exportedTypes;
            int typesCount = customAttributes.Length;

            if (typesCount > 0)
            {
                exportedTypes = new Type[typesCount];
                for (int i = 0; i < typesCount; i++)
                {
                    CommandClassAttribute commandClassAttribute = customAttributes[i] as CommandClassAttribute;
                    if (commandClassAttribute != null)
                    {
                        exportedTypes[i] = commandClassAttribute.Type;
                    }
                }
            }
            else
            {
                exportedTypes = currentAssembly.GetExportedTypes();
            }
            int index = 0;
            StringBuilder stringBuilder = new StringBuilder(null);
            foreach (Type type in exportedTypes)
            {
                MethodInfo[] meths = type.GetMethods();

                foreach (MethodInfo meth in meths)
                {

                    customAttributes = meth.GetCustomAttributes(typeof(CommandMethodAttribute), true);
                    foreach (object obj in customAttributes)
                    {
                        CommandMethodAttribute commandMethodAttribute = (CommandMethodAttribute)obj;
                        stringBuilder.Append("\n " + (index++) + " :  " + commandMethodAttribute.GlobalName + " =>  " +
                                             meth.Name);
                    }
                }
            }
            // MessageBox.Show(stringBuilder.ToString());
            _ed.WriteMessage(stringBuilder.ToString());
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Join 3D polylines
        /// </summary>
        [CommandMethod("pedit3d")]
        public void Join3DLinesToPolyStart()
        {

            if (!_doc.UserData.Contains("pedit3d.lsp_LOADED"))
            {
                string lispDir =
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase.Remove(0, 8));
                string lispFile = lispDir + Settings.Default.lispDir + "pedit3d.lsp";

                lispFile = lispFile.Replace("\\", "\\\\");

                if (File.Exists(lispFile))
                {
                    _doc.SendStringToExecute("(load \"" + lispFile + "\") ", true, false, false);
                }
                _doc.UserData.Add("pedit3d.lsp_LOADED", 1);
            }
            _doc.SendStringToExecute("pedit3dcmd ", true, false, false);
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Level up all entities to z = 0
        /// </summary>
        [CommandMethod("smash")]
        public void LevelUpAllToZeroStart()
        {

            if (!_doc.UserData.Contains("smash.lsp_LOADED"))
            {

                string lispFile =
                    _fileService.GetFile(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Settings.Default.lispDir,
                        "smash.lsp");
                if (lispFile == null)
                {
                    _editorHelper.WriteMessage("Lisp file \"smash.lsp\" does not exist.");
                    return;
                }
                lispFile = lispFile.Replace("\\", "\\\\");

                if (File.Exists(lispFile))
                {
                    _doc.SendStringToExecute("(load \"" + lispFile + "\") ", true, false, false);
                }
                _doc.UserData.Add("smash.lsp_LOADED", 1);
            }
            _doc.SendStringToExecute("smashcmd ", true, false, false);

        }

        /// <summary>
        /// Level up all entities to z = 0
        /// </summary>
        [CommandMethod("flat")]
        public void FlattensAllToZeroStart()
        {

            if (!_doc.UserData.Contains("flat.lsp_LOADED"))
            {

                string lispFile =
                    _fileService.GetFile(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Settings.Default.lispDir,
                        "flat.lsp");
                if (lispFile == null)
                {
                    _editorHelper.WriteMessage("Lisp file \"flat.lsp\" does not exist.");
                    return;
                }
                lispFile = lispFile.Replace("\\", "\\\\");

                if (File.Exists(lispFile))
                {
                    _doc.SendStringToExecute("(load \"" + lispFile + "\") ", true, false, false);
                }
                _doc.UserData.Add("flat.lsp_LOADED", 1);
            }
            _doc.SendStringToExecute("flatcmd ", true, false, false);

        }

        /// <summary>
        /// Copy Block Definition To New Block
        /// </summary>
        [CommandMethod("cb")]
        public void CopyBlockDefInNewDefStart()
        {

            if (!_doc.UserData.Contains("CopyBlock.lsp_LOADED"))
            {
                string lispFile =
                    _fileService.GetFile(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Settings.Default.lispDir,
                        "CopyRenameBlockV1-2.lsp");
                if (lispFile == null)
                {
                    _editorHelper.WriteMessage("Lisp file \"CopyRenameBlockV1-2.lsp\" does not exist.");
                    return;
                }
                lispFile = lispFile.Replace("\\", "\\\\"); // Do not remove this!

                if (File.Exists(lispFile))
                {
                    _doc.SendStringToExecute("(load \"" + lispFile + "\") ", true, false, false);
                }
                _doc.UserData.Add("CopyBlock.lsp_LOADED", 1);
            }
            _doc.SendStringToExecute("DivergeBlockDefAndCopy ", true, false, false);

        }

        /// <summary>
        /// Rename Block Definition Into New Block Definition
        /// </summary>
        [CommandMethod("rb")]
        public void RenameBlockDefInNewDefStart()
        {

            if (!_doc.UserData.Contains("CopyBlock.lsp_LOADED"))
            {
                string lispFile =
                    _fileService.GetFile(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Settings.Default.lispDir,
                        "CopyRenameBlockV1-2.lsp");
                if (lispFile == null)
                {
                    _editorHelper.WriteMessage("Lisp file \"CopyRenameBlockV1-2.lsp\" does not exist.");
                    return;
                }
                lispFile = lispFile.Replace("\\", "\\\\"); // Do not remove this!

                if (File.Exists(lispFile))
                {
                    _doc.SendStringToExecute("(load \"" + lispFile + "\") ", true, false, false);
                }
                _doc.UserData.Add("CopyBlock.lsp_LOADED", 1);
            }
            _doc.SendStringToExecute("DivergeBlockDefAndRename ", true, false, false);
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Replace Single Block With File
        /// </summary>
        [CommandMethod("repb")]
        public void ReplaceBlockWithFileStart()
        {

            Application.SetSystemVariable("FILEDIA", 1);
            var editorUtility = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            PromptEntityResult blockToReplaceResult = editorUtility.PromptForObject("Select the block to replace : ",
                typeof(BlockReference), false);
            if (blockToReplaceResult.Status != PromptStatus.OK)
            {
                return;
            }

            var openFileDialog = new System.Windows.Forms.OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (!File.Exists(openFileDialog.FileName))
            {
                _ed.WriteMessage("File does not exists.");
                return;
            }

            if (!openFileDialog.FileName.EndsWith(".dwg", StringComparison.InvariantCultureIgnoreCase))
            {
                _ed.WriteMessage("File is not a DWG.");
                return;
            }

            Point3d selectedEntityPoint;

            using (Transaction transaction = _db.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = (BlockTable)transaction.GetObject(_db.BlockTableId, OpenMode.ForWrite);
                Entity selectedEntity = (Entity)transaction.GetObject(blockToReplaceResult.ObjectId, OpenMode.ForWrite);

                selectedEntityPoint = ((BlockReference)selectedEntity).Position;
                selectedEntity.Erase();

                blockTable.DowngradeOpen();
                blockTable.Dispose();

                transaction.Commit();
            }

            ReplaceBlockRefWithDWG(_doc, openFileDialog.FileName, selectedEntityPoint, _ed.CurrentUserCoordinateSystem);
            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// /// * Insert Drawing As Block - DWG or DXF *
        /// the source drawig should be drawn as number of
        /// separate entites with or without attributes 
        /// 
        /// </summary>
        /// <param name="destinationDocument"></param>
        /// <param name="sourceDrawing"></param>
        /// <param name="insertionPoint"></param>
        /// <param name="transforMatrix"> </param>
        /// <exception cref="NotImplementedException">Not implemented for DXFs</exception>
        /// <returns>ObjectID of the Block Def that was imported.</returns>
        private void ReplaceBlockRefWithDWG(Document destinationDocument, string sourceDrawing, Point3d insertionPoint,
            Matrix3d transforMatrix)
        {
            Point3d oldPoint = insertionPoint.TransformBy(transforMatrix.Inverse());
            insertionPoint = new Point3d(0, 0, 0);

            Database destinationDb = destinationDocument.Database;
            Editor ed = destinationDocument.Editor;

            string blockname = sourceDrawing.Remove(0, sourceDrawing.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            blockname = blockname.Substring(0, blockname.Length - 4); // remove the extension

            using (destinationDocument.LockDocument())
            {
                using (var inMemoryDb = new Database(false, true))
                {
                    if (sourceDrawing.LastIndexOf(".dwg", StringComparison.Ordinal) > 0)
                    {
                        inMemoryDb.ReadDwgFile(sourceDrawing, FileShare.ReadWrite, true, "");
                    }
                    else if (sourceDrawing.LastIndexOf(".dxf", StringComparison.Ordinal) > 0)
                    {
                        throw new NotImplementedException("DXFs not suppported");
                        //inMemoryDb.DxfIn(string filename, string logFilename);
                    }

                    using (var transaction = destinationDocument.TransactionManager.StartTransaction())
                    {
                        var destinationDatabaseBlockTable =
                            (BlockTable)transaction.GetObject(destinationDb.BlockTableId, OpenMode.ForRead);
                        ObjectId sourceBlockObjectId;
                        if (destinationDatabaseBlockTable.Has(blockname))
                        {

                            ed.WriteMessage("Block " + blockname +
                                            " already exists.\n Attempting to create block reference...");

                            var destinationDatabaseCurrentSpace =
                                (BlockTableRecord)destinationDb.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                            var destinationDatabaseBlockDefinition =
                                (BlockTableRecord)
                                    transaction.GetObject(destinationDatabaseBlockTable[blockname], OpenMode.ForRead);
                            sourceBlockObjectId = destinationDatabaseBlockDefinition.ObjectId;

                            // Create a block reference to the existing block definition
                            using (var blockReference = new BlockReference(insertionPoint, sourceBlockObjectId))
                            {
                                //Matrix3d Mat = Matrix3d.Identity;   

                                Vector3d mov = new Vector3d(oldPoint.X, oldPoint.Y, oldPoint.Z);
                                mov = mov.TransformBy(transforMatrix);
                                blockReference.TransformBy(transforMatrix);
                                blockReference.TransformBy(Matrix3d.Displacement(mov));

                                blockReference.ScaleFactors = new Scale3d(1, 1, 1);
                                destinationDatabaseCurrentSpace.AppendEntity(blockReference);
                                transaction.AddNewlyCreatedDBObject(blockReference, true);
                                ed.Regen();
                                transaction.Commit();
                                // At this point the Bref has become a DBObject and can be disposed
                            }
                            return;
                        }

                        // There is not such block definition, so we are inserting/creating new one
                        sourceBlockObjectId = destinationDb.Insert(blockname, inMemoryDb, true);

                        #region Create Block Ref from the already imported Block Def

                        // We continue here the creation of the new block reference of the already imported block definition
                        var sourceDatabaseCurrentSpace =
                            (BlockTableRecord)destinationDb.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                        using (var blockReference = new BlockReference(insertionPoint, sourceBlockObjectId))
                        {
                            blockReference.ScaleFactors = new Scale3d(1, 1, 1);
                            sourceDatabaseCurrentSpace.AppendEntity(blockReference);
                            transaction.AddNewlyCreatedDBObject(blockReference, true);

                            var blockTableRecord =
                                (BlockTableRecord)blockReference.BlockTableRecord.GetObject(OpenMode.ForRead);
                            var atcoll = blockReference.AttributeCollection;
                            foreach (ObjectId subid in blockTableRecord)
                            {
                                var entity = (Entity)subid.GetObject(OpenMode.ForRead);
                                var attributeDefinition = entity as AttributeDefinition;

                                if (attributeDefinition == null)
                                {
                                    continue;
                                }
                                var attributeReference = new AttributeReference();
                                attributeReference.SetPropertiesFrom(attributeDefinition);
                                attributeReference.Visible = attributeDefinition.Visible;
                                attributeReference.SetAttributeFromBlock(attributeDefinition,
                                    blockReference.BlockTransform);
                                attributeReference.HorizontalMode = attributeDefinition.HorizontalMode;
                                attributeReference.VerticalMode = attributeDefinition.VerticalMode;
                                attributeReference.Rotation = attributeDefinition.Rotation;
                                attributeReference.Position = attributeDefinition.Position +
                                                              insertionPoint.GetAsVector();
                                attributeReference.Tag = attributeDefinition.Tag;
                                attributeReference.FieldLength = attributeDefinition.FieldLength;
                                attributeReference.TextString = attributeDefinition.TextString;
                                attributeReference.AdjustAlignment(destinationDb);

                                atcoll.AppendAttribute(attributeReference);
                                transaction.AddNewlyCreatedDBObject(attributeReference, true);
                            }

                            var mov = new Vector3d(oldPoint.X, oldPoint.Y, oldPoint.Z);
                            mov = mov.TransformBy(transforMatrix);
                            blockReference.TransformBy(transforMatrix);
                            blockReference.TransformBy(Matrix3d.Displacement(mov));

                            transaction.Commit();
                        }

                        #endregion

                        ed.Regen();
                    }
                }
            }
        }

        [CommandMethod("kojtoversion")]
        public void KojtoVersion()
        {

            var utils = new UtilityClass();
            var productName = utils.GetApplicationName();
            var assemblyFileVersion = utils.GetCurrentAssemblyFileVersion();

            _ed.WriteMessage("\n" + productName + "." + assemblyFileVersion);
        }

        [CommandMethod("iu", CommandFlags.Session)]
        public void InsertUcsFromFileStart()
        {
            var pofo = new PromptOpenFileOptions("Select a file")
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            var res = _ed.GetFileNameForOpen(pofo);
            if (res.Status != PromptStatus.OK)
            {
                return;
            }
            using (var loc = _doc.LockDocument())
            {
                using (var destBaseTr = _db.TransactionManager.StartTransaction())
                {
                    var destUcsTable = (UcsTable)destBaseTr.GetObject(_db.UcsTableId, OpenMode.ForWrite);
                    using (var sourceDb = new Database(false, true))
                    {
                        sourceDb.ReadDwgFile(res.StringResult, FileOpenMode.OpenForReadAndAllShare, true, "");
                        using (var tr = sourceDb.TransactionManager.StartTransaction())
                        {
                            var sourceUcsTable = (UcsTable)tr.GetObject(sourceDb.UcsTableId, OpenMode.ForRead);
                            foreach (ObjectId uscRecordId in sourceUcsTable)
                            {
                                var ucsTr = (UcsTableRecord)tr.GetObject(uscRecordId, OpenMode.ForRead);
                                var suffix = 1;
                                var name = ucsTr.Name;
                                while (destUcsTable.Has(name))
                                {
                                    _editorHelper.WriteMessage(String.Format("UCS {0} exists!\n", ucsTr.Name));
                                    name = String.Format("{0}{1}", ucsTr.Name, String.Format("({0})", suffix));
                                    suffix++;
                                }

                                var x = new UcsTableRecord
                                {
                                    Name = name,
                                    Origin = ucsTr.Origin,
                                    XAxis = ucsTr.XAxis,
                                    YAxis = ucsTr.YAxis
                                };
                                destUcsTable.Add(x);
                                destBaseTr.AddNewlyCreatedDBObject(x, true);
                            }
                        }
                    }
                    destBaseTr.Commit();
                }
            }
        }

        [CommandMethod("edu")]
        public void OpenSaveDwgRemoveEduWatermark()
        {
            //var openFileDialog = new OpenFileDialog(
            //    "Select dwg file...", null, "dwg", null, OpenFileDialog.OpenFileDialogFlags.DefaultIsFolder | OpenFileDialog.OpenFileDialogFlags.AllowMultiple);

            //DialogResult dr = openFileDialog.ShowDialog();

            //if (dr != DialogResult.OK)
            //{
            //    return;
            //}

            var dir = new DirectoryInfo(@".\");
            var validationResultFile = dir.GetFiles("ValidationResult*.csv", SearchOption.TopDirectoryOnly).FirstOrDefault();
            var files = File.ReadAllLines(validationResultFile.FullName).Select(x => x.Split(new[] { "@@" }, StringSplitOptions.RemoveEmptyEntries))
                //.Where(x => !x[2].StartsWith("Clean")).Select(x => x[0]).ToArray();
                .Where(x => x[2].StartsWith("Infected")).Select(x => x[0]).ToArray();
            //var dirPath = AttributeHelper.GetFolder();
            //var dir = new DirectoryInfo(dirPath);
            var cleanedFile = "CleanedDwgs.csv";
            var alreadyCleaned = File.Exists(cleanedFile) ? File.ReadAllLines(cleanedFile) : new string[0];
            using (var logger = new StreamWriter(cleanedFile, true))
            using (var errorLogger = new StreamWriter("Errors.csv", true))
            using (Application.DocumentManager.MdiActiveDocument.LockDocument())
                foreach (var x in files)
                {
                    if (alreadyCleaned.Contains(x))
                    {
                        continue;
                    }

                    try
                    {
                        using (var db = new Database(false, true))
                        {
                            //var path = Path.Combine(@"D:\DwgWork", x.Substring(49));
                            //var longPath = $@"\\?\{path}";
                            //new FileInfo(path).IsReadOnly = false;

                            var longPath = $@"\\?\UNC{x.Substring(1)}";
                            db.ReadDwgFile(longPath, FileShare.ReadWrite, false, "");
                            db.SaveAs(longPath, db.OriginalFileSavedByVersion /*db.OriginalFileVersion*/);
                        }
                    }
                    catch (System.Exception e)
                    {
                        errorLogger.WriteLine($"{x} -> { e.Message}");
                    }

                    logger.WriteLine(x);
                }
        }

        [CommandMethod("Recover_files_with_error", CommandFlags.Session)]
        public void RecoverTest()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            DocumentCollection docs = Application.DocumentManager;
            Editor ed = doc.Editor;

#if !acad2013
            ed.WriteMessage("Not supported in ACAD versions lower than 2015.");
            return;
#else
            var oldValue = Application.GetSystemVariable("PROXYNOTICE");
            Application.SetSystemVariable("PROXYNOTICE", 0);


            var errorsFile = @"errors.csv";
            var content = File.ReadAllLines(errorsFile).Select(x => x.Split(new[] { " -> " }, StringSplitOptions.RemoveEmptyEntries));
            var forRepair = content.Where(x => x[1].Equals("eDwgNeedsRecovery")).Select(x => x[0]);
            using (var errorLogger = new StreamWriter("Errors_Recovery.csv", true) { AutoFlush = true })
            {
                foreach (var file in forRepair)
                {
                    //var path = $@"\\?\{file}";
                    try
                    {
                        var path = Path.Combine(@"D:\DwgWork", file.Substring(49));
                        //path = $@"\\?\{path}";
                        docs.AppContextRecoverDocument(path);
                        Application.DocumentManager.MdiActiveDocument.CloseAndSave(path);
                    }
                    catch (System.Exception e)
                    {
                        errorLogger.WriteLine($"{file} -> {e.Message}");
                    }
                }

            }

            Application.SetSystemVariable("PROXYNOTICE", oldValue);
#endif

        }
    }
}