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
    public class MA_Point
    {
        public float price;
        public float place;

        public MA_Point(float price, float place)
        {
            this.price = price;
            this.place = place;
        }
    }
}