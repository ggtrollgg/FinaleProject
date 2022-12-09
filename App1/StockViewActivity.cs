using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace App1
{
    [Activity(Label = "StockViewActivity")]
    public class StockViewActivity : Activity
    {
        Button btnReturnHome;
        ListView lvStock;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ListViewPage_Layout);
            btnReturnHome = FindViewById<Button>(Resource.Id.btnReturnHome);
            btnReturnHome.Click += BtnReturnHome_Click;


            lvStock =  (ListView)FindViewById(Resource.Id.lvStock);



            //Add_To_File("APPL");


            // Create your application here
        }

        private void BtnReturnHome_Click(object sender, EventArgs e)
        {
            Finish();
        }




        private String Read_from_file()
        {
            try
            {
                string str;
                //using (Stream stream = OpenFileOutput("Emailinfo.txt", Android.Content.FileCreationMode.Private))
                using (Stream stream = OpenFileInput("SavedStocks.txt"))
                {
                    try
                    {
                        byte[] buffer = new byte[4096];
                        stream.Read(buffer, 0, buffer.Length);
                        str = System.Text.Encoding.UTF8.GetString(buffer);
                        stream.Close();
                        if (str != null)
                        {
                            //  tv.Text = str;
                            Toast.MakeText(this, str, ToastLength.Short).Show();
                            return str;
                        }
                    }
                    catch (Java.IO.IOException a)
                    {
                        a.PrintStackTrace();
                    }
                }
            }
            catch (Java.IO.FileNotFoundException a)
            {
                a.PrintStackTrace();

            }

            return null;
        }

        private void Add_To_File(String the_stock)
        {

            if (Is_Allready_In_File(the_stock))
            {
                try
                {
                    string str = the_stock;//= et.Text;
                    using (Stream stream = OpenFileOutput("SavedStocks.txt", Android.Content.FileCreationMode.Append))
                    {
                        try
                        {
                            stream.Write(Encoding.ASCII.GetBytes(str), 0, str.Length);
                            stream.Close();
                            Toast.MakeText(this, "save", ToastLength.Short).Show();
                        }
                        catch (Java.IO.IOException a)
                        {
                            a.PrintStackTrace();
                        }
                    }
                }
                catch (Java.IO.FileNotFoundException a)
                {
                    a.PrintStackTrace();
                }
            }
        }

        private bool Is_Allready_In_File(string the_stock)
        {
            string str = Read_from_file();
            if (str== null || str.Contains(the_stock))
            {
                Toast.MakeText(this, "all ready in", ToastLength.Short).Show();
                return false;
            }
            return true;
        }

        
    }
}