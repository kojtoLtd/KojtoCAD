using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;

#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.PlottingServices;     
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Bricscad.PlottingServices;
using Exception = Teigha.Runtime.Exception;
using Application = Bricscad.ApplicationServices.Application;
#endif

using PlotCfg = KojtoCAD.Properties.Settings;

[assembly: CommandClass( typeof( KojtoCAD.Plotter.Plotter ) )]

namespace KojtoCAD.Plotter
{
    public class Plotter
    {
        #region constants

        private const string strDone = "Done";

        private const string strPublished = "Published";

        private const string strFailed = "Failed";

        private const string strSkipped = "Skipped";

        private const string strNoLayouts = "No layouts";

        private const string strCanNotFindPaperFormat = "Can not find paper format";

        private const string strCorruptDwg = "Corrupt drawing";

        private const string dwfExt = "dwf";

        private const string pdfExt = "pdf";

        private const string dwgExt = "dwg";

        private const string dxfExt = "dxf";

        private const string logExt = "log";

        #endregion

        private static readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        public class ReportLog
        {
            private string _logFile;

            public ReportLog()
            {
            }

            public ReportLog(string pathName)
            {
                string day = DateTime.Now.Day.ToString();
                string hour = DateTime.Now.Hour.ToString();
                string min = DateTime.Now.Minute.ToString();
                string sec = DateTime.Now.Second.ToString();

                _logFile = pathName + "_" + day + "_" + hour + "_" + min + "_" + sec + "." + logExt;
                StreamWriter sw = new StreamWriter(_logFile, true);
                sw.Flush();
                sw.Close();
            }

            public string getLogFileName()
            {
                return _logFile;
            }

            public void setLogFileName(string fileName)
            {
                _logFile = fileName;
            }

            public void Log(string logMsg)
            {
                StreamWriter sw = new StreamWriter(_logFile, true);
                sw.WriteLine(logMsg);
                sw.Flush();
                sw.Close();
            }
        }

        // publish infos of layouts for plot
        private static List<PublishInfo> _pubInfos = new List<PublishInfo>();

        public static List<PublishInfo> PubInfos
        {
            set
            {
                _pubInfos = value;
            }
            get
            {
                return _pubInfos;
            }
        }

        public static PublishStatusDlg PublishStatusDialog;

