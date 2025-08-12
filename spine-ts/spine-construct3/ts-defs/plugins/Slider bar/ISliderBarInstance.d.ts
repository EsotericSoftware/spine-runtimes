
interface SliderBarInstanceEventMap<InstType = ISliderBarInstance> extends WorldInstanceEventMap<InstType> {
	"click": InstanceEvent<InstType>;
	"change": InstanceEvent<InstType>;
	"input": InstanceEvent<InstType>;
}

/** Represents the Slider Bar object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/slider-bar | ISliderBarInstance documentation } */
declare class ISliderBarInstance extends IDOMInstance
{
	addEventListener<K extends keyof SliderBarInstanceEventMap<this>>(type: K, listener: (ev: SliderBarInstanceEventMap<this>[K]) => any): void;
	removeEventListener<K extends keyof SliderBarInstanceEventMap<this>>(type: K, listener: (ev: SliderBarInstanceEventMap<this>[K]) => any): void;

	value: number;
	maximum: number;
	minimum: number;
	step: number;
	tooltip: string;
	isEnabled: boolean;
}
