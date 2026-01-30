using Godot;
using System;

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
	public Texture2D textureHeld;
	[Export]
	public Texture2D texturePickUp;

	[Signal]
	delegate void DroppedEventHandler(Item item);
	[Signal]
	delegate void PickedUpEventHandler(Item item);


	public void Use()
	{
		throw new NotImplementedException();
	}

	public void SecondaryUse()
	{
		throw new NotImplementedException();
	}
	

}
