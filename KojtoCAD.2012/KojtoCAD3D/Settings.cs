using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
using KojtoCAD.KojtoCAD3D.WorkClasses;

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
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.Settings))]

namespace KojtoCAD.KojtoCAD3D
{
    public class Settings
    {
        public Containers container = ContextVariablesProvider.Container;
        //----- Settings -------------------------------------
        //[CommandMethod("KojtoCAD_3D", "KCAD_SETTINGS", null, CommandFlags.Modal, null, "KojtoCAD_3D", "KojtoCAD_Settings")]
        [CommandMethod("KojtoCAD_3D", "KCAD_SETTINGS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/Settings.htm", "")]
        public void KojtoCAD_Settings()
        {
            
            
            SettingsForm form = new SettingsForm();
            form.ShowDialog();
            if (form.DialogResult == DialogResult.OK)
            {
                ConstantsAndSettings.FictivebendsLayer = form.FictiveBendsLayer;
                ConstantsAndSettings.BendsLayer = form.BendsLayer;
                ConstantsAndSettings.SetProjectName(form.ProjectName);
                ConstantsAndSettings.SetNormalLength(form.Length_To_Visualise_Normals);
                ConstantsAndSettings.SetPerepherialBendsNormalDirection(form.Pereferial_Bends_Normals_Direction);
                ConstantsAndSettings.SetGlassThickness(form.GlassThicknes);
                ConstantsAndSettings.SetFixing_A(form.Fixing_A);
                ConstantsAndSettings.SetFixing_B(form.Fixing_B);
                ConstantsAndSettings.SetFixing_pereferial_A(form.Fixing_pA);
                ConstantsAndSettings.SetFixing_pereferial_B(form.Fixing_pB);
                ConstantsAndSettings.PartLength = form.PartLength;
                ConstantsAndSettings.PartWidth = form.PartWidth;
                ConstantsAndSettings.PartHeight = form.PartHeight;
                ConstantsAndSettings.MachineL = form.MachineL;
                ConstantsAndSettings.MachineLp = form.MachineLp;
                ConstantsAndSettings.MachineLs = form.MachineLs;
                ConstantsAndSettings.minR = form.minR;
                ConstantsAndSettings.toolR = form.toolR;
                ConstantsAndSettings.NoPereferialFixngBlockName = form.NoPereferialFixngBlockName;
                ConstantsAndSettings.PereferialFixngBlockName = form.PereferialFixngBlockName;
                ConstantsAndSettings.NoPereferialFixngLayerName = form.NoPereferialFixngLayerName;
                ConstantsAndSettings.PereferialFixngLayerName = form.PereferialFixngLayerName;
                ConstantsAndSettings.Node3DLayer = form.Node3DLayer;
                ConstantsAndSettings.Node3DBlock = form.Node3DBlock;
                ConstantsAndSettings.Bends3DLayer = form.Bends3DLayer;
                ConstantsAndSettings.Bends3DBlock = form.Bends3DBlock;
                ConstantsAndSettings.EndsOfBends3DLayer = form.EndsOfBends3DLayer;
                ConstantsAndSettings.EndsOfBends3DBlock = form.EndsOfBends3DBlock;
                ConstantsAndSettings.DistanceNodeToNozzle = form.DistanceNodeToNozzle;

                #region CSV Nodes Columns
                ConstantsAndSettings.CSV_Node_Columns[0] = form.CSV_Node_Columns[0];
                ConstantsAndSettings.CSV_Node_Columns[1] = form.CSV_Node_Columns[1];
                ConstantsAndSettings.CSV_Node_Columns[2] = form.CSV_Node_Columns[2];
                ConstantsAndSettings.CSV_Node_Columns[3] = form.CSV_Node_Columns[3];
                ConstantsAndSettings.CSV_Node_Columns[4] = form.CSV_Node_Columns[4];
                ConstantsAndSettings.CSV_Node_Columns[5] = form.CSV_Node_Columns[5];
                ConstantsAndSettings.CSV_Node_Columns[6] = form.CSV_Node_Columns[6];
                ConstantsAndSettings.CSV_Node_Columns[7] = form.CSV_Node_Columns[7];
                ConstantsAndSettings.CSV_Node_Columns[8] = form.CSV_Node_Columns[8];
                ConstantsAndSettings.CSV_Node_Columns[9] = form.CSV_Node_Columns[9];
                ConstantsAndSettings.CSV_Node_Columns[10] = form.CSV_Node_Columns[10];
                ConstantsAndSettings.CSV_Node_Columns[11] = form.CSV_Node_Columns[11];
                ConstantsAndSettings.CSV_Node_Columns[12] = form.CSV_Node_Columns[12];
                ConstantsAndSettings.CSV_Node_Columns[13] = form.CSV_Node_Columns[13];
                #endregion

                #region CSV Bends Columns
                ConstantsAndSettings.CSV_Bend_Columns[0] = form.CSV_Bend_Columns[0];
                ConstantsAndSettings.CSV_Bend_Columns[1] = form.CSV_Bend_Columns[1];
                ConstantsAndSettings.CSV_Bend_Columns[2] = form.CSV_Bend_Columns[2];
                ConstantsAndSettings.CSV_Bend_Columns[3] = form.CSV_Bend_Columns[3];
                ConstantsAndSettings.CSV_Bend_Columns[4] = form.CSV_Bend_Columns[4];
                ConstantsAndSettings.CSV_Bend_Columns[5] = form.CSV_Bend_Columns[5];
                ConstantsAndSettings.CSV_Bend_Columns[6] = form.CSV_Bend_Columns[6];
                ConstantsAndSettings.CSV_Bend_Columns[7] = form.CSV_Bend_Columns[7];
                ConstantsAndSettings.CSV_Bend_Columns[8] = form.CSV_Bend_Columns[8];
                ConstantsAndSettings.CSV_Bend_Columns[9] = form.CSV_Bend_Columns[9];
                ConstantsAndSettings.CSV_Bend_Columns[10] = form.CSV_Bend_Columns[10];
                #endregion

                #region CSV Triangles Columns
                ConstantsAndSettings.CSV_Triangle_Columns[0] = form.CSV_Triangle_Columns[0];
                ConstantsAndSettings.CSV_Triangle_Columns[1] = form.CSV_Triangle_Columns[1];
                ConstantsAndSettings.CSV_Triangle_Columns[2] = form.CSV_Triangle_Columns[2];
                ConstantsAndSettings.CSV_Triangle_Columns[3] = form.CSV_Triangle_Columns[3];
                ConstantsAndSettings.CSV_Triangle_Columns[4] = form.CSV_Triangle_Columns[4];
                ConstantsAndSettings.CSV_Triangle_Columns[5] = form.CSV_Triangle_Columns[5];
                ConstantsAndSettings.CSV_Triangle_Columns[6] = form.CSV_Triangle_Columns[6];
                ConstantsAndSettings.CSV_Triangle_Columns[7] = form.CSV_Triangle_Columns[7];
                ConstantsAndSettings.CSV_Triangle_Columns[8] = form.CSV_Triangle_Columns[8];
                #endregion

                ConstantsAndSettings.MachineData_alpha_direction = form.MachineData_alpha_direction;
                ConstantsAndSettings.halfGlassFugue = form.halfGlassFugue;
                ConstantsAndSettings.DoubleGlass_h1 = form.DoubleGlass_h1;
                ConstantsAndSettings.DoubleGlass_h2 = form.DoubleGlass_h2;
                ConstantsAndSettings.Single_or_Double_Glass = form.Single_or_Double_Glass;

                container.SetBendsNormals();
                container.SetNodesNormals();

                for (int i = 0; i < container.Bends.Count; i++)
                {
                    container.Bends[i].SetFirstTriangleOffset(ConstantsAndSettings.halfGlassFugue);
                    container.Bends[i].SetSecondTriangleOffset(ConstantsAndSettings.halfGlassFugue);
                }

                ConstantsAndSettings.nodeDensity = form.nodeDensity;
                ConstantsAndSettings.bendDensity = form.bendDensity;
                ConstantsAndSettings.nozzleDensity = form.nozzleDensity;
                ConstantsAndSettings.glassDensity = form.glassDensity;
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SET_CUT_SOLID_LK", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/EXPLICIT_PARAMS.htm", "")]
        public void KojtoCAD_3D_Set_cutSolidLenK()
        {
            
            PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
            pDoubleOpts_.Message = "\n Enter new Value: ";
            pDoubleOpts_.DefaultValue = ConstantsAndSettings.cutSolidsLenK;
            pDoubleOpts_.AllowZero = false;
            pDoubleOpts_.AllowNegative = false;

            PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
            if (pDoubleRes_.Status == PromptStatus.OK)
            {
                ConstantsAndSettings.cutSolidsLenK = pDoubleRes_.Value;
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SET_CUT_SOLID_TH", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/EXPLICIT_PARAMS.htm", "")]
        public void KojtoCAD_3D_Set_cutSolidTH()
        {
            
            PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
            pDoubleOpts_.Message = "\n Enter new cutSolid Thicknes Value: ";
            pDoubleOpts_.DefaultValue = ConstantsAndSettings.cutSolidsThicknes;
            pDoubleOpts_.AllowZero = false;
            pDoubleOpts_.AllowNegative = false;

            PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
            if (pDoubleRes_.Status == PromptStatus.OK)
            {
                ConstantsAndSettings.cutSolidsThicknes = pDoubleRes_.Value;
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SET_CUT_SOLID_ER", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/EXPLICIT_PARAMS.htm", "")]
        public void KojtoCAD_3D_Set_cutSolidsExtrudeRatio()
        {
            
            PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
            pDoubleOpts_.Message = "\n Enter new cutSolid Extrude Ratio Value: ";
            pDoubleOpts_.DefaultValue = ConstantsAndSettings.cutSolidsExtrudeRatio;
            pDoubleOpts_.AllowZero = false;
            pDoubleOpts_.AllowNegative = false;

            PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
            if (pDoubleRes_.Status == PromptStatus.OK)
            {
                ConstantsAndSettings.cutSolidsExtrudeRatio = pDoubleRes_.Value;
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SET_BEND_EXPLICIT_PARAMETERS", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/EXPLICIT_PARAMS.htm", "")]
        public void KojtoCAD_3D_SetBendExplicitParameters()
        {
            
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter an option ";
            pKeyOpts.Keywords.Add("LengthKoeficient");
            pKeyOpts.Keywords.Add("Height");
            pKeyOpts.Keywords.Default = "LengthKoeficient";
            pKeyOpts.AllowNone = true;

            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.OK)
            {
                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "LengthKoeficient": KojtoCAD_3D_SetBendExplicitParametersA(0); break;
                        case "Height": KojtoCAD_3D_SetBendExplicitParametersA(1); break;
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void KojtoCAD_3D_SetBendExplicitParametersA(int k)
        {
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter an option ";
            pKeyOpts.Keywords.Add("Start");
            pKeyOpts.Keywords.Add("End");
            pKeyOpts.Keywords.Default = "Start";
            pKeyOpts.AllowNone = true;

            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.OK)
            {
                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "Start": KojtoCAD_3D_SetBendExplicitParametersB(k, 0); break;
                        case "End": KojtoCAD_3D_SetBendExplicitParametersB(k, 1); break;
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void KojtoCAD_3D_SetBendExplicitParametersB(int k, int l)
        {
            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
            pIntOpts.Message = "\nEnter the Bend  Numer ";

            pIntOpts.AllowZero = false;
            pIntOpts.AllowNegative = false;

            PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
            if (pIntRes.Status == PromptStatus.OK)
            {
                int N = pIntRes.Value - 1;

                double bendValue = (k == 0) ?
                     ((l == 0) ? container.Bends[N].explicit_cutSolidsLenK_Start : container.Bends[N].explicit_cutSolidsLenK_End) :
                     ((l == 0) ? container.Bends[N].explicit_cutSolidsThicknes_Start : container.Bends[N].explicit_cutSolidsThicknes_End);

                PromptDoubleOptions pDoubleOpts_ = new PromptDoubleOptions("");
                pDoubleOpts_.Message = "\n Enter Value: ";
                pDoubleOpts_.DefaultValue = bendValue;
                pDoubleOpts_.AllowZero = true;
                pDoubleOpts_.AllowNegative = true;

                PromptDoubleResult pDoubleRes_ = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts_);
                if (pDoubleRes_.Status == PromptStatus.OK)
                {
                    double value = pDoubleRes_.Value;
                    switch (k)
                    {
                        case 0:
                            switch (l)
                            {
                                case 0: container.Bends[N].explicit_cutSolidsLenK_Start = value; break;
                                case 1: container.Bends[N].explicit_cutSolidsLenK_End = value; break;
                            }
                            break;
                        case 1:
                            switch (l)
                            {
                                case 0: container.Bends[N].explicit_cutSolidsThicknes_Start = value; break;
                                case 1: container.Bends[N].explicit_cutSolidsThicknes_End = value; break;
                            }
                            break;
                    }
                }

                //     WorkClasses.Bend bend = container.Bends[N];
                // MessageBox.Show(string.Format("{0}\n{1}\n{2}\n{3}",bend.explicit_cutSolidsThicknes_Start,bend.explicit_cutSolidsThicknes_End,
                //    bend.explicit_cutSolidsLenK_Start,bend.explicit_cutSolidsLenK_End));
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_CHANGE_3D_MASHINE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/EXPLICIT_PARAMS.htm", "")]
        public void KojtoCAD_3D_Change_3D_Mashine()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

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
                            switch (ConstantsAndSettings.solidFuncVariant)
                            {
                                case 0:
                                    ConstantsAndSettings.solidFuncVariant = 1;
                                    MessageBox.Show("3d mashine = BASE", "Change 3D Mashine");
                                    break;
                                case 1:
                                    ConstantsAndSettings.solidFuncVariant = 0;
                                    MessageBox.Show("3d mashine = ADDITIONAL", "Change 3D Mashine");
                                    break;
                            }
                            break;
                        case "Help":
                            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/EXPLICIT_PARAMS.htm");
                            break;
                    }
                }
            }
            catch { }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_CHANGE_PICK_TRIM_VARIANT", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/EXPLICIT_PARAMS.htm", "")]
        public void KojtoCAD_3D_Change_Pick_Trim_Variant()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

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
                            switch (ConstantsAndSettings.solidTrimPiksVariant)
                            {
                                case 0:
                                    ConstantsAndSettings.solidTrimPiksVariant = 1;
                                    MessageBox.Show("Trimming ON !", "Change 3D Mashine");
                                    break;
                                case 1:
                                    ConstantsAndSettings.solidTrimPiksVariant = 0;
                                    MessageBox.Show("Trimming OFF !", "Change 3D Mashine");
                                    break;
                            }
                            break;
                        case "Help":
                            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/EXPLICIT_PARAMS.htm");
                            break;
                    }
                }
            }
            catch { }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_EX_RATIO_BY_BEND", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/EXPLICIT_PARAMS.htm", "")]
        public void KojtoCAD_3D_ExtrudeRatioByBend()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
                {
                    do
                    {
                        PromptPointResult pPtRes;
                        PromptPointOptions pPtOpts = new PromptPointOptions("");

                        // Prompt for the first point
                        pPtOpts.Message = "\nEnter the first Point of the Bend: ";
                        pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                        if (pPtRes.Status == PromptStatus.OK)
                        {
                            Point3d ptFirst = pPtRes.Value;

                            // Prompt for the second point
                            pPtOpts.Message = "\nEnter the second Point of the Bend: ";
                            pPtOpts.UseBasePoint = true;
                            pPtOpts.BasePoint = ptFirst;
                            pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                            if (pPtRes.Status == PromptStatus.OK)
                            {
                                Point3d ptSecond = pPtRes.Value;

                                if (ptSecond.DistanceTo(ptFirst) >= ConstantsAndSettings.MinBendLength)
                                {
                                    Pair<quaternion, quaternion> pb = new Pair<quaternion, quaternion>(new quaternion(0, ptFirst.X, ptFirst.Y, ptFirst.Z), new quaternion(0, ptSecond.X, ptSecond.Y, ptSecond.Z));
                                    Bend Bend = null;
                                    foreach (Bend bend in container.Bends)
                                    {
                                        if (bend == pb)
                                        {
                                            Bend = bend;
                                            break;
                                        }
                                    }
                                    if ((object)Bend != null)
                                    {
                                        if (!Bend.IsFictive())
                                        {
                                            int kT = 0;//first
                                            #region select triangle side
                                            if (Bend.SecondTriangleNumer >= 0)
                                            {
                                                //------ prompt for triangle
                                                PromptPointResult tPtRes;
                                                PromptPointOptions tPtOpts = new PromptPointOptions("");
                                                tPtOpts.Message = "\nEnter the third Point of the Triangle: ";
                                                tPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(tPtOpts);
                                                if (tPtRes.Status == PromptStatus.OK)
                                                {
                                                    Triplet<quaternion, quaternion, quaternion> pTR = new Triplet<quaternion, quaternion, quaternion>(new quaternion(0, ptFirst.X, ptFirst.Y, ptFirst.Z),
                                                            new quaternion(0, ptSecond.X, ptSecond.Y, ptSecond.Z),
                                                            new quaternion(0, tPtRes.Value.X, tPtRes.Value.Y, tPtRes.Value.Z));
                                                    Triangle TR = null;
                                                    foreach (Triangle tr in container.Triangles)
                                                    {
                                                        if (tr == pTR)
                                                        {
                                                            TR = tr;
                                                            break;
                                                        }
                                                    }
                                                    if ((object)TR != null)
                                                    {
                                                        if (Bend.FirstTriangleNumer != TR.Numer)
                                                            kT = 1;//second
                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show("\nTriangle not found - E R R O R  !", "E R R O R - Selection Triangle", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        ed.WriteMessage("\nTriangle not found - E R R O R  !");
                                                    }
                                                }
                                            }
                                            #endregion

                                            double extrudeRatio = (kT == 0) ? Bend.explicit_first_cutExtrudeRatio : Bend.explicit_second_cutExtrudeRatio;

                                            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                                            pDoubleOpts.Message = "\nEnter the Distance : ";

                                            pDoubleOpts.AllowZero = true;
                                            pDoubleOpts.AllowNegative = true;
                                            pDoubleOpts.DefaultValue = extrudeRatio;
                                            PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                                            if (pDoubleRes.Status == PromptStatus.OK)
                                            {
                                                // double newRatio = pDoubleRes.Value;
                                                if (kT == 0)
                                                    Bend.explicit_first_cutExtrudeRatio = pDoubleRes.Value;
                                                else
                                                    Bend.explicit_second_cutExtrudeRatio = pDoubleRes.Value; ;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                    while (MessageBox.Show("Select next ?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_EXPLICIT_CUTTING_METHOD_FOR_ENDS_IN_NODE", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CUTTING_BENDS_IN_NODES.htm", "")]
        public void KojtoCAD_3D_Set_ExplicitCuttingMethodForEndsOf3D_Bends()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
                {
                    PromptPointResult pPtRes;
                    PromptPointOptions pPtOpts = new PromptPointOptions("");

                    pPtOpts.Message = "\nEnter the Point of the Node: ";
                    pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);

                    if (pPtRes.Status == PromptStatus.OK)
                    {
                        quaternion nq = new quaternion(0, pPtRes.Value.X, pPtRes.Value.Y, pPtRes.Value.Z);
                        WorkClasses.Node Node = null;
                        foreach (WorkClasses.Node node in container.Nodes)
                        {
                            if (node == nq)
                            {
                                Node = node;
                                break;
                            }
                        }
                        if ((object)Node != null)
                        {
                            //----------------
                            PromptKeywordOptions pop_ = new PromptKeywordOptions("");
                            pop_.AppendKeywordsToMessage = true;
                            pop_.AllowNone = false;
                            pop_.Keywords.Add("Base");
                            pop_.Keywords.Add("Additional");
                            pop_.Keywords.Add("Global");
                            pop_.Keywords.Default = "Base";
                            PromptResult res_ = ed.GetKeywords(pop_);
                            if (res_.Status == PromptStatus.OK)
                            {
                                switch (res_.StringResult)
                                {
                                    case "Base": Node.ExplicitCuttingMethodForEndsOf3D_Bends = 0; break;
                                    case "Additional": Node.ExplicitCuttingMethodForEndsOf3D_Bends = 1; break;
                                    case "Global": Node.ExplicitCuttingMethodForEndsOf3D_Bends = -1; break;
                                }
                            }
                            //----------------
                        }
                        //
                    }
                    //
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_CLEAR_EXPLICIT_CUTTING_METHOD_FOR_ALL", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CUTTING_BENDS_IN_NODES.htm", "")]
        public void KojtoCAD_3D_Clear_ExplicitCuttingMethodForEndsOf3D_Bends()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
            {
                PromptKeywordOptions pop = new PromptKeywordOptions("");
                pop.AppendKeywordsToMessage = true;
                pop.AllowNone = false;
                pop.Keywords.Add("Run");
                pop.Keywords.Add("Help");
                pop.Keywords.Default = "Run";
                PromptResult res = ed.GetKeywords(pop);

                if (res.Status == PromptStatus.OK)
                {
                    switch (res.StringResult)
                    {
                        case "Run": foreach (WorkClasses.Node Node in container.Nodes)
                            {
                                Node.ExplicitCuttingMethodForEndsOf3D_Bends = -1;
                            } break;
                        case "Help":
                            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/CUTTING_BENDS_IN_NODES.htm");
                            break;
                    }

                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
