
/** Represents the Sprite Font object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/spritefont | ISpriteFontInstance documentation } */
declare class ISpriteFontInstance extends IWorldInstance
{
	text: string;

	typewriterText(str: string, duration: number): void;
	characterScale: number;
	characterSpacing: number;
	lineHeight: number;
	horizontalAlign: TextAlignHorizontalMode;
	verticalAlign: TextAlignVerticalMode;
	wordWrapMode: TextWordWrapMode;
	readAloud: boolean;
	readonly textWidth: number;
	readonly textHeight: number;
	getTextSize(): Vec2Arr;
	hasTagAtPosition(tag: string, x: number, y: number): boolean;
	getTagAtPosition(x: number, y: number): string;
	getTagCount(tag: string): number;
	getTagPositionAndSize(tag: string, index?: number): TextFragmentPositionAndSize;
}
