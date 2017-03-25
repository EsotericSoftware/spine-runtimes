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
	import spine.Event;

	public class EventQueue {
		internal var objects : Vector.<Object> = new Vector.<Object>();
		internal var animationState : AnimationState;
		public var drainDisabled : Boolean;

		public function EventQueue(animationState : AnimationState) {
			this.animationState = animationState;
		}

		public function start(entry : TrackEntry) : void {
			objects.push(EventType.start);
			objects.push(entry);
			animationState.animationsChanged = true;
		}

		public function interrupt(entry : TrackEntry) : void {
			objects.push(EventType.interrupt);
			objects.push(entry);
		}

		public function end(entry : TrackEntry) : void {
			objects.push(EventType.end);
			objects.push(entry);
			animationState.animationsChanged = true;
		}

		public function dispose(entry : TrackEntry) : void {
			objects.push(EventType.dispose);
			objects.push(entry);
		}

		public function complete(entry : TrackEntry) : void {
			objects.push(EventType.complete);
			objects.push(entry);
		}

		public function event(entry : TrackEntry, event : Event) : void {
			objects.push(EventType.event);
			objects.push(entry);
			objects.push(event);
		}

		public function drain() : void {
			if (drainDisabled) return; // Not reentrant.
			drainDisabled = true;

			var objects : Vector.<Object> = this.objects;
			for (var i : int = 0; i < objects.length; i += 2) {
				var type : EventType = EventType(objects[i]);
				var entry : TrackEntry = TrackEntry(objects[i + 1]);
				switch (type) {
					case EventType.start:
						entry.onStart.invoke(entry);
						animationState.onStart.invoke(entry);
						break;
					case EventType.interrupt:
						entry.onInterrupt.invoke(entry);
						animationState.onInterrupt.invoke(entry);
						break;
					case EventType.end:
						entry.onEnd.invoke(entry);
						animationState.onEnd.invoke(entry);
					// Fall through.
					case EventType.dispose:
						entry.onDispose.invoke(entry);
						animationState.onDispose.invoke(entry);
						animationState.trackEntryPool.free(entry);
						break;
					case EventType.complete:
						entry.onComplete.invoke(entry);
						animationState.onComplete.invoke(entry);
						break;
					case EventType.event:
						var event : Event = Event(objects[i++ + 2]);
						entry.onEvent.invoke(entry, event);
						animationState.onEvent.invoke(entry, event);
						break;
				}
			}
			clear();

			drainDisabled = false;
		}

		public function clear() : void {
			objects.length = 0;
		}
	}
}