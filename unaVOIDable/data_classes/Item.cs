using Godot;
using System;

public enum EquipmentType
{
	//!Enum to determine in which slot the item should be equipped, and what type it is. It's used in EquipmentInventory.
	HeadGear,
    Armor,
    Backpack,
    LargeItem,
    SmallItem,
    Consumable
}

	
	
	
	
	
	
//! Contains data and usage methods for items. Also see PropNode2D.
	
[GlobalClass]
public partial class Item : Resource
{	
	
	
	[Export]
	public Vector2 worldSize; //!<Size of the item node when instantiated into the world. See WorldObjectManager and ItemNode2D.
	[Export]
	public Vector2 gridSize; //!<Cell dimensions of the item when inside of the player's GridInventory. Unimplemented as of right now.
	[Export]
	public Vector2 holdingPoint; //!<Pixel on which the Player should "grip" the weapon. Unimplemented as of right now.
	[Export]
	public SpriteFrames useAnimation; //!<SpriteFrames of the primary use animation, used inside of the player class. See Player.	
	[Export]
	public SpriteFrames refillAnimation; //!<SpriteFrames of the reload/refill animation, used inside of the player class. See Player.	
	[Export]
	public Texture2D Icon; //!<Icon of the item when in the inventory/Hud. See Hud, EquipmentInventory and InventoryGrid(unimplemented)
	[Export]
	public EquipmentType equipmentType; //!<Type of the Equipment, used for determinating which slot the item should go to and what type it is. See equipmentType.
	[Export]
	public int uses = 1; //!<Number of uses in case the item is a consumable.
	[Export]
	public int spawnChance; //!<Chance of spawning when rolled in promiles. See WorldObjectManager.


	public virtual void Use(Player player)
	{
		//! Virtual function blueprint, supposed to be overloaded in inheriting classes.
		GD.Print("Basic, not overloaded use");
	}

	public virtual void SecondaryUse(Player player)
	{
		//! Virtual function blueprint, supposed to be overloaded in inheriting classes.
		GD.Print("Basic, not overloaded secondary use");
	}
	
	public virtual void Refill(Player player)
	{
		//! Virtual function blueprint, supposed to be overloaded in inheriting classes.
		GD.Print("Basic, not overloaded refill (reload for weapons)");
	}

	public virtual void Tick(Player player, double delta)
	{
		//! Virtual function blueprint, supposed to be overloaded in inheriting classes.
		return;
	}

}
