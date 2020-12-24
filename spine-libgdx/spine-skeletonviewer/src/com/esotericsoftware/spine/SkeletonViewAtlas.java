
package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.SkeletonViewer.*;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Pixmap;
import com.badlogic.gdx.graphics.Pixmap.Format;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.Texture.TextureFilter;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.graphics.g2d.TextureAtlas.TextureAtlasData.Page;
import com.badlogic.gdx.utils.Null;

class SkeletonViewAtlas extends TextureAtlas {
	private final SkeletonViewer viewer;
	private @Null FileHandle atlasFile;
	private final AtlasRegion fake;

	public SkeletonViewAtlas (final SkeletonViewer viewer, @Null FileHandle skeletonFile) {
		this.viewer = viewer;

		atlasFile = findAtlasFile(skeletonFile);
		if (atlasFile != null) {
			final TextureAtlasData atlasData = new TextureAtlasData(atlasFile, atlasFile.parent(), false);
			Gdx.app.postRunnable(new Runnable() {
				public void run () {
					boolean linear = true, pma = false;
					for (int i = 0, n = atlasData.getPages().size; i < n; i++) {
						Page page = atlasData.getPages().get(i);
						if (page.pma) pma = true;
						if (page.minFilter != TextureFilter.Linear || page.magFilter != TextureFilter.Linear) {
							linear = false;
							break;
						}
					}
					viewer.ui.linearCheckbox.setChecked(linear);
					viewer.ui.pmaCheckbox.setChecked(pma);
				}
			});
			try {
				load(atlasData);
			} catch (Throwable ex) {
				System.out.println("Error loading atlas: " + atlasFile.file().getAbsolutePath());
				ex.printStackTrace();
				viewer.ui.toast("Error loading atlas: " + atlasFile.name());
				atlasFile = null;
			}
		}

		// Setup a texture atlas that uses a white image for images not found in the atlas.
		Pixmap pixmap = new Pixmap(32, 32, Format.RGBA8888);
		pixmap.setColor(new Color(1, 1, 1, 0.33f));
		pixmap.fill();
		fake = new AtlasRegion(new Texture(pixmap), 0, 0, 32, 32);
		pixmap.dispose();
	}

	private @Null FileHandle findAtlasFile (FileHandle skeletonFile) {
		String baseName = skeletonFile.name();
		for (String startSuffix : startSuffixes) {
			for (String endSuffix : endSuffixes) {
				for (String dataSuffix : dataSuffixes) {
					String suffix = startSuffix + dataSuffix + endSuffix;
					if (baseName.endsWith(suffix)) {
						FileHandle file = findAtlasFile(skeletonFile, baseName.substring(0, baseName.length() - suffix.length()));
						if (file != null) return file;
					}
				}
			}
		}
		return findAtlasFile(skeletonFile, baseName);
	}

	private @Null FileHandle findAtlasFile (FileHandle skeletonFile, String baseName) {
		for (String startSuffix : startSuffixes) {
			for (String endSuffix : endSuffixes) {
				for (String suffix : atlasSuffixes) {
					FileHandle file = skeletonFile.sibling(baseName + startSuffix + suffix + endSuffix);
					if (file.exists()) return file;
				}
			}
		}
		return null;
	}

	public AtlasRegion findRegion (String name) {
		AtlasRegion region = super.findRegion(name);
		if (region == null) {
			// Look for separate image file.
			FileHandle file = viewer.skeletonFile.sibling(name + ".png");
			if (file.exists()) {
				Texture texture = new Texture(file);
				texture.setFilter(TextureFilter.Linear, TextureFilter.Linear);
				region = new AtlasRegion(texture, 0, 0, texture.getWidth(), texture.getHeight());
				region.name = name;
			}
		}
		return region != null ? region : fake;
	}

	public long lastModified () {
		return atlasFile == null ? 0 : atlasFile.lastModified();
	}
}
