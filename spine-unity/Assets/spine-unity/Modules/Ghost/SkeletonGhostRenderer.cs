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
using System.Collections;

namespace Spine.Unity.Modules {
	public class SkeletonGhostRenderer : MonoBehaviour {

		public float fadeSpeed = 10;

		Color32[] colors;
		Color32 black = new Color32(0, 0, 0, 0);
		MeshFilter meshFilter;
		MeshRenderer meshRenderer;

		void Awake () {
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshFilter = gameObject.AddComponent<MeshFilter>();
		}

		public void Initialize (Mesh mesh, Material[] materials, Color32 color, bool additive, float speed, int sortingLayerID, int sortingOrder) {
			StopAllCoroutines();

			gameObject.SetActive(true);
			meshRenderer.sharedMaterials = materials;
			meshRenderer.sortingLayerID = sortingLayerID;
			meshRenderer.sortingOrder = sortingOrder;
			meshFilter.sharedMesh = Instantiate(mesh);
			colors = meshFilter.sharedMesh.colors32;

			if ((color.a + color.r + color.g + color.b) > 0) {
				for (int i = 0; i < colors.Length; i++)
					colors[i] = color;
			}

			fadeSpeed = speed;

			if (additive)
				StartCoroutine(FadeAdditive());
			else
				StartCoroutine(Fade());
		}

		IEnumerator Fade () {
			Color32 c;
			for (int t = 0; t < 500; t++) {
				bool breakout = true;
				for (int i = 0; i < colors.Length; i++) {
					c = colors[i];
					if (c.a > 0)
						breakout = false;

					colors[i] = Color32.Lerp(c, black, Time.deltaTime * fadeSpeed);
				}
				meshFilter.sharedMesh.colors32 = colors;

				if (breakout)
					break;
				
				yield return null;
			}

			Destroy(meshFilter.sharedMesh);
			gameObject.SetActive(false);
		}

		IEnumerator FadeAdditive () {
			Color32 c;
			Color32 black = this.black;

			for (int t = 0; t < 500; t++) {

				bool breakout = true;
				for (int i = 0; i < colors.Length; i++) {
					c = colors[i];
					black.a = c.a;
					if (c.r > 0 || c.g > 0 || c.b > 0)
						breakout = false;

					colors[i] = Color32.Lerp(c, black, Time.deltaTime * fadeSpeed);
				}

				meshFilter.sharedMesh.colors32 = colors;

				if (breakout)
					break;
				yield return null;
			}

			Destroy(meshFilter.sharedMesh);

			gameObject.SetActive(false);
		}

		public void Cleanup () {
			if (meshFilter != null && meshFilter.sharedMesh != null)
				Destroy(meshFilter.sharedMesh);

			Destroy(gameObject);
		}
	}

}
