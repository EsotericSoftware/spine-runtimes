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

#if UNITY_2022_2_OR_NEWER
#define USE_FIND_OBJECTS_BY_TYPE
#endif

#define SPINE_OPTIONAL_ON_DEMAND_LOADING

#if SPINE_OPTIONAL_ON_DEMAND_LOADING

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {

	[DefaultExecutionOrder(1000)]
	public class OnDemandTextureLoaderCleanup : MonoBehaviour {

		private static OnDemandTextureLoaderCleanup singletonInstance;

		struct LoaderListModification {
			public bool isAdd;
			public IDeferredCleanupOnDemandTextureLoader loader;
		}

		readonly List<IDeferredCleanupOnDemandTextureLoader> loaders = new List<IDeferredCleanupOnDemandTextureLoader>();
		readonly List<LoaderListModification> loaderModifications = new List<LoaderListModification>();
		bool isProcessingLoaders = false;

		public static OnDemandTextureLoaderCleanup Instance {
			get {
				if (singletonInstance == null) {
#if USE_FIND_OBJECTS_BY_TYPE
					singletonInstance = FindFirstObjectByType<OnDemandTextureLoaderCleanup>();
#else
					singletonInstance = FindObjectOfType<OnDemandTextureLoaderCleanup>();
#endif
					if (singletonInstance == null) {
						GameObject singletonGameObject = new GameObject("OnDemandTextureLoaderCleanup");
						singletonInstance = singletonGameObject.AddComponent<OnDemandTextureLoaderCleanup>();
						DontDestroyOnLoad(singletonGameObject);
						singletonGameObject.hideFlags = HideFlags.DontSave;
					}
				}
				return singletonInstance;
			}
		}

		private void Awake () {
			if (singletonInstance == null) {
				singletonInstance = this;
				DontDestroyOnLoad(gameObject);
			}
			if (singletonInstance != null && singletonInstance != this) {
				Debug.LogWarning("Multiple OnDemandTextureLoaderCleanup singleton GameObjects found! " +
					"Don't manually add OnDemandTextureLoaderCleanup to each scene, it is created automatically when needed.");
				Destroy(gameObject);
			}
		}

		public void LateUpdate () {
			if (loaders.Count == 0) return;

			isProcessingLoaders = true;
			try {
				for (int i = loaders.Count - 1; i >= 0; --i) {
					IDeferredCleanupOnDemandTextureLoader loader = loaders[i];
					UnityEngine.Object unityObject = loader != null ? loader.UnityObject : null;
					if (!unityObject || !loader.HasUnreleasedRequests) {
						UnregisterInternal(loader);
						continue;
					}

					loader.RunCleanupIteration();
					if (!loader.HasUnreleasedRequests)
						UnregisterInternal(loader);
				}
			} finally {
				isProcessingLoaders = false;
				FlushLoaderListModifications();
			}
		}

		private void OnDestroy () {
			if (singletonInstance != this) return;

			UnloadAllLoaderTextures();
			singletonInstance = null;
		}

		internal static void Register (IDeferredCleanupOnDemandTextureLoader loader) {
			if (loader == null || !loader.UnityObject) return;
			Instance.RegisterInternal(loader);
		}

		internal static void Unregister (IDeferredCleanupOnDemandTextureLoader loader) {
			if (loader == null || !loader.UnityObject || singletonInstance == null) return;
			singletonInstance.UnregisterInternal(loader);
		}

		void RegisterInternal (IDeferredCleanupOnDemandTextureLoader loader) {
			if (isProcessingLoaders) {
				loaderModifications.Add(new LoaderListModification {
					isAdd = true,
					loader = loader
				});
			} else {
				if (loaders.Contains(loader)) return;
				loaders.Add(loader);
			}
		}

		void UnregisterInternal (IDeferredCleanupOnDemandTextureLoader loader) {
			if (isProcessingLoaders) {
				loaderModifications.Add(new LoaderListModification {
					isAdd = false,
					loader = loader
				});
			} else {
				loaders.Remove(loader);
			}
		}

		void UnloadAllLoaderTextures () {
			isProcessingLoaders = true;
			try {
				for (int i = loaders.Count - 1; i >= 0; --i) {
					IDeferredCleanupOnDemandTextureLoader loader = loaders[i];
					UnityEngine.Object unityObject = loader != null ? loader.UnityObject : null;
					if (!unityObject) continue;
					try {
						loader.UnloadAllTextures();
					} catch (System.Exception exception) {
						Debug.LogException(exception, unityObject);
					}
				}
			} finally {
				isProcessingLoaders = false;
				loaders.Clear();
				loaderModifications.Clear();
			}
		}

		void FlushLoaderListModifications () {
			foreach (LoaderListModification entry in loaderModifications) {
				if (entry.isAdd) {
					if (loaders.Contains(entry.loader)) continue;
					loaders.Add(entry.loader);
				} else {
					loaders.Remove(entry.loader);
				}
			}
			loaderModifications.Clear();
		}
	}
}
#endif
