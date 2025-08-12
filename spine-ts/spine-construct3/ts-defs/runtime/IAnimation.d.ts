
/** Represents an animation in an object type.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/ianimation | IAnimation documentation } */
declare class IAnimation
{
	readonly name: string;
    readonly speed: number;
    readonly isLooping: boolean;
    readonly repeatCount: number;
    readonly repeatTo: number;
    readonly isPingPong: boolean;
    readonly frameCount: number;

    getFrames(): IAnimationFrame[];
    frames(): Generator<IAnimationFrame>;
}
