using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Chatterino.Common;
using System.IO;
using System.Collections.Generic;

namespace Chatterino.Mobile.Droid
{
    [Activity(Label = "Chatterino.Mobile", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        internal static MainActivity Current;

        public MainActivity()
        {
            Current = this;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            string loginData = @"oauth=
username=";

            IrcManager.Connect(new StringReader(loginData));

            global::Xamarin.Forms.Forms.Init(this, bundle);
            App app = new App();

            LoadApplication(app);

            GuiEngine.Initialize(new AndroidGuiEngine());

            var fourtf = IrcManager.AddChannel("fourtf");

            //IrcManager.MessageReceived += (s, e) =>
            //{
            //    lock (messages)
            //    {
            //        messages.Add(e.Message);
            //        e.Message.CalculateBounds(null, 1000);
            //    }
            //};
        }

        //internal List<Common.Message> messages = new List<Common.Message>();
    }
}

