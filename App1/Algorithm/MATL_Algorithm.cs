using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace App1
{
    public class MATL_Algorithm
    {
        public List<DataPoint> original_Graph = new List<DataPoint>();
        public List<MovingAverage_Graph> movingAverage_Graph = new List<MovingAverage_Graph>();
        public int maxOrder = 2;

        Thread subMainThread;
        List<Thread> threads = new List<Thread>();
        public int currentDegree = 1;
        public int minOrder = 1;

        public event Action ContinueProcess;

        public MATL_Algorithm(List<DataPoint> points,int maxorder)
        {
            currentDegree = 1;
            original_Graph= points;
            maxOrder = maxorder;

            

        }

        public void SetMinOrder(int order)
        {
            minOrder = order;
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
            Console.WriteLine("event invoked form MATL_Algorithm");
            ContinueProcess?.Invoke();
        }

        private void Thread_Create_Ma_Graphs(int currentorder)
        {
            Console.WriteLine("1created with thread a MA graph in degree: " + currentDegree);
            movingAverage_Graph.Add(new MovingAverage_Graph(original_Graph, currentorder));
            Console.WriteLine("2created with thread a MA graph in degree: " + currentorder);

        }
    }
}