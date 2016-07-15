/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {

public class BoneData {
	internal var _index:int;
	internal var _name:String;
	internal var _parent:BoneData;
	public var length:Number;
	public var x:Number;
	public var y:Number;
	public var rotation:Number;
	public var scaleX:Number = 1;
	public var scaleY:Number = 1;
	public var shearX:Number;
	public var shearY:Number;	
	public var inheritRotation:Boolean = true;
	public var inheritScale:Boolean = true;

	/** @param parent May be null. */
	public function BoneData (index:int, name:String, parent:BoneData) {
		if (index < 0) throw new ArgumentError("index must be >= 0");
		if (name == null) throw new ArgumentError("name cannot be null.");
		_index = index;
		_name = name;
		_parent = parent;
	}
	
	public function get index () : int {
		return _index;
	}

	public function get name () : String {
		return _name;
	}

	/** @return May be null. */
	public function get parent () : BoneData {
		return _parent;
	}

	public function toString () : String {
		return _name;
	}
}

}
