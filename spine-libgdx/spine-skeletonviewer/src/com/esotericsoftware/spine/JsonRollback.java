
package com.esotericsoftware.spine;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Json;
import com.badlogic.gdx.utils.JsonValue;
import com.badlogic.gdx.utils.JsonWriter.OutputType;

/** Takes Spine JSON data and transforms it to work with an older version of Spine. It supports going from version 3.3.xx to
 * 2.1.27.
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
		if (args.length == 0) {
			System.out.println("Usage: <inputFile> [outputFile]");
			System.exit(0);
		}

		JsonValue root = new Json().fromJson(null, new FileHandle(args[0]));

		// In 3.2 skinnedmesh was renamed to weightedmesh.
		setValue(root, "skinnedmesh", "skins", "*", "*", "*", "type", "weightedmesh");

		// In 3.2 shear was added.
		delete(root, "animations", "*", "bones", "*", "shear");

		// In 3.3 ffd was renamed to deform.
		rename(root, "ffd", "animations", "*", "deform");

		// In 3.3 mesh is now a single type, previously they were skinnedmesh if they had weights.
		for (JsonValue value : find(root, new Array(), 0, "skins", "*", "*", "*", "type", "mesh"))
			if (value.parent.get("uvs").size != value.parent.get("vertices").size) value.set("skinnedmesh");

		// In 3.3 linkedmesh is now a single type, previously they were linkedweightedmesh if they had weights.
		for (JsonValue value : find(root, new Array(), 0, "skins", "*", "*", "*", "type", "linkedmesh")) {
			String slot = value.parent.parent.name.replaceAll("", "");
			String skinName = value.parent.getString("skin", "default");
			String parentName = value.parent.getString("parent");
			if (find(root, new Array(), 0,
				("skins~~" + skinName + "~~" + slot + "~~" + parentName + "~~type~~skinnedmesh").split("~~")).size > 0)
				value.set("weightedlinkedmesh");
		}

		// In 3.3 bounding boxes can be weighted.
		for (JsonValue value : find(root, new Array(), 0, "skins", "*", "*", "*", "type", "boundingbox"))
			if (value.parent.getInt("vertexCount") * 2 != value.parent.get("vertices").size)
				value.parent.parent.remove(value.parent.name);

		// In 3.3 paths were added.
		for (JsonValue value : find(root, new Array(), 0, "skins", "*", "*", "*", "type", "path")) {
			String attachment = value.parent.name;
			value.parent.parent.remove(attachment);
			String slot = value.parent.parent.name;
			// Also remove path deform timelines.
			delete(root, "animations", "*", "ffd", "*", slot, attachment);
		}

		// In 3.3 IK constraint timelines no longer require bendPositive.
		for (JsonValue value : find(root, new Array(), 0, "animations", "*", "ik", "*"))
			for (JsonValue child = value.child; child != null; child = child.next)
				if (!child.has("bendPositive")) child.addChild("bendPositive", new JsonValue(true));

		// In 3.3 transform constraints can have more than 1 bone.
		for (JsonValue child = root.getChild("transform"); child != null; child = child.next) {
			JsonValue bones = child.remove("bones");
			if (bones != null) child.addChild("bone", new JsonValue(bones.child.asString()));
		}

		if (args.length > 1)
			new FileHandle(args[1]).writeString(root.prettyPrint(OutputType.json, 130), false, "UTF-8");
		else
			System.out.println(root.prettyPrint(OutputType.json, 130));
	}

	static void setValue (JsonValue root, String newValue, String... path) {
		for (JsonValue value : find(root, new Array(), 0, path))
			value.set(newValue);
	}

	static void rename (JsonValue root, String newName, String... path) {
		for (JsonValue value : find(root, new Array(), 0, path))
			value.name = newName;
	}

	static void delete (JsonValue root, String... path) {
		for (JsonValue value : find(root, new Array(), 0, path))
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
