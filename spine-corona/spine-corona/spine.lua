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
local spine = spine

--localizing modules
local spine_utils = require "spine.utils" --optimized
local spine_SkeletonJson = require "spine.SkeletonJson" --optimized
local spine_SkeletonData = require "spine.SkeletonData" --optimized
local spine_BoneData = require "spine.BoneData" --optimized
local spine_SlotData = require "spine.SlotData" --optimized
local spine_IkConstraintData = require "spine.IkConstraintData" --optimized
local spine_Skin = require "spine.Skin" --optimized
local spine_RegionAttachment = require "spine.RegionAttachment" --optimized
local spine_Skeleton = require "spine.Skeleton" --optimized
local spine_Bone = require "spine.Bone" --optimized
local spine_Slot = require "spine.Slot" --optimized
local spine_IkConstraint = require "spine.IkConstraint" --optimized
local spine_AttachmentType = require "spine.AttachmentType" --optimized
local spine_AttachmentLoader = require "spine.AttachmentLoader" --optimized
local spine_Animation = require "spine.Animation" --optimized (delete meshes)
local spine_AnimationStateData = require "spine.AnimationStateData" --optimized
local spine_AnimationState = require "spine.AnimationState" --optimized
local spine_EventData = require "spine.EventData" --optimized
local spine_Event = require "spine.Event" --optimized
local spine_SkeletonBounds = require "spine.SkeletonBounds" --optimized
local spine_BlendMode = require "spine.BlendMode" --optimized

local spine_BlendMode_normal = spine_BlendMode.normal
local spine_BlendMode_additive = spine_BlendMode.additive
local spine_BlendMode_multiply = spine_BlendMode.multiply
local spine_BlendMode_screen = spine_BlendMode.screen

spine.utils = spine_utils
spine.SkeletonJson = spine_SkeletonJson
spine.SkeletonData = spine_SkeletonData
spine.BoneData = spine_BoneData
spine.SlotData = spine_SlotData
spine.IkConstraintData = spine_IkConstraintData
spine.Skin = spine_Skin
spine.RegionAttachment = spine_RegionAttachment
spine.Skeleton = spine_Skeleton
spine.Bone = spine_Bone
spine.Slot = spine_Slot
spine.IkConstraint = spine_IkConstraint
spine.AttachmentType = spine_AttachmentType
spine.AttachmentLoader = spine_AttachmentLoader
spine.Animation = spine_Animation
spine.AnimationStateData = spine_AnimationStateData
spine.AnimationState = spine_AnimationState
spine.EventData = spine_EventData
spine.Event = spine_Event
spine.SkeletonBounds = spine_SkeletonBounds
spine.BlendMode = spine_BlendMode



local json = require "json"

--localizing functions
local system_ResourceDirectory = system.ResourceDirectory
local system_pathForFile = system.pathForFile
local io_open = io.open
local io_close = io.close
local json_decode = json.decode
local display_newGroup = display.newGroup
local display_newImage = display.newImage
local display_newLine = display.newLine
local display_newCircle = display.newCircle
local display_newRect = display.newRect
local display_remove = display.remove
local ipairs = ipairs
local math_abs = math.abs
 

spine_utils.readFile = function (fileName, base)
	if not base then base = system_ResourceDirectory end
	local path = system_pathForFile(fileName, base)
	local file = io_open(path, "r")
	if not file then return nil end
	local contents = file:read("*a")
	io_close(file)
	return contents
end
 
spine_utils.readJSON = function (text)
	return json_decode(text)
end

local spine_Skeleton_failed = {} -- Placeholder for an image that failed to load.
spine_Skeleton.failed = spine_Skeleton_failed

local spine_Skeleton_new_super = spine_Skeleton.new
spine_Skeleton.new_super = spine_Skeleton_new_super
function spine_Skeleton.new (skeletonData, group)
	local self = spine_Skeleton_new_super(skeletonData)
	self.group = group or display_newGroup()

	self.images = {}

	return self
end

-- Customizes where images are found.
function spine_Skeleton:createImage (attachment)
	return display_newImage(attachment.name .. ".png")
end

-- Customizes what happens when an image changes, return false to recreate the image.
function spine_Skeleton:modifyImage (attachment)
	return false
end

