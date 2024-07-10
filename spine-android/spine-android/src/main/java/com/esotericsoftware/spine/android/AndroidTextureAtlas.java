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

package com.esotericsoftware.spine.android;

import java.io.BufferedInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.util.List;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.AtlasRegion;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.TextureAtlasData;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Null;

import android.content.Context;
import android.content.res.AssetManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;

import kotlin.NotImplementedError;

public class AndroidTextureAtlas {
	private static interface BitmapLoader {
		Bitmap load (String path);
	}

	private Array<AndroidTexture> textures = new Array<>();
	private Array<AtlasRegion> regions = new Array<>();

	private AndroidTextureAtlas (TextureAtlasData data, BitmapLoader bitmapLoader) {
		for (TextureAtlasData.Page page : data.getPages()) {
			page.texture = new AndroidTexture(bitmapLoader.load(page.textureFile.path()));
			textures.add((AndroidTexture)page.texture);
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

	public Array<AndroidTexture> getTextures () {
		return textures;
	}

	public Array<AtlasRegion> getRegions () {
		return regions;
	}

	static public AndroidTextureAtlas fromAsset(String atlasFileName, Context context) {
		TextureAtlasData data = new TextureAtlasData();
		AssetManager assetManager = context.getAssets();

		try {
			FileHandle inputFile = new FileHandle() {
				@Override
				public InputStream read () {
					try {
						return assetManager.open(atlasFileName);
					} catch (IOException e) {
						throw new RuntimeException(e);
					}
				}
			};
			data.load(inputFile, new FileHandle(atlasFileName).parent(), false);
		} catch (Throwable t) {
			throw new RuntimeException(t);
		}

		return new AndroidTextureAtlas(data, path -> {
            path = path.startsWith("/") ? path.substring(1) : path;
            try (InputStream in = new BufferedInputStream(assetManager.open(path))) {
                return BitmapFactory.decodeStream(in);
            } catch (Throwable t) {
                throw new RuntimeException(t);
            }
        });
	}

	static public AndroidTextureAtlas fromFile(File atlasFile) {
		TextureAtlasData data = new TextureAtlasData();

		try {
			FileHandle inputFile = new FileHandle() {
				@Override
				public InputStream read() {
					try {
						return new FileInputStream(atlasFile);
					} catch (FileNotFoundException e) {
						throw new RuntimeException(e);
					}
				}
			};
			data.load(inputFile, new FileHandle(atlasFile).parent(), false);
		} catch (Throwable t) {
			throw new RuntimeException(t);
		}

		return new AndroidTextureAtlas(data, path -> {
			File imageFile = new File(path);
			try (InputStream in = new BufferedInputStream(new FileInputStream(imageFile))) {
				return BitmapFactory.decodeStream(in);
			} catch (Throwable t) {
				throw new RuntimeException(t);
			}
		});
	}

	static public AndroidTextureAtlas fromHttp(URL atlasUrl) {
		throw new NotImplementedError("TODO");
	}
}
