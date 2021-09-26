
package com.esotericsoftware.spine.attachments;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureRegion;

public interface HasTextureRegion {
	/** The name used to find the {@link #getRegion()}. */
	public String getPath ();

	public void setPath (String path);

	/** Sets the region used to draw the attachment. If the region or its properties are changed, {@link #updateRegion()} must be
	 * called. */
	public void setRegion (TextureRegion region);

	public TextureRegion getRegion ();

	/** Updates any values the attachment calculates using the {@link #getRegion()}. Must be called after changing the region or
	 * the region's properties. */
	public void updateRegion ();

	/** The color to tint the attachment. */
	public Color getColor ();
}
