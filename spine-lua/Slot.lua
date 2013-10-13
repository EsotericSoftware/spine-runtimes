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

local Slot = {}
function Slot.new (slotData, skeleton, bone)
	if not slotData then error("slotData cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end
	if not bone then error("bone cannot be nil", 2) end

	local self = {
		data = slotData,
		skeleton = skeleton,
		bone = bone,
		r = 1, g = 1, b = 1, a = 1
	}

	function self:setColor (r, g, b, a)
		self.r = r
		self.g = g
		self.b = b
		self.a = a
	end

	function self:setAttachment (attachment)
		self.attachment = attachment
		self.attachmentTime = self.skeleton.time
	end

	function self:setAttachmentTime (time)
		self.attachmentTime = self.skeleton.time - time
	end

	function self:getAttachmentTime ()
		return self.skeleton.time - self.attachmentTime
	end

	function self:setToSetupPose ()
		local data = self.data

		self:setColor(data.r, data.g, data.b, data.a)

		local attachment
		if data.attachmentName then 
			attachment = self.skeleton:getAttachment(data.name, data.attachmentName)
		end
		self:setAttachment(attachment)
	end

	self:setToSetupPose()

	return self
end
return Slot
