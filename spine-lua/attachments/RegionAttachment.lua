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
	self.vertices = Utils.newNumberArray(8 * 4)
	self.tempColor = Color.newWith(1, 1, 1, 1)
	setmetatable(self, RegionAttachment)

	return self
end

function RegionAttachment:setRegion (region)
	local vertices = self.vertices
	if region.rotate then
		vertices[U2] = region.u
		vertices[V2] = region.v2
		vertices[U3] = region.u
		vertices[V3] = region.v
		vertices[U4] = region.u2
		vertices[V4] = region.v
		vertices[U1] = region.u2
		vertices[V1] = region.v2
	else
		vertices[U1] = region.u
		vertices[V1] = region.v2
		vertices[U2] = region.u
		vertices[V2] = region.v
		vertices[U3] = region.u2
		vertices[V3] = region.v
		vertices[U4] = region.u2
		vertices[V4] = region.v2
	end
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

function RegionAttachment:updateWorldVertices (slot, premultipliedAlpha)
	local skeleton = slot.bone.skeleton
	local skeletonColor = skeleton.color
	local slotColor = slot.color
	local regionColor = self.color
	local alpha = skeletonColor.a * slotColor.a * regionColor.a
	local multiplier = alpha
	if premultipliedAlpha then multiplier = 1 end
	local color = self.tempColor
	color:set(skeletonColor.r * slotColor.r * regionColor.r * multiplier,
		skeletonColor.g * slotColor.g * regionColor.g * multiplier,
		skeletonColor.b * slotColor.b * regionColor.b * multiplier,
		alpha)

	local vertices = self.vertices
	local offset = self.offset
	local bone = slot.bone
	local x = bone.worldX
	local y = bone.worldY
	local a = bone.a
	local b = bone.b
	local c = bone.c
	local d = bone.d
	local offsetX = 0
	local offsetY = 0

	offsetX = offset[OX1]
	offsetY = offset[OY1]
	vertices[X1] = offsetX * a + offsetY * b + x -- br
	vertices[Y1] = offsetX * c + offsetY * d + y
	vertices[C1R] = color.r
	vertices[C1G] = color.g
	vertices[C1B] = color.b
	vertices[C1A] = color.a

	offsetX = offset[OX2]
	offsetY = offset[OY2]
	vertices[X2] = offsetX * a + offsetY * b + x -- bl
	vertices[Y2] = offsetX * c + offsetY * d + y
	vertices[C2R] = color.r
	vertices[C2G] = color.g
	vertices[C2B] = color.b
	vertices[C2A] = color.a

	offsetX = offset[OX3]
	offsetY = offset[OY3]
	vertices[X3] = offsetX * a + offsetY * b + x -- ul
	vertices[Y3] = offsetX * c + offsetY * d + y
	vertices[C3R] = color.r
	vertices[C3G] = color.g
	vertices[C3B] = color.b
	vertices[C3A] = color.a

	offsetX = offset[OX4]
	offsetY = offset[OY4]
	vertices[X4] = offsetX * a + offsetY * b + x -- ur
	vertices[Y4] = offsetX * c + offsetY * d + y
	vertices[C4R] = color.r
	vertices[C4G] = color.g
	vertices[C4B] = color.b
	vertices[C4A] = color.a

	return vertices
end

return RegionAttachment
