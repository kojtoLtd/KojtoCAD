using System;
using System.Windows.Forms;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
#endif

namespace KojtoCAD.KojtoCAD3D
{
    public partial class SettingsForm : Form
    {
        #region members
        private string BendsLayer_;
        public string BendsLayer { get { return (BendsLayer_); } set { BendsLayer_ = value; } }

        private string FictiveBendsLayer_;
        public string FictiveBendsLayer { get { return (FictiveBendsLayer_); } set { FictiveBendsLayer_ = value; } }

        private string ProjectName_;
        public string ProjectName { get { return (ProjectName_); } set { ProjectName_ = value; } }

        private double Length_To_Visualise_Normals_;
        public double Length_To_Visualise_Normals { get { return (Length_To_Visualise_Normals_); } set { Length_To_Visualise_Normals_ = value; } }

        private int Pereferial_Bends_Normals_Direction_;
        public int Pereferial_Bends_Normals_Direction { get { return (Pereferial_Bends_Normals_Direction_); } set { Pereferial_Bends_Normals_Direction_ = value; } }

        private double GlassThicknes_;
        public double GlassThicknes { get { return (GlassThicknes_); } set { GlassThicknes_ = value; } }

        private double fixing_A_;
        public double Fixing_A { get { return (fixing_A_); } set { fixing_A_ = value; } }

        private double fixing_B_;
        public double Fixing_B { get { return (fixing_B_); } set { fixing_B_ = value; } }

        private double fixing_pA_;
        public double Fixing_pA { get { return (fixing_pA_); } set { fixing_pA_ = value; } }

        private double fixing_pB_;
        public double Fixing_pB { get { return (fixing_pB_); } set { fixing_pB_ = value; } }

        private double PartLength_;
        public  double PartLength { get { return (PartLength_); } set { PartLength_ = value; } }

        private double PartWidth_;
        public  double PartWidth { get { return (PartWidth_); } set { PartWidth_ = value; } }

        private double PartHeight_;
        public  double PartHeight { get { return (PartHeight_); } set { PartHeight_ = value; } }

        private double MachineL_;
        public  double MachineL { get { return (MachineL_); } set { MachineL_ = value; } }

        private double MachineLp_;
        public  double MachineLp { get { return (MachineLp_); } set { MachineLp_ = value; } }

        private double MachineLs_ ;
        public  double MachineLs { get { return (MachineLs_); } set { MachineLs_ = value; } }

        private double minR_;
        public double minR { get { return (minR_); } set { minR_ = value; } }

        private double toolR_;
        public double toolR { get { return (toolR_); } set { toolR_ = value; } }

        private string NoPereferialFixngBlockName_;
        public string NoPereferialFixngBlockName { get { return (NoPereferialFixngBlockName_); } set { NoPereferialFixngBlockName_ = value; } }

        private string PereferialFixngBlockName_ ;
        public  string PereferialFixngBlockName { get { return (PereferialFixngBlockName_); } set { PereferialFixngBlockName_ = value; } }

        private string NoPereferialFixngLayerName_ = "LayerFix";
        public  string NoPereferialFixngLayerName { get { return (NoPereferialFixngLayerName_); } set { NoPereferialFixngLayerName_ = value; } }

        private string PereferialFixngLayerName_ = "LayerFix1";
        public string PereferialFixngLayerName { get { return (PereferialFixngLayerName_); } set { PereferialFixngLayerName_ = value; } }

        private string Node3DLayer_;
        public string Node3DLayer { get { return (Node3DLayer_); } set { Node3DLayer_ = value; } }

        private string Node3DBlock_;
        public string Node3DBlock { get { return (Node3DBlock_); } set { Node3DBlock_ = value; } }

        private string Bends3DLayer_;
        public string Bends3DLayer { get { return (Bends3DLayer_); } set { Bends3DLayer_ = value; } }

        private string Bends3DBlock_;
        public  string Bends3DBlock { get { return (Bends3DBlock_); } set { Bends3DBlock_ = value; } }

        private string EndsOfBends3DLayer_ = "La2";
        public string EndsOfBends3DLayer { get { return (EndsOfBends3DLayer_); } set { EndsOfBends3DLayer_ = value; } }

        private  string EndsOfBends3DBlock_ = "testBlockBend";
        public  string EndsOfBends3DBlock { get { return (EndsOfBends3DBlock_); } set { EndsOfBends3DBlock_ = value; } }

        private double DistanceNodeToNozzle_;
        public  double DistanceNodeToNozzle { get { return (DistanceNodeToNozzle_); } set { DistanceNodeToNozzle_ = value; } }

        private bool[] CSV_Node_Columns_ = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public bool[] CSV_Node_Columns { get { return (CSV_Node_Columns_); } set { CSV_Node_Columns_ = value; } }

        private bool[] CSV_Bend_Columns_ = new bool[] { false, false, false, false, false, false, false, false, false, false, false };
        public bool[] CSV_Bend_Columns { get { return (CSV_Bend_Columns_); } set { CSV_Bend_Columns_ = value; } }

        private bool[] CSV_Triangle_Columns_ = new bool[] { false, false, false, false, false, false, false, false, false };
        public bool[] CSV_Triangle_Columns { get { return (CSV_Triangle_Columns_); } set { CSV_Triangle_Columns_ = value; } }

        private bool MachineData_alpha_direction_ = true;
        public bool MachineData_alpha_direction { get { return (MachineData_alpha_direction_); } set { MachineData_alpha_direction_ = value; } }

        private  double halfGlassFugue_ = 30.0;
        public  double halfGlassFugue { get { return (halfGlassFugue_); } set { halfGlassFugue_ = value; } }

        private  double DoubleGlass_h1_ = 13.52;
        public  double DoubleGlass_h1 { get { return (DoubleGlass_h1_); } set { DoubleGlass_h1_ = value; } }

        private double DoubleGlass_h2_ = 13.52;
        public double DoubleGlass_h2 { get { return (DoubleGlass_h2_); } set { DoubleGlass_h2_ = value; } }

        private int Single_or_Double_Glass_ = 1;
        public int Single_or_Double_Glass { get { return (Single_or_Double_Glass_); } set { Single_or_Double_Glass_ = value; } }

        private double nodeDensity_ = 1.0;
        public double nodeDensity { get { return (nodeDensity_); } set { nodeDensity_ = value; } }

        private double bendDensity_ = 1.0;
        public double bendDensity { get { return (bendDensity_); } set { bendDensity_ = value; } }

