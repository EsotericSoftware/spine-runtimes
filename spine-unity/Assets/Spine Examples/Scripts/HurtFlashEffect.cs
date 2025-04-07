/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtFlashEffect : MonoBehaviour {

	const int DefaultFlashCount = 3;

	public int flashCount = DefaultFlashCount;
	public Color flashColor = Color.white;
	[Range(1f / 120f, 1f / 15f)]
	public float interval = 1f / 60f;
	public string fillPhaseProperty = "_FillPhase";
	public string fillColorProperty = "_FillColor";

	MaterialPropertyBlock mpb;
	MeshRenderer meshRenderer;

	public void Flash () {
		if (mpb == null) mpb = new MaterialPropertyBlock();
		if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.GetPropertyBlock(mpb);

		StartCoroutine(FlashRoutine());
	}

	IEnumerator FlashRoutine () {
		if (flashCount < 0) flashCount = DefaultFlashCount;
		int fillPhase = Shader.PropertyToID(fillPhaseProperty);
		int fillColor = Shader.PropertyToID(fillColorProperty);

		WaitForSeconds wait = new WaitForSeconds(interval);

		for (int i = 0; i < flashCount; i++) {
			mpb.SetColor(fillColor, flashColor);
			mpb.SetFloat(fillPhase, 1f);
			meshRenderer.SetPropertyBlock(mpb);
			yield return wait;

			mpb.SetFloat(fillPhase, 0f);
			meshRenderer.SetPropertyBlock(mpb);
			yield return wait;
		}

		yield return null;
	}

}