-- updateWorldTransform positions images.
local updateWorldTransform_super = spine_Skeleton.updateWorldTransform
function spine_Skeleton:updateWorldTransform ()
	updateWorldTransform_super(self)

	local images = self.images
	local group = self.group
	local skeletonR, skeletonG, skeletonB, skeletonA = self.r, self.g, self.b, self.a
	
	for i,slot in ipairs(self.drawOrder) do
		local image = images[slot]
		local attachment = slot.attachment
		if not attachment then -- Attachment is gone, remove the image.
			if image then
				display_remove(image)
				images[slot] = nil
			end
		elseif attachment.type == spine_AttachmentType.region then
			if image and image.attachment ~= attachment then -- Attachment image has changed.
				if self:modifyImage(image, attachment) then
					image.lastR, image.lastA = nil, nil
					image.attachment = attachment
				else -- If not modified, remove the image and it will be recreated.
					display_remove(image)
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
					image = spine_Skeleton.failed
				end
				local slot_data_blendMode = slot.data.blendMode
				if slot_data_blendMode == spine_BlendMode_normal then
					image.blendMode = "normal"
				elseif slot_data_blendMode == spine_BlendMode_additive then
					image.blendMode = "add"
				elseif slot_data_blendMode == spine_BlendMode_multiply then
					image.blendMode = "multiply"
				elseif slot_data_blendMode == spine_BlendMode_screen then
					image.blendMode = "screen"
				end
				images[slot] = image
			end
			-- Position image based on attachment and bone.
			if image ~= spine_Skeleton_failed then
				local bone = slot.bone
				local flipX, flipY = ((bone.worldFlipX and -1) or 1), ((bone.worldFlipY and -1) or 1)

				local x = bone.worldX + attachment.x * bone.m00 + attachment.y * bone.m01
				local y = -(bone.worldY + attachment.x * bone.m10 + attachment.y * bone.m11)
				local image_lastX = image.lastX
				local image_lastY = image.lastY

				if not image_lastX then
					image.x, image.y = x, y
					image.lastX, image.lastY = x, y
				elseif image_lastX ~= x or image_lastY ~= y then
					image:translate(x - image_lastX, y - image_lastY)
					image.lastX, image.lastY = x, y
				end

				local xScale = attachment.scaleX * flipX
				local yScale = attachment.scaleY * flipY
				-- Fix scaling when attachment is rotated 90 or -90.
				local rotation = math_abs(attachment.rotation) % 180
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
				local image_lastRotation = image.lastRotation
				if not image_lastRotation then
					image.rotation = rotation
					image.lastRotation = rotation
				elseif rotation ~= image_lastRotation then
					image:rotate(rotation - image_lastRotation)
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
					
				group:insert(image)
			end
		end
	end

	if self.debug then
		for i,bone in ipairs(self.bones) do
			local bone_line = bone.line
			local bone_worldX = bone.worldX
			local bone_worldY = bone.worldY
			if not bone_line then
				bone_line = display_newLine(0, 0, bone.data.length, 0)
				bone.line = bone_line
				bone_line:setStrokeColor(1, 0, 0)
			end
			bone_line.x = bone_worldX
			bone_line.y = -bone_worldY
			bone_line.rotation = -bone.worldRotation
			if bone.worldFlipX then
				bone_line.xScale = -1
				bone_line.rotation = -bone_line.rotation
			else
				bone_line.xScale = 1
			end
			if bone.worldFlipY then
				bone_line.yScale = -1
				bone_line.rotation = -bone_line.rotation
			else
				bone_line.yScale = 1
			end
			group:insert(bone_line)

			local bone_circle = bone.circle
			if not bone_circle then
				bone_circle = display_newCircle(0, 0, 3)
				bone.circle = bone_circle
				bone_circle:setFillColor(0, 1, 0)
			end
			bone_circle.x = bone_worldX
			bone_circle.y = -bone_worldY
			group:insert(bone_circle)
		end
	end

	if self.debugAabb then
		local bounds = self.bounds
		local boundsRect = self.boundsRect
		if not bounds then
			bounds = spine_SkeletonBounds.new()
			self.bounds = bounds

			boundsRect = display_newRect(group, 0, 0, 0, 0)
			self.boundsRect = boundsRect
			boundsRect:setFillColor(0, 0, 0, 0)
			boundsRect.strokeWidth = 1
			boundsRect:setStrokeColor(0, 1, 0, 1)
		end
		bounds:update(self, true)

		local width = bounds:getWidth()
		local height = bounds:getHeight()
		boundsRect.x = bounds.minX + width / 2
		boundsRect.y = -bounds.minY - height / 2
		boundsRect.width = width
		boundsRect.height = height
		group:insert(boundsRect)
	end
end

return spine
