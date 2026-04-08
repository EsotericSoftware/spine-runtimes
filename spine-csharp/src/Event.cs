/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

using System;

namespace Spine {
	/// <summary>Fired by <see cref="EventTimeline"/> when specific animation times are reached.
	/// <para>See <see cref="Timeline.Apply(Skeleton, float, float, ExposedList{Event}, float, bool, bool, bool, bool)"/> and
	/// <a href="https://esotericsoftware.com/spine-events">Events</a> in the Spine User Guide.</para></summary>
	public class Event {
		internal readonly float time;
		internal readonly EventData data;
		internal int intValue;
		internal float floatValue;
		internal string stringValue;
		internal float volume, balance;

		/// <summary>The event's setup pose data.</summary>
		public EventData Data { get { return data; } }
		/// <summary>The animation time this event was keyed, or -1 for the setup pose.</summary>
		public float Time { get { return time; } }

		/// <summary>The integer payload for this event.</summary>
		public int Int { get { return intValue; } set { intValue = value; } }
		/// <summary>The float payload for this event.</summary>
		public float Float { get { return floatValue; } set { floatValue = value; } }
		/// <summary>The string payload for this event.</summary>
		public string String { get { return stringValue; } set { stringValue = value; } }

		/// <summary>If an audio path is set, the volume for the audio.</summary>
		public float Volume { get { return volume; } set { volume = value; } }
		/// <summary>If an audio path is set, the left/right balance for the audio.</summary>
		public float Balance { get { return balance; } set { balance = value; } }

		public Event (float time, EventData data) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			this.time = time;
			this.data = data;
		}

		override public string ToString () {
			return this.data.Name;
		}
	}
}
