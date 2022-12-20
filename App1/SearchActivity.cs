using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using System.Text.RegularExpressions;
//using static Android.Renderscripts.ScriptGroup;

using System.Threading.Tasks;



namespace App1
{
    [Activity(Label = "SearchActivity")]
    public class SearchActivity : Activity
    {
        EditText etSearch;
        Button btnSearch;

        List<DataPoint> Chart_Points = new List<DataPoint>();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearchLayout);
            etSearch = FindViewById<EditText>(Resource.Id.etSearch);
            btnSearch = FindViewById<Button>(Resource.Id.btnSearch);

            btnSearch.Click += BtnSearch_Click;

           
            // Create your application here
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            String symbol = etSearch.Text;
            if (symbol != null && symbol != "" && Regex.IsMatch(symbol, @"^[a-zA-Z]+$"))
            {
                _ = GetInfoFromWeb(symbol);
            }
        }

        

        public async System.Threading.Tasks.Task GetInfoFromWeb(String symbol)
        {
            using (var httpClient2 = new HttpClient())
            {
                string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
                link = link.Insert(link.Length, symbol);
                link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");

                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    Toast.MakeText(this, "sending requast for info", ToastLength.Short).Show();
                    var response2 = await httpClient2.SendAsync(request);
                    response2.EnsureSuccessStatusCode();

                    string responseBody = await response2.Content.ReadAsStringAsync();
                    JSONArray HistInfo = new JSONArray(responseBody);

                    Console.WriteLine("--------------------------");
                    Console.WriteLine(HistInfo.Length());
                    Console.WriteLine("--------------------------");

                    for (int i = 0; i < HistInfo.Length(); i++)
                    {
                        Chart_Points.Add(new DataPoint((float)HistInfo.GetJSONObject(0).GetDouble("low"), (float)HistInfo.GetJSONObject(0).GetDouble("high"), (string)HistInfo.GetJSONObject(0).Get("date")));
                    }

                    Toast.MakeText(this, "got the info from web", ToastLength.Short).Show();
                    Console.WriteLine("got the info from web");
                    MoveToChartActivity();
                }
            }


            Dispose(true);
            return;
        }

        public void MoveToChartActivity()
        {
            //List<String> Chart_Points_Date = new List<String>();
            //List<float> Chart_Points_Heigh = new List<float>();
            //List<float> Chart_Points_Low = new List<float>();

            Intent intent = new Intent(this, typeof(ChartActivity));
            //intent.PutExtra("Chart_Points_Date", Chart_Points_Date);
            //intent.PutParcelableArrayListExtra("Chart_Points_Date", Chart_Points_Date);
            

            intent.PutExtra("symbol", etSearch.Text);
            //intent.PutStringArrayListExtra("Chart_Points_Date", Chart_Points_Date);
            //intent.PutIntegerArrayListExtra("Chart_Points_Low", Chart_Points_Low);
            //intent.PutIntegerArrayListExtra("Chart_Points_Heigh", Chart_Points_Heigh);


            StartActivity(intent);
        }
    }
}