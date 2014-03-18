using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Cocos2D;

namespace spine_cocos_xna_android
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

            Content.RootDirectory = "Content";

            graphics.IsFullScreen = false;

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333 / 2);

            // Extend battery life under lock.
            //InactiveSleepTime = TimeSpan.FromSeconds(1);

            CCApplication application = new AppDelegate(this, graphics);
            Components.Add(application);
        }

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