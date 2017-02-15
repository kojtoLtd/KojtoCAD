using System;
using System.IO;
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
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.CamCncExport))]

namespace KojtoCAD.KojtoCAD3D
{
    public class CamCncExport
    {
        public Containers container = ContextVariablesProvider.Container;
        //---- CNC -----------------------
        [CommandMethod("KojtoCAD_3D", "KCAD_NODE_TO_CNC", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CAM_NODE_TO_CNC.htm", "")]
        public void KojtoCAD_3D_Node_To_CNC()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Nodes.Count > 0))
                {
                    CNC_Settings form = new CNC_Settings();
                    form.ShowDialog();
                    if (form.DialogResult == DialogResult.OK)
                    {
                        PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Project name <" + ConstantsAndSettings.ProjectName.Trim() + ">: ");
                        pStrOpts.AllowSpaces = true;
                        PromptResult pStrRes = Application.DocumentManager.MdiActiveDocument.Editor.GetString(pStrOpts);
                        if (pStrRes.Status == PromptStatus.OK)
                        {
                            string pjName = pStrRes.StringResult;
                            if (pjName == "")
                                pjName = ConstantsAndSettings.ProjectName;

                            PromptStringOptions pStrOpts_ = new PromptStringOptions("\nEnter Material Name <St3>: ");
                            pStrOpts_.AllowSpaces = true;
                            PromptResult pStrRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetString(pStrOpts_);
                            if (pStrRes_.Status == PromptStatus.OK)
                            {
                                string matName = pStrRes_.StringResult;
                                if (matName == "")
                                    matName = "St3";

                                FolderBrowserDialog dlg = new FolderBrowserDialog();
                                if (dlg.ShowDialog() == DialogResult.OK)
                                {
                                    if ((container != null) && (container.Nodes.Count > 0))
                                    {
                                        foreach (WorkClasses.Node node in container.Nodes)
                                        {
                                            NodeToCNC(dlg.SelectedPath + "\\", node.Numer, form.L, form.Lp, form.Ls, form.R, form.toolR, pjName, form.Workpiece_Length, form.Workpiece_Width, form.Workpiece_Height, matName, " MILLM550, FANUC 0-MC", "4,5OS", "X-300.",
                                                "/M6T13(FREZA 36-SANDVIK)", "H13G0Z150.", "G0 X0 Y170.");
                                        }

                                        MessageBox.Show("Files were Created Successfully !", "CNC Files", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                        MessageBox.Show("Node missing !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
                else
                    MessageBox.Show("Data Base Missing !", "E R R O R", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_NODE_TO_CSV", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/KAM_NODE_TO_CSV.htm", "")]
        public void KojtoCAD_3D_Node_To_Csv()
        {
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if (container != null)
                {
                    if (container.Nodes.Count > 0)
                    {
                        SaveFileDialog dlg = new SaveFileDialog();
                        dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                        dlg.Title = "Enter CSV File Name ";
                        dlg.DefaultExt = "csv";
                        dlg.FileName = "*.csv";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            string fileName = dlg.FileName;

                            CNC_Settings form = new CNC_Settings();
                            form.ShowDialog();
                            if (form.DialogResult == DialogResult.OK)
                            {
                                using (StreamWriter sw = new StreamWriter(fileName))
                                {
                                    #region firstline
                                    int max = 0;
                                    foreach (WorkClasses.Node Node in container.Nodes)
                                    {
                                        if (Node.Bends_Numers_Array.Count > max)
                                            max = Node.Bends_Numers_Array.Count;
                                    }
                                    string LINE = "NodeNumer ; Position ;";
                                    for (int i = 0; i < max; i++)
                                        LINE += " bendNumer ; ; cota +/- ; alpha ; beta ; gama ; deltaX ; deltaZ ; Z1 ; Z2 ;";
                                    sw.WriteLine(LINE);
                                    #endregion
                                    foreach (WorkClasses.Node Node in container.Nodes)
                                    {
                                        string line = (Node.Numer + 1).ToString() + " ;";

                                        bool perereferial = false;
                                        foreach (int nN in Node.Bends_Numers_Array)
                                        {
                                            if (container.Bends[nN].IsPeripheral())
                                            {
                                                perereferial = true;
                                                break;
                                            }
                                        }
                                        if (perereferial)
                                            line += "Pereferial ;";
                                        else
                                            line += " NoPereferial ;";

                                        CNC_bends(ref line, Node, form.L, form.Lp, form.Ls, form.R, form.toolR, form.Workpiece_Length, form.Workpiece_Width, form.Workpiece_Height);

                                        sw.WriteLine(line);
                                    }

                                    sw.Flush();
                                    sw.Close();
                                    MessageBox.Show("The File is saved successfully !", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                    else
                        MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        public void NodeToCNC(string path, int NodeNumer, double L, double Lp, double Ls, double R, double toolR, string projectName, double gabaritX,
        double gabaritY, double gabaritZ, string material, string maschine, string prisposoblenia, string startPos, string toolStr, string G43H, string outCoord)
        {

            try
            {
                WorkClasses.Node node = container.Nodes[NodeNumer];
                string FileName = path + String.Format("O{0}({1}-VYZEL NOMER {2})", 2000 + node.Numer + 1, projectName, node.Numer + 1) + ".txt";
                using (StreamWriter outf = new StreamWriter(FileName))
                {
                    DateTime time = DateTime.Now;
                    outf.WriteLine("%");
                    outf.WriteLine(String.Format("O{0}({1}-VYZEL NOMER {2})", 2000 + node.Numer + 1, projectName, node.Numer + 1));
                    outf.WriteLine("(MACHINE  {0})", maschine);
                    outf.WriteLine(String.Format("(DATE {0}.{1}.{2})", time.Day, time.Month, time.Year));
                    outf.WriteLine(String.Format("(GABAGRITI NA ZAGOTOVKATA X,Y,Z MM {0},{1},{2})", gabaritX, gabaritY, gabaritZ));
                    outf.WriteLine(String.Format("(MATERIAL-{0})", material));
                    outf.WriteLine("(QUADRANT 1,2,3,4)");
                    outf.WriteLine("(CENTAR NA KOORDINATNATA SISTEMA X,Y,Z MM-CENTAR )");
                    outf.WriteLine(String.Format("(PRISPOSOBLENIA-{0})", prisposoblenia));
                    outf.WriteLine("(CYCLE TIME )");
                    outf.WriteLine("G90G94G40G80G50G55G69");
                    outf.WriteLine("G17G22G21G49G98G64G15");
                    outf.WriteLine("G52X0Y0Z0");
                    outf.WriteLine("(===HEADER END/PROGRAMM START===)");
                    outf.WriteLine();
                    outf.WriteLine(String.Format("G0{0}", startPos));
                    outf.WriteLine(toolStr);
                    outf.WriteLine(String.Format("G43{0}", G43H));
                    outf.WriteLine("M1");
                    outf.WriteLine();
                    outf.WriteLine();
                    //-----------------------------
                    CNC_bends(outf, node, L, Lp, Ls, R, toolR, gabaritX, gabaritY, gabaritZ);
                    //------------------------------
                    outf.WriteLine();
                    outf.WriteLine(outCoord);
                    outf.WriteLine("M30");
                    outf.WriteLine("%");
                    outf.Flush();
                    outf.Close();
                }
            }
            catch
            {
                if (((object)container == null) || container.Nodes.Count < 1)
                {
                    MessageBox.Show("Node missing !", "Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public void CNC_bends(StreamWriter outf, WorkClasses.Node node, double L, double Lp, double Ls, double R, double toolR, double gabaritX, double gabaritY, double gabaritZ)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            quaternion tempNodeNormal = node.GetNodesNormalsByNoFictiveBends(ref container);

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                UCS ucs = node.CreateNodeUCS(L, ref container);
                UCS Ucs = node.CreateNodeUCS(L + Lp + Ls, ref container);

                int counter = 0;
                for (int i = 0; i < node.Bends_Numers_Array.Count; i++)
                {
                    WorkClasses.Bend b = container.Bends[node.Bends_Numers_Array[i]];
                    if (b.IsFictive()) { continue; }
                    outf.WriteLine(String.Format("(BAR {0} - PRYT NOMER {1})", counter + 1, b.Numer + 1));
                    outf.WriteLine();

                    //---------------------                    
                    quaternion bendNormal = b.Normal - b.MidPoint;
                    plane gabaritTOP = new plane(ucs.ToACS(new quaternion(0, 10, 0, gabaritZ / 2.0)), ucs.ToACS(new quaternion(0, 100, 0, gabaritZ / 2.0)), ucs.ToACS(new quaternion(0, 0, 10, gabaritZ / 2.0)));
                    plane gabaritBottom = new plane(ucs.ToACS(new quaternion(0, 10, 0, -gabaritZ / 2.0)), ucs.ToACS(new quaternion(0, 100, 0, -gabaritZ / 2.0)), ucs.ToACS(new quaternion(0, 0, 10, -gabaritZ / 2.0)));
                    quaternion Q1 = gabaritTOP.IntersectWithVector((b.MidPoint + b.Start) / 2.0, (b.MidPoint + b.Start) / 2.0 + bendNormal);
                    quaternion Q2 = gabaritTOP.IntersectWithVector(b.MidPoint, b.Normal);
                    Q1 = ucs.FromACS(Q1);
                    Q2 = ucs.FromACS(Q2);
                    complex cQ1 = new complex(Q1.GetX(), Q1.GetY());
                    complex cQ2 = new complex(Q2.GetX(), Q2.GetY());
                    matrix Int_Mat = Common.IntersectCircleAndLine(R, cQ1, cQ2);
                    if (Int_Mat.GetAt(0, 4) < 0) { /*ERRROR*/return; }
                    complex iP1 = new complex(Int_Mat.GetAt(0, 0), Int_Mat.GetAt(0, 1));
                    complex iP2 = new complex(Int_Mat.GetAt(0, 2), Int_Mat.GetAt(0, 3));
                    complex iP = ((cQ2 - iP1).abs() < (cQ2 - iP2).abs()) ? iP1 : iP2;
                    quaternion iQ = ucs.ToACS(new quaternion(0.0, iP.real(), iP.imag(), gabaritZ / 2.0));
                    quaternion cenTOP = iQ;// gabaritTOP.IntersectWithVector(iQ, iQ + bendNormal);
                    quaternion cenBottom = gabaritBottom.IntersectWithVector(iQ, iQ + bendNormal);
                    Q2 = ucs.ToACS(Q2);
                    quaternion outTOP = Q2 - iQ; quaternion inTOP = iQ - Q2;
                    outTOP /= outTOP.abs(); inTOP /= inTOP.abs();
                    outTOP *= toolR; inTOP *= toolR;
                    outTOP = iQ + outTOP; inTOP = iQ + inTOP;

                    // MessageBox.Show(String.Format("{0},{1},{2}\n{3},{4},{5}\n{6},{7},{8}",
                    //iQ.GetX(), iQ.GetY(), iQ.GetZ(), cenTOP.GetX(), cenTOP.GetY(), cenTOP.GetZ(), cenBottom.GetX(), cenBottom.GetY(), cenBottom.GetZ()));
                    //--------------------------
                    double gabaritR = Math.Sqrt(gabaritX * gabaritX / 4.0 + gabaritY * gabaritY / 4.0);
                    matrix gabaritMat = Common.IntersectCircleAndLine(gabaritR, cQ1, cQ2);
                    complex gabaritP1 = new complex(gabaritMat.GetAt(0, 0), gabaritMat.GetAt(0, 1));
                    complex gabaritP2 = new complex(gabaritMat.GetAt(0, 2), gabaritMat.GetAt(0, 3));
                    complex gabaritP = ((cQ2 - gabaritP1).abs() < (cQ2 - gabaritP2).abs()) ? gabaritP1 : gabaritP2;
                    quaternion gabaritQ = ucs.ToACS(new quaternion(0.0, gabaritP.real(), gabaritP.imag(), gabaritZ / 2.0));
                    //---------------------

                    bool bp = ((node.Position - b.Start).abs() < (node.Position - b.End).abs()) ? true : false;//kой край на реброто е във възела
                    Pair<quaternion, quaternion> bendAxe = (bp == true) ?
                        new Pair<quaternion, quaternion>(ucs.FromACS(b.Start), ucs.FromACS(b.End)) :
                        new Pair<quaternion, quaternion>(ucs.FromACS(b.End), ucs.FromACS(b.Start));

                    Pair<quaternion, quaternion> pa = new Pair<quaternion, quaternion>(ucs.FromACS(b.MidPoint), ucs.FromACS(b.Normal));
                    quaternion ucsOriginDescriptor = new quaternion();
                    cenTOP = ucs.FromACS(cenTOP); cenBottom = ucs.FromACS(cenBottom); inTOP = ucs.FromACS(inTOP); outTOP = ucs.FromACS(outTOP);
                    gabaritQ = ucs.FromACS(gabaritQ);
                    //  double test = Math.Sqrt(cenTOP.GetX() * cenTOP.GetX() + cenTOP.GetY() * cenTOP.GetY());
                    // MessageBox.Show(test.ToString());


                    double baseAng = 0.0;
                    if (((b.Normal - b.MidPoint) / (tempNodeNormal - node.Position)).absV() < Constants.zero_dist)
                    {

                        outf.WriteLine(String.Format("#110={0:f4}(ALPHA-OS C)", 0.0));
                        outf.WriteLine(String.Format("#111 = {0:f4} (BETHA-OS B)", 0.0));
                        outf.WriteLine(String.Format("#112 = {0:f4} (DELTA X-G52 X)", 0.0));
                        outf.WriteLine(String.Format("#113 = {0:f4} (DELTA Z-G52 X)", 0.0));

                        Pair<complex, complex> benAxeXY = new Pair<complex, complex>(
                             new complex(bendAxe.First.GetX(), bendAxe.First.GetY()), new complex(bendAxe.Second.GetX(), bendAxe.Second.GetY()));
                        matrix Int_M = Common.IntersectCircleAndLine(R, benAxeXY.First, benAxeXY.Second);
                        double baseAng2 = (benAxeXY.Second - benAxeXY.First).arg();
                        if (baseAng2 > Math.PI) { baseAng2 = baseAng2 - 2 * Math.PI; }

                        outf.WriteLine(String.Format("#114 = {0:f4} (GAMMA R-G68)", baseAng2 * 180.0 / Math.PI));

                        outf.WriteLine(String.Format("#115 = {0:f4} (OBRABOTKA NAD Z=0)", gabaritZ / 2.0 + 5.0));
                        outf.WriteLine(String.Format("#116 = {0:f4} (OBRABOTKA POD Z=0)", -gabaritZ / 2.0 - 2.0));

                    }
                    else
                    {
                        quaternion nB1 = ucs.FromACS(b.Normal);
                        quaternion nB2 = ucs.FromACS(b.MidPoint);
                        complex cN1 = new complex(nB1.GetX(), nB1.GetY());
                        complex cN2 = new complex(nB2.GetX(), nB2.GetY());
                        line2d projectTo_ucsXY_Line = new line2d(cN1, cN2);

                        complex normalTo_projectTo_ucsXY_Line = new complex(projectTo_ucsXY_Line.A, projectTo_ucsXY_Line.B);
                        normalTo_projectTo_ucsXY_Line /= normalTo_projectTo_ucsXY_Line.abs();
                        normalTo_projectTo_ucsXY_Line *= -1.0;

                        baseAng = normalTo_projectTo_ucsXY_Line.arg();
                        if (baseAng > Math.PI) { baseAng = baseAng - 2 * Math.PI; }

                        outf.WriteLine(String.Format("#110={0:f4}(ALPHA-OS C)", -((-baseAng + Math.PI / 2.0) * 180.0 / Math.PI)));

                        pa.First.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        pa.Second.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);

                        bendAxe.First.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        bendAxe.Second.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);

                        ucsOriginDescriptor.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);

                        cenTOP.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        cenBottom.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        inTOP.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        outTOP.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        gabaritQ.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);

                        pa = new Pair<quaternion, quaternion>(ucs.ToACS(pa.First), ucs.ToACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(ucs.ToACS(bendAxe.First), ucs.ToACS(bendAxe.Second));
                        ucsOriginDescriptor = ucs.ToACS(ucsOriginDescriptor);
                        cenTOP = ucs.ToACS(cenTOP); cenBottom = ucs.ToACS(cenBottom); inTOP = ucs.ToACS(inTOP); outTOP = ucs.ToACS(outTOP);
                        gabaritQ = ucs.ToACS(gabaritQ);

                        pa = new Pair<quaternion, quaternion>(Ucs.FromACS(pa.First), Ucs.FromACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(Ucs.FromACS(bendAxe.First), Ucs.FromACS(bendAxe.Second));
                        ucsOriginDescriptor = Ucs.FromACS(ucsOriginDescriptor);
                        cenTOP = Ucs.FromACS(cenTOP); cenBottom = Ucs.FromACS(cenBottom); inTOP = Ucs.FromACS(inTOP); outTOP = Ucs.FromACS(outTOP);
                        gabaritQ = Ucs.FromACS(gabaritQ);

                        quaternion normal = (pa.Second - pa.First) * 1000.0;
                        pa = new Pair<quaternion, quaternion>(pa.First, pa.First + normal);
                        double k = (pa.Second.GetX() < pa.First.GetX()) ? 1.0 : -1.0;

                        double ang = (pa.Second - pa.First).angTo(new quaternion(0, 0, 0, 100));
                        ang *= k;
                        outf.WriteLine(String.Format("#111 = {0:f4} (BETHA-OS B)", ang * 180.0 / Math.PI));

                        pa.First.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        pa.Second.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);

                        bendAxe.First.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        bendAxe.Second.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);

                        ucsOriginDescriptor.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);

                        cenTOP.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        cenBottom.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        inTOP.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        outTOP.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        gabaritQ.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);

                        pa = new Pair<quaternion, quaternion>(Ucs.ToACS(pa.First), Ucs.ToACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(Ucs.ToACS(bendAxe.First), Ucs.ToACS(bendAxe.Second));
                        ucsOriginDescriptor = Ucs.ToACS(ucsOriginDescriptor);
                        cenTOP = Ucs.ToACS(cenTOP); cenBottom = Ucs.ToACS(cenBottom); inTOP = Ucs.ToACS(inTOP); outTOP = Ucs.ToACS(outTOP);
                        gabaritQ = Ucs.ToACS(gabaritQ);

                        pa = new Pair<quaternion, quaternion>(ucs.FromACS(pa.First), ucs.FromACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(ucs.FromACS(bendAxe.First), ucs.FromACS(bendAxe.Second));
                        ucsOriginDescriptor = ucs.FromACS(ucsOriginDescriptor);
                        cenTOP = ucs.FromACS(cenTOP); cenBottom = ucs.FromACS(cenBottom); inTOP = ucs.FromACS(inTOP); outTOP = ucs.FromACS(outTOP);
                        gabaritQ = ucs.FromACS(gabaritQ);
                        outf.WriteLine(String.Format("#112 = {0:f4} (DELTA X-G52 X)", ucsOriginDescriptor.GetX()));
                        outf.WriteLine(String.Format("#113 = {0:f4} (DELTA Z-G52 X)", ucsOriginDescriptor.GetZ()));
                        double backX = ucsOriginDescriptor.GetX();
                        double backZ = ucsOriginDescriptor.GetZ();

                        quaternion moveQ = new quaternion(0, -backX, 0.0, -backZ);
                        pa = new Pair<quaternion, quaternion>(pa.First + moveQ, pa.Second + moveQ);
                        bendAxe = new Pair<quaternion, quaternion>(bendAxe.First + moveQ, bendAxe.Second + moveQ);
                        ucsOriginDescriptor += moveQ;
                        cenTOP += moveQ; cenBottom += moveQ; inTOP += moveQ; outTOP += moveQ;
                        gabaritQ += moveQ;

                        Pair<complex, complex> benAxeXY = new Pair<complex, complex>(
                            new complex(bendAxe.First.GetX(), bendAxe.First.GetY()), new complex(bendAxe.Second.GetX(), bendAxe.Second.GetY()));
                        matrix Int_M = Common.IntersectCircleAndLine(R, benAxeXY.First, benAxeXY.Second);

                        complex Int_P1 = new complex(Int_M.GetAt(0, 0), Int_M.GetAt(0, 1));
                        complex Int_P2 = new complex(Int_M.GetAt(0, 2), Int_M.GetAt(0, 3));
                        complex Int_P = ((benAxeXY.Second - Int_P1).abs() < (benAxeXY.Second - Int_P2).abs()) ? Int_P1 : Int_P2;
                        double baseAng2 = (benAxeXY.Second - benAxeXY.First).arg();
                        if (baseAng2 > Math.PI) { baseAng2 = baseAng2 - 2 * Math.PI; }

                        outf.WriteLine(String.Format("#114 = {0:f4} (GAMMA R-G68)", baseAng2 * 180.0 / Math.PI));

                        //----------------------
                        pa = new Pair<quaternion, quaternion>(ucs.ToACS(pa.First), ucs.ToACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(ucs.ToACS(bendAxe.First), ucs.ToACS(bendAxe.Second));
                        ucsOriginDescriptor = ucs.ToACS(ucsOriginDescriptor);
                        cenTOP = ucs.ToACS(cenTOP); cenBottom = ucs.ToACS(cenBottom); inTOP = ucs.ToACS(inTOP); outTOP = ucs.ToACS(outTOP);
                        gabaritQ = ucs.ToACS(gabaritQ);
                        quaternion gabaritB = gabaritQ + cenBottom - cenTOP;
                        quaternion inBottom = inTOP + cenBottom - cenTOP;

                        double positiveZ = (ucs.FromACS(gabaritQ).GetZ() > ucs.FromACS(inTOP).GetZ()) ? ucs.FromACS(gabaritQ).GetZ() + 5.0 : ucs.FromACS(inTOP).GetZ() + 5.0;
                        double negativeZ = (ucs.FromACS(gabaritB).GetZ() < ucs.FromACS(inBottom).GetZ()) ? ucs.FromACS(gabaritB).GetZ() - 2.0 : ucs.FromACS(inBottom).GetZ() - 2.0;

                        outf.WriteLine(String.Format("#115 = {0:f4} (OBRABOTKA NAD Z=0)", positiveZ));
                        outf.WriteLine(String.Format("#116 = {0:f4} (OBRABOTKA POD Z=0)", negativeZ));

                        #region draw test1
                        /*
                        Line LL = new Line((Point3d)pa.First, (Point3d)pa.Second);
                        LL.ColorIndex = 1;
                        acBlkTblRec.AppendEntity(LL);
                        tr.AddNewlyCreatedDBObject(LL, true);
                       
                        Line LLL = new Line((Point3d)inTOP, (Point3d)outTOP);
                        LLL.ColorIndex = 1;
                        acBlkTblRec.AppendEntity(LLL);
                        tr.AddNewlyCreatedDBObject(LLL, true);

                        
                        Line LLLL = new Line((Point3d)cenTOP, (Point3d)cenBottom);
                        LLLL.ColorIndex = 1;
                        acBlkTblRec.AppendEntity(LLLL);
                        tr.AddNewlyCreatedDBObject(LLLL, true);

                        Line LLLLL = new Line((Point3d)bendAxe.First, (Point3d)bendAxe.Second);
                        LLLLL.ColorIndex = 1;
                        acBlkTblRec.AppendEntity(LLLLL);
                        tr.AddNewlyCreatedDBObject(LLLLL, true);

                        Line PP = new Line((Point3d)outTOP, (Point3d)gabaritQ);
                        PP.ColorIndex = 2;
                        acBlkTblRec.AppendEntity(PP);
                        tr.AddNewlyCreatedDBObject(PP, true);

                        Line PPP = new Line((Point3d)gabaritB, (Point3d)gabaritQ);
                        PPP.ColorIndex = 2;
                        acBlkTblRec.AppendEntity(PPP);
                        tr.AddNewlyCreatedDBObject(PPP, true);

                        Line PPPP = new Line((Point3d)inBottom, (Point3d)inTOP);
                        PPPP.ColorIndex = 2;
                        acBlkTblRec.AppendEntity(PPPP);
                        tr.AddNewlyCreatedDBObject(PPPP, true);
                        */
                        #endregion

                    }

                    outf.WriteLine("G65P1401");
                    outf.WriteLine("M1");
                    outf.WriteLine();

                    counter++;
                }

                //outf.WriteLine(String.Format("G43{0}", G43H));
                tr.Commit();
                Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
            }
        }
        public void CNC_bends(ref string line, WorkClasses.Node node, double L, double Lp, double Ls, double R, double toolR, double gabaritX, double gabaritY, double gabaritZ)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            quaternion tempNodeNormal = node.GetNodesNormalsByNoFictiveBends(ref container);

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                UCS ucs = node.CreateNodeUCS(L, ref container);
                UCS Ucs = node.CreateNodeUCS(L + Lp + Ls, ref container);

                int counter = 0;
                for (int i = 0; i < node.Bends_Numers_Array.Count; i++)
                {
                    WorkClasses.Bend b = container.Bends[node.Bends_Numers_Array[i]];
                    if (b.IsFictive()) { line += (b.Numer + 1).ToString() + " ; Fictive ;"; line += ";;;;;;;;"; continue; }

                    line += (b.Numer + 1).ToString() + " ;1;";
                    line += (ucs.FromACS(b.MidPoint).GetZ() >= 0.0) ? "+ ;" : "- ;";

                    //---------------------                    
                    quaternion bendNormal = b.Normal - b.MidPoint;
                    plane gabaritTOP = new plane(ucs.ToACS(new quaternion(0, 10, 0, gabaritZ / 2.0)), ucs.ToACS(new quaternion(0, 100, 0, gabaritZ / 2.0)), ucs.ToACS(new quaternion(0, 0, 10, gabaritZ / 2.0)));
                    plane gabaritBottom = new plane(ucs.ToACS(new quaternion(0, 10, 0, -gabaritZ / 2.0)), ucs.ToACS(new quaternion(0, 100, 0, -gabaritZ / 2.0)), ucs.ToACS(new quaternion(0, 0, 10, -gabaritZ / 2.0)));
                    quaternion Q1 = gabaritTOP.IntersectWithVector((b.MidPoint + b.Start) / 2.0, (b.MidPoint + b.Start) / 2.0 + bendNormal);
                    quaternion Q2 = gabaritTOP.IntersectWithVector(b.MidPoint, b.Normal);
                    Q1 = ucs.FromACS(Q1);
                    Q2 = ucs.FromACS(Q2);
                    complex cQ1 = new complex(Q1.GetX(), Q1.GetY());
                    complex cQ2 = new complex(Q2.GetX(), Q2.GetY());
                    matrix Int_Mat = Common.IntersectCircleAndLine(R, cQ1, cQ2);
                    if (Int_Mat.GetAt(0, 4) < 0) { /*ERRROR*/return; }
                    complex iP1 = new complex(Int_Mat.GetAt(0, 0), Int_Mat.GetAt(0, 1));
                    complex iP2 = new complex(Int_Mat.GetAt(0, 2), Int_Mat.GetAt(0, 3));
                    complex iP = ((cQ2 - iP1).abs() < (cQ2 - iP2).abs()) ? iP1 : iP2;
                    quaternion iQ = ucs.ToACS(new quaternion(0.0, iP.real(), iP.imag(), gabaritZ / 2.0));
                    quaternion cenTOP = iQ;// gabaritTOP.IntersectWithVector(iQ, iQ + bendNormal);
                    quaternion cenBottom = gabaritBottom.IntersectWithVector(iQ, iQ + bendNormal);
                    Q2 = ucs.ToACS(Q2);
                    quaternion outTOP = Q2 - iQ; quaternion inTOP = iQ - Q2;
                    outTOP /= outTOP.abs(); inTOP /= inTOP.abs();
                    outTOP *= toolR; inTOP *= toolR;
                    outTOP = iQ + outTOP; inTOP = iQ + inTOP;

                    // MessageBox.Show(String.Format("{0},{1},{2}\n{3},{4},{5}\n{6},{7},{8}",
                    //iQ.GetX(), iQ.GetY(), iQ.GetZ(), cenTOP.GetX(), cenTOP.GetY(), cenTOP.GetZ(), cenBottom.GetX(), cenBottom.GetY(), cenBottom.GetZ()));
                    //--------------------------
                    double gabaritR = Math.Sqrt(gabaritX * gabaritX / 4.0 + gabaritY * gabaritY / 4.0);
                    matrix gabaritMat = Common.IntersectCircleAndLine(gabaritR, cQ1, cQ2);
                    complex gabaritP1 = new complex(gabaritMat.GetAt(0, 0), gabaritMat.GetAt(0, 1));
                    complex gabaritP2 = new complex(gabaritMat.GetAt(0, 2), gabaritMat.GetAt(0, 3));
                    complex gabaritP = ((cQ2 - gabaritP1).abs() < (cQ2 - gabaritP2).abs()) ? gabaritP1 : gabaritP2;
                    quaternion gabaritQ = ucs.ToACS(new quaternion(0.0, gabaritP.real(), gabaritP.imag(), gabaritZ / 2.0));
                    //---------------------

                    bool bp = ((node.Position - b.Start).abs() < (node.Position - b.End).abs()) ? true : false;//kой край на реброто е във възела
                    Pair<quaternion, quaternion> bendAxe = (bp == true) ?
                        new Pair<quaternion, quaternion>(ucs.FromACS(b.Start), ucs.FromACS(b.End)) :
                        new Pair<quaternion, quaternion>(ucs.FromACS(b.End), ucs.FromACS(b.Start));

                    Pair<quaternion, quaternion> pa = new Pair<quaternion, quaternion>(ucs.FromACS(b.MidPoint), ucs.FromACS(b.Normal));
                    quaternion ucsOriginDescriptor = new quaternion();
                    cenTOP = ucs.FromACS(cenTOP); cenBottom = ucs.FromACS(cenBottom); inTOP = ucs.FromACS(inTOP); outTOP = ucs.FromACS(outTOP);
                    gabaritQ = ucs.FromACS(gabaritQ);
                    //  double test = Math.Sqrt(cenTOP.GetX() * cenTOP.GetX() + cenTOP.GetY() * cenTOP.GetY());
                    // MessageBox.Show(test.ToString());


                    double baseAng = 0.0;
                    if (((b.Normal - b.MidPoint) / (tempNodeNormal - node.Position)).absV() < Constants.zero_dist)
                    {

                        line += string.Format(" {0} ;", 0.0);//alpha
                        line += string.Format(" {0} ;", 0.0);//beta
                        line += string.Format(" {0} ;", 0.0);
                        line += string.Format(" {0} ;", 0.0);


                        Pair<complex, complex> benAxeXY = new Pair<complex, complex>(
                             new complex(bendAxe.First.GetX(), bendAxe.First.GetY()), new complex(bendAxe.Second.GetX(), bendAxe.Second.GetY()));
                        matrix Int_M = Common.IntersectCircleAndLine(R, benAxeXY.First, benAxeXY.Second);
                        double baseAng2 = (benAxeXY.Second - benAxeXY.First).arg();
                        if (baseAng2 > Math.PI) { baseAng2 = baseAng2 - 2 * Math.PI; }

                        line += string.Format(" {0} ;", baseAng2 * 180.0 / Math.PI);
                        line += string.Format(" {0} ;", gabaritZ / 2.0 + 5.0);
                        line += string.Format(" {0} ;", -gabaritZ / 2.0 - 2.0);
                    }
                    else
                    {
                        quaternion nB1 = ucs.FromACS(b.Normal);
                        quaternion nB2 = ucs.FromACS(b.MidPoint);
                        complex cN1 = new complex(nB1.GetX(), nB1.GetY());
                        complex cN2 = new complex(nB2.GetX(), nB2.GetY());
                        line2d projectTo_ucsXY_Line = new line2d(cN1, cN2);

                        complex normalTo_projectTo_ucsXY_Line = new complex(projectTo_ucsXY_Line.A, projectTo_ucsXY_Line.B);
                        normalTo_projectTo_ucsXY_Line /= normalTo_projectTo_ucsXY_Line.abs();
                        normalTo_projectTo_ucsXY_Line *= -1.0;

                        baseAng = normalTo_projectTo_ucsXY_Line.arg();
                        if (baseAng > Math.PI) { baseAng = baseAng - 2 * Math.PI; }

                        double kAng = ConstantsAndSettings.MachineData_alpha_direction ? -1.0 : 1.0;
                        line += string.Format(" {0} ;", -((-baseAng + Math.PI / 2.0) * 180.0 / Math.PI) * kAng);

                        pa.First.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        pa.Second.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);

                        bendAxe.First.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        bendAxe.Second.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);

                        ucsOriginDescriptor.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);

                        cenTOP.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        cenBottom.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        inTOP.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        outTOP.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);
                        gabaritQ.set_rotateAroundAxe(new quaternion(0, 0, 0, 100.0), -baseAng + Math.PI / 2.0);

                        pa = new Pair<quaternion, quaternion>(ucs.ToACS(pa.First), ucs.ToACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(ucs.ToACS(bendAxe.First), ucs.ToACS(bendAxe.Second));
                        ucsOriginDescriptor = ucs.ToACS(ucsOriginDescriptor);
                        cenTOP = ucs.ToACS(cenTOP); cenBottom = ucs.ToACS(cenBottom); inTOP = ucs.ToACS(inTOP); outTOP = ucs.ToACS(outTOP);
                        gabaritQ = ucs.ToACS(gabaritQ);

                        pa = new Pair<quaternion, quaternion>(Ucs.FromACS(pa.First), Ucs.FromACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(Ucs.FromACS(bendAxe.First), Ucs.FromACS(bendAxe.Second));
                        ucsOriginDescriptor = Ucs.FromACS(ucsOriginDescriptor);
                        cenTOP = Ucs.FromACS(cenTOP); cenBottom = Ucs.FromACS(cenBottom); inTOP = Ucs.FromACS(inTOP); outTOP = Ucs.FromACS(outTOP);
                        gabaritQ = Ucs.FromACS(gabaritQ);

                        quaternion normal = (pa.Second - pa.First) * 1000.0;
                        pa = new Pair<quaternion, quaternion>(pa.First, pa.First + normal);
                        double k = (pa.Second.GetX() < pa.First.GetX()) ? 1.0 : -1.0;

                        double ang = (pa.Second - pa.First).angTo(new quaternion(0, 0, 0, 100));
                        ang *= k;
                        line += string.Format(" {0} ;", ang * 180.0 / Math.PI);

                        pa.First.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        pa.Second.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);

                        bendAxe.First.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        bendAxe.Second.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);

                        ucsOriginDescriptor.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);

                        cenTOP.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        cenBottom.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        inTOP.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        outTOP.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);
                        gabaritQ.set_rotateAroundAxe(new quaternion(0, 0, 100.0, 0.0), ang);

                        pa = new Pair<quaternion, quaternion>(Ucs.ToACS(pa.First), Ucs.ToACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(Ucs.ToACS(bendAxe.First), Ucs.ToACS(bendAxe.Second));
                        ucsOriginDescriptor = Ucs.ToACS(ucsOriginDescriptor);
                        cenTOP = Ucs.ToACS(cenTOP); cenBottom = Ucs.ToACS(cenBottom); inTOP = Ucs.ToACS(inTOP); outTOP = Ucs.ToACS(outTOP);
                        gabaritQ = Ucs.ToACS(gabaritQ);

                        pa = new Pair<quaternion, quaternion>(ucs.FromACS(pa.First), ucs.FromACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(ucs.FromACS(bendAxe.First), ucs.FromACS(bendAxe.Second));
                        ucsOriginDescriptor = ucs.FromACS(ucsOriginDescriptor);
                        cenTOP = ucs.FromACS(cenTOP); cenBottom = ucs.FromACS(cenBottom); inTOP = ucs.FromACS(inTOP); outTOP = ucs.FromACS(outTOP);
                        gabaritQ = ucs.FromACS(gabaritQ);
                        string str1 = string.Format(" {0} ;", ucsOriginDescriptor.GetX());
                        string str2 = string.Format(" {0} ;", ucsOriginDescriptor.GetZ());
                        double backX = ucsOriginDescriptor.GetX();
                        double backZ = ucsOriginDescriptor.GetZ();

                        quaternion moveQ = new quaternion(0, -backX, 0.0, -backZ);
                        pa = new Pair<quaternion, quaternion>(pa.First + moveQ, pa.Second + moveQ);
                        bendAxe = new Pair<quaternion, quaternion>(bendAxe.First + moveQ, bendAxe.Second + moveQ);
                        ucsOriginDescriptor += moveQ;
                        cenTOP += moveQ; cenBottom += moveQ; inTOP += moveQ; outTOP += moveQ;
                        gabaritQ += moveQ;

                        Pair<complex, complex> benAxeXY = new Pair<complex, complex>(
                            new complex(bendAxe.First.GetX(), bendAxe.First.GetY()), new complex(bendAxe.Second.GetX(), bendAxe.Second.GetY()));
                        matrix Int_M = Common.IntersectCircleAndLine(R, benAxeXY.First, benAxeXY.Second);

                        complex Int_P1 = new complex(Int_M.GetAt(0, 0), Int_M.GetAt(0, 1));
                        complex Int_P2 = new complex(Int_M.GetAt(0, 2), Int_M.GetAt(0, 3));
                        complex Int_P = ((benAxeXY.Second - Int_P1).abs() < (benAxeXY.Second - Int_P2).abs()) ? Int_P1 : Int_P2;
                        double baseAng2 = (benAxeXY.Second - benAxeXY.First).arg();
                        if (baseAng2 > Math.PI) { baseAng2 = baseAng2 - 2 * Math.PI; }

                        line += string.Format(" {0} ;", baseAng2 * 180.0 / Math.PI);
                        line += str1;
                        line += str2;

                        //----------------------
                        pa = new Pair<quaternion, quaternion>(ucs.ToACS(pa.First), ucs.ToACS(pa.Second));
                        bendAxe = new Pair<quaternion, quaternion>(ucs.ToACS(bendAxe.First), ucs.ToACS(bendAxe.Second));
                        ucsOriginDescriptor = ucs.ToACS(ucsOriginDescriptor);
                        cenTOP = ucs.ToACS(cenTOP); cenBottom = ucs.ToACS(cenBottom); inTOP = ucs.ToACS(inTOP); outTOP = ucs.ToACS(outTOP);
                        gabaritQ = ucs.ToACS(gabaritQ);
                        quaternion gabaritB = gabaritQ + cenBottom - cenTOP;
                        quaternion inBottom = inTOP + cenBottom - cenTOP;

                        double positiveZ = (ucs.FromACS(gabaritQ).GetZ() > ucs.FromACS(inTOP).GetZ()) ? ucs.FromACS(gabaritQ).GetZ() + 5.0 : ucs.FromACS(inTOP).GetZ() + 5.0;
                        double negativeZ = (ucs.FromACS(gabaritB).GetZ() < ucs.FromACS(inBottom).GetZ()) ? ucs.FromACS(gabaritB).GetZ() - 2.0 : ucs.FromACS(inBottom).GetZ() - 2.0;

                        line += string.Format(" {0} ;", positiveZ);
                        line += string.Format(" {0} ;", negativeZ);

                        #region draw test1
                        /*
                        Line LL = new Line((Point3d)pa.First, (Point3d)pa.Second);
                        LL.ColorIndex = 1;
                        acBlkTblRec.AppendEntity(LL);
                        tr.AddNewlyCreatedDBObject(LL, true);
                       
                        Line LLL = new Line((Point3d)inTOP, (Point3d)outTOP);
                        LLL.ColorIndex = 1;
                        acBlkTblRec.AppendEntity(LLL);
                        tr.AddNewlyCreatedDBObject(LLL, true);

                        
                        Line LLLL = new Line((Point3d)cenTOP, (Point3d)cenBottom);
                        LLLL.ColorIndex = 1;
                        acBlkTblRec.AppendEntity(LLLL);
                        tr.AddNewlyCreatedDBObject(LLLL, true);

                        Line LLLLL = new Line((Point3d)bendAxe.First, (Point3d)bendAxe.Second);
                        LLLLL.ColorIndex = 1;
                        acBlkTblRec.AppendEntity(LLLLL);
                        tr.AddNewlyCreatedDBObject(LLLLL, true);

                        Line PP = new Line((Point3d)outTOP, (Point3d)gabaritQ);
                        PP.ColorIndex = 2;
                        acBlkTblRec.AppendEntity(PP);
                        tr.AddNewlyCreatedDBObject(PP, true);

                        Line PPP = new Line((Point3d)gabaritB, (Point3d)gabaritQ);
                        PPP.ColorIndex = 2;
                        acBlkTblRec.AppendEntity(PPP);
                        tr.AddNewlyCreatedDBObject(PPP, true);

                        Line PPPP = new Line((Point3d)inBottom, (Point3d)inTOP);
                        PPPP.ColorIndex = 2;
                        acBlkTblRec.AppendEntity(PPPP);
                        tr.AddNewlyCreatedDBObject(PPPP, true);
                        */
                        #endregion

                    }


                    counter++;
                }

                //outf.WriteLine(String.Format("G43{0}", G43H));
                tr.Commit();
                Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
            }
        }
    }
}
