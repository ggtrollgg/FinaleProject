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

        private void Convert_List_To_MyPoint(List<MA_Point> graph)
        {
            for(int i = 0; i < graph.Count; i++ ) 
            {
                //each point is 1 unit appart from each other
                //so when i want to draw the graph i could just calculate the distance between point1 and point2 on the graph
                //the graph in MA_view -> that was calculated to fit the entire graph on the screen at once
                // and put multuplay the x value with this distance (in order to put the point in prespective of the graph)
                Points.Add(new MyPoint(i, graph[i].price)); 
            }
        }

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

        public float Calculate_Y_Of_futerPoint(int point_in_the_futer)
        {
            float y_value = M * (point_in_the_futer+ Points.Count-1) + B;
            return y_value;
        }
    }
}