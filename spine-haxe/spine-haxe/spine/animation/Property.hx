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

package spine.animation;

class Property {
	public static inline var rotate:Int = 0;
	public static inline var x:Int = 1;
	public static inline var y:Int = 2;
	public static inline var scaleX:Int = 3;
	public static inline var scaleY:Int = 4;
	public static inline var shearX:Int = 5;
	public static inline var shearY:Int = 6;
	public static inline var inherit:Int = 7;

	public static inline var rgb:Int = 8;
	public static inline var alpha:Int = 9;
	public static inline var rgb2:Int = 10;

	public static inline var attachment:Int = 11;
	public static inline var deform:Int = 12;

	public static inline var event:Int = 13;
	public static inline var drawOrder:Int = 14;

	public static inline var ikConstraint:Int = 15;
	public static inline var transformConstraint:Int = 16;

	public static inline var pathConstraintPosition:Int = 17;
	public static inline var pathConstraintSpacing:Int = 18;
	public static inline var pathConstraintMix:Int = 19;

	public static inline var physicsConstraintInertia:Int = 20;
	public static inline var physicsConstraintStrength:Int = 21;
	public static inline var physicsConstraintDamping:Int = 22;
	public static inline var physicsConstraintMass:Int = 23;
	public static inline var physicsConstraintWind:Int = 24;
	public static inline var physicsConstraintGravity:Int = 25;
	public static inline var physicsConstraintMix:Int = 26;
	public static inline var physicsConstraintReset:Int = 27;

	public static inline var sequence:Int = 28;

	public function new() {}
}
