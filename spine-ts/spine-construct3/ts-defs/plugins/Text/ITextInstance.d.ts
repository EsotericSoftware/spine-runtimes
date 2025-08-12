
/** Represents the Text object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/text | ITextInstance documentation } */
declare class ITextInstance extends IWorldInstance
{
	text: string;

	typewriterText(str: string, duration: number): void;
	fontFace: string;
	isBold: boolean;
	isItalic: boolean;
	sizePt: number;
	fontColor: Vec3Arr;
	lineHeight: number;
	horizontalAlign: TextAlignHorizontalMode;
	verticalAlign: TextAlignVerticalMode;
	wordWrapMode: TextWordWrapMode;
	textDirection: TextDirectionMode;
	readAloud: boolean;
	setFixedResolutionMode(fixedScale: number): void;
	setAutoResolutionMode(): void;
	readonly textWidth: number;
	readonly textHeight: number;
	getTextSize(): Vec2Arr;
	hasTagAtPosition(tag: string, x: number, y: number): boolean;
	getTagAtPosition(x: number, y: number): string;
	getTagCount(tag: string): number;
	getTagPositionAndSize(tag: string, index?: number): TextFragmentPositionAndSize;
	changeIconSet<InstType extends IInstance>(objectClass: IObjectClass<InstType>): void;
	getAsHtmlString(): Promise<string>;
}
