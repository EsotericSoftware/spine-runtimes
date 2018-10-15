using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace Spine.Unity.Examples {
	public class EquipButtonExample : MonoBehaviour {
		public EquipAssetExample asset;
		public EquipSystemExample equipSystem;
		public Image inventoryImage;

		void OnValidate () {
			MatchImage();
		}

		void MatchImage () {
			if (inventoryImage != null)
				inventoryImage.sprite = asset.sprite;
		}

		void Start () {
			MatchImage();

			var button = GetComponent<Button>();
			button.onClick.AddListener(
				delegate { equipSystem.Equip(asset); }
			);	
		}
	}
}
