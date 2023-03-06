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
using static System.Net.Mime.MediaTypeNames;

namespace App1
{
    public class Class_LineGraph : Class_FatherGraph
    {
        ///
        List<float> values = new List<float>();
        float heighest = 0, lowest = -1;



        List<MyPoint> points = new List<MyPoint>();
        List<MyPoint> Changedpoints = new List<MyPoint>();
        List<TextBlock> TextBlocks = new List<TextBlock>();

        MyCamera camera = new MyCamera(0, 0);

        MyPoint lastPlace;
        MyPoint lastPlace2;
        float test_zoomfactor = 1;


        public bool Zoom = false;
        public bool Move = false;

        Paint p1 = new Paint();
        MyPoint point1;
        MyPoint point2;
        MyPoint midPoint;

        Paint TextPaint_X;
        Paint p;
        Paint red;
        Paint green;
        Paint black;

        public Class_LineGraph(Context context) : base(context) 
        {
            p = new Paint();
            p.Color= Color.Red;
            p.StrokeWidth= 6;

            black = new Paint();
            black.Color= Color.ParseColor("#333555");

            red = new Paint();
            red.Color = Color.Red;
            
            green = new Paint();
            green.Color = Color.Green;

            TextPaint_X = new Paint();
            TextPaint_X.Color = Color.Black; 
            TextPaint_X.StrokeWidth= 2;
            TextPaint_X.TextSize = 60;
            TextPaint_X.TextAlign= Paint.Align.Center;
            
        }

        public Class_LineGraph(Context context, Canvas canvas, List<DataPoint> dataPoints) : base(context,canvas,dataPoints) { }

        protected override void OnDraw(Canvas canvas1)
        {
            canvas = canvas1;
            if (dataPoints != null)
            {
                doOnce();

                DrawGraph();
                DrawTouching();
                DrawXexis();

                Invalidate();
            }
        }

        public void doOnce()
        {
            if (values == null || values.Count == 0) { calculateValues(); }
            if (heighest == 0) { findLowHeigh(); }

            

        }

        private void DrawGraph()
        {
            CreateChartPoints();
            DrawPoints();
        }

        private void calculateValues()
        {
            foreach (DataPoint i in dataPoints)
            {
                float avr = (i.low + i.heigh);
                values.Add(avr);
            }
        }
        public void findLowHeigh()
        {
            foreach (float i in values)
            {
                if (i > heighest) heighest = i;
                if (i < lowest || lowest == -1) lowest = i;
            }
        }
        public void CreateChartPoints()
        {
            if (points == null || points.Count == 0)
            {
                points = new List<MyPoint>();
                Changedpoints = new List<MyPoint>();

                for (int i = 0; i < values.Count; i++)
                {
                    points.Add(new MyPoint(((i  * canvas.Width) * (float)(9.0 / 10.0)) / (values.Count - 1), canvas.Height * 19/20 + ((lowest - values[i]) * (1 / (heighest - lowest)) * canvas.Height * 19 / 20)));
                }
                CalculateNewPointes();
            }
        }

        private void CalculateNewPointes()
        {
            Changedpoints.Clear();
            for (int i = 0; i < values.Count; i++)
            {
                Changedpoints.Add( new MyPoint((points[i].x) * test_zoomfactor + camera.CameraOffSetX, points[i].y + camera.CameraOffSetY));
            }
        }

        public void DrawPoints()
        {
            for (int i = 0; i < values.Count; i++)
            {
                canvas.DrawCircle(Changedpoints[i].x, Changedpoints[i].y, 2, p);
                if (i != values.Count - 1)
                {
                    //canvas.DrawLine(Changedpoints[i].x, Changedpoints[i].y, Changedpoints[i + 1].x, Changedpoints[i + 1].y, p);
                }
            }
        }


