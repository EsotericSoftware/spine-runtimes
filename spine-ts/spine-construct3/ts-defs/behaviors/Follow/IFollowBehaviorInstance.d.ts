
type FollowBehaviorMode = "time" | "distance";
type FollowBehaviorInterpolationType = "step" | "linear" | "angular";
type FollowBehaviorPropertyType = "x" | "y" | "z-elevation" | "width" | "height" | "angle" | "opacity" | "visibility" | "destroyed";

/** Represents the Follow behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/follow | IFollowBehaviorInstance documentation } */
declare class IFollowBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	followInstance: IInstance;
    mode: FollowBehaviorMode;
    delay: number;
    maxDelay: number;
    historyRate: number;
    clearHistory(): void;
    rewindHistory(time: number): void;
    setFollowingProperty(prop: FollowBehaviorPropertyType, isEnabled: boolean): void;
    isFollowingProperty(prop: FollowBehaviorPropertyType): boolean;
    setPropertyInterpolation(prop: FollowBehaviorPropertyType, interp: FollowBehaviorInterpolationType): void;
    getPropertyInterpolation(prop: FollowBehaviorPropertyType): FollowBehaviorInterpolationType;

    startFollowingCustomProperty(customProp: string, interp: FollowBehaviorInterpolationType): void;
    stopFollowingCustomProperty(customProp: string): void;
    isFollowingCustomProperty(customProp: string): boolean;
    setCustomPropertyValue(customProp: string, value: number|string): void;
    getDelayedCustomPropertyValue(customProp: string): number|string;

    saveHistoryToJSON(maxDelay?: number): JSONValue;
    loadHistoryFromJSON(json: JSONValue): void;
    
    isEnabled: boolean;
}
