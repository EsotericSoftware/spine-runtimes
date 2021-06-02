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
local table_insert = table.insert
local math_abs = math.abs

local TextureAtlasRegion = require "spine-lua.TextureAtlasRegion"
local TextureWrap = require "spine-lua.TextureWrap"
local TextureFilter = require "spine-lua.TextureFilter"

local TextureAtlasPage = {}
TextureAtlasPage.__index = TextureAtlasPage

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
	setmetatable(self, TextureAtlasPage)
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

function TextureAtlas:parse (atlasContent, imageLoader)
	if not atlasContent then error("atlasContent cannot be nil.", 2) end
	if not imageLoader then error("imageLoader cannot be nil.", 2) end

	function lineIterator(s)
		if s:sub(-1)~="\n" then s=s.."\n" end
		return s:gmatch("(.-)\n")
	end

	local lines = {}
	local index = 0
	local numLines = 0
	for line in lineIterator(atlasContent) do
		lines[numLines] = line
		numLines = numLines + 1
	end

	local readLine = function ()
		if index >= numLines then return nil end
		local line = lines[index]
		index = index + 1
		return line
	end

	local readValue = function ()
		local line = readLine()
		local idx = line:find(":")
		if not idx then error("Invalid line: " .. line, 2) end
		return line:sub(idx + 1):match'^%s*(.*%S)' or ''
	end

	local readTuple = function ()
		local line = readLine()
		local idx = line:find(":")
		if not idx then
			error("Invalid line: " .. line, 2)
		end
		local i = 1
		local lastMatch = idx + 1
		local tuple = {}
		while i <= 3 do
			local comma = line:find(",", lastMatch)
			if not comma then break end
			tuple[i] = line:sub(lastMatch, comma - 1):match'^%s*(.*%S)' or ''
			lastMatch = comma + 1
			i = i + 1
		end
		tuple[i] = line:sub(lastMatch):match'^%s*(.*%S)' or ''
		return tuple
	end

	local parseInt = function (str)
		return tonumber(str)
	end

	local filterFromString = function (str)
		str = str:lower()
		if str == "nearest" then return TextureFilter.Nearest
		elseif str == "linear" then return TextureFilter.Linear
		elseif str == "mipmap" then return TextureFilter.MipMap
		elseif str == "mipmapnearestnearest" then return TextureFilter.MipMapNearestNearest
		elseif str == "mipmaplinearnearest" then return TextureFilter.MipMapLinearNearest
		elseif str == "mipmapnearestlinear" then return TextureFilter.MipMapNearestLinear
		elseif str == "mipmaplinearlinear" then return TextureFilter.MipMapLinearLinear
		else error("Unknown texture wrap: " .. str, 2)
		end
	end

	local page = nil
	while true do
		local line = readLine()
		if not line then break end
		line = line:match'^%s*(.*%S)' or ''
		if line:len() == 0 then
			page = nil
		elseif not page then
			page = TextureAtlasPage.new()
			page.name = line

			local tuple = readTuple()
			if #tuple == 2 then
				page.width = parseInt(tuple[1])
				page.height = parseInt(tuple[2])
				tuple = readTuple()
			else
				-- We only support atlases that have the page width/height
				-- encoded in them. That way we don't rely on any special
				-- wrapper objects for images to get the page size from
				error("Atlas must specify page width/height. Please export to the latest atlas format", 2)
			end

			tuple = readTuple()
			page.minFilter = filterFromString(tuple[1])
			page.magFilter = filterFromString(tuple[2])

			local direction = readValue()
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
			table_insert(self.pages, page)
		else
			local region = TextureAtlasRegion.new()
			region.name = line
			region.page = page

			local rotateValue = readValue()
			if rotateValue == "true" then
				region.degrees = 90
			elseif rotateValue == "false" then
				region.degrees = 0
			else
				region.degrees = tonumber(rotateValue)
			end
			if region.degrees == 90 then region.rotate = true end

			local tuple = readTuple()
			local x = parseInt(tuple[1])
			local y = parseInt(tuple[2])

			tuple = readTuple()
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
			tuple = readTuple()
			if #tuple == 4 then
				tuple = readTuple()
				if #tuple == 4 then
					readTuple()
				end
			end

			region.originalWidth = parseInt(tuple[1])
			region.originalHeight = parseInt(tuple[2])

			tuple = readTuple()
			region.offsetX = parseInt(tuple[1])
			region.offsetY = parseInt(tuple[2])

			region.index = parseInt(readValue())
			region.texture = page.texture
			table_insert(self.regions, region)
		end
	end
end

function TextureAtlas:findRegion(name)
	for _, region in ipairs(self.regions) do
		if region.name == name then return region end
	end
	return nil
end

function TextureAtlas:dispose()
	for _, page in ipairs(self.pairs) do
		-- FIXME implement disposing of pages
		-- love2d doesn't support manual disposing
	end
end

return TextureAtlas
