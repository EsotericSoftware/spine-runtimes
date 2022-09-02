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

#pragma warning disable 0219

#define SPINE_SKELETONMECANIM

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

#if UNITY_2018_3_OR_NEWER
#define NEW_PREFERENCES_SETTINGS_PROVIDER
#endif

#if UNITY_2018_2_OR_NEWER
#define EXPOSES_SPRITE_ATLAS_UTILITIES
#endif

#if !UNITY_2019_4_OR_NEWER
#define PROBLEMATIC_PACKAGE_ASSET_MODIFICATION
#endif

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using CompatibilityProblemInfo = Spine.Unity.SkeletonDataCompatibility.CompatibilityProblemInfo;

namespace Spine.Unity.Editor {

	public class PathAndProblemInfo {
		public string path;
		public CompatibilityProblemInfo compatibilityProblems;
		public string otherProblemDescription;

		public PathAndProblemInfo (string path, CompatibilityProblemInfo compatibilityInfo, string otherProblemDescription) {
			this.path = path;
			this.compatibilityProblems = compatibilityInfo;
			this.otherProblemDescription = otherProblemDescription;
		}
	}

	public static class AssetUtility {

		public const string SkeletonDataSuffix = "_SkeletonData";
		public const string AtlasSuffix = "_Atlas";
		public const string SpriteAtlasSuffix = "_SpriteAtlas";

		/// HACK: This list keeps the asset reference temporarily during importing.
		///
		/// In cases of very large projects/sufficient RAM pressure, when AssetDatabase.SaveAssets is called,
		/// Unity can mistakenly unload assets whose references are only on the stack.
		/// This leads to MissingReferenceException and other errors.
		public static readonly List<ScriptableObject> protectFromStackGarbageCollection = new List<ScriptableObject>();
		public static HashSet<string> assetsImportedInWrongState = new HashSet<string>();

		public static void HandleOnPostprocessAllAssets (string[] imported, List<string> texturesWithoutMetaFile) {
			// In case user used "Assets -> Reimport All", during the import process,
			// asset database is not initialized until some point. During that period,
			// all attempts to load any assets using API (i.e. AssetDatabase.LoadAssetAtPath)
			// will return null, and as result, assets won't be loaded even if they actually exists,
			// which may lead to numerous importing errors.
			// This situation also happens if Library folder is deleted from the project, which is a pretty
			// common case, since when using version control systems, the Library folder must be excluded.
			//
			// So to avoid this, in case asset database is not available, we delay loading the assets
			// until next time.
			//
			// Unity *always* reimports some internal assets after the process is done, so this method
			// is always called once again in a state when asset database is available.
			//
			// Checking whether AssetDatabase is initialized is done by attempting to load
			// a known "marker" asset that should always be available. Failing to load this asset
			// means that AssetDatabase is not initialized.
			AssetUtility.assetsImportedInWrongState.UnionWith(imported);
			if (AssetDatabaseAvailabilityDetector.IsAssetDatabaseAvailable()) {
				string[] combinedAssets = AssetUtility.assetsImportedInWrongState.ToArray();
				AssetUtility.assetsImportedInWrongState.Clear();
				AssetUtility.ImportSpineContent(combinedAssets, texturesWithoutMetaFile);
			}
		}

		#region Match SkeletonData with Atlases
		static readonly AttachmentType[] AtlasTypes = { AttachmentType.Region, AttachmentType.Linkedmesh, AttachmentType.Mesh };

		public static List<string> GetRequiredAtlasRegions (string skeletonDataPath) {
			HashSet<string> requiredPaths = new HashSet<string>();

			if (skeletonDataPath.Contains(".skel")) {
				List<string> requiredPathsResult = new List<string>();
				AddRequiredAtlasRegionsFromBinary(skeletonDataPath, requiredPathsResult);
				return requiredPathsResult;
			}

			TextReader reader = null;
			TextAsset spineJson = AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonDataPath);
			Dictionary<string, object> root = null;
			try {
				if (spineJson != null) {
					reader = new StringReader(spineJson.text);
				} else {
					// On a "Reimport All" the order of imports can be wrong, thus LoadAssetAtPath() above could return null.
					// as a workaround, we provide a fallback reader.
					reader = new StreamReader(skeletonDataPath);
				}
				root = Json.Deserialize(reader) as Dictionary<string, object>;
			} finally {
				if (reader != null)
					reader.Dispose();
			}

			if (root == null || !root.ContainsKey("skins"))
				return new List<string>();

			var skinsList = root["skins"] as List<object>;
			if (skinsList == null)
				return new List<string>();

			foreach (Dictionary<string, object> skinMap in skinsList) {
				if (!skinMap.ContainsKey("attachments"))
					continue;
				foreach (var slot in (Dictionary<string, object>)skinMap["attachments"]) {
					foreach (var attachment in ((Dictionary<string, object>)slot.Value)) {
						var data = ((Dictionary<string, object>)attachment.Value);

						// Ignore non-atlas-requiring types.
						if (data.ContainsKey("type")) {
							AttachmentType attachmentType;
							string typeString = (string)data["type"];
							try {
								attachmentType = (AttachmentType)System.Enum.Parse(typeof(AttachmentType), typeString, true);
							} catch (System.ArgumentException e) {
								// For more info, visit: http://esotericsoftware.com/forum/Spine-editor-and-runtime-version-management-6534
								Debug.LogWarning(string.Format("Unidentified Attachment type: \"{0}\". Skeleton may have been exported from an incompatible Spine version.", typeString), spineJson);
								throw e;
							}

							if (!AtlasTypes.Contains(attachmentType))
								continue;
						}

						if (data.ContainsKey("path"))
							requiredPaths.Add((string)data["path"]);
						else if (data.ContainsKey("name"))
							requiredPaths.Add((string)data["name"]);
						else if (data.ContainsKey("sequence")) {
							Sequence sequence = SkeletonJson.ReadSequence(data["sequence"]);
							if (sequence != null)
								for (int index = 0; index < sequence.Regions.Length; ++index)
									requiredPaths.Add(sequence.GetPath(attachment.Key, index));
							else
								requiredPaths.Add(attachment.Key);
						} else
							requiredPaths.Add(attachment.Key);
					}
				}
			}

			return requiredPaths.ToList();
		}

