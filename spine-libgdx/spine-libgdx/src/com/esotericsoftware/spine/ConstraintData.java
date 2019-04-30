
package com.esotericsoftware.spine;

/** The base class for all constraint datas. */
abstract public class ConstraintData {
	final String name;
	int order;
	boolean skinRequired;

	public ConstraintData (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
	}

	/** The constraint's name, which is unique across all constraints in the skeleton of the same type. */
	public String getName () {
		return name;
	}

	/** The ordinal of this constraint for the order a skeleton's constraints will be applied by
	 * {@link Skeleton#updateWorldTransform()}. */
	public int getOrder () {
		return order;
	}

	public void setOrder (int order) {
		this.order = order;
	}

	/** When true, {@link Skeleton#updateWorldTransform()} only updates this constraint if the {@link Skeleton#getSkin()} contains
	 * this constraint.
	 * @see Skin#getConstraints() */
	public boolean getSkinRequired () {
		return skinRequired;
	}

	public void setSkinRequired (boolean skinRequired) {
		this.skinRequired = skinRequired;
	}

	public String toString () {
		return name;
	}
}
