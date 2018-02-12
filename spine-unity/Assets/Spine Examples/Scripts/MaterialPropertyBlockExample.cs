using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class MaterialPropertyBlockExample : MonoBehaviour {

		public float timeInterval = 1f;
		public Gradient randomColors = new Gradient();
		public string colorPropertyName = "_FillColor";

		MaterialPropertyBlock mpb;
		float timeToNextColor = 0;
		
		void Start () {
			mpb = new MaterialPropertyBlock();
		}

		void Update () {
			if (timeToNextColor <= 0) {
				timeToNextColor = timeInterval;

				Color newColor = randomColors.Evaluate(UnityEngine.Random.value);
				mpb.SetColor(colorPropertyName, newColor);
				GetComponent<MeshRenderer>().SetPropertyBlock(mpb);
			}

			timeToNextColor -= Time.deltaTime;
		}

	}

}