        /// <summary>
        /// Plotter
        /// </summary>
        [CommandMethod("pplot", CommandFlags.Session)]
        public static void PlotterStart()
        {
#if !bcad
            

            if (new UtilityClass().HostAppVersion() < 17)
            {
                throw new System.Exception("Application does not support versions before AutoCAD 2007");
            }
            _pubInfos.Clear();

            // Exit if SDI
            short sdi = (short)Application.GetSystemVariable("SDI");
            if (sdi == 1)
            {
                MessageBox.Show(
                    "Prototyp Plotter does not work in SDI(Single Document Interface) mode. \nChange SDI to 0",
                    "Prototyp Plotter");
                return;
            }

            // Get the state of certain system variables, to be stored in the XML and retrieved later
            Object FileDiaStatus = Application.GetSystemVariable("FILEDIA");
            Object BackGroundPlotStatus = Application.GetSystemVariable("BACKGROUNDPLOT");
            Object RecoveryModeStatus = Application.GetSystemVariable("RECOVERYMODE");

            // Show the initial dialog on first run
            PlotterForm PlotterDialog = new PlotterForm();
            DialogResult result = Application.ShowModalDialog(PlotterDialog);

            if (result != DialogResult.OK) return;

            if (!PlotterDialog.Publish) return;
                             
            string srcDir = PlotCfg.Default.plotSourcePath;
            string saveDir = PlotCfg.Default.plotSavePath;
            bool plotDwf = PlotCfg.Default.plotMakeDwf;
            bool plotPdf = PlotCfg.Default.plotMakePdf;
            ReportLog dwfLog = null;
            ReportLog pdfLog = null;
            string dwfLogFile = "";
            string pdfLogFile = "";
            bool retry = false;
            bool modOnly = false;
            bool readDxf = PlotCfg.Default.plotReadDxf;
            bool readDwg = PlotCfg.Default.plotReadDwg;
            bool ignoreModelSpace = PlotCfg.Default.plotIgnoreModelSpace;
            bool plotAllDWGsInSource = PlotCfg.Default.plotPlotAllDWGs;
            bool plotMultiSheet = PlotCfg.Default.plotMultiSheet;
            string ctbFile = PlotCfg.Default.plotStyleTable;
            string timeout = "30"; // seconds

            //ListView.ListViewItemCollection plotFrames = PlotterDialog.plotFrames;

            FilePlotConfig FilePlotConfigPrint = new FilePlotConfig(
                Settings.Default.plotDevicePrinter,
                Settings.Default.plotPaperSizePrint.Replace(' ', '_'),
                Settings.Default.plotStyleTable,
                Settings.Default.plotSavePath,
                Settings.Default.plotSourcePath,
                "",
                GetExactBlockAndFrameNames(Settings.Default.plotDrawingFrameLayer),
                GetExactBlockAndFrameNames(Settings.Default.plotDrawingFrameName),
                Settings.Default.plotMultiSheet,
                Settings.Default.plotTurnOnViewPorts,
                Settings.Default.plotFitToPage,
                Settings.Default.plotIgnoreModelSpace,
                Settings.Default.plotCenterPlot,
                OutputFileType.None,
                Settings.Default.plotPlotAllDWGs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly,
                Settings.Default.plotTransparency);

            FilePlotConfig FilePlotConfigPdf = new FilePlotConfig(
                Settings.Default.plotDevicePdf,
                Settings.Default.plotPaperSizePdf.Replace(' ', '_'),
                Settings.Default.plotStyleTable,
                Settings.Default.plotSavePath,
                Settings.Default.plotSourcePath,
                "",
                GetExactBlockAndFrameNames(Settings.Default.plotDrawingFrameLayer),
                GetExactBlockAndFrameNames(Settings.Default.plotDrawingFrameName),
                Settings.Default.plotMultiSheet,
                Settings.Default.plotTurnOnViewPorts,
                Settings.Default.plotFitToPage,
                Settings.Default.plotIgnoreModelSpace,
                Settings.Default.plotCenterPlot,
                OutputFileType.PDF,
                Settings.Default.plotPlotAllDWGs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly,
                Settings.Default.plotTransparency
                );


            FilePlotConfig FilePlotConfigDwf = new FilePlotConfig(
                Settings.Default.plotDeviceDwf,
                Settings.Default.plotPaperSizeDwf.Replace(' ', '_'),
                Settings.Default.plotStyleTable,
                Settings.Default.plotSavePath,
                Settings.Default.plotSourcePath,
                "",
                GetExactBlockAndFrameNames(Settings.Default.plotDrawingFrameLayer),
                GetExactBlockAndFrameNames(Settings.Default.plotDrawingFrameName),
                Settings.Default.plotMultiSheet,
                Settings.Default.plotTurnOnViewPorts,
                Settings.Default.plotFitToPage,
                Settings.Default.plotIgnoreModelSpace,
                Settings.Default.plotCenterPlot,
                OutputFileType.DWF,
                Settings.Default.plotPlotAllDWGs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly,
                Settings.Default.plotTransparency);

            if (plotDwf)
            {
                dwfLogFile = saveDir + "\\log_" + dwfExt;
                dwfLog = new ReportLog(dwfLogFile);
                dwfLogFile = dwfLog.getLogFileName();
            }

            if (plotPdf)
            {
                pdfLogFile = saveDir + "\\log_" + pdfExt;
                pdfLog = new ReportLog(pdfLogFile);
                pdfLogFile = pdfLog.getLogFileName();
            }

            // Ignore the model space if opted
            if (ignoreModelSpace)
            {
                foreach (PublishInfo info in _pubInfos)
                {
                    foreach (LayoutInfo layoutInfo in info.LayoutInfos)
                    {
                        if (layoutInfo.IsModel)
                        {
                            layoutInfo.Publish = false;
                        }
                    }
                }
            }

            // Build an XML configuration file to help us track the status of our publishing
            XmlUtils.BuildXml(
                srcDir,
                saveDir,
                saveDir,
                plotDwf,
                plotPdf,
                retry,
                dwfLogFile,
                pdfLogFile,
                timeout,
                modOnly,
                FileDiaStatus,
                BackGroundPlotStatus,
                RecoveryModeStatus);

            try
            {
                // Set our system variables to help programmatic publishing
                Application.SetSystemVariable("FILEDIA", 0);
                Application.SetSystemVariable("BACKGROUNDPLOT", 0);
                Application.SetSystemVariable("RECOVERYMODE", 0);

                // Show the modeless dialog and start to publish
                PublishStatusDialog = new PublishStatusDlg();
                Application.ShowModelessDialog(PublishStatusDialog);
                PublishStatusDialog.PopulateList(ref _pubInfos, timeout);

                string outDir = saveDir;

                foreach (PublishInfo info in _pubInfos)
                {
                    if (PublishStatusDialog.Stop)
                    {
                        PublishStatusDialog.newFile();
                        XmlUtils.DeleteXml();
                        break;
                    }

                    info.DwfPubTime = DateTime.Now.ToString();
                    info.PdfPubTime = DateTime.Now.ToString();

                    if (info.Published)
                    {
                        string status = strDone;
                        if (info.SkipDwg) status = strSkipped;
                        else if (info.Failed && info.CanRetry) status = strFailed;
                        else if (info.Failed && !info.CanRetry) status = strCorruptDwg;

                        if (plotPdf)
                        {
                            string publishedPath = saveDir + "\\" + Path.GetFileNameWithoutExtension(info.DwgName)
                                                   + ".pdf";

                            if (info.SkipDwg || info.Failed) publishedPath = "";

                            PublishStatusDialog.UpdateFileStatus(
                                info.Index, status, publishedPath, false, info.PdfPubTime);

                            if (retry && Same(status, strDone))
                            {
                                string logString = info.DwgName + "," + status + "," + "" + "," + info.PdfPubTime;
                                pdfLog.Log(logString);
                            }
                        }

                        if (plotDwf)
                        {
                            string publishedPath = saveDir + "\\" + Path.GetFileNameWithoutExtension(info.DwgName) + "."
                                                   + dwfExt;

                            if (info.SkipDwg || info.Failed) publishedPath = "";

                            PublishStatusDialog.UpdateFileStatus(
                                info.Index, status, publishedPath, true, info.DwfPubTime);

                            if (retry && Same(status, strDone))
                            {
                                string logString = info.DwgName + "," + status + "," + "" + "," + info.DwfPubTime;
                                dwfLog.Log(logString);
                            }
                        }
                        PublishStatusDialog.newFile();
                        continue;
                    }

                    if (info.SkipDwg)
                    {
                        PublishStatusDialog.UpdateFileStatus(info.Index, strSkipped, "", false, info.DwfPubTime);
                        PublishStatusDialog.newFile();

                        // Update the XML file
                        XmlUtils.UpdatePublishStatus(info, DateTime.Now.ToString(), strPublished);
                        continue;
                    }

                    bool failed = false;

                    if (retry) failed = !info.CanRetry;
                    else failed = info.Failed;

                    if (failed)
                    {
                        string strStatus = strDone;

                        if (info.Failed && info.CanRetry) strStatus = strFailed;
                        else if (info.Failed && !info.CanRetry) strStatus = strCorruptDwg;

                        PublishStatusDialog.UpdateFileStatus(info.Index, strStatus, "", true, info.DwfPubTime);

                        PublishStatusDialog.newFile();

                        // Update the XML file

                        XmlUtils.UpdatePublishStatus(info, DateTime.Now.ToString(), strPublished);
                        continue;
                    }

                    XmlUtils.UpdatePublishStatus(info, DateTime.Now.ToString(), strPublished);

                    try
                    {
                        Document openDoc = null; // Tova e nenujno. Ako mi stigne vremeto shte go razkaram.

                        if (plotDwf)
                        {
                            FilePlotConfigDwf.fileName = Path.GetFileNameWithoutExtension(info.DwgName);
                            PublishFile(info, pdfLog, PublishStatusDialog, out openDoc, FilePlotConfigDwf);
                        }

                        if (plotPdf)
                        {
                            FilePlotConfigPdf.fileName = Path.GetFileNameWithoutExtension(info.DwgName);
                            PublishFile(info, pdfLog, PublishStatusDialog, out openDoc, FilePlotConfigPdf);
                        }

                        if (Settings.Default.plotMakePrint) //Lishkov 20/06/2012 - директно принтиране
                        {
                            List<bool> rotations = new List<bool>();
                            StringCollection formats = new StringCollection();
                            StringCollection names = PublishFileA(info, ref openDoc, ref rotations, ref formats);

                            int counter = 0;
                            string error = "";
                            foreach (string str in names)
                            {
                                string command;
                                if (Settings.Default.plotPaperSizePrint != "Auto")
                                    command = PrintString(
                                        str.Trim(),
                                        Settings.Default.plotDevicePrinter,
                                        Settings.Default.plotPaperSizePrint,
                                        rotations[counter],
                                        Settings.Default.plotStyleTable);
                                else
                                    command = PrintString(
                                        str.Trim(),
                                        Settings.Default.plotDevicePrinter,
                                        formats[counter],
                                        rotations[counter],
                                        Settings.Default.plotStyleTable);

                                try
                                {
#if acad2012
                                    object ActiveDocument = openDoc.AcadDocument;
#endif
#if acad2013
                                   object ActiveDocument = Application.DocumentManager.MdiActiveDocument.GetAcadDocument();
#endif
#if bcad
                                   object ActiveDocument = Application.DocumentManager.MdiActiveDocument.AcadDocument;
#endif


                                    object[] data = { command };
                                    ActiveDocument.GetType()
                                                  .InvokeMember(
                                                      "SendCommand",
                                                      System.Reflection.BindingFlags.InvokeMethod,
                                                      null,
                                                      ActiveDocument,
                                                      data);
                                }
                                catch
                                {
                                    error += str + " ERROR ";
                                }
                                counter++;
                            }

                            PublishStatusDialog.UpdateFileStatus(
                                info.Index, strPublished + " " + error, "", false, info.DwfPubTime);

                        }


                        if (openDoc != null)
                        {
                            //openDoc.CloseAndDiscard();
                            new UtilityClass().CloseDocument(openDoc.Name, false);
                        }

                        //info.Failed = false;

                        //XmlUtils.UpdatePublishStatus(info, DateTime.Now.ToString(), strFailed);
                    }

                    catch (System.Exception ex)
                    {
                        Application.ShowAlertDialog("Exception: " + ex.Message);
                        info.Failed = true;
                    }

                    PublishStatusDialog.newFile();
                }

                //Log failed drawing files...
                int failCount = 0;
                foreach (PublishInfo info in _pubInfos)
                {
                    if (PublishStatusDialog.Stop) return;

                    if (info.Failed && info.CanRetry) failCount++;

                    string status = "";
                    if (info.SkipDwg) status = strSkipped;
                    else if (info.Failed) status = strFailed;

                    if (plotPdf)
                    {
                        if (info.SkipDwg || info.Failed)
                        {
                            string logString = info.DwgName + "," + status + "," + "," + info.PdfPubTime;
                            pdfLog.Log(logString);
                        }
                    }

                    if (plotDwf)
                    {
                        if (info.SkipDwg || info.Failed)
                        {
                            string logString = info.DwgName + "," + status + "," + "," + info.DwfPubTime;
                            dwfLog.Log(logString);
                        }
                    }
                }
                PublishStatusDialog.PublishOver(failCount);
            }
            catch (Exception Ex)
            {
                Application.ShowAlertDialog("Exception: " + Ex.Message + "\n" + Ex.Source + "\n" + Ex.StackTrace);
            }
            finally
            {
                // restore AutoCAD envirioment
                Application.SetSystemVariable("FILEDIA", FileDiaStatus);
                Application.SetSystemVariable("BACKGROUNDPLOT", BackGroundPlotStatus);
                Application.SetSystemVariable("RECOVERYMODE", RecoveryModeStatus);

                Application.SetSystemVariable("USERS1", "");
                Application.SetSystemVariable("USERI1", 0);
                // Delete the main XML file
                XmlUtils.DeleteXml();
            }
#else
            MessageBox.Show("Not available in BricsCAD", "Warning", MessageBoxButtons.OK);
#endif
        }

