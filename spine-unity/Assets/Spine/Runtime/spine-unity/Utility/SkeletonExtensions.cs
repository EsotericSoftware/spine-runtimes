/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;

namespace Spine.Unity {
	public static class SkeletonExtensions {

		#region Colors
		const float ByteToFloat = 1f / 255f;
		public static Color GetColor (this Skeleton s) { return new Color(s.r, s.g, s.b, s.a); }
		public static Color GetColor (this RegionAttachment a) { return new Color(a.r, a.g, a.b, a.a); }
		public static Color GetColor (this MeshAttachment a) { return new Color(a.r, a.g, a.b, a.a); }
		public static Color GetColor (this Slot s) { return new Color(s.r, s.g, s.b, s.a); }
		public static Color GetColorTintBlack (this Slot s) { return new Color(s.r2, s.g2, s.b2, 1f); }

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

		#region Skeleton
		/// <summary>Sets the Skeleton's local scale using a UnityEngine.Vector2. If only individual components need to be set, set Skeleton.ScaleX or Skeleton.ScaleY.</summary>
		public static void SetLocalScale (this Skeleton skeleton, Vector2 scale) {
			skeleton.ScaleX = scale.x;
			skeleton.ScaleY = scale.y;
		}

		/// <summary>Gets the internal bone matrix as a Unity bonespace-to-skeletonspace transformation matrix.</summary>
		public static Matrix4x4 GetMatrix4x4 (this Bone bone) {
			return new Matrix4x4 {
				m00 = bone.a,
				m01 = bone.b,
				m03 = bone.worldX,
				m10 = bone.c,
				m11 = bone.d,
				m13 = bone.worldY,
				m33 = 1
			};
		}
		#endregion

		#region Bone
		/// <summary>Sets the bone's (local) X and Y according to a Vector2</summary>
		public static void SetLocalPosition (this Bone bone, Vector2 position) {
			bone.X = position.x;
			bone.Y = position.y;
		}

		/// <summary>Sets the bone's (local) X and Y according to a Vector3. The z component is ignored.</summary>
		public static void SetLocalPosition (this Bone bone, Vector3 position) {
			bone.X = position.x;
			bone.Y = position.y;
		}

		/// <summary>Gets the bone's local X and Y as a Vector2.</summary>
		public static Vector2 GetLocalPosition (this Bone bone) {
			return new Vector2(bone.x, bone.y);
		}

		/// <summary>Gets the position of the bone in Skeleton-space.</summary>
		public static Vector2 GetSkeletonSpacePosition (this Bone bone) {
			return new Vector2(bone.worldX, bone.worldY);
		}

		/// <summary>Gets a local offset from the bone and converts it into Skeleton-space.</summary>
		public static Vector2 GetSkeletonSpacePosition (this Bone bone, Vector2 boneLocal) {
			Vector2 o;
			bone.LocalToWorld(boneLocal.x, boneLocal.y, out o.x, out o.y);
			return o;
		}

		/// <summary>Gets the bone's Unity World position using its Spine GameObject Transform. UpdateWorldTransform needs to have been called for this to return the correct, updated value.</summary>
		public static Vector3 GetWorldPosition (this Bone bone, UnityEngine.Transform spineGameObjectTransform) {
			return spineGameObjectTransform.TransformPoint(new Vector3(bone.worldX, bone.worldY));
		}

		public static Vector3 GetWorldPosition (this Bone bone, UnityEngine.Transform spineGameObjectTransform, float positionScale) {
			return spineGameObjectTransform.TransformPoint(new Vector3(bone.worldX * positionScale, bone.worldY * positionScale));
		}

		/// <summary>Gets a skeleton space UnityEngine.Quaternion representation of bone.WorldRotationX.</summary>
		public static Quaternion GetQuaternion (this Bone bone) {
			var halfRotation = Mathf.Atan2(bone.c, bone.a) * 0.5f;
			return new Quaternion(0, 0, Mathf.Sin(halfRotation), Mathf.Cos(halfRotation));
		}

		/// <summary>Gets a bone-local space UnityEngine.Quaternion representation of bone.rotation.</summary>
		public static Quaternion GetLocalQuaternion (this Bone bone) {
			var halfRotation = bone.rotation * Mathf.Deg2Rad * 0.5f;
			return new Quaternion(0, 0, Mathf.Sin(halfRotation), Mathf.Cos(halfRotation));
		}

		/// <summary>Returns the Skeleton's local scale as a UnityEngine.Vector2. If only individual components are needed, use Skeleton.ScaleX or Skeleton.ScaleY.</summary>
		public static Vector2 GetLocalScale (this Skeleton skeleton) {
			return new Vector2(skeleton.ScaleX, skeleton.ScaleY);
		}

