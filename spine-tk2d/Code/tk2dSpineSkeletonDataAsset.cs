using System;
using System.IO;
using UnityEngine;
using Spine;

public class tk2dSpineSkeletonDataAsset : ScriptableObject {
	public tk2dSpriteCollectionData spritesData;
	public tk2dSpriteCollection.NormalGenerationMode normalGenerationMode = tk2dSpriteCollection.NormalGenerationMode.None;

	public TextAsset skeletonJSON;
	
	public float scale = 1;
	
	public string[] fromAnimation;
	public string[] toAnimation;
	public float[] duration;
	
	private SkeletonData skeletonData;
	private AnimationStateData stateData;
	
	public SkeletonData GetSkeletonData() {
		if (skeletonData != null) return skeletonData;
		
		MakeSkeletonAndAnimationData();
		return skeletonData;
	}
	
	public AnimationStateData GetAnimationStateData () {
		if (stateData != null) return stateData;
		
		MakeSkeletonAndAnimationData();
		return stateData;
	}
	
	private void MakeSkeletonAndAnimationData() {
		if (spritesData == null) {
			Debug.LogWarning("Sprite collection not set for skeleton data asset: " + name,this);
			return;
		}
		
		if (skeletonJSON == null) {
			Debug.LogWarning("Skeleton JSON file not set for skeleton data asset: " + name,this);
			return;
		}
		
		SkeletonJson json = new SkeletonJson(new tk2dSpineAttachmentLoader(spritesData));
		json.Scale = scale;
		
		try {
			skeletonData = json.ReadSkeletonData(new StringReader(skeletonJSON.text));
		} catch (Exception ex) {
			Debug.Log("Error reading skeleton JSON file for skeleton data asset: " + name + "\n" + ex.Message + "\n" + ex.StackTrace,this);
			return;
		}
		
		stateData = new AnimationStateData(skeletonData);
		for (int i = 0, n = fromAnimation.Length; i < n; i++) {
			if (fromAnimation[i].Length == 0 || toAnimation[i].Length == 0) continue;
			stateData.SetMix(fromAnimation[i], toAnimation[i], duration[i]);
		}
	}

	public void ForceUpdate() {
		MakeSkeletonAndAnimationData();
	}
}
