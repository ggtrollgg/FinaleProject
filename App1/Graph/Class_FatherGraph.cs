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

namespace App1
{
    public class Class_FatherGraph : View
    {

        public Context context;
        public Canvas canvas;
        public List<DataPoint> dataPoints;

        public MyCamera camera = new MyCamera(0, 0);
        public MyPoint midPoint;

        public float test_zoomfactor = 1;
        public float zoomfactor_X = 1;
        public float zoomfactor_Y = 1;

        public float daltaOffsetX = 0;
        public float daltaOffsetY = 0;


        /// <param name="context"></param>

        public Class_FatherGraph(Context context) : base(context)
        {
            this.SetOnTouchListener(new ScaleAndTranslateGestureListener(this, new MyPoint(0,0)));
        }

        public Class_FatherGraph(Context context, Canvas canvas, List<DataPoint> dataPoints) : base(context)
        {
            this.context = context;
            this.canvas = canvas;
            this.dataPoints = dataPoints;
        }
        public void SetDataPoints(List<DataPoint> points)
        {
            this.dataPoints = points;
        }
        

        public void ZoomBy(float zoomfactor_x , float zoomfactor_y)
        {
            if(dataPoints.Count != 0 && canvas != null)
            {
                int distance = canvas.Width/dataPoints.Count;
                //Console.WriteLine("(test_zoomfactor + zoomfactor_x)! < 0.8), is: " + ((test_zoomfactor + zoomfactor_x)! < 0.8));
                //Console.WriteLine("(test_zoomfactor + zoomfactor_x) !> (dataPoints.Count/2.0), is: " + (bool)((test_zoomfactor + zoomfactor_x)! > (dataPoints.Count / 2.0)));
                //Console.WriteLine("!(test_zoomfactor + zoomfactor_x) < 0.8), is: " + !((test_zoomfactor + zoomfactor_x) < 0.8));
                //Console.WriteLine("!(test_zoomfactor + zoomfactor_x) > (dataPoints.Count/2.0), is: " + !((test_zoomfactor + zoomfactor_x) > (dataPoints.Count / 2.0)));
                //Console.WriteLine(" ");
                if (!((test_zoomfactor + zoomfactor_x) < 0.8) && !((test_zoomfactor + zoomfactor_x) > (dataPoints.Count / 2.0))) // the graph can be spreard across 0.8 of the screen, or it can strech so the distance between each point is half canvas width
                {
                    daltaOffsetX = zoomfactor_x * 10 / this.zoomfactor_X;
                    this.zoomfactor_X += zoomfactor_x;
                    //this.zoomfactor_Y += zoomfactor_y;


                    //daltaOffsetX = zoomfactor_x * 100;
                    test_zoomfactor += zoomfactor_x;

                    camera.X_zoom_changed = true;
                }
                //if (!((zoomfactor_Y + zoomfactor_y) < 0.4) && !((zoomfactor_Y + zoomfactor_y) > (dataPoints.Count / 0.5))) // the graph can be spreard across 0.4 of the screen, or it can strech so the distance between each point is 2 times canvas width
                //{
                //    daltaOffsetY = zoomfactor_y * 10 / this.zoomfactor_Y;
                //    this.zoomfactor_Y += zoomfactor_y;
                //    camera.Y_zoom_changed = true;
                //}

                //Console.WriteLine(" ");
                //Console.WriteLine("changed the scale factor");
                //Console.WriteLine("zoomfactor_x is: " + zoomfactor_X);
                //Console.WriteLine("zoomfactor_Y is: " + zoomfactor_Y);
                //Console.WriteLine(" ");
            }

        }
        public void OffsetBy(float offset_x , float offset_y)
        {
            if (!(camera.CameraOffSetX + offset_x >= 0))
            {
                camera.CameraOffSetX += offset_x;
                camera.X_changed = true;
            }

            //if (!(camera.CameraOffSetY + offset_y <= -canvas.Height+200)  && !(camera.CameraOffSetY + offset_y >= canvas.Height-200))
            //{
                camera.CameraOffSetY += offset_y;
                daltaOffsetY = offset_y;
                camera.Y_changed = true;
            //}

            
          

            //Console.WriteLine(" ");
            //Console.WriteLine("changed the offset");
            //Console.WriteLine("offset_x is: " + camera.CameraOffSetX);
            //Console.WriteLine("offset_Y is: " + camera.CameraOffSetY);
            //Console.WriteLine(" ");
        }

        public void SetNewMidPoint(float x, float y)
        {
            midPoint = new MyPoint(x, y);
        }
        public void SetNewMidPoint(MyPoint piv)
        {
            midPoint = piv;
        }


        //public void ReSizeCanvas(double zoomFactor_x, double zoomFactor_y) 
        //{
        //    Matrix matrix = new Matrix();

