/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

package com.esotericsoftware.spine;

import static com.badlogic.gdx.graphics.GL20.*;

import com.badlogic.gdx.graphics.g2d.Batch;

/** Determines how images are blended with existing pixels when drawn. */
public enum BlendMode {
	normal(GL_SRC_ALPHA, GL_ONE, GL_ONE_MINUS_SRC_ALPHA, GL_ONE), //
	additive(GL_SRC_ALPHA, GL_ONE, GL_ONE, GL_ONE), //
	multiply(GL_DST_COLOR, GL_DST_COLOR, GL_ONE_MINUS_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA), //
	screen(GL_ONE, GL_ONE, GL_ONE_MINUS_SRC_COLOR, GL_ONE_MINUS_SRC_COLOR);

	public final int source, sourcePMA, destColor, sourceAlpha;

	BlendMode (int source, int sourcePMA, int destColor, int sourceAlpha) {
		this.source = source;
		this.sourcePMA = sourcePMA;
		this.destColor = destColor;
		this.sourceAlpha = sourceAlpha;
	}

	public void apply (Batch batch, boolean premultipliedAlpha) {
		batch.setBlendFunctionSeparate(premultipliedAlpha ? sourcePMA : source, destColor, sourceAlpha, destColor);
	}

	static public final BlendMode[] values = values();
}
