/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using UnityEngine;
using Spine;

namespace Spine.Unity {
	public class SkeletonDataAsset : ScriptableObject {
		#region Inspector
		public AtlasAsset[] atlasAssets = new AtlasAsset[0];
		#if SPINE_TK2D
		public tk2dSpriteCollectionData spriteCollection;
		public float scale = 1f;
		#else
		public float scale = 0.01f;
		#endif
		public TextAsset skeletonJSON;
		public string[] fromAnimation = new string[0];
		public string[] toAnimation = new string[0];
		public float[] duration = new float[0];
		public float defaultMix;
		public RuntimeAnimatorController controller;

		public bool IsLoaded { get { return this.skeletonData != null; } }

		void Reset () {
			Clear();
		}
		#endregion

		SkeletonData skeletonData;
		AnimationStateData stateData;

		#region Runtime Instantiation
		/// <summary>
		/// Creates a runtime SkeletonDataAsset.</summary>
		public static SkeletonDataAsset CreateRuntimeInstance (TextAsset skeletonDataFile, AtlasAsset atlasAsset, bool initialize, float scale = 0.01f) {
			return CreateRuntimeInstance(skeletonDataFile, new [] {atlasAsset}, initialize, scale);
		}

		/// <summary>
		/// Creates a runtime SkeletonDataAsset.</summary>
		public static SkeletonDataAsset CreateRuntimeInstance (TextAsset skeletonDataFile, AtlasAsset[] atlasAssets, bool initialize, float scale = 0.01f) {
			SkeletonDataAsset skeletonDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();
			skeletonDataAsset.Clear();
			skeletonDataAsset.skeletonJSON = skeletonDataFile;
			skeletonDataAsset.atlasAssets = atlasAssets;
			skeletonDataAsset.scale = scale;

			if (initialize)
				skeletonDataAsset.GetSkeletonData(true);

			return skeletonDataAsset;
		}
		#endregion

		public void Clear () {
			skeletonData = null;
			stateData = null;
		}

		public SkeletonData GetSkeletonData (bool quiet) {
			if (skeletonJSON == null) {
				if (!quiet)
					Debug.LogError("Skeleton JSON file not set for SkeletonData asset: " + name, this);
				Clear();
				return null;
			}

			// Support attachmentless/skinless SkeletonData.
//			if (atlasAssets == null) {
//				atlasAssets = new AtlasAsset[0];
//				if (!quiet)
//					Debug.LogError("Atlas not set for SkeletonData asset: " + name, this);
//				Clear();
//				return null;
//			}
//			#if !SPINE_TK2D
//			if (atlasAssets.Length == 0) {
//				Clear();
//				return null;
//			}
//			#else
//			if (atlasAssets.Length == 0 && spriteCollection == null) {
//				Clear();
//				return null;
//			}
//			#endif

			if (skeletonData != null)
				return skeletonData;

			AttachmentLoader attachmentLoader;
			float skeletonDataScale;
			Atlas[] atlasArray = this.GetAtlasArray();

			#if !SPINE_TK2D
			attachmentLoader = new AtlasAttachmentLoader(atlasArray);
			skeletonDataScale = scale;
			#else
			if (spriteCollection != null) {
				attachmentLoader = new Spine.Unity.TK2D.SpriteCollectionAttachmentLoader(spriteCollection);
				skeletonDataScale = (1.0f / (spriteCollection.invOrthoSize * spriteCollection.halfTargetHeight) * scale);
			} else {
				if (atlasArray.Length == 0) {
					Reset();
					if (!quiet) Debug.LogError("Atlas not set for SkeletonData asset: " + name, this);
					return null;
				}
				attachmentLoader = new AtlasAttachmentLoader(atlasArray);
				skeletonDataScale = scale;
			}
			#endif

			bool isBinary = skeletonJSON.name.ToLower().Contains(".skel");
			SkeletonData loadedSkeletonData;

			try {
				if (isBinary)
					loadedSkeletonData = SkeletonDataAsset.ReadSkeletonData(skeletonJSON.bytes, attachmentLoader, skeletonDataScale);
				else
					loadedSkeletonData = SkeletonDataAsset.ReadSkeletonData(skeletonJSON.text, attachmentLoader, skeletonDataScale);

			} catch (Exception ex) {
				if (!quiet)
					Debug.LogError("Error reading skeleton JSON file for SkeletonData asset: " + name + "\n" + ex.Message + "\n" + ex.StackTrace, this);
				return null;

			}

			this.InitializeWithData(loadedSkeletonData);

			return skeletonData;
		}

		internal void InitializeWithData (SkeletonData sd) {
			this.skeletonData = sd;
			this.stateData = new AnimationStateData(skeletonData);
			FillStateData();
		}

		internal Atlas[] GetAtlasArray () {
			var returnList = new System.Collections.Generic.List<Atlas>(atlasAssets.Length);
			for (int i = 0; i < atlasAssets.Length; i++) {
				var aa = atlasAssets[i];
				if (aa == null) continue;
				var a = aa.GetAtlas();
				if (a == null) continue;
				returnList.Add(a);
			}
			return returnList.ToArray();
		}

		internal static SkeletonData ReadSkeletonData (byte[] bytes, AttachmentLoader attachmentLoader, float scale) {
			var input = new MemoryStream(bytes);
			var binary = new SkeletonBinary(attachmentLoader) {
				Scale = scale
			};
			return binary.ReadSkeletonData(input);
		}

		internal static SkeletonData ReadSkeletonData (string text, AttachmentLoader attachmentLoader, float scale) {
			var input = new StringReader(text);
			var json = new SkeletonJson(attachmentLoader) {
				Scale = scale
			};
			return json.ReadSkeletonData(input);
		}

		public void FillStateData () {
			if (stateData != null) {
				stateData.defaultMix = defaultMix;

				for (int i = 0, n = fromAnimation.Length; i < n; i++) {
					if (fromAnimation[i].Length == 0 || toAnimation[i].Length == 0)
						continue;
					stateData.SetMix(fromAnimation[i], toAnimation[i], duration[i]);
				}
			}
		}

		public AnimationStateData GetAnimationStateData () {
			if (stateData != null)
				return stateData;
			GetSkeletonData(false);
			return stateData;
		}
	}

}
