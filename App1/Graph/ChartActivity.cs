using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using Android.OS;
using Android.Telecom;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using App1.Algorithm;
using Firebase;
using Firebase.Firestore;
using Java.Util;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Google.Api.Distribution.BucketOptions;

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

        bool offlineMode = false;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ChartLayout);

            l1 = FindViewById<LinearLayout>(Resource.Id.LLChart);
            ibHome = FindViewById<ImageButton>(Resource.Id.ibHome);
            ibSave = FindViewById<ImageButton>(Resource.Id.ibSave);
            ibTrack = FindViewById<ImageButton>(Resource.Id.ibTrack);
            ibData = FindViewById<ImageButton>(Resource.Id.ibGraphData);
            ibType = FindViewById<ImageButton>(Resource.Id.ibGraphType);


            ibHome.Click += IbHome_Click;
            ibData.Click += IbData_Click;

            ibSave.Click += IbSave_Click;
            ibTrack.Click += IbTrack_Click;
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

            offlineMode = Intent.GetBooleanExtra("OfflineMode", false);
            if (offlineMode)
            {
                CreateOfflineGraph();
                StartDrawingGraphs();
            }
            else
            {
                Console.WriteLine("1");
                _ = getInfoFromWeb();

                SetUpDataBase();

            }

            RegisterForContextMenu(ibType);

        }

        //context menu staff 
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
        public void StartDrawingGraphs()
        {
            Charts[0].dataPoints = list_DataPoints;
            l1.AddView(Charts[0]);
        }
        //taking information about stock from the internet
        public async Task getInfoFromWeb()
        {
            Console.WriteLine("2");
            using (var httpClient = new HttpClient())

            {

                string symbol = Intent.GetStringExtra("symbol") ?? "AAPL";
                string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
                link = link.Insert(link.Length, symbol);
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
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
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
                        list.Add(close);
                        list_Dates.Add(date);
                        list_DataPoints.Add(new DataPoint(high,low,close,open, date));
                    }


                }
            }
            Console.WriteLine("3");
            StartDrawingGraphs();
            return;
        }


        //Changes the icon of the "saved" and "tracking" buttons when the stock is already in the data base in FireStore
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
                ibSave.SetImageResource(Resource.Drawable.Icon_Favorite_colored2);
                if (Istracked)
                {
                    ibTrack.SetImageResource(Resource.Drawable.Icon_Track_Colored);
                }
                
            }
            else
            {
                Console.WriteLine("change to uncolored icon");
                ibSave.SetImageResource(Resource.Drawable.Icon_Favorite2);
                ibTrack.SetImageResource(Resource.Drawable.Icon_Track);
            }
        }


        //-----Firestore - Data base staff-----//

        //setting up the data base 
        public void SetUpDataBase()
        {
            db = GetDataBase();
            _ = LoadItemsAsync();

        }

        //creating a connection to the database
        public FirebaseFirestore GetDataBase()
        {

            // info from "google-services.json"
            var options = new FirebaseOptions.Builder()
            .SetProjectId("stock-data-base-finalproject")
            .SetApplicationId("stock-data-base-finalproject")
            .SetApiKey("AIzaSyCjiFrMsBwOFvqUZRdohfIiqMsJC5QG_kc")
            .SetStorageBucket("stock-data-base-finalproject.appspot.com")
            .Build();


            //in case there are allready database instenses created in servecis are other places
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

        //creating a request to get the info from the data base in firestore
        private async Task LoadItemsAsync()
        {
            Docs_In_DataBase.Clear();

            Query q = db.Collection("Saved Stocks");
            await q.Get().AddOnSuccessListener(this);

        }

        //on reciving the info from firestore extract the usful info 
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


        //this deletes an instance of the item in the index from the database
        private void DeleteItem_fromDataBase(int index)
        {
            DocumentReference doc = db.Collection("Saved Stocks").Document(Docs_In_DataBase[index].Id);
            doc.Delete();
            Docs_In_DataBase.RemoveAt(index);
            ibSave.SetImageResource(Resource.Drawable.Icon_Favorite2);
        }
        //this creates a new instance of the symbol in the database
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

        //this creates a new instance of the symbol in the database with tracking prices
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

        //in case the stock is already in the data base than delete the privious one and create a new one
        //(sound file is a scrapped idea that i had at the beggining of the project)
        private void UpdateTrackItemAsync(string symbol,int index)
        {
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





        //--------buttons-------//
        //show the tracking PopUP
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

        //when clicked add/update the item to the  database
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

        //close the Tracking PopUp
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            btnTrack.Click -= BtnTrack_Click;
            d.Cancel();
        }

        //when clicked add the stock to the database without a tracking prices
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

        //go to the previous activity
        private void IbHome_Click(object sender, EventArgs e)
        {

            if(db != null && db.App!=null)
            {
                db.App.Dispose();
            }

            Finish();
        }




        // -------algorithm-------//
        //Set Up the Algorithm PopUp and start it
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
        //cancel the PopUp and abort the algorithm
        private void BtnGoBack_Click(object sender, EventArgs e)
        {
            d.Cancel();
            if (MATLAlgo != null)
            {
                MATLAlgo.AbortProcess();
                algoLL.RemoveAllViews();
            }
        }
        //start the algorithm
        private void BtnStart_Click(object sender, EventArgs e)
        {

            foreach (RadioButton rb in radioButtons)
            {
                if (rb.Checked)
                {
                    if(!offlineMode)
                    {
                        string buttonText = rb.Text.Trim();
                        timeleap = buttonText.Replace(" ", "");
                        if (ETdegree.Text.Length == 0)
                        {
                            Console.WriteLine("need to fill the text in MaxOrder");
                            return;
                        }
                        _ = getInfoFromWeb(timeleap);
                        return;
                    }
                    else
                    {
                        StartAlgorithm(list_DataPoints);
                    }
                }
            }


        }


        //in case and the user choose a diffrent time leaps than 1 min 
        //than get from financl.com the stock prices and info with the wanted time leaps
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


                    }


                }
            }


            StartAlgorithm(newList);
            return newList;
        }

        //start the allgorithm process in the class
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

        //an event that the class MATL_Algorithm calls when the process is finished
        //it is about setting up the drawing view for the graphs
        private void Continue_Algorithm_Process()
        {



            if (CBdrawProcess.Checked) //draw process
            {
                MA_View graph_view = new MA_View(this, MATLAlgo);
                hand.LL = algoLL;
                hand.graph_view = graph_view;
                handler = hand;
                m = new Message();
                m.Arg1 = 1;
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

        //-----ProgressBar Events-----//
        //start the progress bar -> make it visibale but with 0% done
        public void Start_progress_Bar(string description)
        {
            m = new Message();
            m.Arg1 = -1; //-1 == make bar visiball
            Hand_Progress.status = description;
            handler = Hand_Progress;
            handler.SendMessage(m);

        }
        //adding progress to the progressBar
        public void Add_progress_ToBar(string description)
        {
            m = new Message();
            m.Arg1 = 0; //0 == add image view to progress bar
            Hand_Progress.status = description;
            handler = Hand_Progress;
            handler.SendMessage(m);

        }
        //adding to the prediction LinearLayout the predicion that the algorithm created
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
                    db.App.Dispose();
                }
            }
            
            
            base.OnPause();
        }

        //when Offline mode is active, insert these hardvoded values to the list

        //I didnt Know there was a standart in the indastry to put these kind of info in a text file
        //(i origanaly had a class that created and worked with txt files but i scrapped it when they changed the conditions to pass the project)
        private void CreateOfflineGraph()
        {
            //in order to get these line i printed the points of the stock : INTC
            //I used the line :
            //Console.WriteLine("list_DataPoints.Add(new DataPoint((float)" +high+ ",(float)" + low+ ",(float)" + close+ ",(float)" + open+"," + '"' + date+'"' + "));");

            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.485, (float)29.485, (float)29.6, "2023-05-11 09:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.505, (float)29.44, (float)29.455, (float)29.48, "2023-05-11 09:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.4688, (float)29.42, (float)29.46, (float)29.4501, "2023-05-11 09:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.5, (float)29.39, (float)29.3911, (float)29.465, "2023-05-11 09:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.4, (float)29.34, (float)29.395, (float)29.395, "2023-05-11 09:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.33, (float)29.415, (float)29.39, "2023-05-11 09:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.495, (float)29.4, (float)29.49, (float)29.415, "2023-05-11 09:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.505, (float)29.42, (float)29.42, (float)29.485, "2023-05-11 09:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.35, (float)29.385, (float)29.42, "2023-05-11 09:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.38, (float)29.425, (float)29.385, "2023-05-11 09:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.39, (float)29.43, (float)29.43, "2023-05-11 09:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.43, (float)29.48, (float)29.43, "2023-05-11 09:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.53, (float)29.473, (float)29.525, (float)29.48, "2023-05-11 09:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.42, (float)29.425, (float)29.5297, "2023-05-11 09:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.3925, (float)29.395, (float)29.425, "2023-05-11 09:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.38, (float)29.395, (float)29.39, "2023-05-11 09:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.41, (float)29.35, (float)29.355, (float)29.39, "2023-05-11 09:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.3525, (float)29.29, (float)29.2924, (float)29.3525, "2023-05-11 09:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.3097, (float)29.27, (float)29.285, (float)29.295, "2023-05-11 09:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.28, (float)29.2202, (float)29.2284, (float)29.28, "2023-05-11 09:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.235, (float)29.2, (float)29.2299, (float)29.225, "2023-05-11 09:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.24, (float)29.2, (float)29.2, (float)29.225, "2023-05-11 09:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.225, (float)29.15, (float)29.155, (float)29.21, "2023-05-11 09:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.22, (float)29.155, (float)29.185, (float)29.155, "2023-05-11 09:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.22, (float)29.18, (float)29.195, (float)29.185, "2023-05-11 09:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.23, (float)29.18, (float)29.225, (float)29.19, "2023-05-11 09:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.24, (float)29.205, (float)29.2099, (float)29.22, "2023-05-11 09:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.25, (float)29.205, (float)29.21, (float)29.205, "2023-05-11 09:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.2199, (float)29.17, (float)29.175, (float)29.215, "2023-05-11 09:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.18, (float)29.13, (float)29.165, (float)29.175, "2023-05-11 09:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.205, (float)29.145, (float)29.185, (float)29.165, "2023-05-11 10:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.21, (float)29.17, (float)29.17, (float)29.18, "2023-05-11 10:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.23, (float)29.16, (float)29.225, (float)29.175, "2023-05-11 10:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.245, (float)29.18, (float)29.19, (float)29.225, "2023-05-11 10:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.2, (float)29.12, (float)29.1299, (float)29.1994, "2023-05-11 10:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.17, (float)29.115, (float)29.165, (float)29.1208, "2023-05-11 10:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.165, (float)29.1, (float)29.1, (float)29.165, "2023-05-11 10:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.13, (float)29.0812, (float)29.1022, (float)29.1, "2023-05-11 10:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.13, (float)29.085, (float)29.095, (float)29.11, "2023-05-11 10:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.15, (float)29.09, (float)29.135, (float)29.0923, "2023-05-11 10:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.15, (float)29.12, (float)29.125, (float)29.135, "2023-05-11 10:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.14, (float)29.1, (float)29.11, (float)29.125, "2023-05-11 10:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.135, (float)29.1, (float)29.105, (float)29.105, "2023-05-11 10:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.13, (float)29.1014, (float)29.105, (float)29.105, "2023-05-11 10:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.11, (float)29.06, (float)29.075, (float)29.1, "2023-05-11 10:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.08, (float)29.03, (float)29.05, (float)29.07, "2023-05-11 10:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.085, (float)29.02, (float)29.02, (float)29.05, "2023-05-11 10:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)28.97, (float)28.97, (float)29.02, "2023-05-11 10:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.1586, (float)28.98, (float)29.13, (float)28.98, "2023-05-11 10:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.14, (float)29.045, (float)29.045, (float)29.135, "2023-05-11 10:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.04, (float)29.08, (float)29.04, "2023-05-11 10:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.105, (float)29.07, (float)29.0994, (float)29.08, "2023-05-11 10:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.135, (float)29.09, (float)29.1308, (float)29.1, "2023-05-11 10:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.14, (float)29.055, (float)29.065, (float)29.13, "2023-05-11 10:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.05, (float)29.0585, (float)29.06, "2023-05-11 10:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.12, (float)29.05, (float)29.12, (float)29.06, "2023-05-11 10:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.13, (float)29.075, (float)29.09, (float)29.125, "2023-05-11 10:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.14, (float)29.09, (float)29.095, (float)29.095, "2023-05-11 10:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.09, (float)29.095, (float)29.095, "2023-05-11 10:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.08, (float)29.095, (float)29.095, "2023-05-11 10:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.1073, (float)29.08, (float)29.0827, (float)29.1, "2023-05-11 10:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.05, (float)29.0601, (float)29.083, "2023-05-11 10:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.065, (float)29.035, (float)29.045, (float)29.065, "2023-05-11 10:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)28.99, (float)29.0001, (float)29.048, "2023-05-11 10:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.98, (float)28.99, (float)29.0012, "2023-05-11 10:34:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.98, (float)28.985, (float)28.99, "2023-05-11 10:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.93, (float)28.93, (float)28.98, "2023-05-11 10:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.945, (float)28.92, (float)28.93, (float)28.935, "2023-05-11 10:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.91, (float)28.935, (float)28.93, "2023-05-11 10:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.9599, (float)28.92, (float)28.945, (float)28.94, "2023-05-11 10:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.945, (float)28.92, (float)28.9322, (float)28.935, "2023-05-11 10:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.9, (float)28.9, (float)28.935, "2023-05-11 10:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.9275, (float)28.89, (float)28.91, (float)28.905, "2023-05-11 10:42:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.88, (float)28.8801, (float)28.91, "2023-05-11 10:43:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.845, (float)28.875, (float)28.884, "2023-05-11 10:44:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.87, (float)28.91, (float)28.875, "2023-05-11 10:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.915, (float)28.925, (float)28.915, "2023-05-11 10:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.951, (float)28.92, (float)28.95, (float)28.925, "2023-05-11 10:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.975, (float)28.94, (float)28.97, (float)28.945, "2023-05-11 10:48:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.955, (float)28.9995, (float)28.965, "2023-05-11 10:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.98, (float)28.99, (float)29, "2023-05-11 10:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.005, (float)28.98, (float)29, (float)28.99, "2023-05-11 10:51:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.9701, (float)28.99, (float)29, "2023-05-11 10:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.9628, (float)28.975, (float)28.9872, "2023-05-11 10:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)28.97, (float)29.055, (float)28.975, "2023-05-11 10:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.075, (float)29.045, (float)29.065, (float)29.055, "2023-05-11 10:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.08, (float)29.04, (float)29.055, (float)29.06, "2023-05-11 10:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.085, (float)29.04, (float)29.075, (float)29.06, "2023-05-11 10:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.075, (float)29.095, (float)29.075, "2023-05-11 10:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.07, (float)29.07, (float)29.1, "2023-05-11 10:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.12, (float)29.075, (float)29.12, (float)29.075, "2023-05-11 11:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.13, (float)29.0901, (float)29.095, (float)29.12, "2023-05-11 11:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.06, (float)29.065, (float)29.1, "2023-05-11 11:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.085, (float)29.06, (float)29.0617, (float)29.0601, "2023-05-11 11:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.025, (float)29.065, (float)29.065, "2023-05-11 11:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.09, (float)29.06, (float)29.09, (float)29.065, "2023-05-11 11:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.095, (float)29.05, (float)29.055, (float)29.0875, "2023-05-11 11:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.055, (float)29.02, (float)29.0299, (float)29.05, "2023-05-11 11:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.0443, (float)29.01, (float)29.03, (float)29.025, "2023-05-11 11:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.02, (float)29.025, (float)29.035, "2023-05-11 11:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.045, (float)28.99, (float)28.9925, (float)29.02, "2023-05-11 11:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.0099, (float)28.98, (float)28.9914, (float)29.001, "2023-05-11 11:11:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.98, (float)28.98, (float)28.9918, "2023-05-11 11:12:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.945, (float)28.955, (float)28.985, "2023-05-11 11:13:00"));
            list_DataPoints.Add(new DataPoint((float)28.975, (float)28.9423, (float)28.95, (float)28.955, "2023-05-11 11:14:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.95, (float)28.96, (float)28.955, "2023-05-11 11:15:00"));
            list_DataPoints.Add(new DataPoint((float)28.975, (float)28.9313, (float)28.97, (float)28.963, "2023-05-11 11:16:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.95, (float)28.95, (float)28.965, "2023-05-11 11:17:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.945, (float)28.96, (float)28.9533, "2023-05-11 11:18:00"));
            list_DataPoints.Add(new DataPoint((float)28.965, (float)28.9401, (float)28.955, (float)28.96, "2023-05-11 11:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.9766, (float)28.945, (float)28.965, (float)28.96, "2023-05-11 11:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.985, (float)28.95, (float)28.975, (float)28.97, "2023-05-11 11:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.015, (float)28.975, (float)29.0062, (float)28.9792, "2023-05-11 11:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.005, (float)28.9701, (float)28.98, (float)29, "2023-05-11 11:23:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.97, (float)28.98, (float)28.98, "2023-05-11 11:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.9799, (float)28.95, (float)28.965, (float)28.9701, "2023-05-11 11:25:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.945, (float)28.965, (float)28.965, "2023-05-11 11:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.96, (float)29.0001, (float)28.96, "2023-05-11 11:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.0023, (float)29.045, (float)29.0023, "2023-05-11 11:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.04, (float)29.065, (float)29.05, "2023-05-11 11:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.05, (float)29.0647, (float)29.065, "2023-05-11 11:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.06, (float)29.095, (float)29.0696, "2023-05-11 11:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.11, (float)29.07, (float)29.08, (float)29.09, "2023-05-11 11:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.075, (float)29.06, (float)29.07, (float)29.07, "2023-05-11 11:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.05, (float)29.05, (float)29.0605, "2023-05-11 11:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.055, (float)29.02, (float)29.03, (float)29.055, "2023-05-11 11:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.02, (float)29.03, (float)29.03, "2023-05-11 11:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.035, (float)29.005, (float)29.025, (float)29.035, "2023-05-11 11:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29.005, (float)29.005, (float)29.025, "2023-05-11 11:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.015, (float)28.99, (float)28.995, (float)29.005, "2023-05-11 11:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.985, (float)29, (float)28.995, "2023-05-11 11:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29, (float)29.035, (float)29.01, "2023-05-11 11:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.03, (float)29.0475, (float)29.035, "2023-05-11 11:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.025, (float)29.055, (float)29.04, "2023-05-11 11:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.0799, (float)29.05, (float)29.0509, (float)29.055, "2023-05-11 11:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.01, (float)29.0128, (float)29.0501, "2023-05-11 11:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.95, (float)28.9599, (float)29.01, "2023-05-11 11:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.93, (float)28.93, (float)28.95, "2023-05-11 11:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.9454, (float)28.9, (float)28.905, (float)28.9326, "2023-05-11 11:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.9175, (float)28.9, (float)28.9058, (float)28.905, "2023-05-11 11:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.9397, (float)28.895, (float)28.9397, (float)28.9075, "2023-05-11 11:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.93, (float)28.95, (float)28.9399, "2023-05-11 11:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.9331, (float)28.9361, (float)28.9401, "2023-05-11 11:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.925, (float)28.96, (float)28.935, "2023-05-11 11:53:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.9599, (float)28.98, (float)28.96, "2023-05-11 11:54:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.96, (float)28.985, (float)28.9758, "2023-05-11 11:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.015, (float)28.97, (float)29.015, (float)28.985, "2023-05-11 11:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.025, (float)29.01, (float)29.015, (float)29.01, "2023-05-11 11:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.015, (float)29.03, (float)29.0162, "2023-05-11 11:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.03, (float)29.045, (float)29.03, "2023-05-11 11:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.035, (float)29.04, (float)29.045, "2023-05-11 12:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.045, (float)29.01, (float)29.025, (float)29.04, "2023-05-11 12:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.025, (float)28.9707, (float)28.975, (float)29.02, "2023-05-11 12:02:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.97, (float)29, (float)28.975, "2023-05-11 12:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.025, (float)28.99, (float)29.01, (float)28.995, "2023-05-11 12:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.0176, (float)29.0001, (float)29.015, (float)29.01, "2023-05-11 12:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29, (float)29.01, (float)29.015, "2023-05-11 12:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29.0187, (float)29.03, (float)29.0187, "2023-05-11 12:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.055, (float)29.03, (float)29.045, (float)29.03, "2023-05-11 12:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.04, (float)29.045, (float)29.05, "2023-05-11 12:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.02, (float)29.02, (float)29.04, "2023-05-11 12:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.055, (float)29.015, (float)29.055, (float)29.02, "2023-05-11 12:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.02, (float)29.025, (float)29.0501, "2023-05-11 12:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.015, (float)29.04, (float)29.025, "2023-05-11 12:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.045, (float)29.02, (float)29.03, (float)29.04, "2023-05-11 12:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.037, (float)29, (float)29.035, (float)29.025, "2023-05-11 12:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.045, (float)29.02, (float)29.03, (float)29.03, "2023-05-11 12:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29.02, (float)29.0225, (float)29.025, "2023-05-11 12:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29.005, (float)29.01, (float)29.0201, "2023-05-11 12:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.01, (float)29.04, (float)29.01, "2023-05-11 12:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.025, (float)29.035, (float)29.035, "2023-05-11 12:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.03, (float)29.05, (float)29.04, "2023-05-11 12:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.05, (float)29.055, (float)29.05, "2023-05-11 12:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.0301, (float)29.04, (float)29.06, "2023-05-11 12:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.0655, (float)29.035, (float)29.0417, (float)29.04, "2023-05-11 12:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.02, (float)29.0428, (float)29.04, "2023-05-11 12:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.045, (float)28.995, (float)28.9956, (float)29.045, "2023-05-11 12:26:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.96, (float)28.98, (float)28.995, "2023-05-11 12:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.97, (float)28.9775, (float)28.97, "2023-05-11 12:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.9709, (float)28.9857, (float)28.975, "2023-05-11 12:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.985, (float)29.015, (float)28.99, "2023-05-11 12:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.995, (float)29.0006, (float)29.02, "2023-05-11 12:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.99, (float)28.9925, (float)29, "2023-05-11 12:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.991, (float)29, (float)29, "2023-05-11 12:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.005, (float)28.97, (float)28.9762, (float)29, "2023-05-11 12:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.035, (float)28.98, (float)29.035, (float)28.98, "2023-05-11 12:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.055, (float)29.0201, (float)29.04, (float)29.04, "2023-05-11 12:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.04, (float)29.055, (float)29.04, "2023-05-11 12:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.025, (float)29.03, (float)29.055, "2023-05-11 12:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.02, (float)29.02, (float)29.0292, "2023-05-11 12:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.02, (float)29.035, (float)29.02, "2023-05-11 12:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.0499, (float)29.02, (float)29.0499, (float)29.03, "2023-05-11 12:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.08, (float)29.0401, (float)29.075, (float)29.0401, "2023-05-11 12:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.08, (float)29.055, (float)29.065, (float)29.075, "2023-05-11 12:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.09, (float)29.06, (float)29.085, (float)29.0668, "2023-05-11 12:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.1099, (float)29.08, (float)29.0995, (float)29.085, "2023-05-11 12:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.08, (float)29.085, (float)29.1, "2023-05-11 12:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.09, (float)29.075, (float)29.09, (float)29.085, "2023-05-11 12:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.083, (float)29.1, (float)29.09, "2023-05-11 12:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.0825, (float)29.0884, (float)29.1, "2023-05-11 12:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.0501, (float)29.0501, (float)29.0895, "2023-05-11 12:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.05, (float)29.0583, (float)29.06, "2023-05-11 12:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.05, (float)29.05, (float)29.0525, "2023-05-11 12:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.03, (float)29.03, (float)29.05, "2023-05-11 12:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.01, (float)29.035, (float)29.02, "2023-05-11 12:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.035, (float)29.05, (float)29.04, "2023-05-11 12:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.0575, (float)29.0336, (float)29.04, (float)29.05, "2023-05-11 12:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.02, (float)29.025, (float)29.04, "2023-05-11 12:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.02, (float)29.04, (float)29.025, "2023-05-11 12:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.02, (float)29.025, (float)29.04, "2023-05-11 12:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29.02, (float)29.03, (float)29.03, "2023-05-11 13:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29.02, (float)29.02, (float)29.025, "2023-05-11 13:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29, (float)29.015, (float)29.025, "2023-05-11 13:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)29, (float)29.005, (float)29.02, "2023-05-11 13:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.9825, (float)28.995, (float)29.01, "2023-05-11 13:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.98, (float)28.985, (float)29, "2023-05-11 13:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)28.985, (float)29.0091, (float)28.985, "2023-05-11 13:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.96, (float)28.9701, (float)29.01, "2023-05-11 13:07:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.96, (float)28.975, (float)28.975, "2023-05-11 13:08:00"));
            list_DataPoints.Add(new DataPoint((float)28.975, (float)28.94, (float)28.955, (float)28.975, "2023-05-11 13:09:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.95, (float)28.97, (float)28.95, "2023-05-11 13:10:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.96, (float)28.96, (float)28.9715, "2023-05-11 13:11:00"));
            list_DataPoints.Add(new DataPoint((float)28.975, (float)28.95, (float)28.9599, (float)28.965, "2023-05-11 13:12:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.935, (float)28.935, (float)28.955, "2023-05-11 13:13:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.93, (float)28.955, (float)28.9365, "2023-05-11 13:14:00"));
            list_DataPoints.Add(new DataPoint((float)28.9899, (float)28.95, (float)28.985, (float)28.955, "2023-05-11 13:15:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.97, (float)28.975, (float)28.99, "2023-05-11 13:16:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.96, (float)28.9657, (float)28.975, "2023-05-11 13:17:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.93, (float)28.94, (float)28.96, "2023-05-11 13:18:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.94, (float)28.955, (float)28.945, "2023-05-11 13:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.985, (float)28.96, (float)28.965, (float)28.96, "2023-05-11 13:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.965, (float)28.92, (float)28.94, (float)28.96, "2023-05-11 13:21:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.93, (float)28.93, (float)28.935, "2023-05-11 13:22:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.93, (float)28.945, (float)28.9397, "2023-05-11 13:23:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.91, (float)28.92, (float)28.95, "2023-05-11 13:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.915, (float)28.93, (float)28.92, "2023-05-11 13:25:00"));
            list_DataPoints.Add(new DataPoint((float)28.9378, (float)28.92, (float)28.93, (float)28.93, "2023-05-11 13:26:00"));
            list_DataPoints.Add(new DataPoint((float)28.945, (float)28.93, (float)28.935, (float)28.935, "2023-05-11 13:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.91, (float)28.92, (float)28.935, "2023-05-11 13:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.925, (float)28.9, (float)28.9101, (float)28.92, "2023-05-11 13:29:00"));
            list_DataPoints.Add(new DataPoint((float)28.915, (float)28.9, (float)28.905, (float)28.91, "2023-05-11 13:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.9, (float)28.905, (float)28.9015, "2023-05-11 13:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.89, (float)28.9, (float)28.905, "2023-05-11 13:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.915, (float)28.9, (float)28.915, (float)28.905, "2023-05-11 13:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.91, (float)28.9103, (float)28.92, "2023-05-11 13:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.918, (float)28.9, (float)28.915, (float)28.915, "2023-05-11 13:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.905, (float)28.91, (float)28.9197, "2023-05-11 13:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.88, (float)28.9, (float)28.9044, "2023-05-11 13:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.89, (float)28.92, (float)28.8915, "2023-05-11 13:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.91, (float)28.91, (float)28.9199, "2023-05-11 13:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.915, (float)28.86, (float)28.87, (float)28.91, "2023-05-11 13:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.875, (float)28.86, (float)28.875, (float)28.87, "2023-05-11 13:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.8799, (float)28.84, (float)28.855, (float)28.875, "2023-05-11 13:42:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.84, (float)28.845, (float)28.85, "2023-05-11 13:43:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.845, (float)28.855, (float)28.85, "2023-05-11 13:44:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.845, (float)28.865, (float)28.85, "2023-05-11 13:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.865, (float)28.82, (float)28.85, (float)28.86, "2023-05-11 13:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.855, (float)28.83, (float)28.835, (float)28.845, "2023-05-11 13:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.825, (float)28.855, (float)28.83, "2023-05-11 13:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.835, (float)28.852, (float)28.855, "2023-05-11 13:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.855, (float)28.83, (float)28.835, (float)28.855, "2023-05-11 13:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.825, (float)28.84, (float)28.835, "2023-05-11 13:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.83, (float)28.85, (float)28.84, "2023-05-11 13:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.848, (float)28.855, (float)28.85, "2023-05-11 13:53:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.85, (float)28.875, (float)28.8561, "2023-05-11 13:54:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.86, (float)28.875, (float)28.875, "2023-05-11 13:55:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.875, (float)28.88, (float)28.875, "2023-05-11 13:56:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.87, (float)28.875, (float)28.889, "2023-05-11 13:57:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.86, (float)28.875, (float)28.87, "2023-05-11 13:58:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.87, (float)28.89, (float)28.87, "2023-05-11 13:59:00"));
            list_DataPoints.Add(new DataPoint((float)28.9, (float)28.87, (float)28.875, (float)28.9, "2023-05-11 14:00:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.86, (float)28.88, (float)28.875, "2023-05-11 14:01:00"));
            list_DataPoints.Add(new DataPoint((float)28.8885, (float)28.87, (float)28.88, (float)28.885, "2023-05-11 14:02:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.87, (float)28.875, (float)28.875, "2023-05-11 14:03:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.865, (float)28.8724, (float)28.88, "2023-05-11 14:04:00"));
            list_DataPoints.Add(new DataPoint((float)28.875, (float)28.86, (float)28.865, (float)28.87, "2023-05-11 14:05:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.86, (float)28.86, (float)28.87, "2023-05-11 14:06:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.86, (float)28.865, (float)28.865, "2023-05-11 14:07:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.84, (float)28.87, (float)28.865, "2023-05-11 14:08:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.8699, (float)28.905, (float)28.8699, "2023-05-11 14:09:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.9, (float)28.905, (float)28.91, "2023-05-11 14:10:00"));
            list_DataPoints.Add(new DataPoint((float)28.915, (float)28.89, (float)28.895, (float)28.905, "2023-05-11 14:11:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.89, (float)28.9088, (float)28.895, "2023-05-11 14:12:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.89, (float)28.89, (float)28.9046, "2023-05-11 14:13:00"));
            list_DataPoints.Add(new DataPoint((float)28.9, (float)28.89, (float)28.8998, (float)28.895, "2023-05-11 14:14:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.89, (float)28.9, (float)28.9, "2023-05-11 14:15:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.88, (float)28.885, (float)28.905, "2023-05-11 14:16:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.88, (float)28.88, (float)28.885, "2023-05-11 14:17:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.86, (float)28.883, (float)28.88, "2023-05-11 14:18:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.8624, (float)28.885, (float)28.8801, "2023-05-11 14:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.87, (float)28.885, (float)28.885, "2023-05-11 14:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.875, (float)28.875, (float)28.89, "2023-05-11 14:21:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.87, (float)28.905, (float)28.875, "2023-05-11 14:22:00"));
            list_DataPoints.Add(new DataPoint((float)28.925, (float)28.89, (float)28.92, (float)28.9, "2023-05-11 14:23:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.92, (float)28.9264, (float)28.9278, "2023-05-11 14:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.92, (float)28.9292, (float)28.93, "2023-05-11 14:25:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.915, (float)28.945, (float)28.925, "2023-05-11 14:26:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.945, (float)28.965, (float)28.945, "2023-05-11 14:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.965, (float)28.94, (float)28.945, (float)28.96, "2023-05-11 14:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.94, (float)28.96, (float)28.945, "2023-05-11 14:29:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.93, (float)28.94, (float)28.97, "2023-05-11 14:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.93, (float)28.94, (float)28.935, "2023-05-11 14:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.93, (float)28.935, (float)28.935, "2023-05-11 14:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.92, (float)28.925, (float)28.94, "2023-05-11 14:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.945, (float)28.92, (float)28.93, (float)28.9201, "2023-05-11 14:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.91, (float)28.9127, (float)28.925, "2023-05-11 14:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.91, (float)28.915, (float)28.91, "2023-05-11 14:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.9199, (float)28.9301, (float)28.9199, "2023-05-11 14:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.93, (float)28.935, (float)28.935, "2023-05-11 14:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.925, (float)28.9501, (float)28.9341, "2023-05-11 14:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.95, (float)28.975, (float)28.955, "2023-05-11 14:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.975, (float)28.96, (float)28.965, (float)28.975, "2023-05-11 14:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.965, (float)28.9717, (float)28.97, "2023-05-11 14:42:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.97, (float)28.9801, (float)28.975, "2023-05-11 14:43:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.98, (float)28.995, (float)28.985, "2023-05-11 14:44:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.96, (float)28.965, (float)28.995, "2023-05-11 14:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.95, (float)28.955, (float)28.97, "2023-05-11 14:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.965, (float)28.95, (float)28.955, (float)28.965, "2023-05-11 14:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.95, (float)28.99, (float)28.9527, "2023-05-11 14:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.98, (float)29.01, (float)28.985, "2023-05-11 14:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29, (float)29.04, (float)29.005, "2023-05-11 14:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29, (float)29.0021, (float)29.04, "2023-05-11 14:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.985, (float)28.99, (float)29.005, "2023-05-11 14:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.97, (float)28.985, (float)28.99, "2023-05-11 14:53:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.98, (float)28.985, (float)28.99, "2023-05-11 14:54:00"));
            list_DataPoints.Add(new DataPoint((float)28.995, (float)28.96, (float)28.9942, (float)28.985, "2023-05-11 14:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.99, (float)28.995, (float)28.995, "2023-05-11 14:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.99, (float)29.005, (float)29, "2023-05-11 14:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.98, (float)29.005, (float)29.005, "2023-05-11 14:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.995, (float)29, (float)29.01, "2023-05-11 14:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.015, (float)28.9926, (float)29, (float)28.995, "2023-05-11 15:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.0077, (float)28.99, (float)28.9901, (float)29.0077, "2023-05-11 15:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.995, (float)29.02, (float)29, "2023-05-11 15:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)29.01, (float)29.01, (float)29.02, "2023-05-11 15:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29.01, (float)29.02, (float)29.02, "2023-05-11 15:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.98, (float)28.985, (float)29.0183, "2023-05-11 15:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.015, (float)28.98, (float)28.995, (float)28.98, "2023-05-11 15:06:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.99, (float)28.99, (float)29, "2023-05-11 15:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.025, (float)28.9925, (float)29.025, (float)28.995, "2023-05-11 15:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.02, (float)29.04, (float)29.03, "2023-05-11 15:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.038, (float)29.045, (float)29.04, "2023-05-11 15:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.03, (float)29.04, (float)29.05, "2023-05-11 15:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.03, (float)29.035, (float)29.035, "2023-05-11 15:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.045, (float)29.03, (float)29.035, (float)29.03, "2023-05-11 15:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29, (float)29.005, (float)29.03, "2023-05-11 15:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.005, (float)28.98, (float)28.985, (float)29.005, "2023-05-11 15:15:00"));
            list_DataPoints.Add(new DataPoint((float)28.9899, (float)28.96, (float)28.96, (float)28.985, "2023-05-11 15:16:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.95, (float)28.955, (float)28.96, "2023-05-11 15:17:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.9501, (float)28.975, (float)28.96, "2023-05-11 15:18:00"));
            list_DataPoints.Add(new DataPoint((float)28.9799, (float)28.955, (float)28.955, (float)28.975, "2023-05-11 15:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.9557, (float)28.94, (float)28.945, (float)28.955, "2023-05-11 15:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.945, (float)28.965, (float)28.945, "2023-05-11 15:21:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.96, (float)28.98, (float)28.97, "2023-05-11 15:22:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.955, (float)28.975, (float)28.975, "2023-05-11 15:23:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.97, (float)28.985, (float)28.975, "2023-05-11 15:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.96, (float)28.975, (float)28.985, "2023-05-11 15:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.005, (float)28.975, (float)29.005, (float)28.98, "2023-05-11 15:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.98, (float)28.985, (float)29.01, "2023-05-11 15:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.98, (float)28.98, (float)28.985, "2023-05-11 15:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.985, (float)28.965, (float)28.975, (float)28.985, "2023-05-11 15:29:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.96, (float)28.965, (float)28.97, "2023-05-11 15:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.965, (float)28.975, (float)28.97, "2023-05-11 15:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.96, (float)28.9809, (float)28.9725, "2023-05-11 15:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.995, (float)28.97, (float)28.97, (float)28.99, "2023-05-11 15:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.975, (float)28.975, (float)28.975, "2023-05-11 15:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.985, (float)28.96, (float)28.975, (float)28.98, "2023-05-11 15:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.975, (float)28.94, (float)28.95, (float)28.975, "2023-05-11 15:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.93, (float)28.945, (float)28.945, "2023-05-11 15:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.94, (float)28.945, (float)28.945, "2023-05-11 15:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.94, (float)28.94, (float)28.94, "2023-05-11 15:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.945, (float)28.92, (float)28.925, (float)28.945, "2023-05-11 15:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.92, (float)28.9334, (float)28.925, "2023-05-11 15:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.935, (float)28.92, (float)28.9215, (float)28.935, "2023-05-11 15:42:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.92, (float)28.925, (float)28.925, "2023-05-11 15:43:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.9146, (float)28.94, (float)28.92, "2023-05-11 15:44:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.91, (float)28.925, (float)28.94, "2023-05-11 15:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.92, (float)28.935, (float)28.925, "2023-05-11 15:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.92, (float)28.94, (float)28.935, "2023-05-11 15:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.945, (float)28.92, (float)28.9255, (float)28.945, "2023-05-11 15:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.92, (float)28.92, (float)28.93, "2023-05-11 15:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.91, (float)28.91, (float)28.92, "2023-05-11 15:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.9, (float)28.915, (float)28.91, "2023-05-11 15:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.89, (float)28.895, (float)28.915, "2023-05-11 15:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.895, (float)28.87, (float)28.875, (float)28.89, "2023-05-11 15:53:00"));
            list_DataPoints.Add(new DataPoint((float)28.875, (float)28.83, (float)28.865, (float)28.875, "2023-05-11 15:54:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.845, (float)28.87, (float)28.87, "2023-05-11 15:55:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.87, (float)28.875, (float)28.87, "2023-05-11 15:56:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.84, (float)28.845, (float)28.875, "2023-05-11 15:57:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.83, (float)28.8399, (float)28.84, "2023-05-11 15:58:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.83, (float)28.87, (float)28.835, "2023-05-11 15:59:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.84, (float)28.85, (float)28.86, "2023-05-11 16:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)28.83, (float)28.83, (float)29.025, "2023-05-12 09:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.905, (float)28.83, (float)28.875, (float)28.83, "2023-05-12 09:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.9399, (float)28.875, (float)28.9337, (float)28.875, "2023-05-12 09:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.9, (float)28.943, (float)28.935, "2023-05-12 09:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.93, (float)28.94, (float)28.95, "2023-05-12 09:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.93, (float)28.9718, (float)28.945, "2023-05-12 09:35:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.94, (float)28.98, (float)28.97, "2023-05-12 09:36:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.96, (float)28.985, (float)28.98, "2023-05-12 09:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.995, (float)28.89, (float)28.91, (float)28.985, "2023-05-12 09:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.935, (float)28.9, (float)28.905, (float)28.91, "2023-05-12 09:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.88, (float)28.915, (float)28.905, "2023-05-12 09:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.91, (float)28.935, (float)28.915, "2023-05-12 09:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.9, (float)28.905, (float)28.93, "2023-05-12 09:42:00"));
            list_DataPoints.Add(new DataPoint((float)28.9475, (float)28.9014, (float)28.9475, (float)28.91, "2023-05-12 09:43:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.925, (float)28.925, (float)28.95, "2023-05-12 09:44:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.88, (float)28.9037, (float)28.925, "2023-05-12 09:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.915, (float)28.8838, (float)28.8839, (float)28.9, "2023-05-12 09:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.935, (float)28.88, (float)28.93, (float)28.88, "2023-05-12 09:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.935, (float)28.9, (float)28.92, (float)28.93, "2023-05-12 09:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.9288, (float)28.895, (float)28.915, (float)28.9175, "2023-05-12 09:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.945, (float)28.9, (float)28.925, (float)28.915, "2023-05-12 09:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.91, (float)28.92, (float)28.93, "2023-05-12 09:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.955, (float)28.905, (float)28.942, (float)28.91, "2023-05-12 09:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.925, (float)28.95, (float)28.945, "2023-05-12 09:53:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.9401, (float)28.992, (float)28.945, "2023-05-12 09:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.98, (float)28.985, (float)28.995, "2023-05-12 09:55:00"));
            list_DataPoints.Add(new DataPoint((float)28.9982, (float)28.96, (float)28.965, (float)28.99, "2023-05-12 09:56:00"));
            list_DataPoints.Add(new DataPoint((float)28.965, (float)28.915, (float)28.93, (float)28.965, "2023-05-12 09:57:00"));
            list_DataPoints.Add(new DataPoint((float)28.965, (float)28.93, (float)28.955, (float)28.935, "2023-05-12 09:58:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.92, (float)28.935, (float)28.95, "2023-05-12 09:59:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.91, (float)28.935, (float)28.93, "2023-05-12 10:00:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.8631, (float)28.88, (float)28.93, "2023-05-12 10:01:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.88, (float)28.91, (float)28.88, "2023-05-12 10:02:00"));
            list_DataPoints.Add(new DataPoint((float)28.925, (float)28.84, (float)28.84, (float)28.905, "2023-05-12 10:03:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.82, (float)28.835, (float)28.845, "2023-05-12 10:04:00"));
            list_DataPoints.Add(new DataPoint((float)28.8599, (float)28.81, (float)28.835, (float)28.83, "2023-05-12 10:05:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.8, (float)28.8093, (float)28.83, "2023-05-12 10:06:00"));
            list_DataPoints.Add(new DataPoint((float)28.8251, (float)28.79, (float)28.824, (float)28.805, "2023-05-12 10:07:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.8215, (float)28.84, (float)28.825, "2023-05-12 10:08:00"));
            list_DataPoints.Add(new DataPoint((float)28.865, (float)28.81, (float)28.85, (float)28.84, "2023-05-12 10:09:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.845, (float)28.87, (float)28.85, "2023-05-12 10:10:00"));
            list_DataPoints.Add(new DataPoint((float)28.875, (float)28.85, (float)28.855, (float)28.87, "2023-05-12 10:11:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.84, (float)28.855, (float)28.855, "2023-05-12 10:12:00"));
            list_DataPoints.Add(new DataPoint((float)28.875, (float)28.855, (float)28.87, (float)28.855, "2023-05-12 10:13:00"));
            list_DataPoints.Add(new DataPoint((float)28.875, (float)28.85, (float)28.855, (float)28.865, "2023-05-12 10:14:00"));
            list_DataPoints.Add(new DataPoint((float)28.865, (float)28.84, (float)28.86, (float)28.855, "2023-05-12 10:15:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.84, (float)28.8439, (float)28.86, "2023-05-12 10:16:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.84, (float)28.87, (float)28.8456, "2023-05-12 10:17:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.845, (float)28.85, (float)28.87, "2023-05-12 10:18:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.84, (float)28.865, (float)28.8467, "2023-05-12 10:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.83, (float)28.83, (float)28.87, "2023-05-12 10:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.82, (float)28.86, (float)28.835, "2023-05-12 10:21:00"));
            list_DataPoints.Add(new DataPoint((float)28.865, (float)28.85, (float)28.86, (float)28.8599, "2023-05-12 10:22:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.84, (float)28.84, (float)28.86, "2023-05-12 10:23:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.835, (float)28.87, (float)28.845, "2023-05-12 10:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.86, (float)28.865, (float)28.87, "2023-05-12 10:25:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.86, (float)28.905, (float)28.865, "2023-05-12 10:26:00"));
            list_DataPoints.Add(new DataPoint((float)28.905, (float)28.845, (float)28.8564, (float)28.905, "2023-05-12 10:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.845, (float)28.855, (float)28.86, "2023-05-12 10:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.83, (float)28.85, (float)28.8501, "2023-05-12 10:29:00"));
            list_DataPoints.Add(new DataPoint((float)28.845, (float)28.8, (float)28.805, (float)28.84, "2023-05-12 10:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.815, (float)28.82, (float)28.815, "2023-05-12 10:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.815, (float)28.78, (float)28.8, (float)28.81, "2023-05-12 10:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.805, (float)28.78, (float)28.785, (float)28.8, "2023-05-12 10:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.75, (float)28.7502, (float)28.7899, "2023-05-12 10:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.75, (float)28.785, (float)28.75, "2023-05-12 10:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.7972, (float)28.76, (float)28.77, (float)28.7872, "2023-05-12 10:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.755, (float)28.77, (float)28.765, "2023-05-12 10:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.78, (float)28.75, (float)28.76, (float)28.7767, "2023-05-12 10:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.775, (float)28.7501, (float)28.775, (float)28.76, "2023-05-12 10:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.775, (float)28.7875, (float)28.78, "2023-05-12 10:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.78, (float)28.785, (float)28.79, "2023-05-12 10:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.815, (float)28.785, (float)28.805, (float)28.785, "2023-05-12 10:42:00"));
            list_DataPoints.Add(new DataPoint((float)28.805, (float)28.78, (float)28.79, (float)28.805, "2023-05-12 10:43:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.765, (float)28.7901, (float)28.79, "2023-05-12 10:44:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.78, (float)28.8, (float)28.795, "2023-05-12 10:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.815, (float)28.765, (float)28.815, (float)28.795, "2023-05-12 10:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.795, (float)28.83, (float)28.815, "2023-05-12 10:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.83, (float)28.865, (float)28.83, "2023-05-12 10:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.885, (float)28.85, (float)28.875, (float)28.87, "2023-05-12 10:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.85, (float)28.85, (float)28.875, "2023-05-12 10:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.835, (float)28.8582, (float)28.855, "2023-05-12 10:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.835, (float)28.84, (float)28.855, "2023-05-12 10:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.845, (float)28.8227, (float)28.84, (float)28.845, "2023-05-12 10:53:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.83, (float)28.875, (float)28.8371, "2023-05-12 10:54:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.8701, (float)28.91, (float)28.875, "2023-05-12 10:55:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.9067, (float)28.915, (float)28.91, "2023-05-12 10:56:00"));
            list_DataPoints.Add(new DataPoint((float)28.925, (float)28.9, (float)28.92, (float)28.91, "2023-05-12 10:57:00"));
            list_DataPoints.Add(new DataPoint((float)28.9275, (float)28.84, (float)28.86, (float)28.92, "2023-05-12 10:58:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.855, (float)28.915, (float)28.87, "2023-05-12 10:59:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.9, (float)28.915, (float)28.915, "2023-05-12 11:00:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.92, (float)28.93, (float)28.92, "2023-05-12 11:01:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.92, (float)28.93, (float)28.935, "2023-05-12 11:02:00"));
            list_DataPoints.Add(new DataPoint((float)28.935, (float)28.92, (float)28.925, (float)28.925, "2023-05-12 11:03:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.9212, (float)28.945, (float)28.9212, "2023-05-12 11:04:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.935, (float)28.989, (float)28.9458, "2023-05-12 11:05:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.97, (float)28.99, (float)28.985, "2023-05-12 11:06:00"));
            list_DataPoints.Add(new DataPoint((float)28.995, (float)28.98, (float)28.99, (float)28.99, "2023-05-12 11:07:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.97, (float)28.97, (float)28.9869, "2023-05-12 11:08:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.945, (float)28.98, (float)28.965, "2023-05-12 11:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.038, (float)28.97, (float)29.038, (float)28.975, "2023-05-12 11:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.01, (float)29.015, (float)29.03, "2023-05-12 11:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.985, (float)28.99, (float)29.02, "2023-05-12 11:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.99, (float)28.995, (float)28.99, "2023-05-12 11:13:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.9523, (float)28.98, (float)28.995, "2023-05-12 11:14:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.9632, (float)28.97, (float)28.975, "2023-05-12 11:15:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.9543, (float)28.995, (float)28.97, "2023-05-12 11:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.99, (float)28.995, (float)28.99, "2023-05-12 11:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.005, (float)28.9801, (float)28.9908, (float)28.995, "2023-05-12 11:18:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.98, (float)28.985, (float)28.99, "2023-05-12 11:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.98, (float)29.015, (float)28.985, "2023-05-12 11:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.025, (float)28.995, (float)29.015, (float)29.018, "2023-05-12 11:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29.0101, (float)29.025, (float)29.0218, "2023-05-12 11:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)28.98, (float)28.985, (float)29.025, "2023-05-12 11:23:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.95, (float)28.955, (float)28.9826, "2023-05-12 11:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.9683, (float)28.95, (float)28.96, (float)28.955, "2023-05-12 11:25:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.95, (float)28.96, (float)28.955, "2023-05-12 11:26:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.94, (float)28.9468, (float)28.955, "2023-05-12 11:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.9463, (float)28.93, (float)28.9385, (float)28.9463, "2023-05-12 11:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.91, (float)28.9399, (float)28.93, "2023-05-12 11:29:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.93, (float)28.935, (float)28.93, "2023-05-12 11:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.9331, (float)28.9399, (float)28.94, "2023-05-12 11:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.91, (float)28.911, (float)28.935, "2023-05-12 11:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.905, (float)28.91, (float)28.9175, "2023-05-12 11:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.905, (float)28.915, (float)28.905, "2023-05-12 11:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.91, (float)28.925, (float)28.915, "2023-05-12 11:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.92, (float)28.925, (float)28.9225, "2023-05-12 11:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.905, (float)28.9159, (float)28.92, "2023-05-12 11:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.9289, (float)28.905, (float)28.9279, (float)28.915, "2023-05-12 11:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.91, (float)28.945, (float)28.925, "2023-05-12 11:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.935, (float)28.9799, (float)28.94, "2023-05-12 11:40:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.98, (float)28.995, (float)28.98, "2023-05-12 11:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.035, (float)28.9922, (float)29.035, (float)28.9996, "2023-05-12 11:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.0301, (float)29.065, (float)29.035, "2023-05-12 11:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.09, (float)29.06, (float)29.0601, (float)29.065, "2023-05-12 11:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.1, (float)29.05, (float)29.0832, (float)29.065, "2023-05-12 11:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.08, (float)29.02, (float)29.03, (float)29.08, "2023-05-12 11:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)28.99, (float)28.9942, (float)29.03, "2023-05-12 11:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)28.99, (float)29.02, (float)28.999, "2023-05-12 11:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.055, (float)29.02, (float)29.04, (float)29.03, "2023-05-12 11:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.04, (float)29.045, (float)29.045, "2023-05-12 11:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.045, (float)29.03, (float)29.04, (float)29.04, "2023-05-12 11:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.0326, (float)29.045, (float)29.04, "2023-05-12 11:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.0301, (float)29.045, (float)29.045, "2023-05-12 11:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.03, (float)29.035, (float)29.05, "2023-05-12 11:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.045, (float)29.01, (float)29.02, (float)29.04, "2023-05-12 11:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.0101, (float)29.04, (float)29.0201, "2023-05-12 11:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.03, (float)29.05, (float)29.04, "2023-05-12 11:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.04, (float)29.055, (float)29.04, "2023-05-12 11:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.07, (float)29.05, (float)29.055, (float)29.06, "2023-05-12 11:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.045, (float)29.045, (float)29.05, "2023-05-12 12:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.02, (float)29.02, (float)29.04, "2023-05-12 12:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29, (float)29, (float)29.025, "2023-05-12 12:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.0012, (float)28.97, (float)28.975, (float)29.0012, "2023-05-12 12:03:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.97, (float)28.975, (float)28.97, "2023-05-12 12:04:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.97, (float)28.975, (float)28.98, "2023-05-12 12:05:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.95, (float)28.955, (float)28.975, "2023-05-12 12:06:00"));
            list_DataPoints.Add(new DataPoint((float)28.955, (float)28.93, (float)28.935, (float)28.955, "2023-05-12 12:07:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.93, (float)28.945, (float)28.93, "2023-05-12 12:08:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.9424, (float)28.9429, (float)28.945, "2023-05-12 12:09:00"));
            list_DataPoints.Add(new DataPoint((float)28.955, (float)28.94, (float)28.94, (float)28.945, "2023-05-12 12:10:00"));
            list_DataPoints.Add(new DataPoint((float)28.9483, (float)28.925, (float)28.945, (float)28.94, "2023-05-12 12:11:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.93, (float)28.945, (float)28.94, "2023-05-12 12:12:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.94, (float)28.97, (float)28.9401, "2023-05-12 12:13:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.96, (float)28.965, (float)28.97, "2023-05-12 12:14:00"));
            list_DataPoints.Add(new DataPoint((float)28.9899, (float)28.9501, (float)28.985, (float)28.96, "2023-05-12 12:15:00"));
            list_DataPoints.Add(new DataPoint((float)28.9999, (float)28.97, (float)28.975, (float)28.99, "2023-05-12 12:16:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.97, (float)28.985, (float)28.975, "2023-05-12 12:17:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.95, (float)28.95, (float)28.98, "2023-05-12 12:18:00"));
            list_DataPoints.Add(new DataPoint((float)28.975, (float)28.95, (float)28.96, (float)28.95, "2023-05-12 12:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.9301, (float)28.935, (float)28.96, "2023-05-12 12:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.9238, (float)28.9376, (float)28.935, "2023-05-12 12:21:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.92, (float)28.925, (float)28.935, "2023-05-12 12:22:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.91, (float)28.91, (float)28.928, "2023-05-12 12:23:00"));
            list_DataPoints.Add(new DataPoint((float)28.9299, (float)28.905, (float)28.915, (float)28.91, "2023-05-12 12:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.9126, (float)28.925, (float)28.915, "2023-05-12 12:25:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.92, (float)28.955, (float)28.92, "2023-05-12 12:26:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.9318, (float)28.9439, (float)28.96, "2023-05-12 12:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.935, (float)28.935, (float)28.94, "2023-05-12 12:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.9101, (float)28.915, (float)28.93, "2023-05-12 12:29:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.91, (float)28.925, (float)28.915, "2023-05-12 12:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.915, (float)28.925, (float)28.9231, "2023-05-12 12:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.925, (float)28.905, (float)28.9109, (float)28.92, "2023-05-12 12:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.9124, (float)28.925, (float)28.92, "2023-05-12 12:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.9376, (float)28.92, (float)28.925, (float)28.925, "2023-05-12 12:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.925, (float)28.925, (float)28.925, "2023-05-12 12:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.925, (float)28.91, (float)28.91, (float)28.925, "2023-05-12 12:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.9156, (float)28.87, (float)28.87, (float)28.91, "2023-05-12 12:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.895, (float)28.87, (float)28.875, (float)28.88, "2023-05-12 12:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.8997, (float)28.8749, (float)28.8926, (float)28.875, "2023-05-12 12:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.9, (float)28.865, (float)28.87, (float)28.9, "2023-05-12 12:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.855, (float)28.86, (float)28.8699, "2023-05-12 12:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.865, (float)28.845, (float)28.855, (float)28.865, "2023-05-12 12:42:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.83, (float)28.8425, (float)28.85, "2023-05-12 12:43:00"));
            list_DataPoints.Add(new DataPoint((float)28.855, (float)28.84, (float)28.85, (float)28.85, "2023-05-12 12:44:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.8201, (float)28.83, (float)28.85, "2023-05-12 12:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.825, (float)28.83, (float)28.825, "2023-05-12 12:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.82, (float)28.82, (float)28.8274, "2023-05-12 12:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.81, (float)28.8199, (float)28.82, "2023-05-12 12:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.8, (float)28.815, (float)28.815, "2023-05-12 12:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.81, (float)28.81, (float)28.815, "2023-05-12 12:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.81, (float)28.825, (float)28.81, "2023-05-12 12:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.805, (float)28.81, (float)28.8299, "2023-05-12 12:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.8089, (float)28.79, (float)28.795, (float)28.8089, "2023-05-12 12:53:00"));
            list_DataPoints.Add(new DataPoint((float)28.805, (float)28.78, (float)28.79, (float)28.79, "2023-05-12 12:54:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.785, (float)28.795, (float)28.785, "2023-05-12 12:55:00"));
            list_DataPoints.Add(new DataPoint((float)28.8253, (float)28.79, (float)28.8117, (float)28.795, "2023-05-12 12:56:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.8, (float)28.8001, (float)28.815, "2023-05-12 12:57:00"));
            list_DataPoints.Add(new DataPoint((float)28.8299, (float)28.8028, (float)28.8211, (float)28.805, "2023-05-12 12:58:00"));
            list_DataPoints.Add(new DataPoint((float)28.825, (float)28.81, (float)28.81, (float)28.825, "2023-05-12 12:59:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.8, (float)28.8095, (float)28.81, "2023-05-12 13:00:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.8, (float)28.8, (float)28.8, "2023-05-12 13:01:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.8, (float)28.81, (float)28.8074, "2023-05-12 13:02:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.8, (float)28.815, (float)28.81, "2023-05-12 13:03:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.81, (float)28.81, (float)28.815, "2023-05-12 13:04:00"));
            list_DataPoints.Add(new DataPoint((float)28.835, (float)28.8, (float)28.825, (float)28.815, "2023-05-12 13:05:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.81, (float)28.8233, (float)28.825, "2023-05-12 13:06:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.82, (float)28.84, (float)28.825, "2023-05-12 13:07:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.825, (float)28.8314, (float)28.84, "2023-05-12 13:08:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.82, (float)28.84, (float)28.8306, "2023-05-12 13:09:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.815, (float)28.83, (float)28.84, "2023-05-12 13:10:00"));
            list_DataPoints.Add(new DataPoint((float)28.8499, (float)28.8201, (float)28.845, (float)28.827, "2023-05-12 13:11:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.835, (float)28.835, (float)28.84, "2023-05-12 13:12:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.8301, (float)28.86, (float)28.8334, "2023-05-12 13:13:00"));
            list_DataPoints.Add(new DataPoint((float)28.855, (float)28.835, (float)28.835, (float)28.855, "2023-05-12 13:14:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.83, (float)28.845, (float)28.84, "2023-05-12 13:15:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.84, (float)28.844, (float)28.85, "2023-05-12 13:16:00"));
            list_DataPoints.Add(new DataPoint((float)28.845, (float)28.83, (float)28.835, (float)28.845, "2023-05-12 13:17:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.83, (float)28.835, (float)28.835, "2023-05-12 13:18:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.83, (float)28.835, (float)28.835, "2023-05-12 13:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.835, (float)28.82, (float)28.8201, (float)28.835, "2023-05-12 13:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.82, (float)28.845, (float)28.825, "2023-05-12 13:21:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.84, (float)28.85, (float)28.845, "2023-05-12 13:22:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.85, (float)28.88, (float)28.85, "2023-05-12 13:23:00"));
            list_DataPoints.Add(new DataPoint((float)28.895, (float)28.87, (float)28.8901, (float)28.8787, "2023-05-12 13:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.87, (float)28.87, (float)28.895, "2023-05-12 13:25:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.86, (float)28.865, (float)28.875, "2023-05-12 13:26:00"));
            list_DataPoints.Add(new DataPoint((float)28.8799, (float)28.8622, (float)28.865, (float)28.8622, "2023-05-12 13:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.865, (float)28.872, (float)28.865, "2023-05-12 13:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.875, (float)28.86, (float)28.8637, (float)28.87, "2023-05-12 13:29:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.86, (float)28.88, (float)28.865, "2023-05-12 13:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.9, (float)28.875, (float)28.895, (float)28.88, "2023-05-12 13:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.895, (float)28.87, (float)28.87, (float)28.895, "2023-05-12 13:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.885, (float)28.87, (float)28.875, (float)28.871, "2023-05-12 13:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.8768, (float)28.85, (float)28.855, (float)28.8768, "2023-05-12 13:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.84, (float)28.855, (float)28.855, "2023-05-12 13:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.84, (float)28.845, (float)28.855, "2023-05-12 13:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.83, (float)28.8402, (float)28.845, "2023-05-12 13:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.84, (float)28.87, (float)28.84, "2023-05-12 13:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.8425, (float)28.845, (float)28.87, "2023-05-12 13:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.83, (float)28.865, (float)28.845, "2023-05-12 13:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.885, (float)28.865, (float)28.885, (float)28.865, "2023-05-12 13:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.875, (float)28.875, (float)28.8894, "2023-05-12 13:42:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.87, (float)28.875, (float)28.87, "2023-05-12 13:43:00"));
            list_DataPoints.Add(new DataPoint((float)28.9, (float)28.87, (float)28.895, (float)28.87, "2023-05-12 13:44:00"));
            list_DataPoints.Add(new DataPoint((float)28.895, (float)28.8646, (float)28.8646, (float)28.895, "2023-05-12 13:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.8678, (float)28.845, (float)28.85, (float)28.865, "2023-05-12 13:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.855, (float)28.835, (float)28.835, (float)28.85, "2023-05-12 13:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.83, (float)28.83, (float)28.835, "2023-05-12 13:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.8456, (float)28.83, (float)28.84, (float)28.835, "2023-05-12 13:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.845, (float)28.83, (float)28.835, (float)28.845, "2023-05-12 13:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.8125, (float)28.8125, (float)28.83, "2023-05-12 13:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.825, (float)28.8, (float)28.8132, (float)28.8176, "2023-05-12 13:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.8209, (float)28.805, (float)28.81, (float)28.815, "2023-05-12 13:53:00"));
            list_DataPoints.Add(new DataPoint((float)28.815, (float)28.8, (float)28.8, (float)28.805, "2023-05-12 13:54:00"));
            list_DataPoints.Add(new DataPoint((float)28.81, (float)28.78, (float)28.7801, (float)28.8, "2023-05-12 13:55:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.78, (float)28.79, (float)28.79, "2023-05-12 13:56:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.78, (float)28.795, (float)28.785, "2023-05-12 13:57:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.78, (float)28.785, (float)28.795, "2023-05-12 13:58:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.78, (float)28.795, (float)28.785, "2023-05-12 13:59:00"));
            list_DataPoints.Add(new DataPoint((float)28.795, (float)28.76, (float)28.765, (float)28.79, "2023-05-12 14:00:00"));
            list_DataPoints.Add(new DataPoint((float)28.78, (float)28.76, (float)28.77, (float)28.76, "2023-05-12 14:01:00"));
            list_DataPoints.Add(new DataPoint((float)28.78, (float)28.7617, (float)28.7727, (float)28.7799, "2023-05-12 14:02:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.77, (float)28.775, (float)28.775, "2023-05-12 14:03:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.7715, (float)28.785, (float)28.7715, "2023-05-12 14:04:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.775, (float)28.795, (float)28.78, "2023-05-12 14:05:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.785, (float)28.7927, (float)28.795, "2023-05-12 14:06:00"));
            list_DataPoints.Add(new DataPoint((float)28.805, (float)28.79, (float)28.805, (float)28.795, "2023-05-12 14:07:00"));
            list_DataPoints.Add(new DataPoint((float)28.805, (float)28.78, (float)28.79, (float)28.8, "2023-05-12 14:08:00"));
            list_DataPoints.Add(new DataPoint((float)28.7999, (float)28.78, (float)28.7915, (float)28.79, "2023-05-12 14:09:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.795, (float)28.8161, (float)28.795, "2023-05-12 14:10:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.81, (float)28.82, (float)28.82, "2023-05-12 14:11:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.79, (float)28.79, (float)28.82, "2023-05-12 14:12:00"));
            list_DataPoints.Add(new DataPoint((float)28.7925, (float)28.78, (float)28.78, (float)28.79, "2023-05-12 14:13:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.78, (float)28.7848, (float)28.785, "2023-05-12 14:14:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.77, (float)28.78, (float)28.78, "2023-05-12 14:15:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.765, (float)28.785, (float)28.775, "2023-05-12 14:16:00"));
            list_DataPoints.Add(new DataPoint((float)28.81, (float)28.78, (float)28.805, (float)28.785, "2023-05-12 14:17:00"));
            list_DataPoints.Add(new DataPoint((float)28.81, (float)28.79, (float)28.81, (float)28.805, "2023-05-12 14:18:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.8, (float)28.82, (float)28.805, "2023-05-12 14:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.815, (float)28.785, (float)28.785, (float)28.81, "2023-05-12 14:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.78, (float)28.785, (float)28.7872, "2023-05-12 14:21:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.78, (float)28.795, (float)28.78, "2023-05-12 14:22:00"));
            list_DataPoints.Add(new DataPoint((float)28.7999, (float)28.775, (float)28.775, (float)28.79, "2023-05-12 14:23:00"));
            list_DataPoints.Add(new DataPoint((float)28.78, (float)28.77, (float)28.7765, (float)28.78, "2023-05-12 14:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.785, (float)28.77, (float)28.78, (float)28.775, "2023-05-12 14:25:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.775, (float)28.78, (float)28.785, "2023-05-12 14:26:00"));
            list_DataPoints.Add(new DataPoint((float)28.799, (float)28.78, (float)28.795, (float)28.785, "2023-05-12 14:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.81, (float)28.79, (float)28.8001, (float)28.795, "2023-05-12 14:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.8185, (float)28.77, (float)28.775, (float)28.805, "2023-05-12 14:29:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.78, (float)28.78, (float)28.78, "2023-05-12 14:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.78, (float)28.76, (float)28.76, (float)28.78, "2023-05-12 14:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.76, (float)28.7806, (float)28.765, "2023-05-12 14:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.7636, (float)28.775, (float)28.7801, "2023-05-12 14:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.785, (float)28.775, (float)28.78, (float)28.78, "2023-05-12 14:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.76, (float)28.77, (float)28.79, "2023-05-12 14:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.775, (float)28.76, (float)28.7627, (float)28.77, "2023-05-12 14:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.77, (float)28.76, (float)28.7619, (float)28.76, "2023-05-12 14:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.77, (float)28.75, (float)28.755, (float)28.7623, "2023-05-12 14:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.76, (float)28.75, (float)28.755, (float)28.755, "2023-05-12 14:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.76, (float)28.74, (float)28.74, (float)28.755, "2023-05-12 14:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.74, (float)28.72, (float)28.7299, (float)28.74, "2023-05-12 14:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.73, (float)28.72, (float)28.73, (float)28.725, "2023-05-12 14:42:00"));
            list_DataPoints.Add(new DataPoint((float)28.75, (float)28.73, (float)28.7499, (float)28.73, "2023-05-12 14:43:00"));
            list_DataPoints.Add(new DataPoint((float)28.75, (float)28.73, (float)28.74, (float)28.745, "2023-05-12 14:44:00"));
            list_DataPoints.Add(new DataPoint((float)28.75, (float)28.73, (float)28.75, (float)28.735, "2023-05-12 14:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.76, (float)28.7405, (float)28.75, (float)28.75, "2023-05-12 14:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.75, (float)28.735, (float)28.745, (float)28.746, "2023-05-12 14:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.76, (float)28.745, (float)28.755, (float)28.745, "2023-05-12 14:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.77, (float)28.74, (float)28.765, (float)28.755, "2023-05-12 14:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.79, (float)28.76, (float)28.785, (float)28.76, "2023-05-12 14:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.7958, (float)28.78, (float)28.79, (float)28.785, "2023-05-12 14:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.78, (float)28.8, (float)28.79, "2023-05-12 14:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.8, (float)28.79, (float)28.795, (float)28.795, "2023-05-12 14:53:00"));
            list_DataPoints.Add(new DataPoint((float)28.82, (float)28.79, (float)28.8136, (float)28.8, "2023-05-12 14:54:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.815, (float)28.825, (float)28.815, "2023-05-12 14:55:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.8, (float)28.805, (float)28.83, "2023-05-12 14:56:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.805, (float)28.825, (float)28.81, "2023-05-12 14:57:00"));
            list_DataPoints.Add(new DataPoint((float)28.835, (float)28.8027, (float)28.81, (float)28.825, "2023-05-12 14:58:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.805, (float)28.825, (float)28.805, "2023-05-12 14:59:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.83, (float)28.835, (float)28.83, "2023-05-12 15:00:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.8, (float)28.81, (float)28.83, "2023-05-12 15:01:00"));
            list_DataPoints.Add(new DataPoint((float)28.845, (float)28.805, (float)28.835, (float)28.81, "2023-05-12 15:02:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.825, (float)28.835, (float)28.835, "2023-05-12 15:03:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.805, (float)28.82, (float)28.835, "2023-05-12 15:04:00"));
            list_DataPoints.Add(new DataPoint((float)28.81, (float)28.8, (float)28.805, (float)28.81, "2023-05-12 15:05:00"));
            list_DataPoints.Add(new DataPoint((float)28.83, (float)28.805, (float)28.83, (float)28.81, "2023-05-12 15:06:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.83, (float)28.84, (float)28.835, "2023-05-12 15:07:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.8325, (float)28.845, (float)28.84, "2023-05-12 15:08:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.83, (float)28.835, (float)28.84, "2023-05-12 15:09:00"));
            list_DataPoints.Add(new DataPoint((float)28.855, (float)28.835, (float)28.85, (float)28.835, "2023-05-12 15:10:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.825, (float)28.825, (float)28.85, "2023-05-12 15:11:00"));
            list_DataPoints.Add(new DataPoint((float)28.8383, (float)28.8201, (float)28.825, (float)28.825, "2023-05-12 15:12:00"));
            list_DataPoints.Add(new DataPoint((float)28.86, (float)28.82, (float)28.835, (float)28.825, "2023-05-12 15:13:00"));
            list_DataPoints.Add(new DataPoint((float)28.84, (float)28.825, (float)28.835, (float)28.8301, "2023-05-12 15:14:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.825, (float)28.845, (float)28.84, "2023-05-12 15:15:00"));
            list_DataPoints.Add(new DataPoint((float)28.85, (float)28.83, (float)28.845, (float)28.845, "2023-05-12 15:16:00"));
            list_DataPoints.Add(new DataPoint((float)28.8559, (float)28.845, (float)28.845, (float)28.845, "2023-05-12 15:17:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.845, (float)28.8665, (float)28.85, "2023-05-12 15:18:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.855, (float)28.865, (float)28.865, "2023-05-12 15:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.8699, (float)28.875, (float)28.8699, "2023-05-12 15:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.865, (float)28.8864, (float)28.875, "2023-05-12 15:21:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.88, (float)28.885, (float)28.885, "2023-05-12 15:22:00"));
            list_DataPoints.Add(new DataPoint((float)28.89, (float)28.86, (float)28.865, (float)28.88, "2023-05-12 15:23:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.86, (float)28.865, (float)28.86, "2023-05-12 15:24:00"));
            list_DataPoints.Add(new DataPoint((float)28.88, (float)28.86, (float)28.86, (float)28.87, "2023-05-12 15:25:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.85, (float)28.8699, (float)28.865, "2023-05-12 15:26:00"));
            list_DataPoints.Add(new DataPoint((float)28.875, (float)28.86, (float)28.865, (float)28.865, "2023-05-12 15:27:00"));
            list_DataPoints.Add(new DataPoint((float)28.865, (float)28.85, (float)28.855, (float)28.865, "2023-05-12 15:28:00"));
            list_DataPoints.Add(new DataPoint((float)28.87, (float)28.855, (float)28.865, (float)28.855, "2023-05-12 15:29:00"));
            list_DataPoints.Add(new DataPoint((float)28.9, (float)28.865, (float)28.895, (float)28.865, "2023-05-12 15:30:00"));
            list_DataPoints.Add(new DataPoint((float)28.92, (float)28.885, (float)28.905, (float)28.895, "2023-05-12 15:31:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.8937, (float)28.895, (float)28.905, "2023-05-12 15:32:00"));
            list_DataPoints.Add(new DataPoint((float)28.9199, (float)28.8925, (float)28.905, (float)28.8925, "2023-05-12 15:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.925, (float)28.9, (float)28.915, (float)28.905, "2023-05-12 15:34:00"));
            list_DataPoints.Add(new DataPoint((float)28.93, (float)28.91, (float)28.92, (float)28.92, "2023-05-12 15:35:00"));
            list_DataPoints.Add(new DataPoint((float)28.925, (float)28.89, (float)28.8918, (float)28.925, "2023-05-12 15:36:00"));
            list_DataPoints.Add(new DataPoint((float)28.905, (float)28.885, (float)28.885, (float)28.89, "2023-05-12 15:37:00"));
            list_DataPoints.Add(new DataPoint((float)28.895, (float)28.88, (float)28.885, (float)28.885, "2023-05-12 15:38:00"));
            list_DataPoints.Add(new DataPoint((float)28.895, (float)28.88, (float)28.89, (float)28.88, "2023-05-12 15:39:00"));
            list_DataPoints.Add(new DataPoint((float)28.9, (float)28.89, (float)28.895, (float)28.9, "2023-05-12 15:40:00"));
            list_DataPoints.Add(new DataPoint((float)28.9, (float)28.89, (float)28.895, (float)28.895, "2023-05-12 15:41:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.8901, (float)28.895, (float)28.9, "2023-05-12 15:42:00"));
            list_DataPoints.Add(new DataPoint((float)28.91, (float)28.89, (float)28.905, (float)28.895, "2023-05-12 15:43:00"));
            list_DataPoints.Add(new DataPoint((float)28.9575, (float)28.905, (float)28.95, (float)28.91, "2023-05-12 15:44:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.92, (float)28.92, (float)28.951, "2023-05-12 15:45:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.925, (float)28.925, (float)28.93, "2023-05-12 15:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.92, (float)28.93, (float)28.925, "2023-05-12 15:47:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.93, (float)28.935, (float)28.935, "2023-05-12 15:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.925, (float)28.94, (float)28.935, "2023-05-12 15:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.94, (float)28.9, (float)28.93, (float)28.94, "2023-05-12 15:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.955, (float)28.925, (float)28.945, (float)28.93, "2023-05-12 15:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.94, (float)28.96, (float)28.95, "2023-05-12 15:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.94, (float)28.945, (float)28.96, "2023-05-12 15:53:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.92, (float)28.935, (float)28.945, "2023-05-12 15:54:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.935, (float)28.97, (float)28.935, "2023-05-12 15:55:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.95, (float)28.95, (float)28.965, "2023-05-12 15:56:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.94, (float)28.95, (float)28.955, "2023-05-12 15:57:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.93, (float)28.93, (float)28.955, "2023-05-12 15:58:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.93, (float)28.96, (float)28.935, "2023-05-12 15:59:00"));
            list_DataPoints.Add(new DataPoint((float)28.95, (float)28.95, (float)28.95, (float)28.95, "2023-05-12 16:00:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.88, (float)28.96, (float)28.9, "2023-05-15 09:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.9692, (float)28.98, (float)28.97, "2023-05-15 09:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)28.97, (float)29.04, (float)28.98, "2023-05-15 09:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)28.98, (float)28.9856, (float)29.05, "2023-05-15 09:33:00"));
            list_DataPoints.Add(new DataPoint((float)28.9936, (float)28.96, (float)28.98, (float)28.98, "2023-05-15 09:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)28.98, (float)29.041, (float)28.98, "2023-05-15 09:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.08, (float)29.015, (float)29.0601, (float)29.05, "2023-05-15 09:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.0687, (float)29.015, (float)29.015, (float)29.065, "2023-05-15 09:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.01, (float)29.02, (float)29.02, "2023-05-15 09:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29, (float)29.01, (float)29.025, "2023-05-15 09:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.98, (float)28.981, (float)29, "2023-05-15 09:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)28.965, (float)29.03, (float)28.9801, "2023-05-15 09:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.01, (float)29.035, (float)29.02, "2023-05-15 09:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.08, (float)29.03, (float)29.075, (float)29.04, "2023-05-15 09:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.085, (float)29.03, (float)29.0445, (float)29.0797, "2023-05-15 09:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.055, (float)29.005, (float)29.025, (float)29.045, "2023-05-15 09:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)28.965, (float)28.965, (float)29.025, "2023-05-15 09:46:00"));
            list_DataPoints.Add(new DataPoint((float)28.995, (float)28.96, (float)28.9801, (float)28.9684, "2023-05-15 09:47:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.95, (float)28.955, (float)28.9801, "2023-05-15 09:48:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.925, (float)28.97, (float)28.955, "2023-05-15 09:49:00"));
            list_DataPoints.Add(new DataPoint((float)28.9901, (float)28.9393, (float)28.94, (float)28.9684, "2023-05-15 09:50:00"));
            list_DataPoints.Add(new DataPoint((float)28.965, (float)28.93, (float)28.93, (float)28.94, "2023-05-15 09:51:00"));
            list_DataPoints.Add(new DataPoint((float)28.969, (float)28.925, (float)28.96, (float)28.935, "2023-05-15 09:52:00"));
            list_DataPoints.Add(new DataPoint((float)28.97, (float)28.9523, (float)28.9693, (float)28.96, "2023-05-15 09:53:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.96, (float)28.9981, (float)28.965, "2023-05-15 09:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.99, (float)28.9901, (float)29, "2023-05-15 09:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.0002, (float)28.98, (float)29, (float)28.99, "2023-05-15 09:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.985, (float)28.985, (float)29, "2023-05-15 09:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.97, (float)28.9799, (float)28.9878, "2023-05-15 09:58:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.95, (float)28.95, (float)28.97, "2023-05-15 09:59:00"));
            list_DataPoints.Add(new DataPoint((float)28.96, (float)28.95, (float)28.96, (float)28.955, "2023-05-15 10:00:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.945, (float)28.985, (float)28.95, "2023-05-15 10:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.985, (float)29.005, (float)28.99, "2023-05-15 10:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.03, (float)29.01, (float)29.01, (float)29.01, "2023-05-15 10:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.0303, (float)29.01, (float)29.025, (float)29.02, "2023-05-15 10:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.055, (float)29.025, (float)29.0338, (float)29.025, "2023-05-15 10:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.035, (float)29.01, (float)29.0201, (float)29.03, "2023-05-15 10:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.025, (float)28.97, (float)28.97, (float)29.025, "2023-05-15 10:07:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.96, (float)28.9621, (float)28.975, "2023-05-15 10:08:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.95, (float)28.96, (float)28.9601, "2023-05-15 10:09:00"));
            list_DataPoints.Add(new DataPoint((float)28.975, (float)28.96, (float)28.97, (float)28.965, "2023-05-15 10:10:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.96, (float)28.97, (float)28.975, "2023-05-15 10:11:00"));
            list_DataPoints.Add(new DataPoint((float)28.986, (float)28.96, (float)28.97, (float)28.96, "2023-05-15 10:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.015, (float)28.97, (float)29.01, (float)28.98, "2023-05-15 10:13:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.97, (float)28.99, (float)29, "2023-05-15 10:14:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.98, (float)29, (float)28.9901, "2023-05-15 10:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.99, (float)28.9928, (float)29, "2023-05-15 10:16:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.99, (float)29, (float)28.995, "2023-05-15 10:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.99, (float)29, (float)28.995, "2023-05-15 10:18:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.9725, (float)28.975, (float)28.9908, "2023-05-15 10:19:00"));
            list_DataPoints.Add(new DataPoint((float)28.98, (float)28.95, (float)28.955, (float)28.98, "2023-05-15 10:20:00"));
            list_DataPoints.Add(new DataPoint((float)28.99, (float)28.955, (float)28.98, (float)28.96, "2023-05-15 10:21:00"));
            list_DataPoints.Add(new DataPoint((float)29, (float)28.92, (float)28.985, (float)28.9889, "2023-05-15 10:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.985, (float)29, (float)28.985, "2023-05-15 10:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.99, (float)29.0021, (float)29, "2023-05-15 10:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.025, (float)29, (float)29.015, (float)29.005, "2023-05-15 10:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)28.99, (float)28.9901, (float)29.015, "2023-05-15 10:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.005, (float)28.99, (float)29, (float)28.99, "2023-05-15 10:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.99, (float)29.01, (float)28.995, "2023-05-15 10:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.01, (float)28.975, (float)28.975, (float)29.01, "2023-05-15 10:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.015, (float)28.97, (float)29.015, (float)28.975, "2023-05-15 10:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.0101, (float)29.0164, (float)29.0167, "2023-05-15 10:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.02, (float)29, (float)29.01, (float)29.02, "2023-05-15 10:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.0575, (float)29.02, (float)29.03, (float)29.02, "2023-05-15 10:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.0575, (float)29.03, (float)29.0503, (float)29.03, "2023-05-15 10:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.065, (float)29.035, (float)29.035, (float)29.06, "2023-05-15 10:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.02, (float)29.04, (float)29.035, "2023-05-15 10:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.03, (float)29.045, (float)29.045, "2023-05-15 10:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.05, (float)29.02, (float)29.025, (float)29.045, "2023-05-15 10:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.04, (float)29.025, (float)29.04, (float)29.025, "2023-05-15 10:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.0575, (float)29.035, (float)29.045, (float)29.035, "2023-05-15 10:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.045, (float)29.05, (float)29.05, "2023-05-15 10:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.05, (float)29.0592, (float)29.055, "2023-05-15 10:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.085, (float)29.05, (float)29.07, (float)29.0575, "2023-05-15 10:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.08, (float)29.05, (float)29.055, (float)29.07, "2023-05-15 10:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.06, (float)29.045, (float)29.0599, (float)29.055, "2023-05-15 10:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.11, (float)29.055, (float)29.1, (float)29.06, "2023-05-15 10:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.1372, (float)29.0901, (float)29.12, (float)29.0901, "2023-05-15 10:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.14, (float)29.1, (float)29.11, (float)29.12, "2023-05-15 10:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.125, (float)29.1008, (float)29.12, (float)29.105, "2023-05-15 10:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.15, (float)29.115, (float)29.1399, (float)29.115, "2023-05-15 10:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.1379, (float)29.12, (float)29.13, (float)29.135, "2023-05-15 10:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.15, (float)29.125, (float)29.14, (float)29.135, "2023-05-15 10:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.145, (float)29.125, (float)29.14, (float)29.135, "2023-05-15 10:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.16, (float)29.14, (float)29.1556, (float)29.15, "2023-05-15 10:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.18, (float)29.155, (float)29.17, (float)29.16, "2023-05-15 10:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.18, (float)29.16, (float)29.165, (float)29.17, "2023-05-15 10:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.175, (float)29.13, (float)29.1387, (float)29.17, "2023-05-15 10:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.145, (float)29.12, (float)29.12, (float)29.1396, "2023-05-15 10:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.12, (float)29.085, (float)29.105, (float)29.12, "2023-05-15 10:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.14, (float)29.105, (float)29.135, (float)29.105, "2023-05-15 11:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.16, (float)29.135, (float)29.1599, (float)29.135, "2023-05-15 11:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.17, (float)29.15, (float)29.165, (float)29.155, "2023-05-15 11:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.175, (float)29.1508, (float)29.1508, (float)29.165, "2023-05-15 11:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.1755, (float)29.1503, (float)29.17, (float)29.155, "2023-05-15 11:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.21, (float)29.17, (float)29.21, (float)29.175, "2023-05-15 11:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.22, (float)29.19, (float)29.22, (float)29.205, "2023-05-15 11:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.225, (float)29.195, (float)29.195, (float)29.22, "2023-05-15 11:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.2, (float)29.18, (float)29.1803, (float)29.2, "2023-05-15 11:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.2099, (float)29.18, (float)29.205, (float)29.185, "2023-05-15 11:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.25, (float)29.205, (float)29.235, (float)29.21, "2023-05-15 11:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.2499, (float)29.21, (float)29.2179, (float)29.24, "2023-05-15 11:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.24, (float)29.2101, (float)29.235, (float)29.22, "2023-05-15 11:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.265, (float)29.235, (float)29.255, (float)29.2399, "2023-05-15 11:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.26, (float)29.235, (float)29.25, (float)29.25, "2023-05-15 11:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.27, (float)29.24, (float)29.27, (float)29.25, "2023-05-15 11:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.27, (float)29.2401, (float)29.245, (float)29.265, "2023-05-15 11:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.25, (float)29.24, (float)29.2434, (float)29.25, "2023-05-15 11:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.26, (float)29.2401, (float)29.255, (float)29.245, "2023-05-15 11:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.2675, (float)29.24, (float)29.26, (float)29.26, "2023-05-15 11:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.27, (float)29.25, (float)29.2642, (float)29.2578, "2023-05-15 11:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.27, (float)29.26, (float)29.265, (float)29.265, "2023-05-15 11:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.27, (float)29.255, (float)29.27, (float)29.265, "2023-05-15 11:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.295, (float)29.265, (float)29.285, (float)29.265, "2023-05-15 11:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.31, (float)29.28, (float)29.305, (float)29.285, "2023-05-15 11:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.31, (float)29.285, (float)29.295, (float)29.31, "2023-05-15 11:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.305, (float)29.2934, (float)29.295, (float)29.295, "2023-05-15 11:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.3, (float)29.29, (float)29.3, (float)29.295, "2023-05-15 11:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.31, (float)29.285, (float)29.2912, (float)29.2976, "2023-05-15 11:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.3, (float)29.29, (float)29.2945, (float)29.29, "2023-05-15 11:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.3, (float)29.27, (float)29.2728, (float)29.295, "2023-05-15 11:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.29, (float)29.26, (float)29.285, (float)29.27, "2023-05-15 11:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.31, (float)29.28, (float)29.29, (float)29.29, "2023-05-15 11:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.32, (float)29.29, (float)29.305, (float)29.295, "2023-05-15 11:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.31, (float)29.3, (float)29.305, (float)29.31, "2023-05-15 11:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.32, (float)29.3, (float)29.3, (float)29.31, "2023-05-15 11:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.32, (float)29.3, (float)29.3052, (float)29.31, "2023-05-15 11:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.31, (float)29.28, (float)29.29, (float)29.305, "2023-05-15 11:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.3, (float)29.2801, (float)29.29, (float)29.2828, "2023-05-15 11:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.3038, (float)29.265, (float)29.28, (float)29.29, "2023-05-15 11:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.2907, (float)29.27, (float)29.285, (float)29.28, "2023-05-15 11:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.3, (float)29.285, (float)29.29, (float)29.285, "2023-05-15 11:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.305, (float)29.29, (float)29.295, (float)29.29, "2023-05-15 11:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.31, (float)29.29, (float)29.305, (float)29.295, "2023-05-15 11:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.33, (float)29.305, (float)29.315, (float)29.305, "2023-05-15 11:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.3112, (float)29.285, (float)29.295, (float)29.31, "2023-05-15 11:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.34, (float)29.285, (float)29.34, (float)29.29, "2023-05-15 11:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.37, (float)29.33, (float)29.345, (float)29.34, "2023-05-15 11:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.36, (float)29.345, (float)29.36, (float)29.3478, "2023-05-15 11:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.37, (float)29.36, (float)29.37, (float)29.36, "2023-05-15 11:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.37, (float)29.36, (float)29.3663, (float)29.365, "2023-05-15 11:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.39, (float)29.3603, (float)29.365, (float)29.3636, "2023-05-15 11:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.3776, (float)29.3501, (float)29.355, (float)29.37, "2023-05-15 11:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.38, (float)29.35, (float)29.375, (float)29.355, "2023-05-15 11:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.38, (float)29.34, (float)29.35, (float)29.38, "2023-05-15 11:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.37, (float)29.3499, (float)29.365, (float)29.3499, "2023-05-15 11:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.3669, (float)29.34, (float)29.355, (float)29.3669, "2023-05-15 11:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.37, (float)29.35, (float)29.3585, (float)29.355, "2023-05-15 11:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.3868, (float)29.355, (float)29.385, (float)29.36, "2023-05-15 11:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.4, (float)29.3807, (float)29.395, (float)29.3855, "2023-05-15 11:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.4, (float)29.39, (float)29.39, (float)29.395, "2023-05-15 12:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.39, (float)29.415, (float)29.395, "2023-05-15 12:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.37, (float)29.37, (float)29.415, "2023-05-15 12:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.38, (float)29.36, (float)29.38, (float)29.3618, "2023-05-15 12:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.39, (float)29.37, (float)29.38, (float)29.375, "2023-05-15 12:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.39, (float)29.365, (float)29.39, (float)29.3701, "2023-05-15 12:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.41, (float)29.38, (float)29.4, (float)29.385, "2023-05-15 12:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.41, (float)29.38, (float)29.41, (float)29.4, "2023-05-15 12:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.405, (float)29.4109, (float)29.41, "2023-05-15 12:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.425, (float)29.41, (float)29.4184, (float)29.41, "2023-05-15 12:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.4, (float)29.43, (float)29.4166, "2023-05-15 12:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.425, (float)29.4378, (float)29.425, "2023-05-15 12:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.41, (float)29.415, (float)29.44, "2023-05-15 12:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.425, (float)29.41, (float)29.42, (float)29.415, "2023-05-15 12:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.4229, (float)29.425, (float)29.425, "2023-05-15 12:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.42, (float)29.46, (float)29.4287, "2023-05-15 12:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.4909, (float)29.4501, (float)29.4909, (float)29.458, "2023-05-15 12:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.49, (float)29.5031, (float)29.495, "2023-05-15 12:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.49, (float)29.5, (float)29.505, "2023-05-15 12:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.53, (float)29.5, (float)29.51, (float)29.5082, "2023-05-15 12:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.49, (float)29.49, (float)29.51, "2023-05-15 12:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.5007, (float)29.48, (float)29.485, (float)29.49, "2023-05-15 12:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.465, (float)29.465, (float)29.49, "2023-05-15 12:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.465, (float)29.44, (float)29.455, (float)29.465, "2023-05-15 12:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.44, (float)29.44, (float)29.46, "2023-05-15 12:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.42, (float)29.425, (float)29.445, "2023-05-15 12:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.42, (float)29.445, (float)29.425, "2023-05-15 12:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.47, (float)29.44, (float)29.46, (float)29.44, "2023-05-15 12:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.4501, (float)29.48, (float)29.459, "2023-05-15 12:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.46, (float)29.46, (float)29.48, "2023-05-15 12:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.47, (float)29.45, (float)29.455, (float)29.465, "2023-05-15 12:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.47, (float)29.45, (float)29.45, (float)29.45, "2023-05-15 12:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.4401, (float)29.4401, (float)29.455, "2023-05-15 12:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.4853, (float)29.44, (float)29.485, (float)29.45, "2023-05-15 12:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.4803, (float)29.49, (float)29.485, "2023-05-15 12:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.49, (float)29.5001, (float)29.49, "2023-05-15 12:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.5181, (float)29.5, (float)29.51, (float)29.5069, "2023-05-15 12:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.515, (float)29.5, (float)29.5084, (float)29.51, "2023-05-15 12:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.54, (float)29.51, (float)29.525, (float)29.51, "2023-05-15 12:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.535, (float)29.52, (float)29.523, (float)29.52, "2023-05-15 12:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.53, (float)29.52, (float)29.52, (float)29.525, "2023-05-15 12:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.54, (float)29.52, (float)29.54, (float)29.52, "2023-05-15 12:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.55, (float)29.532, (float)29.545, (float)29.535, "2023-05-15 12:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.565, (float)29.54, (float)29.545, (float)29.541, "2023-05-15 12:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.565, (float)29.54, (float)29.56, (float)29.545, "2023-05-15 12:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.5697, (float)29.595, (float)29.57, "2023-05-15 12:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.585, (float)29.5968, (float)29.595, "2023-05-15 12:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.595, (float)29.57, (float)29.575, (float)29.5904, "2023-05-15 12:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.575, (float)29.56, (float)29.57, (float)29.575, "2023-05-15 12:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.55, (float)29.555, (float)29.565, "2023-05-15 12:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.54, (float)29.5699, (float)29.56, "2023-05-15 12:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.55, (float)29.5684, (float)29.565, "2023-05-15 12:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.5777, (float)29.5635, (float)29.575, (float)29.565, "2023-05-15 12:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.56, (float)29.56, (float)29.5774, "2023-05-15 12:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.56, (float)29.575, (float)29.57, "2023-05-15 12:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.5999, (float)29.5701, (float)29.595, (float)29.5733, "2023-05-15 12:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.62, (float)29.59, (float)29.605, (float)29.595, "2023-05-15 12:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.6195, (float)29.603, (float)29.615, (float)29.605, "2023-05-15 12:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.645, (float)29.6195, (float)29.64, (float)29.62, "2023-05-15 12:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.6301, (float)29.645, (float)29.64, "2023-05-15 12:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.666, (float)29.63, (float)29.63, (float)29.65, "2023-05-15 13:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.623, (float)29.6274, (float)29.635, "2023-05-15 13:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.625, (float)29.6, (float)29.608, (float)29.625, "2023-05-15 13:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.61, (float)29.59, (float)29.605, (float)29.6086, "2023-05-15 13:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.605, (float)29.63, (float)29.61, "2023-05-15 13:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.615, (float)29.62, (float)29.625, "2023-05-15 13:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.62, (float)29.64, (float)29.6221, "2023-05-15 13:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.655, (float)29.63, (float)29.6384, (float)29.6385, "2023-05-15 13:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.655, (float)29.64, (float)29.65, (float)29.64, "2023-05-15 13:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.675, (float)29.645, (float)29.675, (float)29.6494, "2023-05-15 13:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.71, (float)29.675, (float)29.6899, (float)29.68, "2023-05-15 13:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.67, (float)29.675, (float)29.685, "2023-05-15 13:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.6899, (float)29.665, (float)29.665, (float)29.67, "2023-05-15 13:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.6534, (float)29.665, (float)29.665, "2023-05-15 13:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.685, (float)29.6601, (float)29.68, (float)29.665, "2023-05-15 13:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.685, (float)29.6601, (float)29.665, (float)29.6788, "2023-05-15 13:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.66, (float)29.665, (float)29.67, "2023-05-15 13:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.665, (float)29.62, (float)29.645, (float)29.665, "2023-05-15 13:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.64, (float)29.665, (float)29.65, "2023-05-15 13:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.66, (float)29.62, (float)29.6299, (float)29.66, "2023-05-15 13:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.6299, (float)29.6, (float)29.6091, (float)29.6299, "2023-05-15 13:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.61, (float)29.585, (float)29.59, (float)29.6, "2023-05-15 13:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.595, (float)29.5733, (float)29.575, (float)29.59, "2023-05-15 13:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.5834, (float)29.57, (float)29.575, (float)29.575, "2023-05-15 13:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.56, (float)29.575, (float)29.57, "2023-05-15 13:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.57, (float)29.58, (float)29.575, "2023-05-15 13:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.56, (float)29.56, (float)29.5784, "2023-05-15 13:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.5699, (float)29.52, (float)29.525, (float)29.56, "2023-05-15 13:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.54, (float)29.5013, (float)29.5013, (float)29.52, "2023-05-15 13:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.475, (float)29.48, (float)29.505, "2023-05-15 13:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.47, (float)29.48, (float)29.47, "2023-05-15 13:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.46, (float)29.485, (float)29.48, "2023-05-15 13:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.48, (float)29.495, (float)29.48, "2023-05-15 13:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.495, (float)29.5018, (float)29.4976, "2023-05-15 13:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.5, (float)29.5033, (float)29.505, "2023-05-15 13:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.505, (float)29.48, (float)29.4899, (float)29.505, "2023-05-15 13:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.48, (float)29.48, (float)29.485, "2023-05-15 13:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.4801, (float)29.485, (float)29.485, "2023-05-15 13:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.48, (float)29.485, (float)29.48, "2023-05-15 13:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.45, (float)29.455, (float)29.485, "2023-05-15 13:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.45, (float)29.475, (float)29.46, "2023-05-15 13:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.46, (float)29.465, (float)29.47, "2023-05-15 13:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.45, (float)29.465, (float)29.46, "2023-05-15 13:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.445, (float)29.47, (float)29.465, "2023-05-15 13:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.4764, (float)29.46, (float)29.465, (float)29.47, "2023-05-15 13:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.4718, (float)29.46, (float)29.46, (float)29.4624, "2023-05-15 13:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.485, (float)29.46, (float)29.48, (float)29.465, "2023-05-15 13:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.48, (float)29.485, (float)29.4863, "2023-05-15 13:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.495, (float)29.48, (float)29.49, (float)29.4831, "2023-05-15 13:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.4957, (float)29.48, (float)29.495, (float)29.4917, "2023-05-15 13:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.495, (float)29.48, (float)29.48, (float)29.495, "2023-05-15 13:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.4845, (float)29.5001, (float)29.4845, "2023-05-15 13:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.51, (float)29.5, (float)29.505, (float)29.5099, "2023-05-15 13:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.52, (float)29.5, (float)29.515, (float)29.505, "2023-05-15 13:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.5367, (float)29.51, (float)29.5367, (float)29.515, "2023-05-15 13:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.535, (float)29.52, (float)29.525, (float)29.535, "2023-05-15 13:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.54, (float)29.52, (float)29.535, (float)29.525, "2023-05-15 13:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.55, (float)29.535, (float)29.545, (float)29.54, "2023-05-15 13:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.54, (float)29.5403, (float)29.54, "2023-05-15 13:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.545, (float)29.53, (float)29.535, (float)29.545, "2023-05-15 13:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.545, (float)29.53, (float)29.53, (float)29.535, "2023-05-15 14:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.5556, (float)29.535, (float)29.555, (float)29.535, "2023-05-15 14:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.5553, (float)29.53, (float)29.545, (float)29.5553, "2023-05-15 14:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.555, (float)29.53, (float)29.5312, (float)29.545, "2023-05-15 14:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.54, (float)29.52, (float)29.5291, (float)29.535, "2023-05-15 14:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.535, (float)29.51, (float)29.515, (float)29.53, "2023-05-15 14:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.525, (float)29.51, (float)29.525, (float)29.515, "2023-05-15 14:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.54, (float)29.52, (float)29.5393, (float)29.525, "2023-05-15 14:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.55, (float)29.535, (float)29.545, (float)29.54, "2023-05-15 14:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.55, (float)29.54, (float)29.5408, (float)29.545, "2023-05-15 14:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.54, (float)29.5579, (float)29.55, "2023-05-15 14:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.55, (float)29.555, (float)29.555, "2023-05-15 14:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.55, (float)29.555, (float)29.555, "2023-05-15 14:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.55, (float)29.555, (float)29.56, "2023-05-15 14:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.555, (float)29.59, (float)29.555, "2023-05-15 14:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.565, (float)29.575, (float)29.585, "2023-05-15 14:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.585, (float)29.53, (float)29.55, (float)29.575, "2023-05-15 14:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.545, (float)29.575, (float)29.55, "2023-05-15 14:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.56, (float)29.565, (float)29.575, "2023-05-15 14:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.555, (float)29.565, (float)29.56, "2023-05-15 14:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.555, (float)29.585, (float)29.565, "2023-05-15 14:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.58, (float)29.585, (float)29.585, "2023-05-15 14:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.58, (float)29.595, (float)29.585, "2023-05-15 14:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.595, (float)29.56, (float)29.56, (float)29.595, "2023-05-15 14:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.56, (float)29.565, (float)29.565, "2023-05-15 14:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.565, (float)29.54, (float)29.55, (float)29.565, "2023-05-15 14:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.535, (float)29.56, (float)29.555, "2023-05-15 14:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.552, (float)29.555, (float)29.56, "2023-05-15 14:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.54, (float)29.54, (float)29.555, "2023-05-15 14:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.545, (float)29.56, (float)29.545, "2023-05-15 14:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.55, (float)29.555, (float)29.56, "2023-05-15 14:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.55, (float)29.54, (float)29.55, (float)29.55, "2023-05-15 14:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.54, (float)29.545, (float)29.55, "2023-05-15 14:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.545, (float)29.56, (float)29.55, "2023-05-15 14:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.555, (float)29.56, (float)29.56, "2023-05-15 14:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.5524, (float)29.57, (float)29.56, "2023-05-15 14:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.55, (float)29.5797, (float)29.565, "2023-05-15 14:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.575, (float)29.55, (float)29.555, (float)29.575, "2023-05-15 14:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.5501, (float)29.565, (float)29.555, "2023-05-15 14:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.5768, (float)29.56, (float)29.575, (float)29.56, "2023-05-15 14:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.575, (float)29.565, (float)29.57, (float)29.575, "2023-05-15 14:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.5632, (float)29.575, (float)29.5668, "2023-05-15 14:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.57, (float)29.575, (float)29.575, "2023-05-15 14:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.57, (float)29.585, (float)29.57, "2023-05-15 14:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.595, (float)29.58, (float)29.58, (float)29.585, "2023-05-15 14:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.595, (float)29.565, (float)29.585, (float)29.58, "2023-05-15 14:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.6099, (float)29.58, (float)29.605, (float)29.59, "2023-05-15 14:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.575, (float)29.5876, (float)29.6, "2023-05-15 14:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.58, (float)29.585, (float)29.585, "2023-05-15 14:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.595, (float)29.58, (float)29.585, (float)29.585, "2023-05-15 14:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.58, (float)29.585, (float)29.5824, "2023-05-15 14:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.55, (float)29.555, (float)29.59, "2023-05-15 14:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.555, (float)29.54, (float)29.545, (float)29.555, "2023-05-15 14:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.545, (float)29.545, (float)29.55, "2023-05-15 14:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.54, (float)29.52, (float)29.5287, (float)29.54, "2023-05-15 14:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.536, (float)29.52, (float)29.525, (float)29.525, "2023-05-15 14:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.5272, (float)29.51, (float)29.51, (float)29.525, "2023-05-15 14:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.52, (float)29.49, (float)29.5, (float)29.5133, "2023-05-15 14:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.525, (float)29.5, (float)29.525, (float)29.501, "2023-05-15 14:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.53, (float)29.52, (float)29.5282, (float)29.525, "2023-05-15 14:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.53, (float)29.49, (float)29.51, (float)29.525, "2023-05-15 15:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.535, (float)29.505, (float)29.535, (float)29.505, "2023-05-15 15:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.54, (float)29.52, (float)29.535, (float)29.535, "2023-05-15 15:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.53, (float)29.54, (float)29.535, "2023-05-15 15:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.5571, (float)29.54, (float)29.555, (float)29.545, "2023-05-15 15:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.555, (float)29.53, (float)29.535, (float)29.55, "2023-05-15 15:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.55, (float)29.5202, (float)29.53, (float)29.54, "2023-05-15 15:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.555, (float)29.525, (float)29.555, (float)29.535, "2023-05-15 15:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.5785, (float)29.55, (float)29.57, (float)29.555, "2023-05-15 15:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.575, (float)29.55, (float)29.5608, (float)29.57, "2023-05-15 15:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.54, (float)29.57, (float)29.56, "2023-05-15 15:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.585, (float)29.565, (float)29.575, (float)29.575, "2023-05-15 15:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.57, (float)29.5737, (float)29.575, "2023-05-15 15:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.585, (float)29.56, (float)29.585, (float)29.575, "2023-05-15 15:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.585, (float)29.57, (float)29.58, (float)29.58, "2023-05-15 15:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.565, (float)29.585, (float)29.57, "2023-05-15 15:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.585, (float)29.56, (float)29.57, (float)29.585, "2023-05-15 15:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.5899, (float)29.56, (float)29.58, (float)29.57, "2023-05-15 15:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.58, (float)29.585, (float)29.585, "2023-05-15 15:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.58, (float)29.595, (float)29.59, "2023-05-15 15:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.58, (float)29.595, (float)29.595, "2023-05-15 15:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.62, (float)29.595, (float)29.62, (float)29.595, "2023-05-15 15:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.6117, (float)29.615, (float)29.6117, "2023-05-15 15:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.61, (float)29.63, (float)29.61, "2023-05-15 15:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.6225, (float)29.63, (float)29.63, "2023-05-15 15:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.645, (float)29.62, (float)29.64, (float)29.6315, "2023-05-15 15:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.645, (float)29.63, (float)29.63, (float)29.64, "2023-05-15 15:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.64, (float)29.6372, "2023-05-15 15:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.66, (float)29.64, (float)29.655, (float)29.64, "2023-05-15 15:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.66, (float)29.635, (float)29.655, (float)29.65, "2023-05-15 15:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.66, (float)29.63, (float)29.635, (float)29.655, "2023-05-15 15:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.665, (float)29.635, (float)29.6599, (float)29.635, "2023-05-15 15:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.65, (float)29.675, (float)29.655, "2023-05-15 15:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.67, (float)29.68, (float)29.6745, "2023-05-15 15:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.675, (float)29.65, (float)29.655, (float)29.67, "2023-05-15 15:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.655, (float)29.63, (float)29.645, (float)29.65, "2023-05-15 15:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.66, (float)29.64, (float)29.66, (float)29.645, "2023-05-15 15:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.66, (float)29.675, (float)29.66, "2023-05-15 15:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.675, (float)29.675, (float)29.68, "2023-05-15 15:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.67, (float)29.68, (float)29.675, "2023-05-15 15:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.71, (float)29.68, (float)29.7, (float)29.685, "2023-05-15 15:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.71, (float)29.7, (float)29.705, (float)29.705, "2023-05-15 15:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.7099, (float)29.69, (float)29.705, (float)29.7, "2023-05-15 15:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.71, (float)29.68, (float)29.695, (float)29.705, "2023-05-15 15:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.68, (float)29.695, (float)29.6934, "2023-05-15 15:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.705, (float)29.67, (float)29.6732, (float)29.69, "2023-05-15 15:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.705, (float)29.675, (float)29.705, (float)29.6788, "2023-05-15 15:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.72, (float)29.7, (float)29.71, (float)29.705, "2023-05-15 15:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.72, (float)29.7, (float)29.715, (float)29.715, "2023-05-15 15:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.75, (float)29.7, (float)29.745, (float)29.72, "2023-05-15 15:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.75, (float)29.715, (float)29.725, (float)29.74, "2023-05-15 15:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.75, (float)29.72, (float)29.73, (float)29.73, "2023-05-15 15:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.73, (float)29.72, (float)29.725, (float)29.73, "2023-05-15 15:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.75, (float)29.72, (float)29.745, (float)29.725, "2023-05-15 15:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.7454, (float)29.71, (float)29.71, (float)29.745, "2023-05-15 15:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.75, (float)29.715, (float)29.73, (float)29.715, "2023-05-15 15:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.74, (float)29.725, (float)29.735, (float)29.73, "2023-05-15 15:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.75, (float)29.72, (float)29.7399, (float)29.7399, "2023-05-15 15:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.78, (float)29.72, (float)29.775, (float)29.73, "2023-05-15 15:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.825, (float)29.77, (float)29.815, (float)29.77, "2023-05-15 15:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.83, (float)29.78, (float)29.8, (float)29.8, "2023-05-15 16:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.61, (float)29.6343, (float)29.62, "2023-05-16 09:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.78, (float)29.62, (float)29.75, (float)29.64, "2023-05-16 09:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.81, (float)29.7227, (float)29.725, (float)29.76, "2023-05-16 09:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.745, (float)29.7042, (float)29.705, (float)29.725, "2023-05-16 09:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.72, (float)29.67, (float)29.6709, (float)29.71, "2023-05-16 09:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.71, (float)29.675, (float)29.6841, (float)29.68, "2023-05-16 09:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.6811, (float)29.63, (float)29.635, (float)29.68, "2023-05-16 09:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.6499, (float)29.62, (float)29.6403, (float)29.63, "2023-05-16 09:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.59, (float)29.5912, (float)29.6399, "2023-05-16 09:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.62, (float)29.58, (float)29.605, (float)29.59, "2023-05-16 09:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.6, (float)29.64, (float)29.61, "2023-05-16 09:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.6414, (float)29.67, (float)29.6414, "2023-05-16 09:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.6614, (float)29.6802, (float)29.665, "2023-05-16 09:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.7804, (float)29.69, (float)29.77, (float)29.69, "2023-05-16 09:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.83, (float)29.764, (float)29.8, (float)29.776, "2023-05-16 09:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.9, (float)29.81, (float)29.89, (float)29.81, "2023-05-16 09:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.89, (float)29.825, (float)29.8731, (float)29.88, "2023-05-16 09:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.875, (float)29.81, (float)29.815, (float)29.87, "2023-05-16 09:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.825, (float)29.75, (float)29.7716, (float)29.81, "2023-05-16 09:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.8099, (float)29.7, (float)29.705, (float)29.775, "2023-05-16 09:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.72, (float)29.59, (float)29.59, (float)29.705, "2023-05-16 09:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.625, (float)29.585, (float)29.585, (float)29.59, "2023-05-16 09:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.6006, (float)29.55, (float)29.555, (float)29.58, "2023-05-16 09:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.51, (float)29.5107, (float)29.55, "2023-05-16 09:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.515, (float)29.43, (float)29.44, (float)29.51, "2023-05-16 09:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.4, (float)29.405, (float)29.44, "2023-05-16 09:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.4, (float)29.4501, (float)29.415, "2023-05-16 09:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.5, (float)29.4201, (float)29.495, (float)29.45, "2023-05-16 09:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.5, (float)29.43, (float)29.47, (float)29.49, "2023-05-16 09:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.46, (float)29.4707, (float)29.47, "2023-05-16 09:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.52, (float)29.46, (float)29.475, (float)29.475, "2023-05-16 10:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.53, (float)29.47, (float)29.505, (float)29.47, "2023-05-16 10:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.51, (float)29.565, (float)29.51, "2023-05-16 10:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.5, (float)29.525, (float)29.56, "2023-05-16 10:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.53, (float)29.49, (float)29.52, (float)29.53, "2023-05-16 10:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.54, (float)29.5, (float)29.535, (float)29.5275, "2023-05-16 10:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.555, (float)29.5, (float)29.55, (float)29.53, "2023-05-16 10:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.53, (float)29.545, (float)29.54, "2023-05-16 10:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.52, (float)29.56, (float)29.5495, "2023-05-16 10:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.55, (float)29.555, (float)29.5612, "2023-05-16 10:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.565, (float)29.505, (float)29.505, (float)29.555, "2023-05-16 10:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.535, (float)29.49, (float)29.53, (float)29.5004, "2023-05-16 10:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.565, (float)29.51, (float)29.565, (float)29.54, "2023-05-16 10:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.5, (float)29.525, (float)29.56, "2023-05-16 10:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.51, (float)29.54, (float)29.525, "2023-05-16 10:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.57, (float)29.54, (float)29.565, (float)29.54, "2023-05-16 10:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.55, (float)29.555, (float)29.57, "2023-05-16 10:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.585, (float)29.54, (float)29.575, (float)29.55, "2023-05-16 10:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.58, (float)29.63, (float)29.58, "2023-05-16 10:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.58, (float)29.5989, (float)29.6284, "2023-05-16 10:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.58, (float)29.6, (float)29.595, "2023-05-16 10:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.635, (float)29.6, (float)29.62, (float)29.6, "2023-05-16 10:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.625, (float)29.605, (float)29.6201, (float)29.62, "2023-05-16 10:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.6245, (float)29.56, (float)29.574, (float)29.62, "2023-05-16 10:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.595, (float)29.575, (float)29.5801, (float)29.58, "2023-05-16 10:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.58, (float)29.55, (float)29.569, (float)29.58, "2023-05-16 10:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.5999, (float)29.57, (float)29.59, (float)29.57, "2023-05-16 10:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.585, (float)29.595, (float)29.595, "2023-05-16 10:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.55, (float)29.575, (float)29.59, "2023-05-16 10:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.6116, (float)29.5701, (float)29.6116, (float)29.5702, "2023-05-16 10:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.61, (float)29.68, (float)29.61, "2023-05-16 10:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.645, (float)29.655, (float)29.675, "2023-05-16 10:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.6684, (float)29.63, (float)29.635, (float)29.66, "2023-05-16 10:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.6487, (float)29.62, (float)29.6278, (float)29.635, "2023-05-16 10:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.635, (float)29.61, (float)29.625, (float)29.62, "2023-05-16 10:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.59, (float)29.595, (float)29.625, "2023-05-16 10:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.595, (float)29.57, (float)29.575, (float)29.595, "2023-05-16 10:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.571, (float)29.585, (float)29.58, "2023-05-16 10:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.58, (float)29.59, (float)29.585, "2023-05-16 10:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.615, (float)29.585, (float)29.6, (float)29.585, "2023-05-16 10:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.605, (float)29.59, (float)29.6, (float)29.598, "2023-05-16 10:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.6209, (float)29.59, (float)29.615, (float)29.595, "2023-05-16 10:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.6201, (float)29.6, (float)29.611, (float)29.615, "2023-05-16 10:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.62, (float)29.59, (float)29.615, (float)29.6101, "2023-05-16 10:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.66, (float)29.61, (float)29.652, (float)29.615, "2023-05-16 10:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.61, (float)29.615, (float)29.65, "2023-05-16 10:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.6481, (float)29.6008, (float)29.64, (float)29.62, "2023-05-16 10:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.63, (float)29.6407, "2023-05-16 10:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.63, (float)29.66, (float)29.635, "2023-05-16 10:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.6601, (float)29.63, (float)29.64, (float)29.66, "2023-05-16 10:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.62, (float)29.64, (float)29.64, "2023-05-16 10:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.635, (float)29.64, (float)29.64, "2023-05-16 10:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.6468, (float)29.625, (float)29.63, (float)29.64, "2023-05-16 10:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.62, (float)29.625, (float)29.635, "2023-05-16 10:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.62, (float)29.644, (float)29.625, "2023-05-16 10:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.655, (float)29.63, (float)29.65, (float)29.64, "2023-05-16 10:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.65, (float)29.7, (float)29.655, "2023-05-16 10:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.72, (float)29.68, (float)29.68, (float)29.7, "2023-05-16 10:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.64, (float)29.655, (float)29.6701, "2023-05-16 10:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.64, (float)29.661, (float)29.655, "2023-05-16 10:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.675, (float)29.64, (float)29.645, (float)29.665, "2023-05-16 11:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.62, (float)29.64, (float)29.64, "2023-05-16 11:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.63, (float)29.63, (float)29.635, "2023-05-16 11:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.6301, (float)29.635, "2023-05-16 11:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.605, (float)29.64, (float)29.63, "2023-05-16 11:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.61, (float)29.615, (float)29.64, "2023-05-16 11:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.61, (float)29.625, (float)29.61, "2023-05-16 11:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.6274, (float)29.61, (float)29.615, (float)29.62, "2023-05-16 11:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.615, (float)29.6422, (float)29.615, "2023-05-16 11:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.62, (float)29.63, (float)29.64, "2023-05-16 11:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.62, (float)29.625, (float)29.63, "2023-05-16 11:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.645, (float)29.62, (float)29.635, (float)29.62, "2023-05-16 11:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.625, (float)29.645, (float)29.635, "2023-05-16 11:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.6468, (float)29.62, (float)29.62, (float)29.6468, "2023-05-16 11:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.6, (float)29.63, (float)29.62, "2023-05-16 11:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.6298, (float)29.605, (float)29.61, (float)29.625, "2023-05-16 11:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.6069, (float)29.57, (float)29.57, (float)29.605, "2023-05-16 11:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.61, (float)29.57, (float)29.61, (float)29.575, "2023-05-16 11:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.61, (float)29.59, (float)29.6, (float)29.61, "2023-05-16 11:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.6199, (float)29.6, (float)29.615, (float)29.605, "2023-05-16 11:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.615, (float)29.645, (float)29.62, "2023-05-16 11:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.6499, (float)29.595, (float)29.635, (float)29.6499, "2023-05-16 11:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.615, (float)29.635, (float)29.635, "2023-05-16 11:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.62, (float)29.635, (float)29.635, "2023-05-16 11:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.62, (float)29.645, (float)29.635, "2023-05-16 11:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.63, (float)29.645, "2023-05-16 11:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.6225, (float)29.635, (float)29.63, "2023-05-16 11:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.645, (float)29.63, (float)29.64, (float)29.635, "2023-05-16 11:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.62, (float)29.625, (float)29.635, "2023-05-16 11:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.62, (float)29.69, (float)29.62, "2023-05-16 11:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.67, (float)29.685, (float)29.685, "2023-05-16 11:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.685, (float)29.66, (float)29.68, (float)29.685, "2023-05-16 11:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.66, (float)29.6725, (float)29.685, "2023-05-16 11:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.675, (float)29.66, (float)29.66, (float)29.675, "2023-05-16 11:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.665, (float)29.64, (float)29.64, (float)29.661, "2023-05-16 11:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.6025, (float)29.6025, (float)29.64, "2023-05-16 11:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.625, (float)29.59, (float)29.5999, (float)29.605, "2023-05-16 11:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.58, (float)29.585, (float)29.6, "2023-05-16 11:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.56, (float)29.575, (float)29.58, "2023-05-16 11:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.56, (float)29.5904, (float)29.575, "2023-05-16 11:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.59, (float)29.625, (float)29.595, "2023-05-16 11:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.6, (float)29.625, (float)29.625, "2023-05-16 11:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.615, (float)29.625, (float)29.625, "2023-05-16 11:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.61, (float)29.625, (float)29.625, "2023-05-16 11:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.6201, (float)29.645, (float)29.63, "2023-05-16 11:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.64, (float)29.64, "2023-05-16 11:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.64, (float)29.64, (float)29.64, "2023-05-16 11:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.64, (float)29.645, (float)29.6438, "2023-05-16 11:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.675, (float)29.64, (float)29.675, (float)29.65, "2023-05-16 11:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.675, (float)29.7, (float)29.68, "2023-05-16 11:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.68, (float)29.7, (float)29.7, "2023-05-16 11:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.68, (float)29.69, (float)29.7, "2023-05-16 11:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.725, (float)29.69, (float)29.71, (float)29.695, "2023-05-16 11:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.74, (float)29.705, (float)29.725, (float)29.705, "2023-05-16 11:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.75, (float)29.69, (float)29.695, (float)29.72, "2023-05-16 11:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.68, (float)29.6832, (float)29.695, "2023-05-16 11:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.675, (float)29.68, (float)29.68, "2023-05-16 11:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.67, (float)29.685, (float)29.685, "2023-05-16 11:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.68, (float)29.69, (float)29.685, "2023-05-16 11:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.67, (float)29.685, (float)29.69, "2023-05-16 11:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.6899, (float)29.67, (float)29.68, (float)29.685, "2023-05-16 12:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.67, (float)29.675, (float)29.675, "2023-05-16 12:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.66, (float)29.66, (float)29.67, "2023-05-16 12:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.63, (float)29.6491, (float)29.66, "2023-05-16 12:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.6498, (float)29.66, (float)29.6498, "2023-05-16 12:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.65, (float)29.665, (float)29.665, "2023-05-16 12:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.6871, (float)29.665, (float)29.675, (float)29.67, "2023-05-16 12:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.665, (float)29.6764, (float)29.675, "2023-05-16 12:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.6731, (float)29.68, (float)29.6762, "2023-05-16 12:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.67, (float)29.68, (float)29.685, "2023-05-16 12:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.675, (float)29.675, (float)29.68, "2023-05-16 12:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.675, (float)29.66, (float)29.6601, (float)29.675, "2023-05-16 12:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.675, (float)29.65, (float)29.673, (float)29.66, "2023-05-16 12:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.66, (float)29.665, (float)29.67, "2023-05-16 12:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.655, (float)29.665, (float)29.665, "2023-05-16 12:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.66, (float)29.665, (float)29.665, "2023-05-16 12:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.667, (float)29.64, (float)29.655, (float)29.665, "2023-05-16 12:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.655, (float)29.63, (float)29.645, (float)29.655, "2023-05-16 12:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.6451, (float)29.64, (float)29.645, (float)29.645, "2023-05-16 12:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.6301, (float)29.64, (float)29.645, "2023-05-16 12:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.64, (float)29.64, "2023-05-16 12:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.66, (float)29.635, (float)29.635, (float)29.635, "2023-05-16 12:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.635, (float)29.635, "2023-05-16 12:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.645, (float)29.635, "2023-05-16 12:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.66, (float)29.64, (float)29.655, (float)29.64, "2023-05-16 12:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.67, (float)29.64, (float)29.665, (float)29.65, "2023-05-16 12:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.66, (float)29.64, (float)29.65, (float)29.66, "2023-05-16 12:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.655, (float)29.64, (float)29.64, (float)29.65, "2023-05-16 12:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.6458, (float)29.64, "2023-05-16 12:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.645, (float)29.65, "2023-05-16 12:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.655, (float)29.64, (float)29.645, (float)29.645, "2023-05-16 12:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.6407, (float)29.625, (float)29.6332, (float)29.64, "2023-05-16 12:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.6407, (float)29.63, (float)29.635, (float)29.64, "2023-05-16 12:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.635, (float)29.64, (float)29.64, "2023-05-16 12:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.6561, (float)29.63, (float)29.644, (float)29.645, "2023-05-16 12:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.64, (float)29.6406, (float)29.645, "2023-05-16 12:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.6593, (float)29.635, (float)29.65, (float)29.64, "2023-05-16 12:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.652, (float)29.635, (float)29.641, (float)29.65, "2023-05-16 12:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.665, (float)29.64, (float)29.665, (float)29.645, "2023-05-16 12:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.6584, (float)29.66, (float)29.67, "2023-05-16 12:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.6888, (float)29.67, (float)29.68, (float)29.68, "2023-05-16 12:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.69, (float)29.67, (float)29.675, (float)29.68, "2023-05-16 12:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.66, (float)29.675, (float)29.675, "2023-05-16 12:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.68, (float)29.66, (float)29.6721, (float)29.67, "2023-05-16 12:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.665, (float)29.7, (float)29.67, "2023-05-16 12:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.7, (float)29.67, (float)29.69, (float)29.6903, "2023-05-16 12:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.685, (float)29.665, (float)29.6728, (float)29.685, "2023-05-16 12:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.6755, (float)29.66, (float)29.66, (float)29.67, "2023-05-16 12:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.665, (float)29.6305, (float)29.635, (float)29.665, "2023-05-16 12:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.63, (float)29.6475, (float)29.63, "2023-05-16 12:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.655, (float)29.63, (float)29.65, (float)29.6408, "2023-05-16 12:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.65, (float)29.61, (float)29.615, (float)29.65, "2023-05-16 12:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.6252, (float)29.6, (float)29.6252, (float)29.6144, "2023-05-16 12:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.575, (float)29.5878, (float)29.63, "2023-05-16 12:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.58, (float)29.58, (float)29.5863, "2023-05-16 12:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.595, (float)29.57, (float)29.585, (float)29.58, "2023-05-16 12:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.61, (float)29.58, (float)29.605, (float)29.58, "2023-05-16 12:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.6097, (float)29.6, (float)29.605, (float)29.605, "2023-05-16 12:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.6162, (float)29.6022, (float)29.615, (float)29.6022, "2023-05-16 12:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.63, (float)29.595, (float)29.63, (float)29.6137, "2023-05-16 12:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.64, (float)29.6225, (float)29.6225, (float)29.637, "2023-05-16 13:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.635, (float)29.605, (float)29.61, (float)29.63, "2023-05-16 13:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.62, (float)29.61, (float)29.615, (float)29.618, "2023-05-16 13:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.625, (float)29.605, (float)29.62, (float)29.615, "2023-05-16 13:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.62, (float)29.6, (float)29.61, (float)29.62, "2023-05-16 13:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.61, (float)29.5826, (float)29.5826, (float)29.605, "2023-05-16 13:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.575, (float)29.59, (float)29.585, "2023-05-16 13:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.5838, (float)29.587, (float)29.585, "2023-05-16 13:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.6, (float)29.5801, (float)29.585, (float)29.5801, "2023-05-16 13:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.59, (float)29.58, (float)29.5853, (float)29.59, "2023-05-16 13:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.585, (float)29.55, (float)29.5597, (float)29.585, "2023-05-16 13:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.545, (float)29.55, (float)29.555, "2023-05-16 13:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.555, (float)29.5137, (float)29.5137, (float)29.55, "2023-05-16 13:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.535, (float)29.515, (float)29.525, (float)29.52, "2023-05-16 13:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.535, (float)29.53, (float)29.535, (float)29.53, "2023-05-16 13:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.545, (float)29.52, (float)29.545, (float)29.53, "2023-05-16 13:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.5499, (float)29.53, (float)29.53, (float)29.54, "2023-05-16 13:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.55, (float)29.5309, (float)29.55, (float)29.5309, "2023-05-16 13:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.535, (float)29.54, (float)29.555, "2023-05-16 13:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.555, (float)29.53, (float)29.55, (float)29.53, "2023-05-16 13:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.5531, (float)29.54, (float)29.545, (float)29.55, "2023-05-16 13:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.56, (float)29.53, (float)29.535, (float)29.55, "2023-05-16 13:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.55, (float)29.5343, (float)29.54, (float)29.535, "2023-05-16 13:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.55, (float)29.53, (float)29.54, (float)29.54, "2023-05-16 13:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.545, (float)29.52, (float)29.52, (float)29.545, "2023-05-16 13:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.5299, (float)29.52, (float)29.52, (float)29.5209, "2023-05-16 13:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.5273, (float)29.5, (float)29.5141, (float)29.51, "2023-05-16 13:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.52, (float)29.4975, (float)29.505, (float)29.515, "2023-05-16 13:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.5086, (float)29.47, (float)29.47, (float)29.505, "2023-05-16 13:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.4782, (float)29.455, (float)29.46, (float)29.4706, "2023-05-16 13:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.465, (float)29.45, (float)29.455, (float)29.46, "2023-05-16 13:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.4409, (float)29.45, (float)29.45, "2023-05-16 13:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.4699, (float)29.45, (float)29.4699, (float)29.45, "2023-05-16 13:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.465, (float)29.49, (float)29.465, "2023-05-16 13:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.495, (float)29.46, (float)29.47, (float)29.49, "2023-05-16 13:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.475, (float)29.46, (float)29.465, (float)29.47, "2023-05-16 13:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.47, (float)29.455, (float)29.46, (float)29.4664, "2023-05-16 13:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.45, (float)29.45, (float)29.46, "2023-05-16 13:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.43, (float)29.44, (float)29.46, "2023-05-16 13:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.43, (float)29.44, (float)29.44, "2023-05-16 13:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.43, (float)29.4342, (float)29.43, "2023-05-16 13:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.43, (float)29.45, (float)29.44, "2023-05-16 13:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.4583, (float)29.44, (float)29.445, (float)29.455, "2023-05-16 13:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.41, (float)29.4199, (float)29.45, "2023-05-16 13:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.41, (float)29.425, (float)29.41, "2023-05-16 13:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.42, (float)29.45, (float)29.425, "2023-05-16 13:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.465, (float)29.43, (float)29.4605, (float)29.445, "2023-05-16 13:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.4615, (float)29.48, (float)29.47, "2023-05-16 13:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.497, (float)29.48, (float)29.495, (float)29.48, "2023-05-16 13:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.495, (float)29.48, (float)29.49, (float)29.495, "2023-05-16 13:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.4866, (float)29.46, (float)29.465, (float)29.48, "2023-05-16 13:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.482, (float)29.46, (float)29.47, (float)29.4601, "2023-05-16 13:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.465, (float)29.48, (float)29.475, "2023-05-16 13:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.4879, (float)29.4748, (float)29.4817, (float)29.48, "2023-05-16 13:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.475, (float)29.48, (float)29.485, "2023-05-16 13:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.47, (float)29.47, (float)29.48, "2023-05-16 13:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.47, (float)29.4799, (float)29.475, "2023-05-16 13:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.487, (float)29.47, (float)29.485, (float)29.4705, "2023-05-16 13:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.4899, (float)29.48, (float)29.48, (float)29.48, "2023-05-16 13:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.48, (float)29.485, (float)29.48, "2023-05-16 13:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.53, (float)29.485, (float)29.49, (float)29.49, "2023-05-16 14:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.47, (float)29.4701, (float)29.49, "2023-05-16 14:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.45, (float)29.45, (float)29.48, "2023-05-16 14:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.445, (float)29.46, (float)29.445, "2023-05-16 14:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.435, (float)29.44, (float)29.46, "2023-05-16 14:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.435, (float)29.4398, (float)29.445, "2023-05-16 14:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.445, (float)29.43, (float)29.435, (float)29.44, "2023-05-16 14:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.445, (float)29.41, (float)29.425, (float)29.44, "2023-05-16 14:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.425, (float)29.4, (float)29.41, (float)29.425, "2023-05-16 14:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.4252, (float)29.41, (float)29.42, (float)29.41, "2023-05-16 14:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.418, (float)29.38, (float)29.385, (float)29.418, "2023-05-16 14:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.41, (float)29.38, (float)29.4, (float)29.38, "2023-05-16 14:11:00"));
            list_DataPoints.Add(new DataPoint((float)29.4, (float)29.38, (float)29.4, (float)29.4, "2023-05-16 14:12:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.4001, (float)29.41, (float)29.4001, "2023-05-16 14:13:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.41, (float)29.415, (float)29.415, "2023-05-16 14:14:00"));
            list_DataPoints.Add(new DataPoint((float)29.425, (float)29.41, (float)29.425, (float)29.42, "2023-05-16 14:15:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.42, (float)29.425, (float)29.4299, "2023-05-16 14:16:00"));
            list_DataPoints.Add(new DataPoint((float)29.435, (float)29.42, (float)29.431, (float)29.425, "2023-05-16 14:17:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.43, (float)29.44, (float)29.435, "2023-05-16 14:18:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.42, (float)29.44, (float)29.44, "2023-05-16 14:19:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.435, (float)29.445, (float)29.435, "2023-05-16 14:20:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.42, (float)29.43, (float)29.45, "2023-05-16 14:21:00"));
            list_DataPoints.Add(new DataPoint((float)29.425, (float)29.41, (float)29.425, (float)29.425, "2023-05-16 14:22:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.39, (float)29.41, (float)29.43, "2023-05-16 14:23:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.405, (float)29.42, (float)29.405, "2023-05-16 14:24:00"));
            list_DataPoints.Add(new DataPoint((float)29.425, (float)29.4, (float)29.4101, (float)29.425, "2023-05-16 14:25:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.415, (float)29.42, (float)29.42, "2023-05-16 14:26:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.405, (float)29.405, (float)29.42, "2023-05-16 14:27:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.4, (float)29.42, (float)29.4, "2023-05-16 14:28:00"));
            list_DataPoints.Add(new DataPoint((float)29.425, (float)29.415, (float)29.425, (float)29.42, "2023-05-16 14:29:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.4117, (float)29.42, (float)29.425, "2023-05-16 14:30:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.38, (float)29.385, (float)29.415, "2023-05-16 14:31:00"));
            list_DataPoints.Add(new DataPoint((float)29.39, (float)29.375, (float)29.38, (float)29.3859, "2023-05-16 14:32:00"));
            list_DataPoints.Add(new DataPoint((float)29.395, (float)29.375, (float)29.39, (float)29.39, "2023-05-16 14:33:00"));
            list_DataPoints.Add(new DataPoint((float)29.4, (float)29.3865, (float)29.4, (float)29.3865, "2023-05-16 14:34:00"));
            list_DataPoints.Add(new DataPoint((float)29.405, (float)29.3801, (float)29.385, (float)29.4, "2023-05-16 14:35:00"));
            list_DataPoints.Add(new DataPoint((float)29.405, (float)29.38, (float)29.405, (float)29.38, "2023-05-16 14:36:00"));
            list_DataPoints.Add(new DataPoint((float)29.4114, (float)29.39, (float)29.395, (float)29.405, "2023-05-16 14:37:00"));
            list_DataPoints.Add(new DataPoint((float)29.415, (float)29.395, (float)29.411, (float)29.4, "2023-05-16 14:38:00"));
            list_DataPoints.Add(new DataPoint((float)29.42, (float)29.415, (float)29.42, (float)29.42, "2023-05-16 14:39:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.415, (float)29.43, (float)29.42, "2023-05-16 14:40:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.425, (float)29.44, (float)29.43, "2023-05-16 14:41:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.44, (float)29.45, (float)29.44, "2023-05-16 14:42:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.44, (float)29.4487, (float)29.45, "2023-05-16 14:43:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.44, (float)29.445, (float)29.45, "2023-05-16 14:44:00"));
            list_DataPoints.Add(new DataPoint((float)29.45, (float)29.44, (float)29.445, (float)29.44, "2023-05-16 14:45:00"));
            list_DataPoints.Add(new DataPoint((float)29.46, (float)29.4425, (float)29.455, (float)29.45, "2023-05-16 14:46:00"));
            list_DataPoints.Add(new DataPoint((float)29.47, (float)29.45, (float)29.46, (float)29.455, "2023-05-16 14:47:00"));
            list_DataPoints.Add(new DataPoint((float)29.465, (float)29.4501, (float)29.465, (float)29.4501, "2023-05-16 14:48:00"));
            list_DataPoints.Add(new DataPoint((float)29.47, (float)29.465, (float)29.4654, (float)29.465, "2023-05-16 14:49:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.46, (float)29.475, (float)29.46, "2023-05-16 14:50:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.475, (float)29.49, (float)29.48, "2023-05-16 14:51:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.48, (float)29.485, (float)29.4804, "2023-05-16 14:52:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.48, (float)29.4825, (float)29.4801, "2023-05-16 14:53:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.47, (float)29.48, (float)29.49, "2023-05-16 14:54:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.48, (float)29.4899, (float)29.485, "2023-05-16 14:55:00"));
            list_DataPoints.Add(new DataPoint((float)29.5, (float)29.48, (float)29.495, (float)29.4825, "2023-05-16 14:56:00"));
            list_DataPoints.Add(new DataPoint((float)29.5, (float)29.49, (float)29.5, (float)29.495, "2023-05-16 14:57:00"));
            list_DataPoints.Add(new DataPoint((float)29.4999, (float)29.4701, (float)29.475, (float)29.495, "2023-05-16 14:58:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.47, (float)29.485, (float)29.48, "2023-05-16 14:59:00"));
            list_DataPoints.Add(new DataPoint((float)29.495, (float)29.481, (float)29.485, (float)29.485, "2023-05-16 15:00:00"));
            list_DataPoints.Add(new DataPoint((float)29.485, (float)29.47, (float)29.48, (float)29.48, "2023-05-16 15:01:00"));
            list_DataPoints.Add(new DataPoint((float)29.49, (float)29.47, (float)29.485, (float)29.475, "2023-05-16 15:02:00"));
            list_DataPoints.Add(new DataPoint((float)29.495, (float)29.47, (float)29.475, (float)29.49, "2023-05-16 15:03:00"));
            list_DataPoints.Add(new DataPoint((float)29.48, (float)29.464, (float)29.4759, (float)29.47, "2023-05-16 15:04:00"));
            list_DataPoints.Add(new DataPoint((float)29.475, (float)29.4303, (float)29.44, (float)29.475, "2023-05-16 15:05:00"));
            list_DataPoints.Add(new DataPoint((float)29.445, (float)29.43, (float)29.4316, (float)29.44, "2023-05-16 15:06:00"));
            list_DataPoints.Add(new DataPoint((float)29.44, (float)29.415, (float)29.42, (float)29.43, "2023-05-16 15:07:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.42, (float)29.42, (float)29.4225, "2023-05-16 15:08:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.41, (float)29.415, (float)29.42, "2023-05-16 15:09:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.41, (float)29.41, (float)29.415, "2023-05-16 15:10:00"));
            list_DataPoints.Add(new DataPoint((float)29.43, (float)29.41, (float)29.425, (float)29.415, "2023-05-16 15:11:00"));
        }

    }
}