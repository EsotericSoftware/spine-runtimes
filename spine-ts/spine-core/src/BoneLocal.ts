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

import { Inherit } from "./BoneData.js";
import type { Pose } from "./Pose.js"

/** Stores a bone's local pose. */
export class BoneLocal implements Pose<BoneLocal> {

	/** The local x translation. */
	x = 0;

	/** The local y translation. */
	y = 0;

	/** The local rotation in degrees, counter clockwise. */
	rotation = 0;

	/** The local scaleX. */
	scaleX = 0;

	/** The local scaleY. */
	scaleY = 0;

	/** The local shearX. */
	shearX = 0;

	/** The local shearY. */
	shearY = 0;

	inherit = Inherit.Normal;

	set (pose: BoneLocal): void {
		if (pose == null) throw new Error("pose cannot be null.");
		this.x = pose.x;
		this.y = pose.y;
		this.rotation = pose.rotation;
		this.scaleX = pose.scaleX;
		this.scaleY = pose.scaleY;
		this.shearX = pose.shearX;
		this.shearY = pose.shearY;
		this.inherit = pose.inherit;
	}

	setPosition (x: number, y: number): void {
		this.x = x;
		this.y = y;
	}

	setScale (scaleX: number, scaleY: number): void;
	setScale (scale: number): void;
	setScale (scaleOrX: number, scaleY?: number): void {
		this.scaleX = scaleOrX;
		this.scaleY = scaleY === undefined ? scaleOrX : scaleY;
	}

	/** Determines how parent world transforms affect this bone. */
	public getInherit (): Inherit {
		return this.inherit;
	}

	public setInherit (inherit: Inherit): void {
		if (inherit == null) throw new Error("inherit cannot be null.");
		this.inherit = inherit;
	}
}
