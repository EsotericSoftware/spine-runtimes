package com.esotericsoftware.spine;

/** Stores a group pose. */
public class Group extends Posed<GroupData, GroupPose> {
	public Group (GroupData data) {
		super(data, new GroupPose(), new GroupPose());
		setupPose();
	}

	/** Copy constructor. */
	public Group (Group group) {
		super(group.data, new GroupPose(), new GroupPose());
		pose.set(group.pose);
	}
}
