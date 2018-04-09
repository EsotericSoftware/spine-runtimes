using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class DraggableTransform : MonoBehaviour {

		Vector2 mousePreviousWorld, mouseDeltaWorld;
		Camera mainCamera;

		void Start () {
			mainCamera = Camera.main;
		}

		void Update () {
			Vector2 mouseCurrent = Input.mousePosition;
			Vector2 mouseCurrentWorld = mainCamera.ScreenToWorldPoint(new Vector3(mouseCurrent.x, mouseCurrent.y, -mainCamera.transform.position.z));

			mouseDeltaWorld = mouseCurrentWorld - mousePreviousWorld;
			mousePreviousWorld = mouseCurrentWorld;
		}

		void OnMouseDrag () {
			transform.Translate(mouseDeltaWorld);
		}
	}
}
