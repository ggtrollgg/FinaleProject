﻿using Android.App;
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

        List<MySquare> Squares= new List<MySquare>();
        List<MySquare> ChangedSquares = new List<MySquare>();
        List<TextBlock> TextBlocks_Date = new List<TextBlock>();
        List<TextBlock> TextBlocks_Price = new List<TextBlock>();
        float squars_Width = 2;
        float distance = 2;
        public bool Zoom = false;
        public bool Move = false;
        float lowest = -1, highest = 0;
        bool doOnce = true;
        float textprice_margin = 0;
        Paint p;
        Paint p1 = new Paint();
        Paint orange;
        Paint blue;
        Paint Dred;
        Paint green;
        Paint purple;
        Paint black;

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


            orange = new Paint();
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

            purple  = new Paint();
            purple.Color = Color.Purple;
            purple.StrokeWidth = 5;

            black = new Paint();
            black.Color = Color.Black;
            black.StrokeWidth = 5;
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
                    CalculateNewPointes();
                    ChangeInTextPlaceX(daltaOffsetX);


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

        //things i want to do only once at the start (and  they need canvas)
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

                CreateYexis();
                DrawXexis();
                ChangeInTextPlaceX((float)0.01);
                ChangeInTextPlaceY((float)0.01);

                doOnce = false;
            }
        }

        //find the point with the highest value and the lowest value
        public void findLowHeigh()
        {
            foreach (DataPoint i in dataPoints)
            {
                if (i.close > highest) highest = i.close;
                if (i.close < lowest || lowest == -1) lowest = i.close;
            }
        }

        //create the Squars that represent the close and open of the stock at each timestamp
        //create them so the whole graph fit in a section of the canvas
        //and add them to a list
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
                squars_Width = (float)0.9*x_point_1 - x_point_0; 

                for (int i = 0; i < dataPoints.Count; i++)
                {
                    UpLeft = new MyPoint((i * (price_text_start_x) ) / ((dataPoints.Count - 1)), date_text_start_y + ((lowest - dataPoints[i].close) * (1 / (highest - lowest)) * date_text_start_y));
                    DownRight = new MyPoint(UpLeft.x + squars_Width,date_text_start_y);

                    Squares.Add(new MySquare(UpLeft,DownRight));
                }
                CalculateNewPointes();
            }
        }

        //draw the squars that represent close and open of the stock in every point of timeleap
        public void DrawSquers()
        {
            MyPoint UpLeft;
            MyPoint DownRight;
            for (int i = 0; i < dataPoints.Count; i++)
            {
                UpLeft = ChangedSquares[i].UpLeft;
                DownRight = ChangedSquares[i].DownRight;
                if(UpLeft.y == DownRight.y)
                {
                    canvas.DrawLine(UpLeft.x, UpLeft.y, DownRight.x, DownRight.y, orange);
                }
                if(i ==0)
                {
                    if (dataPoints[i].close > dataPoints[i].open)
                    {
                        canvas.DrawRect(UpLeft.x, UpLeft.y, DownRight.x, DownRight.y, green);
                    }
                    else
                        canvas.DrawRect(UpLeft.x, UpLeft.y, DownRight.x, DownRight.y, p);
                }
                else
                {
                    if (dataPoints[i].close > dataPoints[i-1].close)
                    {
                        canvas.DrawRect(UpLeft.x, UpLeft.y, DownRight.x, DownRight.y, green);
                    }
                    else
                        canvas.DrawRect(UpLeft.x, UpLeft.y, DownRight.x, DownRight.y, p);
                }
            }
        }

        //calculate the new positions of the squares based on their original value and the changing values 
        //--> offest x and y, and scale in the x exiecss
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

                upLeft = new MyPoint(Squares[i].UpLeft.x * zoomfactor_X + camera.CameraOffSetX, Squares[i].UpLeft.y);
                downRight = new MyPoint(upLeft.x + squars_Width * zoomfactor_X, date_text_start_y);

                ChangedSquares.Add(new MySquare(upLeft, downRight));
            }
        }

        //draw the text that represent the time of each point on the screen
        //"Xexis" ---> because the text spreads horizontaly on the bottom of the screen 
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
                    canvas.DrawText(TheString, ChangedSquares[i].Center.x, canvas.Height, textPaint_Date);

                }

            }

        }

        //create the text that represent the price of each point/squar in the graph
        //based on the original/first/defualt position of the points
        //"Yexis" --> the price text stacks verticaly in the right side of the screen
        private void CreateYexis()
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
                        TextBlocks_Price.Add(new TextBlock(TheString, textPaint_Price, price_text_start_x + (width / 2), ChangedSquares[i].Center.y));
                    }

                }
            }

        }

        //Change the place of all the texts that represent the date/time of each point/squar in the graph
        //in case and the gap between each text is big enough to fit another textblock than show the coresponding textblock that fit there
        //in case and the textblocks touch each other than hide the textblocks that are to adjacent to the right of the hidden ones
        private void ChangeInTextPlaceX(float v)
        {
            int place = 0;
            if (v != 0)
            {
                for (int i = 1; i < TextBlocks_Date.Count; i++)
                {
                    if (!TextBlocks_Date[i].Hidden)
                    {
                        float width = textPaint_Date.MeasureText(TextBlocks_Date[0].Text);
                        float RightX = ChangedSquares[0].Center.x + width/(float)2.0;
                        float LeftX = ChangedSquares[i].Center.x - width/(float)2.0;

                        if (RightX >= LeftX-width/10.0)//hide text 
                        {
                            TextBlocks_Date[i].Hidden = true;
                            for (int g = i; g < TextBlocks_Date.Count; g += 2 * i)
                            {
                                TextBlocks_Date[g].Hidden = true;
                            }
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

        //Draws the textblocks that represent range of prices (of each point) on the screen
        private void ChangeInTextPlaceY(float v)
        {

            canvas.DrawRect(price_text_start_x, 0, canvas.Width, canvas.Height, background);

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

                float heighest = dataPoints[Rightest_pointI].close;//just putting some values so i can compere with other values to find the max and min 
                float lowest = dataPoints[Leftest_pointI].close;//---
                float value_of_rightest = dataPoints[Rightest_pointI].close;
                float value_of_canvasHeight = 0;
                float l_hieght = ChangedSquares[Leftest_pointI].UpLeft.y;
                float h_hieght = ChangedSquares[Rightest_pointI].UpLeft.y;

                Paint.FontMetrics fm = textPaint_Price.GetFontMetrics();
                float height = fm.Bottom - fm.Top + fm.Leading; //height in pixels of text

                for (int i = Leftest_pointI; i <= Rightest_pointI; i++)//getting the heighest and lowest price on screen
                {
                    if (dataPoints != null && dataPoints.Count > i && dataPoints[i].close < lowest)
                    {
                        lowest = dataPoints[i].close;
                        l_hieght = ChangedSquares[i].UpLeft.y;
                    }
                    else if (dataPoints != null && dataPoints.Count > i && dataPoints[i].close > heighest)
                    {
                        heighest = dataPoints[i].close;
                        h_hieght = ChangedSquares[i].UpLeft.y;
                    }
                }
                
                if (heighest == lowest)
                    //in case and there is a situation when there is only one point/price visibal on screen
                    //the code will try to find a diffrent price which is higher or lower then the rightest point
                    //so in the avaluation of value per pixel we wont divide by 0
                {
                    int i = Leftest_pointI;
                    while(i > -1 && heighest == lowest) //checking for lowest or highest in the left side of the graph
                    {
                        if (dataPoints != null && dataPoints.Count > i && dataPoints[i].close < lowest)
                        {
                            lowest = dataPoints[i].close;
                            l_hieght = ChangedSquares[i].UpLeft.y;
                        }
                        else if (dataPoints != null && dataPoints.Count > i && dataPoints[i].close > heighest)
                        {
                            heighest = dataPoints[i].close;
                            h_hieght = ChangedSquares[i].UpLeft.y;
                        }
                        i--;
                    }
                    if(heighest == lowest)//didnt find a lower/higher price
                    {
                        Console.WriteLine("could not calaculate price per pixel on screen becuase lowest == hieghest :" + lowest + " = " + heighest);
                        canvas.DrawCircle(ChangedSquares[Rightest_pointI].Center.x, ChangedSquares[Rightest_pointI].Center.y, 5, green);
                        if (dataPoints[Rightest_pointI].close > dataPoints[Rightest_pointI - 1].close)
                        {
                            canvas.DrawRect(price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y - (height / 2), canvas.Width, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 2), green);
                        }

                        else
                        {
                            canvas.DrawRect(price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y - (height / 2), canvas.Width, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 2), Dred);
                        }
                        
                        canvas.DrawText("" + value_of_rightest, price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 4), textPaint_Price);
                        return;
                    }
                }



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

                string the_string = "";

                //generete text-price below the furthest-right point
                for (int i = 0; i < (distance_from_floor_of_canvas / height) + 1; i++)
                {
                    price = value_of_rightest - i * (height * value_per_pixel);
                    price = (int)(price * 100);
                    price = price / 100;

                    the_string = "" + price;
                    if (i == 0)
                    {
                        if (dataPoints[Rightest_pointI].close > dataPoints[Rightest_pointI - 1].close)
                        {
                            canvas.DrawRect(price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y - (height / 2), canvas.Width, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 2), green);
                        }

                        else
                        {
                            canvas.DrawRect(price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y - (height / 2), canvas.Width, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 2), Dred);
                        }
                    }
                    canvas.DrawText(the_string, price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 4) + (i * height), textPaint_Price);
                }

                //genereting text-price above the furthest-right point
                for (int i = 0; i < (int)((distance_from_roof_of_canvas / height) + 1); i++)
                {
                    if (((distance_from_roof_of_canvas / height) + 1) < 2)
                    {
                        i = 1;
                    }
                    price = value_of_rightest + i * (height * value_per_pixel);
                    price = (int)(price * 100);
                    price = price / 100;
                    the_string = "" + price;
                    canvas.DrawText(the_string, price_text_start_x, ChangedSquares[Rightest_pointI].UpLeft.y + (height / 4) - (i * height), textPaint_Price);
                }





            }
        }

        //gets a X value, than it is going through the calculations to becom a new ChangedPoint but in backword order
        //meaning if it was a ChangedPoint (point on graphs that was changed bt offset and scale) it is now a X value that represent an original/defualt point on the graph
        // and thus, it has an I, index in the list of original points
        //because every point x position is determinded by its position in the list(i index) than i can find the I index (or the closes to it) of the point in corrdinate (X,...);
        private int Calculate_Original_Point_I(float x)
        {
            double defualtPointx = (x - camera.CameraOffSetX) / zoomfactor_X;
            double i_d = ((defualtPointx * (Squares.Count-1))) / (price_text_start_x);//i_d => i double
            int i = (int)Math.Round(i_d);
            return i;
        }

        //Same as above but without rounding the resualt
        //this because when i check for the rightest point on screen,
        //the pixel that is the rightest on scree can be more than half way to a point that isnt on screen 
        //but i still want it to retern the rightest point that i can see on screen
        private int Calculate_Original_Point_I_without_rounding(float x)
        {
            double defualtPointx = (x - camera.CameraOffSetX) / zoomfactor_X;
            double i_d = ((defualtPointx * (Squares.Count-1))) / (price_text_start_x); //i_d => i double
            int i = (int)(i_d);
            return i;
        }

        //draw the mid point between 2 of my fingers
        //(when there are more than 1 finger touching the screen)
        private void DrawTouching()
        {
            p1.Color = Color.Black;
            if (midPoint != null)
            {
                canvas.DrawCircle(midPoint.x, midPoint.y, 10, p1);
                int i = Calculate_Original_Point_I_without_rounding(midPoint.x);
                if (0 <= i && i < Squares.Count)
                {
                    canvas.DrawCircle(ChangedSquares[i].Center.x, ChangedSquares[i].Center.y, 10, p1);

                    DrawTextBubbleForPoint(i);
                }
            }

        }

        //draw a text bubble at the left-up corrner of the screen with info
        //of the point that is at the middle of 2 of my fingers that are touching the screen
        private void DrawTextBubbleForPoint(int i)
        {
            float point_x = ChangedSquares[i].Center.x;
            float point_y = ChangedSquares[i].Center.y;

            Paint p_text1 = new Paint();
            p_text1.Color = Color.White;
            p_text1.TextSize = 50;

            float Bordar_x = 50;
            float Bordar_y = 50;

            Paint paint = new Paint();
            paint.Color = Color.ParseColor("#999999");



            Paint p_low = new Paint();
            p_low.Color = Color.Red;
            p_low.TextSize = 40;

            Paint p_heigh = new Paint();
            p_heigh.Color = Color.Green;
            p_heigh.TextSize = 40;

            float box_width = Math.Max(canvas.Width / 3, p_text1.MeasureText(dataPoints[i].date) + 100);
            float box_height = p_text1.TextSize + 2*p_low.TextSize + 2*p_heigh.TextSize + 10;
            canvas.DrawRect(Bordar_x - 50, Bordar_y - 50, Bordar_x + box_width + 50, Bordar_y + box_height + 50, black);
            canvas.DrawRect(Bordar_x, Bordar_y, Bordar_x + box_width, Bordar_y + box_height, paint);


            canvas.DrawText(dataPoints[i].date, Bordar_x + 1, Bordar_y + p_text1.TextSize, p_text1);

            canvas.DrawText("heigh: " + dataPoints[i].heigh, Bordar_x + 1, Bordar_y + p_heigh.TextSize + p_text1.TextSize, p_heigh);
            canvas.DrawText("low: " + dataPoints[i].low, Bordar_x + 1, Bordar_y + p_low.TextSize + p_heigh.TextSize + p_text1.TextSize, p_low);
            if (dataPoints[i].close > dataPoints[i].open)
            {
                canvas.DrawText("close: " + dataPoints[i].close, Bordar_x + 1, Bordar_y + 2 * p_heigh.TextSize + p_low.TextSize + p_text1.TextSize, p_heigh);
                canvas.DrawText("open: " + dataPoints[i].open, Bordar_x + 1, Bordar_y + 2 * p_low.TextSize + 2 * p_heigh.TextSize + p_text1.TextSize, p_low);
            }
            else
            {
                canvas.DrawText("open: " + dataPoints[i].open, Bordar_x + 1, Bordar_y + 2 * p_heigh.TextSize + p_low.TextSize + p_text1.TextSize, p_heigh);
                canvas.DrawText("close: " + dataPoints[i].close, Bordar_x + 1, Bordar_y + 2 * p_low.TextSize + 2 * p_heigh.TextSize + p_text1.TextSize, p_low);
            }

        }



    }
}