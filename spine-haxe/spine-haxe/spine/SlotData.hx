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

class SlotData {
	private var _index:Int;
	private var _name:String;
	private var _boneData:BoneData;

	public var color:Color = new Color(1, 1, 1, 1);
	public var darkColor:Color = null;
	public var attachmentName:String;
	public var blendMode:BlendMode = BlendMode.normal;
	public var visible:Bool = true;

	public function new(index:Int, name:String, boneData:BoneData) {
		if (index < 0)
			throw new SpineException("index must be >= 0.");
		if (name == null)
			throw new SpineException("name cannot be null.");
		if (boneData == null)
			throw new SpineException("boneData cannot be null.");
		_index = index;
		_name = name;
		_boneData = boneData;
	}

	public var index(get, never):Int;

	private function get_index():Int {
		return _index;
	}

	public var name(get, never):String;

	private function get_name():String {
		return _name;
	}

	public var boneData(get, never):BoneData;

	private function get_boneData():BoneData {
		return _boneData;
	}

	public function toString():String {
		return _name;
	}
}
