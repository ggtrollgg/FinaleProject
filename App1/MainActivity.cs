﻿using Android.App;
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
        Button btnstart,btnToListView,btnExit, BtnOffLineMode;
        Android.Content.Intent intent2;
        PendingIntent pendingIntent;
        bool offlineMode = false;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            btnstart = FindViewById<Button>(Resource.Id.btnstart);
            btnExit = FindViewById<Button>(Resource.Id.btnExit);
            btnToListView = FindViewById<Button>(Resource.Id.btnToListView);
            BtnOffLineMode = FindViewById<Button>(Resource.Id.btnTest);

            btnstart.Click += Btnstart_Click;
            btnExit.Click += BtnExit_Click;
            btnToListView.Click += BtnToListView_Click;
            BtnOffLineMode.Click += BtnOffLineMode_Click;



            sp = GetSharedPreferences("KeysForFinaleProject", FileCreationMode.Private);
            ResetKeys_Alaram_setup();



            Manager_API_Keys = new Manager_API_Keys();

            Console.WriteLine();
            if (intent2 != null)
            {
                
                StopService(intent2);
            }
            intent2 = new Intent(this, typeof(TrackingService));
            StartService(intent2);


           
        }

        
        private void BtnOffLineMode_Click(object sender, EventArgs e)
        {
            offlineMode = !offlineMode;
            Toast.MakeText(this, "offlineMode is: " + offlineMode, ToastLength.Short).Show();
        }

        //Create an Alarm Manager that Resets the amount of callsRemain in the keys in SharedPrefrence
        private void ResetKeys_Alaram_setup()
        {
            AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);
            if (alarmManager.NextAlarmClock != null)
                Console.WriteLine("netxt allarm clock is:   " + alarmManager.NextAlarmClock);

            if (alarmManager.NextAlarmClock == null)
            {
                TimeSpan UntillMidNight = DateTime.MaxValue.TimeOfDay - DateTime.Now.TimeOfDay;
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
                           (this, 1, intent, PendingIntentFlags.OneShot);
                }

                alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + UntillMidNight_Mili, pendingIntent);
            }
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

        //goes to search activity
        private void Btnstart_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(SearchActivity));
            intent.PutExtra("OfflineMode", offlineMode);
            StartActivity(intent);

        }

    }
}