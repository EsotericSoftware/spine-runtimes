/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2024, Esoteric Software LLC
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

#define USE_THREADED_SKELETON_UPDATE
#define USE_THREADED_ANIMATION_UPDATE // requires USE_THREADED_SKELETON_UPDATE enabled

#define READ_VOLATILE_ONCE
#if UNITY_2017_3_OR_NEWER
//#define ENABLE_THREAD_PROFILING
#endif

#define DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS // enabled improves performance a bit.

//#define RUN_ALL_ON_MAIN_THREAD // for profiling comparison only
//#define RUN_NO_ANIMATION_UPDATE_ON_MAIN_THREAD // actual configuration option. depends, measured slightly better when disabled.
#define RUN_NO_SKELETON_LATEUPDATE_ON_MAIN_THREAD // actual configuration option, recommended enabled

#if NET_4_6
#define HAS_MANUAL_RESET_EVENT_SLIM
#endif

#if USE_THREADED_SKELETON_UPDATE

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#if ENABLE_THREAD_PROFILING
using UnityEngine.Profiling;
#endif

#if HAS_MANUAL_RESET_EVENT_SLIM
using ResetEvent = System.Threading.ManualResetEventSlim;
#else
using ResetEvent = System.Threading.ManualResetEvent;
#endif

namespace Spine.Unity {
	using WorkerPool = LockFreeWorkerPool<SkeletonUpdateSystem.SkeletonUpdateRange>;
	using WorkerPoolTask = LockFreeWorkerPool<SkeletonUpdateSystem.SkeletonUpdateRange>.Task;

	[DefaultExecutionOrder(0)]
	public class SkeletonUpdateSystem : MonoBehaviour {

		private static SkeletonUpdateSystem singletonInstance;

		const int TimeoutIterationCount = 10000;

