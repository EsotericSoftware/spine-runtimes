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

package com.esotericsoftware.spine;

import java.io.BufferedWriter;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Json;
import com.badlogic.gdx.utils.JsonValue;
import com.badlogic.gdx.utils.JsonValue.ValueType;
import com.badlogic.gdx.utils.JsonWriter.OutputType;

/** Takes Spine JSON data and transforms it to work with an older version of Spine. Target versions:<br>
 * 2.1: supports going from version 3.3.xx to 2.1.27.<br>
 * 3.7: supports going from version 3.8.xx to 3.7.94.<br>
 * 3.8: supports going from version 4.0.xx to 3.8.99 (all curves become linear, separate timelines are lost, constraint translate
 * Y and shear Y are lost).
 * <p>
 * Data can be exported from a Spine project, processed with JsonRollback, then imported into an older version of Spine. However,
 * JsonRollback may remove data for features not supported by the older Spine version. Because of this, JsonRollback is only
 * intended for situations were work was accidentally done with a newer Spine version and now needs to be imported into an older
 * Spine version (eg, if runtime support for the new version is not yet available).
 * <p>
 * Animators should freeze their Spine editor version to match the Spine version supported by the runtime being used. Only when
 * the runtime is updated to support a newer Spine version should animators update their Spine editor version to match. */
public class JsonRollback {
	static public void main (String[] args) throws Exception {
		if (args.length != 2 && args.length != 3) {
			System.out.println("Usage: <inputFile> <targetVersion> [outputFile]");
			System.exit(0);
		}

		String version = args[1];
		if (!version.equals("2.1") && !version.equals("3.7") && !version.equals("3.8")) {
			System.out.println("ERROR: Target version must be: 2.1, 3.7, or 3.8");
			System.out.println("Usage: <inputFile> <toVersion> [outputFile]");
			System.exit(0);
		}

		JsonValue root = new Json().fromJson(null, new FileHandle(args[0]));

		// Update Spine version.
		JsonValue skeleton = root.get("skeleton");
		if (skeleton == null) {
			skeleton = new JsonValue(ValueType.object);
			skeleton.name = "skeleton";
			JsonValue first = root.child;
			root.child = skeleton;
			skeleton.next = first;
		}
		JsonValue spine = skeleton.get("spine");
		if (spine != null)
			spine.set(version + "-from-" + spine.asString());
		else
			skeleton.addChild("spine", new JsonValue(version));

		if (version.equals("2.1")) {
			// In 3.2 skinnedmesh was renamed to weightedmesh.
			setValue(root, "skinnedmesh", "skins", "*", "*", "*", "type", "weightedmesh");

			// In 3.2 shear was added.
			delete(root, "animations", "*", "bones", "*", "shear");

			// In 3.3 ffd was renamed to deform.
			rename(root, "ffd", "animations", "*", "deform");

			// In 3.3 mesh is now a single type, previously they were skinnedmesh if they had weights.
			for (JsonValue value : find(root, new Array<JsonValue>(), 0, "skins", "*", "*", "*", "type", "mesh"))
				if (value.parent.get("uvs").size != value.parent.get("vertices").size) value.set("skinnedmesh");

			// In 3.3 linkedmesh is now a single type, previously they were linkedweightedmesh if they had weights.
			for (JsonValue value : find(root, new Array<JsonValue>(), 0, "skins", "*", "*", "*", "type", "linkedmesh")) {
				String slot = value.parent.parent.name.replaceAll("", "");
				String skinName = value.parent.getString("skin", "default");
				String parentName = value.parent.getString("parent");
				if (find(root, new Array<JsonValue>(), 0,
					("skins~~" + skinName + "~~" + slot + "~~" + parentName + "~~type~~skinnedmesh").split("~~")).size > 0)
					value.set("weightedlinkedmesh");
			}

			// In 3.3 bounding boxes can be weighted.
			for (JsonValue value : find(root, new Array<JsonValue>(), 0, "skins", "*", "*", "*", "type", "boundingbox"))
				if (value.parent.getInt("vertexCount") * 2 != value.parent.get("vertices").size)
					value.parent.parent.remove(value.parent.name);

			// In 3.3 paths were added.
			for (JsonValue value : find(root, new Array<JsonValue>(), 0, "skins", "*", "*", "*", "type", "path")) {
				String attachment = value.parent.name;
				value.parent.parent.remove(attachment);
				String slot = value.parent.parent.name;
				// Also remove path deform timelines.
				delete(root, "animations", "*", "ffd", "*", slot, attachment);
			}

			// In 3.3 IK constraint timelines no longer require bendPositive.
			for (JsonValue value : find(root, new Array<JsonValue>(), 0, "animations", "*", "ik", "*"))
				for (JsonValue child = value.child; child != null; child = child.next)
					if (!child.has("bendPositive")) child.addChild("bendPositive", new JsonValue(true));

			// In 3.3 transform constraints can have more than 1 bone.
			for (JsonValue child = root.getChild("transform"); child != null; child = child.next) {
				JsonValue bones = child.remove("bones");
				if (bones != null) child.addChild("bone", new JsonValue(bones.child.asString()));
			}
		} else if (version.equals("3.7")) {
			JsonValue skins = root.get("skins");
			if (skins != null && skins.isArray()) {
				JsonValue newSkins = new JsonValue(ValueType.object);
				for (JsonValue skinMap = skins.child; skinMap != null; skinMap = skinMap.next) {
					JsonValue attachments = skinMap.get("attachments");
					if (attachments != null) newSkins.addChild(skinMap.getString("name"), skinMap.get("attachments"));
				}
				root.remove("skins");
				root.addChild("skins", newSkins);
			}

			rollbackCurves(root.get("animations"));
		} else if (version.equals("3.8")) {
			linearCurves(root.get("animations"));
			rename(root, "angle", "animations", "*", "bones", "*", "rotate", "value");
			constraintNames(root, "transform");
			constraintNames(root, "path");
			constraintNames(root, "animations", "*", "transform", "*");
			constraintNames(root, "animations", "*", "path", "*");
		}

		if (args.length == 3) {
			System.out.println("Writing: " + args[2]);
			BufferedWriter fileWriter = new BufferedWriter(new FileHandle(args[2]).writer(false, "UTF-8"), 16 * 1024);
			root.prettyPrint(OutputType.json, fileWriter);
			fileWriter.close();
		} else
			System.out.println(root.prettyPrint(OutputType.json, 130));
	}

