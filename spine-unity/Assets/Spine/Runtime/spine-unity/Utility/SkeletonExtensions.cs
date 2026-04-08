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

using UnityEngine;

namespace Spine.Unity {
	public static class SkeletonExtensions {

		#region Colors
		const float ByteToFloat = 1f / 255f;
		public static Color GetColor (this Slot s) { return s.AppliedPose.GetColor(); }
		public static Color GetColorTintBlack (this Slot s) {
			Color? darkColor = s.AppliedPose.GetDarkColor();
			if (!darkColor.HasValue) return Color.black;
			return darkColor.Value;
		}

		public static void SetColor (this Slot slot, Color color) {
			slot.AppliedPose.SetColor(color);
		}

		public static void SetColor (this Slot slot, Color32 color) {
			slot.AppliedPose.SetColor(color);
		}
		#endregion

		#region Skeleton
		/// <summary>Sets the Skeleton's local scale using a UnityEngine.Vector2. If only individual components need to be set, set Skeleton.ScaleX or Skeleton.ScaleY.</summary>
		public static void SetLocalScale (this Skeleton skeleton, Vector2 scale) {
			skeleton.ScaleX = scale.x;
			skeleton.ScaleY = scale.y;
		}

		/// <summary>Gets the internal bone matrix as a Unity bonespace-to-skeletonspace transformation matrix.</summary>
		public static Matrix4x4 GetMatrix4x4 (this Bone bone) {
			return bone.AppliedPose.GetMatrix4x4();
		}

		/// <summary>Gets the internal bone matrix as a Unity bonespace-to-skeletonspace transformation matrix.</summary>
		public static Matrix4x4 GetMatrix4x4 (this BonePose bonePose) {
			return new Matrix4x4 {
				m00 = bonePose.A,
				m01 = bonePose.B,
				m03 = bonePose.WorldX,
				m10 = bonePose.C,
				m11 = bonePose.D,
				m13 = bonePose.WorldY,
				m33 = 1
			};
		}
		#endregion

		#region Bone
		/// <summary>Sets the bone's (local) X and Y according to a Vector2</summary>
		public static void SetLocalPosition (this Bone bone, Vector2 position) {
			bone.Pose.SetLocalPosition(position);
		}

		/// <summary>Sets the bone's (local) X and Y according to a Vector2</summary>
		public static void SetLocalPosition (this BonePose bonePose, Vector2 position) {
			bonePose.X = position.x;
			bonePose.Y = position.y;
		}

		/// <summary>Sets the bone's (local) X and Y according to a Vector3. The z component is ignored.</summary>
		public static void SetLocalPosition (this Bone bone, Vector3 position) {
			bone.Pose.SetLocalPosition(position);
		}

		/// <summary>Sets the bone's (local) X and Y according to a Vector3. The z component is ignored.</summary>
		public static void SetLocalPosition (this BonePose bonePose, Vector3 position) {
			bonePose.X = position.x;
			bonePose.Y = position.y;
		}

		/// <summary>Gets the bone's local X and Y as a Vector2.</summary>
		public static Vector2 GetLocalPosition (this Bone bone) {
			return bone.Pose.GetLocalPosition();
		}

		/// <summary>Gets the bone's local X and Y as a Vector2.</summary>
		public static Vector2 GetLocalPosition (this BonePose bonePose) {
			return new Vector2(bonePose.X, bonePose.Y);
		}

		/// <summary>Gets the position of the bone in Skeleton-space.</summary>
		public static Vector2 GetSkeletonSpacePosition (this Bone bone) {
			return bone.GetSkeletonSpacePosition();
		}

		/// <summary>Gets the position of the bone in Skeleton-space.</summary>
		public static Vector2 GetSkeletonSpacePosition (this BonePose bonePose) {
			return new Vector2(bonePose.WorldX, bonePose.WorldY);
		}

		/// <summary>Gets a local offset from the bone and converts it into Skeleton-space.</summary>
		public static Vector2 GetSkeletonSpacePosition (this Bone bone, Vector2 boneLocal) {
			Vector2 o;
			bone.AppliedPose.LocalToWorld(boneLocal.x, boneLocal.y, out o.x, out o.y);
			return o;
		}

		/// <summary>Gets the bone's Unity World position using its Spine GameObject Transform.
		/// UpdateWorldTransform needs to have been called for this to return the correct, updated value.</summary>
		public static Vector3 GetWorldPosition (this Bone bone, UnityEngine.Transform spineGameObjectTransform) {
			return GetWorldPosition(bone.AppliedPose, spineGameObjectTransform);
		}

