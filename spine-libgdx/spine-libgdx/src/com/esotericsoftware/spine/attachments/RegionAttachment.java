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

package com.esotericsoftware.spine.attachments;

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.utils.Null;

import com.esotericsoftware.spine.BonePose;
import com.esotericsoftware.spine.Slot;
import com.esotericsoftware.spine.SlotPose;

/** An attachment that displays a textured quadrilateral.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-regions">Region attachments</a> in the Spine User Guide. */
public class RegionAttachment extends Attachment implements HasSequence {
	static public final int BLX = 0, BLY = 1;
	static public final int ULX = 2, ULY = 3;
	static public final int URX = 4, URY = 5;
	static public final int BRX = 6, BRY = 7;

	private final Sequence sequence;
	float x, y, scaleX = 1, scaleY = 1, rotation, width, height;
	private String path;
	private final Color color = new Color(1, 1, 1, 1);

	public RegionAttachment (String name, Sequence sequence) {
		super(name);
		if (sequence == null) throw new IllegalArgumentException("sequence cannot be null.");
		this.sequence = sequence;
	}

	/** Copy constructor. */
	protected RegionAttachment (RegionAttachment other) {
		super(other);
		path = other.path;
		x = other.x;
		y = other.y;
		scaleX = other.scaleX;
		scaleY = other.scaleY;
		rotation = other.rotation;
		width = other.width;
		height = other.height;
		color.set(other.color);
		sequence = new Sequence(other.sequence);
	}

	/** Transforms the attachment's four vertices to world coordinates.
	 * <p>
	 * See <a href="https://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
	 * Runtimes Guide.
	 * @param worldVertices The output world vertices. Must have a length >= <code>offset</code> + 8.
	 * @param vertexOffsets The vertex {@link Sequence#getOffsets(int) offsets}.
	 * @param offset The <code>worldVertices</code> index to begin writing values.
	 * @param stride The number of <code>worldVertices</code> entries between the value pairs written. */
	public void computeWorldVertices (Slot slot, float[] vertexOffsets, float[] worldVertices, int offset, int stride) {
		BonePose bone = slot.getBone().getAppliedPose();
		float x = bone.getWorldX(), y = bone.getWorldY();
		float a = bone.getA(), b = bone.getB(), c = bone.getC(), d = bone.getD();

		float offsetX = vertexOffsets[BRX];
		float offsetY = vertexOffsets[BRY];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // br
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffsets[BLX];
		offsetY = vertexOffsets[BLY];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // bl
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffsets[ULX];
		offsetY = vertexOffsets[ULY];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ul
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffsets[URX];
		offsetY = vertexOffsets[URY];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ur
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
	}

	/** Returns the vertex {@link Sequence#getOffsets(int) offsets} for the specified slot pose. */
	public float[] getOffsets (SlotPose pose) {
		return sequence.getOffsets(sequence.resolveIndex(pose));
	}

	/** The local x translation. */
	public float getX () {
		return x;
	}

	public void setX (float x) {
		this.x = x;
	}

	/** The local y translation. */
	public float getY () {
		return y;
	}

	public void setY (float y) {
		this.y = y;
	}

	/** The local scaleX. */
	public float getScaleX () {
		return scaleX;
	}

	public void setScaleX (float scaleX) {
		this.scaleX = scaleX;
	}

	/** The local scaleY. */
	public float getScaleY () {
		return scaleY;
	}

	public void setScaleY (float scaleY) {
		this.scaleY = scaleY;
	}

	/** The local rotation. */
	public float getRotation () {
		return rotation;
	}

	public void setRotation (float rotation) {
		this.rotation = rotation;
	}

	/** The width of the region attachment in Spine. */
	public float getWidth () {
		return width;
	}

	public void setWidth (float width) {
		this.width = width;
	}

	/** The height of the region attachment in Spine. */
	public float getHeight () {
		return height;
	}

	public void setHeight (float height) {
		this.height = height;
	}

	public Sequence getSequence () {
		return sequence;
	}

	public void updateSequence () {
		sequence.update(this);
	}

	public String getPath () {
		return path;
	}

	public void setPath (String path) {
		this.path = path;
	}

	public Color getColor () {
		return color;
	}

	public RegionAttachment copy () {
		return new RegionAttachment(this);
	}

	/** Computes {@link Sequence#getUVs(int) UVs} and {@link Sequence#getOffsets(int) offsets} for a region attachment.
	 * @param uvs Output array for the computed UVs, length of 8.
	 * @param offset Output array for the computed vertex offsets, length of 8. */
	static void computeUVs (@Null TextureRegion region, float x, float y, float scaleX, float scaleY, float rotation, float width,
		float height, float[] offset, float[] uvs) {
		float localX2 = width / 2, localY2 = height / 2;
		float localX = -localX2, localY = -localY2;
		boolean rotated = false;
		if (region instanceof AtlasRegion r) {
			localX += r.offsetX / r.originalWidth * width;
			localY += r.offsetY / r.originalHeight * height;
			if (r.degrees == 90) {
				rotated = true;
				localX2 -= (r.originalWidth - r.offsetX - r.packedHeight) / r.originalWidth * width;
				localY2 -= (r.originalHeight - r.offsetY - r.packedWidth) / r.originalHeight * height;
			} else {
				localX2 -= (r.originalWidth - r.offsetX - r.packedWidth) / r.originalWidth * width;
				localY2 -= (r.originalHeight - r.offsetY - r.packedHeight) / r.originalHeight * height;
			}
		}
		localX *= scaleX;
		localY *= scaleY;
		localX2 *= scaleX;
		localY2 *= scaleY;
		float r = rotation * degRad, cos = cos(r), sin = sin(r);
		float localXCos = localX * cos + x;
		float localXSin = localX * sin;
		float localYCos = localY * cos + y;
		float localYSin = localY * sin;
		float localX2Cos = localX2 * cos + x;
		float localX2Sin = localX2 * sin;
		float localY2Cos = localY2 * cos + y;
		float localY2Sin = localY2 * sin;
		offset[BLX] = localXCos - localYSin;
		offset[BLY] = localYCos + localXSin;
		offset[ULX] = localXCos - localY2Sin;
		offset[ULY] = localY2Cos + localXSin;
		offset[URX] = localX2Cos - localY2Sin;
		offset[URY] = localY2Cos + localX2Sin;
		offset[BRX] = localX2Cos - localYSin;
		offset[BRY] = localYCos + localX2Sin;
		if (region == null) {
			uvs[BLX] = 0;
			uvs[BLY] = 0;
			uvs[ULX] = 0;
			uvs[ULY] = 1;
			uvs[URX] = 1;
			uvs[URY] = 1;
			uvs[BRX] = 1;
			uvs[BRY] = 0;
		} else {
			uvs[BLX] = region.getU2();
			uvs[ULY] = region.getV2();
			uvs[URX] = region.getU();
			uvs[BRY] = region.getV();
			if (rotated) {
				uvs[BLY] = region.getV();
				uvs[ULX] = region.getU2();
				uvs[URY] = region.getV2();
				uvs[BRX] = region.getU();
			} else {
				uvs[BLY] = region.getV2();
				uvs[ULX] = region.getU();
				uvs[URY] = region.getV();
				uvs[BRX] = region.getU2();
			}
		}
	}
}