		internal static void AddRequiredAtlasRegionsFromBinary (string skeletonDataPath, List<string> requiredPaths) {
			SkeletonBinary binary = new SkeletonBinary(new AtlasRequirementLoader(requiredPaths));
			Stream input = null;
			TextAsset data = AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonDataPath);
			try {
				if (data != null) {
					input = new MemoryStream(data.bytes);
				} else {
					// On a "Reimport All" the order of imports can be wrong, thus LoadAssetAtPath() above could return null.
					// as a workaround, we provide a fallback reader.
					input = File.Open(skeletonDataPath, FileMode.Open, FileAccess.Read);
				}
				binary.ReadSkeletonData(input);
			} finally {
				if (input != null)
					input.Dispose();
			}
			binary = null;
		}

		internal static AtlasAssetBase GetMatchingAtlas (List<string> requiredPaths, string skeletonName,
			List<AtlasAssetBase> atlasAssets) {
			atlasAssets.Sort((a, b) => (
					string.CompareOrdinal(b.name, skeletonName)
					- string.CompareOrdinal(a.name, skeletonName)));
			return GetMatchingAtlas(requiredPaths, atlasAssets);
		}

		internal static AtlasRegion FindRegionIgnoringNumberSuffix (this Atlas atlas, string regionPath) {
			AtlasRegion region = atlas.FindRegion(regionPath);
			if (region != null)
				return region;
			return atlas.FindRegionWithNumberSuffix(regionPath);
		}

		internal static AtlasRegion FindRegionWithNumberSuffix (this Atlas atlas, string regionPath) {
			int pathLength = regionPath.Length;
			foreach (AtlasRegion region in atlas.Regions) {
				string name = region.name;
				if (name.StartsWith(regionPath)) {
					string suffix = name.Substring(pathLength);
					if (suffix.All(c => c >= '0' && c <= '9'))
						return region;
				}
			}
			return null;
		}

		internal static AtlasAssetBase GetMatchingAtlas (List<string> requiredPaths, List<AtlasAssetBase> atlasAssets) {
			AtlasAssetBase atlasAssetMatch = null;

			foreach (AtlasAssetBase a in atlasAssets) {
				Atlas atlas = a.GetAtlas();
				bool failed = false;
				foreach (string regionPath in requiredPaths) {
					if (atlas.FindRegionIgnoringNumberSuffix(regionPath) == null) {
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

		public class AtlasRequirementLoader : AttachmentLoader {
			List<string> requirementList;

			public AtlasRequirementLoader (List<string> requirementList) {
				this.requirementList = requirementList;
			}

			public RegionAttachment NewRegionAttachment (Skin skin, string name, string path, Sequence sequence) {
				var regionAttachment = new RegionAttachment(name);
				if (sequence != null)
					LoadSequence(path, sequence);
				else {
					requirementList.Add(path);
					AssignDummyRegion(regionAttachment);
				}
				return regionAttachment;
			}

			public MeshAttachment NewMeshAttachment (Skin skin, string name, string path, Sequence sequence) {
				var meshAttachment = new MeshAttachment(name);
				if (sequence != null)
					LoadSequence(path, sequence);
				else {
					requirementList.Add(path);
					AssignDummyRegion(meshAttachment);
				}
				return meshAttachment;
			}

			public BoundingBoxAttachment NewBoundingBoxAttachment (Skin skin, string name) {
				return new BoundingBoxAttachment(name);
			}

			public PathAttachment NewPathAttachment (Skin skin, string name) {
				return new PathAttachment(name);
			}

			public PointAttachment NewPointAttachment (Skin skin, string name) {
				return new PointAttachment(name);
			}

			public ClippingAttachment NewClippingAttachment (Skin skin, string name) {
				return new ClippingAttachment(name);
			}

			private void LoadSequence (string basePath, Sequence sequence) {
				TextureRegion[] regions = sequence.Regions;
				for (int i = 0, n = regions.Length; i < n; i++) {
					string path = sequence.GetPath(basePath, i);
					requirementList.Add(path);
				}
			}

			private static void AssignDummyRegion (IHasTextureRegion attachment) {
				attachment.Region = new AtlasRegion();
			}
		}
		#endregion

		public static void ImportSpineContent (string[] imported, List<string> texturesWithoutMetaFile,
			bool reimport = false) {

			var atlasPaths = new List<string>();
			var imagePaths = new List<string>();
			var skeletonPaths = new List<PathAndProblemInfo>();
			CompatibilityProblemInfo compatibilityProblemInfo = null;

			foreach (string str in imported) {
				string extension = Path.GetExtension(str).ToLower();
				switch (extension) {
				case ".atlas":
					if (SpineEditorUtilities.Preferences.atlasTxtImportWarning) {
						Debug.LogWarningFormat("`{0}` : If this file is a Spine atlas, please change its extension to `.atlas.txt`. This is to allow Unity to recognize it and avoid filename collisions. You can also set this file extension when exporting from the Spine editor.", str);
					}
					break;
				case ".skel":
					if (SpineEditorUtilities.Preferences.atlasTxtImportWarning) {
						Debug.LogWarningFormat("`{0}` : If this file is a Spine skeleton, please change its extension to `.skel.bytes`. This is to allow Unity to recognize it and avoid filename collisions. You can also set this file extension when exporting from the Spine editor.", str);
					}
					break;
				case ".txt":
					if (str.EndsWith(".atlas.txt", System.StringComparison.Ordinal))
						atlasPaths.Add(str);
					break;
				case ".png":
				case ".jpg":
					imagePaths.Add(str);
					break;
				case ".json": {
					var jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(str);
					string problemDescription = null;
					if (jsonAsset != null && IsSpineData(jsonAsset, out compatibilityProblemInfo, ref problemDescription))
						skeletonPaths.Add(new PathAndProblemInfo(str, compatibilityProblemInfo, problemDescription));
					if (problemDescription != null)
						Debug.LogError(problemDescription, jsonAsset);
					break;
				}
				case ".bytes": {
					if (str.ToLower().EndsWith(".skel.bytes", System.StringComparison.Ordinal)) {
						var binaryAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(str);
						string problemDescription = null;
						if (IsSpineData(binaryAsset, out compatibilityProblemInfo, ref problemDescription))
							skeletonPaths.Add(new PathAndProblemInfo(str, compatibilityProblemInfo, problemDescription));
						if (problemDescription != null)
							Debug.LogError(problemDescription, binaryAsset);
					}
					break;
				}
				}
			}

			AddDependentAtlasIfImageChanged(atlasPaths, imagePaths);

			// Import atlases first.
			var newAtlases = new List<AtlasAssetBase>();
			foreach (string ap in atlasPaths) {
#if PROBLEMATIC_PACKAGE_ASSET_MODIFICATION
				if (ap.StartsWith("Packages"))
					continue;
#endif
				TextAsset atlasText = AssetDatabase.LoadAssetAtPath<TextAsset>(ap);
				AtlasAssetBase atlas = IngestSpineAtlas(atlasText, texturesWithoutMetaFile);
				newAtlases.Add(atlas);
			}
			AddDependentSkeletonIfAtlasChanged(skeletonPaths, atlasPaths);

			// Import skeletons and match them with atlases.
			bool abortSkeletonImport = false;
			foreach (var skeletonPathEntry in skeletonPaths) {
				string skeletonPath = skeletonPathEntry.path;
				var compatibilityProblems = skeletonPathEntry.compatibilityProblems;
				string otherProblemDescription = skeletonPathEntry.otherProblemDescription;
#if PROBLEMATIC_PACKAGE_ASSET_MODIFICATION
				if (skeletonPath.StartsWith("Packages"))
					continue;
#endif
				if (!reimport && CheckForValidSkeletonData(skeletonPath)) {
					ReloadSkeletonData(skeletonPath, compatibilityProblems);
					continue;
				}

				var loadedAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonPath);
				if (compatibilityProblems != null) {
					IngestIncompatibleSpineProject(loadedAsset, compatibilityProblems);
					continue;
				}
				if (otherProblemDescription != null) {
					continue;
				}

				string dir = Path.GetDirectoryName(skeletonPath).Replace('\\', '/');

#if SPINE_TK2D
				IngestSpineProject(loadedAsset, null);
#else
				string skeletonName = Path.GetFileNameWithoutExtension(skeletonPath);
				List<string> requiredPaths = GetRequiredAtlasRegions(skeletonPath);

				List<AtlasAssetBase> atlasesForSkeleton = FindAtlasesAtPath(dir);
				atlasesForSkeleton = atlasesForSkeleton.Union(newAtlases).ToList();
				List<AtlasAssetBase> atlasesInSameDir = atlasesForSkeleton.Where(
					atlas => AssetDatabase.GetAssetPath(atlas).Contains(dir)).ToList();

				AtlasAssetBase atlasMatch = GetMatchingAtlas(requiredPaths, skeletonName, atlasesInSameDir);
				if (atlasMatch == null && atlasesInSameDir.Count > 0) {
					AtlasAssetBase firstAtlas = atlasesInSameDir[0];
					Debug.LogWarning(string.Format(
						"'{0}' atlas found in skeleton directory does not contain all required attachments",
						firstAtlas.name), firstAtlas);

					List<AtlasAssetBase> atlasesInOtherDir = atlasesForSkeleton.Except(atlasesInSameDir).ToList();
					atlasMatch = GetMatchingAtlas(requiredPaths, skeletonName, atlasesInOtherDir);
					if (atlasMatch != null) {
						Debug.Log(string.Format(
							"Using suitable atlas '{0}' of other imported directory. If this is the " +
							"wrong atlas asset, please assign the correct one at the SkeletonData asset.",
							atlasMatch.name), atlasMatch);
					}
				}

				if (atlasMatch != null || requiredPaths.Count == 0) {
					IngestSpineProject(loadedAsset, atlasMatch);
				} else {
					SkeletonImportDialog(skeletonPath, atlasesForSkeleton, requiredPaths, ref abortSkeletonImport);
				}

				if (abortSkeletonImport)
					break;
#endif
			}

			if (atlasPaths.Count > 0 || imagePaths.Count > 0 || skeletonPaths.Count > 0) {
				SkeletonDataAssetInspector[] skeletonDataInspectors = Resources.FindObjectsOfTypeAll<SkeletonDataAssetInspector>();
				foreach (var inspector in skeletonDataInspectors) {
					inspector.UpdateSkeletonData();
				}
			}

			// Any post processing of images

			// Under some circumstances (e.g. on first import) SkeletonGraphic objects
			// have their skeletonGraphic.skeletonDataAsset reference corrupted
			// by the instance of the ScriptableObject being destroyed but still assigned.
			// Here we restore broken skeletonGraphic.skeletonDataAsset references.
			var skeletonGraphicObjects = Resources.FindObjectsOfTypeAll(typeof(SkeletonGraphic)) as SkeletonGraphic[];
			foreach (var skeletonGraphic in skeletonGraphicObjects) {

				if (skeletonGraphic.skeletonDataAsset == null) {
					var skeletonGraphicID = skeletonGraphic.GetInstanceID();
					if (SpineEditorUtilities.DataReloadHandler.savedSkeletonDataAssetAtSKeletonGraphicID.ContainsKey(skeletonGraphicID)) {
						string assetPath = SpineEditorUtilities.DataReloadHandler.savedSkeletonDataAssetAtSKeletonGraphicID[skeletonGraphicID];
						skeletonGraphic.skeletonDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(assetPath);
					}
				}
			}
		}

		static void AddDependentAtlasIfImageChanged (List<string> atlasPaths, List<string> imagePaths) {
			foreach (var imagePath in imagePaths) {
				string atlasPath = imagePath.Replace(".png", ".atlas.txt");
				if (!System.IO.File.Exists(atlasPath))
					continue;

				if (!atlasPaths.Contains(atlasPath)) {
					atlasPaths.Add(atlasPath);
				}
			}
		}

		static void AddDependentSkeletonIfAtlasChanged (List<PathAndProblemInfo> skeletonPaths, List<string> atlasPaths) {
			foreach (var atlasPath in atlasPaths) {
				string skeletonPathJson = atlasPath.Replace(".atlas.txt", ".json");
				string skeletonPathBinary = atlasPath.Replace(".atlas.txt", ".skel.bytes");
				string usedSkeletonPath = System.IO.File.Exists(skeletonPathJson) ? skeletonPathJson :
										System.IO.File.Exists(skeletonPathBinary) ? skeletonPathBinary : null;
				if (usedSkeletonPath == null)
					continue;

				if (skeletonPaths.FindIndex(p => { return p.path == usedSkeletonPath; }) < 0) {
					string problemDescription = null;
					CompatibilityProblemInfo compatibilityProblemInfo = null;
					TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(usedSkeletonPath);
					if (textAsset != null && IsSpineData(textAsset, out compatibilityProblemInfo, ref problemDescription))
						skeletonPaths.Add(new PathAndProblemInfo(usedSkeletonPath, compatibilityProblemInfo, problemDescription));
				}
			}
		}

		static void ReloadSkeletonData (string skeletonJSONPath, CompatibilityProblemInfo compatibilityProblemInfo) {
			string dir = Path.GetDirectoryName(skeletonJSONPath).Replace('\\', '/');
			TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonJSONPath);
			DirectoryInfo dirInfo = new DirectoryInfo(dir);
			FileInfo[] files = dirInfo.GetFiles("*.asset");

			foreach (var f in files) {
				string localPath = dir + "/" + f.Name;
				var obj = AssetDatabase.LoadAssetAtPath(localPath, typeof(Object));
				var skeletonDataAsset = obj as SkeletonDataAsset;
				if (skeletonDataAsset != null) {
					if (skeletonDataAsset.skeletonJSON == textAsset) {
						if (Selection.activeObject == skeletonDataAsset)
							Selection.activeObject = null;

						if (compatibilityProblemInfo != null) {
							SkeletonDataCompatibility.DisplayCompatibilityProblem(compatibilityProblemInfo.DescriptionString(), textAsset);
							return;
						}

						Debug.LogFormat("Changes to '{0}' or atlas detected. Clearing SkeletonDataAsset: {1}", skeletonJSONPath, localPath);
						SpineEditorUtilities.ClearSkeletonDataAsset(skeletonDataAsset);

						string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(skeletonDataAsset));
						string lastHash = EditorPrefs.GetString(guid + "_hash");

						// For some weird reason sometimes Unity loses the internal Object pointer,
						// and as a result, all comparisons with null returns true.
						// But the C# wrapper is still alive, so we can "restore" the object
						// by reloading it from its Instance ID.
						AtlasAssetBase[] skeletonDataAtlasAssets = skeletonDataAsset.atlasAssets;
						if (skeletonDataAtlasAssets != null) {
							for (int i = 0; i < skeletonDataAtlasAssets.Length; i++) {
								if (!ReferenceEquals(null, skeletonDataAtlasAssets[i]) &&
									skeletonDataAtlasAssets[i].Equals(null) &&
									skeletonDataAtlasAssets[i].GetInstanceID() != 0
								) {
									skeletonDataAtlasAssets[i] = EditorUtility.InstanceIDToObject(skeletonDataAtlasAssets[i].GetInstanceID()) as AtlasAssetBase;
								}
							}
						}

						SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
						if (skeletonData != null)
							BlendModeMaterialsUtility.UpdateBlendModeMaterials(skeletonDataAsset, ref skeletonData);

						string currentHash = skeletonData != null ? skeletonData.Hash : null;

#if SPINE_SKELETONMECANIM
						if (currentHash == null || lastHash != currentHash)
							SkeletonBaker.UpdateMecanimClips(skeletonDataAsset);
#endif

						// if (currentHash == null || lastHash != currentHash)
						// Do any upkeep on synchronized assets

						if (currentHash != null)
							EditorPrefs.SetString(guid + "_hash", currentHash);
					}
					SpineEditorUtilities.DataReloadHandler.ReloadSceneSkeletonComponents(skeletonDataAsset);
					SpineEditorUtilities.DataReloadHandler.ReloadAnimationReferenceAssets(skeletonDataAsset);
				}
			}
		}

		#region Import Atlases
		static List<AtlasAssetBase> FindAtlasesAtPath (string path) {
			List<AtlasAssetBase> arr = new List<AtlasAssetBase>();
			DirectoryInfo dir = new DirectoryInfo(path);
			FileInfo[] assetInfoArr = dir.GetFiles("*.asset");

			int subLen = Application.dataPath.Length - 6;
			foreach (var f in assetInfoArr) {
				string assetRelativePath = f.FullName.Substring(subLen, f.FullName.Length - subLen).Replace("\\", "/");
				Object obj = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(AtlasAssetBase));
				if (obj != null)
					arr.Add(obj as AtlasAssetBase);
			}

			return arr;
		}

		static AtlasAssetBase IngestSpineAtlas (TextAsset atlasText, List<string> texturesWithoutMetaFile) {
			if (atlasText == null) {
				Debug.LogWarning("Atlas source cannot be null!");
				return null;
			}

			string primaryName = Path.GetFileNameWithoutExtension(atlasText.name).Replace(".atlas", "");
			string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(atlasText)).Replace('\\', '/');

			string atlasPath = assetPath + "/" + primaryName + AtlasSuffix + ".asset";

			SpineAtlasAsset atlasAsset = (SpineAtlasAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(SpineAtlasAsset));

			List<Material> vestigialMaterials = new List<Material>();

			if (atlasAsset == null)
				atlasAsset = SpineAtlasAsset.CreateInstance<SpineAtlasAsset>();
			else {
				foreach (Material m in atlasAsset.materials)
					vestigialMaterials.Add(m);
			}

			protectFromStackGarbageCollection.Add(atlasAsset);
			atlasAsset.atlasFile = atlasText;

			List<string> pageFiles = new List<string>();
			Atlas atlas = atlasAsset.GetAtlas(onlyMetaData: true);
			if (atlas != null) {
				foreach (var page in atlas.Pages)
					pageFiles.Add(page.name);
			}

			var populatingMaterials = new List<Material>(pageFiles.Count);

			for (int i = 0; i < pageFiles.Count; i++) {
				string texturePath = assetPath + "/" + pageFiles[i];
				Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
				bool textureIsUninitialized = texturesWithoutMetaFile != null && texturesWithoutMetaFile.Contains(texturePath);
				if (SpineEditorUtilities.Preferences.setTextureImporterSettings && textureIsUninitialized) {
					if (string.IsNullOrEmpty(SpineEditorUtilities.Preferences.textureSettingsReference))
						SetDefaultTextureSettings(texturePath, atlasAsset);
					else
						SetReferenceTextureSettings(texturePath, atlasAsset, SpineEditorUtilities.Preferences.textureSettingsReference);
				}

				string pageName = Path.GetFileNameWithoutExtension(pageFiles[i]);

				//because this looks silly
				if (pageName == primaryName && pageFiles.Count == 1)
					pageName = "Material";

				string materialPath = assetPath + "/" + primaryName + "_" + pageName + ".mat";
				Material material = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));

				if (material == null) {
					Shader defaultShader = Shader.Find(SpineEditorUtilities.Preferences.DefaultShader);
					material = defaultShader != null ? new Material(defaultShader) : null;
					if (material) {
						ApplyPMAOrStraightAlphaSettings(material, SpineEditorUtilities.Preferences.textureSettingsReference);
						if (texture != null)
							material.mainTexture = texture;
						AssetDatabase.CreateAsset(material, materialPath);
					}
				} else {
					vestigialMaterials.Remove(material);
					if (texture != null)
						material.mainTexture = texture;
					EditorUtility.SetDirty(material);
					// note: don't call AssetDatabase.SaveAssets() since this would trigger OnPostprocessAllAssets() every time unnecessarily.
				}

				if (material != null) {
					populatingMaterials.Add(material);
				}
			}

			atlasAsset.materials = populatingMaterials.ToArray();

			for (int i = 0; i < vestigialMaterials.Count; i++)
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(vestigialMaterials[i]));

			if (AssetDatabase.GetAssetPath(atlasAsset) == "")
				AssetDatabase.CreateAsset(atlasAsset, atlasPath);
			else
				atlasAsset.Clear();

			EditorUtility.SetDirty(atlasAsset);
			AssetDatabase.SaveAssets();

			if (pageFiles.Count != atlasAsset.materials.Length)
				Debug.LogWarning(string.Format("{0} :: Not all atlas pages were imported. If you rename your image files, please make sure you also edit the filenames specified in the atlas file.", atlasAsset.name), atlasAsset);
			else
				Debug.Log(string.Format("{0} :: Imported with {1} material", atlasAsset.name, atlasAsset.materials.Length), atlasAsset);

			// Iterate regions and bake marked.
			atlasAsset.Clear();
			atlas = atlasAsset.GetAtlas(onlyMetaData: false);
			if (atlas != null) {
				FieldInfo field = typeof(Atlas).GetField("regions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic);
				var regions = (List<AtlasRegion>)field.GetValue(atlas);
				string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
				string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath).Replace('\\', '/');
				string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);

				bool hasBakedRegions = false;
				for (int i = 0; i < regions.Count; i++) {
					AtlasRegion region = regions[i];
					string bakedPrefabPath = Path.Combine(bakedDirPath, AssetUtility.GetPathSafeName(region.name) + ".prefab").Replace("\\", "/");
					GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(bakedPrefabPath, typeof(GameObject));
					if (prefab != null) {
						SkeletonBaker.BakeRegion(atlasAsset, region, false);
						hasBakedRegions = true;
					}
				}

				if (hasBakedRegions) {
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}

			protectFromStackGarbageCollection.Remove(atlasAsset);
			// note: at Asset Pipeline V2 this LoadAssetAtPath of the just created
			// asset returns null, regardless of refresh calls.
			var loadedAtlas = (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(AtlasAssetBase));
			return loadedAtlas != null ? loadedAtlas : atlasAsset;
		}

		public static bool SpriteAtlasSettingsNeedAdjustment (UnityEngine.U2D.SpriteAtlas spriteAtlas) {
#if EXPOSES_SPRITE_ATLAS_UTILITIES
			UnityEditor.U2D.SpriteAtlasPackingSettings packingSettings = UnityEditor.U2D.SpriteAtlasExtensions.GetPackingSettings(spriteAtlas);
			UnityEditor.U2D.SpriteAtlasTextureSettings textureSettings = UnityEditor.U2D.SpriteAtlasExtensions.GetTextureSettings(spriteAtlas);

			bool areSettingsAsDesired =
				packingSettings.enableRotation == true &&
				packingSettings.enableTightPacking == false &&
				textureSettings.readable == true &&
				textureSettings.generateMipMaps == false;
			// note: platformSettings.textureCompression is always providing "Compressed", so we have to skip it.
			return !areSettingsAsDesired;
#else
			return false;
#endif
		}

		public static bool AdjustSpriteAtlasSettings (UnityEngine.U2D.SpriteAtlas spriteAtlas) {
#if EXPOSES_SPRITE_ATLAS_UTILITIES
			UnityEditor.U2D.SpriteAtlasPackingSettings packingSettings = UnityEditor.U2D.SpriteAtlasExtensions.GetPackingSettings(spriteAtlas);
			UnityEditor.U2D.SpriteAtlasTextureSettings textureSettings = UnityEditor.U2D.SpriteAtlasExtensions.GetTextureSettings(spriteAtlas);

			packingSettings.enableRotation = true;
			packingSettings.enableTightPacking = false;
			UnityEditor.U2D.SpriteAtlasExtensions.SetPackingSettings(spriteAtlas, packingSettings);

			textureSettings.readable = true;
			textureSettings.generateMipMaps = false;
			UnityEditor.U2D.SpriteAtlasExtensions.SetTextureSettings(spriteAtlas, textureSettings);

			TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
			platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
			platformSettings.crunchedCompression = false;
			UnityEditor.U2D.SpriteAtlasExtensions.SetPlatformSettings(spriteAtlas, platformSettings);

			string atlasPath = AssetDatabase.GetAssetPath(spriteAtlas);
			Debug.Log(string.Format("Adjusted unsuitable SpriteAtlas settings '{0}'", atlasPath), spriteAtlas);
			return false;
#else
			return true;
#endif
		}

		public static bool GeneratePngFromSpriteAtlas (UnityEngine.U2D.SpriteAtlas spriteAtlas, out string texturePath) {
			texturePath = System.IO.Path.ChangeExtension(AssetDatabase.GetAssetPath(spriteAtlas), ".png");
			if (spriteAtlas == null)
				return false;

			Texture2D tempTexture = SpineSpriteAtlasAsset.AccessPackedTextureEditor(spriteAtlas);
			if (tempTexture == null)
				return false;

			byte[] bytes = null;
			try {
				bytes = tempTexture.EncodeToPNG();
			} catch (System.Exception) {
				// handled below
			}
			if (bytes == null || bytes.Length == 0) {
				Debug.LogError("Could not read Compressed SpriteAtlas. Please enable 'Read/Write Enabled' and ensure 'Compression' is set to 'None' in Inspector.", spriteAtlas);
				return false;
			}
			System.IO.File.WriteAllBytes(texturePath, bytes);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			return System.IO.File.Exists(texturePath);
		}

		public static AtlasAssetBase IngestSpriteAtlas (UnityEngine.U2D.SpriteAtlas spriteAtlas, List<string> texturesWithoutMetaFile) {
			if (spriteAtlas == null) {
				Debug.LogWarning("SpriteAtlas source cannot be null!");
				return null;
			}

			if (SpriteAtlasSettingsNeedAdjustment(spriteAtlas)) {
				// settings need to be adjusted via the 'Spine SpriteAtlas Import' window if you want to use it as a Spine atlas.
				return null;
			}

			Texture2D texture = null;
			{ // only one page file
				string texturePath;
				GeneratePngFromSpriteAtlas(spriteAtlas, out texturePath);
				texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
				if (texture == null && System.IO.File.Exists(texturePath)) {
					EditorUtility.SetDirty(spriteAtlas);
					return null; // next iteration will load the texture as well.
				}
			}

			string primaryName = spriteAtlas.name;
			string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spriteAtlas)).Replace('\\', '/');

			string atlasPath = assetPath + "/" + primaryName + SpriteAtlasSuffix + ".asset";

			SpineSpriteAtlasAsset atlasAsset = AssetDatabase.LoadAssetAtPath<SpineSpriteAtlasAsset>(atlasPath);

			List<Material> vestigialMaterials = new List<Material>();

			if (atlasAsset == null)
				atlasAsset = SpineSpriteAtlasAsset.CreateInstance<SpineSpriteAtlasAsset>();
			else {
				foreach (Material m in atlasAsset.materials)
					vestigialMaterials.Add(m);
			}

			protectFromStackGarbageCollection.Add(atlasAsset);
			atlasAsset.spriteAtlasFile = spriteAtlas;

			int pagesCount = 1;
			var populatingMaterials = new List<Material>(pagesCount);

			{
				string pageName = "SpriteAtlas";

				string materialPath = assetPath + "/" + primaryName + "_" + pageName + ".mat";
				Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

				if (mat == null) {
					mat = new Material(Shader.Find(SpineEditorUtilities.Preferences.defaultShader));
					ApplyPMAOrStraightAlphaSettings(mat, SpineEditorUtilities.Preferences.textureSettingsReference);
					AssetDatabase.CreateAsset(mat, materialPath);
				} else {
					vestigialMaterials.Remove(mat);
				}

				if (texture != null)
					mat.mainTexture = texture;

				EditorUtility.SetDirty(mat);
				// note: don't call AssetDatabase.SaveAssets() since this would trigger OnPostprocessAllAssets() every time unnecessarily.
				populatingMaterials.Add(mat); //atlasAsset.materials[i] = mat;
			}

			atlasAsset.materials = populatingMaterials.ToArray();

			for (int i = 0; i < vestigialMaterials.Count; i++)
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(vestigialMaterials[i]));

			if (AssetDatabase.GetAssetPath(atlasAsset) == "")
				AssetDatabase.CreateAsset(atlasAsset, atlasPath);
			else
				atlasAsset.Clear();

			atlasAsset.GetAtlas();
			atlasAsset.updateRegionsInPlayMode = true;

			EditorUtility.SetDirty(atlasAsset);
			AssetDatabase.SaveAssets();

			Debug.Log(string.Format("{0} :: Imported with {1} material", atlasAsset.name, atlasAsset.materials.Length), atlasAsset);

			protectFromStackGarbageCollection.Remove(atlasAsset);
			return (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(AtlasAssetBase));
		}

		static bool SetDefaultTextureSettings (string texturePath, SpineAtlasAsset atlasAsset) {
			TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
			if (texImporter == null) {
				Debug.LogWarning(string.Format("{0}: Texture asset \"{1}\" not found. Skipping. Please check your atlas file for renamed files.", atlasAsset.name, texturePath), atlasAsset);
				return false;
			}

			texImporter.textureCompression = TextureImporterCompression.Uncompressed;
			texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
			texImporter.mipmapEnabled = false;
			texImporter.alphaIsTransparency = false; // Prevent the texture importer from applying bleed to the transparent parts for PMA.
			texImporter.spriteImportMode = SpriteImportMode.None;
			texImporter.maxTextureSize = 2048;

			EditorUtility.SetDirty(texImporter);
			AssetDatabase.ImportAsset(texturePath);
			AssetDatabase.SaveAssets();
			return true;
		}

