﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Org.Apache.Http.Cookies;
using Org.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    [Activity(Label = "StockViewActivity")]
    public class StockViewActivity : Activity
    {
        Button btnReturnHome, btnShowSaved;

        public static List<String> list = new List<String>();
        public static List<DataPoint> Datalist = new List<DataPoint>();

        ListView lvStock;
        StockAdapter adapter;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ListViewPage_Layout);
            btnReturnHome = FindViewById<Button>(Resource.Id.btnReturnHome);
            btnShowSaved = FindViewById<Button>(Resource.Id.btnShowSaved);

            btnReturnHome.Click += BtnReturnHome_Click;
            list = new List<String>();
            Datalist = new List<DataPoint>();

            //DeleteFile("SavedStocks.txt");
            //Add_To_File("AAPL");
            //Add_To_File("TSLA");
            

            Console.WriteLine(Read_from_file());

           _ = processAllSavedStocks();
        }

        private async Task processAllSavedStocks()
        {
            String s = Read_from_file();
            s = s.Replace("\0", "");
            s = s.Replace("\n", "");

            String[] s2 = s.Split(',');
            Console.WriteLine(s2);
            Console.WriteLine("------------------------------------------------------------------------------------------");
            for (int i = 0; i < s2.Length; i++)
            {
                if (s2[i] != null && s2[i].Length!= 0)
                {
                    list.Add(s2[i]);
                    Console.WriteLine(s2[i]);
                    Console.WriteLine("------------------------------------------------------------------------------------------");
                    DataPoint d = new DataPoint();
                    d.symbol = s2[i];
                    Datalist.Add(d);
                    _ = GetInfoFromWeb(s2[i].ToString(), i);
                }
            }

        }

        private async Task GetInfoFromWeb(string symbol,int place)
        {
            using (var httpClient2 = new HttpClient())
            {

                symbol = symbol.Replace("\0","");
                symbol = symbol.Replace("\n", "");
                symbol = symbol.Replace(",", "");

                string link = "https://financialmodelingprep.com/api/v3/historical-chart/1min/";
                link = link.Insert(link.Length, symbol);
                link = link.Insert(link.Length, "?apikey=0a0b32a8d57dc7a4d38458de98803860");

                using (var request2 = new HttpRequestMessage(new HttpMethod("GET"), link))
                {
                    //Toast.MakeText(this, "sending requast for info", ToastLength.Short).Show();
                    var response2 = await httpClient2.SendAsync(request2);
                    response2.EnsureSuccessStatusCode();
                    string responseBody = await response2.Content.ReadAsStringAsync();
                    JSONArray HistInfo = new JSONArray(responseBody);

                    Console.WriteLine(HistInfo.Length());

                    Datalist[place].low=((float)(HistInfo.GetJSONObject(0).GetDouble("low")));
                    Datalist[place].heigh =((float)(HistInfo.GetJSONObject(0).GetDouble("high")));
                    Datalist[place].date = ((string)(HistInfo.GetJSONObject(0).Get("date")));

                }
            }

            Toast.MakeText(this, "got the info from web?", ToastLength.Short).Show();
            if (place >= Datalist.Count-2)
            { 
                ShowListView();
            }
            return;
        }

        private void ShowListView()
        {
            adapter = new StockAdapter(this, Datalist);
            lvStock = (ListView)FindViewById(Resource.Id.lvStock);
            lvStock.Adapter = adapter;
        }

        

        private void BtnReturnHome_Click(object sender, EventArgs e)
        {
            Finish();
        }

        private String Read_from_file()
        {
            try
            {
                string str;
                //using (Stream stream = OpenFileOutput("Emailinfo.txt", Android.Content.FileCreationMode.Private))
                using (Stream stream = OpenFileInput("SavedStocks.txt"))
                {
                    try
                    {
                        byte[] buffer = new byte[4096];
                        stream.Read(buffer, 0, buffer.Length);
                        str = System.Text.Encoding.UTF8.GetString(buffer);
                        stream.Close();
                        if (str != null)
                        {
                            //  tv.Text = str;
                            //Toast.MakeText(this, str, ToastLength.Short).Show();
                            return str;
                        }
                    }
                    catch (Java.IO.IOException a)
                    {
                        a.PrintStackTrace();
                    }
                }
            }
            catch (Java.IO.FileNotFoundException a)
            {
                a.PrintStackTrace();

            }

            return null;
        }

        private void Add_To_File(String the_stock)
        {
            try
            {
                string str = the_stock + ",";//= et.Text;
                using (Stream stream = OpenFileOutput("SavedStocks.txt", Android.Content.FileCreationMode.Append))
                {
                    try
                    {
                        if (!Is_Allready_In_File(the_stock))
                        {
                            stream.Write(Encoding.ASCII.GetBytes(str), 0, str.Length);
                            stream.Close();
                            Toast.MakeText(this, "saved", ToastLength.Short).Show();
                        }
                    }
                    catch (Java.IO.IOException a)
                    {
                        a.PrintStackTrace();
                    }
                }
            }
            catch (Java.IO.FileNotFoundException a)
            {
                a.PrintStackTrace();
            }
            
        }

        private bool Is_Allready_In_File(string the_stock)
        {
            string str = Read_from_file();
            if (str== null || str.Contains(the_stock))
            {
                Toast.MakeText(this, "all ready in", ToastLength.Short).Show();
                return true;
            }
            return false;
        }

        


    }
}