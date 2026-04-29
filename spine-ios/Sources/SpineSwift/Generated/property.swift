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

import Foundation

/// Property enum
public enum Property: Int32, CaseIterable {
    case rotate = 0
    case x = 1
    case y = 2
    case scaleX = 3
    case scaleY = 4
    case shearX = 5
    case shearY = 6
    case inherit = 7
    case rgb = 8
    case alpha = 9
    case rgb2 = 10
    case attachment = 11
    case deform = 12
    case event = 13
    case drawOrder = 14
    case ikConstraint = 15
    case transformConstraint = 16
    case pathConstraintPosition = 17
    case pathConstraintSpacing = 18
    case pathConstraintMix = 19
    case physicsConstraintInertia = 20
    case physicsConstraintStrength = 21
    case physicsConstraintDamping = 22
    case physicsConstraintMass = 23
    case physicsConstraintWind = 24
    case physicsConstraintGravity = 25
    case physicsConstraintMix = 26
    case physicsConstraintReset = 27
    case sequence = 28
    case sliderTime = 29
    case sliderMix = 30
    case drawOrderFolder = 31

    public static func fromValue(_ value: Int32) -> Property? {
        return Property(rawValue: value)
    }
}