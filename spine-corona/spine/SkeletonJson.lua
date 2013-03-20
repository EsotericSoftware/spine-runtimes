###############################################################################
 # Copyright (c) 2013, Esoteric Software
 # All rights reserved.
 # 
 # Redistribution and use in source and binary forms, with or without
 # modification, are permitted provided that the following conditions are met:
 # 
 # 1. Redistributions of source code must retain the above copyright notice, this
 #    list of conditions and the following disclaimer.
 # 2. Redistributions in binary form must reproduce the above copyright notice,
 #    this list of conditions and the following disclaimer in the documentation
 #    and/or other materials provided with the distribution.
 # 
 # THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 # ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 # WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 # DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 # ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 # (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 # LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 # ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 # (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 # SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ##############################################################################

local utils = require "spine.utils"
local SkeletonData = require "spine.SkeletonData"
local BoneData = require "spine.BoneData"
local SlotData = require "spine.SlotData"
local Skin = require "spine.Skin"
local RegionAttachment = require "spine.RegionAttachment"
local AttachmentResolver = require "spine.AttachmentResolver"
local Animation = require "spine.Animation"
local json = require "json"

local TIMELINE_SCALE = "scale"
local TIMELINE_ROTATE = "rotate"
local TIMELINE_TRANSLATE = "translate"
local TIMELINE_ATTACHMENT = "attachment"
local TIMELINE_COLOR = "color"

local ATTACHMENT_REGION = "region"
local ATTACHMENT_ANIMATED_REGION = "animatedRegion"

local SkeletonJson = {}
function SkeletonJson.new (attachmentResolver)
	if not attachmentResolver then attachmentResolver = AttachmentResolver.new() end

	local self = {
		attachmentResolver = attachmentResolver,
		scale = 1
	}

	function self:readSkeletonDataFile (fileName, base)
		return self:readSkeletonData(utils.readFile(fileName, base))
	end

	local readAttachment

	function self:readSkeletonData (jsonText)
		local skeletonData = SkeletonData.new(self.attachmentResolver)

		local root = json.decode(jsonText)
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
						tonumber(color:sub(1, 2), 16),
						tonumber(color:sub(3, 4), 16),
						tonumber(color:sub(5, 6), 16),
						tonumber(color:sub(7, 8), 16)
					)
				end

				slotData.attachmentName = slotMap["attachment"]

				table.insert(skeletonData.slots, slotData)
			end
		end

		-- Skins.
		map = root["skins"]
		if map then
			for skinName,skinMap in pairs(map) do
				local skin = Skin.new(skinName)
				for slotName,slotMap in pairs(skinMap) do
					local slotIndex = skeletonData:findSlotIndex(slotName)
					for attachmentName,attachmentMap in pairs(slotMap) do
						local attachment = readAttachment(attachmentName, attachmentMap, self.scale)
						skin:addAttachment(slotIndex, attachmentName, attachment)
					end
				end
				if skin.name == "default" then
					skeletonData.defaultSkin = skin
				else
					table.insert(skeletonData.skins, skin)
				end
			end
		end

		return skeletonData
	end

	readAttachment = function (name, map, scale)
		name = map["name"] or name
		local attachment
		local type = map["type"] or ATTACHMENT_REGION
		if type == ATTACHMENT_REGION then
			attachment = RegionAttachment.new(name)
		else
			error("Unknown attachment type: " .. type .. " (" + name + ")")
		end

		attachment.x = (map["x"] or 0) * scale
		attachment.y = (map["y"] or 0) * scale
		attachment.scaleX = (map["scaleX"] or 1)
		attachment.scaleY = (map["scaleY"] or 1)
		attachment.rotation = (map["rotation"] or 0)
		attachment.width = map["width"] * scale
		attachment.height = map["height"] * scale
		return attachment
	end

	function self:readAnimationFile (skeletonData, fileName, base)
		return self:readAnimation(skeletonData, utils.readFile(fileName, base))
	end

	local readCurve
	
	function self:readAnimation (skeletonData, jsonText)
		local timelines = {}
		local duration = 0

		local root = json.decode(jsonText)
		if not root then error("Invalid JSON: " .. jsonText, 2) end

		local bonesMap = root["bones"]
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
						timeline:setKeyframe(keyframeIndex, time, valueMap["angle"])
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
						timeline:setKeyframe(keyframeIndex, time, x, y)
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

		local slotsMap = root["slots"]
		if slotsMap then
			for slotName,timelineMap in pairs(slotsMap) do
				local slotIndex = skeletonData:findSlotIndex(slotName)

				for timelineName,values in pairs(timelineMap) do
					if timelineName == TIMELINE_COLOR then
						local timeline = Animation.ColorTimeline.new()
						timeline.slotIndex = slotIndex

						local keyframeIndex = 0
						for i,valueMap in ipairs(values) do
							local time = valueMap["time"]
							local color = valueMap["color"]
							timeline:setKeyframe(
								keyframeIndex, time, 
								tonumber(color:sub(1, 2), 16),
								tonumber(color:sub(3, 4), 16),
								tonumber(color:sub(5, 6), 16),
								tonumber(color:sub(7, 8), 16)
							)
							readCurve(timeline, keyframeIndex, valueMap)
							keyframeIndex = keyframeIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					elseif timelineName == TIMELINE_ATTACHMENT then
						local timeline = Animation.AttachmentTimeline.new()
						timeline.slotName = slotName

						local keyframeIndex = 0
						for i,valueMap in ipairs(values) do
							local time = valueMap["time"]
							local attachmentName = valueMap["name"]
							if attachmentName == json.null then attachmentName = nil end
							timeline:setKeyframe(keyframeIndex, time, attachmentName)
							keyframeIndex = keyframeIndex + 1
						end
						table.insert(timelines, timeline)
						duration = math.max(duration, timeline:getDuration())

					else
						error("Invalid frame type for a slot: " .. timelineName .. " (" .. slotName .. ")")
					end
				end
			end
		end

		return Animation.new(timelines, duration)
	end

	readCurve = function (timeline, keyframeIndex, valueMap)
		local curve = valueMap["curve"]
		if not curve then return end
		if curve == "stepped" then
			timeline:setStepped(keyframeIndex)
		else
			timeline:setCurve(keyframeIndex, curve[1], curve[2], curve[3], curve[4])
		end
	end

	return self
end
return SkeletonJson
