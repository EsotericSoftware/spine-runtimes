local M = {}

local function new(animationStateData)
    
    local animationState = {}
    local animationStateData = animationStateData
    
    local currentAnimation
    local previousAnimation
    local currentTime = 0
    local previousTime = 0
    local currentLoop
    local previousLoop
    local mixTime = 0
    local mixDuration = 0
    
    local animationQueue = {}
    -- queueObject for queue. Includes animation, loop boolean and delay
    -- Example-Entry = {animation = RunAnimation, loop = false, delay = 1}
    local function newEntry(animationName, loop, delay)
        return  {animation = animationStateData.getSkeletonData():findAnimation(animationName), loop = loop, delay = delay}
    end
    
    -- Used internally to set a new animation as currentAnimation
    local function setAnimationInternal(animation, loop)
        previousAnimation = nil
        if (not animation == nil) and ( not currentAnimation == nil) then
            mixDuration = animationData.getMix(currentAnimation.name,animation.name)
            if(mixDuration > 0) then
                mixTime = 0
                previousAnimation = currentAnimation
                previousTime = currentTime
                previousLoop = currentLoop
            end
        end
        currentAnimation = animation
        currentLoop = loop
        currentTime = 0
    end
    
    local function clearQueue()
        previousAnimation = nil
        currentAnimation = nil
        animationQueue = {}
    end
    
    local function update(delta)
        currentTime = currentTime + delta
        previousTime = previousTime + delta
        mixTime = mixTime + delta
        
        if(#animationQueue>0) then
            --get first element from queue
            local entry = animationQueue[1]
            if(currentTime >= entry.delay) then 
                --set this element as the current animation
                entry = table.remove(animationQueue, 1)
                setAnimationInternal(entry.animation, entry.loop)
            end
        end
    end
    
    local function apply(skeleton)
        if (currentAnimation == nil) then return end
        if (not previousAnimation == nil) then
            previousAnimation.apply(skeleton, previousTime, previousLoop)
            local alpha = mixTime / mixDuration
            if(alpha >= 1) then
                alpha = 1
                previousAnimation = nil
            end
            currentAnimation:mix(skeleton, currentTime, currentLoop, alpha)
        else
            currentAnimation:apply(skeleton, currentTime, currentLoop)
        end
    end
    
    -- Add animation with delay.
    -- May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay.
    local function addAnimationWithDelay(animationName, loop, delay)
        local totalDelay = delay
        
        if(delay <= 0)then
            local previousAnimation
            -- Find the animation that is queued before this one.
            if(table.getn(animationQueue)<=0)then
                previousAnimation = currentAnimation
            else
                previousAnimation = animationQueue[#animationQueue].animation
            end
            
            -- check that animation is not nil and calculate delay
            if (not (previousAnimation == nil)) then
                -- TODO: Make sure this functions as intended.
                totalDelay = previousAnimation.duration - animationStateData.getMix(previousAnimation.name, animationName) + delay
            else
                totalDelay = 0
            end
        end
        local entry = newEntry(animationName, loop, totalDelay)
        table.insert(animationQueue, entry)
    end
    
    local function addAnimation(animationName, loop)
        addAnimationWithDelay(animationName, loop, 0)
    end
    
    
    -- Clears the animationQueue and adds the animation to the empty queue.
    local function setAnimation(animationName, loop)
        clearQueue()
        setAnimationInternal(animationStateData.getSkeletonData():findAnimation(animationName), loop) 
    end
    
    local function getAnimation()
        return currentAnimation
    end
    
    -- Is current animation Complete
    local function isComplete()
        return currentAnimation == nil or currentTime >= currentAnimation.duration
    end
    
    -- Functions reachable from the state object
    animationState.update = update
    animationState.apply = apply
    animationState.isComplete = isComplete
    animationState.setAnimation = setAnimation
    animationState.getAnimation = getAnimation
    animationState.addAnimation = addAnimation
    animationState.addAnimationWithDelay = addAnimationWithDelay
    
    return animationState
    
end

M.new = new

return M