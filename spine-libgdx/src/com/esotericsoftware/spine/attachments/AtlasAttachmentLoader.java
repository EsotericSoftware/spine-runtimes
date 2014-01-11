/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.esotericsoftware.spine.Skin;

import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;

public class AtlasAttachmentLoader implements AttachmentLoader {
	private TextureAtlas atlas;

	public AtlasAttachmentLoader (TextureAtlas atlas) {
		if (atlas == null) throw new IllegalArgumentException("atlas cannot be null.");
		this.atlas = atlas;
	}

	public RegionAttachment newRegionAttachment (Skin skin, String name, String path) {
		RegionAttachment attachment = new RegionAttachment(name);
		attachment.setPath(path);
		AtlasRegion region = atlas.findRegion(path);
		if (region == null)
			throw new RuntimeException("Region not found in atlas: " + attachment + " (region attachment: " + name + ")");
		attachment.setRegion(region);
		return attachment;
	}

	public MeshAttachment newMeshAttachment (Skin skin, String name, String path) {
		MeshAttachment attachment = new MeshAttachment(name);
		attachment.setPath(path);
		AtlasRegion region = atlas.findRegion(path);
		if (region == null)
			throw new RuntimeException("Region not found in atlas: " + attachment + " (region attachment: " + name + ")");
		attachment.setRegion(region);
		return attachment;
	}

	public BoundingBoxAttachment newBoundingBoxAttachment (Skin skin, String name) {
		return new BoundingBoxAttachment(name);
	}
}
