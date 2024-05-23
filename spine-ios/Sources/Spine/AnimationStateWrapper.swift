import Foundation
import SpineCppLite

public typealias AnimationStateListener = (_ type: EventType, _ entry: TrackEntry, _ event: Event?) -> Void

public final class AnimationStateWrapper {
    
    public let animationState: AnimationState
    public let aninationStateEvents: AnimationStateEvents
    
    private var trackEntryListeners = [spine_track_entry: AnimationStateListener]()
    
    private var stateListener: AnimationStateListener?
    
    public init(animationState: AnimationState, aninationStateEvents: AnimationStateEvents) {
        self.animationState = animationState
        self.aninationStateEvents = aninationStateEvents
    }
    
    public func setTrackEntryListener(entry: TrackEntry, listener: AnimationStateListener?) {
        if let listener {
            trackEntryListeners[entry.wrappee] = listener
        } else {
            trackEntryListeners.removeValue(forKey: entry.wrappee)
        }
    }
    
    public func update(delta: Float) {
        animationState.update(delta: delta)
        
        let numEvents = spine_animation_state_events_get_num_events(aninationStateEvents.wrappee)
        for i in 0..<numEvents {
            let type = aninationStateEvents.getEventType(index: i)
            
            let entry = aninationStateEvents.getTrackEntry(index: i)
            let event = aninationStateEvents.getEvent(index: i)
            
            if let trackEntryListener = trackEntryListeners[entry.wrappee] {
                trackEntryListener(type, entry, event)
            }
            if let stateListener {
                stateListener(type, entry, event)
            }
            if type == SPINE_EVENT_TYPE_DISPOSE {
                spine_animation_state_dispose_track_entry(animationState.wrappee, entry.wrappee)
            }
        }
        aninationStateEvents.reset()
    }
    
    public func setStateListener(_ stateListener: AnimationStateListener?) {
        self.stateListener = stateListener
    }
}

