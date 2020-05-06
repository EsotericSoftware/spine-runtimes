using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spine.Unity.Tests
{
    public class RunAnimationStateTests
    {
        [Test]
        public void RunAnimationStateTestsSimplePasses ()
        {
			AnimationStateTests.logImplementation += Log;
			AnimationStateTests.failImplementation += Fail;
			AnimationStateTests.Main("Assets/SpineTests/spine-csharp-tests/tests/assets/test.json");
        }

		public void Log (string message) {
			UnityEngine.Debug.Log(message);
		}

		public void Fail (string message) {
			Assert.Fail(message);
		}
	}
}
