using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Org.Apache.Http.Cookies;
using Org.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Text;


using System.Diagnostics;

using Firebase.Firestore;
using Firebase;

//using Android.Gms.Tasks;

//using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using System.Threading;
using Java.Util.Functions;

namespace App1
{
    [Activity(Label = "StockViewActivity")]
    public class StockViewActivity : Activity, Android.Gms.Tasks.IOnSuccessListener
    {
        Button btnReturnHome, btnShowSaved, btnShowTrack;

        public static List<String> list = new List<String>();
        public static List<StockData> Datalist = new List<StockData>();

        ListView lvStock;
        StockAdapter adapter;

        public FirebaseFirestore db;
        CancellationTokenSource CTS = new CancellationTokenSource();

        bool ShowOnlyTracking= false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ListViewPage_Layout);
            btnReturnHome = FindViewById<Button>(Resource.Id.btnReturnHome);
            btnShowSaved = FindViewById<Button>(Resource.Id.btnShowSaved);
            btnShowTrack = FindViewById<Button>(Resource.Id.btnShowTrack);

            btnShowTrack.Click += BtnShowTrack_Click;
            btnShowSaved.Click += BtnShowSaved_Click;

            btnReturnHome.Click += BtnReturnHome_Click;
            list = new List<String>();
            Datalist = new List<StockData>();

            //DeleteFile("SavedStocks.txt");
            //Add_To_File("AAPL");
            //Add_To_File("TSLA");
            

            Console.WriteLine(Read_from_file());

            db = GetDataBase();
            //AddItem();
            LoadItems();

