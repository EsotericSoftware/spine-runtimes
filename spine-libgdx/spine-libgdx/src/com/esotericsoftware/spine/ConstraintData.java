
package com.esotericsoftware.spine;

abstract public class ConstraintData< //
	T extends Constraint, //
	P extends Pose> //
	extends PosedData<P> {

	public ConstraintData (String name, P setup) {
		super(name, setup);
	}

	/** The constraint's name, unique across all constraints in the skeleton.
	 * <p>
	 * See {@link SkeletonData#findConstraint(String, Class)} and {@link Skeleton#findConstraint(String, Class)}. */
	public String getName () { // Do not port.
		return super.getName();
	}

	abstract public T create (Skeleton skeleton);
}
