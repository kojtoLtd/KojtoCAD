using Castle.Core.Logging;
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
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Teigha.Colors;
using Application = Bricscad.ApplicationServices.Application;
#endif
[assembly: CommandClass(typeof(KojtoCAD.GraphicItems.SheetProfile.SheetDescription))]

namespace KojtoCAD.GraphicItems.SheetProfile
{
    public class SheetDescription
    {
        private static SheetDescriptionForm _sheetDescriptionForm;

        private static ObjectId _polylineId;
        private static Polyline _basePolyLine;

        private static int _lastSegment; //first end most segment number = last segment
        private static int _penultimateSegment; // second endmost segment number = penultimate segment

        private static ArrayList _entityTemp;
        private static Point3d _leaderPoint;

        private static Editor _ed = Application.DocumentManager.MdiActiveDocument.Editor;
        private static Database _db = Application.DocumentManager.MdiActiveDocument.Database;

        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;

        public SheetDescription()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        private static int GetParalellArc(int j, ref double distance)
        {
            int N = -1;
            double dist = -1;
            if (Math.Abs(_basePolyLine.GetBulgeAt(j) - 0.0) > Double.Epsilon)
            {
                LineSegment2d segment = _basePolyLine.GetLineSegment2dAt(j);
                for (int i = 0; i < _basePolyLine.NumberOfVertices; i++)
                {
                    if (i != j)
                    {
                        if (_basePolyLine.GetBulgeAt(i) != 0.0)
                        {
                            LineSegment2d seg1 = _basePolyLine.GetLineSegment2dAt(i);
                            if (segment.IsParallelTo(seg1))
                            {
                                if (N < 0)
                                {
                                    dist = segment.GetDistanceTo(seg1);
                                    N = i;
                                }
                                else
                                {
                                    double d = segment.GetDistanceTo(seg1);
                                    if (d < dist)
                                    {
                                        dist = d;
                                        N = i;
                                    }
                                }

                            }
                        }
                    }
                }
            }
            distance = dist;
            return N;
        }

        private int CountOfArcs()
        {
            int rez = 0;
            for (int i = 0; i < _basePolyLine.NumberOfVertices; i++)
            {
                if (Math.Abs(_basePolyLine.GetBulgeAt(i) - 0) > Double.Epsilon)
                {
                    rez++; // save index i N               
                }
            }
            return rez;
        }

        private double GetThickness()
        {
            double rezult = -1.0;
            int N = -1;
            #region Search_first_ARC_segment_and_save_index_in_N
            for (int i = 0; i < _basePolyLine.NumberOfVertices; i++)
            {
                if (Math.Abs(_basePolyLine.GetBulgeAt(i) - 0) > Double.Epsilon)
                {
                    N = i; // save indeks i N
                    break; // stop the cycle 
                }
            }
            #endregion

            if ((N >= 0) && (N <= (_basePolyLine.NumberOfVertices - 1)))//??? N = 0 is impossible. The first segment. The first segment must be right
            {
                double dist = -1.0;
                int N1 = GetParalellArc(N, ref dist);

                if ((N1 > 0) && (N1 < (_basePolyLine.NumberOfVertices - 1)))
                {
                    LineSegment2d seg = _basePolyLine.GetLineSegment2dAt(N);// find chord
                    LineSegment2d seg1 = _basePolyLine.GetLineSegment2dAt(N1);// find chord

                    //chords are opposite vectors
                    Point2d p1 = seg.StartPoint;
                    Point2d p2 = seg1.EndPoint;

                    rezult = p1.GetDistanceTo(p2);
                }
            }

            return rezult;
        }

        private static double DrawOffset(int N1, int N2, double dist, bool boo)
        {
           
            double rez = 0;
            using (Transaction transaction = _db.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = transaction.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord modelSpace = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace],OpenMode.ForWrite) as BlockTableRecord;

                LayerTable layerTable = transaction.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;

                Polyline bpLine = (Polyline)transaction.GetObject(_polylineId, OpenMode.ForWrite);

                int off = -1;
                Polyline firstPoly = new Polyline();
                firstPoly.SetDatabaseDefaults();

