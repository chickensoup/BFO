using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;


namespace BFO
{
    /// <summary>
    /// 用于存储点对的结构
    /// </summary>
    public class SD
    {
        public int src;
        public int des;

        public SD()
        {
            this.src = 0;
            this.des = 0;
        }

        public SD(int src, int des)
        {
            this.src = src;
            this.des = des;
        }
    }



    class Program
    {
        public static MGraph a;
        public static int searchNode = 0;

        static void Main(string[] args)
        {
            int algtype = 0, filenum = 0;
            do
            {
                Console.WriteLine("number of files");
                filenum = Convert.ToInt32(Console.ReadLine());
            } while (filenum <= 0);

            while (true)
            {
                Console.WriteLine("1: Greedy search with orignal algorithm");
                Console.WriteLine("2: Greedy search with advanced algorithm");
                Console.WriteLine("3: Exhaust search with orignal algorithm");
                Console.WriteLine("4: Exhaust search with advanced algorithm");
                Console.WriteLine("5: Generate new graphs");
                Console.WriteLine("Choice: ");
                algtype = Convert.ToInt32(Console.ReadLine());

                String filename;

                if (algtype == 1)
                {
                    for (int i = 0; i < filenum; i++)
                    {
                        searchNode = 0;
                        filename = i.ToString() + ".txt";
                        testGreedy(filename, PrioAlg.orig);
                    }

                }
                else if (algtype == 2)
                {
                    for (int i = 0; i < filenum; i++)
                    {
                        searchNode = 0;
                        filename = i.ToString() + ".txt";
                        testGreedy(filename, PrioAlg.adv);
                    }

                }
                else if (algtype == 3)
                {
                    for (int i = 0; i < filenum; i++)
                    {
                        searchNode = 0;
                        filename = i.ToString() + ".txt";
                        testBF(filename, PrioAlg.orig);
                    }
                }
                else if (algtype == 4)
                {
                    for (int i = 0; i < filenum; i++)
                    {
                        searchNode = 0;
                        filename = i.ToString() + ".txt";
                        testBF(filename, PrioAlg.adv);
                    }
                }
                else if (algtype == 5)
                {
                    new GraphGenerator(filenum);
                }
                else
                    return;
            }


        }

        public static void testGreedy(String filename, PrioAlg prioalg)
        {
            String outputfile;
            Console.WriteLine("processing " + filename + "...");

            //Greedy Algorithm
            a = createMGraph(filename, prioalg);     //从文件读入数据

            if (a.prioAlg == PrioAlg.orig)
                outputfile = "GreedyOrig.txt";
            else
                outputfile = "GreedyAdv.txt";

            FileStream fs = new FileStream(outputfile, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);


            //测试RTM solution是否schedulable
            Console.WriteLine("测试RTM solution是否schedulable");
            a.mgrpahSetRTMBuffer();
            a.mgraphSetPrio();      //计算优先级
            a.mgraphResponseTime();     //计算resopnse time
            a.mgraphCalcLateness();     //计算lateness
            if (!a.mgraphSchedulable())
            {
                Console.WriteLine("Not schedulable, unable to perform mapping.");
                Console.ReadKey();
                return;
            }
            else
            {
                Console.WriteLine("RTM solution schedulable.");
            }

            //testPrintBuffer(a);
            //testPrintLateness(a);
            Console.WriteLine("RTM buffers = " + countBuffer(a));
            sw.WriteLine(filename);
            sw.WriteLine("RTM buffer: " + countBuffer(a));
            //Console.ReadKey();

            //计算Greedy Algorithm中得到的buffers上界
            Console.WriteLine("计算Greedy Algorithm中得到的buffers上界");
            a.mgrpahSetNoType2Buffer(); //不加type 2 buffer
            a.mgraphSetPrio();      //计算优先级
            a.mgraphResponseTime();     //计算resopnse time
            a.mgraphCalcLateness();     //计算lateness
            Console.WriteLine(GreedyTaskMapping());

            Console.WriteLine("Greedy buffers = " + countBuffer(a));
            sw.WriteLine("Greedy buffer: " + countBuffer(a));
            a.maxBuff = a.minBuff = countBuffer(a);
            
            sw.Flush();
            fs.Close();
        }




