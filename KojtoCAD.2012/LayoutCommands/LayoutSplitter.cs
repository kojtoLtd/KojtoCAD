using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.Linq;
using System.IO;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
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
using Application = Bricscad.ApplicationServices.Application;
#endif

[assembly: CommandClass(typeof(KojtoCAD.LayoutCommands.LayoutSplitter))]

namespace KojtoCAD.LayoutCommands
{
    public class LayoutSplitter
    {
        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
        private readonly DocumentHelper _documentHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        private readonly UtilityClass _utilityClass = new UtilityClass();

        /// <summary>
        /// Layout Splitter
        /// </summary>
        [CommandMethod("LayoutToFile_2012", "DV", null, CommandFlags.Modal, null, "LayoutToFile_2012", "LayoutToFile_2012")]
        public void StartLayoutSplitterStart()
        {
            var keywords = new[] { "EachLayoutToSeparateFile", "GroupsOfLayouts" };
            var insulationModeResult = _editorHelper.PromptForKeywordSelection("How to split?", keywords, false, keywords[0]);
            if (insulationModeResult.Status != PromptStatus.OK)
            {
                return;
            }

            if (insulationModeResult.StringResult == keywords[0])
            {
                SplitEverySingleLayout();
                return;
            }

            if (insulationModeResult.StringResult == keywords[1])
            {
                SplitLayoutsByGroup();
                return;
            }
        }

        private void SplitLayoutsByGroup()
        {
            var layouts = _documentHelper.GetLayouts().OrderBy(x => x).ToArray();
            var dialog = new LayoutsGroups(layouts);
            var dialogResult = _utilityClass.ShowDialog(dialog);
            if (dialogResult != DialogResult.OK)
            {
                _editorHelper.WriteMessage(dialogResult.ToString());
                return;
            }

            var dir = dialog.ResultsPath;
            var resultFiles = dialog.ResultFiles;

            _documentHelper.SwitchToModelSpace();

            Zoom(new Point3d(), new Point3d(), new Point3d(), 1.01075);

            var savefidelity = Application.GetSystemVariable("SAVEFIDELITY");
            Application.SetSystemVariable("SAVEFIDELITY", 0);
            var regenmode = Application.GetSystemVariable("REGENMODE");
            Application.SetSystemVariable("REGENMODE", 0);

            Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem = Matrix3d.Identity;

            Application.MainWindow.Visible = false;

            var allUsedLayotus = resultFiles.SelectMany(x => x.Layouts).Distinct().ToArray();

            Document doc = Application.DocumentManager.MdiActiveDocument;

            // If a layout does not have any Viewports inside, it will not be present in collection
            var layoutsWithViewports = LayoutHelper.CreateArrayOfLayoutNamesAndViewportsDiagonals(allUsedLayotus);

            var visibleEntitiesFromLayouts = layoutsWithViewports.Select(x =>
            new
            {
                Layout = x,
                ObjectIds = FindVisibleModelEntitiesFromLayout(x)
            }).ToArray();

            var allModelEntities = _documentHelper.GetAllModelSpaceObjects();

            // Proceed with SaveAs
            using (var _ = doc.LockDocument())
            {
                foreach (var resultFile in resultFiles)
                {
                    using (var tr = doc.Database.TransactionManager.StartTransaction())
                    {
                        // Delete all other layouts
                        foreach (var l in layouts)
                        {
                            if (!resultFile.Layouts.Contains(l))
                            {
                                LayoutManager.Current.DeleteLayout(l);
                            }

                        }

                        var visibleEntitiesByLayouts = visibleEntitiesFromLayouts
                            .Where(x =>
                                resultFile.Layouts.Contains(x.Layout.LayoutName)
                             )
                            .Select(x => x.ObjectIds).ToArray();

                        var visibleEntities = new HashSet<ObjectId>();
                        foreach (var entitiesByLayout in visibleEntitiesByLayouts)
                        {
                            foreach (ObjectId entityId in entitiesByLayout)
                            {
                                visibleEntities.Add(entityId);
                            }
                        }

                        // Delete not seen objects
                        foreach (ObjectId entityId in allModelEntities)
                        {
                            if (visibleEntities.Contains(entityId))
                            {
                                continue;
                            }

                            tr.GetObject(entityId, OpenMode.ForWrite).Erase();
                        }

                        Application.SetSystemVariable("REGENMODE", 1);

                        doc.Database.SaveAs(Path.Combine(dir, $"{resultFile.FileName}.dwg"), doc.Database.OriginalFileVersion);

                        Application.SetSystemVariable("REGENMODE", 0);
                    }
                }
            }

            Application.MainWindow.Visible = true;

            Application.SetSystemVariable("SAVEFIDELITY", savefidelity);
            Application.SetSystemVariable("REGENMODE", regenmode);
        }