                #region make poly
                ArrayList arrayList = new ArrayList();
                for (int j = 1; j <= (bpLine.NumberOfVertices >> 1); j++)
                {
                    if ((N1 + j) > (bpLine.NumberOfVertices - 1))
                    {
                        off++;
                    }
                    int i = (off < 0) ? (N1 + j) : off;
                    LineSegment2d seg = bpLine.GetLineSegment2dAt(i);
                    double bulge;
                    if ((i - 1) < 0)
                    {
                        bulge = bpLine.GetBulgeAt(bpLine.NumberOfVertices - 1);
                    }
                    else
                    {
                        if ((i - 1) > bpLine.NumberOfVertices - 1)
                        {
                            bulge = bpLine.GetBulgeAt(0);
                        }
                        else
                        {
                            bulge = -bpLine.GetBulgeAt(i - 1);
                        }

                    }
                    firstPoly.AddVertexAt(0, new Point2d(seg.StartPoint.X, seg.StartPoint.Y), bulge, 0, 0);
                    arrayList.Add(bpLine.GetBulgeAt(i));
                }

                modelSpace.AppendEntity(firstPoly);
                transaction.AddNewlyCreatedDBObject(firstPoly, true);
                #endregion

                #region coating
                if (boo)
                {
                    LineSegment2d seg1 = firstPoly.GetLineSegment2dAt(0);
                    LineSegment2d seg2 = bpLine.GetLineSegment2dAt(N2);
                    Complex mid = new Complex((seg2.StartPoint.X + seg2.EndPoint.X) / 2, (seg2.StartPoint.Y + seg2.EndPoint.Y) / 2);
                    Line2d l = new Line2d(new Complex(seg1.StartPoint.X, seg1.StartPoint.Y), new Complex(seg1.EndPoint.X, seg1.EndPoint.Y));

                    int k = l.PositionOfТhePointToLineSign(mid);

                    DBObjectCollection oc = firstPoly.GetOffsetCurves((k < 0) ? dist : -dist);
                    foreach (Entity acEnt in oc)
                    {
                        modelSpace.AppendEntity(acEnt);
                        transaction.AddNewlyCreatedDBObject(acEnt, true);
                        acEnt.TransformBy(_ed.CurrentUserCoordinateSystem);
                        try
                        {
                            acEnt.Layer = _sheetDescriptionForm.GetCoatingLayer();
                        }
                        catch
                        {
                            LayerTableRecord acLyrTblRec = transaction.GetObject(_db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                            acEnt.Layer = acLyrTblRec.Name;
                        }
                        try
                        {
                            System.Drawing.Color color = _sheetDescriptionForm.GetCoatingColor();
                            if (color.Name == "ByLayer")
                            {
                                throw new ArgumentNullException();

                            }
                            acEnt.Color = Color.FromColor(color);
                        }
                        catch
                        {
                            //LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                            LayerTableRecord acLyrTblRec = transaction.GetObject(layerTable[acEnt.Layer], OpenMode.ForWrite) as LayerTableRecord;
                            acEnt.Color = acLyrTblRec.Color;
                        }

                        _entityTemp.Add(acEnt);
                    }
                    Polyline acPoly11 = oc[0] as Polyline;
                }
                #endregion

                #region calculate length
                for (int j = 0; j < firstPoly.NumberOfVertices - 1; j++)
                {
                    if (Math.Abs(firstPoly.GetBulgeAt(j)) != 0.0)
                    {
                        CircularArc2d arc = firstPoly.GetArcSegment2dAt(j);
                        double rad = arc.Radius;
                        double ang = Math.Abs(Math.Atan(firstPoly.GetBulgeAt(j)));
                        rez += (4 * ang * rad);
                    }
                    else
                    {
                        LineSegment2d seg = firstPoly.GetLineSegment2dAt(j);
                        Complex s = new Complex(seg.StartPoint.X, seg.StartPoint.Y);
                        Complex e = new Complex(seg.EndPoint.X, seg.EndPoint.Y);
                        rez += (s - e).abs();
                    }
                }
                #endregion

                firstPoly.Erase();
                transaction.Commit();
            }
            _ed.Regen();
            return rez;
        }

