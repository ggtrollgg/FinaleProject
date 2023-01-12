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
    internal class MyCamera
    {
        public float CameraOffSetX = 0;
        public float CameraOffSetY = 0;
        //
        public MyCamera(float cameraOffSetX, float cameraOffSetY)
        {
            CameraOffSetX = cameraOffSetX;
            CameraOffSetY = cameraOffSetY;
        }
        public MyCamera()
        {

        }


    }
}