using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Java.Util.Concurrent;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;



namespace App1
{
    internal class StockAdapter : BaseAdapter<DataPoint>
    {
        Android.Content.Context context;
        List<DataPoint> objects;


        List<float> list_high = new List<float>();
        List<float> list_low = new List<float>();

        public StockAdapter(Context context, System.Collections.Generic.List<DataPoint> objects)
        {
            this.context = context;
            this.objects = objects;
        }
        public List<DataPoint> GetList()
        {
            return objects;
        }
        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            Android.Views.LayoutInflater layoutInflater = ((StockViewActivity)context).LayoutInflater;
            Android.Views.View view = layoutInflater.Inflate(Resource.Layout.ListView_Saved_Layout, parent, false);
            TextView tvSymbol, tvAlarmSound, tvTrackingPrices, tvLow, tvHeigh;

            tvSymbol = view.FindViewById<TextView>(Resource.Id.tvSymbol);
            tvLow = view.FindViewById<TextView>(Resource.Id.tvLow);
            tvHeigh = view.FindViewById<TextView>(Resource.Id.tvHeigh);


            tvAlarmSound = view.FindViewById<TextView>(Resource.Id.tvAlarmSound);
            tvTrackingPrices = view.FindViewById<TextView>(Resource.Id.tvTrackingPrices);


            DataPoint Temp = objects[position];
           

            if (Temp != null)
            {
                //Task t = testAsync(Temp);
                //t.Wait();

                tvSymbol.Text =   Temp.symbol;
                tvLow.Text = "Low: " + Temp.low;
                tvHeigh.Text = "Heigh: " +Temp.heigh;

            }
            return view;

        }

        public override int Count
        {
            get { return this.objects.Count; }
        }
        public override DataPoint this[int position]
        {
            get { return this.objects[position]; }
        }


        //private  async Task testAsync(String symbol)
        //{
        //    Toast.MakeText(context, "1", ToastLength.Short).Show();
        //    using (var httpClient2 = new HttpClient())
        //    {
        //        Toast.MakeText(context, "2", ToastLength.Short).Show();
        //        using (var request2 = new HttpRequestMessage(new HttpMethod("GET"), "https://financialmodelingprep.com/api/v3/historical-chart/1min/AAPL?apikey=0a0b32a8d57dc7a4d38458de98803860"))
        //        {
        //            Toast.MakeText(context, "3", ToastLength.Short).Show();

        //            var response2 = await httpClient2.SendAsync(request2);
        //            response2.EnsureSuccessStatusCode();
        //            string responseBody = await response2.Content.ReadAsStringAsync();

        //            Toast.MakeText(context, "4", ToastLength.Short).Show();

        //            JSONArray HistInfo = new JSONArray(responseBody);
        //            Console.WriteLine(HistInfo.Length());

        //            list_low = new List<float>();
        //            list_high = new List<float>();

        //            for (int i = 0; i < HistInfo.Length(); i++)
        //            {
        //                //Console.WriteLine(avr);
        //                list_low.Add((float)(HistInfo.GetJSONObject(i).GetDouble("low")));
        //                list_high.Add((float)(HistInfo.GetJSONObject(i).GetDouble("high")));

        //            }
        //        }
        //    }
        //    Toast.MakeText(context, "5", ToastLength.Short).Show();
        //    return;
        //}
    }
}