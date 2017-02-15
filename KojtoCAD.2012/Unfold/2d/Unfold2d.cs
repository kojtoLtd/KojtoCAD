using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Castle.Core.Logging;
using KojtoCAD.GraphicItems.SheetProfile;
using KojtoCAD.Mathematics.Geometry;
using KojtoCAD.Properties;
using KojtoCAD.Unfold._2d;
using KojtoCAD.Utilities;

#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Matrix3d = Autodesk.AutoCAD.Geometry.Matrix3d;
using Point2d = Autodesk.AutoCAD.Geometry.Point2d;
using Point3d = Autodesk.AutoCAD.Geometry.Point3d;
using Point3dCollection = Autodesk.AutoCAD.Geometry.Point3dCollection;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Teigha.Colors;
using Application = Bricscad.ApplicationServices.Application;
#endif
[assembly: CommandClass(typeof(Unfold2D))]

namespace KojtoCAD.Unfold._2d
{
    public struct UnfoldSheetProfileConfig
    {
        public double Thickness;

        public Unfold2D.UnfoldingMethod UnfoldingMethod;
        public string BendDeductionTableFile;

        public double KFactorValue;

        public bool DrawCoating;
        public SheetProfile.CoatingSide CoatingSide;
        public string CoatingLayer;
        public System.Drawing.Color CoatingColor;

        public bool DrawDimensions;
        public string DimensionStyle;
        public string DimensionsLayer;
        public System.Drawing.Color DimensionsColor;
    }

    public class Unfold2D
    {
        //
        private static ObjectId _polylineId;
        private static Polyline _basePolyLine;
        private static Point3d _pickPoint;
        private static Complex _cPickPoint;
        private static int _num1; //first end most segment number
        private static int _num2; // second endmost segment number
        private static ObjectId _tLineId;
        private static ArrayList _unfoldPoints; //points from unfold line
        private static List<ObjectId> _entityTemp; //
        public static Unfold2dForm Unfold2DForm;
        private static Editor _ed;
        private static Database _db;
        private static ILogger _logger = NullLogger.Instance;
        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        public static ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        private static void Draw(Entity obj, Transaction trans)
        {

            var blkTbl = trans.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var blkTblRec = (BlockTableRecord) trans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            blkTblRec.AppendEntity(obj);
            trans.AddNewlyCreatedDBObject(obj, true);
        }

        private static void Draw(Entity obj, int colorindex, Transaction trans)
        {

            var blkTbl = trans.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var blkTblRec = (BlockTableRecord) trans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            obj.ColorIndex = colorindex;
            blkTblRec.AppendEntity(obj);
            trans.AddNewlyCreatedDBObject(obj, true);
        }

        private static void Draw(Entity obj, System.Drawing.Color color, string layer, Transaction trans)
        {
            var blkTbl = (BlockTable) trans.GetObject(_db.BlockTableId, OpenMode.ForRead);
            var blkTblRec = (BlockTableRecord) trans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            var acLyrTbl = (LayerTable) trans.GetObject(_db.LayerTableId, OpenMode.ForRead);

            blkTblRec.AppendEntity(obj);
            trans.AddNewlyCreatedDBObject(obj, true);
            try
            {
                if (color.Name.Contains("ByL"))
                {
                    throw new ArgumentNullException();
                }
                obj.Color = Color.FromColor(color);
            }
            catch
            {
                try
                {
                    if (layer.Contains("Current") && layer.Contains("Layer"))
                    {
                        throw new ArgumentNullException();
                    }
                    var acLyrTblRec = (LayerTableRecord) trans.GetObject(acLyrTbl[layer], OpenMode.ForWrite);
                    obj.Color = acLyrTblRec.Color;
                }
                catch
                {
                    var acLyrTblRec = (LayerTableRecord) trans.GetObject(_db.Clayer, OpenMode.ForWrite);
                    obj.Color = acLyrTblRec.Color;
                }
            }
            try
            {
                if (layer.Contains("Current") && layer.Contains("Layer"))
                {
                    throw new ArgumentNullException();
                }
                obj.Layer = layer;
            }
            catch
            {
                var acLyrTblRec = (LayerTableRecord) trans.GetObject(_db.Clayer, OpenMode.ForWrite);
                obj.Layer = acLyrTblRec.Name;
            }
        }

        private static AlignedDimension AddDim(Complex c1, Complex c2, double k, string layer,
            System.Drawing.Color color, string dimstyle, Transaction acTrans)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            // Open the Block table for read
            var acBlkTbl = (BlockTable) acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);

