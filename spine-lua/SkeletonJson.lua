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

local SkeletonData = require "spine-lua.SkeletonData"
local BoneData = require "spine-lua.BoneData"
local SlotData = require "spine-lua.SlotData"
local Skin = require "spine-lua.Skin"
local AttachmentLoader = require "spine-lua.AttachmentLoader"
local Animation = require "spine-lua.Animation"
local IkConstraintData = require "spine-lua.IkConstraintData"
local IkConstraint = require "spine-lua.IkConstraint"
local EventData = require "spine-lua.EventData"
local Event = require "spine-lua.Event"
local AttachmentType = require "spine-lua.AttachmentType"
local BlendMode = require "spine-lua.BlendMode"

local SkeletonJson = {}
function SkeletonJson.new (attachmentLoader)
	if not attachmentLoader then attachmentLoader = AttachmentLoader.new() end

	local self = {
		attachmentLoader = attachmentLoader,
		scale = 1
	}

	function self:readSkeletonDataFile (fileName, base)
		return self:readSkeletonData(spine.utils.readFile(fileName, base))
	end

	local readAttachment
	local readAnimation
	local readCurve
	local getArray

	function self:readSkeletonData (jsonText)
		local skeletonData = SkeletonData.new(self.attachmentLoader)

		local root = spine.utils.readJSON(jsonText)
		if not root then error("Invalid JSON: " .. jsonText, 2) end

		-- Skeleton.
		if root["skeleton"] then
			local skeletonMap = root["skeleton"]
			skeletonData.hash = skeletonMap["hash"]
			skeletonData.version = skeletonMap["spine"]
			skeletonData.width = skeletonMap["width"] or 0
			skeletonData.height = skeletonMap["height"] or 0
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
			local boneData = BoneData.new(boneName, parent)
			boneData.length = (boneMap["length"] or 0) * self.scale
			boneData.x = (boneMap["x"] or 0) * self.scale
			boneData.y = (boneMap["y"] or 0) * self.scale
			boneData.rotation = (boneMap["rotation"] or 0)
			if boneMap["scaleX"] ~= nil then
				boneData.scaleX = boneMap["scaleX"]
			else
				boneData.scaleX = 1
			end
			if boneMap["scaleY"] ~= nil then
				boneData.scaleY = boneMap["scaleY"]
			else
				boneData.scaleY = 1
			end
			boneData.flipX = boneMap["flipX"] or false
			boneData.flipY = boneMap["flipY"] or false
			if boneMap["inheritScale"] == false then
				boneData.inheritScale = false
			else
				boneData.inheritScale = true
			end
			if boneMap["inheritRotation"] == false then
				boneData.inheritRotation = false
			else
				boneData.inheritRotation = true
			end
			table.insert(skeletonData.bones, boneData)
		end

		-- IK constraints.
		if root["ik"] then
			for i,ikMap in ipairs(root["ik"]) do
				local ikConstraintData = IkConstraintData.new(ikMap["name"])

				for i,boneName in ipairs(ikMap["bones"]) do
					local bone = skeletonData:findBone(boneName)
					if not bone then error("IK bone not found: " .. boneName) end
					table.insert(ikConstraintData.bones, bone)
				end

				local targetName = ikMap["target"]
				ikConstraintData.target = skeletonData:findBone(targetName)
				if not ikConstraintData.target then error("Target bone not found: " .. targetName) end

				if ikMap["bendPositive"] == false then ikConstraintData.bendDirection = -1 end
				if ikMap["mix"] ~= nil then ikConstraintData.mix = ikMap["mix"] end

				table.insert(skeletonData.ikConstraints, ikConstraintData)
			end
		end

		-- Slots.
		if root["slots"] then
			for i,slotMap in ipairs(root["slots"]) do
				local slotName = slotMap["name"]
				local boneName = slotMap["bone"]
				local boneData = skeletonData:findBone(boneName)
				if not boneData then error("Slot bone not found: " .. boneName) end
				local slotData = SlotData.new(slotName, boneData)

				local color = slotMap["color"]
				if color then
					slotData:setColor(
						tonumber(color:sub(1, 2), 16) / 255,
						tonumber(color:sub(3, 4), 16) / 255,
						tonumber(color:sub(5, 6), 16) / 255,
						tonumber(color:sub(7, 8), 16) / 255
					)
				end

				slotData.attachmentName = slotMap["attachment"]
				slotData.blendMode = BlendMode[slotMap["blend"] or "normal"]

				table.insert(skeletonData.slots, slotData)
				skeletonData.slotNameIndices[slotData.name] = #skeletonData.slots
			end
		end

		-- Skins.
		if root["skins"] then
			for skinName,skinMap in pairs(root["skins"]) do
				local skin = Skin.new(skinName)
				for slotName,slotMap in pairs(skinMap) do
					local slotIndex = skeletonData.slotNameIndices[slotName]
					for attachmentName,attachmentMap in pairs(slotMap) do
						local attachment = readAttachment(attachmentName, attachmentMap)
						if attachment then
							skin:addAttachment(slotIndex, attachmentName, attachment)
						end
					end
				end
				if skin.name == "default" then
					skeletonData.defaultSkin = skin
				end
				table.insert(skeletonData.skins, skin)
			end
		end

		-- Events.
		if root["events"] then
			for eventName,eventMap in pairs(root["events"]) do
				local eventData = EventData.new(eventName)
				eventData.intValue = eventMap["int"] or 0
				eventData.floatValue = eventMap["float"] or 0
				eventData.stringValue = eventMap["string"]
				table.insert(skeletonData.events, eventData)
			end
		end

		-- Animations.
		if root["animations"] then
			for animationName,animationMap in pairs(root["animations"]) do
				readAnimation(animationName, animationMap, skeletonData)
			end
		end

		return skeletonData
	end

	readAttachment = function (name, map)
		name = map["name"] or name

		local type = AttachmentType[map["type"] or "region"]
		local path = map["path"] or name

		local scale = self.scale
		if type == AttachmentType.region then
			local region = attachmentLoader:newRegionAttachment(type, name, path)
			if not region then return nil end
			region.x = (map["x"] or 0) * scale
			region.y = (map["y"] or 0) * scale
			if map["scaleX"] ~= nil then
				region.scaleX = map["scaleX"]
			else
				region.scaleX = 1
			end
			if map["scaleY"] ~= nil then
				region.scaleY = map["scaleY"]
			else
				region.scaleY = 1
			end
			region.rotation = (map["rotation"] or 0)
			region.width = map["width"] * scale
			region.height = map["height"] * scale
			
			local color = map["color"]
			if color then
				region.r = tonumber(color:sub(1, 2), 16) / 255
				region.g = tonumber(color:sub(3, 4), 16) / 255
				region.b = tonumber(color:sub(5, 6), 16) / 255
				region.a = tonumber(color:sub(7, 8), 16) / 255
			end

			region:updateOffset()
			return region

		elseif type == AttachmentType.mesh then
			local mesh = attachmentLoader:newMeshAttachment(skin, name, path)
			if not mesh then return null end
			mesh.path = path 
			mesh.vertices = getArray(map, "vertices", scale)
			mesh.triangles = getArray(map, "triangles", 1)
			mesh.regionUVs = getArray(map, "uvs", 1)
			mesh:updateUVs()

			local color = map["color"]
			if color then
				mesh.r = tonumber(color:sub(1, 2), 16) / 255
				mesh.g = tonumber(color:sub(3, 4), 16) / 255
				mesh.b = tonumber(color:sub(5, 6), 16) / 255
				mesh.a = tonumber(color:sub(7, 8), 16) / 255
			end

			mesh.hullLength = (map["hull"] or 0) * 2
			if map["edges"] then mesh.edges = getArray(map, "edges", 1) end
			mesh.width = (map["width"] or 0) * scale
			mesh.height = (map["height"] or 0) * scale
			return mesh

		elseif type == AttachmentType.skinnedmesh then
			local mesh = self.attachmentLoader.newSkinningMeshAttachment(skin, name, path)
			if not mesh then return null end
			mesh.path = path

			local uvs = getArray(map, "uvs", 1)
			local vertices = getArray(map, "vertices", 1)
			local weights = {}
			local bones = {}
			local i, n = 1, #vertices
			while i < n do
				local boneCount = vertices[i]
				i = i + 1
				table.insert(bones, boneCount)
				local nn = i + boneCount * 4
				while i < nn do
					table.insert(bones, vertices[i])
					table.insert(weights, vertices[i + 1] * scale)
					table.insert(weights, vertices[i + 2] * scale)
					table.insert(weights, vertices[i + 3])
					i = i + 4
				end
			end
			mesh.bones = bones
			mesh.weights = weights
			mesh.triangles = getArray(map, "triangles", 1)
			mesh.regionUVs = uvs
			mesh:updateUVs()

			local color = map["color"]
			if color then
				mesh.r = tonumber(color:sub(1, 2), 16) / 255
				mesh.g = tonumber(color:sub(3, 4), 16) / 255
				mesh.b = tonumber(color:sub(5, 6), 16) / 255
				mesh.a = tonumber(color:sub(7, 8), 16) / 255
			end

			mesh.hullLength = (map["hull"] or 0) * 2
			if map["edges"] then mesh.edges = getArray(map, "edges", 1) end
			mesh.width = (map["width"] or 0) * scale
			mesh.height = (map["height"] or 0) * scale
			return mesh

		elseif type == AttachmentType.boundingbox then
			local box = attachmentLoader:newBoundingBoxAttachment(type, name)
			if not box then return nil end
			local vertices = map["vertices"]
			for i,point in ipairs(vertices) do
				table.insert(box.vertices, vertices[i] * scale)
			end
			return box
		end

		error("Unknown attachment type: " .. type .. " (" .. name .. ")")
	end

	readAnimation = function (name, map, skeletonData)
		local timelines = {}
		local duration = 0

		local slotsMap = map["slots"]
		if slotsMap then
			for slotName,timelineMap in pairs(slotsMap) do
				local slotIndex = skeletonData.slotNameIndices[slotName]

				for timelineName,values in pairs(timelineMap) do
					if timelineName == "color" then
						local timeline = Animation.ColorTimeline.new()
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
							readCurve(timeline, frameIndex, valueMap)
							frameIndex = frameIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					elseif timelineName == "attachment" then
						local timeline = Animation.AttachmentTimeline.new()
						timeline.slotName = slotName

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							local attachmentName = valueMap["name"]
							if not attachmentName then attachmentName = nil end
							timeline:setFrame(frameIndex, valueMap["time"], attachmentName)
							frameIndex = frameIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					else
						error("Invalid frame type for a slot: " .. timelineName .. " (" .. slotName .. ")")
					end
				end
			end
		end

		local bonesMap = map["bones"]
		if bonesMap then
			for boneName,timelineMap in pairs(bonesMap) do
				local boneIndex = skeletonData:findBoneIndex(boneName)
				if boneIndex == -1 then error("Bone not found: " .. boneName) end

				for timelineName,values in pairs(timelineMap) do
					if timelineName == "rotate" then
						local timeline = Animation.RotateTimeline.new()
						timeline.boneIndex = boneIndex

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							timeline:setFrame(frameIndex, valueMap["time"], valueMap["angle"])
							readCurve(timeline, frameIndex, valueMap)
							frameIndex = frameIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					elseif timelineName == "translate" or timelineName == "scale" then
						local timeline
						local timelineScale = 1
						if timelineName == "scale" then
							timeline = Animation.ScaleTimeline.new()
						else
							timeline = Animation.TranslateTimeline.new()
							timelineScale = self.scale
						end
						timeline.boneIndex = boneIndex

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							local x = (valueMap["x"] or 0) * timelineScale
							local y = (valueMap["y"] or 0) * timelineScale
							timeline:setFrame(frameIndex, valueMap["time"], x, y)
							readCurve(timeline, frameIndex, valueMap)
							frameIndex = frameIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					elseif timelineName == "flipX" or timelineName == "flipY" then
						local x = timelineName == "flipX"
						local timeline, field
						if x then
							timeline = Animation.FlipXTimeline.new()
							field = "x"
						else
							timeline = Animation.FlipYTimeline.new();
							field = "y"
						end
						timeline.boneIndex = boneIndex

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							timeline:setFrame(frameIndex, valueMap["time"], valueMap[field] or false)
							frameIndex = frameIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					else
						error("Invalid timeline type for a bone: " .. timelineName .. " (" .. boneName .. ")")
					end
				end
			end
		end

		local ik = map["ik"]
		if ik then
			for ikConstraintName,values in pairs(ik) do
				local ikConstraint = skeletonData:findIkConstraint(ikConstraintName)
				local timeline = Animation.IkConstraintTimeline.new()
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
					readCurve(timeline, frameIndex, valueMap)
					frameIndex = frameIndex + 1
				end
				table.insert(timelines, timeline)
				duration = math.max(duration, timeline:getDuration())
			end
		end

		local ffd = map["ffd"]
		if ffd then
			for skinName,slotMap in pairs(ffd) do
				local skin = skeletonData:findSkin(skinName)
				for slotName,meshMap in pairs(slotMap) do
					local slotIndex = skeletonData:findSlotIndex(slotName)
					for meshName,values in pairs(meshMap) do
						local timeline = Animation.FfdTimeline.new()
						local attachment = skin:getAttachment(slotIndex, meshName)
						if not attachment then error("FFD attachment not found: " .. meshName) end
						timeline.slotIndex = slotIndex
						timeline.attachment = attachment
						local isMesh = attachment.type == AttachmentType.mesh
						local vertexCount
						if isMesh then
							vertexCount = #attachment.vertices
						else
							vertexCount = #attachment.weights / 3 * 2
						end

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							local vertices
							if not valueMap["vertices"] then
								if isMesh then
									vertices = attachment.vertices
								else
									vertices = {}
									for i = 1, vertexCount do
										vertices[i] = 0
									end
								end
							else
								local verticesValue = valueMap["vertices"]
								local scale = self.scale
								vertices = {}
								local start = valueMap["offset"] or 0
								for ii = 1, start do
									vertices[ii] = 0
								end
								if scale == 1 then
									for ii = 1, #verticesValue do
										vertices[ii + start] = verticesValue[ii]
									end
								else
									for ii = 1, #verticesValue do
										vertices[ii + start] = verticesValue[ii] * scale
									end
								end
								if isMesh then
									local meshVertices = attachment.vertices
									for ii = 1, vertexCount do
										vertices[ii] = vertices[ii] + meshVertices[ii]
									end
								elseif #verticesValue < vertexCount then
									vertices[vertexCount] = 0
								end
							end
							timeline:setFrame(frameIndex, valueMap["time"], vertices)
							readCurve(timeline, frameIndex, valueMap)
							frameIndex = frameIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())
					end
				end
			end
		end

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
			table.insert(timelines, timeline)
			duration = math.max(duration, timeline:getDuration())
		end

		local events = map["events"]
		if events then
			local timeline = Animation.EventTimeline.new(#events)
			local frameIndex = 0
			for i,eventMap in ipairs(events) do
				local eventData = skeletonData:findEvent(eventMap["name"])
				if not eventData then error("Event not found: " .. eventMap["name"]) end
				local event = Event.new(eventData)
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
				timeline:setFrame(frameIndex, eventMap["time"], event)
				frameIndex = frameIndex + 1
			end
			table.insert(timelines, timeline)
			duration = math.max(duration, timeline:getDuration())
		end

		table.insert(skeletonData.animations, Animation.new(name, timelines, duration))
	end

	readCurve = function (timeline, frameIndex, valueMap)
		local curve = valueMap["curve"]
		if not curve then 
			timeline:setLinear(frameIndex)
		elseif curve == "stepped" then
			timeline:setStepped(frameIndex)
		else
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
