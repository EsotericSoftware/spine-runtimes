package com.esotericsoftware.spine.android.bounds;

/**
 * How a view should be aligned within another view.
 */
public enum Alignment {
    TOP_LEFT(-1.0f, -1.0f),
    TOP_CENTER(0.0f, -1.0f),
    TOP_RIGHT(1.0f, -1.0f),
    CENTER_LEFT(-1.0f, 0.0f),
    CENTER(0.0f, 0.0f),
    CENTER_RIGHT(1.0f, 0.0f),
    BOTTOM_LEFT(-1.0f, 1.0f),
    BOTTOM_CENTER(0.0f, 1.0f),
    BOTTOM_RIGHT(1.0f, 1.0f);

    private final float x;
    private final float y;

    Alignment(float x, float y) {
        this.x = x;
        this.y = y;
    }

    public float getX() {
        return x;
    }

    public float getY() {
        return y;
    }
}
