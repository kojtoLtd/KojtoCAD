using System;
using System.Collections;
using System.Windows.Forms;
using Castle.Core.Logging;
using KojtoCAD.Mathematics.Geometry;
using KojtoCAD.Properties;
using KojtoCAD.Unfold._3d;
using KojtoCAD.Utilities;

#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Matrix3d = Autodesk.AutoCAD.Geometry.Matrix3d;
using Point2d = Autodesk.AutoCAD.Geometry.Point2d;
using Point3d = Autodesk.AutoCAD.Geometry.Point3d;
using Point3dCollection = Autodesk.AutoCAD.Geometry.Point3dCollection;
using Vector3d = Autodesk.AutoCAD.Geometry.Vector3d;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
using Matrix3d = Teigha.Geometry.Matrix3d;
using Point2d = Teigha.Geometry.Point2d;
using Point3d = Teigha.Geometry.Point3d;
using Point3dCollection = Teigha.Geometry.Point3dCollection;
using Vector3d = Teigha.Geometry.Vector3d;
#endif

[assembly: CommandClass(typeof(Unfold3D))]

namespace KojtoCAD.Unfold._3d
{
    public class Unfold3D
    {
        #region members

        //-- members ---------------------
        private readonly Document _doc;
        private readonly Database _db;
        private readonly Editor _ed;
        private double _s;
        private double _r;
        private double _kF; //K-factor
        private int _pos;

        private readonly DocumentHelper _drawingHelper =
            new DocumentHelper(Application.DocumentManager.MdiActiveDocument);

        private static ILogger _logger = NullLogger.Instance;
        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        public static ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        #endregion

        #region constructor

        //-- Constructors ---------------------
        public Unfold3D()
        {
            _doc = Application.DocumentManager.MdiActiveDocument;
            _ed = Application.DocumentManager.MdiActiveDocument.Editor;
            _db = Application.DocumentManager.MdiActiveDocument.Database;
            _s = 3;
            _r = 2;
            _kF = 0.5;
            _pos = 0;
        }

        #endregion

        #region functions

        //--- Functions -------------------------
        private void Draw(Entity obj, Transaction trans)
        {
            var blkTbl = (BlockTable) trans.GetObject(_db.BlockTableId, OpenMode.ForRead);
            var blkTblRec =
                (BlockTableRecord) trans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            blkTblRec.AppendEntity(obj);
            trans.AddNewlyCreatedDBObject(obj, true);
        }

        private void Draw(Entity obj, int ind, Transaction trans)
        {
            var blkTbl = (BlockTable) trans.GetObject(_db.BlockTableId, OpenMode.ForRead);
            var blkTblRec =
                (BlockTableRecord) trans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            blkTblRec.AppendEntity(obj);
            obj.ColorIndex = ind;
            trans.AddNewlyCreatedDBObject(obj, true);
        }

        private Point3d IdPoint(string mess, ref bool status)
        {
            var pointOptions = new PromptPointOptions(mess);
            var basePointResult = _ed.GetPoint(pointOptions);

            status = basePointResult.Status == PromptStatus.OK;
            return basePointResult.Value;
        }

        private void IdPoint(string mess, ref bool status, ref Point3d p)
        {
            var pointOptions = new PromptPointOptions(mess);
            var basePointResult = _ed.GetPoint(pointOptions);

            if (basePointResult.Status == PromptStatus.OK)
            {
                status = true;
                p = basePointResult.Value;
            }
            else
            {
                status = false;
            }
        }

        private Point3dCollection PolyToPointsArray(ref Polyline3d acPoly3D, Transaction acTrans)
        {
            var acPts3D = new Point3dCollection();
            foreach (ObjectId acObjIdVert in acPoly3D)
            {
                var acPolVer3D = (PolylineVertex3d) acTrans.GetObject(acObjIdVert, OpenMode.ForRead);

                acPts3D.Add(acPolVer3D.Position);
            }
            return acPts3D;
        }

        private Point3dCollection PolyToPointsArray(ref Polyline acPoly2D)
        {
            var acPts2D = new Point3dCollection();
            for (var nCnt = 0; nCnt < acPoly2D.NumberOfVertices; nCnt++)
            {
                acPts2D.Add(acPoly2D.GetPoint3dAt(nCnt));
            }
            return acPts2D;
        }

        private ObjectIdCollection GetSelection(ref TypedValue[] typValArr, string mess)
        {
            var opt = new PromptSelectionOptions {MessageForAdding = mess};
            PromptSelectionResult ssPrompt;
            var coll = new ObjectIdCollection();

            // Assign the filter criteria to a SelectionFilter object
            var acSelFtr = new SelectionFilter(typValArr);

            do
            {
                ssPrompt = _ed.GetSelection(opt, acSelFtr);
                if (ssPrompt.Status == PromptStatus.OK)
                {
                    var sSet = ssPrompt.Value;
                    if (sSet.Count > 0)
                    {
                        if (coll.Count > 0)
                        {
                            foreach (var obj in sSet.GetObjectIds())
                            {
                                // Add each object id to the ObjectIdCollection  
                                coll.Add(obj);
                                using (var acTrans = _db.TransactionManager.StartTransaction())
                                {
                                    var ent = (Entity) acTrans.GetObject(obj, OpenMode.ForWrite);
                                    ent.Visible = false;
                                    acTrans.Commit();
                                }
                            }
                        }
                        else
                        {
                            coll = new ObjectIdCollection(sSet.GetObjectIds());
                            foreach (var obj in sSet.GetObjectIds())
                            {
                                using (var acTrans = _db.TransactionManager.StartTransaction())
                                {
                                    var ent = (Entity) acTrans.GetObject(obj, OpenMode.ForWrite);
                                    ent.Visible = false;
                                    acTrans.Commit();
                                }
                            }
                        }
                    }
                }
            } while (ssPrompt.Status == PromptStatus.OK);
            if (coll.Count > 0)
            {
                foreach (ObjectId obj in coll)
                {
                    using (var acTrans = _db.TransactionManager.StartTransaction())
                    {
                        var ent = (Entity) acTrans.GetObject(obj, OpenMode.ForWrite);
                        ent.Visible = true;
                        acTrans.Commit();
                    }
                }
            }
            return coll;
        }

        private void GetSelection(ref TypedValue[] typValArr, ref ObjectIdCollection coll)
        {
            SelectionSet sSet;
            PromptSelectionResult ssPrompt;

            // Assign the filter criteria to a SelectionFilter object
            var acSelFtr = new SelectionFilter(typValArr);

            do
            {
                //Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("1");
                ssPrompt = _ed.GetSelection(acSelFtr);
                if (ssPrompt.Status == PromptStatus.OK)
                {
                    sSet = ssPrompt.Value;
                    if (sSet.Count > 0)
                    {
                        if (coll.Count > 0)
                        {
                            foreach (var obj in sSet.GetObjectIds())
                            {
                                // Add each object id to the ObjectIdCollection  
                                coll.Add(obj);
                                using (var acTrans = _db.TransactionManager.StartTransaction())
                                {
                                    var ent = (Entity) acTrans.GetObject(obj, OpenMode.ForWrite);
                                    ent.Visible = false;
                                    acTrans.Commit();
                                }
                            }
                        }
                        else
                        {
                            coll = new ObjectIdCollection(sSet.GetObjectIds());
                            foreach (var obj in sSet.GetObjectIds())
                            {
                                using (var acTrans = _db.TransactionManager.StartTransaction())
                                {
                                    var ent = (Entity) acTrans.GetObject(obj, OpenMode.ForWrite);
                                    ent.Visible = false;
                                    acTrans.Commit();
                                }
                            }
                        }
                    }
                }
            } while (ssPrompt.Status == PromptStatus.OK);
            if (coll.Count > 0)
            {
                foreach (ObjectId obj in coll)
                {
                    using (var acTrans = _db.TransactionManager.StartTransaction())
                    {
                        var ent = (Entity) acTrans.GetObject(obj, OpenMode.ForWrite);
                        ent.Visible = true;
                        acTrans.Commit();
                    }
                }
            }
        }