        private double nozzleDensity_ = 1.0;
        public double nozzleDensity { get { return (nozzleDensity_); } set { nozzleDensity_ = value; } }

        private double glassDensity_ = 1.0;
        public double glassDensity { get { return (glassDensity_); } set { glassDensity_ = value; } }
        
        #endregion

        public SettingsForm()
        {
            InitializeComponent();

            textBox_Project_Name.Text = " "+UtilityClasses.ConstantsAndSettings.ProjectName;
            textBox_Length_To_Visualise_Normals.Text = String.Format(" {0:f2}",UtilityClasses.ConstantsAndSettings.NormlLengthToShow);
            textBox_Glass_thickness.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.Thickness_of_the_Glass);
            textBox_A.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.fixing_A);
            textBox_B.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.fixing_B);
            textBox_pA.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.fixing_pA);
            textBox_pB.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.fixing_pB);
            textBox_Part_Length.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.PartLength);
            textBox_Part_Width.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.PartWidth);
            textBox_Part_Height.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.PartHeight);
            textBox_Machine_Data_L.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.MachineL);
            textBox_Machine_Data_Lp.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.MachineLp);
            textBox_Machine_Data_Ls.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.MachineLs);
            textBox_Tool_Data_minR.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.minR);
            textBox_Tool_Data_toolR.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.toolR);
            textBox_Nozle_R.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.DistanceNodeToNozzle);

            comboBox_Type_of_peripheral_Normals.Items.Add("Parallel to the normal of the triangle");
            comboBox_Type_of_peripheral_Normals.Items.Add("Parallel to the axis oZ in WCS");
            comboBox_Type_of_peripheral_Normals.Items.Add("Parallel to the axis oY in WCS");
            comboBox_Type_of_peripheral_Normals.Items.Add("Parallel to the axis oX in WCS");
            try
            {
                comboBox_Type_of_peripheral_Normals.SelectedIndex = UtilityClasses.ConstantsAndSettings.PerepherialBendsNormalDirection;
            }
            catch { }
        

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {                
                LayerTable acLyrTbl = acTrans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                int index = 0;
                int indexNoFictive = 0;
                int indexFictive = 0;
                int NoPereferialIndex = 0;
                int PereferialIndex = 0;
                int NodesIndex = 0;
                int Bends3DIndex = 0;
                int EndsOfBends3DLayerIndex = 0;
                foreach (ObjectId acObjId in acLyrTbl)
                {
                    LayerTableRecord acLyrTblRec = acTrans.GetObject(acObjId, OpenMode.ForRead) as LayerTableRecord;
                    string recName = acLyrTblRec.Name.ToUpper();
                    if (recName  == UtilityClasses.ConstantsAndSettings.BendsLayer.ToUpper())
                        indexNoFictive = index;
                    if (recName == UtilityClasses.ConstantsAndSettings.FictivebendsLayer.ToUpper())
                        indexFictive = index;
                    if (recName == UtilityClasses.ConstantsAndSettings.NoPereferialFixngLayerName.ToUpper())
                        NoPereferialIndex = index;
                    if (recName == UtilityClasses.ConstantsAndSettings.PereferialFixngLayerName.ToUpper())
                        PereferialIndex = index;
                    if (recName == UtilityClasses.ConstantsAndSettings.Node3DLayer.ToUpper())
                        NodesIndex = index;
                    if (recName == UtilityClasses.ConstantsAndSettings.Bends3DLayer.ToUpper())
                        Bends3DIndex = index;
                    if (recName == UtilityClasses.ConstantsAndSettings.EndsOfBends3DLayer.ToUpper())
                        EndsOfBends3DLayerIndex = index;

                    comboBox_Bends_Layer.Items.Add(acLyrTblRec.Name);
                    comboBox_Fictive_Bends_Layer.Items.Add(acLyrTblRec.Name);
                    comboBox_NoPereferial_Fixing_Layer.Items.Add(acLyrTblRec.Name);
                    comboBox_Pereferial_Fixing_Layer.Items.Add(acLyrTblRec.Name);
                    comboBox_Nodes_Layer.Items.Add(acLyrTblRec.Name);
                    comboBox_Bends3d_Layer.Items.Add(acLyrTblRec.Name);
                    comboBox_Nozle3d_Layer.Items.Add(acLyrTblRec.Name);
                    index++;
                }
                try
                {
                    comboBox_Bends_Layer.SelectedIndex = indexNoFictive;
                    comboBox_Fictive_Bends_Layer.SelectedIndex = indexFictive;
                    comboBox_NoPereferial_Fixing_Layer.SelectedIndex = NoPereferialIndex;
                    comboBox_Pereferial_Fixing_Layer.SelectedIndex = PereferialIndex;
                    comboBox_Nodes_Layer.SelectedIndex = NodesIndex;
                    comboBox_Bends3d_Layer.SelectedIndex = Bends3DIndex;
                    comboBox_Nozle3d_Layer.SelectedIndex = EndsOfBends3DLayerIndex;
                    index = 0;
                }
                catch { }
                
                //-----------------------                
                NoPereferialIndex = 0;
                PereferialIndex = 0;
                NodesIndex = 0;
                Bends3DIndex = 0;
                EndsOfBends3DLayerIndex = 0;
                BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                foreach (ObjectId acObjId in acBlkTbl)
                {
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acObjId, OpenMode.ForWrite) as BlockTableRecord;
                    if (acBlkTblRec.IsLayout || acBlkTblRec.IsAnonymous) continue;
                    string recName = acBlkTblRec.Name.ToUpper();

                    if (recName == UtilityClasses.ConstantsAndSettings.NoPereferialFixngBlockName.ToUpper())
                        NoPereferialIndex = index;

                    if (recName == UtilityClasses.ConstantsAndSettings.PereferialFixngBlockName.ToUpper())
                        PereferialIndex = index;

                    if (recName == UtilityClasses.ConstantsAndSettings.Node3DBlock.ToUpper())
                        NodesIndex = index;
                    if (recName == UtilityClasses.ConstantsAndSettings.Bends3DBlock.ToUpper())
                        Bends3DIndex = index;
                    if (recName == UtilityClasses.ConstantsAndSettings.EndsOfBends3DBlock.ToUpper())
                        EndsOfBends3DLayerIndex = index;

                    try
                    {
                        comboBox_Block_Name_NoPereferial_Fixing.Items.Add(acBlkTblRec.Name);
                        comboBox_Block_Name_Pereferial_Fixing.Items.Add(acBlkTblRec.Name);
                        comboBox_Nodes_Block.Items.Add(acBlkTblRec.Name);
                        comboBox_Bends3d_Block.Items.Add(acBlkTblRec.Name);
                        comboBox_Nozle3d_Block.Items.Add(acBlkTblRec.Name);
                        index++;
                    }
                    catch
                    {
                    }

                }
                try
                {
                    comboBox_Block_Name_NoPereferial_Fixing.SelectedIndex = NoPereferialIndex;
                    comboBox_Block_Name_Pereferial_Fixing.SelectedIndex = PereferialIndex;
                    comboBox_Nodes_Block.SelectedIndex = NodesIndex;
                    comboBox_Bends3d_Block.SelectedIndex = Bends3DIndex;
                    comboBox_Nozle3d_Block.SelectedIndex = EndsOfBends3DLayerIndex;
                    index = 0;
                }
                catch { }
            }

            // CSV - Nodes
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = true;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
  
            checkBox_CCV_Pereferial.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[0];
            checkBox_CSV_Fictive.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[1];
            checkBox_CSV_Normal_Coordinates.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[2];
            checkBox_CSV_cncNormal_Coordinates.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[3];
            checkBox_sorted_Bends_By_Number.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[4];
            checkBox_sorted_Triangles_By_Number.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[5];
            checkBox_Torsion.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[6];
            checkBox_cncTorsion.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[7];
            checkBox_Angle_between_Bend_and_Normal.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[8];
            checkBox_Angle_between_Bend_and_cncNormal.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[9];
            checkBox_CSV_Angle_Bend_and_Node_Tangetial_Plane.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[10];
            checkBox_CSV_Angle_Bend_and_cncNode_Tangetial_Plane.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[11];
            checkBox_sorted_Bends_counterclockwise.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[12];
            checkBox_sorted_Triangles_counterclockwise.Checked = UtilityClasses.ConstantsAndSettings.CSV_Node_Columns[13];

            // CSV - Bends
            pictureBox_CSV_Convex_Concav.Visible = false;
            pictureBox_CSV_Bends.Visible = true;
            pictureBox_CSV_Bends_Triangles_Normals_Angulars.Visible = false;

            checkBox_CSV_Bends_Position_Type.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[0];
            checkBox_CSV_Bends_Fictive_or_NoFictive.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[1];
            checkBox_CSV_BEnds_convex_or_concave.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[2];
            checkBox_CSV_Bends_between_Nodes.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[3];
            checkBox_CSV_Bends_between_Triangles.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[4];
            checkBox_CSV_Bend_Length_By_Mesh.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[5];
            checkBox_Angle_between_triangles_Normals.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[6];
            checkBox_CSV_Bends_starting_point_of_the_normal.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[7];
            checkBox_CSV_Bends_end_point_of_the_normal.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[8];
            checkBox_CSV_starting_point_of_the_bend.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[9];
            checkBox_CSV_Bends_End_Point_of_the_Bend.Checked = UtilityClasses.ConstantsAndSettings.CSV_Bend_Columns[10];

            //CSV - Triangles
            checkBox_CSV_Triangles_Pereferial.Checked = UtilityClasses.ConstantsAndSettings.CSV_Triangle_Columns[0];
            checkBox_CSV_Triangles_Fictive.Checked = UtilityClasses.ConstantsAndSettings.CSV_Triangle_Columns[1];
            checkBox_CSV_Triangles_Bends.Checked = UtilityClasses.ConstantsAndSettings.CSV_Triangle_Columns[2];
            checkBox_CSV_Triangles_Nodes.Checked = UtilityClasses.ConstantsAndSettings.CSV_Triangle_Columns[3];
            checkBox_CSV_Triangles_Area.Checked = UtilityClasses.ConstantsAndSettings.CSV_Triangle_Columns[4];
            checkBox_CSV_Triangles_MassCenter.Checked = UtilityClasses.ConstantsAndSettings.CSV_Triangle_Columns[5];
            checkBox_CSV_Triangles_Normal.Checked = UtilityClasses.ConstantsAndSettings.CSV_Triangle_Columns[6];
            checkBox_CSV_Triangles_Angles.Checked = UtilityClasses.ConstantsAndSettings.CSV_Triangle_Columns[7];
            checkBox_CSV_Bends_Length.Checked = UtilityClasses.ConstantsAndSettings.CSV_Triangle_Columns[8];

            //-- Machine Data
            checkBox_MachineData_alpha_direction.Checked = UtilityClasses.ConstantsAndSettings.MachineData_alpha_direction;

            //-- Glass
            textBox_Glass_h1.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.DoubleGlass_h1);
            textBox_Glass_h2.Text = String.Format(" {0:f2}", UtilityClasses.ConstantsAndSettings.DoubleGlass_h2);
            textBox_Fugue_Distance.Text = String.Format(" {0:f8}", UtilityClasses.ConstantsAndSettings.halfGlassFugue);

            if (UtilityClasses.ConstantsAndSettings.Single_or_Double_Glass == 1)
            {
                textBox_Glass_h1.Enabled = false;
                textBox_Glass_h2.Enabled = false;
                checkBox_Single_or_Double.Checked = true;
            }
            else
            {
                textBox_Glass_h1.Enabled = true;
                textBox_Glass_h2.Enabled = true;
                checkBox_Single_or_Double.Checked = false;
            }

            textBoxNodeDensity.Text = String.Format(" {0:f4}", UtilityClasses.ConstantsAndSettings.nodeDensity);
            textBoxBendDensity.Text = String.Format(" {0:f4}", UtilityClasses.ConstantsAndSettings.bendDensity);
            textBoxNozzleDensity.Text = String.Format(" {0:f4}", UtilityClasses.ConstantsAndSettings.nozzleDensity);
            textBoxGlassDensity.Text = String.Format(" {0:f4}", UtilityClasses.ConstantsAndSettings.glassDensity);
            
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            #region Length_To_Visualise_Normals
            try
            {
                Length_To_Visualise_Normals_ = double.Parse(textBox_Length_To_Visualise_Normals.Text);
                if (Length_To_Visualise_Normals_ <= 0.0)
                {
                    MessageBox.Show("Missing Corect Length To Visualise Normals > 0.0 !", "E R R O R");
                    tabControl_Settings.SelectedIndex = 1;
                    textBox_Length_To_Visualise_Normals.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect Length To Visualise Normals_ !", "E R R O R");
                tabControl_Settings.SelectedIndex = 1;
                textBox_Length_To_Visualise_Normals.Focus();
                return;
            }
            #endregion

            #region glass height
            try
            {
                GlassThicknes_ = double.Parse(textBox_Glass_thickness.Text);
                if (GlassThicknes_ <= 0.0)
                {
                    MessageBox.Show("Missing Corect Glass Thicknes_ > 0.0 !", "E R R O R");
                    tabControl_Settings.SelectedIndex = 2;
                    textBox_Glass_thickness.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect Glass Thicknes_ > 0.0 !", "E R R O R");
                tabControl_Settings.SelectedIndex = 2;
                textBox_Glass_thickness.Focus();
                return;
            }
            #endregion

            #region glass halfFugue Distance
            try
            {
                halfGlassFugue_ = double.Parse(textBox_Fugue_Distance.Text);
                if (halfGlassFugue_ < 0.0)
                {
                    MessageBox.Show("Missing Corect Fugue Distance > 0.0 !", "E R R O R");
                    tabControl_Settings.SelectedIndex = 2;
                    textBox_Fugue_Distance.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect Fugue Distance > 0.0 !", "E R R O R");
                tabControl_Settings.SelectedIndex = 2;
                textBox_Fugue_Distance.Focus();
                return;
            }          
            #endregion

            if (checkBox_Single_or_Double.Checked)
                Single_or_Double_Glass = 1;
            else
                Single_or_Double_Glass = 0;

            #region glass h1
            if (checkBox_Single_or_Double.Checked == false)
            {
                try
                {
                    DoubleGlass_h1_ = double.Parse(textBox_Glass_h1.Text);
                    if(DoubleGlass_h1_ <= 0.0)
                    {
                        MessageBox.Show("Missing Corect Glass h1 Distance > 0.0 !", "E R R O R");
                        tabControl_Settings.SelectedIndex = 2;
                        textBox_Glass_h1.Focus();
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Missing Corect Glass h1 Distance > 0.0 !", "E R R O R");
                    tabControl_Settings.SelectedIndex = 2;
                    textBox_Glass_h1.Focus();
                    return;
                }
            }
            #endregion

            #region glass h2
            if (checkBox_Single_or_Double.Checked == false)
            {
                try
                {
                    DoubleGlass_h2_ = double.Parse(textBox_Glass_h2.Text);
                    if (DoubleGlass_h2_ <= 0.0)
                    {
                        MessageBox.Show("Missing Corect Glass h2 Distance > 0.0 !", "E R R O R");
                        tabControl_Settings.SelectedIndex = 2;
                        textBox_Glass_h2.Focus();
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Missing Corect Glass h2 Distance > 0.0 !", "E R R O R");
                    tabControl_Settings.SelectedIndex = 2;
                    textBox_Glass_h2.Focus();
                    return;
                }
            }
            #endregion

            #region fixing data A
            try
            {
                fixing_A_ = double.Parse(textBox_A.Text);
                if (fixing_A_ <= 0.0)
                {
                    MessageBox.Show("Fixing Distance >= 0  ?", "Fixing Distance E R R O R");
                    tabControl_Settings.SelectedIndex = 3;
                    textBox_A.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Fixing Distance >= 0  ?", "Fixing Distance E R R O R");
                tabControl_Settings.SelectedIndex = 3;
                textBox_A.Focus();
                return;
            }
            #endregion

            #region fixing data B
            try
            {
                fixing_B_ = double.Parse(textBox_B.Text);
                if (fixing_B_ <= 0.0)
                {
                    MessageBox.Show("Fixing Distance >= 0  ?", "Fixing Distance E R R O R");
                    tabControl_Settings.SelectedIndex = 3;
                    textBox_B.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Fixing Distance >= 0  ?", "Fixing Distance E R R O R");
                tabControl_Settings.SelectedIndex = 3;
                textBox_B.Focus();
                return;
            }
            #endregion

            #region fixing data pA
            try
            {
                fixing_pA_ = double.Parse(textBox_pA.Text);
                if (fixing_pA_ <= 0.0)
                {
                    MessageBox.Show("Pereferial Fixing Distance >= 0  ?", "Pereferial Fixing Distance E R R O R");
                    tabControl_Settings.SelectedIndex = 3;
                    textBox_pA.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Pereferial Fixing Distance >= 0  ?", "Pereferial Fixing Distance E R R O R");
                tabControl_Settings.SelectedIndex = 3;
                textBox_pA.Focus();
                return;
            }
            #endregion

            #region fixing data pB
            try
            {
                fixing_pB_ = double.Parse(textBox_pB.Text);
                if (fixing_pB_ <= 0.0)
                {
                    MessageBox.Show("Pereferial Fixing Distance >= 0  ?", "Pereferial Fixing Distance E R R O R");
                    tabControl_Settings.SelectedIndex = 3;
                    textBox_pB.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Pereferial Fixing Distance >= 0  ?", "Pereferial Fixing Distance E R R O R");
                tabControl_Settings.SelectedIndex = 3;
                textBox_pB.Focus();
                return;
            }
            #endregion

            #region Part Length
            try
            {
                PartLength_ = double.Parse(textBox_Part_Length.Text);
                if (PartLength_ <= 0.0)
                {
                    MessageBox.Show("Part Length  >= 0  ?", "CAM Data E R R O R");
                    tabControl_Settings.SelectedIndex = 4;
                    tabControl_CAM.SelectedIndex = 0;
                    textBox_Part_Length.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Part Length  >= 0  ?", "CAM Data E R R O R");
                tabControl_Settings.SelectedIndex = 4;
                tabControl_CAM.SelectedIndex = 0;
                textBox_Part_Length.Focus();
                return;
            }
            #endregion

            #region Part Width
            try
            {
                PartWidth_ = double.Parse(textBox_Part_Width.Text);
                if (PartWidth_ <= 0.0)
                {
                    MessageBox.Show("Part Width  >= 0  ?", "CAM Data E R R O R");
                    tabControl_Settings.SelectedIndex = 4;
                    tabControl_CAM.SelectedIndex = 0;
                    textBox_Part_Width.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Part Width  >= 0  ?", "CAM Data E R R O R");
                tabControl_Settings.SelectedIndex = 4;
                tabControl_CAM.SelectedIndex = 0;
                textBox_Part_Width.Focus();
                return;
            }
            #endregion

            #region Part Height
            try
            {
                PartHeight_ = double.Parse(textBox_Part_Height.Text);
                if (PartWidth_ <= 0.0)
                {
                    MessageBox.Show("Part Height  >= 0  ?", "CAM Data E R R O R");
                    tabControl_Settings.SelectedIndex = 4;
                    tabControl_CAM.SelectedIndex = 0;
                    textBox_Part_Height.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Part Height  >= 0  ?", "CAM Data E R R O R");
                tabControl_Settings.SelectedIndex = 4;
                tabControl_CAM.SelectedIndex = 0;
                textBox_Part_Height.Focus();
                return;
            }
            #endregion

            #region Machine Data L
            try
            {
                MachineL_ = double.Parse(textBox_Machine_Data_L.Text);
            }
            catch
            {
                MessageBox.Show("Distance L  =  ?", "CAM Data E R R O R");
                tabControl_Settings.SelectedIndex = 4;
                tabControl_CAM.SelectedIndex = 1;
                textBox_Machine_Data_L.Focus();
                return;
            }
            #endregion

            #region Machine Data Lp
            try
            {
                MachineLp_ = double.Parse(textBox_Machine_Data_Lp.Text);
            }
            catch
            {
                MessageBox.Show("Distance Lprim  =  ?", "CAM Data E R R O R");
                tabControl_Settings.SelectedIndex = 4;
                tabControl_CAM.SelectedIndex = 1;
                textBox_Machine_Data_Lp.Focus();
                return;
            }
            #endregion

            #region Machine Data Ls
            try
            {
                MachineLs_ = double.Parse(textBox_Machine_Data_Ls.Text);
            }
            catch
            {
                MessageBox.Show("Distance Lsecond  =  ?", "CAM Data E R R O R");
                tabControl_Settings.SelectedIndex = 4;
                tabControl_CAM.SelectedIndex = 1;
                textBox_Machine_Data_Ls.Focus();
                return;
            }
            #endregion

            #region  minR
            try
            {
                minR_ = double.Parse(textBox_Tool_Data_minR.Text);
                if (minR_ <= 0.0)
                {
                    MessageBox.Show("min R  > 0  ?", "CAM Data E R R O R");
                    tabControl_Settings.SelectedIndex = 4;
                    tabControl_CAM.SelectedIndex = 2;
                    textBox_Tool_Data_minR.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("min R  > 0  ?", "CAM Data E R R O R");
                tabControl_Settings.SelectedIndex = 4;
                tabControl_CAM.SelectedIndex = 2;
                textBox_Tool_Data_minR.Focus();
                return;
            }
            #endregion

            #region toolR
            try
            {
                toolR_ = double.Parse(textBox_Tool_Data_toolR.Text);
                if (toolR_ <= 0.0)
                {
                    MessageBox.Show("Tool R  > 0  ?", "CAM Data E R R O R");
                    tabControl_Settings.SelectedIndex = 4;
                    tabControl_CAM.SelectedIndex = 2;
                    textBox_Tool_Data_toolR.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Tool R  > 0  ?", "CAM Data E R R O R");
                tabControl_Settings.SelectedIndex = 4;
                tabControl_CAM.SelectedIndex = 2;
                textBox_Tool_Data_toolR.Focus();
                return;
            }
            #endregion

            #region Distance Node To Nozzle
            try
            {
                DistanceNodeToNozzle_ = double.Parse(textBox_Nozle_R.Text);
                if (DistanceNodeToNozzle_ <= 0.0)
                {
                    MessageBox.Show("Distance Node To Nozzle  > 0  ?", "E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_CAM.SelectedIndex = 2;
                    textBox_Nozle_R.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Distance Node To Nozzle  > 0  ?", "E R R O R");
                tabControl_Settings.SelectedIndex = 5;
                tabControl_CAM.SelectedIndex = 2;
                textBox_Nozle_R.Focus();
                return;
            }
            #endregion

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                LayerTable acLyrTbl = acTrans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                #region bends layer
                if (comboBox_Bends_Layer.Text != "")
                {                    
                    if (!acLyrTbl.Has(comboBox_Bends_Layer.Text))
                    {
                        MessageBox.Show("\nMissing Layer " + comboBox_Bends_Layer.Text + " !", "Layer E R R O R");
                        tabControl_Settings.SelectedIndex = 0;
                        comboBox_Bends_Layer.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Layer " + comboBox_Bends_Layer.Text + " !", "Layer E R R O R");
                    tabControl_Settings.SelectedIndex = 0;
                    comboBox_Bends_Layer.Focus();
                    return;
                }
                BendsLayer_ = comboBox_Bends_Layer.Text;
                #endregion;

                #region fictive bends layer
                if (comboBox_Fictive_Bends_Layer.Text != "")
                {
                    if (!acLyrTbl.Has(comboBox_Fictive_Bends_Layer.Text))
                    {
                        MessageBox.Show("\nMissing Layer " + comboBox_Fictive_Bends_Layer.Text + " !", "Layer E R R O R");
                        tabControl_Settings.SelectedIndex = 0;
                        comboBox_Fictive_Bends_Layer.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Layer " + comboBox_Fictive_Bends_Layer.Text + " !", "Layer E R R O R");
                    tabControl_Settings.SelectedIndex = 0;
                    comboBox_Fictive_Bends_Layer.Focus();
                    return;
                }
                FictiveBendsLayer_ = comboBox_Fictive_Bends_Layer.Text;
                #endregion

                #region Bends Layer = Fictive Bends Layer
                if (comboBox_Fictive_Bends_Layer.Text.ToUpper() == comboBox_Bends_Layer.Text.ToUpper())
                {
                    MessageBox.Show("\n Bends Layer = Fictive Bends Layer !", "Layer E R R O R");
                    tabControl_Settings.SelectedIndex = 0;
                    comboBox_Fictive_Bends_Layer.Focus();
                    return;
                }
                #endregion

                #region  type of pereferial Normals
                Pereferial_Bends_Normals_Direction_ = -1;
                switch (comboBox_Type_of_peripheral_Normals.Text)
                {
                    case "Parallel to the normal of the triangle": Pereferial_Bends_Normals_Direction_ = 0; break;
                    case "Parallel to the axis oZ in WCS": Pereferial_Bends_Normals_Direction_ = 1; break;
                    case "Parallel to the axis oY in WCS": Pereferial_Bends_Normals_Direction_ = 2; break;
                    case "Parallel to the axis oX in WCS": Pereferial_Bends_Normals_Direction_ = 3; break;
                }
                if (Pereferial_Bends_Normals_Direction_ < 0)
                {
                    MessageBox.Show("\n Wrong type of pereferial Normals !", "Normals Direction E R R O R");
                    tabControl_Settings.SelectedIndex = 0;
                    comboBox_Type_of_peripheral_Normals.Focus();
                    return;
                }
                #endregion

                #region project name
                if (textBox_Project_Name.Text.Length < 0)
                {
                    MessageBox.Show("\n Missing Project Name !", "Project Name E R R O R");
                    tabControl_Settings.SelectedIndex = 0;
                    textBox_Project_Name.Focus();
                    return;
                }
                ProjectName_ = textBox_Project_Name.Text;
                #endregion

                #region NoPreferial Fixing Block
                if (comboBox_Block_Name_NoPereferial_Fixing.Text != "")
                {
                    if (!acBlkTbl.Has(comboBox_Block_Name_NoPereferial_Fixing.Text))
                    {
                        MessageBox.Show("\nMissing Fixing Block " + comboBox_Block_Name_NoPereferial_Fixing.Text + " !", "Block E R R O R");
                        tabControl_Settings.SelectedIndex = 3;
                        comboBox_Block_Name_NoPereferial_Fixing.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Fixing Block " + comboBox_Block_Name_NoPereferial_Fixing.Text + " !", "Block E R R O R");
                    tabControl_Settings.SelectedIndex = 3;
                    comboBox_Block_Name_NoPereferial_Fixing.Focus();
                    return;
                }
                NoPereferialFixngBlockName_ = comboBox_Block_Name_NoPereferial_Fixing.Text;
                #endregion;

                #region Preferial Fixing Block
                if (comboBox_Block_Name_Pereferial_Fixing.Text != "")
                {
                    if (!acBlkTbl.Has(comboBox_Block_Name_Pereferial_Fixing.Text))
                    {
                        MessageBox.Show("\nMissing Fixing Block " + comboBox_Block_Name_Pereferial_Fixing.Text + " !", "Block E R R O R");
                        tabControl_Settings.SelectedIndex = 3;
                        comboBox_Block_Name_Pereferial_Fixing.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Fixing Block " + comboBox_Block_Name_Pereferial_Fixing.Text + " !", "Block E R R O R");
                    tabControl_Settings.SelectedIndex = 3;
                    comboBox_Block_Name_Pereferial_Fixing.Focus();
                    return;
                }
                PereferialFixngBlockName_ = comboBox_Block_Name_Pereferial_Fixing.Text;
                #endregion;

                #region NoPreferial Fixing Layer
                if (comboBox_NoPereferial_Fixing_Layer.Text != "")
                {
                    if (!acLyrTbl.Has(comboBox_NoPereferial_Fixing_Layer.Text))
                    {
                        MessageBox.Show("\nMissing Layer " + comboBox_NoPereferial_Fixing_Layer.Text + " !", "Layer E R R O R");
                        tabControl_Settings.SelectedIndex = 3;
                        comboBox_NoPereferial_Fixing_Layer.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Layer " + comboBox_NoPereferial_Fixing_Layer.Text + " !", "Layer E R R O R");
                    tabControl_Settings.SelectedIndex = 3;
                    comboBox_NoPereferial_Fixing_Layer.Focus();
                    return;
                }
                NoPereferialFixngLayerName_ = comboBox_NoPereferial_Fixing_Layer.Text;
                #endregion;

                #region Preferial Fixing Layer
                if (comboBox_Pereferial_Fixing_Layer.Text != "")
                {
                    if (!acLyrTbl.Has(comboBox_Pereferial_Fixing_Layer.Text))
                    {
                        MessageBox.Show("\nMissing Layer " + comboBox_Pereferial_Fixing_Layer.Text + " !", "Layer E R R O R");
                        tabControl_Settings.SelectedIndex = 3;
                        comboBox_Pereferial_Fixing_Layer.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Layer " + comboBox_Pereferial_Fixing_Layer.Text + " !", "Layer E R R O R");
                    tabControl_Settings.SelectedIndex = 3;
                    comboBox_Pereferial_Fixing_Layer.Focus();
                    return;
                }
                PereferialFixngLayerName_ = comboBox_Pereferial_Fixing_Layer.Text;
                #endregion;

                #region bends3d layer
                if (comboBox_Bends3d_Layer.Text != "")
                {
                    if (!acLyrTbl.Has(comboBox_Bends3d_Layer.Text))
                    {
                        MessageBox.Show("\nMissing Layer " + comboBox_Bends3d_Layer.Text + " !", "Layer E R R O R");
                        tabControl_Settings.SelectedIndex = 5;
                        tabControl_3D.SelectedIndex = 1;
                        comboBox_Bends3d_Layer.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Layer " + comboBox_Bends3d_Layer.Text + " !", "Layer E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_3D.SelectedIndex = 1;
                    comboBox_Bends3d_Layer.Focus();
                    return;
                }
                Bends3DLayer_  = comboBox_Bends3d_Layer.Text;
                #endregion;

                #region ends of bends3d layer
                if (comboBox_Nozle3d_Layer.Text != "")
                {
                    if (!acLyrTbl.Has(comboBox_Nozle3d_Layer.Text))
                    {
                        MessageBox.Show("\nMissing Layer " + comboBox_Nozle3d_Layer.Text + " !", "Layer E R R O R");
                        tabControl_Settings.SelectedIndex = 5;
                        tabControl_3D.SelectedIndex = 2;
                        comboBox_Nozle3d_Layer.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Layer " + comboBox_Nozle3d_Layer.Text + " !", "Layer E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_3D.SelectedIndex = 2;
                    comboBox_Nozle3d_Layer.Focus();
                    return;
                }
                EndsOfBends3DLayer_ = comboBox_Nozle3d_Layer.Text;
                #endregion;

                #region nodes3d layer
                if (comboBox_Nodes_Layer.Text != "")
                {
                    if (!acLyrTbl.Has(comboBox_Nodes_Layer.Text))
                    {
                        MessageBox.Show("\nMissing Layer " + comboBox_Nodes_Layer.Text + " !", "Layer E R R O R");
                        tabControl_Settings.SelectedIndex = 5;
                        tabControl_3D.SelectedIndex = 0;
                        comboBox_Nodes_Layer.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Layer " + comboBox_Nodes_Layer.Text + " !", "Layer E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_3D.SelectedIndex = 0;
                    comboBox_Nodes_Layer.Focus();
                    return;
                }
                Node3DLayer_ = comboBox_Nodes_Layer.Text;
                #endregion;

                #region Node 3D Block
                if (comboBox_Nodes_Block.Text != "")
                {
                    if (!acBlkTbl.Has(comboBox_Nodes_Block.Text))
                    {
                        MessageBox.Show("\nMissing Node Block " + comboBox_Nodes_Block.Text + " !", "Block E R R O R");
                        tabControl_Settings.SelectedIndex = 5;
                        tabControl_3D.SelectedIndex = 0;
                        comboBox_Nodes_Block.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Node Block " + comboBox_Nodes_Block.Text + " !", "Block E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_3D.SelectedIndex = 0;
                    comboBox_Nodes_Block.Focus();
                    return; 
                }
                Node3DBlock_ = comboBox_Nodes_Block.Text;
                #endregion;

                #region Bends 3D Block
                if (comboBox_Bends3d_Block.Text != "")
                {
                    if (!acBlkTbl.Has(comboBox_Bends3d_Block.Text))
                    {
                        MessageBox.Show("\nMissing bend Block " + comboBox_Bends3d_Block.Text + " !", "Block E R R O R");
                        tabControl_Settings.SelectedIndex = 5;
                        tabControl_3D.SelectedIndex = 1;
                        comboBox_Bends3d_Block.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing Bend Block " + comboBox_Bends3d_Block.Text + " !", "Block E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_3D.SelectedIndex = 1;
                    comboBox_Bends3d_Block.Focus();
                    return;
                }
                Bends3DBlock_ = comboBox_Bends3d_Block.Text;
                #endregion;

                #region Ends of Bends 3D Block
                if (comboBox_Nozle3d_Block.Text != "")
                {
                    if (!acBlkTbl.Has(comboBox_Nozle3d_Block.Text))
                    {
                        MessageBox.Show("\nMissing bendEnd Block " + comboBox_Nozle3d_Block.Text + " !", "Block E R R O R");
                        tabControl_Settings.SelectedIndex = 5;
                        tabControl_3D.SelectedIndex = 2;
                        comboBox_Nozle3d_Block.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("\nMissing bendEnd Block " + comboBox_Nozle3d_Block.Text + " !", "Block E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_3D.SelectedIndex = 2;
                    comboBox_Nozle3d_Block.Focus();
                    return;
                }
                EndsOfBends3DBlock_ = comboBox_Nozle3d_Block.Text;
                #endregion;

            }

            #region GlassDensity
            try
            {
                glassDensity_ = double.Parse(textBoxGlassDensity.Text);
                if (glassDensity_ < 0.0)
                {
                    MessageBox.Show("Missing Corect Glass Density > 0.0 !", "E R R O R");
                    tabControl_Settings.SelectedIndex = 2;
                    textBoxGlassDensity.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect Glass Density > 0.0 !", "E R R O R");
                tabControl_Settings.SelectedIndex = 2;
                textBoxGlassDensity.Focus();
                return;
            }
            #endregion

            #region NodeDensity
            try
            {
                nodeDensity_ = double.Parse(textBoxNodeDensity.Text);
                if (glassDensity_ < 0.0)
                {
                    MessageBox.Show("Missing Corect Node Density > 0.0 !", "E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_3D.SelectedIndex = 0;
                    textBoxNodeDensity.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect Node Density > 0.0 !", "E R R O R");
                tabControl_Settings.SelectedIndex = 5;
                tabControl_3D.SelectedIndex = 0;
                textBoxNodeDensity.Focus();
                return;
            }
            #endregion

            #region BendDensity
            try
            {
                bendDensity_ = double.Parse(textBoxBendDensity.Text);
                if (glassDensity_ < 0.0)
                {
                    MessageBox.Show("Missing Corect Nozzle Density > 0.0 !", "E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_3D.SelectedIndex = 1;
                    textBoxBendDensity.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect Nozzle Density > 0.0 !", "E R R O R");
                tabControl_Settings.SelectedIndex = 5;
                tabControl_3D.SelectedIndex = 1;
                textBoxBendDensity.Focus();
                return;
            }
            #endregion

            #region NozzleDensity
            try
            {
                nozzleDensity_ = double.Parse(textBoxNozzleDensity.Text);
                if (glassDensity_ < 0.0)
                {
                    MessageBox.Show("Missing Corect Nozzle Density > 0.0 !", "E R R O R");
                    tabControl_Settings.SelectedIndex = 5;
                    tabControl_3D.SelectedIndex = 2;
                    textBoxNozzleDensity.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Missing Corect Nozzle Density > 0.0 !", "E R R O R");
                tabControl_Settings.SelectedIndex = 5;
                tabControl_3D.SelectedIndex = 2;
                textBoxNozzleDensity.Focus();
                return;
            }
            #endregion

            #region CSV Nodes Columns
            CSV_Node_Columns[0] =checkBox_CCV_Pereferial.Checked ;
            CSV_Node_Columns[1] = checkBox_CSV_Fictive.Checked;
            CSV_Node_Columns[2] = checkBox_CSV_Normal_Coordinates.Checked;           
            CSV_Node_Columns[3] = checkBox_CSV_cncNormal_Coordinates.Checked;
            CSV_Node_Columns[4] = checkBox_sorted_Bends_By_Number.Checked ;
            CSV_Node_Columns[5] = checkBox_sorted_Triangles_By_Number.Checked;
            CSV_Node_Columns[6] = checkBox_Torsion.Checked;
            CSV_Node_Columns[7] = checkBox_cncTorsion.Checked;
            CSV_Node_Columns[8] = checkBox_Angle_between_Bend_and_Normal.Checked ;
            CSV_Node_Columns[9] = checkBox_Angle_between_Bend_and_cncNormal.Checked;
            CSV_Node_Columns[10] = checkBox_CSV_Angle_Bend_and_Node_Tangetial_Plane.Checked;
            CSV_Node_Columns[11] = checkBox_CSV_Angle_Bend_and_cncNode_Tangetial_Plane.Checked;
            CSV_Node_Columns[12] = checkBox_sorted_Bends_counterclockwise.Checked;
            CSV_Node_Columns[13] = checkBox_sorted_Triangles_counterclockwise.Checked ;
            #endregion

            #region CSV Bends Columns
            CSV_Bend_Columns[0] = checkBox_CSV_Bends_Position_Type.Checked ;
            CSV_Bend_Columns[1] = checkBox_CSV_Bends_Fictive_or_NoFictive.Checked;
            CSV_Bend_Columns[2] = checkBox_CSV_BEnds_convex_or_concave.Checked;
            CSV_Bend_Columns[3] = checkBox_CSV_Bends_between_Nodes.Checked;
            CSV_Bend_Columns[4] = checkBox_CSV_Bends_between_Triangles.Checked;
            CSV_Bend_Columns[5] = checkBox_CSV_Bend_Length_By_Mesh.Checked;
            CSV_Bend_Columns[6] = checkBox_Angle_between_triangles_Normals.Checked;
            CSV_Bend_Columns[7] = checkBox_CSV_Bends_starting_point_of_the_normal.Checked ;
            CSV_Bend_Columns[8] = checkBox_CSV_Bends_end_point_of_the_normal.Checked;
            CSV_Bend_Columns[9] = checkBox_CSV_starting_point_of_the_bend.Checked;
            CSV_Bend_Columns[10] = checkBox_CSV_Bends_End_Point_of_the_Bend.Checked;
            #endregion

            #region CSV Triangles Columns
            CSV_Triangle_Columns[0] = checkBox_CSV_Triangles_Pereferial.Checked;
            CSV_Triangle_Columns[1] = checkBox_CSV_Triangles_Fictive.Checked;
            CSV_Triangle_Columns[2] = checkBox_CSV_Triangles_Bends.Checked;
            CSV_Triangle_Columns[3] = checkBox_CSV_Triangles_Nodes.Checked;
            CSV_Triangle_Columns[4] =checkBox_CSV_Triangles_Area.Checked;
            CSV_Triangle_Columns[5] = checkBox_CSV_Triangles_MassCenter.Checked;
            CSV_Triangle_Columns[6] = checkBox_CSV_Triangles_Normal.Checked;
            CSV_Triangle_Columns[7] = checkBox_CSV_Triangles_Angles.Checked;
            CSV_Triangle_Columns[8] = checkBox_CSV_Bends_Length.Checked;

            MachineData_alpha_direction_ = checkBox_MachineData_alpha_direction.Checked;
            #endregion

            this.DialogResult = DialogResult.OK;
        }
        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;           
        }
        private void checkBox_cncTorsion_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = false;
            pictureBox_Torsion.Visible = true;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_cncTorsion_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = true;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_Torsion_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = false;
            pictureBox_Torsion.Visible = true;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;

        }
        private void checkBox_Torsion_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = true;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_CSV_Normal_Coordinates_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = true;
            pictureBox_CSV_NodePosition.Visible = false;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_CSV_Normal_Coordinates_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = true;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_CSV_cncNormal_Coordinates_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = true;
            pictureBox_CSV_NodePosition.Visible = false;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_CSV_cncNormal_Coordinates_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = true;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_Angle_between_Bend_and_Normal_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = false;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = true;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_Angle_between_Bend_and_Normal_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = true;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_Angle_between_Bend_and_cncNormal_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = false;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = true;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_Angle_between_Bend_and_cncNormal_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = true;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_CSV_Angle_Bend_and_Node_Tangetial_Plane_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = false;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = true;
        }
        private void checkBox_CSV_Angle_Bend_and_Node_Tangetial_Plane_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = true;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }
        private void checkBox_CSV_Angle_Bend_and_cncNode_Tangetial_Plane_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = false;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = true;
        }
        private void checkBox_CSV_Angle_Bend_and_cncNode_Tangetial_Plane_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Node_Normal.Visible = false;
            pictureBox_CSV_NodePosition.Visible = true;
            pictureBox_Torsion.Visible = false;
            pictureBox_CSV_Angle_Normal_Bend.Visible = false;
            pictureBox_CSV_TanPlane.Visible = false;
        }

        //-------------------------
        private void checkBox_convex_or_concave_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Convex_Concav.Visible = true;
            pictureBox_CSV_Bends.Visible = false;
            pictureBox_CSV_Bends_Triangles_Normals_Angulars.Visible = false;
        }
        private void checkBox_convex_or_concave_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Convex_Concav.Visible = false;
            pictureBox_CSV_Bends.Visible = true;
            pictureBox_CSV_Bends_Triangles_Normals_Angulars.Visible = false;
        }
        private void checkBox_Angle_between_triangles_Normals_MouseHover(object sender, EventArgs e)
        {
            pictureBox_CSV_Convex_Concav.Visible = false;
            pictureBox_CSV_Bends.Visible = false;
            pictureBox_CSV_Bends_Triangles_Normals_Angulars.Visible = true;
        }
        private void checkBox_Angle_between_triangles_Normals_MouseLeave(object sender, EventArgs e)
        {
            pictureBox_CSV_Convex_Concav.Visible = false;
            pictureBox_CSV_Bends.Visible = true;
            pictureBox_CSV_Bends_Triangles_Normals_Angulars.Visible = false;
        }
        private void checkBox_Single_or_Double_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_Single_or_Double.Checked)
            {
                textBox_Glass_h1.Enabled = false;
                textBox_Glass_h2.Enabled = false;
            }
            else
            {
                textBox_Glass_h1.Enabled = true;
                textBox_Glass_h2.Enabled = true;
            }
        }
        private void Help_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/Settings.htm");                    
        }
        private void Help_General_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/Settings_general.htm");
        }
        private void Help_Normals_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/Normals.htm");
        }
        private void Help_Glass_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/Glass.htm");
        }
        private void Help_Fixing_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/FIXING_SETTINGS.htm");
        }
        private void button_CSV_SETTINGS_TRIANGLES_HELP_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, " http://3dsoft.blob.core.windows.net/kojtocad/html/CSV_FILES_SETTINGS.htm");
        }
        private void button_SETTINGS_CSV_FILES_BENDS_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, " http://3dsoft.blob.core.windows.net/kojtocad/html/CSV_FILES_SETTINGS.htm");
        }
        private void button_Settings_3D_Nozzle_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, " http://3dsoft.blob.core.windows.net/kojtocad/html/3D_SETTINGS.htm");
        }
        private void button_Settings_3D_Bends_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, " http://3dsoft.blob.core.windows.net/kojtocad/html/3D_SETTINGS.htm");
        }
        private void button_Settings_3D_Nodes_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, " http://3dsoft.blob.core.windows.net/kojtocad/html/3D_SETTINGS.htm");
        }
        private void button_Settings_CAM_Part_Data_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/CAM_Settings.htm");
        }
        private void button_Settings_CAM_Machine_Data_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/CAM_Settings.htm");
        }
        private void button_Settings_CAM_Tool_Data_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(this, "http://3dsoft.blob.core.windows.net/kojtocad/html/CAM_Settings.htm");
        }
    }
}
