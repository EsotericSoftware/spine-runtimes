/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#define SPINE_OPTIONAL_ON_DEMAND_LOADING

#if SPINE_OPTIONAL_ON_DEMAND_LOADING

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Spine.Unity.Editor {

	using GenericTextureLoader = GenericOnDemandTextureLoader<AddressableTextureReference, AddressableRequest>;
	using GenericTextureLoaderInspector = GenericOnDemandTextureLoaderInspector<AddressableTextureReference, AddressableRequest>;
	using PlaceholderMaterialMapping = AddressablesTextureLoader.PlaceholderMaterialMapping;
	using PlaceholderTextureMapping = AddressablesTextureLoader.PlaceholderTextureMapping;

	[InitializeOnLoad]
	[CustomEditor(typeof(AddressablesTextureLoader)), CanEditMultipleObjects]
	public class AddressablesTextureLoaderInspector : GenericTextureLoaderInspector {

		// Note: This static ctor ensures the generic base class method RegisterPlayModeChangedCallbacks is
		// definitely called via InitializeOnLoad. Otherwise problems arose where the base class static ctor code
		// is not executed (related to being a generic class).
		static AddressablesTextureLoaderInspector () {
			// The call below is necessary, otherwise the static GenericTextureLoaderInspector ctor is not called.
			GenericTextureLoaderInspector.RegisterPlayModeChangedCallbacks();
		}

		public class AddressablesMethodImplementations : StaticMethodImplementations {
			public override string LoaderSuffix { get { return "_Addressable"; } }

			public override GenericTextureLoader GetOrCreateLoader (string loaderPath) {
				AddressablesTextureLoader loader = AssetDatabase.LoadAssetAtPath<AddressablesTextureLoader>(loaderPath);
				if (loader == null) {
					loader = AddressablesTextureLoader.CreateInstance<AddressablesTextureLoader>();
					AssetDatabase.CreateAsset(loader, loaderPath);
					loader = AssetDatabase.LoadAssetAtPath<AddressablesTextureLoader>(loaderPath);
				} else {
					loader.Clear(clearAtlasAsset: false);
				}
				return loader;
			}

			public override bool SetupOnDemandLoadingReference (
				ref AddressableTextureReference targetTextureReference, Texture targetTexture) {

				string targetTextureGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(targetTexture));
				if (string.IsNullOrEmpty(targetTextureGUID))
					return false;
				targetTextureReference.assetReference = new AssetReferenceTexture(targetTextureGUID);
				return targetTextureReference.assetReference.IsValid();
			}
		}

		#region Context Menu Item
		[MenuItem("CONTEXT/AtlasAssetBase/Add Addressables Loader")]
		static void AddAddressablesLoader (MenuCommand cmd) {
			if (staticMethods == null)
				staticMethods = new AddressablesMethodImplementations();
			staticMethods.AddOnDemandLoader(cmd);
		}
		#endregion

		protected override StaticMethodImplementations CreateStaticMethodImplementations () {
			return new AddressablesMethodImplementations();
		}

		protected override void DrawSingleLineTargetTextureProperty (SerializedProperty property) {
			EditorGUILayout.PropertyField(property.FindPropertyRelative("assetReference"), GUIContent.none, true);
		}
	}
}
#endif