		public static Vector3 GetWorldPosition (this Bone bone, UnityEngine.Transform spineGameObjectTransform, float positionScale) {
			return GetWorldPosition(bone.AppliedPose, spineGameObjectTransform, positionScale);
		}

		public static Vector3 GetWorldPosition (this Bone bone, UnityEngine.Transform spineGameObjectTransform, float positionScale, Vector2 positionOffset) {
			return GetWorldPosition(bone.AppliedPose, spineGameObjectTransform, positionScale, positionOffset);
		}

		/// <summary>Gets the bone's Unity World position using its Spine GameObject Transform.
		/// UpdateWorldTransform needs to have been called for this to return the correct, updated value.</summary>
		public static Vector3 GetWorldPosition (this BonePose bonePose, UnityEngine.Transform spineGameObjectTransform) {
			return spineGameObjectTransform.TransformPoint(new Vector3(
				bonePose.WorldX, bonePose.WorldY));
		}

		public static Vector3 GetWorldPosition (this BonePose bonePose, UnityEngine.Transform spineGameObjectTransform, float positionScale) {
			return spineGameObjectTransform.TransformPoint(new Vector3(
				bonePose.WorldX * positionScale, bonePose.WorldY * positionScale));
		}

		public static Vector3 GetWorldPosition (this BonePose bonePose, UnityEngine.Transform spineGameObjectTransform, float positionScale, Vector2 positionOffset) {
			return spineGameObjectTransform.TransformPoint(new Vector3(
				bonePose.WorldX * positionScale + positionOffset.x, bonePose.WorldY * positionScale + positionOffset.y));
		}

		/// <summary>Gets a skeleton space UnityEngine.Quaternion representation of bone.WorldRotationX.</summary>
		public static Quaternion GetQuaternion (this Bone bone) {
			return bone.AppliedPose.GetQuaternion();
		}

		/// <summary>Gets a skeleton space UnityEngine.Quaternion representation of bone.WorldRotationX.</summary>
		public static Quaternion GetQuaternion (this BonePose bonePose) {
			float halfRotation = Mathf.Atan2(bonePose.C, bonePose.A) * 0.5f;
			return new Quaternion(0, 0, Mathf.Sin(halfRotation), Mathf.Cos(halfRotation));
		}

		/// <summary>Gets a bone-local space UnityEngine.Quaternion representation of bone.rotation.</summary>
		public static Quaternion GetLocalQuaternion (this Bone bone) {
			return bone.Pose.GetLocalQuaternion();
		}

		/// <summary>Gets a bone-local space UnityEngine.Quaternion representation of bone.rotation.</summary>
		public static Quaternion GetLocalQuaternion (this BonePose bonePose) {
			float halfRotation = bonePose.Rotation * Mathf.Deg2Rad * 0.5f;
			return new Quaternion(0, 0, Mathf.Sin(halfRotation), Mathf.Cos(halfRotation));
		}

		/// <summary>Returns the Skeleton's local scale as a UnityEngine.Vector2. If only individual components are needed, use Skeleton.ScaleX or Skeleton.ScaleY.</summary>
		public static Vector2 GetLocalScale (this Skeleton skeleton) {
			return new Vector2(skeleton.ScaleX, skeleton.ScaleY);
		}

		/// <summary>Calculates a 2x2 Transformation Matrix that can convert a skeleton-space position to a bone-local position.</summary>
		public static void GetWorldToLocalMatrix (this Bone bone, out float ia, out float ib, out float ic, out float id) {
			bone.AppliedPose.GetWorldToLocalMatrix(out ia, out ib, out ic, out id);
		}

		/// <summary>Calculates a 2x2 Transformation Matrix that can convert a skeleton-space position to a bone-local position.</summary>
		public static void GetWorldToLocalMatrix (this BonePose bonePose, out float ia, out float ib, out float ic, out float id) {
			float a = bonePose.A, b = bonePose.B, c = bonePose.C, d = bonePose.D;
			float invDet = 1 / (a * d - b * c);
			ia = invDet * d;
			ib = invDet * -b;
			ic = invDet * -c;
			id = invDet * a;
		}

		/// <summary>UnityEngine.Vector2 override of Bone.WorldToLocal. This converts a skeleton-space position into a bone local position.</summary>
		public static Vector2 WorldToLocal (this Bone bone, Vector2 worldPosition) {
			return bone.AppliedPose.WorldToLocal(worldPosition);
		}

