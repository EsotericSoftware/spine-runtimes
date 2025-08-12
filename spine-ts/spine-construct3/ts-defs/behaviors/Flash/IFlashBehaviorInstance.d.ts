
interface FlashBehaviorInstanceEventMap<InstType, BehInstType> extends BehaviorInstanceEventMap<InstType, BehInstType> {
	"flashend": BehaviorInstanceEvent<InstType, BehInstType>;
}

/** Represents the Flash behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/flash | IFlashBehaviorInstance documentation } */
declare class IFlashBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	addEventListener<K extends keyof FlashBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: FlashBehaviorInstanceEventMap<InstType, this>[K]) => any): void;
	removeEventListener<K extends keyof FlashBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: FlashBehaviorInstanceEventMap<InstType, this>[K]) => any): void;

	flash(on: number, off: number, dur: number): void;
	stop(): void;
	readonly isFlashing: boolean;
}
