/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation;

import spine.ArrayUtils;
import spine.Event;
import spine.Skeleton;

/** The base class for all timelines. */
class Timeline {
	/** Uniquely encodes both the type of this timeline and the skeleton properties that it affects. */
	public var propertyIds:Array<String>;
	/** The time in seconds and any other values for each frame. */
	public var frames:Array<Float>;

	/**
	 * @param propertyIds Unique identifiers for the properties the timeline modifies.
	 */
	public function new(frameCount:Int, propertyIds:Array<String>) {
		this.propertyIds = propertyIds;
		frames = ArrayUtils.resize(new Array<Float>(), frameCount * getFrameEntries(), 0);
	}

	/** The number of entries stored per frame. */
	public function getFrameEntries():Int {
		return 1;
	}

	/** The number of frames for this timeline. */
	public function getFrameCount():Int {
		return Std.int(frames.length / getFrameEntries());
	}

	/** Returns the duration of this timeline in seconds. */
	public function getDuration():Float {
		return frames[frames.length - getFrameEntries()];
	}

	/** Applies this timeline to the skeleton.
	 * @param skeleton The skeleton to which the timeline is being applied. This provides access to the bones, slots, and other
	 *           skeleton components that the timeline may change.
	 * @param lastTime The last time in seconds this timeline was applied. Timelines such as spine.animation.EventTimeline trigger only
	 *           at specific times rather than every frame. In that case, the timeline triggers everything between
	 *           lastTime (exclusive) and time (inclusive). Pass -1 the first time an animation is
	 *           applied to ensure frame 0 is triggered.
	 * @param time The time in seconds that the skeleton is being posed for. Most timelines find the frame before and the frame
	 *           after this time and interpolate between the frame values. If beyond the last frame, the last frame will be
	 *           applied.
	 * @param events If any events are fired, they are added to this list. Can be null to ignore fired events or if the timeline
	 *           does not fire events.
	 * @param alpha 0 applies the current or setup value (depending on blend). 1 applies the timeline value.
	 *           Between 0 and 1 applies a value between the current or setup value and the timeline value. By adjusting
	 *           alpha over time, an animation can be mixed in or out. alpha can also be useful to
	 *           apply animations on top of each other (layering).
	 * @param blend Controls how mixing is applied when alpha < 1.
	 * @param direction Indicates whether the timeline is mixing in or out. Used by timelines which perform instant transitions,
	 *           such as spine.animation.DrawOrderTimeline or spine.animation.AttachmentTimeline, and others such as spine.animation.ScaleTimeline.
	 */
	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, blend:MixBlend, direction:MixDirection):Void {
		throw new SpineException("Timeline implementations must override apply()");
	}

	/** Linear search using a stride of 1.
	 * @param time Must be >= the first value in frames.
	 * @return The index of the first value <= time.
	 */
	public static function search1(frames:Array<Float>, time:Float):Int {
		var n:Int = frames.length;
		for (i in 1...n) {
			if (frames[i] > time)
				return i - 1;
		}
		return n - 1;
	}

	/** Linear search using the specified stride.
	 * @param time Must be >= the first value in frames.
	 * @return The index of the first value <= time.
	 */
	public static function search(values:Array<Float>, time:Float, step:Int):Int {
		var n:Int = values.length;
		var i:Int = step;
		while (i < n) {
			if (values[i] > time)
				return i - step;
			i += step;
		}
		return n - step;
	}

	public function toString():String {
		return "Timeline " + Type.getClassName(Type.getClass(this));
	}
}
