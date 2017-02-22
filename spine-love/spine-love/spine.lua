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

local setmetatable = setmetatable

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
spine.PointAttachment = require "spine-lua.attachments.PointAttachment"
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
	local path = fileName
	if base then path = base .. '/' .. path end
	return love.filesystem.read(path)
end

local json = require "spine-love.dkjson"
spine.utils.readJSON = function (text)
	return json.decode(text)
end

local PolygonBatcher = {}
PolygonBatcher.__index = PolygonBatcher

function PolygonBatcher.new(vertexCount)
	local self = {
		mesh = love.graphics.newMesh(vertexCount, "triangles", "dynamic"),
		maxVerticesLength = vertexCount,
		maxIndicesLength = vertexCount * 3,
		verticesLength = 0,
		indicesLength = 0,
		lastTexture = nil,
		isDrawing = false,
		drawCalls = 0,
		vertex = { 0, 0, 0, 0, 0, 0, 0, 0 },
		indices = nil
	}

	local indices = {}
	local i = 1
	local maxIndicesLength = self.maxIndicesLength
	while i <= maxIndicesLength do
		indices[i] = 1
		i = i + 1
	end
	self.indices = indices;

	setmetatable(self, PolygonBatcher)

	return self
end

function PolygonBatcher:begin ()
	if self.isDrawing then error("PolygonBatcher is already drawing. Call PolygonBatcher:stop() before calling PolygonBatcher:begin().", 2) end
	self.lastTexture = nil
	self.isDrawing = true
	self.drawCalls = 0
end

function PolygonBatcher:draw (texture, vertices, numVertices, indices)
	local numIndices = #indices
	local mesh = self.mesh

	if texture ~= self.lastTexture then
		self:flush()
		self.lastTexture = texture
		mesh:setTexture(texture)
	elseif self.verticesLength + numVertices >= self.maxVerticesLength or self.indicesLength + numIndices > self.maxIndicesLength then
		self:flush()
	end

	local i = 1
	local indexStart = self.indicesLength + 1
	local offset = self.verticesLength
	local indexEnd = indexStart + numIndices
	local meshIndices = self.indices
	while indexStart < indexEnd do
		meshIndices[indexStart] = indices[i] + offset
		indexStart = indexStart + 1
		i = i + 1
	end
	self.indicesLength = self.indicesLength + numIndices

	i = 1
	local vertexStart = self.verticesLength + 1
	local vertexEnd = vertexStart + numVertices
	local vertex = self.vertex
	while vertexStart < vertexEnd do
		vertex[1] = vertices[i]
		vertex[2] = vertices[i+1]
		vertex[3] = vertices[i+2]
		vertex[4] = vertices[i+3]
		vertex[5] = vertices[i+4] * 255
		vertex[6] = vertices[i+5] * 255
		vertex[7] = vertices[i+6] * 255
		vertex[8] = vertices[i+7] * 255
		mesh:setVertex(vertexStart, vertex)
		vertexStart = vertexStart + 1
		i = i + 8
	end
	self.verticesLength = self.verticesLength + numVertices
end

function PolygonBatcher:flush ()
	if self.verticesLength == 0 then return end
	local mesh = self.mesh
	mesh:setVertexMap(self.indices)
	mesh:setDrawRange(1, self.indicesLength)
	love.graphics.draw(mesh, 0, 0)

	self.verticesLength = 0
	self.indicesLength = 0
	self.drawCalls = self.drawCalls + 1
end

function PolygonBatcher:stop ()
	if not self.isDrawing then error("PolygonBatcher is not drawing. Call PolygonBatcher:begin() first.", 2) end
	if self.verticesLength > 0 then self:flush() end

	self.lastTexture = nil
	self.isDrawing = false
end

local SkeletonRenderer = {}
SkeletonRenderer.__index = SkeletonRenderer
SkeletonRenderer.QUAD_TRIANGLES = { 1, 2, 3, 3, 4, 1 }

function SkeletonRenderer.new ()
	local self = {
		batcher = PolygonBatcher.new(3 * 500),
		premultipliedAlpha = false
	}

	setmetatable(self, SkeletonRenderer)
	return self
end

local worldVertices = spine.utils.newNumberArray(10000 * 8)
local tmpColor = spine.Color.newWith(0, 0, 0, 0)

