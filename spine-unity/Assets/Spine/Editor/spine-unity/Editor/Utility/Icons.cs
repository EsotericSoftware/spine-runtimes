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

#pragma warning disable 0219

#define SPINE_SKELETONMECANIM

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	public partial class SpineEditorUtilities {
		public static class Icons {
			public static Texture2D skeleton;
			public static Texture2D nullBone;
			public static Texture2D bone;
			public static Texture2D poseBones;
			public static Texture2D boneNib;
			public static Texture2D slot;
			public static Texture2D slotRoot;
			public static Texture2D skinPlaceholder;
			public static Texture2D image;
			public static Texture2D genericAttachment;
			public static Texture2D boundingBox;
			public static Texture2D point;
			public static Texture2D mesh;
			public static Texture2D weights;
			public static Texture2D path;
			public static Texture2D clipping;
			public static Texture2D skin;
			public static Texture2D skinsRoot;
			public static Texture2D animation;
			public static Texture2D animationRoot;
			public static Texture2D spine;
			public static Texture2D userEvent;
			public static Texture2D constraintNib;
			public static Texture2D constraintRoot;
			public static Texture2D constraintTransform;
			public static Texture2D constraintPath;
			public static Texture2D constraintIK;
			public static Texture2D warning;
			public static Texture2D skeletonUtility;
			public static Texture2D hingeChain;
			public static Texture2D subMeshRenderer;
			public static Texture2D skeletonDataAssetIcon;
			public static Texture2D info;
			public static Texture2D unity;

			static Texture2D LoadIcon (string filename) {
				return (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/" + filename);
			}

			public static void Initialize () {
				skeleton = LoadIcon("icon-skeleton.png");
				nullBone = LoadIcon("icon-null.png");
				bone = LoadIcon("icon-bone.png");
				poseBones = LoadIcon("icon-poseBones.png");
				boneNib = LoadIcon("icon-boneNib.png");
				slot = LoadIcon("icon-slot.png");
				slotRoot = LoadIcon("icon-slotRoot.png");
				skinPlaceholder = LoadIcon("icon-skinPlaceholder.png");

				genericAttachment = LoadIcon("icon-attachment.png");
				image = LoadIcon("icon-image.png");
				boundingBox = LoadIcon("icon-boundingBox.png");
				point = LoadIcon("icon-point.png");
				mesh = LoadIcon("icon-mesh.png");
				weights = LoadIcon("icon-weights.png");
				path = LoadIcon("icon-path.png");
				clipping = LoadIcon("icon-clipping.png");

				skin = LoadIcon("icon-skin.png");
				skinsRoot = LoadIcon("icon-skinsRoot.png");
				animation = LoadIcon("icon-animation.png");
				animationRoot = LoadIcon("icon-animationRoot.png");
				spine = LoadIcon("icon-spine.png");
				userEvent = LoadIcon("icon-event.png");
				constraintNib = LoadIcon("icon-constraintNib.png");

				constraintRoot = LoadIcon("icon-constraints.png");
				constraintTransform = LoadIcon("icon-constraintTransform.png");
				constraintPath = LoadIcon("icon-constraintPath.png");
				constraintIK = LoadIcon("icon-constraintIK.png");

				warning = LoadIcon("icon-warning.png");
				skeletonUtility = LoadIcon("icon-skeletonUtility.png");
				hingeChain = LoadIcon("icon-hingeChain.png");
				subMeshRenderer = LoadIcon("icon-subMeshRenderer.png");

				skeletonDataAssetIcon = LoadIcon("SkeletonDataAsset Icon.png");

				info = EditorGUIUtility.FindTexture("console.infoicon.sml");
				unity = EditorGUIUtility.FindTexture("SceneAsset Icon");
			}

			public static Texture2D GetAttachmentIcon (Attachment attachment) {
				// Analysis disable once CanBeReplacedWithTryCastAndCheckForNull
				if (attachment is RegionAttachment)
					return Icons.image;
				else if (attachment is MeshAttachment)
					return ((MeshAttachment)attachment).IsWeighted() ? Icons.weights : Icons.mesh;
				else if (attachment is BoundingBoxAttachment)
					return Icons.boundingBox;
				else if (attachment is PointAttachment)
					return Icons.point;
				else if (attachment is PathAttachment)
					return Icons.path;
				else if (attachment is ClippingAttachment)
					return Icons.clipping;
				else
					return Icons.warning;
			}
		}
	}
}
