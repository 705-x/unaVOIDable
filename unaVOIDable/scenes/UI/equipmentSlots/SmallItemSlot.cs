using Godot;
using System;
//!Panel control for displaying small slots.
public partial class SmallItemSlot : Panel
{
	[Export]
	public TextureRect icon;

	private Item itemData;

	public void SetItem(Item _item)
	{
		itemData = _item;
		icon.Texture = _item.Icon;
		icon.Visible = true;
	}
	public void ClearSlot()
	{
		itemData = null;
		icon.Texture = null;
		icon.Visible = false;
	}

    public override Variant _GetDragData(Vector2 atPosition)
    {
        return base._GetDragData(atPosition);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return base._CanDropData(atPosition, data);
    }


}
