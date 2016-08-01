using UnityEngine;
using System.Collections;

[SelectionBase]
public class SpineboyBeginnerModel : MonoBehaviour {

	#region Inspector
	[Header("Current State")]
	public SpineBeginnerBodyState state;
	public bool facingLeft;
	[Range(-1f, 1f)]
	public float currentSpeed;

	[Header("Balance")]
	public float shootInterval = 0.12f;
	#endregion

	float lastShootTime;
	public event System.Action ShootEvent;	// Lets other scripts know when Spineboy is shooting. Check C# Documentation to learn more about events and delegates.

	#region API
	public void TryJump () {
		StartCoroutine(JumpRoutine());
	}

	public void TryShoot () {
		float currentTime = Time.time;

		if (currentTime - lastShootTime > shootInterval) {
			lastShootTime = currentTime;
			if (ShootEvent != null) ShootEvent();	// Fire the "ShootEvent" event.
		}
	}

	public void TryMove (float speed) {
		currentSpeed = speed; // show the "speed" in the Inspector.

		if (speed != 0) {
			bool speedIsNegative = (speed < 0f);
			facingLeft = speedIsNegative; // Change facing direction whenever speed is not 0.
		}
			
		if (state != SpineBeginnerBodyState.Jumping) {
			state = (speed == 0) ? SpineBeginnerBodyState.Idle : SpineBeginnerBodyState.Running;
		}

	}
	#endregion

	IEnumerator JumpRoutine () {
		if (state == SpineBeginnerBodyState.Jumping) yield break;	// Don't jump when already jumping.

		state = SpineBeginnerBodyState.Jumping;

		// Terribly-coded Fake jumping.
		{
			var pos = transform.localPosition;
			const float jumpTime = 1.2f;
			const float half = jumpTime * 0.5f;
			const float jumpPower = 20f;
			for (float t = 0; t < half; t += Time.deltaTime) {
				float d = jumpPower * (half - t);
				transform.Translate((d * Time.deltaTime) * Vector3.up);
				yield return null;
			}
			for (float t = 0; t < half; t += Time.deltaTime) {
				float d = jumpPower * t;
				transform.Translate((d * Time.deltaTime) * Vector3.down);
				yield return null;
			}
			transform.localPosition = pos;
		}

		state = SpineBeginnerBodyState.Idle;
	}

}

public enum SpineBeginnerBodyState {
	Idle,
	Running,
	Jumping
}