        public static void testBF(String filename, PrioAlg prioalg)
        {
            String outputfile;
            Console.WriteLine("processing " + filename + "...");
    
            //Greedy Algorithm
            a = createMGraph(filename, prioalg);     //从文件读入数据

            if (a.prioAlg == PrioAlg.orig)
                outputfile = "BForig.txt";
            else
                outputfile = "BFadv.txt";

            FileStream fs = new FileStream(outputfile, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);


            //测试RTM solution是否schedulable
            Console.WriteLine("测试RTM solution是否schedulable");
            a.mgrpahSetRTMBuffer();
            a.mgraphSetPrio();      //计算优先级
            a.mgraphResponseTime();     //计算resopnse time
            a.mgraphCalcLateness();     //计算lateness
            if (!a.mgraphSchedulable())
            {
                Console.WriteLine("Not schedulable, unable to perform mapping.");
                Console.ReadKey();
                return;
            }
            else
            {
                Console.WriteLine("RTM solution schedulable.");
                //Console.ReadKey();
            }

            Console.WriteLine("RTM buffers = " + countBuffer(a));
            sw.WriteLine(filename);
            sw.WriteLine("RTM buffer: " + countBuffer(a));
            //Console.ReadKey();

            //计算Greedy Algorithm中得到的buffers上界
            Console.WriteLine("计算Greedy Algorithm中得到的buffers上界");
            a.mgrpahSetNoType2Buffer(); //不加type 2 buffer
            a.mgraphSetPrio();      //计算优先级
            a.mgraphResponseTime();     //计算resopnse time
            a.mgraphCalcLateness();     //计算lateness
            Console.WriteLine(GreedyTaskMapping());

            //testPrintBuffer(a);
            //testPrintLateness(a);
            Console.WriteLine("Greedy buffers = " + countBuffer(a));
            sw.WriteLine("Greedy buffer: " + countBuffer(a));
            a.maxBuff = a.minBuff = countBuffer(a);
            // Console.ReadKey();

            //Optimization procedure
            Console.WriteLine("processing BBSearch on " + filename + "...");

            BBSearch();

            a.mgraphSetPrio();      //计算优先级
            a.mgraphResponseTime();     //计算resopnse time
            a.mgraphCalcLateness();     //计算lateness

            //testPrintBuffer(a);
            double lateness = testPrintLateness(a);

            int buffcount = countBuffer(a);
            Console.WriteLine("Total buffers = " + buffcount);
            Console.WriteLine("Searched nodes " + searchNode);
            //Console.ReadKey();

            //sw.WriteLine(lateness + " " + buffcount + " " + searchNode);
            sw.WriteLine(buffcount + " " + searchNode);
            Console.WriteLine(filename + ":" + buffcount + " " + searchNode);

            sw.Flush();
            fs.Close();
            // Console.ReadKey();
        }




        public static void BBSearch()
        {
            Console.WriteLine("BBSearch begins.");
            // Console.ReadKey();

            a.mgrpahSetNoType2Buffer(); //初始化设置，只设置全部的type 1 buffer但是不设置type 2 buffer
            a.mgraphSetPrio();      //计算优先级
            a.mgraphResponseTime();     //计算resopnse time
            a.mgraphCalcLateness();     //计算lateness                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  

            a.LSlist = new ArrayList();
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                for (int j = 0; j < a.mgraphVexnum; j++)
                {
                    if (a.arcs[i, j].arcAdj == 1 && a.arcs[i, j].arcBuf == 0)
                    {
                        LS lst = new LS(i, j, a);
                        a.LSlist.Add(lst);
                    }
                }
            }

            buffcostCompare bcc = new buffcostCompare();
            a.LSlist.Sort(bcc);

            VisitTree(a);

        }


        class buffcostCompare : System.Collections.IComparer
        {

            public int Compare(object x, object y)
            {
                return ((LS)x).buffcount - ((LS)y).buffcount;
            }
        }


        public static void VisitTree(MGraph s)
        {
            searchNode++;

            s.mgraphSetPrio();
            s.mgraphResponseTime();
            s.mgraphCalcLateness();

            if (s.mgraphSchedulable())
            {
                //TODO
                //LocalOptEvaluate();

                int currentBuff = countBuffer(s);
                if (currentBuff > s.maxBuff)
                    return;
                else if (currentBuff <= s.minBuff)
                {
                    s.minBuff = currentBuff;
                    a = copyMGraph(s);
                }
            }


            if (s.LSlist.Count != 0)
            {
                for (int i = 0; i < s.LSlist.Count; i++)
                {
                    MGraph t2 = copyMGraph(s);

                    LS t1 = (LS)t2.LSlist[i];
                    LS sp = new LS(t1.src, t1.des, t1.buffcount);

                    t2.arcs[sp.src, sp.des].arcBuf = 2;
                    t2.arcs[sp.des, sp.src].arcBuf = 2;

                    t2.LSlist.RemoveAt(i);
                    VisitTree(t2);
                }
            }
        }


