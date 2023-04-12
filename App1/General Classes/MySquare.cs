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
    public class MySquare
    {
        public MyPoint UpLeft { get; set; }
        public MyPoint DownRight { get; set; }

        public MyPoint Center { get; }
        public MySquare()
        { 
        }
        public MySquare(MyPoint upLeft, MyPoint downRight)
        {
            this.UpLeft = new MyPoint(upLeft.x,upLeft.y);
            this.DownRight = new MyPoint(downRight.x, downRight.y);

            this.Center = new MyPoint((upLeft.x + downRight.x) / (float)2.0, (upLeft.y + downRight.y) / (float)2.0);
        }
        public MySquare(float Lx, float Ly, float Rx, float Ry) 
        {
            UpLeft = new MyPoint(Lx, Ly);
            DownRight= new MyPoint(Rx, Ry);

            this.Center = new MyPoint((Lx + Rx) / (float)2.0, (Ly + Ry) / (float)2.0);
        }
        
    }
}