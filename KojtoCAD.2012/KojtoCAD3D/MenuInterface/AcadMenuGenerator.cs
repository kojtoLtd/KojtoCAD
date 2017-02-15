#if !bcad
using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using Autodesk.AutoCAD.Customization;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace KojtoCAD.KojtoCAD3D.MenuInterface
{
    public class AcadMenuGenerator
    {
        public void GenerateUiFile()
        {
            string mainCui = Application.GetSystemVariable("MENUNAME") + ".cuix";
            string[] split = mainCui.Split(new Char[] { '\\' });
            string mainDir = "";
            for (int i = 0; i < split.Length - 2; i++) { mainDir += split[i]; mainDir += "\\"; }

            string myCuiFile = mainDir + "support\\KOJTO_3D.cuix";
            string myCuiSectionName = "KOJTO_3D";

            try
            {
                if (File.Exists(myCuiFile))
                {
                    File.Delete(myCuiFile);
                }
            }
            catch { }

            CustomizationSection pcs = new CustomizationSection();
            pcs.MenuGroupName = myCuiSectionName;

            #region macros
            MacroGroup mg = new MacroGroup(myCuiSectionName, pcs.MenuGroup);
            MenuMacro mmSettings = new MenuMacro(mg, "KCAD_SETTINGS", "^C^CKCAD_SETTINGS", "ID_KCAD_SETTINGS");
            MenuMacro mm0 = new MenuMacro(mg, "KCAD_NETLOAD", "^C^CNETLOAD", "ID_NETLOAD");
            MenuMacro mm1 = new MenuMacro(mg, "KCAD_CREATEMESH", "^C^CKCAD_CREATEMESH", "ID_KCAD_CREATEMESH");
            MenuMacro mm1C = new MenuMacro(mg, "KCAD_READ_MESH_FROM_CSV", "^C^CKCAD_READ_MESH_FROM_CSV", "ID_KCAD_READ_MESH_FROM_CSV");
            MenuMacro munC = new MenuMacro(mg, "KCAD_DELETE_MESH_DATA", "^C^CKCAD_DELETE_MESH_DATA", "ID_KCAD_DELETE_MESH_DATA");
            MenuMacro mm2 = new MenuMacro(mg, "KCAD_READMESH", "^C^CKCAD_READMESH", "ID_KCAD_READMESH");
            MenuMacro mm3 = new MenuMacro(mg, "KCAD_SAVEMESH", "^C^CKCAD_SAVEMESH", "ID_KCAD_SAVEMESH");
            MenuMacro mm4 = new MenuMacro(mg, "KCAD_DRAW", "^C^CKCAD_DRAW", "ID_KCAD_DRAW");
            MenuMacro mm5 = new MenuMacro(mg, "KCAD_DRAW_NORMAL", "^C^CKCAD_DRAW_NORMALS", "ID_KCAD_DRAW_NORMAL");
            MenuMacro mm5_ = new MenuMacro(mg, "KCAD_DRAW_NODE_UCS", "^C^CKCAD_DRAW_NODE_UCS", "ID_KCAD_DRAW_NODE_UCS");
            MenuMacro mm67 = new MenuMacro(mg, "KCAD_SET_GLASS_DISTANCE_FOR_BEND", "^C^CKCAD_SET_GLASS_DISTANCE_FOR_BEND", "ID_KCAD_SET_GLASS_DISTANCE_FOR_BEND");
            MenuMacro mm6 = new MenuMacro(mg, "KCAD_SET_GLASS_DISTANCE_FOR_ALL_BENDS", "^C^CKCAD_SET_GLASS_DISTANCE_FOR_ALL_BENDS", "ID_KCAD_SET_GLASS_DISTANCE_FOR_ALL_BENDS");
            MenuMacro mm7 = new MenuMacro(mg, "KCAD_SHOW_GLASS", "^C^CKCAD_SHOW_GLASS", "ID_KCAD_SHOW_GLASS");
            MenuMacro mm8 = new MenuMacro(mg, "KCAD_GLASS_EDGES", "^C^CKCAD_GLASS_EDGES_BY_FOLD_BASE", "ID_KCAD_GLASS_EDGES");
            MenuMacro mm8A = new MenuMacro(mg, "KCAD_GLASS_EDGES_", "^C^CKCAD_GLASS_EDGES_BY_UNFOLD_BASE", "ID_KCAD_GLASS_EDGES");
            MenuMacro mm9 = new MenuMacro(mg, "KCAD_REVERSE", "^C^CKCAD_REVERSE", "ID_KCAD_REVERSE");
            MenuMacro mm10 = new MenuMacro(mg, "KCAD_GlASS_CONTURS_BY_LEVEL", "^C^CGET_GlASS_CONTURS_BY_LEVEL", "ID_KCAD_GlASS_CONTURS_BY_LEVEL");
            MenuMacro mm11 = new MenuMacro(mg, "KCAD_GlASS_CONTURS_UNFOLD_BY_LEVEL", "^C^CGET_GlASS_CONTURS_UNFOLD_BY_LEVEL", "ID_KCAD_GlASS_CONTURS_UNFOLD_BY_LEVEL");
            MenuMacro mmAA = new MenuMacro(mg, "KCAD_GET_BY_NUMER", "^C^CKCAD_GET_BY_NUMER", "ID_KCAD_GET_BY_NUMER");
            MenuMacro mmAB = new MenuMacro(mg, "KCAD_GET_BY_SELECTION", "^C^CKCAD_GET_BY_SELECTION", "ID_KCAD_GET_BY_SELECTION");
            MenuMacro mmDIST = new MenuMacro(mg, "KCAD_SET_GLASS_DISTANCE_TO_BEND_BY_NUMER", "^C^CKCAD_SET_GLASS_DISTANCE_TO_BEND_BY_NUMER", "ID_SET_GLASS_DISTANCE_TO_BEND_BY_NUMER");
            MenuMacro mmGlasUnfolds = new MenuMacro(mg, "KCAD_GlASS_UNFOLDS_BY_LEVEL", "^C^CKCAD_GlASS_UNFOLDS_BY_LEVEL", "ID_KCAD_GlASS_UNFOLDS_BY_LEVEL");
            MenuMacro mm08 = new MenuMacro(mg, "KCAD_DRAW_NODES_NORMALS_BY_NOFICTIVE", "^C^CKCAD_DRAW_NODES_NORMALS_BY_NOFICTIVE", "ID_KCAD_DRAW_NODES_NORMALS_BY_NOFICTIVE");
            MenuMacro mm09 = new MenuMacro(mg, "KCAD_CHANGE_SELECTED_BENDS_NORMALS", "^C^CKCAD_CHANGE_SELECTED_BENDS_NORMALS", "ID_KCAD_CHANGE_SELECTED_BENDS_NORMALS");
            MenuMacro mm091 = new MenuMacro(mg, "KCAD_SET_EXPLICIT_NODE_NORMAL_BY_POSITION", "^C^CKCAD_SET_EXPLICIT_NODE_NORMAL_BY_POSITION", "ID_KCAD_SET_EXPLICIT_NODE_NORMAL_BY_POSITION");

            MenuMacro mmPrf1 = new MenuMacro(mg, "KCAD_READ_PROFILE_DATA", "^C^CKCAD_DRAW_PROFILE_SOLID", "ID_KCAD_KCAD_DRAW_PROFILE_SOLID");
            MenuMacro mmPrf1_ = new MenuMacro(mg, "KCAD_READ_PROFILE_DATA_A", "^C^CKCAD_DRAW_PROFILE_SOLID_A", "ID_KCAD_KCAD_DRAW_PROFILE_SOLID_A");
            MenuMacro mmPrf1__ = new MenuMacro(mg, "KCAD_ADD_PROFILE_POINT_TO_FILE", "^C^CKCAD_ADD_PROFILE_POINT_TO_FILE", "ID_KCAD_ADD_PROFILE_POINT_TO_FILE");
            MenuMacro mmPrf2 = new MenuMacro(mg, "KCAD_PREPARE_PROFILE_UNFOLD", "^C^CKCAD_PREPARE_PROFILE_UNFOLD", "ID_KCAD_PREPARE_PROFILE_UNFOLD");


            MenuMacro mmCAM_CNC = new MenuMacro(mg, "KCAD_NODE_TO_CNC", "^C^CKCAD_NODE_TO_CNC", "ID_KCAD_NODE_TO_CNC");
            MenuMacro mmCAM_CNC1 = new MenuMacro(mg, "KCAD_NODE_TO_CSV", "^C^CKCAD_NODE_TO_CSV", "ID_KCAD_NODE_TO_CSV");
            MenuMacro mmCAM_CN1 = new MenuMacro(mg, "KCAD_POLYGONS_CSV", "^C^CKCAD_POLYGONS_CSV", "ID_KCAD_POLIGONS_CSV");

            MenuMacro mmPlacement_3D_0 = new MenuMacro(mg, "KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION", "^C^CKCAD_PLACEMENT_OF_NODE_3D_IN_POSITION", "ID_KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION");
            MenuMacro mmPlacement_3D_00 = new MenuMacro(mg, "KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_DELETE", "^C^CKCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_DELETE", "ID_KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_DELETE");
            MenuMacro mmPlacement_3D_000 = new MenuMacro(mg, "KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_HIDE", "^C^CKCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_HIDE", "ID_KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_HIDE");
            MenuMacro mmPlacement_3D_0000 = new MenuMacro(mg, "KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_SHOW", "^C^CKCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_SHOW", "ID_KCAD_PLACEMENT_OF_NODE_3D_IN_POSITION_SHOW");
            MenuMacro mmPlacement_3D_1 = new MenuMacro(mg, "PLACEMENT_OF_BENDS_NOZZLE_BLOCKS_IN_NODES", "^C^CKCAD_PLACEMENT_OF_BENDS_NOZZLE_BLOCKS_IN_NODES", "ID_KCAD_PLACEMENT_OF_BENDS_NOZZLE_BLOCKS_IN_NODES");
            MenuMacro mmPlacement_3D_100 = new MenuMacro(mg, "KCAD_PLACEMENT_OF_BENDS_NOZZLE_3D_IN_POSITION_HIDE", "^C^CKCAD_PLACEMENT_OF_BENDS_NOZZLE_3D_IN_POSITION_HIDE", "ID_KCAD_PLACEMENT_OF_BENDS_NOZZLE_3D_IN_POSITION_HIDE");
            MenuMacro mmPlacement_3D_1000 = new MenuMacro(mg, "KCAD_PLACEMENT_OF_BENDS_NOZZLE_3D_IN_POSITION_SHOW", "^C^CKCAD_PLACEMENT_OF_BENDS_NOZZLE_3D_IN_POSITION_SHOW", "ID_KCAD_PLACEMENT_OF_BENDS_NOZZLE_3D_IN_POSITION_SHOW");
            MenuMacro mmPlacement_3D_10 = new MenuMacro(mg, "PLACEMENT_OF_BEND_NOZZLE_3D_DELETE", "^C^CKCAD_PLACEMENT_OF_BEND_NOZZLE_3D_DELETE", "ID_KCAD_PLACEMENT_OF_BEND_NOZZLE_3D_DELETE");
            MenuMacro mmPlacement_3D_2 = new MenuMacro(mg, "KCAD_PLACEMENT_BENDS_3D", "^C^CKCAD_PLACEMENT_BENDS_3D", "ID_KCAD_PLACEMENT_BENDS_3D");
            MenuMacro mmPlacement_3D_3 = new MenuMacro(mg, "KCAD_PLACEMENT_BEND_3D_DELETE", "^C^CKCAD_PLACEMENT_BEND_3D_DELETE", "ID_KCAD_PLACEMENT_BEND_3D_DELETE");
            MenuMacro mmPlacement_3D_4 = new MenuMacro(mg, "KCAD_PLACEMENT_OF_BENDS_3D_IN_POSITION_HIDE", "^C^CKCAD_PLACEMENT_OF_BENDS_3D_IN_POSITION_HIDE", "ID_KCAD_PLACEMENT_OF_BENDS_3D_IN_POSITION_HIDE");
            MenuMacro mmPlacement_3D_5 = new MenuMacro(mg, "KCAD_PLACEMENT_OF_BENDS_3D_IN_POSITION_SHOW", "^C^CKCAD_PLACEMENT_OF_BENDS_3D_IN_POSITION_SHOW", "ID_KCAD_PLACEMENT_OF_BENDS_3D_IN_POSITION_SHOW");
            MenuMacro mmCAM_Check = new MenuMacro(mg, "KCAD_CALC_MIN_CAM_RADIUS", "^C^CKCAD_CALC_MIN_CAM_RADIUS", "ID_KCAD_CALC_MIN_CAM_RADIUS");

            MenuMacro mmFixing = new MenuMacro(mg, "KCAD_ADD_FIXING_ELEMENTS", "^C^CKCAD_ADD_FIXING_ELEMENTS", "ID_KCAD_ADD_FIXING_ELEMENTS");
            MenuMacro mmRB = new MenuMacro(mg, "KCAD_RESTORE_BENDS_NORMALS", "^C^CKCAD_RESTORE_BENDS_NORMALS", "ID_KCAD_RESTORE_BENDS_NORMALS");
            MenuMacro mmRN = new MenuMacro(mg, "KCAD_RESTORE_NODES_NORMALS", "^C^CKCAD_RESTORE_NODES_NORMALS", "ID_KCAD_RESTORE_NODES_NORMALS");
            MenuMacro mmTN = new MenuMacro(mg, "KCAD_SET_ALL_NODES_NORMALS_EXPLICIT", "^C^CKCAD_SET_ALL_NODES_NORMALS_EXPLICIT", "ID_KCAD_SET_ALL_NODES_NORMALS_EXPLICIT");

            MenuMacro mmGlass_DOUBLE1 = new MenuMacro(mg, "KCAD_SHOW_DOUBLE_GLAS", "^C^CKCAD_SHOW_DOUBLE_GLAS", "ID_KCAD_SHOW_DOUBLE_GLAS");
            MenuMacro mmGlass_DOUBLE2 = new MenuMacro(mg, "KCAD_DOUBLE_GlASS_UNFOLDS_TRIANGLE", "^C^CKCAD_DOUBLE_GlASS_UNFOLDS_TRIANGLE", "ID_KCAD_DOUBLE_GlASS_UNFOLDS_TRIANGLE");
            MenuMacro mmGlass_DOUBLE3 = new MenuMacro(mg, "KCAD_BENDS_TO_SEPARATE_DRAWINGS", "^C^CKCAD_BENDS_TO_SEPARATE_DRAWINGS", "ID_KCAD_BENDS_TO_SEPARATE_DRAWINGS");

            MenuMacro mmRCN = new MenuMacro(mg, "KCAD_PLACEMENT_BENDS_3D_BY_NORMALS", "^C^CKCAD_PLACEMENT_BENDS_3D_BY_NORMALS", "ID_KCAD_PLACEMENT_BENDS_3D_BY_NORMALS");
            MenuMacro mmRPN = new MenuMacro(mg, "KCAD_BEND_SECTION_TO_SEPARATE_DRAWINGS", "^C^CKCAD_BEND_SECTION_TO_SEPARATE_DRAWINGS", "ID_KCAD_BEND_SECTION_TO_SEPARATE_DRAWINGS");
            MenuMacro mmDN = new MenuMacro(mg, "KCAD_DRAW_NUMERS", "^C^CKCAD_DRAW_NUMERS", "ID_KCAD_DRAW_NUMERS");

            MenuMacro mmEN = new MenuMacro(mg, "KCAD_DRAW_SECOND_MESH", "^C^CKCAD_DRAW_SECOND_MESH", "ID_KCAD_DRAW_SECOND_MESH");

            MenuMacro mmRES = new MenuMacro(mg, "KCAD_RESTORE_NODE_NORMAL_BY_SELECTION", "^C^CKCAD_RESTORE_NODE_NORMAL_BY_SELECTION", "ID_KCAD_RESTORE_NODE_NORMAL_BY_SELECTION");
            MenuMacro mmCH = new MenuMacro(mg, "KCAD_KCAD_CHANGE_EXPLICIT_NORMAL_LENGTH_SELECTION", "^C^CKCAD_CHANGE_EXPLICIT_NORMAL_LENGTH_SELECTION", "ID_KCAD_CHANGE_EXPLICIT_NORMAL_LENGTH_SELECTION");

            MenuMacro mmS = new MenuMacro(mg, "KCAD_KCAD_ATTACHING_A_SOLID3D_TO_BEND", "^C^CKCAD_ATTACHING_A_SOLID3D_TO_BEND", "ID_KCAD_ATTACHING_A_SOLID3D_TO_BEND");

            MenuMacro mmS1 = new MenuMacro(mg, "KCAD_KCAD_RESTORE_BEND_NORMAL", "^C^CKCAD_RESTORE_BEND_NORMAL", "ID_KCAD_RESTORE_BEND_NORMAL");

            MenuMacro mmS2 = new MenuMacro(mg, "KCAD_KCAD_EXPROF", "^C^CKCAD_EXPROF", "ID_KCAD_EXPROF");
            MenuMacro mmS3 = new MenuMacro(mg, "KCAD_EXTRIM", "^C^CKCAD_EXTRIM", "ID_KCAD_EXTRIM");

            MenuMacro mmS4 = new MenuMacro(mg, "KCAD_GlASS_POLYGON_UNFOLDS_BY_LEVEL", "^C^CKCAD_GlASS_POLYGON_UNFOLDS_BY_LEVEL", "ID_KCAD_GlASS_POLYGON_UNFOLDS_BY_LEVEL");
            MenuMacro mmS5 = new MenuMacro(mg, "KCAD_GlASS_TRIANGLE_UNFOLDS_BY_LEVEL", "^C^CKCAD_GlASS_TRIANGLE_UNFOLDS_BY_LEVEL", "ID_KCAD_GlASS_TRIANGLE_UNFOLDS_BY_LEVEL");

            MenuMacro mm0_help = new MenuMacro(mg, "KCAD_READMESH_GET_HELP", "^C^CKCAD_READMESH_GET_HELP", "ID_KCAD_READMESH_GET_HELP");
            MenuMacro mm11_help = new MenuMacro(mg, "KCAD_SAVEMESH_GET_HELP", "^C^CKCAD_SAVEMESH_GET_HELP", "ID_KCAD_SAVEMESH_GET_HELP");
            MenuMacro mm2_help = new MenuMacro(mg, "KCAD_DRAW_NODES_NORMALS_BY_NOFICTIVE_HELP", "^C^CKCAD_DRAW_NODES_NORMALS_BY_NOFICTIVE_HELP", "ID_KCAD_DRAW_NODES_NORMALS_BY_NOFICTIVE_HELP");

            MenuMacro mmHELP = new MenuMacro(mg, "KCAD_HELP", "^C^CKCAD_HELP", "ID_KCAD_HELP");
            // MenuMacro mmCCAM = new MenuMacro(mg, "CALC MIN CAM RADIUS", "^C^CKCAD_CALC_MIN_CAM_RADIUS", "ID_KCAD_CALC_MIN_CAM_RADIUS");

            MenuMacro mmHEeP = new MenuMacro(mg, "KCAD_CUTTING_BENDS_IN_NODES", "^C^CKCAD_CUTTING_BENDS_IN_NODES", "ID_KCAD_CUTTING_BENDS_IN_NODES");

            MenuMacro mm0CSV = new MenuMacro(mg, "KCAD_READ_NAMED_POINTS_FROM_CSV", "^C^CKCAD_READ_NAMED_POINTS_FROM_CSV", "ID_KCAD_READ_NAMED_POINTS_FROM_CSV");
            MenuMacro mm1CSV = new MenuMacro(mg, "KCAD_NAMED_POINTS_DRAW", "^C^CKCAD_NAMED_POINTS_DRAW", "ID_KCAD_NAMED_POINTS_DRAW");

            MenuMacro mmGHG = new MenuMacro(mg, "HIDE GLASS", "^C^CKCAD_HIDE_GLASS", "ID_KCAD_HIDE_GLASS");
            MenuMacro mmGSHG = new MenuMacro(mg, "SHOW HIDEN GLASS", "^C^CKCAD_SHOW_HIDEN_GLASS", "ID_KCAD_SHOW_HIDEN_GLASS");

            MenuMacro mmdGHG = new MenuMacro(mg, "HIDE DOUBLE GLASS", "^C^CKCAD_HIDE_DOUBLE_GLASS", "ID_KCAD_HIDE_DOUBLE_GLASS");
            MenuMacro mmdGSHG = new MenuMacro(mg, "SHOW HIDEN DOUBLE GLASS", "^C^CKCAD_SHOW_HIDEN_DOUBLE_GLASS", "ID_KCAD_SHOW_HIDEN_DOUBLE_GLASS");

            MenuMacro mmCG = new MenuMacro(mg, "CENTER of Gravity", "^C^CKCAD_GET_REAL_CENTROID", "ID_KCAD_GET_REAL_CENTROID");

            MenuMacro mmGCC = new MenuMacro(mg, "SPOTS from glass on the triangles - CENTER of Gravity", "^C^CKCAD_GET_GLASS_CONTURS_CENTROID", "ID_KCAD_GET_GLASS_CONTURS_CENTROID");
            MenuMacro mmTBC = new MenuMacro(mg, "TEORETICAL MESH CENTROID", "^C^CKCAD_GET_TEORETICAL_BENDS_CENTROID", "ID_KCAD_GET_TEORETICAL_BENDS_CENTROID");

            MenuMacro mmCPS = new MenuMacro(mg, "SET_CUT_PROFILE_PARAM_GLOBAL", "^C^CKCAD_SET_CUT_SOLID_LK", "ID_KCAD_SET_CUT_SOLID_LK");
            MenuMacro mmCPS1 = new MenuMacro(mg, "SET_CUT_PROFILE_PARAM_GLOBAL", "^C^CKCAD_SET_CUT_SOLID_TH", "ID_KCAD_SET_CUT_SOLID_TH");
            MenuMacro mmCPS3 = new MenuMacro(mg, "SET_CUT_SOLID_ER_GLOBAL", "^C^CKCAD_SET_CUT_SOLID_ER", "ID_KCAD_SET_CUT_SOLID_ER");
            MenuMacro mmCPS2 = new MenuMacro(mg, "SET_CUT_PROFILE_PARAM_BY_BEND_NUMER", "^C^CKCAD_SET_BEND_EXPLICIT_PARAMETERS", "ID_KCAD_SET_BEND_EXPLICIT_PARAMETERS");

            MenuMacro mmCPS4 = new MenuMacro(mg, "TRIMING_VERTICES_VARIANT", "^C^CKCAD_CHANGE_PICK_TRIM_VARIANT", "ID_KCAD_CHANGE_PICK_TRIM_VARIANT");

            MenuMacro mmCPS5 = new MenuMacro(mg, "ERASE_GLASS_3D", "^C^CKCAD_PLACEMENT_3D_GLASS_DELETE", "ID_KCAD_PLACEMENT_3D_GLASS_DELETE");

            MenuMacro mmERS5 = new MenuMacro(mg, "EX_RATIO_BY_BEND", "^C^CKCAD_EX_RATIO_BY_BEND", "ID_KCAD_EX_RATIO_BY_BEND");
            MenuMacro mmERS4 = new MenuMacro(mg, "EXPLICIT_CUTTING_METHOD", "^C^CKCAD_EXPLICIT_CUTTING_METHOD_FOR_ENDS_IN_NODE", "ID_KCAD_EXPLICIT_CUTTING_METHOD_FOR_ENDS_IN_NODE");
            MenuMacro mmER5S5 = new MenuMacro(mg, "CLEAR_EXPLICIT_CUTTING_METHOD", "^C^CKCAD_CLEAR_EXPLICIT_CUTTING_METHOD_FOR_ALL", "ID_KCAD_CLEAR_EXPLICIT_CUTTING_METHOD_FOR_ALL");

            MenuMacro mmERP5 = new MenuMacro(mg, "PROJECT_POINT_TO_PLANE", "^C^CKCAD_PROJECT_POINT_TO_PLANE", "ID_KCAD_PROJECT_POINT_TO_PLANE");

            MenuMacro mmERT5 = new MenuMacro(mg, "ANGLE BETWEEN TRIANGLES", "^C^CKCAD_ANGLE_BETWEEN_TRIANGLES", "ID_KCAD_ANGLE_BETWEEN_TRIANGLES");

            MenuMacro mmMDN = new MenuMacro(mg, "MINIMUMU DISTANCE TO NODE", "^C^CKCAD_NEAREST_FROM_NODES", "ID_KCAD_NEAREST_FROM_NODES");
            MenuMacro mmMDB = new MenuMacro(mg, "MINIMUMU DISTANCE TO BEND MID", "^C^CKCAD_NEAREST_BEND_MIDPOINT", "ID_KCAD_NEAREST_BEND_MIDPOINT");
            MenuMacro mmMDT = new MenuMacro(mg, "MINIMUMU DISTANCE TO TRIANGLE MID", "^C^CKCAD_NEAREST_TRIANGLE_MIDPOINT", "ID_KCAD_NEAREST_TRIANGLE_MIDPOINT");

            MenuMacro mmRSM1 = new MenuMacro(mg, "READ MESH FROM EXTERNAL", "^C^CKCAD_READ_CONTAINER_FROM_EXTERNAL_FILE", "ID_KCAD_READ_CONTAINER_FROM_EXTERNAL_FILE");
            MenuMacro mmRSM2 = new MenuMacro(mg, "SAVE MESH TO EXTERNAL", "^C^CKCAD_SAVE_CONTAINER_IN_EXTERNAL_FILE", "ID_KCAD_SAVE_CONTAINER_IN_EXTERNAL_FILE");

            MenuMacro mmDSN = new MenuMacro(mg, "CREATE NEW MESH FROM CURRENT THROUGH END OF NORMALS", "^C^CKCAD_SMB", "ID_KCAD_NEAREST_FROM_NODES");

            MenuMacro mmMDBS = new MenuMacro(mg, "REURN MINIMAL DISTANCE BETWEEN SOLIDS", "^C^CKCAD_MIN_DISTANCE_BETWEEN_SOLID3D", "ID_KCAD_MIN_DISTANCE_BETWEEN_SOLID3D");

            MenuMacro mmITP = new MenuMacro(mg, "INTERSECTION LINE BETWEE TWO PLANES", "^C^CKCAD_INTERSECTION_LINE_BETWEEN_TWO_PLANES", "ID_KCAD_INTERSECTION_LINE_BETWEEN_TWO_PLANES");

            MenuMacro mmCALC = new MenuMacro(mg, "CALCULATE FORMULA", "^C^CKCAD_CALCULATE_FORMULA", "ID_KCAD_CALCULATE_FORMULA");
            #endregion;

            StringCollection sc = new StringCollection();
            sc.Add("POP15");

            PopMenu pm = new PopMenu(myCuiSectionName, sc, "ID_MyPop1", pcs.MenuGroup);
            PopMenuItem pmi000 = new PopMenuItem(mmCALC, "CALCULATE FORMULA", pm, -1);
            PopMenuItem pmi_000 = new PopMenuItem(pm, -1);
            PopMenuItem pmi0 = new PopMenuItem(mm0, "NETLOAD", pm, -1);
            PopMenuItem pmi_h0 = new PopMenuItem(pm, -1);
            PopMenuItem pmiHELP = new PopMenuItem(mmHELP, "HELP", pm, -1);
            PopMenuItem pmi_0 = new PopMenuItem(pm, -1);
            PopMenuItem pmiSettings = new PopMenuItem(mmSettings, "SETTINGS", pm, -1);


            PopMenuItem pmi_sie = new PopMenuItem(pm, -1);
            #region mesh
            StringCollection scMESH = new StringCollection();
            PopMenu pmMESH = new PopMenu("MESH", scMESH, "ID_SubscMESH", pcs.MenuGroup);
            PopMenuItem pmi1 = new PopMenuItem(mm1, "CREATE  MESH", pmMESH, -1);
            PopMenuItem pmi1C = new PopMenuItem(mm1C, "READ MESH FROM CSV file", pmMESH, -1);
            PopMenuItem pmi_se = new PopMenuItem(pmMESH, -1);
            PopMenuItem pmi2 = new PopMenuItem(mm2, "READ  MESH from this DWG (from dictionary)", pmMESH, -1);
            PopMenuItem pmij2 = new PopMenuItem(mmRSM1, "READ  MESH from External TXT file", pmMESH, -1);
            PopMenuItem pmi2_help = new PopMenuItem(mm0_help, "Read mesh HELP topic", pmMESH, -1);
            PopMenuItem pmi_se_help = new PopMenuItem(pmMESH, -1);
            PopMenuItem pmi4 = new PopMenuItem(mm3, "SAVE  MESH in this DWG (in dictionary)", pmMESH, -1);
            PopMenuItem pmij4 = new PopMenuItem(mmRSM2, "SAVE  MESH to External TXT File", pmMESH, -1);
            PopMenuItem pmi3_help = new PopMenuItem(mm11_help, "Save mesh HELP topic", pmMESH, -1);
            PopMenuItem pmi_se_ = new PopMenuItem(pmMESH, -1);
            PopMenuItem pmdd_help = new PopMenuItem(munC, "Delete Mesh Data from Dictionary", pmMESH, -1);
            PopMenuItem pmi_dd_help = new PopMenuItem(pmMESH, -1);
            PopMenuItem pmu3_help = new PopMenuItem(mmTBC, "Teoretical mesh (No fictive Bends) - Center of Gravity", pmMESH, -1);
            PopMenuItem pmi_ge_ = new PopMenuItem(pmMESH, -1);
            PopMenuItem pmi5 = new PopMenuItem(mm4, "DRAW", pmMESH, -1);
            PopMenuItem pmi6 = new PopMenuItem(mm5, "DRAW  NORMALS", pmMESH, -1);
            PopMenuItem pmi_7 = new PopMenuItem(mmEN, "LINK the ENDS of NORMALS in the NODES", pmMESH, -1);
            PopMenuItem pmi_8 = new PopMenuItem(mmDSN, "CREATE NEW MESH FROM CURRENT THROUGH END OF NORMALS", pmMESH, -1);
            PopMenuItem pmi_ise_ = new PopMenuItem(pmMESH, -1);
            PopMenuItem pmi06 = new PopMenuItem(mm08, "DRAW  NODE NORMALS BY NoFictive Bends", pmMESH, -1);
            PopMenuItem pmi06_help = new PopMenuItem(mm2_help, "HELP", pmMESH, -1);
            PopMenuItem pmi_siie_ = new PopMenuItem(pmMESH, -1);
            PopMenuItem pmi07_ = new PopMenuItem(mm5_, "DRAW  NODE UCS BY NoFictive Bends", pmMESH, -1);
            PopMenuItem pmi07i_ = new PopMenuItem(mmDN, "DRAW  NUMERS", pmMESH, -1);
            PopMenuItem pmi_se__ = new PopMenuItem(pmMESH, -1);
            PopMenuItem pmui = new PopMenuItem(mmAA, "GET_BY_NUMER", pmMESH, -1);
            PopMenuItem pmuii = new PopMenuItem(mmAB, "GET_BY_SELECTION", pmMESH, -1);
            PopMenuItem pmSGff2 = new PopMenuItem(mmERT5, "Angle between the NORMALS of the adjacent Triangles", pmMESH, -1);
            PopMenuItem pmi_gue_ = new PopMenuItem(pmMESH, -1);

            StringCollection scMESH_MINIMUM_DISTANCES = new StringCollection();
            PopMenu pmMESH_MINIMUM_DISTANCES = new PopMenu("Minimum Distance to ..", scMESH_MINIMUM_DISTANCES, "ID_MINDIST", pcs.MenuGroup);
            PopMenuItem pmMD1 = new PopMenuItem(mmMDN, "to Node", pmMESH_MINIMUM_DISTANCES, -1);
            PopMenuItem pmMD2 = new PopMenuItem(mmMDB, "to Bend Mid Point", pmMESH_MINIMUM_DISTANCES, -1);
            PopMenuItem pmMD3 = new PopMenuItem(mmMDT, "to Triangle Centroid", pmMESH_MINIMUM_DISTANCES, -1);
            PopMenuRef pmMESH_MINIMUM_DISTANCES_Ref = new PopMenuRef(pmMESH_MINIMUM_DISTANCES, pmMESH, -1);
            pmMESH.PopMenuItems.Add(pmMESH_MINIMUM_DISTANCES_Ref);

            PopMenuItem pmi_seo__ = new PopMenuItem(pmMESH, -1);

            StringCollection scMESH_MODIFY_NORMALS = new StringCollection();
            PopMenu pmMESH_MODIFY_NORMALS = new PopMenu("MODIFY NORMALS", scMESH_MODIFY_NORMALS, "ID_SubscMESH", pcs.MenuGroup);
            PopMenuItem pmi10 = new PopMenuItem(mm9, "REVERSE NORMALS", pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmt_seo__ = new PopMenuItem(pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmi010 = new PopMenuItem(mm09, "SET EXPLICIT BENDS NORMALS BY DIRECTION", pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmr010 = new PopMenuItem(mmRB, "RESTORE BENDS NORMALS", pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmi0100 = new PopMenuItem(mmS1, "RESTORE BEND NORMAL", pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmt_seo_ = new PopMenuItem(pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmi011 = new PopMenuItem(mm091, "SET EXPLICIT NODE NORMAL", pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmi012 = new PopMenuItem(mmTN, "SET ALL NOES NORMALS EXPLICIT (by bends midpoints centroid)", pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmr011 = new PopMenuItem(mmRN, "RESTORE NODE NORMALS", pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmr01_1 = new PopMenuItem(mmRES, "RESTORE NODE NORMAL BY SELECTION", pmMESH_MODIFY_NORMALS, -1);
            PopMenuItem pmr01_2 = new PopMenuItem(mmCH, "CHENGE EXPLICIT NORMAL LENGTH SELECTION", pmMESH_MODIFY_NORMALS, -1);
            //PopMenuItem pmt_seo = new PopMenuItem(pmMESH_MODIFY_NORMALS, -1);
            PopMenuRef pmMESH__MODIFY_NORMALS_Ref = new PopMenuRef(pmMESH_MODIFY_NORMALS, pmMESH, -1);
            pmMESH.PopMenuItems.Add(pmMESH__MODIFY_NORMALS_Ref);
            PopMenuRef pmMESH_Ref = new PopMenuRef(pmMESH, pm, -1);
            pm.PopMenuItems.Add(pmMESH_Ref);
            #endregion

            PopMenuItem pmi_seu__ = new PopMenuItem(pm, -1);
            #region glass
            StringCollection scGLASS = new StringCollection();
            PopMenu pmGLASS = new PopMenu("GLASS", scGLASS, "ID_GLASS", pcs.MenuGroup);
            PopMenuItem pmi7 = new PopMenuItem(mm7, "MAKE and SHOW  GLASS", pmGLASS, -1);
            PopMenuItem pmi7_ = new PopMenuItem(mmGHG, "HIDE  GLASS", pmGLASS, -1);
            PopMenuItem pmi7__ = new PopMenuItem(mmGSHG, "SHOW HIDEN GLASS", pmGLASS, -1);
            PopMenuItem pmi5__ = new PopMenuItem(mmCPS5, "ERASE GLASS 3D and from database", pmGLASS, -1);
            PopMenuItem pmi_s_e_____ = new PopMenuItem(pmGLASS, -1);
            PopMenuItem pmi8CG = new PopMenuItem(mmGCC, "spots from glass on the triangles - Center of Gravity", pmGLASS, -1);
            PopMenuItem pmi8 = new PopMenuItem(mm8, "SHOW GLASS EDGES (spots from glass on the triangles)", pmGLASS, -1);
            // PopMenuItem pmi8A = new PopMenuItem(mm8A, "SHOW GLASS EDGES_BY_UNFOLD_BASE", pmGLASS, -1);
            PopMenuItem pmi8B = new PopMenuItem(mm10, "SHOW GlASS_CONTURS_BY_LEVEL", pmGLASS, -1);
            PopMenuItem pmi8C = new PopMenuItem(mm11, "SHOW GlASS_CONTURS_UNFOLD_BY_LEVEL", pmGLASS, -1);
            PopMenuItem pmi_s_e____ = new PopMenuItem(pmGLASS, -1);
            PopMenuItem pmi8C_ = new PopMenuItem(mmGlasUnfolds, "SEPARATE ALL - GlASS UNFOLDS BY LEVEL", pmGLASS, -1);
            PopMenuItem pmi8F_ = new PopMenuItem(mmS5, "SEPARATE BY NUMER - TRIANGLE GLASS UNFOLDS", pmGLASS, -1);
            PopMenuItem pmi8FF_ = new PopMenuItem(mmS4, "SEPARATE BY NUMER- POLYGON GLASS UNFOLDS", pmGLASS, -1);
            PopMenuItem pmi_se____ = new PopMenuItem(pmGLASS, -1);
            StringCollection scGLASS_DOUBLE = new StringCollection();
            PopMenu pmGLASS_DOUBLE = new PopMenu("DOUBLE GLASS", scGLASS_DOUBLE, "ID_GLASS_DOUBLE", pcs.MenuGroup);
            PopMenuItem pmu7 = new PopMenuItem(mmGlass_DOUBLE1, "SHOW", pmGLASS_DOUBLE, -1);
            PopMenuItem pmx7 = new PopMenuItem(mmdGHG, "HIDE DOUBLE GLASS", pmGLASS_DOUBLE, -1);
            PopMenuItem pmx7_ = new PopMenuItem(mmdGSHG, "SHOW HIDEN DOUBLE GLASS", pmGLASS_DOUBLE, -1);
            PopMenuItem pmi8__ = new PopMenuItem(mmCPS5, "ERASE GLASS 3D and from database", pmGLASS_DOUBLE, -1);
            PopMenuItem pmi_soe____ = new PopMenuItem(pmGLASS_DOUBLE, -1);
            PopMenuItem pmu8 = new PopMenuItem(mmGlass_DOUBLE2, "UNFOLD TRIANGULAR GLASS", pmGLASS_DOUBLE, -1);
            PopMenuItem pmu8a = new PopMenuItem(mmGlass_DOUBLE3, "BENDS TO SEPARATE DRAWINGS", pmGLASS_DOUBLE, -1);
            PopMenuItem pmu8aa = new PopMenuItem(mmRPN, "BENDS SECTIONS TO SEPARATE DRAWINGS", pmGLASS_DOUBLE, -1);
            PopMenuRef pmGlass_Double_Ref = new PopMenuRef(pmGLASS_DOUBLE, pmGLASS, -1);//***
            pmGLASS.PopMenuItems.Add(pmGlass_Double_Ref);
            PopMenuItem pmi_seu___ = new PopMenuItem(pmGLASS, -1);
            PopMenuItem pmi97 = new PopMenuItem(mm67, "GLASS  DISTANCE  FOR  BEND", pmGLASS, -1);
            PopMenuItem pmi9 = new PopMenuItem(mm6, "GLASS  DISTANCE  FOR  ALL BENDS", pmGLASS, -1);
            PopMenuItem pmi9_ = new PopMenuItem(mmDIST, "SET GLASS DISTANCE TO BEND BY NUMER", pmGLASS, -1);
            PopMenuRef pmGlass_Ref = new PopMenuRef(pmGLASS, pm, -1);
            pm.PopMenuItems.Add(pmGlass_Ref);
            #endregion

            PopMenuItem pmi_se_CAM__ = new PopMenuItem(pm, -1);
            StringCollection scCAM = new StringCollection();
            PopMenu pmCAM = new PopMenu("CAM Tools", scCAM, "ID_CAM_Tools", pcs.MenuGroup);
            PopMenuItem pm_cnc = new PopMenuItem(mmCAM_CNC, "Node to CNC", pmCAM, -1);
            PopMenuItem pm_cnc1 = new PopMenuItem(mmCAM_CNC1, "Node to CSV", pmCAM, -1);
            PopMenuRef pmCAM_Ref = new PopMenuRef(pmCAM, pm, -1);
            pm.PopMenuItems.Add(pmCAM_Ref);

            PopMenuItem pmi_se___ = new PopMenuItem(pm, -1);

            #region profiles
            StringCollection scPROFILES = new StringCollection();
            PopMenu pmPROFILES = new PopMenu("PROFILES", scPROFILES, "ID_SubProfiles", pcs.MenuGroup);
            PopMenuItem pmipi1 = new PopMenuItem(mmS2, "SOLID EXTRUDE FROM REGIONS IN BLOCK", pmPROFILES, -1);
            PopMenuItem pmipi2 = new PopMenuItem(mmS3, "CUT WITH EXTRUDED SOLID", pmPROFILES, -1);
            PopMenuItem pmi_sei_o__ = new PopMenuItem(pmPROFILES, -1);
            PopMenuItem pmip1 = new PopMenuItem(mmPrf1, "DRAW PROFILE SOLID by CSV", pmPROFILES, -1);
            PopMenuItem pmip1_ = new PopMenuItem(mmPrf1_, "DRAW PROFILE by CSV and ERASE PROFILE LINE", pmPROFILES, -1);
            PopMenuItem pmi_sei1 = new PopMenuItem(pmPROFILES, -1);
            PopMenuItem pmip2_ = new PopMenuItem(mmPrf2, "KCAD PREPARE PROFILE UNFOLD", pmPROFILES, -1);
            PopMenuItem pmi_sei___ = new PopMenuItem(pmPROFILES, -1);
            PopMenuItem pmip1i_ = new PopMenuItem(mmPrf1__, "ADD PROFILE POINTS TO_FILE", pmPROFILES, -1);
            PopMenuRef pmProfiles_Ref = new PopMenuRef(pmPROFILES, pm, -1);
            pm.PopMenuItems.Add(pmProfiles_Ref);
            #endregion

            PopMenuItem pmi_se___Placement = new PopMenuItem(pm, -1);

            #region placement
            StringCollection scPlacement_3D = new StringCollection();
            PopMenu pmPlacement_3D = new PopMenu("SHOW PLACEMENT IN 3D", scPlacement_3D, "ID_Placement_3D", pcs.MenuGroup);

            PopMenuItem pmPlacemen__22_00 = new PopMenuItem(mmCG, "Real Center of Gravity", pmPlacement_3D, -1);
            PopMenuItem pmi_seiuuyhu1 = new PopMenuItem(pmPlacement_3D, -1);

            StringCollection scPlacement_3D_NODES = new StringCollection();
            PopMenu pmPlacement_3D_NODES = new PopMenu("NODES 3D", scPlacement_3D_NODES, "ID_Bends_3D", pcs.MenuGroup);
            PopMenuItem pmPlacemen_0 = new PopMenuItem(mmPlacement_3D_0, "PLACEMENT OF NODE 3D IN POSITION", pmPlacement_3D_NODES, -1);
            PopMenuItem pmPlacemen_000 = new PopMenuItem(mmPlacement_3D_000, "PLACEMENT HIDE from Display", pmPlacement_3D_NODES, -1);
            PopMenuItem pmPlacemen_0000 = new PopMenuItem(mmPlacement_3D_0000, "PLACEMENT SHOW to Display", pmPlacement_3D_NODES, -1);
            PopMenuItem pmPlacemen_00 = new PopMenuItem(mmPlacement_3D_00, "PLACEMENT DELETE and from Data Base", pmPlacement_3D_NODES, -1);
            PopMenuRef pmPlacement_3D_NODES_Ref = new PopMenuRef(pmPlacement_3D_NODES, pmPlacement_3D, -1);
            pmPlacement_3D.PopMenuItems.Add(pmPlacement_3D_NODES_Ref);


            PopMenuItem pmi_seiuu1 = new PopMenuItem(pmPlacement_3D, -1);

            StringCollection scPlacement_3D_NOZZLE = new StringCollection();
            PopMenu pmPlacement_3D_NOZZLE = new PopMenu("BENDS NOZZLE 3D", scPlacement_3D_NOZZLE, "ID_Bends_3D", pcs.MenuGroup);
            PopMenuItem pmPlacemen_1 = new PopMenuItem(mmPlacement_3D_1, "PLACEMENT OF BENDS NOZZLE BLOCKS IN NODES", pmPlacement_3D_NOZZLE, -1);
            PopMenuItem pmPlacemen_100 = new PopMenuItem(mmPlacement_3D_100, "PLACEMENT HIDE from Display", pmPlacement_3D_NOZZLE, -1);
            PopMenuItem pmPlacemen_1000 = new PopMenuItem(mmPlacement_3D_1000, "PLACEMENT SHOW to Display", pmPlacement_3D_NOZZLE, -1);
            PopMenuItem pmPlacemen_10 = new PopMenuItem(mmPlacement_3D_10, "PLACEMENT DELETE and from Data Base", pmPlacement_3D_NOZZLE, -1);
            PopMenuRef pmPlacement_3D_NOZZLE_Ref = new PopMenuRef(pmPlacement_3D_NOZZLE, pmPlacement_3D, -1);
            pmPlacement_3D.PopMenuItems.Add(pmPlacement_3D_NOZZLE_Ref);

            PopMenuItem pmi_seiiu1 = new PopMenuItem(pmPlacement_3D, -1);
            // PopMenuItem pmPlacemen_2 = new PopMenuItem(mmPlacement_3D_2, "PLACEMENT BENDS 3D and from Data Base", pmPlacement_3D, -1);
            PopMenuRef pmPlacement_3D_Ref = new PopMenuRef(pmPlacement_3D, pm, -1);
            pm.PopMenuItems.Add(pmPlacement_3D_Ref);

            StringCollection scPlacement_3D_0 = new StringCollection();
            PopMenu pmPlacement_3D_0 = new PopMenu("BENDS 3D", scPlacement_3D_0, "ID_Bends_3D", pcs.MenuGroup);
            PopMenuItem pmPlacemen_2_0 = new PopMenuItem(mmPlacement_3D_2, "PLACEMENT BENDS 3D", pmPlacement_3D_0, -1);
            PopMenuItem pmPlacemen_2_a0 = new PopMenuItem(mmRCN, "PLACEMENT BENDS 3D  ( at the end of normals )", pmPlacement_3D_0, -1);
            PopMenuItem pmPlacemen_2_o0 = new PopMenuItem(mmS, "KCAD ATTACHING AN SOLID3D TO BEND", pmPlacement_3D_0, -1);
            PopMenuItem pmi_seiuuu1 = new PopMenuItem(pmPlacement_3D_0, -1);
            PopMenuItem pmPlacemen_2_10 = new PopMenuItem(mmPlacement_3D_4, "BENDS 3D HIDE", pmPlacement_3D_0, -1);
            PopMenuItem pmPlacemen_2_11 = new PopMenuItem(mmPlacement_3D_5, "BENDS 3D SHOW", pmPlacement_3D_0, -1);
            PopMenuItem pmPlacemen_2_00 = new PopMenuItem(mmPlacement_3D_3, "BENDS 3D DELETE  and from Data Base", pmPlacement_3D_0, -1);
            PopMenuItem pmi_seiuuyu1 = new PopMenuItem(pmPlacement_3D_0, -1);
            PopMenuItem pmPlacemen__2_00 = new PopMenuItem(mmHEeP, "KCAD CUTTING BENDS IN NODES", pmPlacement_3D_0, -1);
            PopMenuItem pmPlacemen__1_00 = new PopMenuItem(mmERS4, "* Explicit Cutting Method for Ends of 3D Bends In given Node", pmPlacement_3D_0, -1);
            PopMenuItem pmPlacemen__1_01 = new PopMenuItem(mmER5S5, "*Clear Explicit Cutting Method for All Bend", pmPlacement_3D_0, -1);
            PopMenuRef pmPlacement_3D_0_Ref = new PopMenuRef(pmPlacement_3D_0, pmPlacement_3D, -1);
            pmPlacement_3D.PopMenuItems.Add(pmPlacement_3D_0_Ref);
            #endregion

            PopMenuItem pmi_se___Placem = new PopMenuItem(pmPlacement_3D, -1);
            PopMenuItem pmPlacemen_10i = new PopMenuItem(mmCAM_Check, "CALC MIN CAM RADIUS", pmPlacement_3D, -1);

            #region fixing elements
            StringCollection scPlacement_3D_Fixing_Elements = new StringCollection();
            PopMenu pmPlacement_3D_Fixing_Elements = new PopMenu("Fixing Elements", scPlacement_3D_Fixing_Elements, "ID_Fixing_Elements", pcs.MenuGroup);
            PopMenuItem pmFixing_0 = new PopMenuItem(mmFixing, "ADD FIXING ELEMENTS", pmPlacement_3D_Fixing_Elements, -1);
            PopMenuRef pmPlacement_3D_Fixing_Elements_Ref = new PopMenuRef(pmPlacement_3D_Fixing_Elements, pmPlacement_3D, -1);
            pmPlacement_3D.PopMenuItems.Add(pmPlacement_3D_Fixing_Elements_Ref);
            #endregion


            PopMenuItem pmi___sie = new PopMenuItem(pm, -1);
            #region csv
            MenuMacro mmCSV_Node_Numer_Position = new MenuMacro(mg, "KCAD_NODE_NUMER_POSITION_CSV", "^C^CKCAD_NODE_NUMER_POSITION_CSV", "ID_KCAD_NODE_NUMER_POSITION_CSV");
            MenuMacro mmCSV_Bend_Joints_Numers = new MenuMacro(mg, "KCAD_BEND_JOINTS_NUMERS_CSV", "^C^CKCAD_BEND_JOINTS_NUMERS_CSV", "ID_KCAD_BEND_JOINTS_NUMERS_CSV");
            MenuMacro mmCSV_Triangles = new MenuMacro(mg, "KCAD_TRIANGLES_CSV", "^C^CKCAD_TRIANGLES_CSV", "ID_KCAD_TRIANGLES_CSV");

            //{
            StringCollection scPlacement_3D_CSV_Elements = new StringCollection();
            PopMenu pmPlacement_3D_CSV_Elements = new PopMenu("CSV files", scPlacement_3D_CSV_Elements, "ID_CSV_Elements", pcs.MenuGroup);

            StringCollection scCSV_NODES = new StringCollection();
            PopMenu pmCSV_NODES = new PopMenu("NODES  ", scCSV_NODES, "ID_NODES_CSV", pcs.MenuGroup);
            PopMenuItem pmCSV_00 = new PopMenuItem(mmCSV_Node_Numer_Position, "NODE NUMER and POSITION to file", pmCSV_NODES, -1);
            PopMenuRef pmCSV_NODES_Ref = new PopMenuRef(pmCSV_NODES, pmPlacement_3D_CSV_Elements, -1);
            pmPlacement_3D_CSV_Elements.PopMenuItems.Add(pmCSV_NODES_Ref);

            StringCollection scCSV_BENDS = new StringCollection();
            PopMenu pmCSV_BENDS = new PopMenu("BENDS  ", scCSV_BENDS, "ID_BENDS_CSV", pcs.MenuGroup);
            PopMenuItem pmCSV_0b = new PopMenuItem(mmCSV_Bend_Joints_Numers, "BEND JOINTS NUMERS", pmCSV_BENDS, -1);
            PopMenuRef pmCSV_BENDS_Ref = new PopMenuRef(pmCSV_BENDS, pmPlacement_3D_CSV_Elements, -1);
            pmPlacement_3D_CSV_Elements.PopMenuItems.Add(pmCSV_BENDS_Ref);

            StringCollection scCSV_TRIANGLES = new StringCollection();
            PopMenu pmCSV_TRIANGLES = new PopMenu("TRIANGLES  ", scCSV_BENDS, "ID_TRIANGLE_CSV", pcs.MenuGroup);
            PopMenuItem pmCSV_t0b = new PopMenuItem(mmCSV_Triangles, "TRIANGLES ", pmCSV_TRIANGLES, -1);
            PopMenuRef pmCSV_TRIANGLES_Ref = new PopMenuRef(pmCSV_TRIANGLES, pmPlacement_3D_CSV_Elements, -1);
            pmPlacement_3D_CSV_Elements.PopMenuItems.Add(pmCSV_TRIANGLES_Ref);

            PopMenuItem pmSQL_ = new PopMenuItem(mmCAM_CN1, "POLYGONS", pmPlacement_3D_CSV_Elements, -1);
            PopMenuItem pmiui___sie = new PopMenuItem(pmPlacement_3D_CSV_Elements, -1);
            PopMenuItem pmCSVV_ = new PopMenuItem(mm0CSV, "READ NAMED POINTS FROM CSV", pmPlacement_3D_CSV_Elements, -1);
            PopMenuItem pmCSVV1_ = new PopMenuItem(mm1CSV, "DRAW NAMED POINTS BY ARRAY NAME", pmPlacement_3D_CSV_Elements, -1);

            //}
            PopMenuRef pmPlacement_3D_CSV_Elements_Ref = new PopMenuRef(pmPlacement_3D_CSV_Elements, pm, -1);
            pm.PopMenuItems.Add(pmPlacement_3D_CSV_Elements_Ref);

            #endregion

            PopMenuItem pmii___sie = new PopMenuItem(pm, -1);

            #region deviation

            MenuMacro mmTR_An = new MenuMacro(mg, "KCAD_AGT", "^C^CKCAD_AGT", "ID_KCAD_AGT");
            MenuMacro mmTR_Dr = new MenuMacro(mg, "KCAD_AGT_DRAW", "^C^CKCAD_AGT_DRAW", "ID_KCAD_AGT_DRAW");
            MenuMacro mmTR_Anp = new MenuMacro(mg, "KCAD_AGP", "^C^CKCAD_AGP", "ID_KCAD_AGP");

            StringCollection scDeviation = new StringCollection();
            PopMenu pmDeviation = new PopMenu("Deviation tools", scPlacement_3D_Fixing_Elements, "ID_Deviation_tools", pcs.MenuGroup);

            StringCollection scTR = new StringCollection();
            PopMenu pmTR = new PopMenu("TRIANGLES  ", scTR, "ID_DEVIATION_TR_CSV", pcs.MenuGroup);
            PopMenuItem pmCSV_00_ = new PopMenuItem(mmTR_An, "Analise and DRAW Lines", pmTR, -1);
            PopMenuItem pmCSV_0_0_ = new PopMenuItem(mmTR_An, "DRAW TEXT in TRIANGLES", pmTR, -1);
            PopMenuRef pmDEVIATION_TR_Ref = new PopMenuRef(pmTR, pmDeviation, -1);
            pmDeviation.PopMenuItems.Add(pmDEVIATION_TR_Ref);
            PopMenuItem pmFixing_00 = new PopMenuItem(mmTR_Anp, "POLIGONs DEVIATION", pmDeviation, -1);

            PopMenuRef pmDeviation_Ref = new PopMenuRef(pmDeviation, pm, -1);
            pm.PopMenuItems.Add(pmDeviation_Ref);
            #endregion

            PopMenuItem pmiii_sie = new PopMenuItem(pm, -1);

            #region stereometry
            MenuMacro mmSG = new MenuMacro(mg, "Line and Plane - Intersection", "^C^CKCAD_ILP", "ID_KCAD_ILP");
            MenuMacro mmSG1 = new MenuMacro(mg, "TWO_LINES", "^C^CKCAD_DBP", "ID_KCAD_DBP");

            StringCollection scSolid_Geometry = new StringCollection();
            PopMenu pmSolid_Geometry = new PopMenu("Solid Geometry", scSolid_Geometry, "ID_Solid_Geometry", pcs.MenuGroup);

            PopMenuItem pmSG_3 = new PopMenuItem(mmMDBS, "Minimal Distance between Solids3D", pmSolid_Geometry, -1);
            PopMenuItem pmi_seiu1u1 = new PopMenuItem(pmSolid_Geometry, -1);
            PopMenuItem pmSG_4 = new PopMenuItem(mmITP, "Intersction Line between two Planes", pmSolid_Geometry, -1);
            PopMenuItem pmSG_0 = new PopMenuItem(mmSG, "Line and Plane - Intersection", pmSolid_Geometry, -1);
            PopMenuItem pmSG_1 = new PopMenuItem(mmSG1, "Two Lines - Crossing", pmSolid_Geometry, -1);
            PopMenuItem pmSG_2 = new PopMenuItem(mmERP5, "Projection of the Point on the Plane", pmSolid_Geometry, -1);

            PopMenuItem pmi_seiuu11 = new PopMenuItem(pmSolid_Geometry, -1);

            StringCollection scCutSolids = new StringCollection();
            PopMenu pmCutSolids = new PopMenu("CUT PARAMETERS", scCutSolids, "ID_Cut_Solids", pcs.MenuGroup);
            PopMenuItem pmCut_0 = new PopMenuItem(mmCPS, "Global - Ratio of Length of the Extensions", pmCutSolids, -1);
            PopMenuItem pmCut_1 = new PopMenuItem(mmCPS1, "Global - Thicnes of the CutProfile", pmCutSolids, -1);
            PopMenuItem pmCut_3 = new PopMenuItem(mmCPS3, "Global - Extrude Ratio", pmCutSolids, -1);
            PopMenuItem pmi_seiu1 = new PopMenuItem(pmCutSolids, -1);
            PopMenuItem pmCutg_2 = new PopMenuItem(mmERS5, "By Bend Selection - Solid Extrude Ratio", pmCutSolids, -1);
            PopMenuItem pmCut_2 = new PopMenuItem(mmCPS2, "By Bend Numer - Ratio of Length /  Thicnes of the CutProfile", pmCutSolids, -1);
            PopMenuItem pmi_sesiu1 = new PopMenuItem(pmCutSolids, -1);
            PopMenuItem pmCut_4 = new PopMenuItem(mmCPS4, "Trimming Vertices on/off", pmCutSolids, -1);

            PopMenuRef pmCutSolids_Ref = new PopMenuRef(pmCutSolids, pmSolid_Geometry, -1);
            pmSolid_Geometry.PopMenuItems.Add(pmCutSolids_Ref);

            //
            PopMenuItem pmi_seiuu02 = new PopMenuItem(pmSolid_Geometry, -1);
            StringCollection scMESH_MINIMUM_DISTANCES_ = new StringCollection();
            PopMenu pmMESH_MINIMUM_DISTANCES_ = new PopMenu("Minimum Distance to ...", scMESH_MINIMUM_DISTANCES_, "ID_MINDISTT", pcs.MenuGroup);
            PopMenuItem pmMD1_ = new PopMenuItem(mmMDN, "to Node", pmMESH_MINIMUM_DISTANCES_, -1);
            PopMenuItem pmMD2_ = new PopMenuItem(mmMDB, "to Bend Mid Point", pmMESH_MINIMUM_DISTANCES_, -1);
            PopMenuItem pmMD3_ = new PopMenuItem(mmMDT, "to Triangle Centroid", pmMESH_MINIMUM_DISTANCES_, -1);
            PopMenuRef pmMESH_MINIMUM_DISTANCES_Ref_ = new PopMenuRef(pmMESH_MINIMUM_DISTANCES_, pmSolid_Geometry, -1);
            pmSolid_Geometry.PopMenuItems.Add(pmMESH_MINIMUM_DISTANCES_Ref_);
            //

            PopMenuItem pmi_seiuu12 = new PopMenuItem(pmSolid_Geometry, -1);
            PopMenuItem pmSGf2 = new PopMenuItem(mmERT5, "Angle between the NORMALS of the adjacent Triangles", pmSolid_Geometry, -1);

            PopMenuRef pmSolid_Geometry_Ref = new PopMenuRef(pmSolid_Geometry, pm, -1);
            pm.PopMenuItems.Add(pmSolid_Geometry_Ref);


            #endregion

            //PopMenuItem pmiii_siye = new PopMenuItem(pm, -1);

            pcs.SaveAs(myCuiFile);
            //LoadMyCui(myCuiFileToSend);
            MessageBox.Show("Menu has been successfully created in\n\n" +
               myCuiFile + "\n\nFor display in the main Menu Bar, use the Command: CUILOAD ", "New Menu:", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
#endif