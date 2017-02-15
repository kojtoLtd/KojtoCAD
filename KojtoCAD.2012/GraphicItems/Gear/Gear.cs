using Castle.Core.Logging;
using KojtoCAD.Utilities;
using System;
using System.Windows.Forms;
using KojtoCAD.Mathematics.Geometry;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
#endif
[assembly: CommandClass(typeof(KojtoCAD.GraphicItems.Gear.Gear))]

namespace KojtoCAD.GraphicItems.Gear
{
    public class Gear
    {
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly EditorHelper _editorHelper;
        private readonly Database _db = Application.DocumentManager.MdiActiveDocument.Database;

        public Gear()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        ///  Draw gear
        /// </summary>
        [CommandMethod("drawgear")]
        public void GearsStart()
        {

            ImportGearForm importGearForm = new ImportGearForm();
            if (importGearForm.ShowDialog() == DialogResult.OK)
            {
                var basePointResult = _editorHelper.PromptForPoint("Pick base point : ", false, false);
                if (basePointResult.Status != PromptStatus.OK)
                {
                    return;
                }

                if (importGearForm.DrawItem == "GEAR")
                {
                    DrawGear(basePointResult.Value, (double)importGearForm.gearModule, (int)importGearForm.gearTeethCount);
                }
                else if (importGearForm.DrawItem == "RACK")
                {
                    DrawRack(basePointResult.Value, (double)importGearForm.gearModule, (int)importGearForm.gearTeethCount);
                }
                _logger.Info(System.Reflection.MethodBase.GetCurrentMethod().Name);

            }
            importGearForm.Close();
        }

        private void DrawGear(Point3d BasePoint, double gearModule, int gearTeethCount)
        {
            Point3d oldBase = BasePoint;
            BasePoint = new Point3d(0, 0, 0);
            Vector3d mov = new Vector3d(oldBase.X, oldBase.Y, oldBase.Z);
            mov = mov.TransformBy(_editorHelper.CurrentUcs);

            double rp = (gearModule * gearTeethCount) / 2; // Primitive radius = (gearModule * gearTeethCount) / 2
            //double gamma = Math.PI * gearModule / (2 * rp);
            double rrc = rp - 1.2 * gearModule; // Roor Radius 
            double rr = rrc; // Roor Radius 
            double rb = rp - gearModule; // Base Radius
            double re = rp + gearModule; // External Radius


            using (Transaction tr = _db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(_db.BlockTableId, OpenMode.ForWrite);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                BlockTableRecord Gear = new BlockTableRecord();
                string nameSalt = DateTime.Now.GetHashCode().ToString();
                Gear.Name = "Gear" + nameSalt;
                Gear.Origin = BasePoint;

                bt.Add(Gear);
                tr.AddNewlyCreatedDBObject(Gear, true);

                ObjectId Layer20 = _drawingHelper.LayerManipulator.CreateLayer("2-0", System.Drawing.Color.Yellow);
                ObjectId Layer32 = _drawingHelper.LayerManipulator.CreateLayer("3-2", System.Drawing.Color.Lime);

                Circle RbCircle = new Circle(BasePoint, Vector3d.ZAxis, rb);
                Circle RrCircle = new Circle(BasePoint, Vector3d.ZAxis, rr);
                Circle RpCircle = new Circle(BasePoint, Vector3d.ZAxis, rp);
                Circle ReCircle = new Circle(BasePoint, Vector3d.ZAxis, re);

                RbCircle.SetLayerId(Layer20, false);
                RrCircle.SetLayerId(Layer20, false);
                RpCircle.SetLayerId(Layer32, false);
                ReCircle.SetLayerId(Layer20, false);

                Gear.AppendEntity(RbCircle);
                Gear.AppendEntity(RrCircle);
                Gear.AppendEntity(RpCircle);
                Gear.AppendEntity(ReCircle);

                Polyline3d GearPolyLine = GetGearPolyline(BasePoint, gearModule, gearTeethCount);
                GearPolyLine.SetLayerId(Layer20, false);
                Gear.AppendEntity(GearPolyLine);

                tr.AddNewlyCreatedDBObject(RbCircle, true);
                tr.AddNewlyCreatedDBObject(RrCircle, true);
                tr.AddNewlyCreatedDBObject(RpCircle, true);
                tr.AddNewlyCreatedDBObject(ReCircle, true);

                tr.AddNewlyCreatedDBObject(GearPolyLine, true);

                BlockReference GearRef = new BlockReference(BasePoint, Gear.ObjectId);
                GearRef.Position = BasePoint;
                GearRef.TransformBy(_editorHelper.CurrentUcs);
                GearRef.TransformBy(Matrix3d.Displacement(mov));
                BlockTableRecord currSpace = (BlockTableRecord)tr.GetObject(_db.CurrentSpaceId, OpenMode.ForWrite);
                currSpace.AppendEntity(GearRef);

                tr.AddNewlyCreatedDBObject(GearRef, true);
                tr.Commit();
            }

        }

