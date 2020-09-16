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

local AttachmentType = require "spine-lua.attachments.AttachmentType"
local VertexAttachment = require "spine-lua.attachments.VertexAttachment"
local Color = require "spine-lua.Color"

local math_cos = math.cos
local math_sin = math.sin

local PointAttachment = {}
PointAttachment.__index = PointAttachment
setmetatable(PointAttachment, { __index = VertexAttachment })

function PointAttachment.new (name)
	if not name then error("name cannot be nil", 2) end

	local self = VertexAttachment.new(name, AttachmentType.point)
	self.x = 0
	self.y = 0
	self.rotation = 0
	self.color = Color.newWith(0.38, 0.94, 0, 1)
	setmetatable(self, BoundingBoxAttachment)
	return self
end

function PointAttachment:computeWorldPosition(bone, point)
	point.x = self.x * bone.a + self.y * bone.b + bone.worldX
	point.y = self.x * bone.c + self.y * bone.d + bone.worldY
	return point
end

function PointAttachment:computeWorldRotation(bone)
	local cos = math_cos(math_rad(self.rotation))
	local sin = math_sin(math_rad(self.rotation))
	local x = cos * bone.a + sin * bone.b
	local y = cos * bone.c + sin * bone.d
	return math_deg(math_atan2(y, x))
end

function PointAttachment:copy ()
	local copy = PointAttachment.new(self.name)
	copy.x = self.x
	copy.y = self.y
	copy.rotation = self.rotation
	copy.color:setFrom(self.color)
	return copy
end

return PointAttachment
