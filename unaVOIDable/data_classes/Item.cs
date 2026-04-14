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
	Vector2 worldSize;
	[Export]
	Vector2 gridSize;
	[Export]
	Vector2 holdingPoint; 
	[Export]
	public SpriteFrames useAnimation;
	[Export]
	public SpriteFrames refillAnimation;
	[Export]
	public Texture2D Icon;
	[Export]
	public EquipmentType equipmentType;
	[Export]
	public int spawnChance; //in promiles


	[Signal]
	delegate void DroppedEventHandler(Item item);
	[Signal]
	delegate void PickedUpEventHandler(Item item);


	public virtual void Use()
	{
		GD.Print("Basic, not overloaded use");
	}

	public virtual void SecondaryUse()
	{
		GD.Print("Basic, not overloaded secondary use");
	}
	
	public virtual void Refill()
	{
		GD.Print("Basic, not overloaded refill (reload for weapons)");
	}

}
