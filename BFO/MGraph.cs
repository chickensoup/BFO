using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


namespace BFO
{

    /// <summary>
    /// 用于计算response time的TC类，T为block的周期，C为block的compute time
    /// </summary>
    public class TC
    {
        public int T;
        public double C;

        public TC()
        {
            this.T = 0;
            this.C = 0.0;
        }

        public TC(int t, double c)
        {
            this.T = t;
            this.C = c;
        }
    }

    /// <summary>
    /// 动态优化所需，worseTime记录计算时间，utilization记录utilization factor
    /// </summary>
    public class LU
    {
        public double worseTime;
        public double utilization;
        public int k_index;  //链路尾节点的index
        public int minT_index;  //链路中周期最短的节点index

        public LU()
        {
            worseTime = 0;
            utilization = 0;
            k_index = -1;
            minT_index = -1;
        }

        public LU(double a, double b)
        {
            worseTime = a;
            utilization = b;
        }
    }

    public class LS
    {
        public int src;
        public int des;
        public int buffcount;

        public LS(int src, int des, MGraph a)
        {
            this.src = src;
            this.des = des;
            this.buffcount = a.arcs[src, des].arcAdjCount;
        }

        public LS(int src, int des, int buffcount)
        {
            this.src = src;
            this.des = des;
            this.buffcount = buffcount;
        }
    }

    public enum PrioAlg { adv, orig };    //



    /// <summary>
    /// graph class
    /// </summary>
    public class MGraph
    {
        public PrioAlg prioAlg;
        public Point[] vexs;       //顶点向量
        public ArcCell[,] arcs;         //邻接矩阵
        public int vexnum, arcnum;      //图的当前顶点数和弧数

        public int maxBuff, minBuff;

        public ArrayList LSlist;

        public MGraph(int vexnum, int arcnum)
        {
            this.arcnum = arcnum;
            this.vexnum = vexnum;

            vexs = new Point[vexnum];
            arcs = new ArcCell[vexnum, vexnum];

        }

        public int mgraphVexnum
        {
            get { return this.vexnum; }
        }

        public int mgraphArcnum
        {
            get { return this.arcnum; }
        }

        public void mgraphSetPointArgs(int index, int period, double comptime)
        {
            this.vexs[index] = new Point(index, period, comptime);
        }

        public void mgraphSetAdjMatrix(int vexnum, String[,] adj)
        {
            for (int i = 0; i < vexnum; i++)
            {
                for (int j = 0; j < vexnum; j++)
                {
                    this.arcs[i, j] = new ArcCell(Int32.Parse(adj[i, j]));
                }
            }

        }

        public void mgraphSetAdjCountMatirx(int vexnum, String[,] adjCount)
        {
            for (int i = 0; i < vexnum; i++)
            {
                for (int j = 0; j < vexnum; j++)
                {
                    this.arcs[i, j].arcAdjCount = Int32.Parse(adjCount[i, j]);
                }
            }
        }

        public void mgraphInitPrio()
        {
            for (int i = 0; i < this.mgraphVexnum; i++)
            {
                this.vexs[i].pointPrio = 0;
            }
        }

        /// <summary>
        /// 添加type1 buffer， adjBuf = 1
        /// 可以添加type2 buffer的地方，adjBuf = 0，在添加type2 buffer之后adjBuf = 2
        /// 相同频率元件之间 adjBuf = 3
        /// 不存在连线， adjBuf = -1
        /// </summary>
        public void mgrpahSetNoType2Buffer()
        {
            for (int i = 0; i < this.mgraphVexnum; i++)
            {
                for (int j = 0; j < this.mgraphVexnum; j++)
                {
                    //i == j
                    if (i == j)
                        this.arcs[i, j].arcBuf = -1;
                    //存在正向连线
                    else if (this.arcs[i, j].arcAdj == 1)
                    {
                        if (this.vexs[i].pointPeriod < this.vexs[j].pointPeriod)
                            this.arcs[i, j].arcBuf = 1;     //type 1 buffer
                        else if (this.vexs[i].pointPeriod == this.vexs[j].pointPeriod)
                            this.arcs[i, j].arcBuf = 3;     //周期相同，不用加buffer， 用arcBuff = 3表示
                        else
                            this.arcs[i, j].arcBuf = 0;     //可以添加type 2 buffer，暂时不加
                    }
                    //存在反向连线
                    else if (this.arcs[i, j].arcAdj == -1)
                    {
                        if (this.vexs[i].pointPeriod > this.vexs[j].pointPeriod)
                            this.arcs[i, j].arcBuf = 1;     //type 1 buffer
                        else if (this.vexs[i].pointPeriod == this.vexs[j].pointPeriod)
                            this.arcs[i, j].arcBuf = 3;     //周期相同，不用加buffer， 用arcBuff = 3表示
                        else
                            this.arcs[i, j].arcBuf = 0;
                    }
                    //不存在连线
                    else
                        this.arcs[i, j].arcBuf = -1;
                }
            }
        }

