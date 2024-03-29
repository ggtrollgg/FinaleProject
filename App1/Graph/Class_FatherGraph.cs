﻿using Android.App;
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
        public Paint background;
        public Paint textPaint_Price;
        public Paint textPaint_Date;
        public MyCamera camera = new MyCamera(0, 0);
        public MyPoint midPoint;
        public float price_text_start_x = 0;
        public float date_text_start_y = 0;
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

            //default values
            price_text_start_x = canvas.Width*8/ (float)10.0; 
            date_text_start_y = canvas.Height * 18 / (float)20.0;
            this.SetOnTouchListener(new ScaleAndTranslateGestureListener(this, new MyPoint(0, 0)));
        }
        public void SetDataPoints(List<DataPoint> points)
        {
            this.dataPoints = points;
        }
        
        //add to zoom scale the values of zoomfactor_x and zoomfactor y
        public void ZoomBy(float zoomfactor_x , float zoomfactor_y)
        {
            if(dataPoints.Count != 0 && canvas != null)
            {
                int distance = canvas.Width/dataPoints.Count;
                if (!((test_zoomfactor + zoomfactor_x) < 0.8) && !((test_zoomfactor + zoomfactor_x) > (dataPoints.Count / 2.0))) // the graph can be spreard across 0.8 of the screen, or it can strech so the distance between each point is half canvas width
                {
                    daltaOffsetX = zoomfactor_x * 10 / this.zoomfactor_X;
                    this.zoomfactor_X += zoomfactor_x;
                    test_zoomfactor += zoomfactor_x;

                    camera.X_zoom_changed = true;
                }

            }

        }

        //offset the camera by these values
        public void OffsetBy(float offset_x , float offset_y)
        {
            if (!(camera.CameraOffSetX + offset_x >= 0))
            {
                camera.CameraOffSetX += offset_x;
                camera.X_changed = true;
            }

                camera.CameraOffSetY += offset_y;
                daltaOffsetY = offset_y;
                camera.Y_changed = true;
        }

        //sey new mid/pivitPoint point 
        //mid/pivitPoints is the point betwween two of my fingers that i zoom in/out of
        public void SetNewMidPoint(float x, float y)
        {
            midPoint = new MyPoint(x, y);
        }
        public void SetNewMidPoint(MyPoint piv)
        {
            midPoint = piv;
        }


        
    }

  

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

        //on touch detection calculates the new pivitpoints,the change in offset
        public bool OnTouch(View v, MotionEvent e)
        {
            if (view.midPoint == null && e.PointerCount == 2)
            {
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
                    break;
                case MotionEventActions.Move:
                    if (e.PointerCount == 1)
                    {
                        float translateX = e.GetX() - lastTouchX;
                        float translateY = e.GetY() - lastTouchY;

                        view.OffsetBy(translateX, translateY);
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
            //on scale hand gester detection calculate the change in scale factor that needed to be added to the zoom factor
            public override bool OnScale(ScaleGestureDetector detector)
            {
                //Console.WriteLine("detector event time: + " + detector.EventTime);
                if (PivotPoint == null)
                {
                    PivotPoint = new MyPoint(detector.FocusX, detector.FocusY);
                    view.SetNewMidPoint(PivotPoint.x, PivotPoint.y);
                }

                ScaleX = (detector.CurrentSpanX - detector.PreviousSpanX)/100*view.zoomfactor_X;
                ScaleY = (detector.CurrentSpanY - detector.PreviousSpanY)/100*view.zoomfactor_Y;

                view.ZoomBy(ScaleX, ScaleY);

                return true;
            }
        }
    }
}