            // Open the Block table record Model space for write
            var acBlkTblRec =
                (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var acLyrTbl = (LayerTable) acTrans.GetObject(db.LayerTableId, OpenMode.ForRead);


            // Create the  dimension

            var qq = new Complex((c2 - c1)*Complex.polar(1.0, Math.PI/2.0));
            qq /= qq.abs();
            qq *= k;
            qq += (c2 + c1)/2.0;

            var acRotDim = new AlignedDimension();
            acRotDim.SetDatabaseDefaults();
            acRotDim.XLine1Point = new Point3d(c1.real(), c1.imag(), 0);
            acRotDim.XLine2Point = new Point3d(c2.real(), c2.imag(), 0);
            acRotDim.DimLinePoint = new Point3d(qq.real(), qq.imag(), 0);
            try
            {
                if (layer.Contains("Current") && layer.Contains("Layer"))
                {
                    throw new ArgumentNullException();
                }
                acRotDim.Layer = layer;
            }
            catch
            {
                var acLyrTblRec = (LayerTableRecord) acTrans.GetObject(db.Clayer, OpenMode.ForWrite);
                acRotDim.Layer = acLyrTblRec.Name;

            }
            try
            {
                //MessageBox.Show("U2d Dim color =" + color.Name);
                if (color.Name.Contains("ByL"))
                {
                    throw new ArgumentNullException();
                }
                acRotDim.Color = Color.FromColor(color);
            }
            catch
            {
                try
                {
                    if (layer.Contains("Current") && layer.Contains("Layer"))
                    {
                        throw new ArgumentNullException();
                    }
                    var acLyrTblRec =
                        (LayerTableRecord) acTrans.GetObject(acLyrTbl[layer], OpenMode.ForWrite);
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

            return acRotDim;
        }

        private static void PrepareRedraw()
        {
            if (_unfoldPoints.Count > 0)
            {
                _unfoldPoints.Clear();
            }
            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                if (_entityTemp.Count > 0)
                {
                    foreach (ObjectId p in _entityTemp)
                    {
                        var ple = (Entity) acTrans.GetObject(p, OpenMode.ForWrite);
                        ple.Erase();
                    }
                    _entityTemp.Clear();
                }

                acTrans.Commit();
            }
        }

        private int GetParalellSegment(int j, ref double distance)
        {
            var n = -1;
            double dist = -1;
            if (_basePolyLine.GetBulgeAt(j) == 0.0)
            {
                var seg = _basePolyLine.GetLineSegment2dAt(j);
                for (var i = 0; i < _basePolyLine.NumberOfVertices; i++)
                {
                    if (i != j)
                    {
                        if (_basePolyLine.GetBulgeAt(i) == 0.0)
                        {
                            var seg1 = _basePolyLine.GetLineSegment2dAt(i);
                            if (seg.IsParallelTo(seg1))
                            {
                                if (n < 0)
                                {
                                    dist = seg.GetDistanceTo(seg1);
                                    n = i;
                                }
                                else
                                {
                                    var d = seg.GetDistanceTo(seg1);
                                    if (d < dist)
                                    {
                                        dist = d;
                                        n = i;
                                    }
                                }

                            }
                        }
                    }
                }
            }
            distance = dist;
            return n;
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
                    var s1 = new Complex(seg.StartPoint.X, seg.StartPoint.Y);
                    var e1 = new Complex(seg.EndPoint.X, seg.EndPoint.Y);
                    var s2 = new Complex(seg1.StartPoint.X, seg1.StartPoint.Y);
                    var e2 = new Complex(seg1.EndPoint.X, seg1.EndPoint.Y);
                    var ang = Math.Abs(((e1 - s1)/(e2 - s2)).arg());
                    ang = ang*180.0/Math.PI;
                    if (!seg.IsParallelTo(seg1) && !(Math.Abs(ang) < 0.00001) &&
                        !(Math.Abs(180 - Math.Abs(ang)) < 0.00001))
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

        //first calculate the thickness
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

            if ((n >= 0) && (n <= _basePolyLine.NumberOfVertices - 1))
                //??? N = 0 is impossible. The first segment. The first segment must be right
            {
                var dist = -1.0;
                var n1 = GetParalellArc(n, ref dist);

                if ((n1 > 0) && (n1 < _basePolyLine.NumberOfVertices - 1))
                {
                    var seg = _basePolyLine.GetLineSegment2dAt(n); // find chord
                    var seg1 = _basePolyLine.GetLineSegment2dAt(n1); // find chord

                    //chords are opposite vectors
                    var p1 = seg.StartPoint;
                    var p2 = seg1.EndPoint;

                    rezult = p1.GetDistanceTo(p2);
                }
            }

            return rezult;
        }

        //returns the value found in the table - KFactorValue
        private static double GetKFactorValue(UnfoldSheetProfileConfig aSpConfig)
        {
            double rez = 0;
            if (aSpConfig.UnfoldingMethod == UnfoldingMethod.KFactor)
            {
                rez = aSpConfig.KFactorValue;
            }
            if (rez < 0)
            {
                MessageBox.Show("Incorrect K-factor Exception ! ", "IO ERROR");
            }
            return rez;
        }

        public static double GetBendDeductionValue(string aCsvFileFullName, double angle, double thickness,
            ref int status)
        {
            var rez = 0.0;
            try
            {
                var file = new FileStream(aCsvFileFullName, FileMode.Open);
                var csv = new StreamReader(file);

                var line = csv.ReadLine(); //заглавието     
                double t, a, bd, pt = 0.0, prebd = -1;

                csv.ReadLine(); // We need to skip the first line, because it contains table headers - no values  !

                while (!csv.EndOfStream)
                {
                    line = csv.ReadLine();
                    var dataarr = line.Split(new[] {' ', ',', ':', '\t'});
                    if (dataarr.Length != 3)
                    {
                        MessageBox.Show("An IO Exception - Incorrect table: " + aCsvFileFullName, "IO ERROR");
                        status = -1;
                        break;
                    }

                    t = Convert.ToDouble(dataarr[1]);
                    a = Convert.ToDouble(dataarr[0]);
                    bd = Convert.ToDouble(dataarr[2]);


                    if (thickness >= t)
                    {
                        pt = t;
                        if (a >= angle)
                        {
                            if (prebd < 0)
                            {
                                prebd = Math.Abs(bd);
                            }

                        }
                        else
                        {
                            prebd = -1;
                        }
                    }
                    else
                    {
                        if (Math.Abs(thickness - pt) < Math.Abs(thickness - t))
                        {
                            rez = prebd;
                            break;
                        }
                        if (a >= angle)
                        {
                            rez = bd;
                            break;
                        }
                    }
                }

                csv.Close();
                //}
            }
            catch (IOException ex)
            {
                var errMsg = "An IO Exception has been thrown while reading : " + aCsvFileFullName + "\n" +
                             ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace;
                MessageBox.Show(errMsg, "IO ERROR");
            }
            return rez;
        }

        //calculated using GetKFactorValue OR GetBendDeductionValue - depending radioButtons in aSPConfig
        private static double GetBendAllowance(double aThickness, double aInternalRadius, double aBendDegree,
            UnfoldSheetProfileConfig aSpConfig)
        {
            double ba;
            var beta = Math.Abs(Math.PI - aBendDegree*Math.PI/180);
                //further to 180 degrees (angle of the arc) in radians

            if (aSpConfig.UnfoldingMethod == UnfoldingMethod.BendDeduction)
            {
                var status = 1;
                var bd = GetBendDeductionValue(aSpConfig.BendDeductionTableFile, aBendDegree, aThickness, ref status);
                ba = 2*(aInternalRadius + aThickness)*Math.Tan(beta/2) - Math.Abs(bd);
                ba *= status;

                string errMsg;
                if (Math.Abs(bd) < 0.000000001)
                {
                    errMsg = "Angle = " + aBendDegree + " degree \n Incorrect Bend Deduction Value in Table = 0";
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(errMsg);
                }
                if (ba <= 0)
                {
                    errMsg = "Angle = " + aBendDegree + " degree \n Incorrect Bend Allowance = " + ba +
                             "\n\n Must be greater than zero !";
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(errMsg);
                }
            }
            else
            {
                var k = GetKFactorValue(aSpConfig);
                ba = (k*aThickness + aInternalRadius)*beta;
            }

            return ba;
        }

        /// <summary>
        /// Unfold 2D Sheet Profile
        /// </summary>
        [CommandMethod("u2d")]
        public void Unfold2DSheetProfileStart()
        {
            
            _ed = Application.DocumentManager.MdiActiveDocument.Editor;
            _db = Application.DocumentManager.MdiActiveDocument.Database;

            // Pick a polyline
            var polyOptions = new PromptEntityOptions("Pick a sheet profile :");
            polyOptions.SetRejectMessage(
                "\nThis is not a generated sheet profile.\nSheet Profile is generated from a polyline with SHP command.\nCreate a sheet profile and try again.");
            polyOptions.AddAllowedClass(typeof (Polyline), true);
            var polyResult = _ed.GetEntity(polyOptions);
            if (polyResult.Status != PromptStatus.OK)
            {
                return;
            }

            var pickInsertionPoint = _ed.GetPoint(new PromptPointOptions("Pick insertion point."));
            if (pickInsertionPoint.Status != PromptStatus.OK)
            {
                return;
            }

            _polylineId = polyResult.ObjectId;
            double thickness;

            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                _basePolyLine = (Polyline) acTrans.GetObject(_polylineId, OpenMode.ForWrite);
                if ((CountOfArcs() + 2 != _basePolyLine.NumberOfVertices >> 1) || (CountOfArcs()%2 > 0))
                {
                    CommandLineHelper.Command("._copy", _basePolyLine.ObjectId, "0,0", "\033", "0,0", "\033");

                    var acPts2D = new Point3dCollection();
                    for (var nCnt = 0; nCnt < _basePolyLine.NumberOfVertices; nCnt++)
                    {
                        acPts2D.Add(_basePolyLine.GetPoint3dAt(nCnt));
                    }
                    for (var nCnt = 1; nCnt < acPts2D.Count; nCnt++)
                    {
                        var point1 = acPts2D[nCnt];
                        var point2 = acPts2D[nCnt - 1];
                        var dist = point1.DistanceTo(point2);
                        if (dist < 0.001)
                        {
                            acPts2D.RemoveAt(nCnt);
                            _basePolyLine.RemoveVertexAt(nCnt);
                            nCnt--;
                        }
                    }

                    for (var nCnt = 0; nCnt < acPts2D.Count; nCnt++)
                    {
                        var point1 = acPts2D[nCnt];
                        var point2 = acPts2D[nCnt + 1];
                        var bulge = _basePolyLine.GetBulgeAt(nCnt);
                        if (bulge != 0)
                        {
                            var arc = _basePolyLine.GetArcSegmentAt(nCnt);
                            var ppp = arc.Center;
                            for (var nCnt1 = 1; nCnt1 < acPts2D.Count - 1; nCnt1++)
                            {
                                if (acPts2D[nCnt1].DistanceTo(ppp) < 0.01)
                                {
                                    var c1 = new Complex(acPts2D[nCnt1].X, acPts2D[nCnt1].Y);
                                    var c2 = new Complex(acPts2D[nCnt1 - 1].X, acPts2D[nCnt1 - 1].Y);
                                    var c3 = new Complex(acPts2D[nCnt1 + 1].X, acPts2D[nCnt1 + 1].Y);

                                    var ml1 = new KLine2D(c1, c2);
                                    var ml2 = new KLine2D(c1, c3);

                                    var c4 = c1 - c2;
                                    c4 /= c4.abs();
                                    c4 *= 0.1;
                                    var c5 = c1 - c3;
                                    c5 /= c5.abs();
                                    c5 *= 0.1;

                                    var c6 = new Complex(point1.X, point1.Y);
                                    var c7 = new Complex(point2.X, point2.Y);

                                    var ml11 = new KLine2D((c1 - c3).real(), (c1 - c3).imag(),
                                        -(c1 - c3).real()*c6.real() - (c1 - c3).imag()*c6.imag());
                                    var ml22 = new KLine2D((c1 - c2).real(), (c1 - c2).imag(),
                                        -(c1 - c2).real()*c7.real() - (c1 - c2).imag()*c7.imag());

                                    var c55 = ml11.IntersectWitch(ml2);
                                    var c44 = ml22.IntersectWitch(ml1);

                                    //c6 -= c5; c7 -= c4;
                                    c6 -= c5 - c1 + c55;
                                    c7 -= c4 - c1 + c44;
                                    c4 = c1 - c4;
                                    c5 = c1 - c5;

                                    _basePolyLine.RemoveVertexAt(nCnt);
                                    _basePolyLine.AddVertexAt(nCnt, new Point2d(c6.real(), c6.imag()), bulge, 0, 0);
                                    _basePolyLine.RemoveVertexAt(nCnt + 1);
                                    _basePolyLine.AddVertexAt(nCnt + 1, new Point2d(c7.real(), c7.imag()), 0, 0, 0);

                                    _basePolyLine.RemoveVertexAt(nCnt1);
                                    _basePolyLine.AddVertexAt(nCnt1, new Point2d(c5.real(), c5.imag()), 0, 0, 0);
                                    _basePolyLine.AddVertexAt(nCnt1, new Point2d(c4.real(), c4.imag()), -bulge, 0, 0);

                                    acPts2D.Clear();
                                    for (var nCnt2 = 0; nCnt2 < _basePolyLine.NumberOfVertices; nCnt2++)
                                    {
                                        acPts2D.Add(_basePolyLine.GetPoint3dAt(nCnt2));
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    acTrans.Commit();
                    CommandLineHelper.Command("._u2d", _basePolyLine.ObjectId, pickInsertionPoint.Value);
                    CommandLineHelper.Command("._erase", _basePolyLine.ObjectId, "\033");
                    return;
                    // MessageBox.Show("Must have an even number of ARC segments !" , "ERROR:");
                    // return;
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
                if (_basePolyLine.NumberOfVertices%2 != 0)
                {
                    MessageBox.Show("Must have an even number of segments !", "ERROR:");
                    return;
                }
                _pickPoint = pickInsertionPoint.Value;
                _cPickPoint = new Complex(_pickPoint.X, _pickPoint.Y);

                #region endmost_thickness_segment_search

                _num1 = -1;
                _num2 = -1;
                for (var i = 0; i < _basePolyLine.NumberOfVertices; i++)
                {
                    var next = i < _basePolyLine.NumberOfVertices - 1 ? i + 1 : 0;
                    var pre = i > 0 ? i - 1 : _basePolyLine.NumberOfVertices - 1;

                    var seg = _basePolyLine.GetLineSegment2dAt(i);
                    if (seg.Length - thickness < 0.000001)
                    {
                        var segPre = _basePolyLine.GetLineSegment2dAt(pre);
                        var segNext = _basePolyLine.GetLineSegment2dAt(next);

                        for (var j = next; j < _basePolyLine.NumberOfVertices; j++)
                        {
                            var seg1 = _basePolyLine.GetLineSegment2dAt(j);
                            if (seg1.Length - thickness < 0.000001)
                            {
                                if (Math.Abs(seg.Length - seg1.Length) < 0.000001)
                                {
                                    var cPre = new Complex(segPre.EndPoint.X, segPre.EndPoint.Y) -
                                               new Complex(segPre.StartPoint.X, segPre.StartPoint.Y);
                                    var cNext = new Complex(segNext.EndPoint.X, segNext.EndPoint.Y) -
                                                new Complex(segNext.StartPoint.X, segNext.StartPoint.Y);
                                    var ang = (cNext/cPre).arg();
                                    if (Math.Abs(ang*180/Math.PI - 180) < 0.001)
                                    {
                                        if (_num1 == -1)
                                        {
                                            _num1 = i;
                                            _num2 = j;
                                        }
                                        else
                                        {
                                            if (Math.Abs(j - i) == _basePolyLine.NumberOfVertices/2)
                                            {
                                                _num1 = i;
                                                _num2 = j;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var p1 = _basePolyLine.GetLineSegmentAt(_num1).EndPoint;
                var p2 = _basePolyLine.GetLineSegmentAt(_num1).StartPoint;
                var p3 = new Point3d((p1.X + p2.X)/2, (p1.Y + p2.Y)/2, (p1.Z + p2.Z)/2);
                var p4 = new Point3d(_ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis.X + p3.X,
                    _ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis.Y + p3.Y,
                    _ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis.Z + p3.Z);
                var v3D = p3.GetVectorTo(p4);
                var tLine = new Line(p1, p2);
                tLine.TransformBy(Matrix3d.Rotation(Math.PI/4.0, v3D, p3));
                tLine.SetDatabaseDefaults();
                Draw(tLine, 4, acTrans);
                _ed.Regen();
                _tLineId = tLine.ObjectId;
                //thickness = (BasePolyLine.GetLineSegment2dAt(NUM1)).Length;

                #endregion

                acTrans.Commit();
            }

            _unfoldPoints = new ArrayList();
            _entityTemp = new List<ObjectId>();

            Unfold2DForm = new Unfold2dForm(thickness);
            Unfold2DForm.U2D_PREPARE_REDRAW += PrepareRedraw;
            Unfold2DForm.ShowDialog();
            Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public static void ClearEvent()
        {
            Unfold2DForm.U2D_PREPARE_REDRAW -= PrepareRedraw;
            if (_unfoldPoints.Count > 0)
            {
                _unfoldPoints.Clear();
            }
            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                var l = (Line) acTrans.GetObject(_tLineId, OpenMode.ForWrite);
                l.Erase();

                acTrans.Commit();
            }
        }

        public static void ClearEventA()
        {
            Unfold2DForm.U2D_PREPARE_REDRAW -= PrepareRedraw;
            if (_unfoldPoints.Count > 0)
            {
                _unfoldPoints.Clear();
            }
            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                if (_entityTemp.Count > 0)
                {
                    foreach (ObjectId p in _entityTemp)
                    {
                        var ple = (Entity) acTrans.GetObject(p, OpenMode.ForWrite);
                        ple.Erase();
                    }
                    _entityTemp.Clear();
                }

                var l = (Line) acTrans.GetObject(_tLineId, OpenMode.ForWrite);
                l.Erase();

                acTrans.Commit();
            }
        }

        public static void Draw2DUnfolding(UnfoldSheetProfileConfig aU2DConfig)
        {
            /*      #region Try to load bend deductions
                  if (aU2DConfig.UnfoldingMethod == UnfoldingMethod.BendDeduction)
                  {
                    BDeductions = new BendDeduction(aU2DConfig.BendDeductionTableFile);
                  }
                  else if (aU2DConfig.UnfoldingMethod == UnfoldingMethod.KFactor)
                  {
                    throw new System.Exception("Not implemented");
                  }
                  else if (aU2DConfig.UnfoldingMethod == UnfoldingMethod.BendAllowence)
                  {
                    throw new System.Exception("Not implemented");
                  }
                  #endregion*/

            var angArr = new ArrayList();
            var angArrP = new ArrayList();

            #region find points of unfolding and adding to an array

            var off = -1;
            _unfoldPoints.Add(_cPickPoint);
            using (_basePolyLine = (Polyline) _polylineId.Open(OpenMode.ForRead))
                for (var j = 1; j < _basePolyLine.NumberOfVertices >> 1; j++)
                {
                    if (_num1 + j > _basePolyLine.NumberOfVertices - 1)
                    {
                        off++;
                    }
                    var i = off < 0 ? _num1 + j : off;

                    var seg = _basePolyLine.GetLineSegment2dAt(i);
                    if (Math.Abs(_basePolyLine.GetBulgeAt(i)) < 0.0000000001)
                    {
                        var c = new Complex(((Complex) _unfoldPoints[_unfoldPoints.Count - 1]).real() + seg.Length,
                            ((Complex) _unfoldPoints[_unfoldPoints.Count - 1]).imag());
                        _unfoldPoints.Add(c);
                    }
                    else
                    {
                        var bAllowance = 0.0;
                        var seg1 = _basePolyLine.GetLineSegment2dAt(GetParalellArc(i, ref bAllowance));
                            //namira hordata na drugata daga

                        var cS = new Complex(seg.StartPoint.X, seg.StartPoint.Y); //start 1 daga
                        var cE = new Complex(seg.EndPoint.X, seg.EndPoint.Y); //end 1 daga
                        var cS1 = new Complex(seg1.StartPoint.X, seg1.StartPoint.Y); // start 2 daga
                        var cE1 = new Complex(seg1.EndPoint.X, seg1.EndPoint.Y); // end 2 daga

                        var c = cS - cE1; // na4alo na dygov segment (hordite sa v obratni posoki)
                        var c1 = cE - cS1; // krai na dygov segment (hordite sa v obratni posoki)

                        var ang = Math.Abs(c.arg() - c1.arg()); //ang betwen endmost lines (c,c1)
                        var iR1 = seg.Length/2/Math.Sin(ang/2); //radius do daga 1
                        var iR2 = seg1.Length/2/Math.Sin(ang/2); // radius do daga 2
                        var iR = iR1 < iR2 ? iR1 : iR2; // Internal is the smaller
                        ang = Math.Abs(Math.PI - ang); // angle between segments is supplementary to 180
                        ang = ang*180/Math.PI; // to degree                

                        Settings.Default.U2D_InternalRadius = (decimal) iR;
                        bAllowance = GetBendAllowance(aU2DConfig.Thickness, iR, ang, aU2DConfig); //dylgina na dygata
                        if (bAllowance < 0)
                        {
                            return;
                        }
                        angArr.Add(ang);
                        angArrP.Add(bAllowance/2 + 1.5);
                        var cc = new Complex(((Complex) _unfoldPoints[_unfoldPoints.Count - 1]).real() + bAllowance,
                            ((Complex) _unfoldPoints[_unfoldPoints.Count - 1]).imag());
                        _unfoldPoints.Add(cc);
                    }

                }

            #endregion

            #region drawing unfold

            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                for (var i = 1; i < _unfoldPoints.Count - 1; i++)
                {
                    var l =
                        new Line(
                            new Point3d(((Complex) _unfoldPoints[i]).real(), ((Complex) _unfoldPoints[i]).imag(), 0),
                            new Point3d(((Complex) _unfoldPoints[i]).real(),
                                ((Complex) _unfoldPoints[i]).imag() + aU2DConfig.Thickness, 0));
                    l.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                    Draw(l, acTrans);
                    _entityTemp.Add(l.ObjectId);
                }

                var acBlkTbl = (BlockTable) acTrans.GetObject(_db.BlockTableId, OpenMode.ForRead);
                var acBlkTblRec =
                    (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                var acPoly = new Polyline();
                acPoly.SetDatabaseDefaults();
                acPoly.AddVertexAt(0,
                    new Point2d(((Complex) _unfoldPoints[0]).real(), ((Complex) _unfoldPoints[0]).imag()), 0, 0, 0);
                acPoly.AddVertexAt(0,
                    new Point2d(((Complex) _unfoldPoints[_unfoldPoints.Count - 1]).real(),
                        ((Complex) _unfoldPoints[_unfoldPoints.Count - 1]).imag()), 0, 0, 0);
                acPoly.AddVertexAt(0,
                    new Point2d(((Complex) _unfoldPoints[_unfoldPoints.Count - 1]).real(),
                        ((Complex) _unfoldPoints[_unfoldPoints.Count - 1]).imag() + aU2DConfig.Thickness), 0, 0, 0);
                acPoly.AddVertexAt(0,
                    new Point2d(((Complex) _unfoldPoints[0]).real(),
                        ((Complex) _unfoldPoints[0]).imag() + aU2DConfig.Thickness), 0, 0, 0);
                acBlkTblRec.AppendEntity(acPoly);
                acTrans.AddNewlyCreatedDBObject(acPoly, true);
                acPoly.Closed = true;

                acPoly.TransformBy(_ed.CurrentUserCoordinateSystem); //?

                _entityTemp.Add(acPoly.ObjectId);

                acTrans.Commit();
            }

            #endregion

            #region drawing dimensions

            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                if (aU2DConfig.DrawDimensions)
                {
                    var ccc = new Complex(0, aU2DConfig.Thickness);
                    var cccc = new Complex(0, aU2DConfig.Thickness + 16);

                    for (var i = 1; i < _unfoldPoints.Count; i++)
                    {
                        var td = AddDim(
                            (Complex) _unfoldPoints[i - 1],
                            (Complex) _unfoldPoints[i],
                            aU2DConfig.DrawCoating ? -aU2DConfig.Thickness - 8 : -8,
                            aU2DConfig.DimensionsLayer,
                            aU2DConfig.DimensionsColor,
                            aU2DConfig.DimensionStyle,
                            acTrans);
                        td = (AlignedDimension) acTrans.GetObject(td.Id, OpenMode.ForWrite);
                        td.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                        _entityTemp.Add(td.ObjectId);
                        i++;
                    }
                    for (var i = 2; i < _unfoldPoints.Count; i++)
                    {
                        var td = AddDim(
                            (Complex) _unfoldPoints[i - 1] + ccc,
                            (Complex) _unfoldPoints[i] + ccc, aU2DConfig.DrawCoating ? aU2DConfig.Thickness + 8 : 8,
                            aU2DConfig.DimensionsLayer,
                            aU2DConfig.DimensionsColor,
                            aU2DConfig.DimensionStyle,
                            acTrans);
                        td = (AlignedDimension) acTrans.GetObject(td.Id, OpenMode.ForWrite);
                        td.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                        _entityTemp.Add(td.ObjectId);
                        i++;
                    }

                    var tdd = AddDim(
                        (Complex) _unfoldPoints[0] + cccc,
                        (Complex) _unfoldPoints[_unfoldPoints.Count - 1] + cccc,
                        aU2DConfig.DrawCoating ? aU2DConfig.Thickness + 8 : 8,
                        aU2DConfig.DimensionsLayer,
                        aU2DConfig.DimensionsColor,
                        aU2DConfig.DimensionStyle,
                        acTrans);
                    tdd = (AlignedDimension) acTrans.GetObject(tdd.Id, OpenMode.ForWrite);
                    tdd.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                    _entityTemp.Add(tdd.ObjectId);

                    var ccccc = (Complex) _unfoldPoints[0];
                    for (var i = 2; i < _unfoldPoints.Count; i++)
                    {
                        var mid = ((Complex) _unfoldPoints[i] + (Complex) _unfoldPoints[i - 1])/2;
                        var tddd = AddDim(
                            ccccc,
                            mid,
                            aU2DConfig.DrawCoating ? 2*aU2DConfig.Thickness + 16 : aU2DConfig.Thickness + 16,
                            aU2DConfig.DimensionsLayer,
                            aU2DConfig.DimensionsColor,
                            aU2DConfig.DimensionStyle, acTrans);
                        tddd = (AlignedDimension) acTrans.GetObject(tddd.Id, OpenMode.ForWrite);
                        tddd.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                        _entityTemp.Add(tddd.ObjectId);
                        ccccc = mid;
                        //
                        var acText = new DBText
                        {
                            Position =
                                new Point3d(((Complex) _unfoldPoints[i]).real() - (double) angArrP[0],
                                    ((Complex) _unfoldPoints[i]).imag() +
                                    (aU2DConfig.DrawCoating ? -2*aU2DConfig.Thickness - 10 : -aU2DConfig.Thickness - 10),
                                    0),
                            Height = 3,
                            Rotation = -Math.PI/2,
                            TextString = ((double) angArr[0]).ToString("f3") + "%%d"
                        };
                        angArr.RemoveAt(0);
                        angArrP.RemoveAt(0);
                        acText.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                        Draw(acText, acTrans);
                        _entityTemp.Add(acText.ObjectId);
                        //
                        i++;
                    }
                    var tdod = AddDim(
                        ccccc,
                        (Complex) _unfoldPoints[_unfoldPoints.Count - 1],
                        aU2DConfig.DrawCoating ? 2*aU2DConfig.Thickness + 16 : aU2DConfig.Thickness + 16,
                        aU2DConfig.DimensionsLayer,
                        aU2DConfig.DimensionsColor,
                        aU2DConfig.DimensionStyle,
                        acTrans);
                    tdod = (AlignedDimension) acTrans.GetObject(tdod.Id, OpenMode.ForWrite);
                    tdod.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                    _entityTemp.Add(tdod.ObjectId);

                    //debelina
                    var tdrd = AddDim(
                        (Complex) _unfoldPoints[0],
                        (Complex) _unfoldPoints[0] + ccc,
                        10,
                        aU2DConfig.DimensionsLayer,
                        aU2DConfig.DimensionsColor,
                        aU2DConfig.DimensionStyle,
                        acTrans);
                    tdrd = (AlignedDimension) acTrans.GetObject(tdrd.Id, OpenMode.ForWrite);
                    tdrd.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                    _entityTemp.Add(tdrd.ObjectId);
                }
                acTrans.Commit();
            }

            #endregion

            #region drawing coating

            if (aU2DConfig.DrawCoating)
            {
                using (var acTrans = _db.TransactionManager.StartTransaction())
                {
                    var ccc = new Complex(0, aU2DConfig.Thickness);
                    if ((aU2DConfig.CoatingSide == SheetProfile.CoatingSide.Left) ||
                        (aU2DConfig.CoatingSide == SheetProfile.CoatingSide.Both))
                    {
                        var l =
                            new Line(
                                new Point3d(((Complex) _unfoldPoints[0] - ccc).real(),
                                    ((Complex) _unfoldPoints[0] - ccc).imag(), 0),
                                new Point3d(((Complex) _unfoldPoints[_unfoldPoints.Count - 1] - ccc).real(),
                                    ((Complex) _unfoldPoints[_unfoldPoints.Count - 1] - ccc).imag(), 0));
                        l.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                        Draw(l, aU2DConfig.CoatingColor, aU2DConfig.CoatingLayer, acTrans);
                        _entityTemp.Add(l.ObjectId);
                    }

                    if ((aU2DConfig.CoatingSide == SheetProfile.CoatingSide.Right) ||
                        (aU2DConfig.CoatingSide == SheetProfile.CoatingSide.Both))
                    {
                        var ll =
                            new Line(
                                new Point3d(((Complex) _unfoldPoints[0] + ccc*2.0).real(),
                                    ((Complex) _unfoldPoints[0] + ccc*2.0).imag(), 0),
                                new Point3d(((Complex) _unfoldPoints[_unfoldPoints.Count - 1] + ccc*2.0).real(),
                                    ((Complex) _unfoldPoints[_unfoldPoints.Count - 1] + ccc*2.0).imag(), 0));
                        ll.TransformBy(_ed.CurrentUserCoordinateSystem); //?
                        Draw(ll, aU2DConfig.CoatingColor, aU2DConfig.CoatingLayer, acTrans);
                        _entityTemp.Add(ll.ObjectId);
                    }
                    acTrans.Commit();
                }
            }

            #endregion

            _ed.Regen();
        }

        public static void GetSheetProfileThickness( /*ObjectId aSheetProfilePolyObjectId , Point3d aPickedPoint*/)
        {
            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                var tLine = (Line) acTrans.GetObject(_tLineId, OpenMode.ForWrite);
                _basePolyLine = (Polyline) acTrans.GetObject(_polylineId, OpenMode.ForWrite);

                tLine.Erase();
                //        Ed.Regen();

                #region endmost_thickness_segment_search

                var buf = _num1;
                _num1 = _num2;
                _num2 = buf;

                var p1 = _basePolyLine.GetLineSegmentAt(_num1).EndPoint;
                var p2 = _basePolyLine.GetLineSegmentAt(_num1).StartPoint;
                var p3 = new Point3d((p1.X + p2.X)/2, (p1.Y + p2.Y)/2, (p1.Z + p2.Z)/2);
                var p4 = new Point3d(_ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis.X + p3.X,
                    _ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis.Y + p3.Y,
                    _ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis.Z + p3.Z);
                var v3D = p3.GetVectorTo(p4);

                tLine = new Line(p1, p2);
                tLine.TransformBy(Matrix3d.Rotation(Math.PI/4.0, v3D, p3));
                tLine.SetDatabaseDefaults();
                Draw(tLine, 2, acTrans);
                _ed.Regen();
                _tLineId = tLine.ObjectId;
                // thickness = (BasePolyLine.GetLineSegment2dAt(NUM1)).Length;

                #endregion

                acTrans.Commit();
            }
            _ed.Regen();
            //return thickness;
        }

        public enum UnfoldingMethod
        {
            BendDeduction = 1,
            KFactor = 2,
            BendAllowence = 3
        }

    }
}