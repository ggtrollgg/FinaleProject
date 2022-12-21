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

namespace App1
{
    internal class SearchStockAdapter : BaseAdapter<ClassSearchStock>
    {
        Android.Content.Context context;
        List<ClassSearchStock> objects;


        List<float> list_high = new List<float>();
        List<float> list_low = new List<float>();

        public SearchStockAdapter(Context context, System.Collections.Generic.List<ClassSearchStock> objects)
        {
            this.context = context;
            this.objects = objects;
        }
        public List<ClassSearchStock> GetList()
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
            TextView tvSymbol, tvPrice, tvName;
            ImageView ivImage;

            Android.Views.View view = layoutInflater.Inflate(Resource.Layout.ListView_Search_Layout, parent, false);

            tvSymbol = view.FindViewById<TextView>(Resource.Id.tvSymbol);
            tvName = view.FindViewById<TextView>(Resource.Id.tvName);
            tvPrice = view.FindViewById<TextView>(Resource.Id.tvPrice);

            ivImage = view.FindViewById<ImageView>(Resource.Id.ivImage);
            ClassSearchStock Temp = objects[position];

            if(Temp != null)
            {
                tvPrice.Text ="" + Temp.price;
                tvName.Text = "" + Temp.companyName;
                tvSymbol.Text = "" + Temp.symbol;
                ivImage.SetImageBitmap(Temp.StockImage);
            }

            return view;

        }

       


        public override int Count
        {
            get { return this.objects.Count; }
        }
        public override ClassSearchStock this[int position]
        {
            get { return this.objects[position]; }
        }
    }
}