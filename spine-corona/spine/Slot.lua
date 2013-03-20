
local utils = require "spine.utils"

local Slot = {}
function Slot.new (slotData, skeleton, bone)
	if not slotData then error("slotData cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end
	if not bone then error("bone cannot be nil", 2) end

	local self = {
		data = slotData,
		skeleton = skeleton,
		bone = bone
	}

	function self:setColor (r, g, b, a)
		self.r = r
		self.g = g
		self.b = b
		self.a = a
	end

	function self:setAttachment (attachment)
		if self.attachment and self.attachment ~= attachment and self.skeleton.images[self.attachment] then
			self.skeleton.images[self.attachment]:removeSelf()
			self.skeleton.images[self.attachment] = nil
		end
		self.attachment = attachment
		self.attachmentTime = self.skeleton.time
	end

	function self:setAttachmentTime (time)
		self.attachmentTime = self.skeleton.time - time
	end

	function self:getAttachmentTime ()
		return self.skeleton.time - self.attachmentTime
	end

	function self:setToBindPose ()
		local data = self.data

		self:setColor(data.r, data.g, data.b, data.a)

		local attachment
		if data.attachmentName then attachment = self.skeleton:getAttachment(data.name, data.attachmentName) end
		self:setAttachment(attachment)
	end

	self:setToBindPose()

	return self
end
return Slot
