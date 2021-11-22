using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spine.Unity.Tests {
	public class RunAnimationStateTests {
		[Test]
		public void RunAnimationStateTestsSimplePasses () {
			AnimationStateTests.logImplementation += Log;
			AnimationStateTests.failImplementation += Fail;

			string testJsonFilename = "test";
			string testJsonPathEnd = "tests/assets/" + testJsonFilename + ".json";
			var guids = UnityEditor.AssetDatabase.FindAssets(testJsonFilename + " t:textasset");
			if (guids.Length <= 0) Fail(testJsonFilename + ".json asset not found.");

			foreach (var guid in guids) {
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				if (assetPath.EndsWith(testJsonPathEnd)) {
					AnimationStateTests.Main(assetPath);
					return;
				}
			}
			Fail(testJsonPathEnd + " not found.");
		}

		public void Log (string message) {
			UnityEngine.Debug.Log(message);
		}

		public void Fail (string message) {
			Assert.Fail(message);
		}
	}
}
