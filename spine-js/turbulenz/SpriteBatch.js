/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

function SpriteBatch (draw2D) {
	this.draw2D = draw2D;
	this.buffer = [];
}
SpriteBatch.prototype = {
	count: 0,
	texture: null,
	blendMode: null,
	begin: function (blendMode, sortMode) {
		this.blendMode = blendMode;
		this.draw2D.begin(blendMode, sortMode);
	},
	add: function (texture, x1, y1, x2, y2, x3, y3, x4, y4, r, g, b, a, u1, v1, u2, v2) {
		if (this.texture && this.texture != texture) this.flush();
		this.texture = texture;
		var index = this.count++ * 16;
		var buffer = this.buffer;
		buffer[index++] = x1;
		buffer[index++] = y1;
		buffer[index++] = x2;
		buffer[index++] = y2;
		buffer[index++] = x3;
		buffer[index++] = y3;
		buffer[index++] = x4;
		buffer[index++] = y4;
		buffer[index++] = r;
		buffer[index++] = g;
		buffer[index++] = b;
		buffer[index++] = a;
		buffer[index++] = u1;
		buffer[index++] = v1;
		buffer[index++] = u2;
		buffer[index] = v2;
	},
	flush: function () {
		if (!this.texture) return;
		this.draw2D.drawRaw(this.texture, this.buffer, this.count, 0);
		this.texture = null;
		this.count = 0;
	},
	end: function () {
		this.flush();
		this.draw2D.end();
	}
};