        private Polyline3d GetSelection_One3dPoly(string message, Transaction trans)
        {
            var opt = new PromptSelectionOptions {MessageForAdding = message};
            PromptSelectionResult ssPrompt;

            var acPoly3D = new Polyline3d();
            var typValArr = new TypedValue[1];
            typValArr.SetValue(new TypedValue((int) DxfCode.Start, "POLYLINE"), 0);

            // Assign the filter criteria to a SelectionFilter object
            var acSelFtr = new SelectionFilter(typValArr);

            do
            {
                //Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("1");
                ssPrompt = _ed.GetSelection(opt, acSelFtr);
                if (ssPrompt.Status == PromptStatus.OK)
                {
                    var sSet = ssPrompt.Value;
                    if (sSet.Count > 0)
                    {
                        var arr = sSet.GetObjectIds();
                        acPoly3D = (Polyline3d) trans.GetObject(arr[0], OpenMode.ForWrite);
                        break;
                    }
                }
            } while (ssPrompt.Status == PromptStatus.OK);
            return acPoly3D;
        }

        private int IndexOf(ref ArrayList points, ref Quaternion q)
        {
            var rez = -1;
            Quaternion Q;
            for (var i = 0; i < points.Count; i++)
            {
                Q = new Quaternion((Quaternion) points[i]);
                if ((Q - q).abs() < Constants.zero_dist)
                {
                    rez = i;
                    break;
                }
            }
            return rez;
        }

        private int IsLine(ref Quaternion q1, ref Quaternion q2, ref Quaternion q3)
        {
            var rez = -1;
            var q = new Quaternion(q1 - q2);
            var qq = new Quaternion(q1 - q3);
            var test = q/qq;
            test.q[0] = 0;
            if (test.norm() <= Constants.zero_dist)
            {
                rez = 0;

            }
            return rez;
        }

        private int IsLine(ref Quaternion q1, ref Quaternion q2, ref ArrayList sLinePoints)
        {
            var rez = -1;
            var q = new Quaternion(q1 - q2);
            for (var i = 0; i < sLinePoints.Count; i++)
            {
                var qq = new Quaternion(q1 - (Quaternion) sLinePoints[i]);
                var test = q/qq;
                test.q[0] = 0;
                if (test.norm() <= Constants.zero_dist)
                {
                    rez = i;
                    break;
                }
            }
            return rez;
        }

        private ArrayList GetPointsBySector(ref Quaternion currQ, int i, ref ArrayList midLinesArray,
            ref ArrayList firstLinePoints, ref ArrayList secondLinePoints)
        {
            var temArray = new ArrayList();
            var midLine = (Quaternion[]) midLinesArray[i];
            var tpp = temArray.Count > 0 ? (Quaternion) temArray[0] : currQ;
            var tp = IsLine(ref midLine[0], ref tpp, ref secondLinePoints);
            var t = IsLine(ref midLine[1], ref currQ, ref secondLinePoints);
            // MessageBox.Show(tp.ToString() + "\n" + t.ToString());
            var dt = Math.Abs(tp - t);
            if ((dt > 1) && (t >= 0))
            {
                var ucs = new UCS(midLine[0], currQ, midLine[1]);
                var t1 = tp < t ? tp : t;
                var t2 = tp > t ? tp : t;

                for (var ii = t1; ii <= t2; ii++)
                {
                    var qv = new Quaternion(ucs.FromACS((Quaternion) secondLinePoints[ii]));
                    if (Math.Abs(qv.GetZ()) > Constants.zero_dist)
                    {
                        t = -1;
                        break;
                    }
                }
            }
            temArray.Add(currQ);
            var index = IndexOf(ref firstLinePoints, ref currQ);
            if (index < firstLinePoints.Count - 1)
            {
                var dq = new Quaternion((Quaternion) firstLinePoints[index + 1]);
                var index1 = IsLine(ref midLine[1], ref currQ, ref dq);

                if (index1 == 0)
                {
                    t = -1;
                } //фалшив триъгълник            
            }
            if (t < 0) //не е връх на триъгълник
            {
                for (var j = IndexOf(ref firstLinePoints, ref currQ) + 1; j < firstLinePoints.Count; j++)
                {
                    var curr = new Quaternion((Quaternion) firstLinePoints[j]);
                    temArray.Add(curr);
                    var tt = IsLine(ref midLine[1], ref curr, ref secondLinePoints);
                    if (tt >= 0)
                    {
                        currQ = curr;
                        break;
                    }
                }
            }
            if (temArray.Count > 2)
            {
                var test1 = (Quaternion) temArray[0];
                var test2 = (Quaternion) temArray[1];
                var ttt = IsLine(ref midLine[0], ref test1, ref test2);
                if (ttt >= 0)
                {
                    temArray.RemoveAt(0);
                }
            }

            return temArray;

            /*          ArrayList tempArray = new ArrayList();
                      Quaternion[] midLine = (Quaternion[])midLinesArray[i];
                      tempArray.Add(currQ);
                      if(i<(midLinesArray.Count-1))
                      {
                          Quaternion[] midLineNext = (Quaternion[])midLinesArray[i+1];
                          int t1 = IsLine(ref midLine[1], ref currQ, ref secondLinePoints);
                          int t2 = IsLine(ref midLineNext[1], ref currQ, ref secondLinePoints);
                          if (Math.Abs(t1 - t2) == 1)
                          {
                              return tempArray;
                          }
                      }

                      UCS ucs = new UCS(midLine[0], currQ, midLine[1]);
                      int index = IndexOf(ref firstLinePoints, ref currQ);
                      for (int j = index + 1; j < firstLinePoints.Count; j++)
                      {
                          Quaternion q = new Quaternion((Quaternion)firstLinePoints[j]);
                          Quaternion q1 = new Quaternion(q);
                          q = ucs.FromACS(q);
                          if (Math.Abs(q.GetZ()) > Constants.zero_dist)
                          {
                              index = j - 1;
                              break;
                          }
                          else
                          {
                              tempArray.Add(q1);
                          }
                      }
                      currQ = (Quaternion)tempArray[tempArray.Count - 1];

                      if (tempArray.Count > 1)
                      {
                          Quaternion q2 = (Quaternion)tempArray[tempArray.Count - 2];
                          int t = IsLine(ref midLine[1], ref currQ, ref secondLinePoints);
                          int t1 = IsLine(ref midLine[1], ref q2, ref secondLinePoints);
                          if (t == t1)
                          {                  
                              tempArray.RemoveAt(tempArray.Count - 1);
                              currQ = (Quaternion)tempArray[tempArray.Count - 1];
                          }
                      }

                      return tempArray;
             */
        }

        private int GetInputData(ref Point3dCollection m, ref Point3dCollection f, ref Point3dCollection s)
        {
            using (var trans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                var midLine = GetSelection_One3dPoly("Select Mid Line", trans);
                m = PolyToPointsArray(ref midLine, trans);
                if (m.Count < 3)
                {
                    Application.ShowAlertDialog("Error\nMidLinePoints.Count = ?\n(there must be more than two points)");
                    return -1;
                }

                var firstLine = GetSelection_One3dPoly("Select First Line", trans);
                f = PolyToPointsArray(ref firstLine, trans);
                var statusP = false;
                if (f.Count < 1)
                {
                    Application.ShowAlertDialog("Error\nFirstLinePoints.Count = ?\n(must have at least one point)");
                    var point = IdPoint("Select Point", ref statusP);
                    if (statusP)
                    {
                        f.Add(point);
                    }
                    else
                    {
                        return -2;
                    }
                }

                var secondLine = GetSelection_One3dPoly("Select Second Line", trans);
                s = PolyToPointsArray(ref secondLine, trans);
                if (s.Count < 1)
                {
                    if (statusP)
                    {
                        Application.ShowAlertDialog(
                            "Error\nSecondLinePoints.Count = ?\n(at least one polyline must have more than one point)");
                        return -3;
                    }
                    Application.ShowAlertDialog("Error\nSecondLinePoints.Count = ?\n(must have at least one point)");
                    var point = IdPoint("Select Point", ref statusP);
                    if (statusP)
                    {
                        s.Add(point);
                    }
                    else
                    {
                        return -4;
                    }
                }
                //if ((s.Count < m.Count) && (f.Count < m.Count))
                //{
                //    Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(
                //        "Error\nSecondLinePoints.Count = ?\n(at least one polyline must have count points  equal or greater than the middle polyline)");
                //    return -5;
                //}}
                trans.Commit();
                return 0;
            }
        }

