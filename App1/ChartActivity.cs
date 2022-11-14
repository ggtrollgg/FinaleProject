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


namespace App1
{
    [Activity(Label = "ChartActivity")]
    public class ChartActivity : Activity
    {
        List<float> list = new List<float>();
        List<string> list_Dates = new List<string>();

        Button btnMove, btnZoom;
        LinearLayout l1;
        StockChart chart;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ChartLayout);
            btnMove = FindViewById<Button>(Resource.Id.btnMove);
            btnZoom = FindViewById<Button>(Resource.Id.btnZoom);

            l1 = FindViewById<LinearLayout>(Resource.Id.LLChart);

            
            btnZoom.Click += BtnZoom_Click;
            btnMove.Click += BtnMove_Click;

            chart = new StockChart(this);

            Console.WriteLine("1");
            _ = testAsync();


        }

        private void BtnMove_Click(object sender, EventArgs e)
        {
            chart.Move = !chart.Move;
        }

        private void BtnZoom_Click(object sender, EventArgs e)
        {
            chart.Zoom = !chart.Zoom;
            if (chart.Zoom)
            {
                btnZoom.Background = null;
            }
        }

        public void test()
        {
            
            float[] arrey = new float[list.Count];
            for (int i = 0; i < arrey.Length; i++)
            {
                arrey[i] = list[i];
            }
            chart.values = arrey;
            l1.AddView(chart);
        }


        public async Task testAsync()
        {
            Console.WriteLine("2");
            using (var httpClient = new HttpClient())

            {
                // using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://financialmodelingprep.com/api/v3/quote-short/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860"))
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://financialmodelingprep.com/api/v3/historical-chart/1min/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860"))
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
                        Console.WriteLine((string)(HistInfo.GetJSONObject(i).Get("date")));
                    }


                }
            }
            Console.WriteLine("3");
            test();
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