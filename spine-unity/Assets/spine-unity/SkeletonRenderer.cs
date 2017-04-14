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

#define SPINE_OPTIONAL_RENDEROVERRIDE
#define SPINE_OPTIONAL_MATERIALOVERRIDE
#define SPINE_OPTIONAL_NORMALS
#define SPINE_OPTIONAL_SOLVETANGENTS

//#define SPINE_OPTIONAL_FRONTFACING

using System.Collections.Generic;
using UnityEngine;
using Spine.Unity.MeshGeneration;

namespace Spine.Unity {
	/// <summary>Renders a skeleton.</summary>
	[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), DisallowMultipleComponent]
	[HelpURL("http://esotericsoftware.com/spine-unity-documentation#Rendering")]
	public class SkeletonRenderer : MonoBehaviour, ISkeletonComponent {

		public delegate void SkeletonRendererDelegate (SkeletonRenderer skeletonRenderer);
		public SkeletonRendererDelegate OnRebuild;

		public SkeletonDataAsset skeletonDataAsset;
		public SkeletonDataAsset SkeletonDataAsset { get { return skeletonDataAsset; } } // ISkeletonComponent
		public string initialSkinName;

		#region Advanced
		// Submesh Separation
		[UnityEngine.Serialization.FormerlySerializedAs("submeshSeparators")]
		[SpineSlot]
		public string[] separatorSlotNames = new string[0];
		[System.NonSerialized]
		public readonly List<Slot> separatorSlots = new List<Slot>();

		public float zSpacing;
		public bool renderMeshes = true, immutableTriangles;
		public bool pmaVertexColors = true;
		public bool clearStateOnDisable = false;

		#if SPINE_OPTIONAL_NORMALS
		public bool calculateNormals;
		#endif
		#if SPINE_OPTIONAL_SOLVETANGENTS
		public bool calculateTangents;
		#endif
		#if SPINE_OPTIONAL_FRONTFACING
		public bool frontFacing;
		#endif

		public bool logErrors = false;

		#if SPINE_OPTIONAL_RENDEROVERRIDE
		public bool disableRenderingOnOverride = true;
		public delegate void InstructionDelegate (SkeletonRenderer.SmartMesh.Instruction instruction);
		event InstructionDelegate generateMeshOverride;
		public event InstructionDelegate GenerateMeshOverride {
			add {
				generateMeshOverride += value;
				if (disableRenderingOnOverride && generateMeshOverride != null) {
					Initialize(false);
					meshRenderer.enabled = false;
				}
			}
			remove {
				generateMeshOverride -= value;
				if (disableRenderingOnOverride && generateMeshOverride == null) {
					Initialize(false);
					meshRenderer.enabled = true;
				}
			}
		}
		#endif

		#if SPINE_OPTIONAL_MATERIALOVERRIDE
		[System.NonSerialized] readonly Dictionary<Material, Material> customMaterialOverride = new Dictionary<Material, Material>();
		public Dictionary<Material, Material> CustomMaterialOverride { get { return customMaterialOverride; } }
		#endif

		// Custom Slot Material
		[System.NonSerialized] readonly Dictionary<Slot, Material> customSlotMaterials = new Dictionary<Slot, Material>();
		public Dictionary<Slot, Material> CustomSlotMaterials { get { return customSlotMaterials; } }
		#endregion

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		[System.NonSerialized] public bool valid;
		[System.NonSerialized] public Skeleton skeleton;
		public Skeleton Skeleton {
			get {
				Initialize(false);
				return skeleton;
			}
		}

		Spine.Unity.DoubleBuffered<SkeletonRenderer.SmartMesh> doubleBufferedMesh;
		readonly SmartMesh.Instruction currentInstructions = new SmartMesh.Instruction();
		readonly ExposedList<ArraysMeshGenerator.SubmeshTriangleBuffer> submeshes = new ExposedList<ArraysMeshGenerator.SubmeshTriangleBuffer>();
		readonly ExposedList<Material> submeshMaterials = new ExposedList<Material>();
		Material[] sharedMaterials = new Material[0];
		float[] tempVertices = new float[8];
		Vector3[] vertices;
		Color32[] colors;
		Vector2[] uvs;
		#if SPINE_OPTIONAL_NORMALS
		Vector3[] normals;
		#endif
		#if SPINE_OPTIONAL_SOLVETANGENTS
		Vector4[] tangents;
		Vector2[] tempTanBuffer;
		#endif

		#region Runtime Instantiation
		public static T NewSpineGameObject<T> (SkeletonDataAsset skeletonDataAsset) where T : SkeletonRenderer {
			return SkeletonRenderer.AddSpineComponent<T>(new GameObject("New Spine GameObject"), skeletonDataAsset);
		}

		/// <summary>Add and prepare a Spine component that derives from SkeletonRenderer to a GameObject at runtime.</summary>
		/// <typeparam name="T">T should be SkeletonRenderer or any of its derived classes.</typeparam>
		public static T AddSpineComponent<T> (GameObject gameObject, SkeletonDataAsset skeletonDataAsset) where T : SkeletonRenderer {
			var c = gameObject.AddComponent<T>();
			if (skeletonDataAsset != null) {
				c.skeletonDataAsset = skeletonDataAsset;
				c.Initialize(false);
			}
			return c;
		}
		#endregion

		public virtual void Awake () {
			Initialize(false);
		}

		void OnDisable () {
			if (clearStateOnDisable && valid)
				ClearState();
		}

		void OnDestroy () {
			if (doubleBufferedMesh == null) return;
			doubleBufferedMesh.GetNext().Dispose();
			doubleBufferedMesh.GetNext().Dispose();
			doubleBufferedMesh = null;
		}

		protected virtual void ClearState () {
			meshFilter.sharedMesh = null;
			currentInstructions.Clear();
			if (skeleton != null) skeleton.SetToSetupPose();
		}

		public virtual void Initialize (bool overwrite) {
			if (valid && !overwrite)
				return;

			// Clear
			{
				if (meshFilter != null)
					meshFilter.sharedMesh = null;

				meshRenderer = GetComponent<MeshRenderer>();
				if (meshRenderer != null) meshRenderer.sharedMaterial = null;

				currentInstructions.Clear();
				vertices = null;
				colors = null;
				uvs = null;
				sharedMaterials = new Material[0];
				submeshMaterials.Clear();
				submeshes.Clear();
				skeleton = null;

				valid = false;
			}

			if (!skeletonDataAsset) {
				if (logErrors)
					Debug.LogError("Missing SkeletonData asset.", this);

				return;
			}
			SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
			if (skeletonData == null)
				return;
			valid = true;

			meshFilter = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();
			doubleBufferedMesh = new DoubleBuffered<SmartMesh>();
			vertices = new Vector3[0];

			skeleton = new Skeleton(skeletonData);
			if (!string.IsNullOrEmpty(initialSkinName) && initialSkinName != "default")
				skeleton.SetSkin(initialSkinName);

			separatorSlots.Clear();
			for (int i = 0; i < separatorSlotNames.Length; i++)
				separatorSlots.Add(skeleton.FindSlot(separatorSlotNames[i]));

			LateUpdate();

			if (OnRebuild != null)
				OnRebuild(this);
		}

		public virtual void LateUpdate () {
			if (!valid)
				return;

			if (
				(!meshRenderer.enabled)
				#if SPINE_OPTIONAL_RENDEROVERRIDE
				&& this.generateMeshOverride == null
				#endif
			)
				return;
			

			// STEP 1. Determine a SmartMesh.Instruction. Split up instructions into submeshes. ============================================================
			ExposedList<Slot> drawOrder = skeleton.drawOrder;
			var drawOrderItems = drawOrder.Items;
			int drawOrderCount = drawOrder.Count;
			bool renderMeshes = this.renderMeshes;

			// Clear last state of attachments and submeshes
			var workingInstruction = this.currentInstructions;
			var workingAttachments = workingInstruction.attachments;
			workingAttachments.Clear(false);
			workingAttachments.GrowIfNeeded(drawOrderCount);
			workingAttachments.Count = drawOrderCount;
			var workingAttachmentsItems = workingInstruction.attachments.Items;

			#if SPINE_OPTIONAL_FRONTFACING
			var workingFlips = workingInstruction.attachmentFlips;
			workingFlips.Clear(false);
			workingFlips.GrowIfNeeded(drawOrderCount);
			workingFlips.Count = drawOrderCount;
			var workingFlipsItems = workingFlips.Items;
			#endif

			var workingSubmeshInstructions = workingInstruction.submeshInstructions;	// Items array should not be cached. There is dynamic writing to this list.
			workingSubmeshInstructions.Clear(false);

			#if !SPINE_TK2D
			bool isCustomSlotMaterialsPopulated = customSlotMaterials.Count > 0;
			#endif

			bool hasSeparators = separatorSlots.Count > 0;
			int vertexCount = 0;
			int submeshVertexCount = 0;
			int submeshTriangleCount = 0, submeshFirstVertex = 0, submeshStartSlotIndex = 0;
			Material lastMaterial = null;
			for (int i = 0; i < drawOrderCount; i++) {
				Slot slot = drawOrderItems[i];
				Attachment attachment = slot.attachment;
				workingAttachmentsItems[i] = attachment;

				#if SPINE_OPTIONAL_FRONTFACING
				bool flip = frontFacing && (slot.bone.WorldSignX != slot.bone.WorldSignY);
				workingFlipsItems[i] = flip;
				#endif

				object rendererObject = null; // An AtlasRegion in plain Spine-Unity. Spine-TK2D hooks into TK2D's system. eventual source of Material object.
				int attachmentVertexCount, attachmentTriangleCount;
				bool noRender = false;

				var regionAttachment = attachment as RegionAttachment;
				if (regionAttachment != null) {
					rendererObject = regionAttachment.RendererObject;
					attachmentVertexCount = 4;
					attachmentTriangleCount = 6;
				} else {
					if (!renderMeshes) {
						noRender = true;
						attachmentVertexCount = 0;
						attachmentTriangleCount = 0;
						//continue;
					} else {
						var meshAttachment = attachment as MeshAttachment;
						if (meshAttachment != null) {
							rendererObject = meshAttachment.RendererObject;
							attachmentVertexCount = meshAttachment.worldVerticesLength >> 1;
							attachmentTriangleCount = meshAttachment.triangles.Length;
						} else {
							noRender = true;
							attachmentVertexCount = 0;
							attachmentTriangleCount = 0;
							//continue;
						}
					}
				}

				// Create a new SubmeshInstruction when material changes. (or when forced to separate by a submeshSeparator)
				// Slot with a separator/new material will become the starting slot of the next new instruction.
				bool forceSeparate = (hasSeparators && separatorSlots.Contains(slot));
				if (noRender) {
					if (forceSeparate && vertexCount > 0
						#if SPINE_OPTIONAL_RENDEROVERRIDE
						&& this.generateMeshOverride != null
						#endif
					) {
						workingSubmeshInstructions.Add(
							new Spine.Unity.MeshGeneration.SubmeshInstruction {
								skeleton = this.skeleton,
								material = lastMaterial,
								startSlot = submeshStartSlotIndex,
								endSlot = i,
								triangleCount = submeshTriangleCount,
								firstVertexIndex = submeshFirstVertex,
								vertexCount = submeshVertexCount,
								forceSeparate = forceSeparate
							}
						);
						submeshTriangleCount = 0;
						submeshVertexCount = 0;
						submeshFirstVertex = vertexCount;
						submeshStartSlotIndex = i;
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

					if (vertexCount > 0 && (forceSeparate || lastMaterial.GetInstanceID() != material.GetInstanceID())) {
						workingSubmeshInstructions.Add(
							new Spine.Unity.MeshGeneration.SubmeshInstruction {
								skeleton = this.skeleton,
								material = lastMaterial,
								startSlot = submeshStartSlotIndex,
								endSlot = i,
								triangleCount = submeshTriangleCount,
								firstVertexIndex = submeshFirstVertex,
								vertexCount = submeshVertexCount,
								forceSeparate = forceSeparate
							}
						);
						submeshTriangleCount = 0;
						submeshVertexCount = 0;
						submeshFirstVertex = vertexCount;
						submeshStartSlotIndex = i;
					}
					// Update state for the next iteration.
					lastMaterial = material;
					submeshTriangleCount += attachmentTriangleCount;
					vertexCount += attachmentVertexCount;
					submeshVertexCount += attachmentVertexCount;
				}
			}

			if (submeshVertexCount != 0) {
				workingSubmeshInstructions.Add(
					new Spine.Unity.MeshGeneration.SubmeshInstruction {
						skeleton = this.skeleton,
						material = lastMaterial,
						startSlot = submeshStartSlotIndex,
						endSlot = drawOrderCount,
						triangleCount = submeshTriangleCount,
						firstVertexIndex = submeshFirstVertex,
						vertexCount = submeshVertexCount,
						forceSeparate = false
					}
				);
			}

			workingInstruction.vertexCount = vertexCount;
			workingInstruction.immutableTriangles = this.immutableTriangles;
			#if SPINE_OPTIONAL_FRONTFACING
			workingInstruction.frontFacing = this.frontFacing;
			#endif


			// STEP 1.9. Post-process workingInstructions. ============================================================

			#if SPINE_OPTIONAL_MATERIALOVERRIDE
			// Material overrides are done here so they can be applied per submesh instead of per slot
			// but they will still be passed through the GenerateMeshOverride delegate,
			// and will still go through the normal material match check step in STEP 3.
			if (customMaterialOverride.Count > 0) { // isCustomMaterialOverridePopulated 
				var workingSubmeshInstructionsItems = workingSubmeshInstructions.Items;
				for (int i = 0; i < workingSubmeshInstructions.Count; i++) {
					var m = workingSubmeshInstructionsItems[i].material;
					Material mo;
					if (customMaterialOverride.TryGetValue(m, out mo)) {
						workingSubmeshInstructionsItems[i].material = mo;
					}
				}
			}
			#endif
			#if SPINE_OPTIONAL_RENDEROVERRIDE
			if (this.generateMeshOverride != null) {
				this.generateMeshOverride(workingInstruction);
				if (disableRenderingOnOverride) return;
			}
			#endif


			// STEP 2. Update vertex buffer based on verts from the attachments.  ============================================================
			// Uses values that were also stored in workingInstruction.
			#if SPINE_OPTIONAL_NORMALS
			bool vertexCountIncreased = ArraysMeshGenerator.EnsureSize(vertexCount, ref this.vertices, ref this.uvs, ref this.colors);
			if (vertexCountIncreased && calculateNormals) {
				Vector3[] localNormals = this.normals = new Vector3[vertexCount];
				Vector3 normal = new Vector3(0, 0, -1);
				for (int i = 0; i < vertexCount; i++)
					localNormals[i] = normal;
			}
			#else
			ArraysMeshGenerator.EnsureSize(vertexCount, ref this.vertices, ref this.uvs, ref this.colors);
			#endif

			Vector3 meshBoundsMin;
			Vector3 meshBoundsMax;
			if (vertexCount <= 0) {
				meshBoundsMin = new Vector3(0, 0, 0);
				meshBoundsMax = new Vector3(0, 0, 0);
			} else {
				meshBoundsMin.x = int.MaxValue;
				meshBoundsMin.y = int.MaxValue;
				meshBoundsMax.x = int.MinValue;
				meshBoundsMax.y = int.MinValue;

				if (zSpacing > 0f) {
					meshBoundsMin.z = 0f;
					meshBoundsMax.z = zSpacing * (drawOrderCount - 1);
				} else {
					meshBoundsMin.z = zSpacing * (drawOrderCount - 1);
					meshBoundsMax.z = 0f;
				}
			}
			int vertexIndex = 0;
			ArraysMeshGenerator.FillVerts(skeleton, 0, drawOrderCount, this.zSpacing, pmaVertexColors, this.vertices, this.uvs, this.colors, ref vertexIndex, ref tempVertices, ref meshBoundsMin, ref meshBoundsMax, renderMeshes);


			// Step 3. Move the mesh data into a UnityEngine.Mesh ============================================================
			var currentSmartMesh = doubleBufferedMesh.GetNext();	// Double-buffer for performance.
			var currentMesh = currentSmartMesh.mesh;
			currentMesh.vertices = this.vertices;
			currentMesh.colors32 = colors;
			currentMesh.uv = uvs;
			currentMesh.bounds = ArraysMeshGenerator.ToBounds(meshBoundsMin, meshBoundsMax);

			var currentSmartMeshInstructionUsed = currentSmartMesh.instructionUsed;
			#if SPINE_OPTIONAL_NORMALS
			if (calculateNormals && currentSmartMeshInstructionUsed.vertexCount < vertexCount)
				currentMesh.normals = normals;
			#endif

			// Check if the triangles should also be updated.
			// This thorough structure check is cheaper than updating triangles every frame.
			bool mustUpdateMeshStructure = CheckIfMustUpdateMeshStructure(workingInstruction, currentSmartMeshInstructionUsed);
			int submeshCount = workingSubmeshInstructions.Count;
			if (mustUpdateMeshStructure) {
				var thisSubmeshMaterials = this.submeshMaterials;
				thisSubmeshMaterials.Clear(false);

				int oldSubmeshCount = submeshes.Count;

				if (submeshes.Capacity < submeshCount)
					submeshes.Capacity = submeshCount;
				for (int i = oldSubmeshCount; i < submeshCount; i++)
					submeshes.Items[i] = new ArraysMeshGenerator.SubmeshTriangleBuffer(workingSubmeshInstructions.Items[i].triangleCount);
				submeshes.Count = submeshCount;
					
				var mutableTriangles = !workingInstruction.immutableTriangles;
				for (int i = 0, last = submeshCount - 1; i < submeshCount; i++) {
					var submeshInstruction = workingSubmeshInstructions.Items[i];

					if (mutableTriangles || i >= oldSubmeshCount) {
	
						#if !SPINE_OPTIONAL_FRONTFACING
						var currentSubmesh = submeshes.Items[i];
						int instructionTriangleCount = submeshInstruction.triangleCount;
						if (renderMeshes) {
							ArraysMeshGenerator.FillTriangles(ref currentSubmesh.triangles, skeleton, instructionTriangleCount, submeshInstruction.firstVertexIndex, submeshInstruction.startSlot, submeshInstruction.endSlot, (i == last));
							currentSubmesh.triangleCount = instructionTriangleCount;
						} else {
							ArraysMeshGenerator.FillTrianglesQuads(ref currentSubmesh.triangles, ref currentSubmesh.triangleCount, ref currentSubmesh.firstVertex, submeshInstruction.firstVertexIndex, instructionTriangleCount, (i == last));
						}
						#else
						SetSubmesh(i, submeshInstruction, currentInstructions.attachmentFlips, i == last);
						#endif

					}

					thisSubmeshMaterials.Add(submeshInstruction.material);
				}

				currentMesh.subMeshCount = submeshCount;

				for (int i = 0; i < submeshCount; ++i)
					currentMesh.SetTriangles(submeshes.Items[i].triangles, i);
			}

			#if SPINE_OPTIONAL_SOLVETANGENTS
			if (calculateTangents) {
				ArraysMeshGenerator.SolveTangents2DEnsureSize(ref this.tangents, ref this.tempTanBuffer, vertices.Length);
				for (int i = 0; i < submeshCount; i++) {
					var submesh = submeshes.Items[i];
					ArraysMeshGenerator.SolveTangents2DTriangles(this.tempTanBuffer, submesh.triangles, submesh.triangleCount, this.vertices, this.uvs, vertexCount);
				}
				ArraysMeshGenerator.SolveTangents2DBuffer(this.tangents, this.tempTanBuffer, vertexCount);
				currentMesh.tangents = this.tangents;
			}
			#endif
				
			// CheckIfMustUpdateMaterialArray (last pushed materials vs currently parsed materials)
			// Needs to check against the Working Submesh Instructions Materials instead of the cached submeshMaterials.
			{
				var lastPushedMaterials = this.sharedMaterials;
				bool mustUpdateRendererMaterials = mustUpdateMeshStructure ||
					(lastPushedMaterials.Length != submeshCount);

				// Assumption at this point: (lastPushedMaterials.Count == workingSubmeshInstructions.Count == thisSubmeshMaterials.Count == submeshCount)

				// Case: mesh structure or submesh count did not change but materials changed.
				if (!mustUpdateRendererMaterials) {
					var workingSubmeshInstructionsItems = workingSubmeshInstructions.Items;
					for (int i = 0; i < submeshCount; i++) {
						if (lastPushedMaterials[i].GetInstanceID() != workingSubmeshInstructionsItems[i].material.GetInstanceID()) {   // Bounds check is implied by submeshCount above.
							mustUpdateRendererMaterials = true;
							{
								var thisSubmeshMaterials = this.submeshMaterials.Items;
								if (mustUpdateRendererMaterials)
									for (int j = 0; j < submeshCount; j++)
										thisSubmeshMaterials[j] = workingSubmeshInstructionsItems[j].material;
							}
							break;
						}
					}
				}

				if (mustUpdateRendererMaterials) {
					if (submeshMaterials.Count == sharedMaterials.Length)
						submeshMaterials.CopyTo(sharedMaterials);
					else
						sharedMaterials = submeshMaterials.ToArray();

					meshRenderer.sharedMaterials = sharedMaterials;
				}
			}


			// Step 4. The UnityEngine.Mesh is ready. Set it as the MeshFilter's mesh. Store the instructions used for that mesh. ============================================================
			meshFilter.sharedMesh = currentMesh;
			currentSmartMesh.instructionUsed.Set(workingInstruction);

		}

		static bool CheckIfMustUpdateMeshStructure (SmartMesh.Instruction a, SmartMesh.Instruction b) {

			#if UNITY_EDITOR
			if (!Application.isPlaying)
				return true;
			#endif

			if (a.vertexCount != b.vertexCount)
				return true;

			if (a.immutableTriangles != b.immutableTriangles)
				return true;

			int attachmentCountB = b.attachments.Count;
			if (a.attachments.Count != attachmentCountB) // Bounds check for the looped storedAttachments count below.
				return true;

			var attachmentsA = a.attachments.Items;
			var attachmentsB = b.attachments.Items;		
			for (int i = 0; i < attachmentCountB; i++) {
				if (attachmentsA[i] != attachmentsB[i])
					return true;
			}

			#if SPINE_OPTIONAL_FRONTFACING
			if (a.frontFacing != b.frontFacing) { 	// if settings changed
				return true;
			} else if (a.frontFacing) { 			// if settings matched, only need to check one.
				var flipsA = a.attachmentFlips.Items;
				var flipsB = b.attachmentFlips.Items;
				for (int i = 0; i < attachmentCountB; i++) {
					if (flipsA[i] != flipsB[i])
						return true;
				}
			}
			#endif

			// Submesh count changed
			int submeshCountA = a.submeshInstructions.Count;
			int submeshCountB = b.submeshInstructions.Count;
			if (submeshCountA != submeshCountB)
				return true;

			// Submesh Instruction mismatch
			var submeshInstructionsItemsA = a.submeshInstructions.Items;
			var submeshInstructionsItemsB = b.submeshInstructions.Items;
			for (int i = 0; i < submeshCountB; i++) {
				var submeshA = submeshInstructionsItemsA[i];
				var submeshB = submeshInstructionsItemsB[i];

				if (!(
					submeshA.vertexCount == submeshB.vertexCount &&
					submeshA.startSlot == submeshB.startSlot &&
					submeshA.endSlot == submeshB.endSlot &&
					submeshA.triangleCount == submeshB.triangleCount &&
					submeshA.firstVertexIndex == submeshB.firstVertexIndex
				))
					return true;			
			}

			return false;
		}

		#if SPINE_OPTIONAL_FRONTFACING
		void SetSubmesh (int submeshIndex, Spine.Unity.MeshGeneration.SubmeshInstruction submeshInstructions, ExposedList<bool> flipStates, bool isLastSubmesh) {
			var currentSubmesh = submeshes.Items[submeshIndex];
			int[] triangles = currentSubmesh.triangles;

			int triangleCount = submeshInstructions.triangleCount;
			int firstVertex = submeshInstructions.firstVertexIndex;

			int trianglesCapacity = triangles.Length;
			if (isLastSubmesh && trianglesCapacity > triangleCount) {
				// Last submesh may have more triangles than required, so zero triangles to the end.
				for (int i = triangleCount; i < trianglesCapacity; i++)
					triangles[i] = 0;

				currentSubmesh.triangleCount = triangleCount;

			} else if (trianglesCapacity != triangleCount) {
				// Reallocate triangles when not the exact size needed.
				currentSubmesh.triangles = triangles = new int[triangleCount];
				currentSubmesh.triangleCount = 0;
			}
				
			if (!this.renderMeshes && !this.frontFacing) {
				// Use stored triangles if possible.
				if (currentSubmesh.firstVertex != firstVertex || currentSubmesh.triangleCount < triangleCount) { //|| currentSubmesh.triangleCount == 0
					currentSubmesh.triangleCount = triangleCount;
					currentSubmesh.firstVertex = firstVertex;

					for (int i = 0; i < triangleCount; i += 6, firstVertex += 4) {
						triangles[i] = firstVertex;
						triangles[i + 1] = firstVertex + 2;
						triangles[i + 2] = firstVertex + 1;
						triangles[i + 3] = firstVertex + 2;
						triangles[i + 4] = firstVertex + 3;
						triangles[i + 5] = firstVertex + 1;
					}
				}
				return;
			}
				
			var flipStatesItems = flipStates.Items;

			// Iterate through all slots and store their triangles. 
			var drawOrderItems = skeleton.DrawOrder.Items;
			int triangleIndex = 0; // Modified by loop
			for (int i = submeshInstructions.startSlot, n = submeshInstructions.endSlot; i < n; i++) {			
				Attachment attachment = drawOrderItems[i].attachment;
				bool flip = frontFacing && flipStatesItems[i];

				// Add RegionAttachment triangles
				if (attachment is RegionAttachment) {
					if (!flip) {
						triangles[triangleIndex] = firstVertex;
						triangles[triangleIndex + 1] = firstVertex + 2;
						triangles[triangleIndex + 2] = firstVertex + 1;
						triangles[triangleIndex + 3] = firstVertex + 2;
						triangles[triangleIndex + 4] = firstVertex + 3;
						triangles[triangleIndex + 5] = firstVertex + 1;
					} else {
						triangles[triangleIndex] = firstVertex + 1;
						triangles[triangleIndex + 1] = firstVertex + 2;
						triangles[triangleIndex + 2] = firstVertex;
						triangles[triangleIndex + 3] = firstVertex + 1;
						triangles[triangleIndex + 4] = firstVertex + 3;
						triangles[triangleIndex + 5] = firstVertex + 2;
					}

					triangleIndex += 6;
					firstVertex += 4;
					continue;
				}

				// Add (Weighted)MeshAttachment triangles
				int[] attachmentTriangles;
				int attachmentVertexCount;
				var meshAttachment = attachment as MeshAttachment;
				if (meshAttachment != null) {
					attachmentVertexCount = meshAttachment.worldVerticesLength >> 1; // length/2
					attachmentTriangles = meshAttachment.triangles;
				} else {
					continue;
				}

				if (flip) {
					for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii += 3, triangleIndex += 3) {
						triangles[triangleIndex + 2] = firstVertex + attachmentTriangles[ii];
						triangles[triangleIndex + 1] = firstVertex + attachmentTriangles[ii + 1];
						triangles[triangleIndex] = firstVertex + attachmentTriangles[ii + 2];
					}
				} else {
					for (int ii = 0, nn = attachmentTriangles.Length; ii < nn; ii++, triangleIndex++) {
						triangles[triangleIndex] = firstVertex + attachmentTriangles[ii];
					}
				}

				firstVertex += attachmentVertexCount;
			}
		}
		#endif

		///<summary>This is a Mesh that also stores the instructions SkeletonRenderer generated for it.</summary>
		public class SmartMesh : System.IDisposable {
			public Mesh mesh = Spine.Unity.SpineMesh.NewMesh();
			public SmartMesh.Instruction instructionUsed = new SmartMesh.Instruction();		

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

			public class Instruction {
				public bool immutableTriangles;
				public int vertexCount = -1;
				public readonly ExposedList<Attachment> attachments = new ExposedList<Attachment>();
				public readonly ExposedList<Spine.Unity.MeshGeneration.SubmeshInstruction> submeshInstructions = new ExposedList<Spine.Unity.MeshGeneration.SubmeshInstruction>();

				#if SPINE_OPTIONAL_FRONTFACING
				public bool frontFacing;
				public readonly ExposedList<bool> attachmentFlips = new ExposedList<bool>();
				#endif

				public void Clear () {
					this.attachments.Clear(false);
					this.submeshInstructions.Clear(false);

					#if SPINE_OPTIONAL_FRONTFACING
					this.attachmentFlips.Clear(false);
					#endif
				}

				public void Set (Instruction other) {
					this.immutableTriangles = other.immutableTriangles;
					this.vertexCount = other.vertexCount;

					this.attachments.Clear(false);
					this.attachments.GrowIfNeeded(other.attachments.Capacity);
					this.attachments.Count = other.attachments.Count;
					other.attachments.CopyTo(this.attachments.Items);

					#if SPINE_OPTIONAL_FRONTFACING
					this.frontFacing = other.frontFacing;
					this.attachmentFlips.Clear(false);
					this.attachmentFlips.GrowIfNeeded(other.attachmentFlips.Capacity);
					this.attachmentFlips.Count = other.attachmentFlips.Count;
					if (this.frontFacing)
						other.attachmentFlips.CopyTo(this.attachmentFlips.Items);
					#endif

					this.submeshInstructions.Clear(false);
					this.submeshInstructions.GrowIfNeeded(other.submeshInstructions.Capacity);
					this.submeshInstructions.Count = other.submeshInstructions.Count;
					other.submeshInstructions.CopyTo(this.submeshInstructions.Items);
				}
			}
		}
	}
}
