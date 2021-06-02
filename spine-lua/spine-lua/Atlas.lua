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

local Atlas = {}

function Atlas.parse(atlasPath, atlasBase)
	local function parseIntTuple4( l )
		local a,b,c,d = string.match( l , " ? ?%a+: ([+-]?%d+), ?([+-]?%d+), ?([+-]?%d+), ?([+-]?%d+)" )
		a,b,c,d = tonumber( a ), tonumber( b ), tonumber( c ), tonumber( d )
		return a and b and c and d and {a, b, c ,d}
	end

	local function parseIntTuple2( l )
		local a,b = string.match( l , " ? ?%a+: ([+-]?%d+), ?([+-]?%d+)" )
		a,b = tonumber( a ), tonumber( b )
		return a and b and {a, b}
	end

	if not atlasPath then
		error("Error: " .. atlasPath .. ".atlas" .. " doesn't exist!")
		return nil
	end

	local atlasLines = spine.utils.readFile( atlasPath, atlasBase )
	if not atlasLines then
		error("Error: " .. atlasPath .. ".atlas" .. " unable to read!")
		return nil
	end

	local pages = {}


	local it = string.gmatch(atlasLines, "(.-)\r?\n") -- iterate over lines
	for l in it do
		if #l == 0 then
			l = it()
			if l then
				local page = { name = l }
				l = it()
				page.size = parseIntTuple2( l )
				if page.size then
					l = it()
				end
				page.format = string.match( l, "%a+: (.+)" )
				page.filter = {string.match( it(), "%a+: (.+),(.+)" )}
				page.wrap = string.match( it(), "%a+: (.+)" )
				page.regions = {}
				table.insert( pages, page )
			else
				break
			end
		else
			local region = {name = l}

			region.rotate = string.match( it(), "%a+: (.+)" ) == "true"
			region.xy = parseIntTuple2( it() )
			region.size = parseIntTuple2( it() )
			l = it()
			region.splits = parseIntTuple4(l)
			if region.splits then
				l = it()
				region.pad = parseIntTuple4(l)
				if region.pad then
					l = it()
				end
			end
			region.orig = parseIntTuple2( l )
			region.offset = parseIntTuple2( it() )
			region.index = tonumber( string.match( it() , "%a+: ([+-]?%d+)" ) )

			table.insert( pages[#pages].regions, region )
		end
	end

	return pages
end

return Atlas
