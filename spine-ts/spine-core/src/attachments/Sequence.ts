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

import { TextureRegion } from "../Texture";
import { Slot } from "../Slot";
import { HasTextureRegion } from "./HasTextureRegion";
import { Utils } from "../Utils";


export class Sequence {
	private static _nextID = 0;

	id = Sequence.nextID();
	regions: TextureRegion[];
	start = 0;
	digits = 0;
	/** The index of the region to show for the setup pose. */
	setupIndex = 0;

	constructor (count: number) {
		this.regions = new Array<TextureRegion>(count);
	}

	copy (): Sequence {
		let copy = new Sequence(this.regions.length);
		Utils.arrayCopy(this.regions, 0, copy.regions, 0, this.regions.length);
		copy.start = this.start;
		copy.digits = this.digits;
		copy.setupIndex = this.setupIndex;
		return copy;
	}

	apply (slot: Slot, attachment: HasTextureRegion) {
		let index = slot.sequenceIndex;
		if (index == -1) index = this.setupIndex;
		if (index >= this.regions.length) index = this.regions.length - 1;
		let region = this.regions[index];
		if (attachment.region != region) {
			attachment.region = region;
			attachment.updateRegion();
		}
	}

	getPath (basePath: string, index: number): string {
		let result = basePath;
		let frame = (this.start + index).toString();
		for (let i = this.digits - frame.length; i > 0; i--)
			result += "0";
		result += frame;
		return result;
	}

	private static nextID (): number {
		return Sequence._nextID++;
	}
}

export enum SequenceMode {
	hold = 0,
	once = 1,
	loop = 2,
	pingpong = 3,
	onceReverse = 4,
	loopReverse = 5,
	pingpongReverse = 6
}

export const SequenceModeValues = [
	SequenceMode.hold,
	SequenceMode.once,
	SequenceMode.loop,
	SequenceMode.pingpong,
	SequenceMode.onceReverse,
	SequenceMode.loopReverse,
	SequenceMode.pingpongReverse
];