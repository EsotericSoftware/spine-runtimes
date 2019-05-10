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
		
		override public function copy (): Attachment {
			var copy : PointAttachment = new PointAttachment(name);
			copy.x = x;
			copy.y = y;
			copy.rotation = rotation;
			copy.color.setFromColor(color);
			return copy;
		}
	}
}
