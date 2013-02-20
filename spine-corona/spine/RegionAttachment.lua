
local RegionAttachment = {}
function RegionAttachment.new (name)
	if not name then error("name cannot be nil", 2) end
	
	local self = {
		name = name
	}
	
	return self
end
return RegionAttachment