#if NEW_PREFERENCES_SETTINGS_PROVIDER
		static bool SetReferenceTextureSettings (string texturePath, SpineAtlasAsset atlasAsset, string referenceAssetPath) {
			var texturePreset = AssetDatabase.LoadAssetAtPath<UnityEditor.Presets.Preset>(referenceAssetPath);
			bool isTexturePreset = texturePreset != null && texturePreset.GetTargetTypeName() == "TextureImporter";
			if (!isTexturePreset)
				return SetDefaultTextureSettings(texturePath, atlasAsset);

			TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
			if (texImporter == null) {
				Debug.LogWarning(string.Format("{0}: Texture asset \"{1}\" not found. Skipping. Please check your atlas file for renamed files.", atlasAsset.name, texturePath), atlasAsset);
				return false;
			}

			texturePreset.ApplyTo(texImporter);
			AssetDatabase.ImportAsset(texturePath);
			AssetDatabase.SaveAssets();
			return true;
		}
#else
		static bool SetReferenceTextureSettings (string texturePath, SpineAtlasAsset atlasAsset, string referenceAssetPath) {
			TextureImporter reference = TextureImporter.GetAtPath(referenceAssetPath) as TextureImporter;
			if (reference == null)
				return SetDefaultTextureSettings(texturePath, atlasAsset);

			TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
			if (texImporter == null) {
				Debug.LogWarning(string.Format("{0}: Texture asset \"{1}\" not found. Skipping. Please check your atlas file for renamed files.", atlasAsset.name, texturePath), atlasAsset);
				return false;
			}

			texImporter.sRGBTexture = reference.sRGBTexture;
			texImporter.textureCompression = reference.textureCompression;
			texImporter.alphaSource = reference.alphaSource;
			texImporter.mipmapEnabled = reference.mipmapEnabled;
			texImporter.alphaIsTransparency = reference.alphaIsTransparency;
			texImporter.spriteImportMode = reference.spriteImportMode;
			texImporter.maxTextureSize = reference.maxTextureSize;
			texImporter.isReadable = reference.isReadable;
			texImporter.filterMode = reference.filterMode;
			texImporter.mipmapFilter = reference.mipmapFilter;
			texImporter.textureType = reference.textureType;

			EditorUtility.SetDirty(texImporter);
			AssetDatabase.ImportAsset(texturePath);
			AssetDatabase.SaveAssets();
			return true;
		}
