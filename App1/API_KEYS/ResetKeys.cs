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

namespace App1
{
    [BroadcastReceiver]
    public class ResetKeys : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Console.WriteLine("_______________________________________________________________________Reseted all Keys for web info_______________________________________________________________________");
            Console.WriteLine("they were: " );
            for (int i = 0; i < MainActivity.sp.GetInt("KeysAmount", -1); i++)
            {
                Console.WriteLine("   ");

                Console.WriteLine("ShardPrefrenc: Key" + i + " is: " + MainActivity.sp.GetString("Key" + i, ""));
                Console.WriteLine("ShardPrefrenc: CallsRemain: " + MainActivity.sp.GetInt("Key" + i + "CallsRemain", 0));

                Console.WriteLine("   ");
            }

            var editor = MainActivity.sp.Edit();
            //editor.PutInt("Key0CallsRemain", 250);
            //editor.PutInt("Key1CallsRemain", 250);
            //editor.PutInt("Key2CallsRemain", 250);
            //editor.Commit();
            Console.WriteLine("___________________________________________________________________________________________________________________________________________________________________________");

        }
    }
}