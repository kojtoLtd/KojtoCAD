using KojtoCAD.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
#else
using Teigha.DatabaseServices;
using Teigha.Runtime;
#endif
#if acad2013
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
#if acad2012
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#endif
#if bcad
using Application = Bricscad.ApplicationServices.Application;
#endif

[assembly: CommandClass(typeof(KojtoCAD.Attribute.CheckAttributes))]

namespace KojtoCAD.Attribute
{
    public class CheckAttributes
    {
        private readonly EditorHelper _editorHelper;

        public CheckAttributes()
        {
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Check if attribute text exists in exceltable
        /// </summary>
        [CommandMethod("GroupCheckAttributes", "ChkAttrIsInExcel", null, CommandFlags.Modal, null, "CheckAttributes", "ChkAttr_inExcel")]
        public void ChkAttrIsInExcelStart()
        {
            Database db = HostApplicationServices.WorkingDatabase;

            ArrayList Blocks = new ArrayList();
            ArrayList BlocksWithLessThanThreeAttribute = new ArrayList();
            ArrayList BlocksWithRepeatingTags = new ArrayList();

            using (Transaction TR = db.TransactionManager.StartTransaction())
            {
                AttributeHelper.ScanForBlocks(db, TR, ref Blocks, ref BlocksWithLessThanThreeAttribute, ref BlocksWithRepeatingTags);

            }

            var bForm = new BaseForm(ref Blocks, ref BlocksWithLessThanThreeAttribute);
            bForm.ShowDialog();
            if (bForm.DialogResult == DialogResult.OK)
            {
                string block = bForm.BlocName();
                string attrTAG = bForm.AttributeTag();
                string DocName = Application.DocumentManager.MdiActiveDocument.Name;
                int ExistAttrColorIndex = bForm.ExistAttributesColorIndex();
                int NotExistAttrColorIndex = bForm.NotExistAttributesColorIndex();
                StringCollection Files = new StringCollection();
                Files.Add(DocName);
                StringCollection LogFileContents = new StringCollection();
                List<StringCollection> exceldata = AttributeHelper.GetExelData(bForm.GetExelFileName(), bForm.SheetName());
                DateTime time = DateTime.Now;
                string logName = "Compare_Attributes_Log_" + String.Format("{0:yyyy/M/d_HH_mm_ss}", time);
                if ((exceldata.Count > 0) && (block.Length > 0) && (attrTAG.Length > 0) && (bForm.OutFolder().Length > 1))
                {
                    if (bForm.CheckBoxAllFiles())
                    {
                        System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                        dlg.Multiselect = true;
                        dlg.Title = "Select Drawing File(s) ";
                        dlg.Filter = "txt files (*.dwg)|*.dwg|All files (*.*)|*.*";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            foreach (string str in dlg.FileNames)
                            {
                                if (str != DocName)
                                {
                                    Files.Add(str);
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    LogFileContents.Add("-----------------------------------------------");
                    LogFileContents.Add("Log File for: ChkAttr_IsInExcel");
                    LogFileContents.Add("Log File Name: " + bForm.OutFolder() + logName + ".log");
                    LogFileContents.Add(" ");
                    LogFileContents.Add("Block: " + block);
                    LogFileContents.Add("TAG: " + attrTAG);
                    LogFileContents.Add("EXCEL File: " + bForm.GetExelFileName());
                    LogFileContents.Add(" ");
                    LogFileContents.Add("Change Attribute Colour if found in Excel to ByLayer: " + bForm.ExistAttributesColorText());
                    LogFileContents.Add("Change Attribute Colour if NOT found in Excel to red: " + bForm.NotExistAttributesColorText());
                    LogFileContents.Add(" ");
                    LogFileContents.Add("Search in files:");

                    foreach (string FName in Files)
                    {
                        LogFileContents.Add(FName);
                    }
                    LogFileContents.Add(" ");
                    LogFileContents.Add("-----------------------------------------------");
                    LogFileContents.Add("SEARCH RESULTS");
                    LogFileContents.Add("-----------------------------------------------");


                    foreach (string FName in Files)
                    {
                        AttributeHelper.CheckAttribute(FName, block, attrTAG, ref exceldata, ref LogFileContents, ExistAttrColorIndex, NotExistAttrColorIndex, false, false);
                    }
                    if (LogFileContents.Count > 0)
                    {

                        string fFullFileName = bForm.OutFolder() + logName + ".log";

                        try
                        {
                            using (StreamWriter writer = new StreamWriter(fFullFileName))
                            {
                                foreach (string str in LogFileContents)
                                {
                                    writer.WriteLine(str);
                                }
                            }
                        }
                        catch
                        {
                            MessageBox.Show("LogFile write error !", "E R R O R");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Missing Input Data !", "E R R O R");
                }
            }
        }

        /// <summary>
        /// Check if attribute is numeric
        /// </summary>
        [CommandMethod("GroupCheckAttributes", "ChkAttrIsNumeric", null, CommandFlags.Modal, null, "CheckAttributes", "ChkAttr_IsNumeric")]
        public void ChkAttrIsNumericStart()
        {
            Database db = HostApplicationServices.WorkingDatabase;

            ArrayList Blocks = new ArrayList();
            ArrayList BlocksWithLessThanThreeAttribute = new ArrayList();
            ArrayList BlocksWithRepeatingTags = new ArrayList();

            using (Transaction TR = db.TransactionManager.StartTransaction())
            {
                AttributeHelper.ScanForBlocks(db, TR, ref Blocks, ref BlocksWithLessThanThreeAttribute, ref BlocksWithRepeatingTags);

            }

            var Form_ChkAttr_IsNumeric = new FormChkAttrIsNumeric(ref Blocks, ref BlocksWithLessThanThreeAttribute);
            Form_ChkAttr_IsNumeric.ShowDialog();
            if (Form_ChkAttr_IsNumeric.DialogResult == DialogResult.OK)
            {
                string block = Form_ChkAttr_IsNumeric.BlocName();
                string attrTAG = Form_ChkAttr_IsNumeric.AttributeTag();
                string DocName = Application.DocumentManager.MdiActiveDocument.Name;
                int ExistAttrColorIndex = Form_ChkAttr_IsNumeric.ExistAttributesColorIndex();
                int NotExistAttrColorIndex = Form_ChkAttr_IsNumeric.NotExistAttributesColorIndex();
                StringCollection Files = new StringCollection();
                Files.Add(DocName);
                StringCollection LogFileContents = new StringCollection();
                List<StringCollection> exceldata = null;
                if (Form_ChkAttr_IsNumeric.SearchInXLSX())
                    exceldata = AttributeHelper.GetExelData(Form_ChkAttr_IsNumeric.GetExelFileName(), Form_ChkAttr_IsNumeric.SheetName());
                DateTime time = DateTime.Now;
                string logName = "Compare_Attributes_Log_" + String.Format("{0:yyyy/M/d_HH_mm_ss}", time);
                if ((block.Length > 0) && (attrTAG.Length > 0) && (Form_ChkAttr_IsNumeric.OutFolder().Length > 1))
                {
                    if (Form_ChkAttr_IsNumeric.CheckBoxAllFiles())
                    {
                        OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                        dlg.Multiselect = true;
                        dlg.Title = "Select Drawing File(s) ";
                        dlg.Filter = "txt files (*.dwg)|*.dwg|All files (*.*)|*.*";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            foreach (string str in dlg.FileNames)
                            {
                                if (str != DocName)
                                {
                                    Files.Add(str);
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    LogFileContents.Add("-----------------------------------------------");
                    LogFileContents.Add("Log File for: ChkAttr_IsInExcel");
                    LogFileContents.Add("Log File Name: " + Form_ChkAttr_IsNumeric.OutFolder() + logName + ".log");
                    LogFileContents.Add(" ");
                    LogFileContents.Add("Block: " + block);
                    LogFileContents.Add("TAG: " + attrTAG);
                    LogFileContents.Add("EXCEL File: " + Form_ChkAttr_IsNumeric.GetExelFileName());
                    LogFileContents.Add(" ");
                    LogFileContents.Add("Change Attribute Colour if found in Excel to ByLayer: " + Form_ChkAttr_IsNumeric.ExistAttributesColorText());
                    LogFileContents.Add("Change Attribute Colour if NOT found in Excel to red: " + Form_ChkAttr_IsNumeric.NotExistAttributesColorText());
                    LogFileContents.Add(" ");
                    LogFileContents.Add("Search in files:");

                    foreach (string FName in Files)
                    {
                        LogFileContents.Add(FName);
                    }
                    LogFileContents.Add(" ");
                    LogFileContents.Add("-----------------------------------------------");
                    LogFileContents.Add("SEARCH RESULTS");
                    LogFileContents.Add("-----------------------------------------------");


                    foreach (string FName in Files)
                    {
                        AttributeHelper.CheckAttribute(FName, block, attrTAG, ref exceldata, ref LogFileContents, ExistAttrColorIndex, NotExistAttrColorIndex, Form_ChkAttr_IsNumeric.CheckRealNumber(), Form_ChkAttr_IsNumeric.RadioButtonDP());
                    }
                    if (LogFileContents.Count > 0)
                    {

                        string fFullFileName = Form_ChkAttr_IsNumeric.OutFolder() + logName + ".log";

                        try
                        {
                            using (StreamWriter writer = new StreamWriter(fFullFileName))
                            {
                                foreach (string str in LogFileContents)
                                {
                                    writer.WriteLine(str);
                                }
                                //                                writer.Close(); // Do not dispose object on multiple lines. USING statement disposes stream when losing scope.
                            }
                        }
                        catch
                        {
                            // TODO : Resolve ILogger from IoC as class member and use it instead.
                            MessageBox.Show("LogFile write error !", "E R R O R");
                        }
                    }
                }
                else
                {
                    // TODO : Resolve ILogger from IoC as class member and use it instead.
                    MessageBox.Show("Missing Input Data !", "E R R O R");
                }
            }
        }

    }
}
