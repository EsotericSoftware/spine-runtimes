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

local table_insert = table.insert
local SkeletonData = require "spine-lua.SkeletonData"
local BoneData = require "spine-lua.BoneData"
local SlotData = require "spine-lua.SlotData"
local Skin = require "spine-lua.Skin"
local AttachmentLoader = require "spine-lua.AttachmentLoader"
local Animation = require "spine-lua.Animation"
local IkConstraintData = require "spine-lua.IkConstraintData"
local IkConstraint = require "spine-lua.IkConstraint"
local PathConstraintData = require "spine-lua.PathConstraintData"
local PathConstraint = require "spine-lua.PathConstraint"
local TransformConstraintData = require "spine-lua.TransformConstraintData"
local TransformConstraint = require "spine-lua.TransformConstraint"
local EventData = require "spine-lua.EventData"
local Event = require "spine-lua.Event"
local AttachmentType = require "spine-lua.attachments.AttachmentType"
local BlendMode = require "spine-lua.BlendMode"
local TransformMode = require "spine-lua.TransformMode"
local utils = require "spine-lua.utils"
local Color = require "spine-lua.Color"

local SkeletonJson = {}
function SkeletonJson.new (attachmentLoader)
	if not attachmentLoader then attachmentLoader = AttachmentLoader.new() end

	local self = {
		attachmentLoader = attachmentLoader,
		scale = 1,
		linkedMeshes = {}
	}

	function self:readSkeletonDataFile (fileName, base)
		return self:readSkeletonData(spine.utils.readFile(fileName, base))
	end

	local readAttachment
	local readAnimation
	local readCurve
	local getArray

	local getValue = function (map, name, default)
		local value = map[name]
		if value == nil then return default else return value end
	end

	function self:readSkeletonData (jsonText)
		local scale = self.scale
		local skeletonData = SkeletonData.new(self.attachmentLoader)
		local root = spine.utils.readJSON(jsonText)
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
		for i,boneMap in ipairs(root["bones"]) do
			local boneName = boneMap["name"]

			local parent = nil
			local parentName = boneMap["parent"]
			if parentName then
				parent = skeletonData:findBone(parentName)
				if not parent then error("Parent bone not found: " .. parentName) end
			end
			local data = BoneData.new(i, boneName, parent)
			data.length = getValue(boneMap, "length", 0) * scale;
			data.x = getValue(boneMap, "x", 0) * scale;
			data.y = getValue(boneMap, "y", 0) * scale;
			data.rotation = getValue(boneMap, "rotation", 0);
			data.scaleX = getValue(boneMap, "scaleX", 1);
			data.scaleY = getValue(boneMap, "scaleY", 1);
			data.shearX = getValue(boneMap, "shearX", 0);
			data.shearY = getValue(boneMap, "shearY", 0);
			data.transformMode = TransformMode[getValue(boneMap, "transform", "normal")]

			table_insert(skeletonData.bones, data)
		end

		-- Slots.
		if root["slots"] then
			for i,slotMap in ipairs(root["slots"]) do
				local index = i
				local slotName = slotMap["name"]
				local boneName = slotMap["bone"]
				local boneData = skeletonData:findBone(boneName)
				if not boneData then error("Slot bone not found: " .. boneName) end
				local data = SlotData.new(i, slotName, boneData)

				local color = slotMap["color"]
				if color then
					data.color:set(tonumber(color:sub(1, 2), 16) / 255,
					               tonumber(color:sub(3, 4), 16) / 255,
					               tonumber(color:sub(5, 6), 16) / 255,
					               tonumber(color:sub(7, 8), 16) / 255)
				end

				local dark = slotMap["dark"]
				if dark then
					data.darkColor = Color.newWith(1, 1, 1, 1)
					data.darkColor:set(tonumber(dark:sub(1, 2), 16) / 255,
					               tonumber(dark:sub(3, 4), 16) / 255,
					               tonumber(dark:sub(5, 6), 16) / 255,
					               0)
				end

				data.attachmentName = getValue(slotMap, "attachment", nil)
				data.blendMode = BlendMode[getValue(slotMap, "blend", "normal")]

				table_insert(skeletonData.slots, data)
				skeletonData.slotNameIndices[data.name] = #skeletonData.slots
			end
		end

		-- IK constraints.
		if root["ik"] then
			for i,constraintMap in ipairs(root["ik"]) do
				local data = IkConstraintData.new(constraintMap["name"])
				data.order = getValue(constraintMap, "order", 0)

				for i,boneName in ipairs(constraintMap["bones"]) do
					local bone = skeletonData:findBone(boneName)
					if not bone then error("IK bone not found: " .. boneName) end
					table_insert(data.bones, bone)
				end

				local targetName = constraintMap["target"]
				data.target = skeletonData:findBone(targetName)
				if not data.target then error("Target bone not found: " .. targetName) end

				if constraintMap["bendPositive"] == false then data.bendDirection = -1 else data.bendDirection = 1 end
				data.mix = getValue(constraintMap, "mix", 1)

				table_insert(skeletonData.ikConstraints, data)
			end
		end

		-- Transform constraints
		if root["transform"] then
			for i,constraintMap in ipairs(root["transform"]) do
				data = TransformConstraintData.new(constraintMap.name)
				data.order = getValue(constraintMap, "order", 0)

				for i,boneName in ipairs(constraintMap.bones) do
					local bone = skeletonData:findBone(boneName)
					if not bone then error("Transform constraint bone not found: " .. boneName, 2) end
					table_insert(data.bones, bone)
				end

				local targetName = constraintMap.target
				data.target = skeletonData:findBone(targetName)
				if not data.target then error("Transform constraint target bone not found: " .. boneName, 2) end

				data.offsetRotation = getValue(constraintMap, "rotation", 0);
				data.offsetX = getValue(constraintMap, "x", 0) * scale;
				data.offsetY = getValue(constraintMap, "y", 0) * scale;
				data.offsetScaleX = getValue(constraintMap, "scaleX", 0);
				data.offsetScaleY = getValue(constraintMap, "scaleY", 0);
				data.offsetShearY = getValue(constraintMap, "shearY", 0);

				data.rotateMix = getValue(constraintMap, "rotateMix", 1);
				data.translateMix = getValue(constraintMap, "translateMix", 1);
				data.scaleMix = getValue(constraintMap, "scaleMix", 1);
				data.shearMix = getValue(constraintMap, "shearMix", 1);

				table_insert(skeletonData.transformConstraints, data)
			end
		end

		-- Path constraints
		if root["path"] then
			for i,constraintMap in ipairs(root.path) do
				local data = PathConstraintData.new(constraintMap.name);
				data.order = getValue(constraintMap, "order", 0)

				for i,boneName in ipairs(constraintMap.bones) do
					local bone = skeletonData:findBone(boneName)
					if not bone then error("Path constraint bone not found: " .. boneName, 2) end
					table_insert(data.bones, bone)
				end

				local targetName = constraintMap.target;
				data.target = skeletonData:findSlot(targetName)
				if data.target == nil then error("Path target slot not found: " .. targetName, 2) end

				data.positionMode = PathConstraintData.PositionMode[getValue(constraintMap, "positionMode", "percent"):lower()]
				data.spacingMode = PathConstraintData.SpacingMode[getValue(constraintMap, "spacingMode", "length"):lower()]
				data.rotateMode = PathConstraintData.RotateMode[getValue(constraintMap, "rotateMode", "tangent"):lower()]
				data.offsetRotation = getValue(constraintMap, "rotation", 0);
				data.position = getValue(constraintMap, "position", 0);
				if data.positionMode == PathConstraintData.PositionMode.fixed then data.position = data.position * scale end
				data.spacing = getValue(constraintMap, "spacing", 0);
				if data.spacingMode == PathConstraintData.SpacingMode.length or data.spacingMode == PathConstraintData.SpacingMode.fixed then data.spacing = data.spacing * scale end
				data.rotateMix = getValue(constraintMap, "rotateMix", 1);
				data.translateMix = getValue(constraintMap, "translateMix", 1);

				table_insert(skeletonData.pathConstraints, data)
			end
		end

		-- Skins.
		if root["skins"] then
			for skinName,skinMap in pairs(root["skins"]) do
				local skin = Skin.new(skinName)
				for slotName,slotMap in pairs(skinMap) do
					local slotIndex = skeletonData.slotNameIndices[slotName]
					for attachmentName,attachmentMap in pairs(slotMap) do
						local attachment = readAttachment(attachmentMap, skin, slotIndex, attachmentName, skeletonData)
						if attachment then
							skin:addAttachment(slotIndex, attachmentName, attachment)
						end
					end
				end
				table_insert(skeletonData.skins, skin)
				if skin.name == "default" then skeletonData.defaultSkin = skin end
			end
		end

		-- Linked meshes
		for i, linkedMesh in ipairs(self.linkedMeshes) do
			local skin = skeletonData.defaultSkin
			if linkedMesh.skin then skin = skeletonData:findSkin(linkedMesh.skin) end
			if not skin then error("Skin not found: " .. linkedMesh.skin) end
			local parent = skin:getAttachment(linkedMesh.slotIndex, linkedMesh.parent)
			if not parent then error("Parent mesh not found: " + linkedMesh.parent) end
			linkedMesh.mesh:setParentMesh(parent)
			linkedMesh.mesh:updateUVs()
		end
		self.linkedMeshes = {}

		-- Events.
		if root["events"] then
			for eventName,eventMap in pairs(root["events"]) do
				local data = EventData.new(eventName)
				data.intValue = getValue(eventMap, "int", 0)
				data.floatValue = getValue(eventMap, "float", 0)
				data.stringValue = getValue(eventMap, "string", "")
				table_insert(skeletonData.events, data)
			end
		end

		-- Animations.
		if root["animations"] then
			for animationName,animationMap in pairs(root["animations"]) do
				readAnimation(animationMap, animationName, skeletonData)
			end
		end

		return skeletonData
	end

	readAttachment = function (map, skin, slotIndex, name, skeletonData)
		local scale = self.scale
		name = getValue(map, "name", name)

		local type = AttachmentType[getValue(map, "type", "region")]
		local path = getValue(map, "path", name)

		if type == AttachmentType.region then
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

		elseif type == AttachmentType.boundingbox then
			local box = attachmentLoader:newBoundingBoxAttachment(skin, name)
			if not box then return nil end
			readVertices(map, box, map.vertexCount * 2)
			local color = map.color
			if color then
				box.color:set(tonumber(color:sub(1, 2), 16) / 255,
				              tonumber(color:sub(3, 4), 16) / 255,
				              tonumber(color:sub(5, 6), 16) / 255,
				              tonumber(color:sub(7, 8), 16) / 255)
			end
			return box

		elseif type == AttachmentType.mesh or type == AttachmentType.linkedmesh then
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
				table_insert(self.linkedMeshes, {
						mesh = mesh,
						skin = getValue(map, "skin", nil),
						slotIndex = slotIndex,
						parent = parent
				})
				return mesh
			end

			local uvs = getArray(map, "uvs", 1)
			readVertices(map, mesh, #uvs)
			mesh.triangles = getArray(map, "triangles", 1)
			-- adjust triangle indices by 1, vertices are one-indexed
			for i,v in ipairs(mesh.triangles) do
				mesh.triangles[i] = v + 1
			end
			mesh.regionUVs = uvs
			mesh:updateUVs()

			mesh.hullLength = getValue(map, "hull", 0) * 2
			return mesh

		elseif type == AttachmentType.path then
			local path = self.attachmentLoader:newPathAttachment(skin, name)
			if not path then return nil end
			path.closed = getValue(map, "closed", false)
			path.constantSpeed = getValue(map, "constantSpeed", true)

			local vertexCount = map.vertexCount
			readVertices(map, path, vertexCount * 2)

			local lengths = utils.newNumberArray(vertexCount / 3, 0)
			for i,v in ipairs(map.lengths) do
				lengths[i] = v * scale
			end
			path.lengths = lengths

			local color = map.color
			if color then
				path.color:set(tonumber(color:sub(1, 2), 16) / 255,
				               tonumber(color:sub(3, 4), 16) / 255,
				               tonumber(color:sub(5, 6), 16) / 255,
				               tonumber(color:sub(7, 8), 16) / 255)
			end
			return path;
		
		elseif type == AttachmentType.point then
			local point = self.attachmentLoader:newPointAttachment(skin, name)
			if not point then return nil end
			point.x = getValue(map, "x", 0) * scale
			point.y = getValue(map, "y", 0) * scale
			point.rotation = getValue(map, "rotation", 0)
			
			local color = map.color
			if color then
				path.color:set(tonumber(color:sub(1, 2), 16) / 255,
				               tonumber(color:sub(3, 4), 16) / 255,
				               tonumber(color:sub(5, 6), 16) / 255,
				               tonumber(color:sub(7, 8), 16) / 255)
			end
			return point
		
		elseif type == AttachmentType.clipping then
			local clip = attachmentLoader:newClippingAttachment(skin, name)
			if not clip then return nil end
			
			local _end = getValue(map, "end", nil)
			if _end then
				local slot = skeletonData:findSlot(_end)
				if not slot then error("Clipping end slot not found: " + _end) end
				clip.endSlot = slot
			end
			
			readVertices(map, clip, map.vertexCount * 2)
			local color = map.color
			if color then
				clip.color:set(tonumber(color:sub(1, 2), 16) / 255,
				              tonumber(color:sub(3, 4), 16) / 255,
				              tonumber(color:sub(5, 6), 16) / 255,
				              tonumber(color:sub(7, 8), 16) / 255)
			end
			return clip
		end

		error("Unknown attachment type: " .. type .. " (" .. name .. ")")
	end

	readVertices = function (map, attachment, verticesLength)
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
			table_insert(bones, boneCount)
			local nn = i + boneCount * 4
			while i < nn do
				table_insert(bones, vertices[i + 1] + 1) -- +1 because bones are one-indexed
				table_insert(weights, vertices[i + 2] * scale)
				table_insert(weights, vertices[i + 3] * scale)
				table_insert(weights, vertices[i + 4])
				i = i + 4
			end
		end
		attachment.bones = bones
		attachment.vertices = weights
	end

	readAnimation = function (map, name, skeletonData)
		local timelines = {}
		local duration = 0
		local scale = self.scale

		-- Slot timelines
		local slotsMap = map["slots"]
		if slotsMap then
			for slotName,timelineMap in pairs(slotsMap) do
				local slotIndex = skeletonData.slotNameIndices[slotName]

				for timelineName,values in pairs(timelineMap) do
					if timelineName == "color" then
						local timeline = Animation.ColorTimeline.new(#values)
						timeline.slotIndex = slotIndex

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
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
						table_insert(timelines, timeline)
						duration = math.max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.ColorTimeline.ENTRIES])
					elseif timelineName == "twoColor" then
						local timeline = Animation.TwoColorTimeline.new(#values)
						timeline.slotIndex = slotIndex

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							local light = valueMap["light"]
							local dark = valueMap["dark"]
							timeline:setFrame(
								frameIndex, valueMap["time"],
								tonumber(light:sub(1, 2), 16) / 255,
								tonumber(light:sub(3, 4), 16) / 255,
								tonumber(light:sub(5, 6), 16) / 255,
								tonumber(light:sub(7, 8), 16) / 255,
								tonumber(dark:sub(1, 2), 16) / 255,
								tonumber(dark:sub(3, 4), 16) / 255,
								tonumber(dark:sub(5, 6), 16) / 255
							)
							readCurve(valueMap, timeline, frameIndex)
							frameIndex = frameIndex + 1
						end
						table_insert(timelines, timeline)
						duration = math.max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.TwoColorTimeline.ENTRIES])
					elseif timelineName == "attachment" then
						local timeline = Animation.AttachmentTimeline.new(#values)
						timeline.slotIndex = slotIndex

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							local attachmentName = valueMap["name"]
							timeline:setFrame(frameIndex, valueMap["time"], attachmentName)
							frameIndex = frameIndex + 1
						end
						table_insert(timelines, timeline)
						duration = math.max(duration, timeline.frames[timeline:getFrameCount() - 1])

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
						local timeline = Animation.RotateTimeline.new(#values)
						timeline.boneIndex = boneIndex

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							timeline:setFrame(frameIndex, valueMap["time"], valueMap["angle"])
							readCurve(valueMap, timeline, frameIndex)
							frameIndex = frameIndex + 1
						end
						table_insert(timelines, timeline)
						duration = math.max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.RotateTimeline.ENTRIES])

					elseif timelineName == "translate" or timelineName == "scale" or timelineName == "shear" then
						local timeline
						local timelineScale = 1
						if timelineName == "scale" then
							timeline = Animation.ScaleTimeline.new(#values)
						elseif timelineName == "shear" then
							timeline = Animation.ShearTimeline.new(#values)
						else
							timeline = Animation.TranslateTimeline.new(#values)
							timelineScale = self.scale
						end
						timeline.boneIndex = boneIndex

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							local x = (valueMap["x"] or 0) * timelineScale
							local y = (valueMap["y"] or 0) * timelineScale
							timeline:setFrame(frameIndex, valueMap["time"], x, y)
							readCurve(valueMap, timeline, frameIndex)
							frameIndex = frameIndex + 1
						end
						table_insert(timelines, timeline)
						duration = math.max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.TranslateTimeline.ENTRIES])
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
				local timeline = Animation.IkConstraintTimeline.new(#values)
				for i,other in pairs(skeletonData.ikConstraints) do
					if other == ikConstraint then
						timeline.ikConstraintIndex = i
						break
					end
				end
				local frameIndex = 0
				for i,valueMap in ipairs(values) do
					local mix = 1
					if valueMap["mix"] ~= nil then mix = valueMap["mix"] end
					local bendPositive = 1
					if valueMap["bendPositive"] == false then bendPositive = -1 end
					timeline:setFrame(frameIndex, valueMap["time"], mix, bendPositive)
					readCurve(valueMap, timeline, frameIndex)
					frameIndex = frameIndex + 1
				end
				table_insert(timelines, timeline)
				duration = math.max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.IkConstraintTimeline.ENTRIES])
			end
		end

		-- Transform constraint timelines.
		local transform = map["transform"]
		if transform then
			for constraintName, values in pairs(transform) do
				local constraint = skeletonData:findTransformConstraint(constraintName)
				local timeline = Animation.TransformConstraintTimeline.new(#values)
				for i,other in pairs(skeletonData.transformConstraints) do
					if other == constraint then
						timeline.transformConstraintIndex = i
						break
					end
				end
				local frameIndex = 0
				for i,valueMap in ipairs(values) do
					timeline:setFrame(frameIndex, valueMap.time, getValue(valueMap, "rotateMix", 1), getValue(valueMap, "translateMix", 1), getValue(valueMap, "scaleMix", 1), getValue(valueMap, "shearMix", 1))
					readCurve(valueMap, timeline, frameIndex)
					frameIndex = frameIndex + 1
				end
				table_insert(timelines, timeline)
				duration = math.max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.TransformConstraintTimeline.ENTRIES])
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
							timeline = Animation.PathConstraintSpacingTimeline.new(#timelineMap)
							if data.spacingMode == PathConstraintData.SpacingMode.length or data.spacingMode == PathConstraintData.SpacingMode.fixed then timelineScale = scale end
						else
							timeline = Animation.PathConstraintPositionTimeline.new(#timelineMap)
							if data.positionMode == PathConstraintData.PositionMode.fixed then timelineScale = scale end
						end
						timeline.pathConstraintIndex = index
						local frameIndex = 0
						for i,valueMap in ipairs(timelineMap) do
							timeline:setFrame(frameIndex, valueMap.time, getValue(valueMap, timelineName, 0) * timelineScale)
							readCurve(valueMap, timeline, frameIndex)
							frameIndex = frameIndex + 1
						end
						table_insert(timelines, timeline)
						duration = math.max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.PathConstraintPositionTimeline.ENTRIES])
					elseif timelineName == "mix" then
						local timeline = Animation.PathConstraintMixTimeline.new(#timelineMap)
						timeline.pathConstraintIndex = index
						local frameIndex = 0
						for i,valueMap in ipairs(timelineMap) do
							timeline:setFrame(frameIndex, valueMap.time, getValue(valueMap, "rotateMix", 1), getValue(valueMap, "translateMix", 1))
							readCurve(valueMap, timeline, frameIndex)
							frameIndex = frameIndex + 1
						end
						table_insert(timelines, timeline)
						duration = math.max(duration, timeline.frames[(timeline:getFrameCount() - 1) * Animation.PathConstraintMixTimeline.ENTRIES])
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
						if weighted then deformLength = math.floor(#vertices / 3) * 2 end

						local timeline = Animation.DeformTimeline.new(#timelineMap)
						timeline.slotIndex = slotIndex
						timeline.attachment = attachment

						local frameIndex = 0
						for i,valueMap in ipairs(timelineMap) do
							local deform = nil
							local verticesValue = getValue(valueMap, "vertices", nil)
							if verticesValue == nil then
								deform = vertices
								if weighted then deform = utils.newNumberArray(deformLength) end
							else
								deform = utils.newNumberArray(deformLength)
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
						table_insert(timelines, timeline)
						duration = math.max(duration, timeline.frames[timeline:getFrameCount() - 1])
					end
				end
			end
		end

		-- Draworder timeline.
		local drawOrderValues = map["drawOrder"]
		if not drawOrderValues then drawOrderValues = map["draworder"] end
		if drawOrderValues then
			local timeline = Animation.DrawOrderTimeline.new(#drawOrderValues)
			local slotCount = #skeletonData.slots
			local frameIndex = 0
			for i,drawOrderMap in ipairs(drawOrderValues) do
				local drawOrder = nil
				local offsets = drawOrderMap["offsets"]
				if offsets then
					drawOrder = {}
					local unchanged = {}
					local originalIndex = 1
					local unchangedIndex = 1
					for ii,offsetMap in ipairs(offsets) do
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
			table_insert(timelines, timeline)
			duration = math.max(duration, timeline.frames[timeline:getFrameCount() - 1])
		end

		-- Event timeline.
		local events = map["events"]
		if events then
			local timeline = Animation.EventTimeline.new(#events)
			local frameIndex = 0
			for i,eventMap in ipairs(events) do
				local eventData = skeletonData:findEvent(eventMap["name"])
				if not eventData then error("Event not found: " .. eventMap["name"]) end
				local event = Event.new(eventMap["time"], eventData)
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
			table_insert(timelines, timeline)
			duration = math.max(duration, timeline.frames[timeline:getFrameCount() - 1])
		end

		table_insert(skeletonData.animations, Animation.new(name, timelines, duration))
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

	return self
end
return SkeletonJson
