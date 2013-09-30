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
 -- 1. A Spine Single User License or Spine Professional License must be
 --    purchased from Esoteric Software and the license must remain valid:
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

if (system.getInfo("environment") == "simulator") then
	package.path = package.path .. ";".. system.pathForFile("../") .. "/?.lua"
end

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
spine.AttachmentLoader = require "spine-lua.AttachmentLoader"
spine.Animation = require "spine-lua.Animation"
spine.AnimationStateData = require "spine-lua.AnimationStateData"
spine.AnimationState = require "spine-lua.AnimationState"

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
	-- Skeleton extends a group.
	local self = spine.Skeleton.new_super(skeletonData)
	self.group = group or display.newGroup()

	-- createImage can customize where images are found.
	function self:createImage (attachment)
		return display.newImage(attachment.name .. ".png")
	end

	-- updateWorldTransform positions images.
	local updateWorldTransform_super = self.updateWorldTransform
	function self:updateWorldTransform ()
		updateWorldTransform_super(self)

		if not self.images then self.images = {} end
		local images = self.images

		for i,slot in ipairs(self.drawOrder) do
			local attachment = slot.attachment
			local image = images[slot]
			if not attachment then
				-- Attachment is gone, remove the image.
				if image and image ~= spine.Skeleton.failed then
					image:removeSelf()
					images[slot] = nil
				end
			else
				-- Attachment image has changed.
				if image and image.attachment ~= attachment and image ~= spine.Skeleton.failed then
					image:removeSelf()
					image = nil
				end
				-- Create new image.
				if not image then
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
					print(slot.data.additiveBlending)
					if slot.data.additiveBlending then
						image.blendMode = "add"
					end
					images[slot] = image
				end
				-- Position image based on attachment and bone.
				if image ~= spine.Skeleton.failed then
					image.x = slot.bone.worldX + attachment.x * slot.bone.m00 + attachment.y * slot.bone.m01
					image.y = -(slot.bone.worldY + attachment.x * slot.bone.m10 + attachment.y * slot.bone.m11)
					image.rotation = -(slot.bone.worldRotation + attachment.rotation)

					-- fix scaling when attachment is rotated 90 degrees
					local rotation = math.abs(attachment.rotation) % 180
					if (rotation == 90) then
					    image.xScale = slot.bone.worldScaleY * attachment.scaleX
					    image.yScale = slot.bone.worldScaleX * attachment.scaleY
					else
					    if (rotation ~= 0 and (slot.bone.worldScaleX ~= 1 or slot.bone.worldScaleY ~= 1)) then
							print("WARNING: Non-uniform bone scaling with attachments not rotated to\n"
								.."         cardinal angles will not work as expected with Corona.\n"
								.."         Bone: "..slot.bone.data.name..", slot: "..slot.data.name..", attachment: "..attachment.name)
						end
					    image.xScale = slot.bone.worldScaleX * attachment.scaleX
					    image.yScale = slot.bone.worldScaleY * attachment.scaleY
					end

					if self.flipX then
						image.xScale = -image.xScale
						image.rotation = -image.rotation
					end
					if self.flipY then
						image.yScale = -image.yScale
						image.rotation = -image.rotation
					end
					image:setFillColor(self.r * slot.r, self.g * slot.g, self.b * slot.b, self.a * slot.a)
					self.group:insert(image)
				end
			end
		end

		if self.debug then
			for i,bone in ipairs(self.bones) do
				if not bone.line then bone.line = display.newLine(0, 0, bone.data.length, 0) end
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
				bone.line:setColor(255, 0, 0)
				self.group:insert(bone.line)

				if not bone.circle then bone.circle = display.newCircle(0, 0, 3) end
				bone.circle.x = bone.worldX
				bone.circle.y = -bone.worldY
				bone.circle:setFillColor(0, 255, 0)
				self.group:insert(bone.circle)
			end
		end
	end
	
	return self
end

return spine
