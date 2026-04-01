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


import type { EventTimeline, Timeline } from "./Animation.js";
import type { AnimationStateListener } from "./AnimationState.js";
import type { EventData } from "./EventData.js";

/** Fired by {@link EventTimeline} when specific animation times are reached.
 *
 * See Timeline {@link Timeline.apply},
 * AnimationStateListener {@link AnimationStateListener.event}, and
 * [Events](http://esotericsoftware.com/spine-events) in the Spine User Guide. */
export class Event {

	/** The animation time this event was keyed, or -1 for the setup pose. */
	time: number = 0;

	readonly data: EventData;

	/** The integer payload for this event. */
	intValue: number = 0;

	/** The float payload for this event. */
	floatValue: number = 0;

	stringValue: string | null = null;

	/** If an audio path is set, the volume for the audio. */
	volume: number = 0;

	/** If an audio path is set, the left/right balance for the audio. */
	balance: number = 0;

	constructor (time: number, data: EventData) {
		if (!data) throw new Error("data cannot be null.");
		this.time = time;
		this.data = data;
	}
}
