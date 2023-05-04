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
        public int degree = 1;

        public MovingAverage_Graph(List<DataPoint> points, int degree1)
        {
            degree= degree1;
            MA_Graph = new List<MA_Point>();
            Calculate_MA_Of(points);
        }

        private void Calculate_MA_Of(List<DataPoint> points)
        {
            float average = 0;
            for(int i = 0; i < points.Count-degree; i++) 
            {
                average = 0;
                for(int g = i; g < degree+i; g++)
                {
                    average += points[g].close;
                }
                average = average /degree;
                MA_Graph.Add(new MA_Point(average,i + (int)Math.Round(degree/2.0))); 
            }
        }
    }
}