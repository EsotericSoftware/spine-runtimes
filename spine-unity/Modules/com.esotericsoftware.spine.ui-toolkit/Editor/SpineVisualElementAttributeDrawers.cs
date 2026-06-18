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

//#define CHANGE_BOUNDS_ON_ANIMATION_CHANGE

using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spine.Unity.Editor {

	[CustomPropertyDrawer(typeof(UITKBlendModeMaterialsAttribute))]
	public class UITKBlendModeMaterialsAttributeDrawer : PropertyDrawer {

		protected UITKBlendModeMaterialsAttribute TargetAttribute { get { return (UITKBlendModeMaterialsAttribute)attribute; } }

		public override VisualElement CreatePropertyGUI (SerializedProperty materialsProperty) {
			var container = new VisualElement();
			PropertyField blendModeMaterials = new PropertyField();
			blendModeMaterials.BindProperty(materialsProperty);

			SerializedProperty normalMaterialProperty = materialsProperty.FindPropertyRelative("normalMaterial");
			SerializedProperty additiveMaterialProperty = materialsProperty.FindPropertyRelative("additiveMaterial");
			SerializedProperty multiplyMaterialProperty = materialsProperty.FindPropertyRelative("multiplyMaterial");
			SerializedProperty screenMaterialProperty = materialsProperty.FindPropertyRelative("screenMaterial");
			PropertyField normalField = new PropertyField();
			PropertyField additiveField = new PropertyField();
			PropertyField multiplyField = new PropertyField();
			PropertyField screenField = new PropertyField();

			normalField.BindProperty(normalMaterialProperty);
			additiveField.BindProperty(additiveMaterialProperty);
			multiplyField.BindProperty(multiplyMaterialProperty);
			screenField.BindProperty(screenMaterialProperty);

			var parentPropertyPath = materialsProperty.propertyPath.Substring(0, materialsProperty.propertyPath.LastIndexOf('.'));
			var parent = materialsProperty.serializedObject.FindProperty(parentPropertyPath);
			SerializedProperty skeletonDataProperty = parent.FindPropertyRelative(TargetAttribute.dataField);

			Button detectMaterialsButton = new Button(() => {
				DetectMaterials(materialsProperty, (SkeletonDataAsset)skeletonDataProperty.objectReferenceValue);
			});
			detectMaterialsButton.text = "Detect Materials";
			container.Add(detectMaterialsButton);

			//container.Add(blendModeMaterials);
			container.Add(normalField);
			container.Add(additiveField);
			container.Add(multiplyField);
			container.Add(screenField);

			container.Bind(materialsProperty.serializedObject);
			return container;
		}

		protected void DetectMaterials (SerializedProperty materialsProperty, SkeletonDataAsset skeletonDataAsset) {
			if (!skeletonDataAsset)
				return;

			SerializedProperty normalMaterialProperty = materialsProperty.FindPropertyRelative("normalMaterial");
			SerializedProperty additiveMaterialProperty = materialsProperty.FindPropertyRelative("additiveMaterial");
			SerializedProperty multiplyMaterialProperty = materialsProperty.FindPropertyRelative("multiplyMaterial");
			SerializedProperty screenMaterialProperty = materialsProperty.FindPropertyRelative("screenMaterial");
			bool hasPMATextures = HasPMATextures(skeletonDataAsset);

			if (hasPMATextures) {
				AssignMaterial(normalMaterialProperty, "Spine-UITK-Normal-PMA");
			} else {
				AssignMaterial(normalMaterialProperty, "Spine-UITK-Normal-Straight");
			}

			if (!skeletonDataAsset.blendModeMaterials.RequiresBlendModeMaterials) {
				additiveMaterialProperty.objectReferenceValue = null;
				multiplyMaterialProperty.objectReferenceValue = null;
				screenMaterialProperty.objectReferenceValue = null;
			} else {
				if (hasPMATextures) {
					AssignMaterial(additiveMaterialProperty, "Spine-UITK-Additive-PMA");
					AssignMaterial(multiplyMaterialProperty, "Spine-UITK-Multiply-PMA");
					AssignMaterial(screenMaterialProperty, "Spine-UITK-Screen-PMA");
				} else {
					AssignMaterial(additiveMaterialProperty, "Spine-UITK-Additive-Straight");
					AssignMaterial(multiplyMaterialProperty, "Spine-UITK-Multiply-Straight");
					AssignMaterial(screenMaterialProperty, "Spine-UITK-Screen-Straight");
				}
			}
			materialsProperty.serializedObject.ApplyModifiedProperties();
		}

		bool HasPMATextures (SkeletonDataAsset skeletonDataAsset) {
			if (skeletonDataAsset.atlasAssets.Length == 0) return false;
			AtlasAssetBase firstAtlasAsset = skeletonDataAsset.atlasAssets[0];
			if (firstAtlasAsset.MaterialCount == 0) return false;
			
			Texture texture = firstAtlasAsset.Materials.First().mainTexture;
			bool detectionSucceeded;
			return IsSkeletonTexturePMA(texture, skeletonDataAsset.name, out detectionSucceeded);
		}

		void AssignMaterial (SerializedProperty property, string name) {
			Material material = MaterialWithName(name);
			if (material != null)
				property.objectReferenceValue = material;
		}

		public static Material MaterialWithName (string name) {
			return SkeletonGraphicUtility.MaterialWithName(name);
		}

		public static bool IsSkeletonTexturePMA (Texture texture, string skeletonName, out bool detectionSucceeded) {
			return SkeletonGraphicUtility.IsSkeletonTexturePMA(texture, skeletonName, out detectionSucceeded);
		}
	}

	[CustomPropertyDrawer(typeof(BoundsFromAnimationAttribute))]
	public class BoundsFromAnimationAttributeDrawer : PropertyDrawer {

		protected BoundsFromAnimationAttribute TargetAttribute { get { return (BoundsFromAnimationAttribute)attribute; } }

		public override VisualElement CreatePropertyGUI (SerializedProperty boundsProperty) {
			var container = new VisualElement();
			PropertyField referenceMeshBounds = new PropertyField();
			referenceMeshBounds.BindProperty(boundsProperty);

			var parentPropertyPath = boundsProperty.propertyPath.Substring(0, boundsProperty.propertyPath.LastIndexOf('.'));
			var parent = boundsProperty.serializedObject.FindProperty(parentPropertyPath);
			SerializedProperty animationProperty = parent.FindPropertyRelative(TargetAttribute.animationField);
			SerializedProperty skeletonDataProperty = parent.FindPropertyRelative(TargetAttribute.dataField);
			SerializedProperty skinProperty = parent.FindPropertyRelative(TargetAttribute.skinField);

#if !CHANGE_BOUNDS_ON_ANIMATION_CHANGE
			Button updateBoundsButton = new Button(() => {
				UpdateMeshBounds(boundsProperty, animationProperty.stringValue,
					(SkeletonDataAsset)skeletonDataProperty.objectReferenceValue, skinProperty.stringValue);
			});
			updateBoundsButton.text = "Update Bounds";
			container.Add(updateBoundsButton);
#else
			referenceMeshBounds.TrackPropertyValue(animationProperty, prop => {
				UpdateMeshBounds(boundsProperty, animationProperty.stringValue,
					(SkeletonDataAsset)skeletonDataProperty.objectReferenceValue, skinProperty.stringValue);
			});
#endif
			container.Add(referenceMeshBounds);

			container.Bind(boundsProperty.serializedObject);
			return container;
		}

		protected void UpdateMeshBounds (SerializedProperty boundsProperty, string boundsAnimation,
			SkeletonDataAsset skeletonDataAsset, string skin) {
			if (!skeletonDataAsset)
				return;

			Bounds bounds = CalculateMeshBounds(boundsAnimation, skeletonDataAsset, skin);
			if (bounds.extents.x == 0 || bounds.extents.y == 0) {
				Debug.LogWarning("Please select different Initial Skin and Bounds Animation. Not setting reference " +
					"bounds as current combination (likely no attachments visible) leads to zero Mesh bounds.");
				bounds.center = Vector3.zero;
				bounds.extents = Vector3.one * 2f;
			}
			boundsProperty.boundsValue = bounds;
			boundsProperty.serializedObject.ApplyModifiedProperties();
		}

		protected Bounds CalculateMeshBounds (string animationName, SkeletonDataAsset skeletonDataAsset, string skin) {
			SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
			Skeleton skeleton = new Skeleton(skeletonData);
			if (!string.IsNullOrEmpty(skin) && !string.Equals(skin, "default", System.StringComparison.Ordinal))
				skeleton.SetSkin(skin);
			skeleton.SetupPose();

			Spine.Animation animation = skeletonData.FindAnimation(animationName);
			if (animation != null)
				animation.Apply(skeleton, -1, 0, false, null, 1.0f, MixFrom.Setup, false, false, false);

			skeleton.Update(0f);
			skeleton.UpdateWorldTransform(Physics.Update);

			float x, y, width, height;
			SkeletonClipping clipper = new SkeletonClipping();
			float[] vertexBuffer = null;
			skeleton.GetBounds(out x, out y, out width, out height, ref vertexBuffer, clipper);
			if (x == int.MaxValue) {
				return new Bounds();
			}

			Bounds bounds = new Bounds();
			Vector2 halfSize = new Vector2(width * 0.5f, height * 0.5f);
			bounds.center = new Vector3(x + halfSize.x, -y - halfSize.y, 0.0f);
			bounds.extents = new Vector3(halfSize.x, halfSize.y, 0.0f);
			return bounds;
		}
	}
}
