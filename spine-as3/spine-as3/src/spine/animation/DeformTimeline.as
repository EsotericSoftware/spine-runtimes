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

package spine.animation {
import spine.attachments.VertexAttachment;
import spine.Event;
import spine.Skeleton;
import spine.Slot;

public class DeformTimeline extends CurveTimeline {
	public var slotIndex:int;
	public var frames:Vector.<Number>;
	public var frameVertices:Vector.<Vector.<Number>>;
	public var attachment:VertexAttachment;

	public function DeformTimeline (frameCount:int) {
		super(frameCount);
		frames = new Vector.<Number>(frameCount, true);
		frameVertices = new Vector.<Vector.<Number>>(frameCount, true);
	}

	/** Sets the time and value of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, vertices:Vector.<Number>) : void {
		frames[frameIndex] = time;
		frameVertices[frameIndex] = vertices;
	}

	override public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
		var slot:Slot = skeleton.slots[slotIndex];
		var slotAttachment:VertexAttachment = slot.attachment as VertexAttachment;
		if (!slotAttachment || !slotAttachment.applyDeform(attachment)) return;

		var frames:Vector.<Number> = this.frames;
		if (time < frames[0]) return; // Time is before first frame.

		var frameVertices:Vector.<Vector.<Number>> = this.frameVertices;
		var vertexCount:int = frameVertices[0].length;

		var vertices:Vector.<Number> = slot.attachmentVertices;
		if (vertices.length != vertexCount) alpha = 1; // Don't mix from uninitialized slot vertices.
		vertices.length = vertexCount;

		var i:int;
		if (time >= frames[frames.length - 1]) { // Time is after last frame.
			var lastVertices:Vector.<Number> = frameVertices[int(frames.length - 1)];
			if (alpha < 1) {
				for (i = 0; i < vertexCount; i++)
					vertices[i] += (lastVertices[i] - vertices[i]) * alpha;
			} else {
				for (i = 0; i < vertexCount; i++)
					vertices[i] = lastVertices[i];
			}
			return;
		}

		// Interpolate between the previous frame and the current frame.
		var frame:int = Animation.binarySearch1(frames, time);
		var prevVertices:Vector.<Number> = frameVertices[int(frame - 1)];
		var nextVertices:Vector.<Number> = frameVertices[frame];
		var frameTime:Number = frames[frame];		
		var percent:Number = getCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime));		

		var prev:Number;
		if (alpha < 1) {
			for (i = 0; i < vertexCount; i++) {
				prev = prevVertices[i];
				vertices[i] += (prev + (nextVertices[i] - prev) * percent - vertices[i]) * alpha;
			}
		} else {
			for (i = 0; i < vertexCount; i++) {
				prev = prevVertices[i];
				vertices[i] = prev + (nextVertices[i] - prev) * percent;
			}
		}
	}
}

}