#endif

		static void ApplyPMAOrStraightAlphaSettings (Material material, string referenceTextureSettings) {
			bool isUsingPMAWorkflow = string.IsNullOrEmpty(referenceTextureSettings) ||
				(!referenceTextureSettings.ToLower().Contains("straight") && referenceTextureSettings.ToLower().Contains("pma"));

			MaterialChecks.EnablePMATextureAtMaterial(material, isUsingPMAWorkflow);
		}
		#endregion

		#region Import SkeletonData (json or binary)
		internal static string GetSkeletonDataAssetFilePath (TextAsset spineJson) {
			string primaryName = Path.GetFileNameWithoutExtension(spineJson.name);
			string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spineJson)).Replace('\\', '/');
			return assetPath + "/" + primaryName + SkeletonDataSuffix + ".asset";
		}

		internal static SkeletonDataAsset IngestIncompatibleSpineProject (TextAsset spineJson,
			CompatibilityProblemInfo compatibilityProblemInfo) {

			if (spineJson == null)
				return null;

			string filePath = GetSkeletonDataAssetFilePath(spineJson);
			SkeletonDataAsset skeletonDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(SkeletonDataAsset));
			if (skeletonDataAsset == null) {
				skeletonDataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
				skeletonDataAsset.skeletonJSON = spineJson;
				AssetDatabase.CreateAsset(skeletonDataAsset, filePath);
			}
			EditorUtility.SetDirty(skeletonDataAsset);

			SkeletonDataCompatibility.DisplayCompatibilityProblem(compatibilityProblemInfo.DescriptionString(), spineJson);
			return skeletonDataAsset;
		}

		internal static SkeletonDataAsset IngestSpineProject (TextAsset spineJson, params AtlasAssetBase[] atlasAssets) {
			string filePath = GetSkeletonDataAssetFilePath(spineJson);

#if SPINE_TK2D
			if (spineJson != null) {
				SkeletonDataAsset skeletonDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(SkeletonDataAsset));
				if (skeletonDataAsset == null) {
					skeletonDataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
					skeletonDataAsset.skeletonJSON = spineJson;
					skeletonDataAsset.fromAnimation = new string[0];
					skeletonDataAsset.toAnimation = new string[0];
					skeletonDataAsset.duration = new float[0];
					skeletonDataAsset.defaultMix = SpineEditorUtilities.Preferences.defaultMix;
					skeletonDataAsset.scale = SpineEditorUtilities.Preferences.defaultScale;

					AssetDatabase.CreateAsset(skeletonDataAsset, filePath);
					AssetDatabase.SaveAssets();
				} else {
					skeletonDataAsset.Clear();
					skeletonDataAsset.GetSkeletonData(true);
				}

				return skeletonDataAsset;
			} else {
				EditorUtility.DisplayDialog("Error!", "Tried to ingest null Spine data.", "OK");
				return null;
			}

