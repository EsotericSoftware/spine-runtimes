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

#pragma warning disable 0219

#if UNITY_2022_1_OR_NEWER
#define FREE_MOVE_HANDLE_HAS_NO_ROTATION_PARAM
#endif

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
	using EventType = UnityEngine.EventType;

	public static class SpineHandles {
		public static Color BoneColor { get { return new Color(0.8f, 0.8f, 0.8f, 0.4f); } }
		public static Color PathColor { get { return new Color(254 / 255f, 127 / 255f, 0); } }
		public static Color TransformContraintColor { get { return new Color(170 / 255f, 226 / 255f, 35 / 255f); } }
		public static Color IkColor { get { return new Color(228 / 255f, 90 / 255f, 43 / 255f); } }
		public static Color PointColor { get { return new Color(1f, 1f, 0f, 1f); } }

		static Vector3[] _boneMeshVerts = {
			new Vector3(0, 0, 0),
			new Vector3(0.1f, 0.1f, 0),
			new Vector3(1, 0, 0),
			new Vector3(0.1f, -0.1f, 0)
		};
		static Mesh _boneMesh;
		public static Mesh BoneMesh {
			get {
				if (_boneMesh == null) {
					_boneMesh = new Mesh {
						vertices = _boneMeshVerts,
						uv = new Vector2[4],
						triangles = new[] { 0, 1, 2, 2, 3, 0 }
					};
					_boneMesh.RecalculateBounds();
					_boneMesh.RecalculateNormals();
				}
				return _boneMesh;
			}
		}

		static Mesh _arrowheadMesh;
		public static Mesh ArrowheadMesh {
			get {
				if (_arrowheadMesh == null) {
					_arrowheadMesh = new Mesh {
						vertices = new[] {
							new Vector3(0, 0),
							new Vector3(-0.1f, 0.05f),
							new Vector3(-0.1f, -0.05f)
						},
						uv = new Vector2[3],
						triangles = new[] { 0, 1, 2 }
					};
					_arrowheadMesh.RecalculateBounds();
					_arrowheadMesh.RecalculateNormals();
				}
				return _arrowheadMesh;
			}
		}

		static Material _boneMaterial;
		static Material BoneMaterial {
			get {
				if (_boneMaterial == null) {
					_boneMaterial = new Material(Shader.Find("Hidden/Spine/Bones"));
					_boneMaterial.SetColor("_Color", SpineHandles.BoneColor);
				}

				return _boneMaterial;
			}
		}
		public static Material GetBoneMaterial () {
			BoneMaterial.SetColor("_Color", SpineHandles.BoneColor);
			return BoneMaterial;
		}

		public static Material GetBoneMaterial (Color color) {
			BoneMaterial.SetColor("_Color", color);
			return BoneMaterial;
		}

		static Material _ikMaterial;
		public static Material IKMaterial {
			get {
				if (_ikMaterial == null) {
					_ikMaterial = new Material(Shader.Find("Hidden/Spine/Bones"));
					_ikMaterial.SetColor("_Color", SpineHandles.IkColor);
				}
				return _ikMaterial;
			}
		}

		static GUIStyle _boneNameStyle;
		public static GUIStyle BoneNameStyle {
			get {
				if (_boneNameStyle == null) {
					_boneNameStyle = new GUIStyle(EditorStyles.whiteMiniLabel) {
						alignment = TextAnchor.MiddleCenter,
						stretchWidth = true,
						padding = new RectOffset(0, 0, 0, 0),
						contentOffset = new Vector2(-5f, 0f)
					};
				}
				return _boneNameStyle;
			}
		}

		static GUIStyle _pathNameStyle;
		public static GUIStyle PathNameStyle {
			get {
				if (_pathNameStyle == null) {
					_pathNameStyle = new GUIStyle(SpineHandles.BoneNameStyle);
					_pathNameStyle.normal.textColor = SpineHandles.PathColor;
				}
				return _pathNameStyle;
			}
		}

		static GUIStyle _pointNameStyle;
		public static GUIStyle PointNameStyle {
			get {
				if (_pointNameStyle == null) {
					_pointNameStyle = new GUIStyle(SpineHandles.BoneNameStyle);
					_pointNameStyle.normal.textColor = SpineHandles.PointColor;
				}
				return _pointNameStyle;
			}
		}

		public static void DrawBoneNames (Transform transform, Skeleton skeleton, float positionScale = 1f,
			Vector2? positionOffset = null) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Vector2 offset = positionOffset == null ? Vector2.zero : positionOffset.Value;
			GUIStyle style = BoneNameStyle;
			foreach (Bone b in skeleton.Bones) {
				if (!b.Active) continue;
				Vector3 pos = new Vector3(b.WorldX * positionScale + offset.x, b.WorldY * positionScale + offset.y, 0)
					+ (new Vector3(b.A, b.C) * (b.Data.Length * 0.5f));
				pos = transform.TransformPoint(pos);
				Handles.Label(pos, b.Data.Name, style);
			}
		}

		public static void DrawBones (Transform transform, Skeleton skeleton, float positionScale = 1f,
			Vector2? positionOffset = null) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Vector2 offset = positionOffset == null ? Vector2.zero : positionOffset.Value;
			float boneScale = 1.8f; // Draw the root bone largest;
			DrawCrosshairs2D(skeleton.Bones.Items[0].GetWorldPosition(transform, positionScale, offset), 0.08f, positionScale);

			foreach (Bone b in skeleton.Bones) {
				if (!b.Active) continue;
				DrawBone(transform, b, boneScale, positionScale, positionOffset);
				boneScale = 1f;
			}
		}

		static Vector3[] _boneWireBuffer = new Vector3[5];
		static Vector3[] GetBoneWireBuffer (Matrix4x4 m) {
			for (int i = 0, n = _boneMeshVerts.Length; i < n; i++)
				_boneWireBuffer[i] = m.MultiplyPoint(_boneMeshVerts[i]);

			_boneWireBuffer[4] = _boneWireBuffer[0]; // closed polygon.
			return _boneWireBuffer;
		}
		public static void DrawBoneWireframe (Transform transform, Bone b, Color color, float skeletonRenderScale = 1f,
			Vector2? positionOffset = null) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Vector2 offset = positionOffset == null ? Vector2.zero : positionOffset.Value;
			Handles.color = color;
			Vector3 pos = new Vector3(b.WorldX * skeletonRenderScale + offset.x, b.WorldY * skeletonRenderScale + offset.y, 0);
			float length = b.Data.Length;

			if (length > 0) {
				Quaternion rot = Quaternion.Euler(0, 0, b.WorldRotationX);
				Vector3 scale = Vector3.one * length * b.WorldScaleX * skeletonRenderScale;
				const float my = 1.5f;
				scale.y *= (SpineEditorUtilities.Preferences.handleScale + 1) * 0.5f;
				scale.y = Mathf.Clamp(scale.x, -my * skeletonRenderScale, my * skeletonRenderScale);
				Handles.DrawPolyLine(GetBoneWireBuffer(transform.localToWorldMatrix * Matrix4x4.TRS(pos, rot, scale)));
				Vector3 wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, color, transform.forward, skeletonRenderScale);
			} else {
				Vector3 wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, color, transform.forward, skeletonRenderScale);
			}
		}

		public static void DrawBone (Transform transform, Bone b, float boneScale, float skeletonRenderScale = 1f,
			Vector2? positionOffset = null) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Vector2 offset = positionOffset == null ? Vector2.zero : positionOffset.Value;
			Vector3 pos = new Vector3(b.WorldX * skeletonRenderScale + offset.x, b.WorldY * skeletonRenderScale + offset.y, 0);
			float length = b.Data.Length;
			if (length > 0) {
				Quaternion rot = Quaternion.Euler(0, 0, b.WorldRotationX);
				Vector3 scale = Vector3.one * length * b.WorldScaleX * skeletonRenderScale;
				const float my = 1.5f;
				scale.y *= (SpineEditorUtilities.Preferences.handleScale + 1f) * 0.5f;
				scale.y = Mathf.Clamp(scale.x, -my * skeletonRenderScale, my * skeletonRenderScale);
				SpineHandles.GetBoneMaterial().SetPass(0);
				Graphics.DrawMeshNow(SpineHandles.BoneMesh, transform.localToWorldMatrix * Matrix4x4.TRS(pos, rot, scale));
			} else {
				Vector3 wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, SpineHandles.BoneColor, transform.forward, boneScale * skeletonRenderScale);
			}
		}

		public static void DrawBone (Transform transform, Bone b, float boneScale, Color color, float skeletonRenderScale = 1f,
			Vector2? positionOffset = null) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Vector2 offset = positionOffset == null ? Vector2.zero : positionOffset.Value;
			Vector3 pos = new Vector3(b.WorldX * skeletonRenderScale + offset.x, b.WorldY * skeletonRenderScale + offset.y, 0);
			float length = b.Data.Length;
			if (length > 0) {
				Quaternion rot = Quaternion.Euler(0, 0, b.WorldRotationX);
				Vector3 scale = Vector3.one * length * b.WorldScaleX;
				const float my = 1.5f;
				scale.y *= (SpineEditorUtilities.Preferences.handleScale + 1f) * 0.5f;
				scale.y = Mathf.Clamp(scale.x, -my, my);
				SpineHandles.GetBoneMaterial(color).SetPass(0);
				Graphics.DrawMeshNow(SpineHandles.BoneMesh, transform.localToWorldMatrix * Matrix4x4.TRS(pos, rot, scale));
			} else {
				Vector3 wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, color, transform.forward, boneScale * skeletonRenderScale);
			}
		}

		public static void DrawPaths (Transform transform, Skeleton skeleton) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			foreach (Slot s in skeleton.DrawOrder) {
				PathAttachment p = s.Attachment as PathAttachment;
				if (p != null) SpineHandles.DrawPath(s, p, transform, true);
			}
		}

		static float[] pathVertexBuffer;
		public static void DrawPath (Slot s, PathAttachment p, Transform t, bool includeName) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			int worldVerticesLength = p.WorldVerticesLength;

			if (pathVertexBuffer == null || pathVertexBuffer.Length < worldVerticesLength)
				pathVertexBuffer = new float[worldVerticesLength];

			float[] pv = pathVertexBuffer;
			p.ComputeWorldVertices(s, pv);

			Color ocolor = Handles.color;
			Handles.color = SpineHandles.PathColor;

			Matrix4x4 m = t.localToWorldMatrix;
			const int step = 6;
			int n = worldVerticesLength - step;
			Vector3 p0, p1, p2, p3;
			for (int i = 2; i < n; i += step) {
				p0 = m.MultiplyPoint(new Vector3(pv[i], pv[i + 1]));
				p1 = m.MultiplyPoint(new Vector3(pv[i + 2], pv[i + 3]));
				p2 = m.MultiplyPoint(new Vector3(pv[i + 4], pv[i + 5]));
				p3 = m.MultiplyPoint(new Vector3(pv[i + 6], pv[i + 7]));
				DrawCubicBezier(p0, p1, p2, p3);
			}

			n += step;
			if (p.Closed) {
				p0 = m.MultiplyPoint(new Vector3(pv[n - 4], pv[n - 3]));
				p1 = m.MultiplyPoint(new Vector3(pv[n - 2], pv[n - 1]));
				p2 = m.MultiplyPoint(new Vector3(pv[0], pv[1]));
				p3 = m.MultiplyPoint(new Vector3(pv[2], pv[3]));
				DrawCubicBezier(p0, p1, p2, p3);
			}

			const float endCapSize = 0.05f;
			Vector3 firstPoint = m.MultiplyPoint(new Vector3(pv[2], pv[3]));
			SpineHandles.DrawDot(firstPoint, endCapSize);

			//if (!p.Closed) SpineHandles.DrawDot(m.MultiplyPoint(new Vector3(pv[n - 4], pv[n - 3])), endCapSize);
			if (includeName) Handles.Label(firstPoint + new Vector3(0, 0.1f), p.Name, PathNameStyle);

			Handles.color = ocolor;
		}

		public static void DrawDot (Vector3 position, float size) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Handles.DotHandleCap(0, position, Quaternion.identity, size * HandleUtility.GetHandleSize(position), EventType.Ignore); //Handles.DotCap(0, position, Quaternion.identity, size * HandleUtility.GetHandleSize(position));
		}

		public static void DrawBoundingBoxes (Transform transform, Skeleton skeleton) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			foreach (Slot slot in skeleton.Slots) {
				BoundingBoxAttachment bba = slot.Attachment as BoundingBoxAttachment;
				if (bba != null) SpineHandles.DrawBoundingBox(slot, bba, transform);
			}
		}

		public static void DrawBoundingBox (Slot slot, BoundingBoxAttachment box, Transform t) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			if (box.Vertices.Length <= 2) return; // Handle cases where user creates a BoundingBoxAttachment but doesn't actually define it.

			float[] worldVerts = new float[box.WorldVerticesLength];
			box.ComputeWorldVertices(slot, worldVerts);

			Handles.color = Color.green;
			Vector3 lastVert = Vector3.zero;
			Vector3 vert = Vector3.zero;
			Vector3 firstVert = t.TransformPoint(new Vector3(worldVerts[0], worldVerts[1], 0));
			for (int i = 0; i < worldVerts.Length; i += 2) {
				vert.x = worldVerts[i];
				vert.y = worldVerts[i + 1];
				vert.z = 0;

				vert = t.TransformPoint(vert);

				if (i > 0)
					Handles.DrawLine(lastVert, vert);

				lastVert = vert;
			}

			Handles.DrawLine(lastVert, firstVert);
		}

		public static void DrawPointAttachment (Bone bone, PointAttachment pointAttachment, Transform skeletonTransform) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			if (bone == null) return;
			if (pointAttachment == null) return;

			Vector2 localPos;
			pointAttachment.ComputeWorldPosition(bone, out localPos.x, out localPos.y);
			float localRotation = pointAttachment.ComputeWorldRotation(bone);
			Matrix4x4 m = Matrix4x4.TRS(localPos, Quaternion.Euler(0, 0, localRotation), Vector3.one) * Matrix4x4.TRS(Vector3.right * 0.25f, Quaternion.identity, Vector3.one);

			DrawBoneCircle(skeletonTransform.TransformPoint(localPos), SpineHandles.PointColor, Vector3.back, 1.3f);
			DrawArrowhead(skeletonTransform.localToWorldMatrix * m);
		}

		public static void DrawConstraints (Transform transform, Skeleton skeleton, float skeletonRenderScale = 1f,
			Vector2? positionOffset = null) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Vector2 offset = positionOffset == null ? Vector2.zero : positionOffset.Value;
			Vector3 targetPos;
			Vector3 pos;
			bool active;
			Color handleColor;
			const float Thickness = 4f;
			Vector3 normal = transform.forward;

			// Transform Constraints
			handleColor = SpineHandles.TransformContraintColor;
			foreach (TransformConstraint tc in skeleton.TransformConstraints) {
				Bone targetBone = tc.Target;
				targetPos = targetBone.GetWorldPosition(transform, skeletonRenderScale, offset);

				if (tc.MixX > 0 || tc.MixY > 0) {
					if ((tc.MixX > 0 && tc.MixX != 1f) ||
						(tc.MixY > 0 && tc.MixY != 1f)) {
						Handles.color = handleColor;
						foreach (Bone b in tc.Bones) {
							pos = b.GetWorldPosition(transform, skeletonRenderScale, offset);
							Handles.DrawDottedLine(targetPos, pos, Thickness);
						}
					}
					SpineHandles.DrawBoneCircle(targetPos, handleColor, normal, 1.3f * skeletonRenderScale);
					Handles.color = handleColor;
					SpineHandles.DrawCrosshairs(targetPos, 0.2f, targetBone.A, targetBone.B, targetBone.C, targetBone.D, transform, skeletonRenderScale);
				}
			}

			// IK Constraints
			handleColor = SpineHandles.IkColor;
			foreach (IkConstraint ikc in skeleton.IkConstraints) {
				Bone targetBone = ikc.Target;
				targetPos = targetBone.GetWorldPosition(transform, skeletonRenderScale, offset);
				ExposedList<Bone> bones = ikc.Bones;
				active = ikc.Mix > 0;
				if (active) {
					pos = bones.Items[0].GetWorldPosition(transform, skeletonRenderScale, offset);
					switch (bones.Count) {
					case 1: {
						Handles.color = handleColor;
						Handles.DrawLine(targetPos, pos);
						SpineHandles.DrawBoneCircle(targetPos, handleColor, normal);
						Matrix4x4 m = bones.Items[0].GetMatrix4x4();
						m.m03 = targetBone.WorldX * skeletonRenderScale + offset.x;
						m.m13 = targetBone.WorldY * skeletonRenderScale + offset.y;
						SpineHandles.DrawArrowhead(transform.localToWorldMatrix * m);
						break;
					}
					case 2: {
						Bone childBone = bones.Items[1];
						Vector3 child = childBone.GetWorldPosition(transform, skeletonRenderScale, offset);
						Handles.color = handleColor;
						Handles.DrawLine(child, pos);
						Handles.DrawLine(targetPos, child);
						SpineHandles.DrawBoneCircle(pos, handleColor, normal, 0.5f);
						SpineHandles.DrawBoneCircle(child, handleColor, normal, 0.5f);
						SpineHandles.DrawBoneCircle(targetPos, handleColor, normal);
						Matrix4x4 m = childBone.GetMatrix4x4();
						m.m03 = targetBone.WorldX * skeletonRenderScale + offset.x;
						m.m13 = targetBone.WorldY * skeletonRenderScale + offset.y;
						SpineHandles.DrawArrowhead(transform.localToWorldMatrix * m);
						break;
					}
					}
				}
				//Handles.Label(targetPos, ikc.Data.Name, SpineHandles.BoneNameStyle);
			}

			// Path Constraints
			handleColor = SpineHandles.PathColor;
			foreach (PathConstraint pc in skeleton.PathConstraints) {
				active = pc.MixX > 0 || pc.MixY > 0 || pc.MixRotate > 0;
				if (active)
					foreach (Bone b in pc.Bones)
						SpineHandles.DrawBoneCircle(b.GetWorldPosition(transform, skeletonRenderScale, offset),
							handleColor, normal, 1f * skeletonRenderScale);
			}
		}

		public static void DrawReferenceRect (SkeletonGraphic skeletonGraphic, Color color) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			RectTransform rectTransform = skeletonGraphic.rectTransform;
			Vector2 referenceRectSize = skeletonGraphic.GetReferenceRectSize();

			Vector3 position = rectTransform.position;
			Vector3 right = rectTransform.TransformVector(Vector3.right * referenceRectSize.x);
			Vector3 up = rectTransform.TransformVector(Vector3.up * referenceRectSize.y);

			Vector3 cornerVertexBL = position - rectTransform.pivot.x * right - rectTransform.pivot.y * up;
			DrawRect(cornerVertexBL, right, up, color);
		}

		public static void DrawRectTransformRect (SkeletonGraphic skeletonGraphic, Color color) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			RectTransform rectTransform = skeletonGraphic.rectTransform;
			Vector2 rectTransformSize = skeletonGraphic.RectTransformSize;

			Vector3 position = rectTransform.position;
			Vector3 right = rectTransform.TransformVector(Vector3.right * rectTransformSize.x);
			Vector3 up = rectTransform.TransformVector(Vector3.up * rectTransformSize.y);

			Vector3 cornerVertexBL = position - rectTransform.pivot.x * right - rectTransform.pivot.y * up;
			DrawRect(cornerVertexBL, right, up, color);
		}

		public static void DrawRect (Vector3 cornerVertexBL, Vector3 right, Vector3 up, Color color) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Vector3 v0 = cornerVertexBL;
			Vector3 v1 = v0 + right;
			Vector3 v2 = v0 + right + up;
			Vector3 v3 = v0 + up;
			Color previousColor = UnityEditor.Handles.color;
			UnityEditor.Handles.color = color;
			UnityEditor.Handles.DrawLine(v0, v1);
			UnityEditor.Handles.DrawLine(v1, v2);
			UnityEditor.Handles.DrawLine(v2, v3);
			UnityEditor.Handles.DrawLine(v3, v0);
			UnityEditor.Handles.color = previousColor;
		}

		public static void DrawPivotOffsetHandle (SkeletonGraphic skeletonGraphic, Color color) {
			// Note: not limiting to current.type == EventType.Repaint because the FreeMoveHandle requires interaction.

			float handleSize = HandleUtility.GetHandleSize(skeletonGraphic.transform.position);
			float controlSize = handleSize * 0.3f;
			float discSize = handleSize * 0.03f;
			Vector3 snap = Vector3.zero;
			Color savedColor = Handles.color;

			Handles.color = color;
			Vector2 scaledOffset = skeletonGraphic.GetScaledPivotOffset();
			Vector3 worldSpaceOffset = skeletonGraphic.transform.TransformPoint(scaledOffset);
			EditorGUI.BeginChangeCheck();

#if FREE_MOVE_HANDLE_HAS_NO_ROTATION_PARAM
			Vector3 newWorldSpacePosition = Handles.FreeMoveHandle(worldSpaceOffset, controlSize, snap, Handles.CircleHandleCap);
#else
			Vector3 newWorldSpacePosition = Handles.FreeMoveHandle(worldSpaceOffset, Quaternion.identity, controlSize, snap, Handles.CircleHandleCap);
#endif
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(skeletonGraphic, "Change Offset to Pivot");
				Vector3 localScaledOffset = skeletonGraphic.transform.InverseTransformPoint(newWorldSpacePosition);
				skeletonGraphic.SetScaledPivotOffset(localScaledOffset);
				skeletonGraphic.UpdateMeshToInstructions();
			}
			Handles.DrawSolidDisc(newWorldSpacePosition, skeletonGraphic.transform.forward, discSize);
			Handles.color = savedColor;
		}

		static void DrawCrosshairs2D (Vector3 position, float scale, float skeletonRenderScale = 1f) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			scale *= SpineEditorUtilities.Preferences.handleScale * skeletonRenderScale;
			Handles.DrawLine(position + new Vector3(-scale, 0), position + new Vector3(scale, 0));
			Handles.DrawLine(position + new Vector3(0, -scale), position + new Vector3(0, scale));
		}

		static void DrawCrosshairs (Vector3 position, float scale, float a, float b, float c, float d, Transform transform, float skeletonRenderScale = 1f) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			scale *= SpineEditorUtilities.Preferences.handleScale * skeletonRenderScale;

			Vector3 xOffset = (Vector3)(new Vector2(a, c).normalized * scale);
			Vector3 yOffset = (Vector3)(new Vector2(b, d).normalized * scale);
			xOffset = transform.TransformDirection(xOffset);
			yOffset = transform.TransformDirection(yOffset);

			Handles.DrawLine(position + xOffset, position - xOffset);
			Handles.DrawLine(position + yOffset, position - yOffset);
		}

		static void DrawArrowhead2D (Vector3 pos, float localRotation, float scale = 1f) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			scale *= SpineEditorUtilities.Preferences.handleScale;

			SpineHandles.IKMaterial.SetPass(0);
			Graphics.DrawMeshNow(SpineHandles.ArrowheadMesh, Matrix4x4.TRS(pos, Quaternion.Euler(0, 0, localRotation), new Vector3(scale, scale, scale)));
		}

		static void DrawArrowhead (Vector3 pos, Quaternion worldQuaternion) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Graphics.DrawMeshNow(SpineHandles.ArrowheadMesh, pos, worldQuaternion, 0);
		}

		static void DrawArrowhead (Matrix4x4 m) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			float s = SpineEditorUtilities.Preferences.handleScale;
			m.m00 *= s;
			m.m01 *= s;
			m.m02 *= s;
			m.m10 *= s;
			m.m11 *= s;
			m.m12 *= s;
			m.m20 *= s;
			m.m21 *= s;
			m.m22 *= s;

			SpineHandles.IKMaterial.SetPass(0);
			Graphics.DrawMeshNow(SpineHandles.ArrowheadMesh, m);
		}

		static void DrawBoneCircle (Vector3 pos, Color outlineColor, Vector3 normal, float scale = 1f) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			scale *= SpineEditorUtilities.Preferences.handleScale;

			Color o = Handles.color;
			Handles.color = outlineColor;
			float firstScale = 0.08f * scale;
			Handles.DrawSolidDisc(pos, normal, firstScale);
			const float Thickness = 0.03f;
			float secondScale = firstScale - (Thickness * SpineEditorUtilities.Preferences.handleScale * scale);

			if (secondScale > 0f) {
				Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
				Handles.DrawSolidDisc(pos, normal, secondScale);
			}

			Handles.color = o;
		}

		internal static void DrawCubicBezier (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;

			Handles.DrawBezier(p0, p3, p1, p2, Handles.color, Texture2D.whiteTexture, 2f);
			//			const float dotSize = 0.01f;
			//			Quaternion q = Quaternion.identity;
			//			Handles.DotCap(0, p0, q, dotSize);
			//			Handles.DotCap(0, p1, q, dotSize);
			//			Handles.DotCap(0, p2, q, dotSize);
			//			Handles.DotCap(0, p3, q, dotSize);
			//			Handles.DrawLine(p0, p1);
			//			Handles.DrawLine(p3, p2);
		}
	}
}
