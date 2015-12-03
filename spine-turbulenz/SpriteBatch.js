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
