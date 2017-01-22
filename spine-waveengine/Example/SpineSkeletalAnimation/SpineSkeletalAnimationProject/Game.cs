#region Using Statements
using System;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
#endregion

namespace SpineSkeletalAnimationProject
{
    public class Game : WaveEngine.Framework.Game
    {
        public override void Initialize(IApplication application)
        {
            base.Initialize(application);

            ViewportManager vm = WaveServices.GetService<ViewportManager>();
            vm.Activate(800, 480, ViewportManager.StretchMode.Fill);

            WaveServices.ScreenContextManager.To(new ScreenContext(new MyScene()));
        }
    }
}
