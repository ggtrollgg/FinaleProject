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
            //tvPrice = view.FindViewById<TextView>(Resource.Id.tvPrice);

            //ivImage = view.FindViewById<ImageView>(Resource.Id.ivImage);
            if(objects!= null && objects.Count > 0)
            {
                ClassSearchStock Temp = objects[position];

                if (Temp != null)
                {

                    tvName.Text = "" + Temp.companyName;
                    tvSymbol.Text = "" + Temp.symbol;
                    //ivImage.SetImageBitmap(Temp.StockImage);
                    //ivImage.SetImageURI((Android.Net.Uri)Temp.StockImage);

                    //var image_link = new Android.Net.Uri(Temp.StockImage);
                    //Uri myUri = Uri.parse("http://www.google.com");



                    //var image_link2 = new System.Uri(Temp.StockImage);
                    //if(Temp.StockImage != null)
                    //{
                    //    tvPrice.Text = "" + Temp.price;
                    //    //Android.Net.Uri myUri = Android.Net.Uri.Parse(Temp.StockImage);
                    //    //ivImage.SetImageURI(myUri);



                    //    //ivImage.SetImageURI(Android.Net.Uri.Parse(Android.Net.Uri.Decode(Temp.StockImage)));
                    //    //ivImage.SetImageURI(myUri);


                    //    //Bitmap bmp = BitmapFactory.DecodeStream(ContentResolver.OpenInputStream(myUri));
                    //    //ContentResolver contentResolver;
                    //    //Bitmap bmp = BitmapFactory.DecodeStream(contentResolver.OpenInputStream(myUri));
                    //    //ivImage.SetImageBitmap(bmp);

                    //    //Activity activity = new Activity();
                    //    //var input = activity.ContentResolver.OpenInputStream(myUri);
                    //    //ivImage.SetImageBitmap(BitmapFactory.DecodeStream(input));

                    //    //Bitmap bmp = BitmapFactory.DecodeStream(Activity.ContentResolver.OpenInputStream(myUri));
                    //    //ContentResolver contentResolver;

                    //}

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