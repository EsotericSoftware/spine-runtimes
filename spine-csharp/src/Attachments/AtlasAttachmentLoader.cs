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

using System;

namespace Spine {

	/// <summary>
	/// An AttachmentLoader that configures attachments using texture regions from an Atlas.
	/// See <a href='http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data'>Loading Skeleton Data</a> in the Spine Runtimes Guide.
	/// </summary>
	public class AtlasAttachmentLoader : AttachmentLoader {
		private Atlas[] atlasArray;

		public AtlasAttachmentLoader (params Atlas[] atlasArray) {
			if (atlasArray == null) throw new ArgumentNullException("atlas", "atlas array cannot be null.");
			this.atlasArray = atlasArray;
		}

		private void LoadSequence (string name, string basePath, Sequence sequence) {
			TextureRegion[] regions = sequence.Regions;
			for (int i = 0, n = regions.Length; i < n; i++) {
				string path = sequence.GetPath(basePath, i);
				regions[i] = FindRegion(path);
				if (regions[i] == null) throw new ArgumentException(string.Format("Region not found in atlas: {0} (region attachment: {1})", path, name));
			}
		}

		public RegionAttachment NewRegionAttachment (Skin skin, string name, string path, Sequence sequence) {
			RegionAttachment attachment = new RegionAttachment(name);
			if (sequence != null)
				LoadSequence(name, path, sequence);
			else {
				AtlasRegion region = FindRegion(path);
				if (region == null)
					throw new ArgumentException(string.Format("Region not found in atlas: {0} (region attachment: {1})", path, name));
				attachment.Region = region;
			}
			return attachment;
		}

		public MeshAttachment NewMeshAttachment (Skin skin, string name, string path, Sequence sequence) {
			MeshAttachment attachment = new MeshAttachment(name);
			if (sequence != null)
				LoadSequence(name, path, sequence);
			else {
				AtlasRegion region = FindRegion(path);
				if (region == null)
					throw new ArgumentException(string.Format("Region not found in atlas: {0} (region attachment: {1})", path, name));
				attachment.Region = region;
			}
			return attachment;
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

		public AtlasRegion FindRegion (string name) {
			AtlasRegion region;

			for (int i = 0; i < atlasArray.Length; i++) {
				region = atlasArray[i].FindRegion(name);
				if (region != null)
					return region;
			}

			return null;
		}
	}
}