	static private void log (String message) {
		System.out.println(message);
	}

	static private void constraintNames (JsonValue root, String... path) {
		for (JsonValue map : find(root, new Array<JsonValue>(), 0, path)) {
			for (JsonValue constraint = map.child; constraint != null; constraint = constraint.next) {
				for (JsonValue child = constraint.child; child != null; child = child.next) {
					if (child.name.equals("mixRotate"))
						child.name = "rotateMix";
					else if (child.name.equals("mixX") || child.name.equals("mixY"))
						child.name = "translateMix";
					else if (child.name.equals("mixScaleX") || child.name.equals("mixScaleY")) {
						child.name = "scaleMix";
					} else if (child.name.equals("mixShearX") || child.name.equals("mixShearY")) //
						child.name = "shearMix";
				}
			}
		}
	}

	static private void linearCurves (JsonValue map) {
		if (map == null) return;

		if (map.isObject() && map.parent.isArray()) { // Probably a key.
			if (map.parent.name != null) {
				String name = map.parent.name;
				if (name.equals("translatex") || name.equals("translatey") //
					|| name.equals("scalex") || name.equals("scaley") //
					|| name.equals("shearx") || name.equals("sheary") //
					|| name.equals("rgb") || name.equals("rgb2") || name.equals("alpha")) {
					map.parent.remove();
					log("Separate timelines removed: " + name);
				}
				if (name.equals("rgba"))
					map.parent.name = "color";
				else if (name.equals("rgba2")) //
					map.parent.name = "twoColor";
			}
		}

		JsonValue curve = map.get("curve");
		if (curve == null) {
			for (JsonValue child = map.child; child != null; child = child.next)
				linearCurves(child);
			return;
		}
		if (!curve.isString()) {
			curve.remove();
			log("Bezier curve changed to linear.");
		}
	}

	static private void rollbackCurves (JsonValue map) {
		if (map == null) return;

		if (map.isObject() && map.parent.isArray()) { // Probably a key.
			if (!map.has("time")) map.addChild("time", new JsonValue(0f));
			if (map.parent.name != null) {
				if (map.parent.name.equals("rotate") && !map.has("angle"))
					map.addChild("angle", new JsonValue(0f));
				else if (map.parent.name.equals("scale")) {
					if (!map.has("x")) map.addChild("x", new JsonValue(1f));
					if (!map.has("y")) map.addChild("y", new JsonValue(1f));
				}
			}
		}

		JsonValue curve = map.get("curve");
		if (curve == null) {
			for (JsonValue child = map.child; child != null; child = child.next)
				rollbackCurves(child);
			return;
		}
		if (curve.isNumber()) {
			curve.addChild(new JsonValue(curve.asFloat()));
			curve.setType(ValueType.array);
			curve.addChild(new JsonValue(map.getFloat("c2", 0)));
			curve.addChild(new JsonValue(map.getFloat("c3", 1)));
			curve.addChild(new JsonValue(map.getFloat("c4", 1)));
			map.remove("c2");
			map.remove("c3");
			map.remove("c4");
		}
	}

	static void setValue (JsonValue root, String newValue, String... path) {
		for (JsonValue value : find(root, new Array<JsonValue>(), 0, path))
			value.set(newValue);
	}

	static void rename (JsonValue root, String newName, String... path) {
		for (JsonValue value : find(root, new Array<JsonValue>(), 0, path))
			value.name = newName;
	}

	static void delete (JsonValue root, String... path) {
		for (JsonValue value : find(root, new Array<JsonValue>(), 0, path))
			value.parent.remove(value.name);
	}

	static Array<JsonValue> find (JsonValue current, Array<JsonValue> values, int index, String... path) {
		String name = path[index];
		if (current.name == null) {
			if (name.equals("*") && index == path.length - 1)
				values.add(current);
			else if (current.has(name)) return find(current.get(name), values, index, path);
		} else if (name.equals("*") || current.name.equals(name)) {
			if (++index == path.length || (index == path.length - 1 && current.isString() && current.asString().equals(path[index])))
				values.add(current);
			else {
				for (JsonValue child = current.child; child != null; child = child.next)
					find(child, values, index, path);
			}
		}
		return values;
	}
}