        /*
        public static void VisitTree(ArrayList LSlist)
        {
            a.mgraphSetPrio();
            a.mgraphResponseTime();
            a.mgraphCalcLateness();

            if (a.mgraphSchedulable())
            {
                //TODO
                //LocalOptEvaluate();
                
                
                int currentBuff = countBuffer(a);
                if (currentBuff > a.maxBuff)
                    return;
                else if (currentBuff < a.minBuff)
                    a.minBuff = currentBuff;
            }

            buffcostCompare bcc = new buffcostCompare();
            LSlist.Sort(bcc);

            while (LSlist.Count != 0)
            {
                LS t = (LS)LSlist[0];
                LS sp = new LS(t.src, t.des, t.buffcount);

                a.arcs[sp.src, sp.des].arcBuf = 2;
                a.arcs[sp.des, sp.src].arcBuf = 2;
                

                LSlist.RemoveAt(0);
                VisitTree(LSlist);
            }
        }
         */


        public static int GreedyTaskMapping()
        {
            while (!a.mgraphSchedulable())
            {
                Console.WriteLine("Not schedulable");
                Console.WriteLine("Start to add type 2 buffer.");

                testPrintBuffer(a);

                int Fmax = maxLatenessBlock(a);
                List<SD> Ft2List = findInLinksLowRate(a, Fmax);
                if (Ft2List.Count == 0)
                    Ft2List = recFindInListLowRate(a, Fmax);

                SD Fscr = minLatenessSrcBlk(a, Ft2List);

                a.arcs[Fscr.src, Fscr.des].arcBuf = 2;
                a.arcs[Fscr.des, Fscr.src].arcBuf = 2;
                Console.WriteLine("Type 2 buffer block added between Point " + (Fscr.src + 1) + " and Point " + (Fscr.des + 1));

                a.mgraphSetPrio();
                a.mgraphResponseTime();
                a.mgraphCalcLateness();

                if (a.mgraphSchedulable())
                {
                    Console.WriteLine("\nSchedulable.");
                }
                else
                {
                    Console.WriteLine("\nNot Schedulable.");
                }
                Console.WriteLine("Press any key to continue...");
                //Console.ReadKey();
            }

            Console.WriteLine("\n\n==========**********==========");
            Console.WriteLine("==========**********==========");
            Console.WriteLine("This task set is schedulable now.");

            testPrintBuffer(a);
            testPrintLateness(a);

            return countBuffer(a);
        }





