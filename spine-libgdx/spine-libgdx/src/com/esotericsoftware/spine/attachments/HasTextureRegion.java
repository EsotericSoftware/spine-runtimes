
package com.esotericsoftware.spine.attachments;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.utils.Null;

public interface HasTextureRegion {
	/** The name used to find the {@link #getRegion()}. */
	public String getPath ();

	public void setPath (String path);

	public TextureRegion getRegion ();

	/** Sets the region used to draw the attachment. After setting the region or if the region's properties are changed,
	 * {@link #updateRegion()} must be called. */
	public void setRegion (TextureRegion region);

	/** Updates any values the attachment calculates using the {@link #getRegion()}. Must be called after setting the
	 * {@link #getRegion()} or if the region's properties are changed. */
	public void updateRegion ();

	/** The color to tint the attachment. */
	public Color getColor ();

	public @Null Sequence getSequence ();

	public void setSequence (@Null Sequence sequence);
}
