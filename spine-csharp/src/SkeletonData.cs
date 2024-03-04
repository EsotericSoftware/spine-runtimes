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

	/// <summary>Stores the setup pose and all of the stateless data for a skeleton.</summary>
	public class SkeletonData {
		internal string name;
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>(); // Ordered parents first
		internal ExposedList<SlotData> slots = new ExposedList<SlotData>(); // Setup pose draw order.
		internal ExposedList<Skin> skins = new ExposedList<Skin>();
		internal Skin defaultSkin;
		internal ExposedList<EventData> events = new ExposedList<EventData>();
		internal ExposedList<Animation> animations = new ExposedList<Animation>();
		internal ExposedList<IkConstraintData> ikConstraints = new ExposedList<IkConstraintData>();
		internal ExposedList<TransformConstraintData> transformConstraints = new ExposedList<TransformConstraintData>();
		internal ExposedList<PathConstraintData> pathConstraints = new ExposedList<PathConstraintData>();
		internal ExposedList<PhysicsConstraintData> physicsConstraints = new ExposedList<PhysicsConstraintData>();
		internal float x, y, width, height, referenceScale = 100;
		internal string version, hash;

		// Nonessential.
		internal float fps;
		internal string imagesPath, audioPath;

		/// <summary>The skeleton's name, which by default is the name of the skeleton data file when possible, or null when a name hasn't been
		/// set.</summary>
		public string Name { get { return name; } set { name = value; } }

		/// <summary>The skeleton's bones, sorted parent first. The root bone is always the first bone.</summary>
		public ExposedList<BoneData> Bones { get { return bones; } }

		/// <summary>The skeleton's slots in the setup pose draw order.</summary>
		public ExposedList<SlotData> Slots { get { return slots; } }

		/// <summary>All skins, including the default skin.</summary>
		public ExposedList<Skin> Skins { get { return skins; } set { skins = value; } }

		/// <summary>
		/// The skeleton's default skin.
		/// By default this skin contains all attachments that were not in a skin in Spine.
		/// </summary>
		/// <return>May be null.</return>
		public Skin DefaultSkin { get { return defaultSkin; } set { defaultSkin = value; } }

		/// <summary>The skeleton's events.</summary>
		public ExposedList<EventData> Events { get { return events; } set { events = value; } }
		/// <summary>The skeleton's animations.</summary>
		public ExposedList<Animation> Animations { get { return animations; } set { animations = value; } }
		/// <summary>The skeleton's IK constraints.</summary>
		public ExposedList<IkConstraintData> IkConstraints { get { return ikConstraints; } set { ikConstraints = value; } }
		/// <summary>The skeleton's transform constraints.</summary>
		public ExposedList<TransformConstraintData> TransformConstraints { get { return transformConstraints; } set { transformConstraints = value; } }
		/// <summary>The skeleton's path constraints.</summary>
		public ExposedList<PathConstraintData> PathConstraints { get { return pathConstraints; } set { pathConstraints = value; } }
		/// <summary>The skeleton's physics constraints.</summary>
		public ExposedList<PhysicsConstraintData> PhysicsConstraints { get { return physicsConstraints; } set { physicsConstraints = value; } }

		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Width { get { return width; } set { width = value; } }
		public float Height { get { return height; } set { height = value; } }

		/// <summary> Baseline scale factor for applying distance-dependent effects on non-scalable properties, such as angle or scale. Default
		/// is 100.</summary>
		public float ReferenceScale { get { return referenceScale; } set { referenceScale = value; } }

		/// <summary>The Spine version used to export this data, or null.</summary>
		public string Version { get { return version; } set { version = value; } }

		/// <summary>The skeleton data hash. This value will change if any of the skeleton data has changed.
		/// May be null.</summary>
		public string Hash { get { return hash; } set { hash = value; } }

		public string ImagesPath { get { return imagesPath; } set { imagesPath = value; } }

		/// <summary> The path to the audio directory as defined in Spine. Available only when nonessential data was exported.
		/// May be null.</summary>
		public string AudioPath { get { return audioPath; } set { audioPath = value; } }

		/// <summary>The dopesheet FPS in Spine, or zero if nonessential data was not exported.</summary>
		public float Fps { get { return fps; } set { fps = value; } }

		// --- Bones

		/// <summary>
		/// Finds a bone by comparing each bone's name.
		/// It is more efficient to cache the results of this method than to call it multiple times.</summary>
		/// <returns>May be null.</returns>
		public BoneData FindBone (string boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName", "boneName cannot be null.");
			BoneData[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				BoneData bone = bones[i];
				if (bone.name == boneName) return bone;
			}
			return null;
		}

		// --- Slots

		/// <returns>May be null.</returns>
		public SlotData FindSlot (string slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName", "slotName cannot be null.");
			SlotData[] slots = this.slots.Items;
			for (int i = 0, n = this.slots.Count; i < n; i++) {
				SlotData slot = slots[i];
				if (slot.name == slotName) return slot;
			}
			return null;
		}

		// --- Skins

		/// <returns>May be null.</returns>
		public Skin FindSkin (string skinName) {
			if (skinName == null) throw new ArgumentNullException("skinName", "skinName cannot be null.");
			foreach (Skin skin in skins)
				if (skin.name == skinName) return skin;
			return null;
		}

		// --- Events

		/// <returns>May be null.</returns>
		public EventData FindEvent (string eventDataName) {
			if (eventDataName == null) throw new ArgumentNullException("eventDataName", "eventDataName cannot be null.");
			foreach (EventData eventData in events)
				if (eventData.name == eventDataName) return eventData;
			return null;
		}

		// --- Animations

		/// <returns>May be null.</returns>
		public Animation FindAnimation (string animationName) {
			if (animationName == null) throw new ArgumentNullException("animationName", "animationName cannot be null.");
			Animation[] animations = this.animations.Items;
			for (int i = 0, n = this.animations.Count; i < n; i++) {
				Animation animation = animations[i];
				if (animation.name == animationName) return animation;
			}
			return null;
		}

		// --- IK constraints

		/// <returns>May be null.</returns>
		public IkConstraintData FindIkConstraint (string constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			IkConstraintData[] ikConstraints = this.ikConstraints.Items;
			for (int i = 0, n = this.ikConstraints.Count; i < n; i++) {
				IkConstraintData ikConstraint = ikConstraints[i];
				if (ikConstraint.name == constraintName) return ikConstraint;
			}
			return null;
		}

		// --- Transform constraints

		/// <returns>May be null.</returns>
		public TransformConstraintData FindTransformConstraint (string constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			TransformConstraintData[] transformConstraints = this.transformConstraints.Items;
			for (int i = 0, n = this.transformConstraints.Count; i < n; i++) {
				TransformConstraintData transformConstraint = transformConstraints[i];
				if (transformConstraint.name == constraintName) return transformConstraint;
			}
			return null;
		}

		// --- Path constraints

		/// <summary>
		/// Finds a path constraint by comparing each path constraint's name. It is more efficient to cache the results of this method
		/// than to call it multiple times.
		/// </summary>
		/// <returns>May be null.</returns>
		public PathConstraintData FindPathConstraint (string constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			PathConstraintData[] pathConstraints = this.pathConstraints.Items;
			for (int i = 0, n = this.pathConstraints.Count; i < n; i++) {
				PathConstraintData constraint = pathConstraints[i];
				if (constraint.name.Equals(constraintName)) return constraint;
			}
			return null;
		}

		// --- Physics constraints

		/// <summary>
		/// Finds a physics constraint by comparing each physics constraint's name. It is more efficient to cache the results of this
		/// method than to call it multiple times.
		/// </summary>
		/// <returns>May be null.</returns>
		public PhysicsConstraintData FindPhysicsConstraint (String constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			PhysicsConstraintData[] physicsConstraints = this.physicsConstraints.Items;
			for (int i = 0, n = this.physicsConstraints.Count; i < n; i++) {
				PhysicsConstraintData constraint = (PhysicsConstraintData)physicsConstraints[i];
				if (constraint.name.Equals(constraintName)) return constraint;
			}
			return null;
		}

		// ---

		override public string ToString () {
			return name ?? base.ToString();
		}
	}
}
