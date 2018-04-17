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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {

	// This is a sample component for C# vertex effects for Spine rendering components.
	// Using shaders and materials to control vertex properties is still more performant
	// than using this API, but in cases where your vertex effect logic cannot be
	// expressed as shader code, these vertex effects can be useful.
	public class TwoByTwoTransformEffectExample : MonoBehaviour {

		public Vector2 xAxis = new Vector2(1, 0);
		public Vector2 yAxis = new Vector2(0, 1);

		SkeletonRenderer skeletonRenderer;

		void OnEnable () {
			skeletonRenderer = GetComponent<SkeletonRenderer>();
			if (skeletonRenderer == null) return;

			// Use the OnPostProcessVertices callback to modify the vertices at the correct time.
			skeletonRenderer.OnPostProcessVertices -= ProcessVertices;
			skeletonRenderer.OnPostProcessVertices += ProcessVertices;

			Debug.Log("2x2 Transform Effect Enabled.");
		}

		void ProcessVertices (MeshGeneratorBuffers buffers) {
			if (!this.enabled)
				return;

			int vertexCount = buffers.vertexCount; // For efficiency, limit your effect to the actual mesh vertex count using vertexCount

			// Modify vertex positions by accessing Vector3[] vertexBuffer
			var vertices = buffers.vertexBuffer;
			Vector3 transformedPos = default(Vector3);
			for (int i = 0; i < vertexCount; i++) {
				Vector3 originalPos = vertices[i];
				transformedPos.x = (xAxis.x * originalPos.x) + (yAxis.x * originalPos.y);
				transformedPos.y = (xAxis.y * originalPos.x) + (yAxis.y * originalPos.y);
				vertices[i] = transformedPos;
			}

		}

		void OnDisable () {
			if (skeletonRenderer == null) return;
			skeletonRenderer.OnPostProcessVertices -= ProcessVertices;
			Debug.Log("2x2 Transform Effect Disabled.");
		}
	}

}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(Spine.Unity.Examples.TwoByTwoTransformEffectExample))]
public class TwoByTwoTransformEffectExampleEditor : UnityEditor.Editor {

	Spine.Unity.Examples.TwoByTwoTransformEffectExample Target { get { return target as Spine.Unity.Examples.TwoByTwoTransformEffectExample; } }

	void OnSceneGUI () {
		var transform = Target.transform;
		LocalVectorHandle(ref Target.xAxis, transform, Color.red);
		LocalVectorHandle(ref Target.yAxis, transform, Color.green);
	}

	static void LocalVectorHandle (ref Vector2 v, Transform transform, Color color) {
		Color originalColor = UnityEditor.Handles.color;
		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawLine(transform.position, transform.TransformPoint(v));
		v = transform.InverseTransformPoint(UnityEditor.Handles.FreeMoveHandle(transform.TransformPoint(v), Quaternion.identity, 0.3f, Vector3.zero, UnityEditor.Handles.CubeHandleCap));
		UnityEditor.Handles.color = originalColor;
	}
}
#endif
