using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App1
{
    public class Class_CandleGraph : Class_FatherGraph
    {
        Paint p;


        List<float> values = new List<float>();
        float heighest = 0, lowest = -1;


        List<MyPoint> points = new List<MyPoint>();
        List<MyPoint> Changedpoints = new List<MyPoint>();

        MyCamera camera = new MyCamera(0, 0);

        MyPoint lastPlace;
        MyPoint lastPlace2;
        float test_zoomfactor = 1;


        public bool Zoom = false;
        public bool Move = false;

        Paint p1 = new Paint();
        MyPoint point1;
        MyPoint point2;
        MyPoint midPoint;


        public Class_CandleGraph(Context context) : base(context)
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
                if (values == null || values.Count == 0) { calculateValues(); }
                if (heighest == 0) { findLowHeigh(); }
                CreatChartPoints();
                DrawPoints();
                //DrawTouching();
                //DrawXexis();
                Invalidate();
            }
        }

        private void calculateValues()
        {
            foreach (DataPoint i in dataPoints)
            {
                float avr = (i.low + i.heigh);
                values.Add(avr);
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
        public void CreatChartPoints()
        {
            if (points == null || points.Count == 0)
            {
                points = new List<MyPoint>();
                Changedpoints = new List<MyPoint>();

                for (int i = 0; i < values.Count; i++)
                {
                    points.Add(new MyPoint((i * canvas.Width) / (values.Count - 1), canvas.Height + ((lowest - values[i]) * (1 / (heighest - lowest)) * canvas.Height)));
                }
                CalculateNewPointes();
            }
        }

        private void CalculateNewPointes()
        {
            for (int i = 0; i < values.Count; i++)
            {
                Changedpoints.Add(new MyPoint((points[i].x) * test_zoomfactor + camera.CameraOffSetX, points[i].y + camera.CameraOffSetY));
            }
        }

        public void DrawPoints()
        {
            for (int i = 0; i < values.Count; i++)
            {
                canvas.DrawCircle(Changedpoints[i].x, Changedpoints[i].y, 2, p);
                if (i != values.Count - 1)
                {
                    canvas.DrawLine(Changedpoints[i].x, Changedpoints[i].y, Changedpoints[i + 1].x, Changedpoints[i + 1].y, p);
                }
            }
        }

    }
}