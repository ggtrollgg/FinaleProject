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
    public class EventStarter_Test
    {
        public event Action Call_Handler;

        public EventStarter_Test() { }

        public void start()
        {
            Thread t = new Thread(TestHandlerFromThread);
            t.Start();

        }

        private void TestHandlerFromThread()
        {
            Call_Handler?.Invoke();
        }
    }
}