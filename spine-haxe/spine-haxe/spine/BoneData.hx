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

package spine;

/** The setup pose for a bone. */
class BoneData extends PosedData<BonePose> {
	/** The index of the bone in spine.Skeleton.getBones(). */
	public final index:Int;

	public final parent:BoneData = null;

	/** The bone's length. */
	public var length = 0.;

	// Nonessential.

	/** The color of the bone as it was in Spine, or a default color if nonessential data was not exported. Bones are not usually
	 * rendered at runtime. */
	public var color = new Color(0, 0, 0, 0);

	/** The bone icon as it was in Spine, or null if nonessential data was not exported. */
	public var icon:String = null;

	/** The bone icon's display size scale, or 1 if nonessential data was not exported. */
	public var iconSize:Float = 1;

	/** The bone icon's display rotation in degrees, or 0 if nonessential data was not exported. */
	public var iconRotation:Float = 0;

	/** False if the bone was hidden in Spine and nonessential data was exported. Does not affect runtime rendering. */
	public var visible = false;

	public function new(index:Int, name:String, parent:BoneData) {
		super(name, new BonePose());
		if (index < 0)
			throw new SpineException("index must be >= 0.");
		if (name == null)
			throw new SpineException("name cannot be null.");
		this.index = index;
		this.parent = parent;
	}

	/** Copy method. */
	public function copy(data:BoneData, parent:BoneData) {
		var copy = new BoneData(data.index, data.name, parent);
		length = data.length;
		setupPose.set(data.setupPose);
		return copy;
	}
}
