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

local SkeletonData = {}
function SkeletonData.new ()
	local self = {
		version = 0, hash = 0,
		width = 0, height = 0,
		bones = {},
		slots = {},
		slotNameIndices = {},
		skins = {},
		events = {},
		animations = {},
		ikConstraints = {},
		defaultSkin = nil
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
		return self.slotNameIndices[slotName] or -1
	end

	function self:findSkin (skinName)
		if not skinName then error("skinName cannot be nil.", 2) end
		for i,skin in ipairs(self.skins) do
			if skin.name == skinName then return skin end
		end
		return nil
	end

	function self:findEvent (eventName)
		if not eventName then error("eventName cannot be nil.", 2) end
		for i,event in ipairs(self.events) do
			if event.name == eventName then return event end
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

	function self:findIkConstraint (ikConstraintName)
		if not ikConstraintName then error("ikConstraintName cannot be nil.", 2) end
		for i,ikConstraint in ipairs(self.ikConstraints) do
			if ikConstraint.name == ikConstraintName then return ikConstraint end
		end
		return nil
	end

	return self
end
return SkeletonData
