-------------------------------------------------------------------------------
-- Spine Runtimes License Agreement
-- Last updated January 1, 2020. Replaces all prior versions.
--
-- Copyright (c) 2013-2020, Esoteric Software LLC
--
-- Integration of the Spine Runtimes into software or otherwise creating
-- derivative works of the Spine Runtimes is permitted under the terms and
-- conditions of Section 2 of the Spine Editor License Agreement:
-- http:--esotericsoftware.com/spine-editor-license
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
local tonumber = tonumber
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
		height = 0,
		pma = false
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

	local readLine = atlasContent:gmatch("[ \t]*(.-)[ \t]*\r?\n")

	local trim = function (value)
		return value:match("^%s*(.-)%s*$")
	end

	local entry = {}
	local readEntry = function (entry, line)
		if not line then return 0 end
		if line:len() == 0 then return 0 end

		local colon = line:find(":")
		if not colon then return 0 end
		entry[0] = trim(line:sub(1, colon))
		local lastMatch = colon + 1
		local i = 1
		while true do
			local comma = line:find(",", lastMatch)
			if not comma then
				entry[i] = trim(line:sub(lastMatch))
				return i
			end
			entry[i] = trim(line:sub(lastMatch, comma - lastMatch))
			lastMatch = comma + 1
			if i == 4 then return 4 end
			i = i + 1
		end
	end

	local page
	local region

	local pageFields = {}
	pageFields["size"] = function ()
		page.width = tonumber(entry[1])
		page.height = tonumber(entry[2])
	end
	pageFields["format"] = function ()
		-- page.format = Format[tuple[0]] we don't need format in Lua
	end
	pageFields["filter"] = function ()
		page.minFilter = TextureFilter[entry[1]]
		page.magFilter = TextureFilter[entry[2]]
	end
	pageFields["repeat"] = function ()
		if entry[1]:find("x") then page.uWrap = TextureWrap.Repeat end
		if entry[1]:find("y") then page.vWrap = TextureWrap.Repeat end
	end
	pageFields["pma"] = function ()
		page.pma = entry[1] == "true"
	end

	local regionFields = {}
	regionFields["xy"] = function () -- Deprecated, use bounds.
		region.x = tonumber(entry[1])
		region.y = tonumber(entry[2])
	end
	regionFields["size"] = function () -- Deprecated, use bounds.
		region.width = tonumber(entry[1])
		region.height = tonumber(entry[2])
	end
	regionFields["bounds"] = function ()
		region.x = tonumber(entry[1])
		region.y = tonumber(entry[2])
		region.width = tonumber(entry[3])
		region.height = tonumber(entry[4])
	end
	regionFields["offset"] = function () -- Deprecated, use offsets.
		region.offsetX = tonumber(entry[1])
		region.offsetY = tonumber(entry[2])
	end
	regionFields["orig"] = function () -- Deprecated, use offsets.
		region.originalWidth = tonumber(entry[1])
		region.originalHeight = tonumber(entry[2])
	end
	regionFields["offsets"] = function ()
		region.offsetX = tonumber(entry[1])
		region.offsetY = tonumber(entry[2])
		region.originalWidth = tonumber(entry[3])
		region.originalHeight = tonumber(entry[4])
	end
	regionFields["rotate"] = function ()
		local value = entry[1]
		if value == "true" then
			region.degrees = 90
		elseif value ~= "false" then
			region.degrees = tonumber(value)
		end
	end
	regionFields["index"] = function ()
		region.index = tonumber(entry[1])
	end

	local line = readLine()
	-- Ignore empty lines before first entry.
	while line and line:len() == 0 do
		line = readLine()
	end
	-- Header entries.
	while true do
		if not line or line:len() == 0 then break end
		if readEntry(entry, line) == 0 then break end -- Silently ignore all header fields.
		line = readLine()
	end

	-- Page and region entries.
	local names
	local values
	while true do
		if not line then break end
		if line:len() == 0 then
			page = nil
			line = readLine()
		elseif not page then
			page = TextureAtlasPage.new()
			page.name = trim(line)
			while true do
				line = readLine()
				if readEntry(entry, line) == 0 then break end
				local field = pageFields[entry[0]]
				if field then field() end
			end
			page.texture = imageLoader(page.name)
			-- FIXME - Apply the filter and wrap settings to the texture.
			-- page.texture:setFilters(page.minFilter, page.magFilter)
			-- page.texture:setWraps(page.uWrap, page.vWrap)
			table_insert(self.pages, page)
		else
			region = TextureAtlasRegion.new()
			region.page = page
			region.name = line
			while true do
				line = readLine()
				local count = readEntry(entry, line)
				if count == 0 then break end
				local field = regionFields[entry[0]]
				if field then
					field()
				else
					if not names then
						names = {}
						values = {}
					end
					table_insert(names, entry[0])
					local entryValues = {}
					local i = 0
					while i < count do
						table_insert(entryValues, tonumber(entry[i + 1]))
						 i = i + 1
					end
					table_insert(values, entryValues)
				end
			end
			if region.originalWidth == 0 and region.originalHeight == 0 then
				region.originalWidth = region.width
				region.originalHeight = region.height
			end
			if names and #names > 0 then
				region.names = names
				region.values = values
				names = nil
				values = nil
			end
			region.u = region.x / page.width
			region.v = region.y / page.height
			if region.degrees == 90 then
				region.u2 = (region.x + region.height) / page.width
				region.v2 = (region.y + region.width) / page.height
			else
				region.u2 = (region.x + region.width) / page.width
				region.v2 = (region.y + region.height) / page.height
			end
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
