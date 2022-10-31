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
        public static ViewportInModelSpace GetVPortToModelSpaceDiagonal(Viewport vpr)
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

            return new ViewportInModelSpace(new Point3d(c2.GetX(), c2.GetY(), 0), new Point3d(c1.GetX(), c1.GetY(), 0));
        }

        /// <summary>
        /// NB: Layouts without Viewports will be skipped.
        /// </summary>
        /// <returns></returns>
        public static ICollection<LayoutForSplitting> CreateArrayOfLayoutNamesAndViewportsDiagonals()
        {
            var res = new List<LayoutForSplitting>();

            Database db = HostApplicationServices.WorkingDatabase;

            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                var blockTable = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                foreach (ObjectId btrId in blockTable)
                {
                    var btr = (BlockTableRecord)transaction.GetObject(btrId, OpenMode.ForRead);
                    if (!btr.IsLayout || btr.Name.ToUpper() == BlockTableRecord.ModelSpace.ToUpper())
                    {
                        continue;
                    }

                    Layout lo = (Layout)transaction.GetObject(btr.LayoutId, OpenMode.ForRead);
                    var viewports = new List<ViewportInModelSpace>();

                    // LayoutManager.Current.CurrentLayout = layoutName; // NB: NO, it's slow!
                    var defaultViewport = true;
                    foreach (ObjectId LayoutObjId in btr)
                    {
                        var vpr = transaction.GetObject(LayoutObjId, OpenMode.ForWrite) as Viewport;
                        if (vpr != null)
                        {
                            if (defaultViewport)
                            {
                                defaultViewport = false;
                                continue;
                            }
                            var diagonal = GetVPortToModelSpaceDiagonal(vpr);
                            viewports.Add(diagonal);
                        }
                    }
                    if (viewports.Count > 0)
                    {
                        var layoutViewports = new LayoutForSplitting(lo.LayoutName, lo.ObjectId, viewports);
                        res.Add(layoutViewports);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// NB: Layouts without Viewports will be skipped.
        /// </summary>
        /// <returns></returns>
        public static ICollection<LayoutForSplitting> CreateArrayOfLayoutNamesAndViewportsDiagonals(string[] layoutNames)
        {
            var res = new List<LayoutForSplitting>();
            Database db = HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var layoutsDictionary = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                foreach (var layoutName in layoutNames)
                {
                    // Making current Layout active will set Viewport's number property.
                    // We have to skip the viewport with Number=1, because it is the default layout's viewport. 
                    // We choose not to do that, because layout switching it's slow.
                    // LayoutManager.Current.CurrentLayout = layoutName; // NB: NO, it's slow!

                    var layout = tr.GetObject(layoutsDictionary.GetAt(layoutName), OpenMode.ForRead) as Layout;
                    var layoutBtr = tr.GetObject(layout.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                    var viewports = new List<ViewportInModelSpace>();
                    var layoutViewports = new LayoutForSplitting(layoutName, layout.BlockTableRecordId, viewports);

                    var defaultViewport =  true;
                    foreach (var objId in layoutBtr)
                    {
                        var vp = tr.GetObject(objId, OpenMode.ForRead) as Viewport;
                        if (vp is null) 
                        {
                            continue;
                        }
                        if (defaultViewport)
                        {
                            // The first found viewport is always the layout's default one. We have to skip it.
                            defaultViewport = false;
                            continue;
                        }
                        var diagonal = GetVPortToModelSpaceDiagonal(vp);
                        viewports.Add(diagonal);
                    }

                    if (viewports.Count > 0)
                    {
                        res.Add(layoutViewports);
                    }
                }
            }

            return res;
        }
    }
}
