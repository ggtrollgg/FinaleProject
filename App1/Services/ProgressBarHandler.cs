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
    public class ProgressBarHandler:Handler
    {
        Context context;
        public LinearLayout LLProgress,LLProgressBar, LLPrediction;
        public TextView TVProgress, TVPrediction;
        public string status;
        public string prediction;
        ImageView iv;
        public int maxProgress_calls = 5;

        [Obsolete]
        public ProgressBarHandler(Context context)
        {
            this.context = context;

        }
        [Obsolete]
        public ProgressBarHandler(Context context, LinearLayout LLProgg, TextView TVProgg, LinearLayout LLPred, TextView TVPred)
        {
            this.context = context;
            this.LLProgressBar = LLProgg;
            TVProgress= TVProgg;
            TVPrediction= TVPred;
            LLPrediction= LLPred;
        }

        public override void HandleMessage(Message msg)
        {
            if(status == null || LLProgress == null || LLProgressBar == null || LLPrediction == null || TVProgress==null || TVPrediction== null )
            {
                Console.WriteLine("MyError!!!!!! ------> There is something null in ProgressBarHandler");
                return;
            }
            if(LLProgress.Visibility != ViewStates.Visible)
            {
                LLProgress.Visibility = ViewStates.Visible;
                LLPrediction.Visibility = ViewStates.Gone;
            }
            if (msg.Arg1 == -1)
            {
                TVProgress.Text = "" + status;
            }
            if(msg.Arg1== 0)
            {
                Add_progress_ToBar();
            }
            else if(msg.Arg1 == 1)
            {
                LLProgressBar.RemoveAllViews();
                LLProgress.Visibility = ViewStates.Gone;

                LLPrediction.Visibility = ViewStates.Visible;
                TVPrediction.Text = "" +  prediction;
            }


        }

        private void Add_progress_ToBar()
        {
            TVProgress.Text ="" + status;
            float screen_width = LLProgress.Width;
            screen_width -= 20 * maxProgress_calls; //20 - margin
            iv = new ImageView(context);
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams((int)(screen_width/maxProgress_calls), 100);
            layoutParams.SetMargins(10,0,10,0);
            iv.LayoutParameters = layoutParams;
            iv.SetBackgroundColor(Android.Graphics.Color.Aqua);
            LLProgressBar.AddView(iv);

        }
    }
}