/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_EDITOR
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Spine.Unity {
    [ExecuteInEditMode]
    [AddComponentMenu("Spine/EditorSkeletonPlayer")]
    [RequireComponent(typeof(ISkeletonAnimation))]
    public class EditorSkeletonPlayer : MonoBehaviour {
        private IEditorSkeletonWrapper skeletonWrapper;
        private TrackEntry trackEntry;
        private string oldAnimationName;
        private bool oldLoop;
        private double oldTime;

        [DidReloadScripts]
        private static void OnReloaded() {
            // Force start when scripts are reloaded
            EditorSkeletonPlayer[] editorSpineAnimations = FindObjectsOfType<EditorSkeletonPlayer>();

            foreach (EditorSkeletonPlayer editorSpineAnimation in editorSpineAnimations) 
                editorSpineAnimation.Start();
        }

        private void Start() {
            if (Application.isPlaying) return;

            if (skeletonWrapper == null) {
                if (TryGetComponent(out SkeletonAnimation skeletonAnimation))
                    skeletonWrapper = new SkeletonAnimationWrapper(skeletonAnimation);
                else if (TryGetComponent(out SkeletonGraphic skeletonGraphic))
                    skeletonWrapper = new SkeletonGraphicWrapper(skeletonGraphic);
            }

            oldTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += EditorUpdate;
        }

        private void OnDestroy() {
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate() {
            if (enabled == false || Application.isPlaying) return;
            if (skeletonWrapper == null) return;
            if (skeletonWrapper.State == null) return;

            // Update animation
            if (oldAnimationName != skeletonWrapper.AnimationName || oldLoop != skeletonWrapper.Loop) {
                trackEntry = skeletonWrapper.State.SetAnimation(0, skeletonWrapper.AnimationName, skeletonWrapper.Loop);
                oldAnimationName = skeletonWrapper.AnimationName;
                oldLoop = skeletonWrapper.Loop;
            }

            // Update speed
            if (trackEntry != null) 
                trackEntry.TimeScale = skeletonWrapper.Speed;

            float deltaTime = (float)(EditorApplication.timeSinceStartup - oldTime);
            skeletonWrapper.Update(deltaTime);
            oldTime = EditorApplication.timeSinceStartup;

            // Force repaint to update animation smoothly
            EditorApplication.QueuePlayerLoopUpdate();
        }

        private class SkeletonAnimationWrapper : IEditorSkeletonWrapper {
            private SkeletonAnimation _skeletonAnimation;

            public SkeletonAnimationWrapper(SkeletonAnimation skeletonAnimation) {
                _skeletonAnimation = skeletonAnimation;
            }

            public string AnimationName {
                get { return _skeletonAnimation.AnimationName; }
            }

            public bool Loop {
                get { return _skeletonAnimation.loop; }
            }

            public float Speed {
                get { return _skeletonAnimation.timeScale; }
            }

            public Spine.AnimationState State {
                get { return _skeletonAnimation.state; }
            }

            public void Update(float deltaTime) {
                _skeletonAnimation.Update(deltaTime);
            }
        }

        private class SkeletonGraphicWrapper : IEditorSkeletonWrapper {
            private SkeletonGraphic _skeletonGraphic;

            public SkeletonGraphicWrapper(SkeletonGraphic skeletonGraphic) {
                _skeletonGraphic = skeletonGraphic;
            }

            public string AnimationName {
                get { return _skeletonGraphic.startingAnimation; }
            }

            public bool Loop {
                get { return _skeletonGraphic.startingLoop; }
            }

            public float Speed {
                get { return _skeletonGraphic.timeScale; }
            }

            public Spine.AnimationState State {
                get { return _skeletonGraphic.AnimationState; }
            }

            public void Update(float deltaTime) {
                _skeletonGraphic.Update(deltaTime);
            }
        }

        private interface IEditorSkeletonWrapper {
            string AnimationName { get; }

            bool Loop { get; }

            float Speed { get; }

            Spine.AnimationState State { get; }

            void Update(float deltaTime);
        }
    }
}
#endif