        /// <summary>
        /// 添加type1 buffer， adjBuf = 1
        /// 可以添加type2 buffer的地方，作为RTM solution则都添加上type 2 buffer， adjBuf = 2
        /// 相同频率元件之间 adjBuf = 3
        /// 不存在连线， adjBuf = -1
        /// </summary>
        public void mgrpahSetRTMBuffer()
        {
            for (int i = 0; i < this.mgraphVexnum; i++)
            {
                for (int j = 0; j < this.mgraphVexnum; j++)
                {
                    //i == j
                    if (i == j)
                        this.arcs[i, j].arcBuf = -1;
                    //存在正向连线
                    else if (this.arcs[i, j].arcAdj == 1)
                    {
                        if (this.vexs[i].pointPeriod < this.vexs[j].pointPeriod)
                            this.arcs[i, j].arcBuf = 1;     //type 1 buffer
                        else if (this.vexs[i].pointPeriod == this.vexs[j].pointPeriod)
                            this.arcs[i, j].arcBuf = 3;     //周期相同，不用加buffer， 用arcBuff = 3表示
                        else
                            this.arcs[i, j].arcBuf = 2;     //可以添加type 2 buffer
                    }
                    //存在反向连线
                    else if (this.arcs[i, j].arcAdj == -1)
                    {
                        if (this.vexs[i].pointPeriod > this.vexs[j].pointPeriod)
                            this.arcs[i, j].arcBuf = 1;     //type 1 buffer
                        else if (this.vexs[i].pointPeriod == this.vexs[j].pointPeriod)
                            this.arcs[i, j].arcBuf = 3;     //周期相同，不用加buffer， 用arcBuff = 3表示
                        else
                            this.arcs[i, j].arcBuf = 2;
                    }
                    //不存在连线
                    else
                        this.arcs[i, j].arcBuf = -1;
                }
            }
        }


