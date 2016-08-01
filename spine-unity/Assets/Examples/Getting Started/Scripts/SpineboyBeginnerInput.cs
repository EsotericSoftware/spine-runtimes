using UnityEngine;
using System.Collections;

public class SpineboyBeginnerInput : MonoBehaviour {

	#region Inspector
	public string horizontalAxis = "Horizontal";
	public string attackButton = "Fire1";
	public string jumpButton = "Jump";

	public SpineboyBeginnerModel model;

	void OnValidate () {
		if (model == null)
			model = GetComponent<SpineboyBeginnerModel>();
	}
	#endregion

	void Update () {
		if (model == null) return;

		float currentHorizontal = Input.GetAxisRaw(horizontalAxis);
		model.TryMove(currentHorizontal);

		if (Input.GetButton(attackButton))
			model.TryShoot();

		if (Input.GetButtonDown(jumpButton))
			model.TryJump();
	
	}


}