        private ArrayList MakeSegmentsArrays(ref ArrayList midLinesArray, ref ArrayList firstLinePoints,
            ref ArrayList secondLinePoints)
        {
            var currQ = new Quaternion((Quaternion) firstLinePoints[0]);
            var currQs = new Quaternion((Quaternion) secondLinePoints[0]);
            var allSegmentPoints = new ArrayList();
            for (var i = 0; i < midLinesArray.Count; i++)
            {
                var tempArray1 = GetPointsBySector(ref currQ, i, ref midLinesArray, ref firstLinePoints,
                    ref secondLinePoints);
                var tempArray2 = GetPointsBySector(ref currQs, i, ref midLinesArray, ref secondLinePoints,
                    ref firstLinePoints);

                ArrayList[] segmentPoints = {tempArray1, tempArray2};
                allSegmentPoints.Add(segmentPoints);

            }
            return allSegmentPoints;
        }

        private void AddDim(Complex c1, Complex c2, double k)
        {
            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                var acBlkTbl = (BlockTable) acTrans.GetObject(_db.BlockTableId,
                    OpenMode.ForRead);

                // Open the Block table record Model space for write
                var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForWrite);

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
                acRotDim.DimensionStyle = _db.Dimstyle;
                acRotDim.Layer = "DIM";

