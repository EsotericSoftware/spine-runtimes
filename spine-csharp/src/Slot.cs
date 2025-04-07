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

using System;

namespace Spine {

	/// <summary>
	/// Stores a slot's current pose. Slots organize attachments for <see cref="Skeleton.DrawOrder"/> purposes and provide a place to store
	/// state for an attachment.State cannot be stored in an attachment itself because attachments are stateless and may be shared
	/// across multiple skeletons.
	/// </summary>
	public class Slot {
		internal SlotData data;
		internal Bone bone;
		internal float r, g, b, a;
		internal float r2, g2, b2;
		internal bool hasSecondColor;
		internal Attachment attachment;
		internal int sequenceIndex;
		internal ExposedList<float> deform = new ExposedList<float>();
		internal int attachmentState;

		public Slot (SlotData data, Bone bone) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (bone == null) throw new ArgumentNullException("bone", "bone cannot be null.");
			this.data = data;
			this.bone = bone;

			// darkColor = data.darkColor == null ? null : new Color();
			if (data.hasSecondColor) {
				r2 = g2 = b2 = 0;
			}

			SetToSetupPose();
		}

		/// <summary>Copy constructor.</summary>
		public Slot (Slot slot, Bone bone) {
			if (slot == null) throw new ArgumentNullException("slot", "slot cannot be null.");
			if (bone == null) throw new ArgumentNullException("bone", "bone cannot be null.");
			data = slot.data;
			this.bone = bone;
			r = slot.r;
			g = slot.g;
			b = slot.b;
			a = slot.a;

			// darkColor = slot.darkColor == null ? null : new Color(slot.darkColor);
			if (slot.hasSecondColor) {
				r2 = slot.r2;
				g2 = slot.g2;
				b2 = slot.b2;
			} else {
				r2 = g2 = b2 = 0;
			}
			hasSecondColor = slot.hasSecondColor;

			attachment = slot.attachment;
			sequenceIndex = slot.sequenceIndex;
			deform.AddRange(slot.deform);
		}

		/// <summary>The slot's setup pose data.</summary>
		public SlotData Data { get { return data; } }
		/// <summary>The bone this slot belongs to.</summary>
		public Bone Bone { get { return bone; } }
		/// <summary>The skeleton this slot belongs to.</summary>
		public Skeleton Skeleton { get { return bone.skeleton; } }
		/// <summary>The color used to tint the slot's attachment. If <see cref="HasSecondColor"/> is set, this is used as the light color for two
		/// color tinting.</summary>
		public float R { get { return r; } set { r = value; } }
		/// <summary>The color used to tint the slot's attachment. If <see cref="HasSecondColor"/> is set, this is used as the light color for two
		/// color tinting.</summary>
		public float G { get { return g; } set { g = value; } }
		/// <summary>The color used to tint the slot's attachment. If <see cref="HasSecondColor"/> is set, this is used as the light color for two
		/// color tinting.</summary>
		public float B { get { return b; } set { b = value; } }
		/// <summary>The color used to tint the slot's attachment. If <see cref="HasSecondColor"/> is set, this is used as the light color for two
		/// color tinting.</summary>
		public float A { get { return a; } set { a = value; } }

		public void ClampColor () {
			r = MathUtils.Clamp(r, 0, 1);
			g = MathUtils.Clamp(g, 0, 1);
			b = MathUtils.Clamp(b, 0, 1);
			a = MathUtils.Clamp(a, 0, 1);
		}

		/// <summary>The dark color used to tint the slot's attachment for two color tinting, ignored if two color tinting is not used.</summary>
		/// <seealso cref="HasSecondColor"/>
		public float R2 { get { return r2; } set { r2 = value; } }
		/// <summary>The dark color used to tint the slot's attachment for two color tinting, ignored if two color tinting is not used.</summary>
		/// <seealso cref="HasSecondColor"/>
		public float G2 { get { return g2; } set { g2 = value; } }
		/// <summary>The dark color used to tint the slot's attachment for two color tinting, ignored if two color tinting is not used.</summary>
		/// <seealso cref="HasSecondColor"/>
		public float B2 { get { return b2; } set { b2 = value; } }
		/// <summary>Whether R2 G2 B2 are used to tint the slot's attachment for two color tinting. False if two color tinting is not used.</summary>
		public bool HasSecondColor { get { return data.hasSecondColor; } set { data.hasSecondColor = value; } }

		public void ClampSecondColor () {
			r2 = MathUtils.Clamp(r2, 0, 1);
			g2 = MathUtils.Clamp(g2, 0, 1);
			b2 = MathUtils.Clamp(b2, 0, 1);
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
			set {
				if (deform == null) throw new ArgumentNullException("deform", "deform cannot be null.");
				deform = value;
			}
		}

		/// <summary>Sets this slot to the setup pose.</summary>
		public void SetToSetupPose () {
			r = data.r;
			g = data.g;
			b = data.b;
			a = data.a;

			// if (darkColor != null) darkColor.set(data.darkColor);
			if (HasSecondColor) {
				r2 = data.r2;
				g2 = data.g2;
				b2 = data.b2;
			}

			if (data.attachmentName == null)
				Attachment = null;
			else {
				attachment = null;
				Attachment = bone.skeleton.GetAttachment(data.index, data.attachmentName);
			}
		}

		override public string ToString () {
			return data.name;
		}
	}
}
