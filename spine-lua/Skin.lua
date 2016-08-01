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

local Skin = {}
function Skin.new (name)
	if not name then error("name cannot be nil", 2) end
	
	local self = {
		name = name,
		attachments = {}
	}

	function self:addAttachment (slotIndex, name, attachment)
		if not name then error("name cannot be nil.", 2) end
		self.attachments[slotIndex .. ":" .. name] = { slotIndex, name, attachment }
	end

	function self:getAttachment (slotIndex, name)
		if not name then error("name cannot be nil.", 2) end
		local values = self.attachments[slotIndex .. ":" .. name]
		if not values then return nil end
		return values[3]
	end

	function self:findNamesForSlot (slotIndex)
		local names = {}
		for k,v in self.attachments do
			if v[1] == slotIndex then table.insert(names, v[2]) end
		end
	end

	function self:findAttachmentsForSlot (slotIndex)
		local attachments = {}
		for k,v in self.attachments do
			if v[1] == slotIndex then table.insert(attachments, v[3]) end
		end
	end

	return self
end
return Skin
