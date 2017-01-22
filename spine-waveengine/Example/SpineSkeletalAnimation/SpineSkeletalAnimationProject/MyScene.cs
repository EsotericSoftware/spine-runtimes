#region Using Statements
using System;
using System.Collections.Generic;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Animation;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.UI;
using WaveEngine.Spine;
#endregion

namespace SpineSkeletalAnimationProject
{
    public class MyScene : Scene
    {
        Entity skeleton;
        string file;

        protected override void CreateScene()
        {
            RenderManager.BackgroundColor = Color.CornflowerBlue;

            file = "spineboy";
            //file = "spineboyNew";
            //file = "goblins-ffd";

            this.skeleton = new Entity("Spine")
                            .AddComponent(new Transform2D() 
                            { 
                                X = WaveServices.ViewportManager.VirtualWidth / 2,
                                Y = WaveServices.ViewportManager.VirtualHeight - 20,
                                XScale = file == "spineboyNew" ? 0.45f : 1,
                                YScale = file == "spineboyNew" ? 0.45f : 1,
                            })
                            .AddComponent(new WaveEngine.Spine.SkeletalData("Content/" + file + ".atlas"))
                            .AddComponent(new WaveEngine.Spine.SkeletalAnimation("Content/" + file + ".json"))
                            .AddComponent(new WaveEngine.Spine.SkeletalRenderer() { ActualDebugMode = WaveEngine.Spine.SkeletalRenderer.DebugMode.None });

            EntityManager.Add(this.skeleton);
            
            #region UI
            if (file == "goblins-ffd")
            {
                var skeletalAnim = this.skeleton.FindComponent<WaveEngine.Spine.SkeletalAnimation>();

                skeletalAnim.Skin = "goblin";
            }

            Slider slider1 = new Slider()
               {
                   Margin = new Thickness(10, 40, 0, 0),
                   Width = 500,
                   Minimum = -25,
                   Maximum = 25,
                   Value = 0
               };

            slider1.RealTimeValueChanged += (s, e) =>
            {
                var entity = EntityManager.Find("Light0");
                var component = skeleton.FindComponent<WaveEngine.Spine.SkeletalAnimation>();
                component.Speed = e.NewValue / 10f;
            };

            EntityManager.Add(slider1);

            ToggleSwitch debugMode = new ToggleSwitch()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 10, 0),
                IsOn = false,
                OnText = "Debug On",
                OffText = "Debug Off",
                Width = 200
            };

            debugMode.Toggled += (s, o) =>
            {
                RenderManager.DebugLines = ((ToggleSwitch)s).IsOn;
            };

            EntityManager.Add(debugMode);

            CheckBox debugBones = new CheckBox()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 60, 10, 0),
                Text = "Bones",
                Width = 150,
            };

            debugBones.Checked += (s, o) =>
            {
                if (o.Value)
                {
                    this.skeleton.FindComponent<WaveEngine.Spine.SkeletalRenderer>().ActualDebugMode |= WaveEngine.Spine.SkeletalRenderer.DebugMode.Bones;
                }
                else
                {
                    this.skeleton.FindComponent<WaveEngine.Spine.SkeletalRenderer>().ActualDebugMode &= WaveEngine.Spine.SkeletalRenderer.DebugMode.Quads;
                }
            };

            EntityManager.Add(debugBones);

            CheckBox debugQuads = new CheckBox()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 110, 10, 0),
                Text = "Quads",
                Width = 150,
            };

            debugQuads.Checked += (s, o) =>
            {
                if (o.Value)
                {
                    this.skeleton.FindComponent<WaveEngine.Spine.SkeletalRenderer>().ActualDebugMode |= WaveEngine.Spine.SkeletalRenderer.DebugMode.Quads;
                }
                else
                {
                    this.skeleton.FindComponent<WaveEngine.Spine.SkeletalRenderer>().ActualDebugMode &= WaveEngine.Spine.SkeletalRenderer.DebugMode.Bones;
                }
            };

            EntityManager.Add(debugQuads); 
            #endregion
        }

        protected override void Start()
        {
            base.Start();

            var anim = this.skeleton.FindComponent<WaveEngine.Spine.SkeletalAnimation>();

            switch (file)
            {
                case "spineboyNew":
                    anim.CurrentAnimation = "run";
                    break;
                case "goblins-ffd":
                    anim.CurrentAnimation = "walk";
                    break;
                case "spineboy":
                    anim.CurrentAnimation = "walk";
                    break;
            }

            anim.Play(true);            
        }
    }
}
