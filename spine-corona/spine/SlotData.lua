
local SlotData = {}
function SlotData.new (name, boneData)
	if not name then error("name cannot be nil", 2) end
	if not boneData then error("boneData cannot be nil", 2) end
	
	local self = {
		name = name,
		boneData = boneData
	}

	function self:setColor (r, g, b, a)
		self.r = r
		self.g = g
		self.b = b
		self.a = a
	end

	self:setColor(255, 255, 255, 255)
	
	return self;
end
return SlotData
