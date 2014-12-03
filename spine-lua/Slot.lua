-------------------------------------------------------------------------------
-- Spine Runtimes Software License
-- Version 2.1
-- 
-- Copyright (c) 2013, Esoteric Software
-- All rights reserved.
-- 
-- You are granted a perpetual, non-exclusive, non-sublicensable and
-- non-transferable license to install, execute and perform the Spine Runtimes
-- Software (the "Software") solely for internal use. Without the written
-- permission of Esoteric Software (typically granted by licensing Spine), you
-- may not (a) modify, translate, adapt or otherwise create derivative works,
-- improvements of the Software or develop new applications using the Software
-- or (b) remove, delete, alter or obscure any trademarks or any copyright,
-- trademark, patent or other intellectual property or proprietary rights
-- notices on or in the Software, including any copy thereof. Redistributions
-- in binary or source form must include this license and terms.
-- 
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
-- OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
-- WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
-- OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
-- ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local Slot = {}
function Slot.new (slotData, bone)
	if not slotData then error("slotData cannot be nil", 2) end
	if not bone then error("bone cannot be nil", 2) end

	local self = {
		data = slotData,
		bone = bone,
		r = 1, g = 1, b = 1, a = 1,
		attachment = nil,
		attachmentTime = 0,
		attachmentVertices = nil,
		attachmentVerticesCount = 0
	}

	function self:setColor (r, g, b, a)
		self.r = r
		self.g = g
		self.b = b
		self.a = a
	end

	function self:setAttachment (attachment)
		self.attachment = attachment
		self.attachmentTime = self.bone.skeleton.time
		self.attachmentVerticesCount = 0
	end

	function self:setAttachmentTime (time)
		self.attachmentTime = self.bone.skeleton.time - time
	end

	function self:getAttachmentTime ()
		return self.bone.skeleton.time - self.attachmentTime
	end

	function self:setToSetupPose ()
		local data = self.data

		self:setColor(data.r, data.g, data.b, data.a)

		local attachment
		if data.attachmentName then 
			attachment = self.bone.skeleton:getAttachment(data.name, data.attachmentName)
		end
		self:setAttachment(attachment)
	end

	self:setToSetupPose()

	return self
end
return Slot
