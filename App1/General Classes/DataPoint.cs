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
    public class DataPoint
    {
        public float heigh;
        public float low;
        public float close;
        public string date;
        public float open;
        public float price; 

       

        public DataPoint(float heigh, float low, float close,float open, String date)
        {
            this.heigh = heigh;
            this.low = low;
            this.close = close;
            this.date = date;
            this.open = open;
        }


    }
}