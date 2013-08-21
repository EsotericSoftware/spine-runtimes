 -------------------------------------------------------------------------------
 -- Copyright (c) 2013, Esoteric Software
 -- All rights reserved.
 -- 
 -- Redistribution and use in source and binary forms, with or without
 -- modification, are permitted provided that the following conditions are met:
 -- 
 -- 1. Redistributions of source code must retain the above copyright notice, this
 --    list of conditions and the following disclaimer.
 -- 2. Redistributions in binary form must reproduce the above copyright notice,
 --    this list of conditions and the following disclaimer in the documentation
 --    and/or other materials provided with the distribution.
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

local SkeletonData = {}
function SkeletonData.new ()
	local self = {
		bones = {},
		slots = {},
    slotNameIndices = {},
		skins = {},
		animations = {}
	}

	function self:findBone (boneName)
		if not boneName then error("boneName cannot be nil.", 2) end
		for i,bone in ipairs(self.bones) do
			if bone.name == boneName then return bone end
		end
		return nil
	end

	function self:findBoneIndex (boneName)
		if not boneName then error("boneName cannot be nil.", 2) end
		for i,bone in ipairs(self.bones) do
			if bone.name == boneName then return i end
		end
		return -1
	end

	function self:findSlot (slotName)
		if not slotName then error("slotName cannot be nil.", 2) end
		for i,slot in ipairs(self.slots) do
			if slot.name == slotName then return slot end
		end
		return nil
	end

	function self:findSlotIndex (slotName)
		if not slotName then error("slotName cannot be nil.", 2) end
		return slotNameIndices[slotName] or -1
	end

	function self:findSkin (skinName)
		if not skinName then error("skinName cannot be nil.", 2) end
		for i,skin in ipairs(self.skins) do
			if skin.name == skinName then return skin end
		end
		return nil
	end

	function self:findAnimation (animationName)
		if not animationName then error("animationName cannot be nil.", 2) end
		for i,animation in ipairs(self.animations) do
			if animation.name == animationName then return animation end
		end
		return nil
	end

	return self
end
return SkeletonData
