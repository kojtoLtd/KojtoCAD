using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using MenuItem = Autodesk.AutoCAD.Windows.MenuItem;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Bricscad.Windows;
using MenuItem = Bricscad.Windows.MenuItem;
using Application = Bricscad.ApplicationServices.Application;
#endif

using PRE_BEND = KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<KojtoCAD.KojtoCAD3D.quaternion, KojtoCAD.KojtoCAD3D.quaternion>;
using PRE_BEND_ARRAY = System.Collections.Generic.List<KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<KojtoCAD.KojtoCAD3D.quaternion, KojtoCAD.KojtoCAD3D.quaternion>>;

using PRE_TRIANGLE = KojtoCAD.KojtoCAD3D.UtilityClasses.Triplet<KojtoCAD.KojtoCAD3D.quaternion, KojtoCAD.KojtoCAD3D.quaternion, KojtoCAD.KojtoCAD3D.quaternion>;
using TRIANGLES_ARRAY = System.Collections.Generic.List<KojtoCAD.KojtoCAD3D.WorkClasses.Triangle>;
using POLYGONS_ARRAY = System.Collections.Generic.List<KojtoCAD.KojtoCAD3D.WorkClasses.Polygon>;

using Bend = KojtoCAD.KojtoCAD3D.WorkClasses.Bend;
using Node = KojtoCAD.KojtoCAD3D.WorkClasses.Node;
using Triangle = KojtoCAD.KojtoCAD3D.WorkClasses.Triangle;

namespace KojtoCAD.KojtoCAD3D
{
    namespace UtilityClasses
    {
        public class Pair<T, U>
        {
            public Pair() { }
            public Pair(T first, U second)
            {
                First = first;
                Second = second;
            }

            public T First { get; set; }
            public U Second { get; set; }
        };
        public class Triplet<T, U, V>
        {
            public Triplet() { }
            public Triplet(T first)
            {
                First = first;
            }
            public Triplet(T first, U second)
            {
                First = first;
                Second = second;
            }
            public Triplet(T first, U second, V third)
            {
                First = first;
                Second = second;
                Third = third;
            }

            public T First { get; set; }
            public U Second { get; set; }
            public V Third { get; set; }
        };

        public class KojtoGlassDomesException : System.Exception
        {
            const string eMessage = "KojtoGlassDomes Exception " + "";
            public int numer = 0;
            public KojtoGlassDomesException() : base(eMessage) { }
            public KojtoGlassDomesException(string auxMessage) : base(String.Format("{0} - {1}", eMessage, auxMessage)) { }
            public KojtoGlassDomesException(int Numer, string auxMessage) : base(String.Format("{0} - {1}", eMessage, auxMessage)) { numer = Numer; }
        }

        public class ConstantsAndSettings
        {
            #region members
            // -- temp -- not saved ------
            private static double GasketA_ = 15.0;
            public static double GasketA { get { return (GasketA_); } set { GasketA_ = value; } }

            private static double GasketB_ = 15.0;
            public static double GasketB { get { return (GasketB_); } set { GasketB_ = value; } }

            private static double GasketC_ = 15.0;
            public static double GasketC { get { return (GasketC_); } set { GasketC_ = value; } }
            //----------------------------

            private static double MinDistBhetwenNodes_ = 0.002;
            public static double MinDistBhetwenNodes { get { return (MinDistBhetwenNodes_); } }

            private static double MinBendLength_ = 0.0002;
            public static double MinBendLength { get { return (MinBendLength_); } }

            private static string fictivebendsLayer_ = "Fictive";
            public static string FictivebendsLayer { get { return (fictivebendsLayer_); } set { fictivebendsLayer_ = value; } }

            private static string bendsLayer_ = "000";
            public static string BendsLayer { get { return (bendsLayer_); } set { bendsLayer_ = value; } }

            private static double NormlLengthToShow_ = 200.0;
            public static double NormlLengthToShow { get { return (NormlLengthToShow_); } }

            private static int PerepherialBendsNormalDirection_ = 0;//0 = parallel to Triangle Normal , 
            //1 - in Plane parallel to axe oZ , 2 - in plane parralel to Axe oY, 3- in plane parralel to Axe oX
            public static int PerepherialBendsNormalDirection { get { return (PerepherialBendsNormalDirection_); } }

            private static double Glassheight_ = 30.0;
            public static double Thickness_of_the_Glass { get { return (Glassheight_); } }

            private static double halfGlassFugue_ = 4.0;
            public static double halfGlassFugue { get { return (halfGlassFugue_); } set { halfGlassFugue_ = value; } }

            private static double DoubleGlass_h1_ = 13.52;
            public static double DoubleGlass_h1 { get { return (DoubleGlass_h1_); } set { DoubleGlass_h1_ = value; } }

            private static double DoubleGlass_h2_ = 13.52;
            public static double DoubleGlass_h2 { get { return (DoubleGlass_h2_); } set { DoubleGlass_h2_ = value; } }

            private static int Single_or_Double_Glass_ = 1;
            public static int Single_or_Double_Glass { get { return (Single_or_Double_Glass_); } set { Single_or_Double_Glass_ = value; } }

            private static string projectName = "testProject";
            public static string ProjectName { get { return (projectName); } }

            private static double fixing_A_ = 50.0;
            public static double fixing_A { get { return (fixing_A_); } }

            private static double fixing_B_ = 50.0;
            public static double fixing_B { get { return (fixing_B_); } }

            private static double fixing_pA_ = 50.0;
            public static double fixing_pA { get { return (fixing_pA_); } }//pereferial

            private static double fixing_pB_ = 50.0;
            public static double fixing_pB { get { return (fixing_pB_); } }//pereferial

            private static double PartLength_ = 200.0;
            public static double PartLength { get { return (PartLength_); } set { PartLength_ = value; } }

            private static double PartWidth_ = 200.0;
            public static double PartWidth { get { return (PartWidth_); } set { PartWidth_ = value; } }

            private static double PartHeight_ = 80.0;
            public static double PartHeight { get { return (PartHeight_); } set { PartHeight_ = value; } }

            private static double MachineL_ = 0.0;
            public static double MachineL { get { return (MachineL_); } set { MachineL_ = value; } }

            private static double MachineLp_ = 90.0;
            public static double MachineLp { get { return (MachineLp_); } set { MachineLp_ = value; } }

            private static double MachineLs_ = 75.0;
            public static double MachineLs { get { return (MachineLs_); } set { MachineLs_ = value; } }

            private static double minR_ = 65.0;
            public static double minR { get { return (minR_); } set { minR_ = value; } }

            private static double toolR_ = 10.0;
            public static double toolR { get { return (toolR_); } set { toolR_ = value; } }

            private static string NoPereferialFixngBlockName_ = "BlockFix";
            public static string NoPereferialFixngBlockName { get { return (NoPereferialFixngBlockName_); } set { NoPereferialFixngBlockName_ = value; } }

            private static string PereferialFixngBlockName_ = "BlockFix1";
            public static string PereferialFixngBlockName { get { return (PereferialFixngBlockName_); } set { PereferialFixngBlockName_ = value; } }

            private static string NoPereferialFixngLayerName_ = "LayerFix";
            public static string NoPereferialFixngLayerName { get { return (NoPereferialFixngLayerName_); } set { NoPereferialFixngLayerName_ = value; } }

            private static string PereferialFixngLayerName_ = "LayerFix1";
            public static string PereferialFixngLayerName { get { return (PereferialFixngLayerName_); } set { PereferialFixngLayerName_ = value; } }

            private static string Node3DLayer_ = "La1";
            public static string Node3DLayer { get { return (Node3DLayer_); } set { Node3DLayer_ = value; } }

            private static string Node3DBlock_ = "testBlockNode";
            public static string Node3DBlock { get { return (Node3DBlock_); } set { Node3DBlock_ = value; } }

            private static string Bends3DLayer_ = "La2";
            public static string Bends3DLayer { get { return (Bends3DLayer_); } set { Bends3DLayer_ = value; } }

            private static string Bends3DBlock_ = "testBlockBend";
            public static string Bends3DBlock { get { return (Bends3DBlock_); } set { Bends3DBlock_ = value; } }

            private static string EndsOfBends3DLayer_ = "La2";
            public static string EndsOfBends3DLayer { get { return (EndsOfBends3DLayer_); } set { EndsOfBends3DLayer_ = value; } }

            private static string EndsOfBends3DBlock_ = "testBlockBend";
            public static string EndsOfBends3DBlock { get { return (EndsOfBends3DBlock_); } set { EndsOfBends3DBlock_ = value; } }

            private static double DistanceNodeToNozzle_ = 40.0;
            public static double DistanceNodeToNozzle { get { return (DistanceNodeToNozzle_); } set { DistanceNodeToNozzle_ = value; } }

            private static bool[] CSV_Node_Columns_ = new bool[] { 
                false, /* preferial colulumn */
                false, /* fictive column */
                false, /* Normal coordinates (by all bends) */
                false, /* Normal coordinates (by noFictive bends) */
                false, /* sorted bends by Nymer */
                false, /* sorted triangles by Number */
                false, /* Torsion By all Bends */
                false, /* Torsion By noFictive Bends */
                false, /* Angle between beand and Node Normal (by all bends) */
                false, /* Angle between beand and Node Normal (by noFictive bends) */
                false, /* Angle between beand and Node Plane (by all bends) */
                false, /* Angle between beand and Node Plane (by noFictive bends)*/
                false, /* sorted bends counterclockwise */
                false  /* sorted triangles counterclockwise */
            };
            public static bool[] CSV_Node_Columns { get { return (CSV_Node_Columns_); } set { CSV_Node_Columns_ = value; } }

            private static bool[] CSV_Bend_Columns_ = new bool[] { 
                false, /* pereferial */
                false, /* fictive */
                false, /* convex */
                false, /* nodes */
                false, /* triangles */
                false, /* length */
                false, /* angle between triangles normals */
                false, /* starting point of the bend normal */
                false, /* end point of the bend normal */
                false, /* starting point of the bend */
                false  /* end point of the bend */
            };
            public static bool[] CSV_Bend_Columns { get { return (CSV_Bend_Columns_); } set { CSV_Bend_Columns_ = value; } }

            private static string SQLServer_ = "";
            public static string SQLServer { get { return (SQLServer_); } set { SQLServer_ = value; } }

            private static string SQLDataBaseName_ = "";
            public static string SQLDataBaseName { get { return (SQLDataBaseName_); } set { SQLDataBaseName_ = value; } }

            private static string SQLFileCatalog_ = "";
            public static string SQLFileCatalog { get { return (SQLFileCatalog_); } set { SQLFileCatalog_ = value; } }

            private static bool[] CSV_Triangle_Columns_ = new bool[] { false, false, false, false, false, false, false, false, false };
            public static bool[] CSV_Triangle_Columns { get { return (CSV_Triangle_Columns_); } set { CSV_Triangle_Columns_ = value; } }

            private static bool MachineData_alpha_direction_ = true;
            public static bool MachineData_alpha_direction { get { return (MachineData_alpha_direction_); } set { MachineData_alpha_direction_ = value; } }

            private static double nodeDensity_ = 1.0;
            public static double nodeDensity { get { return (nodeDensity_); } set { nodeDensity_ = value; } }

            private static double bendDensity_ = 1.0;
            public static double bendDensity { get { return (bendDensity_); } set { bendDensity_ = value; } }

            private static double nozzleDensity_ = 1.0;
            public static double nozzleDensity { get { return (nozzleDensity_); } set { nozzleDensity_ = value; } }

            private static double glassDensity_ = 1.0;
            public static double glassDensity { get { return (glassDensity_); } set { glassDensity_ = value; } }

            private static double cutSolidsThicknes_ = 300.0;//KCAD_SET_CUT_SOLID_TH
            public static double cutSolidsThicknes { get { return (cutSolidsThicknes_); } set { cutSolidsThicknes_ = value; } }

            private static double cutSolidsLenK_ = 1.5;// KCAD_SET_CUT_SOLID_LK
            public static double cutSolidsLenK { get { return (cutSolidsLenK_); } set { cutSolidsLenK_ = value; } }

            private static int solidFuncVariant_ = 1; //KCAD_CHANGE_3D_MASHINE
            public static int solidFuncVariant { get { return (solidFuncVariant_); } set { solidFuncVariant_ = value; } }

            private static double cutSolidsExtrudeRatio_ = 15.0;//KCAD_SET_CUT_SOLID_ER
            public static double cutSolidsExtrudeRatio { get { return (cutSolidsExtrudeRatio_); } set { cutSolidsExtrudeRatio_ = value; } }

            private static int solidTrimPiksVariant_ = 0; 
            public static int solidTrimPiksVariant { get { return (solidTrimPiksVariant_); } set { solidTrimPiksVariant_ = value; } }

            #endregion

            public static void SetProjectName(string name) { projectName = name; }
            public static double GetMinDistBhetwenNodes() { return MinDistBhetwenNodes_; }
            public static double GetMinBendLength() { return MinBendLength_; }
            public static string GetFictivebendsLayer() { return fictivebendsLayer_; }
            public static string GetBendsLayer() { return bendsLayer_; }
            public static void SetNormalLength(double L) { NormlLengthToShow_ = L; }
            public static void SetGlassThickness(double L) { Glassheight_ = L; }
            public static double GetNormalLength() { return NormlLengthToShow_; }
            public static int GetPerepherialBendsNormalDirection() { return PerepherialBendsNormalDirection_; }
            public static void SetPerepherialBendsNormalDirection(int d) { PerepherialBendsNormalDirection_ = d; }
            public static void SetFixing_A(double a) { fixing_A_ = a; }
            public static void SetFixing_B(double b) { fixing_B_ = b; }
            public static void SetFixing_pereferial_A(double a) { fixing_pA_ = a; }
            public static void SetFixing_pereferial_B(double b) { fixing_pB_ = b; }

