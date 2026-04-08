/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {

	/// <summary>
	/// An <see cref="AttachmentLoader"/> that configures attachments using texture regions from an <see cref="Atlas"/>.
	/// See <a href='http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data'>Loading Skeleton Data</a> in the Spine Runtimes Guide.
	/// </summary>
	public class AtlasAttachmentLoader : AttachmentLoader {
		private Atlas[] atlasArray;
		/// <summary>If true, <see cref="FindRegion(string, string)"/> may return null. If false, an error is raised if the texture region is not
		/// found. Default is false.</summary>
		public bool allowMissingRegions;

		public AtlasAttachmentLoader (params Atlas[] atlasArray)
			: this(false, atlasArray) {
		}

		public AtlasAttachmentLoader (bool allowMissingRegions, params Atlas[] atlasArray) {
			if (atlasArray == null) throw new ArgumentNullException("atlas", "atlas array cannot be null.");
			this.atlasArray = atlasArray;
			this.allowMissingRegions = allowMissingRegions;
		}

		/// <summary>Sets each <see cref="Sequence.Regions"/> by calling <see cref="FindRegion(string, string)"/> for each texture region using
		/// <see cref="Sequence.GetPath(string, int)"/>.</summary>
		protected void FindRegions (string name, string basePath, Sequence sequence) {
			TextureRegion[] regions = sequence.Regions;
			for (int i = 0, n = regions.Length; i < n; i++) {
				regions[i] = FindRegion(name, sequence.GetPath(basePath, i));
			}
		}

		/// <summary>Looks for the region with the specified path. If not found and <see cref="allowMissingRegions"/> is false, an error is
		/// raised.</summary>
		protected AtlasRegion FindRegion (string name, string path) {
			for (int i = 0; i < atlasArray.Length; i++) {
				AtlasRegion region = atlasArray[i].FindRegion(path);
				if (region != null)
					return region;
			}
			if (!allowMissingRegions)
				throw new ArgumentException(string.Format("Region not found in atlas: {0} (attachment: {1})", path, name));
			return null;
		}

		public RegionAttachment NewRegionAttachment (Skin skin, string name, string path, Sequence sequence) {
			FindRegions(name, path, sequence);
			return new RegionAttachment(name, sequence);
		}

		public MeshAttachment NewMeshAttachment (Skin skin, string name, string path, Sequence sequence) {
			FindRegions(name, path, sequence);
			return new MeshAttachment(name, sequence);
		}

		public BoundingBoxAttachment NewBoundingBoxAttachment (Skin skin, string name) {
			return new BoundingBoxAttachment(name);
		}

		public PathAttachment NewPathAttachment (Skin skin, string name) {
			return new PathAttachment(name);
		}

		public PointAttachment NewPointAttachment (Skin skin, string name) {
			return new PointAttachment(name);
		}

		public ClippingAttachment NewClippingAttachment (Skin skin, string name) {
			return new ClippingAttachment(name);
		}
	}
}