        /// <summary>
        /// 找到lateness最大的顶点
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static int maxLatenessBlock(MGraph a)
        {
            int id = 0;
            double lateness = 0;
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                if (lateness < a.vexs[i].pointLateness)
                {
                    id = i;
                    lateness = a.vexs[i].pointLateness;
                }
            }
            return id;
        }


        public static List<SD> findInLinksLowRate(MGraph a, int Fmax)
        {
            List<SD> list = new List<SD>(a.mgraphVexnum);
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                if (a.arcs[i, Fmax].arcAdj == 1 && a.arcs[i, Fmax].arcBuf == 0)     //可以添加type 2 buffer
                    list.Add(new SD(i, Fmax));
            }

            return list;
        }

        /// <summary>
        /// 找到可以添加type 2 buffer的点对
        /// </summary>
        /// <param name="a"></param>
        /// <param name="Fmax"></param>
        /// <returns></returns>
        public static List<SD> recFindInListLowRate(MGraph a, int Fmax)
        {
            List<SD> list = new List<SD>(a.mgraphVexnum);
            int index = Fmax;

            //如果找到Fmax的前驱可以添加type 2 buffer，直接返回这个配对
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                if (a.arcs[i, index].arcAdj == 1 && a.arcs[i, index].arcBuf == 0)
                {
                    Console.WriteLine("Type 2 buffer added between " + i + " and " + index + "\n");
                    list.Add(new SD(i, Fmax));
                    return list;
                }
            }

            //找不到，将Fmax的前驱放入pre list中
            List<int> pre = new List<int>(a.mgraphVexnum);
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                if (a.arcs[i, index].arcAdj == 1 && (a.arcs[i, index].arcBuf == 1 || a.arcs[i, index].arcBuf == 3))
                    pre.Add(i);
            }

            //广度搜索，直到找到可以放入的配对
            while (list.Count == 0)
            {
                for (int j = 0; j < pre.Count; j++)
                    recAddInListLowRate(a, pre[j], list);

                List<int> t = new List<int>(pre.Count);
                for (int i = 0; i < pre.Count; i++)
                    t.Add(pre[i]);


                for (int i = 0; i < t.Count; i++)
                {
                    for (int j = 0; j < a.mgraphVexnum; j++)
                    {
                        if (a.arcs[j, t[i]].arcAdj == 1 && (a.arcs[j, t[i]].arcBuf == 1 || a.arcs[j, t[i]].arcBuf == 3))
                            pre.Add(j);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 将与Fmax点相连的（i, Fmax）且能添加type 2 buffer的点对添加到list中
        /// </summary>
        /// <param name="a"></param>
        /// <param name="Fmax"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static void recAddInListLowRate(MGraph a, int Fmax, List<SD> list)
        {
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                if (a.arcs[i, Fmax].arcAdj == 1 && a.arcs[i, Fmax].arcBuf == 0)
                {
                    list.Add(new SD(i, Fmax));
                }
            }


        }

        /// <summary>
        /// 如果存在多个可以添加type 2 buffer的点对，选择那个SRC点的lateness最小的点对
        /// </summary>
        /// <param name="a"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static SD minLatenessSrcBlk(MGraph a, List<SD> list)
        {
            if (list.Count == 1)
                return list[0];

            int id = 0;
            double lateness = a.vexs[list[0].src].pointLateness;
            for (int i = 1; i < list.Count; i++)
            {
                double t = a.vexs[list[i].src].pointLateness;
                if (t < lateness)
                {
                    id = i;
                    lateness = t;
                }
            }

            return list[id];
        }

        /// <summary>
        /// 复制一个MGraph
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static MGraph copyMGraph(MGraph s)
        {
            int vexnum = s.mgraphVexnum;
            int arcnum = s.mgraphArcnum;
            MGraph b = new MGraph(s.mgraphVexnum, s.mgraphArcnum);
            for (int i = 0; i < vexnum; i++)
            {
                b.mgraphSetPointArgs(i, s.vexs[i].pointPeriod, s.vexs[i].pointComptime);

                for (int j = 0; j < vexnum; j++)
                {
                    b.arcs[i, j] = new ArcCell(s.arcs[i, j].arcAdj, s.arcs[i, j].arcBuf, s.arcs[i, j].arcAdjCount);
                }
            }


            b.LSlist = new ArrayList();
            for (int i = 0; i < s.LSlist.Count; i++)
            {
                LS t = (LS)s.LSlist[i];
                LS sp = new LS(t.src, t.des, t.buffcount);
                b.LSlist.Add(sp);
            }

            b.maxBuff = s.maxBuff;
            b.minBuff = s.minBuff;
            b.prioAlg = s.prioAlg;

            return b;
        }




        /// <summary>
        /// 从文件读入参数构建MGraph
        /// </summary>
        /// <returns></returns>
        public static MGraph createMGraph(String filename, PrioAlg prioalg)
        {
            Console.WriteLine("Starting reading the graph parameter...");
            StreamReader sr = new StreamReader(filename);
            String line;

            //first line: read vexnum & arcnum
            line = sr.ReadLine();
            String[] temp = new String[2];
            temp = line.Split(' ');
            MGraph a = new MGraph(Int32.Parse(temp[0]), Int32.Parse(temp[1]));

            //next vexnum lines: read vexs info
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                line = sr.ReadLine();
                temp = line.Split(' ');
                a.mgraphSetPointArgs(i, Int32.Parse(temp[0]), Double.Parse(temp[1]));

            }

            //next vexnum lines: read adj matrix
            String[,] temp2 = new String[a.mgraphVexnum, a.mgraphVexnum];
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                line = sr.ReadLine();
                String[] temp3 = new String[a.mgraphVexnum];
                temp3 = line.Split(' ');
                for (int j = 0; j < a.mgraphVexnum; j++)
                    temp2[i, j] = temp3[j];
            }
            a.mgraphSetAdjMatrix(a.mgraphVexnum, temp2);

            String[,] temp4 = new String[a.mgraphVexnum, a.mgraphVexnum];
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                line = sr.ReadLine();
                String[] temp5 = new String[a.mgraphVexnum];
                temp5 = line.Split(' ');
                for (int j = 0; j < a.mgraphVexnum; j++)
                    temp4[i, j] = temp5[j];
            }
            a.mgraphSetAdjCountMatirx(a.mgraphVexnum, temp4);
            a.prioAlg = prioalg;


            sr.Close();

            Console.WriteLine("Graph paremeters read complete.\n");
            return a;
        }


        /// <summary>
        /// 打印读入的图信息
        /// </summary>
        /// <param name="s"></param>
        public static void testPrint(MGraph s)
        {
            Console.WriteLine("This graph contains " + s.mgraphVexnum + " vertexs and " + s.mgraphArcnum + "arcs\n");
            for (int i = 0; i < s.mgraphVexnum; i++)
            {
                Point t = s.vexs[i];
                Console.WriteLine("Point No. " + (t.pointID + 1) + ", period " + t.pointPeriod + ", compute time " + t.pointComptime);
            }

            Console.WriteLine("Arcs matrix:");
            for (int i = 0; i < s.mgraphVexnum; i++)
            {
                for (int j = 0; j < s.mgraphVexnum; j++)
                {
                    Console.Write(s.arcs[i, j].arcAdj + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("==========**********==========");
        }

        /// <summary>
        /// 打印顶点的response time和Priority
        /// </summary>
        /// <param name="s"></param>
        public static void testPrintPrio(MGraph s)
        {
            Console.WriteLine("\nPrint graph.");
            for (int i = 0; i < s.mgraphVexnum; i++)
            {
                Console.WriteLine("Point " + s.vexs[i].pointID + ": Priority: " + s.vexs[i].pointPrio + " Response Time:" + s.vexs[i].pointResopnsetime);
            }
        }

        /// <summary>
        /// 打印buffers矩阵
        /// </summary>
        /// <param name="s"></param>
        public static void testPrintBuffer(MGraph s)
        {
            Console.WriteLine("Buffers matrix:\nArcs:");
            for (int i = 0; i < s.mgraphVexnum; i++)
            {
                for (int j = 0; j < s.mgraphVexnum; j++)
                {
                    Console.Write(s.arcs[i, j].arcAdj + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nBuffers:");
            for (int i = 0; i < s.mgraphVexnum; i++)
            {
                for (int j = 0; j < s.mgraphVexnum; j++)
                {
                    Console.Write(s.arcs[i, j].arcBuf + " ");
                }
                Console.WriteLine();
            }

            /*
            Console.WriteLine("\nbuffer counts between vex:");
            for (int i = 0; i < s.mgraphVexnum; i++)
            {
                for (int j = 0; j < s.mgraphVexnum; j++)
                {
                    Console.Write(s.arcs[i, j].arcAdjCount + " ");
                }
                Console.WriteLine();
            }
             * */

            Console.WriteLine("==========**********==========\n");

        }

        /// <summary>
        /// 打印最终信息
        /// </summary>
        /// <param name="a"></param>
        public static double testPrintLateness(MGraph a)
        {
            Console.WriteLine("\n==========**********==========");
            double lateness = 0;
            for (int i = 0; i < a.mgraphVexnum; i++)
            {
                Console.WriteLine("Point " + (i + 1) + " Period " + a.vexs[i].pointPeriod + ", Priority " + (a.vexs[i].pointPrio + 1) + ", response time " + Math.Round(a.vexs[i].pointResopnsetime, 3) + ", lateness " + Math.Round(a.vexs[i].pointLateness, 3));
                lateness += a.vexs[i].pointLateness;
            }
            Console.WriteLine("==========**********==========\n");
            Console.WriteLine("lateness = " + Math.Round((-lateness), 3));
            return -lateness;
        }

        /// <summary>
        /// 计算总buffer数量
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static int countBuffer(MGraph a)
        {
            int buffCount = 0;
            for (int i = 1; i < a.mgraphVexnum; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    //if (a.arcs[i, j].arcBuf == 1)
                    //    buffCount += a.arcs[i, j].arcAdjCount;
                    //else if (a.arcs[i, j].arcBuf == 2)
                    if (a.arcs[i, j].arcBuf == 2)
                        buffCount += 2 * a.arcs[i, j].arcAdjCount;
                }
            }
            return buffCount;
        }

    }


}


