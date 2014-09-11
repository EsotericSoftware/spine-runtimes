
package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;

public class IkConstraintData {
	final String name;
	final Array<BoneData> bones = new Array();
	BoneData target;
	int bendDirection = 1;
	float mix = 1;

	public IkConstraintData (String name) {
		this.name = name;
	}

	public String getName () {
		return name;
	}

	public Array<BoneData> getBones () {
		return bones;
	}

	public BoneData getTarget () {
		return target;
	}

	public void setTarget (BoneData target) {
		this.target = target;
	}

	public int getBendDirection () {
		return bendDirection;
	}

	public void setBendDirection (int bendDirection) {
		this.bendDirection = bendDirection;
	}

	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}

	public String toString () {
		return name;
	}
}
