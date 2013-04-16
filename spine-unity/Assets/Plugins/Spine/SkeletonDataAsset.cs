using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

public class SkeletonDataAsset : ScriptableObject {
	public AtlasAsset atlasAsset;
	public TextAsset skeletonJSON;
	public float scale = 1;
	public String[] fromAnimation;
	public String[] toAnimation;
	public float[] duration;
	private SkeletonData skeletonData;
	private AnimationStateData stateData;

	public void Clear () {
		skeletonData = null;
		stateData = null;
	}

	public SkeletonData GetSkeletonData (bool quiet) {
		if (atlasAsset == null) {
			if (!quiet)
				Debug.LogWarning("Atlas not set for skeleton data asset: " + name, this);
			Clear();
			return null;
		}

		if (skeletonJSON == null) {
			if (!quiet)
				Debug.LogWarning("Skeleton JSON file not set for skeleton data asset: " + name, this);
			Clear();
			return null;
		}

		Atlas atlas = atlasAsset.GetAtlas();
		if (atlas == null) {
			Clear();
			return null;
		}

		if (skeletonData != null)
			return skeletonData;

		SkeletonJson json = new SkeletonJson(atlas);
		json.Scale = scale;
		try {
			skeletonData = json.ReadSkeletonData(new StringReader(skeletonJSON.text));
		} catch (Exception) {
			if (!quiet)
				Debug.LogException(new Exception("Error reading skeleton JSON file for skeleton data asset: " + name), this);
			return null;
		}

		stateData = new AnimationStateData(skeletonData);
		for (int i = 0, n = fromAnimation.Length; i < n; i++)
			stateData.SetMix(fromAnimation[i], toAnimation[i], duration[i]);

		return skeletonData;
	}

	public AnimationStateData  GetAnimationStateData () {
		if (stateData != null)
			return stateData;
		GetSkeletonData(false);
		return stateData;
	}
}
