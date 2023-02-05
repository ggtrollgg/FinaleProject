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

using System.Net.Http;
using System.Threading.Tasks;
using Org.Json;

using Firebase.Firestore;
using Firebase;

//import doc, deleteDoc from firestore;
using Java.Util;
using Java.Lang.Reflect;
using System.Runtime.InteropServices.ComTypes;
using Android.Graphics;
using Firestore.Admin.V1;
using System.Collections.ObjectModel;
using Android.Gms.Extensions;

namespace App1
{
    [Activity(Label = "Testing_Database_Activity")]
    public class Testing_Database_Activity : Activity, Android.Gms.Tasks.IOnSuccessListener
    {
        public FirebaseFirestore db;
        List<DocumentSnapshot> Docs_In_DataBase = new List<DocumentSnapshot>();

        Button btnPrintDataBase,btnDelete,btnAdd,btnAddDelete;
        Test_ZoomCanvas TZC;
        ZoomableCanvasView ZCV;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //TZC = new Test_ZoomCanvas(this);
            //SetContentView(TZC);

            ZCV = new ZoomableCanvasView(this);
            SetContentView(ZCV);
            /*
            SetContentView(Resource.Layout.Testing_DataBase_Layout);
            // Create your application here
            btnPrintDataBase = FindViewById<Button>(Resource.Id.btnPrintDataBase);
            btnAdd = FindViewById<Button>(Resource.Id.btnAdd);
            btnDelete = FindViewById<Button>(Resource.Id.btnDelete);
            btnAddDelete = FindViewById<Button>(Resource.Id.btnAddDelete);

            btnAddDelete.Click += BtnAddDelete_Click;
            btnDelete.Click += BtnDelete_Click;
            btnAdd.Click += BtnAdd_Click;
            btnPrintDataBase.Click += BtnPrintDataBase_Click;


            SetUpDataBase();
            */

        }

        private void BtnAddDelete_Click(object sender, EventArgs e)
        {
            int index = -1;
            int i = 0;

            string symbol = "OB";
            bool IsInDataBase = false;

            foreach (var doc in Docs_In_DataBase)
            {
                if (symbol == (string)doc.Get("symbol"))
                {
                    IsInDataBase = true;
                    index = i;
                }
                i++;
            }

            if (IsInDataBase) //it is in the data base
            {
                Console.WriteLine("deleting from database");
                DeleteItem_fromDataBase(index);
                return;
            }
            else //it isn't in the data base
            {
                Console.WriteLine("adding to database");
                AddItem_ToDataBAse(symbol);
                _ = LoadItemsAsync(); //refresh to the real oreder of items in the data base
                return;
            }
        }

        private void BtnPrintDataBase_Click(object sender, EventArgs e)
        {
            Console.WriteLine();
            if (db != null)
            {
                Console.WriteLine("db isnt null");
                Console.WriteLine(Docs_In_DataBase);
                int i = 0;
                foreach (var doc in Docs_In_DataBase)
                {
                    i++;
                    Console.WriteLine(i + ") " + (string)doc.Get("symbol"));
                }

                if (Docs_In_DataBase.Count == 0)
                {
                    Console.WriteLine("the data base is empty");
                }

                Console.WriteLine("\n");
            }

        }



        private void BtnDelete_Click(object sender, EventArgs e)
        {
            int index = -1;
            int i = 0;

            string symbol = "OB";
            bool IsInDataBase = false;

            foreach (var doc in Docs_In_DataBase)
            {
                if (symbol == (string)doc.Get("symbol"))
                {
                    IsInDataBase = true;
                    index = i;
                }
                i++;
            }

            if (IsInDataBase) //it is in the data base
            {
                Console.WriteLine("deleting from database");
                DeleteItem_fromDataBase(index);
                return;
            }
            Console.WriteLine("didnt deleting because it isnt in the data base");

        }

        private void DeleteItem_fromDataBase( int index)
        {
            DocumentReference doc = db.Collection("Saved Stocks").Document(Docs_In_DataBase[index].Id);
            doc.Delete();
            Docs_In_DataBase.RemoveAt(index);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {

            string symbol = "OB";
            bool IsInDataBase = false;

            foreach (var doc in Docs_In_DataBase)
            {
                if(symbol == (string)doc.Get("symbol"))
                    IsInDataBase= true;
            }

            if (!IsInDataBase)
            {
                Console.WriteLine("adding to database");
                AddItem_ToDataBAse("OB");
                _=LoadItemsAsync();
                return;
            }

            Console.WriteLine("didnt add to database");
        }

        private void AddItem_ToDataBAse(string symbol)
        {
            HashMap map = new HashMap();
            map.Put("symbol", symbol);
            map.Put("LastDate", "");
            map.Put("SoundFile", "");
            map.Put("TrackingPrices", "");
            map.Put("heigh", 0);
            map.Put("low", 0);

            CollectionReference collection = db.Collection("Saved Stocks");
            collection.Add(map);

            //DocumentReference docRef = db.Collection("Saved Stocks").Document();
            //docRef.Set(map);
        }


        



        public void SetUpDataBase()
        {
            db = GetDataBase();
            _ = LoadItemsAsync();
        }


        private FirebaseFirestore GetDataBase()
        {
            var options = new FirebaseOptions.Builder()
            .SetProjectId("stock-data-base-finalproject")
            .SetApplicationId("stock-data-base-finalproject")
            .SetApiKey("AIzaSyCjiFrMsBwOFvqUZRdohfIiqMsJC5QG_kc")
            .SetStorageBucket("stock-data-base-finalproject.appspot.com")
            .Build();


            var app = FirebaseApp.InitializeApp(this, options);
            db = FirebaseFirestore.GetInstance(app);
            return db;
        }

        private async Task LoadItemsAsync()
        {
            Docs_In_DataBase.Clear();

            Query q = db.Collection("Saved Stocks");
            await q.Get().AddOnSuccessListener(this);

        }

        public void OnSuccess(Java.Lang.Object result)
        {

            var snapshot = (QuerySnapshot)result;
            foreach (var doc in snapshot.Documents)
            {
                Docs_In_DataBase.Add(doc);
                
            }
        }


        protected override void OnPause()
        {
            if(db != null && db.App != null)      
            {
                db.App.Delete();
                db.Terminate();
            } 
           
            base.OnPause();
        }
    }
}