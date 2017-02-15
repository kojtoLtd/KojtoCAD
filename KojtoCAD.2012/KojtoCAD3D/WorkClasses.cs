using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
#endif
using NODES_ARRAY = System.Collections.Generic.List<KojtoCAD.KojtoCAD3D.WorkClasses.Node>;

using PRE_BEND = KojtoCAD.KojtoCAD3D.UtilityClasses.Pair<KojtoCAD.KojtoCAD3D.quaternion, KojtoCAD.KojtoCAD3D.quaternion>;
using BENDS_ARRAY = System.Collections.Generic.List<KojtoCAD.KojtoCAD3D.WorkClasses.Bend>;

using PRE_TRIANGLE = KojtoCAD.KojtoCAD3D.UtilityClasses.Triplet<KojtoCAD.KojtoCAD3D.quaternion, KojtoCAD.KojtoCAD3D.quaternion, KojtoCAD.KojtoCAD3D.quaternion>;
using TRIANGLES_ARRAY = System.Collections.Generic.List<KojtoCAD.KojtoCAD3D.WorkClasses.Triangle>;
using POLYGONS_ARRAY = System.Collections.Generic.List<KojtoCAD.KojtoCAD3D.WorkClasses.Polygon>;


namespace KojtoCAD.KojtoCAD3D
{
    namespace WorkClasses
    {
        public class Node
        {
            //----  Members ------------------------------------
            private string Key_;
            public string Key { get { return (Key_); } }

            private int Numer_;
            public int Numer { get { return (Numer_); } set { Numer_ = value; } }

            private quaternion Position_;
            public quaternion Position { get { return (Position_); } set { Position_ = value; } }

            private quaternion Normal_;
            public quaternion Normal { get { return (GetNormal()); } set { Normal_ = value; } }

            private quaternion ExplicitNormal_;
            private double ExplicitNormalLength_;
            public quaternion ExplicitNormal { get { return (ExplicitNormal_); } set { ExplicitNormal_ = SetExplicitNormal(value); } }
            public double ExplicitNormalLength { get { return (ExplicitNormalLength_); } set { ExplicitNormalLength_ = value; } }

            private UtilityClasses.Pair<int, Handle> SolidHandle_;//int is validhandle, int<0 - not valid
            public UtilityClasses.Pair<int, Handle> SolidHandle { get { return (SolidHandle_); } set { SolidHandle_ = value; } }

            private List<int> bends_Numers;//номерата на прътите които се събират в този възел от масива с пръти
            private List<int> triangles_Numers;//номерата на триъгълниците които се събират в този възел

            public List<int> Bends_Numers_Array { get { return (bends_Numers); } }
            public List<int> Triangles_Numers_Array { get { return (triangles_Numers); } }

            private int ExplicitCuttingMethodForEndsOf3D_Bends_;
            public int ExplicitCuttingMethodForEndsOf3D_Bends { get { return (ExplicitCuttingMethodForEndsOf3D_Bends_); } set { ExplicitCuttingMethodForEndsOf3D_Bends_ = value; } }

            //---- Constructors ------------------------------------------------------
            public Node()
            {
                Numer_ = -1;
                Position_ = new quaternion();
                Normal_ = null;
                ExplicitNormal_ = null;
                ExplicitNormalLength_ = 1.0;
                bends_Numers = new List<int>();
                triangles_Numers = new List<int>();
                Key_ = Guid.NewGuid().ToString();
                ExplicitCuttingMethodForEndsOf3D_Bends = -1;
                SolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
            }
            public Node(ref Node n, bool bNewKey = false)
            {
                Key_ = n.Key_;
                Numer_ = n.Numer_;
                Position_ = n.Position_;
                Normal_ = n.Normal_;
                ExplicitNormal_ = n.ExplicitNormal_;
                ExplicitNormalLength_ = n.ExplicitNormalLength_;
                bends_Numers = new List<int>();
                triangles_Numers = new List<int>();
                if (bNewKey)
                {
                    Key_ = Guid.NewGuid().ToString();
                    ExplicitCuttingMethodForEndsOf3D_Bends = -1;
                    SolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                }
                else
                {
                    Key_ = n.Key_;
                    ExplicitCuttingMethodForEndsOf3D_Bends = -1;
                    SolidHandle_ = new UtilityClasses.Pair<int, Handle>(n.SolidHandle_.First, n.SolidHandle_.Second);
                }

                foreach (int i in n.bends_Numers)
                {
                    bends_Numers.Add(i);
                }

                foreach (int i in n.triangles_Numers)
                {
                    triangles_Numers.Add(i);
                }

            }
            public Node(quaternion pos, int numer = -1)
            {
                Position_ = pos;
                Numer_ = numer;
                Normal_ = null;
                ExplicitNormal_ = null;
                ExplicitNormalLength_ = 1.0;
                bends_Numers = new List<int>();
                triangles_Numers = new List<int>();
                Key_ = Guid.NewGuid().ToString();
                SolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                ExplicitCuttingMethodForEndsOf3D_Bends = -1;
            }
            public Node(Xrecord xrec)
            {
                TypedValue[] res = xrec.Data.AsArray();
                bends_Numers = new List<int>();
                triangles_Numers = new List<int>();

                Numer_ = (int)res[0].Value;
                Position_ = new quaternion(0.0, (double)res[1].Value, (double)res[2].Value, (double)res[3].Value);
                Normal_ = new quaternion(0.0, (double)res[4].Value, (double)res[5].Value, (double)res[6].Value);
                Key_ = (string)res[7].Value;
                long ln = Convert.ToInt64((string)res[9].Value, 16);
                SolidHandle_ = new UtilityClasses.Pair<int, Handle>((int)res[8].Value, new Handle(ln));

                int bends_count = (int)res[10].Value;

                int bc = (int)res[10].Value;
                int ii = 11;
                if (bc > 0)
                {
                    for (int i = 0; i < bc; i++, ii++)
                    {
                        int v = (int)res[ii].Value;
                        bends_Numers.Add(v);
                    }
                }

                int bcc = (int)res[ii].Value;
                if (bcc > 0)
                {
                    for (int i = 0; i < bcc; i++, ii++)
                    {
                        int v = (int)res[ii + 1].Value;
                        triangles_Numers.Add(v);
                    }
                }

                int lastIndex = res.Count() - 1;

                //-- Explicit cutting method for ends of 3D bends
                ExplicitCuttingMethodForEndsOf3D_Bends_ = (int)res[lastIndex - 5].Value;

                //--- Explicit normal
                ExplicitNormalLength_ = (double)res[lastIndex - 4].Value;
                if ((int)res[lastIndex - 3].Value == 1)
                {
                    ExplicitNormal_ = new quaternion(0.0, (double)res[lastIndex - 2].Value, (double)res[lastIndex - 1].Value, (double)res[lastIndex].Value);
                }
                else
                {
                    ExplicitNormal_ = null;
                }

            }
            public Node(StreamReader sr)
            {
                List<string> list = new List<string>();
                bends_Numers = new List<int>();
                triangles_Numers = new List<int>();

                for (int i = 0; i < 11; i++)
                    list.Add(sr.ReadLine());

                Numer_ = int.Parse(list[0]);
                Position_ = new quaternion(0.0, double.Parse(list[1]), double.Parse(list[2]), double.Parse(list[3]));
                Normal_ = new quaternion(0.0, double.Parse(list[4]), double.Parse(list[5]), double.Parse(list[6]));
                Key_ = list[7];

                long ln = Convert.ToInt64(list[9], 16);
                SolidHandle_ = new UtilityClasses.Pair<int, Handle>(int.Parse(list[8]), new Handle(ln));

                int bends_count = int.Parse(list[10]);

                for (int i = 0; i < bends_count; i++)
                    list.Add(sr.ReadLine());

                int bc = bends_count;
                int ii = 11;

                if (bc > 0)
                {
                    for (int i = 0; i < bc; i++, ii++)
                    {
                        int v = int.Parse(list[ii]);
                        bends_Numers.Add(v);
                    }
                }



                list.Add(sr.ReadLine());
                int bcc = int.Parse(list[ii]);
                if (bcc > 0)
                {
                    for (int i = 0; i < bcc; i++)
                        list.Add(sr.ReadLine());

                    for (int i = 0; i < bcc; i++, ii++)
                    {
                        int v = int.Parse(list[ii + 1]);
                        triangles_Numers.Add(v);
                    }
                }

                list.Add(sr.ReadLine()); list.Add(sr.ReadLine()); list.Add(sr.ReadLine());
                list.Add(sr.ReadLine()); list.Add(sr.ReadLine()); list.Add(sr.ReadLine());

                int lastIndex = list.Count() - 1;

                ExplicitCuttingMethodForEndsOf3D_Bends_ = int.Parse(list[lastIndex - 5]);

                ExplicitNormalLength_ = double.Parse(list[lastIndex - 4]);
                int k = int.Parse(list[lastIndex - 3]);
                if (k == 1)
                {
                    ExplicitNormal_ = new quaternion(0.0, double.Parse(list[lastIndex - 2]),
                        double.Parse(list[lastIndex - 1]), double.Parse(list[lastIndex]));
                }
                else
                {
                    ExplicitNormal_ = null;
                }
            }
            //--- Functions -----------------------------------------------------
            public string GetKey() { return Key_; }
            public int GetNumer() { return Numer_; }
            public void SetNumer(int n) { Numer_ = n; }
            public quaternion GetPosition() { return Position_; }
            public void SetPosition(quaternion q) { Position_ = q; }
            public quaternion GetNormal()
            {
                if ((object)ExplicitNormal_ != null)
                    return ExplicitNormal_;
                else
                    return Normal_;
            }
            public void SetNormal(quaternion q) { Normal_ = q; }
            public quaternion SetExplicitNormal(quaternion q)
            {
                if ((object)q != null)
                {
                    quaternion exQ = ((q - Position_).angTo(Normal_ - Position_) <= Math.PI / 2.0) ? (q - Position_) : (Position_ - q);
                    exQ /= exQ.abs();
                    ExplicitNormal_ = Position_ + exQ;
                }
                else
                    ExplicitNormal_ = null;

                return ExplicitNormal_;
            }
            public quaternion SetExplicitNormal(quaternion q1, quaternion q2)
            {
                if ((object)q1 != null && (object)q2 != null)
                {
                    quaternion exQ = ((q2 - q1).angTo(Normal_ - Position_) <= Math.PI / 2.0) ? (q2 - q1) : (q1 - q2);
                    exQ /= exQ.abs();
                    ExplicitNormal_ = Position_ + exQ;
                }
                else
                    ExplicitNormal_ = null;

                return ExplicitNormal_;
            }

            public int GetBendsCount() { return bends_Numers.Count; }
            public int GetBendsCount(ref UtilityClasses.Containers container)
            {
                int N = 0;
                foreach (int nN in bends_Numers) if (!container.Bends[nN].IsFictive()) N++;

                return N;
            }
            public void AddBend(Bend b) { bends_Numers.Add(b.GetNumer()); }
            public void RemoveBendsAt(int index) { if (index < bends_Numers.Count) { bends_Numers.RemoveAt(index); } }
            public void RemoveBendsNumber(int number) { if (bends_Numers.Contains(number)) { bends_Numers.Remove(number); } }
            public int GetBendsAt(int index) { return bends_Numers.ElementAt(index); }
            public bool ContainsBendNumber(int number) { return bends_Numers.IndexOf(number) >= 0; }
            public int IndexOfBendNumber(int number) { return bends_Numers.IndexOf(number); }

            public void ClearTriangles()
            {
                triangles_Numers.Clear();
                triangles_Numers = new List<int>();
            }
            public int GetTrianglesCount() { return triangles_Numers.Count; }
            public void AddTriangle(Triangle t) { triangles_Numers.Add(t.GetNumer()); }
            public void AddTriangle(int t) { triangles_Numers.Add(t); }
            public void RemoveTrianglesAt(int index) { if (index < triangles_Numers.Count) { triangles_Numers.RemoveAt(index); } }
            public void RemoveTrianglesNumber(int number) { if (triangles_Numers.Contains(number)) { triangles_Numers.Remove(number); } }
            public int GetTrianglesAt(int index) { return triangles_Numers.ElementAt(index); }
            public bool ContainsTriangleNumber(int number) { return triangles_Numers.IndexOf(number) >= 0; }
            public int IndexOfTriangleNumber(int number) { return triangles_Numers.IndexOf(number); }
            public void ReverseNormal()
            {
                if ((object)Normal_ != null)
                {
                    try
                    {
                        Normal_ = Position_ + Position_ - Normal_;
                        if ((object)ExplicitNormal_ != null)
                            ExplicitNormal_ = (Position_ - ExplicitNormal_) + Position_;
                    }
                    catch { }
                }
            }

            public bool IsFictive(ref UtilityClasses.Containers container)
            {
                bool rez = true;
                foreach (int bN in bends_Numers)
                {
                    if (!container.Bends[bN].IsFictive())
                    {
                        rez = false;
                        break;
                    }
                }
                return rez;
            }

            private void ArangeTrianglesW(ref List<int> rez, ref TRIANGLES_ARRAY trARR)
            {
                Triangle tr = trARR[rez[rez.Count - 1]];
                foreach (int N in Triangles_Numers_Array)
                {
                    if (rez.IndexOf(N) < 0)
                    {
                        Triangle TR = trARR[N];
                        int bN = -1;
                        Triangle.IsContiguous(tr, TR, ref bN);
                        if (bN >= 0)
                        {
                            rez.Add(N);
                            ArangeTrianglesW(ref rez, ref trARR);
                            break;
                        }
                    }
                }
            }
            //sort triangles Counterclockwise
            public List<int> ArangeTriangles(ref TRIANGLES_ARRAY trARR, ref BENDS_ARRAY bends)
            {
                List<int> rez = new List<int>();
                if (Triangles_Numers_Array.Count > 1)
                {
                    //ako ima perifern triangle go wzimame za na4alen
                    #region ima li periferen triangle
                    foreach (int N in Triangles_Numers_Array)
                    {
                        Triangle TR = trARR[N];
                        if (bends[TR.GetFirstBendNumer()].IsPeripheral() ||
                            bends[TR.GetSecondBendNumer()].IsPeripheral() ||
                            bends[TR.GetThirdBendNumer()].IsPeripheral())
                        {
                            rez.Add(N);
                            break;
                        }

                    }
                    #endregion

                    //wsi4ki triygylnici sa wytre6ni
                    if (rez.Count == 0)
                        rez.Add(Triangles_Numers_Array[0]);

                    ArangeTrianglesW(ref rez, ref trARR);

                    Triangle tR = trARR[rez[0]];
                    Triangle Tr = trARR[rez[1]];
                    int bN = -1;
                    Triangle.IsContiguous(tR, Tr, ref bN);
                    UCS ucs = new UCS(Position_, tR.GetCentroid(), bends[bN].MidPoint);
                    if (ucs.FromACS(Normal_).GetZ() < 0)
                        rez.Reverse();
                }
                else
                {
                    rez.Add(Triangles_Numers_Array[0]);
                }
                return rez;
            }

