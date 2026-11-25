using Godot;

public partial class TintBlack : Node2D {
	public override void _Ready() {
		// The coin's tint-black slot is additive. The middle coin overrides its additive
		// material with a CanvasItemMaterial, which only supports light tint.
		// The other coins use the default tint-black shader, with modulation on the right.
		foreach (var name in new[] { "Original", "SingleColor", "Modulated" }) {
			GetNode<SpineSprite>(name).GetAnimationState().SetAnimation("animation", true, 0);
		}
	}
}
