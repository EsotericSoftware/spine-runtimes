#if UNITY_2017 || UNITY_2018 || (UNITY_2019_1_OR_NEWER && SPINE_TIMELINE_PACKAGE_DOWNLOADED)
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class SpineSkeletonFlipBehaviour : PlayableBehaviour {
    public bool flipX, flipY;
}
#endif