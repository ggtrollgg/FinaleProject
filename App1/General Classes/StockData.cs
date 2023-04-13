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
    public class StockData
    {

        public float price;
        public float open;
        public String symbol;

        public string SoundName = null;
        public List<float> TrackingPrices = null;


        public StockData()
        {

        }
        public StockData(float price, float open, String symbol)
        {
            this.price = price;
            this.open = open;
            this.symbol = symbol;
        }
         public StockData(float price, float open, string symbol, string soundName)
        {
            this.price = price;
            this.open = open;

            this.symbol = symbol;
            this.SoundName = soundName;
            
        }
        public StockData(float price, float open, string symbol, string soundName, List<float> trackingPrices)
        {
            this.price = price;
            this.open = open;

            this.symbol = symbol;
            this.SoundName = soundName;
            this.TrackingPrices = trackingPrices;
        }
    }
}