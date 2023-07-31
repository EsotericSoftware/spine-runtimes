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

#if (UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_WSA || UNITY_WP8 || UNITY_WP8_1)
#define IS_UNITY
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {
	public class Atlas : IEnumerable<AtlasRegion> {
		readonly List<AtlasPage> pages = new List<AtlasPage>();
		List<AtlasRegion> regions = new List<AtlasRegion>();
		TextureLoader textureLoader;

		#region IEnumerable implementation
		public IEnumerator<AtlasRegion> GetEnumerator () {
			return regions.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
			return regions.GetEnumerator();
		}
		#endregion

		public List<AtlasRegion> Regions { get { return regions; } }
		public List<AtlasPage> Pages { get { return pages; } }

#if !(IS_UNITY)
#if WINDOWS_STOREAPP
		private async Task ReadFile (string path, TextureLoader textureLoader) {
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			var file = await folder.GetFileAsync(path).AsTask().ConfigureAwait(false);
			using (StreamReader reader = new StreamReader(await file.OpenStreamForReadAsync().ConfigureAwait(false))) {
				try {
					Atlas atlas = new Atlas(reader, Path.GetDirectoryName(path), textureLoader);
					this.pages = atlas.pages;
					this.regions = atlas.regions;
					this.textureLoader = atlas.textureLoader;
				} catch (Exception ex) {
					throw new Exception("Error reading atlas file: " + path, ex);
				}
			}
		}

		public Atlas (string path, TextureLoader textureLoader) {
			this.ReadFile(path, textureLoader).Wait();
		}
#else
		public Atlas (string path, TextureLoader textureLoader) {
#if WINDOWS_PHONE
			Stream stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(path);
			using (StreamReader reader = new StreamReader(stream)) {
#else
			using (StreamReader reader = new StreamReader(path)) {
#endif // WINDOWS_PHONE
				try {
					Atlas atlas = new Atlas(reader, Path.GetDirectoryName(path), textureLoader);
					this.pages = atlas.pages;
					this.regions = atlas.regions;
					this.textureLoader = atlas.textureLoader;
				} catch (Exception ex) {
					throw new Exception("Error reading atlas file: " + path, ex);
				}
			}
		}
#endif // WINDOWS_STOREAPP
#endif

		public Atlas (List<AtlasPage> pages, List<AtlasRegion> regions) {
			if (pages == null) throw new ArgumentNullException("pages", "pages cannot be null.");
			if (regions == null) throw new ArgumentNullException("regions", "regions cannot be null.");
			this.pages = pages;
			this.regions = regions;
			this.textureLoader = null;
		}

		public Atlas (TextReader reader, string imagesDir, TextureLoader textureLoader) {
			if (reader == null) throw new ArgumentNullException("reader", "reader cannot be null.");
			if (imagesDir == null) throw new ArgumentNullException("imagesDir", "imagesDir cannot be null.");
			if (textureLoader == null) throw new ArgumentNullException("textureLoader", "textureLoader cannot be null.");
			this.textureLoader = textureLoader;

			string[] entry = new string[5];
			AtlasPage page = null;
			AtlasRegion region = null;

			Dictionary<string, Action> pageFields = new Dictionary<string, Action>(5);
			pageFields.Add("size", () => {
				page.width = int.Parse(entry[1], CultureInfo.InvariantCulture);
				page.height = int.Parse(entry[2], CultureInfo.InvariantCulture);
			});
			pageFields.Add("format", () => {
				page.format = (Format)Enum.Parse(typeof(Format), entry[1], false);
			});
			pageFields.Add("filter", () => {
				page.minFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), entry[1], false);
				page.magFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), entry[2], false);
			});
			pageFields.Add("repeat", () => {
				if (entry[1].IndexOf('x') != -1) page.uWrap = TextureWrap.Repeat;
				if (entry[1].IndexOf('y') != -1) page.vWrap = TextureWrap.Repeat;
			});
			pageFields.Add("pma", () => {
				page.pma = entry[1] == "true";
			});

			Dictionary<string, Action> regionFields = new Dictionary<string, Action>(8);
			regionFields.Add("xy", () => { // Deprecated, use bounds.
				region.x = int.Parse(entry[1], CultureInfo.InvariantCulture);
				region.y = int.Parse(entry[2], CultureInfo.InvariantCulture);
			});
			regionFields.Add("size", () => { // Deprecated, use bounds.
				region.width = int.Parse(entry[1], CultureInfo.InvariantCulture);
				region.height = int.Parse(entry[2], CultureInfo.InvariantCulture);
			});
			regionFields.Add("bounds", () => {
				region.x = int.Parse(entry[1], CultureInfo.InvariantCulture);
				region.y = int.Parse(entry[2], CultureInfo.InvariantCulture);
				region.width = int.Parse(entry[3], CultureInfo.InvariantCulture);
				region.height = int.Parse(entry[4], CultureInfo.InvariantCulture);
			});
			regionFields.Add("offset", () => { // Deprecated, use offsets.
				region.offsetX = int.Parse(entry[1], CultureInfo.InvariantCulture);
				region.offsetY = int.Parse(entry[2], CultureInfo.InvariantCulture);
			});
			regionFields.Add("orig", () => { // Deprecated, use offsets.
				region.originalWidth = int.Parse(entry[1], CultureInfo.InvariantCulture);
				region.originalHeight = int.Parse(entry[2], CultureInfo.InvariantCulture);
			});
			regionFields.Add("offsets", () => {
				region.offsetX = int.Parse(entry[1], CultureInfo.InvariantCulture);
				region.offsetY = int.Parse(entry[2], CultureInfo.InvariantCulture);
				region.originalWidth = int.Parse(entry[3], CultureInfo.InvariantCulture);
				region.originalHeight = int.Parse(entry[4], CultureInfo.InvariantCulture);
			});
			regionFields.Add("rotate", () => {
				string value = entry[1];
				if (value == "true")
					region.degrees = 90;
				else if (value != "false")
					region.degrees = int.Parse(value, CultureInfo.InvariantCulture);
			});
			regionFields.Add("index", () => {
				region.index = int.Parse(entry[1], CultureInfo.InvariantCulture);
			});

			string line = reader.ReadLine();
			// Ignore empty lines before first entry.
			while (line != null && line.Trim().Length == 0)
				line = reader.ReadLine();
			// Header entries.
			while (true) {
				if (line == null || line.Trim().Length == 0) break;
				if (ReadEntry(entry, line) == 0) break; // Silently ignore all header fields.
				line = reader.ReadLine();
			}
			// Page and region entries.
			List<string> names = null;
			List<int[]> values = null;
			while (true) {
				if (line == null) break;
				if (line.Trim().Length == 0) {
					page = null;
					line = reader.ReadLine();
				} else if (page == null) {
					page = new AtlasPage();
					page.name = line.Trim();
					while (true) {
						if (ReadEntry(entry, line = reader.ReadLine()) == 0) break;
						Action field;
						if (pageFields.TryGetValue(entry[0], out field)) field(); // Silently ignore unknown page fields.
					}
					textureLoader.Load(page, Path.Combine(imagesDir, page.name));
					pages.Add(page);
				} else {
					region = new AtlasRegion();
					region.page = page;
					region.name = line;
					while (true) {
						int count = ReadEntry(entry, line = reader.ReadLine());
						if (count == 0) break;
						Action field;
						if (regionFields.TryGetValue(entry[0], out field))
							field();
						else {
							if (names == null) {
								names = new List<string>(8);
								values = new List<int[]>(8);
							}
							names.Add(entry[0]);
							int[] entryValues = new int[count];
							for (int i = 0; i < count; i++)
								int.TryParse(entry[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out entryValues[i]); // Silently ignore non-integer values.
							values.Add(entryValues);
						}
					}
					if (region.originalWidth == 0 && region.originalHeight == 0) {
						region.originalWidth = region.width;
						region.originalHeight = region.height;
					}
					if (names != null && names.Count > 0) {
						region.names = names.ToArray();
						region.values = values.ToArray();
						names.Clear();
						values.Clear();
					}
					region.u = region.x / (float)page.width;
					region.v = region.y / (float)page.height;
					if (region.degrees == 90) {
						region.u2 = (region.x + region.height) / (float)page.width;
						region.v2 = (region.y + region.width) / (float)page.height;

						int tempSwap = region.packedWidth;
						region.packedWidth = region.packedHeight;
						region.packedHeight = tempSwap;
					} else {
						region.u2 = (region.x + region.width) / (float)page.width;
						region.v2 = (region.y + region.height) / (float)page.height;
					}
					regions.Add(region);
				}
			}
		}

		static private int ReadEntry (string[] entry, string line) {
			if (line == null) return 0;
			line = line.Trim();
			if (line.Length == 0) return 0;
			int colon = line.IndexOf(':');
			if (colon == -1) return 0;
			entry[0] = line.Substring(0, colon).Trim();
			for (int i = 1, lastMatch = colon + 1; ; i++) {
				int comma = line.IndexOf(',', lastMatch);
				if (comma == -1) {
					entry[i] = line.Substring(lastMatch).Trim();
					return i;
				}
				entry[i] = line.Substring(lastMatch, comma - lastMatch).Trim();
				lastMatch = comma + 1;
				if (i == 4) return 4;
			}
		}

		public void FlipV () {
			for (int i = 0, n = regions.Count; i < n; i++) {
				AtlasRegion region = regions[i];
				region.v = 1 - region.v;
				region.v2 = 1 - region.v2;
			}
		}

		/// <summary>Returns the first region found with the specified name. This method uses string comparison to find the region, so the result
		/// should be cached rather than calling this method multiple times.</summary>
		/// <returns>The region, or null.</returns>
		public AtlasRegion FindRegion (string name) {
			for (int i = 0, n = regions.Count; i < n; i++)
				if (regions[i].name == name) return regions[i];
			return null;
		}

		public void Dispose () {
			if (textureLoader == null) return;
			for (int i = 0, n = pages.Count; i < n; i++)
				textureLoader.Unload(pages[i].rendererObject);
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
		public string name;
		public int width, height;
		public Format format = Format.RGBA8888;
		public TextureFilter minFilter = TextureFilter.Nearest;
		public TextureFilter magFilter = TextureFilter.Nearest;
		public TextureWrap uWrap = TextureWrap.ClampToEdge;
		public TextureWrap vWrap = TextureWrap.ClampToEdge;
		public bool pma;
		public object rendererObject;

		public AtlasPage Clone () {
			return MemberwiseClone() as AtlasPage;
		}
	}

	public class AtlasRegion : TextureRegion {
		public AtlasPage page;
		public string name;
		public int x, y;
		public float offsetX, offsetY;
		public int originalWidth, originalHeight;
		public int packedWidth { get { return width; } set { width = value; } }
		public int packedHeight { get { return height; } set { height = value; } }
		public int degrees;
		public bool rotate;
		public int index;
		public string[] names;
		public int[][] values;

		override public int OriginalWidth { get { return originalWidth; } }
		override public int OriginalHeight { get { return originalHeight; } }

		public AtlasRegion Clone () {
			return MemberwiseClone() as AtlasRegion;
		}
	}

	public interface TextureLoader {
		void Load (AtlasPage page, string path);
		void Unload (Object texture);
	}
}
