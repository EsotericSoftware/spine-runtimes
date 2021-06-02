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

local setmetatable = setmetatable
local math_pi = math.pi
local math_sin = math.sin
local math_cos = math.cos

local AttachmentType = require "spine-lua.attachments.AttachmentType"
local Attachment = require "spine-lua.attachments.Attachment"
local Color = require "spine-lua.Color"
local Utils = require "spine-lua.utils"

local OX1 = 1
local OY1 = 2
local OX2 = 3
local OY2 = 4
local OX3 = 5
local OY3 = 6
local OX4 = 7
local OY4 = 8

local X1 = 1
local Y1 = 2
local U1 = 3
local V1 = 4
local C1R = 5
local C1G = 6
local C1B = 7
local C1A = 8

local X2 = 9
local Y2 = 10
local U2 = 11
local V2 = 12
local C2R = 13
local C2G = 14
local C2B = 15
local C2A = 16

local X3 = 17
local Y3 = 18
local U3 = 19
local V3 = 20
local C3R = 21
local C3G = 22
local C3B = 23
local C3A = 24

local X4 = 25
local Y4 = 26
local U4 = 27
local V4 = 28
local C4R = 29
local C4G = 30
local C4B = 31
local C4A = 32

local RegionAttachment = {}
RegionAttachment.__index = RegionAttachment
setmetatable(RegionAttachment, { __index = Attachment })

RegionAttachment.OX1 = 1
RegionAttachment.OY1 = 2
RegionAttachment.OX2 = 3
RegionAttachment.OY2 = 4
RegionAttachment.OX3 = 5
RegionAttachment.OY3 = 6
RegionAttachment.OX4 = 7
RegionAttachment.OY4 = 8

RegionAttachment.X1 = 1
RegionAttachment.Y1 = 2
RegionAttachment.U1 = 3
RegionAttachment.V1 = 4
RegionAttachment.C1R = 5
RegionAttachment.C1G = 6
RegionAttachment.C1B = 7
RegionAttachment.C1A = 8

RegionAttachment.X2 = 9
RegionAttachment.Y2 = 10
RegionAttachment.U2 = 11
RegionAttachment.V2 = 12
RegionAttachment.C2R = 13
RegionAttachment.C2G = 14
RegionAttachment.C2B = 15
RegionAttachment.C2A = 16

RegionAttachment.X3 = 17
RegionAttachment.Y3 = 18
RegionAttachment.U3 = 19
RegionAttachment.V3 = 20
RegionAttachment.C3R = 21
RegionAttachment.C3G = 22
RegionAttachment.C3B = 23
RegionAttachment.C3A = 24

RegionAttachment.X4 = 25
RegionAttachment.Y4 = 26
RegionAttachment.U4 = 27
RegionAttachment.V4 = 28
RegionAttachment.C4R = 29
RegionAttachment.C4G = 30
RegionAttachment.C4B = 31
RegionAttachment.C4A = 32

function RegionAttachment.new (name)
	if not name then error("name cannot be nil", 2) end

	local self = Attachment.new(name, AttachmentType.region)
	self.x = 0
	self.y = 0
	self.scaleX = 1
	self.scaleY = 1
	self.rotation = 0
	self.width = 0
	self.height = 0
	self.color = Color.newWith(1, 1, 1, 1)
	self.path = nil
	self.rendererObject = nil
	self.region = nil
	self.offset = Utils.newNumberArray(8)
	self.uvs = Utils.newNumberArray(8)
	self.tempColor = Color.newWith(1, 1, 1, 1)
	setmetatable(self, RegionAttachment)

	return self
end

function RegionAttachment:updateOffset ()
	local regionScaleX = self.width / self.region.originalWidth * self.scaleX
	local regionScaleY = self.height / self.region.originalHeight * self.scaleY
	local localX = -self.width / 2 * self.scaleX + self.region.offsetX * regionScaleX
	local localY = -self.height / 2 * self.scaleY + self.region.offsetY * regionScaleY
	local localX2 = localX + self.region.width * regionScaleX
	local localY2 = localY + self.region.height * regionScaleY
	local radians = self.rotation * math_pi / 180
	local cos = math_cos(radians)
	local sin = math_sin(radians)
	local localXCos = localX * cos + self.x
	local localXSin = localX * sin
	local localYCos = localY * cos + self.y
	local localYSin = localY * sin
	local localX2Cos = localX2 * cos + self.x
	local localX2Sin = localX2 * sin
	local localY2Cos = localY2 * cos + self.y
	local localY2Sin = localY2 * sin
	local offset = self.offset
	offset[OX1] = localXCos - localYSin
	offset[OY1] = localYCos + localXSin
	offset[OX2] = localXCos - localY2Sin
	offset[OY2] = localY2Cos + localXSin
	offset[OX3] = localX2Cos - localY2Sin
	offset[OY3] = localY2Cos + localX2Sin
	offset[OX4] = localX2Cos - localYSin
	offset[OY4] = localYCos + localX2Sin
end

function RegionAttachment:setRegion (region)
	local uvs = self.uvs
	if region.rotate then
		uvs[5] = region.u
		uvs[6] = region.v2
		uvs[7] = region.u
		uvs[8] = region.v
		uvs[1] = region.u2
		uvs[2] = region.v
		uvs[3] = region.u2
		uvs[4] = region.v2
	else
		uvs[3] = region.u
		uvs[4] = region.v2
		uvs[5] = region.u
		uvs[6] = region.v
		uvs[7] = region.u2
		uvs[8] = region.v
		uvs[1] = region.u2
		uvs[2] = region.v2
	end
end

function RegionAttachment:computeWorldVertices (bone, worldVertices, offset, stride)
	offset = offset + 1
	local vertexOffset = self.offset
	local x = bone.worldX
	local y = bone.worldY
	local a = bone.a
	local b = bone.b
	local c = bone.c
	local d = bone.d
	local offsetX = 0
	local offsetY = 0

	offsetX = vertexOffset[7]
	offsetY = vertexOffset[8]
	worldVertices[offset] = offsetX * a + offsetY * b + x -- br
	worldVertices[offset + 1] = offsetX * c + offsetY * d + y
	offset = offset + stride

	offsetX = vertexOffset[1]
	offsetY = vertexOffset[2]
	worldVertices[offset] = offsetX * a + offsetY * b + x -- bl
	worldVertices[offset + 1] = offsetX * c + offsetY * d + y
	offset = offset + stride

	offsetX = vertexOffset[3]
	offsetY = vertexOffset[4]
	worldVertices[offset] = offsetX * a + offsetY * b + x -- ul
	worldVertices[offset + 1] = offsetX * c + offsetY * d + y
	offset = offset + stride

	offsetX = vertexOffset[5]
	offsetY = vertexOffset[6]
	worldVertices[offset] = offsetX * a + offsetY * b + x -- ur
	worldVertices[offset + 1] = offsetX * c + offsetY * d + y
end

function RegionAttachment:copy ()
	local copy = RegionAttachment.new(self.name)
	copy.x = self.x
	copy.y = self.y
	copy.scaleX = self.scaleX
	copy.scaleY = self.scaleY
	copy.rotation = self.rotation
	copy.width = self.width
	copy.height = self.height
	copy.color:setFrom(self.color)
	copy.path = self.path
	copy.rendererObject = self.rendererObject
	copy.region = self.region
	copy.offset = Utils.copy(self.offset)
	copy.uvs = Utils.copy(self.uvs)
	copy.tempColor:setFrom(self.tempColor)
	return copy
end

return RegionAttachment