            //--- Operators -------------------------------------------------------------------------------------------------
            public override bool Equals(Object obj)
            {
                return ((obj is Node) && (this == (Node)obj));
            }
            public override int GetHashCode()
            {
                return (Numer_.GetHashCode() ^ Position_.GetHashCode() ^ Normal_.GetHashCode() ^ bends_Numers.GetHashCode() ^ triangles_Numers.GetHashCode());
            }
            public override string ToString()
            {
                string Pos = String.Format("Position: {0} , {1} , {2}\n", Position_.GetX(), Position_.GetY(), Position_.GetZ());
                string No = "";
                if (Normal_ != null)
                    No = String.Format("Normal: {0} , {1} , {2}\n", Normal_.GetX(), Normal_.GetY(), Normal_.GetZ());
                else
                    No = "null\n";

                string bends = "Bends Numers Array: \n";
                foreach (int ib in bends_Numers)
                {
                    bends += "       " + ib.ToString() + "\n";
                }
                string trs = "Triangles Numers : \n";
                foreach (int it in triangles_Numers)
                {
                    trs += "       " + it.ToString() + "\n";
                }

                string mess = String.Format("Node\n-----\n\nKey = {0} \n {1} \n {2} \n {3} \n {4} \n {5}", Key_, Numer_, Pos, No, bends, trs);
                return mess;
                //return (String.Format("{0} , {1} , {2}", this.Position.GetX(), this.Position.GetY(), this.Position.GetZ()));
            }
            public Xrecord GetXrecord()
            {
                Xrecord xRec = new Xrecord();
                if ((object)Normal_ != null)
                {
                    ResultBuffer rB = new ResultBuffer(
                           new TypedValue((int)DxfCode.Int32, Numer_),         //0
                           new TypedValue((int)DxfCode.Real, Position_.GetX()),//1
                           new TypedValue((int)DxfCode.Real, Position_.GetY()),//2
                           new TypedValue((int)DxfCode.Real, Position_.GetZ()),//3
                           new TypedValue((int)DxfCode.Real, Normal_.GetX()),  //4
                           new TypedValue((int)DxfCode.Real, Normal_.GetY()),  //5
                           new TypedValue((int)DxfCode.Real, Normal_.GetZ()),  //6 
                           new TypedValue((int)DxfCode.Text, Key_),                 //7
                           new TypedValue((int)DxfCode.Int32, SolidHandle_.First),  //8
                           new TypedValue((int)DxfCode.ExtendedDataHandle, SolidHandle_.Second),//9
                           new TypedValue((int)DxfCode.Int32, bends_Numers.Count));//10
                    for (int i = 0; i < bends_Numers.Count; i++)
                    {
                        rB.Add(new TypedValue((int)DxfCode.Int32, bends_Numers[i]));
                    }

                    rB.Add(new TypedValue((int)DxfCode.Int32, triangles_Numers.Count));
                    for (int i = 0; i < triangles_Numers.Count; i++)
                    {
                        rB.Add(new TypedValue((int)DxfCode.Int32, triangles_Numers[i]));
                    }

                    //--- Explicit cutting method for ends of 3D bends
                    rB.Add(new TypedValue((int)DxfCode.Int32, ExplicitCuttingMethodForEndsOf3D_Bends_));

                    //--- Explicit Normal
                    rB.Add(new TypedValue((int)DxfCode.Real, ExplicitNormalLength_));
                    rB.Add(new TypedValue((int)DxfCode.Int32, (((object)ExplicitNormal_ == null) ? 0 : 1)));
                    if ((object)ExplicitNormal_ != null)
                    {
                        rB.Add(new TypedValue((int)DxfCode.Real, ExplicitNormal_.GetX()));
                        rB.Add(new TypedValue((int)DxfCode.Real, ExplicitNormal_.GetY()));
                        rB.Add(new TypedValue((int)DxfCode.Real, ExplicitNormal_.GetZ()));
                    }
                    else
                    {
                        rB.Add(new TypedValue((int)DxfCode.Real, Normal_.GetX()));
                        rB.Add(new TypedValue((int)DxfCode.Real, Normal_.GetY()));
                        rB.Add(new TypedValue((int)DxfCode.Real, Normal_.GetZ()));
                    }


                    xRec.Data = rB;
                }
                else
                {
                    xRec = null;
                }

                return xRec;
            }

            public void ToStream(StreamWriter sw)
            {
                sw.WriteLine(Numer_);//0
                sw.WriteLine(Position_.GetX());//1
                sw.WriteLine(Position_.GetY());//2
                sw.WriteLine(Position_.GetZ());//3
                sw.WriteLine(Normal_.GetX());//4
                sw.WriteLine(Normal_.GetY());//5
                sw.WriteLine(Normal_.GetZ());//6
                sw.WriteLine(Key_);//7
                sw.WriteLine(SolidHandle_.First);//8
                sw.WriteLine(SolidHandle_.Second);//9
                sw.WriteLine(bends_Numers.Count);//10
                for (int i = 0; i < bends_Numers.Count; i++)
                {
                    sw.WriteLine(bends_Numers[i]);
                }
                sw.WriteLine(triangles_Numers.Count);
                for (int i = 0; i < triangles_Numers.Count; i++)
                {
                    sw.WriteLine(triangles_Numers[i]);
                }

                sw.WriteLine(ExplicitCuttingMethodForEndsOf3D_Bends_);
                sw.WriteLine(ExplicitNormalLength_);
                sw.WriteLine((((object)ExplicitNormal_ == null) ? 0 : 1));
                if ((object)ExplicitNormal_ != null)
                {
                    sw.WriteLine(ExplicitNormal_.GetX());
                    sw.WriteLine(ExplicitNormal_.GetY());
                    sw.WriteLine(ExplicitNormal_.GetZ());
                }
                else
                {
                    sw.WriteLine(Normal_.GetX());
                    sw.WriteLine(Normal_.GetY());
                    sw.WriteLine(Normal_.GetZ());
                }

            }

            public static bool operator ==(Node n1, Node n2)
            {
                return (n1.Position_ - n2.Position_).abs() < UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes();
            }
            public static bool operator !=(Node n1, Node n2)
            {
                return (!(n1 == n2));
            }

            public static bool operator ==(Node n1, quaternion q)
            {
                return (n1.Position_ - q).abs() < UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes();
            }
            public static bool operator !=(Node n1, quaternion q)
            {
                return (!(n1 == q));
            }

            public static bool operator ==(quaternion q, Node n1)
            {
                return (n1.Position_ - q).abs() < UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes();
            }
            public static bool operator !=(quaternion q, Node n1)
            {
                return (!(n1 == q));
            }

            //--------------------
            public void FillBends_Numbers_Array(ref List<Bend> bendsARR)
            {
                for (int i = 0; i < bendsARR.Count; i++)
                {
                    if ((this == bendsARR[i].GetPreBend().First) || (this == bendsARR[i].GetPreBend().Second))
                    {
                        bends_Numers.Add(i);
                    }
                }
            }

            //----------------------CAM,   CNC -------------------------
            public quaternion GetNodesNormalsByNoFictiveBends(ref UtilityClasses.Containers container)
            {
                if ((object)ExplicitNormal_ != null)
                    return ExplicitNormal_;
                else
                {

                    quaternion q = new quaternion();
                    int counter = 0;
                    foreach (int bN in Bends_Numers_Array)
                    {
                        if (!container.Bends[bN].IsFictive())
                        {
                            quaternion q1 = container.Bends[bN].Normal - container.Bends[bN].MidPoint;
                            q1 /= q1.abs();
                            q += q1;
                            counter++;
                        }
                    }
                    q /= counter;
                    #region Zero exeption
                    if (q.abs() < Constants.zero_dist)
                    {
                        q *= counter;
                        foreach (int bN in Bends_Numers_Array)
                        {
                            if (container.Bends[bN].IsFictive())
                            {
                                quaternion q1 = container.Bends[bN].Normal - container.Bends[bN].MidPoint;
                                q += q1;
                                counter++;
                                q /= counter;
                                break;
                            }
                        }
                    }
                    #endregion;
                    q /= q.abs();
                    return Position + q;
                }
            }
            public UCS CreateNodeUCS(double L, ref UtilityClasses.Containers container)
            {
                Bend b = container.Bends[bends_Numers[0]];
                quaternion normal = GetNodesNormalsByNoFictiveBends(ref container);
                quaternion sub = Position - normal;
                double k = (sub.abs() + L) / sub.abs();
                quaternion o = normal + sub * k;

                PRE_BEND rebro =
                    ((b.Start - Position).abs() < (b.End - Position).abs()) ? new PRE_BEND(b.Start, b.End) :
                    new PRE_BEND(b.End, b.Start);

                UCS ucs = new UCS(o, normal, o + rebro.Second - rebro.First);


                return new UCS(o, ucs.ToACS(new quaternion(0, 0, 100, 0)), ucs.ToACS(new quaternion(0, 0, 0, 100)));
            }
            #region old
            /*
            public MathLibKCAD.UCS CreateNodeUCS(double L, ref UtilityClasses.Containers container)
            {
                Bend b = container.Bends[this.bends_Numers[0]];
                MathLibKCAD.quaternion sub = Position - Normal;
                double k = (sub.abs() + L) / sub.abs();
                MathLibKCAD.quaternion o = Normal_ + sub * k;

                UtilityClasses.Pair<MathLibKCAD.quaternion, MathLibKCAD.quaternion> rebro =
                    ((b.Start - Position).abs() < (b.End - Position).abs()) ? new UtilityClasses.Pair<MathLibKCAD.quaternion, MathLibKCAD.quaternion>(b.Start, b.End) :
                    new UtilityClasses.Pair<MathLibKCAD.quaternion, MathLibKCAD.quaternion>(b.End, b.Start);

                MathLibKCAD.UCS ucs = new MathLibKCAD.UCS(o, Normal, o + rebro.Second - rebro.First);


                return new MathLibKCAD.UCS(o, ucs.ToACS(new MathLibKCAD.quaternion(0, 0, 100, 0)), ucs.ToACS(new MathLibKCAD.quaternion(0, 0, 0, 100)));
            }
            */
            #endregion

            public UtilityClasses.Triplet<int, double, quaternion> CenterOfGravity(ref UtilityClasses.Containers container, double density = 1.0)
            {
                UtilityClasses.Triplet<int, double, quaternion> rez =
                    new UtilityClasses.Triplet<int, double, quaternion>(-1, -1.0, new quaternion());

                Solid3d SOLID = null;
                using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    if (SolidHandle_.First >= 0)
                    {
                        try
                        {
                            BlockReference ent = tr.GetObject(UtilityClasses.GlobalFunctions.GetObjectId(SolidHandle.Second), OpenMode.ForWrite) as BlockReference;
                            DBObjectCollection coll = new DBObjectCollection();
                            ent.Explode(coll);

                            foreach (DBObject obj in coll)
                            {
                                acBlkTblRec.AppendEntity((Entity)obj);
                                tr.AddNewlyCreatedDBObject(obj, true);

                                SOLID = tr.GetObject(obj.ObjectId, OpenMode.ForWrite) as Solid3d;
                            }
                        }
                        catch { }

                        foreach (int N in Bends_Numers_Array)
                        {
                            Bend bend = container.Bends[N];

                            if (bend.startSolidHandle.First >= 0)
                            {
                                try
                                {
                                    BlockReference br = tr.GetObject(UtilityClasses.GlobalFunctions.GetObjectId(bend.startSolidHandle.Second), OpenMode.ForWrite) as BlockReference;
                                    DBObjectCollection Coll = new DBObjectCollection();
                                    br.Explode(Coll);

                                    foreach (DBObject obj in Coll)
                                    {
                                        acBlkTblRec.AppendEntity((Entity)obj);
                                        tr.AddNewlyCreatedDBObject(obj, true);

                                        Solid3d sol = tr.GetObject(obj.ObjectId, OpenMode.ForWrite) as Solid3d;
                                        SOLID.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                                    }
                                }
                                catch { }
                            }

                            if (bend.endSolidHandle.First >= 0)
                            {
                                try
                                {
                                    BlockReference br = tr.GetObject(UtilityClasses.GlobalFunctions.GetObjectId(bend.endSolidHandle.Second), OpenMode.ForWrite) as BlockReference;
                                    DBObjectCollection Coll = new DBObjectCollection();
                                    br.Explode(Coll);

                                    foreach (DBObject obj in Coll)
                                    {
                                        acBlkTblRec.AppendEntity((Entity)obj);
                                        tr.AddNewlyCreatedDBObject(obj, true);

                                        Solid3d sol = tr.GetObject(obj.ObjectId, OpenMode.ForWrite) as Solid3d;
                                        SOLID.BooleanOperation(BooleanOperationType.BoolSubtract, sol);
                                    }
                                }
                                catch { }
                            }

                        }
                    }

                    if (SOLID.MassProperties.Volume > 0)
                        rez = new UtilityClasses.Triplet<int, double, quaternion>(1, SOLID.MassProperties.Volume * density, SOLID.MassProperties.Centroid);
                    //tr.Commit();
                }
                return rez;
            }
        }
        public class Bend
        {
            //----------  Members ----------------------------------------------------------------------
            private string Key_;
            public string Key { get { return (Key_); } }

            private int Numer_;
            public int Numer { get { return (Numer_); } set { Numer_ = value; } }

            private PRE_BEND Pre_Bend_;
            public PRE_BEND Pre_Bend { get { return (Pre_Bend_); } set { Pre_Bend_ = value; } }

            double Length_;
            public double Length { get { return (Length_); } }
            public double Size { get { return (Length_); } set { Length_ = value; } }

            private quaternion MidPoint_;
            public quaternion MidPoint { get { return (MidPoint_); } set { MidPoint_ = value; } }

            public quaternion Start { get { return (Pre_Bend_.First); } }
            public quaternion End { get { return (Pre_Bend_.Second); } }

            private quaternion Normal_;
            public quaternion Normal { get { return (Normal_); } set { Normal_ = value; } }

            private bool Fictive_;
            public bool Fictive { get { return (Fictive_); } }

            private int First_Triangle_Numer_;
            public int FirstTriangleNumer { get { return (First_Triangle_Numer_); } }

            private int Second_Triangle_Numer_;
            public int SecondTriangleNumer { get { return (Second_Triangle_Numer_); } }

            private int StartNode_Numer_;
            public int StartNodeNumer { get { return (StartNode_Numer_); } }

            private int EndNode_Numer_;
            public int EndNodeNumer { get { return (EndNode_Numer_); } }

            private double FirstTriangleOffset_;
            public double FirstTriangleOffset { get { return (FirstTriangleOffset_); } }

            private double SecondTriangleOffset_;
            public double SecondTriangleOffset { get { return (SecondTriangleOffset_); } set { if (!Fictive_) { SecondTriangleOffset_ = value; } } }

            private UtilityClasses.Pair<int, Handle> startSolidHandle_;//int is validhandle, int<0 - not valid
            public UtilityClasses.Pair<int, Handle> startSolidHandle { get { return (startSolidHandle_); } set { startSolidHandle_ = value; } }

            private UtilityClasses.Pair<int, Handle> endSolidHandle_;//int is validhandle, int<0 - not valid
            public UtilityClasses.Pair<int, Handle> endSolidHandle { get { return (endSolidHandle_); } set { endSolidHandle_ = value; } }

            private UtilityClasses.Pair<int, Handle> SolidHandle_;//int is validhandle, int<0 - not valid
            public UtilityClasses.Pair<int, Handle> SolidHandle { get { return (SolidHandle_); } set { SolidHandle_ = value; } }

            private int ExplicitNormal_;
            public int ExplicitNormal { get { return (ExplicitNormal_); } set { ExplicitNormal_ = value; } }

            private double explicit_cutSolidsThicknes_Start_ = -1.0;
            public double explicit_cutSolidsThicknes_Start { get { return (explicit_cutSolidsThicknes_Start_); } set { explicit_cutSolidsThicknes_Start_ = value; } }

            private double explicit_cutSolidsThicknes_End_ = -1.0;
            public double explicit_cutSolidsThicknes_End { get { return (explicit_cutSolidsThicknes_End_); } set { explicit_cutSolidsThicknes_End_ = value; } }

            private double explicit_cutSolidsLenK_Start_ = -1.0;
            public double explicit_cutSolidsLenK_Start { get { return (explicit_cutSolidsLenK_Start_); } set { explicit_cutSolidsLenK_Start_ = value; } }

            private double explicit_cutSolidsLenK_End_ = -1.0;
            public double explicit_cutSolidsLenK_End { get { return (explicit_cutSolidsLenK_End_); } set { explicit_cutSolidsLenK_End_ = value; } }

            private double explicit_first_cutExtrudeRatio_ = -1.0;
            public double explicit_first_cutExtrudeRatio { get { return (explicit_first_cutExtrudeRatio_); } set { explicit_first_cutExtrudeRatio_ = value; } }

            private double explicit_second_cutExtrudeRatio_ = -1.0;
            public double explicit_second_cutExtrudeRatio { get { return (explicit_second_cutExtrudeRatio_); } set { explicit_second_cutExtrudeRatio_ = value; } }

