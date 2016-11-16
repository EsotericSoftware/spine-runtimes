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
import spine.Poolable;

public class TrackEntry implements Poolable {
	public var animation:Animation;
	public var next:TrackEntry, mixingFrom:TrackEntry;
	public var onStart:Listeners = new Listeners();
	public var onInterrupt:Listeners = new Listeners();
	public var onEnd:Listeners = new Listeners();
	public var onDispose:Listeners = new Listeners();
	public var onComplete:Listeners = new Listeners();
	public var onEvent:Listeners = new Listeners();
	public var trackIndex:int;
	public var loop:Boolean;
	public var eventThreshold:Number, attachmentThreshold:Number, drawOrderThreshold:Number;
	public var animationStart:Number, animationEnd:Number, animationLast:Number, nextAnimationLast:Number;
	public var delay:Number, trackTime:Number, trackLast:Number, nextTrackLast:Number, trackEnd:Number, timeScale:Number;
	public var alpha:Number, mixTime:Number, mixDuration:Number, mixAlpha:Number;
	public var timelinesFirst:Vector.<Boolean> = new Vector.<Boolean>();
	public var timelinesRotation:Vector.<Number> = new Vector.<Number>();
	
	public function TrackEntry () {		
	}
	
	public function getAnimationTime():Number {
		if (loop) {
			var duration:Number = animationEnd - animationStart;
			if (duration == 0) return animationStart;
			return (trackTime % duration) + animationStart;
		}
		return Math.min(trackTime + animationStart, animationEnd);
	}
	
	public function reset ():void {
		next = null;
		mixingFrom = null;
		animation = null;
		onStart.listeners.length = 0;
		onInterrupt.listeners.length = 0;
		onEnd.listeners.length = 0;
		onDispose.listeners.length = 0;
		onComplete.listeners.length = 0;
		onEvent.listeners.length = 0;
		timelinesFirst.length = 0;
		timelinesRotation.length = 0;
	}
	
	public function resetRotationDirection ():void {
		timelinesRotation.length = 0;
	}
}
}
