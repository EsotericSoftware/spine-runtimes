/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#if (UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_WSA || UNITY_WP8 || UNITY_WP8_1)
#define IS_UNITY
#endif

namespace Spine {
#if IS_UNITY
	using Color32F = UnityEngine.Color;
#endif

	/// <summary>Interface for an attachment that gets 1 or more texture regions from a <see cref="Sequence"/>.</summary>
	public interface IHasSequence {
		/// <summary>The base path for the attachment's texture region.</summary>
		string Path { get; set; }
		/// <summary>The color the attachment is tinted, to be combined with <see cref="SlotPose.Color"/>.</summary>
		Color32F GetColor ();
		void SetColor (Color32F color);
		void SetColor (float r, float g, float b, float a);
		/// <summary>The sequence that provides texture regions, UVs, and vertex offsets for rendering this attachment.</summary>
		Sequence Sequence { get; }
		void UpdateSequence ();
	}
}