        private void DrawRack(Point3d BasePoint, double gearModule, int gearTeethCount)
        {
            Point3d oldBase = BasePoint;
            BasePoint = new Point3d(0, 0, 0);
            Vector3d mov = new Vector3d(oldBase.X, oldBase.Y, oldBase.Z);
            mov = mov.TransformBy(_editorHelper.CurrentUcs);

            double toothAngle = 70.0;
            double angle1 = Math.Sin(20.0 * (Math.PI / 180.0));
            double angle2 = Math.Cos(20.0 * (Math.PI / 180.0));

            double toothHead = gearModule * (Math.PI / 2) - ((angle1 * 2) * (1 / angle2));
            double toothValley = gearModule * (Math.PI / 2) - ((angle1 * 2.4) * (1 / angle2));

            double toothLenght = gearModule * 2.2 / angle2;
            double toothLenght2 = gearModule * gearTeethCount * Math.PI;

            Polyline rackBasePoly = new Polyline();
            rackBasePoly.AddVertexAt(0, new Point2d(BasePoint.X, BasePoint.Y + 1.2 * gearModule), 0.0, 0.0, 0.0);
            rackBasePoly.AddVertexAt(1, new Point2d(BasePoint.X + toothLenght2, BasePoint.Y + 1.2 * gearModule), 0.0, 0.0, 0.0);

            Polyline3d rackGearPoly = new Polyline3d();
            rackGearPoly = GetRackGearPoly(BasePoint, toothLenght, toothHead, toothValley, toothAngle, gearTeethCount);
            #region Transaction - set the layers and insert the created polylines in the DWG

            Transaction tr = _db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(_db.BlockTableId, OpenMode.ForWrite);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                BlockTableRecord GearRack = new BlockTableRecord();
                string nameSalt = DateTime.Now.GetHashCode().ToString();
                GearRack.Name = "GearRack" + nameSalt;
                GearRack.Origin = BasePoint;

                bt.Add(GearRack);
                tr.AddNewlyCreatedDBObject(GearRack, true);

                ObjectId Layer20 = _drawingHelper.LayerManipulator.CreateLayer("2-0", System.Drawing.Color.Yellow);
                ObjectId Layer32 = _drawingHelper.LayerManipulator.CreateLayer("3-2", System.Drawing.Color.Lime);

                rackGearPoly.SetLayerId(Layer20, false);
                rackBasePoly.SetLayerId(Layer32, false);

                GearRack.AppendEntity(rackGearPoly);
                GearRack.AppendEntity(rackBasePoly);

                tr.AddNewlyCreatedDBObject(rackGearPoly, true);
                tr.AddNewlyCreatedDBObject(rackBasePoly, true);


                BlockReference GearRackRef = new BlockReference(BasePoint, GearRack.ObjectId);
                GearRackRef.Position = BasePoint;
                GearRackRef.TransformBy(_editorHelper.CurrentUcs);
                GearRackRef.TransformBy(Matrix3d.Displacement(mov));
                BlockTableRecord currSpace = (BlockTableRecord)tr.GetObject(_db.CurrentSpaceId, OpenMode.ForWrite);
                currSpace.AppendEntity(GearRackRef);

                tr.AddNewlyCreatedDBObject(GearRackRef, true);
                tr.Commit();
            }
            #endregion
        }

        private Polyline3d GetRackGearPoly(Point3d startPoint, double toothLenght, double toothHead, double toothValley, double toothAngle, int gearTeethCount)
        {
            Point3dCollection gearRackPolyPts = new Point3dCollection();
            gearRackPolyPts.Add(startPoint);
            // The tooth is drawn with 5 points. We always get the previous point draw the next point : gearRackPolyPts.Count-1 !
            for (int toothIndex = 0; toothIndex < gearTeethCount; toothIndex++)
            {
                gearRackPolyPts.Add(GeometryUtility.GetPlanePolarPoint(gearRackPolyPts[gearRackPolyPts.Count - 1], toothAngle * (Math.PI / 180.0), toothLenght));
                gearRackPolyPts.Add(GeometryUtility.GetPlanePolarPoint(gearRackPolyPts[gearRackPolyPts.Count - 1], 0.0, toothHead));
                gearRackPolyPts.Add(GeometryUtility.GetPlanePolarPoint(gearRackPolyPts[gearRackPolyPts.Count - 1], -toothAngle * (Math.PI / 180.0), toothLenght));
                gearRackPolyPts.Add(GeometryUtility.GetPlanePolarPoint(gearRackPolyPts[gearRackPolyPts.Count - 1], 0.0, toothValley));
            }
            return new Polyline3d(Poly3dType.SimplePoly, gearRackPolyPts, false);
        }

