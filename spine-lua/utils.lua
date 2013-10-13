------------------------------------------------------------------------------
 -- Spine Runtime Software License - Version 1.0
 -- 
 -- Copyright (c) 2013, Esoteric Software
 -- All rights reserved.
 -- 
 -- Redistribution and use in source and binary forms in whole or in part, with
 -- or without modification, are permitted provided that the following conditions
 -- are met:
 -- 
 -- 1. A Spine Essential, Professional, Enterprise, or Education License must
 --    be purchased from Esoteric Software and the license must remain valid:
 --    http://esotericsoftware.com/
 -- 2. Redistributions of source code must retain this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer.
 -- 3. Redistributions in binary form must reproduce this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer, in the documentation and/or other materials provided with the
 --    distribution.
 -- 
 -- THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 -- ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 -- WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 -- DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 -- ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 -- (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 -- LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 -- ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 -- (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 -- SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ------------------------------------------------------------------------------

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
