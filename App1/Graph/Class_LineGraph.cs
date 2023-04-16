using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Firebase.Firestore.Model;
using Org.W3c.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        List<TextBlock> TextBlocks_Y = new List<TextBlock>();


        bool running = true;
        float frame_rate = (float)(1.0 / 2);

        float text_start_y;
        float text_margin_y = 0;//30px

        bool doingOnce = true;

        public bool Zoom = false;
        public bool Move = false;

        Paint p1 = new Paint();

        Paint TextPaint_X;
        Paint TextPaint_Y;


        Paint p;
        Paint red;
        Paint green;
        
        Paint black;
        Paint r;
        Paint g;

        public Class_LineGraph(Context context) : base(context) 
        {


            p = new Paint();
            p.Color= Color.Red;
            p.StrokeWidth= 6;

            background = new Paint();
            background.Color = Color.ParseColor("#333555");

            black = new Paint();
            black.Color = Color.Black;

            red = new Paint();
            red.Color = Color.Red;
            
            green = new Paint();
            green.Color = Color.Green;


            TextPaint_X = new Paint();
            TextPaint_X.Color = Color.Black; 
            TextPaint_X.StrokeWidth= 2;
            TextPaint_X.TextSize = 60;
            TextPaint_X.TextAlign= Paint.Align.Center;

            TextPaint_Y = new Paint();
            TextPaint_Y.Color = Color.Black;
            TextPaint_Y.StrokeWidth = 2;
            TextPaint_Y.TextScaleX = (float)2.0;
            TextPaint_Y.TextSize = 30;

            r = new Paint();
            r.Color = Color.Red;
            r.StrokeWidth = 5;

            g = new Paint();
            g.Color = Color.Green;
            g.StrokeWidth = 5;

           // start_Refresh_Thread();



        }

        public void start_Refresh_Thread()
        {
            running = true;
            ThreadStart MyThreadStart = new ThreadStart(RefreshInvalidate);
            Thread t = new Thread(MyThreadStart);
            t.Start();
        }

        private void RefreshInvalidate()
        {
            while (running)
            {
                Invalidate();
                Thread.Sleep((int)frame_rate * 1000);
            }
            return;
        }

        public Class_LineGraph(Context context, Canvas canvas, List<DataPoint> dataPoints) : base(context,canvas,dataPoints) { }

        protected override void OnDraw(Canvas canvas1)
        {
            canvas = canvas1;
            if (dataPoints != null)
            {
                if (doingOnce)
                {
                    doOnce();
                }
                    
                DrawGraph();
                
                if(camera!= null && (camera.X_changed || camera.Y_changed || camera.X_zoom_changed || camera.Y_zoom_changed)) 
                {
                    camera.X_zoom_changed = false;
                    camera.Y_zoom_changed = false;
                    camera.X_changed = false;
                    camera.Y_changed = false;

                    CalculateNewPointes();
                    ChangeInTextPlaceX(daltaOffsetX);


                }

                DrawXexis();
                ChangeInTextPlaceY((float)0.01);

                DrawTouching();

                Invalidate();
            }
        }

       
        public void doOnce()
        {
            if (values == null || values.Count == 0) { calculateValues(); }
            if (heighest == 0) { findLowHeigh(); }


            TextPaint_X.TextSize = canvas.Width / 18;
            text_start_y = canvas.Width - TextPaint_Y.MeasureText("12.123") - text_margin_y;


            if (points == null || points.Count == 0)
            {
                CreateChartPoints();
            }
            DrawXexis();
            DrawYexis();

            ChangeInTextPlaceX((float)0.001);
            ChangeInTextPlaceY((float)-0.01);
            doingOnce = false;
        }

        private void DrawGraph()
        {
            DrawPoints();
        }
        public void DrawPoints()
        {
            

            for (int i = 0; i < values.Count; i++)
            {
                if (i != values.Count - 1)
                {
                    if (i < values.Count-1)
                    {
                        if (values[i] < values[i +1])
                        {
                            canvas.DrawLine(Changedpoints[i].x, Changedpoints[i].y, Changedpoints[i + 1].x, Changedpoints[i + 1].y, g);
                        }
                        else
                        {
                            canvas.DrawLine(Changedpoints[i].x, Changedpoints[i].y, Changedpoints[i + 1].x, Changedpoints[i + 1].y, r);
                        }
                    }

                }
            }
        }
        private void calculateValues()
        {
            foreach (DataPoint i in dataPoints)
            {
                values.Add(i.close);
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

                    points.Add(new MyPoint(i * (text_start_y) / (values.Count - 1), canvas.Height * 19 / 20 + ((lowest - values[i]) * (1 / (heighest - lowest)) * canvas.Height * 19 / 20)));
                }
                CalculateNewPointes();
            }
        }
        private void CalculateNewPointes()
        {
            Changedpoints.Clear();
            for (int i = 0; i < values.Count; i++)
            {

                Changedpoints.Add(new MyPoint((points[i].x) * zoomfactor_X + camera.CameraOffSetX, points[i].y * zoomfactor_Y + camera.CameraOffSetY));
                
            }
            
        }

        

        //text functions
        private void DrawXexis()
        {
            String TheString;
            float width = 0;
            canvas.DrawRect(0,canvas.Height-TextPaint_X.TextSize,canvas.Width,canvas.Height, background);

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
                    canvas.DrawText(TheString, Changedpoints[i].x, canvas.Height, TextPaint_X);


                }

            }

        }
        private void DrawYexis()
        {
            String TheString;
            float width = 0;

            if (TextBlocks_Y == null || TextBlocks_Y.Count == 0)
            {
                for (int i = 0; i < dataPoints.Count; i++)
                {

                    if (values.Count != 0)
                    {
                        TheString = "" + values[i];
                        width = TextPaint_Y.MeasureText(TheString);
                        TextBlocks_Y.Add(new TextBlock(TheString, TextPaint_Y, text_start_y+(width/2), Changedpoints[i].y));
                    }

                }
            }

        }
        private void ChangeInTextPlaceX(float v)
        {
            int Anchor_place = 0;
            int place = 0;


            if (v != 0) 
            {
                for (int i = 1; i < TextBlocks.Count; i++)
                {
                    if (!TextBlocks[i].Hidden)
                    {
                        float width = TextPaint_X.MeasureText(TextBlocks[0].Text);
                        float RightX = Changedpoints[0].x + width / 2;
                        float LeftX = Changedpoints[i].x - width / 2;

                        if (RightX >= LeftX)//hide text 
                        {
                            TextBlocks[i].Hidden = true;

                            for (int g = i; g < TextBlocks.Count; g += 2 * i)
                            {
                                TextBlocks[g].Hidden = true;

                            }

                            ChangeInTextPlaceX((float)0.001);
                            return;
                        }

                        float distance = LeftX - RightX;
                        if (distance > width)//reavel hidden text 
                        {
                            place = (i + 1) / 2; //rounding up 
                            for (int g = place; g < TextBlocks.Count; g += i)
                            {
                                TextBlocks[g].Hidden = false;
                            }
                            return;
                        }
                    }
                }
            }
        }
        private void ChangeInTextPlaceY(float v)
        {
            canvas.DrawRect(text_start_y, 0, canvas.Width, canvas.Height, background);
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
            // if there is no point at exectly 1 height of text from the point (upword or downword):  --- maybe uneccery 
            //      than calculate the dividance between lowest and heighest point on screen
            //      to calculate the value of 1 height text in reffrence to the value of canvas height in the current zoom(value of canvas height =  dividance between lowest and heighest point on screen)

            //thoery 2:
            //the "anchor"/absolot value is the right most point on screen
            // than showes the neerest price thats is 1 height of text from him (on the y exies)
            //than calculate the dividance between lowest and heighest point on screen
            //to calculate the value of 1 height text in reffrence to the value of canvas height in the current zoom(value of canvas height =  dividance between lowest and heighest point on screen)

            //practice 
            if (v != 0 && values.Count!=0) // change in y exies
            {

                


                int Leftest_pointI = Calculate_Original_Point_I(0);//the left most visibal point on the screen
                int Rightest_pointI = Calculate_Original_Point_I_without_rounding(text_start_y);//the right most visibale point on the screen

                if (Leftest_pointI >= values.Count-1)
                {
                    Leftest_pointI = values.Count - 2;
                }
                if (Rightest_pointI >= values.Count)
                {
                    Rightest_pointI = values.Count - 1;
                    
                }

                float heighest = values[Rightest_pointI];//just putting some values so i can compere with other values to find the max and min 
                float lowest = values[Leftest_pointI];//---
                float value_of_rightest = values[Rightest_pointI];
                float value_of_canvasHeight = 0;

                float l_hieght = Changedpoints[Leftest_pointI].y;
                float h_hieght = Changedpoints[Rightest_pointI].y;


                Paint.FontMetrics fm = TextPaint_Y.GetFontMetrics();
                float height = fm.Bottom - fm.Top + fm.Leading; //height in pixels of text


                for (int i = Leftest_pointI; i <= Rightest_pointI; i++)//getting the heighest and lowest price on screen
                {
                    if (values != null & values.Count>i && values[i]< lowest)
                    {
                        lowest = values[i];
                        l_hieght = Changedpoints[i].y;
                    }
                    else if (values != null & values.Count > i && values[i]> heighest)
                    {
                        heighest = values[i];
                        h_hieght = Changedpoints[i].y;
                    }
                }


                

                float value_per_pixel = (heighest - lowest) / (Math.Abs(h_hieght - l_hieght));
                value_of_canvasHeight = value_per_pixel*canvas.Height; //value of the canvas height
                

                float distance_from_roof_of_canvas = Changedpoints[Rightest_pointI].y;
                float distance_from_floor_of_canvas = canvas.Height - distance_from_roof_of_canvas;
                float price = 0;

                string TheString = ""+value_of_rightest;
                float width = TextPaint_Y.MeasureText(TheString);
               

                Rect bounds = new Rect();
                TextPaint_Y.GetTextBounds(TheString, 0, TheString.Length, bounds);

                canvas.DrawCircle(Changedpoints[Rightest_pointI].x, Changedpoints[Rightest_pointI].y, 5, green);


                string the_string = "";

                //generete text-price below the furthest-right point
                for (int i = 0; i < (distance_from_floor_of_canvas / height) + 1; i++)
                {
                    price = value_of_rightest - i * (height * value_per_pixel);
                    price = (int)(price * 100);
                    price = price / 100;

                    the_string = "" + price;
                    if(i == 0)
                    {
                        if (values[Rightest_pointI] > values[Rightest_pointI - 1])
                        {
                            canvas.DrawRect(text_start_y, Changedpoints[Rightest_pointI].y - (height / 2), canvas.Width, Changedpoints[Rightest_pointI].y + (height / 2), green);
                        }
                            
                        else
                        {
                            canvas.DrawRect(text_start_y, Changedpoints[Rightest_pointI].y - (height / 2), canvas.Width, Changedpoints[Rightest_pointI].y + (height / 2), red);
                        }
                    }
                    canvas.DrawText(the_string, text_start_y, Changedpoints[Rightest_pointI].y + (height / 4)  + (i * height), TextPaint_Y);
                }

                //genereting text-price above the furthest-right point
                for (int i = 0; i < (int)((distance_from_roof_of_canvas / height) + 1); i++)
                {
                    if(((distance_from_roof_of_canvas / height) + 1) < 2)
                    {
                        i = 1;
                    }
                    price = value_of_rightest + i * (height * value_per_pixel);
                    price = (int)(price * 100);
                    price = price / 100;
                    the_string = "" + price;
                    canvas.DrawText(the_string, text_start_y, Changedpoints[Rightest_pointI].y + (height / 4) - (i * height), TextPaint_Y);
                }

                



            }
        }

        


        //calculations to find points on graph
        private int Calculate_Original_Point_I(float x)
        {
            double defualtPointx = (x - camera.CameraOffSetX) / test_zoomfactor;
            double itest2 = ((defualtPointx * (points.Count-1)) ) / (text_start_y);
            int i = (int)Math.Round(itest2);
            return i;
        }
        
        private int Calculate_Original_Point_I_without_rounding(float x)
        {
            double defualtPointx = (x - camera.CameraOffSetX) / test_zoomfactor;
            double itest2 = ((defualtPointx * (points.Count-1))) / (text_start_y);
            int i = (int)(itest2);
            return i;
        }



        //draw point touching and text bubble that upear above it with info
        private void DrawTouching()
        {
            p1.Color = Color.Black;
            if (midPoint != null)
            {
                canvas.DrawCircle(midPoint.x, midPoint.y, 10, p1);
                int i= Calculate_Original_Point_I(midPoint.x);
                if (0 <= i && i < points.Count)
                {
                    canvas.DrawCircle(Changedpoints[i].x, Changedpoints[i].y, 10, p1);

                    DrawTextBubbleForPoint(i);
                }
            }
        }

        private void DrawTextBubbleForPoint(int i)
        {
            float point_x = Changedpoints[i].x;
            float point_y = Changedpoints[i].y;

            Paint p_text1 = new Paint();
            p_text1.Color = Color.White;
            p_text1.TextSize = 50;

            

           

            float Bordar_x = point_x / 2;
            float Bordar_y = point_y / 2;




            Paint paint= new Paint();
            paint.Color = Color.ParseColor("#999999");

            

            Paint p_low = new Paint();
            p_low.Color = Color.Red;
            p_low.TextSize = 40;

            Paint p_heigh = new Paint();
            p_heigh.Color = Color.Green;
            p_heigh.TextSize = 40;

            float box_width = Math.Max(canvas.Width / 3, p_text1.MeasureText(dataPoints[i].date) + 100);
            float box_height = p_text1.TextSize + p_low.TextSize + p_heigh.TextSize + 10;
            canvas.DrawRect(Bordar_x - 50, Bordar_y - 50, Bordar_x + box_width + 50, Bordar_y + box_height + 50, black);
            canvas.DrawRect(Bordar_x, Bordar_y, Bordar_x + box_width, Bordar_y + box_height, paint);


            canvas.DrawText(dataPoints[i].date, Bordar_x + 1, Bordar_y + p_text1.TextSize, p_text1);

            canvas.DrawText("heigh: " + dataPoints[i].heigh, Bordar_x + 1, Bordar_y + p_heigh.TextSize + p_text1.TextSize , p_heigh);
            canvas.DrawText("low: " + dataPoints[i].low, Bordar_x + 1, Bordar_y + p_low.TextSize + p_heigh.TextSize + p_text1.TextSize, p_low);
            
        }

        
    }








    



}