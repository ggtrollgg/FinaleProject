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

namespace App1.Algorithm
{
    public class MA_View:View
    {
        List<MovingAverage_Graph> Graphs= new List<MovingAverage_Graph>();
        public Context context;
        public Canvas canvas;

        bool DoOnce = true;
        float cilling = 100;
        float floor;
        float right_wall;
        float highest = -1;
        float lowest = -1;

        Paint black;

        public MA_View(Context context, List<MovingAverage_Graph> graphs) : base(context)
        {
            this.context = context;
            Graphs = graphs;

            black= new Paint();
            black.Color= Color.Black;
        }

        protected override void OnDraw(Canvas canvas1)
        { 
            if(DoOnce) 
            {
                canvas = canvas1;
                Console.WriteLine("started drawing the MA graph");
                floor = canvas.Height - 100;
                right_wall= canvas.Width - 100;
                FindLowHigh();
                Draw_Graphs();
                //canvas.DrawCircle(0,0,100,black);
                DoOnce = false;
                //Invalidate();
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
            int total_points = Graphs[0].MA_Graph.Count;
            float place = 0;
            float price = 0;

            for(int graph = 0; graph< Graphs.Count; graph++) 
            {
                //need to generete a random color
                for (int i = 0; i < Graphs[graph].MA_Graph.Count; i++)
                {
                    place = Graphs[graph].MA_Graph[i].place;
                    price = Graphs[graph].MA_Graph[i].price;
                    canvas.DrawPoint(place * (right_wall) / (total_points - 1), floor + ((lowest - price) * (1 / (highest - lowest)) * floor),black);
                }
                Console.WriteLine("canvas.Save + Invalidate");
                //canvas.Save(); // i dont remeber if canvas works that way 
                //Invalidate();
                Thread.Sleep(1000);
                
            }

        }
    }
}