        /// <summary>
        /// 设置优先级
        /// </summary>
        public void mgraphSetPrio()
        {
            //Console.WriteLine("Starting priority assignment...");

            int vexnum = this.mgraphVexnum;
            ArrayList T = new ArrayList(vexnum);      //所有点的集合
            ArrayList S = new ArrayList(vexnum);      //所有当前可到达的点的集合
            int[,] arc = new int[vexnum, vexnum];
            int[,] buf = new int[vexnum, vexnum];
            bool[] visited = new bool[vexnum];
            int PRIORITY = 0;

            //initialize
            for (int i = 0; i < vexnum; i++)
            {
                T.Add(this.vexs[i]);
                visited[i] = false;
                this.vexs[i].priority = -1;

                for (int j = 0; j < vexnum; j++)
                {
                    arc[i, j] = this.arcs[i, j].arcAdj;
                    buf[i, j] = this.arcs[i, j].arcBuf;
                }
            }


            //开始priority assignment
            while (setPrioFinished(visited) == false)
            {
                //Console.ReadKey();


                //Console.WriteLine("\n" + "存在未分配的点，开始分配优先级。");
                /*
                Console.WriteLine("未分配优先级的点为：");
                for (int k = 0; k < this.vexnum; k++)
                {
                    if (visited[k] == false)
                        Console.WriteLine("Point " + (k + 1));
                }
                 * */

                //Console.WriteLine("将所有没有入度的点放入S");
                //将所有没有入度的点放入S
                for (int i = 0; i < vexnum; i++)
                {
                    //如果已经访问过，跳过
                    if (visited[i])
                        continue;

                    //如果已经在S集中则跳过
                    bool sExist = false;
                    for (int k = 0; k < S.Count; k++)
                    {
                        if (((Point)S[k]).pointID == i)
                        {
                            sExist = true;
                            break;
                        }
                    }
                    if (sExist)
                        continue;

                    bool candidate = true;
                    for (int j = 0; j < vexnum; j++)
                    {
                        //已经去掉的点，不再考虑边
                        if (visited[j])
                            continue;

                        //如果存在连线-1且buf != 2，说明存在入度
                        if (arc[i, j] == -1 && buf[i, j] != 2)
                        {
                            candidate = false;
                            break;
                        }
                    }//end of for
                    if (candidate == true)
                        S.Add(this.vexs[i]);
                }//end of for
                //Console.WriteLine("S集合中的候选点为：");
                /*
                for (int i = 0; i < S.Count; i++)
                {
                    //Console.WriteLine("Point " + (((Point)S[i]).pointID + 1));
                }
                 */


                //Console.WriteLine("找出S集合中下一个要分配优先级的点。");
                //找出S集合中的候选点
                if (S.Count == 1)
                {
                    int id = ((Point)S[0]).pointID;
                    this.vexs[id].pointPrio = PRIORITY;
                    PRIORITY++;
                    visited[id] = true;

                    //Console.WriteLine("S集合中只有一个顶点" + (id + 1) + "，分配优先级" + PRIORITY);
                    S.RemoveAt(0);
                }
                else
                {
                    //Console.WriteLine("S集合中有" + S.Count + "个顶点");

                    //按照period升序排列，频率最高的排在前面
                    S.Sort(0, S.Count, new ListCompare());

                    //选择相同周期的前几个点
                    int currentPeriod = ((Point)S[0]).pointPeriod;
                    int count = 1;

                    for (int i = 1; i < S.Count; i++)
                    {
                        if (((Point)S[i]).pointPeriod == currentPeriod)
                            count++;
                    }

                    if (count == 1)
                    {
                        int id = ((Point)S[0]).pointID;
                        this.vexs[id].pointPrio = PRIORITY;
                        PRIORITY++;
                        visited[id] = true;

                        //Console.WriteLine("S集合中顶点" + (id + 1) + "，分配优先级" + PRIORITY);
                        S.RemoveAt(0);
                    }
                    //若存在多个点周期相同，就要比较laxity
                    else
                    {
                        double MINLaxity = 0;
                        int MINIndex = ((Point)S[0]).pointID;
                        int sIndex = 0;
                        for (int i = 0; i < count; i++)
                        {
                            List<int> succ = new List<int>();
                            int currentIndex = ((Point)S[i]).pointID;

                            //succ(i)
                            succ.Add(currentIndex);

                            
                            for (int j = 0; j < vexnum; j++)
                            {
                                if (arc[currentIndex, j] == 1)
                                    succ.Add(j);
                            }

                            double minLaxity = 0;
                            for (int j = 0; j < succ.Count; j++)
                            {
                                double worseTime, laxity;
                                
                                if (this.prioAlg == PrioAlg.orig)
                                {
                                    worseTime = maxPathCompTime(succ[j]);
                                    laxity = this.vexs[succ[j]].pointPeriod - worseTime;
                                }
                                else
                                {
                                    worseTime = calcSuccResopnseTime(succ[j], currentIndex);
                                    laxity = 1 - worseTime / this.vexs[succ[j]].pointPeriod;
                                    
                                    
                                    //worseTime = maxPathCompTimeAdv(succ[j], this.vexs[succ[j]].pointPeriod);
                                    //laxity = 1 - worseTime / this.vexs[succ[j]].pointPeriod;
                                }
                                
                                
                                if (j == 0)
                                    minLaxity = laxity;
                                else
                                {
                                    if (laxity < minLaxity)
                                        minLaxity = laxity;
                                }
                            }

                            if (i == 0)
                            {
                                MINIndex = currentIndex;
                                sIndex = i;
                                MINLaxity = minLaxity;
                            }
                            else
                            {
                                if (minLaxity < MINLaxity)
                                {
                                    MINLaxity = minLaxity;
                                    MINIndex = currentIndex;
                                    sIndex = i;
                                }
                            }

                            succ.Clear();

                        }//end of for

                        int id = MINIndex;
                        this.vexs[id].pointPrio = PRIORITY;
                        PRIORITY++;
                        visited[id] = true;

                        //Console.WriteLine("S集合中顶点" + (id + 1) + "，分配优先级" + PRIORITY);
                        S.RemoveAt(sIndex);
                    }//end of else
                }
            }

            //Console.WriteLine("Priority assignment complete！\n");
            //Console.WriteLine("==========**********==========");
        }


