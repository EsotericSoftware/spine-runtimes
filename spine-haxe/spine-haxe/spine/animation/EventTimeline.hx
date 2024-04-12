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

package spine.animation;

import spine.animation.Timeline;
import spine.Event;
import spine.Skeleton;

class EventTimeline extends Timeline {
	public var events:Array<Event>;

	public function new(frameCount:Int) {
		super(frameCount, [Std.string(Property.event)]);
		events = new Array<Event>();
		events.resize(frameCount);
	}

	public override function getFrameCount():Int {
		return frames.length;
	}

	/** Sets the time in seconds and the event for the specified key frame. */
	public function setFrame(frame:Int, event:Event):Void {
		frames[frame] = event.time;
		events[frame] = event;
	}

	/** Fires events for frames > `lastTime` and <= `time`. */
	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		if (events == null)
			return;

		var frameCount:Int = frames.length;

		if (lastTime > time) // Apply events after lastTime for looped animations.
		{
			apply(skeleton, lastTime, 2147483647, events, alpha, blend, direction);
			lastTime = -1;
		} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
		{
			return;
		}

		if (time < frames[0]) return;

		var frame:Int;
		var i:Int = 0;
		if (lastTime >= frames[0]) {
			i = Timeline.search1(frames, lastTime) + 1;
			var frameTime:Float = frames[i];
			while (i > 0) // Fire multiple events with the same frame.
			{
				if (frames[i - 1] != frameTime)
					break;
				i--;
			}
		}
		while (i < frameCount && time >= frames[i]) {
			events.push(this.events[i]);
			i++;
		}
	}
}
