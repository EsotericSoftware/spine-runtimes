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

local setmetatable = setmetatable
local math_min = math.min
local math_max = math.max
local ipairs = ipairs
local table_insert = table.insert
local table_remove = table.remove

local Triangulator = {}
Triangulator.__index = Triangulator

function Triangulator.new ()
	local self = {
		convexPolygons = {},
		convexPolygonsIndices = {},
		indicesArray = {},
		isConcaveArray = {},
		triangles = {}
	}
	setmetatable(self, Triangulator)

	return self
end

function Triangulator:triangulate (verticesArray)
	local vertices = verticesArray
	local vertexCount = #verticesArray / 2

	self.indicesArray = {}
	local indicesArray = self.indicesArray
	local indices = utils.setArraySize(indicesArray, vertexCount)
	local i = 0
	while i < vertexCount do
		indices[i] = i
		i = i + 1
	end

	local isConcaveArray = self.isConcaveArray
	local isConcave = isConcaveArray
	i = 0
	while i < vertexCount do
		isConcave[i] = self:isConcave(i, vertexCount, vertices, indices)
		i = i + 1
	end

	self.triangles = {}
	local triangles = self.triangles;

	while vertexCount > 3 do
		-- Find ear tip.
		local previous = vertexCount - 1
		local i = 0
		local _next = 1
		while true do
			local goToHead = false
			local breakLoop = false
			if not isConcave[i] then
				local p1 = indices[previous] * 2 + 1
				local p2 = indices[i] * 2 + 1
				local p3 = indices[_next] * 2 + 1
				local p1x = vertices[p1]
				local p1y = vertices[p1 + 1]
				local p2x = vertices[p2]
				local p2y = vertices[p2 + 1]
				local p3x = vertices[p3]
				local p3y = vertices[p3 + 1]
				local ii = ((_next + 1) % vertexCount)
				while ii ~= previous do
					if isConcave[ii] then
						local v = indices[ii] * 2 + 1
						local vx = vertices[v]
						local vy = vertices[v + 1]
						if self:positiveArea(p3x, p3y, p1x, p1y, vx, vy) then
							if self:positiveArea(p1x, p1y, p2x, p2y, vx, vy) then
								if self:positiveArea(p2x, p2y, p3x, p3y, vx, vy) then
									goToHead = true
									break
								end
							end
						end
					end
					ii = (ii + 1) % vertexCount
				end
				if (not goToHead) then 
					breakLoop = true
					break
				end
			end
			
			if breakLoop then break end

			if _next == 0 then
				repeat
					if not isConcave[i] then
						break;
					end
					i = i - 1
				until i == 0
				break
			end

			previous = i
			i = _next
			_next = (_next + 1) % vertexCount
		end

		-- Cut ear tip.
		table_insert(triangles, indices[(vertexCount + i - 1) % vertexCount] + 1)
		table_insert(triangles, indices[i] + 1)
		table_insert(triangles, indices[(i + 1) % vertexCount] + 1)
		if i == 0 then
			local ii = 1
			while ii <= #indicesArray do
				indicesArray[ii-1] = indicesArray[ii]
				isConcaveArray[ii-1] = isConcaveArray[ii]
				ii = ii + 1
			end
		else
			table_remove(indicesArray, i)
			table_remove(isConcaveArray, i)
		end
		vertexCount = vertexCount - 1

		local previousIndex = (vertexCount + i - 1) % vertexCount
		local nextIndex = i
		if i == vertexCount then nextIndex = 0 end
		isConcave[previousIndex] = self:isConcave(previousIndex, vertexCount, vertices, indices)
		isConcave[nextIndex] = self:isConcave(nextIndex, vertexCount, vertices, indices)
	end

	if vertexCount == 3 then
		table_insert(triangles, indices[2] + 1)
		table_insert(triangles, indices[0] + 1)
		table_insert(triangles, indices[1] + 1)
	end

	return triangles
end

function Triangulator:isConcave(index, vertexCount, vertices, indices)
	local previous = indices[(vertexCount + index - 1) % vertexCount] * 2 + 1;
	local current = indices[index] * 2 + 1;
	local _next = indices[(index + 1) % vertexCount] * 2 + 1;
	return not self:positiveArea(vertices[previous], vertices[previous + 1], vertices[current], vertices[current + 1], vertices[_next],vertices[_next + 1]);
end

function Triangulator:positiveArea(p1x, p1y, p2x, p2y, p3x, p3y)
	return p1x * (p3y - p2y) + p2x * (p1y - p3y) + p3x * (p2y - p1y) >= 0
end

function Triangulator:winding(p1x, p1y, p2x, p2y, p3x, p3y)
	local px = p2x - p1x
	local py = p2y - p1y
	if p3x * py - p3y * px + px * p1y - p1x * py >= 0 then
		return 1
	else
		return -1;
	end
end

return Triangulator
