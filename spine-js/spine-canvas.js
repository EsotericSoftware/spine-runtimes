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

spine.SkeletonRenderer = function (imagesPath) {
	this.imagesPath = imagesPath;
	this.lastTime = Date.now();
};

spine.SkeletonRenderer.prototype = {
	skeletonData: null,
	state: null,
	scale: 1,
	skeleton: null,
	
	load: function(jsonText) {
		var imagesPath = this.imagesPath;
		var json = new spine.SkeletonJson({
			newRegionAttachment: function (skin, name, path) {
				var image = new Image();
				image.src = imagesPath + path + ".png";
				var attachment = new spine.RegionAttachment(name);
				attachment.rendererObject = image;
				return attachment;
			},
			newBoundingBoxAttachment: function (skin, name) {
				return new spine.BoundingBoxAttachment(name);
			}
		});
		json.scale = this.scale;
		this.skeletonData = json.readSkeletonData(JSON.parse(jsonText));
		spine.Bone.yDown = true;
		
		this.skeleton = new spine.Skeleton(this.skeletonData);

		var stateData = new spine.AnimationStateData(this.skeletonData);
		this.state = new spine.AnimationState(stateData);
	},

	update: function() {
		var now = Date.now();
		var delta = (now - this.lastTime) / 1000;
		this.lastTime = now;

		this.state.update(delta);
		this.state.apply(this.skeleton);
		this.skeleton.updateWorldTransform();
	},

	render: function(context) {
		var skeleton = this.skeleton, drawOrder = skeleton.drawOrder;
		context.translate(skeleton.x, skeleton.y);

		for (var i = 0, n = drawOrder.length; i < n; i++) {
			var slot = drawOrder[i];
			var attachment = slot.attachment;
			if (!(attachment instanceof spine.RegionAttachment)) continue;
			var bone = slot.bone;

			var x = bone.worldX + attachment.x * bone.m00 + attachment.y * bone.m01;
			var y = bone.worldY + attachment.x * bone.m10 + attachment.y * bone.m11;
			var rotation = -(bone.worldRotation + attachment.rotation) * Math.PI / 180;
			var w = attachment.width * bone.worldScaleX, h = attachment.height * bone.worldScaleY;
			context.translate(x, y);
			context.rotate(rotation);
			context.drawImage(attachment.rendererObject, -w / 2, -h / 2, w, h);
			context.rotate(-rotation);
			context.translate(-x, -y);
		}

		context.translate(-skeleton.x, -skeleton.y);
	},

	animate: function (id) {
		var canvas = document.getElementById(id);
		var context = canvas.getContext("2d");
		var requestAnimationFrame = window.requestAnimationFrame ||
			window.webkitRequestAnimationFrame ||
			window.mozRequestAnimationFrame ||
			function (callback) {
				window.setTimeout(callback, 1000 / 60);
			};
		var self = this;
		function renderFrame () {
			context.clearRect(0, 0, canvas.width, canvas.height);
			self.update();
			self.render(context);
			requestAnimationFrame(renderFrame);
		};
		renderFrame();
	}
};
