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
