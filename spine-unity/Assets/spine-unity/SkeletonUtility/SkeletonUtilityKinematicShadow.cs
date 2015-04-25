/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonUtilityKinematicShadow : MonoBehaviour {
	public bool hideShadow = true;
	public Transform parent;
	Dictionary<Transform, Transform> shadowTable;
	GameObject shadowRoot;

	void Start () {
		shadowRoot = (GameObject)Instantiate(gameObject);
		if (hideShadow)
			shadowRoot.hideFlags = HideFlags.HideInHierarchy;

		if(parent == null)
			shadowRoot.transform.parent = transform.root;
		else
			shadowRoot.transform.parent = parent;

		shadowTable = new Dictionary<Transform, Transform>();

		Destroy(shadowRoot.GetComponent<SkeletonUtilityKinematicShadow>());

		shadowRoot.transform.position = transform.position;
		shadowRoot.transform.rotation = transform.rotation;

		Vector3 scaleRef = transform.TransformPoint(Vector3.right);
		float scale = Vector3.Distance(transform.position, scaleRef);
		shadowRoot.transform.localScale = Vector3.one;

		var shadowJoints = shadowRoot.GetComponentsInChildren<Joint>();
		foreach (Joint j in shadowJoints) {
			j.connectedAnchor *= scale;
		}

		var joints = GetComponentsInChildren<Joint>();
		foreach (var j in joints)
			Destroy(j);

		var rbs = GetComponentsInChildren<Rigidbody>();
		foreach (var rb in rbs)
			Destroy(rb);

		var colliders = GetComponentsInChildren<Collider>();
		foreach (var c in colliders)
			Destroy(c);


		//match by bone name
		var shadowBones = shadowRoot.GetComponentsInChildren<SkeletonUtilityBone>();
		var bones = GetComponentsInChildren<SkeletonUtilityBone>();

		//build bone lookup
		foreach (var b in bones) {
			if (b.gameObject == gameObject)
				continue;

			foreach (var sb in shadowBones) {
				if (sb.GetComponent<Rigidbody>() == null)
					continue;

				if (sb.boneName == b.boneName) {
					shadowTable.Add(sb.transform, b.transform);
					break;
				}
			}
		}

		foreach (var b in shadowBones)
			Destroy(b);
	}

	void FixedUpdate () {
		shadowRoot.GetComponent<Rigidbody>().MovePosition(transform.position);
		shadowRoot.GetComponent<Rigidbody>().MoveRotation(transform.rotation);

		foreach (var pair in shadowTable) {
			pair.Value.localPosition = pair.Key.localPosition;
			pair.Value.localRotation = pair.Key.localRotation;
		}
	}
}