        private static void Drawtext(double Area, double len, double density, string material)
        {
            double total_len = 0;
            #region calculate length
            for (int j = 0; j < _basePolyLine.NumberOfVertices; j++)
            {
                if (Math.Abs(_basePolyLine.GetBulgeAt(j)) != 0.0)
                {
                    CircularArc2d arc = _basePolyLine.GetArcSegment2dAt(j);
                    double rad = arc.Radius;
                    double ang = Math.Abs(Math.Atan(_basePolyLine.GetBulgeAt(j)));
                    total_len += (4 * ang * rad);
                }
                else
                {
                    LineSegment2d seg = _basePolyLine.GetLineSegment2dAt(j);
                    Complex s = new Complex(seg.StartPoint.X, seg.StartPoint.Y);
                    Complex e = new Complex(seg.EndPoint.X, seg.EndPoint.Y);
                    total_len += (s - e).abs();
                }
            }
            #endregion

            #region draw leader text
            double dist = _sheetDescriptionForm.GetThickness();
            using (Transaction acTrans = _db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(_db.BlockTableId,
                                             OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;


                DBText[] acText = new DBText[] { new DBText(), new DBText(), new DBText(), new DBText(), new DBText(), new DBText(), new DBText(), new DBText() };
                Point3d p = _leaderPoint;
                acText[0].Position = p;
                acText[1].Position = new Point3d(p.X, p.Y - 4 * 2, 0);
                acText[2].Position = new Point3d(p.X, p.Y - 4 * 4, 0);
                acText[3].Position = new Point3d(p.X, p.Y - 4 * 6, 0);
                acText[4].Position = new Point3d(p.X, p.Y - 4 * 8, 0);
                acText[5].Position = new Point3d(p.X, p.Y - 4 * 10, 0);
                acText[6].Position = new Point3d(p.X, p.Y - 4 * 12, 0);
                acText[7].Position = new Point3d(p.X, p.Y - 4 * 14, 0);

                acText[0].TextString = "Material :                        " + material;
                acText[1].TextString = "Total Surface Area :        " + (total_len / 10).ToString("f4") + " dm2";
                acText[2].TextString = "Area of section :               " + (Area / (10 * 10)).ToString("f4") + " dm2";
                acText[3].TextString = "Coating Lines Length :      " + (len / 100).ToString("f4") + " dm ";
                acText[4].TextString = "Coating Area :                  " + (len / 10).ToString("f4") + " dm2/m";

                Area /= 1000000;//1 m^2
                double Volume = Area * 1.0;
                double mass = Volume * density;

                acText[5].TextString = "Mass per meter :              " + mass.ToString("f5") + " kg.";
                acText[6].TextString = "Volume per meter :             " + Volume.ToString("f5") + " m3";
                acText[7].TextString = "Density :                         " + (density / 1000).ToString("f3") + " kg/dm3";


                foreach (DBText t in acText)
                {
                    t.Height = 4;
                    #region layer and color
                    try
                    {
                        if (_sheetDescriptionForm.GetCoatingLayer().Contains("Current") && _sheetDescriptionForm.GetCoatingLayer().Contains("Layer"))
                        {
                            throw new ArgumentNullException();
                        }
                        t.Layer = _sheetDescriptionForm.GetCoatingLayer();
                    }
                    catch
                    {
                        LayerTableRecord acLyrTblRec = acTrans.GetObject(_db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                        t.Layer = acLyrTblRec.Name;

                    }
                    try
                    {
                        //MessageBox.Show("U2d Dim color =" + color.Name);
                        if (_sheetDescriptionForm.GetCoatingColor().Name.Contains("ByL"))
                        {
                            throw new ArgumentNullException();
                        }
                        t.Color = Color.FromColor(_sheetDescriptionForm.GetCoatingColor());
                    }
                    catch
                    {
                        // LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                        // acRotDim.Color = acLyrTblRec.Color;
                        try
                        {
                            if (_sheetDescriptionForm.GetCoatingLayer().Contains("Current") && _sheetDescriptionForm.GetCoatingLayer().Contains("Layer"))
                            {
                                throw new ArgumentNullException();
                            }
                            LayerTableRecord acLyrTblRec = acTrans.GetObject(acLyrTbl[_sheetDescriptionForm.GetCoatingLayer()], OpenMode.ForWrite) as LayerTableRecord;
                            t.Color = acLyrTblRec.Color;
                        }
                        catch
                        {
                            LayerTableRecord acLyrTblRec = acTrans.GetObject(_db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                            t.Color = acLyrTblRec.Color;
                        }

                        // acBlkTblRec.AppendEntity(t);
                        // acTrans.AddNewlyCreatedDBObject(t, true);                       
                    }

                    #endregion
                    t.TransformBy(_ed.CurrentUserCoordinateSystem);
                    acBlkTblRec.AppendEntity(t);
                    acTrans.AddNewlyCreatedDBObject(t, true);
                    _entityTemp.Add(t);
                }

                acTrans.Commit();
            }
            #endregion
            _ed.Regen();
        }

        /// <summary>
        /// Sheet metal description 
        /// </summary>
        [CommandMethod("shd")]
        public void SheetDescriptionStart()
        {
            
            _basePolyLine = new Polyline();
            _lastSegment = _penultimateSegment = -1;
            _polylineId = new ObjectId();

            // Pick a polyline
            PromptEntityOptions PolyOptions = new PromptEntityOptions("Pick a sheet profile :");
            PolyOptions.SetRejectMessage("\nThis is not a generated sheet profile.\nSheet Profile is generated from a polyline with SHP command.\nCreate a sheet profile and try again.");
            PolyOptions.AddAllowedClass(typeof(Polyline), true);
            PromptEntityResult PolyResult = _ed.GetEntity(PolyOptions);
            if (PolyResult.Status != PromptStatus.OK)
            {
                return;
            }

            PromptPointResult PickInsertionPoint = _ed.GetPoint(new PromptPointOptions("Pick Leader point."));
            if (PickInsertionPoint.Status != PromptStatus.OK)
            {
                return;
            }

            _leaderPoint = PickInsertionPoint.Value;

            _polylineId = PolyResult.ObjectId;
            double thickness = 0.0;

            try
            {

                #region transaction
                using (Transaction acTrans = _db.TransactionManager.StartTransaction())
                {
                    _basePolyLine = (Polyline)acTrans.GetObject(_polylineId, OpenMode.ForWrite);
                    if ((Math.Abs(_basePolyLine.StartPoint.TransformBy(_ed.CurrentUserCoordinateSystem.Inverse()).Z) > 0.000001) ||
                        (Math.Abs(_basePolyLine.EndPoint.TransformBy(_ed.CurrentUserCoordinateSystem.Inverse()).Z) > 0.000001))
                    {
                        MessageBox.Show("Polyline must lie in the XY Plane of the UCS !", "ERROR:");
                        return;
                    }
                    if (((CountOfArcs() + 2) != (_basePolyLine.NumberOfVertices >> 1)) || ((CountOfArcs() % 2) > 0))
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
                    if ((_basePolyLine.NumberOfVertices % 2) != 0)
                    {
                        MessageBox.Show("Must have an even number of segments !", "ERROR:");
                        return;
                    }

                    #region endmost_thickness_segment_search
                    _lastSegment = -1;
                    _penultimateSegment = -1;
                    for (int i = 0; i < _basePolyLine.NumberOfVertices; i++)
                    {
                        int next = (i < (_basePolyLine.NumberOfVertices - 1)) ? (i + 1) : 0;
                        int pre = (i > 0) ? (i - 1) : (_basePolyLine.NumberOfVertices - 1);

                        LineSegment2d seg = _basePolyLine.GetLineSegment2dAt(i);
                        if ((seg.Length - thickness) < 0.000001)
                        {
                            LineSegment2d seg_pre = _basePolyLine.GetLineSegment2dAt(pre);
                            LineSegment2d seg_next = _basePolyLine.GetLineSegment2dAt(next);

                            for (int j = next; j < _basePolyLine.NumberOfVertices; j++)
                            {
                                LineSegment2d seg1 = _basePolyLine.GetLineSegment2dAt(j);
                                if ((seg1.Length - thickness) < 0.000001)
                                {
                                    if (Math.Abs(seg.Length - seg1.Length) < 0.000001)
                                    {
                                        Complex cPre = (new Complex(seg_pre.EndPoint.X, seg_pre.EndPoint.Y)) - (new Complex(seg_pre.StartPoint.X, seg_pre.StartPoint.Y));
                                        Complex cNext = (new Complex(seg_next.EndPoint.X, seg_next.EndPoint.Y)) - (new Complex(seg_next.StartPoint.X, seg_next.StartPoint.Y));
                                        double ang = (cNext / cPre).arg();
                                        if (Math.Abs(ang * 180 / Math.PI - 180) < 0.001)
                                        {
                                            if (_lastSegment == -1)
                                            {
                                                _lastSegment = i;
                                                _penultimateSegment = j;
                                            }
                                            else
                                            {
                                                if (Math.Abs(j - i) == (int)(_basePolyLine.NumberOfVertices / 2))
                                                {
                                                    _lastSegment = i;
                                                    _penultimateSegment = j;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    _ed.Regen();

                    #endregion

                    BlockTable acBlkTbl;
                    acBlkTbl = acTrans.GetObject(_db.BlockTableId, OpenMode.ForWrite) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Polyline pl = _basePolyLine.Clone() as Polyline;
                    acBlkTblRec.AppendEntity(pl);
                    acTrans.AddNewlyCreatedDBObject(pl, true);
                    _basePolyLine.Visible = false;

                    _basePolyLine.TransformBy(_ed.CurrentUserCoordinateSystem.Inverse());

                    acTrans.Commit();
                }
                #endregion

                _entityTemp = new ArrayList();

                _sheetDescriptionForm = new SheetDescriptionForm(thickness);
                _sheetDescriptionForm.ShowDialog();
                using (Transaction acTrans = _db.TransactionManager.StartTransaction())
                {
                    _basePolyLine = (Polyline)acTrans.GetObject(_polylineId, OpenMode.ForWrite);
                    _basePolyLine.Erase();
                    acTrans.Commit();
                }
            }
            catch
            {
                using (Transaction acTrans = _db.TransactionManager.StartTransaction())
                {
                    _basePolyLine = (Polyline)acTrans.GetObject(_polylineId, OpenMode.ForWrite);
                    _basePolyLine.Erase();
                    acTrans.Commit();
                }
            }

            _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public static void RedrawDescription()
        {
            double len = 0;
            if (_entityTemp.Count > 0)
            {
                using (Transaction acTrans = _db.TransactionManager.StartTransaction())
                {
                    foreach (Entity acEnt in _entityTemp)
                    {
                        Entity e = (Entity)acTrans.GetObject(acEnt.ObjectId, OpenMode.ForWrite);
                        e.Erase();
                    }
                    acTrans.Commit();
                }
                _entityTemp.Clear();
            }
            _ed.Regen();


            if (_sheetDescriptionForm.GetCoating() == 0)
            {
                len = DrawOffset(_lastSegment, _penultimateSegment, _sheetDescriptionForm.GetThickness(), _sheetDescriptionForm.GetCheckBoxDrawCoating());
            }
            else
            {
                if (_sheetDescriptionForm.GetCoating() == 2)
                {
                    len = DrawOffset(_penultimateSegment, _lastSegment, _sheetDescriptionForm.GetThickness(), _sheetDescriptionForm.GetCheckBoxDrawCoating());
                }
                else
                {
                    if (_sheetDescriptionForm.GetCoating() == 1)
                    {
                        len = DrawOffset(_lastSegment, _penultimateSegment, _sheetDescriptionForm.GetThickness(), _sheetDescriptionForm.GetCheckBoxDrawCoating());
                        len += DrawOffset(_penultimateSegment, _lastSegment, _sheetDescriptionForm.GetThickness(), _sheetDescriptionForm.GetCheckBoxDrawCoating());
                        len += 2 * _sheetDescriptionForm.GetThickness();
                    }
                }
            }

            if ((_sheetDescriptionForm.GetCoating() == 0) || (_sheetDescriptionForm.GetCoating() == 1) || (_sheetDescriptionForm.GetCoating() == 2))
            {
                Drawtext(_basePolyLine.Area, len, _sheetDescriptionForm.GetDensity() * 1000, _sheetDescriptionForm.GetMaterial());
            }
        }

        public static void RedrawCoating()
        {
            using (Transaction Tr = _db.TransactionManager.StartTransaction())
            {
                LayerTable LayerTable = Tr.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (Entity Ent in _entityTemp)
                {
                    Entity e = (Entity)Tr.GetObject(Ent.ObjectId, OpenMode.ForWrite);
                    try
                    {
                        if (_sheetDescriptionForm.GetCoatingLayer().Contains("Current") && _sheetDescriptionForm.GetCoatingLayer().Contains("Layer"))
                        {
                            throw new ArgumentNullException();
                        }
                        Ent.Layer = _sheetDescriptionForm.GetCoatingLayer();
                    }
                    catch
                    {
                        LayerTableRecord acLyrTblRec = Tr.GetObject(_db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                        Ent.Layer = acLyrTblRec.Name;

                    }
                    try
                    {
                        //MessageBox.Show("U2d Dim color =" + color.Name);
                        if (_sheetDescriptionForm.GetCoatingColor().Name.Contains("ByL"))
                        {
                            throw new ArgumentNullException();
                        }
                        Ent.Color = Color.FromColor(_sheetDescriptionForm.GetCoatingColor());
                    }
                    catch
                    {
                        // LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                        // acRotDim.Color = acLyrTblRec.Color;
                        try
                        {
                            if (_sheetDescriptionForm.GetCoatingLayer().Contains("Current") && _sheetDescriptionForm.GetCoatingLayer().Contains("Layer"))
                            {
                                throw new ArgumentNullException();
                            }
                            LayerTableRecord acLyrTblRec = Tr.GetObject(LayerTable[_sheetDescriptionForm.GetCoatingLayer()], OpenMode.ForWrite) as LayerTableRecord;
                            Ent.Color = acLyrTblRec.Color;
                        }
                        catch
                        {
                            LayerTableRecord acLyrTblRec = Tr.GetObject(_db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                            Ent.Color = acLyrTblRec.Color;
                        }

                        // acBlkTblRec.AppendEntity(t);
                        // acTrans.AddNewlyCreatedDBObject(t, true);                       
                    }

                }

                Tr.Commit();
            }
            _ed.Regen();
        }

        public static void RedrawColor()
        {
            using (Transaction acTrans = _db.TransactionManager.StartTransaction())
            {
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (Entity t in _entityTemp)
                {
                    Entity e = (Entity)acTrans.GetObject(t.ObjectId, OpenMode.ForWrite);
                    try
                    {
                        //MessageBox.Show("U2d Dim color =" + color.Name);
                        if (_sheetDescriptionForm.GetCoatingColor().Name.Contains("ByL"))
                        {
                            throw new ArgumentNullException();
                        }
                        t.Color = Color.FromColor(_sheetDescriptionForm.GetCoatingColor());
                    }
                    catch
                    {
                        // LayerTableRecord acLyrTblRec = acTrans.GetObject(Db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                        // acRotDim.Color = acLyrTblRec.Color;
                        try
                        {
                            if (_sheetDescriptionForm.GetCoatingLayer().Contains("Current") && _sheetDescriptionForm.GetCoatingLayer().Contains("Layer"))
                            {
                                throw new ArgumentNullException();
                            }
                            LayerTableRecord acLyrTblRec = acTrans.GetObject(acLyrTbl[_sheetDescriptionForm.GetCoatingLayer()], OpenMode.ForWrite) as LayerTableRecord;
                            t.Color = acLyrTblRec.Color;
                        }
                        catch
                        {
                            LayerTableRecord acLyrTblRec = acTrans.GetObject(_db.Clayer, OpenMode.ForWrite) as LayerTableRecord;
                            t.Color = acLyrTblRec.Color;
                        }

                    }
                }
                acTrans.Commit();
            }
            _ed.Regen();
        }

        public static void Clear()
        {
            if (_entityTemp.Count > 0)
            {
                using (Transaction acTrans = _db.TransactionManager.StartTransaction())
                {
                    foreach (Entity acEnt in _entityTemp)
                    {
                        Entity e = (Entity)acTrans.GetObject(acEnt.ObjectId, OpenMode.ForWrite);
                        e.Erase();
                    }

                    acTrans.Commit();
                }
                _entityTemp.Clear();
            }
            _ed.Regen();
        }
    }
}