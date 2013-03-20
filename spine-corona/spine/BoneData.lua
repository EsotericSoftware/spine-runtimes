
local BoneData = {}
function BoneData.new (name, parent)
	if not name then error("name cannot be nil", 2) end

	local self = {
		name = name,
		parent = parent
	}

	return self
end
return BoneData
