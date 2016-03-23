

/*****************************************************************************
 * Spine Extensions by Mitch Thompson and John Dy
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using Spine;
using Spine.Unity;

namespace Spine.Unity {
	public static class SkeletonExtensions {

		const float ByteToFloat = 1f / 255f;

		#region Colors
		public static Color GetColor (this Skeleton s) { return new Color(s.r, s.g, s.b, s.a); }
		public static Color GetColor (this RegionAttachment a) { return new Color(a.r, a.g, a.b, a.a); }
		public static Color GetColor (this MeshAttachment a) { return new Color(a.r, a.g, a.b, a.a); }
		public static Color GetColor (this WeightedMeshAttachment a) { return new Color(a.r, a.g, a.b, a.a);	}

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

		public static void SetColor (this WeightedMeshAttachment attachment, Color color) {
			attachment.A = color.a;
			attachment.R = color.r;
			attachment.G = color.g;
			attachment.B = color.b;
		}

		public static void SetColor (this WeightedMeshAttachment attachment, Color32 color) {
			attachment.A = color.a * ByteToFloat;
			attachment.R = color.r * ByteToFloat;
			attachment.G = color.g * ByteToFloat;
			attachment.B = color.b * ByteToFloat;
		}
		#endregion

		#region Bone Position
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
		#endregion

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
		#endregion
	}
}
