using System;
using System.Windows.Forms;
using KojtoCAD.Mathematics.Geometry;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using AcadRegion = Autodesk.AutoCAD.Interop.Common.AcadRegion;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
using Exception = Teigha.Runtime.Exception;
using AcadRegion = BricscadDb.AcadRegion;
#endif

[assembly: CommandClass(typeof(KojtoCAD.RegionCommands.RegionDescription))]

namespace KojtoCAD.RegionCommands
{
    public class RegionDescription
    {
        private static readonly Editor Ed =
            Application.DocumentManager.MdiActiveDocument.Editor;

        private static readonly Database Db =
            Application.DocumentManager.MdiActiveDocument.Database;

        private static readonly DocumentHelper DrawingHelper =
            new DocumentHelper(Application.DocumentManager.MdiActiveDocument);

        private readonly EditorHelper _editorHelper =
            new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        private static ObjectId _textId;

        /// <summary>
        /// Region Description - momentum description
        /// </summary>
        [CommandMethod("regd")]
        public void RegionDescriptionStart()
        {
            
            var sysUnits =
                Application.GetSystemVariable("INSUNITS").ToString();
            if ((sysUnits != "1") && (sysUnits != "2") && (sysUnits != "4") && (sysUnits != "5") && (sysUnits != "6")
                && (sysUnits != "14"))
            {
                MessageBox.Show("\nUnits to scale inserted content: UNRECOGNIZED ?!", "units E R R O R");
                return;
            }

            // Prompt the user to select region mode - points or object
            //var regDescrModeOpts =new PromptKeywordOptions("\nSelect mode [Polylines And Circles/Regions/Block]", "PolylinesAndCircles Regions Block");
            var regDescrModeOpts = new PromptKeywordOptions("")
            {
                Message = "Select cross section mode:"
            };
            regDescrModeOpts.Keywords.Add("PolylinesAndCircles");
            regDescrModeOpts.Keywords.Add("Regions");
            regDescrModeOpts.Keywords.Add("Block");
            var regDescrModeRslt = Ed.GetKeywords(regDescrModeOpts);
            // If the user pressed cancel - return with no error
            if (regDescrModeRslt.Status != PromptStatus.OK)
            {
                return;
            }

            var myRegionColl = new DBObjectCollection();
            var objectsType = typeof(bool);

            switch (regDescrModeRslt.StringResult)
            {
                case "PolylinesAndCircles":

                    #region Contours Selection

                    // Start a transaction
                    using (var tr = Db.TransactionManager.StartTransaction())
                    {
                        #region contour

                        // Request for objects to be selected in the drawing area       \
                        Ed.WriteMessage("\nSelect object describing outer contour");

                        //TypeVals.SetValue(new TypedValue((int)DxfCode.Start , "POLYLINE") , 0);
                        //TypeVals.SetValue(new TypedValue((int)DxfCode.Operator , "OR") , 1);
                        //TypeVals.SetValue(new TypedValue((int)DxfCode.Start , "CIRCLE") , 2);   
                        // Assign the filter criteria to a SelectionFilter object
                        //SelectionFilter SelFilter = new SelectionFilter(TypeVals);

                        //PromptSelectionResult SSPrompt = Ed.GetSelection(SelFilter);
                        var ssPrompt = Ed.GetSelection();

                        // If the prompt status is OK, objects were selected
                        if (ssPrompt.Status == PromptStatus.OK)
                        {

                            var acSSet = ssPrompt.Value;

                            // Step through the objects in the selection set
                            var acDbObjColl = new DBObjectCollection();
                            foreach (SelectedObject acSsObj in acSSet)
                            {
                                // Check to make sure a valid SelectedObject object was returned
                                if (acSsObj == null)
                                {
                                    continue;
                                }
                                // Open the selected object for write
                                var acEnt = tr.GetObject(acSsObj.ObjectId, OpenMode.ForWrite) as Entity;

                                if (acEnt != null)
                                {
                                    acDbObjColl.Add(acEnt);
                                }
                                else
                                {
                                    Ed.WriteMessage("Invalid input.");
                                }
                            }
                            myRegionColl = Region.CreateFromCurves(acDbObjColl);
                        }

                        #endregion

                        tr.Commit();
                    }

                    // If the user did not select any objects
                    if (myRegionColl.Count == 0)
                    {
                        return;
                    }

                    objectsType = typeof(Polyline);

                    #endregion

                    break;

                case "Regions":

                    #region Regions Selection

                    // Prompt for Regions
                    var regionsSsOpts = new PromptSelectionOptions {AllowDuplicates = false};

                    //PromptEntityResult ObjectSelectionRslt = Ed.GetEntity(ObjectSelectionOpts);
                    var regionsSsPrompt = Ed.GetSelection(regionsSsOpts);

                    // If the prompt status is OK, objects were selected
                    if (regionsSsPrompt.Status != PromptStatus.OK)
                    {
                        return;
                    }

                    var regionsacSSet = regionsSsPrompt.Value;
                    // Step through the objects in the selection set
                    using (var tr = Db.TransactionManager.StartTransaction())
                    {
                        var acDbObjColl = new DBObjectCollection();
                        foreach (SelectedObject acSsObj in regionsacSSet)
                        {
                            // Check to make sure a valid SelectedObject object was returned
                            if (acSsObj == null)
                            {
                                continue;
                            }
                            // Open the selected object for write
                            var acEnt = tr.GetObject(acSsObj.ObjectId, OpenMode.ForWrite) as Region;
                            if (acEnt != null)
                            {
                                var acDBObjColl_ = new DBObjectCollection();
                                acEnt.Explode(acDBObjColl_);
                                foreach (Entity acEntt in acDBObjColl_)
                                {
                                    acDbObjColl.Add(acEntt);
                                }
                            }
                        }
                        myRegionColl = Region.CreateFromCurves(acDbObjColl);
                    }

                    // Dispatch function here

                    #endregion

                    objectsType = typeof(Region);
                    break;

                case "Block":

                    #region Block Selection

                    // Ask the user to select a block

                    var peo = new PromptEntityOptions("\nSelect a block:");
                    peo.AllowNone = false;
                    peo.SetRejectMessage("\nMust select a block.");
                    peo.AddAllowedClass(typeof(BlockReference), false);

                    var per = Ed.GetEntity(peo);
                    if (per.Status != PromptStatus.OK)
                    {
                        return;
                    }


                    using (var tr = Db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            var ent = (Entity) tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            var br = ent as BlockReference;

                            if (br != null)
                            {
                                var btr =
                                    (BlockTableRecord) tr.GetObject(br.BlockTableRecord, OpenMode.ForWrite);
                                var acDbObjColl = new DBObjectCollection();
                                foreach (var id in btr)
                                {
                                    var ent2 = (Entity) tr.GetObject(id, OpenMode.ForWrite);
                                    var str = ent2.GetType().ToString().Split('.');
                                    var Str = str[str.Length - 1];

                                    if (Str != "Polyline")
                                    {
                                        if (Str == "Region")
                                        {
                                            var acEnt = (Region) tr.GetObject(ent2.ObjectId, OpenMode.ForWrite);
                                            var acDBObjColl_ = new DBObjectCollection();
                                            acEnt.Explode(acDBObjColl_);
                                            foreach (Entity acEntt in acDBObjColl_)
                                            {
                                                acDbObjColl.Add(acEntt);
                                            }
                                        }
                                        else
                                        {
                                            acDbObjColl.Add(ent2);
                                        }
                                    }
                                    else
                                    {
                                        var pl = ent2 as Polyline;
                                        if (pl != null)
                                        {
                                            if (pl.Closed)
                                            {
                                                acDbObjColl.Add(pl);
                                            }
                                            else
                                            {
                                                if ((pl.StartPoint.DistanceTo(pl.EndPoint) < 0.0000001)
                                                    && (Str == "Polyline"))
                                                {
                                                    acDbObjColl.Add(pl);
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Not Closed PolyLine !");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var crl = ent2 as Circle;
                                            if (crl != null)
                                            {
                                                acDbObjColl.Add(crl);
                                            }
                                        }
                                    }
                                } //
                                myRegionColl = new DBObjectCollection();
                                myRegionColl = Region.CreateFromCurves(acDbObjColl);
                                var ori = new Point3d(0, 0, 0);
                                var pos = br.Position;
                                foreach (Region reg in myRegionColl)
                                {
                                    var Reg = reg;
                                    MoveRegionInOrigin(ref Reg, ref ori, ref pos);
                                }
                                tr.Commit();
                            }
                        }
                        catch (Exception e)
                        {
                            Ed.WriteMessage(e.ToString());
                        }
                    }

                    #endregion

                    objectsType = typeof(BlockReference);
                    break;
            }

            #region Density selection

            /*      PromptDoubleOptions DensityOptions = new PromptDoubleOptions( "Enter density [kg/m3] : " );
              DensityOptions.UseDefaultValue = true;
              DensityOptions.DefaultValue = Settings.Default.RegionDescrDensity;
              PromptDoubleResult DensityResult = Ed.GetDouble( DensityOptions );
              if ( DensityResult.Status != PromptStatus.OK )
              {
                return;
              }
              else if ( DensityResult.Value == 0.0 )
              {
                MessageBox.Show( "Density must be greater than zero!" );
                return;
              }*/
            Settings.Default.RegionDescrDensity = 1000.0 /*DensityResult.Value*/;
            Settings.Default.Save();

            #endregion

            #region Insertion Point selection

            var insPointOptions = new PromptPointOptions("Pick insertion point : ");
            var insPointResult = Ed.GetPoint(insPointOptions);
            if (insPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            #endregion

            if (objectsType.Name == "Boolean")
            {
                MessageBox.Show("Error identifing type.");
                return;
            }
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                var extractedRegion = ExtractRegionFromObjects(ref myRegionColl, tr);
                //RegMassProps = GetRegionMassProperties(ExtractedRegion);

                var regMassProps = new RegionMassProperties(extractedRegion, Settings.Default.RegionDescrDensity)
                {
                    TextInsertionPoint = insPointResult.Value
                };


                var rdForm = new RegionDescriptionForm(regMassProps, Ed.Document);
                rdForm.ShowDialog();
                if (rdForm.DialogResult == DialogResult.OK)
                {
                    _textId = new ObjectId();
                }

                tr.Commit();
            }
        }

        public static void Draw(ref RegionMassProperties regMassProps, string dimstyle)
        {
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                var color = System.Drawing.Color.Red;
                DrawingHelper.LayerManipulator.CreateLayer("DIM", color);
                var acLine1 = new Line(
                    new Point3d(regMassProps.MRegCentroid.X, regMassProps.MRegCentroid.Y + 15.0, 0),
                    new Point3d(regMassProps.MRegCentroid.X, regMassProps.MRegCentroid.Y - 15.0, 0));
                var acLine2 = new Line(
                    new Point3d(regMassProps.MRegCentroid.X - 15.0, regMassProps.MRegCentroid.Y, 0),
                    new Point3d(regMassProps.MRegCentroid.X + 15.0, regMassProps.MRegCentroid.Y, 0));
                acLine1.Layer = "DIM";
                acLine2.Layer = "DIM";

                DrawingHelper.AddEntityDefinitionToCurrentSpace(acLine1, tr);
                DrawingHelper.AddEntityDefinitionToCurrentSpace(acLine2, tr);

                var acCirc = new Circle();
                acCirc.SetDatabaseDefaults();
                acCirc.Center = regMassProps.MRegCentroid;
                acCirc.Radius = 10.0;
                acCirc.Layer = "DIM";
                DrawingHelper.AddEntityDefinitionToCurrentSpace(acCirc, tr);

                var c1 = new Complex(regMassProps.MRegCentroid.X, regMassProps.MMaxPoint.Y);
                var c2 = new Complex(regMassProps.MMaxPoint.X, regMassProps.MMaxPoint.Y);
                AddRegDescrDimLine(c1, c2, 50.0, dimstyle);

                c1 = new Complex(regMassProps.MRegCentroid.X, regMassProps.MMaxPoint.Y);
                c2 = new Complex(regMassProps.MMinPoint.X, regMassProps.MMaxPoint.Y);
                AddRegDescrDimLine(c2, c1, 50.0, dimstyle);

                c1 = new Complex(regMassProps.MMaxPoint.X, regMassProps.MRegCentroid.Y);
                c2 = new Complex(regMassProps.MMaxPoint.X, regMassProps.MMaxPoint.Y);
                AddRegDescrDimLine(c2, c1, 50.0, dimstyle);

                c1 = new Complex(regMassProps.MMaxPoint.X, regMassProps.MRegCentroid.Y);
                c2 = new Complex(regMassProps.MMaxPoint.X, regMassProps.MMinPoint.Y);
                AddRegDescrDimLine(c1, c2, 50.0, dimstyle);

                tr.Commit();
            }
            Ed.Regen();
        }


