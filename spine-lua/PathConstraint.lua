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

local setmetatable = setmetatable
local math_pi = math.pi
local math_atan2 = math.atan2
local math_sqrt = math.sqrt
local math_acos = math.acos
local math_sin = math.sin
local table_insert = table.insert
local math_deg = math.deg
local math_rad = math.rad
local math_abs = math.abs

local PathConstraint = {}
PathConstraint.__index = PathConstraint

PathConstraint.NONE = -1
PathConstraint.BEFORE = -2
PathConstraint.AFTER = -3

function PathConstraint.new (data, skeleton)
	if not data then error("data cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end

	local self = {
		data = data,
		bones = {},
		target = skeleton:findSlot(data.target.name),
    position = data.position,
    spacing = data.spacing,
    rotateMix = data.rotateMix,
    translateMix = data.translateMix,
    spaces = {},
    positions = {},
    world = {},
    curves = {},
    lengths = {},
    segments = {}
	}
	setmetatable(self, PathConstraint)
  
  for i,bone in ipairs(data.bones) do
    table_insert(self.bones, skeleton:findBone(bone))
  end

	return self
end

function PathConstraint:apply ()
  self:update()
end

function PathConstraint:update ()
  
end

return PathConstraint