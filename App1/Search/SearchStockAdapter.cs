using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;





namespace App1
{
    internal class SearchStockAdapter : BaseAdapter<ClassSearchStock>
    {
        Android.Content.Context context;
        List<ClassSearchStock> objects;


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
            Android.Views.LayoutInflater layoutInflater = ((SearchActivity)context).LayoutInflater;
            Android.Views.View view = layoutInflater.Inflate(Resource.Layout.ListView_Search_OnlyText_Layout2, parent, false);

            TextView tvSymbol, tvName;


           

            tvSymbol = view.FindViewById<TextView>(Resource.Id.tvSymbol);
            tvName = view.FindViewById<TextView>(Resource.Id.tvName);

            if(objects!= null && objects.Count > 0)
            {
                ClassSearchStock Temp = objects[position];

                if (Temp != null)
                {

                    tvName.Text = "" + Temp.companyName;
                    tvSymbol.Text = "" + Temp.symbol;

                }
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