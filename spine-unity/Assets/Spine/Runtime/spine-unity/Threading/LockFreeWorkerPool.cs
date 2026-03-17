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

using System;
using System.Collections.Generic;
using System.Threading;
#if SPINE_ENABLE_THREAD_PROFILING
using UnityEngine.Profiling;
#endif

/// Class to distribute work items like ThreadPool.QueueUserWorkItem but keep the same tasks at the same thread
/// across frames, increasing core affinity (and in some scenarios with lower cache pressure on secondary cores,
/// perhaps even reduce cache eviction).
public class LockFreeWorkerPool<T> : IDisposable {
	public class Task {
		public T parameters;
		public Action<T, int> function;
	}

	private readonly int _threadCount;
	private readonly Thread[] _threads;
	private readonly LockFreeSPSCQueue<Task>[] _taskQueues;
	private readonly AutoResetEvent[] _taskAvailable;
	private volatile bool _running = true;

	public LockFreeWorkerPool (int threadCount, int queueCapacity = 2) {
		_threadCount = threadCount;
		_threads = new Thread[_threadCount];
		_taskQueues = new LockFreeSPSCQueue<Task>[_threadCount];
		_taskAvailable = new AutoResetEvent[_threadCount];

		for (int i = 0; i < _threadCount; i++) {
			_taskQueues[i] = new LockFreeSPSCQueue<Task>(queueCapacity);
			_taskAvailable[i] = new AutoResetEvent(false);

			int index = i; // Capture the index for the thread
			_threads[i] = new Thread(() => WorkerLoop(index));
			_threads[i].Start();
		}
	}

	/// <summary>Enqueues a task item if there is space available.</summary>
	/// <returns>True if the item was successfully enqueued, false otherwise.</returns>
	public bool EnqueueTask (int threadIndex, Task task) {
		if (threadIndex < 0 || threadIndex >= _threadCount)
			throw new ArgumentOutOfRangeException("threadIndex");

		bool success = _taskQueues[threadIndex].Enqueue(task);
		if (!success) {
			return false;
		}

		_taskAvailable[threadIndex].Set();
		return true;
	}

	private void WorkerLoop (int threadIndex) {
#if SPINE_ENABLE_THREAD_PROFILING
		Profiler.BeginThreadProfiling("Spine Threads", "Spine Thread " + threadIndex);
#endif
		while (_running) {
			Task task = null;
			bool success = _taskQueues[threadIndex].Dequeue(out task);
			if (success) {
				task.function(task.parameters, threadIndex);
			} else {
				_taskAvailable[threadIndex].WaitOne();
			}
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
