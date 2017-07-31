-------------------------------------------------------------------------------
-- Spine Runtimes Software License v2.5
--
-- Copyright (c) 2013-2016, Esoteric Software
-- All rights reserved.
--
-- You are granted a perpetual, non-exclusive, non-sublicensable, and
-- non-transferable license to use, install, execute, and perform the Spine
-- Runtimes software and derivative works solely for personal or internal
-- use. Without the written permission of Esoteric Software (see Section 2 of
-- the Spine Software License Agreement), you may not (a) modify, translate,
-- adapt, or develop new applications using the Spine Runtimes or otherwise
-- create derivative works or improvements of the Spine Runtimes or (b) remove,
-- delete, alter, or obscure any trademarks or any copyright, trademark, patent,
-- or other intellectual property or proprietary rights notices on or in the
-- Software, including any copy thereof. Redistributions in binary or source
-- form must include this license and terms.
--
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
-- USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
-- IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
-- ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
-- POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local setmetatable = setmetatable
local table_insert = table.insert

local Skin = {}
Skin.__index = Skin

function Skin.new (name)
	if not name then error("name cannot be nil", 2) end

	local self = {
		name = name,
		attachments = {}
	}
	setmetatable(self, Skin)

	return self
end

function Skin:addAttachment (slotIndex, name, attachment)
	if not name then error("name cannot be nil.", 2) end
	if not self.attachments[slotIndex] then self.attachments[slotIndex] = {} end
	self.attachments[slotIndex][name] = attachment
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
							print("Set attachment " .. attachment.name .. " on slot " .. slot.data.name)
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
	for k,v in self.attachments do
		if v[1] == slotIndex then table_insert(names, v[2]) end
	end
end

function Skin:findAttachmentsForSlot (slotIndex)
	local attachments = {}
	for k,v in self.attachments do
		if v[1] == slotIndex then table_insert(attachments, v[3]) end
	end
end

return Skin
