
package com.esotericsoftware.spine;

public class Event {
	final private EventData data;
	private int intValue;
	private float floatValue;
	private String stringValue;

	public Event (EventData data) {
		this.data = data;
	}

	public int getInt () {
		return intValue;
	}

	public void setInt (int intValue) {
		this.intValue = intValue;
	}

	public float getFloat () {
		return floatValue;
	}

	public void setFloat (float floatValue) {
		this.floatValue = floatValue;
	}

	public String getString () {
		return stringValue;
	}

	public void setString (String stringValue) {
		this.stringValue = stringValue;
	}

	public EventData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
