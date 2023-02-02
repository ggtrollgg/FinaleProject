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

namespace App1
{
    internal class Test_ZoomCanvas : View
    {
        private Context context;
        Canvas canvas;


        MyCamera camera = new MyCamera(0, 0);
        MySquare Bigsquare;
        MySquare square;
        MySquare Smallsquare;

        MyPoint lastPlace;
        MyPoint lastPlace2;
        MyPoint LastmidPoint;

        float zoomfactorX = 1;
        bool doOnce = true;

        
        MyPoint point1;
        MyPoint point2;
        MyPoint midPoint;

        
        Paint p1 = new Paint();
        Paint p2 = new Paint();

        Paint pBig;
        Paint pNorm;
        Paint pSmall;

        public Test_ZoomCanvas(Context context) : base(context)
        {
            this.context = context;
            pBig = new Paint();
            pBig.Color = Color.Blue;

            this.context = context;
            pNorm = new Paint();
            pNorm.Color = Color.Green;

            this.context = context;
            pSmall = new Paint();
            pSmall.Color = Color.Pink;

            p2.Color= Color.Red;
        }

        protected override void OnDraw(Canvas canvas1)
        {
            this.canvas = canvas1;
            DoOnce();
            ScaleCanvas();
            DrawSquers();
            DrawTouching();
            canvas.DrawCircle(canvas.Width / 2 + camera.CameraOffSetX, canvas.Height / 2 + camera.CameraOffSetY, 25, p2);
            canvas.DrawCircle(canvas.Width / 2 + camera.CameraOffSetX, canvas.Height / 2 + camera.CameraOffSetY, 25, p1);

            Console.WriteLine(zoomfactorX);
            Console.WriteLine(camera.CameraOffSetX);
            Console.WriteLine(camera.CameraOffSetY);
            Invalidate();
        }

        private void ScaleCanvas()
        {
            float posX = canvas.Width / 2;
            float posY = canvas.Height / 2;
            //canvas.Translate(0, 0);
            canvas.Scale(2, 2, posX, posY);
            //canvas.Scale(zoomfactorX, zoomfactorY);




            //if(LastmidPoint!= null) {
            //    posX = LastmidPoint.x; 
            //    posY = LastmidPoint.y;
            //}
            //canvas.Translate(posX, posY);

            //if (midPoint!= null)
            //    canvas.Scale(zoomfactorX, 2, midPoint.x, midPoint.y);
            //else
            //    canvas.Scale(zoomfactorX, 2, posX, posY);
        }

        private void DoOnce()
        {
            if (doOnce)
            {
                float Height = canvas.Height;
                float Width = canvas.Width;

                Bigsquare = new MySquare(Width / 8, Height / 8, Width * 7 / 8, Height * 7 / 8);
                square = new MySquare(Width / 4, Height / 4, Width * 3 / 4, Height * 3 / 4);
                Smallsquare = new MySquare(Width / 3, Height / 3, Width * 2 / 3, Height * 2 / 3);
                doOnce = false;
            }
            
        }

        private void DrawSquers()
        {
            MyPoint UpLeft = Bigsquare.UpLeft;
            MyPoint DownRight = Bigsquare.DownRight;
            canvas.DrawRect(UpLeft.x + camera.CameraOffSetX, UpLeft.y + camera.CameraOffSetY, DownRight.x + camera.CameraOffSetX, DownRight.y + camera.CameraOffSetY, pBig);

            MyPoint UpLeft1 = square.UpLeft;
            MyPoint DownRight1 = square.DownRight;
            canvas.DrawRect(UpLeft1.x + camera.CameraOffSetX, UpLeft1.y + camera.CameraOffSetY, DownRight1.x + camera.CameraOffSetX, DownRight1.y + camera.CameraOffSetY, pNorm);

            MyPoint UpLeft2 = Smallsquare.UpLeft;
            MyPoint DownRight2 = Smallsquare.DownRight;
            canvas.DrawRect(UpLeft2.x + camera.CameraOffSetX, UpLeft2.y + camera.CameraOffSetY, DownRight2.x + camera.CameraOffSetX, DownRight2.y + camera.CameraOffSetY, pSmall);
        }

        private void DrawTouching()
        {
            p1.Color = Color.Black;
            if (point1 != null && point2 != null && midPoint != null)
            {
                canvas.DrawCircle(point1.x, point1.y, 25, p1);
                canvas.DrawCircle(point2.x, point2.y, 25, p1);
                canvas.DrawCircle(midPoint.x, midPoint.y, 10, p1);

            }
        }
        public override bool OnTouchEvent(MotionEvent e)
        {
            
            if(e.PointerCount == 1)
            {
                if (lastPlace== null)
                {
                    lastPlace = new MyPoint(e.GetX(), e.GetY());
                }
                camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
                camera.CameraOffSetY += (float)e.GetY() - lastPlace.y;
            }

            else if (e.PointerCount > 1)
            {
                p1.Color = Color.Black;

                point1 = new MyPoint((float)e.GetX(), (float)e.GetY());
                point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));

                if (e.Action == MotionEventActions.Pointer2Down || e.Action == MotionEventActions.Down)
                {
                    midPoint = new MyPoint((point1.x + point2.x) / 2, (point1.y + point2.y) / 2);
                    if(lastPlace2!= null)
                    {
                        LastmidPoint = new MyPoint((lastPlace.x + lastPlace2.x) / 2, (lastPlace.y + lastPlace2.y) / 2);
                    }
                }
            }
            if (e.Action == MotionEventActions.Up)
            {
                point1 = null;
                point2 = null;
                midPoint = null;
            }



            //if (lastPlace == null)
            //{
            //    lastPlace = new MyPoint(e.GetX(), e.GetY());
            //    return true;
            //}
            //else
            //{
                if (e.Action == MotionEventActions.Move && lastPlace != null)
                {
                    if (e.PointerCount > 1 && midPoint != null)
                    {
                        point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));
                        if (lastPlace2 == null)
                        {
                            lastPlace2 = new MyPoint(point2.x, point2.y);
                        }

                        zoomfactorX += ((float)e.GetX() - lastPlace.x) / 1000;// + ((float)point2.x - lastPlace2.x) / 100;

                        lastPlace2.x = point2.x;
                        lastPlace2.y = point2.y;

                    }
                }
           // }

            
           if(lastPlace!= null && e.PointerCount == 1)
           {
               lastPlace.x = e.GetX();
               lastPlace.y = e.GetY();
           }
           if (e.Action == MotionEventActions.Up)
           {
             lastPlace = null;
           }
            return true;


        }


    }
}