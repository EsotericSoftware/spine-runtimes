/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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
using System.Collections.Generic;
using System.IO;

namespace Spine {

	/// <summary>
	/// Base class for loading skeleton data from a file.
	/// <para>
	/// See<a href="http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data">JSON and binary data</a> in the
	/// Spine Runtimes Guide.</para>
	/// </summary>
	public abstract class SkeletonLoader {
		protected readonly AttachmentLoader attachmentLoader;
		protected float scale = 1;
		protected readonly List<LinkedMesh> linkedMeshes = new List<LinkedMesh>();

		/// <summary>Creates a skeleton loader that loads attachments using an <see cref="AtlasAttachmentLoader"/> with the specified atlas.
		/// </summary>
		public SkeletonLoader (params Atlas[] atlasArray) {
			attachmentLoader = new AtlasAttachmentLoader(atlasArray);
		}

		/// <summary>Creates a skeleton loader that loads attachments using the specified attachment loader.
		/// <para>See <a href='http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data'>Loading skeleton data</a> in the
		/// Spine Runtimes Guide.</para></summary>
		public SkeletonLoader (AttachmentLoader attachmentLoader) {
			if (attachmentLoader == null) throw new ArgumentNullException("attachmentLoader", "attachmentLoader cannot be null.");
			this.attachmentLoader = attachmentLoader;
		}

		/// <summary>Scales bone positions, image sizes, and translations as they are loaded. This allows different size images to be used at
		/// runtime than were used in Spine.
		/// <para>
		/// See <a href="http://esotericsoftware.com/spine-loading-skeleton-data#Scaling">Scaling</a> in the Spine Runtimes Guide.</para>
		/// </summary>
		public float Scale {
			get { return scale; }
			set {
				if (scale == 0) throw new ArgumentNullException("scale", "scale cannot be 0.");
				this.scale = value;
			}
		}

		public abstract SkeletonData ReadSkeletonData (string path);

		protected class LinkedMesh {
			internal string parent, skin;
			internal int slotIndex;
			internal MeshAttachment mesh;
			internal bool inheritTimelines;

			public LinkedMesh (MeshAttachment mesh, string skin, int slotIndex, string parent, bool inheritTimelines) {
				this.mesh = mesh;
				this.skin = skin;
				this.slotIndex = slotIndex;
				this.parent = parent;
				this.inheritTimelines = inheritTimelines;
			}
		}

	}
}
