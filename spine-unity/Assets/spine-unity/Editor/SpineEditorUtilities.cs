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
using System.Linq;
using System.Reflection;
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
		public static Texture2D slotRoot;
		public static Texture2D skinPlaceholder;
		public static Texture2D image;
		public static Texture2D boundingBox;
		public static Texture2D mesh;
		public static Texture2D weights;
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
		public static Texture2D unityIcon;
		public static Texture2D controllerIcon;

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
					_boneMesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
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
			slotRoot = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-slotRoot.png");
			skinPlaceholder = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skinPlaceholder.png");
			image = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-image.png");
			boundingBox = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-boundingBox.png");
			mesh = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-mesh.png");
			weights = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-weights.png");
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

			unityIcon = EditorGUIUtility.FindTexture("SceneAsset Icon");

			controllerIcon = EditorGUIUtility.FindTexture("AnimatorController Icon");
		}
	}

	public static string editorPath = "";
	public static string editorGUIPath = "";
	static Dictionary<int, GameObject> skeletonRendererTable;
	static Dictionary<int, SkeletonUtilityBone> skeletonUtilityBoneTable;
	public static float defaultScale = 0.01f;
	public static float defaultMix = 0.2f;
	public static string defaultShader = "Spine/Skeleton";
	public static bool initialized;

	const string DEFAULT_MIX_KEY = "SPINE_DEFAULT_MIX";

	static SpineEditorUtilities () {
		Initialize();
	}

	static void Initialize () {
		defaultMix = EditorPrefs.GetFloat(DEFAULT_MIX_KEY, 0.2f);

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
		initialized = true;
	}

	public static void ConfirmInitialization () {
		if (!initialized || Icons.skeleton == null)
			Initialize();
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

	static void OnPostprocessAllAssets (string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths) {
		ImportSpineContent(imported, false);
	}
	public static void ImportSpineContent (string[] imported, bool reimport = false) {
		List<string> atlasPaths = new List<string>();
		List<string> imagePaths = new List<string>();
		List<string> skeletonPaths = new List<string>();

		foreach (string str in imported) {
			string extension = Path.GetExtension(str).ToLower();
			switch (extension) {
				case ".txt":
					if (str.EndsWith(".atlas.txt")) {
						atlasPaths.Add(str);
					}
					break;
				case ".png":
				case ".jpg":
					imagePaths.Add(str);
					break;
				case ".json":
					TextAsset spineDataFile = (TextAsset)AssetDatabase.LoadAssetAtPath(str, typeof(TextAsset));
					if (IsValidSpineData(spineDataFile)) {
						skeletonPaths.Add(str);
					}
					break;
			}
		}


		List<AtlasAsset> atlases = new List<AtlasAsset>();

		//import atlases first
		foreach (string ap in atlasPaths) {
			if (!reimport && CheckForValidAtlas(ap))
				continue;

			TextAsset atlasText = (TextAsset)AssetDatabase.LoadAssetAtPath(ap, typeof(TextAsset));
			AtlasAsset atlas = IngestSpineAtlas(atlasText);
			atlases.Add(atlas);
		}

		//import skeletons and match them with atlases
		bool abortSkeletonImport = false;
		foreach (string sp in skeletonPaths) {
			if (!reimport && CheckForValidSkeletonData(sp)) {
				ResetExistingSkeletonData(sp);
				continue;
			}


			string dir = Path.GetDirectoryName(sp);

			var localAtlases = FindAtlasesAtPath(dir);
			var requiredPaths = GetRequiredAtlasRegions(sp);
			var atlasMatch = GetMatchingAtlas(requiredPaths, localAtlases);

			if (atlasMatch != null) {
				IngestSpineProject(AssetDatabase.LoadAssetAtPath(sp, typeof(TextAsset)) as TextAsset, atlasMatch);
			} else {
				bool resolved = false;
				while (!resolved) {
					int result = EditorUtility.DisplayDialogComplex("Skeleton JSON Import Error!", "Could not find matching AtlasAsset for " + Path.GetFileNameWithoutExtension(sp), "Select", "Skip", "Abort");
					switch (result) {
						case -1:
							Debug.Log("Select Atlas");
							AtlasAsset selectedAtlas = GetAtlasDialog(Path.GetDirectoryName(sp));
							if (selectedAtlas != null) {
								localAtlases.Clear();
								localAtlases.Add(selectedAtlas);
								atlasMatch = GetMatchingAtlas(requiredPaths, localAtlases);
								if (atlasMatch != null) {
									resolved = true;
									IngestSpineProject(AssetDatabase.LoadAssetAtPath(sp, typeof(TextAsset)) as TextAsset, atlasMatch);
								}
							}

							break;
						case 0:
							var atlasList = MultiAtlasDialog(requiredPaths, Path.GetDirectoryName(sp), Path.GetFileNameWithoutExtension(sp));

							if (atlasList != null)
								IngestSpineProject(AssetDatabase.LoadAssetAtPath(sp, typeof(TextAsset)) as TextAsset, atlasList.ToArray());

							resolved = true;
							break;

						case 1:
							Debug.Log("Skipped importing: " + Path.GetFileName(sp));
							resolved = true;
							break;


						case 2:
							//abort
							abortSkeletonImport = true;
							resolved = true;
							break;
					}
				}
			}

			if (abortSkeletonImport)
				break;
		}

		//TODO:  any post processing of images
	}

	static bool CheckForValidSkeletonData (string skeletonJSONPath) {

		string dir = Path.GetDirectoryName(skeletonJSONPath);
		TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(skeletonJSONPath, typeof(TextAsset));
		DirectoryInfo dirInfo = new DirectoryInfo(dir);

		FileInfo[] files = dirInfo.GetFiles("*.asset");

		foreach (var f in files) {
			string localPath = dir + "/" + f.Name;
			var obj = AssetDatabase.LoadAssetAtPath(localPath, typeof(Object));
			if (obj is SkeletonDataAsset) {
				var skeletonDataAsset = (SkeletonDataAsset)obj;
				if (skeletonDataAsset.skeletonJSON == textAsset)
					return true;
			}
		}

		return false;
	}

	static void ResetExistingSkeletonData (string skeletonJSONPath) {

		string dir = Path.GetDirectoryName(skeletonJSONPath);
		TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(skeletonJSONPath, typeof(TextAsset));
		DirectoryInfo dirInfo = new DirectoryInfo(dir);

		FileInfo[] files = dirInfo.GetFiles("*.asset");

		foreach (var f in files) {
			string localPath = dir + "/" + f.Name;
			var obj = AssetDatabase.LoadAssetAtPath(localPath, typeof(Object));
			if (obj is SkeletonDataAsset) {
				var skeletonDataAsset = (SkeletonDataAsset)obj;

				if (skeletonDataAsset.skeletonJSON == textAsset) {
					if (Selection.activeObject == skeletonDataAsset)
						Selection.activeObject = null;

					skeletonDataAsset.Reset();

					string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(skeletonDataAsset));
					string lastHash = EditorPrefs.GetString(guid + "_hash");

					if (lastHash != skeletonDataAsset.GetSkeletonData(true).Hash) {
						//do any upkeep on synchronized assets
						UpdateMecanimClips(skeletonDataAsset);
					}

					EditorPrefs.SetString(guid + "_hash", skeletonDataAsset.GetSkeletonData(true).Hash);
				}
			}
		}
	}

	static void UpdateMecanimClips (SkeletonDataAsset skeletonDataAsset) {
		if (skeletonDataAsset.controller == null)
			return;

		SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
	}


	static bool CheckForValidAtlas (string atlasPath) {

		string dir = Path.GetDirectoryName(atlasPath);
		TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(TextAsset));
		DirectoryInfo dirInfo = new DirectoryInfo(dir);

		FileInfo[] files = dirInfo.GetFiles("*.asset");

		foreach (var f in files) {
			string localPath = dir + "/" + f.Name;
			var obj = AssetDatabase.LoadAssetAtPath(localPath, typeof(Object));
			if (obj is AtlasAsset) {
				var atlasAsset = (AtlasAsset)obj;
				if (atlasAsset.atlasFile == textAsset)
					return true;
			}
		}

		return false;
	}

	static List<AtlasAsset> MultiAtlasDialog (List<string> requiredPaths, string initialDirectory, string header = "") {

		List<AtlasAsset> atlasAssets = new List<AtlasAsset>();

		bool resolved = false;
		string lastAtlasPath = initialDirectory;
		while (!resolved) {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(header);
			sb.AppendLine("Atlases:");
			if (atlasAssets.Count == 0) {
				sb.AppendLine("\t--none--");
			}
			for (int i = 0; i < atlasAssets.Count; i++) {
				sb.AppendLine("\t" + atlasAssets[i].name);
			}

			sb.AppendLine();
			sb.AppendLine("Missing Regions:");

			List<string> missingRegions = new List<string>(requiredPaths);

			foreach (var atlasAsset in atlasAssets) {
				var atlas = atlasAsset.GetAtlas();
				for (int i = 0; i < missingRegions.Count; i++) {
					if (atlas.FindRegion(missingRegions[i]) != null) {
						missingRegions.RemoveAt(i);
						i--;
					}
				}
			}

			if (missingRegions.Count == 0) {
				break;
			}

			for (int i = 0; i < missingRegions.Count; i++) {
				sb.AppendLine("\t" + missingRegions[i]);
			}

			int result = EditorUtility.DisplayDialogComplex("Atlas Selection", sb.ToString(), "Select", "Finish", "Abort");

			switch (result) {
				case 0:
					AtlasAsset selectedAtlasAsset = GetAtlasDialog(lastAtlasPath);
					if (selectedAtlasAsset != null) {
						var atlas = selectedAtlasAsset.GetAtlas();
						bool hasValidRegion = false;
						foreach (string str in missingRegions) {
							if (atlas.FindRegion(str) != null) {
								hasValidRegion = true;
								break;
							}
						}

						atlasAssets.Add(selectedAtlasAsset);
					}
					break;

				case 1:
					resolved = true;
					break;

				case 2:
					atlasAssets = null;
					resolved = true;
					break;
			}


		}


		return atlasAssets;
	}

	static AtlasAsset GetAtlasDialog (string dirPath) {
		string path = EditorUtility.OpenFilePanel("Select AtlasAsset...", dirPath, "asset");
		if (path == "")
			return null;

		int subLen = Application.dataPath.Length - 6;
		string assetRelativePath = path.Substring(subLen, path.Length - subLen).Replace("\\", "/");

		Object obj = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(AtlasAsset));

		if (obj == null || obj.GetType() != typeof(AtlasAsset))
			return null;

		return (AtlasAsset)obj;
	}

	public static List<string> GetRequiredAtlasRegions (string jsonPath) {
		List<string> requiredPaths = new List<string>();

		// FIXME - This doesn't work for a binary skeleton file!
		if (jsonPath.Contains(".skel")) return requiredPaths;

		TextAsset spineJson = (TextAsset)AssetDatabase.LoadAssetAtPath(jsonPath, typeof(TextAsset));

		StringReader reader = new StringReader(spineJson.text);
		var root = Json.Deserialize(reader) as Dictionary<string, object>;

		foreach (KeyValuePair<string, object> entry in (Dictionary<string, object>)root["skins"]) {
			foreach (KeyValuePair<string, object> slotEntry in (Dictionary<string, object>)entry.Value) {

				foreach (KeyValuePair<string, object> attachmentEntry in ((Dictionary<string, object>)slotEntry.Value)) {
					var data = ((Dictionary<string, object>)attachmentEntry.Value);
					if (data.ContainsKey("type")) {
						if ((string)data["type"] == "boundingbox") {
							continue;
						}
							
					}
					if (data.ContainsKey("path"))
						requiredPaths.Add((string)data["path"]);
					else if (data.ContainsKey("name"))
						requiredPaths.Add((string)data["name"]);
					else
						requiredPaths.Add(attachmentEntry.Key);
					//requiredPaths.Add((string)sdf["path"]);
				}
			}
		}

		return requiredPaths;
	}
	static AtlasAsset GetMatchingAtlas (List<string> requiredPaths, List<AtlasAsset> atlasAssets) {
		AtlasAsset atlasAssetMatch = null;

		foreach (AtlasAsset a in atlasAssets) {
			Atlas atlas = a.GetAtlas();
			bool failed = false;
			foreach (string regionPath in requiredPaths) {
				if (atlas.FindRegion(regionPath) == null) {
					failed = true;
					break;
				}
			}

			if (!failed) {
				atlasAssetMatch = a;
				break;
			}

		}

		return atlasAssetMatch;
	}

	static List<AtlasAsset> FindAtlasesAtPath (string path) {
		List<AtlasAsset> arr = new List<AtlasAsset>();

		DirectoryInfo dir = new DirectoryInfo(path);
		FileInfo[] assetInfoArr = dir.GetFiles("*.asset");

		int subLen = Application.dataPath.Length - 6;

		foreach (var f in assetInfoArr) {
			string assetRelativePath = f.FullName.Substring(subLen, f.FullName.Length - subLen).Replace("\\", "/");

			Object obj = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(AtlasAsset));
			if (obj != null) {
				arr.Add(obj as AtlasAsset);
			}

		}


		return arr;
	}

	public static bool IsValidSpineData (TextAsset asset) {
		if (asset.name.Contains(".skel")) return true;

		object obj = null;
		try {
			obj = Json.Deserialize(new StringReader(asset.text));
		} catch (System.Exception) {
		}
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
		for (int i = 0; i < atlasLines.Length - 1; i++) {
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

	static SkeletonDataAsset IngestSpineProject (TextAsset spineJson, params AtlasAsset[] atlasAssets) {
		string primaryName = Path.GetFileNameWithoutExtension(spineJson.name);
		string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spineJson));
		string filePath = assetPath + "/" + primaryName + "_SkeletonData.asset";

		if (spineJson != null && atlasAssets != null) {

			SkeletonDataAsset skelDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(SkeletonDataAsset));
			if (skelDataAsset == null) {
				skelDataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
				skelDataAsset.atlasAssets = atlasAssets;
				skelDataAsset.skeletonJSON = spineJson;
				skelDataAsset.fromAnimation = new string[0];
				skelDataAsset.toAnimation = new string[0];
				skelDataAsset.duration = new float[0];
				skelDataAsset.defaultMix = defaultMix;
				skelDataAsset.scale = defaultScale;

				AssetDatabase.CreateAsset(skelDataAsset, filePath);
				AssetDatabase.SaveAssets();
			} else {
				skelDataAsset.atlasAssets = atlasAssets;
				skelDataAsset.Reset();
				skelDataAsset.GetSkeletonData(true);
			}

			return skelDataAsset;
		} else {
			EditorUtility.DisplayDialog("Error!", "Must specify both Spine JSON and AtlasAsset array", "OK");
			return null;
		}
	}

	[MenuItem("Assets/Spine/Instantiate (SkeletonAnimation)")]
	static void InstantiateSkeletonAnimation () {
		Object[] arr = Selection.objects;
		foreach (Object o in arr) {
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
			string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

			InstantiateSkeletonAnimation((SkeletonDataAsset)o, skinName);
			SceneView.RepaintAll();
		}
	}

	[MenuItem("Assets/Spine/Instantiate (SkeletonAnimation)", true)]
	static bool ValidateInstantiateSkeletonAnimation () {
		Object[] arr = Selection.objects;

		if (arr.Length == 0)
			return false;

		foreach (Object o in arr) {
			if (o.GetType() != typeof(SkeletonDataAsset))
				return false;
		}

		return true;
	}

	public static SkeletonAnimation InstantiateSkeletonAnimation (SkeletonDataAsset skeletonDataAsset, string skinName) {
		return InstantiateSkeletonAnimation(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
	}

	public static SkeletonAnimation InstantiateSkeletonAnimation (SkeletonDataAsset skeletonDataAsset, Skin skin = null) {
		GameObject go = new GameObject(skeletonDataAsset.name.Replace("_SkeletonData", ""), typeof(MeshFilter), typeof(MeshRenderer), typeof(SkeletonAnimation));
		SkeletonAnimation anim = go.GetComponent<SkeletonAnimation>();
		anim.skeletonDataAsset = skeletonDataAsset;

		bool requiresNormals = false;

		foreach (AtlasAsset atlasAsset in anim.skeletonDataAsset.atlasAssets) {
			foreach (Material m in atlasAsset.materials) {
				if (m.shader.name.Contains("Lit")) {
					requiresNormals = true;
					break;
				}
			}
		}



		anim.calculateNormals = requiresNormals;

		SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

		if (data == null) {
			for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
				string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
				skeletonDataAsset.atlasAssets[i] = (AtlasAsset)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAsset));
			}

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

	[MenuItem("Assets/Spine/Instantiate (Mecanim)")]
	static void InstantiateSkeletonAnimator () {
		Object[] arr = Selection.objects;
		foreach (Object o in arr) {
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
			string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

			InstantiateSkeletonAnimator((SkeletonDataAsset)o, skinName);
			SceneView.RepaintAll();
		}
	}

	[MenuItem("Assets/Spine/Instantiate (SkeletonAnimation)", true)]
	static bool ValidateInstantiateSkeletonAnimator () {
		Object[] arr = Selection.objects;

		if (arr.Length == 0)
			return false;

		foreach (Object o in arr) {
			if (o.GetType() != typeof(SkeletonDataAsset))
				return false;
		}

		return true;
	}

	public static SkeletonAnimator InstantiateSkeletonAnimator (SkeletonDataAsset skeletonDataAsset, string skinName) {
		return InstantiateSkeletonAnimator(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
	}

	public static SkeletonAnimator InstantiateSkeletonAnimator (SkeletonDataAsset skeletonDataAsset, Skin skin = null) {
		GameObject go = new GameObject(skeletonDataAsset.name.Replace("_SkeletonData", ""), typeof(MeshFilter), typeof(MeshRenderer), typeof(Animator), typeof(SkeletonAnimator));

		if (skeletonDataAsset.controller == null) {
			SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
		}

		go.GetComponent<Animator>().runtimeAnimatorController = skeletonDataAsset.controller;

		SkeletonAnimator anim = go.GetComponent<SkeletonAnimator>();
		anim.skeletonDataAsset = skeletonDataAsset;

		bool requiresNormals = false;

		foreach (AtlasAsset atlasAsset in anim.skeletonDataAsset.atlasAssets) {
			foreach (Material m in atlasAsset.materials) {
				if (m.shader.name.Contains("Lit")) {
					requiresNormals = true;
					break;
				}
			}
		}

		anim.calculateNormals = requiresNormals;

		SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

		if (data == null) {
			for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
				string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
				skeletonDataAsset.atlasAssets[i] = (AtlasAsset)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAsset));
			}

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
		anim.skeleton.UpdateWorldTransform();
		anim.LateUpdate();

		return anim;
	}

	static bool preferencesLoaded = false;

	[PreferenceItem("Spine")]
	static void PreferencesGUI () {
		if (!preferencesLoaded) {
			preferencesLoaded = true;
			defaultMix = EditorPrefs.GetFloat(DEFAULT_MIX_KEY, 0.2f);
		}


		EditorGUILayout.LabelField("Auto-Import Settings", EditorStyles.boldLabel);
		EditorGUI.BeginChangeCheck();
		defaultMix = EditorGUILayout.FloatField("Default Mix", defaultMix);
		if (EditorGUI.EndChangeCheck())
			EditorPrefs.SetFloat(DEFAULT_MIX_KEY, defaultMix);

		GUILayout.Space(20);
		EditorGUILayout.LabelField("3rd Party Settings", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("TK2D");

		if (GUILayout.Button("Enable", GUILayout.Width(64)))
			EnableTK2D();
		if (GUILayout.Button("Disable", GUILayout.Width(64)))
			DisableTK2D();
		GUILayout.EndHorizontal();
	}


	//TK2D Support
	const string SPINE_TK2D_DEFINE = "SPINE_TK2D";

	static void EnableTK2D () {
		bool added = false;
		foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup))) {
			string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
			if (!defines.Contains(SPINE_TK2D_DEFINE)) {
				added = true;
				if (defines.EndsWith(";"))
					defines = defines + SPINE_TK2D_DEFINE;
				else
					defines = defines + ";" + SPINE_TK2D_DEFINE;

				PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
			}
		}

		if (added) {
			Debug.LogWarning("Setting Scripting Define Symbol " + SPINE_TK2D_DEFINE);
		} else {
			Debug.LogWarning("Already Set Scripting Define Symbol " + SPINE_TK2D_DEFINE);
		}
	}


	static void DisableTK2D () {
		bool removed = false;
		foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup))) {
			string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
			if (defines.Contains(SPINE_TK2D_DEFINE)) {
				removed = true;
				if (defines.Contains(SPINE_TK2D_DEFINE + ";"))
					defines = defines.Replace(SPINE_TK2D_DEFINE + ";", "");
				else
					defines = defines.Replace(SPINE_TK2D_DEFINE, "");

				PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
			}
		}

		if (removed) {
			Debug.LogWarning("Removing Scripting Define Symbol " + SPINE_TK2D_DEFINE);
		} else {
			Debug.LogWarning("Already Removed Scripting Define Symbol " + SPINE_TK2D_DEFINE);
		}
	}

}