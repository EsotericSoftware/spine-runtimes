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

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.Null;

/** The setup pose for a bone. */
public class BoneData extends PosedData<BonePose> {
	final int index;
	@Null final BoneData parent;
	float length;

	// Nonessential.
	final Color color = new Color(0.61f, 0.61f, 0.61f, 1); // 9b9b9bff
	@Null String icon;
	boolean visible;

	public BoneData (int index, String name, @Null BoneData parent) {
		super(name, new BonePose());
		if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.index = index;
		this.parent = parent;
	}

	/** Copy constructor. */
	public BoneData (BoneData data, @Null BoneData parent) {
		this(data.index, data.name, parent);
		length = data.length;
		setupPose.set(data.setupPose);
	}

	/** The bone's name, unique across all bones in the skeleton.
	 * <p>
	 * See {@link SkeletonData#findBone(String)} and {@link Skeleton#findBone(String)}. */
	public String getName () { // Do not port.
		return super.getName();
	}

	/** The {@link Skeleton#bones} index. */
	public int getIndex () {
		return index;
	}

	/** The parent bone, or null if this bone is the root. */
	public @Null BoneData getParent () {
		return parent;
	}

	/** The bone's length. */
	public float getLength () {
		return length;
	}

	public void setLength (float length) {
		this.length = length;
	}

	/** The color of the bone as it was in Spine, or a default color if nonessential data was not exported. Bones are not usually
	 * rendered at runtime. */
	public Color getColor () {
		return color;
	}

	/** The bone icon name as it was in Spine, or null if nonessential data was not exported. */
	public @Null String getIcon () {
		return icon;
	}

	public void setIcon (@Null String icon) {
		this.icon = icon;
	}

	/** False if the bone was hidden in Spine and nonessential data was exported. Does not affect runtime rendering. */
	public boolean getVisible () {
		return visible;
	}

	public void setVisible (boolean visible) {
		this.visible = visible;
	}

	/** Determines how a bone inherits world transforms from parent bones. */
	static public enum Inherit {
		normal, onlyTranslation, noRotationOrReflection, noScale, noScaleOrReflection;

		static public final Inherit[] values = Inherit.values();
	}
}
