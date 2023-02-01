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

namespace App1
{
    public class Class_FatherGraph : View
    {

        public Context context;
        public Canvas canvas;
        public List<DataPoint> dataPoints;



        /// <param name="context"></param>

        public Class_FatherGraph(Context context) : base(context)
        {

        }

        public Class_FatherGraph(Context context, Canvas canvas, List<DataPoint> dataPoints) : base(context)
        {
            this.context = context;
            this.canvas = canvas;
            this.dataPoints = dataPoints;
        }
        public void SetDataPoints(List<DataPoint> points)
        {
            this.dataPoints = points;
        }
        
        //public void ReSizeCanvas(double zoomFactor_x, double zoomFactor_y) 
        //{
        //    Matrix matrix = new Matrix();

        //    // Save the current matrix
        //    matrix.Save();

        //    // Translate the origin to the center of the canvas
        //    matrix.Translate(canvas.Width / 2, canvas.Height / 2);

        //    // Scale the canvas by a factor of 2
        //    matrix.Scale(2, 2);

        //    // Translate the origin back to the top left corner of the canvas
        //    matrix.Translate(-canvas.Width / 2, -canvas.Height / 2);

        //    // Set the transformation on the canvas
        //    canvas.Transform(matrix);

        //    // Draw your graph here

        //    // Restore the matrix
        //    matrix.Restore();
        //}
    }

    //public interface Class_FatherGraph : View
    //{

    //    public Context context;
    //    public Canvas canvas;
    //    public List<DataPoint> dataPoints;



    //    /// <param name="context"></param>

    //    public Class_FatherGraph(Context context) : base(context)
    //    {

    //    }

    //    public Class_FatherGraph(Context context, Canvas canvas, List<DataPoint> dataPoints) : base(context)
    //    {
    //        this.context = context;
    //        this.canvas = canvas;
    //        this.dataPoints = dataPoints;
    //    }
    //}
}