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