                // Add the new object to Model space and the transaction
                acBlkTblRec.AppendEntity(acRotDim);
                acTrans.AddNewlyCreatedDBObject(acRotDim, true);

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }

        }

        #endregion

        /// <summary>
        /// Unfold 3D Sheet Profile
        /// </summary>
        [CommandMethod("unfold3d")]
        public void Unfold3DSheetProfileStart()
        {
            
            //подсигуряваме LAYER за оразмеряване
            _drawingHelper.LayerManipulator.CreateLayer("DIM", System.Drawing.Color.Red);

            #region input_data_form

            var form = new Unfold3dForm(_s, _r, _kF, _pos);
            if (form.ShowDialog() != DialogResult.OK)
            {
                form.Dispose();
                return;
            }
            else
            {
                _s = form.m_S;
                _r = form.m_R;
                _kF = form.m_kF;
                _pos = form.pos;
            }

            #endregion

            #region prepare

            Point3dCollection midPointsColl = null;
            Point3dCollection firstPointsColl = null;
            Point3dCollection secondPointsColl = null;

            //получаваме входните данни
            var rez = GetInputData(ref midPointsColl, ref firstPointsColl, ref secondPointsColl);
            if (rez < 0)
            {
                return;
            }

            //конвертираме данните
            var midLinePoints = new ArrayList();
            var firstLinePoints = new ArrayList();
            var secondLinePoints = new ArrayList();

            foreach (Point3d p in midPointsColl)
            {
                midLinePoints.Add(new Quaternion(0.0, p.X, p.Y, p.Z));
            }

            //Уточняваме положението на материала
            var len = ((Quaternion) midLinePoints[0] - (Quaternion) midLinePoints[1]).abs();
            var len1 = ((Quaternion) midLinePoints[2] - (Quaternion) midLinePoints[1]).abs();
            len = (len + len1)*3.0/8.0;
            var uccs = new UCS((Quaternion) midLinePoints[0],
                new Quaternion(0, firstPointsColl[0].X, firstPointsColl[0].Y, firstPointsColl[0].Z),
                (Quaternion) midLinePoints[1]);
            var qp1 = new Quaternion(0, 0, 0, 0);
            var qp2 = new Quaternion(0, 0, 0, len);
            var qp3 = new Quaternion(0, len/15.0, 0, len - len/3.0);
            var qp4 = new Quaternion(0, -len/15.0, 0, len - len/3.0);
            var qp5 = new Quaternion(0, 0, len/15.0, len - len/3.0);
            var qp6 = new Quaternion(0, 0, -len/15.0, len - len/3.0);
            qp1 = uccs.ToACS(qp1);
            qp2 = uccs.ToACS(qp2);
            qp3 = uccs.ToACS(qp3);
            qp4 = uccs.ToACS(qp4);
            qp5 = uccs.ToACS(qp5);
            qp6 = uccs.ToACS(qp6);


            using (var trans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                var aLine = new Line(new Point3d(qp1.GetX(), qp1.GetY(), qp1.GetZ()),
                    new Point3d(qp2.GetX(), qp2.GetY(), qp2.GetZ()));
                var aLine1 = new Line(new Point3d(qp2.GetX(), qp2.GetY(), qp2.GetZ()),
                    new Point3d(qp3.GetX(), qp3.GetY(), qp3.GetZ()));
                var aLine2 = new Line(new Point3d(qp2.GetX(), qp2.GetY(), qp2.GetZ()),
                    new Point3d(qp4.GetX(), qp4.GetY(), qp4.GetZ()));
                var aLine3 = new Line(new Point3d(qp2.GetX(), qp2.GetY(), qp2.GetZ()),
                    new Point3d(qp5.GetX(), qp5.GetY(), qp5.GetZ()));
                var aLine4 = new Line(new Point3d(qp2.GetX(), qp2.GetY(), qp2.GetZ()),
                    new Point3d(qp6.GetX(), qp6.GetY(), qp6.GetZ()));
                aLine.SetDatabaseDefaults();
                Draw(aLine, 2, trans);
                aLine1.SetDatabaseDefaults();
                Draw(aLine1, 2, trans);
                aLine2.SetDatabaseDefaults();
                Draw(aLine2, 2, trans);
                aLine3.SetDatabaseDefaults();
                Draw(aLine3, 2, trans);
                aLine4.SetDatabaseDefaults();
                Draw(aLine4, 2, trans);



                _ed.UpdateScreen();
                if (MessageBox.Show(
                    "Is this the right side of the material: \n\n If direction is correct click YES \n\n otherwise click NO",
                    "Side of the the Material",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (Point3d p in firstPointsColl)
                    {
                        firstLinePoints.Add(new Quaternion(0.0, p.X, p.Y, p.Z));
                    }
                    foreach (Point3d p in secondPointsColl)
                    {
                        secondLinePoints.Add(new Quaternion(0.0, p.X, p.Y, p.Z));
                    }

                }
                else //материала е към отрицателната посока на Z; - сменяме подреждане first-second
                {
                    foreach (Point3d p in firstPointsColl)
                    {
                        secondLinePoints.Add(new Quaternion(0.0, p.X, p.Y, p.Z));
                    }
                    foreach (Point3d p in secondPointsColl)
                    {
                        firstLinePoints.Add(new Quaternion(0.0, p.X, p.Y, p.Z));
                    }
                }

                var id = aLine.Id;
                var ent = (Entity) id.GetObject(OpenMode.ForWrite);
                ent.Erase();

                id = aLine1.Id;
                ent = (Entity) id.GetObject(OpenMode.ForWrite);
                ent.Erase();

                id = aLine2.Id;
                ent = (Entity) id.GetObject(OpenMode.ForWrite);
                ent.Erase();

                id = aLine3.Id;
                ent = (Entity) id.GetObject(OpenMode.ForWrite);
                ent.Erase();

                id = aLine4.Id;
                ent = (Entity) id.GetObject(OpenMode.ForWrite);
                ent.Erase();

                trans.Commit();
            }

            //селектиране на отворите

            var typValArr = new TypedValue[5];
            typValArr.SetValue(new TypedValue((int) DxfCode.Operator, "<or"), 0);
            typValArr.SetValue(new TypedValue((int) DxfCode.Start, "POLYLINE"), 1);
            typValArr.SetValue(new TypedValue((int) DxfCode.Start, "LWPOLYLINE"), 2);
            typValArr.SetValue(new TypedValue((int) DxfCode.Start, "CIRCLE"), 3);
            typValArr.SetValue(new TypedValue((int) DxfCode.Operator, "or>"), 4);
            var holes = GetSelection(ref typValArr,
                "Select the contours of the holes or opened polyLines (Circles, 2d and 3d PolyLines) or press space to continue.");

            var holesCircles = new ArrayList();
                //масив от Quaternions с отвори окръжности - real=R,   X,Y,Z - center
            var holesPLines = new ArrayList();
                //масив от масиви  с полилинии 3D (може и да неса отвори )- точките са кватерниони
            var holesLwLines = new ArrayList();
                //масив от масиви  с полилинии 2D (може и да неса отвори )- точките са кватерниони
            foreach (ObjectId id in holes)
            {
                using (var acTrans = _db.TransactionManager.StartTransaction())
                {
                    var ent = (Entity) id.GetObject(OpenMode.ForWrite);
                    //MessageBox.Show(ent.GetRXClass().DxfName);
                    if (ent.GetRXClass().DxfName == "CIRCLE")
                    {
                        var ob = (Circle) id.GetObject(OpenMode.ForWrite);
                        var center = new Quaternion(ob.Radius, ob.Center.X, ob.Center.Y, ob.Center.Z);
                        holesCircles.Add(center);
                    }
                    else
                    {
                        if (ent.GetRXClass().DxfName == "POLYLINE")
                        {
                            var ob = (Polyline3d) id.GetObject(OpenMode.ForWrite);
                            var line = PolyToPointsArray(ref ob, acTrans);
                            var Line = new ArrayList();
                            foreach (Point3d p in line)
                            {
                                Line.Add(new Quaternion(0, p.X, p.Y, p.Z));
                            }
                            holesPLines.Add(Line);
                        }
                        else
                        {
                            if (ent.GetRXClass().DxfName == "LWPOLYLINE")
                            {
                                var ob = (Polyline) id.GetObject(OpenMode.ForWrite);
                                var line = PolyToPointsArray(ref ob);
                                var Line = new ArrayList();
                                foreach (Point3d p in line)
                                {
                                    Line.Add(new Quaternion(ob.GetBulgeAt(line.IndexOf(p)), p.X, p.Y, p.Z));
                                }
                                Line.Add(new Quaternion(0, ob.Normal.X, ob.Normal.Y, ob.Normal.Z));
                                holesLwLines.Add(Line);
                            }
                        }
                    }
                    acTrans.Commit();
                }

            }

            //избор на стартова точка
            var status = false;
            var startPoint = IdPoint("Pick a Start Point:", ref status);
            if (status == false)
            {
                Application.ShowAlertDialog("User Cancel !\n\n Start Point Ecape");
                return;
            }
            var start = new Complex(startPoint.X, startPoint.Y);

            #endregion

            #region calc

            //правиме масив от последователните отесечки на стредната линия
            var midLinesArray = new ArrayList();
            for (var i = 1; i < midLinePoints.Count; i++)
            {
                Quaternion[] qarr = {(Quaternion) midLinePoints[i - 1], (Quaternion) midLinePoints[i]};
                midLinesArray.Add(qarr);
            }

            //---- масив - всеки от елемените от тип ArrayLisт[] с дължина 2 ArrayLisт[0]съдържа съответстващите
            //на сегмента от средната линия (midLinesArray[i]) точки от firstLine ;ArrayLisт[1]-от secondline
            //масивите  midLinesArray,allSegmentPoints са с еднаква дължина и съотвтни индекси
            var allSegmentPoints = MakeSegmentsArrays(ref midLinesArray, ref firstLinePoints, ref secondLinePoints);

            //за всеки сегмент от средната линия се определя координатана система
            //начало първата точка, ос X - първа точка на сегмента-първа точка от масива точки съответстващ на този сегмент и firstLine
            //положителната посока на Y е към края на сегмента
            //дължината е = midLinesArray, индексите са аъответни
            //материала е винаги от положителната страна на ос Z
            var ucsArray = new ArrayList();
            for (var i = 0; i < midLinesArray.Count; i++)
            {
                var arr = (ArrayList[]) allSegmentPoints[i];
                var segment = (Quaternion[]) midLinesArray[i];
                var c = (Quaternion) arr[0][0];
                var ucs = new UCS(segment[0], c, segment[1]);
                ucsArray.Add(ucs);
            }

            var foldigdirectionMarks = new ArrayList {0}; //0-horizontalno; 1 - top ; - 1 down
            for (var i = 1; i < ucsArray.Count; i++)
            {
                var ucsI1 = (UCS) ucsArray[i - 1];
                var ucsI = (UCS) ucsArray[i];
                var c = ucsI1.FromACS(ucsI.ToACS(new Quaternion(0.0, 0.0, 0.0, 1000.0)));
                if (c.GetZ() == 0.0)
                {
                    foldigdirectionMarks.Add(0);
                }
                else
                {
                    if (c.GetZ() < 0.0)
                    {
                        foldigdirectionMarks.Add(-1);
                    }
                    else
                    {
                        foldigdirectionMarks.Add(1);
                    }
                }
            }


            //Намираме ъглите между плоскостите за всеки BEND
            //ако центъра на закрълението и полилинията са от една и съща страна на материала
            //ъгъла се приема за отрицателен
            var bendsAnglesArray = new ArrayList {0.0};
            for (var i = 0; i < ucsArray.Count - 1; i++)
            {
                var ucs1 = new UCS((UCS) ucsArray[i]);
                var ucs2 = new UCS((UCS) ucsArray[i + 1]);
                var arr = (Quaternion[]) midLinesArray[i + 1];
                //ако края на сегмента от средната линия принадлежащ на слеващата координатна система 
                //има координата Z в текущата координатна система > 0 то тъи ка то материала е в положителната
                //посока на Z то полилинията сгъва материала е в полжителна посока ( към материала) и 
                // полилинията и центъра на закръглението са от различни страни на материала т.е ъгъла е положителен
                var ang = ucs1.FromACS(arr[1]).GetZ() >= 0
                    ? UCS.AngleBetweenZaxes(ucs1, ucs2)
                    : -UCS.AngleBetweenZaxes(ucs1, ucs2);
                bendsAnglesArray.Add(ang);
            }
            bendsAnglesArray.Add(0.0);

            //определяме K-Factora или го взимаме от формата
            var kf = _kF; //за сега го взимаме от формата
            //Намираме дължините на дъгите (1/2) за всеки BEND
            var arcArray = new ArrayList {0.0};
            for (var i = 1; i < bendsAnglesArray.Count - 1; i++)
            {
                if (_pos == 1) //Bend Allowance
                {
                    arcArray.Add(kf/2.0);
                }
                else
                {
                    var l = Math.Abs((double) bendsAnglesArray[i])*(_r + kf*_s); // L = fi*(R+KF*S)
                    l /= 2; // делим по равно 1/2 за съседните участъци
                    arcArray.Add(l);
                }
            }
            arcArray.Add(0.0);

            //определяме разстоянието от зададения BEND до успоредната линия "начало на сгъвката" 
            //или линията граница на "равнинната чст"
            // L = (R+S)*tan(f1/2) - ако центъра на закръглението и полилинията са от различни страни на материала
            // L = R*tan(f1/2) - ако центъра на закръглението и полилинията са от една и съща страна на материала
            var removingLenArray = new ArrayList {0.0};
            for (var i = 1; i < bendsAnglesArray.Count - 1; i++)
            {
                var l = (double) bendsAnglesArray[i] >= 0
                    ? (_s + _r)*Math.Tan((double) bendsAnglesArray[i]/2)
                    : Math.Abs(_r*Math.Tan((double) bendsAnglesArray[i]/2));
                if (Math.Abs(Math.Abs((double) bendsAnglesArray[i])*180.0/Math.PI - 180.0) <= Constants.zero_dist)
                {
                    l = _r;
                }
                if ((((Quaternion) midLinePoints[i] - (Quaternion) midLinePoints[i + 1]).abs() <= l) ||
                    (((Quaternion) midLinePoints[i] - (Quaternion) midLinePoints[i - 1]).abs() <= l))
                {
                    l = 0.0;
                }

                removingLenArray.Add(l);
            }
            removingLenArray.Add(0.0);

            var coincidence = new ArrayList {new[] {true, true}};
            for (var i = 0; i < allSegmentPoints.Count - 1; i++)
            {
                ArrayList[] points = {((ArrayList[]) allSegmentPoints[i])[0], ((ArrayList[]) allSegmentPoints[i])[1]};
                ArrayList[] pointsN =
                {
                    ((ArrayList[]) allSegmentPoints[i + 1])[0],
                    ((ArrayList[]) allSegmentPoints[i + 1])[1]
                };
                var firstSidePoints = points[0]; // точките от пъравта полилиния за този учатък
                var secondSidePoints = points[1]; // точките от втората полилиния за този учатък
                var firstSidePointsN = pointsN[0]; // точките от пъравта полилиния за следващия учатък
                var secondSidePointsN = pointsN[1];
                    // точките от втората полилиния за следващия учатък
                var distf =
                    ((Quaternion) firstSidePoints[firstSidePoints.Count - 1] - (Quaternion) firstSidePointsN[0]).abs();
                var dists =
                    ((Quaternion) secondSidePoints[secondSidePoints.Count - 1] - (Quaternion) secondSidePointsN[0]).abs();
                coincidence.Add(new[] {distf <= Constants.zero_dist, dists <= Constants.zero_dist});
            }
            coincidence.Add(new[] {true, true});
            //---------------------------------------------------------------------------------------------------------------------
            var ucss_2D = new ArrayList();
            var allSegmentPointsComplex = new ArrayList();
            var segments2Ducs = new ArrayList(); //двойките pre,next координатни системи
            var midLineSegmentsComplex = new ArrayList(); //сегментите от средната линия
            var linesI = new ArrayList(); //линиите граници на равнинния сегмент
            var ucss = new UCS2d();
            for (var i = 0; i < allSegmentPoints.Count; i++)
            {
                var ucs = new UCS((UCS) ucsArray[i]); //координатната система на участъка
                Quaternion[] midLine =
                {
                    new Quaternion(ucs.FromACS(((Quaternion[]) midLinesArray[i])[0])),
                    new Quaternion(ucs.FromACS(((Quaternion[]) midLinesArray[i])[1]))
                };
                var midLineStart = new Quaternion(midLine[0]);
                    //стартова точка на сегмента от средната линия
                var midLineEnd = new Quaternion(midLine[1]);
                    //крайна точка на сегмента от средната линия
                ArrayList[] points = {((ArrayList[]) allSegmentPoints[i])[0], ((ArrayList[]) allSegmentPoints[i])[1]};
                var firstSidePoints = points[0]; // точките от пъравта полилиния за този учатък
                var secondSidePoints = points[1]; // точките от втората полилиния за този учатък
                //конвертираме към равнина
                var midLineStartComplex = ucss.ToACS(new Complex(midLineStart.GetX(), midLineStart.GetY()));
                var midLineEndComplex = ucss.ToACS(new Complex(midLineEnd.GetX(), midLineEnd.GetY()));
                var firstSideComplex = new ArrayList();
                var secondSideComplex = new ArrayList();

                for (var j = 0; j < firstSidePoints.Count; j++)
                {
                    var q = new Quaternion(ucs.FromACS((Quaternion) firstSidePoints[j]));
                    firstSideComplex.Add(new Complex(ucss.ToACS(new Complex(q.GetX(), q.GetY()))));
                }
                for (var j = 0; j < secondSidePoints.Count; j++)
                {
                    var q = new Quaternion(ucs.FromACS((Quaternion) secondSidePoints[j]));
                    secondSideComplex.Add(new Complex(ucss.ToACS(new Complex(q.GetX(), q.GetY()))));
                }

                //дефинираме линиите съответстващи на бендовете от входните данни
                var pLine = new KLine2D((Complex) secondSideComplex[0], (Complex) firstSideComplex[0]);
                var nLine = new KLine2D((Complex) secondSideComplex[secondSideComplex.Count - 1],
                    (Complex) firstSideComplex[firstSideComplex.Count - 1]);

                //дефинираме линиите граници на равнинните части 
                var pLineI = new KLine2D(pLine, (double) removingLenArray[i]);
                var nLineI = new KLine2D(nLine, -(double) removingLenArray[i + 1]);

                //запазваме текущата UCS
                var currUcs = new UCS2d(ucss);
                ucss_2D.Add(ucss);

                //дефинираме следващата 2D координатна система
                ucss = new UCS2d(midLineEndComplex,
                    ((Complex) firstSideComplex[firstSideComplex.Count - 1] - midLineEndComplex).arg());

                //сменяваме краините точки на участъците от стараната на firstLine             
                if (firstSideComplex.Count == 1) //тригълен участък с връх в firstLine
                {
                    firstSideComplex[0] = pLineI.IntersectWitch(nLineI);
                }
                else
                {
                    //посока pre->next търсим първата линия неуспоредна на бедраото "pre"
                    var n = -1;
                    for (var j = 0; j < firstSideComplex.Count - 1; j++)
                    {
                        var line = new KLine2D((Complex) firstSideComplex[j], (Complex) firstSideComplex[j + 1]);
                        if (KLine2D.IsParalel(pLine, line) == false)
                        {
                            n = j;
                            firstSideComplex[j] = pLineI.IntersectWitch(line);
                            for (var jj = 0; jj < j; jj++)
                            {
                                firstSideComplex.RemoveAt(0);
                            }
                            break;
                        }
                    }
                    if (n < 0) //мним триъгълник
                    {
                        var c = pLineI.IntersectWitch(nLineI);
                        firstSideComplex.Clear();
                        firstSideComplex.Add(c);
                    }
                    //посока next->pre търсим първата линия неуспоредна на бедраото "next"    
                    n = -1;
                    for (var j = firstSideComplex.Count - 1; j > 0; j--)
                    {
                        var line = new KLine2D((Complex) firstSideComplex[j], (Complex) firstSideComplex[j - 1]);
                        if (KLine2D.IsParalel(nLine, line) == false)
                        {
                            n = j;
                            firstSideComplex[j] = nLineI.IntersectWitch(line);
                            for (var jj = firstSideComplex.Count - 1; jj > j; jj--)
                            {
                                firstSideComplex.RemoveAt(firstSideComplex.Count - 1);
                            }
                            break;
                        }
                    }
                    if (n < 0) //мним триъгълник
                    {
                        var c = pLineI.IntersectWitch(nLineI);
                        firstSideComplex.Clear();
                        firstSideComplex.Add(c);
                    }
                    //при къси страни може да се получи грешка
                    if (firstSideComplex.Count > 1)
                    {
                        var t = currUcs.ToACS(new Complex(0, 0));
                        var c = (Complex) firstSideComplex[firstSideComplex.Count - 1];
                        c -= (Complex) firstSideComplex[0];
                        c += t;
                        c = currUcs.FromACS(c);
                        if (c.imag() < 0)
                        {
                            var cc = pLineI.IntersectWitch(nLineI);
                            firstSideComplex.Clear();
                            firstSideComplex.Add(cc);
                        }
                    }
                }

                //сменяваме краините точки на участъците от стараната на secondLine 
                //------------------------------------------------------------------------------------------------------------
                if (secondSideComplex.Count == 1) //тригълен участък с връх в secondLine
                {
                    secondSideComplex[0] = pLineI.IntersectWitch(nLineI);
                }
                else
                {
                    //посока pre->next търсим първата линия неуспоредна на бедраото "pre"
                    var n = -1;
                    for (var j = 0; j < secondSideComplex.Count - 1; j++)
                    {
                        var line = new KLine2D((Complex) secondSideComplex[j], (Complex) secondSideComplex[j + 1]);
                        if (KLine2D.IsParalel(pLine, line) == false)
                        {
                            n = j;
                            secondSideComplex[j] = pLineI.IntersectWitch(line);
                            for (var jj = 0; jj < j; jj++)
                            {
                                secondSideComplex.RemoveAt(0);
                            }
                            break;
                        }
                    }
                    if (n < 0) //мним триъгълник
                    {
                        var c = pLineI.IntersectWitch(nLineI);
                        secondSideComplex.Clear();
                        secondSideComplex.Add(c);
                    }
                    //посока next->pre търсим първата линия неуспоредна на бедраото "next"       
                    n = -1;
                    for (var j = secondSideComplex.Count - 1; j > 0; j--)
                    {
                        var line = new KLine2D((Complex) secondSideComplex[j], (Complex) secondSideComplex[j - 1]);
                        if (KLine2D.IsParalel(nLine, line) == false)
                        {
                            n = j;
                            secondSideComplex[j] = nLineI.IntersectWitch(line);
                            for (var jj = secondSideComplex.Count - 1; jj > j; jj--)
                            {
                                secondSideComplex.RemoveAt(secondSideComplex.Count - 1);
                            }
                            break;
                        }
                    }
                    if (n < 0) //мним триъгълник
                    {
                        var c = pLineI.IntersectWitch(nLineI);
                        secondSideComplex.Clear();
                        secondSideComplex.Add(c);
                    }
                    if (secondSideComplex.Count > 1)
                    {
                        var t = currUcs.ToACS(new Complex(0, 0));
                        var c = (Complex) secondSideComplex[secondSideComplex.Count - 1];
                        c -= (Complex) secondSideComplex[0];
                        c += t;
                        c = currUcs.FromACS(c);
                        if (c.imag() < 0)
                        {
                            var cc = pLineI.IntersectWitch(nLineI);
                            secondSideComplex.Clear();
                            secondSideComplex.Add(cc);
                        }
                    }
                }
                //-------------------------------------------------------------------------
                ArrayList[] pointsComplex = {firstSideComplex, secondSideComplex};
                UCS2d[] ucssArr = {currUcs, ucss};
                Complex[] midLineComplex = {midLineStartComplex, midLineEndComplex};
                KLine2D[] linesarr = {pLineI, nLineI};

                allSegmentPointsComplex.Add(pointsComplex);
                segments2Ducs.Add(ucssArr);
                midLineSegmentsComplex.Add(midLineComplex);
                linesI.Add(linesarr);
            }
            var acLine = new Line();
            var bendConturPoints = new ArrayList();
            var allSegmentPointsComplex1 = new ArrayList();
            for (var i = 0; i < allSegmentPointsComplex.Count; i++)
            {
                var currUcs = new UCS2d(((UCS2d[]) segments2Ducs[i])[0]);
                ucss = new UCS2d(((UCS2d[]) segments2Ducs[i])[1]);
                //var pLineI = ((Line2d[]) linesI[i])[0];
                //var nLineI = ((Line2d[]) linesI[i])[1];
                var midLineStart = new Complex(((Complex[]) midLineSegmentsComplex[i])[0]);
                var midLineEnd = new Complex(((Complex[]) midLineSegmentsComplex[i])[1]);
                ArrayList[] points =
                {
                    ((ArrayList[]) allSegmentPointsComplex[i])[0],
                    ((ArrayList[]) allSegmentPointsComplex[i])[1]
                };
                var iiii = i == 0 ? i : i - 1;
                ArrayList[] pointsP =
                {
                    ((ArrayList[]) allSegmentPointsComplex[iiii])[0],
                    ((ArrayList[]) allSegmentPointsComplex[iiii])[1]
                };
                iiii = i == allSegmentPointsComplex.Count - 1 ? i : i + 1;
                ArrayList[] pointsN =
                {
                    ((ArrayList[]) allSegmentPointsComplex[iiii])[0],
                    ((ArrayList[]) allSegmentPointsComplex[iiii])[1]
                };
                var firstSide = new ArrayList(points[0]);
                var secondSide = new ArrayList(points[1]);
                var firstSideN = new ArrayList(pointsN[0]);
                var secondSideN = new ArrayList(pointsN[1]);
                var firstSideP = new ArrayList(pointsP[0]);
                var secondSideP = new ArrayList(pointsP[1]);

                //-------------------------------------------------------------------------
                //намираме "ort" на оста Y за currUCS и ucss
                var origin = currUcs.ToACS(new Complex(0, 0));
                var originN = ucss.ToACS(new Complex(0, 0));

                var ort = new Complex(currUcs.ToACS(new Complex(0, 1)) - origin);
                var ortN = new Complex(ucss.ToACS(new Complex(0, 1)) - originN);

                //проекцията на първата точка от сегмента midLiline (midLineStartComplex) върху pLineI
                var midLineStartPLineI = new Complex(midLineStart + ort*(double) removingLenArray[i]);

                //проекцията на първата точка от сегмента midLiline (midLineStartComplex) върху pLineI
                var midLineEndNLineI = new Complex(midLineEnd - ortN*(double) removingLenArray[i + 1]);

                //характерни точки PRE
                var pre1 = new Complex(currUcs.FromACS((Complex) firstSide[0]));
                var pre2 = new Complex(currUcs.FromACS((Complex) firstSideP[firstSideP.Count - 1]));
                var pre3 = new Complex(Math.Abs(pre1.real()) <= Math.Abs(pre2.real()) ? pre1 : pre2);
                var pf = new Complex(pre3.real(), currUcs.FromACS((Complex) firstSide[0]).imag());
                pf = currUcs.ToACS(pf);
                pre1 = new Complex(currUcs.FromACS((Complex) secondSide[0]));
                pre2 = new Complex(currUcs.FromACS((Complex) secondSideP[secondSideP.Count - 1]));
                pre3 = new Complex(Math.Abs(pre1.real()) <= Math.Abs(pre2.real()) ? pre1 : pre2);
                var ps = new Complex(pre3.real(), currUcs.FromACS((Complex) firstSide[0]).imag());
                ps = currUcs.ToACS(ps);

                pre1 = new Complex(ucss.FromACS((Complex) firstSide[firstSide.Count - 1]));
                pre2 = new Complex(ucss.FromACS((Complex) firstSideN[0]));
                pre3 = new Complex(Math.Abs(pre1.real()) <= Math.Abs(pre2.real()) ? pre1 : pre2);
                var nf = new Complex(pre3.real(), ucss.FromACS((Complex) firstSide[firstSide.Count - 1]).imag());
                nf = ucss.ToACS(nf);

                pre1 = new Complex(ucss.FromACS((Complex) secondSide[secondSide.Count - 1]));
                pre2 = new Complex(ucss.FromACS((Complex) secondSideN[0]));
                pre3 = new Complex(Math.Abs(pre1.real()) <= Math.Abs(pre2.real()) ? pre1 : pre2);
                var ns = new Complex(pre3.real(), ucss.FromACS((Complex) firstSide[firstSide.Count - 1]).imag());
                ns = ucss.ToACS(ns);

                //намираме съответните точки върху линиите на огъване
                var pfBend = pf - ort*(double) arcArray[i];
                var psBend = ps - ort*(double) arcArray[i];
                var nfBend = nf + ortN*(double) arcArray[i + 1];
                var nsBend = ns + ortN*(double) arcArray[i + 1];

                Complex[] bcp = {pf, pfBend, psBend, ps, nf, nfBend, nsBend, ns};
                bendConturPoints.Add(bcp);

                if (i > 0)
                {
                    /*                 if (((bool)((bool[])coincidence[i])[0] == true) && (firstSide.Count>1))
                                     {
                                         firstSide.Insert(0, pfBend);
                                     }
                                     else*/
                    {
                        firstSide.Insert(0, pf);
                        firstSide.Insert(0, pfBend);
                    }
                    /*                  if (((bool)((bool[])coincidence[i])[1] == true) && (secondSide.Count>1))
                                      {
                                          secondSide.Insert(0, psBend);
                                      }
                                      else*/
                    {
                        secondSide.Insert(0, ps);
                        secondSide.Insert(0, psBend);
                    }
                }
                if (i < allSegmentPointsComplex.Count - 1)
                {
                    /*                  if (((bool)((bool[])coincidence[i + 1])[0] == true) && (firstSide.Count>1))
                                      {
                                          firstSide.Add(nfBend);
                                      }
                                      else*/
                    {
                        firstSide.Add(nf);
                        firstSide.Add(nfBend);
                    }
                    /*                 if (((bool)((bool[])coincidence[i + 1])[1] == true) && (secondSide.Count>1))
                                     {
                                         secondSide.Add(nsBend);
                                     }
                                     else*/
                    {
                        secondSide.Add(ns);
                        secondSide.Add(nsBend);
                    }
                }

                ArrayList[] pointsComplex = {firstSide, secondSide};
                allSegmentPointsComplex1.Add(pointsComplex);
            }

            #endregion

            //-----

            #region draw

            var bendsConturPre = (Complex[]) bendConturPoints[0];
            var target = (bendsConturPre[5] + bendsConturPre[6])/2.0;
            var fP = new ArrayList();
            var sP = new ArrayList();

            ArrayList[] points0 =
            {
                ((ArrayList[]) allSegmentPointsComplex1[0])[0],
                ((ArrayList[]) allSegmentPointsComplex1[0])[1]
            };
            var firstSide0 = new ArrayList(points0[0]);
            var secondSide0 = new ArrayList(points0[1]);
            foreach (Complex c in firstSide0)
            {
                fP.Add(c);
            }
            foreach (Complex c in secondSide0)
            {
                sP.Add(c);
            }

            var translations = new ArrayList {new Complex(0.0, 0.0)};
            var bc = new ArrayList {bendsConturPre};
            for (var i = 1; i < allSegmentPointsComplex.Count; i++)
            {
                ArrayList[] points =
                {
                    ((ArrayList[]) allSegmentPointsComplex1[i])[0],
                    ((ArrayList[]) allSegmentPointsComplex1[i])[1]
                };
                var firstSide = new ArrayList(points[0]);
                var secondSide = new ArrayList(points[1]);

                var bendsContur = (Complex[]) bendConturPoints[i];

                //търсим вктор на транслация
                var v = target - (bendsContur[1] + bendsContur[2])/2.0;

                bendsContur[0] += v;
                bendsContur[1] += v;
                bendsContur[2] += v;
                bendsContur[3] += v;
                bendsContur[4] += v;
                bendsContur[5] += v;
                bendsContur[6] += v;
                bendsContur[7] += v;
                bc.Add(bendsContur);
                target = (bendsContur[5] + bendsContur[6])/2.0;

                foreach (Complex c in firstSide)
                {
                    fP.Add(c + v);
                }
                foreach (Complex c in secondSide)
                {
                    sP.Add(c + v);
                }

                translations.Add(v);
            }

            var ucsR = new UCS2d(start, Math.PI/2.0); //за хоризонтиране
            //създаване на полилинията          
            var srttTransV = start - ((Complex) fP[0] + (Complex) sP[0])/2.0;
            var polyLine = new ArrayList();
            foreach (Complex c in fP)
            {
                polyLine.Add(c + srttTransV);
            }
            sP.Reverse();
            foreach (Complex c in sP)
            {
                polyLine.Add(c + srttTransV);
            }
            polyLine.Add((Complex) fP[0] + srttTransV);

            using (var acTrans = _db.TransactionManager.StartTransaction())
            {
                var acBlkTbl = (BlockTable) acTrans.GetObject(_db.BlockTableId,
                    OpenMode.ForRead);

                var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite);

                var acPoly = new Polyline();
                acPoly.SetDatabaseDefaults();
                var counter = 0;
                foreach (Complex c in polyLine)
                {
                    acPoly.AddVertexAt(counter,
                        new Point2d((ucsR.FromACS(c) + start).real(), (ucsR.FromACS(c) + start).imag()), 0, 0, 0);
                    counter++;
                }

                acBlkTblRec.AppendEntity(acPoly);
                acTrans.AddNewlyCreatedDBObject(acPoly, true);



                var curUcsMatrix = _doc.Editor.CurrentUserCoordinateSystem;
                var curUcs = curUcsMatrix.CoordinateSystem3d;
                for (var i = 0; i < bc.Count; i++)
                {
                    var arr = (Complex[]) bc[i];
                    var p1 = new Complex(ucsR.FromACS(arr[0] + srttTransV) + start);
                    var p2 = new Complex(ucsR.FromACS(arr[3] + srttTransV) + start);
                    var p3 = new Complex(ucsR.FromACS(arr[1] + srttTransV) + start);
                    var p4 = new Complex(ucsR.FromACS(arr[2] + srttTransV) + start);
                    var p5 = new Complex(ucsR.FromACS(arr[4] + srttTransV) + start);
                    var p6 = new Complex(ucsR.FromACS(arr[7] + srttTransV) + start);
                    var p7 = new Complex(ucsR.FromACS(arr[5] + srttTransV) + start);
                    var p8 = new Complex(ucsR.FromACS(arr[6] + srttTransV) + start);

                    if (i > 0)
                    {
                        acLine = new Line(new Point3d(p1.real(), p1.imag(), 0), new Point3d(p2.real(), p2.imag(), 0));
                        acLine.SetDatabaseDefaults();
                        Draw(acLine, acTrans);
                        acLine = new Line(new Point3d(p3.real(), p3.imag(), 0), new Point3d(p4.real(), p4.imag(), 0));
                        acLine.SetDatabaseDefaults();
                        Draw(acLine, acTrans);

                        var acText = new DBText {Height = 5.75};
                        var razm = (double) bendsAnglesArray[i]*180.0/Math.PI;
                        if (razm < 0)
                        {
                            razm = -180.0 + Math.Abs(razm);
                        }
                        else
                        {
                            razm = 180.0 - razm;
                        }
                        acText.TextString = razm.ToString("f3") + "%%d";
                        var startAng = new Complex(p4.real() - p3.real(), p4.imag() - p3.imag()).arg();
                        acText.Position = new Point3d((p3.real() + p4.real())/2.0, (p3.imag() + p4.imag())/2.0, 0);
                        if (razm >= 0)
                        {
                            acText.TransformBy(Matrix3d.Rotation(startAng, curUcs.Zaxis, acText.Position));
                            double dX;
                            if (Math.Abs(Math.Cos(startAng)) > 0.00001)
                            {
                                dX = (double) arcArray[i]/Math.Cos(startAng);
                            }
                            else
                            {
                                dX = (double) arcArray[i];
                            }
                            if (Math.Abs(dX) < (double) arcArray[i])
                            {
                                dX = (double) arcArray[i]*dX/Math.Abs(dX);
                            }

                            acText.TransformBy(Matrix3d.Displacement(new Vector3d(dX + dX + acText.Height, 0, 0)));
                        }
                        else
                        {
                            acText.TransformBy(Matrix3d.Rotation(startAng + Math.PI, curUcs.Zaxis, acText.Position));
                            double dX;
                            if (Math.Abs(Math.Cos(startAng)) > 0.00001)
                            {
                                dX = (double) arcArray[i]/Math.Cos(startAng);
                            }
                            else
                            {
                                dX = (double) arcArray[i];
                            }
                            if (Math.Abs(dX) < (double) arcArray[i])
                            {
                                dX = (double) arcArray[i]*dX/Math.Abs(dX);
                            }

                            acText.TransformBy(Matrix3d.Displacement(new Vector3d(dX + dX, 0, 0)));
                        }


                        Draw(acText, acTrans);
                    }

                    if (i < bc.Count - 1)
                    {
                        acLine = new Line(new Point3d(p5.real(), p5.imag(), 0), new Point3d(p6.real(), p6.imag(), 0));
                        acLine.SetDatabaseDefaults();
                        Draw(acLine, acTrans);
                        acLine = new Line(new Point3d(p7.real(), p7.imag(), 0), new Point3d(p8.real(), p8.imag(), 0));
                        acLine.SetDatabaseDefaults();
                        Draw(acLine, acTrans);
                    }

                    if ((p3 - p7).abs() > 3*_s)
                    {
                        AddDim(i == 0 ? ucsR.FromACS((Complex) fP[0] + srttTransV) + start : p3,
                            i == bc.Count - 1 ? ucsR.FromACS((Complex) fP[fP.Count - 1] + srttTransV) + start : p7,
                            -30.0);
                    }
                    if ((p4 - p8).abs() > 3*_s)
                    {
                        AddDim(i == 0 ? ucsR.FromACS((Complex) sP[sP.Count - 1] + srttTransV) + start : p4,
                            i == bc.Count - 1 ? ucsR.FromACS((Complex) sP[0] + srttTransV) + start : p8, 20.0);
                    }

                    //AddDim((p7 + p8) / 2, (p5 + p6) / 2, 0);
                }

                acTrans.Commit();
            }
            //размер по дебелина
            var qs = ucsR.FromACS((Complex) fP[0] + srttTransV) + start;
            var qq = ucsR.FromACS((Complex) sP[sP.Count - 1] + srttTransV) + start;
            AddDim(qs, qq, 10.0 + _s);

            qs = ucsR.FromACS((Complex) fP[fP.Count - 1] + srttTransV) + start;
            qq = ucsR.FromACS((Complex) sP[0] + srttTransV) + start;
            AddDim(qs, qq, -10.0 - _s);

            //изчертаване на кръглите отвори
            for (var i = 0; i < ucsArray.Count; i++)
            {
                var qUcs = (UCS) ucsArray[i];
                var nUcs = new UCS(); //следваща
                var pUcs = new UCS(); //следваща
                if (i == 0)
                {
                    pUcs = (UCS) ucsArray[i + 1];
                }
                else
                {
                    pUcs = (UCS) ucsArray[i - 1];
                }
                if (i < ucsArray.Count - 1)
                {
                    nUcs = (UCS) ucsArray[i + 1];
                }
                else
                {
                    nUcs = (UCS) ucsArray[i - 1];
                }
                var midLineN = (new Quaternion(nUcs.FromACS(((Quaternion[]) midLinesArray[i])[0])) +
                                       new Quaternion(nUcs.FromACS(((Quaternion[]) midLinesArray[i])[1])))/
                                      2.0;
                var midLineP = (new Quaternion(pUcs.FromACS(((Quaternion[]) midLinesArray[i])[0])) +
                                       new Quaternion(pUcs.FromACS(((Quaternion[]) midLinesArray[i])[1])))/
                                      2.0;


                foreach (Quaternion h in holesCircles)
                {
                    if (h.q[0] <= 0.0)
                    {
                        continue;
                    }
                    var Qq = new Quaternion(0, h.GetX(), h.GetY(), h.GetZ());
                    var q = new Quaternion(qUcs.FromACS(Qq));

                    bool isp;
                    if (ucsArray.Count > 2)
                    {
                        isp = (Math.Sign(midLineP.GetZ()) == Math.Sign(pUcs.FromACS(Qq).GetZ())) &&
                              (Math.Sign(midLineN.GetZ()) == Math.Sign(nUcs.FromACS(Qq).GetZ()));
                    }
                    else
                    {
                        isp = true;
                    }

                    // Quaternion t = nUCS.FromACS(qUCS.ToACS(new Quaternion(0, 0, 10.0, 0)));
                    // MessageBox.Show(t.GetX().ToString() + " , " + t.GetY().ToString() + " , " + t.GetZ());
                    if ((Math.Abs(q.GetZ()) < Constants.zero_dist) && isp && (h.real() > 0.0))
                    {
                        var cen = new Complex(q.GetX(), q.GetY());
                        cen = ((UCS2d) ucss_2D[i]).ToACS(cen);
                        cen += (Complex) translations[i];
                        cen = ucsR.FromACS(cen + srttTransV) + start;

                        using (var acTrans = _db.TransactionManager.StartTransaction())
                        {
                            // Open the Block table for read
                            var acBlkTbl = (BlockTable) acTrans.GetObject(_db.BlockTableId,
                                OpenMode.ForRead);

                            // Open the Block table record Model space for write
                            var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                OpenMode.ForWrite);

                            // Create a circle that is at 2,3 with a radius of 4.25
                            var acCirc = new Circle();
                            acCirc.SetDatabaseDefaults();
                            acCirc.Center = new Point3d(cen.real(), cen.imag(), 0);
                            acCirc.Radius = h.real();

                            acBlkTblRec.AppendEntity(acCirc);
                            acTrans.AddNewlyCreatedDBObject(acCirc, true);

                            acTrans.Commit();
                        }
                        h.q[0] = -1.0;
                    }
                }
            }

            //изчертаване на отворите от 3DPOLY
            for (var i = 0; i < ucsArray.Count; i++)
            {
                var qUcs = (UCS) ucsArray[i];
                var nUcs = new UCS(); //следваща
                var pUcs = new UCS(); //следваща
                if (i == 0)
                {
                    pUcs = (UCS) ucsArray[i + 1];
                }
                else
                {
                    pUcs = (UCS) ucsArray[i - 1];
                }
                if (i < ucsArray.Count - 1)
                {
                    nUcs = (UCS) ucsArray[i + 1];
                }
                else
                {
                    nUcs = (UCS) ucsArray[i - 1];
                }
                var midLineN = (new Quaternion(nUcs.FromACS(((Quaternion[]) midLinesArray[i])[0])) +
                                       new Quaternion(nUcs.FromACS(((Quaternion[]) midLinesArray[i])[1])))/
                                      2.0;
                var midLineP = (new Quaternion(pUcs.FromACS(((Quaternion[]) midLinesArray[i])[0])) +
                                       new Quaternion(pUcs.FromACS(((Quaternion[]) midLinesArray[i])[1])))/
                                      2.0;


                foreach (ArrayList lines in holesPLines)
                {
                    var Qq = ((Quaternion) lines[0] + (Quaternion) lines[1])/2.0;
                    var q = new Quaternion(qUcs.FromACS(Qq));
                    bool isp;
                    if (ucsArray.Count > 2)
                    {
                        isp = (Math.Sign(midLineP.GetZ()) == Math.Sign(pUcs.FromACS(Qq).GetZ())) &&
                              (Math.Sign(midLineN.GetZ()) == Math.Sign(nUcs.FromACS(Qq).GetZ()));
                    }
                    else
                    {
                        isp = true;
                    }
                    if ((Math.Abs(q.GetZ()) < Constants.zero_dist) && isp)
                    {
                        using (var acTrans = _db.TransactionManager.StartTransaction())
                        {
                            // Open the Block table for read
                            var acBlkTbl = (BlockTable) acTrans.GetObject(_db.BlockTableId,
                                OpenMode.ForRead);

                            // Open the Block table record Model space for write
                            var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                OpenMode.ForWrite);

                            var acPoly = new Polyline();
                            acPoly.SetDatabaseDefaults();
                            var counter = 0;

                            foreach (Quaternion qu in lines)
                            {
                                var qua = qUcs.FromACS(qu);
                                var cen = new Complex(qua.GetX(), qua.GetY());
                                cen = ((UCS2d) ucss_2D[i]).ToACS(cen);
                                cen += (Complex) translations[i];
                                cen = ucsR.FromACS(cen + srttTransV) + start;

                                acPoly.AddVertexAt(counter, new Point2d(cen.real(), cen.imag()), 0, 0, 0);
                                counter++;
                            }


                            // Add the new object to the block table record and the transaction
                            acBlkTblRec.AppendEntity(acPoly);
                            acTrans.AddNewlyCreatedDBObject(acPoly, true);

                            // Save the new object to the database
                            acTrans.Commit();

                        }
                    }
                }
            }

            //изчертаване на отворите от 2DPOLY
            for (var i = 0; i < ucsArray.Count; i++)
            {
                var qUcs = (UCS) ucsArray[i];
                var nUcs = new UCS(); //следваща
                var pUcs = new UCS(); //следваща
                if (i == 0)
                {
                    pUcs = (UCS) ucsArray[i + 1];
                }
                else
                {
                    pUcs = (UCS) ucsArray[i - 1];
                }
                if (i < ucsArray.Count - 1)
                {
                    nUcs = (UCS) ucsArray[i + 1];
                }
                else
                {
                    nUcs = (UCS) ucsArray[i - 1];
                }
                var midLineN = (new Quaternion(nUcs.FromACS(((Quaternion[]) midLinesArray[i])[0])) +
                                       new Quaternion(nUcs.FromACS(((Quaternion[]) midLinesArray[i])[1])))/
                                      2.0;
                var midLineP = (new Quaternion(pUcs.FromACS(((Quaternion[]) midLinesArray[i])[0])) +
                                       new Quaternion(pUcs.FromACS(((Quaternion[]) midLinesArray[i])[1])))/
                                      2.0;


                foreach (ArrayList lines in holesLwLines)
                {
                    var q1 = new Quaternion((Quaternion) lines[0]);
                    var q2 = new Quaternion((Quaternion) lines[1]);
                    q1.q[0] = 0.0;
                    q2.q[0] = 0.0;
                    var Qq = (q1 + q2)/2.0;
                    var q = new Quaternion(qUcs.FromACS(Qq));
                    bool isp;
                    if (ucsArray.Count > 2)
                    {
                        isp = (Math.Sign(midLineP.GetZ()) == Math.Sign(pUcs.FromACS(Qq).GetZ())) &&
                              (Math.Sign(midLineN.GetZ()) == Math.Sign(nUcs.FromACS(Qq).GetZ()));
                    }
                    else
                    {
                        isp = true;
                    }
                    if ((Math.Abs(q.GetZ()) < Constants.zero_dist) && isp)
                    {
                        using (var acTrans = _db.TransactionManager.StartTransaction())
                        {
                            // Open the Block table for read
                            var acBlkTbl = (BlockTable) acTrans.GetObject(_db.BlockTableId,
                                OpenMode.ForRead);

                            // Open the Block table record Model space for write
                            var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                OpenMode.ForWrite);

                            var acPoly = new Polyline();
                            acPoly.SetDatabaseDefaults();
                            var counter = 0;

                            var normal = new Quaternion((Quaternion) lines[lines.Count - 1]);
                            normal *= 10.0;
                            normal += qUcs.o;
                            normal = qUcs.FromACS(normal);
                            lines.RemoveAt(lines.Count - 1);
                            foreach (Quaternion qu in lines)
                            {
                                var bulge = qu.real();
                                var quu = new Quaternion(0, qu.GetX(), qu.GetY(), qu.GetZ());
                                var qua = qUcs.FromACS(quu);

                                var cen = new Complex(qua.GetX(), qua.GetY());
                                cen = ((UCS2d) ucss_2D[i]).ToACS(cen);
                                cen += (Complex) translations[i];
                                cen = ucsR.FromACS(cen + srttTransV) + start;

                                acPoly.AddVertexAt(counter, new Point2d(cen.real(), cen.imag()), 0, 0, 0);
                                if (normal.GetZ() >= 0)
                                {
                                    acPoly.SetBulgeAt(lines.IndexOf(qu), bulge);
                                }
                                else
                                {
                                    acPoly.SetBulgeAt(lines.IndexOf(qu), -bulge);
                                }
                                counter++;
                            }


                            // Add the new object to the block table record and the transaction
                            acBlkTblRec.AppendEntity(acPoly);
                            acTrans.AddNewlyCreatedDBObject(acPoly, true);

                            // Save the new object to the database
                            acTrans.Commit();

                        }
                    }
                }
            }

            #endregion

            _ed.UpdateScreen();
            Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
    }
} 