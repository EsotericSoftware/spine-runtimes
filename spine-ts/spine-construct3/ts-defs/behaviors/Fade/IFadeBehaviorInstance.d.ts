
interface FadeBehaviorInstanceEventMap<InstType, BehInstType> extends BehaviorInstanceEventMap<InstType, BehInstType> {
	"fadeinend": BehaviorInstanceEvent<InstType, BehInstType>;
	"waitend": BehaviorInstanceEvent<InstType, BehInstType>;
	"fadeoutend": BehaviorInstanceEvent<InstType, BehInstType>;
}

/** Represents the Fade behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/fade | IFadeBehaviorInstance documentation } */
declare class IFadeBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	addEventListener<K extends keyof FadeBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: FadeBehaviorInstanceEventMap<InstType, this>[K]) => any): void;
	removeEventListener<K extends keyof FadeBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: FadeBehaviorInstanceEventMap<InstType, this>[K]) => any): void;

	startFade(): void;
	restartFade(): void;
	fadeInTime: number;
	waitTime: number;
	fadeOutTime: number;
}
