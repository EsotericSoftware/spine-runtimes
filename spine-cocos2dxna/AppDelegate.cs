using System.Reflection;
using Microsoft.Xna.Framework;
using Cocos2D;
using CocosDenshion;

namespace SpineCocosXna
{
    public class AppDelegate : CCApplication
    {

        int preferredWidth;
        int preferredHeight;

        public AppDelegate(Game game, GraphicsDeviceManager graphics)
            : base(game, graphics)
        {
            s_pSharedApplication = this;

            preferredWidth = 1024;
            preferredHeight = 768;
            graphics.PreferredBackBufferWidth = preferredWidth;
            graphics.PreferredBackBufferHeight = preferredHeight;

            CCDrawManager.InitializeDisplay(game,
                                          graphics,
                                          DisplayOrientation.LandscapeRight | DisplayOrientation.LandscapeLeft);


            graphics.PreferMultiSampling = false;

        }

        /// <summary>
        /// Implement for initialize OpenGL instance, set source path, etc...
        /// </summary>
        public override bool InitInstance()
        {
            return base.InitInstance();
        }

        /// <summary>
        ///  Implement CCDirector and CCScene init code here.
        /// </summary>
        /// <returns>
        ///  true  Initialize success, app continue.
        ///  false Initialize failed, app terminate.
        /// </returns>
        public override bool ApplicationDidFinishLaunching()
        {
            //initialize director
            CCDirector pDirector = CCDirector.SharedDirector;
            pDirector.SetOpenGlView();


            // 2D projection
            pDirector.Projection = CCDirectorProjection.Projection2D;

            var resPolicy = CCResolutionPolicy.ExactFit; // This will stretch out your game

            CCDrawManager.SetDesignResolutionSize(preferredWidth,
                                                  preferredHeight,
                                                  resPolicy);

            // turn on display FPS
            //pDirector.DisplayStats = true;

            // set FPS. the default value is 1.0/60 if you don't call this
            pDirector.AnimationInterval = 1.0 / 60;

            CCScene pScene = IntroLayer.Scene;

            pDirector.RunWithScene(pScene);
            return true;
        }

        /// <summary>
        /// The function be called when the application enters the background
        /// </summary>
        public override void ApplicationDidEnterBackground()
        {
            // stop all of the animation actions that are running.
            CCDirector.SharedDirector.Pause();

            // if you use SimpleAudioEngine, your music must be paused
            //CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic = true;
        }

        /// <summary>
        /// The function be called when the application enter foreground  
        /// </summary>
        public override void ApplicationWillEnterForeground()
        {
            CCDirector.SharedDirector.Resume();

            // if you use SimpleAudioEngine, your background music track must resume here. 
            //CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic = false;

        }
    }
}