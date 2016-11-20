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

// Contributed by: Mitch Thompson and John Dy

using UnityEngine;
using Spine;

namespace Spine.Unity {
	public static class SkeletonExtensions {

		#region Colors
		const float ByteToFloat = 1f / 255f;
		public static Color GetColor (this Skeleton s) { return new Color(s.r, s.g, s.b, s.a); }
		public static Color GetColor (this RegionAttachment a) { return new Color(a.r, a.g, a.b, a.a); }
		public static Color GetColor (this MeshAttachment a) { return new Color(a.r, a.g, a.b, a.a); }

		public static void SetColor (this Skeleton skeleton, Color color) {
			skeleton.A = color.a;
			skeleton.R = color.r;
			skeleton.G = color.g;
			skeleton.B = color.b;
		}

		public static void SetColor (this Skeleton skeleton, Color32 color) {
			skeleton.A = color.a * ByteToFloat;
			skeleton.R = color.r * ByteToFloat;
			skeleton.G = color.g * ByteToFloat;
			skeleton.B = color.b * ByteToFloat;
		}

		public static void SetColor (this Slot slot, Color color) {
			slot.A = color.a;
			slot.R = color.r;
			slot.G = color.g;
			slot.B = color.b;
		}

		public static void SetColor (this Slot slot, Color32 color) {
			slot.A = color.a * ByteToFloat;
			slot.R = color.r * ByteToFloat;
			slot.G = color.g * ByteToFloat;
			slot.B = color.b * ByteToFloat;
		}

		public static void SetColor (this RegionAttachment attachment, Color color) {
			attachment.A = color.a;
			attachment.R = color.r;
			attachment.G = color.g;
			attachment.B = color.b;
		}

		public static void SetColor (this RegionAttachment attachment, Color32 color) {
			attachment.A = color.a * ByteToFloat;
			attachment.R = color.r * ByteToFloat;
			attachment.G = color.g * ByteToFloat;
			attachment.B = color.b * ByteToFloat;
		}

		public static void SetColor (this MeshAttachment attachment, Color color) {
			attachment.A = color.a;
			attachment.R = color.r;
			attachment.G = color.g;
			attachment.B = color.b;
		}

		public static void SetColor (this MeshAttachment attachment, Color32 color) {
			attachment.A = color.a * ByteToFloat;
			attachment.R = color.r * ByteToFloat;
			attachment.G = color.g * ByteToFloat;
			attachment.B = color.b * ByteToFloat;
		}
		#endregion

		#region Bone
		public static void SetPosition (this Bone bone, Vector2 position) {
			bone.X = position.x;
			bone.Y = position.y;
		}

		public static void SetPosition (this Bone bone, Vector3 position) {
			bone.X = position.x;
			bone.Y = position.y;
		}

		public static Vector2 GetLocalPosition (this Bone bone) {
			return new Vector2(bone.x, bone.y);
		}

		public static Vector2 GetSkeletonSpacePosition (this Bone bone) {
			return new Vector2(bone.worldX, bone.worldY);
		}

		public static Vector3 GetWorldPosition (this Bone bone, UnityEngine.Transform parentTransform) {		
			return parentTransform.TransformPoint(new Vector3(bone.worldX, bone.worldY));
		}

		public static Matrix4x4 GetMatrix4x4 (this Bone bone) {
			return new Matrix4x4 {
				m00 = bone.a, m01 = bone.b, m03 = bone.worldX,
				m10 = bone.c, m11 = bone.d, m13 = bone.worldY,
				m33 = 1
			};
		}

		public static void GetWorldToLocalMatrix (this Bone bone, out float ia, out float ib, out float ic, out float id) {
			float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			float invDet = 1 / (a * d - b * c);
			ia = invDet * d;
			ib = invDet * -b;
			ic = invDet * -c;
			id = invDet * a;
		}
		#endregion

		#region Attachments
		public static Material GetMaterial (this Attachment a) {
			var regionAttachment = a as RegionAttachment;
			if (regionAttachment != null)
				return (Material)((AtlasRegion)regionAttachment.RendererObject).page.rendererObject;

			var meshAttachment = a as MeshAttachment;
			if (meshAttachment != null)
				return (Material)((AtlasRegion)meshAttachment.RendererObject).page.rendererObject;			

			return null;
		}

		/// <summary>Fills a Vector2 buffer with local vertices.</summary>
		/// <param name="va">The VertexAttachment</param>
		/// <param name="slot">Slot where the attachment belongs.</param>
		/// <param name="buffer">Correctly-sized buffer. Use attachment's .WorldVerticesLength to get the correct size. If null, a new Vector2[] of the correct size will be allocated.</param>
		public static Vector2[] GetLocalVertices (this VertexAttachment va, Slot slot, Vector2[] buffer) {
			int floatsCount = va.worldVerticesLength;
			int bufferTargetSize = floatsCount >> 1;
			buffer = buffer ?? new Vector2[bufferTargetSize];
			if (buffer.Length < bufferTargetSize) throw new System.ArgumentException(string.Format("Vector2 buffer too small. {0} requires an array of size {1}. Use the attachment's .WorldVerticesLength to get the correct size.", va.Name, floatsCount), "buffer");

			if (va.bones == null) {
				var localVerts = va.vertices;
				for (int i = 0; i < bufferTargetSize; i++) {
					int j = i * 2;
					buffer[i] = new Vector2(localVerts[j], localVerts[j+1]);
				}
			} else {
				var floats = new float[floatsCount];
				va.ComputeWorldVertices(slot, floats);

				Bone sb = slot.bone;
				float ia, ib, ic, id, bwx = sb.worldX, bwy = sb.worldY;
				sb.GetWorldToLocalMatrix(out ia, out ib, out ic, out id);

				for (int i = 0; i < bufferTargetSize; i++) {
					int j = i * 2;
					float x = floats[j] - bwx, y = floats[j+1] - bwy;
					buffer[i] = new Vector2(x * ia + y * ib, x * ic + y * id);
				}
			}

			return buffer;
		}