        private void DrawXexis()
        {
            String TheString;
            float width = 0;
            canvas.DrawRect(0,canvas.Height-TextPaint_X.TextSize,canvas.Width,canvas.Height,black);

            if (TextBlocks == null || TextBlocks.Count == 0)
            {
                for (int i = 0; i < dataPoints.Count; i++)
                {

                    if (dataPoints.Count != 0)
                    {
                        TheString = dataPoints[i].date;
                        TheString = TheString.Remove(0, 10);
                        TextBlocks.Add(new TextBlock(TheString, TextPaint_X, Changedpoints[i].x, canvas.Height));

                    }

                }
            }

            for (int i = 0; i < TextBlocks.Count; i++)
            {

                if (TextBlocks.Count != 0 && !TextBlocks[i].Hidden)
                {
                    TheString = TextBlocks[i].Text;
                    //TheString = TheString.Remove(0, 10);
                    canvas.DrawText(TheString, Changedpoints[i].x, canvas.Height, TextPaint_X);

                    width = TextPaint_X.MeasureText(TheString);


                    //canvas.DrawCircle(TextBlocks[i].LeftDown.x * test_zoomfactor + camera.CameraOffSetX, TextBlocks[i].LeftDown.y, 5, red);
                    //canvas.DrawCircle(TextBlocks[i].RightDown.x * test_zoomfactor + camera.CameraOffSetX, TextBlocks[i].RightDown.y, 5, green);

                    //canvas.DrawCircle(TextBlocks[i].LeftDown.x * test_zoomfactor + camera.CameraOffSetX - width / 2, TextBlocks[i].LeftDown.y, 5, red);
                    canvas.DrawCircle(Changedpoints[i].x - width / 2, TextBlocks[i].LeftDown.y, 5, red);
                    canvas.DrawCircle(Changedpoints[i].x  + width / 2, TextBlocks[i].RightDown.y, 5, green);

                }

            }

        }



        private int CalculatePointZoomingOn()
        {
            double defualtPointx = (midPoint.x - camera.CameraOffSetX) / test_zoomfactor;
            //double defualtPointx = ((midPoint.x ) / test_zoomfactor ) - camera.CameraOffSetX;
           // Console.WriteLine("defalt x ix :" + defualtPointx);

            //float x = (i * 9 / 10 * canvas.Width) / (values.Count - 1);
            double g = ((values.Count - 1) * midPoint.x * 10) / (9  * canvas.Width);

            double defualtI = (defualtPointx * (points.Count - 1)) / canvas.Width;
            //float defualtI = ((midPoint.x) * (points.Count - 1)) / (canvas.Width );
            //Console.WriteLine("mid point x:" + midPoint.x);

            //float i = (((midPoint.x - camera.CameraOffSetX) / test_zoomfactor) / (canvas.Width / (points.Count - 1)));
            float i = (((midPoint.x / test_zoomfactor) - camera.CameraOffSetX) / (canvas.Width / (points.Count - 1)));
            //float i = ((((midPoint.x / test_zoomfactor) - camera.CameraOffSetX) * (points.Count - 1) )/ (canvas.Width));

            double itest = (defualtPointx * (values.Count - 1) ) / ( canvas.Width);
            itest = itest * (10 / 9);


           // Console.WriteLine("dedualtI is: " + defualtI);
            //Console.WriteLine("i is: " + i);
            //Console.WriteLine("itest is: " + itest);
            //Console.WriteLine("g is: " + g);

            //Console.WriteLine("the soposed x from the calculation is: " + ((midPoint.x - camera.CameraOffSetX) / test_zoomfactor));
            //Console.WriteLine("the soposed x from the calculation is: " + ((midPoint.x ) / test_zoomfactor)- camera.CameraOffSetX);



            //double changed_x = midPoint.x * test_zoomfactor + camera.CameraOffSetX;
            //double def_x = (changed_x - camera.CameraOffSetX) / test_zoomfactor;
            double itest2 = ((defualtPointx * (points.Count))*20) / (canvas.Width*18);
           // Console.WriteLine("itest2 : " + itest2);


            int defualtI2 = (int)Math.Round(defualtI);
            //int i2 = (int)Math.Round(i);
            //int i2 = (int)Math.Round(itest);
            int i2 = (int)Math.Round(itest2);
            //return defualtI2;
            return i2;
        }
        private void DrawTouching()
        {
            p1.Color = Color.Black;
            if (point1 != null && point2 != null && midPoint != null)
            {
                canvas.DrawCircle(point1.x, point1.y, 100, p1);
                canvas.DrawCircle(point2.x, point2.y, 100, p1);
                canvas.DrawCircle(midPoint.x, midPoint.y, 10, p1);

                int i = CalculatePointZoomingOn();

                if (0 <= i && i < points.Count)
                {
                    canvas.DrawCircle(Changedpoints[i].x, Changedpoints[i].y, 10, p1);
                }
                //Console.WriteLine("mispoint x is: " + midPoint.x);
                //Console.WriteLine("changed x is: " + Changedpoints[i].x);
                //Console.WriteLine("changed-1 x is: " + Changedpoints[i-1].x);
                //Console.WriteLine("changed-2 x is: " + Changedpoints[i-2].x);
                //Console.WriteLine("changed-3 x is: " + Changedpoints[i-3].x);
                //Console.WriteLine("changed-4 x is: " + Changedpoints[i-4].x);


                p1.Color = Color.Blue;
                //canvas.DrawCircle(Changedpoints[i-1].x, Changedpoints[i].y, 10, p1);
                p1.Color = Color.Green;
                //canvas.DrawCircle(Changedpoints[i+1].x, Changedpoints[i].y, 10, p1);
            }
        }
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (e.PointerCount > 1)
            {
                p1.Color = Color.Black;

                point1 = new MyPoint((float)e.GetX(), (float)e.GetY());
                point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));

