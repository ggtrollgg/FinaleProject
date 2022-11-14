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
        public MyPoint[] Changedpoints;
        MyCamera camera = new MyCamera(0,0);


        MyPoint lastPlace;
        float test_zoomfactor = 1;


        public bool Zoom = false;
        public bool Move = false;

        Paint p1 = new Paint();
        MyPoint point1;
        MyPoint point2;
        MyPoint midPoint;

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
                DrawTouching();
                Invalidate();
            }
        }

        

        private void DrawTouching()
        {
            p1.Color = Color.Black;
            if(point1 != null && point2 != null && midPoint!=null)
            {
                canvas.DrawCircle(point1.x, point1.y, 100, p1);
                canvas.DrawCircle(point2.x, point2.y, 100, p1);
                canvas.DrawCircle(midPoint.x, midPoint.y, 10, p1);

                int i = CalculatePointZoomingOn();

                //Console.WriteLine("mispoint x is: " + midPoint.x);
                //Console.WriteLine("changed x is: " + Changedpoints[i].x);
                //Console.WriteLine("changed-1 x is: " + Changedpoints[i-1].x);
                //Console.WriteLine("changed-2 x is: " + Changedpoints[i-2].x);
                //Console.WriteLine("changed-3 x is: " + Changedpoints[i-3].x);
                //Console.WriteLine("changed-4 x is: " + Changedpoints[i-4].x);

                canvas.DrawCircle(Changedpoints[i].x, Changedpoints[i-5].y, 10, p1);
                p1.Color = Color.Blue;
                canvas.DrawCircle(Changedpoints[i-1].x, Changedpoints[i].y, 10, p1);
                p1.Color = Color.Green;
                canvas.DrawCircle(Changedpoints[i+1].x, Changedpoints[i].y, 10, p1);
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
                Changedpoints = new MyPoint[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    points[i] = (new MyPoint((i * canvas.Width) / (values.Length - 1), canvas.Height + ((lowest - values[i]) * (1 / (heighest - lowest)) * canvas.Height)));
                    //Changedpoints[i] = (new MyPoint((i * canvas.Width) / (values.Length - 1), canvas.Height + ((lowest - values[i]) * (1 / (heighest - lowest)) * canvas.Height)));
                }
                CalculateNewPointes();
            }
        }
        public void DrawPoints()
        {
            for (int i = 0; i < values.Length; i++)
            {
                //canvas.DrawCircle( (i * canvas.Width )/ (values.Length-1), canvas.Height + ((lowest- values[i] )*(1/(heighest-lowest)) * canvas.Height), (float)2, p);
                //canvas.DrawCircle(points[i].x + camera.CameraOffSetX, points[i].y, 2, p);
                canvas.DrawCircle(Changedpoints[i].x, Changedpoints[i].y, 2, p);
                if (i != values.Length - 1)
                {
                    // canvas.DrawLine( (points[i].x )*test_zoomfactor + camera.CameraOffSetX, points[i].y + camera.CameraOffSetY  , (points[i + 1].x )*test_zoomfactor + camera.CameraOffSetX, points[i + 1].y + camera.CameraOffSetY, p);
                    canvas.DrawLine(Changedpoints[i].x, Changedpoints[i].y, Changedpoints[i+1].x, Changedpoints[i+1].y, p);
                }
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (e.PointerCount > 1)
            {
                p1.Color = Color.Black;

                point1 = new MyPoint((float)e.GetX(), (float)e.GetY());
                point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));

                if(e.Action == MotionEventActions.Pointer2Down)
                {
                    midPoint = new MyPoint((point1.x + point2.x) / 2, (point1.y + point2.y) / 2);
                }
            }
            if(e.Action == MotionEventActions.Up)
            {
                point1 = null;
                point2 = null;
                midPoint = null;
            }



            if (lastPlace == null)
            {
                lastPlace = new MyPoint(e.GetX(), e.GetY());
                return true;
            }
            else
            {
                if (e.Action == MotionEventActions.Move)
                {
                    if (e.PointerCount > 1)
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
                            if(!(camera.CameraOffSetX + (float)e.GetX() - lastPlace.x >= 0))
                            {
                                camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
                            }
                            camera.CameraOffSetY += (float)e.GetY() - lastPlace.y;
                        }
                    }
                }
            }

            CalculateNewPointes();

            lastPlace.x = e.GetX();
            lastPlace.y = e.GetY();
            return true;


        }

        private void CalculateNewPointes()
        {
           for(int i = 0; i < values.Length; i++)
            {
               Changedpoints[i] = new MyPoint((points[i].x) * test_zoomfactor + camera.CameraOffSetX, points[i].y + camera.CameraOffSetY);
            }
        }

        private int CalculatePointZoomingOn()
        {
            float defualtPointx = (midPoint.x - camera.CameraOffSetX) / test_zoomfactor;
            float defualtI = (defualtPointx*(values.Length - 1))/canvas.Width;


            float i = (((midPoint.x - camera.CameraOffSetX) / test_zoomfactor)/(canvas.Width/(values.Length-1)));

            Console.WriteLine("dedualtI is: " + defualtI);
            Console.WriteLine("i is: " + i);

            //Console.WriteLine("the soposed x from the calculation is: " + ((midPoint.x - camera.CameraOffSetX) / test_zoomfactor));
            int defualtI2 = (int)Math.Round(defualtI);
            int i2 = (int)Math.Round(i);
            //Console.WriteLine("i2 is:" + i2);
            //return i2;
            return defualtI2;  
        }
    }

        //public override bool OnTouchEvent(MotionEvent e)
        //{
        //    if(lastPlace == null)
        //    {
        //        lastPlace = new MyPoint(e.GetX(), e.GetY());
        //        return true;
        //    }
        //    else
        //    {
        //        if (e.PointerCount > 1)
        //        {
        //            Paint p1 = new Paint();
        //            p1.Color = Color.Black;

        //            //e.GetToolMajor(e.FindPointerIndex(1));
        //            //test_zoomfactor += (e.GetToolMajor(e.GetPointerId(0)) - lastPlace.x) / 100000;

        //            MyPoint point1 = new MyPoint((float)e.GetX(), (float)e.GetY());




        //            //MyPoint point2 = new MyPoint(e.GetAxisValue(Axis.X), e.GetAxisValue(Axis.Y));
        //            MyPoint point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));


        //            canvas.DrawCircle(point1.x, point1.y, 100, p1);
        //            canvas.DrawCircle(point2.x, point2.y, 100, p1);

        //            Invalidate();
        //            //test_zoomfactor += ((float)e.GetX() - lastPlace.x) / 100;
        //        }
        //        if (e.Action == MotionEventActions.Move )
        //        {
        //            if(e.PointerCount > 1)
        //            {

        //            }
        //            else
        //            {
        //                if (Zoom)
        //                {
        //                    test_zoomfactor += ((float)e.GetX() - lastPlace.x) / 100;
        //                }
        //                if (Move)
        //                {
        //                    camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
        //                    camera.CameraOffSetY += (float)e.GetY() - lastPlace.y;
        //                }
        //            }
        //        }
        //    }

        //    lastPlace = new MyPoint(e.GetX(), e.GetY());

        //    return true;
        //}

    //}
}