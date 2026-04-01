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

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.Animation.EventTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.AnimationState.AnimationStateListener;

/** Fired by {@link EventTimeline} when specific animation times are reached.
 * <p>
 * See {@link Timeline#apply(Skeleton, float, float, com.badlogic.gdx.utils.Array, float, boolean, boolean, boolean, boolean)},
 * {@link AnimationStateListener#event(com.esotericsoftware.spine.AnimationState.TrackEntry, Event)}, and
 * <a href="https://esotericsoftware.com/spine-events">Events</a> in the Spine User Guide. */
public class Event {
	final float time;
	final EventData data;
	int intValue;
	float floatValue;
	String stringValue;
	float volume, balance;

	public Event (float time, EventData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.time = time;
		this.data = data;
	}

	/** The integer payload for this event. */
	public int getInt () {
		return intValue;
	}

	public void setInt (int intValue) {
		this.intValue = intValue;
	}

	/** The float payload for this event. */
	public float getFloat () {
		return floatValue;
	}

	public void setFloat (float floatValue) {
		this.floatValue = floatValue;
	}

	/** The string payload for this event. */
	public String getString () {
		return stringValue;
	}

	public void setString (String stringValue) {
		if (stringValue == null) throw new IllegalArgumentException("stringValue cannot be null.");
		this.stringValue = stringValue;
	}

	/** If an audio path is set, the volume for the audio. */
	public float getVolume () {
		return volume;
	}

	public void setVolume (float volume) {
		this.volume = volume;
	}

	/** If an audio path is set, the left/right balance for the audio. */
	public float getBalance () {
		return balance;
	}

	public void setBalance (float balance) {
		this.balance = balance;
	}

	/** The animation time this event was keyed, or -1 for the setup pose. */
	public float getTime () {
		return time;
	}

	/** The event's setup pose data. */
	public EventData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
