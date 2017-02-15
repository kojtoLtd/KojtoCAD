using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D;

using KojtoCAD.KojtoCAD3D.UtilityClasses;
using KojtoCAD.KojtoCAD3D.WorkClasses;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
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
[assembly: CommandClass(typeof(GeometryToCsv))]

namespace KojtoCAD.KojtoCAD3D
{
    public class GeometryToCsv
    {
        public Containers container = ContextVariablesProvider.Container;

        public List<Pair<string, List<Pair<string, Point3d>>>> NamedPointsARRAYs =
            new List<Pair<string, List<Pair<string, Point3d>>>>();

        #region Nodes
        [CommandMethod("KojtoCAD_3D", "KCAD_NODE_NUMER_POSITION_CSV", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CREATE_NODE_NUMER_POSITION_CSV.htm", "")]
        public void KojtoCAD_3D_Node_Numer_And_Position_To_Csv_File()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("Pereferial");
                pKeyOpts.Keywords.Add("NoPereferial");
                pKeyOpts.Keywords.Add("Fictive");
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.Keywords.Default = "All";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "NoPereferial": KojtoCAD_3D_NoPereferial_Nodes_Numer_And_Position_To_Csv_File(); break;
                        case "Pereferial": KojtoCAD_3D_Pereferial_Nodes_Numer_And_Position_To_Csv_File(); break;
                        case "Fictive": KojtoCAD_3D_Fictive_Nodes_Numer_And_Position_To_Csv_File(); break;
                        case "All":
                            KojtoCAD_3D_All_Nodes_Numer_And_Position_To_Csv_File(
                                ConstantsAndSettings.CSV_Node_Columns[0],
                                ConstantsAndSettings.CSV_Node_Columns[1],
                                ConstantsAndSettings.CSV_Node_Columns[2],
                                ConstantsAndSettings.CSV_Node_Columns[3],
                                ConstantsAndSettings.CSV_Node_Columns[4],
                                ConstantsAndSettings.CSV_Node_Columns[5],
                                ConstantsAndSettings.CSV_Node_Columns[13],
                                ConstantsAndSettings.CSV_Node_Columns[12],
                                ConstantsAndSettings.CSV_Node_Columns[6],
                                ConstantsAndSettings.CSV_Node_Columns[7],
                                ConstantsAndSettings.CSV_Node_Columns[8],
                                ConstantsAndSettings.CSV_Node_Columns[9],
                                ConstantsAndSettings.CSV_Node_Columns[10],
                                ConstantsAndSettings.CSV_Node_Columns[11]);
                            break;
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void KojtoCAD_3D_All_Nodes_Numer_And_Position_To_Csv_File(bool pereferial, bool fictive, bool normals, bool normalsByNoFictiveBends,
            bool sortedBendsByNumer, bool sortedTrianglesByNumer, bool trCounterclockwise, bool bendCounterclockwise, bool torsion, bool cncTorsion,
            bool angleBetweenNodeAndBend, bool cnc_angleBetweenNodeAndBend, bool angleBetweenNodeAndTangentPlane, bool angleBetweenNodeAndCNCTangentPlane)
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
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {
                            #region first line
                            string first = "Joint Numer ;";
                            if (pereferial)
                                first += " Position Type ;";
                            if (fictive)
                                first += " Type ;";

                            if (sortedBendsByNumer)
                                first += " Bends List by Number ;";

                            if (sortedTrianglesByNumer)
                                first += " Triangles List by Number ;";

                            first += " Global X ; Global Y ; Global Z ;";

                            if (normals)
                                first += " Normal X ; Normal Y ; Normal Z ;";

                            if (normalsByNoFictiveBends)
                                first += " cncNormal X ; cncNormal Y ; cncNormal Z ; angle between Normals [degree] ; Distance between Normals ;";

                            if (angleBetweenNodeAndBend)
                                first += " Node/Bend Angle ;";

                            if (cnc_angleBetweenNodeAndBend)
                                first += " cncNode/Bend Angle ;";

                            if (angleBetweenNodeAndTangentPlane)
                                first += " Plane/Bend Angle ;";

                            if (angleBetweenNodeAndCNCTangentPlane)
                                first += " cncPlane/Bend Angle ;";

                            if (torsion)
                                first += " Torsion ;";

                            if (cncTorsion)
                                first += " cncTorsion ;";

                            if (bendCounterclockwise)
                                first += " Bends by Counterclockwise ;";

                            if (trCounterclockwise)
                                first += " Triangles by Counterclockwise ;";

                            #endregion

                            sw.WriteLine(first);
                            foreach (WorkClasses.Node Node in container.Nodes)
                            {
                                quaternion tempNodeNormal = Node.GetNodesNormalsByNoFictiveBends(ref container);

                                List<int> bndNumbers = new List<int>();
                                foreach (int bN in Node.Bends_Numers_Array) { bndNumbers.Add(bN); }
                                //Comparer<int> defComp = Comparer<int>.Default;
                                bndNumbers.Sort();

                                //string line = string.Format("{0};{1:f5};{2:f5};{3:f5};", Node.Numer + 1, Node.Position.GetX(), Node.Position.GetY(), Node.Position.GetZ());
                                string line = string.Format("{0};", Node.Numer + 1);
                                #region pereferial
                                if (pereferial)
                                {
                                    bool PEREFERIAL = false;
                                    foreach (int nN in Node.Bends_Numers_Array)
                                    {
                                        if (container.Bends[nN].IsPeripheral())
                                        {
                                            PEREFERIAL = true;
                                            break;
                                        }
                                    }

                                    line += (PEREFERIAL) ? " Pereferial ;" : " NoPereferial ;";
                                }
                                #endregion

                                bool FICTIVE = true;
                                #region fictive
                                //if (fictive)
                                {
                                    foreach (int nN in Node.Bends_Numers_Array)
                                    {
                                        if (!container.Bends[nN].IsFictive())
                                        {
                                            FICTIVE = false;
                                            break;
                                        }
                                    }
                                    if (fictive)
                                        line += (FICTIVE) ? " Fictive ;" : " NoFictive ;";
                                }
                                #endregion

                                #region sortedBendsByNumer
                                if (sortedBendsByNumer)
                                {
                                    bndNumbers.Sort();
                                    string temp = " ";

                                    for (int i = 0; i < bndNumbers.Count; i++)
                                    {
                                        string fic = "";
                                        if (container.Bends[bndNumbers[i]].IsFictive())
                                            fic += " fictive ";

                                        if (i == bndNumbers.Count - 1)
                                            temp += (bndNumbers[i] + 1) + fic + ";";
                                        else
                                            temp += (bndNumbers[i] + 1) + fic + ",";
                                    }
                                    line += temp;
                                }
                                #endregion

                                #region sortedTrianglesByNumer
                                if (sortedTrianglesByNumer)
                                {
                                    List<int> trNumbers = new List<int>();
                                    foreach (int tN in Node.Triangles_Numers_Array) { trNumbers.Add(tN); }
                                    //Comparer<int> defComp = Comparer<int>.Default;
                                    trNumbers.Sort();
                                    string temp = " ";

                                    for (int i = 0; i < trNumbers.Count; i++)
                                    {
                                        if (i == trNumbers.Count - 1)
                                            temp += (trNumbers[i] + 1) + ";";
                                        else
                                            temp += (trNumbers[i] + 1) + ",";
                                    }
                                    line += temp;

                                }
                                #endregion

                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ; ", Node.Position.GetX(), Node.Position.GetY(), Node.Position.GetZ());

                                #region normals
                                if (normals)
                                {
                                    quaternion nQ = Node.Normal - Node.Position;
                                    nQ /= nQ.abs();
                                    nQ *= 60.0;
                                    nQ += Node.Position;
                                    line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", nQ.GetX(), nQ.GetY(), nQ.GetZ());
                                }
                                #endregion

                                #region normalsByNoFictiveBends
                                if ((normalsByNoFictiveBends) && (!FICTIVE))
                                {
                                    quaternion nq = Node.Normal - Node.Position;
                                    quaternion nQ = Node.GetNodesNormalsByNoFictiveBends(ref container);
                                    nQ -= Node.Position;

                                    double ang = nq.angTo(nQ);
                                    ang *= (180.0 / Math.PI);
                                    double dist = (nQ - nq).abs();

                                    nQ /= nQ.abs();
                                    nQ *= 60.0;
                                    nQ += Node.Position;
                                    line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ; {3:f5} ; {4:f5} ;", nQ.GetX(), nQ.GetY(), nQ.GetZ(), ang, dist);
                                }
                                #endregion

                                #region angleBetweenNodeAndBend
                                if (angleBetweenNodeAndBend)
                                {
                                    for (int i = 0; i < bndNumbers.Count; i++)
                                    {
                                        int N = bndNumbers[i];
                                        quaternion Q = container.Bends[N].MidPoint - Node.Position;
                                        double ang = Q.angTo(Node.Normal - Node.Position);
                                        if (i == bndNumbers.Count - 1)
                                            line += string.Format(" {0:f5} ; ", ang * 180.0 / Math.PI);
                                        else
                                            line += string.Format(" {0:f5} , ", ang * 180.0 / Math.PI);
                                    }
                                }
                                #endregion

                                #region cnc angleBetweenNodeAndBend
                                if (cnc_angleBetweenNodeAndBend)
                                {
                                    for (int i = 0; i < bndNumbers.Count; i++)
                                    {
                                        if (!container.Bends[bndNumbers[i]].IsFictive())
                                        {
                                            int N = bndNumbers[i];
                                            quaternion Q = container.Bends[N].MidPoint - Node.Position;
                                            double ang = Q.angTo(tempNodeNormal - Node.Position);
                                            if (i == bndNumbers.Count - 1)
                                                line += string.Format(" {0:f5} ; ", ang * 180.0 / Math.PI);
                                            else
                                                line += string.Format(" {0:f5} , ", ang * 180.0 / Math.PI);
                                        }
                                        else
                                        {
                                            if (i == bndNumbers.Count - 1)
                                                line += string.Format(" {0} ; ", "Fictive");
                                            else
                                                line += string.Format(" {0} ,", "Fictive");
                                        }
                                    }
                                }
                                #endregion

                                #region angleBetweenNodeAndTangentPlane
                                if (angleBetweenNodeAndTangentPlane)
                                {
                                    for (int i = 0; i < bndNumbers.Count; i++)
                                    {
                                        int N = bndNumbers[i];
                                        quaternion Q = container.Bends[N].MidPoint - Node.Position;
                                        double ang = Math.PI / 2.0 - Q.angTo(Node.Normal - Node.Position);
                                        if (i == bndNumbers.Count - 1)
                                            line += string.Format(" {0:f5} ; ", ang * 180.0 / Math.PI);
                                        else
                                            line += string.Format(" {0:f5} , ", ang * 180.0 / Math.PI);
                                    }
                                }
                                #endregion

                                #region angleBetweenNodeAndCNCTangentPlane
                                if (angleBetweenNodeAndCNCTangentPlane)
                                {
                                    for (int i = 0; i < bndNumbers.Count; i++)
                                    {
                                        if (!container.Bends[bndNumbers[i]].IsFictive())
                                        {
                                            int N = bndNumbers[i];
                                            quaternion Q = container.Bends[N].MidPoint - Node.Position;
                                            double ang = Math.PI / 2.0 - Q.angTo(tempNodeNormal - Node.Position);
                                            if (i == bndNumbers.Count - 1)
                                                line += string.Format(" {0:f5} ; ", ang * 180.0 / Math.PI);
                                            else
                                                line += string.Format(" {0:f5} , ", ang * 180.0 / Math.PI);
                                        }
                                        else
                                        {
                                            if (i == bndNumbers.Count - 1)
                                                line += string.Format(" {0} ; ", "Fictive");
                                            else
                                                line += string.Format(" {0} ,", "Fictive");
                                        }
                                    }
                                }
                                #endregion

                                #region torsion
                                if (torsion)
                                {
                                    for (int i = 0; i < bndNumbers.Count; i++)
                                    {
                                        int N = bndNumbers[i];
                                        double angTorsion = 0.0;
                                        Bend bend = container.Bends[N];
                                        UCS nUCS = new UCS(Node.Position, bend.MidPoint, Node.Normal);
                                        quaternion qNormal = nUCS.FromACS(bend.Normal);
                                        quaternion qMidPoint = nUCS.FromACS(bend.MidPoint);
                                        if (qNormal.GetZ() != 0.0)
                                        {
                                            qNormal = qNormal - qMidPoint;
                                            qNormal /= qNormal.abs();
                                            qNormal *= 10000.0;

                                            angTorsion = Math.PI / 2.0 - qNormal.angToZ();

                                        }
                                        if (i == bndNumbers.Count - 1)
                                            line += string.Format(" {0:f5} ;", angTorsion * 180.0 / Math.PI);
                                        else
                                            line += string.Format(" {0:f5}, ", angTorsion * 180.0 / Math.PI);
                                    }
                                }
                                #endregion

                                #region cncTorsion
                                if (cncTorsion)
                                {
                                    for (int i = 0; i < bndNumbers.Count; i++)
                                    {
                                        if (!container.Bends[bndNumbers[i]].IsFictive())
                                        {
                                            int N = bndNumbers[i];
                                            double angTorsion = 0.0;
                                            Bend bend = container.Bends[N];
                                            UCS nUCS = new UCS(Node.Position, bend.MidPoint, tempNodeNormal);
                                            quaternion qNormal = nUCS.FromACS(bend.Normal);
                                            quaternion qMidPoint = nUCS.FromACS(bend.MidPoint);
                                            if (qNormal.GetZ() != 0.0)
                                            {
                                                qNormal = qNormal - qMidPoint;
                                                qNormal /= qNormal.abs();
                                                qNormal *= 10000.0;

                                                angTorsion = Math.PI / 2.0 - qNormal.angToZ();

                                            }
                                            if (i == bndNumbers.Count - 1)
                                                line += string.Format(" {0:f5} ;", angTorsion * 180.0 / Math.PI);
                                            else
                                                line += string.Format(" {0:f5}, ", angTorsion * 180.0 / Math.PI);
                                        }
                                        else
                                        {
                                            if (i == bndNumbers.Count - 1)
                                                line += string.Format(" {0} ; ", "Fictive");
                                            else
                                                line += string.Format(" {0} ,", "Fictive");
                                        }

                                    }
                                }
                                #endregion

                                #region  trCounterclockwise
                                if ((trCounterclockwise) || (bendCounterclockwise))
                                {

                                    if (Node.Triangles_Numers_Array.Count > 1)
                                    {

                                        List<int> trArangeList = Node.ArangeTriangles(ref container.Triangles, ref container.Bends);

                                        if (bendCounterclockwise)
                                        {
                                            string temp = " ";

                                            Triangle trr = container.Triangles[trArangeList[0]];
                                            Triangle TRR = container.Triangles[trArangeList[1]];
                                            int bN = -1;
                                            Triangle.IsContiguous(trr, TRR, ref bN);
                                            Bend otherBend = container.Bends[trr.GetFirstBendNumer()];
                                            if (!(otherBend.Numer != bN && (otherBend.StartNodeNumer == Node.Numer || otherBend.EndNodeNumer == Node.Numer)))
                                            {
                                                otherBend = container.Bends[trr.GetSecondBendNumer()];
                                                if (!(otherBend.Numer != bN && (otherBend.StartNodeNumer == Node.Numer || otherBend.EndNodeNumer == Node.Numer)))
                                                    otherBend = container.Bends[trr.GetThirdBendNumer()];
                                            }

                                            int firstNumer = otherBend.Numer;
                                            temp += (otherBend.Numer + 1) + ",";
                                            temp += (bN + 1) + ",";

                                            for (int i = 1; i < trArangeList.Count - 1; i++)
                                            {
                                                int j = i + 1;
                                                Triangle tR = container.Triangles[trArangeList[i]];
                                                Triangle Tr = container.Triangles[trArangeList[j]];
                                                bN = -1;
                                                Triangle.IsContiguous(tR, Tr, ref bN);
                                                temp += (bN + 1) + ",";
                                            }
                                            trr = container.Triangles[trArangeList[trArangeList.Count - 1]];
                                            otherBend = container.Bends[trr.GetFirstBendNumer()];
                                            if (!(otherBend.Numer != bN && (otherBend.StartNodeNumer == Node.Numer || otherBend.EndNodeNumer == Node.Numer)))
                                            {
                                                otherBend = container.Bends[trr.GetSecondBendNumer()];
                                                if (!(otherBend.Numer != bN && (otherBend.StartNodeNumer == Node.Numer || otherBend.EndNodeNumer == Node.Numer)))
                                                    otherBend = container.Bends[trr.GetThirdBendNumer()];
                                            }
                                            if (otherBend.Numer != firstNumer)
                                                temp += (otherBend.Numer + 1).ToString();

                                            line += temp;
                                            line += ";";
                                        }

                                        if (trCounterclockwise)
                                        {
                                            string temp = " ";
                                            for (int i = 0; i < trArangeList.Count; i++)
                                            {
                                                if (i == trArangeList.Count - 1)
                                                    temp += (trArangeList[i] + 1) + ";";
                                                else
                                                    temp += (trArangeList[i] + 1) + ",";
                                            }
                                            line += temp;
                                        }
                                    }
                                    else
                                    {
                                        if (bendCounterclockwise)
                                        {
                                            Triangle TR = container.Triangles[Node.Triangles_Numers_Array[0]];
                                            UCS ucs = new UCS(Node.Position, TR.GetCentroid(), container.Bends[Node.Bends_Numers_Array[0]].MidPoint);
                                            if (ucs.FromACS(Node.Normal).GetZ() < 0)
                                                line += string.Format(" {0},{1} ;", Node.Bends_Numers_Array[0] + 1, Node.Bends_Numers_Array[1] + 1);
                                            else
                                                line += string.Format(" {0},{1} ;", Node.Bends_Numers_Array[1] + 1, Node.Bends_Numers_Array[0] + 1);
                                        }

                                        if (trCounterclockwise)
                                        {
                                            line += string.Format(" {0} ;", Node.Triangles_Numers_Array[0] + 1);
                                        }
                                    }
                                }
                                #endregion

                                sw.WriteLine(line);
                            }

                            sw.Flush();
                            sw.Close();
                        }
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void KojtoCAD_3D_NoPereferial_Nodes_Numer_And_Position_To_Csv_File()
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
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {
                            string first = "Joint Numer ; Global X; Global Y; Global Z;";
                            sw.WriteLine(first);
                            foreach (var Node in container.Nodes)
                            {
                                bool pereferial = false;
                                foreach (int bN in Node.Bends_Numers_Array)
                                {
                                    if (container.Bends[bN].IsPeripheral())
                                    {
                                        pereferial = true;
                                        break;
                                    }
                                }

                                if (!pereferial)
                                {
                                    string line = string.Format("{0};{1:f5};{2:f5};{3:f5}", Node.Numer + 1, Node.Position.GetX(), Node.Position.GetY(), Node.Position.GetZ());
                                    sw.WriteLine(line);
                                }
                            }

                            sw.Flush();
                            sw.Close();
                        }
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void KojtoCAD_3D_Pereferial_Nodes_Numer_And_Position_To_Csv_File()
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
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {
                            string first = "Joint Numer ; Global X; Global Y; Global Z;";
                            sw.WriteLine(first);
                            foreach (var Node in container.Nodes)
                            {
                                bool pereferial = false;
                                foreach (int bN in Node.Bends_Numers_Array)
                                {
                                    if (container.Bends[bN].IsPeripheral())
                                    {
                                        pereferial = true;
                                        break;
                                    }
                                }

                                if (pereferial)
                                {
                                    string line = string.Format("{0};{1:f5};{2:f5};{3:f5}", Node.Numer + 1, Node.Position.GetX(), Node.Position.GetY(), Node.Position.GetZ());
                                    sw.WriteLine(line);
                                }
                            }

                            sw.Flush();
                            sw.Close();
                        }
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void KojtoCAD_3D_Fictive_Nodes_Numer_And_Position_To_Csv_File()
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
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {
                            string first = "Joint Numer ; Global X; Global Y; Global Z;";
                            sw.WriteLine(first);
                            foreach (var Node in container.Nodes)
                            {
                                bool fictive = true;
                                foreach (int bN in Node.Bends_Numers_Array)
                                {
                                    if (!container.Bends[bN].IsFictive())
                                    {
                                        fictive = false;
                                        break;
                                    }
                                }

                                if (fictive)
                                {
                                    string line = string.Format("{0};{1:f5};{2:f5};{3:f5}", Node.Numer + 1, Node.Position.GetX(), Node.Position.GetY(), Node.Position.GetZ());
                                    sw.WriteLine(line);
                                }
                            }

                            sw.Flush();
                            sw.Close();
                        }

                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Bends
        [CommandMethod("KojtoCAD_3D", "KCAD_BEND_JOINTS_NUMERS_CSV", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/BEND_JOINTS_NUMERS_CSV.htm", "")]
        public void KojtoCAD_3D_Bend_Joints_Numers()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("Pereferial");
                pKeyOpts.Keywords.Add("NoPereferial");
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.Keywords.Default = "All";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "NoPereferial": if (!GlobalFunctions.PromptYesOrNo("Write Type (Pereferial or NoPereferial) ? "))
                                if (!GlobalFunctions.PromptYesOrNo("Write Type (Fictive or NoFictive) ? "))
                                    if (!GlobalFunctions.PromptYesOrNo("Write Length ? "))
                                        KojtoCAD_3D_NoPereferial_Bends_Joints_Numers(false);
                                    else
                                        KojtoCAD_3D_NoPereferial_Bends_Joints_Numers(false, false, true);
                                else
                                    if (!GlobalFunctions.PromptYesOrNo("Write Length ? "))
                                        KojtoCAD_3D_NoPereferial_Bends_Joints_Numers(false, true, false);
                                    else
                                        KojtoCAD_3D_NoPereferial_Bends_Joints_Numers(false, true, true);
                            else
                                if (!GlobalFunctions.PromptYesOrNo("Write Type (Fictive or NoFictive) ? "))
                                    if (!GlobalFunctions.PromptYesOrNo("Write Length ? "))
                                        KojtoCAD_3D_NoPereferial_Bends_Joints_Numers(true);
                                    else
                                        KojtoCAD_3D_NoPereferial_Bends_Joints_Numers(true, false, true);
                                else
                                    if (!GlobalFunctions.PromptYesOrNo("Write Length ? "))
                                        KojtoCAD_3D_NoPereferial_Bends_Joints_Numers(true, true, false);
                                    else
                                        KojtoCAD_3D_NoPereferial_Bends_Joints_Numers(true, true, true);
                            break;

                        case "Pereferial": if (!GlobalFunctions.PromptYesOrNo("Write Length ? "))
                                KojtoCAD_3D_Pereferial_Bends_Joints_Numers();
                            else
                                KojtoCAD_3D_Pereferial_Bends_Joints_Numers(true);
                            break;
                        case "All": KojtoCAD_3D_All_Bends_Joints_Numers(
                            ConstantsAndSettings.CSV_Bend_Columns[0],
                            ConstantsAndSettings.CSV_Bend_Columns[1],
                            ConstantsAndSettings.CSV_Bend_Columns[5],
                            ConstantsAndSettings.CSV_Bend_Columns[2],
                            ConstantsAndSettings.CSV_Bend_Columns[3],
                            ConstantsAndSettings.CSV_Bend_Columns[4],
                            ConstantsAndSettings.CSV_Bend_Columns[6],
                            ConstantsAndSettings.CSV_Bend_Columns[7],
                            ConstantsAndSettings.CSV_Bend_Columns[8],
                            ConstantsAndSettings.CSV_Bend_Columns[9],
                            ConstantsAndSettings.CSV_Bend_Columns[10]
                                                                        );
                            break;
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void KojtoCAD_3D_All_Bends_Joints_Numers(bool pereferial, bool type, bool length, bool convex, bool nodes, bool triangles,
            bool trNormalsAngle, bool startingPointOfBendNormal, bool endPointOfBendNormal, bool bendStartPoint, bool bendEndPoint)
        {
            if (container != null)
            {
                if (container.Bends.Count > 0)
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                    dlg.Title = "Enter CSV File Name ";
                    dlg.DefaultExt = "csv";
                    dlg.FileName = "*.csv";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = dlg.FileName;
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {
                            #region first line (caption)
                            string first = "Frame Numer ;";
                            if (nodes)
                                first += "Joint I;  Joint II;";
                            if (triangles)
                                first += "Triangle I;  Triangle II;";
                            if (pereferial)
                                first += "  Position Type ;";
                            if (type)
                                first += " Type ;";
                            if (convex)
                                first += " Convex or Concave ;";
                            if (length)
                                first += " Length ;";
                            if (trNormalsAngle)
                                first += " Angle - TrNormals ;";
                            if (startingPointOfBendNormal)
                                first += " normalStart X ; normalStart Y ; normalStart Z ;";
                            if (endPointOfBendNormal)
                                first += " normalEnd X ; normalEnd Y ; normalEnd Z ;";
                            if (bendStartPoint)
                                first += " bendStart X ; bendStart Y ; bendStart Z ;";
                            if (bendEndPoint)
                                first += " bendEnd X ; bendEnd Y ; bendEnd Z ;";

                            sw.WriteLine(first);
                            #endregion
                            foreach (Bend Bend in container.Bends)
                            {
                                int f = Bend.StartNodeNumer + 1;
                                int s = Bend.EndNodeNumer + 1;

                                string line = "";
                                line = string.Format(" {0} ;", Bend.Numer + 1);

                                #region nodes
                                if (nodes)
                                    if (f < s)
                                    {
                                        line += string.Format(" {0} ; {1} ;", f, s);
                                    }
                                    else
                                    {
                                        line += string.Format(" {0} ; {1} ;", s, f);
                                    }
                                #endregion

                                #region triangles
                                f = Bend.FirstTriangleNumer;
                                s = Bend.SecondTriangleNumer;
                                if (s < f) { int buff = f; f = s; s = buff; }
                                if (triangles)
                                {
                                    if (f >= 0)
                                        line += string.Format(" {0} ; {1} ;", f + 1, s + 1);
                                    else
                                        line += string.Format(" {0} ; {1} ;", "", s + 1);
                                }
                                #endregion

                                bool concave = false;
                                #region pereferial , fictive , convex, length
                                if (pereferial)
                                    line += ((Bend.IsPeripheral()) ? " Pereferial ;" : " NoPereferial ;");
                                if (type)
                                    line += ((Bend.IsFictive()) ? " Fictive ;" : " NoFictive ;");
                                if (convex)
                                {
                                    UCS ucsC = Bend.GetUCS();
                                    quaternion tQ = ucsC.FromACS(container.Triangles[Bend.FirstTriangleNumer].GetCentroid());
                                    if (tQ.GetY() > 0.0)
                                    {
                                        line += " concave ;";
                                        concave = true;
                                    }
                                    else
                                        if (tQ.GetY() < 0.0)
                                            line += " convex ;";
                                        else
                                            line += " planar ;";
                                }
                                if (length)
                                    line += string.Format(" {0:f5} ;", Bend.Length);
                                #endregion

                                #region Angle between triangles normals
                                if (trNormalsAngle && !Bend.IsPeripheral())
                                {
                                    Pair<quaternion, quaternion> tQ1 = container.Triangles[Bend.FirstTriangleNumer].Normal;
                                    Pair<quaternion, quaternion> tQ2 = container.Triangles[Bend.SecondTriangleNumer].Normal;
                                    quaternion ttQ1 = tQ1.Second - tQ1.First;
                                    quaternion ttQ2 = tQ2.Second - tQ2.First;

                                    double ang = ttQ1.angTo(ttQ2);
                                    if (concave)
                                        ang = -ang;
                                    ang *= (180.0 / Math.PI);

                                    line += string.Format(" {0:f5} ;", ang);
                                }
                                #endregion

                                #region normal start, end
                                if (startingPointOfBendNormal)
                                {
                                    line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", Bend.MidPoint.GetX(), Bend.MidPoint.GetY(), Bend.MidPoint.GetZ());
                                }

                                if (endPointOfBendNormal)
                                {
                                    line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", Bend.Normal.GetX(), Bend.Normal.GetY(), Bend.Normal.GetZ());
                                }
                                #endregion

                                #region bend start, end
                                if (bendStartPoint)
                                {
                                    line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", Bend.Start.GetX(), Bend.Start.GetY(), Bend.Start.GetZ());
                                }

                                if (bendEndPoint)
                                {
                                    line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", Bend.End.GetX(), Bend.End.GetY(), Bend.End.GetZ());
                                }
                                #endregion

                                sw.WriteLine(line);

                            }

                            sw.Flush();
                            sw.Close();
                        }
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void KojtoCAD_3D_NoPereferial_Bends_Joints_Numers(bool pereferial, bool type = false, bool length = false)
        {
            if (container != null)
            {
                if (container.Bends.Count > 0)
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                    dlg.Title = "Enter CSV File Name ";
                    dlg.DefaultExt = "csv";
                    dlg.FileName = "*.csv";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = dlg.FileName;
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {
                            #region first line (caption)
                            string first = "Frame Numer ; Joint I;  Joint II;";
                            if (pereferial)
                                first += "  Position Type ;";
                            if (type)
                                first += " Type ;";
                            if (length)
                                first += " Length ;";

                            sw.WriteLine(first);
                            #endregion
                            foreach (Bend Bend in container.Bends)
                            {
                                if (!Bend.IsPeripheral())
                                {
                                    int f = Bend.StartNodeNumer + 1;
                                    int s = Bend.EndNodeNumer + 1;

                                    string line = "";
                                    if (f < s)
                                    {
                                        line = string.Format("{0};{1};{2};", Bend.Numer + 1, f, s);
                                    }
                                    else
                                    {
                                        line = string.Format("{0};{1};{2};", Bend.Numer + 1, s, f);
                                    }

                                    if (pereferial)
                                        line += ((Bend.IsPeripheral()) ? " Pereferial ;" : " NoPereferial ;");
                                    if (type)
                                        line += ((Bend.IsFictive()) ? " Fictive ;" : " NoFictive ;");
                                    if (length)
                                        line += string.Format(" {0:f5} ;", Bend.Length);

                                    sw.WriteLine(line);
                                }
                            }

                            sw.Flush();
                            sw.Close();
                        }
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void KojtoCAD_3D_Pereferial_Bends_Joints_Numers(bool length = false)
        {
            if (container != null)
            {
                if (container.Bends.Count > 0)
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                    dlg.Title = "Enter CSV File Name ";
                    dlg.DefaultExt = "csv";
                    dlg.FileName = "*.csv";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = dlg.FileName;
                        using (StreamWriter sw = new StreamWriter(fileName))
                        {

                            #region first line
                            if (!length)
                            {
                                string first = "Frame Numer ; Joint I;  Joint II;";
                                sw.WriteLine(first);
                            }
                            else
                            {
                                string first = "Frame Numer ; Joint I;  Joint II; Length ;";
                                sw.WriteLine(first);
                            }
                            #endregion

                            foreach (Bend Bend in container.Bends)
                            {
                                if (Bend.IsPeripheral())
                                {
                                    int f = Bend.StartNodeNumer + 1;
                                    int s = Bend.EndNodeNumer + 1;
                                    if (f < s)
                                    {
                                        #region f < s
                                        if (!length)
                                        {
                                            string line = string.Format("{0};{1};{2};", Bend.Numer + 1, f, s);
                                            sw.WriteLine(line);
                                        }
                                        else
                                        {
                                            string line = string.Format("{0};{1};{2};{3:f5}", Bend.Numer + 1, f, s, Bend.Length);
                                            sw.WriteLine(line);
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region f > s
                                        if (!length)
                                        {
                                            string line = string.Format("{0};{1};{2};", Bend.Numer + 1, s, f);
                                            sw.WriteLine(line);
                                        }
                                        else
                                        {
                                            string line = string.Format("{0};{1};{2};{3};", Bend.Numer + 1, s, f, Bend.Length);
                                            sw.WriteLine(line);
                                        }
                                        #endregion
                                    }
                                }
                            }

                            sw.Flush();
                            sw.Close();
                        }
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("\nData Base Empty !\n\nMissing Nodes !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Triangles
        [CommandMethod("KojtoCAD_3D", "KCAD_TRIANGLES_CSV", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/TRIANGLES_CSV_FILE.htm", "")]
        public void KojtoCAD_3D_Triangles_Csv_File()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
                pKeyOpts.Message = "\nEnter an option ";
                pKeyOpts.Keywords.Add("Pereferial");
                pKeyOpts.Keywords.Add("NoPereferial");
                pKeyOpts.Keywords.Add("NoFictive");
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.Keywords.Default = "All";
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.OK)
                {
                    switch (pKeyRes.StringResult)
                    {
                        case "NoPereferial": KojtoCAD_3D_NoPereferial_Triangles(
                           ConstantsAndSettings.CSV_Triangle_Columns[0],
                            ConstantsAndSettings.CSV_Triangle_Columns[1],
                             ConstantsAndSettings.CSV_Triangle_Columns[2],
                              ConstantsAndSettings.CSV_Triangle_Columns[3],
                               ConstantsAndSettings.CSV_Triangle_Columns[4],
                                ConstantsAndSettings.CSV_Triangle_Columns[5],
                                 ConstantsAndSettings.CSV_Triangle_Columns[6],
                                  ConstantsAndSettings.CSV_Triangle_Columns[7],
                                  ConstantsAndSettings.CSV_Triangle_Columns[8]
                           );
                            break;
                        case "Pereferial": KojtoCAD_3D_Pereferial_Triangles(
                            ConstantsAndSettings.CSV_Triangle_Columns[0],
                             ConstantsAndSettings.CSV_Triangle_Columns[1],
                              ConstantsAndSettings.CSV_Triangle_Columns[2],
                               ConstantsAndSettings.CSV_Triangle_Columns[3],
                                ConstantsAndSettings.CSV_Triangle_Columns[4],
                                 ConstantsAndSettings.CSV_Triangle_Columns[5],
                                  ConstantsAndSettings.CSV_Triangle_Columns[6],
                                   ConstantsAndSettings.CSV_Triangle_Columns[7],
                                   ConstantsAndSettings.CSV_Triangle_Columns[8]
                            );
                            break;
                        case "NoFictive": KojtoCAD_3D_NoFictive_Triangles(
                           ConstantsAndSettings.CSV_Triangle_Columns[0],
                            ConstantsAndSettings.CSV_Triangle_Columns[1],
                             ConstantsAndSettings.CSV_Triangle_Columns[2],
                              ConstantsAndSettings.CSV_Triangle_Columns[3],
                               ConstantsAndSettings.CSV_Triangle_Columns[4],
                                ConstantsAndSettings.CSV_Triangle_Columns[5],
                                 ConstantsAndSettings.CSV_Triangle_Columns[6],
                                  ConstantsAndSettings.CSV_Triangle_Columns[7],
                                  ConstantsAndSettings.CSV_Triangle_Columns[8]
                           );
                            break;
                        case "All": KojtoCAD_3D_All_Triangles(
                            ConstantsAndSettings.CSV_Triangle_Columns[0],
                             ConstantsAndSettings.CSV_Triangle_Columns[1],
                              ConstantsAndSettings.CSV_Triangle_Columns[2],
                               ConstantsAndSettings.CSV_Triangle_Columns[3],
                                ConstantsAndSettings.CSV_Triangle_Columns[4],
                                 ConstantsAndSettings.CSV_Triangle_Columns[5],
                                  ConstantsAndSettings.CSV_Triangle_Columns[6],
                                   ConstantsAndSettings.CSV_Triangle_Columns[7],
                                    ConstantsAndSettings.CSV_Triangle_Columns[8]
                            );
                            break;
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void KojtoCAD_3D_All_Triangles(bool pereferial, bool fictive, bool bends, bool nodes, bool area, bool massCenter, bool normal, bool angles, bool lengths)
        {
            if ((container != null) && (container.Triangles.Count > 0))
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                dlg.Title = "Enter CSV File Name ";
                dlg.DefaultExt = "csv";
                dlg.FileName = "*.csv";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dlg.FileName;
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        #region First Line
                        string LINE = " trNumer ;";
                        if (pereferial)
                            LINE += "Position Type ;";
                        if (fictive)
                            LINE += " Type ;";
                        if (bends)
                            LINE += " Bends ;";
                        if (lengths)
                            LINE += "first length ; second length ; third length ;";
                        if (nodes)
                            LINE += " Nodes ;";
                        if (area)
                            LINE += " Area ;";
                        if (massCenter)
                            LINE += " centeroid_X ; centeroid_Y ; centeroid_Z ;";
                        if (normal)
                            LINE += " normal_X ; normal_Y ; normal_Z ;";
                        if (angles)
                            LINE += " angle 1 ; angle 2 ; angle 3 ;";
                        sw.WriteLine(LINE);
                        #endregion

                        foreach (Triangle TR in container.Triangles)
                        {
                            string line = string.Format(" {0} ;", TR.Numer + 1);
                            if (pereferial)
                            {
                                bool bPereferial = container.Bends[TR.GetFirstBendNumer()].IsPeripheral() ||
                                     container.Bends[TR.GetSecondBendNumer()].IsPeripheral() || container.Bends[TR.GetThirdBendNumer()].IsPeripheral();
                                if (bPereferial)
                                    line += string.Format(" {0} ;", "Pereferial");
                                else
                                    line += string.Format(" {0} ;", "NoPereferial");
                            }
                            if (fictive)
                            {
                                bool bFictive = container.Bends[TR.GetFirstBendNumer()].IsFictive() &&
                                    container.Bends[TR.GetSecondBendNumer()].IsFictive() && container.Bends[TR.GetThirdBendNumer()].IsFictive();
                                if (bFictive)
                                    line += string.Format(" {0} ;", "Fictive");
                                else
                                    line += string.Format(" {0} ;", "NoFictive");
                            }

                            List<int> bendsN = new List<int> { TR.GetFirstBendNumer() + 1, TR.GetSecondBendNumer() + 1, TR.GetThirdBendNumer() + 1 };
                            bendsN.Sort();
                            if (bends)
                            {
                                line += string.Format(" {0}, {1}, {2} ;", bendsN[0], bendsN[1], bendsN[2]);
                            }
                            if (lengths)
                            {
                                line += string.Format(" {0}; {1}; {2} ;", container.Bends[bendsN[0] - 1].Length,
                                    container.Bends[bendsN[1] - 1].Length, container.Bends[bendsN[2] - 1].Length);
                            }

                            List<int> nodesN = new List<int> { TR.NodesNumers[0] + 1, TR.NodesNumers[1] + 1, TR.NodesNumers[2] + 1 };
                            nodesN.Sort();
                            if (nodes)
                            {

                                line += string.Format(" {0}, {1}, {2} ;", nodesN[0], nodesN[1], nodesN[2]);
                            }

                            if (area)
                                line += string.Format(" {0:f7} ;", TR.GetArea());

                            if (massCenter)
                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", TR.Normal.First.GetX(), TR.Normal.First.GetY(), TR.Normal.First.GetZ());


                            if (normal)
                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", TR.Normal.Second.GetX(), TR.Normal.Second.GetY(), TR.Normal.Second.GetZ());

                            if (angles)
                            {
                                var n1 = container.Nodes[nodesN[0] - 1];
                                var n2 = container.Nodes[nodesN[1] - 1];
                                var n3 = container.Nodes[nodesN[2] - 1];

                                double ang1 = (n2.Position - n1.Position).angTo(n3.Position - n1.Position);
                                double ang2 = (n1.Position - n2.Position).angTo(n3.Position - n2.Position);
                                double ang3 = (n1.Position - n3.Position).angTo(n2.Position - n3.Position);

                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", ang1 * 180.0 / Math.PI, ang2 * 180.0 / Math.PI, ang3 * 180.0 / Math.PI);

                            }

                            sw.WriteLine(line);
                        }

                        sw.Flush();
                        sw.Close();
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Triangles !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void KojtoCAD_3D_Pereferial_Triangles(bool pereferial, bool fictive, bool bends, bool nodes, bool area, bool massCenter, bool normal, bool angles, bool lengths)
        {
            if ((container != null) && (container.Triangles.Count > 0))
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                dlg.Title = "Enter CSV File Name ";
                dlg.DefaultExt = "csv";
                dlg.FileName = "*.csv";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dlg.FileName;
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        #region First Line
                        string LINE = " trNumer ;";
                        if (pereferial)
                            LINE += "Position Type ;";
                        if (fictive)
                            LINE += " Type ;";
                        if (bends)
                            LINE += " Bends ;";
                        if (lengths)
                            LINE += "first length ; second length ; third length ;";
                        if (nodes)
                            LINE += " Nodes ;";
                        if (area)
                            LINE += " Area ;";
                        if (massCenter)
                            LINE += " centeroid_X ; centeroid_Y ; centeroid_Z ;";
                        if (normal)
                            LINE += " normal_X ; normal_Y ; normal_Z ;";
                        if (angles)
                            LINE += " angle 1 ; angle 2 ; angle 3 ;";
                        sw.WriteLine(LINE);
                        #endregion

                        foreach (Triangle TR in container.Triangles)
                        {
                            if (!(container.Bends[TR.GetFirstBendNumer()].IsPeripheral() ||
                                container.Bends[TR.GetSecondBendNumer()].IsPeripheral() ||
                                container.Bends[TR.GetThirdBendNumer()].IsPeripheral()))
                                continue;

                            string line = string.Format(" {0} ;", TR.Numer + 1);
                            if (pereferial)
                            {
                                bool bPereferial = container.Bends[TR.GetFirstBendNumer()].IsPeripheral() ||
                                     container.Bends[TR.GetSecondBendNumer()].IsPeripheral() || container.Bends[TR.GetThirdBendNumer()].IsPeripheral();
                                if (bPereferial)
                                    line += string.Format(" {0} ;", "Pereferial");
                                else
                                    line += string.Format(" {0} ;", "NoPereferial");
                            }
                            if (fictive)
                            {
                                bool bFictive = container.Bends[TR.GetFirstBendNumer()].IsFictive() &&
                                    container.Bends[TR.GetSecondBendNumer()].IsFictive() && container.Bends[TR.GetThirdBendNumer()].IsFictive();
                                if (bFictive)
                                    line += string.Format(" {0} ;", "Fictive");
                                else
                                    line += string.Format(" {0} ;", "NoFictive");
                            }

                            List<int> bendsN = new List<int> { TR.GetFirstBendNumer() + 1, TR.GetSecondBendNumer() + 1, TR.GetThirdBendNumer() + 1 };
                            bendsN.Sort();
                            if (bends)
                            {
                                line += string.Format(" {0}, {1}, {2} ;", bendsN[0], bendsN[1], bendsN[2]);
                            }
                            if (lengths)
                            {
                                line += string.Format(" {0}; {1}; {2} ;", container.Bends[bendsN[0] - 1].Length,
                                    container.Bends[bendsN[1] - 1].Length, container.Bends[bendsN[2] - 1].Length);
                            }

                            List<int> nodesN = new List<int> { TR.NodesNumers[0] + 1, TR.NodesNumers[1] + 1, TR.NodesNumers[2] + 1 };
                            nodesN.Sort();
                            if (nodes)
                            {

                                line += string.Format(" {0}, {1}, {2} ;", nodesN[0], nodesN[1], nodesN[2]);
                            }

                            if (area)
                                line += string.Format(" {0:f7} ;", TR.GetArea());

                            if (massCenter)
                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", TR.Normal.First.GetX(), TR.Normal.First.GetY(), TR.Normal.First.GetZ());


                            if (normal)
                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", TR.Normal.Second.GetX(), TR.Normal.Second.GetY(), TR.Normal.Second.GetZ());

                            if (angles)
                            {
                                var n1 = container.Nodes[nodesN[0] - 1];
                                var n2 = container.Nodes[nodesN[1] - 1];
                                var n3 = container.Nodes[nodesN[2] - 1];

                                double ang1 = (n2.Position - n1.Position).angTo(n3.Position - n1.Position);
                                double ang2 = (n1.Position - n2.Position).angTo(n3.Position - n2.Position);
                                double ang3 = (n1.Position - n3.Position).angTo(n2.Position - n3.Position);

                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", ang1 * 180.0 / Math.PI, ang2 * 180.0 / Math.PI, ang3 * 180.0 / Math.PI);

                            }

                            sw.WriteLine(line);
                        }

                        sw.Flush();
                        sw.Close();
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Triangles !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void KojtoCAD_3D_NoPereferial_Triangles(bool pereferial, bool fictive, bool bends, bool nodes, bool area, bool massCenter, bool normal, bool angles, bool lengths)
        {
            if ((container != null) && (container.Triangles.Count > 0))
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                dlg.Title = "Enter CSV File Name ";
                dlg.DefaultExt = "csv";
                dlg.FileName = "*.csv";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dlg.FileName;
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        #region First Line
                        string LINE = " trNumer ;";
                        if (pereferial)
                            LINE += "Position Type ;";
                        if (fictive)
                            LINE += " Type ;";
                        if (bends)
                            LINE += " Bends ;";
                        if (lengths)
                            LINE += "first length ; second length ; third length ;";
                        if (nodes)
                            LINE += " Nodes ;";
                        if (area)
                            LINE += " Area ;";
                        if (massCenter)
                            LINE += " centeroid_X ; centeroid_Y ; centeroid_Z ;";
                        if (normal)
                            LINE += " normal_X ; normal_Y ; normal_Z ;";
                        if (angles)
                            LINE += " angle 1 ; angle 2 ; angle 3 ;";
                        sw.WriteLine(LINE);
                        #endregion

                        foreach (Triangle TR in container.Triangles)
                        {
                            if (container.Bends[TR.GetFirstBendNumer()].IsPeripheral() ||
                                container.Bends[TR.GetSecondBendNumer()].IsPeripheral() ||
                                container.Bends[TR.GetThirdBendNumer()].IsPeripheral())
                                continue;

                            string line = string.Format(" {0} ;", TR.Numer + 1);
                            if (pereferial)
                            {
                                bool bPereferial = container.Bends[TR.GetFirstBendNumer()].IsPeripheral() ||
                                     container.Bends[TR.GetSecondBendNumer()].IsPeripheral() || container.Bends[TR.GetThirdBendNumer()].IsPeripheral();
                                if (bPereferial)
                                    line += string.Format(" {0} ;", "Pereferial");
                                else
                                    line += string.Format(" {0} ;", "NoPereferial");
                            }
                            if (fictive)
                            {
                                bool bFictive = container.Bends[TR.GetFirstBendNumer()].IsFictive() &&
                                    container.Bends[TR.GetSecondBendNumer()].IsFictive() && container.Bends[TR.GetThirdBendNumer()].IsFictive();
                                if (bFictive)
                                    line += string.Format(" {0} ;", "Fictive");
                                else
                                    line += string.Format(" {0} ;", "NoFictive");
                            }

                            List<int> bendsN = new List<int> { TR.GetFirstBendNumer() + 1, TR.GetSecondBendNumer() + 1, TR.GetThirdBendNumer() + 1 };
                            bendsN.Sort();
                            if (bends)
                            {
                                line += string.Format(" {0}, {1}, {2} ;", bendsN[0], bendsN[1], bendsN[2]);
                            }

                            if (lengths)
                            {
                                line += string.Format(" {0}; {1}; {2} ;", container.Bends[bendsN[0] - 1].Length,
                                    container.Bends[bendsN[1] - 1].Length, container.Bends[bendsN[2] - 1].Length);
                            }

                            List<int> nodesN = new List<int> { TR.NodesNumers[0] + 1, TR.NodesNumers[1] + 1, TR.NodesNumers[2] + 1 };
                            nodesN.Sort();
                            if (nodes)
                            {

                                line += string.Format(" {0}, {1}, {2} ;", nodesN[0], nodesN[1], nodesN[2]);
                            }

                            if (area)
                                line += string.Format(" {0:f7} ;", TR.GetArea());

                            if (massCenter)
                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", TR.Normal.First.GetX(), TR.Normal.First.GetY(), TR.Normal.First.GetZ());


                            if (normal)
                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", TR.Normal.Second.GetX(), TR.Normal.Second.GetY(), TR.Normal.Second.GetZ());

                            if (angles)
                            {
                                var n1 = container.Nodes[nodesN[0] - 1];
                                var n2 = container.Nodes[nodesN[1] - 1];
                                var n3 = container.Nodes[nodesN[2] - 1];

                                double ang1 = (n2.Position - n1.Position).angTo(n3.Position - n1.Position);
                                double ang2 = (n1.Position - n2.Position).angTo(n3.Position - n2.Position);
                                double ang3 = (n1.Position - n3.Position).angTo(n2.Position - n3.Position);

                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", ang1 * 180.0 / Math.PI, ang2 * 180.0 / Math.PI, ang3 * 180.0 / Math.PI);

                            }

                            sw.WriteLine(line);
                        }

                        sw.Flush();
                        sw.Close();
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Triangles !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void KojtoCAD_3D_NoFictive_Triangles(bool pereferial, bool fictive, bool bends, bool nodes, bool area, bool massCenter, bool normal, bool angles, bool lengths)
        {
            if ((container != null) && (container.Triangles.Count > 0))
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                dlg.Title = "Enter CSV File Name ";
                dlg.DefaultExt = "csv";
                dlg.FileName = "*.csv";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dlg.FileName;
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        #region First Line
                        string LINE = " trNumer ;";
                        if (pereferial)
                            LINE += "Position Type ;";
                        if (fictive)
                            LINE += " Type ;";
                        if (bends)
                            LINE += " Bends ;";
                        if (lengths)
                            LINE += "first length ; second length ; third length ;";
                        if (nodes)
                            LINE += " Nodes ;";
                        if (area)
                            LINE += " Area ;";
                        if (massCenter)
                            LINE += " centeroid_X ; centeroid_Y ; centeroid_Z ;";
                        if (normal)
                            LINE += " normal_X ; normal_Y ; normal_Z ;";
                        if (angles)
                            LINE += " angle 1 ; angle 2 ; angle 3 ;";
                        sw.WriteLine(LINE);
                        #endregion

                        foreach (Triangle TR in container.Triangles)
                        {
                            if (container.Bends[TR.GetFirstBendNumer()].IsFictive() &&
                                container.Bends[TR.GetSecondBendNumer()].IsFictive() &&
                                container.Bends[TR.GetThirdBendNumer()].IsFictive())
                                continue;

                            string line = string.Format(" {0} ;", TR.Numer + 1);
                            if (pereferial)
                            {
                                bool bPereferial = container.Bends[TR.GetFirstBendNumer()].IsPeripheral() ||
                                     container.Bends[TR.GetSecondBendNumer()].IsPeripheral() || container.Bends[TR.GetThirdBendNumer()].IsPeripheral();
                                if (bPereferial)
                                    line += string.Format(" {0} ;", "Pereferial");
                                else
                                    line += string.Format(" {0} ;", "NoPereferial");
                            }
                            if (fictive)
                            {
                                bool bFictive = container.Bends[TR.GetFirstBendNumer()].IsFictive() &&
                                    container.Bends[TR.GetSecondBendNumer()].IsFictive() && container.Bends[TR.GetThirdBendNumer()].IsFictive();
                                if (bFictive)
                                    line += string.Format(" {0} ;", "Fictive");
                                else
                                    line += string.Format(" {0} ;", "NoFictive");
                            }

                            List<int> bendsN = new List<int> { TR.GetFirstBendNumer() + 1, TR.GetSecondBendNumer() + 1, TR.GetThirdBendNumer() + 1 };
                            bendsN.Sort();
                            if (bends)
                            {
                                line += string.Format(" {0}, {1}, {2} ;", bendsN[0], bendsN[1], bendsN[2]);
                            }
                            if (lengths)
                            {
                                line += string.Format(" {0}; {1}; {2} ;", container.Bends[bendsN[0] - 1].Length,
                                    container.Bends[bendsN[1] - 1].Length, container.Bends[bendsN[2] - 1].Length);
                            }

                            List<int> nodesN = new List<int> { TR.NodesNumers[0] + 1, TR.NodesNumers[1] + 1, TR.NodesNumers[2] + 1 };
                            nodesN.Sort();
                            if (nodes)
                            {

                                line += string.Format(" {0}, {1}, {2} ;", nodesN[0], nodesN[1], nodesN[2]);
                            }

                            if (area)
                                line += string.Format(" {0:f7} ;", TR.GetArea());

                            if (massCenter)
                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", TR.Normal.First.GetX(), TR.Normal.First.GetY(), TR.Normal.First.GetZ());


                            if (normal)
                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", TR.Normal.Second.GetX(), TR.Normal.Second.GetY(), TR.Normal.Second.GetZ());

                            if (angles)
                            {
                                var n1 = container.Nodes[nodesN[0] - 1];
                                var n2 = container.Nodes[nodesN[1] - 1];
                                var n3 = container.Nodes[nodesN[2] - 1];

                                double ang1 = (n2.Position - n1.Position).angTo(n3.Position - n1.Position);
                                double ang2 = (n1.Position - n2.Position).angTo(n3.Position - n2.Position);
                                double ang3 = (n1.Position - n3.Position).angTo(n2.Position - n3.Position);

                                line += string.Format(" {0:f5} ; {1:f5} ; {2:f5} ;", ang1 * 180.0 / Math.PI, ang2 * 180.0 / Math.PI, ang3 * 180.0 / Math.PI);

                            }

                            sw.WriteLine(line);
                        }

                        sw.Flush();
                        sw.Close();
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n\nMissing Triangles !", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        #region Poligons
        [CommandMethod("KojtoCAD_3D", "KCAD_POLYGONS_CSV", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/CREATE_POLIGONS_CSV.htm", "")]
        public void KojtoCAD_3D_Poligons_Csv_File()
        {
            
            if ((container != null) && (container.Bends.Count > 0) && (container.Nodes.Count > 0) && (container.Triangles.Count > 0))
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
                dlg.Title = "Enter CSV File Name ";
                dlg.DefaultExt = "csv";
                dlg.FileName = "*.csv";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dlg.FileName;
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        #region First Line
                        string LINE = " Numer ;";
                        LINE += " Type ;";// planar or notplanar
                        LINE += " Perimeter ;";//perimeter
                        LINE += " Area ;";//perimeter
                        LINE += " Bends List ;";
                        LINE += " Triangles List ;";
                        sw.WriteLine(LINE);
                        #endregion

                        foreach (Polygon POL in container.Polygons)
                        {
                            List<Bend> sortedBnends = new List<Bend>();
                            double perimeter = 0.0;

                            #region sort bends
                            foreach (int Num in POL.Bends_Numers_Array)
                            {
                                sortedBnends.Add(container.Bends[Num]);
                                perimeter += container.Bends[Num].Length;
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
                            #endregion

                            double area = 0.0;
                            foreach (int tN in POL.Triangles_Numers_Array)
                            {
                                area += container.Triangles[tN].GetArea();
                            }

                            string bends = "";
                            for (int i = 0; i < sortedBnends.Count; i++)
                            {
                                Bend bend = sortedBnends[i];
                                if (i < sortedBnends.Count - 1)
                                    bends += string.Format("{0}, ", bend.Numer + 1);
                                else
                                    bends += string.Format("{0} ", bend.Numer + 1);
                            }

                            List<int> arr = new List<int>();
                            foreach (Bend bend in sortedBnends)
                            {
                                foreach (int tN in POL.Triangles_Numers_Array)
                                {
                                    Triangle tr = container.Triangles[tN];
                                    if (tr.GetFirstBendNumer() == bend.Numer || tr.GetSecondBendNumer() == bend.Numer || tr.GetThirdBendNumer() == bend.Numer)
                                    {
                                        arr.Add(tr.Numer);
                                        break;
                                    }
                                }
                            }
                            if (POL.Triangles_Numers_Array.Count > arr.Count)
                            {
                                foreach (int tN in POL.Triangles_Numers_Array)
                                {
                                    if (arr.IndexOf(tN) < 0)
                                    {
                                        arr.Add(tN);
                                    }
                                }
                            }

                            string triangles = "";
                            for (int i = 0; i < arr.Count; i++)
                            {
                                if (i < arr.Count - 1)
                                    triangles += string.Format("{0}, ", arr[i] + 1);
                                else
                                    triangles += string.Format("{0} ", arr[i] + 1);
                            }

                            string line = string.Format(" {0} ; {1} ; {2:f5} ; {3:f5} ; {4} ; {5}", POL.GetNumer() + 1, POL.IsPlanar(ref container.Triangles) ? "Planar" : "NonPlanar",
                                perimeter, area, bends, triangles);

                            sw.WriteLine(line);
                        }

                        sw.Flush();
                        sw.Close();
                        MessageBox.Show("OK", "Save to File ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
                MessageBox.Show("\nData Base Empty !\n", "Range Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        [CommandMethod("KojtoCAD_3D", "KCAD_READ_NAMED_POINTS_FROM_CSV", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/READ_NAMED_POINTS_FROM_CSV.htm", "")]
        public void KojtoCAD_3D_Read_NamedPintesFromCSV()
        {
            
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "CSV or Text Files|*.csv;*.txt|Text Files|*.txt|Office Files|*.doc|All Files|*.*";
            dlg.Multiselect = false;
            dlg.Title = "Select CSV File ";
            dlg.DefaultExt = "csv";
            if (dlg.ShowDialog() == DialogResult.OK)
            {

                Pair<int, PromptStatus> NameColumn_Pair =
                              GlobalFunctions.GetInt(1, "Sequential Number of Column for Name: ", false, false);

                if (NameColumn_Pair.Second == PromptStatus.OK)
                {
                    int NameColumnNumber = NameColumn_Pair.First;

                    Pair<int, PromptStatus> X_Pair =
                               GlobalFunctions.GetInt(NameColumnNumber + 1, "Sequential Number of Column for X Coordinat: ", false, false);
                    if (X_Pair.Second == PromptStatus.OK)
                    {
                        int X_ColumnNumber = X_Pair.First;

                        Pair<int, PromptStatus> Y_Pair =
                                GlobalFunctions.GetInt(X_ColumnNumber + 1, "Sequential Number of Column for Y Coordinat: ", false, false);

                        if (Y_Pair.Second == PromptStatus.OK)
                        {
                            int Y_ColumnNumber = Y_Pair.First;

                            Pair<int, PromptStatus> Z_Pair =
                                GlobalFunctions.GetInt(Y_ColumnNumber + 1, "Sequential Number of Column for Z Coordinat: ", false, false);
                            if (Z_Pair.Second == PromptStatus.OK)
                            {
                                Pair<string, PromptStatus> YesOrNo_Draw =
                                    GlobalFunctions.GetKey(new[] { "Yes", "No" }, 0, "Draw Text ? ");
                                if (YesOrNo_Draw.Second == PromptStatus.OK)
                                {
                                    List<Pair<string, Point3d>> list = new List<Pair<string, Point3d>>();
                                    string ArrayName = "";
                                    Pair<string, PromptStatus> arrName =
                                        GlobalFunctions.GetString(NamedPointsARRAYs.Count.ToString(), "Array Name (Esc = no name and no will be remembered) ?");
                                    if (arrName.Second == PromptStatus.OK)
                                    {
                                        bool exist = false;
                                        arrName.First = arrName.First.TrimStart();
                                        arrName.First = arrName.First.TrimEnd();
                                        foreach (Pair<string, List<Pair<string, Point3d>>> item in NamedPointsARRAYs)
                                        {
                                            if (arrName.First.ToUpper() == item.First.ToUpper())
                                            {
                                                exist = true;
                                                break;
                                            }
                                        }
                                        if (!exist)
                                            ArrayName = arrName.First;
                                        else
                                        {
                                            MessageBox.Show("\nName exist in list !", "E R R O R ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nERROR: Name exist in list !");
                                            return;
                                        }
                                    }

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
                                                string Name = split[NameColumnNumber - 1];

                                                split[X_ColumnNumber - 1] = split[X_ColumnNumber - 1].Replace(',', '.');
                                                split[Y_ColumnNumber - 1] = split[Y_ColumnNumber - 1].Replace(',', '.');
                                                split[Z_Pair.First - 1] = split[Z_Pair.First - 1].Replace(',', '.');

                                                double X = double.NaN;
                                                double Y = double.NaN;
                                                double Z = double.NaN;
                                                try
                                                {
                                                    X = double.Parse(split[X_ColumnNumber - 1]);
                                                    Y = double.Parse(split[Y_ColumnNumber - 1]);
                                                    Z = double.Parse(split[Z_Pair.First - 1]);
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

                                                list.Add(new Pair<string, Point3d>(Name, new Point3d(X, Y, Z)));
                                            }
                                            catch {
                                            }
                                        }

                                        sr.Close();
                                    }

                                    if (arrName.Second == PromptStatus.OK)
                                        NamedPointsARRAYs.Add(new Pair<string, List<Pair<string, Point3d>>>(ArrayName, list));

                                    if (YesOrNo_Draw.First == "Yes")
                                    {
                                        Pair<int, PromptStatus> DirectionVariant_Pair =
                                            GlobalFunctions.GetInt(1, "Draw Text Parallel to ? ( 1 (X and oXY), 2 (X_and_oXZ), 3 (Y and oXY), 4 (Y and oYZ), 5 (Z and oXZ), 6 (Z and oYZ))", false, false);
                                        if (DirectionVariant_Pair.Second == PromptStatus.OK)
                                        {
                                            Pair<double, PromptStatus> textHeight =
                                                   GlobalFunctions.GetDouble(5.0, "Enter Text Height", false, false);
                                            if (textHeight.Second == PromptStatus.OK)
                                            {
                                                foreach (Pair<string, Point3d> pa in list)
                                                {
                                                    UCS ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 1, 0, 0), pa.Second + new quaternion(0, 0, 1, 0));
                                                    switch (DirectionVariant_Pair.First)
                                                    {
                                                        case 2: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 1, 0, 0), pa.Second + new quaternion(0, 0, 0, 1)); break;
                                                        case 3: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 0, 1, 0), pa.Second + new quaternion(0, 1, 0, 0)); break;
                                                        case 4: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 0, 1, 0), pa.Second + new quaternion(0, 0, 0, 1)); break;
                                                        case 5: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 0, 0, 1), pa.Second + new quaternion(0, 1, 0, 0)); break;
                                                        case 6: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 0, 0, 1), pa.Second + new quaternion(0, 0, 1, 0)); break;
                                                    }

                                                    Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                                    ObjectId ID = GlobalFunctions.DrawText(new Point3d(), textHeight.First, pa.First, ref mat);

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [CommandMethod("KojtoCAD_3D", "KCAD_NAMED_POINTS_DRAW", null, CommandFlags.Modal, null, "http://3dsoft.blob.core.windows.net/kojtocad/html/NAMED_POINTS_DRAW.htm", "")]
        public void KojtoCAD_3D_NamedPints_Draw()
        {
            
            bool rezult = false;
            Form_Arrays_of_named_points form = new Form_Arrays_of_named_points(ref NamedPointsARRAYs);
            form.ShowDialog();
            if (form.DialogResult == DialogResult.OK)
            {
                string Name = form.Name;
                foreach (Pair<string, List<Pair<string, Point3d>>> item in NamedPointsARRAYs)
                {
                    if (item.First == Name)
                    {
                        rezult = true;
                        Pair<int, PromptStatus> DirectionVariant_Pair =
                                            GlobalFunctions.GetInt(1, "Draw Text Parallel to ? ( 1 (X and oXY), 2 (X_and_oXZ), 3 (Y and oXY), 4 (Y and oYZ), 5 (Z and oXZ), 6 (Z and oYZ))", false, false);
                        if (DirectionVariant_Pair.Second == PromptStatus.OK)
                        {
                            Pair<double, PromptStatus> textHeight =
                                   GlobalFunctions.GetDouble(5.0, "Enter Text Height", false, false);
                            if (textHeight.Second == PromptStatus.OK)
                            {
                                foreach (Pair<string, Point3d> pa in item.Second)
                                {
                                    UCS ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 1, 0, 0), pa.Second + new quaternion(0, 0, 1, 0));
                                    switch (DirectionVariant_Pair.First)
                                    {
                                        case 2: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 1, 0, 0), pa.Second + new quaternion(0, 0, 0, 1)); break;
                                        case 3: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 0, 1, 0), pa.Second + new quaternion(0, 1, 0, 0)); break;
                                        case 4: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 0, 1, 0), pa.Second + new quaternion(0, 0, 0, 1)); break;
                                        case 5: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 0, 0, 1), pa.Second + new quaternion(0, 1, 0, 0)); break;
                                        case 6: ucs = new UCS(pa.Second, pa.Second + new quaternion(0, 0, 0, 1), pa.Second + new quaternion(0, 0, 1, 0)); break;
                                    }

                                    Matrix3d mat = new Matrix3d(ucs.GetAutoCAD_Matrix3d());

                                    ObjectId ID = GlobalFunctions.DrawText(new Point3d(), textHeight.First, pa.First, ref mat);

                                }
                            }
                        }
                    }
                }
                if (!rezult)
                {
                    MessageBox.Show("\nThe Name not exist in list !", "E R R O R ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nERROR: The Name not exist in list !");
                }
            }
        }

    }
}
