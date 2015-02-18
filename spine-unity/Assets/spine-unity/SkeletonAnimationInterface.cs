using UnityEngine;
using System.Collections;

public delegate void UpdateBonesDelegate (SkeletonRenderer skeletonRenderer);
public interface ISkeletonAnimation {
	event UpdateBonesDelegate UpdateLocal;
	event UpdateBonesDelegate UpdateWorld;
	event UpdateBonesDelegate UpdateComplete;

	void LateUpdate ();
}