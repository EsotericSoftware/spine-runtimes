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

#define ENABLE_WORK_STEALING

using System;
using System.Collections.Generic;
using System.Threading;
#if SPINE_ENABLE_THREAD_PROFILING
using UnityEngine.Profiling;
#endif

/// Class to distribute work items like ThreadPool.QueueUserWorkItem but keep the same tasks at the same thread
/// across frames, increasing core affinity (and in some scenarios with lower cache pressure on secondary cores,
/// perhaps even reduce cache eviction).
public class LockFreeWorkStealingWorkerPool<T> : IDisposable {
	public class Task {
		public T parameters;
		public Action<T, int> function;
	}

	private readonly int _threadCount;
	private readonly Thread[] _threads;
	private readonly LockFreeWorkStealingDeque<Task>[] _taskQueues;
	private readonly AutoResetEvent[] _taskAvailable;
	private volatile bool _running = true;

	public LockFreeWorkStealingWorkerPool (int threadCount, int queueCapacity = 8) {
		_threadCount = threadCount;
		_threads = new Thread[_threadCount];
		_taskQueues = new LockFreeWorkStealingDeque<Task>[_threadCount];
		_taskAvailable = new AutoResetEvent[_threadCount];

		for (int i = 0; i < _threadCount; i++) {
			_taskQueues[i] = new LockFreeWorkStealingDeque<Task>(queueCapacity);
			_taskAvailable[i] = new AutoResetEvent(false);
			int index = i; // Capture the index for the thread
			_threads[i] = new Thread(() => WorkerLoop(index));
		}
		for (int i = 0; i < _threadCount; i++) {
			_threads[i].Start();
		}
	}

	/// <summary>Enqueues a task item if there is space available, but does
	/// not start processing until <see cref="AllowTaskProcessing"/> is called.</summary>
	/// <returns>True if the item was successfully enqueued, false otherwise.</returns>
	public bool EnqueueTask (int threadIndex, Task task) {
		if (threadIndex < 0 || threadIndex >= _threadCount)
			throw new ArgumentOutOfRangeException("threadIndex");

		_taskQueues[threadIndex].PushTop(task);
		return true;
	}

	/// <summary>
	/// Call this method after <see cref="EnqueueTaskWithoutProcessing"/> to start processing all enqueued tasks.
	/// This limitation comes from LockFreeWorkStealingDeque requiring the same thread calling Push and Pop,
	/// which would not be the case here.
	/// </summary>
	/// <param name="numAsyncThreads">Limits the number of active worker threads. Note that when work stealing is
	/// enabled, empty threads steal tasks from other threads even if no tasks were originally enqueued at them.</param>
	public void AllowTaskProcessing (int numAsyncThreads) {
		for (int t = 0; t < numAsyncThreads; ++t)
			_taskAvailable[t].Set();
	}

	private void WorkerLoop (int threadIndex) {
#if SPINE_ENABLE_THREAD_PROFILING
		Profiler.BeginThreadProfiling("Spine Threads", "Spine Thread " + threadIndex);
#endif
		while (_running) {
			_taskAvailable[threadIndex].WaitOne();
			Task task = null;
			bool success;
			do {
				success = _taskQueues[threadIndex].Pop(out task);
				if (success) {
					task.function(task.parameters, threadIndex);
				} else {
	#if ENABLE_WORK_STEALING
					int stealThreadIndex = (threadIndex + 1) % _threadCount;
					while (stealThreadIndex != threadIndex) { // circle complete
						while (true) {
							task = null;
							bool stealSuccessful = _taskQueues[stealThreadIndex].Steal(out task);
							if (!stealSuccessful)
								break;
							task.function(task.parameters, threadIndex);
						}
						stealThreadIndex = (stealThreadIndex + 1) % _threadCount;
					}
	#endif
				}
			} while (success);
		}
#if SPINE_ENABLE_THREAD_PROFILING
		Profiler.EndThreadProfiling();
#endif
	}

	public void Dispose () {
		_running = false;

		for (int i = 0; i < _threadCount; i++) {
			_taskAvailable[i].Set(); // Wake up threads to exit
		}
		
		foreach (var thread in _threads) {
			thread.Join();
		}

		for (int i = 0; i < _threadCount; i++) {
			_taskAvailable[i].Close();
		}
	}
}
