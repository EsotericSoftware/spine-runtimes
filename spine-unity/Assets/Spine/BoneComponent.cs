/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

/// <summary>Sets a GameObject's transform to match a bone on a Spine skeleton.</summary>
[ExecuteInEditMode]
[AddComponentMenu("Spine/BoneComponent")]
public class BoneComponent : MonoBehaviour {
	public SkeletonComponent skeletonComponent;
	public Bone bone;

	/// <summary>If a bone isn't set, boneName is used to find the bone.</summary>
	public String boneName;

	protected Transform cachedTransform;
	protected Transform skeletonComponentTransform;

	void Awake () {
		cachedTransform = transform;

		if(skeletonComponent == null) return;
		skeletonComponentTransform = skeletonComponent.transform;
	}

	public void LateUpdate () {
		if (skeletonComponent == null) return;
		if (bone == null) {
			if (boneName == null) return;
			bone = skeletonComponent.skeleton.FindBone(boneName);
			if (bone == null) {
				Debug.Log("Bone not found: " + boneName, this);
				return;
			}
		}

		if (cachedTransform.parent == skeletonComponentTransform) {
			cachedTransform.localPosition = new Vector3(bone.worldX, bone.worldY, cachedTransform.localPosition.z);
			Vector3 rotation = cachedTransform.localRotation.eulerAngles;
			cachedTransform.localRotation = Quaternion.Euler(rotation.x, rotation.y, bone.worldRotation);
		} else {
			cachedTransform.position = skeletonComponentTransform.TransformPoint(new Vector3(bone.worldX, bone.worldY, cachedTransform.position.z));
			Vector3 rotation = skeletonComponentTransform.rotation.eulerAngles;
			cachedTransform.rotation = Quaternion.Euler(rotation.x, rotation.y, 
			                                            skeletonComponentTransform.rotation.eulerAngles.z + bone.worldRotation);
		}

	}

}
