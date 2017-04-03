
package com.esotericsoftware.spine;

import com.badlogic.gdx.assets.AssetDescriptor;
import com.badlogic.gdx.assets.AssetLoaderParameters;
import com.badlogic.gdx.assets.AssetManager;
import com.badlogic.gdx.assets.loaders.AssetLoader;
import com.badlogic.gdx.assets.loaders.FileHandleResolver;
import com.badlogic.gdx.assets.loaders.SynchronousAssetLoader;
import com.badlogic.gdx.assets.loaders.BitmapFontLoader.BitmapFontParameter;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;

/** {@link AssetLoader} for {@link SkeletonData} instances. Loads the data file (.json) or (.bin) synchronously and loads the
 * {@link TextureAtlas} containing regions as a dependency. The {@link SkeletonLoaderParameter} allows you to set scale
 * @author Vojtìch Zdzieblo */
public class SkeletonDataLoader extends SynchronousAssetLoader<SkeletonData, SkeletonDataLoader.SkeletonLoaderParameter> {

	public SkeletonDataLoader (FileHandleResolver resolver) {
		super(resolver);
	}

	@Override
	public SkeletonData load (AssetManager assetManager, String fileName, FileHandle file, SkeletonLoaderParameter parameter) {
		TextureAtlas atlas = assetManager.get(file.parent() + "/" + file.nameWithoutExtension() + ".atlas", TextureAtlas.class);

		if (file.extension().equals("json")) {
			SkeletonJson json = new SkeletonJson(atlas);
			if (parameter != null) json.setScale(parameter.scale);
			return json.readSkeletonData(file);
		} else {
			SkeletonBinary binary = new SkeletonBinary(atlas);
			if (parameter != null) binary.setScale(parameter.scale);
			return binary.readSkeletonData(file);
		}
	}

	@Override
	public Array<AssetDescriptor> getDependencies (String fileName, FileHandle file, SkeletonLoaderParameter parameter) {
		Array<AssetDescriptor> dependencies = new Array();

		dependencies.add(new AssetDescriptor(file.parent() + "/" + file.nameWithoutExtension() + ".atlas", TextureAtlas.class));

		return dependencies;
	}

	static public class SkeletonLoaderParameter extends AssetLoaderParameters<SkeletonData> {
		public float scale = 1f;

		public SkeletonLoaderParameter (float scale) {
			this.scale = scale;
		}
	}
}