        /// <summary>
        /// The theory behind this function is in : 
        /// Algorithm_to_Describe_the_Ideal_Spur_Gear_Profile.pdf
        /// in the project_directory/articles/ folder.
        /// </summary>
        /// <param name="BasePoint"></param>
        /// <param name="gearModule"></param>
        /// <param name="gearTeethCount"></param>
        /// <returns></returns>
        private Polyline3d GetGearPolyline(Point3d BasePoint, double gearModule, int gearTeethCount)
        {
            double mGearTeethCount = (double)gearTeethCount;

            double primitiveRadius = (gearModule * mGearTeethCount) / 2; // Primitive radius = (gearModule * gearTeethCount) / 2
            double gamma = Math.PI * gearModule / (2 * primitiveRadius);
            double rootRadiusConst = primitiveRadius - 1.2 * gearModule; // Roor Radius 
            double rootRadius = rootRadiusConst; // Root Radius 
            double baseRadius = primitiveRadius - gearModule; // Base Radius
            double externalRadius = primitiveRadius + gearModule; // External Radius

            /*
             * are = angle alpha to the External Radius
             * tre = angle teta to the External Radius
             * arp = angle alpha to the Primitive Radius
       
            Short formulas.
            double are = Math.Sqrt((re ^ 2 - rb ^ 2) / (rb ^ 2)) - Math.Atan(Sqr((re ^ 2 - rb ^ 2) / (rb ^ 2)));
            double tre = Math.Sqrt((re ^ 2 - rb ^ 2) / (rb ^ 2));
            double arp = Math.Sqrt((rp ^ 2 - rb ^ 2) / (rb ^ 2)) - Math.Atan(Sqr((rp ^ 2 - rb ^ 2) / (rb ^ 2)));
            */

            double are = Math.Sqrt((Math.Pow(externalRadius, 2) - Math.Pow(baseRadius, 2)) / Math.Pow(baseRadius, 2.0)) - Math.Atan(Math.Sqrt((Math.Pow(externalRadius, 2) - Math.Pow(baseRadius, 2)) / (Math.Pow(baseRadius, 2))));
            double tre = Math.Sqrt((Math.Pow(externalRadius, 2) - Math.Pow(baseRadius, 2)) / Math.Pow(baseRadius, 2));
            double arp = Math.Sqrt((Math.Pow(primitiveRadius, 2) - Math.Pow(baseRadius, 2)) / Math.Pow(baseRadius, 2)) - Math.Atan(Math.Sqrt((Math.Pow(primitiveRadius, 2) - Math.Pow(baseRadius, 2)) / Math.Pow(baseRadius, 2)));

            double epsilon = are - arp;
            double se = (gamma - 2 * epsilon);
            double sigma = se + 2 * are;
            double tau = 360.0 / mGearTeethCount;
            double taurot = 0;
            double x = 0;
            double xi = 0;
            double y = 0;
            double yi = 0;
            double r = 0;

            bool sectorIsDone = false;
            bool gearIsDone = false;

            Point3dCollection GearPolyPoints = new Point3dCollection();
            //IntegerCollection bulgeIndices = new IntegerCollection();

            uint k = 0;
            do
            {
                double teta = 0;
                #region Sector A
                //'========= Sector A ========='      
                double stepA = 0.1;
                sectorIsDone = false;
                x = rootRadiusConst;
                y = 0;



                do
                {
                    GearPolyPoints.Add(new Point3d(rotationX(taurot, x, y) + BasePoint.X, rotationY(taurot, x, y) + BasePoint.Y, 0));
                    x = x + stepA;
                    k = k + 1;
                    if (x > baseRadius)
                    {
                        sectorIsDone = true;
                    }
                } while (!sectorIsDone);
                #endregion

                #region Sector B
                //'======== Sector B ========='
                double stepB = 0.1;
                sectorIsDone = false;
                do
                {
                    x = baseRadius * Math.Cos(teta * Math.PI / 180) + baseRadius * teta * Math.PI / 180 * Math.Sin(teta * Math.PI / 180);
                    y = baseRadius * Math.Sin(teta * Math.PI / 180) - baseRadius * teta * Math.PI / 180 * Math.Cos(teta * Math.PI / 180);
                    GearPolyPoints.Add(new Point3d(rotationX(taurot, x, y) + BasePoint.X, rotationY(taurot, x, y) + BasePoint.Y, 0));

                    teta = teta + stepB;
                    k = k + 1;
                    if (Math.Atan(y / x) > are)
                    {
                        sectorIsDone = true;
                        // Tuk mahame poslednata tochka, zashtoto tq preskacha s mnogo radiusa na vunshnata okrujnost i stava vruhche.
                        GearPolyPoints.RemoveAt(GearPolyPoints.Count - 1);
                    }
                } while (!sectorIsDone);
                #endregion

                #region Sector C
                //'========= Sector C ========='
                teta = are * 180 / Math.PI;
                double stepC = 0.1;
                sectorIsDone = false;
                do
                {
                    x = externalRadius * Math.Cos(teta * Math.PI / 180);
                    y = externalRadius * Math.Sin(teta * Math.PI / 180);
                    GearPolyPoints.Add(new Point3d(rotationX(taurot, x, y) + BasePoint.X, rotationY(taurot, x, y) + BasePoint.Y, 0));
                    teta = teta + stepC;
                    k = k + 1;
                    if (teta > (are + se) * 180 / Math.PI)
                    {
                        sectorIsDone = true;
                    }
                } while (!sectorIsDone);
                #endregion

                #region Sector D
                //'========= Sector D ========='
                teta = tre * 180 / Math.PI;
                double stepD = 0.1;
                sectorIsDone = false;
                do
                {
                    xi = baseRadius * Math.Cos(teta * Math.PI / 180) + baseRadius * teta * Math.PI / 180 * Math.Sin(teta * Math.PI / 180);
                    yi = -(baseRadius * Math.Sin(teta * Math.PI / 180) - baseRadius * teta * Math.PI / 180 * Math.Cos(teta * Math.PI / 180));
                    x = rotationX(sigma * 180 / Math.PI, xi, yi);
                    y = rotationY(sigma * 180 / Math.PI, xi, yi);
                    GearPolyPoints.Add(new Point3d(rotationX(taurot, x, y) + BasePoint.X, rotationY(taurot, x, y) + BasePoint.Y, 0));
                    teta = teta - stepD;
                    k = k + 1;
                    if (teta < 0)
                    {
                        sectorIsDone = true;
                    }
                } while (!sectorIsDone);
                #endregion

                #region Sector E
                //'========= Sector E ========='
                double stepE = 0.1;
                sectorIsDone = false;
                do
                {
                    GearPolyPoints.Add(new Point3d(rotationX(taurot, x, y) + BasePoint.X, rotationY(taurot, x, y) + BasePoint.Y, 0));
                    x = x - stepE;
                    y = x * Math.Tan(sigma);
                    k = k + 1;
                    r = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                    if (r < rootRadiusConst)
                    {
                        sectorIsDone = true;
                    }
                } while (!sectorIsDone);
                #endregion

                #region Sector F
                //'========= Sector F ========='
                double stepF = 0.1;
                teta = sigma * 180 / Math.PI;
                sectorIsDone = false;
                do
                {
                    x = (rootRadius) * Math.Cos(teta * Math.PI / 180);
                    y = (rootRadius) * Math.Sin(teta * Math.PI / 180);
                    GearPolyPoints.Add(new Point3d(rotationX(taurot, x, y) + BasePoint.X, rotationY(taurot, x, y) + BasePoint.Y, 0));
                    k = k + 1;
                    teta = teta + stepF;
                    if (teta > tau)
                    {
                        sectorIsDone = true;
                    }
                } while (!sectorIsDone);
                #endregion

                //bulgeIndices.Add(GearPolyPoints.Count);

                taurot = taurot + tau;
                if (taurot > tau * gearTeethCount)
                {
                    gearIsDone = true;
                }
            } while (!gearIsDone);

            Polyline3d GearPoly = new Polyline3d(Poly3dType.SimplePoly, GearPolyPoints, false);
            /*
           DoubleCollection Bulges = new DoubleCollection();
           Bulges.Capacity = GearPolyPoints.Count;
      
           for (int i = 0 ; i < Bulges.Capacity ; i++)
           {
             if (bulgeIndices.Contains(i))
             {
               Bulges.Add(-(gearModule*0.3));
               //Bulges[i] = gearModule * 0.2;
             }
             else
             {
               //Bulges[i] = 0.0;
               Bulges.Add(0.0);
             }
        
           } */

            //Polyline2d GearPoly = new Polyline2d(Poly2dType.SimplePoly , GearPolyPoints , 0.0 , false , 0.0 , 0.0 , Bulges);

            return GearPoly;
        }

        private double rotationX(double rotang, double x, double y)
        {
            return x * Math.Cos(rotang * Math.PI / 180) - y * Math.Sin(rotang * Math.PI / 180);
        }

        private double rotationY(double rotang, double x, double y)
        {
            return x * Math.Sin(rotang * Math.PI / 180) + y * Math.Cos(rotang * Math.PI / 180);
        }
    }
}
