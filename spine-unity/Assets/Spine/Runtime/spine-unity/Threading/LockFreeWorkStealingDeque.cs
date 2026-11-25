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

#if NET_STANDARD || NET_STANDARD_2_0 || NET_STANDARD_2_1 || NET_4_6
#define HAS_SYSTEM_THREADING_VOLATILE
#endif

using System.Threading;

/// <summary>
/// A generic lock-free deque supporting work-stealing based on the paper
/// "Dynamic Circular Work-Stealing Deque", authors David Chase and Yossi Lev
/// https://www.dre.vanderbilt.edu/~schmidt/PDF/work-stealing-dequeue.pdf.
/// Requires that Push and Pop are called from the same thread.
/// </summary>
public class LockFreeWorkStealingDeque<T> {
	public static readonly T Empty = default(T);
	public static readonly T Abort = default(T);

	private /*volatile*/ CircularArray<T> activeArray;
	private int bottom = 0;
	private int top = 0;

	public int Capacity { get { return activeArray.Size; } }

	public LockFreeWorkStealingDeque (int capacity) {
		capacity = UnityEngine.Mathf.NextPowerOfTwo(capacity);
		activeArray = new CircularArray<T>(capacity);
		bottom = 0;
		top = 0;
	}

	static int VolatileRead (ref int location) {
#if HAS_SYSTEM_THREADING_VOLATILE
		return Volatile.Read(ref location);
#else
		return Thread.VolatileRead(ref location);
#endif
	}

	static void VolatileWrite (ref int location, int value) {
#if HAS_SYSTEM_THREADING_VOLATILE
		Volatile.Write(ref location, value);
#else
		Thread.VolatileWrite(ref location, value);
#endif
	}

	/// <summary>Push an element (at the bottom), has to be called by owner of the deque, not a thief.</summary>
	public void Push (T item) {
		int b = bottom;
		int t = VolatileRead(ref top);
		CircularArray<T> a = this.activeArray;
		int size = b - t;
		if (size >= a.Size - 1) {
			a = a.Grow(b, t, a.Size * 2);
			Thread.MemoryBarrier(); // requires full fence to publish the copied elements before the new array reference.
			this.activeArray = a;
		}
		a.Put(b, item);
		VolatileWrite(ref bottom, b + 1);
	}

	/// <summary>
	/// Makes a different worker than the owner steal an element (from the top).
	/// Returns false if empty or if another thief took the element.
	/// </summary>
	public bool Steal (out T item) {
		bool empty;
		return Steal(out item, out empty);
	}

	/// <summary>
	/// Makes a different worker than the owner steal an element (from the top).
	/// Returns false if empty or if another thief took the element.
	/// </summary>
	/// <param name="empty">True if the deque was empty. False with a false return value means another thief
	/// took the element, the caller may retry.</param>
	public bool Steal (out T item, out bool empty) {
		int t = top;
		Thread.MemoryBarrier(); // requires full fence between observing top and bottom.
		int b = VolatileRead(ref bottom);
		CircularArray<T> a = this.activeArray;
		int size = b - t;
		if (size <= 0) {
			item = Empty;
			empty = true;
			return false;
		}
		empty = false;
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
		Thread.MemoryBarrier(); // requires full fence here to prevent store-load reordering
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
		} else {
			item = o;
		}
		bottom = t + 1;
		return wasSuccessful;
	}
}
