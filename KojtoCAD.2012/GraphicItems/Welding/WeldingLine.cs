using System;
using System.Windows.Forms;
using KojtoCAD.Mathematics.Geometry;
using KojtoCAD.Properties;
using KojtoCAD.Utilities;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
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

[assembly: CommandClass(typeof(KojtoCAD.GraphicItems.Welding.WeldingLine))]
namespace KojtoCAD.GraphicItems.Welding
{
    public class WeldingLine : IDisposable
    {
        private readonly Document _doc = Application.DocumentManager.MdiActiveDocument;
        private readonly Editor _ed = Application.DocumentManager.MdiActiveDocument.Editor;
        private readonly Database _db = Application.DocumentManager.MdiActiveDocument.Database;
        private readonly DocumentHelper _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        private DBObjectCollection _weldingArcs = new DBObjectCollection();
        private Point3dCollection _weldingVertices = new Point3dCollection();

        /// <summary>
        /// Welding Line
        /// </summary>
        [CommandMethod("wl")]
        public void WeldingLineStart()
        {
            
            #region Select welding mode
            var weldingModeOpts = new PromptKeywordOptions("\nWeld over points or object? ");
            weldingModeOpts.Keywords.Add("Points");
            weldingModeOpts.Keywords.Add("polYline");
            weldingModeOpts.Keywords.Add("Line");
            weldingModeOpts.Keywords.Add("Circle");
            weldingModeOpts.Keywords.Add("Arc");
            weldingModeOpts.Keywords.Default = "Points";

            var weldingModeRslt = _ed.GetKeywords(weldingModeOpts);
            // If the user pressed cancel - return with no error
            if (weldingModeRslt.Status != PromptStatus.OK)
            {
                return;
            }

            var selectedObjectId = new ObjectId();
            // If the user selected Points mode 
            if (weldingModeRslt.StringResult == "Points")
            {
                //Prompt the user for vertices    
                _weldingVertices.Clear();
                _weldingVertices = GetPointsFromUser();
                if (_weldingVertices.Count < 1)
                {
                    return;
                }
            }
            // Else the user selected an Object mode from the list
            else
            {
                // Prompt for object
                var objectSelectionOpts = new PromptEntityOptions("\nSelect the object : ");
                objectSelectionOpts.SetRejectMessage("\nEntity is not of the predifined type :" + weldingModeRslt.StringResult);
                objectSelectionOpts.AddAllowedClass(typeof(Line), false);
                objectSelectionOpts.AddAllowedClass(typeof(Polyline), false);
                objectSelectionOpts.AddAllowedClass(typeof(Circle), false);
                objectSelectionOpts.AddAllowedClass(typeof(Arc), false);

                var objectSelectionRslt = _ed.GetEntity(objectSelectionOpts);
                // If the user pressed cancel - return with no error
                if (objectSelectionRslt.Status != PromptStatus.OK)
                {
                    return;
                }
                // else get the slected object id and type
                selectedObjectId = objectSelectionRslt.ObjectId;
            }
            #endregion

            #region Welding side selection

            // Prompt the user to select object side M for middle - draw on the middle of the line or Point - for side.       
            var weldingSideOpts = new PromptKeywordOptions("\nPick side point or draw in the middle. ");
            weldingSideOpts.Keywords.Add("Side");
            weldingSideOpts.Keywords.Add("Middle ");
            weldingSideOpts.Keywords.Default = "Side";
            var weldingSideRslt = _ed.GetKeywords(weldingSideOpts);
            // If the user pressed cancel - return with no error
            if (weldingSideRslt.Status != PromptStatus.OK)
            {
                return;
            }

            // If tThe user selected 'Side'
            var sidePoint = new Point3d();
            if (weldingSideRslt.StringResult == "Side")
            {
                // Prompt the user to pick up a side point on the desired side.
                var sidePointOpts = new PromptPointOptions("\nPick side point : ");
                var sidePointRslt = _ed.GetPoint(sidePointOpts);
                // If the user pressed cancel - return with no error
                if (sidePointRslt.Status != PromptStatus.OK)
                {
                    return;
                }
                sidePoint = sidePointRslt.Value;
            }
            // Else If the user selected 'Middle'
            else if (weldingSideRslt.StringResult == "Middle")
            {
                sidePoint = new Point3d(0, 0, 0);
            }
            // else 
            // Not supposed to end here, because the choise in the prompt is restricted to two options 
            #endregion

            #region Welding arcs lenght and distance selection

            // Prompt the user to set welding arcs lenght
            var weldingArcsChordOpts = new PromptDoubleOptions("\nWelding arcs lenght : ")
            {
                UseDefaultValue = true,
                DefaultValue = Settings.Default.WeldingArcsLenght
            };

            var weldingArcsChordRslt = _ed.GetDouble(weldingArcsChordOpts);
            // If the user pressed cancel - return with no error
            if (weldingArcsChordRslt.Status != PromptStatus.OK)
            {
                return;
            }
            if (weldingArcsChordRslt.Value <= 0)
            {
                _ed.WriteMessage("Lenght must be positive number.");
                return;
            }
            Settings.Default.WeldingArcsLenght = weldingArcsChordRslt.Value;
            //else
            // The choice is Ok and lenght is provided. Continue...

            // Prompt the user to set the distance between the welding arcs
            var weldingArcsDistanceOpts = new PromptDoubleOptions("\nSet the distance between the welding arcs : ")
            {
                UseDefaultValue = true,
                DefaultValue = Settings.Default.WeldingArcsDistance
            };
            var weldingArcsOffsetRslt = _ed.GetDouble(weldingArcsDistanceOpts);
            // If the user pressed cancel - return with no error
            if (weldingArcsOffsetRslt.Status != PromptStatus.OK)
            {
                return;
            }
            if (weldingArcsOffsetRslt.Value <= 0)
            {
                _ed.WriteMessage("Distance must be positive number.");
            }
            Settings.Default.WeldingArcsDistance = weldingArcsOffsetRslt.Value;
            Settings.Default.Save();
            //else
            // The choice is Ok and distance is provided. Continue...
            #endregion

            #region Hand over the process to the specific drawing function
            _weldingArcs.Clear();

            switch (weldingModeRslt.StringResult)
            {
                #region Points
                case "Points":

                    for (var i = 0; i < _weldingVertices.Count; i++)
                    {
                        // слага ги в глобалната
                        _weldingVertices[i] = _weldingVertices[i].TransformBy(GeometryUtility.GetTransforMatrixToWcs());
                    }
                    using (var tr = _doc.Database.TransactionManager.StartTransaction())        
                    { 
                        var sPt = new Point3d(sidePoint.X + 0.00001, sidePoint.Y + 0.00001, sidePoint.Z);
                        sPt = sPt.TransformBy(GeometryUtility.GetTransforMatrixToWcs());                            
                        // реперната точка и тя в глобалната

                        var tP1 = new Point3d(0, 0, 0);
                        var tP2 = new Point3d(_weldingVertices[1].X - _weldingVertices[0].X, _weldingVertices[1].Y - _weldingVertices[0].Y, _weldingVertices[1].Z - _weldingVertices[0].Z);
                        var tP3 = new Point3d(sPt.X - _weldingVertices[0].X, sPt.Y - _weldingVertices[0].Y, sPt.Z - _weldingVertices[0].Z);

                      
                        var ucs = GeometryUtility.GetUcs(tP1, tP2, tP3, false);
                        for (var i = 0; i < _weldingVertices.Count; i++)
                        {
                            _weldingVertices[i] = _weldingVertices[i].TransformBy(ucs);                       
                        }
                        if (weldingSideRslt.StringResult == "Side")                                          
                        {
                            sidePoint = sidePoint.TransformBy(_ed.CurrentUserCoordinateSystem);               
                            sidePoint = sidePoint.TransformBy(ucs);                                          
                        }

                        var oldUcs = _ed.CurrentUserCoordinateSystem;  
                        _ed.CurrentUserCoordinateSystem = Matrix3d.Identity; 

                        _weldingArcs = BuildWeldingArcsOverPts(_weldingVertices,
                                                              sidePoint,
                                                              weldingArcsChordRslt.Value,
                                                              weldingArcsOffsetRslt.Value,
                                                              weldingArcsOffsetRslt.Value / 2);
                        DrawWeldingLine(_weldingArcs, tr);

                        _ed.CurrentUserCoordinateSystem = oldUcs;  

                        foreach (Arc a in _weldingArcs)            
                        {
                            a.TransformBy(ucs.Inverse());         
                        }


                        tr.Commit();
                    }
                    break;
                #endregion

                #region Line
                case "Line":
                    using (var tr = _doc.Database.TransactionManager.StartTransaction())
                    {
                        _weldingVertices.Clear();

                        var ent = tr.GetObject(selectedObjectId, OpenMode.ForRead) as Entity;                                                                  // Lishkov 07/07/2012 
                        var name = ent.GetType().ToString();                                                                                                   // Lishkov 07/07/2012 
                        if ((name.IndexOf("Line") < 0) || (name.IndexOf("Polyline") >= 0)) { MessageBox.Show("Selection is not  Line !", "E R R O R"); break; }   // Lishkov 07/07/2012 

                        var weldingLine = (Line) tr.GetObject(selectedObjectId, OpenMode.ForWrite);
 
                        var sPt = new Point3d(sidePoint.X + 0.00001, sidePoint.Y + 0.00001, sidePoint.Z);
                        sPt = sPt.TransformBy(GeometryUtility.GetTransforMatrixToWcs());                     

                        var tP1 = new Point3d(0, 0, 0);
                        var tP2 = new Point3d(weldingLine.EndPoint.X - weldingLine.StartPoint.X, weldingLine.EndPoint.Y - weldingLine.StartPoint.Y, weldingLine.EndPoint.Z - weldingLine.StartPoint.Z);
                        var tP3 = new Point3d(sPt.X - weldingLine.StartPoint.X, sPt.Y - weldingLine.StartPoint.Y, sPt.Z - weldingLine.StartPoint.Z);
    
                        var ucs = GeometryUtility.GetUcs(tP1, tP2, tP3, false);
                        weldingLine.TransformBy(ucs);                                                        
                        if (weldingSideRslt.StringResult == "Side")                                          
                        {
                            sidePoint = sidePoint.TransformBy(_ed.CurrentUserCoordinateSystem);               
                            sidePoint = sidePoint.TransformBy(ucs);                                          
                        }

                        var oldUcs = _ed.CurrentUserCoordinateSystem;  
                        _ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                        _weldingVertices.Add(weldingLine.StartPoint);
                        _weldingVertices.Add(weldingLine.EndPoint);

                        _weldingArcs = BuildWeldingArcsOverPts(_weldingVertices,
                                                              sidePoint,
                                                              weldingArcsChordRslt.Value,
                                                              weldingArcsOffsetRslt.Value,
                                                              weldingArcsOffsetRslt.Value / 2);
                        DrawWeldingLine(_weldingArcs, tr);

                        _ed.CurrentUserCoordinateSystem = oldUcs;  

                        foreach (Arc a in _weldingArcs)            
                        {
                            a.TransformBy(ucs.Inverse());         
                        }

                        weldingLine.TransformBy(ucs.Inverse());   


                        tr.Commit();
                    }

                    break;
                #endregion

                #region Polyline
                case "polYline":
                case "Polyline":
                case "polyline":

                    using (var tr = _doc.Database.TransactionManager.StartTransaction())
                    {
                        var weldingPoly = tr.GetObject(selectedObjectId, OpenMode.ForWrite) as Polyline;
                        if (weldingPoly == null)
                        {
                            MessageBox.Show("Selection is not  PolyLine !", "E R R O R"); 
                            break;
                        }  

                        var temp = new Point3dCollection();
                        var temp1 = new Point3dCollection();
                        DBObjectCollection acDbObjColl = null; //acPoly.GetOffsetCurves(0.25);

                        var acBlkTbl = tr.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        var acBlkTblRec = (BlockTableRecord) tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                        //офсетирано копие
                        acDbObjColl = weldingPoly.GetOffsetCurves(0.001);//тестова на много малко разстояние от оригинала // Lishkov 07/07/2012 
                        foreach (Entity acEnt in acDbObjColl)
                        {
                            // Add each offset object
                            acBlkTblRec.AppendEntity(acEnt);
                            tr.AddNewlyCreatedDBObject(acEnt, true);
                        }
                        Polyline weldingPolyOffset = null;
                        try
                        {
                            weldingPolyOffset = tr.GetObject(acDbObjColl[0].ObjectId, OpenMode.ForWrite) as Polyline;
                        }
                        catch { }
                        if (weldingPolyOffset != null)
                        {
                            //точките деления по двете полилинии             
                            temp = GeometryUtility.DividePoly(ref weldingPoly, weldingArcsOffsetRslt.Value);
                            temp1 = GeometryUtility.DividePoly(ref  weldingPolyOffset, weldingArcsOffsetRslt.Value);

                            //проверка вярна ли е посоката на офсетиране
                            var tP = sidePoint.TransformBy(GeometryUtility.GetTransforMatrixToWcs());
                            var dist1 = temp[0].DistanceTo(tP);
                            var dist2 = temp1[0].DistanceTo(tP);

                            foreach (Point3d p in temp)
                            {
                                if (p.DistanceTo(tP) < dist1) { dist1 = p.DistanceTo(tP); }
                            }
                            foreach (Point3d p in temp1)
                            {
                                if (p.DistanceTo(tP) < dist2) { dist2 = p.DistanceTo(tP); }
                            }

                            //ако не е вярна посоката на офсетиране
                            #region
                            if (dist1 < dist2)//ако посоката е грешна изтриваме тестовата и правим в обратна поскока на зададеното разстояние
                            {
                                weldingPolyOffset.Erase();
                                acDbObjColl = weldingPoly.GetOffsetCurves(-weldingArcsChordRslt.Value);
                                foreach (Entity acEnt in acDbObjColl)
                                {
                                    // Add each offset object
                                    acBlkTblRec.AppendEntity(acEnt);
                                    tr.AddNewlyCreatedDBObject(acEnt, true);
                                }
                                try
                                {
                                    weldingPolyOffset = tr.GetObject(acDbObjColl[0].ObjectId, OpenMode.ForWrite) as Polyline;
                                }
                                catch
                                {
                                    MessageBox.Show("Offset error", "E R R O R");
                                    return;
                                }
                                temp1 = GeometryUtility.DividePoly(ref  weldingPolyOffset, weldingArcsOffsetRslt.Value);
                            }
                            else //ако посоката е вярна изтриваме тестовата и правим в същата поскока на зададеното разстояние // Lishkov 07/07/2012 
                            {
                                weldingPolyOffset.Erase();
                                acDbObjColl = weldingPoly.GetOffsetCurves(weldingArcsChordRslt.Value);
                                foreach (Entity acEnt in acDbObjColl)
                                {
                                    // Add each offset object
                                    acBlkTblRec.AppendEntity(acEnt);
                                    tr.AddNewlyCreatedDBObject(acEnt, true);
                                }
                                try
                                {
                                    weldingPolyOffset = tr.GetObject(acDbObjColl[0].ObjectId, OpenMode.ForWrite) as Polyline;
                                }
                                catch
                                {
                                    MessageBox.Show("Offset error", "E R R O R");
                                    return;
                                }
                                temp1 = GeometryUtility.DividePoly(ref  weldingPolyOffset, weldingArcsOffsetRslt.Value);
                            }
                            #endregion

                            //work
                            var oldUcs = _ed.CurrentUserCoordinateSystem;
                            _ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

                            for (var i = 0; i < temp.Count - 1; i++)
                            {
                                var tempP = temp1[0];
                                var oP = temp1[0];
                                foreach (Point3d p in temp1)
                                {
                                    if (temp[i].DistanceTo(p) < temp[i].DistanceTo(tempP))
                                    {
                                        oP = tempP;
                                        tempP = p;
                                    }
                                }

                                var ucs = GeometryUtility.GetUcs(temp[i], temp[i + 1], tempP, true);

                                Arc acArc;
                                if (weldingSideRslt.StringResult != "Side")
                                {
                                    acArc = new Arc(
                                        new Point3d(0, 0, 0), 
                                        weldingArcsChordRslt.Value/2,
                                        Math.PI/2.0 + Math.PI, 
                                        Math.PI/2.0 + Math.PI + Math.PI
                                        );
                                }
                                else
                                {
                                    // BricsCAD hack
                                    acArc = new Arc(
                                        new Point3d(),
                                        weldingArcsChordRslt.Value/2,
                                        Math.PI/2.0 + Math.PI,
                                        Math.PI/2.0 + Math.PI + Math.PI
                                        ) {Center = new Point3d(0, weldingArcsChordRslt.Value/2, 0)};
                                }

                                _weldingArcs.Add(acArc);
                                try
                                {
                                    acArc.TransformBy(ucs);
                                }
                                catch
                                {
                                    _weldingArcs.RemoveAt(_weldingArcs.Count - 1);
                                    if (i > 0)
                                    {
                                        var tP1 = new Point3d(0, 0, 0);
                                        var tP2 = new Point3d(temp[i].X - temp[i - 1].X, temp[i].Y - temp[i - 1].Y, temp[i].Z - temp[i - 1].Z);
                                        var tP3 = new Point3d(oP.X - temp[i - 1].X, oP.Y - temp[i - 1].Y, oP.Z - temp[i - 1].Z);

                                        ucs = GeometryUtility.GetUcs(tP1, tP2, tP3, true);
                                        try
                                        {
                                            _weldingArcs.Add(acArc);
                                            var lp1 = new Point3d(0, 0, 0);
                                            var lp2 = new Point3d(weldingArcsOffsetRslt.Value, 0, 0);
                                            var l = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                            acArc.TransformBy(Matrix3d.Displacement(lp1.GetVectorTo(lp2)));
                                            acArc.TransformBy(ucs);
                                            acArc.TransformBy(Matrix3d.Displacement(-temp[i - 1].GetVectorTo(Point3d.Origin)));
                                        }
                                        catch
                                        {
                                            _weldingArcs.RemoveAt(_weldingArcs.Count - 1);
                                        }
                                    }
                                }
                                acArc.SetDatabaseDefaults();
                            }
                            if (!weldingPoly.Closed)
                            {
                                var acA = (Arc)_weldingArcs[_weldingArcs.Count - 1];
                                var acArc = (Entity) acA.Clone();
                                acArc.TransformBy(Matrix3d.Displacement(temp[temp.Count - 2].GetVectorTo(temp[temp.Count - 1])));
                                _weldingArcs.Add(acArc);
                            }
                            weldingPolyOffset.Erase();
                            DrawWeldingLine(_weldingArcs, tr);
                            _ed.CurrentUserCoordinateSystem = oldUcs;
                        }
                        tr.Commit();
                    }
                    break;
                #endregion

                #region Circle
                case "Circle":
                    // ElseIf the user picked up object of type 'arc' or 'circle' 
                    //    Call the function that makes welding over arc or circle. 
                    //    _Circle is an Arc with start angle of 0 and end angle of 360
                    using (var tr = _doc.Database.TransactionManager.StartTransaction())
                    {
                        var weldingCircle = tr.GetObject(selectedObjectId, OpenMode.ForRead) as Circle;
                        if (weldingCircle != null)
                        {
                            using (var T = _doc.Database.TransactionManager.StartTransaction())
                            {
                                var weldingLine = T.GetObject(selectedObjectId, OpenMode.ForWrite) as Circle;

                                var r = new Ray
                                {
                                    BasePoint = weldingLine.Center,
                                    UnitDir = weldingLine.Center.GetVectorTo(weldingLine.StartPoint)
                                };

                                var pts = new Point3dCollection();
                                weldingLine.IntersectWith(r, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                                var sPt = new Point3d(sidePoint.X + 0.00001, sidePoint.Y + 0.00001, sidePoint.Z);
                                sPt = sPt.TransformBy(GeometryUtility.GetTransforMatrixToWcs());
                                var ucs = GeometryUtility.GetUcs(weldingLine.Center, pts[0], sPt, false);
                                weldingLine.TransformBy(ucs);
                                if (weldingSideRslt.StringResult == "Side")
                                {
                                    sidePoint = sidePoint.TransformBy(_ed.CurrentUserCoordinateSystem);
                                    sidePoint = sidePoint.TransformBy(ucs);
                                }

                                var oldUcs = _ed.CurrentUserCoordinateSystem;
                                _ed.CurrentUserCoordinateSystem = Matrix3d.Identity;



                                var arcSide = GeometryUtility.SideOfPointToArc(weldingCircle.Center, weldingCircle.Radius, sidePoint);
                                var sAng = -1 + weldingCircle.Center.DistanceTo(sidePoint) / weldingCircle.Radius;
                                _weldingArcs = BuildWeldingArcsOverArc(weldingCircle.Center,
                                                                      weldingCircle.Radius,
                                                                      0.0,
                                                                      2 * Math.PI,
                                    /* sAng ,  */
                                                                      weldingArcsChordRslt.Value,
                                                                      weldingArcsOffsetRslt.Value,
                                                                      0,
                                                                      arcSide);
                                DrawWeldingLine(_weldingArcs, T);

                                _ed.CurrentUserCoordinateSystem = oldUcs;
                                foreach (Arc a in _weldingArcs)
                                {
                                    a.TransformBy(ucs.Inverse());
                                }
                                weldingLine.TransformBy(ucs.Inverse());

                                T.Commit();
                            }
                        }
                        else
                        {
                            MessageBox.Show("\nYou did not slected object of type Circle. Try again...");
                        }
                        tr.Commit();
                    }
                    break;
                #endregion

                #region Arc
                case "Arc":
                    using (var tr = _doc.Database.TransactionManager.StartTransaction())
                    {
                        var weldingArc = tr.GetObject(selectedObjectId, OpenMode.ForRead) as Arc;
                        if (weldingArc != null)
                        {

                            using (var T = _doc.Database.TransactionManager.StartTransaction())
                            {
                                var weldingLine = T.GetObject(selectedObjectId, OpenMode.ForWrite) as Arc;

                                var sPt = sidePoint.TransformBy(GeometryUtility.GetTransforMatrixToWcs());

                                var tP1 = new Point3d(0, 0, 0);
                                var tP2 = new Point3d(weldingLine.EndPoint.X - weldingLine.StartPoint.X, weldingLine.EndPoint.Y - weldingLine.StartPoint.Y, weldingLine.EndPoint.Z - weldingLine.StartPoint.Z);
                                var tP3 = new Point3d(sPt.X - weldingLine.StartPoint.X, sPt.Y - weldingLine.StartPoint.Y, sPt.Z - weldingLine.StartPoint.Z);

                                var ucs = GeometryUtility.GetUcs(tP1, tP2, tP3, false);
                                weldingLine.TransformBy(ucs);
                                if (weldingSideRslt.StringResult == "Side")
                                {
                                    sidePoint = sidePoint.TransformBy(_ed.CurrentUserCoordinateSystem);
                                    sidePoint = sidePoint.TransformBy(ucs);
                                }

                                var oldUcs = _ed.CurrentUserCoordinateSystem;
                                _ed.CurrentUserCoordinateSystem = Matrix3d.Identity;



                                var arcSide = GeometryUtility.SideOfPointToArc(weldingArc.Center, weldingArc.Radius, sidePoint);

                                var sAng = -1 + weldingArc.Center.DistanceTo(sidePoint) / weldingArc.Radius;

                                _weldingArcs = BuildWeldingArcsOverArc(weldingArc.Center,
                                                                      weldingArc.Radius,
                                                                      weldingArc.StartAngle,
                                                                      weldingArc.EndAngle,
                                                                      weldingArcsChordRslt.Value,
                                                                      weldingArcsOffsetRslt.Value,
                                                                      weldingArcsOffsetRslt.Value / 2,
                                                                      arcSide);

                                DrawWeldingLine(_weldingArcs, T);

                                _ed.CurrentUserCoordinateSystem = oldUcs;

                                foreach (Arc a in _weldingArcs)
                                {
                                    a.TransformBy(ucs.Inverse());
                                }

                                weldingLine.TransformBy(ucs.Inverse());

                                T.Commit();
                            }
                        }
                        else
                        {
                            MessageBox.Show("\nYou did not slected object of type Arc. Try again...");
                        }
                        tr.Commit();
                    }
                    break;
                #endregion
            }
            #endregion

        }

        private DBObjectCollection BuildWeldingArcsOverArc(Point3d arcCenter, double aArcRadius, double aArcStartAngle, double aArcEndAngle, double aArcsChord, double aArcsDistance, double aArcsOffset, ArcSide aArcSide)
        {
            double weldingLenght, offda, ang1, ang2, ang3 = 0, radius, c, da, r, size, space, offset;
            Point3d pc, p1, p2, p3, pCarc = new Point3d(0, 0, 0);

            var localWeldingArcs = new DBObjectCollection();

            pc = arcCenter;
            r = aArcRadius;
            ang1 = aArcStartAngle;
            ang2 = aArcEndAngle;

            size = aArcsChord;
            space = aArcsDistance;
            offset = aArcsOffset;

            // if the start angle is greater, we need to add 360 deg. in order to keep the rotation counter clockwise
            // that means we draw arcs from the smaller to the bigger deg. 
            // If the EndingAngle is smaller, then we will make the other side, not out side, because
            // we go from SmallerAngle to LargerAngle, not from StartAngle to EndAngle !!
            // Draw yourself a sketch if you can not get it the first time.
            if (ang2 < ang1)
            {
                ang2 = ang2 + 2 * Math.PI;
            }

            // The lenght of the welding is the lenght of the arc
            weldingLenght = (ang2 - ang1) * r;

            // convert the linear space between the welding arcs 
            // into degrees
            da = space / r;

            offda = offset / r;

            if (aArcSide == ArcSide.ArcSideOutside)
            {
                ang3 = 0;
            }
            else if (aArcSide == ArcSide.ArcSideInside)
            {
                ang3 = Math.PI;
            }
            else
            {
                ang3 = 0;
            }

            radius = size * 0.625;
            c = size * 0.375;

            if (aArcSide == ArcSide.ArcSideOutside)
            {
                ang1 = ang1 + offda;
                while (ang1 < ang2)
                {
                    p1 = GeometryUtility.GetPlanePolarPoint(pc, ang1, r);
                    p3 = GeometryUtility.GetPlanePolarPoint(p1, ang1 + ang3, size);
                    p2 = GeometryUtility.GetPlanePolarPoint(p1, ang1 + ang3, size / 2);
                    pCarc = GeometryUtility.GetPlanePolarPoint(p2, ang1 + ang3 + GeometryUtility.DoubleToRadians(90), c);
                    localWeldingArcs.Add(new Arc(new Point3d(), 
                        radius,
                        GeometryUtility.GetAngleFromXAxis(pCarc, p1),
                        GeometryUtility.GetAngleFromXAxis(pCarc, p3)) {Center = pCarc}/*BricsCAD hack*/);
                    ang1 = ang1 + da;
                }
            }
            else if (aArcSide == ArcSide.ArcSideInside)
            {
                ang2 = ang2 - offda;
                while (ang2 > ang1)
                {
                    p1 = GeometryUtility.GetPlanePolarPoint(pc, ang2, r);
                    p3 = GeometryUtility.GetPlanePolarPoint(p1, ang2 + ang3, size);
                    p2 = GeometryUtility.GetPlanePolarPoint(p1, ang2 + ang3, size / 2);
                    pCarc = GeometryUtility.GetPlanePolarPoint(p2, ang2 + ang3 + GeometryUtility.DoubleToRadians(90), c);
                    localWeldingArcs.Add(new Arc(new Point3d(), 
                                                   radius,
                                                   GeometryUtility.GetAngleFromXAxis(pCarc, p1),
                                                   GeometryUtility.GetAngleFromXAxis(pCarc, p3)) {Center = pCarc}/*BricsCAD hack*/);
                    ang2 = ang2 - da;
                }
            }
            else if (aArcSide == ArcSide.ArcSideMiddle)
            {
                ang2 = ang2 + offda;
                while (ang2 > ang1)
                {
                    p1 = GeometryUtility.GetPlanePolarPoint(pc, ang2, r - aArcsChord / 2);   //   -aArcsChord / 2 places the welding arc in the middle
                    p3 = GeometryUtility.GetPlanePolarPoint(p1, ang2 + ang3, size);
                    p2 = GeometryUtility.GetPlanePolarPoint(p1, ang2 + ang3, size / 2);
                    pCarc = GeometryUtility.GetPlanePolarPoint(p2, ang2 + ang3 + GeometryUtility.DoubleToRadians(90), c);
                    localWeldingArcs.Add(new Arc(pCarc,
                                                 radius,
                                                 GeometryUtility.GetAngleFromXAxis(pCarc, p1),
                                                 GeometryUtility.GetAngleFromXAxis(pCarc, p3)) {Center = pCarc}/*BricsCAD hack*/);
                    ang2 = ang2 - da;
                }
            }
            return localWeldingArcs;
        }
        private DBObjectCollection BuildWeldingArcsOverPts(Point3dCollection aWeldingLinePts, Point3d aSidePt, double aArcsLenght, double aArcsDistance, double aArcsOffset)
        {
            // We should think that Point[i] and Point[i + 1] form a line
            double lenght, ang1, ang2, ang3, pAng3, c, d, radius, distP1Px = 0.0;
            Point3d px, pc, p1, p2, p3, p4 = new Point3d(0, 0, 0);

            p3 = aSidePt;

            var size = aArcsLenght;
            var space = aArcsDistance;
            var offset = aArcsOffset;

            var weldingArcs = new DBObjectCollection();

            for (var i = 0; i < aWeldingLinePts.Count - 1; i++)
            {
                p1 = aWeldingLinePts[i];
                p2 = aWeldingLinePts[i + 1];
                lenght = p1.DistanceTo(p2);

                if (aSidePt.IsEqualTo(new Point3d(0, 0, 0))) // Draw in the middle of the line
                {
                    ang1 = GeometryUtility.GetAngleFromXAxis(p1, p2);
                    ang2 = ang1 - GeometryUtility.DoubleToRadians(90);
                    ang3 = ang1 + GeometryUtility.DoubleToRadians(90);

                    d = aArcsOffset;
                    px = GeometryUtility.GetPlanePolarPoint(p1, ang1, d);
                    radius = size * 0.625;
                    c = size * 0.375;
                    distP1Px = p1.DistanceTo(px);

                    while (distP1Px < lenght)
                    {
                        pc = GeometryUtility.GetPlanePolarPoint(px, ang1, c);
                        p2 = GeometryUtility.GetPlanePolarPoint(pc, ang2, size / 2);
                        p4 = GeometryUtility.GetPlanePolarPoint(pc, ang3, size / 2);
                        var arc = new Arc(pc,
                            radius,
                            GeometryUtility.GetAngleFromXAxis(pc, p4),
                            GeometryUtility.GetAngleFromXAxis(pc, p2));
                        // BricsCAD hack
                        arc.Center = pc;
                        weldingArcs.Add(arc);
                        d = d + space;
                        px = GeometryUtility.GetPlanePolarPoint(p1, ang1, d);
                        distP1Px = p1.DistanceTo(px);
                    }
                }
                else // Side point selected
                {
                    ang1 = GeometryUtility.GetAngleFromXAxis(p1, p2);
                    ang2 = GeometryUtility.GetAngleFromXAxis(p1, p3);

                    var tempLine = new Line(p1, p2);
                    var direction = GeometryUtility.OffsetDirection(tempLine, p3);
                    if (direction == 1)
                    {
                        ang3 = ang1 - GeometryUtility.DoubleToRadians(90.0);
                    }
                    else
                    {
                        ang3 = ang1 + GeometryUtility.DoubleToRadians(90.0);
                    }
                    d = offset;
                    px = GeometryUtility.GetPlanePolarPoint(p1, ang1, d);
                    radius = size * 0.625;
                    c = size * 0.375;
                    distP1Px = p1.DistanceTo(px);
                    while (distP1Px < lenght)
                    {
                        pAng3 = ang3 + GeometryUtility.DoubleToRadians(90);
                        p3 = GeometryUtility.GetPlanePolarPoint(px, ang3, size);
                        p2 = GeometryUtility.GetPlanePolarPoint(px, ang3, size / 2);
                        pc = GeometryUtility.GetPlanePolarPoint(p2, pAng3, c);
                        var arc = new Arc(pc,
                            radius,
                            GeometryUtility.GetAngleFromXAxis(pc, px),
                            GeometryUtility.GetAngleFromXAxis(pc, p3));
                        // BricsCAD hack
                        arc.Center = pc;

                        weldingArcs.Add(arc);
                        d = d + space;
                        px = GeometryUtility.GetPlanePolarPoint(p1, ang1, d);
                        distP1Px = p1.DistanceTo(px);
                    }
                }
            }
            return weldingArcs;
        }

        private Point3dCollection GetPointsFromUser()
        {
            var vertices = new Point3dCollection();
            // Set up the selection options
            // (used for all vertices)
            var verticesOpts = new PromptPointOptions("\nSelect point: ");
            verticesOpts.AllowNone = true;

            // Get the start point for the polyline
            var verticesRslt = _ed.GetPoint(verticesOpts);
            while (verticesRslt.Status == PromptStatus.OK)
            {
                // Add the selected point to the list
                vertices.Add(verticesRslt.Value);

                // Drag a temp line during selection of subsequent points
                verticesOpts.UseBasePoint = true;
                verticesOpts.BasePoint = verticesRslt.Value;
                verticesOpts.UseDashedLine = true;
                verticesRslt = _ed.GetPoint(verticesOpts);
                if (verticesRslt.Status == PromptStatus.OK)
                {
                    // For each point selected, draw a temporary segment
                    // start point, end point, current color, highlighted?
                    _ed.DrawVector(vertices[vertices.Count - 1], verticesRslt.Value, Color.FromColor(System.Drawing.Color.Yellow).ColorIndex, false);
                }
            }
            return vertices;
        }

        /// <summary>
        /// Takes an array of welding arcs 
        /// then creates welding block definition 
        /// appends every welding arc from the arcs array to that definition and to the database
        /// and creates a reference to the definition
        /// </summary>
        /// <summary>
        /// Takes an array of welding arcs 
        /// then creates welding block definition 
        /// appends every welding arc from the arcs array to that definition and to the database
        /// and creates a reference to the definition
        /// </summary>
        /// <param name="weldingArcs">
        /// </param>
        /// <param name="tr"></param>
        private void DrawWeldingLine(DBObjectCollection weldingArcs, Transaction tr)
        {
            var previousLayer = (LayerTableRecord) tr.GetObject(_db.Clayer, OpenMode.ForRead);
            var bt = (BlockTable) tr.GetObject(_db.BlockTableId, OpenMode.ForWrite);
            using (var currentSpace = (BlockTableRecord) tr.GetObject(_db.CurrentSpaceId, OpenMode.ForWrite))
            {
                if (!_drawingHelper.LayerManipulator.LayerExists(Settings.Default.weldingLineLayer))
                {
                    _drawingHelper.LayerManipulator.CreateLayer(Settings.Default.weldingLineLayer,
                        Settings.Default.weldingLineLayerColor);
                }
                _drawingHelper.LayerManipulator.ChangeLayer(Settings.Default.weldingLineLayer);

                var weldingLineDefinition = new BlockTableRecord();
                var nameSalt = DateTime.Now.GetHashCode().ToString();
                weldingLineDefinition.Name = "WeldingLine" + nameSalt;

                bt.Add(weldingLineDefinition);
                tr.AddNewlyCreatedDBObject(weldingLineDefinition, true);

                foreach (DBObject arcObject in weldingArcs)
                {
                    var mArc = (Arc) arcObject;
                    mArc.Layer = "0";
                    weldingLineDefinition.AppendEntity(mArc);
                    tr.AddNewlyCreatedDBObject(arcObject, true);
                }

                var weldingLineRef = new BlockReference(new Point3d(0, 0, 0), weldingLineDefinition.ObjectId);

                currentSpace.AppendEntity(weldingLineRef);
                tr.AddNewlyCreatedDBObject(weldingLineRef, true);

                _drawingHelper.LayerManipulator.ChangeLayer(previousLayer.Name);
            }
        }

        public void Dispose()
        {
            _weldingArcs.Dispose();
            _weldingVertices.Dispose();
        }
    }
}
