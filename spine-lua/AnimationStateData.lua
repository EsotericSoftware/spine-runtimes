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

local AnimationStateData = {}

function AnimationStateData.new (skeletonData)
	if not skeletonData then error("skeletonData cannot be nil", 2) end

	local self = {
		skeletonData = skeletonData,
		animationToMixTime = {},
		defaultMix = 0
	}

	function self:setMix (fromName, toName, duration)
		if not self.animationToMixTime[fromName] then
			self.animationToMixTime[fromName] = {}
		end
		self.animationToMixTime[fromName][toName] = duration
	end

	function self:getMix (fromName, toName)
		local first = self.animationToMixTime[fromName]
		if not first then return self.defaultMix end
		local duration = first[toName]
		if not duration then return self.defaultMix end
		return duration
	end

	return self
end
return AnimationStateData