		/// <summary>Calculates world vertices and fills a Vector2 buffer.</summary>
		/// <param name="a">The VertexAttachment</param>
		/// <param name="slot">Slot where the attachment belongs.</param>
		/// <param name="buffer">Correctly-sized buffer. Use attachment's .WorldVerticesLength to get the correct size. If null, a new Vector2[] of the correct size will be allocated.</param>
		public static Vector2[] GetWorldVertices (this VertexAttachment a, Slot slot, Vector2[] buffer) {
			int worldVertsLength = a.worldVerticesLength;
			int bufferTargetSize = worldVertsLength >> 1;
			buffer = buffer ?? new Vector2[bufferTargetSize];
			if (buffer.Length < bufferTargetSize) throw new System.ArgumentException(string.Format("Vector2 buffer too small. {0} requires an array of size {1}. Use the attachment's .WorldVerticesLength to get the correct size.", a.Name, worldVertsLength), "buffer");

			var floats = new float[worldVertsLength];
			a.ComputeWorldVertices(slot, floats);

			for (int i = 0, n = worldVertsLength >> 1; i < n; i++) {
				int j = i * 2;
				buffer[i] = new Vector2(floats[j], floats[j + 1]);
			}

			return buffer;
		}
		#endregion
	}
}

namespace Spine {
	public static class SkeletonExtensions {
		public static bool IsWeighted (this VertexAttachment va) {
			return va.bones != null && va.bones.Length > 0;
		}

		#region Transform Modes
		public static bool InheritsRotation (this TransformMode mode) {
			const int RotationBit = 0;
			return ((int)mode & (1U << RotationBit)) == 0;
		}

		public static bool InheritsScale (this TransformMode mode) {
			const int ScaleBit = 1;
			return ((int)mode & (1U << ScaleBit)) == 0;
		}
		#endregion

		#region Posing
		[System.Obsolete("Old Animation.Apply method signature. Please use the 8 parameter signature. See summary to learn about the extra arguments.")]
		public static void Apply (this Spine.Animation animation, Skeleton skeleton, float lastTime, float time, bool loop, ExposedList<Event> events) {
			animation.Apply(skeleton, lastTime, time, loop, events, 1f, false, false);
		}

		/// <summary>Resets the DrawOrder to the Setup Pose's draw order</summary>
		public static void SetDrawOrderToSetupPose (this Skeleton skeleton) {
			var slotsItems = skeleton.slots.Items;
			int n = skeleton.slots.Count;

			var drawOrder = skeleton.drawOrder;
			drawOrder.Clear(false);
			drawOrder.GrowIfNeeded(n);
			System.Array.Copy(slotsItems, drawOrder.Items, n);
		}

		/// <summary>Resets the color of a slot to Setup Pose value.</summary>
		public static void SetColorToSetupPose (this Slot slot) {
			slot.r = slot.data.r;
			slot.g = slot.data.g;
			slot.b = slot.data.b;
			slot.a = slot.data.a;
		}

		/// <summary>Sets a slot's attachment to setup pose. If you have the slotIndex, Skeleton.SetSlotAttachmentToSetupPose is faster.</summary>
		public static void SetAttachmentToSetupPose (this Slot slot) {
			var slotData = slot.data;
			slot.Attachment = slot.bone.skeleton.GetAttachment(slotData.name, slotData.attachmentName);
		}

		/// <summary>Resets the attachment of slot at a given slotIndex to setup pose. This is faster than Slot.SetAttachmentToSetupPose.</summary>
		public static void SetSlotAttachmentToSetupPose (this Skeleton skeleton, int slotIndex) {
			var slot = skeleton.slots.Items[slotIndex];
			var attachmentName = slot.data.attachmentName;
			if (string.IsNullOrEmpty(attachmentName)) {
				slot.Attachment = null;
			} else {
				var attachment = skeleton.GetAttachment(slotIndex, attachmentName);
				slot.Attachment = attachment;
			}
		}

		/// <summary>
		/// Shortcut for posing a skeleton at a specific time. Time is in seconds. (frameNumber / 30f) will give you seconds.
		/// If you need to do this often, you should get the Animation object yourself using skeleton.data.FindAnimation. and call Apply on that.</summary>
		/// <param name = "skeleton">The skeleton to pose.</param>
		/// <param name="animationName">The name of the animation to use.</param>
		/// <param name = "time">The time of the pose within the animation.</param>
		/// <param name = "loop">Wraps the time around if it is longer than the duration of the animation.</param>
		public static void PoseWithAnimation (this Skeleton skeleton, string animationName, float time, bool loop) {
			// Fail loud when skeleton.data is null.
			Spine.Animation animation = skeleton.data.FindAnimation(animationName);
			if (animation == null) return;
			animation.Apply(skeleton, 0, time, loop, null, 1f, false, false);
		}

		/// <summary>Resets Skeleton parts to Setup Pose according to a Spine.Animation's keyed items.</summary>
		public static void SetKeyedItemsToSetupPose (this Animation animation, Skeleton skeleton) {
			animation.Apply(skeleton, 0, 0, false, null, 0, true, true);
		}
		#endregion
	}
}
