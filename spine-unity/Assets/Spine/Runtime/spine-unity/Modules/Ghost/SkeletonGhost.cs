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

// Contributed by: Mitch Thompson

using UnityEngine;
using System.Collections.Generic;

namespace Spine.Unity.Modules {
	
	[RequireComponent(typeof(SkeletonRenderer))]
	public class SkeletonGhost : MonoBehaviour {
		// Internal Settings
		const HideFlags GhostHideFlags = HideFlags.HideInHierarchy;
		const string GhostingShaderName = "Spine/Special/SkeletonGhost";

		[Header("Animation")]
		public bool ghostingEnabled = true;
		[Tooltip("The time between invididual ghost pieces being spawned.")]
		[UnityEngine.Serialization.FormerlySerializedAs("spawnRate")]
		public float spawnInterval = 1f/30f;
		[Tooltip("Maximum number of ghosts that can exist at a time. If the fade speed is not fast enough, the oldest ghost will immediately disappear to enforce the maximum number.")]
		public int maximumGhosts = 10;
		public float fadeSpeed = 10;

		[Header("Rendering")]
		public Shader ghostShader;
		public Color32 color = new Color32(0xFF, 0xFF, 0xFF, 0x00); // default for additive.
		[Tooltip("Remember to set color alpha to 0 if Additive is true")]
		public bool additive = true;
		[Tooltip("0 is Color and Alpha, 1 is Alpha only.")]
		[Range(0, 1)]
		public float textureFade = 1;

		[Header("Sorting")]
		public bool sortWithDistanceOnly;
		public float zOffset = 0f;

		float nextSpawnTime;
		SkeletonGhostRenderer[] pool;
		int poolIndex = 0;
		SkeletonRenderer skeletonRenderer;
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		readonly Dictionary<Material, Material> materialTable = new Dictionary<Material, Material>();

		void Start () {
			Initialize(false);
		}

		public void Initialize (bool overwrite) {
			if (pool == null || overwrite) {
				if (ghostShader == null)
					ghostShader = Shader.Find(GhostingShaderName);

				skeletonRenderer = GetComponent<SkeletonRenderer>();
				meshFilter = GetComponent<MeshFilter>();
				meshRenderer = GetComponent<MeshRenderer>();
				nextSpawnTime = Time.time + spawnInterval;
				pool = new SkeletonGhostRenderer[maximumGhosts];
				for (int i = 0; i < maximumGhosts; i++) {
					GameObject go = new GameObject(gameObject.name + " Ghost", typeof(SkeletonGhostRenderer));
					pool[i] = go.GetComponent<SkeletonGhostRenderer>();
					go.SetActive(false);
					go.hideFlags = GhostHideFlags;
				}

				var skeletonAnimation = skeletonRenderer as Spine.Unity.IAnimationStateComponent;
				if (skeletonAnimation != null)
					skeletonAnimation.AnimationState.Event += OnEvent;
			}
		}

		//SkeletonAnimation
		/*
		 *	Int Value:		0 sets ghostingEnabled to false, 1 sets ghostingEnabled to true
		 *	Float Value:	Values greater than 0 set the spawnRate equal the float value
		 *	String Value:	Pass RGBA hex color values in to set the color property.  IE:   "A0FF8BFF"
		 */
		void OnEvent (Spine.TrackEntry trackEntry, Spine.Event e) {
			if (e.Data.Name.Equals("Ghosting", System.StringComparison.Ordinal)) {
				ghostingEnabled = e.Int > 0;
				if (e.Float > 0)
					spawnInterval = e.Float;
				
				if (!string.IsNullOrEmpty(e.stringValue))
					this.color = HexToColor(e.String);
			}
		}

		//SkeletonAnimator
		//SkeletonAnimator or Mecanim based animations only support toggling ghostingEnabled.  Be sure not to set anything other than the Int param in Spine or String will take priority.
		void Ghosting (float val) {
			ghostingEnabled = val > 0;
		}

		void Update () {
			if (!ghostingEnabled)
				return;

			if (Time.time >= nextSpawnTime) {
				GameObject go = pool[poolIndex].gameObject;

				Material[] materials = meshRenderer.sharedMaterials;
				for (int i = 0; i < materials.Length; i++) {
					var originalMat = materials[i];
					Material ghostMat;
					if (!materialTable.ContainsKey(originalMat)) {
						ghostMat = new Material(originalMat) {
							shader = ghostShader,
							color = Color.white
						};

						if (ghostMat.HasProperty("_TextureFade"))
							ghostMat.SetFloat("_TextureFade", textureFade);

						materialTable.Add(originalMat, ghostMat);
					} else {
						ghostMat = materialTable[originalMat];
					}

					materials[i] = ghostMat;
				}

				var goTransform = go.transform;
				goTransform.parent = transform;

				pool[poolIndex].Initialize(meshFilter.sharedMesh, materials, color, additive, fadeSpeed, meshRenderer.sortingLayerID, (sortWithDistanceOnly) ? meshRenderer.sortingOrder : meshRenderer.sortingOrder - 1);

				goTransform.localPosition = new Vector3(0f, 0f, zOffset);
				goTransform.localRotation = Quaternion.identity;
				goTransform.localScale = Vector3.one;

				goTransform.parent = null;

				poolIndex++;

				if (poolIndex == pool.Length)
					poolIndex = 0;

				nextSpawnTime = Time.time + spawnInterval;
			}
		}

		void OnDestroy () {
			if (pool != null) {
				for (int i = 0; i < maximumGhosts; i++)
					if (pool[i] != null) pool[i].Cleanup();
			}

			foreach (var mat in materialTable.Values)
				Destroy(mat);
		}

		//based on UnifyWiki  http://wiki.unity3d.com/index.php?title=HexConverter
		static Color32 HexToColor (string hex) {
			const System.Globalization.NumberStyles HexNumber = System.Globalization.NumberStyles.HexNumber;

			if (hex.Length < 6)
				return Color.magenta;

			hex = hex.Replace("#", "");
			byte r = byte.Parse(hex.Substring(0, 2), HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), HexNumber);
			byte a = 0xFF;
			if (hex.Length == 8)
				a = byte.Parse(hex.Substring(6, 2), HexNumber);

			return new Color32(r, g, b, a);
		}
	}

}
