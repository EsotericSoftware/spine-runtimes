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
using System.Text;

namespace Spine {
	/// <summary>
	/// Holds texture regions, UVs, and vertex offsets for rendering a region or mesh attachment. <see cref="Regions"/> must
	/// be populated and <see cref="Update(IHasSequence)"/> called before use.
	/// </summary>
	public class Sequence {
		static int nextID = 0;
		static readonly Object nextIdLock = new Object();

		internal readonly int id;
		internal readonly TextureRegion[] regions;
		internal readonly bool pathSuffix;
		internal float[][] uvs, offsets;
		internal int start, digits, setupIndex;

		/// <summary>The starting number for the numeric <see cref="GetPath(string, int)">path</see> suffix.</summary>
		public int Start { get { return start; } set { start = value; } }
		/// <summary>The minimum number of digits in the numeric <see cref="GetPath(string, int)">path</see> suffix, for zero
		/// padding. 0 for no zero padding.</summary>
		public int Digits { get { return digits; } set { digits = value; } }
		/// <summary>The index of the region to show for the setup pose.</summary>
		public int SetupIndex { get { return setupIndex; } set { setupIndex = value; } }
		/// <summary>The list of texture regions this sequence will display.</summary>
		public TextureRegion[] Regions { get { return regions; } }
		/// <summary>Returns true if the <see cref="GetPath(string, int)">path</see> has a numeric suffix.</summary>
		public bool HasPathSuffix { get { return pathSuffix; } }
		/// <summary>Returns a unique ID for this attachment.</summary>
		public int Id { get { return id; } }

		/// <param name="count">The number of texture regions this sequence will display.</param>
		/// <param name="pathSuffix">If true, the <see cref="GetPath(string, int)">path</see> has a numeric suffix. If false, all
		/// regions will use the same path, so <c>count</c> should be 1.</param>
		public Sequence (int count, bool pathSuffix) {
			lock (Sequence.nextIdLock) {
				id = Sequence.nextID++;
			}
			regions = new TextureRegion[count];
			this.pathSuffix = pathSuffix;
		}

		/// <summary>Copy constructor.</summary>
		public Sequence (Sequence other) {
			lock (Sequence.nextIdLock) {
				id = Sequence.nextID++;
			}
			int regionCount = other.regions.Length;
			regions = new TextureRegion[regionCount];
			Array.Copy(other.regions, 0, regions, 0, regionCount);

			start = other.start;
			digits = other.digits;
			setupIndex = other.setupIndex;
			pathSuffix = other.pathSuffix;

			if (other.uvs != null) {
				int length = other.uvs[0].Length;
				uvs = new float[regionCount][];
				for (int i = 0; i < regionCount; i++) {
					uvs[i] = new float[length];
					Array.Copy(other.uvs[i], 0, uvs[i], 0, length);
				}
			}
			if (other.offsets != null) {
				offsets = new float[regionCount][];
				for (int i = 0; i < regionCount; i++) {
					offsets[i] = new float[8];
					Array.Copy(other.offsets[i], 0, offsets[i], 0, 8);
				}
			}
		}

		public void Update (IHasSequence attachment) {
			int regionCount = regions.Length;
			RegionAttachment region = attachment as RegionAttachment;
			if (region != null) {
				uvs = new float[regionCount][];
				offsets = new float[regionCount][];
				for (int i = 0; i < regionCount; i++) {
					uvs[i] = new float[8];
					offsets[i] = new float[8];
					RegionAttachment.ComputeUVs(regions[i], region.x, region.y, region.scaleX, region.scaleY, region.rotation,
						region.width, region.height, offsets[i], uvs[i]);
				}
			} else {
				MeshAttachment mesh = attachment as MeshAttachment;
				if (mesh != null) {
					float[] regionUVs = mesh.regionUVs;
					uvs = new float[regionCount][];
					offsets = null;
					for (int i = 0; i < regionCount; i++) {
						uvs[i] = new float[regionUVs.Length];
						MeshAttachment.ComputeUVs(regions[i], regionUVs, uvs[i]);
					}
				}
			}
		}

		/// <summary>Returns the <see cref="Regions"/> index for the <see cref="SlotPose.SequenceIndex"/>.</summary>
		public int ResolveIndex (SlotPose pose) {
			int index = pose.SequenceIndex;
			if (index == -1) index = setupIndex;
			if (index >= regions.Length) index = regions.Length - 1;
			return index;
		}

		/// <summary>Returns the texture region from <see cref="Regions"/> for the specified index.</summary>
		public TextureRegion GetRegion (int index) {
			return regions[index];
		}

		/// <summary>Returns the UVs for the specified index. <see cref="Regions">Regions</see> must be populated and
		/// <see cref="Update(IHasSequence)"/> called before calling this method.</summary>
		public float[] GetUVs (int index) {
			return uvs[index];
		}

		/// <summary>
		/// Returns vertex offsets from the center of a <see cref="RegionAttachment"/>. Invalid to call for a <see cref="MeshAttachment"/>.
		/// </summary>
		public float[] GetOffsets (int index) {
			return offsets[index];
		}

		/// <summary>Returns the specified base path with an optional numeric suffix for the specified index.</summary>
		public string GetPath (string basePath, int index) {
			if (!pathSuffix) return basePath;
			var buffer = new StringBuilder(basePath.Length + digits);
			buffer.Append(basePath);
			string frame = (start + index).ToString();
			for (int i = digits - frame.Length; i > 0; i--)
				buffer.Append('0');
			buffer.Append(frame);
			return buffer.ToString();
		}
	}

	/// <summary>Controls how <see cref="Sequence.Regions"/> are displayed over time.</summary>
	public enum SequenceMode {
		Hold, Once, Loop, Pingpong, OnceReverse, LoopReverse, PingpongReverse
	}
}
