/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 *
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.attachments {
import spine.Bone;

public dynamic class BoundingBoxAttachment extends Attachment {
	public var vertices:Vector.<Number> = new Vector.<Number>();

	public function BoundingBoxAttachment (name:String) {
		super(name);
	}

	public function computeWorldVertices (x:Number, y:Number, bone:Bone, worldVertices:Vector.<Number>, worldVerticesOffset:int = 0) : void {
		x += bone.worldX;
		y += bone.worldY;
		var m00:Number = bone.m00;
		var m01:Number = bone.m01;
		var m10:Number = bone.m10;
		var m11:Number = bone.m11;
		var vertices:Vector.<Number> = this.vertices;
		for (var i:int = 0, n:int = vertices.length; i < n; i += 2) {
			var ii:int = i + 1;
			var px:Number = vertices[i];
			var py:Number = vertices[ii];
			worldVertices[worldVerticesOffset + i] = px * m00 + py * m01 + x;
			worldVertices[worldVerticesOffset + ii] = px * m10 + py * m11 + y;
		}
	}
}

}
