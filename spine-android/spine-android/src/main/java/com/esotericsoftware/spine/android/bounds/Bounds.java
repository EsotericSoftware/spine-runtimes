package com.esotericsoftware.spine.android.bounds;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.Skeleton;

/**
 * Bounds denoted by the top left corner coordinates {@code x} and {@code y}
 * and the {@code width} and {@code height}.
 */
public class Bounds {
    private double x;
    private double y;
    private double width;
    private double height;

    public Bounds() {
        this.x = 0;
        this.y = 0;
        this.width = 0;
        this.height = 0;
    }

    public Bounds(double x, double y, double width, double height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public Bounds(Skeleton skeleton) {
        Vector2 offset = new Vector2(0, 0);
        Vector2 size = new Vector2(0, 0);
        FloatArray floatArray = new FloatArray();

        skeleton.getBounds(offset, size, floatArray);

        x = offset.x;
        y = offset.y;
        width = size.x;
        height = size.y;
    }

    public double getX() {
        return x;
    }

    public void setX(double x) {
        this.x = x;
    }

    public double getY() {
        return y;
    }

    public void setY(double y) {
        this.y = y;
    }

    public double getWidth() {
        return width;
    }

    public void setWidth(double width) {
        this.width = width;
    }

    public double getHeight() {
        return height;
    }

    public void setHeight(double height) {
        this.height = height;
    }
}
