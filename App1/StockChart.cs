using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net.Wifi.Aware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using Point = Android.Graphics.Point;

namespace App1
{
    internal class StockChart : View
    {
        private Context context;
        Canvas canvas;

        public float[] values;
        float heighest = 0, lowest = -1;

        public MyPoint[] points;
        MyCamera camera = new MyCamera(0,0);

        MyPoint lastPlace;
        float test_zoomfactor = 1;

        public bool Zoom = false;
        public bool Move = false;

        Paint p;
        public StockChart(Context context) : base(context)
        {
            this.context = context;
            p = new Paint();
            p.Color = Color.Red;
            var v = new Vector();
        }

        protected override void OnDraw(Canvas canvas1)
        {
            this.canvas = canvas1;
            if(values != null)
            {
                if (heighest == 0) {findLowHeigh();}
                CreatChartPoints();
                DrawPoints();
                Invalidate();
            }
        }

        public void findLowHeigh()
        {
            foreach (float i in values)
            {
                if(i> heighest) heighest = i;
                if(i< lowest || lowest == -1) lowest = i;
            }
        }

        public void CreatChartPoints()
        {
            if (points == null)
            {
                points = new MyPoint[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    points[i] = (new MyPoint((i * canvas.Width) / (values.Length - 1), canvas.Height + ((lowest - values[i]) * (1 / (heighest - lowest)) * canvas.Height)));
                }
            }
        }
        public void DrawPoints()
        {
            for (int i = 0; i < values.Length; i++)
            {
                //canvas.DrawCircle( (i * canvas.Width )/ (values.Length-1), canvas.Height + ((lowest- values[i] )*(1/(heighest-lowest)) * canvas.Height), (float)2, p);
                //canvas.DrawCircle(points[i].x + camera.CameraOffSetX, points[i].y, 2, p);
                if (i != values.Length - 1)
                {
                    canvas.DrawLine( (points[i].x )*test_zoomfactor + camera.CameraOffSetX, points[i].y + camera.CameraOffSetY  , (points[i + 1].x )*test_zoomfactor + camera.CameraOffSetX, points[i + 1].y + camera.CameraOffSetY, p);
                }
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if(lastPlace == null)
            {
                lastPlace = new MyPoint(e.GetX(), e.GetY());
                return true;
            }
            else
            {
                if (e.PointerCount > 1)
                {
                    Paint p1 = new Paint();
                    p1.Color = Color.Black;

                    //e.GetToolMajor(e.FindPointerIndex(1));
                    //test_zoomfactor += (e.GetToolMajor(e.GetPointerId(0)) - lastPlace.x) / 100000;

                    MyPoint point1 = new MyPoint((float)e.GetX(), (float)e.GetY());
                   



                    //MyPoint point2 = new MyPoint(e.GetAxisValue(Axis.X), e.GetAxisValue(Axis.Y));
                    MyPoint point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));


                    canvas.DrawCircle(point1.x, point1.y, 100, p1);
                    canvas.DrawCircle(point2.x, point2.y, 100, p1);
                    Invalidate();

                    //test_zoomfactor += ((float)e.GetX() - lastPlace.x) / 100;
                }
                if (e.Action == MotionEventActions.Move )
                {
                    if(e.PointerCount > 1)
                    {

                    }
                    else
                    {
                        if (Zoom)
                        {
                            test_zoomfactor += ((float)e.GetX() - lastPlace.x) / 100;
                        }
                        if (Move)
                        {
                            camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
                            camera.CameraOffSetY += (float)e.GetY() - lastPlace.y;
                        }
                    }
                }
            }

            lastPlace = new MyPoint(e.GetX(), e.GetY());
            return true;
        }

    }
}