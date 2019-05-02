/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation {
	public class TimelineType {
		public var ordinal : int;

		public function TimelineType(order : int) {
			this.ordinal = order;
		}

		public static const rotate : TimelineType = new TimelineType(0);
		public static const translate : TimelineType = new TimelineType(1);
		public static const scale : TimelineType = new TimelineType(2);
		public static const shear : TimelineType = new TimelineType(3);
		public static const attachment : TimelineType = new TimelineType(4);
		public static const color : TimelineType = new TimelineType(5);
		public static const deform : TimelineType = new TimelineType(6);
		public static const event : TimelineType = new TimelineType(7);
		public static const drawOrder : TimelineType = new TimelineType(8);
		public static const ikConstraint : TimelineType = new TimelineType(9);
		public static const transformConstraint : TimelineType = new TimelineType(10);
		public static const pathConstraintPosition : TimelineType = new TimelineType(11);
		public static const pathConstraintSpacing : TimelineType = new TimelineType(12);
		public static const pathConstraintMix : TimelineType = new TimelineType(13);
		public static const twoColor : TimelineType = new TimelineType(14);
	}
}
