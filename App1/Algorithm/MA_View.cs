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
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Grpc;

namespace App1.Algorithm
{
    public class MA_View:View
    {
        MATL_Algorithm Algorithm;
        List<MovingAverage_Graph> Graphs= new List<MovingAverage_Graph>();
        List<List<MyPoint>> Graphs_on_Canvas= new List<List<MyPoint>>();

        List<MyPoint> OriginalGraph = new List<MyPoint>();
        List<MyPoint> trendLine = new List<MyPoint>();
        MyPoint prediction = new MyPoint(0,0);

        public Android.Content.Context context;
        public Canvas canvas;

        bool DoOnce = true;
        bool running = true;
        bool started = true;

        float floor;
        float right_wall;
        float highest = -1;
        float lowest = -1;
        int total_points;

        DateTime currentTime = DateTime.Now;
        TimeSpan span;
        int index_action = 1;
        int difference = 0;

        Paint black;
        Paint black_graph;
        Paint red;
        Paint green;
        List<Paint> randoms= new List<Paint>();


        public MA_View(Android.Content.Context context, MATL_Algorithm algo) : base(context)
        {
            this.context = context;
            Algorithm = algo;
            Graphs = algo.movingAverage_Graph;
            Do_OnCreate();
        }

        public MA_View(Android.Content.Context context, List<MovingAverage_Graph> graphs) : base(context)
        {

            this.context = context;
            Graphs = graphs;
            Do_OnCreate();
        }

        protected override void OnDraw(Canvas canvas1)
        { 

            canvas = canvas1;
            if (DoOnce) //things i want to do only once at the start (and  they need canvas)
            {
                Console.WriteLine("started drawing the MA graph");
                floor = canvas.Height - 100;
                right_wall= canvas.Width - 100;
                FindLowHigh();
                CreateALLGraphs();

                DoOnce = false;
            }

            if (index_action < Graphs.Count + 5)
            {
                Check_time_span();
            }
            Draw_Graphs();
            Invalidate();

        }

        //things i want to do only once at the start (and  they dont need canvas)
        private void Do_OnCreate()
        {
            total_points = Algorithm.original_Graph.Count;
            black = new Paint();
            black.Color = Color.Black;
            black.StrokeWidth = 5;
            black_graph = new Paint();
            black_graph.Color = Color.Argb(155,0,0,0);
            black_graph.StrokeWidth = 6;
            red = new Paint();
            red.Color = Color.Red;
            red.StrokeWidth = 6;
            green = new Paint();
            green.Color = Color.DarkGreen;
            green.StrokeWidth = 6;
            Random ram = new Random();
            for (int i = 0; i < Graphs.Count; i++)
            {
                Paint random = new Paint();
                random.Color = Color.Argb(255, ram.Next(150)+50, ram.Next(150)+50, ram.Next(150) + 50);
                random.StrokeWidth = 8;
                randoms.Add(random);

            }

        }

        //check the time between the last time the draw function was invalidated
        //after enought time has past than up the index action by one ->
        //index action = how many graphs to draw 
        private void Check_time_span()
        {
            
            span = DateTime.Now - currentTime;
            difference = (int)(span.TotalMilliseconds);

            if (difference < 0) 
            {
                difference = 1000 - difference;
            }

            if (difference > 500)
            {
                currentTime = DateTime.Now;
                index_action++;
            }
        }

        //find the point with the highest value and the lowest value
        private void FindLowHigh()
        {
            
            foreach (DataPoint i in Algorithm.original_Graph) // will streach so the graph fit the original graph
            {
                if (i.close > highest) highest = i.close;
                if (i.close < lowest || lowest == -1) lowest = i.close;
            }
        }


        //create the lists that contains the points of the graphs with values of x and y
        //and fill these lists so all the points are within an area i want 
        private void CreateALLGraphs()
        {

            float place = 0;
            float price = 0;
            create_toScale_trendLine();
            create_toScale_OriginalGraph();
            create_toScale_Prediction();

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

        //create the prediction of the futer value and translate it to fit the canvas/graph scale
        private void create_toScale_Prediction()
        {
            prediction = Convert_To_defualt_scale(Algorithm.FuterPoint + total_points, Algorithm.prediction.price + Algorithm.variation);
            Console.WriteLine("variation is: " + Algorithm.variation);
        }

        //translate the original graph  to fit the canvas/graph scale with x and y values
        private void create_toScale_OriginalGraph()
        {
            for (int i = 0; i < total_points; i++)
            {
                OriginalGraph.Add(Convert_To_defualt_scale(i, Algorithm.original_Graph[i].close));
            }
        }

        //create the TrendLine and translate it to fit the canvas/graph scale
        private void create_toScale_trendLine()
        {
            MyPoint start = new MyPoint(0, Algorithm.trendline.Calculate_Y_Of(0));
            trendLine.Add(Convert_To_defualt_scale(0, start.y));
            trendLine.Add(Convert_To_defualt_scale(total_points+ Algorithm.FuterPoint, Algorithm.prediction.price));
        }

        //gets an MA_Point values and translates them to MyPoint values
        private MyPoint Convert_To_defualt_scale(int place,float price)
        {
            return new MyPoint(place * (right_wall) / (total_points - 1), floor + ((lowest - price) * (1 / (highest - lowest)) * floor));
        }

        //Draws the MA Graphs, the original graph and the trendline
        private void Draw_Graphs()
        {
            int smaller_limit = Math.Min(index_action, Graphs.Count);
            for (int i = 0; i < total_points - 1; i++) //drawing original/real stock price
            {
                canvas.DrawLine(OriginalGraph[i].x, OriginalGraph[i].y, OriginalGraph[i + 1].x, OriginalGraph[i + 1].y, black_graph);
            }

            for (int graph = 0; graph < smaller_limit; graph++) 
            {
                for (int i = 0; i < Graphs[graph].MA_Graph.Count-1; i++)
                {
                    canvas.DrawLine(Graphs_on_Canvas[graph][i].x, Graphs_on_Canvas[graph][i].y, Graphs_on_Canvas[graph][i+1].x, Graphs_on_Canvas[graph][i+1].y, randoms[graph]);
                }
                
                
            }

            
            canvas.DrawLine(trendLine[0].x, trendLine[0].y, trendLine[1].x, trendLine[1].y, green); //draw trendline
            canvas.DrawCircle(prediction.x, prediction.y, 8, red);
            started = false;
        }
    }
}