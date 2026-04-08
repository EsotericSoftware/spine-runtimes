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
	/// <summary>Stores bones and slots to be posed by animations and application code. Multiple skeleton instances can share the same
	/// <see cref="SkeletonData"/>, including animations, attachments, and skins.
	/// <para>After posing, call <see cref="UpdateWorldTransform(Physics)"/> to apply constraints and compute world transforms for
	/// rendering.</para>
	/// <para>See <see href="https://esotericsoftware.com/spine-runtime-architecture#Instance-objects">Instance objects</see> in the
	/// Spine Runtimes Guide.</para></summary>
	public class Skeleton {
		static private readonly int[] quadTriangles = { 0, 1, 2, 2, 3, 0 };
		internal SkeletonData data;
		internal ExposedList<Bone> bones;
		internal ExposedList<Slot> slots;
		internal readonly DrawOrder drawOrder;
		internal ExposedList<IConstraint> constraints;
		internal ExposedList<PhysicsConstraint> physics;
		internal ExposedList<object> updateCache = new ExposedList<object>();
		internal ExposedList<IPosedInternal> resetCache = new ExposedList<IPosedInternal>(16);
		internal Skin skin;
		// Color is a struct, set to protected to prevent
		// Color32F color = slot.color; color.a = 0.5;
		// modifying just a copy of the struct instead of the original
		// object as in reference implementation.
		protected Color32F color;
		internal float x, y, scaleX = 1, time, windX = 1, windY = 0, gravityX = 0, gravityY = 1;
		/// <summary>Private to enforce usage of ScaleY getter taking Bone.yDown into account.</summary>
		private float scaleY = 1;
		internal int update;

		public Skeleton (SkeletonData data) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			this.data = data;

			bones = new ExposedList<Bone>(data.bones.Count);
			Bone[] bonesItems = this.bones.Items;
			foreach (BoneData boneData in data.bones) {
				Bone bone;
				if (boneData.parent == null) {
					bone = new Bone(boneData, null);
				} else {
					Bone parent = bonesItems[boneData.parent.index];
					bone = new Bone(boneData, parent);
					parent.children.Add(bone);
				}
				this.bones.Add(bone);
			}

			slots = new ExposedList<Slot>(data.slots.Count);
			foreach (SlotData slotData in data.slots)
				slots.Add(new Slot(slotData, this));
			drawOrder = new DrawOrder(slots);

			physics = new ExposedList<PhysicsConstraint>(8);
			constraints = new ExposedList<IConstraint>(data.constraints.Count);
			foreach (IConstraintData constraintData in data.constraints) {
				IConstraint constraint = constraintData.Create(this);
				PhysicsConstraint physicsConstraint = constraint as PhysicsConstraint;
				if (physicsConstraint != null) physics.Add(physicsConstraint);
				constraints.Add(constraint);
			}
			physics.TrimExcess();

			color = new Color32F(1, 1, 1, 1);

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
					newBone = new Bone(bone, null);
				else {
					Bone parent = bones.Items[bone.parent.data.index];
					newBone = new Bone(bone, parent);
					parent.children.Add(newBone);
				}
				bones.Add(newBone);
			}

			slots = new ExposedList<Slot>(skeleton.slots.Count);
			Bone[] bonesItems = bones.Items;
			foreach (Slot slot in skeleton.slots)
				slots.Add(new Slot(slot, bonesItems[slot.bone.data.index], this));

			drawOrder = new DrawOrder(slots);
			drawOrder.pose.Clear();
			foreach (Slot slot in skeleton.drawOrder.pose)
				drawOrder.pose.Add(slots.Items[slot.data.index]);

			physics = new ExposedList<PhysicsConstraint>(skeleton.physics.Count);
			constraints = new ExposedList<IConstraint>(skeleton.constraints.Count);
			foreach (IConstraint other in skeleton.constraints) {
				IConstraint constraint = other.Copy(this);
				PhysicsConstraint physicsConstraint = constraint as PhysicsConstraint;
				if (physicsConstraint != null) physics.Add(physicsConstraint);
				constraints.Add(constraint);
			}

			skin = skeleton.skin;
			color = skeleton.color;
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
			updateCache.Clear();
			resetCache.Clear();

			drawOrder.Unconstrained();
			Slot[] slots = this.slots.Items;
			for (int i = 0, n = this.slots.Count; i < n; i++) {
				((IPosedInternal)slots[i]).Unconstrained();
			}

			int boneCount = this.bones.Count;
			Bone[] bones = this.bones.Items;
			for (int i = 0; i < boneCount; i++) {
				Bone bone = bones[i];
				bone.sorted = bone.data.skinRequired;
				bone.active = !bone.sorted;
				((IPosedInternal)bone).Unconstrained();
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
			IConstraint[] constraints = this.constraints.Items;

			{ // scope added to prevent compile error of n already being declared in enclosing scope
				int n = this.constraints.Count;
				for (int i = 0; i < n; i++) {
					((IPosedInternal)constraints[i]).Unconstrained();
				}
				for (int i = 0; i < n; i++) {
					IConstraint constraint = constraints[i];
					constraint.Active = constraint.IsSourceActive
						&& (!constraint.IData.SkinRequired || (skin != null && skin.constraints.Contains(constraint.IData)));
					if (constraint.Active) constraint.Sort(this);
				}

				for (int i = 0; i < boneCount; i++)
					SortBone(bones[i]);

				object[] updateCache = this.updateCache.Items;
				n = this.updateCache.Count;
				for (int i = 0; i < n; i++) {
					Bone bone = updateCache[i] as Bone;
					if (bone != null) updateCache[i] = bone.appliedPose;
				}
			}
		}

		internal void Constrained (IPosedInternal obj) {
			if (obj.PoseEqualsApplied) { // if (obj.pose == obj.appliedPose) {
				obj.Constrained();
				resetCache.Add(obj);
			}
		}

		internal void SortBone (Bone bone) {
			if (bone.sorted || !bone.active) return;
			Bone parent = bone.parent;
			if (parent != null) SortBone(parent);
			bone.sorted = true;
			updateCache.Add(bone);
		}

		internal void SortReset (ExposedList<Bone> bones) {
			Bone[] items = bones.Items;
			for (int i = 0, n = bones.Count; i < n; i++) {
				Bone bone = items[i];
				if (bone.active) {
					if (bone.sorted) SortReset(bone.children);
					bone.sorted = false;
				}
			}
		}

		/// <summary>
		/// Updates the world transform for each bone and applies all constraints.
		/// <para>
		/// See <a href="http://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
		/// Runtimes Guide.</para>
		/// </summary>
		public void UpdateWorldTransform (Physics physics) {
			update++;

			drawOrder.ResetConstrained();
			IPosedInternal[] resetCache = this.resetCache.Items;
			for (int i = 0, n = this.resetCache.Count; i < n; i++) {
				resetCache[i].ResetConstrained();
			}

			object[] updateCache = this.updateCache.Items;
			for (int i = 0, n = this.updateCache.Count; i < n; i++)
				((IUpdate)updateCache[i]).Update(this, physics);
		}

		/// <summary>Sets the bones, constraints, slots, and draw order to their setup pose values.</summary>
		public void SetupPose () {
			SetupPoseBones();
			SetupPoseSlots();
		}

		/// <summary>Sets the bones and constraints to their setup pose values.</summary>
		public void SetupPoseBones () {
			Bone[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++)
				bones[i].SetupPose();

			IConstraint[] constraints = this.constraints.Items;
			for (int i = 0, n = this.constraints.Count; i < n; i++)
				constraints[i].SetupPose();
		}

		/// <summary>Sets the slots and draw order to their setup pose values.</summary>
		public void SetupPoseSlots () {
			drawOrder.SetupPose();
			Slot[] slots = this.slots.Items;
			for (int i = 0, n = this.slots.Count; i < n; i++)
				slots[i].SetupPose();
		}

		/// <summary>The skeleton's setup pose data.</summary>
		public SkeletonData Data { get { return data; } }
		/// <summary>The skeleton's bones, sorted parent first. The root bone is always the first bone.</summary>
		public ExposedList<Bone> Bones { get { return bones; } }
		/// <summary>
		/// The list of bones and constraints, sorted in the order they should be updated, as computed by <see cref="UpdateCache()"/>.
		/// </summary>
		public ExposedList<object> UpdateCacheList { get { return updateCache; } }
		/// <summary>Returns the root bone, or null if the skeleton has no bones.</summary>
		public Bone RootBone {
			get { return bones.Count == 0 ? null : bones.Items[0]; }
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

		/// <summary>The skeleton's slots. To add a slot, also add it to <see cref="DrawOrder.Pose"/>.</summary>
		public ExposedList<Slot> Slots { get { return slots; } }

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

		/// <summary>
		/// The skeleton's draw order. Use <see cref="DrawOrder.AppliedPose"/> for rendering and
		/// <see cref="DrawOrder.Pose"/> for changing the draw order.
		/// </summary>
		public DrawOrder DrawOrder {
			get { return drawOrder; }
		}

		/// <summary>The skeleton's current skin. May be null. See <see cref="SetSkin(Spine.Skin)"/></summary>
		public Skin Skin {
			/// <summary>The skeleton's current skin. May be null.</summary>
			get { return skin; }
			/// <summary>Sets a skin, <see cref="SetSkin(Skin)"/>.</summary>
			set { SetSkin(value); }
		}

		/// <summary>Sets a skin by name (see <see cref="SetSkin(Spine.Skin)"/>).</summary>
		public void SetSkin (string skinName) {
			Skin foundSkin = data.FindSkin(skinName);
			if (foundSkin == null) throw new ArgumentException("Skin not found: " + skinName, "skinName");
			SetSkin(foundSkin);
		}

		/// <summary>
		/// <para>Sets the skin used to look up attachments before looking in <see cref="SkeletonData.DefaultSkin"/>. If the skin is
		/// changed, <see cref="UpdateCache()"/> is called.
		/// </para>
		/// <para>Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no
		/// old skin, each slot's setup mode attachment is attached from the new skin.
		/// </para>
		/// <para>After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
		/// <see cref="Skeleton.SetupPoseSlots()"/>. Also, often <see cref="AnimationState.Apply(Skeleton)"/> is called before the next time the skeleton is
		/// rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin.</para>
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
							if (attachment != null) slot.pose.Attachment = attachment;
						}
					}
				}
			}
			skin = newSkin;
			UpdateCache();
		}

		/// <summary>Finds an attachment by looking in the <see cref="Skeleton.Skin"/> and <see cref="SkeletonData.DefaultSkin"/> using the slot name and attachment
		/// name.</summary>
		/// <returns>May be null.</returns>
		/// <seealso cref="GetAttachment(int, string)"/>
		public Attachment GetAttachment (string slotName, string placeholderName) {
			SlotData slot = data.FindSlot(slotName);
			if (slot == null) throw new ArgumentException("Slot not found: " + slotName, "slotName");
			return GetAttachment(slot.index, placeholderName);
		}

		/// <summary>Finds an attachment by looking in the skin and skeletonData.defaultSkin using the slot index and skin
		/// placeholder name. First the skin is checked and if the attachment was not found, the default skin is checked.</summary>
		/// <para>
		/// See <a href="http://esotericsoftware.com/spine-runtime-skins">Runtime skins</a> in the Spine Runtimes Guide.</para>
		/// <returns>May be null.</returns>
		public Attachment GetAttachment (int slotIndex, string placeholderName) {
			if (placeholderName == null) throw new ArgumentNullException("placeholderName", "placeholderName cannot be null.");
			if (skin != null) {
				Attachment attachment = skin.GetAttachment(slotIndex, placeholderName);
				if (attachment != null) return attachment;
			}
			if (data.defaultSkin != null) return data.defaultSkin.GetAttachment(slotIndex, placeholderName);
			return null;
		}

		/// <summary>A convenience method to set an attachment by finding the slot with <see cref="FindSlot(string)"/>, finding the attachment with
		/// <see cref="GetAttachment(int, string)"/>, then setting the slot's <see cref="SlotPose.Attachment"/>.</summary>
		/// <param name="placeholderName">May be null to clear the slot's attachment.</param>
		public void SetAttachment (string slotName, string placeholderName) {
			if (slotName == null) throw new ArgumentNullException("slotName", "slotName cannot be null.");

			Slot slot = FindSlot(slotName);
			if (slot == null) throw new ArgumentException("Slot not found: " + slotName, "slotName");
			Attachment attachment = null;
			if (placeholderName != null) {
				attachment = GetAttachment(slot.data.index, placeholderName);
				if (attachment == null)
					throw new ArgumentException("Attachment not found: " + placeholderName + ", for slot: " + slotName, "placeholderName");
			}
			slot.pose.Attachment = attachment;
		}

		/// <summary>The skeleton's constraints.</summary>
		public ExposedList<IConstraint> Constraints { get { return constraints; } }
		/// <summary>The skeleton's physics constraints.</summary>
		public ExposedList<PhysicsConstraint> PhysicsConstraints { get { return physics; } }

		/// <summary>Finds a constraint of the specified type by comparing each constraint's name. It is more efficient to cache the
		/// results of this method than to call it multiple times.</summary>
		/// <returns>May be null.</returns>
		public T FindConstraint<T> (string constraintName) where T : class, IConstraint {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");

			IConstraint[] constraints = this.constraints.Items;
			for (int i = 0, n = this.constraints.Count; i < n; i++) {
				IConstraint constraint = constraints[i];
				if (constraint is T && constraint.IData.Name == constraintName) return (T)constraint;
			}
			return null;
		}

		/// <summary>Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the applied pose.</summary>
		/// <param name="x">The horizontal distance between the skeleton origin and the left side of the AABB.</param>
		/// <param name="y">The vertical distance between the skeleton origin and the bottom side of the AABB.</param>
		/// <param name="width">The width of the AABB</param>
		/// <param name="height">The height of the AABB.</param>
		/// <param name="vertexBuffer">Reference to hold a float[]. May be a null reference. This method will assign it a new float[] with the appropriate size as needed.</param>
		public void GetBounds (out float x, out float y, out float width, out float height, ref float[] vertexBuffer,
			SkeletonClipping clipper = null) {

			float[] temp = vertexBuffer;
			temp = temp ?? new float[8];
			ExposedList<Slot> drawOrder = this.drawOrder.appliedPose;
			Slot[] slots = drawOrder.Items;
			float minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
			for (int i = 0, n = drawOrder.Count; i < n; i++) {
				Slot slot = slots[i];
				if (!slot.bone.active) continue;
				int verticesLength = 0;
				float[] vertices = null;
				int[] triangles = null;
				Attachment attachment = slot.appliedPose.attachment;
				RegionAttachment region = attachment as RegionAttachment;
				if (region != null) {
					verticesLength = 8;
					vertices = temp;
					if (vertices.Length < 8) vertices = temp = new float[8];
					region.ComputeWorldVertices(slot, region.GetOffsets(slot.appliedPose), vertices, 0, 2);
					triangles = quadTriangles;
				} else {
					MeshAttachment mesh = attachment as MeshAttachment;
					if (mesh != null) {
						verticesLength = mesh.WorldVerticesLength;
						vertices = temp;
						if (vertices.Length < verticesLength) vertices = temp = new float[verticesLength];
						mesh.ComputeWorldVertices(this, slot, 0, verticesLength, temp, 0, 2);
						triangles = mesh.Triangles;
					} else if (clipper != null) {
						ClippingAttachment clip = attachment as ClippingAttachment;
						if (clip != null) {
							clipper.ClipEnd(slot);
							clipper.ClipStart(this, slot, clip);
							continue;
						}
					}
				}

				if (vertices != null) {
					if (clipper != null && clipper.IsClipping && clipper.ClipTriangles(vertices, triangles, triangles.Length)) {
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

		/// <returns>A copy of the color to tint all the skeleton's attachments.</returns>
		public Color32F GetColor () {
			return color;
		}

		/// <summary>Sets the color to tint all the skeleton's attachments.</summary>
		public void SetColor (Color32F color) {
			this.color = color;
		}

		/// <summary>
		/// A convenience method for setting the skeleton color. The color can also be set by
		/// <see cref="SetColor(Color32F)"/>
		/// </summary>
		public void SetColor (float r, float g, float b, float a) {
			color = new Color32F(r, g, b, a);
		}

		/// <summary><para> Scales the entire skeleton on the X axis.</para>
		/// <para>
		/// Bones that do not inherit scale are still affected by this property.</para></summary>
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		/// <summary><para> Scales the entire skeleton on the Y axis.</para>
		/// <para>
		/// Bones that do not inherit scale are still affected by this property.</para></summary>
		public float ScaleY { get { return scaleY * (Bone.yDown ? -1 : 1); } set { scaleY = value; } }

		/// <summary>
		/// Scales the entire skeleton on the X and Y axes.
		/// <para>
		/// Bones that do not inherit scale are still affected by this property.
		/// </para></summary>
		public void SetScale (float scaleX, float scaleY) {
			this.scaleX = scaleX;
			this.scaleY = scaleY;
		}

		/// <summary><para>The skeleton X position, which is added to the root bone worldX position.</para>
		/// <para>
		/// Bones that do not inherit translation are still affected by this property.</para></summary>
		public float X { get { return x; } set { x = value; } }
		/// <summary><para>The skeleton Y position, which is added to the root bone worldY position.</para>
		/// <para>
		/// Bones that do not inherit translation are still affected by this property.</para></summary>
		public float Y { get { return y; } set { y = value; } }

		/// <summary>
		/// Sets the skeleton X and Y position, which is added to the root bone worldX and worldY position.
		/// <para>
		/// Bones that do not inherit translation are still affected by this property.</para></summary>
		public void SetPosition (float x, float y) {
			this.x = x;
			this.y = y;
		}

		/// <summary>The x component of a vector that defines the direction <see cref="PhysicsConstraintPose.Wind"/> is applied.</summary>
		public float WindX { get { return windX; } set { windX = value; } }
		/// <summary>The y component of a vector that defines the direction <see cref="PhysicsConstraintPose.Wind"/> is applied.</summary>
		public float WindY { get { return windY; } set { windY = value; } }
		/// <summary>The x component of a vector that defines the direction <see cref="PhysicsConstraintPose.Gravity"/> is applied.</summary>
		public float GravityX { get { return gravityX; } set { gravityX = value; } }
		/// <summary>The y component of a vector that defines the direction <see cref="PhysicsConstraintPose.Gravity"/> is applied.</summary>
		public float GravityY { get { return gravityY; } set { gravityY = value; } }

		/// <summary>
		/// Calls <see cref="PhysicsConstraint.Translate(float, float)"/> for each physics constraint.
		/// </summary>
		public void PhysicsTranslate (float x, float y) {
			PhysicsConstraint[] physicsConstraints = this.physics.Items;
			for (int i = 0, n = this.physics.Count; i < n; i++)
				physicsConstraints[i].Translate(x, y);
		}

		/// <summary>
		/// Calls <see cref="PhysicsConstraint.Rotate(float, float, float)"/> for each physics constraint.
		/// </summary>
		public void PhysicsRotate (float x, float y, float degrees) {
			PhysicsConstraint[] physicsConstraints = this.physics.Items;
			for (int i = 0, n = this.physics.Count; i < n; i++)
				physicsConstraints[i].Rotate(x, y, degrees);
		}

		/// <summary>Returns the skeleton's time, used for time-based manipulations, such as <see cref="PhysicsConstraint"/>.</summary>
		/// <seealso cref="Update(float)"/>
		public float Time { get { return time; } set { time = value; } }

		/// <summary>Increments the skeleton's <see cref="time"/>.</summary>
		public void Update (float delta) {
			time += delta;
		}

		override public string ToString () {
			return data.name;
		}
	}
}
