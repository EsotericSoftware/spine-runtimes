/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

//#define SPINE_TRIANGLECHECK // Avoid calling SetTriangles at the cost of checking for mesh differences (vertex counts, memberwise attachment list compare) every frame.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Spine.Unity {
	public static class SpineMesh {
		internal const HideFlags MeshHideflags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

		/// <summary>Factory method for creating a new mesh for use in Spine components. This can be called in field initializers.</summary>
		public static Mesh NewMesh () {
			var m = new Mesh();
			m.MarkDynamic();
			m.name = "Skeleton Mesh";
			m.hideFlags = SpineMesh.MeshHideflags;
			return m;
		}
	}

	/// <summary>Instructions for how to generate a mesh or submesh out of a range of slots in a given skeleton.</summary>
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
	}

	[System.Serializable]
	public class MeshGenerator {
		public Settings settings = Settings.Default;

		[System.Serializable]
		public struct Settings {
			public bool renderMeshes;
			public bool useClipping;
			[Space]
			[Range(-0.1f, 0f)] public float zSpacing;
			[Space]
			[Header("Vertex Data")]
			public bool pmaVertexColors;
			public bool tintBlack;
			public bool calculateTangents;
			public bool addNormals;
			public bool immutableTriangles;

			static public Settings Default {
				get {
					return new Settings {
						pmaVertexColors = true,
						zSpacing = 0f,
						useClipping = true,
						tintBlack = false,
						calculateTangents = false,
						renderMeshes = true,
						addNormals = false,
						immutableTriangles = false
					};
				}
			}
		}

		const float BoundsMinDefault = float.MaxValue;
		const float BoundsMaxDefault = float.MinValue;

		[NonSerialized] readonly ExposedList<Vector3> vertexBuffer = new ExposedList<Vector3>();
		[NonSerialized] readonly ExposedList<Vector2> uvBuffer = new ExposedList<Vector2>();
		[NonSerialized] readonly ExposedList<Color32> colorBuffer = new ExposedList<Color32>();
		[NonSerialized] readonly ExposedList<ExposedList<int>> submeshes = new ExposedList<ExposedList<int>> { new ExposedList<int>(6) }; // start with 1 submesh.

		[NonSerialized] Vector2 meshBoundsMin, meshBoundsMax;
		[NonSerialized] float meshBoundsThickness;
		[NonSerialized] int submeshIndex = 0;

		[NonSerialized] SkeletonClipping clipper = new SkeletonClipping();
		[NonSerialized] float[] tempVerts = new float[8];
		[NonSerialized] int[] regionTriangles = { 0, 1, 2, 2, 3, 0 };

		#region Optional Buffers
		[NonSerialized] Vector3[] normals;
		[NonSerialized] Vector4[] tangents;
		[NonSerialized] Vector2[] tempTanBuffer;
		[NonSerialized] ExposedList<Vector2> uv2;
		[NonSerialized] ExposedList<Vector2> uv3;
		#endregion

		#region Step 1 : Generate Instructions
		public static void GenerateSingleSubmeshInstruction (SkeletonRendererInstruction instructionOutput, Skeleton skeleton, bool renderMeshes, Material material) {
			ExposedList<Slot> drawOrder = skeleton.drawOrder;
			int drawOrderCount = drawOrder.Count;

			// Clear last state of attachments and submeshes
			instructionOutput.Clear(); // submeshInstructions.Clear(); attachments.Clear();
			var workingSubmeshInstructions = instructionOutput.submeshInstructions;
			workingSubmeshInstructions.Resize(1);

			#if SPINE_TRIANGLECHECK
			instructionOutput.attachments.Resize(drawOrderCount);
			var workingAttachmentsItems = instructionOutput.attachments.Items;
			int totalRawVertexCount = 0;
			#endif

			var current = new SubmeshInstruction {
				skeleton = skeleton,
				preActiveClippingSlotSource = -1,
				startSlot = 0,
				#if SPINE_TRIANGLECHECK
				rawFirstVertexIndex = 0,
				#endif
				material = material,
				forceSeparate = false,
				endSlot = drawOrderCount
			};

			#if SPINE_TRIANGLECHECK
			bool skeletonHasClipping = false;
			var drawOrderItems = drawOrder.Items;
			for (int i = 0; i < drawOrderCount; i++) {
				Slot slot = drawOrderItems[i];
				Attachment attachment = slot.attachment;

				workingAttachmentsItems[i] = attachment;
				int attachmentTriangleCount = 0;
				int attachmentVertexCount = 0;
				

				var regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {
					attachmentVertexCount = 4;
					attachmentTriangleCount = 6;
				} else {
					if (renderMeshes) {
						var meshAttachment = attachment as MeshAttachment;
						if (meshAttachment != null) {
							attachmentVertexCount = meshAttachment.worldVerticesLength >> 1;
							attachmentTriangleCount = meshAttachment.triangles.Length;						
						} else {
							var clippingAttachment = attachment as ClippingAttachment;
							if (clippingAttachment != null) {
								current.hasClipping = true;
								skeletonHasClipping = true;
							}
							attachmentVertexCount = 0;
							attachmentTriangleCount = 0;
							//continue;
						}
					}
				}
				current.rawTriangleCount += attachmentTriangleCount;
				current.rawVertexCount += attachmentVertexCount;
				totalRawVertexCount += attachmentVertexCount;
				
			}

			instructionOutput.hasActiveClipping = skeletonHasClipping;
			instructionOutput.rawVertexCount = totalRawVertexCount;
			#endif

			workingSubmeshInstructions.Items[0] = current;
		}
			
		public static void GenerateSkeletonRendererInstruction (SkeletonRendererInstruction instructionOutput, Skeleton skeleton, Dictionary<Slot, Material> customSlotMaterials, List<Slot> separatorSlots, bool generateMeshOverride, bool immutableTriangles = false, bool renderMeshes = true) {
//			if (skeleton == null) throw new ArgumentNullException("skeleton");
//			if (instructionOutput == null) throw new ArgumentNullException("instructionOutput");

			ExposedList<Slot> drawOrder = skeleton.drawOrder;
			int drawOrderCount = drawOrder.Count;

			// Clear last state of attachments and submeshes
			instructionOutput.Clear(); // submeshInstructions.Clear(); attachments.Clear();
			var workingSubmeshInstructions = instructionOutput.submeshInstructions;
			#if SPINE_TRIANGLECHECK
			instructionOutput.attachments.Resize(drawOrderCount);
			var workingAttachmentsItems = instructionOutput.attachments.Items;
			int totalRawVertexCount = 0;
			bool skeletonHasClipping = false;
			#endif

			var current = new SubmeshInstruction {
				skeleton = skeleton,
				preActiveClippingSlotSource = -1
			};

			#if !SPINE_TK2D
			bool isCustomSlotMaterialsPopulated = customSlotMaterials != null && customSlotMaterials.Count > 0;
			#endif

			int separatorCount = separatorSlots == null ? 0 : separatorSlots.Count;
			bool hasSeparators = separatorCount > 0;

			int clippingAttachmentSource = -1;
			int lastPreActiveClipping = -1;
			SlotData clippingEndSlot = null;
			int submeshIndex = 0;
			var drawOrderItems = drawOrder.Items;
			bool currentHasRenderable = false;
			for (int i = 0; i < drawOrderCount; i++) {
				Slot slot = drawOrderItems[i];
				Attachment attachment = slot.attachment;
				#if SPINE_TRIANGLECHECK
				workingAttachmentsItems[i] = attachment;
				int attachmentVertexCount = 0, attachmentTriangleCount = 0;
				#endif

				object rendererObject = null; // An AtlasRegion in plain Spine-Unity. Spine-TK2D hooks into TK2D's system. eventual source of Material object.
				bool noRender = false; // Using this allows empty slots as separators, and keeps separated parts more stable despite slots being reordered

				var regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {
					rendererObject = regionAttachment.RendererObject;
					#if SPINE_TRIANGLECHECK
					attachmentVertexCount = 4;
					attachmentTriangleCount = 6;
					#endif
					currentHasRenderable = true;
				} else {
					if (renderMeshes) {
						var meshAttachment = attachment as MeshAttachment;
						if (meshAttachment != null) {
							rendererObject = meshAttachment.RendererObject;
							#if SPINE_TRIANGLECHECK
							attachmentVertexCount = meshAttachment.worldVerticesLength >> 1;
							attachmentTriangleCount = meshAttachment.triangles.Length;
							#endif
							currentHasRenderable = true;
						} else {
							#if SPINE_TRIANGLECHECK
							var clippingAttachment = attachment as ClippingAttachment;
							if (clippingAttachment != null) {
								clippingEndSlot = clippingAttachment.endSlot;
								clippingAttachmentSource = i;
								current.hasClipping = true;
								skeletonHasClipping = true;
							}
							#endif
							noRender = true;
							//continue;
						}
					} else {
						noRender = true;
					}
				}

				if (clippingEndSlot != null && slot.data == clippingEndSlot) {
					clippingEndSlot = null;
					clippingAttachmentSource = -1;
				}

				// Create a new SubmeshInstruction when material changes. (or when forced to separate by a submeshSeparator)
				// Slot with a separator/new material will become the starting slot of the next new instruction.
				if (hasSeparators) { //current.forceSeparate = hasSeparators && separatorSlots.Contains(slot);
					current.forceSeparate = false;
					for (int s = 0; s < separatorCount; s++) {
						if (Slot.ReferenceEquals(slot, separatorSlots[s])) {
							current.forceSeparate = true;
							break;
						}
					}
				}

				if (noRender) {
					if (current.forceSeparate && currentHasRenderable && generateMeshOverride) {
						{ // Add
							current.endSlot = i;
							current.preActiveClippingSlotSource = lastPreActiveClipping;

							workingSubmeshInstructions.Resize(submeshIndex + 1);
							workingSubmeshInstructions.Items[submeshIndex] = current;

							submeshIndex++;
						}
						currentHasRenderable = false;
						current.startSlot = i;
						lastPreActiveClipping = clippingAttachmentSource;
						#if SPINE_TRIANGLECHECK
						current.rawTriangleCount = 0;
						current.rawVertexCount = 0;
						current.rawFirstVertexIndex = totalRawVertexCount;
						current.hasClipping = clippingAttachmentSource >= 0;
						#endif
					}
				} else {
					#if !SPINE_TK2D
					Material material;
					if (isCustomSlotMaterialsPopulated) {
						if (!customSlotMaterials.TryGetValue(slot, out material))
							material = (Material)((AtlasRegion)rendererObject).page.rendererObject;				
					} else {
						material = (Material)((AtlasRegion)rendererObject).page.rendererObject;
					}
					#else
					Material material = (rendererObject.GetType() == typeof(Material)) ? (Material)rendererObject : (Material)((AtlasRegion)rendererObject).page.rendererObject;
					#endif

					if (currentHasRenderable && (current.forceSeparate || !System.Object.ReferenceEquals(current.material, material))) { // Material changed. Add the previous submesh.
						{ // Add
							current.endSlot = i;
							current.preActiveClippingSlotSource = lastPreActiveClipping;
				
							workingSubmeshInstructions.Resize(submeshIndex + 1);
							workingSubmeshInstructions.Items[submeshIndex] = current;
							submeshIndex++;
						}
						currentHasRenderable = false;
						current.startSlot = i;
						lastPreActiveClipping = clippingAttachmentSource;
						#if SPINE_TRIANGLECHECK
						current.rawTriangleCount = 0;
						current.rawVertexCount = 0;
						current.rawFirstVertexIndex = totalRawVertexCount;
						current.hasClipping = clippingAttachmentSource >= 0;
						#endif
					}

					// Update state for the next Attachment.
					current.material = material;
					#if SPINE_TRIANGLECHECK
					current.rawTriangleCount += attachmentTriangleCount;
					current.rawVertexCount += attachmentVertexCount;
					current.rawFirstVertexIndex = totalRawVertexCount;
					totalRawVertexCount += attachmentVertexCount;
					#endif
				}
			}
				
			if (currentHasRenderable) {
				{ // Add last or only submesh.
					current.endSlot = drawOrderCount;
					current.preActiveClippingSlotSource = lastPreActiveClipping;
					current.forceSeparate = false;

					workingSubmeshInstructions.Resize(submeshIndex + 1);
					workingSubmeshInstructions.Items[submeshIndex] = current;
					//submeshIndex++;	
				}
			}

			#if SPINE_TRIANGLECHECK
			instructionOutput.hasActiveClipping = skeletonHasClipping;
			instructionOutput.rawVertexCount = totalRawVertexCount;
			#endif
			instructionOutput.immutableTriangles = immutableTriangles;
		}

		public static void TryReplaceMaterials (ExposedList<SubmeshInstruction> workingSubmeshInstructions, Dictionary<Material, Material> customMaterialOverride) {
			// Material overrides are done here so they can be applied per submesh instead of per slot
			// but they will still be passed through the GenerateMeshOverride delegate,
			// and will still go through the normal material match check step in STEP 3.
			var workingSubmeshInstructionsItems = workingSubmeshInstructions.Items;
			for (int i = 0; i < workingSubmeshInstructions.Count; i++) {
				var m = workingSubmeshInstructionsItems[i].material;
				Material mo;
				if (customMaterialOverride.TryGetValue(m, out mo))
					workingSubmeshInstructionsItems[i].material = mo;
			}
		}
		#endregion

		#region Step 2 : Populate vertex data and triangle index buffers.
		public void BeginNewMesh () {
			vertexBuffer.Clear(false);
			colorBuffer.Clear(false);
			uvBuffer.Clear(false);
			clipper.ClipEnd();

			{
				meshBoundsMin.x = BoundsMinDefault;
				meshBoundsMin.y = BoundsMinDefault;
				meshBoundsMax.x = BoundsMaxDefault;
				meshBoundsMax.y = BoundsMaxDefault;
				meshBoundsThickness = 0f;
			}

			submeshes.Count = 1;
			submeshes.Items[0].Clear(false);
			submeshIndex = 0;
		}

		public void AddSubmesh (SubmeshInstruction instruction) {
			var settings = this.settings;
			if (!settings.renderMeshes) {
				AddSubmeshQuadsOnly(instruction);
				return;
			}

			if (submeshes.Count - 1 < submeshIndex) {
				submeshes.Resize(submeshIndex + 1);
				if (submeshes.Items[submeshIndex] == null)
					submeshes.Items[submeshIndex] = new ExposedList<int>();
			}
			var submesh = submeshes.Items[submeshIndex];
			submesh.Clear(false);

			var skeleton = instruction.skeleton;
			var drawOrderItems = skeleton.drawOrder.Items;

			Color32 color;
			float skeletonA = skeleton.a * 255, skeletonR = skeleton.r, skeletonG = skeleton.g, skeletonB = skeleton.b;
			Vector2 meshBoundsMin = this.meshBoundsMin, meshBoundsMax = this.meshBoundsMax;

			// Settings
			float zSpacing = settings.zSpacing;
			#if SPINE_TRIANGLECHECK
			bool useClipping = settings.useClipping && instruction.hasClipping;
			#else
			bool useClipping = settings.useClipping;
			#endif

			if (useClipping) {
				if (instruction.preActiveClippingSlotSource >= 0) {
					Debug.Log("PreActiveClipping");
					var slot = drawOrderItems[instruction.preActiveClippingSlotSource];
					clipper.ClipStart(slot, slot.attachment as ClippingAttachment);
				}	
			}

			bool pmaVertexColors = settings.pmaVertexColors;
			for (int slotIndex = instruction.startSlot; slotIndex < instruction.endSlot; slotIndex++) {
				var slot = drawOrderItems[slotIndex];
				var attachment = slot.attachment;
				float z = zSpacing * slotIndex;

				var workingVerts = this.tempVerts;
				float[] uvs;
				int[] attachmentTriangleIndices;
				int attachmentVertexCount;
				int attachmentIndexCount;

				Color c = default(Color);

				var region = attachment as RegionAttachment;
				if (region != null) {
					region.ComputeWorldVertices(slot.bone, workingVerts, 0);
					uvs = region.uvs;
					attachmentTriangleIndices = regionTriangles;
					c.r = region.r; c.g = region.g; c.b = region.b; c.a = region.a;
					attachmentVertexCount = 4;
					attachmentIndexCount = 6;
				} else {
					var mesh = attachment as MeshAttachment;
					if (mesh != null) {
						int meshVertexCount = mesh.worldVerticesLength;
						if (workingVerts.Length < meshVertexCount) {
							workingVerts = new float[meshVertexCount];
							this.tempVerts = workingVerts;
						}
						mesh.ComputeWorldVertices(slot, 0, meshVertexCount, workingVerts, 0); //meshAttachment.ComputeWorldVertices(slot, tempVerts);
						uvs = mesh.uvs;
						attachmentTriangleIndices = mesh.triangles;
						c.r = mesh.r; c.g = mesh.g; c.b = mesh.b; c.a = mesh.a;
						attachmentVertexCount = meshVertexCount >> 1; // meshVertexCount / 2;
						attachmentIndexCount = mesh.triangles.Length;
					} else {
						if (useClipping) {
							var clippingAttachment = attachment as ClippingAttachment;
							if (clippingAttachment != null) {
								clipper.ClipStart(slot, clippingAttachment);
								continue;
							}
						}

						continue;
					}
				}

				if (pmaVertexColors) {
					color.a = (byte)(skeletonA * slot.a * c.a);
					color.r = (byte)(skeletonR * slot.r * c.r * color.a);
					color.g = (byte)(skeletonG * slot.g * c.g * color.a);
					color.b = (byte)(skeletonB * slot.b * c.b * color.a);
					if (slot.data.blendMode == BlendMode.Additive) color.a = 0;
				} else {
					color.a = (byte)(skeletonA * slot.a * c.a);
					color.r = (byte)(skeletonR * slot.r * c.r * 255);
					color.g = (byte)(skeletonG * slot.g * c.g * 255);
					color.b = (byte)(skeletonB * slot.b * c.b * 255);
				}

				if (useClipping && clipper.IsClipping()) {					
					clipper.ClipTriangles(workingVerts, attachmentVertexCount << 1, attachmentTriangleIndices, attachmentIndexCount, uvs);
					workingVerts = clipper.clippedVertices.Items;
					attachmentVertexCount = clipper.clippedVertices.Count >> 1;
					attachmentTriangleIndices = clipper.clippedTriangles.Items;
					attachmentIndexCount = clipper.clippedTriangles.Count;
					uvs = clipper.clippedUVs.Items;
				}

				if (attachmentVertexCount != 0 && attachmentIndexCount != 0) {
					if (settings.tintBlack)
						AddAttachmentTintBlack(slot.r2, slot.g2, slot.b2, attachmentVertexCount);

					//AddAttachment(workingVerts, uvs, color, attachmentTriangleIndices, attachmentVertexCount, attachmentIndexCount, ref meshBoundsMin, ref meshBoundsMax, z);
					int ovc = vertexBuffer.Count;
					// Add data to vertex buffers
					{
						int newVertexCount = ovc + attachmentVertexCount;
						if (newVertexCount > vertexBuffer.Items.Length) { // Manual ExposedList.Resize()
							Array.Resize(ref vertexBuffer.Items, newVertexCount);
							Array.Resize(ref uvBuffer.Items, newVertexCount);
							Array.Resize(ref colorBuffer.Items, newVertexCount);
						}
						vertexBuffer.Count = uvBuffer.Count = colorBuffer.Count = newVertexCount;
					}

					var vbi = vertexBuffer.Items;
					var ubi = uvBuffer.Items;
					var cbi = colorBuffer.Items;
					for (int i = 0; i < attachmentVertexCount; i++) {
						int vi = ovc + i;
						int i2 = i << 1; // i * 2
						float x = workingVerts[i2];
						float y = workingVerts[i2 + 1];

						vbi[vi].x = x;
						vbi[vi].y = y;
						vbi[vi].z = z;
						ubi[vi].x = uvs[i2];
						ubi[vi].y = uvs[i2 + 1];
						cbi[vi] = color;

						// Calculate bounds.
						if (x < meshBoundsMin.x) meshBoundsMin.x = x;
						else if (x > meshBoundsMax.x) meshBoundsMax.x = x;
						if (y < meshBoundsMin.y) meshBoundsMin.y = y;
						else if (y > meshBoundsMax.y) meshBoundsMax.y = y;
					}

					// Add data to triangle buffer
					int oldTriangleCount = submesh.Count;
					{ //submesh.Resize(oldTriangleCount + attachmentIndexCount);
						int newTriangleCount = oldTriangleCount + attachmentIndexCount;
						if (newTriangleCount > submesh.Items.Length) Array.Resize(ref submesh.Items, newTriangleCount); 
						submesh.Count = newTriangleCount;
					}
					var submeshItems = submesh.Items;
					for (int i = 0; i < attachmentIndexCount; i++)
						submeshItems[oldTriangleCount + i] = attachmentTriangleIndices[i] + ovc;
				}

				clipper.ClipEnd(slot);
			}
			clipper.ClipEnd();
				
			this.meshBoundsMin = meshBoundsMin;
			this.meshBoundsMax = meshBoundsMax;
			meshBoundsThickness = instruction.endSlot * zSpacing;

			// Trim or zero submesh triangles.
			var currentSubmeshItems = submesh.Items;
			for (int i = submesh.Count, n = currentSubmeshItems.Length; i < n; i++)
				currentSubmeshItems[i] = 0;

			// Next AddSubmesh will use a new submeshIndex value.
			submeshIndex++;
		}

		public void ScaleVertexData (float scale) {
			var vbi = vertexBuffer.Items;
			for (int i = 0, n = vertexBuffer.Count; i < n; i++) {
//				vbi[i].x *= scale;
//				vbi[i].y *= scale;
				vbi[i] *= scale;
			}

			meshBoundsMin *= scale;
			meshBoundsMax *= scale;
			meshBoundsThickness *= scale;
		}

		void AddSubmeshQuadsOnly (SubmeshInstruction instruction) {
			const int attachmentVertexCount = 4;
			const int attachmentIndexCount = 6;
			int[] attachmentTriangleIndices = regionTriangles;

			var settings = this.settings;

			if (submeshes.Count - 1 < submeshIndex) {
				submeshes.Resize(submeshIndex + 1);
				if (submeshes.Items[submeshIndex] == null)
					submeshes.Items[submeshIndex] = new ExposedList<int>();
			}
			var submesh = submeshes.Items[submeshIndex];
			submesh.Clear(false);

			var skeleton = instruction.skeleton;
			var drawOrderItems = skeleton.drawOrder.Items;

			Color32 color;
			float skeletonA = skeleton.a * 255, skeletonR = skeleton.r, skeletonG = skeleton.g, skeletonB = skeleton.b;
			Vector2 meshBoundsMin = this.meshBoundsMin, meshBoundsMax = this.meshBoundsMax;

			// Settings
			float zSpacing = settings.zSpacing;

			bool pmaVertexColors = settings.pmaVertexColors;
			for (int slotIndex = instruction.startSlot; slotIndex < instruction.endSlot; slotIndex++) {
				var slot = drawOrderItems[slotIndex];
				var attachment = slot.attachment;
				float z = zSpacing * slotIndex;

				var workingVerts = this.tempVerts;
				float[] uvs;

				Color c = default(Color);

				var region = attachment as RegionAttachment;
				if (region != null) {
					region.ComputeWorldVertices(slot.bone, workingVerts, 0);
					uvs = region.uvs;
					c.r = region.r; c.g = region.g; c.b = region.b; c.a = region.a;
				} else {
					continue;
				}

				if (pmaVertexColors) {
					color.a = (byte)(skeletonA * slot.a * c.a);
					color.r = (byte)(skeletonR * slot.r * c.r * color.a);
					color.g = (byte)(skeletonG * slot.g * c.g * color.a);
					color.b = (byte)(skeletonB * slot.b * c.b * color.a);
					if (slot.data.blendMode == BlendMode.Additive) color.a = 0;
				} else {
					color.a = (byte)(skeletonA * slot.a * c.a);
					color.r = (byte)(skeletonR * slot.r * c.r * 255);
					color.g = (byte)(skeletonG * slot.g * c.g * 255);
					color.b = (byte)(skeletonB * slot.b * c.b * 255);
				}

				{
					if (settings.tintBlack)
						AddAttachmentTintBlack(slot.r2, slot.g2, slot.b2, attachmentVertexCount);

					//AddAttachment(workingVerts, uvs, color, attachmentTriangleIndices, attachmentVertexCount, attachmentIndexCount, ref meshBoundsMin, ref meshBoundsMax, z);
					int ovc = vertexBuffer.Count;
					// Add data to vertex buffers
					{
						int newVertexCount = ovc + attachmentVertexCount;
						if (newVertexCount > vertexBuffer.Items.Length) { // Manual ExposedList.Resize()
							Array.Resize(ref vertexBuffer.Items, newVertexCount);
							Array.Resize(ref uvBuffer.Items, newVertexCount);
							Array.Resize(ref colorBuffer.Items, newVertexCount);
						}
						vertexBuffer.Count = uvBuffer.Count = colorBuffer.Count = newVertexCount;
					}

					var vbi = vertexBuffer.Items;
					var ubi = uvBuffer.Items;
					var cbi = colorBuffer.Items;
					for (int i = 0; i < attachmentVertexCount; i++) {
						int vi = ovc + i;
						int i2 = i << 1; // i * 2
						float x = workingVerts[i2];
						float y = workingVerts[i2 + 1];

						vbi[vi].x = x;
						vbi[vi].y = y;
						vbi[vi].z = z;
						ubi[vi].x = uvs[i2];
						ubi[vi].y = uvs[i2 + 1];
						cbi[vi] = color;

						// Calculate bounds.
						if (x < meshBoundsMin.x) meshBoundsMin.x = x;
						else if (x > meshBoundsMax.x) meshBoundsMax.x = x;
						if (y < meshBoundsMin.y) meshBoundsMin.y = y;
						else if (y > meshBoundsMax.y) meshBoundsMax.y = y;
					}

					// TODO: Simplify triangle buffer handling.
					// Add data to triangle buffer
					int oldTriangleCount = submesh.Count;
					{ //submesh.Resize(oldTriangleCount + attachmentIndexCount);
						int newTriangleCount = oldTriangleCount + attachmentIndexCount;
						if (newTriangleCount > submesh.Items.Length) Array.Resize(ref submesh.Items, newTriangleCount); 
						submesh.Count = newTriangleCount;
					}
					var submeshItems = submesh.Items;
					for (int i = 0; i < attachmentIndexCount; i++)
						submeshItems[oldTriangleCount + i] = attachmentTriangleIndices[i] + ovc;
				}
			}

			this.meshBoundsMin = meshBoundsMin;
			this.meshBoundsMax = meshBoundsMax;
			meshBoundsThickness = instruction.endSlot * zSpacing;

			// Trim or zero submesh triangles.
			var currentSubmeshItems = submesh.Items;
			for (int i = submesh.Count, n = currentSubmeshItems.Length; i < n; i++)
				currentSubmeshItems[i] = 0;

			// Next AddSubmesh will use a new submeshIndex value.
			submeshIndex++;
		}

		void AddAttachmentTintBlack (float r2, float g2, float b2, int vertexCount) {
			var rg = new Vector2(r2, g2);
			var bo = new Vector2(b2, 1f);

			int ovc = vertexBuffer.Count;				
			int newVertexCount = ovc + vertexCount;
			{ 
				if (uv2 == null) {
					uv2 = new ExposedList<Vector2>();
					uv3 = new ExposedList<Vector2>();
				}
				if (newVertexCount > uv2.Items.Length) { // Manual ExposedList.Resize()
					Array.Resize(ref uv2.Items, newVertexCount);
					Array.Resize(ref uv3.Items, newVertexCount);
				}
				uv2.Count = uv3.Count = newVertexCount;
			}

			var uv2i = uv2.Items;
			var uv3i = uv3.Items;
			for (int i = 0; i < vertexCount; i++) {
				uv2i[ovc + i] = rg;
				uv3i[ovc + i] = bo;
			}
		}
		#endregion

		#region Step 3 : Transfer vertex and triangle data to UnityEngine.Mesh
		public void FillVertexData (Mesh mesh) {
			var vbi = vertexBuffer.Items;
			var ubi = uvBuffer.Items;
			var cbi = colorBuffer.Items;
			var sbi = submeshes.Items;
			int submeshCount = submeshes.Count;

			// Zero the extra.
			{
				int listCount = vertexBuffer.Count;
				int arrayLength = vertexBuffer.Items.Length;
				var vector3zero = Vector3.zero;
				for (int i = listCount; i < arrayLength; i++)
					vbi[i] = vector3zero;
			}

			// Set the vertex buffer.
			{
				mesh.vertices = vbi;
				mesh.uv = ubi;
				mesh.colors32 = cbi;

				if (meshBoundsMin.x == BoundsMinDefault) {
					mesh.bounds = new Bounds();
				} else {
					Vector2 halfSize = (meshBoundsMax - meshBoundsMin) * 0.5f;
					mesh.bounds = new Bounds {
						center = (Vector3)(meshBoundsMin + halfSize),
						extents = new Vector3(halfSize.x, halfSize.y, meshBoundsThickness * 0.5f)
					};
					//mesh.bounds = ArraysMeshGenerator.ToBounds(meshBoundsMin, meshBoundsMax);
				}
			}

			{
				int vertexCount = this.vertexBuffer.Count;
				if (settings.addNormals) {
					int oldLength = 0;

					if (normals == null)
						normals = new Vector3[vertexCount];	
					else
						oldLength = normals.Length;

					if (oldLength < vertexCount) {
						Array.Resize(ref this.normals, vertexCount);
						var localNormals = this.normals;
						for (int i = oldLength; i < vertexCount; i++) localNormals[i] = Vector3.back;
					}
					mesh.normals = this.normals;
				}

				if (settings.tintBlack) {
					mesh.uv2 = this.uv2.Items;
					mesh.uv3 = this.uv3.Items;
				}

				if (settings.calculateTangents) {
					MeshGenerator.SolveTangents2DEnsureSize(ref this.tangents, ref this.tempTanBuffer, vertexCount);
					for (int i = 0; i < submeshCount; i++) {
						var submesh = sbi[i].Items;
						int triangleCount = sbi[i].Count;
						MeshGenerator.SolveTangents2DTriangles(this.tempTanBuffer, submesh, triangleCount, vbi, ubi, vertexCount);
					}
					MeshGenerator.SolveTangents2DBuffer(this.tangents, this.tempTanBuffer, vertexCount);
					mesh.tangents = this.tangents;
				}
			}
		}

		public void FillTriangles (Mesh mesh) {
			int submeshCount = submeshes.Count;
			var submeshesItems = submeshes.Items;
			mesh.subMeshCount = submeshCount;

			for (int i = 0; i < submeshCount; i++)
				mesh.SetTriangles(submeshesItems[i].Items, i, false);				
		}

		public void FillTrianglesSingle (Mesh mesh) {
			mesh.SetTriangles(submeshes.Items[0].Items, 0, false);
		}
		#endregion

		public void TrimExcess () {
			vertexBuffer.TrimExcess();
			uvBuffer.TrimExcess();
			colorBuffer.TrimExcess();

			if (uv2 != null) uv2.TrimExcess();
			if (uv3 != null) uv3.TrimExcess();

			int count = vertexBuffer.Count;
			if (normals != null) Array.Resize(ref normals, count);
			if (tangents != null) Array.Resize(ref tangents, count);
		}

		#region TangentSolver2D
		// Thanks to contributions from forum user ToddRivers

		/// <summary>Step 1 of solving tangents. Ensure you have buffers of the correct size.</summary>
		/// <param name="tangentBuffer">Eventual Vector4[] tangent buffer to assign to Mesh.tangents.</param>
		/// <param name="tempTanBuffer">Temporary Vector2 buffer for calculating directions.</param>
		/// <param name="vertexCount">Number of vertices that require tangents (or the size of the vertex array)</param>
		internal static void SolveTangents2DEnsureSize (ref Vector4[] tangentBuffer, ref Vector2[] tempTanBuffer, int vertexCount) {
			if (tangentBuffer == null || tangentBuffer.Length < vertexCount)
				tangentBuffer = new Vector4[vertexCount];

			if (tempTanBuffer == null || tempTanBuffer.Length < vertexCount * 2)
				tempTanBuffer = new Vector2[vertexCount * 2]; // two arrays in one.
		}

		/// <summary>Step 2 of solving tangents. Fills (part of) a temporary tangent-solution buffer based on the vertices and uvs defined by a submesh's triangle buffer. Only needs to be called once for single-submesh meshes.</summary>
		/// <param name="tempTanBuffer">A temporary Vector3[] for calculating tangents.</param>
		/// <param name="vertices">The mesh's current vertex position buffer.</param>
		/// <param name="triangles">The mesh's current triangles buffer.</param>
		/// <param name="uvs">The mesh's current uvs buffer.</param>
		/// <param name="vertexCount">Number of vertices that require tangents (or the size of the vertex array)</param>
		/// <param name = "triangleCount">The number of triangle indexes in the triangle array to be used.</param>
		internal static void SolveTangents2DTriangles (Vector2[] tempTanBuffer, int[] triangles, int triangleCount, Vector3[] vertices, Vector2[] uvs, int vertexCount) {
			Vector2 sdir;
			Vector2 tdir;
			for (int t = 0; t < triangleCount; t += 3) {
				int i1 = triangles[t + 0];
				int i2 = triangles[t + 1];
				int i3 = triangles[t + 2];

				Vector3 v1 = vertices[i1];
				Vector3 v2 = vertices[i2];
				Vector3 v3 = vertices[i3];

				Vector2 w1 = uvs[i1];
				Vector2 w2 = uvs[i2];
				Vector2 w3 = uvs[i3];

				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;

				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;

				float div = s1 * t2 - s2 * t1;
				float r = (div == 0f) ? 0f : 1f / div;

				sdir.x = (t2 * x1 - t1 * x2) * r;
				sdir.y = (t2 * y1 - t1 * y2) * r;
				tempTanBuffer[i1] = tempTanBuffer[i2] = tempTanBuffer[i3] = sdir;

				tdir.x = (s1 * x2 - s2 * x1) * r;
				tdir.y = (s1 * y2 - s2 * y1) * r;
				tempTanBuffer[vertexCount + i1] = tempTanBuffer[vertexCount + i2] = tempTanBuffer[vertexCount + i3] = tdir;
			}
		}

		/// <summary>Step 3 of solving tangents. Fills a Vector4[] tangents array according to values calculated in step 2.</summary>
		/// <param name="tangents">A Vector4[] that will eventually be used to set Mesh.tangents</param>
		/// <param name="tempTanBuffer">A temporary Vector3[] for calculating tangents.</param>
		/// <param name="vertexCount">Number of vertices that require tangents (or the size of the vertex array)</param>
		internal static void SolveTangents2DBuffer (Vector4[] tangents, Vector2[] tempTanBuffer, int vertexCount) {

			Vector4 tangent;
			tangent.z = 0;
			for (int i = 0; i < vertexCount; ++i) {
				Vector2 t = tempTanBuffer[i]; 

				// t.Normalize() (aggressively inlined). Even better if offloaded to GPU via vertex shader.
				float magnitude = Mathf.Sqrt(t.x * t.x + t.y * t.y);
				if (magnitude > 1E-05) {
					float reciprocalMagnitude = 1f/magnitude;
					t.x *= reciprocalMagnitude;
					t.y *= reciprocalMagnitude;
				}

				Vector2 t2 = tempTanBuffer[vertexCount + i];
				tangent.x = t.x;
				tangent.y = t.y;
				//tangent.z = 0;
				tangent.w = (t.y * t2.x > t.x * t2.y) ? 1 : -1; // 2D direction calculation. Used for binormals.
				tangents[i] = tangent;
			}

		}
		#endregion
	}

	public class MeshRendererBuffers : IDisposable {
		DoubleBuffered<SmartMesh> doubleBufferedMesh;
		internal readonly ExposedList<Material> submeshMaterials = new ExposedList<Material>();
		internal Material[] sharedMaterials = new Material[0];

		public void Initialize () {
			doubleBufferedMesh = new DoubleBuffered<SmartMesh>();
		}

		public Material[] GetUpdatedShaderdMaterialsArray () {
			if (submeshMaterials.Count == sharedMaterials.Length)
				submeshMaterials.CopyTo(sharedMaterials);
			else
				sharedMaterials = submeshMaterials.ToArray();

			return sharedMaterials;
		}

		public bool MaterialsChangedInLastUpdate () {
			int newSubmeshMaterials = submeshMaterials.Count;
			var sharedMaterials = this.sharedMaterials;
			if (newSubmeshMaterials != sharedMaterials.Length) return true;

			var submeshMaterialsItems = submeshMaterials.Items;
			for (int i = 0; i < newSubmeshMaterials; i++)
				if (!Material.ReferenceEquals(submeshMaterialsItems[i], sharedMaterials[i])) return true; //if (submeshMaterialsItems[i].GetInstanceID() != sharedMaterials[i].GetInstanceID()) return true;

			return false;
		}

		public void UpdateSharedMaterials (ExposedList<SubmeshInstruction> instructions) {
			int newSize = instructions.Count;
			{ //submeshMaterials.Resize(instructions.Count);
				if (newSize > submeshMaterials.Items.Length)
					Array.Resize(ref submeshMaterials.Items, newSize);
				submeshMaterials.Count = newSize;
			}

			var submeshMaterialsItems = submeshMaterials.Items;
			var instructionsItems = instructions.Items;
			for (int i = 0; i < newSize; i++)
				submeshMaterialsItems[i] = instructionsItems[i].material;
		}

		public SmartMesh GetNextMesh () {
			return doubleBufferedMesh.GetNext();
		}

		public void Clear () {
			sharedMaterials = new Material[0];
			submeshMaterials.Clear();
		}

		public void Dispose () {
			if (doubleBufferedMesh == null) return;
			doubleBufferedMesh.GetNext().Dispose();
			doubleBufferedMesh.GetNext().Dispose();
			doubleBufferedMesh = null;
		}

		///<summary>This is a Mesh that also stores the instructions SkeletonRenderer generated for it.</summary>
		public class SmartMesh : IDisposable {
			public Mesh mesh = SpineMesh.NewMesh();
			public SkeletonRendererInstruction instructionUsed = new SkeletonRendererInstruction();		

			public void Dispose () {
				if (mesh != null) {
					#if UNITY_EDITOR
					if (Application.isEditor && !Application.isPlaying)
						UnityEngine.Object.DestroyImmediate(mesh);
					else
						UnityEngine.Object.Destroy(mesh);
					#else
					UnityEngine.Object.Destroy(mesh);
					#endif
				}
				mesh = null;
			}
		}
	}

	public class SkeletonRendererInstruction {
		public bool immutableTriangles;
		public readonly ExposedList<SubmeshInstruction> submeshInstructions = new ExposedList<SubmeshInstruction>();

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
				this.hasActiveClipping = instruction.hasClipping;
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

			var drawOrder = instructionsItems[0].skeleton.drawOrder.Items;
			for (int i = 0; i < attachmentCount; i++)
				attachmentsItems[i] = drawOrder[startSlot + i].attachment;
			#endif
		}

		public void Set (SkeletonRendererInstruction other) {
			this.immutableTriangles = other.immutableTriangles;

			#if SPINE_TRIANGLECHECK
			this.hasActiveClipping = other.hasActiveClipping;
			this.rawVertexCount = other.rawVertexCount;
			this.attachments.Clear(false);
			this.attachments.GrowIfNeeded(other.attachments.Capacity);
			this.attachments.Count = other.attachments.Count;
			other.attachments.CopyTo(this.attachments.Items);
			#endif

			this.submeshInstructions.Clear(false);
			this.submeshInstructions.GrowIfNeeded(other.submeshInstructions.Capacity);
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
