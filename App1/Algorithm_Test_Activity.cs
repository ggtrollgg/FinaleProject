using Android.App;
using Android.Content;
using Android.Gms.Common.Api.Internal;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using App1.Algorithm;
using Java.Lang;
using Java.Util;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
//using static Android.Bluetooth.BluetoothClass;
//using static System.Net.Mime.MediaTypeNames;

namespace App1
{
    [Activity(Label = "Algorithm_Test_Activity")]
    public class Algorithm_Test_Activity : Activity
    {
        public List<DataPoint> list_dataPoints = new List<DataPoint>();
        LinearLayout l1;
        Button btnTrack, btnCancel;
        EditText etTrackingPrices;
        ImageButton ibHome, ibSave, ibTrack, ibData, ibType;
        //StockChart chart;
        Class_LineGraph chart2;


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

        Dialog d;
        MATL_Algorithm MATLAlgo;

        string timeleap = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            hand = new MyHandler(this);
            Hand_Progress = new ProgressBarHandler(this);
            context = this;
            StartDialog();
            // Create your application here
        }
        

        private void StartDialog()
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
            if(MATLAlgo != null)
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
            if(ETdegree.Text.Length < 1)
            {
                Console.WriteLine("need to fill the order field ");
                return;
            }
            if(ETfuterPoint.Text.Length < 1)
            {
                Console.WriteLine("need to fill the futer point field ");
                return;
            }


            MATLAlgo = new MATL_Algorithm(newList, int.Parse(ETdegree.Text), int.Parse(ETfuterPoint.Text),this);

            if(ETMinOrder.Text != "")
            {
                if(int.Parse(ETMinOrder.Text) > int.Parse(ETdegree.Text))
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
        protected override void OnPause()
        {

            base.OnPause();
        }
    }

}