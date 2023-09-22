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

class SequenceMode {
	public static var hold(default, never):SequenceMode = new SequenceMode("hold", 0);
	public static var once(default, never):SequenceMode = new SequenceMode("once", 1);
	public static var loop(default, never):SequenceMode = new SequenceMode("loop", 2);
	public static var pingpong(default, never):SequenceMode = new SequenceMode("pingpong", 3);
	public static var onceReverse(default, never):SequenceMode = new SequenceMode("onceReverse", 4);
	public static var loopReverse(default, never):SequenceMode = new SequenceMode("loopReverse", 5);
	public static var pingpongReverse(default, never):SequenceMode = new SequenceMode("pingpongReverse", 6);

	public static var values(default, never):Array<SequenceMode> = [hold, once, loop, pingpong, onceReverse, loopReverse, pingpongReverse];

	public var name(default, null):String;
	public var value:Int;

	public function new(name:String, value:Int) {
		this.name = name;
		this.value = value;
	}

	public static function fromName(name:String):SequenceMode {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