        /// <summary>
        /// 判断优先级设置是否完成
        /// </summary>
        /// <param name="visited"></param>
        /// <returns></returns>
        public bool setPrioFinished(bool[] visited)
        {
            int tCount = 0;
            for (int i = 0; i < vexnum; i++)
            {
                if (visited[i])
                    tCount++;
            }

            if (tCount == vexnum)
                return true;
            else
                return false;
        }


        /// <summary>
        /// 设置优先级,考虑动态优化过程
        /// </summary>
        public void mgraphSetPrio_DynamicOpt()
        {
            //Console.WriteLine("Starting priority assignment...");

            int vexnum = this.mgraphVexnum;
            ArrayList T = new ArrayList(vexnum);      //所有点的集合
            ArrayList S = new ArrayList(vexnum);      //所有当前可到达的点的集合
            int[,] arc = new int[vexnum, vexnum];
            int[,] buf = new int[vexnum, vexnum];
            bool[] visited = new bool[vexnum];
            int PRIORITY = 0;

            //initialize
            for (int i = 0; i < vexnum; i++)
            {
                T.Add(this.vexs[i]);
                visited[i] = false;
                this.vexs[i].priority = -1;

                for (int j = 0; j < vexnum; j++)
                {
                    arc[i, j] = this.arcs[i, j].arcAdj;
                    buf[i, j] = this.arcs[i, j].arcBuf;
                }
            }


            //开始priority assignment
            while (setPrioFinished(visited) == false)
            {
                //Console.ReadKey();

                //Console.WriteLine("将所有没有入度的点放入S");
                //将所有没有入度的点放入S
                for (int i = 0; i < vexnum; i++)
                {
                    //如果已经访问过，跳过
                    if (visited[i])
                        continue;

                    //如果已经在S集中则跳过
                    bool sExist = false;
                    for (int k = 0; k < S.Count; k++)
                    {
                        if (((Point)S[k]).pointID == i)
                        {
                            sExist = true;
                            break;
                        }
                    }
                    if (sExist)
                        continue;

                    bool candidate = true;
                    for (int j = 0; j < vexnum; j++)
                    {
                        //已经去掉的点，不再考虑边
                        if (visited[j])
                            continue;

                        //如果存在连线-1且buf != 2，说明存在入度
                        if (arc[i, j] == -1 && buf[i, j] != 2)
                        {
                            candidate = false;
                            break;
                        }
                    }//end of for
                    if (candidate == true)
                        S.Add(this.vexs[i]);
                }//end of for
                //Console.WriteLine("S集合中的候选点为：");


                //Console.WriteLine("找出S集合中下一个要分配优先级的点。");
                //找出S集合中的候选点
                if (S.Count == 1)
                {
                    int id = ((Point)S[0]).pointID;
                    //Console.WriteLine("S集合中只有一个顶点" + (id + 1));
                    checkAndOpt_Dyn(id);    //检测是否需要动态优化，并进行适当调整

                    this.vexs[id].pointPrio = PRIORITY;
                    PRIORITY++;
                    visited[id] = true;

                    ///Console.WriteLine("S集合中只有一个顶点" + (id + 1) + "，分配优先级" + PRIORITY);
                    S.RemoveAt(0);
                }
                else
                {
                    //Console.WriteLine("S集合中有" + S.Count + "个顶点");

                    //按照period升序排列，频率最高的排在前面
                    S.Sort(0, S.Count, new ListCompare());

                    //选择相同周期的前几个点
                    int currentPeriod = ((Point)S[0]).pointPeriod;
                    int count = 1;

                    for (int i = 1; i < S.Count; i++)
                    {
                        if (((Point)S[i]).pointPeriod == currentPeriod)
                            count++;
                    }

                    //只有一个点，检查这个点的laxity是否满足laxity >= u；若不满足，则动态调整
                    if (count == 1)
                    {
                        int id = ((Point)S[0]).pointID;
                        //Console.WriteLine("S集合中顶点" + (id + 1));
                        checkAndOpt_Dyn(id);    //检测是否需要动态优化，并进行适当调整

                        this.vexs[id].pointPrio = PRIORITY;
                        PRIORITY++;
                        visited[id] = true;

                        //Console.WriteLine("S集合中顶点" + (id + 1) + "，分配优先级" + PRIORITY);
                        S.RemoveAt(0);
                    }
                    //若存在多个点周期相同，就要比较laxity
                    else
                    {
                        double MINLaxity = 0;
                        int MINIndex = ((Point)S[0]).pointID;
                        int sIndex = 0;
                        for (int i = 0; i < count; i++)
                        {
                            List<int> succ = new List<int>();
                            int currentIndex = ((Point)S[i]).pointID;

                            //succ(i)
                            succ.Add(currentIndex);
                            for (int j = 0; j < vexnum; j++)
                            {
                                if (arc[currentIndex, j] == 1)
                                    succ.Add(j);
                            }

                            double minLaxity = 0;
                            for (int j = 0; j < succ.Count; j++)
                            {
                                double worseTime, laxity;

                                if (this.prioAlg == PrioAlg.orig)
                                {
                                    worseTime = maxPathCompTime(succ[j]);
                                    laxity = this.vexs[succ[j]].pointPeriod - worseTime;
                                }
                                else
                                {
                                    worseTime = calcSuccResopnseTime(succ[j], currentIndex);
                                    laxity = 1 - worseTime / this.vexs[succ[j]].pointPeriod;


                                    //worseTime = maxPathCompTimeAdv(succ[j], this.vexs[succ[j]].pointPeriod);
                                    //laxity = 1 - worseTime / this.vexs[succ[j]].pointPeriod;
                                }


                                if (j == 0)
                                    minLaxity = laxity;
                                else
                                {
                                    if (laxity < minLaxity)
                                        minLaxity = laxity;
                                }
                            }

                            if (i == 0)
                            {
                                MINIndex = currentIndex;
                                sIndex = i;
                                MINLaxity = minLaxity;
                            }
                            else
                            {
                                if (minLaxity < MINLaxity)
                                {
                                    MINLaxity = minLaxity;
                                    MINIndex = currentIndex;
                                    sIndex = i;
                                }
                            }

                            succ.Clear();

                        }//end of for

                        int id = MINIndex;
                        checkAndOpt_Dyn(id);
                        this.vexs[id].pointPrio = PRIORITY;
                        PRIORITY++;
                        visited[id] = true;

                        //Console.WriteLine("S集合中顶点" + (id + 1) + "，分配优先级" + PRIORITY);
                        S.RemoveAt(sIndex);
                    }//end of else
                }
            }
        }


