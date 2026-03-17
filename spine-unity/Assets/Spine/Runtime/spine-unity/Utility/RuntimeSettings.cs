/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

using System.IO;
using UnityEngine;

namespace Spine.Unity {

	/// <summary>
	/// Project settings stored in a ScriptableObject for use in the build at runtime.
	/// Writing the ScriptableObject asset is performed by <see cref="RuntimeSettingsEditor"/>,
	/// triggered by preferences setting changes.
	/// </summary>
	public class RuntimeSettings : ScriptableObject {

		public bool useThreadedMeshGeneration = false;
		public static bool UseThreadedMeshGeneration {
			get { return Instance.useThreadedMeshGeneration; }
			set { Instance.useThreadedMeshGeneration = value; }
		}

		public bool useThreadedAnimation = false;
		public static bool UseThreadedAnimation{
			get { return Instance.useThreadedAnimation; }
			set { Instance.useThreadedAnimation = value; }
		}

		/// <summary>Path relative to "Assets/Resources".</summary>
		public const string ResourcePath = "SpineRuntimeSettings";

		private static RuntimeSettings singletonInstance;
		public static RuntimeSettings Instance {
			get {
				if (singletonInstance == null) {
					singletonInstance = Resources.Load<RuntimeSettings>(ResourcePath);
				}
				if (singletonInstance == null) {
					singletonInstance = CreateInstance<RuntimeSettings>();
				}
				return singletonInstance;
			}
		}
	}
}
