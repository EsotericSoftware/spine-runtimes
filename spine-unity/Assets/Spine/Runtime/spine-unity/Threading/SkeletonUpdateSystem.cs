/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#if UNITY_2022_2_OR_NEWER
#define USE_FIND_OBJECTS_BY_TYPE
#endif

#if !SPINE_DISABLE_THREADING
#define USE_THREADED_SKELETON_UPDATE
#define USE_THREADED_ANIMATION_UPDATE // requires USE_THREADED_SKELETON_UPDATE enabled
#endif

#if !SPINE_DISABLE_LOAD_BALANCING
#define ENABLE_WORK_STEALING // load balancing, enabled improves performance, distributes work to otherwise idle threads.
#endif

#define READ_VOLATILE_ONCE

#define DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS // enabled improves performance a bit.

//#define RUN_ALL_ON_MAIN_THREAD // for profiling comparison only
// actual configuration option, does not matter with mainThreadUpdateCallbacks enabled.
// measured slightly better when disabled with disabled work-stealing (load balancing), better when enabled with work-stealing enabled
#if ENABLE_WORK_STEALING
#define RUN_NO_ANIMATION_UPDATE_ON_MAIN_THREAD
#endif

#if ENABLE_WORK_STEALING
#define REQUIRES_MORE_CHUNKS
#else
#define RUN_NO_SKELETON_LATEUPDATE_ON_MAIN_THREAD // actual configuration option, recommended enabled when not using work stealing
#endif

#if NET_STANDARD_2_0 || NET_STANDARD_2_1 || NET_4_6
#define HAS_MANUAL_RESET_EVENT_SLIM
#endif

#if USE_THREADED_SKELETON_UPDATE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#if SPINE_ENABLE_THREAD_PROFILING
using UnityEngine.Profiling;
#endif

#if HAS_MANUAL_RESET_EVENT_SLIM
using ResetEvent = System.Threading.ManualResetEventSlim;
#else
using ResetEvent = System.Threading.ManualResetEvent;
#endif

namespace Spine.Unity {
#if ENABLE_WORK_STEALING
	using WorkerPool = LockFreeWorkStealingWorkerPool<SkeletonUpdateSystem.SkeletonUpdateRange>;
	using WorkerPoolTask = LockFreeWorkStealingWorkerPool<SkeletonUpdateSystem.SkeletonUpdateRange>.Task;
#else
	using WorkerPool = LockFreeWorkerPool<SkeletonUpdateSystem.SkeletonUpdateRange>;
	using WorkerPoolTask = LockFreeWorkerPool<SkeletonUpdateSystem.SkeletonUpdateRange>.Task;
#endif
	[DefaultExecutionOrder(0)]
	public class SkeletonUpdateSystem : MonoBehaviour {

		private static SkeletonUpdateSystem singletonInstance;

		const int TimeoutIterationCount = 10000;

#if REQUIRES_MORE_CHUNKS
		public int UpdateChunksPerThread = 8;
		public int LateUpdateChunksPerThread = 8;
#else
		public int UpdateChunksPerThread = 1;
		public int LateUpdateChunksPerThread = 1;
#endif

