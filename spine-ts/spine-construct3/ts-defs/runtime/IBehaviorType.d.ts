
/** Represents a behavior type in a project (the behavior equivalent of an object type). */
declare class IBehaviorType
{
    readonly runtime: IRuntime;
    readonly behavior: IBehavior_;
    readonly name: string;
}
