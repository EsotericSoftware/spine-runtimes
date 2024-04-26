package com.esotericsoftware.spine.android;

import android.content.res.AssetManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.TextureAtlasData;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Null;

import java.io.BufferedInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.Buffer;

public class AndroidTextureAtlas {
    private static interface BitmapLoader {
        Bitmap load(String path);
    }

    private Array<AndroidTexture> textures = new Array<>();
    private Array<AtlasRegion> regions = new Array<>();
    private AndroidTextureAtlas(TextureAtlasData data, BitmapLoader bitmapLoader) {
        for (TextureAtlasData.Page page: data.getPages()) {
            page.texture = new AndroidTexture(bitmapLoader.load(page.textureFile.path()));
            textures.add((AndroidTexture) page.texture);
        }

        for (TextureAtlasData.Region region : data.getRegions()) {
            AtlasRegion atlasRegion = new AtlasRegion(region.page.texture, region.left, region.top, //
                    region.rotate ? region.height : region.width, //
                    region.rotate ? region.width : region.height);
            atlasRegion.index = region.index;
            atlasRegion.name = region.name;
            atlasRegion.offsetX = region.offsetX;
            atlasRegion.offsetY = region.offsetY;
            atlasRegion.originalHeight = region.originalHeight;
            atlasRegion.originalWidth = region.originalWidth;
            atlasRegion.rotate = region.rotate;
            atlasRegion.degrees = region.degrees;
            atlasRegion.names = region.names;
            atlasRegion.values = region.values;
            if (region.flip) atlasRegion.flip(false, true);
            regions.add(atlasRegion);
        }
    }

    /** Returns the first region found with the specified name. This method uses string comparison to find the region, so the
     * result should be cached rather than calling this method multiple times. */
    public @Null AtlasRegion findRegion (String name) {
        for (int i = 0, n = regions.size; i < n; i++)
            if (regions.get(i).name.equals(name)) return regions.get(i);
        return null;
    }

    public Array<AndroidTexture> getTextures() {
        return textures;
    }

    public Array<AtlasRegion> getRegions() {
        return regions;
    }

    static public AndroidTextureAtlas loadFromAssets(String atlasFile, AssetManager assetManager) {
        TextureAtlasData data = new TextureAtlasData();

        try {
            FileHandle inputFile = new FileHandle() {
                @Override
                public InputStream read() {
                    try {
                        return assetManager.open(atlasFile);
                    } catch (IOException e) {
                        throw new RuntimeException(e);
                    }
                }
            };
            data.load(inputFile, new FileHandle(atlasFile).parent(), false);
        } catch (Throwable t) {
            throw new RuntimeException(t);
        }

        return new AndroidTextureAtlas(data, new BitmapLoader() {
            @Override
            public Bitmap load(String path) {
                path = path.startsWith("/") ? path.substring(1) : path;
                try (InputStream in = new BufferedInputStream(assetManager.open(path))) {
                    return BitmapFactory.decodeStream(in);
                } catch (Throwable t) {
                    throw new RuntimeException(t);
                }
            }
        });
    }
}
