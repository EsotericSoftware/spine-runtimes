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

package spine;

class PhysicsConstraintData extends ConstraintData {
	public var bone:BoneData;
	public var x:Float = 0;
	public var y:Float = 0;
	public var rotate:Float = 0;
	public var scaleX:Float = 0;
	public var shearX:Float = 0;
	public var limit:Float = 0;
	public var step:Float = 0;
	public var inertia:Float = 0;
	public var strength:Float = 0;
	public var damping:Float = 0;
	public var massInverse:Float = 0;
	public var wind:Float = 0;
	public var gravity:Float = 0;
	public var mix:Float = 0;
	public var inertiaGlobal:Bool = false;
	public var strengthGlobal:Bool = false;
	public var dampingGlobal:Bool = false;
	public var massGlobal:Bool = false;
	public var windGlobal:Bool = false;
	public var gravityGlobal:Bool = false;
	public var mixGlobal:Bool = false;
	
	public function new(name:String) {
		super(name, 0, false);
	}

}
