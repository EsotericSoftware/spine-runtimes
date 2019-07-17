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

// Not for optimization. Do  not disable.
#define SPINE_TRIANGLECHECK // Avoid calling SetTriangles at the cost of checking for mesh differences (vertex counts, memberwise attachment list compare) every frame.
//#define SPINE_DEBUG

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Spine.Unity {
	/// <summary>Instructions used by a SkeletonRenderer to render a mesh.</summary>
	public class SkeletonRendererInstruction {
		public readonly ExposedList<SubmeshInstruction> submeshInstructions = new ExposedList<SubmeshInstruction>();

		public bool immutableTriangles;
		#if SPINE_TRIANGLECHECK
		public bool hasActiveClipping;
		public int rawVertexCount = -1;
		public readonly ExposedList<Attachment> attachments = new ExposedList<Attachment>();
		#endif

		public void Clear () {
			#if SPINE_TRIANGLECHECK
			this.attachments.Clear(false);
			rawVertexCount = -1;
			hasActiveClipping = false;
			#endif
			this.submeshInstructions.Clear(false);
		}

		public void Dispose () {
			attachments.Clear(true);
		}

		public void SetWithSubset (ExposedList<SubmeshInstruction> instructions, int startSubmesh, int endSubmesh) {
			#if SPINE_TRIANGLECHECK
			int runningVertexCount = 0;
			#endif

			var submeshes = this.submeshInstructions;
			submeshes.Clear(false);
			int submeshCount = endSubmesh - startSubmesh;
			submeshes.Resize(submeshCount);
			var submeshesItems = submeshes.Items;
			var instructionsItems = instructions.Items;
			for (int i = 0; i < submeshCount; i++) {
				var instruction = instructionsItems[startSubmesh + i];
				submeshesItems[i] = instruction;
				#if SPINE_TRIANGLECHECK
				this.hasActiveClipping |= instruction.hasClipping;
				submeshesItems[i].rawFirstVertexIndex = runningVertexCount; // Ensure current instructions have correct cached values.
				runningVertexCount += instruction.rawVertexCount; // vertexCount will also be used for the rest of this method.
				#endif
			}
			#if SPINE_TRIANGLECHECK
			this.rawVertexCount = runningVertexCount;

			// assumption: instructions are contiguous. start and end are valid within instructions.

			int startSlot = instructionsItems[startSubmesh].startSlot;
			int endSlot = instructionsItems[endSubmesh - 1].endSlot;
			attachments.Clear(false);
			int attachmentCount = endSlot - startSlot;
			attachments.Resize(attachmentCount);
			var attachmentsItems = attachments.Items;

			var drawOrderItems = instructionsItems[0].skeleton.drawOrder.Items;
			for (int i = 0; i < attachmentCount; i++) {
				Slot slot = drawOrderItems[startSlot + i];
				if (!slot.bone.active) continue;
				attachmentsItems[i] = slot.attachment;
			}
			
			#endif
		}

		public void Set (SkeletonRendererInstruction other) {
			this.immutableTriangles = other.immutableTriangles;

			#if SPINE_TRIANGLECHECK
			this.hasActiveClipping = other.hasActiveClipping;
			this.rawVertexCount = other.rawVertexCount;
			this.attachments.Clear(false);
			this.attachments.EnsureCapacity(other.attachments.Capacity);
			this.attachments.Count = other.attachments.Count;
			other.attachments.CopyTo(this.attachments.Items);
			#endif

			this.submeshInstructions.Clear(false);
			this.submeshInstructions.EnsureCapacity(other.submeshInstructions.Capacity);
			this.submeshInstructions.Count = other.submeshInstructions.Count;
			other.submeshInstructions.CopyTo(this.submeshInstructions.Items);
		}

		public static bool GeometryNotEqual (SkeletonRendererInstruction a, SkeletonRendererInstruction b) {
			#if SPINE_TRIANGLECHECK
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			return true;
			#endif

			if (a.hasActiveClipping || b.hasActiveClipping) return true; // Triangles are unpredictable when clipping is active.

			// Everything below assumes the raw vertex and triangle counts were used. (ie, no clipping was done)
			if (a.rawVertexCount != b.rawVertexCount) return true;

			if (a.immutableTriangles != b.immutableTriangles) return true;

			int attachmentCountB = b.attachments.Count;
			if (a.attachments.Count != attachmentCountB) return true; // Bounds check for the looped storedAttachments count below.

			// Submesh count changed
			int submeshCountA = a.submeshInstructions.Count;
			int submeshCountB = b.submeshInstructions.Count;
			if (submeshCountA != submeshCountB) return true;

			// Submesh Instruction mismatch
			var submeshInstructionsItemsA = a.submeshInstructions.Items;
			var submeshInstructionsItemsB = b.submeshInstructions.Items;

			var attachmentsA = a.attachments.Items;
			var attachmentsB = b.attachments.Items;		
			for (int i = 0; i < attachmentCountB; i++)
				if (!System.Object.ReferenceEquals(attachmentsA[i], attachmentsB[i])) return true;

			for (int i = 0; i < submeshCountB; i++) {
				var submeshA = submeshInstructionsItemsA[i];
				var submeshB = submeshInstructionsItemsB[i];

				if (!(
					submeshA.rawVertexCount == submeshB.rawVertexCount &&
					submeshA.startSlot == submeshB.startSlot &&
					submeshA.endSlot == submeshB.endSlot
					&& submeshA.rawTriangleCount == submeshB.rawTriangleCount &&
					submeshA.rawFirstVertexIndex == submeshB.rawFirstVertexIndex
				))
					return true;			
			}

			return false;
			#else
			// In normal immutable triangle use, immutableTriangles will be initially false, forcing the smartmesh to update the first time but never again after that, unless there was an immutableTriangles flag mismatch..
			if (a.immutableTriangles || b.immutableTriangles)
				return (a.immutableTriangles != b.immutableTriangles);

			return true;
			#endif
		}
	}
}
