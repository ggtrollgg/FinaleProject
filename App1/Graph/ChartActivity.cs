using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using Android.OS;
using Android.Views;
using Android.Widget;
using App1.Algorithm;
using Firebase;
using Firebase.Firestore;
//import doc, deleteDoc from firestore;
using Java.Util;
using Org.Json;
using System;
using System.Collections.Generic;
//using Syncfusion.SfChart.XForms;
//using Xamarin.Forms;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace App1
{
    [Activity(Label = "ChartActivity")]
    public class ChartActivity : Activity, Android.Gms.Tasks.IOnSuccessListener
    {
        
        List<float> list = new List<float>();
        List<string> list_Dates = new List<string>();
        List<DataPoint> list_DataPoints= new List<DataPoint>();

        //List<string> Symbols_In_DataBase = new List<string>();
        List<DocumentSnapshot> Docs_In_DataBase = new List<DocumentSnapshot>();
        List<Class_FatherGraph> Charts = new List<Class_FatherGraph>();
        //Button btnMove, btnZoom;

        LinearLayout l1;
        Button btnTrack, btnCancel;
        EditText etTrackingPrices;



        ImageButton ibHome,ibSave,ibTrack,ibData,ibType;
        

        //StockChart chart;
        Class_LineGraph chart2;
        
        Dialog d;


        public FirebaseFirestore db;
        string Symbol = "";
        string trackingprices = "";





        //algorithm added to activity:
        Context context;
        LinearLayout algoLL, LLProgress, LLProgressBar, LLPrediction;
        Button btnStart, btnGoBack;
        List<RadioButton> radioButtons = new List<RadioButton>();
        RadioButton RB1min, RB5min, RB15min, RB30min, RB1hour;
        TextView TVProgress, TVPrediction;
        CheckBox CBdoubleAver, CBdrawProcess;
        EditText ETdegree, ETMinOrder, ETfuterPoint;
        MyHandler hand;
        ProgressBarHandler Hand_Progress;
        Handler handler;
        Message m = new Message();
        MATL_Algorithm MATLAlgo;
        string timeleap = "";





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

            //chart = new StockChart(this);
            Symbol = Intent.GetStringExtra("symbol") ?? "";
            

            ColumGraph chart1= new ColumGraph(this);
            chart2 = new Class_LineGraph(this);
            Class_CandleGraph chart3 = new Class_CandleGraph(this);

            Charts.Add(chart2);
            Charts.Add(chart1);
            Charts.Add(chart3);

            hand = new MyHandler(this);
            Hand_Progress = new ProgressBarHandler(this);
            context = this;

            Console.WriteLine("1");
            _ = getInfoFromWeb();

            SetUpDataBase();

            RegisterForContextMenu(ibType);

        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);
            MenuInflater.Inflate(Resource.Menu.Type_Menu, menu);
        }
        public override bool OnContextItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_LineGraph1)
            {
                l1.RemoveAllViews();
                Charts[0].dataPoints = list_DataPoints;
                l1.AddView(Charts[0]);
                return true;
            }
            else if (item.ItemId == Resource.Id.action_ColumGraph1)
            {
                l1.RemoveAllViews();
                Charts[1].dataPoints = list_DataPoints;
                l1.AddView(Charts[1]);
                return true;
            }
            else if (item.ItemId == Resource.Id.action_CandleGraph1)
            {
                try
                {
                    l1.RemoveAllViews();
                    Charts[2].dataPoints = list_DataPoints;
                    l1.AddView(Charts[2]);
                    return true;
                }
                catch
                {
                    Console.WriteLine("tryed to add view to candle graph and something went wrong");
                }
                
            }
            
            return false;

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
            //chart.values = arrey;
            //chart.Dates = arrey2;

            //chart2.dataPoints = list_DataPoints;
            //l1.AddView(chart2);
            Charts[0].dataPoints = list_DataPoints;
            l1.AddView(Charts[0]);
            //l1.AddView(chart);
        }
        //taking information about stock from the internet
        public async Task getInfoFromWeb()
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
                //link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");


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
                


                // using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://financialmodelingprep.com/api/v3/quote-short/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860"))
                //using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://financialmodelingprep.com/api/v3/historical-chart/1min/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860"))
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    //request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                    MainActivity.Manager_API_Keys.UseKey(k.Key);
                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();


                    float high,low,close,open;
                    

                    JSONArray HistInfo = new JSONArray(responseBody);
                    Console.WriteLine(HistInfo.Length());
                    int length = HistInfo.Length() - 1;
                    for (int i = length; i >= 0; i--)
                    {
                         high = (float)HistInfo.GetJSONObject(i).GetDouble("high");
                         low = (float)HistInfo.GetJSONObject(i).GetDouble("low");
                         close = (float)HistInfo.GetJSONObject(i).GetDouble("close");
                         open = (float)HistInfo.GetJSONObject(i).GetDouble("open");
                         string date = (string)HistInfo.GetJSONObject(i).Get("date");

                       // float avr = (float)(high + low) / 2;
                       //  Console.WriteLine("avr: " + avr);
                       // Console.WriteLine("close: " + close);

                        list.Add(close);
                        list_Dates.Add(date);
                        list_DataPoints.Add(new DataPoint(high, low,close,open, date));

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

        public void ChangeIcons()
        {
            string symbol = Intent.GetStringExtra("symbol") ?? "AAPL";
            bool IsInDataBase = false;
            bool Istracked = false;

            foreach (var doc in Docs_In_DataBase)
            {
                if (symbol == (string)doc.Get("symbol"))
                {
                    IsInDataBase = true;
                    if ((string)doc.Get("TrackingPrices") != "")
                    {
                        Istracked= true;
                    }
                }
            }

            if (IsInDataBase) //it is in the data base
            {
                Console.WriteLine("change to colored icon");
                //ibSave.SetImageDrawable((Android.Graphics.Drawables.Drawable)"@drawble/icon_favorite_colored");
                ibSave.SetImageResource(Resource.Drawable.Icon_Favorite_colored2);
                if (Istracked)
                {
                    ibTrack.SetImageResource(Resource.Drawable.Icon_Track_Colored);
                }
                
            }
            else
            {
                Console.WriteLine("change to uncolored icon");
                //ibSave.SetImageDrawable((Android.Graphics.Drawables.Drawable)Resource.Drawable.Icon_Favorite);
                ibSave.SetImageResource(Resource.Drawable.Icon_Favorite2);
                ibTrack.SetImageResource(Resource.Drawable.Icon_Track);
            }
        }


        public void SetUpDataBase()
        {
            db = GetDataBase();
            _ = LoadItemsAsync();

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

            try
            {
                var app = FirebaseApp.InitializeApp(this, options);
                db = FirebaseFirestore.GetInstance(app);
                return db;
            }
            catch
            {
                try
                {
                    var app = FirebaseApp.GetApps(this);
                    db = FirebaseFirestore.GetInstance(app[0]);
                    return db;
                }
                catch 
                {
                    Console.WriteLine("used db app[2]");
                    var app = FirebaseApp.GetApps(this);
                    db = FirebaseFirestore.GetInstance(app[1]);
                    return db;
                }
                
            }
        }

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
                if((string)doc.Get("symbol") == Symbol)
                {
                    trackingprices = (string)doc.Get("TrackingPrices");
                }
            }
            ChangeIcons();
        }



        private void DeleteItem_fromDataBase(int index)
        {
            DocumentReference doc = db.Collection("Saved Stocks").Document(Docs_In_DataBase[index].Id);
            doc.Delete();
            Docs_In_DataBase.RemoveAt(index);
            //ibSave.SetImageDrawable((Android.Graphics.Drawables.Drawable)Resource.Drawable.Icon_Favorite);
            ibSave.SetImageResource(Resource.Drawable.Icon_Favorite2);
        }
        private void AddItem_ToDataBAse(string symbol)
        {
            HashMap map = new HashMap();
            map.Put("symbol", symbol);
            map.Put("SoundFile", "");
            map.Put("TrackingPrices", "");
            map.Put("Price", list_DataPoints[0].close);
            map.Put("Open", list_DataPoints[0].open);

            CollectionReference collection = db.Collection("Saved Stocks");
            collection.Add(map);
        }



        private void AddTrackItem_ToDataBAse(string symbol)
        {
            HashMap map = new HashMap();
            map.Put("symbol", symbol);
            map.Put("SoundFile", "");
            map.Put("TrackingPrices", etTrackingPrices.Text);
            map.Put("Price", list_DataPoints[0].close);
            map.Put("Open", list_DataPoints[0].open);

            CollectionReference collection = db.Collection("Saved Stocks");
            collection.Add(map);
        }
        private void UpdateTrackItemAsync(string symbol,int index)
        {
            //Docs_In_DataBase[index].("TrackingPrices").Set(etTrackingPrices.Text);
            //await Docs_In_DataBase[index].UpdateAsync("Capital", false);
            string soundfile = (string)Docs_In_DataBase[index].Get("SoundFile");
            DeleteItem_fromDataBase(index);

            HashMap map = new HashMap();
            map.Put("symbol", symbol);
            map.Put("SoundFile", soundfile);
            map.Put("TrackingPrices", etTrackingPrices.Text);
            map.Put("Price", list_DataPoints[0].close);
            map.Put("Open", list_DataPoints[0].open);

            CollectionReference collection = db.Collection("Saved Stocks");
            collection.Add(map);
        }





        //buttons
        private void IbTrack_Click(object sender, EventArgs e)
        {
            
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.Custom_PopUp_Track);
            d.SetTitle(Symbol);
            d.SetCancelable(true);

            btnCancel = d.FindViewById<Button>(Resource.Id.btnCancel);
            btnTrack = d.FindViewById<Button>(Resource.Id.btnTrack);
            etTrackingPrices = d.FindViewById<EditText>(Resource.Id.etTrackingPrices);
            etTrackingPrices.Text = etTrackingPrices.Text + trackingprices;

            btnCancel.Click += BtnCancel_Click;
            btnTrack.Click += BtnTrack_Click;
            d.Show();
        }

        private void BtnTrack_Click(object sender, EventArgs e)
        {
            if(Symbol == "")
            {
                Console.WriteLine("didnt get symbol in chart activity");
                return;
            }

            bool IsInDataBase = false;
            int i = 0,index = -1;
            foreach (var doc in Docs_In_DataBase)
            {
                if (Symbol == (string)doc.Get("symbol"))
                {
                    IsInDataBase = true;
                    index = i;
                }
                i++;
            }
            if (IsInDataBase)//if in data base than update to new values
            {
                UpdateTrackItemAsync(Symbol,index);
                _ = LoadItemsAsync();
                d.Cancel();
            }
            else//if not in data than add new
            {
                AddTrackItem_ToDataBAse(Symbol);
                _ = LoadItemsAsync();
                d.Cancel();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            btnTrack.Click -= BtnTrack_Click;
            d.Cancel();
        }

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

        private void IbType_Click(object sender, EventArgs e)
        {
            //d = new Dialog(this);
            //d.SetContentView(Resource.Layout.Custom_PopUp_MiniGraph);
            //d.SetTitle("type of graphs");
            //d.SetCancelable(true);
            //d.Show();
        }

        private void IbHome_Click(object sender, EventArgs e)
        {
            //db.App.Delete();
            db.App.Dispose();
            //db.Dispose();
            //db = null;

            Finish();
        }









































        // algorithm
        private void IbData_Click(object sender, EventArgs e)
        {
            d = new Dialog(this);
            d.SetContentView(Resource.Layout.Custom_PopUp_Algorithm);
            d.SetTitle("abot the stock");
            d.SetCancelable(true);

            if (radioButtons.Count > 0)
            {
                radioButtons.Clear();
            }

            algoLL = d.FindViewById<LinearLayout>(Resource.Id.LLcanvas);
            LLProgress = d.FindViewById<LinearLayout>(Resource.Id.LLProgress);
            LLProgressBar = d.FindViewById<LinearLayout>(Resource.Id.LLProgressBar);
            LLPrediction = d.FindViewById<LinearLayout>(Resource.Id.LLPrediction);

            ETdegree = d.FindViewById<EditText>(Resource.Id.ETdegree);
            ETMinOrder = d.FindViewById<EditText>(Resource.Id.ETMinOrder);
            ETfuterPoint = d.FindViewById<EditText>(Resource.Id.ETfuterPoint);

            TVPrediction = d.FindViewById<TextView>(Resource.Id.TVPrediction);
            TVProgress = d.FindViewById<TextView>(Resource.Id.TVProgress);


            RB1min = d.FindViewById<RadioButton>(Resource.Id.RB1min);
            RB5min = d.FindViewById<RadioButton>(Resource.Id.RB5min);
            RB15min = d.FindViewById<RadioButton>(Resource.Id.RB15min);
            RB30min = d.FindViewById<RadioButton>(Resource.Id.RB30min);
            RB1hour = d.FindViewById<RadioButton>(Resource.Id.RB1hour);

            radioButtons.Add(RB1min);
            radioButtons.Add(RB5min);
            radioButtons.Add(RB15min);
            radioButtons.Add(RB30min);
            radioButtons.Add(RB1hour);

            CBdoubleAver = d.FindViewById<CheckBox>(Resource.Id.CBdoubleAver);
            CBdrawProcess = d.FindViewById<CheckBox>(Resource.Id.CBdrawProcess);

            btnStart = d.FindViewById<Button>(Resource.Id.btnStart);
            btnGoBack = d.FindViewById<Button>(Resource.Id.btnGoBack);
            //setting up handler for progress bar
            Hand_Progress.TVPrediction = TVPrediction;
            Hand_Progress.TVProgress = TVProgress;
            Hand_Progress.LLPrediction = LLPrediction;
            Hand_Progress.LLProgress = LLProgress;
            Hand_Progress.LLProgressBar = LLProgressBar;

            //there can only be one raidobutton check in any instance
            foreach (RadioButton rb in radioButtons)
            {
                rb.Click += (s, e) =>
                {
                    //when a radio button is clicked, turn off all other checked boxes. can be more efficent if stop when find a checked box because there will alwayes be only 2 boxes checked.
                    foreach (RadioButton rb2 in radioButtons)
                    {
                        if (s != rb2 && rb2.Checked)
                        {
                            rb2.Checked = false;
                        }
                    }


                };
            }
            btnStart.Click += BtnStart_Click;
            btnGoBack.Click += BtnGoBack_Click;
            d.Show();

        }



        private void BtnGoBack_Click(object sender, EventArgs e)
        {
            d.Cancel();
            if (MATLAlgo != null)
            {
                MATLAlgo.AbortProcess();
                algoLL.RemoveAllViews();
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {

            foreach (RadioButton rb in radioButtons)
            {
                if (rb.Checked)
                {
                    string buttonText = rb.Text.Trim();
                    timeleap = buttonText.Replace(" ", "");

                    //Console.WriteLine("-------------------------");
                    //Console.WriteLine(timeleap);
                    //Console.WriteLine("-------------------------");
                    if (ETdegree.Text.Length == 0)
                    {
                        Console.WriteLine("need to fill the text in MaxOrder");
                        return;
                    }
                    _ = getInfoFromWeb(timeleap);
                    break;
                }
            }
            //need to get new series of points from web :(

        }



        public async Task<List<DataPoint>> getInfoFromWeb(string timeLeap)
        {
            List<DataPoint> newList = new List<DataPoint>();
            using (var httpClient = new HttpClient())
            {


                string symbol = Intent.GetStringExtra("symbol") ?? "AAPL";
                string link = "https://financialmodelingprep.com/api/v3/historical-chart/";
                link = link.Insert(link.Length, timeLeap + "/");
                link = link.Insert(link.Length, symbol);

                API_Key k = MainActivity.Manager_API_Keys.GetBestKey();
                if (k != null && k.Key != "" && k.GetCallsRemaining() > 0)
                {
                    link = link.Insert(link.Length, "?apikey=" + k.Key);
                }
                else
                {
                    Console.WriteLine("there was a problem with the keys at stockview activity ");
                    return null;
                }
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    //request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                    MainActivity.Manager_API_Keys.UseKey(k.Key);
                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();


                    float high, low, close, open;


                    JSONArray HistInfo = new JSONArray(responseBody);
                    Console.WriteLine(HistInfo.Length());
                    int length = HistInfo.Length() - 1;
                    for (int i = length; i >= 0; i--)
                    {
                        high = (float)HistInfo.GetJSONObject(i).GetDouble("high");
                        low = (float)HistInfo.GetJSONObject(i).GetDouble("low");
                        close = (float)HistInfo.GetJSONObject(i).GetDouble("close");
                        open = (float)HistInfo.GetJSONObject(i).GetDouble("open");
                        string date = (string)HistInfo.GetJSONObject(i).Get("date");

                        newList.Add(new DataPoint(high, low, close, open, date));

                        //Console.WriteLine((string)(HistInfo.GetJSONObject(i).Get("date")));
                    }


                }
            }


            StartAlgorithm(newList);
            return newList;
        }

        private void StartAlgorithm(List<DataPoint> newList)
        {
            if (ETdegree.Text.Length < 1)
            {
                Console.WriteLine("need to fill the order field ");
                return;
            }
            if (ETfuterPoint.Text.Length < 1)
            {
                Console.WriteLine("need to fill the futer point field ");
                return;
            }


            MATLAlgo = new MATL_Algorithm(newList, int.Parse(ETdegree.Text), int.Parse(ETfuterPoint.Text), this);

            if (ETMinOrder.Text != "")
            {
                if (int.Parse(ETMinOrder.Text) > int.Parse(ETdegree.Text))
                {
                    Console.WriteLine("order need to be equal or bigger than Minorder ");
                    return;
                }

                MATLAlgo.SetMinOrder(int.Parse(ETMinOrder.Text));
            }

            Start_progress_Bar("creating graphs of moving averages ");
            MATLAlgo.ContinueProcess += Continue_Algorithm_Process;
            MATLAlgo.Start_Algorithm();


        }


        private void Continue_Algorithm_Process()
        {



            if (CBdrawProcess.Checked) //draw process
            {
                //Console.WriteLine("CBdrawProcess is checked");
                MA_View graph_view = new MA_View(this, MATLAlgo);
                hand.LL = algoLL;
                hand.graph_view = graph_view;
                handler = hand;
                m = new Message();
                m.Arg1 = 1;
                //handler.HandleMessage(m);
                handler.SendMessage(m);
                Add_progress_ToBar("drawing graphs");

            }
            else
            {
                hand.LL = algoLL;
                if (hand.graph_view != null)
                {
                    hand.graph_view = null;
                }
                handler = hand;
                m = new Message();
                m.Arg1 = 1;
                handler.SendMessage(m); //hide the canvas if visiball
            }
            Add_Prediction("My predection is: \n in " + MATLAlgo.FuterPoint + " * " + timeleap + " the price of the stock will be: " + MATLAlgo.prediction.price);
        }



        public void Start_progress_Bar(string description)
        {
            m = new Message();
            m.Arg1 = -1; //0 == add image view to progress bar
            Hand_Progress.status = description;
            handler = Hand_Progress;
            handler.SendMessage(m);

        }
        public void Add_progress_ToBar(string description)
        {
            m = new Message();
            m.Arg1 = 0; //0 == add image view to progress bar
            Hand_Progress.status = description;
            handler = Hand_Progress;
            handler.SendMessage(m);

        }
        public void Add_Prediction(string description)
        {
            m = new Message();
            m.Arg1 = 1; //1 == show prediction
            Hand_Progress.prediction = description;
            handler = Hand_Progress;
            handler.SendMessage(m);
        }




























        //private void BtnStart_Click(object sender, EventArgs e)
        //{
        //    if(RB1min.Checked)//if selected 1 min between each point
        //    {
        //        MATL_Algorithm MATLAlgo = new MATL_Algorithm(list_DataPoints, int.Parse(ETdegree.Text), int.Parse(ETfuterPoint.Text), this);
        //        while (MATLAlgo.movingAverage_Graph.Count < int.Parse(ETdegree.Text))
        //        {
        //            Thread.Sleep(1000);
        //        }

        //        if (CBdrawProcess.Checked)
        //        {
        //            Console.WriteLine("CBdrawProcess is checked");
        //            algoLL.Visibility = ViewStates.Visible;
        //            MA_View graph_view = new MA_View(this, MATLAlgo.movingAverage_Graph);
        //            algoLL.AddView(graph_view);
        //            //add canvas view
        //        }
        //    }
        //    else
        //    {
        //        //need to get new series of points from web :(
        //    }
        //}

        //private void CBdrawProcess_Click(object sender, EventArgs e)
        //{
        //    if (CBdrawProcess.Checked)
        //    {
        //        Console.WriteLine("CBdrawProcess is checked");
        //        //add canvas view
        //    }
        //    else
        //    {
        //        //remove algo view
        //    }
        //}























































        protected override void OnPause()
        {
            if(db!= null)
            {
                if (db.App != null)
                {
                    //db.App.Delete();
                    //db.Terminate();
                    db.App.Dispose();
                    //db = null;
                }
            }
            
            
            base.OnPause();
        }


        //not used
        //private void BtnMove_Click(object sender, EventArgs e)
        //{
        //    chart.Move = !chart.Move;
        //    if (chart.Move)
        //    {
        //        btnMove.SetBackgroundColor(Android.Graphics.Color.Green);
        //    }
        //    else
        //    {
        //        btnMove.SetBackgroundColor(Android.Graphics.Color.Red);
        //    }
        //}

        //private void BtnZoom_Click(object sender, EventArgs e)
        //{
        //    chart.Zoom = !chart.Zoom;
        //    if (chart.Zoom)
        //    {
        //        btnZoom.SetBackgroundColor(Android.Graphics.Color.Green);
        //        //btnZoom.Background = (Android.Graphics.Drawables.Drawable)"green";
        //    }
        //    else
        //    {
        //        btnZoom.SetBackgroundColor(Android.Graphics.Color.Red);
        //        //btnZoom.Background = (Android.Graphics.Drawables.Drawable)"red";
        //    }
        //}

        //public String[] CleanAndSaperet(String TheContent)
        //{
        //    if (TheContent == null) { Console.WriteLine("the content is null"); return null; }

        //    if (TheContent.Contains("\n")) TheContent = TheContent.Replace("\n", "");
        //    if (TheContent.Contains("\r")) TheContent = TheContent.Replace("\r", "");

        //    TheContent = TheContent.Replace('{', ' ');
        //    TheContent = TheContent.Replace('}', ' ');
        //    TheContent = TheContent.Replace('[', ' ');
        //    TheContent = TheContent.Replace(']', ' ');
        //    TheContent = TheContent.Replace('(', ' ');
        //    TheContent = TheContent.Replace(')', ' ');
        //    TheContent = TheContent.Replace(':', ',');
        //    TheContent = TheContent.Replace('"', ' ');
        //    TheContent = TheContent.Replace(" ", "");

        //    String[] s = TheContent.Split(',');
        //    return s;
        //}
    }
}