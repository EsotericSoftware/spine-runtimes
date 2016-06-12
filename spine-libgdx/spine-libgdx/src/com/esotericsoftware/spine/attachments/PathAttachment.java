/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.badlogic.gdx.graphics.Color;
import com.esotericsoftware.spine.Slot;

public class PathAttachment extends VertexAttachment {
	float[] lengths;
	boolean closed, constantSpeed;

	// Nonessential.
	final Color color = new Color(1, 0.5f, 0, 1);

	public PathAttachment (String name) {
		super(name);
	}

	public void computeWorldVertices (Slot slot, float[] worldVertices) {
		super.computeWorldVertices(slot, worldVertices);
	}

	public void computeWorldVertices (Slot slot, int start, int count, float[] worldVertices, int offset) {
		super.computeWorldVertices(slot, start, count, worldVertices, offset);
	}

	public boolean getClosed () {
		return closed;
	}

	public void setClosed (boolean closed) {
		this.closed = closed;
	}

	public boolean getConstantSpeed () {
		return constantSpeed;
	}

	public void setConstantSpeed (boolean constantSpeed) {
		this.constantSpeed = constantSpeed;
	}

	/** Returns the length in the setup pose from the start of the path to the end of each curve. */
	public float[] getLengths () {
		return lengths;
	}

	public void setLengths (float[] lengths) {
		this.lengths = lengths;
	}

	public Color getColor () {
		return color;
	}
}
