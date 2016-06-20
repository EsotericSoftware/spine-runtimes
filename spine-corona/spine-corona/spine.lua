-------------------------------------------------------------------------------
-- Spine Runtimes Software License
-- Version 2.3
-- 
-- Copyright (c) 2013-2015, Esoteric Software
-- All rights reserved.
-- 
-- You are granted a perpetual, non-exclusive, non-sublicensable and
-- non-transferable license to use, install, execute and perform the Spine
-- Runtimes Software (the "Software") and derivative works solely for personal
-- or internal use. Without the written permission of Esoteric Software (see
-- Section 2 of the Spine Software License Agreement), you may not (a) modify,
-- translate, adapt or otherwise create derivative works, improvements of the
-- Software or develop new applications using the Software or (b) remove,
-- delete, alter or obscure any trademarks or any copyright, trademark, patent
-- or other intellectual property or proprietary rights notices on or in the
-- Software, including any copy thereof. Redistributions in binary or source
-- form must include this license and terms.
-- 
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
-- OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
-- WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
-- OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
-- ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

spine = {}

spine.utils = require "spine-lua.utils"
spine.SkeletonJson = require "spine-lua.SkeletonJson"
spine.SkeletonData = require "spine-lua.SkeletonData"
spine.BoneData = require "spine-lua.BoneData"
spine.SlotData = require "spine-lua.SlotData"
spine.IkConstraintData = require "spine-lua.IkConstraintData"
spine.Skin = require "spine-lua.Skin"
spine.RegionAttachment = require "spine-lua.RegionAttachment"
spine.MeshAttachment = require "spine-lua.MeshAttachment"
spine.SkinnedMeshAttachment = require "spine-lua.SkinnedMeshAttachment"
spine.Skeleton = require "spine-lua.Skeleton"
spine.Bone = require "spine-lua.Bone"
spine.Slot = require "spine-lua.Slot"
spine.IkConstraint = require "spine-lua.IkConstraint"
spine.AttachmentType = require "spine-lua.AttachmentType"
spine.AttachmentLoader = require "spine-lua.AttachmentLoader"
spine.Animation = require "spine-lua.Animation"
spine.AnimationStateData = require "spine-lua.AnimationStateData"
spine.AnimationState = require "spine-lua.AnimationState"
spine.EventData = require "spine-lua.EventData"
spine.Event = require "spine-lua.Event"
spine.SkeletonBounds = require "spine-lua.SkeletonBounds"
spine.BlendMode = require "spine-lua.BlendMode"

spine.utils.readFile = function (fileName, base)
	if not base then base = system.ResourceDirectory end
	local path = system.pathForFile(fileName, base)
	local file = io.open(path, "r")
	if not file then return nil end
	local contents = file:read("*a")
	io.close(file)
	return contents
end
 
local json = require "json"
spine.utils.readJSON = function (text)
	return json.decode(text)
end
 
spine.Skeleton.failed = {} -- Placeholder for an image that failed to load.
 
