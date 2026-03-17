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
using System.Threading;

/// <summary>
/// A generic lock-free single-producer single-consumer queue.
/// </summary>
public class LockFreeSPSCQueue<T> {
	private readonly int capacity;
	private readonly T[] circularBuffer;
	private int headIndex;
	private int tailIndex;

	public LockFreeSPSCQueue (int allocatedCapacity) {
		capacity = allocatedCapacity;
		circularBuffer = new T[allocatedCapacity];
		headIndex = tailIndex = 0;
	}

	/// <summary>Enqueues an item if there is space available.</summary>
	/// <returns>True if the item was successfully enqueued, false otherwise.</returns>
	public bool Enqueue (T item) {
		int head = Thread.VolatileRead(ref headIndex);
		int nextHead = (head + 1) % capacity;

		if (nextHead == Thread.VolatileRead(ref tailIndex))
			return false; // queue is full

		circularBuffer[head] = item;
		Thread.VolatileWrite(ref headIndex, nextHead);
		return true;
	}

	/// <summary>
	/// Dequeues an item unless the queue is empty.
	/// </summary>
	/// <param name="item">The dequeued item, or the default element if empty.</param>
	/// <returns>True if the item was successfully dequeued, false otherwise.</returns>
	public bool Dequeue (out T item) {
		int tail = Thread.VolatileRead(ref tailIndex);

		if (tail == Thread.VolatileRead(ref headIndex)) {
			item = default(T); // queue is empty
			return false;
		}

		item = circularBuffer[tail];
		int nextTail = (tail + 1) % capacity;
		Thread.VolatileWrite(ref tailIndex, nextTail);
		return true;
	}
}
