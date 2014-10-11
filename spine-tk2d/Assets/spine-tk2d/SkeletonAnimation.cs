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

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Spine;

[ExecuteInEditMode]
[AddComponentMenu("Spine/SkeletonAnimation")]
public class SkeletonAnimation : SkeletonRenderer {
	public float timeScale = 1;
	public bool loop;
	public Spine.AnimationState state;

	public delegate void UpdateBonesDelegate (SkeletonAnimation skeleton);

	public UpdateBonesDelegate UpdateLocal;
	public UpdateBonesDelegate UpdateWorld;
	public UpdateBonesDelegate UpdateComplete;
	[SerializeField]
	private String
		_animationName;

	public String AnimationName {
		get {
			TrackEntry entry = state.GetCurrent(0);
			return entry == null ? null : entry.Animation.Name;
		}
		set {
			if (_animationName == value)
				return;
			_animationName = value;
			if (value == null || value.Length == 0)
				state.ClearTrack(0);
			else
				state.SetAnimation(0, value, loop);
		}
	}

	public override void Reset () {
		base.Reset();
		if (!valid)
			return;

		state = new Spine.AnimationState(skeletonDataAsset.GetAnimationStateData());
		if (_animationName != null && _animationName.Length > 0) {
			state.SetAnimation(0, _animationName, loop);
			Update(0);
		}
	}

	public virtual void Update () {
		Update(Time.deltaTime);
	}

	public virtual void Update (float deltaTime) {
		if (!valid)
			return;

		deltaTime *= timeScale;
		skeleton.Update(deltaTime);
		state.Update(deltaTime);
		state.Apply(skeleton);

		if (UpdateLocal != null) 
			UpdateLocal(this);

		skeleton.UpdateWorldTransform();

		if (UpdateWorld != null) { 
			UpdateWorld(this);
			skeleton.UpdateWorldTransform();
		}

		if (UpdateComplete != null) { 
			UpdateComplete(this);
		}
	}
}