#else
			if (spineJson != null && atlasAssets != null) {
				SkeletonDataAsset skeletonDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(SkeletonDataAsset));
				if (skeletonDataAsset == null) {
					skeletonDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();
					{
						skeletonDataAsset.atlasAssets = atlasAssets;
						skeletonDataAsset.skeletonJSON = spineJson;
						skeletonDataAsset.defaultMix = SpineEditorUtilities.Preferences.defaultMix;
						skeletonDataAsset.scale = SpineEditorUtilities.Preferences.defaultScale;
						skeletonDataAsset.blendModeMaterials.applyAdditiveMaterial = !SpineEditorUtilities.Preferences.UsesPMAWorkflow;
					}
					AssetDatabase.CreateAsset(skeletonDataAsset, filePath);
				} else {
					skeletonDataAsset.atlasAssets = atlasAssets;
					SpineEditorUtilities.ClearSkeletonDataAsset(skeletonDataAsset);
				}
				var skeletonData = skeletonDataAsset.GetSkeletonData(true);
				if (skeletonData != null)
					BlendModeMaterialsUtility.UpdateBlendModeMaterials(skeletonDataAsset, ref skeletonData);
				AssetDatabase.SaveAssets();

				return skeletonDataAsset;
			} else {
				EditorUtility.DisplayDialog("Error!", "Must specify both Spine JSON and AtlasAsset array", "OK");
				return null;
			}
