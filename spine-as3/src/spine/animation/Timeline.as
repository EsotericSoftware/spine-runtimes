package spine.animation {
import spine.Skeleton;

public interface Timeline {
	/** Sets the value(s) for the specified time. */
	function apply (skeleton:Skeleton, time:Number, alpha:Number) : void;
}

}