        private void SplitEverySingleLayout()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Application.SetSystemVariable("TILEMODE", 1);

            Zoom(new Point3d(), new Point3d(), new Point3d(), 1.01075);

            var savefidelity = Application.GetSystemVariable("SAVEFIDELITY");
            Application.SetSystemVariable("SAVEFIDELITY", 0);
            var regenmode = Application.GetSystemVariable("REGENMODE");
            Application.SetSystemVariable("REGENMODE", 0);

            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            PromptStringOptions prefixOptions = new PromptStringOptions("Enter Prefix :");
            PromptResult prefixResult = ed.GetString(prefixOptions);
            if (prefixResult.Status != PromptStatus.OK)
            {
                return;
            }
            PromptStringOptions sufixOptions = new PromptStringOptions("\n\rEnter Sufix :");
            PromptResult sufixResult = ed.GetString(sufixOptions);
            if (sufixResult.Status != PromptStatus.OK)
            {
                return;
            }

            string selectedFolder = GetFolder();// Името на каталога в който ще се запсват файловете
            if (selectedFolder.Length < 2) { return; }
            selectedFolder = selectedFolder.Replace(@"\", @"\\");
            selectedFolder += prefixResult.StringResult;

            Application.MainWindow.Visible = false;

            try
            {
                var modelEntitiesGroupedByLayouts = new List<Pair<LayoutForSplitting, ObjectIdCollection>>();
                using (doc.LockDocument())
                {
                    using (Transaction transaction = doc.TransactionManager.StartTransaction())
                    {
                        var layoutsWithViewports = LayoutHelper.CreateArrayOfLayoutNamesAndViewportsDiagonals();

                        _documentHelper.SwitchToModelSpace();

                        foreach (var layout in layoutsWithViewports)
                        {
                            var visibleEntitiesFromLayout = FindVisibleModelEntitiesFromLayout(layout);
                            if (visibleEntitiesFromLayout.Count > 0)
                            {
                                modelEntitiesGroupedByLayouts.Add(new Pair<LayoutForSplitting, ObjectIdCollection>(layout, visibleEntitiesFromLayout));
                            }
                        }
                    }
                }

                var allModelEntities = _documentHelper.GetAllModelSpaceObjects();

                using (DocumentLock DocLock = doc.LockDocument())
                {
                    foreach (var layoutAndVisibleEntities in modelEntitiesGroupedByLayouts)
                    {
                        using (Transaction acTrans = db.TransactionManager.StartTransaction())
                        {
                            // Erase all hidden entities
                            foreach (ObjectId modelEntity in allModelEntities)
                            {
                                bool exist = false;
                                foreach (ObjectId visibleEntity in layoutAndVisibleEntities.Second)
                                {
                                    if (visibleEntity == modelEntity)
                                    {
                                        exist = true;
                                        break;
                                    }
                                }
                                if (!exist)
                                {
                                    Entity ent = null;
                                    try
                                    {
                                        ent = acTrans.GetObject(modelEntity, OpenMode.ForWrite) as Entity;
                                    }
                                    catch { }

                                    ent?.Erase();
                                }
                            }

                            // Delete all other layouts but current
                            foreach (var it in modelEntitiesGroupedByLayouts)
                            {
                                if (it.First.LayoutName != layoutAndVisibleEntities.First.LayoutName)
                                {
                                    LayoutManager.Current.DeleteLayout(it.First.LayoutName);
                                }
                            }

                            // Requested for the output file.
                            Application.SetSystemVariable("REGENMODE", 1);

                            // Save as new file with deleted entities and layouts
                            try
                            {
                                db.SaveAs(selectedFolder + layoutAndVisibleEntities.First.LayoutName + sufixResult.StringResult + ".DWG", DwgVersion.Current);
                            }
                            catch { }

                            Application.SetSystemVariable("REGENMODE", 0);
                            // Do not commit the transaction. All erased objects will be resurrected.
                        }
                    }
                }

            }
            catch { }

            Application.MainWindow.Visible = true;

            Application.SetSystemVariable("SAVEFIDELITY", savefidelity);
            Application.SetSystemVariable("REGENMODE", regenmode);
        }

        private ObjectIdCollection FindVisibleModelEntitiesFromLayout(LayoutForSplitting layout)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = new ObjectIdCollection();

