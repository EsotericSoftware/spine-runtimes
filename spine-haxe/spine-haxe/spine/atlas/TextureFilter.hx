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

package spine.atlas;

class TextureFilter {
	public static var nearest(default, never):TextureFilter = new TextureFilter(0, "nearest");
	public static var linear(default, never):TextureFilter = new TextureFilter(1, "linear");
	public static var mipMap(default, never):TextureFilter = new TextureFilter(2, "mipMap");
	public static var mipMapNearestNearest(default, never):TextureFilter = new TextureFilter(3, "mipMapNearestNearest");
	public static var mipMapLinearNearest(default, never):TextureFilter = new TextureFilter(4, "mipMapLinearNearest");
	public static var mipMapNearestLinear(default, never):TextureFilter = new TextureFilter(5, "mipMapNearestLinear");
	public static var mipMapLinearLinear(default, never):TextureFilter = new TextureFilter(6, "mipMapLinearLinear");

	public static var values(default, never):Array<TextureFilter> = [
		nearest,
		linear,
		mipMap,
		mipMapNearestNearest,
		mipMapLinearNearest,
		mipMapNearestLinear,
		mipMapLinearLinear
	];

	public var ordinal(default, null):Int;
	public var name(default, null):String;

	public function new(ordinal:Int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}

	public static function fromName(name:String):TextureFilter {
		for (value in values) {
			if (value.name == name.toLowerCase())
				return value;
		}
		return null;
	}
}
