/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {

	// This is a sample component for C# vertex effects for Spine rendering components.
	// Using shaders and materials to control vertex properties is still more performant
	// than using this API, but in cases where your vertex effect logic cannot be
	// expressed as shader code, these vertex effects can be useful.
	public class JitterEffectExample : MonoBehaviour {

		[Range(0f, 0.8f)]
		public float jitterMagnitude = 0.2f;

		SkeletonRenderer skeletonRenderer;

		void OnEnable () {
			skeletonRenderer = GetComponent<SkeletonRenderer>();
			if (skeletonRenderer == null) return;

			// Use the OnPostProcessVertices callback to modify the vertices at the correct time.
			skeletonRenderer.OnPostProcessVertices -= ProcessVertices;
			skeletonRenderer.OnPostProcessVertices += ProcessVertices;

			Debug.Log("Jitter Effect Enabled.");
		}

		void ProcessVertices (MeshGeneratorBuffers buffers) {
			if (!this.enabled) return;

			// For efficiency, limit your effect to the actual mesh vertex count using vertexCount
			int vertexCount = buffers.vertexCount;

			// Modify vertex positions by accessing Vector3[] vertexBuffer
			var vertices = buffers.vertexBuffer;
			for (int i = 0; i < vertexCount; i++)
				vertices[i] += (Vector3)(Random.insideUnitCircle * jitterMagnitude);

			// You can also modify uvs and colors.
			//var uvs = buffers.uvBuffer;
			//var colors = buffers.colorBuffer;

			//
		}

		void OnDisable () {
			if (skeletonRenderer == null) return;
			skeletonRenderer.OnPostProcessVertices -= ProcessVertices;

			Debug.Log("Jitter Effect Disabled.");
		}
	}

}
