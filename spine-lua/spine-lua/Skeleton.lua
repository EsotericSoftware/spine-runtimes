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

local utils = require "spine-lua.utils"
local Bone = require "spine-lua.Bone"
local Slot = require "spine-lua.Slot"
local IkConstraint = require "spine-lua.IkConstraint"
local PathConstraint = require "spine-lua.PathConstraint"
local TransformConstraint = require "spine-lua.TransformConstraint"
local AttachmentLoader = require "spine-lua.AttachmentLoader"
local AttachmentType = require "spine-lua.attachments.AttachmentType"
local Color = require "spine-lua.Color"

local setmetatable = setmetatable
local ipairs = ipairs
local table_insert = table.insert
local math_min = math.min
local math_max = math.max

local Skeleton = {}
Skeleton.__index = Skeleton

function Skeleton.new (data)
	if not data then error("data cannot be nil", 2) end

	local self = {
		data = data,
		bones = {},
		slots = {},
		slotsByName = {},
		drawOrder = {},
		ikConstraints = {},
		transformConstraints = {},
		pathConstraints = {},
		_updateCache = {},
		updateCacheReset = {},
		skin = nil,
		color = Color.newWith(1, 1, 1, 1),
		time = 0,
		scaleX = 1, scaleY = 1,
		x = 0, y = 0
	}
	setmetatable(self, Skeleton)

	for _,boneData in ipairs(data.bones) do
		local bone = nil
		if boneData.parent == nil then
			bone = Bone.new(boneData, self, nil)
		else
			local parent = self.bones[boneData.parent.index]
			bone = Bone.new(boneData, self, parent)
			table_insert(parent.children, bone)
		end
		table_insert(self.bones, bone)
	end

	for _,slotData in ipairs(data.slots) do
		local bone = self.bones[slotData.boneData.index]
		local slot = Slot.new(slotData, bone)
		table_insert(self.slots, slot)
		self.slotsByName[slot.data.name] = slot
		table_insert(self.drawOrder, slot)
	end

	for _, ikConstraintData in ipairs(data.ikConstraints) do
		table_insert(self.ikConstraints, IkConstraint.new(ikConstraintData, self))
	end

	for _, transformConstraintData in ipairs(data.transformConstraints) do
		table_insert(self.transformConstraints, TransformConstraint.new(transformConstraintData, self))
	end

	for _, pathConstraintData in ipairs(data.pathConstraints) do
		table_insert(self.pathConstraints, PathConstraint.new(pathConstraintData, self))
	end

	self:updateCache()

	return self
end


-- Caches information about bones and IK constraints. Must be called if bones or IK constraints are added or removed.
function Skeleton:updateCache ()
	local updateCache = {}
	self._updateCache = updateCache
	self.updateCacheReset = {}

	local bones = self.bones
	for _, bone in ipairs(bones) do
		bone.sorted = bone.data.skinRequired
		bone.active = not bone.sorted
	end

	if self.skin then
		local skinBones = self.skin.bones
		for i, boneData in ipairs(skinBones) do
			local bone = bones[boneData.index]
			while bone do
				bone.sorted = false
				bone.active = true
				bone = bone.parent
			end
		end
	end

	local ikConstraints = self.ikConstraints
	local transformConstraints = self.transformConstraints
	local pathConstraints = self.pathConstraints
	local ikCount = #ikConstraints
	local transformCount = #transformConstraints
	local pathCount = #pathConstraints
	local constraintCount = ikCount + transformCount + pathCount

	local i = 0
	while i < constraintCount do
		local found = false
		local ii = 1
		while ii <= ikCount do
			local constraint = ikConstraints[ii]
			if constraint.data.order == i then
				self:sortIkConstraint(constraint)
				found = true
				break
			end
			ii = ii + 1
		end

		if not found then
			ii = 1
			while ii <= transformCount do
				local constraint = transformConstraints[ii]
				if constraint.data.order == i then
					self:sortTransformConstraint(constraint)
					found = true
					break
				end
				ii = ii + 1
			end
		end

		if not found then
			ii = 1
			while ii <= pathCount do
				local constraint = pathConstraints[ii]
				if constraint.data.order == i then
					self:sortPathConstraint(constraint)
					break
				end
				ii = ii + 1
			end
		end

		i = i + 1
	end

	for _, bone in ipairs(self.bones) do
		self:sortBone(bone)
	end