        //    // Save the current matrix
        //    matrix.Save();

        //    // Translate the origin to the center of the canvas
        //    matrix.Translate(canvas.Width / 2, canvas.Height / 2);

        //    // Scale the canvas by a factor of 2
        //    matrix.Scale(2, 2);

        //    // Translate the origin back to the top left corner of the canvas
        //    matrix.Translate(-canvas.Width / 2, -canvas.Height / 2);

        //    // Set the transformation on the canvas
        //    canvas.Transform(matrix);

        //    // Draw your graph here

        //    // Restore the matrix
        //    matrix.Restore();
        //}
    }

    //public interface Class_FatherGraph : View
    //{

    //    public Context context;
    //    public Canvas canvas;
    //    public List<DataPoint> dataPoints;



    //    /// <param name="context"></param>

    //    public Class_FatherGraph(Context context) : base(context)
    //    {

    //    }

    //    public Class_FatherGraph(Context context, Canvas canvas, List<DataPoint> dataPoints) : base(context)
    //    {
    //        this.context = context;
    //        this.canvas = canvas;
    //        this.dataPoints = dataPoints;
    //    }




    class ScaleAndTranslateGestureListener : Java.Lang.Object, View.IOnTouchListener
    {
        private readonly Class_FatherGraph view;
        private ScaleGestureDetector scaleDetector;
        public static MyPoint PivotPoint;

        private float lastTouchX;
        private float lastTouchY;
        private bool isNotMoving = false;

        public ScaleAndTranslateGestureListener(Class_FatherGraph view, MyPoint PivotPoint1)
        {
            PivotPoint = PivotPoint1;

            this.view = view;
            this.scaleDetector = new ScaleGestureDetector(view.Context, new ScaleListener(view));


        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if (view.midPoint == null && e.PointerCount == 2)
            {
                //Console.WriteLine("touching with 2 fingers");

                view.SetNewMidPoint((e.GetX()+ e.GetAxisValue(Axis.X, e.FindPointerIndex(e.GetPointerId(1))))/2,(e.GetY()+ e.GetAxisValue(Axis.Y, e.FindPointerIndex(e.GetPointerId(1))))/2);
                
            }
            switch (e.Action)
            {
               
                case MotionEventActions.Down:
                    // Store the initial touch coordinates
                    lastTouchX = e.GetX();
                    lastTouchY = e.GetY();
                    PivotPoint = null;
                    view.SetNewMidPoint(null);
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
                    //PivotPoint = null;
                    break;
                case MotionEventActions.Move:
                    if (e.PointerCount == 1)
                    {
                        float translateX = e.GetX() - lastTouchX;
                        float translateY = e.GetY() - lastTouchY;

                        view.OffsetBy(translateX, translateY);
                        //view.OffsetBy(translateX, 0);


                        // Translate the graph by the calculated distance
                        //view.Translate(translateX, translateY);

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
        private class ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            private readonly Class_FatherGraph view;
            float ScaleY = 1.0f;
            float ScaleX = 1.0f;
            public ScaleListener(Class_FatherGraph view)
            {
                this.view = view;
            }

            public override bool OnScale(ScaleGestureDetector detector)
            {
                //Console.WriteLine("detector event time: + " + detector.EventTime);
                if (PivotPoint == null)
                {
                    PivotPoint = new MyPoint(detector.FocusX, detector.FocusY);
                    view.SetNewMidPoint(PivotPoint.x, PivotPoint.y);
                }

                //ScaleX = detector.CurrentSpanX / detector.PreviousSpanX;
                //ScaleY = detector.CurrentSpanY / detector.PreviousSpanY;

                //ScaleX = (detector.CurrentSpanX - detector.PreviousSpanX) / 100;
                //ScaleY = (detector.CurrentSpanY - detector.PreviousSpanY) / 100;

                ScaleX = (detector.CurrentSpanX - detector.PreviousSpanX)/100*view.zoomfactor_X;
                ScaleY = (detector.CurrentSpanY - detector.PreviousSpanY)/100*view.zoomfactor_Y;

                view.ZoomBy(ScaleX, ScaleY);

                //Console.WriteLine("ScaleX: + " + ScaleX);
                //Console.Write("ScaleY: " + ScaleY);
                //Console.WriteLine("*************************" + "\n\n\n");
                // Zoom in or out on the graph based on the scale factor
                //view.Zoom(detector.ScaleFactor, detector.FocusX, detector.FocusY);

                //ScaleY = (float)Math.Max(ScaleY, 0.91);

                //view.Zoom(detector.ScaleFactor, PivotPoint.x, PivotPoint.y);

                //view.Zoom(ScaleX, ScaleY, PivotPoint.x, PivotPoint.y);



                return true;
            }
        }
    }
}