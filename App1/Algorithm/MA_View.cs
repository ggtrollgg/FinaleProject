using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Firestore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Grpc;

namespace App1.Algorithm
{
    public class MA_View:View
    {
        List<MovingAverage_Graph> Graphs= new List<MovingAverage_Graph>();
        List<List<MyPoint>> Graphs_on_Canvas= new List<List<MyPoint>>();

        public Android.Content.Context context;
        public Canvas canvas;

        bool DoOnce = true;
        bool running = true;
        bool started = false;
        float cilling = 100;
        float floor;
        float right_wall;
        float highest = -1;
        float lowest = -1;

        DateTime currentTime = DateTime.Now;
        TimeSpan span;
        int index_action = 1;
        int difference = 0;

        Paint black;
        Paint red;
        //Paint random;
        List<Paint> randoms= new List<Paint>();
        public MA_View(Android.Content.Context context, List<MovingAverage_Graph> graphs) : base(context)
        {
            this.context = context;
            Graphs = graphs;

            
            //random.SetARGB(1,0,0,0);
            //random.Color = Color.Argb(255,0,0,0);
            //random.StrokeWidth = 6;
            //random.Color = 255255255;

            black= new Paint();
            black.Color= Color.Black;
            black.StrokeWidth= 6;

            red = new Paint();
            red.Color= Color.Red;
            red.StrokeWidth= 6;

            Random ram = new Random();
            for(int i = 0; i<graphs.Count; i++)
            {
                Paint random = new Paint();
                random.Color = Color.Argb(255, ram.Next(200), ram.Next(200), ram.Next(200));
                random.StrokeWidth= 6;
                randoms.Add(random);
            }
           



            Thread frameRate = new Thread(FrameRate_Invalidate);
            //frameRate.Start();
        }

        protected override void OnDraw(Canvas canvas1)
        { 
            //started= true;
            if(DoOnce) 
            {
                canvas = canvas1;
                Console.WriteLine("started drawing the MA graph");
                floor = canvas.Height - 100;
                right_wall= canvas.Width - 100;
                FindLowHigh();
                CreateALLGraphs();
                //canvas.DrawCircle(0,0,100,black);
                DoOnce = false;
            }
            
            Check_time_span();

            

            Draw_Graphs();
            //if (canvas.SaveCount >= 1)
            //{
            //    canvas.Restore();
            //    canvas.Save();
            //}
            Invalidate();
            //started= false;
        }

        private void FrameRate_Invalidate()
        {
            while(running)
            {
                if (!started)
                {
                    Invalidate();
                    //Console.WriteLine("Invalidated");
                }
                    
                Thread.Sleep(1000);
            }
        }

        private void Check_time_span()
        {
            span = DateTime.Now - currentTime;
            difference = (int)(span.TotalMilliseconds);

            if (difference < 0) 
            {
                difference = 1000 - difference;
            }

            if (difference > 2000)
            {
                currentTime = DateTime.Now;
                index_action++;
                Console.WriteLine("increast index_action to: " + index_action);
            }
        }

        private void CreateALLGraphs()
        {
            int total_points = Graphs[0].MA_Graph.Count;
            float place = 0;
            float price = 0;

            for (int graph = 0; graph < Graphs.Count; graph++)
            {
                Graphs_on_Canvas.Add(new List<MyPoint>());
                for (int i = 0; i < Graphs[graph].MA_Graph.Count; i++)
                {
                    place = Graphs[graph].MA_Graph[i].place;
                    price = Graphs[graph].MA_Graph[i].price;
                    Graphs_on_Canvas[graph].Add(new MyPoint(place * (right_wall) / (total_points - 1), floor + ((lowest - price) * (1 / (highest - lowest)) * floor)));

                    Console.Write("(" + Graphs_on_Canvas[graph][i].x + ","+ Graphs_on_Canvas[graph][i].y + "), ");
                }
                Console.WriteLine("----");
                Console.WriteLine("Create next graph");
                Console.WriteLine("----");
            }
        }

        private void FindLowHigh()
        {
            foreach (MA_Point i in Graphs[0].MA_Graph)
            {
                if (i.price > highest) highest = i.price;
                if (i.price < lowest || lowest == -1) lowest = i.price;
            }
        }

        private void Draw_Graphs()
        {
            int smaller_limit = Math.Min(index_action, Graphs.Count);
           


            for(int graph = 0; graph< smaller_limit; graph++) 
            {
                //need to generete a random color
                for (int i = 0; i < Graphs[graph].MA_Graph.Count-1; i++)
                {
                    canvas.DrawLine(Graphs_on_Canvas[graph][i].x, Graphs_on_Canvas[graph][i].y, Graphs_on_Canvas[graph][i+1].x, Graphs_on_Canvas[graph][i+1].y, randoms[graph]);
                }
                
                
            }
            //canvas.Save();
            //started = false;
            //Invalidate();
        }
    }
}