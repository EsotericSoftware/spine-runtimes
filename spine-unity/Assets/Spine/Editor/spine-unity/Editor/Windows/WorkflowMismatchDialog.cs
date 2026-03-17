/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#if UNITY_2019_1_OR_NEWER
#define HAS_MODAL_UTILITY
#endif

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

#if !HAS_MODAL_UTILITY
	public class WorkflowMismatchDialog {
		public enum DialogResult {
			None = -1,
			Switch = 0,
			ReexportInstructions = 1,
			Ignore = 2
		}

		const string REEXPORT_INSTRUCTIONS_URL = "https://esotericsoftware.com/spine-unity-assets#Correct-Texture-Packer-export";

		public static DialogResult ShowDialog (string atlasName, bool isLinearPMAMismatch, bool atlasIsPMA) {
			int result;
			if (isLinearPMAMismatch) {
				result = EditorUtility.DisplayDialogComplex("Premultiply Alpha Atlas not supported",
					string.Format("Atlas '{0}':\n" +
					"Atlas was exported with 'Premultiply alpha' (PMA) enabled, but " +
					"the Unity project is set to Linear color space, which does not support PMA.\n\n" +
					"We can switch your Unity Project to Gamma color space to fix this.\n\n" +
					"Alternatively you can re-export your atlas from Spine with straight alpha instead.", atlasName),
					"Switch to Gamma", "Export Instructions", "Don't show again");
				if (result == 0) { // Switch to Gamma
					PlayerSettings.colorSpace = ColorSpace.Gamma;
					Debug.Log("Switched Unity project to Gamma color space to support PMA atlas textures. To change it back go to 'Project Settings - Player - Other Settings - Color Space'.");
				} else if (result == 1) { // Export Instructions
					Application.OpenURL(REEXPORT_INSTRUCTIONS_URL);
				} else { // Don't show again
					SpineEditorUtilities.Preferences.ShowWorkflowMismatchDialog = false;
					Debug.Log("Disabled Workflow Mismatch Dialog, only logging a warning. To re-enable go to 'Edit - Preferences - Spine'.");
				}
			} else {
				string title = "PMA vs Straight Alpha Preset Mismatch";
				string text;
				if (atlasIsPMA) {
					text = "Atlas was exported with 'Premultiply alpha' (PMA) enabled, but " +
						"the Spine Preferences Auto-Import presets are set to straight alpha.\n\n" +
						"We can switch your presets to PMA to fix this.\n\n" +
						"Alternatively you can re-export your atlas from Spine with straight alpha instead.";
				} else {
					text = "Atlas was exported with 'straight alpha' ('Premultiply alpha' disabled), but " +
						"the Spine Preferences Auto-Import presets are set to PMA.\n\n" +
						"We can switch your presets to straight alpha to fix this.\n\n" +
						"Alternatively you can re-export your atlas from Spine with Premultiply alpha instead.";
				}

				result = EditorUtility.DisplayDialogComplex(title,
					string.Format("Atlas '{0}':\n{1}", atlasName, text),
					"Switch Presets", "Export Instructions", "Don't show again");
				if (result == 0) { // Switch Presets
					if (atlasIsPMA) {
						SpineEditorUtilities.Preferences.SwitchToPMADefaults();
						Debug.Log("Switched Auto-Import presets to PMA texture workflow. To change it back go to 'Edit - Preferences - Spine'.");
					} else {
						SpineEditorUtilities.Preferences.SwitchToStraightAlphaDefaults();
						Debug.Log("Switched Auto-Import presets to straight alpha texture workflow. To change it back go to 'Edit - Preferences - Spine'.");
					}
				} else if (result == 1) { // Export Instructions
					Application.OpenURL(REEXPORT_INSTRUCTIONS_URL);
				} else { // Don't show again
					SpineEditorUtilities.Preferences.ShowWorkflowMismatchDialog = false;
					Debug.Log("Disabled Workflow Mismatch Dialog, only logging a warning. To re-enable go to 'Edit - Preferences - Spine'.");
				}
			}
			return (DialogResult)result;
		}
	}
