using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using System.Net.Http;


//using MatthiWare.FinancialModelingPrep;
using System;
using Android.Widget;
using System.Threading.Tasks;
using System.Threading;
using Org.Json;
using System.Collections.Generic;
using Android.Content;

namespace App1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button btnstart,btnToActivity2;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            btnstart = FindViewById<Button>(Resource.Id.btnstart);
            btnstart.Click += Btnstart_Click;
            
            //btnToActivity2 = FindViewById<Button>(Resource.Id.btnToActivity2);
            //btnToActivity2.Click += BtnToActivity2_Click;


        }


        private void Btnstart_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(ChartActivity));
            StartActivity(intent);

        }


        

       



    }
}