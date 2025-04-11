/*
The MIT License (MIT)

Copyright (c) 2021-present AgogPixel

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

// Adapted from https://github.com/agogpixel/phaser3-ts-utils/tree/main

let components = (Phaser.GameObjects.Components as any);
export const ComputedSize = components.ComputedSize;
export const Depth = components.Depth;
export const Flip = components.Flip;
export const ScrollFactor = components.ScrollFactor;
export const Transform = components.Transform;
export const Visible = components.Visible;
export const Origin = components.Origin;
export const Alpha = components.Alpha;

export interface Type<
	T,
	P extends any[] = any[]
	> extends Function {
	new(...args: P): T;
}

export type Mixin<GameObjectComponent, GameObjectConstraint extends Phaser.GameObjects.GameObject> = <
	GameObjectType extends Type<GameObjectConstraint>
	>(
	BaseGameObject: GameObjectType
) => GameObjectType & Type<GameObjectComponent>;

export function createMixin<
	GameObjectComponent,
	GameObjectConstraint extends Phaser.GameObjects.GameObject = Phaser.GameObjects.GameObject
> (
	...component: GameObjectComponent[]
): Mixin<GameObjectComponent, GameObjectConstraint> {
	return (BaseGameObject) => {
		(Phaser as any).Class.mixin(BaseGameObject, component);
		return BaseGameObject as any;
	};
}

type ComputedSizeMixin = Mixin<Phaser.GameObjects.Components.ComputedSize, Phaser.GameObjects.GameObject>;
export const ComputedSizeMixin: ComputedSizeMixin = createMixin<Phaser.GameObjects.Components.ComputedSize>(ComputedSize);

type DepthMixin = Mixin<Phaser.GameObjects.Components.Depth, Phaser.GameObjects.GameObject>;
export const DepthMixin: DepthMixin = createMixin<Phaser.GameObjects.Components.Depth>(Depth);

type FlipMixin = Mixin<Phaser.GameObjects.Components.Flip, Phaser.GameObjects.GameObject>;
export const FlipMixin: FlipMixin = createMixin<Phaser.GameObjects.Components.Flip>(Flip);

type ScrollFactorMixin = Mixin<Phaser.GameObjects.Components.ScrollFactor, Phaser.GameObjects.GameObject>;
export const ScrollFactorMixin: ScrollFactorMixin = createMixin<Phaser.GameObjects.Components.ScrollFactor>(ScrollFactor);

type TransformMixin = Mixin<Phaser.GameObjects.Components.Transform, Phaser.GameObjects.GameObject>;
export const TransformMixin: TransformMixin = createMixin<Phaser.GameObjects.Components.Transform>(Transform);

type VisibleMixin = Mixin<Phaser.GameObjects.Components.Visible, Phaser.GameObjects.GameObject>;
export const VisibleMixin: VisibleMixin = createMixin<Phaser.GameObjects.Components.Visible>(Visible);

type OriginMixin = Mixin<Phaser.GameObjects.Components.Origin, Phaser.GameObjects.GameObject>;
export const OriginMixin: OriginMixin = createMixin<Phaser.GameObjects.Components.Origin>(Origin);

type AlphaMixin = Mixin<Phaser.GameObjects.Components.Alpha, Phaser.GameObjects.GameObject>;
export const AlphaMixin: AlphaMixin = createMixin<Phaser.GameObjects.Components.Alpha>(Alpha);