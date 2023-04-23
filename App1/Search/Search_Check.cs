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
using System.Threading;

namespace App1.Search
{
    internal class Search_Check
    {
        MyHandler handler;
        Thread function;

        public Search_Check(MyHandler handler, Thread function)
        {
            this.handler = handler;
            this.function = function;
        }

        public Search_Check(MyHandler handler)
        {
            this.handler = handler;

        }
    }
}