            //---------------------     Constructors -------------------------------------------------
            public Bend()
            {
                Numer_ = -1;
                Pre_Bend_ = null;
                Normal_ = null;
                Fictive_ = true;
                First_Triangle_Numer_ = -1;
                Second_Triangle_Numer_ = -1;
                StartNode_Numer_ = -1;
                EndNode_Numer_ = -1;
                Length_ = -1.0;
                MidPoint_ = null;
                Key_ = Guid.NewGuid().ToString();
                FirstTriangleOffset_ = 0.0;
                SecondTriangleOffset_ = 0.0;
                startSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                endSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                SolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                ExplicitNormal_ = 0;
                explicit_cutSolidsThicknes_Start_ = -1.0;
                explicit_cutSolidsThicknes_End_ = -1.0;
                explicit_cutSolidsLenK_Start_ = -1.0;
                explicit_cutSolidsLenK_End_ = -1.0;

                explicit_first_cutExtrudeRatio_ = -1.0;
                explicit_second_cutExtrudeRatio_ = -1.0;
            }
            public Bend(ref Bend b, bool bNewKey = false)
            {
                Numer_ = b.Numer_;
                Pre_Bend_ = b.Pre_Bend_;
                Normal_ = b.Normal_;
                Fictive_ = b.Fictive_;
                First_Triangle_Numer_ = b.First_Triangle_Numer_;
                Second_Triangle_Numer_ = b.Second_Triangle_Numer_;
                StartNode_Numer_ = b.StartNode_Numer_;
                EndNode_Numer_ = b.EndNode_Numer_;
                Length_ = b.Length_;
                MidPoint_ = b.MidPoint_;
                ExplicitNormal_ = b.ExplicitNormal_;
                if (bNewKey)
                    Key_ = Guid.NewGuid().ToString();
                else
                    Key_ = b.Key_;

                FirstTriangleOffset_ = b.FirstTriangleOffset_;
                SecondTriangleOffset_ = b.SecondTriangleOffset_;

                if (bNewKey)
                {
                    startSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                    endSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                    SolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));

                    explicit_cutSolidsThicknes_Start_ = -1.0;
                    explicit_cutSolidsThicknes_End_ = -1.0;
                    explicit_cutSolidsLenK_Start_ = -1.0;
                    explicit_cutSolidsLenK_End_ = -1.0;

