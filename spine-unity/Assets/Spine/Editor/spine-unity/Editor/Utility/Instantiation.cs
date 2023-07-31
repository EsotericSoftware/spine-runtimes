/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#pragma warning disable 0219

#define SPINE_SKELETONMECANIM

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using EventType = UnityEngine.EventType;

	public partial class SpineEditorUtilities {
		public static class DragAndDropInstantiation {
			public struct SpawnMenuData {
				public Vector3 spawnPoint;
				public Transform parent;
				public int siblingIndex;
				public SkeletonDataAsset skeletonDataAsset;
				public EditorInstantiation.InstantiateDelegate instantiateDelegate;
				public bool isUI;
			}

			public static void SceneViewDragAndDrop (SceneView sceneview) {
				UnityEngine.Event current = UnityEngine.Event.current;
				UnityEngine.Object[] references = DragAndDrop.objectReferences;
				if (current.type == EventType.Layout)
					return;

				// Allow drag and drop of one SkeletonDataAsset.
				if (references.Length == 1) {
					SkeletonDataAsset skeletonDataAsset = references[0] as SkeletonDataAsset;
					if (skeletonDataAsset != null) {
						Vector2 mousePos = current.mousePosition;

						bool invalidSkeletonData = skeletonDataAsset.GetSkeletonData(true) == null;
						if (invalidSkeletonData) {
							DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
							Handles.BeginGUI();
							GUI.Label(new Rect(mousePos + new Vector2(20f, 20f), new Vector2(400f, 40f)), new GUIContent(string.Format("{0} is invalid.\nCannot create new Spine GameObject.", skeletonDataAsset.name), SpineEditorUtilities.Icons.warning));
							Handles.EndGUI();
							return;
						} else {
							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
							Handles.BeginGUI();
							GUI.Label(new Rect(mousePos + new Vector2(20f, 20f), new Vector2(400f, 20f)), new GUIContent(string.Format("Create Spine GameObject ({0})", skeletonDataAsset.skeletonJSON.name), SpineEditorUtilities.Icons.skeletonDataAssetIcon));
							Handles.EndGUI();

							if (current.type == EventType.DragPerform) {
								RectTransform rectTransform = (Selection.activeGameObject == null) ? null : Selection.activeGameObject.GetComponent<RectTransform>();
								Plane plane = (rectTransform == null) ? new Plane(Vector3.back, Vector3.zero) : new Plane(-rectTransform.forward, rectTransform.position);
								Vector3 spawnPoint = MousePointToWorldPoint2D(mousePos, sceneview.camera, plane);
								ShowInstantiateContextMenu(skeletonDataAsset, spawnPoint, null, 0);
								DragAndDrop.AcceptDrag();
								current.Use();
							}
						}
					}
				}
			}

			public static void ShowInstantiateContextMenu (SkeletonDataAsset skeletonDataAsset, Vector3 spawnPoint,
				Transform parent, int siblingIndex = 0) {
				GenericMenu menu = new GenericMenu();

				// SkeletonAnimation
				menu.AddItem(new GUIContent("SkeletonAnimation"), false, HandleSkeletonComponentDrop, new SpawnMenuData {
					skeletonDataAsset = skeletonDataAsset,
					spawnPoint = spawnPoint,
					parent = parent,
					siblingIndex = siblingIndex,
					instantiateDelegate = (data) => EditorInstantiation.InstantiateSkeletonAnimation(data),
					isUI = false
				});

				// SkeletonGraphic
				System.Type skeletonGraphicInspectorType = System.Type.GetType("Spine.Unity.Editor.SkeletonGraphicInspector");
				if (skeletonGraphicInspectorType != null) {
					MethodInfo graphicInstantiateDelegate = skeletonGraphicInspectorType.GetMethod("SpawnSkeletonGraphicFromDrop", BindingFlags.Static | BindingFlags.Public);
					if (graphicInstantiateDelegate != null)
						menu.AddItem(new GUIContent("SkeletonGraphic (UI)"), false, HandleSkeletonComponentDrop, new SpawnMenuData {
							skeletonDataAsset = skeletonDataAsset,
							spawnPoint = spawnPoint,
							parent = parent,
							siblingIndex = siblingIndex,
							instantiateDelegate = System.Delegate.CreateDelegate(typeof(EditorInstantiation.InstantiateDelegate), graphicInstantiateDelegate) as EditorInstantiation.InstantiateDelegate,
							isUI = true
						});
				}

#if SPINE_SKELETONMECANIM
				menu.AddSeparator("");
				// SkeletonMecanim
				menu.AddItem(new GUIContent("SkeletonMecanim"), false, HandleSkeletonComponentDrop, new SpawnMenuData {
					skeletonDataAsset = skeletonDataAsset,
					spawnPoint = spawnPoint,
					parent = parent,
					siblingIndex = siblingIndex,
					instantiateDelegate = (data) => EditorInstantiation.InstantiateSkeletonMecanim(data),
					isUI = false
				});
#endif

				menu.ShowAsContext();
			}

			public static void HandleSkeletonComponentDrop (object spawnMenuData) {
				SpawnMenuData data = (SpawnMenuData)spawnMenuData;

				if (data.skeletonDataAsset.GetSkeletonData(true) == null) {
					EditorUtility.DisplayDialog("Invalid SkeletonDataAsset", "Unable to create Spine GameObject.\n\nPlease check your SkeletonDataAsset.", "Ok");
					return;
				}

				bool isUI = data.isUI;

				Component newSkeletonComponent = data.instantiateDelegate.Invoke(data.skeletonDataAsset);
				GameObject newGameObject = newSkeletonComponent.gameObject;
				Transform newTransform = newGameObject.transform;

				GameObject usedParent = data.parent != null ? data.parent.gameObject : isUI ? Selection.activeGameObject : null;
				if (usedParent)
					newTransform.SetParent(usedParent.transform, false);
				if (data.siblingIndex != 0)
					newTransform.SetSiblingIndex(data.siblingIndex);

				newTransform.position = isUI ? data.spawnPoint : RoundVector(data.spawnPoint, 2);

				if (isUI) {
					SkeletonGraphic skeletonGraphic = ((SkeletonGraphic)newSkeletonComponent);
					if (usedParent != null && usedParent.GetComponent<RectTransform>() != null) {
						skeletonGraphic.MatchRectTransformWithBounds();
					} else
						Debug.Log("Created a UI Skeleton GameObject not under a RectTransform. It may not be visible until you parent it to a canvas.");
					if (skeletonGraphic.HasMultipleSubmeshInstructions() && !skeletonGraphic.allowMultipleCanvasRenderers)
						Debug.Log("This mesh uses multiple atlas pages or blend modes. " +
							"You need to enable 'Multiple Canvas Renderers for correct rendering. " +
							"Consider packing attachments to a single atlas page if possible.", skeletonGraphic);
				}

				if (!isUI && usedParent != null && usedParent.transform.localScale != Vector3.one)
					Debug.Log("New Spine GameObject was parented to a scaled Transform. It may not be the intended size.");

				Selection.activeGameObject = newGameObject;
				//EditorGUIUtility.PingObject(newGameObject); // Doesn't work when setting activeGameObject.
				Undo.RegisterCreatedObjectUndo(newGameObject, "Create Spine GameObject");
			}

			/// <summary>
			/// Rounds off vector components to a number of decimal digits.
			/// </summary>
			public static Vector3 RoundVector (Vector3 vector, int digits) {
				vector.x = (float)System.Math.Round(vector.x, digits);
				vector.y = (float)System.Math.Round(vector.y, digits);
				vector.z = (float)System.Math.Round(vector.z, digits);
				return vector;
			}

			/// <summary>
			/// Converts a mouse point to a world point on a plane.
			/// </summary>
			static Vector3 MousePointToWorldPoint2D (Vector2 mousePosition, Camera camera, Plane plane) {
				Vector3 screenPos = new Vector3(mousePosition.x, camera.pixelHeight - mousePosition.y, 0f);
				Ray ray = camera.ScreenPointToRay(screenPos);
				float distance;
				bool hit = plane.Raycast(ray, out distance);
				return ray.GetPoint(distance);
			}
		}
	}
}