		public static SkeletonUpdateSystem Instance {
			get {
				if (singletonInstance == null) {
#if USE_FIND_OBJECTS_BY_TYPE
					singletonInstance = FindFirstObjectByType<SkeletonUpdateSystem>();
#else
					singletonInstance = FindObjectOfType<SkeletonUpdateSystem>();
#endif
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
			public int taskIndex;
			public int frameCount;
			public UpdateTiming updateTiming;
		}

		public struct SkeletonPartitionRange {
			public int rangeStart;
			public int rangeEndExclusive;
			public int threadIndex;
		}

		public List<SkeletonAnimationBase> skeletonAnimationsUpdate = new List<SkeletonAnimationBase>();
		public List<SkeletonAnimationBase> skeletonAnimationsFixedUpdate = new List<SkeletonAnimationBase>();
		public List<SkeletonAnimationBase> skeletonAnimationsLateUpdate = new List<SkeletonAnimationBase>();
		public List<ISkeletonRenderer> skeletonRenderers = new List<ISkeletonRenderer>();
		WorkerPoolTask[] genericSkeletonTasks = null;

		public WorkerPool workerPool;

		ExposedList<SkeletonPartitionRange> taskPartitionsUpdate = null;
		ExposedList<SkeletonPartitionRange> taskPartitionsLateUpdate = null;

		public List<ResetEvent> updateDone = new List<ResetEvent>(4);
		public List<ResetEvent> lateUpdateDone = new List<ResetEvent>(4);

#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
		volatile protected int[] skeletonsLateUpdatedAtTask;
		protected int[] mainThreadProcessedAtTask;
		public AutoResetEvent lateUpdateWorkAvailable;
#endif
		protected Exception[] exceptions;
		protected UnityEngine.Object[] exceptionObjects;
		volatile protected int numExceptionsSet = 0;
		protected int usedThreadCount = -1;

		public void DeferredLogException (Exception exc, UnityEngine.Object context, int threadIndex) {
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
			skeletonAnimation.IsUpdatedExternally = true;

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
			skeletonAnimation.IsUpdatedExternally = false;
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
			int numAsyncThreads = mainThreadUpdateCallbacks ? numThreads - 1 : numThreads;
#else
			int numAsyncThreads = numThreads - 1;
#endif
			int tasksPerThread = UpdateChunksPerThread;
			int numTasks = numThreads * tasksPerThread;
			if (workerPool == null)
				workerPool = new WorkerPool(numThreads, tasksPerThread + 1);
			if (genericSkeletonTasks == null || genericSkeletonTasks.Length < numTasks) {
				genericSkeletonTasks = new WorkerPoolTask[numTasks];
				for (int t = 0; t < genericSkeletonTasks.Length; ++t) {
					genericSkeletonTasks[t] = new WorkerPoolTask();
				}
			}
#if SPINE_ENABLE_THREAD_PROFILING
			if (profilerSamplerUpdate == null) {
				profilerSamplerUpdate = new CustomSampler[numThreads];
			}
#endif
			int endIndexThreaded;
			int numAvailableThreads = mainThreadUpdateCallbacks ? numAsyncThreads : UsedThreadCount;
			PartitionTasks(ref taskPartitionsUpdate, out endIndexThreaded, tasksPerThread, skeletons.Count,
				numAsyncThreads, numAvailableThreads);

			for (int t = 0; t < updateDone.Count; ++t) {
				updateDone[t].Reset();
			}
			for (int t = updateDone.Count; t < numTasks; ++t) {
				updateDone.Add(new ResetEvent(false));
			}

			if (exceptions == null) {
				exceptions = new Exception[numThreads];
				exceptionObjects = new UnityEngine.Object[numThreads];
			}
			numExceptionsSet = 0;

			int skeletonEnd = skeletons.Count;

			SkeletonAnimationBase.ExternalDeltaTime = Time.deltaTime;
			SkeletonAnimationBase.ExternalUnscaledDeltaTime = Time.unscaledDeltaTime;
			MainThreadBeforeUpdate(skeletons, skeletonEnd);

#if RUN_ALL_ON_MAIN_THREAD
			for (int r = 0; r < skeletons.Count; ++r) {
				skeletons[r].UpdateExternal(Time.frameCount, calledFromOnlyMainThread: true);
			}
#else
			if (!mainThreadUpdateCallbacks)
				UpdateAsyncThreadedCallbacks(skeletons, updateTiming, taskPartitionsUpdate,
					numAsyncThreads, skeletonEnd);
			else
				UpdateAsyncSplitMainThreadCallbacks(skeletons, updateTiming, taskPartitionsUpdate,
					numAsyncThreads, skeletonEnd);
#endif
			MainThreadAfterUpdate(skeletons, skeletonEnd);
		}

		protected void PartitionTasks(ref ExposedList<SkeletonPartitionRange> taskPartitions, out int outAsyncEndExclusive,
			int tasksPerThread, int skeletonCount, int numAsyncThreads, int numAvailableThreads) {

			int numAsyncTasks = numAsyncThreads * tasksPerThread;
			if (taskPartitions == null) {
				taskPartitions = new ExposedList<SkeletonPartitionRange>(numAsyncTasks);
			}
			if (taskPartitions.Count != numAsyncTasks) {
				taskPartitions.Resize(numAsyncTasks);
			}

			int rangePerThread = Mathf.CeilToInt((float)skeletonCount / (float)numAvailableThreads);
			int rangePerTask = Math.Max(1, Mathf.CeilToInt((float)rangePerThread / (float)tasksPerThread));

			int totalAsyncTasks = 0;
			int threadStart = 0;
			int threadEnd = Mathf.Min(rangePerThread, skeletonCount);
			SkeletonPartitionRange[] partitionItems = taskPartitions.Items;
			for (int threadIndex = 0; threadIndex < numAsyncThreads; ++threadIndex) {
				int start = threadStart;
				int end = Mathf.Min(start + rangePerTask, threadEnd);
				for (int t = 0; t < tasksPerThread; ++t) {
					partitionItems[totalAsyncTasks++] = new SkeletonPartitionRange() {
						rangeStart = start,
						rangeEndExclusive = end,
						threadIndex = threadIndex
					};
					start = end;
					end = Mathf.Min(end + rangePerTask, threadEnd);
				}
				threadStart = threadEnd;
				threadEnd = Mathf.Min(threadEnd + rangePerThread, skeletonCount);
			}
			outAsyncEndExclusive = threadStart; // threadStart == previous threadEnd
		}

		protected void UpdateAsyncThreadedCallbacks (List<SkeletonAnimationBase> skeletons, UpdateTiming timing,
			ExposedList<SkeletonPartitionRange> asyncTaskPartitions, int numAsyncThreads, int skeletonEnd) {

			SkeletonPartitionRange[] asyncPartitionsItems = asyncTaskPartitions.Items;
			for (int taskIndex = 0, count = asyncTaskPartitions.Count; taskIndex < count; ++taskIndex) {
				SkeletonPartitionRange partition = asyncPartitionsItems[taskIndex];
				if (partition.rangeStart == partition.rangeEndExclusive) {
					updateDone[taskIndex].Set();
					continue;
				}
				var range = new SkeletonUpdateRange() {
					rangeStart = partition.rangeStart,
					rangeEndExclusive = partition.rangeEndExclusive,
					taskIndex = taskIndex,
					frameCount = Time.frameCount,
					updateTiming = timing
				};
				UpdateSkeletonsAsync(range, partition.threadIndex);
			}
#if ENABLE_WORK_STEALING
			workerPool.AllowTaskProcessing(numAsyncThreads);
#endif
			SkeletonPartitionRange lastAsyncPartition = asyncPartitionsItems[asyncTaskPartitions.Count - 1];
			if (lastAsyncPartition.rangeEndExclusive < skeletonEnd) {
				// this main thread does some work as well, otherwise it's only waiting.
				var range = new SkeletonUpdateRange() {
					rangeStart = lastAsyncPartition.rangeEndExclusive,
					rangeEndExclusive = skeletonEnd,
					taskIndex = -1,
					frameCount = Time.frameCount,
					updateTiming = timing
				};
				UpdateSkeletonsSynchronous(skeletons, range);
			}
			WaitForThreadUpdateTasks(asyncTaskPartitions.Count);
		}

		protected void UpdateAsyncSplitMainThreadCallbacks (List<SkeletonAnimationBase> skeletons, UpdateTiming timing,
			ExposedList<SkeletonPartitionRange> asyncTaskPartitions, int numAsyncThreads, int skeletonEnd) {

			SkeletonPartitionRange[] asyncPartitionsItems = asyncTaskPartitions.Items;
			SkeletonPartitionRange lastAsyncPartition = asyncPartitionsItems[asyncTaskPartitions.Count - 1];
			int endIndexThreaded = lastAsyncPartition.rangeEndExclusive;

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
				for (int taskIndex = 0, count = asyncTaskPartitions.Count; taskIndex < count; ++taskIndex) {
					SkeletonPartitionRange partition = asyncPartitionsItems[taskIndex];
					if (partition.rangeStart == partition.rangeEndExclusive) {
						updateDone[taskIndex].Set();
						continue;
					}
					var range = new SkeletonUpdateRange() {
						rangeStart = partition.rangeStart,
						rangeEndExclusive = partition.rangeEndExclusive,
						taskIndex = taskIndex,
						frameCount = Time.frameCount,
						updateTiming = timing
					};
					UpdateSkeletonsAsyncSplit(range, partition.threadIndex);
				}
#if ENABLE_WORK_STEALING
				workerPool.AllowTaskProcessing(numAsyncThreads);
#endif
				// main thread
				if (isFirstIteration && lastAsyncPartition.rangeEndExclusive < skeletonEnd) {
					// this main thread does complete update work in the first iteration, otherwise it's only waiting.
					var range = new SkeletonUpdateRange() {
						rangeStart = lastAsyncPartition.rangeEndExclusive,
						rangeEndExclusive = skeletonEnd,
						taskIndex = -1,
						frameCount = Time.frameCount,
						updateTiming = timing
					};
					UpdateSkeletonsSynchronous(skeletons, range);
				}

				// wait for all threaded tasks
				WaitForThreadUpdateTasks(asyncTaskPartitions.Count);
				for (int t = 0; t < asyncTaskPartitions.Count; ++t) {
					updateDone[t].Reset();
				}

				// Note: the call above contains calls to ResetEvent.WaitOne, creating implicit memory barriers.
				// The explicit memory barrier below is added to ensure a memory barrier is in place on the many
				// Unity target platforms.
				Thread.MemoryBarrier();

				// process main thread callback part
				anyWorkLeft = UpdateSkeletonsMainThreadSplit(skeletons, endIndexThreaded, Time.frameCount);
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
			int tasksPerThread = LateUpdateChunksPerThread;
			int numTasks = numThreads * tasksPerThread;
			if (workerPool == null)
				workerPool = new WorkerPool(numThreads, tasksPerThread * 2);
			if (genericSkeletonTasks == null || genericSkeletonTasks.Length < numTasks) {
				genericSkeletonTasks = new WorkerPoolTask[numTasks];
				for (int t = 0; t < genericSkeletonTasks.Length; ++t) {
					genericSkeletonTasks[t] = new WorkerPoolTask();
				}
			}
#if SPINE_ENABLE_THREAD_PROFILING
			if (profilerSamplerLateUpdate == null) {
				profilerSamplerLateUpdate = new CustomSampler[numThreads];
			}
#endif
			int endIndexThreaded;
#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			int numAvailableThreads = numAsyncThreads;
#else
			int numAvailableThreads = UsedThreadCount;
#endif
			PartitionTasks(ref taskPartitionsLateUpdate, out endIndexThreaded, tasksPerThread, skeletonRenderers.Count,
				numAsyncThreads, numAvailableThreads);
			ExposedList<SkeletonPartitionRange> asyncTaskPartitions = taskPartitionsLateUpdate;
			int numAsyncTasks = asyncTaskPartitions.Count;

#if !DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			for (int t = 0; t < lateUpdateDone.Count; ++t) {
				lateUpdateDone[t].Reset();
			}
			for (int t = lateUpdateDone.Count; t < numAsyncTasks; ++t) {
				lateUpdateDone.Add(new ResetEvent(false));
			}
#endif // !DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS

			int skeletonEnd = skeletonRenderers.Count;
#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			if (skeletonsLateUpdatedAtTask == null) {
				skeletonsLateUpdatedAtTask = new int[numAsyncTasks];
				mainThreadProcessedAtTask = new int[numAsyncTasks];
				lateUpdateWorkAvailable = new AutoResetEvent(false);
			}
			for (int t = 0; t < numAsyncTasks; ++t) {
				skeletonsLateUpdatedAtTask[t] = 0;
			}
#endif
			MainThreadPrepareLateUpdate(endIndexThreaded);

			SkeletonPartitionRange[] asyncPartitionsItems = asyncTaskPartitions.Items;
			for (int taskIndex = 0, count = asyncTaskPartitions.Count; taskIndex < count; ++taskIndex) {
				SkeletonPartitionRange partition = asyncPartitionsItems[taskIndex];
				if (partition.rangeStart == partition.rangeEndExclusive) {
#if !DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
					lateUpdateDone[taskIndex].Set();
#endif
					continue;
				}
				var range = new SkeletonUpdateRange() {
					rangeStart = partition.rangeStart,
					rangeEndExclusive = partition.rangeEndExclusive,
					taskIndex = taskIndex,
					frameCount = Time.frameCount,
					updateTiming = UpdateTiming.InLateUpdate
				};
				LateUpdateSkeletonsAsync(range, partition.threadIndex);
			}
#if ENABLE_WORK_STEALING
			workerPool.AllowTaskProcessing(numAsyncThreads);
#endif
			SkeletonPartitionRange lastAsyncPartition = asyncPartitionsItems[asyncTaskPartitions.Count - 1];
			if (lastAsyncPartition.rangeEndExclusive < skeletonEnd) {
				// this main thread does some work as well, otherwise it's only waiting.
				var range = new SkeletonUpdateRange() {
					rangeStart = lastAsyncPartition.rangeEndExclusive,
					rangeEndExclusive = skeletonEnd,
					taskIndex = -1,
					frameCount = Time.frameCount,
					updateTiming = UpdateTiming.InLateUpdate
				};
				LateUpdateSkeletonsSynchronous(range);
			}

#if RUN_ALL_ON_MAIN_THREAD
			return; // nothing left to do after all processed as LateUpdateSkeletonsSynchronous
#endif

#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			for (int t = 0; t < numAsyncTasks; ++t) {
				mainThreadProcessedAtTask[t] = 0;
			}
			bool anySkeletonsLeft = false;
			bool timedOut = false;
			do {
				bool wasWorkAvailable = false;
				anySkeletonsLeft = false;
				for (int t = 0; t < numAsyncTasks; ++t) {
					SkeletonPartitionRange partition = asyncPartitionsItems[t];

					int rendererStartIndex = partition.rangeStart;
					int countAtTask = partition.rangeEndExclusive - rendererStartIndex;
#if READ_VOLATILE_ONCE
					int updatedAtWorkerThread = skeletonsLateUpdatedAtTask[t];

					while (mainThreadProcessedAtTask[t] < updatedAtWorkerThread) {
#else
					while (mainThreadProcessed[t] < skeletonsLateUpdatedAtThread[t]) {
#endif
						wasWorkAvailable = true;
						int r = mainThreadProcessedAtTask[t] + rendererStartIndex;
						var skeletonRenderer = this.skeletonRenderers[r];
						if (skeletonRenderer.RequiresMeshBufferAssignmentMainThread)
							skeletonRenderer.UpdateMeshAndMaterialsToBuffers();
						mainThreadProcessedAtTask[t]++;
					}

#if READ_VOLATILE_ONCE
					if (updatedAtWorkerThread < countAtTask) {
#else
					if (skeletonsLateUpdatedAtThread[t] < countAtTask) {
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
			WaitForThreadLateUpdateTasks(numAsyncTasks);

			// Additional main thread update when the mesh data could not be assigned from worker thread
			// and has to be assigned from main thread.
			for (int r = 0; r < endIndexThreaded; ++r) {
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

		private void WaitForThreadUpdateTasks (int numAsyncTasks) {
			for (int t = 0; t < numAsyncTasks; ++t) {
				int timeoutMilliseconds = 1000;
#if HAS_MANUAL_RESET_EVENT_SLIM
				bool success = updateDone[t].Wait(timeoutMilliseconds);
#else // HAS_MANUAL_RESET_EVENT_SLIM
				bool success = updateDone[t].WaitOne(timeoutMilliseconds);
#endif // HAS_MANUAL_RESET_EVENT_SLIM
				if (!success)
					Debug.LogError(string.Format("Waiting for updateDone on main thread ran into a timeout (task index: {0})!", t));
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

#if !DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
		private void WaitForThreadLateUpdateTasks (int numAsyncTasks) {
			for (int t = 0; t < numAsyncTasks; ++t) {
				int timeoutMilliseconds = 1000;
#if HAS_MANUAL_RESET_EVENT_SLIM
				bool success = lateUpdateDone[t].Wait(timeoutMilliseconds);
#else // HAS_MANUAL_RESET_EVENT_SLIM
				bool success = lateUpdateDone[t].WaitOne(timeoutMilliseconds);
#endif // HAS_MANUAL_RESET_EVENT_SLIM
				if (!success)
					Debug.LogError(string.Format("Waiting for lateUpdateDone on main thread ran into a timeout (task index: {0})!", t));
			}
		}
#endif // !DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS

#if SPINE_ENABLE_THREAD_PROFILING
		CustomSampler[] profilerSamplerUpdate = null;
		CustomSampler[] profilerSamplerLateUpdate = null;
#endif

		/// <summary>Perform Update at all SkeletonRenderers asynchronously.</summary>
		void UpdateSkeletonsAsync (SkeletonUpdateRange range, int threadIndex) {
#if SPINE_ENABLE_THREAD_PROFILING
			if (profilerSamplerUpdate[threadIndex] == null) {
				profilerSamplerUpdate[threadIndex] = CustomSampler.Create("Spine Update " + threadIndex);
			}
#endif
			WorkerPoolTask task = genericSkeletonTasks[range.taskIndex];
			task.parameters = range;
			task.function = cachedUpdateSkeletonsAsyncImpl;
			bool enqueueSucceeded;
			do {
				enqueueSucceeded = workerPool.EnqueueTask(threadIndex, task);
			} while (!enqueueSucceeded);
		}
		// avoid allocation, unfortunately this is really necessary
		static Action<SkeletonUpdateRange, int> cachedUpdateSkeletonsAsyncImpl = UpdateSkeletonsAsyncImpl;
		static void UpdateSkeletonsAsyncImpl (SkeletonUpdateRange range, int threadIndex) {
			var instance = Instance;
#if SPINE_ENABLE_THREAD_PROFILING
			if (instance.profilerSamplerUpdate[threadIndex] == null) {
				instance.profilerSamplerUpdate[threadIndex] = CustomSampler.Create("Spine Update " + threadIndex);
			}
			instance.profilerSamplerUpdate[threadIndex].Begin();
#endif
			int frameCount = range.frameCount;
			int start = range.rangeStart;
			int end = range.rangeEndExclusive;
			int taskIndex = range.taskIndex;
			var skeletonAnimations = instance.skeletonAnimationsUpdate;
			if (range.updateTiming == UpdateTiming.InFixedUpdate) skeletonAnimations = instance.skeletonAnimationsFixedUpdate;
			else if (range.updateTiming == UpdateTiming.InLateUpdate) skeletonAnimations = instance.skeletonAnimationsLateUpdate;

			for (int r = start; r < end; ++r) {
				try {
					skeletonAnimations[r].UpdateExternal(frameCount, calledFromOnlyMainThread: false);
				} catch (Exception exc) {
					instance.DeferredLogException(exc, skeletonAnimations[r], threadIndex);
				}
			}
			instance.updateDone[taskIndex].Set();
#if SPINE_ENABLE_THREAD_PROFILING
				instance.profilerSamplerUpdate[threadIndex].End();
#endif
		}

		//------------------------------------------------------------------------------------------
		/// <summary>Perform Update at all SkeletonRenderers asynchronously and split off at
		/// main-thread callbacks.</summary>
		void UpdateSkeletonsAsyncSplit (SkeletonUpdateRange range, int threadIndex) {

#if SPINE_ENABLE_THREAD_PROFILING
			if (profilerSamplerUpdate[threadIndex] == null) {
				profilerSamplerUpdate[threadIndex] = CustomSampler.Create("Spine Update " + threadIndex);
			}
#endif
			bool enqueueSucceeded;
			do {
				WorkerPoolTask task = genericSkeletonTasks[range.taskIndex];
				task.parameters = range;
				task.function = cachedUpdateSkeletonsAsyncSplitImpl;
				enqueueSucceeded = workerPool.EnqueueTask(threadIndex, task);
			} while (!enqueueSucceeded);
		}
		// avoid allocation, unfortunately this is really necessary
		static Action<SkeletonUpdateRange, int> cachedUpdateSkeletonsAsyncSplitImpl = UpdateSkeletonsAsyncSplitImpl;
		static void UpdateSkeletonsAsyncSplitImpl (SkeletonUpdateRange range, int threadIndex) {
			int frameCount = range.frameCount;
			int start = range.rangeStart;
			int end = range.rangeEndExclusive;
			int taskIndex = range.taskIndex;
			var instance = Instance;
			var skeletonAnimations = instance.skeletonAnimationsUpdate;
			if (range.updateTiming == UpdateTiming.InFixedUpdate) skeletonAnimations = instance.skeletonAnimationsFixedUpdate;
			else if (range.updateTiming == UpdateTiming.InLateUpdate) skeletonAnimations = instance.skeletonAnimationsLateUpdate;

			var splitUpdateMethod = instance.splitUpdateMethod;

#if SPINE_ENABLE_THREAD_PROFILING
			if (instance.profilerSamplerUpdate[threadIndex] == null) {
				instance.profilerSamplerUpdate[threadIndex] = CustomSampler.Create("Spine Update " + threadIndex);
			}
			instance.profilerSamplerUpdate[threadIndex].Begin();
#endif
			for (int r = start; r < end; ++r) {
				try {
					SkeletonAnimationBase targetSkeletonAnimation = skeletonAnimations[r];
					if (!splitUpdateMethod[r].IsDone) {
						splitUpdateMethod[r] = targetSkeletonAnimation.UpdateInternalSplit(splitUpdateMethod[r], frameCount);
					}
				} catch (Exception exc) {
					instance.DeferredLogException(exc, skeletonAnimations[r], threadIndex);
				}
			}
			instance.updateDone[taskIndex].Set();

#if SPINE_ENABLE_THREAD_PROFILING
			instance.profilerSamplerUpdate[threadIndex].End();
#endif
		}

		bool UpdateSkeletonsMainThreadSplit (List<SkeletonAnimationBase> skeletons, int endIndexThreaded,
			int frameCount) {
			bool anyWorkLeft = false;

			for (int r = 0; r < endIndexThreaded; ++r) {
				try {
					SkeletonAnimationBase targetSkeletonAnimation = skeletons[r];
					if (splitUpdateMethod[r].IsInitialState) {
						Debug.LogError("Internal threading logic error: skeletonAnimations never called UpdateInternal before!", skeletons[r]);
					} else {
						if (!splitUpdateMethod[r].IsDone) {
							anyWorkLeft = true;
							splitUpdateMethod[r] = targetSkeletonAnimation.UpdateInternalSplit(splitUpdateMethod[r], frameCount);
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
			int frameCount = range.frameCount;

			for (int r = start; r < end; ++r) {
				skeletons[r].UpdateExternal(frameCount, calledFromOnlyMainThread: true);
			}
		}

		/// <summary>Perform LateUpdate at all SkeletonRenderers asynchronously.</summary>
		static Action<SkeletonUpdateRange, int> cachedLateUpdateSkeletonsAsyncImpl = LateUpdateSkeletonsAsyncImpl;
		void LateUpdateSkeletonsAsync (SkeletonUpdateRange range, int threadIndex) {
#if SPINE_ENABLE_THREAD_PROFILING
			if (profilerSamplerLateUpdate[threadIndex] == null) {
				profilerSamplerLateUpdate[threadIndex] = CustomSampler.Create("Spine LateUpdate " + threadIndex);
			}
#endif
			bool enqueueSucceeded;
			WorkerPoolTask task = genericSkeletonTasks[range.taskIndex];
			task.parameters = range;
			task.function = cachedLateUpdateSkeletonsAsyncImpl;
			do {
				enqueueSucceeded = workerPool.EnqueueTask(threadIndex, task);
			} while (!enqueueSucceeded);
		}

		static void LateUpdateSkeletonsAsyncImpl (SkeletonUpdateRange range, int threadIndex) {
			int start = range.rangeStart;
			int end = range.rangeEndExclusive;
			int taskIndex = range.taskIndex; 
			var instance = Instance;

#if SPINE_ENABLE_THREAD_PROFILING
			if (instance.profilerSamplerLateUpdate[threadIndex] == null) {
				instance.profilerSamplerLateUpdate[threadIndex] = CustomSampler.Create("Spine LateUpdate " + threadIndex);
			}
			instance.profilerSamplerLateUpdate[threadIndex].Begin();
#endif
#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			instance.skeletonsLateUpdatedAtTask[taskIndex] = 0;
#endif
			for (int r = start; r < end; ++r) {
				try {
					instance.skeletonRenderers[r].LateUpdateImplementation(calledFromMainThread: false);
				} catch (Exception exc) {
					instance.DeferredLogException(exc, instance.skeletonRenderers[r].Component, threadIndex);
				}
#if DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
				Interlocked.Increment(ref instance.skeletonsLateUpdatedAtTask[taskIndex]);
				instance.lateUpdateWorkAvailable.Set(); // signal as soon as it can be processed by main thread
#endif
			}
#if !DONT_WAIT_FOR_ALL_LATEUPDATE_TASKS
			instance.lateUpdateDone[taskIndex].Set(); // signal once after all work is done
#endif
#if SPINE_ENABLE_THREAD_PROFILING
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