                    explicit_first_cutExtrudeRatio_ = -1.0;
                    explicit_second_cutExtrudeRatio_ = -1.0;
                }
                else
                {
                    startSolidHandle_ = new UtilityClasses.Pair<int, Handle>(b.startSolidHandle_.First, b.startSolidHandle_.Second);
                    endSolidHandle_ = new UtilityClasses.Pair<int, Handle>(b.endSolidHandle_.First, b.endSolidHandle_.Second);
                    SolidHandle_ = new UtilityClasses.Pair<int, Handle>(b.SolidHandle_.First, b.SolidHandle_.Second);

                    explicit_cutSolidsThicknes_Start_ = b.explicit_cutSolidsThicknes_Start_;
                    explicit_cutSolidsThicknes_End_ = b.explicit_cutSolidsThicknes_End_;
                    explicit_cutSolidsLenK_Start_ = b.explicit_cutSolidsLenK_Start_;
                    explicit_cutSolidsLenK_End_ = b.explicit_cutSolidsLenK_End_;

                    explicit_first_cutExtrudeRatio_ = b.explicit_second_cutExtrudeRatio_;
                    explicit_second_cutExtrudeRatio_ = b.explicit_second_cutExtrudeRatio_;
                }
            }
            public Bend(PRE_BEND PB, bool F = false)
            {
                Numer_ = -1;
                Pre_Bend_ = new PRE_BEND(PB.First, PB.Second);
                Normal_ = null;
                ExplicitNormal_ = 0;
                Fictive_ = F;
                First_Triangle_Numer_ = -1;
                Second_Triangle_Numer_ = -1;
                StartNode_Numer_ = -1;
                EndNode_Numer_ = -1;
                Length_ = (PB.Second - PB.First).abs();
                MidPoint_ = (PB.Second + PB.First) / 2.0;
                Key_ = Guid.NewGuid().ToString();
                FirstTriangleOffset_ = 0.0;
                SecondTriangleOffset_ = 0.0;
                startSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                endSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                SolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                explicit_cutSolidsThicknes_Start_ = -1.0;
                explicit_cutSolidsThicknes_End_ = -1.0;
                explicit_cutSolidsLenK_Start_ = -1.0;
                explicit_cutSolidsLenK_End_ = -1.0;

                explicit_first_cutExtrudeRatio_ = -1.0;
                explicit_second_cutExtrudeRatio_ = -1.0;
            }
            public Bend(Xrecord xrec)
            {
                TypedValue[] res = xrec.Data.AsArray();
                Numer_ = (int)res[0].Value;
                Pre_Bend_ = new PRE_BEND(new quaternion(0.0, (double)res[1].Value, (double)res[2].Value, (double)res[3].Value),
                    new quaternion(0.0, (double)res[4].Value, (double)res[5].Value, (double)res[6].Value));
                Normal_ = new quaternion(0.0, (double)res[7].Value, (double)res[8].Value, (double)res[9].Value);
                int iFictive = (int)res[10].Value;
                Fictive_ = (iFictive > 0) ? true : false;
                First_Triangle_Numer_ = (int)res[11].Value;
                Second_Triangle_Numer_ = (int)res[12].Value;
                StartNode_Numer_ = (int)res[13].Value;
                EndNode_Numer_ = (int)res[14].Value;
                Key_ = (string)res[15].Value;
                Length_ = (Pre_Bend_.Second - Pre_Bend_.First).abs();
                MidPoint_ = (Pre_Bend_.Second + Pre_Bend_.First) / 2.0;
                FirstTriangleOffset_ = (double)res[16].Value;
                SecondTriangleOffset_ = (double)res[17].Value;

                long ln = Convert.ToInt64((string)res[19].Value, 16);
                startSolidHandle_ = new UtilityClasses.Pair<int, Handle>((int)res[18].Value, new Handle(ln));

                long ln_ = Convert.ToInt64((string)res[21].Value, 16);
                endSolidHandle_ = new UtilityClasses.Pair<int, Handle>((int)res[20].Value, new Handle(ln_));

                long LN_ = Convert.ToInt64((string)res[23].Value, 16);
                SolidHandle_ = new UtilityClasses.Pair<int, Handle>((int)res[22].Value, new Handle(LN_));

                ExplicitNormal_ = (int)res[24].Value;

                explicit_cutSolidsThicknes_Start_ = (double)res[25].Value;
                explicit_cutSolidsThicknes_End_ = (double)res[26].Value;
                explicit_cutSolidsLenK_Start_ = (double)res[27].Value;
                explicit_cutSolidsLenK_End_ = (double)res[28].Value;
                explicit_first_cutExtrudeRatio_ = (double)res[29].Value;
                explicit_second_cutExtrudeRatio_ = (double)res[30].Value;
            }
            public Bend(StreamReader sr)
            {
                List<string> list = new List<string>();
                for (int i = 0; i < 31; i++)
                    list.Add(sr.ReadLine());
                Numer_ = int.Parse(list[0]);
                Pre_Bend_ = new PRE_BEND(new quaternion(0.0, double.Parse(list[1]), double.Parse(list[2]), double.Parse(list[3])),
                    new quaternion(0.0, double.Parse(list[4]), double.Parse(list[5]), double.Parse(list[6])));
                Normal_ = new quaternion(0.0, double.Parse(list[7]), double.Parse(list[8]), double.Parse(list[9]));
                int iFictive = int.Parse(list[10]);
                Fictive_ = (iFictive > 0) ? true : false;
                First_Triangle_Numer_ = int.Parse(list[11]);
                Second_Triangle_Numer_ = int.Parse(list[12]);
                StartNode_Numer_ = int.Parse(list[13]);
                EndNode_Numer_ = int.Parse(list[14]);
                Key_ = list[15];
                Length_ = (Pre_Bend_.Second - Pre_Bend_.First).abs();
                MidPoint_ = (Pre_Bend_.Second + Pre_Bend_.First) / 2.0;
                FirstTriangleOffset_ = double.Parse(list[16]);
                SecondTriangleOffset_ = double.Parse(list[17]);

                long ln = Convert.ToInt64(list[19], 16);
                startSolidHandle_ = new UtilityClasses.Pair<int, Handle>(int.Parse(list[18]), new Handle(ln));

                long ln_ = Convert.ToInt64(list[21], 16);
                endSolidHandle_ = new UtilityClasses.Pair<int, Handle>(int.Parse(list[20]), new Handle(ln_));

                long LN_ = Convert.ToInt64(list[23], 16);
                SolidHandle_ = new UtilityClasses.Pair<int, Handle>(int.Parse(list[22]), new Handle(LN_));

                ExplicitNormal_ = int.Parse(list[24]);
                explicit_cutSolidsThicknes_Start_ = double.Parse(list[25]);
                explicit_cutSolidsThicknes_End_ = double.Parse(list[26]);
                explicit_cutSolidsLenK_Start_ = double.Parse(list[27]);
                explicit_cutSolidsLenK_End_ = double.Parse(list[28]);
                explicit_first_cutExtrudeRatio_ = double.Parse(list[29]);
                explicit_second_cutExtrudeRatio_ = double.Parse(list[30]);
            }

            //-------- Functions -------------------------------------------------------------------
            public bool IsPeripheral() { return Second_Triangle_Numer_ < 0; }
            public bool IsPeripheralAndIsFictive() { return Second_Triangle_Numer_ < 0 && Fictive_; }
            public string GetKey() { return Key_; }
            public int GetNumer() { return Numer_; }
            public double GetLength() { return (Pre_Bend_.Second - Pre_Bend_.First).abs(); }
            public void SetNumer(int n) { Numer_ = n; }
            public bool IsFictive() { return Fictive_; }
            public int GetFirstTriangleNumer() { return First_Triangle_Numer_; }
            public int GetSecondTriangleNumer() { return Second_Triangle_Numer_; }
            public void SetFirstTriangleNumer(int n) { First_Triangle_Numer_ = n; }
            public void SetSecondTriangleNumer(int n) { Second_Triangle_Numer_ = n; }
            public void SetFirstTriangleNumer(Triangle t) { First_Triangle_Numer_ = t.GetNumer(); }
            public void SetSecondTriangleNumer(Triangle t) { Second_Triangle_Numer_ = t.GetNumer(); }
            public int GetStartNodeNumer() { return StartNode_Numer_; }
            public int GetFirstNodeNumer() { return StartNode_Numer_; }
            public int GetEndNodeNumer() { return EndNode_Numer_; }
            public int GetSecondNodeNumer() { return EndNode_Numer_; }
            public void SetStartNodeNumer(int n) { StartNode_Numer_ = n; }
            public void SetStartNodeNumer(Node n) { StartNode_Numer_ = n.GetNumer(); }
            public void SetFirstNodeNumer(int n) { StartNode_Numer_ = n; }
            public void SetFirstNodeNumer(Node n) { StartNode_Numer_ = n.GetNumer(); }
            public void SetEndNodeNumer(int n) { EndNode_Numer_ = n; }
            public void SetEndNodeNumer(Node n) { EndNode_Numer_ = n.GetNumer(); }
            public void SetSecondNodeNumer(int n) { EndNode_Numer_ = n; }
            public void SetSecondNodeNumer(Node n) { EndNode_Numer_ = n.GetNumer(); }
            public void SetFirstTriangleOffset(double d) { if (!Fictive_)FirstTriangleOffset_ = d; }
            public void SetSecondTriangleOffset(double d) { if (!Fictive_)SecondTriangleOffset_ = d; }

            public PRE_BEND GetPreBend() { return Pre_Bend_; }
            public void SetPreBend(quaternion first, quaternion second)
            {
                Pre_Bend_ = new PRE_BEND(first, second);
            }
            public void SetPreBend(Point3d first, Point3d second)
            {
                Pre_Bend_ = new PRE_BEND(new quaternion(0, first.X, first.Y, first.Z), new quaternion(0, second.X, second.Y, second.Z));
            }
            public void SetPreBend(quaternion first, Point3d second)
            {
                Pre_Bend_ = new PRE_BEND(first, new quaternion(0, second.X, second.Y, second.Z));
            }
            public void SetPreBend(Point3d first, quaternion second)
            {
                Pre_Bend_ = new PRE_BEND(new quaternion(0, first.X, first.Y, first.Z), second);
            }
            public quaternion GetFirst() { return Pre_Bend_.First; }
            public quaternion GetStart() { return Pre_Bend_.First; }
            public quaternion GetSecond() { return Pre_Bend_.Second; }
            public quaternion GetEnd() { return Pre_Bend_.Second; }
            public quaternion GetMid() { return MidPoint_; }
            public quaternion GetCentroid() { return MidPoint_; }
            public quaternion Centroid() { return MidPoint_; }
            public complex GetStartComplex(ref UCS ucs)
            {
                quaternion rez = ucs.FromACS(Pre_Bend_.First);

                return new complex(rez.GetX(), rez.GetY());
            }
            public complex GetEndComplex(ref UCS ucs)
            {
                quaternion rez = ucs.FromACS(Pre_Bend_.Second);

                return new complex(rez.GetX(), rez.GetY());
            }
            public plane GetPlane()
            {
                return new plane(MidPoint, Start, Normal);
            }
            public UCS GetUCS()
            {
                return new UCS(MidPoint, Start, Normal);
            }
            public UCS GetUCS_A()
            {
                return new UCS(MidPoint, End, Normal);
            }

            public quaternion GetNormal() { return Normal_; }
            public PRE_BEND GetNormalAsSegment() { return new PRE_BEND(Centroid(), Normal_); }
            public void SetNormal(Triangle t1, Triangle t2, double len = 1.0)
            {
                quaternion T1 = t1.GetNormalDirection() - t1.GetCentroid();
                quaternion T2 = t2.GetNormalDirection() - t2.GetCentroid();

                T1 /= T1.abs(); //dalgina = 1
                T2 /= T2.abs(); //dalgina = 1

                Normal_ = (T1 + T2) / 2.0;
                Normal_ /= Normal_.abs();//dalgina = 1
                Normal_ *= len;
                Normal_ += GetCentroid();

            }
            public void SetNormalByDirection(quaternion Start, quaternion End, double len = 1.0)
            {
                quaternion direction = End - Start;
                direction /= direction.abs();
                direction *= len;
                Normal_ = Centroid() + direction;
                UCS ucs = GetUCS();
                Normal_ = ucs.ToACS(new quaternion(0, 0, 1.0, 0));
            }
            public void ReverseNormal()
            {
                if (Normal_ != null)
                {
                    try
                    {
                        Normal_ = MidPoint_ + MidPoint_ - Normal_;
                    }
                    catch { }
                }
            }
            public quaternion GetStrchedNormalByGlassheight(double glassHeight, quaternion T)
            {
                quaternion q = Normal_ - MidPoint_;
                double ang = T.angTo(q);
                q /= q.abs();
                q *= (glassHeight / Math.Cos(ang));
                //q += MidPoint_;

                return q;
            }

            public int GetIndex(ref List<Bend> bendsLIST)
            {
                int rez = -1;
                foreach (Bend b in bendsLIST)
                {
                    int n = b.GetNumer();
                    if (n == Numer_)
                    {
                        rez = n;
                        break;
                    }

                }

                return rez;
            }
            public Xrecord GetXrecord()
            {
                Xrecord xRec = new Xrecord();
                if (Pre_Bend_ != null && (object)Normal_ != null)
                {
                    int iFictive = (Fictive_) ? 1 : -1;
                    xRec.Data = new ResultBuffer(
                          new TypedValue((int)DxfCode.Int32, Numer_),                //0
                          new TypedValue((int)DxfCode.Real, Pre_Bend_.First.GetX()), //1
                          new TypedValue((int)DxfCode.Real, Pre_Bend_.First.GetY()), //2
                          new TypedValue((int)DxfCode.Real, Pre_Bend_.First.GetZ()), //3
                          new TypedValue((int)DxfCode.Real, Pre_Bend_.Second.GetX()),//4
                          new TypedValue((int)DxfCode.Real, Pre_Bend_.Second.GetY()),//5
                          new TypedValue((int)DxfCode.Real, Pre_Bend_.Second.GetZ()),//6
                          new TypedValue((int)DxfCode.Real, Normal_.GetX()),   //7
                          new TypedValue((int)DxfCode.Real, Normal_.GetY()),   //8
                          new TypedValue((int)DxfCode.Real, Normal_.GetZ()),   //9
                          new TypedValue((int)DxfCode.Int32, iFictive),             //10
                          new TypedValue((int)DxfCode.Int32, First_Triangle_Numer_), //11
                          new TypedValue((int)DxfCode.Int32, Second_Triangle_Numer_),//12
                          new TypedValue((int)DxfCode.Int32, StartNode_Numer_),      //13
                          new TypedValue((int)DxfCode.Int32, EndNode_Numer_),        //14
                          new TypedValue((int)DxfCode.Text, Key_),                   //15
                          new TypedValue((int)DxfCode.Real, FirstTriangleOffset_),   //16
                          new TypedValue((int)DxfCode.Real, SecondTriangleOffset_),  //17 
                          new TypedValue((int)DxfCode.Int32, startSolidHandle_.First),  //18
                          new TypedValue((int)DxfCode.ExtendedDataHandle, startSolidHandle_.Second),//19  
                          new TypedValue((int)DxfCode.Int32, endSolidHandle_.First),  //20
                          new TypedValue((int)DxfCode.ExtendedDataHandle, endSolidHandle_.Second),//21
                          new TypedValue((int)DxfCode.Int32, SolidHandle_.First),  //22
                          new TypedValue((int)DxfCode.ExtendedDataHandle, SolidHandle_.Second),//23   
                          new TypedValue((int)DxfCode.Int32, ExplicitNormal_),//24
                          new TypedValue((int)DxfCode.Real, explicit_cutSolidsThicknes_Start_),//25
                          new TypedValue((int)DxfCode.Real, explicit_cutSolidsThicknes_End_),//26
                          new TypedValue((int)DxfCode.Real, explicit_cutSolidsLenK_Start_),//27
                          new TypedValue((int)DxfCode.Real, explicit_cutSolidsLenK_End_),//28
                          new TypedValue((int)DxfCode.Real, explicit_first_cutExtrudeRatio_),//29
                          new TypedValue((int)DxfCode.Real, explicit_second_cutExtrudeRatio_)//30
                          );
                }
                else
                {
                    xRec = null;
                }
                return xRec;
            }
            public void ToStream(StreamWriter sw)
            {
                int iFictive = (Fictive_) ? 1 : -1;

                sw.WriteLine(Numer_);//0
                sw.WriteLine(Pre_Bend_.First.GetX());//1
                sw.WriteLine(Pre_Bend_.First.GetY());//2
                sw.WriteLine(Pre_Bend_.First.GetZ());//3
                sw.WriteLine(Pre_Bend_.Second.GetX()); //4
                sw.WriteLine(Pre_Bend_.Second.GetY());//5
                sw.WriteLine(Pre_Bend_.Second.GetZ());//6
                sw.WriteLine(Normal_.GetX()); //7
                sw.WriteLine(Normal_.GetY()); //8
                sw.WriteLine(Normal_.GetZ());//9
                sw.WriteLine(iFictive);//10
                sw.WriteLine(First_Triangle_Numer_);//11
                sw.WriteLine(Second_Triangle_Numer_);//12
                sw.WriteLine(StartNode_Numer_);//13
                sw.WriteLine(EndNode_Numer_);//14
                sw.WriteLine(Key_);//15
                sw.WriteLine(FirstTriangleOffset_); //16
                sw.WriteLine(SecondTriangleOffset_); //17
                sw.WriteLine(startSolidHandle_.First); //18
                sw.WriteLine(startSolidHandle_.Second); //19
                sw.WriteLine(endSolidHandle_.First); //20
                sw.WriteLine(endSolidHandle_.Second); //21
                sw.WriteLine(SolidHandle_.First); //22
                sw.WriteLine(SolidHandle_.Second); //23
                sw.WriteLine(ExplicitNormal_); //24
                sw.WriteLine(explicit_cutSolidsThicknes_Start_);//25
                sw.WriteLine(explicit_cutSolidsThicknes_End_);//26
                sw.WriteLine(explicit_cutSolidsLenK_Start_);//27
                sw.WriteLine(explicit_cutSolidsLenK_End_);//28
                sw.WriteLine(explicit_first_cutExtrudeRatio_);//29
                sw.WriteLine(explicit_second_cutExtrudeRatio_);//30

            }
            //--- Operators ------------------------------------------------------------------------
            public override bool Equals(Object obj)
            {
                return ((obj is Bend) && (this == (Bend)obj));
            }
            public override int GetHashCode()
            {
                return (Numer_.GetHashCode() ^ Pre_Bend_.GetHashCode() ^ Fictive_.GetHashCode() ^
                    First_Triangle_Numer_.GetHashCode() ^ Second_Triangle_Numer_.GetHashCode() ^
                    StartNode_Numer_.GetHashCode() ^ EndNode_Numer_.GetHashCode());
            }
            public override string ToString()
            {
                String rez = String.Format("Bend\n-----\n\nNumer: {0} \nKey: {1}\nStart Node Numer: {2}\nEnd Node Numer: {3}\nFirst Triangle Numer: {4}\nSecond Triangle Numer: {5}\n",
                    Numer_, Key_, StartNode_Numer_, EndNode_Numer_, First_Triangle_Numer_, Second_Triangle_Numer_);
                if (Pre_Bend_ != null)
                    rez += String.Format("Start: {0} , {1} , {2}\nEnd: {3} , {4} , {5}\n",
                        Pre_Bend_.First.GetX(), Pre_Bend_.First.GetY(), Pre_Bend_.First.GetZ(),
                        Pre_Bend_.Second.GetX(), Pre_Bend_.Second.GetY(), Pre_Bend_.Second.GetZ());
                else
                    rez += "null\n";

                if (Normal_ != null)
                    rez += String.Format("Normal: {0} , {1}, {2}\n", Normal_.GetX(), Normal_.GetY(), Normal_.GetZ());
                else
                    rez += "null\n";

                if (Fictive_)
                {
                    rez += "\n Fictive = true";
                }
                else
                {
                    rez += "\n Fictive = false";
                }
                return rez;
            }

            public static bool operator ==(Bend b1, Bend b2)
            {
                bool rez = false;

                if (((b1.StartNode_Numer_ == b2.StartNode_Numer_) || (b1.StartNode_Numer_ == b2.EndNode_Numer_)) &&
                    ((b1.EndNode_Numer_ == b2.StartNode_Numer_) || (b1.EndNode_Numer_ == b2.EndNode_Numer_)) && (b1.StartNode_Numer_ != b1.EndNode_Numer_))
                {
                    rez = true;
                }

                return rez;
            }
            public static bool operator !=(Bend b1, Bend b2)
            {
                return (!(b1 == b2));
            }

            public static bool operator ==(Bend b1, PRE_BEND b2)
            {
                return UtilityClasses.GlobalFunctions.IsEqualPreBend(b1.Pre_Bend_, b2);
            }
            public static bool operator !=(Bend b1, PRE_BEND b2)
            {
                return (!(b1 == b2));
            }

            public static bool operator ==(PRE_BEND b2, Bend b1)
            {
                return UtilityClasses.GlobalFunctions.IsEqualPreBend(b1.Pre_Bend_, b2);
            }
            public static bool operator !=(PRE_BEND b2, Bend b1)
            {
                return (!(b1 == b2));
            }

            public static bool operator ==(Bend b1, UtilityClasses.Pair<int, int> b2)
            {
                bool bb1 = b1.StartNodeNumer == b2.First || b1.StartNodeNumer == b2.Second;
                bool bb2 = b1.EndNodeNumer == b2.First || b1.EndNodeNumer == b2.Second;
                bool bb3 = b1.StartNodeNumer == b1.EndNodeNumer;

                return bb1 && bb2 && !bb3;
            }
            public static bool operator !=(Bend b1, UtilityClasses.Pair<int, int> b2)
            {
                return (!(b1 == b2));
            }

            public static bool operator ==(UtilityClasses.Pair<int, int> b2, Bend b1)
            {
                bool bb1 = b1.StartNodeNumer == b2.First || b1.StartNodeNumer == b2.Second;
                bool bb2 = b1.EndNodeNumer == b2.First || b1.EndNodeNumer == b2.Second;
                bool bb3 = b1.StartNodeNumer == b1.EndNodeNumer;

                return bb1 && bb2 && !bb3;
            }
            public static bool operator !=(UtilityClasses.Pair<int, int> b2, Bend b1)
            {
                return (!(b1 == b2));
            }

            public static bool operator ==(quaternion q, Bend b)
            {
                quaternion q1 = q - b.Pre_Bend.First;
                quaternion q2 = q - b.Pre_Bend.Second;

                return Math.Abs((b.Pre_Bend.First - b.Pre_Bend.Second).abs() - q1.abs() - q2.abs()) < Constants.zero_dist;
            }
            public static bool operator !=(quaternion q, Bend b)
            {
                return (!(q == b));
            }

            public static bool operator ==(Bend b, quaternion q)
            {
                quaternion q1 = q - b.Pre_Bend.First;
                quaternion q2 = q - b.Pre_Bend.Second;

                return Math.Abs((b.Pre_Bend.First - b.Pre_Bend.Second).abs() - q1.abs() - q2.abs()) < Constants.zero_dist;
            }
            public static bool operator !=(Bend b, quaternion q)
            {
                return (!(q == b));
            }


            public static bool IsPrebendsEqual(PRE_BEND b1, PRE_BEND b2)
            {
                return UtilityClasses.GlobalFunctions.IsEqualPreBend(b1, b2);
            }

            //-----------------------------
            public void FillStartNode_Number(ref List<Node> nodesARR)
            {
                for (int i = 0; i < nodesARR.Count; i++)
                {
                    if (Pre_Bend_.First == nodesARR[i])
                    {
                        StartNode_Numer_ = i;
                        break;
                    }
                }
            }
            public void FillEndNode_Number(ref List<Node> nodesARR)
            {
                for (int i = 0; i < nodesARR.Count; i++)
                {
                    if (Pre_Bend_.Second == nodesARR[i])
                    {
                        EndNode_Numer_ = i;
                        break;
                    }
                }
            }

            //--------------------------------
            public UtilityClasses.Triplet<int, double, quaternion> CenterOfGravity(double densityB = 1.0, double densityN = 1.0)
            {
                UtilityClasses.Triplet<int, double, quaternion> rez =
                    new UtilityClasses.Triplet<int, double, quaternion>(-1, -1.0, new quaternion());

                if (Fictive_)
                    return new UtilityClasses.Triplet<int, double, quaternion>(-1, -1.0, new quaternion());

                double solidMass = 0.0;
                quaternion solidCentroid = new quaternion();

                double npzzleMass_s = 0.0;
                quaternion npzzle_S_Centroid = new quaternion();

                double npzzleMass_e = 0.0;
                quaternion npzzle_E_Centroid = new quaternion();

                using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = tr.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;


                    if (SolidHandle_.First >= 0)
                    {
                        try
                        {
                            Solid3d SOLID = tr.GetObject(UtilityClasses.GlobalFunctions.GetObjectId(SolidHandle_.Second), OpenMode.ForRead) as Solid3d;
                            solidMass = SOLID.MassProperties.Volume * densityB;
                            solidCentroid = new quaternion(0, SOLID.MassProperties.Centroid.X, SOLID.MassProperties.Centroid.Y, SOLID.MassProperties.Centroid.Z);
                        }
                        catch { }
                    }

                    if (endSolidHandle_.First >= 0)
                    {
                        try
                        {
                            BlockReference br = tr.GetObject(UtilityClasses.GlobalFunctions.GetObjectId(endSolidHandle_.Second), OpenMode.ForRead) as BlockReference;
                            DBObjectCollection Coll = new DBObjectCollection();
                            br.Explode(Coll);

                            foreach (DBObject obj in Coll)
                            {
                                acBlkTblRec.AppendEntity((Entity)obj);
                                tr.AddNewlyCreatedDBObject(obj, true);

                                Solid3d sol = tr.GetObject(obj.ObjectId, OpenMode.ForWrite) as Solid3d;
                                npzzleMass_e += sol.MassProperties.Volume * densityN;
                                Point3d tempP = sol.MassProperties.Centroid;
                                quaternion tempQ = new quaternion(0, tempP.X, tempP.Y, tempP.Z);
                                npzzle_E_Centroid += sol.MassProperties.Volume * densityN * tempQ;
                            }
                            if (npzzleMass_e > 0)
                                npzzle_E_Centroid /= npzzleMass_e;
                        }
                        catch { }

                    }

                    if (startSolidHandle_.First >= 0)
                    {
                        try
                        {
                            BlockReference br = tr.GetObject(UtilityClasses.GlobalFunctions.GetObjectId(startSolidHandle_.Second), OpenMode.ForRead) as BlockReference;
                            DBObjectCollection Coll = new DBObjectCollection();
                            br.Explode(Coll);

                            foreach (DBObject obj in Coll)
                            {
                                acBlkTblRec.AppendEntity((Entity)obj);
                                tr.AddNewlyCreatedDBObject(obj, true);

                                Solid3d sol = tr.GetObject(obj.ObjectId, OpenMode.ForWrite) as Solid3d;
                                npzzleMass_s += sol.MassProperties.Volume * densityN;
                                Point3d tempP = sol.MassProperties.Centroid;
                                quaternion tempQ = new quaternion(0, tempP.X, tempP.Y, tempP.Z);
                                npzzle_S_Centroid += sol.MassProperties.Volume * densityN * tempQ;
                            }
                            if (npzzleMass_s > 0)
                                npzzle_S_Centroid /= npzzleMass_s;
                        }
                        catch { }

                    }

                    double mass = npzzleMass_s + npzzleMass_e + solidMass;
                    if (mass > 0.0)
                    {
                        quaternion cQ = (solidCentroid * solidMass + npzzleMass_s * npzzle_S_Centroid + npzzleMass_e * npzzle_E_Centroid) / mass;
                        rez = new UtilityClasses.Triplet<int, double, quaternion>(1, mass, cQ);
                    }
                    //tr.Commit();
                }
                return rez;
            }

            public UtilityClasses.Pair<Polyline, Point3dCollection> GetCutSolid(quaternion positiveDirectionByZ_pointer, double offset = 0.0)
            {
                double thicknes = UtilityClasses.ConstantsAndSettings.cutSolidsThicknes;
                double m = UtilityClasses.ConstantsAndSettings.cutSolidsLenK;
                Polyline pl = null;

                double thS = (explicit_cutSolidsThicknes_Start_ >= 0.0) ? explicit_cutSolidsThicknes_Start_ : thicknes;
                double thE = (explicit_cutSolidsThicknes_End_ >= 0.0) ? explicit_cutSolidsThicknes_End_ : thicknes;

                double mS = (explicit_cutSolidsLenK_Start_ >= 0.0) ? explicit_cutSolidsLenK_Start_ : m;
                double mE = (explicit_cutSolidsLenK_End_ >= 0.0) ? explicit_cutSolidsLenK_End_ : m;

                UtilityClasses.Pair<Polyline, Point3dCollection> rez = new UtilityClasses.Pair<Polyline, Point3dCollection>(null, null);

                UCS ucs = GetUCS();
                if ((ucs.FromACS(positiveDirectionByZ_pointer)).GetZ() < 0.0)
                {
                    ucs = GetUCS_A();
                    double buff = thS; thS = thE; thE = buff;
                    buff = mS; mS = mE; mE = buff;
                }




                Point3dCollection list = new Point3dCollection();
                quaternion Q = new quaternion(0, 0, -thE, offset);
                list.Add((Point3d)(ucs.ToACS(Q)));

                quaternion Q1 = new quaternion(0, Length_ * mS, -thE, offset);
                list.Add((Point3d)(ucs.ToACS(Q1)));

                quaternion Q2 = new quaternion(0, Length_ * mS, thS, offset);
                list.Add((Point3d)(ucs.ToACS(Q2)));

                quaternion Q3 = new quaternion(0, -Length_ * mE, thS, offset);
                list.Add((Point3d)(ucs.ToACS(Q3)));

                quaternion Q4 = new quaternion(0, -Length_ * mE, -thE, offset);
                list.Add((Point3d)(ucs.ToACS(Q4)));

                quaternion Q5 = new quaternion(0, 0, -thE, offset);
                list.Add((Point3d)(ucs.ToACS(Q5)));


                pl = UtilityClasses.GlobalFunctions.GetPoly(list);

                rez = new UtilityClasses.Pair<Polyline, Point3dCollection>(pl, list);

                return rez;
            }

        }
        public class Triangle
        {
            #region Members
            private string Key_;
            public string Key { get { return (Key_); } }

            private int Numer_;
            public int Numer { get { return (Numer_); } set { Numer_ = value; } }

            private PRE_BEND Normal_;
            public PRE_BEND Normal { get { return (Normal_); } set { Normal_ = value; } }

            private PRE_TRIANGLE Nodes_;
            public PRE_TRIANGLE Nodes { get { return (Nodes_); } }

            private UtilityClasses.Pair<int, bool>[] Bends_; //<bend numer, fictive>
            public UtilityClasses.Pair<int, bool>[] Bends { get { return (Bends_); } }

            private int[] NodesNumers_;
            public int[] NodesNumers { get { return (NodesNumers_); } }

            private int PolygonNumer_;
            public int PolygonNumer { get { return (PolygonNumer_); } set { PolygonNumer_ = value; } }

            private UtilityClasses.Pair<int, Handle> lowSolidHandle_;//int is validhandle, int<0 - not valid
            public UtilityClasses.Pair<int, Handle> lowSolidHandle { get { return (lowSolidHandle_); } set { lowSolidHandle_ = value; } }

            private UtilityClasses.Pair<int, Handle> upSolidHandle_;//int is validhandle, int<0 - not valid
            public UtilityClasses.Pair<int, Handle> upSolidHandle { get { return (upSolidHandle_); } set { upSolidHandle_ = value; } }

            #endregion

            #region constructors
            public Triangle()
            {
                Numer_ = -1;
                Bends_ = null;
                NodesNumers_ = null;
                Normal_ = null;
                Nodes_ = null;
                Key_ = Guid.NewGuid().ToString();
                PolygonNumer_ = -1;
                lowSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                upSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));

            }
            public Triangle(ref Triangle t, bool bNewKey = false)
            {
                Numer_ = t.Numer_;

                if (t.Bends_ != null)
                    Bends_ = new UtilityClasses.Pair<int, bool>[3] { t.Bends_[0], t.Bends_[1], t.Bends_[2] };
                else
                    Bends_ = null;

                if (t.NodesNumers_ != null)
                    NodesNumers_ = new int[3] { t.NodesNumers_[0], t.NodesNumers_[1], t.NodesNumers_[2] };
                else
                    NodesNumers_ = null;

                Nodes_ = t.Nodes_;
                Normal_ = t.Normal_;

                PolygonNumer_ = t.PolygonNumer_;

                if (bNewKey)
                {
                    Key_ = Guid.NewGuid().ToString();
                    lowSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                    upSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));

                }
                else
                {
                    Key_ = t.Key_;
                    lowSolidHandle_ = new UtilityClasses.Pair<int, Handle>(t.lowSolidHandle_.First, t.lowSolidHandle_.Second);
                    upSolidHandle_ = new UtilityClasses.Pair<int, Handle>(t.upSolidHandle_.First, t.upSolidHandle_.Second);
                }
            }
            public Triangle(int FirstBend_Numer, int FirstNode_Numer, int numer, bool b_IsFictive)
            {
                Numer_ = numer;
                Bends_ = new UtilityClasses.Pair<int, bool>[3] { new UtilityClasses.Pair<int, bool>(FirstBend_Numer, b_IsFictive),
                new UtilityClasses.Pair<int, bool>(-1,false),
                new UtilityClasses.Pair<int, bool>(-1,false)};
                NodesNumers_ = new int[3] { FirstNode_Numer, -1, -1 };
                Normal_ = null;
                Nodes_ = null;
                Key_ = Guid.NewGuid().ToString();
                PolygonNumer_ = -1;
                lowSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
                upSolidHandle_ = new UtilityClasses.Pair<int, Handle>(-1, new Handle(-1));
            }
            public Triangle(Xrecord xrec)
            {
                TypedValue[] res = xrec.Data.AsArray();
                Numer_ = (int)res[0].Value;
                Bends_ = new UtilityClasses.Pair<int, bool>[3] {
                 new UtilityClasses.Pair<int, bool>((int)res[1].Value, ((int)res[22].Value > 0) ? true : false), 
                 new UtilityClasses.Pair<int, bool>((int)res[2].Value, ((int)res[23].Value > 0) ? true : false),
                 new UtilityClasses.Pair<int, bool>((int)res[3].Value, ((int)res[24].Value > 0) ? true : false)};
                NodesNumers_ = new int[3] { (int)res[4].Value, (int)res[5].Value, (int)res[6].Value };
                Normal_ = new PRE_BEND(new quaternion(0.0, (double)res[7].Value, (double)res[8].Value, (double)res[9].Value),
                                              new quaternion(0.0, (double)res[10].Value, (double)res[11].Value, (double)res[12].Value));
                Nodes_ = new PRE_TRIANGLE(new quaternion(0.0, (double)res[13].Value, (double)res[14].Value, (double)res[15].Value),
                                              new quaternion(0.0, (double)res[16].Value, (double)res[17].Value, (double)res[18].Value),
                                              new quaternion(0.0, (double)res[19].Value, (double)res[20].Value, (double)res[21].Value));
                PolygonNumer_ = (int)res[25].Value;
                Key_ = (string)res[26].Value;

                long ln = Convert.ToInt64((string)res[28].Value, 16);
                lowSolidHandle_ = new UtilityClasses.Pair<int, Handle>((int)res[27].Value, new Handle(ln));

                long ln_ = Convert.ToInt64((string)res[30].Value, 16);
                upSolidHandle_ = new UtilityClasses.Pair<int, Handle>((int)res[29].Value, new Handle(ln_));

            }
            public Triangle(StreamReader sr)
            {
                List<string> list = new List<string>();
                for (int i = 0; i < 31; i++)
                    list.Add(sr.ReadLine());

                Numer_ = int.Parse(list[0]);
                Bends_ = new UtilityClasses.Pair<int, bool>[3] {
                 new UtilityClasses.Pair<int, bool>(int.Parse(list[1]), (int.Parse(list[22]) > 0) ? true : false), 
                 new UtilityClasses.Pair<int, bool>(int.Parse(list[2]), (int.Parse(list[23]) > 0) ? true : false),
                 new UtilityClasses.Pair<int, bool>(int.Parse(list[3]), (int.Parse(list[24]) > 0) ? true : false)};
                NodesNumers_ = new int[3] { int.Parse(list[4]), int.Parse(list[5]), int.Parse(list[6]) };
                Normal_ = new PRE_BEND(new quaternion(0.0, double.Parse(list[7]), double.Parse(list[8]), double.Parse(list[9])),
                                              new quaternion(0.0, double.Parse(list[10]), double.Parse(list[11]), double.Parse(list[12])));
                Nodes_ = new PRE_TRIANGLE(new quaternion(0.0, double.Parse(list[13]), double.Parse(list[14]), double.Parse(list[15])),
                                              new quaternion(0.0, double.Parse(list[16]), double.Parse(list[17]), double.Parse(list[18])),
                                              new quaternion(0.0, double.Parse(list[19]), double.Parse(list[20]), double.Parse(list[21])));
                PolygonNumer_ = int.Parse(list[25]);
                Key_ = list[26];

                long ln = Convert.ToInt64(list[28], 16);
                lowSolidHandle_ = new UtilityClasses.Pair<int, Handle>(int.Parse(list[27]), new Handle(ln));

                long ln_ = Convert.ToInt64(list[30], 16);
                upSolidHandle_ = new UtilityClasses.Pair<int, Handle>(int.Parse(list[29]), new Handle(ln_));
            }
            #endregion

            #region Get Set
            public int GetNumer() { return Numer_; }
            public void SetNumer(int n) { Numer_ = n; }
            public string GetKey() { return Key; }
            public PRE_TRIANGLE GetPreNodes() { return Nodes_; }
            public void SetPreNodes(PRE_TRIANGLE pt)
            {
                if (pt != null)
                    Nodes_ = new PRE_TRIANGLE(pt.First, pt.Second, pt.Third);
                else
                    Nodes_ = null;
            }
            public bool IsFirstBendFictive() { return Bends_[0].Second; }
            public bool IsSecondBendFictive() { return Bends_[1].Second; }
            public bool IsThirdBendFictive() { return Bends_[2].Second; }
            public int GetFirstBendNumer() { return Bends_[0].First; }
            public int GetSecondBendNumer() { return Bends_[1].First; }
            public int GetThirdBendNumer() { return Bends_[2].First; }
            public void SetFirstBendNumer(int n) { Bends_[0].First = n; }
            public void SetFirstBendNumer(int n, bool b) { Bends_[0] = new UtilityClasses.Pair<int, bool>(n, b); }
            public void SetSecondBendNumer(int n) { Bends_[1].First = n; }
            public void SetSecondBendNumer(int n, bool b) { Bends_[1] = new UtilityClasses.Pair<int, bool>(n, b); }
            public void SetThirdBendNumer(int n) { Bends_[2].First = n; }
            public void SetThirdBendNumer(int n, bool b) { Bends_[2] = new UtilityClasses.Pair<int, bool>(n, b); }

            public int[] GetNodesNumers() { return NodesNumers_; }
            private void SetNodesNumers(int[] n)
            {
                if ((n != null) && (n[0] != n[1]) && (n[0] != n[2]) && (n[1] != n[2]))
                {
                    NodesNumers_ = new int[3] { n[0], n[1], n[2] };
                }
            }
            private void SetNodesNumers(int n1, int n2, int n3)
            {
                if ((n1 != n2) && (n1 != n3) && (n2 != n3))
                {
                    NodesNumers_ = new int[3] { n1, n2, n3 };
                }
            }
            private void SetNodesNumers(Node[] N)
            {
                int[] n = new int[3] { N[0].GetNumer(), N[1].GetNumer(), N[2].GetNumer() };
                if ((n != null) && (n[0] != n[1]) && (n[0] != n[2]) && (n[1] != n[2]))
                {
                    NodesNumers_ = new int[3] { n[0], n[1], n[2] };
                }
            }
            private void SetNodesNumers(Node N1, Node N2, Node N3)
            {
                int[] n = new int[3] { N1.GetNumer(), N2.GetNumer(), N3.GetNumer() };
                if ((n != null) && (n[0] != n[1]) && (n[0] != n[2]) && (n[1] != n[2]))
                {
                    NodesNumers_ = new int[3] { n[0], n[1], n[2] };
                }
            }
            public void SetFirstNodeNumer(int n)
            {
                if ((object)NodesNumers_ == null)
                    NodesNumers_ = new int[3] { n, -1, -1 };
                else
                    NodesNumers_[0] = n;
            }
            public void SetSecondNodeNumer(int n)
            {
                if ((object)NodesNumers_ == null)
                    NodesNumers_ = new int[3] { -1, n, -1 };
                else
                    NodesNumers_[1] = n;
            }
            public void SetThirdNodeNumer(int n)
            {
                if ((object)NodesNumers_ == null)
                    NodesNumers_ = new int[3] { -1, -1, n };
                else
                    NodesNumers_[2] = n;
            }

            public UtilityClasses.Pair<int, bool>[] Getbends() { return Bends_; }
            public void SetBends(Bend b1, Bend b2, Bend b3, List<Node> nodesLIST)
            {
                int n11 = b2.GetStartNodeNumer();
                int n12 = b2.GetEndNodeNumer();

                int n21 = b3.GetStartNodeNumer();
                int n22 = b3.GetEndNodeNumer();

                int[] nodesnumers = new int[3] { b1.GetStartNodeNumer(), b1.GetEndNodeNumer(), -1 };
                if ((n11 != nodesnumers[0]) && (n11 != nodesnumers[1]))
                {
                    nodesnumers[2] = n11;
                }
                else
                {
                    nodesnumers[2] = n12;
                }

                if ((n21 == nodesnumers[0] || n21 == nodesnumers[1] || n21 == nodesnumers[2]) &&
                    (n22 == nodesnumers[0] || n22 == nodesnumers[1] || n22 == nodesnumers[2]) && (n21 != n22))
                {
                    if ((nodesnumers[0] != nodesnumers[1]) && (nodesnumers[0] != nodesnumers[2]) && (nodesnumers[1] != nodesnumers[2]))
                    {
                        Nodes_ = new PRE_TRIANGLE(nodesLIST[nodesnumers[0]].GetPosition(), nodesLIST[nodesnumers[1]].GetPosition(), nodesLIST[nodesnumers[2]].GetPosition());
                        SetNodesNumers(nodesnumers);
                        Bends_ = new UtilityClasses.Pair<int, bool>[3] 
                        { new UtilityClasses.Pair<int, bool>(b1.GetNumer(),b1.IsFictive()),
                        new UtilityClasses.Pair<int, bool>(b2.GetNumer(),b2.IsFictive()),
                        new UtilityClasses.Pair<int, bool>(b3.GetNumer(),b3.IsFictive())};
                    }
                }

            }
            public quaternion GetCentroid()
            {
                quaternion centroid = new quaternion();
                if (Normal_ != null)
                {
                    centroid = Normal_.First;
                }
                else
                {
                    centroid = (Nodes_.First + Nodes_.Second) / 2.0;
                    centroid = centroid - Nodes_.Third;
                    centroid *= (2.0 / 3.0);
                    centroid = Nodes_.Third + centroid;
                }

                return centroid;
            }
            public Xrecord GetXrecord()
            {
                Xrecord xRec = new Xrecord();
                if (Bends_ != null && NodesNumers_ != null && (object)Normal_ != null && Nodes_ != null)
                {
                    int FictiveFirst = Bends_[0].Second ? 1 : -1;
                    int FictiveSecond = Bends_[1].Second ? 1 : -1;
                    int FictiveThird = Bends_[2].Second ? 1 : -1;

                    xRec.Data = new ResultBuffer(
                          new TypedValue((int)DxfCode.Int32, Numer_),              //0
                          new TypedValue((int)DxfCode.Int32, Bends_[0].First),     //1
                          new TypedValue((int)DxfCode.Int32, Bends_[1].First),     //2
                          new TypedValue((int)DxfCode.Int32, Bends_[2].First),     //3
                          new TypedValue((int)DxfCode.Int32, NodesNumers_[0]),     //4
                          new TypedValue((int)DxfCode.Int32, NodesNumers_[1]),     //5
                          new TypedValue((int)DxfCode.Int32, NodesNumers_[2]),     //6               
                          new TypedValue((int)DxfCode.Real, Normal_.First.GetX()),    //7
                          new TypedValue((int)DxfCode.Real, Normal_.First.GetY()),    //8
                          new TypedValue((int)DxfCode.Real, Normal_.First.GetZ()),    //9
                          new TypedValue((int)DxfCode.Real, Normal_.Second.GetX()),   //10
                          new TypedValue((int)DxfCode.Real, Normal_.Second.GetY()),   //11
                          new TypedValue((int)DxfCode.Real, Normal_.Second.GetZ()),   //12
                          new TypedValue((int)DxfCode.Real, Nodes_.First.GetX()),     //13
                          new TypedValue((int)DxfCode.Real, Nodes_.First.GetY()),     //14
                          new TypedValue((int)DxfCode.Real, Nodes_.First.GetZ()),     //15
                          new TypedValue((int)DxfCode.Real, Nodes_.Second.GetX()),    //16
                          new TypedValue((int)DxfCode.Real, Nodes_.Second.GetY()),    //17
                          new TypedValue((int)DxfCode.Real, Nodes_.Second.GetZ()),    //18
                          new TypedValue((int)DxfCode.Real, Nodes_.Third.GetX()),     //19
                          new TypedValue((int)DxfCode.Real, Nodes_.Third.GetY()),     //20
                          new TypedValue((int)DxfCode.Real, Nodes_.Third.GetZ()),     //21
                          new TypedValue((int)DxfCode.Int32, FictiveFirst),         //22
                          new TypedValue((int)DxfCode.Int32, FictiveSecond),        //23
                          new TypedValue((int)DxfCode.Int32, FictiveThird),         //24
                          new TypedValue((int)DxfCode.Int32, PolygonNumer_),     //25
                          new TypedValue((int)DxfCode.Text, Key),                //26
                          new TypedValue((int)DxfCode.Int32, lowSolidHandle_.First),  //27
                          new TypedValue((int)DxfCode.ExtendedDataHandle, lowSolidHandle_.Second),//28  
                          new TypedValue((int)DxfCode.Int32, upSolidHandle_.First),  //29
                          new TypedValue((int)DxfCode.ExtendedDataHandle, upSolidHandle_.Second)); //30                 
                }
                else
                {
                    xRec = null;
                }
                return xRec;
            }
            /// <GetFictiveRange>
            /// 0 - 0 ficive bends
            /// 1 - 1 ficive bends
            /// 2 - 2 ficive bends
            /// 3 - 3 ficive bends
            /// </GetFictiveRange>
            /// <returns></returns>
            public int GetFictiveRange()
            {
                int rez = 0;

                if (Bends_[0].Second) { rez++; }
                if (Bends_[1].Second) { rez++; }
                if (Bends_[2].Second) { rez++; }

                return rez;
            }
            public UtilityClasses.Pair<int, int> GetOtherNodeNumers(int NodeNumer)
            {
                UtilityClasses.Pair<int, int> pa = new UtilityClasses.Pair<int, int>(-1, -1);
                if (NodeNumer == NodesNumers_[0] || NodeNumer == NodesNumers_[1] || NodeNumer == NodesNumers_[2])
                {
                    int f, s, t;
                    f = NodeNumer;
                    s = t = -1;
                    if (NodeNumer == NodesNumers_[0]) { s = NodesNumers_[1]; t = NodesNumers_[2]; }
                    if (NodeNumer == NodesNumers_[1]) { s = NodesNumers_[0]; t = NodesNumers_[2]; }
                    if (NodeNumer == NodesNumers_[2]) { s = NodesNumers_[0]; t = NodesNumers_[1]; }

                    pa.First = s; pa.Second = t;
                }
                return pa;
            }
            public double GetArea()
            {
                double rez = 0.0;
                try
                {
                    UCS ucs = new UCS(Nodes_.First, Nodes_.Second, Nodes_.Third);
                    quaternion node3 = ucs.FromACS(Nodes_.Third);
                    rez = (Nodes_.Second - Nodes_.First).abs() * node3.GetY() / 2.0;
                }
                catch { }

                return double.IsNaN(rez) ? 0.0 : rez;
            }
            public int GetNearestNode(ref UtilityClasses.Containers container, quaternion q)
            {
                int rez = -1;

                quaternion f = container.Nodes[NodesNumers_[0]].Position;
                quaternion s = container.Nodes[NodesNumers_[1]].Position;
                quaternion t = container.Nodes[NodesNumers_[2]].Position;

                double[] d = new double[] { (q - f).abs(), (q - s).abs(), (q - t).abs() };
                int[] m = new int[] { -1, -1, -1 };

                m[0] = NodesNumers_[0];
                m[1] = NodesNumers_[1];
                m[2] = NodesNumers_[2];

                if (d[1] < d[0]) { m[0] = m[1]; m[1] = NodesNumers_[0]; d[0] = d[1]; d[1] = (q - f).abs(); }
                rez = (d[2] < d[0]) ? m[2] : m[0];

                return rez;
            }

            #endregion

            public void ToStream(StreamWriter sw)
            {
                int FictiveFirst = Bends_[0].Second ? 1 : -1;
                int FictiveSecond = Bends_[1].Second ? 1 : -1;
                int FictiveThird = Bends_[2].Second ? 1 : -1;

                sw.WriteLine(Numer_);//0
                sw.WriteLine(Bends_[0].First);//1
                sw.WriteLine(Bends_[1].First);//2
                sw.WriteLine(Bends_[2].First);//3
                sw.WriteLine(NodesNumers_[0]);//4
                sw.WriteLine(NodesNumers_[1]);//5
                sw.WriteLine(NodesNumers_[2]);//6
                sw.WriteLine(Normal_.First.GetX());//7
                sw.WriteLine(Normal_.First.GetY());//8
                sw.WriteLine(Normal_.First.GetZ());//9
                sw.WriteLine(Normal_.Second.GetX());//10
                sw.WriteLine(Normal_.Second.GetY());//11
                sw.WriteLine(Normal_.Second.GetZ());//12
                sw.WriteLine(Nodes_.First.GetX());//13
                sw.WriteLine(Nodes_.First.GetY());//14
                sw.WriteLine(Nodes_.First.GetZ());//15
                sw.WriteLine(Nodes_.Second.GetX());//16
                sw.WriteLine(Nodes_.Second.GetY());//17
                sw.WriteLine(Nodes_.Second.GetZ());//18
                sw.WriteLine(Nodes_.Third.GetX());//19
                sw.WriteLine(Nodes_.Third.GetY());//20
                sw.WriteLine(Nodes_.Third.GetZ());//21
                sw.WriteLine(FictiveFirst);//22
                sw.WriteLine(FictiveSecond);//23
                sw.WriteLine(FictiveThird);//24
                sw.WriteLine(PolygonNumer_);//25
                sw.WriteLine(Key_);//26
                sw.WriteLine(lowSolidHandle_.First);//27
                sw.WriteLine(lowSolidHandle_.Second);//28
                sw.WriteLine(upSolidHandle_.First);//29
                sw.WriteLine(upSolidHandle_.Second);//30
            }

            public void RotateIndexesToUp(List<Bend> bendsLIST, ref  List<Node> nodesLIST)
            {
                if ((Bends_ != null) && (NodesNumers_ != null) && (Nodes_ != null))
                {
                    int b3 = Bends_[0].First; int b1 = Bends_[1].First; int b2 = Bends_[2].First;
                    Bends_ = null;
                    NodesNumers_ = null;
                    Nodes_ = null;
                    SetBends(bendsLIST[b1], bendsLIST[b2], bendsLIST[b3], nodesLIST);
                }
            }
            public void RotateIndexesToDown(ref List<Bend> bendsLIST, ref  List<Node> nodesLIST)
            {
                if ((Bends_ != null) && (NodesNumers_ != null) && (Nodes_ != null))
                {
                    int b1 = Bends_[2].First; int b3 = Bends_[1].First; int b2 = Bends_[0].First;
                    Bends_ = null;
                    NodesNumers_ = null;
                    Nodes_ = null;
                    SetBends(bendsLIST[b1], bendsLIST[b2], bendsLIST[b3], nodesLIST);
                }
            }

            #region normals and centroids
            public quaternion GetNormalDirection(quaternion negativeSideMarker_Z = null)
            {
                quaternion sec = new quaternion();
                if (Normal_ != null)
                {
                    sec = Normal_.Second;
                }
                else
                {
                    UCS UCS = new UCS(Nodes_.First, Nodes_.Second, Nodes_.Third);

                    quaternion centr = GetCentroid();
                    sec = UCS.ToACS(new quaternion(0, 0, 0, 1)) - Nodes_.First + centr;
                }
                return sec;
            }
            public PRE_BEND GetNormalDirectionA(quaternion negativeSideMarker_Z = null)
            {
                quaternion sec = new quaternion();
                if (Normal_ != null)
                {
                    return Normal_;
                }
                else
                {
                    UCS UCS = new UCS(Nodes_.First, Nodes_.Second, Nodes_.Third);

                    quaternion centr = GetCentroid();
                    sec = UCS.ToACS(new quaternion(0, 0, 0, 1)) - Nodes_.First + centr;
                    return new PRE_BEND(centr, sec);
                }
            }

            public void SetNormalNull()
            {
                Normal_ = null;
            }
            public void SetNormal(quaternion negativeSideMarker_Z = null)
            {
                if (Normal_ != null)
                    Normal_ = null;

                Normal_ = GetNormalDirectionA(negativeSideMarker_Z);
            }
            public void SetNormal(PRE_BEND nor, quaternion negativeSideMarker_Z = null)
            {
                if (Normal_ != null)
                    Normal_ = null;

                Normal_ = new PRE_BEND(nor.First, nor.Second);
            }
            public void ReverseNormal()
            {
                if (Normal_ != null)
                {
                    if (Normal_.Second != null)
                    {
                        try
                        {
                            Normal_.Second = Normal_.First + Normal_.First - Normal_.Second;
                        }
                        catch { }
                    }
                }
            }
            #endregion

            #region UCSs
            public UCS GetUcsByCentroid1()
            {
                UCS ucs = new UCS(Normal_.First, (Nodes.First + Nodes.Second) / 2.0, Nodes.Second);
                if (ucs.FromACS(Normal_.Second).GetZ() < 0.0)
                {
                    ucs = new UCS(Normal_.First, (Nodes.First + Nodes.Second) / 2.0, Nodes.First);
                }

                return ucs;
            }
            public UCS GetUcsByCentroid2()
            {
                UCS ucs = new UCS(Normal_.First, (Nodes.Second + Nodes.Third) / 2.0, Nodes.Third);
                if (ucs.FromACS(Normal_.Second).GetZ() < 0.0)
                {
                    ucs = new UCS(Normal_.First, (Nodes.Second + Nodes.Third) / 2.0, Nodes.Second);
                }

                return ucs;
            }
            public UCS GetUcsByCentroid3()
            {
                UCS ucs = new UCS(Normal_.First, (Nodes.First + Nodes.Third) / 2.0, Nodes.Third);
                if (ucs.FromACS(Normal_.Second).GetZ() < 0.0)
                {
                    ucs = new UCS(Normal_.First, (Nodes.First + Nodes.Third) / 2.0, Nodes.First);
                }

                return ucs;
            }
            public UCS GetUcsByNodes1()
            {
                UCS ucs = new UCS((Nodes.Second + Nodes.First) / 2.0, Nodes.Second, Normal_.First);
                if (ucs.FromACS(Normal_.Second).GetZ() < 0.0)
                {
                    ucs = new UCS((Nodes.Second + Nodes.First) / 2.0, Nodes.First, Normal_.First);
                }

                return ucs;
            }
            public UCS GetUcsByNodes2()
            {
                UCS ucs = new UCS((Nodes.Second + Nodes.Third) / 2.0, Nodes.Third, Normal_.First);
                if (ucs.FromACS(Normal_.Second).GetZ() < 0.0)
                {
                    ucs = new UCS((Nodes.Second + Nodes.Third) / 2.0, Nodes.Second, Normal_.First);
                }

                return ucs;
            }
            public UCS GetUcsByNodes3()
            {
                UCS ucs = new UCS((Nodes.First + Nodes.Third) / 2.0, Nodes.Third, Normal_.First);
                if (ucs.FromACS(Normal_.Second).GetZ() < 0.0)
                {
                    ucs = new UCS((Nodes.First + Nodes.Third) / 2.0, Nodes.First, Normal_.First);
                }

                return ucs;
            }
            public UCS GetUcsByBends1(ref List<Bend> bends)
            {
                Bend bend = bends[GetFirstBendNumer()];
                UCS ucs = new UCS((bend.Start + bend.End) / 2.0, bend.End, Normal_.First);
                if (ucs.FromACS(Normal_.Second).GetZ() < 0.0)
                {
                    ucs = new UCS((bend.Start + bend.End) / 2.0, bend.Start, Normal_.First);
                }
                return ucs;
            }
            public UCS GetUcsByBends2(ref List<Bend> bends)
            {
                Bend bend = bends[GetSecondBendNumer()];
                UCS ucs = new UCS((bend.Start + bend.End) / 2.0, bend.End, Normal_.First);
                if (ucs.FromACS(Normal_.Second).GetZ() < 0.0)
                {
                    ucs = new UCS((bend.Start + bend.End) / 2.0, bend.Start, Normal_.First);
                }
                return ucs;
            }
            public UCS GetUcsByBends3(ref List<Bend> bends)
            {
                Bend bend = bends[GetThirdBendNumer()];
                UCS ucs = new UCS((bend.Start + bend.End) / 2.0, bend.End, Normal_.First);
                if (ucs.FromACS(Normal_.Second).GetZ() < 0.0)
                {
                    ucs = new UCS((bend.Start + bend.End) / 2.0, bend.Start, Normal_.First);
                }
                return ucs;
            }
            public UCS GetUcsByBends(int N, ref List<Bend> bends)
            {
                if (N == Bends_[0].First)
                    return GetUcsByBends1(ref bends);
                else
                    if (N == Bends_[1].First)
                        return GetUcsByBends2(ref bends);
                    else
                        if (N == Bends_[2].First)
                            return GetUcsByBends3(ref bends);
                        else
                            return null;
            }
            public plane GetPlane()
            {
                return new plane(Nodes_.First, Nodes_.Second, Nodes_.Third);
            }
            #endregion

            #region operators
            public static bool operator ==(Triangle t1, Triangle t2)
            {
                return ((t1.Nodes_.Second - t1.Nodes_.First).abs() >= UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes()) &&
                        (UtilityClasses.GlobalFunctions.IsEqualTriangle(t1.Nodes_, t2.Nodes_));
            }
            public static bool operator !=(Triangle t1, Triangle t2)
            {
                return (!(t1 == t2));
            }

            public static bool operator ==(Triangle t1, PRE_TRIANGLE t2)
            {
                return ((t1.Nodes_.Second - t1.Nodes_.First).abs() >= UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes()) &&
                        (UtilityClasses.GlobalFunctions.IsEqualTriangle(t1.Nodes_, t2));
            }
            public static bool operator !=(Triangle t1, PRE_TRIANGLE t2)
            {
                return (!(t1 == t2));
            }

            public static bool operator ==(PRE_TRIANGLE t2, Triangle t1)
            {
                return ((t1.Nodes_.Second - t1.Nodes_.First).abs() >= UtilityClasses.ConstantsAndSettings.GetMinDistBhetwenNodes()) &&
                        (UtilityClasses.GlobalFunctions.IsEqualTriangle(t1.Nodes_, t2));
            }
            public static bool operator !=(PRE_TRIANGLE t2, Triangle t1)
            {
                return (!(t1 == t2));
            }

            public static bool operator ==(Triangle t1, quaternion q)
            {
                PRE_TRIANGLE pt1 = new PRE_TRIANGLE(t1.Nodes.First, t1.Nodes.Second, q);
                PRE_TRIANGLE pt2 = new PRE_TRIANGLE(t1.Nodes.First, t1.Nodes.Third, q);
                PRE_TRIANGLE pt3 = new PRE_TRIANGLE(t1.Nodes.Second, t1.Nodes.Third, q);
                double Area = Math.Abs(t1.GetArea());
                double area = Math.Abs(UtilityClasses.GlobalFunctions.GetArea(pt1));
                area += Math.Abs(UtilityClasses.GlobalFunctions.GetArea(pt2));
                area += Math.Abs(UtilityClasses.GlobalFunctions.GetArea(pt3));

                double d = Area - area;
                d += 0.00001;// MathLibKCAD.Constants.zero_area_difference;

                return d >= 0.0;
            }
            public static bool operator !=(Triangle t1, quaternion q)
            {
                return (!(t1 == q));
            }

            public static bool operator ==(quaternion q, Triangle t1)
            {
                PRE_TRIANGLE pt1 = new PRE_TRIANGLE(t1.Nodes.First, t1.Nodes.Second, q);
                PRE_TRIANGLE pt2 = new PRE_TRIANGLE(t1.Nodes.First, t1.Nodes.Third, q);
                PRE_TRIANGLE pt3 = new PRE_TRIANGLE(t1.Nodes.Second, t1.Nodes.Third, q);
                double Area = Math.Abs(t1.GetArea());
                double area = Math.Abs(UtilityClasses.GlobalFunctions.GetArea(pt1));
                area += Math.Abs(UtilityClasses.GlobalFunctions.GetArea(pt2));
                area += Math.Abs(UtilityClasses.GlobalFunctions.GetArea(pt3));

                double d = Area - area;
                d += 0.00001;// MathLibKCAD.Constants.zero_area_difference;
                return d >= 0.0;
            }
            public static bool operator !=(quaternion q, Triangle t1)
            {
                return (!(t1 == q));
            }

            #endregion

            public void ContiguousByFictivebend(ref TRIANGLES_ARRAY arr, ref  List<int> rezTrNumbers)
            {
                #region FirstBend
                if (Bends_[0].Second)
                {
                    foreach (Triangle tr in arr)
                    {
                        if ((tr.Numer_ != Numer_) &&
                            ((tr.Bends_[0].First == Bends_[0].First) || (tr.Bends_[1].First == Bends_[0].First) || (tr.Bends_[2].First == Bends_[0].First)))
                        {
                            bool exist = false;
                            foreach (int i in rezTrNumbers)
                            {
                                if (i == tr.Numer_)
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist)
                            {
                                rezTrNumbers.Add(tr.Numer_);
                                tr.ContiguousByFictivebend(ref arr, ref rezTrNumbers);
                            }
                        }
                    }
                }
                #endregion
                #region SecondBend
                if (Bends_[1].Second)
                {
                    foreach (Triangle tr in arr)
                    {
                        if ((tr.Numer_ != Numer_) &&
                            ((tr.Bends_[0].First == Bends_[1].First) || (tr.Bends_[1].First == Bends_[1].First) || (tr.Bends_[2].First == Bends_[1].First)))
                        {
                            bool exist = false;
                            foreach (int i in rezTrNumbers)
                            {
                                if (i == tr.Numer_)
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist)
                            {
                                rezTrNumbers.Add(tr.Numer_);
                                tr.ContiguousByFictivebend(ref arr, ref rezTrNumbers);
                            }
                        }
                    }
                }
                #endregion
                #region ThirdBend
                if (Bends_[2].Second)
                {
                    foreach (Triangle tr in arr)
                    {
                        if ((tr.Numer_ != Numer_) &&
                            ((tr.Bends_[0].First == Bends_[2].First) || (tr.Bends_[1].First == Bends_[2].First) || (tr.Bends_[2].First == Bends_[2].First)))
                        {
                            bool exist = false;
                            foreach (int i in rezTrNumbers)
                            {
                                if (i == tr.Numer_)
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist)
                            {
                                rezTrNumbers.Add(tr.Numer_);
                                tr.ContiguousByFictivebend(ref arr, ref rezTrNumbers);
                            }
                        }
                    }
                }
                #endregion
            }
            public static bool IsContiguous(Triangle tr1, Triangle tr2, ref int bendNumer)
            {
                int rez = -1;

                bool b1 = (tr1.Bends_[0].First == tr2.Bends_[0].First) || (tr1.Bends_[0].First == tr2.Bends_[1].First) || (tr1.Bends_[0].First == tr2.Bends_[2].First);
                bool b2 = (tr1.Bends_[1].First == tr2.Bends_[0].First) || (tr1.Bends_[1].First == tr2.Bends_[1].First) || (tr1.Bends_[1].First == tr2.Bends_[2].First);
                bool b3 = (tr1.Bends_[2].First == tr2.Bends_[0].First) || (tr1.Bends_[2].First == tr2.Bends_[1].First) || (tr1.Bends_[2].First == tr2.Bends_[2].First);

                if (b1) { rez = tr1.Bends_[0].First; }
                if (b2) { rez = tr1.Bends_[1].First; }
                if (b3) { rez = tr1.Bends_[2].First; }

                bendNumer = rez;

                return b1 || b2 || b3; ;
            }

            public bool contain_Bends_with_numbers(int nu1, int nu2)
            {
                bool b1 = nu1 == Bends_[0].First || nu1 == Bends_[1].First || nu1 == Bends_[2].First;
                bool b2 = nu2 == Bends_[0].First || nu2 == Bends_[1].First || nu2 == Bends_[2].First;
                if (b1 && b2)
                    return true;
                else
                    return false;
            }

            #region system
            public override bool Equals(Object obj)
            {
                return ((obj is Triangle) && (this == (Triangle)obj));
            }
            public override int GetHashCode()
            {
                //return this.ToString().GetHashCode();
                return (Numer_.GetHashCode() ^ Bends_[0].GetHashCode() ^
                    Bends_[1].GetHashCode() ^ Bends_[2].GetHashCode() ^
                    NodesNumers_[0].GetHashCode() ^ NodesNumers_[1].GetHashCode() ^
                    NodesNumers_[2].GetHashCode());
            }
            public override string ToString()
            {
                string rez = string.Format("Triangle\n----------\n\nkey: {0}\nNumer: {1}\n\n", Key, Numer_);
                string cen = string.Format("Centroid: {0} , {1} , {2}\n\n", Normal_.First.GetX(), Normal_.First.GetY(), Normal_.First.GetZ());
                string nor = string.Format("Normal: {0} , {1} , {2}\n\n", Normal_.Second.GetX(), Normal_.Second.GetY(), Normal_.Second.GetZ());
                string pt1 = string.Format("Coordinates: \n{0} , {1} , {2}\n", Nodes_.First.GetX(), Nodes_.First.GetY(), Nodes_.First.GetZ());
                string pt2 = string.Format("{0} , {1} , {2}\n", Nodes_.Second.GetX(), Nodes_.Second.GetY(), Nodes_.Second.GetZ());
                string pt3 = string.Format("{0} , {1} , {2}\n\n", Nodes_.Third.GetX(), Nodes_.Third.GetY(), Nodes_.Third.GetZ());
                string nn1 = string.Format("Node Numers:\n{0} , {1} , {2}\n\n", NodesNumers_[0], NodesNumers_[1], NodesNumers_[2]);
                string bn = string.Format("Bends Numers:\n{0} / {1} , {2} / {3}, {4} / {5}\n", Bends_[0].First, (Bends_[0].Second) ? "Fictive" : "No Fictive",
                    Bends_[1].First, (Bends_[1].Second) ? "Fictive" : "No Fictive",
                    Bends_[2].First, (Bends_[2].Second) ? "Fictive" : "No Fictive");

                rez += (cen + nor + pt1 + pt2 + pt3 + nn1 + bn);

                return rez;
            }
            #endregion

            //----
            public UtilityClasses.Pair<Polyline, Point3dCollection> GetPickCuter(ref UtilityClasses.Containers container, short s = 0)
            {
                Polyline pl = null;

                UtilityClasses.Pair<Polyline, Point3dCollection> rez = new UtilityClasses.Pair<Polyline, Point3dCollection>(null, null);

                Bend bend1 = container.Bends[GetFirstBendNumer()];
                Bend bend2 = container.Bends[GetSecondBendNumer()];

                if (s == 1)
                {
                    bend1 = container.Bends[GetFirstBendNumer()];
                    bend2 = container.Bends[GetThirdBendNumer()];
                }
                else
                    if (s == 2)
                    {
                        bend1 = container.Bends[GetSecondBendNumer()];
                        bend2 = container.Bends[GetThirdBendNumer()];
                    }


                //common node
                quaternion Q = ((bend1.StartNodeNumer == bend2.StartNodeNumer) || (bend1.StartNodeNumer == bend2.EndNodeNumer)) ?
                    bend1.Start : bend1.End;

                UCS ucs = new UCS(bend1.MidPoint, bend2.MidPoint, Q);
                if ((ucs.FromACS(Normal_.Second)).GetZ() < 0.0)
                    ucs = new UCS(bend2.MidPoint, bend1.MidPoint, Q);

                double Y = (ucs.FromACS(Q)).GetY();

                Point3dCollection list = new Point3dCollection();
                quaternion Q1 = new quaternion(0, 0, Y, -1000);
                list.Add((Point3d)ucs.ToACS(Q1));

                quaternion Q2 = new quaternion(0, 10000, Y, -1000);
                list.Add((Point3d)ucs.ToACS(Q2));

                quaternion Q3 = new quaternion(0, 10000, Y, 1000);
                list.Add((Point3d)ucs.ToACS(Q3));

                quaternion Q4 = new quaternion(0, -10000, Y, 1000);
                list.Add((Point3d)ucs.ToACS(Q4));

                quaternion Q5 = new quaternion(0, -10000, Y, -1000);
                list.Add((Point3d)ucs.ToACS(Q5));

                quaternion Q6 = new quaternion(0, 0, Y, -1000);
                list.Add((Point3d)ucs.ToACS(Q6));

                pl = UtilityClasses.GlobalFunctions.GetPoly(list);

                rez = new UtilityClasses.Pair<Polyline, Point3dCollection>(pl, list);

                return rez;
            }
            //------------
            public UtilityClasses.Pair<Polyline, PRE_TRIANGLE>
                GetOffsetContourAs3DPolyLine_ByBendNormals(ref UtilityClasses.Containers container, bool byvalue = false, double offset = 0.0)
            {
                UtilityClasses.Pair<Polyline, PRE_TRIANGLE> rez =
                   new UtilityClasses.Pair<Polyline, PRE_TRIANGLE>(null, new PRE_TRIANGLE(null, null, null));

                try
                {
                    Bend bend1 = container.Bends[Bends_[0].First];
                    Bend bend2 = container.Bends[Bends_[1].First];
                    Bend bend3 = container.Bends[Bends_[2].First];

                    double offset1 = bend1.IsFictive() ? 0.0 : ((bend1.FirstTriangleNumer == Numer_) ? bend1.FirstTriangleOffset : bend1.SecondTriangleOffset);
                    double offset2 = bend2.IsFictive() ? 0.0 : ((bend2.FirstTriangleNumer == Numer_) ? bend2.FirstTriangleOffset : bend2.SecondTriangleOffset);
                    double offset3 = bend3.IsFictive() ? 0.0 : ((bend3.FirstTriangleNumer == Numer_) ? bend3.FirstTriangleOffset : bend3.SecondTriangleOffset);

                    if (byvalue)
                    {
                        offset1 = offset;
                        offset2 = offset;
                        offset3 = offset;
                    }

                    UCS ucsBend1 = new UCS(bend1.MidPoint, bend1.End, bend1.Normal);
                    UCS ucsBend2 = new UCS(bend2.MidPoint, bend2.End, bend2.Normal);
                    UCS ucsBend3 = new UCS(bend3.MidPoint, bend3.End, bend3.Normal);

                    double k1 = (ucsBend1.FromACS(Normal_.First).GetZ() >= 0.0) ? 1.0 : -1.0;
                    double k2 = (ucsBend2.FromACS(Normal_.First).GetZ() >= 0.0) ? 1.0 : -1.0;
                    double k3 = (ucsBend3.FromACS(Normal_.First).GetZ() >= 0.0) ? 1.0 : -1.0;

                    quaternion q11 = new quaternion(ucsBend1.ToACS(new quaternion(0, 0, 0, offset1 * k1)));
                    quaternion q12 = new quaternion(ucsBend1.ToACS(new quaternion(0, 1, 0, offset1 * k1)));
                    quaternion q13 = new quaternion(ucsBend1.ToACS(new quaternion(0, 0, 1, offset1 * k1)));

                    quaternion q21 = new quaternion(ucsBend2.ToACS(new quaternion(0, 0, 0, offset2 * k2)));
                    quaternion q22 = new quaternion(ucsBend2.ToACS(new quaternion(0, 1, 0, offset2 * k2)));
                    quaternion q23 = new quaternion(ucsBend2.ToACS(new quaternion(0, 0, 1, offset2 * k2)));


                    quaternion q31 = new quaternion(ucsBend3.ToACS(new quaternion(0, 0, 0, offset3 * k3)));
                    quaternion q32 = new quaternion(ucsBend3.ToACS(new quaternion(0, 1, 0, offset3 * k3)));
                    quaternion q33 = new quaternion(ucsBend3.ToACS(new quaternion(0, 0, 1, offset3 * k3)));

                    plane plB1 = new plane(q11, q12, q13);
                    plane plB2 = new plane(q21, q22, q23);
                    plane plB3 = new plane(q31, q32, q33);

                    plane thisPlane = new plane(Nodes_.First, Nodes_.Second, Nodes_.Third);

                    quaternion[] l1 = plB1.IntersectWithPlane(plB2);
                    quaternion[] l2 = plB1.IntersectWithPlane(plB3);
                    quaternion[] l3 = plB2.IntersectWithPlane(plB3);

                    quaternion f = thisPlane.IntersectWithVector(l1[0], l1[1]);
                    quaternion s = thisPlane.IntersectWithVector(l2[0], l2[1]);
                    quaternion t = thisPlane.IntersectWithVector(l3[0], l3[1]);


                    Point3dCollection list = new Point3dCollection();
                    list.Add((Point3d)f);
                    list.Add((Point3d)s);
                    list.Add((Point3d)t);
                    rez = new UtilityClasses.Pair<Polyline, PRE_TRIANGLE>(UtilityClasses.GlobalFunctions.GetPoly(list), new PRE_TRIANGLE(f, s, t));

                }
                catch { }

                return rez;
            }
            public UtilityClasses.Pair<Polyline, PRE_TRIANGLE>
                GetOffsetContourAs3DPolyLine_ByTriangleNormal(ref UtilityClasses.Containers container, bool byvalue = false, double offset = 0.0)
            {
                UtilityClasses.Pair<Polyline, PRE_TRIANGLE> rez =
                    new UtilityClasses.Pair<Polyline, PRE_TRIANGLE>(null, new PRE_TRIANGLE(null, null, null));

                try
                {
                    Bend bend1 = container.Bends[Bends_[0].First];
                    Bend bend2 = container.Bends[Bends_[1].First];
                    Bend bend3 = container.Bends[Bends_[2].First];

                    double offset1 = bend1.IsFictive() ? 0.0 : ((bend1.FirstTriangleNumer == Numer_) ? bend1.FirstTriangleOffset : bend1.SecondTriangleOffset);
                    double offset2 = bend2.IsFictive() ? 0.0 : ((bend2.FirstTriangleNumer == Numer_) ? bend2.FirstTriangleOffset : bend2.SecondTriangleOffset);
                    double offset3 = bend3.IsFictive() ? 0.0 : ((bend3.FirstTriangleNumer == Numer_) ? bend3.FirstTriangleOffset : bend3.SecondTriangleOffset);

                    if (byvalue)
                    {
                        offset1 = offset;
                        offset2 = offset;
                        offset3 = offset;
                    }

                    UCS ucsBend1 = new UCS(bend1.Start, bend1.End, Normal_.First);
                    if ((ucsBend1.FromACS(Normal_.Second)).GetZ() < 0.0)
                        ucsBend1 = new UCS(bend1.End, bend1.Start, Normal_.First);

                    UCS ucsBend2 = new UCS(bend2.Start, bend2.End, Normal_.First);
                    if ((ucsBend2.FromACS(Normal_.Second)).GetZ() < 0.0)
                        ucsBend2 = new UCS(bend2.End, bend2.Start, Normal_.First);

                    UCS ucsBend3 = new UCS(bend3.Start, bend3.End, Normal_.First);
                    if ((ucsBend3.FromACS(Normal_.Second)).GetZ() < 0.0)
                        ucsBend3 = new UCS(bend3.End, bend3.Start, Normal_.First);

                    quaternion q11 = new quaternion(0, 0, offset1, 0);
                    quaternion q12 = new quaternion(0, 100, offset1, 0);

                    quaternion q21 = ucsBend2.ToACS(new quaternion(0, 0, offset2, 0));
                    quaternion q22 = ucsBend2.ToACS(new quaternion(0, 100, offset2, 0));

                    quaternion q31 = ucsBend3.ToACS(new quaternion(0, 0, offset3, 0));
                    quaternion q32 = ucsBend3.ToACS(new quaternion(0, 100, offset3, 0));

                    q21 = ucsBend1.FromACS(q21); q22 = ucsBend1.FromACS(q22);
                    q31 = ucsBend1.FromACS(q31); q32 = ucsBend1.FromACS(q32);

                    q21 = new quaternion(0, q21.GetX(), q21.GetY(), 0.0);
                    q22 = new quaternion(0, q22.GetX(), q22.GetY(), 0.0);

                    q31 = new quaternion(0, q31.GetX(), q31.GetY(), 0.0);
                    q32 = new quaternion(0, q32.GetX(), q32.GetY(), 0.0);

                    line2d line1 = new line2d(new complex(q11.GetX(), q11.GetY()), new complex(q12.GetX(), q12.GetY()));
                    line2d line2 = new line2d(new complex(q21.GetX(), q21.GetY()), new complex(q22.GetX(), q22.GetY()));
                    line2d line3 = new line2d(new complex(q31.GetX(), q31.GetY()), new complex(q32.GetX(), q32.GetY()));

                    complex c1 = line1.IntersectWitch(line2);
                    complex c2 = line1.IntersectWitch(line3);
                    complex c3 = line2.IntersectWitch(line3);

                    quaternion f = ucsBend1.ToACS(new quaternion(0, c1.real(), c1.imag(), 0.0));
                    quaternion s = ucsBend1.ToACS(new quaternion(0, c2.real(), c2.imag(), 0.0));
                    quaternion t = ucsBend1.ToACS(new quaternion(0, c3.real(), c3.imag(), 0.0));

                    Point3dCollection list = new Point3dCollection();
                    list.Add((Point3d)f);
                    list.Add((Point3d)s);
                    list.Add((Point3d)t);

                    rez = new UtilityClasses.Pair<Polyline, PRE_TRIANGLE>(UtilityClasses.GlobalFunctions.GetPoly(list), new PRE_TRIANGLE(f, s, t));

                }
                catch { }

                return rez;
            }
            public UtilityClasses.Pair<Polyline, PRE_TRIANGLE> GetOffsetContourAs3DPolyLine(short type, ref UtilityClasses.Containers container, bool byvalue = false, double offset = 0.0)
            {
                return (type == 0) ? GetOffsetContourAs3DPolyLine_ByBendNormals(ref container, byvalue, offset) :
                       GetOffsetContourAs3DPolyLine_ByTriangleNormal(ref container, byvalue, offset);
            }
        }
        public class Polygon
        {
            private int Numer;
            private string Key;

            private List<int> triangles_Numers;
            private List<int> bends_Numers;

            public int GetNumer() { return Numer; }
            public void SetNumer(int n) { Numer = n; }
            public string GetKey() { return Key; }
            public void SetKey(string key) { Key = key; }

            public List<int> Triangles_Numers_Array
            {
                get { return (triangles_Numers); }
                set { triangles_Numers = value; }
            }
            public List<int> Bends_Numers_Array
            {
                get { return (bends_Numers); }
                set { bends_Numers = value; }
            }

            public Polygon()
            {
                triangles_Numers = null;
                bends_Numers = null;
                Numer = -1;
                Key = Guid.NewGuid().ToString();
            }
            public Polygon(ref Node n, bool bNewKey = false)
            {
                if (bNewKey)
                    Key = Guid.NewGuid().ToString();
                else
                    Key = n.Key;
                Numer = n.Numer;
                bends_Numers = new List<int>();
                triangles_Numers = new List<int>();
                foreach (int i in n.Bends_Numers_Array)
                {
                    bends_Numers.Add(i);
                }

                foreach (int i in n.Triangles_Numers_Array)
                {
                    triangles_Numers.Add(i);
                }
            }
            public Polygon(Triangle tr, ref TRIANGLES_ARRAY Triangles, ref POLYGONS_ARRAY Polygons)
            {
                if (!UtilityClasses.GlobalFunctions.IsFound(tr.GetNumer(), Polygons))
                {
                    triangles_Numers = new List<int>();
                    tr.ContiguousByFictivebend(ref Triangles, ref  triangles_Numers);
                    if (triangles_Numers.Count > 0)
                    {
                        bends_Numers = new List<int>();
                        foreach (int i in triangles_Numers)
                        {
                            Triangle t = Triangles[i];

                            if (!t.IsFirstBendFictive()) { bends_Numers.Add(t.GetFirstBendNumer()); }
                            if (!t.IsSecondBendFictive()) { bends_Numers.Add(t.GetSecondBendNumer()); }
                            if (!t.IsThirdBendFictive()) { bends_Numers.Add(t.GetThirdBendNumer()); }
                        }
                    }
                    else
                    {
                        triangles_Numers = null;
                        bends_Numers = null;
                    }
                    Numer = -1;
                    Key = Guid.NewGuid().ToString();
                }
            }
            public Polygon(Xrecord xrec)
            {
                TypedValue[] res = xrec.Data.AsArray();
                bends_Numers = new List<int>();
                triangles_Numers = new List<int>();

                Numer = (int)res[0].Value;
                Key = (string)res[1].Value;

                int bends_count = (int)res[2].Value;

                int bc = (int)res[2].Value;
                int ii = 3;
                if (bc > 0)
                {
                    for (int i = 0; i < bc; i++, ii++)
                    {
                        int v = (int)res[ii].Value;
                        bends_Numers.Add(v);
                    }
                }
                int bcc = (int)res[ii].Value;
                if (bcc > 0)
                {
                    for (int i = 0; i < bcc; i++, ii++)
                    {
                        int v = (int)res[ii + 1].Value;
                        triangles_Numers.Add(v);
                    }
                }

            }
            public Polygon(StreamReader sr)
            {
                bends_Numers = new List<int>();
                triangles_Numers = new List<int>();

                List<string> list = new List<string>();
                for (int i = 0; i < 4; i++)
                    list.Add(sr.ReadLine());

                bends_Numers = new List<int>();
                triangles_Numers = new List<int>();

                Numer = int.Parse(list[0]);
                Key = list[1];

                int bends_count = int.Parse(list[2]);
                for (int i = 0; i < bends_count; i++)
                    list.Add(sr.ReadLine());

                int bc = bends_count;
                int ii = 3;

                if (bc > 0)
                {
                    for (int i = 0; i < bc; i++, ii++)
                    {
                        int v = int.Parse(list[ii]);
                        bends_Numers.Add(v);
                    }
                }

                int bcc = int.Parse(list[ii]);
                for (int i = 0; i < bcc; i++)
                    list.Add(sr.ReadLine());

                if (bcc > 0)
                {
                    for (int i = 0; i < bcc; i++, ii++)
                    {
                        int v = int.Parse(list[ii + 1]);
                        triangles_Numers.Add(v);
                    }
                }

            }

            public List<int> GetNodesNumersArray(ref BENDS_ARRAY bends)
            {
                List<int> rez = new List<int>();
                foreach (int bendN in Bends_Numers_Array)
                {
                    int startNodeNumer = bends[bendN].StartNodeNumer;
                    int endNodeNumer = bends[bendN].EndNodeNumer;
                    if (rez.IndexOf(startNodeNumer) < 0) { rez.Add(startNodeNumer); }
                    if (rez.IndexOf(endNodeNumer) < 0) { rez.Add(endNodeNumer); }
                }
                return rez;
            }
            public bool IsPlanar(ref NODES_ARRAY nodes, ref BENDS_ARRAY bends, List<int> nodesNumers = null)
            {
                bool planar = true;
                if (nodesNumers == null)
                {
                    nodesNumers = GetNodesNumersArray(ref bends);
                }
                quaternion pos1 = nodes[nodesNumers[0]].Position;
                quaternion pos2 = nodes[nodesNumers[1]].Position;
                quaternion pos3 = nodes[nodesNumers[2]].Position;

                UCS checkUCS = new UCS(pos1, pos2, pos3);
                for (int k = 3; k < nodesNumers.Count; k++)
                {
                    if (Math.Abs(checkUCS.FromACS(nodes[nodesNumers[k]].Position).GetZ()) > Constants.zero_dist)
                    {
                        planar = false;
                        break;
                    }
                }

                return planar;
            }
            public bool IsPlanar(ref TRIANGLES_ARRAY triangles)
            {
                bool planar = true;
                Triangle tr = triangles[Triangles_Numers_Array[0]];
                UCS checkUCS = tr.GetUcsByCentroid1();

                for (int k = 1; k < Triangles_Numers_Array.Count; k++)
                {
                    Triangle TR = triangles[Triangles_Numers_Array[k]];
                    if (Math.Abs(checkUCS.FromACS(TR.Normal.First).GetZ()) > Constants.zero_dist)
                    {
                        planar = false;
                        break;
                    }
                }

                return planar;
            }
            public List<int> GetTrianglesInNodeNumersArray(ref NODES_ARRAY nodes, int nodeNumer)
            {
                List<int> rez = new List<int>();
                foreach (int triangleNmuer in nodes[nodeNumer].Triangles_Numers_Array)
                {
                    if (triangles_Numers.IndexOf(triangleNmuer) >= 0)
                        rez.Add(triangleNmuer);
                }
                return rez;
            }
            public List<quaternion> GetOffsetIntersections(ref UtilityClasses.Containers container)
            {
                List<Bend> sortedBnends = new List<Bend>();
                List<PRE_BEND> offsetBends = new List<PRE_BEND>();
                List<plane> perefierialPlanes = new List<plane>();
                List<Solid3d> perefierialSolids = new List<Solid3d>();

                bool planar = IsPlanar(ref container.Triangles);

                #region podregdam prytite
                foreach (int Num in Bends_Numers_Array)
                {
                    if (container.Bends[Num].Fictive == false)
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
                    Triangle TR = container.Triangles[bend.FirstTriangleNumer];
                    double offset = bend.FirstTriangleOffset;
                    if (TR.PolygonNumer != GetNumer())
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
                #endregion

                #region planar
                if (planar)
                {
                    Triangle TRi = container.Triangles[Triangles_Numers_Array[0]];
                    UCS ucs = TRi.GetUcsByCentroid1();

                    offsetBends.Add(offsetBends[0]);
                    offsetBends.Add(offsetBends[1]);

                    List<quaternion> bL1 = new List<quaternion>();

                    for (int i = 0; i < offsetBends.Count - 1; i++)
                    {
                        if (((offsetBends[i].Second - offsetBends[i].First) / (offsetBends[i + 1].Second - offsetBends[i + 1].First)).absV() > Constants.zero_dist)
                        {
                            line2d line1 = new line2d(ucs.FromACS(offsetBends[i].First), ucs.FromACS(offsetBends[i].Second));
                            line2d line2 = new line2d(ucs.FromACS(offsetBends[i + 1].First), ucs.FromACS(offsetBends[i + 1].Second));
                            complex c = line1.IntersectWitch(line2);

                            quaternion Q1 = ucs.ToACS(new quaternion(0, c.real(), c.imag(), 0));
                            bL1.Add(Q1);

                        }
                    }

                    return bL1;
                }
                #endregion

                #region podregdam rawninite
                foreach (Bend bend in sortedBnends)
                {
                    Triangle TR = container.Triangles[bend.FirstTriangleNumer];
                    double offset = bend.FirstTriangleOffset;
                    if (TR.PolygonNumer != GetNumer())
                    {
                        TR = container.Triangles[bend.SecondTriangleNumer];
                        offset = bend.SecondTriangleOffset;
                    }
                    UCS ucs = TR.GetUcsByBends(bend.Numer, ref container.Bends);
                    plane pl = new plane(ucs.ToACS(new quaternion(0, 0, offset, 0)),
                        ucs.ToACS(new quaternion(0, 1000, offset, 0)),
                        ucs.ToACS(new quaternion(0, 0, offset, 1000)));


                    perefierialPlanes.Add(pl);

                }

                #endregion

                List<quaternion> rez = new List<quaternion>();
                for (int k = 0; k < sortedBnends.Count; k++)
                {
                    Bend bend = sortedBnends[k];
                    int pre = k - 1; if (pre < 0) { pre = sortedBnends.Count - 1; }

                    #region Q points
                    //if(perefierialPlanes[k].IsParallel(perefierialPlanes[pre]))
                    // MessageBox.Show("");
                    quaternion[] perifI1 = perefierialPlanes[k].IntersectWithPlane(perefierialPlanes[pre]);

                    foreach (int trN in triangles_Numers)
                    {
                        Triangle tr = container.Triangles[trN];
                        plane pl = new plane(tr.Normal.First,
                            container.Bends[tr.GetFirstBendNumer()].Start, container.Bends[tr.GetFirstBendNumer()].End);
                        quaternion Q1 = pl.IntersectWithVector(perifI1[0], perifI1[1]);

                        #region check for exist and add

                        if (Q1 == tr)
                        {
                            bool exist = false;
                            foreach (quaternion q in rez)
                            {
                                if ((q - Q1).abs() < Constants.zero_dist)
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist) { rez.Add(Q1); }
                        }
                        #endregion

                    }
                    #endregion

                }

                return rez;
            }
            public List<plane> GetOffsetSideSurfaces(ref UtilityClasses.Containers container)
            {
                List<Bend> sortedBnends = new List<Bend>();
                List<PRE_BEND> offsetBends = new List<PRE_BEND>();
                List<plane> perefierialPlanes = new List<plane>();

                #region podregdam prytite
                foreach (int Num in Bends_Numers_Array)
                {
                    if (container.Bends[Num].Fictive == false)
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
                    Triangle TR = container.Triangles[bend.FirstTriangleNumer];
                    double offset = bend.FirstTriangleOffset;
                    if (TR.PolygonNumer != GetNumer())
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
                #endregion

                #region podregdam rawninite
                foreach (Bend bend in sortedBnends)
                {
                    Triangle TR = container.Triangles[bend.FirstTriangleNumer];
                    double offset = bend.FirstTriangleOffset;
                    if (TR.PolygonNumer != GetNumer())
                    {
                        TR = container.Triangles[bend.SecondTriangleNumer];
                        offset = bend.SecondTriangleOffset;
                    }
                    UCS ucs = TR.GetUcsByBends(bend.Numer, ref container.Bends);
                    plane pl = new plane(ucs.ToACS(new quaternion(0, 0, offset, 0)),
                        ucs.ToACS(new quaternion(0, 1000, offset, 0)),
                        ucs.ToACS(new quaternion(0, 0, offset, 1000)));


                    perefierialPlanes.Add(pl);

                }

                #endregion

                return perefierialPlanes;
            }

            public List<Bend> GetPeripherialBendsChain(ref UtilityClasses.Containers container)
            {
                List<Bend> sortedBnends = new List<Bend>();
                foreach (int Num in Bends_Numers_Array)
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

                return sortedBnends;
            }
            public List<Triangle> GetPeripherialTrianglesChain(ref UtilityClasses.Containers container)
            {
                List<Triangle> rez = new TRIANGLES_ARRAY();
                List<Bend> sortedBends = GetPeripherialBendsChain(ref container);
                foreach (Bend bend in sortedBends)
                {
                    bool b1 = Triangles_Numers_Array.IndexOf(bend.FirstTriangleNumer) >= 0;
                    bool b2 = Triangles_Numers_Array.IndexOf(bend.SecondTriangleNumer) >= 0;

                    if (b1)
                        rez.Add(container.Triangles[bend.FirstTriangleNumer]);
                    else
                        if (b2)
                            rez.Add(container.Triangles[bend.SecondTriangleNumer]);

                }

                return rez;
            }
            //----------------------
            public Xrecord GetXrecord()
            {
                Xrecord xRec = new Xrecord();
                ResultBuffer rB = new ResultBuffer(
                    new TypedValue((int)DxfCode.Int32, Numer),         //0
                    new TypedValue((int)DxfCode.Text, Key),
                    new TypedValue((int)DxfCode.Int32, bends_Numers.Count));
                for (int i = 0; i < bends_Numers.Count; i++)
                {
                    rB.Add(new TypedValue((int)DxfCode.Int32, bends_Numers[i]));
                }

                rB.Add(new TypedValue((int)DxfCode.Int32, triangles_Numers.Count));
                for (int i = 0; i < triangles_Numers.Count; i++)
                {
                    rB.Add(new TypedValue((int)DxfCode.Int32, triangles_Numers[i]));
                }
                xRec.Data = rB;

                return xRec;
            }

            public void ToStream(StreamWriter sw)
            {
                sw.WriteLine(Numer);//0
                sw.WriteLine(Key);//1
                sw.WriteLine(bends_Numers.Count);//2
                for (int i = 0; i < bends_Numers.Count; i++)
                {
                    sw.WriteLine(bends_Numers[i]);
                }
                sw.WriteLine(triangles_Numers.Count);
                for (int i = 0; i < triangles_Numers.Count; i++)
                {
                    sw.WriteLine(triangles_Numers[i]);
                }
            }
        }
    }
}
