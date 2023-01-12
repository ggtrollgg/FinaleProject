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


        public MySquare()
        { 
        }
        public MySquare(MyPoint upLeft, MyPoint downRight)
        {
            this.UpLeft = new MyPoint(upLeft.x,upLeft.y);
            this.DownRight = new MyPoint(downRight.x, downRight.y);
        }
        public MySquare(float Lx, float Ly, float Rx, float Ry) 
        {
            UpLeft = new MyPoint(Lx, Ly);
            DownRight= new MyPoint(Rx, Ry);
        }
        public void MoveTo(float Lx, float Ly, float Rx, float Ry)
        {
            UpLeft = new MyPoint(Lx, Ly);
            DownRight = new MyPoint(Rx, Ry);
        }
    }
}