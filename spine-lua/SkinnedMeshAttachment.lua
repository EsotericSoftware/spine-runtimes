-------------------------------------------------------------------------------
-- Spine Runtimes Software License
-- Version 2.3
-- 
-- Copyright (c) 2013-2015, Esoteric Software
-- All rights reserved.
-- 
-- You are granted a perpetual, non-exclusive, non-sublicensable and
-- non-transferable license to use, install, execute and perform the Spine
-- Runtimes Software (the "Software") and derivative works solely for personal
-- or internal use. Without the written permission of Esoteric Software (see
-- Section 2 of the Spine Software License Agreement), you may not (a) modify,
-- translate, adapt or otherwise create derivative works, improvements of the
-- Software or develop new applications using the Software or (b) remove,
-- delete, alter or obscure any trademarks or any copyright, trademark, patent
-- or other intellectual property or proprietary rights notices on or in the
-- Software, including any copy thereof. Redistributions in binary or source
-- form must include this license and terms.
-- 
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
-- OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
-- WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
-- OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
-- ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local AttachmentType = require "spine-lua.AttachmentType"

local SkinnedMeshAttachment = {}
function SkinnedMeshAttachment.new (name)
	if not name then error("name cannot be nil", 2) end
	
	local self = {
		name = name,
		type = AttachmentType.mesh,
		bones = nil,
		weights = nil,
		uvs = nil,
		regionUVs = nil,
		triangles = nil,
		hullLength = 0,
		r = 1, g = 1, b = 1, a = 1,
		path = nil,
		rendererObject = nil,
		regionU = 0, regionV = 0, regionU2 = 0, regionV2 = 0, regionRotate = false,
		regionOffsetX = 0, regionOffsetY = 0,
		regionWidth = 0, regionHeight = 0,
		regionOriginalWidth = 0, regionOriginalHeight = 0,
		edges = nil,
		width = 0, height = 0
	}

	function self:updateUVs ()
		local width, height = self.regionU2 - self.regionU, self.regionV2 - self.regionV
		local n = #self.regionUVs
		if not self.uvs or #self.uvs ~= n then
			self.uvs = {}
		end
		if self.regionRotate then
			for i = 1, n, 2 do
				self.uvs[i] = self.regionU + self.regionUVs[i + 1] * width
				self.uvs[i + 1] = self.regionV + height - self.regionUVs[i] * height
			end
		else
			for i = 1, n, 2 do
				self.uvs[i] = self.regionU + self.regionUVs[i] * width
				self.uvs[i + 1] = self.regionV + self.regionUVs[i + 1] * height
			end
		end
	end

	function self:computeWorldVertices (x, y, slot, worldVertices)
		local skeletonBones = slot.skeleton.bones
		local weights = self.weights
		local bones = self.bones

		local w, v, b, f = 0, 0, 0, 0
		local	n = bones.length
		local wx, wy, bone, vx, vy, weight
		if #slot.attachmentVertices == 0 then
			while v < n do
				wx = 0
				wy = 0
				local nn = bones[v] + v
				v = v + 1
				while v <= nn do
					bone = skeletonBones[bones[v]]
					vx = weights[b]
					vy = weights[b + 1]
					weight = weights[b + 2]
					wx = wx + (vx * bone.m00 + vy * bone.m01 + bone.worldX) * weight
					wy = wy + (vx * bone.m10 + vy * bone.m11 + bone.worldY) * weight
					v = v + 1
					b = b + 3
				end
				worldVertices[w] = wx + x
				worldVertices[w + 1] = wy + y
				w = w + 2
			end
		else
			local ffd = slot.attachmentVertices
			while v < n do
				wx = 0
				wy = 0
				local nn = bones[v] + v
				v = v + 1
				while v <= nn do
					bone = skeletonBones[bones[v]]
					vx = weights[b] + ffd[f]
					vy = weights[b + 1] + ffd[f + 1]
					weight = weights[b + 2]
					wx = wx + (vx * bone.m00 + vy * bone.m01 + bone.worldX) * weight
					wy = wy + (vx * bone.m10 + vy * bone.m11 + bone.worldY) * weight
					v = v + 1
					b = b + 3
					f = f + 2
				end
				worldVertices[w] = wx + x
				worldVertices[w + 1] = wy + y
				w = w + 2
			end
		end
	end

	return self
end
return SkinnedMeshAttachment
