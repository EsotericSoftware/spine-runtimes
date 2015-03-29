/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

public class SkeletonDataAsset : ScriptableObject {
	public AtlasAsset[] atlasAssets;
#if SPINE_TK2D
	public tk2dSpriteCollectionData spriteCollection;
#endif
	public TextAsset skeletonJSON;
	public float scale = 1;
	public String[] fromAnimation;
	public String[] toAnimation;
	public float[] duration;
	public float defaultMix;
	public RuntimeAnimatorController controller;
	private SkeletonData skeletonData;
	private AnimationStateData stateData;
	private double loadTime = -1;

	public double LoadTime {
		get {
			return loadTime;
		}
	}

	

	public void Reset() {
		skeletonData = null;
		stateData = null;
	}

	public SkeletonData GetSkeletonData(bool quiet) {
		if (skeletonData == null) {
			loadTime = -1;
		}
			

		DateTime startLoadTime = DateTime.Now;

		if (atlasAssets == null) {
			atlasAssets = new AtlasAsset[0];
			if (!quiet)
				Debug.LogError("Atlas not set for SkeletonData asset: " + name, this);
			Reset();
			return null;
		}

		if (skeletonJSON == null) {
			if (!quiet)
				Debug.LogError("Skeleton JSON file not set for SkeletonData asset: " + name, this);
			Reset();
			return null;
		}

#if !SPINE_TK2D
		if (atlasAssets.Length == 0) {
			Reset();
			return null;
		}
#else
		if (atlasAssets.Length == 0 && spriteCollection == null) {
			Reset();
			return null;
		}
#endif

		Atlas[] atlasArr = new Atlas[atlasAssets.Length];
		for (int i = 0; i < atlasAssets.Length; i++) {
			if (atlasAssets[i] == null) {
				Reset();
				return null;
			}
			atlasArr[i] = atlasAssets[i].GetAtlas();
			if (atlasArr[i] == null) {
				Reset();
				return null;
			}
		}

		if (skeletonData != null)
			return skeletonData;

		bool isBinary = skeletonJSON.name.EndsWith(".skel");

		if (isBinary) {
			SkeletonBinary binary;

#if !SPINE_TK2D
			binary = new SkeletonBinary(atlasArr);
			binary.Scale = scale;
#else
		if (spriteCollection != null) {
			binary = new SkeletonBinary(new SpriteCollectionAttachmentLoader(spriteCollection));
			binary.Scale = (1.0f / (spriteCollection.invOrthoSize * spriteCollection.halfTargetHeight) * scale) * 100f;
		} else {
			if (atlasArr.Length == 0) {
				Reset();
				if (!quiet)
					Debug.LogError("Atlas not set for SkeletonData asset: " + name, this);
				return null;
			}
			binary = new SkeletonBinary(atlasArr);
			binary.Scale = scale;
		}
#endif
			try {
				skeletonData = binary.ReadSkeletonData(new BufferedStream(new MemoryStream(skeletonJSON.bytes)));
			} catch (Exception ex) {
				if (!quiet)
					Debug.LogError("Error reading skeleton binary file for SkeletonData asset: " + name + "\n" + ex.Message + "\n" + ex.StackTrace, this);
				return null;
			}

		} else {
			SkeletonJson json;

#if !SPINE_TK2D
			json = new SkeletonJson(atlasArr);
			json.Scale = scale;
#else
		if (spriteCollection != null) {
			json = new SkeletonJson(new SpriteCollectionAttachmentLoader(spriteCollection));
			json.Scale = (1.0f / (spriteCollection.invOrthoSize * spriteCollection.halfTargetHeight) * scale) * 100f;
		} else {
			if (atlasArr.Length == 0) {
				Reset();
				if (!quiet)
					Debug.LogError("Atlas not set for SkeletonData asset: " + name, this);
				return null;
			}
			json = new SkeletonJson(atlasArr);
			json.Scale = scale;
		}
#endif


			try {
				skeletonData = json.ReadSkeletonData(new StringReader(skeletonJSON.text));
			} catch (Exception ex) {
				if (!quiet)
					Debug.LogError("Error reading skeleton JSON file for SkeletonData asset: " + name + "\n" + ex.Message + "\n" + ex.StackTrace, this);
				return null;
			}
		}

		stateData = new AnimationStateData(skeletonData);
		FillStateData();

		loadTime = (DateTime.Now - startLoadTime).TotalMilliseconds;

		return skeletonData;
	}

	public void FillStateData () {
		if (stateData == null)
			return;

		stateData.DefaultMix = defaultMix;
		for (int i = 0, n = fromAnimation.Length; i < n; i++) {
			if (fromAnimation[i].Length == 0 || toAnimation[i].Length == 0)
				continue;
			stateData.SetMix(fromAnimation[i], toAnimation[i], duration[i]);
		}
	}

	public AnimationStateData GetAnimationStateData() {
		if (stateData != null)
			return stateData;
		GetSkeletonData(false);
		return stateData;
	}
}
