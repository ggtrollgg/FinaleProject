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
    public class Manager_API_Keys
    {
        public static List<API_Key> API_Keys = new List<API_Key>();
        ISharedPreferences Mysp = MainActivity.sp;

        public Manager_API_Keys() 
        {
            string key;
            int calls = 0;
            

            var editor = MainActivity.sp.Edit();
            if (Mysp.GetInt("KeysAmount", -1) == -1) //
            {
                editor.PutInt("KeysAmount", 3);
                editor.PutString("Key0", "0a0b32a8d57dc7a4d38458de98803860");
                editor.PutString("Key1", "8bdedb14d7674def460cb3a84f1fd429");
                editor.PutString("Key2", "561897c32bf107b87c107244081b759f");

                //i have no way to know how many keys remain on a new device so i put 255 in each 
                editor.PutInt("Key0CallsRemain", 255);
                editor.PutInt("Key1CallsRemain", 255);
                editor.PutInt("Key2CallsRemain", 255);

                editor.Commit();
            }

            for(int i = 0; i < MainActivity.sp.GetInt("KeysAmount", -1); i++)
            {
                key = Mysp.GetString("Key" + i, "");
                calls = Mysp.GetInt("Key" + i + "CallsRemain", 0);

                API_Keys.Add(new API_Key(key));
                API_Keys[i].SetCallsRemaining(calls);
                calls = 0;
            }


        }

        //get the key with most calls remain
        public API_Key GetBestKey()
        {
            int highest = 0;
            string bestkey = "";

            foreach(var key in API_Keys)
            {
                if(key.GetCallsRemaining() > highest)
                {
                    highest= key.GetCallsRemaining();
                    bestkey = key.Key;

                }
            }

            API_Key k = new API_Key(bestkey);
            k.SetCallsRemaining(highest);
            return k;
        }

        //use a key
        public void UseKey(string Skey)
        {
            for(int i = 0; i < API_Keys.Count; i++)
            {
                if (Skey == API_Keys[i].Key)
                {
                    API_Keys[i].UseKey();
                    var editor = MainActivity.sp.Edit();
                    editor.PutInt("Key" + i + "CallsRemain", API_Keys[i].GetCallsRemaining());
                    editor.Commit();
                    break;
                }
            }
            
        }
    }
}