		/// <summary>UnityEngine.Vector2 override of Bone.WorldToLocal. This converts a skeleton-space position into a bone local position.</summary>
		public static Vector2 WorldToLocal (this BonePose bonePose, Vector2 worldPosition) {
			Vector2 o;
			bonePose.WorldToLocal(worldPosition.x, worldPosition.y, out o.x, out o.y);
			return o;
		}

		/// <summary>Sets the skeleton-space position of a bone.</summary>
		/// <returns>The local position in its parent bone space, or in skeleton space if it is the root bone.</returns>
		public static Vector2 SetPositionSkeletonSpace (this Bone bone, Vector2 skeletonSpacePosition) {
			if (bone.Parent == null) { // root bone
				bone.SetLocalPosition(skeletonSpacePosition);
				return skeletonSpacePosition;
			} else {
				Bone parent = bone.Parent;
				Vector2 parentLocal = parent.WorldToLocal(skeletonSpacePosition);
				bone.SetLocalPosition(parentLocal);
				return parentLocal;
			}
		}
		#endregion

		#region Attachments
		public static Material GetMaterial (this Attachment a) {
			object rendererObject = null;
			IHasSequence renderableAttachment = a as IHasSequence;
			if (renderableAttachment != null) {
				rendererObject = renderableAttachment.Sequence.Regions[0];
			}

			if (rendererObject == null)
				return null;

			return (Material)((AtlasRegion)rendererObject).page.rendererObject;
		}

		/// <summary>Fills a Vector2 buffer with local vertices.</summary>
		/// <param name="va">The VertexAttachment</param>
		/// <param name="slot">Slot where the attachment belongs.</param>
		/// <param name="buffer">Correctly-sized buffer. Use attachment's .WorldVerticesLength to get the correct size.
		/// If null, a new Vector2[] of the correct size will be allocated.</param>
		public static Vector2[] GetLocalVertices (this VertexAttachment va, Skeleton skeleton, Slot slot, Vector2[] buffer) {
			int floatsCount = va.WorldVerticesLength;
			int bufferTargetSize = floatsCount >> 1;
			buffer = buffer ?? new Vector2[bufferTargetSize];
			if (buffer.Length < bufferTargetSize) throw new System.ArgumentException(
				string.Format("Vector2 buffer too small. {0} requires an array of size {1}. " +
				"Use the attachment's .WorldVerticesLength to get the correct size.", va.Name, floatsCount), "buffer");

			if (va.Bones == null && slot.Pose.Deform.Count == 0) {
				float[] localVerts = va.Vertices;
				for (int i = 0; i < bufferTargetSize; i++) {
					int j = i * 2;
					buffer[i] = new Vector2(localVerts[j], localVerts[j + 1]);
				}
			} else {
				float[] floats = new float[floatsCount];
				va.ComputeWorldVertices(skeleton, slot, floats);

				Bone sb = slot.Bone;
				BonePose pose = slot.Bone.AppliedPose;
				float ia, ib, ic, id, bwx = pose.WorldX, bwy = pose.WorldY;
				sb.GetWorldToLocalMatrix(out ia, out ib, out ic, out id);
				for (int i = 0; i < bufferTargetSize; i++) {
					int j = i * 2;
					float x = floats[j] - bwx, y = floats[j + 1] - bwy;
					buffer[i] = new Vector2(x * ia + y * ib, x * ic + y * id);
				}
			}

			return buffer;
		}

		/// <summary>Calculates world vertices and fills a Vector2 buffer.</summary>
		/// <param name="a">The VertexAttachment</param>
		/// <param name="slot">Slot where the attachment belongs.</param>
		/// <param name="buffer">Correctly-sized buffer. Use attachment's .WorldVerticesLength to get the correct size. If null, a new Vector2[] of the correct size will be allocated.</param>
		public static Vector2[] GetWorldVertices (this VertexAttachment a, Skeleton skeleton, Slot slot, Vector2[] buffer) {
			int worldVertsLength = a.WorldVerticesLength;
			int bufferTargetSize = worldVertsLength >> 1;
			buffer = buffer ?? new Vector2[bufferTargetSize];
			if (buffer.Length < bufferTargetSize) throw new System.ArgumentException(string.Format("Vector2 buffer too small. {0} requires an array of size {1}. Use the attachment's .WorldVerticesLength to get the correct size.", a.Name, worldVertsLength), "buffer");

			float[] floats = new float[worldVertsLength];
			a.ComputeWorldVertices(skeleton, slot, floats);

			for (int i = 0, n = worldVertsLength >> 1; i < n; i++) {
				int j = i * 2;
				buffer[i] = new Vector2(floats[j], floats[j + 1]);
			}

			return buffer;
		}

