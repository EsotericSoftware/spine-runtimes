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
	self.uvs = nil
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
	local regionUVs = self.regionUVs
	if not self.uvs or (#self.uvs ~= #regionUVs) then self.uvs = utils.newNumberArray(#regionUVs) end
	local uvs = self.uvs
	if self.region and self.region.rotate then
		local i = 0
		local n = #uvs
		while i < n do
			uvs[i + 1] = u + regionUVs[i + 2] * width;
			uvs[i + 2] = v + height - regionUVs[i + 1] * height;
			i = i + 2
		end
	else
		local i = 0
		local n = #uvs
		while i < n do
			uvs[i + 1] = u + regionUVs[i + 1] * width;
			uvs[i + 2] = v + regionUVs[i + 2] * height;
			i = i + 2
		end
	end
end

function MeshAttachment:applyDeform (sourceAttachment)
	return self == sourceAttachment or (self.inheritDeform and self.parentMesh == sourceAttachment)
end

function MeshAttachment:setParentMesh (parentMesh)
	self.parentMesh = parentMesh
	if parentMesh then
		self.bones = parentMesh.bones
		self.vertices = parentMesh.vertices
		self.worldVerticesLength = parentMesh.worldVerticesLength
		self.regionUVs = parentMesh.regionUVs
		self.triangles = parentMesh.triangles
		self.hullLength = parentMesh.hullLength
	end
end

return MeshAttachment
