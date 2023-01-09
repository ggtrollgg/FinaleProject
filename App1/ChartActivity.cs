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
//using Syncfusion.SfChart.XForms;
//using Xamarin.Forms;

using System.Net.Http;
using System.Threading.Tasks;
using Org.Json;

using Firebase.Firestore;
using Firebase;
//import doc, deleteDoc from firestore;
using Java.Util;
using Java.Lang.Reflect;
using System.Runtime.InteropServices.ComTypes;
using Android.Graphics;
using Firestore.Admin.V1;
using System.Collections.ObjectModel;
using Android.Gms.Extensions;

namespace App1
{
    [Activity(Label = "ChartActivity")]
    public class ChartActivity : Activity, Android.Gms.Tasks.IOnSuccessListener
    {
        List<float> list = new List<float>();
        List<string> list_Dates = new List<string>();

        //List<string> Symbols_In_DataBase = new List<string>();
        List<DocumentSnapshot> Docs_In_DataBase = new List<DocumentSnapshot>();

        Button btnMove, btnZoom;
        ImageButton ibHome,ibSave,ibTrack,ibData,ibType;
        LinearLayout l1;

        StockChart chart;
        Dialog d;

        public FirebaseFirestore db;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ChartLayout);
            //btnMove = FindViewById<Button>(Resource.Id.btnMove);
            //btnZoom = FindViewById<Button>(Resource.Id.btnZoom);

            l1 = FindViewById<LinearLayout>(Resource.Id.LLChart);
            ibHome = FindViewById<ImageButton>(Resource.Id.ibHome);
            ibSave = FindViewById<ImageButton>(Resource.Id.ibSave);
            ibTrack = FindViewById<ImageButton>(Resource.Id.ibTrack);
            ibData = FindViewById<ImageButton>(Resource.Id.ibGraphData);
            ibType = FindViewById<ImageButton>(Resource.Id.ibGraphType);


            ibHome.Click += IbHome_Click;
            ibType.Click += IbType_Click;
            ibData.Click += IbData_Click;

            ibSave.Click += IbSave_Click;
            ibTrack.Click += IbTrack_Click;

            //btnZoom.Click += BtnZoom_Click;
            //btnMove.Click += BtnMove_Click;

            chart = new StockChart(this);

            Console.WriteLine("1");
            _ = testAsync();

