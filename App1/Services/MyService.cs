using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Firebase.Firestore;
using Firebase;
using Java.Util;


namespace App1
{
    [Service]
    public class MyService : Service, Android.Gms.Tasks.IOnSuccessListener
    {
        int counter;
        bool running = false;

        int TimeBetweenChecks = 1; // seconds
        int numberOfCallsIcanMake = 250; // 250 is the number of calls i can make with one account in one day
        MyHandler myhandler; //interacting with the gui/main thread
        public FirebaseFirestore db; //database


        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [Obsolete]
        public override void OnCreate()
        {
            base.OnCreate();
            myhandler = new MyHandler(this);
            TimeBetweenChecks= 60*60*24; //seconds in the day
            TimeBetweenChecks = TimeBetweenChecks/numberOfCallsIcanMake; // (seconds in the day / number of calls i can make in a day) = time gap between each call
            db = GetDataBase();

        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {

            counter = intent.GetIntExtra("counter", 10);
            Toast.MakeText(this, "Service started" + counter, ToastLength.Short).Show();
            Thread t = new Thread(Run);
            t.Start();

            return base.OnStartCommand(intent, flags, startId);

        }
        private void Run()
        {
            running = true;
            while (running)
            {
                Thread.Sleep(3000);
                Message mes = new Message();
                mes.Arg1 = counter;
                myhandler.SendMessage(mes);
                counter--;
            }









            StopSelf();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            counter = 0; //stop condition
            Toast.MakeText(this, "Service stopped ", ToastLength.Short).Show();

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


            var app = FirebaseApp.InitializeApp(this, options);
            db = FirebaseFirestore.GetInstance(app);
            return db;
        }
        private void LoadItems()
        {
            //if (Datalist.Count > 0)
            //{
            //    Datalist.Clear();
            //}
            //if (list.Count > 0)
            //{
            //    list.Clear();
            //}
            //if (Temp_Datalist.Count > 0)
            //{
            //    Temp_Datalist.Clear();
            //}


            // generate a query (request) from the database
            Query q =
               db
               .Collection("Saved Stocks")
               .WhereNotEqualTo("TrackingPrices", "");
            q.Get().AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            //IsDataCountFull = false;
            Console.WriteLine("OnSuccess");
            var snapshot = (QuerySnapshot)result;
            StockData data;
            int i = 0;
            // iterate through each document in the collection
            foreach (var doc in snapshot.Documents)
            {

                string tr = (string)doc.Get("TrackingPrices");
                List<float> trackingprices = new List<float>();
                if (tr != null && tr != "")
                {
                    string[] trs = tr.Split(',');

                    foreach (string price in trs)
                    {
                        if (price != null && price != "")
                        {
                            trackingprices.Add(float.Parse(price));
                            Console.WriteLine("The tracking prices of: " + doc.Get("Symbol") + " are: " + price);
                        }
                    }
                }
                if (trackingprices.Count > 0)
                {
                    data = new StockData((float)doc.Get("heigh"), (float)doc.Get("low"), (string)doc.Get("symbol"), (string)doc.Get("LastDate"), (string)doc.Get("SoundFile"), trackingprices);
                   // Datalist.Add(data);
                   // _ = GetInfoFromWeb(data.symbol, i);
                }
                i++;
            }
            
            //ThreadStart MyThreadStart = new ThreadStart(Checkifstillloading);
            //System.Threading.Thread t = new System.Threading.Thread(MyThreadStart);

           // t.Start();
            //Toast.MakeText(this, "sent all data requests", ToastLength.Short).Show();
            //IsDataCountFull = true;

        }


    }
}