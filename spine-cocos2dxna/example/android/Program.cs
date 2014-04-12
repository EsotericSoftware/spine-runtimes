using System;
using System.Diagnostics;

using Android.Content.PM;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Uri = Android.Net.Uri;
using Microsoft.Xna.Framework;

namespace spine_cocos_xna_android
{

    [Activity(Label = "spine_cocos_xna_android"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class Program : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Game1.Activity = this;
            var game = new Game1();

            var frameLayout = new FrameLayout(this);
            frameLayout.AddView(game.Window);
            this.SetContentView(frameLayout);

            //SetContentView(game.Window);
            game.Run(GameRunBehavior.Asynchronous);
        }
    }


}

