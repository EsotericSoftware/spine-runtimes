/*****************************************************************************
 * SkeletonBaker added by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
#define SPINE_SKELETON_ANIMATOR

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Spine;


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
/// MeshAttachment
/// SkinnedMeshAttachment
/// 
/// [LIMITATIONS]
/// *Inverse Kinematics & Bezier Curves are baked into the animation at 60fps and are not realtime. Use bakeIncrement constant to adjust key density if desired.
/// **Non-uniform Scale Keys  (ie:  if ScaleX and ScaleY are not equal to eachother, it will not be accurate to Spine source)
/// ***Events may only fire 1 type of data per event in Unity safely so priority to String data if present in Spine key, otherwise a Float is sent whether the Spine key was Int or Float with priority given to Int.
/// 
/// [DOES NOT SUPPORT]
/// FlipX or FlipY (Maybe one day)
/// FFD (Unity does not provide access to BlendShapes with code)
/// Color Keys (Maybe one day when Unity supports full FBX standard and provides access with code)
/// InheritScale (Never.  Unity and Spine do scaling very differently)
/// Draw Order Keyframes
/// </summary>
/// 
namespace Spine.Unity.Editor {
	public static class SkeletonBaker {

		#region SkeletonAnimator's Mecanim Clips
		#if SPINE_SKELETON_ANIMATOR
		public static void GenerateMecanimAnimationClips (SkeletonDataAsset skeletonDataAsset) {
			//skeletonDataAsset.Clear();
			var data = skeletonDataAsset.GetSkeletonData(true);
			if (data == null) {
				Debug.LogError("SkeletonData failed!", skeletonDataAsset);
				return;
			}

			string dataPath = AssetDatabase.GetAssetPath(skeletonDataAsset);
			string controllerPath = dataPath.Replace("_SkeletonData", "_Controller").Replace(".asset", ".controller");

		#if UNITY_5
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
		#else
		UnityEditorInternal.AnimatorController controller;

		if (skeletonDataAsset.controller != null) {
			controller = (UnityEditorInternal.AnimatorController)skeletonDataAsset.controller;
			controllerPath = AssetDatabase.GetAssetPath(controller);
		} else {
			if (File.Exists(controllerPath)) {
				if (EditorUtility.DisplayDialog("Controller Overwrite Warning", "Unknown Controller already exists at: " + controllerPath, "Update", "Overwrite")) {
					controller = (UnityEditorInternal.AnimatorController)AssetDatabase.LoadAssetAtPath(controllerPath, typeof(RuntimeAnimatorController));
				} else {
					controller = (UnityEditorInternal.AnimatorController)UnityEditorInternal.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
				}
			} else {
				controller = (UnityEditorInternal.AnimatorController)UnityEditorInternal.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
			}
		}
		#endif

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

			foreach (var anim in data.Animations) {
				string name = anim.Name;
				spineAnimationTable.Add(name, anim);

				if (unityAnimationClipTable.ContainsKey(name) == false) {
					//generate new dummy clip
					AnimationClip newClip = new AnimationClip();
					newClip.name = name;
					#if !(UNITY_5)
					AnimationUtility.SetAnimationType(newClip, ModelImporterAnimationType.Generic);
					#endif
					AssetDatabase.AddObjectToAsset(newClip, controller);
					unityAnimationClipTable.Add(name, newClip);
				}

				AnimationClip clip = unityAnimationClipTable[name];

				clip.SetCurve("", typeof(GameObject), "dummy", AnimationCurve.Linear(0, 0, anim.Duration, 0));
				var settings = AnimationUtility.GetAnimationClipSettings(clip);
				settings.stopTime = anim.Duration;

				SetAnimationSettings(clip, settings);

				AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);

				foreach (Timeline t in anim.Timelines) {
					if (t is EventTimeline) {
						ParseEventTimeline((EventTimeline)t, clip, SendMessageOptions.DontRequireReceiver);
					}
				}

				EditorUtility.SetDirty(clip);

				unityAnimationClipTable.Remove(name);
			}

			//clear no longer used animations
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

		#region Baking
		/// <summary>
		/// Interval between key sampling for Bezier curves, IK controlled bones, and Inherit Rotation effected bones.
		/// </summary>
		const float bakeIncrement = 1 / 60f;

		public static void BakeToPrefab (SkeletonDataAsset skeletonDataAsset, ExposedList<Skin> skins, string outputPath = "", bool bakeAnimations = true, bool bakeIK = true, SendMessageOptions eventOptions = SendMessageOptions.DontRequireReceiver) {
			if (skeletonDataAsset == null || skeletonDataAsset.GetSkeletonData(true) == null) {
				Debug.LogError("Could not export Spine Skeleton because SkeletonDataAsset is null or invalid!");
				return;
			}

			if (outputPath == "") {
				outputPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(skeletonDataAsset)) + "/Baked";
				System.IO.Directory.CreateDirectory(outputPath);
			}

			var skeletonData = skeletonDataAsset.GetSkeletonData(true);
			bool hasAnimations = bakeAnimations && skeletonData.Animations.Count > 0;
			#if UNITY_5
			UnityEditor.Animations.AnimatorController controller = null;
			#else
			UnityEditorInternal.AnimatorController controller = null;
			#endif
			if (hasAnimations) {
				string controllerPath = outputPath + "/" + skeletonDataAsset.skeletonJSON.name + " Controller.controller";
				bool newAnimContainer = false;

				var runtimeController = AssetDatabase.LoadAssetAtPath(controllerPath, typeof(RuntimeAnimatorController));

				#if UNITY_5
				if (runtimeController != null) {
					controller = (UnityEditor.Animations.AnimatorController)runtimeController;
				} else {
					controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
					newAnimContainer = true;
				}
				#else
				if (runtimeController != null) {
				controller = (UnityEditorInternal.AnimatorController)runtimeController;
				} else {
				controller = UnityEditorInternal.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
				newAnimContainer = true;
				}
				#endif

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
						List<string> temp = new List<string>();
						skin.FindNamesForSlot(s, temp);
						foreach (string str in temp) {
							if (!attachmentNames.Contains(str))
								attachmentNames.Add(str);
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
						#if UNITY_5
						controller.AddMotion(clip);
						#else
						UnityEditorInternal.AnimatorController.AddAnimationClipToController(controller, clip);
						#endif

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
					prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
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

				GameObject prefabRoot = new GameObject("root");

				Dictionary<string, Transform> slotTable = new Dictionary<string, Transform>();
				Dictionary<string, Transform> boneTable = new Dictionary<string, Transform>();
				List<Transform> boneList = new List<Transform>();

				//create bones
				for (int i = 0; i < skeletonData.Bones.Count; i++) {
					var boneData = skeletonData.Bones.Items[i];
					Transform boneTransform = new GameObject(boneData.Name).transform;
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
					if (boneData.InheritRotation)
						boneTransform.localRotation = Quaternion.Euler(0, 0, boneData.Rotation);
					else
						boneTransform.rotation = Quaternion.Euler(0, 0, boneData.Rotation);

					if (boneData.InheritScale)
						boneTransform.localScale = new Vector3(boneData.ScaleX, boneData.ScaleY, 1);
				}

				//create slots and attachments
				for (int i = 0; i < skeletonData.Slots.Count; i++) {
					var slotData = skeletonData.Slots.Items[i];
					Transform slotTransform = new GameObject(slotData.Name).transform;
					slotTransform.parent = prefabRoot.transform;
					slotTable.Add(slotData.Name, slotTransform);

					List<Attachment> attachments = new List<Attachment>();
					List<string> attachmentNames = new List<string>();

					skin.FindAttachmentsForSlot(i, attachments);
					skin.FindNamesForSlot(i, attachmentNames);

					if (skin != skeletonData.DefaultSkin) {
						skeletonData.DefaultSkin.FindAttachmentsForSlot(i, attachments);
						skeletonData.DefaultSkin.FindNamesForSlot(i, attachmentNames);
					}

					for (int a = 0; a < attachments.Count; a++) {
						var attachment = attachments[a];
						var attachmentName = attachmentNames[a];
						var attachmentMeshName = "[" + slotData.Name + "] " + attachmentName;
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
							material = ExtractMaterial(attachment);
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
								mesh = ExtractWeightedMeshAttachment(attachmentMeshName, meshAttachment, i, skeletonData, boneList, mesh);
							else
								mesh = ExtractMeshAttachment(attachmentMeshName, meshAttachment, mesh);
							
							material = ExtractMaterial(attachment);
							unusedMeshNames.Remove(attachmentMeshName);
							if (newPrefab || meshTable.ContainsKey(attachmentMeshName) == false)
								AssetDatabase.AddObjectToAsset(mesh, prefab);
						} else
							continue;

						Transform attachmentTransform = new GameObject(attachmentName).transform;

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
						attachmentTransform.GetComponent<Renderer>().sortingOrder = i;

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
					PrefabUtility.ReplacePrefab(prefabRoot, prefab, ReplacePrefabOptions.ConnectToPrefab);
				} else {

					foreach (string str in unusedMeshNames) {
						Mesh.DestroyImmediate(meshTable[str], true);
					}

					PrefabUtility.ReplacePrefab(prefabRoot, prefab, ReplacePrefabOptions.ReplaceNameBased);
				}

				EditorGUIUtility.PingObject(prefab);

				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();

				GameObject.DestroyImmediate(prefabRoot);
			}

		}

		static Bone extractionBone;
		static Slot extractionSlot;

		static Bone GetExtractionBone () {
			if (extractionBone != null)
				return extractionBone;

			SkeletonData skelData = new SkeletonData();
			BoneData data = new BoneData(0, "temp", null);
			data.ScaleX = 1;
			data.ScaleY = 1;
			data.Length = 100;

			skelData.Bones.Add(data);

			Skeleton skeleton = new Skeleton(skelData);

			Bone bone = new Bone(data, skeleton, null);
			bone.UpdateWorldTransform();

			extractionBone = bone;

			return extractionBone;
		}

		static Slot GetExtractionSlot () {
			if (extractionSlot != null)
				return extractionSlot;

			Bone bone = GetExtractionBone();

			SlotData data = new SlotData(0, "temp", bone.Data);
			Slot slot = new Slot(data, bone);
			extractionSlot = slot;
			return extractionSlot;
		}

		static Material ExtractMaterial (Attachment attachment) {
			if (attachment == null || attachment is BoundingBoxAttachment)
				return null;

			if (attachment is RegionAttachment) {
				var att = (RegionAttachment)attachment;
				return (Material)((AtlasRegion)att.RendererObject).page.rendererObject;
			} else if (attachment is MeshAttachment) {
				var att = (MeshAttachment)attachment;
				return (Material)((AtlasRegion)att.RendererObject).page.rendererObject;
			} else {
				return null;
			}
		}

		static Mesh ExtractRegionAttachment (string name, RegionAttachment attachment, Mesh mesh = null) {
			var bone = GetExtractionBone();

			bone.X = -attachment.X;
			bone.Y = -attachment.Y;
			bone.UpdateWorldTransform();

			Vector2[] uvs = ExtractUV(attachment.UVs);
			float[] floatVerts = new float[8];
			attachment.ComputeWorldVertices(bone, floatVerts);
			Vector3[] verts = ExtractVerts(floatVerts);

			//unrotate verts now that they're centered
			for (int i = 0; i < verts.Length; i++) {
				verts[i] = Quaternion.Euler(0, 0, -attachment.Rotation) * verts[i];
			}

			int[] triangles = new int[6] { 1, 3, 0, 2, 3, 1 };
			Color color = new Color(attachment.R, attachment.G, attachment.B, attachment.A);

			if (mesh == null)
				mesh = new Mesh();

			mesh.triangles = new int[0];

			mesh.vertices = verts;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.colors = new Color[] { color, color, color, color };
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.name = name;

			return mesh;
		}

		static Mesh ExtractMeshAttachment (string name, MeshAttachment attachment, Mesh mesh = null) {
			var slot = GetExtractionSlot();

			slot.Bone.X = 0;
			slot.Bone.Y = 0;
			slot.Bone.UpdateWorldTransform();

			Vector2[] uvs = ExtractUV(attachment.UVs);
			float[] floatVerts = new float[attachment.WorldVerticesLength];
			attachment.ComputeWorldVertices(slot, floatVerts);
			Vector3[] verts = ExtractVerts(floatVerts);

			int[] triangles = attachment.Triangles;
			Color color = new Color(attachment.R, attachment.G, attachment.B, attachment.A);

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

		static Mesh ExtractWeightedMeshAttachment (string name, MeshAttachment attachment, int slotIndex, SkeletonData skeletonData, List<Transform> boneList, Mesh mesh = null) {
			if (attachment.Bones == null)
				throw new System.ArgumentException("Mesh is not weighted.", "attachment");

			Skeleton skeleton = new Skeleton(skeletonData);
			skeleton.UpdateWorldTransform();

			float[] floatVerts = new float[attachment.WorldVerticesLength];
			attachment.ComputeWorldVertices(skeleton.Slots.Items[slotIndex], floatVerts);

			Vector2[] uvs = ExtractUV(attachment.UVs);
			Vector3[] verts = ExtractVerts(floatVerts);

			int[] triangles = attachment.Triangles;
			Color color = new Color(attachment.R, attachment.G, attachment.B, attachment.A);

			mesh = mesh ?? new Mesh();

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
							warningBuilder.Insert(0, "[Weighted Mesh: " + name + "]\r\nUnity only supports 4 weight influences per vertex!  The 4 strongest influences will be used.\r\n");

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

		static Vector2[] ExtractUV (float[] floats) {
			Vector2[] arr = new Vector2[floats.Length / 2];

			for (int i = 0; i < floats.Length; i += 2) {
				arr[i / 2] = new Vector2(floats[i], floats[i + 1]);
			}

			return arr;
		}

		static Vector3[] ExtractVerts (float[] floats) {
			Vector3[] arr = new Vector3[floats.Length / 2];

			for (int i = 0; i < floats.Length; i += 2) {
				arr[i / 2] = new Vector3(floats[i], floats[i + 1], 0);// *scale;
			}

			return arr;
		}

		static AnimationClip ExtractAnimation (string name, SkeletonData skeletonData, Dictionary<int, List<string>> slotLookup, bool bakeIK, SendMessageOptions eventOptions, AnimationClip clip = null) {
			var animation = skeletonData.FindAnimation(name);

			var timelines = animation.Timelines;

			if (clip == null) {
				clip = new AnimationClip();
			} else {
				clip.ClearCurves();
				AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
			}

			#if UNITY_5

			#else
			AnimationUtility.SetAnimationType(clip, ModelImporterAnimationType.Generic);
			#endif

			clip.name = name;

			Skeleton skeleton = new Skeleton(skeletonData);

			List<int> ignoreRotateTimelineIndexes = new List<int>();

			if (bakeIK) {
				foreach (IkConstraint i in skeleton.IkConstraints) {
					foreach (Bone b in i.Bones) {
						int index = skeleton.FindBoneIndex(b.Data.Name);
						ignoreRotateTimelineIndexes.Add(index);
						BakeBone(b, animation, clip);
					}
				}
			}

			foreach (Bone b in skeleton.Bones) {
				if (b.Data.InheritRotation == false) {
					int index = skeleton.FindBoneIndex(b.Data.Name);

					if (ignoreRotateTimelineIndexes.Contains(index) == false) {
						ignoreRotateTimelineIndexes.Add(index);
						BakeBone(b, animation, clip);
					}
				}
			}

			foreach (Timeline t in timelines) {
				skeleton.SetToSetupPose();

				if (t is ScaleTimeline) {
					ParseScaleTimeline(skeleton, (ScaleTimeline)t, clip);
				} else if (t is TranslateTimeline) {
					ParseTranslateTimeline(skeleton, (TranslateTimeline)t, clip);
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

		static int BinarySearch (float[] values, float target) {
			int low = 0;
			int high = values.Length - 2;
			if (high == 0) return 1;
			int current = (int)((uint)high >> 1);
			while (true) {
				if (values[(current + 1)] <= target)
					low = current + 1;
				else
					high = current;
				if (low == high) return (low + 1);
				current = (int)((uint)(low + high) >> 1);
			}
		}

		static void ParseEventTimeline (EventTimeline timeline, AnimationClip clip, SendMessageOptions eventOptions) {

			float[] frames = timeline.Frames;
			var events = timeline.Events;

			List<AnimationEvent> animEvents = new List<AnimationEvent>();
			for (int i = 0; i < frames.Length; i++) {
				var ev = events[i];

				AnimationEvent ae = new AnimationEvent();
				//MITCH: left todo:  Deal with Mecanim's zero-time missed event
				ae.time = frames[i];
				ae.functionName = ev.Data.Name;
				ae.messageOptions = eventOptions;

				if (!string.IsNullOrEmpty(ev.String)) {
					ae.stringParameter = ev.String;
				} else {
					if (ev.Int == 0 && ev.Float == 0) {
						//do nothing, raw function
					} else {
						if (ev.Int != 0)
							ae.floatParameter = (float)ev.Int;
						else
							ae.floatParameter = ev.Float;
					}

				}

				animEvents.Add(ae);
			}

			AnimationUtility.SetAnimationEvents(clip, animEvents.ToArray());
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

				int frameIndex = (time >= frames[frames.Length - 1] ? frames.Length : BinarySearch(frames, time)) - 1;

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

		static float GetUninheritedRotation (Bone b) {

			Bone parent = b.Parent;
			float angle = b.AppliedRotation;

			while (parent != null) {
				angle -= parent.AppliedRotation;
				parent = parent.Parent;
			}

			return angle;
		}

		static void BakeBone (Bone bone, Spine.Animation animation, AnimationClip clip) {
			Skeleton skeleton = bone.Skeleton;
			bool inheritRotation = bone.Data.InheritRotation;

			skeleton.SetToSetupPose();
			animation.Apply(skeleton, 0, 0, true, null);
			skeleton.UpdateWorldTransform();
			float duration = animation.Duration;

			AnimationCurve curve = new AnimationCurve();

			List<Keyframe> keys = new List<Keyframe>();

			float rotation = bone.AppliedRotation;
			if (!inheritRotation)
				rotation = GetUninheritedRotation(bone);

			keys.Add(new Keyframe(0, rotation, 0, 0));

			int listIndex = 1;

			float r = rotation;

			int steps = Mathf.CeilToInt(duration / bakeIncrement);

			float currentTime = 0;
			float lastTime = 0;
			float angle = rotation;

			for (int i = 1; i <= steps; i++) {
				currentTime += bakeIncrement;
				if (i == steps)
					currentTime = duration;

				animation.Apply(skeleton, lastTime, currentTime, true, null);
				skeleton.UpdateWorldTransform();

				int pIndex = listIndex - 1;

				Keyframe pk = keys[pIndex];

				pk = keys[pIndex];

				if (inheritRotation)
					rotation = bone.AppliedRotation;
				else {
					rotation = GetUninheritedRotation(bone);
				}

				angle += Mathf.DeltaAngle(angle, rotation);

				r = angle;

				float rOut = (r - pk.value) / (currentTime - pk.time);

				pk.outTangent = rOut;

				keys.Add(new Keyframe(currentTime, r, rOut, 0));

				keys[pIndex] = pk;

				listIndex++;
				lastTime = currentTime;
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

			float endTime = timeline.Frames[(timeline.FrameCount * 3) - 3];

			float currentTime = timeline.Frames[0];

			List<Keyframe> xKeys = new List<Keyframe>();
			List<Keyframe> yKeys = new List<Keyframe>();

			xKeys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[1] + boneData.X, 0, 0));
			yKeys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[2] + boneData.Y, 0, 0));

			int listIndex = 1;
			int frameIndex = 1;
			int f = 3;
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

					timeline.Apply(skeleton, lastTime, currentTime, null, 1);

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

					timeline.Apply(skeleton, lastTime, currentTime, null, 1);

					lastTime = time;
					listIndex++;
				} else if (curveType == 2) {

					//bezier
					Keyframe px = xKeys[pIndex];
					Keyframe py = yKeys[pIndex];

					float time = frames[f];

					int steps = Mathf.FloorToInt((time - px.time) / bakeIncrement);

					for (int i = 1; i <= steps; i++) {
						currentTime += bakeIncrement;
						if (i == steps)
							currentTime = time;

						timeline.Apply(skeleton, lastTime, currentTime, null, 1);

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
				f += 3;
			}

			xCurve = EnsureCurveKeyCount(new AnimationCurve(xKeys.ToArray()));
			yCurve = EnsureCurveKeyCount(new AnimationCurve(yKeys.ToArray()));



			string path = GetPath(boneData);
			const string propertyName = "localPosition";

			clip.SetCurve(path, typeof(Transform), propertyName + ".x", xCurve);
			clip.SetCurve(path, typeof(Transform), propertyName + ".y", yCurve);
			clip.SetCurve(path, typeof(Transform), propertyName + ".z", zCurve);
		}

		static AnimationCurve EnsureCurveKeyCount (AnimationCurve curve) {
			if (curve.length == 1)
				curve.AddKey(curve.keys[0].time + 0.25f, curve.keys[0].value);

			return curve;
		}

		static void ParseScaleTimeline (Skeleton skeleton, ScaleTimeline timeline, AnimationClip clip) {
			var boneData = skeleton.Data.Bones.Items[timeline.BoneIndex];
			var bone = skeleton.Bones.Items[timeline.BoneIndex];

			AnimationCurve xCurve = new AnimationCurve();
			AnimationCurve yCurve = new AnimationCurve();
			AnimationCurve zCurve = new AnimationCurve();

			float endTime = timeline.Frames[(timeline.FrameCount * 3) - 3];

			float currentTime = timeline.Frames[0];

			List<Keyframe> xKeys = new List<Keyframe>();
			List<Keyframe> yKeys = new List<Keyframe>();

			xKeys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[1] * boneData.ScaleX, 0, 0));
			yKeys.Add(new Keyframe(timeline.Frames[0], timeline.Frames[2] * boneData.ScaleY, 0, 0));

			int listIndex = 1;
			int frameIndex = 1;
			int f = 3;
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

					timeline.Apply(skeleton, lastTime, currentTime, null, 1);

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

					timeline.Apply(skeleton, lastTime, currentTime, null, 1);

					lastTime = time;
					listIndex++;
				} else if (curveType == 2) {
					//bezier
					Keyframe px = xKeys[pIndex];
					Keyframe py = yKeys[pIndex];

					float time = frames[f];

					int steps = Mathf.FloorToInt((time - px.time) / bakeIncrement);

					for (int i = 1; i <= steps; i++) {
						currentTime += bakeIncrement;
						if (i == steps)
							currentTime = time;

						timeline.Apply(skeleton, lastTime, currentTime, null, 1);

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
				f += 3;
			}

			xCurve = EnsureCurveKeyCount(new AnimationCurve(xKeys.ToArray()));
			yCurve = EnsureCurveKeyCount(new AnimationCurve(yKeys.ToArray()));

			string path = GetPath(boneData);
			string propertyName = "localScale";

			clip.SetCurve(path, typeof(Transform), propertyName + ".x", xCurve);
			clip.SetCurve(path, typeof(Transform), propertyName + ".y", yCurve);
			clip.SetCurve(path, typeof(Transform), propertyName + ".z", zCurve);
		}

		static void ParseRotateTimeline (Skeleton skeleton, RotateTimeline timeline, AnimationClip clip) {
			var boneData = skeleton.Data.Bones.Items[timeline.BoneIndex];
			var bone = skeleton.Bones.Items[timeline.BoneIndex];

			AnimationCurve curve = new AnimationCurve();

			float endTime = timeline.Frames[(timeline.FrameCount * 2) - 2];

			float currentTime = timeline.Frames[0];

			List<Keyframe> keys = new List<Keyframe>();

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

					timeline.Apply(skeleton, lastTime, currentTime, null, 1);

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

					timeline.Apply(skeleton, lastTime, currentTime, null, 1);

					lastTime = time;
					listIndex++;
				} else if (curveType == 2) {
					//bezier
					Keyframe pk = keys[pIndex];

					float time = frames[f];

					timeline.Apply(skeleton, lastTime, currentTime, null, 1);
					skeleton.UpdateWorldTransform();

					rotation = frames[f + 1] + boneData.Rotation;
					angle += Mathf.DeltaAngle(angle, rotation);
					float r = angle;

					int steps = Mathf.FloorToInt((time - pk.time) / bakeIncrement);

					for (int i = 1; i <= steps; i++) {
						currentTime += bakeIncrement;
						if (i == steps)
							currentTime = time;

						timeline.Apply(skeleton, lastTime, currentTime, null, 1);
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
			string propertyName = "localEulerAnglesBaked";

			EditorCurveBinding xBind = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName + ".x");
			AnimationUtility.SetEditorCurve(clip, xBind, new AnimationCurve());
			EditorCurveBinding yBind = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName + ".y");
			AnimationUtility.SetEditorCurve(clip, yBind, new AnimationCurve());
			EditorCurveBinding zBind = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName + ".z");
			AnimationUtility.SetEditorCurve(clip, zBind, curve);
		}

		static string GetPath (BoneData b) {
			return GetPathRecurse(b).Substring(1);
		}

		static string GetPathRecurse (BoneData b) {
			if (b == null) {
				return "";
			}

			return GetPathRecurse(b.Parent) + "/" + b.Name;
		}
		#endregion

		static void SetAnimationSettings (AnimationClip clip, AnimationClipSettings settings) {
			#if UNITY_5
			AnimationUtility.SetAnimationClipSettings(clip, settings);
			#else
			MethodInfo methodInfo = typeof(AnimationUtility).GetMethod("SetAnimationClipSettings", BindingFlags.Static | BindingFlags.NonPublic);
			methodInfo.Invoke(null, new object[] { clip, settings });

			EditorUtility.SetDirty(clip);
			#endif
		}


	}

}
	
