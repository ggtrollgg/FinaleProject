using Android.App;
using Android.Content;
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
    public class MyCamera
    {
        public float CameraOffSetX = 0;
        public float CameraOffSetY = 0;

        public bool X_changed = false;
        public bool Y_changed = false;
        public bool X_zoom_changed = false;
        public bool Y_zoom_changed = false;
        public MyCamera(float cameraOffSetX, float cameraOffSetY)
        {
            CameraOffSetX = cameraOffSetX;
            CameraOffSetY = cameraOffSetY;
        }



    }
}