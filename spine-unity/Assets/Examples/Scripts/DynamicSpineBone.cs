/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using System.Collections;

public class DynamicSpineBone : MonoBehaviour {

	public Transform speedReference;

	[SpineBone]
	public string boneName;

	[Range(-90, 90)]
	public float minRotation = -45;
	[Range(-90, 90)]
	public float maxRotation = 45;

	[Range(-2000, 2000)]
	public float rotationFactor = 300;

	[Range(5, 30)]
	public float returnSpeed = 10;

	[Range(100, 1000)]
	public float boneSpeed = 300;

	public float returnThreshhold = 0.01f;

	public bool useAcceleration;


	SkeletonAnimation skeletonAnimation;
	float goalRotation;
	Spine.Bone bone;
	Vector3 velocity;
	Vector3 acceleration;
	Vector3 lastPosition;

	void Start() {
		if (speedReference == null)
			speedReference = transform;

		skeletonAnimation = GetComponent<SkeletonAnimation>();
		bone = SpineBone.GetBone(boneName, skeletonAnimation);
		skeletonAnimation.UpdateLocal += UpdateLocal;
		lastPosition = speedReference.position;
	}

	void FixedUpdate() {
		acceleration = (speedReference.position - lastPosition) - velocity;
		velocity = speedReference.position - lastPosition;
		lastPosition = speedReference.position;
	}

	void UpdateLocal(SkeletonRenderer renderer) {
		Vector3 vec = useAcceleration ? acceleration : velocity;

		if (Mathf.Abs(vec.x) < returnThreshhold)
			goalRotation = Mathf.Lerp(goalRotation, 0, returnSpeed * Time.deltaTime);
		else
			goalRotation += vec.x * rotationFactor * Time.deltaTime * (bone.WorldFlipX ? -1 : 1);

		goalRotation = Mathf.Clamp(goalRotation, minRotation, maxRotation);

		bone.Rotation = Mathf.Lerp(bone.Rotation, bone.Rotation + goalRotation, boneSpeed * Time.deltaTime);

	}
}
