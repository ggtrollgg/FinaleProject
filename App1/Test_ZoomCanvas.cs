using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Icu.Number;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Translation;
using Android.Widget;
using App1.General_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static Android.InputMethodServices.Keyboard;

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
        MyPoint LastPivitPoint;

        Paint p1 = new Paint();
        Paint p2 = new Paint();
        Paint p3 = new Paint();
        Paint p4 = new Paint();
        Paint brown = new Paint();
        Paint yellow = new Paint();

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

            p1.Color = Color.Black;
            p2.Color = Color.Red;
            p3.Color = Color.Purple;
            p4.Color = Color.Green;
            yellow.Color = Color.Yellow;
            brown.Color = Color.Brown;

            ThreadStart threadStart1 = new ThreadStart(PrintParameters);
            Thread t1 = new Thread(threadStart1);
            t1.Start();
        }

        public void PrintParameters()
        {
            int x = 100;
            while (x >= 0)
            {
                Thread.Sleep(2000);
                Console.WriteLine("\n" + "********************************************************");
                Console.WriteLine("");

                Console.WriteLine("zoomfactorX is: " + zoomfactorX);
                Console.WriteLine("zoomfactorY is: " + zoomfactorY);
                Console.WriteLine("camera.CameraOffSetX is: " + camera.CameraOffSetX);
                Console.WriteLine("camera.CameraOffSetY is: " + camera.CameraOffSetY);
                if (PivitPoint != null && LastPivitPoint != null)
                {
                    Console.WriteLine("pivitPoint is: (" + PivitPoint.x + "," + PivitPoint.y + ") ");
                    Console.WriteLine("LastpivitPoint is: (" + LastPivitPoint.x + "," + LastPivitPoint.y + ") ");
                }
                if (midPoint != null)
                    Console.WriteLine("midpoint is: (" + midPoint.x + "," + midPoint.y + ") ");
                if (point2 != null)
                    Console.WriteLine("point2 is: (" + point2.x + "," + point2.y + ") ");
                if (lastPlace2 != null)
                    Console.WriteLine("lastplace2 is: (" + lastPlace2.x + "," + lastPlace2.y + ") ");
                if (point1 != null)
                    Console.WriteLine("point1 is: (" + point1.x + "," + point1.y + ") ");
                if (lastPlace != null)
                    Console.WriteLine("lastPlace is: (" + lastPlace.x + "," + lastPlace.y + ") ");

                Console.WriteLine("");
                Console.WriteLine("\n" + "********************************************************");
                x--;
            }

            Console.WriteLine("Printer stopped");
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


        protected override void OnDraw(Canvas canvas1)
        {
            this.canvas = canvas1;

            DoOnce();
            DrawTouching();
            canvas.DrawCircle(canvas.Width / 2, canvas.Height / 2, 25, brown);
            canvas.DrawCircle(canvas.Width / 2 + camera.CameraOffSetX, canvas.Height / 2 + camera.CameraOffSetY, 25, p4);

            if (PivitPoint != null)
            {
                //canvas.DrawCircle(PivitPoint.x + camera.CameraOffSetX / zoomfactorX, PivitPoint.y + camera.CameraOffSetY / zoomfactorY, 25, p2);
                //canvas.DrawCircle(PivitPoint.x + camera.CameraOffSetX, PivitPoint.y + camera.CameraOffSetY, 25, p2);
                //canvas.DrawCircle(PivitPoint.x * zoomfactorX + camera.CameraOffSetX, PivitPoint.y * zoomfactorY + camera.CameraOffSetY, 25, p2);
                canvas.DrawCircle(PivitPoint.x, PivitPoint.y, 25, p2);
            }



            // ScaleCanvas();
             ReSizeCanvas();

            DrawSquers();


            //canvas.DrawCircle(canvas.Width / 2 + camera.CameraOffSetX / zoomfactorX, canvas.Height / 2 + camera.CameraOffSetY / zoomfactorY, 25, p2);
            //canvas.DrawCircle(canvas.Width / 2 + camera.CameraOffSetX / zoomfactorX, canvas.Height / 2 + camera.CameraOffSetY / zoomfactorY, 25, p1);
            if (PivitPoint != null)
            {
                canvas.DrawCircle(PivitPoint.x + camera.CameraOffSetX / zoomfactorX, PivitPoint.y + camera.CameraOffSetY / zoomfactorY, 25, p3);
                //canvas.DrawCircle((PivitPoint.x + camera.CameraOffSetX) / zoomfactorX, (PivitPoint.y + camera.CameraOffSetY) / zoomfactorY, 25, p3);
            }


            //Console.WriteLine(zoomfactorX);
            //Console.WriteLine(camera.CameraOffSetX);
            //Console.WriteLine(camera.CameraOffSetY);


            Invalidate();
        }


        public void ReSizeCanvas()
        {
            
            Matrix matrix = new Matrix();

            // Save the current matrix
            canvas.Save();

            // Translate the origin to the center of the canvas
            canvas.Translate(canvas.Width / 2, canvas.Height / 2);

            // Scale the canvas by a factor of 2
            canvas.Scale(zoomfactorX, zoomfactorY);

            // Translate the origin back to the top left corner of the canvas
            canvas.Translate(-canvas.Width / 2, -canvas.Height / 2);

            // Set the transformation on the canvas
            //canvas.Transform(matrix);

            // Draw your graph here

            // Restore the matrix
            //canvas.Restore();
            
        }

        private void ScaleCanvas()
        {
            float posX = canvas.Width / 2;
            float posY = canvas.Height / 2;
            //canvas.Translate(0, 0);
            //canvas.Scale(2, 2, posX, posY);

            //canvas.Translate(camera.CameraOffSetX, camera.CameraOffSetY);
            //canvas.Scale(zoomfactorX, zoomfactorY, posX + camera.CameraOffSetX, posY+camera.CameraOffSetY);
            canvas.Scale(zoomfactorX, zoomfactorY, posX, posY);



            //if (LastPivitPoint != null && PivitPoint != null)
            //{
            //    canvas.Translate(PivitPoint.x-LastPivitPoint.x, PivitPoint.y - LastPivitPoint.y);
            //}
            //if (PivitPoint != null)
            //{
            //    canvas.Scale(zoomfactorX, zoomfactorY, PivitPoint.x, PivitPoint.y);
            //}

            //if (midPoint != null)
            //    canvas.Scale(zoomfactorX, zoomfactorY, midPoint.x, midPoint.y);
            //else if(LastmidPoint != null)
            //    canvas.Scale(zoomfactorX, zoomfactorY, LastmidPoint.x, LastmidPoint.y);
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



            //canvas.DrawRect(0,0,100,100, p3);
            //canvas.DrawRect(100, 100, 200, 200, yellow);
            //canvas.DrawRect(200, 200, 300, 300, brown);

        }

        private void DrawTouching()
        {
            p1.Color = Color.Black;

            if (point1 != null && point2 != null && midPoint != null)
            {
                canvas.DrawCircle(point1.x, point1.y, 25, p1);
                canvas.DrawCircle(point2.x, point2.y, 25, p1);
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

            if (OnUpMotion(e))
                return true;

            Moving_Offset(e);
            Zooming(e);

            return true;

            //if (e.PointerCount > 1 && e.Action == MotionEventActions.Pointer2Down)
            //{
            //      point1 = new MyPoint((float)e.GetX(), (float)e.GetY());
            //      point2 = new MyPoint((float)e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), (float)e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));
            //      PivitPoint = new MyPoint((float)(point1.x + point2.x) / (float)2, (float)(point1.y + point2.y) / (float)2);
            //}
            //if(e.PointerCount > 1 && (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Pointer2Up))
            //{
            //    if (PivitPoint != null)
            //    {
            //        LastPivitPoint = new MyPoint(PivitPoint.x, PivitPoint.y);
            //    }
            //}
            //if (e.Action == MotionEventActions.PointerUp)
            //{
            //    point1 = null;
            //    point2 = null;
            //    midPoint = null;
            //    lastPlace2 = null;

            //}
            //if (e.Action == MotionEventActions.Up && e.PointerCount == 1)
            //{
            //    lastPlace = null;
            //    //Console.WriteLine("camera.CameraOffSetX is: " + camera.CameraOffSetX);
            //    return true;
            //}


            ////if (e.PointerCount == 1)
            ////{
            ////    if (lastPlace== null)
            ////    {
            ////        lastPlace = new MyPoint(e.GetX(), e.GetY());
            ////    }
            ////    //camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
            ////    //camera.CameraOffSetY += (float)e.GetY() - lastPlace.y;

            ////   // Console.WriteLine("touching: (" + (float)e.GetX() +"," + (float)e.GetY());
            ////}

            //if (e.PointerCount > 1)
            //{
            //    p1.Color = Color.Black;

            //    point1 = new MyPoint((float)e.GetX(), (float)e.GetY());
            //    point2 = new MyPoint((float)e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), (float)e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));
            //    //PivitPoint = new MyPoint((float)(point1.x + point2.x) / (float)2, (float)(point1.y + point2.y) / (float)2);

            //    if (e.Action == MotionEventActions.Pointer2Down || e.Action == MotionEventActions.Down)
            //    {
            //        midPoint = new MyPoint((point1.x + point2.x) / 2, (point1.y + point2.y) / 2);
            //        if(lastPlace2!= null)
            //        {
            //            LastmidPoint = new MyPoint((lastPlace.x + lastPlace2.x) / 2, (lastPlace.y + lastPlace2.y) / 2);
            //        }
            //    }
            //}

            ////if (lastPlace == null)
            ////{
            ////    lastPlace = new MyPoint(e.GetX(), e.GetY());
            ////    return true;
            ////}
            ////else
            ////{
            //if (e.Action == MotionEventActions.Move && e.PointerCount > 1)
            //{
            //    if (midPoint != null)
            //    {
            //        if(point2 != null)
            //        {
            //            if(lastPlace2 == null)
            //                lastPlace2 = new MyPoint(point2.x, point2.y);

            //            lastPlace2.x = point2.x; 
            //            lastPlace2.y = point2.y;
            //        }


            //        point2 = new MyPoint(e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));
            //        //if (lastPlace2 == null)
            //        //{
            //        //    lastPlace2 = new MyPoint(point2.x, point2.y);
            //        //}

            //        //zo
            //        //
            //        //zoomfactorX += ((float)e.GetX() - lastPlace.x) / 10000;// + ((float)point2.x - lastPlace2.x) / 100;
            //        //zoomfactorY += ((float)e.GetY() - lastPlace.y) / 10000;

            //        if(!((zoomfactorX - ((float)point2.x - lastPlace2.x) / 1000) <= 0.1))
            //        {
            //            zoomfactorX -= ((float)point2.x - lastPlace2.x) / 1000;
            //        }
            //        if (!((zoomfactorY - ((float)point2.y - lastPlace2.y) / 1000) <= 0.1))
            //        {
            //            zoomfactorY -= ((float)point2.y - lastPlace2.y) / 1000;
            //        }



            //    }
            //}

            //// }

            //if (lastPlace != null && e.PointerCount == 1)
            //{
            //    lastPlace.x = e.GetX();
            //    lastPlace.y = e.GetY();
            //}
            //return true;
        }

        //int counter = 0;
        private bool OnUpMotion(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Up && e.PointerCount == 1) //if the main pointer is up (i dont know what is the main pointer so iassum it is the firts one)
            {
                //point1 = null;
                lastPlace = null;

                //Console.WriteLine("excited  from counter==1 and mothion up");
                return true;
            }
            else if (e.PointerCount > 1)//i dint check if the code "MotionEventActions.PointerUp" works if there is only one pointer;
            {

                if ((e.Action == MotionEventActions.PointerUp || e.Action == MotionEventActions.Pointer2Up))
                {

                    if (PivitPoint != null)
                    {
                        if (LastPivitPoint == null)
                            LastPivitPoint = new MyPoint(PivitPoint.x, PivitPoint.y);
                        else
                        {
                            LastPivitPoint.x = PivitPoint.x;
                            LastmidPoint.y = PivitPoint.y;
                        }
                    }

                    midPoint = null;
                    point2 = null;
                    lastPlace2 = null;
                    point1 = null;
                    lastPlace = null;




                    //counter++;
                    //Console.WriteLine("reseting last place " + counter);
                    //return true;


                    //if (e.Action == MotionEventActions.Up && e.Action == MotionEventActions.PointerUp)
                    //{
                    //    //Console.WriteLine("primery and non primery pointer went up at the same time *");
                    //    midPoint = null;
                    //    point2 = null;
                    //    lastPlace2 = null;
                    //    point1 = null;
                    //    lastPlace = null;
                    //    return true;
                    //}
                    //if (e.Action == MotionEventActions.PointerUp)
                    //{
                    //   // Console.WriteLine("non primery  up **");
                    //    //point1 = new MyPoint(point2);
                    //    //lastPlace = new MyPoint(lastPlace2);

                    //    midPoint = null;
                    //    point2 = null;
                    //    lastPlace2 = null;

                    //}
                    //if (e.Action == MotionEventActions.Up)
                    //{
                    //   // Console.WriteLine("primery went up ***");
                    //    midPoint = null;

                    //    //point1 = new MyPoint(point2);
                    //    //lastPlace = new MyPoint(lastPlace2);
                    //    point1 = null;
                    //    lastPlace = null;

                    //    //point2 = null;
                    //    //lastPlace2 = null; 
                    //}
                }
            }
            return false;
        }

        private void Zooming(MotionEvent e)
        {

            if (e.PointerCount > 1)
            {
                SetUpZoomVariable(e);

                if (e.Action == MotionEventActions.Move)
                {
                    float deltaX = (point1.x - lastPlace.x) / 1000;
                    float deltaY = (point1.y - lastPlace.y) / 1000;

                    double vector1 = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                    float deltaX2 = (point2.x - lastPlace2.x) / 1000;
                    float deltaY2 = (point2.y - lastPlace2.y) / 1000;

                    float finaledeltaX = 0;
                    float finaledeltaY = 0;



                    if (!((zoomfactorX + deltaX2) <= 0.1))
                    {
                        zoomfactorX += deltaX2;
                    }
                    if (!((zoomfactorY - deltaY2) <= 0.1))
                    {
                        zoomfactorY -= deltaY2;
                    }
                }

            }
        }

        private void SetUpZoomVariable(MotionEvent e)
        {
            if (point1 == null)
                point1 = new MyPoint((float)e.GetX(), (float)e.GetY());

            if (point2 == null)
                point2 = new MyPoint((float)e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))), (float)e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))));

            //if (lastPlace == null)
            //  lastPlace = new MyPoint(point1.x, point1.y);

            if (lastPlace2 == null)
                lastPlace2 = new MyPoint(point2.x, point2.y);
            else
            {
                lastPlace2.x = point2.x;
                lastPlace2.y = point2.y;
            }

            if (e.Action == MotionEventActions.Pointer2Down || e.Action == MotionEventActions.Down)
            {
                if (LastmidPoint == null)
                    LastmidPoint = new MyPoint((point1.x + point2.x) / 2, (point1.y + point2.y) / 2);
                else
                {
                    LastmidPoint.x = (point1.x + point2.x) / 2;
                    LastmidPoint.y = (point1.y + point2.y) / 2;
                }

            }


            point1.y = (float)e.GetY();
            point1.x = (float)e.GetX();

            point2.x = (float)e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1)));
            point2.y = (float)e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1)));




            if (e.Action == MotionEventActions.Pointer2Down || e.Action == MotionEventActions.Down)
            {
                if (midPoint == null)
                    midPoint = new MyPoint((point1.x + point2.x) / 2, (point1.y + point2.y) / 2);
                else
                {
                    midPoint.x = (point1.x + point2.x) / 2;
                    midPoint.y = (point1.y + point2.y) / 2;
                }

                if (PivitPoint == null)
                    PivitPoint = new MyPoint((float)(point1.x + point2.x) / 2, (float)(point1.y + point2.y) / 2);
                else
                {
                    PivitPoint.x = (point1.x + point2.x) / 2;
                    PivitPoint.y = (point1.y + point2.y) / 2;
                }

            }
        }

        private void Moving_Offset(MotionEvent e)
        {
            if (e.PointerCount == 1)
            {

                if (lastPlace == null)
                {
                    lastPlace = new MyPoint(e.GetX(), e.GetY());
                }
                if (e.Action == MotionEventActions.Down)
                {
                    lastPlace.x = e.GetX();
                    lastPlace.y = e.GetY();
                }

                camera.CameraOffSetX += (float)e.GetX() - lastPlace.x;
                camera.CameraOffSetY += (float)e.GetY() - lastPlace.y;


                lastPlace.x = e.GetX();
                lastPlace.y = e.GetY();
            }
        }

    }





    /*
    class ZoomableCanvasView : View
    {
        private float scaleFactor = 1.0f;
        private float focusX = 0.0f;
        private float focusY = 0.0f;
        private Matrix matrix = new Matrix();

        Paint p3 = new Paint();
        Paint brown = new Paint();
        Paint yellow = new Paint();

        public ZoomableCanvasView(Context context) : base(context)
        {
            // Enable scaling gestures
            p3.Color = Color.Purple;
            yellow.Color = Color.Yellow;
            brown.Color = Color.Brown;
            this.SetOnTouchListener(new ScaleGestureListener(this));
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            
            
            // Apply the zoom transformation
            canvas.Save();
            canvas.Scale(scaleFactor, scaleFactor, focusX, focusY);

            // Draw your graph here
            canvas.DrawRect(0, 0, 100, 100, p3);
            canvas.DrawRect(100, 100, 200, 200, yellow);
            canvas.DrawRect(200, 200, 300, 300, brown);
            canvas.DrawRect(300, 300, 400, 400, p3);
            canvas.DrawRect(400, 400, 500, 500, yellow);
            canvas.DrawRect(500, 500, 600, 600, brown);

            canvas.Restore();
        }

        public void Zoom(float scaleFactor, float focusX, float focusY)
        {
            this.scaleFactor *= scaleFactor;
            this.focusX = focusX;
            this.focusY = focusY;
            this.Invalidate();
        }
    }

    class ScaleGestureListener : Java.Lang.Object, View.IOnTouchListener
    {
        private readonly ZoomableCanvasView view;
        private ScaleGestureDetector detector;

        public ScaleGestureListener(ZoomableCanvasView view)
        {
            this.view = view;
            this.detector = new ScaleGestureDetector(view.Context, new Temp_ScaleListener(view));
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            detector.OnTouchEvent(e);
            return true;
        }
    }

    class Temp_ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
    {
        private readonly ZoomableCanvasView view;

        public Temp_ScaleListener(ZoomableCanvasView view)
        {
            this.view = view;
        }

        public override bool OnScale(ScaleGestureDetector detector)
        {
            view.Zoom(detector.ScaleFactor, detector.FocusX, detector.FocusY);
            return true;
        }
    }
    */

    // Custom View class that implements zoom and translate functionality for a graph
    class ZoomableCanvasView : View
    {

        MyPoint PivotPoint = new MyPoint();
        MyPoint LastPivotPoint = new MyPoint();
        float ScaleX = 1.0f;
        float ScaleY = 1.0f;
        float w;
        float h;
        // Variables to store the current scale factor, focus point for scaling, and translation values
        private float scaleFactor = 1.0f;


        private float focusX = 0.0f;
        private float focusY = 0.0f;
        
        private float translateX = 0.0f;
        private float translateY = 0.0f;

        private Matrix matrix = new Matrix();

        Canvas Thecanvas;
        Paint p3 = new Paint();
        Paint brown = new Paint();
        Paint yellow = new Paint();
        Paint red = new Paint();
        Paint green = new Paint();
        Paint blue = new Paint();
        // Constructor for the custom view
        public ZoomableCanvasView(Context context) : base(context)
        {
            p3.Color = Color.Purple;
            yellow.Color = Color.Yellow;
            brown.Color = Color.Brown;
            green.Color = Color.Green;
            blue.Color = Color.Blue;
            red.Color = Color.Red;

            PivotPoint.x = 0.0f;
            PivotPoint.y = 0.0f;

            LastPivotPoint.x = 0.0f;
            LastPivotPoint.y = 0.0f;

            // Enable scaling and translation gestures on the view
            this.SetOnTouchListener(new Temp_ScaleAndTranslateGestureListener(this,PivotPoint));
        }

        // Method to draw the graph on the canvas
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            Thecanvas= canvas;
            w = Thecanvas.Width;
            h = Thecanvas.Height;

           //Test_Multupale_ZoominIn();
            //ReScaleCanvas();

            //// Apply the zoom and translation transformations to the canvas
            Thecanvas.Save();
            matrix.Reset();

            //matrix.PostScale(scaleFactor, scaleFactor, focusX, focusY);
            if (PivotPoint != null)
                matrix.PostScale(scaleFactor, scaleFactor, PivotPoint.x, PivotPoint.y);
            else
               matrix.PostScale(scaleFactor, scaleFactor, LastPivotPoint.x, LastPivotPoint.y);
            //matrix.PostTranslate(translateX, translateY);

            Thecanvas.Concat(matrix);

            // Draw your graph here
            DrawStaff();
            


            
            //Thecanvas.Restore();
            Thecanvas.DrawCircle(focusX, focusY, 25, p3);

            this.Invalidate();
        }

        private void DrawStaff()
        {
            //Thecanvas.DrawRect(0, 0, 100, 100, p3);
            //Thecanvas.DrawRect(100, 100, 200, 200, yellow);
            //Thecanvas.DrawRect(200, 200, 300, 300, brown);
            //Thecanvas.DrawRect(300, 300, 400, 400, p3);
            //Thecanvas.DrawRect(400, 400, 500, 500, yellow);
            //Thecanvas.DrawRect(500, 500, 600, 600, brown);

            

            Thecanvas.DrawRect(w / 4, h / 4, w * (3/4.0f), h*(3/4.0f), brown); //half size of canvas and in the middle of it;
            Thecanvas.DrawRect(w / 4, h / 4, w * (2 / 4.0f), h * (2 / 4.0f),red);//
            Thecanvas.DrawRect(w / 4, h / 4, w * (3 / 8.0f), h * (3 / 8.0f), green);//

            Thecanvas.DrawRect(w / 4, h / 4, w * (5 / 16.0f), h * (5 / 16.0f), blue);//
            Thecanvas.DrawRect(w / 4, h / 4, w * (9 / 32.0f), h * (9 / 32.0f), yellow);//
            Thecanvas.DrawRect(w / 4, h / 4, w * (17 / 64.0f), h * (17 / 64.0f), p3);//
            Thecanvas.DrawRect(w / 4, h / 4, w * (33 / 128.0f), h * (33 / 128.0f), brown);//
            Thecanvas.DrawRect(w / 4, h / 4, w * (65 / 256.0f), h * (65 / 256.0f), red);//
        }

        public void Test_Multupale_ZoominIn()
        {
            Thecanvas.Save();
            matrix.Reset();


            matrix.PostScale(2, 2, w / 2, h / 2);
            matrix.PostScale(2, 2, w / 2, h / 2);
            //matrix.PostScale(2, 2, w * (1 / 4.0f), h * (1 / 4.0f));
            //matrix.PostScale(2, 2, w * (3 / 8.0f), h * (3 / 8.0f));

            //matrix.PostTranslate(translateX, translateY);
            Thecanvas.Concat(matrix);
            //Thecanvas.Save();
           
        }

        private void ReScaleCanvas()
        {
            Thecanvas.Save();
            matrix.Reset();

            //matrix.PostScale(scaleFactor, scaleFactor, focusX, focusY);
            if (PivotPoint != null)
            {
                matrix.PostScale(scaleFactor, scaleFactor, PivotPoint.x, PivotPoint.y);
                //matrix.PostScale(ScaleX, ScaleY, PivotPoint.x, PivotPoint.y);
            }

            else
            {
                matrix.PostScale(scaleFactor, scaleFactor, LastPivotPoint.x, LastPivotPoint.y);
                //matrix.PostScale(ScaleX, ScaleY, LastPivotPoint.x, LastPivotPoint.y);
            }

            //matrix.PostTranslate(translateX, translateY);
            Thecanvas.Concat(matrix);
        }





        // Method to zoom in or out on the graph
        public void Zoom(float scaleFactor, float focusX, float focusY)
        {
            this.scaleFactor *= scaleFactor;
            this.focusX = focusX;
            this.focusY = focusY;

            //LastPivotPoint.y = focusY;
            //LastPivotPoint.x = focusX;
            if (PivotPoint.x != focusX)
            {
                //Thecanvas.Save();
                PivotPoint.y = focusY;
                PivotPoint.x = focusX;
                Console.WriteLine("PivotPoint.x: + " + PivotPoint.x);
                Console.WriteLine("PivotPoint.y : + " + PivotPoint.y);
                Console.WriteLine(" ");
            }
            //PivotPoint.y = focusY;
           // PivotPoint.x = focusX;

            


            this.Invalidate();
        }
        //public void Zoom(float ScaleX1,float ScaleY1, float focusX, float focusY)
        //{
        //    //this.scaleFactor *= scaleFactor;
        //    if(this.ScaleX * ScaleX1 > 0.1)
        //        this.ScaleX *= ScaleX1;
        //    if (this.ScaleY * ScaleY1 > 0.1 && this.ScaleY * ScaleY1 < 10)
        //        this.ScaleY *= ScaleY1;

        //    Console.WriteLine("ScaleX: + " + ScaleX);
        //    Console.WriteLine("ScaleY: " + ScaleY);
        //    Console.WriteLine("*************************" + "\n\n\n");

        //    this.focusX = focusX;
        //    this.focusY = focusY;

        //    //LastPivotPoint.y = focusY;
        //    //LastPivotPoint.x = focusX;
        //    if (PivotPoint.x != focusX)
        //    {
        //        //Thecanvas.Save();
        //    }
        //    PivotPoint.y = focusY;
        //    PivotPoint.x = focusX;

        //    this.Invalidate();
        //}
        // Method to translate the graph
        public void Translate(float translateX, float translateY)
        {
            this.translateX += translateX;
            this.translateY += translateY;
            this.Invalidate();
        }
    }

    // Class to handle scaling and translation gestures
    class Temp_ScaleAndTranslateGestureListener : Java.Lang.Object, View.IOnTouchListener
    {
        private readonly ZoomableCanvasView view;
        private ScaleGestureDetector scaleDetector;
        public static MyPoint PivotPoint;
        
        private float lastTouchX;
        private float lastTouchY;

        public Temp_ScaleAndTranslateGestureListener(ZoomableCanvasView view, MyPoint PivotPoint1)
        {
            PivotPoint= PivotPoint1;

            this.view = view;
            this.scaleDetector = new ScaleGestureDetector(view.Context, new Temp_ScaleListener(view));


        }

        public bool OnTouch(View v, MotionEvent e)
        {
            
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    // Store the initial touch coordinates
                    lastTouchX = e.GetX();
                    lastTouchY = e.GetY();
                    break;
                case MotionEventActions.PointerUp:
                    if (e.PointerCount > 1)
                    {
                        lastTouchX = e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1)));
                        lastTouchY = e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1)));
                    }
                    else
                    {
                        lastTouchX = e.GetX();
                        lastTouchY = e.GetY();
                    }
                    PivotPoint = null;
                    break;
                case MotionEventActions.Pointer2Up:
                    if (e.PointerCount > 1)
                    {
                        lastTouchX = e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(0)));
                        lastTouchY = e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(0)));
                    }
                    else
                    {
                        lastTouchX = e.GetX();
                        lastTouchY = e.GetY();
                    }
                    PivotPoint = null;
                    break;
                case MotionEventActions.Move:
                    if (e.PointerCount == 1)
                    {
                        float translateX = e.GetX() - lastTouchX;
                        float translateY = e.GetY() - lastTouchY;

                        // Translate the graph by the calculated distance
                        view.Translate(translateX, translateY);

                        // Update the last touch coordinates with the current touch coordinates
                        lastTouchX = e.GetX();
                        lastTouchY = e.GetY();
                    }
                    break;
                
            }

            // Pass the touch event to the scale gesture detector to handle scaling gestures
            scaleDetector.OnTouchEvent(e);

            return true;
        }

        // Class to handle scaling gestures
        private class Temp_ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            private readonly ZoomableCanvasView view;
            float ScaleY = 1.0f;
            float ScaleX = 1.0f;
            public Temp_ScaleListener(ZoomableCanvasView view)
            {
                this.view = view;
            }

            public override bool OnScale(ScaleGestureDetector detector)
            {
                //Console.WriteLine("detector event time: + " + detector.EventTime);
                if(PivotPoint == null)
                {
                    PivotPoint = new MyPoint(detector.FocusX, detector.FocusY);
                }

                ScaleX = detector.CurrentSpanX/ detector.PreviousSpanX;
                ScaleY = detector.CurrentSpanY / detector.PreviousSpanY;

                //Console.WriteLine("ScaleX: + " + ScaleX);
                //Console.Write("ScaleY: " + ScaleY);
                //Console.WriteLine("*************************" + "\n\n\n");
                // Zoom in or out on the graph based on the scale factor
                //view.Zoom(detector.ScaleFactor, detector.FocusX, detector.FocusY);

                //ScaleY = (float)Math.Max(ScaleY, 0.91);

                view.Zoom(detector.ScaleFactor, PivotPoint.x, PivotPoint.y);
                //view.Zoom(ScaleX, ScaleY, PivotPoint.x, PivotPoint.y);
                return true;
            }
        }
    }
}
            