            public static Xrecord GetXrecord()
            {
                Xrecord xRec = new Xrecord();
                // new TypedValue((int)DxfCode.Text, "KOJTO_GLASS_DOMESS_CONSTANTS_AND_SETTINGS")
                xRec.Data = new ResultBuffer(
                     new TypedValue((int)DxfCode.Real, MinDistBhetwenNodes_),//0
                     new TypedValue((int)DxfCode.Real, MinBendLength_),      //1
                     new TypedValue((int)DxfCode.Text, fictivebendsLayer_),  //2
                     new TypedValue((int)DxfCode.Text, bendsLayer_),         //3
                     new TypedValue((int)DxfCode.Real, NormlLengthToShow_),  //4
                     new TypedValue((int)DxfCode.Int32, PerepherialBendsNormalDirection_), //5
                     new TypedValue((int)DxfCode.Real, Glassheight_),  //6
                     new TypedValue((int)DxfCode.Text, projectName),   //7
                     new TypedValue((int)DxfCode.Real, fixing_A_),     //8
                     new TypedValue((int)DxfCode.Real, fixing_B_),     //9
                     new TypedValue((int)DxfCode.Real, fixing_pA_),    //10
                     new TypedValue((int)DxfCode.Real, fixing_pB_),    //11
                     new TypedValue((int)DxfCode.Real, PartLength_),   //12
                     new TypedValue((int)DxfCode.Real, PartWidth_),    //13
                     new TypedValue((int)DxfCode.Real, PartHeight_),    //14
                     new TypedValue((int)DxfCode.Real, MachineL_),     //15
                     new TypedValue((int)DxfCode.Real, MachineLp_),     //16
                     new TypedValue((int)DxfCode.Real, MachineLs_),      //17
                     new TypedValue((int)DxfCode.Real, minR_),          //18
                     new TypedValue((int)DxfCode.Real, toolR_),          //19
                     new TypedValue((int)DxfCode.Text, NoPereferialFixngBlockName_),//20
                     new TypedValue((int)DxfCode.Text, PereferialFixngBlockName_),//21
                     new TypedValue((int)DxfCode.Text, NoPereferialFixngLayerName_),//22
                     new TypedValue((int)DxfCode.Text, PereferialFixngLayerName_),//23
                     new TypedValue((int)DxfCode.Text, Node3DLayer_), //24
                     new TypedValue((int)DxfCode.Text, Node3DBlock_), //25
                     new TypedValue((int)DxfCode.Text, Bends3DLayer_), //26
                     new TypedValue((int)DxfCode.Text, Bends3DBlock_), //27
                     new TypedValue((int)DxfCode.Text, EndsOfBends3DLayer_),  //28
                     new TypedValue((int)DxfCode.Text, EndsOfBends3DBlock_),  //29
                     new TypedValue((int)DxfCode.Real, DistanceNodeToNozzle_), //30
                    //-------------------------------------------------------------
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[0] ? 1 : 0), //31
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[1] ? 1 : 0), //32
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[2] ? 1 : 0), //33
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[3] ? 1 : 0), //34
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[4] ? 1 : 0), //35
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[5] ? 1 : 0), //36
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[6] ? 1 : 0), //37
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[7] ? 1 : 0), //38
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[8] ? 1 : 0), //39
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[9] ? 1 : 0), //40
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[10] ? 1 : 0), //41
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[11] ? 1 : 0), //42
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[12] ? 1 : 0), //43
                     new TypedValue((int)DxfCode.Int32, CSV_Node_Columns_[13] ? 1 : 0), //44
                    //-----------------------------------------------------------
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[0] ? 1 : 0), //45
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[1] ? 1 : 0), //46
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[2] ? 1 : 0), //47
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[3] ? 1 : 0), //48
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[4] ? 1 : 0), //49
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[5] ? 1 : 0), //50
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[6] ? 1 : 0), //51
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[7] ? 1 : 0), //52
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[8] ? 1 : 0), //53
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[9] ? 1 : 0), //54
                     new TypedValue((int)DxfCode.Int32, CSV_Bend_Columns_[10] ? 1 : 0), //55
                    //-------------------------------------------------------------------
                     new TypedValue((int)DxfCode.Int32, CSV_Triangle_Columns_[0] ? 1 : 0), //56
                     new TypedValue((int)DxfCode.Int32, CSV_Triangle_Columns_[1] ? 1 : 0), //57
                     new TypedValue((int)DxfCode.Int32, CSV_Triangle_Columns_[2] ? 1 : 0), //58
                     new TypedValue((int)DxfCode.Int32, CSV_Triangle_Columns_[3] ? 1 : 0), //59
                     new TypedValue((int)DxfCode.Int32, CSV_Triangle_Columns_[4] ? 1 : 0), //60
                     new TypedValue((int)DxfCode.Int32, CSV_Triangle_Columns_[5] ? 1 : 0), //61
                     new TypedValue((int)DxfCode.Int32, CSV_Triangle_Columns_[6] ? 1 : 0), //62
                     new TypedValue((int)DxfCode.Int32, CSV_Triangle_Columns_[7] ? 1 : 0), //63
                     new TypedValue((int)DxfCode.Int32, CSV_Triangle_Columns_[8] ? 1 : 0), //64
                    //---------------------------------------------------------------------------
                     new TypedValue((int)DxfCode.Int32, MachineData_alpha_direction ? 1 : 0), //65
                    //---------------------------------------------------------------------------
                     new TypedValue((int)DxfCode.Int32, Single_or_Double_Glass_),   //66                     
                     new TypedValue((int)DxfCode.Real, halfGlassFugue_),            //67
                     new TypedValue((int)DxfCode.Real, DoubleGlass_h1_),            //68
                     new TypedValue((int)DxfCode.Real, DoubleGlass_h2_),            //69
                     new TypedValue((int)DxfCode.Real, nodeDensity_),               //70
                     new TypedValue((int)DxfCode.Real, bendDensity_),               //71
                     new TypedValue((int)DxfCode.Real, nozzleDensity_),             //72
                     new TypedValue((int)DxfCode.Real, glassDensity_),               //73
                     new TypedValue((int)DxfCode.Real, cutSolidsThicknes_),          //74
                     new TypedValue((int)DxfCode.Real, cutSolidsLenK_),              //75
                     new TypedValue((int)DxfCode.Int32, solidFuncVariant_),           //76
                     new TypedValue((int)DxfCode.Real, cutSolidsExtrudeRatio_),       //77
                     new TypedValue((int)DxfCode.Int32, solidTrimPiksVariant_)       //78
                    );

                return xRec;
            }
            public static void Set(Xrecord xrec)
            {
                TypedValue[] res = xrec.Data.AsArray();
                MinDistBhetwenNodes_ = (double)res[0].Value;
                MinBendLength_ = (double)res[1].Value;
                fictivebendsLayer_ = (string)res[2].Value;
                bendsLayer_ = (string)res[3].Value;
                NormlLengthToShow_ = (double)res[4].Value;
                PerepherialBendsNormalDirection_ = (int)res[5].Value;
                Glassheight_ = (double)res[6].Value;
                projectName = (string)res[7].Value;
                fixing_A_ = (double)res[8].Value;
                fixing_B_ = (double)res[9].Value;
                fixing_pA_ = (double)res[10].Value;
                fixing_pB_ = (double)res[11].Value;
                PartLength_ = (double)res[12].Value;
                PartWidth_ = (double)res[13].Value;
                PartHeight_ = (double)res[14].Value;
                MachineL_ = (double)res[15].Value;
                MachineLp_ = (double)res[16].Value;
                MachineLs_ = (double)res[17].Value;
                minR_ = (double)res[18].Value;
                toolR_ = (double)res[19].Value;
                NoPereferialFixngBlockName_ = (string)res[20].Value;
                PereferialFixngBlockName_ = (string)res[21].Value;
                NoPereferialFixngLayerName_ = (string)res[22].Value;
                PereferialFixngLayerName_ = (string)res[23].Value;
                Node3DLayer_ = (string)res[24].Value;
                Node3DBlock_ = (string)res[25].Value;
                Bends3DLayer_ = (string)res[26].Value;
                Bends3DBlock_ = (string)res[27].Value;
                EndsOfBends3DLayer_ = (string)res[28].Value;
                EndsOfBends3DBlock_ = (string)res[29].Value;
                DistanceNodeToNozzle_ = (double)res[30].Value;
                //--------------------------------------------
                CSV_Node_Columns_[0] = ((int)res[31].Value == 1) ? true : false;
                CSV_Node_Columns_[1] = ((int)res[32].Value == 1) ? true : false;
                CSV_Node_Columns_[2] = ((int)res[33].Value == 1) ? true : false;
                CSV_Node_Columns_[3] = ((int)res[34].Value == 1) ? true : false;
                CSV_Node_Columns_[4] = ((int)res[35].Value == 1) ? true : false;
                CSV_Node_Columns_[5] = ((int)res[36].Value == 1) ? true : false;
                CSV_Node_Columns_[6] = ((int)res[37].Value == 1) ? true : false;
                CSV_Node_Columns_[7] = ((int)res[38].Value == 1) ? true : false;
                CSV_Node_Columns_[8] = ((int)res[39].Value == 1) ? true : false;
                CSV_Node_Columns_[9] = ((int)res[40].Value == 1) ? true : false;
                CSV_Node_Columns_[10] = ((int)res[41].Value == 1) ? true : false;
                CSV_Node_Columns_[11] = ((int)res[42].Value == 1) ? true : false;
                CSV_Node_Columns_[12] = ((int)res[43].Value == 1) ? true : false;
                CSV_Node_Columns_[13] = ((int)res[44].Value == 1) ? true : false;
                //----------------------------------------------
                CSV_Bend_Columns_[0] = ((int)res[45].Value == 1) ? true : false;
                CSV_Bend_Columns_[1] = ((int)res[46].Value == 1) ? true : false;
                CSV_Bend_Columns_[2] = ((int)res[47].Value == 1) ? true : false;
                CSV_Bend_Columns_[3] = ((int)res[48].Value == 1) ? true : false;
                CSV_Bend_Columns_[4] = ((int)res[49].Value == 1) ? true : false;
                CSV_Bend_Columns_[5] = ((int)res[50].Value == 1) ? true : false;
                CSV_Bend_Columns_[6] = ((int)res[51].Value == 1) ? true : false;
                CSV_Bend_Columns_[7] = ((int)res[52].Value == 1) ? true : false;
                CSV_Bend_Columns_[8] = ((int)res[53].Value == 1) ? true : false;
                CSV_Bend_Columns_[9] = ((int)res[54].Value == 1) ? true : false;
                CSV_Bend_Columns_[10] = ((int)res[55].Value == 1) ? true : false;
                //---------------------------------------------------------------
                CSV_Triangle_Columns_[0] = ((int)res[56].Value == 1) ? true : false;
                CSV_Triangle_Columns_[1] = ((int)res[57].Value == 1) ? true : false;
                CSV_Triangle_Columns_[2] = ((int)res[58].Value == 1) ? true : false;
                CSV_Triangle_Columns_[3] = ((int)res[59].Value == 1) ? true : false;
                CSV_Triangle_Columns_[4] = ((int)res[60].Value == 1) ? true : false;
                CSV_Triangle_Columns_[5] = ((int)res[61].Value == 1) ? true : false;
                CSV_Triangle_Columns_[6] = ((int)res[62].Value == 1) ? true : false;
                CSV_Triangle_Columns_[7] = ((int)res[63].Value == 1) ? true : false;
                CSV_Triangle_Columns_[8] = ((int)res[64].Value == 1) ? true : false;
                //-------------------------------------------------------------------
                MachineData_alpha_direction_ = ((int)res[65].Value == 1) ? true : false;
                //--------------------------------------------------------------------
                Single_or_Double_Glass_ = (int)res[66].Value;
                halfGlassFugue_ = (double)res[67].Value;
                DoubleGlass_h1_ = (double)res[68].Value;
                DoubleGlass_h2_ = (double)res[69].Value;
                nodeDensity_ = (double)res[70].Value;
                bendDensity_ = (double)res[71].Value;
                nozzleDensity_ = (double)res[72].Value;
                glassDensity_ = (double)res[73].Value;
                cutSolidsThicknes_ = (double)res[74].Value;
                cutSolidsLenK_ = (double)res[75].Value;
                solidFuncVariant_ = (int)res[76].Value;
                cutSolidsExtrudeRatio_ = (double)res[77].Value;
                solidTrimPiksVariant_ = (int)res[78].Value;
            }
            public static string GetString()
            {
                string rez = "\nConstants and Settings\n-----------------------------\n\nPrject: " + projectName + "\n";
                rez += string.Format("MinDistBhetwenNodes = {0}\nMinBendLength = {1}\n", MinDistBhetwenNodes_, MinBendLength_);
                rez += string.Format("Fictive Bends Layer = {0}\nBends Layer = {1}\nNormal Length = {2}\n", fictivebendsLayer_, bendsLayer_, NormlLengthToShow_);
                rez += string.Format("Perepherial Bends Normal Direction: {0}\n", (PerepherialBendsNormalDirection_ == 0) ? "By Triangle" : "By Vertical Plane");
                rez += string.Format("Thickness of the Glass: {0}\n", Glassheight_);
                return rez;
            }

            public static void ToStream(StreamWriter sw)
            {
                sw.WriteLine(MinDistBhetwenNodes_);//0
                sw.WriteLine(MinBendLength_);//1
                sw.WriteLine(fictivebendsLayer_);//2
                sw.WriteLine(bendsLayer_);//3
                sw.WriteLine(NormlLengthToShow_);//4
                sw.WriteLine(PerepherialBendsNormalDirection_);//5
                sw.WriteLine(Glassheight_);//6
                sw.WriteLine(projectName);//7
                sw.WriteLine(fixing_A_);//8
                sw.WriteLine(fixing_B_);//9
                sw.WriteLine(fixing_pA_);//10
                sw.WriteLine(fixing_pB_);//11
                sw.WriteLine(PartLength_);//12
                sw.WriteLine(PartWidth_);//13
                sw.WriteLine(PartHeight_);//14
                sw.WriteLine(MachineL_);//15
                sw.WriteLine(MachineLp_);//16
                sw.WriteLine(MachineLs_);//17
                sw.WriteLine(minR_);//18
                sw.WriteLine(toolR_);//19
                sw.WriteLine(NoPereferialFixngBlockName_);//20
                sw.WriteLine(PereferialFixngBlockName_);//21
                sw.WriteLine(NoPereferialFixngLayerName_);//22
                sw.WriteLine(PereferialFixngLayerName_);//23
                sw.WriteLine(Node3DLayer_);//24
                sw.WriteLine(Node3DBlock_);//25
                sw.WriteLine(Bends3DLayer_);//26
                sw.WriteLine(Bends3DBlock_);//27
                sw.WriteLine(EndsOfBends3DLayer_);//28
                sw.WriteLine(EndsOfBends3DBlock_);//29
                sw.WriteLine(DistanceNodeToNozzle_);//30
                //-------------------------------------------
                sw.WriteLine(CSV_Node_Columns_[0] ? 1 : 0);//31
                sw.WriteLine(CSV_Node_Columns_[1] ? 1 : 0);//32
                sw.WriteLine(CSV_Node_Columns_[2] ? 1 : 0);//33
                sw.WriteLine(CSV_Node_Columns_[3] ? 1 : 0);//34
                sw.WriteLine(CSV_Node_Columns_[4] ? 1 : 0);//35
                sw.WriteLine(CSV_Node_Columns_[5] ? 1 : 0);//36
                sw.WriteLine(CSV_Node_Columns_[6] ? 1 : 0);//37
                sw.WriteLine(CSV_Node_Columns_[7] ? 1 : 0);//38
                sw.WriteLine(CSV_Node_Columns_[8] ? 1 : 0);//39
                sw.WriteLine(CSV_Node_Columns_[9] ? 1 : 0);//40
                sw.WriteLine(CSV_Node_Columns_[10] ? 1 : 0);//41
                sw.WriteLine(CSV_Node_Columns_[11] ? 1 : 0);//42
                sw.WriteLine(CSV_Node_Columns_[12] ? 1 : 0);//43
                sw.WriteLine(CSV_Node_Columns_[10] ? 1 : 0);//44
                //----------------------------------------------
                sw.WriteLine(CSV_Bend_Columns_[0] ? 1 : 0);//45
                sw.WriteLine(CSV_Bend_Columns_[1] ? 1 : 0);//46
                sw.WriteLine(CSV_Bend_Columns_[2] ? 1 : 0);//47
                sw.WriteLine(CSV_Bend_Columns_[3] ? 1 : 0);//48
                sw.WriteLine(CSV_Bend_Columns_[4] ? 1 : 0);//49
                sw.WriteLine(CSV_Bend_Columns_[5] ? 1 : 0);//50
                sw.WriteLine(CSV_Bend_Columns_[6] ? 1 : 0);//51
                sw.WriteLine(CSV_Bend_Columns_[7] ? 1 : 0);//52
                sw.WriteLine(CSV_Bend_Columns_[8] ? 1 : 0);//53
                sw.WriteLine(CSV_Bend_Columns_[9] ? 1 : 0);//54
                sw.WriteLine(CSV_Bend_Columns_[10] ? 1 : 0);//55
                //-----------------------------------------------
                sw.WriteLine(CSV_Triangle_Columns_[0] ? 1 : 0);//56
                sw.WriteLine(CSV_Triangle_Columns_[1] ? 1 : 0);//57
                sw.WriteLine(CSV_Triangle_Columns_[2] ? 1 : 0);//58
                sw.WriteLine(CSV_Triangle_Columns_[3] ? 1 : 0);//59
                sw.WriteLine(CSV_Triangle_Columns_[4] ? 1 : 0);//60
                sw.WriteLine(CSV_Triangle_Columns_[5] ? 1 : 0);//61
                sw.WriteLine(CSV_Triangle_Columns_[6] ? 1 : 0);//62
                sw.WriteLine(CSV_Triangle_Columns_[7] ? 1 : 0);//63
                sw.WriteLine(CSV_Triangle_Columns_[8] ? 1 : 0);//64
                //---------------------------------------------------
                sw.WriteLine(MachineData_alpha_direction ? 1 : 0);//65
                //--------------------------------------------------
                sw.WriteLine(Single_or_Double_Glass_);//66
                sw.WriteLine(halfGlassFugue_);//67
                sw.WriteLine(DoubleGlass_h1_);//68
                sw.WriteLine(DoubleGlass_h2_);//69
                sw.WriteLine(nodeDensity_);//70
                sw.WriteLine(bendDensity_);//71
                sw.WriteLine(nozzleDensity_);//72
                sw.WriteLine(glassDensity_);//73
                sw.WriteLine(cutSolidsThicknes_);//74
                sw.WriteLine(cutSolidsLenK_);//75
                sw.WriteLine(solidFuncVariant_);//76
                sw.WriteLine(cutSolidsExtrudeRatio_);//77
                sw.WriteLine(solidTrimPiksVariant_);//78
            }
            public static void Set(StreamReader sr)
            {
                List<string> list = new List<string>();
                for (int i = 0; i < 79; i++)
                    list.Add(sr.ReadLine());

                MinDistBhetwenNodes_ = double.Parse(list[0]);
                MinBendLength_ = double.Parse(list[1]);
                fictivebendsLayer_ = list[2];
                bendsLayer_ = list[3];
                NormlLengthToShow_ = double.Parse(list[4]);
                PerepherialBendsNormalDirection_ = int.Parse(list[5]);
                Glassheight_ = double.Parse(list[6]);
                projectName = list[7];
                fixing_A_ = double.Parse(list[8]);
                fixing_B_ = double.Parse(list[9]);
                fixing_pA_ = double.Parse(list[10]);
                fixing_pB_ = double.Parse(list[11]);
                PartLength_ = double.Parse(list[12]);
                PartWidth_ = double.Parse(list[13]);
                PartHeight_ = double.Parse(list[14]);
                MachineL_ = double.Parse(list[15]);
                MachineLp_ = double.Parse(list[16]);
                MachineLs_ = double.Parse(list[17]);
                minR_ = double.Parse(list[18]);
                toolR_ = double.Parse(list[19]);
                NoPereferialFixngBlockName_ = list[20];
                PereferialFixngBlockName_ = list[21];
                NoPereferialFixngLayerName_ = list[22];
                PereferialFixngLayerName_ = list[23];
                Node3DLayer_ = list[24];
                Node3DBlock_ = list[25];
                Bends3DLayer_ = list[26];
                Bends3DBlock_ = list[27];
                EndsOfBends3DLayer_ = list[28];
                EndsOfBends3DBlock_ = list[29];
                DistanceNodeToNozzle_ = double.Parse(list[30]);
                //------------------------------------------------
                CSV_Node_Columns_[0] = (int.Parse(list[31]) == 1) ? true : false;
                CSV_Node_Columns_[1] = (int.Parse(list[32]) == 1) ? true : false;
                CSV_Node_Columns_[2] = (int.Parse(list[33]) == 1) ? true : false;
                CSV_Node_Columns_[3] = (int.Parse(list[34]) == 1) ? true : false;
                CSV_Node_Columns_[4] = (int.Parse(list[35]) == 1) ? true : false;
                CSV_Node_Columns_[5] = (int.Parse(list[36]) == 1) ? true : false;
                CSV_Node_Columns_[6] = (int.Parse(list[37]) == 1) ? true : false;
                CSV_Node_Columns_[7] = (int.Parse(list[38]) == 1) ? true : false;
                CSV_Node_Columns_[8] = (int.Parse(list[39]) == 1) ? true : false;
                CSV_Node_Columns_[9] = (int.Parse(list[40]) == 1) ? true : false;
                CSV_Node_Columns_[10] = (int.Parse(list[41]) == 1) ? true : false;
                CSV_Node_Columns_[11] = (int.Parse(list[42]) == 1) ? true : false;
                CSV_Node_Columns_[12] = (int.Parse(list[43]) == 1) ? true : false;
                CSV_Node_Columns_[13] = (int.Parse(list[44]) == 1) ? true : false;
                //------------------------------------------------------------------
                CSV_Bend_Columns_[0] = (int.Parse(list[45]) == 1) ? true : false;
                CSV_Bend_Columns_[1] = (int.Parse(list[46]) == 1) ? true : false;
                CSV_Bend_Columns_[2] = (int.Parse(list[47]) == 1) ? true : false;
                CSV_Bend_Columns_[3] = (int.Parse(list[48]) == 1) ? true : false;
                CSV_Bend_Columns_[4] = (int.Parse(list[49]) == 1) ? true : false;
                CSV_Bend_Columns_[5] = (int.Parse(list[50]) == 1) ? true : false;
                CSV_Bend_Columns_[6] = (int.Parse(list[51]) == 1) ? true : false;
                CSV_Bend_Columns_[7] = (int.Parse(list[52]) == 1) ? true : false;
                CSV_Bend_Columns_[8] = (int.Parse(list[53]) == 1) ? true : false;
                CSV_Bend_Columns_[9] = (int.Parse(list[54]) == 1) ? true : false;
                CSV_Bend_Columns_[10] = (int.Parse(list[55]) == 1) ? true : false;
                //------------------------------------------------------------------
                CSV_Triangle_Columns_[0] = (int.Parse(list[56]) == 1) ? true : false;
                CSV_Triangle_Columns_[1] = (int.Parse(list[57]) == 1) ? true : false;
                CSV_Triangle_Columns_[2] = (int.Parse(list[58]) == 1) ? true : false;
                CSV_Triangle_Columns_[3] = (int.Parse(list[59]) == 1) ? true : false;
                CSV_Triangle_Columns_[4] = (int.Parse(list[60]) == 1) ? true : false;
                CSV_Triangle_Columns_[5] = (int.Parse(list[61]) == 1) ? true : false;
                CSV_Triangle_Columns_[6] = (int.Parse(list[62]) == 1) ? true : false;
                CSV_Triangle_Columns_[7] = (int.Parse(list[63]) == 1) ? true : false;
                CSV_Triangle_Columns_[8] = (int.Parse(list[64]) == 1) ? true : false;
                //-------------------------------------------------------------------
                MachineData_alpha_direction_ = (int.Parse(list[65]) == 1) ? true : false;
                //------------------------------------------------------------------------
                Single_or_Double_Glass_ = int.Parse(list[66]);
                halfGlassFugue_ = double.Parse(list[67]);
                DoubleGlass_h1_ = double.Parse(list[68]);
                DoubleGlass_h2_ = double.Parse(list[69]);
                nodeDensity_ = double.Parse(list[70]);
                bendDensity_ = double.Parse(list[71]);
                nozzleDensity_ = double.Parse(list[72]);
                glassDensity_ = double.Parse(list[73]);
                cutSolidsThicknes_ = double.Parse(list[74]);
                cutSolidsLenK_ = double.Parse(list[75]);
                solidFuncVariant_ = int.Parse(list[76]);
                cutSolidsExtrudeRatio_ = double.Parse(list[77]);
                solidTrimPiksVariant_ = int.Parse(list[78]);
            }

        }
        public class GlobalFunctions
        {
            //ads_queueexpr("(command""_POINT"" ""4,4,0"")")
            [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
            extern static public int ads_queueexpr(string strExpr);

            //acedPostCommand("_POINT 3,3,0 ")
            [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acedPostCommand@@YAHPB_W@Z")]
            extern static public int acedPostCommand(string strExpr);

            public static void OpenHelpHTML(string adr)
            {
                Help.ShowHelp(new Control(null, ""), adr);
            }

            public static bool PromptYesOrNo(string mess)
            {
                bool rez = false;

                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\n" + mess + " ";
                pKeyOpts.Keywords.Add("Yes");
                pKeyOpts.Keywords.Add("No");
                pKeyOpts.Keywords.Default = "No";
                pKeyOpts.AllowNone = false;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "Yes": rez = true; break;
                        case "No": rez = false; break;
                    }
                }

                return rez;
            }

            public static ObjectId GetObjectId(TypedValue tv)
            {
                long ln = Convert.ToInt64((string)tv.Value, 16);
                Handle hn = new Handle(ln);
                return HostApplicationServices.WorkingDatabase.GetObjectId(false, hn, 0);
            }
            public static ObjectId GetObjectId(long ln)
            {
                Handle hn = new Handle(ln);
                return HostApplicationServices.WorkingDatabase.GetObjectId(false, hn, 0);
            }
            public static ObjectId GetObjectId(Handle hn)
            {
                return HostApplicationServices.WorkingDatabase.GetObjectId(false, hn, 0);
            }

            public static List<Entity> GetSelection(ref TypedValue[] acTypValAr, string mess)//selects and makes invisible selected items
            {
                List<Entity> list = new List<Entity>();
                SelectionFilter SelFilter = new SelectionFilter(acTypValAr);
                PromptSelectionOptions sOptions = new PromptSelectionOptions();
                sOptions.MessageForAdding = mess; //"\r\nSelect first 2d line";
                sOptions.AllowDuplicates = false;
                sOptions.SingleOnly = true;

                PromptSelectionResult acSSPrompt = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection(sOptions, SelFilter);
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    using (Transaction acTrans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        foreach (SelectedObject acSSObj in acSSet)
                        {
                            if (acSSObj != null)
                            {
                                Entity acEnt = acTrans.GetObject(acSSObj.ObjectId, OpenMode.ForWrite) as Entity;
                                if (acEnt != null)
                                {
                                    list.Add(acEnt);
                                    acEnt.Visible = false;
                                }
                            }
                        }

                        acTrans.Commit();
                        Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
                    }
                }
                else
                {
                    list = null;
                }

                return list;
            }
            //Example:
            //     List<Entity> LIST = MyUtilityClasses.GetSelection(ref acTypValAr, "\r\nSelct bends");
            public static Pair<List<Entity>, List<Entity>> GetSelectionOn(string layer, string fictivelayer)//select from editor bends; first - bends, second - fictive bends
            {
                List<List<Entity>> list = new List<List<Entity>>();

                List<Entity> list1 = new List<Entity>();
                List<Entity> list2 = new List<Entity>();

                TypedValue[] acTypValAr = new TypedValue[5];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Operator, "<or"), 0);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE"), 1);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "LWPOLYLINE"), 2);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "LINE"), 3);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Operator, "or>"), 4);

                do
                {
                    List<Entity> LIST = GetSelection(ref acTypValAr, "\r\nSelct bends");
                    if ((LIST != null) && (LIST.Count > 0))
                    {
                        list.Add(LIST);
                    }
                } while (MessageBox.Show("Next selection of objects?", "New Selection", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);

                using (Transaction acTrans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    foreach (List<Entity> LIST in list)
                    {
                        foreach (Entity ent in LIST)
                        {
                            Entity acEnt = acTrans.GetObject(ent.ObjectId, OpenMode.ForWrite) as Entity;
                            acEnt.Visible = true;
                            if (layer == ent.Layer)
                            {
                                list1.Add(ent);
                            }
                            else
                            {
                                if (fictivelayer == ent.Layer)
                                {
                                    list2.Add(ent);
                                }
                            }
                        }
                    }

                    acTrans.Commit();
                    Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
                }

                if (list1.Count() < 1) { list1 = null; }
                if (list2.Count() < 1) { list2 = null; }


                return new Pair<List<Entity>, List<Entity>>(list1, list2);
            }

            public static PRE_BEND_ARRAY GetPreBends(List<Entity> SelectedEntites)//итерира през избраните обекти и ги превръща в масив от тип  PRE_BEND
            {
                PRE_BEND_ARRAY PreBends = new PRE_BEND_ARRAY();
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    foreach (Entity ent in SelectedEntites)
                    {
                        Type type = ent.GetType();
                        if (type.Name != "Line")
                        {
                            Polyline pl = tr.GetObject(ent.ObjectId, OpenMode.ForRead) as Polyline;
                            if (pl != null)
                            {
                                int vn = pl.NumberOfVertices;
                                Point3d old = pl.GetPoint3dAt(0);
                                for (int i = 1; i < vn; i++)
                                {
                                    Point3d pt = pl.GetPoint3dAt(i);
                                    quaternion start = new quaternion(0.0, old.X, old.Y, old.Z);
                                    quaternion end = new quaternion(0.0, pt.X, pt.Y, pt.Z);
                                    PRE_BEND prb = new PRE_BEND(start, end);
                                    PreBends.Add(prb);
                                    old = pt;
                                }
                                //ако полилинията е затворена и последната точка не съвпада с първата трябва да добавим затварящата линия
                                if ((pl.Closed == true) && (pl.StartPoint.DistanceTo(old) > ConstantsAndSettings.GetMinDistBhetwenNodes()))
                                {
                                    quaternion start = new quaternion(0.0, old.X, old.Y, old.Z);
                                    quaternion end = new quaternion(0.0, pl.StartPoint.X, pl.StartPoint.Y, pl.StartPoint.Z);
                                    PRE_BEND prb = new PRE_BEND(start, end);
                                    PreBends.Add(prb);
                                }
                            }
                            else // 3d poly
                            {
                                Polyline3d p3d = tr.GetObject(ent.ObjectId, OpenMode.ForRead) as Polyline3d;
                                if (p3d != null)
                                {
                                    PolylineVertex3d old = null;
                                    //PolylineVertex3d START = null;
                                    foreach (ObjectId vId in p3d)
                                    {
                                        PolylineVertex3d v3d = (PolylineVertex3d)tr.GetObject(vId, OpenMode.ForRead);
                                        if (old != null)
                                        {
                                            quaternion start = new quaternion(0.0, old.Position.X, old.Position.Y, old.Position.Z);
                                            quaternion end = new quaternion(0.0, v3d.Position.X, v3d.Position.Y, v3d.Position.Z);
                                            PRE_BEND prb = new PRE_BEND(start, end);
                                            PreBends.Add(prb);
                                        }
                                        old = v3d;
                                        // if (START == null) { START = old; }
                                    }
                                    //ако полилинията е затворена и последната точка не съвпада с първата трябва да добавим затварящата линия
                                    if ((p3d.Closed == true) && (p3d.StartPoint.DistanceTo(old.Position) > ConstantsAndSettings.GetMinDistBhetwenNodes()))
                                    {
                                        quaternion start = new quaternion(0.0, old.Position.X, old.Position.Y, old.Position.Z);
                                        quaternion end = new quaternion(0.0, p3d.StartPoint.X, p3d.StartPoint.Y, p3d.StartPoint.Z);
                                        PRE_BEND prb = new PRE_BEND(start, end);
                                        PreBends.Add(prb);
                                    }
                                }
                            }
                        }
                        else// Line
                        {
                            Line acLine = tr.GetObject(ent.ObjectId, OpenMode.ForRead) as Line;
                            quaternion start = new quaternion(0.0, acLine.StartPoint.X, acLine.StartPoint.Y, acLine.StartPoint.Z);
                            quaternion end = new quaternion(0.0, acLine.EndPoint.X, acLine.EndPoint.Y, acLine.EndPoint.Z);
                            PRE_BEND prb = new PRE_BEND(start, end);
                            PreBends.Add(prb);
                        }

                    }
                }
                return PreBends;
            }
            //Example:
            //     MyUtilityClasses.Pair<List<Entity>, List<Entity>> list = MyUtilityClasses.Functions.GetSelectionOn("Layer0", "layer1");
            //     PRE_BEND_ARRAY arr1 = MyUtilityClasses.Functions.GetPreBends(list.First);
            //     PRE_BEND_ARRAY arr2 = MyUtilityClasses.Functions.GetPreBends(list.Second);

            public static bool IsEqualTriangle(PRE_TRIANGLE tr1, PRE_TRIANGLE tr2)
            {
                bool rez = false;

                // bool b0 = (tr1.Third - tr1.Second).abs() + (tr1.Second - tr1.First).abs() + (tr1.First - tr1.Third).abs()
                //           - (tr2.Third - tr2.Second).abs() - (tr2.Second - tr2.First).abs() - (tr2.First - tr2.Third).abs() > ConstantsAndSettings.GetMinDistBhetwenNodes();
                // if (!b0)
                {

                    bool b1 = (tr1.First - tr2.First).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes() ||
                               (tr1.First - tr2.Second).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes() ||
                               (tr1.First - tr2.Third).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes();

                    bool b2 = (tr1.Second - tr2.First).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes() ||
                              (tr1.Second - tr2.Second).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes() ||
                              (tr1.Second - tr2.Third).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes();

                    bool b3 = (tr1.Third - tr2.First).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes() ||
                              (tr1.Third - tr2.Second).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes() ||
                              (tr1.Third - tr2.Third).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes();

                    bool b4 = (tr1.First - tr1.Second).abs() >= ConstantsAndSettings.GetMinDistBhetwenNodes() &&
                              (tr1.First - tr1.Third).abs() >= ConstantsAndSettings.GetMinDistBhetwenNodes() &&
                              (tr1.Second - tr1.Third).abs() >= ConstantsAndSettings.GetMinDistBhetwenNodes();

                    rez = b1 && b2 && b3 && b4;
                }
                return rez;
            }
            public static bool IsEqualPreBend(PRE_BEND pb1, PRE_BEND pb2)//true-if the two sections are limited to the same points
            {
                bool rez = false;
                bool b0 = Math.Abs((pb1.Second - pb1.First).abs() - (pb2.Second - pb2.First).abs()) > ConstantsAndSettings.GetMinDistBhetwenNodes();
                if (!b0)
                {
                    bool b1 = (pb1.First - pb2.First).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes() || (pb1.First - pb2.Second).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes();
                    bool b2 = (pb1.Second - pb2.First).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes() || (pb1.Second - pb2.Second).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes();
                    bool b3 = (pb1.First - pb1.Second).abs() >= ConstantsAndSettings.GetMinBendLength();

                    rez = b1 && b2 && b3;
                }
                return rez;
            }
            public static Pair<Pair<int, PRE_BEND>, quaternion> ExistTriangle(PRE_BEND pb1, ref PRE_BEND_ARRAY arr)
            {
                Pair<int, PRE_BEND> p1 = new Pair<int, PRE_BEND>(-1, new PRE_BEND(new quaternion(), new quaternion()));
                Pair<Pair<int, PRE_BEND>, quaternion> rez = new Pair<Pair<int, PRE_BEND>, quaternion>(p1, new quaternion());
                // rez.First.First = -1 no closing Bend of the triangle
                // rez.First.First >= 0 There is a correct triangle
                // rez.First.First = -3 Length of the closing Bend is less than the required
                // rez.First.First = -2 length of the Bend is less than the required

                if ((arr != null) && (arr.Count > 0))
                {
                    foreach (PRE_BEND PB in arr)
                    {
                        if ((PB.Second - PB.First).abs() < ConstantsAndSettings.GetMinBendLength())//length of the Bend is less than the required
                        {
                            rez.First.First = -2;//length of the Bend is less than the required
                            break;
                        }
                        else
                        {
                            if (!IsEqualPreBend(PB, pb1))
                            {
                                Pair<Pair<bool, PRE_BEND>, quaternion> t = IsConected(PB, pb1);
                                if ((t.First.Second.Second - t.First.Second.First).abs() < ConstantsAndSettings.GetMinBendLength())//образуват триъгълник с малка по дължина затваряща страна
                                {
                                    rez.First.First = -3;//Length of the closing Bend is less than the required
                                    break;
                                }
                                else
                                {
                                    if (t.First.First)
                                    {
                                        foreach (PRE_BEND PBB in arr)//search closure triangle Bend in no fictive Bends
                                        {
                                            if (IsEqualPreBend(PBB, t.First.Second))
                                            {
                                                rez.First.First = 1;// exist correct triangle
                                                rez.First.Second = t.First.Second;
                                                rez.Second = t.Second;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (rez.First.First >= 0) { break; }
                        }
                    }
                }
                return rez;
            }
            public static Pair<Pair<int, PRE_BEND>, quaternion> ExistTriangle(PRE_BEND pb1, ref PRE_BEND_ARRAY arr, ref PRE_BEND_ARRAY arr_fictive)
            // rez.First.First >= 0 There is a correct triangle
            // rez.First.First = -1 no closing Bend of the triangle      
            // rez.First.First = -2 length of the Bend is less than the required
            // rez.First.First = -3 Length of the closing Bend is less than the required
            {
                Pair<int, PRE_BEND> p1 = new Pair<int, PRE_BEND>(-1, new PRE_BEND(new quaternion(), new quaternion()));
                Pair<Pair<int, PRE_BEND>, quaternion> rez = new Pair<Pair<int, PRE_BEND>, quaternion>(p1, new quaternion());
                // rez.First.First = -1 no closing Bend of the triangle
                // rez.First.First >= 0 There is a correct triangle
                // rez.First.First = -3 Length of the closing Bend is less than the required
                // rez.First.First = -2 length of the Bend is less than the required

                if ((arr != null) && (arr.Count > 0))
                {
                    foreach (PRE_BEND PB in arr)
                    {
                        if ((PB.Second - PB.First).abs() < ConstantsAndSettings.GetMinBendLength())//дължината на пръта е по- малка от необходимото
                        {
                            rez.First.First = -2;//дължината на пръта е по- малка от необходимото
                            break;
                        }
                        else
                        {
                            if (!IsEqualPreBend(PB, pb1))
                            {
                                Pair<Pair<bool, PRE_BEND>, quaternion> t = IsConected(PB, pb1);
                                if ((t.First.Second.Second - t.First.Second.First).abs() < ConstantsAndSettings.GetMinBendLength())//образуват триъгълник с малка по дължина затваряща страна
                                {
                                    rez.First.First = -3;//дължината на затварящата страна е по- малка от необходимото
                                    break;
                                }
                                else
                                {
                                    if (t.First.First)
                                    {
                                        foreach (PRE_BEND PBB in arr)//търсим затваряща триъгълника страна в нефиктивните пръти
                                        {
                                            if (IsEqualPreBend(PBB, t.First.Second))
                                            {
                                                rez.First.First = 1;// съществува коректен триъгълник
                                                rez.First.Second = t.First.Second;
                                                rez.Second = t.Second;
                                                break;
                                            }
                                        }
                                        if ((rez.First.First != 1) && (arr_fictive != null) && (arr_fictive.Count > 0))//търсим затваряща триъгълника страна в фиктивните пръти
                                        {
                                            foreach (PRE_BEND PBB in arr_fictive)
                                            {
                                                if (IsEqualPreBend(PBB, t.First.Second))
                                                {
                                                    rez.First.First = 1;// съществува коректен триъгълник
                                                    rez.First.Second = t.First.Second;
                                                    rez.Second = t.Second;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (rez.First.First >= 0) { break; }
                        }
                    }
                    if ((rez.First.First != 1) && (arr_fictive != null) && (arr_fictive.Count > 0))
                    {
                        rez = ExistTriangle(pb1, ref arr_fictive);
                    }
                }
                return rez;
            }

            public static Pair<Pair<bool, PRE_BEND>, quaternion> IsConected(PRE_BEND pb1, PRE_BEND pb2)
            // if they have a common point rez.First.First = true, rez.First.Second will contain both ends of the line formed by the free ends of the two lines, rez.Second - common point            
            //no common point rez.First.First = false, rez.First.Second will contain two starting points, rez.Second - the starting point of the first line
            {
                Pair<bool, PRE_BEND> rez = new Pair<bool, PRE_BEND>();

                double d1 = (pb1.First - pb2.First).abs(); // distance from the start point of the first line to the start point of the second line
                double d2 = (pb1.First - pb2.Second).abs(); // distance from the start point of the first line to the end point of the second line
                double d3 = (pb1.Second - pb2.First).abs(); //distance from the end point of the first line to the start point of the second line
                double d4 = (pb1.Second - pb2.Second).abs(); //distance from the end point of the first line to the end point of the second line


                rez.First = (d1 <= ConstantsAndSettings.GetMinDistBhetwenNodes()) || //starting points of the two lines coincide  
                           (d2 <= ConstantsAndSettings.GetMinDistBhetwenNodes()) || //start point of the first line coincides with the end of the second line 
                           (d3 <= ConstantsAndSettings.GetMinDistBhetwenNodes()) || //end point of the first line coincides with the start point of the second line
                           (d4 <= ConstantsAndSettings.GetMinDistBhetwenNodes());   //endpoints of the two lines coincide


                quaternion first = (d3 <= ConstantsAndSettings.GetMinDistBhetwenNodes() || d4 <= ConstantsAndSettings.GetMinDistBhetwenNodes()) ? pb1.First : pb1.Second;//свободен край от първата линия
                quaternion second = (d2 <= ConstantsAndSettings.GetMinDistBhetwenNodes() || d4 <= ConstantsAndSettings.GetMinDistBhetwenNodes()) ? pb2.First : pb2.Second;//свободен край от втората линия
                rez.Second = new PRE_BEND(first, second);//segment formed by the free ends of the two lines

                quaternion third = ((first - pb1.First).abs() <= ConstantsAndSettings.GetMinDistBhetwenNodes()) ? pb1.Second : pb1.First;// common point ( if rez.First=true )

                return new Pair<Pair<bool, PRE_BEND>, quaternion>(rez, third);
            }

            public static bool IsFound(int digit, POLYGONS_ARRAY ARR)
            {
                bool rez = false;
                foreach (WorkClasses.Polygon P in ARR)
                {
                    List<int> list = P.Triangles_Numers_Array;
                    foreach (int i in list)
                    {
                        if (i == digit)
                        {
                            rez = true;
                            break;
                        }
                    }
                    if (rez)
                        break;
                }
                return rez;
            }

            public static Matrix3d MathLibKCADUcsToMatrix3d(ref UCS ucs)
            {
                return new Matrix3d(new double[]{ucs.ang.GetAt(1, 1), ucs.ang.GetAt(1, 2), ucs.ang.GetAt(1, 3), ucs.o.GetX(),
                                                 ucs.ang.GetAt(2, 1), ucs.ang.GetAt(2, 2), ucs.ang.GetAt(2, 3), ucs.o.GetY(),
                                                 ucs.ang.GetAt(3, 1), ucs.ang.GetAt(3, 2), ucs.ang.GetAt(3, 3), ucs.o.GetZ(),
                                                 0.0,                 0.0,                 0.0,                 1.0});
            }

            public static double GetArea(quaternion q1, quaternion q2, quaternion q3)
            {
                double rez = 0.0;
                try
                {
                    UCS ucs = new UCS(q1, q2, q3);
                    quaternion node3 = ucs.FromACS(q3);
                    rez = (q2 - q1).abs() * node3.GetY() / 2.0;
                }
                catch { }
                return double.IsNaN(rez) ? 0.0 : rez;
            }
            public static double GetArea(PRE_TRIANGLE ptrr)
            {
                double rez = 0.0;
                try
                {
                    UCS ucs = new UCS(ptrr.First, ptrr.Second, ptrr.Third);
                    quaternion node3 = ucs.FromACS(ptrr.Third);
                    rez = (ptrr.Second - ptrr.First).abs() * node3.GetY() / 2.0;
                }
                catch { }
                return double.IsNaN(rez) ? 0.0 : rez;
            }
            public static bool IsParallel(PRE_BEND pb1, PRE_BEND pb2)
            {
                double d = ((pb2.Second - pb2.First) / (pb1.Second - pb1.First)).absV();
                return d < Constants.zero_dist;
            }

            public static int GetCommonNode(Bend b1, Bend b2)
            {
                int rez = -1;
                if (b1.StartNodeNumer == b2.EndNodeNumer || b1.StartNodeNumer == b2.StartNodeNumer)
                    rez = b1.StartNodeNumer;
                else
                    if (b1.EndNodeNumer == b2.EndNodeNumer || b1.EndNodeNumer == b2.StartNodeNumer)
                        rez = b1.EndNodeNumer;

                return rez;
            }

            #region GET
            public static Polyline GetPoly(ref quaternion[] list, bool close = true)
            {
                Polyline rez = null;
                if (list.Length > 2)
                {
                    rez = new Polyline();

                    quaternion origin = new quaternion();
                    foreach (quaternion qQ in list)
                        origin += qQ;
                    origin /= list.Length;

                    Point3d second = (Point3d)list[1];
                    if (((Point3d)list[0]).DistanceTo(second) < ConstantsAndSettings.GetMinDistBhetwenNodes())
                    {
                        for (int k = 2; k < list.Length; k++)
                        {
                            if (((Point3d)list[0]).DistanceTo((Point3d)list[k]) > ConstantsAndSettings.GetMinDistBhetwenNodes())
                            {
                                second = (Point3d)list[k];
                                break;
                            }
                        }
                    }

                    UCS ucs = new UCS(list[0], second, origin);
                    Matrix3d UCS = new Matrix3d(ucs.GetAutoCAD_Matrix3d());
                    for (int i = 0; i < list.Length; i++)
                    {
                        quaternion q = ucs.FromACS(list[i]);
                        rez.AddVertexAt(i, new Point2d(q.GetX(), q.GetY()), 0, 0, 0);
                    }
                    if (close)
                    {
                        quaternion qq = ucs.FromACS(list[0]);
                        rez.AddVertexAt(list.Length, new Point2d(qq.GetX(), qq.GetY()), 0, 0, 0);
                    }
                    rez.TransformBy(UCS);
                }

                return rez;
            }
            public static Polyline GetPoly(Point3dCollection list, bool close = true)
            {
                Polyline rez = null;

                if (list.Count > 2)
                {
                    rez = new Polyline();

                    quaternion origin = new quaternion();
                    foreach (Point3d qQ in list)
                        origin += qQ;
                    origin /= list.Count;

                    Point3d second = (Point3d)list[1];
                    if (((Point3d)list[0]).DistanceTo(second) < ConstantsAndSettings.GetMinDistBhetwenNodes())
                    {
                        for (int k = 2; k < list.Count; k++)
                        {
                            if (((Point3d)list[0]).DistanceTo((Point3d)list[k]) > ConstantsAndSettings.GetMinDistBhetwenNodes())
                            {
                                second = (Point3d)list[k];
                                break;
                            }
                        }
                    }

                    UCS ucs = new UCS(list[0], second, origin);
                    Matrix3d UCS = new Matrix3d(ucs.GetAutoCAD_Matrix3d());
                    for (int i = 0; i < list.Count; i++)
                    {
                        quaternion q = ucs.FromACS(list[i]);
                        rez.AddVertexAt(i, new Point2d(q.GetX(), q.GetY()), 0, 0, 0);
                    }
                    if (close)
                    {
                        quaternion qq = ucs.FromACS(list[0]);
                        rez.AddVertexAt(list.Count, new Point2d(qq.GetX(), qq.GetY()), 0, 0, 0);
                    }
                    rez.TransformBy(UCS);
                }

                return rez;
            }
            public static Polyline GetPoly(Point3dCollection list, ref UCS pLineUCS, ref  Matrix3d pLineUCS_Matrix, bool close = true)
            {
                Polyline rez = null;

                if (list.Count > 2)
                {
                    rez = new Polyline();

                    quaternion origin = new quaternion();
                    foreach (Point3d qQ in list)
                        origin += qQ;
                    origin /= list.Count;

                    Point3d second = (Point3d)list[1];
                    if (((Point3d)list[0]).DistanceTo(second) < ConstantsAndSettings.GetMinDistBhetwenNodes())
                    {
                        for (int k = 2; k < list.Count; k++)
                        {
                            if (((Point3d)list[0]).DistanceTo((Point3d)list[k]) > ConstantsAndSettings.GetMinDistBhetwenNodes())
                            {
                                second = (Point3d)list[k];
                                break;
                            }
                        }
                    }

                    UCS ucs = new UCS(list[0], second, origin);
                    Matrix3d UCS = new Matrix3d(ucs.GetAutoCAD_Matrix3d());
                    pLineUCS = ucs;
                    pLineUCS_Matrix = UCS;
                    for (int i = 0; i < list.Count; i++)
                    {
                        quaternion q = ucs.FromACS(list[i]);
                        rez.AddVertexAt(i, new Point2d(q.GetX(), q.GetY()), 0, 0, 0);
                    }
                    if (close)
                    {
                        quaternion qq = ucs.FromACS(list[0]);
                        rez.AddVertexAt(list.Count, new Point2d(qq.GetX(), qq.GetY()), 0, 0, 0);
                    }
                    rez.TransformBy(UCS);
                }

                return rez;
            }

            public static Pair<string, PromptStatus> GetKey(string[] keys, int def, string Mess, bool AllowNone = true)
            {
                Pair<string, PromptStatus> rez = new Pair<string, PromptStatus>("", PromptStatus.Error);
                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\n" + Mess;

                foreach (string str in keys)
                    pKeyOpts.Keywords.Add(str);

                pKeyOpts.AllowNone = AllowNone;
                try
                {
                    pKeyOpts.Keywords.Default = keys[def];
                }
                catch
                {
                    return rez;
                }

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                    rez = new Pair<string, PromptStatus>(pKeyRes.StringResult, PromptStatus.OK);

                return rez;
            }
            public static Pair<string, PromptStatus> GetString(string def, string Mess, bool AllowSpaces = false)
            {
                Pair<string, PromptStatus> rez = new Pair<string, PromptStatus>(def, PromptStatus.Error);
                PromptStringOptions pStrOpts = new PromptStringOptions("");
                pStrOpts.Message = "\n" + Mess;
                pStrOpts.AllowSpaces = AllowSpaces;
                pStrOpts.DefaultValue = def;
                PromptResult pStrRes = Application.DocumentManager.MdiActiveDocument.Editor.GetString(pStrOpts);
                if (pStrRes.Status == PromptStatus.OK)
                    rez = new Pair<string, PromptStatus>(pStrRes.StringResult, PromptStatus.OK);

                return rez;
            }
            public static Pair<double, PromptStatus> GetDouble(double def, string Mess, bool AllowZero = false, bool AllowNegative = false)
            {
                Pair<double, PromptStatus> rez = new Pair<double, PromptStatus>(def, PromptStatus.Error);

                PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
                pDoubleOpts.Message = "\n" + Mess;

                pDoubleOpts.AllowZero = AllowZero;
                pDoubleOpts.AllowNegative = AllowNegative;
                pDoubleOpts.DefaultValue = def;

                PromptDoubleResult pDoubleRes = Application.DocumentManager.MdiActiveDocument.Editor.GetDouble(pDoubleOpts);
                if (pDoubleRes.Status == PromptStatus.OK)
                    rez = new Pair<double, PromptStatus>(pDoubleRes.Value, pDoubleRes.Status);

                return rez;
            }
            public static Pair<int, PromptStatus> GetInt(int def, string Mess, bool AllowZero = false, bool AllowNegative = false)
            {
                Pair<int, PromptStatus> rez = new Pair<int, PromptStatus>(def, PromptStatus.Error);

                PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
                pIntOpts.Message = "\n" + Mess;
                pIntOpts.AllowZero = AllowZero;
                pIntOpts.AllowNegative = AllowNegative;
                pIntOpts.DefaultValue = def;

                PromptIntegerResult pIntRes = Application.DocumentManager.MdiActiveDocument.Editor.GetInteger(pIntOpts);
                if (pIntRes.Status == PromptStatus.OK)
                    rez = new Pair<int, PromptStatus>(pIntRes.Value, pIntRes.Status);

                return rez;
            }
            public static Pair<Point3d, PromptStatus> GetPoint(Point3d def, string Mess, Point3d baseP = new Point3d(), bool UseBasePoint = false, bool AllowNone = false)
            {
                Pair<Point3d, PromptStatus> rez = new Pair<Point3d, PromptStatus>(def, PromptStatus.Error);
                PromptPointOptions pPtOpts = new PromptPointOptions("");
                pPtOpts.Message = "\n" + Mess;
                pPtOpts.UseBasePoint = true;
                pPtOpts.AllowNone = AllowNone;
                pPtOpts.UseBasePoint = UseBasePoint;
                if (UseBasePoint)
                    pPtOpts.BasePoint = baseP;

                PromptPointResult pPtRes = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(pPtOpts);
                if (pPtRes.Status == PromptStatus.OK)
                    rez = new Pair<Point3d, PromptStatus>(pPtRes.Value, pPtRes.Status);

                return rez;
            }
            #endregion

            #region Points Collections
            public static Point3dCollection GetPoints3D_Collection(ref Triplet<double, double, double>[] list)
            {
                Point3dCollection rez = new Point3dCollection();
                foreach (Triplet<double, double, double> t in list)
                {
                    rez.Add(new Point3d(t.First, t.Second, t.Third));
                }
                return rez;
            }
            public static Point3dCollection GetPoints3D_Collection(ref quaternion[] list)
            {
                Point3dCollection rez = new Point3dCollection();
                foreach (quaternion t in list)
                {
                    rez.Add((Point3d)t);
                }
                return rez;
            }
            public static Point3dCollection GetPoints3D_Collection(ref List<Triplet<double, double, double>> list)
            {
                Point3dCollection rez = new Point3dCollection();
                foreach (Triplet<double, double, double> t in list)
                {
                    rez.Add(new Point3d(t.First, t.Second, t.Third));
                }
                return rez;
            }
            public static Point3dCollection GetPoints3D_Collection(ref List<quaternion> list)
            {
                Point3dCollection rez = new Point3dCollection();
                foreach (quaternion t in list)
                {
                    rez.Add((Point3d)t);
                }
                return rez;
            }
            public static Point2dCollection GetPoints2D_Collection(ref Pair<double, double>[] list)
            {
                Point2dCollection rez = new Point2dCollection();
                foreach (Pair<double, double> t in list)
                {
                    rez.Add(new Point2d(t.First, t.Second));
                }
                return rez;
            }
            public static Point2dCollection GetPoints2D_Collection(ref complex[] list)
            {
                Point2dCollection rez = new Point2dCollection();
                foreach (complex t in list)
                {
                    rez.Add(new Point2d(t.real(), t.imag()));
                }
                return rez;
            }
            public static Point2dCollection GetPoints2D_Collection(ref List<complex> list)
            {
                Point2dCollection rez = new Point2dCollection();
                foreach (complex t in list)
                {
                    rez.Add(new Point2d(t.real(), t.imag()));
                }
                return rez;
            }
            public static Point2dCollection GetPoints2D_Collection(ref List<Pair<double, double>> list)
            {
                Point2dCollection rez = new Point2dCollection();
                foreach (Pair<double, double> t in list)
                {
                    rez.Add(new Point2d(t.First, t.Second));
                }
                return rez;
            }
            #endregion

            #region draw bez tranzakciq

            public static ObjectId GetPoly3d(Point3dCollection list, bool close = true)//bez tranzakciq
            {
                ObjectId REZ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                if (list.Count > 2)
                {
                    using (Polyline3d rez = new Polyline3d())
                    {
                        rez.SetDatabaseDefaults();
                        using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                        {
                            REZ = curSpace.AppendEntity(rez);
                            foreach (Point3d pnt in list)
                            {
                                using (PolylineVertex3d poly3dVertex = new PolylineVertex3d(pnt))
                                    rez.AppendVertex(poly3dVertex);
                            }
                            if (close == true)
                                using (PolylineVertex3d poly3dVertex = new PolylineVertex3d(list[0]))
                                    rez.AppendVertex(poly3dVertex);
                        }
                    }
                }
                ed.UpdateScreen();
                return REZ;
            }

            #region Line
            public static ObjectId DrawLine(Point3d p1, Point3d p2)//bez tranzakciq
            {
                ObjectId REZ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Line rez = new Line(p1, p2))
                {
                    rez.SetDatabaseDefaults();
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }
                //ed.WriteMessage("");
                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawLine(Point3d p1, Point3d p2, int colorindex)//bez tranzakciq
            {
                ObjectId REZ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Line rez = new Line(p1, p2))
                {
                    rez.SetDatabaseDefaults();
                    rez.ColorIndex = colorindex;
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }
                //ed.WriteMessage("");
                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawLine(Point3d p1, Point3d p2, int colorindex, ref Matrix3d transform)//bez tranzakciq
            {
                ObjectId REZ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Line rez = new Line(p1, p2))
                {
                    rez.SetDatabaseDefaults();
                    rez.ColorIndex = colorindex;
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }
                //ed.WriteMessage("");
                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawLine(Point3d p1, Point3d p2, string layer)//bez tranzakciq
            {
                ObjectId REZ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Line rez = new Line(p1, p2))
                {
                    rez.Layer = layer;
                    rez.SetDatabaseDefaults();
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }
                //ed.WriteMessage("");
                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawLine(Point3d p1, Point3d p2, string layer, ref Matrix3d transform)//bez tranzakciq
            {
                ObjectId REZ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Line rez = new Line(p1, p2))
                {
                    rez.SetDatabaseDefaults();
                    rez.Layer = layer;
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }
                //ed.WriteMessage("");
                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawLine(Point3d p1, Point3d p2, string layer, int colorindex, ref Matrix3d transform)//bez tranzakciq
            {
                ObjectId REZ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Line rez = new Line(p1, p2))
                {
                    rez.SetDatabaseDefaults();
                    rez.Layer = layer;
                    rez.ColorIndex = colorindex;
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }
                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawLine(Point3d p1, Point3d p2, ref Matrix3d transform)//bez tranzakciq
            {
                ObjectId REZ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Line rez = new Line(p1, p2))
                {
                    rez.SetDatabaseDefaults();
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }
                //ed.WriteMessage("");
                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawLine(Point3d p1, Point3d p2, int colorindex, string layer)//bez tranzakciq
            {
                ObjectId REZ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Line rez = new Line(p1, p2))
                {
                    rez.SetDatabaseDefaults();
                    rez.ColorIndex = colorindex;
                    rez.Layer = layer;
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }
                //ed.WriteMessage("");
                ed.UpdateScreen();
                return REZ;
            }
            #endregion

            #region circle
            public static ObjectId DrawCircle(double radius, Point3d Center, string layer, int colorindex, ref Matrix3d transform)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Circle rez = new Circle())
                {
                    rez.SetDatabaseDefaults();
                    rez.Center = Center;
                    rez.Radius = radius;
                    rez.Layer = layer;
                    rez.ColorIndex = colorindex;
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawCircle(double radius, Point3d Center, ref Matrix3d transform)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Circle rez = new Circle())
                {
                    rez.SetDatabaseDefaults();
                    rez.Center = Center;
                    rez.Radius = radius;
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawCircle(double radius, Point3d Center, int colorindex, ref Matrix3d transform)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Circle rez = new Circle())
                {
                    rez.SetDatabaseDefaults();
                    rez.ColorIndex = colorindex;
                    rez.Center = Center;
                    rez.Radius = radius;
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawCircle(double radius, Point3d Center, string layer, ref Matrix3d transform)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Circle rez = new Circle())
                {
                    rez.SetDatabaseDefaults();
                    rez.Layer = layer;
                    rez.Center = Center;
                    rez.Radius = radius;
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawCircle(double radius, Point3d Center, string layer)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Circle rez = new Circle())
                {
                    rez.SetDatabaseDefaults();
                    rez.Layer = layer;
                    rez.Center = Center;
                    rez.Radius = radius;
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawCircle(double radius, Point3d Center, int colorindex)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Circle rez = new Circle())
                {
                    rez.SetDatabaseDefaults();
                    rez.ColorIndex = colorindex;
                    rez.Center = Center;
                    rez.Radius = radius;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawCircle(double radius, Point3d Center, int colorindex, string layer)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Circle rez = new Circle())
                {
                    rez.SetDatabaseDefaults();
                    rez.ColorIndex = colorindex;
                    rez.Layer = layer;
                    rez.Center = Center;
                    rez.Radius = radius;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawCircle(double radius, Point3d Center)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Circle rez = new Circle())
                {
                    rez.SetDatabaseDefaults();
                    rez.Center = Center;
                    rez.Radius = radius;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            #endregion

            #region sphere
            public static ObjectId DrawSphere(double radius, Point3d Center)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Solid3d rez = new Solid3d())
                {
                    rez.SetDatabaseDefaults();
                    rez.CreateSphere(radius);
                    rez.TransformBy(Matrix3d.Displacement(Center - Point3d.Origin));
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawSphere(double radius, Point3d Center, int colorindex)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Solid3d rez = new Solid3d())
                {
                    rez.SetDatabaseDefaults();
                    rez.CreateSphere(radius);
                    rez.ColorIndex = colorindex;

                    rez.TransformBy(Matrix3d.Displacement(Center - Point3d.Origin));
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawSphere(double radius, Point3d Center, string layer)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Solid3d rez = new Solid3d())
                {
                    rez.SetDatabaseDefaults();
                    rez.CreateSphere(radius);
                    rez.Layer = layer;
                    rez.TransformBy(Matrix3d.Displacement(Center - Point3d.Origin));
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawSphere(double radius, Point3d Center, int colorindex, string layer)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Solid3d rez = new Solid3d())
                {
                    rez.SetDatabaseDefaults();
                    rez.CreateSphere(radius);
                    rez.ColorIndex = colorindex;
                    rez.Layer = layer;
                    rez.TransformBy(Matrix3d.Displacement(Center - Point3d.Origin));
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawSphere(double radius, Point3d Center, ref Matrix3d transform)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Solid3d rez = new Solid3d())
                {
                    rez.SetDatabaseDefaults();
                    rez.CreateSphere(radius);
                    rez.TransformBy(Matrix3d.Displacement(Center - Point3d.Origin));
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawSphere(double radius, Point3d Center, int colorindex, ref Matrix3d transform)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Solid3d rez = new Solid3d())
                {
                    rez.SetDatabaseDefaults();
                    rez.CreateSphere(radius);
                    rez.ColorIndex = colorindex;
                    rez.TransformBy(Matrix3d.Displacement(Center - Point3d.Origin));
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawSphere(double radius, Point3d Center, string layer, ref Matrix3d transform)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Solid3d rez = new Solid3d())
                {
                    rez.SetDatabaseDefaults();
                    rez.CreateSphere(radius);
                    rez.Layer = layer;
                    rez.TransformBy(Matrix3d.Displacement(Center - Point3d.Origin));
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            public static ObjectId DrawSphere(double radius, Point3d Center, int colorindex, string layer, ref Matrix3d transform)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                ObjectId REZ = ObjectId.Null;

                using (Solid3d rez = new Solid3d())
                {
                    rez.SetDatabaseDefaults();
                    rez.CreateSphere(radius);
                    rez.ColorIndex = colorindex;
                    rez.Layer = layer;
                    rez.TransformBy(Matrix3d.Displacement(Center - Point3d.Origin));
                    rez.TransformBy(transform);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        REZ = curSpace.AppendEntity(rez);
                    }
                }

                ed.UpdateScreen();
                return REZ;
            }
            #endregion

            #region TEXT
            public static ObjectId DrawText(Point3d Position, double Height, string TextString)//bez tranzakciq
            {
                ObjectId acText_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (DBText acText = new DBText())
                {
                    acText.SetDatabaseDefaults();
                    acText.Position = Position;
                    acText.Height = Height;
                    acText.TextString = TextString;
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acText_ = curSpace.AppendEntity(acText);
                    }
                }
                ed.UpdateScreen();
                return acText_;
            }
            public static ObjectId DrawText(Point3d Position, double Height, string TextString, ref Matrix3d mat)//bez tranzakciq
            {
                ObjectId acText_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (DBText acText = new DBText())
                {
                    acText.SetDatabaseDefaults();
                    acText.Position = Position;
                    acText.Height = Height;
                    acText.TextString = TextString;
                    if ((object)mat != null)
                        acText.TransformBy(mat);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acText_ = curSpace.AppendEntity(acText);
                    }
                }
                ed.UpdateScreen();
                return acText_;
            }
            public static ObjectId DrawText(Point3d Position, double Height, string TextString, int colorIndex)//bez tranzakciq
            {
                ObjectId acText_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (DBText acText = new DBText())
                {
                    acText.SetDatabaseDefaults();
                    acText.Position = Position;
                    acText.Height = Height;
                    acText.TextString = TextString;
                    acText.ColorIndex = colorIndex;
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acText_ = curSpace.AppendEntity(acText);
                    }
                }
                ed.UpdateScreen();
                return acText_;
            }
            public static ObjectId DrawText(Point3d Position, double Height, string TextString, int colorIndex, ref Matrix3d mat)//bez tranzakciq
            {
                ObjectId acText_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (DBText acText = new DBText())
                {
                    acText.SetDatabaseDefaults();
                    acText.Position = Position;
                    acText.Height = Height;
                    acText.TextString = TextString;
                    acText.ColorIndex = colorIndex;
                    if ((object)mat != null)
                        acText.TransformBy(mat);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acText_ = curSpace.AppendEntity(acText);
                    }
                }
                ed.UpdateScreen();
                return acText_;
            }
            public static ObjectId DrawText(Point3d Position, double Height, string TextString, int colorIndex, string layer, ref Matrix3d mat)//bez tranzakciq
            {
                ObjectId acText_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (DBText acText = new DBText())
                {
                    acText.SetDatabaseDefaults();
                    acText.Position = Position;
                    acText.Height = Height;
                    acText.TextString = TextString;
                    acText.ColorIndex = colorIndex;
                    acText.Layer = layer;
                    if ((object)mat != null)
                        acText.TransformBy(mat);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acText_ = curSpace.AppendEntity(acText);
                    }
                }
                ed.UpdateScreen();
                return acText_;
            }
            public static ObjectId DrawText(Point3d Position, double Height, string TextString, int colorIndex, string layer)//bez tranzakciq
            {
                ObjectId acText_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (DBText acText = new DBText())
                {
                    acText.SetDatabaseDefaults();
                    acText.Position = Position;
                    acText.Height = Height;
                    acText.TextString = TextString;
                    acText.ColorIndex = colorIndex;
                    acText.Layer = layer;
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acText_ = curSpace.AppendEntity(acText);
                    }
                }
                ed.UpdateScreen();
                return acText_;
            }
            public static ObjectId DrawText(Point3d Position, double Height, string TextString, string layer)//bez tranzakciq
            {
                ObjectId acText_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (DBText acText = new DBText())
                {
                    acText.SetDatabaseDefaults();
                    acText.Position = Position;
                    acText.Height = Height;
                    acText.TextString = TextString;
                    acText.Layer = layer;
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acText_ = curSpace.AppendEntity(acText);
                    }
                }
                ed.UpdateScreen();
                return acText_;
            }
            public static ObjectId DrawText(Point3d Position, double Height, string TextString, string layer, ref Matrix3d mat)//bez tranzakciq
            {
                ObjectId acText_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (DBText acText = new DBText())
                {
                    acText.SetDatabaseDefaults();
                    acText.Position = Position;
                    acText.Height = Height;
                    acText.TextString = TextString;
                    acText.Layer = layer;
                    if ((object)mat != null)
                        acText.TransformBy(mat);
                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acText_ = curSpace.AppendEntity(acText);
                    }
                }
                ed.UpdateScreen();
                return acText_;
            }
            #endregion

            #region Poly2D
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }

                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, ref Matrix3d mat, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, ref Matrix3d mat, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, int colorIndex, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, string layer, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, ref Matrix3d mat, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Point2dCollection Points, ref Matrix3d mat, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, Points[i], 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, Points[0], 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }

            public static ObjectId DrawPoly2D(ref List<complex> Points, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, ref Matrix3d mat, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, ref Matrix3d mat, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, int colorIndex, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, string layer, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, ref Matrix3d mat, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<complex> Points, ref Matrix3d mat, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }

            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, ref Matrix3d mat, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, ref Matrix3d mat, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, int colorIndex, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, string layer, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, ref Matrix3d mat, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref List<Pair<double, double>> Points, ref Matrix3d mat, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Count, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }

            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, ref Matrix3d mat, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, ref Matrix3d mat, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, int colorIndex, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, string layer, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, ref Matrix3d mat, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref Pair<double, double>[] Points, ref Matrix3d mat, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].First, Points[i].Second), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].First, Points[0].Second), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }

            public static ObjectId DrawPoly2D(ref complex[] Points, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, ref Matrix3d mat, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, ref Matrix3d mat, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, int colorIndex, string layer, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, string layer, int colorIndex, ref Matrix3d mat, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, ref Matrix3d mat, string layer, int colorIndex, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            public static ObjectId DrawPoly2D(ref complex[] Points, ref Matrix3d mat, int colorIndex, string layer, bool close = true)//bez tranzakciq
            {
                ObjectId acPoly_ = ObjectId.Null;
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.SetDatabaseDefaults();

                    for (int i = 0; i < Points.Length; i++)
                    {
                        acPoly.AddVertexAt(i, new Point2d(Points[i].real(), Points[i].imag()), 0, 0, 0);
                    }
                    if (close)
                        acPoly.AddVertexAt(Points.Length, new Point2d(Points[0].real(), Points[0].imag()), 0, 0, 0);

                    acPoly.ColorIndex = colorIndex;
                    acPoly.Layer = layer;

                    if ((object)mat != null)
                        acPoly.TransformBy(mat);

                    using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                    {
                        acPoly_ = curSpace.AppendEntity(acPoly);
                    }
                }
                ed.UpdateScreen();
                return acPoly_;
            }
            #endregion

            #endregion

            //-------------

            //first line (sL1,eL1), second line (sL2,eL2), return PRE_BEND - min segment between lines
            public static PRE_BEND GetCrossingLinesDistance(quaternion sL1, quaternion eL1, quaternion sL2, quaternion eL2, bool draw = false, int colorIndex_D = 1)
            {
                PRE_BEND rez = null;

                Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;

                try
                {
                    {

                        quaternion fLfP = sL1;
                        {
                            quaternion fLsP = eL1;
                            {
                                quaternion sLfP = sL2;
                                {
                                    quaternion sLsP = eL2;

                                    quaternion FQ = fLsP - fLfP;
                                    quaternion SQ = sLsP - sLfP;

                                    UCS UCS = new UCS(fLfP, fLsP, fLfP + SQ);

                                    fLfP = UCS.FromACS(fLfP); fLsP = UCS.FromACS(fLsP);
                                    sLfP = UCS.FromACS(sLfP); sLsP = UCS.FromACS(sLsP);

                                    double dist = Math.Abs(sLsP.GetZ());

                                    #region draw
                                    if ((dist > 0) && !double.IsNaN(dist))
                                    {
                                        line2d fLine = new line2d(new complex(fLfP.GetX(), fLfP.GetY()),
                                            new complex(fLsP.GetX(), fLsP.GetY()));

                                        line2d sLine = new line2d(new complex(sLfP.GetX(), sLfP.GetY()),
                                            new complex(sLsP.GetX(), sLsP.GetY()));

                                        complex lC = fLine.IntersectWitch(sLine);

                                        quaternion F = UCS.ToACS(new quaternion(0.0, lC.real(), lC.imag(), 0.0));
                                        quaternion S = UCS.ToACS(new quaternion(0.0, lC.real(), lC.imag(), sLsP.GetZ()));

                                        if (draw)
                                            DrawLine(new Point3d(F.GetX(), F.GetY(), F.GetZ()), new Point3d(S.GetX(), S.GetY(), S.GetZ()), colorIndex_D);
                                        rez = new PRE_BEND(F, S);
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return rez;
            }

            //--  Glass Orinted functions
            private static Point3dCollection KojtoGlassDomes_GetGlassConturByOffsetHeigtForPlanarRegions(ref Containers container, double height)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                Point3dCollection rez = new Point3dCollection();

                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl;
                    acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (WorkClasses.Polygon POL in container.Polygons)
                    {

                        List<List<quaternion>> qlist1 = container.Get_Glass_Edge(POL, true);
                        List<quaternion> list = new List<quaternion>();

                        bool planar = POL.IsPlanar(ref container.Triangles);

                        if (planar)
                        {
                            #region
                            foreach (List<quaternion> llist in qlist1)
                            {
                                foreach (quaternion q in llist)
                                {
                                    list.Add(q);
                                }
                            }
                            #endregion

                            #region planar
                            Triangle tr = container.Triangles[POL.Triangles_Numers_Array[0]];
                            quaternion mV = (tr.Normal.Second - tr.Normal.First);
                            mV /= mV.abs(); mV *= height;

                            Polyline3d acPoly3d = new Polyline3d();
                            acPoly3d.SetDatabaseDefaults();
                            acBlkTblRec.AppendEntity(acPoly3d);
                            trans.AddNewlyCreatedDBObject(acPoly3d, true);

                            foreach (quaternion Q in list)
                            {
                                PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)(Q + mV));
                                acPoly3d.AppendVertex(acPolVer3d);
                                trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                rez.Add((Point3d)(Q + mV));
                            }
                            quaternion Q_ = list[0] + mV;
                            PolylineVertex3d acPolVer3d_ = new PolylineVertex3d((Point3d)Q_);
                            acPoly3d.AppendVertex(acPolVer3d_);
                            trans.AddNewlyCreatedDBObject(acPolVer3d_, true);
                            rez.Add((Point3d)Q_);
                            #endregion
                        }

                    }
                    trans.Commit();
                    ed.UpdateScreen();
                }

                return rez;
            }
            private static Point3dCollection KojtoGlassDomes_GetGlassConturByOffsetHeigtForPlanarRegions(ref Containers container, WorkClasses.Polygon POL, double height, bool show = true)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                Point3dCollection rez = new Point3dCollection();

                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl;
                    acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    List<List<quaternion>> qlist1 = container.Get_Glass_Edge(POL, true);
                    List<quaternion> list = new List<quaternion>();

                    bool planar = POL.IsPlanar(ref container.Triangles);

                    if (planar)
                    {
                        #region
                        foreach (List<quaternion> llist in qlist1)
                        {
                            foreach (quaternion q in llist)
                            {
                                list.Add(q);
                            }
                        }
                        #endregion

                        #region planar
                        Triangle tr = container.Triangles[POL.Triangles_Numers_Array[0]];
                        quaternion mV = (tr.Normal.Second - tr.Normal.First);
                        mV /= mV.abs(); mV *= height;

                        Polyline3d acPoly3d = new Polyline3d();
                        if (show)
                        {
                            acPoly3d.SetDatabaseDefaults();
                            acBlkTblRec.AppendEntity(acPoly3d);
                            trans.AddNewlyCreatedDBObject(acPoly3d, true);
                        }

                        foreach (quaternion Q in list)
                        {
                            if (show)
                            {
                                PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)(Q + mV));
                                acPoly3d.AppendVertex(acPolVer3d);
                                trans.AddNewlyCreatedDBObject(acPolVer3d, true);
                            }
                            rez.Add((Point3d)(Q + mV));
                        }
                        quaternion Q_ = list[0] + mV;
                        if (show)
                        {
                            PolylineVertex3d acPolVer3d_ = new PolylineVertex3d((Point3d)Q_);
                            acPoly3d.AppendVertex(acPolVer3d_);
                            trans.AddNewlyCreatedDBObject(acPolVer3d_, true);
                        }
                        rez.Add((Point3d)Q_);
                        #endregion
                    }

                    trans.Commit();
                    ed.UpdateScreen();
                }

                return rez;
            }
           
            //--------- 3d mashine

            public static Solid3d GetPolygonGlassByheight(ref Containers container, WorkClasses.Polygon POL, double height, Transaction trans, BlockTableRecord acBlkTblRec)
            {
                if (ConstantsAndSettings.solidFuncVariant == 0)
                    return GetPolygonGlassByheightOLD(ref container, POL, height, trans, acBlkTblRec);
                else
                   return GetPolygonGlassByheightNEW(ref container, POL, height, trans, acBlkTblRec);
            }

            #region new 3d mashine
            public static Solid3d GetPolygonGlassByheightNEW(ref Containers container, WorkClasses.Polygon POL, double height, Transaction trans, BlockTableRecord acBlkTblRec)
            {          
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                Solid3d Solid = null;
          
                using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
                {
                    //foreach (WorkClasses.Polygon POL in container.Polygons)
                    {
                        ObjectIdCollection coll = new ObjectIdCollection();
                        List<Pair<int, ObjectId>> coll2 = new List<Pair<int, ObjectId>>();
                        List<Pair<int, ObjectId>> coll2_negative = new List<Pair<int, ObjectId>>();

                        foreach (int trN in POL.Triangles_Numers_Array)
                        {
                            Triangle TR = container.Triangles[trN];

                            quaternion normal = TR.Normal.Second - TR.Normal.First;
                            normal /= normal.abs();
                            normal *= height;

                            Pair<Polyline, PRE_TRIANGLE> z = TR.GetOffsetContourAs3DPolyLine(1, ref container, true, -150.0);

                            using (Polyline rez = z.First)
                            {
                                curSpace.AppendEntity(rez);

                                using (Solid3d solid = new Solid3d())
                                {
                                    solid.CreateExtrudedSolid(rez, new Vector3d(normal.GetX(), normal.GetY(), normal.GetZ()), new SweepOptions());

                                    //--------------
                                    #region firstBend
                                    //if (container.Bends[TR.GetFirstBendNumer()].IsFictive())
                                    {
                                        Pair<Polyline, Point3dCollection> zz = container.Bends[TR.GetFirstBendNumer()].GetCutSolid(TR.Normal.First, 0.0);
                                        using (Polyline REZ = zz.First)
                                        {
                                            REZ.Closed = true;

                                            UCS ucs = container.Bends[TR.GetFirstBendNumer()].GetUCS();
                                            if ((ucs.FromACS(TR.Normal.Second)).GetZ() < 0.0)
                                                ucs = container.Bends[TR.GetFirstBendNumer()].GetUCS_A();

                                            quaternion Z = ucs.ToACS(new quaternion(0, 0, 0, -10000.0)) - container.Bends[TR.GetFirstBendNumer()].MidPoint;

                                            using (Solid3d sol = new Solid3d())
                                            {
                                                sol.CreateExtrudedSolid(REZ, new Vector3d(Z.GetX(), Z.GetY(), Z.GetZ()), new SweepOptions());
                                                solid.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                                            }

                                            try
                                            {
                                                REZ.Erase();
                                            }
                                            catch { }
                                        }

                                    }
                                    #endregion

                                    #region secondBend
                                    //if (container.Bends[TR.GetSecondBendNumer()].IsFictive())
                                    {
                                        Pair<Polyline, Point3dCollection> zz = container.Bends[TR.GetSecondBendNumer()].GetCutSolid(TR.Normal.First, 0.0);
                                        using (Polyline REZ = zz.First)
                                        {
                                            REZ.Closed = true;

                                            UCS ucs = container.Bends[TR.GetSecondBendNumer()].GetUCS();
                                            if ((ucs.FromACS(TR.Normal.Second)).GetZ() < 0.0)
                                                ucs = container.Bends[TR.GetSecondBendNumer()].GetUCS_A();

                                            quaternion Z = ucs.ToACS(new quaternion(0, 0, 0, -10000.0)) - container.Bends[TR.GetSecondBendNumer()].MidPoint;

                                            using (Solid3d sol = new Solid3d())
                                            {
                                                sol.CreateExtrudedSolid(REZ, new Vector3d(Z.GetX(), Z.GetY(), Z.GetZ()), new SweepOptions());
                                                solid.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                                            }

                                            try
                                            {
                                                REZ.Erase();
                                            }
                                            catch { }
                                        }
                                    }
                                    #endregion

                                    #region thirdBend
                                    // if (container.Bends[TR.GetThirdBendNumer()].IsFictive())
                                    {
                                        Pair<Polyline, Point3dCollection> zz = container.Bends[TR.GetThirdBendNumer()].GetCutSolid(TR.Normal.First, 0.0);
                                        using (Polyline REZ = zz.First)
                                        {
                                            REZ.Closed = true;

                                            UCS ucs = container.Bends[TR.GetThirdBendNumer()].GetUCS();
                                            if ((ucs.FromACS(TR.Normal.Second)).GetZ() < 0.0)
                                                ucs = container.Bends[TR.GetThirdBendNumer()].GetUCS_A();

                                            quaternion Z = ucs.ToACS(new quaternion(0, 0, 0, -10000.0)) - container.Bends[TR.GetThirdBendNumer()].MidPoint;

                                            using (Solid3d sol = new Solid3d())
                                            {
                                                sol.CreateExtrudedSolid(REZ, new Vector3d(Z.GetX(), Z.GetY(), Z.GetZ()), new SweepOptions());
                                                solid.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                                            }

                                            try
                                            {
                                                REZ.Erase();
                                            }
                                            catch { }
                                        }
                                    }
                                    #endregion

                                    //-----------------                                    
                                    //-----------------
                                    if (ConstantsAndSettings.solidTrimPiksVariant == 1)
                                    {
                                        #region first Pick
                                        if (TR.IsFirstBendFictive() && TR.IsSecondBendFictive())
                                        {
                                            Bend bend1 = container.Bends[TR.GetFirstBendNumer()];
                                            Bend bend2 = container.Bends[TR.GetSecondBendNumer()];

                                            //common node
                                            quaternion Q = ((bend1.StartNodeNumer == bend2.StartNodeNumer) || (bend1.StartNodeNumer == bend2.EndNodeNumer)) ?
                                                bend1.Start : bend1.End;


                                            Pair<Polyline, Point3dCollection> ZZ = TR.GetPickCuter(ref container, 0);
                                            using (Polyline REZ = ZZ.First)
                                            {
                                                REZ.Closed = true;

                                                UCS ucs = new UCS(bend1.MidPoint, bend2.MidPoint, Q);
                                                if ((ucs.FromACS(TR.Normal.Second)).GetZ() < 0.0)
                                                    ucs = new UCS(bend2.MidPoint, bend1.MidPoint, Q);

                                                quaternion Z = ucs.ToACS(new quaternion(0, 0, 10000.0, 0.0)) - container.Bends[TR.GetThirdBendNumer()].MidPoint;

                                                using (Solid3d sol = new Solid3d())
                                                {
                                                    sol.CreateExtrudedSolid(REZ, new Vector3d(Z.GetX(), Z.GetY(), Z.GetZ()), new SweepOptions());
                                                    solid.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                                                }

                                                try
                                                {
                                                    REZ.Erase();
                                                }
                                                catch { }
                                            }
                                        }
                                        #endregion

                                        #region second Pick
                                        if (TR.IsFirstBendFictive() && TR.IsThirdBendFictive())
                                        {
                                            Bend bend1 = container.Bends[TR.GetFirstBendNumer()];
                                            Bend bend2 = container.Bends[TR.GetThirdBendNumer()];

                                            //common node
                                            quaternion Q = ((bend1.StartNodeNumer == bend2.StartNodeNumer) || (bend1.StartNodeNumer == bend2.EndNodeNumer)) ?
                                                bend1.Start : bend1.End;


                                            Pair<Polyline, Point3dCollection> ZZ = TR.GetPickCuter(ref container, 1);
                                            using (Polyline REZ = ZZ.First)
                                            {
                                                REZ.Closed = true;

                                                UCS ucs = new UCS(bend1.MidPoint, bend2.MidPoint, Q);
                                                if ((ucs.FromACS(TR.Normal.Second)).GetZ() < 0.0)
                                                    ucs = new UCS(bend2.MidPoint, bend1.MidPoint, Q);

                                                quaternion Z = ucs.ToACS(new quaternion(0, 0, 10000.0, .0)) - container.Bends[TR.GetThirdBendNumer()].MidPoint;

                                                using (Solid3d sol = new Solid3d())
                                                {
                                                    sol.CreateExtrudedSolid(REZ, new Vector3d(Z.GetX(), Z.GetY(), Z.GetZ()), new SweepOptions());
                                                    solid.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                                                }

                                                try
                                                {
                                                    REZ.Erase();
                                                }
                                                catch { }
                                            }
                                        }
                                        #endregion

                                        #region third Pick
                                        if (TR.IsSecondBendFictive() && TR.IsThirdBendFictive())
                                        {
                                            Bend bend1 = container.Bends[TR.GetSecondBendNumer()];
                                            Bend bend2 = container.Bends[TR.GetThirdBendNumer()];

                                            //common node
                                            quaternion Q = ((bend1.StartNodeNumer == bend2.StartNodeNumer) || (bend1.StartNodeNumer == bend2.EndNodeNumer)) ?
                                                bend1.Start : bend1.End;


                                            Pair<Polyline, Point3dCollection> ZZ = TR.GetPickCuter(ref container, 2);
                                            using (Polyline REZ = ZZ.First)
                                            {
                                                REZ.Closed = true;

                                                UCS ucs = new UCS(bend1.MidPoint, bend2.MidPoint, Q);
                                                if ((ucs.FromACS(TR.Normal.Second)).GetZ() < 0.0)
                                                    ucs = new UCS(bend2.MidPoint, bend1.MidPoint, Q);

                                                quaternion Z = ucs.ToACS(new quaternion(0, 0, 10000, 0.0)) - container.Bends[TR.GetThirdBendNumer()].MidPoint;

                                                using (Solid3d sol = new Solid3d())
                                                {
                                                    sol.CreateExtrudedSolid(REZ, new Vector3d(Z.GetX(), Z.GetY(), Z.GetZ()), new SweepOptions());
                                                    solid.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                                                }

                                                try
                                                {
                                                    REZ.Erase();
                                                }
                                                catch { }
                                            }
                                        }
                                        #endregion
                                    }
                                    //--------------------

                                    coll.Add(curSpace.AppendEntity(solid));
                                }

                                rez.Erase();
                            }
                        }
                        List<Bend> sortedBends = new List<Bend>();
                        #region podregdam prytite
                        foreach (int Num in POL.Bends_Numers_Array)
                        {
                            if (container.Bends[Num].Fictive == false)
                                sortedBends.Add(container.Bends[Num]);
                        }
                        for (int i = 0; i < sortedBends.Count - 1; i++)
                        {
                            Bend cB = sortedBends[i];
                            for (int j = i + 1; j < sortedBends.Count; j++)
                            {
                                bool b1 = (cB.StartNodeNumer == sortedBends[j].StartNodeNumer) || (cB.StartNodeNumer == sortedBends[j].EndNodeNumer);
                                bool b2 = (cB.EndNodeNumer == sortedBends[j].StartNodeNumer) || (cB.EndNodeNumer == sortedBends[j].EndNodeNumer);
                                if (b1 || b2)
                                {
                                    if (j > i + 1)
                                    {
                                        Bend buff = sortedBends[i + 1];
                                        sortedBends[i + 1] = sortedBends[j];
                                        sortedBends[j] = buff;
                                    }
                                    break;
                                }
                            }
                        }
                        #endregion

                        #region cut solid

                        #region generate
                        for (int k = 0; k < sortedBends.Count; k++)
                        {

                            Bend bend = sortedBends[k];

                            Triangle TR = (POL.Triangles_Numers_Array.IndexOf(bend.FirstTriangleNumer) >= 0) ?
                                    container.Triangles[bend.FirstTriangleNumer] : container.Triangles[bend.SecondTriangleNumer];

                            double offset = (TR.Numer == bend.FirstTriangleNumer) ? bend.FirstTriangleOffset : bend.SecondTriangleOffset;
                            double ExtrudeRatio = (TR.Numer == bend.FirstTriangleNumer) ? bend.explicit_first_cutExtrudeRatio : bend.explicit_second_cutExtrudeRatio;

                            Pair<Polyline, Point3dCollection> zz = bend.GetCutSolid(TR.Normal.First, offset);

                            UCS ucs = bend.GetUCS();
                            if ((ucs.FromACS(TR.Normal.First)).GetZ() < 0.0)
                                ucs = bend.GetUCS_A();

                            double ratio = (ExtrudeRatio < 0) ? ConstantsAndSettings.cutSolidsExtrudeRatio : ExtrudeRatio;
                            quaternion Z = ucs.ToACS(new quaternion(0, 0, 0, (offset != 0)  ? -offset*ratio:-40.0*ratio)) - bend.MidPoint;


                            using (Solid3d sol = new Solid3d())
                            {                                
                                sol.CreateExtrudedSolid(zz.First, new Vector3d(Z.GetX(), Z.GetY(), Z.GetZ()), new SweepOptions());
                                coll2.Add(new Pair<int, ObjectId>(bend.Numer, curSpace.AppendEntity(sol)));
                            }

                            using (Solid3d sol1 = new Solid3d())
                            {
                                sol1.CreateExtrudedSolid(zz.First, new Vector3d(-Z.GetX() * 100.0, -Z.GetY() * 100.0, -Z.GetZ() * 100.0), new SweepOptions());
                                coll2_negative.Add(new Pair<int, ObjectId>(bend.Numer, curSpace.AppendEntity(sol1)));
                            }
                        }
                        #endregion

                        #region truncate
                        using (Transaction acTrans = db.TransactionManager.StartTransaction())
                        {
                            Solid = acTrans.GetObject(coll[0], OpenMode.ForWrite) as Solid3d;
                            for (int i = 1; i < coll.Count; i++)
                            {
                                Solid3d sol = acTrans.GetObject(coll[i], OpenMode.ForWrite) as Solid3d;
                                Solid.BooleanOperation(BooleanOperationType.BoolUnite, sol);
                                sol.Dispose();
                            }

                            for (int k = 0; k < sortedBends.Count; k++)
                            {

                                int pre = k - 1; if (pre < 0) { pre = sortedBends.Count - 1; }
                                int next = k + 1; if (next > sortedBends.Count - 1) { next = 0; }

                                Bend bend = sortedBends[k];
                                Bend bendPRE = sortedBends[pre];
                                Bend bendNEXT = sortedBends[next];

                                Triangle TR = (POL.Triangles_Numers_Array.IndexOf(bend.FirstTriangleNumer) >= 0) ?
                                     container.Triangles[bend.FirstTriangleNumer] : container.Triangles[bend.SecondTriangleNumer];

                                UCS ucs = bend.GetUCS();
                                if ((ucs.FromACS(TR.Normal.First)).GetZ() < 0.0)
                                    ucs = bend.GetUCS_A();

                                Solid3d solid = acTrans.GetObject(coll2[k].Second, OpenMode.ForWrite) as Solid3d;

                                if ((ucs.FromACS(bendPRE.MidPoint)).GetZ() < 0.0)
                                {
                                    Solid3d solc = acTrans.GetObject(coll2_negative[pre].Second, OpenMode.ForWrite) as Solid3d;
                                    Solid3d solc1 = solc.Clone() as Solid3d;
                                    solid.BooleanOperation(BooleanOperationType.BoolSubtract, solc1);
                                }

                                if ((ucs.FromACS(bendNEXT.MidPoint)).GetZ() < 0.0)
                                {
                                    Solid3d solc = acTrans.GetObject(coll2_negative[next].Second, OpenMode.ForWrite) as Solid3d;
                                    Solid3d solc1 = solc.Clone() as Solid3d;
                                    solid.BooleanOperation(BooleanOperationType.BoolSubtract, solc1);
                                }

                                Solid.BooleanOperation(BooleanOperationType.BoolSubtract, solid);
                            }

                            for (int k = 0; k < sortedBends.Count; k++)
                            {
                                try
                                {
                                    Solid3d solc = acTrans.GetObject(coll2_negative[k].Second, OpenMode.ForWrite) as Solid3d;
                                    solc.Erase();
                                    solc.Dispose();
                                }
                                catch { }
                            }

                            acTrans.Commit();
                        }
                        #endregion

                        #endregion
   
                    }
                }
                ed.UpdateScreen();
                return Solid;
            }
            #endregion

            #region old 3d mashine

            public static Solid3d GetPolygonGlassByheightOLD(ref Containers container, WorkClasses.Polygon POL, double height, Transaction trans, BlockTableRecord acBlkTblRec)
            {
                Solid3d rez = new Solid3d();
                try
                {
                    List<Bend> sortedBnends = new List<Bend>();
                    List<PRE_BEND> offsetBends = new List<PRE_BEND>();
                    List<plane> perefierialPlanes = new List<plane>();
                    List<plane> extPlanes = new List<plane>();
                    List<quaternion[]> ripsPeriferial = new List<quaternion[]>();
                    List<quaternion[]> ripsExternal = new List<quaternion[]>();
                    List<quaternion[]> ripsPeriferial_Ex = new List<quaternion[]>();
                    List<quaternion[]> ripsExternal_Ex = new List<quaternion[]>();

                    List<Solid3d> perefierialSolids = new List<Solid3d>();

                    #region podregdam prytite
                    foreach (int Num in POL.Bends_Numers_Array)
                    {
                        sortedBnends.Add(container.Bends[Num]);
                    }
                    for (int i = 0; i < sortedBnends.Count - 1; i++)
                    {
                        Bend cB = sortedBnends[i];
                        for (int j = i + 1; j < sortedBnends.Count; j++)
                        {
                            bool b1 = (cB.StartNodeNumer == sortedBnends[j].StartNodeNumer) || (cB.StartNodeNumer == sortedBnends[j].EndNodeNumer);
                            bool b2 = (cB.EndNodeNumer == sortedBnends[j].StartNodeNumer) || (cB.EndNodeNumer == sortedBnends[j].EndNodeNumer);
                            if (b1 || b2)
                            {
                                if (j > i + 1)
                                {
                                    Bend buff = sortedBnends[i + 1];
                                    sortedBnends[i + 1] = sortedBnends[j];
                                    sortedBnends[j] = buff;
                                }
                                break;
                            }
                        }
                    }
                    for (int i = 0; i < sortedBnends.Count - 1; i++)
                    {
                        bool b1 = (sortedBnends[i].StartNodeNumer == sortedBnends[i + 1].StartNodeNumer) || (sortedBnends[i].StartNodeNumer == sortedBnends[i + 1].EndNodeNumer);
                        if (i == 0)
                        {
                            int nNN = (!b1) ? sortedBnends[i].StartNodeNumer : sortedBnends[i].EndNodeNumer;
                            //sortedNodes.Add(container.Nodes[nNN]);
                        }
                        int nN = (b1) ? sortedBnends[i].StartNodeNumer : sortedBnends[i].EndNodeNumer;
                        // sortedNodes.Add(container.Nodes[nN]);
                    }
                    foreach (Bend bend in sortedBnends)
                    {
                        try
                        {
                            Triangle TR = container.Triangles[bend.FirstTriangleNumer];
                            double offset = bend.FirstTriangleOffset;
                            if (TR.PolygonNumer != POL.GetNumer())
                            {
                                TR = container.Triangles[bend.SecondTriangleNumer];
                                offset = bend.SecondTriangleOffset;
                            }

                            UCS ucs = TR.GetUcsByBends(bend.Numer, ref container.Bends);
                            quaternion s = ucs.FromACS(bend.Start);
                            quaternion e = ucs.FromACS(bend.End);
                            quaternion q = new quaternion(0, 0, offset, 0);
                            PRE_BEND pb = new PRE_BEND(ucs.ToACS(s + q), ucs.ToACS(e + q));
                            offsetBends.Add(pb);
                        }
                        catch
                        {
                            MessageBox.Show(bend.Numer.ToString() + "\n" + "----\n" + bend.SecondTriangleNumer.ToString());
                        }
                    }
                    #endregion

                    if (!POL.IsPlanar(ref container.Triangles))
                    {
                        #region podregdam rawninite
                        foreach (Bend bend in sortedBnends)
                        {
                            Triangle TR = container.Triangles[bend.FirstTriangleNumer];
                            double offset = bend.FirstTriangleOffset;
                            if (TR.PolygonNumer != POL.GetNumer())
                            {
                                TR = container.Triangles[bend.SecondTriangleNumer];
                                offset = bend.SecondTriangleOffset;
                            }
                            UCS ucs = TR.GetUcsByBends(bend.Numer, ref container.Bends);
                            plane pl = new plane(ucs.ToACS(new quaternion(0, 0, offset, 0)),
                                ucs.ToACS(new quaternion(0, 1000, offset, 0)),
                                ucs.ToACS(new quaternion(0, 0, offset, 0)) + TR.Normal.Second - TR.Normal.First);

                            plane pl1 = new plane(ucs.ToACS(new quaternion(0, 0, -1000, 0)),
                                                    ucs.ToACS(new quaternion(0, 1000, -1000, 0)),
                                                     ucs.ToACS(new quaternion(0, 0, -1000, 0)) + TR.Normal.Second - TR.Normal.First);
                            perefierialPlanes.Add(pl);
                            extPlanes.Add(pl1);
                        }

                        #endregion

                        #region base rips for cutsolids
                        for (int k = 0; k < sortedBnends.Count; k++)
                        {
                            int pre = k - 1; if (pre < 0) { pre = sortedBnends.Count - 1; }

                            UCS vUCS = sortedBnends[k].GetUCS();
                            quaternion qu1 = vUCS.FromACS(sortedBnends[pre].Start);
                            quaternion qu2 = vUCS.FromACS(sortedBnends[pre].End);
                            double tD = Math.Abs(qu1.GetZ() - qu2.GetZ());

                            //if (!perefierialPlanes[k].IsParallel(perefierialPlanes[pre]))
                            if (tD > Constants.zero_dist)
                            {
                                ripsPeriferial.Add(perefierialPlanes[k].IntersectWithPlane(perefierialPlanes[pre]));
                                ripsExternal.Add(extPlanes[k].IntersectWithPlane(extPlanes[pre]));
                                ripsPeriferial_Ex.Add(null);
                                ripsExternal_Ex.Add(null);
                            }
                            else
                            {
                                #region parallel planes
                                int nodeN = GetCommonNode(sortedBnends[pre], sortedBnends[k]);
                                quaternion nodePOS = container.Nodes[nodeN].Position;
                                //-------
                                int trN = sortedBnends[k].GetFirstTriangleNumer();
                                double offset = sortedBnends[k].FirstTriangleOffset;
                                if (container.Triangles[trN].PolygonNumer != POL.GetNumer())
                                {
                                    trN = sortedBnends[k].GetSecondTriangleNumer();
                                    offset = sortedBnends[k].SecondTriangleOffset;
                                }

                                UCS ucs = container.Triangles[trN].GetUcsByBends(sortedBnends[k].Numer, ref container.Bends);
                                double length = sortedBnends[k].Length / 2.0;
                                nodePOS = ucs.FromACS(nodePOS);
                                if (nodePOS.GetX() < 0.0) { length *= -1.0; }
                                quaternion q1 = new quaternion(0, length * 1.5, offset, -1000);
                                quaternion q2 = new quaternion(0, length * 1.5, offset, 1000.0);

                                quaternion q11 = new quaternion(0, length * 1.5, -1000.0, -1000);
                                quaternion q22 = new quaternion(0, length * 1.5, -1000.0, 1000.0);

                                quaternion[] list1 = new quaternion[] { ucs.ToACS(q1), ucs.ToACS(q2) };
                                quaternion[] list2 = new quaternion[] { ucs.ToACS(q11), ucs.ToACS(q22) };

                                ripsPeriferial.Add(list1);
                                ripsExternal.Add(list2);
                                //------------------
                                nodePOS = ucs.ToACS(nodePOS);
                                int TRN = sortedBnends[pre].GetFirstTriangleNumer();
                                double offsett = sortedBnends[pre].FirstTriangleOffset;
                                if (container.Triangles[TRN].PolygonNumer != POL.GetNumer())
                                {
                                    TRN = sortedBnends[pre].GetSecondTriangleNumer();
                                    offsett = sortedBnends[pre].SecondTriangleOffset;
                                }

                                UCS UCS = container.Triangles[TRN].GetUcsByBends(sortedBnends[pre].Numer, ref container.Bends);
                                double Length = sortedBnends[pre].Length / 2.0;
                                nodePOS = UCS.FromACS(nodePOS);
                                if (nodePOS.GetX() < 0.0) { Length *= -1.0; }
                                quaternion qq1 = new quaternion(0, Length * 1.5, offset, -1000);
                                quaternion qq2 = new quaternion(0, Length * 1.5, offset, 1000.0);

                                quaternion qq11 = new quaternion(0, Length * 1.5, -1000.0, -1000);
                                quaternion qq22 = new quaternion(0, Length * 1.5, -1000.0, 1000.0);

                                quaternion[] list11 = new quaternion[] { UCS.ToACS(qq1), UCS.ToACS(qq2) };
                                quaternion[] list22 = new quaternion[] { UCS.ToACS(qq11), UCS.ToACS(qq22) };

                                ripsPeriferial_Ex.Add(list11);
                                ripsExternal_Ex.Add(list22);

                                #endregion
                            }
                        }
                        #endregion

                        List<Solid3d> Solids = new List<Solid3d>();
                        foreach (int trN in POL.Triangles_Numers_Array)
                        {
                            #region cutSolids
                            perefierialSolids = new List<Solid3d>();
                            //foreach (WorkClasses.Bend bend in sortedBnends)
                            for (int k = 0; k < sortedBnends.Count; k++)
                            {
                                Bend bend = sortedBnends[k];
                                Triangle TR = container.Triangles[bend.FirstTriangleNumer];
                                double offset = bend.FirstTriangleOffset;
                                if (TR.PolygonNumer != POL.GetNumer())
                                {
                                    TR = container.Triangles[bend.SecondTriangleNumer];
                                    offset = bend.SecondTriangleOffset;
                                }
                                UCS ucs = TR.GetUcsByBends(bend.Numer, ref container.Bends);

                                int pre = k - 1; if (pre < 0) { pre = sortedBnends.Count - 1; }
                                int next = k + 1; if (next > sortedBnends.Count - 1) { next = 0; }

                                plane LOW = new plane(ucs.ToACS(new quaternion(0, 0, 0, -400.0)),
                                         ucs.ToACS(new quaternion(0, 100, 0, -400.0)),
                                         ucs.ToACS(new quaternion(0, 0, 100, -400.0)));

                                plane HIGH = new plane(ucs.ToACS(new quaternion(0, 0, 0, 400.0)),
                                    ucs.ToACS(new quaternion(0, 100, 0, 400.0)),
                                    ucs.ToACS(new quaternion(0, 0, 100, 400.0)));

                                quaternion[] PerifI1 = ripsPeriferial[k];
                                quaternion[] PerifI2 = ripsExternal[k];
                                quaternion[] PerifI11 = ripsPeriferial[next];
                                quaternion[] PerifI22 = ripsExternal[next];
                                if (ripsPeriferial_Ex[next] != null)
                                {
                                    PerifI11 = ripsPeriferial_Ex[next];
                                    PerifI22 = ripsExternal_Ex[next];
                                }

                                quaternion Q1_low = LOW.IntersectWithVector(PerifI1[0], PerifI1[1]);
                                quaternion Q2_low = LOW.IntersectWithVector(PerifI11[0], PerifI11[1]);
                                quaternion Q3_low = LOW.IntersectWithVector(PerifI22[0], PerifI22[1]);
                                quaternion Q4_low = LOW.IntersectWithVector(PerifI2[0], PerifI2[1]);

                                quaternion Q1_high = HIGH.IntersectWithVector(PerifI1[0], PerifI1[1]);
                                quaternion Q2_high = HIGH.IntersectWithVector(PerifI11[0], PerifI11[1]);
                                quaternion Q3_high = HIGH.IntersectWithVector(PerifI22[0], PerifI22[1]);
                                quaternion Q4_high = HIGH.IntersectWithVector(PerifI2[0], PerifI2[1]);


                                quaternion[] listF = new quaternion[] { Q1_low, Q4_low, Q4_high, Q1_high };
                                quaternion[] listR = new quaternion[] { Q2_low, Q3_low, Q3_high, Q2_high };
                                Polyline pF = GetPoly(ref listF);
                                Polyline pR = GetPoly(ref listR);
                                acBlkTblRec.AppendEntity(pF);
                                trans.AddNewlyCreatedDBObject(pF, true);
                                acBlkTblRec.AppendEntity(pR);
                                trans.AddNewlyCreatedDBObject(pR, true);


                                #region solid
                                try
                                {

                                    Entity[] Curves = new Entity[2];
                                    Curves[0] = pF; Curves[1] = pR;

                                    Entity[] gCurves = new Entity[4];
                                    gCurves[0] = new Line((Point3d)Q1_low, (Point3d)Q2_low);
                                    gCurves[1] = new Line((Point3d)Q4_low, (Point3d)Q3_low);
                                    gCurves[2] = new Line((Point3d)Q1_high, (Point3d)Q2_high);
                                    gCurves[3] = new Line((Point3d)Q4_high, (Point3d)Q3_high);


                                    LoftOptions lo = new LoftOptions();
                                    Solid3d acSol3D = new Solid3d();
                                    acSol3D.SetDatabaseDefaults();
                                    acSol3D.CreateLoftedSolid(Curves, gCurves, null, lo);
                                    acBlkTblRec.AppendEntity(acSol3D);
                                    trans.AddNewlyCreatedDBObject(acSol3D, true);

                                    Curves[0].Erase(); Curves[1].Erase();


                                    perefierialSolids.Add(acSol3D);
                                }
                                catch
                                {
                                    pF.Erase(); pR.Erase();
                                }
                                #endregion
                            }
                            #endregion

                            Triangle tr = container.Triangles[trN];

                            #region triangle
                            PRE_TRIANGLE pt = tr.GetPreNodes();
                            if ((object)pt.First != null && (object)pt.Second != null && (object)pt.Third != null)
                            {
                                PRE_TRIANGLE ptt = pt;//container.GetInnererTriangle(tr);
                                Polyline3d acPoly3d = new Polyline3d();
                                acPoly3d.SetDatabaseDefaults();
                                acBlkTblRec.AppendEntity(acPoly3d);
                                trans.AddNewlyCreatedDBObject(acPoly3d, true);

                                PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)ptt.First);
                                acPoly3d.AppendVertex(acPolVer3d);
                                trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                PolylineVertex3d acPolVer3d1 = new PolylineVertex3d((Point3d)ptt.Second);
                                acPoly3d.AppendVertex(acPolVer3d1);
                                trans.AddNewlyCreatedDBObject(acPolVer3d1, true);

                                PolylineVertex3d acPolVer3d2 = new PolylineVertex3d((Point3d)ptt.Third);
                                acPoly3d.AppendVertex(acPolVer3d2);
                                trans.AddNewlyCreatedDBObject(acPolVer3d2, true);


                                PolylineVertex3d acPolVer3d3 = new PolylineVertex3d((Point3d)ptt.First);
                                acPoly3d.AppendVertex(acPolVer3d3);
                                trans.AddNewlyCreatedDBObject(acPolVer3d3, true);

                                quaternion vec = tr.Normal.Second - tr.Normal.First;
                                vec /= vec.abs(); vec *= height;
                                Solid3d acSol3D = new Solid3d();
                                acSol3D.CreateExtrudedSolid(acPoly3d, new Vector3d(vec.GetX(), vec.GetY(), vec.GetZ()), new SweepOptions());

                                acPoly3d.Erase();

                                foreach (Pair<int, bool> pb in tr.Bends)
                                {
                                    if (pb.Second)
                                    {
                                        Bend BEND = container.Bends[pb.First];
                                        //if (!planar)
                                        {
                                            if ((container.IsBendConvex(BEND) > 0) || (container.IsBendConvex(BEND) < 0))
                                            {
                                                quaternion bNo = BEND.GetStrchedNormalByGlassheight(height, tr.Normal.Second - tr.Normal.First);
                                                double half = BEND.Length / 2.0;
                                                UCS UCS = tr.GetUcsByBends(BEND.Numer, ref container.Bends);
                                                quaternion yahoo = UCS.FromACS(BEND.MidPoint + bNo);
                                                double he = Math.Abs(yahoo.GetY());

                                                if ((bNo - vec).abs() > Constants.zero_dist)
                                                {
                                                    Point3dCollection points1 = new Point3dCollection();
                                                    points1.Add((Point3d)BEND.End);
                                                    points1.Add((Point3d)(BEND.End + vec));
                                                    points1.Add((Point3d)(BEND.End + bNo));
                                                    Polyline poly1 = GetPoly(points1, true);
                                                    acBlkTblRec.AppendEntity(poly1);
                                                    trans.AddNewlyCreatedDBObject(poly1, true);

                                                    Point3dCollection points2 = new Point3dCollection();
                                                    points2.Add((Point3d)BEND.Start);
                                                    points2.Add((Point3d)(BEND.Start + vec));
                                                    points2.Add((Point3d)(BEND.Start + bNo));
                                                    Polyline poly2 = GetPoly(points2, true);
                                                    acBlkTblRec.AppendEntity(poly2);
                                                    trans.AddNewlyCreatedDBObject(poly2, true);


                                                    Entity[] Curves = new Entity[2];
                                                    Curves[0] = poly1; Curves[1] = poly2;

                                                    Entity[] gCurves = new Entity[3];
                                                    gCurves[0] = new Line((Point3d)BEND.End, (Point3d)BEND.Start);
                                                    gCurves[1] = new Line((Point3d)(BEND.End + vec), (Point3d)(BEND.Start + vec));
                                                    gCurves[2] = new Line((Point3d)(BEND.End + bNo), (Point3d)(BEND.Start + bNo));

                                                    try
                                                    {
                                                        LoftOptions lo = new LoftOptions();
                                                        Solid3d Sol3D_ = new Solid3d();
                                                        Sol3D_.SetDatabaseDefaults();
                                                        Sol3D_.CreateLoftedSolid(Curves, gCurves, null, lo);
                                                        acBlkTblRec.AppendEntity(Sol3D_);
                                                        trans.AddNewlyCreatedDBObject(Sol3D_, true);

                                                        Curves[0].Erase(); Curves[1].Erase();

                                                        if (container.IsBendConvex(BEND) < 0)
                                                            acSol3D.BooleanOperation(BooleanOperationType.BoolSubtract, Sol3D_);
                                                        else
                                                            acSol3D.BooleanOperation(BooleanOperationType.BoolUnite, Sol3D_);

                                                    }
                                                    catch { Curves[0].Erase(); Curves[1].Erase(); }
                                                }
                                            }
                                        }
                                    }
                                }

                                foreach (Solid3d s in perefierialSolids)
                                {
                                    try
                                    {
                                        acSol3D.BooleanOperation(BooleanOperationType.BoolSubtract, s);
                                        s.Dispose();
                                    }
                                    catch
                                    {
                                        s.Erase();
                                        s.Dispose();
                                    }
                                }
                                Solids.Add(acSol3D);
                            }
                            #endregion

                            for (int i = 1; i < Solids.Count; i++)
                            {
                                try
                                {
                                    Solids[0].BooleanOperation(BooleanOperationType.BoolUnite, Solids[i]);                                    
                                }
                                catch{}
                            }

                            rez = Solids[0];
                        }

                    }//***
                    else
                    {
                        #region planar
                        //if (planar)
                        {
                            Triangle TRi = container.Triangles[POL.Triangles_Numers_Array[0]];
                            UCS ucs = TRi.GetUcsByCentroid1();

                            offsetBends.Add(offsetBends[0]);
                            offsetBends.Add(offsetBends[1]);

                            Polyline3d acPoly3d = new Polyline3d();
                            acBlkTblRec.AppendEntity(acPoly3d);
                            trans.AddNewlyCreatedDBObject(acPoly3d, true);

                            Polyline3d acPoly3D = new Polyline3d();
                            acBlkTblRec.AppendEntity(acPoly3D);
                            trans.AddNewlyCreatedDBObject(acPoly3D, true);

                            List<quaternion> bL1 = new List<quaternion>();
                            List<quaternion> bL2 = new List<quaternion>();
                            for (int i = 0; i < offsetBends.Count - 1; i++)
                            {
                                if (((offsetBends[i].Second - offsetBends[i].First) / (offsetBends[i + 1].Second - offsetBends[i + 1].First)).absV() > Constants.zero_dist)
                                {
                                    line2d line1 = new line2d(ucs.FromACS(offsetBends[i].First), ucs.FromACS(offsetBends[i].Second));
                                    line2d line2 = new line2d(ucs.FromACS(offsetBends[i + 1].First), ucs.FromACS(offsetBends[i + 1].Second));
                                    complex c = line1.IntersectWitch(line2);

                                    quaternion Q1 = ucs.ToACS(new quaternion(0, c.real(), c.imag(), 0));
                                    quaternion Q2 = ucs.ToACS(new quaternion(0, c.real(), c.imag(), height));
                                    bL1.Add(Q1);
                                    bL2.Add(Q2);

                                    PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)Q1);
                                    acPoly3d.AppendVertex(acPolVer3d);
                                    trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                    PolylineVertex3d acPolVer3D = new PolylineVertex3d((Point3d)Q2);
                                    acPoly3D.AppendVertex(acPolVer3D);
                                    trans.AddNewlyCreatedDBObject(acPolVer3D, true);
                                }
                            }

                            if ((bL1[0] - bL1[bL1.Count - 1]).abs() > Constants.zero_dist)
                            {
                                PolylineVertex3d acPolVer3d = new PolylineVertex3d((Point3d)bL1[0]);
                                acPoly3d.AppendVertex(acPolVer3d);
                                trans.AddNewlyCreatedDBObject(acPolVer3d, true);

                                PolylineVertex3d acPolVer3D = new PolylineVertex3d((Point3d)bL2[0]);
                                acPoly3D.AppendVertex(acPolVer3D);
                                trans.AddNewlyCreatedDBObject(acPolVer3D, true);
                            }

                            Entity[] Curves = { (Entity)acPoly3d, (Entity)acPoly3D };
                            Entity[] gCurves = {(Entity)(new Line((Point3d)bL1[0],(Point3d)bL2[0])),
                                               (Entity)(new Line((Point3d)bL1[1],(Point3d)bL2[1])),
                                               (Entity)(new Line((Point3d)bL1[2],(Point3d)bL2[2]))};

                            LoftOptions lo = new LoftOptions();
                            Solid3d acSol3D = new Solid3d();
                            acSol3D.SetDatabaseDefaults();
                            acSol3D.CreateLoftedSolid(Curves, gCurves, null, lo);
                            acBlkTblRec.AppendEntity(acSol3D);
                            trans.AddNewlyCreatedDBObject(acSol3D, true);
                            rez = acSol3D;

                            Curves[0].Erase(); Curves[1].Erase();
                        }
                        #endregion
                    }
                }
                catch
                {
                    rez = null;
                }

                return rez;
            }
            
            #endregion

            //---------

            public static List<Pair<int/*Triangle numer*/, Point3dCollection/*Region Points*/>>
                GetPolygonGlassByheight_Ex(ref Containers container, WorkClasses.Polygon POL, double height, Transaction trans, BlockTableRecord acBlkTblRec, bool show3d = true)
            {
                List<Pair<int, Point3dCollection>> rez = new List<Pair<int, Point3dCollection>>();
                bool planar = POL.IsPlanar(ref container.Triangles);
                if (planar)
                {
                    rez.Add(new Pair<int, Point3dCollection>(-1, KojtoGlassDomes_GetGlassConturByOffsetHeigtForPlanarRegions(ref container, POL, height, show3d)));
                }
                else
                {
                    Solid3d solid = GetPolygonGlassByheight(ref container, POL, height, trans, acBlkTblRec);
                    try
                    {
                        if (show3d)
                        {
                            acBlkTblRec.AppendEntity(solid);
                            trans.AddNewlyCreatedDBObject(solid, true);
                        }
                    }
                    catch { }

                    DBObjectCollection exploded = new DBObjectCollection();
                    solid.Explode(exploded);

                    foreach (DBObject obj in exploded)
                    {
                        Entity ent = (Entity)obj;
                        string type = ent.GetType().ToString();
                        if ((obj != null) && (type.IndexOf("Region") >= 0))
                        {
                            acBlkTblRec.AppendEntity((Entity)obj);
                            trans.AddNewlyCreatedDBObject((Entity)obj, true);
                            var reg = (Region)obj;
                            quaternion qN = new quaternion(0, reg.Normal.X, reg.Normal.Y, reg.Normal.Z);
                            Pair<Region, List<Triangle>> pa =
                                new Pair<Region, List<Triangle>>(reg, new TRIANGLES_ARRAY());
                            foreach (int N in POL.Triangles_Numers_Array)
                            {
                                Triangle TR = container.Triangles[N];
                                UCS tUCS = TR.GetUcsByCentroid1();
                                if ((qN / (TR.Normal.Second - TR.Normal.First)).absV() < Constants.zero_dist)
                                {
                                    Point3dCollection pColl = new Point3dCollection();
                                    IntegerCollection iCol1 = new IntegerCollection();
                                    IntegerCollection iCol2 = new IntegerCollection();
                                    reg.GetGripPoints(pColl, iCol1, iCol2);
                                    quaternion psevdo_cen = new quaternion();

                                    foreach (Point3d p in pColl)
                                    {
                                        psevdo_cen += p;
                                    }
                                    psevdo_cen /= pColl.Count;
                                    psevdo_cen = tUCS.FromACS(psevdo_cen);
                                    double H = Math.Abs(psevdo_cen.GetZ());
                                    psevdo_cen = new quaternion(0, psevdo_cen.GetX(), psevdo_cen.GetY(), 0.0);
                                    psevdo_cen = tUCS.ToACS(psevdo_cen);
                                    if ((psevdo_cen == TR) && (H > height * .5))
                                    {
                                        if (show3d)
                                            pa.Second.Add(TR);
                                        rez.Add(new Pair<int, Point3dCollection>(TR.Numer, pColl));
                                    }
                                }
                            }
                            if (pa.Second.Count == 0) reg.Erase();
                        }
                    }
                    try { solid.Erase(); }
                    catch { }
                }

                return rez;
            }
            //-----------------
            // nalaga se za6toto dawa nqkakwa gre6ka pri polzwane na "GetPolygonGlassByheight_Ex"
            // sled zarwarqne na 4ertga dawa "corupt memory" - opit za pisane v razvalena pamet
            // vig //****
            public static List<Pair<int/*Triangle numer*/, Point3dCollection/*Region Points*/>>
                GetPolygonGlassByheight_ExA(ref Containers container, WorkClasses.Polygon POL, double height, Transaction trans, BlockTableRecord acBlkTblRec)
            {
                List<Pair<int, Point3dCollection>> rez = new List<Pair<int, Point3dCollection>>();

                bool planar = POL.IsPlanar(ref container.Triangles);
                if (!planar)
                {
                    Solid3d solid = GetPolygonGlassByheight(ref container, POL, height, trans, acBlkTblRec);
                    try
                    {
                        acBlkTblRec.AppendEntity(solid);//***
                        trans.AddNewlyCreatedDBObject(solid, true);//***
                    }
                    catch { }

                    DBObjectCollection exploded = new DBObjectCollection();
                    solid.Explode(exploded);

                    foreach (DBObject obj in exploded)
                    {
                        Entity ent = (Entity)obj;
                        string type = ent.GetType().ToString();
                        if ((obj != null) && (type.IndexOf("Region") >= 0))
                        {
                            acBlkTblRec.AppendEntity((Entity)obj);
                            trans.AddNewlyCreatedDBObject((Entity)obj, true);
                            Region reg = (Region)obj;
                            quaternion qN = new quaternion(0, reg.Normal.X, reg.Normal.Y, reg.Normal.Z);
                            Pair<Region, List<Triangle>> pa =
                                new Pair<Region, List<Triangle>>(reg, new TRIANGLES_ARRAY());
                            foreach (int N in POL.Triangles_Numers_Array)
                            {
                                Triangle TR = container.Triangles[N];
                                UCS tUCS = TR.GetUcsByCentroid1();
                                if ((qN / (TR.Normal.Second - TR.Normal.First)).absV() < Constants.zero_dist)
                                {
                                    Point3dCollection pColl = new Point3dCollection();
                                    IntegerCollection iCol1 = new IntegerCollection();
                                    IntegerCollection iCol2 = new IntegerCollection();
                                    reg.GetGripPoints(pColl, iCol1, iCol2);
                                    quaternion psevdo_cen = new quaternion();

                                    foreach (Point3d p in pColl)
                                    {
                                        psevdo_cen += p;
                                    }
                                    psevdo_cen /= pColl.Count;
                                    psevdo_cen = tUCS.FromACS(psevdo_cen);
                                    double H = Math.Abs(psevdo_cen.GetZ());
                                    psevdo_cen = new quaternion(0, psevdo_cen.GetX(), psevdo_cen.GetY(), 0.0);
                                    psevdo_cen = tUCS.ToACS(psevdo_cen);
                                    if ((psevdo_cen == TR) && (H > height * .5))
                                    {
                                        rez.Add(new Pair<int, Point3dCollection>(TR.Numer, pColl));
                                    }
                                }
                            }
                            if (pa.Second.Count == 0) reg.Erase();
                        }

                    }
                    try { solid.Erase(); }
                    catch { }
                }

                return rez;
            }
            //-----------------
            public static void ArrangeTrianglesByAdjoining(ref Containers container, WorkClasses.Polygon POL, ref List<Pair<int, Point3dCollection>> Regions, Transaction trans, BlockTableRecord acBlkTblRec)
            {
                Triangle TR = container.Triangles[Regions[0].First];
                List<int> Bak = new List<int>();
                Bak.Add(TR.Numer);
                #region prepare
                List<quaternion> reg = new List<quaternion>();
                quaternion Cen = new quaternion();
                foreach (Point3d P in Regions[0].Second)
                {
                    quaternion q = new quaternion(0, P.X, P.Y, P.Z);
                    reg.Add(q);
                    Cen += q;
                }
                Cen /= Regions[0].Second.Count;

                quaternion qq = new quaternion();
                for (int i = 1; i < Regions[0].Second.Count; i++)
                {
                    if (Regions[0].Second[0].DistanceTo(Regions[0].Second[i]) > Constants.zero_dist)
                    {
                        qq = Regions[0].Second[i];
                        break;
                    }
                }
                #endregion
                UCS ucs = new UCS(Regions[0].Second[0], qq, Cen);
                reg.Add(reg[0]);

                PRE_BEND_ARRAY thisREG_2d = new PRE_BEND_ARRAY();
                #region first draw
                Polyline pll = new Polyline();
                for (int i = 0; i < reg.Count; i++)
                {
                    quaternion Q = ucs.FromACS(reg[i]);
                    pll.AddVertexAt(i, new Point2d(Q.GetX(), Q.GetY()), 0, 0, 0);
                    if (i < reg.Count - 1)
                        thisREG_2d.Add(new PRE_BEND(reg[i], Q));
                }
                acBlkTblRec.AppendEntity(pll);
                trans.AddNewlyCreatedDBObject(pll, true);
                #endregion

                Triangle[] trARR = container.GetAdjoiningTriangles(TR);

                for (int i = 0; i < trARR.Length; i++)
                {
                    if ((object)trARR[i] != null)
                    {
                        if (POL.Triangles_Numers_Array.IndexOf(trARR[i].Numer) >= 0)
                        {
                            if (Bak.IndexOf(trARR[i].Numer) < 0)
                            {
                                //MessageBox.Show((TR.Numer + 1).ToString() + "\n" + (trARR[i].Numer + 1).ToString());
                                ArrangeTrianglesByAdjoiningA(ref container, POL, ref Regions, trans, acBlkTblRec, TR, trARR[i], thisREG_2d, ref Bak);
                            }
                        }
                    }
                }
            }
            public static void ArrangeTrianglesByAdjoiningA(ref Containers container, WorkClasses.Polygon POL, ref List<Pair<int, Point3dCollection>> Regions, Transaction trans, BlockTableRecord acBlkTblRec,
                Triangle parentTR, Triangle TR, PRE_BEND_ARRAY parentREG_2d, ref List<int> bak)
            {
                if (bak.IndexOf(TR.Numer) >= 0) { return; }
                bak.Add(TR.Numer);


                #region search common bend
                int ambBendNumer = TR.GetFirstBendNumer();
                if ((ambBendNumer != parentTR.GetFirstBendNumer()) && (ambBendNumer != parentTR.GetSecondBendNumer()) && (ambBendNumer != parentTR.GetThirdBendNumer()))
                    ambBendNumer = TR.GetSecondBendNumer();
                if ((ambBendNumer != parentTR.GetFirstBendNumer()) && (ambBendNumer != parentTR.GetSecondBendNumer()) && (ambBendNumer != parentTR.GetThirdBendNumer()))
                    ambBendNumer = TR.GetThirdBendNumer();
                #endregion
                Bend commonBend = container.Bends[ambBendNumer];
                if (!commonBend.IsFictive()) { return; }
                UCS commonBendUCS = commonBend.GetUCS();
                Pair<Pair<int, quaternion>, Pair<int, quaternion>> nearestTwoPoints =
                    GetTwoNearestPoints(ref parentREG_2d, commonBendUCS);

                #region
                quaternion orig = parentREG_2d[nearestTwoPoints.First.First].Second;
                quaternion ORIG = parentREG_2d[nearestTwoPoints.First.First].First;
                quaternion oX = parentREG_2d[nearestTwoPoints.Second.First].Second;
                quaternion OX = parentREG_2d[nearestTwoPoints.Second.First].First;
                quaternion oY = new quaternion();
                foreach (PRE_BEND iQ in parentREG_2d)
                {
                    oY += iQ.Second;
                }
                oY /= parentREG_2d.Count;


                List<quaternion> reg = null;
                quaternion iCen = new quaternion();
                foreach (Pair<int, Point3dCollection> iQ in Regions)
                {
                    if (iQ.First == TR.Numer)
                    {
                        reg = new List<quaternion>();
                        foreach (Point3d iP in iQ.Second)
                        {
                            quaternion cq = new quaternion(0, iP.X, iP.Y, iP.Z);
                            reg.Add(cq);
                            iCen += cq;
                        }
                        break;
                    }
                }
                if (reg == null) { return; }
                iCen /= reg.Count;
                #endregion
                reg.Add(reg[0]);

                UCS ucs_plane = new UCS(orig, oX, oY);
                UCS ucs_3D = new UCS(ORIG, OX, iCen);

                PRE_BEND_ARRAY thisREG_2d = new PRE_BEND_ARRAY();
                Polyline pll = new Polyline();
                for (int i = 0; i < reg.Count; i++)
                {
                    quaternion Q = reg[i];
                    Q = ucs_3D.FromACS(Q);
                    Q = new quaternion(0, Q.GetX(), -Q.GetY(), 0);
                    Q = ucs_plane.ToACS(Q);
                    pll.AddVertexAt(i, new Point2d(Q.GetX(), Q.GetY()), 0, 0, 0);
                    if (i < reg.Count - 1)
                        thisREG_2d.Add(new PRE_BEND(reg[i], Q));
                }
                acBlkTblRec.AppendEntity(pll);
                trans.AddNewlyCreatedDBObject(pll, true);

                Triangle[] trARR = container.GetAdjoiningTriangles(TR);
                for (int i = 0; i < trARR.Length; i++)
                {
                    if ((object)trARR[i] != null)
                    {
                        if (trARR[i].Numer != parentTR.Numer)
                        {
                            if (POL.Triangles_Numers_Array.IndexOf(trARR[i].Numer) >= 0)
                            {
                                if (bak.IndexOf(trARR[i].Numer) < 0)
                                {
                                    //MessageBox.Show((TR.Numer + 1).ToString() + "\n" + (trARR[i].Numer + 1).ToString());
                                    ArrangeTrianglesByAdjoiningA(ref container, POL, ref Regions, trans, acBlkTblRec, TR, trARR[i], thisREG_2d, ref bak);
                                }
                            }
                        }
                    }
                }
            }

            public static Pair<Pair<int, quaternion>, Pair<int, quaternion>> GetTwoNearestPoints(ref PRE_BEND_ARRAY points, UCS ucs)
            {
                int minIndex1 = 0;
                quaternion min1 = points[0].First;
                double dist1 = Math.Abs((ucs.FromACS(min1)).GetZ());

                int minIndex2 = 1;
                quaternion min2 = points[1].First;
                double dist2 = Math.Abs((ucs.FromACS(min2)).GetZ());

                if (dist2 < dist1)
                {
                    double dBuff = dist1; dist1 = dist2; dist2 = dBuff;
                    quaternion qBuff = min1; min1 = min2; min2 = qBuff;
                    int iBuff = minIndex1; minIndex1 = minIndex2; minIndex2 = iBuff;
                }

                for (int i = 2; i < points.Count; i++)
                {
                    double distt = Math.Abs((ucs.FromACS(points[i].First)).GetZ());
                    if (distt < dist1)
                    {
                        min2 = min1;
                        dist2 = dist1;
                        minIndex2 = minIndex1;

                        min1 = points[i].First;
                        dist1 = distt;
                        minIndex1 = i;
                    }
                    else
                        if ((distt < dist2) && ((points[i].First - min1).abs() > Constants.zero_dist))
                        {
                            min2 = points[i].First;
                            dist2 = distt;
                            minIndex2 = i;
                        }

                }

                Pair<int, quaternion> fMin = new Pair<int, quaternion>(minIndex1, ucs.ToACS(min1));
                Pair<int, quaternion> sMin = new Pair<int, quaternion>(minIndex2, ucs.ToACS(min2));

                return new Pair<Pair<int, quaternion>, Pair<int, quaternion>>(fMin, sMin);
            }

            //------
            public static void ArrangeTrianglesByAdjoiningU(ref UCS ucs, ref Containers container, WorkClasses.Polygon POL, ref List<Pair<int, Point3dCollection>> Regions, Transaction trans, BlockTableRecord acBlkTblRec)
            {
                Triangle TR = container.Triangles[Regions[0].First];
                List<int> Bak = new List<int>();
                Bak.Add(TR.Numer);
                #region prepare
                List<quaternion> reg = new List<quaternion>();
                quaternion Cen = new quaternion();
                foreach (Point3d P in Regions[0].Second)
                {
                    quaternion q = new quaternion(0, P.X, P.Y, P.Z);
                    reg.Add(q);
                    Cen += q;
                }
                Cen /= Regions[0].Second.Count;

                quaternion qq = new quaternion();
                for (int i = 1; i < Regions[0].Second.Count; i++)
                {
                    if (Regions[0].Second[0].DistanceTo(Regions[0].Second[i]) > Constants.zero_dist)
                    {
                        qq = Regions[0].Second[i];
                        break;
                    }
                }
                #endregion
                // MathLibKCAD.UCS ucs = new MathLibKCAD.UCS(Regions[0].Second[0], qq, Cen);
                reg.Add(reg[0]);

                PRE_BEND_ARRAY thisREG_2d = new PRE_BEND_ARRAY();
                #region first draw
                Polyline pll = new Polyline();
                for (int i = 0; i < reg.Count; i++)
                {
                    quaternion Q = ucs.FromACS(reg[i]);
                    pll.AddVertexAt(i, new Point2d(Q.GetX(), Q.GetY()), 0, 0, 0);
                    if (i < reg.Count - 1)
                        thisREG_2d.Add(new PRE_BEND(reg[i], Q));
                }
                acBlkTblRec.AppendEntity(pll);
                trans.AddNewlyCreatedDBObject(pll, true);
                #endregion

                Triangle[] trARR = container.GetAdjoiningTriangles(TR);

                for (int i = 0; i < trARR.Length; i++)
                {
                    if ((object)trARR[i] != null)
                    {
                        if (POL.Triangles_Numers_Array.IndexOf(trARR[i].Numer) >= 0)
                        {
                            if (Bak.IndexOf(trARR[i].Numer) < 0)
                            {
                                //MessageBox.Show((TR.Numer + 1).ToString() + "\n" + (trARR[i].Numer + 1).ToString());
                                ArrangeTrianglesByAdjoiningA(ref container, POL, ref Regions, trans, acBlkTblRec, TR, trARR[i], thisREG_2d, ref Bak);
                            }
                        }
                    }
                }
            }

            //--- Bends Cutting --------------

            //http://3dsoft.blob.core.windows.net/kojtocad/images/CutterWall.png

            public static ObjectId GetCutterWall(int bendNumer0/*base bend*/, int bendNumer, quaternion nodePoint, quaternion Q, ref Containers container, double thickness, double ins, double up)
            {
                ObjectId rez = ObjectId.Null;

                Bend bend = container.Bends[bendNumer];
                Bend bend0 = container.Bends[bendNumer0];

                quaternion mid = (bend.Start + bend.End) / 2.0;
                quaternion mid0 = (bend0.Start + bend0.End) / 2.0;

                quaternion qB = mid - nodePoint; qB /= qB.abs(); qB *= 1000; qB += nodePoint;
                quaternion qB0 = mid0 - nodePoint; qB0 /= qB0.abs(); qB0 *= 1000; qB0 += nodePoint;

                quaternion qMID = (qB + qB0) / 2.0;

                double L = (qMID - nodePoint).abs();

                double dist = (qMID - nodePoint).abs();
                if (dist <= Constants.zero_dist)//bends are parallel
                {
                    qMID = bend0.Normal - mid0;
                    qMID *= 1000;
                    qMID += nodePoint;
                }

                UCS UCS = new UCS(nodePoint, qMID, mid0);
                UCS ucs = new UCS((nodePoint + qMID) / 2.0, qMID, UCS.ToACS(new quaternion(0, 0, 0, -100)));
                if ((ucs.FromACS(Q)).GetZ() < 0)
                    ucs = new UCS((nodePoint + qMID) / 2.0, qMID, UCS.ToACS(new quaternion(0, 0, 0, 100)));

                Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                Document acDoc = Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;

                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    // Create a polyline with two segments (3 points)
                    Polyline acPoly = new Polyline();
                    acPoly.SetDatabaseDefaults();
                    acPoly.AddVertexAt(0, new Point2d(-up - L / 2.0, -1000), 0, 0, 0);
                    acPoly.AddVertexAt(1, new Point2d(up + L / 2.0, -1000), 0, 0, 0);
                    acPoly.AddVertexAt(2, new Point2d(up + L / 2.0, 1000), 0, 0, 0);
                    acPoly.AddVertexAt(3, new Point2d(-up - L / 2.0, 1000), 0, 0, 0);
                    acPoly.AddVertexAt(4, new Point2d(-up - L / 2.0, -1000), 0, 0, 0);

                    acPoly.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -thickness + ins)));

                    acBlkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);

                    try
                    {
                        Solid3d acSol3D = new Solid3d();
                        acSol3D.SetDatabaseDefaults();
                        acSol3D.CreateExtrudedSolid(acPoly, new Vector3d(0, 0, thickness), new SweepOptions());

                        acSol3D.TransformBy(mat);

                        acBlkTblRec.AppendEntity(acSol3D);
                        acTrans.AddNewlyCreatedDBObject(acSol3D, true);

                        acPoly.Erase();
                        rez = acSol3D.ObjectId;
                    }
                    catch { }

                    acTrans.Commit();
                }

                return rez;
            }
            public static ObjectId GetCutterWall(int bendNumer0/*base bend*/, int bendNumer, quaternion nodePoint, quaternion node_Normal, quaternion Q, ref Containers container, double thickness, double ins, double up)
            {
                ObjectId rez = ObjectId.Null;

                Bend bend = container.Bends[bendNumer];
                Bend bend0 = container.Bends[bendNumer0];

                quaternion mid = (bend.Start + bend.End) / 2.0;
                quaternion mid0 = (bend0.Start + bend0.End) / 2.0;

                quaternion qB = mid - nodePoint; qB /= qB.abs(); qB *= 1000; qB += nodePoint;
                quaternion qB0 = mid0 - nodePoint; qB0 /= qB0.abs(); qB0 *= 1000; qB0 += nodePoint;

                quaternion qMID = (qB + qB0) / 2.0;

                double L = (qMID - nodePoint).abs();

                double dist = (qMID - nodePoint).abs();
                if (dist <= Constants.zero_dist)//bends are parallel
                {
                    qMID = bend0.Normal - mid0;
                    qMID *= 1000;
                    qMID += nodePoint;
                }

                UCS UCSp = new UCS(nodePoint, qMID, node_Normal);
                quaternion qp = UCSp.FromACS(mid0);
                qp = new quaternion(0, qp.GetX(), 0, qp.GetZ());
                qp = UCSp.ToACS(qp);                

                UCS UCS = new UCS(nodePoint, qMID, qp);
                UCS ucs = new UCS((nodePoint + qMID) / 2.0, qMID, UCS.ToACS(new quaternion(0, 0, 0, -100)));
                if ((ucs.FromACS(Q)).GetZ() < 0)
                    ucs = new UCS((nodePoint + qMID) / 2.0, qMID, UCS.ToACS(new quaternion(0, 0, 0, 100)));

                Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                Document acDoc = Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;

                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    // Create a polyline with two segments (3 points)
                    Polyline acPoly = new Polyline();
                    acPoly.SetDatabaseDefaults();
                    acPoly.AddVertexAt(0, new Point2d(-up - L / 2.0, -1000), 0, 0, 0);
                    acPoly.AddVertexAt(1, new Point2d(up + L / 2.0, -1000), 0, 0, 0);
                    acPoly.AddVertexAt(2, new Point2d(up + L / 2.0, 1000), 0, 0, 0);
                    acPoly.AddVertexAt(3, new Point2d(-up - L / 2.0, 1000), 0, 0, 0);
                    acPoly.AddVertexAt(4, new Point2d(-up - L / 2.0, -1000), 0, 0, 0);

                    acPoly.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, -thickness + ins)));

                    acBlkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);

                    try
                    {
                        Solid3d acSol3D = new Solid3d();
                        acSol3D.SetDatabaseDefaults();
                        acSol3D.CreateExtrudedSolid(acPoly, new Vector3d(0, 0, thickness), new SweepOptions());

                        acSol3D.TransformBy(mat);

                        acBlkTblRec.AppendEntity(acSol3D);
                        acTrans.AddNewlyCreatedDBObject(acSol3D, true);

                        acPoly.Erase();
                        rez = acSol3D.ObjectId;
                    }
                    catch { }

                    acTrans.Commit();
                }

                return rez;
            }
        }

        public class PreBendComparer : IComparer<Pair<quaternion, quaternion>>
        {
            public int Compare(PRE_BEND x, PRE_BEND y)
            {
                int rez = 0;
                double xl = (x.Second - x.First).abs();
                double yl = (y.Second - y.First).abs();

                if (xl == yl) { rez = 0; }
                else
                    if (xl < yl) { rez = -1; }
                    else
                        rez = 1;
                return rez;
            }
        }
        public class BendComparer : IComparer<Bend>
        {
            public int Compare(Bend x, Bend y)
            {
                int rez = 0;
                double xl = x.GetLength();
                double yl = y.GetLength();

                if (xl == yl) { rez = 0; }
                else
                    if (xl < yl) { rez = -1; }
                    else
                        rez = 1;
                return rez;
            }
        }
        public class PreTriangleComparer : IComparer<Triplet<quaternion, quaternion, quaternion>>
        {
            public int Compare(PRE_TRIANGLE x, PRE_TRIANGLE y)
            {
                int rez = 0;
                double xl = (x.Second - x.First).abs() + (x.Second - x.Third).abs() + (x.Third - x.First).abs();
                double yl = (y.Second - y.First).abs() + (y.Second - y.Third).abs() + (y.Third - y.First).abs();

                if (xl == yl) { rez = 0; }
                else
                    if (xl < yl) { rez = -1; }
                    else
                        rez = 1;
                return rez;
            }
        }
        public class ComplexComparerByArgument : IComparer<complex>
        {
            public int Compare(complex x, complex y)
            {
                int rez = 0;
                double xl = x.arg();
                double yl = y.arg();

                if (xl == yl) { rez = 0; }
                else
                    if (xl < yl) { rez = -1; }
                    else
                        rez = 1;
                return rez;
            }
        }

        public class BendComparerByNumer : IComparer<Bend>
        {
            public int Compare(Bend x, Bend y)
            {
                int rez = 0;
                int xl = x.Numer;
                int yl = y.Numer;

                if (xl == yl) { rez = 0; }
                else
                    if (xl < yl) { rez = -1; }
                    else
                        rez = 1;
                return rez;
            }
        }
        public class NodeComparerByNumer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                int rez = 0;
                int xl = x.Numer;
                int yl = y.Numer;

                if (xl == yl) { rez = 0; }
                else
                    if (xl < yl) { rez = -1; }
                    else
                        rez = 1;
                return rez;
            }
        }
        public class TriangleComparerByNumer : IComparer<Triangle>
        {
            public int Compare(Triangle x, Triangle y)
            {
                int rez = 0;
                int xl = x.GetNumer();
                int yl = y.GetNumer();

                if (xl == yl) { rez = 0; }
                else
                    if (xl < yl) { rez = -1; }
                    else
                        rez = 1;
                return rez;
            }
        }
        public class PolygonComparerByNumer : IComparer<WorkClasses.Polygon>
        {
            public int Compare(WorkClasses.Polygon x, WorkClasses.Polygon y)
            {
                int rez = 0;
                int xl = x.GetNumer();
                int yl = y.GetNumer();

                if (xl == yl) { rez = 0; }
                else
                    if (xl < yl) { rez = -1; }
                    else
                        rez = 1;
                return rez;
            }
        }

        public class ContextMenu
        {
            private static ContextMenuExtension cme;

            public static void Attach()
            {

                cme = new ContextMenuExtension();

                MenuItem mi = new MenuItem("Count");

                mi.Click += new EventHandler(OnCount);

                cme.MenuItems.Add(mi);

                RXClass rxc = RXObject.GetClass(typeof(Entity));

                Application.AddObjectContextMenuExtension(rxc, cme);

            }
            public static void Detach()
            {

                RXClass rxc = RXObject.GetClass(typeof(Entity));

                Application.RemoveObjectContextMenuExtension(rxc, cme);

            }
            private static void OnCount(Object o, EventArgs e)
            {

                Document doc = Application.DocumentManager.MdiActiveDocument;

                doc.SendStringToExecute("TEST ", true, false, false);

            }
        }

        public class FormulaEvaluator
        {
            private readonly Regex bracketsRegex = new Regex(@"([a-z]*)\(([^\(\)]+)\)(\^|!?)", RegexOptions.Compiled);
            private readonly Regex cosRegex = new Regex(@"cos(-?\d+.?\d*)", RegexOptions.Compiled);
            private readonly Regex sinRegex = new Regex(@"sin(-?\d+.?\d*)", RegexOptions.Compiled);
            private readonly Regex tanRegex = new Regex(@"tan(-?\d+.?\d*)", RegexOptions.Compiled);
            private readonly Regex cotanRegex = new Regex(@"ctn(-?\d+.?\d*)", RegexOptions.Compiled);
            private readonly Regex powerRegex = new Regex(@"(-?\d+\.?\d*)\^(-?\d+\.?\d*)", RegexOptions.Compiled);
            private readonly Regex multiplyRegex = new Regex(@"(-?\d+\.?\d*)\*(-?\d+\.?\d*)", RegexOptions.Compiled);
            private readonly Regex divideRegex = new Regex(@"(-?\d+\.?\d*)/(-?\d+\.?\d*)", RegexOptions.Compiled);
            private readonly Regex addRegex = new Regex(@"(-?\d+\.?\d*)\+(-?\d+\.?\d*)", RegexOptions.Compiled);
            private readonly Regex subtractRegex = new Regex(@"(-?\d+\.?\d*)-(-?\d+\.?\d*)", RegexOptions.Compiled);

            public double Evaluate(string expr)
            {
                expr = expr.Replace(" ", "").ToLower();

                Match m = bracketsRegex.Match(expr);
                while (m.Success)
                {
                    expr = expr.Replace("(" + m.Groups[2].Value + ")", Solve(m.Groups[2].Value));
                    m = bracketsRegex.Match(expr);
                }

                return Convert.ToDouble(Solve(expr));
            }

            private string Solve(string expr)
            {
                if (expr.IndexOf("cos") != -1) expr = Do(cosRegex, expr, (x) =>
                    Math.Cos(Convert.ToDouble(x.Groups[1].Value) * Math.PI/180.0).ToString());

                if (expr.IndexOf("sin") != -1) expr = Do(sinRegex, expr, (x) =>
                    Math.Sin(Convert.ToDouble(x.Groups[1].Value) * Math.PI / 180.0).ToString());

                if (expr.IndexOf("tan") != -1) expr = Do(tanRegex, expr, (x) =>
                    Math.Tan(Convert.ToDouble(x.Groups[1].Value) * Math.PI / 180.0).ToString());

                if (expr.IndexOf("ctn") != -1) expr = Do(cotanRegex, expr, (x) =>
                    (1.0 / Math.Tan(Convert.ToDouble(x.Groups[1].Value) * Math.PI / 180.0)).ToString());

                if (expr.IndexOf("^") != -1) expr = Do(powerRegex, expr, (x) =>
                    Math.Pow(Convert.ToDouble(x.Groups[1].Value), Convert.ToDouble(x.Groups[2].Value)).ToString());

                if (expr.IndexOf("/") != -1) expr = Do(divideRegex, expr, (x) =>
                    (Convert.ToDouble(x.Groups[1].Value) / Convert.ToDouble(x.Groups[2].Value)).ToString());

                if (expr.IndexOf("*") != -1) expr = Do(multiplyRegex, expr, (x) =>
                    (Convert.ToDouble(x.Groups[1].Value) * Convert.ToDouble(x.Groups[2].Value)).ToString());

                if (expr.IndexOf("+") != -1) expr = Do(addRegex, expr, (x) =>
                    (Convert.ToDouble(x.Groups[1].Value) + Convert.ToDouble(x.Groups[2].Value)).ToString());

                if (expr.IndexOf("-") != -1) expr = Do(subtractRegex, expr, (x) =>
                    (Convert.ToDouble(x.Groups[1].Value) - Convert.ToDouble(x.Groups[2].Value)).ToString());

                return expr;
            }

            private static string Do(Regex regex, string formula, Func<Match, string> func)
            {
                MatchCollection collection = regex.Matches(formula);

                if (collection.Count == 0) return formula;

                for (int i = 0; i < collection.Count; i++)
                    formula = formula.Replace(collection[i].Groups[0].Value, func(collection[i]));

                formula = Do(regex, formula, func);

                return formula;
            }
        }
    }
}

