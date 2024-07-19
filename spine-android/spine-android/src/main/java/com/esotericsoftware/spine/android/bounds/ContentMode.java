package com.esotericsoftware.spine.android.bounds;

/**
 * How a view should be inscribed into another view.
 */
public enum ContentMode {
    /**
     * As large as possible while still containing the source view entirely within the target view.
     */
    FIT,
    /**
     * Fill the target view by distorting the source's aspect ratio.
     */
    FILL
}