end

function Skeleton:sortIkConstraint (constraint)
	constraint.active = constraint.target.active and ((not constraint.data.skinRequired) or (self.skin and utils.arrayContains(self.skin.constraints, constraint.data)))
	if not constraint.active then return end

	local target = constraint.target
	self:sortBone(target)

	local constrained = constraint.bones
	local parent = constrained[1]
	self:sortBone(parent)

	if #constrained > 1 then
		local child = constrained[#constrained]
		local contains = false
		for _, updatable in ipairs(self._updateCache) do
			if updatable == child then
				contains = true
				break
			end
		end
		if not contains then table_insert(self.updateCacheReset, child) end
	end

	table_insert(self._updateCache, constraint)

	self:sortReset(parent.children)
	constrained[#constrained].sorted = true
end

function Skeleton:sortPathConstraint(constraint)
	constraint.active = constraint.target.bone.active and ((not constraint.data.skinRequired) or (self.skin and utils.arrayContains(self.skin.constraints, constraint.data)))
	if not constraint.active then return end

	local slot = constraint.target
	local slotIndex = slot.data.index
	local slotBone = slot.bone
	if self.skin then self:sortPathConstraintAttachment(self.skin, slotIndex, slotBone) end
	if self.data.defaultSkin and not (self.data.defaultSkin == self.skin) then
		self:sortPathConstraintAttachment(self.data.defaultSkin, slotIndex, slotBone)
	end
	for _,skin in ipairs(self.data.skins) do
		self:sortPathConstraintAttachment(skin, slotIndex, slotBone)
	end

	local attachment = slot.attachment
	if attachment and attachment.type == AttachmentType.path then self:sortPathConstraintAttachmentWith(attachment, slotBone) end

	local constrained = constraint.bones
	for _,bone in ipairs(constrained) do
		self:sortBone(bone)
	end

	table_insert(self._updateCache, constraint)

	for _,bone in ipairs(constrained) do
		self:sortReset(bone.children)
	end

	for _,bone in ipairs(constrained) do
		bone.sorted = true
	end
end

function Skeleton:sortTransformConstraint(constraint)
	constraint.active = constraint.target.active and ((not constraint.data.skinRequired) or (self.skin and utils.arrayContains(self.skin.constraints, constraint.data)))
	if not constraint.active then return end

	self:sortBone(constraint.target)

	local constrained = constraint.bones
	if constraint.data.local_ then
		for _,bone in ipairs(constrained) do
			local child = constrained[#constrained]
			local contains = false
			self:sortBone(child.parent)
			for _,updatable in ipairs(self._updateCache) do
				if updatable == child then
					contains = true
					break
				end
			end
			if not contains then table_insert(self.updateCacheReset, child) end
		end
	else
		for _,bone in ipairs(constrained) do
			self:sortBone(bone)
		end
	end

	table_insert(self._updateCache, constraint)

	for _,bone in ipairs(constrained) do
		self:sortReset(bone.children)
	end

	for _,bone in ipairs(constrained) do
		bone.sorted = true
	end
end

function Skeleton:sortPathConstraintAttachment(skin, slotIndex, slotBone)
	local attachments = skin.attachments[slotIndex]
	if not attachments then return end
	for _,attachment in pairs(attachments) do
		self:sortPathConstraintAttachmentWith(attachment, slotBone)
	end
end

function Skeleton:sortPathConstraintAttachmentWith(attachment, slotBone)
	if attachment.type ~= AttachmentType.path then return end
	local pathBones = attachment.bones
	if not pathBones then
		self:sortBone(slotBone)
	else
		local bones = self.bones
		local i = 0
		local n = #pathBones
		while i < n do
			local boneCount = pathBones[i + 1]
			i = i + 1
			local nn = i + boneCount
			while i < nn do
				self:sortBone(bones[pathBones[i + 1]])
				i = i + 1
			end
		end
	end
end

function Skeleton:sortBone(bone)
	if bone.sorted then return end
	local parent = bone.parent
	if parent then self:sortBone(parent) end
	bone.sorted = true
	table_insert(self._updateCache, bone)
end

function Skeleton:sortReset(bones)
	for _, bone in ipairs(bones) do
		if bone.active then
			if bone.sorted then self:sortReset(bone.children) end
			bone.sorted = false
		end
	end
end

-- Updates the world transform for each bone and applies IK constraints.
function Skeleton:updateWorldTransform ()
	local updateCacheReset = self.updateCacheReset
	for _,bone in ipairs(updateCacheReset) do
		bone.ax = bone.x
		bone.ay = bone.y
		bone.arotation = bone.rotation
		bone.ascaleX = bone.scaleX
		bone.ascaleY = bone.scaleY
		bone.ashearX = bone.shearX
		bone.ashearY = bone.shearY
		bone.appliedValid = true
	end

	local updateCache = self._updateCache
	for _, updatable in ipairs(updateCache) do
		updatable:update()
	end
end

function Skeleton:setToSetupPose ()
	self:setBonesToSetupPose()
	self:setSlotsToSetupPose()
end

function Skeleton:setBonesToSetupPose ()
	for _,bone in ipairs(self.bones) do
		bone:setToSetupPose()
	end

	for _,ikConstraint in ipairs(self.ikConstraints) do
		ikConstraint.mix = ikConstraint.data.mix
		ikConstraint.softness = ikConstraint.data.softness
		ikConstraint.bendDirection = ikConstraint.data.bendDirection
		ikConstraint.compress = ikConstraint.data.compress
		ikConstraint.stretch = ikConstraint.data.stretch
	end

	local transformConstraints = self.transformConstraints
	for _, constraint in ipairs(transformConstraints) do
		local data = constraint.data
		constraint.rotateMix = data.rotateMix
		constraint.translateMix = data.translateMix
		constraint.scaleMix = data.scaleMix
		constraint.shearMix = data.shearMix
	end

	local pathConstraints = self.pathConstraints
	for _, constraint in ipairs(pathConstraints) do
		local data = constraint.data
		constraint.position = data.position
		constraint.spacing = data.spacing
		constraint.rotateMix = data.rotateMix
		constraint.translateMix = data.translateMix
	end
end

function Skeleton:setSlotsToSetupPose ()
	for i,slot in ipairs(self.slots) do
		self.drawOrder[i] = slot
		slot:setToSetupPose()
	end
end

function Skeleton:getRootBone ()
	return self.bones[1]
end

function Skeleton:findBone (boneName)
	if not boneName then error("boneName cannot be nil.", 2) end
	for _,bone in ipairs(self.bones) do
		if bone.data.name == boneName then return bone end
	end
	return nil
end

function Skeleton:findBoneIndex(boneName)
	if not boneName then error("boneName cannot be nil.", 2) end
	for i,bone in ipairs(self.bones) do
		if bone.data.name == boneName then return i end
	end
	return -1
end

function Skeleton:findSlot (slotName)
	if not slotName then error("slotName cannot be nil.", 2) end
	return self.slotsByName[slotName]
end

function Skeleton:findSlotIndex(slotName)
	if not slotName then error("slotName cannot be nil.", 2) end
	for i, slot in ipairs(self.slots) do
		if slot.data.name == slotName then return i end
	end
	return -1
end

-- Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default skin}.
-- Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was
-- no old skin, each slot's setup mode attachment is attached from the new skin.
function Skeleton:setSkin (skinName)
	local skin = self.data:findSkin(skinName)
	if not skin then error("Skin not found: " .. skinName, 2) end
	self:setSkinByReference(skin)
end

function Skeleton:setSkinByReference(newSkin)
	if (self.skin == newSkin) then return end
	if newSkin then
		if self.skin then
			newSkin:attachAll(self, self.skin)
		else
			local slots = self.slots
			for i, slot in ipairs(slots) do
				local name = slot.data.attachmentName
				if name then
					local attachment = newSkin:getAttachment(i, name)
					if attachment then
						slot:setAttachment(attachment)
					end
				end
			end
		end
	end
	self.skin = newSkin
	self:updateCache()
end

function Skeleton:getAttachment (slotName, attachmentName)
	return self:getAttachmentByIndex(self.data.slotNameIndices[slotName], attachmentName)
end

function Skeleton:getAttachmentByIndex (slotIndex, attachmentName)
	if self.skin then
		local attachment = self.skin:getAttachment(slotIndex, attachmentName)
		if attachment then return attachment end
	end
	if self.data.defaultSkin then
		return self.data.defaultSkin:getAttachment(slotIndex, attachmentName)
	end
	return nil
end

function Skeleton:setAttachment (slotName, attachmentName)
	if not slotName then error("slotName cannot be nil.", 2) end
	for i,slot in ipairs(self.slots) do
		if slot.data.name == slotName then
			local attachment = nil
			if attachmentName then
				attachment = self:getAttachmentByIndex(i, attachmentName)
				if not attachment then error("Attachment not found: " .. attachmentName .. ", for slot: " .. slotName, 2) end
			end
			slot:setAttachment(attachment)
			return
		end
	end
	error("Slot not found: " .. slotName, 2)
end

function Skeleton:findIkConstraint(constraintName)
	if not constraintName then error("constraintName cannot be null.", 2) end
	local ikConstraints = self.ikConstraints
	for _, ikConstraint in ipairs(ikConstraints) do
		if ikConstraint.data.name == constraintName then return ikConstraint end
	end
	return nil
end

function Skeleton:findTransformConstraint(constraintName)
	if not constraintName then error("constraintName cannot be null.", 2) end
	local transformConstraints = self.transformConstraints
	for _, transformConstraint in ipairs(transformConstraints) do
		if transformConstraint.data.name == constraintName then return transformConstraint end
	end
	return nil
end

function Skeleton:findPathConstraint(constraintName)
	if not constraintName then error("constraintName cannot be null.", 2) end
	local pathConstraints = self.pathConstraints
	for _, pathConstraint in ipairs(pathConstraints) do
		if pathConstraint.data.name == constraintName then return pathConstraint end
	end
	return nil
end

function Skeleton:getBounds(offset, size)
	if not offset then error("offset cannot be null.", 2) end
	if not size then error("size cannot be null.", 2) end
	local drawOrder = self.drawOrder;
	local minX = 99999999
	local minY = 99999999
	local maxX = -99999999
	local maxY = -99999999
	for _, slot in ipairs(drawOrder) do
		if slot.bone.active then
			local vertices = {}
			local attachment = slot.attachment
			if attachment then
				if attachment.type == AttachmentType.region then
					attachment:computeWorldVertices(slot.bone, vertices, 0, 2)
				elseif attachment.type == AttachmentType.mesh then
					attachment:computeWorldVertices(slot, 0, attachment.worldVerticesLength, vertices, 0, 2)
				end
			end
			if #vertices > 0 then
				local nn = #vertices
				local ii = 1
				while ii <= nn do
					local x = vertices[ii]
					local y = vertices[ii + 1]
					minX = math_min(minX, x)
					minY = math_min(minY, y)
					maxX = math_max(maxX, x)
					maxY = math_max(maxY, y)
					ii = ii + 2
				end
			end
		end
	end
	offset[1] = minX
	offset[2] = minY
	size[1] = maxX - minX
	size[2] = maxY - minY
end

function Skeleton:update (delta)
	self.time = self.time + delta
end

function Skeleton:setColor (r, g, b, a)
	self.color.r = r
	self.color.g = g
	self.color.b = b
	self.color.a = a
end

return Skeleton
