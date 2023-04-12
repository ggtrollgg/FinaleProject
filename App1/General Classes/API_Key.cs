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

namespace App1.General_Classes
{
    public class API_Key
    {
        public string Key { get; set; }
        private int CallsRemaining = 0;

        public API_Key(string k) 
        {
            Key= k;
        }

        public void SetCallsRemaining(int callsRemaining)
        {
            this.CallsRemaining= callsRemaining;
        }
        public void UseKey()
        {
            this.CallsRemaining --;
        }
        public int GetCallsRemaining(int callsRemaining)
        {
            return this.CallsRemaining;
        }

    }
}