/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

/*****************************************************************************
 * Skeleton Utility created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

/// <summary>Sets a GameObject's transform to match a bone on a Spine skeleton.</summary>
[ExecuteInEditMode]
[AddComponentMenu("Spine/SkeletonUtilityBone")]
public class SkeletonUtilityBone : MonoBehaviour {

	public enum Mode { Follow, Override }

	[System.NonSerialized]
	public bool valid;

	[System.NonSerialized]
	public SkeletonUtility skeletonUtility;

	[System.NonSerialized]
	public Bone bone;

	public Mode mode;
	
	public bool zPosition = true;
	public bool position;
	public bool rotation;
	public bool scale;

	[Range(0f,1f)]
	public float overrideAlpha = 1;

	/// <summary>If a bone isn't set, boneName is used to find the bone.</summary>
	public String boneName;

	public Transform parentReference;

	[HideInInspector]
	public bool transformLerpComplete;

	protected Transform cachedTransform;
	protected Transform skeletonTransform;

	public bool NonUniformScaleWarning{
		get{
			return nonUniformScaleWarning;
		}
	}
	private bool nonUniformScaleWarning;

	public void Reset () {
		bone = null;
		cachedTransform = transform;
		valid = skeletonUtility != null && skeletonUtility.skeletonRenderer != null && skeletonUtility.skeletonRenderer.valid;
		if (!valid) return;
		skeletonTransform = skeletonUtility.transform;

		skeletonUtility.OnReset -= HandleOnReset;
		skeletonUtility.OnReset += HandleOnReset;

		DoUpdate();
	}

	void OnEnable(){
		skeletonUtility = GetComponentInParent<SkeletonUtility>();
		if(skeletonUtility == null)
			return;

		skeletonUtility.RegisterBone(this);

		skeletonUtility.OnReset += HandleOnReset;
	}

	void HandleOnReset ()
	{
		Reset ();
	}

	void OnDisable(){
		if(skeletonUtility != null){
			skeletonUtility.OnReset -= HandleOnReset;

			skeletonUtility.UnregisterBone(this);
		}
	}

	public void DoUpdate () {

		if (!valid) {
			Reset();
			return;
		}

		Spine.Skeleton skeleton = skeletonUtility.skeletonRenderer.skeleton;

		if (bone == null) {
			if (boneName == null || boneName.Length == 0) return;
			bone = skeleton.FindBone(boneName);
			if (bone == null) {
				Debug.LogError("Bone not found: " + boneName, this);
				return;
			}
		}


		float flipRotation = (skeleton.flipX ^ skeleton.flipY) ? -1f : 1f;


		if(mode == Mode.Follow){
			if(position){
				cachedTransform.localPosition = new Vector3(bone.x, bone.y, 0);
			}
			
			if(rotation){

				if(bone.Data.InheritRotation){
					cachedTransform.localRotation = Quaternion.Euler(0,0,bone.rotationIK);
				}
				else{
					Vector3 euler = skeletonTransform.rotation.eulerAngles;
					cachedTransform.rotation = Quaternion.Euler(euler.x, euler.y, skeletonTransform.rotation.eulerAngles.z + (bone.worldRotation * flipRotation) );
				}

			}

			if(scale){
				cachedTransform.localScale = new Vector3(bone.scaleX, bone.scaleY, 1);

				nonUniformScaleWarning = (bone.scaleX != bone.scaleY);
			}

		}
		else if(mode == Mode.Override){
			if(transformLerpComplete)
				return;

			if(parentReference == null){
				if(position){
					bone.x = Mathf.Lerp(bone.x, cachedTransform.localPosition.x, overrideAlpha);
					bone.y = Mathf.Lerp(bone.y, cachedTransform.localPosition.y, overrideAlpha);
				}

				if(rotation){
					bone.rotation = Mathf.LerpAngle(bone.Rotation, cachedTransform.localRotation.eulerAngles.z, overrideAlpha);
				}

				if(scale){
					bone.scaleX = Mathf.Lerp(bone.scaleX, cachedTransform.localScale.x, overrideAlpha);
					bone.scaleY = Mathf.Lerp(bone.scaleY, cachedTransform.localScale.y, overrideAlpha);

					nonUniformScaleWarning = (bone.scaleX != bone.scaleY);
				}
			}
			else{
				if(position){
					Vector3 pos = parentReference.InverseTransformPoint(cachedTransform.position);
					bone.x = Mathf.Lerp(bone.x, pos.x, overrideAlpha);
					bone.y = Mathf.Lerp(bone.y, pos.y, overrideAlpha);
				}
				
				if(rotation){
					bone.rotation = Mathf.LerpAngle(bone.Rotation, Quaternion.LookRotation( Vector3.forward, parentReference.InverseTransformDirection( cachedTransform.up ) ).eulerAngles.z, overrideAlpha);
				}

				//TODO: Something about this
				if(scale){
					bone.scaleX = Mathf.Lerp(bone.scaleX, cachedTransform.localScale.x, overrideAlpha);
					bone.scaleY = Mathf.Lerp(bone.scaleY, cachedTransform.localScale.y, overrideAlpha);
					
					nonUniformScaleWarning = (bone.scaleX != bone.scaleY);
				}
			}

			transformLerpComplete = true;
		}		
	}

	void OnDrawGizmos(){
		if(NonUniformScaleWarning){
			Gizmos.DrawIcon(transform.position + new Vector3(0,0.128f,0), "icon-warning");
		}
	}
}