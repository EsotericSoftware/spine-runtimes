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

namespace Spine {
	/// <summary>
	/// Stores the setup pose for a <see cref="PhysicsConstraint"/>.
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class PhysicsConstraintData : ConstraintData<PhysicsConstraint, PhysicsConstraintPose> {
		internal BoneData bone;
		internal float x, y, rotate, scaleX, shearX, limit, step;
		internal bool inertiaGlobal, strengthGlobal, dampingGlobal, massGlobal, windGlobal, gravityGlobal, mixGlobal;

		public PhysicsConstraintData (string name)
			: base(name, new PhysicsConstraintPose()) {
		}

		override public IConstraint Create (Skeleton skeleton) {
			return new PhysicsConstraint(this, skeleton);
		}

		/// <summary>The bone constrained by this physics constraint.</summary>
		public BoneData Bone { get { return bone; } }

		/// <summary>The time in milliseconds required to advance the physics simulation one step.</summary>
		public float Step { get { return step; } set { step = value; } }
		/// <summary>Physics influence on x translation, 0-1.</summary>
		public float X { get { return x; } set { x = value; } }
		/// <summary>Physics influence on y translation, 0-1.</summary>
		public float Y { get { return y; } set { y = value; } }
		/// <summary>Physics influence on rotation, 0-1.</summary>
		public float Rotate { get { return rotate; } set { rotate = value; } }
		/// <summary>Physics influence on scaleX, 0-1.</summary>
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		/// <summary>Physics influence on shearX, 0-1.</summary>
		public float ShearX { get { return shearX; } set { shearX = value; } }
		/// <summary>Movement greater than the limit will not have a greater effect on physics.</summary>
		public float Limit { get { return limit; } set { limit = value; } }
		/// <summary>True when this constraint's inertia is controlled by global slider timelines.</summary>
		public bool InertiaGlobal { get { return inertiaGlobal; } set { inertiaGlobal = value; } }
		/// <summary>True when this constraint's strength is controlled by global slider timelines.</summary>
		public bool StrengthGlobal { get { return strengthGlobal; } set { strengthGlobal = value; } }
		/// <summary>True when this constraint's damping is controlled by global slider timelines.</summary>
		public bool DampingGlobal { get { return dampingGlobal; } set { dampingGlobal = value; } }
		/// <summary>True when this constraint's mass is controlled by global slider timelines.</summary>
		public bool MassGlobal { get { return massGlobal; } set { massGlobal = value; } }
		/// <summary>True when this constraint's wind is controlled by global slider timelines.</summary>
		public bool WindGlobal { get { return windGlobal; } set { windGlobal = value; } }
		/// <summary>True when this constraint's gravity is controlled by global slider timelines.</summary>
		public bool GravityGlobal { get { return gravityGlobal; } set { gravityGlobal = value; } }
		/// <summary>True when this constraint's mix is controlled by global slider timelines.</summary>
		public bool MixGlobal { get { return mixGlobal; } set { mixGlobal = value; } }
	}
}
