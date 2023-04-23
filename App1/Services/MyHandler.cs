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
    internal class MyHandler : Handler
    {
        Context context;


        [Obsolete]
        public MyHandler(Context context)
        {
            this.context = context;

        }
        [Obsolete]
        public override void HandleMessage(Message msg)
        {
            Toast.MakeText(context, "" + msg.Arg1, ToastLength.Short).Show();
        }
        [Obsolete]
        public void ShowListView_SearchStock(Context context, ListView listview, List<ClassSearchStock> SearchDatalist, SearchStockAdapter adapter)
        {
            adapter = new SearchStockAdapter(context, SearchDatalist);
            listview.Adapter = adapter;
        }
    }
}