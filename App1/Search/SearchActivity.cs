using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Java.Util;
using Java.Util.Functions;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;

using System.Text.RegularExpressions;
using System.Threading;
//using static Android.Renderscripts.ScriptGroup;

using System.Threading.Tasks;
using CancellationTokenSource = System.Threading.CancellationTokenSource;

namespace App1
{
    [Activity(Label = "SearchActivity")]
    public class SearchActivity : Activity
    {
        EditText etSearch;
        Button btnHome;
        ListView lvSearchedStocks;

        public static List<ClassSearchStock> SearchDatalist = new List<ClassSearchStock>();
        SearchStockAdapter adapter;

        CancellationTokenSource CTS = new CancellationTokenSource();
        List<DataPoint> Chart_Points = new List<DataPoint>();
        Dialog d;
        LinearLayout l1;
        Class_LineGraph MiniGraph;

        String lastSearch;
        int LongClick_pos;

        Context content;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearchLayout);
            content = this;
            lastSearch = "";
            etSearch = FindViewById<EditText>(Resource.Id.etSearch);
            btnHome = FindViewById<Button>(Resource.Id.btnHome);

            //btnSearch.Click += BtnSearch_Click;

            lvSearchedStocks = (ListView)FindViewById(Resource.Id.lvSearchedStoks);
            lvSearchedStocks.ItemClick += LvSearchedStocks_ItemClick;
            lvSearchedStocks.ItemLongClick += LvSearchedStocks_ItemLongClick;

            //8bdedb14d7674def460cb3a84f1fd429
            //0a0b32a8d57dc7a4d38458de98803860

            //etSearch.AfterTextChanged += EtSearch_AfterTextChanged;