        public LU utilizationCompTime(int index, int tk)
        {
            LU lu = new LU();
            
            bool noIncoming = true;
            List<int> candi = new List<int>(vexnum);
            for (int i = 0; i < vexnum; i++)
            {
                if (this.arcs[i, index].arcAdj == 1)
                {
                    noIncoming = false;
                    candi.Add(i);
                }
            }

            if (noIncoming)
            {
                lu.worseTime = this.vexs[index].pointComptime;
                lu.utilization = lu.worseTime / this.vexs[index].pointPeriod;
                lu.k_index = lu.minT_index = index;
                
                return lu;
            }
            else
            {
                LU minLU = new LU(0, 0);
                LU tLU = new LU();

                double tTime = 0;
                double tU = 0;
                for (int i = 0; i < candi.Count; i++)
                {
                    tTime = tk / (this.vexs[candi[i]].pointPeriod) * this.vexs[index].pointComptime + utilizationCompTime(candi[i], tk).worseTime;
                    tU = utilizationCompTime(candi[i], tk).utilization + this.vexs[candi[i]].pointComptime / this.vexs[candi[i]].pointPeriod;
                    tLU.worseTime = tTime;
                    tLU.utilization = tU;
                    tLU.k_index = index;
                    tLU.minT_index = (this.vexs[candi[i]].pointPeriod > this.vexs[index].pointPeriod) ? index : candi[i];

                    if (tLU.worseTime > minLU.worseTime)
                    {
                        minLU.worseTime = tLU.worseTime;
                        minLU.utilization = tLU.utilization;
                        minLU.minT_index = tLU.minT_index;
                        minLU.k_index = tLU.k_index;
                    }
                }

                return minLU;
            }

        }


