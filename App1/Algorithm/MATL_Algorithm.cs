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
        public int maxDegree = 2;

        Thread subMainThread;
        List<Thread> threads = new List<Thread>();
        int currentDegree = 1;
        int minDegree = 1;

        public MATL_Algorithm(List<DataPoint> points,int maxdegree)
        {
            currentDegree = 1;
            original_Graph= points;
            maxDegree = maxdegree;

            subMainThread = new Thread(Create_MA_Graphs);
            subMainThread.Start();

        }

        private void Create_MA_Graphs()
        {
            for(int i = minDegree; i <= maxDegree; i++)
            {
                threads.Add(new Thread((Thread_Create_Ma_Graphs)));
                threads[i-minDegree].Start();
                Thread.Sleep(500);
                currentDegree++;
            }

        }

        private void Thread_Create_Ma_Graphs()
        {
            Console.WriteLine("1created with thread a MA graph in degree: " + currentDegree);
            movingAverage_Graph.Add(new MovingAverage_Graph(original_Graph, currentDegree));
            Console.WriteLine("2created with thread a MA graph in degree: " + currentDegree);
        }
    }
}