		/// <summary>Gets the PointAttachment's Unity World position using its Spine GameObject Transform.</summary>
		public static Vector3 GetWorldPosition (this PointAttachment attachment, Slot slot, Transform spineGameObjectTransform) {
			Vector3 skeletonSpacePosition;
			skeletonSpacePosition.z = 0;
			attachment.ComputeWorldPosition(slot.Bone.AppliedPose, out skeletonSpacePosition.x, out skeletonSpacePosition.y);
			return spineGameObjectTransform.TransformPoint(skeletonSpacePosition);
		}

		/// <summary>Gets the PointAttachment's Unity World position using its Spine GameObject Transform.</summary>
		public static Vector3 GetWorldPosition (this PointAttachment attachment, Bone bone, Transform spineGameObjectTransform) {
			Vector3 skeletonSpacePosition;
			skeletonSpacePosition.z = 0;
			attachment.ComputeWorldPosition(bone.AppliedPose, out skeletonSpacePosition.x, out skeletonSpacePosition.y);
			return spineGameObjectTransform.TransformPoint(skeletonSpacePosition);
		}
		#endregion
	}
}

namespace Spine {
	using System;

	public struct BoneMatrix {
		public float a, b, c, d, x, y;

		/// <summary>Recursively calculates a worldspace bone matrix based on BoneData.</summary>
		public static BoneMatrix CalculateSetupWorld (BoneData boneData) {
			if (boneData == null)
				return default(BoneMatrix);

			// End condition: isRootBone
			if (boneData.Parent == null)
				return GetInheritedInternal(boneData, default(BoneMatrix));

			BoneMatrix result = CalculateSetupWorld(boneData.Parent);
			return GetInheritedInternal(boneData, result);
		}

		static BoneMatrix GetInheritedInternal (BoneData boneData, BoneMatrix parentMatrix) {
			BoneData parent = boneData.Parent;
			if (parent == null) return new BoneMatrix(boneData); // isRootBone

			float pa = parentMatrix.a, pb = parentMatrix.b, pc = parentMatrix.c, pd = parentMatrix.d;
			BoneMatrix result = default(BoneMatrix);
			var setup = boneData.GetSetupPose();
			result.x = pa * setup.X + pb * setup.Y + parentMatrix.x;
			result.y = pc * setup.X + pd * setup.Y + parentMatrix.y;

			switch (setup.Inherit) {
			case Inherit.Normal: {
				float rotationY = setup.Rotation + 90 + setup.ShearY;
				float la = MathUtils.CosDeg(setup.Rotation + setup.ShearX) * setup.ScaleX;
				float lb = MathUtils.CosDeg(rotationY) * setup.ScaleY;
				float lc = MathUtils.SinDeg(setup.Rotation + setup.ShearX) * setup.ScaleX;
				float ld = MathUtils.SinDeg(rotationY) * setup.ScaleY;
				result.a = pa * la + pb * lc;
				result.b = pa * lb + pb * ld;
				result.c = pc * la + pd * lc;
				result.d = pc * lb + pd * ld;
				break;
			}
			case Inherit.OnlyTranslation: {
				float rotationY = setup.Rotation + 90 + setup.ShearY;
				result.a = MathUtils.CosDeg(setup.Rotation + setup.ShearX) * setup.ScaleX;
				result.b = MathUtils.CosDeg(rotationY) * setup.ScaleY;
				result.c = MathUtils.SinDeg(setup.Rotation + setup.ShearX) * setup.ScaleX;
				result.d = MathUtils.SinDeg(rotationY) * setup.ScaleY;
				break;
			}
			case Inherit.NoRotationOrReflection: {
				float s = pa * pa + pc * pc, prx;
				if (s > 0.0001f) {
					s = Math.Abs(pa * pd - pb * pc) / s;
					pb = pc * s;
					pd = pa * s;
					prx = MathUtils.Atan2(pc, pa) * MathUtils.RadDeg;
				} else {
					pa = 0;
					pc = 0;
					prx = 90 - MathUtils.Atan2(pd, pb) * MathUtils.RadDeg;
				}
				float rx = setup.Rotation + setup.ShearX - prx;
				float ry = setup.Rotation + setup.ShearY - prx + 90;
				float la = MathUtils.CosDeg(rx) * setup.ScaleX;
				float lb = MathUtils.CosDeg(ry) * setup.ScaleY;
				float lc = MathUtils.SinDeg(rx) * setup.ScaleX;
				float ld = MathUtils.SinDeg(ry) * setup.ScaleY;
				result.a = pa * la - pb * lc;
				result.b = pa * lb - pb * ld;
				result.c = pc * la + pd * lc;
				result.d = pc * lb + pd * ld;
				break;
			}
			case Inherit.NoScale:
			case Inherit.NoScaleOrReflection: {
				float cos = MathUtils.CosDeg(setup.Rotation), sin = MathUtils.SinDeg(setup.Rotation);
				float za = pa * cos + pb * sin;
				float zc = pc * cos + pd * sin;
				float s = (float)Math.Sqrt(za * za + zc * zc);
				if (s > 0.00001f)
					s = 1 / s;
				za *= s;
				zc *= s;
				s = (float)Math.Sqrt(za * za + zc * zc);
				float r = MathUtils.PI / 2 + MathUtils.Atan2(zc, za);
				float zb = MathUtils.Cos(r) * s;
				float zd = MathUtils.Sin(r) * s;
				float la = MathUtils.CosDeg(setup.ShearX) * setup.ScaleX;
				float lb = MathUtils.CosDeg(90 + setup.ShearY) * setup.ScaleY;
				float lc = MathUtils.SinDeg(setup.ShearX) * setup.ScaleX;
				float ld = MathUtils.SinDeg(90 + setup.ShearY) * setup.ScaleY;
				if (setup.Inherit != Inherit.NoScaleOrReflection ? pa * pd - pb * pc < 0 : false) {
					zb = -zb;
					zd = -zd;
				}
				result.a = za * la + zb * lc;
				result.b = za * lb + zb * ld;
				result.c = zc * la + zd * lc;
				result.d = zc * lb + zd * ld;
				break;
			}
			}

			return result;
		}

