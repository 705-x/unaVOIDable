using Godot;
using System;

//!The physical body of an item when dropped into the world. Takes it's corresponding Resource and uses it as a blueprint to spawn the item in-game-world.

public partial class ItemNode2D : RigidBody2D
{
	[Export]
	public Item ItemData;
	[Export]
	private Sprite2D sprite;

	private CollisionShape2D shape;

	
	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Sprite");
		shape = GetNode<CollisionShape2D>("CollisionShape");
		if (ItemData != null)
		{
			sprite.Texture = ItemData.Icon;
			
			var rect = new RectangleShape2D
			{
				Size = sprite.GetRect().Size
			};
			shape.SetDeferred(CollisionShape2D.PropertyName.Shape, rect);
			this.GravityScale = 0;
		}   
	}

	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		
	}
	
	public void OnPickedUp(Item item)
	{
		//!QueueFrees the item when it is picked up.
		QueueFree(); 
	}

}