function SkeletonRenderer:draw (skeleton)
	local batcher = self.batcher
	local premultipliedAlpha = self.premultipliedAlpha

	local lastLoveBlendMode = love.graphics.getBlendMode()
	love.graphics.setBlendMode("alpha")
	local lastBlendMode = spine.BlendMode.normal
	batcher:begin()

	local drawOrder = skeleton.drawOrder
	for i, slot in ipairs(drawOrder) do
		local attachment = slot.attachment
		local vertices = nil
		local indics = nil
		local texture = nil
		local color = tmpColor
		if attachment then
			if attachment.type == spine.AttachmentType.region then
				numVertices = 4
				vertices = self:computeRegionVertices(slot, attachment, premultipliedAlpha, color)
				indices = SkeletonRenderer.QUAD_TRIANGLES
				texture = attachment.region.renderObject.texture
			elseif attachment.type == spine.AttachmentType.mesh then
				numVertices = attachment.worldVerticesLength / 2
				vertices = self:computeMeshVertices(slot, attachment, premultipliedAlpha, color)
				indices = attachment.triangles
				texture = attachment.region.renderObject.texture
			end

			if texture then
				local slotBlendMode = slot.data.blendMode
				if lastBlendMode ~= slotBlendMode then
					if slotBlendMode == spine.BlendMode.normal then
						love.graphics.setBlendMode("alpha")
					elseif slotBlendMode == spine.BlendMode.additive then
						love.graphics.setBlendMode("additive")
					elseif slotBlendMode == spine.BlendMode.multiply then
						love.graphics.setBlendMode("multiply")
					elseif slotBlendMode == spine.BlendMode.screen then
						love.graphics.setBlendMode("screen")
					end
					lastBlendMode = slotBlendMode
					batcher:stop()
					batcher:begin()
				end
				batcher:draw(texture, vertices, numVertices, indices)
			end
		end
	end

	batcher:stop()
	love.graphics.setBlendMode(lastLoveBlendMode)
end

function SkeletonRenderer:computeRegionVertices(slot, region, pma, color)
	local skeleton = slot.bone.skeleton
	local skeletonColor = skeleton.color
	local slotColor = slot.color
	local regionColor = region.color
	local alpha = skeletonColor.a * slotColor.a * regionColor.a
	local multiplier = alpha
	if pma then multiplier = 1 end
	color:set(skeletonColor.r * slotColor.r * regionColor.r * multiplier,
				skeletonColor.g * slotColor.g * regionColor.g * multiplier,
				skeletonColor.b * slotColor.b * regionColor.b * multiplier,
				alpha)

	local vertices = worldVertices
	region:computeWorldVertices(slot.bone, vertices, 0, 8)

	local uvs = region.uvs

	vertices[3] = uvs[1]
	vertices[4] = uvs[2]
	vertices[5] = color.r
	vertices[6] = color.g
	vertices[7] = color.b
	vertices[8] = color.a

	vertices[11] = uvs[3]
	vertices[12] = uvs[4]
	vertices[13] = color.r
	vertices[14] = color.g
	vertices[15] = color.b
	vertices[16] = color.a

	vertices[19] = uvs[5]
	vertices[20] = uvs[6]
	vertices[21] = color.r
	vertices[22] = color.g
	vertices[23] = color.b
	vertices[24] = color.a

	vertices[27] = uvs[7]
	vertices[28] = uvs[8]
	vertices[29] = color.r
	vertices[30] = color.g
	vertices[31] = color.b
	vertices[32] = color.a

	return vertices
end

function SkeletonRenderer:computeMeshVertices(slot, mesh, pma, color)
	local skeleton = slot.bone.skeleton
	local skeletonColor = skeleton.color
	local slotColor = slot.color
	local meshColor = mesh.color
	local alpha = skeletonColor.a * slotColor.a * meshColor.a
	local multiplier = alpha
	if pma then multiplier = 1 end
	color:set(skeletonColor.r * slotColor.r * meshColor.r * multiplier,
				skeletonColor.g * slotColor.g * meshColor.g * multiplier,
				skeletonColor.b * slotColor.b * meshColor.b * multiplier,
				alpha)
			
	local numVertices = mesh.worldVerticesLength / 2
	local vertices = worldVertices
	mesh:computeWorldVertices(slot, 0, mesh.worldVerticesLength, vertices, 0, 8)
	
	local uvs = mesh.uvs
	local i = 1
	local n = numVertices + 1
	local u = 1
	local v = 3
	while i < n do
		vertices[v] = uvs[u]
		vertices[v + 1] = uvs[u + 1]
		vertices[v + 2] = color.r
		vertices[v + 3] = color.g
		vertices[v + 4] = color.b
		vertices[v + 5] = color.a
		i = i + 1
		u = u + 2
		v = v + 8
	end
	return vertices
end

spine.PolygonBatcher = PolygonBatcher
spine.SkeletonRenderer = SkeletonRenderer

return spine
