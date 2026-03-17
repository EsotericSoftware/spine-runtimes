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
/// An array with wrap-around access for LockFreeWorkStealingDeque based on the paper
/// "Dynamic Circular Work-Stealing Deque", authors David Chase and Yossi Lev.
/// https://www.dre.vanderbilt.edu/~schmidt/PDF/work-stealing-dequeue.pdf
/// Modified to support negative indices.
/// </summary>
public class CircularArray<T> {
	private int size;
	private T[] segment;
	private uint mask;

	public int Size { get { return size; } }

	public CircularArray (int sizePoT) {
		this.size = sizePoT;
		segment = new T[sizePoT];
		mask = (uint)(sizePoT - 1);
	}

	public T Get (int i) {
		return this.segment[((uint)i) & mask];
	}

	public void Put (int i, T item) {
		this.segment[((uint)i) & mask] = item;
	}

	public CircularArray<T> Grow (int b, int t, int newSizePoT) {
		CircularArray<T> a = new CircularArray<T>(newSizePoT);
		for (int i = t; i < b; i++) {
			a.Put(i, this.Get(i));
		}
		return a;
	}
}
