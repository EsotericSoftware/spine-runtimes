-------------------------------------------------------------------------------
-- Spine Runtimes Software License
-- Version 2.1
-- 
-- Copyright (c) 2013, Esoteric Software
-- All rights reserved.
-- 
-- You are granted a perpetual, non-exclusive, non-sublicensable and
-- non-transferable license to install, execute and perform the Spine Runtimes
-- Software (the "Software") solely for internal use. Without the written
-- permission of Esoteric Software (typically granted by licensing Spine), you
-- may not (a) modify, translate, adapt or otherwise create derivative works,
-- improvements of the Software or develop new applications using the Software
-- or (b) remove, delete, alter or obscure any trademarks or any copyright,
-- trademark, patent or other intellectual property or proprietary rights
-- notices on or in the Software, including any copy thereof. Redistributions
-- in binary or source form must include this license and terms.
-- 
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
-- OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
-- WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
-- OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
-- ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local AttachmentType = require "spine-lua.AttachmentType"

local BoundingBoxAttachment = {}
function BoundingBoxAttachment.new (name)
	if not name then error("name cannot be nil", 2) end

	local self = {
		name = name,
		type = AttachmentType.boundingbox,
		vertices = {}
	}

	function self:computeWorldVertices (x, y, bone, worldVertices)
		x = x + bone.worldX
		y = y + bone.worldY
		local m00 = bone.m00
		local m01 = bone.m01
		local m10 = bone.m10
		local m11 = bone.m11
		local vertices = self.vertices
		local count = #vertices
		for i = 1, count, 2 do
			local px = vertices[i]
			local py = vertices[i + 1]
			worldVertices[i] = px * m00 + py * m01 + x
			worldVertices[i + 1] = px * m10 + py * m11 + y
		end
	end

	return self
end
return BoundingBoxAttachment
