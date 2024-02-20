using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Spine.Unity.Editor {

	/// <summary>
	/// Utility class for working with TextureImporter.
	/// </summary>
	public static class TextureImporterUtils {

		private static IEnumerable<string> GetAllPossiblePlatforms() {
			BuildTarget[] buildTargets = (BuildTarget[])Enum.GetValues(typeof(BuildTarget));
			var platformNames = buildTargets.Select(x => x.ToString()).ToList();
			
			// Add additional platforms that are not part of BuildTarget enum.
			platformNames.Add("Server");
			
			return platformNames.ToArray();
		}

		public static bool TryDisableOverrides(TextureImporter importer, out List<string> disabledPlatforms) {
			IEnumerable<string> platforms = GetAllPossiblePlatforms();
			disabledPlatforms = new List<string>();

			foreach (string platform in platforms) {
				var platformSettings = importer.GetPlatformTextureSettings(platform);

				if (!platformSettings.overridden) {
					continue;
				}

				disabledPlatforms.Add(platform);
				platformSettings.overridden = false;
				importer.SetPlatformTextureSettings(platformSettings);
			}

			if (disabledPlatforms.Count <= 0) {
				return false;
			}

			importer.SaveAndReimport();
			return true;
		}

		public static void EnableOverrides(TextureImporter importer, List<string> platformsToEnable) {
			if (platformsToEnable.Count == 0) {
				return;
			}

			foreach (string platform in platformsToEnable) {
				var platformSettings = importer.GetPlatformTextureSettings(platform);
				platformSettings.overridden = true;
				importer.SetPlatformTextureSettings(platformSettings);
			}

			importer.SaveAndReimport();
		}
	}
}