		/// <summary>Constructor for a local bone matrix based on Setup Pose BoneData.</summary>
		public BoneMatrix (BoneData boneData) {
			var setup = boneData.GetSetupPose();
			float rotationY = setup.Rotation + 90 + setup.ShearY;
			float rotationX = setup.Rotation + setup.ShearX;

			a = MathUtils.CosDeg(rotationX) * setup.ScaleX;
			c = MathUtils.SinDeg(rotationX) * setup.ScaleX;
			b = MathUtils.CosDeg(rotationY) * setup.ScaleY;
			d = MathUtils.SinDeg(rotationY) * setup.ScaleY;
			x = setup.X;
			y = setup.Y;
		}

		/// <summary>Constructor for a local bone matrix based on a bone instance's current pose.</summary>
		public BoneMatrix (Bone bone) {
			var bonePose = bone.Pose;
			float rotationY = bonePose.Rotation + 90 + bonePose.ShearY;
			float rotationX = bonePose.Rotation + bonePose.ShearX;

			a = MathUtils.CosDeg(rotationX) * bonePose.ScaleX;
			c = MathUtils.SinDeg(rotationX) * bonePose.ScaleX;
			b = MathUtils.CosDeg(rotationY) * bonePose.ScaleY;
			d = MathUtils.SinDeg(rotationY) * bonePose.ScaleY;
			x = bonePose.X;
			y = bonePose.Y;
		}

		public BoneMatrix TransformMatrix (BoneMatrix local) {
			return new BoneMatrix {
				a = this.a * local.a + this.b * local.c,
				b = this.a * local.b + this.b * local.d,
				c = this.c * local.a + this.d * local.c,
				d = this.c * local.b + this.d * local.d,
				x = this.a * local.x + this.b * local.y + this.x,
				y = this.c * local.x + this.d * local.y + this.y
			};
		}
	}

	public static class SpineSkeletonExtensions {
		public static bool IsWeighted (this VertexAttachment va) {
			return va.Bones != null && va.Bones.Length > 0;
		}

		#region Inherit Modes
		public static bool InheritsRotation (this Inherit mode) {
			return mode == Inherit.Normal || mode == Inherit.NoScale || mode == Inherit.NoScaleOrReflection;
		}

		public static bool InheritsScale (this Inherit mode) {
			return mode == Inherit.Normal || mode == Inherit.NoRotationOrReflection;
		}
		#endregion
	}
}
