using KojtoCAD.Properties;
using KojtoCAD.Utilities;
using System;
using System.IO;
using System.Windows.Forms;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Application = Bricscad.ApplicationServices.Application;
#endif

[assembly:CommandClass(typeof(KojtoCAD.PointCloud.PointCloud))]

namespace KojtoCAD.PointCloud
{
    public class PointCloud
    {
        /// <summary>
        /// Load pcloud files. 
        /// Load only 1 of 1000 points
        /// </summary>
        [CommandMethod("pcloud")]
        public void PointCloudStart()
        {
            
            LoadPointCloudForm form = new LoadPointCloudForm();
            if ( form.ShowDialog() != DialogResult.OK ) { form.Dispose(); return; }
            form.Hide();

            // OpenFileDialog dlg = new OpenFileDialog();
            int count = 0;
            Database acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            double minX = 153592; double maxX = 163655;
            double minY = 87427; double maxY = 88619;
            double minZ = -500; double maxZ = 5500;
            string[] strARR = { form.FN1, form.FN2, form.FN3, form.FN4, form.FN5 };
            for ( int i = 0; i < 5; i++ )
            {
                if ( strARR[i].Length > 0 )
                {
                    FileStream aFile = new FileStream(strARR[i], FileMode.Open);
                    StreamReader sr = new StreamReader(aFile);
                    string strLine;
                    string[] strArr;
                    using ( Transaction acTrans = acCurDb.TransactionManager.StartTransaction() )
                    {
                        // Open the Block table for read
                        BlockTable acBlkTbl;
                        acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                        // Open the Block table record Model space for write
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        strLine = sr.ReadLine();
                        while ( strLine != null )
                        {
                            strArr = strLine.Split(';');
                            double x = Convert.ToDouble(strArr[0]);
                            double y = Convert.ToDouble(strArr[1]);
                            double z = Convert.ToDouble(strArr[2]);

                            if ( ( ( x >= minX ) && ( x <= maxX ) ) && ( ( y >= minY ) && ( y <= maxY ) ) && ( ( z >= minZ ) && ( z <= maxZ ) ) )
                            {
                                if ( count == 0 )
                                {
                                    DBPoint acPoint = new DBPoint(new Point3d(x, y, z));

                                    acPoint.SetDatabaseDefaults();
                                    acBlkTblRec.AppendEntity(acPoint);
                                    acTrans.AddNewlyCreatedDBObject(acPoint, true);

                                    // Set the style for all point objects in the drawing
                                    acCurDb.Pdmode = 34;
                                    acCurDb.Pdsize = 1;
                                }
                                count++;
                            }
                            if ( count == 100 )
                            {
                                count = 0;
                            }

                            strLine = sr.ReadLine();
                        }

                        // Save the new object to the database
                        acTrans.Commit();
                    }
                    sr.Close();
                }
            }
        }
    }
}
