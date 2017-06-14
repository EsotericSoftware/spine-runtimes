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

-- FIXME the logic in this file uses 0-based indexing. Each array
-- access adds 1 to the calculated index. We should switch the logic
-- to 1-based indexing eventually.

local setmetatable = setmetatable

local AttachmentType = require "spine-lua.attachments.AttachmentType"
local Attachment = require "spine-lua.attachments.Attachment"

local nextID = 0;
local SHL_11 = 2048;

local VertexAttachment = {}
VertexAttachment.__index = VertexAttachment
setmetatable(VertexAttachment, { __index = Attachment })

function VertexAttachment.new (name, attachmentType)
	local self = Attachment.new(name, attachmentType)
	self.bones = nil
	self.vertices = nil
	self.worldVerticesLength = 0
	while nextID > 65535 do
		nextID = nextID - 65535
	end
	self.id = nextID * SHL_11
	nextID = nextID + 1
	setmetatable(self, VertexAttachment)
	return self
end

function VertexAttachment:computeWorldVertices (slot, start, count, worldVertices, offset, stride)
	count = offset + (count / 2) * stride
	local skeleton = slot.bone.skeleton
	local deformArray = slot.attachmentVertices
	local vertices = self.vertices
	local bones = self.bones
	if not bones then
		if #deformArray > 0 then vertices = deformArray end
		local bone = slot.bone
		x = bone.worldX
		y = bone.worldY
		local a = bone.a
		local b = bone.b
		local c = bone.c
		local d = bone.d
		local v = start
		local w = offset
		while w < count do
			local vx = vertices[v + 1]
			local vy = vertices[v + 2]
			worldVertices[w + 1] = vx * a + vy * b + x
			worldVertices[w + 2] = vx * c + vy * d + y
			v = v + 2
			w = w + stride
		end
		return
	end
	local v = 0
	local skip = 0
	local i = 0
	while i < start do
		local n = bones[v + 1]
		v = v + n + 1
		skip = skip + n
		i = i + 2
	end
	local skeletonBones = skeleton.bones
	if #deformArray == 0 then
		local w = offset
		local b = skip * 3
		while w < count do
			local wx = 0
			local wy = 0
			local n = bones[v + 1]
			v = v + 1
			n = n + v
			while v < n do
				local bone = skeletonBones[bones[v + 1]]
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
			w = w + stride
		end
	else
		local deform = deformArray
		local w = offset
		local b = skip * 3
		local f = skip * 2
		while w < count do
			local wx = 0
			local wy = 0
			local n = bones[v + 1]
			v = v + 1
			n = n + v

			while v < n do
				local bone = skeletonBones[bones[v + 1]]
				local vx = vertices[b + 1] + deform[f + 1]
				local vy = vertices[b + 2] + deform[f + 2]
				local weight = vertices[b + 3]
				wx = wx + (vx * bone.a + vy * bone.b + bone.worldX) * weight
				wy = wy + (vx * bone.c + vy * bone.d + bone.worldY) * weight
				v = v + 1
				b = b + 3
				f = f + 2
			end
			worldVertices[w + 1] = wx
			worldVertices[w + 2] = wy
			w = w + stride
		end
	end
end

function VertexAttachment:applyDeform (sourceAttachment)
	return self == sourceAttachment
end

return VertexAttachment
