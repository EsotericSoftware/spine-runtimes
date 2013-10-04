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
