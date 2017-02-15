using Castle.Core.Logging;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using System;
using System.Collections;
using System.Windows.Forms;
using KojtoCAD.Mathematics.Geometry;
using Line2d = KojtoCAD.Mathematics.Geometry.KLine2D;
#if !bcad
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
using Application = Bricscad.ApplicationServices.Application;
using Exception = Teigha.Runtime.Exception;
#endif
[assembly: CommandClass(typeof(KojtoCAD.GraphicItems.SheetProfile.SheetProfile))]

namespace KojtoCAD.GraphicItems.SheetProfile
{
    public class SheetProfile
    {
        private static readonly Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;
        private static readonly Database Db = Application.DocumentManager.MdiActiveDocument.Database;
        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        public static ObjectId PolylineId;
        public static ObjectId PolylineId1;
        private static int _counter;                   //slugi za proverka dali e pyrwo izvikvane
        static SheetProfileGenerationForm _sheetProfileForm;
        private static Polyline _basePolyline;
        private static Polyline _firstPoly;
        private static ArrayList _coatingLinesArray;       //Array of CoatingPolylines 
        private static ArrayList _alignedDimensionsArray;  //array of dimenisions
        private static ArrayList _angularDimensionArray;   //array of dimenisions
        private static ArrayList _rotatedDimensionArray;   //array of dimenisions
        private static ArrayList _directions;              //yglite za orazmerqvaneto- projections 4rez RotatedDimension


        private static Polyline _basePolyLine;
        private static int _num1; //first end most segment number
        private static int _num2; // second endmost segment number
        private static ILogger _logger = NullLogger.Instance;

        public static ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        private int CountOfArcs()
        {
            var rez = 0;
            for (var i = 0; i < _basePolyLine.NumberOfVertices; i++)
            {
                if (_basePolyLine.GetBulgeAt(i) != 0)
                {
                    rez++; // save indeks i N               
                }
            }
            return rez;
        }
        private double GetThickness()
        {
            var rezult = -1.0;
            var n = -1;
            #region Search_first_ARC_segment_and_save_index_in_N
            for (var i = 0; i < _basePolyLine.NumberOfVertices; i++)
            {
                if (_basePolyLine.GetBulgeAt(i) != 0)
                {
                    n = i; // save indeks i N
                    break; // stop the cycle 
                }
            }
            #endregion

            if ((n < 0) || (n > _basePolyLine.NumberOfVertices - 1))
            {
                return rezult;
            }
            var dist = -1.0;
            var n1 = GetParalellArc(n, ref dist);

            if ((n1 <= 0) || (n1 >= _basePolyLine.NumberOfVertices - 1))
            {
                return rezult;
            }
            var seg = _basePolyLine.GetLineSegment2dAt(n);// find chord
            var seg1 = _basePolyLine.GetLineSegment2dAt(n1);// find chord

            //chords are opposite vectors
            var p1 = seg.StartPoint;
            var p2 = seg1.EndPoint;

            rezult = p1.GetDistanceTo(p2);

            return rezult;
        }
        private static int GetParalellArc(int j, ref double distance)
        {
            var n = -1;
            double dist = -1;
            if (_basePolyLine.GetBulgeAt(j) != 0.0)
            {
                var seg = _basePolyLine.GetLineSegment2dAt(j);
                for (var i = 0; i < _basePolyLine.NumberOfVertices; i++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    if (_basePolyLine.GetBulgeAt(i) == 0.0)
                    {
                        continue;
                    }
                    var seg1 = _basePolyLine.GetLineSegment2dAt(i);
                    if (!seg.IsParallelTo(seg1))
                    {
                        continue;
                    }
                    if (n < 0)
                    {
                        dist = seg.GetDistanceTo(seg1);
                        n = i;
                    }
                    else
                    {
                        var d = seg.GetDistanceTo(seg1);
                        if (!(d < dist))
                        {
                            continue;
                        }
                        dist = d;
                        n = i;
                    }
                }
            }
            distance = dist;
            return n;
        }
        public void DrawPolylineFromProfile(int n1, int n2, double dist, Point3d polyLineInsPoint)
        {
            DrawOffsetA(n1, n2, -dist / 2, true, polyLineInsPoint);
        }

