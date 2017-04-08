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

local TextureAtlasRegion = require "spine-lua.TextureAtlasRegion"
local TextureWrap = require "spine-lua.TextureWrap"
local TextureFilter = require "spine-lua.TextureFilter"

local setmetatable = setmetatable
local tonumber = tonumber
local math_abs = math.abs
local string_sub = string.sub
local string_gmatch = string.gmatch
local string_match = string.match
local string_lower = string.lower
local string_find = string.find
local string_len = string.len

local TextureAtlasPage = {}

function TextureAtlasPage.new ()
	local self = {
		name = nil,
		minFilter = nil,
		magFilter = nil,
		uWrap = nil,
		vWrap = nil,
		texture = nil,
		width = 0,
		height = 0
	}
	return self
end


local TextureAtlas = {}
TextureAtlas.__index = TextureAtlas

function TextureAtlas.new (atlasContent, imageLoader)
	local self = {
		pages = {},
		regions = {}
	}
	setmetatable(self, TextureAtlas)

	self:parse(atlasContent, imageLoader)

	return self
end

local function lineIterator(s)
	if string_sub(s, -1)~="\n" then s=s.."\n" end
	return string_gmatch(s, "(.-)\n")
end

local readLine = function (indexArray, numLines, lines)
	local index = indexArray[1]
	if index >= numLines then return nil end
	local line = lines[index]
	index = index + 1
	indexArray[1] = index
	return line
end

local readValue = function (indexArray, numLines, lines)
	local index = indexArray[1]
	local line, newIndex = readLine(indexArray, numLines, lines)
	local idx = string_find(line, ":")
	if not idx then error("Invalid line: " .. line, 2) end
	return string_match(string_sub(line, idx + 1),'^%s*(.*%S)') or ''
end

local readTuple = function (indexArray, numLines, lines)
	local index = indexArray[1]
	local line, newIndex = readLine(indexArray, numLines, lines)
	local idx = string_find(line, ":")
	if not idx then 
		error("Invalid line: " .. line, 2)
	end
	local i = 1
	local lastMatch = idx + 1
	local tuple = {}
	while i <= 3 do
		local comma = string_find(line, ",", lastMatch)
		if not comma then break end
		tuple[i] = string_match(string_sub(line, lastMatch, comma - 1), '^%s*(.*%S)') or ''
		lastMatch = comma + 1
		i = i + 1
	end
	tuple[i] = string_match(string_sub(line, lastMatch), '^%s*(.*%S)') or ''
	return tuple
end

local parseInt = function (str)
	return tonumber(str)
end


local TextureFilter_Nearest = TextureFilter.Nearest
local TextureFilter_Linear = TextureFilter.Linear
local TextureFilter_MipMap = TextureFilter.MipMap
local TextureFilter_MipMapNearestNearest = TextureFilter.MipMapNearestNearest
local TextureFilter_MipMapLinearNearest = TextureFilter.MipMapLinearNearest
local TextureFilter_MipMapNearestLinear = TextureFilter.MipMapNearestLinear
local TextureFilter_MipMapLinearLinear = TextureFilter.MipMapLinearLinear
local filterFromString = function (str)
	str = string_lower(str)
	if str == "nearest" then return TextureFilter_Nearest
	elseif str == "linear" then return TextureFilter_Linear
	elseif str == "mipmap" then return TextureFilter_MipMap
	elseif str == "mipmapnearestnearest" then return TextureFilter_MipMapNearestNearest
	elseif str == "mipmaplinearnearest" then return TextureFilter_MipMapLinearNearest
	elseif str == "mipmapnearestlinear" then return TextureFilter_MipMapNearestLinear
	elseif str == "mipmaplinearlinear" then return TextureFilter_MipMapLinearLinear
	else error("Unknown texture wrap: " .. str, 2)
	end
end

