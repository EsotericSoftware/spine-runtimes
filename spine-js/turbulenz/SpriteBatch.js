
function SpriteBatch (draw2D) {
	this.draw2D = draw2D;
	this.buffer = [];
}
SpriteBatch.prototype = {
	count: 0,
	texture: null,
	begin: function (blendMode, sortMode) {
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
