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
    [Activity(Label = "StockViewActivity")]
    public class StockViewActivity : Activity
    {
        Button btnReturnHome;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ListViewPage_Layout);
            btnReturnHome = FindViewById<Button>(Resource.Id.btnReturnHome);
            btnReturnHome.Click += BtnReturnHome_Click;

            // Create your application here
        }

        private void BtnReturnHome_Click(object sender, EventArgs e)
        {
            Finish();
        }
    }
}