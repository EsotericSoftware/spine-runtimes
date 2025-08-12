
type HTMLContentType = "html" | "bbcode" | "text";
type HTMLSetClassMode = "add" | "toggle" | "remove";
type HTMLSetAttributeMode = "set" | "remove";
type HTMLInsertAtType = "start" | "end" | "replace";
type HTMLScrollDirectionType = "left" | "top";

interface HTMLInstanceElementEvent<InstType = IHTMLElementInstance> extends InstanceEvent<InstType> {
	targetId: string;
	targetClass: string;
}

interface HTMLInstanceAnimationEvent<InstType = IHTMLElementInstance> extends HTMLInstanceElementEvent<InstType> {
	animationName: string;
}

interface HTMLInstanceEventMap<InstType = IHTMLElementInstance> extends WorldInstanceEventMap<InstType> {
	"click": HTMLInstanceElementEvent<InstType>;
	"animationend": HTMLInstanceAnimationEvent<InstType>;
}

/** Represents the HTML Element object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/html-element | IHTMLElementInstance documentation } */
declare class IHTMLElementInstance extends IDOMInstance
{
	addEventListener<K extends keyof HTMLInstanceEventMap<this>>(type: K, listener: (ev: HTMLInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof HTMLInstanceEventMap<this>>(type: K, listener: (ev: HTMLInstanceEventMap<this>[K]) => any): void;

	setContent(str: string, type?: HTMLContentType, selector?: string, isAll?: boolean): Promise<void>;
	insertContent(str: string, type?: HTMLContentType, atEnd?: boolean, selector?: string, isAll?: boolean): Promise<void>;
	setContentClass(mode: HTMLSetClassMode, classArr: string | string[], selector: string, isAll?: boolean): Promise<void>;
	setContentAttribute(mode: HTMLSetAttributeMode, attrib: string, value: string, selector: string, isAll?: boolean): Promise<void>;
	setContentCssStyle(propName: string, value: string, selector: string, isAll?: boolean) : Promise<void>;

	positionInstanceAtElement(inst: IWorldInstance, selector: string): Promise<void>;
	createSpriteImgElement(spriteInst: ISpriteInstance, selector: string, insertAt: HTMLInsertAtType, id?: string, class_?: string): Promise<void>;
	setScrollPosition(selector: string, direction: HTMLScrollDirectionType, position: number): Promise<void>;

	htmlContent: string;
	textContent: string;
}
