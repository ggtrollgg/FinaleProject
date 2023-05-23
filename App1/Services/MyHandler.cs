using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using App1.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App1
{
    public class MyHandler : Handler
    {
        Context context;
        public LinearLayout LL ;
        public MA_View graph_view;

       [Obsolete]
        public MyHandler(Context context)
        {
            this.context = context;

        }
        [Obsolete]
        public MyHandler(Context context, LinearLayout LL, MA_View graph_view)
        {
            this.context = context;
            this.LL = LL;
        }
        //take corresponding action to the data in the message 
        public override void HandleMessage(Message msg)
        {
            if(graph_view== null)  //if the  drawer of the algorithm graphs has been disposed of than clear all views in the LinearLayout it was in;
            {
                if (LL != null)
                {
                    this.LL.Visibility = ViewStates.Gone;
                    if (LL.RootView != null)
                    {
                        this.LL.RemoveAllViews();
                    }
                }
                return;
            }
            if(LL!= null) //add the drawing view of the algorithm again
            {
                this.LL.Visibility = ViewStates.Visible;
                if (LL.RootView != null)
                {
                    this.LL.RemoveAllViews();
                }
                this.LL.AddView(graph_view);
            }

        }

    }
}