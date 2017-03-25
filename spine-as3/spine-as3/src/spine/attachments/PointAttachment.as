/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.attachments {
	import spine.Color;
	import spine.MathUtils;
	import spine.Bone;

	public dynamic class PointAttachment extends VertexAttachment {
		public var x : Number, y : Number, rotation : Number;
		public var color : Color = new Color(0.38, 0.94, 0, 1);

		public function PointAttachment(name : String) {
			super(name);
		}

		public function computeWorldPosition(bone : Bone, point : Vector.<Number>) : Vector.<Number> {
			point[0] = this.x * bone.a + this.y * bone.b + bone.worldX;
			point[1] = this.x * bone.c + this.y * bone.d + bone.worldY;
			return point;
		}

		public function computeWorldRotation(bone : Bone) : Number {
			var cos : Number = MathUtils.cosDeg(this.rotation), sin : Number = MathUtils.sinDeg(this.rotation);
			var x : Number = cos * bone.a + sin * bone.b;
			var y : Number = cos * bone.c + sin * bone.d;
			return Math.atan2(y, x) * MathUtils.radDeg;
		}
	}
}