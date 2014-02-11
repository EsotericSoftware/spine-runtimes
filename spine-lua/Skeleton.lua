-------------------------------------------------------------------------------
-- Spine Runtimes Software License
-- Version 2
-- 
-- Copyright (c) 2013, Esoteric Software
-- All rights reserved.
-- 
-- You are granted a perpetual, non-exclusive, non-sublicensable and
-- non-transferable license to install, execute and perform the Spine Runtimes
-- Software (the "Software") solely for internal use. Without the written
-- permission of Esoteric Software, you may not (a) modify, translate, adapt or
-- otherwise create derivative works, improvements of the Software or develop
-- new applications using the Software or (b) remove, delete, alter or obscure
-- any trademarks or any copyright, trademark, patent or other intellectual
-- property or proprietary rights notices on or in the Software, including
-- any copy thereof. Redistributions in binary or source form must include
-- this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
-- "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
-- TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
-- PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
-- DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
-- (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
-- LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
-- ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
-- (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
-- THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local Bone = require "spine-lua.Bone"
local Slot = require "spine-lua.Slot"
local AttachmentLoader = require "spine-lua.AttachmentLoader"

local Skeleton = {}
function Skeleton.new (skeletonData)
	if not skeletonData then error("skeletonData cannot be nil", 2) end

	local self = {
		data = skeletonData,
		bones = {},
		slots = {},
		slotsByName = {},
		drawOrder = {},
		r = 1, g = 1, b = 1, a = 1,
		x = 0, y = 0
	}

	function self:updateWorldTransform ()
		for i,bone in ipairs(self.bones) do
			bone:updateWorldTransform(self.flipX, self.flipY)
		end
	end

	function self:setToSetupPose ()
		self:setBonesToSetupPose()
		self:setSlotsToSetupPose()
	end

	function self:setBonesToSetupPose ()
		for i,bone in ipairs(self.bones) do
			bone:setToSetupPose()
		end
	end

	function self:setSlotsToSetupPose ()
		for i,slot in ipairs(self.slots) do
			slot:setToSetupPose()
		end
	end

	function self:getRootBone ()
		return self.bones[1]
	end

	function self:findBone (boneName)
		if not boneName then error("boneName cannot be nil.", 2) end
		for i,bone in ipairs(self.bones) do
			if bone.data.name == boneName then return bone end
		end
		return nil
	end

	function self:findSlot (slotName)
		if not slotName then error("slotName cannot be nil.", 2) end
		return self.slotsByName[slotName]
	end

	function self:setSkin (skinName)
		local newSkin
		if skinName then
			newSkin = self.data:findSkin(skinName)
			if not newSkin then error("Skin not found: " .. skinName, 2) end
			if self.skin then
				-- Attach all attachments from the new skin if the corresponding attachment from the old skin is currently attached.
				for k,v in pairs(self.skin.attachments) do
					local attachment = v[3]
					local slotIndex = v[1]
					local slot = self.slots[slotIndex]
					if slot.attachment == attachment then
						local name = v[2]
						local newAttachment = newSkin:getAttachment(slotIndex, name)
						if newAttachment then slot:setAttachment(newAttachment) end
					end
				end
			end
		end
		self.skin = newSkin
	end

	function self:getAttachment (slotName, attachmentName)
		if not slotName then error("slotName cannot be nil.", 2) end
		if not attachmentName then error("attachmentName cannot be nil.", 2) end
		local slotIndex = skeletonData.slotNameIndices[slotName]
		if slotIndex == -1 then error("Slot not found: " .. slotName, 2) end
		if self.skin then
			local attachment = self.skin:getAttachment(slotIndex, attachmentName)
			if attachment then return attachment end
		end
		if self.data.defaultSkin then
			return self.data.defaultSkin:getAttachment(slotIndex, attachmentName)
		end
		return nil
	end

	function self:setAttachment (slotName, attachmentName)
		if not slotName then error("slotName cannot be nil.", 2) end
		for i,slot in ipairs(self.slots) do
			if slot.data.name == slotName then
				if not attachmentName then 
					slot:setAttachment(nil)
				else
					slot:setAttachment(self:getAttachment(slotName, attachmentName))
				end
				return
			end
		end
		error("Slot not found: " .. slotName, 2)
	end

	function self:update (delta)
		self.time = self.time + delta
	end

	for i,boneData in ipairs(skeletonData.bones) do
		local parent
		if boneData.parent then parent = self.bones[spine.utils.indexOf(skeletonData.bones, boneData.parent)] end
		table.insert(self.bones, Bone.new(boneData, parent))
	end

	for i,slotData in ipairs(skeletonData.slots) do
		local bone = self.bones[spine.utils.indexOf(skeletonData.bones, slotData.boneData)]
		local slot = Slot.new(slotData, self, bone)
		table.insert(self.slots, slot)
		self.slotsByName[slot.data.name] = slot
		table.insert(self.drawOrder, slot)
	end

	return self
end
return Skeleton
