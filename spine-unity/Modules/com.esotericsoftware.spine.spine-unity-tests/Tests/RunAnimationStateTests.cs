/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
