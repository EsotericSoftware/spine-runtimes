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
local AttachmentType = require "spine-lua.attachments.AttachmentType"
local VertexAttachment = require "spine-lua.attachments.VertexAttachment"
local utils = require "spine-lua.utils"
local Color = require "spine-lua.Color"

local MeshAttachment = {}
MeshAttachment.__index = MeshAttachment
setmetatable(MeshAttachment, { __index = VertexAttachment })

function MeshAttachment.new (name)
	if not name then error("name cannot be nil", 2) end

	local self = VertexAttachment.new(name, AttachmentType.mesh)
	self.region = nil
	self.path = nil
	self.regionUVs = nil
	self.worldVertices = nil
	self.triangles = nil
	self.color = Color.newWith(1, 1, 1, 1)
	self.hullLength = 0
	self.parentMesh = nil
	self.inheritDeform = false
	self.tempColor = Color.newWith(1, 1, 1, 1)
	setmetatable(self, MeshAttachment)
	return self
end

function MeshAttachment:updateUVs ()
	local regionUVs = self.regionUVs
	local verticesLength = #regionUVs
	local worldVerticesLength = (verticesLength / 2) * 8
	if not self.worldVertices or #self.worldVertices ~= worldVerticesLength then
		self.worldVertices = utils.newNumberArray(worldVerticesLength)
	end

	local u = 0
	local v = 0
	local width = 0
	local height = 0
	if not self.region then
		u = 0
		v = 0
		width = 1
		height = 1
	else
		u = self.region.u;
		v = self.region.v;
		width = self.region.u2 - u;
		height = self.region.v2 - v;
	end
	if self.region and self.region.rotate then
		local i = 0
		local w = 2
		while i < verticesLength do
			self.worldVertices[w + 1] = u + regionUVs[i + 2] * width;
			self.worldVertices[w + 2] = v + height - regionUVs[i + 1] * height;
			i = i + 2
			w = w + 8
		end
	else
		local i = 0
		local w = 2
		while i < verticesLength do
			self.worldVertices[w + 1] = u + regionUVs[i + 1] * width;
			self.worldVertices[w + 2] = v + regionUVs[i + 2] * height;
			i = i + 2
			w = w + 8
		end
	end
end

function MeshAttachment:updateWorldVertices(slot, premultipliedAlpha)
	local skeleton = slot.bone.skeleton
	local skeletonColor = skeleton.color
	local slotColor = slot.color
	local meshColor = self.color

	local alpha = skeletonColor.a * slotColor.a * meshColor.a
	local multiplier = 1
	if premultipliedAlpha then multiplier = alpha end
	local color = self.tempColor
	color:set(skeletonColor.r * slotColor.r * meshColor.r * multiplier,
		skeletonColor.g * slotColor.g * meshColor.g * multiplier,
		skeletonColor.b * slotColor.b * meshColor.b * multiplier,
		alpha)

	local deformArray = slot.attachmentVertices
	local vertices = self.vertices
	local worldVertices = self.worldVertices
	local bones = self.bones
	if not bones then
		local verticesLength = #vertices
		if #deformArray > 0 then vertices = deformArray end
		local bone = slot.bone;
		local x = bone.worldX
		local y = bone.worldY
		local a = bone.a
		local b = bone.b
		local c = bone.c
		local d = bone.d
		local v = 0
		local w = 0
		while v < verticesLength do
			local vx = vertices[v + 1]
			local vy = vertices[v + 2]
			worldVertices[w + 1] = vx * a + vy * b + x
			worldVertices[w + 2] = vx * c + vy * d + y
			worldVertices[w + 5] = color.r
			worldVertices[w + 6] = color.g
			worldVertices[w + 7] = color.b
			worldVertices[w + 8] = color.a
			v = v + 2
			w = w + 8
		end
		return worldVertices
	end

	local skeletonBones = skeleton.bones
	if #deformArray == 0 then
		local w = 0
		local v = 0
		local b = 0
		local n = #bones
		while v < n do
			local wx = 0
			local wy = 0
			local nn = bones[v + 1];
			v = v + 1
			nn = nn + v
			while v < nn do
				local bone = skeletonBones[bones[v + 1]];
				local vx = vertices[b + 1]
				local vy = vertices[b + 2]
				local weight = vertices[b + 3]
				wx = wx + (vx * bone.a + vy * bone.b + bone.worldX) * weight
				wy = wy + (vx * bone.c + vy * bone.d + bone.worldY) * weight
				v = v + 1
				b = b + 3
			end
			worldVertices[w + 1] = wx
			worldVertices[w + 2] = wy
			worldVertices[w + 5] = color.r
			worldVertices[w + 6] = color.g
			worldVertices[w + 7] = color.b
			worldVertices[w + 8] = color.a
			w = w + 8
		end
	else
		local deform = deformArray;
		local w = 0
		local v = 0
		local b = 0
		local f = 0
		local n = #bones
		while v < n do
			local wx = 0
			local wy = 0
			local nn = bones[v + 1]
			v = v + 1
			nn = nn + v
			while v < nn do
				local bone = skeletonBones[bones[v + 1]];
				local vx = vertices[b + 1] + deform[f + 1]
				local vy = vertices[b + 2] + deform[f + 2]
				local weight = vertices[b + 3]
				wx = wx + (vx * bone.a + vy * bone.b + bone.worldX) * weight
				wy = wy + (vx * bone.c + vy * bone.d + bone.worldY) * weight
				b = b + 3
				f = f + 2
				v = v + 1
			end
			worldVertices[w + 1] = wx;
			worldVertices[w + 2] = wy;
			worldVertices[w + 5] = color.r;
			worldVertices[w + 6] = color.g;
			worldVertices[w + 7] = color.b;
			worldVertices[w + 8] = color.a;
			w = w + 8
		end
	end
	return worldVertices;
end

function MeshAttachment:applyDeform (sourceAttachment)
	return self == sourceAttachment or (self.inheritDeform and self.parentMesh == sourceAttachment)
end

function MeshAttachment:setParentMesh (parentMesh)
	self.parentMesh = parentMesh
	if parentMesh then
		self.bones = parentMesh.bones
		self.vertices = parentMesh.vertices
		self.regionUVs = parentMesh.regionUVs
		self.triangles = parentMesh.triangles
		self.hullLength = parentMesh.hullLength
	end
end

return MeshAttachment
