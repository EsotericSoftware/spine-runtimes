/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using UnityEngine;
using Spine;

public class SkeletonDataAsset : ScriptableObject {
	public tk2dSpriteCollectionData spriteCollection;
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
		if (spriteCollection == null) {
			if (!quiet)
				Debug.LogWarning("Sprite collection not set for skeleton data asset: " + name, this);
			Clear();
			return null;
		}

		if (skeletonJSON == null) {
			if (!quiet)
				Debug.LogWarning("Skeleton JSON file not set for skeleton data asset: " + name, this);
			Clear();
			return null;
		}

		if (skeletonData != null)
			return skeletonData;

		SkeletonJson json = new SkeletonJson(new SpriteCollectionAttachmentLoader(spriteCollection));
		json.Scale = 1.0f / (spriteCollection.invOrthoSize * spriteCollection.halfTargetHeight) * scale;

		try {
			skeletonData = json.ReadSkeletonData(new StringReader(skeletonJSON.text));
		} catch (Exception ex) {
			Debug.Log("Error reading skeleton JSON file for skeleton data asset: " + name + "\n" +
				ex.Message + "\n" + ex.StackTrace, this);
			return null;
		}

		stateData = new AnimationStateData(skeletonData);
		for (int i = 0, n = fromAnimation.Length; i < n; i++) {
			if (fromAnimation[i].Length == 0 || toAnimation[i].Length == 0)
				continue;
			stateData.SetMix(fromAnimation[i], toAnimation[i], duration[i]);
		}

		return skeletonData;
	}

	public AnimationStateData GetAnimationStateData () {
		if (stateData != null)
			return stateData;
		GetSkeletonData(false);
		return stateData;
	}
}
