using Android.App;
using Android.Content;
using Android.Graphics;
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
    public class ClassSearchStock
    {
        public String symbol;
        public String companyName;


        public ClassSearchStock()
        {
            
        }
        public ClassSearchStock(String symbol, String companyName)
        {
            this.symbol = symbol;
            this.companyName = companyName;
        }
        public ClassSearchStock(String symbol, String companyName, float price)
        {
            this.symbol = symbol;
            this.companyName = companyName;

        }
        public ClassSearchStock(String symbol, String companyName, float price,String StockImage)
        {
            this.symbol = symbol;
            this.companyName = companyName;
        }
    }
}