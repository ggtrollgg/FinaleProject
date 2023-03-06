using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using App1.General_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App1
{
    internal class TextBlock
    {
        public string Text;
        public Paint p;

        public MyPoint LeftDown;
        public MyPoint RightDown;
        float width = 0;

        public bool Hidden = false;
        
        public TextBlock()
        {
        }

        public TextBlock(string text, Paint p)
        {
            Text = text;
            this.p = p;
        }

        public TextBlock(string text, Paint p , float x, float y)
        {
            Text = text;
            this.p = p;
            width = p.MeasureText(text);
            //Console.WriteLine("the width of the text is: " + width);

            LeftDown = new MyPoint(x - width/2, y);
            
            RightDown = new MyPoint(x + width/2, y);
        }

    }
}