        /// <summary>
        /// Build a list of the layouts contained in the various
        /// DWG and DWF files
        /// </summary>
        /// <param name="aSrcDir"></param>
        /// <param name="aReadDwg"></param>
        /// <param name="aReadDxf"></param>
        /// <param name="aPlotAllDWGsInDir"></param>
        /// <returns></returns>
        public static bool BuildList(string aSrcDir, bool aReadDwg, bool aReadDxf, bool aPlotAllDWGsInDir)
        {
            _pubInfos.Clear();

            // Now get the list of drawings in the selected
            // source directory
            SearchOption searchOpts = SearchOption.TopDirectoryOnly;
            if (aPlotAllDWGsInDir)
            {
                searchOpts = SearchOption.AllDirectories;
            }

            string[] dwgFiles = Directory.GetFiles(aSrcDir + "\\", "*." + dwgExt, searchOpts);
            string[] dxfFiles = Directory.GetFiles(aSrcDir + "\\", "*." + dxfExt, searchOpts);

            // Show alert if there isn't any DWG/DXF file available
            if (dwgFiles.Length < 1 && dxfFiles.Length < 1)
            {
                Application.ShowAlertDialog("No DWG/DXF files available, " + "please select a different source path.");

                return false;
            }

            // Set the wait cursor but remember the old one
            Cursor current = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            Document Doc = Application.DocumentManager.MdiActiveDocument;

            DocumentLock DocLock = Doc.LockDocument();
            using (DocLock)
            {
                // Get all the layouts in drawing file and prepare the list
                // to publish

                if (aReadDwg)
                {
                    foreach (string file in dwgFiles)
                    {
                        PreparePublishList(file, true);
                    }
                }
                if (aReadDxf)
                {
                    foreach (string file in dxfFiles)
                    {
                        PreparePublishList(file, false);
                    }
                }
            }

            // Set the cursor back

            Cursor.Current = current;
            return true;
        }

