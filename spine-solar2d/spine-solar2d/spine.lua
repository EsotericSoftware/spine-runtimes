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

local spine = {}

spine.utils = require "spine-lua.utils"
spine.SkeletonJson = require "spine-lua.SkeletonJson"
spine.SkeletonData = require "spine-lua.SkeletonData"
spine.BoneData = require "spine-lua.BoneData"
spine.SlotData = require "spine-lua.SlotData"
spine.IkConstraintData = require "spine-lua.IkConstraintData"
spine.Skin = require "spine-lua.Skin"
spine.Attachment = require "spine-lua.attachments.Attachment"
spine.BoundingBoxAttachment = require "spine-lua.attachments.BoundingBoxAttachment"
spine.RegionAttachment = require "spine-lua.attachments.RegionAttachment"
spine.MeshAttachment = require "spine-lua.attachments.MeshAttachment"
spine.VertexAttachment = require "spine-lua.attachments.VertexAttachment"
spine.PathAttachment = require "spine-lua.attachments.PathAttachment"
spine.PointAttachment = require "spine-lua.attachments.PointAttachment"
spine.ClippingAttachment = require "spine-lua.attachments.ClippingAttachment"
spine.Skeleton = require "spine-lua.Skeleton"
spine.Bone = require "spine-lua.Bone"
spine.Slot = require "spine-lua.Slot"
spine.IkConstraint = require "spine-lua.IkConstraint"
spine.AttachmentType = require "spine-lua.attachments.AttachmentType"
spine.AttachmentLoader = require "spine-lua.AttachmentLoader"
spine.Animation = require "spine-lua.Animation"
spine.AnimationStateData = require "spine-lua.AnimationStateData"
spine.AnimationState = require "spine-lua.AnimationState"
spine.EventData = require "spine-lua.EventData"
spine.Event = require "spine-lua.Event"
spine.SkeletonBounds = require "spine-lua.SkeletonBounds"
spine.BlendMode = require "spine-lua.BlendMode"
spine.TextureAtlas = require "spine-lua.TextureAtlas"
spine.TextureRegion = require "spine-lua.TextureRegion"
spine.TextureAtlasRegion = require "spine-lua.TextureAtlasRegion"
spine.AtlasAttachmentLoader = require "spine-lua.AtlasAttachmentLoader"
spine.Color = require "spine-lua.Color"
spine.Triangulator = require "spine-lua.Triangulator"
spine.SkeletonClipping = require "spine-lua.SkeletonClipping"
spine.JitterEffect = require "spine-lua.vertexeffects.JitterEffect"
spine.SwirlEffect = require "spine-lua.vertexeffects.SwirlEffect"
spine.Interpolation = require "spine-lua.Interpolation"

spine.utils.readFile = function (fileName, base)
	if not base then base = system.ResourceDirectory end
	local path = system.pathForFile(fileName, base)
	local file = io.open(path, "r")
	if not file then return nil end
	local contents = file:read("*a")
	io.close(file)
	return contents
end

local json = require "json"
spine.utils.readJSON = function (text)
	return json.decode(text)
end

local QUAD_TRIANGLES = { 1, 2, 3, 3, 4, 1 }
spine.Skeleton.new_super = spine.Skeleton.new
spine.Skeleton.updateWorldTransform_super = spine.Skeleton.updateWorldTransform
spine.Skeleton.new = function(skeletonData, group)
	local self = spine.Skeleton.new_super(skeletonData)
	self.group = group or display.newGroup()
	self.drawingGroup = nil
	self.premultipliedAlpha = false
	self.batches = 0
	self.tempColor = spine.Color.newWith(1, 1, 1, 1)
	self.tempColor2 = spine.Color.newWith(-1, 1, 1, 1)
	self.tempVertex = {
		x = 0,
		y = 0,
		u = 0,
		v = 0,
		light = spine.Color.newWith(1, 1, 1, 1),
		dark = spine.Color.newWith(0, 0, 0, 0)
	}
	self.clipper = spine.SkeletonClipping.new()
	return self
end

local function colorEquals(color1, color2)
	return color1.r == color2.r and color1.g == color2.g and color1.b == color2.b and color1.a == color2.a
end

local function toCoronaBlendMode(blendMode)
	if blendMode == spine.BlendMode.normal then
		return "normal"
	elseif blendMode == spine.BlendMode.additive then
		return "add"
	elseif blendMode == spine.BlendMode.multiply then
		return "multiply"
	elseif blendMode == spine.BlendMode.screen then
		return "screen"
	end
end

local worldVertices = spine.utils.newNumberArray(10000 * 8)