function TextureAtlas:parse (atlasContent, imageLoader)
	if not atlasContent then error("atlasContent cannot be nil.", 2) end
	if not imageLoader then error("imageLoader cannot be nil.", 2) end

	local lines = {}
	local indexArray = {0}
	local numLines = 0
	for line in lineIterator(atlasContent) do
		lines[numLines] = line
		numLines = numLines + 1
	end

	local page = nil
	while true do
		local line = readLine(indexArray, numLines, lines)
		if not line then break end
		line = string_match(line, '^%s*(.*%S)') or ''
		if string_len(line) == 0 then
			page = nil
		elseif not page then
			page = TextureAtlasPage.new()
			page.name = line

			local tuple = readTuple(indexArray, numLines, lines)
			if #tuple == 2 then
				page.width = parseInt(tuple[1])
				page.height = parseInt(tuple[2])
				tuple = readTuple(indexArray, numLines, lines)
			else
				-- We only support atlases that have the page width/height
				-- encoded in them. That way we don't rely on any special
				-- wrapper objects for images to get the page size from
				error("Atlas must specify page width/height. Please export to the latest atlas format", 2)
			end

			tuple = readTuple(indexArray, numLines, lines)
			page.minFilter = filterFromString(tuple[1])
			page.magFilter = filterFromString(tuple[2])

			local direction = readValue(indexArray, numLines, lines)
			page.uWrap = TextureWrap.ClampToEdge
			page.vWrap = TextureWrap.ClampToEdge
			if direction == "x" then
				page.uWrap = TextureWrap.Repeat
			elseif direction == "y" then
				page.vWrap = TextureWrap.Repeat
			elseif direction == "xy" then
				page.uWrap = TextureWrap.Repeat
				page.vWrap = TextureWrap.Repeat
			end

			page.texture = imageLoader(line)
			-- FIXME page.texture:setFilters(page.minFilter, page.magFilter)
			-- FIXME page.texture:setWraps(page.uWrap, page.vWrap)
			local pages = self.pages
			pages[#pages + 1] = page
		else
			local region = TextureAtlasRegion.new()
			region.name = line
			region.page = page

			if readValue(indexArray, numLines, lines) == "true" then region.rotate = true end

			local tuple = readTuple(indexArray, numLines, lines)
			local x = parseInt(tuple[1])
			local y = parseInt(tuple[2])

			tuple = readTuple(indexArray, numLines, lines)
			local width = parseInt(tuple[1])
			local height = parseInt(tuple[2])

			region.u = x / page.width
			region.v = y / page.height
			if region.rotate then
				region.u2 = (x + height) / page.width
				region.v2 = (y + width) / page.height
			else
				region.u2 = (x + width) / page.width
				region.v2 = (y + height) / page.height
			end

			region.x = x
			region.y = y
			region.width = math_abs(width)
			region.height = math_abs(height)

			-- Read and skip optional splits
			tuple = readTuple(indexArray, numLines, lines)
			if #tuple == 4 then
				tuple = readTuple(indexArray, numLines, lines)
				if #tuple == 4 then
					local dummyTuple = readTuple(indexArray, numLines, lines)
				end
			end

			region.originalWidth = parseInt(tuple[1])
			region.originalHeight = parseInt(tuple[2])

			tuple = readTuple(indexArray, numLines, lines)
			region.offsetX = parseInt(tuple[1])
			region.offsetY = parseInt(tuple[2])

			region.index = parseInt(readValue(indexArray, numLines, lines))
			region.texture = page.texture
			local regions = self.regions
			regions[#regions + 1] = region
		end
	end
end

function TextureAtlas:findRegion(name)
	local regions = self.regions
	for i=1, #regions do
		local region = regions[i]
		if region.name == name then return region end
	end
	return nil
end

function TextureAtlas:dispose()
	local self_pairs = self.pairs
	for i=1, #self_pairs do
		local page = self_pairs[i]
		-- FIXME implement disposing of pages
		-- love2d doesn't support manual disposing
	end
end

return TextureAtlas