        private static bool PreparePublishList(string file, bool isDwg)
        {
            bool pubListIsOk = true;

            PublishInfo PublishInfo = new PublishInfo();

            // Open the drawing, get all the layouts and prepare the
            // publish nodes...
            _pubInfos.Add(PublishInfo);
            try
            {
                PublishInfo.DwgName = file;
                using (Database Db = new Database(false, true))
                {
                    if (isDwg)
                    {
                        Db.ReadDwgFile(file, FileShare.ReadWrite, true, "");
                    }
                    else
                    {
                        Db.DxfIn(file, null);
                    }

                    // Get the layout names and populate the PublishInfo         
                    using (Transaction Tr = Db.TransactionManager.StartTransaction())
                    {
                        DBDictionary LayoutDictionary =
                            (DBDictionary)Tr.GetObject(Db.LayoutDictionaryId, OpenMode.ForRead);
                        foreach (DictionaryEntry DictEntryLayout in LayoutDictionary)
                        {
                            Layout Layout = (Layout)Tr.GetObject((ObjectId)DictEntryLayout.Value, OpenMode.ForRead);
                            PublishInfo.Add(DictEntryLayout.Key.ToString(), Layout.Handle, Layout.ModelType);
                        }
                        Tr.Commit();
                    }
                }
            }
            catch
            {
                // Error, used to prepare the report...
                PublishInfo.Failed = true;
                PublishInfo.CanRetry = false;
                pubListIsOk = false;
            }
            return pubListIsOk;
        }
#if !bcad
        /// <summary>
        /// Start the plot job for the current drawing (DB). All Layouts are in.
        /// </summary>
        /// <param name="aInfo"></param>
        /// <param name="aLogFile"></param>
        /// <param name="aStatDlg"></param>
        /// <param name="aDoc"></param>
        /// <param name="aPltCfg"></param>
        private static void PublishFile(
            PublishInfo aInfo, ReportLog aLogFile, PublishStatusDlg aStatDlg, out Document aDoc, FilePlotConfig aPltCfg)
        {
            aDoc = null;
            try
            {
                // set the extension of the file depending on the output opt
                string ext = ((aPltCfg.OutputFileType == OutputFileType.DWF) ? dwfExt : pdfExt);

                DocumentCollection DocMgr = Application.DocumentManager;

                //Turn of dwgchecking because of Athena's proxy items
                const string systemVarDwgCheck = "DWGCHECK";
                Int16 dwgCheckPrevious = (Int16)Application.GetSystemVariable(systemVarDwgCheck);
                Application.SetSystemVariable(systemVarDwgCheck, 2);

                if (File.Exists(aInfo.DwgName))
                {
                    //aDoc = DocMgr.Open(aInfo.DwgName);
                    aDoc = new UtilityClass().ReadDocument(aInfo.DwgName);
                }

                Editor Ed = aDoc.Editor;
                Database Db = aDoc.Database;

                using (DocumentLock DocLock = aDoc.LockDocument())
                {
                    if (aPltCfg.turnOnAllViewPorts)
                    {
                        TurnOnViewports(aDoc);
                    }

                    using (Transaction Tr = Db.TransactionManager.StartTransaction())
                    {
                        BlockTable Bt = (BlockTable)Tr.GetObject(aDoc.Database.BlockTableId, OpenMode.ForRead);
                        PlotInfo PlotInfo = new PlotInfo();

                        // A PlotEngine does the actual plotting (can also create one for Preview)
                        if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                        {
                            PlotEngine PlotEngine = PlotFactory.CreatePublishEngine();
                            using (PlotEngine)
                            {
                                DBDictionary layoutDict = Tr.GetObject(Db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                                var LayoutsToPlot = new ObjectId[layoutDict.Count];
                                int actualLayoutsToPlotCount = 0;
                                foreach (ObjectId BtrId in Bt)
                                {
                                    BlockTableRecord Btr = (BlockTableRecord)Tr.GetObject(BtrId, OpenMode.ForRead);
                                    if (Btr.IsLayout)
                                    {
                                        // traverse all layouts in the collection to check if this one is opted
                                        foreach (LayoutInfo LInfo in aInfo.LayoutInfos)
                                        {
                                            var currLayout = (Layout)Tr.GetObject(Btr.LayoutId, OpenMode.ForRead);
                                            if (currLayout.LayoutName == LInfo.Layout && LInfo.Publish)
                                            {
                                                var frameExtents = FindPlotWindowExtents(Tr, Btr, aPltCfg);
                                                if (frameExtents != new Extents2d())
                                                {
                                                    LayoutsToPlot[currLayout.TabOrder]=BtrId;
                                                    actualLayoutsToPlotCount++;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (actualLayoutsToPlotCount == 0)
                                {
                                    PublishStatusDialog.UpdateFileStatus(
                                        aInfo.Index, strNoLayouts, "", false, aInfo.DwfPubTime);
                                    return;
                                    //Ed.WriteMessage( "There are no layouts in the configuration. File : " + Db.Filename );
                                }
                                
                                // Create a Progress Dialog to provide info and allow the user to cancel
                                PlotProgressDialog PlotProgressDialog = new PlotProgressDialog(
                                    false, actualLayoutsToPlotCount, true);
                                using (PlotProgressDialog)
                                {
                                    int numSheet = 1;
                                    PlotRotation Rotation = PlotRotation.Degrees000;
                                    Extents2d frameExtents = new Extents2d();
                                    var choosenCanonicalPaperSize = aPltCfg.canonicalPaperSize;

                                    foreach (ObjectId LayoutId in LayoutsToPlot)
                                    {
                                        if (LayoutId == new ObjectId())
                                        {
                                            continue;
                                        }
                                        BlockTableRecord LayoutBtr =
                                            (BlockTableRecord)Tr.GetObject(LayoutId, OpenMode.ForRead);
                                        Layout Lt = (Layout)Tr.GetObject(LayoutBtr.LayoutId, OpenMode.ForRead);

                                        string new_canonicalPaperSize = "";

                                        try
                                        {
                                            new_canonicalPaperSize = aPltCfg.canonicalPaperSize;

                                            if ((aPltCfg.canonicalPaperSize != "Auto"))
                                            {
                                                string[] split = new_canonicalPaperSize.Split(new[] { '(', ')' });
                                                string[] split1 = split[1].Split(new[] { '_' });

                                                double w = Convert.ToDouble(split1[0]);
                                                double h = Convert.ToDouble(split1[2]);
                                                bool isRotated = w < h;

                                                frameExtents = FindPlotWindowExtents(Tr, LayoutBtr, aPltCfg);
                                                if (frameExtents == null)
                                                {
                                                    numSheet++;
                                                }
                                                var isR = frameExtents.MaxPoint.Y - frameExtents.MinPoint.Y
                                                          > frameExtents.MaxPoint.X - frameExtents.MinPoint.X;

                                                if (isRotated)
                                                {
                                                    Rotation = isR ? PlotRotation.Degrees000 : PlotRotation.Degrees090;
                                                }
                                                else
                                                {
                                                    Rotation = isR ? PlotRotation.Degrees090 : PlotRotation.Degrees000;
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            aPltCfg.canonicalPaperSize = new_canonicalPaperSize;
                                        }

                                        // If the user selected "Auto" paper attemtp to find the correct paper size 
                                        if (aPltCfg.canonicalPaperSize == "Auto")
                                        {
                                            string[] formats =
                                                {
                                                    "ISO_full_bleed_A4_(210.00_x_297.00_MM)",
                                                    "ISO_full_bleed_A3_(297.00_x_420.00_MM)",
                                                    "ISO_full_bleed_A2_(420.00_x_594.00_MM)",
                                                    "ISO_full_bleed_A1_(594.00_x_841.00_MM)",
                                                    "ISO_full_bleed_A0_(841.00_x_1189.00_MM)"
                                                };
                                            double[] areas =
                                                {
                                                    210.00 * 297.00, 297.00 * 420.00, 420.00 * 594.00,
                                                    594.00 * 841.00, 841.00 * 1189.00
                                                };

                                            frameExtents = FindPlotWindowExtents(Tr, LayoutBtr, aPltCfg);
                                            if (frameExtents == new Extents2d())
                                            {
                                                continue;
                                            }
                                            var isR = frameExtents.MaxPoint.Y - frameExtents.MinPoint.Y
                                                      > frameExtents.MaxPoint.X - frameExtents.MinPoint.X;
                                            var area = Math.Abs(frameExtents.MaxPoint.Y - frameExtents.MinPoint.Y)
                                                       * (frameExtents.MaxPoint.X - frameExtents.MinPoint.X);
                                            int N = 0;
                                            double sub = Math.Abs(area - areas[0]);
                                            for (int i = 0; i < areas.Length; i++)
                                            {
                                                double sub_ = Math.Abs(area - areas[i]);
                                                if (sub_ < sub)
                                                {
                                                    sub = sub_;
                                                    N = i;
                                                }
                                            }

                                            string _canonicalPaperSize = formats[N];

                                            Rotation = isR ? PlotRotation.Degrees000 : PlotRotation.Degrees090;

                                            aPltCfg.canonicalPaperSize = _canonicalPaperSize;
                                        }

                                        PlotInfo = GetValidatedPlotInfo(Db, Lt, aPltCfg, Rotation, frameExtents);

                                        //Only, if this is the first sheet
                                        if (numSheet == 1)
                                        {
                                            PlotProgressDialog.set_PlotMsgString(
                                                PlotMessageIndex.DialogTitle, "KojtoCAD Plot Progress");
                                            PlotProgressDialog.set_PlotMsgString(
                                                PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                                            PlotProgressDialog.set_PlotMsgString(
                                                PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                                            PlotProgressDialog.set_PlotMsgString(
                                                PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                                            PlotProgressDialog.set_PlotMsgString(
                                                PlotMessageIndex.SheetProgressCaption, "Sheet Progress");

                                            PlotProgressDialog.LowerPlotProgressRange = 0;
                                            PlotProgressDialog.UpperPlotProgressRange = 100;
                                            PlotProgressDialog.PlotProgressPos = 0;

                                            // Let's start the plot, at last
                                            PlotProgressDialog.OnBeginPlot();
                                            PlotProgressDialog.IsVisible = true;
                                            PlotEngine.BeginPlot(PlotProgressDialog, null);

                                            // We'll be plotting a single document Let's plot to file (4th parameter)
                                            if (aPltCfg.plotMultiSheet)
                                            {
                                                //string docName
                                                PlotEngine.BeginDocument(
                                                    PlotInfo,
                                                    aDoc.Name,
                                                    null,
                                                    1,
                                                    true,
                                                    aPltCfg.savePath + "\\" + aPltCfg.fileName + "." + ext);
                                                DeleteFile(aPltCfg.savePath + "\\" + aPltCfg.fileName + "." + ext);
                                            }
                                        }

                                        if (!aPltCfg.plotMultiSheet)
                                        {
                                            if (aInfo.LayoutInfos.Count > 0)
                                            {
                                                PlotEngine.BeginDocument(
                                                    PlotInfo,
                                                    aDoc.Name,
                                                    null,
                                                    1,
                                                    true,
                                                    aPltCfg.savePath + "\\" + Lt.LayoutName + "." + ext);
                                            }
                                            else if (aInfo.LayoutInfos.Count == 0)
                                            {
                                                PlotEngine.BeginDocument(
                                                    PlotInfo,
                                                    aDoc.Name,
                                                    null,
                                                    1,
                                                    true,
                                                    aPltCfg.savePath + "\\" + aPltCfg.fileName + ext);
                                            }
                                            DeleteFile(
                                                aPltCfg.savePath + "\\" + aPltCfg.fileName + "_" + Lt.LayoutName + "."
                                                + ext);
                                        }

                                        // Which may contain multiple sheets
                                        PlotProgressDialog.StatusMsgString = "Plotting "
                                                                             + aDoc.Name.Substring(
                                                                                 aDoc.Name.LastIndexOf("\\") + 1)
                                                                             + " - sheet " + numSheet.ToString()
                                                                             + " of " + actualLayoutsToPlotCount;

                                        PlotProgressDialog.OnBeginSheet();
                                        PlotProgressDialog.LowerSheetProgressRange = 0;
                                        PlotProgressDialog.UpperSheetProgressRange = 100;
                                        PlotProgressDialog.SheetProgressPos = 0;
                                        PlotPageInfo ppi = new PlotPageInfo();

                                        PlotProgressDialog.SheetProgressPos = 20;

                                        if (aPltCfg.plotMultiSheet)
                                        {
                                            PlotEngine.BeginPage(ppi, PlotInfo, (numSheet == actualLayoutsToPlotCount), null);
                                        }
                                        else
                                        {
                                            PlotEngine.BeginPage(ppi, PlotInfo, true, null);
                                        }
                                        PlotProgressDialog.SheetProgressPos = 40;

                                        PlotEngine.BeginGenerateGraphics(null);
                                        PlotProgressDialog.SheetProgressPos = 60;
                                        PlotEngine.EndGenerateGraphics(null);

                                        // Finish the sheet
                                        PlotProgressDialog.SheetProgressPos = 80;
                                        PlotEngine.EndPage(null);
                                        PlotProgressDialog.SheetProgressPos = 100;
                                        PlotProgressDialog.OnEndSheet();
                                        numSheet++;

                                        if (!aPltCfg.plotMultiSheet)
                                        {
                                            PlotEngine.EndDocument(null);
                                        }

                                        aPltCfg.canonicalPaperSize = choosenCanonicalPaperSize;
                                    } //end of foreach statement onto a collection 

                                    // Finish the document
                                    if (numSheet != 1)
                                    {
                                        if (aPltCfg.plotMultiSheet)
                                        {
                                            PlotEngine.EndDocument(null);
                                        }

                                        // And finish the plot
                                        PlotProgressDialog.PlotProgressPos = 100;
                                        PlotProgressDialog.OnEndPlot();
                                        PlotEngine.EndPlot(null);
                                    }
                                    else
                                    {
                                        //I know, this is not the best practice... 
                                        throw new Exception(ErrorStatus.OK, "Frame NOT found!");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Ed.WriteMessage("\nAnother plot is in progress.");
                        }
                    }
                }
                Application.SetSystemVariable(systemVarDwgCheck, dwgCheckPrevious);

                PublishStatusDialog.UpdateFileStatus(aInfo.Index, strPublished, "", false, aInfo.DwfPubTime);
            }
            catch (Exception Ex)
            {
                PublishStatusDialog.UpdateFileStatus(aInfo.Index, Ex.Message, "", false, aInfo.DwfPubTime);
            }
        }
#endif
#if !bcad
        private static PlotInfo GetValidatedPlotInfo(
            Database Db, Layout Lt, FilePlotConfig aPltCfg, PlotRotation Rotation, Extents2d frameExtents)
        {
            // Make a PlotSettings object based on the layout settings which we then customize
            PlotSettings plotSettings = new PlotSettings(Lt.ModelType);
            plotSettings.CopyFrom(Lt);

            plotSettings.PlotPlotStyles = true;

            PlotSettingsValidator plotSettingsValidator = PlotSettingsValidator.Current;
            plotSettingsValidator.RefreshLists(plotSettings);
            //This is the right order of setting plotSettings properties;
            //Any shifts may cause eInvalidInput or ePlotInfoValidation
            plotSettingsValidator.SetPlotWindowArea(plotSettings, frameExtents);
            plotSettingsValidator.RefreshLists(plotSettings);

            plotSettingsValidator.SetPlotType(plotSettings, PlotType.Window);
            plotSettingsValidator.RefreshLists(plotSettings);

            plotSettingsValidator.SetUseStandardScale(plotSettings, true);

            plotSettingsValidator.SetStdScaleType(plotSettings, StdScaleType.ScaleToFit);
            plotSettingsValidator.RefreshLists(plotSettings);

            plotSettingsValidator.SetPlotCentered(plotSettings, aPltCfg.centerPlot);

            plotSettingsValidator.SetPlotConfigurationName(plotSettings, aPltCfg.plotDevice, aPltCfg.canonicalPaperSize);
            plotSettingsValidator.RefreshLists(plotSettings);

            plotSettingsValidator.SetCurrentStyleSheet(plotSettings, aPltCfg.plotStyleTable);
            plotSettingsValidator.RefreshLists(plotSettings);

            plotSettingsValidator.SetPlotRotation(plotSettings, Rotation);

            plotSettings.PlotTransparency = aPltCfg.PlotTransparency;
            // We need a PlotInfo object linked to the layout
            PlotInfo plotInfo = new PlotInfo {Layout = Lt.Id};

            // Make the layout we're plotting current
            LayoutManager.Current.CurrentLayout = Lt.LayoutName;

            PlotInfoValidator plotInfoValidator = new PlotInfoValidator
            {
                MediaMatchingPolicy = MatchingPolicy.MatchEnabled
            };

            // We need to link the PlotInfo to the PlotSettings and then validate it
            plotInfo.OverrideSettings = plotSettings;
            plotInfoValidator.Validate(plotInfo);

            // Update the layout plot settings forever
            using (Transaction PlotTr = Db.TransactionManager.StartTransaction())
            {
                Lt.UpgradeOpen();
                Lt.CopyFrom(plotSettings);
                PlotTr.Commit();
            }

            return plotInfo;
        }
          
        private static StringCollection PublishFileA(
            PublishInfo aInfo, ref Document aDoc, ref List<bool> rotations, ref StringCollection formats)
        {
            StringCollection LayoutsToPlot = new StringCollection();
            rotations = new List<bool>();
            try
            {
                DocumentCollection DocMgr = Application.DocumentManager;

                if (File.Exists(aInfo.DwgName))
                {
                    //aDoc = DocMgr.Open(aInfo.DwgName, false);
                    aDoc = new UtilityClass().ReadDocument(aInfo.DwgName);
                }

                if (aDoc == null)
                {
                    return LayoutsToPlot;
                }
                Editor Ed = aDoc.Editor;
                Database Db = aDoc.Database;
                using (DocumentLock DocLock = aDoc.LockDocument())
                {
                    using (Transaction Tr = Db.TransactionManager.StartTransaction())
                    {
                        BlockTable Bt = (BlockTable)Tr.GetObject(Db.BlockTableId, OpenMode.ForRead);

                        // A PlotEngine does the actual plotting (can also create one for Preview)
                        Layout currLayout;
                        var filePlotConfig = new FilePlotConfig
                                                 {
                                                     drawingFrameLayer =
                                                         GetExactBlockAndFrameNames(
                                                             Settings.Default.plotDrawingFrameLayer),
                                                     drawingFrameName =
                                                         GetExactBlockAndFrameNames(
                                                             Settings.Default.plotDrawingFrameName)
                                                 };
                        foreach (ObjectId BtrId in Bt)
                        {
                            BlockTableRecord Btr = (BlockTableRecord)Tr.GetObject(BtrId, OpenMode.ForRead);
                            if (Btr.IsLayout)
                            {
                                // traverse all layouts in the collection to check if this one is opted
                                foreach (LayoutInfo LInfo in aInfo.LayoutInfos)
                                {
                                    currLayout = (Layout)Tr.GetObject(Btr.LayoutId, OpenMode.ForRead);
                                    if (currLayout.LayoutName == LInfo.Layout && LInfo.Publish)
                                    {
                                        LayoutsToPlot.Add(currLayout.LayoutName);

                                        var frameExtents = FindPlotWindowExtents(Tr, Btr, filePlotConfig);
                                        double width = Math.Abs(frameExtents.MaxPoint.X - frameExtents.MinPoint.X);
                                        double height = Math.Abs(frameExtents.MaxPoint.Y - frameExtents.MinPoint.Y);
                                        bool isR = width < height;
                                        rotations.Add(isR);
                                        formats.Add(width * height < (210 * 297 + 500) ? "A4" : "A3");
                                    }
                                }
                            }
                        }

                        if (LayoutsToPlot.Count == 0)
                        {
                            PublishStatusDialog.UpdateFileStatus(aInfo.Index, strNoLayouts, "", false, aInfo.DwfPubTime);
                            Ed.WriteMessage("There are no layouts in the configuration. File : " + Db.Filename);
                        }
                    }

                }
                //return LayoutsToPlot;
            }
            catch /*(Autodesk.AutoCAD.Runtime.Exception Ex)*/
            {
                //MessageBox.Show(Ex.Message + "\n" + Ex.Source + "\n" + Ex.StackTrace);
                // PublishStatusDialog.UpdateFileStatus(aInfo.Index, Ex.Message, "", false, aInfo.DwfPubTime);
            }
            /*         
            string device = Settings.Default.plotDevicePrinter.Trim();
            string PS = Settings.Default.paperSizePrint.Trim();
            foreach (string str in LayoutsToPlot)
            {
                string name = str;
                name.Trim();                
                string command = PrintString(name, device, PS, true);
                MessageBox.Show(command);
                aDoc.SendStringToExecute(command, false, false, true);
            }
        */
            return LayoutsToPlot;
        }
#endif
        private static string PrintString(
            string LayoutName, string DeviceName, string PaperSize, bool DrawingOrientation, string plotStyleTable)
        {
            string command = "-plot";
            command += "\n"; //Entrer

            //Detailed plot configuration [Yes/No] <No>: 
            command += "Yes";
            command += "\n"; //Entrer

            //Enter a layout name or [?] <current>:
            command += LayoutName;
            command += "\n"; //Entrer

            //Enter an output device name or [?] <None>:
            command += DeviceName;
            command += "\n"; //Entrer

            //Enter paper size or [?] <A4>:
            command += PaperSize;
            command += "\n"; //Entrer

            //Enter paper units [Inches/Millimeters] <Millimeters>:
            command += "M";
            command += "\n"; //Entrer

            //Enter drawing orientation [Portrait/Landscape] <Portrait>:
            if (DrawingOrientation)
            {
                command += "P";
            }
            else
            {
                command += "L";
            }
            command += "\n"; //Entrer

            //Plot upside down? [Yes/No] <No>:
            command += "N";
            command += "\n"; //Entrer

            //Enter plot area [Display/Extents/Limits/View/Window] <Display>:
            command += "E";
            command += "\n"; //Entrer

            //Enter plot scale (Plotted Millimeters=Drawing Units) or [Fit] <Fit>:
            command += "Fit";
            command += "\n"; //Entrer

            //Enter plot offset (x,y) or [Center] <11.55,-13.65>:
            command += "Center";
            command += "\n"; //Entrer

            //Plot with plot styles? [Yes/No] <Yes>:
            command += "Y";
            command += "\n"; //Entrer

            //Enter plot style table name or [?] (enter . for none) <>:
            command += plotStyleTable;
            command += "\n"; //Entrer

            //Plot with lineweights? [Yes/No] <Yes>:
            command += "Y";
            command += "\n"; //Entrer

            //Scale lineweights with plot scale? [Yes/No] <No>: 
            command += "N";
            command += "\n"; //Entrer

            //Plot paper space first? [Yes/No] <No>:
            command += "Y";
            command += "\n"; //Entrer

            //Hide paperspace objects? [Yes/No] <No>: 
            command += "N";
            command += "\n"; //Entrer

            //Enter shade plot setting [As displayed/legacy Wireframe/legacy Hidden/Visual styles/Rendered] <As displayed>:
            // command += "A";
            // command += "\n";//Entrer

            //Write the plot to a file [Yes/No] <N>:
            command += "N";
            command += "\n"; //Entrer

            //Save changes to page setup [Yes/No]? <N>
            command += "N";
            command += "\n"; //Entrer

            //Proceed with plot [Yes/No] <Y>:
            command += "Y";
            command += "\n"; //Entrer

            return command;
        }

        private static Extents2d FindPlotWindowExtents(Transaction transaction, BlockTableRecord layoutBtr, FilePlotConfig plotConfig)
        {
            foreach (var objectId in layoutBtr)
            {
                var polyline = transaction.GetObject(objectId, OpenMode.ForRead) as Polyline;
                if (polyline != null && plotConfig.drawingFrameLayer.Contains(polyline.Layer.ToUpper()))
                {
                    return new Extents2d(
                        polyline.GeometricExtents.MinPoint.X,
                        polyline.GeometricExtents.MinPoint.Y,
                        polyline.GeometricExtents.MaxPoint.X,
                        polyline.GeometricExtents.MaxPoint.Y);
                }
                var blockReference = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                if (blockReference != null)
                {
                    // DynamicBlockTableRecord does not return only dynamic block table record. 
                    // When the block is ( for example ) pure polyline in block and not a dynamic block at all then the
                    // DynamicBlockTableRecord property returns the blockTableRecord of the normal block -> they overlap in this case.x
                    
                    //var dynamicBlockId = blockReference.DynamicBlockTableRecord;
                    var blockId = blockReference.DynamicBlockTableRecord;
                    var block = transaction.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                    if (block != null && plotConfig.drawingFrameName.Contains(block.Name.ToUpper()))
                    {
                        return new Extents2d(
                            blockReference.GeometricExtents.MinPoint.X,
                            blockReference.GeometricExtents.MinPoint.Y,
                            blockReference.GeometricExtents.MaxPoint.X,
                            blockReference.GeometricExtents.MaxPoint.Y);
                    }
                }
            }
            return new Extents2d();
        }

        private static bool Same(string first, string second)
        {
            return (string.Compare(first, second, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        private static void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        private static List<string> GetExactBlockAndFrameNames(string mergedNames)
        {
            var separator = new[] { ';' };
            var names = mergedNames.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < names.Length; i++)
            {
                names[i] = names[i].Trim().ToUpper();
            }
            return new List<string>(names);
        }

        private static void TurnOnViewports(Document document)
        {
            Editor editor = document.Editor;
            TypedValue[] viewportFilter = { new TypedValue((int)DxfCode.Start, "Viewport") };

            // try-catch is needed for layouts with no viewport -> the editor throws exception instead of returning an empty selection
            try
            {
                PromptSelectionResult viewportSelection = editor.SelectAll(new SelectionFilter(viewportFilter));
                if ( viewportSelection.Value.Count.Equals(0) )
                {
                    return;
                }
                SelectionSet selectionSet = viewportSelection.Value as SelectionSet;
                using ( Transaction transaction = document.Database.TransactionManager.StartTransaction() )
                {
                    foreach ( ObjectId objectId in selectionSet.GetObjectIds() )
                    {
                        Viewport viewport = (Viewport) transaction.GetObject(objectId, OpenMode.ForWrite);
                        if ( !viewport.On )
                        {
                            try
                            {
                                viewport.On = true;
                                viewport.UpdateDisplay();
                            }
                            catch ( Exception )
                            {
                            }
                        }
                    }
                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                // Exception // eNotApplicable 
                // There are drawings in which the engineers draw directly on the layout 
                // without using any viewports at all.
                // That is why the filter throws exception and we must continue on.
            }
        }
    }

    // Layout info per drawing
    public class LayoutInfo
    {
        private string _layout;
        public string Layout
        {
            set
            {
                _layout = value;
            }
            get
            {
                return _layout;
            }
        }

        private Handle _handle;
        public Handle Handle
        {
            set
            {
                _handle = value;
            }
            get
            {
                return _handle;
            }
        }

        private bool _bIsModel;
        public bool IsModel
        {
            set
            {
                _bIsModel = value;
            }
            get
            {
                return _bIsModel;
            }
        }

        private bool _publish;
        public bool Publish
        {
            set
            {
                _publish = value;
            }
            get
            {
                return _publish;
            }
        }

        public LayoutInfo ( )
        {
            _publish = true;
        }

        public LayoutInfo Copy ( )
        {
            LayoutInfo info = new LayoutInfo( );
            info.Layout = Layout;
            info.Publish = Publish;
            info._bIsModel = _bIsModel;
            return info;
        }

        // Required for check/uncheck feature
        public override string ToString ( )
        {
            return Layout;
        }
    }

    // Info on drawing selected to publish which contains Layout info
    public class PublishInfo
    {
        public PublishInfo ( )
        {
            _layoutInfos = new List<LayoutInfo>( );
            _failedFile = false;
            _skipDwg = false;
        }

        private List<LayoutInfo> _layoutInfos;
        public List<LayoutInfo> LayoutInfos
        {
            set
            {
                _layoutInfos = value;
            }
            get
            {
                return _layoutInfos;
            }
        }

        private string _dwfPubTime = "";
        public string DwfPubTime
        {
            set
            {
                _dwfPubTime = value;
            }
            get
            {
                return _dwfPubTime;
            }
        }

        private string _pdfPubTime = "";
        public string PdfPubTime
        {
            set
            {
                _pdfPubTime = value;
            }
            get
            {
                return _pdfPubTime;
            }
        }

        private string _dwgName;
        public string DwgName
        {
            set
            {
                _dwgName = value;
            }
            get
            {
                return _dwgName;
            }
        }

        private bool _published = false;
        public bool Published
        {
            set
            {
                _published = value;
            }
            get
            {
                return _published;
            }
        }

        private int _idx = 0;
        public int Index
        {
            set
            {
                _idx = value;
            }
            get
            {
                return _idx;
            }
        }

        private bool _canRetry = true;
        public bool CanRetry
        {
            set
            {
                _canRetry = value;
            }
            get
            {
                return _canRetry;
            }
        }

        private bool _skipDwg;
        public bool SkipDwg
        {
            set
            {
                _skipDwg = value;
            }
            get
            {
                return _skipDwg;
            }
        }

        private bool _failedFile;
        public bool Failed
        {
            set
            {
                _failedFile = value;
            }
            get
            {
                return _failedFile;
            }
        }

        public void Add ( string LayoutName, Handle LayoutHandle, bool IsModel )
        {
            LayoutInfo layoutInfo = new LayoutInfo( );
            layoutInfo.Layout = LayoutName;
            layoutInfo.Handle = LayoutHandle;
            layoutInfo.IsModel = IsModel;
            _layoutInfos.Add( layoutInfo );
        }

        public PublishInfo Copy ( )
        {
            PublishInfo copy = new PublishInfo( );
            copy.Failed = _failedFile;
            copy.Index = _idx;
            copy.SkipDwg = _skipDwg;
            copy.DwgName = _dwgName;
            copy.CanRetry = _canRetry;

            foreach ( LayoutInfo info in _layoutInfos )
            {
                LayoutInfo copyInfo = info.Copy( );
                copy.LayoutInfos.Add( copyInfo );
            }
            return copy;
        }

        public override string ToString ( )
        {
            return Path.GetFileNameWithoutExtension( _dwgName );
        }
    }

    public struct FilePlotConfig
    {
        public string plotDevice;
        public string canonicalPaperSize;
        public string plotStyleTable;
        public string savePath;
        public string sourcePath;
        public string fileName;
        public bool PlotTransparency;
        public List<string> drawingFrameLayer;
        public List<string> drawingFrameName;

        public bool plotMultiSheet;
        public bool fitToPage;
        public bool turnOnAllViewPorts;
        public bool ignoreModelSpace;
        public bool centerPlot;

        public SearchOption DirSearchOpts;
        public OutputFileType OutputFileType;

        public FilePlotConfig ( string aPlotDevice,
                              string aCanonicalPaperSize,
                              string aPlotStyle,
                              string aSavePath,
                              string aSourcePath,
                              string aFileName,
                              List<string> aDrawingFrameLayer,
                              List<string> aDrawingFrameName,
                              bool aPlotMultiSheet,
                              bool aTurnOnAllViewPorts,
                              bool aFitToPage,
                              bool aIgnoreModelSpace,
                              bool aCenterPlot,
                              OutputFileType aOutputFileType,
                              SearchOption aDirSearchOpts,
                              bool plotTransparency)
        {
            plotDevice = aPlotDevice;
            canonicalPaperSize = aCanonicalPaperSize;
            plotStyleTable = aPlotStyle;
            savePath = aSavePath;
            sourcePath = aSourcePath;
            fileName = aFileName;
            drawingFrameLayer = aDrawingFrameLayer;
            drawingFrameName = aDrawingFrameName;
            plotMultiSheet = aPlotMultiSheet;
            OutputFileType = aOutputFileType;
            DirSearchOpts = aDirSearchOpts;
            turnOnAllViewPorts = aTurnOnAllViewPorts;
            centerPlot = aCenterPlot;
            fitToPage = aFitToPage;
            ignoreModelSpace = aIgnoreModelSpace;
            PlotTransparency = plotTransparency;
        }
    }

    public enum OutputFileType { PDF = 1, DWF, DWFx, None };

}