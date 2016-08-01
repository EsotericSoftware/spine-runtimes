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
local utils = require "spine-lua.utils"

local SkeletonBounds = {}
function SkeletonBounds.new ()
	local self = {
		polygons = {},
		boundingBoxes = {},
		minX = 0, minY = 0, maxX = 0, maxY = 0
	}

	function aabbCompute ()
		local polygons = self.polygons
		local minX, minY, maxX, maxY = 9999999, 9999999, -9999999, -9999999
		for i,vertices in ipairs(polygons) do
			local count = #vertices
			for ii = 1, count, 2 do
				local x = vertices[ii]
				local y = vertices[ii + 1]
				minX = math.min(minX, x)
				minY = math.min(minY, y)
				maxX = math.max(maxX, x)
				maxY = math.max(maxY, y)
			end
		end
		self.minX = minX
		self.minY = minY
		self.maxX = maxX
		self.maxY = maxY
	end

	function self:update (skeleton, updateAabb)
		local x = skeleton.x
		local y = skeleton.y

		local polygons = {}
		self.polygons = polygons

		local boundingBoxes = {}
		self.boundingBoxes = boundingBoxes

		for i,slot in ipairs(skeleton.slots) do
			local boundingBox = slot.attachment
			if boundingBox and boundingBox.type == AttachmentType.boundingbox then
				table.insert(boundingBoxes, boundingBox)

				local polygon = {}
				table.insert(polygons, polygon)

				boundingBox:computeWorldVertices(x, y, slot.bone, polygon)
			end
		end

		if updateAabb then aabbCompute() end
	end

	function self:aabbContainsPoint (x, y)
		return x >= self.minX and x <= self.maxX and y >= self.minY and y <= self.maxY
	end

	function self:aabbIntersectsSegment (x1, y1, x2, y2)
		local minX, minY, maxX, maxY = self.minX, self.minY, self.maxX, self.maxY
		if (x1 <= minX and x2 <= minX) or (y1 <= minY and y2 <= minY) or (x1 >= maxX and x2 >= maxX) or (y1 >= maxY and y2 >= maxY) then
			return false
		end
		local m = (y2 - y1) / (x2 - x1)
		local y = m * (minX - x1) + y1
		if y > minY and y < maxY then return true end
		y = m * (maxX - x1) + y1
		if y > minY and y < maxY then return true end
		local x = (minY - y1) / m + x1
		if x > minX and x < maxX then return true end
		x = (maxY - y1) / m + x1
		if x > minX and x < maxX then return true end
		return false
	end

	function self:aabbIntersectsSkeleton (bounds)
		return self.minX < bounds.maxX and self.maxX > bounds.minX and self.minY < bounds.maxY and self.maxY > bounds.minY
	end

	function self:containsPoint (x, y)
		for i,polygon in ipairs(self.polygons) do
			if self:polygonContainsPoint(polygon, x, y) then return self.boundingBoxes[i] end
		end
		return nil
	end

	function self:intersectsSegment (x1, y1, x2, y2)
		for i,polygon in ipairs(self.polygons) do
			if self:polygonIntersectsSegment(polygon, x1, y1, x2, y2) then return self.boundingBoxes[i] end
		end
		return nil
	end

	function self:polygonContainsPoint (polygon, x, y)
		local nn = #polygon
		local prevIndex = nn - 1
		local inside = false
		for ii = 1, nn, 2 do
			local vertexY = polygon[ii + 1]
			local prevY = polygon[prevIndex + 1]
			if (vertexY < y and prevY >= y) or (prevY < y and vertexY >= y) then
				local vertexX = polygon[ii]
				if vertexX + (y - vertexY) / (prevY - vertexY) * (polygon[prevIndex] - vertexX) < x then inside = not inside end
			end
			prevIndex = ii
		end
		return inside
	end

	function self:polygonIntersectsSegment (polygon, x1, y1, x2, y2)
		local nn = #polygon
		local width12, height12 = x1 - x2, y1 - y2
		local det1 = x1 * y2 - y1 * x2
		local x3, y3 = polygon[nn - 2], polygon[nn - 1]
		for ii = 1, nn, 2 do
			local x4, y4 = polygon[ii], polygon[ii + 1]
			local det2 = x3 * y4 - y3 * x4
			local width34, height34 = x3 - x4, y3 - y4
			local det3 = width12 * height34 - height12 * width34
			local x = (det1 * width34 - width12 * det2) / det3
			if ((x >= x3 and x <= x4) or (x >= x4 and x <= x3)) and ((x >= x1 and x <= x2) or (x >= x2 and x <= x1)) then
				local y = (det1 * height34 - height12 * det2) / det3
				if ((y >= y3 and y <= y4) or (y >= y4 and y <= y3)) and ((y >= y1 and y <= y2) or (y >= y2 and y <= y1)) then return true end
			end
			x3 = x4
			y3 = y4
		end
		return false
	end

	function self:getPolygon (attachment)
		local index = spine.utils.indexOf(self.boundingBoxes, attachment)
		if index == -1 then
			return nil
		else
			return self.polygons[index]
		end
	end

	function self:getWidth ()
		return self.maxX - self.minX
	end

	function self:getHeight ()
		return self.maxY - self.minY
	end

	return self
end
return SkeletonBounds