		/// <summary>Calculates a 2x2 Transformation Matrix that can convert a skeleton-space position to a bone-local position.</summary>
		public static void GetWorldToLocalMatrix (this Bone bone, out float ia, out float ib, out float ic, out float id) {
			float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			float invDet = 1 / (a * d - b * c);
			ia = invDet * d;
			ib = invDet * -b;
			ic = invDet * -c;
			id = invDet * a;
		}

		/// <summary>UnityEngine.Vector2 override of Bone.WorldToLocal. This converts a skeleton-space position into a bone local position.</summary>
		public static Vector2 WorldToLocal (this Bone bone, Vector2 worldPosition) {
			Vector2 o;
			bone.WorldToLocal(worldPosition.x, worldPosition.y, out o.x, out o.y);
			return o;
		}

		/// <summary>Sets the skeleton-space position of a bone.</summary>
		/// <returns>The local position in its parent bone space, or in skeleton space if it is the root bone.</returns>
		public static Vector2 SetPositionSkeletonSpace (this Bone bone, Vector2 skeletonSpacePosition) {
			if (bone.parent == null) { // root bone
				bone.SetLocalPosition(skeletonSpacePosition);
				return skeletonSpacePosition;
			} else {
				var parent = bone.parent;
				Vector2 parentLocal = parent.WorldToLocal(skeletonSpacePosition);
				bone.SetLocalPosition(parentLocal);
				return parentLocal;
			}
		}
		#endregion

		#region Attachments
		public static Material GetMaterial (this Attachment a) {
			object rendererObject = null;
			var renderableAttachment = a as IHasRendererObject;
			if (renderableAttachment != null)
				rendererObject = renderableAttachment.RendererObject;

			if (rendererObject == null)
				return null;

			#if SPINE_TK2D
			return (rendererObject.GetType() == typeof(Material)) ? (Material)rendererObject : (Material)((AtlasRegion)rendererObject).page.rendererObject;
			#else
			return (Material)((AtlasRegion)rendererObject).page.rendererObject;
			#endif
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

		/// <summary>Gets the PointAttachment's Unity World position using its Spine GameObject Transform.</summary>
		public static Vector3 GetWorldPosition (this PointAttachment attachment, Slot slot, Transform spineGameObjectTransform) {
			Vector3 skeletonSpacePosition;
			skeletonSpacePosition.z = 0;
			attachment.ComputeWorldPosition(slot.bone, out skeletonSpacePosition.x, out skeletonSpacePosition.y);
			return spineGameObjectTransform.TransformPoint(skeletonSpacePosition);
		}

		/// <summary>Gets the PointAttachment's Unity World position using its Spine GameObject Transform.</summary>
		public static Vector3 GetWorldPosition (this PointAttachment attachment, Bone bone, Transform spineGameObjectTransform) {
			Vector3 skeletonSpacePosition;
			skeletonSpacePosition.z = 0;
			attachment.ComputeWorldPosition(bone, out skeletonSpacePosition.x, out skeletonSpacePosition.y);
			return spineGameObjectTransform.TransformPoint(skeletonSpacePosition);
		}
		#endregion
	}
}

namespace Spine {
	using System;
	using System.Collections.Generic;

	public struct BoneMatrix {
		public float a, b, c, d, x, y;

		/// <summary>Recursively calculates a worldspace bone matrix based on BoneData.</summary>
		public static BoneMatrix CalculateSetupWorld (BoneData boneData) {
			if (boneData == null)
				return default(BoneMatrix);

			// End condition: isRootBone
			if (boneData.parent == null)
				return GetInheritedInternal(boneData, default(BoneMatrix));

			BoneMatrix result = CalculateSetupWorld(boneData.parent);
			return GetInheritedInternal(boneData, result);
		}

