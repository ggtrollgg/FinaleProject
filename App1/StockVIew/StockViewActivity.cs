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
using Java.Lang;
using Firestore.Admin.V1;

namespace App1
{
    [Activity(Label = "StockViewActivity")]
    public class StockViewActivity : Activity, Android.Gms.Tasks.IOnSuccessListener
    {
        Button btnReturnHome, btnShowSaved, btnShowTrack;

        public static List<string> list = new List<string>();
        public static List<StockData> Datalist = new List<StockData>();
        public static List<StockData> Temp_Datalist = new List<StockData>();
        List<DocumentSnapshot> Docs_In_DataBase = new List<DocumentSnapshot>();

        bool IsDataCountFull = false;
        ListView lvStock;
        StockAdapter adapter;
        string queryType = "normal";

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
            list = new List<string>();
            Datalist = new List<StockData>();
            lvStock = (ListView)FindViewById(Resource.Id.lvStock);
            //DeleteFile("SavedStocks.txt");
            //Add_To_File("AAPL");
            //Add_To_File("TSLA");


            //Console.WriteLine(Read_from_file());

            db = GetDataBase();
            //AddItem();
            LoadItems();

           //_ = processAllSavedStocks();
        }

        //buttons
        private void BtnShowSaved_Click(object sender, EventArgs e)
        {
            if(ShowOnlyTracking)
            {
                queryType = "normal";
                ShowOnlyTracking = false;
                lvStock.ItemClick -= LvStock_ItemClick;
                LoadItems();
            }
            

        }

        private void BtnShowTrack_Click(object sender, EventArgs e)
        {
            if(!ShowOnlyTracking)
            {
                queryType = "Tracking";
                ShowOnlyTracking = true;
                lvStock.ItemClick -= LvStock_ItemClick;
                LoadTrackingItems();
                //ShowListView();

                return;
            }
        }

        private void LoadTrackingItems()
        {
            if (Datalist.Count > 0)
            {
                Datalist.Clear();
            }
            if (list.Count > 0)
            {
                list.Clear();
            }
            if (Temp_Datalist.Count > 0)
            {
                Temp_Datalist.Clear();
            }
            // generate a query (request) from the database
            Query q = 
                db
                .Collection("Saved Stocks")
                .WhereNotEqualTo("TrackingPrices", "");
            q.Get().AddOnSuccessListener(this);
        }

        private void BtnReturnHome_Click(object sender, EventArgs e)
        {
            //ShowListView();
             db.App.Delete();
             db.Terminate();
             Finish();
        }



        //data base
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

        private void DeleteItem_fromDataBase(int index)
        {
            DocumentReference doc = db.Collection("Saved Stocks").Document(Docs_In_DataBase[index].Id);
            doc.Delete();
            Docs_In_DataBase.RemoveAt(index);
        }
        private void UpdateTrackItemAsync(string symbol, int index)
        {
            if (index >= Docs_In_DataBase.Count || index < 0) 
            {
                Console.WriteLine("tried to update Track item that was out of index. index was: " + index + " docs_in_datavase.count = " + Docs_In_DataBase.Count);
                return;
            }


            string TrackingPrices = (string)Docs_In_DataBase[index].Get("TrackingPrices");
            string soundfile = (string)Docs_In_DataBase[index].Get("SoundFile");
            string LastDate = "";
            float heigh = 0;
            float low = 0;

            DeleteItem_fromDataBase(index);

            Console.WriteLine("the symbol updated is: " + symbol);
            foreach (StockData data in Datalist)
            {
                if(data.symbol== symbol)
                {
                    heigh= data.heigh;
                    low = data.low;
                    LastDate = data.date;
                    break;
                }
            }

            


            HashMap map = new HashMap();
            map.Put("symbol", symbol);
            map.Put("LastDate", LastDate);
            map.Put("SoundFile", soundfile);
            map.Put("TrackingPrices", TrackingPrices);
            map.Put("heigh", heigh);
            map.Put("low", low);

            

            CollectionReference collection = db.Collection("Saved Stocks");
            collection.Add(map);
        }
        private void LoadItems()
        {
            if(Datalist.Count > 0)
            {
                Datalist.Clear();
            }
            if (list.Count > 0)
            {
                list.Clear();
            }
            if(Temp_Datalist.Count > 0)
            {
                Temp_Datalist.Clear();
            }
            // generate a query (request) from the database
            Query q = db.Collection("Saved Stocks");
            q.Get().AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            IsDataCountFull = false;
            Console.WriteLine("OnSuccess");
            if (queryType == "normal")
            {
                // gets List of HashMaps whick represent the DB students
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
                            if (price != null && price != "" && IsDigitsOnly(price))
                            {
                                trackingprices.Add(float.Parse(price));
                                Console.WriteLine("The tracking prices of: " + doc.Get("Symbol") + " are: " + price);
                            }
                        }
                    }
                    if (trackingprices.Count > 0)
                    {
                        data = new StockData((float)doc.Get("heigh"), (float)doc.Get("low"), (string)doc.Get("symbol"), (string)doc.Get("LastDate"), (string)doc.Get("SoundFile"), trackingprices);
                        Datalist.Add(data);
                        Docs_In_DataBase.Add(doc);
                        _ = GetInfoFromWeb(data.symbol, i);
                    }
                    else
                    {
                        //float heigh = (float)doc.Get("heigh");
                        //float low = (float)doc.Get("low");
                        //string symbol = (string)doc.Get("symbol");
                        //string LastDate = (string)doc.Get("LastDate");
                        //if (!ShowOnlyTracking)
                        //{
                        data = new StockData((float)doc.Get("heigh"), (float)doc.Get("low"), (string)doc.Get("LastDate"), (string)doc.Get("symbol"));
                        Datalist.Add(data);
                        Docs_In_DataBase.Add(doc);
                        _ = GetInfoFromWeb(data.symbol, i);
                        //}
                    }

