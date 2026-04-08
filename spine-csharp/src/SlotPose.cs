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

#if UNITY_5_3_OR_NEWER
#define IS_UNITY
#endif

using System;

namespace Spine {
#if IS_UNITY
	using Color32F = UnityEngine.Color;
#endif

	/// <summary>Stores a slot's pose.</summary>
	public class SlotPose : IPose<SlotPose> {
		// Color is a struct, thus set to protected to prevent
		// Color color = slot.color; color.a = 0.5; modifying just a copy of the struct instead of the original
		// object as in reference implementation.
		protected Color32F color = new Color32F(1, 1, 1, 1);
		protected Color32F? darkColor = null;
		internal Attachment attachment; // Not used in setup pose.
		internal int sequenceIndex;
		internal readonly ExposedList<float> deform = new ExposedList<float>();

		internal SlotPose () {
		}

		public void Set (SlotPose pose) {
			if (pose == null) throw new ArgumentNullException("pose", "pose cannot be null.");
			color = pose.color;
			if (darkColor.HasValue) darkColor = pose.darkColor;
			attachment = pose.attachment;
			sequenceIndex = pose.sequenceIndex;
			deform.Clear(false);
			deform.AddRange(pose.deform);
		}

		/// <returns>A copy of the color used to tint the slot's attachment. If <see cref="DarkColor"/> is set, this is used as the light color for two
		/// color tinting.</returns>
		public Color32F GetColor () {
			return color;
		}

		/// <summary>Sets the color used to tint the slot's attachment. If <see cref="DarkColor"/> is set, this is used as the light color for two
		/// color tinting.</summary>
		public void SetColor (Color32F color) {
			this.color = color;
		}

		/// <summary>Clamps the <see cref="GetColor()">color</see> used to tint the slot's attachment to the 0-1 range.</summary>
		public void ClampColor () {
			color.Clamp();
		}

		/// <returns>A copy of the dark color used to tint the slot's attachment for two color tinting, or null if two color tinting is not used. The dark
		/// color's alpha is not used.</returns>
		public Color32F? GetDarkColor () {
			return darkColor;
		}

		/// <summary>Sets the dark color used to tint the slot's attachment for two color tinting, or null if two color tinting is not used. The dark
		/// color's alpha is not used.</summary>
		public void SetDarkColor (Color32F? darkColor) {
			this.darkColor = darkColor;
		}

		/// <summary>Clamps the <see cref="GetDarkColor()">dark color</see> used to tint the slot's attachment to the 0-1 range.</summary>
		public void ClampDarkColor () {
			if (darkColor.HasValue) darkColor = darkColor.Value.Clamp();
		}

		/// <summary>
		/// The current attachment for the slot, or null if the slot has no attachment.
		/// If the attachment is changed, resets <see cref="SequenceIndex"/> and clears the <see cref="Deform"/>.
		/// The deform is not cleared if the old attachment has the same <see cref="VertexAttachment.TimelineAttachment"/> as the
		/// specified attachment.</summary>
		public Attachment Attachment {
			/// <summary>The current attachment for the slot, or null if the slot has no attachment.</summary>
			get { return attachment; }
			/// <summary>
			/// Sets the slot's attachment and, if the attachment changed, resets <see cref="SequenceIndex"/> and clears the <see cref="Deform"/>.
			/// The deform is not cleared if the old attachment has the same <see cref="VertexAttachment.TimelineAttachment"/> as the
			/// specified attachment.</summary>
			/// <param name="value">May be null.</param>
			set {
				if (attachment == value) return;
				if (!(value is VertexAttachment) || !(this.attachment is VertexAttachment)
					|| ((VertexAttachment)value).TimelineAttachment != ((VertexAttachment)this.attachment).TimelineAttachment) {
					deform.Clear();
				}
				this.attachment = value;
				sequenceIndex = -1;
			}
		}

		/// <summary>
		/// The index of the texture region to display when the slot's attachment has a <see cref="Sequence"/>. -1 represents the
		/// <see cref="Sequence.SetupIndex"/>.
		/// </summary>
		public int SequenceIndex { get { return sequenceIndex; } set { sequenceIndex = value; } }

		/// <summary> Vertices to deform the slot's attachment. For an unweighted mesh, the entries are local positions for each vertex. For a
		/// weighted mesh, the entries are an offset for each vertex which will be added to the mesh's local vertex positions.
		/// <para />
		/// See <see cref="VertexAttachment.ComputeWorldVertices(Slot, int, int, float[], int, int)"/> and <see cref="DeformTimeline"/>.</summary>
		public ExposedList<float> Deform {
			get {
				return deform;
			}
		}
	}
}
