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

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.AnimationState.AnimationStateListener;

/** Stores the current pose values for an {@link Event}.
 * <p>
 * See Timeline
 * {@link Timeline#apply(Skeleton, float, float, com.badlogic.gdx.utils.Array, float, com.esotericsoftware.spine.Animation.MixPose, com.esotericsoftware.spine.Animation.MixDirection)},
 * AnimationStateListener {@link AnimationStateListener#event(com.esotericsoftware.spine.AnimationState.TrackEntry, Event)}, and
 * <a href="http://esotericsoftware.com/spine-events">Events</a> in the Spine User Guide. */
public class Event {
	final private EventData data;
	int intValue;
	float floatValue;
	String stringValue;
	final float time;

	public Event (float time, EventData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.time = time;
		this.data = data;
	}

	public int getInt () {
		return intValue;
	}

	public void setInt (int intValue) {
		this.intValue = intValue;
	}

	public float getFloat () {
		return floatValue;
	}

	public void setFloat (float floatValue) {
		this.floatValue = floatValue;
	}

	public String getString () {
		return stringValue;
	}

	public void setString (String stringValue) {
		this.stringValue = stringValue;
	}

	/** The animation time this event was keyed. */
	public float getTime () {
		return time;
	}

	/** The events's setup pose data. */
	public EventData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
