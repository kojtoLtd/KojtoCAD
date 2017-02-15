using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using KojtoCAD.KojtoCAD3D.UtilityClasses;
using KojtoCAD.KojtoCAD3D.WorkClasses;

#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
using Teigha.Colors;
using Teigha.BoundaryRepresentation;
using Face = Teigha.DatabaseServices.Face;
using Application = Bricscad.ApplicationServices.Application;
using Window = Bricscad.Windows.Window;
#endif
[assembly: CommandClass(typeof(KojtoCAD.KojtoCAD3D.KojtoGlassDomes))]

namespace KojtoCAD.KojtoCAD3D.UtilityClasses
{
    public class Containers
    {
        public List<Pair<quaternion, quaternion>> ValidPrebends;
        public List<Pair<quaternion, quaternion>> ValidFictivePrebends;

        public List<Pair<quaternion, quaternion>> InValidPrebends;
        public List<Pair<quaternion, quaternion>> InValidFictivePrebends;

        public List<Bend> Bends;
        public List<Node> Nodes;
        public List<Triangle> Triangles;
        public List<Polygon> Polygons;
        public List<int> peripheralBendsNumers;

        public bool error = false;

        public Containers()
        {
            error = false;
            ValidPrebends = null;//new PRE_BEND_ARRAY();
            ValidFictivePrebends = null;// new PRE_BEND_ARRAY();
            InValidPrebends = null;// new PRE_BEND_ARRAY();
            InValidFictivePrebends = null;// new PRE_BEND_ARRAY();
            Bends = new List<Bend>();
            Nodes = new List<Node>();
            Triangles = new List<Triangle>();
            Polygons = new List<Polygon>();
            peripheralBendsNumers = new List<int>();

        }
        public Containers(ref Pair<List<Entity>, List<Entity>> list)
        {
            error = false;

            ValidPrebends = new List<Pair<quaternion, quaternion>>();               //corect bends
            ValidFictivePrebends = new List<Pair<quaternion, quaternion>>();        //corect fictive Bends
            InValidPrebends = new List<Pair<quaternion, quaternion>>();             //Incorrect Bends 
            InValidFictivePrebends = new List<Pair<quaternion, quaternion>>();      //Incorrect fictive Bend

            List<Pair<quaternion, quaternion>> arr = new List<Pair<quaternion, quaternion>>();
            List<Pair<quaternion, quaternion>> arrF = new List<Pair<quaternion, quaternion>>();

            //-------------------
            DateTime dt0 = DateTime.Now;
            string dtString = dt0.ToString() + "   Start";
            //---------------------------------------------------

            if (list.First != null)
                arr = GlobalFunctions.GetPreBends(list.First);//Bends
            if (list.Second != null)
                arrF = GlobalFunctions.GetPreBends(list.Second);//fictive Bends

            PreBendComparer pbc = new PreBendComparer();
            arr.Sort(pbc);
            arrF.Sort(pbc);

            //-------------------
            DateTime dt1 = DateTime.Now;
            dtString += "\n" + dt1.ToString() + "   GetPreBends";
            //---------------------------------------------------

            #region PRE_BEND normal Bends
            foreach (Pair<quaternion, quaternion> PB in arr)//запълваме контейнерите за нефиктивните пръти
            {
                Pair<Pair<int, Pair<quaternion, quaternion>>, quaternion> t = GlobalFunctions.ExistTriangle(PB, ref arr, ref arrF);
                if (t.First.First >= 0)//Коректни нормални (нефиктивни) пръти
                {
                    bool exist = false;
                    foreach (Pair<quaternion, quaternion> PBB in ValidPrebends) //проверяваме дали няма повторение на пръти (дали в ValidPrebends е постваян прът със същите крайни точки)
                    {
                        if (GlobalFunctions.IsEqualPreBend(PB, PBB)) // ако имат еднакви краини точки (са еднакви отсечки )
                        {
                            exist = true;                    //в ValidPrebends е постваян прът със същите крайни точки                               
                            break;                           //прекъсваме проверката
                        }
                    }
                    if (!exist)
                    {
                        foreach (Pair<quaternion, quaternion> PBB in ValidFictivePrebends)
                        {
                            if (GlobalFunctions.IsEqualPreBend(PB, PBB))
                            {
                                exist = true;
                                break;
                            }
                        }
                    }

                    if (!exist)                              //ако вече не е добавян прът със същите крайни точки
                        ValidPrebends.Add(PB);             //добавяме пръта
                    else
                        InValidPrebends.Add(PB); //Некоректни нормални (нефиктивни) пръти

                }
                else
                {
                    InValidPrebends.Add(PB); //Некоректни нормални (нефиктивни) пръти
                }
            }
            #endregion

            #region PRE_BEND fictive Bends
            //correctness of the fictive Bends does not depend on array nefiktivnite rods
            //can be obtained only by the fictitious triangle Bends
            //коректността на фиктивните пръти не зависи от масива с нефиктивните пръти
            //може да се поучи триъгълник само от фиктивни пръти
            foreach (Pair<quaternion, quaternion> PB in arrF)
            {
                Pair<Pair<int, Pair<quaternion, quaternion>>, quaternion> t = GlobalFunctions.ExistTriangle(PB, ref arrF, ref arr);
                if (t.First.First >= 0)//Коректни фиктивни пръти
                {
                    bool exist = false;
                    foreach (Pair<quaternion, quaternion> PBB in ValidFictivePrebends)
                        //check if there is a repeat of Bends (whether ValidFictivePrebends postvayan Bend is the same endpoints)
                        //проверяваме дали няма повторение на пръти (дали в ValidFictivePrebends е постваян прът със същите крайни точки)
                    {
                        if (GlobalFunctions.IsEqualPreBend(PB, PBB))
                            //If the edges have the same points (the same sections)
                            // ако имат еднакви краини точки (са еднакви отсечки )
                        {
                            exist = true;                    //в ValidFictivePrebends е постваян прът със същите крайни точки
                            break;                           //прекъсваме проверката
                        }
                    }
                    if (!exist)
                    {
                        foreach (Pair<quaternion, quaternion> PBB in ValidPrebends)
                        {
                            if (GlobalFunctions.IsEqualPreBend(PB, PBB))
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                    if (!exist)                             //if not already added rod of same endpoints  //ако вече не е добавян прът със същите крайни точки
                        ValidFictivePrebends.Add(PB);        //add Bend //добавяме пръта
                    else
                        InValidFictivePrebends.Add(PB); //Incorrect fictive Bends //Некоректни фиктивни пръти
                }
                else
                {
                    InValidFictivePrebends.Add(PB); //Incorrect fictive Bends //Некоректни фиктивни пръти
                }
            }//
            #endregion

            //-------------------
            DateTime dtt1 = DateTime.Now;
            string dttString = dtt1.ToString() + "    Преобразуване в preBend";
            dtString += "\n" + dttString;
            //---------------------------------------------------               

            #region Primary fill Array of Bends. !  No numbers attached to Nodes and Triangles....
            Bends = new List<Bend>();
            foreach (Pair<quaternion, quaternion> pb in ValidPrebends)
            {
                Bends.Add(new Bend(pb, false));
            }

            foreach (Pair<quaternion, quaternion> pb in ValidFictivePrebends)
            {
                Bends.Add(new Bend(pb, true));
            }
            #endregion

            //-------------------
            DateTime dtt2 = DateTime.Now;
            dttString = dtt2.ToString() + "  Масив с пръти";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region Primary fill Array of Nodes. !  No numbers attached to Bends and Triangles...
            Nodes = new List<Node>();
            foreach (Bend b in Bends)
            {
                bool exist = false;
                foreach (Node n in Nodes)
                {
                    if (b.GetPreBend().First == n)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                    Nodes.Add(new Node(b.GetPreBend().First));

                exist = false;
                foreach (Node n in Nodes)
                {
                    if (b.GetPreBend().Second == n)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                    Nodes.Add(new Node(b.GetPreBend().Second));
            }
            #endregion

            //-------------------
            DateTime dtt3 = DateTime.Now;
            dttString = dtt3.ToString() + "     Масив с възли";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region Fill the Numer of Node and Bend (for faster searching for creating Triangles) the Numer of Nodes is equal to its index in the array Nodes
            //слагамсе номера на възлите и прътите (за по-бързо търсене при образуване на триъгълниците) номера на възела е равен на позицията му в масива Nodes
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].SetNumer(i);
            }
            for (int i = 0; i < Bends.Count; i++)
            {
                Bends[i].SetNumer(i);
            }
            #endregion

            //-------------------
            DateTime dtt4 = DateTime.Now;
            dttString = dtt4.ToString() + "  попълване на номера ";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region Fill the array with the Numer of Bends per Node. Simultaneously complete and StartNode_Number, EndNode_Number for the Bend
            //Попълваме масива с номера на пръти за всеки възел. Едновременно се поълват и StartNode_Number, EndNode_Number за съответния Bend
            foreach (Node n in Nodes)
            {
                n.FillBends_Numbers_Array(ref Bends);
            }

            foreach (Bend b in Bends)
            {
                b.FillStartNode_Number(ref Nodes);
                b.FillEndNode_Number(ref Nodes);
            }
            #endregion

            //-------------------
            DateTime dtt5 = DateTime.Now;
            dttString = dtt5.ToString() + "  попълване на номера ";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region Create Triangles
            Triangles = new List<Triangle>();
            int tr_counter = 0;
            foreach (Node n in Nodes)
            {
                int bc = n.GetBendsCount();
                for (int i = 0; i < bc; i++)
                {
                    Bend b1 = Bends[n.GetBendsAt(i)];
                    for (int j = i + 1; j < bc; j++)
                    {
                        Bend b2 = Bends[n.GetBendsAt(j)];

                        int third = ((b2.GetStartNodeNumer() == b1.GetStartNodeNumer()) || (b2.GetStartNodeNumer() == b1.GetEndNodeNumer())) ? b2.GetEndNodeNumer() : b2.GetStartNodeNumer();
                        int first = ((b1.GetStartNodeNumer() == b2.GetStartNodeNumer()) || (b1.GetStartNodeNumer() == b2.GetEndNodeNumer())) ? b1.GetEndNodeNumer() : b1.GetStartNodeNumer();

                        Pair<quaternion, quaternion> pb1 = new Pair<quaternion, quaternion>(Nodes[first].GetPosition(), Nodes[third].GetPosition());

                        bool exist_bend = false;
                        foreach (Bend bend in Bends)
                        {
                            if (bend == pb1)
                            {
                                exist_bend = true;
                                break;
                            }
                        }

                        if (exist_bend)
                        {
                            Triplet<quaternion, quaternion, quaternion> ptr = new Triplet<quaternion, quaternion, quaternion>(b1.GetStart(), b1.GetEnd(), Nodes[third].GetPosition());

                            // double perimeter = (ptr.Third - ptr.Second).abs() + (ptr.Third - ptr.First).abs() + (ptr.First - ptr.Second).abs();

                            int exist_triangle = -1;//проверяваме дали в масива има вече такъв триъгълник                                                              
                            foreach (Triangle tt in Triangles)
                            {
                                if (ptr == tt)
                                {
                                    exist_triangle = 1;
                                    break;
                                }
                            }
                            if (exist_triangle < 0)//не е съдаван такъв триъгълник
                            {
                                quaternion m = b1.GetMid();
                                quaternion q = (Nodes[third].GetPosition() - m) / 3.0 + m;
                                int bn = b1.GetNumer();
                                Bend bendd = Bends[bn];
                                Triangle rt = new Triangle(b1.GetNumer(), third, tr_counter, b1.IsFictive());
                                // rt.SetNormalFirstPoint(q);                               
                                n.AddTriangle(tr_counter);
                                rt.SetPreNodes(ptr);
                                rt.SetNormal(new Pair<quaternion, quaternion>(rt.GetCentroid(), null));
                                Triangles.Add(rt);
                                tr_counter++;
                            }
                        }
                    }//
                }

            }

            #endregion

            //-------------------
            DateTime dtt6 = DateTime.Now;
            dttString = dtt6.ToString() + "  Създаване на триъгълници";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region попълвам данните за триъгълниците
            foreach (Triangle t in Triangles)
            {
                int FirstBendStartNodeNumber = Bends[t.GetFirstBendNumer()].GetStartNodeNumer();
                int FirstBendEndNodeNumber = Bends[t.GetFirstBendNumer()].GetEndNodeNumer();

                Pair<quaternion, quaternion> sb = new Pair<quaternion, quaternion>(t.GetPreNodes().Second, t.GetPreNodes().Third);
                Pair<quaternion, quaternion> tb = new Pair<quaternion, quaternion>(t.GetPreNodes().First, t.GetPreNodes().Third);
                foreach (Bend b in Bends)
                {
                    if (b.GetNumer() != t.GetFirstBendNumer())
                    {
                        int StartNodeNumber = b.GetStartNodeNumer();
                        int EndNodeNumber = b.GetEndNodeNumer();
                        if (b == sb)
                        {
                            t.SetSecondBendNumer(b.GetNumer(), b.IsFictive());
                            if ((FirstBendStartNodeNumber == StartNodeNumber) || (FirstBendStartNodeNumber == EndNodeNumber))
                            {
                                t.SetSecondNodeNumer(FirstBendEndNodeNumber);
                            }
                            else
                            {
                                t.SetSecondNodeNumer(FirstBendStartNodeNumber);
                            }
                        }
                        else
                        {
                            if (b == tb)
                            {
                                t.SetThirdBendNumer(b.GetNumer(), b.IsFictive());
                                if ((FirstBendStartNodeNumber == StartNodeNumber) || (FirstBendStartNodeNumber == EndNodeNumber))
                                {
                                    t.SetThirdNodeNumer(FirstBendEndNodeNumber);
                                }
                                else
                                {
                                    t.SetThirdNodeNumer(FirstBendStartNodeNumber);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            //-------------------
            DateTime dtt7 = DateTime.Now;
            dttString = dtt7.ToString() + "   Попълване на данните за триъгълниците";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region попълвам нонерата на триъгълниците във възлите

            foreach (Node n in Nodes)
            {
                n.ClearTriangles();
                int N = n.GetNumer();
                foreach (Triangle t in Triangles)
                {
                    int[] T = t.GetNodesNumers();
                    if ((T[0] == N) || (T[1] == N) || ((T[2] == N)))
                    {
                        int tN = t.GetNumer();
                        if (n.IndexOfTriangleNumber(tN) < 0)
                            n.AddTriangle(tN);
                    }
                }
            }

            #endregion

            #region номерата на триъгълниците към прътите
            foreach (Bend b in Bends)
            {
                int ib = b.GetNumer();
                foreach (Triangle t in Triangles)
                {
                    if (ib == t.GetFirstBendNumer() || ib == t.GetSecondBendNumer() || ib == t.GetThirdBendNumer())
                    {
                        if (b.GetFirstTriangleNumer() < 0)
                            b.SetFirstTriangleNumer(t.GetNumer());
                        else
                            b.SetSecondTriangleNumer(t.GetNumer());
                        if (b.GetSecondTriangleNumer() > 0)
                            break;
                    }
                }
            }
            #endregion

            peripheralBendsNumers = new List<int>();
            #region попълваме масива с периферни пръти
            foreach (Bend b in Bends)
            {
                if (b.GetSecondTriangleNumer() < 0)
                    peripheralBendsNumers.Add(b.GetNumer());
            }
            #endregion

            #region check for Bends that are fictitive and peripheral
            List<int> PeripheralAndIsFictiveBendsNumers = new List<int>();
            foreach (int N in peripheralBendsNumers)
            {
                if (Bends[N].IsPeripheralAndIsFictive())
                {
                    PeripheralAndIsFictiveBendsNumers.Add(N);
                }
            }
            if (PeripheralAndIsFictiveBendsNumers.Count > 0)
            {
                if (MessageBox.Show("Show incorrect Bends ? ", "ERROR in fictive and peripherial bends", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Database db = HostApplicationServices.WorkingDatabase;
                    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        foreach (int N in PeripheralAndIsFictiveBendsNumers)
                        {
                            Bend bend = Bends[N];
                            quaternion mid = bend.MidPoint;
                            double length = bend.Length / 10.0;

                            Solid3d acSol3D = new Solid3d();
                            acSol3D.SetDatabaseDefaults();
                            acSol3D.CreateSphere(length);
                            acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(mid.GetX(), mid.GetY(), mid.GetZ()) - Point3d.Origin));

                            acBlkTblRec.AppendEntity(acSol3D);
                            tr.AddNewlyCreatedDBObject(acSol3D, true);

                        }
                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                arr.Clear();
                arrF.Clear();
                Bends.Clear();
                Nodes.Clear();
                Triangles.Clear();
                ValidPrebends.Clear();
                ValidFictivePrebends.Clear();
                InValidPrebends.Clear();
                InValidFictivePrebends.Clear();
                peripheralBendsNumers.Clear();
                PeripheralAndIsFictiveBendsNumers.Clear();
            }
            #endregion

            //-------------------
            DateTime dt2 = DateTime.Now;
            string dtString1 = dt2.ToString();
            //---------------------------------------------------                
            // MessageBox.Show(ValidPrebends.Count.ToString() + "\n\n" + ValidFictivePrebends.Count.ToString() + "\n\n" + InValidPrebends.Count.ToString() + "\n\n" + InValidFictivePrebends.Count.ToString() + "\n\n Triangles:  " + Triangles.Count.ToString() +"\n\n"+ dtString + "\n\n" + dtString1);

            #region извличам полигоните (многоъгълниците)
            Polygons = new List<Polygon>();
            foreach (Triangle ttr in Triangles)
            {
                Polygon p = new Polygon(ttr, ref Triangles, ref Polygons);
                if ((p.Triangles_Numers_Array != null) && (p.Bends_Numers_Array != null))
                {
                    Polygons.Add(p);
                }
            }
            #endregion

            #region Fill Poligon Numers
            for (int i = 0; i < Polygons.Count; i++)
            {
                Polygons[i].SetNumer(i);
                foreach (int N in Polygons[i].Triangles_Numers_Array)
                {
                    Triangles[N].PolygonNumer = i;
                }
            }
            #endregion

            SetTrianglesNormalsSameAsTheSet();

            bool testTrianglesNormals = true;
            foreach (Triangle tr in Triangles)
            {
                if ((object)tr.Normal == null || (object)tr.Normal.First == null || (object)tr.Normal.Second == null)
                {
                    testTrianglesNormals = false;
                    break;
                }
            }
            if (testTrianglesNormals)
            {
                SetBendsNormals();
                SetNodesNormals();
            }
            else
            {
                error = true;
            }

        }
        public Containers(ref List<Pair<quaternion, quaternion>> arr, ref List<Pair<quaternion, quaternion>> arrF)
        {
            error = false;

            ValidPrebends = new List<Pair<quaternion, quaternion>>();               //corect bends
            ValidFictivePrebends = new List<Pair<quaternion, quaternion>>();        //corect fictive Bends
            InValidPrebends = new List<Pair<quaternion, quaternion>>();             //Incorrect Bends 
            InValidFictivePrebends = new List<Pair<quaternion, quaternion>>();      //Incorrect fictive Bend

            //-------------------
            DateTime dt0 = DateTime.Now;
            string dtString = dt0.ToString() + "   Start";
            //---------------------------------------------------

            PreBendComparer pbc = new PreBendComparer();
            arr.Sort(pbc);
            arrF.Sort(pbc);

            //-------------------
            DateTime dt1 = DateTime.Now;
            dtString += "\n" + dt1.ToString() + "   GetPreBends";
            //---------------------------------------------------

            #region PRE_BEND normal Bends
            foreach (Pair<quaternion, quaternion> PB in arr)//запълваме контейнерите за нефиктивните пръти
            {
                Pair<Pair<int, Pair<quaternion, quaternion>>, quaternion> t = GlobalFunctions.ExistTriangle(PB, ref arr, ref arrF);
                if (t.First.First >= 0)//Коректни нормални (нефиктивни) пръти
                {
                    bool exist = false;
                    foreach (Pair<quaternion, quaternion> PBB in ValidPrebends) //проверяваме дали няма повторение на пръти (дали в ValidPrebends е постваян прът със същите крайни точки)
                    {
                        if (GlobalFunctions.IsEqualPreBend(PB, PBB)) // ако имат еднакви краини точки (са еднакви отсечки )
                        {
                            exist = true;                    //в ValidPrebends е постваян прът със същите крайни точки                               
                            break;                           //прекъсваме проверката
                        }
                    }
                    if (!exist)
                    {
                        foreach (Pair<quaternion, quaternion> PBB in ValidFictivePrebends)
                        {
                            if (GlobalFunctions.IsEqualPreBend(PB, PBB))
                            {
                                exist = true;
                                break;
                            }
                        }
                    }

                    if (!exist)                              //ако вече не е добавян прът със същите крайни точки
                        ValidPrebends.Add(PB);             //добавяме пръта
                    else
                        InValidPrebends.Add(PB); //Некоректни нормални (нефиктивни) пръти

                }
                else
                {
                    InValidPrebends.Add(PB); //Некоректни нормални (нефиктивни) пръти
                }
            }
            #endregion

            #region PRE_BEND fictive Bends
            //correctness of the fictive Bends does not depend on array nefiktivnite rods
            //can be obtained only by the fictitious triangle Bends
            //коректността на фиктивните пръти не зависи от масива с нефиктивните пръти
            //може да се поучи триъгълник само от фиктивни пръти
            foreach (Pair<quaternion, quaternion> PB in arrF)
            {
                Pair<Pair<int, Pair<quaternion, quaternion>>, quaternion> t = GlobalFunctions.ExistTriangle(PB, ref arrF, ref arr);
                if (t.First.First >= 0)//Коректни фиктивни пръти
                {
                    bool exist = false;
                    foreach (Pair<quaternion, quaternion> PBB in ValidFictivePrebends)
                        //check if there is a repeat of Bends (whether ValidFictivePrebends postvayan Bend is the same endpoints)
                        //проверяваме дали няма повторение на пръти (дали в ValidFictivePrebends е постваян прът със същите крайни точки)
                    {
                        if (GlobalFunctions.IsEqualPreBend(PB, PBB))
                            //If the edges have the same points (the same sections)
                            // ако имат еднакви краини точки (са еднакви отсечки )
                        {
                            exist = true;                    //в ValidFictivePrebends е постваян прът със същите крайни точки
                            break;                           //прекъсваме проверката
                        }
                    }
                    if (!exist)
                    {
                        foreach (Pair<quaternion, quaternion> PBB in ValidPrebends)
                        {
                            if (GlobalFunctions.IsEqualPreBend(PB, PBB))
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                    if (!exist)                             //if not already added rod of same endpoints  //ако вече не е добавян прът със същите крайни точки
                        ValidFictivePrebends.Add(PB);        //add Bend //добавяме пръта
                    else
                        InValidFictivePrebends.Add(PB); //Incorrect fictive Bends //Некоректни фиктивни пръти
                }
                else
                {
                    InValidFictivePrebends.Add(PB); //Incorrect fictive Bends //Некоректни фиктивни пръти
                }
            }//
            #endregion

            //-------------------
            DateTime dtt1 = DateTime.Now;
            string dttString = dtt1.ToString() + "    Преобразуване в preBend";
            dtString += "\n" + dttString;
            //---------------------------------------------------               

            #region Primary fill Array of Bends. !  No numbers attached to Nodes and Triangles....
            Bends = new List<Bend>();
            foreach (Pair<quaternion, quaternion> pb in ValidPrebends)
            {
                Bends.Add(new Bend(pb, false));
            }

            foreach (Pair<quaternion, quaternion> pb in ValidFictivePrebends)
            {
                Bends.Add(new Bend(pb, true));
            }
            #endregion

            //-------------------
            DateTime dtt2 = DateTime.Now;
            dttString = dtt2.ToString() + "  Масив с пръти";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region Primary fill Array of Nodes. !  No numbers attached to Bends and Triangles...
            Nodes = new List<Node>();
            foreach (Bend b in Bends)
            {
                bool exist = false;
                foreach (Node n in Nodes)
                {
                    if (b.GetPreBend().First == n)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                    Nodes.Add(new Node(b.GetPreBend().First));

                exist = false;
                foreach (Node n in Nodes)
                {
                    if (b.GetPreBend().Second == n)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                    Nodes.Add(new Node(b.GetPreBend().Second));
            }
            #endregion

            //-------------------
            DateTime dtt3 = DateTime.Now;
            dttString = dtt3.ToString() + "     Масив с възли";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region Fill the Numer of Node and Bend (for faster searching for creating Triangles) the Numer of Nodes is equal to its index in the array Nodes
            //слагамсе номера на възлите и прътите (за по-бързо търсене при образуване на триъгълниците) номера на възела е равен на позицията му в масива Nodes
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].SetNumer(i);
            }
            for (int i = 0; i < Bends.Count; i++)
            {
                Bends[i].SetNumer(i);
            }
            #endregion

            //-------------------
            DateTime dtt4 = DateTime.Now;
            dttString = dtt4.ToString() + "  попълване на номера ";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region Fill the array with the Numer of Bends per Node. Simultaneously complete and StartNode_Number, EndNode_Number for the Bend
            //Попълваме масива с номера на пръти за всеки възел. Едновременно се поълват и StartNode_Number, EndNode_Number за съответния Bend
            foreach (Node n in Nodes)
            {
                n.FillBends_Numbers_Array(ref Bends);
            }

            foreach (Bend b in Bends)
            {
                b.FillStartNode_Number(ref Nodes);
                b.FillEndNode_Number(ref Nodes);
            }
            #endregion

            //-------------------
            DateTime dtt5 = DateTime.Now;
            dttString = dtt5.ToString() + "  попълване на номера ";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region Create Triangles
            Triangles = new List<Triangle>();
            int tr_counter = 0;
            foreach (Node n in Nodes)
            {
                int bc = n.GetBendsCount();
                for (int i = 0; i < bc; i++)
                {
                    Bend b1 = Bends[n.GetBendsAt(i)];
                    for (int j = i + 1; j < bc; j++)
                    {
                        Bend b2 = Bends[n.GetBendsAt(j)];

                        int third = ((b2.GetStartNodeNumer() == b1.GetStartNodeNumer()) || (b2.GetStartNodeNumer() == b1.GetEndNodeNumer())) ? b2.GetEndNodeNumer() : b2.GetStartNodeNumer();
                        int first = ((b1.GetStartNodeNumer() == b2.GetStartNodeNumer()) || (b1.GetStartNodeNumer() == b2.GetEndNodeNumer())) ? b1.GetEndNodeNumer() : b1.GetStartNodeNumer();

                        Pair<quaternion, quaternion> pb1 = new Pair<quaternion, quaternion>(Nodes[first].GetPosition(), Nodes[third].GetPosition());

                        bool exist_bend = false;
                        foreach (Bend bend in Bends)
                        {
                            if (bend == pb1)
                            {
                                exist_bend = true;
                                break;
                            }
                        }

                        if (exist_bend)
                        {
                            Triplet<quaternion, quaternion, quaternion> ptr = new Triplet<quaternion, quaternion, quaternion>(b1.GetStart(), b1.GetEnd(), Nodes[third].GetPosition());

                            // double perimeter = (ptr.Third - ptr.Second).abs() + (ptr.Third - ptr.First).abs() + (ptr.First - ptr.Second).abs();

                            int exist_triangle = -1;//проверяваме дали в масива има вече такъв триъгълник                                                              
                            foreach (Triangle tt in Triangles)
                            {
                                if (ptr == tt)
                                {
                                    exist_triangle = 1;
                                    break;
                                }
                            }
                            if (exist_triangle < 0)//не е съдаван такъв триъгълник
                            {
                                quaternion m = b1.GetMid();
                                quaternion q = (Nodes[third].GetPosition() - m) / 3.0 + m;
                                int bn = b1.GetNumer();
                                Bend bendd = Bends[bn];
                                Triangle rt = new Triangle(b1.GetNumer(), third, tr_counter, b1.IsFictive());
                                // rt.SetNormalFirstPoint(q);                               
                                n.AddTriangle(tr_counter);
                                rt.SetPreNodes(ptr);
                                rt.SetNormal(new Pair<quaternion, quaternion>(rt.GetCentroid(), null));
                                Triangles.Add(rt);
                                tr_counter++;
                            }
                        }
                    }//
                }

            }

            #endregion

            //-------------------
            DateTime dtt6 = DateTime.Now;
            dttString = dtt6.ToString() + "  Създаване на триъгълници";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region попълвам данните за триъгълниците
            foreach (Triangle t in Triangles)
            {
                int FirstBendStartNodeNumber = Bends[t.GetFirstBendNumer()].GetStartNodeNumer();
                int FirstBendEndNodeNumber = Bends[t.GetFirstBendNumer()].GetEndNodeNumer();

                Pair<quaternion, quaternion> sb = new Pair<quaternion, quaternion>(t.GetPreNodes().Second, t.GetPreNodes().Third);
                Pair<quaternion, quaternion> tb = new Pair<quaternion, quaternion>(t.GetPreNodes().First, t.GetPreNodes().Third);
                foreach (Bend b in Bends)
                {
                    if (b.GetNumer() != t.GetFirstBendNumer())
                    {
                        int StartNodeNumber = b.GetStartNodeNumer();
                        int EndNodeNumber = b.GetEndNodeNumer();
                        if (b == sb)
                        {
                            t.SetSecondBendNumer(b.GetNumer(), b.IsFictive());
                            if ((FirstBendStartNodeNumber == StartNodeNumber) || (FirstBendStartNodeNumber == EndNodeNumber))
                            {
                                t.SetSecondNodeNumer(FirstBendEndNodeNumber);
                            }
                            else
                            {
                                t.SetSecondNodeNumer(FirstBendStartNodeNumber);
                            }
                        }
                        else
                        {
                            if (b == tb)
                            {
                                t.SetThirdBendNumer(b.GetNumer(), b.IsFictive());
                                if ((FirstBendStartNodeNumber == StartNodeNumber) || (FirstBendStartNodeNumber == EndNodeNumber))
                                {
                                    t.SetThirdNodeNumer(FirstBendEndNodeNumber);
                                }
                                else
                                {
                                    t.SetThirdNodeNumer(FirstBendStartNodeNumber);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            //-------------------
            DateTime dtt7 = DateTime.Now;
            dttString = dtt7.ToString() + "   Попълване на данните за триъгълниците";
            dtString += "\n" + dttString;
            //---------------------------------------------------

            #region попълвам нонерата на триъгълниците във възлите

            foreach (Node n in Nodes)
            {
                n.ClearTriangles();
                int N = n.GetNumer();
                foreach (Triangle t in Triangles)
                {
                    int[] T = t.GetNodesNumers();
                    if ((T[0] == N) || (T[1] == N) || ((T[2] == N)))
                    {
                        int tN = t.GetNumer();
                        if (n.IndexOfTriangleNumber(tN) < 0)
                            n.AddTriangle(tN);
                    }
                }
            }

            #endregion

            #region номерата на триъгълниците към прътите
            foreach (Bend b in Bends)
            {
                int ib = b.GetNumer();
                foreach (Triangle t in Triangles)
                {
                    if (ib == t.GetFirstBendNumer() || ib == t.GetSecondBendNumer() || ib == t.GetThirdBendNumer())
                    {
                        if (b.GetFirstTriangleNumer() < 0)
                            b.SetFirstTriangleNumer(t.GetNumer());
                        else
                            b.SetSecondTriangleNumer(t.GetNumer());
                        if (b.GetSecondTriangleNumer() > 0)
                            break;
                    }
                }
            }
            #endregion

            peripheralBendsNumers = new List<int>();
            #region попълваме масива с периферни пръти
            foreach (Bend b in Bends)
            {
                if (b.GetSecondTriangleNumer() < 0)
                    peripheralBendsNumers.Add(b.GetNumer());
            }
            #endregion

            #region check for Bends that are fictitive and peripheral
            List<int> PeripheralAndIsFictiveBendsNumers = new List<int>();
            foreach (int N in peripheralBendsNumers)
            {
                if (Bends[N].IsPeripheralAndIsFictive())
                {
                    PeripheralAndIsFictiveBendsNumers.Add(N);
                }
            }
            if (PeripheralAndIsFictiveBendsNumers.Count > 0)
            {
                if (MessageBox.Show("Show incorrect Bends ? ", "ERROR in fictive and peripherial bends", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Database db = HostApplicationServices.WorkingDatabase;
                    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        foreach (int N in PeripheralAndIsFictiveBendsNumers)
                        {
                            Bend bend = Bends[N];
                            quaternion mid = bend.MidPoint;
                            double length = bend.Length / 10.0;

                            Solid3d acSol3D = new Solid3d();
                            acSol3D.SetDatabaseDefaults();
                            acSol3D.CreateSphere(length);
                            acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(mid.GetX(), mid.GetY(), mid.GetZ()) - Point3d.Origin));

                            acBlkTblRec.AppendEntity(acSol3D);
                            tr.AddNewlyCreatedDBObject(acSol3D, true);

                        }
                        tr.Commit();
                        ed.UpdateScreen();
                    }
                }
                arr.Clear();
                arrF.Clear();
                Bends.Clear();
                Nodes.Clear();
                Triangles.Clear();
                ValidPrebends.Clear();
                ValidFictivePrebends.Clear();
                InValidPrebends.Clear();
                InValidFictivePrebends.Clear();
                peripheralBendsNumers.Clear();
                PeripheralAndIsFictiveBendsNumers.Clear();
            }
            #endregion

            //-------------------
            DateTime dt2 = DateTime.Now;
            string dtString1 = dt2.ToString();
            //---------------------------------------------------                
            // MessageBox.Show(ValidPrebends.Count.ToString() + "\n\n" + ValidFictivePrebends.Count.ToString() + "\n\n" + InValidPrebends.Count.ToString() + "\n\n" + InValidFictivePrebends.Count.ToString() + "\n\n Triangles:  " + Triangles.Count.ToString() +"\n\n"+ dtString + "\n\n" + dtString1);

            #region извличам полигоните (многоъгълниците)
            Polygons = new List<Polygon>();
            foreach (Triangle ttr in Triangles)
            {
                Polygon p = new Polygon(ttr, ref Triangles, ref Polygons);
                if ((p.Triangles_Numers_Array != null) && (p.Bends_Numers_Array != null))
                {
                    Polygons.Add(p);
                }
            }
            #endregion

            #region Fill Poligon Numers
            for (int i = 0; i < Polygons.Count; i++)
            {
                Polygons[i].SetNumer(i);
                foreach (int N in Polygons[i].Triangles_Numers_Array)
                {
                    Triangles[N].PolygonNumer = i;
                }
            }
            #endregion

            SetTrianglesNormalsSameAsTheSet();

            bool testTrianglesNormals = true;
            foreach (Triangle tr in Triangles)
            {
                if ((object)tr.Normal == null || (object)tr.Normal.First == null || (object)tr.Normal.Second == null)
                {
                    testTrianglesNormals = false;
                    break;
                }
            }
            if (testTrianglesNormals)
            {
                SetBendsNormals();
                SetNodesNormals();
            }
            else
            {
                error = true;
            }

        }           

        public void Save(bool first = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            string addres = "ROOT_KOJTO_GLASS_DOMMES";
            if (!first)
                addres = "ROOT_KOJTO_GLASS_DOMMES_S";

            try
            {
                #region delete old
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    DBDictionary NOD = default(DBDictionary);
                    ObjectId idNod = db.NamedObjectsDictionaryId;
                    NOD = tr.GetObject(idNod, OpenMode.ForWrite, false) as DBDictionary;
                    if (NOD != null)
                    {
                        if (NOD.Contains(addres))
                        {
                            NOD.UpgradeOpen();
                            NOD.Remove(NOD.GetAt(addres));
                        }
                    }
                    tr.Commit();
                    ed.UpdateScreen();
                }
                #endregion

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    ObjectId RootId = CreateBaseDictionaryEntry(addres);// create a base level dictionary entry

                    ObjectId appId1 = CreateDictionaryEntry(RootId, "BENDS");
                    DBDictionary childBends = tr.GetObject(appId1, OpenMode.ForRead, false) as DBDictionary;

                    ObjectId appId2 = CreateDictionaryEntry(RootId, "NODES");
                    DBDictionary childNodes = tr.GetObject(appId2, OpenMode.ForRead, false) as DBDictionary;

                    ObjectId appId3 = CreateDictionaryEntry(RootId, "TRIANGLES");
                    DBDictionary childTriangles = tr.GetObject(appId3, OpenMode.ForRead, false) as DBDictionary;

                    ObjectId appId4 = CreateDictionaryEntry(RootId, "POLYGONS");
                    DBDictionary childPolygons = tr.GetObject(appId4, OpenMode.ForRead, false) as DBDictionary;

                    ObjectId appId5 = CreateDictionaryEntry(RootId, "CONSTANTS_AND_SETTINGS");
                    DBDictionary childConstandsAndSettings = tr.GetObject(appId5, OpenMode.ForRead, false) as DBDictionary;

                    foreach (Bend b in Bends)
                    {
                        Xrecord xRec = b.GetXrecord();
                        string key = b.GetKey();

                        childBends.UpgradeOpen();
                        childBends.SetAt(key, xRec);
                        db.TransactionManager.AddNewlyCreatedDBObject(xRec, true);

                    }

                    foreach (Node n in Nodes)
                    {
                        Xrecord xRec = n.GetXrecord();
                        string key = n.GetKey();

                        childNodes.UpgradeOpen();
                        childNodes.SetAt(key, xRec);
                        db.TransactionManager.AddNewlyCreatedDBObject(xRec, true);

                    }

                    foreach (Triangle t in Triangles)
                    {
                        Xrecord xRec = t.GetXrecord();
                        string key = t.GetKey();

                        childTriangles.UpgradeOpen();
                        childTriangles.SetAt(key, xRec);
                        db.TransactionManager.AddNewlyCreatedDBObject(xRec, true);

                    }

                    foreach (Polygon pol in Polygons)
                    {
                        Xrecord xRec = pol.GetXrecord();
                        string key = pol.GetKey();

                        childPolygons.UpgradeOpen();
                        childPolygons.SetAt(key, xRec);
                        db.TransactionManager.AddNewlyCreatedDBObject(xRec, true);

                    }

                    {
                        Xrecord xRec = ConstantsAndSettings.GetXrecord();
                        string key = "GLASS_DOMMES_CONSTANT_AND_SETTINGS";
                        childConstandsAndSettings.UpgradeOpen();
                        childConstandsAndSettings.SetAt(key, xRec);
                        db.TransactionManager.AddNewlyCreatedDBObject(xRec, true);
                    }


                    tr.Commit();
                    ed.UpdateScreen();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("ERROR");
                ed.WriteMessage(ex.ToString());
            }
        }
        public void Read(bool first = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            DBDictionary NOD = default(DBDictionary);
            ObjectId idNod = db.NamedObjectsDictionaryId;

            string addres = "ROOT_KOJTO_GLASS_DOMMES";
            if (!first)
                addres = "ROOT_KOJTO_GLASS_DOMMES_S";

            /*
                this.Bends.Clear();
                this.Triangles.Clear();
                this.Nodes.Clear();
                this.Polygons.Clear();

                
                Bends = new BENDS_ARRAY();
                Nodes = new NODES_ARRAY();
                Triangles = new TRIANGLES_ARRAY();
                Polygons = new POLYGONS_ARRAY();
                peripheralBendsNumers = new List<int>();
                */

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                NOD = tr.GetObject(idNod, OpenMode.ForRead, false) as DBDictionary;
                if ((NOD != null))
                {
                    if (NOD.Contains(addres))
                    {
                        ObjectId idDict = NOD.GetAt(addres);
                        DBDictionary dbDict = tr.GetObject(idDict, OpenMode.ForRead) as DBDictionary;
                        if ((dbDict != null))
                        {
                            if (dbDict.Contains("BENDS"))
                            {
                                ObjectId id = dbDict.GetAt("BENDS");
                                DBDictionary dbDictBends = tr.GetObject(id, OpenMode.ForRead) as DBDictionary;
                                if (dbDictBends != null)
                                {
                                    DbDictionaryEnumerator index = dbDictBends.GetEnumerator();
                                    while (index.MoveNext())
                                    {
                                        if ((object)index.Current.Value != null)
                                        {
                                            ObjectId idd = index.Current.Value;
                                            Xrecord xrec = tr.GetObject(idd, OpenMode.ForRead) as Xrecord;
                                            if (xrec != null)
                                            {
                                                Bends.Add(new Bend(xrec));
                                            }
                                            xrec.Dispose();
                                        }
                                    }
                                    index.Dispose();
                                }
                                dbDictBends.Dispose();
                            }

                            if (dbDict.Contains("NODES"))
                            {
                                ObjectId id = dbDict.GetAt("NODES");
                                DBDictionary dbDictNodes = tr.GetObject(id, OpenMode.ForRead) as DBDictionary;
                                if (dbDictNodes != null)
                                {
                                    DbDictionaryEnumerator index = dbDictNodes.GetEnumerator();
                                    while (index.MoveNext())
                                    {
                                        if ((object)index.Current.Value != null)
                                        {
                                            ObjectId idd = index.Current.Value;
                                            Xrecord xrec = tr.GetObject(idd, OpenMode.ForRead) as Xrecord;
                                            if (xrec != null)
                                            {
                                                Nodes.Add(new Node(xrec));
                                            }
                                            xrec.Dispose();
                                        }
                                    }
                                    index.Dispose();
                                }
                                dbDictNodes.Dispose();
                            }

                            if (dbDict.Contains("TRIANGLES"))
                            {
                                ObjectId id = dbDict.GetAt("TRIANGLES");
                                DBDictionary dbDictTriangles = tr.GetObject(id, OpenMode.ForRead) as DBDictionary;
                                if (dbDictTriangles != null)
                                {
                                    DbDictionaryEnumerator index = dbDictTriangles.GetEnumerator();
                                    while (index.MoveNext())
                                    {
                                        if ((object)index.Current.Value != null)
                                        {
                                            ObjectId idd = index.Current.Value;
                                            Xrecord xrec = tr.GetObject(idd, OpenMode.ForRead) as Xrecord;
                                            if (xrec != null)
                                            {
                                                Triangles.Add(new Triangle(xrec));
                                            }
                                            xrec.Dispose();
                                        }
                                    }
                                    index.Dispose();
                                }
                                dbDictTriangles.Dispose();
                            }

                            if (dbDict.Contains("POLYGONS"))
                            {
                                ObjectId id = dbDict.GetAt("POLYGONS");
                                DBDictionary dbDictPolygons = tr.GetObject(id, OpenMode.ForRead) as DBDictionary;
                                if (dbDictPolygons != null)
                                {
                                    DbDictionaryEnumerator index = dbDictPolygons.GetEnumerator();
                                    while (index.MoveNext())
                                    {
                                        if ((object)index.Current.Value != null)
                                        {
                                            ObjectId idd = index.Current.Value;
                                            Xrecord xrec = tr.GetObject(idd, OpenMode.ForRead) as Xrecord;
                                            if (xrec != null)
                                            {
                                                Polygons.Add(new Polygon(xrec));
                                            }
                                            xrec.Dispose();
                                        }
                                    }
                                    index.Dispose();
                                }
                                dbDictPolygons.Dispose();
                            }

                            BendComparerByNumer pbcB = new BendComparerByNumer();
                            NodeComparerByNumer pbcN = new NodeComparerByNumer();
                            TriangleComparerByNumer pbcT = new TriangleComparerByNumer();
                            PolygonComparerByNumer pbcP = new PolygonComparerByNumer();
                            Bends.Sort(pbcB);
                            Nodes.Sort(pbcN);
                            Triangles.Sort(pbcT);
                            Polygons.Sort(pbcP);

                            peripheralBendsNumers = new List<int>();
                            #region попълваме масива с периферни пръти
                            foreach (Bend b in Bends)
                            {
                                if (b.GetSecondTriangleNumer() < 0)
                                    peripheralBendsNumers.Add(b.GetNumer());
                            }
                            #endregion


                            if (dbDict.Contains("CONSTANTS_AND_SETTINGS"))
                            {
                                ObjectId id = dbDict.GetAt("CONSTANTS_AND_SETTINGS");
                                DBDictionary dbDictConstantsAndSettings = tr.GetObject(id, OpenMode.ForRead) as DBDictionary;
                                if (dbDictConstantsAndSettings != null)
                                {
                                    DbDictionaryEnumerator index = dbDictConstantsAndSettings.GetEnumerator();
                                    while (index.MoveNext())
                                    {
                                        if ((object)index.Current.Value != null)
                                        {
                                            ObjectId idd = index.Current.Value;
                                            Xrecord xrec = tr.GetObject(idd, OpenMode.ForRead) as Xrecord;
                                            if (xrec != null)
                                            {
                                                ConstantsAndSettings.Set(xrec);
                                            }
                                            xrec.Dispose();
                                        }
                                        break;
                                    }
                                    index.Dispose();
                                }
                                dbDictConstantsAndSettings.Dispose();
                            }
                        }
                        dbDict.Dispose();
                    }
                }
                NOD.Dispose();
            }
        }
        public bool Delete(bool first = true)
        {
            bool rez = false;

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            string addres = "ROOT_KOJTO_GLASS_DOMMES";
            if (!first)
                addres = "ROOT_KOJTO_GLASS_DOMMES_S";

            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    DBDictionary NOD = default(DBDictionary);
                    ObjectId idNod = db.NamedObjectsDictionaryId;
                    NOD = tr.GetObject(idNod, OpenMode.ForWrite, false) as DBDictionary;
                    if (NOD != null)
                    {                            
                        if (NOD.Contains(addres))
                        {
                            NOD.UpgradeOpen();
                            NOD.Remove(NOD.GetAt(addres));
                            rez = true;
                        }
                    }
                    tr.Commit();
                    ed.UpdateScreen();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ERROR");
                ed.WriteMessage(ex.ToString());
                rez = false;
            }
            return rez;
        }
        //--------------------------------------------------------------
        public bool SaveInExternalFile(string filename)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    sw.WriteLine("--- Header");
                    sw.WriteLine(Bends.Count);
                    sw.WriteLine(Nodes.Count);
                    sw.WriteLine(Triangles.Count);
                    sw.WriteLine(Polygons.Count);
                    sw.WriteLine("--- End Header");
                    sw.WriteLine();
                    sw.WriteLine("--- Bends ---");
                    sw.WriteLine();
                    foreach (Bend b in Bends)
                    {
                        sw.WriteLine("-------------------------------");
                        b.ToStream(sw);
                    }
                    sw.WriteLine();
                    sw.WriteLine("--- Nodes ---");
                    sw.WriteLine();
                    foreach (Node n in Nodes)
                    {
                        sw.WriteLine("==================================");
                        n.ToStream(sw);
                    }
                    sw.WriteLine();
                    sw.WriteLine("--- Triangles ---");
                    sw.WriteLine();
                    foreach (Triangle t in Triangles)
                    {
                        sw.WriteLine("-------------------------------");
                        t.ToStream(sw);
                    }
                    sw.WriteLine();
                    sw.WriteLine("--- Polygons ---");
                    sw.WriteLine();
                    foreach (Polygon p in Polygons)
                    {
                        sw.WriteLine("==================================");
                        p.ToStream(sw);
                    }
                    sw.WriteLine();
                    sw.WriteLine("--- Settings ---");
                    sw.WriteLine();
                    ConstantsAndSettings.ToStream(sw);
                    sw.WriteLine();

                    sw.Flush();
                    sw.Close();
                }
            }
            catch
            {
                MessageBox.Show("File not Saved correctly !", "ERROR");
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nFile not Saved correctly !\n");
                return false;
            }
            return true;
        }
        public bool ReadFromExternalFile(string filename)
        {
            bool bMess = false;
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    long bendsCount = 0;
                    long nodesCount = 0;
                    long trianglesCount = 0;
                    long polygonsCount = 0;
                    try
                    {
                        sr.ReadLine(); // header line
                        bendsCount = Convert.ToInt64(sr.ReadLine());
                        nodesCount = Convert.ToInt64(sr.ReadLine());
                        trianglesCount = Convert.ToInt64(sr.ReadLine());
                        polygonsCount = Convert.ToInt64(sr.ReadLine());
                    }
                    catch
                    {
                        bMess = true;
                        MessageBox.Show("Error reading the head of  file !", "ERROR");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError reading the Head Part of  file !\n");
                        throw new InvalidOperationException("Head cannot be read !");
                    }

                    //MessageBox.Show(string.Format("{0}\n{1}\n{2}\n{3}",bendsCount, nodesCount, trianglesCount, polygonsCount));
                    sr.ReadLine();// end header line
                    sr.ReadLine();// space
                    sr.ReadLine();//--- Bends ---
                    sr.ReadLine();//space
                    sr.ReadLine();//"------------------------------"
                    try
                    {
                        for (int i = 0; i < bendsCount; i++)
                        {
                            Bend bend = new Bend(sr);
                            Bends.Add(bend);
                            sr.ReadLine();//"------------------------------"
                        }
                    }
                    catch
                    {
                        bMess = true;
                        MessageBox.Show("Error reading the Bends Part of  file !", "ERROR");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError reading the Bends Part of  file !\n");
                        throw new InvalidOperationException("Bends Data cannot be read !");
                    }
                    //sr.ReadLine();//space
                    sr.ReadLine();//--- Nodes ---
                    sr.ReadLine();//space
                    sr.ReadLine();//"==============================="  
                    try
                    {
                        for (int i = 0; i < nodesCount; i++)
                        {
                            Node node = new Node(sr);
                            Nodes.Add(node);
                            sr.ReadLine();//"------------------------------"
                        }
                    }
                    catch
                    {
                        bMess = true;
                        MessageBox.Show("Error reading the Nodes Part of  file !", "ERROR");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError reading the Nodes Part of  file !\n");
                        throw new InvalidOperationException("Nodes Data cannot be read !");
                    }

                    sr.ReadLine();//--- Triangles ---
                    sr.ReadLine();//space
                    sr.ReadLine();//"------------------------------"

                    try
                    {
                        for (int i = 0; i < trianglesCount; i++)
                        {
                            Triangle tr = new Triangle(sr);
                            Triangles.Add(tr);
                            sr.ReadLine();//"------------------------------"
                        }
                    }
                    catch
                    {
                        bMess = true;
                        MessageBox.Show("Error reading the Triangles Part of  file !", "ERROR");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError reading the Triangles Part of  file !\n");
                        throw new InvalidOperationException("Triangles Data cannot be read !");
                    }

                    sr.ReadLine();//--- Polygons ---
                    sr.ReadLine();//space
                    sr.ReadLine();//"------------------------------"

                    try
                    {
                        //MessageBox.Show(polygonsCount.ToString());
                        for (int i = 0; i < polygonsCount; i++)
                        {
                            Polygon PO = new Polygon(sr);
                            Polygons.Add(PO);
                            sr.ReadLine();//"------------------------------"
                        }
                    }
                    catch
                    {
                        bMess = true;
                        MessageBox.Show("Error reading the Polygons Part of  file !", "ERROR");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError reading the Polygons Part of  file !\n");
                        throw new InvalidOperationException("Polygons Data cannot be read !");
                    }

                    sr.ReadLine();//--- Settings ---
                    sr.ReadLine();//space

                    #region попълваме масива с периферни пръти
                    //----------------------------
                    peripheralBendsNumers = new List<int>();                       
                    foreach (Bend b in Bends)
                    {
                        if (b.GetSecondTriangleNumer() < 0)
                            peripheralBendsNumers.Add(b.GetNumer());
                    }
                    //----------------------------
                    #endregion

                    try
                    {
                        ConstantsAndSettings.Set(sr);
                    }
                    catch
                    {
                        bMess = true;
                        MessageBox.Show("Error reading the Settings Part of  file !", "ERROR");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError reading the Settings Part of  file !\n");
                        throw new InvalidOperationException("Settings Data cannot be read !");
                    }


                    sr.Close();
                }
            }
            catch
            {
                if (!bMess)
                {
                    MessageBox.Show("Error reading file !", "ERROR");
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError reading file !\n");
                }

                return false;
            }

            return true;
        }
        //-------------------------------------------------------------
        public Triangle GetFirstAdjoiningTriangle(int trNumer)
        {
            Triangle rez = null;
            if ((trNumer < Triangles.Count) && (trNumer >= 0))
            {


                int N1 = Bends[Triangles[trNumer].GetFirstBendNumer()].FirstTriangleNumer;
                int N2 = Bends[Triangles[trNumer].GetFirstBendNumer()].SecondTriangleNumer;

                int N = (N1 == trNumer) ? N2 : N1;

                if ((N >= 0) && (N < Triangles.Count))
                    rez = Triangles[N];
            }
            return rez;
        }
        public Triangle GetSecondAdjoiningTriangle(int trNumer)
        {
            Triangle rez = null;
            if ((trNumer < Triangles.Count) && (trNumer >= 0))
            {
                int N1 = Bends[Triangles[trNumer].GetSecondBendNumer()].FirstTriangleNumer;
                int N2 = Bends[Triangles[trNumer].GetSecondBendNumer()].SecondTriangleNumer;

                int N = (N1 == trNumer) ? N2 : N1;

                if ((N >= 0) && (N < Triangles.Count))
                    rez = Triangles[N];
            }
            return rez;
        }
        public Triangle GetThirdAdjoiningTriangle(int trNumer)
        {
            Triangle rez = null;
            if ((trNumer < Triangles.Count) && (trNumer >= 0))
            {
                int N1 = Bends[Triangles[trNumer].GetThirdBendNumer()].FirstTriangleNumer;
                int N2 = Bends[Triangles[trNumer].GetThirdBendNumer()].SecondTriangleNumer;

                int N = (N1 == trNumer) ? N2 : N1;

                if ((N >= 0) && (N < Triangles.Count))
                    rez = Triangles[N];
            }
            return rez;
        }
        public Triangle[] GetAdjoiningTriangles(int trNumer)
        {
            return new Triangle[] { GetFirstAdjoiningTriangle(trNumer), GetSecondAdjoiningTriangle(trNumer), GetThirdAdjoiningTriangle(trNumer) };
        }
        public Triangle[] GetAdjoiningTriangles(Triangle tr)
        {
            return new Triangle[] { GetFirstAdjoiningTriangle(tr.Numer), GetSecondAdjoiningTriangle(tr.Numer), GetThirdAdjoiningTriangle(tr.Numer) };
        }

        public void SetTrianglesNormalsSameAsTheSet(Triangle TR)
        {
            quaternion q = TR.Normal.Second - TR.Normal.First;
            Triangle[] trARR = GetAdjoiningTriangles(TR);

            for (int i = 0; i < 3; i++)
                //foreach (Triangle tr in trARR)
            {
                Triangle tr = trARR[i];
                if ((Object)tr != null)
                {
                    if ((object)tr.Normal.Second == null)
                    {
                        Bend bend = null;
                        switch (i)
                        {
                            case 0: bend = Bends[TR.GetFirstBendNumer()]; break;
                            case 1: bend = Bends[TR.GetSecondBendNumer()]; break;
                            case 2: bend = Bends[TR.GetThirdBendNumer()]; break;
                        }

                        bool StartEnd = true;
                        UCS ucs = new UCS(bend.Start, bend.End, TR.GetCentroid());
                        if (ucs.FromACS(TR.Normal.Second).GetZ() < 0.0)
                        {
                            ucs = new UCS(bend.End, bend.Start, TR.GetCentroid());
                            StartEnd = false;
                        }

                        UCS UCS = new UCS();
                        if (StartEnd)
                            UCS = new UCS(tr.GetCentroid(), bend.End, bend.Start);
                        else
                            UCS = new UCS(tr.GetCentroid(), bend.Start, bend.End);

                        tr.Normal.Second = UCS.ToACS(new quaternion(0, 0, 0, 1));
                        SetTrianglesNormalsSameAsTheSet(tr);
                    }
                }
            }
        }
        public void SetTrianglesNormalsSameAsTheSet()
        {
            foreach (Triangle TR in Triangles)
            {
                TR.Normal.Second = null;
            }

            Triangles[0].SetNormal();
            SetTrianglesNormalsSameAsTheSet(Triangles[0]);
        }
        public void ReverseTrianglesNormals()
        {
            foreach (Triangle TR in Triangles)
            {
                TR.Normal.Second = TR.Normal.First + TR.Normal.First - TR.Normal.Second;
            }
        }
        #region old set nodes normals
        /*
               public void SetNodesNormals()
                {
                    foreach (WorkClasses.Node node in Nodes)
                    {
                        MathLibKCAD.quaternion q = new MathLibKCAD.quaternion();
                        foreach (int trN in node.Triangles_Numers_Array)
                        {
                            MathLibKCAD.quaternion q1=Triangles[trN].Normal.Second - Triangles[trN].Normal.First;
                            q1 /= q1.abs();
                            q += q1;
                        }
                        q /= node.Triangles_Numers_Array.Count;
                        q /= q.abs();
                        node.Normal = node.Position + q;
                    }
                }             
             */
        #endregion
        public void SetNodesNormals()
        {
            foreach (Node node in Nodes)
            {
                if ((object)node.ExplicitNormal == null)
                {
                    quaternion q = new quaternion();
                    foreach (int bN in node.Bends_Numers_Array)
                    {
                        quaternion q1 = Bends[bN].Normal - Bends[bN].MidPoint;
                        q1 /= q1.abs();
                        q += q1;
                    }
                    q /= node.Bends_Numers_Array.Count;
                    q /= q.abs();
                    node.Normal = node.Position + q;
                }
            }
        }
        public void RestoreNodesNormals()
        {
            foreach (Node node in Nodes)
            {
                quaternion q = new quaternion();
                foreach (int bN in node.Bends_Numers_Array)
                {
                    quaternion q1 = Bends[bN].Normal - Bends[bN].MidPoint;
                    q1 /= q1.abs();
                    q += q1;
                }
                q /= node.Bends_Numers_Array.Count;
                q /= q.abs();
                node.Normal = node.Position + q;
                node.ExplicitNormal = null;
            }
        }
        public void RestoreNodesNormals(int nodeNumer)
        {
            Node node = Nodes[nodeNumer];

            quaternion q = new quaternion();
            foreach (int bN in node.Bends_Numers_Array)
            {
                quaternion q1 = Bends[bN].Normal - Bends[bN].MidPoint;
                q1 /= q1.abs();
                q += q1;
            }
            q /= node.Bends_Numers_Array.Count;
            q /= q.abs();
            node.Normal = node.Position + q;
            node.ExplicitNormal = null;

        }
        public void ReverseNodesNormals()
        {
            foreach (Node n in Nodes)
            {
                n.ReverseNormal();
            }
        }
        public void SetBendsNormals()
        {
            foreach (Bend bend in Bends)
            {
                if (bend.ExplicitNormal == 0)
                {
                    quaternion m = bend.MidPoint;
                    if ((object)bend.MidPoint == null)
                    {
                        m = (bend.Start + bend.End) / 2.0;
                        bend.MidPoint = m;
                    }

                    quaternion q1 = Triangles[bend.FirstTriangleNumer].Normal.Second - Triangles[bend.FirstTriangleNumer].Normal.First;
                    quaternion q3 = m + q1;
                    q1 /= q1.abs();
                    if (bend.SecondTriangleNumer < 0)
                    {
                        if (ConstantsAndSettings.PerepherialBendsNormalDirection == 0)
                        {
                            bend.Normal = q3;
                        }
                        else
                        {
                            UCS trUCS = new UCS(Triangles[bend.FirstTriangleNumer].Normal.First, bend.Start, bend.End);
                            bool bSign = (trUCS.FromACS(Triangles[bend.FirstTriangleNumer].Normal.Second).GetZ() >= 0) ? true : false;
                            if (!bSign)
                                trUCS = new UCS(Triangles[bend.FirstTriangleNumer].Normal.First, bend.End, bend.Start);

                            if (ConstantsAndSettings.PerepherialBendsNormalDirection == 1)
                            {
                                quaternion q2 = new quaternion(0, 0, 0, 1) + m;
                                bend.Normal = q2;
                                if (Math.Abs(trUCS.FromACS(q2).GetZ()) <= ConstantsAndSettings.MinDistBhetwenNodes)
                                {
                                    if ((Triangles[bend.FirstTriangleNumer].Normal.First - m).abs() < (Triangles[bend.FirstTriangleNumer].Normal.First - q2).abs())
                                    {
                                        bend.Normal = m + m - q2;
                                    }
                                }
                                else
                                    if (trUCS.FromACS(m + (q2 - m) * 10000.0).GetZ() < 0.0)
                                    {
                                        bend.Normal = m + m - q2;
                                    }
                            }
                            if (ConstantsAndSettings.PerepherialBendsNormalDirection == 2)
                            {
                                quaternion q2 = new quaternion(0, 0, 1, 0) + m;
                                bend.Normal = q2;
                                if (Math.Abs(trUCS.FromACS(q2).GetZ()) <= ConstantsAndSettings.MinDistBhetwenNodes)
                                {
                                    if ((Triangles[bend.FirstTriangleNumer].Normal.First - m).abs() < (Triangles[bend.FirstTriangleNumer].Normal.First - q2).abs())
                                    {
                                        bend.Normal = m + m - q2;
                                    }
                                }
                                else
                                    if (trUCS.FromACS(m + (q2 - m) * 10000.0).GetZ() < 0.0)
                                    {
                                        bend.Normal = m + m - q2;
                                    }
                            }
                            if (ConstantsAndSettings.PerepherialBendsNormalDirection == 3)
                            {
                                quaternion q2 = new quaternion(0, 1, 0, 0) + m;
                                bend.Normal = q2;
                                if (Math.Abs(trUCS.FromACS(q2).GetZ()) <= ConstantsAndSettings.MinDistBhetwenNodes)
                                {
                                    if ((Triangles[bend.FirstTriangleNumer].Normal.First - m).abs() < (Triangles[bend.FirstTriangleNumer].Normal.First - q2).abs())
                                    {
                                        bend.Normal = m + m - q2;
                                    }
                                }
                                else
                                    if (trUCS.FromACS(m + (q2 - m) * 10000.0).GetZ() < 0.0)
                                    {
                                        bend.Normal = m + m - q2;
                                    }
                            }
                        }

                        quaternion bak = bend.Normal;
                        UCS ucs = bend.GetUCS();
                        bend.Normal = ucs.ToACS(new quaternion(0, 0, 1.0, 0));
                        if (double.IsNaN(bend.Normal.GetX()) || double.IsNaN(bend.Normal.GetY()) || double.IsNaN(bend.Normal.GetZ()))
                        {
                            bend.Normal = bak;
                            string mess = string.Format("Bend Numer {0} Normal Error !", bend.Numer + 1);
                            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(mess);
                            MessageBox.Show(mess, "E R R O R");
                        }
                    }
                    else
                    {
                        quaternion q2 = Triangles[bend.SecondTriangleNumer].Normal.Second - Triangles[bend.SecondTriangleNumer].Normal.First;
                        q2 /= q2.abs();

                        bend.Normal = (q1 + q2) / 2.0;
                        bend.Normal /= bend.Normal.abs();
                        bend.Normal += m;
                    }
                }
            }//
        }
        public void RestoreBendsNormals()
        {
            foreach (Bend bend in Bends)
            {
                quaternion m = bend.MidPoint;
                if ((object)bend.MidPoint == null)
                {
                    m = (bend.Start + bend.End) / 2.0;
                    bend.MidPoint = m;
                }

                quaternion q1 = Triangles[bend.FirstTriangleNumer].Normal.Second - Triangles[bend.FirstTriangleNumer].Normal.First;
                quaternion q3 = m + q1;
                q1 /= q1.abs();
                if (bend.SecondTriangleNumer < 0)
                {
                    if (ConstantsAndSettings.PerepherialBendsNormalDirection == 0)
                    {
                        bend.Normal = q3;
                    }
                    else
                    {
                        UCS trUCS = new UCS(Triangles[bend.FirstTriangleNumer].Normal.First, bend.Start, bend.End);
                        bool bSign = (trUCS.FromACS(Triangles[bend.FirstTriangleNumer].Normal.Second).GetZ() >= 0) ? true : false;
                        if (!bSign)
                            trUCS = new UCS(Triangles[bend.FirstTriangleNumer].Normal.First, bend.End, bend.Start);

                        if (ConstantsAndSettings.PerepherialBendsNormalDirection == 1)
                        {
                            quaternion q2 = new quaternion(0, 0, 0, 1) + m;
                            bend.Normal = q2;
                            if (Math.Abs(trUCS.FromACS(q2).GetZ()) <= ConstantsAndSettings.MinDistBhetwenNodes)
                            {
                                if ((Triangles[bend.FirstTriangleNumer].Normal.First - m).abs() < (Triangles[bend.FirstTriangleNumer].Normal.First - q2).abs())
                                {
                                    bend.Normal = m + m - q2;
                                }
                            }
                            else
                                if (trUCS.FromACS(m + (q2 - m) * 10000.0).GetZ() < 0.0)
                                {
                                    bend.Normal = m + m - q2;
                                }
                        }
                        if (ConstantsAndSettings.PerepherialBendsNormalDirection == 2)
                        {
                            quaternion q2 = new quaternion(0, 0, 1, 0) + m;
                            bend.Normal = q2;
                            if (Math.Abs(trUCS.FromACS(q2).GetZ()) <= ConstantsAndSettings.MinDistBhetwenNodes)
                            {
                                if ((Triangles[bend.FirstTriangleNumer].Normal.First - m).abs() < (Triangles[bend.FirstTriangleNumer].Normal.First - q2).abs())
                                {
                                    bend.Normal = m + m - q2;
                                }
                            }
                            else
                                if (trUCS.FromACS(m + (q2 - m) * 10000.0).GetZ() < 0.0)
                                {
                                    bend.Normal = m + m - q2;
                                }
                        }
                        if (ConstantsAndSettings.PerepherialBendsNormalDirection == 3)
                        {
                            quaternion q2 = new quaternion(0, 1, 0, 0) + m;
                            bend.Normal = q2;
                            if (Math.Abs(trUCS.FromACS(q2).GetZ()) <= ConstantsAndSettings.MinDistBhetwenNodes)
                            {
                                if ((Triangles[bend.FirstTriangleNumer].Normal.First - m).abs() < (Triangles[bend.FirstTriangleNumer].Normal.First - q2).abs())
                                {
                                    bend.Normal = m + m - q2;
                                }
                            }
                            else
                                if (trUCS.FromACS(m + (q2 - m) * 10000.0).GetZ() < 0.0)
                                {
                                    bend.Normal = m + m - q2;
                                }
                        }
                    }

                    quaternion bak = bend.Normal;
                    UCS ucs = bend.GetUCS();
                    bend.Normal = ucs.ToACS(new quaternion(0, 0, 1.0, 0));
                    if (double.IsNaN(bend.Normal.GetX()) || double.IsNaN(bend.Normal.GetY()) || double.IsNaN(bend.Normal.GetZ()))
                    {
                        bend.Normal = bak;
                        string mess = string.Format("Bend Numer {0} Normal Error !", bend.Numer + 1);
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(mess);
                        MessageBox.Show(mess, "E R R O R");
                    }
                }
                else
                {
                    quaternion q2 = Triangles[bend.SecondTriangleNumer].Normal.Second - Triangles[bend.SecondTriangleNumer].Normal.First;
                    q2 /= q2.abs();

                    bend.Normal = (q1 + q2) / 2.0;
                    bend.Normal /= bend.Normal.abs();
                    bend.Normal += m;
                }
                bend.ExplicitNormal = 0;
            }//
        }
        public void ReverseBendsNormals()
        {
            foreach (Bend b in Bends)
            {
                b.Normal = b.MidPoint + b.MidPoint - b.Normal;
            }
        }
        //---------------------------------------------------------------
        public void ReverseNormals()
        {
            foreach (Triangle tr in Triangles)
            {
                tr.Normal.Second = tr.Normal.First + tr.Normal.First - tr.Normal.Second;
            }
            foreach (Node n in Nodes)
            {
                n.ReverseNormal();
            }
            foreach (Bend b in Bends)
            {
                b.Normal = b.MidPoint + b.MidPoint - b.Normal;
            }
        }
        public Triplet<quaternion, quaternion, quaternion> GetInnererTriangle(Triangle ambientTR, bool bbo = false)
        {
            Triplet<quaternion, quaternion, quaternion> rez = new Triplet<quaternion, quaternion, quaternion>(new quaternion(), new quaternion(), new quaternion());
            bool bb = (bbo == true) ? true : ((!Bends[ambientTR.GetFirstBendNumer()].IsFictive()) && (!Bends[ambientTR.GetSecondBendNumer()].IsFictive()) && (!Bends[ambientTR.GetThirdBendNumer()].IsFictive()));
            if (bb)
            {
                UCS ucs = ambientTR.GetUcsByCentroid1();
                Bend bend1 = Bends[ambientTR.GetFirstBendNumer()];
                Bend bend2 = Bends[ambientTR.GetSecondBendNumer()];
                Bend bend3 = Bends[ambientTR.GetThirdBendNumer()];

                double offset1 = (bend1.FirstTriangleNumer == ambientTR.Numer) ? bend1.FirstTriangleOffset : bend1.SecondTriangleOffset;
                double offset2 = (bend2.FirstTriangleNumer == ambientTR.Numer) ? bend2.FirstTriangleOffset : bend2.SecondTriangleOffset;
                double offset3 = (bend3.FirstTriangleNumer == ambientTR.Numer) ? bend3.FirstTriangleOffset : bend3.SecondTriangleOffset;

                line2d l1 = new line2d(bend1.GetStartComplex(ref ucs), bend1.GetEndComplex(ref ucs));
                line2d l2 = new line2d(bend2.GetStartComplex(ref ucs), bend2.GetEndComplex(ref ucs));
                line2d l3 = new line2d(bend3.GetStartComplex(ref ucs), bend3.GetEndComplex(ref ucs));

                l1 = new line2d(l1, -offset1);
                l2 = new line2d(l2, -offset2);
                l3 = new line2d(l3, -offset3);

                complex ic1 = l1.IntersectWitch(l2);
                complex ic2 = l1.IntersectWitch(l3);
                complex ic3 = l2.IntersectWitch(l3);

                rez.First = ucs.ToACS(new quaternion(0, ic1.real(), ic1.imag(), 0.0));
                rez.Second = ucs.ToACS(new quaternion(0, ic2.real(), ic2.imag(), 0.0));
                rez.Third = ucs.ToACS(new quaternion(0, ic3.real(), ic3.imag(), 0.0));
            }
            else
            {
                rez.First = null; rez.Second = null; rez.Third = null;
            }
            return rez;
        }
        public Triplet<quaternion, quaternion, quaternion> GetInnererTriangle(int ambientTriangleNumer, bool bbo = false)
        {
            Triplet<quaternion, quaternion, quaternion> rez = new Triplet<quaternion, quaternion, quaternion>(new quaternion(), new quaternion(), new quaternion());
            Triangle TR = Triangles[ambientTriangleNumer];
            bool bb = (bbo == true) ? true : ((!Bends[TR.GetFirstBendNumer()].IsFictive()) && (!Bends[TR.GetSecondBendNumer()].IsFictive()) && (!Bends[TR.GetThirdBendNumer()].IsFictive()));
            if (bb)
            {
                UCS ucs = TR.GetUcsByCentroid1();
                Bend bend1 = Bends[TR.GetFirstBendNumer()];
                Bend bend2 = Bends[TR.GetSecondBendNumer()];
                Bend bend3 = Bends[TR.GetThirdBendNumer()];

                double offset1 = (bend1.FirstTriangleNumer == TR.Numer) ? bend1.FirstTriangleOffset : bend1.SecondTriangleOffset;
                double offset2 = (bend2.FirstTriangleNumer == TR.Numer) ? bend2.FirstTriangleOffset : bend2.SecondTriangleOffset;
                double offset3 = (bend3.FirstTriangleNumer == TR.Numer) ? bend3.FirstTriangleOffset : bend3.SecondTriangleOffset;

                line2d l1 = new line2d(bend1.GetStartComplex(ref ucs), bend1.GetEndComplex(ref ucs));
                line2d l2 = new line2d(bend2.GetStartComplex(ref ucs), bend2.GetEndComplex(ref ucs));
                line2d l3 = new line2d(bend3.GetStartComplex(ref ucs), bend3.GetEndComplex(ref ucs));

                l1 = new line2d(l1, -offset1);
                l2 = new line2d(l2, -offset2);
                l3 = new line2d(l3, -offset3);

                complex ic1 = l1.IntersectWitch(l2);
                complex ic2 = l1.IntersectWitch(l3);
                complex ic3 = l2.IntersectWitch(l3);

                rez.First = ucs.ToACS(new quaternion(0, ic1.real(), ic1.imag(), 0.0));
                rez.Second = ucs.ToACS(new quaternion(0, ic2.real(), ic2.imag(), 0.0));
                rez.Third = ucs.ToACS(new quaternion(0, ic3.real(), ic3.imag(), 0.0));
            }
            else
            {
                rez.First = null; rez.Second = null; rez.Third = null;
            }
            return rez;
        }

        //-- double glass
        public Triplet<quaternion, quaternion, quaternion> GetInnererTriangle(Triangle TR, double a/*lowerSide_H*/, double b/*thickness*/)
        {
            Triplet<quaternion, quaternion, quaternion> rez = new Triplet<quaternion, quaternion, quaternion>(new quaternion(), new quaternion(), new quaternion());
            if ((!Bends[TR.GetFirstBendNumer()].IsFictive()) || (!Bends[TR.GetSecondBendNumer()].IsFictive()) || (!Bends[TR.GetThirdBendNumer()].IsFictive()))
            {
                Bend bend1 = Bends[TR.GetFirstBendNumer()];
                Bend bend2 = Bends[TR.GetSecondBendNumer()];
                Bend bend3 = Bends[TR.GetThirdBendNumer()];

                double offset1 = (bend1.FirstTriangleNumer == TR.Numer) ? bend1.FirstTriangleOffset : bend1.SecondTriangleOffset;
                double offset2 = (bend2.FirstTriangleNumer == TR.Numer) ? bend2.FirstTriangleOffset : bend2.SecondTriangleOffset;
                double offset3 = (bend3.FirstTriangleNumer == TR.Numer) ? bend3.FirstTriangleOffset : bend3.SecondTriangleOffset;

                UCS ucsTR = TR.GetUcsByCentroid1();
                UCS ucsBend1 = bend1.GetUCS();
                UCS ucsBend2 = bend2.GetUCS();
                UCS ucsBend3 = bend3.GetUCS();
                UCS ucsTR1 = TR.GetUcsByBends1(ref Bends);
                UCS ucsTR2 = TR.GetUcsByBends2(ref Bends);
                UCS ucsTR3 = TR.GetUcsByBends3(ref Bends);

                quaternion Centroid = TR.GetCentroid();
                quaternion trNormal = TR.Normal.Second - TR.Normal.First;

                quaternion V1 = ucsBend1.FromACS(Centroid); V1 = V1 - (new quaternion(0, V1.GetX(), V1.GetY(), 0));
                quaternion V2 = ucsBend2.FromACS(Centroid); V2 = V2 - (new quaternion(0, V2.GetX(), V2.GetY(), 0));
                quaternion V3 = ucsBend3.FromACS(Centroid); V3 = V3 - (new quaternion(0, V3.GetX(), V3.GetY(), 0));

                V1 = ucsBend1.ToACS(V1) - bend1.MidPoint;
                V2 = ucsBend2.ToACS(V2) - bend2.MidPoint;
                V3 = ucsBend3.ToACS(V3) - bend3.MidPoint;

                V1 /= V1.abs();
                V2 /= V2.abs();
                V3 /= V3.abs();

                V1 *= offset1;
                V2 *= offset2;
                V3 *= offset3;

                plane pl1 = new plane(bend1.Start + V1, bend1.End + V1, bend1.Normal + V1);
                plane pl2 = new plane(bend2.Start + V2, bend2.End + V2, bend2.Normal + V2);
                plane pl3 = new plane(bend3.Start + V3, bend3.End + V3, bend3.Normal + V3);
                plane tringlePlane = new plane(TR.Nodes.First, TR.Nodes.Second, TR.Nodes.Third);


                quaternion q11 = pl1.IntersectWithVector(Centroid + trNormal * a, bend1.MidPoint + trNormal * a);
                quaternion q12 = pl1.IntersectWithVector(Centroid + trNormal * (a + b), bend1.MidPoint + trNormal * (a + b));
                quaternion q1 = (ucsTR1.FromACS(q11).GetY() > ucsTR1.FromACS(q12).GetY()) ?
                    tringlePlane.IntersectWithVector(q11, q11 + trNormal) : tringlePlane.IntersectWithVector(q12, q12 + trNormal);

                quaternion q21 = pl2.IntersectWithVector(Centroid + trNormal * a, bend2.MidPoint + trNormal * a);
                quaternion q22 = pl2.IntersectWithVector(Centroid + trNormal * (a + b), bend2.MidPoint + trNormal * (a + b));
                quaternion q2 = (ucsTR2.FromACS(q21).GetY() > ucsTR2.FromACS(q22).GetY()) ?
                    tringlePlane.IntersectWithVector(q21, q21 + trNormal) : tringlePlane.IntersectWithVector(q22, q22 + trNormal);

                quaternion q31 = pl3.IntersectWithVector(Centroid + trNormal * a, bend3.MidPoint + trNormal * a);
                quaternion q32 = pl3.IntersectWithVector(Centroid + trNormal * (a + b), bend3.MidPoint + trNormal * (a + b));
                quaternion q3 = (ucsTR3.FromACS(q31).GetY() > ucsTR3.FromACS(q32).GetY()) ?
                    tringlePlane.IntersectWithVector(q31, q31 + trNormal) : tringlePlane.IntersectWithVector(q32, q32 + trNormal);

                complex cBend1 = bend1.GetStartComplex(ref ucsTR) - bend1.GetEndComplex(ref ucsTR);
                complex cBend2 = bend2.GetStartComplex(ref ucsTR) - bend2.GetEndComplex(ref ucsTR);
                complex cBend3 = bend3.GetStartComplex(ref ucsTR) - bend3.GetEndComplex(ref ucsTR);


                q1 = ucsTR.FromACS(q1);
                q2 = ucsTR.FromACS(q2);
                q3 = ucsTR.FromACS(q3);

                complex c1 = new complex(q1.GetX(), q1.GetY());
                complex c2 = new complex(q2.GetX(), q2.GetY());
                complex c3 = new complex(q3.GetX(), q3.GetY());

                line2d l1 = new line2d(c1, c1 + cBend1);
                line2d l2 = new line2d(c2, c2 + cBend2);
                line2d l3 = new line2d(c3, c3 + cBend3);

                complex ic1 = l1.IntersectWitch(l2);
                complex ic2 = l1.IntersectWitch(l3);
                complex ic3 = l2.IntersectWitch(l3);

                rez.First = ucsTR.ToACS(new quaternion(0, ic1.real(), ic1.imag(), 0.0));
                rez.Second = ucsTR.ToACS(new quaternion(0, ic2.real(), ic2.imag(), 0.0));
                rez.Third = ucsTR.ToACS(new quaternion(0, ic3.real(), ic3.imag(), 0.0));

            }
            else
            {
                rez.First = null; rez.Second = null; rez.Third = null;
            }
            return rez;
        }

        //--- System ----------------
        public ObjectId CreateBaseDictionaryEntry(string companyName)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId companyDictId = default(ObjectId);
            try
            {
                // start a new transaction
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    //First, get the NOD...
                    DBDictionary NOD = trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead, false) as DBDictionary;
                    // Define a corporate level dictionary
                    DBDictionary companyDict = default(DBDictionary);
                    try
                    {
                        // Just throw if it doesn’t exist do nothing else.
                        companyDict = trans.GetObject(NOD.GetAt(companyName), OpenMode.ForRead) as DBDictionary;
                        companyDictId = companyDict.ObjectId;
                    }
                    catch
                    {
                        // Doesn't exist, so create one, and set it in the NOD
                        companyDict = new DBDictionary();
                        // upgrade from read to write status
                        NOD.UpgradeOpen();
                        NOD.SetAt(companyName, companyDict);
                        // tell the transaction manager about the new object
                        db.TransactionManager.AddNewlyCreatedDBObject(companyDict, true);
                        companyDictId = companyDict.ObjectId;
                    }
                    // commit the changes
                    trans.Commit();
                    Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
                }
            }
            finally
            {
                // no need to call as will be done automatically
                // trans.Dispose()
            }
            // return the company diction
            return companyDictId;
        }
        public ObjectId CreateDictionaryEntry(ObjectId parentDictId, string childDictName)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            ObjectId childDictId = default(ObjectId);
            try
            {
                // start a new transaction
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    //First, get the parent dictionary...
                    DBDictionary parentDict = trans.GetObject(parentDictId, OpenMode.ForRead, false) as DBDictionary;
                    // try and get the child dictionary entry
                    DBDictionary childDict = default(DBDictionary);
                    try
                    {
                        // Just throw if it doesn’t exist do nothing else.
                        childDict = trans.GetObject(parentDict.GetAt(childDictName), OpenMode.ForRead) as DBDictionary;
                        childDictId = childDict.ObjectId;
                    }
                    catch
                    {
                        // Doesn't exist, so create one, and set it in the NOD
                        childDict = new DBDictionary();
                        // upgrade from read to write status
                        parentDict.UpgradeOpen();
                        parentDict.SetAt(childDictName, childDict);
                        // tell the transaction manager about the new object
                        db.TransactionManager.AddNewlyCreatedDBObject(childDict, true);
                        childDictId = childDict.ObjectId;
                    }
                    // commit the transaction
                    trans.Commit();
                    Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
                }
            }
            finally
            {
                // no need to call as will be done automatically
                // trans.Dispose()
            }
            // return the company diction
            return childDictId;
        }

        //--- Poligon Functions ----------------------
        public List<quaternion> GetGlassEdgeForNodeInPolygon(int nodeNumer, Polygon POL, bool variant = false)
        {
            List<quaternion> qlist = new List<quaternion>();
            Node node = Nodes[nodeNumer];
            List<Triangle> trArr = new List<Triangle>();

            #region podregdame triygylnicite taka 4e naj-otpred sa tezi s perifernite pryti
            List<int> listTrianglesInNode = POL.GetTrianglesInNodeNumersArray(ref Nodes, nodeNumer);
            foreach (int NN in listTrianglesInNode)
            {
                Triangle triangle = Triangles[NN];
                if (POL.Triangles_Numers_Array.IndexOf(triangle.Numer) >= 0)
                {
                    int fictiveRange = triangle.GetFictiveRange();
                    if (fictiveRange == 3)
                        trArr.Add(Triangles[NN]);
                    else
                    {
                        bool perif = false;
                        Bend b1 = Bends[triangle.GetFirstBendNumer()];
                        Bend b2 = Bends[triangle.GetSecondBendNumer()];
                        Bend b3 = Bends[triangle.GetThirdBendNumer()];
                        if ((b1.IsFictive() == false) &&
                            ((b1.Start - node.Position).abs() < ConstantsAndSettings.GetMinDistBhetwenNodes() ||
                             (b1.End - node.Position).abs() < ConstantsAndSettings.GetMinDistBhetwenNodes()))
                        {
                            perif = true;
                        }
                        else
                            if ((b2.IsFictive() == false) &&
                                ((b2.Start - node.Position).abs() < ConstantsAndSettings.GetMinDistBhetwenNodes() ||
                                 (b2.End - node.Position).abs() < ConstantsAndSettings.GetMinDistBhetwenNodes()))
                            {
                                perif = true;
                            }
                            else
                                if ((b3.IsFictive() == false) &&
                                    ((b3.Start - node.Position).abs() < ConstantsAndSettings.GetMinDistBhetwenNodes() ||
                                     (b3.End - node.Position).abs() < ConstantsAndSettings.GetMinDistBhetwenNodes()))
                                {
                                    perif = true;
                                }
                        if (perif)
                            trArr.Insert(0, Triangles[NN]);
                        else
                            trArr.Add(Triangles[NN]);
                    }
                }
            }
            #endregion

            List<int> bendsNumers = new List<int>();
            #region Pyrwo slagame fiktiwnite pryti kojto sa grani4ni za 2 sysedni tiygylnika
            //pyrwiq triygylnik e s perifernen pryt zaradi po-gornoto podregdane
            //prenaregdemame i triygylnicite da obrazuwat weriga ot dopreni po fiktiwnite pryti triygylnici
            for (int i = 0; i < trArr.Count - 1; i++)
            {
                for (int j = i + 1; j < trArr.Count; j++)
                {
                    int numer = -1;
                    if (Triangle.IsContiguous(trArr[i], trArr[j], ref numer))
                    {
                        Triangle TR = trArr[i + 1];
                        trArr[i + 1] = trArr[j];
                        trArr[j] = TR;
                        bendsNumers.Add(numer);
                        break;
                    }
                }
            }
            #endregion

            #region Dobawqme pyrwiq i krajniq periferni pryti
            Bend b11 = Bends[trArr[0].GetFirstBendNumer()];
            Bend b22 = Bends[trArr[0].GetSecondBendNumer()];
            Bend b33 = Bends[trArr[0].GetThirdBendNumer()];
            if (!b11.IsFictive() && (((b11.Start - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes) ||
                                     ((b11.End - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes)))
            { bendsNumers.Insert(0, trArr[0].GetFirstBendNumer()); }
            if (!b22.IsFictive() && (((b22.Start - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes) ||
                                     ((b22.End - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes)))
            { bendsNumers.Insert(0, trArr[0].GetSecondBendNumer()); }
            if (!b33.IsFictive() && (((b33.Start - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes) ||
                                     ((b33.End - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes)))
            { bendsNumers.Insert(0, trArr[0].GetThirdBendNumer()); }
            if (trArr.Count > 1)
            {
                b11 = Bends[trArr[trArr.Count - 1].GetFirstBendNumer()];
                b22 = Bends[trArr[trArr.Count - 1].GetSecondBendNumer()];
                b33 = Bends[trArr[trArr.Count - 1].GetThirdBendNumer()];
                if (!b11.IsFictive() && (((b11.Start - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes) ||
                                         ((b11.End - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes)))
                { bendsNumers.Add(trArr[trArr.Count - 1].GetFirstBendNumer()); }
                if (!b22.IsFictive() && (((b22.Start - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes) ||
                                         ((b22.End - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes)))
                { bendsNumers.Add(trArr[trArr.Count - 1].GetSecondBendNumer()); }
                if (!b33.IsFictive() && (((b33.Start - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes) ||
                                         ((b33.End - node.Position).abs() < ConstantsAndSettings.MinDistBhetwenNodes)))
                { bendsNumers.Add(trArr[trArr.Count - 1].GetThirdBendNumer()); }
            }
            #endregion

            double ang = 0.0;
            #region Namirame ygyla mevdu perifernite pryti
            foreach (Triangle tr in trArr)
            {
                Pair<int, int> pa = tr.GetOtherNodeNumers(nodeNumer);
                quaternion q1 = Nodes[pa.First].Position - Nodes[nodeNumer].Position;
                quaternion q2 = Nodes[pa.Second].Position - Nodes[nodeNumer].Position;
                ang += q1.angTo(q2);
            }
            #endregion

            #region offsets

            double offset1 = (Bends[bendsNumers[0]].FirstTriangleNumer == trArr[0].Numer) ? Bends[bendsNumers[0]].FirstTriangleOffset : Bends[bendsNumers[0]].SecondTriangleOffset;
            double offset2 = (Bends[bendsNumers[bendsNumers.Count - 1]].FirstTriangleNumer == trArr[trArr.Count - 1].Numer) ? Bends[bendsNumers[bendsNumers.Count - 1]].FirstTriangleOffset : Bends[bendsNumers[bendsNumers.Count - 1]].SecondTriangleOffset;

            //offset1 = 50; offset2 = 50;

            #endregion

            //**;
            UCS nUCS = Bends[bendsNumers[0]].GetUCS();
            quaternion sQ = nUCS.FromACS(Bends[bendsNumers[bendsNumers.Count - 1]].Start);
            quaternion eQ = nUCS.FromACS(Bends[bendsNumers[bendsNumers.Count - 1]].End);
            bool parallel_planes = Math.Abs(sQ.GetZ() - eQ.GetZ()) < Constants.zero_dist;

            int intersect_triangle_INDEX = -1;
            #region Namirame prese4nata to4ka na perifernite za regiona pryti ot tozi wyzel
            quaternion qFirst = (Bends[bendsNumers[0]].StartNodeNumer == nodeNumer) ?
                Nodes[Bends[bendsNumers[0]].EndNodeNumer].Position : Nodes[Bends[bendsNumers[0]].StartNodeNumer].Position;
            UCS ucs = new UCS(trArr[0].GetCentroid(), Nodes[nodeNumer].Position, qFirst);

            quaternion qNode = ucs.FromACS(Nodes[nodeNumer].Position);
            qFirst = ucs.FromACS(qFirst);
            complex cNode = new complex(qNode.GetX(), qNode.GetY());
            complex cSec = new complex(qFirst.GetX(), qFirst.GetY());
            line2d L1 = new line2d(cNode, cSec);
            line2d L11 = new line2d(L1, -offset1);

            line2d L222 = L11;
            plane planeFirst = new plane();
            plane planeSecond = new plane();

            #region pereferialbends - parallel
            if ((Math.Abs(ang - Math.PI) < Constants.zero_angular_difference) ||
                (Math.Abs(ang) < Constants.zero_angular_difference) || parallel_planes)
            {
                if (offset1 != offset2)
                {
                    System.Exception e = new KojtoGlassDomesException(1, "Bends offset exception.");
                    throw e;
                }
                line2d L2 = new line2d(new complex(L1.A, L1.B) + cNode, cNode);

                complex inersectComplex = L11.IntersectWitch(L2);
                quaternion intersectPoint = ucs.ToACS(new quaternion(0, inersectComplex.real(), inersectComplex.imag(), 0));
                qlist.Add(intersectPoint);
            }
                #endregion
            else
                if (!variant)
                {
                    {

                        complex cThr = (cSec - cNode) * complex.polar(1.0, ang) + cNode;
                        line2d L2 = new line2d(cNode, cThr);

                        line2d L22 = new line2d(L2, -offset2);
                        L222 = L22;

                        complex inersectComplex = L11.IntersectWitch(L22);
                        quaternion intersectPoint = ucs.ToACS(new quaternion(0, inersectComplex.real(), inersectComplex.imag(), 0));
                        qlist.Add(intersectPoint);
                    }
                    //-------------------------------------------
                    #region convert coordinates
                    double iANG = (qFirst - qNode).angTo(ucs.FromACS(qlist[qlist.Count - 1]) - qNode);
                    double cANG = 0.0;
                    for (int k = 0; k < trArr.Count; k++)
                    {
                        Triangle tr = trArr[k];
                        Pair<int, int> pa = tr.GetOtherNodeNumers(nodeNumer);
                        quaternion q1 = Nodes[pa.First].Position - Nodes[nodeNumer].Position;
                        quaternion q2 = Nodes[pa.Second].Position - Nodes[nodeNumer].Position;
                        cANG += q1.angTo(q2);
                        bool b1 = false;
                        if (k > 0)
                        {
                            int ContiguousBendNumer = -1;
                            if (Triangle.IsContiguous(tr, trArr[k - 1], ref ContiguousBendNumer))
                            {
                                int firstNodeNUmer = (Bends[ContiguousBendNumer].StartNodeNumer == Nodes[nodeNumer].Numer) ?
                                    Bends[ContiguousBendNumer].EndNodeNumer : Bends[ContiguousBendNumer].StartNodeNumer;
                                b1 = pa.First == firstNodeNUmer;
                            }
                        }
                        else
                            b1 = (Nodes[pa.First].Position - qFirst).abs() < (Nodes[pa.Second].Position - qFirst).abs();
                        if (cANG > iANG)
                        {
                            if (k > 0)
                            {
                                quaternion Q1 = new quaternion();
                                quaternion Q2 = new quaternion();
                                if (b1)
                                {
                                    Q1 = Nodes[pa.First].Position;
                                    Q2 = Nodes[pa.Second].Position;
                                }
                                else
                                {
                                    Q2 = Nodes[pa.First].Position;
                                    Q1 = Nodes[pa.Second].Position;
                                }
                                quaternion M = (Q1 + Q2) / 2.0;
                                M = (M - Nodes[nodeNumer].Position) * 2.0 / 3.0 + Nodes[nodeNumer].Position;
                                UCS tUCS1 = new UCS(M, Nodes[nodeNumer].Position, Q1);
                                //--
                                double dist1 = (Q1 - Nodes[nodeNumer].Position).abs();
                                double dist2 = (Q2 - Nodes[nodeNumer].Position).abs();

                                complex cQ1 = (cSec - cNode) * complex.polar(1.0, cANG - q1.angTo(q2)) + cNode;
                                complex cQ2 = (cSec - cNode) * complex.polar(1.0, cANG) + cNode;

                                Q1 = new quaternion(0, cQ1.real(), cQ1.imag(), 0);
                                Q2 = new quaternion(0, cQ2.real(), cQ2.imag(), 0);
                                Q1 = ucs.ToACS(Q1); Q2 = ucs.ToACS(Q2);
                                Q1 -= Nodes[nodeNumer].Position; Q2 -= Nodes[nodeNumer].Position;
                                Q1 /= Q1.abs(); Q2 /= Q2.abs();
                                Q1 *= dist1; Q2 *= dist2;
                                Q1 += Nodes[nodeNumer].Position; Q2 += Nodes[nodeNumer].Position;

                                M = (Q1 + Q2) / 2.0;
                                M = (M - Nodes[nodeNumer].Position) * 2.0 / 3.0 + Nodes[nodeNumer].Position;
                                UCS tUCS2 = new UCS(M, Nodes[nodeNumer].Position, Q1);

                                qlist[qlist.Count - 1] = tUCS2.FromACS(qlist[qlist.Count - 1]);
                                qlist[qlist.Count - 1] = tUCS1.ToACS(qlist[qlist.Count - 1]);
                            }
                            break;
                        }
                    }
                    #endregion
                    //-------------------------------------------
                }
                else
                {
                    //MathLibKCAD.plane planeFirst = new MathLibKCAD.plane();
                    // MathLibKCAD.plane planeSecond = new MathLibKCAD.plane();

                    #region firstplane, first edge
                    quaternion normalFIRST = trArr[0].Normal.Second - trArr[0].Normal.First;
                    UCS tempUCS = new UCS(Bends[bendsNumers[0]].MidPoint, Nodes[nodeNumer].Position, Bends[bendsNumers[0]].MidPoint + normalFIRST);
                    if (tempUCS.FromACS(trArr[0].GetCentroid()).GetZ() < 0)
                    {
                        tempUCS = new UCS(Bends[bendsNumers[0]].MidPoint,
                            Nodes[nodeNumer].Position, Bends[bendsNumers[0]].MidPoint + trArr[0].Normal.First - trArr[0].Normal.Second);
                    }
                    planeFirst = new plane(tempUCS.ToACS(new quaternion(0, 0, 0, offset1)),
                        tempUCS.ToACS(new quaternion(0, 100, 0, offset1)), tempUCS.ToACS(new quaternion(0, 0, 100, offset1)));
                    Pair<quaternion, quaternion> firstEdge1 = new Pair<quaternion, quaternion>(tempUCS.ToACS(new quaternion(0, 0, 0, offset1)), tempUCS.ToACS(new quaternion(0, 100, 0, offset1)));
                    Pair<quaternion, quaternion> firstEdge2 = new Pair<quaternion, quaternion>(firstEdge1.First + normalFIRST * 100, firstEdge1.Second + normalFIRST * 100);
                    #endregion

                    #region secondplane, second edge
                    quaternion normalSECOND = trArr[trArr.Count - 1].Normal.Second - trArr[trArr.Count - 1].Normal.First;
                    UCS tempUCS1 = new UCS(Bends[bendsNumers[bendsNumers.Count - 1]].MidPoint,
                        Nodes[nodeNumer].Position, Bends[bendsNumers[bendsNumers.Count - 1]].MidPoint + normalSECOND);
                    if (tempUCS1.FromACS(trArr[trArr.Count - 1].GetCentroid()).GetZ() < 0)
                    {
                        tempUCS1 = new UCS(Bends[bendsNumers[bendsNumers.Count - 1]].MidPoint,
                            Nodes[nodeNumer].Position,
                            Bends[bendsNumers[bendsNumers.Count - 1]].MidPoint + trArr[trArr.Count - 1].Normal.First - trArr[trArr.Count - 1].Normal.Second);
                    }
                    planeSecond = new plane(tempUCS1.ToACS(new quaternion(0, 0, 0, offset2)),
                        tempUCS1.ToACS(new quaternion(0, 100, 0, offset2)), tempUCS1.ToACS(new quaternion(0, 0, 100, offset2)));

                    Pair<quaternion, quaternion> secondEdge1 = new Pair<quaternion, quaternion>(tempUCS1.ToACS(new quaternion(0, 0, 0, offset2)), tempUCS1.ToACS(new quaternion(0, 100, 0, offset2)));
                    Pair<quaternion, quaternion> secondEdge2 = new Pair<quaternion, quaternion>(secondEdge1.First + normalSECOND * 100, secondEdge1.Second + normalSECOND * 100);
                    #endregion

                    quaternion p1 = planeFirst.IntersectWithVector(secondEdge1.First, secondEdge1.Second);
                    quaternion p2 = planeFirst.IntersectWithVector(secondEdge2.First, secondEdge2.Second);

                    quaternion CP = new quaternion();
                    #region Namirame prese4nata to4ka na perifernite za regiona pryti ot tozi wyzel
                    //foreach (WorkClasses.Triangle tgr in trArr)

                    for (int k = 0; k < trArr.Count; k++)
                    {
                        Triangle tgr = trArr[k];
                        plane pl = new plane(tgr.Nodes.First, tgr.Nodes.Second, tgr.Nodes.Third);
                        CP = pl.IntersectWithVector(p1, p2);
                        if (tgr == CP)
                        {
                            intersect_triangle_INDEX = k;
                            break;
                        }
                    }
                    #endregion

                    qlist.Add(CP);
                }
            #endregion

            List<quaternion> currentLIST = new List<quaternion>();
            if (!variant)
            {
                #region intersect with fictive bends
                List<quaternion> buffer = new List<quaternion>();//check for replays 
                double intpANG = 0.0;// (qFirst - qNode).angTo(ucs.FromACS(qlist[qlist.Count - 1]) - qNode);
                //----
                foreach (Triangle tr in trArr)
                {
                    Pair<int, int> pa = tr.GetOtherNodeNumers(nodeNumer);
                    quaternion q1 = Nodes[pa.First].Position - Nodes[nodeNumer].Position;
                    quaternion q2 = Nodes[pa.Second].Position - Nodes[nodeNumer].Position;
                    if (tr == qlist[qlist.Count - 1])
                    {
                        intpANG += (q1.angTo(q2) / 2.0);
                        break;
                    }
                    else
                    {
                        intpANG += q1.angTo(q2);
                    }
                }
                //----

                //търсим всички пръти за които ъгъла е по-малък или равен на intpANG
                double currANG = 0.0;
                int currentBendIndex = 0; //indeksa w bendsNumer                   
                List<double> angles = new List<double>();
                angles.Add(intpANG);
                currentLIST.Add(qlist[qlist.Count - 1]);
                buffer.Add(qlist[qlist.Count - 1]);
                for (int k = 0; k < trArr.Count - 1; k++)
                {
                    Triangle tr = trArr[k];
                    Pair<int, int> pa = tr.GetOtherNodeNumers(nodeNumer);
                    quaternion q1 = Nodes[pa.First].Position - Nodes[nodeNumer].Position;
                    quaternion q2 = Nodes[pa.Second].Position - Nodes[nodeNumer].Position;
                    currANG += q1.angTo(q2);
                    currentBendIndex++;
                    complex cOther = (cSec - cNode) * complex.polar(1.0, currANG) + cNode;
                    line2d L2 = new line2d(cNode, cOther);
                    double distToNode = 0.0;
                    complex inersectComplex = new complex();

                    if (currANG < intpANG)
                        inersectComplex = L11.IntersectWitch(L2);
                    else
                        inersectComplex = L222.IntersectWitch(L2);

                    distToNode = (inersectComplex - cNode).abs();
                    Bend b = Bends[bendsNumers[currentBendIndex]];
                    quaternion qOther = (b.StartNodeNumer == nodeNumer) ? Nodes[b.EndNodeNumer].Position : Nodes[b.StartNodeNumer].Position;
                    quaternion qIntersect = qOther - Nodes[nodeNumer].Position;
                    qIntersect /= qIntersect.abs();
                    qIntersect *= distToNode;
                    qIntersect += Nodes[nodeNumer].Position;//*******

                    bool exist = false;
                    foreach (quaternion bq in buffer)
                    {
                        if ((bq - qIntersect).abs() < ConstantsAndSettings.GetMinDistBhetwenNodes())
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (!exist)
                    {
                        if (currANG < intpANG)
                        {
                            currentLIST.Insert(0, qIntersect);
                            angles.Insert(0, currANG);
                        }
                        else
                        {
                            currentLIST.Add(qIntersect);
                            angles.Add(currANG);
                        }
                    }

                    for (int i = 0; i < currentLIST.Count - 1; i++)
                    {
                        for (int j = i + 1; j < currentLIST.Count; j++)
                        {
                            if (angles[i] > angles[j])
                            {
                                double a = angles[i];
                                quaternion qa = currentLIST[i];
                                currentLIST[i] = currentLIST[j];
                                angles[i] = angles[j];
                                angles[j] = a;
                                currentLIST[j] = qa;
                            }
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region intersect with fictive bends
                currentLIST.Add(qlist[qlist.Count - 1]);
                List<int> buf = new List<int>();
                for (int k = 0; k < intersect_triangle_INDEX; k++)
                {
                    Triangle TR = trArr[k];
                    foreach (int bN in bendsNumers)
                    {
                        Bend bend = Bends[bN];
                        if (bend.IsFictive())
                        {
                            if (bend.FirstTriangleNumer == TR.Numer || bend.SecondTriangleNumer == TR.Numer)
                            {
                                if (buf.IndexOf(bend.Numer) < 0)
                                {
                                    quaternion cpp = planeFirst.IntersectWithVector(bend.Start, bend.End);
                                    currentLIST.Insert(currentLIST.Count - 1, cpp);
                                    buf.Add(bend.Numer);
                                }
                            }
                        }
                    }
                }

                try
                {
                    for (int k = intersect_triangle_INDEX; k < trArr.Count; k++)
                    {
                        Triangle TR = trArr[k];
                        foreach (int bN in bendsNumers)
                        {
                            Bend bend = Bends[bN];
                            if (bend.IsFictive())
                            {
                                if (bend.FirstTriangleNumer == TR.Numer || bend.SecondTriangleNumer == TR.Numer)
                                {
                                    if (buf.IndexOf(bend.Numer) < 0)
                                    {
                                        quaternion cpp = planeSecond.IntersectWithVector(bend.Start, bend.End);
                                        currentLIST.Add(cpp);
                                        buf.Add(bend.Numer);
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
                #endregion
            }

            return currentLIST;
        }
        public List<List<quaternion>> Get_Glass_Edge(Polygon POL, bool variant = false)
        {
            List<List<quaternion>> qlist1 = new List<List<quaternion>>();
            List<int> listNODES = POL.GetNodesNumersArray(ref Bends);

            #region nodes aligment
            for (int i = 0; i < listNODES.Count - 1; i++)
            {
                int j = i + 1;
                bool conected = false;
                do
                {
                    Pair<int, int> PA = new Pair<int, int>(listNODES[i], listNODES[j]);
                    foreach (int bN in POL.Bends_Numers_Array)
                    {
                        Bend bBend = Bends[bN];
                        if (bBend == PA)
                        {
                            conected = true;
                            break;
                        }
                    }
                    if (!conected)
                        j++;
                } while ((!conected) && (j < listNODES.Count - 1));
                if (j > i + 1)
                {
                    int vN = listNODES[i + 1];
                    listNODES[i + 1] = listNODES[j];
                    listNODES[j] = vN;
                }
            }
            #endregion

            foreach (int N in listNODES)
            {
                qlist1.Add(GetGlassEdgeForNodeInPolygon(N, POL, variant));
            }
            #region aligment
            List<Node> Numers = new List<Node>();
            foreach (List<quaternion> llist in qlist1)
            {
                quaternion qs = (llist[0] + llist[llist.Count - 1]) / 2.0;
                double d = 1000000;
                int numer = -1;
                List<int> nodeNumers = POL.GetNodesNumersArray(ref Bends);
                foreach (int N in nodeNumers)
                {
                    Node node = Nodes[N];
                    if ((node.Position - qs).abs() < d)
                    {
                        d = (node.Position - qs).abs();
                        numer = N;
                    }
                }
                Numers.Add(Nodes[numer]);
            }

            #region point from node is nearest to other node
            for (int i = 0; i < qlist1.Count; i++)
            {
                List<quaternion> LIST = qlist1[i];
                for (int j = 0; j < LIST.Count; j++)
                {
                    double dist = (Numers[i].Position - LIST[j]).abs();
                    for (int k = 0; k < qlist1.Count; k++)
                    {
                        if (k == i) { continue; }
                        try
                        {
                            double dist1 = (Numers[k].Position - LIST[j]).abs();
                            if (dist > dist1)
                            {
                                quaternion buff = LIST[j];
                                qlist1[i].RemoveAt(j);

                                if (qlist1[k].Count > 1)
                                    qlist1[k].Add(buff);

                                j--;

                            }
                        }
                        catch { }
                    }
                }
            }
            #endregion

            for (int i = 0; i < qlist1.Count; i++)
            {
                int j = (i == qlist1.Count - 1) ? 0 : i + 1;
                List<quaternion> LIST = qlist1[i];
                if (LIST.Count > 1)
                {

                    quaternion midle = Numers[j].Position - Numers[i].Position;

                    // double dist1 = (LIST[0] - midle).abs();
                    // double dist2 = (LIST[LIST.Count - 1] - midle).abs();
                    double dist1 = midle.angTo(LIST[0] - Numers[i].Position);
                    double dist2 = midle.angTo(LIST[1] - Numers[i].Position);
                    if (dist1 < dist2)
                        qlist1[i].Reverse();
                    else
                        if (Math.Abs(dist1 - dist2) < Constants.zero_angular_difference)
                        {
                            dist1 = (LIST[0] - Numers[i].Position).abs();
                            dist2 = (LIST[1] - Numers[i].Position).abs();
                            if (dist1 < dist2)
                                qlist1[i].Reverse();
                        }
                }
            }

            #endregion

            return qlist1;
        }
        public int IsBendConvex(Bend bend)
        {
            int rez = 0; // dwata triygylnika sa w edna rwnina
            double dist = (Triangles[bend.FirstTriangleNumer].Normal.First - bend.MidPoint).abs();
            // MathLibKCAD.quaternion normal2 = bend.Normal - bend.MidPoint;
            // normal2 *= 10.0;               
            double dist1 = ((Triangles[bend.FirstTriangleNumer].Normal.Second) - (bend.Normal)).abs();

            if (Math.Abs(dist1 - dist) > Constants.zero_dist)
            {
                if (dist1 > dist)
                    rez = 1; // convex (izpyknal)
                else
                    if (dist1 < dist)
                        rez = -1;//concave (wdlybnat)
            }

            return rez;
        }
    }
}