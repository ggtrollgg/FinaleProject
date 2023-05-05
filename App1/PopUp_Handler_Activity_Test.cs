using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using App1.Algorithm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace App1
{
    [Activity(Label = "PopUp_Handler_Activity_Test")]
    public class PopUp_Handler_Activity_Test : Activity
    {

        MyHandler myHandler;
        Handler handler;


        Dialog d;
        LinearLayout algoLL;
        Button btnShow;
        EventStarter_Test Event_st;

        MATL_Algorithm MATLAlgo;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            myHandler = new MyHandler(this);
            ShowPopUp();

            // Create your application here
        }

        private void ShowPopUp()
        {
            d = new Dialog(this);
            //d.SetContentView(Resource.Layout.PopUp_Test);
            d.SetContentView(Resource.Layout.Custom_PopUp_Algorithm);
            d.SetTitle("abot the stock");
            d.SetCancelable(true);

            //algoLL = d.FindViewById<LinearLayout>(Resource.Id.LLalgo);
            //btnShow = d.FindViewById<Button>(Resource.Id.btnShow);

            algoLL = d.FindViewById<LinearLayout>(Resource.Id.LLcanvas);
            btnShow = d.FindViewById<Button>(Resource.Id.btnStart);

            btnShow.Click += BtnShow_Click;
            d.Show();
        }
        
        private void BtnShow_Click(object sender, EventArgs e)
        {
            _ = Temp();
        }
        private async Task Temp()
        {
            _ = TestHandlerFromTask();
            return;
        }
        private async Task TestHandlerFromTask()
        {
            //Event_st = new EventStarter_Test();
            //Event_st.Call_Handler += TestHandlerFromThread;
            //Event_st.start();

            List<DataPoint> newList = new List<DataPoint>();
            newList.Add(new DataPoint(10, 4, 10, "2020-05-05"));
            newList.Add(new DataPoint(9, 4, 9, "2020-05-05"));
            newList.Add(new DataPoint(8, 4, 8, "2020-05-05"));
            newList.Add(new DataPoint(7, 4, 7, "2020-05-05"));
            newList.Add(new DataPoint(6, 4, 6, "2020-05-05"));
            newList.Add(new DataPoint(5, 4, 5, "2020-05-05"));
            newList.Add(new DataPoint(4, 4, 4, "2020-05-05"));
            newList.Add(new DataPoint(3, 2, 3, "2020-05-05"));

            MATLAlgo = new MATL_Algorithm(newList, 2);
            //MATLAlgo.ContinueProcess += TestHandlerFromThread;
            MATLAlgo.ContinueProcess += Continue_Algorithm_Process;
            MATLAlgo.Start_Algorithm();



        }
        private void Continue_Algorithm_Process()
        {
            //Console.WriteLine("CBdrawProcess is checked");
            MA_View graph_view = new MA_View(this, MATLAlgo.movingAverage_Graph);
            myHandler.graph_view = graph_view;
            //myHandler.LL = algoLL;
            //handler = myHandler;
            //Message m = new Message();
            //m.Arg1 = 1;
            //handler.HandleMessage(m);


            myHandler.LL = algoLL;
            handler = myHandler;
            Message m = new Message();
            m.Arg1 = 1;
            handler.SendMessage(m);
        }
        private void TestHandlerFromThread()
        {
            myHandler.LL = algoLL;
            handler = myHandler;
            Message m = new Message();
            m.Arg1 = 100;
            handler.SendMessage(m);
        }
    }
}