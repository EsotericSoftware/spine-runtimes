
/** Base class of an actively running timeline or tween.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/interfaces/itimelinestatebase | ITimelineStateBase documentation } */
declare class ITimelineStateBase
{
	pause(): void;
    resume(): void;
    stop(): void;
    hasTags(tags: string): boolean;
    time: number;
    totalTime: number;
    isLooping: boolean;
    isPingPong: boolean;
    playbackRate: number;
    
    readonly progress: number;
    readonly tags: string;
    readonly finished: Promise<void>;
    readonly isPlaying: boolean;
    readonly isPaused: boolean;
    readonly isReleased: boolean;
}
