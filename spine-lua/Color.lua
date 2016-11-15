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

local setmetatable = setmetatable
local utils = require "spine-lua.utils"

local Color = {}
Color.__index = Color

function Color.new ()
	local self = {
		r = 0, g = 0, b = 0, a = 0
	}
	setmetatable(self, Color)

	return self
end

function Color.newWith (r, g, b, a)
	local self = {
		r = r, g = g, b = b, a = a
	}
	setmetatable(self, Color)

	return self
end

function Color:set(r, g, b, a)
	self.r = r
	self.g = g
	self.b = b
	self.a = a
end

function Color:setFrom(color)
	self.r = color.r
	self.g = color.g
	self.b = color.b
	self.a = color.a
end

function Color:add(r, g, b, a)
	self.r = self.r + r
	self.g = self.g + g
	self.b = self.b + b
	self.a = self.a + a
	self:clamp()
end

function Color:clamp()
	self.r = utils.clamp(self.r, 0, 1)
	self.g = utils.clamp(self.g, 0, 1)
	self.b = utils.clamp(self.b, 0, 1)
	self.a = utils.clamp(self.a, 0, 1)
end

return Color