           //_ = processAllSavedStocks();
        }

        private void BtnShowSaved_Click(object sender, EventArgs e)
        {
            if(ShowOnlyTracking)
            {
                ShowOnlyTracking = false;
                LoadItems();
            }


        }

        private void BtnShowTrack_Click(object sender, EventArgs e)
        {
            if(!ShowOnlyTracking)
            {
                ShowOnlyTracking = true;
                LoadItems();
            }
            
        }

        private void AddItem()
        {
            // Create a HashMap to store your data like an object
            // HashMap is a collection of "keys" and "values"
            HashMap map = new HashMap();
            // save data
            // map.Put([field name], content);
            map.Put("symbol", "TSLA");
            map.Put("LastDate", "");
            map.Put("SoundFile", "");
            map.Put("TrackingPrices", "");
            map.Put("heigh", 0);
            map.Put("low", 0);

            // create an empty document reference for firestore
            DocumentReference docRef = db.Collection("Saved Stocks").Document();
            // puts the map info in the document
            docRef.Set(map);
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
            if(Datalist.Count > 0)
            {
                Datalist.Clear();
            }
            // generate a query (request) from the database
            Query q = db.Collection("Saved Stocks");
            q.Get().AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {

            // gets List of HashMaps whick represent the DB students
            
            var snapshot = (QuerySnapshot)result;
            StockData data;
            int i = 0;
            // iterate through each document in the collection
            foreach (var doc in snapshot.Documents)
            {
                
                String tr = (String)doc.Get("TrackingPrice");
                List<float> trackingprices = new List<float>();
                if (tr != null)
                {
                    String[] trs = tr.Split(',');
                    
                    foreach (String price in trs)
                    {
                        if(price != null && price != "")
                        {
                            trackingprices.Add(float.Parse(price));
                        }
                    }
                }
                if(trackingprices.Count > 0)
                {
                    data = new StockData((float)doc.Get("heigh"), (float)doc.Get("low"), (string)doc.Get("LastDate"), (string)doc.Get("symbol"), (string)doc.Get("SoundFile"), trackingprices);
                    Datalist.Add(data);
                     _ = GetInfoFromWeb(data.symbol, i);
                }
                else
                {
                    //float heigh = (float)doc.Get("heigh");
                    //float low = (float)doc.Get("low");
                    //String symbol = (String)doc.Get("symbol");
                    //String LastDate = (String)doc.Get("LastDate");
                    if(!ShowOnlyTracking)
                    {
                        data = new StockData((float)doc.Get("heigh"), (float)doc.Get("low"), (String)doc.Get("LastDate"), (String)doc.Get("symbol"));
                        Datalist.Add(data);
                        _ = GetInfoFromWeb(data.symbol, i);
                    } 
                }

                i++;
            }
            if(Datalist.Count == 0)
            {
                ShowListView();
            }
        }

        

        private async Task GetInfoFromWeb(string symbol,int place)
        {
            using (var httpClient2 = new HttpClient())
            {
                

                symbol = symbol.Replace("\0","");
                symbol = symbol.Replace("\n", "");
                symbol = symbol.Replace(",", "");

                string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
                link = link.Insert(link.Length, symbol);
                //link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");
                link = link.Insert(link.Length, "?apikey=8bdedb14d7674def460cb3a84f1fd429");
                //8bdedb14d7674def460cb3a84f1fd429

                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    Toast.MakeText(this, "sending requast for info", ToastLength.Short).Show();
                    //CancellationToken token = CTS.Token;


                    //ThreadStart MyThreadStart = new ThreadStart(Test_stopCall);
                    //Thread t = new Thread(MyThreadStart);

                    //var response2 = await httpClient2.SendAsync(request,CTS.Token);
                    var response2 = await httpClient2.SendAsync(request);
                    response2.EnsureSuccessStatusCode();

                    string responseBody = await response2.Content.ReadAsStringAsync();
                    JSONArray HistInfo = new JSONArray(responseBody);

                    Console.WriteLine(HistInfo.Length());

                    Datalist[place].low=((float)(HistInfo.GetJSONObject(0).GetDouble("low")));
                    Datalist[place].heigh =((float)(HistInfo.GetJSONObject(0).GetDouble("high")));
                    Datalist[place].date = ((string)(HistInfo.GetJSONObject(0).Get("date")));
                    
                }
            }

            Toast.MakeText(this, "got the info from web?", ToastLength.Short).Show();

            ShowListView();

            Dispose(true);
            return;
        }

        //public void Test_stopCall()
        //{
        //    Thread.Sleep(5000);

        //    CTS.Cancel();
        //}
        private void ShowListView()
        {
            adapter = new StockAdapter(this, Datalist);
            lvStock = (ListView)FindViewById(Resource.Id.lvStock);
            lvStock.Adapter = adapter;
        }

        private void BtnReturnHome_Click(object sender, EventArgs e)
        {
            db.App.Delete();
            db.Terminate();
            Finish();
        }














        private async Task processAllSavedStocks()
        {
            String s = Read_from_file();



            s = s.Replace("\0", "");
            s = s.Replace("\n", "");

            String[] s2 = s.Split(',');
            Console.WriteLine(s2);
            Console.WriteLine("------------------------------------------------------------------------------------------");
            for (int i = 0; i < s2.Length; i++)
            {
                if (s2[i] != null && s2[i].Length != 0)
                {
                    list.Add(s2[i]);
                    Console.WriteLine(s2[i]);
                    Console.WriteLine("------------------------------------------------------------------------------------------");
                    StockData d = new StockData();
                    d.symbol = s2[i];
                    Datalist.Add(d);
                    _ = GetInfoFromWeb(s2[i].ToString(), i);
                }
            }

        }
        private String Read_from_file()
        {
            try
            {
                string str;
                //using (Stream stream = OpenFileOutput("Emailinfo.txt", Android.Content.FileCreationMode.Private))
                using (Stream stream = OpenFileInput("SavedStocks.txt"))
                {
                    try
                    {
                        byte[] buffer = new byte[4096];
                        stream.Read(buffer, 0, buffer.Length);
                        str = System.Text.Encoding.UTF8.GetString(buffer);
                        stream.Close();
                        if (str != null)
                        {
                            //  tv.Text = str;
                            //Toast.MakeText(this, str, ToastLength.Short).Show();
                            return str;
                        }
                    }
                    catch (Java.IO.IOException a)
                    {
                        a.PrintStackTrace();
                    }
                }
            }
            catch (Java.IO.FileNotFoundException a)
            {
                a.PrintStackTrace();

            }

            return null;
        }

        private void Add_To_File(String the_stock)
        {
            try
            {
                string str = the_stock + ",";//= et.Text;
                using (Stream stream = OpenFileOutput("SavedStocks.txt", Android.Content.FileCreationMode.Append))
                {
                    try
                    {
                        if (!Is_Allready_In_File(the_stock))
                        {
                            stream.Write(Encoding.ASCII.GetBytes(str), 0, str.Length);
                            stream.Close();
                            Toast.MakeText(this, "saved", ToastLength.Short).Show();
                        }
                    }
                    catch (Java.IO.IOException a)
                    {
                        a.PrintStackTrace();
                    }
                }
            }
            catch (Java.IO.FileNotFoundException a)
            {
                a.PrintStackTrace();
            }
            
        }

        private bool Is_Allready_In_File(string the_stock)
        {
            string str = Read_from_file();
            if (str== null || str.Contains(the_stock))
            {
                Toast.MakeText(this, "all ready in", ToastLength.Short).Show();
                return true;
            }
            return false;
        }

        


    }
}