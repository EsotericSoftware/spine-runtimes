
/** Represents an actively running tween.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/interfaces/itweenstate | ITweenState documentation } */
declare class ITweenState extends ITimelineStateBase
{
	stop(): void;
    setEase(easeName: string): void;
    readonly instance: IWorldInstance;
    isDestroyOnComplete: boolean;
    readonly value: number;
}
