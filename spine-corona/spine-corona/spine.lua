------------------------------------------------------------------------------
 -- Spine Runtime Software License - Version 1.0
 -- 
 -- Copyright (c) 2013, Esoteric Software
 -- All rights reserved.
 -- 
 -- Redistribution and use in source and binary forms in whole or in part, with
 -- or without modification, are permitted provided that the following conditions
 -- are met:
 -- 
 -- 1. A Spine Essential, Professional, Enterprise, or Education License must
 --    be purchased from Esoteric Software and the license must remain valid:
 --    http://esotericsoftware.com/
 -- 2. Redistributions of source code must retain this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer.
 -- 3. Redistributions in binary form must reproduce this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer, in the documentation and/or other materials provided with the
 --    distribution.
 -- 
 -- THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 -- ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 -- WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 -- DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 -- ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 -- (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 -- LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 -- ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 -- (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 -- SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ------------------------------------------------------------------------------

spine = {}
 
spine.utils = require "spine-lua.utils"
spine.SkeletonJson = require "spine-lua.SkeletonJson"
spine.SkeletonData = require "spine-lua.SkeletonData"
spine.BoneData = require "spine-lua.BoneData"
spine.SlotData = require "spine-lua.SlotData"
spine.Skin = require "spine-lua.Skin"
spine.RegionAttachment = require "spine-lua.RegionAttachment"
spine.Skeleton = require "spine-lua.Skeleton"
spine.Bone = require "spine-lua.Bone"
spine.Slot = require "spine-lua.Slot"
spine.AttachmentType = require "spine-lua.AttachmentType"
spine.AttachmentLoader = require "spine-lua.AttachmentLoader"
spine.Animation = require "spine-lua.Animation"
spine.AnimationStateData = require "spine-lua.AnimationStateData"
spine.AnimationState = require "spine-lua.AnimationState"
spine.EventData = require "spine-lua.EventData"
spine.Event = require "spine-lua.Event"
spine.SkeletonBounds = require "spine-lua.SkeletonBounds"

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

	-- Customizes what happens when an image changes, return false to recreate the image.
	function self:modifyImage (attachment)
		return false
	end

	-- updateWorldTransform positions images.
	local updateWorldTransform_super = self.updateWorldTransform
	function self:updateWorldTransform ()
		updateWorldTransform_super(self)

		local images = self.images
		local skeletonR, skeletonG, skeletonB, skeletonA = self.r * 255, self.g * 255, self.b * 255, self.a
		for i,slot in ipairs(self.drawOrder) do
			local image = images[slot]
			local attachment = slot.attachment
			if not attachment then -- Attachment is gone, remove the image.
				if image then
					image:removeSelf()
					images[slot] = nil
				end
			elseif attachment.type == spine.AttachmentType.region then
				if image and image.attachment ~= attachment then -- Attachment image has changed.
					if self:modifyImage(image, attachment) then
						image.lastR, image.lastA = nil, nil
						image.attachment = attachment
					else -- If not modified, remove the image and it will be recreated.
						image:removeSelf()
						images[slot] = nil
						image = nil
					end
				end
				if not image then -- Create new image.
					image = self:createImage(attachment)
					if image then
						image.attachment = attachment
						image:setReferencePoint(display.CenterReferencePoint)
						image.width = attachment.width
						image.height = attachment.height
					else
						print("Error creating image: " .. attachment.name)
						image = spine.Skeleton.failed
					end
					if slot.data.additiveBlending then image.blendMode = "add" end
					images[slot] = image
				end
				-- Position image based on attachment and bone.
				if image ~= spine.Skeleton.failed then
					local flipX, flipY = ((self.flipX and -1) or 1), ((self.flipY and -1) or 1)

					local x = slot.bone.worldX + attachment.x * slot.bone.m00 + attachment.y * slot.bone.m01
					local y = -(slot.bone.worldY + attachment.x * slot.bone.m10 + attachment.y * slot.bone.m11)
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
						xScale = xScale * slot.bone.worldScaleY
						yScale = yScale * slot.bone.worldScaleX
					else
						xScale = xScale * slot.bone.worldScaleX
						yScale = yScale * slot.bone.worldScaleY
						if rotation ~= 0 and xScale ~= yScale and not image.rotationWarning then
							image.rotationWarning = true
							print("WARNING: Non-uniform bone scaling with attachments not rotated to\n"
								.."         cardinal angles will not work as expected with Corona.\n"
								.."         Bone: "..slot.bone.data.name..", slot: "..slot.data.name..", attachment: "..attachment.name)
						end
					end
					if not image.lastScaleX then
						image.xScale, image.yScale = xScale, yScale
						image.lastScaleX, image.lastScaleY = xScale, yScale
					elseif image.lastScaleX ~= xScale or image.lastScaleY ~= yScale then
						image:scale(xScale / image.lastScaleX, yScale / image.lastScaleY)
						image.lastScaleX, image.lastScaleY = xScale, yScale
					end

					rotation = -(slot.bone.worldRotation + attachment.rotation) * flipX * flipY
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
					bone.line:setColor(255, 0, 0)
				end
				bone.line.x = bone.worldX
				bone.line.y = -bone.worldY
				bone.line.rotation = -bone.worldRotation
				if self.flipX then
					bone.line.xScale = -1
					bone.line.rotation = -bone.line.rotation
				else
					bone.line.xScale = 1
				end
				if self.flipY then
					bone.line.yScale = -1
					bone.line.rotation = -bone.line.rotation
				else
					bone.line.yScale = 1
				end
				self.group:insert(bone.line)

				if not bone.circle then
					bone.circle = display.newCircle(0, 0, 3)
					bone.circle:setFillColor(0, 255, 0)
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
				self.boundsRect:setStrokeColor(0, 255, 0, 255)
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
