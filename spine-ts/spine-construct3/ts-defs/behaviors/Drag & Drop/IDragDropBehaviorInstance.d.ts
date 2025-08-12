
type DragDropBehaviorAxes = "both" | "horizontal" | "vertical";

interface DragDropBehaviorInstanceEventMap<InstType, BehInstType> extends BehaviorInstanceEventMap<InstType, BehInstType> {
	"dragstart": BehaviorInstanceEvent<InstType, BehInstType>;
	"drop": BehaviorInstanceEvent<InstType, BehInstType>;
}

/** Represents the Car behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/drag-drop | IDragDropBehaviorInstance documentation } */
declare class IDragDropBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	addEventListener<K extends keyof DragDropBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: DragDropBehaviorInstanceEventMap<InstType, this>[K]) => any): void;
	removeEventListener<K extends keyof DragDropBehaviorInstanceEventMap<InstType, this>>(type: K, listener: (ev: DragDropBehaviorInstanceEventMap<InstType, this>[K]) => any): void;

	axes: DragDropBehaviorAxes;
	drop(): void;
	readonly isDragging: boolean;
	isEnabled: boolean;
}
