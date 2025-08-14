
type TweenPropertyType = "x" | "y" | "width" | "height" | "angle" | "opacity" | "color" | "z-elevation" | "x-scale" | "y-scale" | "position" | "size" | "scale" | "value";
type TweenEndValueType = number | Vec2Arr | Vec3Arr;
type TweenBuiltInEaseType = "linear" | "in-sine" | "out-sine" | "in-out-sine" | "in-elastic" | "out-elastic" | "in-out-elastic" | "in-back" | "out-back" | "in-out-back" |
	"in-bounce" | "out-bounce" | "in-out-bounce" | "in-cubic" | "out-cubic" | "in-out-cubic" | "in-quadratic" | "out-quadratic" | "in-out-quadratic" |
	"in-quartic" | "out-quartic" | "in-out-quartic" | "in-quintic" | "out-quintic" | "in-out-quintic" | "in-circular" | "out-circular" | "in-out-circular" |
	"in-exponential" | "out-exponential" | "in-out-exponential";

interface StartTweenOpts {
	tags?: string | string[];
	destroyOnComplete?: boolean;
	loop?: boolean;
	pingPong?: boolean;
	repeatCount?: number;
	startValue?: number;
}

/** Represents the Tween behavior.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/behavior-interfaces/tween | ITweenBehaviorInstance documentation } */
declare class ITweenBehaviorInstance<InstType> extends IBehaviorInstance<InstType>
{
	startTween(prop: TweenPropertyType, endValue: TweenEndValueType, time: number, ease: TweenBuiltInEaseType | TweenCustomEaseType, opts?: StartTweenOpts): ITweenState;

	allTweens(): Generator<ITweenState>;
	tweensByTags(tags: string | string[]): Generator<ITweenState>;
	isEnabled: boolean;
}
