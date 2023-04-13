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

using System.Threading.Tasks;
using System.Threading;

using Firebase.Firestore;
using Firebase;
using Java.Util;
using System.Net.Http;
using Org.Json;
using Java.Lang;
using static Google.Firestore.V1.StructuredAggregationQuery.Aggregation;

namespace App1
{
    [Service]
    internal class Test_service : Service, Android.Gms.Tasks.IOnSuccessListener
    {
        int counter;
        bool running = false;

        int TimeBetweenChecks = 1; // seconds
        int numberOfCallsIcanMake = 250; // 250 is the number of calls i can make with one account in one day
        MyHandler myhandler; //interacting with the gui/main thread

        public FirebaseFirestore db; //database
        PendingIntent pendingIntent; //peding intent for the alarm manager
        public static List<StockData> Datalist = new List<StockData>();
        List<Integer> TrackPriceSurDes = new List<Integer>();

        int NotificationCount = 0;
        string NOTIFICATION_CHANNEL_ID = "StockPriceAlarm";
        int NOTIFICATION_ID = 1;
        bool CallInProcess = false;
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [Obsolete]
        public override void OnCreate()
        {
            base.OnCreate();
            myhandler = new MyHandler(this);
            //TimeBetweenChecks = 60 * 60 * 24; //seconds in the day
            //TimeBetweenChecks = TimeBetweenChecks / numberOfCallsIcanMake; // (seconds in the day / number of calls i can make in a day) = time gap between each call

            TimeBetweenChecks = 30;
            db = GetDataBase();

        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {

            counter = intent.GetIntExtra("counter", 10);
            Toast.MakeText(this, "Service started", ToastLength.Short).Show();
            System.Threading.Thread t = new System.Threading.Thread(Run);
            t.Start();

            return base.OnStartCommand(intent, flags, startId);

        }
        private void Run()
        {
            running = true;
            while (running)
            {
                if (!CallInProcess)
                {
                    if (db != null && db.App != null)
                    {
                        db.App.Dispose();
                    }

                    LoadItems();
                }
                Console.WriteLine("time between checks is: " + TimeBetweenChecks + " seconds");
                System.Threading.Thread.Sleep(TimeBetweenChecks * 1000);
            }
            StopSelf();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            running = false;//stop condition
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CancelAll();

            if( db!= null )
            {
                if(db.App != null)
                    db.App.Dispose();
                db.Dispose();
                db = null;
            }
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


            //var app = FirebaseApp.InitializeApp(this, options);
            //db = FirebaseFirestore.GetInstance(app);
            //return db;
        }
        private void LoadItems()
        {
            CallInProcess= true;
            if (Datalist.Count > 0)
            {
                Datalist.Clear();
            }
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

        public async void OnSuccess(Java.Lang.Object result)
        {
            //IsDataCountFull = false;
            Console.WriteLine("OnSuccess");
            var snapshot = (QuerySnapshot)result;
            StockData data;
            List<string> Symbols = new List<string>();
            int i = 0;
            // iterate through each document in the collection
            foreach (var doc in snapshot.Documents)
            {

                string tr = (string)doc.Get("TrackingPrices");
                if (tr[tr.Length - 1] == ',')
                {
                    tr.Remove(tr.Length - 1);
                }
                List<float> trackingprices = new List<float>();
                if (tr != null && tr != "")
                {
                    string[] trs = tr.Split(',');
                    Console.WriteLine("The tracking prices of: " + doc.Get("Symbol") + " are:");
                    foreach (string price in trs)
                    {
                        if (price != null && price != "")
                        {
                            trackingprices.Add(float.Parse(price));
                            Console.Write(" " + price);
                        }
                    }
                    Console.WriteLine("  ");
                }
                if (trackingprices.Count > 0)
                {
                    data = new StockData((float)doc.Get("Price"), (float)doc.Get("Open"), (string)doc.Get("symbol"), (string)doc.Get("SoundFile"), trackingprices);
                    Datalist.Add(data);
                    Symbols.Add(data.symbol);

                }
                i++;
            }

            await GetCurrentPriceFromWeb(Symbols);

        }

        private async Task GetCurrentPriceFromWeb(List<string> symbols)
        {
            using (var httpClient2 = new HttpClient())
            {
                string symbolss = "";
                foreach (string symbol in symbols)
                {
                    symbolss = symbolss + ',' + symbol;
                }


                string link = "https://financialmodelingprep.com/api/v3/quote/";
                link = link.Insert(link.Length, symbolss);
                //link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");
                link = link.Insert(link.Length, "?apikey=8bdedb14d7674def460cb3a84f1fd429");
                //8bdedb14d7674def460cb3a84f1fd429

                Console.WriteLine("creating a get request to financialmodeling ");
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    Console.WriteLine("sending request to financialmodeling ");
                    var response2 = await httpClient2.SendAsync(request);
                    response2.EnsureSuccessStatusCode();

                    string responseBody = await response2.Content.ReadAsStringAsync();
                    JSONArray HistInfo = new JSONArray(responseBody);

                    float currentPrice = 0;
                    float lastPrice = 0;
                    float trackprice_Alarm = -1;

                    SendDemoNotification();


                }
            }
            CallInProcess = false;
            Toast.MakeText(this, "finished checking tracking prices", ToastLength.Short).Show();
            Dispose(true);
            return;
        }

        private void SendDemoNotification()
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

        private float CheckIfSurpesst(float lastPrice, float currentPrice, List<float> trackingPrices)
        {
            float priceSur = -1;
            if (lastPrice < currentPrice) //if the price is rising than check if srpest any tracking prices that are hier than the last loaded value of the stock
            {
                foreach (float price in trackingPrices)
                {
                    if (currentPrice > price && price > priceSur)
                    {
                        priceSur = price;
                    }
                }
            }
            else
            {
                foreach (float price in trackingPrices)
                {
                    if (currentPrice < price && price < priceSur)
                    {
                        priceSur = price;
                    }
                }
            }

            return priceSur;
        }
    }
}