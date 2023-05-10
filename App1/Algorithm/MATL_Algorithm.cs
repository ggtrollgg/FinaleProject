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

        //threads
        Thread subMainThread;
        List<Thread> threads = new List<Thread>();

        //MA order controll
        public int maxOrder = 2;
        public int currentDegree = 1;
        public int minOrder = 1;



        public event Action ContinueProcess;

        public MATL_Algorithm(List<DataPoint> points,int maxorder, int futerPoint)
        {
            currentDegree = 1;
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
            for(int i = minOrder; i <= maxOrder; i++)
            {
                threads.Add(new Thread(() => Thread_Create_Ma_Graphs(currentDegree)));
                threads[i-minOrder].Start();
                Thread.Sleep(10); //for preformence
                currentDegree++;
            }
            //Looper.Prepare();

            if (!Create_Average_Of_Graphs())
            {

                Console.WriteLine("something went wrong with 'create_Average_Of_graphs' ");
                return;
            }



            trendline = new TrendLine(Average_Of_Graphs);
            
            Console.WriteLine("event invoked form MATL_Algorithm");
            ContinueProcess?.Invoke();

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
                foreach (DataPoint point in original_Graph)
                {
                    Average_Of_Graphs.Add(new MA_Point(point.close, 1));
                }

                float price;
                int place;
                //int count = 0;

                for (int i = 0; i < movingAverage_Graph.Count; i++)
                {
                    if (i == 0 && minOrder == 1) // if min order== 1 than the first graph is the original graph, and we already added the values of the original graph
                    {
                        i = 1;
                    }
                   // count = movingAverage_Graph[i].MA_Graph.Count;
                    for (int g = 0; g < movingAverage_Graph[i].MA_Graph.Count ; g++)
                    {
                        place = (int)movingAverage_Graph[i].MA_Graph[g].place;
                        price = movingAverage_Graph[i].MA_Graph[g].price;
                        Average_Of_Graphs[place].price += price;
                        //using place as counter to see how many prices have been added to this point > because every order reduces the amount of points on the graph by 2
                        //this isent progremer friendly at all, but the allternetive is adding another field to the class or create an arrey thar will
                        //keep track of the amount of times that i added price to a field
                        Average_Of_Graphs[place].place += 1;
                       

                    }

                }


                
                for(int i = 0; i < Average_Of_Graphs.Count; i++)
                {
                    price = Average_Of_Graphs[i].price / Average_Of_Graphs[i].place;
                    Average_Of_Graphs[i].price = price;
                    Average_Of_Graphs[i].place = i;

                }
                //process ended

                return true;
            }
            //process ended
            return false;
            
        }
   
    }
}