		public static SkeletonUpdateSystem Instance {
			get {
				if (singletonInstance == null) {
					singletonInstance = FindObjectOfType<SkeletonUpdateSystem>();
					if (singletonInstance == null) {
						GameObject singletonGameObject = new GameObject("SkeletonUpdateSystem");
						singletonInstance = singletonGameObject.AddComponent<SkeletonUpdateSystem>();
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
				Debug.LogWarning("Multiple SkeletonUpdateSystem singleton GameObjects found! " +
					"Don't manually add SkeletonUpdateSystem to each scene, it is created automatically when needed.");
				Destroy(gameObject);
			}
		}

		private void OnDestroy () {
			if (singletonInstance == this)
				singletonInstance = null;
		}

		public static int SkeletonSortComparer (ISkeletonRenderer first, ISkeletonRenderer second) {
			SkeletonDataAsset firstDataAsset = first.SkeletonDataAsset;
			SkeletonDataAsset secondDataAsset = second.SkeletonDataAsset;
			if (firstDataAsset == null) return secondDataAsset == null ? 0 : -1;
			else if (secondDataAsset == null) return 1;
			else return firstDataAsset.GetHashCode() - secondDataAsset.GetHashCode();
		}
		public static int SkeletonSortComparer (SkeletonAnimationBase first, SkeletonAnimationBase second) {
			SkeletonDataAsset firstDataAsset = first.SkeletonDataAsset;
			SkeletonDataAsset secondDataAsset = second.SkeletonDataAsset;
			if (firstDataAsset == null) return secondDataAsset == null ? 0 : -1;
			else if (secondDataAsset == null) return 1;
			else return firstDataAsset.GetHashCode() - secondDataAsset.GetHashCode();
		}
		public static readonly Comparison<ISkeletonRenderer> SkeletonRendererComparer = SkeletonSortComparer;
		public static readonly Comparison<SkeletonAnimationBase> SkeletonAnimationComparer = SkeletonSortComparer;

		public struct SkeletonUpdateRange {
			public int rangeStart;
			public int rangeEndExclusive;
			public int frameCount;
			public float deltaTime;
			public UpdateTiming updateTiming;
		}

		public List<SkeletonAnimationBase> skeletonAnimationsUpdate = new List<SkeletonAnimationBase>();
		public List<SkeletonAnimationBase> skeletonAnimationsFixedUpdate = new List<SkeletonAnimationBase>();
		public List<SkeletonAnimationBase> skeletonAnimationsLateUpdate = new List<SkeletonAnimationBase>();
		public List<ISkeletonRenderer> skeletonRenderers = new List<ISkeletonRenderer>();
		WorkerPoolTask[] genericSkeletonTasks = null;

		public WorkerPool workerPool;

		public List<ResetEvent> updateDone = new List<ResetEvent>(4);
		public List<ResetEvent> lateUpdateDone = new List<ResetEvent>(4);

#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
		protected int[] rendererStartIndex;
		protected int[] rendererEndIndexExclusive;
		volatile protected int[] skeletonsLateUpdatedAtThread;
		protected int[] mainThreadProcessed;
		public AutoResetEvent lateUpdateWorkAvailable;
#endif
		protected Exception[] exceptions;
		protected UnityEngine.Object[] exceptionObjects;
		volatile protected int numExceptionsSet = 0;
		protected int usedThreadCount = -1;

		public void DeferredLogException(Exception exc, UnityEngine.Object context, int threadIndex) {
			exceptions[threadIndex] = exc;
			exceptionObjects[threadIndex] = context;
			numExceptionsSet++;
		}

		protected bool mainThreadUpdateCallbacks = true;
		protected CoroutineIterator[] splitUpdateMethod = null;

		protected bool sortSkeletonRenderers = false;
		protected bool sortSkeletonAnimations = false;

		int UsedThreadCount {
			get {
				if (usedThreadCount < 0) {
					usedThreadCount = Environment.ProcessorCount;
				}
				return usedThreadCount;
			}
			set {
				usedThreadCount = value;
			}
		}

		/// <summary>
		/// Enable to issue update callbacks (e.g. <see cref="SkeletonAnimationBase.UpdateLocal"/>) always from the
		/// main thread, at the cost of splitting overhead switching between main and worker thread.
		/// Disable to allow update callbacks from worker threads without splitting execution.
		/// </summary>
		public bool MainThreadUpdateCallbacks {
			set { mainThreadUpdateCallbacks = value; }
			get { return mainThreadUpdateCallbacks; }
		}

		/// <summary>
		/// Optimization setting. Enable to group ISkeletonRenderers by type (by SkeletonDataAsset) for mesh updates.
		/// Potentially allows for better cache locality, however this may be detrimental if skeleton types vary in
		/// complexity.
		/// </summary>
		public bool GroupRenderersBySkeletonType {
			set { sortSkeletonRenderers = value; }
			get { return sortSkeletonRenderers; }
		}

		/// <summary>
		/// Optimization setting. Enable to group skeletons to be animated by type (by SkeletonDataAsset).
		/// Potentially allows for better cache locality, however this may be detrimental if skeleton types vary in
		/// complexity.
		/// </summary>
		public bool GroupAnimationBySkeletonType {
			set { sortSkeletonAnimations = value; }
			get { return sortSkeletonAnimations; }
		}

#if USE_THREADED_ANIMATION_UPDATE
		public void RegisterForUpdate (UpdateTiming updateTiming, SkeletonAnimationBase skeletonAnimation) {
			skeletonAnimation.isUpdatedExternally = true;

			var skeletonAnimations = skeletonAnimationsUpdate;
			if (updateTiming == UpdateTiming.InFixedUpdate) skeletonAnimations = skeletonAnimationsFixedUpdate;
			else if (updateTiming == UpdateTiming.InLateUpdate) skeletonAnimations = skeletonAnimationsLateUpdate;

			if (skeletonAnimations.Contains(skeletonAnimation))
				return;
			skeletonAnimations.Add(skeletonAnimation);
		}

		public void UnregisterFromUpdate (UpdateTiming updateTiming, SkeletonAnimationBase skeletonAnimation) {
			var skeletonAnimations = skeletonAnimationsUpdate;
			if (updateTiming == UpdateTiming.InFixedUpdate) skeletonAnimations = skeletonAnimationsFixedUpdate;
			else if (updateTiming == UpdateTiming.InLateUpdate) skeletonAnimations = skeletonAnimationsLateUpdate;

			skeletonAnimations.Remove(skeletonAnimation);
			skeletonAnimation.isUpdatedExternally = false;
		}
#endif

		public void RegisterForUpdate (ISkeletonRenderer renderer) {
			renderer.IsUpdatedExternally = true;
			if (skeletonRenderers.Contains(renderer))
				return;
			skeletonRenderers.Add(renderer);
		}

		public void UnregisterFromUpdate (ISkeletonRenderer renderer) {
			skeletonRenderers.Remove(renderer);
			renderer.IsUpdatedExternally = false;
		}

#if USE_THREADED_ANIMATION_UPDATE
		public void Update () {
			if (skeletonAnimationsUpdate.Count > 0)
				UpdateAsync(skeletonAnimationsUpdate, UpdateTiming.InUpdate);
		}

		public void FixedUpdate () {
			if (skeletonAnimationsFixedUpdate.Count > 0)
				UpdateAsync(skeletonAnimationsFixedUpdate, UpdateTiming.InFixedUpdate);
		}
#endif
		public void LateUpdate () {
#if USE_THREADED_ANIMATION_UPDATE
			if (skeletonAnimationsLateUpdate.Count > 0)
				UpdateAsync(skeletonAnimationsLateUpdate, UpdateTiming.InLateUpdate);
#endif
			LateUpdateAsync();
		}

		public void UpdateAsync (List<SkeletonAnimationBase> skeletons, UpdateTiming updateTiming) {
			if (skeletons.Count == 0) return;

			// Sort by skeleton data to allow for better cache utilization.
			if (sortSkeletonAnimations)
				skeletons.Sort(SkeletonAnimationComparer);

			int numThreads = UsedThreadCount;
#if RUN_ALL_ON_MAIN_THREAD
			int numAsyncThreads = 0;
#elif RUN_NO_ANIMATION_UPDATE_ON_MAIN_THREAD
			int numAsyncThreads = numThreads;
#else
			int numAsyncThreads = numThreads - 1;
#endif

			if (workerPool == null)
				workerPool = new WorkerPool(numThreads);
			if (genericSkeletonTasks == null) {
				genericSkeletonTasks = new WorkerPoolTask[numThreads];
				for (int t = 0; t < numThreads; ++t) {
					genericSkeletonTasks[t] = new WorkerPoolTask();
				}
			}
		
			for (int t = 0; t < updateDone.Count; ++t) {
				updateDone[t].Reset();
			}
			if (updateDone.Count != numThreads) {
				for (int t = updateDone.Count; t < numThreads; ++t) {
					updateDone.Add(new ResetEvent(false));
				}
			}

			if (exceptions == null) {
				exceptions = new Exception[numThreads];
				exceptionObjects = new UnityEngine.Object[numThreads];
			}
			numExceptionsSet = 0;

			int rangePerThread = Mathf.CeilToInt((float)skeletons.Count / (float)numThreads);

			int skeletonEnd = skeletons.Count;

			int endIndexThreaded = Math.Min(skeletonEnd, rangePerThread * numAsyncThreads);
			MainThreadBeforeUpdate(skeletons, skeletonEnd);

#if RUN_ALL_ON_MAIN_THREAD
			for (int r = 0; r < skeletons.Count; ++r) {
				skeletons[r].UpdateInternal(Time.deltaTime, Time.frameCount, calledFromOnlyMainThread: true);
			}
#else
			if (!mainThreadUpdateCallbacks)
				UpdateAsyncThreadedCallbacks(skeletons, updateTiming,
					numThreads, numAsyncThreads, rangePerThread, skeletonEnd);
			else
				UpdateAsyncSplitMainThreadCallbacks(skeletons, updateTiming,
					numAsyncThreads, rangePerThread, skeletonEnd, endIndexThreaded);
#endif
			MainThreadAfterUpdate(skeletons, skeletonEnd);
		}

		protected void UpdateAsyncThreadedCallbacks (List<SkeletonAnimationBase> skeletons, UpdateTiming timing,
			int numThreads, int numAsyncThreads, int rangePerThread,
			int skeletonEnd) {

			int start = 0;
			int end = Mathf.Min(rangePerThread, skeletonEnd);

			for (int t = 0; t < numThreads; ++t) {
				var range = new SkeletonUpdateRange() {
					rangeStart = start,
					rangeEndExclusive = end,
					deltaTime = Time.deltaTime,
					frameCount = Time.frameCount,
					updateTiming = timing
				};

				if (t < numAsyncThreads) {
					UpdateSkeletonsAsync(range, t);
				} else {
					// this main thread does some work as well, otherwise it's only waiting.
					UpdateSkeletonsSynchronous(skeletons, range);
				}

				start = end;
				if (start >= skeletonEnd) {
					while (++t < numAsyncThreads)
						updateDone[t].Set();
					break;
				}
				end = Mathf.Min(end + rangePerThread, skeletonEnd);
			}

			WaitForThreadUpdateTasks(numAsyncThreads);
		}

		protected void UpdateAsyncSplitMainThreadCallbacks (List<SkeletonAnimationBase> skeletons, UpdateTiming timing,
			int numAsyncThreads, int rangePerThread,
			int skeletonEnd, int endIndexThreaded) {

			if (splitUpdateMethod == null) {
				splitUpdateMethod = new CoroutineIterator[skeletons.Count];
			}
			int requiredCount = endIndexThreaded; //skeletonAnimations.Count;
			if (splitUpdateMethod.Length < requiredCount) {
				Array.Resize(ref splitUpdateMethod, requiredCount);
			}

			bool isFirstIteration = true;
			bool anyWorkLeft;
			int timeoutCounter = 0;
			do {
				int start = 0;
				int end = Mathf.Min(rangePerThread, skeletonEnd);
				for (int t = 0; t < numAsyncThreads; ++t) {
					var range = new SkeletonUpdateRange() {
						rangeStart = start,
						rangeEndExclusive = end,
						deltaTime = Time.deltaTime,
						frameCount = Time.frameCount,
						updateTiming = timing
					};

					UpdateSkeletonsAsyncSplit(range, t);
	
					start = end;
					if (start >= skeletonEnd) {
						while (++t < numAsyncThreads) {
							updateDone[t].Set();
						}
						break;
					}
					end = Mathf.Min(end + rangePerThread, skeletonEnd);
				}

				// main thread
				if (isFirstIteration && start != skeletonEnd) {
					var range = new SkeletonUpdateRange() {
						rangeStart = start,
						rangeEndExclusive = end,
						deltaTime = Time.deltaTime,
						frameCount = Time.frameCount,
						updateTiming = timing
					};
					// this main thread does complete update work in the first iteration, otherwise it's only waiting.
					UpdateSkeletonsSynchronous(skeletons, range);
				}

				// wait for all threaded tasks
				WaitForThreadUpdateTasks(numAsyncThreads);
				for (int t = 0; t < updateDone.Count; ++t) {
					updateDone[t].Reset();
				}

				// Note: the call above contains calls to ResetEvent.WaitOne, creating implicit memory barriers.
				// The explicit memory barrier below is added to ensure a memory barrier is in place on the many
				// Unity target platforms.
				Thread.MemoryBarrier();
		
				// process main thread callback part
				anyWorkLeft = UpdateSkeletonsMainThreadSplit(skeletons, endIndexThreaded, Time.deltaTime, Time.frameCount);
				isFirstIteration = false;
			} while (anyWorkLeft && ++timeoutCounter < TimeoutIterationCount);

			if (timeoutCounter >= TimeoutIterationCount) {
				Debug.LogError("Internal threading logic error: exited Update loop after timeout!");
			}

			for (int i = 0; i < endIndexThreaded; ++i) {
				splitUpdateMethod[i] = new CoroutineIterator();
			}
		}

		protected void MainThreadBeforeUpdate (List<SkeletonAnimationBase> skeletons, int skeletonEnd) {
			for (int i = 0; i < skeletonEnd; ++i) {
				skeletons[i].MainThreadBeforeUpdateInternal();
			}
		}

		protected void MainThreadAfterUpdate (List<SkeletonAnimationBase> skeletons, int skeletonEnd) {
			for (int i = 0; i < skeletonEnd; ++i) {
				skeletons[i].MainThreadAfterUpdateInternal();
			}
		}

		public void LateUpdateAsync () {
			if (skeletonRenderers.Count == 0) return;

			// Sort by skeleton data to allow for better cache utilization.
			if (sortSkeletonRenderers)
				skeletonRenderers.Sort(SkeletonRendererComparer);

			int numThreads = UsedThreadCount;
#if RUN_ALL_ON_MAIN_THREAD
			int numAsyncThreads = 0;
#elif RUN_NO_SKELETON_LATEUPDATE_ON_MAIN_THREAD
			int numAsyncThreads = numThreads;
#else
			int numAsyncThreads = numThreads - 1;
#endif
			if (workerPool == null)
				workerPool = new WorkerPool(numThreads);
			if (genericSkeletonTasks == null) {
				genericSkeletonTasks = new WorkerPoolTask[numThreads];
				for (int t = 0; t < numThreads; ++t) {
					genericSkeletonTasks[t] = new WorkerPoolTask();
				}
			}
		
			for (int t = 0; t < lateUpdateDone.Count; ++t) {
				lateUpdateDone[t].Reset();
			}
			if (lateUpdateDone.Count != numThreads) {
				for (int t = lateUpdateDone.Count; t < numThreads; ++t) {
					lateUpdateDone.Add(new ResetEvent(false));
				}
			}
	
			int rangePerThread = Mathf.CeilToInt((float)skeletonRenderers.Count / (float)numThreads);
			int skeletonEnd = skeletonRenderers.Count;
#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			if (skeletonsLateUpdatedAtThread == null) {
				skeletonsLateUpdatedAtThread = new int[numThreads];
				mainThreadProcessed = new int[numThreads];
				rendererStartIndex = new int[numThreads];
				rendererEndIndexExclusive = new int[numThreads];
				lateUpdateWorkAvailable = new AutoResetEvent(false);
			}
			for (int t = 0; t < numThreads; ++t) {
				rendererStartIndex[t] = rangePerThread * t;
				rendererEndIndexExclusive[t] = Mathf.Min(rangePerThread * (t + 1), skeletonEnd);
			}
			for (int t = 0; t < numAsyncThreads; ++t) {
				skeletonsLateUpdatedAtThread[t] = 0;
			}
#endif
			int endIndexThreaded = Math.Min(skeletonEnd, rangePerThread * numAsyncThreads);
			MainThreadPrepareLateUpdate(endIndexThreaded);

			int start = 0;
			int end = Mathf.Min(rangePerThread, skeletonEnd);

			for (int t = 0; t < numThreads; ++t) {
				var range = new SkeletonUpdateRange() {
					rangeStart = start,
					rangeEndExclusive = end,
					deltaTime = Time.deltaTime,
					frameCount = Time.frameCount,
					updateTiming = UpdateTiming.InLateUpdate
				};

				if (t < numAsyncThreads) {
					LateUpdateSkeletonsAsync(range, t);
				} else {
					// this main thread does some work as well, otherwise it's only waiting.
					LateUpdateSkeletonsSynchronous(range);
				}

				start = end;
				if (start >= skeletonEnd) {
					while (++t < numAsyncThreads)
						lateUpdateDone[t].Set();
					break;
				}
				end = Mathf.Min(end + rangePerThread, skeletonEnd);
			}

#if RUN_ALL_ON_MAIN_THREAD
			return; // nothing left to do after all processed as LateUpdateSkeletonsSynchronous
#endif

#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			for (int t = 0; t < numAsyncThreads; ++t) {
				mainThreadProcessed[t] = 0;
			}
			bool anySkeletonsLeft = false;
			bool wasWorkAvailable = false;
			bool timedOut = false;
			do {
				anySkeletonsLeft = false;
				for (int t = 0; t < numAsyncThreads; ++t) {
					int threadEndIndex = rendererEndIndexExclusive[t] - rendererStartIndex[t];
#if READ_VOLATILE_ONCE
					int updatedAtWorkerThread = skeletonsLateUpdatedAtThread[t];

					while (mainThreadProcessed[t] < updatedAtWorkerThread) {
#else
					while (mainThreadProcessed[t] < skeletonsLateUpdatedAtThread[t]) {
#endif
						wasWorkAvailable = true;
						int r = mainThreadProcessed[t] + rendererStartIndex[t];
						var skeletonRenderer = this.skeletonRenderers[r];
						if (skeletonRenderer.RequiresMeshBufferAssignmentMainThread)
							skeletonRenderer.UpdateMeshAndMaterialsToBuffers();
						mainThreadProcessed[t]++;
					}

#if READ_VOLATILE_ONCE
					if (updatedAtWorkerThread < threadEndIndex) {
#else
					if (skeletonsLateUpdatedAtThread[t] < threadEndIndex) {
#endif
						anySkeletonsLeft = true;
					}
				}
				LogWorkerThreadExceptions();

				if (!wasWorkAvailable) {
					int timeoutMilliseconds = 1000;
					timedOut = !lateUpdateWorkAvailable.WaitOne(timeoutMilliseconds);
				}
			} while (anySkeletonsLeft && !timedOut);
			if (timedOut) {
				Debug.LogError("Internal threading logic error: exited LateUpdate loop after timeout!");
			}
#else
			// wait for all threaded task, then process all renderers in main thread
			WaitForThreadLateUpdateTasks(numAsyncThreads);

			// Additional main thread update when the mesh data could not be assigned from worker thread
			// and has to be assigned from main thread.
			int maxNonUpdatedRenderer = Math.Min(rangePerThread * numAsyncThreads, this.skeletonRenderers.Count);

			for (int r = 0; r < maxNonUpdatedRenderer; ++r) {
				var skeletonRenderer = this.skeletonRenderers[r];
				if (skeletonRenderer.RequiresMeshBufferAssignmentMainThread)
					skeletonRenderer.UpdateMeshAndMaterialsToBuffers();
			}
#endif
		}

		protected void MainThreadPrepareLateUpdate (int endIndexThreaded) {
			for (int i = 0; i < endIndexThreaded; ++i) {
				skeletonRenderers[i].MainThreadPrepareLateUpdateInternal();
			}
		}

		private void WaitForThreadUpdateTasks (int numAsyncThreads) {
			for (int t = 0, n = numAsyncThreads; t < n; ++t) {
				int timeoutMilliseconds = 1000;
#if HAS_MANUAL_RESET_EVENT_SLIM
				updateDone[t].Wait(timeoutMilliseconds);
#else // HAS_MANUAL_RESET_EVENT_SLIM
				updateDone[t].WaitOne(timeoutMilliseconds);
#endif // HAS_MANUAL_RESET_EVENT_SLIM
			}
			LogWorkerThreadExceptions();
		}

		private void LogWorkerThreadExceptions () {
			if (numExceptionsSet > 0) {
				for (int t = 0; t < exceptions.Length; ++t) {
					if (exceptions[t] == null) continue;
					Debug.LogError(string.Format("Exception in worker thread {0}: {1}.\nStackTrace: {2}",
						t, exceptions[t].Message, exceptions[t].StackTrace), exceptionObjects[t]);
					exceptions[t] = null;
					exceptionObjects[t] = null;
				}
				numExceptionsSet = 0;
			}
		}

		private void WaitForThreadLateUpdateTasks (int numAsyncThreads) {
			for (int t = 0, n = numAsyncThreads; t < n; ++t) {
				int timeoutMilliseconds = 1000;
#if HAS_MANUAL_RESET_EVENT_SLIM
				lateUpdateDone[t].Wait(timeoutMilliseconds);
#else // HAS_MANUAL_RESET_EVENT_SLIM
				lateUpdateDone[t].WaitOne(timeoutMilliseconds);
#endif // HAS_MANUAL_RESET_EVENT_SLIM
			}
		}

#if ENABLE_THREAD_PROFILING
		CustomSampler[] profilerSamplerUpdate = new CustomSampler[16];
		CustomSampler[] profilerSamplerLateUpdate = new CustomSampler[16];
#endif

		/// <summary>Perform Update at all SkeletonRenderers asynchronously.</summary>
		void UpdateSkeletonsAsync (SkeletonUpdateRange range, int threadIndex) {
#if ENABLE_THREAD_PROFILING
			if (profilerSamplerUpdate[threadIndex] == null) {
				profilerSamplerUpdate[threadIndex] = CustomSampler.Create("Spine Update " + threadIndex);
			}
#endif
			WorkerPoolTask task = genericSkeletonTasks[threadIndex];
			task.parameters = range;
			task.function = cachedUpdateSkeletonsAsyncImpl;
			bool enqueueSucceeded;
			do {
				enqueueSucceeded = workerPool.EnqueueTask(threadIndex, genericSkeletonTasks[threadIndex]);
			} while (!enqueueSucceeded);
		}
		// avoid allocation, unfortunately this is really necessary
		static Action<SkeletonUpdateRange, int> cachedUpdateSkeletonsAsyncImpl = UpdateSkeletonsAsyncImpl;
		static void UpdateSkeletonsAsyncImpl (SkeletonUpdateRange range, int threadIndex) {
			var instance = Instance;
#if ENABLE_THREAD_PROFILING
			instance.profilerSamplerUpdate[threadIndex].Begin();
#endif
			float deltaTime = range.deltaTime;
			int frameCount = range.frameCount;
			int start = range.rangeStart;
			int end = range.rangeEndExclusive;
			var skeletonAnimations = instance.skeletonAnimationsUpdate;
			if (range.updateTiming == UpdateTiming.InFixedUpdate) skeletonAnimations = instance.skeletonAnimationsFixedUpdate;
			else if (range.updateTiming == UpdateTiming.InLateUpdate) skeletonAnimations = instance.skeletonAnimationsLateUpdate;

			for (int r = start; r < end; ++r) {
				try {
					skeletonAnimations[r].UpdateInternal(deltaTime, frameCount, calledFromOnlyMainThread: false);
				} catch (Exception exc) {
					instance.DeferredLogException(exc, skeletonAnimations[r], threadIndex);
				}
			}
			instance.updateDone[threadIndex].Set();
#if ENABLE_THREAD_PROFILING
			instance.profilerSamplerUpdate[threadIndex].End();
#endif
		}

		//------------------------------------------------------------------------------------------
		/// <summary>Perform Update at all SkeletonRenderers asynchronously and split off at
		/// main-thread callbacks.</summary>
		void UpdateSkeletonsAsyncSplit (SkeletonUpdateRange range, int threadIndex) {
			
#if ENABLE_THREAD_PROFILING
			if (profilerSamplerUpdate[threadIndex] == null) {
				profilerSamplerUpdate[threadIndex] = CustomSampler.Create("Spine Update " + threadIndex);
			}
#endif
			bool enqueueSucceeded;
			do {
				WorkerPoolTask task = genericSkeletonTasks[threadIndex];
				task.parameters = range;
				task.function = cachedUpdateSkeletonsAsyncSplitImpl;
				enqueueSucceeded = workerPool.EnqueueTask(threadIndex, genericSkeletonTasks[threadIndex]);
			} while (!enqueueSucceeded);
		}
		// avoid allocation, unfortunately this is really necessary
		static Action<SkeletonUpdateRange, int> cachedUpdateSkeletonsAsyncSplitImpl = UpdateSkeletonsAsyncSplitImpl;
		static void UpdateSkeletonsAsyncSplitImpl (SkeletonUpdateRange range, int threadIndex) {
			float deltaTime = range.deltaTime;
			int frameCount = range.frameCount;
			int start = range.rangeStart;
			int end = range.rangeEndExclusive;
			var instance = Instance;
			var skeletonAnimations = instance.skeletonAnimationsUpdate;
			if (range.updateTiming == UpdateTiming.InFixedUpdate) skeletonAnimations = instance.skeletonAnimationsFixedUpdate;
			else if (range.updateTiming == UpdateTiming.InLateUpdate) skeletonAnimations = instance.skeletonAnimationsLateUpdate;

			var splitUpdateMethod = instance.splitUpdateMethod;

#if ENABLE_THREAD_PROFILING
			instance.profilerSamplerUpdate[threadIndex].Begin();
#endif
			for (int r = start; r < end; ++r) {
				try {
					SkeletonAnimationBase targetSkeletonAnimation = skeletonAnimations[r];
					if (!splitUpdateMethod[r].IsDone) {
						splitUpdateMethod[r] = targetSkeletonAnimation.UpdateInternalSplit(splitUpdateMethod[r], deltaTime, frameCount);
					}
				} catch (Exception exc) {
					instance.DeferredLogException(exc, skeletonAnimations[r], threadIndex);
				}
			}
			instance.updateDone[threadIndex].Set();

#if ENABLE_THREAD_PROFILING
			instance.profilerSamplerUpdate[threadIndex].End();
#endif
		}

		bool UpdateSkeletonsMainThreadSplit (List<SkeletonAnimationBase> skeletons, int endIndexThreaded,
			float deltaTime, int frameCount) {
			bool anyWorkLeft = false;

			for (int r = 0; r < endIndexThreaded; ++r) {
				try {
					SkeletonAnimationBase targetSkeletonAnimation = skeletons[r];
					if (splitUpdateMethod[r].IsInitialState) {
						Debug.LogError("Internal threading logic error: skeletonAnimations never called UpdateInternal before!", skeletons[r]);
					} else {
						if (!splitUpdateMethod[r].IsDone) {
							anyWorkLeft = true;
							splitUpdateMethod[r] = targetSkeletonAnimation.UpdateInternalSplit(splitUpdateMethod[r], deltaTime, frameCount);
						}
					}
				} catch (Exception exc) {
					Debug.LogError(string.Format("Exception in main thread: {0}.\nStackTrace: {1}",
						exc.Message, exc.StackTrace));
				}
			}
			return anyWorkLeft;
		}

		void UpdateSkeletonsSynchronous (List<SkeletonAnimationBase> skeletons, SkeletonUpdateRange range) {
			int start = range.rangeStart;
			int end = range.rangeEndExclusive;
			
			for (int r = start; r < end; ++r) {
				skeletons[r].UpdateInternal(range.deltaTime, range.frameCount, calledFromOnlyMainThread: true);
			}
		}

		/// <summary>Perform LateUpdate at all SkeletonRenderers asynchronously.</summary>
		static Action<SkeletonUpdateRange, int> cachedLateUpdateSkeletonsAsyncImpl = LateUpdateSkeletonsAsyncImpl;
		void LateUpdateSkeletonsAsync (SkeletonUpdateRange range, int threadIndex) {
#if ENABLE_THREAD_PROFILING
			if (profilerSamplerLateUpdate[threadIndex] == null) {
				profilerSamplerLateUpdate[threadIndex] = CustomSampler.Create("Spine LateUpdate " + threadIndex);
			}
#endif
			bool enqueueSucceeded;
			WorkerPoolTask task = genericSkeletonTasks[threadIndex];
			task.parameters = range;
			task.function = cachedLateUpdateSkeletonsAsyncImpl;
			do {
				enqueueSucceeded = workerPool.EnqueueTask(threadIndex, genericSkeletonTasks[threadIndex]);
			} while (!enqueueSucceeded);
		}

		static void LateUpdateSkeletonsAsyncImpl (SkeletonUpdateRange range, int threadIndex) {
			int start = range.rangeStart;
			int end = range.rangeEndExclusive;
			var instance = Instance;

#if ENABLE_THREAD_PROFILING
			instance.profilerSamplerLateUpdate[threadIndex].Begin();
#endif
#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			instance.skeletonsLateUpdatedAtThread[threadIndex] = 0;
#endif
			for (int r = start; r < end; ++r) {
				try {
					instance.skeletonRenderers[r].LateUpdateImplementation(calledFromMainThread: false);
				} catch (Exception exc) {
					instance.DeferredLogException(exc, instance.skeletonRenderers[r].Component, threadIndex);
				}
#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
				Interlocked.Increment(ref instance.skeletonsLateUpdatedAtThread[threadIndex]);
				instance.lateUpdateWorkAvailable.Set(); // signal as soon as it can be processed by main thread
#endif
			}
#if !DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			instance.lateUpdateDone[threadIndex].Set(); // signal once after all work is done
#endif
#if ENABLE_THREAD_PROFILING
			instance.profilerSamplerLateUpdate[threadIndex].End();
#endif
		}

		void LateUpdateSkeletonsSynchronous (SkeletonUpdateRange range) {
			int start = range.rangeStart;
			int end = range.rangeEndExclusive;

			for (int r = start; r < end; ++r) {
				skeletonRenderers[r].LateUpdateImplementation(calledFromMainThread: true);
			}
		}
	}
}

#endif // USE_THREADED_SKELETON_UPDATE
