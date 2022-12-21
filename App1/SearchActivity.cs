﻿using Android.App;
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



namespace App1
{
    [Activity(Label = "SearchActivity")]
    public class SearchActivity : Activity
    {
        EditText etSearch;
        Button btnSearch;
        ListView lvSearchedStocks;

        public static List<ClassSearchStock> SearchDatalist = new List<ClassSearchStock>();
        SearchStockAdapter adapter;

        List<DataPoint> Chart_Points = new List<DataPoint>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearchLayout);
            etSearch = FindViewById<EditText>(Resource.Id.etSearch);
            btnSearch = FindViewById<Button>(Resource.Id.btnSearch);

            btnSearch.Click += BtnSearch_Click;


            //8bdedb14d7674def460cb3a84f1fd429
            //0a0b32a8d57dc7a4d38458de98803860

            etSearch.AfterTextChanged += EtSearch_AfterTextChanged;



            //SearchDatalist.Add(new ClassSearchStock("AAPL", "inc appl", (float)143.5));
            //ShowListView();

            // Create your application here
        }

        private void EtSearch_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            ThreadStart MyThreadStart = new ThreadStart(RefreshList);
            Thread t = new Thread(MyThreadStart);
            t.Start();
            //t.Join();

            test();
            //ShowListView();
        }

        public void test()
        {
            //Thread.Sleep(1000);
            ShowListView();
        }
        public void RefreshList()
        {
            if (SearchDatalist.Count > 0)
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

        private void BtnSearch_Click(object sender, EventArgs e)
        {

            //if (SearchDatalist.Count > 0)
            //{
            //    SearchDatalist.Clear();
            //}
            //String Stock_name = etSearch.Text;
            //if (Stock_name != null && Stock_name != "" && Regex.IsMatch(Stock_name, @"^[a-zA-Z]+$"))
            //{
            //    _ = GetCorrespondingStocks(Stock_name);
            //}

            //RefreshList();

            //Console.Write(SearchDatalist);
            //ShowListView();

        }



        public async System.Threading.Tasks.Task GetCorrespondingStocks(String symbol)
        {
            using (var httpClient2 = new HttpClient())
            {
                string link = "https://financialmodelingprep.com/api/v3/search?query=";
                link = link.Insert(link.Length, symbol);
                //link = link.Insert(link.Length, "&limit=10&exchange=NASDAQ&apikey=0a0b32a8d57dc7a4d38458de98803860");
                link = link.Insert(link.Length, "&limit=10&exchange=NASDAQ&apikey=8bdedb14d7674def460cb3a84f1fd429");
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    //Toast.MakeText(this, "sending requast for info", ToastLength.Short).Show();
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
                        //SearchDatalist.Add(new ClassSearchStock(sym,compName,price,urlImage));
                        SearchDatalist.Add(new ClassSearchStock(sym, compName));
                        _ = GetImageAndPrice_FromWeb(i);
                        Thread.Sleep(0500);
                    }

                }
                //Toast.MakeText(this, "got symbol and company name", ToastLength.Short).Show();
                Console.WriteLine("got symbol and company name");
                ShowListView();
                
                //for (int i = 0; i < SearchDatalist.Count; i++)
                //{
                //   _ = GetImageAndPrice_FromWeb(i);
                //}

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
                    SearchDatalist[place].price = price;
                    SearchDatalist[place].StockImage = urlImage;
                }
                //Toast.MakeText(this, "got symbol and company name", ToastLength.Short).Show();
                Console.WriteLine("got image and price");

            }

            //ShowListView();
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
            adapter = new SearchStockAdapter(this, SearchDatalist);
            if(lvSearchedStocks == null)
            {
                lvSearchedStocks = (ListView)FindViewById(Resource.Id.lvSearchedStoks);
            }
            lvSearchedStocks.Adapter = adapter;

            lvSearchedStocks.ItemClick += LvSearchedStocks_ItemClick;
        }

        private void LvSearchedStocks_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Console.WriteLine("clicked!");
        }
    }
}