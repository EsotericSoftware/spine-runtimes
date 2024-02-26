/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2024, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Spine.Unity.Editor {

	/// <summary>
	/// Utility class for working with TextureImporter.
	/// </summary>
	public static class TextureImporterUtility {

		private static IEnumerable<string> GetAllPlatforms() {
			BuildTarget[] buildTargets = (BuildTarget[])Enum.GetValues(typeof(BuildTarget));
			var platformNames = buildTargets.Select(x => x.ToString()).ToList();

			// Add additional platforms that are not part of BuildTarget enum.
			platformNames.Add("Server");

			return platformNames.ToArray();
		}

		/// <summary>Disables Texture Import settings platform overrides for all platforms.</summary>
		/// <param name="importer">The TextureImporter wrapper of the target texture asset.</param>
		/// <param name="disabledPlatforms">A list populated with platforms where overrides were previously enabled and
		/// which have now been disabled.</param>
		/// <returns>True if an override has been disabled for any platform, false otherwise.</returns>
		public static bool DisableOverrides(TextureImporter importer, out List<string> disabledPlatforms) {
			IEnumerable<string> platforms = GetAllPlatforms();
			disabledPlatforms = new List<string>();

			foreach (string platform in platforms) {
				var platformSettings = importer.GetPlatformTextureSettings(platform);
				if (!platformSettings.overridden)
					continue;

				disabledPlatforms.Add(platform);
				platformSettings.overridden = false;
				importer.SetPlatformTextureSettings(platformSettings);
			}

			if (disabledPlatforms.Count <= 0)
				return false;

			importer.SaveAndReimport();
			return true;
		}

		/// <summary>Enables Texture Import settings platform overrides for given platforms.</summary>
		/// <param name="importer">The TextureImporter wrapper of the target texture asset.</param>
		/// <param name="platformsToEnable">A list of platforms for which overrides shall be enabled.</param>
		public static void EnableOverrides(TextureImporter importer, List<string> platformsToEnable) {
			if (platformsToEnable.Count == 0)
				return;

			foreach (string platform in platformsToEnable) {
				TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings(platform);
				platformSettings.overridden = true;
				importer.SetPlatformTextureSettings(platformSettings);
			}
			importer.SaveAndReimport();
		}
	}
}
