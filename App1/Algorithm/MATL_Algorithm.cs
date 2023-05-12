using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using App1.General_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace App1
{
    public class MATL_Algorithm
    {
        public List<DataPoint> original_Graph = new List<DataPoint>(); // the original graph
        public List<MovingAverage_Graph> movingAverage_Graph = new List<MovingAverage_Graph>(); // list of all moving averages -> list of graphs 
        public List<MA_Point> Average_Of_Graphs = new List<MA_Point>(); // average of all moving graphs -> a graph


        public TrendLine trendline; //the trendline of the "Average_Of_Graphs"  graph
        public int FuterPoint;
        public MA_Point prediction = new MA_Point(0, 0);
        public float variation = 0;

        //threads
        Thread subMainThread;
        //Thread tempThred;
        List<Thread> threads = new List<Thread>();

        //MA order controll
        public int maxOrder = 2;
        public int currentDegree = 2;
        public int minOrder = 2;



        public event Action ContinueProcess;

        public MATL_Algorithm(List<DataPoint> points,int maxorder, int futerPoint)
        {
            //currentDegree = 1;
            original_Graph= points;
            maxOrder = maxorder;
            FuterPoint = futerPoint;
            

        }



        public void SetMinOrder(int order)
        {
            minOrder = order;
            currentDegree = order;
        }
        public void Start_Algorithm()
        {
            subMainThread = new Thread(Create_MA_Graphs);
            subMainThread.Start();
           

            //Continue_Algorithm_Process?.Invoke();
        }
        private void Create_MA_Graphs()
        {
            //currentDegree= minOrder;
            for(int i = minOrder; i <= maxOrder; i++)
            {
                Console.WriteLine(currentDegree);
                threads.Add(new Thread(() => Thread_Create_Ma_Graphs(currentDegree)));
                threads[i-minOrder].Start();
                Thread.Sleep(20); //for preformence
                //Test_add_curretDegree();
                currentDegree++;
            }
            //Looper.Prepare();

            if (!Create_Average_Of_Graphs())
            {
                Console.WriteLine("something went wrong with 'create_Average_Of_graphs' ");
                return;
            } //creates average of all moving averages graphs including the original graph
            Create_TrendLine();

            CalculateVariation();
            CalculatePrediction();
            


            Console.WriteLine("event invoked form MATL_Algorithm");
            ContinueProcess?.Invoke();

        }

        private void Test_add_curretDegree()
        {
            Console.Write("added to current degree, was: " + currentDegree);
            Console.WriteLine(" and now: " +( currentDegree + 1));
            currentDegree++;
        }
        private void CalculatePrediction()
        {
            prediction.place = Average_Of_Graphs.Count + FuterPoint;
            prediction.price = trendline.Calculate_Y_Of_futerPoint(FuterPoint) + variation;
        }
        private void CalculateVariation() //calculate and adjust the variation between the original graph and the average_of_Graphs
        {
            float sum_variatoin = 0;
            for(int i =0; i < Average_Of_Graphs.Count; i++)
            {
                sum_variatoin += original_Graph[i].close - Average_Of_Graphs[i].price;
            }
            variation = sum_variatoin / Average_Of_Graphs.Count;
        }

        
        private void Create_TrendLine()
        {
            trendline = new TrendLine(Average_Of_Graphs);
            trendline.Create_TrendLine();
        }

        private void Thread_Create_Ma_Graphs(int currentorder)
        {
            Console.WriteLine("1created with thread a MA graph in degree: " + currentDegree);
            movingAverage_Graph.Add(new MovingAverage_Graph(original_Graph, currentorder));
            Console.WriteLine("2created with thread a MA graph in degree: " + currentorder);

        }

        public bool Create_Average_Of_Graphs()
        {
            if (movingAverage_Graph.Count > 0 && original_Graph.Count > 0)
            {
                //foreach (DataPoint point in original_Graph)
                //{
                //    Average_Of_Graphs.Add(new MA_Point(point.close, 1));
                //}

                foreach (MA_Point point in movingAverage_Graph[0].MA_Graph)
                {
                    Average_Of_Graphs.Add(new MA_Point(point.price, 1));
                }

                float price;
                int place;
                //int count = 0;

                Print_info_of_graphs();


                //for (int i = 0; i < movingAverage_Graph.Count; i++)
                for (int i = 1; i < movingAverage_Graph.Count; i++)
                {
                    //if (i == 0 && minOrder == 1 && movingAverage_Graph.Count > 1) // if min order== 1 than the first graph is the original graph, and we already added the values of the original graph
                    //{
                    //    i = 1;
                    //}
                   // count = movingAverage_Graph[i].MA_Graph.Count;

                    for (int g = 0; g < movingAverage_Graph[i].MA_Graph.Count ; g++)
                    {
                        place = (int)movingAverage_Graph[i].MA_Graph[g].place;
                        price = movingAverage_Graph[i].MA_Graph[g].price;
                        //Average_Of_Graphs[place].price += price;
                        Average_Of_Graphs[place - (int)(Math.Floor(minOrder/2.0)) ].price += price;

                        //using place as counter to see how many prices have been added to this point > because every order reduces the amount of points on the graph by 2
                        //this isent progremer friendly at all, but the allternetive is adding another field to the class or create an arrey thar will
                        //keep track of the amount of times that i added price to a field
                        Average_Of_Graphs[place - (int)(Math.Floor(minOrder / 2.0))].place += 1;
                       

                    }

                }


                
                for(int i = 0; i < Average_Of_Graphs.Count; i++)
                {
                    if(Average_Of_Graphs[i].place!= 0)
                    {
                        price = Average_Of_Graphs[i].price / Average_Of_Graphs[i].place;
                        Average_Of_Graphs[i].price = price;
                        Average_Of_Graphs[i].place = i;
                    }
                }
                //process ended

                return true;
            }
            //process ended
            return false;
            
        }

        private void Print_info_of_graphs()
        {
            Console.WriteLine("Average of graphs count is: " + Average_Of_Graphs.Count);
            Console.WriteLine("his last point place is: " + Average_Of_Graphs[Average_Of_Graphs.Count - 1].place);
            Console.WriteLine("____");

            Console.WriteLine("movingAverage_Graph.Count is: " + movingAverage_Graph.Count);
            Console.WriteLine("___");


            for (int i = 0; i < movingAverage_Graph.Count; i++)
            {
                Console.WriteLine("movingAverage_Graph[" + i + "] count is: " + movingAverage_Graph[i].MA_Graph.Count);
                Console.WriteLine("his Order is: " + movingAverage_Graph[i].order);
                Console.WriteLine("his first point place is: " + movingAverage_Graph[i].MA_Graph[0].place);
                Console.WriteLine("his last point place is: " + movingAverage_Graph[i].MA_Graph[movingAverage_Graph[i].MA_Graph.Count - 1].place);
                Console.WriteLine("____");
            }
        }
    }
}