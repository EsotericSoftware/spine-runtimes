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
using Spine;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkeletonAnimation))]
public class SkeletonAnimationInspector : SkeletonRendererInspector {
	protected SerializedProperty animationName, loop, timeScale;

	// Inspector playback variables
	protected bool doPlayback;
	protected float lastUpdateTime;

	protected override void OnEnable () {
		base.OnEnable();
		animationName = serializedObject.FindProperty("_animationName");
		loop = serializedObject.FindProperty("loop");
		timeScale = serializedObject.FindProperty("timeScale");
		doPlayback = false;
	}

	void OnDisable()
	{
		if (doPlayback)
		{
			TogglePlayback(false);
		}
	}

	protected override void gui () {
		base.gui();

		SkeletonAnimation component = (SkeletonAnimation)target;
		if (!component.valid) return;

		// Animation name.
		{
			String[] animations = new String[component.skeleton.Data.Animations.Count + 1];
			animations[0] = "<None>";
			int animationIndex = 0;
			for (int i = 0; i < animations.Length - 1; i++) {
				String name = component.skeleton.Data.Animations[i].Name;
				animations[i + 1] = name;
				if (name == animationName.stringValue)
					animationIndex = i + 1;
			}
		
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Animation", GUILayout.Width(EditorGUIUtility.labelWidth));
			animationIndex = EditorGUILayout.Popup(animationIndex, animations);
			EditorGUILayout.EndHorizontal();

			String selectedAnimationName = animationIndex == 0 ? null : animations[animationIndex];
			component.AnimationName = selectedAnimationName;
			animationName.stringValue = selectedAnimationName;
		}

		EditorGUILayout.PropertyField(loop);
		EditorGUILayout.PropertyField(timeScale);
		component.timeScale = Math.Max(component.timeScale, 0);

		if (component.state != null)
		{
			string playButton = (doPlayback) ? "\u2225" : "\u25BA";
			if (GUILayout.Button(playButton))
			{
				TogglePlayback();
			}
		}
	}

	protected void TogglePlayback(bool playback)
	{
		doPlayback = playback;
		if (doPlayback)
		{
			lastUpdateTime = Time.realtimeSinceStartup;
			EditorApplication.update += PlaybackUpdate;
		}
		else
		{
			EditorApplication.update -= PlaybackUpdate;
		}
	}

	protected void TogglePlayback()
	{
		TogglePlayback(!doPlayback);
	}

	protected void PlaybackUpdate()
	{
		if (Application.isPlaying)
			TogglePlayback(false);

		SkeletonAnimation component = (SkeletonAnimation)target;
		if (!component.valid)
		{
			TogglePlayback(false);
			return;
		}
		
		float deltaTime = Time.realtimeSinceStartup - lastUpdateTime;
		deltaTime *= component.timeScale;
		component.Update(deltaTime);
		EditorUtility.SetDirty(target);
		lastUpdateTime = Time.realtimeSinceStartup;
	}
}
