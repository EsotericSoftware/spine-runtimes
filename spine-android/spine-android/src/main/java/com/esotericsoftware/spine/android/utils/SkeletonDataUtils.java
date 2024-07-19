package com.esotericsoftware.spine.android.utils;

import android.content.Context;
import android.content.res.AssetManager;

import com.badlogic.gdx.files.FileHandle;
import com.esotericsoftware.spine.SkeletonBinary;
import com.esotericsoftware.spine.SkeletonData;
import com.esotericsoftware.spine.SkeletonJson;
import com.esotericsoftware.spine.SkeletonLoader;
import com.esotericsoftware.spine.android.AndroidAtlasAttachmentLoader;
import com.esotericsoftware.spine.android.AndroidTextureAtlas;

import java.io.BufferedInputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;

/**
 * Helper to load {@link SkeletonData} from assets.
 */
public class SkeletonDataUtils {

    /**
     * Loads a {@link SkeletonData} from the file {@code skeletonFile} in assets using {@link Context}.
     * Uses the provided {@link AndroidTextureAtlas} to resolve attachment images.
     *
     * Throws a {@link RuntimeException} in case the skeleton data could not be loaded.
     */
    public static SkeletonData fromAsset(AndroidTextureAtlas atlas, String skeletonFileName, Context context) {
        AndroidAtlasAttachmentLoader attachmentLoader = new AndroidAtlasAttachmentLoader(atlas);

        SkeletonLoader skeletonLoader;
        if (skeletonFileName.endsWith(".json")) {
            skeletonLoader = new SkeletonJson(attachmentLoader);
        } else {
            skeletonLoader = new SkeletonBinary(attachmentLoader);
        }

        SkeletonData skeletonData;

        AssetManager assetManager = context.getAssets();
        try (InputStream in = new BufferedInputStream(assetManager.open(skeletonFileName))) {
            skeletonData = skeletonLoader.readSkeletonData(in);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
        return skeletonData;
    }

    /**
     * Loads a {@link SkeletonData} from the file {@code skeletonFile}. Uses the provided {@link AndroidTextureAtlas} to resolve attachment images.
     *
     * Throws a {@link RuntimeException} in case the skeleton data could not be loaded.
     */
    public static SkeletonData fromFile(AndroidTextureAtlas atlas, File skeletonFile) {
        AndroidAtlasAttachmentLoader attachmentLoader = new AndroidAtlasAttachmentLoader(atlas);

        SkeletonLoader skeletonLoader;
        if (skeletonFile.getPath().endsWith(".json")) {
            skeletonLoader = new SkeletonJson(attachmentLoader);
        } else {
            skeletonLoader = new SkeletonBinary(attachmentLoader);
        }

        return skeletonLoader.readSkeletonData(new FileHandle(skeletonFile));
    }

    /**
     * Loads a {@link SkeletonData} from the URL {@code skeletonURL}. Uses the provided {@link AndroidTextureAtlas} to resolve attachment images.
     *
     * Throws a {@link RuntimeException} in case the skeleton data could not be loaded.
     */
    public static SkeletonData fromHttp(AndroidTextureAtlas atlas, URL skeletonUrl, File targetDirectory) {
        File skeletonFile = HttpUtils.downloadFrom(skeletonUrl, targetDirectory);
        return fromFile(atlas, skeletonFile);
    }
}
