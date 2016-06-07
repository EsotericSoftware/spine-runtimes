using UnityEngine;

namespace Spine.Unity.Editor {
	public static class AssetDatabaseAvailabilityDetector {
		const string MARKER_RESOURCE_NAME = "SpineAssetDatabaseMarker";
		private static bool _isMarkerLoaded;

		public static bool IsAssetDatabaseAvailable (bool forceCheck = false) {
			if (!forceCheck && _isMarkerLoaded)
				return true;

			TextAsset markerTextAsset = Resources.Load<TextAsset>(MARKER_RESOURCE_NAME);
			_isMarkerLoaded = markerTextAsset != null;
			if (markerTextAsset != null) {
				Resources.UnloadAsset(markerTextAsset);
			}

			return _isMarkerLoaded;
		}
	}
}
