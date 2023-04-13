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
    internal class StockAdapter : BaseAdapter<StockData>
    {
        Android.Content.Context context;
        List<StockData> objects;

        public StockAdapter(Context context, System.Collections.Generic.List<StockData> objects)
        {
            this.context = context;
            this.objects = objects;
        }
        public List<StockData> GetList()
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
            Android.Views.View view = SavedLayout(position, convertView, parent);
            StockData Temp = objects[position];

            if (Temp!=null && Temp.TrackingPrices!=null )//&& Temp.SoundName != null && Temp.SoundName != "")
            {
                view = TrackingLayout(position, convertView, parent);
            }
            return view;

        }

        private View TrackingLayout(int position, View convertView, ViewGroup parent)
        {

            Android.Views.LayoutInflater layoutInflater = ((StockViewActivity)context).LayoutInflater;
            TextView tvSymbol, tvTrackingPrices, tvOpen, tvPrice;

            Android.Views.View view = layoutInflater.Inflate(Resource.Layout.ListView_Track_Layout, parent, false);
            tvSymbol = view.FindViewById<TextView>(Resource.Id.tvSymbol);
            tvOpen = view.FindViewById<TextView>(Resource.Id.tvOpen);
            tvPrice = view.FindViewById<TextView>(Resource.Id.tvPrice);

            //tvAlarmSound = view.FindViewById<TextView>(Resource.Id.tvAlarmSound);
            tvTrackingPrices = view.FindViewById<TextView>(Resource.Id.tvTrackingPrices);
            



            StockData Temp = objects[position];


            if (Temp != null)
            {
                //Task t = testAsync(Temp);
                //t.Wait();

                tvSymbol.Text = Temp.symbol;
                tvOpen.Text = "Open " + Temp.open;
                tvPrice.Text = "Current price: " + Temp.price;
                tvTrackingPrices.Text = "";


                if (Temp.open > Temp.price)
                {
                    tvPrice.SetTextColor((Android.Content.Res.ColorStateList)"#FF0000"); //red
                    tvOpen.SetTextColor((Android.Content.Res.ColorStateList)"#008000"); //green
                }
                else
                {
                    tvPrice.SetTextColor((Android.Content.Res.ColorStateList)"#FF0000"); //red
                    tvOpen.SetTextColor((Android.Content.Res.ColorStateList)"#008000"); //green
                }

                if (Temp.TrackingPrices!= null)
                {
                    foreach (float num in Temp.TrackingPrices)
                    {
                        tvTrackingPrices.Text += num.ToString() + ",";
                    }

                    string text = tvTrackingPrices.Text;
                    int index = text.Length - 1;
                    text = text.Remove(index);
                    tvTrackingPrices.Text = text;
                }
                

                //tvAlarmSound.Text = "" + Temp.SoundName;
            }
            return view;
        }

        private View SavedLayout(int position, View convertView, ViewGroup parent)
        {
            Android.Views.LayoutInflater layoutInflater = ((StockViewActivity)context).LayoutInflater;
            Android.Views.View view = layoutInflater.Inflate(Resource.Layout.ListView_Saved_Layout, parent, false);

            TextView tvSymbol, tvOpen, tvPrice;
            tvSymbol = view.FindViewById<TextView>(Resource.Id.tvSymbol);
            //tvOpen = view.FindViewById<TextView>(Resource.Id.tvOpen);
            tvOpen = view.FindViewById<TextView> (Resource.Id.tvOpen);
            tvPrice = view.FindViewById<TextView>(Resource.Id.tvPrice);


            StockData Temp = objects[position];
            if (Temp.open > Temp.price)
            {
                tvPrice.SetTextColor((Android.Content.Res.ColorStateList)"#FF0000"); //red
                tvOpen.SetTextColor((Android.Content.Res.ColorStateList)"#008000"); //green
            }
            else
            {
                tvPrice.SetTextColor((Android.Content.Res.ColorStateList)"#FF0000"); //red
                tvOpen.SetTextColor((Android.Content.Res.ColorStateList)"#008000"); //green
            }
            if (Temp != null)
            {

                tvSymbol.Text = Temp.symbol;
                tvOpen.Text = "Open: " + Temp.open.ToString();
                tvPrice.Text = "Current price: " + Temp.price.ToString();

            }
            return view;
        }

        public override int Count
        {
            get { return this.objects.Count; }
        }
        public override StockData this[int position]
        {
            get { return this.objects[position]; }
        }



    }
}