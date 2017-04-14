-------------------------------------------------------------------------------
-- Spine Runtimes Software License v2.5
--
-- Copyright (c) 2013-2016, Esoteric Software
-- All rights reserved.
--
-- You are granted a perpetual, non-exclusive, non-sublicensable, and
-- non-transferable license to use, install, execute, and perform the Spine
-- Runtimes software and derivative works solely for personal or internal
-- use. Without the written permission of Esoteric Software (see Section 2 of
-- the Spine Software License Agreement), you may not (a) modify, translate,
-- adapt, or develop new applications using the Spine Runtimes or otherwise
-- create derivative works or improvements of the Spine Runtimes or (b) remove,
-- delete, alter, or obscure any trademarks or any copyright, trademark, patent,
-- or other intellectual property or proprietary rights notices on or in the
-- Software, including any copy thereof. Redistributions in binary or source
-- form must include this license and terms.
--
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
-- USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
-- IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
-- ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
-- POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local SkeletonData = require "spine-lua.SkeletonData"
local BoneData = require "spine-lua.BoneData"
local SlotData = require "spine-lua.SlotData"
local Skin = require "spine-lua.Skin"
local AttachmentLoader = require "spine-lua.AttachmentLoader"
local Animation = require "spine-lua.Animation"
local IkConstraintData = require "spine-lua.IkConstraintData"
local PathConstraintData = require "spine-lua.PathConstraintData"
local TransformConstraintData = require "spine-lua.TransformConstraintData"
local EventData = require "spine-lua.EventData"
local Event = require "spine-lua.Event"
local AttachmentType = require "spine-lua.attachments.AttachmentType"
local BlendMode = require "spine-lua.BlendMode"
local TransformMode = require "spine-lua.TransformMode"
local utils = spine.utils

local AttachmentLoader_new = AttachmentLoader.new
local BoneData_new = BoneData.new
local SkeletonData_new = SkeletonData.new
local SlotData_new = SlotData.new
local IkConstraintData_new = IkConstraintData.new
local TransformConstraintData_new = TransformConstraintData.new
local PathConstraintData_new = PathConstraintData.new
local Skin_new = Skin.new
local Event_new = Event.new
local EventData_new = EventData.new
local Animation_new = Animation.new
local Animation_ColorTimeline_new = Animation.ColorTimeline.new
local Animation_AttachmentTimeline_new = Animation.AttachmentTimeline.new
local Animation_RotateTimeline_new = Animation.RotateTimeline.new
local Animation_ScaleTimeline_new = Animation.ScaleTimeline.new
local Animation_ShearTimeline_new = Animation.ShearTimeline.new
local Animation_TranslateTimeline_new = Animation.TranslateTimeline.new
local Animation_IkConstraintTimeline_new = Animation.IkConstraintTimeline.new
local Animation_TransformConstraintTimeline_new = Animation.TransformConstraintTimeline.new
local Animation_PathConstraintSpacingTimeline_new = Animation.PathConstraintSpacingTimeline.new
local Animation_PathConstraintPositionTimeline_new = Animation.PathConstraintPositionTimeline.new
local Animation_PathConstraintMixTimeline_new = Animation.PathConstraintMixTimeline.new
local Animation_DeformTimeline_new = Animation.DeformTimeline.new
local Animation_DrawOrderTimeline_new = Animation.DrawOrderTimeline.new
local Animation_EventTimeline_new = Animation.EventTimeline.new

local Animation_RotateTimeline_ENTRIES = Animation.RotateTimeline.ENTRIES
local Animation_IkConstraintTimeline_ENTRIES = Animation.IkConstraintTimeline.ENTRIES
local Animation_TransformConstraintTimeline_ENTRIES = Animation.TransformConstraintTimeline.ENTRIES

local PathConstraintData_PositionMode = PathConstraintData.PositionMode
local PathConstraintData_PositionMode_fixed = PathConstraintData_PositionMode.fixed
local PathConstraintData_SpacingMode = PathConstraintData.SpacingMode
local PathConstraintData_SpacingMode_length = PathConstraintData_SpacingMode.length
local PathConstraintData_SpacingMode_fixed = PathConstraintData_SpacingMode.fixed
local PathConstraintData_RotateMode = PathConstraintData.RotateMode

local utils_newNumberArray = utils.newNumberArray


