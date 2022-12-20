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

using System.Text.RegularExpressions;
using static Android.Renderscripts.ScriptGroup;

namespace App1
{
    [Activity(Label = "SearchActivity")]
    public class SearchActivity : Activity
    {
        EditText etSearch;
        Button btnSearch;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearchLayout);
            etSearch = FindViewById<EditText>(Resource.Id.etSearch);
            btnSearch = FindViewById<Button>(Resource.Id.btnSearch);

            btnSearch.Click += BtnSearch_Click;

           
            // Create your application here
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            String symbol = etSearch.Text;
            if (symbol != null && symbol != "" && Regex.IsMatch(symbol, @"^[a-zA-Z]+$"))
            {

            }
        }

        public void MoveToChartView()
        {
            Intent intent = new Intent(this, typeof(ChartActivity));
            StartActivity(intent);
        }
    }
}