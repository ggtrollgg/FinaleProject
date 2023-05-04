using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using App1.Algorithm;
using Java.Util;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App1
{
    [Activity(Label = "Algorithm_Test_Activity")]
    public class Algorithm_Test_Activity : Activity
    {
        public List<DataPoint> list_dataPoints = new List<DataPoint>();
        LinearLayout l1, algoLL;

        Button btnTrack, btnCancel, btnStart;
        RadioButton RB1min, RB5min, RB15min, RB30min, RB1hour;
        List<RadioButton> radioButtons = new List<RadioButton>();

        CheckBox CBdoubleAver, CBdrawProcess;

        EditText etTrackingPrices, ETdegree, ETfuterPoint;



        ImageButton ibHome, ibSave, ibTrack, ibData, ibType;


        //StockChart chart;
        Class_LineGraph chart2;

        Dialog d;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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

            ETdegree = d.FindViewById<EditText>(Resource.Id.ETdegree);
            ETfuterPoint = d.FindViewById<EditText>(Resource.Id.ETfuterPoint);

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

            d.Show();

        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            
            foreach(RadioButton rb in radioButtons)
            {
                if (rb.Checked)
                {
                    string buttonText = rb.Text.Trim();
                    string timeleap = buttonText.Replace(" ", "");

                    //Console.WriteLine("-------------------------");
                    //Console.WriteLine(timeleap);
                    //Console.WriteLine("-------------------------");

                    Task<List<DataPoint>> ldp = getInfoFromWeb(timeleap);
                    //ldp.Wait();
                    //ldp.ConfigureAwait(true);

                    //if (ldp.IsCompleted)
                    //{
                    //    if(ldp.Result != null)
                    //    {
                    //        MATL_Algorithm MATLAlgo = new MATL_Algorithm(ldp.Result, int.Parse(ETdegree.Text));
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

                    //        }
                    //    }
                    //}

                    break;
                }
            }
            //need to get new series of points from web :(
            
        }



        public async Task<List<DataPoint>> getInfoFromWeb(String timeLeap)
        {
            List<DataPoint> newList = new List<DataPoint>();
            using (var httpClient = new HttpClient())
            {
                

                string symbol = Intent.GetStringExtra("symbol") ?? "AAPL";

                //Intent intent = new Intent(this, typeof(SearchActivity));

                //Intent.GetFloatArrayExtra("Chart_Points_Heigh");
                //Intent.GetFloatArrayExtra("Chart_Points_Low");
                //Intent.GetStringArrayExtra("Chart_Points_Date");

                //string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";

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
            MATL_Algorithm MATLAlgo = new MATL_Algorithm(newList, int.Parse(ETdegree.Text));
            while (MATLAlgo.movingAverage_Graph.Count < int.Parse(ETdegree.Text))
            {
                Thread.Sleep(1000);
            }

            if (CBdrawProcess.Checked)
            {
                Console.WriteLine("CBdrawProcess is checked");
                algoLL.Visibility = ViewStates.Visible;
                MA_View graph_view = new MA_View(this, MATLAlgo.movingAverage_Graph);
                if(algoLL.RootView != null)
                {
                    algoLL.RemoveAllViews();
                }
                algoLL.AddView(graph_view);

            }
        }
    }

}