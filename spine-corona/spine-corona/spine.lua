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

spine = {}

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
	self = spine.Skeleton.new_super(skeletonData)
	self.group = group or display.newGroup()
	self.drawingGroup = nil
	self.premultipliedAlpha = false
	self.batches = 0
	return self
end

local function colorEquals(color1, color2)
	if not color1 and not color2 then return true end
	if not color1 and color2 then return false end
	if color1 and not color2 then return false end
	return color1[1] == color2[1] and color1[2] == color2[2] and color1[3] == color2[3] and color1[4] == color2[4]
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

function spine.Skeleton:updateWorldTransform()
	spine.Skeleton.updateWorldTransform_super(self)
	local premultipliedAlpha = self.premultipliedAlpha

	self.batches = 0

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
	local color = nil
	local lastColor = nil
	local texture = nil
	local lastTexture = nil
	local blendMode = nil
	local lastBlendMode = nil
	for i,slot in ipairs(drawOrder) do
		local attachment = slot.attachment
		local vertices = nil
		local indices = nil
		if attachment then
			if attachment.type == spine.AttachmentType.region then
				vertices = attachment:updateWorldVertices(slot, premultipliedAlpha)
				indices = QUAD_TRIANGLES
				texture = attachment.region.renderObject.texture
				color = { vertices[5], vertices[6], vertices[7], vertices[8]}
				blendMode = toCoronaBlendMode(slot.data.blendMode)
			elseif attachment.type == spine.AttachmentType.mesh then
				vertices = attachment:updateWorldVertices(slot, premultipliedAlpha)
				indices = attachment.triangles
				texture = attachment.region.renderObject.texture
				color = { vertices[5], vertices[6], vertices[7], vertices[8] }
				blendMode = toCoronaBlendMode(slot.data.blendMode)
			end

			if texture and vertices and indices then
				if not lastTexture then lastTexture = texture end
				if not lastColor then lastColor = color end
				if not lastBlendMode then lastBlendMode = blendMode end

				if (texture ~= lastTexture or not colorEquals(color, lastColor) or blendMode ~= lastBlendMode) then
					self:flush(groupVertices, groupUvs, groupIndices, lastTexture, lastColor, lastBlendMode, drawingGroup)
					lastTexture = texture
					lastColor = color
					lastBlendMode = blendMode
					groupVertices = {}
					groupUvs = {}
					groupIndices = {}
				end

				self:batch(vertices, indices, groupVertices, groupUvs, groupIndices)
			end
		end
	end

	if #groupVertices > 0 then
		self:flush(groupVertices, groupUvs, groupIndices, texture, color, blendMode, drawingGroup)
	end
end

function spine.Skeleton:flush(groupVertices, groupUvs, groupIndices, texture, color, blendMode, drawingGroup)
	mesh = display.newMesh(drawingGroup, 0, 0, {
			mode = "indexed",
			vertices = groupVertices,
			uvs = groupUvs,
			indices = groupIndices
	})
	mesh.fill = texture
	mesh:setFillColor(color[1], color[2], color[3])
	mesh.alpha = color[4]
	mesh.blendMode = blendMode
	mesh:translate(mesh.path:getVertexOffset())
	self.batches = self.batches + 1
end

function spine.Skeleton:batch(vertices, indices, groupVertices, groupUvs, groupIndices)
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
	local numVertices = #vertices
	local vertexStart = #groupVertices + 1
	local vertexEnd = vertexStart + numVertices / 4
	while vertexStart < vertexEnd do
		groupVertices[vertexStart] = vertices[i]
		groupVertices[vertexStart+1] = vertices[i+1]
		groupUvs[vertexStart] = vertices[i+2]
		groupUvs[vertexStart+1] = vertices[i+3]
		vertexStart = vertexStart + 2
		i = i + 8
	end
end

return spine
