package spine.animation;

import openfl.Vector;
import spine.Event;
import spine.PathConstraint;
import spine.Skeleton;

class PathConstraintSpacingTimeline extends CurveTimeline1 {
	/** The index of the path constraint slot in {@link Skeleton#pathConstraints} that will be changed. */
	public var pathConstraintIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, pathConstraintIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.pathConstraintSpacing + "|" + pathConstraintIndex]));
		this.pathConstraintIndex = pathConstraintIndex;
	}

	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Vector<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var constraint:PathConstraint = skeleton.pathConstraints[pathConstraintIndex];
		if (!constraint.active)
			return;

		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					constraint.spacing = constraint.data.spacing;
				case MixBlend.first:
					constraint.spacing += (constraint.data.spacing - constraint.spacing) * alpha;
			}
			return;
		}

		var spacing:Float = getCurveValue(time);
		if (blend == MixBlend.setup) {
			constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
		} else {
			constraint.spacing += (spacing - constraint.spacing) * alpha;
		}
	}
}
