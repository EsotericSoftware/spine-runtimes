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

local utils = require "spine-lua.utils"
local Triangulator = require "spine-lua.Triangulator"

local setmetatable = setmetatable
local math_min = math.min
local math_max = math.max
local ipairs = ipairs
local table_insert = table.insert
local table_remove = table.remove

local SkeletonClipping = {}
SkeletonClipping.__index = SkeletonClipping

function SkeletonClipping.new ()
	local self = {
		triangulator = Triangulator.new(),
		clippingPolygon = {},
		clipOutput = {},
		clippedVertices = {},
		clippedUVs = {},
		clippedTriangles = {},		
		clipAttachment = nil
	}
	setmetatable(self, SkeletonClipping)

	return self
end

function SkeletonClipping:clipStart(slot, clip)
	if self.clipAttachment then return 0 end
	self.clipAttachment = clip
	
	local n = clip.worldVerticesLength
	self.clippingPolygon = {}
	local vertices = self.clippingPolygon
	clip:computeWorldVertices(slot, 0, n, vertices, 0, 2)
	self:makeClockwise(self.clippingPolygon)
	self.clippingPolygons = self.triangulator:decompose(self.clippingPolygon, self.triangulator:triangulate(self.clippingPolygon))
	for i,polygon in ipairs(self.clippingPolygons) do
		self:makeClockwise(polygon)
		table_insert(polygon, polygon[1])
		table_insert(polygon, polygon[2])		
	end
	return #self.clippingPolygons
end

function SkeletonClipping:clipEnd(slot)
	if self.clipAttachment and self.clipAttachment.endSlot == slot then self:clipEnd2() end
end

function SkeletonClipping:clipEnd2()
	if self.clipAttachment == nil then return end
	self.clipAttachment = nil
	self.clippingPolygons = nil
	self.clippedVertices = {}
	self.clippedUVs = {}
	self.clippedTriangles = {}
	self.clippingPolygon = {}
end

function SkeletonClipping:isClipping()
	return self.clipAttachment ~= nil
end

function SkeletonClipping:clipTriangles(vertices, uvs, triangles, trianglesLength)
	self.clippedVertices = {}
	self.clippedUVs = {}
	self.clippedTriangles = {}
	local clippedVertices = self.clippedVertices
	local clippedUVs = self.clippedUVs
	local clippedTriangles = self.clippedTriangles
	local polygons = self.clippingPolygons
	local polygonsCount = #self.clippingPolygons

	local index = 1
	
	local i = 1
	while i <= trianglesLength do
		local vertexOffset = (triangles[i] - 1) * 2 + 1
		local x1 = vertices[vertexOffset]
		local y1 = vertices[vertexOffset + 1]
		local u1 = uvs[vertexOffset]
		local v1 = uvs[vertexOffset + 1]

		vertexOffset = (triangles[i + 1] - 1) * 2 + 1
		local x2 = vertices[vertexOffset]
		local y2 = vertices[vertexOffset + 1]
		local u2 = uvs[vertexOffset]
		local v2 = uvs[vertexOffset + 1]

		vertexOffset = (triangles[i + 2] - 1) * 2 + 1;
		local x3 = vertices[vertexOffset]
		local y3 = vertices[vertexOffset + 1]
		local u3 = uvs[vertexOffset]
		local v3 = uvs[vertexOffset + 1]

		local p = 1
		while p <= polygonsCount do
			local s = #clippedVertices + 1
			local clipOutput = {}
			if (self:clip(x1, y1, x2, y2, x3, y3, polygons[p], clipOutput)) then
				local clipOutputLength = #clipOutput
				if (clipOutputLength > 0) then
					local d0 = y2 - y3
					local d1 = x3 - x2
					local d2 = x1 - x3
					local d4 = y3 - y1
					local d = 1 / (d0 * d2 + d1 * (y1 - y3));

					local clipOutputCount = clipOutputLength / 2
					local clipOutputItems = clipOutput
					local clippedVerticesItems = clippedVertices
					local clippedUVsItems = clippedUVs
					local ii = 1
					while ii <= clipOutputLength do
						local x = clipOutputItems[ii]
						local y = clipOutputItems[ii + 1]
						clippedVerticesItems[s] = x
						clippedVerticesItems[s + 1] = y						
						local c0 = x - x3
						local c1 = y - y3
						local a = (d0 * c0 + d1 * c1) * d
						local b = (d4 * c0 + d2 * c1) * d
						local c = 1 - a - b
						clippedUVsItems[s] = u1 * a + u2 * b + u3 * c
						clippedUVsItems[s + 1] = v1 * a + v2 * b + v3 * c
						s = s + 2
						ii = ii + 2
					end

					s = #clippedTriangles + 1
					local clippedTrianglesItems = clippedTriangles
					clipOutputCount = clipOutputCount - 1
					ii = 1
					while ii < clipOutputCount do
						clippedTrianglesItems[s] = index
						clippedTrianglesItems[s + 1] = index + ii
						clippedTrianglesItems[s + 2] = index + ii + 1
						s = s + 3
						ii = ii + 1
					end
					index = index + clipOutputCount + 1
				end
			else
				local clippedVerticesItems = clippedVertices
				local clippedUVsItems = clippedUVs
				clippedVerticesItems[s] = x1
				clippedVerticesItems[s + 1] = y1
				clippedVerticesItems[s + 2] = x2
				clippedVerticesItems[s + 3] = y2
				clippedVerticesItems[s + 4] = x3
				clippedVerticesItems[s + 5] = y3

				clippedUVsItems[s] = u1
				clippedUVsItems[s + 1] = v1
				clippedUVsItems[s + 2] = u2
				clippedUVsItems[s + 3] = v2
				clippedUVsItems[s + 4] = u3
				clippedUVsItems[s + 5] = v3					

				s = #clippedTriangles + 1
				local clippedTrianglesItems = clippedTriangles
				clippedTrianglesItems[s] = index
				clippedTrianglesItems[s + 1] = index + 1
				clippedTrianglesItems[s + 2] = index + 2
				index = index + 3;
				break
			end
			p = p + 1
		end
		i = i + 3
	end
