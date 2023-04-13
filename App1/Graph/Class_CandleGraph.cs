using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using App1.General_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Android.Service.Notification.NotificationListenerService;
using System.Threading;

namespace App1
{
    public class Class_CandleGraph : Class_FatherGraph
    {
        List<MySquare> Squares = new List<MySquare>();
        List<MySquare> ChangedSquares = new List<MySquare>();
        List<TextBlock> TextBlocks_Date = new List<TextBlock>();
        List<TextBlock> TextBlocks_Price = new List<TextBlock>();

        float squars_Width = 2;
        float distance = 2;

        float lowest = -1, highest = 0;
        bool doOnce = true;
        float textprice_margin = 0;
        MyPoint midPoint;

        public bool Zoom = false;
        public bool Move = false;
        public bool running = false;
        Paint p;
        Paint p1 = new Paint();

        Paint orange;
        Paint blue;
        Paint Dred;
        Paint green;
        Paint purple;

        public Class_CandleGraph(Context context) : base(context)
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

            orange= new Paint();
            orange.Color = Color.Orange;
            orange.StrokeWidth = 2;

            green = new Paint();
            green.Color = Color.Green;
            green.StrokeWidth = 5;

            blue = new Paint();
            blue.Color = Color.Blue;
            blue.StrokeWidth = 5;

            Dred = new Paint();
            Dred.Color = Color.DarkRed;
            Dred.StrokeWidth = 5;

            purple = new Paint();
            purple.Color = Color.Purple;
            purple.StrokeWidth = 5;

            //running = true;
            //ThreadStart MyThreadStart = new ThreadStart(PrintChangedSquares);
            //Thread t = new Thread(MyThreadStart);
            //t.Start();
        }

