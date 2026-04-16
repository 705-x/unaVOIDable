using Godot;
using System;

public enum EquipmentType
{
	HeadGear,
    Armor,
    Backpack,
    LargeItem,
    SmallItem,
    Consumable
}

//Enum to determine in which slot the item should be equipped. It's used in EquipmentInventory. I don't see an item being un-equippable
//in one of those categories.

[GlobalClass]
public partial class Item : Resource
{
		
	[Export]
	public Vector2 worldSize;
	[Export]
	public Vector2 gridSize;
	[Export]
	public Vector2 holdingPoint; 
	[Export]
	public SpriteFrames useAnimation;
	[Export]
	public SpriteFrames refillAnimation;
	[Export]
	public Texture2D Icon;
	[Export]
	public EquipmentType equipmentType;
	[Export]
	public int uses = 1; //this only gets checked if the item is a consumable, 1 by default
	[Export]
	public int spawnChance; //in promiles


	[Signal]
	delegate void DroppedEventHandler(Item item);
	[Signal]
	delegate void PickedUpEventHandler(Item item);


	public virtual void Use(Player player)
	{
		GD.Print("Basic, not overloaded use");
	}

	public virtual void SecondaryUse(Player player)
	{
		GD.Print("Basic, not overloaded secondary use");
	}
	
	public virtual void Refill(Player player)
	{
		GD.Print("Basic, not overloaded refill (reload for weapons)");
	}

	public virtual void Tick(Player player, double delta)
	{
		return;
	}

}
