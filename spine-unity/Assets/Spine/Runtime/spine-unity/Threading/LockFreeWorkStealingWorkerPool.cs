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

using ResetEvent = System.Threading.ManualResetEvent;

/// <summary>
/// Worker pool with a single submission deque owned by the submitting thread. The submitter pushes at the
/// bottom, workers only steal from the top, so tasks are taken in FIFO order by whichever worker is running.
/// The deque supports pushing while workers steal, so the submitter never has to wait for idle workers between
/// batches: a batch is complete when every pushed task has been executed, tracked by a counter and a done event.
/// </summary>
public class LockFreeWorkStealingWorkerPool<T> : IDisposable {
	public class Task {
		public T parameters;
		public Action<T, int> function;
	}

	private readonly int _threadCount;
	private readonly Thread[] _threads;
	private readonly LockFreeWorkStealingDeque<Task> _tasks;
	private readonly AutoResetEvent[] _taskAvailable;
	private readonly ResetEvent _allTasksDone;
	private Exception _workerException;
	/// <summary>Pushed tasks not yet executed. Counts one extra from the first <see cref="EnqueueTask"/> of a batch
	/// until <see cref="AllowTaskProcessing"/>, so early execution by a still-stealing worker cannot signal completion
	/// while tasks are still being added.</summary>
	private int _tasksRemaining;
	/// <summary>Accessed only by the submitting thread, not by workers.</summary>
	private bool _batchOpen;
	/// <summary>Accessed only by the submitting thread, not by workers.</summary>
	private bool _processingTasks;
	private volatile bool _running = true;

	/// <summary>The first recorded worker exception, or null. Can be read without waiting for task completion.</summary>
	public Exception WorkerException {
		get { return Interlocked.CompareExchange(ref _workerException, null, null); }
	}

	public LockFreeWorkStealingWorkerPool (int threadCount, int queueCapacity = 8) {
		_threadCount = threadCount;
		_threads = new Thread[_threadCount];
		_tasks = new LockFreeWorkStealingDeque<Task>(queueCapacity * threadCount);
		_taskAvailable = new AutoResetEvent[_threadCount];
		_allTasksDone = new ResetEvent(true);

		for (int i = 0; i < _threadCount; i++) {
			_taskAvailable[i] = new AutoResetEvent(false);
			int index = i; // Capture the index for the thread
			_threads[i] = new Thread(() => WorkerLoop(index));
			_threads[i].IsBackground = true; // never keeps the process alive, also after Abandon
		}
		for (int i = 0; i < _threadCount; i++) {
			_threads[i].Start();
		}
	}

	/// <summary>Enqueues a task item. A worker that is still stealing from the preceding batch may execute it
	/// immediately, so the task's inputs must be complete before this call. Join the preceding batch with
	/// <see cref="WaitUntilIdle"/> before modifying task objects or enqueueing.
	/// EnqueueTask, AllowTaskProcessing and WaitUntilIdle must be called from the same submitting thread.</summary>
	/// <param name="threadIndex">Ignored, for API compatibility only, tasks are not bound to a worker.</param>
	/// <returns>True if the item was successfully enqueued, false otherwise.</returns>
	public bool EnqueueTask (int threadIndex, Task task) {
		if (threadIndex < 0 || threadIndex >= _threadCount)
			throw new ArgumentOutOfRangeException("threadIndex");
		if (_processingTasks)
			throw new InvalidOperationException("Call WaitUntilIdle before enqueueing another batch.");

		if (!_batchOpen) {
			// First task of the batch + 1, so count cannot reach 0 while tasks are still being added.
			// AllowTaskProcessing removes the extra one again.
			_allTasksDone.Reset();
			Interlocked.Exchange(ref _tasksRemaining, 2);
			_batchOpen = true;
		} else {
			Interlocked.Increment(ref _tasksRemaining);
		}
		_tasks.Push(task); // publishes the task to stealing workers
		return true;
	}

	/// <summary>
	/// Call this method after <see cref="EnqueueTask"/> to wake workers and complete the batch submission.
	/// No task objects may be modified until <see cref="WaitUntilIdle"/> successfully joins the batch.
	/// </summary>
	/// <param name="numAsyncThreads">Number of worker threads to wake. Workers still stealing from the preceding
	/// batch take part regardless.</param>
	public void AllowTaskProcessing (int numAsyncThreads) {
		if (numAsyncThreads < 0 || numAsyncThreads > _threadCount)
			throw new ArgumentOutOfRangeException("numAsyncThreads");
		if (_processingTasks)
			throw new InvalidOperationException("Call WaitUntilIdle before starting another batch.");
		if (numAsyncThreads == 0 || !_batchOpen) return;

		_processingTasks = true;
		for (int t = 0; t < numAsyncThreads; ++t)
			_taskAvailable[t].Set();
		// Remove the extra count of the submission phase, completing the batch if every task already ran.
		if (Interlocked.Decrement(ref _tasksRemaining) == 0)
			_allTasksDone.Set();
	}

	/// <summary>Waits until every task of the batch has been executed. Only a successful wait permits task objects
	/// to be reused. Worker failures are rethrown here.</summary>
	public bool WaitUntilIdle (int timeoutMilliseconds) {
		if (!_processingTasks) return true;
		bool success = _allTasksDone.WaitOne(timeoutMilliseconds);
		if (!success) return false;

		if (_workerException != null)
			throw new InvalidOperationException("A Spine worker failed.", _workerException);
		_processingTasks = false;
		_batchOpen = false;
		return true;
	}

	private void WorkerLoop (int threadIndex) {
#if SPINE_ENABLE_THREAD_PROFILING
		Profiler.BeginThreadProfiling("Spine Threads", "Spine Thread " + threadIndex);
#endif
		while (_running) {
			_taskAvailable[threadIndex].WaitOne();
			while (_running) {
				Task task;
				bool empty;
				if (!_tasks.Steal(out task, out empty)) {
					if (empty) break;
					continue; // another worker took the top element, retry
				}
				try {
					task.function(task.parameters, threadIndex);
				} catch (Exception exc) {
					Interlocked.CompareExchange(ref _workerException, exc, null);
				} finally {
					if (Interlocked.Decrement(ref _tasksRemaining) == 0)
						_allTasksDone.Set();
				}
			}
		}
#if SPINE_ENABLE_THREAD_PROFILING
		Profiler.EndThreadProfiling();
#endif
	}

	/// <summary>Releases the worker threads without waiting for them, for use when a batch ran into a timeout or
	/// a worker failed. Idle workers exit. A worker inside a task exits only if that task ever returns. A worker stuck
	/// in it stays alive as a background thread, holding its stack and whatever it holds, without touching the queue
	/// again. The caller continues with a new pool. The events stay open: workers woken from their wake-up event by
	/// this call may still be inside that wait, and a worker returning from its task signals the done event (a closed
	/// handle would throw on that thread).</summary>
	public void Abandon () {
		_running = false;
		for (int i = 0; i < _threadCount; i++)
			_taskAvailable[i].Set();
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
		_allTasksDone.Close();
	}
}