        private int RegionsRelation(ref Region reg1, ref Region reg2)
        {
            var rez = 0;
            var reg11 = reg1.Clone() as Region;
            var reg22 = reg2.Clone() as Region;

            var area = reg1.Area < reg2.Area ? reg1.Area : reg2.Area;
            var area1 = Math.Abs(reg1.Area - reg2.Area);

            reg11.BooleanOperation(BooleanOperationType.BoolIntersect, reg22);
            reg22.Dispose();
            if (reg11 != null)
            {
                if (reg11.Area > 0)
                {
                    rez = Math.Abs(reg11.Area - area) < 0.000000001 ? 1 : -1;
                }
                reg11.Dispose();
            }

            return rez;
        }

        public Region ExtractRegionFromObjects(ref DBObjectCollection regions, Transaction tr)
        {
            var arr = new int[regions.Count];
            var resultingRegion = new Region();

                BlockTable acBlkTbl;
                acBlkTbl = tr.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                foreach (Region reg in regions)
                {
                    acBlkTblRec.AppendEntity(reg);
                    tr.AddNewlyCreatedDBObject(reg, true);
                }
                for (var i = 0; i < regions.Count; i++)
                {
                    var Reg = (Region)regions[i];
                        // Tr.GetObject(Regions[i].ObjectId, OpenMode.ForWrite) as Region;              
                    for (var j = 0; j < regions.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }
                        var reg = regions[j] as Region;
                        if (reg.Area > Reg.Area)
                        {
                            var test = RegionsRelation(ref Reg, ref reg);
                            if (test == 1)
                            {
                                // Reg.ColorIndex = 1;
                                arr[i] = test;
                            }
                        }
                    }
                }
                var bas = 0;
                for (var i = 0; i < regions.Count; i++)
                {
                    if (arr[i] != 1)
                    {
                        bas = i;
                        break;
                    }
                }
                var regB = tr.GetObject(regions[bas].ObjectId, OpenMode.ForWrite) as Region;
                for (var i = 0; i < regions.Count; i++)
                {
                    if (i == bas)
                    {
                        continue;
                    }
                    var reg = tr.GetObject(regions[i].ObjectId, OpenMode.ForWrite) as Region;
                    if (arr[i] != 1)
                    {
                        regB.BooleanOperation(BooleanOperationType.BoolUnite, reg);
                    }
                }

