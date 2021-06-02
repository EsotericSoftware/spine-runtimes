-------------------------------------------------------------------------------
-- Spine Runtimes License Agreement
-- Last updated January 1, 2020. Replaces all prior versions.
--
-- Copyright (c) 2013-2020, Esoteric Software LLC
--
-- Integration of the Spine Runtimes into software or otherwise creating
-- derivative works of the Spine Runtimes is permitted under the terms and
-- conditions of Section 2 of the Spine Editor License Agreement:
-- http://esotericsoftware.com/spine-editor-license
--
-- Otherwise, it is permitted to integrate the Spine Runtimes into software
-- or otherwise create derivative works of the Spine Runtimes (collectively,
-- "Products"), provided that each user of the Products must obtain their own
-- Spine Editor license and redistribution of the Products in any form must
-- include this license and copyright notice.
--
-- THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
-- EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
-- WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
-- DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
-- DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
-- (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
-- BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
-- ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
-- (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
-- THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local setmetatable = setmetatable
local table_insert = table.insert
local AttachmentType = require "spine-lua.attachments.AttachmentType"

local SkinEntry = {}
SkinEntry.__index = SkinEntry

function SkinEntry.new (slotIndex, name, attachment)
	local self = {
		slotIndex = slotIndex,
		name = name,
		attachment = attachment
	}
	setmetatable(self, SkinEntry)

	return self
end

local Skin = {}
Skin.__index = Skin

function Skin.new (name)
	if not name then error("name cannot be nil", 2) end

	local self = {
		name = name,
		attachments = {},
		bones = {},
		constraints = {}
	}
	setmetatable(self, Skin)

	return self
end

function Skin:setAttachment (slotIndex, name, attachment)
	if not name then error("name cannot be nil.", 2) end
	if not self.attachments[slotIndex] then self.attachments[slotIndex] = {} end
	self.attachments[slotIndex][name] = attachment
end

function Skin:addSkin (skin)
	for i, bone in ipairs(skin.bones) do
		local contained = false
		for j, otherBone in ipairs(self.bones) do
			if otherBone == bone then
			contained = true
			break
			end
		end
		if not contained then table_insert(self.bones, bone) end
	end

	for i, constraint in ipairs(skin.constraints) do
		local contained = false
		for j, otherConstraint in ipairs(self.constraints) do
			if otherConstraint == constraint then
				contained = true
				break
			end
		end
		if not contained then table_insert(self.constraints, constraint) end
	end

	local attachments = skin:getAttachments()
	for i, entry in ipairs(attachments) do
		self:setAttachment(entry.slotIndex, entry.name, entry.attachment)
	end
end

function Skin:copySkin (skin)
	for i, bone in ipairs(skin.bones) do
		local contained = false
		for j, otherBone in ipairs(self.bones) do
			if otherBone == bone then
			contained = true
			break
			end
		end
		if not contained then table_insert(self.bones, bone) end
	end

	for i, constraint in ipairs(skin.constraints) do
		local contained = false
		for j, otherConstraint in ipairs(self.constraints) do
			if otherConstraint == constraint then
			contained = true
			break
			end
		end
		if not contained then table_insert(self.constraints, constraint) end
	end

	local attachments = skin:getAttachments()
	for i, entry in ipairs(attachments) do
		if entry.attachment.type == AttachmentType.mesh then
			entry.attachment = entry.attachment:newLinkedMesh()
			self:setAttachment(entry.slotIndex, entry.name, entry.attachment)
		else
			entry.attachment = entry.attachment:copy()
			self:setAttachment(entry.slotIndex, entry.name, entry.attachment)
		end
	end
end

function Skin:getAttachment (slotIndex, name)
	if not name then error("name cannot be nil.", 2) end
	local dictionary = self.attachments[slotIndex]
	if dictionary then
		return dictionary[name]
	else
		return nil
	end
end

function Skin:removeAttachment (slotIndex, name)
	local slotAttachments = self.attachments[slotIndex]
	if slotAttachments then
		slotAttachments[name] = nil
	end
end

function Skin:getAttachments ()
	local entries = {}
	for slotIndex, slotAttachments in pairs(self.attachments) do
		if slotAttachments then
			for name, attachment in pairs(slotAttachments) do
				if attachment then
					table_insert(entries, SkinEntry.new(slotIndex, name, attachment))
				end
			end
		end
	end
	return entries
end

function Skin:getAttachmentsForSlot (slotIndex)
	local entries = {}
	local slotAttachments = self.attachments[slotIndex]
	if slotAttachments then
		for name, attachment in pairs(slotAttachments) do
			if attachment then
				table_insert(entries, SkinEntry.new(slotIndex, name, attachment))
			end
		end
	end
	return entries
end

function Skin:clear ()
	self.attachments = {}
	self.bones = {}
	self.constraints = {}
end

function Skin:attachAll(skeleton, oldSkin)
	for i, slot in ipairs(skeleton.slots) do
		local slotAttachment = slot.attachment
		if slotAttachment then
			local dictionary = oldSkin.attachments[i]
			if (dictionary) then
				for key, value in pairs(dictionary) do
					local skinAttachment = value
					if slotAttachment == skinAttachment then
						local attachment = self:getAttachment(i, key)
						if attachment then
							slot:setAttachment(attachment)
						end
						break
					end
				end
			end
		end
	end
end

function Skin:findNamesForSlot (slotIndex)
	local names = {}
	for _,v in self.attachments do
		if v[1] == slotIndex then table_insert(names, v[2]) end
	end
end

function Skin:findAttachmentsForSlot (slotIndex)
	local attachments = {}
	for _,v in self.attachments do
		if v[1] == slotIndex then table_insert(attachments, v[3]) end
	end
end

return Skin
