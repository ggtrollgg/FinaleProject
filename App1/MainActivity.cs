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

        public static ISharedPreferences sp;
        public static Manager_API_Keys Manager_API_Keys;
        Button btnstart,btnToListView,btnExit,btnTest;
        Android.Content.Intent intent2;
        //
        PendingIntent pendingIntent;



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
            Manager_API_Keys = new Manager_API_Keys();


            //API_Keys.Add(new API_Key("0a0b32a8d57dc7a4d38458de98803860"));

            //0a0b32a8d57dc7a4d38458de98803860  //ggtroll 35
            //8bdedb14d7674def460cb3a84f1fd429 //ggtroll 36
            //561897c32bf107b87c107244081b759f //ggtroll 37

            AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);
            Console.WriteLine(" ");
            Console.WriteLine("  ");
            Console.WriteLine("   ");
            Console.WriteLine("     ");
            Console.WriteLine("      ");
            Console.WriteLine("       ");
            Console.WriteLine("        ");


            Console.WriteLine("alarmManageris null?  " + (alarmManager == null));
            if (alarmManager.NextAlarmClock != null)
                Console.WriteLine("netxt allarm clock is:   " + alarmManager.NextAlarmClock);

            if (alarmManager.NextAlarmClock == null )
            {
                TimeSpan UntillMidNight = DateTime.MaxValue.TimeOfDay - DateTime.Now.TimeOfDay;
                //TimeSpan UntillMidNight = DateTime.Now.TimeOfDay - DateTime.Now.TimeOfDay;
                int UntillMidNight_Mili = (int)(UntillMidNight.TotalMilliseconds);

                Console.WriteLine("UntillMidNight_Mili: " + UntillMidNight_Mili + " UntillMidNight: " + UntillMidNight);
                Console.WriteLine("DateTime.MaxValue.TimeOfDay: " + DateTime.MaxValue.TimeOfDay + " DateTime.Now.TimeOfDay: " + DateTime.Now.TimeOfDay);
                Console.WriteLine("DateTime.MaxValue.TimeOfDay - DateTime.Now.TimeOfDay: " + (DateTime.MaxValue.TimeOfDay - DateTime.Now.TimeOfDay) + " UntillMidNight: " + UntillMidNight);

                //UntillMidNight_Mili = 5000;

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

                //pendingIntent = PendingIntent.GetBroadcast(this, 1, intent, 0);

                //alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + UntillMidNight_Mili, pendingIntent);
                alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 60000, pendingIntent);
            }

            Console.WriteLine(" ");
            Console.WriteLine("  ");
            Console.WriteLine("   ");
            Console.WriteLine("     ");
            Console.WriteLine("      ");
            Console.WriteLine("       ");
            Console.WriteLine("        ");


            Console.WriteLine();
            if (intent2 != null)
            {
                
                StopService(intent2);
            }
            intent2 = new Intent(this, typeof(TrackingService));
            StartService(intent2);


           
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(Algorithm_Test_Activity));
            //Intent intent = new Intent(this, typeof(PopUp_Handler_Activity_Test));
            intent.PutExtra("symbol", "OB");
            StartActivity(intent);

            //var editor = sp.Edit();
            //editor.PutInt("Key0CallsRemain", 222);
            //editor.PutInt("Key1CallsRemain", 222);
            //editor.PutInt("Key2CallsRemain", 222);
            //editor.Commit();

            //for (int i = 0; i < sp.GetInt("KeysAmount", -1); i++)
            //{
            //    Console.WriteLine("   ");

            //    Console.WriteLine("ShardPrefrenc: Key" + i + " is: " +  sp.GetString("Key" + i, ""));
            //    Console.WriteLine("ShardPrefrenc: CallsRemain: " + sp.GetInt("Key" + i + "CallsRemain", 0));
            //    Console.WriteLine("Manager_API_Key: Key" + i + " is: " + Manager_API_Keys.API_Keys[i].Key);
            //    Console.WriteLine("ShardPrefrenc: CallsRemain: " + Manager_API_Keys.API_Keys[i].GetCallsRemaining());


            //    Console.WriteLine("   ");
            //}



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