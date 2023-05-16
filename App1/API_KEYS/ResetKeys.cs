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
using Java.Lang;

namespace App1
{
    [BroadcastReceiver]
    public class ResetKeys : BroadcastReceiver
    {
        int NOTIFICATION_ID = 1;
        string NOTIFICATION_CHANNEL_ID = "ResetKeysAlarm";
        Context con;
        public override void OnReceive(Context context, Intent intent)
        {
            con = context;
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

            Toast.MakeText(context, "Reseted keys" , ToastLength.Long).Show();
            // Toast 

        }


        public void create_notification()
        {
            //NOTIFICATION_ID = 2;
            //Intent ii = new Intent(con, typeof(MainActivity));
            //ii.PutExtra("key", "new message");
            //ii.PutExtra("symbol", "lololol");
            //PendingIntent pendingIntent = PendingIntent.GetActivity(con, 0, ii, 0);

            //Notification.Builder notificationBuilder = new Notification.Builder(con)
            //.SetSmallIcon(Resource.Drawable.Icon_Favorite_colored)
            //.SetContentTitle("lololol")
            //.SetContentText("alarm activated: ");


            //var notificationManager = GetSystemService(NotificationService) as NotificationManager;

            //foreach (var notification in notificationManager.GetActiveNotifications())
            //{
            //    if (notification.Id == NOTIFICATION_ID)
            //    {
            //        notificationManager.Cancel(NOTIFICATION_ID); //if there is already a notification with this id than cancel it 
            //    }
            //}



            //notificationBuilder.SetContentIntent(pendingIntent);
            ////Build.VERSION_CODES.O - is a reference to API level 26 (Android Oreo which is Android 8)if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            //if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            //{
            //    NotificationChannel notificationChannel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, "NOTIFICATION_CHANNEL_NAME", NotificationImportance.High);
            //    notificationBuilder.SetChannelId(NOTIFICATION_CHANNEL_ID);
            //    notificationManager.CreateNotificationChannel(notificationChannel);
            //}
            //notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
        }
    }
}