
/** Represents an object instance with an associated DOM element.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/object-interfaces/idominstance | IDOMInstance documentation } */
declare class IDOMInstance extends IWorldInstance
{
    /** Get the corresponding HTML element for this instance. Note this only works
     * in DOM mode - it will throw an exception in worker mode.    */
	getElement(): HTMLElement;

    focus(): void;
    blur(): void;

    setCssStyle(prop: string, val: string): void;
}
