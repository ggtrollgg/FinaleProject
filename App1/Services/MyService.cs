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

        PendingIntent pendingIntent; //peding intent for the alarm manager
        public static List<StockData> Datalist = new List<StockData>();

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
            running = false;//stop condition
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
                if (tr[tr.Length-1] == ',')
                {
                    tr.Remove(tr.Length-1);
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
                    data = new StockData((float)doc.Get("heigh"), (float)doc.Get("low"), (string)doc.Get("symbol"), (string)doc.Get("LastDate"), (string)doc.Get("SoundFile"), trackingprices);
                    Datalist.Add(data);
                    Symbols.Add(data.symbol);
                   
                }
                i++;
            }

            await GetCurrentPriceFromWeb(Symbols);

            //ThreadStart MyThreadStart = new ThreadStart(Checkifstillloading);
            //System.Threading.Thread t = new System.Threading.Thread(MyThreadStart);

            // t.Start();
            //Toast.MakeText(this, "sent all data requests", ToastLength.Short).Show();
            //IsDataCountFull = true;

        }

        private async Task GetCurrentPriceFromWeb(List<string> symbols)
        {
            using (var httpClient2 = new HttpClient())
            {
                string symbolss = "";
                foreach (string symbol in symbols) 
                { 
                    symbolss = symbolss + ',' +  symbol;
                }
               

                string link = "https://financialmodelingprep.com/api/v3/quote/";
                link = link.Insert(link.Length, symbolss);
                //link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");
                link = link.Insert(link.Length, "?apikey=8bdedb14d7674def460cb3a84f1fd429");
                //8bdedb14d7674def460cb3a84f1fd429

                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {

                    var response2 = await httpClient2.SendAsync(request);
                    response2.EnsureSuccessStatusCode();

                    string responseBody = await response2.Content.ReadAsStringAsync();
                    JSONArray HistInfo = new JSONArray(responseBody);

                    //Console.WriteLine(HistInfo.Length());

                    //Datalist[place].low = ((float)(HistInfo.GetJSONObject(0).GetDouble("low")));
                    //Datalist[place].heigh = ((float)(HistInfo.GetJSONObject(0).GetDouble("high")));
                    //Datalist[place].date = ((string)(HistInfo.GetJSONObject(0).Get("date")));


                }
            }

            //Toast.MakeText(this, "data list count is: " + Datalist.Count, ToastLength.Short).Show();
            Dispose(true);
            return;
        }
    }
}