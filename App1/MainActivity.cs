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
using Android.Gms.Common.Api.Internal;
using static Android.Provider.CallLog;
using static System.Net.Mime.MediaTypeNames;
using static Xamarin.Essentials.Platform;
using Intent = Android.Content.Intent;

namespace App1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@drawable/stocks_icon")]
    public class MainActivity : Activity
    {

        public static ISharedPreferences sp;//,offsp;
        public static Manager_API_Keys Manager_API_Keys;
        Button btnstart,btnToListView,btnExit,btnTest;
        Android.Content.Intent intent2;
        PendingIntent pendingIntent;
        bool offlineMode = false;


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
            //offsp = GetSharedPreferences("OfflineMode", FileCreationMode.Private);


            Manager_API_Keys = new Manager_API_Keys();


            //API_Keys.Add(new API_Key("0a0b32a8d57dc7a4d38458de98803860"));

            //0a0b32a8d57dc7a4d38458de98803860  //ggtroll 35
            //8bdedb14d7674def460cb3a84f1fd429 //ggtroll 36
            //561897c32bf107b87c107244081b759f //ggtroll 37
            ResetKeys_Alaram_setup();

            
            


            Console.WriteLine();
            if (intent2 != null)
            {
                
                StopService(intent2);
            }
            intent2 = new Intent(this, typeof(TrackingService));
            StartService(intent2);


           
        }

        private void ResetKeys_Alaram_setup()
        {
            AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);
            if (alarmManager.NextAlarmClock != null)
                Console.WriteLine("netxt allarm clock is:   " + alarmManager.NextAlarmClock);

            if (alarmManager.NextAlarmClock == null)
            {
                TimeSpan UntillMidNight = DateTime.MaxValue.TimeOfDay - DateTime.Now.TimeOfDay;
                //TimeSpan UntillMidNight = DateTime.Now.TimeOfDay - DateTime.Now.TimeOfDay;
                int UntillMidNight_Mili = (int)(UntillMidNight.TotalMilliseconds);

                Intent intent = new Intent(this, typeof(ResetKeys));
                pendingIntent = null;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                {
                    pendingIntent = PendingIntent.GetBroadcast
                           (this, 1, intent, PendingIntentFlags.Mutable);
                }
                else
                {
                    pendingIntent = PendingIntent.GetBroadcast
                           (this, 1, intent, PendingIntentFlags.OneShot);//PendingIntent.FLAG_ONE_SHOT);
                }

                alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + UntillMidNight_Mili, pendingIntent);
            }
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            
            offlineMode = !offlineMode;
            Toast.MakeText(this, "offlineMode is: " + offlineMode, ToastLength.Short).Show();
        }


        //buttons
        private void BtnToListView_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(StockViewActivity));
            intent.PutExtra("OfflineMode", offlineMode);
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
            intent.PutExtra("OfflineMode", offlineMode);
            StartActivity(intent);

        }

    }
}