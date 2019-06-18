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

package spine {
	public class BoneData {
		internal var _index : int;
		internal var _name : String;
		internal var _parent : BoneData;
		public var length : Number;
		public var x : Number;
		public var y : Number;
		public var rotation : Number;
		public var scaleX : Number = 1;
		public var scaleY : Number = 1;
		public var shearX : Number;
		public var shearY : Number;
		public var transformMode : TransformMode = TransformMode.normal;
		public var skinRequired : Boolean;
		public var color : Color = new Color(0, 0, 0, 0);

		/** @param parent May be null. */
		public function BoneData(index : int, name : String, parent : BoneData) {
			if (index < 0) throw new ArgumentError("index must be >= 0");
			if (name == null) throw new ArgumentError("name cannot be null.");
			_index = index;
			_name = name;
			_parent = parent;
		}

		public function get index() : int {
			return _index;
		}

		public function get name() : String {
			return _name;
		}

		/** @return May be null. */
		public function get parent() : BoneData {
			return _parent;
		}

		public function toString() : String {
			return _name;
		}
	}
}
