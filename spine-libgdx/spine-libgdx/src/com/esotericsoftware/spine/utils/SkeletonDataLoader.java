/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.assets.AssetDescriptor;
import com.badlogic.gdx.assets.AssetLoaderParameters;
import com.badlogic.gdx.assets.AssetManager;
import com.badlogic.gdx.assets.loaders.AsynchronousAssetLoader;
import com.badlogic.gdx.assets.loaders.FileHandleResolver;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;

import com.esotericsoftware.spine.SkeletonBinary;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.SkeletonJson;
import com.esotericsoftware.spine.attachments.AtlasAttachmentLoader;
import com.esotericsoftware.spine.attachments.AttachmentLoader;

/** An asset loader to create and load skeleton data. The data file is assumed to be binary if it ends with <code>.skel</code>,
 * otherwise JSON is assumed. The {@link SkeletonDataParameter} can provide a texture atlas name or an {@link AttachmentLoader}.
 * If neither is provided, a texture atlas name based on the skeleton file name with an <code>.atlas</code> extension is used.
 * When a texture atlas name is used, the texture atlas is loaded by the asset manager as a dependency.
 * <p>
 * Example:
 * 
 * <pre>
 * // Load skeleton.json and skeleton.atlas:
 * assetManager.load("skeleton.json", SkeletonData.class);
 * // Or specify the atlas/AttachmentLoader and scale:
 * assetManager.setLoader(SkeletonData.class, new SkeletonDataLoader(new InternalFileHandleResolver()));
 * SkeletonDataParameter parameter = new SkeletonDataParameter("skeleton2x.atlas", 2);
 * assetManager.load("skeleton.json", SkeletonData.class, parameter);
 * </pre>
 */
public class SkeletonDataLoader extends AsynchronousAssetLoader<SkeletonData, SkeletonDataLoader.SkeletonDataParameter> {
	private SkeletonData skeletonData;

	public SkeletonDataLoader (FileHandleResolver resolver) {
		super(resolver);
	}

	/** @param parameter May be null. */
	public void loadAsync (AssetManager manager, String fileName, FileHandle file, SkeletonDataParameter parameter) {
		float scale = 1;
		AttachmentLoader attachmentLoader = null;
		if (parameter != null) {
			scale = parameter.scale;
			if (parameter.attachmentLoader != null)
				attachmentLoader = parameter.attachmentLoader;
			else if (parameter.atlasName != null)
				attachmentLoader = new AtlasAttachmentLoader(manager.get(parameter.atlasName, TextureAtlas.class));
		}
		if (attachmentLoader == null)
			attachmentLoader = new AtlasAttachmentLoader(manager.get(file.pathWithoutExtension() + ".atlas", TextureAtlas.class));

		if (file.extension().equalsIgnoreCase("skel")) {
			SkeletonBinary skeletonBinary = new SkeletonBinary(attachmentLoader);
			skeletonBinary.setScale(scale);
			skeletonData = skeletonBinary.readSkeletonData(file);
		} else {
			SkeletonJson skeletonJson = new SkeletonJson(attachmentLoader);
			skeletonJson.setScale(scale);
			skeletonData = skeletonJson.readSkeletonData(file);
		}
	}

	/** @param parameter May be null. */
	public SkeletonData loadSync (AssetManager manager, String fileName, FileHandle file, SkeletonDataParameter parameter) {
		SkeletonData skeletonData = this.skeletonData;
		this.skeletonData = null;
		return skeletonData;
	}

	/** @param parameter May be null. */
	public Array<AssetDescriptor> getDependencies (String fileName, FileHandle file, SkeletonDataParameter parameter) {
		if (parameter == null) return null;
		if (parameter.attachmentLoader != null) return null;
		Array<AssetDescriptor> dependencies = new Array();
		dependencies.add(new AssetDescriptor(parameter.atlasName, TextureAtlas.class));
		return dependencies;
	}

	static public class SkeletonDataParameter extends AssetLoaderParameters<SkeletonData> {
		public String atlasName;
		public AttachmentLoader attachmentLoader;
		public float scale = 1;

		public SkeletonDataParameter () {
		}

		public SkeletonDataParameter (String atlasName) {
			this.atlasName = atlasName;
		}

		public SkeletonDataParameter (String atlasName, float scale) {
			this.atlasName = atlasName;
			this.scale = scale;
		}

		public SkeletonDataParameter (AttachmentLoader attachmentLoader) {
			this.attachmentLoader = attachmentLoader;
		}

		public SkeletonDataParameter (AttachmentLoader attachmentLoader, float scale) {
			this.attachmentLoader = attachmentLoader;
			this.scale = scale;
		}
	}
}