            foreach (var viewport in layout.Viewports)
            {
                var selectionResult = ed.SelectCrossingWindow(viewport.TopRight, viewport.BottomLeft);
                if (selectionResult.Status == PromptStatus.OK)
                {
                    SelectionSet selectedEntities = selectionResult.Value;
                    foreach (var id in selectedEntities.GetObjectIds())
                    {
                        res.Add(id);
                    }
                }
            }
            return res;
        }

        private string GetFolder()
        {
            string SelectedFolder = "";
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SelectedFolder = folderBrowserDialog.SelectedPath;
            }
            return SelectedFolder + "\\";
        }

        private void Zoom(Point3d pMin, Point3d pMax, Point3d pCenter, double dFactor)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            int nCurVport = System.Convert.ToInt32(Application.GetSystemVariable("CVPORT"));

            // Get the extents of the current space no points 
            // or only a center point is provided
            // Check to see if Model space is current
            if (acCurDb.TileMode == true)
            {
                if (pMin.Equals(new Point3d()) == true &&
                    pMax.Equals(new Point3d()) == true)
                {
                    pMin = acCurDb.Extmin;
                    pMax = acCurDb.Extmax;
                }
            }
            else
            {
                // Check to see if Paper space is current
                if (nCurVport == 1)
                {
                    // Get the extents of Paper space
                    if (pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true)
                    {
                        pMin = acCurDb.Pextmin;
                        pMax = acCurDb.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if (pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true)
                    {
                        pMin = acCurDb.Extmin;
                        pMax = acCurDb.Extmax;
                    }
                }
            }

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Get the current view
                using (ViewTableRecord acView = acDoc.Editor.GetCurrentView())
                {
                    Extents3d eExtents;

                    // Translate WCS coordinates to DCS
                    Matrix3d matWCS2DCS;
                    matWCS2DCS = Matrix3d.PlaneToWorld(acView.ViewDirection);
                    matWCS2DCS = Matrix3d.Displacement(acView.Target - Point3d.Origin) * matWCS2DCS;
                    matWCS2DCS = Matrix3d.Rotation(-acView.ViewTwist,
                                                   acView.ViewDirection,
                                                   acView.Target) * matWCS2DCS;

                    // If a center point is specified, define the min and max 
                    // point of the extents
                    // for Center and Scale modes
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        pMin = new Point3d(pCenter.X - (acView.Width / 2),
                                           pCenter.Y - (acView.Height / 2), 0);

                        pMax = new Point3d((acView.Width / 2) + pCenter.X,
                                           (acView.Height / 2) + pCenter.Y, 0);
                    }

                    // Create an extents object using a line
                    using (Line acLine = new Line(pMin, pMax))
                    {
                        eExtents = new Extents3d(acLine.Bounds.Value.MinPoint,
                                                 acLine.Bounds.Value.MaxPoint);
                    }

                    // Calculate the ratio between the width and height of the current view
                    double dViewRatio;
                    dViewRatio = (acView.Width / acView.Height);

                    // Tranform the extents of the view
                    matWCS2DCS = matWCS2DCS.Inverse();
                    eExtents.TransformBy(matWCS2DCS);

                    double dWidth;
                    double dHeight;
                    Point2d pNewCentPt;

                    // Check to see if a center point was provided (Center and Scale modes)
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        dWidth = acView.Width;
                        dHeight = acView.Height;

                        if (dFactor == 0)
                        {
                            pCenter = pCenter.TransformBy(matWCS2DCS);
                        }

                        pNewCentPt = new Point2d(pCenter.X, pCenter.Y);
                    }
                    else // Working in Window, Extents and Limits mode
                    {
                        // Calculate the new width and height of the current view
                        dWidth = eExtents.MaxPoint.X - eExtents.MinPoint.X;
                        dHeight = eExtents.MaxPoint.Y - eExtents.MinPoint.Y;

                        // Get the center of the view
                        pNewCentPt = new Point2d(((eExtents.MaxPoint.X + eExtents.MinPoint.X) * 0.5),
                                                 ((eExtents.MaxPoint.Y + eExtents.MinPoint.Y) * 0.5));
                    }

                    // Check to see if the new width fits in current window
                    if (dWidth > (dHeight * dViewRatio)) dHeight = dWidth / dViewRatio;

                    // Resize and scale the view
                    if (dFactor != 0)
                    {
                        acView.Height = dHeight * dFactor;
                        acView.Width = dWidth * dFactor;
                    }

                    // Set the center of the view
                    acView.CenterPoint = pNewCentPt;

                    // Set the current view
                    acDoc.Editor.SetCurrentView(acView);
                }

                // Commit the changes
                acTrans.Commit();
            }
        }
    }
}
