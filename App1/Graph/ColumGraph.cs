using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using App1.General_Classes;
using Firebase.Firestore.Model;
using Kotlin.Jvm.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App1
{
    internal class ColumGraph : Class_FatherGraph
    {
        //List<MyPoint> points = new List<MyPoint>();
        //List<MyPoint> Changedpoints = new List<MyPoint>();

        List<MySquare> Squares= new List<MySquare>();
        List<MySquare> ChangedSquares = new List<MySquare>();
        List<TextBlock> TextBlocks_Date = new List<TextBlock>();


        float squars_Width = 2;
        float distance = 2;

        float lowest = -1, highest = 0;
        bool doOnce = true;
        float textprice_margin = 0;
        //MyPoint lastPlace;
        //MyPoint lastPlace2;
        //float test_zoomfactor = 1;

        //MyPoint point1;
        //MyPoint point2;
        MyPoint midPoint;

        public bool Zoom = false;
        public bool Move = false;

        Paint p;
        Paint p1 = new Paint();

        Paint blue;
        Paint green;
        Paint purple;

        public ColumGraph(Context context) : base(context)
        {
            p = new Paint();
            p.Color = Color.Red;
            p.StrokeWidth = 6;

            background = new Paint();
            background.Color = Color.ParseColor("#333555");


            textPaint_Date = new Paint();
            textPaint_Date.Color = Color.Black;
            textPaint_Date.StrokeWidth = 2;
            textPaint_Date.TextSize = 60;
            textPaint_Date.TextAlign = Paint.Align.Center;

            textPaint_Price = new Paint();
            textPaint_Price.Color = Color.Black;
            textPaint_Price.StrokeWidth = 2;
            textPaint_Price.TextScaleX = (float)2.0;
            textPaint_Price.TextSize = 30;
            //Text  Paint_Y.TextAlign = Paint.Align.Center;

            green = new Paint();
            green.Color = Color.Green;
            green.StrokeWidth = 5;

            blue = new Paint();
            blue.Color = Color.Blue;
            blue.StrokeWidth = 5;

            purple  = new Paint();
            purple.Color = Color.Purple;
            purple.StrokeWidth = 5;
        }


        protected override void OnDraw(Canvas canvas1)
        {
            canvas = canvas1;
            if (dataPoints != null)
            {
                DoOnce();

                if (camera != null && (camera.X_changed || camera.Y_changed || camera.X_zoom_changed || camera.Y_zoom_changed))
                {
                    camera.X_zoom_changed = false;
                    camera.Y_zoom_changed = false;
                    camera.X_changed = false;
                    camera.Y_changed = false;
                    //Console.WriteLine("calculating new values for squers");
                    CalculateNewPointes();
                    ChangeInTextPlaceX(daltaOffsetX);
                    //ChangeInTextPlaceY(daltaOffsetY);


                }

                DrawGraph();
                DrawXexis();
                DrawTouching();
                Invalidate();
            }
        }

        private void DrawGraph()
        {
            
            DrawSquers();
        }

        private void DrawTouching()
        {
            p1.Color = Color.Black;
            if (midPoint != null)
            {
                canvas.DrawCircle(midPoint.x, midPoint.y, 10, p1);
            }
        }

        //public override bool OnTouchEvent(MotionEvent e)
        //{
        //    if (e.PointerCount > 1)
        //    {
        //        p1.Color = Color.Black;

        //        point1 = new MyPoint((float)e.GetX(), (float)e.GetY());
        //        point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));

        //        if (e.Action == MotionEventActions.Pointer2Down || e.Action == MotionEventActions.Down)
        //        {
        //            midPoint = new MyPoint((point1.x + point2.x) / 2, (point1.y + point2.y) / 2);
        //        }
        //    }
        //    if (e.Action == MotionEventActions.Up)
        //    {
        //        point1 = null;
        //        point2 = null;
        //        midPoint = null;
        //    }

        //    if (lastPlace == null)
        //    {
        //        lastPlace = new MyPoint(e.GetX(), e.GetY());
        //        return true;
        //    }
        //    else
        //    {
        //        if (e.Action == MotionEventActions.Move)
        //        {
        //            if (e.PointerCount > 1 && midPoint != null)
        //            {
        //                point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));
        //                if (lastPlace2 == null)
        //                {
        //                    lastPlace2 = new MyPoint(point2.x, point2.y);
        //                }
        //                test_zoomfactor += ((float)e.GetX() - lastPlace.x) / 100 + ((float)point2.x - lastPlace2.x) / 100;

        //                lastPlace2.x = point2.x;
        //                lastPlace2.y = point2.y;
        //            }
        //            else
        //            {
        //                //if (!(camera.CameraOffSetX + (float)e.GetX() - lastPlace.x >= 0))
        //                //{
        //                //    camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
        //                //}
        //                camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
        //                camera.CameraOffSetY += (float)e.GetY() - lastPlace.y;
        //                Console.WriteLine("The camera ofsetx is: " + camera.CameraOffSetX);
        //            }
        //        }
        //    }

        //    CalculateNewPointes();

        //    lastPlace.x = e.GetX();
        //    lastPlace.y = e.GetY();
        //    return true;


        //}


        private void DoOnce()
        {
            if (doOnce)
            {
                findLowHeigh();



                textPaint_Date.TextSize = canvas.Width / 18;
                price_text_start_x = canvas.Width - textPaint_Price.MeasureText("12.123") - textprice_margin;

                Paint.FontMetrics fm = textPaint_Date.GetFontMetrics();
                float height = fm.Bottom - fm.Top + fm.Leading; //height in pixels of text
                date_text_start_y = canvas.Height - (height + 5);

                CreateChartSquars();

                DrawXexis();
                ChangeInTextPlaceX((float)0.01);

                doOnce = false;
            }
        }

        public void findLowHeigh()
        {
            foreach (DataPoint i in dataPoints)
            {
                if (i.heigh > highest) highest = i.heigh;
                if (i.heigh < lowest || lowest == -1) lowest = i.heigh;
            }
        }

        public void CreateChartSquars()
        {
            if (Squares == null || Squares.Count == 0)
            {
                Squares = new List<MySquare>();
                ChangedSquares = new List<MySquare>();
                MyPoint UpLeft;
                MyPoint DownRight;
                squars_Width = 2;
                distance = 2;

                float x_point_0 = (0 * (price_text_start_x)) / ((dataPoints.Count - 1));
                float x_point_1 = (1 * (price_text_start_x)) / ((dataPoints.Count - 1));
                squars_Width = (float)0.9*x_point_1 - x_point_0; //temporery

                for (int i = 0; i < dataPoints.Count; i++)
                {
                    //points.Add(new MyPoint((i * canvas.Width) / (dataPoints.Count - 1), canvas.Height + ((lowest - values[i]) * (1 / (heighest - lowest)) * canvas.Height)));
                    //UpLeft = new MyPoint(i * (squars_Width + 1), canvas.Height- (dataPoints[i].heigh )* canvas.Height);
                    //UpLeft = new MyPoint(i * (squars_Width)+ i*distance, canvas.Height +  (lowest-dataPoints[i].heigh) *(1/(highest-lowest)) * canvas.Height);
                    //UpLeft = new MyPoint(i * (squars_Width) + i * distance, canvas.Height + ((lowest - dataPoints[i].heigh) * (1 / (highest - lowest)) * canvas.Height));
                    //UpLeft = new MyPoint((i * squars_Width + i * canvas.Width ) / (dataPoints.Count - 1 ), canvas.Height + ((lowest - dataPoints[i].heigh) * (1 / (highest - lowest)) * canvas.Height));

                   // UpLeft = new MyPoint((i*(canvas.Width-(squars_Width)) + i*2) / (dataPoints.Count - 1), canvas.Height + ((lowest - dataPoints[i].heigh) * (1 / (highest - lowest)) * canvas.Height));
                   //DownRight = new MyPoint(UpLeft.x+squars_Width, canvas.Height);

                    UpLeft = new MyPoint((i * (price_text_start_x) ) / ((dataPoints.Count - 1)), date_text_start_y + ((lowest - dataPoints[i].heigh) * (1 / (highest - lowest)) * date_text_start_y));
                    DownRight = new MyPoint(UpLeft.x + squars_Width,date_text_start_y);

                    Squares.Add(new MySquare(UpLeft,DownRight));
                }
                CalculateNewPointes();
            }
        }

        public void DrawSquers()
        {
            MyPoint UpLeft;
            MyPoint DownRight;
            for (int i = 0; i < dataPoints.Count; i++)
            {
                UpLeft = ChangedSquares[i].UpLeft;
                DownRight = ChangedSquares[i].DownRight;
                canvas.DrawRect(UpLeft.x,UpLeft.y,DownRight.x,DownRight.y,p);
            }
        }

        private void CalculateNewPointes()
        {
            MyPoint upLeft;
            MyPoint downRight;
            if (ChangedSquares.Count > 0)
            {
                ChangedSquares.Clear();
            }
            for (int i = 0; i < dataPoints.Count; i++)
            {
                //Changedpoints.Add(new MyPoint((points[i].x) * test_zoomfactor + camera.CameraOffSetX, points[i].y + camera.CameraOffSetY));
                //upLeft = new MyPoint(Squares[i].UpLeft.x * zoomfactor_X + camera.CameraOffSetX, Squares[i].UpLeft.y + camera.CameraOffSetY);
                //downRight = new MyPoint(upLeft.x + squars_Width * zoomfactor_X, canvas.Height+camera.CameraOffSetY);

                upLeft = new MyPoint(Squares[i].UpLeft.x * zoomfactor_X + camera.CameraOffSetX, Squares[i].UpLeft.y);
                downRight = new MyPoint(upLeft.x + squars_Width * zoomfactor_X, date_text_start_y);

                ChangedSquares.Add(new MySquare(upLeft, downRight));
            }
        }




        private void DrawXexis()
        {
            String TheString;
            float width = 0;
            canvas.DrawRect(0, canvas.Height - textPaint_Date.TextSize, canvas.Width, canvas.Height, background);

            if (TextBlocks_Date == null || TextBlocks_Date.Count == 0)
            {
                for (int i = 0; i < dataPoints.Count; i++)
                {

                    if (dataPoints.Count != 0)
                    {
                        TheString = dataPoints[i].date;
                        TheString = TheString.Remove(0, 10);
                        TextBlocks_Date.Add(new TextBlock(TheString, textPaint_Date, ChangedSquares[i].Center.x, canvas.Height));

                    }

                }
            }

            for (int i = 0; i < TextBlocks_Date.Count; i++)
            {

                if (TextBlocks_Date.Count != 0 && !TextBlocks_Date[i].Hidden)
                {
                    TheString = TextBlocks_Date[i].Text;
                    //TheString = TheString.Remove(0, 10);
                    canvas.DrawText(TheString, ChangedSquares[i].Center.x, canvas.Height, textPaint_Date);

                    //width = TextPaint_X.MeasureText(TheString);
                    //canvas.DrawCircle(Changedpoints[i].x - width / 2, TextBlocks[i].LeftDown.y, 5, red);
                    //canvas.DrawCircle(Changedpoints[i].x  + width / 2, TextBlocks[i].RightDown.y, 5, green);

                    canvas.DrawCircle(ChangedSquares[i].UpLeft.x, TextBlocks_Date[i].LeftDown.y, 5, blue);
                    canvas.DrawCircle(ChangedSquares[i].DownRight.x, TextBlocks_Date[i].RightDown.y, 5, green);
                    canvas.DrawCircle(ChangedSquares[i].Center.x, TextBlocks_Date[i].Center.y, 5, purple);

                }

            }

        }

        private void ChangeInTextPlaceX(float v)
        {
            int Anchor_place = 0;
            int place = 0;
            // Console.WriteLine(v);

            if (v != 0)
            {
                for (int i = 1; i < TextBlocks_Date.Count; i++)
                {
                    if (!TextBlocks_Date[i].Hidden)
                    {
                        float width = textPaint_Date.MeasureText(TextBlocks_Date[0].Text);
                        float RightX = ChangedSquares[0].DownRight.x;
                        float LeftX = ChangedSquares[0].UpLeft.x;

                        if (RightX >= LeftX)//hide text 
                        {
                            TextBlocks_Date[i].Hidden = true;
                            // Console.Write("i hid the text in place: ");
                            for (int g = i; g < TextBlocks_Date.Count; g += 2 * i)
                            {
                                TextBlocks_Date[g].Hidden = true;
                                //Console.Write(g + " ");
                            }
                            // Console.WriteLine("  ");
                            ChangeInTextPlaceX((float)0.001);
                            return;
                        }

                        float distance = LeftX - RightX;
                        if (distance > width)//reavel hidden text 
                        {
                            place = (i + 1) / 2; //rounding up 
                            for (int g = place; g < TextBlocks_Date.Count; g += i)
                            {
                                TextBlocks_Date[g].Hidden = false;
                            }
                            return;
                        }
                    }
                }
            }
        }
       
        private int Calculate_Original_Point_I(float x)
        {
            double defualtPointx = (x - camera.CameraOffSetX) / zoomfactor_X;
            double i_d = ((defualtPointx * (Squares.Count))) / (price_text_start_x);//i_d => i double
            int i = (int)Math.Round(i_d);
            return i;
        }

        private int Calculate_Original_Point_I_without_rounding(float x)
        {
            double defualtPointx = (x - camera.CameraOffSetX) / zoomfactor_X;
            double i_d = ((defualtPointx * (Squares.Count))) / (price_text_start_x); //i_d => i double
            int i = (int)(i_d);
            return i;
        }


    }
}