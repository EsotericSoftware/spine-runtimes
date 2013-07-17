local M = {}

local function new(skeletonData)
    local animationData = {}
    
    --Stores information about mixing from one animation to another.
    -- Example:
    -- animationToMixTime = {
    --      "fromName" = {
    --          "toName1" = 0.3
    --          "toName2" = 0.4
    --          "toName3" = 0.5
    --      },
    --      "Jump" = {
    --          "Run" = 0.4
    --      },
    --      ...
    -- }
    -- To get mix time from one animation to another: animationMixTIme[fromName][toName]
    local animationToMixTime = {}
    local skeletonData = skeletonData
    local defaultMix = 0
    
    local function setMix(fromName, toName, duration)
        if (animationToMixTime[fromName] == nil) then
            animationToMixTime[fromName] = {}
        end
        animationToMixTime[fromName][toName] = duration
    end
    
    local function getMix(fromName, toName)
        -- returns defaultMix if fromName-toName pair is not defined.
        if (animationToMixTime[fromName]== nil) then return defaultMix end
        local duration = animationToMixTime[fromName][toName]
        if (duration == nil) then return defaultMix end
        return duration
    end
    
    local function setDefaultMix(mix)
        defaultMix = mix
    end
    
    local function getDefaultMix()
        return defaultMix
    end
    
    local function getSkeletonData()
        return skeletonData
    end
    
    animationData.setMix = setMix
    animationData.getMix = getMix
    animationData.setDefaultMix = setDefaultMix
    animationData.getDefaultMix = getDefaultMix
    animationData.getSkeletonData = getSkeletonData
    
    return animationData
end

M.new = new

return M