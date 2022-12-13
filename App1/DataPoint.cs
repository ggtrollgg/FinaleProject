﻿using Android.App;
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
    public class DataPoint
    {
        public float heigh;
        public float low;
        public String symbol;
        public string date;

        public string SoundName = null;
        public List<float> TrackingPrices = null;

        public DataPoint()
        {

        }
        public DataPoint(float heigh, float low, string date, String symbol)
        {
            this.heigh = heigh;
            this.low = low;
            this.date = date;
            this.symbol = symbol;
        }
        public DataPoint(float heigh, float low, string symbol, string date, string soundName, List<float> trackingPrices) : this(heigh, low, symbol, date)
        {
            this.heigh = heigh;
            this.low = low;
            this.date = date;
            this.symbol = symbol;
            this.SoundName = soundName;
            this.TrackingPrices = trackingPrices;
        }
    }
}