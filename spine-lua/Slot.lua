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

local Color = require "spine-lua.Color"

local Slot = {}
Slot.__index = Slot

function Slot.new (data, bone)
	if not data then error("slotData cannot be nil", 2) end
	if not bone then error("bone cannot be nil", 2) end

	local self = {
		data = data,
		bone = bone,
		color = Color.newWith(1, 1, 1, 1),
		attachment = nil,
		attachmentTime = 0,
		attachmentVertices = {},
		attachmentVerticesCount = 0
	}
	setmetatable(self, Slot)

	self:setToSetupPose()

	return self
end

function Slot:setAttachment (attachment)
	if self.attachment == attachment then return end
	self.attachment = attachment
	self.attachmentTime = self.bone.skeleton.time
	self.attachmentVerticesCount = 0
end

function Slot:setAttachmentTime (time)
	self.attachmentTime = self.bone.skeleton.time - time
end

function Slot:getAttachmentTime ()
	return self.bone.skeleton.time - self.attachmentTime
end

function Slot:setToSetupPose ()
	local data = self.data

	self.color:setFrom(data.color)

	local attachment = nil
	if data.attachmentName then
		attachment = self.bone.skeleton:getAttachmentByIndex(data.index, data.attachmentName)
	end
	self:setAttachment(attachment)
end

return Slot
