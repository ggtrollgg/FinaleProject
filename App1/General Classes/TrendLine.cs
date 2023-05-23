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

namespace App1.General_Classes
{
    public class TrendLine
    {
        public List<MyPoint> Points = new List<MyPoint>();
        // y = Mx + B
        float X; // average x
        float Y; // average y
        float B; // b value 
        float M; // שיפוע


        public TrendLine(List<MyPoint> graph)
        {
            foreach( MyPoint p in graph)
            {
                this.Points.Add(p);
            }
            
        }
        public TrendLine(List<MA_Point> graph)
        {
           Convert_List_To_MyPoint(graph);
            

        }

        //convert a list of MA_Points to a list of "MyPoint"
        private void Convert_List_To_MyPoint(List<MA_Point> graph)
        {
            for(int i = 0; i < graph.Count; i++ ) 
            {
                Points.Add(new MyPoint(i, graph[i].price)); 
            }
        }

        //create all the components that represent a trend line
        public void Create_TrendLine()
        {
            for(int i = 0; i < Points.Count; i++) 
            {
                X += this.Points[i].x;
                Y += this.Points[i].y;
            }

            X = X/Points.Count;
            Y = Y/Points.Count;

            Calculate_M();

            B = Y - M*X;

        }

        //calculate the slope of the trend line
        private void Calculate_M()
        {
            M = 0;
            float Upper_formula = 0;
            float Lower_formula = 0;

            for (int i = 0; i < Points.Count; i++)
            {
                Upper_formula += (Points[i].x - X)*(Points[i].y - Y);
                Lower_formula += (Points[i].x - X) * (Points[i].x - X);
            }

            M = Upper_formula / Lower_formula;
        }

        
        public float Calculate_Y_Of(float x_value)
        {
            float y_value = M*x_value + B;
            return y_value;
        }

        //returns the y value of the point in the futer --> point 2 in the futer is the point that is in the end the exsisting list of points in the graph + 2
        public float Calculate_Y_Of_futerPoint(int point_in_the_futer)
        {
            float y_value = M * (point_in_the_futer+ Points.Count-1) + B;
            return y_value;
        }
    }
}