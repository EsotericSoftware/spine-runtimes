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

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Null;

/** A node in a skeleton's hierarchy with a transform that affects its children and their attachments. A bone has a number of
 * poses:
 * <ul>
 * <li>{@link #data}: The setup pose.
 * <li>{@link #pose}: The unconstrained local pose. Set by animations and application code.
 * <li>{@link #appliedPose}: The local pose to use for rendering. Possibly modified by constraints.
 * <li>World transform: the local pose combined with the parent world transform. Computed on a pose by
 * {@link BonePose#updateWorldTransform(Skeleton)} and {@link Skeleton#updateWorldTransform(Physics)}.
 * </ul>
 */
public class Bone extends PosedActive<BoneData, BonePose> {
	@Null final Bone parent;
	final Array<Bone> children = new Array(true, 4, Bone[]::new);
	boolean sorted;

	public Bone (BoneData data, @Null Bone parent) {
		super(data, new BonePose(), new BonePose());
		this.parent = parent;
		appliedPose.bone = this;
		constrainedPose.bone = this;
	}

	/** Copy constructor. Does not copy the {@link #children} bones. */
	public Bone (Bone bone, @Null Bone parent) {
		this(bone.data, parent);
		pose.set(bone.pose);
	}

	/** The parent bone, or null if this is the root bone. */
	public @Null Bone getParent () {
		return parent;
	}

	/** The immediate children of this bone. */
	public Array<Bone> getChildren () {
		return children;
	}
}
