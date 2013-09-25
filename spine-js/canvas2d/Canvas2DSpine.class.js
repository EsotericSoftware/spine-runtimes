/*******************************************************************************
* Helper class to render Spine animation on a canvas 2d context without external
* dependencies
*
* Author : Alaa-eddine KADDOURI
* http://ezelia.com/en/
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
* 1. Redistributions of source code must retain the above copyright notice, this
* list of conditions and the following disclaimer.
* 2. Redistributions in binary form must reproduce the above copyright notice,
* this list of conditions and the following disclaimer in the documentation
* and/or other materials provided with the distribution.
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
******************************************************************************/
/*
TODO : 
 - handle texture scaling
 - benchmark setTransform() vs canvas scale + rotate and use setTransform if better


*/

var Canvas2DSpine = function(basePath, x, y)
{
    this.basePath = basePath;
	this.skeletonData = undefined;
	this.lastTime = Date.now();
	this.state = undefined;
	this.skeleton = undefined;
	this.vertices = [];
	this.ready = false;
	
	this.x = x;
	this.y = y;
}

Canvas2DSpine.prototype.load = function (skeletonName, animation, skin, mix) {
	var _this = this;	
	this.ready = false;
	var loadAtlas = function(atlasText) {
		var textureCount = 0;
		atlas = new spine.Atlas(atlasText, {
			load: function (page, path) {
				textureCount++;
				
				page.img = new Image();
				page.img.onload = function() {
					page.width = page.img.width;
					page.height = page.img.height;
					atlas.updateUVs(page);
					textureCount--;			
				}
				page.img.src = _this.basePath + path;
			},
			unload: function (texture) {
				texture.destroy();
			}
		});
		function waitForTextures() {
			if (!textureCount)
			    Ajax.get(_this.basePath + skeletonName + ".json", function (skeletonText) {
					var json = new spine.SkeletonJson(new spine.AtlasAttachmentLoader(atlas));
					_this.skeletonData = json.readSkeletonData(JSON.parse(skeletonText));
					
					spine.Bone.yDown = true;

					_this.skeleton = new spine.Skeleton(_this.skeletonData);
					_this.skeleton.getRootBone().x = _this.x;
					_this.skeleton.getRootBone().y = _this.y;
					_this.skeleton.updateWorldTransform();

					var stateData = new spine.AnimationStateData(_this.skeletonData);	
					_this.state = new spine.AnimationState(stateData);
					if (mix)
					{
						for (var i = 0; i<mix.length; i++) 
							stateData.setMixByName.apply(stateData, mix[i]);
						
						
					}
					
					if (skin)
					{
						_this.skeleton.setSkinByName(skin);
						_this.skeleton.setSlotsToSetupPose();					
					}
					_this.state.setAnimationByName(animation, true);
					
					_this.ready = true;
					
				});
			else
				setTimeout(waitForTextures, 100);
		}
		waitForTextures();
	}
	
	
	Ajax.get(this.basePath + skeletonName + ".atlas", loadAtlas);
}

Canvas2DSpine.prototype.update = function () {
	if (!this.ready) return;
	var dt = (Date.now() - this.lastTime)/1000;
	this.lastTime = Date.now();
	
	
	this.state.update(dt);
	this.state.apply(this.skeleton);
	this.skeleton.updateWorldTransform();
	
	
}
Canvas2DSpine.prototype.setAnimation = function (animation, repeat) {
	this.state.setAnimationByName(animation, repeat);
	
}



Canvas2DSpine.prototype.draw = function (context) {
	context.clearRect(0, 0, canvas.width, canvas.height);
	var skeleton = this.skeleton;
	var drawOrder = skeleton.drawOrder;
	for (var i = 0, n = drawOrder.length; i < n; i++) {
		var slot = drawOrder[i];
		var attachment = slot.attachment;
		if (!(attachment instanceof spine.RegionAttachment)) continue;
		attachment.computeVertices(skeleton.x, skeleton.y, slot.bone, this.vertices);
		
		var x = this.vertices[2];
		var y = this.vertices[3];
		
		var w = attachment.rendererObject.width;
		var h = attachment.rendererObject.height;
		var px = attachment.rendererObject.x;
		var py = attachment.rendererObject.y;
		
		var scaleX = attachment.scaleX;
		var scaleY = attachment.scaleY;
		var angle = -(slot.bone.worldRotation + attachment.rotation) * Math.PI/180;
            if(skeleton.flipX) {
                
				scaleX *= -1;
                angle *= -1;
            }

            if(skeleton.flipY) {
                
				scaleY *= -1;
                angle *= -1;
            }		
		
		context.save();
		context.translate(x, y);
		context.rotate(angle);
		context.scale(scaleX, scaleY);

		context.drawImage(attachment.rendererObject.page.img, px, py, w, h, 0, 0, w, h);
		context.restore();

	}
	
	
}