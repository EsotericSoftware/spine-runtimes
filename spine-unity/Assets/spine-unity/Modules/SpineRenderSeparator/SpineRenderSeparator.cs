using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

[ExecuteInEditMode]
public class SpineRenderSeparator : MonoBehaviour {
	[SerializeField]
	protected SkeletonRenderer skeletonRenderer;
	public SkeletonRenderer SkeletonRenderer {
		get { return skeletonRenderer; }
		set {
			if (skeletonRenderer != null)
				skeletonRenderer.GenerateMeshOverride -= SeparatelyRenderSubmeshes;
			skeletonRenderer = value;
		}
	}

	MeshRenderer masterMeshRenderer;

	public List<Spine.Unity.SubmeshRenderer> submeshRenderers = new List<SubmeshRenderer>();
	public bool propagateMaterialPropertyBlock = false;

	void OnEnable () {
		if (skeletonRenderer == null) return;
		if (block == null) block = new MaterialPropertyBlock();	
		masterMeshRenderer = skeletonRenderer.GetComponent<MeshRenderer>();
		masterMeshRenderer.enabled = false;
		skeletonRenderer.GenerateMeshOverride -= SeparatelyRenderSubmeshes;
		skeletonRenderer.GenerateMeshOverride += SeparatelyRenderSubmeshes;
	}

	void OnDisable () {
		if (skeletonRenderer == null) return;

		masterMeshRenderer.enabled = true;
		skeletonRenderer.GenerateMeshOverride -= SeparatelyRenderSubmeshes;

		foreach (var s in submeshRenderers)
			s.ClearMesh();		
	}

	MaterialPropertyBlock block;

	void SeparatelyRenderSubmeshes (SkeletonRenderer.SmartMesh.Instruction instruction) {
		var submeshInstructions = instruction.submeshInstructions;
		var submeshInstructionsItems = submeshInstructions.Items;
		for (int i = 0; i < instruction.submeshInstructions.Count; i++) {
			if (i >= submeshRenderers.Count) return;
			var submeshRenderer = submeshRenderers[i];
			submeshRenderer.RenderSubmesh(submeshInstructionsItems[i]);

			if (propagateMaterialPropertyBlock) {
				masterMeshRenderer.GetPropertyBlock(block);
				submeshRenderer.SetPropertyBlock(block);
			}

		}
	}



}
