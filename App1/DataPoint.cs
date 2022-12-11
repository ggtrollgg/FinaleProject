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
        public String symbol;
        public string date;

        public DataPoint()
        {

        }
        public DataPoint(float heigh, float low, string date, String symbol)
        {
            this.heigh = heigh;
            this.low = low;
            this.date = date;
            this.symbol = symbol;
        }
    }
}