/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
