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

local AttachmentType = require "spine-lua.AttachmentType"

local RegionAttachment = {}
function RegionAttachment.new (name)
	if not name then error("name cannot be nil", 2) end

	local self = {
		name = name,
		type = AttachmentType.region,
		x = 0, y = 0,
		rotation = 0,
		scaleX = 1, scaleY = 1,
		width = 0, height = 0,
		offset = {},
		uvs = {},
		r = 1, g = 1, b = 1, a = 1,
		path = nil,
		rendererObject = nil,
		regionOffsetX = 0, regionOffsetY = 0,
		regionWidth = 0, regionHeight = 0,
		regionOriginalWidth = 0, regionOriginalHeight = 0
	}

	function self:updateOffset ()
		local regionScaleX = self.width / self.regionOriginalWidth * self.scaleX
		local regionScaleY = self.height / self.regionOriginalHeight * self.scaleY
		local localX = -self.width / 2 * self.scaleX + self.regionOffsetX * regionScaleX
		local localY = -self.height / 2 * self.scaleY + self.regionOffsetY * regionScaleY
		local localX2 = localX + self.regionWidth * regionScaleX
		local localY2 = localY + self.regionHeight * regionScaleY
		local radians = self.rotation * math.pi / 180
		local cos = math.cos(radians)
		local sin = math.sin(radians)
		local localXCos = localX * cos + self.x
		local localXSin = localX * sin
		local localYCos = localY * cos + self.y
		local localYSin = localY * sin
		local localX2Cos = localX2 * cos + self.x
		local localX2Sin = localX2 * sin
		local localY2Cos = localY2 * cos + self.y
		local localY2Sin = localY2 * sin
		local offset = self.offset
		offset[0] = localXCos - localYSin -- X1
		offset[1] = localYCos + localXSin -- Y1
		offset[2] = localXCos - localY2Sin -- X2
		offset[3] = localY2Cos + localXSin -- Y2
		offset[4] = localX2Cos - localY2Sin -- X3
		offset[5] = localY2Cos + localX2Sin -- Y3
		offset[6] = localX2Cos - localYSin -- X4
		offset[7] = localYCos + localX2Sin -- Y4
	end

	function self:computeWorldVertices (x, y, bone, worldVertices)
		x = x + bone.worldX
		y = y + bone.worldY
		local m00, m01, m10, m11 = bone.m00, bone.m01, bone.m10, bone.m11
		local offset = self.offset
		local vertices = self.vertices;
		vertices[0] = offset[0] * m00 + offset[1] * m01 + x
		vertices[1] = offset[0] * m10 + offset[1] * m11 + y
		vertices[2] = offset[2] * m00 + offset[3] * m01 + x
		vertices[3] = offset[2] * m10 + offset[3] * m11 + y
		vertices[4] = offset[4] * m00 + offset[5] * m01 + x
		vertices[5] = offset[4] * m10 + offset[5] * m11 + y
		vertices[6] = offset[6] * m00 + offset[7] * m01 + x
		vertices[7] = offset[6] * m10 + offset[7] * m11 + y
	end

	return self
end
return RegionAttachment