        /// <summary>
        /// 给定点，检测是否需要进行动态优化，并进行频率的动态调整
        /// </summary>
        /// <param name="index"></param>
        public void checkAndOpt_Dyn(int index)
        {
            LU lu = new LU();

            //succ(i)
            while(true)
            {
                lu = utilizationCompTime(index, this.vexs[index].pointPeriod);
                double laxity = 1 - lu.worseTime / this.vexs[index].pointPeriod;

                if (laxity >= lu.utilization || lu.k_index == lu.minT_index)
                    break;

                Console.WriteLine("laxity=" + laxity + " U=" + lu.utilization + " minT_index=" + lu.minT_index + 1 + " k_index=" + lu.k_index + 1);
                Console.WriteLine("check for point " + (index + 1));
                Console.WriteLine((lu.minT_index + 1) + "频率为" + this.vexs[lu.minT_index].pointPeriod + "动态调整加倍");
                Console.ReadKey();
                this.vexs[lu.minT_index].pointPeriod *= 2;
                MGraphUpdate(lu.minT_index);
            }
        }

        /// <summary>
        /// 动态调整频率之后，需要更新arcs数组
        /// </summary>
        /// <param name="index"></param>
        public void MGraphUpdate(int index)
        {
            int vexnum = this.mgraphVexnum;
            for (int i = 0; i < vexnum; i++)
            {
                if (this.arcs[i, index].arcAdj == 1)
                {
                    int index_t = this.vexs[index].pointPeriod;
                    int i_t = this.vexs[i].pointPeriod;
                    if (index_t == i_t)
                    {
                        this.arcs[i, index].arcBuf = 3;
                        this.arcs[index, i].arcBuf = 3;
                    }
                }
            }
        }






        /// <summary>
        /// 根据优先级计算id为index的点的response time,如果index为后继节点则需要把orig节点也算进来
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double calcSuccResopnseTime(int index, int orig)
        {
            double Cj = this.vexs[index].pointComptime;
            ArrayList pre = new ArrayList(vexnum);
            int preCount = 0;
            double mincost = 0;
            int cost = 0;


            for (int i = 0; i < vexnum; i++)
            {
                if (this.vexs[i].pointPrio != -1)
                {
                    TC p = new TC(this.vexs[i].pointPeriod, this.vexs[i].pointComptime);
                    pre.Add(p);
                    mincost += p.C;
                    preCount++;
                }
            }

            if (orig != index)
            {
                TC p = new TC(this.vexs[orig].pointPeriod, this.vexs[orig].pointComptime);
                pre.Add(p);
                mincost += p.C;
                preCount++;
            }

            if (pre.Count == 0)
            {
                return Cj;
            }

            pre.Sort(0, pre.Count, new TCCompare());

            for (cost = 0; ; cost++)
            {
                double a = cost;
                double b1 = 0.0;
                double b2 = 0.0;
                for (int k = 0; k < pre.Count; k++)
                {
                    TC p = (TC)pre[k];
                    b1 += ceiling((double)cost, p.T) * p.C;
                    b2 += ceiling((double)cost, p.T) * p.C;
                }

                b1 += Cj;
                b2 += Cj;

                if (b2 <= a)
                    break;
            }

            double Rj = Cj;
            for (int i = 0; i < pre.Count; i++)
            {
                TC p = (TC)pre[i];
                Rj += ceiling((double)cost, p.T) * p.C;
            }

            return Rj;
        }




