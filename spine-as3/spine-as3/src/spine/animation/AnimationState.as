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

package spine.animation {
import spine.MathUtils;
import spine.Bone;
import spine.Pool;
import flash.utils.Dictionary;
import spine.Event;
import spine.Skeleton;

public class AnimationState {
	internal static var emptyAnimation:Animation = new Animation("<empty>", new Vector.<Timeline>(), 0);
	
	public var data:AnimationStateData;
	public var tracks:Vector.<TrackEntry> = new Vector.<TrackEntry>();
	internal var events:Vector.<Event> = new Vector.<Event>();
	public var onStart:Listeners = new Listeners();
	public var onInterrupt:Listeners = new Listeners();
	public var onEnd:Listeners = new Listeners();
	public var onDispose:Listeners = new Listeners();
	public var onComplete:Listeners = new Listeners();
	public var onEvent:Listeners = new Listeners();
	internal var queue:EventQueue;
	internal var propertyIDs:Dictionary = new Dictionary();
	internal var animationsChanged:Boolean;
	public var timeScale:Number = 1;
	internal var trackEntryPool:Pool;
	
	public function AnimationState(data:AnimationStateData) {
		if (data == null) throw new ArgumentError("data can not be null");
		this.data = data;
		this.queue = new EventQueue(this);
		this.trackEntryPool = new Pool(function():Object {
			return new TrackEntry();
		});
	}
	
	public function update (delta:Number):void {
		delta *= timeScale;
		for (var i:int = 0, n:int = tracks.length; i < n; i++) {
			var current:TrackEntry = tracks[i];
			if (current == null) continue;

			current.animationLast = current.nextAnimationLast;
			current.trackLast = current.nextTrackLast;

			var currentDelta:Number = delta * current.timeScale;

			if (current.delay > 0) {
				current.delay -= currentDelta;
				if (current.delay > 0) continue;
				currentDelta = -current.delay;
				current.delay = 0;
			}

			var next:TrackEntry = current.next;
			if (next != null) {
				// When the next entry's delay is passed, change to the next entry, preserving leftover time.
				var nextTime:Number = current.trackLast - next.delay;
				if (nextTime >= 0) {
					next.delay = 0;
					next.trackTime = nextTime + delta * next.timeScale;
					current.trackTime += currentDelta;
					setCurrent(i, next);
					while (next.mixingFrom != null) {
						next.mixTime += currentDelta;
						next = next.mixingFrom;
					}
					continue;
				}
				updateMixingFrom(current, delta, true);
			} else {
				updateMixingFrom(current, delta, true);
				// Clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
				if (current.trackLast >= current.trackEnd && current.mixingFrom == null) {
					tracks[i] = null;
					queue.end(current);
					disposeNext(current);
					continue;
				}
			}

			current.trackTime += currentDelta;
		}

		queue.drain();
	}
	
	private function updateMixingFrom (entry:TrackEntry, delta:Number, canEnd:Boolean):void {
		var from:TrackEntry = entry.mixingFrom;
		if (from == null) return;

		if (canEnd && entry.mixTime >= entry.mixDuration && entry.mixTime > 0) {
			queue.end(from);
			var newFrom:TrackEntry = from.mixingFrom;
			entry.mixingFrom = newFrom;
			if (newFrom == null) return;
			entry.mixTime = from.mixTime;
			entry.mixDuration = from.mixDuration;
			from = newFrom;
		}

		from.animationLast = from.nextAnimationLast;
		from.trackLast = from.nextTrackLast;
		var mixingFromDelta:Number = delta * from.timeScale;
		from.trackTime += mixingFromDelta;
		entry.mixTime += mixingFromDelta;

		updateMixingFrom(from, delta, canEnd && from.alpha == 1);
	}
	
	public function apply (skeleton:Skeleton):void {
		if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");
		if (animationsChanged) _animationsChanged();

		var events:Vector.<Event> = this.events;

		for (var i:int = 0, n:int = tracks.length; i < n; i++) {
			var current:TrackEntry = tracks[i];
			if (current == null || current.delay > 0) continue;

			// Apply mixing from entries first.
			var mix:Number = current.alpha;
			if (current.mixingFrom != null) mix *= applyMixingFrom(current, skeleton);

			// Apply current entry.
			var animationLast:Number = current.animationLast, animationTime:Number = current.getAnimationTime();
			var timelineCount:int = current.animation.timelines.length;
			var timelines:Vector.<Timeline> = current.animation.timelines;
			var ii:int = 0;
			if (mix == 1) {
				for (ii = 0; ii < timelineCount; ii++)
					Timeline(timelines[ii]).apply(skeleton, animationLast, animationTime, events, 1, true, false);
			} else {
				var firstFrame:Boolean = current.timelinesRotation.length == 0;
				if (firstFrame) current.timelinesRotation.length = timelineCount << 1;
				var timelinesRotation:Vector.<Number> = current.timelinesRotation;

				var timelinesFirst:Vector.<Boolean> = current.timelinesFirst;
				for (ii = 0; ii < timelineCount; ii++) {
					var timeline:Timeline = timelines[ii];
					if (timeline is RotateTimeline) {
						applyRotateTimeline(timeline, skeleton, animationTime, mix, timelinesFirst[ii], timelinesRotation, ii << 1,
							firstFrame);
					} else
						timeline.apply(skeleton, animationLast, animationTime, events, mix, timelinesFirst[ii], false);
				}
			}
			queueEvents(current, animationTime);
			current.nextAnimationLast = animationTime;
			current.nextTrackLast = current.trackTime;
		}

		queue.drain();
	}
	
	private function applyMixingFrom (entry:TrackEntry, skeleton:Skeleton):Number {
		var from:TrackEntry = entry.mixingFrom;
		if (from.mixingFrom != null) applyMixingFrom(from, skeleton);

		var mix:Number = 0;
		if (entry.mixDuration == 0) // Single frame mix to undo mixingFrom changes.
			mix = 1;
		else {
			mix = entry.mixTime / entry.mixDuration;
			if (mix > 1) mix = 1;			
		}

		var events:Vector.<Event> = mix < from.eventThreshold ? this.events : null;
		var attachments:Boolean = mix < from.attachmentThreshold, drawOrder:Boolean = mix < from.drawOrderThreshold;
		var animationLast:Number = from.animationLast, animationTime:Number = from.getAnimationTime();
		var timelineCount:int = from.animation.timelines.length;
		var timelines:Vector.<Timeline> = from.animation.timelines;
		var timelinesFirst:Vector.<Boolean> = from.timelinesFirst;
		var alpha:Number = from.alpha * entry.mixAlpha * (1 - mix);

		var firstFrame:Boolean = from.timelinesRotation.length == 0;
		if (firstFrame) from.timelinesRotation.length = timelineCount << 1;
		var timelinesRotation:Vector.<Number> = from.timelinesRotation;

		for (var i:int = 0; i < timelineCount; i++) {
			var timeline:Timeline = timelines[i];
			var setupPose:Boolean = timelinesFirst[i];
			if (timeline is RotateTimeline)
				applyRotateTimeline(timeline, skeleton, animationTime, alpha, setupPose, timelinesRotation, i << 1, firstFrame);
			else {
				if (!setupPose) {
					if (!attachments && timeline is AttachmentTimeline) continue;
					if (!drawOrder && timeline is DrawOrderTimeline) continue;
				}
				timeline.apply(skeleton, animationLast, animationTime, events, alpha, setupPose, true);
			}
		}

		queueEvents(from, animationTime);
		from.nextAnimationLast = animationTime;
		from.nextTrackLast = from.trackTime;

		return mix;
	}
	
	private function applyRotateTimeline (timeline:Timeline, skeleton:Skeleton, time:Number, alpha:Number, setupPose:Boolean,
		timelinesRotation:Vector.<Number>, i:int, firstFrame:Boolean):void {
		if (alpha == 1) {
			timeline.apply(skeleton, 0, time, null, 1, setupPose, false);
			return;
		}

		var rotateTimeline:RotateTimeline = RotateTimeline(timeline);
		var frames:Vector.<Number> = rotateTimeline.frames;		
		var bone:Bone = skeleton.bones[rotateTimeline.boneIndex];
		if (time < frames[0]) {
			if (setupPose) bone.rotation = bone.data.rotation;
			return;
		}

		var r2:Number;
		if (time >= frames[frames.length - RotateTimeline.ENTRIES]) // Time is after last frame.
			r2 = bone.data.rotation + frames[frames.length + RotateTimeline.PREV_ROTATION];
		else {
			// Interpolate between the previous frame and the current frame.
			var frame:int = Animation.binarySearch(frames, time, RotateTimeline.ENTRIES);
			var prevRotation:Number = frames[frame + RotateTimeline.PREV_ROTATION];
			var frameTime:Number = frames[frame];
			var percent:Number = rotateTimeline.getCurvePercent((frame >> 1) - 1,
				1 - (time - frameTime) / (frames[frame + RotateTimeline.PREV_TIME] - frameTime));

			r2 = frames[frame + RotateTimeline.ROTATION] - prevRotation;
			r2 -= (16384 - int((16384.499999999996 - r2 / 360))) * 360;
			r2 = prevRotation + r2 * percent + bone.data.rotation;
			r2 -= (16384 - int((16384.499999999996 - r2 / 360))) * 360;
		}

		// Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
		var r1:Number = setupPose ? bone.data.rotation : bone.rotation;
		var total:Number, diff:Number = r2 - r1;
		if (diff == 0) {
			if (firstFrame) {
				timelinesRotation[i] = 0;
				total = 0;
			} else
				total = timelinesRotation[i];
		} else {
			diff -= (16384 - int((16384.499999999996 - diff / 360))) * 360;
			var lastTotal:Number, lastDiff:Number;
			if (firstFrame) {
				lastTotal = 0;
				lastDiff = diff;
			} else {
				lastTotal = timelinesRotation[i]; // Angle and direction of mix, including loops.
				lastDiff = timelinesRotation[i + 1]; // Difference between bones.
			}
			var current:Boolean = diff > 0, dir:Boolean = lastTotal >= 0;
			// Detect cross at 0 (not 180).			
			if (MathUtils.signum(lastDiff) != MathUtils.signum(diff) && Math.abs(lastDiff) <= 90) {
				// A cross after a 360 rotation is a loop.
				if (Math.abs(lastTotal) > 180) lastTotal += 360 * MathUtils.signum(lastTotal);
				dir = current;
			}
			total = diff + lastTotal - lastTotal % 360; // Store loops as part of lastTotal.
			if (dir != current) total += 360 * MathUtils.signum(lastTotal);
			timelinesRotation[i] = total;
		}
		timelinesRotation[i + 1] = diff;
		r1 += total * alpha;
		bone.rotation = r1 - (16384 - int((16384.499999999996 - r1 / 360))) * 360;
	}
	
	private function queueEvents (entry:TrackEntry, animationTime:Number):void {
		var animationStart:Number = entry.animationStart, animationEnd:Number = entry.animationEnd;
		var duration:Number = animationEnd - animationStart;
		var trackLastWrapped:Number = entry.trackLast % duration;

		// Queue events before complete.
		var events:Vector.<Event> = this.events;
		var event:Event;
		var i:int = 0, n:int = events.length;
		for (; i < n; i++) {
			event = events[i];
			if (event.time < trackLastWrapped) break;
			if (event.time > animationEnd) continue; // Discard events outside animation start/end.
			queue.event(entry, event);
		}

		// Queue complete if completed a loop iteration or the animation.
		if (entry.loop ? (trackLastWrapped > entry.trackTime % duration)
			: (animationTime >= animationEnd && entry.animationLast < animationEnd)) {
			queue.complete(entry);
		}

		// Queue events after complete.
		for (; i < n; i++) {
			event = events[i];
			if (event.time < animationStart) continue; // Discard events outside animation start/end.
			queue.event(entry, events[i]);
		}
		events.length = 0;
	}
	
	public function clearTracks ():void {
		queue.drainDisabled = true;
		for (var i:int = 0, n:int = tracks.length; i < n; i++)
			clearTrack(i);
		tracks.length = 0;
		queue.drainDisabled = false;
		queue.drain();
	}
	
	public function clearTrack (trackIndex:int):void {
		if (trackIndex >= tracks.length) return;
		var current:TrackEntry = tracks[trackIndex];
		if (current == null) return;

		queue.end(current);

		disposeNext(current);

		var entry:TrackEntry = current;
		while (true) {
			var from:TrackEntry = entry.mixingFrom;
			if (from == null) break;
			queue.end(from);
			entry.mixingFrom = null;
			entry = from;
		}

		tracks[current.trackIndex] = null;

		queue.drain();
	}
	
	
	private function setCurrent (index:int, current:TrackEntry):void {
		var from:TrackEntry = expandToIndex(index);
		tracks[index] = current;

		if (from != null) {
			queue.interrupt(from);
			current.mixingFrom = from;
			current.mixTime = 0;

			from.timelinesRotation.length = 0;

			// If not completely mixed in, set mixAlpha so mixing out happens from current mix to zero.
			if (from.mixingFrom != null) current.mixAlpha *= Math.min(from.mixTime / from.mixDuration, 1);
		}

		queue.start(current);
	}
	
	public function setAnimationByName (trackIndex:int, animationName:String, loop:Boolean):TrackEntry {
		var animation:Animation = data.skeletonData.findAnimation(animationName);
		if (animation == null) throw new ArgumentError("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}
	
	public function setAnimation (trackIndex:int, animation:Animation, loop:Boolean):TrackEntry {
		if (animation == null) throw new ArgumentError("animation cannot be null.");
		var current:TrackEntry = expandToIndex(trackIndex);
		if (current != null) {
			if (current.nextTrackLast == -1) {
				// Don't mix from an entry that was never applied.
				tracks[trackIndex] = null;
				queue.interrupt(current);
				queue.end(current);
				disposeNext(current);
				current = null;
			} else
				disposeNext(current);
		}
		var entry:TrackEntry = trackEntry(trackIndex, animation, loop, current);
		setCurrent(trackIndex, entry);
		queue.drain();
		return entry;
	}
	
	public function addAnimationByName (trackIndex:int, animationName:String, loop:Boolean, delay:Number):TrackEntry {
		var animation:Animation = data.skeletonData.findAnimation(animationName);
		if (animation == null) throw new ArgumentError("Animation not found: " + animationName);
		return addAnimation(trackIndex, animation, loop, delay);
	}
	
	public function addAnimation (trackIndex:int, animation:Animation, loop:Boolean, delay:Number):TrackEntry {
		if (animation == null) throw new ArgumentError("animation cannot be null.");

		var last:TrackEntry = expandToIndex(trackIndex);
		if (last != null) {
			while (last.next != null)
				last = last.next;
		}

		var entry:TrackEntry = trackEntry(trackIndex, animation, loop, last);

		if (last == null) {
			setCurrent(trackIndex, entry);
			queue.drain();
		} else {
			last.next = entry;
			if (delay <= 0) {
				var duration:Number = last.animationEnd - last.animationStart;
				if (duration != 0)
					delay += duration * (1 + (int)(last.trackTime / duration)) - data.getMix(last.animation, animation);
				else
					delay = 0;
			}
		}

		entry.delay = delay;
		return entry;
	}
	
	public function setEmptyAnimation (trackIndex:int, mixDuration:Number):TrackEntry {
		var entry:TrackEntry = setAnimation(trackIndex, emptyAnimation, false);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}
	
	public function addEmptyAnimation (trackIndex:int, mixDuration:Number, delay:Number):TrackEntry {
		if (delay <= 0) delay -= mixDuration;
		var entry:TrackEntry = addAnimation(trackIndex, emptyAnimation, false, delay);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}
	
	public function setEmptyAnimations (mixDuration:Number):void {
		queue.drainDisabled = true;
		for (var i:int = 0, n:int = tracks.length; i < n; i++) {
			var current:TrackEntry = tracks[i];
			if (current != null) setEmptyAnimation(current.trackIndex, mixDuration);
		}
		queue.drainDisabled = false;
		queue.drain();
	}
	
	private function expandToIndex (index:int):TrackEntry {
		if (index < tracks.length) return tracks[index];		
		tracks.length = index + 1;
		return null;
	}
	
	private function trackEntry (trackIndex:int, animation:Animation, loop:Boolean, last:TrackEntry):TrackEntry {
		var entry:TrackEntry = TrackEntry(trackEntryPool.obtain());
		entry.trackIndex = trackIndex;
		entry.animation = animation;
		entry.loop = loop;

		entry.eventThreshold = 0;
		entry.attachmentThreshold = 0;
		entry.drawOrderThreshold = 0;

		entry.animationStart = 0;
		entry.animationEnd = animation.duration;
		entry.animationLast = -1;
		entry.nextAnimationLast = -1;

		entry.delay = 0;
		entry.trackTime = 0;
		entry.trackLast = -1;
		entry.nextTrackLast = -1;
		entry.trackEnd = loop ? int.MAX_VALUE : entry.animationEnd;
		entry.timeScale = 1;

		entry.alpha = 1;
		entry.mixAlpha = 1;
		entry.mixTime = 0;
		entry.mixDuration = last == null ? 0 : data.getMix(last.animation, animation);
		return entry;
	}
	
	private function disposeNext (entry:TrackEntry):void{
		var next:TrackEntry = entry.next;
		while (next != null) {
			queue.dispose(next);
			next = next.next;
		}
		entry.next = null;
	}
	
	private function _animationsChanged ():void {
		animationsChanged = false;

		var propertyIDs:Dictionary = this.propertyIDs = new Dictionary();

		// Compute timelinesFirst from lowest to highest track entries.
		var i:int = 0, n:int = tracks.length;
		for (var key:String in propertyIDs) {
    		delete propertyIDs[key];
		}
		var entry:TrackEntry;
		for (; i < n; i++) { // Find first non-null entry.
			entry = tracks[i];
			if (entry == null) continue;
			setTimelinesFirst(entry);
			i++;
			break;
		}
		for (; i < n; i++) { // Rest of entries.
			entry = tracks[i];
			if (entry != null) checkTimelinesFirst(entry);
		}
	}
	
	private function setTimelinesFirst (entry:TrackEntry):void {
		if (entry.mixingFrom != null) {
			setTimelinesFirst(entry.mixingFrom);
			checkTimelinesUsage(entry, entry.timelinesFirst);
			return;
		}
		var propertyIDs:Dictionary = this.propertyIDs;
		var timelines:Vector.<Timeline> = entry.animation.timelines;
		var n:int = timelines.length;
		var usage:Vector.<Boolean> = entry.timelinesFirst;
		usage.length = n;
		for (var i:int = 0; i < n; i++) {
			var id:String = timelines[i].getPropertyId().toString();
			propertyIDs[id] = id;
			usage[i] = true;
		}
	}
	
	private function checkTimelinesFirst (entry:TrackEntry):void {
		if (entry.mixingFrom != null) checkTimelinesFirst(entry.mixingFrom);
		checkTimelinesUsage(entry, entry.timelinesFirst);
	}
	
	private function checkTimelinesUsage (entry:TrackEntry, usageArray:Vector.<Boolean>):void {
		var propertyIDs:Dictionary = this.propertyIDs;
		var timelines:Vector.<Timeline> = entry.animation.timelines;
		var n:int = timelines.length;
		var usage:Vector.<Boolean> = usageArray;
		usageArray.length = n;
		for (var i:int = 0; i < n; i++) {
			var id:String = timelines[i].getPropertyId().toString();
			usage[i] = !propertyIDs.hasOwnProperty(id);			
			propertyIDs[id] = id;
		}
	}
	
	public function getCurrent (trackIndex:int):TrackEntry {
		if (trackIndex >= tracks.length) return null;
		return tracks[trackIndex];
	}
	
	public function clearListeners ():void {
		onStart.listeners.length = 0;
		onInterrupt.listeners.length = 0;
		onEnd.listeners.length = 0;
		onDispose.listeners.length = 0;
		onComplete.listeners.length = 0;
		onEvent.listeners.length = 0;	
	}
	
	public function clearListenerNotifications ():void {
		queue.clear();
	}
}
}
