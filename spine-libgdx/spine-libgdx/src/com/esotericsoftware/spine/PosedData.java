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

/** The base class for storing setup data for a posed object. May be shared with multiple instances. */
abstract public class PosedData<P extends Pose> {
	final String name;
	final P setupPose;
	boolean skinRequired;

	protected PosedData (String name, P setupPose) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
		this.setupPose = setupPose;
	}

	public String getName () {
		return name;
	}

	/** The setup pose that most animations are relative to. */
	public P getSetupPose () {
		return setupPose;
	}

	/** When true, {@link Skeleton#updateWorldTransform(Physics)} only updates this constraint if the {@link Skeleton#skin}
	 * contains this constraint.
	 * <p>
	 * See {@link Skin#constraints}. */
	public boolean getSkinRequired () {
		return skinRequired;
	}

	public void setSkinRequired (boolean skinRequired) {
		this.skinRequired = skinRequired;
	}

	public String toString () {
		return name;
	}
}
