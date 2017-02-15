using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class BlockSelection : Form
    {
        private string BlockName_;
        public string BlockName { get { return (BlockName_); } set { BlockName_ = value; } }

        public BlockSelection()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            InitializeComponent();

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                List<string> blockList = new List<string>();
                foreach (ObjectId acObjId in acBlkTbl)
                {
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acObjId, OpenMode.ForWrite) as BlockTableRecord;
                    if (acBlkTblRec.IsLayout || acBlkTblRec.IsAnonymous) continue;
                    string recName = acBlkTblRec.Name.ToUpper();
                    blockList.Add(recName);
                    //comboBox_Blocks.Items.Add
                }

                if (blockList.Count() > 0)
                {
                    blockList.Sort();
                    foreach (string str in blockList)
                    {
                        comboBox_Blocks.Items.Add(str);
                    }

                    comboBox_Blocks.SelectedIndex = 0;
                }
                
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            #region Block
            if (comboBox_Blocks.Text != "")
            {
                BlockName_ = comboBox_Blocks.Text.Trim();
            }
            
            #endregion;

            this.DialogResult = DialogResult.OK;
        }
    }
}
