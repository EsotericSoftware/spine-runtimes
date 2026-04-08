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

using System;

namespace Spine {

	/// <summary>
	/// Stores a pose for a physics constraint.
	/// </summary>
	public class PhysicsConstraintPose : IPose<PhysicsConstraintPose> {
		internal float inertia, strength, damping, massInverse, wind, gravity, mix;

		public void Set (PhysicsConstraintPose pose) {
			inertia = pose.inertia;
			strength = pose.strength;
			damping = pose.damping;
			massInverse = pose.massInverse;
			wind = pose.wind;
			gravity = pose.gravity;
			mix = pose.mix;
		}

		/// <summary>Controls how much bone movement is converted into physics movement.</summary>
		public float Inertia { get { return inertia; } set { inertia = value; } }
		/// <summary>The amount of force used to return properties to the unconstrained value.</summary>
		public float Strength { get { return strength; } set { strength = value; } }
		/// <summary>Reduces the speed of physics movements, with more of a reduction at higher speeds.</summary>
		public float Damping { get { return damping; } set { damping = value; } }
		/// <summary>Determines susceptibility to acceleration.</summary>
		public float MassInverse { get { return massInverse; } set { massInverse = value; } }
		/// <summary>Applies a constant force along the <see cref="Skeleton.WindX"/>, <see cref="Skeleton.WindY"/> vector.</summary>
		public float Wind { get { return wind; } set { wind = value; } }
		/// <summary>Applies a constant force along the <see cref="Skeleton.GravityX"/>, <see cref="Skeleton.GravityY"/> vector.</summary>
		public float Gravity { get { return gravity; } set { gravity = value; } }
		/// <summary>A percentage (0+) that controls the mix between the constrained and unconstrained poses.</summary>
		public float Mix { get { return mix; } set { mix = value; } }
	}
}
