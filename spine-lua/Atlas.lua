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

local Atlas = {}

function Atlas.parse(atlasPath, atlasBase)
	local function parseIntTuple4( l )
		local a,b,c,d = string.match( l , " ? ?%a+: ([+-]?%d+), ?([+-]?%d+), ?([+-]?%d+), ?([+-]?%d+)" )
		local a,b,c,d = tonumber( a ), tonumber( b ), tonumber( c ), tonumber( d )
		return a and b and c and d and {a, b, c ,d}
	end

	local function parseIntTuple2( l )
		local a,b = string.match( l , " ? ?%a+: ([+-]?%d+), ?([+-]?%d+)" )
		local a,b = tonumber( a ), tonumber( b )
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
