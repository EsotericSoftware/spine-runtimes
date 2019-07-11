/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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

		#if !(IS_UNITY)
		#if WINDOWS_STOREAPP
		private async Task ReadFile(string path, TextureLoader textureLoader) {
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			var file = await folder.GetFileAsync(path).AsTask().ConfigureAwait(false);
			using (var reader = new StreamReader(await file.OpenStreamForReadAsync().ConfigureAwait(false))) {
				try {
					Load(reader, Path.GetDirectoryName(path), textureLoader);
				} catch (Exception ex) {
					throw new Exception("Error reading atlas file: " + path, ex);
				}
			}
		}

		public Atlas(string path, TextureLoader textureLoader) {
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
					Load(reader, Path.GetDirectoryName(path), textureLoader);
				} catch (Exception ex) {
					throw new Exception("Error reading atlas file: " + path, ex);
				}

			}
		}
		#endif // WINDOWS_STOREAPP

		#endif

		public Atlas (TextReader reader, string dir, TextureLoader textureLoader) {
			Load(reader, dir, textureLoader);
		}

		public Atlas (List<AtlasPage> pages, List<AtlasRegion> regions) {
			this.pages = pages;
			this.regions = regions;
			this.textureLoader = null;
		}

		private void Load (TextReader reader, string imagesDir, TextureLoader textureLoader) {
			if (textureLoader == null) throw new ArgumentNullException("textureLoader", "textureLoader cannot be null.");
			this.textureLoader = textureLoader;

			string[] tuple = new string[4];
			AtlasPage page = null;
			while (true) {
				string line = reader.ReadLine();
				if (line == null) break;
				if (line.Trim().Length == 0)
					page = null;
				else if (page == null) {
					page = new AtlasPage();
					page.name = line;

					if (ReadTuple(reader, tuple) == 2) { // size is only optional for an atlas packed with an old TexturePacker.
						page.width = int.Parse(tuple[0], CultureInfo.InvariantCulture);
						page.height = int.Parse(tuple[1], CultureInfo.InvariantCulture);
						ReadTuple(reader, tuple);
					}
					page.format = (Format)Enum.Parse(typeof(Format), tuple[0], false);

					ReadTuple(reader, tuple);
					page.minFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[0], false);
					page.magFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[1], false);

					string direction = ReadValue(reader);
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

					string rotateValue = ReadValue(reader);
					if (rotateValue == "true")
						region.degrees = 90;
					else if (rotateValue == "false")
						region.degrees = 0;
					else
						region.degrees = int.Parse(rotateValue);
					region.rotate = region.degrees == 90;

					ReadTuple(reader, tuple);
					int x = int.Parse(tuple[0], CultureInfo.InvariantCulture);
					int y = int.Parse(tuple[1], CultureInfo.InvariantCulture);

					ReadTuple(reader, tuple);
					int width = int.Parse(tuple[0], CultureInfo.InvariantCulture);
					int height = int.Parse(tuple[1], CultureInfo.InvariantCulture);

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

					if (ReadTuple(reader, tuple) == 4) { // split is optional
						region.splits = new [] {int.Parse(tuple[0], CultureInfo.InvariantCulture),
												int.Parse(tuple[1], CultureInfo.InvariantCulture),
												int.Parse(tuple[2], CultureInfo.InvariantCulture),
												int.Parse(tuple[3], CultureInfo.InvariantCulture)};

						if (ReadTuple(reader, tuple) == 4) { // pad is optional, but only present with splits
							region.pads = new [] {int.Parse(tuple[0], CultureInfo.InvariantCulture),
												int.Parse(tuple[1], CultureInfo.InvariantCulture),
												int.Parse(tuple[2], CultureInfo.InvariantCulture),
												int.Parse(tuple[3], CultureInfo.InvariantCulture)};

							ReadTuple(reader, tuple);
						}
					}

					region.originalWidth = int.Parse(tuple[0], CultureInfo.InvariantCulture);
					region.originalHeight = int.Parse(tuple[1], CultureInfo.InvariantCulture);

					ReadTuple(reader, tuple);
					region.offsetX = int.Parse(tuple[0], CultureInfo.InvariantCulture);
					region.offsetY = int.Parse(tuple[1], CultureInfo.InvariantCulture);

					region.index = int.Parse(ReadValue(reader), CultureInfo.InvariantCulture);

					regions.Add(region);
				}
			}
		}

		static string ReadValue (TextReader reader) {
			string line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1) throw new Exception("Invalid line: " + line);
			return line.Substring(colon + 1).Trim();
		}

		/// <summary>Returns the number of tuple values read (1, 2 or 4).</summary>
		static int ReadTuple (TextReader reader, string[] tuple) {
			string line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1) throw new Exception("Invalid line: " + line);
			int i = 0, lastMatch = colon + 1;
			for (; i < 3; i++) {
				int comma = line.IndexOf(',', lastMatch);
				if (comma == -1) break;
				tuple[i] = line.Substring(lastMatch, comma - lastMatch).Trim();
				lastMatch = comma + 1;
			}
			tuple[i] = line.Substring(lastMatch).Trim();
			return i + 1;
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
		public Format format;
		public TextureFilter minFilter;
		public TextureFilter magFilter;
		public TextureWrap uWrap;
		public TextureWrap vWrap;
		public object rendererObject;
		public int width, height;

		public AtlasPage Clone () {
			return MemberwiseClone() as AtlasPage;
		}
	}

	public class AtlasRegion {
		public AtlasPage page;
		public string name;
		public int x, y, width, height;
		public float u, v, u2, v2;
		public float offsetX, offsetY;
		public int originalWidth, originalHeight;
		public int index;
		public bool rotate;
		public int degrees;
		public int[] splits;
		public int[] pads;

		public AtlasRegion Clone () {
			return MemberwiseClone() as AtlasRegion;
		}
	}

	public interface TextureLoader {
		void Load (AtlasPage page, string path);
		void Unload (Object texture);
	}
}
