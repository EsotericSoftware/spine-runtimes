using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Cocos2D;

namespace spine_cocos_xna_wp8
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //#if MACOS
            //            Content.RootDirectory = "AngryNinjas/Content";
            //#else
            Content.RootDirectory = "Content";
            //#endif
            //
            //#if XBOX || OUYA
            //            graphics.IsFullScreen = true;
            //#else
            graphics.IsFullScreen = false;
            //#endif

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333 / 2);

            // Extend battery life under lock.
            //InactiveSleepTime = TimeSpan.FromSeconds(1);

            CCApplication application = new AppDelegate(this, graphics);
            Components.Add(application);
            //#if XBOX || OUYA
            //            CCDirector.SharedDirector.GamePadEnabled = true;
            //            application.GamePadButtonUpdate += new CCGamePadButtonDelegate(application_GamePadButtonUpdate);
            //#endif
        }

        //#if XBOX || OUYA
        //        private void application_GamePadButtonUpdate(CCGamePadButtonStatus backButton, CCGamePadButtonStatus startButton, CCGamePadButtonStatus systemButton, CCGamePadButtonStatus aButton, CCGamePadButtonStatus bButton, CCGamePadButtonStatus xButton, CCGamePadButtonStatus yButton, CCGamePadButtonStatus leftShoulder, CCGamePadButtonStatus rightShoulder, PlayerIndex player)
        //        {
        //            if (backButton == CCGamePadButtonStatus.Pressed)
        //            {
        //                ProcessBackClick();
        //            }
        //        }
        //#endif

        private void ProcessBackClick()
        {
            if (CCDirector.SharedDirector.CanPopScene)
            {
                CCDirector.SharedDirector.PopScene();
            }
            else
            {
                Exit();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                ProcessBackClick();
            }

            // TODO: Add your update logic here


            base.Update(gameTime);
        }
    }
}