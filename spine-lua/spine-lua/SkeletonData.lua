-------------------------------------------------------------------------------
-- Spine Runtimes License Agreement
-- Last updated January 1, 2020. Replaces all prior versions.
--
-- Copyright (c) 2013-2020, Esoteric Software LLC
--
-- Integration of the Spine Runtimes into software or otherwise creating
-- derivative works of the Spine Runtimes is permitted under the terms and
-- conditions of Section 2 of the Spine Editor License Agreement:
-- http://esotericsoftware.com/spine-editor-license
--
-- Otherwise, it is permitted to integrate the Spine Runtimes into software
-- or otherwise create derivative works of the Spine Runtimes (collectively,
-- "Products"), provided that each user of the Products must obtain their own
-- Spine Editor license and redistribution of the Products in any form must
-- include this license and copyright notice.
--
-- THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
-- EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
-- WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
-- DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
-- DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
-- (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
-- BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
-- ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
-- (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
-- THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local setmetatable = setmetatable

local SkeletonData = {}
SkeletonData.__index = SkeletonData

function SkeletonData.new ()
	local self = {
		name,
		bones = {},
		slots = {},
		skins = {},
		defaultSkin = nil,
		events = {},
		animations = {},
		ikConstraints = {},
		transformConstraints = {},
		pathConstraints = {},
		x, y, width, height,
		version, hash, imagesPath,
		slotNameIndices = {}
	}
	setmetatable(self, SkeletonData)

	return self
end

function SkeletonData:findBone (boneName)
	if not boneName then error("boneName cannot be nil.", 2) end
	for _,bone in ipairs(self.bones) do
		if bone.name == boneName then return bone end
	end
	return nil
end

function SkeletonData:findBoneIndex (boneName)
	if not boneName then error("boneName cannot be nil.", 2) end
	for i,bone in ipairs(self.bones) do
		if bone.name == boneName then return i end
	end
	return -1
end

function SkeletonData:findSlot (slotName)
	if not slotName then error("slotName cannot be nil.", 2) end
	for i,slot in ipairs(self.slots) do
		if slot.name == slotName then return slot end
	end
	return nil
end

function SkeletonData:findSlotIndex (slotName)
	if not slotName then error("slotName cannot be nil.", 2) end
	return self.slotNameIndices[slotName] or -1
end

function SkeletonData:findSkin (skinName)
	if not skinName then error("skinName cannot be nil.", 2) end
	for _,skin in ipairs(self.skins) do
		if skin.name == skinName then return skin end
	end
	return nil
end

function SkeletonData:findEvent (eventName)
	if not eventName then error("eventName cannot be nil.", 2) end
	for _,event in ipairs(self.events) do
		if event.name == eventName then return event end
	end
	return nil
end

function SkeletonData:findAnimation (animationName)
	if not animationName then error("animationName cannot be nil.", 2) end
	for _,animation in ipairs(self.animations) do
		if animation.name == animationName then return animation end
	end
	return nil
end

function SkeletonData:findIkConstraint (constraintName)
	if not constraintName then error("constraintName cannot be nil.", 2) end
	for _,constraint in ipairs(self.ikConstraints) do
		if constraint.name == constraintName then return constraint end
	end
	return nil
end

function SkeletonData:findTransformConstraint (constraintName)
	if not constraintName then error("constraintName cannot be nil.", 2) end
	for _,constraint in ipairs(self.transformConstraints) do
		if constraint.name == constraintName then return constraint end
	end
	return nil
end

function SkeletonData:findPathConstraint (constraintName)
	if not constraintName then error("constraintName cannot be nil.", 2) end
	for _,constraint in ipairs(self.pathConstraints) do
		if constraint.name == constraintName then return constraint end
	end
	return nil
end

function SkeletonData:findPathConstraintIndex (constraintName)
	if not constraintName then error("constraintName cannot be nil.", 2) end
	for i,constraint in ipairs(self.pathConstraints) do
		if constraint.name == constraintName then return i end
	end
	return -1
end

return SkeletonData
