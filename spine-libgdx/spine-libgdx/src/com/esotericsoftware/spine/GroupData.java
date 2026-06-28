
package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Null;

/** Stores the setup pose for a {@link Group}. */
public class GroupData extends PosedData<GroupPose> {
	@Null SlotData clipSlot;
	boolean clipInverse;

	// Nonessential.
	boolean visible = true;

	public GroupData (String name) {
		super(name, new GroupPose());
	}

	/** The slot used as this group's clip/mask source, or null if clipping is disabled. */
	public @Null SlotData getClipSlot () {
		return clipSlot;
	}

	/** @param clipSlot May be null if clipping is disabled. */
	public void setClipSlot (@Null SlotData clipSlot) {
		this.clipSlot = clipSlot;
	}

	/** When true, pixels outside the clip slot are kept instead of pixels inside it. */
	public boolean getClipInverse () {
		return clipInverse;
	}

	public void setClipInverse (boolean clipInverse) {
		this.clipInverse = clipInverse;
	}

	/** False if the group was hidden in Spine and nonessential data was exported. Does not affect runtime rendering. */
	public boolean getVisible () {
		return visible;
	}

	public void setVisible (boolean visible) {
		this.visible = visible;
	}
}
