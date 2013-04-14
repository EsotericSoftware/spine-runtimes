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
		public Format Format;
		public TextureFilter MinFilter;
		public TextureFilter MagFilter;
		public TextureWrap UWrap;
		public TextureWrap VWrap;
		public int TextureWidth;
		public int TextureHeight;
		public List<AtlasRegion> Regions;
		public Object Texture;

		public Atlas (String path, Object texture, int textureWidth, int textureHeight) {
			using (StreamReader reader = new StreamReader(path)) {
				try {
					initialize(reader, texture, textureWidth, textureHeight);
				} catch (Exception ex) {
					throw new Exception("Error reading atlas file: " + path, ex);
				}
			}
		}

		public Atlas (TextReader reader, Object texture, int textureWidth, int textureHeight) {
			initialize(reader, texture, textureWidth, textureHeight);
		}

		private void initialize (TextReader reader, Object texture, int textureWidth, int textureHeight) {
			TextureWidth = textureWidth;
			TextureHeight = textureHeight;
			Texture = texture;

			Regions = new List<AtlasRegion>();
			float invTexWidth = 1f / textureWidth;
			float invTexHeight = 1f / textureHeight;
			String[] tuple = new String[4];

			// Skip past first page name.
			while (true) {
				String line = reader.ReadLine();
				if (line.Trim().Length != 0)
					break;
			}

			Format = (Format)Enum.Parse(typeof(Format), readValue(reader), false);

			readTuple(reader, tuple);
			MinFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[0]);
			MagFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[1]);

			String direction = readValue(reader);
			UWrap = TextureWrap.ClampToEdge;
			VWrap = TextureWrap.ClampToEdge;
			if (direction == "x")
				UWrap = TextureWrap.Repeat;
			else if (direction == "y")
				VWrap = TextureWrap.Repeat;
			else if (direction == "xy")
				UWrap = VWrap = TextureWrap.Repeat;

			while (true) {
				String line = reader.ReadLine();
				if (line == null || line.Trim().Length == 0) break;

				AtlasRegion region = new AtlasRegion();
				region.Atlas = this;
				region.Name = line;

				region.Rotate = Boolean.Parse(readValue(reader));

				readTuple(reader, tuple);
				int x = int.Parse(tuple[0]);
				int y = int.Parse(tuple[1]);

				readTuple(reader, tuple);
				int width = int.Parse(tuple[0]);
				int height = int.Parse(tuple[1]);

				region.U = x * invTexWidth;
				region.V = y * invTexHeight;
				region.U2 = (x + width) * invTexWidth;
				region.V2 = (y + height) * invTexHeight;
				region.Width = Math.Abs(width);
				region.Height = Math.Abs(height);

				if (readTuple(reader, tuple) == 4) { // split is optional
					region.Splits = new int[] {int.Parse(tuple[0]), int.Parse(tuple[1]),
								int.Parse(tuple[2]), int.Parse(tuple[3])};

					if (readTuple(reader, tuple) == 4) { // pad is optional, but only present with splits
						region.Pads = new int[] {int.Parse(tuple[0]), int.Parse(tuple[1]),
									int.Parse(tuple[2]), int.Parse(tuple[3])};

						readTuple(reader, tuple);
					}
				}

				region.OriginalWidth = int.Parse(tuple[0]);
				region.OriginalHeight = int.Parse(tuple[1]);

				readTuple(reader, tuple);
				region.OffsetX = int.Parse(tuple[0]);
				region.OffsetY = int.Parse(tuple[1]);

				region.Index = int.Parse(readValue(reader));

				Regions.Add(region);
			}

			while (true) {
				String line = reader.ReadLine();
				if (line == null)
					break;
				if (line.Trim().Length != 0) throw new Exception("An atlas with multiple images is not supported.");
			}
		}

		static String readValue (TextReader reader) {
			String line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1)
				throw new Exception("Invalid line: " + line);
			return line.Substring(colon + 1).Trim();
		}

		/** Returns the number of tuple values read (2 or 4). */
		static int readTuple (TextReader reader, String[] tuple) {
			String line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1)
				throw new Exception("Invalid line: " + line);
			int i = 0, lastMatch = colon + 1;
			for (i = 0; i < 3; i++) {
				int comma = line.IndexOf(',', lastMatch);
				if (comma == -1) {
					if (i == 0)
						throw new Exception("Invalid line: " + line);
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
			for (int i = 0, n = Regions.Count; i < n; i++)
				if (Regions[i].Name == name)
					return Regions[i];
			return null;
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

	public class AtlasRegion {
		public Atlas Atlas;
		public float U, V;
		public float U2, V2;
		public int Width, Height;
		public int Index;
		public String Name;
		public float OffsetX, OffsetY;
		public int OriginalWidth, OriginalHeight;
		public bool Rotate;
		public int[] Splits;
		public int[] Pads;
	}
}
