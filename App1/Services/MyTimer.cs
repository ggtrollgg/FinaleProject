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

namespace App1
{
    internal class MyTimer
    {
        int counter;
        Handler handler;
        public bool stop;
        public MyTimer(Handler handler, int counter)
        {
            this.handler = handler;
            this.counter = counter;
        }
        public void Begin()
        {
            ThreadStart threadStart = new ThreadStart(Run);
            Thread t = new Thread(threadStart);
            this.stop = false;
            t.Start();
        }
        private void Run()
        {
            while (this.counter > 0) if (!stop)
                {
                    counter--;
                    Thread.Sleep(1000);
                    Message msg = new Message();
                    msg.Arg1 = counter; handler.SendMessage(msg);
                }
        }
    }
}