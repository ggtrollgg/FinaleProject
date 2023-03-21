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
    [Activity(Label = "Test_ServiceAndNotifivation_Activity")]
    public class Test_ServiceAndNotifivation_Activity : Activity
    {
        Button btnSend, btnStart, btnStop;
        Intent intent2;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Test_serviceANDnotification_layout);
            // Create your application here
            btnSend = FindViewById<Button>(Resource.Id.btnSend);
            btnStart = FindViewById<Button>(Resource.Id.btnStart);
            btnStop = FindViewById<Button>(Resource.Id.btnStop);

            
            btnSend.Click += BtnSend_Click;
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if (intent2 != null)
            {
                StopService(intent2);
            }
            //Copy of the code in service 

        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (intent2 != null)
            {
                StopService(intent2);
            }
            intent2 = new Intent(this, typeof(Test_service));
            //intent2 = new Intent(this, typeof(MyService));
            StartService(intent2);
            //Copy of the code in service 

        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            float trackprice_Alarm = 666;
            int g = 0;
            int NOTIFICATION_ID = 0;
            List<string> symbols = new List<string>();
            symbols.Add("AAPL");
            string NOTIFICATION_CHANNEL_ID = "StockPriceAlarm";

            //Copy of the code in service 
            if (trackprice_Alarm != -1)
            {

                NOTIFICATION_ID = g;
                //Intent i = new Intent(this, typeof(ChartActivity));
                Intent i = new Intent(this, typeof(MainActivity));
                i.PutExtra("key", "new message");
                i.PutExtra("symbol", symbols[g]);
                PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, i, 0);

                Notification.Builder notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.Icon_Favorite_colored)
                .SetContentTitle("Price Alarm for stock: " + symbols[g])
                .SetContentText("text text");
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);


                foreach (var notification in notificationManager.GetActiveNotifications())
                {
                    if (notification.Id == NOTIFICATION_ID)
                    {
                        notificationManager.Cancel(NOTIFICATION_ID); //if there is already a notification with this id than cancel it 
                    }
                }



                notificationBuilder.SetContentIntent(pendingIntent);
                //Build.VERSION_CODES.O - is a reference to API level 26 (Android Oreo which is Android 8)if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    NotificationChannel notificationChannel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, "NOTIFICATION_CHANNEL_NAME", NotificationImportance.High);
                    notificationBuilder.SetChannelId(NOTIFICATION_CHANNEL_ID);
                    notificationManager.CreateNotificationChannel(notificationChannel);
                }
                notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
            }
        }
    }
}