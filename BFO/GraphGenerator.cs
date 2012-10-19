using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace BFO
{
    public class GraphGenerator
    {
        //utilization factor
        static double uti;

        //number of source blocks: 
        static int rootnum = 1;

        //number of total blocks: 
        public static int blocknum = 6;

        static double utilization = uti / blocknum;

        //possible rates: 
        static int[] rates;

        //possibility of one block have one output: 
        static double rateOneOutput = 0.5;

        //possibility of one block have two outputs: 
        static double rateTwoOutput = 1 - rateOneOutput;

        //successor search nodes: i
        static int succRange = rootnum * 2;

        public static Random ro = new Random(DateTime.Now.Millisecond);

        public GraphGenerator(int num)
        {
            StreamReader sr = new StreamReader("GraphPara.txt");

            sr.ReadLine();
            uti = Convert.ToDouble(sr.ReadLine());

            sr.ReadLine();
            rootnum = Convert.ToInt32(sr.ReadLine());

            sr.ReadLine();
            blocknum = Convert.ToInt32(sr.ReadLine());

            utilization = uti / blocknum;

            sr.ReadLine();
            String[] rateString = sr.ReadLine().Trim().Split(' ');
            rates = new int[rateString.Length];
            for (int i = 0; i < rateString.Length; i++)
                rates[i] = Convert.ToInt32(rateString[i]);

            sr.ReadLine();
            rateOneOutput = Convert.ToDouble(sr.ReadLine());
            rateTwoOutput = 1 - rateOneOutput;

            sr.ReadLine();
            sr.ReadLine();
            succRange = Convert.ToInt32(sr.ReadLine());


            for (int i = 0; i < num; i++)
            {
                randomGenerate(i);
            }
        }

        public void randomGenerate(int index)
        {
            Point[] vexs = new Point[blocknum];
            ArcCell[,] arcs = new ArcCell[blocknum, blocknum];


            for (int i = 0; i < blocknum; i++)
            {
                vexs[i] = new Point(i);

                for (int j = 0; j < blocknum; j++)
                    arcs[i, j] = new ArcCell();
            }


            for (int i = 0; i < blocknum; i++)
            {
                int start, end;

                if (i < rootnum)
                {
                    vexs[i].period = randomRate(rates);
                    start = rootnum;
                    end = i + succRange;

                    if (end >= blocknum)
                        end = blocknum - 1;

                    if (start > end)
                        continue;
                }
                else
                {
                    if (vexs[i].period == 0)
                        vexs[i].period = randomRate(rates);

                    start = i + 1;
                    end = i + succRange;

                    if (end >= blocknum)
                        end = blocknum - 1;

                    if (start > end)
                        continue;
                }

                int succNum = 1;
                if (ro.NextDouble() >= rateOneOutput)
                    succNum = 2;
                else
                    succNum = 1;

                for (int j = 0; j < succNum; j++)
                {
                    Boolean reset = false;

                    int k = ro.Next(start, end + 1);
                    int initK = k;

                    int kRate;
                    if (vexs[k].sourceCount == 0)
                    {
                        do
                        {
                            kRate = randomRate(rates);
                        } while (kRate == vexs[i].period);

                        vexs[k].period = kRate;
                        vexs[k].sourceCount++;
                        vexs[i].succCount++;

                        arcs[i, k].adj = 1;
                        arcs[i, k].adjCount++;
                        arcs[k, i].adj = -1;
                        arcs[k, i].adjCount++;
                    }
                    else if (vexs[k].sourceCount == 1 && vexs[k].period != vexs[i].period)
                    {
                        vexs[k].sourceCount++;
                        vexs[i].succCount++;
                        arcs[i, k].adj = 1;
                        arcs[i, k].adjCount++;
                        arcs[k, i].adj = -1;
                        arcs[k, i].adjCount++;
                    }
                    else if ((vexs[k].sourceCount == 1 && vexs[k].period == vexs[i].period) || vexs[k].sourceCount == 2)
                    {
                        do
                        {
                            k++;
                            if (k > end)
                            {
                                k = start;
                                reset = true;
                            }
                            if (k == initK && reset == true)
                                break;

                            if (vexs[k].sourceCount == 0)
                            {
                                do
                                {
                                    kRate = randomRate(rates);
                                } while (kRate == vexs[i].period);

                                vexs[k].period = kRate;
                                vexs[k].sourceCount++;
                                vexs[i].succCount++;

                                arcs[i, k].adj = 1;
                                arcs[i, k].adjCount++;
                                arcs[k, i].adj = -1;
                                arcs[k, i].adjCount++;
                                break;
                            }
                            else if (vexs[k].sourceCount == 1 && vexs[k].period != vexs[i].period)
                            {
                                vexs[k].sourceCount++;
                                vexs[i].succCount++;
                                arcs[i, k].adj = 1;
                                arcs[i, k].adjCount++;
                                arcs[k, i].adj = -1;
                                arcs[k, i].adjCount++;
                                break;
                            }
                        } while (true);
                    }
                }
            }



            //write to g.in

            for (int i = 0; i < blocknum; i++)
            {
                double rantime = vexs[i].period * utilization;
                rantime = rantime + rantime * ro.Next(-1, 1) / 10;

                vexs[i].comptime = rantime;
            }

            int arcnum = 0;
            for (int i = 0; i < blocknum; i++)
            {
                for (int j = 0; j < blocknum; j++)
                {
                    if (arcs[i, j].adj == 1)
                        arcnum++;
                }
            }

            String outputfile = index.ToString() + ".txt";
            FileStream fs = new FileStream(outputfile, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(blocknum + " " + arcnum);

            double u = 0;
            for (int i = 0; i < blocknum; i++)
            {
                sw.WriteLine(vexs[i].period + " " + vexs[i].comptime);
                //System.Console.WriteLine(vexs[i].period + " " + vexs[i].comptime + " " + vexs[i].comptime / vexs[i].period);

                u += vexs[i].comptime / vexs[i].period;

            }

            for (int i = 0; i < blocknum; i++)
            {
                for (int j = 0; j < blocknum; j++)
                {
                    sw.Write(arcs[i, j].adj + " ");
                }
                sw.WriteLine();
            }

            for (int i = 0; i < blocknum; i++)
            {
                for (int j = 0; j < blocknum; j++)
                {
                    //sw.Write(arcs[i, j].adjCount + " ");
                    sw.Write("1 ");
                }
                sw.WriteLine();
            }

            sw.Flush();
            sw.Close();


            System.Console.WriteLine("utilization = " + u);
            return;
        }

        public static int randomRate(int[] rates)
        {
            return rates[ro.Next(0, rates.Length)];
        }
    }

}