                    i++;
                }
            }

            if (queryType == "Tracking")
            {
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
                            if (price != null && price != "" && IsDigitsOnly(price))
                            {
                                trackingprices.Add(float.Parse(price));
                                Console.WriteLine("The tracking prices of: " + doc.Get("Symbol") + " are: " + price);
                            }
                        }
                    }
                    if (trackingprices.Count > 0)
                    {
                        data = new StockData((float)doc.Get("heigh"), (float)doc.Get("low"), (string)doc.Get("symbol"), (string)doc.Get("LastDate"), (string)doc.Get("SoundFile"), trackingprices);
                        Datalist.Add(data);
                        Docs_In_DataBase.Add(doc);
                        _ = GetInfoFromWeb(data.symbol, i);
                    }
                    i++;
                }
            }
            ThreadStart MyThreadStart = new ThreadStart(Checkifstillloading);
            System.Threading.Thread t = new System.Threading.Thread(MyThreadStart);
            
            t.Start();
            Toast.MakeText(this, "sent all data requests", ToastLength.Short).Show();
            IsDataCountFull = true;
            
        }

        public bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c != ',' && (c < '0' || c > '9'))
                    return false;
            }

            return true;
        }

        //loading checker
        private void TimeOut()
        {
            System.Threading.Thread.Sleep(3000);
            Console.WriteLine("I am Function");
        }

        private void Checkifstillloading()
        {
            bool flag = true;
            int counter = 0;
            while(flag)
            {
                System.Threading.Thread.Sleep(10000);
                if (IsDataCountFull && Temp_Datalist.Count == Datalist.Count)
                {
                    //Toast.MakeText(this, "something went wrong", ToastLength.Short).Show();
                    //Console.WriteLine("something went wrong");
                    flag = false;

                }
                else
                {
                    if (counter == 12)
                    {
                        Console.WriteLine("the code should not be stuck in loading for 2 min L :(");
                        //Toast.MakeText(this, "loading... for 2 min", ToastLength.Short).Show();
                    }
                    else
                    {
                        Console.WriteLine("loading...");
                        //Toast.MakeText(this, "loading...", ToastLength.Short).Show();
                    }
                     

                }
                counter++;
            }
            return;
        }


        //get info on stock from internet
        private async Task GetInfoFromWeb(string symbol,int place)
        {
            using (var httpClient2 = new HttpClient())
            {
                symbol = symbol.Replace("\0","");
                symbol = symbol.Replace("\n", "");
                symbol = symbol.Replace(",", "");

                string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
                link = link.Insert(link.Length, symbol);
                link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");
                //link = link.Insert(link.Length, "?apikey=8bdedb14d7674def460cb3a84f1fd429");
                //8bdedb14d7674def460cb3a84f1fd429

                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {

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

           //Toast.MakeText(this, "data list count is: " + Datalist.Count, ToastLength.Short).Show();
            Console.WriteLine("data list count is: " + Datalist.Count);
            Console.WriteLine("Temp_Datalist count is: " + Temp_Datalist.Count);
            Console.WriteLine("place is: " + place);
            Console.WriteLine("adding to temp data list in place: " + place);
            Temp_Datalist.Add(Datalist[place]);

            
            if (IsDataCountFull && Temp_Datalist.Count == Datalist.Count)
            {
                int count = Docs_In_DataBase.Count;
                for (int g = count-1; g >= 0; g--)
                {
                    UpdateTrackItemAsync((string)Docs_In_DataBase[g].Get("symbol"), g);
                }
                
                Toast.MakeText(this, "presenting the list", ToastLength.Short).Show();
                ShowListView();
            }

            //if (place >= Datalist.Count - 1)
            //{
                //Toast.MakeText(this, "last data point", ToastLength.Short).Show();
                ////ShowListView();
            //}
                

            Dispose(true);
            return;
        }


        //show the listview of stocks
        private void ShowListView()
        {
            adapter = new StockAdapter(this, Temp_Datalist);
            
            lvStock.Adapter = adapter;

            lvStock.ItemClick += LvStock_ItemClick;
        }

        //if stock was clicked than move to the chart of the stock
        private void LvStock_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Console.WriteLine("clicked! moving to chartActivity");
            Console.WriteLine("symbol clicked: " + Datalist[e.Position].symbol);

            MoveToChartActivity(Datalist[e.Position].symbol);
        }

        public void MoveToChartActivity(string symbol)
        {
            Intent intent = new Intent(this, typeof(ChartActivity));
            intent.PutExtra("symbol", symbol);
            StartActivity(intent);
        }



        

        protected override void OnPause()
        {
            //db.App.Delete();
            //db.Terminate();
            //db.Dispose();
            //db= null;
            if (db != null)
            {
                if (db.App != null)
                {
                    db.App.Delete();
                    db.Terminate();
                    Console.WriteLine("db terminated");
                }
            }
            base.OnPause();
        }


        protected override void OnResume()
        {
            if(db == null)
            {
                db = GetDataBase();
                //AddItem();
                //LoadItems();
            }
            

            base.OnResume();
        }









        //not in use
        private async Task processAllSavedStocks()
        {
            string s = Read_from_file();



            s = s.Replace("\0", "");
            s = s.Replace("\n", "");

            string[] s2 = s.Split(',');
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
        private string Read_from_file()
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

        private void Add_To_File(string the_stock)
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