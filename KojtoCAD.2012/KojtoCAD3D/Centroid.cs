using System;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
#if !bcad
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
using Teigha.Colors;
using Teigha.BoundaryRepresentation;
using Face = Teigha.DatabaseServices.Face;
using Application = Bricscad.ApplicationServices.Application;
using Window = Bricscad.Windows.Window;
#endif
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.Centroid))]

namespace KojtoCAD.KojtoCAD3D
{
    public class Centroid
    {
        public Containers container = ContextVariablesProvider.Container;

        [CommandMethod("KojtoCAD_3D", "KCAD_GET_GLASS_CONTURS_CENTROID", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/SPOTS_CENTROID.htm", "")]
        public void KojtoCAD_3D_Get_Glass_Conturs_Centroid()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {
                PromptKeywordOptions pop = new PromptKeywordOptions("");
                pop.AppendKeywordsToMessage = true;
                pop.AllowNone = false;
                pop.Keywords.Add("Run");
                pop.Keywords.Add("Help");
                pop.Keywords.Default = "Run";
                PromptResult res = ed.GetKeywords(pop);
                //_AcAp.Application.ShowAlertDialog(res.ToString());
                if (res.Status == PromptStatus.OK)
                {
                    switch (res.StringResult)
                    {
                        case "Run":
                            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                            {
                                double area = 0.0;
                                quaternion cen = new quaternion();
                                foreach (WorkClasses.Triangle TR in container.Triangles)
                                {

                                    Triplet<quaternion, quaternion, quaternion> pr = container.GetInnererTriangle(TR.Numer, true);

                                    double ar = Math.Abs(GlobalFunctions.GetArea(pr));

                                    area += ar;
                                    quaternion centroid = new quaternion();
                                    centroid = (pr.First + pr.Second) / 2.0;
                                    centroid = centroid - pr.Third;
                                    centroid *= (2.0 / 3.0);
                                    centroid = pr.Third + centroid;
                                    centroid *= ar;
                                    cen += centroid;
                                }
                                if (area > 0.0)
                                {
                                    cen /= area;
                                    string title = "\nTeoretical Mesh Glass Conturs center of gravity\n------------------\n\n";
                                    string mess = string.Format("{0} total area: {1} \n Coordinate: {2:f5},{3:f5},{4:f5}", title, area,
                                        cen.GetX(), cen.GetY(), cen.GetZ());
                                    ed.WriteMessage(mess);

                                    MessageBox.Show(mess, "Glass Conturs - teoretical mesh center of gravity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "Help":
                            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/SPOTS_CENTROID.htm");
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(
                    string.Format("\nError: {0}\nStackTrace: {1}", ex.Message, ex.StackTrace));
            }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_GET_TEORETICAL_BENDS_CENTROID", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/MESH_CENTROID.htm", "")]
        public void KojtoCAD_3D_Get_Teoretical_Bends_Centroid()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {
                PromptKeywordOptions pop = new PromptKeywordOptions("");
                pop.AppendKeywordsToMessage = true;
                pop.AllowNone = false;
                pop.Keywords.Add("Run");
                pop.Keywords.Add("Help");
                pop.Keywords.Default = "Run";
                PromptResult res = ed.GetKeywords(pop);
                //_AcAp.Application.ShowAlertDialog(res.ToString());
                if (res.Status == PromptStatus.OK)
                {
                    switch (res.StringResult)
                    {
                        case "Run":
                            if ((container != null) && (container.Bends.Count > 0))
                            {
                                quaternion qSum_Bends_Teor = new quaternion();
                                double mass = 0.0;
                                foreach (WorkClasses.Bend bend in container.Bends)
                                {
                                    if (!bend.IsFictive())
                                    {
                                        qSum_Bends_Teor += (bend.MidPoint * bend.Length);
                                        mass += bend.Length;
                                    }
                                }
                                if (mass > 0)
                                {
                                    qSum_Bends_Teor /= mass;
                                    //UtilityClasses.GlobalFunctions.DrawLine(new Point3d(1000, 1000, 0), (Point3d)qSum_Bends_Real, 3);
                                    string title = "\nTeoretical Mesh Bends center of gravity\n------------------\n\n";
                                    string mess = string.Format("{0} total length: {1} \n Coordinate: {2:f5},{3:f5},{4:f5}", title, mass,
                                        qSum_Bends_Teor.GetX(), qSum_Bends_Teor.GetY(), qSum_Bends_Teor.GetZ());
                                    ed.WriteMessage(mess);

                                    MessageBox.Show(mess, "Bends - teoretical mesh center of gravity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                                MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "Help":
                            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/MESH_CENTROID.htm");
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(
                    string.Format("\nError: {0}\nStackTrace: {1}", ex.Message, ex.StackTrace));
            }
            finally { ed.CurrentUserCoordinateSystem = old; }

        }

        [CommandMethod("KojtoCAD_3D", "KCAD_GET_REAL_CENTROID", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/REAL_CENTROID.htm", "")]
        public void KojtoCAD_3D_Get_Real_3D_Centroid()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {
                Pair<double, PromptStatus> nodeDensityPair =
                     GlobalFunctions.GetDouble(ConstantsAndSettings.nodeDensity, "\nNodes Density: ");
                if (nodeDensityPair.Second == PromptStatus.OK)
                {
                    Pair<double, PromptStatus> bendDensityPair =
                         GlobalFunctions.GetDouble(ConstantsAndSettings.bendDensity, "\nBends Density: ");
                    if (bendDensityPair.Second == PromptStatus.OK)
                    {

                        Pair<double, PromptStatus> nozzleDensityPair =
                         GlobalFunctions.GetDouble(ConstantsAndSettings.nozzleDensity, "\nEnd of Bends (Nozzle) Density: ");

                        if (nozzleDensityPair.Second == PromptStatus.OK)
                        {
                            Pair<double, PromptStatus> glassDensityPair =
                                GlobalFunctions.GetDouble(ConstantsAndSettings.glassDensity, "\nGlass Density: ");

                            if (glassDensityPair.Second == PromptStatus.OK)
                            {
                                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                                {
                                    PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                                    pKeyOpts.Message = "\nEnter an option ";
                                    pKeyOpts.Keywords.Add("All");
                                    pKeyOpts.Keywords.Add("Glass");
                                    pKeyOpts.Keywords.Add("Mesh");
                                    pKeyOpts.Keywords.Add("Triangles");
                                    pKeyOpts.Keywords.Add("Polygons");
                                    pKeyOpts.Keywords.Add("Bends");
                                    pKeyOpts.Keywords.Add("Nodes");
                                    pKeyOpts.Keywords.Default = "All";
                                    pKeyOpts.AllowNone = true;

                                    PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                                    {
                                        switch (pKeyRes.StringResult)
                                        {
                                            case "All":
                                                #region All
                                                Triplet<int, double, quaternion> POL = GetPolygonsCentroid(ref container, glassDensityPair.First);
                                                Triplet<int, double, quaternion> TRI = GetTrianglesCentroid(ref container, glassDensityPair.First);
                                                Triplet<int, double, quaternion> NOD = GetNodesCentroid(ref container, nodeDensityPair.First);
                                                Triplet<int, double, quaternion> BEN = GetBendsCentroid(ref container, bendDensityPair.First, nozzleDensityPair.First);
                                                if ((POL.First > 0) || (TRI.First > 0) || (NOD.First > 0) || (BEN.First > 0))
                                                {
                                                    quaternion TEMP_Q = POL.Second * POL.Third + TRI.Second * TRI.Third + NOD.Second * NOD.Third + BEN.Second * BEN.Third;
                                                    double TEMP_V = POL.Second + TRI.Second + NOD.Second + BEN.Second;
                                                    TEMP_Q /= TEMP_V;

                                                    string title = "\nCenter of gravity\n------------------\n\n";
                                                    string mess = string.Format("{0} total volume: {1} \n Coordinate: {2:f5},{3:f5},{4:f5}", title, TEMP_V,
                                                        TEMP_Q.GetX(), TEMP_Q.GetY(), TEMP_Q.GetZ());
                                                    ed.WriteMessage(mess);

                                                    MessageBox.Show(mess, "Center of gravity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                #endregion
                                                break;
                                            case "Glass":
                                                #region Glass
                                                Triplet<int, double, quaternion> pol = GetPolygonsCentroid(ref container, glassDensityPair.First);
                                                Triplet<int, double, quaternion> tri = GetTrianglesCentroid(ref container, glassDensityPair.First);
                                                if ((pol.First > 0) || (tri.First > 0))
                                                {
                                                    quaternion tempQ = pol.Second * pol.Third + tri.Second * tri.Third;
                                                    double tempV = pol.Second + tri.Second;
                                                    tempQ /= tempV;

                                                    string title = "\nGlass center of gravity\n------------------\n\n";
                                                    string mess = string.Format("{0} total volume: {1} \n Coordinate: {2:f5},{3:f5},{4:f5}", title, tempV,
                                                        tempQ.GetX(), tempQ.GetY(), tempQ.GetZ());
                                                    ed.WriteMessage(mess);

                                                    MessageBox.Show(mess, "Glass - center of gravity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                #endregion
                                                break;
                                            case "Mesh":
                                                #region Mesh
                                                Triplet<int, double, quaternion> nod = GetNodesCentroid(ref container, nodeDensityPair.First);
                                                Triplet<int, double, quaternion> ben = GetBendsCentroid(ref container, bendDensityPair.First, nozzleDensityPair.First);

                                                if ((nod.First > 0) || (ben.First > 0))
                                                {
                                                    quaternion tempQ = nod.Second * nod.Third + ben.Second * ben.Third;
                                                    double tempV = nod.Second + ben.Second;
                                                    tempQ /= tempV;

                                                    string title = "\nMesh center of gravity\n------------------\n\n";
                                                    string mess = string.Format("{0} total volume: {1} \n Coordinate: {2:f5},{3:f5},{4:f5}", title, tempV,
                                                        tempQ.GetX(), tempQ.GetY(), tempQ.GetZ());
                                                    ed.WriteMessage(mess);

                                                    MessageBox.Show(mess, "Mesh - center of gravity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                #endregion
                                                break;
                                            case "Polygons":
                                                #region Polygons
                                                Triplet<int, double, quaternion> pp = GetPolygonsCentroid(ref container, glassDensityPair.First);
                                                if (pp.First > 0)
                                                {
                                                    string title = "\nPolygons center of gravity\n------------------\n\n";
                                                    string mess = string.Format("{0} total volume: {1} \n Coordinate: {2:f5},{3:f5},{4:f5}", title, pp.Second,
                                                        pp.Third.GetX(), pp.Third.GetY(), pp.Third.GetZ());
                                                    ed.WriteMessage(mess);

                                                    MessageBox.Show(mess, "Polygons - center of gravity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                #endregion
                                                break;
                                            case "Triangles":
                                                #region trianles
                                                Triplet<int, double, quaternion> ppp = GetTrianglesCentroid(ref container, glassDensityPair.First);
                                                if (ppp.First > 0)
                                                {
                                                    string title = "\nTriangless center of gravity\n------------------\n\n";
                                                    string mess = string.Format("{0} total volume: {1} \n Coordinate: {2:f5},{3:f5},{4:f5}", title, ppp.Second,
                                                        ppp.Third.GetX(), ppp.Third.GetY(), ppp.Third.GetZ());
                                                    ed.WriteMessage(mess);

                                                    MessageBox.Show(mess, "Triangles - center of gravity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                #endregion
                                                break;
                                            case "Bends":
                                                #region bends
                                                Triplet<int, double, quaternion> trB = GetBendsCentroid(ref container, bendDensityPair.First, nozzleDensityPair.First);
                                                if (trB.First > 0)
                                                {
                                                    string title = "\nBends center of gravity\n------------------\n\n";
                                                    string mess = string.Format("{0} total volume: {1} \n Coordinate: {2:f5},{3:f5},{4:f5}", title, trB.Second,
                                                        trB.Third.GetX(), trB.Third.GetY(), trB.Third.GetZ());
                                                    ed.WriteMessage(mess);

                                                    MessageBox.Show(mess, "Bends - center of gravity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                #endregion
                                                break;
                                            case "Nodes":
                                                #region nodes
                                                Triplet<int, double, quaternion> tr = GetNodesCentroid(ref container, nodeDensityPair.First);
                                                if (tr.First > 0)
                                                {
                                                    string title = "\nNodes center of gravity\n------------------\n\n";
                                                    string mess = string.Format("{0} total volume: {1} \n Coordinate: {2:f5},{3:f5},{4:f5}", title, tr.Second,
                                                        tr.Third.GetX(), tr.Third.GetY(), tr.Third.GetZ());
                                                    ed.WriteMessage(mess);

                                                    MessageBox.Show(mess, "Nodes - center of gravity", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                #endregion
                                                break;
                                        }
                                    }
                                }
                                else
                                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }//
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }
        #region subFunctions
        private static Triplet<int, double, quaternion> GetNodesCentroid(ref Containers container, double density = 1.0)
        {
            Triplet<int, double, quaternion> rez =
                    new Triplet<int, double, quaternion>(-1, 0.0, new quaternion());

            double mass = 0.0;
            quaternion Q = new quaternion();
            foreach (WorkClasses.Node node in container.Nodes)
            {
                if (!node.IsFictive(ref container))
                {
                    if (node.SolidHandle.First >= 0)
                    {
                        Triplet<int, double, quaternion> tQ = node.CenterOfGravity(ref container, density);
                        if (tQ.First > 0)
                        {
                            mass += tQ.Second;
                            Q += tQ.Second * tQ.Third;
                        }
                    }
                }
            }
            if (mass > 0.0)
            {
                Q /= mass;
                rez = new Triplet<int, double, quaternion>(1, mass, Q);
            }

            return rez;
        }
        private static Triplet<int, double, quaternion> GetBendsCentroid(ref Containers container, double densityB = 1.0, double densityN = 1.0)
        {
            Triplet<int, double, quaternion> rez =
                    new Triplet<int, double, quaternion>(-1, 0.0, new quaternion());

            double mass = 0.0;
            quaternion Q = new quaternion();
            foreach (WorkClasses.Bend bend in container.Bends)
            {
                if (!bend.IsFictive())
                {

                    Triplet<int, double, quaternion> tQ = bend.CenterOfGravity(densityB, densityN);
                    if (tQ.First > 0)
                    {
                        mass += tQ.Second;
                        Q += tQ.Second * tQ.Third;
                    }
                }
            }
            if (mass > 0.0)
            {
                Q /= mass;
                rez = new Triplet<int, double, quaternion>(1, mass, Q);
            }

            return rez;
        }
        private static Triplet<int, double, quaternion> GetPolygonsCentroid(ref Containers container, double density = 1.0)
        {
            Triplet<int, double, quaternion> rez =
                    new Triplet<int, double, quaternion>(-1, 0.0, new quaternion());

            double mass = 0.0;
            quaternion Q = new quaternion();

            foreach (WorkClasses.Polygon POL in container.Polygons)
            {
                WorkClasses.Triangle TR = container.Triangles[POL.Triangles_Numers_Array[0]];
                if (TR.lowSolidHandle.First >= 0)
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            Solid3d ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                            double mass1 = ent.MassProperties.Volume * density;
                            quaternion q1 = ent.MassProperties.Centroid * mass1;
                            double mass2 = 0.0;
                            quaternion q2 = new quaternion();
                            if (TR.upSolidHandle.First >= 0)
                            {
                                Solid3d ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                                mass2 = ent1.MassProperties.Volume * density;
                                q2 = ent1.MassProperties.Centroid * mass2;
                            }

                            Q += (q1 + q2);
                            mass += (mass1 + mass2);
                        }
                        catch { }
                    }
                }
            }
            if (mass > 0.0)
            {
                Q /= mass;
                rez = new Triplet<int, double, quaternion>(1, mass, Q);
            }

            return rez;
        }
        private static Triplet<int, double, quaternion> GetTrianglesCentroid(ref Containers container, double density = 1.0)
        {
            Triplet<int, double, quaternion> rez =
                    new Triplet<int, double, quaternion>(-1, 0.0, new quaternion());

            double mass = 0.0;
            quaternion Q = new quaternion();

            foreach (WorkClasses.Triangle TR in container.Triangles)
            {
                if (TR.lowSolidHandle.First >= 0)
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            Solid3d ent = tr.GetObject(GlobalFunctions.GetObjectId(TR.lowSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                            double mass1 = ent.MassProperties.Volume * density;
                            quaternion q1 = ent.MassProperties.Centroid * mass1;
                            double mass2 = 0.0;
                            quaternion q2 = new quaternion();
                            if (TR.upSolidHandle.First >= 0)
                            {
                                Solid3d ent1 = tr.GetObject(GlobalFunctions.GetObjectId(TR.upSolidHandle.Second), OpenMode.ForWrite) as Solid3d;
                                mass2 = ent1.MassProperties.Volume * density;
                                q2 = ent1.MassProperties.Centroid * mass2;
                            }

                            Q += (q1 + q2);
                            mass += (mass1 + mass2);
                        }
                        catch { }
                    }
                }
            }
            if (mass > 0.0)
            {
                Q /= mass;
                rez = new Triplet<int, double, quaternion>(1, mass, Q);
            }

            return rez;
        }
        #endregion
    }
}