            SetUpDataBase();
            
        }

        private void IbTrack_Click(object sender, EventArgs e)
        {

        }




        //putting the info into the class and putting the class in the linear layout
        public void test()
        {

            float[] arrey = new float[list.Count];
            String[] arrey2 = new string[list.Count];
            for (int i = 0; i < arrey.Length; i++)
            {
                arrey[i] = list[i];
                arrey2[i] = list_Dates[i];
            }
            chart.values = arrey;
            chart.Dates = arrey2;
            l1.AddView(chart);
        }
        //taking information about stock from the internet
        public async Task testAsync()
        {
            Console.WriteLine("2");
            using (var httpClient = new HttpClient())

            {
                string symbol = Intent.GetStringExtra("symbol") ?? "AAPL";

                //Intent intent = new Intent(this, typeof(SearchActivity));

                //Intent.GetFloatArrayExtra("Chart_Points_Heigh");
                //Intent.GetFloatArrayExtra("Chart_Points_Low");
                //Intent.GetStringArrayExtra("Chart_Points_Date");


                string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
                link = link.Insert(link.Length, symbol);
                link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");





                // using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://financialmodelingprep.com/api/v3/quote-short/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860"))
                //using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://financialmodelingprep.com/api/v3/historical-chart/1min/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860"))
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    //request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");

                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();




                    JSONArray HistInfo = new JSONArray(responseBody);
                    Console.WriteLine(HistInfo.Length());

                    for (int i = 0; i < HistInfo.Length(); i++)
                    {
                        float avr = (float)((HistInfo.GetJSONObject(i).GetDouble("low") + HistInfo.GetJSONObject(i).GetDouble("high")) / 2);
                        //Console.WriteLine(avr);
                        list.Add(avr);
                        list_Dates.Add((string)(HistInfo.GetJSONObject(i).Get("date")));
                        //Console.WriteLine((string)(HistInfo.GetJSONObject(i).Get("date")));
                    }


                }
            }
            Console.WriteLine("3");
            test();
            return;
        }



        //suppose to be about data base
        //currently doesnt work properly

        public void SetUpDataBase()
        {
            db = GetDataBase();
            _ = LoadItemsAsync();




            //string symbol = Intent.GetStringExtra("symbol") ?? "AAPL";
            //if (Symbols_In_DataBase.Contains(symbol))
            //{
            //    Color c = new Color();
            //    c = Color.Green;
            //    ibSave.SetColorFilter(c);
            //}
           
        }
        public FirebaseFirestore GetDataBase()
        {

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

        //private void LoadItems()
        //{
        //    // generate a query (request) from the database
        //    Query q = db.Collection("Saved Stocks");
        //    if (Symbols_In_DataBase.Count > 0)
        //    {
        //        Symbols_In_DataBase.Clear();
        //        SymDoc_In_DataBase.Clear();
        //    }
        //    q.Get().AddOnSuccessListener(this); 
        //}

        //public void OnSuccess(Java.Lang.Object result)
        //{
        //    string symbol = Intent.GetStringExtra("symbol") ?? "AAPL";
        //    var snapshot = (QuerySnapshot)result;
        //    StockData data;
        //    int i = 0;
        //    // iterate through each document in the collection
        //    foreach (var doc in snapshot.Documents)
        //    {
        //        Symbols_In_DataBase.Add((string)doc.Get("symbol"));
        //        SymDoc_In_DataBase.Add(doc);
        //       //if(symbol== (string)doc.Get("symbol"))
        //       // {
        //       //     Console.WriteLine("The stock is already in the data base");
        //       //     return;
        //       // }

        //    }
        //    //Console.WriteLine("the stock wasnt in the data base");
        //    //AddItemSave(symbol);
        //}

        private async Task LoadItemsAsync()
        {
            Docs_In_DataBase.Clear();

            Query q = db.Collection("Saved Stocks");
            await q.Get().AddOnSuccessListener(this);

        }
        public void OnSuccess(Java.Lang.Object result)
        {
            var snapshot = (QuerySnapshot)result;
            foreach (var doc in snapshot.Documents)
            {
                Docs_In_DataBase.Add(doc);
            }
        }



        private void DeleteItem_fromDataBase(int index)
        {
            DocumentReference doc = db.Collection("Saved Stocks").Document(Docs_In_DataBase[index].Id);
            doc.Delete();
            Docs_In_DataBase.RemoveAt(index);
        }
        private void AddItem_ToDataBAse(string symbol)
        {
            HashMap map = new HashMap();
            map.Put("symbol", symbol);
            map.Put("LastDate", "");
            map.Put("SoundFile", "");
            map.Put("TrackingPrices", "");
            map.Put("heigh", 0);
            map.Put("low", 0);

            CollectionReference collection = db.Collection("Saved Stocks");
            collection.Add(map);
        }


        //private void AddItemSave(string symbol)
        //{
        //    Console.WriteLine("Adding the stock: " + symbol + " to the data base");

        //    HashMap map = new HashMap();
        //    map.Put("symbol", symbol);
        //    map.Put("LastDate", "");
        //    map.Put("SoundFile", "");
        //    map.Put("TrackingPrices", "");
        //    map.Put("heigh", 0);
        //    map.Put("low", 0);
        //    DocumentReference docRef = db.Collection("Saved Stocks").Document();

        //    docRef.Set(map);

        //    db.App.Dispose();
        //    db.App.Delete();
        //    //db.Terminate();
        //}

        //private void DeleteItemSave( int index)
        //{
        //    //db.Collection("Saved Stocks").Document(SymDoc_In_DataBase[index].Id).Delete();
        //    DocumentReference doc = db.Collection("Saved Stocks").Document(SymDoc_In_DataBase[index].Id);
        //    doc.Delete();


        //    SymDoc_In_DataBase.RemoveAt(index);
        //    Symbols_In_DataBase.RemoveAt((int)index);
        //}




        //buttons
        private void IbSave_Click(object sender, EventArgs e)
        {
            int index = -1;
            int i = 0;

            string symbol = Intent.GetStringExtra("symbol") ?? "AAPL";
            bool IsInDataBase = false;

            foreach (var doc in Docs_In_DataBase)
            {
                if (symbol == (string)doc.Get("symbol"))
                {
                    IsInDataBase = true;
                    index = i;
                }
                i++;
            }

            if (IsInDataBase) //it is in the data base
            {
                Console.WriteLine("deleting from database");
                DeleteItem_fromDataBase(index);
                return;
            }
            else //it isn't in the data base
            {
                Console.WriteLine("adding to database");
                AddItem_ToDataBAse(symbol);
                _ = LoadItemsAsync(); //refresh to the real oreder of items in the data base
                return;
            }

        }

        private void IbData_Click(object sender, EventArgs e)
        {
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.Custom_PopUp_MiniGraph);
            d.SetTitle("abot the stock");
            d.SetCancelable(true);
            d.Show();
        }

        private void IbType_Click(object sender, EventArgs e)
        {
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.Custom_PopUp_MiniGraph);
            d.SetTitle("type of graphs");
            d.SetCancelable(true);
            d.Show();
        }

        private void IbHome_Click(object sender, EventArgs e)
        {
            db.App.Delete();
            db.Dispose();

            Finish();
        }




        //not used
        private void BtnMove_Click(object sender, EventArgs e)
        {
            chart.Move = !chart.Move;
            if (chart.Move)
            {
                btnMove.SetBackgroundColor(Android.Graphics.Color.Green);
            }
            else
            {
                btnMove.SetBackgroundColor(Android.Graphics.Color.Red);
            }
        }

        private void BtnZoom_Click(object sender, EventArgs e)
        {
            chart.Zoom = !chart.Zoom;
            if (chart.Zoom)
            {
                btnZoom.SetBackgroundColor(Android.Graphics.Color.Green);
                //btnZoom.Background = (Android.Graphics.Drawables.Drawable)"green";
            }
            else
            {
                btnZoom.SetBackgroundColor(Android.Graphics.Color.Red);
                //btnZoom.Background = (Android.Graphics.Drawables.Drawable)"red";
            }
        }

        public String[] CleanAndSaperet(String TheContent)
        {
            if (TheContent == null) { Console.WriteLine("the content is null"); return null; }

            if (TheContent.Contains("\n")) TheContent = TheContent.Replace("\n", "");
            if (TheContent.Contains("\r")) TheContent = TheContent.Replace("\r", "");

            TheContent = TheContent.Replace('{', ' ');
            TheContent = TheContent.Replace('}', ' ');
            TheContent = TheContent.Replace('[', ' ');
            TheContent = TheContent.Replace(']', ' ');
            TheContent = TheContent.Replace('(', ' ');
            TheContent = TheContent.Replace(')', ' ');
            TheContent = TheContent.Replace(':', ',');
            TheContent = TheContent.Replace('"', ' ');
            TheContent = TheContent.Replace(" ", "");

            String[] s = TheContent.Split(',');
            return s;
        }
    }
}