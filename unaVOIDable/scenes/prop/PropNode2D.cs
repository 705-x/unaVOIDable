using Godot;
using System;

//!The physical body of of a prop in the world. Takes it's corresponding Resource and uses it as a blueprint to spawn the prop.
public partial class PropNode2D : RigidBody2D
{
	public Prop propData;
	[Export]
	private Sprite2D sprite;
	[Export]
	private CollisionPolygon2D shape;
	[Export]
	private LightOccluder2D occluder; //!This class later generates a occluder2D based on the sprite's shape.

	public PropNode2D(){
		
	}
	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Sprite");
		shape = GetNode<CollisionPolygon2D>("CollisionPolygon");
		occluder = GetNode<LightOccluder2D>("LightOccluder");
		
		if (propData != null)
		{
			sprite.Texture = propData.texture;
			var shapePolygon = CreateCollisionPoly(propData.texture).Polygon;  
			shape.Polygon = shapePolygon;
			occluder.Occluder ??= new OccluderPolygon2D();
			occluder.Occluder.Polygon = shapePolygon; 
			this.Mass = propData.Mass;     
		}   
	}
	public override void _Process(double delta)
	{
	}

	public static CollisionPolygon2D CreateCollisionPoly(Texture2D texture)
	{
		//!This Generates the collision polygon based on sprite shape. Uses it also to create the LightOccluder's shape.
		var image = texture.GetImage();
		Bitmap bitmap = new();

		bitmap.CreateFromImageAlpha(image);

		var polygons = bitmap.OpaqueToPolygons(new Rect2I(Vector2I.Zero, bitmap.GetSize()));

		CollisionPolygon2D collisionPoly = new();
    	var poly = polygons[0];

    	Vector2 offset = bitmap.GetSize() / 2;

    	for (int i = 0; i < poly.Length; i++)
        	poly[i] -= offset;

    	collisionPoly.Polygon = poly;
    	return collisionPoly;
	}

	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
	}
}
