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

import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.utils.Null;

import com.esotericsoftware.spine.Skin;

/** An {@link AttachmentLoader} that configures attachments using texture regions from an {@link TextureAtlas}.
 * <p>
 * See <a href='https://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data'>Loading skeleton data</a> in the
 * Spine Runtimes Guide. */
@SuppressWarnings("javadoc")
public class AtlasAttachmentLoader implements AttachmentLoader {
	private TextureAtlas atlas;

	/** If true, {@link #findRegion(String, String)} may return null. If false, an error is raised if the texture region is not
	 * found. Default is false. */
	public boolean allowMissingRegions;

	public AtlasAttachmentLoader (TextureAtlas atlas) {
		this(atlas, false);
	}

	public AtlasAttachmentLoader (TextureAtlas atlas, boolean allowMissingRegions) {
		if (atlas == null) throw new IllegalArgumentException("atlas cannot be null.");
		this.atlas = atlas;
		this.allowMissingRegions = allowMissingRegions;
	}

	/** Sets each {@link Sequence#regions} by calling {@link #findRegion(String, String)} for each texture region using
	 * {@link Sequence#getPath(String, int)}. */
	protected void findRegions (String name, String basePath, Sequence sequence) {
		TextureRegion[] regions = sequence.getRegions();
		for (int i = 0, n = regions.length; i < n; i++)
			regions[i] = findRegion(name, sequence.getPath(basePath, i));
	}

	/** Looks for the region with the specified path. If not found and {@link #allowMissingRegions} is false, an error is
	 * raised. */
	protected @Null AtlasRegion findRegion (String name, String path) {
		AtlasRegion region = atlas.findRegion(path);
		if (region == null && !allowMissingRegions)
			throw new RuntimeException("Region not found in atlas: " + path + " (attachment: " + name + ")");
		return region;
	}

	public RegionAttachment newRegionAttachment (Skin skin, String name, String path, Sequence sequence) {
		findRegions(name, path, sequence);
		return new RegionAttachment(name, sequence);
	}

	public MeshAttachment newMeshAttachment (Skin skin, String name, String path, Sequence sequence) {
		findRegions(name, path, sequence);
		return new MeshAttachment(name, sequence);
	}

	public BoundingBoxAttachment newBoundingBoxAttachment (Skin skin, String name) {
		return new BoundingBoxAttachment(name);
	}

	public ClippingAttachment newClippingAttachment (Skin skin, String name) {
		return new ClippingAttachment(name);
	}

	public PathAttachment newPathAttachment (Skin skin, String name) {
		return new PathAttachment(name);
	}

	public PointAttachment newPointAttachment (Skin skin, String name) {
		return new PointAttachment(name);
	}
}