        /// <summary>
        /// 对于Sk，计算max{path(Sk)}
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double maxPathCompTime(int index)
        {
            bool noIncoming = true;
            List<int> candi = new List<int>(vexnum);
            for (int i = 0; i < vexnum; i++)
            {
                if (this.arcs[i, index].arcAdj == 1)
                {
                    noIncoming = false;
                    candi.Add(i);
                }
            }

            if (noIncoming)
                return this.vexs[index].pointComptime;
            else
            {
                double worseTime = 0;
                double tTime = 0;
                for (int i = 0; i < candi.Count; i++)
                {
                    tTime = calcResopnseTime(candi[i]);
                    if (tTime > worseTime)
                        worseTime = tTime;
                }

                return worseTime;
            }
        }


        /// <summary>
        /// 对于Sk，计算max{path(Sk)}
        /// </summary>
        /// <param name="index"></param>
        /// <param name="tk"></param>
        /// <returns></returns>
        public double maxPathCompTimeAdv(int index, int tk)
        {
            bool noIncoming = true;
            List<int> candi = new List<int>(vexnum);
            for (int i = 0; i < vexnum; i++)
            {
                if (this.arcs[i, index].arcAdj == 1)
                {
                    noIncoming = false;
                    candi.Add(i);
                }
            }

            if (noIncoming)
                return ( ceiling(tk, this.vexs[index].pointPeriod) * this.vexs[index].pointComptime );
            else
            {
                double worseTime = 0;
                double tTime = 0;
                for (int i = 0; i < candi.Count; i++)
                {
                    tTime = ceiling(tk, this.vexs[index].pointPeriod) * this.vexs[index].pointComptime + maxPathCompTimeAdv(candi[i], tk);
                    if (tTime > worseTime)
                        worseTime = tTime;
                }

                return worseTime;
            }
        }


        /// <summary>
        /// 根据优先级计算整个图的response time
        /// </summary>
        public void mgraphResponseTime()
        {
            for (int i = 0; i < this.mgraphVexnum; i++)
            {
                double t = calcResopnseTime(i);
                this.vexs[i].pointResopnsetime = t;
                /*                Console.WriteLine("Point " + i + " Response Time:" + t);*/
            }
        }

        /// <summary>
        /// 根据优先级计算id为index的点的response time
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double calcResopnseTime(int index)
        {
            double Cj = this.vexs[index].pointComptime;
            ArrayList pre = new ArrayList(vexnum);
            int preCount = 0;
            double mincost = 0;
            int cost = 0;


            for (int i = 0; i < vexnum; i++)
            {
                if (this.vexs[i].pointPrio < this.vexs[index].pointPrio)
                {
                    TC p = new TC(this.vexs[i].pointPeriod, this.vexs[i].pointComptime);
                    pre.Add(p);
                    mincost += p.C;
                    preCount++;
                }
            }

            if (pre.Count == 0)
            {
                return Cj;
            }

            pre.Sort(0, pre.Count, new TCCompare());

            for (cost = 0; ; cost++)
            {
                double a = cost;
                double b1 = 0.0;
                double b2 = 0.0;
                for (int k = 0; k < pre.Count; k++)
                {
                    TC p = (TC)pre[k];
                    b1 += ceiling((double)cost, p.T) * p.C;
                    b2 += ceiling((double)cost, p.T) * p.C;
                }

                b1 += Cj;
                b2 += Cj;

                if (b2 <= a)
                    break;
            }

            double Rj = Cj;
            for (int i = 0; i < pre.Count; i++)
            {
                TC p = (TC)pre[i];
                Rj += ceiling((double)cost, p.T) * p.C;
            }

            return Rj;
        }

