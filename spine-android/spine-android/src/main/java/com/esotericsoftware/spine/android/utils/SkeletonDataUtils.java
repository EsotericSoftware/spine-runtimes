package com.esotericsoftware.spine.android.utils;

import android.content.Context;
import android.content.res.AssetManager;

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

import kotlin.NotImplementedError;

public class SkeletonDataUtils {

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
    public static SkeletonData fromFile(AndroidTextureAtlas atlas, File skeletonFile) {
        throw new NotImplementedError("TODO");
    }

    public static SkeletonData fromHttp(AndroidTextureAtlas atlas, URL skeletonUrl) {
        throw new NotImplementedError("TODO");
    }
}
