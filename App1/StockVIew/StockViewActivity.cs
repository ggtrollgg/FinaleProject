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
using static AndroidX.RecyclerView.Widget.RecyclerView;

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
        System.Threading.Thread t;
        bool IsDataCountFull = false;
        ListView lvStock;
        StockAdapter adapter;
        string queryType = "normal";
        public FirebaseFirestore db;
        bool ShowOnlyTracking= false;
        bool offlineMode= false;

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

            if (Intent.GetBooleanExtra("OfflineMode", false))
            {
                List<float> tp = new List<float>
                {
                    20,
                    (float)21.3
                }; //tracking prices

                offlineMode = true;
                Datalist.Add(new StockData(100, (float)95.3, "Demo_stock"));
                Datalist.Add(new StockData((float)27.9, (float)29.2, "Demo2_stock", "", tp));
                ShowListView();
            }
            else
            {
                db = GetDataBase(); ;
                LoadItems();
            }
            

           //_ = processAllSavedStocks();
        }

        //buttons
        private void BtnShowSaved_Click(object sender, EventArgs e)
        {
            if (offlineMode)
            {
                if (ShowOnlyTracking)
                {
                    ShowOnlyTracking = false;
                    Datalist.Clear();

                    List<float> tp = new List<float>
                    {
                        20,
                        (float)21.3
                    }; //tracking prices
                    Datalist.Add(new StockData(100, (float)95.3, "Demo_stock"));
                    Datalist.Add(new StockData((float)27.9, (float)29.2, "Demo2_stock", "", tp));

                    ShowListView();
                }


                return;
            }
            else if(ShowOnlyTracking)
            {
                queryType = "normal";
                ShowOnlyTracking = false;
                lvStock.ItemClick -= LvStock_ItemClick;
                LoadItems();
            }
            

        }
        //Refresh the items in firestore, clear the view and get from firestore only the stocks i have tracking prices on
        private void BtnShowTrack_Click(object sender, EventArgs e)
        {
            if (offlineMode)
            {
                if (!ShowOnlyTracking)
                {
                    ShowOnlyTracking = true;
                    Datalist.Clear();

                    List<float> tp = new List<float>
                    {
                        20,
                        (float)21.3
                    }; //tracking prices
                    Datalist.Add(new StockData((float)27.9, (float)29.2, "Demo2_stock", "", tp));
                    ShowListView();
                }

                return;
            }
            if (!ShowOnlyTracking && !offlineMode)
            {
                queryType = "Tracking";
                ShowOnlyTracking = true;
                lvStock.ItemClick -= LvStock_ItemClick;
                LoadTrackingItems();
                //ShowListView();

                return;
            }
        }
        private void BtnReturnHome_Click(object sender, EventArgs e)
        {
            //ShowListView();
            if( !offlineMode)
                db.App.Dispose();
            if(t!= null && t.ThreadState== System.Threading.ThreadState.Running)
            {
                t.Abort();
            }
             //db.Terminate();
             
             Finish();
        }


        //create conntectin to the firestore data base
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

        //request get all the items from the friestore
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

        //request et from fire store only the items with traking prices
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

        //on getting the info from fire store, extract the info on the stocks from it
        public async void OnSuccess(Java.Lang.Object result)
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
                        data = new StockData((float)doc.Get("Price"), (float)doc.Get("Open"), (string)doc.Get("symbol"), (string)doc.Get("SoundFile"), trackingprices);
                        Datalist.Add(data);
                        Docs_In_DataBase.Add(doc);
                    }
                    else
                    {
                        data = new StockData((float)doc.Get("Price"), (float)doc.Get("Open"), (string)doc.Get("symbol"), (string)doc.Get("SoundFile"));
                        Datalist.Add(data);
                        Docs_In_DataBase.Add(doc);
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
                    if (tr[tr.Length - 1] == ',')
                    {
                        tr.Remove(tr.Length - 1);
                    }
                    if (tr != null && tr != "")
                    {
                        string[] trs = tr.Split(',');
                        Console.WriteLine("The tracking prices of: " + doc.Get("Symbol") + " are: ");
                        foreach (string price in trs)
                        {
                            if (price != null && price != "" && IsDigitsOnly(price))
                            {
                                trackingprices.Add(float.Parse(price));
                                Console.Write(" "+ price);
                            }
                        }
                    }
                    if (trackingprices.Count > 0)
                    {
                        data = new StockData((float)doc.Get("Price"), (float)doc.Get("Open"), (string)doc.Get("symbol"), (string)doc.Get("SoundFile"), trackingprices);
                        Datalist.Add(data);
                        Docs_In_DataBase.Add(doc);
                    }
                    i++;
                }
            }
            List<string> symbols= new List<string>();
            foreach(var stock in Datalist)
            {
                symbols.Add(stock.symbol);
            }
            await Bulk_GetInfoFromWeb(symbols);
            Toast.MakeText(this, "got all data from requests", ToastLength.Short).Show();
            IsDataCountFull = true;
            
        }

        //is a string composed of only strings
        public bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c != ',' && c!= '.' && (c < '0' || c > '9'))
                    return false;
            }

            return true;
        }


        //get info on stock from internet
        
        private async Task Bulk_GetInfoFromWeb(List<string> symbols)
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
                API_Key k = MainActivity.Manager_API_Keys.GetBestKey();
                if (k != null && k.Key != "" && k.GetCallsRemaining() > 0)
                {
                    link = link.Insert(link.Length, "?apikey=" + k.Key);
                }
                else
                {
                    Console.WriteLine("there was a problem with the keys at stockview activity ");
                    return;
                }
                Console.WriteLine("creating a get request to financialmodeling ");
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    Console.WriteLine("sending request to financialmodeling ");
                    MainActivity.Manager_API_Keys.UseKey(k.Key);
                    var response2 = await httpClient2.SendAsync(request);
                    response2.EnsureSuccessStatusCode();

                    string responseBody = await response2.Content.ReadAsStringAsync();
                    JSONArray HistInfo = new JSONArray(responseBody);

                    float currentPrice = 0;
                    //float lastPrice = (Datalist[0].heigh + Datalist[0].low) / 2;
                    float open = 0;

                    for (int i = 0; i < symbols.Count; i++)
                    {
                        currentPrice = (float)HistInfo.GetJSONObject(i).GetDouble("price");
                        open = (float)HistInfo.GetJSONObject(i).GetDouble("open");
                        Datalist[i].price= currentPrice;
                        Datalist[i].open= open;
                    }
                }

                UpdateAllItems_V2();//update all the items in the firestore so the "open" and "price" are the most recent values

            }

            Toast.MakeText(this, "presenting the list", ToastLength.Short).Show();
            ShowListView();
            Dispose(true);
            return;
        }

        //apdate all the stocks in the firestore
        private void UpdateAllItems_V2()
        {

            CollectionReference collection = db.Collection("Saved Stocks");
            string tp = "";
            HashMap map = new HashMap();

            //for every stock in the database add a hash map with the correct attrabuts 
            foreach (var stock in Datalist)
            {
                tp = "";

                map = new HashMap();
                map.Put("symbol", stock.symbol);
                map.Put("SoundFile", stock.SoundName);
                if (stock.TrackingPrices != null)
                {
                    foreach (var num in stock.TrackingPrices)
                    {
                        tp = tp + num.ToString() + ",";
                    }
                    if (tp.Length > 0)
                        tp = tp.Remove(tp.Length - 1);
                }
                map.Put("TrackingPrices", tp);
                map.Put("Open", stock.open);
                map.Put("Price", stock.price);



                
                collection.Add(map);
            }


            //dlete from cloud
            int count = Docs_In_DataBase.Count;
            for (int i = count-1; i > -1 ; i--)
            {
                DocumentReference doc = db.Collection("Saved Stocks").Document(Docs_In_DataBase[i].Id);
                doc.Delete();
                Docs_In_DataBase.RemoveAt(i);
            }
            
        }


        //show the listview of stocks
        private void ShowListView()
        {
            //adapter = new StockAdapter(this, Temp_Datalist);

            adapter = new StockAdapter(this, Datalist);
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
            intent.PutExtra("OfflineMode", offlineMode);
            StartActivity(intent);
        }



        

        protected override void OnPause()
        {
            if (t!=null && t.ThreadState == System.Threading.ThreadState.Running)
            {
                t.Abort();
            }
            if (db != null)
            {
                if (db.App != null)
                {
                    db.App.Dispose();
                    Console.WriteLine("db terminated");
                }
            }
            base.OnPause();
        }


        protected override void OnResume()
        {
            offlineMode = Intent.GetBooleanExtra("OfflineMode", false);

            if(db == null && offlineMode)
            {
                db = GetDataBase();
                //AddItem();
                //LoadItems();
            }
            base.OnResume();
        }









        //not in use
        //private void UpdateTrackItemAsync(string symbol, int index)
        //{
        //    if (index >= Docs_In_DataBase.Count || index < 0)
        //    {
        //        Console.WriteLine("tried to update Track item that was out of index. index was: " + index + " docs_in_datavase.count = " + Docs_In_DataBase.Count);
        //        return;
        //    }


        //    string TrackingPrices = (string)Docs_In_DataBase[index].Get("TrackingPrices");
        //    string soundfile = (string)Docs_In_DataBase[index].Get("SoundFile");
        //    //string LastDate = "";
        //    float heigh = 0;
        //    float low = 0;

        //    DeleteItem_fromDataBase(index);

        //    Console.WriteLine("the symbol updated is: " + symbol);

        //    //putting the info that i got from the internet
        //    foreach (StockData data in Datalist)
        //    {
        //        if (data.symbol == symbol)
        //        {
        //            heigh = data.price;
        //            low = data.open;
        //            //LastDate = data.date;
        //            break;
        //        }
        //    }




        //    HashMap map = new HashMap();
        //    map.Put("symbol", symbol);
        //    //map.Put("LastDate", LastDate);
        //    map.Put("SoundFile", soundfile);
        //    map.Put("TrackingPrices", TrackingPrices);
        //    map.Put("Open", heigh);
        //    map.Put("Price", low);



        //    CollectionReference collection = db.Collection("Saved Stocks");
        //    collection.Add(map);

        //}
        //private async Task GetInfoFromWeb(string symbol, int place)
        //{
        //    using (var httpClient2 = new HttpClient())
        //    {
        //        symbol = symbol.Replace("\0", "");
        //        symbol = symbol.Replace("\n", "");
        //        symbol = symbol.Replace(",", "");

        //        string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
        //        link = link.Insert(link.Length, symbol);
        //        //link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860"); //ggtroll 35
        //        //link = link.Insert(link.Length, "?apikey=8bdedb14d7674def460cb3a84f1fd429"); //ggtroll 36
        //        //link = link.Insert(link.Length, "?apikey=561897c32bf107b87c107244081b759f"); //ggtroll 37

        //        API_Key k = MainActivity.Manager_API_Keys.GetBestKey();
        //        if (k != null && k.Key != "" && k.GetCallsRemaining() > 0)
        //        {
        //            link = link.Insert(link.Length, "?apikey=" + k.Key);
        //        }
        //        else
        //        {
        //            Console.WriteLine("there was a problem with the keys at stockview activity ");
        //            return;
        //        }


        //        using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
        //        {
        //            Console.WriteLine("created new httprequest message");
        //            MainActivity.Manager_API_Keys.UseKey(k.Key);


        //            var response2 = await httpClient2.SendAsync(request);
        //            response2.EnsureSuccessStatusCode();
        //            string responseBody = await response2.Content.ReadAsStringAsync();
        //            JSONArray HistInfo = new JSONArray(responseBody);
        //            Console.WriteLine(HistInfo.Length());

        //            Datalist[place].open = ((float)(HistInfo.GetJSONObject(0).GetDouble("low")));
        //            Datalist[place].price = ((float)(HistInfo.GetJSONObject(0).GetDouble("high")));
        //            //Datalist[place].date = ((string)(HistInfo.GetJSONObject(0).Get("date")));
        //        }
        //    }

        //    //Toast.MakeText(this, "data list count is: " + Datalist.Count, ToastLength.Short).Show();
        //    Console.WriteLine("data list count is: " + Datalist.Count);
        //    Console.WriteLine("Temp_Datalist count is: " + Temp_Datalist.Count);
        //    Console.WriteLine("place is: " + place);
        //    Console.WriteLine("adding to temp data list in place: " + place);
        //    Temp_Datalist.Add(Datalist[place]);


        //    if (IsDataCountFull && Temp_Datalist.Count == Datalist.Count)
        //    {
        //        int count = Docs_In_DataBase.Count;
        //        for (int g = count - 1; g >= 0; g--)
        //        {
        //            UpdateTrackItemAsync((string)Docs_In_DataBase[g].Get("symbol"), g);
        //        }
        //        Toast.MakeText(this, "presenting the list", ToastLength.Short).Show();
        //        ShowListView();
        //    }
        //    Dispose(true);
        //    return;
        //}

        //private async Task processAllSavedStocks()
        //{
        //    string s = Read_from_file();



        //    s = s.Replace("\0", "");
        //    s = s.Replace("\n", "");

        //    string[] s2 = s.Split(',');
        //    Console.WriteLine(s2);
        //    Console.WriteLine("------------------------------------------------------------------------------------------");
        //    for (int i = 0; i < s2.Length; i++)
        //    {
        //        if (s2[i] != null && s2[i].Length != 0)
        //        {
        //            list.Add(s2[i]);
        //            Console.WriteLine(s2[i]);
        //            Console.WriteLine("------------------------------------------------------------------------------------------");
        //            StockData d = new StockData();
        //            d.symbol = s2[i];
        //            Datalist.Add(d);
        //            _ = GetInfoFromWeb(s2[i].ToString(), i);
        //        }
        //    }

        //}
        //private string Read_from_file()
        //{
        //    try
        //    {
        //        string str;
        //        //using (Stream stream = OpenFileOutput("Emailinfo.txt", Android.Content.FileCreationMode.Private))
        //        using (Stream stream = OpenFileInput("SavedStocks.txt"))
        //        {
        //            try
        //            {
        //                byte[] buffer = new byte[4096];
        //                stream.Read(buffer, 0, buffer.Length);
        //                str = System.Text.Encoding.UTF8.GetString(buffer);
        //                stream.Close();
        //                if (str != null)
        //                {
        //                    //  tv.Text = str;
        //                    //Toast.MakeText(this, str, ToastLength.Short).Show();
        //                    return str;
        //                }
        //            }
        //            catch (Java.IO.IOException a)
        //            {
        //                a.PrintStackTrace();
        //            }
        //        }
        //    }
        //    catch (Java.IO.FileNotFoundException a)
        //    {
        //        a.PrintStackTrace();

        //    }

        //    return null;
        //}

        //private void Add_To_File(string the_stock)
        //{
        //    try
        //    {
        //        string str = the_stock + ",";//= et.Text;
        //        using (Stream stream = OpenFileOutput("SavedStocks.txt", Android.Content.FileCreationMode.Append))
        //        {
        //            try
        //            {
        //                if (!Is_Allready_In_File(the_stock))
        //                {
        //                    stream.Write(Encoding.ASCII.GetBytes(str), 0, str.Length);
        //                    stream.Close();
        //                    Toast.MakeText(this, "saved", ToastLength.Short).Show();
        //                }
        //            }
        //            catch (Java.IO.IOException a)
        //            {
        //                a.PrintStackTrace();
        //            }
        //        }
        //    }
        //    catch (Java.IO.FileNotFoundException a)
        //    {
        //        a.PrintStackTrace();
        //    }

        //}

        //private bool Is_Allready_In_File(string the_stock)
        //{
        //    string str = Read_from_file();
        //    if (str== null || str.Contains(the_stock))
        //    {
        //        Toast.MakeText(this, "all ready in", ToastLength.Short).Show();
        //        return true;
        //    }
        //    return false;
        //}




    }
}