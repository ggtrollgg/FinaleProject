using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Java.Util;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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
        ListView lvSearchedStocks;

        public static List<ClassSearchStock> Datalist = new List<ClassSearchStock>();
        SearchStockAdapter adapter;

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
            if(Datalist.Count > 0)
            {
                Datalist.Clear();
            }
            String Stock_name = etSearch.Text;
            if (Stock_name != null && Stock_name != "" && Regex.IsMatch(Stock_name, @"^[a-zA-Z]+$"))
            {
                _ = GetCorrespondingStocks(Stock_name);
            }
        }

        

        public async System.Threading.Tasks.Task GetCorrespondingStocks(String symbol)
        {
            using (var httpClient2 = new HttpClient())
            {
                //string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
                //link = link.Insert(link.Length, symbol);
                //link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");


                string link = "https://financialmodelingprep.com/api/v3/search?query=";
                link = link.Insert(link.Length, symbol);
                link = link.Insert(link.Length, "&limit=10&exchange=NASDAQ&apikey=0a0b32a8d57dc7a4d38458de98803860");
                


                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    Toast.MakeText(this, "sending requast for info", ToastLength.Short).Show();
                    var response2 = await httpClient2.SendAsync(request);
                    response2.EnsureSuccessStatusCode();

                    string responseBody = await response2.Content.ReadAsStringAsync();
                    JSONArray Data = new JSONArray(responseBody);

                    Console.WriteLine("--------------------------");
                    Console.WriteLine(Data.Length());
                    Console.WriteLine("--------------------------");

                    for(int i = 0; i < Data.Length(); i++)
                    {
                        String sym = (string)Data.GetJSONObject(i).Get("symbol");
                        String compName = (string)Data.GetJSONObject(i).Get("name");

                        //float price = (float)Data.GetJSONObject(i).GetDouble("price");
                        //String urlImage = (string)Data.GetJSONObject(i).Get("image");
                        //Datalist.Add(new ClassSearchStock(sym,compName,price,urlImage));
                        Datalist.Add(new ClassSearchStock(sym, compName));
                    }

                }
                Toast.MakeText(this, "got symbol and company name", ToastLength.Short).Show();
                Console.WriteLine("got symbol and company name");

                for (int i = 0; i < Datalist.Count; i++)
                {
                    _ = GetImageAndPrice_FromWeb(i);
                }

                //ShowListView();
            }


            Dispose(true);
            return;
        }

        public async System.Threading.Tasks.Task GetImageAndPrice_FromWeb(int place)
        {
            using (var httpClient = new HttpClient())
            {

                string link = "https://financialmodelingprep.com/api/v3/profile/";
                link = link.Insert(link.Length, Datalist[place].symbol);
                link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");



                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    //Toast.MakeText(this, "sending requast for info", ToastLength.Short).Show();
                    var response2 = await httpClient.SendAsync(request);
                    response2.EnsureSuccessStatusCode();

                    string responseBody = await response2.Content.ReadAsStringAsync();
                    JSONArray Data = new JSONArray(responseBody);

                    String compName = (string)Data.GetJSONObject(0).Get("companyName");
                    float price = (float)Data.GetJSONObject(0).GetDouble("price");
                    String urlImage = (string)Data.GetJSONObject(0).Get("image");
                    //Datalist.Add(new ClassSearchStock(sym,compName,price,urlImage));

                    Datalist[place].companyName = compName;
                    Datalist[place].price = price;
                    Datalist[place].StockImage = urlImage;
                }
                Toast.MakeText(this, "got symbol and company name", ToastLength.Short).Show();
                Console.WriteLine("got symbol and company name");

            }
            ShowListView();
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

        private void ShowListView()
        {
            adapter = new SearchStockAdapter(this, Datalist);
            lvSearchedStocks = (ListView)FindViewById(Resource.Id.lvSearchedStoks);
            lvSearchedStocks.Adapter = adapter;
        }
    }
}