using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using App1.General_Classes;
using Firebase.Firestore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App1
{
    internal class ColumGraph : Class_FatherGraph
    {
        List<MyPoint> points = new List<MyPoint>();
        List<MyPoint> Changedpoints = new List<MyPoint>();

        List<MySquare> Squares= new List<MySquare>();
        List<MySquare> ChangedSquares = new List<MySquare>();

        float squars_Width = 2;
        float distance = 2;
        float lowest = -1, highest = 0;
        bool doOnce = true;

        MyCamera camera = new MyCamera(0, 0);


        MyPoint lastPlace;
        MyPoint lastPlace2;
        float test_zoomfactor = 1;



        public bool Zoom = false;
        public bool Move = false;

        Paint p;
        Paint p1 = new Paint();
        MyPoint point1;
        MyPoint point2;
        MyPoint midPoint;

        public ColumGraph(Context context) : base(context)
        {
            p = new Paint();
            p.Color = Color.Red;
            p.StrokeWidth = 6;
        }


        protected override void OnDraw(Canvas canvas1)
        {
            canvas = canvas1;
            if (dataPoints != null)
            {
                DoOnce();
                DrawGraph();
                
                //DrawTouching();
                //DrawXexis();

                Invalidate();
            }
        }

        private void DrawGraph()
        {
            CreateChartSquars();
            DrawSquers();
        }

        private void DrawTouching()
        {
            p1.Color = Color.Black;
            if (point1 != null && point2 != null && midPoint != null)
            {
                canvas.DrawCircle(point1.x, point1.y, 100, p1);
                canvas.DrawCircle(point2.x, point2.y, 100, p1);
                canvas.DrawCircle(midPoint.x, midPoint.y, 10, p1);
                p1.Color = Color.Blue;
                p1.Color = Color.Green;
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
                        test_zoomfactor += ((float)e.GetX() - lastPlace.x) / 100 + ((float)point2.x - lastPlace2.x) / 100;

                        lastPlace2.x = point2.x;
                        lastPlace2.y = point2.y;
                    }
                    else
                    {
                        //if (!(camera.CameraOffSetX + (float)e.GetX() - lastPlace.x >= 0))
                        //{
                        //    camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
                        //}
                        camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
                        camera.CameraOffSetY += (float)e.GetY() - lastPlace.y;
                        Console.WriteLine("The camera ofsetx is: " + camera.CameraOffSetX);
                    }
                }
            }

            CalculateNewPointes();

            lastPlace.x = e.GetX();
            lastPlace.y = e.GetY();
            return true;


        }


        private void DoOnce()
        {
            if (doOnce)
            {
                findLowHeigh();
                doOnce= false;
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

                for (int i = 0; i < dataPoints.Count; i++)
                {
                    //points.Add(new MyPoint((i * canvas.Width) / (dataPoints.Count - 1), canvas.Height + ((lowest - values[i]) * (1 / (heighest - lowest)) * canvas.Height)));
                    //UpLeft = new MyPoint(i * (squars_Width + 1), canvas.Height- (dataPoints[i].heigh )* canvas.Height);
                    //UpLeft = new MyPoint(i * (squars_Width)+ i*distance, canvas.Height +  (lowest-dataPoints[i].heigh) *(1/(highest-lowest)) * canvas.Height);
                    //UpLeft = new MyPoint(i * (squars_Width) + i * distance, canvas.Height + ((lowest - dataPoints[i].heigh) * (1 / (highest - lowest)) * canvas.Height));
                    //UpLeft = new MyPoint((i * squars_Width + i * canvas.Width ) / (dataPoints.Count - 1 ), canvas.Height + ((lowest - dataPoints[i].heigh) * (1 / (highest - lowest)) * canvas.Height));
                    UpLeft = new MyPoint(i*(canvas.Width-squars_Width) / (dataPoints.Count - 1), canvas.Height + ((lowest - dataPoints[i].heigh) * (1 / (highest - lowest)) * canvas.Height));
                    DownRight = new MyPoint(UpLeft.x+squars_Width, canvas.Height);

                    Squares.Add(new MySquare(UpLeft,DownRight));
                }
                CalculateNewPointes();
            }
        }

        private void CalculateNewPointes()
        {
            MyPoint upLeft;
            MyPoint downRight;
            for (int i = 0; i < dataPoints.Count; i++)
            {
                //Changedpoints.Add(new MyPoint((points[i].x) * test_zoomfactor + camera.CameraOffSetX, points[i].y + camera.CameraOffSetY));
                upLeft = new MyPoint(Squares[i].UpLeft.x + camera.CameraOffSetX,Squares[i].UpLeft.y + camera.CameraOffSetY);
                downRight = new MyPoint(upLeft.x + squars_Width*test_zoomfactor, canvas.Height+camera.CameraOffSetY);
                ChangedSquares.Add(new MySquare(upLeft,downRight));
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
                canvas.DrawRect(UpLeft.x+camera.CameraOffSetX,UpLeft.y,DownRight.x+camera.CameraOffSetX,DownRight.y,p);
            }
        }

    }
}