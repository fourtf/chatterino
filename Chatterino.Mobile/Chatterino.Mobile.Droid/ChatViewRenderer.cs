using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Chatterino.Common;
using Message = Chatterino.Common.Message;

[assembly: Xamarin.Forms.ExportRenderer(typeof(Chatterino.Mobile.ChatView), typeof(Chatterino.Mobile.Droid.ChatViewRenderer))]
namespace Chatterino.Mobile.Droid
{
    public class ChatViewRenderer : Xamarin.Forms.Platform.Android.ViewRenderer<ChatView, _ChatView>
    {
        _ChatView view;

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<ChatView> e)
        {
            base.OnElementChanged(e);

            view = new _ChatView(Context);
            SetNativeControl(view);
        }

        //Paint paint = new Paint
        //{
        //    Color = Android.Graphics.Color.Red
        //};

        //public override void Draw(Canvas canvas)
        //{
        //    base.Draw(canvas);

        //    canvas.DrawCircle(100, 100, 50, paint);
        //}
    }

    public class _ChatView : View
    {
        TwitchChannel channel = null;
        int totalMessageHeight = 0;

        public _ChatView(Context context) : base(context)
        {
            channel = TwitchChannel.AddChannel("pajlada");

            channel.MessageAdded += Channel_MessageAdded;
        }

        private void Channel_MessageAdded(object sender, MessageAddedEventArgs e)
        {
            e.Message.CalculateBounds(null, MeasuredWidth);

            MainActivity.Current.RunOnUiThread(() => Invalidate());
        }

        //void updateMessageBounds(bool emoteChanged = false)
        //{
        //    int totalHeight = 0;
        //    lock (messages)
        //    {
        //        foreach (var msg in messages)
        //        {
        //            msg.Y = totalHeight;
        //            totalHeight += msg.Height;
        //        }
        //    }
        //    totalMessageHeight = totalHeight;
        //}

        List<Message> messages = new List<Message>();

        Paint paint = new Paint
        {
            Color = Color.Red
        };

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            SetMeasuredDimension(widthMeasureSpec, 10000);
        }

        public override void OnDrawForeground(Canvas canvas)
        {
            //base.OnDrawForeground(canvas);

            Console.WriteLine("###drawing");

            int y = 0;
            Message[] M;

            lock (messages)
                M = messages.ToArray();

            foreach (Message msg in M)
            {
                canvas.DrawCircle(100, y, 20, paint);
                msg.Draw(canvas, 0, y, null, -1);
                y += msg.Height;
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            //base.OnDraw(canvas);

            Console.WriteLine("###background");
        }
    }
}