using System;
using System.Collections.Generic;
using KojtoCAD.Mathematics.Geometry;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
#endif

namespace KojtoCAD.LayoutCommands
{
    public static class LayoutHelper
    {
        public static Pair<Point3d, Point3d> GetVPortToModelSpaceDiagpnal(ref Viewport vpr)
        {
            double xLnegth = vpr.Width / vpr.CustomScale;
            double yLength = vpr.Height / vpr.CustomScale;

            Quaternion q1 = new Quaternion(0, 1, 0, 0);
            Quaternion q2 = new Quaternion(0, 0, 1, 0);
            q1 = q1.get_rotateAroundAxe(new Quaternion(0, 0, 0, 1), vpr.TwistAngle);
            q2 = q2.get_rotateAroundAxe(new Quaternion(0, 0, 0, 1), vpr.TwistAngle);

            UCS ucs = new UCS(new Quaternion(), q1, q2);
            Vector3d vMOV = vpr.ViewTarget - Point3d.Origin;
            Quaternion qMOV = new Quaternion(0, vMOV.X, vMOV.Y, vMOV.Z);

            Quaternion c = new Quaternion(0, vpr.ViewCenter.X, vpr.ViewCenter.Y, 0);
            Quaternion c1 = new Quaternion(0, c.GetX() + xLnegth / 2.0, c.GetY() + yLength / 2.0, 0);
            Quaternion c2 = new Quaternion(0, c.GetX() - xLnegth / 2.0, c.GetY() - yLength / 2.0, 0);
            c = ucs.FromACS(c);
            c1 = ucs.FromACS(c1);
            c2 = ucs.FromACS(c2);

            c += qMOV; c1 += qMOV; c2 += qMOV;

            return new Pair<Point3d, Point3d>(new Point3d(c1.GetX(), c1.GetY(), 0), new Point3d(c2.GetX(), c2.GetY(), 0));
        }

        public static List<Pair<Pair<string, ObjectId>, List<Pair<Point3d, Point3d>>>> CreateArrayOfLayoutNamesAndViewportsDiagonals()
        {
            List<Pair<Pair<string, ObjectId>, List<Pair<Point3d, Point3d>>>> rez = new List<Pair<Pair<string, ObjectId>, List<Pair<Point3d, Point3d>>>>();

            Database db = HostApplicationServices.WorkingDatabase;

            using ( Transaction transaction = db.TransactionManager.StartTransaction() )
            {
                BlockTable blockTable = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                foreach ( ObjectId btrId in blockTable )
                {
                    var btr = (BlockTableRecord)transaction.GetObject(btrId, OpenMode.ForRead);
                    if ( btr.IsLayout && btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper() )
                    {
                        Layout lo = (Layout)transaction.GetObject(btr.LayoutId, OpenMode.ForRead);
                        List<Pair<Point3d, Point3d>> curARR = new List<Pair<Point3d, Point3d>>();
                        //---------------------  make layout current -----
                        // LayoutManager acLayoutMgr;
                        // acLayoutMgr = LayoutManager.Current;
                        // acLayoutMgr.CurrentLayout = lo.LayoutName;
                        //------------------------------------------------------------------
                        int count = 0;
                        foreach ( ObjectId LayoutObjId in btr )
                        {
                            Object Bref = transaction.GetObject(LayoutObjId, OpenMode.ForWrite) as Object;
                            if ( Bref != null )
                            {
                                string type = Bref.GetType().ToString();
                                if ( type == "Autodesk.AutoCAD.DatabaseServices.Viewport" )
                                {
                                    if ( count > 0 )
                                    {

                                        var vpr = (Viewport)Bref;
                                        Pair<Point3d, Point3d> diagonal = GetVPortToModelSpaceDiagpnal(ref vpr);
                                        curARR.Add(diagonal);

                                    }
                                    else
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                        if ( curARR.Count > 0 )
                        {
                            Pair<string, ObjectId> rezs = new Pair<string, ObjectId>(lo.LayoutName, lo.ObjectId);
                            rez.Add(new Pair<Pair<string, ObjectId>, List<Pair<Point3d, Point3d>>>(rezs, curARR));
                        }
                    }
                }
            }

            return rez;
        }

    }
}
