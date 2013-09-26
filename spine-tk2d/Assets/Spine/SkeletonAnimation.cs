/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
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

/** Extends SkeletonComponent to apply an animation. */
[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkeletonAnimation : SkeletonComponent {
	public bool useAnimationName;
	public String animationName;
	public bool loop;
	public Spine.AnimationState state;

	override public void Initialize () {
		base.Initialize(); // Call overridden method to initialize the skeleton.
		
		state = new Spine.AnimationState(skeletonDataAsset.GetAnimationStateData());
	}

	override public void UpdateSkeleton () {
		if (useAnimationName) {
			// Keep AnimationState in sync with animationName and loop fields.
			TrackEntry entry = state.GetCurrent(0);
			if (animationName == null || animationName.Length == 0) {
				if (entry != null && entry.Animation != null)
					state.Clear(0);
			} else if (entry == null || entry.Animation == null || animationName != entry.Animation.Name) {
				Spine.Animation animation = skeleton.Data.FindAnimation(animationName);
				if (animation != null)
					state.SetAnimation(0, animation, loop);
			} else if (entry != null)
				entry.Loop = loop;
		}
		
		// Apply the animation.
		state.Update(Time.deltaTime * timeScale);
		state.Apply(skeleton);

		// Call overridden method to call skeleton Update and UpdateWorldTransform.
		base.UpdateSkeleton();
	}
}
