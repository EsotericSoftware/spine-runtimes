using Godot;
using System;

public partial class MixAndMatch : SpineSprite
{
	public override void _Ready()
	{
		var data = GetSkeleton().GetData();
		var custom_skin = NewSkin("custom-skin");
		var skin_base = data.FindSkin("skin-base");
		custom_skin.AddSkin(skin_base);
		custom_skin.AddSkin(data.FindSkin("nose/short"));
		custom_skin.AddSkin(data.FindSkin("eyelids/girly"));
		custom_skin.AddSkin(data.FindSkin("eyes/violet"));
		custom_skin.AddSkin(data.FindSkin("hair/brown"));
		custom_skin.AddSkin(data.FindSkin("clothes/hoodie-orange"));
		custom_skin.AddSkin(data.FindSkin("legs/pants-jeans"));
		custom_skin.AddSkin(data.FindSkin("accessories/bag"));
		custom_skin.AddSkin(data.FindSkin("accessories/hat-red-yellow"));
		GetSkeleton().SetSkin(custom_skin);

		foreach (SpineSkinEntry entry in custom_skin.GetAttachments())
		{
			Console.WriteLine(entry.GetSlotIndex() + " " + entry.GetName());
		}

		GetAnimationState().SetAnimation("dance", true, 0);
	}
}