        private static void DrawOffsetA(int n1, int n2, double dist, bool boo, Point3d polyLineInsPoint)
        {
            using (var acTrans = Db.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(Db.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                var acBlkTblRec =
                    (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var bpLine = (Polyline)acTrans.GetObject(PolylineId, OpenMode.ForWrite);

                var off = -1;
                var acPoly1 = new Polyline();
                acPoly1.SetDatabaseDefaults();

                #region make poly
                var arrL = new ArrayList();
                for (var j = 1; j <= bpLine.NumberOfVertices >> 1; j++)
                {
                    if (n1 + j > bpLine.NumberOfVertices - 1)
                    {
                        off++;
                    }
                    var i = off < 0 ? n1 + j : off;
                    var seg = bpLine.GetLineSegment2dAt(i);
                    double bulge;
                    if (i - 1 < 0)
                    {
                        bulge = bpLine.GetBulgeAt(bpLine.NumberOfVertices - 1);
                    }
                    else
                    {
                        if (i - 1 > bpLine.NumberOfVertices - 1)
                        {
                            bulge = bpLine.GetBulgeAt(0);
                        }
                        else
                        {
                            bulge = -bpLine.GetBulgeAt(i - 1);
                        }

                    }
                    acPoly1.AddVertexAt(0, new Point2d(seg.StartPoint.X, seg.StartPoint.Y), bulge, 0, 0);
                    arrL.Add(bpLine.GetBulgeAt(i));
                }

                acBlkTblRec.AppendEntity(acPoly1);
                acTrans.AddNewlyCreatedDBObject(acPoly1, true);
                #endregion

                #region coating
                var acPoly11 = new Polyline();
                if (boo)
                {
                    var seg1 = acPoly1.GetLineSegment2dAt(0);
                    var seg2 = bpLine.GetLineSegment2dAt(n2);
                    var mid = new Complex((seg2.StartPoint.X + seg2.EndPoint.X) / 2, (seg2.StartPoint.Y + seg2.EndPoint.Y) / 2);
                    var l = new Line2d(new Complex(seg1.StartPoint.X, seg1.StartPoint.Y), new Complex(seg1.EndPoint.X, seg1.EndPoint.Y));

                    var k = l.PositionOfТhePointToLineSign(mid);

                    var oc = acPoly1.GetOffsetCurves(k < 0 ? dist : -dist);
                    foreach (Entity acEnt in oc)
                    {
                        acBlkTblRec.AppendEntity(acEnt);
                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                    }
                    acPoly11 = (Polyline) oc[0];
                }
                #endregion

                CommandLineHelper.Command("._FILLET", "_R", 0.0);
                CommandLineHelper.Command("._FILLET", "_P", acPoly11.ObjectId);

                var acVec3D = acPoly11.EndPoint.GetVectorTo(polyLineInsPoint);
                acPoly11.TransformBy(Matrix3d.Displacement(acVec3D));

                acPoly11.TransformBy(Ed.CurrentUserCoordinateSystem);

                acPoly1.Erase();
                acTrans.Commit();
            }
            Ed.Regen();
        }

        //private static void Draw(Entity obj)
        //{
        //    Editor Ed = Application.DocumentManager.MdiActiveDocument.Editor;
        //    Database Db = Application.DocumentManager.MdiActiveDocument.Database;
        //    using (Transaction Trans = Db.TransactionManager.StartTransaction())
        //    {
        //        BlockTable BlkTbl = Trans.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
        //        BlockTableRecord BlkTblRec = Trans.GetObject(BlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        //        BlkTblRec.AppendEntity(obj);
        //        Trans.AddNewlyCreatedDBObject(obj, true);
        //        Trans.Commit();
        //    }
        //}
        private static Complex ConvertVertexToComplex(Polyline pl, int i)
        {
            return new Complex(pl.GetPoint2dAt(i).X, pl.GetPoint2dAt(i).Y);
        }
        private static AlignedDimension AddDim(Complex c1, Complex c2, double k, string layer, System.Drawing.Color color, string dimstyle)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;

            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                var acBlkTbl = acTrans.GetObject(db.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                var acBlkTblRec =
                    (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var acLyrTbl = (LayerTable) acTrans.GetObject(db.LayerTableId, OpenMode.ForRead);


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
                    if (layer.Contains("Current") && layer.Contains("Layer")) { throw new ArgumentNullException(); }
                    acRotDim.Layer = layer;
                }
                catch
                {
                    var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                    acRotDim.Layer = acLyrTblRec.Name;

                }
                try
                {
                    // MessageBox.Show("Profile Dim color = " +color.Name);
                    if (color.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                    acRotDim.Color = Color.FromColor(color);
                }
                catch
                {
                    try
                    {
                        if (layer.Contains("Current") && layer.Contains("Layer")) { throw new ArgumentNullException(); }
                        var acLyrTblRec = (LayerTableRecord)acTrans.GetObject(acLyrTbl[layer], OpenMode.ForWrite);
                        acRotDim.Color = acLyrTblRec.Color;
                    }
                    catch
                    {
                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                        acRotDim.Color = acLyrTblRec.Color;
                    }
                }

                try
                {
                    // Open the DimStyle table for read
                    var acDimStyleTbl = (DimStyleTable) acTrans.GetObject(db.DimStyleTableId, OpenMode.ForRead);
                    var acDimStyleTblRec1 =
                        (DimStyleTableRecord) acTrans.GetObject(acDimStyleTbl[dimstyle], OpenMode.ForWrite);
                    acRotDim.SetDimstyleData(acDimStyleTblRec1);
                }
                catch
                {
                    acRotDim.DimensionStyle = db.Dimstyle;
                }


                // Add the new object to Model space and the transaction
                acBlkTblRec.AppendEntity(acRotDim);
                acTrans.AddNewlyCreatedDBObject(acRotDim, true);

                // Commit the changes and dispose of the transaction
                acTrans.Commit();

                return acRotDim;
            }

        }
        private static RotatedDimension AddRotDim(Complex c1, Complex c2, double ang, double k, string layer, System.Drawing.Color color, string dimstyle)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            RotatedDimension acRotDim;
            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                var acBlkTbl = (BlockTable) acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);

                // Open the Block table record Model space for write
                var acBlkTblRec =
                    (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var acLyrTbl = (LayerTable) acTrans.GetObject(db.LayerTableId, OpenMode.ForRead);


                if (ang < 0)
                {
                    ang += Math.PI;
                }

                var qq = Complex.polar(1.0, ang);
                qq *= Complex.polar(1.0, Math.PI / 2.0);
                qq *= k;
                qq = (c1 + c2) / 2.0 + qq;

                // Create the rotated dimension
                acRotDim = new RotatedDimension();
                acRotDim.SetDatabaseDefaults();
                acRotDim.XLine1Point = new Point3d(c1.real(), c1.imag(), 0);
                acRotDim.XLine2Point = new Point3d(c2.real(), c2.imag(), 0);
                acRotDim.Rotation = ang;
                acRotDim.DimLinePoint = new Point3d(qq.real(), qq.imag(), 0);
                //acRotDim.DimensionStyle = Db.Dimstyle;
                try
                {
                    if (layer.Contains("Current") && layer.Contains("Layer")) { throw new ArgumentNullException(); }
                    acRotDim.Layer = layer;
                }
                catch
                {
                    var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                    acRotDim.Layer = acLyrTblRec.Name;

                }
                try
                {
                    //MessageBox.Show("Profile Dim color = " + color.Name);
                    if (color.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                    acRotDim.Color = Color.FromColor(color);
                }
                catch
                {
                    try
                    {
                        if (layer.Contains("Current") && layer.Contains("Layer")) { throw new ArgumentNullException(); }
                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[layer], OpenMode.ForWrite);
                        acRotDim.Color = acLyrTblRec.Color;
                    }
                    catch
                    {
                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                        acRotDim.Color = acLyrTblRec.Color;
                    }

                }

                try
                {
                    // Open the DimStyle table for read
                    var acDimStyleTbl = (DimStyleTable) acTrans.GetObject(db.DimStyleTableId, OpenMode.ForRead);
                    var acDimStyleTblRec1 =
                        (DimStyleTableRecord) acTrans.GetObject(acDimStyleTbl[dimstyle], OpenMode.ForWrite);
                    acRotDim.SetDimstyleData(acDimStyleTblRec1);
                }
                catch
                {
                    acRotDim.DimensionStyle = db.Dimstyle;
                }


                // Add the new object to Model space and the transaction
                acBlkTblRec.AppendEntity(acRotDim);
                acTrans.AddNewlyCreatedDBObject(acRotDim, true);

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return acRotDim;
        }
        private static LineAngularDimension2 AddAngDim(Complex c1, Complex c2, Complex c3, Complex c4, double k, string layer, System.Drawing.Color color, string dimstyle)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;

            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                var acBlkTbl = (BlockTable) acTrans.GetObject(db.BlockTableId,
                    OpenMode.ForRead);

                // Open the Block table record Model space for write
                var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var acLyrTbl = (LayerTable) acTrans.GetObject(db.LayerTableId, OpenMode.ForRead);


                // Create the  dimension
                var qq = c2 + (c2 - c1) * k * 1.5 / (c2 - c1).abs();
                var qq1 = c4 + (c4 - c3) * k * 1.5 / (c4 - c3).abs();
                qq = (qq + qq1) / 2;

                if ((qq - (c4 + c2) / 2).abs() < 10.0)
                {
                    var nc = qq - (c4 + c2) / 2;
                    nc /= nc.abs();
                    qq += nc * 10.0;
                }

                var acLinAngDim = new LineAngularDimension2();
                acLinAngDim.SetDatabaseDefaults();
                acLinAngDim.XLine1Start = new Point3d(c1.real(), c1.imag(), 0);
                acLinAngDim.XLine1End = new Point3d(c2.real(), c2.imag(), 0);
                acLinAngDim.XLine2Start = new Point3d(c3.real(), c3.imag(), 0);
                acLinAngDim.XLine2End = new Point3d(c4.real(), c4.imag(), 0);
                acLinAngDim.ArcPoint = new Point3d(qq.real(), qq.imag(), 0);
                acLinAngDim.DimensionStyle = db.Dimstyle;
                try
                {
                    if (layer.Contains("Current") && layer.Contains("Layer")) { throw new ArgumentNullException(); }
                    acLinAngDim.Layer = layer;
                }
                catch
                {
                    var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                    acLinAngDim.Layer = acLyrTblRec.Name;

                }
                try
                {
                    //MessageBox.Show("Profile Dim color = " + color.Name);
                    if (color.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                    acLinAngDim.Color = Color.FromColor(color);
                }
                catch
                {
                    try
                    {
                        if (layer.Contains("Current") && layer.Contains("Layer")) { throw new ArgumentNullException(); }
                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[layer], OpenMode.ForWrite);
                        acLinAngDim.Color = acLyrTblRec.Color;
                    }
                    catch
                    {
                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                        acLinAngDim.Color = acLyrTblRec.Color;
                    }
                }
                try
                {
                    // Open the DimStyle table for read
                    var acDimStyleTbl = (DimStyleTable) acTrans.GetObject(db.DimStyleTableId,
                        OpenMode.ForRead);
                    var acDimStyleTblRec1 = (DimStyleTableRecord) acTrans.GetObject(acDimStyleTbl[dimstyle],
                        OpenMode.ForWrite);
                    acLinAngDim.SetDimstyleData(acDimStyleTblRec1);
                }
                catch
                {
                    acLinAngDim.DimensionStyle = db.Dimstyle;
                }

                // Add the new object to Model space and the transaction
                acBlkTblRec.AppendEntity(acLinAngDim);
                acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);


