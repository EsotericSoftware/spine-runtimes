
package com.esotericsoftware.spine;

/** Determines how physics and other non-deterministic updates are applied. */
public enum Physics {
	/** Physics are not updated or applied. */
	none,

	/** Physics are {@link PhysicsConstraint#reset() reset}. */
	reset,

	/** Physics are updated and the pose from physics is applied. */
	update,

	/** Physics are not updated but the pose from physics is applied. */
	pose
}
