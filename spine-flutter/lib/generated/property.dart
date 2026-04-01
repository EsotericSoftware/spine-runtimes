//
// Spine Runtimes License Agreement
// Last updated April 5, 2025. Replaces all prior versions.
//
// Copyright (c) 2013-2025, Esoteric Software LLC
//
// Integration of the Spine Runtimes into software or otherwise creating
// derivative works of the Spine Runtimes is permitted under the terms and
// conditions of Section 2 of the Spine Editor License Agreement:
// http://esotericsoftware.com/spine-editor-license
//
// Otherwise, it is permitted to integrate the Spine Runtimes into software
// or otherwise create derivative works of the Spine Runtimes (collectively,
// "Products"), provided that each user of the Products must obtain their own
// Spine Editor license and redistribution of the Products in any form must
// include this license and copyright notice.
//
// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

// AUTO GENERATED FILE, DO NOT EDIT.

/// Property enum
enum Property {
  rotate(0),
  x(1),
  y(2),
  scaleX(3),
  scaleY(4),
  shearX(5),
  shearY(6),
  inherit(7),
  rgb(8),
  alpha(9),
  rgb2(10),
  attachment(11),
  deform(12),
  event(13),
  drawOrder(14),
  ikConstraint(15),
  transformConstraint(16),
  pathConstraintPosition(17),
  pathConstraintSpacing(18),
  pathConstraintMix(19),
  physicsConstraintInertia(20),
  physicsConstraintStrength(21),
  physicsConstraintDamping(22),
  physicsConstraintMass(23),
  physicsConstraintWind(24),
  physicsConstraintGravity(25),
  physicsConstraintMix(26),
  physicsConstraintReset(27),
  sequence(28),
  sliderTime(29),
  sliderMix(30),
  drawOrderFolder(31);

  const Property(this.value);
  final int value;

  static Property fromValue(int value) {
    return values.firstWhere(
      (e) => e.value == value,
      orElse: () => throw ArgumentError('Invalid Property value: $value'),
    );
  }
}