            etSearch.TextChanged += EtSearch_TextChanged;
            btnHome.Click += BtnHome_Click;

        }

        //buttons
        private void BtnHome_Click(object sender, EventArgs e)
        {
            Finish();
        }
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            RefreshList();
        }


        //-------mini graph------
        //which item was clicked and what symbol it has, and create popup
        private void LvSearchedStocks_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            LongClick_pos= e.Position;
            d = new Dialog(content);

            d.SetContentView(Resource.Layout.Custom_PopUp_MiniGraph);
            d.SetTitle(SearchDatalist[LongClick_pos].symbol);
            d.SetCancelable(true);
            l1 = d.FindViewById<LinearLayout>(Resource.Id.LLChart);


            MiniGraph = new Class_LineGraph(this);
            if (Chart_Points.Count > 0)
            {
                Chart_Points.Clear();
            }
            _ = GetPricePoints(SearchDatalist[LongClick_pos].symbol);
            //activatePopUp();



        }


        //get prices of said symbol 
        public async System.Threading.Tasks.Task GetPricePoints(string symbol)
        {
            using (var httpClient = new HttpClient())

            {
                string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
                link = link.Insert(link.Length, symbol);
                //link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");
                // using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://financialmodelingprep.com/api/v3/quote-short/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860"))
                //using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://financialmodelingprep.com/api/v3/historical-chart/1min/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860"))

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
                    JSONArray HistInfo = new JSONArray(responseBody);

                    Console.WriteLine(HistInfo.Length());
                    if(Chart_Points.Count > 0)
                    {
                        Chart_Points.Clear();
                    }
                    for (int i = 0; i < HistInfo.Length(); i++)
                    {
                        Chart_Points.Add(new DataPoint((float)HistInfo.GetJSONObject(i).GetDouble("high"), (float)HistInfo.GetJSONObject(i).GetDouble("low"), (float)HistInfo.GetJSONObject(i).GetDouble("close"), (float)HistInfo.GetJSONObject(i).GetDouble("open"),(string)HistInfo.GetJSONObject(i).Get("date")));
                    }

                }
            }
            activatePopUp();
            //Dispose();
            return;
        }


        //put prices and dates of symbol in minigraph and activate popup
        public void activatePopUp()
        {
            MiniGraph.dataPoints = Chart_Points;
            d.Show();
            l1.AddView(MiniGraph);
            
        }



        //search for closest 10 stocks if text was changed in edittext box
        //if cancaktuin token is cancenlble => meaning a reqwest was already made => i can stop it and not waste resorces on it
        private void EtSearch_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (lastSearch != null && lastSearch != etSearch.Text)
            {
                if(CTS.Token.CanBeCanceled)
                {
                    CTS.Cancel();
                    CTS.Dispose();
                    CTS = new CancellationTokenSource();
                }
                if (etSearch.Text == "")
                {
                    lastSearch = "";
                    if (SearchDatalist != null && SearchDatalist.Count > 0)
                    {
                        SearchDatalist.Clear();
                        ShowListView(); // it will be empty
                    }
                }
                else
                {
                    lastSearch = etSearch.Text;
                    RefreshList();//refresh to the new closest 10 stocks
                }

            }
        }

        //private void EtSearch_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        //{
            
        //    if (lastSearch != null && lastSearch != etSearch.Text)
        //    {
        //        if (etSearch.Text == "")
        //        {
        //            lastSearch = "";
        //            if (SearchDatalist != null && SearchDatalist.Count > 0)
        //            {
        //                SearchDatalist.Clear();
        //                ShowListView();
        //            }
        //        }
        //        else
        //        {
        //            lastSearch = etSearch.Text;
        //            RefreshList();
        //        }

        //    }
        //}

        public void RefreshList()
        {
            if (SearchDatalist != null && SearchDatalist.Count > 0)
            {
                SearchDatalist.Clear();
            }

            String Stock_name = etSearch.Text;

            if (Stock_name != null && Stock_name != "" && Regex.IsMatch(Stock_name, @"^[a-zA-Z]+$"))
            {
                Console.WriteLine(Stock_name);
                _ = GetCorrespondingStocks(Stock_name);
            }
        }

        


        //reqwesting the 10 closest stocks to the text in the text box from the internet
        public async System.Threading.Tasks.Task GetCorrespondingStocks(String symbol)
        {
            using (var httpClient2 = new HttpClient())
            {
                string link = "https://financialmodelingprep.com/api/v3/search?query=";
                link = link.Insert(link.Length, symbol);
                //link = link.Insert(link.Length, "&limit=10&exchange=NASDAQ&apikey=0a0b32a8d57dc7a4d38458de98803860");
                //link = link.Insert(link.Length, "&limit=10&exchange=NASDAQ&apikey=8bdedb14d7674def460cb3a84f1fd429");

                API_Key k = MainActivity.Manager_API_Keys.GetBestKey();
                if (k != null && k.Key != "" && k.GetCallsRemaining() > 0)
                {
                    link = link.Insert(link.Length, "&limit=10&exchange=NASDAQ&apikey=" + k.Key);
                }
                else
                {
                    Console.WriteLine("there was a problem with the keys at stockview activity ");
                    return;
                }
                

                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    //Toast.MakeText(this, "sending requast for info", ToastLength.Short).Show();
                    MainActivity.Manager_API_Keys.UseKey(k.Key);
                    var response2 = await httpClient2.SendAsync(request,CTS.Token);
                    
                    var temp = response2.StatusCode;
                    if (temp == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Toast.MakeText(this, "the serever is full right now, try again later", ToastLength.Long).Show();
                        return;
                    }
                    response2.EnsureSuccessStatusCode();

                    string responseBody = await response2.Content.ReadAsStringAsync();
                    JSONArray Data = new JSONArray(responseBody);

                    Console.WriteLine("--------------------------");
                    Console.WriteLine(Data.Length());
                    Console.WriteLine("--------------------------");

                    for(int i = 0; i < Data.Length(); i++)
                    {
                        String sym = (String)Data.GetJSONObject(i).Get("symbol");
                        String compName = (String)Data.GetJSONObject(i).Get("name");

                        SearchDatalist.Add(new ClassSearchStock(sym, compName));


                       // _ = GetImageAndPrice_FromWeb(i);
                        //Thread.Sleep(0500);
                    }

                }
                //Toast.MakeText(this, "got symbol and company name", ToastLength.Short).Show();

                Console.WriteLine("got symbol and company name");
                ShowListView();//Show the new 10 closest stocks
            }
            Dispose(true);
            return;
        }


        //when i call for information i can get specific info for each line,
        //so when i want x of closest stocks i dont get much info aside from name and descripation
        //so this is to get the image of the stock from web and for the current price of the stock
        public async System.Threading.Tasks.Task GetImageAndPrice_FromWeb(int place)
        {
            using (var httpClient = new HttpClient())
            {

                string link = "https://financialmodelingprep.com/api/v3/profile/";
                link = link.Insert(link.Length, SearchDatalist[place].symbol);
                //link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");
                link = link.Insert(link.Length, "?apikey=8bdedb14d7674def460cb3a84f1fd429");


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
                    //SearchDatalist.Add(new ClassSearchStock(sym,compName,price,urlImage));

                    SearchDatalist[place].companyName = compName;
                    //SearchDatalist[place].price = price;
                    //SearchDatalist[place].StockImage = urlImage;
                }
                //Toast.MakeText(this, "got symbol and company name", ToastLength.Short).Show();
                Console.WriteLine("got image and price");

            }

            //ShowListView();
            Dispose(true);
            return;
        }



        //moving to the chart activity
        public void MoveToChartActivity(String symbol)
        {
            Intent intent = new Intent(this, typeof(ChartActivity));
            intent.PutExtra("symbol", symbol);
            StartActivity(intent);
        }


        //refresh/show the listview
        private void ShowListView()
        {
            adapter = new SearchStockAdapter(this, SearchDatalist);
            lvSearchedStocks.Adapter = adapter;
        }


        //detect which item was clicked and than move to the chart activity of the stock
        private void LvSearchedStocks_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Console.WriteLine("clicked! moving to chartActivity");
            Console.WriteLine("symbol clicked: " + SearchDatalist[e.Position].symbol);
            MoveToChartActivity(SearchDatalist[e.Position].symbol);
        }
    }
}