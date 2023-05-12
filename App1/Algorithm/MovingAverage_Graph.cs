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

namespace App1
{
    public class MovingAverage_Graph
    {
        public List<MA_Point> MA_Graph;
        public int order = 1;

        public MovingAverage_Graph(List<DataPoint> points, int order1)
        {
            order= order1;
            MA_Graph = new List<MA_Point>();
            Calculate_MA_Of(points);
        }

        private void Calculate_MA_Of(List<DataPoint> points)
        {
            float average = 0;
           // Console.WriteLine("order: " + order);
           // Console.WriteLine("points.count: " + points.Count);

            for(int i = 0; i < points.Count-order; i++) 
            {
                average = 0;
                for(int g = i; g < order +i; g++)
                {
                    average += points[g].close;
                }
                average = average /order;
                //MA_Graph.Add(new MA_Point(average,i + (int)Math.Round(order /2.0) -1 ));
                MA_Graph.Add(new MA_Point(average, i + (int)Math.Floor(order / 2.0)));
                //if (i == 0)
                //{
                //    int num = 17;
                //    int num2 = 19;
                //    Console.WriteLine("order is: " + order);
                //    Console.WriteLine("num is: " + num);
                //    Console.WriteLine("i + (int)Math.Round(order /2.0) is: " + (i + (int)Math.Round(order / 2.0)));
                //    Console.WriteLine("i + (int)Math.Round(9 /2.0) 9is: " + (i + (int)Math.Round(9 / 2.0)));
                //    Console.WriteLine("i + (int)Math.Round(num /2.0) 9is: " + (i + (int)Math.Round(num / 2.0)));
                //    Console.WriteLine("i + (int)Math.Round(17 /2.0) 9is: " + (i + (int)Math.Round(17 / 2.0)));
                //    Console.WriteLine("i + (int)Math.Round(num /2.0) 9is: " + (i + (int)Math.Round(num2 / 2.0)));
                //    Console.WriteLine("i + (int)Math.Round(19 /2.0) 9is: " + (i + (int)Math.Round(19 / 2.0)));
                //    Math.Ceiling(order/2.0);
                //    Math.Floor(order / 2.0);
                //}


                //Console.WriteLine(" MA_graph[" + i + "] place is : " + MA_Graph[i].place);
               // Console.WriteLine("order is: " + order);
            }
        }
    }
}