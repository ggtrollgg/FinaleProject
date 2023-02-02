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
        

        float zoomfactorX = 1;
        float zoomfactorY = 1;
        bool doOnce = true;

        
        MyPoint point1;
        MyPoint point2;
        MyPoint midPoint;
        MyPoint LastmidPoint;

        MyPoint PivitPoint;

        Paint p1 = new Paint();
        Paint p2 = new Paint();
        Paint p3 = new Paint();

        Paint pBig;
        Paint pNorm;
        Paint pSmall;

        public Test_ZoomCanvas(Context context) : base(context)
        {
            this.context = context;
            pBig = new Paint();
            pBig.Color = Color.Blue;

            
            pNorm = new Paint();
            pNorm.Color = Color.Green;

            
            pSmall = new Paint();
            pSmall.Color = Color.Pink;

            p2.Color= Color.Red;
            p2.Color = Color.Purple;
        }

        protected override void OnDraw(Canvas canvas1)
        {
            this.canvas = canvas1;

            DoOnce();
            DrawTouching();
            ScaleCanvas();

            DrawSquers();
            

            canvas.DrawCircle(canvas.Width / 2 + camera.CameraOffSetX / zoomfactorX, canvas.Height / 2 + camera.CameraOffSetY / zoomfactorY, 25, p2);
            canvas.DrawCircle(canvas.Width / 2 + camera.CameraOffSetX / zoomfactorX, canvas.Height / 2 + camera.CameraOffSetY / zoomfactorY, 25, p1);

            //Console.WriteLine(zoomfactorX);
            //Console.WriteLine(camera.CameraOffSetX);
            //Console.WriteLine(camera.CameraOffSetY);


            Invalidate();
        }

        private void ScaleCanvas()
        {
            float posX = canvas.Width / 2;
            float posY = canvas.Height / 2;
            //canvas.Translate(0, 0);
            //canvas.Scale(2, 2, posX, posY);
            //canvas.Scale(zoomfactorX, zoomfactorY, posX, posY);




            if (LastmidPoint != null)
            {
                posX = LastmidPoint.x;
                posY = LastmidPoint.y;
            }
            //canvas.Translate(posX, posY);


            if(PivitPoint!= null)
            {
                canvas.Scale(zoomfactorX, zoomfactorY, PivitPoint.x, PivitPoint.y);
            }

            //if (midPoint != null)
            //    canvas.Scale(zoomfactorX, zoomfactorY, midPoint.x, midPoint.y);
            //else if(LastmidPoint != null)
            //    canvas.Scale(zoomfactorX, zoomfactorY, LastmidPoint.x, LastmidPoint.y);
        }

        private void DoOnce()
        {
            if (doOnce)
            {
                float Height = canvas.Height;
                float Width = canvas.Width;

                //Bigsquare = new MySquare(Width / 8, Height / 8, Width * 7 / 8, Height * 7 / 8);
                //square = new MySquare(Width / 4, Height / 4, Width * 3 / 4, Height * 3 / 4);
                //Smallsquare = new MySquare(Width / 3, Height / 3, Width * 2 / 3, Height * 2 / 3);

                Bigsquare = new MySquare(Width / 8, Width / 8, Width * 2 / 8, Width * 2 / 8);
                square = new MySquare(Width * 3 / 8, Height / 8, Width * 4 / 8, Width * 2 / 8);
                Smallsquare = new MySquare(Width * 5 / 8, Width / 8, Width * 6 / 8, Width * 2 / 8);


                doOnce = false;
            }
            
        }

        private void DrawSquers()
        {
            MyPoint UpLeft = Bigsquare.UpLeft;
            MyPoint DownRight = Bigsquare.DownRight;
            //canvas.DrawRect(UpLeft.x + camera.CameraOffSetX, UpLeft.y + camera.CameraOffSetY, DownRight.x + camera.CameraOffSetX, DownRight.y + camera.CameraOffSetY, pBig);
            canvas.DrawRect(UpLeft.x + camera.CameraOffSetX / zoomfactorX, UpLeft.y + camera.CameraOffSetY / zoomfactorY, DownRight.x + camera.CameraOffSetX / zoomfactorX, DownRight.y + camera.CameraOffSetY / zoomfactorY, pBig);

            MyPoint UpLeft1 = square.UpLeft;
            MyPoint DownRight1 = square.DownRight;
            //canvas.DrawRect(UpLeft1.x + camera.CameraOffSetX, UpLeft1.y + camera.CameraOffSetY, DownRight1.x + camera.CameraOffSetX, DownRight1.y + camera.CameraOffSetY, pNorm);
            canvas.DrawRect(UpLeft1.x + camera.CameraOffSetX / zoomfactorX, UpLeft1.y + camera.CameraOffSetY / zoomfactorY, DownRight1.x + camera.CameraOffSetX / zoomfactorX, DownRight1.y + camera.CameraOffSetY / zoomfactorY, pNorm);

            MyPoint UpLeft2 = Smallsquare.UpLeft;
            MyPoint DownRight2 = Smallsquare.DownRight;
            //canvas.DrawRect(UpLeft2.x + camera.CameraOffSetX, UpLeft2.y + camera.CameraOffSetY, DownRight2.x + camera.CameraOffSetX, DownRight2.y + camera.CameraOffSetY, pSmall);
            canvas.DrawRect(UpLeft2.x + camera.CameraOffSetX / zoomfactorX, UpLeft2.y + camera.CameraOffSetY / zoomfactorY, DownRight2.x + camera.CameraOffSetX / zoomfactorX, DownRight2.y + camera.CameraOffSetY / zoomfactorY, pSmall);
        }

        private void DrawTouching()
        {
            p1.Color = Color.Black;

            if (point1 != null && point2 != null && midPoint != null)
            {
                canvas.DrawCircle(point1.x  , point1.y, 25, p1);
                canvas.DrawCircle(point2.x , point2.y, 25, p1);
                canvas.DrawCircle(midPoint.x, midPoint.y, 10, p1);

                //canvas.DrawCircle(point1.x / zoomfactorX, point1.y / zoomfactorY, 25, p2);
                //canvas.DrawCircle(point2.x / zoomfactorX, point2.y / zoomfactorY, 25, p2);
                //canvas.DrawCircle(midPoint.x / zoomfactorX, midPoint.y / zoomfactorY, 10, p2);

                //canvas.DrawCircle(point1.x * zoomfactorX, point1.y * zoomfactorY, 25, p3);
                //canvas.DrawCircle(point2.x * zoomfactorX, point2.y * zoomfactorY, 25, p3);
                //canvas.DrawCircle(midPoint.x * zoomfactorX, midPoint.y * zoomfactorY, 10, p3);

            }
        }
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down)
            {
                //if (e.PointerCount > 1)
                //{
                //    p1.Color = Color.Black;
                //    point1 = new MyPoint((float)e.GetX(), (float)e.GetY());
                //    point2 = new MyPoint((float)e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), (float)e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));
                //    PivitPoint = new MyPoint((float)(point1.x + point2.x) / (float)2, (float)(point1.y + point2.y) / (float)2);
                //}
            }
            if (e.Action == MotionEventActions.Up)
            {
                point1 = null;
                point2 = null;
                midPoint = null;
                lastPlace2 = null;
            }
            if (e.Action == MotionEventActions.Up && e.PointerCount == 1)
            {
                lastPlace = null;
                //Console.WriteLine("camera.CameraOffSetX is: " + camera.CameraOffSetX);
                return true;
            }


            if (e.PointerCount == 1)
            {
                if (lastPlace== null)
                {
                    lastPlace = new MyPoint(e.GetX(), e.GetY());
                }
                camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
                camera.CameraOffSetY += (float)e.GetY() - lastPlace.y;

               // Console.WriteLine("touching: (" + (float)e.GetX() +"," + (float)e.GetY());
            }

            else if (e.PointerCount > 1)
            {
                p1.Color = Color.Black;

                point1 = new MyPoint((float)e.GetX(), (float)e.GetY());
                point2 = new MyPoint((float)e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), (float)e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));
                PivitPoint = new MyPoint((float)(point1.x + point2.x) / (float)2, (float)(point1.y + point2.y) / (float)2);

                if (e.Action == MotionEventActions.Pointer2Down || e.Action == MotionEventActions.Down)
                {
                    midPoint = new MyPoint((point1.x + point2.x) / 2, (point1.y + point2.y) / 2);
                    if(lastPlace2!= null)
                    {
                        LastmidPoint = new MyPoint((lastPlace.x + lastPlace2.x) / 2, (lastPlace.y + lastPlace2.y) / 2);
                    }
                }
            }
            



            //if (lastPlace == null)
            //{
            //    lastPlace = new MyPoint(e.GetX(), e.GetY());
            //    return true;
            //}
            //else
            //{
                if (e.Action == MotionEventActions.Move && e.PointerCount > 1)
                {
                    if (midPoint != null)
                    {
                        point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));
                        if (lastPlace2 == null)
                        {
                            lastPlace2 = new MyPoint(point2.x, point2.y);
                        }

                        //zoomfactorX += ((float)e.GetX() - lastPlace.x) / 10000;// + ((float)point2.x - lastPlace2.x) / 100;
                        //zoomfactorY += ((float)e.GetY() - lastPlace.y) / 10000;

                        zoomfactorX -= ((float)point2.x - lastPlace2.x) / 1000;
                        zoomfactorY -= ((float)point2.y - lastPlace2.y) / 1000;


                        lastPlace2.x = point2.x;
                        lastPlace2.y = point2.y;

                    }
                }
                
           // }

            
           
          
            if (lastPlace != null && e.PointerCount == 1)
            {
                lastPlace.x = e.GetX();
                lastPlace.y = e.GetY();
            }
            return true;
        }
    }
}