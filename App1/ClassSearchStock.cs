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

        //public float price;
        //public Bitmap StockImage;
        //public String StockImage;

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
            //this.price = price;

        }
        public ClassSearchStock(String symbol, String companyName, float price,String StockImage)
        {
            //this.StockImage = StockImage;
            this.symbol = symbol;
            //this.price = price;
            this.companyName = companyName;
        }
    }
}