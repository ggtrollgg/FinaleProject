using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase;
using Firebase.Firestore;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace App1
{
    [Activity(Label = "Test_ServiceAndNotifivation_Activity")]
    public class Test_ServiceAndNotifivation_Activity : Activity, Android.Gms.Tasks.IOnSuccessListener
    {
        Button btnSend, btnStart, btnStop,btnDelete,btnTerminate, btnDataBaseStart;
        Intent intent2;
        public FirebaseFirestore db;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Test_serviceANDnotification_layout);
            // Create your application here
            btnSend = FindViewById<Button>(Resource.Id.btnSend);
            btnStart = FindViewById<Button>(Resource.Id.btnStart);
            btnStop = FindViewById<Button>(Resource.Id.btnStop);
            btnDelete = FindViewById<Button>(Resource.Id.btnDelete);
            btnDataBaseStart = FindViewById<Button>(Resource.Id.btnDataBaseStart);
            btnTerminate = FindViewById<Button>(Resource.Id.btnTerminate);
            
            btnSend.Click += BtnSend_Click;
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;

            btnDelete.Click += BtnDelete_Click;
            btnTerminate.Click += BtnTerminate_Click;
            btnDataBaseStart.Click += BtnDataBaseStart_Click;
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (db != null)
            {
                if (db.App != null)
                {
                    db.App.Dispose();
                    //db.Terminate();

                    Console.WriteLine("app Dispose");
                }
            }
        }

        private void BtnDataBaseStart_Click(object sender, EventArgs e)
        {
            db = GetDataBase();
            Console.WriteLine("started database db");
        }

        private void BtnTerminate_Click(object sender, EventArgs e)
        {
            Console.WriteLine("desposing of db");
            if (db != null)
            {
                Console.WriteLine("db isnt null");
                if (db.App != null)
                {
                    Console.WriteLine("db.app isnt null");
                    db.App.Dispose();
                    db = null;

                    Console.WriteLine("db Dispose");
                }
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if (intent2 != null)
            {
                StopService(intent2);
            }
            //Copy of the code in service 

        }



        public FirebaseFirestore GetDataBase()
        {
            FirebaseFirestore db;
            // info from "google-services.json"
            var options = new FirebaseOptions.Builder()
            .SetProjectId("stock-data-base-finalproject")
            .SetApplicationId("stock-data-base-finalproject")
            .SetApiKey("AIzaSyCjiFrMsBwOFvqUZRdohfIiqMsJC5QG_kc")
            .SetStorageBucket("stock-data-base-finalproject.appspot.com")
            .Build();


            try
            {
                var app = FirebaseApp.InitializeApp(this, options);
                db = FirebaseFirestore.GetInstance(app);
                return db;
            }
            catch
            {
                var app = FirebaseApp.GetApps(this);
                db = FirebaseFirestore.GetInstance(app[0]);
                return db;
            }


        }



        private void LoadItems()
        {
            // generate a query (request) from the database
            Query q = db.Collection("Saved Stocks");
            q.Get().AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {

            Console.WriteLine("OnSuccess");

            // gets List of HashMaps whick represent the DB students
            var snapshot = (QuerySnapshot)result;
            StockData data;
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