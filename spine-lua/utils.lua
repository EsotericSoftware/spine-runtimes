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

local utils = {}

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

return utils
