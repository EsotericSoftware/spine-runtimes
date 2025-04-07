/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

// Contributed by: Mitch Thompson

#if UNITY_2023_2_OR_NEWER
#define MENU_REQUIRES_DIFFERENT_NESTED_NAME
#endif

using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	public struct SpineDrawerValuePair {
		public string stringValue;
		public SerializedProperty property;

		public SpineDrawerValuePair (string val, SerializedProperty property) {
			this.stringValue = val;
			this.property = property;
		}
	}

	public abstract class SpineTreeItemDrawerBase<T> : PropertyDrawer where T : SpineAttributeBase {
		protected SkeletonDataAsset skeletonDataAsset;
		internal const string NoneStringConstant = "<None>";

		internal virtual string NoneString { get { return NoneStringConstant; } }

		GUIContent noneLabel;
		GUIContent NoneLabel (Texture2D image = null) {
			if (noneLabel == null) noneLabel = new GUIContent(NoneString);
			noneLabel.image = image;
			return noneLabel;
		}

		static GUIStyle errorPopupStyle;
		GUIStyle ErrorPopupStyle {
			get {
				if (errorPopupStyle == null) errorPopupStyle = new GUIStyle(EditorStyles.popup);
				errorPopupStyle.normal.textColor = Color.red;
				errorPopupStyle.hover.textColor = Color.red;
				errorPopupStyle.focused.textColor = Color.red;
				errorPopupStyle.active.textColor = Color.red;
				return errorPopupStyle;
			}
		}

		protected T TargetAttribute { get { return (T)attribute; } }
		protected SerializedProperty SerializedProperty { get; private set; }

		protected abstract Texture2D Icon { get; }

		protected bool IsValueValid (SerializedProperty property) {
			if (skeletonDataAsset != null) {
				SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
				if (skeletonData != null && !string.IsNullOrEmpty(property.stringValue))
					return IsValueValid(skeletonData, property);
			}
			return true;
		}

		protected virtual bool IsValueValid (SkeletonData skeletonData, SerializedProperty property) { return true; }

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			SerializedProperty = property;

			if (property.propertyType != SerializedPropertyType.String) {
				EditorGUI.LabelField(position, "ERROR:", "May only apply to type string");
				return;
			}

			// Handle multi-editing when instances don't use the same SkeletonDataAsset.
			if (!SpineInspectorUtility.TargetsUseSameData(property.serializedObject)) {
				EditorGUI.DelayedTextField(position, property, label);
				return;
			}

			SerializedProperty dataField = property.FindBaseOrSiblingProperty(TargetAttribute.dataField);

			if (dataField != null) {
				UnityEngine.Object objectReferenceValue = dataField.objectReferenceValue;
				if (objectReferenceValue is SkeletonDataAsset) {
					skeletonDataAsset = (SkeletonDataAsset)objectReferenceValue;
				} else if (objectReferenceValue is IHasSkeletonDataAsset) {
					IHasSkeletonDataAsset hasSkeletonDataAsset = (IHasSkeletonDataAsset)objectReferenceValue;
					if (hasSkeletonDataAsset != null)
						skeletonDataAsset = hasSkeletonDataAsset.SkeletonDataAsset;
				} else if (objectReferenceValue != null) {
					EditorGUI.LabelField(position, "ERROR:", "Invalid reference type");
					return;
				}

			} else {
				UnityEngine.Object targetObject = property.serializedObject.targetObject;

				IHasSkeletonDataAsset hasSkeletonDataAsset = targetObject as IHasSkeletonDataAsset;
				if (hasSkeletonDataAsset == null) {
					Component component = targetObject as Component;
					if (component != null)
						hasSkeletonDataAsset = component.GetComponentInChildren(typeof(IHasSkeletonDataAsset)) as IHasSkeletonDataAsset;
				}

				if (hasSkeletonDataAsset != null)
					skeletonDataAsset = hasSkeletonDataAsset.SkeletonDataAsset;
			}

			if (skeletonDataAsset == null) {
				if (TargetAttribute.fallbackToTextField) {
					EditorGUI.PropertyField(position, property); //EditorGUI.TextField(position, label, property.stringValue);
				} else {
					EditorGUI.LabelField(position, "ERROR:", "Must have reference to a SkeletonDataAsset");
				}

				skeletonDataAsset = property.serializedObject.targetObject as SkeletonDataAsset;
				if (skeletonDataAsset == null) return;
			}

			position = EditorGUI.PrefixLabel(position, label);

			Texture2D image = Icon;
			GUIStyle usedStyle = IsValueValid(property) ? EditorStyles.popup : ErrorPopupStyle;
			string propertyStringValue = (property.hasMultipleDifferentValues) ? SpineInspectorUtility.EmDash : property.stringValue;

			if (!TargetAttribute.avoidGenericMenu) {
				if (GUI.Button(position, string.IsNullOrEmpty(propertyStringValue) ? NoneLabel(image) :
					SpineInspectorUtility.TempContent(propertyStringValue, image), usedStyle))
					Selector(property);
			} else {
				SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
				List<GUIContent> contentList = new List<GUIContent>();
				List<string> valueList = new List<string>();
				PopulatePopupList(ref contentList, ref valueList, image, property, TargetAttribute, skeletonData);
				int currentIndex = valueList.IndexOf(propertyStringValue);
				int previousIndex = currentIndex;
				currentIndex = EditorGUI.Popup(position, currentIndex, contentList.ToArray());
				if (previousIndex != currentIndex) {
					property.stringValue = valueList[currentIndex];
					property.serializedObject.ApplyModifiedProperties();
				}
			}
		}

		public ISkeletonComponent GetTargetSkeletonComponent (SerializedProperty property) {
			SerializedProperty dataField = property.FindBaseOrSiblingProperty(TargetAttribute.dataField);

			if (dataField != null) {
				ISkeletonComponent skeletonComponent = dataField.objectReferenceValue as ISkeletonComponent;
				if (dataField.objectReferenceValue != null && skeletonComponent != null) // note the overloaded UnityEngine.Object == null check. Do not simplify.
					return skeletonComponent;
			} else {
				Component component = property.serializedObject.targetObject as Component;
				if (component != null)
					return component.GetComponentInChildren(typeof(ISkeletonComponent)) as ISkeletonComponent;
			}

			return null;
		}

		protected virtual void Selector (SerializedProperty property) {
			SkeletonData data = skeletonDataAsset.GetSkeletonData(true);
			if (data == null) return;

			GenericMenu menu = new GenericMenu();
			PopulateMenu(menu, property, this.TargetAttribute, data);
			menu.ShowAsContext();
		}

		protected abstract void PopulateMenu (GenericMenu menu, SerializedProperty property, T targetAttribute, SkeletonData data);

		protected virtual void HandleSelect (object menuItemObject) {
			SpineDrawerValuePair clickedItem = (SpineDrawerValuePair)menuItemObject;
			SerializedProperty serializedProperty = clickedItem.property;
			if (serializedProperty.serializedObject.isEditingMultipleObjects) serializedProperty.stringValue = "oaifnoiasf��123526"; // HACK: to trigger change on multi-editing.
			serializedProperty.stringValue = clickedItem.stringValue;
			serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		protected virtual void PopulatePopupList (ref List<GUIContent> contentList, ref List<string> valueList,
			Texture2D image, SerializedProperty property, T targetAttribute, SkeletonData data) {
			contentList.Add(new GUIContent("Type Not Supported"));
			valueList.Add(string.Empty);
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			return 18;
		}

	}

	[CustomPropertyDrawer(typeof(SpineSlot))]
	public class SpineSlotDrawer : SpineTreeItemDrawerBase<SpineSlot> {

		protected override Texture2D Icon { get { return SpineEditorUtilities.Icons.slot; } }

		protected override bool IsValueValid (SkeletonData skeletonData, SerializedProperty property) {
			return skeletonData.FindSlot(property.stringValue) != null;
		}

		protected override void PopulateMenu (GenericMenu menu, SerializedProperty property, SpineSlot targetAttribute, SkeletonData data) {
			if (TargetAttribute.includeNone)
				menu.AddItem(new GUIContent(NoneString), !property.hasMultipleDifferentValues && string.IsNullOrEmpty(property.stringValue), HandleSelect, new SpineDrawerValuePair(string.Empty, property));

			IEnumerable<SlotData> orderedSlots = data.Slots.Items.OrderBy(slotData => slotData.Name);
			foreach (SlotData slotData in orderedSlots) {
				int slotIndex = slotData.Index;
				string name = slotData.Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal)) {

					if (targetAttribute.containsBoundingBoxes) {
						List<Skin.SkinEntry> skinEntries = new List<Skin.SkinEntry>();
						foreach (Skin skin in data.Skins) {
							skin.GetAttachments(slotIndex, skinEntries);
						}

						bool hasBoundingBox = false;
						foreach (Skin.SkinEntry entry in skinEntries) {
							BoundingBoxAttachment bbAttachment = entry.Attachment as BoundingBoxAttachment;
							if (bbAttachment != null) {
								string menuLabel = bbAttachment.IsWeighted() ? name + " (!)" : name;
								menu.AddItem(new GUIContent(menuLabel), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
								hasBoundingBox = true;
								break;
							}
						}

						if (!hasBoundingBox)
							menu.AddDisabledItem(new GUIContent(name));

					} else {
						menu.AddItem(new GUIContent(name), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
					}

				}

			}
		}

	}

	[CustomPropertyDrawer(typeof(SpineSkin))]
	public class SpineSkinDrawer : SpineTreeItemDrawerBase<SpineSkin> {
		const string DefaultSkinName = "default";

		protected override Texture2D Icon { get { return SpineEditorUtilities.Icons.skin; } }

		internal override string NoneString { get { return TargetAttribute.defaultAsEmptyString ? DefaultSkinName : NoneStringConstant; } }

		protected override bool IsValueValid (SkeletonData skeletonData, SerializedProperty property) {
			return skeletonData.FindSkin(property.stringValue) != null;
		}

		public static void GetSkinMenuItems (SkeletonData data, List<string> outputNames, List<GUIContent> outputMenuItems, bool includeNone = true) {
			if (data == null) return;
			if (outputNames == null) return;
			if (outputMenuItems == null) return;

			ExposedList<Skin> skins = data.Skins;

			outputNames.Clear();
			outputMenuItems.Clear();

			Texture2D icon = SpineEditorUtilities.Icons.skin;

			if (includeNone) {
				outputNames.Add("");
				outputMenuItems.Add(new GUIContent(NoneStringConstant, icon));
			}

			foreach (Skin s in skins) {
				string skinName = s.Name;
				outputNames.Add(skinName);
				outputMenuItems.Add(new GUIContent(skinName, icon));
			}
		}

		protected override void PopulateMenu (GenericMenu menu, SerializedProperty property, SpineSkin targetAttribute, SkeletonData data) {
			menu.AddDisabledItem(new GUIContent(skeletonDataAsset.name));
			menu.AddSeparator("");

			if (targetAttribute.includeNone)
				menu.AddItem(new GUIContent(NoneStringConstant), !property.hasMultipleDifferentValues && string.IsNullOrEmpty(property.stringValue), HandleSelect, new SpineDrawerValuePair(string.Empty, property));

			for (int i = 0; i < data.Skins.Count; i++) {
				string name = data.Skins.Items[i].Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal)) {
					bool isDefault = string.Equals(name, DefaultSkinName, StringComparison.Ordinal);
					string choiceValue = TargetAttribute.defaultAsEmptyString && isDefault ? string.Empty : name;
					menu.AddItem(new GUIContent(name), !property.hasMultipleDifferentValues && choiceValue == property.stringValue, HandleSelect, new SpineDrawerValuePair(choiceValue, property));
				}

			}
		}

		protected override void PopulatePopupList (ref List<GUIContent> contentList, ref List<string> valueList,
			Texture2D image, SerializedProperty property, SpineSkin targetAttribute, SkeletonData data) {

			if (targetAttribute.includeNone) {
				contentList.Add(new GUIContent(NoneStringConstant, image));
				valueList.Add(string.Empty);
			}

			for (int i = 0; i < data.Skins.Count; i++) {
				string name = data.Skins.Items[i].Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal)) {
					bool isDefault = string.Equals(name, DefaultSkinName, StringComparison.Ordinal);
					string choiceValue = TargetAttribute.defaultAsEmptyString && isDefault ? string.Empty : name;
					contentList.Add(new GUIContent(name, image));
					valueList.Add(choiceValue);
				}
			}
		}

	}

	[CustomPropertyDrawer(typeof(SpineAnimation))]
	public class SpineAnimationDrawer : SpineTreeItemDrawerBase<SpineAnimation> {

		protected override Texture2D Icon { get { return SpineEditorUtilities.Icons.animation; } }

		protected override bool IsValueValid (SkeletonData skeletonData, SerializedProperty property) {
			return skeletonData.FindAnimation(property.stringValue) != null;
		}

		public static void GetAnimationMenuItems (SkeletonData data, List<string> outputNames, List<GUIContent> outputMenuItems, bool includeNone = true) {
			if (data == null) return;
			if (outputNames == null) return;
			if (outputMenuItems == null) return;

			ExposedList<Animation> animations = data.Animations;

			outputNames.Clear();
			outputMenuItems.Clear();

			if (includeNone) {
				outputNames.Add("");
				outputMenuItems.Add(new GUIContent(NoneStringConstant, SpineEditorUtilities.Icons.animation));
			}

			foreach (Animation a in animations) {
				string animationName = a.Name;
				outputNames.Add(animationName);
				outputMenuItems.Add(new GUIContent(animationName, SpineEditorUtilities.Icons.animation));
			}
		}

		protected override void PopulateMenu (GenericMenu menu, SerializedProperty property, SpineAnimation targetAttribute, SkeletonData data) {
			ExposedList<Animation> animations = skeletonDataAsset.GetAnimationStateData().SkeletonData.Animations;

			if (TargetAttribute.includeNone)
				menu.AddItem(new GUIContent(NoneString), !property.hasMultipleDifferentValues && string.IsNullOrEmpty(property.stringValue), HandleSelect, new SpineDrawerValuePair(string.Empty, property));

			for (int i = 0; i < animations.Count; i++) {
				string name = animations.Items[i].Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal))
					menu.AddItem(new GUIContent(name), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
			}
		}

		protected override void PopulatePopupList (ref List<GUIContent> contentList, ref List<string> valueList,
			Texture2D image, SerializedProperty property, SpineAnimation targetAttribute, SkeletonData data) {

			ExposedList<Animation> animations = data.Animations;
			if (targetAttribute.includeNone) {
				contentList.Add(new GUIContent(NoneString, image));
				valueList.Add(string.Empty);
			}

			for (int i = 0; i < animations.Count; i++) {
				string name = animations.Items[i].Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal)) {
					contentList.Add(new GUIContent(name, image));
					valueList.Add(name);
				}
			}
		}

	}

	[CustomPropertyDrawer(typeof(SpineEvent))]
	public class SpineEventNameDrawer : SpineTreeItemDrawerBase<SpineEvent> {

		protected override Texture2D Icon { get { return SpineEditorUtilities.Icons.userEvent; } }

		protected override bool IsValueValid (SkeletonData skeletonData, SerializedProperty property) {
			return skeletonData.FindEvent(property.stringValue) != null;
		}

		public static void GetEventMenuItems (SkeletonData data, List<string> eventNames, List<GUIContent> menuItems, bool includeNone = true) {
			if (data == null) return;

			ExposedList<EventData> animations = data.Events;

			eventNames.Clear();
			menuItems.Clear();

			if (includeNone) {
				eventNames.Add("");
				menuItems.Add(new GUIContent(NoneStringConstant, SpineEditorUtilities.Icons.userEvent));
			}

			foreach (EventData a in animations) {
				string animationName = a.Name;
				eventNames.Add(animationName);
				menuItems.Add(new GUIContent(animationName, SpineEditorUtilities.Icons.userEvent));
			}
		}

		protected override void PopulateMenu (GenericMenu menu, SerializedProperty property, SpineEvent targetAttribute, SkeletonData data) {
			ExposedList<EventData> events = skeletonDataAsset.GetSkeletonData(false).Events;

			if (TargetAttribute.includeNone)
				menu.AddItem(new GUIContent(NoneString), !property.hasMultipleDifferentValues && string.IsNullOrEmpty(property.stringValue), HandleSelect, new SpineDrawerValuePair(string.Empty, property));

			for (int i = 0; i < events.Count; i++) {
				EventData eventObject = events.Items[i];
				string name = eventObject.Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal)) {
					if (!TargetAttribute.audioOnly || !string.IsNullOrEmpty(eventObject.AudioPath)) {
						menu.AddItem(new GUIContent(name), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
					}
				}

			}
		}

	}

	[CustomPropertyDrawer(typeof(SpineIkConstraint))]
	public class SpineIkConstraintDrawer : SpineTreeItemDrawerBase<SpineIkConstraint> {

		protected override Texture2D Icon { get { return SpineEditorUtilities.Icons.constraintIK; } }

		protected override bool IsValueValid (SkeletonData skeletonData, SerializedProperty property) {
			return skeletonData.FindIkConstraint(property.stringValue) != null;
		}

		protected override void PopulateMenu (GenericMenu menu, SerializedProperty property, SpineIkConstraint targetAttribute, SkeletonData data) {
			ExposedList<IkConstraintData> constraints = skeletonDataAsset.GetSkeletonData(false).IkConstraints;

			if (TargetAttribute.includeNone)
				menu.AddItem(new GUIContent(NoneString), !property.hasMultipleDifferentValues && string.IsNullOrEmpty(property.stringValue), HandleSelect, new SpineDrawerValuePair(string.Empty, property));

			for (int i = 0; i < constraints.Count; i++) {
				string name = constraints.Items[i].Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal))
					menu.AddItem(new GUIContent(name), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
			}
		}

	}

	[CustomPropertyDrawer(typeof(SpineTransformConstraint))]
	public class SpineTransformConstraintDrawer : SpineTreeItemDrawerBase<SpineTransformConstraint> {

		protected override Texture2D Icon { get { return SpineEditorUtilities.Icons.constraintTransform; } }

		protected override bool IsValueValid (SkeletonData skeletonData, SerializedProperty property) {
			return skeletonData.FindTransformConstraint(property.stringValue) != null;
		}

		protected override void PopulateMenu (GenericMenu menu, SerializedProperty property, SpineTransformConstraint targetAttribute, SkeletonData data) {
			ExposedList<TransformConstraintData> constraints = skeletonDataAsset.GetSkeletonData(false).TransformConstraints;

			if (TargetAttribute.includeNone)
				menu.AddItem(new GUIContent(NoneString), !property.hasMultipleDifferentValues && string.IsNullOrEmpty(property.stringValue), HandleSelect, new SpineDrawerValuePair(string.Empty, property));

			for (int i = 0; i < constraints.Count; i++) {
				string name = constraints.Items[i].Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal))
					menu.AddItem(new GUIContent(name), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
			}
		}
	}

	[CustomPropertyDrawer(typeof(SpinePathConstraint))]
	public class SpinePathConstraintDrawer : SpineTreeItemDrawerBase<SpinePathConstraint> {

		protected override Texture2D Icon { get { return SpineEditorUtilities.Icons.constraintPath; } }

		protected override bool IsValueValid (SkeletonData skeletonData, SerializedProperty property) {
			return skeletonData.FindPathConstraint(property.stringValue) != null;
		}

		protected override void PopulateMenu (GenericMenu menu, SerializedProperty property, SpinePathConstraint targetAttribute, SkeletonData data) {
			ExposedList<PathConstraintData> constraints = skeletonDataAsset.GetSkeletonData(false).PathConstraints;

			if (TargetAttribute.includeNone)
				menu.AddItem(new GUIContent(NoneString), !property.hasMultipleDifferentValues && string.IsNullOrEmpty(property.stringValue), HandleSelect, new SpineDrawerValuePair(string.Empty, property));

			for (int i = 0; i < constraints.Count; i++) {
				string name = constraints.Items[i].Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal))
					menu.AddItem(new GUIContent(name), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
			}
		}
	}

	[CustomPropertyDrawer(typeof(SpineAttachment))]
	public class SpineAttachmentDrawer : SpineTreeItemDrawerBase<SpineAttachment> {

		protected override Texture2D Icon { get { return SpineEditorUtilities.Icons.genericAttachment; } }

		protected override void PopulateMenu (GenericMenu menu, SerializedProperty property, SpineAttachment targetAttribute, SkeletonData data) {
			ISkeletonComponent skeletonComponent = GetTargetSkeletonComponent(property);
			List<Skin> validSkins = new List<Skin>();

			if (skeletonComponent != null && targetAttribute.currentSkinOnly) {
				Skin currentSkin = null;

				SerializedProperty skinProperty = property.FindBaseOrSiblingProperty(targetAttribute.skinField);
				if (skinProperty != null) currentSkin = skeletonComponent.Skeleton.Data.FindSkin(skinProperty.stringValue);

				currentSkin = currentSkin ?? skeletonComponent.Skeleton.Skin;
				if (currentSkin != null)
					validSkins.Add(currentSkin);
				else
					validSkins.Add(data.Skins.Items[0]);

			} else {
				foreach (Skin skin in data.Skins)
					if (skin != null) validSkins.Add(skin);
			}

			List<string> attachmentNames = new List<string>();
			List<string> placeholderNames = new List<string>();
			string prefix = "";

			if (skeletonComponent != null && targetAttribute.currentSkinOnly)
				menu.AddDisabledItem(new GUIContent((skeletonComponent as Component).gameObject.name + " (Skeleton)"));
			else
				menu.AddDisabledItem(new GUIContent(skeletonDataAsset.name));

			menu.AddSeparator("");
			if (TargetAttribute.includeNone) {
				const string NullAttachmentName = "";
				menu.AddItem(new GUIContent("Null"), !property.hasMultipleDifferentValues && property.stringValue == NullAttachmentName, HandleSelect, new SpineDrawerValuePair(NullAttachmentName, property));
				menu.AddSeparator("");
			}

			Skin defaultSkin = data.Skins.Items[0];
			SerializedProperty slotProperty = property.FindBaseOrSiblingProperty(TargetAttribute.slotField);

			string slotMatch = "";
			if (slotProperty != null) {
				if (slotProperty.propertyType == SerializedPropertyType.String)
					slotMatch = slotProperty.stringValue.ToLower();
			}

			foreach (Skin skin in validSkins) {
				string skinPrefix = skin.Name + "/";

				if (validSkins.Count > 1)
					prefix = skinPrefix;

				for (int i = 0; i < data.Slots.Count; i++) {
					if (slotMatch.Length > 0 && !(data.Slots.Items[i].Name.Equals(slotMatch, StringComparison.OrdinalIgnoreCase)))
						continue;

					attachmentNames.Clear();
					placeholderNames.Clear();

					List<Skin.SkinEntry> skinEntries = new List<Skin.SkinEntry>();
					skin.GetAttachments(i, skinEntries);
					foreach (Skin.SkinEntry entry in skinEntries) {
						attachmentNames.Add(entry.Name);
					}

					if (skin != defaultSkin) {
						foreach (Skin.SkinEntry entry in skinEntries) {
							placeholderNames.Add(entry.Name);
						}
						skinEntries.Clear();
						defaultSkin.GetAttachments(i, skinEntries);
						foreach (Skin.SkinEntry entry in skinEntries) {
							attachmentNames.Add(entry.Name);
						}
					}

					for (int a = 0; a < attachmentNames.Count; a++) {
						string attachmentPath = attachmentNames[a];
						string menuPath = prefix + data.Slots.Items[i].Name + "/" + attachmentPath;
						string name = attachmentNames[a];

						if (targetAttribute.returnAttachmentPath)
							name = skin.Name + "/" + data.Slots.Items[i].Name + "/" + attachmentPath;

						if (targetAttribute.placeholdersOnly && !placeholderNames.Contains(attachmentPath)) {
							menu.AddDisabledItem(new GUIContent(menuPath));
						} else {
							menu.AddItem(new GUIContent(menuPath), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
						}
					}

				}
			}
		}

	}

	[CustomPropertyDrawer(typeof(SpineBone))]
	public class SpineBoneDrawer : SpineTreeItemDrawerBase<SpineBone> {

		protected override Texture2D Icon { get { return SpineEditorUtilities.Icons.bone; } }

		protected override bool IsValueValid (SkeletonData skeletonData, SerializedProperty property) {
			return skeletonData.FindBone(property.stringValue) != null;
		}

		protected override void PopulateMenu (GenericMenu menu, SerializedProperty property, SpineBone targetAttribute, SkeletonData data) {
			menu.AddDisabledItem(new GUIContent(skeletonDataAsset.name));
			menu.AddSeparator("");

			if (TargetAttribute.includeNone)
				menu.AddItem(new GUIContent(NoneString), !property.hasMultipleDifferentValues && string.IsNullOrEmpty(property.stringValue), HandleSelect, new SpineDrawerValuePair(string.Empty, property));

			for (int i = 0; i < data.Bones.Count; i++) {
				BoneData bone = data.Bones.Items[i];
				string name = bone.Name;
				if (name.StartsWith(targetAttribute.startsWith, StringComparison.Ordinal)) {
					// jointName = "root/hip/bone" to show a hierarchial tree.
					string jointName = name;
					BoneData iterator = bone;
					while ((iterator = iterator.Parent) != null) {
#if MENU_REQUIRES_DIFFERENT_NESTED_NAME
						jointName = string.Format("{0} /{1}", iterator.Name, jointName);
#else
						jointName = string.Format("{0}/{1}", iterator.Name, jointName);
#endif
					}
					menu.AddItem(new GUIContent(jointName), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
				}
			}
		}

	}

	[CustomPropertyDrawer(typeof(SpineAtlasRegion))]
	public class SpineAtlasRegionDrawer : PropertyDrawer {
		SerializedProperty atlasProp;

		protected SpineAtlasRegion TargetAttribute { get { return (SpineAtlasRegion)attribute; } }

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType != SerializedPropertyType.String) {
				EditorGUI.LabelField(position, "ERROR:", "May only apply to type string");
				return;
			}

			string atlasAssetFieldName = TargetAttribute.atlasAssetField;
			if (string.IsNullOrEmpty(atlasAssetFieldName))
				atlasAssetFieldName = "atlasAsset";

			atlasProp = property.FindBaseOrSiblingProperty(atlasAssetFieldName);

			if (atlasProp == null) {
				EditorGUI.LabelField(position, "ERROR:", "Must have AtlasAsset variable!");
				return;
			} else if (atlasProp.objectReferenceValue == null) {
				EditorGUI.LabelField(position, "ERROR:", "Atlas variable must not be null!");
				return;
			} else if (!atlasProp.objectReferenceValue.GetType().IsSubclassOf(typeof(AtlasAssetBase)) &&
						atlasProp.objectReferenceValue.GetType() != typeof(AtlasAssetBase)) {
				EditorGUI.LabelField(position, "ERROR:", "Atlas variable must be of type AtlasAsset!");
			}

			position = EditorGUI.PrefixLabel(position, label);

			if (GUI.Button(position, property.stringValue, EditorStyles.popup))
				Selector(property);

		}

		void Selector (SerializedProperty property) {
			GenericMenu menu = new GenericMenu();
			AtlasAssetBase atlasAsset = (AtlasAssetBase)atlasProp.objectReferenceValue;
			Atlas atlas = atlasAsset.GetAtlas();
			FieldInfo field = typeof(Atlas).GetField("regions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			List<AtlasRegion> regions = (List<AtlasRegion>)field.GetValue(atlas);

			for (int i = 0; i < regions.Count; i++) {
				string name = regions[i].name;
				menu.AddItem(new GUIContent(name), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
			}

			menu.ShowAsContext();
		}

		static void HandleSelect (object val) {
			SpineDrawerValuePair pair = (SpineDrawerValuePair)val;
			pair.property.stringValue = pair.stringValue;
			pair.property.serializedObject.ApplyModifiedProperties();
		}

	}

}
