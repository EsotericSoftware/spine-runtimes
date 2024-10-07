/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	public class Skeleton {
		static private readonly int[] quadTriangles = { 0, 1, 2, 2, 3, 0 };
		internal SkeletonData data;
		internal ExposedList<Bone> bones;
		internal ExposedList<Slot> slots;
		internal ExposedList<Slot> drawOrder;
		internal ExposedList<IkConstraint> ikConstraints;
		internal ExposedList<TransformConstraint> transformConstraints;
		internal ExposedList<PathConstraint> pathConstraints;
		internal ExposedList<PhysicsConstraint> physicsConstraints;
		internal ExposedList<IUpdatable> updateCache = new ExposedList<IUpdatable>();
		internal Skin skin;
		internal float r = 1, g = 1, b = 1, a = 1;
		internal float x, y, scaleX = 1, time;
		/// <summary>Private to enforce usage of ScaleY getter taking Bone.yDown into account.</summary>
		private float scaleY = 1;

		/// <summary>The skeleton's setup pose data.</summary>
		public SkeletonData Data { get { return data; } }
		/// <summary>The skeleton's bones, sorted parent first. The root bone is always the first bone.</summary>
		public ExposedList<Bone> Bones { get { return bones; } }
		/// <summary>The list of bones and constraints, sorted in the order they should be updated,
		/// as computed by <see cref="UpdateCache()"/>.</summary>
		public ExposedList<IUpdatable> UpdateCacheList { get { return updateCache; } }
		/// <summary>The skeleton's slots.</summary>
		public ExposedList<Slot> Slots { get { return slots; } }
		/// <summary>The skeleton's slots in the order they should be drawn.
		/// The returned array may be modified to change the draw order.</summary>
		public ExposedList<Slot> DrawOrder { get { return drawOrder; } }
		/// <summary>The skeleton's IK constraints.</summary>
		public ExposedList<IkConstraint> IkConstraints { get { return ikConstraints; } }
		/// <summary>The skeleton's path constraints.</summary>
		public ExposedList<PathConstraint> PathConstraints { get { return pathConstraints; } }
		/// <summary>The skeleton's physics constraints.</summary>
		public ExposedList<PhysicsConstraint> PhysicsConstraints { get { return physicsConstraints; } }
		/// <summary>The skeleton's transform constraints.</summary>
		public ExposedList<TransformConstraint> TransformConstraints { get { return transformConstraints; } }

		/// <summary>The skeleton's current skin. May be null. See <see cref="SetSkin(Spine.Skin)"/></summary>
		public Skin Skin {
			/// <summary>The skeleton's current skin. May be null.</summary>
			get { return skin; }
			/// <summary>Sets a skin, <see cref="SetSkin(Skin)"/>.</summary>
			set { SetSkin(value); }
		}
		public float R { get { return r; } set { r = value; } }
		public float G { get { return g; } set { g = value; } }
		public float B { get { return b; } set { b = value; } }
		public float A { get { return a; } set { a = value; } }
		/// <summary><para>The skeleton X position, which is added to the root bone worldX position.</para>
		/// <para>
		/// Bones that do not inherit translation are still affected by this property.</para></summary>
		public float X { get { return x; } set { x = value; } }
		/// <summary><para>The skeleton Y position, which is added to the root bone worldY position.</para>
		/// <para>
		/// Bones that do not inherit translation are still affected by this property.</para></summary>
		public float Y { get { return y; } set { y = value; } }
		/// <summary><para> Scales the entire skeleton on the X axis.</para>
		/// <para>
		/// Bones that do not inherit scale are still affected by this property.</para></summary>
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		/// <summary><para> Scales the entire skeleton on the Y axis.</para>
		/// <para>
		/// Bones that do not inherit scale are still affected by this property.</para></summary>
		public float ScaleY { get { return scaleY * (Bone.yDown ? -1 : 1); } set { scaleY = value; } }

		[Obsolete("Use ScaleX instead. FlipX is when ScaleX is negative.")]
		public bool FlipX { get { return scaleX < 0; } set { scaleX = value ? -1f : 1f; } }

		[Obsolete("Use ScaleY instead. FlipY is when ScaleY is negative.")]
		public bool FlipY { get { return scaleY < 0; } set { scaleY = value ? -1f : 1f; } }
		/// <summary>Returns the skeleton's time. This is used for time-based manipulations, such as <see cref="PhysicsConstraint"/>.</summary>
		/// <seealso cref="Update(float)"/>
		public float Time { get { return time; } set { time = value; } }

		/// <summary>Returns the root bone, or null if the skeleton has no bones.</summary>
		public Bone RootBone {
			get { return bones.Count == 0 ? null : bones.Items[0]; }
		}

		public Skeleton (SkeletonData data) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			this.data = data;

			bones = new ExposedList<Bone>(data.bones.Count);
			Bone[] bonesItems = this.bones.Items;
			foreach (BoneData boneData in data.bones) {
				Bone bone;
				if (boneData.parent == null) {
					bone = new Bone(boneData, this, null);
				} else {
					Bone parent = bonesItems[boneData.parent.index];
					bone = new Bone(boneData, this, parent);
					parent.children.Add(bone);
				}
				this.bones.Add(bone);
			}

			slots = new ExposedList<Slot>(data.slots.Count);
			drawOrder = new ExposedList<Slot>(data.slots.Count);
			foreach (SlotData slotData in data.slots) {
				Bone bone = bonesItems[slotData.boneData.index];
				Slot slot = new Slot(slotData, bone);
				slots.Add(slot);
				drawOrder.Add(slot);
			}

			ikConstraints = new ExposedList<IkConstraint>(data.ikConstraints.Count);
			foreach (IkConstraintData ikConstraintData in data.ikConstraints)
				ikConstraints.Add(new IkConstraint(ikConstraintData, this));

			transformConstraints = new ExposedList<TransformConstraint>(data.transformConstraints.Count);
			foreach (TransformConstraintData transformConstraintData in data.transformConstraints)
				transformConstraints.Add(new TransformConstraint(transformConstraintData, this));

			pathConstraints = new ExposedList<PathConstraint>(data.pathConstraints.Count);
			foreach (PathConstraintData pathConstraintData in data.pathConstraints)
				pathConstraints.Add(new PathConstraint(pathConstraintData, this));

			physicsConstraints = new ExposedList<PhysicsConstraint>(data.physicsConstraints.Count);
			foreach (PhysicsConstraintData physicsConstraintData in data.physicsConstraints)
				physicsConstraints.Add(new PhysicsConstraint(physicsConstraintData, this));

			UpdateCache();
		}

		/// <summary>Copy constructor.</summary>
		public Skeleton (Skeleton skeleton) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			data = skeleton.data;

			bones = new ExposedList<Bone>(skeleton.bones.Count);
			foreach (Bone bone in skeleton.bones) {
				Bone newBone;
				if (bone.parent == null)
					newBone = new Bone(bone, this, null);
				else {
					Bone parent = bones.Items[bone.parent.data.index];
					newBone = new Bone(bone, this, parent);
					parent.children.Add(newBone);
				}
				bones.Add(newBone);
			}

			slots = new ExposedList<Slot>(skeleton.slots.Count);
			Bone[] bonesItems = bones.Items;
			foreach (Slot slot in skeleton.slots) {
				Bone bone = bonesItems[slot.bone.data.index];
				slots.Add(new Slot(slot, bone));
			}

			drawOrder = new ExposedList<Slot>(slots.Count);
			Slot[] slotsItems = slots.Items;
			foreach (Slot slot in skeleton.drawOrder)
				drawOrder.Add(slotsItems[slot.data.index]);

			ikConstraints = new ExposedList<IkConstraint>(skeleton.ikConstraints.Count);
			foreach (IkConstraint ikConstraint in skeleton.ikConstraints)
				ikConstraints.Add(new IkConstraint(ikConstraint, skeleton));

			transformConstraints = new ExposedList<TransformConstraint>(skeleton.transformConstraints.Count);
			foreach (TransformConstraint transformConstraint in skeleton.transformConstraints)
				transformConstraints.Add(new TransformConstraint(transformConstraint, skeleton));

			pathConstraints = new ExposedList<PathConstraint>(skeleton.pathConstraints.Count);
			foreach (PathConstraint pathConstraint in skeleton.pathConstraints)
				pathConstraints.Add(new PathConstraint(pathConstraint, skeleton));

			physicsConstraints = new ExposedList<PhysicsConstraint>(skeleton.physicsConstraints.Count);
			foreach (PhysicsConstraint physicsConstraint in skeleton.physicsConstraints)
				physicsConstraints.Add(new PhysicsConstraint(physicsConstraint, skeleton));

			skin = skeleton.skin;
			r = skeleton.r;
			g = skeleton.g;
			b = skeleton.b;
			a = skeleton.a;
			x = skeleton.x;
			y = skeleton.y;
			scaleX = skeleton.scaleX;
			scaleY = skeleton.scaleY;
			time = skeleton.time;

			UpdateCache();
		}

		/// <summary>Caches information about bones and constraints. Must be called if the <see cref="Skin"/> is modified or if bones, constraints, or
		/// constraints, or weighted path attachments are added or removed.</summary>
		public void UpdateCache () {
			ExposedList<IUpdatable> updateCache = this.updateCache;
			updateCache.Clear();

			int boneCount = this.bones.Count;
			Bone[] bones = this.bones.Items;
			for (int i = 0; i < boneCount; i++) {
				Bone bone = bones[i];
				bone.sorted = bone.data.skinRequired;
				bone.active = !bone.sorted;
			}
			if (skin != null) {
				BoneData[] skinBones = skin.bones.Items;
				for (int i = 0, n = skin.bones.Count; i < n; i++) {
					Bone bone = bones[skinBones[i].index];
					do {
						bone.sorted = false;
						bone.active = true;
						bone = bone.parent;
					} while (bone != null);
				}
			}

			int ikCount = this.ikConstraints.Count, transformCount = this.transformConstraints.Count, pathCount = this.pathConstraints.Count,
				physicsCount = this.physicsConstraints.Count;
			IkConstraint[] ikConstraints = this.ikConstraints.Items;
			TransformConstraint[] transformConstraints = this.transformConstraints.Items;
			PathConstraint[] pathConstraints = this.pathConstraints.Items;
			PhysicsConstraint[] physicsConstraints = this.physicsConstraints.Items;
			int constraintCount = ikCount + transformCount + pathCount + physicsCount;
			for (int i = 0; i < constraintCount; i++) {
				for (int ii = 0; ii < ikCount; ii++) {
					IkConstraint constraint = ikConstraints[ii];
					if (constraint.data.order == i) {
						SortIkConstraint(constraint);
						goto continue_outer;
					}
				}
				for (int ii = 0; ii < transformCount; ii++) {
					TransformConstraint constraint = transformConstraints[ii];
					if (constraint.data.order == i) {
						SortTransformConstraint(constraint);
						goto continue_outer;
					}
				}
				for (int ii = 0; ii < pathCount; ii++) {
					PathConstraint constraint = pathConstraints[ii];
					if (constraint.data.order == i) {
						SortPathConstraint(constraint);
						goto continue_outer;
					}
				}
				for (int ii = 0; ii < physicsCount; ii++) {
					PhysicsConstraint constraint = physicsConstraints[ii];
					if (constraint.data.order == i) {
						SortPhysicsConstraint(constraint);
						goto continue_outer;
					}
				}
				continue_outer: { }
			}

			for (int i = 0; i < boneCount; i++)
				SortBone(bones[i]);
		}

		private void SortIkConstraint (IkConstraint constraint) {
			constraint.active = constraint.target.active
				&& (!constraint.data.skinRequired || (skin != null && skin.constraints.Contains(constraint.data)));
			if (!constraint.active) return;

			Bone target = constraint.target;
			SortBone(target);

			ExposedList<Bone> constrained = constraint.bones;
			Bone parent = constrained.Items[0];
			SortBone(parent);

			if (constrained.Count == 1) {
				updateCache.Add(constraint);
				SortReset(parent.children);
			} else {
				Bone child = constrained.Items[constrained.Count - 1];
				SortBone(child);

				updateCache.Add(constraint);

				SortReset(parent.children);
				child.sorted = true;
			}
		}

		private void SortTransformConstraint (TransformConstraint constraint) {
			constraint.active = constraint.target.active
				&& (!constraint.data.skinRequired || (skin != null && skin.constraints.Contains(constraint.data)));
			if (!constraint.active) return;

			SortBone(constraint.target);

			Bone[] constrained = constraint.bones.Items;
			int boneCount = constraint.bones.Count;
			if (constraint.data.local) {
				for (int i = 0; i < boneCount; i++) {
					Bone child = constrained[i];
					SortBone(child.parent);
					SortBone(child);
				}
			} else {
				for (int i = 0; i < boneCount; i++)
					SortBone(constrained[i]);
			}

			updateCache.Add(constraint);

			for (int i = 0; i < boneCount; i++)
				SortReset(constrained[i].children);
			for (int i = 0; i < boneCount; i++)
				constrained[i].sorted = true;
		}

		private void SortPathConstraint (PathConstraint constraint) {
			constraint.active = constraint.target.bone.active
				&& (!constraint.data.skinRequired || (skin != null && skin.constraints.Contains(constraint.data)));
			if (!constraint.active) return;

			Slot slot = constraint.target;
			int slotIndex = slot.data.index;
			Bone slotBone = slot.bone;
			if (skin != null) SortPathConstraintAttachment(skin, slotIndex, slotBone);
			if (data.defaultSkin != null && data.defaultSkin != skin)
				SortPathConstraintAttachment(data.defaultSkin, slotIndex, slotBone);

			Attachment attachment = slot.attachment;
			if (attachment is PathAttachment) SortPathConstraintAttachment(attachment, slotBone);

			Bone[] constrained = constraint.bones.Items;
			int boneCount = constraint.bones.Count;
			for (int i = 0; i < boneCount; i++)
				SortBone(constrained[i]);

			updateCache.Add(constraint);

			for (int i = 0; i < boneCount; i++)
				SortReset(constrained[i].children);
			for (int i = 0; i < boneCount; i++)
				constrained[i].sorted = true;
		}

		private void SortPathConstraintAttachment (Skin skin, int slotIndex, Bone slotBone) {
			foreach (Skin.SkinEntry entry in skin.Attachments)
				if (entry.SlotIndex == slotIndex) SortPathConstraintAttachment(entry.Attachment, slotBone);
		}

		private void SortPathConstraintAttachment (Attachment attachment, Bone slotBone) {
			if (!(attachment is PathAttachment)) return;
			int[] pathBones = ((PathAttachment)attachment).bones;
			if (pathBones == null)
				SortBone(slotBone);
			else {
				Bone[] bones = this.bones.Items;
				for (int i = 0, n = pathBones.Length; i < n;) {
					int nn = pathBones[i++];
					nn += i;
					while (i < nn)
						SortBone(bones[pathBones[i++]]);
				}
			}
		}

		private void SortPhysicsConstraint (PhysicsConstraint constraint) {
			Bone bone = constraint.bone;
			constraint.active = bone.active
				&& (!constraint.data.skinRequired || (skin != null && skin.constraints.Contains(constraint.data)));
			if (!constraint.active) return;

			SortBone(bone);

			updateCache.Add(constraint);

			SortReset(bone.children);
			bone.sorted = true;
		}

		private void SortBone (Bone bone) {
			if (bone.sorted) return;
			Bone parent = bone.parent;
			if (parent != null) SortBone(parent);
			bone.sorted = true;
			updateCache.Add(bone);
		}

		private static void SortReset (ExposedList<Bone> bones) {
			Bone[] bonesItems = bones.Items;
			for (int i = 0, n = bones.Count; i < n; i++) {
				Bone bone = bonesItems[i];
				if (!bone.active) continue;
				if (bone.sorted) SortReset(bone.children);
				bone.sorted = false;
			}
		}

		/// <summary>
		/// Updates the world transform for each bone and applies all constraints.
		/// <para>
		/// See <a href="http://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
		/// Runtimes Guide.</para>
		/// </summary>
		public void UpdateWorldTransform (Physics physics) {
			Bone[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				Bone bone = bones[i];
				bone.ax = bone.x;
				bone.ay = bone.y;
				bone.arotation = bone.rotation;
				bone.ascaleX = bone.scaleX;
				bone.ascaleY = bone.scaleY;
				bone.ashearX = bone.shearX;
				bone.ashearY = bone.shearY;
			}

			IUpdatable[] updateCache = this.updateCache.Items;
			for (int i = 0, n = this.updateCache.Count; i < n; i++)
				updateCache[i].Update(physics);
		}

		/// <summary>
		/// Temporarily sets the root bone as a child of the specified bone, then updates the world transform for each bone and applies
		/// all constraints.
		/// </summary>
		public void UpdateWorldTransform (Physics physics, Bone parent) {
			if (parent == null) throw new ArgumentNullException("parent", "parent cannot be null.");

			// Apply the parent bone transform to the root bone. The root bone always inherits scale, rotation and reflection.
			Bone rootBone = this.RootBone;
			float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			rootBone.worldX = pa * x + pb * y + parent.worldX;
			rootBone.worldY = pc * x + pd * y + parent.worldY;

			float rx = (rootBone.rotation + rootBone.shearX) * MathUtils.DegRad;
			float ry = (rootBone.rotation + 90 + rootBone.shearY) * MathUtils.DegRad;
			float la = (float)Math.Cos(rx) * rootBone.scaleX;
			float lb = (float)Math.Cos(ry) * rootBone.scaleY;
			float lc = (float)Math.Sin(rx) * rootBone.scaleX;
			float ld = (float)Math.Sin(ry) * rootBone.scaleY;
			rootBone.a = (pa * la + pb * lc) * scaleX;
			rootBone.b = (pa * lb + pb * ld) * scaleX;
			rootBone.c = (pc * la + pd * lc) * scaleY;
			rootBone.d = (pc * lb + pd * ld) * scaleY;

			// Update everything except root bone.
			IUpdatable[] updateCache = this.updateCache.Items;
			for (int i = 0, n = this.updateCache.Count; i < n; i++) {
				IUpdatable updatable = updateCache[i];
				if (updatable != rootBone) updatable.Update(physics);
			}
		}

		/// <summary>
		/// Calls <see cref="PhysicsConstraint.Translate(float, float)"/> for each physics constraint.
		/// </summary>
		public void PhysicsTranslate (float x, float y) {
			PhysicsConstraint[] physicsConstraints = this.physicsConstraints.Items;
			for (int i = 0, n = this.physicsConstraints.Count; i < n; i++)
				physicsConstraints[i].Translate(x, y);
		}

		/// <summary>
		/// Calls <see cref="PhysicsConstraint.Rotate(float, float, float)"/> for each physics constraint.
		/// </summary>
		public void PhysicsRotate (float x, float y, float degrees) {
			PhysicsConstraint[] physicsConstraints = this.physicsConstraints.Items;
			for (int i = 0, n = this.physicsConstraints.Count; i < n; i++)
				physicsConstraints[i].Rotate(x, y, degrees);
		}

		/// <summary>Increments the skeleton's <see cref="time"/>.</summary>
		public void Update (float delta) {
			time += delta;
		}

		/// <summary>Sets the bones, constraints, and slots to their setup pose values.</summary>
		public void SetToSetupPose () {
			SetBonesToSetupPose();
			SetSlotsToSetupPose();
		}

		/// <summary>Sets the bones and constraints to their setup pose values.</summary>
		public void SetBonesToSetupPose () {
			Bone[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++)
				bones[i].SetToSetupPose();

			IkConstraint[] ikConstraints = this.ikConstraints.Items;
			for (int i = 0, n = this.ikConstraints.Count; i < n; i++)
				ikConstraints[i].SetToSetupPose();

			TransformConstraint[] transformConstraints = this.transformConstraints.Items;
			for (int i = 0, n = this.transformConstraints.Count; i < n; i++)
				transformConstraints[i].SetToSetupPose();

			PathConstraint[] pathConstraints = this.pathConstraints.Items;
			for (int i = 0, n = this.pathConstraints.Count; i < n; i++)
				pathConstraints[i].SetToSetupPose();

			PhysicsConstraint[] physicsConstraints = this.physicsConstraints.Items;
			for (int i = 0, n = this.physicsConstraints.Count; i < n; i++)
				physicsConstraints[i].SetToSetupPose();
		}

		public void SetSlotsToSetupPose () {
			Slot[] slots = this.slots.Items;
			int n = this.slots.Count;
			Array.Copy(slots, 0, drawOrder.Items, 0, n);
			for (int i = 0; i < n; i++)
				slots[i].SetToSetupPose();
		}

		/// <summary>Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it
		/// repeatedly.</summary>
		/// <returns>May be null.</returns>
		public Bone FindBone (string boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName", "boneName cannot be null.");
			Bone[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				Bone bone = bones[i];
				if (bone.data.name == boneName) return bone;
			}
			return null;
		}

		/// <summary>Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it
		/// repeatedly.</summary>
		/// <returns>May be null.</returns>
		public Slot FindSlot (string slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName", "slotName cannot be null.");
			Slot[] slots = this.slots.Items;
			for (int i = 0, n = this.slots.Count; i < n; i++) {
				Slot slot = slots[i];
				if (slot.data.name == slotName) return slot;
			}
			return null;
		}

		/// <summary>Sets a skin by name (see <see cref="SetSkin(Spine.Skin)"/>).</summary>
		public void SetSkin (string skinName) {
			Skin foundSkin = data.FindSkin(skinName);
			if (foundSkin == null) throw new ArgumentException("Skin not found: " + skinName, "skinName");
			SetSkin(foundSkin);
		}

		/// <summary>
		/// <para>Sets the skin used to look up attachments before looking in the <see cref="SkeletonData.DefaultSkin"/>. If the
		/// skin is changed, <see cref="UpdateCache()"/> is called.
		/// </para>
		/// <para>Attachments from the new skin are attached if the corresponding attachment from the old skin was attached.
		/// If there was no old skin, each slot's setup mode attachment is attached from the new skin.
		/// </para>
		/// <para>After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
		/// <see cref="Skeleton.SetSlotsToSetupPose()"/>.
		/// Also, often <see cref="AnimationState.Apply(Skeleton)"/> is called before the next time the
		/// skeleton is rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin.</para>
		/// </summary>
		/// <param name="newSkin">May be null.</param>
		public void SetSkin (Skin newSkin) {
			if (newSkin == skin) return;
			if (newSkin != null) {
				if (skin != null)
					newSkin.AttachAll(this, skin);
				else {
					Slot[] slots = this.slots.Items;
					for (int i = 0, n = this.slots.Count; i < n; i++) {
						Slot slot = slots[i];
						string name = slot.data.attachmentName;
						if (name != null) {
							Attachment attachment = newSkin.GetAttachment(i, name);
							if (attachment != null) slot.Attachment = attachment;
						}
					}
				}
			}
			skin = newSkin;
			UpdateCache();
		}

		/// <summary>Finds an attachment by looking in the <see cref="Skeleton.Skin"/> and <see cref="SkeletonData.DefaultSkin"/> using the slot name and attachment name.</summary>
		/// <returns>May be null.</returns>
		public Attachment GetAttachment (string slotName, string attachmentName) {
			return GetAttachment(data.FindSlot(slotName).index, attachmentName);
		}

		/// <summary>Finds an attachment by looking in the skin and skeletonData.defaultSkin using the slot index and attachment name.First the skin is checked and if the attachment was not found, the default skin is checked.</summary>
		/// <returns>May be null.</returns>
		public Attachment GetAttachment (int slotIndex, string attachmentName) {
			if (attachmentName == null) throw new ArgumentNullException("attachmentName", "attachmentName cannot be null.");
			if (skin != null) {
				Attachment attachment = skin.GetAttachment(slotIndex, attachmentName);
				if (attachment != null) return attachment;
			}
			return data.defaultSkin != null ? data.defaultSkin.GetAttachment(slotIndex, attachmentName) : null;
		}

		/// <summary>A convenience method to set an attachment by finding the slot with FindSlot, finding the attachment with GetAttachment, then setting the slot's slot.Attachment.</summary>
		/// <param name="attachmentName">May be null to clear the slot's attachment.</param>
		public void SetAttachment (string slotName, string attachmentName) {
			if (slotName == null) throw new ArgumentNullException("slotName", "slotName cannot be null.");
			Slot[] slots = this.slots.Items;
			for (int i = 0, n = this.slots.Count; i < n; i++) {
				Slot slot = slots[i];
				if (slot.data.name == slotName) {
					Attachment attachment = null;
					if (attachmentName != null) {
						attachment = GetAttachment(i, attachmentName);
						if (attachment == null) throw new Exception("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}
					slot.Attachment = attachment;
					return;
				}
			}
			throw new Exception("Slot not found: " + slotName);
		}

		/// <summary>Finds an IK constraint by comparing each IK constraint's name. It is more efficient to cache the results of this method
		/// than to call it repeatedly.</summary>
		/// <returns>May be null.</returns>
		public IkConstraint FindIkConstraint (string constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			IkConstraint[] ikConstraints = this.ikConstraints.Items;
			for (int i = 0, n = this.ikConstraints.Count; i < n; i++) {
				IkConstraint ikConstraint = ikConstraints[i];
				if (ikConstraint.data.name == constraintName) return ikConstraint;
			}
			return null;
		}

		/// <summary>Finds a transform constraint by comparing each transform constraint's name. It is more efficient to cache the results of
		/// this method than to call it repeatedly.</summary>
		/// <returns>May be null.</returns>
		public TransformConstraint FindTransformConstraint (string constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			TransformConstraint[] transformConstraints = this.transformConstraints.Items;
			for (int i = 0, n = this.transformConstraints.Count; i < n; i++) {
				TransformConstraint transformConstraint = transformConstraints[i];
				if (transformConstraint.data.Name == constraintName) return transformConstraint;
			}
			return null;
		}

		/// <summary>Finds a path constraint by comparing each path constraint's name. It is more efficient to cache the results of this method
		/// than to call it repeatedly.</summary>
		/// <returns>May be null.</returns>
		public PathConstraint FindPathConstraint (string constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			PathConstraint[] pathConstraints = this.pathConstraints.Items;
			for (int i = 0, n = this.pathConstraints.Count; i < n; i++) {
				PathConstraint constraint = pathConstraints[i];
				if (constraint.data.Name.Equals(constraintName)) return constraint;
			}
			return null;
		}

		/// <summary>Finds a physics constraint by comparing each physics constraint's name. It is more efficient to cache the results of this
		/// method than to call it repeatedly.</summary>
		/// <returns>May be null.</returns>
		public PhysicsConstraint FindPhysicsConstraint (String constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			PhysicsConstraint[] physicsConstraints = this.physicsConstraints.Items;
			for (int i = 0, n = this.physicsConstraints.Count; i < n; i++) {
				PhysicsConstraint constraint = physicsConstraints[i];
				if (constraint.data.name.Equals(constraintName)) return constraint;
			}
			return null;
		}

		/// <summary>Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.</summary>
		/// <param name="x">The horizontal distance between the skeleton origin and the left side of the AABB.</param>
		/// <param name="y">The vertical distance between the skeleton origin and the bottom side of the AABB.</param>
		/// <param name="width">The width of the AABB</param>
		/// <param name="height">The height of the AABB.</param>
		/// <param name="vertexBuffer">Reference to hold a float[]. May be a null reference. This method will assign it a new float[] with the appropriate size as needed.</param>
		public void GetBounds (out float x, out float y, out float width, out float height, ref float[] vertexBuffer,
			SkeletonClipping clipper = null) {

			float[] temp = vertexBuffer;
			temp = temp ?? new float[8];
			Slot[] drawOrder = this.drawOrder.Items;
			float minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
			for (int i = 0, n = this.drawOrder.Count; i < n; i++) {
				Slot slot = drawOrder[i];
				if (!slot.bone.active) continue;
				int verticesLength = 0;
				float[] vertices = null;
				int[] triangles = null;
				Attachment attachment = slot.attachment;
				RegionAttachment region = attachment as RegionAttachment;
				if (region != null) {
					verticesLength = 8;
					vertices = temp;
					if (vertices.Length < 8) vertices = temp = new float[8];
					region.ComputeWorldVertices(slot, temp, 0, 2);
					triangles = quadTriangles;
				} else {
					MeshAttachment mesh = attachment as MeshAttachment;
					if (mesh != null) {
						verticesLength = mesh.WorldVerticesLength;
						vertices = temp;
						if (vertices.Length < verticesLength) vertices = temp = new float[verticesLength];
						mesh.ComputeWorldVertices(slot, 0, verticesLength, temp, 0, 2);
						triangles = mesh.Triangles;
					} else if (clipper != null) {
						ClippingAttachment clip = attachment as ClippingAttachment;
						if (clip != null) {
							clipper.ClipStart(slot, clip);
							continue;
						}
					}
				}

				if (vertices != null) {
					if (clipper != null && clipper.IsClipping) {
						clipper.ClipTriangles(vertices, triangles, triangles.Length);
						vertices = clipper.ClippedVertices.Items;
						verticesLength = clipper.ClippedVertices.Count;
					}

					for (int ii = 0; ii < verticesLength; ii += 2) {
						float vx = vertices[ii], vy = vertices[ii + 1];
						minX = Math.Min(minX, vx);
						minY = Math.Min(minY, vy);
						maxX = Math.Max(maxX, vx);
						maxY = Math.Max(maxY, vy);
					}
				}
				if (clipper != null) clipper.ClipEnd(slot);
			}
			if (clipper != null) clipper.ClipEnd();
			x = minX;
			y = minY;
			width = maxX - minX;
			height = maxY - minY;
			vertexBuffer = temp;
		}

		override public string ToString () {
			return data.name;
		}

		/// <summary>Determines how physics and other non-deterministic updates are applied.</summary>
		public enum Physics {
			/// <summary>Physics are not updated or applied.</summary>
			None,

			/// <summary>Physics are reset to the current pose.</summary>
			Reset,

			/// <summary>Physics are updated and the pose from physics is applied.</summary>
			Update,

			/// <summary>Physics are not updated but the pose from physics is applied.</summary>
			Pose
		}
	}
}
