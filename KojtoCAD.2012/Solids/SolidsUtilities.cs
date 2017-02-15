using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using System;
using System.Windows.Forms;
using KojtoCAD.Mathematics.Geometry;
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

[assembly: CommandClass(typeof(KojtoCAD.Solids.SolidsUtilities))]

namespace KojtoCAD.Solids
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SolidsUtilities
    {
        private readonly Editor _ed = Application.DocumentManager.MdiActiveDocument.Editor;
        private readonly Database _db = Application.DocumentManager.MdiActiveDocument.Database;
        private readonly EditorHelper _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);

        #region Solid projection funx

        static ObjectId[] solids_buff = { new ObjectId(), new ObjectId(), new ObjectId(), new ObjectId(), new ObjectId(), new ObjectId(), new ObjectId() };
        static Point3d _centroid = new Point3d();
        static Point3d _target = new Point3d();
        static Vector3d[] vO = { new Vector3d(), new Vector3d(), new Vector3d() };

        /// <summary>
        /// Solid Projection
        /// </summary>
        [CommandMethod("spr")]
        public void SolidProjectionStart()   //IfTheFigureIsInAssembly
        {
            
            _ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
            string command = "_copybase 0,0,0 all  ";
            Utilities.CommandLineHelper.ExecuteStringOverInvoke(command);
            Utilities.CommandLineHelper.ExecuteStringOverInvoke("sprp ");
        }

        [CommandMethod("sprp")]
        public void SolidProjectionIfTheFigureIsASeparateDrawingtNotUsedDirectlyStart()
        {
            
            #region Solid Selection
            // Solid collections
            ObjectIdCollection SolidCollection = new ObjectIdCollection();

            // Prompt for Solid
            PromptEntityOptions ObjectSelectionOpts = new PromptEntityOptions("\nSelect Solid : ");
            ObjectSelectionOpts.AllowNone = false;
            ObjectSelectionOpts.SetRejectMessage("\nEntity is not a solid");
            ObjectSelectionOpts.AddAllowedClass(typeof(Solid), false);
            ObjectSelectionOpts.AddAllowedClass(typeof(Solid3d), false);
            PromptEntityResult ObjectSelectionRslt = _ed.GetEntity(ObjectSelectionOpts);

            if (ObjectSelectionRslt.Status == PromptStatus.OK)
            {
                SolidCollection.Add(ObjectSelectionRslt.ObjectId);
            }
            else
            {
                return;
            }

            if (SolidCollection.Count == 0)
            {
                MessageBox.Show("Solid selection is empty. Aborting...");
                return;
            }
            if (SolidCollection.Count > 1)
            {
                MessageBox.Show("Solid selection Count > 1. Aborting...");
                return;
            }
            #endregion

            #region Origin Point selection
            PromptPointOptions OriginPointOptions = new PromptPointOptions("\n\rPick origin point : ");
            PromptPointResult OriginPointResult = _ed.GetPoint(OriginPointOptions);
            if (OriginPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            #endregion

            #region XAxe Point selection
            PromptPointOptions XAxePointOptions = new PromptPointOptions("\n\rPick X Axe point : ");
            PromptPointResult XAxePointResult = _ed.GetPoint(XAxePointOptions);
            if (XAxePointResult.Status != PromptStatus.OK)
            {
                return;
            }
            #endregion

            #region YAxe Point selection
            PromptPointOptions YAxePointOptions = new PromptPointOptions("\n\rPick Y Axe point : ");
            PromptPointResult YAxePointResult = _ed.GetPoint(YAxePointOptions);
            if (YAxePointResult.Status != PromptStatus.OK)
            {
                return;
            }
            #endregion

            #region Standard selection
            // Prompt the user to select scale

            PromptKeywordOptions StandardOpts = new PromptKeywordOptions("");
            StandardOpts.Message = "\nEnter standard : ";
            StandardOpts.Keywords.Add("American");
            StandardOpts.Keywords.Add("BDS");
            StandardOpts.AllowNone = false;
            PromptResult StandardRslt = _ed.GetKeywords(StandardOpts);
            // If the user pressed cancel - return with no error
            if (StandardRslt.Status != PromptStatus.OK)
            {
                return;
            }
            bool BDS = (StandardRslt.StringResult == "BDS") ? true : false;
            #endregion

            #region Target Point selection
            PromptPointOptions TargetPointOptions = new PromptPointOptions("\n\rPick target point: ");
            PromptPointResult TargetPointResult = _ed.GetPoint(TargetPointOptions);
            if (TargetPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            #endregion

            #region
            using (Transaction acTrans = _db.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(this._db.BlockTableId, OpenMode.ForRead) as BlockTable;

                var modelSpace = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                foreach (ObjectId ent in modelSpace)
                {
                    bool exist = false;
                    foreach (ObjectId id in SolidCollection)
                    {
                        if (id == ent)
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (!exist)
                    {
                        Entity acEnt = acTrans.GetObject(ent, OpenMode.ForWrite) as Entity;
                        acEnt.Erase();
                    }
                }

                acTrans.Commit();
            }
            #endregion

            DrawSolidDescription(SolidCollection, OriginPointResult.Value, XAxePointResult.Value, YAxePointResult.Value, TargetPointResult, BDS);
            KojtoCAD.Utilities.CommandLineHelper.ExecuteStringOverInvoke("_pasteclip 0,0 ");
            KojtoCAD.Utilities.CommandLineHelper.ExecuteStringOverInvoke("_ucs pre ");
        }

        [CommandMethod("hideflat")]
        public void HideflatNotUsedDirectlyStart()
        {
            
            using (Transaction acTrans = _db.TransactionManager.StartTransaction())
            {
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (ObjectId acObjId in acLyrTbl)
                {
                    LayerTableRecord acLyrTblRec;
                    acLyrTblRec = acTrans.GetObject(acObjId, OpenMode.ForWrite) as LayerTableRecord;
                    String name = acLyrTblRec.Name;
                    if (name.Contains("PH-"))
                    {
                        acLyrTblRec.IsOff = true;
                    }
                }

                acTrans.Commit();
            }
        }
        [CommandMethod("eraseflat")]
        public void EraseflatNotUsedDirectlyStart()
        {
            
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction acTrans = Db.TransactionManager.StartTransaction())
            {
                for (int i = 1; i < 7; i++)
                {
                    Solid3d ent = acTrans.GetObject(solids_buff[i], OpenMode.ForWrite) as Solid3d;
                    //ent.Visible = false;
                    ent.Erase();
                }

                acTrans.Commit();
            }
        }
        [CommandMethod("finalflat")]
        public void FinalflatNotUsedDirectlyStart()
        {
            
            Database Db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction acTrans = Db.TransactionManager.StartTransaction())
            {

                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Solid3d ent = acTrans.GetObject(solids_buff[0], OpenMode.ForWrite) as Solid3d;
                ent.Erase();

                acTrans.Commit();
            }
        }

        public static void DrawSolidDescription(ObjectIdCollection OuterContours, Point3d origin, Point3d Xaxe, Point3d Yaxe, PromptPointResult target, bool bds)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;  

            Vector3d vX, vY, vZ;
            ObjectId ID = OuterContours[OuterContours.Count - 1];
            Point3d centroid = new Point3d();
            string layer;
            ObjectId[] solids = { ID, ID, ID, ID, ID, ID, ID };
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Entity ent = acTrans.GetObject(ID, OpenMode.ForRead) as Entity;

                #region check selection
                String[] str = ent.GetType().ToString().Split('.');
                if (str[str.Length - 1] != "Solid3d")//check for solid
                {
                    MessageBox.Show("No selected Solid !");
                    return;
                }

                Quaternion q1 = new Quaternion(Xaxe.X - origin.X, Xaxe.Y - origin.Y, Xaxe.Z - origin.Z, 0);
                Quaternion q2 = new Quaternion(Yaxe.X - origin.X, Yaxe.Y - origin.Y, Yaxe.Z - origin.Z, 0);

                if (q1.abs() < 10E-11)
                {
                    MessageBox.Show("origin = Xaxe !");
                    return;
                }

                if (q2.abs() < 10E-11)
                {
                    MessageBox.Show("origin = Xaxe !");
                    return;
                }


                q1 /= q1.abs();
                q2 /= q2.abs();

                Quaternion q = q2 / q1;
                q = new Quaternion(0, q.GetX(), q.GetY(), q.GetZ());
                if (q.norm() < 10E-14)
                {
                    MessageBox.Show("three points in one line !");
                    return;
                }
                #endregion

                Solid3d solid = acTrans.GetObject(ID, OpenMode.ForWrite) as Solid3d;
                layer = solid.Layer;
                solids[0] = solid.ObjectId;

                centroid = solid.MassProperties.Centroid;

                UCS UCS = new UCS(new Quaternion(0, origin.X, origin.Y, origin.Z), new Quaternion(0, Xaxe.X, Xaxe.Y, Xaxe.Z), new Quaternion(0, Yaxe.X, Yaxe.Y, Yaxe.Z));
                Quaternion ucsX = UCS.ToACS(new Quaternion(0, 1, 0, 0));
                Quaternion ucsY = UCS.ToACS(new Quaternion(0, 0, 1, 0));
                Quaternion ucsZ = UCS.ToACS(new Quaternion(0, 0, 0, 1));
                vX = origin.GetVectorTo(new Point3d(ucsX.GetX(), ucsX.GetY(), ucsX.GetZ()));
                vY = origin.GetVectorTo(new Point3d(ucsY.GetX(), ucsY.GetY(), ucsY.GetZ()));
                vZ = origin.GetVectorTo(new Point3d(ucsZ.GetX(), ucsZ.GetY(), ucsZ.GetZ()));

                _centroid = centroid;
                vO[0] = vX; vO[1] = vY; vO[2] = vZ;
                _target = target.Value;

                Matrix3d newMatrix = new Matrix3d();
                newMatrix = Matrix3d.AlignCoordinateSystem(centroid, vX, vY, vZ, centroid, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis);
                solid.TransformBy(newMatrix);


                Point3d MIN = new Point3d(solid.GeometricExtents.MinPoint.X - centroid.X, solid.GeometricExtents.MinPoint.Y - centroid.Y, solid.GeometricExtents.MinPoint.Z - centroid.Z);
                Point3d MAX = new Point3d(solid.GeometricExtents.MaxPoint.X - centroid.X, solid.GeometricExtents.MaxPoint.Y - centroid.Y, solid.GeometricExtents.MaxPoint.Z - centroid.Z);

                double MaxX = (Math.Abs(MIN.X) > Math.Abs(MAX.X)) ? Math.Abs(MIN.X) : Math.Abs(MAX.X);
                double MaxY = (Math.Abs(MIN.Y) > Math.Abs(MAX.Y)) ? Math.Abs(MIN.Y) : Math.Abs(MAX.Y);
                double MaxZ = (Math.Abs(MIN.Z) > Math.Abs(MAX.Z)) ? Math.Abs(MIN.Z) : Math.Abs(MAX.Z);


                Solid3d[] solidARR = { solid.Clone() as Solid3d, solid.Clone() as Solid3d, solid.Clone() as Solid3d, solid.Clone() as Solid3d, solid.Clone() as Solid3d };

                for (int i = 0; i < 5; i++)
                {
                    modelSpace.AppendEntity(solidARR[i]);
                    acTrans.AddNewlyCreatedDBObject(solidARR[i], true);
                }

                Vector3d vRot1 = centroid.GetVectorTo(new Point3d(centroid.X, centroid.Y + 2.5 * Math.Abs(MAX.X), centroid.Z));
                Vector3d vRot2 = centroid.GetVectorTo(new Point3d(centroid.X + 2.5 * Math.Abs(MAX.X), centroid.Y, centroid.Z));
                Vector3d vRot3 = centroid.GetVectorTo(new Point3d(centroid.X, centroid.Y, centroid.Z + 2.5 * Math.Abs(MAX.Z)));


                if (bds)
                {
                    solidARR[0].TransformBy(Matrix3d.Rotation(-Math.PI / 2.0, vRot1, centroid));
                    solidARR[0].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X - MaxZ - 1.5 * MaxX, centroid.Y, centroid.Z))));
                    solids[1] = solidARR[0].ObjectId;
                    solidARR[1].TransformBy(Matrix3d.Rotation(Math.PI / 2.0, vRot1, centroid));
                    solidARR[1].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X + MaxZ + 1.5 * MaxX, centroid.Y, centroid.Z))));
                    solids[2] = solidARR[1].ObjectId;
                    solidARR[2].TransformBy(Matrix3d.Rotation(Math.PI, vRot1, centroid));
                    solidARR[2].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X + 3 * MaxZ + 2.5 * MaxX, centroid.Y, centroid.Z))));
                    solids[3] = solidARR[2].ObjectId;
                    solidARR[3].TransformBy(Matrix3d.Rotation(Math.PI / 2, vRot2, centroid));
                    solidARR[3].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X, centroid.Y - MaxZ - 1.5 * MaxY, centroid.Z))));
                    solids[4] = solidARR[3].ObjectId;
                    solidARR[4].TransformBy(Matrix3d.Rotation(-Math.PI / 2, vRot2, centroid));
                    solidARR[4].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X, centroid.Y + MaxZ + 1.5 * MaxY, centroid.Z))));
                    solids[5] = solidARR[4].ObjectId;
                }
                else
                {
                    solidARR[0].TransformBy(Matrix3d.Rotation(-Math.PI / 2.0, vRot1, centroid));
                    solidARR[0].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X + MaxZ + 1.5 * MaxX, centroid.Y, centroid.Z))));
                    solids[1] = solidARR[0].ObjectId;

                    solidARR[1].TransformBy(Matrix3d.Rotation(Math.PI / 2.0, vRot1, centroid));
                    solidARR[1].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X - MaxZ - 1.5 * MaxX, centroid.Y, centroid.Z))));
                    solids[2] = solidARR[1].ObjectId;

                    solidARR[2].TransformBy(Matrix3d.Rotation(Math.PI, vRot1, centroid));
                    solidARR[2].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X + 3 * MaxZ + 2.5 * MaxX, centroid.Y, centroid.Z))));
                    solids[3] = solidARR[2].ObjectId;
                    solidARR[3].TransformBy(Matrix3d.Rotation(Math.PI / 2, vRot2, centroid));
                    solidARR[3].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X, centroid.Y + MaxZ + 1.5 * MaxY, centroid.Z))));
                    solids[4] = solidARR[3].ObjectId;
                    solidARR[4].TransformBy(Matrix3d.Rotation(-Math.PI / 2, vRot2, centroid));
                    solidARR[4].TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X, centroid.Y - MaxZ - 1.5 * MaxY, centroid.Z))));
                    solids[5] = solidARR[4].ObjectId;
                }

                Solid3d solid1 = solid.Clone() as Solid3d;
                modelSpace.AppendEntity(solid1);
                acTrans.AddNewlyCreatedDBObject(solid1, true);

                solids[6] = solid1.ObjectId;

                solid1.TransformBy(Matrix3d.Rotation(Math.PI / 4, vRot3, centroid));
                solid1.TransformBy(Matrix3d.Rotation(-Math.PI / 3.28854, vRot2, centroid));

                double dX = Math.Abs(solid1.GeometricExtents.MinPoint.X - centroid.X) + Math.Abs(solid.GeometricExtents.MaxPoint.X - centroid.X);
                double dY = Math.Abs(solid1.GeometricExtents.MinPoint.Y - centroid.Y) + Math.Abs(solid.GeometricExtents.MaxPoint.Y - centroid.Y);
                solid1.TransformBy(Matrix3d.Displacement(centroid.GetVectorTo(new Point3d(centroid.X + dX + 1.5 * MaxX, centroid.Y + dY + 1.5 * MaxY, 0))));

                acTrans.Commit();
            }

            for (int i = 0; i < 7; i++)
            {
                solids_buff[i] = solids[i];
            }


            doc.SendStringToExecute("-view\rO\rT\r", false, false, false);
            doc.SendStringToExecute("TILEMODE\r0\r", false, false, false);
            // Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("TILEMODE", 0);
            // Doc.Editor.SwitchToPaperSpace();
            doc.SendStringToExecute("MSPACE\r", false, false, false);
            doc.SendStringToExecute("SOLPROF\rall\r\rY\rY\rY\r", false, false, false);
            doc.SendStringToExecute("PSPACE\r", false, false, false);
            doc.SendStringToExecute("TILEMODE\r1\r", false, false, false);
            //Doc.SendStringToExecute("ZOOM\rE\r" , false , false , false);
            doc.SendStringToExecute("hideflat\r", false, false, false);

            if (target.Status == PromptStatus.OK)
            {
                string bs = centroid.X.ToString() + "," + centroid.Y.ToString();
                string ts = target.Value.X.ToString() + "," + target.Value.Y.ToString();

                doc.SendStringToExecute("move\rlast\r\r" + bs + "\r" + ts + "\r", false, false, false);
                doc.SendStringToExecute("ZOOM\rE\r", false, false, false);

            }

            doc.SendStringToExecute("eraseflat\r", false, false, false);
            doc.SendStringToExecute("finalflat\r", false, false, false);
            doc.SendStringToExecute("ZOOM\rE\r", false, false, false);
        }
        #endregion
    }
}
