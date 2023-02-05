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
    public class MyPoint
    {
        public float x;
        public float y;
        public MyPoint()
        {

        }
        
        public MyPoint(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public MyPoint(MyPoint p)
        {
            x = p.x;
            y = p.y;
        }
    }
}