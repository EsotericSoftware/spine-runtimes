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

local utils = {}

local math_sqrt = math.sqrt
local math_random = math.random

utils.degRad = math.pi / 180

function tablePrint (tt, indent, done)
	done = done or {}
	for key, value in pairs(tt) do
		local spaces = string.rep (" ", indent)
		if type(value) == "table" and not done [value] then
			done [value] = true
			print(spaces .. "{")
			utils.print(value, indent + 2, done)
			print(spaces .. "}")
		else
			io.write(spaces .. tostring(key) .. " = ")
			utils.print(value, indent + 2, done)
		end
	end
end

function utils.print (value, indent, done)
	indent = indent or 0
	if "nil" == type(value) then
		print(tostring(nil))
	elseif "table" == type(value) then
		local spaces = string.rep (" ", indent)
		print(spaces .. "{")
		tablePrint(value, indent + 2)
		print(spaces .. "}")
	elseif "string" == type(value) then
		print("\"" .. value .. "\"")
	else
		print(tostring(value))
	end
end

function utils.indexOf (haystack, needle)
	for i,value in ipairs(haystack) do
		if value == needle then return i end
	end
	return nil
end

function utils.copy (from, to)
	if not to then to = {} end
	for k,v in pairs(from) do
		to[k] = v
	end
	return to
end

function utils.newNumberArray (size)
	local a = {}
	local i = 1
	while i <= size do
		a[i] = 0
		i = i + 1
	end
	return a
end

function utils.newNumberArrayZero (size)
	local a = {}
	local i = 0
	while i < size do
		a[i] = 0
		i = i + 1
	end
	return a
end

function utils.setArraySize (array, size)
	if #array == size then return array end
	if #array < size then
		local i = #array + 1
		while i <= size do
			array[i] = 0
			i = i + 1
		end
	else
		local originalSize = #array
		local i = originalSize
		while i > size do
			array[i] = nil -- dirty trick to appease # without realloc
			i = i - 1
		end
	end
	return array
end

function utils.arrayCopy (src, srcOffset, dst, dstOffset, size)
	local n = srcOffset + size
	while srcOffset < n do
		dst[dstOffset] = src[srcOffset]
		dstOffset = dstOffset + 1
		srcOffset = srcOffset + 1
	end
end

function utils.arrayContains(array, element)
	for i, arrayElement in ipairs(array) do
		if arrayElement == element then return true end
	end
	return false
end

function utils.clamp (value, min, max)
	if value < min then return min end
	if value > max then return max end
	return value
end

function utils.signum (value)
	if value < 0 then
		return -1
	elseif value > 0 then
		return 1
	else
		return 0
	end
end

-- Implements Java float modulo
function utils.mod(a, b)
	if b < 0 then b = -b end
	if a < 0 then
		return -(-a % b)
	else
		return a % b
	end
end

function utils.randomTriangular(min, max)
	return utils.randomTriangularWith(min, max, (min + max) * 0.5)
end

function utils.randomTriangularWith(min, max, mode)
	local u = math.random()
	local d = max - min
	if (u <= (mode - min) / d) then return min + math_sqrt(u * d * (mode - min)) end
	return max - math_sqrt((1 - u) * d * (max - mode))
end

function utils.testBit(value, bit)
	if (value == nil) then return 0 end
	return value % (2 * bit) >= bit
end

function utils.setBit(value, bit)
	if (value == nil) then return 0 end
	if value % (2 * bit) >= bit then
		return value
	end
	return value + bit
end

function utils.clearBit(value, bit)
	if (value == nil) then return 0 end
	if value % (2 * bit) >= bit then
		return value - bit
	end
	return value
end

return utils