#else
	using Icons = SpineEditorUtilities.Icons;

	public class WorkflowMismatchDialog : EditorWindow {
		public enum DialogResult {
			None = -1,
			Switch = 0,
			ReexportInstructions = 1,
			Ignore = 2
		}

		const string REEXPORT_INSTRUCTIONS_URL = "https://esotericsoftware.com/spine-unity-assets#Correct-Texture-Packer-export";

		private static DialogResult dialogResult = DialogResult.None;
		private static WorkflowMismatchDialog currentWindow;
		private bool isLinearPMAMismatch;
		private bool atlasIsPMA;
		private string atlasName;
		private bool dontShowAgain = false;

		public static DialogResult ShowDialog (string atlasName, bool isLinearPMAMismatch, bool atlasIsPMA) {

			if (currentWindow != null) {
				currentWindow.Close();
			}

			dialogResult = DialogResult.None;
			currentWindow = CreateInstance<WorkflowMismatchDialog>();
			currentWindow.atlasIsPMA = atlasIsPMA;
			currentWindow.atlasName = atlasName;
			currentWindow.isLinearPMAMismatch = isLinearPMAMismatch;

			string title = isLinearPMAMismatch ?
				"Premultiply Alpha Atlas not supported" : "PMA vs Straight Alpha Preset Mismatch";
			currentWindow.titleContent = new GUIContent(title);

			Vector2 windowSize = new Vector2(400, 240);
			currentWindow.minSize = windowSize;
			currentWindow.maxSize = windowSize;

			float x = (Screen.currentResolution.width - windowSize.x) / 2;
			float y = (Screen.currentResolution.height - windowSize.y) * 0.25f;
			currentWindow.position = new Rect(x, y, windowSize.x, windowSize.y);

			currentWindow.ShowModalUtility();
			return dialogResult;
		}

		void OnGUI () {
			GUIStyle dialogStyle = new GUIStyle("window");
			dialogStyle.padding = new RectOffset(0, 0, 0, 0);

			GUILayout.BeginArea(new Rect(15, 15, position.width - 30, position.height - 30));

			GUIStyle messageStyle = new GUIStyle(EditorStyles.label) {
				richText = true,
				wordWrap = true,
				fontSize = EditorStyles.label.fontSize,
				alignment = TextAnchor.UpperLeft
			};

			GUILayout.BeginHorizontal();

			var warningIcon = Icons.warning;
			if (warningIcon != null) {
				GUI.DrawTexture(new Rect(0, 0, 60, 60), warningIcon);
				GUILayout.Space(70);
			}

			if (isLinearPMAMismatch)
				DrawLinearColorSpacePMAMismatch(atlasName, messageStyle);
			else
				DrawStraightPMAPresetsMismatch(messageStyle, atlasIsPMA);

			GUILayout.EndHorizontal();

			GUILayout.EndArea();
		}

		void DrawLinearColorSpacePMAMismatch (string atlasName, GUIStyle messageStyle) {
			GUILayout.BeginVertical();
			GUILayout.Label(
				string.Format("Atlas '{0}':\n" +
				"Atlas was exported with '<b>Premultiply alpha</b>' (PMA) enabled, but " +
				"the Unity project is set to <b>Linear color space</b>, which does not support PMA.\n\n" +
				"We can <b>switch your Unity Project to Gamma</b> color space to fix this.\n\n" +
				"Alternatively you can <b>re-export your atlas</b> from Spine with <b>straight alpha</b> instead.", atlasName),
				messageStyle
			);
			GUILayout.Space(10);
			dontShowAgain = !SpineEditorUtilities.Preferences.ShowWorkflowMismatchDialog;
			dontShowAgain = EditorGUILayout.ToggleLeft(new GUIContent("Don't show again",
				"Don't show this dialog again and only log a warning. To re-enable go to 'Edit - Preferences - Spine'."),
				dontShowAgain);
			if (dontShowAgain == SpineEditorUtilities.Preferences.ShowWorkflowMismatchDialog) {
				SpineEditorUtilities.Preferences.ShowWorkflowMismatchDialog = !dontShowAgain;
				if (dontShowAgain)
					Debug.Log("Disabled Workflow Mismatch Dialog, only logging a warning. To re-enable go to 'Edit - Preferences - Spine'.");
			}

			GUILayout.Space(10);
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Switch to Gamma", GUILayout.Width(120), GUILayout.Height(24))) {
				dialogResult = DialogResult.Switch;
				PlayerSettings.colorSpace = ColorSpace.Gamma;
				Debug.Log("Switched Unity project to Gamma color space to support PMA atlas textures. To change it back go to 'Project Settings - Player - Other Settings - Color Space'.");
				currentWindow = null;
				Close();
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Re-export Instructions", GUILayout.Width(140), GUILayout.Height(24))) {
				dialogResult = DialogResult.ReexportInstructions;
				Application.OpenURL(REEXPORT_INSTRUCTIONS_URL);
				currentWindow = null;
				Close();
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Ignore", GUILayout.Width(75), GUILayout.Height(24))) {
				dialogResult = DialogResult.Ignore;
				currentWindow = null;
				Close();
			}
		}

		void DrawStraightPMAPresetsMismatch (GUIStyle messageStyle, bool atlasIsPMA) {
			GUILayout.BeginVertical();

			if (atlasIsPMA) {
				GUILayout.Label(
					string.Format("Atlas '{0}':\n" +
					"Atlas was exported with '<b>Premultiply alpha</b>' (PMA) enabled, but " +
					"the Spine Preferences Auto-Import presets are set to <b>straight alpha</b>.\n\n" +
					"We can <b>switch your presets to PMA</b> to fix this.\n\n" +
					"Alternatively you can <b>re-export your atlas</b> from Spine with <b>straight alpha</b> instead.", atlasName),
					messageStyle);
			} else {
				GUILayout.Label(
					string.Format("Atlas '{0}':\n" +
					"Atlas was exported with '<b>straight alpha</b>' ('Premultiply alpha' disabled), but " +
					"the Spine Preferences Auto-Import presets are set to <b>PMA</b>.\n\n" +
					"We can <b>switch your presets to straight alpha</b> to fix this.\n\n" +
					"Alternatively you can <b>re-export your atlas</b> from Spine with <b>Premultiply alpha</b> instead.", atlasName),
					messageStyle);
			}

			GUILayout.Space(10);
			dontShowAgain = !SpineEditorUtilities.Preferences.ShowWorkflowMismatchDialog;
			dontShowAgain = EditorGUILayout.ToggleLeft(new GUIContent("Don't show again",
				"Don't show this dialog again and only log a warning. To re-enabled go to 'Edit - Preferences - Spine'."),
				dontShowAgain);
			if (dontShowAgain == SpineEditorUtilities.Preferences.ShowWorkflowMismatchDialog) {
				SpineEditorUtilities.Preferences.ShowWorkflowMismatchDialog = !dontShowAgain;
				if (dontShowAgain)
					Debug.Log("Disabled Workflow Mismatch Dialog, only logging a warning. To re-enable go to 'Edit - Preferences - Spine'.");
			}

			GUILayout.Space(10);
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Switch Presets", GUILayout.Width(120), GUILayout.Height(24))) {
				dialogResult = DialogResult.Switch;
				if (atlasIsPMA) {
					SpineEditorUtilities.Preferences.SwitchToPMADefaults();
					Debug.Log("Switched Auto-Import presets to PMA texture workflow. To change it back go to 'Edit - Preferences - Spine'.");
				} else {
					SpineEditorUtilities.Preferences.SwitchToStraightAlphaDefaults();
					Debug.Log("Switched Auto-Import presets to straight alpha texture workflow. To change it back go to 'Edit - Preferences - Spine'.");
				}
				currentWindow = null;
				Close();
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Re-export Instructions", GUILayout.Width(140), GUILayout.Height(24))) {
				dialogResult = DialogResult.ReexportInstructions;
				Application.OpenURL(REEXPORT_INSTRUCTIONS_URL);
				currentWindow = null;
				Close();
			}
			GUILayout.Space(5);
			if (GUILayout.Button("Ignore", GUILayout.Width(75), GUILayout.Height(24))) {
				dialogResult = DialogResult.Ignore;
				currentWindow = null;
				Close();
			}
		}

		void OnDestroy () {
			if (currentWindow == this) {
				currentWindow = null;
				if (dialogResult == DialogResult.None)
					dialogResult = DialogResult.Ignore;
			}
		}
	}
#endif
}
