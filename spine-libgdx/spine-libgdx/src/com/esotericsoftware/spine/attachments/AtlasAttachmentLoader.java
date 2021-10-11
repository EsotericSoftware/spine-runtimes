/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

/** An {@link AttachmentLoader} that configures attachments using texture regions from an {@link Atlas}.
 * <p>
 * See <a href='http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data'>Loading skeleton data</a> in the
 * Spine Runtimes Guide. */
@SuppressWarnings("javadoc")
public class AtlasAttachmentLoader implements AttachmentLoader {
	private TextureAtlas atlas;

	public AtlasAttachmentLoader (TextureAtlas atlas) {
		if (atlas == null) throw new IllegalArgumentException("atlas cannot be null.");
		this.atlas = atlas;
	}

	private void loadSequence (String name, String basePath, Sequence sequence) {
		TextureRegion[] regions = sequence.getRegions();
		for (int i = 0, n = regions.length; i < n; i++) {
			String path = sequence.getPath(basePath, i);
			regions[i] = atlas.findRegion(path);
			if (regions[i] == null) throw new RuntimeException("Region not found in atlas: " + path + " (sequence: " + name + ")");
		}
	}

	public RegionAttachment newRegionAttachment (Skin skin, String name, String path, @Null Sequence sequence) {
		RegionAttachment attachment = new RegionAttachment(name);
		if (sequence != null)
			loadSequence(name, path, sequence);
		else {
			AtlasRegion region = atlas.findRegion(path);
			if (region == null)
				throw new RuntimeException("Region not found in atlas: " + path + " (region attachment: " + name + ")");
			attachment.setRegion(region);
		}
		return attachment;
	}

	public MeshAttachment newMeshAttachment (Skin skin, String name, String path, @Null Sequence sequence) {
		MeshAttachment attachment = new MeshAttachment(name);
		if (sequence != null)
			loadSequence(name, path, sequence);
		else {
			AtlasRegion region = atlas.findRegion(path);
			if (region == null)
				throw new RuntimeException("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
			attachment.setRegion(region);
		}
		return attachment;
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
