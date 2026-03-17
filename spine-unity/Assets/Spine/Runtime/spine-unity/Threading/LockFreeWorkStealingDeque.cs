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

using System.Threading;

/// <summary>
/// A generic lock-free deque supporting work-stealing based on the paper
/// "Dynamic Circular Work-Stealing Deque", authors David Chase and Yossi Lev
/// https://www.dre.vanderbilt.edu/~schmidt/PDF/work-stealing-dequeue.pdf.
/// Requires that Push and Pop are called from the same thread.
/// Simplified by not supporting growing the array size during a Push,
/// in our usage scenario we populate tasks ahead of time before the first Pop.
/// </summary>
public class LockFreeWorkStealingDeque<T> {
	public static readonly T Empty = default(T);
	public static readonly T Abort = default(T);

	private /*volatile*/ CircularArray<T> activeArray;
	private volatile int bottom = 0;
	private volatile int top = 0;

	public int Capacity { get { return activeArray.Size; } }

	public LockFreeWorkStealingDeque (int capacity) {
		capacity = UnityEngine.Mathf.NextPowerOfTwo(capacity);
		activeArray = new CircularArray<T>(capacity);
		bottom = 0;
		top = 0;
	}

	/// <summary>Push an element (at the bottom), has to be called by owner of the deque, not a thief.</summary>
	public void Push (T item) {
		int b = bottom;
		int t = top;
		CircularArray<T> a = this.activeArray;
		int size = b - t;
		if (size >= a.Size - 1) {
			a = a.Grow(b, t, a.Size * 2);
			this.activeArray = a;
		}
		a.Put(b, item);
		bottom = b + 1;
	}

	/// <summary>Non-standard addition for ahead-of-time pushing to maintain queue FIFO order.
	/// Push an element at the top, must only be called before any other thread calls Push, Pop or Steal.</summary>
	public void PushTop (T item) {
		int b = bottom;
		int t = top;
		CircularArray<T> a = this.activeArray;
		int size = b - t;
		if (size >= a.Size - 1) {
			a = a.Grow(b, t, a.Size * 2);
			this.activeArray = a;
		}
		int newT = t - 1;
		a.Put(newT, item);
		top = newT;
	}

	/// <summary>
	/// Makes a different worker than the owner steal an element (from the top).
	/// Returns false if empty.
	/// </summary>
	public bool Steal (out T item) {
		int t = top;
		int b = bottom;
		CircularArray<T> a = this.activeArray;
		int size = b - t;
		if (size <= 0) {
			item = Empty;
			return false;
		}
		T o = a.Get(t);
		// increment top
		if (Interlocked.CompareExchange(ref top, t + 1, t) != t) {
			item = Abort;
			return false;
		}
		item = o;
		return true;
	}

	/// <summary>Pop an element (from the bottom), has to be called by owner of the deque, not a thief.</summary>
	/// <returns>false if empty.</returns>
	public bool Pop (out T item) {
		int b = bottom;
		CircularArray<T> a = this.activeArray;
		--b;
		this.bottom = b;
		int t = top;
		int size = b - t;
		if (size < 0) {
			bottom = t;
			item = Empty;
			return false;
		}
		T o = a.Get(b);
		if (size > 0) {
			item = o;
			return true;
		}

		bool wasSuccessful = true;
		if (Interlocked.CompareExchange(ref top, t + 1, t) != t) {
			item = Empty;
			wasSuccessful = false;
		}
		else {
			item = o;
		}
		bottom = t + 1;
		return wasSuccessful;
	}
}