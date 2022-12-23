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
    public class MainActivity : Activity
    {
        Button btnstart,btnToListView,btnExit;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            btnstart = FindViewById<Button>(Resource.Id.btnstart);
            btnExit = FindViewById<Button>(Resource.Id.btnExit);
            btnToListView = FindViewById<Button>(Resource.Id.btnToListView);


            btnstart.Click += Btnstart_Click;
            btnExit.Click += BtnExit_Click;
            btnToListView.Click += BtnToListView_Click;

            String symbol = "AAPL";
            string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
            link = link.Insert(link.Length, symbol);
            link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");
            
            if (link != "https://financialmodelingprep.com/api/v3/historical-chart/1min/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860")
            {
                Toast.MakeText(this, "false", ToastLength.Long).Show();
                Console.WriteLine(link);
                Console.WriteLine("https://financialmodelingprep.com/api/v3/historical-chart/1min/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860");
            }

        }

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