/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

// Not for optimization. Do not disable.
#define SPINE_TRIANGLECHECK // Avoid calling SetTriangles at the cost of checking for mesh differences (vertex counts, memberwise attachment list compare) every frame.
//#define SPINE_DEBUG

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Spine.Unity {
	public static class SpineMesh {
		internal const HideFlags MeshHideflags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

		/// <summary>Factory method for creating a new mesh for use in Spine components. This can be called in field initializers.</summary>
		public static Mesh NewSkeletonMesh () {
			var m = new Mesh();
			m.MarkDynamic();
			m.name = "Skeleton Mesh";
			m.hideFlags = SpineMesh.MeshHideflags;
			return m;
		}
	}

	/// <summary>Instructions for how to generate a mesh or submesh: "Render this skeleton's slots: start slot, up to but not including endSlot, using this material."</summary>
	public struct SubmeshInstruction {
		public Skeleton skeleton;
		public int startSlot;
		public int endSlot;
		public Material material;

		public bool forceSeparate;
		public int preActiveClippingSlotSource;

		#if SPINE_TRIANGLECHECK
		// Cached values because they are determined in the process of generating instructions,
		// but could otherwise be pulled from accessing attachments, checking materials and counting tris and verts.
		public int rawTriangleCount;
		public int rawVertexCount;
		public int rawFirstVertexIndex;
		public bool hasClipping;
		#endif

		/// <summary>The number of slots in this SubmeshInstruction's range. Not necessarily the number of attachments.</summary>
		public int SlotCount { get { return endSlot - startSlot; } }

		public override string ToString () {
			return
				string.Format("[SubmeshInstruction: slots {0} to {1}. (Material){2}. preActiveClippingSlotSource:{3}]",
					startSlot,
					endSlot - 1,
					material == null ? "<none>" : material.name,
					preActiveClippingSlotSource
				);
		}
	}
}