                for (var i = 0; i < regions.Count; i++)
                {
                    if (i == bas)
                    {
                        continue;
                    }
                    var reg = tr.GetObject(regions[i].ObjectId, OpenMode.ForWrite) as Region;
                    if (arr[i] == 1)
                    {
                        regB.BooleanOperation(BooleanOperationType.BoolSubtract, reg);
                    }
                }
                regB.ColorIndex = 2;
                resultingRegion = regB;

            return resultingRegion;
        }

        public void MoveRegionInOrigin(ref Region reg, ref Point3d from, ref Point3d to)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec =
                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var acVec3D = from.GetVectorTo(to);

                reg.TransformBy(Matrix3d.Displacement(acVec3D));

                acTrans.Commit();
            }
        }

        public static void DrawText(ref RegionMassProperties aRms, string dU, string mU, Database db)
        {
            #region scale

            var scale = "mm";
            var scaleUnitsValue = UnitsValue.Millimeters;
            var sysUnits =
                Application.GetSystemVariable("INSUNITS").ToString();
            switch (sysUnits)
            {
                case "1":
                    scale = "inch";
                    scaleUnitsValue = UnitsValue.Inches;
                    break;
                case "2":
                    scale = "feet";
                    scaleUnitsValue = UnitsValue.Feet;
                    break;
                case "4":
                    scale = "mm";
                    scaleUnitsValue = UnitsValue.Millimeters;
                    break;
                case "5":
                    scale = "cm";
                    scaleUnitsValue = UnitsValue.Centimeters;
                    break;
                case "6":
                    scale = "m";
                    scaleUnitsValue = UnitsValue.Meters;
                    break;
                case "14":
                    scale = "dm";
                    scaleUnitsValue = UnitsValue.Decimeters;
                    break;
                default:
                    MessageBox.Show("\nUnits to scale inserted content: UNRECOGNIZED ?!", "units E R R O R");
                    break;
            }

            var format = "f5";
            var mTextMessage = "";

            #endregion

            mTextMessage += AssemblyText(
                "Area",
                aRms.Area*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.AreaUnit), 2),
                aRms.AreaUnit,
                "2",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Iy",
                aRms.Iy*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.IyUnit), 4),
                aRms.IyUnit,
                "4",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Wy_Upper",
                aRms.WyUpper*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.WyUpperUnit), 3),
                aRms.WyUpperUnit,
                "3",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Wy_Lower",
                aRms.WyLower*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.WyLowerUnit), 3),
                aRms.WyLowerUnit,
                "3",
                scale,
                format);
            mTextMessage += AssemblyText(
                "D_Upper",
                aRms.DUpper*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.DUpperUnit), 1),
                aRms.DUpperUnit,
                "",
                scale,
                format);
            mTextMessage += AssemblyText(
                "D_Lower",
                aRms.DLower*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.DLowerUnit), 1),
                aRms.DLowerUnit,
                "",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Iyy",
                aRms.Iyy*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.IyyUnit), 1),
                aRms.IyyUnit,
                "",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Iz",
                aRms.Iz*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.IzUnit), 4),
                aRms.IzUnit,
                "4",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Wz_Right",
                aRms.WzRight*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.WzRightUnit), 3),
                aRms.WzRightUnit,
                "3",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Wz_Left",
                aRms.WzLeft*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.WzLeftUnit), 3),
                aRms.WzLeftUnit,
                "3",
                scale,
                format);
            mTextMessage += AssemblyText(
                "D_Right",
                aRms.DRight*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.DRightUnit), 1),
                aRms.DRightUnit,
                "",
                scale,
                format);
            mTextMessage += AssemblyText(
                "D_Left",
                aRms.DLeft*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.DLeftUnit), 1),
                aRms.DLeftUnit,
                "",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Izz",
                aRms.Izz*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.IzzUnit), 1),
                aRms.IzzUnit,
                "",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Imin",
                aRms.Imin*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.IminUnit), 4),
                aRms.IminUnit,
                "4",
                scale,
                format);
            mTextMessage += AssemblyText(
                "Imax",
                aRms.Imax*Math.Pow(UnitsConverter.GetConversionFactor(scaleUnitsValue, aRms.ImaxUnit), 4),
                aRms.ImaxUnit,
                "4",
                scale,
                format);

            // Density
            var density = aRms.Density*UnitsConverter.GetConversionFactor(UnitsValue.Millimeters, aRms.DensityUnit);
            mTextMessage += "{\\A0Density:\\~" + density.ToString(scale != "m" ? format : "") + "\\~" + dU +
                            "\\S3;}\\P\n";

            // weight G
            var g = aRms.G*UnitsConverter.GetConversionFactor(UnitsValue.Millimeters, aRms.GUnit);
            mTextMessage += "{\\A0G:\\~" + g.ToString(scale != "m" ? format : "") + "\\~" + mU + "}\\P\n";
            Ed.WriteMessage(mTextMessage);

            // draw message
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable) tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var btr = (BlockTableRecord) tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                if (!_textId.IsNull && !_textId.IsErased)
                {
                    var regoinTextEnt = (Entity) tr.GetObject(_textId, OpenMode.ForWrite);
                    regoinTextEnt.Erase(true);
                }

                var regionText = new MText();
                regionText.SetDatabaseDefaults();
                regionText.Location = aRms.TextInsertionPoint;
                regionText.Width = 1000;
                regionText.Height = 1.2;
                regionText.Contents = mTextMessage + "\n";
                btr.AppendEntity(regionText);
                _textId = regionText.ObjectId;
                tr.AddNewlyCreatedDBObject(regionText, true);
                tr.Commit();
            }

            CommandLineHelper.Command("_REGEN");
        }

        private static void AddRegDescrDimLine(Complex c1, Complex c2, double k, string dimstyle)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                var acBlkTbl = (BlockTable) acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);

                // Open the Block table record Model space for write
                var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Create the  dimension

                var qq = new Complex((c2 - c1) * Complex.polar(1.0, Math.PI / 2.0));
                qq /= qq.abs();
                qq *= k;
                qq += (c2 + c1) / 2.0;

                var acRotDim = new AlignedDimension();
                acRotDim.SetDatabaseDefaults();
                acRotDim.XLine1Point = new Point3d(c1.real(), c1.imag(), 0);
                acRotDim.XLine2Point = new Point3d(c2.real(), c2.imag(), 0);
                acRotDim.DimLinePoint = new Point3d(qq.real(), qq.imag(), 0);

                try
                {
                    // Open the DimStyle table for read
                    var acDimStyleTbl = (DimStyleTable) acTrans.GetObject(db.DimStyleTableId, OpenMode.ForRead);

                    var acDimStyleTblRec1 =
                        (DimStyleTableRecord) acTrans.GetObject(acDimStyleTbl[dimstyle], OpenMode.ForWrite);
                    acRotDim.SetDimstyleData(acDimStyleTblRec1);
                    acRotDim.DimensionStyle = acDimStyleTbl[dimstyle];

                }
                catch
                {
                    acRotDim.DimensionStyle = db.Dimstyle;
                }


                acRotDim.LayerId = DrawingHelper.LayerManipulator.CreateLayer("DIM", System.Drawing.Color.Red);

                // Add the new object to Model space and the transaction
                acBlkTblRec.AppendEntity(acRotDim);
                acTrans.AddNewlyCreatedDBObject(acRotDim, true);

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }

        }

        private static string AssemblyText(
            string name, double aP, UnitsValue uv, string grade, string scale, string format)
        {
            switch (uv)
            {
                case UnitsValue.Millimeters:
                    scale = "mm";
                    break;
                case UnitsValue.Centimeters:
                    scale = "cm";
                    break;
                case UnitsValue.Decimeters:
                    scale = "dm";
                    break;
                case UnitsValue.Meters:
                    scale = "m";
                    break;
                case UnitsValue.Inches:
                    scale = "inch";
                    break;
                case UnitsValue.Feet:
                    scale = "feet";
                    break;
                default:
                    throw new ArgumentException("Bad conversion unit in DrawText()!");
            }

            var rez = "{\\A0" + name + ":\\~" + aP.ToString(scale != "m" ? format : "") + "\\~" + scale + "\\S" + grade
                         + ";}\\P\n";
            return rez;
        }
    }

    public class RegionMassProperties
    {
        public Point3d MMaxPoint;

        public Point3d MMinPoint;

        public Point3d MRegCentroid;

        public Point3d TextInsertionPoint;

        #region Mass Properties Init to 0

        public double Area;

        public UnitsValue AreaUnit;

        public double Iy;

        public UnitsValue IyUnit;

        public double Iyy;

        public UnitsValue IyyUnit;

        public double WyUpper;

        public UnitsValue WyUpperUnit;

        public double WyLower;

        public UnitsValue WyLowerUnit;

        public double DUpper;

        public UnitsValue DUpperUnit;

        public double DLower;

        public UnitsValue DLowerUnit;

        public double Iz;

        public UnitsValue IzUnit;

        public double Izz;

        public UnitsValue IzzUnit;

        public double WzRight;

        public UnitsValue WzRightUnit;

        public double WzLeft;

        public UnitsValue WzLeftUnit;

        public double DRight;

        public UnitsValue DRightUnit;

        public double DLeft;

        public UnitsValue DLeftUnit;

        public double Imin;

        public UnitsValue IminUnit;

        public double Imax;

        public UnitsValue ImaxUnit;

        public double Density;

        public UnitsValue DensityUnit = UnitsValue.Millimeters;

        public double G;

        public UnitsValue GUnit = UnitsValue.Millimeters;

        public double LinearVolume;

        public UnitsValue LinearVolumeUnit = UnitsValue.Millimeters;

        #endregion

        public RegionMassProperties()
        {
            var scaleUnitsValue = UnitsValue.Millimeters;
            var sysUnits =
                Application.GetSystemVariable("INSUNITS").ToString();
            switch (sysUnits)
            {
                case "1":
                    scaleUnitsValue = UnitsValue.Inches;
                    break;
                case "2":
                    scaleUnitsValue = UnitsValue.Feet;
                    break;
                case "4":
                    scaleUnitsValue = UnitsValue.Millimeters;
                    break;
                case "5":
                    scaleUnitsValue = UnitsValue.Centimeters;
                    break;
                case "6":
                    scaleUnitsValue = UnitsValue.Meters;
                    break;
                case "14":
                    scaleUnitsValue = UnitsValue.Decimeters;
                    break;
                default:
                    MessageBox.Show("\nUnits to scale inserted content: UNRECOGNIZED ?!", "units E R R O R");
                    break;
            }
            AreaUnit = scaleUnitsValue;
            IyUnit = scaleUnitsValue;
            WyUpperUnit = scaleUnitsValue;
            WyLowerUnit = scaleUnitsValue;
            DUpperUnit = scaleUnitsValue;
            DLowerUnit = scaleUnitsValue;
            IyyUnit = scaleUnitsValue;
            IzUnit = scaleUnitsValue;
            WzRightUnit = scaleUnitsValue;
            WzLeftUnit = scaleUnitsValue;
            DRightUnit = scaleUnitsValue;
            DLeftUnit = scaleUnitsValue;
            IzzUnit = scaleUnitsValue;
            IminUnit = scaleUnitsValue;
            ImaxUnit = scaleUnitsValue;
        }

        public RegionMassProperties(Region reg, double density)
        {

            var k = 1.0;
            var scaleUnitsValue = UnitsValue.Millimeters;
            var sysUnits = Application.GetSystemVariable("INSUNITS").ToString();
            switch (sysUnits)
            {
                case "1":
                    scaleUnitsValue = UnitsValue.Inches;
                    k = 25.4;
                    break;
                case "2":
                    scaleUnitsValue = UnitsValue.Feet;
                    k = 25.4 * 12.0;
                    break;
                case "4":
                    scaleUnitsValue = UnitsValue.Millimeters;
                    k = 1.0;
                    break;
                case "5":
                    scaleUnitsValue = UnitsValue.Centimeters;
                    k = 10.0;
                    break;
                case "6":
                    scaleUnitsValue = UnitsValue.Meters;
                    k = 1000.0;
                    break;
                case "14":
                    scaleUnitsValue = UnitsValue.Decimeters;
                    k = 100.0;
                    break;
                default:
                    MessageBox.Show("\nUnits to scale inserted content: UNRECOGNIZED ?!", "units E R R O R");
                    break;
            }
            AreaUnit = scaleUnitsValue;
            IyUnit = scaleUnitsValue;
            WyUpperUnit = scaleUnitsValue;
            WyLowerUnit = scaleUnitsValue;
            DUpperUnit = scaleUnitsValue;
            DLowerUnit = scaleUnitsValue;
            IyyUnit = scaleUnitsValue;
            IzUnit = scaleUnitsValue;
            WzRightUnit = scaleUnitsValue;
            WzLeftUnit = scaleUnitsValue;
            DRightUnit = scaleUnitsValue;
            DLeftUnit = scaleUnitsValue;
            IzzUnit = scaleUnitsValue;
            IminUnit = scaleUnitsValue;
            ImaxUnit = scaleUnitsValue;

            // area
            Area = reg.Area;

            // Linear Volume
            Density = density;
            LinearVolume = Area * k * k / (1000.0 * 1000.0);

            // weight G
            G = Density * LinearVolume;

            var Reg = (AcadRegion) reg.AcadObject;
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            var tempCentroid = (double[])Reg.Centroid;
            MRegCentroid = new Point3d(tempCentroid[0], tempCentroid[1], 0.0);

            MMaxPoint = reg.GeometricExtents.MaxPoint;
            MMinPoint = reg.GeometricExtents.MinPoint;

            // Extents

            var maxZ = Math.Abs(reg.GeometricExtents.MaxPoint.Y - MRegCentroid.Y);
            var minZ = Math.Abs(reg.GeometricExtents.MinPoint.Y - MRegCentroid.Y);
            var maxY = Math.Abs(reg.GeometricExtents.MaxPoint.X - MRegCentroid.X);
            var minY = Math.Abs(reg.GeometricExtents.MinPoint.X - MRegCentroid.X);
            DUpper = maxZ;
            DLower = minZ;
            DRight = maxY;
            DLeft = minY;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var origin = new Point3d(0, 0, 0);
                reg = (Region) tr.GetObject(reg.ObjectId, OpenMode.ForWrite);

                var acVec3D = MRegCentroid.GetVectorTo(origin);
                reg.TransformBy(Matrix3d.Displacement(acVec3D));

                Reg = (AcadRegion)reg.AcadObject;

                var principalMoments = (double[])Reg.PrincipalMoments;
                var momentOfInertia = (double[])Reg.MomentOfInertia;

                // Y Moments     
                Iy = momentOfInertia[0];
                WyUpper = Iy / maxZ;
                WyLower = Iy / minZ;
                Iyy = Math.Sqrt(Iy / Reg.Area);

                // Z Moments        
                Iz = momentOfInertia[1];
                WzRight = Iz / maxY;
                WzLeft = Iz / minY;
                Izz = Math.Sqrt(Iz / Reg.Area);


                // principal moments       
                Imin = principalMoments[0];
                Imax = principalMoments[1];

                Reg.Erase();
                tr.Commit();
            }
        }
    }
}
