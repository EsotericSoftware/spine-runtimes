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

class BoneData {
	private var _index:Int;
	private var _name:String;
	private var _parent:BoneData;

	public var length:Float = 0;
	public var x:Float = 0;
	public var y:Float = 0;
	public var rotation:Float = 0;
	public var scaleX:Float = 1;
	public var scaleY:Float = 1;
	public var shearX:Float = 0;
	public var shearY:Float = 0;
	public var inherit:Inherit = Inherit.normal;
	public var skinRequired:Bool = false;
	public var color:Color = new Color(0, 0, 0, 0);
	public var icon:String;
	public var visible:Bool = false;

	/** @param parent May be null. */
	public function new(index:Int, name:String, parent:BoneData) {
		if (index < 0)
			throw new SpineException("index must be >= 0");
		if (name == null)
			throw new SpineException("name cannot be null.");
		_index = index;
		_name = name;
		_parent = parent;
	}

	public var index(get, never):Int;

	private function get_index():Int {
		return _index;
	}

	public var name(get, never):String;

	function get_name():String {
		return _name;
	}

	/** @return May be null. */
	public var parent(get, never):BoneData;

	private function get_parent():BoneData {
		return _parent;
	}

	public function toString():String {
		return _name;
	}
}