local setmetatable = setmetatable
local tonumber = tonumber
local pairs = pairs
local type = type
local math_max = math.max
local math_floor = math.floor


local SkeletonJson = {}
SkeletonJson.__index = SkeletonJson


local getValue = function (map, name, default)
	local value = map[name]
	if value == nil then return default else return value end
end

function SkeletonJson.new (attachmentLoader)
	if not attachmentLoader then attachmentLoader = AttachmentLoader_new() end

	local self = {
		attachmentLoader = attachmentLoader,
		scale = 1,
		linkedMeshes = {}
	}
	setmetatable(self, SkeletonJson)

	return self
end

local readAttachment
local readAnimation
local readCurve
local getArray

function SkeletonJson:readSkeletonData (jsonText)
	local scale = self.scale
	local skeletonData = SkeletonData_new(self.attachmentLoader)
	local root = utils.readJSON(jsonText)
	if not root then error("Invalid JSON: " .. jsonText, 2) end

	-- Skeleton.
	local skeletonMap = root["skeleton"]
	if skeletonMap then
		skeletonData.hash = skeletonMap["hash"]
		skeletonData.version = skeletonMap["spine"]
		skeletonData.width = skeletonMap["width"]
		skeletonData.height = skeletonMap["height"]
		skeletonData.fps = skeletonMap["fps"]
		skeletonData.imagesPath = skeletonMap["images"]
	end

	-- Bones.
	local skeletonData_bones = skeletonData.bones
	local rootBones = root["bones"]
	for i=1, #rootBones do
		local boneMap = rootBones[i]
		local boneName = boneMap["name"]
		local parentName = boneMap["parent"]

		local parent
		if parentName then
			parent = skeletonData:findBone(parentName)
			if not parent then error("Parent bone not found: " .. parentName) end
		end
		local data = BoneData_new(i, boneName, parent)
		data.length = getValue(boneMap, "length", 0) * scale
		data.x = getValue(boneMap, "x", 0) * scale
		data.y = getValue(boneMap, "y", 0) * scale
		data.rotation = getValue(boneMap, "rotation", 0)
		data.scaleX = getValue(boneMap, "scaleX", 1)
		data.scaleY = getValue(boneMap, "scaleY", 1)
		data.shearX = getValue(boneMap, "shearX", 0)
		data.shearY = getValue(boneMap, "shearY", 0)
		data.transformMode = TransformMode[getValue(boneMap, "transform", "normal")]

		skeletonData_bones[#skeletonData_bones + 1] = data
	end


	-- Slots.
	local rootSlots = root["slots"]
	if rootSlots then
		local skeletonData_slots = skeletonData.slots
		local skeletonData_slotNameIndices = skeletonData.slotNameIndices

		for i=1, #rootSlots do
			local slotMap = rootSlots[i]

			local slotName = slotMap["name"]
			local boneName = slotMap["bone"]
			local boneData = skeletonData:findBone(boneName)
			if not boneData then error("Slot bone not found: " .. boneName) end
			local data = SlotData_new(i, slotName, boneData)

			local color = slotMap["color"]
			if color then
				data.color:set(tonumber(color:sub(1, 2), 16) / 255,
				               tonumber(color:sub(3, 4), 16) / 255,
				               tonumber(color:sub(5, 6), 16) / 255,
				               tonumber(color:sub(7, 8), 16) / 255)
			end

			data.attachmentName = getValue(slotMap, "attachment", nil)
			data.blendMode = BlendMode[getValue(slotMap, "blend", "normal")]

			skeletonData_slots[#skeletonData_slots + 1] = data
			skeletonData_slotNameIndices[data.name] = #skeletonData_slots
		end
	end

	-- IK constraints.
	local rootIk = root["ik"]
	if rootIk then
		local skeletonData_ikConstraints = skeletonData.ikConstraints
		for i=1, #rootIk do
			local constraintMap = rootIk[i]

			local data = IkConstraintData_new(constraintMap["name"])
			local data_bones = data.bones
			data.order = getValue(constraintMap, "order", 0)

			local constraintMapBones = constraintMap["bones"]
			for i=1, #constraintMapBones do
				local boneName = constraintMapBones[i]
				local bone = skeletonData:findBone(boneName)
				if not bone then error("IK bone not found: " .. boneName) end
				data_bones[#data_bones + 1] = bone
			end

			local targetName = constraintMap["target"]
			data.target = skeletonData:findBone(targetName)
			if not data.target then error("Target bone not found: " .. targetName) end

			if constraintMap["bendPositive"] == false then data.bendDirection = -1 else data.bendDirection = 1 end
			data.mix = getValue(constraintMap, "mix", 1)

			skeletonData_ikConstraints[#skeletonData_ikConstraints + 1] = data
		end
	end

	-- Transform constraints
	local rootTransform = root["transform"]
	if rootTransform then
		local skeletonData_transformConstraints = skeletonData.transformConstraints
		for i=1, #rootTransform do
			local constraintMap = rootTransform[i]
			local data = TransformConstraintData_new(constraintMap.name)
			local data_bones = data.bones
			data.order = getValue(constraintMap, "order", 0)

			local constraintMapBones = constraintMap.bones
			for i=1, #constraintMapBones do
				local boneName = constraintMapBones[i]
				local bone = skeletonData:findBone(boneName)
				if not bone then error("Transform constraint bone not found: " .. boneName, 2) end
				data_bones[#data_bones + 1] = bone
			end

			local targetName = constraintMap.target
			data.target = skeletonData:findBone(targetName)
			if not data.target then error("Transform constraint target bone not found: " .. boneName, 2) end

			data.offsetRotation = getValue(constraintMap, "rotation", 0)
			data.offsetX = getValue(constraintMap, "x", 0) * scale
			data.offsetY = getValue(constraintMap, "y", 0) * scale
			data.offsetScaleX = getValue(constraintMap, "scaleX", 0)
			data.offsetScaleY = getValue(constraintMap, "scaleY", 0)
			data.offsetShearY = getValue(constraintMap, "shearY", 0)

			data.rotateMix = getValue(constraintMap, "rotateMix", 1)
			data.translateMix = getValue(constraintMap, "translateMix", 1)
			data.scaleMix = getValue(constraintMap, "scaleMix", 1)
			data.shearMix = getValue(constraintMap, "shearMix", 1)

			skeletonData_transformConstraints[#skeletonData_transformConstraints + 1] = data
		end
	end

	-- Path constraints
	local rootPath = root["path"]
	if rootPath then
		local skeletonData_pathConstraints = skeletonData.pathConstraints
		for i=1, #rootPath do
			local constraintMap = rootPath[i]
			local data = PathConstraintData_new(constraintMap.name)
			local data_bones = data.bones
			data.order = getValue(constraintMap, "order", 0)
			
			local constraintMapBones = constraintMap.bones
			for i=1, #constraintMapBones do
				local boneName = constraintMapBones[i]
				local bone = skeletonData:findBone(boneName)
				if not bone then error("Path constraint bone not found: " .. boneName, 2) end
				data_bones[#data_bones + 1] = bone
			end

			local targetName = constraintMap.target;
			data.target = skeletonData:findSlot(targetName)
			if data.target == nil then error("Path target slot not found: " .. targetName, 2) end

			data.positionMode = PathConstraintData_PositionMode[getValue(constraintMap, "positionMode", "percent"):lower()]
			local spacingMode = PathConstraintData_SpacingMode[getValue(constraintMap, "spacingMode", "length"):lower()]
			data.spacingMode = spacingMode
			data.rotateMode = PathConstraintData_RotateMode[getValue(constraintMap, "rotateMode", "tangent"):lower()]
			data.offsetRotation = getValue(constraintMap, "rotation", 0)
			data.position = getValue(constraintMap, "position", 0)
			if data.positionMode == PathConstraintData_PositionMode_fixed then data.position = data.position * scale end
			data.spacing = getValue(constraintMap, "spacing", 0)
			if spacingMode == PathConstraintData_SpacingMode_length or spacingMode == PathConstraintData_SpacingMode_fixed then data.spacing = data.spacing * scale end
			data.rotateMix = getValue(constraintMap, "rotateMix", 1)
			data.translateMix = getValue(constraintMap, "translateMix", 1)

			skeletonData_pathConstraints[#skeletonData_pathConstraints + 1] = data
		end
	end

	-- Skins.
	local rootSkins = root["skins"]
	if rootSkins then
		for skinName,skinMap in pairs(rootSkins) do
			local skin = Skin_new(skinName)
			local skeletonData_slotNameIndices = skeletonData.slotNameIndices
			for slotName,slotMap in pairs(skinMap) do
				local slotIndex = skeletonData_slotNameIndices[slotName]
				for attachmentName,attachmentMap in pairs(slotMap) do
					local attachment = readAttachment(self, attachmentMap, skin, slotIndex, attachmentName)
					if attachment then
						skin:addAttachment(slotIndex, attachmentName, attachment)
					end
				end
			end
			local skeletonData_skins = skeletonData.skins
			skeletonData_skins[#skeletonData_skins + 1] = skin
			if skin.name == "default" then skeletonData.defaultSkin = skin end
		end
	end

	-- Linked meshes
	local linkedMeshes = self.linkedMeshes
	for i=1, #linkedMeshes do
		local linkedMesh = linkedMeshes[i]
		local skin = skeletonData.defaultSkin
		if linkedMesh.skin then skin = skeletonData.findSkin(linkedMesh.skin) end
		if not skin then error("Skin not found: " .. linkedMesh.skin) end
		local parent = skin:getAttachment(linkedMesh.slotIndex, linkedMesh.parent)
		if not parent then error("Parent mesh not found: " + linkedMesh.parent) end
		linkedMesh.mesh:setParentMesh(parent)
		linkedMesh.mesh:updateUVs()
	end
	self.linkedMeshes = {}

	-- Events.
	local rootEvents = root["events"]
	if rootEvents then
		local skeletonData_events = skeletonData.events
		for eventName,eventMap in pairs(rootEvents) do
			local data = EventData_new(eventName)
			data.intValue = getValue(eventMap, "int", 0)
			data.floatValue = getValue(eventMap, "float", 0)
			data.stringValue = getValue(eventMap, "string", "")
			skeletonData_events[#skeletonData_events + 1] = data
		end
	end

	-- Animations.
	local rootAnimations = root["animations"]
	if rootAnimations then
		for animationName,animationMap in pairs(rootAnimations) do
			readAnimation(self, animationMap, animationName, skeletonData)
		end
	end

	return skeletonData
end
local SkeletonJson_readSkeletonData = SkeletonJson.readSkeletonData


function SkeletonJson:readSkeletonDataFile (fileName, base)
	return SkeletonJson_readSkeletonData(self, utils.readFile(fileName, base))
end


local AttachmentType_region = AttachmentType.region
local AttachmentType_boundingbox = AttachmentType.boundingbox
local AttachmentType_mesh = AttachmentType.mesh
local AttachmentType_linkedmesh = AttachmentType.linkedmesh
local AttachmentType_path = AttachmentType.path
readAttachment = function (self, map, skin, slotIndex, name)
	local scale = self.scale
	local attachmentLoader = self.attachmentLoader
	name = getValue(map, "name", name)

	local regionType = AttachmentType[getValue(map, "type", "region")]
	local path = getValue(map, "path", name)

	if regionType == AttachmentType_region then
		local region = attachmentLoader:newRegionAttachment(skin, name, path)
		if not region then return nil end
		region.path = path
		region.x = getValue(map, "x", 0) * scale
		region.y = getValue(map, "y", 0) * scale
		region.scaleX = getValue(map, "scaleX", 1);
		region.scaleY = getValue(map, "scaleY", 1);
		region.rotation = getValue(map, "rotation", 0);
		region.width = map.width * scale;
		region.height = map.height * scale;

		local color = map["color"]
		if color then
			region.color:set(tonumber(color:sub(1, 2), 16) / 255,
			                 tonumber(color:sub(3, 4), 16) / 255,
			                 tonumber(color:sub(5, 6), 16) / 255,
			                 tonumber(color:sub(7, 8), 16) / 255)
		end
		
		region:updateOffset()
		return region

	elseif regionType == AttachmentType_boundingbox then
		local box = attachmentLoader:newBoundingBoxAttachment(skin, name)
		if not box then return nil end
		readVertices(self, map, box, map.vertexCount * 2)
		local color = map.color
		if color then
			box.color:set(tonumber(color:sub(1, 2), 16) / 255,
			              tonumber(color:sub(3, 4), 16) / 255,
			              tonumber(color:sub(5, 6), 16) / 255,
			              tonumber(color:sub(7, 8), 16) / 255)
		end
		return box

	elseif regionType == AttachmentType_mesh or regionType == AttachmentType_linkedmesh then
		local mesh = attachmentLoader:newMeshAttachment(skin, name, path)
		if not mesh then return null end
		mesh.path = path

		local color = map.color
		if color then
			mesh.color:set(tonumber(color:sub(1, 2), 16) / 255,
			               tonumber(color:sub(3, 4), 16) / 255,
			               tonumber(color:sub(5, 6), 16) / 255,
			               tonumber(color:sub(7, 8), 16) / 255)
		end

		local parent = map.parent
		if parent then
			mesh.inheritDeform = getValue(map, "deform", true)

			local linkedMeshes = self.linkedMeshes
			linkedMeshes[#linkedMeshes + 1] = {
					mesh = mesh,
					skin = getValue(map, "skin", nil),
					slotIndex = slotIndex,
					parent = parent
			}
			return mesh
		end

		local uvs = getArray(map, "uvs", 1)
		readVertices(self, map, mesh, #uvs)
		local mesh_triangles = getArray(map, "triangles", 1)
		mesh.triangles = mesh_triangles
		-- adjust triangle indices by 1, vertices are one-indexed
		for i=1, #mesh_triangles do
			mesh_triangles[i] = mesh_triangles[i] + 1
		end
		mesh.regionUVs = uvs
		mesh:updateUVs()

		mesh.hullLength = getValue(map, "hull", 0) * 2
		return mesh

	elseif regionType == AttachmentType_path then
		local path = self.attachmentLoader:newPathAttachment(skin, name)
		if not path then return nil end
		path.closed = getValue(map, "closed", false)
		path.constantSpeed = getValue(map, "constantSpeed", true)

		local vertexCount = map.vertexCount
		readVertices(self, map, path, vertexCount * 2)

		local lengths = utils_newNumberArray(vertexCount / 3, 0)
		local map_lengths = map.lengths
		for i=1, #map_lengths do
			lengths[i] = map_lengths[i] * scale
		end
		path.lengths = lengths

		local color = map.color
		if color then
			path.color:set(tonumber(color:sub(1, 2), 16) / 255,
			               tonumber(color:sub(3, 4), 16) / 255,
			               tonumber(color:sub(5, 6), 16) / 255,
			               tonumber(color:sub(7, 8), 16) / 255)
		end
		return path
	end

	error("Unknown attachment type: " .. regionType .. " (" .. name .. ")")
end


readVertices = function (self, map, attachment, verticesLength)
	local scale = self.scale
	attachment.worldVerticesLength = verticesLength
	local vertices = getArray(map, "vertices", 1)
	if verticesLength == #vertices then
		if scale ~= 1 then
			local i = 0
			local n = #vertices
			while i < n do
				vertices[i + 1] = vertices[i + 1] * scale
				i = i + 1
			end
		end
		attachment.vertices = vertices
		return
	end

	local weights = {}
	local bones = {}
	local i = 0
	local n = #vertices
	while i < n do
		local boneCount = vertices[i + 1]
		i = i + 1
		bones[#bones + 1] = boneCount
		local nn = i + boneCount * 4
		while i < nn do
			bones[#bones + 1] = vertices[i + 1] + 1 -- +1 because bones are one-indexed
			weights[#weights + 1] = vertices[i + 2] * scale
			weights[#weights + 1] = vertices[i + 3] * scale
			weights[#weights + 1] = vertices[i + 4]
			i = i + 4
		end
	end
	attachment.bones = bones
	attachment.vertices = weights
end


readAnimation = function (self, map, name, skeletonData)
	local timelines = {}
	local duration = 0
	local scale = self.scale

	-- Slot timelines
	local slotsMap = map["slots"]
	if slotsMap then
		local skeletonData_slotNameIndices = skeletonData.slotNameIndices
		for slotName,timelineMap in pairs(slotsMap) do
			local slotIndex = skeletonData_slotNameIndices[slotName]

			for timelineName,values in pairs(timelineMap) do
				if timelineName == "color" then
					local timeline = Animation_ColorTimeline_new(#values)
					timeline.slotIndex = slotIndex

					local frameIndex = 0
					for i=1, #values do
						local valueMap = values[i]
						local color = valueMap["color"]
						timeline:setFrame(
							frameIndex, valueMap["time"],
							tonumber(color:sub(1, 2), 16) / 255,
							tonumber(color:sub(3, 4), 16) / 255,
							tonumber(color:sub(5, 6), 16) / 255,
							tonumber(color:sub(7, 8), 16) / 255
						)
						readCurve(valueMap, timeline, frameIndex)
						frameIndex = frameIndex + 1
					end
					timelines[#timelines + 1] = timeline
					duration = math_max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.ColorTimeline.ENTRIES])

				elseif timelineName == "attachment" then
					local timeline = Animation_AttachmentTimeline_new(#values)
					timeline.slotIndex = slotIndex

					local frameIndex = 0
					for i=1, #values do
						local valueMap = values[i]
						local attachmentName = valueMap["name"]
						timeline:setFrame(frameIndex, valueMap["time"], attachmentName)
						frameIndex = frameIndex + 1
					end
					timelines[#timelines + 1] = timeline
					duration = math_max(duration, timeline.frames[timeline:getFrameCount() - 1])

				else
					error("Invalid frame type for a slot: " .. timelineName .. " (" .. slotName .. ")")
				end
			end
		end
	end

	-- Bone timelines
	local bonesMap = map["bones"]
	if bonesMap then
		for boneName,timelineMap in pairs(bonesMap) do
			local boneIndex = skeletonData:findBoneIndex(boneName)
			if boneIndex == -1 then error("Bone not found: " .. boneName) end

			for timelineName,values in pairs(timelineMap) do
				if timelineName == "rotate" then
					local timeline = Animation_RotateTimeline_new(#values)
					timeline.boneIndex = boneIndex

					local frameIndex = 0
					for i=1, #values do
						local valueMap = values[i]
						timeline:setFrame(frameIndex, valueMap["time"], valueMap["angle"])
						readCurve(valueMap, timeline, frameIndex)
						frameIndex = frameIndex + 1
					end
					timelines[#timelines + 1] = timeline
					duration = math_max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation_RotateTimeline_ENTRIES])

				elseif timelineName == "translate" or timelineName == "scale" or timelineName == "shear" then
					local timeline
					local timelineScale = 1
					if timelineName == "scale" then
						timeline = Animation_ScaleTimeline_new(#values)
					elseif timelineName == "shear" then
						timeline = Animation_ShearTimeline_new(#values)
					else
						timeline = Animation_TranslateTimeline_new(#values)
						timelineScale = self.scale
					end
					timeline.boneIndex = boneIndex

					local frameIndex = 0
					for i=1, #values do
						local valueMap = values[i]
						local x = (valueMap["x"] or 0) * timelineScale
						local y = (valueMap["y"] or 0) * timelineScale
						timeline:setFrame(frameIndex, valueMap["time"], x, y)
						readCurve(valueMap, timeline, frameIndex)
						frameIndex = frameIndex + 1
					end
					timelines[#timelines + 1] = timeline
					duration = math_max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.TranslateTimeline.ENTRIES])
				else
					error("Invalid timeline type for a bone: " .. timelineName .. " (" .. boneName .. ")")
				end
			end
		end
	end

	-- IK timelines.
	local ik = map["ik"]
	if ik then
		for ikConstraintName,values in pairs(ik) do
			local ikConstraint = skeletonData:findIkConstraint(ikConstraintName)
			local timeline = Animation_IkConstraintTimeline_new(#values)
			for i,other in pairs(skeletonData.ikConstraints) do
				if other == ikConstraint then
					timeline.ikConstraintIndex = i
					break
				end
			end
			local frameIndex = 0
			for i=1, #values do
				local valueMap = values[i]
				local mix = 1
				if valueMap["mix"] ~= nil then mix = valueMap["mix"] end
				local bendPositive = 1
				if valueMap["bendPositive"] == false then bendPositive = -1 end
				timeline:setFrame(frameIndex, valueMap["time"], mix, bendPositive)
				readCurve(valueMap, timeline, frameIndex)
				frameIndex = frameIndex + 1
			end
			timelines[#timelines + 1] = timeline
			duration = math_max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation_IkConstraintTimeline_ENTRIES])
		end
	end

	-- Transform constraint timelines.
	local transform = map["transform"]
	if transform then
		for constraintName, values in pairs(transform) do
			local constraint = skeletonData:findTransformConstraint(constraintName)
			local timeline = Animation_TransformConstraintTimeline_new(#values)
			for i,other in pairs(skeletonData.transformConstraints) do
				if other == constraint then
					timeline.transformConstraintIndex = i
					break
				end
			end
			local frameIndex = 0
			for i=1, #values do
				local valueMap = values[i]
				timeline:setFrame(frameIndex, valueMap.time, getValue(valueMap, "rotateMix", 1), getValue(valueMap, "translateMix", 1), getValue(valueMap, "scaleMix", 1), getValue(valueMap, "shearMix", 1))
				readCurve(valueMap, timeline, frameIndex)
				frameIndex = frameIndex + 1
			end
			timelines[#timelines + 1] = timeline
			duration = math_max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation_TransformConstraintTimeline_ENTRIES])
		end
	end

	-- Path constraint timelines.
	if map.paths then
		for constraintName,constraintMap in pairs(map.paths) do
			local index = skeletonData:findPathConstraintIndex(constraintName)
			if index == -1 then error("Path constraint not found: " .. constraintName, 2) end
			local data = skeletonData.pathConstraints[index]
			for timelineName, timelineMap in pairs(constraintMap) do
				if timelineName == "position" or timelineName == "spacing" then
					local timeline = nil
					local timelineScale = 1
					if timelineName == "spacing" then
						timeline = Animation_PathConstraintSpacingTimeline_new(#timelineMap)
						if data.spacingMode == PathConstraintData_SpacingMode_length or data.spacingMode == PathConstraintData_SpacingMode_fixed then timelineScale = scale end
					else
						timeline = Animation_PathConstraintPositionTimeline_new(#timelineMap)
						if data.positionMode == PathConstraintData_PositionMode_fixed then timelineScale = scale end
					end
					timeline.pathConstraintIndex = index
					local frameIndex = 0
					for i=1, #timelineMap do
						local valueMap = timelineMap[i]
						timeline:setFrame(frameIndex, valueMap.time, getValue(valueMap, timelineName, 0) * timelineScale)
						readCurve(valueMap, timeline, frameIndex)
						frameIndex = frameIndex + 1
					end
					timelines[#timelines + 1] = timeline
					duration = math_max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.PathConstraintPositionTimeline.ENTRIES])
				elseif timelineName == "mix" then
					local timeline = Animation_PathConstraintMixTimeline_new(#timelineMap)
					timeline.pathConstraintIndex = index
					local frameIndex = 0
					for i=1, #timelineMap do
						local valueMap = timelineMap[i]
						timeline:setFrame(frameIndex, valueMap.time, getValue(valueMap, "rotateMix", 1), getValue(valueMap, "translateMix", 1))
						readCurve(valueMap, timeline, frameIndex)
						frameIndex = frameIndex + 1
					end
					timelines[#timelines + 1] = timeline
					duration = math_max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.PathConstraintMixTimeline.ENTRIES])
				end
			end
		end
	end

	-- Deform timelines.
	if map.deform then
		for deformName, deformMap in pairs(map.deform) do
			local skin = skeletonData:findSkin(deformName)
			if not skin then error("Skin not found: " .. deformName, 2) end
			for slotName,slotMap in pairs(deformMap) do
				local slotIndex = skeletonData:findSlotIndex(slotName)
				if slotIndex == -1 then error("Slot not found: " .. slotMap.name, 2) end
				for timelineName,timelineMap in pairs(slotMap) do
					local attachment = skin:getAttachment(slotIndex, timelineName)
					if not attachment then error("Deform attachment not found: " .. timelineMap.name, 2) end
					local weighted = attachment.bones ~= nil
					local vertices = attachment.vertices;
					local deformLength = #vertices
					if weighted then deformLength = math_floor(#vertices / 3) * 2 end

					local timeline = Animation_DeformTimeline_new(#timelineMap)
					timeline.slotIndex = slotIndex
					timeline.attachment = attachment

					local frameIndex = 0
					for i=1, #timelineMap do
						local valueMap = timelineMap[i]
						local deform = nil
						local verticesValue = getValue(valueMap, "vertices", nil)
						if verticesValue == nil then
							deform = vertices
							if weighted then deform = utils_newNumberArray(deformLength) end
						else
							deform = utils_newNumberArray(deformLength)
							local start = getValue(valueMap, "offset", 0) + 1
							utils.arrayCopy(verticesValue, 1, deform, start, #verticesValue)
							if scale ~= 1 then
								local i = start
								local n = i + #verticesValue
								while i < n do
									deform[i] = deform[i] * scale
									i = i + 1
								end
							end
							if not weighted then
								local i = 1
								local n = i + deformLength
								while i < n do
									deform[i] = deform[i] + vertices[i]
									i = i + 1
								end
							end
						end

						timeline:setFrame(frameIndex, valueMap.time, deform)
						readCurve(valueMap, timeline, frameIndex)
						frameIndex = frameIndex + 1
					end
					timelines[#timelines + 1] = timeline
					duration = math_max(duration, timeline.frames[timeline:getFrameCount() - 1])
				end
			end
		end
	end

	-- Draworder timeline.
	local drawOrderValues = map["drawOrder"]
	if not drawOrderValues then drawOrderValues = map["draworder"] end
	if drawOrderValues then
		local timeline = Animation_DrawOrderTimeline_new(#drawOrderValues)
		local slotCount = #skeletonData.slots
		local frameIndex = 0

		for i=1, #drawOrderValues do
			local drawOrderMap = drawOrderValues[i]
			local drawOrder = nil
			local offsets = drawOrderMap["offsets"]
			if offsets then
				drawOrder = {}
				local unchanged = {}
				local originalIndex = 1
				local unchangedIndex = 1

				for ii=1, #offsets do
					local offsetMap = offsets[ii]
					local slotIndex = skeletonData:findSlotIndex(offsetMap["slot"])
					if slotIndex == -1 then error("Slot not found: " .. offsetMap["slot"]) end
					-- Collect unchanged items.
					while originalIndex ~= slotIndex do
						unchanged[unchangedIndex] = originalIndex
						unchangedIndex = unchangedIndex + 1
						originalIndex = originalIndex + 1
					end
					-- Set changed items.
					drawOrder[originalIndex + offsetMap["offset"]] = originalIndex
					originalIndex = originalIndex + 1
				end

				-- Collect remaining unchanged items.
				while originalIndex <= slotCount do
					unchanged[unchangedIndex] = originalIndex
					unchangedIndex = unchangedIndex + 1
					originalIndex = originalIndex + 1
				end
				-- Fill in unchanged items.
				for ii = slotCount, 1, -1 do
					if not drawOrder[ii] then
						unchangedIndex = unchangedIndex - 1
						drawOrder[ii] = unchanged[unchangedIndex]
					end
				end
			end
			timeline:setFrame(frameIndex, drawOrderMap["time"], drawOrder)
			frameIndex = frameIndex + 1
		end
		timelines[#timelines + 1] = timeline
		duration = math_max(duration, timeline.frames[timeline:getFrameCount() - 1])
	end

	-- Event timeline.
	local events = map["events"]
	if events then
		local timeline = Animation_EventTimeline_new(#events)
		local frameIndex = 0

		for i=1, #events do
			local eventMap = events[i]
			local eventData = skeletonData:findEvent(eventMap["name"])
			if not eventData then error("Event not found: " .. eventMap["name"]) end
			local event = Event_new(eventMap["time"], eventData)
			if eventMap["int"] ~= nil then
				event.intValue = eventMap["int"]
			else
				event.intValue = eventData.intValue
			end
			if eventMap["float"] ~= nil then
				event.floatValue = eventMap["float"]
			else
				event.floatValue = eventData.floatValue
			end
			if eventMap["string"] ~= nil then
				event.stringValue = eventMap["string"]
			else
				event.stringValue = eventData.stringValue
			end
			timeline:setFrame(frameIndex, event)
			frameIndex = frameIndex + 1
		end
		timelines[#timelines + 1] = timeline
		duration = math_max(duration, timeline.frames[timeline:getFrameCount() - 1])
	end

	local skeletonData_animations = skeletonData.animations
	skeletonData_animations[#skeletonData_animations + 1] = Animation_new(name, timelines, duration)
end

readCurve = function (map, timeline, frameIndex)
	local curve = map["curve"]
	if not curve then return end
	if curve == "stepped" then
		timeline:setStepped(frameIndex)
	elseif #curve > 0 then
		timeline:setCurve(frameIndex, curve[1], curve[2], curve[3], curve[4])
	end
end

getArray = function (map, name, scale)
	local list = map[name]
	local values = {}
	if scale == 1 then
		for i = 1, #list do
			values[i] = list[i]
		end
	else
		for i = 1, #list do
			values[i] = list[i] * scale
		end
	end
	return values
end


return SkeletonJson