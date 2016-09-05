/*****************************************************************************
 * Spine Extensions by Mitch Thompson and John Dy
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using Spine;

namespace Spine.Unity {
	public static class SkeletonExtensions {

		const float ByteToFloat = 1f / 255f;

		#region Colors
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
		#endregion

	}
}

namespace Spine {
	public static class SkeletonExtensions {
		#region Posing
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
			animation.Apply(skeleton, 0, time, loop, null);
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
			var attachment = skeleton.GetAttachment(slotIndex, slot.data.attachmentName);
			slot.Attachment = attachment;
		}

		/// <summary>Resets Skeleton parts to Setup Pose according to a Spine.Animation's keyed items.</summary>
		public static void SetKeyedItemsToSetupPose (this Animation animation, Skeleton skeleton) {
			var timelinesItems = animation.timelines.Items;
			for (int i = 0, n = timelinesItems.Length; i < n; i++)
				timelinesItems[i].SetToSetupPose(skeleton);
		}

		public static void SetToSetupPose (this Timeline timeline, Skeleton skeleton) {
			if (timeline != null) {
				// sorted according to assumed likelihood here

				// Bone
				if (timeline is RotateTimeline) {
					var bone = skeleton.bones.Items[((RotateTimeline)timeline).boneIndex];
					bone.rotation = bone.data.rotation;
				} else if (timeline is TranslateTimeline) {
					var bone = skeleton.bones.Items[((TranslateTimeline)timeline).boneIndex];
					bone.x = bone.data.x;
					bone.y = bone.data.y;
				} else if (timeline is ScaleTimeline) {
					var bone = skeleton.bones.Items[((ScaleTimeline)timeline).boneIndex];
					bone.scaleX = bone.data.scaleX;
					bone.scaleY = bone.data.scaleY;


				// Attachment
				} else if (timeline is DeformTimeline) {
					var slot = skeleton.slots.Items[((DeformTimeline)timeline).slotIndex];
					slot.attachmentVertices.Clear(false);

				// Slot
				} else if (timeline is AttachmentTimeline) {
					skeleton.SetSlotAttachmentToSetupPose(((AttachmentTimeline)timeline).slotIndex);

				} else if (timeline is ColorTimeline) {
					skeleton.slots.Items[((ColorTimeline)timeline).slotIndex].SetColorToSetupPose();


				// Constraint
				} else if (timeline is IkConstraintTimeline) {
					var ikTimeline = (IkConstraintTimeline)timeline;
					var ik = skeleton.ikConstraints.Items[ikTimeline.ikConstraintIndex];
					var data = ik.data;
					ik.bendDirection = data.bendDirection;
					ik.mix = data.mix;

				// Skeleton
				} else if (timeline is DrawOrderTimeline) {
					skeleton.SetDrawOrderToSetupPose();

				}

			}

		}
		#endregion
	}
}