                acTrans.Commit();
                return acLinAngDim;
            }
        }
        private static void CreateDimensions(Polyline pl2, Polyline pl3, double k,
        bool drawDim, DimensionsStandart dimensionStandart, string layers, System.Drawing.Color color, string dimstyle)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            if (!drawDim)
            {
                return;
            }
            if (dimensionStandart == 0)
            {
                var b = false;
                for (var nCnt = 1; nCnt < pl2.NumberOfVertices; nCnt++)
                {
                    #region linear
                    if (pl2.GetBulgeAt(nCnt - 1) == 0)
                    {
                        var c1 = ConvertVertexToComplex(pl2, nCnt - 1);
                        var c2 = ConvertVertexToComplex(pl2, nCnt);
                        var c11 = ConvertVertexToComplex(pl3, nCnt - 1);
                        var c22 = ConvertVertexToComplex(pl3, nCnt);
                        var c3 = (ConvertVertexToComplex(pl3, nCnt - 1) + ConvertVertexToComplex(pl3, nCnt)) / 2.0;
                        var c33 = (ConvertVertexToComplex(pl2, nCnt - 1) + ConvertVertexToComplex(pl2, nCnt)) / 2.0;
                        if (nCnt == pl2.NumberOfVertices - 1)
                        {
                            if (b)
                            {
                                var line = new Line2d(c1, c2);
                                var back = k;
                                if (k < 10)
                                {
                                    k = 10;
                                }
                                else
                                {
                                    k *= Math.Abs((k + 10) / k);
                                }
                                _alignedDimensionsArray.Add(AddDim(c1, c2, k * line.PositionOfТhePointToLineSign(c3), layers == "" ? "0" : layers, color, dimstyle));
                                k = back;
                            }
                            else
                            {
                                var line = new Line2d(c11, c22);
                                var back = k;
                                if (k < 10)
                                {
                                    k = 10;
                                }
                                else
                                {
                                    k *= Math.Abs((k + 10) / k);
                                }
                                _alignedDimensionsArray.Add(AddDim(c11, c22, k * line.PositionOfТhePointToLineSign(c33), layers == "" ? "0" : layers, color, dimstyle));
                                k = back;
                            }
                        }
                        else
                        {
                            var c4 = (ConvertVertexToComplex(pl2, nCnt + 1) + ConvertVertexToComplex(pl2, nCnt)) / 2.0;
                            var line1 = new Line2d(c1, c2);
                            if (line1.PositionOfТhePointToLineSign(c3) == line1.PositionOfТhePointToLineSign(c4))
                            {
                                b = true;
                                var line = new Line2d(c1, c2);
                                var back = k;
                                if (k < 10)
                                {
                                    k = 10;
                                }
                                else
                                {
                                    k *= Math.Abs((k + 10) / k);
                                }
                                _alignedDimensionsArray.Add(AddDim(c1, c2, k * line.PositionOfТhePointToLineSign(c3), layers == "" ? "0" : layers, color, dimstyle));
                                k = back;
                            }
                            else
                            {
                                b = false;
                                var line = new Line2d(c11, c22);
                                var back = k;
                                if (k < 10)
                                {
                                    k = 10;
                                }
                                else
                                {
                                    k *= Math.Abs((k + 10) / k);
                                }
                                _alignedDimensionsArray.Add(AddDim(c11, c22, k * line.PositionOfТhePointToLineSign(c33), layers == "" ? "0" : layers, color, dimstyle));
                                k = back;
                            }
                        }
                    }
                    #endregion

                    #region angular
                    if (pl2.GetBulgeAt(nCnt - 1) != 0)
                    {
                        var c1 = ConvertVertexToComplex(pl2, nCnt - 1);
                        var c2 = ConvertVertexToComplex(pl3, nCnt - 1);
                        var c3 = ConvertVertexToComplex(pl2, nCnt);
                        var c4 = ConvertVertexToComplex(pl3, nCnt);

                        var back = k;
                        if (k < 10)
                        {
                            k = 10;
                        }
                        else
                        {
                            k *= Math.Abs((k + 10) / k);
                        }
                        _angularDimensionArray.Add((c1 - c3).abs() > (c2 - c4).abs()
                            ? AddAngDim(c2, c1, c4, c3, k, layers == "" ? "0" : layers, color, dimstyle)
                            : AddAngDim(c1, c2, c3, c4, k, layers == "" ? "0" : layers, color, dimstyle));
                        k = back;
                    }
                    #endregion
                }
            }
            else
            {
                if (pl2.NumberOfVertices > 2)//ako ne e edeni4na prava
                {
                    #region calc1
                    var vertexarr = new ArrayList();
                    var pt2Arr = new ArrayList();
                    var pt3Arr = new ArrayList();
                    using (var acTrans = db.TransactionManager.StartTransaction())
                    {
                        var pt2 = (Polyline)acTrans.GetObject(pl2.ObjectId, OpenMode.ForWrite);
                        var pt3 = (Polyline)acTrans.GetObject(pl3.ObjectId, OpenMode.ForWrite);
                        CommandLineHelper.Command("._FILLET", "_R", 0);
                        CommandLineHelper.Command("._FILLET", "_P", pt2.ObjectId);
                        CommandLineHelper.Command("._FILLET", "_P", pt3.ObjectId);
                        for (var i = 1; i < pt2.NumberOfVertices - 1; i++)
                        {
                            pt2Arr.Add(ConvertVertexToComplex(pt2, i));
                            pt3Arr.Add(ConvertVertexToComplex(pt3, i));
                        }
                    }
                    var curr = -1;
                    for (var nCnt = 1; nCnt < pl2.NumberOfVertices; nCnt++)
                    {
                        #region dimens
                        if (pl2.GetBulgeAt(nCnt - 1) != 0)
                        {
                            #region calc
                            curr++;
                            var c1 = ConvertVertexToComplex(pl2, nCnt - 1);
                            var c2 = ConvertVertexToComplex(pl3, nCnt - 1);
                            var c3 = ConvertVertexToComplex(pl2, nCnt);
                            var c4 = ConvertVertexToComplex(pl3, nCnt);

                            if ((c1 - c3).abs() > (c2 - c4).abs())
                            {
                                vertexarr.Add((Complex)pt2Arr[curr]);
                            }
                            else
                            {
                                vertexarr.Add((Complex)pt3Arr[curr]);
                            }
                            #endregion
                        }
                        #endregion
                    }
                    var sc1 = ConvertVertexToComplex(pl2, 0);
                    var sc2 = ConvertVertexToComplex(pl3, 0);
                    vertexarr.Insert(0,
                        (sc1 - (Complex) vertexarr[0]).abs() < (sc2 - (Complex) vertexarr[0]).abs()
                            ? ConvertVertexToComplex(pl2, 0)
                            : ConvertVertexToComplex(pl3, 0));

                    sc1 = ConvertVertexToComplex(pl2, pl2.NumberOfVertices - 1);
                    sc2 = ConvertVertexToComplex(pl3, pl3.NumberOfVertices - 1);

                    vertexarr.Add((sc1 - (Complex) vertexarr[vertexarr.Count - 1]).abs() <
                                  (sc2 - (Complex) vertexarr[vertexarr.Count - 1]).abs()
                        ? ConvertVertexToComplex(pl2, pl2.NumberOfVertices - 1)
                        : ConvertVertexToComplex(pl3, pl3.NumberOfVertices - 1));

                    #endregion

                    for (var nCnt = 1; nCnt < vertexarr.Count; nCnt++)
                    {

                        var ang = (double)_directions[nCnt - 1];
                        var line = new Line2d((Complex)vertexarr[nCnt - 1], (Complex)vertexarr[nCnt]);
                        double koef = 1;
                        if (vertexarr.Count > 2)
                        {
                            Complex mid;
                            if (nCnt < vertexarr.Count - 1)
                            {
                                mid = ((Complex)vertexarr[nCnt + 1] + (Complex)vertexarr[nCnt]) / 2.0;
                            }
                            else
                            {
                                mid = ((Complex)vertexarr[nCnt - 1] + (Complex)vertexarr[nCnt - 2]) / 2.0;
                            }
                            koef = line.PositionOfТhePointToLineSign(mid);
                        }


                        var back = k;
                        if (k < 10)
                        {
                            k = 10;
                        }
                        else
                        {
                            k *= Math.Abs((k + 10) / k);
                        }

                        var ds = AddRotDim((Complex)vertexarr[nCnt - 1], (Complex)vertexarr[nCnt], ang, k * koef, layers == "" ? "0" : layers, color, dimstyle);
                        _rotatedDimensionArray.Add(ds);
                        k = back;
                    }
                }
                else//orazmerqvane kakto pri DimensionStandart == 0
                {
                    var b = false;
                    for (var nCnt = 1; nCnt < pl2.NumberOfVertices; nCnt++)
                    {
                        #region linear
                        if (pl2.GetBulgeAt(nCnt - 1) == 0)
                        {
                            var c1 = ConvertVertexToComplex(pl2, nCnt - 1);
                            var c2 = ConvertVertexToComplex(pl2, nCnt);
                            var c11 = ConvertVertexToComplex(pl3, nCnt - 1);
                            var c22 = ConvertVertexToComplex(pl3, nCnt);
                            var c3 = (ConvertVertexToComplex(pl3, nCnt - 1) + ConvertVertexToComplex(pl3, nCnt)) / 2.0;
                            var c33 = (ConvertVertexToComplex(pl2, nCnt - 1) + ConvertVertexToComplex(pl2, nCnt)) / 2.0;
                            if (nCnt == pl2.NumberOfVertices - 1)
                            {
                                if (b)
                                {
                                    var line = new Line2d(c1, c2);
                                    var back = k;
                                    if (k < 10)
                                    {
                                        k = 10;
                                    }
                                    else
                                    {
                                        k *= Math.Abs((k + 10) / k);
                                    }
                                    _alignedDimensionsArray.Add(AddDim(c1, c2, k * line.PositionOfТhePointToLineSign(c3), layers == "" ? "0" : layers, color, dimstyle));
                                    k = back;
                                }
                                else
                                {
                                    var line = new Line2d(c11, c22);
                                    var back = k;
                                    if (k < 10)
                                    {
                                        k = 10;
                                    }
                                    else
                                    {
                                        k *= Math.Abs((k + 10) / k);
                                    }
                                    _alignedDimensionsArray.Add(AddDim(c11, c22, k * line.PositionOfТhePointToLineSign(c33), layers == "" ? "0" : layers, color, dimstyle));
                                    k = back;
                                }
                            }
                            else
                            {
                                var c4 = (ConvertVertexToComplex(pl2, nCnt + 1) + ConvertVertexToComplex(pl2, nCnt)) / 2.0;
                                var line1 = new Line2d(c1, c2);
                                if (line1.PositionOfТhePointToLineSign(c3) == line1.PositionOfТhePointToLineSign(c4))
                                {
                                    b = true;
                                    var line = new Line2d(c1, c2);
                                    var back = k;
                                    if (k < 10)
                                    {
                                        k = 10;
                                    }
                                    else
                                    {
                                        k *= Math.Abs((k + 10) / k);
                                    }
                                    _alignedDimensionsArray.Add(AddDim(c1, c2, k * line.PositionOfТhePointToLineSign(c3), layers == "" ? "0" : layers, color, dimstyle));
                                    k = back;
                                }
                                else
                                {
                                    b = false;
                                    var line = new Line2d(c11, c22);
                                    var back = k;
                                    if (k < 10)
                                    {
                                        k = 10;
                                    }
                                    else
                                    {
                                        k *= Math.Abs((k + 10) / k);
                                    }
                                    _alignedDimensionsArray.Add(AddDim(c11, c22, k * line.PositionOfТhePointToLineSign(c33), layers == "" ? "0" : layers, color, dimstyle));
                                    k = back;
                                }
                            }
                        }
                        #endregion

                        #region angular
                        if (pl2.GetBulgeAt(nCnt - 1) != 0)
                        {
                            var c1 = ConvertVertexToComplex(pl2, nCnt - 1);
                            var c2 = ConvertVertexToComplex(pl3, nCnt - 1);
                            var c3 = ConvertVertexToComplex(pl2, nCnt);
                            var c4 = ConvertVertexToComplex(pl3, nCnt);

                            var back = k;
                            if (k < 10)
                            {
                                k = 10;
                            }
                            else
                            {
                                k *= Math.Abs((k + 10) / k);
                            }
                            _angularDimensionArray.Add((c1 - c3).abs() > (c2 - c4).abs()
                                ? AddAngDim(c2, c1, c4, c3, k, layers == "" ? "0" : layers, color, dimstyle)
                                : AddAngDim(c1, c2, c3, c4, k, layers == "" ? "0" : layers, color, dimstyle));
                            k = back;
                        }
                        #endregion
                    }
                }
            }
        }

        private static void PrepareRedraw()
        {
            try
            {
                if (_counter <= 0)
                {
                    return;
                }
                var db = Application.DocumentManager.MdiActiveDocument.Database;

                using (var acTrans = db.TransactionManager.StartTransaction())
                {
                    var acBlkTbl = (BlockTable) acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);

                    // Open the Block table record Model space for write
                    var acBlkTblRec =
                        (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    var pll = (Polyline)acTrans.GetObject(_firstPoly.ObjectId, OpenMode.ForWrite);
                    pll.Erase();
                    var pl = (Polyline)acTrans.GetObject(PolylineId1, OpenMode.ForWrite);

                    _basePolyline = (Polyline) pl.Clone();
                    acBlkTblRec.AppendEntity(_basePolyline);
                    acTrans.AddNewlyCreatedDBObject(_basePolyline, true);
                    //BasePolyline.Visible = true;

                    if (_coatingLinesArray.Count > 0)
                    {
                        foreach (Polyline p in _coatingLinesArray)
                        {
                            var ple = (Polyline)acTrans.GetObject(p.ObjectId, OpenMode.ForWrite);
                            ple.Erase();
                        }
                        _coatingLinesArray.RemoveRange(0, _coatingLinesArray.Count - 1);
                    }

                    if (_alignedDimensionsArray.Count > 0)
                    {
                        foreach (AlignedDimension p in _alignedDimensionsArray)
                        {
                            var ple = (AlignedDimension)acTrans.GetObject(p.ObjectId, OpenMode.ForWrite);
                            ple.Erase();
                        }
                        _alignedDimensionsArray.RemoveRange(0, _alignedDimensionsArray.Count - 1);
                    }

                    if (_angularDimensionArray.Count > 0)
                    {
                        foreach (LineAngularDimension2 p in _angularDimensionArray)
                        {
                            var ple = (LineAngularDimension2)acTrans.GetObject(p.ObjectId, OpenMode.ForWrite);
                            ple.Erase();
                        }
                        _angularDimensionArray.RemoveRange(0, _angularDimensionArray.Count - 1);
                    }

                    if (_rotatedDimensionArray.Count > 0)
                    {
                        foreach (RotatedDimension p in _rotatedDimensionArray)
                        {
                            var ple = (RotatedDimension)acTrans.GetObject(p.ObjectId, OpenMode.ForWrite);
                            ple.Erase();
                        }
                        _rotatedDimensionArray.RemoveRange(0, _rotatedDimensionArray.Count - 1);
                    }



                    acTrans.Commit();
                }

                PolylineId = _basePolyline.ObjectId;
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Polyline To Profile
        /// </summary>
        [CommandMethod("shp")]
        public void PolyToProfileStart()
        {
            
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            _directions = new ArrayList();

            // Pick a polyline
            var polyOptions = new PromptEntityOptions("Pick a NOT CLOSED polyline : ");
            polyOptions.SetRejectMessage("\nEntity is not a polyline.");
            polyOptions.AddAllowedClass(typeof(Polyline), true);
            var polyResult = ed.GetEntity(polyOptions);
            if (polyResult.Status != PromptStatus.OK)
            {
                return;
            }

            PolylineId1 = polyResult.ObjectId;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable) tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                // Open the Block table record Model space for write
                var btrModel = (BlockTableRecord) tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var pline = (Polyline)tr.GetObject(PolylineId1, OpenMode.ForWrite);
                for (var i = 1; i < pline.NumberOfVertices; i++)
                {
                    var ang = (ConvertVertexToComplex(pline, i) - ConvertVertexToComplex(pline, i - 1)).arg();
                    _directions.Add(ang);
                }

                _basePolyline = (Polyline) pline.Clone();
                btrModel.AppendEntity(_basePolyline);
                tr.AddNewlyCreatedDBObject(_basePolyline, true);
                PolylineId = _basePolyline.ObjectId;
                tr.Commit();
            }


            _sheetProfileForm = new SheetProfileGenerationForm(PolylineId);
            _sheetProfileForm.PrepareRedraw += PrepareRedraw;
            _counter = 0;
            _sheetProfileForm.ShowDialog();
            Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Profile To polyline
        /// </summary>
        [CommandMethod("psh")]
        public void ProfileToPolyStart()
        {
            
            _basePolyLine = new Polyline();
            _num1 = _num2 = -1;
            PolylineId = new ObjectId();

            // Pick a polyline
            var polyOptions = new PromptEntityOptions("Pick a sheet profile :");
            polyOptions.SetRejectMessage("\nThis is not a generated sheet profile.\nSheet Profile is generated from a polyline with SHP command.\nCreate a sheet profile and try again.");
            polyOptions.AddAllowedClass(typeof(Polyline), true);
            var polyResult = Ed.GetEntity(polyOptions);
            if (polyResult.Status != PromptStatus.OK)
            {
                return;
            }

            var pickInsertionPoint = Ed.GetPoint(new PromptPointOptions("Pick insertion point : "));
            if (pickInsertionPoint.Status != PromptStatus.OK)
            {
                return;
            }

            PolylineId = polyResult.ObjectId;
            double thickness;

            using (var tr = Db.TransactionManager.StartTransaction())
            {
                _basePolyLine = (Polyline)tr.GetObject(PolylineId, OpenMode.ForWrite);
                if ((Math.Abs(_basePolyLine.StartPoint.TransformBy(Ed.CurrentUserCoordinateSystem.Inverse()).Z) > 0.000001) ||
                                 (Math.Abs(_basePolyLine.EndPoint.TransformBy(Ed.CurrentUserCoordinateSystem.Inverse()).Z) > 0.000001))
                {
                    MessageBox.Show("Polyline must lie in the XY Plane of the UCS !", "ERROR:");
                    return;
                }
                if ((CountOfArcs() + 2 != _basePolyLine.NumberOfVertices >> 1) || (CountOfArcs() % 2 > 0))
                {
                    MessageBox.Show("Must have an even number of ARC segments !", "ERROR:");
                    return;
                }
                thickness = GetThickness();
                if (thickness <= 0)
                {
                    MessageBox.Show("PolyLine has only one segment or no ARC segment !", "ERROR:");
                    return;
                }
                if (_basePolyLine.Closed == false)
                {
                    MessageBox.Show("The PolyLline is not closed !", "ERROR:");
                    return;
                }
                if (_basePolyLine.NumberOfVertices % 2 != 0)
                {
                    MessageBox.Show("Must have an even number of segments !", "ERROR:");
                    return;
                }

                #region endmost_thickness_segment_search
                _num1 = -1;
                _num2 = -1;
                for (var i = 0; i < _basePolyLine.NumberOfVertices; i++)
                {
                    var next = i < _basePolyLine.NumberOfVertices - 1 ? i + 1 : 0;
                    var pre = i > 0 ? i - 1 : _basePolyLine.NumberOfVertices - 1;

                    var seg = _basePolyLine.GetLineSegment2dAt(i);
                    if (!(seg.Length - thickness < 0.000001))
                    {
                        continue;
                    }
                    var segPre = _basePolyLine.GetLineSegment2dAt(pre);
                    var segNext = _basePolyLine.GetLineSegment2dAt(next);

                    for (var j = next; j < _basePolyLine.NumberOfVertices; j++)
                    {
                        var seg1 = _basePolyLine.GetLineSegment2dAt(j);
                        if (!(seg1.Length - thickness < 0.000001))
                        {
                            continue;
                        }
                        if (!(Math.Abs(seg.Length - seg1.Length) < 0.000001))
                        {
                            continue;
                        }
                        var cPre = new Complex(segPre.EndPoint.X, segPre.EndPoint.Y) - new Complex(segPre.StartPoint.X, segPre.StartPoint.Y);
                        var cNext = new Complex(segNext.EndPoint.X, segNext.EndPoint.Y) - new Complex(segNext.StartPoint.X, segNext.StartPoint.Y);
                        var ang = (cNext / cPre).arg();
                        if (!(Math.Abs(ang*180/Math.PI - 180) < 0.001))
                        {
                            continue;
                        }
                        if (_num1 == -1)
                        {
                            _num1 = i;
                            _num2 = j;
                        }
                        else
                        {
                            if (Math.Abs(j - i) != _basePolyLine.NumberOfVertices/2)
                            {
                                continue;
                            }
                            _num1 = i;
                            _num2 = j;
                        }
                    }
                }

                Ed.Regen();

                #endregion

                tr.Commit();
            }

            DrawPolylineFromProfile(_num1, _num2, thickness, pickInsertionPoint.Value);
            Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public static void ClearEvent()
        {
            _sheetProfileForm.PrepareRedraw -= PrepareRedraw;
            var db = Application.DocumentManager.MdiActiveDocument.Database;

            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                var pl = (Polyline)acTrans.GetObject(PolylineId1, OpenMode.ForWrite);
                pl.Erase();
                acTrans.Commit();
            }
        }
        public static void ClearEventA()
        {
            try
            {
                _sheetProfileForm.PrepareRedraw -= PrepareRedraw;
                var db = Application.DocumentManager.MdiActiveDocument.Database;

                using (var acTrans = db.TransactionManager.StartTransaction())
                {
                    var pl = (Polyline)acTrans.GetObject(_firstPoly.ObjectId, OpenMode.ForWrite);
                    pl.Erase();

                    if (_coatingLinesArray.Count > 0)
                    {
                        foreach (Polyline p in _coatingLinesArray)
                        {
                            var ple = (Polyline)acTrans.GetObject(p.ObjectId, OpenMode.ForWrite);
                            ple.Erase();
                        }
                        _coatingLinesArray.RemoveRange(0, _coatingLinesArray.Count - 1);
                    }

                    if (_alignedDimensionsArray.Count > 0)
                    {
                        foreach (AlignedDimension p in _alignedDimensionsArray)
                        {
                            var ple = (AlignedDimension)acTrans.GetObject(p.ObjectId, OpenMode.ForWrite);
                            ple.Erase();
                        }
                        _alignedDimensionsArray.RemoveRange(0, _alignedDimensionsArray.Count - 1);
                    }

                    if (_angularDimensionArray.Count > 0)
                    {
                        foreach (LineAngularDimension2 p in _angularDimensionArray)
                        {
                            var ple = (LineAngularDimension2)acTrans.GetObject(p.ObjectId, OpenMode.ForWrite);
                            ple.Erase();
                        }
                        _angularDimensionArray.RemoveRange(0, _angularDimensionArray.Count - 1);
                    }

                    if (_rotatedDimensionArray.Count > 0)
                    {
                        foreach (RotatedDimension p in _rotatedDimensionArray)
                        {
                            var ple = (RotatedDimension)acTrans.GetObject(p.ObjectId, OpenMode.ForWrite);
                            ple.Erase();
                        }
                        _rotatedDimensionArray.RemoveRange(0, _rotatedDimensionArray.Count - 1);
                    }

                    acTrans.Commit();
                }
            }
            catch
            {
            }
        }
        public static void DrawProfile(SheetProfileConfig aSpConfig)
        {
            try
            {
                #region  inner
                var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                var db = Application.DocumentManager.MdiActiveDocument.Database;

                _coatingLinesArray = new ArrayList();      //create
                _alignedDimensionsArray = new ArrayList(); //create
                _angularDimensionArray = new ArrayList();  //create
                _rotatedDimensionArray = new ArrayList();  //create

                if (aSpConfig.InternalRadius < 0.01) { aSpConfig.InternalRadius = 0.0001; }
                var midLineRadius = aSpConfig.SheetThickness / 2 + aSpConfig.InternalRadius;//radiusa do srednata linia

                using (var acTrans = db.TransactionManager.StartTransaction())
                {
                    var acBlkTbl = (BlockTable) acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
                    // Open the Block table record Model space for write
                    var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    var acLyrTbl = (LayerTable) acTrans.GetObject(db.LayerTableId, OpenMode.ForRead);

                    _firstPoly = (Polyline)acTrans.GetObject(PolylineId, OpenMode.ForWrite);
                    DBObjectCollection acDbObjColl;
                    Polyline secondPoly;
                    if (aSpConfig.OffsetSide == OffsetSide.Center)
                    {
                        //midline exist
                        CommandLineHelper.Command("._FILLET", "_R", Math.Abs(midLineRadius));
                        CommandLineHelper.Command("._FILLET", "_P", _firstPoly.ObjectId);

                        if (aSpConfig.DrawCoating)
                        {
                            //MessageBox.Show("Profile Coating color = " + aSPConfig.CoatingColor.Name);
                            #region Coating
                            switch (aSpConfig.CoatingSide)
                            {
                                case CoatingSide.Both:
                                {

                                    acDbObjColl = _firstPoly.GetOffsetCurves(1.5 * Math.Abs(aSpConfig.SheetThickness));
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        acBlkTblRec.AppendEntity(acEnt);
                                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                                    }
                                    _coatingLinesArray.Add((Polyline) acDbObjColl[0]);
                                    try
                                    {
                                        if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Layer = aSpConfig.CoatingLayer;
                                    }
                                    catch
                                    {
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[0]).Layer = acLyrTblRec.Name;
                                    }
                                    try
                                    {
                                        if (aSpConfig.CoatingColor.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Color = Color.FromColor(aSpConfig.CoatingColor);
                                    }
                                    catch
                                    {
                                        // LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                                        // ((Polyline)CoatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        try
                                        {
                                            if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[aSpConfig.CoatingLayer], OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }
                                        catch
                                        {
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }
                                    }
                                    var br = 0;
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        if (br > 0)
                                        {
                                            acEnt.Erase();
                                        }
                                        br++;
                                    }

                                    acDbObjColl = _firstPoly.GetOffsetCurves(-1.5 * Math.Abs(aSpConfig.SheetThickness));
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        acBlkTblRec.AppendEntity(acEnt);
                                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                                    }
                                    _coatingLinesArray.Add((Polyline) acDbObjColl[0]);
                                    try
                                    {
                                        if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[1]).Layer = aSpConfig.CoatingLayer;
                                    }
                                    catch
                                    {
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[1]).Layer = acLyrTblRec.Name;
                                    }
                                    try
                                    {
                                        if (aSpConfig.CoatingColor.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[1]).Color = Color.FromColor(aSpConfig.CoatingColor);
                                    }
                                    catch
                                    {
                                        //LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                                        //((Polyline)CoatingLinesArray[1]).Color = acLyrTblRec.Color;
                                        try
                                        {
                                            if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[aSpConfig.CoatingLayer], OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[1]).Color = acLyrTblRec.Color;
                                        }
                                        catch
                                        {
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[1]).Color = acLyrTblRec.Color;
                                        }

                                    }
                                    br = 0;
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        if (br > 0)
                                        {
                                            acEnt.Erase();
                                        }
                                        br++;
                                    }
                                }
                                    break;
                                case CoatingSide.Left:
                                {
                                    acDbObjColl = _firstPoly.GetOffsetCurves(-1.5 * Math.Abs(aSpConfig.SheetThickness));
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        acBlkTblRec.AppendEntity(acEnt);
                                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                                    }
                                    _coatingLinesArray.Add((Polyline) acDbObjColl[0]);
                                    try
                                    {
                                        if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Layer = aSpConfig.CoatingLayer;
                                    }
                                    catch
                                    {
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[0]).Layer = acLyrTblRec.Name;
                                    }
                                    try
                                    {
                                        if (aSpConfig.CoatingColor.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Color = Color.FromColor(aSpConfig.CoatingColor);
                                    }
                                    catch
                                    {
                                        // LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                                        // ((Polyline)CoatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        try
                                        {
                                            if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[aSpConfig.CoatingLayer], OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }
                                        catch
                                        {
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                            if (acLyrTblRec != null)
                                                ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }
                                    }
                                    var br = 0;
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        if (br > 0)
                                        {
                                            acEnt.Erase();
                                        }
                                        br++;
                                    }
                                }
                                    break;
                                default:
                                {
                                    acDbObjColl = _firstPoly.GetOffsetCurves(1.5 * Math.Abs(aSpConfig.SheetThickness));
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        acBlkTblRec.AppendEntity(acEnt);
                                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                                    }
                                    _coatingLinesArray.Add((Polyline) acDbObjColl[0]);
                                    try
                                    {
                                        if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Layer = aSpConfig.CoatingLayer;
                                    }
                                    catch
                                    {
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[0]).Layer = acLyrTblRec.Name;
                                    }
                                    try
                                    {
                                        if (aSpConfig.CoatingColor.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Color = Color.FromColor(aSpConfig.CoatingColor);
                                    }
                                    catch
                                    {
                                        // LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                                        // ((Polyline)CoatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        try
                                        {
                                            if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[aSpConfig.CoatingLayer], OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }
                                        catch
                                        {
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }

                                    }
                                    var br = 0;
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        if (br > 0)
                                        {
                                            acEnt.Erase();
                                        }
                                        br++;
                                    }
                                }
                                    break;
                            }
                            #endregion
                        }

                        #region profile_lines
                        acDbObjColl = _firstPoly.GetOffsetCurves(Math.Abs(aSpConfig.SheetThickness / 2.0));
                        foreach (Entity acEnt in acDbObjColl)
                        {
                            acBlkTblRec.AppendEntity(acEnt);
                            acTrans.AddNewlyCreatedDBObject(acEnt, true);
                        }
                        _firstPoly.Erase();
                        _firstPoly = acDbObjColl[0] as Polyline;
                        {
                            var br = 0;
                            foreach (Entity acEnt in acDbObjColl)
                            {
                                if (br > 0)
                                {
                                    acEnt.Erase();
                                }
                                br++;
                            }
                        }

                        acDbObjColl = _firstPoly.GetOffsetCurves(-Math.Abs(aSpConfig.SheetThickness));
                        foreach (Entity acEnt in acDbObjColl)
                        {
                            acBlkTblRec.AppendEntity(acEnt);
                            acTrans.AddNewlyCreatedDBObject(acEnt, true);
                        }
                        secondPoly = (Polyline) acDbObjColl[0];
                        {
                            var br = 0;
                            foreach (Entity acEnt in acDbObjColl)
                            {
                                if (br > 0)
                                {
                                    acEnt.Erase();
                                }
                                br++;
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        var k = aSpConfig.OffsetSide == OffsetSide.Right ? 1 : -1;

                        #region create mid line
                        acDbObjColl = _firstPoly.GetOffsetCurves(k * Math.Abs(aSpConfig.SheetThickness / 2.0));
                        foreach (Entity acEnt in acDbObjColl)
                        {
                            acBlkTblRec.AppendEntity(acEnt);
                            acTrans.AddNewlyCreatedDBObject(acEnt, true);
                        }
                        _firstPoly.Erase();
                        _firstPoly = acDbObjColl[0] as Polyline;
                        {
                            var br = 0;
                            foreach (Entity acEnt in acDbObjColl)
                            {
                                if (br > 0)
                                {
                                    acEnt.Erase();
                                }
                                br++;
                            }
                        }
                        #endregion

                        CommandLineHelper.Command("._FILLET", "_R", Math.Abs(midLineRadius));
                        CommandLineHelper.Command("._FILLET", "_P", _firstPoly.ObjectId);

                        if (aSpConfig.DrawCoating)
                        {
                            #region Coating
                            if (aSpConfig.CoatingSide == CoatingSide.Both)
                            {

                                acDbObjColl = _firstPoly.GetOffsetCurves(1.5 * Math.Abs(aSpConfig.SheetThickness));
                                foreach (Entity acEnt in acDbObjColl)
                                {
                                    acBlkTblRec.AppendEntity(acEnt);
                                    acTrans.AddNewlyCreatedDBObject(acEnt, true);
                                }
                                _coatingLinesArray.Add((Polyline) acDbObjColl[0]);
                                try
                                {
                                    if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                    ((Polyline)_coatingLinesArray[0]).Layer = aSpConfig.CoatingLayer;
                                }
                                catch
                                {
                                    var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                    ((Polyline)_coatingLinesArray[0]).Layer = acLyrTblRec.Name;
                                }
                                try
                                {
                                    if (aSpConfig.CoatingColor.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                                    ((Polyline)_coatingLinesArray[0]).Color = Color.FromColor(aSpConfig.CoatingColor);
                                }
                                catch
                                {
                                    //LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                                    //((Polyline)CoatingLinesArray[0]).Color = acLyrTblRec.Color;
                                    try
                                    {
                                        if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[aSpConfig.CoatingLayer], OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                    }
                                    catch
                                    {
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                    }

                                }
                                var br = 0;
                                foreach (Entity acEnt in acDbObjColl)
                                {
                                    if (br > 0)
                                    {
                                        acEnt.Erase();
                                    }
                                    br++;
                                }

                                acDbObjColl = _firstPoly.GetOffsetCurves(-1.5 * Math.Abs(aSpConfig.SheetThickness));
                                foreach (Entity acEnt in acDbObjColl)
                                {
                                    acBlkTblRec.AppendEntity(acEnt);
                                    acTrans.AddNewlyCreatedDBObject(acEnt, true);
                                }
                                _coatingLinesArray.Add((Polyline) acDbObjColl[0]);
                                try
                                {
                                    if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                    ((Polyline)_coatingLinesArray[1]).Layer = aSpConfig.CoatingLayer;
                                }
                                catch
                                {
                                    var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                    ((Polyline)_coatingLinesArray[1]).Layer = acLyrTblRec.Name;
                                }
                                try
                                {
                                    if (aSpConfig.CoatingColor.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                                    ((Polyline)_coatingLinesArray[1]).Color = Color.FromColor(aSpConfig.CoatingColor);
                                }
                                catch
                                {
                                    //LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                                    //((Polyline)CoatingLinesArray[1]).Color = acLyrTblRec.Color;
                                    try
                                    {
                                        if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[aSpConfig.CoatingLayer], OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[1]).Color = acLyrTblRec.Color;
                                    }
                                    catch
                                    {
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[1]).Color = acLyrTblRec.Color;
                                    }

                                }
                                br = 0;
                                foreach (Entity acEnt in acDbObjColl)
                                {
                                    if (br > 0)
                                    {
                                        acEnt.Erase();
                                    }
                                    br++;
                                }
                            }
                            else
                            {
                                if (aSpConfig.CoatingSide == CoatingSide.Left)
                                {
                                    acDbObjColl = _firstPoly.GetOffsetCurves(-1.5 * Math.Abs(aSpConfig.SheetThickness));
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        acBlkTblRec.AppendEntity(acEnt);
                                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                                    }
                                    _coatingLinesArray.Add((Polyline) acDbObjColl[0]);
                                    try
                                    {
                                        if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Layer = aSpConfig.CoatingLayer;
                                    }
                                    catch
                                    {
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[0]).Layer = acLyrTblRec.Name;
                                    }
                                    try
                                    {
                                        if (aSpConfig.CoatingColor.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Color = Color.FromColor(aSpConfig.CoatingColor);
                                    }
                                    catch
                                    {
                                        //LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                                        //((Polyline)CoatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        try
                                        {
                                            if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[aSpConfig.CoatingLayer], OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }
                                        catch
                                        {
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }

                                    }
                                    var br = 0;
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        if (br > 0)
                                        {
                                            acEnt.Erase();
                                        }
                                        br++;
                                    }
                                }
                                else
                                {
                                    acDbObjColl = _firstPoly.GetOffsetCurves(1.5 * Math.Abs(aSpConfig.SheetThickness));
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        acBlkTblRec.AppendEntity(acEnt);
                                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                                    }
                                    _coatingLinesArray.Add((Polyline) acDbObjColl[0]);
                                    try
                                    {
                                        if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Layer = aSpConfig.CoatingLayer;
                                    }
                                    catch
                                    {
                                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                        ((Polyline)_coatingLinesArray[0]).Layer = acLyrTblRec.Name;
                                    }
                                    try
                                    {
                                        if (aSpConfig.CoatingColor.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                                        ((Polyline)_coatingLinesArray[0]).Color = Color.FromColor(aSpConfig.CoatingColor);
                                    }
                                    catch
                                    {
                                        //LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                                        //((Polyline)CoatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        try
                                        {
                                            if (aSpConfig.CoatingLayer.Contains("Current") && aSpConfig.CoatingLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[aSpConfig.CoatingLayer], OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }
                                        catch
                                        {
                                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                                            ((Polyline)_coatingLinesArray[0]).Color = acLyrTblRec.Color;
                                        }

                                    }
                                    var br = 0;
                                    foreach (Entity acEnt in acDbObjColl)
                                    {
                                        if (br > 0)
                                        {
                                            acEnt.Erase();
                                        }
                                        br++;
                                    }
                                }
                            }
                            #endregion
                        }


                        #region profile_lines
                        acDbObjColl = _firstPoly.GetOffsetCurves(-k * Math.Abs(aSpConfig.SheetThickness / 2.0));
                        foreach (Entity acEnt in acDbObjColl)
                        {
                            acBlkTblRec.AppendEntity(acEnt);
                            acTrans.AddNewlyCreatedDBObject(acEnt, true);
                        }
                        _firstPoly.Erase();
                        _firstPoly = acDbObjColl[0] as Polyline;
                        {
                            var br = 0;
                            foreach (Entity acEnt in acDbObjColl)
                            {
                                if (br > 0)
                                {
                                    acEnt.Erase();
                                }
                                br++;
                            }
                        }

                        acDbObjColl = _firstPoly.GetOffsetCurves(k * Math.Abs(aSpConfig.SheetThickness));
                        foreach (Entity acEnt in acDbObjColl)
                        {
                            acBlkTblRec.AppendEntity(acEnt);
                            acTrans.AddNewlyCreatedDBObject(acEnt, true);
                        }
                        secondPoly = (Polyline) acDbObjColl[0];
                        {
                            var br = 0;
                            foreach (Entity acEnt in acDbObjColl)
                            {
                                if (br > 0)
                                {
                                    acEnt.Erase();
                                }
                                br++;
                            }
                        }
                        #endregion
                    }

                    #region create_endmost_lines
                    var accLine = new Line(_firstPoly.StartPoint, secondPoly.StartPoint);
                    accLine.SetDatabaseDefaults();
                    acBlkTblRec.AppendEntity(accLine);
                    acTrans.AddNewlyCreatedDBObject(accLine, true);

                    // Line acccLine = new Line(new Point3d(FirstPoly.EndPoint.X , FirstPoly.EndPoint.Y , 0) , new Point3d(SecondPoly.EndPoint.X , SecondPoly.EndPoint.Y , 0));
                    var acccLine = new Line(_firstPoly.EndPoint, secondPoly.EndPoint);
                    acccLine.SetDatabaseDefaults();
                    acBlkTblRec.AppendEntity(acccLine);
                    acTrans.AddNewlyCreatedDBObject(acccLine, true);
                    #endregion

                    CreateDimensions(_firstPoly,
                                      secondPoly,
                                      aSpConfig.SheetThickness,
                                      aSpConfig.DrawDimensions,
                                      aSpConfig.DimensionStandart,
                                      aSpConfig.DimensionsLayer,
                                      aSpConfig.DimensionsColor,
                                      aSpConfig.DimensionStyle
                                      );

                    //Join to FirstPoly
                    if (aSpConfig.InternalRadius >= 0.001)
                    {
                        CommandLineHelper.Command("._PEDIT", _firstPoly.ObjectId, "_J", acccLine.ObjectId, secondPoly.ObjectId, "\033", "\033");
                        CommandLineHelper.Command("._PEDIT", _firstPoly.ObjectId, "_J", accLine.ObjectId, "\033", "\033");
                    }
                    else
                    {
                        for (var nCnt = 0; nCnt < _firstPoly.NumberOfVertices - 1; nCnt++)
                        {
                            try
                            {
                                var radius = _firstPoly.GetArcSegmentAt(nCnt).Radius;
                                if (radius < 0.011)
                                {
                                    var c1 = new Complex(_firstPoly.GetLineSegment2dAt(nCnt).StartPoint.X, _firstPoly.GetLineSegment2dAt(nCnt).StartPoint.Y);
                                    var c2 = new Complex(_firstPoly.GetLineSegment2dAt(nCnt - 1).MidPoint.X, _firstPoly.GetLineSegment2dAt(nCnt - 1).MidPoint.Y);
                                    var l1 = new Line2d(c1, c2);
                                    var c3 = new Complex(_firstPoly.GetLineSegment2dAt(nCnt).EndPoint.X, _firstPoly.GetLineSegment2dAt(nCnt).EndPoint.Y);
                                    var c4 = new Complex(_firstPoly.GetLineSegment2dAt(nCnt + 1).MidPoint.X, _firstPoly.GetLineSegment2dAt(nCnt + 1).MidPoint.Y);
                                    var l2 = new Line2d(c3, c4);

                                    var c5 = l1.IntersectWitch(l2);
                                    //FirstPoly.SetBulgeAt(nCnt, 0.0);
                                    _firstPoly.RemoveVertexAt(nCnt);
                                    // FirstPoly.RemoveVertexAt(nCnt);
                                    _firstPoly.AddVertexAt(nCnt, new Point2d(c5.real(), c5.imag()), 0.0, 0, 0);
                                    _firstPoly.RemoveVertexAt(nCnt + 1);
                                    // FirstPoly.RemoveVertexAt(nCnt);                        
                                    _firstPoly.AddVertexAt(nCnt + 1, new Point2d(c5.real(), c5.imag()), 0.0, 0, 0);
                                }
                            }
                            catch
                            {
                            }
                        }
                        for (var nCnt = 0; nCnt < secondPoly.NumberOfVertices - 1; nCnt++)
                        {
                            try
                            {
                                var radius = secondPoly.GetArcSegmentAt(nCnt).Radius;
                                if (radius < 0.011)
                                {

                                    var c1 = new Complex(secondPoly.GetLineSegment2dAt(nCnt).StartPoint.X, secondPoly.GetLineSegment2dAt(nCnt).StartPoint.Y);
                                    var c2 = new Complex(secondPoly.GetLineSegment2dAt(nCnt - 1).MidPoint.X, secondPoly.GetLineSegment2dAt(nCnt - 1).MidPoint.Y);
                                    var l1 = new Line2d(c1, c2);
                                    var c3 = new Complex(secondPoly.GetLineSegment2dAt(nCnt).EndPoint.X, secondPoly.GetLineSegment2dAt(nCnt).EndPoint.Y);
                                    var c4 = new Complex(secondPoly.GetLineSegment2dAt(nCnt + 1).MidPoint.X, secondPoly.GetLineSegment2dAt(nCnt + 1).MidPoint.Y);
                                    var l2 = new Line2d(c3, c4);

                                    var c5 = l1.IntersectWitch(l2);
                                    //SecondPoly.SetBulgeAt(nCnt, 0.0);
                                    secondPoly.RemoveVertexAt(nCnt);
                                    // SecondPoly.RemoveVertexAt(nCnt);
                                    secondPoly.AddVertexAt(nCnt, new Point2d(c5.real(), c5.imag()), 0.0, 0, 0);
                                    secondPoly.RemoveVertexAt(nCnt + 1);
                                    // SecondPoly.RemoveVertexAt(nCnt);                          
                                    secondPoly.AddVertexAt(nCnt + 1, new Point2d(c5.real(), c5.imag()), 0.0, 0, 0);

                                }
                            }
                            catch
                            {
                            }
                        }

                        CommandLineHelper.Command("._PEDIT", _firstPoly.ObjectId, "_J", acccLine.ObjectId, secondPoly.ObjectId, "\033", "\033");
                        CommandLineHelper.Command("._PEDIT", _firstPoly.ObjectId, "_J", accLine.ObjectId, "\033", "\033");
                    }

                    try
                    {
                        if (aSpConfig.SheetLayer.Contains("Current") && aSpConfig.SheetLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                        _firstPoly.Layer = aSpConfig.SheetLayer;
                    }
                    catch
                    {
                        var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                        _firstPoly.Layer = acLyrTblRec.Name;
                    }
                    try
                    {
                        if (aSpConfig.SheetColor.Name.Contains("ByL")) { throw new ArgumentNullException(); }
                        _firstPoly.Color = Color.FromColor(aSpConfig.SheetColor);
                    }
                    catch
                    {
                        try
                        {
                            if (aSpConfig.SheetLayer.Contains("Current") && aSpConfig.SheetLayer.Contains("Layer")) { throw new ArgumentNullException(); }
                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(acLyrTbl[aSpConfig.SheetLayer], OpenMode.ForWrite);
                            _firstPoly.Color = acLyrTblRec.Color;
                        }
                        catch
                        {
                            var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                            _firstPoly.Color = acLyrTblRec.Color;
                        }
                    }

                    acTrans.Commit();
                }
                ed.UpdateScreen();
                _counter++;
                #endregion
            }
            catch
            {
                Application.ShowAlertDialog(
                  "Tickness or Radius ERROR !" + "\n"
                  + aSpConfig.InternalRadius + "\n"
                  + aSpConfig.SheetThickness
                  );
            }
        }

        public struct SheetProfileConfig
        {
            public ObjectId PolylineId;

            public double SheetThickness;
            public double InternalRadius;

            public OffsetSide OffsetSide;
            public string SheetLayer;
            public System.Drawing.Color SheetColor;

            public bool DrawCoating;
            public CoatingSide CoatingSide;
            public string CoatingLayer;
            public System.Drawing.Color CoatingColor;

            public bool DrawDimensions;
            public string DimensionStyle;
            public string DimensionsLayer;
            public System.Drawing.Color DimensionsColor;

            public DimensionsStandart DimensionStandart;
        }

        public enum CoatingSide
        {
            Left = 1,
            Right = 2,
            Both = 3
        }


        public enum OffsetSide
        {
            Left = 1,
            Right = 2,
            Center = 3
        }

        public enum DimensionsStandart
        {
            ArcsAndLines = 0,
            Projections = 1
        }
    }


}
