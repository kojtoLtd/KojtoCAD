using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D;

using KojtoCAD.KojtoCAD3D.UtilityClasses;
using KojtoCAD.KojtoCAD3D.WorkClasses;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
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
[assembly: CommandClass(typeof(Kojto3DMesh))]

namespace KojtoCAD.KojtoCAD3D
{
    public class Kojto3DMesh
    {
        public Containers container = ContextVariablesProvider.Container;

        public Dictionary<Handle, Pair<Pair<quaternion, quaternion>, Pair<quaternion, quaternion>>> buffDictionary =
            ContextVariablesProvider.BuffDictionary;

        //Read new from Selection in Editor
        //[CommandMethod("KojtoCAD_3D", "KCAD_CREATEMESH", null, CommandFlags.Modal, null, "KojtoCAD_3D", "KojtoCAD_3D_Read_New_From_Selection")]  
        [CommandMethod("KojtoCAD_3D", "KCAD_CREATEMESH", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/KCAD_CREATEMESH.htm", "")]
        public void KojtoCAD_3D_Method_Read_New_From_Selection()
        {
            

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {
                Pair<List<Entity>, List<Entity>> pa =
                GlobalFunctions.GetSelectionOn(ConstantsAndSettings.GetBendsLayer(),
                                                             ConstantsAndSettings.GetFictivebendsLayer());

                ContextVariablesProvider.Container = new Containers(ref pa);

                int notfictive = 0;
                foreach (Bend b in container.Bends)
                {
                    if (!b.Fictive) { notfictive++; }
                }

                string formatString = "{0,-20}\t{1,-12}\n\n";

                string mess = "\n";
                mess += string.Format(formatString, "Nodes:", container.Nodes.Count);
                mess += string.Format(formatString, "All Bends:", container.Bends.Count);
                mess += string.Format(formatString, "Bends:", notfictive);
                mess += string.Format(formatString, "Fictive Bends:", container.Bends.Count - notfictive);
                mess += string.Format(formatString, "Peripheral Bends:", container.peripheralBendsNumers.Count);
                mess += string.Format(formatString, "Triangles:", container.Triangles.Count);
                mess += string.Format(formatString, "Polygons:", container.Polygons.Count);

                if (!container.error)
                {
                    MessageBox.Show("\nMesh data was created from the selected objects !\n\n" + mess +
                    "\n\n" + ConstantsAndSettings.GetString(), "Mesh data was created from the selected objects", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ed.WriteMessage("\nMesh data was created from the selected objects !");
                }
                else
                {
                    String errmess = "Mesh data ERROR !\n\n";
                    errmess += "1. Execute command \"KCAD_CHEK\"\n";
                    errmess += "2. Select suitable Layer and Color. \n";
                    errmess += "3. Execute command \"KCAD_DRAW_PEREFERIAL_BENDS\"\n";
                    errmess += "4. Look matches !";
                    MessageBox.Show(errmess, "E R R O R - Selection Reading", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch
            {
                MessageBox.Show("\nMesh data was created from the selected objects !", "E R R O R - Selection Reading", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ed.WriteMessage("\nMesh data was NOT created from the selected objects !");
            }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_READ_MESH_FROM_CSV", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/READ_MESH_FROM_CSV.htm", "")]
        public void KojtoCAD_3D_Method_Read_New_From_CSV()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
            dlg.Multiselect = false;
            dlg.Title = "Select CSV File ";
            dlg.DefaultExt = "csv";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    List<Pair<quaternion, quaternion>> listNoFictive = new List<Pair<quaternion, quaternion>>();
                    List<Pair<quaternion, quaternion>> listFictive = new List<Pair<quaternion, quaternion>>();

                    bool read = false;

                    #region read file
                    Pair<int, PromptStatus> Fictive_Pair =
                                   GlobalFunctions.GetInt(2, "Sequential Number of Column for Fictive Property: ", false, false);
                    if ((Fictive_Pair.Second == PromptStatus.OK) && (Fictive_Pair.First > 0))
                    {
                        int Fictive_Property_ColumnNumber = Fictive_Pair.First - 1;

                        Pair<int, PromptStatus> X_Start_Pair =
                            GlobalFunctions.GetInt(Fictive_Pair.First + 1, "Sequential Number of Column for First Point X Coordinat: ", false, false);
                        if ((X_Start_Pair.Second == PromptStatus.OK) && (X_Start_Pair.First > 0))
                        {
                            int X_Start_ColumnNumber = X_Start_Pair.First - 1;

                            Pair<int, PromptStatus> Y_Start_Pair =
                                 GlobalFunctions.GetInt(X_Start_Pair.First + 1, "Sequential Number of Column for First Point Y Coordinat: ", false, false);
                            if ((Y_Start_Pair.Second == PromptStatus.OK) && (Y_Start_Pair.First > 0))
                            {
                                int Y_Start_ColumnNumber = Y_Start_Pair.First - 1;

                                Pair<int, PromptStatus> Z_Start_Pair =
                                 GlobalFunctions.GetInt(Y_Start_Pair.First + 1, "Sequential Number of Column for First Point Z Coordinat: ", false, false);

                                if ((Z_Start_Pair.Second == PromptStatus.OK) && (Z_Start_Pair.First > 0))
                                {
                                    int Z_Start_ColumnNumber = Z_Start_Pair.First - 1;

                                    Pair<int, PromptStatus> X_End_Pair =
                                         GlobalFunctions.GetInt(Z_Start_Pair.First + 1, "Sequential Number of Column for Second Point X Coordinat: ", false, false);

                                    if ((X_End_Pair.Second == PromptStatus.OK) && (X_End_Pair.First > 0))
                                    {
                                        int X_End_ColumnNumber = X_End_Pair.First - 1;

                                        Pair<int, PromptStatus> Y_End_Pair =
                                         GlobalFunctions.GetInt(X_End_Pair.First + 1, "Sequential Number of Column for Second Point Y Coordinat: ", false, false);
                                        if ((Y_End_Pair.Second == PromptStatus.OK) && (Y_End_Pair.First > 0))
                                        {
                                            int Y_End_ColumnNumber = Y_End_Pair.First - 1;

                                            Pair<int, PromptStatus> Z_End_Pair =
                                                 GlobalFunctions.GetInt(Y_End_Pair.First + 1, "Sequential Number of Column for Second Point Z Coordinat: ", false, false);
                                            if ((Z_End_Pair.Second == PromptStatus.OK) && (Z_End_Pair.First > 0))
                                            {
                                                int Z_End_ColumnNumber = Z_End_Pair.First - 1;

                                                using (StreamReader sr = new StreamReader(dlg.FileName))
                                                {
                                                    string line;
                                                    char[] splitChars = { ';' };

                                                    while ((line = sr.ReadLine()) != null)
                                                    {
                                                        //if (line.IndexOf(';') < 0) { continue; }

                                                        string[] split = line.Split(splitChars);

                                                        try
                                                        {
                                                            bool Nofictive = split[Fictive_Property_ColumnNumber].IndexOf('N') >= 0 ||
                                                                 split[Fictive_Property_ColumnNumber].IndexOf('n') >= 0;

                                                            split[X_Start_ColumnNumber] = split[X_Start_ColumnNumber].Replace(',', '.');
                                                            split[Y_Start_ColumnNumber] = split[Y_Start_ColumnNumber].Replace(',', '.');
                                                            split[Z_Start_ColumnNumber] = split[Z_Start_ColumnNumber].Replace(',', '.');

                                                            split[X_End_ColumnNumber] = split[X_End_ColumnNumber].Replace(',', '.');
                                                            split[Y_End_ColumnNumber] = split[Y_End_ColumnNumber].Replace(',', '.');
                                                            split[Z_End_ColumnNumber] = split[Z_End_ColumnNumber].Replace(',', '.');

                                                            double sX = double.NaN;
                                                            double sY = double.NaN;
                                                            double sZ = double.NaN;

                                                            double eX = double.NaN;
                                                            double eY = double.NaN;
                                                            double eZ = double.NaN;

                                                            try
                                                            {
                                                                sX = double.Parse(split[X_Start_ColumnNumber]);
                                                                sY = double.Parse(split[Y_Start_ColumnNumber]);
                                                                sZ = double.Parse(split[Z_Start_ColumnNumber]);

                                                                eX = double.Parse(split[X_End_ColumnNumber]);
                                                                eY = double.Parse(split[Y_End_ColumnNumber]);
                                                                eZ = double.Parse(split[Z_End_ColumnNumber]);
                                                            }
                                                            catch (FormatException)
                                                            {
                                                                MessageBox.Show("Unable to convert '{0}' to a Double.", line, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                                continue;
                                                            }
                                                            catch (OverflowException)
                                                            {
                                                                MessageBox.Show("'{0}' is outside the range of a Double.", line, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                                continue;
                                                            }

                                                            if (Nofictive)
                                                            {
                                                                listNoFictive.Add(new Pair<quaternion, quaternion>(new quaternion(0, sX, sY, sZ), new quaternion(0, eX, eY, eZ)));
                                                                //UtilityClasses.GlobalFunctions.DrawLine((Point3d)(new quaternion(0, sX, sY, sZ)), (Point3d)(new quaternion(0, eX, eY, eZ)));
                                                            }
                                                            else
                                                            {
                                                                listFictive.Add(new Pair<quaternion, quaternion>(new quaternion(0, sX, sY, sZ), new quaternion(0, eX, eY, eZ)));
                                                                //UtilityClasses.GlobalFunctions.DrawLine((Point3d)(new quaternion(0, sX, sY, sZ)), (Point3d)(new quaternion(0, eX, eY, eZ)));
                                                            }
                                                        }
                                                        catch {
                                                        }
                                                    }


                                                    sr.Close();
                                                }
                                                read = true;
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                    #endregion

                    if (!read) { return; }

                    ContextVariablesProvider.Container = new Containers(ref listNoFictive, ref listFictive);

                    int notfictive = 0;
                    foreach (Bend b in container.Bends)
                    {
                        if (!b.Fictive) { notfictive++; }
                    }

                    string formatString = "{0,-20}\t{1,-12}\n\n";

                    string mess = "\n";
                    mess += string.Format(formatString, "Nodes:", container.Nodes.Count);
                    mess += string.Format(formatString, "All Bends:", container.Bends.Count);
                    mess += string.Format(formatString, "Bends:", notfictive);
                    mess += string.Format(formatString, "Fictive Bends:", container.Bends.Count - notfictive);
                    mess += string.Format(formatString, "Peripheral Bends:", container.peripheralBendsNumers.Count);
                    mess += string.Format(formatString, "Triangles:", container.Triangles.Count);
                    mess += string.Format(formatString, "Polygons:", container.Polygons.Count);

                    if (!container.error)
                    {
                        MessageBox.Show("\nMesh data was created from the selected objects !\n\n" + mess +
                        "\n\n" + ConstantsAndSettings.GetString(), "Mesh data was created from the selected objects", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ed.WriteMessage("\nMesh data was created from the selected objects !");
                    }
                    else
                    {
                        String errmess = "Mesh data ERROR !\n\n";
                        errmess += "1. Execute command \"KCAD_CHEK\"\n";
                        errmess += "2. Select suitable Layer and Color. \n";
                        errmess += "3. Execute command \"KCAD_DRAW_PEREFERIAL_BENDS\"\n";
                        errmess += "4. Look matches !";
                        MessageBox.Show(errmess, "E R R O R - Selection Reading", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                catch
                {
                    MessageBox.Show("\nMesh data was created from CSV file !", "E R R O R - CSV file Reading", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ed.WriteMessage("\nMesh data was NOT created from the CSV file !");
                }

            }//

        }

        //Read old from Dictionary in DataBase
        [CommandMethod("KojtoCAD_3D", "KCAD_READMESH", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Read_Old_From_Dictionary()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\n Base or Second";

            pKeyOpts.Keywords.Add("Base");
            pKeyOpts.Keywords.Add("Second");
            pKeyOpts.Keywords.Default = "Base";
            pKeyOpts.AllowNone = false;

            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.OK)
            {
                bool addres = pKeyRes.StringResult == "Base";

                try
                {
                    ContextVariablesProvider.Container = new Containers();
                    container.Read(addres);
                    ed.UpdateScreen();

                    int notfictive = 0;
                    foreach (Bend b in container.Bends)
                    {
                        if (!b.Fictive) { notfictive++; }
                    }

                    string formatString = "{0,-20}\t{1,-12}\n\n";

                    string mess = "\n";
                    mess += string.Format(formatString, "Nodes:", container.Nodes.Count);
                    mess += string.Format(formatString, "All Bends:", container.Bends.Count);
                    mess += string.Format(formatString, "Bends:", notfictive);
                    mess += string.Format(formatString, "Fictive Bends:", container.Bends.Count - notfictive);
                    mess += string.Format(formatString, "Peripheral Bends:", container.peripheralBendsNumers.Count);
                    mess += string.Format(formatString, "Triangles:", container.Triangles.Count);
                    mess += string.Format(formatString, "Polygons:", container.Polygons.Count);

                    MessageBox.Show("\nMesh data was loaded from the Dictionary !\n\n" + mess +
                    "\n\n" + ConstantsAndSettings.GetString(), "Reading from the Dictionary ...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ed.WriteMessage("\nMesh data was loaded from the Dictionary !");
                }
                catch
                {
                    ed.UpdateScreen();
                    MessageBox.Show("\nMesh data was NOT loaded from the Dictionary !", "ERROR while reading form Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ed.WriteMessage("\nMesh data was NOT loaded from the Dictionary !");
                }
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_READMESH_GET_HELP", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Read_Old_From_Dictionary_help()
        {
            
            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/KCAD_READMESH.htm");
        }

        //Save Data in Dictionary Data Base
        [CommandMethod("KojtoCAD_3D", "KCAD_SAVEMESH", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Save_To_Dictionary()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\n Base or Second";

            pKeyOpts.Keywords.Add("Base");
            pKeyOpts.Keywords.Add("Second");
            pKeyOpts.Keywords.Default = "Base";
            pKeyOpts.AllowNone = false;

            PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.OK)
            {
                bool addres = pKeyRes.StringResult == "Base";

                if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
                {
                    try
                    {
                        container.Save(addres);
                        //MessageBox.Show("\nMesh data was saved to the Dictionary  !\n\n", "Save to Dictionary was successfull", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ed.WriteMessage("\nMesh data was saved to the Dictionary  !");

                        if (MessageBox.Show("\nMesh data was saved to the Dictionary  ! \n\nSave new data from mesh in drawing ?",
                            "Save Drawing File ?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Application.DocumentManager.MdiActiveDocument.SendStringToExecute(".QSAVE ", true, false, false);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("\nMesh data was NOT saved to the Dictionary !\n\n", "ERROR while saving to Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ed.WriteMessage("\nMesh data was NOT saved to the Dictionary !");
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nThere is nothing to Record !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SAVEMESH_GET_HELP", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Method_Save_To_Dictionary_help()
        {
            
            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/KCAD_SAVEMESH.htm");
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_DELETE_MESH_DATA", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_Delete_Mesh_Data()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            // if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
            {
                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\n Base or Second";

                pKeyOpts.Keywords.Add("Base");
                pKeyOpts.Keywords.Add("Second");
                pKeyOpts.Keywords.Default = "Base";
                pKeyOpts.AllowNone = false;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    Pair<string, PromptStatus> strOpt =
                    GlobalFunctions.GetKey(new[] { "Yes", "No" }, 1, "\nAre you sure you want to delete the data ?");
                    if ((strOpt.Second == PromptStatus.OK) && (strOpt.First == "Yes"))
                    {
                        bool addres = pKeyRes.StringResult == "Base";
                        try
                        {
                            bool b = container.Delete(addres);
                            if (b)
                            {
                                ed.WriteMessage("\nMesh data was erased from the Dictionary  !");

                                if (MessageBox.Show("\nMesh data was erased from the Dictionary  ! \n\nSave drawing ?",
                                     "Save Drawing File ?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    Application.DocumentManager.MdiActiveDocument.SendStringToExecute(".QSAVE ", true, false, false);
                                }
                            }
                        }
                        catch
                        {
                            MessageBox.Show("\nMesh data was NOT erasedd from the Dictionary !\n\n", "ERROR while erased from Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ed.WriteMessage("\nMesh data was NOT erased from the Dictionary !");
                        }
                    }
                }
            }
            // else
            // MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_HELP", null, CommandFlags.Modal)]
        public void KojtoCAD_3D_GetHelp()
        {
            
            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/main.htm");
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SAVE_CONTAINER_IN_EXTERNAL_FILE", null, CommandFlags.Modal, null, "", "")]
        public void SaveContainerInExternalFile()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.Filter = "MSH or Text Files|*.msh;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                dlg.Title = "Enter MSH File Name ";
                dlg.DefaultExt = "msh";
                dlg.FileName = "*.msh";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dlg.FileName;
                    if (container.SaveInExternalFile(fileName))
                    {
                        MessageBox.Show("Succes to write File !", "Save File...");
                        ed.WriteMessage("\n Succes to write File ! \n");
                    }
                    else
                    {
                        MessageBox.Show("unSuccess to write File !", "Save File...");
                        ed.WriteMessage("\n unSuccess to write File ! \n");
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_READ_CONTAINER_FROM_EXTERNAL_FILE", null, CommandFlags.Modal, null, "", "")]
        public void ReadContainerFromExternalFile()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "MSH or Text Files|*.msh;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
            dlg.Multiselect = false;
            dlg.Title = "Select MSH File ";
            dlg.DefaultExt = "msh";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                container = new Containers();
                if (container.ReadFromExternalFile(dlg.FileName))
                {
                    int notfictive = 0;
                    foreach (Bend b in container.Bends)
                    {
                        if (!b.Fictive) { notfictive++; }
                    }

                    string formatString = "{0,-20}\t{1,-12}\n\n";

                    string mess = "\n";
                    mess += string.Format(formatString, "Nodes:", container.Nodes.Count);
                    mess += string.Format(formatString, "All Bends:", container.Bends.Count);
                    mess += string.Format(formatString, "Bends:", notfictive);
                    mess += string.Format(formatString, "Fictive Bends:", container.Bends.Count - notfictive);
                    mess += string.Format(formatString, "Peripheral Bends:", container.peripheralBendsNumers.Count);
                    mess += string.Format(formatString, "Triangles:", container.Triangles.Count);
                    mess += string.Format(formatString, "Polygons:", container.Polygons.Count);

                    MessageBox.Show("\nMesh data was loaded from the Dictionary !\n\n" + mess +
                    "\n\n" + ConstantsAndSettings.GetString(), "Reading from the Dictionary ...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ed.WriteMessage("\nMesh data was loaded from the Dictionary !");
                }
                else
                    container = new Containers();
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_SMB", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/DRAW_SECOND_MESH.htm", "")]
        public void KojtoCAD_Create_new_Mesh_from_Current_through_end_of_normals()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {

                Pair<string, PromptStatus> strOpt =
                        GlobalFunctions.GetKey(new string[] { "Run", "Help" }, 0, "\nYou want to run the function ?");
                if ((strOpt.Second == PromptStatus.OK))
                {
                    switch (strOpt.First)
                    {
                        case "Run":
                            if ((container != null) && (container.Bends.Count > 0) && (container.Triangles.Count > 0))
                            {
                                #region #nodes (one normals)

                                double k = ConstantsAndSettings.NormlLengthToShow;

                                foreach (WorkClasses.Node node in container.Nodes)
                                {
                                    quaternion n = node.GetNormal() - node.Position;
                                    n *= (((object)node.ExplicitNormal != null) ? node.ExplicitNormalLength : k);

                                    node.SetPosition(node.Position + n);
                                    node.Normal = new quaternion();
                                    node.ExplicitNormal = null;
                                    node.ExplicitNormalLength = 1.0;
                                }
                                #endregion

                                #region tiangles
                                foreach (Triangle TR in container.Triangles)
                                {
                                    TR.SetPreNodes(new Triplet<quaternion, quaternion, quaternion>(
                                        container.Nodes[TR.NodesNumers[0]].Position,
                                        container.Nodes[TR.NodesNumers[1]].Position,
                                        container.Nodes[TR.NodesNumers[2]].Position));

                                    quaternion centroid = (TR.Nodes.First + TR.Nodes.Second) / 2.0;
                                    centroid = centroid - TR.Nodes.Third;
                                    centroid *= (2.0 / 3.0);
                                    centroid = TR.Nodes.Third + centroid;

                                    UCS trUCS = new UCS(TR.Nodes.First, TR.Nodes.Second, TR.Nodes.Third);
                                    if (trUCS.FromACS(TR.Normal.First).GetZ() > 0.0)
                                        trUCS = new UCS(TR.Nodes.Second, TR.Nodes.First, TR.Nodes.Third);

                                    quaternion z = trUCS.ToACS(new quaternion(0, 0, 0, 1.0)) - trUCS.ToACS(new quaternion());
                                    TR.Normal = new Pair<quaternion, quaternion>(centroid, centroid + z);

                                }
                                #endregion

                                #region bends
                                foreach (Bend bend in container.Bends)
                                {
                                    quaternion st = container.Nodes[bend.StartNodeNumer].Position;
                                    quaternion en = container.Nodes[bend.EndNodeNumer].Position;
                                    quaternion n = bend.Normal - bend.MidPoint;

                                    bend.Pre_Bend = new Pair<quaternion, quaternion>(st, en);

                                    bend.MidPoint = (st + en) / 2.0;
                                    if (bend.SecondTriangleNumer >= 0)
                                    {
                                        // bend.SetNormal(container.Triangles[bend.FirstTriangleNumer], container.Triangles[bend.SecondTriangleNumer]);
                                        Pair<quaternion, quaternion> pre1 = container.Triangles[bend.FirstTriangleNumer].Normal;
                                        Pair<quaternion, quaternion> pre2 = container.Triangles[bend.SecondTriangleNumer].Normal;

                                        quaternion q1 = pre1.Second - pre1.First;
                                        quaternion q2 = pre2.Second - pre2.First;

                                        q1 *= 1000.0; q2 *= 1000.0;
                                        quaternion q = (q1 + q2) / 2.0;
                                        q += bend.MidPoint;

                                        bend.Normal = q;

                                    }
                                    else
                                        bend.Normal = bend.MidPoint + n;

                                    if (bend.SecondTriangleNumer < 0)
                                    {
                                        if (ConstantsAndSettings.PerepherialBendsNormalDirection == 0)
                                        {
                                            Triangle tr = container.Triangles[bend.FirstTriangleNumer];
                                            bend.Normal = bend.MidPoint + tr.Normal.Second - tr.Normal.First;
                                        }
                                    }

                                    n = bend.Normal - bend.MidPoint;
                                    n /= n.abs();
                                    bend.Normal = bend.MidPoint + n;

                                    bend.Size = (st - en).abs();


                                }
                                #endregion

                                #region #nodes normals by nofictive
                                foreach (WorkClasses.Node node in container.Nodes)
                                {
                                    node.SetNormal(node.GetNodesNormalsByNoFictiveBends(ref container));
                                }
                                #endregion

                                MessageBox.Show("The Mesh Data are successfully changed !", "Success ");
                            }
                            else
                                MessageBox.Show("\nData Base Empty !\n\nMissing Bends !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "Help":
                            GlobalFunctions.OpenHelpHTML("http://3dsoft.blob.core.windows.net/kojtocad/html/DRAW_SECOND_MESH.htm");
                            break;
                    }
                }
            }
            catch { }
            finally
            {
                ed.CurrentUserCoordinateSystem = old;
            }
        }

        //--- CHECK Functions --------------------------------------------------------------------------        
        //proverqwa dali wyrhu nqkoq otse4ka levi kraina to4ka na druga otse4ka - cepi q w tazi to4ka
        [CommandMethod("KojtoCAD_3D", "KCAD_CHEK", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/KCAD_CREATEMESH.htm", "")]
        public void KojtoCAD_3D_Check()
        {
            
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Matrix3d old = ed.CurrentUserCoordinateSystem;
            ed.CurrentUserCoordinateSystem = Matrix3d.Identity;

            try
            {
                TypedValue[] acTypValAr = new TypedValue[1];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "LINE"), 0);

                List<Entity> solids = GlobalFunctions.GetSelection(ref acTypValAr, "Select Lines: ");

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (Entity ent in solids)
                    {
                        Line Line1 = tr.GetObject(ent.ObjectId, OpenMode.ForWrite) as Line;
                        quaternion qStart1 = Line1.StartPoint;
                        quaternion qEnd1 = Line1.EndPoint;
                        quaternion q1 = qEnd1 - qStart1;
                        foreach (Entity ENT in solids)
                        {
                            if (ENT.ObjectId != ent.ObjectId)
                            {
                                Line Line2 = tr.GetObject(ENT.ObjectId, OpenMode.ForWrite) as Line;
                                quaternion qStart2 = Line2.StartPoint;
                                quaternion qEnd2 = Line2.EndPoint;

                                quaternion QQ1 = Common.IsCoincidentWithLine(qStart1, qEnd1, qStart2);
                                quaternion QQ2 = Common.IsCoincidentWithLine(qStart1, qEnd1, qEnd2);
                                if (((object)QQ1 != null || (object)QQ2 != null) && !((object)QQ1 != null && (object)QQ2 != null))
                                {
                                    //internal - Q1 != null
                                    quaternion Q1 = Common.IsInternalForSegment(qStart1, qEnd1, qStart2);
                                    //internal - Q2 != null
                                    quaternion Q2 = Common.IsInternalForSegment(qStart1, qEnd1, qEnd2);

                                    #region split or no
                                    if ((object)Q1 != null || (object)Q2 != null)
                                    {
                                        Point3d p = (Point3d)(q1 * (((object)Q1 != null) ? Q1.real() : Q2.real()) + qStart1);

                                        Line Line3 = new Line((Point3d)qStart1, p);
                                        Line3.Layer = Line1.Layer;
                                        Line3.ColorIndex = Line1.ColorIndex;
                                        acBlkTblRec.AppendEntity(Line3);
                                        tr.AddNewlyCreatedDBObject(Line3, true);

                                        Line Line4 = new Line((Point3d)qEnd1, p);
                                        Line4.Layer = Line1.Layer;
                                        Line4.ColorIndex = Line1.ColorIndex;
                                        acBlkTblRec.AppendEntity(Line4);
                                        tr.AddNewlyCreatedDBObject(Line4, true);

                                        Line1.Erase();
                                        break;

                                    }
                                }
                                    #endregion

                            }
                        }
                        if (!Line1.IsErased)
                            Line1.Visible = true;
                    }
                    tr.Commit();
                }
            }
            catch { }
            finally { ed.CurrentUserCoordinateSystem = old; }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_BENDS_TO_DICTIONARY", null, CommandFlags.Modal, null, "", "")]
        public void KojtoCAD_3D_BendsToDictionary()
        {
            
            if (container != null)
            {
                if (container.Bends.Count > 0)
                {
                    buffDictionary.Clear();
                    foreach (Bend bend in container.Bends)
                    {
                        if (bend.IsFictive()) continue;
                        if (bend.SolidHandle.First >= 0)
                        {

                            buffDictionary.Add(bend.SolidHandle.Second,
                                new Pair<Pair<quaternion, quaternion>, Pair<quaternion, quaternion>>(new Pair<quaternion, quaternion>(bend.Start, bend.End), new Pair<quaternion, quaternion>(bend.MidPoint, bend.Normal)));

                        }
                    }
                }
            }
        }
    }
}
