using System;
using System.Collections.Generic;
using System.IO;

namespace Spine {
	abstract public class BaseAtlas {
		List<AtlasPage> pages = new List<AtlasPage>();
		List<AtlasRegion> regions = new List<AtlasRegion>();

		abstract protected AtlasPage NewAtlasPage (String path);

		public void load (StreamReader reader, String imagesDir) {
			String[] tuple = new String[4];
			AtlasPage page = null;
			while (true) {
				String line = reader.ReadLine();
				if (line == null) break;
				if (line.Trim().Length == 0)
					page = null;
				else if (page == null) {
					page = NewAtlasPage(Path.Combine(imagesDir, line));

					page.Format = (Format)Enum.Parse(typeof(Format), readValue(reader), false);

					readTuple(reader, tuple);
					page.MinFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[0]);
					page.MagFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[1]);

					String direction = readValue(reader);
					page.UWrap = TextureWrap.ClampToEdge;
					page.VWrap = TextureWrap.ClampToEdge;
					if (direction == "x")
						page.UWrap = TextureWrap.Repeat;
					else if (direction == "y")
						page.VWrap = TextureWrap.Repeat;
					else if (direction == "xy")
						page.UWrap = page.VWrap = TextureWrap.Repeat;

					pages.Add(page);

				} else {
					AtlasRegion region = new AtlasRegion();
					region.Name = line;
					region.Page = page;

					region.Rotate = Boolean.Parse(readValue(reader));

					readTuple(reader, tuple);
					int x = int.Parse(tuple[0]);
					int y = int.Parse(tuple[1]);

					readTuple(reader, tuple);
					int width = int.Parse(tuple[0]);
					int height = int.Parse(tuple[1]);

					float invTexWidth = 1f / page.GetTextureWidth();
					float invTexHeight = 1f / page.GetTextureHeight();
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

					regions.Add(region);
				}
			}
		}

		static String readValue (StreamReader reader) {
			String line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1) throw new Exception("Invalid line: " + line);
			return line.Substring(colon + 1).Trim();
		}

		/** Returns the number of tuple values read (2 or 4). */
		static int readTuple (StreamReader reader, String[] tuple) {
			String line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1) throw new Exception("Invalid line: " + line);
			int i = 0, lastMatch = colon + 1;
			for (i = 0; i < 3; i++) {
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
				if (regions[i].Name == name) return regions[i];
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

	abstract public class AtlasPage {
		public Format Format;
		public TextureFilter MinFilter;
		public TextureFilter MagFilter;
		public TextureWrap UWrap;
		public TextureWrap VWrap;

		abstract public int GetTextureWidth ();
		abstract public int GetTextureHeight ();
	}

	public class AtlasRegion {
		public AtlasPage Page;
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
