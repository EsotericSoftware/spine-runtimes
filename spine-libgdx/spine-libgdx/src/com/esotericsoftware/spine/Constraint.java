
package com.esotericsoftware.spine;

abstract public class Constraint< //
	T extends Constraint<T, D, P>, //
	D extends ConstraintData<T, P>, //
	P extends Pose> //
	extends PosedActive<D, P> implements Update {

	public Constraint (D data, P pose, P constrained) {
		super(data, pose, constrained);
	}

	abstract public T copy (Skeleton skeleton);

	abstract void sort (Skeleton skeleton);

	boolean isSourceActive () {
		return true;
	}
}
