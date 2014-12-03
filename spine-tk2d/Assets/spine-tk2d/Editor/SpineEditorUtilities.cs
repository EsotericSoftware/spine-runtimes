#pragma warning disable 0219
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

/*****************************************************************************
 * Spine Editor Utilities created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Spine;

[InitializeOnLoad]
public class SpineEditorUtilities : AssetPostprocessor {

	public static class Icons {
		public static Texture2D skeleton;
		public static Texture2D nullBone;
		public static Texture2D bone;
		public static Texture2D poseBones;
		public static Texture2D boneNib;
		public static Texture2D slot;
		public static Texture2D skinPlaceholder;
		public static Texture2D image;
		public static Texture2D boundingBox;
		public static Texture2D mesh;
		public static Texture2D skin;
		public static Texture2D skinsRoot;
		public static Texture2D animation;
		public static Texture2D animationRoot;
		public static Texture2D spine;
		public static Texture2D _event;
		public static Texture2D constraintNib;
		public static Texture2D warning;
		public static Texture2D skeletonUtility;
		public static Texture2D hingeChain;
		public static Texture2D subMeshRenderer;

		public static Mesh boneMesh {
			get {
				if (_boneMesh == null) {
					_boneMesh = new Mesh();
					_boneMesh.vertices = new Vector3[4] {
						Vector3.zero,
						new Vector3(-0.1f, 0.1f, 0),
						Vector3.up,
						new Vector3(0.1f, 0.1f, 0)
					};
					_boneMesh.uv = new Vector2[4];
					_boneMesh.triangles = new int[6]{0,1,2,2,3,0};
					_boneMesh.RecalculateBounds();
					_boneMesh.RecalculateNormals();
				}

				return _boneMesh;
			}
		}

		internal static Mesh _boneMesh;

		public static Material boneMaterial {
			get {
				if (_boneMaterial == null) {
#if UNITY_4_3
					_boneMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
					_boneMaterial.SetColor("_TintColor", new Color(0.4f, 0.4f, 0.4f, 0.25f));
#else
					_boneMaterial = new Material(Shader.Find("Spine/Bones"));
					_boneMaterial.SetColor("_Color", new Color(0.4f, 0.4f, 0.4f, 0.25f));
#endif

				}

				return _boneMaterial;
			}
		}

		internal static Material _boneMaterial;

		public static void Initialize () {
			skeleton = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skeleton.png");
			nullBone = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-null.png");
			bone = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-bone.png");
			poseBones = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-poseBones.png");
			boneNib = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-boneNib.png");
			slot = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-slot.png");
			skinPlaceholder = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skinPlaceholder.png");
			image = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-image.png");
			boundingBox = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-boundingBox.png");
			mesh = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-mesh.png");
			skin = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skinPlaceholder.png");
			skinsRoot = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skinsRoot.png");
			animation = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-animation.png");
			animationRoot = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-animationRoot.png");
			spine = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-spine.png");
			_event = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-event.png");
			constraintNib = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-constraintNib.png");
			warning = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-warning.png");
			skeletonUtility = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skeletonUtility.png");
			hingeChain = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-hingeChain.png");
			subMeshRenderer = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-subMeshRenderer.png");
		}
	}

	public static string editorPath = "";
	public static string editorGUIPath = "";
	static Dictionary<int, GameObject> skeletonRendererTable;
	static Dictionary<int, SkeletonUtilityBone> skeletonUtilityBoneTable;
	public static float defaultScale = 0.01f;
	public static float defaultMix = 0.2f;
	public static string defaultShader = "Spine/Skeleton";
	
	static SpineEditorUtilities () {
		DirectoryInfo rootDir = new DirectoryInfo(Application.dataPath);
		FileInfo[] files = rootDir.GetFiles("SpineEditorUtilities.cs", SearchOption.AllDirectories);
		editorPath = Path.GetDirectoryName(files[0].FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets"));
		editorGUIPath = editorPath + "/GUI";	

		Icons.Initialize();

		skeletonRendererTable = new Dictionary<int, GameObject>();
		skeletonUtilityBoneTable = new Dictionary<int, SkeletonUtilityBone>();

		EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
		EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;

		HierarchyWindowChanged();
	}

	static void HierarchyWindowChanged () {
		skeletonRendererTable.Clear();
		skeletonUtilityBoneTable.Clear();

		SkeletonRenderer[] arr = Object.FindObjectsOfType<SkeletonRenderer>();

		foreach (SkeletonRenderer r in arr)
			skeletonRendererTable.Add(r.gameObject.GetInstanceID(), r.gameObject);

		SkeletonUtilityBone[] boneArr = Object.FindObjectsOfType<SkeletonUtilityBone>();
		foreach (SkeletonUtilityBone b in boneArr)
			skeletonUtilityBoneTable.Add(b.gameObject.GetInstanceID(), b);
	}

	static void HierarchyWindowItemOnGUI (int instanceId, Rect selectionRect) {
		if (skeletonRendererTable.ContainsKey(instanceId)) {
			Rect r = new Rect(selectionRect); 
			r.x = r.width - 15;
			r.width = 15;

			GUI.Label(r, Icons.spine);
		} else if (skeletonUtilityBoneTable.ContainsKey(instanceId)) {
				Rect r = new Rect(selectionRect); 
				r.x -= 26;

				if (skeletonUtilityBoneTable[instanceId] != null) {
					if (skeletonUtilityBoneTable[instanceId].transform.childCount == 0)
						r.x += 13;
				
					r.y += 2;

					r.width = 13;
					r.height = 13;

					if (skeletonUtilityBoneTable[instanceId].mode == SkeletonUtilityBone.Mode.Follow) {
						GUI.DrawTexture(r, Icons.bone);
					} else {
						GUI.DrawTexture(r, Icons.poseBones);
					}
				}

			}

	}

	[MenuItem("Assets/Spine/Spawn")]
	static void SpawnAnimatedSkeleton () {
		Object[] arr = Selection.objects;
		foreach (Object o in arr) {
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
			string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

			SpawnAnimatedSkeleton((SkeletonDataAsset)o, skinName);
			SceneView.RepaintAll();
		}
	}

	[MenuItem("Assets/Spine/Spawn", true)]
	static bool ValidateSpawnAnimatedSkeleton () {
		Object[] arr = Selection.objects;
		
		if (arr.Length == 0)
			return false;
		
		foreach (Object o in arr) {
			if (o.GetType() != typeof(SkeletonDataAsset))
				return false;
		}
		
		return true;
	}

	public static SkeletonAnimation SpawnAnimatedSkeleton (SkeletonDataAsset skeletonDataAsset, string skinName) {
		SkeletonData skelData = skeletonDataAsset.GetSkeletonData(false);
		if(skelData == null){
			return null;
		}
		return SpawnAnimatedSkeleton(skeletonDataAsset, skelData.FindSkin(skinName));
	}

	public static SkeletonAnimation SpawnAnimatedSkeleton (SkeletonDataAsset skeletonDataAsset, Skin skin = null) {
		GameObject go = new GameObject(skeletonDataAsset.name.Replace("_SkeletonData", ""), typeof(MeshFilter), typeof(MeshRenderer), typeof(SkeletonAnimation));
		SkeletonAnimation anim = go.GetComponent<SkeletonAnimation>();
		anim.skeletonDataAsset = skeletonDataAsset;



		SkeletonData data = skeletonDataAsset.GetSkeletonData(false);

		if (data == null) {
			return null;
		}

		if (skin == null)
			skin = data.DefaultSkin;
			
		if (skin == null)
			skin = data.Skins[0];

		anim.Reset();
		
		anim.skeleton.SetSkin(skin);
		anim.initialSkinName = skin.Name;
		
		anim.skeleton.Update(1);
		anim.state.Update(1);
		anim.state.Apply(anim.skeleton);
		anim.skeleton.UpdateWorldTransform();

		return anim;
	}

	static bool IsSpineJSON (TextAsset asset) {
		object obj = Json.Deserialize(new StringReader(asset.text));
		if (obj == null) {
			Debug.LogError("Is not valid JSON");
			return false;
		}
		
		Dictionary<string, object> root = (Dictionary<string, object>)obj;
		
		if (!root.ContainsKey("skeleton"))
			return false;
		
		Dictionary<string, object> skeletonInfo = (Dictionary<string, object>)root["skeleton"];
		
		string spineVersion = (string)skeletonInfo["spine"];
        //TODO:  reject old versions
        
        return true;
	}

	//TK2D helpers

	[MenuItem("Assets/Create/Spine/SkeletonData From Selection", true)]
	static bool CreateSkeletonDataFromSelectionValidate(){
		int spineJsonCount = 0;
		int collectionCount = 0;

		foreach(Object obj in Selection.objects){
			if(obj is TextAsset){
				TextAsset t = obj as TextAsset;
				if(IsSpineJSON(t))
					spineJsonCount++;
			}
			else if(obj is GameObject){
				GameObject go = obj as GameObject;
				var spriteCollection = go.GetComponent<tk2dSpriteCollection>();
				if(spriteCollection != null){
					if(spriteCollection.spriteCollection != null){
						collectionCount++;
						if(collectionCount > 1){
							Debug.LogWarning("SkeletonData From Selection only works when 1 Collection is selected.");
							return false;
						}
					}
				}
			}
		}

		if(spineJsonCount > 0 && collectionCount == 1){
			return true;
		}

		return false;
	}

	[MenuItem("Assets/Create/Spine/SkeletonData From Selection")]
	static void CreateSkeletonDataFromSelection(){

		List<TextAsset> jsonList = new List<TextAsset>();
		tk2dSpriteCollectionData collectionData = null;

		foreach(Object obj in Selection.objects){
			if(obj is TextAsset){
				TextAsset t = obj as TextAsset;
				if(IsSpineJSON(t))
					jsonList.Add(t);
			}
			else if(obj is GameObject){
				GameObject go = obj as GameObject;
				var spriteCollection = go.GetComponent<tk2dSpriteCollection>();
				if(spriteCollection != null){
					if(spriteCollection.spriteCollection != null){
						collectionData = spriteCollection.spriteCollection;
                    }
                }
            }
        }

		if(collectionData == null)
			return;

		foreach(TextAsset t in jsonList){
			string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(t)) + "/" + t.name + "_SkeletonData.asset";

			SkeletonDataAsset skeletonDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(path, typeof(SkeletonDataAsset));
			if(skeletonDataAsset == null){
				skeletonDataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
				AssetDatabase.CreateAsset(skeletonDataAsset, path);
			}

			skeletonDataAsset.skeletonJSON = t;
			skeletonDataAsset.spriteCollection = collectionData;
			skeletonDataAsset.defaultMix = 0.2f;
			skeletonDataAsset.fromAnimation = new string[0];
			skeletonDataAsset.toAnimation = new string[0];
			skeletonDataAsset.duration = new float[0];

			EditorUtility.SetDirty(skeletonDataAsset);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

    }
}