		static BoneMatrix GetInheritedInternal (BoneData boneData, BoneMatrix parentMatrix) {
			var parent = boneData.parent;
			if (parent == null) return new BoneMatrix(boneData); // isRootBone

			float pa = parentMatrix.a, pb = parentMatrix.b, pc = parentMatrix.c, pd = parentMatrix.d;
			BoneMatrix result = default(BoneMatrix);
			result.x = pa * boneData.x + pb * boneData.y + parentMatrix.x;
			result.y = pc * boneData.x + pd * boneData.y + parentMatrix.y;

			switch (boneData.transformMode) {
				case TransformMode.Normal: {
					float rotationY = boneData.rotation + 90 + boneData.shearY;
					float la = MathUtils.CosDeg(boneData.rotation + boneData.shearX) * boneData.scaleX;
					float lb = MathUtils.CosDeg(rotationY) * boneData.scaleY;
					float lc = MathUtils.SinDeg(boneData.rotation + boneData.shearX) * boneData.scaleX;
					float ld = MathUtils.SinDeg(rotationY) * boneData.scaleY;
					result.a = pa * la + pb * lc;
					result.b = pa * lb + pb * ld;
					result.c = pc * la + pd * lc;
					result.d = pc * lb + pd * ld;
					break;
				}
				case TransformMode.OnlyTranslation: {
					float rotationY = boneData.rotation + 90 + boneData.shearY;
					result.a = MathUtils.CosDeg(boneData.rotation + boneData.shearX) * boneData.scaleX;
					result.b = MathUtils.CosDeg(rotationY) * boneData.scaleY;
					result.c = MathUtils.SinDeg(boneData.rotation + boneData.shearX) * boneData.scaleX;
					result.d = MathUtils.SinDeg(rotationY) * boneData.scaleY;
					break;
				}
				case TransformMode.NoRotationOrReflection: {
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
					float rx = boneData.rotation + boneData.shearX - prx;
					float ry = boneData.rotation + boneData.shearY - prx + 90;
					float la = MathUtils.CosDeg(rx) * boneData.scaleX;
					float lb = MathUtils.CosDeg(ry) * boneData.scaleY;
					float lc = MathUtils.SinDeg(rx) * boneData.scaleX;
					float ld = MathUtils.SinDeg(ry) * boneData.scaleY;
					result.a = pa * la - pb * lc;
					result.b = pa * lb - pb * ld;
					result.c = pc * la + pd * lc;
					result.d = pc * lb + pd * ld;
					break;
				}
				case TransformMode.NoScale:
				case TransformMode.NoScaleOrReflection: {
					float cos = MathUtils.CosDeg(boneData.rotation), sin = MathUtils.SinDeg(boneData.rotation);
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
					float la = MathUtils.CosDeg(boneData.shearX) * boneData.scaleX;
					float lb = MathUtils.CosDeg(90 + boneData.shearY) * boneData.scaleY;
					float lc = MathUtils.SinDeg(boneData.shearX) * boneData.scaleX;
					float ld = MathUtils.SinDeg(90 + boneData.shearY) * boneData.scaleY;
					if (boneData.transformMode != TransformMode.NoScaleOrReflection ? pa * pd - pb * pc < 0 : false) {
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
			float rotationY = boneData.rotation + 90 + boneData.shearY;
			float rotationX = boneData.rotation + boneData.shearX;

			a = MathUtils.CosDeg(rotationX) * boneData.scaleX;
			c = MathUtils.SinDeg(rotationX) * boneData.scaleX;
			b = MathUtils.CosDeg(rotationY) * boneData.scaleY;
			d = MathUtils.SinDeg(rotationY) * boneData.scaleY;
			x = boneData.x;
			y = boneData.y;
		}

		/// <summary>Constructor for a local bone matrix based on a bone instance's current pose.</summary>
		public BoneMatrix (Bone bone) {
			float rotationY = bone.rotation + 90 + bone.shearY;
			float rotationX = bone.rotation + bone.shearX;

			a = MathUtils.CosDeg(rotationX) * bone.scaleX;
			c = MathUtils.SinDeg(rotationX) * bone.scaleX;
			b = MathUtils.CosDeg(rotationY) * bone.scaleY;
			d = MathUtils.SinDeg(rotationY) * bone.scaleY;
			x = bone.x;
			y = bone.y;
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

	public static class SkeletonExtensions {
		public static bool IsWeighted (this VertexAttachment va) {
			return va.bones != null && va.bones.Length > 0;
		}

		public static bool IsRenderable (this Attachment a) {
			return a is IHasRendererObject;
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
		internal static void SetPropertyToSetupPose (this Skeleton skeleton, int propertyID) {
			int tt = propertyID >> 24;
			var timelineType = (TimelineType)tt;
			int i = propertyID - (tt << 24);

			Bone bone;
			IkConstraint ikc;
			PathConstraint pc;

			switch (timelineType) {
			// Bone
			case TimelineType.Rotate:
				bone = skeleton.bones.Items[i];
				bone.rotation = bone.data.rotation;
				break;
			case TimelineType.Translate:
				bone = skeleton.bones.Items[i];
				bone.x = bone.data.x;
				bone.y = bone.data.y;
				break;
			case TimelineType.Scale:
				bone = skeleton.bones.Items[i];
				bone.scaleX = bone.data.scaleX;
				bone.scaleY = bone.data.scaleY;
				break;
			case TimelineType.Shear:
				bone = skeleton.bones.Items[i];
				bone.shearX = bone.data.shearX;
				bone.shearY = bone.data.shearY;
				break;

			// Slot
			case TimelineType.Attachment:
				skeleton.SetSlotAttachmentToSetupPose(i);
				break;
			case TimelineType.Color:
				skeleton.slots.Items[i].SetColorToSetupPose();
				break;
			case TimelineType.TwoColor:
				skeleton.slots.Items[i].SetColorToSetupPose();
				break;
			case TimelineType.Deform:
				skeleton.slots.Items[i].Deform.Clear();
				break;

			// Skeleton
			case TimelineType.DrawOrder:
				skeleton.SetDrawOrderToSetupPose();
				break;

			// IK Constraint
			case TimelineType.IkConstraint:
				ikc = skeleton.ikConstraints.Items[i];
				ikc.mix = ikc.data.mix;
				ikc.softness = ikc.data.softness;
				ikc.bendDirection = ikc.data.bendDirection;
				ikc.stretch = ikc.data.stretch;
				break;

			// TransformConstraint
			case TimelineType.TransformConstraint:
				var tc = skeleton.transformConstraints.Items[i];
				var tcData = tc.data;
				tc.rotateMix = tcData.rotateMix;
				tc.translateMix = tcData.translateMix;
				tc.scaleMix = tcData.scaleMix;
				tc.shearMix = tcData.shearMix;
				break;

			// Path Constraint
			case TimelineType.PathConstraintPosition:
				pc = skeleton.pathConstraints.Items[i];
				pc.position = pc.data.position;
				break;
			case TimelineType.PathConstraintSpacing:
				pc = skeleton.pathConstraints.Items[i];
				pc.spacing = pc.data.spacing;
				break;
			case TimelineType.PathConstraintMix:
				pc = skeleton.pathConstraints.Items[i];
				pc.rotateMix = pc.data.rotateMix;
				pc.translateMix = pc.data.translateMix;
				break;
			}
		}

		/// <summary>Resets the DrawOrder to the Setup Pose's draw order</summary>
		public static void SetDrawOrderToSetupPose (this Skeleton skeleton) {
			var slotsItems = skeleton.slots.Items;
			int n = skeleton.slots.Count;

			var drawOrder = skeleton.drawOrder;
			drawOrder.Clear(false);
			drawOrder.EnsureCapacity(n);
			drawOrder.Count = n;
			System.Array.Copy(slotsItems, drawOrder.Items, n);
		}

		/// <summary>Resets all the slots on the skeleton to their Setup Pose attachments but does not reset slot colors.</summary>
		public static void SetSlotAttachmentsToSetupPose (this Skeleton skeleton) {
			var slotsItems = skeleton.slots.Items;
			for (int i = 0; i < skeleton.slots.Count; i++) {
				Slot slot = slotsItems[i];
				string attachmentName = slot.data.attachmentName;
				slot.Attachment = string.IsNullOrEmpty(attachmentName) ? null : skeleton.GetAttachment(i, attachmentName);
			}
		}

		/// <summary>Resets the color of a slot to Setup Pose value.</summary>
		public static void SetColorToSetupPose (this Slot slot) {
			slot.r = slot.data.r;
			slot.g = slot.data.g;
			slot.b = slot.data.b;
			slot.a = slot.data.a;
			slot.r2 = slot.data.r2;
			slot.g2 = slot.data.g2;
			slot.b2 = slot.data.b2;
		}

		/// <summary>Sets a slot's attachment to setup pose. If you have the slotIndex, Skeleton.SetSlotAttachmentToSetupPose is faster.</summary>
		public static void SetAttachmentToSetupPose (this Slot slot) {
			var slotData = slot.data;
			slot.Attachment = slot.bone.skeleton.GetAttachment(slotData.name, slotData.attachmentName);
		}

		/// <summary>Resets the attachment of slot at a given slotIndex to setup pose. This is faster than Slot.SetAttachmentToSetupPose.</summary>
		public static void SetSlotAttachmentToSetupPose (this Skeleton skeleton, int slotIndex) {
			var slot = skeleton.slots.Items[slotIndex];
			string attachmentName = slot.data.attachmentName;
			if (string.IsNullOrEmpty(attachmentName)) {
				slot.Attachment = null;
			} else {
				var attachment = skeleton.GetAttachment(slotIndex, attachmentName);
				slot.Attachment = attachment;
			}
		}

		/// <summary>Resets Skeleton parts to Setup Pose according to a Spine.Animation's keyed items.</summary>
		public static void SetKeyedItemsToSetupPose (this Animation animation, Skeleton skeleton) {
			animation.Apply(skeleton, 0, 0, false, null, 0, MixBlend.Setup, MixDirection.Out);
		}

		public static void AllowImmediateQueue (this TrackEntry trackEntry) {
			if (trackEntry.nextTrackLast < 0) trackEntry.nextTrackLast = 0;
		}
		#endregion
	}
}
