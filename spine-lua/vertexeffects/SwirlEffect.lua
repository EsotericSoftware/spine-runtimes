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

local utils = require "spine-lua.utils"
local interpolation = require "spine-lua.Interpolation"

local setmetatable = setmetatable
local math_min = math.min
local math_max = math.max
local ipairs = ipairs
local table_insert = table.insert
local table_remove = table.remove
local utils_deg_rad = utils.degRad
local math_sqrt = math.sqrt
local math_sin = math.sin
local math_cos = math.cos

local SwirlEffect = {}
SwirlEffect.__index = SwirlEffect

function SwirlEffect.new (radius)
	local self = {
		worldX = 0,
		worldY = 0,
		centerX = 0,
		centerY = 0,
		radius = radius,
		angle = 0,
		interpolation = interpolation.pow2
	}
	setmetatable(self, SwirlEffect)

	return self
end

function SwirlEffect:beginEffect (skeleton)
	self.worldX = skeleton.x + self.centerX
	self.worldY = skeleton.y + self.centerY
	self.angleRad = self.angle * utils_deg_rad
end

function SwirlEffect:transform (vertex)
	local x = vertex.x - self.worldX
	local y = vertex.y - self.worldY
	local dist = math_sqrt(x * x + y * y)
	if (dist < self.radius) then
		local theta = interpolation.apply(self.interpolation, 0, self.angleRad, (self.radius - dist) / self.radius)
		local cos = math_cos(theta)
		local sin = math_sin(theta)
		vertex.x = cos * x - sin * y + self.worldX
		vertex.y = sin * x + cos * y + self.worldY
	end
end

function SwirlEffect:endEffect ()
end

return SwirlEffect
