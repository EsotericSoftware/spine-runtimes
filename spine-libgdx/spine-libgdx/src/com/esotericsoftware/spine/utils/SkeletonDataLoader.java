package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.assets.AssetDescriptor;
import com.badlogic.gdx.assets.AssetLoaderParameters;
import com.badlogic.gdx.assets.AssetManager;
import com.badlogic.gdx.assets.loaders.AsynchronousAssetLoader;
import com.badlogic.gdx.assets.loaders.FileHandleResolver;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.SkeletonJson;

/**
 * {@link com.badlogic.gdx.assets.loaders.AssetLoader} for {@link SkeletonData} instances.
 * All {@link com.badlogic.gdx.graphics.Texture} instances will be loaded asdependencies.
 * Passing a {@link SkeletonDataParameter} allows to set scale for SkeletonJson.
 *
 * @author AyoCrazy
 */

public class SkeletonDataLoader extends AsynchronousAssetLoader<SkeletonData, SkeletonDataLoader.SkeletonDataParameter> {
    private SkeletonData skeletonData;

    public SkeletonDataLoader(FileHandleResolver resolver) {
        super(resolver);
    }

    @Override
    public void loadAsync(AssetManager manager, String fileName, FileHandle file, SkeletonDataLoader.SkeletonDataParameter parameter) {
        TextureAtlas atlas = manager.get(file.pathWithoutExtension() + ".atlas", TextureAtlas.class);
        SkeletonJson sJson = new SkeletonJson(atlas);
        if (parameter != null)
            sJson.setScale(parameter.scale);
        skeletonData = sJson.readSkeletonData(file);
    }

    @Override
    public SkeletonData loadSync(AssetManager manager, String fileName, FileHandle file, SkeletonDataLoader.SkeletonDataParameter parameter) {
        SkeletonData skeletonData = this.skeletonData;
        this.skeletonData = null;
        return skeletonData;
    }

    @Override
    public Array<AssetDescriptor> getDependencies(String fileName, FileHandle file, SkeletonDataLoader.SkeletonDataParameter parameter) {
        Array<AssetDescriptor> deps = new Array();
        deps.add(new AssetDescriptor(file.pathWithoutExtension() + ".atlas", TextureAtlas.class));
        return deps;
    }

    static public class SkeletonDataParameter extends AssetLoaderParameters<SkeletonData> {
        float scale = 1f;
        
        public SkeletonDataParameter(float scale) {
            this.scale = scale;
        }
    }
}
