/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2022, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using NUnit.Framework;
using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class RootMotionTests {

	const float WalkRootMotionVelocity = 9.656f; // value specific to this test animation
	const float PositionEpsilon = 0.01f;

	GameObject rootGameObject;
	SkeletonRootMotion skeletonRootMotion;
	SkeletonAnimation skeletonAnimation;
	Transform skeletonTransform;

	float savedFixedTimeStep;
	Vector2 translationDeltaSum = Vector2.zero;
	float rotationDeltaSum = 0f;
	int loopsComplete = 0;

	[UnityTest]
	public IEnumerator Test01BasicFunctionality () {
		SetupLoadingPrefab("RootMotionTestPrefabRoot", "straight-walk");
		float lastPositionX = skeletonTransform.position.x;

		// root motion shall lead to increasing transform position
		for (int i = 0; i < 30; ++i) {
			yield return null;
			lastPositionX = ExpectIncreasingPositionX(lastPositionX, Time.deltaTime * WalkRootMotionVelocity);
		}

		// disabling the root motion component shall no longer change the transform
		skeletonRootMotion.enabled = false;
		for (int i = 0; i < 3; ++i) {
			yield return null;
			lastPositionX = ExpectUnchangedPositionX(lastPositionX);
		}
	}

	[UnityTest]
	public IEnumerator Test02BoneOffsetNoRigidbody () {
		SetupLoadingPrefab("RootMotionTestPrefabRoot", "straight-walk");
		yield return null;
		Bone topLevelBone = skeletonAnimation.skeleton.Bones.Items[0];
		Bone rootMotionBone = skeletonRootMotion.RootMotionBone;
		bool rootMotionIsTopLevelBone = rootMotionBone == topLevelBone;

		float lastPositionX = topLevelBone.GetLocalPosition().x;
		float lastRootMotionBoneX = rootMotionBone.GetLocalPosition().x;
		// with enabled root motion component the top level bone shall no longer change position.
		for (int i = 0; i < 15; ++i) {
			yield return null;
			if (rootMotionIsTopLevelBone)
				lastPositionX = ExpectUnchangedLocalBonePositionX(topLevelBone, lastPositionX);
			else {
				lastPositionX = ExpectDecreasingLocalBonePositionX(topLevelBone, lastPositionX);
				lastRootMotionBoneX = ExpectUnchangedLocalBonePositionX(rootMotionBone, lastRootMotionBoneX);
			}
		}

		// disabling the root motion component shall no longer reset the root motion bone
		skeletonRootMotion.enabled = false;
		for (int i = 0; i < 3; ++i) {
			yield return null;
			lastPositionX = ExpectIncreasingLocalBonePositionX(rootMotionBone, lastPositionX);
		}
	}

	[UnityTest]
	public IEnumerator Test03UpdateCallbacks () {
		SetupLoadingPrefab("RootMotionTestPrefabRoot", "straight-walk");
		float lastPositionX = skeletonTransform.position.x;

		skeletonRootMotion.ProcessRootMotionOverride -= ProcessRootMotionNoOp;
		skeletonRootMotion.ProcessRootMotionOverride += ProcessRootMotionNoOp;
		skeletonRootMotion.disableOnOverride = true;
		// with disableOnOverride it shall no longer change the transform
		for (int i = 0; i < 3; ++i) {
			yield return null;
			lastPositionX = ExpectUnchangedPositionX(lastPositionX);
		}

		skeletonRootMotion.disableOnOverride = false;
		// with disableOnOverride = false it shall again apply root motion to the transform
		for (int i = 0; i < 3; ++i) {
			yield return null;
			lastPositionX = ExpectIncreasingPositionX(lastPositionX, Time.deltaTime * WalkRootMotionVelocity);
		}
	}

	[UnityTest]
	public IEnumerator Test04PhysicsUpdateCallbacks2D () {
		return TestPhysicsUpdateCallbacks("straight-walk-rigidbody2d");
	}

	[UnityTest]
	public IEnumerator Test05PhysicsUpdateCallbacks3D () {
		return TestPhysicsUpdateCallbacks("straight-walk-rigidbody3d");
	}

	public IEnumerator TestPhysicsUpdateCallbacks (string gameObjectName) {
		SetupLoadingPrefab("RootMotionTestPrefabRoot", gameObjectName);
		float lastPositionX = skeletonTransform.position.x;

		skeletonRootMotion.ProcessRootMotionOverride -= ProcessRootMotionNoOp;
		skeletonRootMotion.ProcessRootMotionOverride += ProcessRootMotionNoOp;

		skeletonRootMotion.PhysicsUpdateRootMotionOverride -= ProcessRootMotionNoOp;
		skeletonRootMotion.PhysicsUpdateRootMotionOverride += ProcessRootMotionNoOp;
		skeletonRootMotion.disableOnOverride = true;

		Debug.Log("Testing physics update with animation timing InUpdate.");
		skeletonAnimation.UpdateTiming = UpdateTiming.InUpdate;
		yield return new WaitForFixedUpdate();
		yield return null;
		lastPositionX = skeletonTransform.position.x;
		// a rigidbody is assigned, disableOnOverride shall disable applying root motion when PhysicsUpdateRootMotionOverride is set
		for (int i = 0; i < 3; ++i) {
			yield return new WaitForFixedUpdate();
			yield return null;
			lastPositionX = ExpectUnchangedPositionX(lastPositionX);
		}

		// same when using InFixedUpdate
		Debug.Log("Testing physics update with animation timing InFixedUpdate.");
		skeletonAnimation.UpdateTiming = UpdateTiming.InFixedUpdate;
		yield return new WaitForFixedUpdate();
		lastPositionX = skeletonTransform.position.x;
		for (int i = 0; i < 3; ++i) {
			yield return new WaitForFixedUpdate();
			lastPositionX = ExpectUnchangedPositionX(lastPositionX);
		}

		Debug.Log("Testing physics update InFixedUpdate with disableOnOverride = false.");
		Time.fixedDeltaTime = 0.5f;

		skeletonRootMotion.disableOnOverride = false;
		skeletonAnimation.UpdateTiming = UpdateTiming.InFixedUpdate;
		yield return new WaitForFixedUpdate();
		lastPositionX = skeletonTransform.position.x;
		for (int i = 0; i < 3; ++i) {
			yield return null;
			lastPositionX = ExpectUnchangedPositionX(lastPositionX);
			yield return new WaitForFixedUpdate();
			lastPositionX = ExpectIncreasingPositionX(lastPositionX, Time.deltaTime * WalkRootMotionVelocity);
		}
	}

	[UnityTest]
	public IEnumerator Test06CallbackDistancesInUpdate2D () {
		return TestCallbackDistancesInUpdate("straight-walk-rigidbody2d");
	}

	[UnityTest]
	public IEnumerator Test07CallbackDistancesInUpdate3D () {
		return TestCallbackDistancesInUpdate("straight-walk-rigidbody3d");
	}

	public IEnumerator TestCallbackDistancesInUpdate (string gameObjectName) {
		SetupLoadingPrefab("RootMotionTestPrefabRoot", gameObjectName);
		float lastPositionX = skeletonTransform.position.x;

		Debug.Log("Testing physics update with animation timing InUpdate.");
		skeletonAnimation.UpdateTiming = UpdateTiming.InUpdate;
		skeletonRootMotion.disableOnOverride = true;

		skeletonRootMotion.ProcessRootMotionOverride -= CumulateDelta;
		skeletonRootMotion.ProcessRootMotionOverride += CumulateDelta;
		skeletonRootMotion.PhysicsUpdateRootMotionOverride -= EndAndCompareCumulatedDelta;
		skeletonRootMotion.PhysicsUpdateRootMotionOverride += EndAndCompareCumulatedDelta;

		for (int i = 0; i < 30; ++i) {
			yield return new WaitForFixedUpdate();
			yield return null;
		}

		Debug.Log("Testing physics update animating InUpdate with very long fixedDeltaTime (physics timestep).");
		Time.fixedDeltaTime = 0.5f;

		skeletonRootMotion.disableOnOverride = false;
		lastPositionX = skeletonTransform.position.x;
		for (int i = 0; i < 6; ++i) { // only few iterations since WaitForFixedUpdate takes 0.5 seconds now!
			yield return new WaitForFixedUpdate();
			yield return null;
		}
	}

	[UnityTest]
	public IEnumerator Test08CallbackDistancesInFixedUpdate2D () {
		return TestCallbackDistancesInFixedUpdate("straight-walk-rigidbody2d");
	}

	[UnityTest]
	public IEnumerator Test09CallbackDistancesInFixedUpdate3D () {
		return TestCallbackDistancesInFixedUpdate("straight-walk-rigidbody3d");
	}

	public IEnumerator TestCallbackDistancesInFixedUpdate (string gameObjectName) {
		SetupLoadingPrefab("RootMotionTestPrefabRoot", gameObjectName);
		float lastPositionX = skeletonTransform.position.x;

		Debug.Log("Testing physics update with animation timing InFixedUpdate.");
		skeletonAnimation.UpdateTiming = UpdateTiming.InFixedUpdate;
		skeletonRootMotion.disableOnOverride = true;

		skeletonRootMotion.ProcessRootMotionOverride -= CumulateDelta;
		skeletonRootMotion.ProcessRootMotionOverride += CumulateDelta;
		skeletonRootMotion.PhysicsUpdateRootMotionOverride -= EndAndCompareCumulatedDelta;
		skeletonRootMotion.PhysicsUpdateRootMotionOverride += EndAndCompareCumulatedDelta;

		// a rigidbody is assigned, disableOnOverride shall disable applying root motion when PhysicsUpdateRootMotionOverride is set
		for (int i = 0; i < 30; ++i) {
			yield return new WaitForFixedUpdate();
			yield return null;
		}

		Debug.Log("Testing physics update animating InFixedUpdate with very long fixedDeltaTime (physics timestep).");
		Time.fixedDeltaTime = 0.5f;

		skeletonRootMotion.disableOnOverride = false;
		lastPositionX = skeletonTransform.position.x;
		for (int i = 0; i < 6; ++i) { // only few iterations since WaitForFixedUpdate takes 0.5 seconds now!
			yield return new WaitForFixedUpdate();
			yield return null;
		}
	}

	[UnityTest]
	public IEnumerator Test10DeltaCompensation () {
		SetupLoadingPrefab("RootMotionTestPrefabRoot", "jump-rootmotion");
		Transform jumpTarget = GameObject.Find("jump-target").transform;
		Time.timeScale = 3;
		skeletonAnimation.AnimationState.Complete += OnLoopComplete;
		yield return null;

		for (int i = 0; i < 600 && loopsComplete == 0; ++i) {
			// note: done via existing example component now.
			// Vector3 toTarget = jumpTarget.position - skeletonTransform.position;
			// skeletonRootMotion.AdjustRootMotionToDistance(toTarget);
			yield return null;
		}
		Assert.AreEqual(1, loopsComplete);
		Assert.AreEqual(jumpTarget.transform.position.x, skeletonTransform.position.x, PositionEpsilon);
	}

	public void OnLoopComplete (TrackEntry trackEntry) {
		++loopsComplete;
	}

	GameObject SetupLoadingPrefab (string prefabAssetName, string skeletonObjectName) {
		GameObject prefab = Resources.Load<GameObject>(prefabAssetName);
		rootGameObject = MonoBehaviour.Instantiate(prefab);
		Assert.IsNotNull(rootGameObject);

		GameObject straightWalkObject = GameObject.Find(skeletonObjectName);
		Assert.IsNotNull(straightWalkObject);

		skeletonTransform = straightWalkObject.transform;
		skeletonAnimation = straightWalkObject.GetComponent<SkeletonAnimation>();
		skeletonRootMotion = skeletonAnimation.GetComponent<SkeletonRootMotion>();
		Assert.IsNotNull(skeletonAnimation);
		Assert.IsNotNull(skeletonRootMotion);
		return rootGameObject;
	}

	[UnitySetUp]
	public IEnumerator SetUp () {
		savedFixedTimeStep = Time.fixedDeltaTime;
		translationDeltaSum = Vector2.zero;
		rotationDeltaSum = 0f;
		yield return null;
	}

	[UnityTearDown]
	public IEnumerator TearDown () {
		Time.timeScale = 1;
		Time.fixedDeltaTime = savedFixedTimeStep;
		GameObject.Destroy(rootGameObject);
		yield return null;
	}

	void ProcessRootMotionNoOp (SkeletonRootMotionBase component, Vector2 translation, float rotation) {
	}

	void CumulateDelta (SkeletonRootMotionBase component, Vector2 translationDelta, float rotationDelta) {
		translationDeltaSum += translationDelta;
		rotationDeltaSum += rotationDelta;
		Debug.Log("  Accumulating movement delta for later comparison. Single delta: " + translationDelta);
	}

	void EndAndCompareCumulatedDelta (SkeletonRootMotionBase component, Vector2 cumulatedTranslation, float cumulatedRotation) {
		Debug.Log("  Cumulated movement delta from callback: " + cumulatedTranslation);
		Assert.AreEqual(translationDeltaSum.x, cumulatedTranslation.x);
		Assert.AreEqual(translationDeltaSum.y, cumulatedTranslation.y);
		Assert.AreEqual(rotationDeltaSum, cumulatedRotation);

		translationDeltaSum = Vector2.zero;
		rotationDeltaSum = 0f;
		Debug.Log("  Successfully compared cumulated delta.");
	}

	float ExpectUnchangedPositionX (float lastPositionX) {
		float positionX = skeletonTransform.position.x;
		Assert.AreEqual(positionX, lastPositionX);
		return positionX;
	}

	float ExpectIncreasingPositionX (float lastPositionX, float difference) {
		float positionX = skeletonTransform.position.x;
		Assert.Greater(positionX, lastPositionX);
		//Debug.Log(string.Format("positionX {0}, lastPositionX {1}, deltatime {2}.", positionX, lastPositionX, Time.deltaTime));
		Assert.AreEqual(lastPositionX + difference, positionX, PositionEpsilon);
		return positionX;
	}

	float ExpectUnchangedLocalBonePositionX (Bone bone, float lastPositionX) {
		float positionX = bone.GetLocalPosition().x;
		Assert.AreEqual(positionX, lastPositionX);
		return positionX;
	}

	float ExpectIncreasingLocalBonePositionX (Bone bone, float lastPositionX) {
		float positionX = bone.GetLocalPosition().x;
		Assert.Greater(positionX, lastPositionX);
		return positionX;
	}

	float ExpectDecreasingLocalBonePositionX (Bone bone, float lastPositionX) {
		float positionX = bone.GetLocalPosition().x;
		Assert.Less(positionX, lastPositionX);
		return positionX;
	}
}
