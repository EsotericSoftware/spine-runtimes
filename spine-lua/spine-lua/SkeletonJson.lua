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

local math_max = math.max
local math_floor = math.floor

local SkeletonJson = {}
function SkeletonJson.new (attachmentLoader)
	if not attachmentLoader then attachmentLoader = AttachmentLoader.new() end

	local self = {
		attachmentLoader = attachmentLoader,
		scale = 1,
		linkedMeshes = {}
	}

	function self:readSkeletonDataFile (fileName, base)
		return self:readSkeletonData(utils.readFile(fileName, base))
	end

	local readAttachment
	local readAnimation
	local readCurve
	local readTimeline1
	local readTimeline2
	local getArray

	local getValue = function (map, name, default)
		local value = map[name]
		if value == nil then return default else return value end
	end

	function self:readSkeletonData (jsonText)
		local scale = self.scale
		local skeletonData = SkeletonData.new(self.attachmentLoader)
		local root = utils.readJSON(jsonText)
		if not root then error("Invalid JSON: " .. jsonText, 2) end

		-- Skeleton.
		local skeletonMap = root["skeleton"]
		if skeletonMap then
			skeletonData.hash = skeletonMap["hash"]
			skeletonData.version = skeletonMap["spine"]
			skeletonData.x = skeletonMap["x"]
			skeletonData.y = skeletonMap["y"]
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
			data.length = getValue(boneMap, "length", 0) * scale
			data.x = getValue(boneMap, "x", 0) * scale
			data.y = getValue(boneMap, "y", 0) * scale
			data.rotation = getValue(boneMap, "rotation", 0)
			data.scaleX = getValue(boneMap, "scaleX", 1)
			data.scaleY = getValue(boneMap, "scaleY", 1)
			data.shearX = getValue(boneMap, "shearX", 0)
			data.shearY = getValue(boneMap, "shearY", 0)
			data.transformMode = TransformMode[getValue(boneMap, "transform", "normal")]
			data.skinRequired = getValue(boneMap, "skin", false)

			local color = boneMap["color"]
			if color then
				data.color = Color.newWith(tonumber(color:sub(1, 2), 16) / 255,
					tonumber(color:sub(3, 4), 16) / 255,
					tonumber(color:sub(5, 6), 16) / 255,
					tonumber(color:sub(7, 8), 16) / 255)
			end

			table_insert(skeletonData.bones, data)
		end

		-- Slots.
		if root["slots"] then
			for i,slotMap in ipairs(root["slots"]) do
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
					data.darkColor = Color.newWith(
						tonumber(dark:sub(1, 2), 16) / 255,
						tonumber(dark:sub(3, 4), 16) / 255,
						tonumber(dark:sub(5, 6), 16) / 255, 0)
				end

				data.attachmentName = getValue(slotMap, "attachment", nil)
				data.blendMode = BlendMode[getValue(slotMap, "blend", "normal")]

				table_insert(skeletonData.slots, data)
				skeletonData.nameToSlot[data.name] = data
			end
		end

		-- IK constraints.
		if root["ik"] then
			for _,constraintMap in ipairs(root["ik"]) do
				local data = IkConstraintData.new(constraintMap["name"])
				data.order = getValue(constraintMap, "order", 0)
				data.skinRequired = getValue(constraintMap, "skin", false)

				for _,boneName in ipairs(constraintMap["bones"]) do
					local bone = skeletonData:findBone(boneName)
					if not bone then error("IK bone not found: " .. boneName) end
					table_insert(data.bones, bone)
				end

				local targetName = constraintMap["target"]
				data.target = skeletonData:findBone(targetName)
				if not data.target then error("Target bone not found: " .. targetName) end

				data.mix = getValue(constraintMap, "mix", 1)
				data.softness = getValue(constraintMap, "softness", 0) * scale
				if constraintMap["bendPositive"] == nil or constraintMap["bendPositive"] == true then
					data.bendDirection = 1
				else
					data.bendDirection = -1
				end
				if constraintMap["compress"] == nil or constraintMap["compress"] == false then data.compress = false else data.compress = true end
				if constraintMap["stretch"] == nil	or constraintMap["stretch"] == false then data.stretch = false else data.stretch = true end
				if constraintMap["uniform"] == nil or	constraintMap["uniform"] == false then data.uniform = false else data.uniform = true end

				table_insert(skeletonData.ikConstraints, data)
			end
		end

		-- Transform constraints
		if root["transform"] then
			for _,constraintMap in ipairs(root["transform"]) do
				local data = TransformConstraintData.new(constraintMap.name)
				data.order = getValue(constraintMap, "order", 0)
				data.skinRequired = getValue(constraintMap, "skin", false)

				for _,boneName in ipairs(constraintMap.bones) do
					local bone = skeletonData:findBone(boneName)
					if not bone then error("Transform constraint bone not found: " .. boneName) end
					table_insert(data.bones, bone)
				end

				local targetName = constraintMap.target
				data.target = skeletonData:findBone(targetName)
				if not data.target then error("Transform constraint target bone not found: " .. (targetName or "none")) end

				data.local_ = getValue(constraintMap, "local", false)
				data.relative = getValue(constraintMap, "relative", false)
				data.offsetRotation = getValue(constraintMap, "rotation", 0)
				data.offsetX = getValue(constraintMap, "x", 0) * scale
				data.offsetY = getValue(constraintMap, "y", 0) * scale
				data.offsetScaleX = getValue(constraintMap, "scaleX", 0)
				data.offsetScaleY = getValue(constraintMap, "scaleY", 0)
				data.offsetShearY = getValue(constraintMap, "shearY", 0)

				data.mixRotate = getValue(constraintMap, "rotateMix", 1)
				data.mixX = getValue(constraintMap, "mixX", 1)
				data.mixY = getValue(constraintMap, "mixY", data.mixX)
				data.mixScaleX = getValue(constraintMap, "mixScaleX", 1)
				data.mixScaleY = getValue(constraintMap, "mixScaleY", data.mixScaleX)
				data.mixShearY = getValue(constraintMap, "mixShearY", 1)

				table_insert(skeletonData.transformConstraints, data)
			end
		end

		-- Path constraints
		if root["path"] then
			for _,constraintMap in ipairs(root["path"]) do
				local data = PathConstraintData.new(constraintMap.name)
				data.order = getValue(constraintMap, "order", 0)
				data.skinRequired = getValue(constraintMap, "skin", false)

				for _,boneName in ipairs(constraintMap.bones) do
					local bone = skeletonData:findBone(boneName)
					if not bone then error("Path constraint bone not found: " .. boneName) end
					table_insert(data.bones, bone)
				end

				local targetName = constraintMap.target
				data.target = skeletonData:findSlot(targetName)
				if data.target == nil then error("Path target slot not found: " .. targetName) end

				data.positionMode = PathConstraintData.PositionMode[getValue(constraintMap, "positionMode", "percent"):lower()]
				data.spacingMode = PathConstraintData.SpacingMode[getValue(constraintMap, "spacingMode", "length"):lower()]
				data.rotateMode = PathConstraintData.RotateMode[getValue(constraintMap, "rotateMode", "tangent"):lower()]
				data.offsetRotation = getValue(constraintMap, "rotation", 0)
				data.position = getValue(constraintMap, "position", 0)
				if data.positionMode == PathConstraintData.PositionMode.fixed then data.position = data.position * scale end
				data.spacing = getValue(constraintMap, "spacing", 0)
				if data.spacingMode == PathConstraintData.SpacingMode.length or data.spacingMode == PathConstraintData.SpacingMode.fixed then data.spacing = data.spacing * scale end
				data.mixRotate = getValue(constraintMap, "mixRotate", 1)
				data.mixX = getValue(constraintMap, "mixX", 1)
				data.mixY = getValue(constraintMap, "mixY", data.mixX)

				table_insert(skeletonData.pathConstraints, data)
			end
		end

		-- Skins.
		if root["skins"] then
			for skinName,skinMap in pairs(root["skins"]) do
				local skin = Skin.new(skinMap["name"])
				
				if skinMap["bones"] then
					for _, entry in ipairs(skinMap["bones"]) do
						local bone = skeletonData:findBone(entry)
						if bone == nil then error("Skin bone not found:  " .. entry) end
						table_insert(skin.bones, bone)
					end
				end
				
				if skinMap["ik"] then
					for _, entry in ipairs(skinMap["ik"]) do
						local constraint = skeletonData:findIkConstraint(entry)
						if constraint == nil then error("Skin IK constraint not found:  " .. entry) end
						table_insert(skin.constraints, constraint)
					end
				end
				
				if skinMap["transform"] then
					for _, entry in ipairs(skinMap["transform"]) do
						local constraint = skeletonData:findTransformConstraint(entry)
						if constraint == nil then error("Skin transform constraint not found:  " .. entry) end
						table_insert(skin.constraints, constraint)
					end
				end
				
				if skinMap["path"] then
					for _, entry in ipairs(skinMap["path"]) do
						local constraint = skeletonData:findPathConstraint(entry)
						if constraint == nil then error("Skin path constraint not found:  " .. entry) end
						table_insert(skin.constraints, constraint)
					end
				end
				
				for slotName,slotMap in pairs(skinMap.attachments) do
					local slotIndex = skeletonData:findSlot(slotName).index
					for attachmentName,attachmentMap in pairs(slotMap) do
						local attachment = readAttachment(attachmentMap, skin, slotIndex, attachmentName, skeletonData)
						if attachment then
							skin:setAttachment(slotIndex, attachmentName, attachment)
						end
					end
				end
				table_insert(skeletonData.skins, skin)
				if skin.name == "default" then skeletonData.defaultSkin = skin end
			end
		end

		-- Linked meshes
		for _, linkedMesh in ipairs(self.linkedMeshes) do
			local skin = skeletonData.defaultSkin
			if linkedMesh.skin then skin = skeletonData:findSkin(linkedMesh.skin) end
			if not skin then error("Skin not found: " .. linkedMesh.skin) end
			local parent = skin:getAttachment(linkedMesh.slotIndex, linkedMesh.parent)
			if not parent then error("Parent mesh not found: " + linkedMesh.parent) end
			if linkedMesh.inheritDeform then
				linkedMesh.mesh.deformAttachment = parent
			else
				linkedMesh.mesh.deformAttachment = linkedMesh.mesh
			end

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
				data.audioPath = getValue(eventMap, "audio", nil)
				if data.audioPath ~= nil then
					data.volume = getValue(eventMap, "volume", 1)
					data.balance = getValue(eventMap, "balance", 0)
				end
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
			region.scaleX = getValue(map, "scaleX", 1)
			region.scaleY = getValue(map, "scaleY", 1)
			region.rotation = getValue(map, "rotation", 0)
			region.width = map.width * scale
			region.height = map.height * scale

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
			if not mesh then return nil end
			mesh.path = path

			local color = map.color
			if color then
				mesh.color:set(tonumber(color:sub(1, 2), 16) / 255,
					tonumber(color:sub(3, 4), 16) / 255,
					tonumber(color:sub(5, 6), 16) / 255,
					tonumber(color:sub(7, 8), 16) / 255)
			end

			mesh.width = getValue(map, "width", 0) * scale
			mesh.height = getValue(map, "height", 0) * scale

			local parent = map.parent
			if parent then
				table_insert(self.linkedMeshes, {
					mesh = mesh,
					skin = getValue(map, "skin", nil),
					slotIndex = slotIndex,
					parent = parent,
					inheritDeform = getValue(map, "deform", true)
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
			return path

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
		local scale = self.scale

		-- Slot timelines.
		local slotsMap = map["slots"]
		if slotsMap then
			for slotName,slotMap in pairs(slotsMap) do
				local slotIndex = skeletonData:findSlot(slotName).index
				for timelineName,timelineMap in pairs(slotMap) do
					if not timelineMap then
					elseif timelineName == "attachment" then
						local timeline = Animation.AttachmentTimeline.new(#timelineMap, slotIndex)
						for i,keyMap in ipairs(timelineMap) do
							timeline:setFrame(i + 1, getValue(keyMap, "time", 0), keyMap["name"])
						end
						table_insert(timelines, timeline)
					elseif timelineName == "rgba" then
						local timeline = Animation.RGBATimeline.new(#timelineMap, #timelineMap * 4, slotIndex)
						local keyMap = timelineMap[1]
						local time = getValue(keyMap, "time", 0)
						local color = keyMap["color"]
						local r = tonumber(color:sub(1, 2), 16) / 255
						local g = tonumber(color:sub(3, 4), 16) / 255
						local b = tonumber(color:sub(5, 6), 16) / 255
						local a = tonumber(color:sub(7, 8), 16) / 255
						local bezier = 0
						for i,keyMap in ipairs(timelineMap) do
							local frame = i - 1
							timeline:setFrame(frame, time, r, g, b, a)
							local nextMap = timelineMap[i + 1]
							if not nextMap then
								timeline:shrink(bezier)
								break
							end
							local time2 = getValue(nextMap, "time", 0)
							color = nextMap["color"]
							local nr = tonumber(color:sub(1, 2), 16) / 255
							local ng = tonumber(color:sub(3, 4), 16) / 255
							local nb = tonumber(color:sub(5, 6), 16) / 255
							local na = tonumber(color:sub(7, 8), 16) / 255
							local curve = keyMap.curve
							if curve then
								bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, a, na, 1)
							end
							time = time2
							r = nr
							g = ng
							b = nb
							a = na
						end
						table_insert(timelines, timeline)
					elseif timelineName == "rgb" then
						local timeline = Animation.RGBTimeline.new(#timelineMap, #timelineMap * 3, slotIndex)
						local keyMap = timelineMap[1]
						local time = getValue(keyMap, "time", 0)
						local color = keyMap["color"]
						local r = tonumber(color:sub(1, 2), 16) / 255
						local g = tonumber(color:sub(3, 4), 16) / 255
						local b = tonumber(color:sub(5, 6), 16) / 255
						local bezier = 0
						for i,keyMap in ipairs(timelineMap) do
							local frame = i - 1
							timeline:setFrame(frame, time, r, g, b)
							local nextMap = timelineMap[i + 1]
							if not nextMap then
								timeline:shrink(bezier)
								break
							end
							local time2 = getValue(nextMap, "time", 0)
							color = nextMap["color"]
							local nr = tonumber(color:sub(1, 2), 16) / 255
							local ng = tonumber(color:sub(3, 4), 16) / 255
							local nb = tonumber(color:sub(5, 6), 16) / 255
							local curve = keyMap.curve
							if curve then
								bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1)
							end
							time = time2
							r = nr
							g = ng
							b = nb
						end
						table_insert(timelines, timeline)
					elseif timelineName == "alpha" then
						table_insert(timelines, readTimeline1(timelineMap, Animation.AlphaTimeline.new(#timelineMap, #timelineMap, slotIndex), 0, 1))
					elseif timelineName == "rgba2" then
						local timeline = Animation.RGBA2Timeline.new(#timelineMap, #timelineMap * 7, slotIndex)
						local keyMap = timelineMap[1]
						local time = getValue(keyMap, "time", 0)
						local color = keyMap["light"]
						local r = tonumber(color:sub(1, 2), 16) / 255
						local g = tonumber(color:sub(3, 4), 16) / 255
						local b = tonumber(color:sub(5, 6), 16) / 255
						local a = tonumber(color:sub(7, 8), 16) / 255
						color = keyMap["dark"]
						local r2 = tonumber(color:sub(1, 2), 16) / 255
						local g2 = tonumber(color:sub(3, 4), 16) / 255
						local b2 = tonumber(color:sub(5, 6), 16) / 255
						local bezier = 0
						for i,keyMap in ipairs(timelineMap) do
							local frame = i - 1
							timeline:setFrame(frame, time, r, g, b, a, r2, g2, b2)
							local nextMap = timelineMap[i + 1]
							if not nextMap then
								timeline:shrink(bezier)
								break
							end
							local time2 = getValue(nextMap, "time", 0)
							color = nextMap["light"]
							local nr = tonumber(color:sub(1, 2), 16) / 255
							local ng = tonumber(color:sub(3, 4), 16) / 255
							local nb = tonumber(color:sub(5, 6), 16) / 255
							local na = tonumber(color:sub(7, 8), 16) / 255
							color = nextMap["dark"]
							local nr2 = tonumber(color:sub(1, 2), 16) / 255
							local ng2 = tonumber(color:sub(3, 4), 16) / 255
							local nb2 = tonumber(color:sub(5, 6), 16) / 255
							local curve = keyMap.curve
							if curve then
								bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, a, na, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, r2, nr2, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, g2, ng2, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 6, time, time2, b2, nb2, 1)
							end
							time = time2
							r = nr
							g = ng
							b = nb
							a = na
							r2 = nr2
							g2 = ng2
							b2 = nb2
						end
						table_insert(timelines, timeline)
					elseif timelineName == "rgb2" then
						local timeline = Animation.RGB2Timeline.new(#timelineMap, #timelineMap * 6, slotIndex)
						local keyMap = timelineMap[1]
						local time = getValue(keyMap, "time", 0)
						local color = keyMap["light"]
						local r = tonumber(color:sub(1, 2), 16) / 255
						local g = tonumber(color:sub(3, 4), 16) / 255
						local b = tonumber(color:sub(5, 6), 16) / 255
						color = keyMap["dark"]
						local r2 = tonumber(color:sub(1, 2), 16) / 255
						local g2 = tonumber(color:sub(3, 4), 16) / 255
						local b2 = tonumber(color:sub(5, 6), 16) / 255
						local bezier = 0
						for i,keyMap in ipairs(timelineMap) do
							local frame = i - 1
							timeline:setFrame(frame, time, r, g, b, r2, g2, b2)
							local nextMap = timelineMap[i + 1]
							if not nextMap then
								timeline:shrink(bezier)
								break
							end
							local time2 = getValue(nextMap, "time", 0)
							color = nextMap["light"]
							local nr = tonumber(color:sub(1, 2), 16) / 255
							local ng = tonumber(color:sub(3, 4), 16) / 255
							local nb = tonumber(color:sub(5, 6), 16) / 255
							color = nextMap["dark"]
							local nr2 = tonumber(color:sub(1, 2), 16) / 255
							local ng2 = tonumber(color:sub(3, 4), 16) / 255
							local nb2 = tonumber(color:sub(5, 6), 16) / 255
							local curve = keyMap.curve
							if curve then
								bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, r2, nr2, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, g2, ng2, 1)
								bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, b2, nb2, 1)
							end
							time = time2
							r = nr
							g = ng
							b = nb
							r2 = nr2
							g2 = ng2
							b2 = nb2
						end
						table_insert(timelines, timeline)
					else
						error("Invalid timeline type for a slot: " .. timelineName .. " (" .. slotName .. ")")
					end
				end
			end
		end

		-- Bone timelines.
		local bonesMap = map["bones"]
		if bonesMap then
			for boneName,boneMap in pairs(bonesMap) do
				local boneIndex = skeletonData:findBoneIndex(boneName)
				if boneIndex == -1 then error("Bone not found: " .. boneName) end
				for timelineName,timelineMap in pairs(boneMap) do
					if not timelineMap then
					elseif timelineName == "rotate" then
						table_insert(timelines, readTimeline1(timelineMap, Animation.RotateTimeline.new(#timelineMap, #timelineMap, boneIndex), 0, 1))
					elseif timelineName == "translate" then
						local timeline = Animation.TranslateTimeline.new(#timelineMap, #timelineMap * 2, boneIndex)
						table_insert(timelines, readTimeline2(timelineMap, timeline, "x", "y", 0, scale))
					elseif timelineName == "translatex" then
						local timeline = Animation.TranslateXTimeline.new(#timelineMap, #timelineMap, boneIndex)
						table_insert(timelines, readTimeline1(timelineMap, timeline, 0, scale))
					elseif timelineName == "translatey" then
						local timeline = Animation.TranslateYTimeline.new(#timelineMap, #timelineMap, boneIndex)
						table_insert(timelines, readTimeline1(timelineMap, timeline, 0, scale))
					elseif timelineName == "scale" then
						local timeline = Animation.ScaleTimeline.new(#timelineMap, #timelineMap * 2, boneIndex)
						table_insert(timelines, readTimeline2(timelineMap, "x", "y", 1, 1))
					elseif timelineName == "scalex" then
						local timeline = Animation.ScaleXTimeline.new(#timelineMap, #timelineMap, boneIndex)
						table_insert(timelines, readTimeline1(timelineMap, timeline, 1, 1))
					elseif timelineName == "scaley" then
						local timeline = Animation.ScaleYTimeline.new(#timelineMap, #timelineMap, boneIndex)
						table_insert(timelines, readTimeline1(timelineMap, timeline, 1, 1))
					elseif timelineName == "shear" then
						local timeline = Animation.ShearTimeline.new(#timelineMap, #timelineMap * 2, boneIndex)
						table_insert(timelines, readTimeline2(timelineMap, "x", "y", 0, 1))
					elseif timelineName == "shearx" then
						local timeline = Animation.ShearXTimeline.new(#timelineMap, #timelineMap, boneIndex)
						table_insert(timelines, readTimeline1(timelineMap, timeline, 0, 1))
					elseif timelineName == "sheary" then
						local timeline = Animation.ShearYTimeline.new(#timelineMap, #timelineMap, boneIndex)
						table_insert(timelines, readTimeline1(timelineMap, timeline, 0, 1))
					else
						error("Invalid timeline type for a bone: " .. timelineName .. " (" .. boneName .. ")")
					end
				end
			end
		end

		-- IK timelines.
		local ik = map["ik"]
		if ik then
			for constraintName,timelineMap in pairs(ik) do
				local keyMap = timelineMap[1]
				if keyMap then
					local constraintIndex = -1
					for i,other in pairs(skeletonData.ikConstraints) do
						if other.name == constraintName then
							constraintIndex = i
							break
						end
					end
					local timeline = Animation.IkConstraintTimeline.new(#timelineMap, #timelineMap * 2, constraintIndex)
					local time = getValue(keyMap, "time", 0)
					local mix = getValue(keyMap, "mix", 1)
					local softness = getValue(keyMap, "softness", 0) * scale
					local bezier = 0
					for i,keyMap in ipairs(timelineMap) do
						local frame = i - 1
						local bendPositive = 1
						local compress = false
						local stretch = false
						if keyMap["bendPositive"] == false then bendPositive = -1 end
						if keyMap["compress"] ~= nil then compress = keyMap["compress"] end
						if keyMap["stretch"] ~= nil then stretch = keyMap["stretch"] end
						timeline:setFrame(frame, time, mix, softness, bendPositive, compress, stretch)
						local nextMap = timelineMap[i + 1]
						if not nextMap then
							timeline:shrink(bezier)
							break
						end
						local time2 = getValue(nextMap, "time", 0)
						color = nextMap["color"]
						local mix2 = getValue(nextMap, "mix", 1)
						local softness2 = getValue(nextMap, "softness", 0) * scale
						local curve = keyMap.curve
						if curve then
							bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mix, mix2, 1)
							bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, softness, softness2, scale)
						end
						time = time2
						mix = mix2
						softness = softness2
					end
				end
				table_insert(timelines, timeline)
			end
		end

		-- Transform constraint timelines.
		local transform = map["transform"]
		if transform then
			for constraintName, timelineMap in pairs(transform) do
				local keyMap = timelineMap[1]
				if keyMap then
					local constraintIndex = -1
					for i,other in pairs(skeletonData.transformConstraints) do
						if other.name == constraintName then
							constraintIndex = i
							break
						end
					end
					local timeline = Animation.TransformConstraintTimeline.new(#timelineMap, #timelineMap * 4, constraintIndex)
					local time = getValue(keyMap, "time", 0)
					local mixRotate = getValue(keyMap, "mixRotate", 0)
					local mixX = getValue(keyMap, "mixX", 1)
					local mixY = getValue(keyMap, "mixY", mixX)
					local mixScaleX = getValue(keyMap, "mixScaleX", 1)
					local mixScaleY = getValue(keyMap, "mixScaleY", mixScaleX)
					local mixShearY = getValue(keyMap, "mixShearY", 1)
					local bezier = 0
					for i,keyMap in ipairs(timelineMap) do
						local frame = i - 1
						timeline:setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY)
						local nextMap = timelineMap[frame + 1]
						if not nextMap then
							timeline:shrink(bezier)
							break
						end
						local time2 = getValue(nextMap, "time", 0)
						local mixRotate2 = getValue(nextMap, "mixRotate", 1)
						local mixX2 = getValue(nextMap, "mixX", 1)
						local mixY2 = getValue(nextMap, "mixY", mixX2)
						local mixScaleX2 = getValue(nextMap, "mixScaleX", 1)
						local mixScaleY2 = getValue(nextMap, "mixScaleY", mixScaleX2)
						local mixShearY2 = getValue(nextMap, "mixShearY", 1)
						local curve = keyMap.curve
						if curve then
							bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1)
							bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1)
							bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1)
							bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, mixScaleX, mixScaleX2, 1)
							bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, mixScaleY, mixScaleY2, 1)
							bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, mixShearY, mixShearY2, 1)
						end
						time = time2
						mixRotate = mixRotate2
						mixX = mixX2
						mixY = mixY2
						mixScaleX = mixScaleX2
						mixScaleY = mixScaleY2
						mixScaleX = mixScaleX2
					end
					table_insert(timelines, timeline)
				end
			end
		end

		-- Path constraint timelines.
		if map.path then
			for constraintName,constraintMap in pairs(map.path) do
				local constraint, constraintIndex = -1
				for i,other in pairs(skeletonData.transformConstraints) do
					if other.name == constraintName then
						constraintIndex = i
						constraint = other
						break
					end
				end
				for timelineName, timelineMap in pairs(constraintMap) do
					local keyMap = timelineMap[1]
					if keyMap then
						if timelineName == "position" then
							local timeline = Animation.PathConstraintPositionTimeline.new(#timelineMap, #timelineMap, constraintIndex)
							local timelineScale = 1
							if constraint.positionMode == PositionMode.fixed then timelineScale = scale end
							table_insert(timelines, readTimeline1(timelineMap, timeline, 0, timelineScale))
						elseif timelineName == "spacing" then
							local timeline = Animation.PathConstraintSpacingTimeline.new(#timelineMap, #timelineMap, constraintIndex)
							local timelineScale = 1
							if data.spacingMode == SpacingMode.Length or data.spacingMode == SpacingMode.Fixed then timelineScale = scale end
							table_insert(timelines, readTimeline1(timelineMap, timeline, 0, timelineScale))
						elseif timelineName == "mix" then
							local timeline = Animation.PathConstraintMixTimeline.new(#timelineMap, #timelineMap * 3, constraintIndex)
							local time = getValue(keyMap, "time", 0)
							local mixRotate = getValue(keyMap, "mixRotate", 1)
							local mixX = getValue(keyMap, "mixX", 1)
							local mixY = getValue(keyMap, "mixY", mixX)
							local bezier = 0
							for i,keyMap in ipairs(timelineMap) do
								local frame = i - 1
								timeline:setFrame(frame, time, mixRotate, mixX, mixY)
								local nextMap = timelineMap[frame + 1]
								if not nextMap then
									timeline:shrink(bezier)
									break
								end
								local time2 = getValue(nextMap, "time", 0)
								local mixRotate2 = getValue(nextMap, "mixRotate", 1)
								local mixX2 = getValue(nextMap, "mixX", 1)
								local mixY2 = getValue(nextMap, "mixY", mixX2)
								local curve = keyMap.curve
								if curve then
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1)
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1)
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1)
								end
								time = time2
								mixRotate = mixRotate2
								mixX = mixX2
								mixY = mixY2
								keyMap = nextMap
							end
							table_insert(timelines, timeline)
						end
					end
				end
			end
		end

		-- Deform timelines.
		if map.deform then
			for deformName, deformMap in pairs(map.deform) do
				local skin = skeletonData:findSkin(deformName)
				if not skin then error("Skin not found: " .. deformName) end
				for slotName,slotMap in pairs(deformMap) do
					local slotIndex = skeletonData:findSlot(slotName).index
					if slotIndex == -1 then error("Slot not found: " .. slotMap.name) end
					for timelineName,timelineMap in pairs(slotMap) do
						local keyMap = timelineMap[1]
						if keyMap then
							local attachment = skin:getAttachment(slotIndex, timelineName)
							if not attachment then error("Deform attachment not found: " .. timelineMap.name) end
							local weighted = attachment.bones ~= nil
							local vertices = attachment.vertices
							local deformLength = #vertices
							if weighted then deformLength = math_floor(deformLength / 3) * 2 end

							local timeline = Animation.DeformTimeline.new(#timelineMap, #timelineMap, slotIndex, attachment)
							local bezier = 0
							for i,keyMap in ipairs(timelineMap) do
								local deform = nil
								local verticesValue = getValue(keyMap, "vertices", nil)
								if verticesValue == nil then
									deform = vertices
									if weighted then deform = utils.newNumberArray(deformLength) end
								else
									deform = utils.newNumberArray(deformLength)
									local start = getValue(keyMap, "offset", 0) + 1
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
								local frame = i - 1
								timeline:setFrame(frame, time, mixRotate, mixX, mixY)
								local nextMap = timelineMap[frame + 1]
								if not nextMap then
									timeline:shrink(bezier)
									break
								end
								local time2 = getValue(nextMap, "time", 0)
								local curve = keyMap.curve
								if curve then bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, 0, 1, 1) end
								time = time2
							end
							table_insert(timelines, timeline)
						end
					end
				end
			end
		end

		-- Draw order timelines.
		if map["drawOrder"] then
			local timeline = Animation.DrawOrderTimeline.new(#map["drawOrder"])
			local slotCount = #skeletonData.slots
			local frame = 0
			for _,drawOrderMap in ipairs(map["drawOrder"]) do
				local drawOrder = nil
				local offsets = drawOrderMap["offsets"]
				if offsets then
					drawOrder = {}
					local unchanged = {}
					local originalIndex = 1
					local unchangedIndex = 1
					for _,offsetMap in ipairs(offsets) do
						local slotIndex = skeletonData:findSlot(offsetMap["slot"]).index
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
				timeline:setFrame(frame, getValue(drawOrderMap, "time", 0), drawOrder)
				frame = frame + 1
			end
			table_insert(timelines, timeline)
		end

		-- Event timelines.
		local events = map["events"]
		if events then
			local timeline = Animation.EventTimeline.new(#events)
			local frame = 0
			for _,eventMap in ipairs(events) do
				local eventData = skeletonData:findEvent(eventMap["name"])
				if not eventData then error("Event not found: " .. eventMap["name"]) end
				local event = Event.new(getValue(eventMap, "time", 0), eventData)
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
				if eventData.audioPath ~= nil then
					event.volume = getValue(eventMap, "volume", 1)
					event.balance = getValue(eventMap, "balance", 0)
				end
				timeline:setFrame(frame, event)
				frame = frame + 1
			end
			table_insert(timelines, timeline)
		end

		local duration = 0
		for _,timeline in ipairs(timelines) do
			duration = math_max(duration, timeline:getDuration())
		end
		table_insert(skeletonData.animations, Animation.new(name, timelines, duration))
	end

	readCurve = function (map, timeline, frame)
		local curve = map["curve"]
		if not curve then return end
		if curve == "stepped" then
			timeline:setStepped(frame)
		else
			timeline:setCurve(frame, getValue(map, "curve", 0), getValue(map, "c2", 0), getValue(map, "c3", 1), getValue(map, "c4", 1))
		end
	end

	readTimeline1 = function (keys, timeline, defaultValue, scale)
		local keyMap = keys[1]
		local time = getValue(keyMap, "time", 0)
		local value = getValue(keyMap, "value", defaultValue) * scale
		local bezier = 0
		for i,keyMap in ipairs(keys) do
			local frame = i - 1
			timeline:setFrame(frame, time, value)
			local nextMap = keys[frame + 1]
			if not nextMap then break end
			local time2 = getValue(nextMap, "time", 0)
			local value2 = getValue(nextMap, "value", defaultValue) * scale
			local curve = keyMap.curve
			if curve then bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value, value2, scale) end
			time = time2
			value = value2
		end
		timeline:shrink(bezier)
		return timeline
	end

	readTimeline2 = function (keys, timeline, name1, name2, defaultValue, scale)
		local keyMap = keys[1]
		local time = getValue(keyMap, "time", 0)
		local value1 = getValue(keyMap, name1, defaultValue) * scale
		local value2 = getValue(keyMap, name2, defaultValue) * scale
		local bezier = 0
		for i,keyMap in ipairs(keys) do
			local frame = i - 1
			timeline:setFrame(frame, time, value1, value2)
			local nextMap = keys[frame + 1]
			if not nextMap then break end
			local time2 = getValue(nextMap, "time", 0)
			local nvalue1 = getValue(nextMap, name1, defaultValue) * scale
			local nvalue2 = getValue(nextMap, name2, defaultValue) * scale
			local curve = keyMap.curve
			if curve then
				bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale)
				bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale)
			end
			time = time2
			value1 = nvalue1
			value2 = nvalue2
		end
		timeline:shrink(bezier)
		return timeline
	end

	readCurve = function (curve, timeline, bezier, frame, value, time1, time2, value1, value2, scale)
		if curve == "stepped" then
			if value ~= 0 then timeline.setStepped(frame) end
			return bezier
		end
		local i = value * 4
		local cx1 = curve[i]
		local cy1 = curve[i + 1] * scale
		local cx2 = curve[i + 2]
		local cy2 = curve[i + 3] * scale
		timeline.setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2)
		return bezier + 1
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
