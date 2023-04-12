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
using App1.General_Classes;

namespace App1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@drawable/stocks_icon")]
    public class MainActivity : Activity
    {

        public static ISharedPreferences sp;
        Button btnstart,btnToListView,btnExit,btnTest;
        Intent intent2;
        //

        
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            btnstart = FindViewById<Button>(Resource.Id.btnstart);
            btnExit = FindViewById<Button>(Resource.Id.btnExit);
            btnToListView = FindViewById<Button>(Resource.Id.btnToListView);
            btnTest = FindViewById<Button>(Resource.Id.btnTest);

            btnstart.Click += Btnstart_Click;
            btnExit.Click += BtnExit_Click;
            btnToListView.Click += BtnToListView_Click;
            btnTest.Click += BtnTest_Click;
            sp = GetSharedPreferences("KeysForFinaleProject", FileCreationMode.Private);



            //API_Keys.Add(new API_Key("0a0b32a8d57dc7a4d38458de98803860"));

            //0a0b32a8d57dc7a4d38458de98803860  //ggtroll 35
            //8bdedb14d7674def460cb3a84f1fd429 //ggtroll 36
            //561897c32bf107b87c107244081b759f //ggtroll 37


            Console.WriteLine();
            if (intent2 != null)
            {
                StopService(intent2);
            }
            intent2 = new Intent(this, typeof(MyService));
            StartService(intent2);


        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(Testing_Database_Activity));
            //intent.PutExtra("symbol", "OB");
            StartActivity(intent);
            
        }

        //buttons
        private void BtnToListView_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(StockViewActivity));
            StartActivity(intent);
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Finish();
        }

        private void Btnstart_Click(object sender, EventArgs e)
        {
            //Intent intent = new Intent(this, typeof(ChartActivity));
            Intent intent = new Intent(this, typeof(SearchActivity));
            StartActivity(intent);

        }

    }
}