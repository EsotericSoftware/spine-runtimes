using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Spine;

[InitializeOnLoad]
public static class SpineEditorUtilities {

	public static class Icons{
		public static Texture2D skeleton;
		public static Texture2D nullBone;
		public static Texture2D bone;
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

		public static Mesh boneMesh{
			get{
				if(_boneMesh == null){
					_boneMesh = new Mesh();
					_boneMesh.vertices = new Vector3[4]{Vector3.zero, new Vector3(-0.1f,0.1f,0), Vector3.up, new Vector3(0.1f,0.1f,0)};
					_boneMesh.uv = new Vector2[4];
					_boneMesh.triangles = new int[6]{0,1,2,2,3,0};
					_boneMesh.RecalculateBounds();
					_boneMesh.RecalculateNormals();
				}

				return _boneMesh;
			}
		}
		internal static Mesh _boneMesh;


		public static Material boneMaterial{
			get{
				if(_boneMaterial == null){
					_boneMaterial = new Material(Shader.Find("Spine/Bones"));
					_boneMaterial.SetColor("_Color", new Color(0.4f, 0.4f, 0.4f, 0.25f));
				}

				return _boneMaterial;
			}
		}
		internal static Material _boneMaterial;

		public static void Initialize(){
			skeleton = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-skeleton.png");
			nullBone = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-null.png");
			bone = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-bone.png");
			boneNib = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-boneNib.png");
			slot = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-slot.png");
			skinPlaceholder = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-skinPlaceholder.png");
			image = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-image.png");
			boundingBox = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-boundingBox.png");
			mesh = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-mesh.png");
			skin = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-skinPlaceholder.png");
			skinsRoot = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-skinsRoot.png");
			animation = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-animation.png");
			animationRoot = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-animationRoot.png");
			spine = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-spine.png");
			_event = (Texture2D)AssetDatabase.LoadMainAssetAtPath( SpineEditorUtilities.editorGUIPath + "/icon-event.png");
		}

	}



	public static string editorPath = "";
	public static string editorGUIPath = "";

	static List<int> skeletonRendererInstanceIDs;

	public static float defaultScale = 0.01f;
	public static float defaultMix = 0.2f;
	public static string defaultShader = "Spine/Skeleton";
	
