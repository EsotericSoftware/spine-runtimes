
package com.esotericsoftware.spine.utils;

import java.util.Locale;

public class JsonWriter {
	private final StringBuilder buffer = new StringBuilder();
	private int depth = 0;
	private boolean needsComma = false;

	public void writeObjectStart () {
		writeCommaIfNeeded();
		buffer.append("{");
		depth++;
		needsComma = false;
	}

	public void writeObjectEnd () {
		depth--;
		if (needsComma) {
			buffer.append("\n");
			writeIndent();
		}
		buffer.append("}");
		needsComma = true;
	}

	public void writeArrayStart () {
		writeCommaIfNeeded();
		buffer.append("[");
		depth++;
		needsComma = false;
	}

	public void writeArrayEnd () {
		depth--;
		if (needsComma) {
			buffer.append("\n");
			writeIndent();
		}
		buffer.append("]");
		needsComma = true;
	}

	public void writeName (String name) {
		writeCommaIfNeeded();
		buffer.append("\n");
		writeIndent();
		buffer.append("\"").append(name).append("\": ");
		needsComma = false;
	}

	public void writeValue (String value) {
		writeCommaIfNeeded();
		if (value == null) {
			buffer.append("null");
		} else {
			buffer.append("\"").append(escapeString(value)).append("\"");
		}
		needsComma = true;
	}

	public void writeValue (float value) {
		writeCommaIfNeeded();
		buffer.append(String.format(Locale.US, "%.6f", value).replaceAll("0+$", "").replaceAll("\\.$", ""));
		needsComma = true;
	}

	public void writeValue (int value) {
		writeCommaIfNeeded();
		buffer.append(String.valueOf(value));
		needsComma = true;
	}

	public void writeValue (boolean value) {
		writeCommaIfNeeded();
		buffer.append(String.valueOf(value));
		needsComma = true;
	}

	public void writeNull () {
		writeCommaIfNeeded();
		buffer.append("null");
		needsComma = true;
	}

	public void close () {
		buffer.append("\n");
	}

	public String getString () {
		return buffer.toString();
	}

	private void writeCommaIfNeeded () {
		if (needsComma) {
			buffer.append(",");
		}
	}

	private void writeIndent () {
		for (int i = 0; i < depth; i++) {
			buffer.append("  ");
		}
	}

	private String escapeString (String str) {
		return str.replace("\\", "\\\\").replace("\"", "\\\"").replace("\b", "\\b").replace("\f", "\\f").replace("\n", "\\n")
			.replace("\r", "\\r").replace("\t", "\\t");
	}
}
