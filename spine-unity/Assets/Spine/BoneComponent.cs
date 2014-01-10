/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

/*
TODO:Redo Documentation And Formatting for BoneComponent!  
	Uses a Spine.Bone and manages a Gameobject to match the position,scale and rotation of said Spine.Bone.
	-You may or may not parent the bone component to the parentskeleton via the transform class and it will still work with or without!
	-scaling,rotation, and positioning works more effectively if you parent from the skeleton’s Gameobject because unity can handle the parental arithmetic.
	-Disable Component to stop this effect 
	-scaling does not affect Colliders(Unity Limitation)
	-Updates on LateUpdate, After SkeletonComponent!
	-Best effect if Z Spacing on the skeleton is (Z < 0.1 || Z == 0)
 */
[ExecuteInEditMode]
public class BoneComponent : MonoBehaviour
{

		public Spine.Bone attachedBone = null;
		public string boneName = "";
		public SkeletonComponent parentSkeleton = null;
		private Vector3 CurrentRotation;//Cache Rotation

		/// <summary>
		/// Connects the bone by name.
		/// </summary>
		/// <returns><c>true</c>, if bone was connected, <c>false</c> otherwise.</returns>
		/// <param name="BoneName">Bone name.</param>
		/// <param name="Skeleton">Skeleton.</param>
		/// <param name="ParentThisObject">If set to <c>true</c>, parents itself to skeleton.</param>
		public bool ConnectBone (string BoneName, SkeletonComponent Skeleton, bool ParentThisObject)
		{
				if (boneName.Length != 0 && Skeleton != null)//find it and attach it!
						foreach (Spine.Bone Bone in Skeleton.skeleton.bones)//Looking for bone
								if (Bone.ToString () == BoneName) {//Found
										attachedBone = Bone;
										boneName = BoneName;
										parentSkeleton = Skeleton;
										if (ParentThisObject)
												transform.parent = Skeleton.transform;
										return false;
								}
				return true;
		}
		
		/// <summary>
		/// Connects the bone.
		/// </summary>
		/// <returns><c>true</c>, if bone was connected, <c>false</c> otherwise.</returns>
		/// <param name="Bone">Bone.</param>
		/// <param name="Skeleton">Skeleton.</param>
		/// <param name="ParentThisObject">If set to <c>true</c>, parents itself to skeleton.</param>
		public bool ConnectBone (Spine.Bone Bone, SkeletonComponent Skeleton, bool ParentThisObject)
		{
				if (Skeleton != null && Bone != null) {
						foreach (Spine.Bone bone in Skeleton.skeleton.bones)//Checking for existence
								if (bone == Bone) {//Found
										attachedBone = Bone;
										boneName = Bone.ToString ();
										parentSkeleton = Skeleton;
										if (ParentThisObject)
												transform.parent = Skeleton.transform;
										return false;
								}
				}
				return true;
		}

		public void LateUpdate ()//So you don’t have to change the script execution order
		{
				if (parentSkeleton != null && attachedBone != null && attachedBone.ToString () == boneName) {
						if (transform.parent == parentSkeleton.transform) {
								transform.localPosition = new Vector3 ((attachedBone.worldX * attachedBone.worldScaleX), (attachedBone.worldY * attachedBone.worldScaleY), transform.localPosition.z);
								CurrentRotation = transform.localRotation.eulerAngles;//To avoid the accident of changing values that don’t need to be changed!…Also this is a Cache!
								transform.localRotation = Quaternion.Euler (CurrentRotation.x, CurrentRotation.y, attachedBone.worldRotation);
						} else {
								transform.position = parentSkeleton.transform.TransformPoint (new Vector3 ((attachedBone.worldX * attachedBone.worldScaleX), (attachedBone.worldY * attachedBone.worldScaleY), 0f));//Has same effect if not parented
								CurrentRotation = parentSkeleton.transform.rotation.eulerAngles;
								transform.rotation = Quaternion.Euler (CurrentRotation.x, CurrentRotation.y, parentSkeleton.transform.rotation.eulerAngles.z + attachedBone.worldRotation);//Same as position, Same effect
								transform.localScale = parentSkeleton.transform.localScale;//Because its not parented we need to set scale, and even then its not a very good job(Parent it to fix it)
						}
				} else
						ConnectBone (boneName, parentSkeleton, false);
		}
}
