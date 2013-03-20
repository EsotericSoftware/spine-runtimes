
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
