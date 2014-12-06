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
	
	[MenuItem("Assets/Spine/Ingest")]
	static void IngestSpineProjectFromSelection () {
		TextAsset spineJson = null;
		TextAsset atlasText = null;

		List<TextAsset> spineJsonList = new List<TextAsset>();

		foreach (UnityEngine.Object o in Selection.objects) {
			if (o.GetType() != typeof(TextAsset))
				continue;
			
			string fileName = Path.GetFileName(AssetDatabase.GetAssetPath(o));
			
			if (fileName.EndsWith(".json"))
				spineJson = (TextAsset)o;
			else if (fileName.EndsWith(".atlas.txt"))
					atlasText = (TextAsset)o;
		}
		
		if (spineJson == null) {
			EditorUtility.DisplayDialog("Error!", "Spine JSON file not found in selection!", "OK");
			return;
		}
		
		string primaryName = Path.GetFileNameWithoutExtension(spineJson.name);
		string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spineJson));
		
		if (atlasText == null) {
			string atlasPath = assetPath + "/" + primaryName + ".atlas.txt";
			atlasText = (TextAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(TextAsset));
		}

		AtlasAsset atlasAsset = IngestSpineAtlas(atlasText);

		IngestSpineProject(spineJson, atlasAsset);
	}

	static void OnPostprocessAllAssets (string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths) {
		//debug
//		return;

		AtlasAsset sharedAtlas = null;

		System.Array.Sort<string>(imported);

		foreach (string str in imported) {
			if (Path.GetExtension(str).ToLower() == ".json") {
				TextAsset spineJson = (TextAsset)AssetDatabase.LoadAssetAtPath(str, typeof(TextAsset));
				if (IsSpineJSON(spineJson)) {

					if (sharedAtlas != null) {
						string spinePath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spineJson));
						string atlasPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(sharedAtlas));
						if (spinePath != atlasPath)
							sharedAtlas = null;
					}

					SkeletonDataAsset data = AutoIngestSpineProject(spineJson, sharedAtlas);
					if (data == null)
						continue;

					sharedAtlas = data.atlasAsset;


					string dir = Path.GetDirectoryName(Path.GetDirectoryName(AssetDatabase.GetAssetPath(data)));
					string prefabPath = Path.Combine(dir, data.skeletonJSON.name + ".prefab").Replace("\\", "/");

					if (File.Exists(prefabPath) == false) {
						SkeletonAnimation anim = SpawnAnimatedSkeleton(data);
						PrefabUtility.CreatePrefab(prefabPath, anim.gameObject, ReplacePrefabOptions.ReplaceNameBased);
						if (EditorApplication.isPlaying)
							GameObject.Destroy(anim.gameObject);
						else
							GameObject.DestroyImmediate(anim.gameObject);
					} else {

					}


				}
			}
		}
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

	static SkeletonDataAsset AutoIngestSpineProject (TextAsset spineJson, Object atlasSource = null) {
		TextAsset atlasText = null;
		AtlasAsset atlasAsset = null;

		if (atlasSource != null) {
			if (atlasSource.GetType() == typeof(TextAsset)) {
				atlasText = (TextAsset)atlasSource;
			} else if (atlasSource.GetType() == typeof(AtlasAsset)) {
					atlasAsset = (AtlasAsset)atlasSource;
				}
		}

		if (atlasText == null && atlasAsset == null) {
			string primaryName = Path.GetFileNameWithoutExtension(spineJson.name);
			string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spineJson));
			
			if (atlasText == null) {
				string atlasPath = assetPath + "/" + primaryName + ".atlas.txt";
				atlasText = (TextAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(TextAsset));

				if (atlasText == null) {
					//can't find atlas, likely because using a shared atlas
					bool abort = !EditorUtility.DisplayDialog("Atlas not Found", "Expecting " + spineJson.name + ".atlas\n" + "Press OK to select Atlas", "OK", "Abort");
					if (abort) {
						//do nothing, let it error later
					} else {
						string path = EditorUtility.OpenFilePanel("Find Atlas source...", Path.GetDirectoryName(Application.dataPath) + "/" + assetPath, "txt");
						if (path != "") {
							path = path.Replace("\\", "/");
							path = path.Replace(Application.dataPath.Replace("\\", "/"), "Assets");
							atlasText = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
						}
					}

				}
			}
		}

		if (atlasAsset == null)
			atlasAsset = IngestSpineAtlas(atlasText);

		return IngestSpineProject(spineJson, atlasAsset);
	}

	static AtlasAsset IngestSpineAtlas (TextAsset atlasText) {
		if (atlasText == null) {
			Debug.LogWarning("Atlas source cannot be null!");
			return null;
		}

		string primaryName = Path.GetFileNameWithoutExtension(atlasText.name).Replace(".atlas", "");
		string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(atlasText));

		string atlasPath = assetPath + "/" + primaryName + "_Atlas.asset";

		AtlasAsset atlasAsset = (AtlasAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(AtlasAsset));


		if (atlasAsset == null)
			atlasAsset = AtlasAsset.CreateInstance<AtlasAsset>();

		atlasAsset.atlasFile = atlasText;

		//strip CR
		string atlasStr = atlasText.text;
		atlasStr = atlasStr.Replace("\r", "");

		string[] atlasLines = atlasStr.Split('\n');
		List<string> pageFiles = new List<string>();
		for (int i = 0; i < atlasLines.Length-1; i++) {
			if (atlasLines[i].Length == 0)
				pageFiles.Add(atlasLines[i + 1]);
		}
		
		atlasAsset.materials = new Material[pageFiles.Count];
		
		for (int i = 0; i < pageFiles.Count; i++) {
			string texturePath = assetPath + "/" + pageFiles[i];
			Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
			
			TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
			texImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			texImporter.mipmapEnabled = false;
			texImporter.alphaIsTransparency = false;
			texImporter.maxTextureSize = 2048;

			EditorUtility.SetDirty(texImporter);
			AssetDatabase.ImportAsset(texturePath);
			AssetDatabase.SaveAssets();
			
			string pageName = Path.GetFileNameWithoutExtension(pageFiles[i]);
			
			//because this looks silly
			if (pageName == primaryName && pageFiles.Count == 1)
				pageName = "Material";
			
			string materialPath = assetPath + "/" + primaryName + "_" + pageName + ".mat";
			Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));

			if (mat == null) {
				mat = new Material(Shader.Find(defaultShader));
				AssetDatabase.CreateAsset(mat, materialPath);
			}
			
			mat.mainTexture = texture;
			EditorUtility.SetDirty(mat);

			AssetDatabase.SaveAssets();
			
			atlasAsset.materials[i] = mat;
		}

		if (AssetDatabase.GetAssetPath(atlasAsset) == "")
			AssetDatabase.CreateAsset(atlasAsset, atlasPath);
		else
			atlasAsset.Reset();

		AssetDatabase.SaveAssets();

		return (AtlasAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(AtlasAsset));
	}

	static SkeletonDataAsset IngestSpineProject (TextAsset spineJson, AtlasAsset atlasAsset = null) {
		string primaryName = Path.GetFileNameWithoutExtension(spineJson.name);
		string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spineJson));
		string filePath = assetPath + "/" + primaryName + "_SkeletonData.asset";

		if (spineJson != null && atlasAsset != null) {

			SkeletonDataAsset skelDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(SkeletonDataAsset));
			if (skelDataAsset == null) {
				skelDataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
				skelDataAsset.atlasAsset = atlasAsset;
				skelDataAsset.skeletonJSON = spineJson;
				skelDataAsset.fromAnimation = new string[0];
				skelDataAsset.toAnimation = new string[0];
				skelDataAsset.duration = new float[0];
				skelDataAsset.defaultMix = defaultMix;
				skelDataAsset.scale = defaultScale;
				
				AssetDatabase.CreateAsset(skelDataAsset, filePath);
				AssetDatabase.SaveAssets();
			} else {
				skelDataAsset.Reset();
				skelDataAsset.GetSkeletonData(true);
			}

			return skelDataAsset;
		} else {
			EditorUtility.DisplayDialog("Error!", "Must specify both Spine JSON and Atlas TextAsset", "OK");
			return null;
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
		return SpawnAnimatedSkeleton(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
	}

	public static SkeletonAnimation SpawnAnimatedSkeleton (SkeletonDataAsset skeletonDataAsset, Skin skin = null) {
		GameObject go = new GameObject(skeletonDataAsset.name.Replace("_SkeletonData", ""), typeof(MeshFilter), typeof(MeshRenderer), typeof(SkeletonAnimation));
		SkeletonAnimation anim = go.GetComponent<SkeletonAnimation>();
		anim.skeletonDataAsset = skeletonDataAsset;

		bool requiresNormals = false;

		foreach (Material m in anim.skeletonDataAsset.atlasAsset.materials) {
			if (m.shader.name.Contains("Lit")) {
				requiresNormals = true;
				break;
			}
		}

		anim.calculateNormals = requiresNormals;

		SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

		if (data == null) {
			string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAsset);
			skeletonDataAsset.atlasAsset = (AtlasAsset)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAsset));
			data = skeletonDataAsset.GetSkeletonData(true);
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
}