	static SpineEditorUtilities(){
		DirectoryInfo rootDir = new DirectoryInfo(Application.dataPath);
		FileInfo[] files = rootDir.GetFiles("SpineEditorUtilities.cs", SearchOption.AllDirectories);
		editorPath = Path.GetDirectoryName(files[0].FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets"));
		editorGUIPath = editorPath + "/GUI";	

		Icons.Initialize();

		skeletonRendererInstanceIDs = new List<int>();

		EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
		EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;

		HierarchyWindowChanged();
	}

	static void HierarchyWindowChanged(){
		skeletonRendererInstanceIDs.Clear();

		SkeletonRenderer[] arr = Object.FindObjectsOfType<SkeletonRenderer>();

		foreach(SkeletonRenderer r in arr)
			skeletonRendererInstanceIDs.Add(r.gameObject.GetInstanceID());
	}

	static void HierarchyWindowItemOnGUI(int instanceId, Rect selectionRect){
		if(skeletonRendererInstanceIDs.Contains(instanceId)){
			Rect r = new Rect (selectionRect); 
			r.x = r.width - 15;
			r.width = 15;

			GUI.Label(r, Icons.spine);
		}
	}
	
	[MenuItem("Assets/Spine/Ingest")]
	static void IngestSpineProject(){
		TextAsset spineJson = null;
		TextAsset atlasText = null;
		
		foreach(UnityEngine.Object o in Selection.objects){
			if(o.GetType() != typeof(TextAsset))
				continue;
			
			string fileName = Path.GetFileName(AssetDatabase.GetAssetPath(o));
			
			if(fileName.EndsWith(".json"))
				spineJson = (TextAsset)o;
			else if(fileName.EndsWith(".atlas.txt"))
				atlasText = (TextAsset)o;
		}
		
		if(spineJson == null){
			EditorUtility.DisplayDialog("Error!", "Spine JSON file not found in selection!", "OK");
			return;
		}
		
		string primaryName = Path.GetFileNameWithoutExtension(spineJson.name);
		string assetPath = Path.GetDirectoryName( AssetDatabase.GetAssetPath(spineJson));
		
		if(atlasText == null){
			string atlasPath = assetPath + "/" + primaryName + ".atlas.txt";
			atlasText = (TextAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(TextAsset));
		}
		
		if(spineJson != null && atlasText != null){
			
			AssetPreview.GetMiniTypeThumbnail(typeof(bool));
			
			AtlasAsset atlasAsset = AtlasAsset.CreateInstance<AtlasAsset>();
			atlasAsset.atlasFile = atlasText;



			
			string[] atlasLines = atlasText.text.Split('\n');
			List<string> pageFiles = new List<string>();
			for(int i = 0; i < atlasLines.Length-1; i++){
				if(atlasLines[i].Length == 0)
					pageFiles.Add(atlasLines[i+1]);
			}

			atlasAsset.materials = new Material[pageFiles.Count];

			for(int i = 0; i < pageFiles.Count; i++){
				string texturePath = assetPath + "/" + pageFiles[i];
				Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
				
				TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
				texImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				texImporter.mipmapEnabled = false;
				EditorUtility.SetDirty(texImporter);
				AssetDatabase.ImportAsset(texturePath);
				AssetDatabase.SaveAssets();

				string pageName = Path.GetFileNameWithoutExtension(pageFiles[i]);

				//because this looks silly
				if(pageName == primaryName && pageFiles.Count == 1)
					pageName = "Material";

				string materialPath = assetPath + "/" + primaryName + "_" + pageName + ".mat";

				Material mat = new Material(Shader.Find(defaultShader));

				mat.mainTexture = texture;
				
				AssetDatabase.CreateAsset(mat, materialPath);
				AssetDatabase.SaveAssets();
				
				atlasAsset.materials[i] = mat;
			}
			
			AssetDatabase.CreateAsset(atlasAsset, assetPath + "/" + primaryName + "_Atlas.asset");
			AssetDatabase.SaveAssets();
			
			SkeletonDataAsset skelDataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
			skelDataAsset.atlasAsset = atlasAsset;
			skelDataAsset.skeletonJSON = spineJson;
			skelDataAsset.fromAnimation = new string[0];
			skelDataAsset.toAnimation = new string[0];
			skelDataAsset.duration = new float[0];
			skelDataAsset.defaultMix = defaultMix;
			skelDataAsset.scale = defaultScale;

			AssetDatabase.CreateAsset(skelDataAsset, assetPath + "/" + primaryName + "_SkeletonData.asset");
			AssetDatabase.SaveAssets();
		}
		else{
			EditorUtility.DisplayDialog("Error!", "Atlas file not found in selection!", "OK");
			return;
		}
	}

	[MenuItem("Assets/Spine/Spawn")]
	static void SpawnAnimatedSkeleton(){
		Object[] arr = Selection.objects;

		foreach(Object o in arr){

			string guid = AssetDatabase.AssetPathToGUID( AssetDatabase.GetAssetPath( o ) );
			string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

			SpawnAnimatedSkeleton((SkeletonDataAsset)o, skinName);
			SceneView.RepaintAll();
		}
	}

	[MenuItem("Assets/Spine/Spawn", true)]
	static bool ValidateSpawnAnimatedSkeleton(){
		Object[] arr = Selection.objects;
		
		if(arr.Length == 0)
			return false;
		
		foreach(Object o in arr){
			if(o.GetType() != typeof(SkeletonDataAsset))
				return false;
		}
		
		return true;
	}

	public static SkeletonAnimation SpawnAnimatedSkeleton(SkeletonDataAsset skeletonDataAsset, string skinName){
		return SpawnAnimatedSkeleton(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
	}

	public static SkeletonAnimation SpawnAnimatedSkeleton(SkeletonDataAsset skeletonDataAsset, Skin skin = null){
		
		GameObject go = new GameObject(skeletonDataAsset.name.Replace("_SkeletonData", ""), typeof(MeshFilter), typeof(MeshRenderer), typeof(SkeletonAnimation));
		SkeletonAnimation anim = go.GetComponent<SkeletonAnimation>();
		anim.skeletonDataAsset = skeletonDataAsset;

		bool requiresNormals = false;

		foreach(Material m in anim.skeletonDataAsset.atlasAsset.materials){
			if(m.shader.name.Contains("Lit")){
				requiresNormals = true;
				break;
			}
		}

		anim.calculateNormals = requiresNormals;

		if(skin == null)
			skin = skeletonDataAsset.GetSkeletonData(true).DefaultSkin;

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
