/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace Spine {
	public class Atlas {
		List<AtlasPage> pages = new List<AtlasPage>();
		List<AtlasRegion> regions = new List<AtlasRegion>();
		TextureLoader textureLoader;

		public Atlas (String path, TextureLoader textureLoader) {
			using (StreamReader reader = new StreamReader(path)) {
				try {
					Load(reader, Path.GetDirectoryName(path), textureLoader);
				} catch (Exception ex) {
					throw new Exception("Error reading atlas file: " + path, ex);
				}
			}
		}

		public Atlas (TextReader reader, String dir, TextureLoader textureLoader) {
			Load(reader, dir, textureLoader);
		}

		private void Load (TextReader reader, String imagesDir, TextureLoader textureLoader) {
			if (textureLoader == null) throw new ArgumentNullException("textureLoader cannot be null.");
			this.textureLoader = textureLoader;

			String[] tuple = new String[4];
			AtlasPage page = null;
			while (true) {
				String line = reader.ReadLine();
				if (line == null) break;
				if (line.Trim().Length == 0)
					page = null;
				else if (page == null) {
					page = new AtlasPage();
					page.name = line;

					page.format = (Format)Enum.Parse(typeof(Format), readValue(reader), false);

					readTuple(reader, tuple);
					page.minFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[0]);
					page.magFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[1]);

					String direction = readValue(reader);
					page.uWrap = TextureWrap.ClampToEdge;
					page.vWrap = TextureWrap.ClampToEdge;
					if (direction == "x")
						page.uWrap = TextureWrap.Repeat;
					else if (direction == "y")
						page.vWrap = TextureWrap.Repeat;
					else if (direction == "xy")
						page.uWrap = page.vWrap = TextureWrap.Repeat;

					textureLoader.Load(page, Path.Combine(imagesDir, line));

					pages.Add(page);

				} else {
					AtlasRegion region = new AtlasRegion();
					region.name = line;
					region.page = page;

					region.rotate = Boolean.Parse(readValue(reader));

					readTuple(reader, tuple);
					int x = int.Parse(tuple[0]);
					int y = int.Parse(tuple[1]);

					readTuple(reader, tuple);
					int width = int.Parse(tuple[0]);
					int height = int.Parse(tuple[1]);

					region.u = x / (float)page.width;
					region.v = y / (float)page.height;
					if (region.rotate) {
						region.u2 = (x + height) / (float)page.width;
						region.v2 = (y + width) / (float)page.height;
					} else {
						region.u2 = (x + width) / (float)page.width;
						region.v2 = (y + height) / (float)page.height;
					}
					region.x = x;
					region.y = y;
					region.width = Math.Abs(width);
					region.height = Math.Abs(height);

					if (readTuple(reader, tuple) == 4) { // split is optional
						region.splits = new int[] {int.Parse(tuple[0]), int.Parse(tuple[1]),
								int.Parse(tuple[2]), int.Parse(tuple[3])};

						if (readTuple(reader, tuple) == 4) { // pad is optional, but only present with splits
							region.pads = new int[] {int.Parse(tuple[0]), int.Parse(tuple[1]),
									int.Parse(tuple[2]), int.Parse(tuple[3])};

							readTuple(reader, tuple);
						}
					}

					region.originalWidth = int.Parse(tuple[0]);
					region.originalHeight = int.Parse(tuple[1]);

					readTuple(reader, tuple);
					region.offsetX = int.Parse(tuple[0]);
					region.offsetY = int.Parse(tuple[1]);

					region.index = int.Parse(readValue(reader));

					regions.Add(region);
				}
			}
		}

		static String readValue (TextReader reader) {
			String line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1) throw new Exception("Invalid line: " + line);
			return line.Substring(colon + 1).Trim();
		}

		/** Returns the number of tuple values read (2 or 4). */
		static int readTuple (TextReader reader, String[] tuple) {
			String line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1) throw new Exception("Invalid line: " + line);
			int i = 0, lastMatch = colon + 1;
			for (; i < 3; i++) {
				int comma = line.IndexOf(',', lastMatch);
				if (comma == -1) {
					if (i == 0) throw new Exception("Invalid line: " + line);
					break;
				}
				tuple[i] = line.Substring(lastMatch, comma - lastMatch).Trim();
				lastMatch = comma + 1;
			}
			tuple[i] = line.Substring(lastMatch).Trim();
			return i + 1;
		}

		/** Returns the first region found with the specified name. This method uses string comparison to find the region, so the result
		 * should be cached rather than calling this method multiple times.
		 * @return The region, or null. */
		public AtlasRegion FindRegion (String name) {
			for (int i = 0, n = regions.Count; i < n; i++)
				if (regions[i].name == name) return regions[i];
			return null;
		}

		public void Dispose () {
			for (int i = 0, n = pages.Count; i < n; i++)
				textureLoader.Unload(pages[i].texture);
		}
	}

	public enum Format {
		Alpha,
		Intensity,
		LuminanceAlpha,
		RGB565,
		RGBA4444,
		RGB888,
		RGBA8888
	}

	public enum TextureFilter {
		Nearest,
		Linear,
		MipMap,
		MipMapNearestNearest,
		MipMapLinearNearest,
		MipMapNearestLinear,
		MipMapLinearLinear
	}

	public enum TextureWrap {
		MirroredRepeat,
		ClampToEdge,
		Repeat
	}

	public class AtlasPage {
		public String name;
		public Format format;
		public TextureFilter minFilter;
		public TextureFilter magFilter;
		public TextureWrap uWrap;
		public TextureWrap vWrap;
		public Object texture;
		public int width, height;
	}

	public class AtlasRegion {
		public AtlasPage page;
		public String name;
		public int x, y, width, height;
		public float u, v, u2, v2;
		public float offsetX, offsetY;
		public int originalWidth, originalHeight;
		public int index;
		public bool rotate;
		public int[] splits;
		public int[] pads;
	}

	public interface TextureLoader {
		void Load (AtlasPage page, String path);
		void Unload (Object texture);
	}
}