        public void PrintChangedSquares()
        {
            float Lx = -1;
            float Ly = -1;
            float Rx = -1;
            float Ry = -1;
            float high = 0;
            float low = 0;
            float close = 0;
            while (running)
            {
                Thread.Sleep(10000);
                for (int i = 0; i < ChangedSquares.Count; i++)
                {
                    Lx = ChangedSquares[i].UpLeft.x;
                    Ly = ChangedSquares[i].UpLeft.y;
                    Rx = ChangedSquares[i].DownRight.x;
                    Ry = ChangedSquares[i].DownRight.y;
                    high = dataPoints[i].heigh;
                    low = dataPoints[i].low;
                    close = dataPoints[i].close;
                    Console.Write("i = " + i + " , LeftUp:(" + Lx + "," + Ly + ")    DownRight:(" + Rx + "," + Ry + ")");
                    Console.WriteLine(" close: " + close + " , low: " + low + " , high: " + high);
                }
                running= false;
            }
            
            
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

                ChangeInTextPlaceY((float)0.01);

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

      
        private void DoOnce()
        {
            if (doOnce)
            {
                findLowHeigh();

                textPaint_Date.TextSize = canvas.Width / 18;
                price_text_start_x = canvas.Width - textPaint_Price.MeasureText("12.123") - textprice_margin;

                Paint.FontMetrics fm = textPaint_Date.GetFontMetrics();
                float height = fm.Bottom - fm.Top + fm.Leading;
                date_text_start_y = canvas.Height - (height + 5);

                CreateChartSquars();

                DrawYexis();
                DrawXexis();
                ChangeInTextPlaceX((float)0.01);
                ChangeInTextPlaceY((float)0.01);

                doOnce = false;
            }
        }

        public void findLowHeigh()
        {
            foreach (DataPoint i in dataPoints)
            {
                if (i.heigh > highest) highest = i.heigh;
                if (i.low < lowest || lowest == -1) lowest = i.low;
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
                squars_Width = (float)0.9 * x_point_1 - x_point_0; //temporery

                for (int i = 0; i < dataPoints.Count; i++)
                {

                    UpLeft = new MyPoint((i * (price_text_start_x)) / ((dataPoints.Count - 1)), date_text_start_y + ((lowest - dataPoints[i].heigh) * (1 / (highest - lowest)) * date_text_start_y));
                    DownRight = new MyPoint(UpLeft.x + squars_Width, date_text_start_y + ((lowest - dataPoints[i].low) * (1 / (highest - lowest)) * date_text_start_y));

                    Squares.Add(new MySquare(UpLeft, DownRight));
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
                if (UpLeft.y == DownRight.y)
                {
                    canvas.DrawLine(UpLeft.x, UpLeft.y, DownRight.x, DownRight.y, orange);
                }
                if(i == 0)
                {
                    canvas.DrawRect(UpLeft.x, UpLeft.y, DownRight.x, DownRight.y, orange);
                }
                else if (dataPoints[i].close> dataPoints[i - 1].close)
                {
                    canvas.DrawRect(UpLeft.x, UpLeft.y, DownRight.x, DownRight.y, green);
                }
                else
                {
                   canvas.DrawRect(UpLeft.x, UpLeft.y, DownRight.x, DownRight.y, p);
                }
                
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

                upLeft = new MyPoint(Squares[i].UpLeft.x * zoomfactor_X + camera.CameraOffSetX, Squares[i].UpLeft.y+camera.CameraOffSetY);
                downRight = new MyPoint(upLeft.x + squars_Width * zoomfactor_X, Squares[i].DownRight.y +camera.CameraOffSetY);

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
                    TheString = dataPoints[i].date;
                    TheString = TheString.Remove(0, 10);
                    TextBlocks_Date.Add(new TextBlock(TheString, textPaint_Date, ChangedSquares[i].Center.x, canvas.Height));
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
        private void DrawYexis()
        {
            String TheString;
            float width = 0;

            if (TextBlocks_Price == null || TextBlocks_Price.Count == 0)
            {
                for (int i = 0; i < dataPoints.Count; i++)
                {

                    if (dataPoints.Count != 0)
                    {
                        TheString = "" + dataPoints[i].close;
                        width = textPaint_Price.MeasureText(TheString);
                        //TextBlocks_Y.Add(new TextBlock(TheString, TextPaint_Y, canvas.Width * (float)(18.0 / 20) + 2*width, Changedpoints[i].y));
                        TextBlocks_Price.Add(new TextBlock(TheString, textPaint_Price, price_text_start_x + (width / 2), ChangedSquares[i].Center.y));
                    }

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
                        float RightX = ChangedSquares[0].Center.x + width / (float)2.0;
                        float LeftX = ChangedSquares[i].Center.x - width / (float)2.0;

                        if (RightX >= LeftX - width / 10.0)//hide text 
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

        private void ChangeInTextPlaceY(float v)
        {
            //
            //canvas.DrawRect(date_text_start_y, 0, canvas.Width, canvas.Height, background);
            canvas.DrawRect(price_text_start_x, 0, canvas.Width, canvas.Height, background);
            int Anchor_place = 0;
            int place = 0;


            //pre-theory: 
            //
            //1) find lowest on screen point
            //2) find heighst on screnn point 
            //3)take their values
            //4)the values between the two points on the y acssis shold be the range of prices showen in the y=exies

            //thoery 1:
            //the "anchor"/absolot value is the right most point on screen
            // than showes the neerest price thats is 1 height of text from him (on the y exies)
            //      than calculate the dividance between lowest and heighest point on screen
            // if there is no point at exectly 1 height of text from the point (upword or downword):  --- maybe uneccery 
            //      to calculate the value of 1 height text in reffrence to the value of canvas height in the current zoom(value of canvas height =  dividance between lowest and heighest point on screen)

            //thoery 2:
            //the "anchor"/absolot value is the right most point on screen
            // than showes the neerest price thats is 1 height of text from him (on the y exies)
            //than calculate the dividance between lowest and heighest point on screen
            //to calculate the value of 1 height text in reffrence to the value of canvas height in the current zoom(value of canvas height =  dividance between lowest and heighest point on screen)

            //practice 
            if (v != 0 && dataPoints.Count != 0) // change in y exies
            {




                int Leftest_pointI = Calculate_Original_Point_I(0);//the left most visibal point on the screen
                int Rightest_pointI = Calculate_Original_Point_I_without_rounding(price_text_start_x);//the right most visibale point on the screen



                if (Leftest_pointI >= dataPoints.Count - 1)
                {
                    Leftest_pointI = dataPoints.Count - 2;
                }
                if (Rightest_pointI >= dataPoints.Count)
                {
                    Rightest_pointI = dataPoints.Count - 1;

                }

                float heighest = dataPoints[Leftest_pointI].close;//just putting some values so i can compere with other values to find the max and min 
                float lowest = dataPoints[Leftest_pointI].close;//---
                float value_of_rightest = dataPoints[Rightest_pointI].close;
                float value_of_canvasHeight = 0;
                float l_hieght = 0;
                float h_hieght = 0;

                for (int i = Leftest_pointI; i <= Rightest_pointI; i++)//getting the heighest and lowest price on screen
                {
                    if (dataPoints != null & dataPoints.Count > i && dataPoints[i].close < lowest)
                    {
                        lowest = dataPoints[i].close;
                        l_hieght = ChangedSquares[i].DownRight.y;
                    }
                    else if (dataPoints != null & dataPoints.Count > i && dataPoints[i].close > heighest)
                    {
                        heighest = dataPoints[i].close;
                        h_hieght = ChangedSquares[i].UpLeft.y;
                    }
                }

                //value_of_canvasHeight = heighest - lowest; //value of the canvas height
                //float value_per_pixel = value_of_canvasHeight / canvas.Height;


                float value_per_pixel = (heighest - lowest) / (Math.Abs(h_hieght - l_hieght));
                value_of_canvasHeight = value_per_pixel * canvas.Height; //value of the canvas height


                float distance_from_roof_of_canvas = ChangedSquares[Rightest_pointI].UpLeft.y;
                float distance_from_floor_of_canvas = canvas.Height - distance_from_roof_of_canvas;
                float price = 0;

                string TheString = "" + value_of_rightest;
                float width = textPaint_Price.MeasureText(TheString);


                Rect bounds = new Rect();
                textPaint_Price.GetTextBounds(TheString, 0, TheString.Length, bounds);

                canvas.DrawCircle(ChangedSquares[Rightest_pointI].Center.x, ChangedSquares[Rightest_pointI].Center.y, 5, green);


                Paint.FontMetrics fm = textPaint_Price.GetFontMetrics();
                //float height = fm.Descent - fm.Ascent;
                float height = fm.Bottom - fm.Top + fm.Leading; //height in pixels of text
                string the_string = "";
                MySquare Anchor = new MySquare();
                //generete text-price below the furthest-right point
                for (int i = 0; i < (distance_from_floor_of_canvas / height) + 1; i++)
                {
                    price = value_of_rightest - i * (height * value_per_pixel);
                    price = (int)(price * 1000);
                    price = price / 1000;

                    the_string = "" + price;
                    if (i == 0)
                    { 
                        if (dataPoints[Rightest_pointI].close > dataPoints[Rightest_pointI - 1].close)
                        {
                            //canvas.DrawRect(text_start_y, TextBlocks_Y[Rightest_pointI].LeftDown.y - (height / 2), canvas.Width, TextBlocks_Y[Rightest_pointI].LeftDown.y + (height / 2), green);
                            canvas.DrawRect(price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y - (height / 2), canvas.Width, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 2), green);
                            Anchor = new MySquare(price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y - (height / 2), canvas.Width, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 2));
                        }
                        else
                        {
                            // canvas.DrawRect(text_start_y, TextBlocks_Y[Rightest_pointI].LeftDown.y - (height / 2), canvas.Width, TextBlocks_Y[Rightest_pointI].LeftDown.y + (height / 2), red);
                            canvas.DrawRect(price_text_start_x, ChangedSquares[Rightest_pointI].DownRight.y - (height / 2), canvas.Width, ChangedSquares[Rightest_pointI].DownRight.y + (height / 2), Dred);
                            Anchor = new MySquare(price_text_start_x, ChangedSquares[Rightest_pointI].DownRight.y - (height / 2), canvas.Width, ChangedSquares[Rightest_pointI].DownRight.y + (height / 2));

                        }
                    }
                    //canvas.DrawText(the_string, text_start_y, TextBlocks_Y[Rightest_pointI].LeftDown.y + (height / 4) + (i * height), TextPaint_Y);
                    //canvas.DrawText(the_string, price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 4) + (i * height), textPaint_Price);
                    canvas.DrawText(the_string, price_text_start_x,Anchor.DownRight.y - (height / 4) + (i * height), textPaint_Price);
                }

                //genereting text-price above the furthest-right point
                for (int i = 0; i < (int)((distance_from_roof_of_canvas / height) + 1); i++)
                {
                    if (((distance_from_roof_of_canvas / height) + 1) < 2)
                    {
                        i = 1;
                    }
                    price = value_of_rightest + i * (height * value_per_pixel);
                    price = (int)(price * 1000);
                    price = price / 1000;
                    the_string = "" + price;
                    //canvas.DrawText(the_string, text_start_y, TextBlocks_Y[Rightest_pointI].LeftDown.y + (height / 4) - (i * height), TextPaint_Y);
                    //canvas.DrawText(the_string, price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 4) - (i * height), textPaint_Price);
                    if (Anchor != null && Anchor.DownRight != null )
                        canvas.DrawText(the_string, price_text_start_x, Anchor.DownRight.y - (height / 4) - (i * height), textPaint_Price);
                }





            }
        }





        private int Calculate_Original_Point_I(float x)
        {
            double defualtPointx = (x - camera.CameraOffSetX) / zoomfactor_X;
            double i_d = ((defualtPointx * (Squares.Count - 1))) / (price_text_start_x);//i_d => i double
            int i = (int)Math.Round(i_d);
            return i;
        }

        private int Calculate_Original_Point_I_without_rounding(float x)
        {
            double defualtPointx = (x - camera.CameraOffSetX) / zoomfactor_X;
            double i_d = ((defualtPointx * (Squares.Count - 1))) / (price_text_start_x); //i_d => i double
            int i = (int)(i_d);
            return i;
        }



    }
}