#endif
		}
		#endregion

		#region Spine Skeleton Data File Validation
		public static bool CheckForValidSkeletonData (string skeletonJSONPath) {
			string dir = Path.GetDirectoryName(skeletonJSONPath).Replace('\\', '/');
			TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonJSONPath);
			DirectoryInfo dirInfo = new DirectoryInfo(dir);
			FileInfo[] files = dirInfo.GetFiles("*.asset");

			foreach (var path in files) {
				string localPath = dir + "/" + path.Name;
				var obj = AssetDatabase.LoadAssetAtPath(localPath, typeof(Object));
				var skeletonDataAsset = obj as SkeletonDataAsset;
				if (skeletonDataAsset != null && skeletonDataAsset.skeletonJSON == textAsset)
					return true;
			}
			return false;
		}

		public static bool IsSpineData (TextAsset asset, out CompatibilityProblemInfo compatibilityProblemInfo, ref string problemDescription) {
			bool isSpineSkeletonData;
			SkeletonDataCompatibility.VersionInfo fileVersion = SkeletonDataCompatibility.GetVersionInfo(asset, out isSpineSkeletonData, ref problemDescription);
			compatibilityProblemInfo = SkeletonDataCompatibility.GetCompatibilityProblemInfo(fileVersion);
			return isSpineSkeletonData;
		}
		#endregion

		#region Dialogs
		public static void SkeletonImportDialog (string skeletonPath, List<AtlasAssetBase> localAtlases, List<string> requiredPaths, ref bool abortSkeletonImport) {
			bool resolved = false;
			while (!resolved) {

				string filename = Path.GetFileNameWithoutExtension(skeletonPath);
				int result = EditorUtility.DisplayDialogComplex(
					string.Format("AtlasAsset for \"{0}\"", filename),
					string.Format("Could not automatically set the AtlasAsset for \"{0}\".\n\n (You may resolve this manually later.)", filename),
					"Resolve atlases...", "Import without atlases", "Stop importing"
				);

				switch (result) {
				case -1: { // Select Atlas
					string pathForwardSlash = Path.GetDirectoryName(skeletonPath).Replace('\\', '/');
					AtlasAssetBase selectedAtlas = BrowseAtlasDialog(pathForwardSlash, localAtlases);
					if (selectedAtlas != null) {
						localAtlases.Clear();
						localAtlases.Add(selectedAtlas);
						var atlasMatch = AssetUtility.GetMatchingAtlas(requiredPaths, localAtlases);
						if (atlasMatch != null) {
							resolved = true;
							AssetUtility.IngestSpineProject(AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonPath), atlasMatch);
						}
					}
					break;
				}
				case 0: { // Resolve AtlasAssets...
					string pathForwardSlash = Path.GetDirectoryName(skeletonPath).Replace('\\', '/');
					var atlasList = MultiAtlasDialog(requiredPaths, pathForwardSlash,
						localAtlases, filename);
					if (atlasList != null)
						AssetUtility.IngestSpineProject(AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonPath), atlasList.ToArray());

					resolved = true;
					break;
				}
				case 1: // Import without atlas
					Debug.LogWarning("Imported with missing atlases. Skeleton will not render: " + Path.GetFileName(skeletonPath));
					AssetUtility.IngestSpineProject(AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonPath), new AtlasAssetBase[] { });
					resolved = true;
					break;
				case 2: // Stop importing all
					abortSkeletonImport = true;
					resolved = true;
					break;
				}
			}
		}

		public static List<AtlasAssetBase> MultiAtlasDialog (List<string> requiredPaths, string initialDirectory,
			List<AtlasAssetBase> localAtlases, string filename = "") {

			List<AtlasAssetBase> atlasAssets = new List<AtlasAssetBase>();
			bool resolved = false;
			string lastAtlasPath = initialDirectory;
			while (!resolved) {

				// Build dialog box message.
				var missingRegions = new List<string>(requiredPaths);
				var dialogText = new StringBuilder();
				{
					dialogText.AppendLine(string.Format("SkeletonDataAsset for \"{0}\"", filename));
					dialogText.AppendLine("has missing regions.");
					dialogText.AppendLine();
					dialogText.AppendLine("Current Atlases:");

					if (atlasAssets.Count == 0)
						dialogText.AppendLine("\t--none--");

					for (int i = 0; i < atlasAssets.Count; i++)
						dialogText.AppendLine("\t" + atlasAssets[i].name);

					dialogText.AppendLine();
					dialogText.AppendLine("Missing Regions:");

					foreach (var atlasAsset in atlasAssets) {
						var atlas = atlasAsset.GetAtlas();
						for (int i = 0; i < missingRegions.Count; i++) {
							if (atlas.FindRegionIgnoringNumberSuffix(missingRegions[i]) != null) {
								missingRegions.RemoveAt(i);
								i--;
							}
						}
					}

					int n = missingRegions.Count;
					if (n == 0)
						break;

					const int MaxListLength = 15;
					for (int i = 0; (i < n && i < MaxListLength); i++)
						dialogText.AppendLine(string.Format("\t {0}", missingRegions[i]));

					if (n > MaxListLength)
						dialogText.AppendLine(string.Format("\t... {0} more...", n - MaxListLength));
				}

				// Show dialog box.
				int result = EditorUtility.DisplayDialogComplex(
					"SkeletonDataAsset has missing Atlas.",
					dialogText.ToString(),
					"Browse Atlas...", "Import anyway", "Cancel import"
				);

				switch (result) {
				case 0: // Browse...
					AtlasAssetBase selectedAtlasAsset = BrowseAtlasDialog(lastAtlasPath, localAtlases);
					if (selectedAtlasAsset != null) {
						if (!atlasAssets.Contains(selectedAtlasAsset)) {
							var atlas = selectedAtlasAsset.GetAtlas();
							bool hasValidRegion = false;
							foreach (string str in missingRegions) {
								if (atlas.FindRegionIgnoringNumberSuffix(str) != null) {
									hasValidRegion = true;
									break;
								}
							}
							atlasAssets.Add(selectedAtlasAsset);
						}
					}
					break;
				case 1: // Import anyway
					resolved = true;
					break;
				case 2: // Cancel
					atlasAssets = null;
					resolved = true;
					break;
				}
			}

			return atlasAssets;
		}

		public static AtlasAssetBase BrowseAtlasDialog (string dirPath, List<AtlasAssetBase> localAtlases) {
			string path = EditorUtility.OpenFilePanel("Select AtlasAsset...", dirPath, "asset");
			if (path == "")
				return null; // Canceled or closed by user.

			int subLen = Application.dataPath.Length - 6;
			string assetRelativePath = path.Substring(subLen, path.Length - subLen).Replace("\\", "/");

			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(AtlasAssetBase));
			if (obj == null) {
				// atlas assets that were just created fail to load, search localAtlases
				foreach (AtlasAssetBase localAtlas in localAtlases) {
					string newAtlasPath = AssetDatabase.GetAssetPath(localAtlas);
					if (newAtlasPath == assetRelativePath)
						return localAtlas;
				}
			}

			if (obj == null || !(obj is AtlasAssetBase)) {
				Debug.Log("Chosen asset was not of type AtlasAssetBase");
				return null;
			}
			return (AtlasAssetBase)obj;
		}
		#endregion

		public static string GetPathSafeName (string name) {
			foreach (char c in System.IO.Path.GetInvalidFileNameChars()) { // Doesn't handle more obscure file name limitations.
				name = name.Replace(c, '_');
			}
			return name;
		}
	}

	public static class EditorInstantiation {
		public delegate Component InstantiateDelegate (SkeletonDataAsset skeletonDataAsset);

		public class SkeletonComponentSpawnType {
			public string menuLabel;
			public InstantiateDelegate instantiateDelegate;
			public bool isUI;
		}

		internal static readonly List<SkeletonComponentSpawnType> additionalSpawnTypes = new List<SkeletonComponentSpawnType>();

		public static void TryInitializeSkeletonRendererSettings (SkeletonRenderer skeletonRenderer, Skin skin = null) {
			const string PMAShaderQuery = "Spine/";
			const string TintBlackShaderQuery = "Tint Black";

			if (skeletonRenderer == null) return;
			var skeletonDataAsset = skeletonRenderer.skeletonDataAsset;
			if (skeletonDataAsset == null) return;

			bool pmaVertexColors = false;
			bool tintBlack = false;
			foreach (AtlasAssetBase atlasAsset in skeletonDataAsset.atlasAssets) {
				if (!pmaVertexColors) {
					foreach (Material m in atlasAsset.Materials) {
						if (m.shader.name.Contains(PMAShaderQuery)) {
							pmaVertexColors = true;
							break;
						}
					}
				}

				if (!tintBlack) {
					foreach (Material m in atlasAsset.Materials) {
						if (m.shader.name.Contains(TintBlackShaderQuery)) {
							tintBlack = true;
							break;
						}
					}
				}
			}

			skeletonRenderer.pmaVertexColors = pmaVertexColors;
			skeletonRenderer.tintBlack = tintBlack;
			skeletonRenderer.zSpacing = SpineEditorUtilities.Preferences.defaultZSpacing;

			var data = skeletonDataAsset.GetSkeletonData(false);
			bool noSkins = data.DefaultSkin == null && (data.Skins == null || data.Skins.Count == 0); // Support attachmentless/skinless SkeletonData.
			skin = skin ?? data.DefaultSkin ?? (noSkins ? null : data.Skins.Items[0]);
			if (skin != null && skin != data.DefaultSkin) {
				skeletonRenderer.initialSkinName = skin.Name;
			}
		}

		public static SkeletonAnimation InstantiateSkeletonAnimation (SkeletonDataAsset skeletonDataAsset, string skinName,
			bool destroyInvalid = true, bool useObjectFactory = true) {

			var skeletonData = skeletonDataAsset.GetSkeletonData(true);
			var skin = skeletonData != null ? skeletonData.FindSkin(skinName) : null;
			return InstantiateSkeletonAnimation(skeletonDataAsset, skin, destroyInvalid, useObjectFactory);
		}

		public static SkeletonAnimation InstantiateSkeletonAnimation (SkeletonDataAsset skeletonDataAsset, Skin skin = null,
			bool destroyInvalid = true, bool useObjectFactory = true) {

			SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

			if (data == null) {
				for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
					string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
					skeletonDataAsset.atlasAssets[i] = (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAssetBase));
				}
				data = skeletonDataAsset.GetSkeletonData(false);
			}

			if (data == null) {
				Debug.LogWarning("InstantiateSkeletonAnimation tried to instantiate a skeleton from an invalid SkeletonDataAsset.", skeletonDataAsset);
				return null;
			}

			string spineGameObjectName = string.Format("Spine GameObject ({0})", skeletonDataAsset.name.Replace(AssetUtility.SkeletonDataSuffix, ""));
			GameObject go = EditorInstantiation.NewGameObject(spineGameObjectName, useObjectFactory,
				typeof(MeshFilter), typeof(MeshRenderer), typeof(SkeletonAnimation));
			SkeletonAnimation newSkeletonAnimation = go.GetComponent<SkeletonAnimation>();
			newSkeletonAnimation.skeletonDataAsset = skeletonDataAsset;
			TryInitializeSkeletonRendererSettings(newSkeletonAnimation, skin);

			// Initialize
			try {
				newSkeletonAnimation.Initialize(false);
			} catch (System.Exception e) {
				if (destroyInvalid) {
					Debug.LogWarning("Editor-instantiated SkeletonAnimation threw an Exception. Destroying GameObject to prevent orphaned GameObject.\n" + e.Message, skeletonDataAsset);
					GameObject.DestroyImmediate(go);
				}
				throw e;
			}

			newSkeletonAnimation.loop = SpineEditorUtilities.Preferences.defaultInstantiateLoop;
			newSkeletonAnimation.state.Update(0);
			newSkeletonAnimation.state.Apply(newSkeletonAnimation.skeleton);
			newSkeletonAnimation.skeleton.UpdateWorldTransform();

			return newSkeletonAnimation;
		}

		/// <summary>Handles creating a new GameObject in the Unity Editor. This uses the new ObjectFactory API where applicable.</summary>
		public static GameObject NewGameObject (string name, bool useObjectFactory) {
#if NEW_PREFAB_SYSTEM
			if (useObjectFactory)
				return ObjectFactory.CreateGameObject(name);
#endif
			return new GameObject(name);
		}

		/// <summary>Handles creating a new GameObject in the Unity Editor. This uses the new ObjectFactory API where applicable.</summary>
		public static GameObject NewGameObject (string name, bool useObjectFactory, params System.Type[] components) {
#if NEW_PREFAB_SYSTEM
			if (useObjectFactory)
				return ObjectFactory.CreateGameObject(name, components);
#endif
			return new GameObject(name, components);
		}

		public static void InstantiateEmptySpineGameObject<T> (string name, bool useObjectFactory) where T : MonoBehaviour {
			var parentGameObject = Selection.activeObject as GameObject;
			var parentTransform = parentGameObject == null ? null : parentGameObject.transform;

			var gameObject = EditorInstantiation.NewGameObject(name, useObjectFactory, typeof(T));
			gameObject.transform.SetParent(parentTransform, false);
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = gameObject;
			EditorGUIUtility.PingObject(Selection.activeObject);
		}

		#region SkeletonMecanim
