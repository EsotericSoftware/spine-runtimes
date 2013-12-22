------------------------------------------------------------------------------
 -- Spine Runtime Software License - Version 1.0
 -- 
 -- Copyright (c) 2013, Esoteric Software
 -- All rights reserved.
 -- 
 -- Redistribution and use in source and binary forms in whole or in part, with
 -- or without modification, are permitted provided that the following conditions
 -- are met:
 -- 
 -- 1. A Spine Essential, Professional, Enterprise, or Education License must
 --    be purchased from Esoteric Software and the license must remain valid:
 --    http://esotericsoftware.com/
 -- 2. Redistributions of source code must retain this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer.
 -- 3. Redistributions in binary form must reproduce this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer, in the documentation and/or other materials provided with the
 --    distribution.
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

local SkeletonData = require "spine-lua.SkeletonData"
local BoneData = require "spine-lua.BoneData"
local SlotData = require "spine-lua.SlotData"
local Skin = require "spine-lua.Skin"
local AttachmentLoader = require "spine-lua.AttachmentLoader"
local Animation = require "spine-lua.Animation"
local EventData = require "spine-lua.EventData"
local Event = require "spine-lua.Event"
local AttachmentType = require "spine-lua.AttachmentType"

local TIMELINE_SCALE = "scale"
local TIMELINE_ROTATE = "rotate"
local TIMELINE_TRANSLATE = "translate"
local TIMELINE_ATTACHMENT = "attachment"
local TIMELINE_COLOR = "color"

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

	function self:readSkeletonData (jsonText)
		local skeletonData = SkeletonData.new(self.attachmentLoader)

		local root = spine.utils.readJSON(jsonText)
		if not root then error("Invalid JSON: " .. jsonText, 2) end

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
			boneData.scaleX = (boneMap["scaleX"] or 1)
			boneData.scaleY = (boneMap["scaleY"] or 1)
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
				slotData.additiveBlending = slotMap["additive"]

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
						local attachment = readAttachment(attachmentName, attachmentMap, self.scale)
						if attachment then
							skin:addAttachment(slotIndex, attachmentName, attachment)
						end
					end
				end
				if skin.name == "default" then
					skeletonData.defaultSkin = skin
				else
					table.insert(skeletonData.skins, skin)
				end
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

	readAttachment = function (name, map, scale)
		name = map["name"] or name
		local attachment
		local type = AttachmentType[map["type"] or "region"]
		attachment = attachmentLoader:newAttachment(type, name)
		if not attachment then return nil end

		if type == AttachmentType.region then
			attachment.x = (map["x"] or 0) * scale
			attachment.y = (map["y"] or 0) * scale
			attachment.scaleX = (map["scaleX"] or 1)
			attachment.scaleY = (map["scaleY"] or 1)
			attachment.rotation = (map["rotation"] or 0)
			attachment.width = map["width"] * scale
			attachment.height = map["height"] * scale
		elseif type == AttachmentType.boundingbox then
			local vertices = map["vertices"]
			for i,point in ipairs(vertices) do
				table.insert(attachment.vertices, vertices[i] * scale)
			end
		end

		return attachment
	end

	readAnimation = function (name, map, skeletonData)
		local timelines = {}
		local duration = 0

		local bonesMap = map["bones"]
		if bonesMap then
			for boneName,timelineMap in pairs(bonesMap) do
				local boneIndex = skeletonData:findBoneIndex(boneName)
				if boneIndex == -1 then error("Bone not found: " .. boneName) end

				for timelineName,values in pairs(timelineMap) do
					if timelineName == TIMELINE_ROTATE then
						local timeline = Animation.RotateTimeline.new()
						timeline.boneIndex = boneIndex

						local keyframeIndex = 0
						for i,valueMap in ipairs(values) do
							local time = valueMap["time"]
							timeline:setFrame(keyframeIndex, time, valueMap["angle"])
							readCurve(timeline, keyframeIndex, valueMap)
							keyframeIndex = keyframeIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					elseif timelineName == TIMELINE_TRANSLATE or timelineName == TIMELINE_SCALE then
						local timeline
						local timelineScale = 1
						if timelineName == TIMELINE_SCALE then
							timeline = Animation.ScaleTimeline.new()
						else
							timeline = Animation.TranslateTimeline.new()
							timelineScale = self.scale
						end
						timeline.boneIndex = boneIndex

						local keyframeIndex = 0
						for i,valueMap in ipairs(values) do
							local time = valueMap["time"]
							local x = (valueMap["x"] or 0) * timelineScale
							local y = (valueMap["y"] or 0) * timelineScale
							timeline:setFrame(keyframeIndex, time, x, y)
							readCurve(timeline, keyframeIndex, valueMap)
							keyframeIndex = keyframeIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					else
						error("Invalid timeline type for a bone: " .. timelineName .. " (" .. boneName .. ")")
					end
				end
			end
		end

		local slotsMap = map["slots"]
		if slotsMap then
			for slotName,timelineMap in pairs(slotsMap) do
				local slotIndex = skeletonData.slotNameIndices[slotName]

				for timelineName,values in pairs(timelineMap) do
					if timelineName == TIMELINE_COLOR then
						local timeline = Animation.ColorTimeline.new()
						timeline.slotIndex = slotIndex

						local keyframeIndex = 0
						for i,valueMap in ipairs(values) do
							local time = valueMap["time"]
							local color = valueMap["color"]
							timeline:setFrame(
								keyframeIndex, time, 
								tonumber(color:sub(1, 2), 16) / 255,
								tonumber(color:sub(3, 4), 16) / 255,
								tonumber(color:sub(5, 6), 16) / 255,
								tonumber(color:sub(7, 8), 16) / 255
							)
							readCurve(timeline, keyframeIndex, valueMap)
							keyframeIndex = keyframeIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					elseif timelineName == TIMELINE_ATTACHMENT then
						local timeline = Animation.AttachmentTimeline.new()
						timeline.slotName = slotName

						local frameIndex = 0
						for i,valueMap in ipairs(values) do
							local time = valueMap["time"]
							local attachmentName = valueMap["name"]
							if not attachmentName then attachmentName = nil end
							timeline:setFrame(frameIndex, time, attachmentName)
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

		local events = map["events"]
		if events then
			local timeline = Animation.EventTimeline.new(#events)
			local frameIndex = 0
			for i,eventMap in ipairs(events) do
				local eventData = skeletonData:findEvent(eventMap["name"])
				if not eventData then error("Event not found: " + eventMap["name"]) end
				local event = Event.new(eventData)
				event.intValue = eventMap["int"] or eventData.intValue
				event.floatValue = eventMap["float"] or eventData.floatValue
				event.stringValue = eventMap["string"] or eventData.stringValue
				timeline:setFrame(frameIndex, eventMap["time"], event)
				frameIndex = frameIndex + 1
			end
			table.insert(timelines, timeline)
			duration = math.max(duration, timeline:getDuration())
		end

		local drawOrderValues = map["draworder"]
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
						if slotIndex == -1 then error("Slot not found: " + offsetMap["slot"]) end
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

		table.insert(skeletonData.animations, Animation.new(name, timelines, duration))
	end

	readCurve = function (timeline, frameIndex, valueMap)
		local curve = valueMap["curve"]
		if not curve then return end
		if curve == "stepped" then
			timeline:setStepped(frameIndex)
		else
			timeline:setCurve(frameIndex, curve[1], curve[2], curve[3], curve[4])
		end
	end

	return self
end
return SkeletonJson
