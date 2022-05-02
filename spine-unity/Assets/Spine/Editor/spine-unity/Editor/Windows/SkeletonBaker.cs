/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

#define SPINE_SKELETONMECANIM

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	/// <summary>
	/// [SUPPORTS]
	/// Linear, Constant, and Bezier Curves*
	/// Inverse Kinematics*
	/// Inherit Rotation
	/// Translate Timeline
	/// Rotate Timeline
	/// Scale Timeline**
	/// Event Timeline***
	/// Attachment Timeline
	///
	/// RegionAttachment
	/// MeshAttachment (optionally Skinned)
	///
	/// [LIMITATIONS]
	/// *Bezier Curves are baked into the animation at 60fps and are not realtime. Use bakeIncrement constant to adjust key density if desired.
	/// *Inverse Kinematics is baked into the animation at 60fps and are not realtime. Use bakeIncrement constant to adjust key density if desired.
	/// ***Events may only fire 1 type of data per event in Unity safely so priority to String data if present in Spine key, otherwise a Float is sent whether the Spine key was Int or Float with priority given to Int.
	///
	/// [DOES NOT SUPPORT]
	/// FFD (Unity does not provide access to BlendShapes with code)
	/// Color Keys (Maybe one day when Unity supports full FBX standard and provides access with code)
	/// Draw Order Keyframes
	/// </summary>
	public static class SkeletonBaker {

		const string SpineEventStringId = "SpineEvent";
		const float EventTimeEqualityEpsilon = 0.01f;

		#region SkeletonMecanim's Mecanim Clips
#if SPINE_SKELETONMECANIM
		public static void UpdateMecanimClips (SkeletonDataAsset skeletonDataAsset) {
			if (skeletonDataAsset.controller == null)
				return;

			SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
		}

		public static void GenerateMecanimAnimationClips (SkeletonDataAsset skeletonDataAsset) {
			var data = skeletonDataAsset.GetSkeletonData(true);
			if (data == null) {
				Debug.LogError("SkeletonData loading failed!", skeletonDataAsset);
				return;
			}

			string dataPath = AssetDatabase.GetAssetPath(skeletonDataAsset);
			string controllerPath = dataPath.Replace(AssetUtility.SkeletonDataSuffix, "_Controller").Replace(".asset", ".controller");
			UnityEditor.Animations.AnimatorController controller;
			if (skeletonDataAsset.controller != null) {
				controller = (UnityEditor.Animations.AnimatorController)skeletonDataAsset.controller;
				controllerPath = AssetDatabase.GetAssetPath(controller);
			} else {
				if (File.Exists(controllerPath)) {
					if (EditorUtility.DisplayDialog("Controller Overwrite Warning", "Unknown Controller already exists at: " + controllerPath, "Update", "Overwrite")) {
						controller = (UnityEditor.Animations.AnimatorController)AssetDatabase.LoadAssetAtPath(controllerPath, typeof(RuntimeAnimatorController));
					} else {
						controller = (UnityEditor.Animations.AnimatorController)UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
					}
				} else {
					controller = (UnityEditor.Animations.AnimatorController)UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
				}

			}

			skeletonDataAsset.controller = controller;
			EditorUtility.SetDirty(skeletonDataAsset);

			UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(controllerPath);

			var unityAnimationClipTable = new Dictionary<string, AnimationClip>();
			var spineAnimationTable = new Dictionary<string, Spine.Animation>();

			foreach (var o in objs) {
				//Debug.LogFormat("({0}){1} : {3} + {2} + {4}", o.GetType(), o.name, o.hideFlags, o.GetInstanceID(), o.GetHashCode());
				// There is a bug in Unity 5.3.3 (and likely before) that creates
				// a duplicate AnimationClip when you duplicate a Mecanim Animator State.
				// These duplicates seem to be identifiable by their HideFlags, so we'll exclude them.
				if (o is AnimationClip) {
					var clip = o as AnimationClip;
					if (!clip.HasFlag(HideFlags.HideInHierarchy)) {
						if (unityAnimationClipTable.ContainsKey(clip.name)) {
							Debug.LogWarningFormat("Duplicate AnimationClips were found named {0}", clip.name);
						}
						unityAnimationClipTable.Add(clip.name, clip);
					}
				}
			}

			foreach (var animations in data.Animations) {
				string animationName = animations.Name; // Review for unsafe names. Requires runtime implementation too.
				spineAnimationTable.Add(animationName, animations);

				if (unityAnimationClipTable.ContainsKey(animationName) == false) {
					AnimationClip newClip = new AnimationClip {
						name = animationName
					};
					//AssetDatabase.CreateAsset(newClip, Path.GetDirectoryName(dataPath) + "/" + animationName + ".asset");
					AssetDatabase.AddObjectToAsset(newClip, controller);
					unityAnimationClipTable.Add(animationName, newClip);
				}

				AnimationClip clip = unityAnimationClipTable[animationName];
				clip.SetCurve("", typeof(GameObject), "dummy", AnimationCurve.Linear(0, 0, animations.Duration, 0));
				var settings = AnimationUtility.GetAnimationClipSettings(clip);
				settings.stopTime = animations.Duration;
				SetAnimationSettings(clip, settings);

				var previousAnimationEvents = AnimationUtility.GetAnimationEvents(clip);
				var animationEvents = new List<AnimationEvent>();
				foreach (Timeline t in animations.Timelines) {
					if (t is EventTimeline)
						ParseEventTimeline(ref animationEvents, (EventTimeline)t, SendMessageOptions.DontRequireReceiver);
				}
				AddPreviousUserEvents(ref animationEvents, previousAnimationEvents);
				AnimationUtility.SetAnimationEvents(clip, animationEvents.ToArray());

				EditorUtility.SetDirty(clip);
				unityAnimationClipTable.Remove(animationName);
			}

			foreach (var clip in unityAnimationClipTable.Values) {
				AnimationClip.DestroyImmediate(clip, true);
			}

			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
		}

		static bool HasFlag (this UnityEngine.Object o, HideFlags flagToCheck) {
			return (o.hideFlags & flagToCheck) == flagToCheck;
		}
#endif
		#endregion

		#region Prefab and AnimationClip Baking
		/// <summary>
		/// Interval between key sampling for Bezier curves, IK controlled bones, and Inherit Rotation effected bones.
		/// </summary>
		const float BakeIncrement = 1 / 60f;

		public static void BakeToPrefab (SkeletonDataAsset skeletonDataAsset, ExposedList<Skin> skins, string outputPath = "", bool bakeAnimations = true, bool bakeIK = true, SendMessageOptions eventOptions = SendMessageOptions.DontRequireReceiver) {
			if (skeletonDataAsset == null || skeletonDataAsset.GetSkeletonData(true) == null) {
				Debug.LogError("Could not export Spine Skeleton because SkeletonData Asset is null or invalid!");
				return;
			}

			if (outputPath == "") {
				outputPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(skeletonDataAsset)).Replace('\\', '/') + "/Baked";
				System.IO.Directory.CreateDirectory(outputPath);
			}

			var skeletonData = skeletonDataAsset.GetSkeletonData(true);
			bool hasAnimations = bakeAnimations && skeletonData.Animations.Count > 0;
			UnityEditor.Animations.AnimatorController controller = null;
			if (hasAnimations) {
				string controllerPath = outputPath + "/" + skeletonDataAsset.skeletonJSON.name + " Controller.controller";
				bool newAnimContainer = false;

				var runtimeController = AssetDatabase.LoadAssetAtPath(controllerPath, typeof(RuntimeAnimatorController));

				if (runtimeController != null) {
					controller = (UnityEditor.Animations.AnimatorController)runtimeController;
				} else {
					controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
					newAnimContainer = true;
				}

				var existingClipTable = new Dictionary<string, AnimationClip>();
				var unusedClipNames = new List<string>();
				Object[] animObjs = AssetDatabase.LoadAllAssetsAtPath(controllerPath);

				foreach (Object o in animObjs) {
					if (o is AnimationClip) {
						var clip = (AnimationClip)o;
						existingClipTable.Add(clip.name, clip);
						unusedClipNames.Add(clip.name);
					}
				}

				Dictionary<int, List<string>> slotLookup = new Dictionary<int, List<string>>();

				int skinCount = skins.Count;

				for (int s = 0; s < skeletonData.Slots.Count; s++) {
					List<string> attachmentNames = new List<string>();
					for (int i = 0; i < skinCount; i++) {
						var skin = skins.Items[i];
						var skinEntries = new List<Skin.SkinEntry>();
						skin.GetAttachments(s, skinEntries);
						foreach (var entry in skinEntries) {
							if (!attachmentNames.Contains(entry.Name))
								attachmentNames.Add(entry.Name);
						}
					}
					slotLookup.Add(s, attachmentNames);
				}

				foreach (var anim in skeletonData.Animations) {

					AnimationClip clip = null;
					if (existingClipTable.ContainsKey(anim.Name)) {
						clip = existingClipTable[anim.Name];
					}

					clip = ExtractAnimation(anim.Name, skeletonData, slotLookup, bakeIK, eventOptions, clip);

					if (unusedClipNames.Contains(clip.name)) {
						unusedClipNames.Remove(clip.name);
					} else {
						AssetDatabase.AddObjectToAsset(clip, controller);
						controller.AddMotion(clip);
					}
				}

				if (newAnimContainer) {
					EditorUtility.SetDirty(controller);
					AssetDatabase.SaveAssets();
					AssetDatabase.ImportAsset(controllerPath, ImportAssetOptions.ForceUpdate);
					AssetDatabase.Refresh();
				} else {

					foreach (string str in unusedClipNames) {
						AnimationClip.DestroyImmediate(existingClipTable[str], true);
					}

					EditorUtility.SetDirty(controller);
					AssetDatabase.SaveAssets();
					AssetDatabase.ImportAsset(controllerPath, ImportAssetOptions.ForceUpdate);
					AssetDatabase.Refresh();
				}
			}

			foreach (var skin in skins) {
				bool newPrefab = false;

				string prefabPath = outputPath + "/" + skeletonDataAsset.skeletonJSON.name + " (" + skin.Name + ").prefab";

				Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));

				if (prefab == null) {
#if NEW_PREFAB_SYSTEM
					GameObject emptyGameObject = new GameObject();
					prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(emptyGameObject, prefabPath, InteractionMode.AutomatedAction);
					GameObject.DestroyImmediate(emptyGameObject);
#else
					prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
#endif
					newPrefab = true;
				}

				Dictionary<string, Mesh> meshTable = new Dictionary<string, Mesh>();
				List<string> unusedMeshNames = new List<string>();
				Object[] assets = AssetDatabase.LoadAllAssetsAtPath(prefabPath);
				foreach (var obj in assets) {
					if (obj is Mesh) {
						meshTable.Add(obj.name, (Mesh)obj);
						unusedMeshNames.Add(obj.name);
					}
				}

				GameObject prefabRoot = EditorInstantiation.NewGameObject("root", true);

				Dictionary<string, Transform> slotTable = new Dictionary<string, Transform>();
				Dictionary<string, Transform> boneTable = new Dictionary<string, Transform>();
				List<Transform> boneList = new List<Transform>();

				//create bones
				for (int i = 0; i < skeletonData.Bones.Count; i++) {
					var boneData = skeletonData.Bones.Items[i];
					Transform boneTransform = EditorInstantiation.NewGameObject(boneData.Name, true).transform;
					boneTransform.parent = prefabRoot.transform;
					boneTable.Add(boneTransform.name, boneTransform);
					boneList.Add(boneTransform);
				}

				for (int i = 0; i < skeletonData.Bones.Count; i++) {

					var boneData = skeletonData.Bones.Items[i];
					Transform boneTransform = boneTable[boneData.Name];
					Transform parentTransform = null;
					if (i > 0)
						parentTransform = boneTable[boneData.Parent.Name];
					else
						parentTransform = boneTransform.parent;

					boneTransform.parent = parentTransform;
					boneTransform.localPosition = new Vector3(boneData.X, boneData.Y, 0);
					var tm = boneData.TransformMode;
					if (tm.InheritsRotation())
						boneTransform.localRotation = Quaternion.Euler(0, 0, boneData.Rotation);
					else
						boneTransform.rotation = Quaternion.Euler(0, 0, boneData.Rotation);

					if (tm.InheritsScale())
						boneTransform.localScale = new Vector3(boneData.ScaleX, boneData.ScaleY, 1);
				}

				//create slots and attachments
				for (int slotIndex = 0; slotIndex < skeletonData.Slots.Count; slotIndex++) {
					var slotData = skeletonData.Slots.Items[slotIndex];
					Transform slotTransform = EditorInstantiation.NewGameObject(slotData.Name, true).transform;
					slotTransform.parent = prefabRoot.transform;
					slotTable.Add(slotData.Name, slotTransform);

					var skinEntries = new List<Skin.SkinEntry>();
					skin.GetAttachments(slotIndex, skinEntries);
					if (skin != skeletonData.DefaultSkin)
						skeletonData.DefaultSkin.GetAttachments(slotIndex, skinEntries);

					for (int a = 0; a < skinEntries.Count; a++) {
						var attachment = skinEntries[a].Attachment;
						string attachmentName = skinEntries[a].Name;
						string attachmentMeshName = "[" + slotData.Name + "] " + attachmentName;
						Vector3 offset = Vector3.zero;
						float rotation = 0;
						Mesh mesh = null;
						Material material = null;
						bool isWeightedMesh = false;

						if (meshTable.ContainsKey(attachmentMeshName))
							mesh = meshTable[attachmentMeshName];
						if (attachment is RegionAttachment) {
							var regionAttachment = (RegionAttachment)attachment;
							offset.x = regionAttachment.X;
							offset.y = regionAttachment.Y;
							rotation = regionAttachment.Rotation;
							mesh = ExtractRegionAttachment(attachmentMeshName, regionAttachment, mesh);
							material = attachment.GetMaterial();
							unusedMeshNames.Remove(attachmentMeshName);
							if (newPrefab || meshTable.ContainsKey(attachmentMeshName) == false)
								AssetDatabase.AddObjectToAsset(mesh, prefab);
						} else if (attachment is MeshAttachment) {
							var meshAttachment = (MeshAttachment)attachment;
							isWeightedMesh = (meshAttachment.Bones != null);
							offset.x = 0;
							offset.y = 0;
							rotation = 0;

							if (isWeightedMesh)
								mesh = ExtractWeightedMeshAttachment(attachmentMeshName, meshAttachment, slotIndex, skeletonData, boneList, mesh);
							else
								mesh = ExtractMeshAttachment(attachmentMeshName, meshAttachment, mesh);

							material = attachment.GetMaterial();
							unusedMeshNames.Remove(attachmentMeshName);
							if (newPrefab || meshTable.ContainsKey(attachmentMeshName) == false)
								AssetDatabase.AddObjectToAsset(mesh, prefab);
						} else
							continue;

						Transform attachmentTransform = EditorInstantiation.NewGameObject(attachmentName, true).transform;

						attachmentTransform.parent = slotTransform;
						attachmentTransform.localPosition = offset;
						attachmentTransform.localRotation = Quaternion.Euler(0, 0, rotation);

						if (isWeightedMesh) {
							attachmentTransform.position = Vector3.zero;
							attachmentTransform.rotation = Quaternion.identity;
							var skinnedMeshRenderer = attachmentTransform.gameObject.AddComponent<SkinnedMeshRenderer>();
							skinnedMeshRenderer.rootBone = boneList[0];
							skinnedMeshRenderer.bones = boneList.ToArray();
							skinnedMeshRenderer.sharedMesh = mesh;
						} else {
							attachmentTransform.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
							attachmentTransform.gameObject.AddComponent<MeshRenderer>();
						}

						attachmentTransform.GetComponent<Renderer>().sharedMaterial = material;
						attachmentTransform.GetComponent<Renderer>().sortingOrder = slotIndex;

						if (attachmentName != slotData.AttachmentName)
							attachmentTransform.gameObject.SetActive(false);
					}

				}

				foreach (var slotData in skeletonData.Slots) {
					Transform slotTransform = slotTable[slotData.Name];
					slotTransform.parent = boneTable[slotData.BoneData.Name];
					slotTransform.localPosition = Vector3.zero;
					slotTransform.localRotation = Quaternion.identity;
					slotTransform.localScale = Vector3.one;
				}

				if (hasAnimations) {
					var animator = prefabRoot.AddComponent<Animator>();
					animator.applyRootMotion = false;
					animator.runtimeAnimatorController = (RuntimeAnimatorController)controller;
					EditorGUIUtility.PingObject(controller);
				}

				if (newPrefab) {
#if NEW_PREFAB_SYSTEM
					PrefabUtility.SaveAsPrefabAssetAndConnect(prefabRoot, prefabPath, InteractionMode.AutomatedAction);
#else
					PrefabUtility.ReplacePrefab(prefabRoot, prefab, ReplacePrefabOptions.ConnectToPrefab);
#endif
				} else {

					foreach (string str in unusedMeshNames) {
						Mesh.DestroyImmediate(meshTable[str], true);
					}

#if NEW_PREFAB_SYSTEM
					PrefabUtility.SaveAsPrefabAssetAndConnect(prefabRoot, prefabPath, InteractionMode.AutomatedAction);
#else
					PrefabUtility.ReplacePrefab(prefabRoot, prefab, ReplacePrefabOptions.ReplaceNameBased);
#endif
				}


				EditorGUIUtility.PingObject(prefab);

				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();

				GameObject.DestroyImmediate(prefabRoot);

			}
		}

		#region Attachment Baking
		static Bone DummyBone;
		static Slot DummySlot;

		internal static Bone GetDummyBone () {
			if (DummyBone != null)
				return DummyBone;

			SkeletonData skelData = new SkeletonData();
			BoneData data = new BoneData(0, "temp", null) {
				ScaleX = 1,
				ScaleY = 1,
				Length = 100
			};

			skelData.Bones.Add(data);

			Skeleton skeleton = new Skeleton(skelData);

			Bone bone = new Bone(data, skeleton, null);
			bone.UpdateWorldTransform();

			DummyBone = bone;

			return DummyBone;
		}

		internal static Slot GetDummySlot () {
			if (DummySlot != null)
				return DummySlot;

			Bone bone = GetDummyBone();

			SlotData data = new SlotData(0, "temp", bone.Data);
			Slot slot = new Slot(data, bone);
			DummySlot = slot;
			return DummySlot;
		}

		internal static Mesh ExtractRegionAttachment (string name, RegionAttachment attachment, Mesh mesh = null, bool centered = true) {
			var slot = GetDummySlot();
			var bone = slot.Bone;

			if (centered) {
				bone.X = -attachment.X;
				bone.Y = -attachment.Y;
			}

			bone.UpdateWorldTransform();

			Vector2[] uvs = ExtractUV(attachment.UVs);
			float[] floatVerts = new float[8];
			attachment.ComputeWorldVertices(slot, floatVerts, 0);
			Vector3[] verts = ExtractVerts(floatVerts);

			//unrotate verts now that they're centered
			if (centered) {
				for (int i = 0; i < verts.Length; i++)
					verts[i] = Quaternion.Euler(0, 0, -attachment.Rotation) * verts[i];
			}

			int[] triangles = { 1, 3, 0, 2, 3, 1 };
			Color color = attachment.GetColor();

			if (mesh == null)
				mesh = new Mesh();

			mesh.triangles = new int[0];

			mesh.vertices = verts;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.colors = new[] { color, color, color, color };
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.name = name;

			return mesh;
		}

		internal static Mesh ExtractMeshAttachment (string name, MeshAttachment attachment, Mesh mesh = null) {
			var slot = GetDummySlot();

			slot.Bone.X = 0;
			slot.Bone.Y = 0;
			slot.Bone.UpdateWorldTransform();

			Vector2[] uvs = ExtractUV(attachment.UVs);
			float[] floatVerts = new float[attachment.WorldVerticesLength];
			attachment.ComputeWorldVertices(slot, floatVerts);
			Vector3[] verts = ExtractVerts(floatVerts);

			int[] triangles = attachment.Triangles;
			Color color = attachment.GetColor();

			if (mesh == null)
				mesh = new Mesh();

			mesh.triangles = new int[0];

			mesh.vertices = verts;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			Color[] colors = new Color[verts.Length];
			for (int i = 0; i < verts.Length; i++)
				colors[i] = color;

			mesh.colors = colors;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.name = name;

			return mesh;
		}

		public class BoneWeightContainer {
			public struct Pair {
				public Transform bone;
				public float weight;

				public Pair (Transform bone, float weight) {
					this.bone = bone;
					this.weight = weight;
				}
			}

			public List<Transform> bones;
			public List<float> weights;
			public List<Pair> pairs;


			public BoneWeightContainer () {
				this.bones = new List<Transform>();
				this.weights = new List<float>();
				this.pairs = new List<Pair>();
			}

			public void Add (Transform transform, float weight) {
				bones.Add(transform);
				weights.Add(weight);

				pairs.Add(new Pair(transform, weight));
			}
		}

		internal static Mesh ExtractWeightedMeshAttachment (string name, MeshAttachment attachment, int slotIndex, SkeletonData skeletonData, List<Transform> boneList, Mesh mesh = null) {
			if (!attachment.IsWeighted())
				throw new System.ArgumentException("Mesh is not weighted.", "attachment");

			Skeleton skeleton = new Skeleton(skeletonData);
			skeleton.UpdateWorldTransform();

			float[] floatVerts = new float[attachment.WorldVerticesLength];
			attachment.ComputeWorldVertices(skeleton.Slots.Items[slotIndex], floatVerts);

			Vector2[] uvs = ExtractUV(attachment.UVs);
			Vector3[] verts = ExtractVerts(floatVerts);

			int[] triangles = attachment.Triangles;
			Color color = new Color(attachment.R, attachment.G, attachment.B, attachment.A);

			mesh = (mesh == null) ? new Mesh() : mesh;

			mesh.triangles = new int[0];

			mesh.vertices = verts;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			Color[] colors = new Color[verts.Length];
			for (int i = 0; i < verts.Length; i++)
				colors[i] = color;

			mesh.colors = colors;
			mesh.name = name;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			// Handle weights and binding
			var weightTable = new Dictionary<int, BoneWeightContainer>();
			var warningBuilder = new System.Text.StringBuilder();

			int[] bones = attachment.Bones;
			float[] weights = attachment.Vertices;
			for (int w = 0, v = 0, b = 0, n = bones.Length; v < n; w += 2) {

				int nn = bones[v++] + v;
				for (; v < nn; v++, b += 3) {
					Transform boneTransform = boneList[bones[v]];
					int vIndex = w / 2;
					BoneWeightContainer container;
					if (weightTable.ContainsKey(vIndex))
						container = weightTable[vIndex];
					else {
						container = new BoneWeightContainer();
						weightTable.Add(vIndex, container);
					}

					float weight = weights[b + 2];
					container.Add(boneTransform, weight);
				}
			}

			BoneWeight[] boneWeights = new BoneWeight[weightTable.Count];

			for (int i = 0; i < weightTable.Count; i++) {
				BoneWeight bw = new BoneWeight();
				var container = weightTable[i];

				var pairs = container.pairs.OrderByDescending(pair => pair.weight).ToList();

				for (int b = 0; b < pairs.Count; b++) {
					if (b > 3) {
						if (warningBuilder.Length == 0)
							warningBuilder.Insert(0, "[Weighted Mesh: " + name + "]\r\nUnity only supports 4 weight influences per vertex! The 4 strongest influences will be used.\r\n");

						warningBuilder.AppendFormat("{0} ignored on vertex {1}!\r\n", pairs[b].bone.name, i);
						continue;
					}

					int boneIndex = boneList.IndexOf(pairs[b].bone);
					float weight = pairs[b].weight;

					switch (b) {
					case 0:
						bw.boneIndex0 = boneIndex;
						bw.weight0 = weight;
						break;
					case 1:
						bw.boneIndex1 = boneIndex;
						bw.weight1 = weight;
						break;
					case 2:
						bw.boneIndex2 = boneIndex;
						bw.weight2 = weight;
						break;
					case 3:
						bw.boneIndex3 = boneIndex;
						bw.weight3 = weight;
						break;
					}
				}

				boneWeights[i] = bw;
			}

			Matrix4x4[] bindPoses = new Matrix4x4[boneList.Count];
			for (int i = 0; i < boneList.Count; i++) {
				bindPoses[i] = boneList[i].worldToLocalMatrix;
			}

			mesh.boneWeights = boneWeights;
			mesh.bindposes = bindPoses;

			string warningString = warningBuilder.ToString();
			if (warningString.Length > 0)
				Debug.LogWarning(warningString);


			return mesh;
		}

		internal static Vector2[] ExtractUV (float[] floats) {
			Vector2[] arr = new Vector2[floats.Length / 2];

			for (int i = 0; i < floats.Length; i += 2) {
				arr[i / 2] = new Vector2(floats[i], floats[i + 1]);
			}

			return arr;
		}

		internal static Vector3[] ExtractVerts (float[] floats) {
			Vector3[] arr = new Vector3[floats.Length / 2];

			for (int i = 0; i < floats.Length; i += 2) {
				arr[i / 2] = new Vector3(floats[i], floats[i + 1], 0);// *scale;
			}

			return arr;
		}
		#endregion

		#region Animation Baking
		static AnimationClip ExtractAnimation (string name, SkeletonData skeletonData, Dictionary<int, List<string>> slotLookup, bool bakeIK, SendMessageOptions eventOptions, AnimationClip clip = null) {
			var animation = skeletonData.FindAnimation(name);

			var timelines = animation.Timelines;

			if (clip == null) {
				clip = new AnimationClip();
			} else {
				clip.ClearCurves();
				AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
			}

			clip.name = name;

			Skeleton skeleton = new Skeleton(skeletonData);

			List<int> ignoreRotateTimelineIndexes = new List<int>();

			if (bakeIK) {
				foreach (IkConstraint i in skeleton.IkConstraints) {
					foreach (Bone b in i.Bones) {
						ignoreRotateTimelineIndexes.Add(b.Data.Index);
						BakeBoneConstraints(b, animation, clip);
					}
				}
			}

			foreach (Bone b in skeleton.Bones) {
				if (!b.Data.TransformMode.InheritsRotation()) {
					int index = b.Data.Index;
					if (ignoreRotateTimelineIndexes.Contains(index) == false) {
						ignoreRotateTimelineIndexes.Add(index);
						BakeBoneConstraints(b, animation, clip);
					}
				}
			}

			foreach (Timeline t in timelines) {
				skeleton.SetToSetupPose();

				if (t is ScaleTimeline) {
					ParseScaleTimeline(skeleton, (ScaleTimeline)t, clip);
				} else if (t is ScaleXTimeline) {
					ParseSingleSplitScaleTimeline(skeleton, (ScaleXTimeline)t, null, clip);
				} else if (t is ScaleYTimeline) {
					ParseSingleSplitScaleTimeline(skeleton, null, (ScaleYTimeline)t, clip);
				} else if (t is TranslateTimeline) {
					ParseTranslateTimeline(skeleton, (TranslateTimeline)t, clip);
				} else if (t is TranslateXTimeline) {
					ParseSingleSplitTranslateTimeline(skeleton, (TranslateXTimeline)t, null, clip);
				} else if (t is TranslateYTimeline) {
					ParseSingleSplitTranslateTimeline(skeleton, null, (TranslateYTimeline)t, clip);
				} else if (t is RotateTimeline) {
					//bypass any rotation keys if they're going to get baked anyway to prevent localEulerAngles vs Baked collision
					if (ignoreRotateTimelineIndexes.Contains(((RotateTimeline)t).BoneIndex) == false)
						ParseRotateTimeline(skeleton, (RotateTimeline)t, clip);
				} else if (t is AttachmentTimeline) {
					ParseAttachmentTimeline(skeleton, (AttachmentTimeline)t, slotLookup, clip);
				} else if (t is EventTimeline) {
					ParseEventTimeline((EventTimeline)t, clip, eventOptions);
				}

			}

			var settings = AnimationUtility.GetAnimationClipSettings(clip);
			settings.loopTime = true;
			settings.stopTime = Mathf.Max(clip.length, 0.001f);

			SetAnimationSettings(clip, settings);

			clip.EnsureQuaternionContinuity();

			EditorUtility.SetDirty(clip);

			return clip;
		}

		internal static int Search (float[] frames, float time) {
			int n = frames.Length;
			for (int i = 1; i < n; i++)
				if (frames[i] > time) return i - 1;
			return n - 1;
		}

		static void BakeBoneConstraints (Bone bone, Spine.Animation animation, AnimationClip clip) {
			Skeleton skeleton = bone.Skeleton;
			bool inheritRotation = bone.Data.TransformMode.InheritsRotation();

			animation.Apply(skeleton, 0, 0, false, null, 1f, MixBlend.Setup, MixDirection.In);
			skeleton.UpdateWorldTransform();
			float duration = animation.Duration;

			AnimationCurve curve = new AnimationCurve();

			List<Keyframe> keys = new List<Keyframe>();

			float rotation = bone.AppliedRotation;
			if (!inheritRotation)
				rotation = GetUninheritedAppliedRotation(bone);

			keys.Add(new Keyframe(0, rotation, 0, 0));

			int listIndex = 1;

			float r = rotation;

			int steps = Mathf.CeilToInt(duration / BakeIncrement);

			float currentTime = 0;
			float angle = rotation;

			for (int i = 1; i <= steps; i++) {
				currentTime += BakeIncrement;
				if (i == steps)
					currentTime = duration;

				animation.Apply(skeleton, 0, currentTime, true, null, 1f, MixBlend.Setup, MixDirection.In);
				skeleton.UpdateWorldTransform();

				int pIndex = listIndex - 1;

				Keyframe pk = keys[pIndex];

				pk = keys[pIndex];

				rotation = inheritRotation ? bone.AppliedRotation : GetUninheritedAppliedRotation(bone);

				angle += Mathf.DeltaAngle(angle, rotation);

				r = angle;

				float rOut = (r - pk.value) / (currentTime - pk.time);

				pk.outTangent = rOut;

				keys.Add(new Keyframe(currentTime, r, rOut, 0));

				keys[pIndex] = pk;

				listIndex++;
			}

			curve = EnsureCurveKeyCount(new AnimationCurve(keys.ToArray()));

			string path = GetPath(bone.Data);
			string propertyName = "localEulerAnglesBaked";

			EditorCurveBinding xBind = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName + ".x");
			AnimationUtility.SetEditorCurve(clip, xBind, new AnimationCurve());
			EditorCurveBinding yBind = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName + ".y");
			AnimationUtility.SetEditorCurve(clip, yBind, new AnimationCurve());
			EditorCurveBinding zBind = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName + ".z");
			AnimationUtility.SetEditorCurve(clip, zBind, curve);
		}

		static void ParseTranslateTimeline (Skeleton skeleton, TranslateTimeline timeline, AnimationClip clip) {
			var boneData = skeleton.Data.Bones.Items[timeline.BoneIndex];
			var bone = skeleton.Bones.Items[timeline.BoneIndex];

			AnimationCurve xCurve = new AnimationCurve();
			AnimationCurve yCurve = new AnimationCurve();
			AnimationCurve zCurve = new AnimationCurve();

			float endTime = timeline.Frames[(timeline.FrameCount * TranslateTimeline.ENTRIES) - TranslateTimeline.ENTRIES];

			float currentTime = timeline.Frames[0];

			List<Keyframe> xKeys = new List<Keyframe>();
			List<Keyframe> yKeys = new List<Keyframe>();

			xKeys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[1] + boneData.X, 0, 0));
			yKeys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[2] + boneData.Y, 0, 0));

			int listIndex = 1;
			int frameIndex = 1;
			int f = TranslateTimeline.ENTRIES;
			float[] frames = timeline.Frames;
			skeleton.SetToSetupPose();
			float lastTime = 0;
			while (currentTime < endTime) {
				int pIndex = listIndex - 1;

				float curveType = timeline.GetCurveType(frameIndex - 1);
				if (curveType == 0) {
					//linear
					Keyframe px = xKeys[pIndex];
					Keyframe py = yKeys[pIndex];

					float time = frames[f];
					float x = frames[f + 1] + boneData.X;
					float y = frames[f + 2] + boneData.Y;

					float xOut = (x - px.value) / (time - px.time);
					float yOut = (y - py.value) / (time - py.time);

					px.outTangent = xOut;
					py.outTangent = yOut;

					xKeys.Add(new Keyframe(time, x, xOut, 0));
					yKeys.Add(new Keyframe(time, y, yOut, 0));

					xKeys[pIndex] = px;
					yKeys[pIndex] = py;

					currentTime = time;

					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else if (curveType == 1) {
					//stepped
					Keyframe px = xKeys[pIndex];
					Keyframe py = yKeys[pIndex];

					float time = frames[f];
					float x = frames[f + 1] + boneData.X;
					float y = frames[f + 2] + boneData.Y;

					float xOut = float.PositiveInfinity;
					float yOut = float.PositiveInfinity;

					px.outTangent = xOut;
					py.outTangent = yOut;

					xKeys.Add(new Keyframe(time, x, xOut, 0));
					yKeys.Add(new Keyframe(time, y, yOut, 0));

					xKeys[pIndex] = px;
					yKeys[pIndex] = py;

					currentTime = time;

					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else {
					//bezier
					Keyframe px = xKeys[pIndex];
					Keyframe py = yKeys[pIndex];

					float time = frames[f];

					int steps = Mathf.FloorToInt((time - px.time) / BakeIncrement);

					for (int i = 1; i <= steps; i++) {
						currentTime += BakeIncrement;
						if (i == steps)
							currentTime = time;

						timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

						px = xKeys[listIndex - 1];
						py = yKeys[listIndex - 1];

						float xOut = (bone.X - px.value) / (currentTime - px.time);
						float yOut = (bone.Y - py.value) / (currentTime - py.time);

						px.outTangent = xOut;
						py.outTangent = yOut;

						xKeys.Add(new Keyframe(currentTime, bone.X, xOut, 0));
						yKeys.Add(new Keyframe(currentTime, bone.Y, yOut, 0));

						xKeys[listIndex - 1] = px;
						yKeys[listIndex - 1] = py;

						listIndex++;
						lastTime = currentTime;
					}
				}

				frameIndex++;
				f += TranslateTimeline.ENTRIES;
			}

			xCurve = EnsureCurveKeyCount(new AnimationCurve(xKeys.ToArray()));
			yCurve = EnsureCurveKeyCount(new AnimationCurve(yKeys.ToArray()));



			string path = GetPath(boneData);
			const string propertyName = "localPosition";

			clip.SetCurve(path, typeof(Transform), propertyName + ".x", xCurve);
			clip.SetCurve(path, typeof(Transform), propertyName + ".y", yCurve);
			clip.SetCurve(path, typeof(Transform), propertyName + ".z", zCurve);
		}

		/// <summary>Parses a single TranslateXTimeline or TranslateYTimeline.
		/// Only one of <c>timelineX</c> or <c>timelineY</c> shall be filled out, the other must be null.</summary>
		static void ParseSingleSplitTranslateTimeline (Skeleton skeleton,
			TranslateXTimeline timelineX, TranslateYTimeline timelineY, AnimationClip clip) {

			bool isXTimeline = timelineX != null;
			CurveTimeline1 timeline = isXTimeline ? timelineX : timelineY as CurveTimeline1;
			IBoneTimeline boneTimeline = isXTimeline ? timelineX : timelineY as IBoneTimeline;

			var boneData = skeleton.Data.Bones.Items[boneTimeline.BoneIndex];
			var bone = skeleton.Bones.Items[boneTimeline.BoneIndex];
			float boneDataOffset = isXTimeline ? boneData.X : boneData.Y;

			AnimationCurve curve = new AnimationCurve();
			float endTime = timeline.Frames[(timeline.FrameCount * TranslateXTimeline.ENTRIES) - TranslateXTimeline.ENTRIES];
			float currentTime = timeline.Frames[0];
			List<Keyframe> keys = new List<Keyframe>();
			keys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[1] + boneDataOffset, 0, 0));

			int listIndex = 1;
			int frameIndex = 1;
			int f = TranslateXTimeline.ENTRIES;
			float[] frames = timeline.Frames;
			skeleton.SetToSetupPose();
			float lastTime = 0;
			while (currentTime < endTime) {
				int pIndex = listIndex - 1;

				float curveType = timeline.GetCurveType(frameIndex - 1);
				if (curveType == 0) {
					//linear
					Keyframe p = keys[pIndex];

					float time = frames[f];
					float value = frames[f + 1] + boneDataOffset;
					float valueOut = (value - p.value) / (time - p.time);
					p.outTangent = valueOut;
					keys.Add(new Keyframe(time, value, valueOut, 0));

					keys[pIndex] = p;
					currentTime = time;
					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else if (curveType == 1) {
					//stepped
					Keyframe p = keys[pIndex];

					float time = frames[f];
					float value = frames[f + 1] + boneDataOffset;
					float valueOut = float.PositiveInfinity;
					p.outTangent = valueOut;
					keys.Add(new Keyframe(time, value, valueOut, 0));

					keys[pIndex] = p;
					currentTime = time;
					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else {
					//bezier
					Keyframe p = keys[pIndex];

					float time = frames[f];

					int steps = Mathf.FloorToInt((time - p.time) / BakeIncrement);

					for (int i = 1; i <= steps; i++) {
						currentTime += BakeIncrement;
						if (i == steps)
							currentTime = time;

						timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

						p = keys[listIndex - 1];
						float boneOffset = isXTimeline ? bone.X : bone.Y;
						float valueOut = (boneOffset - p.value) / (currentTime - p.time);
						p.outTangent = valueOut;
						keys.Add(new Keyframe(currentTime, boneOffset, valueOut, 0));

						keys[listIndex - 1] = p;

						listIndex++;
						lastTime = currentTime;
					}
				}

				frameIndex++;
				f += TranslateXTimeline.ENTRIES;
			}

			curve = EnsureCurveKeyCount(new AnimationCurve(keys.ToArray()));

			string path = GetPath(boneData);
			const string propertyName = "localPosition";

			clip.SetCurve(path, typeof(Transform), propertyName + (isXTimeline ? ".x" : ".y"), curve);
		}

		static void ParseScaleTimeline (Skeleton skeleton, ScaleTimeline timeline, AnimationClip clip) {
			var boneData = skeleton.Data.Bones.Items[timeline.BoneIndex];
			var bone = skeleton.Bones.Items[timeline.BoneIndex];

			AnimationCurve xCurve = new AnimationCurve();
			AnimationCurve yCurve = new AnimationCurve();
			AnimationCurve zCurve = new AnimationCurve();

			float endTime = timeline.Frames[(timeline.FrameCount * ScaleTimeline.ENTRIES) - ScaleTimeline.ENTRIES];

			float currentTime = timeline.Frames[0];

			List<Keyframe> xKeys = new List<Keyframe>();
			List<Keyframe> yKeys = new List<Keyframe>();

			xKeys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[1] * boneData.ScaleX, 0, 0));
			yKeys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[2] * boneData.ScaleY, 0, 0));

			int listIndex = 1;
			int frameIndex = 1;
			int f = ScaleTimeline.ENTRIES;
			float[] frames = timeline.Frames;
			skeleton.SetToSetupPose();
			float lastTime = 0;
			while (currentTime < endTime) {
				int pIndex = listIndex - 1;
				float curveType = timeline.GetCurveType(frameIndex - 1);
				if (curveType == 0) {
					//linear
					Keyframe px = xKeys[pIndex];
					Keyframe py = yKeys[pIndex];

					float time = frames[f];
					float x = frames[f + 1] * boneData.ScaleX;
					float y = frames[f + 2] * boneData.ScaleY;

					float xOut = (x - px.value) / (time - px.time);
					float yOut = (y - py.value) / (time - py.time);

					px.outTangent = xOut;
					py.outTangent = yOut;

					xKeys.Add(new Keyframe(time, x, xOut, 0));
					yKeys.Add(new Keyframe(time, y, yOut, 0));

					xKeys[pIndex] = px;
					yKeys[pIndex] = py;

					currentTime = time;

					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else if (curveType == 1) {
					//stepped
					Keyframe px = xKeys[pIndex];
					Keyframe py = yKeys[pIndex];

					float time = frames[f];
					float x = frames[f + 1] * boneData.ScaleX;
					float y = frames[f + 2] * boneData.ScaleY;

					float xOut = float.PositiveInfinity;
					float yOut = float.PositiveInfinity;

					px.outTangent = xOut;
					py.outTangent = yOut;

					xKeys.Add(new Keyframe(time, x, xOut, 0));
					yKeys.Add(new Keyframe(time, y, yOut, 0));

					xKeys[pIndex] = px;
					yKeys[pIndex] = py;

					currentTime = time;

					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else {
					//bezier
					Keyframe px = xKeys[pIndex];
					Keyframe py = yKeys[pIndex];

					float time = frames[f];

					int steps = Mathf.FloorToInt((time - px.time) / BakeIncrement);

					for (int i = 1; i <= steps; i++) {
						currentTime += BakeIncrement;
						if (i == steps)
							currentTime = time;

						timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

						px = xKeys[listIndex - 1];
						py = yKeys[listIndex - 1];

						float xOut = (bone.ScaleX - px.value) / (currentTime - px.time);
						float yOut = (bone.ScaleY - py.value) / (currentTime - py.time);

						px.outTangent = xOut;
						py.outTangent = yOut;

						xKeys.Add(new Keyframe(currentTime, bone.ScaleX, xOut, 0));
						yKeys.Add(new Keyframe(currentTime, bone.ScaleY, yOut, 0));

						xKeys[listIndex - 1] = px;
						yKeys[listIndex - 1] = py;

						listIndex++;
						lastTime = currentTime;
					}
				}

				frameIndex++;
				f += ScaleTimeline.ENTRIES;
			}

			xCurve = EnsureCurveKeyCount(new AnimationCurve(xKeys.ToArray()));
			yCurve = EnsureCurveKeyCount(new AnimationCurve(yKeys.ToArray()));

			string path = GetPath(boneData);
			string propertyName = "localScale";

			clip.SetCurve(path, typeof(Transform), propertyName + ".x", xCurve);
			clip.SetCurve(path, typeof(Transform), propertyName + ".y", yCurve);
			clip.SetCurve(path, typeof(Transform), propertyName + ".z", zCurve);
		}

		static void ParseSingleSplitScaleTimeline (Skeleton skeleton,
			ScaleXTimeline timelineX, ScaleYTimeline timelineY, AnimationClip clip) {

			bool isXTimeline = timelineX != null;
			CurveTimeline1 timeline = isXTimeline ? timelineX : timelineY as CurveTimeline1;
			IBoneTimeline boneTimeline = isXTimeline ? timelineX : timelineY as IBoneTimeline;

			var boneData = skeleton.Data.Bones.Items[boneTimeline.BoneIndex];
			var bone = skeleton.Bones.Items[boneTimeline.BoneIndex];
			float boneDataOffset = isXTimeline ? boneData.ScaleX : boneData.ScaleY;

			AnimationCurve curve = new AnimationCurve();
			float endTime = timeline.Frames[(timeline.FrameCount * ScaleXTimeline.ENTRIES) - ScaleXTimeline.ENTRIES];
			float currentTime = timeline.Frames[0];
			List<Keyframe> keys = new List<Keyframe>();
			keys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[1] * boneDataOffset, 0, 0));

			int listIndex = 1;
			int frameIndex = 1;
			int f = ScaleXTimeline.ENTRIES;
			float[] frames = timeline.Frames;
			skeleton.SetToSetupPose();
			float lastTime = 0;
			while (currentTime < endTime) {
				int pIndex = listIndex - 1;
				float curveType = timeline.GetCurveType(frameIndex - 1);
				if (curveType == 0) {
					//linear
					Keyframe p = keys[pIndex];

					float time = frames[f];
					float value = frames[f + 1] * boneDataOffset;
					float valueOut = (value - p.value) / (time - p.time);
					p.outTangent = valueOut;
					keys.Add(new Keyframe(time, value, valueOut, 0));

					keys[pIndex] = p;
					currentTime = time;
					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else if (curveType == 1) {
					//stepped
					Keyframe p = keys[pIndex];

					float time = frames[f];
					float value = frames[f + 1] * boneDataOffset;
					float valueOut = float.PositiveInfinity;
					p.outTangent = valueOut;
					keys.Add(new Keyframe(time, value, valueOut, 0));

					keys[pIndex] = p;
					currentTime = time;
					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else {
					//bezier
					Keyframe p = keys[pIndex];
					float time = frames[f];
					int steps = Mathf.FloorToInt((time - p.time) / BakeIncrement);

					for (int i = 1; i <= steps; i++) {
						currentTime += BakeIncrement;
						if (i == steps)
							currentTime = time;

						timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

						p = keys[listIndex - 1];

						float boneScale = isXTimeline ? bone.ScaleX : bone.ScaleY;
						float valueOut = (boneScale - p.value) / (currentTime - p.time);
						p.outTangent = valueOut;
						keys.Add(new Keyframe(currentTime, boneScale, valueOut, 0));

						keys[listIndex - 1] = p;

						listIndex++;
						lastTime = currentTime;
					}
				}

				frameIndex++;
				f += ScaleXTimeline.ENTRIES;
			}

			curve = EnsureCurveKeyCount(new AnimationCurve(keys.ToArray()));

			string path = GetPath(boneData);
			string propertyName = "localScale";

			clip.SetCurve(path, typeof(Transform), propertyName + (isXTimeline ? ".x" : ".y"), curve);
		}

		static void ParseRotateTimeline (Skeleton skeleton, RotateTimeline timeline, AnimationClip clip) {
			var boneData = skeleton.Data.Bones.Items[timeline.BoneIndex];
			var bone = skeleton.Bones.Items[timeline.BoneIndex];

			var curve = new AnimationCurve();

			float endTime = timeline.Frames[(timeline.FrameCount * 2) - 2];

			float currentTime = timeline.Frames[0];

			var keys = new List<Keyframe>();

			float rotation = timeline.Frames[1] + boneData.Rotation;

			keys.Add(new Keyframe(timeline.Frames[0], rotation, 0, 0));

			int listIndex = 1;
			int frameIndex = 1;
			int f = 2;
			float[] frames = timeline.Frames;
			skeleton.SetToSetupPose();
			float lastTime = 0;
			float angle = rotation;
			while (currentTime < endTime) {
				int pIndex = listIndex - 1;
				float curveType = timeline.GetCurveType(frameIndex - 1);

				if (curveType == 0) {
					//linear
					Keyframe pk = keys[pIndex];

					float time = frames[f];

					rotation = frames[f + 1] + boneData.Rotation;
					angle += Mathf.DeltaAngle(angle, rotation);
					float r = angle;

					float rOut = (r - pk.value) / (time - pk.time);

					pk.outTangent = rOut;

					keys.Add(new Keyframe(time, r, rOut, 0));

					keys[pIndex] = pk;

					currentTime = time;

					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else if (curveType == 1) {
					//stepped

					Keyframe pk = keys[pIndex];

					float time = frames[f];

					rotation = frames[f + 1] + boneData.Rotation;
					angle += Mathf.DeltaAngle(angle, rotation);
					float r = angle;

					float rOut = float.PositiveInfinity;

					pk.outTangent = rOut;

					keys.Add(new Keyframe(time, r, rOut, 0));

					keys[pIndex] = pk;

					currentTime = time;

					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);

					lastTime = time;
					listIndex++;
				} else {
					//bezier
					Keyframe pk = keys[pIndex];

					float time = frames[f];

					timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);
					skeleton.UpdateWorldTransform();

					rotation = frames[f + 1] + boneData.Rotation;
					angle += Mathf.DeltaAngle(angle, rotation);
					float r = angle;

					int steps = Mathf.FloorToInt((time - pk.time) / BakeIncrement);

					for (int i = 1; i <= steps; i++) {
						currentTime += BakeIncrement;
						if (i == steps)
							currentTime = time;

						timeline.Apply(skeleton, lastTime, currentTime, null, 1, MixBlend.Setup, MixDirection.In);
						skeleton.UpdateWorldTransform();
						pk = keys[listIndex - 1];

						rotation = bone.Rotation;
						angle += Mathf.DeltaAngle(angle, rotation);
						r = angle;

						float rOut = (r - pk.value) / (currentTime - pk.time);

						pk.outTangent = rOut;

						keys.Add(new Keyframe(currentTime, r, rOut, 0));

						keys[listIndex - 1] = pk;

						listIndex++;
						lastTime = currentTime;
					}
				}

				frameIndex++;
				f += 2;
			}

			curve = EnsureCurveKeyCount(new AnimationCurve(keys.ToArray()));

			string path = GetPath(boneData);
			const string propertyName = "localEulerAnglesBaked";

			EditorCurveBinding xBind = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName + ".x");
			AnimationUtility.SetEditorCurve(clip, xBind, new AnimationCurve());
			EditorCurveBinding yBind = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName + ".y");
			AnimationUtility.SetEditorCurve(clip, yBind, new AnimationCurve());
			EditorCurveBinding zBind = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName + ".z");
			AnimationUtility.SetEditorCurve(clip, zBind, curve);
		}

		static void ParseEventTimeline (EventTimeline timeline, AnimationClip clip, SendMessageOptions eventOptions) {
			var animationEvents = new List<AnimationEvent>();
			ParseEventTimeline(ref animationEvents, timeline, eventOptions);
			AnimationUtility.SetAnimationEvents(clip, animationEvents.ToArray());
		}

		static void ParseEventTimeline (ref List<AnimationEvent> animationEvents,
			EventTimeline timeline, SendMessageOptions eventOptions) {

			float[] frames = timeline.Frames;
			var events = timeline.Events;

			for (int i = 0, n = frames.Length; i < n; i++) {
				var spineEvent = events[i];
				string eventName = spineEvent.Data.Name;
				if (SpineEditorUtilities.Preferences.mecanimEventIncludeFolderName)
					eventName = eventName.Replace("/", ""); // calls method FolderNameEventName()
				else
					eventName = eventName.Substring(eventName.LastIndexOf('/') + 1); // calls method EventName()
				var unityAnimationEvent = new AnimationEvent {
					time = frames[i],
					functionName = eventName,
					messageOptions = eventOptions,
					stringParameter = SpineEventStringId
				};

				if (!string.IsNullOrEmpty(spineEvent.String)) {
					unityAnimationEvent.stringParameter = spineEvent.String;
				} else if (spineEvent.Int != 0) {
					unityAnimationEvent.intParameter = spineEvent.Int;
				} else if (spineEvent.Float != 0) {
					unityAnimationEvent.floatParameter = spineEvent.Float;
				} // else, paramless function/Action.

				animationEvents.Add(unityAnimationEvent);
			}
		}

		static void AddPreviousUserEvents (ref List<AnimationEvent> allEvents, AnimationEvent[] previousEvents) {
			foreach (AnimationEvent previousEvent in previousEvents) {
				if (previousEvent.stringParameter == SpineEventStringId)
					continue;
				var identicalEvent = allEvents.Find(newEvent => {
					return newEvent.functionName == previousEvent.functionName &&
						Mathf.Abs(newEvent.time - previousEvent.time) < EventTimeEqualityEpsilon;
				});
				if (identicalEvent != null)
					continue;

				allEvents.Add(previousEvent);
			}
		}

		static void ParseAttachmentTimeline (Skeleton skeleton, AttachmentTimeline timeline, Dictionary<int, List<string>> slotLookup, AnimationClip clip) {
			var attachmentNames = slotLookup[timeline.SlotIndex];

			string bonePath = GetPath(skeleton.Slots.Items[timeline.SlotIndex].Bone.Data);
			string slotPath = bonePath + "/" + skeleton.Slots.Items[timeline.SlotIndex].Data.Name;

			Dictionary<string, AnimationCurve> curveTable = new Dictionary<string, AnimationCurve>();

			foreach (string str in attachmentNames) {
				curveTable.Add(str, new AnimationCurve());
			}

			float[] frames = timeline.Frames;

			if (frames[0] != 0) {
				string startingName = skeleton.Slots.Items[timeline.SlotIndex].Data.AttachmentName;
				foreach (var pair in curveTable) {
					if (startingName == "" || startingName == null) {
						pair.Value.AddKey(new Keyframe(0, 0, float.PositiveInfinity, float.PositiveInfinity));
					} else {
						if (pair.Key == startingName) {
							pair.Value.AddKey(new Keyframe(0, 1, float.PositiveInfinity, float.PositiveInfinity));
						} else {
							pair.Value.AddKey(new Keyframe(0, 0, float.PositiveInfinity, float.PositiveInfinity));
						}
					}
				}
			}

			float currentTime = timeline.Frames[0];
			float endTime = frames[frames.Length - 1];
			int f = 0;
			while (currentTime < endTime) {
				float time = frames[f];

				int frameIndex = Search(frames, time);

				string name = timeline.AttachmentNames[frameIndex];
				foreach (var pair in curveTable) {
					if (name == "") {
						pair.Value.AddKey(new Keyframe(time, 0, float.PositiveInfinity, float.PositiveInfinity));
					} else {
						if (pair.Key == name) {
							pair.Value.AddKey(new Keyframe(time, 1, float.PositiveInfinity, float.PositiveInfinity));
						} else {
							pair.Value.AddKey(new Keyframe(time, 0, float.PositiveInfinity, float.PositiveInfinity));
						}
					}
				}

				currentTime = time;
				f += 1;
			}

			foreach (var pair in curveTable) {
				string path = slotPath + "/" + pair.Key;
				string prop = "m_IsActive";

				clip.SetCurve(path, typeof(GameObject), prop, pair.Value);
			}
		}

		static AnimationCurve EnsureCurveKeyCount (AnimationCurve curve) {
			if (curve.length == 1)
				curve.AddKey(curve.keys[0].time + 0.25f, curve.keys[0].value);

			return curve;
		}

		static float GetUninheritedAppliedRotation (Bone b) {
			Bone parent = b.Parent;
			float angle = b.AppliedRotation;

			while (parent != null) {
				angle -= parent.AppliedRotation;
				parent = parent.Parent;
			}

			return angle;
		}
		#endregion
		#endregion

		#region Region Baking
		public static GameObject BakeRegion (SpineAtlasAsset atlasAsset, AtlasRegion region, bool autoSave = true) {
			atlasAsset.GetAtlas(); // Initializes atlasAsset.

			string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
			string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath).Replace('\\', '/');
			string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);
			string bakedPrefabPath = Path.Combine(bakedDirPath, AssetUtility.GetPathSafeName(region.name) + ".prefab").Replace("\\", "/");

			GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(bakedPrefabPath, typeof(GameObject));
			GameObject root;
			Mesh mesh;
			bool isNewPrefab = false;

			if (!Directory.Exists(bakedDirPath))
				Directory.CreateDirectory(bakedDirPath);

			if (prefab == null) {
				root = EditorInstantiation.NewGameObject("temp", true, typeof(MeshFilter), typeof(MeshRenderer));
#if NEW_PREFAB_SYSTEM
				prefab = PrefabUtility.SaveAsPrefabAsset(root, bakedPrefabPath);
#else
				prefab = PrefabUtility.CreatePrefab(bakedPrefabPath, root);
#endif

				isNewPrefab = true;
				Object.DestroyImmediate(root);
			}

			mesh = (Mesh)AssetDatabase.LoadAssetAtPath(bakedPrefabPath, typeof(Mesh));

			Material mat = null;
			mesh = atlasAsset.GenerateMesh(region.name, mesh, out mat);
			if (isNewPrefab) {
				AssetDatabase.AddObjectToAsset(mesh, prefab);
				prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
			}

			EditorUtility.SetDirty(mesh);
			EditorUtility.SetDirty(prefab);

			if (autoSave) {
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			prefab.GetComponent<MeshRenderer>().sharedMaterial = mat;

			return prefab;
		}
		#endregion

		static string GetPath (BoneData b) {
			return GetPathRecurse(b).Substring(1);
		}

		static string GetPathRecurse (BoneData b) {
			if (b == null) return "";
			return GetPathRecurse(b.Parent) + "/" + b.Name;
		}

		static void SetAnimationSettings (AnimationClip clip, AnimationClipSettings settings) {
			AnimationUtility.SetAnimationClipSettings(clip, settings);
		}


	}

}
