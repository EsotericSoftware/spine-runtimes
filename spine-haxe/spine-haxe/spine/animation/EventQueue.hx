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

import spine.Event;

class EventQueue {
	private var objects:Array<Dynamic>;
	private var animationState:AnimationState;

	public var drainDisabled:Bool = false;

	public function new(animationState:AnimationState) {
		this.animationState = animationState;
		objects = new Array<Dynamic>();
	}

	public function start(entry:TrackEntry):Void {
		objects.push(EventType.start);
		objects.push(entry);
		animationState.animationsChanged = true;
	}

	public function interrupt(entry:TrackEntry):Void {
		objects.push(EventType.interrupt);
		objects.push(entry);
	}

	public function end(entry:TrackEntry):Void {
		objects.push(EventType.end);
		objects.push(entry);
		animationState.animationsChanged = true;
	}

	public function dispose(entry:TrackEntry):Void {
		objects.push(EventType.dispose);
		objects.push(entry);
	}

	public function complete(entry:TrackEntry):Void {
		objects.push(EventType.complete);
		objects.push(entry);
	}

	public function event(entry:TrackEntry, event:Event):Void {
		objects.push(EventType.event);
		objects.push(entry);
		objects.push(event);
	}

	public function drain():Void {
		if (drainDisabled)
			return; // Not reentrant.
		drainDisabled = true;

		var i:Int = 0;
		while (i < objects.length) {
			var type:EventType = cast(objects[i], EventType);
			var entry:TrackEntry = cast(objects[i + 1], TrackEntry);
			switch (type) {
				case EventType.start:
					entry.onStart.invoke(entry);
					animationState.onStart.invoke(entry);
				case EventType.interrupt:
					entry.onInterrupt.invoke(entry);
					animationState.onInterrupt.invoke(entry);
				case EventType.end:
					entry.onEnd.invoke(entry);
					animationState.onEnd.invoke(entry);
					entry.onDispose.invoke(entry);
					animationState.onDispose.invoke(entry);
					animationState.trackEntryPool.free(entry);
				case EventType.dispose:
					entry.onDispose.invoke(entry);
					animationState.onDispose.invoke(entry);
					animationState.trackEntryPool.free(entry);
				case EventType.complete:
					entry.onComplete.invoke(entry);
					animationState.onComplete.invoke(entry);
				case EventType.event:
					var event:Event = cast(objects[i++ + 2], Event);
					entry.onEvent.invoke(entry, event);
					animationState.onEvent.invoke(entry, event);
			}
			i += 2;
		}
		clear();

		drainDisabled = false;
	}

	public function clear():Void {
		objects.resize(0);
	}
}