end

function SkeletonClipping:clip(x1, y1, x2, y2, x3, y3, clippingArea, output)
	local originalOutput = output
	local clipped = false
	local scratch = {}

	-- Avoid copy at the end.
	local input = nil
	if #clippingArea % 4 >= 2 then
		input = output
		output = scratch
	else
		input = scratch
	end

	table_insert(input, x1)
	table_insert(input, y1)
	table_insert(input, x2)
	table_insert(input, y2)
	table_insert(input, x3)
	table_insert(input, y3)
	table_insert(input, x1)
	table_insert(input, y1)

	local clippingVertices = clippingArea
	local clippingVerticesLast = #clippingArea - 4 + 1
	local i = 1
	while true do
		local edgeX = clippingVertices[i]
		local edgeY = clippingVertices[i + 1]
		local edgeX2 = clippingVertices[i + 2]
		local edgeY2 = clippingVertices[i + 3]
		local deltaX = edgeX - edgeX2
		local deltaY = edgeY - edgeY2

		local inputVertices = input
		local inputVerticesLength = #input - 2
		local outputStart = #output
		local ii = 1
		while ii <= inputVerticesLength do
			local inputX = inputVertices[ii]
			local inputY = inputVertices[ii + 1]
			local inputX2 = inputVertices[ii + 2]
			local inputY2 = inputVertices[ii + 3]
			local side2 = deltaX * (inputY2 - edgeY2) - deltaY * (inputX2 - edgeX2) > 0
			local continue = false;
			if deltaX * (inputY - edgeY2) - deltaY * (inputX - edgeX2) > 0 then
				if side2 then -- v1 inside, v2 inside
					table_insert(output, inputX2)
					table_insert(output, inputY2)
					continue = true
				else
					-- v1 inside, v2 outside
					local c0 = inputY2 - inputY
					local c2 = inputX2 - inputX
					local ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / (c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY))
					table_insert(output, edgeX + (edgeX2 - edgeX) * ua)
					table_insert(output, edgeY + (edgeY2 - edgeY) * ua)
				end
			elseif side2 then -- v1 outside, v2 inside
				local c0 = inputY2 - inputY
				local c2 = inputX2 - inputX
				local ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / (c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY))
				table_insert(output, edgeX + (edgeX2 - edgeX) * ua)
				table_insert(output, edgeY + (edgeY2 - edgeY) * ua)
				table_insert(output, inputX2)
				table_insert(output, inputY2)
			end			
			if not continue then clipped = true end
			ii = ii + 2
		end

		if outputStart == #output then -- All edges outside.
			for i, v in ipairs(originalOutput) do
				originalOutput[i] = nil
			end
			return true
		end

		table_insert(output, output[1])
		table_insert(output, output[2])

		if (i == clippingVerticesLast) then break end
		local temp = output
		output = input
		for i, v in ipairs(output) do
			output[i] = nil
		end
		input = temp
		i = i + 2
	end

	if originalOutput ~= output then
		for i, v in ipairs(originalOutput) do
			originalOutput[i] = nil
		end
		i = 1
		local n = #output - 2
		while i <= n do
			originalOutput[i] = output[i]
			i = i + 1
		end
	else
		utils.setArraySize(originalOutput, #originalOutput - 2)
	end

	return clipped
end

function SkeletonClipping:makeClockwise(polygon)
	local vertices = polygon
	local verticesLength = #polygon
	local area = vertices[verticesLength - 2 + 1] * vertices[1 + 1] - vertices[0 + 1] * vertices[verticesLength - 1 + 1]
	local p1x
	local p1y
	local p2x
	local p2y
	local i = 1
	local n = verticesLength - 3 + 1
	while i <= n do
		p1x = vertices[i]
		p1y = vertices[i + 1]
		p2x = vertices[i + 2]
		p2y = vertices[i + 3]
		area = area + p1x * p2y - p2x * p1y
		i = i + 2
	end
	if (area < 0) then return end

	i = 1
	local lastX = verticesLength - 2 + 1
	n = verticesLength / 2
	while i <= n do
		local x = vertices[i]
		local y = vertices[i + 1]
		local other = lastX - i + 1
		vertices[i] = vertices[other]
		vertices[i + 1] = vertices[other + 1]
		vertices[other] = x
		vertices[other + 1] = y
		i = i + 2
	end
end

return SkeletonClipping
