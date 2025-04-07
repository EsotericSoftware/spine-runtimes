/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

using UnityEngine;

namespace Spine.Unity {
	public static class MathUtilities {
		public static float InverseLerp (float a, float b, float value) {
			return (value - a) / (b - a);
		}

		/// <summary>
		/// Returns the linear interpolation ratio of <c>a</c> to <c>b</c> that <c>value</c> lies on.
		/// This is the t value that fulfills <c>value = lerp(a, b, t)</c>.
		/// </summary>
		public static Vector2 InverseLerp (Vector2 a, Vector2 b, Vector2 value) {
			return new Vector2(
				(value.x - a.x) / (b.x - a.x),
				(value.y - a.y) / (b.y - a.y));
		}

		/// <summary>
		/// Returns the linear interpolation ratio of <c>a</c> to <c>b</c> that <c>value</c> lies on.
		/// This is the t value that fulfills <c>value = lerp(a, b, t)</c>.
		/// </summary>
		public static Vector3 InverseLerp (Vector3 a, Vector3 b, Vector3 value) {
			return new Vector3(
				(value.x - a.x) / (b.x - a.x),
				(value.y - a.y) / (b.y - a.y),
				(value.z - a.z) / (b.z - a.z));
		}

		/// <summary>
		/// Returns the linear interpolation ratio of <c>a</c> to <c>b</c> that <c>value</c> lies on.
		/// This is the t value that fulfills <c>value = lerp(a, b, t)</c>.
		/// </summary>
		public static Vector4 InverseLerp (Vector4 a, Vector4 b, Vector4 value) {
			return new Vector4(
				(value.x - a.x) / (b.x - a.x),
				(value.y - a.y) / (b.y - a.y),
				(value.z - a.z) / (b.z - a.z),
				(value.w - a.w) / (b.w - a.w));
		}
	}
}
