/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

/** Stores the setup pose values for an {@link Event}.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-events">Events</a> in the Spine User Guide. */
public class EventData {
	final String name;
	int intValue;
	float floatValue;
	String stringValue, audioPath;
	float volume, balance;

	public EventData (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
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

	public String getAudioPath () {
		return audioPath;
	}

	public void setAudioPath (String audioPath) {
		if (audioPath == null) throw new IllegalArgumentException("audioPath cannot be null.");
		this.audioPath = audioPath;
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

	/** The name of the event, which is unique across all events in the skeleton. */
	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}
}