#if SPINE_SKELETONMECANIM
		public static SkeletonMecanim InstantiateSkeletonMecanim (SkeletonDataAsset skeletonDataAsset, string skinName) {
			return InstantiateSkeletonMecanim(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
		}

		public static SkeletonMecanim InstantiateSkeletonMecanim (SkeletonDataAsset skeletonDataAsset, Skin skin = null,
			bool destroyInvalid = true, bool useObjectFactory = true) {
			SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

			if (data == null) {
				for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
					string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
					skeletonDataAsset.atlasAssets[i] = (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAssetBase));
				}
				data = skeletonDataAsset.GetSkeletonData(false);
			}

			if (data == null) {
				Debug.LogWarning("InstantiateSkeletonMecanim tried to instantiate a skeleton from an invalid SkeletonDataAsset.", skeletonDataAsset);
				return null;
			}

			string spineGameObjectName = string.Format("Spine Mecanim GameObject ({0})", skeletonDataAsset.name.Replace(AssetUtility.SkeletonDataSuffix, ""));
			GameObject go = EditorInstantiation.NewGameObject(spineGameObjectName, useObjectFactory,
				typeof(MeshFilter), typeof(MeshRenderer), typeof(Animator), typeof(SkeletonMecanim));

			if (skeletonDataAsset.controller == null) {
				SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
				Debug.Log(string.Format("Mecanim controller was automatically generated and assigned for {0}", skeletonDataAsset.name), skeletonDataAsset);
			}

			go.GetComponent<Animator>().runtimeAnimatorController = skeletonDataAsset.controller;

			SkeletonMecanim newSkeletonMecanim = go.GetComponent<SkeletonMecanim>();
			newSkeletonMecanim.skeletonDataAsset = skeletonDataAsset;
			TryInitializeSkeletonRendererSettings(newSkeletonMecanim, skin);

			// Initialize
			try {
				newSkeletonMecanim.Initialize(false);
			} catch (System.Exception e) {
				if (destroyInvalid) {
					Debug.LogWarning("Editor-instantiated SkeletonAnimation threw an Exception. Destroying GameObject to prevent orphaned GameObject.", skeletonDataAsset);
					GameObject.DestroyImmediate(go);
				}
				throw e;
			}

			newSkeletonMecanim.skeleton.UpdateWorldTransform();
			newSkeletonMecanim.LateUpdate();

			return newSkeletonMecanim;
		}
#endif
		#endregion
	}
}
