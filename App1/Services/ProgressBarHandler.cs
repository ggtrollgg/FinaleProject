using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
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
        Animation FadeIn;

        [Obsolete]
        public ProgressBarHandler(Context context)
        {
            this.context = context;
            FadeIn = AnimationUtils.LoadAnimation(context, Resource.Animation.FadeIn);
        }
        [Obsolete]
        public ProgressBarHandler(Context context, LinearLayout LLProgg, TextView TVProgg, LinearLayout LLPred, TextView TVPred)
        {
            this.context = context;
            this.LLProgressBar = LLProgg;
            TVProgress= TVProgg;
            TVPrediction= TVPred;
            LLPrediction= LLPred;
            FadeIn = AnimationUtils.LoadAnimation(context, Resource.Animation.FadeIn);
        }

        //take corresponding action to the data in the message 
        public override void HandleMessage(Message msg)
        {
            if(status == null || LLProgress == null || LLProgressBar == null || LLPrediction == null || TVProgress==null || TVPrediction== null )
            {
                Console.WriteLine("MyError!!!!!! ------> There is something null in ProgressBarHandler");
                return;
            }
            if(LLProgress.Visibility != ViewStates.Visible) //make progress bar visibale (and in case it is a back to back activation of the algorithm, than hide the prediction text)
            {
                LLProgress.Visibility = ViewStates.Visible;
                LLPrediction.Visibility = ViewStates.Gone;
            }
            if (msg.Arg1 == -1) //when the process stats but nothing has finished yet -> the progress bar states something along the line of: "algorithm started" 
            {
                TVProgress.Text = "" + status;
            }
            if(msg.Arg1== 0) // add to progrees bar another image viwe and put the right string
            {
                Add_progress_ToBar();
            }
            else if(msg.Arg1 == 1) //remove the imageViews, hide the Progressbar and Show prediction of algorithm
            {
                LLProgressBar.RemoveAllViews();
                LLProgress.Visibility = ViewStates.Gone;

                LLPrediction.Visibility = ViewStates.Visible;
                TVPrediction.Text = "" +  prediction;
            }


        }

        //add a image view item to the view and make it fade in 
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
            iv.StartAnimation(FadeIn);

        }
    }
}