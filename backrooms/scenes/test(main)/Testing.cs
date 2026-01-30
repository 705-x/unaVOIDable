using Godot;
using System;
using System.Numerics;

public partial class Testing : Node
{
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	var packedScene = GD.Load<PackedScene>("res://scenes/groundItem/item_node_2d.tscn");
	Item item = GD.Load<Item>("res://scenes/test(main)/TestItem.tres");
	var worldItemNode = packedScene.Instantiate();
	var worldItem = worldItemNode as ItemNode2D;
	if (worldItem == null)
	{
		GD.PrintErr("cannot cast root to RigidBody2D!");
		return;
	}
	worldItem.ItemData = item.Duplicate() as Item;
	worldItem.GlobalPosition = new (100, 100);
	GetTree().CurrentScene.AddChild(worldItem);
	
	packedScene = GD.Load<PackedScene>("res://scenes/prop/prop_node_2d.tscn");
	Prop prop = GD.Load<Prop>("res://scenes/test(main)/Crate.tres");
	var worldPropNode = packedScene.Instantiate();
	var worldProp = worldPropNode as PropNode2D;
	worldProp.propData = prop.Duplicate() as Prop;
	worldProp.GlobalPosition = new(200,200);
	GetTree().CurrentScene.AddChild(worldProp);
	
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
