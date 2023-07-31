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
using System.Text;

namespace Spine {
	public class Sequence {
		static int nextID = 0;
		static readonly Object nextIdLock = new Object();

		internal readonly int id;
		internal readonly TextureRegion[] regions;
		internal int start, digits, setupIndex;

		public int Start { get { return start; } set { start = value; } }
		public int Digits { get { return digits; } set { digits = value; } }
		/// <summary>The index of the region to show for the setup pose.</summary>
		public int SetupIndex { get { return setupIndex; } set { setupIndex = value; } }
		public TextureRegion[] Regions { get { return regions; } }
		/// <summary>Returns a unique ID for this attachment.</summary>
		public int Id { get { return id; } }

		public Sequence (int count) {
			lock (Sequence.nextIdLock) {
				id = Sequence.nextID++;
			}
			regions = new TextureRegion[count];
		}

		/// <summary>Copy constructor.</summary>
		public Sequence (Sequence other) {
			lock (Sequence.nextIdLock) {
				id = Sequence.nextID++;
			}
			regions = new TextureRegion[other.regions.Length];
			Array.Copy(other.regions, 0, regions, 0, regions.Length);

			start = other.start;
			digits = other.digits;
			setupIndex = other.setupIndex;
		}

		public void Apply (Slot slot, IHasTextureRegion attachment) {
			int index = slot.SequenceIndex;
			if (index == -1) index = setupIndex;
			if (index >= regions.Length) index = regions.Length - 1;
			TextureRegion region = regions[index];
			if (attachment.Region != region) {
				attachment.Region = region;
				attachment.UpdateRegion();
			}
		}

		public string GetPath (string basePath, int index) {
			StringBuilder buffer = new StringBuilder(basePath.Length + digits);
			buffer.Append(basePath);
			string frame = (start + index).ToString();
			for (int i = digits - frame.Length; i > 0; i--)
				buffer.Append('0');
			buffer.Append(frame);
			return buffer.ToString();
		}
	}

	public enum SequenceMode {
		Hold, Once, Loop, Pingpong, OnceReverse, LoopReverse, PingpongReverse
	}
}
