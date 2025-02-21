/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated February 20, 2024. Replaces all prior versions.
 *
 * Copyright (c) 2013-2024, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * https://esotericsoftware.com/spine-editor-license
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

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.Skeleton.Physics;

/** The interface for items updated by {@link Skeleton#updateWorldTransform(Physics)}. */
public interface Updatable {
	/** @param physics Determines how physics and other non-deterministic updates are applied. */
	public void update (Physics physics);

	/** Returns false when this item won't be updated by
	 * {@link Skeleton#updateWorldTransform(com.esotericsoftware.spine.Skeleton.Physics)} because a skin is required and the
	 * {@link Skeleton#getSkin() active skin} does not contain this item.
	 * @see Skin#getBones()
	 * @see Skin#getConstraints()
	 * @see BoneData#getSkinRequired()
	 * @see ConstraintData#getSkinRequired() */
	public boolean isActive ();
}