                if (e.Action == MotionEventActions.Pointer2Down || e.Action == MotionEventActions.Down)
                {
                    midPoint = new MyPoint((point1.x + point2.x) / 2, (point1.y + point2.y) / 2);
                }
            }
            if (e.Action == MotionEventActions.Up)
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
                    if (e.PointerCount > 1 && midPoint != null)
                    {
                        point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));
                        if (lastPlace2 == null)
                        {
                            lastPlace2 = new MyPoint(point2.x, point2.y);
                        }


                        //test_zoomfactor += Math.Max((Math.Abs((float)e.GetX() - midPoint.x) / 1000),Math.Abs( ((float)point2.x - midPoint.x) / 1000));

                        //if(Math.Abs((float)e.GetX() - lastPlace.x) > Math.Abs((float)point2.x - lastPlace2.x))
                        //{
                        //    test_zoomfactor += ((float)e.GetX() - lastPlace.x) / 100;
                        //}
                        //else
                        //{
                        //    test_zoomfactor += ((float)point2.x - lastPlace2.x) / 100;
                        //}

                        test_zoomfactor += ((float)e.GetX() - lastPlace.x) / 100;// + ((float)point2.x - lastPlace2.x) / 100;
                        ChangeInTextPlace((float)e.GetX() - lastPlace.x);


                        


                        lastPlace2.x = point2.x;
                        lastPlace2.y = point2.y;

                    }
                    else
                    {
                        if (Zoom)
                        {
                            test_zoomfactor += ((float)e.GetX() - lastPlace.x) / 100;
                        }
                        if (Move)
                        {
                            if (!(camera.CameraOffSetX + (float)e.GetX() - lastPlace.x >= 0))
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

        private void ChangeInTextPlace(float v)
        {
            int Anchor_place = 0;
            int place = 0;
            if (v > 0)
            {
                for(int i = 1; i < TextBlocks.Count; i++)
                {
                    if (!TextBlocks[i].Hidden)
                    {
                        float width = TextPaint_X.MeasureText(TextBlocks[0].Text);
                        float RightX = Changedpoints[0].x + width / 2;
                        float LeftX = Changedpoints[i].x - width / 2;


                        float distance = LeftX - RightX;
                        if(distance > width)
                        {
                            place = (i+1) / 2; //rounding up 
                            for (int g = place; g < TextBlocks.Count; g +=  i)
                            {
                                TextBlocks[g].Hidden = false;
                            }
                            return;
                        }

                    }
                }
            }
            else
            {
                for (int i = 1; i < TextBlocks.Count; i++)
                {
                    if (!TextBlocks[i].Hidden)
                    {
                        float width = TextPaint_X.MeasureText(TextBlocks[0].Text);
                        float RightX = Changedpoints[0].x + width/2;
                        float LeftX = Changedpoints[i].x - width / 2;



                        if(RightX >= LeftX)
                        {
                            TextBlocks[i].Hidden = true;
                            for(int g = i; g < TextBlocks.Count; g += 2*i) 
                            {
                                TextBlocks[g].Hidden = true;
                            }
                            return;
                        }



                    }
                }
            }
        }
    }
}