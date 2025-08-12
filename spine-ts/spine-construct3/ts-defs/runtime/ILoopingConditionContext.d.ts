
/** Utility class for creating looping conditions */
declare class ILoopingConditionContext
{
	retrigger(): void;
	
	readonly isStopped: boolean;
	
	release(): void;
}
