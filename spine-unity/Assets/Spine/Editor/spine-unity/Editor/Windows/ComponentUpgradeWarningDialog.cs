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

#if UNITY_2018_2_OR_NEWER
#define HAS_BATCHMODE_QUERY
#endif

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	using Icons = SpineEditorUtilities.Icons;

	public class ComponentUpgradeWarningDialog : EditorWindow {

		public enum DialogResult {
			None = -1,
			OpenGuide = 0,
			Continue = 1
		}

		const string UPGRADE_GUIDE_URL = "https://github.com/EsotericSoftware/spine-runtimes/tree/4.3-beta/spine-unity/Assets/Spine/Documentation/4.3-split-component-upgrade-guide.md";

		private static DialogResult dialogResult = DialogResult.None;
		private static ComponentUpgradeWarningDialog currentWindow;

		public static DialogResult ShowDialog () {
#if HAS_BATCHMODE_QUERY
			if (Application.isBatchMode) return DialogResult.None;
#endif
			if (currentWindow != null) {
				currentWindow.Close();
			}

			dialogResult = DialogResult.None;
			currentWindow = CreateInstance<ComponentUpgradeWarningDialog>();

			string title = "Spine Unity 4.3 - Critical Upgrade Notice";
			currentWindow.titleContent = new GUIContent(title);

			Vector2 windowSize = new Vector2(500, 450);
			currentWindow.minSize = windowSize;
			currentWindow.maxSize = windowSize;

			float x = (Screen.currentResolution.width - windowSize.x) / 2;
			float y = (Screen.currentResolution.height - windowSize.y) * 0.25f;
			currentWindow.position = new Rect(x, y, windowSize.x, windowSize.y);

#if HAS_MODAL_UTILITY
			currentWindow.ShowModalUtility();
#else
			currentWindow.ShowUtility();
#endif
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

			GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) {
				richText = true,
				fontSize = EditorStyles.boldLabel.fontSize,
				alignment = TextAnchor.UpperLeft
			};

			GUILayout.BeginHorizontal();

			var warningIcon = Icons.warning;
			if (warningIcon != null) {
				GUI.DrawTexture(new Rect(0, 0, 60, 60), warningIcon);
				GUILayout.Space(70);
			}

			GUILayout.BeginVertical();

			GUILayout.Label("<b>New projects:</b> Ignore this message.", messageStyle);
			GUILayout.Label("<b>Existing projects:</b> You MUST read the upgrade guide!", messageStyle);
			GUILayout.Space(10);

			GUILayout.Label("<b>Major Architecture Change</b>", headerStyle);
			GUILayout.Label(
				"Main skeleton components are now " +
				"<b>split into separate rendering and animation components</b>.\n\n" +
				"Components will be <b>automatically split</b> when scenes/prefabs are opened:\n" +
				"• <b>SkeletonAnimation</b> → SkeletonAnimation + SkeletonRenderer\n" +
				"• <b>SkeletonMecanim</b> → SkeletonMecanim + SkeletonRenderer\n" +
				"• <b>SkeletonGraphic</b> → SkeletonAnimation + SkeletonGraphic",
				messageStyle
			);
			GUILayout.Space(10);

			GUILayout.Label("<b>Without proper preparation:</b>", headerStyle);
			GUILayout.Label(
				"• Component references in your scripts may be lost\n" +
				"• Builds may have missing components",
				messageStyle
			);
			GUILayout.Space(10);

			GUILayout.Label("<b>Required Steps:</b>", headerStyle);
			GUILayout.Label(
				"1. <b>Backup your project NOW</b>\n" +
				"2. <b>READ: 4.3-split-component-upgrade-guide.md</b>\n" +
				"3. <b>Proceed according to the guide.</b>",
				messageStyle
			);
			GUILayout.Space(15);

			GUILayout.Label(
				"<color=red><b>Do not proceed without reading the guide or you risk breaking your project.</b></color>",
				messageStyle
			);

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Open Upgrade Guide", GUILayout.Width(140), GUILayout.Height(28))) {
				dialogResult = DialogResult.OpenGuide;
				Application.OpenURL(UPGRADE_GUIDE_URL);
				currentWindow = null;
				Close();
			}
			GUILayout.Space(10);
			if (GUILayout.Button("I understand the risks - Continue", GUILayout.Width(200), GUILayout.Height(28))) {
				dialogResult = DialogResult.Continue;
				currentWindow = null;
				SpineEditorUtilities.Preferences.ShowSplitComponentChangeWarning = false;
				Close();
			}

			GUILayout.EndHorizontal();

			GUILayout.EndArea();
		}

		void OnDestroy () {
			if (currentWindow == this) {
				currentWindow = null;
				if (dialogResult == DialogResult.None)
					dialogResult = DialogResult.Continue;
			}
		}
	}
}