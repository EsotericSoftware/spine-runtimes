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
spine.PointAttachment = require "spine-lua.attachments.ClippingAttachment"
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

function PolygonBatcher.new(vertexCount, useTwoColorTint)
	if useTwoColorTint == nil then useTwoColorTint = false end
	
	local vertexFormat
	local twoColorTintShader = nil
	
	if useTwoColorTint then
		vertexFormat = {
			{"VertexPosition", "float", 2}, -- The x,y position of each vertex.
			{"VertexTexCoord", "float", 2}, -- The u,v texture coordinates of each vertex.
			{"VertexColor", "byte", 4}, -- The r,g,b,a light color of each vertex.
			{"VertexColor2", "byte", 4} -- The r,g,b,a dark color of each vertex.
		}
		local vertexcode = [[
			attribute vec4 VertexColor2;
			varying vec4 color2;
			
			vec4 position(mat4 transform_projection, vec4 vertex_position) {
				color2 = VertexColor2;
				return transform_projection * vertex_position;
			}
    ]]
		
		local pixelcode = [[
			varying vec4 color2;
			
			vec4 effect(vec4 color, Image texture, vec2 texture_coords, vec2 screen_coords) {
				vec4 texColor = Texel(texture, texture_coords);
				float alpha = texColor.a * color.a;
				vec4 outputColor;
				outputColor.a = alpha;
				outputColor.rgb = (1.0 - texColor.rgb) * color2.rgb * alpha + texColor.rgb * color.rgb;
				return outputColor;
			}
    ]]
 
		twoColorTintShader = love.graphics.newShader(pixelcode, vertexcode)
	else
		vertexFormat = {
			{"VertexPosition", "float", 2}, -- The x,y position of each vertex.
			{"VertexTexCoord", "float", 2}, -- The u,v texture coordinates of each vertex.
			{"VertexColor", "byte", 4} -- The r,g,b,a light color of each vertex.
		}
	end		
	
	local self = {
		mesh = love.graphics.newMesh(vertexFormat, vertexCount, "triangles", "dynamic"),
		maxVerticesLength = vertexCount,
		maxIndicesLength = vertexCount * 3,
		verticesLength = 0,
		indicesLength = 0,
		lastTexture = nil,
		isDrawing = false,
		drawCalls = 0,
		vertex = { 0, 0, 0, 0, 0, 0, 0, 0 },
		indices = nil,
		useTwoColorTint = useTwoColorTint,
		twoColorTintShader = twoColorTintShader,
		tempVertex = {
			x = 0,
			y = 0,
			u = 0,
			v = 0,
			light = spine.Color.newWith(1, 1, 1, 1),
			dark = spine.Color.newWith(0, 0, 0, 0)
		}
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

function PolygonBatcher:draw (texture, vertices, uvs, numVertices, indices, color, darkColor, vertexEffect)
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

	local vertexStart = self.verticesLength + 1
	local vertexEnd = vertexStart + numVertices
	local vertex = self.vertex
	if not self.useTwoColorTint then
		vertex[5] = color.r * 255
		vertex[6] = color.g * 255
		vertex[7] = color.b * 255
		vertex[8] = color.a * 255
	else
		vertex[5] = color.r * 255
		vertex[6] = color.g * 255
		vertex[7] = color.b * 255
		vertex[8] = color.a * 255
		vertex[9] = darkColor.r * 255
		vertex[10] = darkColor.g * 255
		vertex[11] = darkColor.b * 255
		vertex[12] = darkColor.a * 255
	end
	
	local v = 1
	if (vertexEffect) then
		local tempVertex = self.tempVertex
		while vertexStart < vertexEnd do
			tempVertex.x = vertices[v]
			tempVertex.y = vertices[v + 1]
			tempVertex.u = uvs[v]
			tempVertex.v = uvs[v + 1]
			tempVertex.light:setFrom(color)
			tempVertex.dark:setFrom(darkColor)
			vertexEffect:transform(tempVertex)
			vertex[1] = tempVertex.x
			vertex[2] = tempVertex.y
			vertex[3] = tempVertex.u
			vertex[4] = tempVertex.v
			vertex[5] = tempVertex.light.r * 255
			vertex[6] = tempVertex.light.g * 255
			vertex[7] = tempVertex.light.b * 255
			vertex[8] = tempVertex.light.a * 255
			if (self.useTwoColorTint) then
				vertex[9] = tempVertex.dark.r * 255
				vertex[10] = tempVertex.dark.g * 255
				vertex[11] = tempVertex.dark.b * 255
				vertex[12] = tempVertex.dark.a * 255
			end
			mesh:setVertex(vertexStart, vertex)
			vertexStart = vertexStart + 1
			v = v + 2
		end
	else
		while vertexStart < vertexEnd do
			vertex[1] = vertices[v]
			vertex[2] = vertices[v + 1]
			vertex[3] = uvs[v]
			vertex[4] = uvs[v + 1]
			mesh:setVertex(vertexStart, vertex)
			vertexStart = vertexStart + 1
			v = v + 2
		end
	end
	self.verticesLength = self.verticesLength + numVertices
end

function PolygonBatcher:flush ()
	if self.verticesLength == 0 then return end
	local mesh = self.mesh
	mesh:setVertexMap(self.indices)
	mesh:setDrawRange(1, self.indicesLength)
	if not self.useTwoColorTint then
		love.graphics.draw(mesh, 0, 0)
	else
		love.graphics.setShader(self.twoColorTintShader)
		love.graphics.draw(mesh, 0, 0)
		love.graphics.setShader()
	end

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

function SkeletonRenderer.new (useTwoColorTint)
	if not useTwoColorTint then useTwoColorTint = false end
	local self = {
		batcher = PolygonBatcher.new(3 * 500, useTwoColorTint),
		premultipliedAlpha = false,
		useTwoColorTint = useTwoColorTint,
		clipper = spine.SkeletonClipping.new(),
		vertexEffect = nil
	}

	setmetatable(self, SkeletonRenderer)
	return self
end

local worldVertices = spine.utils.newNumberArray(10000 * 12)
local tmpColor = spine.Color.newWith(0, 0, 0, 0)
local tmpColor2 = spine.Color.newWith(0, 0, 0, 0)

function SkeletonRenderer:draw (skeleton)
	local batcher = self.batcher
	local premultipliedAlpha = self.premultipliedAlpha

	if (self.vertexEffect) then self.vertexEffect:beginEffect(skeleton) end

	local lastLoveBlendMode = love.graphics.getBlendMode()
	love.graphics.setBlendMode("alpha")
	local lastBlendMode = spine.BlendMode.normal
	batcher:begin()

	local drawOrder = skeleton.drawOrder
	for i, slot in ipairs(drawOrder) do
		local attachment = slot.attachment
		local vertices = worldVertices
		local uvs = nil
		local indices = nil
		local texture = nil
		local color = tmpColor
		if attachment then
			if attachment.type == spine.AttachmentType.region then
				numVertices = 4
				attachment:computeWorldVertices(slot.bone, vertices, 0, 2)
				uvs = attachment.uvs
				indices = SkeletonRenderer.QUAD_TRIANGLES
				texture = attachment.region.renderObject.texture
			elseif attachment.type == spine.AttachmentType.mesh then
				numVertices = attachment.worldVerticesLength / 2
				attachment:computeWorldVertices(slot, 0, attachment.worldVerticesLength, vertices, 0, 2)
				uvs = attachment.uvs
				indices = attachment.triangles
				texture = attachment.region.renderObject.texture
			elseif attachment.type == spine.AttachmentType.clipping then
				self.clipper:clipStart(slot, attachment)
			end

			if texture then								
				local slotBlendMode = slot.data.blendMode
				if lastBlendMode ~= slotBlendMode then
          batcher:stop()
					batcher:begin()
          
					if slotBlendMode == spine.BlendMode.normal then
 						love.graphics.setBlendMode("alpha")
					elseif slotBlendMode == spine.BlendMode.additive then
 						love.graphics.setBlendMode("add")
					elseif slotBlendMode == spine.BlendMode.multiply then
						love.graphics.setBlendMode("multiply")
					elseif slotBlendMode == spine.BlendMode.screen then
						love.graphics.setBlendMode("screen")
					end
					lastBlendMode = slotBlendMode					
				end
				
				local skeleton = slot.bone.skeleton
				local skeletonColor = skeleton.color
				local slotColor = slot.color
				local attachmentColor = attachment.color
				local alpha = skeletonColor.a * slotColor.a * attachmentColor.a
				local multiplier = alpha
				if premultipliedAlpha then multiplier = 1 end
				color:set(skeletonColor.r * slotColor.r * attachmentColor.r * multiplier,
							skeletonColor.g * slotColor.g * attachmentColor.g * multiplier,
							skeletonColor.b * slotColor.b * attachmentColor.b * multiplier,
							alpha)
						
				local dark = tmpColor2
				if slot.darkColor then dark = slot.darkColor
				else dark:set(0, 0, 0, 0) end
				
				if self.clipper:isClipping() then
					self.clipper:clipTriangles(vertices, attachment.uvs, indices, #indices)
					vertices = self.clipper.clippedVertices
					numVertices = #vertices / 2
					uvs = self.clipper.clippedUVs
					indices = self.clipper.clippedTriangles
				end
				
				batcher:draw(texture, vertices, uvs, numVertices, indices, color, dark, self.vertexEffect)
			end
			
			self.clipper:clipEnd(slot)
		end
	end

	batcher:stop()
	love.graphics.setBlendMode(lastLoveBlendMode)
	self.clipper:clipEnd2()
	if (self.vertexEffect) then self.vertexEffect:endEffect(skeleton) end
end

spine.PolygonBatcher = PolygonBatcher
spine.SkeletonRenderer = SkeletonRenderer

return spine