        /// <summary>
        /// 
        /// </summary>
        public class TCCompare : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return ((TC)x).T - ((TC)y).T;
            }
        }

        /// <summary>
        /// 计算上取整
        /// </summary>
        /// <param name="r"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public int ceiling(double r, int t)
        {
            double a = (double)(r / t);
            if ((a - (int)a) < 0.001)
                return (int)a;
            else
                return (int)a + 1;
        }

        public int ceiling(int tk, int t)
        {
            if (tk % t < 0.001)
                return tk / t;
            else
                return (tk / t + 1);
        }



        public class ListCompare : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return ((Point)x).pointPeriod - ((Point)y).pointPeriod;
            }
        }

        /// <summary>
        /// 检测是否schedulable
        /// </summary>
        /// <returns></returns>
        public bool mgraphSchedulable()
        {
            for (int i = 0; i < vexnum; i++)
            {
                double lateness = this.vexs[i].pointResopnsetime - this.vexs[i].pointPeriod;
                if (Math.Abs(lateness) < 0.0001)
                    lateness = 0;


                if (lateness > 0)
                    return false;
            }
            return true;
        }


        /// <summary>
        /// 计算每个顶点的lateness
        /// </summary>
        public void mgraphCalcLateness()
        {
            for (int i = 0; i < vexnum; i++)
            {
                this.vexs[i].pointLateness = this.vexs[i].pointResopnsetime - this.vexs[i].pointPeriod;
                if (Math.Abs(this.vexs[i].pointLateness) < 0.001)
                    this.vexs[i].pointLateness = 0;
            }
        }
    }

    /// <summary>
    /// block类型
    /// </summary>
    public class Point
    {
        public int id;			    //编号
        public int period;	        //functional block的周期
        public int priority;	    //优先级
        public double comptime;       //computing time
        public double responsetime;	//worst-case response time
        public double lateness;        //lateness = responsetime - period
        public int sourceCount;    //前驱节点数
        public int succCount;      //后继节点数

        public Point()
        {
            this.id = 0;
            this.period = 0;
            this.priority = -1;
            this.comptime = 0;
            this.responsetime = 0;
            this.lateness = responsetime - lateness;
            this.sourceCount = 0;
            this.succCount = 0;
        }

        public Point(int index)
        {
            this.id = index;
            this.period = 0;
            this.comptime = 0;
            this.sourceCount = 0;
            this.succCount = 0;
        }

        public Point(int index, int period, double comptime)
        {
            this.id = index;
            this.period = period;
            this.priority = -1;
            this.comptime = comptime;
            this.responsetime = comptime;
            this.lateness = responsetime - lateness;
            this.sourceCount = 0;
            this.succCount = 0;
        }

        public Point(int id, int period, double comptime, double responsetime)
        {
            this.id = id;
            this.period = period;
            this.priority = -1;
            this.comptime = comptime;
            this.responsetime = responsetime;
            this.lateness = responsetime - lateness;
            this.sourceCount = 0;
            this.succCount = 0;
        }

        public Point(int id, int period, int priority, double comptime, double responsetime)
        {
            this.id = id;
            this.period = period;
            this.priority = priority;
            this.comptime = comptime;
            this.responsetime = responsetime;
            this.lateness = responsetime - lateness;
            this.sourceCount = 0;
            this.succCount = 0;
        }

        public int pointID
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public int pointPeriod
        {
            get { return this.period; }
            set { this.period = value; }
        }

        public int pointPrio
        {
            get { return this.priority; }
            set { this.priority = value; }
        }

        public double pointComptime
        {
            get { return this.comptime; }
            set { this.comptime = value; }
        }

        public double pointResopnsetime
        {
            get { return this.responsetime; }
            set { this.responsetime = value; }
        }

        public double pointLateness
        {
            get { return this.lateness; }
            set { this.lateness = value; }
        }
    }

    /// <summary>
    /// 弧类型
    /// </summary>
    public class ArcCell
    {
        public int adj;                //用1和-1表示边类型
        public int buf;                //buffer type: 1 or 2
        public int adjCount;           //两个定点之间可能存在多条边

        public ArcCell()
        {
            this.adj = 0;
            this.buf = 0;
            this.adjCount = 0;
        }

        public ArcCell(int adj)
        {
            this.adj = adj;
            this.buf = 0;
        }

        public ArcCell(int adj, int buf, int adjCount)
        {
            this.adj = adj;
            this.buf = buf;
            this.adjCount = adjCount;
        }

        public void setArcCount(int count)
        {
            this.adjCount = count;
        }

        public int arcAdj
        {
            get { return this.adj; }
            set { this.adj = value; }
        }

        public int arcBuf
        {
            get { return this.buf; }
            set { this.buf = value; }
        }

        public int arcAdjCount
        {
            get { return this.adjCount; }
            set { this.adjCount = value; }
        }
    }

}
