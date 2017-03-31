/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.attachments;

import com.badlogic.gdx.graphics.Color;
import com.esotericsoftware.spine.PathConstraint;

/** An attachment whose vertices make up a composite Bezier curve.
 * <p>
 * See {@link PathConstraint} and <a href="http://esotericsoftware.com/spine-paths">Paths</a> in the Spine User Guide. */
public class PathAttachment extends VertexAttachment {
	float[] lengths;
	boolean closed, constantSpeed;

	// Nonessential.
	final Color color = new Color(1, 0.5f, 0, 1); // ff7f00ff

	public PathAttachment (String name) {
		super(name);
	}

	/** If true, the start and end knots are connected. */
	public boolean getClosed () {
		return closed;
	}

	public void setClosed (boolean closed) {
		this.closed = closed;
	}

	/** If true, additional calculations are performed to make calculating positions along the path more accurate. If false, fewer
	 * calculations are performed but calculating positions along the path is less accurate. */
	public boolean getConstantSpeed () {
		return constantSpeed;
	}

	public void setConstantSpeed (boolean constantSpeed) {
		this.constantSpeed = constantSpeed;
	}

	/** The lengths along the path in the setup pose from the start of the path to the end of each Bezier curve. */
	public float[] getLengths () {
		return lengths;
	}

	public void setLengths (float[] lengths) {
		this.lengths = lengths;
	}

	/** The color of the path as it was in Spine. Available only when nonessential data was exported. Paths are not usually
	 * rendered at runtime. */
	public Color getColor () {
		return color;
	}
}
