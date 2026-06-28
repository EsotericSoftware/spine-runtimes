package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.Null;

/** Stores a pose for a group. */
public class GroupPose implements Pose<GroupPose> {
	@Null Color color, darkColor;

	public void set (GroupPose pose) {
		setColor(pose.color);
		setDarkColor(pose.darkColor);
	}

	/** The tint color for this group, or null when group tinting is disabled. If {@link #darkColor} is set, this is used as the light
	 * color for two color tinting. */
	public @Null Color getColor () {
		return color;
	}

	/** @param color May be null when group tinting is disabled. */
	public void setColor (@Null Color color) {
		if (color == null) {
			this.color = null;
			return;
		}
		if (this.color == null) this.color = new Color();
		this.color.set(color);
	}

	/** The dark color used to tint this group for two color tinting, or null if two color tinting is not used. The dark color's
	 * alpha is not used. */
	public @Null Color getDarkColor () {
		return darkColor;
	}

	/** @param darkColor May be null when two color tinting is disabled. */
	public void setDarkColor (@Null Color darkColor) {
		if (darkColor == null) {
			this.darkColor = null;
			return;
		}
		if (this.darkColor == null) this.darkColor = new Color();
		this.darkColor.set(darkColor);
	}
}
