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

import openfl.utils.Dictionary;
import openfl.Vector;
import spine.Event;
import spine.Skeleton;

class Animation {
	private var _name:String;
	private var _timelines:Vector<Timeline>;
	private var _timelineIds:Dictionary<String, Bool> = new Dictionary<String, Bool>();

	public var duration:Float = 0;

	public function new(name:String, timelines:Vector<Timeline>, duration:Float) {
		if (name == null)
			throw new SpineException("name cannot be null.");
		_name = name;
		setTimelines(timelines);
		this.duration = duration;
	}

	public function setTimelines(timelines:Vector<Timeline>) {
		if (timelines == null)
			throw new SpineException("timelines cannot be null.");
		_timelines = timelines;
		_timelineIds = new Dictionary<String, Bool>();
		for (timeline in timelines) {
			var ids:Vector<String> = timeline.propertyIds;
			for (id in ids) {
				_timelineIds[id] = true;
			}
		}
	}

	public function hasTimeline(ids:Vector<String>):Bool {
		for (id in ids) {
			if (_timelineIds[id])
				return true;
		}
		return false;
	}

	/** Poses the skeleton at the specified time for this animation. */
	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, loop:Bool, events:Vector<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");

		if (loop && duration != 0) {
			time %= duration;
			if (lastTime > 0)
				lastTime %= duration;
		}

		for (timeline in timelines) {
			timeline.apply(skeleton, lastTime, time, events, alpha, blend, direction);
		}
	}

	public var name(get, never):String;

	private function get_name():String {
		return _name;
	}

	public function toString():String {
		return _name;
	}

	public var timelines(get, never):Vector<Timeline>;

	private function get_timelines():Vector<Timeline> {
		return _timelines;
	}
}
