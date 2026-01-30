using Godot;
using System;

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
			sprite.Texture = ItemData.texturePickUp;
			
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

   	public void ApplyThrowImpulse(float angle, float force)
	{
		Vector2 direction = Vector2.Right.Rotated(angle);
		LinearVelocity = direction * force;
	}

	internal void ApplyDropImpulse(float angle, float v)
	{
		throw new NotImplementedException();
	}
	
	public void OnPickedUp(Item item)
	{
		QueueFree(); 
	}

}