function spine.Skeleton:updateWorldTransform()
	spine.Skeleton.updateWorldTransform_super(self)
	local premultipliedAlpha = self.premultipliedAlpha

	self.batches = 0

	if (self.vertexEffect) then self.vertexEffect:beginEffect(self) end

	-- Remove old drawing group, we will start anew
	if self.drawingGroup then self.drawingGroup:removeSelf() end
	local drawingGroup = display.newGroup()
	self.drawingGroup = drawingGroup
	self.group:insert(drawingGroup)

	local drawOrder = self.drawOrder
	local currentGroup = nil
	local groupVertices = {}
	local groupIndices = {}
	local groupUvs = {}
	local color = self.tempColor
	local lastColor = self.tempColor2
	lastColor.r = -1
	local texture = nil
	local lastTexture = nil
	local blendMode = nil
	local lastBlendMode = nil
	local renderable = {
		vertices = nil,
		uvs = nil
	}

	for _,slot in ipairs(drawOrder) do
		local attachment = slot.attachment
		local vertices = nil
		local uvs = nil
		local numVertices = 0
		local indices = nil

		if slot.bone.active then
			local isClippingAttachment = false
			if attachment then
				if attachment.type == spine.AttachmentType.region then
					numVertices = 4
					vertices = worldVertices
					attachment:computeWorldVertices(slot.bone, vertices, 0, 2)
					uvs = attachment.uvs
					indices = QUAD_TRIANGLES
					texture = attachment.region.renderObject.texture
					blendMode = toCoronaBlendMode(slot.data.blendMode)
				elseif attachment.type == spine.AttachmentType.mesh then
					numVertices = attachment.worldVerticesLength / 2
					vertices = worldVertices
					attachment:computeWorldVertices(slot, 0, attachment.worldVerticesLength, vertices, 0, 2)
					uvs = attachment.uvs
					indices = attachment.triangles
					texture = attachment.region.renderObject.texture
					blendMode = toCoronaBlendMode(slot.data.blendMode)
				elseif attachment.type == spine.AttachmentType.clipping then
					self.clipper:clipStart(slot, attachment)
					isClippingAttachment = true
				end

				if texture and vertices and indices then
					local skeleton = slot.bone.skeleton
					local skeletonColor = skeleton.color
					local slotColor = slot.color
					local attachmentColor = attachment.color
					local alpha = skeletonColor.a * slotColor.a * attachmentColor.a
					local multiplier = 1
					if premultipliedAlpha then multiplier = alpha end

					color:set(skeletonColor.r * slotColor.r * attachmentColor.r * multiplier,
							skeletonColor.g * slotColor.g * attachmentColor.g * multiplier,
							skeletonColor.b * slotColor.b * attachmentColor.b * multiplier,
							alpha)

					if not lastTexture then lastTexture = texture end
					if lastColor.r == -1 then lastColor:setFrom(color) end
					if not lastBlendMode then lastBlendMode = blendMode end

					if (texture ~= lastTexture or not colorEquals(color, lastColor) or blendMode ~= lastBlendMode) then
						self:flush(groupVertices, groupUvs, groupIndices, lastTexture, lastColor, lastBlendMode, drawingGroup)
						lastTexture = texture
						lastColor:setFrom(color)
						lastBlendMode = blendMode
						groupVertices = {}
						groupUvs = {}
						groupIndices = {}
					end

					if self.clipper:isClipping() then
						self.clipper:clipTriangles(vertices, uvs, indices, #indices)
						vertices = self.clipper.clippedVertices
						numVertices = #vertices / 2
						uvs = self.clipper.clippedUVs
						indices = self.clipper.clippedTriangles
					end

					self:batch(vertices, uvs, numVertices, indices, groupVertices, groupUvs, groupIndices)

				end
				if not isClippingAttachment then self.clipper:clipEnd(slot) end
			end
		end
	end

	if #groupVertices > 0 then
		self:flush(groupVertices, groupUvs, groupIndices, texture, color, blendMode, drawingGroup)
	end

	self.clipper:clipEnd2()
	if (self.vertexEffect) then self.vertexEffect:endEffect() end
end

function spine.Skeleton:flush(groupVertices, groupUvs, groupIndices, texture, color, blendMode, drawingGroup)
	local mesh = display.newMesh(drawingGroup, 0, 0, {
		mode = "indexed",
		vertices = groupVertices,
		uvs = groupUvs,
		indices = groupIndices
	})
	mesh.fill = texture
	mesh:setFillColor(color.r, color.g, color.b)
	mesh.alpha = color.a
	mesh.blendMode = blendMode
	mesh:translate(mesh.path:getVertexOffset())
	self.batches = self.batches + 1
end

function spine.Skeleton:batch(vertices, uvs, numVertices, indices, groupVertices, groupUvs, groupIndices)
	local numIndices = #indices
	local i = 1
	local indexStart = #groupIndices + 1
	local offset = #groupVertices / 2
	local indexEnd = indexStart + numIndices

	while indexStart < indexEnd do
		groupIndices[indexStart] = indices[i] + offset
		indexStart = indexStart + 1
		i = i + 1
	end

	i = 1
	local vertexStart = #groupVertices + 1
	local vertexEnd = vertexStart + numVertices * 2
	if (self.vertexEffect) then
		local effect = self.vertexEffect
		local vertex = self.tempVertex
		while vertexStart < vertexEnd do
			vertex.x = vertices[i]
			vertex.y = vertices[i+1]
			vertex.u = uvs[i]
			vertex.v = uvs[i+1]
			effect:transform(vertex);
			groupVertices[vertexStart] = vertex.x
			groupVertices[vertexStart+1] = vertex.y
			groupUvs[vertexStart] = vertex.u
			groupUvs[vertexStart+1] = vertex.v
			vertexStart = vertexStart + 2
			i = i + 2
		end
	else
		while vertexStart < vertexEnd do
			groupVertices[vertexStart] = vertices[i]
			groupVertices[vertexStart+1] = vertices[i+1]
			groupUvs[vertexStart] = uvs[i]
			groupUvs[vertexStart+1] = uvs[i+1]
			vertexStart = vertexStart + 2
			i = i + 2
		end
	end
end

return spine
