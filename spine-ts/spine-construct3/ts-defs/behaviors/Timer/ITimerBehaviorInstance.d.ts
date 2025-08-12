
type TimerBehaviorTimerType = "once" | "regular";

declare class TimerBehaviorEvent<InstType = IInstance, BehInstType = ITimerBehaviorInstance<InstType>> extends BehaviorInstanceEvent<InstType, BehInstType>
{
	tag: string;
}

interface TimerBehaviorInstanceEventMap<InstType, BehInstType> extends BehaviorInstanceEventMap<InstType, BehInstType> {
	"timer": TimerBehaviorEvent<InstType, BehInstType>;
}

/** Represents the Timer behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/timer | ITimerBehaviorInstance documentation } */
declare class ITimerBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	addEventListener<K extends keyof TimerBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: TimerBehaviorInstanceEventMap<InstType, this>[K]) => any): void;
	removeEventListener<K extends keyof TimerBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: TimerBehaviorInstanceEventMap<InstType, this>[K]) => any): void;

	startTimer(duration: number, name: string, type?: TimerBehaviorTimerType): void;
	setTimerPaused(name: string, isPaused: boolean): void;
	setAllTimersPaused(isPaused: boolean): void;
	stopTimer(name: string): void;
	stopAllTimers(): void;
	isTimerRunning(name: string): boolean;
	isTimerPaused(name: string): boolean;
	getCurrentTime(name: string): number;
	getTotalTime(name: string): number;
	getDuration(name: string): number;
	hasFinished(name: string): boolean;
}
