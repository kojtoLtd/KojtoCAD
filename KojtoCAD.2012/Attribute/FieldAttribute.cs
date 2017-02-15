using System;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
#else
using Teigha.DatabaseServices;
using Teigha.Runtime;
#endif

[assembly : CommandClass(typeof(KojtoCAD.Attribute.FieldAttribute))]

namespace KojtoCAD.Attribute
{
    public class FieldAttribute
    {

        /// <summary>
        /// "Replace field attribute text"
        /// </summary>
        [CommandMethod("FieldAttribute", "FLT", null, CommandFlags.Modal, null, "ReplaceFieldAttributeText", "FieldAttribute")]
        public void ReplaceFieldAttributeTextStart()
        {
            Database db = HostApplicationServices.WorkingDatabase;
                          
            #region Get Layots Names and add to StringCollection layoutNames
            using ( Transaction acTrans = db.TransactionManager.StartTransaction() )
            {
                LayoutManager acLayoutMgr;
                acLayoutMgr = LayoutManager.Current;

                Layout acLayout;
                acLayout = acTrans.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout), OpenMode.ForRead) as Layout;


                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                foreach ( ObjectId btrId in acBlkTbl )
                {
                    BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(btrId, OpenMode.ForRead);
                    if ( btr.IsLayout && btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper() )
                    {
                        Layout lo = (Layout)acTrans.GetObject(btr.LayoutId, OpenMode.ForRead);
                        // acLayoutMgr.CurrentLayout = lo.LayoutName;
                        //----------------------------------
                        // if (lo.LayoutName == acLayout.LayoutName)
                        {
                            foreach ( ObjectId LayoutObjId in btr )
                            {
                                Object Bref = acTrans.GetObject(LayoutObjId, OpenMode.ForWrite) as Object;
                                if ( Bref != null )
                                {
                                    string type = Bref.GetType().ToString();
                                    if ( type == "Autodesk.AutoCAD.DatabaseServices.BlockReference" )
                                    {

                                        for ( int i = 0; i < ( (BlockReference)Bref ).AttributeCollection.Count; i++ )
                                        {
                                            AttributeReference AttRef = (AttributeReference)acTrans.GetObject(( (BlockReference)Bref ).AttributeCollection[i], OpenMode.ForWrite);
                                            //if (AttRef.Tag.ToString().ToUpper() == "DRAWING")
                                            {
                                                //MessageBox.Show(lo.LayoutName + "\n" + ((BlockReference)Bref).Name + "\n" + AttRef.Tag.ToString());
                                                String str = ( (AttributeReference)AttRef ).TextString;
                                                if ( ( AttRef.Tag.ToString().ToUpper() == "DRAWING" ) || ( AttRef.Tag.ToString().ToUpper() == "DRAWING-NR" ) )
                                                {
                                                    str = lo.LayoutName;
                                                }
                                                ( (AttributeReference)AttRef ).TextString = "";
                                                ( (AttributeReference)AttRef ).TextString = str;

                                            }

                                        }
                                    }
                                }
                            }
                        }
                        //--------------------------------------
                    }

                }

                acTrans.Commit();
            }
            #endregion

        }

    }
}
