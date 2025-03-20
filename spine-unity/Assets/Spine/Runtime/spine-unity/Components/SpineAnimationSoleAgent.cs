using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Spine.Unity;
using UnityEngine;
using Object = System.Object;

namespace Spine.Unity
{
    [ExecuteAlways]
    public class SpineAnimationSoleAgent : MonoBehaviour
    {
        private void Start()
        {
            var animations = FindObjectsByType<SkeletonAnimation>(FindObjectsSortMode.None);

            for (int i = 0; i < animations.Length; i++)
            {
                SpineAnimationAgentManager.Register(animations[i]);
            }
        }

        private void Update()
        {
            SpineAnimationAgentManager.Update();
        }

        private void LateUpdate()
        {
            SpineAnimationAgentManager.LateUpdate();
        }

        private void OnDestroy()
        {
        }
    }

    public class SpineAnimationAgentManager
    {
        public static bool isPlaying;
        public static int currentFrame;

        public static void Register(SkeletonAnimation anim)
        {
            Initialize();

            for (int i = 0; i < _bags.Length; i++)
            {
                var bag = _bags[i];
                if (bag != null && bag.animation == anim)
                {
                    return;
                }
            }


            for (int i = 0; i < _bags.Length; i++)
            {
                var bag = _bags[i];
                if (bag == null || !bag.isValid)
                {
                    _bags[i] = new AnimationBag()
                    {
                        animation = anim,
                        isNeedUpdateMeshFlag = false
                    };
                    break;
                }
            }
        }

        public static void Unregister(SkeletonAnimation anim)
        {
            for (int i = 0; i < _bags.Length; i++)
            {
                var bag = _bags[i];
                if (bag != null && bag.animation == anim)
                {
                    _bags[i] = null;
                    break;
                }
            }
        }

        private static bool isInitialized;

        private static AnimationBag[] _bags = new AnimationBag[64];
        private static float _time;

        private static OrderablePartitioner<AnimationBag> _partitioner =
            Partitioner.Create(_bags, EnumerablePartitionerOptions.NoBuffering);

        private static ParallelOptions _parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = Mathf.Max(1, Environment.ProcessorCount * 2 - 1)
        };


        public static void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;


            var scripts = Resources.FindObjectsOfTypeAll<SpineAnimationSoleAgent>();
            // Debug.LogError($"===>{scripts.Length}");
            foreach (var script in scripts)
            {
                UnityEngine.Object.Destroy(script.gameObject);
            }

            var go = new GameObject("SpineAnimationSoleAgent", typeof(SpineAnimationSoleAgent));
            go.hideFlags = HideFlags.HideAndDontSave;
#if !UNITY_EDITOR
            UnityEngine.Object.DontDestroyOnLoad(go);
#endif
        }


        private static void ParallelProcessBags(Action<AnimationBag> processAction)
        {
            Parallel.ForEach(_partitioner, _parallelOptions, bag =>
            {
                if (bag == null || !bag.isValid) return;
                // Debug.Log($"{currentFrame} is processing  {Thread.CurrentThread.ManagedThreadId}");

                try
                {
                    processAction(bag);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
        }

        public static void Update()
        {
            isPlaying = Application.isPlaying;
            currentFrame = Time.frameCount;
            _time = Time.deltaTime;
            Debug.Log($"{currentFrame} ==>");
            // 更新动画
            ParallelProcessBags(bag => bag.animation.UpdateAnimation(_time));
            // 应用物理数据
            for (int i = 0; i < _bags.Length; i++)
            {
                var bag = _bags[i];
                if (bag != null && bag.isValid)
                    _bags[i].animation.ApplyPhysicsData();
            }

            // 应用数据
            ParallelProcessBags(bag => bag.animation.ApplyData());
        }

        public static void LateUpdate()
        {
            // 检查是否需要更新网格
            for (int i = 0; i < _bags.Length; i++)
            {
                var item = _bags[i];
                if (item != null && item.isValid)
                {
                    item.isNeedUpdateMeshFlag = item.animation.CheckIsNeedUpdateMesh();
                    _bags[i] = item;
                }
            }

            // 更新网格
            ParallelProcessBags(bag =>
            {
                if (bag.isNeedUpdateMeshFlag)
                {
                    bag.animation.LateUpdateMesh();
                }
            });

            // 填充网格
            for (int i = 0; i < _bags.Length; i++)
            {
                var bag = _bags[i];
                if (bag != null && bag.isValid)
                    _bags[i].animation.FillMesh();
            }
        }

        public static void OnDestroy()
        {
            for (int i = 0; i < _bags.Length; i++)
            {
                _bags[i] = null;
            }
        }
    }

    public class AnimationBag
    {
        public SkeletonAnimation animation;
        public bool isNeedUpdateMeshFlag;

        public bool isValid => animation;
    }
}