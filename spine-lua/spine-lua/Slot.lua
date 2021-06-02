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
		darkColor = nil,
		attachment = nil,
		attachmentTime = 0,
		attachmentState = 0,
		deform = {}
	}

	setmetatable(self, Slot)

	if data.darkColor then self.darkColor = Color.newWith(1, 1, 1, 1) end

	self:setToSetupPose()

	return self
end

function Slot:setAttachment (attachment)
	if self.attachment == attachment then return end
	self.attachment = attachment
	self.attachmentTime = self.bone.skeleton.time
	self.deform = {}
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
	if self.darkColor then self.darkColor:setFrom(data.darkColor) end

	local attachment = nil
	if data.attachmentName then
		attachment = self.bone.skeleton:getAttachmentByIndex(data.index, data.attachmentName)
	end
	self:setAttachment(attachment)
end

return Slot
