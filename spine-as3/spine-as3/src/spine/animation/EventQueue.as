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
