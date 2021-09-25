/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.AnimationState.AnimationStateListener;

/** Stores the current pose values for an {@link Event}.
 * <p>
 * See Timeline
 * {@link Timeline#apply(Skeleton, float, float, com.badlogic.gdx.utils.Array, float, com.esotericsoftware.spine.Animation.MixBlend, com.esotericsoftware.spine.Animation.MixDirection)},
 * AnimationStateListener {@link AnimationStateListener#event(com.esotericsoftware.spine.AnimationState.TrackEntry, Event)}, and
 * <a href="http://esotericsoftware.com/spine-events">Events</a> in the Spine User Guide. */
public class Event {
	private final EventData data;
	int intValue;
	float floatValue;
	String stringValue;
	float volume, balance;
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
		if (stringValue == null) throw new IllegalArgumentException("stringValue cannot be null.");
		this.stringValue = stringValue;
	}

	public float getVolume () {
		return volume;
	}

	public void setVolume (float volume) {
		this.volume = volume;
	}

	public float getBalance () {
		return balance;
	}

	public void setBalance (float balance) {
		this.balance = balance;
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