spine.Skeleton.new_super = spine.Skeleton.new
function spine.Skeleton.new (skeletonData, group)
	local self = spine.Skeleton.new_super(skeletonData)
	self.group = group or display.newGroup()

	self.images = {}

	-- Customizes where images are found.
	function self:createImage (attachment)
		return display.newImage(attachment.name .. ".png")
	end

	-- Customizes where images are found.
	function self:createMesh (attachment, meshParameters)
		local mesh = display.newMesh(meshParameters)
		mesh.fill = {type="image", filename=attachment.name .. ".png"}
		return mesh
	end

	-- Customizes what happens when an image changes, return false to recreate the image.
	function self:modifyImage (attachment)
		return false
	end

	-- updateWorldTransform positions images.
	local updateWorldTransform_super = self.updateWorldTransform
	function self:updateWorldTransform ()
		updateWorldTransform_super(self)

		local images = self.images
		local skeletonR, skeletonG, skeletonB, skeletonA = self.r, self.g, self.b, self.a
		for i,slot in ipairs(self.drawOrder) do
			local image = images[slot]
			local attachment = slot.attachment
			if not attachment then -- Attachment is gone, remove the image.
				if image then
					display.remove(image)
					images[slot] = nil
				end
			elseif attachment.type == spine.AttachmentType.region then
				if image and image.attachment ~= attachment then -- Attachment image has changed.
					if self:modifyImage(image, attachment) then
						image.lastR, image.lastA = nil, nil
						image.attachment = attachment
					else -- If not modified, remove the image and it will be recreated.
						display.remove(image)
						images[slot] = nil
						image = nil
					end
				end
				if not image then -- Create new image.
					image = self:createImage(attachment)
					if image then
						image.attachment = attachment
						image.anchorX = 0.5
						image.anchorY = 0.5
						image.width = attachment.width
						image.height = attachment.height
					else
						print("Error creating image: " .. attachment.name)
						image = spine.Skeleton.failed
					end
					if slot.data.blendMode == spine.BlendMode.normal then
						image.blendMode = "normal"
					elseif slot.data.blendMode == spine.BlendMode.additive then
						image.blendMode = "add"
					elseif slot.data.blendMode == spine.BlendMode.multiply then
						image.blendMode = "multiply"
					elseif slot.data.blendMode == spine.BlendMode.screen then
						image.blendMode = "screen"
					end
					images[slot] = image
				end
				-- Position image based on attachment and bone.
				if image ~= spine.Skeleton.failed then
					local bone = slot.bone
					local flipX, flipY = ((bone.worldFlipX and -1) or 1), ((bone.worldFlipY and -1) or 1)

					local x = bone.worldX + attachment.x * bone.m00 + attachment.y * bone.m01
					local y = -(bone.worldY + attachment.x * bone.m10 + attachment.y * bone.m11)
					if not image.lastX then
						image.x, image.y = x, y
						image.lastX, image.lastY = x, y
					elseif image.lastX ~= x or image.lastY ~= y then
						image:translate(x - image.lastX, y - image.lastY)
						image.lastX, image.lastY = x, y
					end

					local xScale = attachment.scaleX * flipX
					local yScale = attachment.scaleY * flipY
					-- Fix scaling when attachment is rotated 90 or -90.
					local rotation = math.abs(attachment.rotation) % 180
					if (rotation == 90) then
						xScale = xScale * bone.worldScaleY
						yScale = yScale * bone.worldScaleX
					else
						xScale = xScale * bone.worldScaleX
						yScale = yScale * bone.worldScaleY
						if rotation ~= 0 and xScale ~= yScale and not image.rotationWarning then
							image.rotationWarning = true
							print("WARNING: Non-uniform bone scaling with attachments not rotated to\n"
								.."         cardinal angles will not work as expected with Corona.\n"
								.."         Bone: "..bone.data.name..", slot: "..slot.data.name..", attachment: "..attachment.name)
						end
					end
					if not image.lastScaleX then
						image.xScale, image.yScale = xScale, yScale
						image.lastScaleX, image.lastScaleY = xScale, yScale
					elseif image.lastScaleX ~= xScale or image.lastScaleY ~= yScale then
						image:scale(xScale / image.lastScaleX, yScale / image.lastScaleY)
						image.lastScaleX, image.lastScaleY = xScale, yScale
					end

					rotation = -(bone.worldRotation + attachment.rotation) * flipX * flipY
					if not image.lastRotation then
						image.rotation = rotation
						image.lastRotation = rotation
					elseif rotation ~= image.lastRotation then
						image:rotate(rotation - image.lastRotation)
						image.lastRotation = rotation
					end

					local r, g, b = skeletonR * slot.r, skeletonG * slot.g, skeletonB * slot.b
					if image.lastR ~= r or image.lastG ~= g or image.lastB ~= b or not image.lastR then
						image:setFillColor(r, g, b)
						image.lastR, image.lastG, image.lastB = r, g, b
					end
					local a = skeletonA * slot.a
					if a and (image.lastA ~= a or not image.lastA) then
						image.lastA = a
						image.alpha = image.lastA -- 0-1 range, unlike RGB.
					end
					
					self.group:insert(image)
				end
			elseif attachment.type == spine.AttachmentType.mesh or attachment.type == spine.AttachmentType.skinnedmesh then

				if image and image.attachment ~= attachment then -- Attachment image has changed.
					if self:modifyImage(image, attachment) then
						image.lastR, image.lastA = nil, nil
						image.attachment = attachment
					else -- If not modified, remove the image and it will be recreated.
						display.remove(image)
						images[slot] = nil
						image = nil
					end
				end

				local worldVertices = {}
				attachment:updateUVs()
				attachment:computeWorldVertices(0, 0, slot, worldVertices)

				for i = 2, #worldVertices, 2 do
					worldVertices[i] = -worldVertices[i]
				end

				if not image then
					local meshParameters = {
						mode = "indexed",
						vertices =  worldVertices,
						indices = attachment.triangles,
						uvs = attachment.uvs,
						zeroBasedIndices = true,
					}
					image = self:createMesh(attachment, meshParameters)
					if image then
						if slot.data.blendMode == spine.BlendMode.normal then
							image.blendMode = "normal"
						elseif slot.data.blendMode == spine.BlendMode.additive then
							image.blendMode = "add"
						elseif slot.data.blendMode == spine.BlendMode.multiply then
							image.blendMode = "multiply"
						elseif slot.data.blendMode == spine.BlendMode.screen then
							image.blendMode = "screen"
						end
						self.images[slot] = image
						image:translate( image.path:getVertexOffset() )
					end
				else
					for i = 1, #worldVertices, 2 do
						image.path:setVertex( 1+ 0.5*(i-1), worldVertices[i], worldVertices[i+1])
					end
				end

				if image then
					local r, g, b = skeletonR * slot.r, skeletonG * slot.g, skeletonB * slot.b
					if image.lastR ~= r or image.lastG ~= g or image.lastB ~= b or not image.lastR then
						image:setFillColor(r, g, b)
						image.lastR, image.lastG, image.lastB = r, g, b
					end
					local a = skeletonA * slot.a
					if a and (image.lastA ~= a or not image.lastA) then
						image.lastA = a
						image.alpha = image.lastA
					end
					self.group:insert(image)
				end

			end
		end

		if self.debug then
			for i,bone in ipairs(self.bones) do
				if not bone.line then
					bone.line = display.newLine(0, 0, bone.data.length, 0)
					bone.line:setStrokeColor(1, 0, 0)
				end
				bone.line.x = bone.worldX
				bone.line.y = -bone.worldY
				bone.line.rotation = -bone.worldRotation
				if bone.worldFlipX then
					bone.line.xScale = -1
					bone.line.rotation = -bone.line.rotation
				else
					bone.line.xScale = 1
				end
				if bone.worldFlipY then
					bone.line.yScale = -1
					bone.line.rotation = -bone.line.rotation
				else
					bone.line.yScale = 1
				end
				self.group:insert(bone.line)

				if not bone.circle then
					bone.circle = display.newCircle(0, 0, 3)
					bone.circle:setFillColor(0, 1, 0)
				end
				bone.circle.x = bone.worldX
				bone.circle.y = -bone.worldY
				self.group:insert(bone.circle)
			end
		end

		if self.debugAabb then
			if not self.bounds then
				self.bounds = spine.SkeletonBounds.new()
				self.boundsRect = display.newRect(self.group, 0, 0, 0, 0)
				self.boundsRect:setFillColor(0, 0, 0, 0)
				self.boundsRect.strokeWidth = 1
				self.boundsRect:setStrokeColor(0, 1, 0, 1)
			end
			self.bounds:update(self, true)
			local width = self.bounds:getWidth()
			local height = self.bounds:getHeight()
			self.boundsRect.x = self.bounds.minX + width / 2
			self.boundsRect.y = -self.bounds.minY - height / 2
			self.boundsRect.width = width
			self.boundsRect.height = height
			self.group:insert(self.boundsRect)
		end
	end
	return self
end

return spine
