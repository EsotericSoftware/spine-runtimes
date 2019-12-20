/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

package com.esotericsoftware.spine.utils;

public class SpineUtils {
	static public final float PI = 3.1415927f;
	static public final float PI2 = PI * 2;
	static public final float radiansToDegrees = 180f / PI;
	static public final float radDeg = radiansToDegrees;
	static public final float degreesToRadians = PI / 180;
	static public final float degRad = degreesToRadians;

	public static float cosDeg (float angle) {
		return (float)Math.cos(angle * degRad);
	}

	public static float sinDeg (float angle) {
		return (float)Math.sin(angle * degRad);
	}

	public static float cos (float angle) {
		return (float)Math.cos(angle);
	}

	public static float sin (float angle) {
		return (float)Math.sin(angle);
	}

	public static float atan2 (float y, float x) {
		return (float)Math.atan2(y, x);
	}

	static public void arraycopy (Object src, int srcPos, Object dest, int destPos, int length) {
		if (src == null) throw new IllegalArgumentException("src cannot be null.");
		if (dest == null) throw new IllegalArgumentException("dest cannot be null.");
		try {
			System.arraycopy(src, srcPos, dest, destPos, length);
		} catch (ArrayIndexOutOfBoundsException ex) {
			throw new ArrayIndexOutOfBoundsException( //
				"Src: " + java.lang.reflect.Array.getLength(src) + ", " + srcPos //
					+ ", dest: " + java.lang.reflect.Array.getLength(dest) + ", " + destPos //
					+ ", count: " + length);
		}
	}
}
