/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonRootMotionBase))]
	[CanEditMultipleObjects]
	public class SkeletonRootMotionBaseInspector : UnityEditor.Editor {
		protected SerializedProperty rootMotionBoneName;
		protected SerializedProperty transformPositionX;
		protected SerializedProperty transformPositionY;
		protected SerializedProperty rootMotionScaleX;
		protected SerializedProperty rootMotionScaleY;
		protected SerializedProperty rigidBody2D;
		protected SerializedProperty rigidBody;

		protected GUIContent rootMotionBoneNameLabel;
		protected GUIContent transformPositionXLabel;
		protected GUIContent transformPositionYLabel;
		protected GUIContent rootMotionScaleXLabel;
		protected GUIContent rootMotionScaleYLabel;
		protected GUIContent rigidBody2DLabel;
		protected GUIContent rigidBodyLabel;

		protected virtual void OnEnable () {

			rootMotionBoneName = serializedObject.FindProperty("rootMotionBoneName");
			transformPositionX = serializedObject.FindProperty("transformPositionX");
			transformPositionY = serializedObject.FindProperty("transformPositionY");
			rootMotionScaleX = serializedObject.FindProperty("rootMotionScaleX");
			rootMotionScaleY = serializedObject.FindProperty("rootMotionScaleY");
			rigidBody2D = serializedObject.FindProperty("rigidBody2D");
			rigidBody = serializedObject.FindProperty("rigidBody");

			rootMotionBoneNameLabel = new UnityEngine.GUIContent("Root Motion Bone", "The bone to take the motion from.");
			transformPositionXLabel = new UnityEngine.GUIContent("X", "Root transform position (X)");
			transformPositionYLabel = new UnityEngine.GUIContent("Y", "Use the Y-movement of the bone.");
			rootMotionScaleXLabel = new UnityEngine.GUIContent("Root Motion Scale (X)", "Scale applied to the horizontal root motion delta. Can be used for delta compensation to e.g. stretch a jump to the desired distance.");
			rootMotionScaleYLabel = new UnityEngine.GUIContent("Root Motion Scale (Y)", "Scale applied to the vertical root motion delta. Can be used for delta compensation to e.g. stretch a jump to the desired distance.");
			rigidBody2DLabel = new UnityEngine.GUIContent("Rigidbody2D",
				"Optional Rigidbody2D: Assign a Rigidbody2D here if you want " +
				" to apply the root motion to the rigidbody instead of the Transform." +
				"\n\n" +
				"Note that animation and physics updates are not always in sync." +
				"Some jitter may result at certain framerates.");
			rigidBodyLabel = new UnityEngine.GUIContent("Rigidbody",
				"Optional Rigidbody: Assign a Rigidbody here if you want " +
				" to apply the root motion to the rigidbody instead of the Transform." +
				"\n\n" +
				"Note that animation and physics updates are not always in sync." +
				"Some jitter may result at certain framerates.");
		}

		public override void OnInspectorGUI () {
			MainPropertyFields();
			OptionalPropertyFields();
			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void MainPropertyFields () {
			EditorGUILayout.PropertyField(rootMotionBoneName, rootMotionBoneNameLabel);
			EditorGUILayout.PropertyField(transformPositionX, transformPositionXLabel);
			EditorGUILayout.PropertyField(transformPositionY, transformPositionYLabel);

			EditorGUILayout.PropertyField(rootMotionScaleX, rootMotionScaleXLabel);
			EditorGUILayout.PropertyField(rootMotionScaleY, rootMotionScaleYLabel);
		}

		protected virtual void OptionalPropertyFields () {
			//EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(rigidBody2D, rigidBody2DLabel);
			EditorGUILayout.PropertyField(rigidBody, rigidBodyLabel);
		}
	}
}
